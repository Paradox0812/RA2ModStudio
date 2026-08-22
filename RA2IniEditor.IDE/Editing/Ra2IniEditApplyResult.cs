namespace RA2IniEditor.IDE.Editing;

internal enum Ra2IniEditApplyOutcomeKind
{
    Applied = 0,
    PreviewUnavailable,
    ConfirmationRequired,
    StalePreview,
    TransactionRejected,
    UnexpectedFailure
}

internal sealed class Ra2IniEditApplyRequest
{
    public Ra2IniEditApplyRequest(
        Guid previewId,
        bool explicitConfirmationGranted)
    {
        PreviewId = previewId;
        ExplicitConfirmationGranted = explicitConfirmationGranted;
    }

    public Guid PreviewId { get; }

    public bool ExplicitConfirmationGranted { get; }
}

/// <summary>
/// 表示一次已确认结构化编辑的最终提交结果。
/// </summary>
internal sealed class Ra2IniEditApplyResult
{
    private Ra2IniEditApplyResult(
        Ra2IniEditApplyOutcomeKind outcomeKind,
        Ra2IniEditPreviewCurrencyKind currencyKind,
        Guid previewId,
        Ra2EditableDocumentSession? updatedSession,
        string? textToSyncToEditor,
        string? undoText,
        string? redoText,
        int? undoCaretOffset,
        int? redoCaretOffset,
        int operationCount,
        bool isDirtyAfterApply,
        string message)
    {
        if (!Enum.IsDefined(outcomeKind))
            throw new ArgumentOutOfRangeException(nameof(outcomeKind));
        if (!Enum.IsDefined(currencyKind))
            throw new ArgumentOutOfRangeException(nameof(currencyKind));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Apply result message cannot be empty.", nameof(message));

        bool succeeded = outcomeKind == Ra2IniEditApplyOutcomeKind.Applied;
        bool hasApplyEvidence =
            updatedSession is not null &&
            textToSyncToEditor is not null &&
            undoText is not null &&
            redoText is not null &&
            undoCaretOffset is not null &&
            redoCaretOffset is not null &&
            operationCount > 0;
        if (succeeded != hasApplyEvidence)
            throw new ArgumentException("Apply result evidence is inconsistent.");
        if (succeeded && currencyKind != Ra2IniEditPreviewCurrencyKind.Current)
            throw new ArgumentException("Successful apply result must be current.");
        if (!succeeded &&
            (updatedSession is not null ||
             textToSyncToEditor is not null ||
             undoText is not null ||
             redoText is not null ||
             undoCaretOffset is not null ||
             redoCaretOffset is not null ||
             operationCount != 0 ||
             isDirtyAfterApply))
        {
            throw new ArgumentException("Failed apply result cannot carry commit evidence.");
        }

        OutcomeKind = outcomeKind;
        CurrencyKind = currencyKind;
        PreviewId = previewId;
        UpdatedSession = updatedSession;
        TextToSyncToEditor = textToSyncToEditor;
        UndoText = undoText;
        RedoText = redoText;
        UndoCaretOffset = undoCaretOffset;
        RedoCaretOffset = redoCaretOffset;
        OperationCount = operationCount;
        IsDirtyAfterApply = isDirtyAfterApply;
        Message = message;
    }

    public bool Succeeded => OutcomeKind == Ra2IniEditApplyOutcomeKind.Applied;

    public Ra2IniEditApplyOutcomeKind OutcomeKind { get; }

    public Ra2IniEditPreviewCurrencyKind CurrencyKind { get; }

    public Guid PreviewId { get; }

    public Ra2EditableDocumentSession? UpdatedSession { get; }

    public string? TextToSyncToEditor { get; }

    public string? UndoText { get; }

    public string? RedoText { get; }

    public int? UndoCaretOffset { get; }

    public int? RedoCaretOffset { get; }

    public int OperationCount { get; }

