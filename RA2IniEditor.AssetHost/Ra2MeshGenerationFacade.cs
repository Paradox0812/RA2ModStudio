using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text.Json;

namespace RA2IniEditor.AssetHost;

public enum Ra2ReferenceImageFormat
{
    Png = 0,
    Jpeg
}

public enum Ra2MeshGenerationFailureKind
{
    None = 0,
    InvalidRequest,
    BundleMissing,
    BundleRejected,
    ProviderNotConfigured,
    ProviderNotReady,
    CapabilityUnavailable,
    ConsentDeclined,
    RemoteGenerationFailed,
    ArtifactMissing,
    ArtifactRejected,
    ArtifactTooLarge,
    ResourceLimitExceeded,
    TimedOut,
    Canceled,
    CleanupFailed,
    UnexpectedFailure
}

public sealed class Ra2MeshGenerationRequest
{
    private readonly byte[] _referenceImage;

    public Ra2MeshGenerationRequest(
        string referenceName,
        Ra2ReferenceImageFormat referenceFormat,
        ReadOnlySpan<byte> referenceImage,
        string designBrief,
        string negativeConstraints,
        TimeSpan timeout,
        int seed = 1)
    {
        ReferenceName = referenceName ?? string.Empty;
        ReferenceFormat = referenceFormat;
        _referenceImage = referenceImage.ToArray();
        DesignBrief = designBrief ?? string.Empty;
        NegativeConstraints = negativeConstraints ?? string.Empty;
        Timeout = timeout;
        Seed = seed;
    }

    public string ReferenceName { get; }
    public Ra2ReferenceImageFormat ReferenceFormat { get; }
    public ReadOnlyMemory<byte> ReferenceImage => _referenceImage;
    public string DesignBrief { get; }
    public string NegativeConstraints { get; }
    public TimeSpan Timeout { get; }
    public int Seed { get; }
}

public readonly record struct Ra2MeshGenerationProgress(
    long Sequence,
    string Phase,
    double? Percent,
    string Message);

public sealed class Ra2MeshGenerationResult
{
    private readonly byte[]? _meshGlb;
    private readonly byte[]? _previewPng;

    private Ra2MeshGenerationResult(
        string state,
        Ra2MeshGenerationFailureKind failureKind,
        string message,
        string providerId,
        string modelId,
        byte[]? meshGlb,
        byte[]? previewPng)
    {
        State = state;
        FailureKind = failureKind;
        Message = message;
        ProviderId = providerId;
        ModelId = modelId;
        _meshGlb = meshGlb;
        _previewPng = previewPng;
    }

    public bool Succeeded => FailureKind == Ra2MeshGenerationFailureKind.None;
    public bool IsReady => Succeeded && string.Equals(State, "Ready", StringComparison.Ordinal);
    public bool HasArtifact => _meshGlb is { Length: > 0 };
    public string State { get; }
    public Ra2MeshGenerationFailureKind FailureKind { get; }
    public string Message { get; }
    public string ProviderId { get; }
    public string ModelId { get; }
    public ReadOnlyMemory<byte> MeshGlb => _meshGlb ?? ReadOnlyMemory<byte>.Empty;
    public ReadOnlyMemory<byte> PreviewPng => _previewPng ?? ReadOnlyMemory<byte>.Empty;

    internal static Ra2MeshGenerationResult Ready(string providerId, string modelId) =>
        new("Ready", Ra2MeshGenerationFailureKind.None, string.Empty, providerId, modelId, null, null);

    internal static Ra2MeshGenerationResult Candidate(string providerId, string modelId, byte[] glb, byte[]? preview) =>
        new("CandidateReady", Ra2MeshGenerationFailureKind.None, string.Empty, providerId, modelId, glb, preview);

    internal static Ra2MeshGenerationResult Failure(string state, Ra2MeshGenerationFailureKind kind, string message) =>
        new(state, kind, message, string.Empty, string.Empty, null, null);
}

/// <summary>
/// Public experimental boundary used by the IDE. Host leases and protocol DTOs never cross it.
/// </summary>
public sealed class Ra2MeshGenerationFacade
{
    private const string ManifestSchema = "ra2-asset-provider-bundle/1";
    private const string ProviderId = "tencent-hy3d-openai-compatible";
    private const string ProviderVersion = "1.0.0";
    private const string ModelId = "hunyuan-3d-professional";
    private const string ModelRevision = "3.1-geometry";
    private const int MaximumReferenceBytes = 6 * 1024 * 1024;
    private const int MaximumOwnedGlbBytes = 16 * 1024 * 1024;
    private const int MaximumOwnedPreviewBytes = 4 * 1024 * 1024;

