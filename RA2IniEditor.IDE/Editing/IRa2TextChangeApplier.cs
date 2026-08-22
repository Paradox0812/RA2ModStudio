namespace RA2IniEditor.IDE.Editing;

internal interface IRa2TextChangeApplier
{
    Ra2TextChangeApplyResult Apply(Ra2EditableDocumentState documentState, Ra2TextChange change);
}
