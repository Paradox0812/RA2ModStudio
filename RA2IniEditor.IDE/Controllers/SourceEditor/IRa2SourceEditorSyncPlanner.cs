namespace RA2IniEditor.IDE.Controllers.SourceEditor;

internal interface IRa2SourceEditorSyncPlanner
{
    int ClampCaretOffset(int caretOffset, int textLength);

    Ra2SourceEditorSyncPlan CreateTextSyncPlan(
        Ra2SourceEditorSyncOperationKind kind,
        string text,
        int? requestedCaretOffset = null,
        bool shouldSetReadOnly = false,
        bool shouldSetEditable = false);
}