    public bool IsDirtyAfterApply { get; }

    public string Message { get; }

    public static Ra2IniEditApplyResult Applied(
        Ra2IniEditPreview preview,
        Ra2EditableDocumentSession updatedSession,
        int undoCaretOffset,
        int redoCaretOffset)
    {
        ArgumentNullException.ThrowIfNull(preview);
        ArgumentNullException.ThrowIfNull(updatedSession);
        if (!preview.Succeeded || preview.CandidateText is null)
            throw new ArgumentException("Only a successful preview can be applied.", nameof(preview));
        if (updatedSession.DocumentId != preview.Snapshot.DocumentId ||
            updatedSession.EditRevision != preview.Snapshot.EditRevision + 1 ||
            !string.Equals(
                updatedSession.DocumentState.CurrentText,
                preview.CandidateText,
                StringComparison.Ordinal))
        {
            throw new ArgumentException("Updated session does not match the preview transaction.", nameof(updatedSession));
        }

        return new Ra2IniEditApplyResult(
            Ra2IniEditApplyOutcomeKind.Applied,
            Ra2IniEditPreviewCurrencyKind.Current,
            preview.PreviewId,
            updatedSession,
            preview.CandidateText,
            preview.Snapshot.Text,
            preview.CandidateText,
            Math.Clamp(undoCaretOffset, 0, preview.Snapshot.Text.Length),
            Math.Clamp(redoCaretOffset, 0, preview.CandidateText.Length),
            preview.OperationPreviews.Count,
            updatedSession.DocumentState.IsDirty,
            $"已在当前文档内存中应用 {preview.OperationPreviews.Count} 项结构化编辑；尚未保存。");
    }

    public static Ra2IniEditApplyResult PreviewUnavailable(Guid previewId)
        => Failed(
            Ra2IniEditApplyOutcomeKind.PreviewUnavailable,
            Ra2IniEditPreviewCurrencyKind.Current,
            previewId,
            "编辑预览不可用、已失效或已消费，请重新生成预览。");

    public static Ra2IniEditApplyResult ConfirmationRequired(Guid previewId)
        => Failed(
            Ra2IniEditApplyOutcomeKind.ConfirmationRequired,
            Ra2IniEditPreviewCurrencyKind.Current,
            previewId,
            "应用结构化编辑前需要用户显式确认。");

    public static Ra2IniEditApplyResult Stale(
        Guid previewId,
        Ra2IniEditPreviewCurrencyResult currency)
    {
        ArgumentNullException.ThrowIfNull(currency);
        if (currency.IsCurrent)
            throw new ArgumentException("A current preview cannot produce a stale apply result.", nameof(currency));

        return Failed(
            Ra2IniEditApplyOutcomeKind.StalePreview,
            currency.Kind,
            previewId,
            currency.Message);
    }

    public static Ra2IniEditApplyResult TransactionRejected(
        Guid previewId,
        string? message = null)
        => Failed(
            Ra2IniEditApplyOutcomeKind.TransactionRejected,
            Ra2IniEditPreviewCurrencyKind.Current,
            previewId,
            string.IsNullOrWhiteSpace(message)
                ? "编辑器事务拒绝了结构化编辑。"
                : message);

    public static Ra2IniEditApplyResult UnexpectedFailure(Guid previewId)
        => Failed(
            Ra2IniEditApplyOutcomeKind.UnexpectedFailure,
            Ra2IniEditPreviewCurrencyKind.Current,
            previewId,
            "无法完成结构化编辑事务，当前文件尚未保存。");

    private static Ra2IniEditApplyResult Failed(
        Ra2IniEditApplyOutcomeKind outcomeKind,
        Ra2IniEditPreviewCurrencyKind currencyKind,
        Guid previewId,
        string message)
        => new(
            outcomeKind,
            currencyKind,
            previewId,
            null,
            null,
            null,
            null,
            null,
            null,
            0,
            false,
            message);
}
