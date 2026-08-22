using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.Search;

internal enum Ra2ReplaceFailureKind
{
    None,
    NotEditable,
    ProjectScopeNotSupported,
    EmptyQuery,
    InvalidRegex,
    RegexTimeout,
    ZeroLengthMatch,
    NoMatches,
    NoChanges,
    TooManyMatches,
    Unexpected
}

internal sealed class Ra2CurrentFileReplacePlan
{
    private Ra2CurrentFileReplacePlan(
        bool success,
        Ra2ReplaceFailureKind failureKind,
        string message,
        Guid documentId,
        int editRevision,
        string originalText,
        string updatedText,
        Ra2TextChangeSet? changeSet,
        int matchCount)
    {
        Success = success;
        FailureKind = failureKind;
        Message = message;
        DocumentId = documentId;
        EditRevision = editRevision;
        OriginalText = originalText;
        UpdatedText = updatedText;
        ChangeSet = changeSet;
        MatchCount = matchCount;
    }

    public bool Success { get; }

    public Ra2ReplaceFailureKind FailureKind { get; }

    public string Message { get; }

    public Guid DocumentId { get; }

    public int EditRevision { get; }

    public string OriginalText { get; }

    public string UpdatedText { get; }

    public Ra2TextChangeSet? ChangeSet { get; }

    public int MatchCount { get; }

    public bool IsCurrentFor(Ra2EditableDocumentSession? session)
        => Success &&
           session is not null &&
           session.DocumentId == DocumentId &&
           session.EditRevision == EditRevision &&
           string.Equals(session.DocumentState.CurrentText, OriginalText, StringComparison.Ordinal);

    public static Ra2CurrentFileReplacePlan Succeeded(
        Ra2EditableDocumentSession session,
        string updatedText,
        Ra2TextChangeSet changeSet,
        int matchCount)
        => new(
            true,
            Ra2ReplaceFailureKind.None,
            $"已预览 {matchCount} 处替换；尚未修改文件。",
            session.DocumentId,
            session.EditRevision,
            session.DocumentState.CurrentText,
            updatedText,
            changeSet,
            matchCount);

    public static Ra2CurrentFileReplacePlan Failed(Ra2ReplaceFailureKind failureKind, string message)
        => new(false, failureKind, message, Guid.Empty, 0, string.Empty, string.Empty, null, 0);
}
