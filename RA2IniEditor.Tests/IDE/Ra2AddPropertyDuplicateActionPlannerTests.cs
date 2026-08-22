using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AddPropertyDuplicateActionPlannerTests
{
    [Fact]
    public void PlanReplaceExisting_ReplacesValueOnlyAndPreservesInlineComment()
    {
        const string text = "[HTNK]\nStrength=400 ; hp\n";
        Ra2IniTextDocument document = Parse(text);
        Ra2DuplicateKeyMatch match = Find(document, text, "Strength");

        Ra2AddPropertyInsertPlan plan = new Ra2AddPropertyInsertPlanner().PlanReplaceExisting(
            match,
            "Strength",
            "500");
        string nextText = Apply(text, plan.Change);

        Assert.Contains("Strength=500 ; hp", nextText);
        Assert.DoesNotContain("Strength=400", nextText);
        Assert.Equal(nextText.IndexOf("500", StringComparison.Ordinal) + 3, plan.CaretOffset);
    }

    [Fact]
    public void PlanReplaceExisting_ReplacesEmptyValue()
    {
        const string text = "[HTNK]\nStrength= ; hp\n";
        Ra2IniTextDocument document = Parse(text);
        Ra2DuplicateKeyMatch match = Find(document, text, "Strength");

        Ra2AddPropertyInsertPlan plan = new Ra2AddPropertyInsertPlanner().PlanReplaceExisting(
            match,
            "Strength",
            "500");

        Assert.Contains("Strength=500 ; hp", Apply(text, plan.Change));
    }

    [Fact]
    public void PlanInsertDuplicate_StillInsertsNewLine()
    {
        const string text = "[HTNK]\nStrength=400\n";
        Ra2IniTextDocument document = Parse(text);

        Ra2AddPropertyInsertPlan plan = new Ra2AddPropertyInsertPlanner().PlanInsertDuplicate(
            document,
            text.IndexOf("Strength", StringComparison.Ordinal),
            "Strength",
            "500");

        string nextText = Apply(text, plan.Change);
        Assert.Contains("Strength=400", nextText);
        Assert.Contains("Strength=500", nextText);
    }

    private static Ra2DuplicateKeyMatch Find(Ra2IniTextDocument document, string text, string key)
        => new Ra2DuplicateKeyDetector().FindInCurrentSection(
            document,
            text.IndexOf(key, StringComparison.Ordinal),
            key) ?? throw new InvalidOperationException("Expected duplicate match.");

    private static Ra2IniTextDocument Parse(string text)
        => new Ra2IniTextDocumentParser().Parse(text);

    private static string Apply(string text, Ra2TextChange change)
        => text.Remove(change.Span.Start, change.Span.Length).Insert(change.Span.Start, change.NewText);
}
