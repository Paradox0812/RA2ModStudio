namespace RA2IniEditor.IDE.Services;

/// <summary>
/// Represents one readonly Project Explorer section classification result for the current file.
/// </summary>
public sealed class ReadonlySectionClassificationResult
{
    public ReadonlySectionClassificationResult(
        string sectionId,
        int lineNumber,
        string? displayName,
        string typeGroup,
        string? factionGroup)
    {
        SectionId = sectionId;
        LineNumber = lineNumber;
        DisplayName = displayName;
        TypeGroup = typeGroup;
        FactionGroup = factionGroup;
    }

    public string SectionId { get; }

    public int LineNumber { get; }

    public string? DisplayName { get; }

    public string TypeGroup { get; }

    public string? FactionGroup { get; }
}
