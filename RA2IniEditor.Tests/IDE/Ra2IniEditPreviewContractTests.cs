using RA2IniEditor.Core;
using RA2IniEditor.IDE.Editing;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2IniEditPreviewContractTests
{
    [Fact]
    public void CompareDiagnostics_IgnoresLocationAndAnalysisVersionDrift()
    {
        Ra2AutomationDiagnosticFact current = Fact(
            "REF_MISSING_TARGET",
            "Missing Weapon",
            line: 3,
            column: 2,
            analysisVersion: 1);
        Ra2AutomationDiagnosticFact candidate = Fact(
            "REF_MISSING_TARGET",
            "Missing Weapon",
            line: 9,
            column: 5,
            analysisVersion: 2);

        Ra2AutomationDiagnosticDelta delta = Ra2AutomationDiagnosticDeltaCalculator.Compare([current], [candidate]);

        Assert.Empty(delta.Added);
        Assert.Empty(delta.Removed);
    }

    [Fact]
    public void CompareDiagnostics_MessageChangeProducesAddedAndRemovedEvidence()
    {
        Ra2AutomationDiagnosticFact current = Fact("REF_MISSING_TARGET", "Missing Gun", line: 3);
        Ra2AutomationDiagnosticFact candidate = Fact("REF_MISSING_TARGET", "Missing Laser", line: 3);

        Ra2AutomationDiagnosticDelta delta = Ra2AutomationDiagnosticDeltaCalculator.Compare([current], [candidate]);

        Assert.Same(candidate, Assert.Single(delta.Added));
        Assert.Same(current, Assert.Single(delta.Removed));
    }

    [Fact]
    public void CompareDiagnostics_UsesMultisetCountsAndPreservesOrder()
    {
        Ra2AutomationDiagnosticFact first = Fact("FIELD_UNKNOWN_KEY", "Unknown", line: 1);
        Ra2AutomationDiagnosticFact duplicate = Fact("FIELD_UNKNOWN_KEY", "Unknown", line: 2);
        Ra2AutomationDiagnosticFact added = Fact("FIELD_BOOLEAN_INVALID", "Invalid", line: 3);

        Ra2AutomationDiagnosticDelta delta = Ra2AutomationDiagnosticDeltaCalculator.Compare(
            [first],
            [duplicate, first, added]);

        Assert.Equal([first, added], delta.Added);
        Assert.Empty(delta.Removed);
    }

    private static Ra2AutomationDiagnosticFact Fact(
        string code,
        string message,
        int line,
        int column = 1,
        int analysisVersion = 1)
        => new(
            code,
            "Test",
            IniIssueSeverity.Warning,
            message,
            "rulesmd.ini",
            line,
            column,
            "E1",
            "Primary",
            analysisVersion);
}
