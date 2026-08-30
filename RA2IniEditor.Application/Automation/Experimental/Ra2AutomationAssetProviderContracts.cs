using System.Security.Cryptography;

namespace RA2IniEditor.Application.Automation.Experimental;

public enum Ra2AutomationAssetProviderFailureKind
{
    None = 0,
    InvalidManifest,
    MissingSource,
    UnexpectedSource,
    SourceMismatch,
    AggregateContentLimitExceeded,
    Canceled,
    ProviderFailed
}

public enum Ra2AutomationAssetVerificationLevel
{
    IdentityExtensionAndHash = 0
}

public sealed class Ra2AutomationAssetProviderDescriptor
{
    public Ra2AutomationAssetProviderDescriptor(
        string id,
        int version,
        IEnumerable<Ra2AutomationAssetKind> supportedKinds)
    {
        Id = Ra2AutomationAssetContractValidation.ValidateText(id, 128, nameof(id));
        if (version <= 0)
            throw new ArgumentOutOfRangeException(nameof(version));
        ArgumentNullException.ThrowIfNull(supportedKinds);

        Ra2AutomationAssetKind[] kinds = supportedKinds.Distinct().ToArray();
        if (kinds.Length == 0 || kinds.Any(kind => !Enum.IsDefined(kind)))
            throw new ArgumentException("At least one valid asset kind is required.", nameof(supportedKinds));

        Version = version;
        SupportedKinds = Array.AsReadOnly(kinds);
    }

    public string Id { get; }
    public int Version { get; }
    public IReadOnlyList<Ra2AutomationAssetKind> SupportedKinds { get; }
}

public sealed class Ra2AutomationAssetSource
{
    public const int MaximumContentBytes = 16 * 1024 * 1024;

    private readonly byte[] _content;

    public Ra2AutomationAssetSource(
        string requirementId,
        string fileName,
        Ra2AutomationAssetKind kind,
        byte[] content)
    {
        if (!Enum.IsDefined(kind))
            throw new ArgumentOutOfRangeException(nameof(kind));
        ArgumentNullException.ThrowIfNull(content);
        if (content.Length is < 1 or > MaximumContentBytes)
            throw new ArgumentOutOfRangeException(nameof(content), $"Asset content must contain 1..{MaximumContentBytes} bytes.");

        RequirementId = Ra2AutomationAssetContractValidation.ValidateText(
            requirementId,
            Ra2AutomationAssetRequirement.MaximumRequirementIdLength,
            nameof(requirementId));
        FileName = Ra2AutomationAssetContractValidation.ValidateFileName(fileName, nameof(fileName));
        Kind = kind;
        _content = content.ToArray();
    }

    public string RequirementId { get; }
    public string FileName { get; }
    public Ra2AutomationAssetKind Kind { get; }
    public int ContentLength => _content.Length;

    public byte[] CopyContent() => _content.ToArray();
}

public sealed class Ra2AutomationAssetArtifact
{
    private readonly byte[] _content;

    public Ra2AutomationAssetArtifact(
        string requirementId,
        string fileName,
        Ra2AutomationAssetKind kind,
        byte[] content,
        Ra2AutomationAssetVerificationLevel verificationLevel)
    {
        if (!Enum.IsDefined(kind))
            throw new ArgumentOutOfRangeException(nameof(kind));
        if (!Enum.IsDefined(verificationLevel))
            throw new ArgumentOutOfRangeException(nameof(verificationLevel));
        ArgumentNullException.ThrowIfNull(content);
        if (content.Length is < 1 or > Ra2AutomationAssetSource.MaximumContentBytes)
            throw new ArgumentOutOfRangeException(nameof(content));

        RequirementId = Ra2AutomationAssetContractValidation.ValidateText(
            requirementId,
            Ra2AutomationAssetRequirement.MaximumRequirementIdLength,
            nameof(requirementId));
        FileName = Ra2AutomationAssetContractValidation.ValidateFileName(fileName, nameof(fileName));
        Kind = kind;
        VerificationLevel = verificationLevel;
        _content = content.ToArray();
        Sha256 = Convert.ToHexString(SHA256.HashData(_content));
    }

    public string RequirementId { get; }
    public string FileName { get; }
    public Ra2AutomationAssetKind Kind { get; }
    public int ContentLength => _content.Length;
    public string Sha256 { get; }
    public Ra2AutomationAssetVerificationLevel VerificationLevel { get; }

    public byte[] CopyContent() => _content.ToArray();

}

