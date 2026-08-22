using RA2IniEditor.Core;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.ViewModels;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiDiagnosticSummaryProviderTests
{
    [Fact]
    public void Summarize_CurrentLineDiagnosticIsIncludedFirst()
    {
        Ra2CurrentFileAiDiagnosticSummaryProvider provider = new();

        IReadOnlyList<Ra2AiDiagnosticSummary> summaries = provider.Summarize(
            [
                CreateIssue("FIELD_UNKNOWN_KEY", IniIssueSeverity.Error, line: 6, section: "HTNK", key: "Strength"),
                CreateIssue("INI_STRUCTURE", IniIssueSeverity.Warning, line: 4, section: "Other", key: "Other")
            ],
            currentFilePath: "rulesmd.ini",
            currentFileDisplayName: "rulesmd.ini",
            currentVersion: 1,
            caretLineNumber: 4,
            sectionName: "HTNK",
            keyName: "Strength",
            maxCount: 5);

        Assert.Equal("INI_STRUCTURE", summaries[0].Code);
        Assert.Equal("current line", summaries[0].MatchReason);
    }

    [Fact]
    public void Summarize_CurrentKeyDiagnosticIsIncluded()
    {
        Ra2CurrentFileAiDiagnosticSummaryProvider provider = new();

        IReadOnlyList<Ra2AiDiagnosticSummary> summaries = provider.Summarize(
            [
                CreateIssue("FIELD_VALUE", IniIssueSeverity.Warning, line: 8, section: "Other", key: "Strength"),
                CreateIssue("INI_STRUCTURE", IniIssueSeverity.Warning, line: 9, section: "Other", key: "Armor")
            ],
            currentFilePath: "rulesmd.ini",
            currentFileDisplayName: "rulesmd.ini",
            currentVersion: 1,
            caretLineNumber: 4,
            sectionName: "HTNK",
            keyName: "Strength",
            maxCount: 5);

        Assert.Contains(summaries, summary =>
            summary.Code == "FIELD_VALUE" &&
            summary.KeyName == "Strength" &&
            summary.MatchReason == "current key");
    }

    [Fact]
    public void Summarize_CurrentSectionDiagnosticIsIncluded()
    {
        Ra2CurrentFileAiDiagnosticSummaryProvider provider = new();

        IReadOnlyList<Ra2AiDiagnosticSummary> summaries = provider.Summarize(
            [
                CreateIssue("REF_MISSING_TARGET", IniIssueSeverity.Warning, line: 8, section: "HTNK", key: "Primary"),
                CreateIssue("INI_STRUCTURE", IniIssueSeverity.Warning, line: 9, section: "Other", key: "Armor")
            ],
            currentFilePath: "rulesmd.ini",
            currentFileDisplayName: "rulesmd.ini",
            currentVersion: 1,
            caretLineNumber: 4,
            sectionName: "HTNK",
            keyName: "Strength",
            maxCount: 5);

        Assert.Contains(summaries, summary =>
            summary.Code == "REF_MISSING_TARGET" &&
            summary.SectionName == "HTNK" &&
            summary.MatchReason == "current section");
    }

    [Fact]
    public void Summarize_ResultCountIsBoundedByTopNAndHardCap()
    {
        IdeDiagnosticIssueViewModel[] issues = Enumerable.Range(1, 20)
            .Select(index => CreateIssue($"ISSUE_{index:00}", IniIssueSeverity.Warning, line: index, section: "HTNK", key: $"Key{index}"))
            .ToArray();
        Ra2CurrentFileAiDiagnosticSummaryProvider provider = new();

        IReadOnlyList<Ra2AiDiagnosticSummary> topFive = provider.Summarize(
            issues,
            currentFilePath: "rulesmd.ini",
            currentFileDisplayName: "rulesmd.ini",
            currentVersion: 1,
            caretLineNumber: 0,
            sectionName: "NoMatch",
            keyName: "NoMatch",
            maxCount: 5);
        IReadOnlyList<Ra2AiDiagnosticSummary> hardCapped = provider.Summarize(
            issues,
            currentFilePath: "rulesmd.ini",
            currentFileDisplayName: "rulesmd.ini",
            currentVersion: 1,
            caretLineNumber: 0,
            sectionName: "NoMatch",
            keyName: "NoMatch",
            maxCount: 50);

        Assert.Equal(5, topFive.Count);
        Assert.Equal(Ra2CurrentFileAiDiagnosticSummaryProvider.HardMaxDiagnosticCount, hardCapped.Count);
    }

    [Fact]
    public void Summarize_NoDiagnosticsReturnsEmptySafely()
    {
        Ra2CurrentFileAiDiagnosticSummaryProvider provider = new();

        IReadOnlyList<Ra2AiDiagnosticSummary> summaries = provider.Summarize(
            [],
            currentFilePath: "rulesmd.ini",
            currentFileDisplayName: "rulesmd.ini",
            currentVersion: 1,
            caretLineNumber: 4,
            sectionName: "HTNK",
            keyName: "Strength",
            maxCount: 5);

        Assert.Empty(summaries);
    }

    [Fact]
    public void Summarize_FiltersToCurrentFileAndVersionWithoutWholeProjectDump()
    {
        Ra2CurrentFileAiDiagnosticSummaryProvider provider = new();

        IReadOnlyList<Ra2AiDiagnosticSummary> summaries = provider.Summarize(
            [
                CreateIssue("CURRENT", IniIssueSeverity.Warning, line: 4, filePath: "rulesmd.ini", section: "HTNK", key: "Strength", version: 3),
                CreateIssue("OTHER_FILE", IniIssueSeverity.Error, line: 4, filePath: "artmd.ini", section: "HTNK", key: "Strength", version: 3),
                CreateIssue("STALE", IniIssueSeverity.Error, line: 4, filePath: "rulesmd.ini", section: "HTNK", key: "Strength", version: 2)
            ],
            currentFilePath: "rulesmd.ini",
            currentFileDisplayName: "rulesmd.ini",
            currentVersion: 3,
            caretLineNumber: 4,
            sectionName: "HTNK",
            keyName: "Strength",
            maxCount: 5);

        Ra2AiDiagnosticSummary summary = Assert.Single(summaries);
        Assert.Equal("CURRENT", summary.Code);
    }

    [Fact]
    public void Summarize_DoesNotMutateIssuesViewModel()
    {
        IssuesViewModel viewModel = new();
        viewModel.ReplaceIssues([
            CreateIssue("CURRENT", IniIssueSeverity.Warning, line: 4, section: "HTNK", key: "Strength")
        ]);
        IdeDiagnosticIssueViewModel selected = viewModel.Items[0];
        viewModel.SelectedIssue = selected;
        Ra2CurrentFileAiDiagnosticSummaryProvider provider = new();

        IReadOnlyList<Ra2AiDiagnosticSummary> summaries = provider.Summarize(
            viewModel.Items.ToArray(),
            currentFilePath: "rulesmd.ini",
            currentFileDisplayName: "rulesmd.ini",
            currentVersion: 1,
            caretLineNumber: 4,
            sectionName: "HTNK",
            keyName: "Strength",
            maxCount: 5);

        Assert.Single(summaries);
        Assert.Single(viewModel.Items);
        Assert.Same(selected, viewModel.SelectedIssue);
        Assert.Equal(1, viewModel.TotalCount);
        Assert.Equal(1, viewModel.FilteredCount);
    }

    private static IdeDiagnosticIssueViewModel CreateIssue(
        string code,
        IniIssueSeverity severity,
        int? line,
        string filePath = "rulesmd.ini",
        string? section = null,
        string? key = null,
        int version = 1)
        => new(
            code,
            "DiagnosticService",
            severity,
            $"{code} message",
            filePath,
            line,
            columnNumber: null,
            section,
            key,
            version);
}
