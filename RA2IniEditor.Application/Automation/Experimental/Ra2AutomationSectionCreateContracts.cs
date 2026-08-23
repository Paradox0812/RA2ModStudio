using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Automation.Experimental;

public sealed class Ra2AutomationSectionCreateOperation
{
    public Ra2AutomationSectionCreateOperation(string sectionName, Ra2SectionKind expectedSectionKind)
    {
        if (!Enum.IsDefined(expectedSectionKind))
            throw new ArgumentOutOfRangeException(nameof(expectedSectionKind));
        if (string.IsNullOrWhiteSpace(sectionName))
            throw new ArgumentException("A section name is required.", nameof(sectionName));

        string normalized = sectionName.Trim();
        if (normalized.Length > Ra2AutomationEditOperation.MaximumSectionNameLength ||
            normalized.IndexOfAny(['\r', '\n', '\0', '=', '[', ']']) >= 0)
        {
            throw new ArgumentException("The section name is too long or contains unsupported characters.", nameof(sectionName));
        }

        SectionName = normalized;
        ExpectedSectionKind = expectedSectionKind;
    }

    public string SectionName { get; }
    public Ra2SectionKind ExpectedSectionKind { get; }
}

public sealed class Ra2AutomationSectionCreatePreview
{
    internal Ra2AutomationSectionCreatePreview(
        int operationIndex,
        Ra2AutomationSectionCreateOperation operation,
        Ra2SectionKind actualSectionKind,
        bool isClassificationResolved,
        Ra2AutomationFieldAuthoringDisposition authoringDisposition,
        Ra2AutomationTextSpan affectedOriginalSpan,
        string summary)
    {
        if (operationIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(operationIndex));
        ArgumentNullException.ThrowIfNull(operation);
        if (!Enum.IsDefined(actualSectionKind))
            throw new ArgumentOutOfRangeException(nameof(actualSectionKind));
        if (!Enum.IsDefined(authoringDisposition))
            throw new ArgumentOutOfRangeException(nameof(authoringDisposition));
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("A section creation preview summary is required.", nameof(summary));

        OperationIndex = operationIndex;
        Operation = operation;
        ActualSectionKind = actualSectionKind;
        IsClassificationResolved = isClassificationResolved;
        AuthoringDisposition = authoringDisposition;
        AffectedOriginalSpan = affectedOriginalSpan;
        Summary = summary;
    }

    public int OperationIndex { get; }
    public Ra2AutomationSectionCreateOperation Operation { get; }
    public Ra2SectionKind ActualSectionKind { get; }
    public bool IsClassificationResolved { get; }
    public Ra2AutomationFieldAuthoringDisposition AuthoringDisposition { get; }
    public Ra2AutomationTextSpan AffectedOriginalSpan { get; }
    public string Summary { get; }
}
