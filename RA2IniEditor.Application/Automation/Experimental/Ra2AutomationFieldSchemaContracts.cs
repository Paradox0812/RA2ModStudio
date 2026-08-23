using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Automation.Experimental;

public enum Ra2AutomationFieldSchemaQueryFailureKind
{
    None = 0,
    DocumentTooLarge,
    NotFound,
    ResultLimitExceeded,
    Canceled,
    AnalysisFailed
}

public enum Ra2AutomationFieldAuthoringDisposition
{
    Normal = 0,
    Caution,
    Blocked
}

public sealed class Ra2AutomationFieldSchemaQuery
{
    public const int MaximumKeyLength = 256;

    public Ra2AutomationFieldSchemaQuery(Ra2SectionKind sectionKind, string key)
    {
        if (!Enum.IsDefined(sectionKind))
            throw new ArgumentOutOfRangeException(nameof(sectionKind));
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("A field key is required.", nameof(key));

        string normalized = key.Trim();
        if (normalized.Length > MaximumKeyLength ||
            normalized.IndexOfAny(['\r', '\n', '\0', '=']) >= 0)
        {
            throw new ArgumentException("The field key is too long or contains unsupported characters.", nameof(key));
        }

        SectionKind = sectionKind;
        Key = normalized;
    }

    public Ra2SectionKind SectionKind { get; }
    public string Key { get; }
}

public sealed class Ra2AutomationFieldSchemaFact
{
    internal Ra2AutomationFieldSchemaFact(
        string key,
        Ra2SectionKind sectionKind,
        IReadOnlyCollection<Ra2SectionKind> appliesTo,
        FieldEditorKind editorKind,
        Ra2FieldValueKind valueKind,
        Ra2FieldBooleanValueStyle booleanStyle,
        IReadOnlyCollection<string> allowedValues,
        string? enumName,
        string separator,
        string? displayName,
        string? description,
        IReadOnlyCollection<string> aliases,
        Ra2FieldSourceKind sourceKind,
        Ra2AutomationFieldTrustLevel trustLevel,
        Ra2AutomationFieldAuthoringDisposition authoringDisposition)
    {
        Key = key;
        SectionKind = sectionKind;
        AppliesTo = Array.AsReadOnly(appliesTo.ToArray());
        EditorKind = editorKind;
        ValueKind = valueKind;
        BooleanStyle = booleanStyle;
        AllowedValues = Array.AsReadOnly(allowedValues.ToArray());
        EnumName = enumName;
        Separator = separator;
        DisplayName = displayName;
        Description = description;
        Aliases = Array.AsReadOnly(aliases.ToArray());
        SourceKind = sourceKind;
        TrustLevel = trustLevel;
        AuthoringDisposition = authoringDisposition;
    }

    public string Key { get; }
    public Ra2SectionKind SectionKind { get; }
    public IReadOnlyList<Ra2SectionKind> AppliesTo { get; }
    public FieldEditorKind EditorKind { get; }
    public Ra2FieldValueKind ValueKind { get; }
    public Ra2FieldBooleanValueStyle BooleanStyle { get; }
    public IReadOnlyList<string> AllowedValues { get; }
    public string? EnumName { get; }
    public string Separator { get; }
    public string? DisplayName { get; }
    public string? Description { get; }
    public IReadOnlyList<string> Aliases { get; }
    public Ra2FieldSourceKind SourceKind { get; }
    public Ra2AutomationFieldTrustLevel TrustLevel { get; }
    public Ra2AutomationFieldAuthoringDisposition AuthoringDisposition { get; }
}

public sealed class Ra2AutomationFieldSchemaQueryResult
{
    internal Ra2AutomationFieldSchemaQueryResult(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationFieldSchemaQueryFailureKind failureKind,
        string message,
        Ra2AutomationFieldSchemaFact? fact)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!Enum.IsDefined(failureKind))
            throw new ArgumentOutOfRangeException(nameof(failureKind));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("A result message is required.", nameof(message));
        if ((failureKind == Ra2AutomationFieldSchemaQueryFailureKind.None) != (fact is not null))
            throw new ArgumentException("Field schema result state is inconsistent.", nameof(fact));

        Succeeded = failureKind == Ra2AutomationFieldSchemaQueryFailureKind.None;
        FailureKind = failureKind;
        Message = message;
        DocumentId = snapshot.DocumentId;
        Version = snapshot.Version;
        FilePath = snapshot.FilePath;
        FieldRegistryRevision = snapshot.FieldRegistry.Revision;
        Fact = fact;
    }

    public bool Succeeded { get; }
    public Ra2AutomationFieldSchemaQueryFailureKind FailureKind { get; }
    public string Message { get; }
    public Guid DocumentId { get; }
    public int Version { get; }
    public string FilePath { get; }
    public long FieldRegistryRevision { get; }
    public Ra2AutomationFieldSchemaFact? Fact { get; }
}
