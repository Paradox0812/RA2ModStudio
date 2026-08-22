namespace RA2IniEditor.Application.Language;

internal interface IRa2CaretContextService
{
    Ra2CaretContext GetContext(Ra2DocumentSemanticModel model, int offset);
}