public sealed class Ra2AutomationAssetProviderResult
{
    private Ra2AutomationAssetProviderResult(
        Ra2AutomationAssetManifest manifest,
        Ra2AutomationAssetProviderDescriptor provider,
        Ra2AutomationAssetProviderFailureKind failureKind,
        string message,
        IEnumerable<Ra2AutomationAssetArtifact>? artifacts = null,
        IEnumerable<string>? relatedRequirementIds = null)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(provider);
        if (!Enum.IsDefined(failureKind))
            throw new ArgumentOutOfRangeException(nameof(failureKind));

        Ra2AutomationAssetArtifact[] artifactArray = (artifacts ?? []).ToArray();
        string[] relatedIds = (relatedRequirementIds ?? [])
            .Select(id => Ra2AutomationAssetContractValidation.ValidateText(
                id,
                Ra2AutomationAssetRequirement.MaximumRequirementIdLength,
                nameof(relatedRequirementIds)))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        bool succeeded = failureKind == Ra2AutomationAssetProviderFailureKind.None;
        if (succeeded != (artifactArray.Length == manifest.Requirements.Count && relatedIds.Length == 0))
            throw new ArgumentException("The provider result payload does not match its failure state.");
        if (!succeeded && artifactArray.Length != 0)
            throw new ArgumentException("A failed provider result cannot contain partial artifacts.", nameof(artifacts));
        if (succeeded)
        {
            if (manifest.Requirements.Any(requirement =>
                    requirement.Bindings.Any(binding => binding.State != Ra2AutomationAssetBindingState.Proposed) ||
                    !provider.SupportedKinds.Contains(requirement.Kind) ||
                    !Ra2AutomationAssetContractValidation.HasExpectedExtension(requirement.FileName, requirement.Kind)))
            {
                throw new ArgumentException("A successful provider result requires fully bound, supported manifest requirements.", nameof(manifest));
            }

            for (int index = 0; index < manifest.Requirements.Count; index++)
            {
                Ra2AutomationAssetRequirement requirement = manifest.Requirements[index];
                Ra2AutomationAssetArtifact artifact = artifactArray[index];
                if (!string.Equals(artifact.RequirementId, requirement.RequirementId, StringComparison.Ordinal) ||
                    !string.Equals(artifact.FileName, requirement.FileName, StringComparison.OrdinalIgnoreCase) ||
                    artifact.Kind != requirement.Kind)
                {
                    throw new ArgumentException("Successful artifacts must follow and exactly close the manifest requirements.", nameof(artifacts));
                }
            }
        }

        Succeeded = succeeded;
        FailureKind = failureKind;
        Message = Ra2AutomationAssetContractValidation.ValidateText(message, 1024, nameof(message));
        ProjectSessionId = manifest.ProjectSessionId;
        TemplateId = manifest.TemplateId;
        TemplateVersion = manifest.TemplateVersion;
        ProviderId = provider.Id;
        ProviderVersion = provider.Version;
        Artifacts = Array.AsReadOnly(artifactArray);
        RelatedRequirementIds = Array.AsReadOnly(relatedIds);
    }

    public bool Succeeded { get; }
    public Ra2AutomationAssetProviderFailureKind FailureKind { get; }
    public string Message { get; }
    public Guid ProjectSessionId { get; }
    public string TemplateId { get; }
    public int TemplateVersion { get; }
    public string ProviderId { get; }
    public int ProviderVersion { get; }
    public IReadOnlyList<Ra2AutomationAssetArtifact> Artifacts { get; }
    public IReadOnlyList<string> RelatedRequirementIds { get; }

    public static Ra2AutomationAssetProviderResult CreateSuccess(
        Ra2AutomationAssetManifest manifest,
        Ra2AutomationAssetProviderDescriptor provider,
        string message,
        IEnumerable<Ra2AutomationAssetArtifact> artifacts)
        => new(
            manifest,
            provider,
            Ra2AutomationAssetProviderFailureKind.None,
            message,
            artifacts);

    public static Ra2AutomationAssetProviderResult CreateFailure(
        Ra2AutomationAssetManifest manifest,
        Ra2AutomationAssetProviderDescriptor provider,
        Ra2AutomationAssetProviderFailureKind failureKind,
        string message,
        IEnumerable<string>? relatedRequirementIds = null)
    {
        if (failureKind == Ra2AutomationAssetProviderFailureKind.None)
            throw new ArgumentException("A failure result requires a non-success failure kind.", nameof(failureKind));

        return new(
            manifest,
            provider,
            failureKind,
            message,
            relatedRequirementIds: relatedRequirementIds);
    }
}

public interface IRa2AutomationAssetProvider
{
    Ra2AutomationAssetProviderDescriptor GetDescriptor();

    Ra2AutomationAssetProviderResult Resolve(
        Ra2AutomationAssetManifest manifest,
        IReadOnlyList<Ra2AutomationAssetSource> sources,
        CancellationToken cancellationToken = default);
}
