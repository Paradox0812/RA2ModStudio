using System.Text.Json;

namespace RA2IniEditor.AssetHost;

internal enum Ra2ProviderOperation
{
    Probe = 0,
    Generate
}

internal sealed class Ra2GenerationProtocolSession
{
    private readonly Ra2ProviderOperation _operation;
    private readonly Ra2GenerationProviderConfiguration _configuration;
    private readonly Ra2GenerationRequest? _request;
    private readonly Action<Ra2GenerationProgress>? _progressSink;
    private readonly List<Ra2GenerationCandidateDeclaration> _candidates = new();
    private readonly Queue<Ra2GenerationProgress> _progressSummary = new();
    private readonly HashSet<string> _candidateIds = new(StringComparer.Ordinal);
    private bool _started;
    private bool _terminal;
    private long _lastProgressSequence = -1;
    private int _progressCount;

    internal Ra2GenerationProtocolSession(
        Ra2ProviderOperation operation,
        Ra2GenerationProviderConfiguration configuration,
        Ra2GenerationRequest? request,
        Action<Ra2GenerationProgress>? progressSink = null)
    {
        _operation = operation;
        _configuration = configuration;
        _request = request;
        _progressSink = progressSink;
    }

    internal Ra2GenerationProviderDescriptor? Descriptor { get; private set; }
    internal bool ModelReady { get; private set; }
    internal bool HasTerminal => _terminal;
    internal Ra2GenerationFailureKind ProviderFailureKind { get; private set; }
    internal string ProviderFailureMessage { get; private set; } = string.Empty;
    internal IReadOnlyList<Ra2GenerationCandidateDeclaration> Candidates => _candidates;
    internal IReadOnlyList<Ra2GenerationProgress> ProgressSummary => _progressSummary.ToArray();
    internal IReadOnlyList<string> CompletedCandidateIds { get; private set; } = Array.Empty<string>();

    internal void AcceptLine(ReadOnlySpan<byte> utf8Line)
    {
        if (_terminal)
        {
            throw new Ra2GenerationProtocolException("Protocol output followed the terminal message.");
        }

        RejectDuplicateRootProperties(utf8Line);
        using JsonDocument document = JsonDocument.Parse(utf8Line.ToArray(), new JsonDocumentOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 16
        });

        JsonElement root = document.RootElement;
        if (root.ValueKind != JsonValueKind.Object || !TryGetRequiredString(root, "kind", 64, out string kind))
        {
            throw new Ra2GenerationProtocolException("Protocol message kind is missing or invalid.");
        }

