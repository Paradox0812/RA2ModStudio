using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2CompletionCommitCoordinator : IRa2CompletionCommitCoordinator
{
    private readonly IRa2CompletionCommitPlanner _planner;
    private readonly IRa2TextChangeApplier _applier;

    public Ra2CompletionCommitCoordinator(
        IRa2CompletionCommitPlanner planner,
        IRa2TextChangeApplier applier)
    {
        _planner = planner ?? throw new ArgumentNullException(nameof(planner));
        _applier = applier ?? throw new ArgumentNullException(nameof(applier));
    }

    public Ra2CompletionCommitApplyResult TryCommit(
        Ra2EditableDocumentSession session,
        Ra2CompletionResult completionResult,
        Ra2CompletionItem selectedItem)
    {
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(completionResult);
        ArgumentNullException.ThrowIfNull(selectedItem);

        Ra2TextChange change = _planner.PlanCommit(completionResult, selectedItem);
        Ra2TextChangeApplyResult applyResult = _applier.Apply(session.DocumentState, change);
        if (!applyResult.Success || applyResult.DocumentState is null || applyResult.TextDocument is null)
            return Ra2CompletionCommitApplyResult.Failed(applyResult.ErrorMessage ?? "Completion commit failed.");

        Ra2EditableDocumentSession nextSession = session.ContinueWith(
            applyResult.DocumentState,
            applyResult.TextDocument);
        int caretOffset = change.Span.Start + selectedItem.InsertText.Length;
        return Ra2CompletionCommitApplyResult.Succeeded(nextSession, caretOffset);
    }
}
