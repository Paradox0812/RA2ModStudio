using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core.Schema;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationReferenceQueryTests
{
    private readonly Ra2AutomationDocumentQueryService _service = new();

    [Fact]
    public void FindReferences_HeaderQueryReturnsCaseInsensitiveSourceOrder()
    {
        const string text =
            "[InfantryTypes]\n" +
            "0=E1\n" +
            "1=TANK\n" +
            "[E1]\n" +
            "Primary=SharedWeapon\n" +
            "[TANK]\n" +
            "Secondary=sharedweapon\n" +
            "[SharedWeapon]\n" +
            "Damage=90\n";

        Ra2AutomationReferenceQueryResult result = _service.FindReferences(
            AutomationTestSupport.Snapshot(text),
            new Ra2AutomationReferenceQuery(text.LastIndexOf("[SharedWeapon]", StringComparison.Ordinal) + 1));

        Ra2AutomationReferenceTargetFact target = AssertSuccess(result);
        Assert.Equal("SharedWeapon", target.Name);
        Assert.Equal(Ra2SectionKind.Weapon, target.Kind);
        Assert.True(result.HasReferences);
        Assert.Equal(new[] { "E1", "TANK" }, result.References.Select(reference => reference.SourceSectionName));
        Assert.Equal(new[] { "Primary", "Secondary" }, result.References.Select(reference => reference.SourceKey));
        Assert.Equal(new[] { 5, 7 }, result.References.Select(reference => reference.LineNumber));
        Assert.Equal(
            new[] { "SharedWeapon", "sharedweapon" },
            result.References.Select(reference => AutomationTestSupport.Slice(text, reference.ValueSpan)));
    }

    [Fact]
    public void FindReferences_ValueQueryResolvesTargetWithoutDefinition()
    {
        const string text = "[InfantryTypes]\n0=E1\n[E1]\nPrimary=MissingWeapon\n";
        int sourceOffset = text.IndexOf("MissingWeapon", StringComparison.Ordinal) + 1;

        Ra2AutomationReferenceQueryResult result = _service.FindReferences(
            AutomationTestSupport.Snapshot(text),
            new Ra2AutomationReferenceQuery(sourceOffset));

        Ra2AutomationReferenceTargetFact target = AssertSuccess(result);
        Assert.Equal("MissingWeapon", target.Name);
        Assert.Single(result.References);
        Assert.Equal("Primary", result.References[0].SourceKey);
    }

    [Fact]
    public void FindReferences_SelectionWinsOverCaretContext()
    {
        const string text =
            "[InfantryTypes]\n" +
            "0=E1\n" +
            "[E1]\n" +
            "Primary=FirstWeapon\n" +
            "Secondary=SecondWeapon\n";
        int firstStart = text.IndexOf("FirstWeapon", StringComparison.Ordinal);
        int caretOffset = text.IndexOf("SecondWeapon", StringComparison.Ordinal) + 1;

        Ra2AutomationReferenceQueryResult result = _service.FindReferences(
            AutomationTestSupport.Snapshot(text),
            new Ra2AutomationReferenceQuery(
                caretOffset,
                new Ra2AutomationTextSpan(firstStart, "FirstWeapon".Length)));

        Ra2AutomationReferenceTargetFact target = AssertSuccess(result);
        Assert.Equal("FirstWeapon", target.Name);
        Assert.Equal("Primary", Assert.Single(result.References).SourceKey);
    }

    [Fact]
    public void FindReferences_InvalidSelectionFallsBackToCaretContext()
    {
        const string text = "[InfantryTypes]\n0=E1\n[E1]\nPrimary=FirstWeapon\n";
        int keyStart = text.IndexOf("Primary", StringComparison.Ordinal);
        int caretOffset = text.IndexOf("FirstWeapon", StringComparison.Ordinal) + 1;

        Ra2AutomationReferenceQueryResult result = _service.FindReferences(
            AutomationTestSupport.Snapshot(text),
            new Ra2AutomationReferenceQuery(
                caretOffset,
                new Ra2AutomationTextSpan(keyStart, "Primary".Length)));

        Ra2AutomationReferenceTargetFact target = AssertSuccess(result);
        Assert.Equal("FirstWeapon", target.Name);
        Assert.Equal("Primary", Assert.Single(result.References).SourceKey);
    }

    [Fact]
    public void FindReferences_ResolvedTargetWithNoUsagesIsSuccessfulEmpty()
    {
        const string text = "[Unused]\nValue=1\n";

        Ra2AutomationReferenceQueryResult result = _service.FindReferences(
            AutomationTestSupport.Snapshot(text),
            new Ra2AutomationReferenceQuery(text.IndexOf("Unused", StringComparison.Ordinal) + 1));

        Ra2AutomationReferenceTargetFact target = AssertSuccess(result);
        Assert.Equal("Unused", target.Name);
        Assert.Empty(result.References);
        Assert.False(result.HasReferences);
    }

    [Fact]
    public void FindReferences_KeyAndOutOfRangeLocationsAreTypedFailuresWithoutPayload()
    {
        const string text = "[E1]\nStrength=100\n";
        Ra2AutomationReferenceQueryResult unresolved = _service.FindReferences(
            AutomationTestSupport.Snapshot(text),
            new Ra2AutomationReferenceQuery(text.IndexOf("Strength", StringComparison.Ordinal)));
        Ra2AutomationReferenceQueryResult invalidOffset = _service.FindReferences(
            AutomationTestSupport.Snapshot(text),
            new Ra2AutomationReferenceQuery(text.Length + 1));
        Ra2AutomationReferenceQueryResult invalidSelection = _service.FindReferences(
            AutomationTestSupport.Snapshot(text),
            new Ra2AutomationReferenceQuery(
                0,
                new Ra2AutomationTextSpan(text.Length - 2, 4)));

        AssertFailure(unresolved, Ra2AutomationReferenceQueryFailureKind.TargetNotResolved);
        AssertFailure(invalidOffset, Ra2AutomationReferenceQueryFailureKind.InvalidLocation);
        AssertFailure(invalidSelection, Ra2AutomationReferenceQueryFailureKind.InvalidLocation);
    }

    [Fact]
    public void FindReferences_ReportsExactTokenAndLineSpans()
    {
        const string text = "[InfantryTypes]\r\n0=E1\r\n[E1]\r\nPrimary=Weapon; comment\r\n";

        Ra2AutomationReferenceQueryResult result = _service.FindReferences(
            AutomationTestSupport.Snapshot(text),
            new Ra2AutomationReferenceQuery(text.IndexOf("Weapon", StringComparison.Ordinal) + 1));

        AssertSuccess(result);
        Ra2AutomationReferenceFact reference = Assert.Single(result.References);
        Assert.Equal("E1", reference.SourceSectionName);
        Assert.Equal("Primary", reference.SourceKey);
        Assert.Equal(4, reference.LineNumber);
        Assert.Equal(
            "Primary=Weapon; comment",
            AutomationTestSupport.Slice(text, reference.LineSpan));
        Assert.Equal("Weapon", AutomationTestSupport.Slice(text, reference.ValueSpan));
    }

    private static Ra2AutomationReferenceTargetFact AssertSuccess(Ra2AutomationReferenceQueryResult result)
    {
        Assert.True(result.Succeeded, result.Message);
        Assert.Equal(Ra2AutomationReferenceQueryFailureKind.None, result.FailureKind);
        Assert.NotNull(result.Target);
        Assert.NotEmpty(result.Message);
        return result.Target!;
    }

    private static void AssertFailure(
        Ra2AutomationReferenceQueryResult result,
        Ra2AutomationReferenceQueryFailureKind expectedFailure)
    {
        Assert.False(result.Succeeded);
        Assert.Equal(expectedFailure, result.FailureKind);
        Assert.NotEmpty(result.Message);
        Assert.Null(result.Target);
        Assert.Empty(result.References);
        Assert.False(result.HasReferences);
    }
}
