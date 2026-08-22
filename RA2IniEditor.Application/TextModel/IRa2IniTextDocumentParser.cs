namespace RA2IniEditor.Application.TextModel;

internal interface IRa2IniTextDocumentParser
{
    Ra2IniTextDocument Parse(string text);
}
