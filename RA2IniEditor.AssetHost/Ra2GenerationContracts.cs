using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;

namespace RA2IniEditor.AssetHost;

internal static class Ra2GenerationLimits
{
    public const string ProtocolIdentity = "ra2-voxel-generation/1";
    public const int ProtocolVersion = 1;
    public const int MaximumPromptBytes = 16 * 1024;
    public const int MaximumNegativeConstraintBytes = 8 * 1024;
    public const int MaximumReferenceCount = 4;
    public const int MaximumReferenceBytes = 32 * 1024 * 1024;
    public const int MaximumInputBytes = 64 * 1024 * 1024;
    public const int MaximumCandidateCount = 4;
    public const long MaximumArtifactBytes = 256L * 1024 * 1024;
    public const long MaximumRunBytes = 512L * 1024 * 1024;
    public const long DefaultWorkspaceRootBytes = 4L * 1024 * 1024 * 1024;
    public const long MinimumWorkspaceRootBytes = MaximumRunBytes;
    public const long MaximumWorkspaceRootBytes = 64L * 1024 * 1024 * 1024;
    public const int MaximumProtocolLineBytes = 1024 * 1024;
    public const int MaximumProtocolLines = 4096;
    public const int MaximumProgressEvents = 1024;
    public const int MaximumStandardErrorBytes = 64 * 1024;
    public static readonly TimeSpan DefaultProbeTimeout = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan MinimumProbeTimeout = TimeSpan.FromSeconds(5);
    public static readonly TimeSpan MaximumProbeTimeout = TimeSpan.FromSeconds(60);
    public static readonly TimeSpan DefaultOrphanTtl = TimeSpan.FromHours(24);
    public static readonly TimeSpan MinimumOrphanTtl = TimeSpan.FromHours(1);
    public static readonly TimeSpan MaximumOrphanTtl = TimeSpan.FromDays(30);
    public static readonly TimeSpan MinimumRunTimeout = TimeSpan.FromSeconds(10);
    public static readonly TimeSpan MaximumRunTimeout = TimeSpan.FromMinutes(30);
}

[Flags]
internal enum Ra2GenerationCapability
{
    None = 0,
    ReferenceImageToMesh = 1
}

internal enum Ra2GenerationSeedBehavior
{
    Unsupported = 0,
    BestEffort,
    DeterministicDeclared
}

internal enum Ra2GenerationMediaKind
{
    Png = 0,
    Jpeg,
    Webp
}

internal enum Ra2GenerationArtifactKind
{
    MeshGlb = 0,
    PreviewPng,
    ProviderJson
}

internal enum Ra2GenerationState
{
    Created = 0,
    Starting,
    Probing,
    Ready,
    Running,
    Validating,
    CandidateReady,
    Failed,
    Canceled,
    TimedOut
}

internal enum Ra2GenerationFailureKind
{
    None = 0,
    InvalidRequest,
    ProviderNotConfigured,
    ProviderNotReady,
    ProviderIdentityMismatch,
    ExecutableHashMismatch,
    CapabilityUnsupported,
    LicenseNotAccepted,
    WorkspaceRejected,
    ProcessStartFailed,
    ProtocolViolation,
    ProviderReportedFailure,
    OutputMissing,
    OutputRejected,
    ResourceLimitExceeded,
    TimedOut,
    Canceled,
    TerminationFailed,
    ProcessCrashed,
    ReplayMismatch,
    CleanupFailed,
    UnexpectedFailure
}

internal sealed class Ra2GenerationProviderConfiguration
{
    private readonly ReadOnlyCollection<string> _forbiddenRoots;

