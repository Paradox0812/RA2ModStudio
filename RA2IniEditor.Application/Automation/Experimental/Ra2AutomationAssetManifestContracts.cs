namespace RA2IniEditor.Application.Automation.Experimental;

public enum Ra2AutomationAssetKind
{
    ShpAnimation = 0,
    Cameo,
    VxlModel,
    HvaAnimation
}

public enum Ra2AutomationAssetBindingState
{
    Proposed = 0,
    PendingSchema
}

public sealed class Ra2AutomationAssetBindingFact
{
    internal Ra2AutomationAssetBindingFact(
        Guid documentId,
        string filePath,
        string sectionName,
        string key,
        string value,
        Ra2AutomationAssetBindingState state)
    {
        if (documentId == Guid.Empty)
            throw new ArgumentException("Asset binding document identity cannot be empty.", nameof(documentId));
        if (!Enum.IsDefined(state))
            throw new ArgumentOutOfRangeException(nameof(state));

        DocumentId = documentId;
        FilePath = ValidateText(filePath, 4096, nameof(filePath));
        SectionName = ValidateText(sectionName, Ra2AutomationEditOperation.MaximumSectionNameLength, nameof(sectionName));
        Key = ValidateText(key, Ra2AutomationEditOperation.MaximumKeyLength, nameof(key));
        Value = ValidateText(value, Ra2AutomationEditOperation.MaximumValueLength, nameof(value));
        State = state;
    }

    public Guid DocumentId { get; }
    public string FilePath { get; }
    public string SectionName { get; }
    public string Key { get; }
    public string Value { get; }
    public Ra2AutomationAssetBindingState State { get; }

    private static string ValidateText(string value, int maximumLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Asset binding text cannot be empty.", parameterName);
        string normalized = value.Trim();
        if (normalized.Length > maximumLength || normalized.IndexOfAny(['\r', '\n', '\0']) >= 0)
            throw new ArgumentException("Asset binding text is invalid or exceeds its limit.", parameterName);
        return normalized;
    }
}

public sealed class Ra2AutomationAssetRequirement
{
    public const int MaximumRequirementIdLength = 128;
    public const int MaximumFileNameLength = 260;
    public const int MaximumBriefLength = 4096;
    public const int MaximumBindingCount = 8;

    internal Ra2AutomationAssetRequirement(
        string requirementId,
        string fileName,
        Ra2AutomationAssetKind kind,
        string generationBrief,
        int? width,
        int? height,
        string? palette,
        IEnumerable<Ra2AutomationAssetBindingFact> bindings)
    {
        if (!Enum.IsDefined(kind))
            throw new ArgumentOutOfRangeException(nameof(kind));
        RequirementId = Ra2AutomationAssetContractValidation.ValidateText(requirementId, MaximumRequirementIdLength, nameof(requirementId));
        FileName = Ra2AutomationAssetContractValidation.ValidateFileName(fileName, nameof(fileName));
        GenerationBrief = Ra2AutomationAssetContractValidation.ValidateText(generationBrief, MaximumBriefLength, nameof(generationBrief));
        if ((width is null) != (height is null) || width is <= 0 or > 8192 || height is <= 0 or > 8192)
            throw new ArgumentOutOfRangeException(nameof(width), "Asset dimensions must be absent together or both be within 1..8192.");
        if (palette is not null)
            palette = Ra2AutomationAssetContractValidation.ValidateFileName(palette, nameof(palette));

        ArgumentNullException.ThrowIfNull(bindings);
        Ra2AutomationAssetBindingFact[] bindingArray = bindings.ToArray();
        if (bindingArray.Length is < 1 or > MaximumBindingCount || bindingArray.Any(binding => binding is null))
            throw new ArgumentOutOfRangeException(nameof(bindings));
        if (bindingArray
            .GroupBy(binding => $"{binding.DocumentId:N}|{binding.SectionName}|{binding.Key}", StringComparer.OrdinalIgnoreCase)
            .Any(group => group.Count() > 1))
        {
            throw new ArgumentException("Asset bindings must be unique per document, section and key.", nameof(bindings));
        }

        Kind = kind;
        Width = width;
        Height = height;
        Palette = palette;
        Bindings = Array.AsReadOnly(bindingArray);
    }

    public string RequirementId { get; }
    public string FileName { get; }
    public Ra2AutomationAssetKind Kind { get; }
    public string GenerationBrief { get; }
    public int? Width { get; }
    public int? Height { get; }
    public string? Palette { get; }
    public IReadOnlyList<Ra2AutomationAssetBindingFact> Bindings { get; }

}

