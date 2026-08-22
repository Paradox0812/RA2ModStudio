using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionCommitPlanApplyIntegrationTests
{
    [Fact]
    public void PlanAndApply_KeyCompletionProducesEditedBufferOnly()
    {
        Ra2CompletionResult completionResult = new(
            [],
            new Ra2TextSpan(0, 3));
        Ra2CompletionItem selectedItem = new("Strength", Ra2CompletionItemKind.Key, insertText: "Strength=");
        Ra2TextChange change = new Ra2CompletionCommitPlanner().PlanCommit(completionResult, selectedItem);

        Ra2TextChangeApplyResult applyResult = CreateApplier().Apply(
            CreateState("Str"),
            change);

        Assert.True(applyResult.Success);
        Assert.Equal("Strength=", applyResult.DocumentState!.CurrentText);
        Assert.Equal(Ra2EditorDocumentState.EditableDirty, applyResult.DocumentState.State);
    }

    [Fact]
    public void PlanAndApply_ReferenceCompletionProducesEditedBufferOnly()
    {
        Ra2CompletionResult completionResult = new(
            [],
            new Ra2TextSpan("Primary=".Length, 0));
        Ra2CompletionItem selectedItem = new("120mm", Ra2CompletionItemKind.Reference);
        Ra2TextChange change = new Ra2CompletionCommitPlanner().PlanCommit(completionResult, selectedItem);

        Ra2TextChangeApplyResult applyResult = CreateApplier().Apply(
            CreateState("Primary="),
            change);

        Assert.True(applyResult.Success);
        Assert.Equal("Primary=120mm", applyResult.DocumentState!.CurrentText);
    }

    private static IRa2TextChangeApplier CreateApplier()
        => new Ra2TextChangeApplier(new Ra2IniTextDocumentParser(), new Ra2DirtyStateService());

    private static Ra2EditableDocumentState CreateState(string currentText)
        => new("rulesmd.ini", currentText, currentText, Ra2EditorDocumentState.EditableClean);
}