    private readonly string _bundleManifestPath;
    private readonly string _workspaceRoot;
    private readonly ReadOnlyCollection<string> _forbiddenRoots;
    private readonly bool _licenseAccepted;

    private Ra2MeshGenerationFacade(
        string bundleManifestPath,
        string workspaceRoot,
        IReadOnlyList<string> forbiddenRoots,
        bool licenseAccepted)
    {
        _bundleManifestPath = Path.GetFullPath(bundleManifestPath);
        _workspaceRoot = Path.GetFullPath(workspaceRoot);
        _forbiddenRoots = Array.AsReadOnly(forbiddenRoots.Select(Path.GetFullPath).ToArray());
        _licenseAccepted = licenseAccepted;
    }

    public static Ra2MeshGenerationFacade CreateFromBundle(
        string bundleManifestPath,
        string workspaceRoot,
        IReadOnlyList<string> forbiddenRoots,
        bool licenseAccepted)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bundleManifestPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRoot);
        ArgumentNullException.ThrowIfNull(forbiddenRoots);
        return new(bundleManifestPath, workspaceRoot, forbiddenRoots, licenseAccepted);
    }

    public async ValueTask<Ra2MeshGenerationResult> ProbeAsync(CancellationToken cancellationToken = default)
    {
        if (!TryCreateConfiguration(out Ra2GenerationProviderConfiguration? configuration, out Ra2MeshGenerationResult? failure))
            return failure!;

        Ra2GenerationProbeResult result = await new Ra2VoxelGenerationHost()
            .ProbeAsync(configuration!, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded || result.Descriptor is null)
            return FromHostFailure(result.State.ToString(), result.FailureKind, result.Message);
        return Ra2MeshGenerationResult.Ready(result.Descriptor.ProviderId, result.Descriptor.ModelId);
    }

    public async ValueTask<Ra2MeshGenerationResult> GenerateAsync(
        Ra2MeshGenerationRequest request,
        IProgress<Ra2MeshGenerationProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!ValidateRequest(request, out string validationMessage))
            return Ra2MeshGenerationResult.Failure("Failed", Ra2MeshGenerationFailureKind.InvalidRequest, validationMessage);
        if (!TryCreateConfiguration(out Ra2GenerationProviderConfiguration? configuration, out Ra2MeshGenerationResult? failure))
            return failure!;

        var hostProgress = progress is null ? null : new Progress<Ra2GenerationProgress>(item =>
            progress.Report(new(item.Sequence, item.Phase, item.Percent, item.Message)));
        var reference = new Ra2GenerationReferenceImage(
            request.ReferenceName,
            request.ReferenceFormat == Ra2ReferenceImageFormat.Png ? Ra2GenerationMediaKind.Png : Ra2GenerationMediaKind.Jpeg,
            request.ReferenceImage.Span);
        var hostRequest = new Ra2GenerationRequest(
            Guid.NewGuid(),
            request.DesignBrief,
            request.NegativeConstraints,
            [reference],
            request.Seed,
            candidateCount: 1,
            includePreviewPng: true,
            ProviderId,
            ModelRevision,
            request.Timeout);

        Ra2GenerationRunResult result = await new Ra2VoxelGenerationHost()
            .RunAsync(configuration!, hostRequest, hostProgress, cancellationToken).ConfigureAwait(false);
        if (!result.Succeeded || result.Lease is null)
            return FromHostFailure(result.State.ToString(), result.FailureKind, result.Message);

        await using IRa2GenerationWorkspaceLease lease = result.Lease;
        try
        {
            if (lease.Candidates.Count != 1)
                return Ra2MeshGenerationResult.Failure("Failed", Ra2MeshGenerationFailureKind.ArtifactRejected, "Provider returned an unexpected candidate count.");
            Ra2GenerationCandidate candidate = lease.Candidates[0];
            Ra2GenerationArtifact? glb = candidate.Artifacts.SingleOrDefault(a => a.Kind == Ra2GenerationArtifactKind.MeshGlb);
            if (glb is null)
                return Ra2MeshGenerationResult.Failure("Failed", Ra2MeshGenerationFailureKind.ArtifactMissing, "Provider did not return a GLB artifact.");
            byte[] glbBytes = await ReadOwnedArtifactAsync(lease, candidate.CandidateId, glb, MaximumOwnedGlbBytes, cancellationToken).ConfigureAwait(false);
            Ra2GenerationArtifact? preview = candidate.Artifacts.SingleOrDefault(a => a.Kind == Ra2GenerationArtifactKind.PreviewPng);
            byte[]? previewBytes = preview is null ? null : await ReadOwnedArtifactAsync(lease, candidate.CandidateId, preview, MaximumOwnedPreviewBytes, cancellationToken).ConfigureAwait(false);
            return Ra2MeshGenerationResult.Candidate(ProviderId, ModelId, glbBytes, previewBytes);
        }
        catch (OperationCanceledException)
        {
            return Ra2MeshGenerationResult.Failure("Canceled", Ra2MeshGenerationFailureKind.Canceled, "Generation was canceled.");
        }
        catch (InvalidDataException exception)
        {
            return Ra2MeshGenerationResult.Failure("Failed", Ra2MeshGenerationFailureKind.ArtifactRejected, exception.Message);
        }
        catch (IOException exception)
        {
            return Ra2MeshGenerationResult.Failure("Failed", Ra2MeshGenerationFailureKind.ArtifactRejected, exception.Message);
        }
    }

    private bool TryCreateConfiguration(
        out Ra2GenerationProviderConfiguration? configuration,
        out Ra2MeshGenerationResult? failure)
    {
        configuration = null;
        failure = null;
        if (!File.Exists(_bundleManifestPath))
        {
            failure = Ra2MeshGenerationResult.Failure("Failed", Ra2MeshGenerationFailureKind.BundleMissing, "Bundled generation provider is missing.");
            return false;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(File.ReadAllBytes(_bundleManifestPath));
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || HasDuplicateOrUnknownManifestProperties(root))
                throw new InvalidDataException();
            string schema = RequiredString(root, "schema");
            string executable = RequiredString(root, "executable");
            string sha256 = RequiredString(root, "sha256");
            if (!string.Equals(schema, ManifestSchema, StringComparison.Ordinal) ||
                !string.Equals(RequiredString(root, "protocol"), Ra2GenerationLimits.ProtocolIdentity, StringComparison.Ordinal) ||
                !string.Equals(RequiredString(root, "providerId"), ProviderId, StringComparison.Ordinal) ||
                !string.Equals(RequiredString(root, "providerVersion"), ProviderVersion, StringComparison.Ordinal) ||
                !string.Equals(RequiredString(root, "modelId"), ModelId, StringComparison.Ordinal) ||
                !string.Equals(RequiredString(root, "modelRevision"), ModelRevision, StringComparison.Ordinal) ||
                !string.Equals(RequiredString(root, "capability"), "ReferenceImageToMesh", StringComparison.Ordinal) ||
                sha256.Length != 64 || !sha256.All(Uri.IsHexDigit))
            {
                throw new InvalidDataException();
            }
            string bundleRoot = Path.GetDirectoryName(_bundleManifestPath)!;
            if ((File.GetAttributes(_bundleManifestPath) & FileAttributes.ReparsePoint) != 0 ||
                (File.GetAttributes(bundleRoot) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException();
            string executablePath = Path.GetFullPath(Path.Combine(bundleRoot, executable));
            if (!IsSameOrDescendant(executablePath, bundleRoot) ||
                (File.GetAttributes(executablePath) & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidDataException();
            }
            configuration = new(
                executablePath,
                sha256.ToUpperInvariant(),
                ProviderId,
                ProviderVersion,
                ModelId,
                ModelRevision,
                Ra2GenerationCapability.ReferenceImageToMesh,
                _licenseAccepted,
                _workspaceRoot,
                _forbiddenRoots);
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or JsonException or InvalidDataException or ArgumentException)
        {
            failure = Ra2MeshGenerationResult.Failure("Failed", Ra2MeshGenerationFailureKind.BundleRejected, "Bundled generation provider manifest is invalid.");
            return false;
        }
    }

    private static bool ValidateRequest(Ra2MeshGenerationRequest request, out string message)
    {
        message = string.Empty;
        ReadOnlySpan<byte> bytes = request.ReferenceImage.Span;
        if (string.IsNullOrWhiteSpace(request.ReferenceName) || bytes.Length is < 1 or > MaximumReferenceBytes)
            message = "Exactly one bounded reference image is required.";
        else if (request.ReferenceFormat == Ra2ReferenceImageFormat.Png &&
                 (bytes.Length < 8 || !bytes[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })))
            message = "The reference image is not a valid PNG signature.";
        else if (request.ReferenceFormat == Ra2ReferenceImageFormat.Jpeg &&
                 (bytes.Length < 3 || bytes[0] != 0xFF || bytes[1] != 0xD8 || bytes[2] != 0xFF))
            message = "The reference image is not a valid JPEG signature.";
        else if (System.Text.Encoding.UTF8.GetByteCount(request.DesignBrief) > 8 * 1024 ||
                 System.Text.Encoding.UTF8.GetByteCount(request.NegativeConstraints) > 4 * 1024)
            message = "Generation text exceeds the bounded product contract.";
        else if (request.Timeout < TimeSpan.FromMinutes(1) || request.Timeout > TimeSpan.FromMinutes(20))
            message = "Generation timeout must be between one and twenty minutes.";
        return message.Length == 0;
    }

    private static async Task<byte[]> ReadOwnedArtifactAsync(
        IRa2GenerationWorkspaceLease lease,
        string candidateId,
        Ra2GenerationArtifact artifact,
        int maximumBytes,
        CancellationToken cancellationToken)
    {
        if (artifact.Length is < 1 || artifact.Length > maximumBytes)
            throw new InvalidDataException("Generated artifact exceeds the IDE-owned memory boundary.");
        await using Stream stream = await lease.OpenArtifactReadAsync(candidateId, artifact.ArtifactId, cancellationToken).ConfigureAwait(false);
        using var memory = new MemoryStream((int)artifact.Length);
        await stream.CopyToAsync(memory, cancellationToken).ConfigureAwait(false);
        byte[] bytes = memory.ToArray();
        if (bytes.Length != artifact.Length ||
            !string.Equals(Convert.ToHexString(SHA256.HashData(bytes)), artifact.Sha256, StringComparison.Ordinal))
            throw new InvalidDataException("Generated artifact failed the owned-copy integrity check.");
        return bytes;
    }

    private static string RequiredString(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out JsonElement value) || value.ValueKind != JsonValueKind.String ||
            string.IsNullOrWhiteSpace(value.GetString()))
            throw new InvalidDataException();
        return value.GetString()!;
    }

    private static bool HasDuplicateOrUnknownManifestProperties(JsonElement root)
    {
        string[] allowed =
        [
            "schema", "executable", "sha256", "protocol", "providerId",
            "providerVersion", "modelId", "modelRevision", "capability"
        ];
        HashSet<string> names = new(StringComparer.Ordinal);
        foreach (JsonProperty property in root.EnumerateObject())
        {
            if (!allowed.Contains(property.Name, StringComparer.Ordinal) || !names.Add(property.Name))
                return true;
        }
        return names.Count != allowed.Length;
    }

    private static bool IsSameOrDescendant(string path, string root)
    {
        string normalizedRoot = Path.TrimEndingDirectorySeparator(Path.GetFullPath(root));
        string normalizedPath = Path.GetFullPath(path);
        return string.Equals(normalizedPath, normalizedRoot, StringComparison.OrdinalIgnoreCase) ||
            normalizedPath.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static Ra2MeshGenerationResult FromHostFailure(string state, Ra2GenerationFailureKind kind, string message) =>
        Ra2MeshGenerationResult.Failure(state, kind switch
        {
            Ra2GenerationFailureKind.InvalidRequest => Ra2MeshGenerationFailureKind.InvalidRequest,
            Ra2GenerationFailureKind.ProviderNotConfigured => Ra2MeshGenerationFailureKind.ProviderNotConfigured,
            Ra2GenerationFailureKind.ProviderNotReady => Ra2MeshGenerationFailureKind.ProviderNotReady,
            Ra2GenerationFailureKind.CapabilityUnsupported => Ra2MeshGenerationFailureKind.CapabilityUnavailable,
            Ra2GenerationFailureKind.LicenseNotAccepted => Ra2MeshGenerationFailureKind.ConsentDeclined,
            Ra2GenerationFailureKind.OutputMissing => Ra2MeshGenerationFailureKind.ArtifactMissing,
            Ra2GenerationFailureKind.OutputRejected => Ra2MeshGenerationFailureKind.ArtifactRejected,
            Ra2GenerationFailureKind.ResourceLimitExceeded => Ra2MeshGenerationFailureKind.ResourceLimitExceeded,
            Ra2GenerationFailureKind.TimedOut => Ra2MeshGenerationFailureKind.TimedOut,
            Ra2GenerationFailureKind.Canceled => Ra2MeshGenerationFailureKind.Canceled,
            Ra2GenerationFailureKind.CleanupFailed => Ra2MeshGenerationFailureKind.CleanupFailed,
            Ra2GenerationFailureKind.ProviderReportedFailure or Ra2GenerationFailureKind.ProcessCrashed or
                Ra2GenerationFailureKind.ProcessStartFailed => Ra2MeshGenerationFailureKind.RemoteGenerationFailed,
            _ => Ra2MeshGenerationFailureKind.UnexpectedFailure
        }, string.IsNullOrWhiteSpace(message) ? "Generation provider operation failed." : message);
}
