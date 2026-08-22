namespace RA2IniEditor.IDE.Search;

internal sealed record Ra2SearchMatch(
    string FileName,
    string FilePath,
    int LineNumber,
    int ColumnNumber,
    string SectionName,
    string Preview,
    string MatchedText,
    int CharacterIndex,
    int Length);
