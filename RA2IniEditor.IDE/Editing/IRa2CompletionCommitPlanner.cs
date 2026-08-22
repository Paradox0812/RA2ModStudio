using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.Editing;

internal interface IRa2CompletionCommitPlanner
{
    Ra2TextChange PlanCommit(Ra2CompletionResult result, Ra2CompletionItem selectedItem);
}