        switch (kind)
        {
            case "started":
                AcceptStarted(root);
                return;
            case "probe_completed":
                AcceptProbeCompleted(root);
                return;
            case "progress":
                AcceptProgress(root);
                return;
            case "candidate":
                AcceptCandidate(root);
                return;
            case "completed":
                AcceptCompleted(root);
                return;
            case "failed":
                AcceptFailed(root);
                return;
            default:
                throw new Ra2GenerationProtocolException("The protocol message is invalid for this operation.");
        }
    }

    internal void EnsureProbeCompleted()
    {
        if (!_started || !_terminal || Descriptor is null)
        {
            throw new Ra2GenerationProtocolException("The readiness probe did not complete its required protocol flow.");
        }
    }

    internal void EnsureGenerationCompleted()
    {
        if (_operation != Ra2ProviderOperation.Generate || !_started || !_terminal ||
            ProviderFailureKind != Ra2GenerationFailureKind.None || _candidates.Count == 0 ||
            CompletedCandidateIds.Count != _candidates.Count ||
            !CompletedCandidateIds.SequenceEqual(_candidates.Select(candidate => candidate.CandidateId), StringComparer.Ordinal))
        {
            throw new Ra2GenerationProtocolException("The generation protocol did not close its declared candidate set.");
        }
    }

    private void AcceptStarted(JsonElement root)
    {
        if (_started || _terminal)
        {
            throw new Ra2GenerationProtocolException("The provider emitted duplicate or misplaced started evidence.");
        }

        string expectedOperation = _operation == Ra2ProviderOperation.Probe ? "probe" : "generate";
        if (!TryGetRequiredString(root, "protocol", 64, out string protocol) ||
            !string.Equals(protocol, Ra2GenerationLimits.ProtocolIdentity, StringComparison.Ordinal) ||
            !TryGetRequiredString(root, "operation", 16, out string operation) ||
            !string.Equals(operation, expectedOperation, StringComparison.Ordinal) ||
            !TryGetRequiredString(root, "providerId", 128, out string providerId) ||
            !TryGetRequiredString(root, "providerVersion", 128, out string providerVersion) ||
            !TryGetRequiredString(root, "modelId", 128, out string modelId) ||
            !TryGetRequiredString(root, "modelRevision", 128, out string modelRevision))
        {
            throw new Ra2GenerationProtocolException("The provider start acknowledgement is invalid.");
        }

        if (!string.Equals(providerId, _configuration.ExpectedProviderId, StringComparison.Ordinal) ||
            !string.Equals(providerVersion, _configuration.ExpectedProviderVersion, StringComparison.Ordinal) ||
            !string.Equals(modelId, _configuration.ExpectedModelId, StringComparison.Ordinal) ||
            !string.Equals(modelRevision, _configuration.ExpectedModelRevision, StringComparison.Ordinal))
        {
            throw new Ra2GenerationIdentityException();
        }

        if (_operation == Ra2ProviderOperation.Generate &&
            (!TryGetRequiredString(root, "requestFingerprint", 64, out string requestFingerprint) ||
             !string.Equals(requestFingerprint, _request!.Fingerprint, StringComparison.Ordinal)))
        {
            throw new Ra2GenerationProtocolException("The provider acknowledged a different generation request.");
        }

        _started = true;
    }

    private void AcceptProbeCompleted(JsonElement root)
    {
        if (_operation != Ra2ProviderOperation.Probe || !_started || _terminal)
        {
            throw new Ra2GenerationProtocolException("The probe terminal message is misplaced.");
        }

        if (!root.TryGetProperty("descriptor", out JsonElement descriptorElement) ||
            descriptorElement.ValueKind != JsonValueKind.Object ||
            !TryParseDescriptor(descriptorElement, out Ra2GenerationProviderDescriptor? descriptor) ||
            !root.TryGetProperty("modelReady", out JsonElement modelReadyElement) ||
            modelReadyElement.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            throw new Ra2GenerationProtocolException("The probe result is invalid.");
        }

        Descriptor = descriptor;
        ModelReady = modelReadyElement.GetBoolean();
        _terminal = true;
    }

    private void AcceptProgress(JsonElement root)
    {
        if (_operation != Ra2ProviderOperation.Generate || !_started || _terminal ||
            !TryGetRequiredInt64(root, "sequence", out long sequence) || sequence <= _lastProgressSequence ||
            !TryGetRequiredString(root, "phase", 128, out string phase) ||
            !TryGetOptionalString(root, "message", 1024, out string message))
        {
            throw new Ra2GenerationProtocolException("The progress message is invalid or out of order.");
        }

        double? percent = null;
        if (root.TryGetProperty("percent", out JsonElement percentElement))
        {
            if (!percentElement.TryGetDouble(out double parsedPercent) || !double.IsFinite(parsedPercent) ||
                parsedPercent is < 0 or > 100)
            {
                throw new Ra2GenerationProtocolException("The progress percentage is invalid.");
            }

            percent = parsedPercent;
        }

        _progressCount++;
        if (_progressCount > Ra2GenerationLimits.MaximumProgressEvents)
        {
            throw new Ra2GenerationProtocolException("The provider emitted too many progress messages.");
        }

        _lastProgressSequence = sequence;
        var progress = new Ra2GenerationProgress(sequence, phase, percent, message);
        if (_progressSummary.Count == 64)
        {
            _progressSummary.Dequeue();
        }

        _progressSummary.Enqueue(progress);
        _progressSink?.Invoke(progress);
    }

    private void AcceptCandidate(JsonElement root)
    {
        if (_operation != Ra2ProviderOperation.Generate || !_started || _terminal ||
            !TryGetRequiredString(root, "candidateId", 128, out string candidateId) ||
            !Ra2GenerationValidation.IsIdentity(candidateId) || !_candidateIds.Add(candidateId) ||
            !root.TryGetProperty("artifacts", out JsonElement artifactsElement) ||
            artifactsElement.ValueKind != JsonValueKind.Array)
        {
            throw new Ra2GenerationProtocolException("The candidate declaration is invalid.");
        }

        if (_candidates.Count >= Ra2GenerationLimits.MaximumCandidateCount)
        {
            throw new Ra2GenerationProtocolException("The provider declared too many candidates.");
        }

        var artifacts = new List<Ra2GenerationArtifactDeclaration>();
        var artifactIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (JsonElement artifact in artifactsElement.EnumerateArray())
        {
            if (artifact.ValueKind != JsonValueKind.Object ||
                !TryGetRequiredString(artifact, "artifactId", 128, out string artifactId) ||
                !Ra2GenerationValidation.IsIdentity(artifactId) || !artifactIds.Add(artifactId) ||
                !TryGetRequiredString(artifact, "kind", 64, out string artifactKindText) ||
                !Enum.TryParse(artifactKindText, ignoreCase: false, out Ra2GenerationArtifactKind artifactKind) ||
                !TryGetRequiredString(artifact, "path", 512, out string path) ||
                !TryGetRequiredInt64(artifact, "length", out long length) || length <= 0 ||
                !TryGetRequiredString(artifact, "sha256", 64, out string sha256) ||
                !Ra2GenerationValidation.IsUpperSha256(sha256))
            {
                throw new Ra2GenerationProtocolException("A candidate artifact declaration is invalid.");
            }

            artifacts.Add(new Ra2GenerationArtifactDeclaration(artifactId, artifactKind, path, length, sha256));
        }

        if (artifacts.Count == 0 || artifacts.Count > 3 || artifacts.All(artifact => artifact.Kind != Ra2GenerationArtifactKind.MeshGlb))
        {
            throw new Ra2GenerationProtocolException("A candidate must declare one mesh artifact and a bounded optional preview/report set.");
        }

        _candidates.Add(new Ra2GenerationCandidateDeclaration(candidateId, artifacts));
    }

    private void AcceptCompleted(JsonElement root)
    {
        if (_operation != Ra2ProviderOperation.Generate || !_started || _terminal ||
            !TryGetRequiredString(root, "requestFingerprint", 64, out string requestFingerprint) ||
            !string.Equals(requestFingerprint, _request!.Fingerprint, StringComparison.Ordinal) ||
            !root.TryGetProperty("candidateIds", out JsonElement candidatesElement) ||
            candidatesElement.ValueKind != JsonValueKind.Array)
        {
            throw new Ra2GenerationProtocolException("The generation terminal message is invalid.");
        }

        var ids = new List<string>();
        foreach (JsonElement candidate in candidatesElement.EnumerateArray())
        {
            if (candidate.ValueKind != JsonValueKind.String ||
                !Ra2GenerationValidation.IsIdentity(candidate.GetString() ?? string.Empty))
            {
                throw new Ra2GenerationProtocolException("The generation terminal candidate set is invalid.");
            }

            ids.Add(candidate.GetString()!);
        }

        CompletedCandidateIds = ids.AsReadOnly();
        _terminal = true;
    }

    private void AcceptFailed(JsonElement root)
    {
        if (!_started || _terminal ||
            !TryGetRequiredString(root, "failureKind", 64, out string failureKindText) ||
            !Enum.TryParse(failureKindText, ignoreCase: false, out Ra2GenerationFailureKind failureKind) ||
            !IsProviderOwnedFailure(failureKind) ||
            !TryGetOptionalString(root, "message", 1024, out string message))
        {
            throw new Ra2GenerationProtocolException("The provider failure message is invalid.");
        }

        ProviderFailureKind = failureKind;
        ProviderFailureMessage = message;
        _terminal = true;
    }

    private static bool IsProviderOwnedFailure(Ra2GenerationFailureKind failureKind) => failureKind is
        Ra2GenerationFailureKind.InvalidRequest or
        Ra2GenerationFailureKind.ProviderNotReady or
        Ra2GenerationFailureKind.CapabilityUnsupported or
        Ra2GenerationFailureKind.ProviderReportedFailure or
        Ra2GenerationFailureKind.OutputMissing or
        Ra2GenerationFailureKind.ResourceLimitExceeded;

    private bool TryParseDescriptor(JsonElement root, out Ra2GenerationProviderDescriptor? descriptor)
    {
        descriptor = null;
        if (!TryGetRequiredString(root, "providerId", 128, out string providerId) ||
            !TryGetRequiredInt32(root, "protocolVersion", out int protocolVersion) ||
            !TryGetRequiredString(root, "providerVersion", 128, out string providerVersion) ||
            !TryGetRequiredString(root, "modelId", 128, out string modelId) ||
            !TryGetRequiredString(root, "modelRevision", 128, out string modelRevision) ||
            !TryGetRequiredString(root, "executableSha256", 64, out string executableSha256) ||
            !TryGetRequiredString(root, "seedBehavior", 64, out string seedBehaviorText) ||
            !Enum.TryParse(seedBehaviorText, ignoreCase: false, out Ra2GenerationSeedBehavior seedBehavior) ||
            !TryGetRequiredInt32(root, "maximumReferenceCount", out int maximumReferenceCount) ||
            !TryGetRequiredInt32(root, "maximumCandidateCount", out int maximumCandidateCount) ||
            !TryGetRequiredInt64(root, "maximumInputBytes", out long maximumInputBytes) ||
            !TryGetRequiredInt64(root, "maximumOutputBytes", out long maximumOutputBytes) ||
            !TryGetRequiredString(root, "licenseId", 128, out string licenseId) ||
            !TryGetOptionalString(root, "licenseUrl", 2048, out string licenseUrl) ||
            !TryGetRequiredBoolean(root, "redistributable", out bool redistributable) ||
            !TryGetRequiredBoolean(root, "requiresUserAcceptance", out bool requiresUserAcceptance) ||
            !root.TryGetProperty("capabilities", out JsonElement capabilitiesElement) ||
            capabilitiesElement.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        Ra2GenerationCapability capabilities = Ra2GenerationCapability.None;
        foreach (JsonElement capability in capabilitiesElement.EnumerateArray())
        {
            if (capability.ValueKind != JsonValueKind.String ||
                !Enum.TryParse(capability.GetString(), ignoreCase: false, out Ra2GenerationCapability parsed) ||
                parsed == Ra2GenerationCapability.None)
            {
                return false;
            }

            capabilities |= parsed;
        }

        descriptor = new Ra2GenerationProviderDescriptor(
            providerId,
            protocolVersion,
            providerVersion,
            modelId,
            modelRevision,
            executableSha256,
            capabilities,
            seedBehavior,
            maximumReferenceCount,
            maximumCandidateCount,
            maximumInputBytes,
            maximumOutputBytes,
            licenseId,
            licenseUrl,
            redistributable,
            requiresUserAcceptance);
        return true;
    }

    private static void RejectDuplicateRootProperties(ReadOnlySpan<byte> utf8Json)
    {
        var reader = new Utf8JsonReader(utf8Json, new JsonReaderOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 16
        });
        var names = new HashSet<string>(StringComparer.Ordinal);
        int depth = -1;
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.StartObject)
            {
                depth++;
            }
            else if (reader.TokenType == JsonTokenType.EndObject)
            {
                depth--;
            }
            else if (reader.TokenType == JsonTokenType.PropertyName && depth == 0)
            {
                string name = reader.GetString() ?? string.Empty;
                if (!names.Add(name))
                {
                    throw new Ra2GenerationProtocolException("The protocol message contains a duplicate root property.");
                }
            }
        }
    }

    private static bool TryGetRequiredString(JsonElement root, string name, int maximumBytes, out string value)
    {
        value = string.Empty;
        if (!root.TryGetProperty(name, out JsonElement element) || element.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = element.GetString() ?? string.Empty;
        return Ra2GenerationValidation.IsBoundedText(value, maximumBytes);
    }

    private static bool TryGetOptionalString(JsonElement root, string name, int maximumBytes, out string value)
    {
        value = string.Empty;
        if (!root.TryGetProperty(name, out JsonElement element))
        {
            return true;
        }

        if (element.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = element.GetString() ?? string.Empty;
        return value.Length == 0 || (System.Text.Encoding.UTF8.GetByteCount(value) <= maximumBytes && !value.Any(char.IsControl));
    }

    private static bool TryGetRequiredInt32(JsonElement root, string name, out int value)
    {
        value = 0;
        return root.TryGetProperty(name, out JsonElement element) && element.TryGetInt32(out value);
    }

    private static bool TryGetRequiredInt64(JsonElement root, string name, out long value)
    {
        value = 0;
        return root.TryGetProperty(name, out JsonElement element) && element.TryGetInt64(out value);
    }

    private static bool TryGetRequiredBoolean(JsonElement root, string name, out bool value)
    {
        value = false;
        if (!root.TryGetProperty(name, out JsonElement element) || element.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
        {
            return false;
        }

        value = element.GetBoolean();
        return true;
    }
}

internal sealed record Ra2GenerationArtifactDeclaration(
    string ArtifactId,
    Ra2GenerationArtifactKind Kind,
    string RelativePath,
    long Length,
    string Sha256);

internal sealed class Ra2GenerationCandidateDeclaration
{
    private readonly IReadOnlyList<Ra2GenerationArtifactDeclaration> _artifacts;

    internal Ra2GenerationCandidateDeclaration(
        string candidateId,
        IEnumerable<Ra2GenerationArtifactDeclaration> artifacts)
    {
        CandidateId = candidateId;
        _artifacts = Array.AsReadOnly(artifacts.ToArray());
    }

    internal string CandidateId { get; }
    internal IReadOnlyList<Ra2GenerationArtifactDeclaration> Artifacts => _artifacts;
}

internal sealed class Ra2GenerationProtocolException : Exception
{
    internal Ra2GenerationProtocolException(string message)
        : base(message)
    {
    }
}

internal sealed class Ra2GenerationIdentityException : Exception
{
}
