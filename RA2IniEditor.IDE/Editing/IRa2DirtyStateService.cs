namespace RA2IniEditor.IDE.Editing;

internal interface IRa2DirtyStateService
{
    Ra2EditorDocumentState GetNextState(
        Ra2EditorDocumentState currentState,
        bool textChanged,
        bool saved);
}