    public Ra2GenerationProviderConfiguration(
        string executablePath,
        string expectedExecutableSha256,
        string expectedProviderId,
        string expectedProviderVersion,
        string expectedModelId,
        string expectedModelRevision,
        Ra2GenerationCapability requiredCapability,
        bool licenseAccepted,
        string workspaceRoot,
        IEnumerable<string>? forbiddenRoots = null,
        TimeSpan? probeTimeout = null,
        TimeSpan? orphanTtl = null,
        long maximumWorkspaceRootBytes = Ra2GenerationLimits.DefaultWorkspaceRootBytes)
    {
        ExecutablePath = executablePath ?? string.Empty;
        ExpectedExecutableSha256 = expectedExecutableSha256 ?? string.Empty;
        ExpectedProviderId = expectedProviderId ?? string.Empty;
        ExpectedProviderVersion = expectedProviderVersion ?? string.Empty;
        ExpectedModelId = expectedModelId ?? string.Empty;
        ExpectedModelRevision = expectedModelRevision ?? string.Empty;
        RequiredCapability = requiredCapability;
        LicenseAccepted = licenseAccepted;
        WorkspaceRoot = workspaceRoot ?? string.Empty;
        _forbiddenRoots = Array.AsReadOnly((forbiddenRoots ?? Array.Empty<string>()).ToArray());
        ProbeTimeout = probeTimeout ?? Ra2GenerationLimits.DefaultProbeTimeout;
        OrphanTtl = orphanTtl ?? Ra2GenerationLimits.DefaultOrphanTtl;
        MaximumWorkspaceRootBytes = maximumWorkspaceRootBytes;
    }

    public string ExecutablePath { get; }
    public string ExpectedExecutableSha256 { get; }
    public string ExpectedProviderId { get; }
    public string ExpectedProviderVersion { get; }
    public string ExpectedModelId { get; }
    public string ExpectedModelRevision { get; }
    public Ra2GenerationCapability RequiredCapability { get; }
    public bool LicenseAccepted { get; }
    public string WorkspaceRoot { get; }
    public IReadOnlyList<string> ForbiddenRoots => _forbiddenRoots;
    public TimeSpan ProbeTimeout { get; }
    public TimeSpan OrphanTtl { get; }
    public long MaximumWorkspaceRootBytes { get; }
}

internal sealed class Ra2GenerationProviderDescriptor
{
    public Ra2GenerationProviderDescriptor(
        string providerId,
        int protocolVersion,
        string providerVersion,
        string modelId,
        string modelRevision,
        string executableSha256,
        Ra2GenerationCapability capabilities,
        Ra2GenerationSeedBehavior seedBehavior,
        int maximumReferenceCount,
        int maximumCandidateCount,
        long maximumInputBytes,
        long maximumOutputBytes,
        string licenseId,
        string licenseUrl,
        bool redistributable,
        bool requiresUserAcceptance)
    {
        ProviderId = providerId;
        ProtocolVersion = protocolVersion;
        ProviderVersion = providerVersion;
        ModelId = modelId;
        ModelRevision = modelRevision;
        ExecutableSha256 = executableSha256;
        Capabilities = capabilities;
        SeedBehavior = seedBehavior;
        MaximumReferenceCount = maximumReferenceCount;
        MaximumCandidateCount = maximumCandidateCount;
        MaximumInputBytes = maximumInputBytes;
        MaximumOutputBytes = maximumOutputBytes;
        LicenseId = licenseId;
        LicenseUrl = licenseUrl;
        Redistributable = redistributable;
        RequiresUserAcceptance = requiresUserAcceptance;
    }

    public string ProviderId { get; }
    public int ProtocolVersion { get; }
    public string ProviderVersion { get; }
    public string ModelId { get; }
    public string ModelRevision { get; }
    public string ExecutableSha256 { get; }
    public Ra2GenerationCapability Capabilities { get; }
    public Ra2GenerationSeedBehavior SeedBehavior { get; }
    public int MaximumReferenceCount { get; }
    public int MaximumCandidateCount { get; }
    public long MaximumInputBytes { get; }
    public long MaximumOutputBytes { get; }
    public string LicenseId { get; }
    public string LicenseUrl { get; }
    public bool Redistributable { get; }
    public bool RequiresUserAcceptance { get; }
}

internal sealed class Ra2GenerationReferenceImage
{
    private readonly byte[] _content;

    public Ra2GenerationReferenceImage(string name, Ra2GenerationMediaKind mediaKind, ReadOnlySpan<byte> content)
    {
        Name = name ?? string.Empty;
        MediaKind = mediaKind;
        _content = content.ToArray();
        Sha256 = Convert.ToHexString(SHA256.HashData(_content));
    }

