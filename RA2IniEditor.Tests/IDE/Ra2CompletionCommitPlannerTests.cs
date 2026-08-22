using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionCommitPlannerTests
{
    [Fact]
    public void PlanCommit_UsesReplacementSpanAndInsertText()
    {
        Ra2CompletionResult result = new(
            [],
            new Ra2TextSpan(12, 3));
        Ra2CompletionItem item = new(
            "Strength",
            Ra2CompletionItemKind.Key,
            insertText: "Strength");

        Ra2TextChange change = new Ra2CompletionCommitPlanner().PlanCommit(result, item);

        Assert.Equal(12, change.Span.Start);
        Assert.Equal(3, change.Span.Length);
        Assert.Equal("Strength", change.NewText);
        Assert.Equal("CompletionCommit", change.Reason);
    }

    [Fact]
    public void PlanCommit_ZeroLengthReplacementSpanRepresentsInsertion()
    {
        Ra2CompletionResult result = new(
            [],
            new Ra2TextSpan(20, 0));
        Ra2CompletionItem item = new(
            "120mm",
            Ra2CompletionItemKind.Reference,
            insertText: "120mm");

        Ra2TextChange change = new Ra2CompletionCommitPlanner().PlanCommit(result, item);

        Assert.Equal(20, change.Span.Start);
        Assert.Equal(0, change.Span.Length);
        Assert.Equal("120mm", change.NewText);
    }

    [Fact]
    public void PlanCommit_DoesNotMutateResultOrItem()
    {
        Ra2CompletionResult result = new(
            [
                new Ra2CompletionItem("Sight", Ra2CompletionItemKind.Key)
            ],
            new Ra2TextSpan(4, 2));
        Ra2CompletionItem item = result.Items[0];

        Ra2TextChange change = new Ra2CompletionCommitPlanner().PlanCommit(result, item);

        Assert.Equal("Sight", item.Label);
        Assert.Equal("Sight", item.InsertText);
        Assert.Equal(4, result.ReplacementSpan.Start);
        Assert.Equal(2, result.ReplacementSpan.Length);
        Assert.Equal("Sight", change.NewText);
    }
}
