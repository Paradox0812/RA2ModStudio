namespace RA2IniEditor.IDE.TextModel;

internal enum Ra2IniDocumentLineKind
{
    Blank = 0,
    Comment = 1,
    SectionHeader = 2,
    KeyValue = 3,
    Raw = 4
}
