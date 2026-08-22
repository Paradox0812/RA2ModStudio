using RA2IniEditor.IDE.Search;

namespace RA2IniEditor.IDE.ViewModels;

/// <summary>
/// Represents one navigable search result item.
/// </summary>
public sealed class SearchResultItemViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SearchResultItemViewModel"/> class.
    /// </summary>
    public SearchResultItemViewModel(string fileName, int lineNumber, string sectionName, string preview)
        : this(fileName, string.Empty, lineNumber, 1, sectionName, preview, string.Empty, 0, 0)
    {
    }

    internal SearchResultItemViewModel(Ra2SearchMatch match)
        : this(
            match.FileName,
            match.FilePath,
            match.LineNumber,
            match.ColumnNumber,
            match.SectionName,
            match.Preview,
            match.MatchedText,
            match.CharacterIndex,
            match.Length)
    {
    }

    private SearchResultItemViewModel(
        string fileName,
        string filePath,
        int lineNumber,
        int columnNumber,
        string sectionName,
        string preview,
        string matchedText,
        int characterIndex,
        int length)
    {
        FileName = fileName;
        FilePath = filePath;
        LineNumber = lineNumber;
        ColumnNumber = columnNumber;
        SectionName = sectionName;
        Preview = preview;
        MatchedText = matchedText;
        CharacterIndex = characterIndex;
        Length = length;
    }

    /// <summary>
    /// Gets the mock file name.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the mock line number.
    /// </summary>
    public int LineNumber { get; }

    /// <summary>
    /// Gets the mock section name.
    /// </summary>
    public string SectionName { get; }

    /// <summary>
    /// Gets the source-line preview for this result.
    /// </summary>
    public string Preview { get; }

    internal string FilePath { get; }

    internal string MatchedText { get; }

    internal int CharacterIndex { get; }

    internal int Length { get; }

    internal int ColumnNumber { get; }
}
