namespace RA2IniEditor.IDE.Editing;

internal interface IRa2EditorSavePlanBuilder
{
    Ra2EditorSavePlan Build(Ra2EditableDocumentSession session);
}
