using RA2IniEditor.Core;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2IniEditPreviewContractTests
{
    [Fact]
    public void CompareDiagnostics_IgnoresLocationAndAnalysisVersionDrift()
    {
        Ra2DiagnosticFact current = Fact(
            "REF_MISSING_TARGET",
            "Missing Weapon",
            line: 3,
            column: 2,
            analysisVersion: 1);
        Ra2DiagnosticFact candidate = Fact(
            "REF_MISSING_TARGET",
            "Missing Weapon",
            line: 9,
            column: 5,
            analysisVersion: 2);

        var delta = Ra2IniEditPreview.CompareDiagnostics([current], [candidate]);

        Assert.Empty(delta.Added);
        Assert.Empty(delta.Removed);
    }

    [Fact]
    public void CompareDiagnostics_MessageChangeProducesAddedAndRemovedEvidence()
    {
        Ra2DiagnosticFact current = Fact("REF_MISSING_TARGET", "Missing Gun", line: 3);
        Ra2DiagnosticFact candidate = Fact("REF_MISSING_TARGET", "Missing Laser", line: 3);

        var delta = Ra2IniEditPreview.CompareDiagnostics([current], [candidate]);

        Assert.Same(candidate, Assert.Single(delta.Added));
        Assert.Same(current, Assert.Single(delta.Removed));
    }

    [Fact]
    public void CompareDiagnostics_UsesMultisetCountsAndPreservesOrder()
    {
        Ra2DiagnosticFact first = Fact("FIELD_UNKNOWN_KEY", "Unknown", line: 1);
        Ra2DiagnosticFact duplicate = Fact("FIELD_UNKNOWN_KEY", "Unknown", line: 2);
        Ra2DiagnosticFact added = Fact("FIELD_BOOLEAN_INVALID", "Invalid", line: 3);

        var delta = Ra2IniEditPreview.CompareDiagnostics(
            [first],
            [duplicate, first, added]);

        Assert.Equal([first, added], delta.Added);
        Assert.Empty(delta.Removed);
    }

    private static Ra2DiagnosticFact Fact(
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
