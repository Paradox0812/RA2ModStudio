using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AddPropertyInsertPlannerTests
{
    [Fact]
    public void PlanInsert_InsertsKeyValueBelowCurrentLine()
    {
        const string text = "[HTNK]\nName=HTNK\nArmor=heavy\n";
        Ra2AddPropertyInsertPlan plan = Plan(text, text.IndexOf("Name=HTNK", StringComparison.Ordinal), "Strength", "400");

        string result = Apply(text, plan.Change);

        Assert.Equal("[HTNK]\nName=HTNK\nStrength=400\nArmor=heavy\n", result);
        Assert.Equal(result.IndexOf("Strength=400", StringComparison.Ordinal) + "Strength=400".Length, plan.CaretOffset);
    }

    [Fact]
    public void PlanInsert_EmptyValueInsertsKeyEquals()
    {
        const string text = "[HTNK]\n";
        Ra2AddPropertyInsertPlan plan = Plan(text, text.IndexOf("[HTNK]", StringComparison.Ordinal), "Primary", "");

        Assert.Equal("[HTNK]\nPrimary=\n", Apply(text, plan.Change));
    }

    [Fact]
    public void PlanInsert_AtSectionHeaderInsertsAfterHeader()
    {
        const string text = "[HTNK]\nName=HTNK\n";
        Ra2AddPropertyInsertPlan plan = Plan(text, text.IndexOf("[HTNK]", StringComparison.Ordinal) + 1, "Primary", "120mm");

        Assert.Equal("[HTNK]\nPrimary=120mm\nName=HTNK\n", Apply(text, plan.Change));
    }

    [Fact]
    public void PlanInsert_DocumentWithoutTrailingNewlineAddsNewlineBeforeInsertedLine()
    {
        const string text = "[HTNK]\nName=HTNK";
        Ra2AddPropertyInsertPlan plan = Plan(text, text.Length, "Strength", "400");

        Assert.Equal("[HTNK]\nName=HTNK\nStrength=400", Apply(text, plan.Change));
    }

    [Fact]
    public void PlanInsert_PreservesCrLf()
    {
        const string text = "[HTNK]\r\nName=HTNK\r\nArmor=heavy\r\n";
        Ra2AddPropertyInsertPlan plan = Plan(text, text.IndexOf("Name=HTNK", StringComparison.Ordinal), "Strength", "400");

        Assert.Equal("[HTNK]\r\nName=HTNK\r\nStrength=400\r\nArmor=heavy\r\n", Apply(text, plan.Change));
    }

    [Fact]
    public void PlanInsert_UsesRawKeyAndNeverDisplayName()
    {
        const string text = "[HTNK]\n";
        Ra2AddPropertyInsertPlan plan = Plan(text, text.Length, "Strength", "400");

        Assert.DoesNotContain("Health", plan.Change.NewText);
        Assert.Contains("Strength=400", plan.Change.NewText);
    }

    [Fact]
    public void PlanInsert_ProducesDuplicateKeyWarningForCurrentSection()
    {
        const string text = "[HTNK]\nStrength=300\n";
        Ra2AddPropertyInsertPlan plan = Plan(text, text.Length, "Strength", "400");

        Assert.Contains(plan.Warnings, warning => warning.Contains("already contain key", StringComparison.OrdinalIgnoreCase));
    }

    private static Ra2AddPropertyInsertPlan Plan(string text, int caretOffset, string option, string value)
        => new Ra2AddPropertyInsertPlanner().PlanInsert(
            new Ra2IniTextDocumentParser().Parse(text),
            caretOffset,
            option,
            value);

    private static string Apply(string text, Ra2TextChange change)
        => text.Remove(change.Span.Start, change.Span.Length).Insert(change.Span.Start, change.NewText);
}
