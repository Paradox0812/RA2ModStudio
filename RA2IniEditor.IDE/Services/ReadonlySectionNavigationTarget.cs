namespace RA2IniEditor.IDE.Services;

/// <summary>
/// Represents an exact readonly section header location in the current source text.
/// </summary>
public sealed class ReadonlySectionNavigationTarget
{
    public ReadonlySectionNavigationTarget(string sectionId, int oneBasedLineNumber, int characterIndex)
    {
        SectionId = sectionId;
        OneBasedLineNumber = oneBasedLineNumber;
        CharacterIndex = characterIndex;
    }

    public string SectionId { get; }

    public int OneBasedLineNumber { get; }

    public int CharacterIndex { get; }
}
