namespace RA2IniEditor.IDE.Controllers.SourceEditor;

internal enum Ra2SourceEditorSyncOperationKind
{
    LoadFile,
    Revert,
    CompletionCommit,
    AddPropertyInsert,
    AddPropertyReplace,
    ExternalReload
}

internal sealed class Ra2SourceEditorSyncPlan
{
    public Ra2SourceEditorSyncPlan(
        Ra2SourceEditorSyncOperationKind kind,
        string text,
        int? caretOffset = null,
        bool shouldSetReadOnly = false,
        bool shouldSetEditable = false)
    {
        if (shouldSetReadOnly && shouldSetEditable)
            throw new ArgumentException("A source editor sync plan cannot request readonly and editable state at the same time.");

        Kind = kind;
        Text = text ?? string.Empty;
        CaretOffset = caretOffset;
        ShouldSetReadOnly = shouldSetReadOnly;
        ShouldSetEditable = shouldSetEditable;
    }

    public Ra2SourceEditorSyncOperationKind Kind { get; }

    public string Text { get; }

    public int? CaretOffset { get; }

    public bool ShouldSetReadOnly { get; }

    public bool ShouldSetEditable { get; }
}
