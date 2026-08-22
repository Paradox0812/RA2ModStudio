namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2EditorSaveBoundary : IRa2EditorSaveBoundary
{
    public bool CanSave(Ra2EditableDocumentState documentState)
    {
        ArgumentNullException.ThrowIfNull(documentState);

        return documentState.State == Ra2EditorDocumentState.EditableDirty;
    }
}
