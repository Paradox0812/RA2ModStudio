namespace RA2IniEditor.IDE.TextModel;

internal interface IRa2IniTextDocumentParser
{
    Ra2IniTextDocument Parse(string text);
}
