using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2CompletionCommitPlanner : IRa2CompletionCommitPlanner
{
    public const string CompletionCommitReason = "CompletionCommit";

    public Ra2TextChange PlanCommit(Ra2CompletionResult result, Ra2CompletionItem selectedItem)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(selectedItem);

        return new Ra2TextChange(
            result.ReplacementSpan,
            selectedItem.InsertText,
            CompletionCommitReason);
    }
}