    public string Name { get; }
    public Ra2GenerationMediaKind MediaKind { get; }
    public int Length => _content.Length;
    public string Sha256 { get; }
    internal ReadOnlyMemory<byte> Content => _content;
}

internal sealed class Ra2GenerationRequest
{
    private readonly ReadOnlyCollection<Ra2GenerationReferenceImage> _references;

    public Ra2GenerationRequest(
        Guid runId,
        string prompt,
        string negativeConstraints,
        IEnumerable<Ra2GenerationReferenceImage> references,
        int seed,
        int candidateCount,
        bool includePreviewPng,
        string expectedProviderId,
        string expectedModelRevision,
        TimeSpan timeout)
    {
        RunId = runId;
        Prompt = prompt ?? string.Empty;
        NegativeConstraints = negativeConstraints ?? string.Empty;
        _references = Array.AsReadOnly((references ?? Array.Empty<Ra2GenerationReferenceImage>()).ToArray());
        Seed = seed;
        CandidateCount = candidateCount;
        IncludePreviewPng = includePreviewPng;
        ExpectedProviderId = expectedProviderId ?? string.Empty;
        ExpectedModelRevision = expectedModelRevision ?? string.Empty;
        Timeout = timeout;
        Fingerprint = ComputeFingerprint();
    }

    public Guid RunId { get; }
    public string Prompt { get; }
    public string NegativeConstraints { get; }
    public IReadOnlyList<Ra2GenerationReferenceImage> References => _references;
    public int Seed { get; }
    public int CandidateCount { get; }
    public bool IncludePreviewPng { get; }
    public string ExpectedProviderId { get; }
    public string ExpectedModelRevision { get; }
    public TimeSpan Timeout { get; }
    public string Fingerprint { get; }

    private string ComputeFingerprint()
    {
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        Append(hash, "ra2-generation-request/1");
        Append(hash, Prompt);
        Append(hash, NegativeConstraints);
        Append(hash, Seed.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Append(hash, CandidateCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
        Append(hash, IncludePreviewPng ? "1" : "0");
        Append(hash, ExpectedProviderId);
        Append(hash, ExpectedModelRevision);
        foreach (Ra2GenerationReferenceImage reference in _references)
        {
            Append(hash, reference.Name);
            Append(hash, reference.MediaKind.ToString());
            Append(hash, reference.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));
            Append(hash, reference.Sha256);
        }

        return Convert.ToHexString(hash.GetHashAndReset());
    }

    private static void Append(IncrementalHash hash, string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        Span<byte> length = stackalloc byte[4];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(length, bytes.Length);
        hash.AppendData(length);
        hash.AppendData(bytes);
    }
}

internal readonly record struct Ra2GenerationProgress(long Sequence, string Phase, double? Percent, string Message);

internal sealed record Ra2GenerationArtifact(
    string ArtifactId,
    Ra2GenerationArtifactKind Kind,
    long Length,
    string Sha256);

internal sealed class Ra2GenerationCandidate
{
    private readonly ReadOnlyCollection<Ra2GenerationArtifact> _artifacts;

    public Ra2GenerationCandidate(string candidateId, IEnumerable<Ra2GenerationArtifact> artifacts)
    {
        CandidateId = candidateId;
        _artifacts = Array.AsReadOnly(artifacts.ToArray());
    }

    public string CandidateId { get; }
    public IReadOnlyList<Ra2GenerationArtifact> Artifacts => _artifacts;
}

internal interface IRa2GenerationWorkspaceLease : IAsyncDisposable
{
    IReadOnlyList<Ra2GenerationCandidate> Candidates { get; }

    ValueTask<Stream> OpenArtifactReadAsync(
        string candidateId,
        string artifactId,
        CancellationToken cancellationToken = default);
}

internal sealed class Ra2GenerationProbeResult
{
    private Ra2GenerationProbeResult(
        Ra2GenerationState state,
        Ra2GenerationFailureKind failureKind,
        string message,
        Ra2GenerationProviderDescriptor? descriptor,
        DateTimeOffset startedUtc,
        DateTimeOffset completedUtc)
    {
        State = state;
        FailureKind = failureKind;
        Message = message;
        Descriptor = descriptor;
        StartedUtc = startedUtc;
        CompletedUtc = completedUtc;
    }

