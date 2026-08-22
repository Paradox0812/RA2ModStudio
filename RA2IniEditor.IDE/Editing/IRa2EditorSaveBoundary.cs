namespace RA2IniEditor.IDE.Editing;

internal interface IRa2EditorSaveBoundary
{
    bool CanSave(Ra2EditableDocumentState documentState);
}
