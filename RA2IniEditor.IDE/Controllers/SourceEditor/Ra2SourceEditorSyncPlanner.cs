namespace RA2IniEditor.IDE.Controllers.SourceEditor;

internal sealed class Ra2SourceEditorSyncPlanner : IRa2SourceEditorSyncPlanner
{
    public int ClampCaretOffset(int caretOffset, int textLength)
    {
        if (textLength < 0)
            throw new ArgumentOutOfRangeException(nameof(textLength), "Text length cannot be negative.");

        return Math.Clamp(caretOffset, 0, textLength);
    }

    public Ra2SourceEditorSyncPlan CreateTextSyncPlan(
        Ra2SourceEditorSyncOperationKind kind,
        string text,
        int? requestedCaretOffset = null,
        bool shouldSetReadOnly = false,
        bool shouldSetEditable = false)
    {
        string normalizedText = text ?? string.Empty;
        int? normalizedCaretOffset = requestedCaretOffset is int offset
            ? ClampCaretOffset(offset, normalizedText.Length)
            : null;

        return new Ra2SourceEditorSyncPlan(
            kind,
            normalizedText,
            normalizedCaretOffset,
            shouldSetReadOnly,
            shouldSetEditable);
    }
}