    public bool Succeeded => State == Ra2GenerationState.Ready;
    public Ra2GenerationState State { get; }
    public Ra2GenerationFailureKind FailureKind { get; }
    public string Message { get; }
    public Ra2GenerationProviderDescriptor? Descriptor { get; }
    public DateTimeOffset StartedUtc { get; }
    public DateTimeOffset CompletedUtc { get; }
    public TimeSpan Duration => CompletedUtc - StartedUtc;

    internal static Ra2GenerationProbeResult Ready(
        Ra2GenerationProviderDescriptor descriptor,
        DateTimeOffset startedUtc,
        DateTimeOffset completedUtc) =>
        new(Ra2GenerationState.Ready, Ra2GenerationFailureKind.None, string.Empty, descriptor, startedUtc, completedUtc);

    internal static Ra2GenerationProbeResult Failure(
        Ra2GenerationState state,
        Ra2GenerationFailureKind failureKind,
        string message,
        DateTimeOffset startedUtc,
        DateTimeOffset completedUtc) =>
        new(state, failureKind, message, null, startedUtc, completedUtc);
}

internal sealed class Ra2GenerationRunResult
{
    private readonly ReadOnlyCollection<Ra2GenerationProgress> _progress;

    private Ra2GenerationRunResult(
        Ra2GenerationState state,
        Ra2GenerationFailureKind failureKind,
        string message,
        IRa2GenerationWorkspaceLease? lease,
        IEnumerable<Ra2GenerationProgress> progress,
        DateTimeOffset startedUtc,
        DateTimeOffset completedUtc)
    {
        State = state;
        FailureKind = failureKind;
        Message = message;
        Lease = lease;
        _progress = Array.AsReadOnly(progress.ToArray());
        StartedUtc = startedUtc;
        CompletedUtc = completedUtc;
    }

    public bool Succeeded => State == Ra2GenerationState.CandidateReady;
    public Ra2GenerationState State { get; }
    public Ra2GenerationFailureKind FailureKind { get; }
    public string Message { get; }
    public IRa2GenerationWorkspaceLease? Lease { get; }
    public IReadOnlyList<Ra2GenerationProgress> Progress => _progress;
    public DateTimeOffset StartedUtc { get; }
    public DateTimeOffset CompletedUtc { get; }
    public TimeSpan Duration => CompletedUtc - StartedUtc;

    internal static Ra2GenerationRunResult Success(
        IRa2GenerationWorkspaceLease lease,
        IEnumerable<Ra2GenerationProgress> progress,
        DateTimeOffset startedUtc,
        DateTimeOffset completedUtc) =>
        new(Ra2GenerationState.CandidateReady, Ra2GenerationFailureKind.None, string.Empty, lease, progress, startedUtc, completedUtc);

    internal static Ra2GenerationRunResult Failure(
        Ra2GenerationState state,
        Ra2GenerationFailureKind failureKind,
        string message,
        IEnumerable<Ra2GenerationProgress> progress,
        DateTimeOffset startedUtc,
        DateTimeOffset completedUtc) =>
        new(state, failureKind, message, null, progress, startedUtc, completedUtc);
}

internal interface IRa2VoxelGenerationHost
{
    ValueTask<Ra2GenerationProbeResult> ProbeAsync(
        Ra2GenerationProviderConfiguration configuration,
        CancellationToken cancellationToken = default);

    ValueTask<Ra2GenerationRunResult> RunAsync(
        Ra2GenerationProviderConfiguration configuration,
        Ra2GenerationRequest request,
        IProgress<Ra2GenerationProgress>? progress = null,
        CancellationToken cancellationToken = default);
}

internal sealed class Ra2GenerationWorkspaceCleanupException : IOException
{
    internal Ra2GenerationWorkspaceCleanupException(string message)
        : base(message)
    {
    }
}
