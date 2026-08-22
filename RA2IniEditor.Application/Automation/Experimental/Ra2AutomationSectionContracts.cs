using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Automation.Experimental;

public enum Ra2AutomationSectionQueryFailureKind
{
    None = 0,
    DocumentTooLarge = 1,
    NotFound = 2,
    AmbiguousSection = 3,
    ResultLimitExceeded = 4,
    Canceled = 5,
    AnalysisFailed = 6
}

public sealed class Ra2AutomationSectionQuery
{
    public Ra2AutomationSectionQuery(string sectionName, int? occurrence = null)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
            throw new ArgumentException("Section name is required.", nameof(sectionName));

        if (occurrence < 0)
            throw new ArgumentOutOfRangeException(nameof(occurrence));

        SectionName = sectionName.Trim();
        Occurrence = occurrence;
    }

    public string SectionName { get; }

    public int? Occurrence { get; }
}

public sealed class Ra2AutomationSectionQueryResult
{
    internal Ra2AutomationSectionQueryResult(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationSectionQueryFailureKind failureKind,
        string message,
        Ra2AutomationSectionFact? section)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("A result message is required.", nameof(message));

        bool succeeded = failureKind == Ra2AutomationSectionQueryFailureKind.None;
        if (succeeded != (section is not null))
            throw new ArgumentException("Section result payload does not match its failure state.", nameof(section));

        Succeeded = succeeded;
        FailureKind = failureKind;
        Message = message;
        DocumentId = snapshot.DocumentId;
        Version = snapshot.Version;
        FilePath = snapshot.FilePath;
        FieldRegistryRevision = snapshot.FieldRegistry.Revision;
        Section = section;
    }

    public bool Succeeded { get; }

    public Ra2AutomationSectionQueryFailureKind FailureKind { get; }

    public string Message { get; }

    public Guid DocumentId { get; }

    public int Version { get; }

    public string FilePath { get; }

    public long FieldRegistryRevision { get; }

    public Ra2AutomationSectionFact? Section { get; }
}

public sealed class Ra2AutomationSectionFact
{
    internal Ra2AutomationSectionFact(
        string name,
        Ra2SectionKind kind,
        int occurrence,
        int headerLineNumber,
        Ra2AutomationTextSpan headerSpan,
        Ra2AutomationTextSpan bodySpan,
        Ra2AutomationTextSpan fullSpan,
        IReadOnlyList<Ra2AutomationFieldFact> fields)
    {
        Name = name;
        Kind = kind;
        Occurrence = occurrence;
        HeaderLineNumber = headerLineNumber;
        HeaderSpan = headerSpan;
        BodySpan = bodySpan;
        FullSpan = fullSpan;
        Fields = Array.AsReadOnly((fields ?? throw new ArgumentNullException(nameof(fields))).ToArray());
    }

    public string Name { get; }

    public Ra2SectionKind Kind { get; }

    public int Occurrence { get; }

    public int HeaderLineNumber { get; }

    public Ra2AutomationTextSpan HeaderSpan { get; }

    public Ra2AutomationTextSpan BodySpan { get; }

    public Ra2AutomationTextSpan FullSpan { get; }

    public IReadOnlyList<Ra2AutomationFieldFact> Fields { get; }
}

public sealed class Ra2AutomationFieldFact
{
    internal Ra2AutomationFieldFact(
        string key,
        string effectiveValue,
        int lineNumber,
        Ra2AutomationTextSpan lineSpan,
        Ra2AutomationTextSpan keySpan,
        Ra2AutomationTextSpan? valueSpan)
    {
        Key = key;
        EffectiveValue = effectiveValue;
        LineNumber = lineNumber;
        LineSpan = lineSpan;
        KeySpan = keySpan;
        ValueSpan = valueSpan;
    }

    public string Key { get; }

    public string EffectiveValue { get; }

    public int LineNumber { get; }

    public Ra2AutomationTextSpan LineSpan { get; }

    public Ra2AutomationTextSpan KeySpan { get; }

    public Ra2AutomationTextSpan? ValueSpan { get; }
}
