namespace RA2IniEditor.IDE.Language;

internal enum Ra2CaretRegion
{
    Unknown = 0,
    SectionHeader = 1,
    Key = 2,
    Value = 3,
    Comment = 4,
    Whitespace = 5
}
