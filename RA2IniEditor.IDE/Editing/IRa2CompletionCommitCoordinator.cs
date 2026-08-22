using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.Editing;

internal interface IRa2CompletionCommitCoordinator
{
    Ra2CompletionCommitApplyResult TryCommit(
        Ra2EditableDocumentSession session,
        Ra2CompletionResult completionResult,
        Ra2CompletionItem selectedItem);
}
