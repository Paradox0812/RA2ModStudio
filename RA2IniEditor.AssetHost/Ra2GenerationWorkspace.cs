using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RA2IniEditor.AssetHost;

internal sealed class Ra2GenerationWorkspace
{
    private const string RootMarkerName = ".ra2-asset-host-root";
    private const string RootMarkerContent = "ra2-asset-host-root/1\n";
    private const string RunMarkerName = ".ra2-run.json";
    private const string ActiveLockName = ".active.lock";
    private static readonly StringComparison PathComparison = OperatingSystem.IsWindows()
        ? StringComparison.OrdinalIgnoreCase
        : StringComparison.Ordinal;

    private readonly Ra2GenerationProviderConfiguration _configuration;
    private readonly Ra2GenerationRequest _request;
    private readonly DateTimeOffset _createdUtc;
    private FileStream? _activeLock;
    private bool _leaseTransferred;

    private Ra2GenerationWorkspace(
        Ra2GenerationProviderConfiguration configuration,
        Ra2GenerationRequest request,
        string workspaceRoot,
        string runRoot,
        FileStream activeLock,
        DateTimeOffset createdUtc)
    {
        _configuration = configuration;
        _request = request;
        WorkspaceRoot = workspaceRoot;
        RunRoot = runRoot;
        _createdUtc = createdUtc;
        StagingRoot = Path.Combine(runRoot, "staging");
        InputRoot = Path.Combine(StagingRoot, "inputs");
        ProviderOutputRoot = Path.Combine(StagingRoot, "provider-output");
        _activeLock = activeLock;
    }

    internal string WorkspaceRoot { get; }
    internal string RunRoot { get; }
    internal string StagingRoot { get; }
    internal string InputRoot { get; }
    internal string ProviderOutputRoot { get; }

    internal static long MeasureRunBytes(string runRoot)
    {
        try
        {
            return Directory.Exists(runRoot) ? ComputeDirectoryBytes(runRoot) : 0;
        }
        catch (Exception exception) when (exception is FileNotFoundException or DirectoryNotFoundException)
        {
            // A provider may atomically replace a file while the watchdog is enumerating.
            // The next bounded sample will observe the stable tree; this is not itself a quota breach.
            return 0;
        }
    }

