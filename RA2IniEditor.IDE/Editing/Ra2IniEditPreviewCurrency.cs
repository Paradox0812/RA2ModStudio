namespace RA2IniEditor.IDE.Editing;

internal enum Ra2IniEditPreviewCurrencyKind
{
    Current = 0,
    PreviewFailed,
    NoEditableSession,
    ReadOnly,
    DocumentIdentityChanged,
    EditRevisionChanged,
    SessionTextChanged,
    EditorTextChanged,
    FieldRegistryChanged
}

internal sealed class Ra2IniEditPreviewCurrencyResult
{
    private Ra2IniEditPreviewCurrencyResult(
        Ra2IniEditPreviewCurrencyKind kind,
        string message)
    {
        Kind = kind;
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Currency result message cannot be empty.", nameof(message))
            : message;
    }

    public bool IsCurrent => Kind == Ra2IniEditPreviewCurrencyKind.Current;

    public Ra2IniEditPreviewCurrencyKind Kind { get; }

    public string Message { get; }

    public static Ra2IniEditPreviewCurrencyResult Valid()
        => new(Ra2IniEditPreviewCurrencyKind.Current, "编辑预览仍与当前文档一致。");

    public static Ra2IniEditPreviewCurrencyResult Stale(
        Ra2IniEditPreviewCurrencyKind kind,
        string message)
    {
        if (kind == Ra2IniEditPreviewCurrencyKind.Current)
            throw new ArgumentOutOfRangeException(nameof(kind));

        return new Ra2IniEditPreviewCurrencyResult(kind, message);
    }
}

/// <summary>
/// 只判断预览是否仍有效，不持有预览，也不应用文本。
/// </summary>
internal sealed class Ra2IniEditPreviewCurrencyEvaluator
{
    public Ra2IniEditPreviewCurrencyResult Evaluate(
        Ra2IniEditPreview preview,
        Ra2EditableDocumentSession? currentSession,
        string? currentEditorText,
        long currentFieldRegistryRevision)
    {
        ArgumentNullException.ThrowIfNull(preview);

        if (!preview.Succeeded)
        {
            return Stale(
                Ra2IniEditPreviewCurrencyKind.PreviewFailed,
                "失败的编辑预览不能进入应用阶段。");
        }

        if (currentSession is null)
        {
            return Stale(
                Ra2IniEditPreviewCurrencyKind.NoEditableSession,
                "当前没有可编辑会话。");
        }

        if (currentSession.DocumentState.State == Ra2EditorDocumentState.ReadOnlyPreview)
        {
            return Stale(
                Ra2IniEditPreviewCurrencyKind.ReadOnly,
                "当前文档处于只读预览状态。");
        }

        if (currentSession.DocumentId != preview.Snapshot.DocumentId)
        {
            return Stale(
                Ra2IniEditPreviewCurrencyKind.DocumentIdentityChanged,
                "当前编辑文档已经切换。");
        }

        if (currentSession.EditRevision != preview.Snapshot.EditRevision)
        {
            return Stale(
                Ra2IniEditPreviewCurrencyKind.EditRevisionChanged,
                "当前文档在预览后已发生编辑。");
        }

        if (!string.Equals(
                currentSession.DocumentState.CurrentText,
                preview.Snapshot.Text,
                StringComparison.Ordinal))
        {
            return Stale(
                Ra2IniEditPreviewCurrencyKind.SessionTextChanged,
                "当前编辑会话文本与预览原文不一致。");
        }

        if (!string.Equals(currentEditorText, preview.Snapshot.Text, StringComparison.Ordinal))
        {
            return Stale(
                Ra2IniEditPreviewCurrencyKind.EditorTextChanged,
                "当前编辑器文本与预览原文不一致。");
        }

        if (currentFieldRegistryRevision != preview.Snapshot.FieldRegistry.Revision)
        {
            return Stale(
                Ra2IniEditPreviewCurrencyKind.FieldRegistryChanged,
                "字段库已在预览后重新加载。");
        }

        return Ra2IniEditPreviewCurrencyResult.Valid();
    }

    private static Ra2IniEditPreviewCurrencyResult Stale(
        Ra2IniEditPreviewCurrencyKind kind,
        string message)
        => Ra2IniEditPreviewCurrencyResult.Stale(kind, message);
}