public sealed class Ra2AutomationAssetManifest
{
    public const int MaximumRequirementCount = 32;

    internal Ra2AutomationAssetManifest(
        Guid projectSessionId,
        string templateId,
        int templateVersion,
        IEnumerable<Ra2AutomationAssetRequirement> requirements)
    {
        if (projectSessionId == Guid.Empty)
            throw new ArgumentException("Project session identity cannot be empty.", nameof(projectSessionId));
        if (string.IsNullOrWhiteSpace(templateId))
            throw new ArgumentException("Manifest template identity cannot be empty.", nameof(templateId));
        if (templateVersion <= 0)
            throw new ArgumentOutOfRangeException(nameof(templateVersion));
        ArgumentNullException.ThrowIfNull(requirements);

        Ra2AutomationAssetRequirement[] requirementArray = requirements.ToArray();
        if (requirementArray.Length is < 1 or > MaximumRequirementCount || requirementArray.Any(item => item is null))
            throw new ArgumentOutOfRangeException(nameof(requirements));
        if (requirementArray.GroupBy(item => item.RequirementId, StringComparer.Ordinal).Any(group => group.Count() > 1) ||
            requirementArray.GroupBy(item => item.FileName, StringComparer.OrdinalIgnoreCase).Any(group => group.Count() > 1))
        {
            throw new ArgumentException("Manifest requirement identities and file names must be unique.", nameof(requirements));
        }

        ProjectSessionId = projectSessionId;
        TemplateId = templateId.Trim();
        TemplateVersion = templateVersion;
        Requirements = Array.AsReadOnly(requirementArray);
    }

    public Guid ProjectSessionId { get; }
    public string TemplateId { get; }
    public int TemplateVersion { get; }
    public IReadOnlyList<Ra2AutomationAssetRequirement> Requirements { get; }
}

public sealed class Ra2AutomationProjectTemplateExpansionResult
{
    internal Ra2AutomationProjectTemplateExpansionResult(
        Ra2AutomationProjectSnapshot snapshot,
        Ra2AutomationTemplateExpansionFailureKind failureKind,
        string message,
        Ra2AutomationProjectEditPlan? plan,
        Ra2AutomationAssetManifest? assetManifest,
        IEnumerable<Ra2AutomationTemplateWarningFact>? warnings = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!Enum.IsDefined(failureKind))
            throw new ArgumentOutOfRangeException(nameof(failureKind));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("A project template expansion result message is required.", nameof(message));

        bool succeeded = failureKind == Ra2AutomationTemplateExpansionFailureKind.None;
        if (succeeded != (plan is not null && assetManifest is not null))
            throw new ArgumentException("The project template payload does not match its failure state.");
        if (succeeded &&
            (plan!.ExpectedProjectSessionId != snapshot.ProjectSessionId ||
             plan.ExpectedProjectRevision != snapshot.ProjectRevision ||
             assetManifest!.ProjectSessionId != snapshot.ProjectSessionId))
        {
            throw new ArgumentException("The project template payload does not match its snapshot.");
        }

        Ra2AutomationTemplateWarningFact[] warningArray = (warnings ?? []).ToArray();
        if (!succeeded && warningArray.Length != 0)
            throw new ArgumentException("A failed project expansion cannot contain partial warnings.", nameof(warnings));

        Succeeded = succeeded;
        FailureKind = failureKind;
        Message = message.Trim();
        ProjectSessionId = snapshot.ProjectSessionId;
        ProjectRevision = snapshot.ProjectRevision;
        ProjectRootPath = snapshot.ProjectRootPath;
        FieldRegistryRevision = snapshot.Documents[0].FieldRegistry.Revision;
        Plan = plan;
        AssetManifest = assetManifest;
        Warnings = Array.AsReadOnly(warningArray);
    }

    public bool Succeeded { get; }
    public Ra2AutomationTemplateExpansionFailureKind FailureKind { get; }
    public string Message { get; }
    public Guid ProjectSessionId { get; }
    public long ProjectRevision { get; }
    public string ProjectRootPath { get; }
    public long FieldRegistryRevision { get; }
    public Ra2AutomationProjectEditPlan? Plan { get; }
    public Ra2AutomationAssetManifest? AssetManifest { get; }
    public IReadOnlyList<Ra2AutomationTemplateWarningFact> Warnings { get; }
}
