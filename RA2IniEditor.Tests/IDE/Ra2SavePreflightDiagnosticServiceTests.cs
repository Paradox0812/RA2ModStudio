using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Diagnostics;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.FieldRegistry;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SavePreflightDiagnosticServiceTests
{
    [Fact]
    public void Analyze_WhenCurrentTextHasStructureIssue_ReturnsIssueSummary()
    {
        Ra2SavePreflightDiagnosticService service = new();
        CurrentSourceSnapshot snapshot = CreateSnapshot(
            """
            [E1]
            Name=GI
            """);

        Ra2SavePreflightResult result = service.Analyze(
            snapshot,
            """
            [E1]
            Name=GI
            Name=Duplicate
            """,
            fieldProvider: null);

        Assert.True(result.WasRun);
        Assert.True(result.HasIssues);
        Assert.Equal(1, result.IssueCount);
        Assert.Equal(0, result.ErrorCount);
        Assert.Equal(1, result.WarningCount);
        Assert.Equal("INI_STRUCTURE", result.Issues[0].Code);
        Assert.Contains("保存前检查发现 1 个可能问题", result.SummaryText, StringComparison.Ordinal);
        Assert.Contains("结构问题：1", result.SourceSummaryText, StringComparison.Ordinal);
        Assert.Contains("警告：1", result.SeveritySummaryText, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_WhenNoIssue_ReturnsCleanSummary()
    {
        Ra2SavePreflightDiagnosticService service = new();
        CurrentSourceSnapshot snapshot = CreateSnapshot(
            """
            [E1]
            Name=GI
            """);

        Ra2SavePreflightResult result = service.Analyze(snapshot, snapshot.Text, fieldProvider: null);

        Assert.True(result.WasRun);
        Assert.False(result.HasIssues);
        Assert.Equal(0, result.IssueCount);
        Assert.Equal("保存前检查完成，未发现问题。", result.SummaryText);
        Assert.Equal("来源汇总：无。", result.SourceSummaryText);
        Assert.Equal("严重度汇总：无。", result.SeveritySummaryText);
    }

    [Theory]
    [InlineData(SourceEditorState.Empty)]
    [InlineData(SourceEditorState.Loading)]
    [InlineData(SourceEditorState.DeferredLargeFile)]
    [InlineData(SourceEditorState.ReadFailed)]
    public void Analyze_WhenSnapshotCannotRunDiagnostics_ReturnsNotRun(SourceEditorState state)
    {
        Ra2SavePreflightDiagnosticService service = new();
        CurrentSourceSnapshot snapshot = new("C:\\mod", "C:\\mod\\rules.ini", "rules.ini", "text", 1, state);

        Ra2SavePreflightResult result = service.Analyze(snapshot, "Name=Changed", fieldProvider: null);

        Assert.False(result.WasRun);
        Assert.False(result.HasIssues);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void FromIssues_BuildsSummaryBySource()
    {
        Ra2SavePreflightResult result = Ra2SavePreflightResult.FromIssues(
        [
            CreateIssue("INI_STRUCTURE", "CoreParserValidator", IniIssueSeverity.Warning),
            CreateIssue(Ra2FieldDiagnosticService.UnknownKeyCode, "Field", IniIssueSeverity.Warning),
            CreateIssue(Ra2ReferenceDiagnosticService.MissingTargetCode, "Reference", IniIssueSeverity.Warning),
            CreateIssue(CurrentFileReadonlyDiagnosticService.DiagnosticExceptionCode, "DiagnosticService", IniIssueSeverity.Error)
        ]);

        Assert.Equal(4, result.IssueCount);
        Assert.Equal("保存前检查发现 4 个可能问题。", result.SummaryText);
        Assert.Equal(
            """
            结构问题：1
            字段问题：1
            引用问题：1
            诊断服务问题：1
            """.ReplaceLineEndings(),
            result.SourceSummaryText.ReplaceLineEndings());
        Assert.Contains("错误：1", result.SeveritySummaryText, StringComparison.Ordinal);
        Assert.Contains("警告：3", result.SeveritySummaryText, StringComparison.Ordinal);
    }

    [Fact]
    public void FromIssues_UsesFriendlyNonBlockingSummaryForWarnings()
    {
        Ra2SavePreflightResult result = Ra2SavePreflightResult.FromIssues(
        [
            CreateIssue(Ra2FieldDiagnosticService.InvalidBooleanValueCode, "Field", IniIssueSeverity.Warning)
        ]);

        Assert.Equal("保存前检查发现 1 个可能问题。", result.SummaryText);
        Assert.DoesNotContain("阻止", result.SummaryText, StringComparison.Ordinal);
        Assert.Equal(0, result.ErrorCount);
        Assert.Equal(1, result.WarningCount);
    }

    [Fact]
    public void Analyze_WhenFieldProviderIsAvailable_ReusesCurrentFileFieldDiagnostics()
    {
        Ra2SavePreflightDiagnosticService service = new();
        CurrentSourceSnapshot snapshot = CreateSnapshot(
            """
            [InfantryTypes]
            0=E1
            [E1]
            Armor=light
            """);
        var provider = new LocalRa2FieldDefinitionProvider(
        [
            new Ra2FieldDefinition(
                "Armor",
                [Ra2SectionKind.Infantry],
                FieldEditorKind.Text,
                Ra2FieldSourceKind.User,
                valueMetadata: Ra2FieldValueMetadata.Unknown)
        ]);

        Ra2SavePreflightResult result = service.Analyze(
            snapshot,
            """
            [InfantryTypes]
            0=E1
            [E1]
            Armor=light
            ArmorX=paper
            """,
            provider);

        Assert.Contains(result.Issues, issue => issue.Code == Ra2FieldDiagnosticService.UnknownKeyCode);
    }

    [Fact]
    public void Analyze_DoesNotWarnForNumberWithInlineSemicolonComment()
    {
        Ra2SavePreflightDiagnosticService service = new();
        CurrentSourceSnapshot snapshot = CreateSnapshot(
            """
            [WeaponTypes]
            0=120mm
            [120mm]
            Damage=175
            """);
        var provider = new LocalRa2FieldDefinitionProvider(
        [
            new Ra2FieldDefinition(
                "Damage",
                [Ra2SectionKind.Weapon],
                FieldEditorKind.Text,
                Ra2FieldSourceKind.User,
                valueMetadata: new Ra2FieldValueMetadata(Ra2FieldValueKind.Integer))
        ]);

        Ra2SavePreflightResult result = service.Analyze(
            snapshot,
            """
            [WeaponTypes]
            0=120mm
            [120mm]
            Damage=175;125
            """,
            provider);

        Assert.False(result.HasIssues);
        Assert.DoesNotContain(result.Issues, issue => issue.Code == Ra2FieldDiagnosticService.InvalidNumberValueCode);
    }

    private static CurrentSourceSnapshot CreateSnapshot(string text)
        => new("C:\\mod", "C:\\mod\\rules.ini", "rules.ini", text, 42, SourceEditorState.Loaded);

    private static IdeDiagnosticIssueViewModel CreateIssue(
        string code,
        string sourceKind,
        IniIssueSeverity severity)
        => new(
            code,
            sourceKind,
            severity,
            "message",
            "C:\\mod\\rules.ini",
            1,
            1,
            "E1",
            "Name",
            42);
}
