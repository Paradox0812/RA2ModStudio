namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2CompletionCommitApplyResult
{
    private Ra2CompletionCommitApplyResult(
        bool success,
        Ra2EditableDocumentSession? session,
        int caretOffset,
        string? errorMessage)
    {
        Success = success;
        Session = session;
        CaretOffset = caretOffset;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }

    public Ra2EditableDocumentSession? Session { get; }

    public int CaretOffset { get; }

    public string? ErrorMessage { get; }

    public static Ra2CompletionCommitApplyResult Succeeded(
        Ra2EditableDocumentSession session,
        int caretOffset)
    {
        ArgumentNullException.ThrowIfNull(session);

        return new Ra2CompletionCommitApplyResult(true, session, caretOffset, null);
    }

    public static Ra2CompletionCommitApplyResult Failed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Completion commit failure message cannot be empty.", nameof(errorMessage));

        return new Ra2CompletionCommitApplyResult(false, null, 0, errorMessage);
    }
}