    internal static async ValueTask<Ra2WorkspacePreparationResult> PrepareAsync(
        Ra2GenerationProviderConfiguration configuration,
        Ra2GenerationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            string workspaceRoot = NormalizeDirectory(configuration.WorkspaceRoot);
            if (IsReparsePointIfPresent(workspaceRoot) ||
                IsSameOrDescendant(Path.GetFullPath(configuration.ExecutablePath), workspaceRoot))
            {
                return Ra2WorkspacePreparationResult.Failure(
                    Ra2GenerationFailureKind.WorkspaceRejected,
                    "The generation workspace overlaps a protected location.");
            }

            foreach (string forbidden in configuration.ForbiddenRoots)
            {
                if (string.IsNullOrWhiteSpace(forbidden) || !Path.IsPathFullyQualified(forbidden))
                {
                    return Ra2WorkspacePreparationResult.Failure(
                        Ra2GenerationFailureKind.WorkspaceRejected,
                        "A forbidden workspace boundary is invalid.");
                }

                string forbiddenRoot = NormalizeDirectory(forbidden);
                if (IsSameOrDescendant(workspaceRoot, forbiddenRoot))
                {
                    return Ra2WorkspacePreparationResult.Failure(
                        Ra2GenerationFailureKind.WorkspaceRejected,
                        "The generation workspace overlaps a protected location.");
                }
            }

            Ra2GenerationFailureKind rootFailure = await PrepareRootAndSweepAsync(
                workspaceRoot,
                configuration,
                cancellationToken).ConfigureAwait(false);
            if (rootFailure != Ra2GenerationFailureKind.None)
            {
                return Ra2WorkspacePreparationResult.Failure(rootFailure, RootFailureMessage(rootFailure));
            }

            string runRoot = Path.Combine(workspaceRoot, request.RunId.ToString("D"));
            if (Directory.Exists(runRoot))
            {
                return Ra2WorkspacePreparationResult.Failure(
                    Ra2GenerationFailureKind.WorkspaceRejected,
                    "The generation run identity already exists.");
            }

            Directory.CreateDirectory(runRoot);
            string markerPath = Path.Combine(runRoot, RunMarkerName);
            string lockPath = Path.Combine(runRoot, ActiveLockName);
            FileStream? activeLock = null;
            try
            {
                activeLock = new FileStream(lockPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.Asynchronous);
                DateTimeOffset createdUtc = DateTimeOffset.UtcNow;
                await WriteRunMarkerAsync(markerPath, request.RunId, "Staging", cancellationToken).ConfigureAwait(false);
                var workspace = new Ra2GenerationWorkspace(configuration, request, workspaceRoot, runRoot, activeLock, createdUtc);
                Directory.CreateDirectory(workspace.InputRoot);
                Directory.CreateDirectory(workspace.ProviderOutputRoot);
                await workspace.StageRequestAsync(cancellationToken).ConfigureAwait(false);
                activeLock = null;
                return Ra2WorkspacePreparationResult.Success(workspace);
            }
            finally
            {
                if (activeLock is not null)
                {
                    await activeLock.DisposeAsync().ConfigureAwait(false);
                    try
                    {
                        Directory.Delete(runRoot, recursive: true);
                    }
                    catch
                    {
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            return Ra2WorkspacePreparationResult.Failure(Ra2GenerationFailureKind.Canceled, "Workspace preparation was canceled.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException)
        {
            return Ra2WorkspacePreparationResult.Failure(
                Ra2GenerationFailureKind.WorkspaceRejected,
                "The generation workspace could not be prepared safely.");
        }
    }

    internal async ValueTask<Ra2WorkspacePromotionResult> ValidateAndPromoteAsync(
        Ra2GenerationProviderDescriptor descriptor,
        IReadOnlyList<Ra2GenerationCandidateDeclaration> declarations,
        IReadOnlyList<string> completedCandidateIds,
        CancellationToken cancellationToken)
    {
        string pendingRoot = Path.Combine(RunRoot, "completed.pending");
        string completedRoot = Path.Combine(RunRoot, "completed");
        string artifactRoot = Path.Combine(pendingRoot, "artifacts");
        try
        {
            if (declarations.Count != _request.CandidateCount ||
                declarations.Count != completedCandidateIds.Count ||
                !completedCandidateIds.SequenceEqual(declarations.Select(candidate => candidate.CandidateId), StringComparer.Ordinal))
            {
                return Ra2WorkspacePromotionResult.Failure(
                    Ra2GenerationFailureKind.OutputMissing,
                    "The provider candidate set did not match the request.");
            }

            Directory.CreateDirectory(artifactRoot);
            long aggregateBytes = 0;
            var candidates = new List<Ra2GenerationCandidate>(declarations.Count);
            var artifactPaths = new Dictionary<(string CandidateId, string ArtifactId), string>();
            foreach (Ra2GenerationCandidateDeclaration candidate in declarations)
            {
                var admitted = new List<Ra2GenerationArtifact>();
                foreach (Ra2GenerationArtifactDeclaration artifact in candidate.Artifacts)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!TryResolveProviderOutput(artifact.RelativePath, out string sourcePath) ||
                        !File.Exists(sourcePath) || IsReparsePointIfPresent(sourcePath) ||
                        !EnsureNoReparsePoints(ProviderOutputRoot, sourcePath))
                    {
                        return Ra2WorkspacePromotionResult.Failure(
                            Ra2GenerationFailureKind.OutputRejected,
                            "A provider artifact escaped or violated the workspace boundary.");
                    }

                    var info = new FileInfo(sourcePath);
                    if (info.Length != artifact.Length || info.Length <= 0 || info.Length > Ra2GenerationLimits.MaximumArtifactBytes)
                    {
                        return Ra2WorkspacePromotionResult.Failure(
                            Ra2GenerationFailureKind.ResourceLimitExceeded,
                            "A provider artifact exceeded its declared or allowed size.");
                    }

                    aggregateBytes += info.Length;
                    if (aggregateBytes > Ra2GenerationLimits.MaximumRunBytes ||
                        aggregateBytes > descriptor.MaximumOutputBytes)
                    {
                        return Ra2WorkspacePromotionResult.Failure(
                            Ra2GenerationFailureKind.ResourceLimitExceeded,
                            "Provider artifacts exceeded the aggregate output limit.");
                    }

                    if (!await ValidateMagicAsync(sourcePath, artifact.Kind, cancellationToken).ConfigureAwait(false))
                    {
                        return Ra2WorkspacePromotionResult.Failure(
                            Ra2GenerationFailureKind.OutputRejected,
                            "A provider artifact did not match its declared format.");
                    }

                    string hash = await Ra2GenerationValidation.ComputeFileSha256Async(sourcePath, cancellationToken).ConfigureAwait(false);
                    if (!string.Equals(hash, artifact.Sha256, StringComparison.Ordinal))
                    {
                        return Ra2WorkspacePromotionResult.Failure(
                            Ra2GenerationFailureKind.OutputRejected,
                            "A provider artifact hash did not match its declaration.");
                    }

                    string destinationName = hash + ExtensionFor(artifact.Kind);
                    string pendingPath = Path.Combine(artifactRoot, destinationName);
                    if (!File.Exists(pendingPath))
                    {
                        File.Copy(sourcePath, pendingPath, overwrite: false);
                    }

                    admitted.Add(new Ra2GenerationArtifact(artifact.ArtifactId, artifact.Kind, artifact.Length, hash));
                    artifactPaths.Add((candidate.CandidateId, artifact.ArtifactId), Path.Combine("artifacts", destinationName));
                }

                candidates.Add(new Ra2GenerationCandidate(candidate.CandidateId, admitted));
            }

            await WriteResultManifestAsync(
                pendingRoot,
                descriptor,
                candidates,
                artifactPaths,
                cancellationToken).ConfigureAwait(false);
            Directory.Move(pendingRoot, completedRoot);
            try
            {
                Directory.Delete(StagingRoot, recursive: true);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return Ra2WorkspacePromotionResult.Failure(
                    Ra2GenerationFailureKind.CleanupFailed,
                    "The provider staging area could not be cleaned after validation.");
            }

            await WriteRunMarkerAsync(
                Path.Combine(RunRoot, RunMarkerName),
                _request.RunId,
                "Completed",
                cancellationToken).ConfigureAwait(false);
            var lease = new Ra2GenerationWorkspaceLease(
                RunRoot,
                completedRoot,
                candidates,
                artifactPaths,
                _activeLock!);
            _activeLock = null;
            _leaseTransferred = true;
            return Ra2WorkspacePromotionResult.Success(lease);
        }
        catch (OperationCanceledException)
        {
            return Ra2WorkspacePromotionResult.Failure(Ra2GenerationFailureKind.Canceled, "Artifact validation was canceled.");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or CryptographicException)
        {
            return Ra2WorkspacePromotionResult.Failure(
                Ra2GenerationFailureKind.OutputRejected,
                "Provider artifacts could not be validated safely.");
        }
        finally
        {
            if (Directory.Exists(pendingRoot))
            {
                try
                {
                    Directory.Delete(pendingRoot, recursive: true);
                }
                catch
                {
                    // The enclosing failed-run cleanup owns the remaining quarantine attempt.
                }
            }
        }
    }

    internal async ValueTask<bool> CleanupFailedRunAsync()
    {
        if (_leaseTransferred)
        {
            return true;
        }

        if (_activeLock is not null)
        {
            await _activeLock.DisposeAsync().ConfigureAwait(false);
            _activeLock = null;
        }

        try
        {
            if (Directory.Exists(RunRoot))
            {
                Directory.Delete(RunRoot, recursive: true);
            }

            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            try
            {
                await WriteRunMarkerAsync(
                    Path.Combine(RunRoot, RunMarkerName),
                    _request.RunId,
                    "Quarantined",
                    CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
            }

            return false;
        }
    }

    private async ValueTask StageRequestAsync(CancellationToken cancellationToken)
    {
        var references = new List<object>(_request.References.Count);
        foreach (Ra2GenerationReferenceImage reference in _request.References)
        {
            string extension = reference.MediaKind switch
            {
                Ra2GenerationMediaKind.Png => ".png",
                Ra2GenerationMediaKind.Jpeg => ".jpg",
                Ra2GenerationMediaKind.Webp => ".webp",
                _ => throw new InvalidOperationException("Unsupported reference media kind.")
            };
            string relativePath = Path.Combine("inputs", reference.Sha256 + extension).Replace('\\', '/');
            string destination = Path.Combine(StagingRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
            await File.WriteAllBytesAsync(destination, reference.Content.ToArray(), cancellationToken).ConfigureAwait(false);
            references.Add(new
            {
                reference.Name,
                mediaKind = reference.MediaKind.ToString(),
                reference.Length,
                reference.Sha256,
                path = relativePath
            });
        }

        byte[] requestJson = JsonSerializer.SerializeToUtf8Bytes(new
        {
            schema = "ra2-generation-request/1",
            runId = _request.RunId,
            _request.Prompt,
            _request.NegativeConstraints,
            references,
            _request.Seed,
            _request.CandidateCount,
            _request.IncludePreviewPng,
            _request.ExpectedProviderId,
            _request.ExpectedModelRevision,
            timeoutMilliseconds = checked((long)_request.Timeout.TotalMilliseconds),
            fingerprint = _request.Fingerprint,
            outputDirectory = "provider-output"
        });
        await File.WriteAllBytesAsync(Path.Combine(StagingRoot, "request.json"), requestJson, cancellationToken).ConfigureAwait(false);
    }

    private bool TryResolveProviderOutput(string relativePath, out string fullPath)
    {
        fullPath = string.Empty;
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath) ||
            relativePath.Contains(':', StringComparison.Ordinal) || relativePath.Contains('\0'))
        {
            return false;
        }

        string normalizedRelative = relativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        fullPath = Path.GetFullPath(Path.Combine(ProviderOutputRoot, normalizedRelative));
        return IsSameOrDescendant(fullPath, ProviderOutputRoot) &&
            !string.Equals(fullPath, NormalizeDirectory(ProviderOutputRoot), PathComparison);
    }

    private static async ValueTask<Ra2GenerationFailureKind> PrepareRootAndSweepAsync(
        string workspaceRoot,
        Ra2GenerationProviderConfiguration configuration,
        CancellationToken cancellationToken)
    {
        bool existed = Directory.Exists(workspaceRoot);
        if (!existed)
        {
            Directory.CreateDirectory(workspaceRoot);
        }

        if (IsReparsePointIfPresent(workspaceRoot))
        {
            return Ra2GenerationFailureKind.WorkspaceRejected;
        }

        string rootMarker = Path.Combine(workspaceRoot, RootMarkerName);
        if (!File.Exists(rootMarker))
        {
            if (Directory.EnumerateFileSystemEntries(workspaceRoot).Any())
            {
                return Ra2GenerationFailureKind.WorkspaceRejected;
            }

            await File.WriteAllTextAsync(rootMarker, RootMarkerContent, new UTF8Encoding(false), cancellationToken).ConfigureAwait(false);
        }
        else if (!string.Equals(await File.ReadAllTextAsync(rootMarker, cancellationToken).ConfigureAwait(false), RootMarkerContent, StringComparison.Ordinal) ||
                 IsReparsePointIfPresent(rootMarker))
        {
            return Ra2GenerationFailureKind.WorkspaceRejected;
        }

        foreach (string entry in Directory.EnumerateFileSystemEntries(workspaceRoot))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.Equals(Path.GetFileName(entry), RootMarkerName, StringComparison.Ordinal))
            {
                continue;
            }

            if (!Directory.Exists(entry) || !Guid.TryParseExact(Path.GetFileName(entry), "D", out Guid runId) ||
                !TryReadRunMarker(entry, runId, out DateTimeOffset lastActivityUtc) ||
                !EnsureNoReparsePoints(entry, entry))
            {
                return Ra2GenerationFailureKind.WorkspaceRejected;
            }

            string lockPath = Path.Combine(entry, ActiveLockName);
            if (!File.Exists(lockPath) || IsReparsePointIfPresent(lockPath))
            {
                return Ra2GenerationFailureKind.WorkspaceRejected;
            }

            FileStream? orphanLock = null;
            try
            {
                orphanLock = new FileStream(lockPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                continue;
            }

            await using (orphanLock.ConfigureAwait(false))
            {
                if (DateTimeOffset.UtcNow - lastActivityUtc >= configuration.OrphanTtl)
                {
                    orphanLock.Close();
                    Directory.Delete(entry, recursive: true);
                }
            }
        }

        long rootBytes = ComputeDirectoryBytes(workspaceRoot);
        return rootBytes > configuration.MaximumWorkspaceRootBytes
            ? Ra2GenerationFailureKind.ResourceLimitExceeded
            : Ra2GenerationFailureKind.None;
    }

    private static bool TryReadRunMarker(string runRoot, Guid expectedRunId, out DateTimeOffset lastActivityUtc)
    {
        lastActivityUtc = default;
        string markerPath = Path.Combine(runRoot, RunMarkerName);
        if (!File.Exists(markerPath) || IsReparsePointIfPresent(markerPath))
        {
            return false;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(markerPath), new JsonDocumentOptions { MaxDepth = 8 });
            JsonElement root = document.RootElement;
            return root.TryGetProperty("protocol", out JsonElement protocol) &&
                string.Equals(protocol.GetString(), Ra2GenerationLimits.ProtocolIdentity, StringComparison.Ordinal) &&
                root.TryGetProperty("runId", out JsonElement runId) && runId.TryGetGuid(out Guid parsedRunId) &&
                parsedRunId == expectedRunId &&
                root.TryGetProperty("lastActivityUtc", out JsonElement lastActivity) &&
                lastActivity.TryGetDateTimeOffset(out lastActivityUtc);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static async ValueTask WriteRunMarkerAsync(
        string markerPath,
        Guid runId,
        string state,
        CancellationToken cancellationToken)
    {
        byte[] content = JsonSerializer.SerializeToUtf8Bytes(new
        {
            protocol = Ra2GenerationLimits.ProtocolIdentity,
            runId,
            state,
            lastActivityUtc = DateTimeOffset.UtcNow
        });
        string temporary = markerPath + ".tmp";
        await File.WriteAllBytesAsync(temporary, content, cancellationToken).ConfigureAwait(false);
        File.Move(temporary, markerPath, overwrite: true);
    }

    private async ValueTask WriteResultManifestAsync(
        string pendingRoot,
        Ra2GenerationProviderDescriptor descriptor,
        IReadOnlyList<Ra2GenerationCandidate> candidates,
        IReadOnlyDictionary<(string CandidateId, string ArtifactId), string> artifactPaths,
        CancellationToken cancellationToken)
    {
        DateTimeOffset completedUtc = DateTimeOffset.UtcNow;
        byte[] result = JsonSerializer.SerializeToUtf8Bytes(new
        {
            schema = "ra2-generation-result/1",
            protocol = Ra2GenerationLimits.ProtocolIdentity,
            descriptor.ProviderId,
            descriptor.ProviderVersion,
            descriptor.ModelId,
            descriptor.ModelRevision,
            descriptor.ExecutableSha256,
            requestFingerprint = _request.Fingerprint,
            _request.Seed,
            inputHashes = _request.References.Select(reference => reference.Sha256).ToArray(),
            descriptor.SeedBehavior,
            descriptor.LicenseId,
            licenseAccepted = _configuration.LicenseAccepted,
            startedUtc = _createdUtc,
            completedUtc,
            durationMilliseconds = checked((long)(completedUtc - _createdUtc).TotalMilliseconds),
            terminalState = Ra2GenerationState.CandidateReady.ToString(),
            failureKind = Ra2GenerationFailureKind.None.ToString(),
            candidates = candidates.Select(candidate => new
            {
                candidate.CandidateId,
                artifacts = candidate.Artifacts.Select(artifact => new
                {
                    artifact.ArtifactId,
                    kind = artifact.Kind.ToString(),
                    artifact.Length,
                    artifact.Sha256,
                    relativeArtifact = artifactPaths[(candidate.CandidateId, artifact.ArtifactId)].Replace('\\', '/')
                })
            })
        });
        await File.WriteAllBytesAsync(Path.Combine(pendingRoot, "result.json"), result, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask<bool> ValidateMagicAsync(
        string path,
        Ra2GenerationArtifactKind kind,
        CancellationToken cancellationToken)
    {
        await using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
        if (kind == Ra2GenerationArtifactKind.MeshGlb)
        {
            byte[] header = new byte[12];
            if (await stream.ReadAsync(header, cancellationToken).ConfigureAwait(false) != header.Length ||
                !header.AsSpan(0, 4).SequenceEqual("glTF"u8) ||
                BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(4, 4)) != 2 ||
                BinaryPrimitives.ReadUInt32LittleEndian(header.AsSpan(8, 4)) != stream.Length ||
                stream.Length < 20)
            {
                return false;
            }

            return true;
        }

        if (kind == Ra2GenerationArtifactKind.PreviewPng)
        {
            byte[] signature = new byte[8];
            return await stream.ReadAsync(signature, cancellationToken).ConfigureAwait(false) == signature.Length &&
                signature.AsSpan().SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        }

        try
        {
            using JsonDocument _ = await JsonDocument.ParseAsync(stream, new JsonDocumentOptions { MaxDepth = 32 }, cancellationToken)
                .ConfigureAwait(false);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string ExtensionFor(Ra2GenerationArtifactKind kind) => kind switch
    {
        Ra2GenerationArtifactKind.MeshGlb => ".glb",
        Ra2GenerationArtifactKind.PreviewPng => ".png",
        Ra2GenerationArtifactKind.ProviderJson => ".json",
        _ => throw new InvalidOperationException("Unsupported artifact kind.")
    };

    private static string NormalizeDirectory(string path) =>
        Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

    private static bool IsSameOrDescendant(string candidate, string root)
    {
        string normalizedCandidate = Path.GetFullPath(candidate);
        string normalizedRoot = NormalizeDirectory(root);
        return string.Equals(normalizedCandidate, normalizedRoot, PathComparison) ||
            normalizedCandidate.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, PathComparison);
    }

    private static bool IsReparsePointIfPresent(string path)
    {
        if (!File.Exists(path) && !Directory.Exists(path))
        {
            return false;
        }

        return (File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0;
    }

    private static bool EnsureNoReparsePoints(string root, string target)
    {
        string normalizedRoot = NormalizeDirectory(root);
        string normalizedTarget = Path.GetFullPath(target);
        if (!IsSameOrDescendant(normalizedTarget, normalizedRoot))
        {
            return false;
        }

        if (IsReparsePointIfPresent(normalizedRoot))
        {
            return false;
        }

        string relative = Path.GetRelativePath(normalizedRoot, normalizedTarget);
        string current = normalizedRoot;
        foreach (string segment in relative.Split(
                     new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                     StringSplitOptions.RemoveEmptyEntries))
        {
            current = Path.Combine(current, segment);
            if (IsReparsePointIfPresent(current))
            {
                return false;
            }
        }

        if (Directory.Exists(normalizedTarget))
        {
            foreach (string entry in Directory.EnumerateFileSystemEntries(normalizedTarget, "*", SearchOption.AllDirectories))
            {
                if (IsReparsePointIfPresent(entry))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static long ComputeDirectoryBytes(string root)
    {
        long total = 0;
        foreach (string file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories))
        {
            if (IsReparsePointIfPresent(file))
            {
                throw new IOException("Reparse points are not allowed in the workspace.");
            }

            total = checked(total + new FileInfo(file).Length);
        }

        return total;
    }

    private static string RootFailureMessage(Ra2GenerationFailureKind failureKind) => failureKind switch
    {
        Ra2GenerationFailureKind.ResourceLimitExceeded => "The generation workspace root exceeds its configured budget.",
        _ => "The generation workspace root is not exclusively owned by AssetHost."
    };
}

internal sealed class Ra2WorkspacePreparationResult
{
    private Ra2WorkspacePreparationResult(
        Ra2GenerationWorkspace? workspace,
        Ra2GenerationFailureKind failureKind,
        string message)
    {
        Workspace = workspace;
        FailureKind = failureKind;
        Message = message;
    }

    internal Ra2GenerationWorkspace? Workspace { get; }
    internal Ra2GenerationFailureKind FailureKind { get; }
    internal string Message { get; }
    internal bool Succeeded => Workspace is not null;

    internal static Ra2WorkspacePreparationResult Success(Ra2GenerationWorkspace workspace) =>
        new(workspace, Ra2GenerationFailureKind.None, string.Empty);

    internal static Ra2WorkspacePreparationResult Failure(Ra2GenerationFailureKind kind, string message) =>
        new(null, kind, message);
}

internal sealed class Ra2WorkspacePromotionResult
{
    private Ra2WorkspacePromotionResult(
        IRa2GenerationWorkspaceLease? lease,
        Ra2GenerationFailureKind failureKind,
        string message)
    {
        Lease = lease;
        FailureKind = failureKind;
        Message = message;
    }

    internal IRa2GenerationWorkspaceLease? Lease { get; }
    internal Ra2GenerationFailureKind FailureKind { get; }
    internal string Message { get; }
    internal bool Succeeded => Lease is not null;

    internal static Ra2WorkspacePromotionResult Success(IRa2GenerationWorkspaceLease lease) =>
        new(lease, Ra2GenerationFailureKind.None, string.Empty);

    internal static Ra2WorkspacePromotionResult Failure(Ra2GenerationFailureKind kind, string message) =>
        new(null, kind, message);
}
