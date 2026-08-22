namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2DirtyStateService : IRa2DirtyStateService
{
    public Ra2EditorDocumentState GetNextState(
        Ra2EditorDocumentState currentState,
        bool textChanged,
        bool saved)
    {
        if (currentState == Ra2EditorDocumentState.ReadOnlyPreview)
            return Ra2EditorDocumentState.ReadOnlyPreview;

        if (currentState == Ra2EditorDocumentState.EditableDirty && saved && !textChanged)
            return Ra2EditorDocumentState.EditableClean;

        if (textChanged)
            return Ra2EditorDocumentState.EditableDirty;

        return currentState;
    }
}
