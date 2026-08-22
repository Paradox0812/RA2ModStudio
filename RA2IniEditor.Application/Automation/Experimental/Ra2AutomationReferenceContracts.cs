using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Automation.Experimental;

public enum Ra2AutomationReferenceQueryFailureKind
{
    None = 0,
    DocumentTooLarge = 1,
    InvalidLocation = 2,
    TargetNotResolved = 3,
    ResultLimitExceeded = 4,
    Canceled = 5,
    AnalysisFailed = 6
}

public sealed class Ra2AutomationReferenceQuery
{
    public Ra2AutomationReferenceQuery(
        int sourceOffset,
        Ra2AutomationTextSpan? selectionSpan = null)
    {
        if (sourceOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(sourceOffset));

        if (selectionSpan is Ra2AutomationTextSpan selected && selected.Length <= 0)
            throw new ArgumentOutOfRangeException(nameof(selectionSpan));

        SourceOffset = sourceOffset;
        SelectionSpan = selectionSpan;
    }

    public int SourceOffset { get; }

    public Ra2AutomationTextSpan? SelectionSpan { get; }
}

public sealed class Ra2AutomationReferenceQueryResult
{
    internal Ra2AutomationReferenceQueryResult(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationReferenceQueryFailureKind failureKind,
        string message,
        Ra2AutomationReferenceTargetFact? target,
        IReadOnlyList<Ra2AutomationReferenceFact> references)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("A result message is required.", nameof(message));

        ArgumentNullException.ThrowIfNull(references);
        bool succeeded = failureKind == Ra2AutomationReferenceQueryFailureKind.None;
        if (succeeded != (target is not null))
            throw new ArgumentException("Reference result target does not match its failure state.", nameof(target));

        if (!succeeded && references.Count != 0)
            throw new ArgumentException("Failure results cannot contain references.", nameof(references));

        Succeeded = succeeded;
        FailureKind = failureKind;
        Message = message;
        DocumentId = snapshot.DocumentId;
        Version = snapshot.Version;
        FilePath = snapshot.FilePath;
        FieldRegistryRevision = snapshot.FieldRegistry.Revision;
        Target = target;
        References = Array.AsReadOnly(references.ToArray());
        HasReferences = References.Count > 0;
    }

    public bool Succeeded { get; }

    public Ra2AutomationReferenceQueryFailureKind FailureKind { get; }

    public string Message { get; }

    public Guid DocumentId { get; }

    public int Version { get; }

    public string FilePath { get; }

    public long FieldRegistryRevision { get; }

    public Ra2AutomationReferenceTargetFact? Target { get; }

    public IReadOnlyList<Ra2AutomationReferenceFact> References { get; }

    public bool HasReferences { get; }
}

public sealed class Ra2AutomationReferenceTargetFact
{
    internal Ra2AutomationReferenceTargetFact(string name, Ra2SectionKind kind)
    {
        Name = name;
        Kind = kind;
    }

    public string Name { get; }

    public Ra2SectionKind Kind { get; }
}

public sealed class Ra2AutomationReferenceFact
{
    internal Ra2AutomationReferenceFact(
        string sourceSectionName,
        string sourceKey,
        int lineNumber,
        Ra2AutomationTextSpan lineSpan,
        Ra2AutomationTextSpan valueSpan)
    {
        SourceSectionName = sourceSectionName;
        SourceKey = sourceKey;
        LineNumber = lineNumber;
        LineSpan = lineSpan;
        ValueSpan = valueSpan;
    }

    public string SourceSectionName { get; }

    public string SourceKey { get; }

    public int LineNumber { get; }

    public Ra2AutomationTextSpan LineSpan { get; }

    public Ra2AutomationTextSpan ValueSpan { get; }
}
