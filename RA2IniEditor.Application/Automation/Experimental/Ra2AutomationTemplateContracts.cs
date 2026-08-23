namespace RA2IniEditor.Application.Automation.Experimental;

public enum Ra2AutomationTemplateParameterKind
{
    Identifier = 0,
    String,
    Integer,
    Float,
    Boolean,
    Reference
}

public enum Ra2AutomationTemplateExpansionFailureKind
{
    None = 0,
    TemplateNotFound,
    TemplateVersionMismatch,
    InvalidArguments,
    MissingRequiredArgument,
    UnknownArgument,
    DuplicateArgument,
    FieldSchemaNotFound,
    BlockedFieldTrust,
    OperationLimitExceeded,
    DocumentTooLarge,
    Canceled,
    ExpansionFailed,
    RequiredSectionNotFound,
    RequiredSectionKindMismatch
}

public enum Ra2AutomationTemplateOutputKind
{
    Skeleton = 0,
    CompleteObject
}

public enum Ra2AutomationTemplateWarningKind
{
    FieldTrustCaution = 0
}

public sealed class Ra2AutomationTemplateParameterDescriptor
{
    internal Ra2AutomationTemplateParameterDescriptor(
        string name,
        Ra2AutomationTemplateParameterKind kind,
        bool required,
        string? defaultValue)
    {
        Name = name;
        Kind = kind;
        Required = required;
        DefaultValue = defaultValue;
    }

    public string Name { get; }
    public Ra2AutomationTemplateParameterKind Kind { get; }
    public bool Required { get; }
    public string? DefaultValue { get; }
}

public sealed class Ra2AutomationTemplateDescriptor
{
    internal Ra2AutomationTemplateDescriptor(
        string id,
        int version,
        string displayName,
        string summary,
        Ra2AutomationTemplateOutputKind outputKind,
        IEnumerable<Ra2AutomationTemplateParameterDescriptor> parameters)
    {
        Id = id;
        Version = version;
        DisplayName = displayName;
        Summary = summary;
        OutputKind = outputKind;
        Parameters = Array.AsReadOnly(parameters.ToArray());
    }

    public string Id { get; }
    public int Version { get; }
    public string DisplayName { get; }
    public string Summary { get; }
    public Ra2AutomationTemplateOutputKind OutputKind { get; }
    public IReadOnlyList<Ra2AutomationTemplateParameterDescriptor> Parameters { get; }
}

public sealed class Ra2AutomationTemplateArgument
{
    public const int MaximumNameLength = 256;
    public const int MaximumValueLength = Ra2AutomationEditOperation.MaximumValueLength;

    public Ra2AutomationTemplateArgument(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("A template argument name is required.", nameof(name));
        if (name.Length > MaximumNameLength || name.IndexOfAny(['\r', '\n', '\0']) >= 0)
            throw new ArgumentException("The template argument name exceeds the supported limit.", nameof(name));
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length > MaximumValueLength || value.IndexOfAny(['\r', '\n', '\0']) >= 0)
            throw new ArgumentException("The template argument value exceeds the supported limit.", nameof(value));

        Name = name;
        Value = value;
    }

    public string Name { get; }
    public string Value { get; }
}

public sealed class Ra2AutomationTemplateExpansionRequest
{
    public const int MaximumTemplateIdLength = 128;
    public const int MaximumArgumentCount = 64;

    public Ra2AutomationTemplateExpansionRequest(
        string templateId,
        int templateVersion,
        IEnumerable<Ra2AutomationTemplateArgument> arguments)
    {
        if (string.IsNullOrWhiteSpace(templateId))
            throw new ArgumentException("A template identity is required.", nameof(templateId));
        string normalizedId = templateId.Trim();
        if (normalizedId.Length > MaximumTemplateIdLength || normalizedId.IndexOfAny(['\r', '\n', '\0']) >= 0)
            throw new ArgumentException("The template identity exceeds the supported limit.", nameof(templateId));
        if (templateVersion <= 0)
            throw new ArgumentOutOfRangeException(nameof(templateVersion));
        ArgumentNullException.ThrowIfNull(arguments);

        Ra2AutomationTemplateArgument[] argumentArray = arguments.ToArray();
        if (argumentArray.Length > MaximumArgumentCount)
            throw new ArgumentOutOfRangeException(nameof(arguments));
        if (argumentArray.Any(argument => argument is null))
            throw new ArgumentException("Template arguments cannot contain null entries.", nameof(arguments));

        TemplateId = normalizedId;
        TemplateVersion = templateVersion;
        Arguments = Array.AsReadOnly(argumentArray);
    }

    public string TemplateId { get; }
    public int TemplateVersion { get; }
    public IReadOnlyList<Ra2AutomationTemplateArgument> Arguments { get; }
}

public sealed class Ra2AutomationTemplateWarningFact
{
    internal Ra2AutomationTemplateWarningFact(
        Ra2AutomationTemplateWarningKind kind,
        string sectionName,
        string key,
        Ra2AutomationFieldTrustLevel trustLevel,
        string message)
    {
        Kind = kind;
        SectionName = sectionName;
        Key = key;
        TrustLevel = trustLevel;
        Message = message;
    }

    public Ra2AutomationTemplateWarningKind Kind { get; }
    public string SectionName { get; }
    public string Key { get; }
    public Ra2AutomationFieldTrustLevel TrustLevel { get; }
    public string Message { get; }
}

public sealed class Ra2AutomationTemplateExpansionResult
{
    internal Ra2AutomationTemplateExpansionResult(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationTemplateExpansionFailureKind failureKind,
        string message,
        Ra2AutomationEditPlan? plan,
        IEnumerable<Ra2AutomationTemplateWarningFact>? warnings = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!Enum.IsDefined(failureKind))
            throw new ArgumentOutOfRangeException(nameof(failureKind));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("A template expansion result message is required.", nameof(message));
        if ((failureKind == Ra2AutomationTemplateExpansionFailureKind.None) != (plan is not null))
            throw new ArgumentException("The template expansion payload does not match its failure state.", nameof(plan));

        Ra2AutomationTemplateWarningFact[] warningArray = (warnings ?? []).ToArray();
        if (failureKind != Ra2AutomationTemplateExpansionFailureKind.None && warningArray.Length != 0)
            throw new ArgumentException("A failed template expansion cannot contain partial warnings.", nameof(warnings));

        Succeeded = failureKind == Ra2AutomationTemplateExpansionFailureKind.None;
        FailureKind = failureKind;
        Message = message;
        DocumentId = snapshot.DocumentId;
        Version = snapshot.Version;
        FilePath = snapshot.FilePath;
        FieldRegistryRevision = snapshot.FieldRegistry.Revision;
        Plan = plan;
        Warnings = Array.AsReadOnly(warningArray);
    }

    public bool Succeeded { get; }
    public Ra2AutomationTemplateExpansionFailureKind FailureKind { get; }
    public string Message { get; }
    public Guid DocumentId { get; }
    public int Version { get; }
    public string FilePath { get; }
    public long FieldRegistryRevision { get; }
    public Ra2AutomationEditPlan? Plan { get; }
    public IReadOnlyList<Ra2AutomationTemplateWarningFact> Warnings { get; }
}
