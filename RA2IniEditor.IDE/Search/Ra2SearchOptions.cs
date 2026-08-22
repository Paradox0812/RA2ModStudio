namespace RA2IniEditor.IDE.Search;

internal enum Ra2SearchScope
{
    Project,
    CurrentFile
}

internal enum Ra2SearchFailureKind
{
    None,
    EmptyQuery,
    InvalidPattern,
    InvalidRegex,
    RegexTimeout,
    NoFiles,
    Canceled,
    Unexpected
}

internal sealed record Ra2SearchOptions(
    string Query,
    Ra2SearchScope Scope,
    bool IsCaseSensitive,
    bool IsWholeWord,
    bool UseRegex,
    string FilePattern);
