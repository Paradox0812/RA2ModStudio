namespace RA2IniEditor.IDE.Language;

internal interface IRa2CaretContextService
{
    Ra2CaretContext GetContext(Ra2DocumentSemanticModel model, int offset);
}
