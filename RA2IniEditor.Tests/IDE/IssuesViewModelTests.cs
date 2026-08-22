using RA2IniEditor.Core;
using RA2IniEditor.IDE.ViewModels;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class IssuesViewModelTests
{
    [Fact]
    public void Clear_RemovesItemsSelectionAndUpdatesStatus()
    {
        IssuesViewModel viewModel = new();
        IdeDiagnosticIssueViewModel issue = CreateIssue(1);
        viewModel.ReplaceIssues([issue]);
        viewModel.SelectedIssue = issue;

        viewModel.Clear(IssuesStatusMessages.SkippedDeferredLargeFile);

        Assert.Empty(viewModel.Items);
        Assert.Null(viewModel.SelectedIssue);
        Assert.Equal(IssuesStatusMessages.SkippedDeferredLargeFile, viewModel.StatusText);
    }

    [Fact]
    public void ReplaceIssues_ReplacesItemsClearsSelectionAndReportsCount()
    {
        IssuesViewModel viewModel = new();

        viewModel.ReplaceIssues([CreateIssue(1), CreateIssue(2)]);

        Assert.Equal(2, viewModel.Items.Count);
        Assert.Null(viewModel.SelectedIssue);
        Assert.Equal("Found 2 issues.", viewModel.StatusText);
        Assert.Equal(2, viewModel.TotalCount);
        Assert.Equal(2, viewModel.FilteredCount);
    }

    [Fact]
    public void ReplaceIssues_ReportsEmptyAndSingleCounts()
    {
        IssuesViewModel viewModel = new();

        viewModel.ReplaceIssues([]);

        Assert.Empty(viewModel.Items);
        Assert.Equal(IssuesStatusMessages.NoIssuesFound, viewModel.StatusText);

        viewModel.ReplaceIssues([CreateIssue(1)]);

        Assert.Single(viewModel.Items);
        Assert.Equal("Found 1 issue.", viewModel.StatusText);
    }

    [Theory]
    [InlineData(IssuesStatusMessages.NoFileSelected)]
    [InlineData(IssuesStatusMessages.Pending)]
    [InlineData(IssuesStatusMessages.SkippedDeferredLargeFile)]
    [InlineData(IssuesStatusMessages.SkippedReadFailed)]
    [InlineData(IssuesStatusMessages.SkippedSourceNotLoaded)]
    [InlineData(IssuesStatusMessages.Failed)]
    [InlineData(IssuesStatusMessages.SkippedStaleResult)]
    public void Clear_UsesCentralizedStatusMessages(string statusText)
    {
        IssuesViewModel viewModel = new();
        viewModel.ReplaceIssues([CreateIssue(1)]);
        viewModel.SelectedIssue = viewModel.Items[0];

        viewModel.Clear(statusText);

        Assert.Empty(viewModel.Items);
        Assert.Null(viewModel.SelectedIssue);
        Assert.Equal(statusText, viewModel.StatusText);
    }

    [Fact]
    public void IdeDiagnosticIssueViewModel_FormatsLocationText()
    {
        Assert.Equal("-", CreateIssue(null).LocationText);
        Assert.Equal("Line 3", CreateIssue(3).LocationText);

        IdeDiagnosticIssueViewModel issue = new(
            "INI_STRUCTURE",
            "CoreParserValidator",
            IniIssueSeverity.Warning,
            "message",
            "rules.ini",
            3,
            4,
            null,
            null,
            1);

        Assert.Equal("Line 3, Col 4", issue.LocationText);
    }

    [Fact]
    public void IdeDiagnosticIssueViewModel_FormatsSeverityAndSourceText()
    {
        IdeDiagnosticIssueViewModel warning = CreateIssue(3);
        IdeDiagnosticIssueViewModel parser = CreateIssueWithSource("CoreParser");
        IdeDiagnosticIssueViewModel validator = CreateIssueWithSource("CoreValidator");
        IdeDiagnosticIssueViewModel combined = CreateIssueWithSource("CoreParserValidator");
        IdeDiagnosticIssueViewModel diagnosticService = CreateIssueWithSource("DiagnosticService");
        IdeDiagnosticIssueViewModel custom = CreateIssueWithSource("CustomSource");

        Assert.Equal("Warning", warning.SeverityText);
        Assert.False(string.IsNullOrWhiteSpace(warning.SeverityMarker));
        Assert.Equal("Parser", parser.SourceText);
        Assert.Equal("Validator", validator.SourceText);
        Assert.Equal("Parser / Validator", combined.SourceText);
        Assert.Equal("Diagnostic Service", diagnosticService.SourceText);
        Assert.Equal("CustomSource", custom.SourceText);
    }

    [Fact]
    public void Filters_UpdateVisibleItemsWithoutDroppingStoredIssues()
    {
        IssuesViewModel viewModel = new();
        viewModel.ReplaceIssues(
        [
            CreateIssueWithSource("CoreParserValidator", IniIssueSeverity.Error, "rules.ini", "duplicate key"),
            CreateIssueWithSource("DiagnosticService", IniIssueSeverity.Warning, "art.ini", "parse warning")
        ]);

        viewModel.SelectedSeverityFilter = IssuesSeverityFilterNames.Error;

        var issue = Assert.Single(viewModel.Items);
        Assert.Equal(IniIssueSeverity.Error, issue.Severity);
        Assert.Equal(2, viewModel.TotalCount);
        Assert.Equal(1, viewModel.FilteredCount);
        Assert.Equal("Showing 1 of 2 issues.", viewModel.StatusText);

        viewModel.SelectedSeverityFilter = IssuesSeverityFilterNames.All;
        viewModel.SourceFilterText = "art";

        issue = Assert.Single(viewModel.Items);
        Assert.Equal("art.ini", issue.FilePath);

        viewModel.ClearFilters();

        Assert.Equal(2, viewModel.Items.Count);
        Assert.Equal(IssuesSeverityFilterNames.All, viewModel.SelectedSeverityFilter);
        Assert.Equal(string.Empty, viewModel.SourceFilterText);
        Assert.Equal(string.Empty, viewModel.SearchText);
    }

    [Fact]
    public void Filters_FieldDiagnosticsBySourceAndSearchText()
    {
        IssuesViewModel viewModel = new();
        viewModel.ReplaceIssues(
        [
            CreateIssueWithCodeAndSource("FIELD_UNKNOWN_KEY", "Field", IniIssueSeverity.Warning, "rules.ini", "未知字段：ArmorX"),
            CreateIssueWithCodeAndSource("INI_STRUCTURE", "CoreParserValidator", IniIssueSeverity.Warning, "rules.ini", "duplicate key")
        ]);

        viewModel.SourceFilterText = "Field";

        var issue = Assert.Single(viewModel.Items);
        Assert.Equal("FIELD_UNKNOWN_KEY", issue.Code);

        viewModel.SourceFilterText = string.Empty;
        viewModel.SearchText = "未知字段";

        issue = Assert.Single(viewModel.Items);
        Assert.Equal("Field", issue.SourceText);
    }

    [Fact]
    public void Filters_ReferenceDiagnosticsBySource()
    {
        IssuesViewModel viewModel = new();
        viewModel.ReplaceIssues(
        [
            CreateIssueWithCodeAndSource("REF_MISSING_TARGET", "Reference", IniIssueSeverity.Warning, "rules.ini", "引用目标可能不存在：MissingWeapon"),
            CreateIssueWithCodeAndSource("FIELD_UNKNOWN_KEY", "Field", IniIssueSeverity.Warning, "rules.ini", "未知字段：ArmorX")
        ]);

        viewModel.SourceFilterText = "Reference";

        var issue = Assert.Single(viewModel.Items);
        Assert.Equal("REF_MISSING_TARGET", issue.Code);
        Assert.Equal("Reference", issue.SourceText);
    }

    [Fact]
    public void ReplaceIssues_StableSortsAndDeduplicates()
    {
        IssuesViewModel viewModel = new();
        IdeDiagnosticIssueViewModel duplicateA = CreateIssueWithSource("CoreParserValidator", IniIssueSeverity.Warning, "rules.ini", "same");
        IdeDiagnosticIssueViewModel duplicateB = CreateIssueWithSource("CoreParserValidator", IniIssueSeverity.Warning, "rules.ini", "same");
        IdeDiagnosticIssueViewModel error = CreateIssueWithSource("DiagnosticService", IniIssueSeverity.Error, "art.ini", "error");

        viewModel.ReplaceIssues([duplicateA, error, duplicateB]);

        Assert.Equal(2, viewModel.Items.Count);
        Assert.Equal(IniIssueSeverity.Error, viewModel.Items[0].Severity);
        Assert.Equal(IniIssueSeverity.Warning, viewModel.Items[1].Severity);
    }

    private static IdeDiagnosticIssueViewModel CreateIssue(int? lineNumber)
        => new(
            "INI_STRUCTURE",
            "CoreParserValidator",
            IniIssueSeverity.Warning,
            "message",
            "rules.ini",
            lineNumber,
            null,
            null,
            null,
            1);

    private static IdeDiagnosticIssueViewModel CreateIssueWithSource(string sourceKind)
        => new(
            "INI_STRUCTURE",
            sourceKind,
            IniIssueSeverity.Warning,
            "message",
            "rules.ini",
            3,
            null,
            null,
            null,
            1);

    private static IdeDiagnosticIssueViewModel CreateIssueWithSource(
        string sourceKind,
        IniIssueSeverity severity,
        string filePath,
        string message)
        => new(
            "INI_STRUCTURE",
            sourceKind,
            severity,
            message,
            filePath,
            3,
            null,
            null,
            null,
            1);

    private static IdeDiagnosticIssueViewModel CreateIssueWithCodeAndSource(
        string code,
        string sourceKind,
        IniIssueSeverity severity,
        string filePath,
        string message)
        => new(
            code,
            sourceKind,
            severity,
            message,
            filePath,
            3,
            null,
            null,
            null,
            1);
}
