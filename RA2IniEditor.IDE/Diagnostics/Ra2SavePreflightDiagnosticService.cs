using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.ViewModels;

namespace RA2IniEditor.IDE.Diagnostics;

internal sealed class Ra2SavePreflightDiagnosticService
{
    private readonly CurrentFileReadonlyDiagnosticService _diagnosticService;

    public Ra2SavePreflightDiagnosticService()
        : this(new CurrentFileReadonlyDiagnosticService())
    {
    }

    internal Ra2SavePreflightDiagnosticService(CurrentFileReadonlyDiagnosticService diagnosticService)
    {
        _diagnosticService = diagnosticService ?? throw new ArgumentNullException(nameof(diagnosticService));
    }

    public Ra2SavePreflightResult Analyze(
        CurrentSourceSnapshot? snapshot,
        string currentText,
        IRa2FieldDefinitionProvider? fieldProvider)
    {
        if (snapshot is null || !snapshot.CanRunDiagnostics)
            return Ra2SavePreflightResult.NotRun();

        CurrentSourceSnapshot diagnosticSnapshot = new(
            snapshot.ProjectRootPath,
            snapshot.FilePath,
            snapshot.FileName,
            currentText ?? string.Empty,
            snapshot.Version,
            snapshot.State,
            snapshot.EncodingMetadata);

        IReadOnlyList<IdeDiagnosticIssueViewModel> issues = _diagnosticService.Analyze(diagnosticSnapshot, fieldProvider);
        return Ra2SavePreflightResult.FromIssues(issues);
    }
}

internal sealed class Ra2SavePreflightResult
{
    private Ra2SavePreflightResult(bool wasRun, IReadOnlyList<IdeDiagnosticIssueViewModel> issues)
    {
        WasRun = wasRun;
        Issues = issues;
        ErrorCount = issues.Count(issue => issue.Severity == IniIssueSeverity.Error);
        WarningCount = issues.Count(issue => issue.Severity == IniIssueSeverity.Warning);
        InfoCount = issues.Count - ErrorCount - WarningCount;
        SourceSummaries = BuildSourceSummaries(issues);
    }

    public bool WasRun { get; }

    public IReadOnlyList<IdeDiagnosticIssueViewModel> Issues { get; }

    public int IssueCount => Issues.Count;

    public int ErrorCount { get; }

    public int WarningCount { get; }

    public int InfoCount { get; }

    public IReadOnlyList<Ra2SavePreflightSourceSummary> SourceSummaries { get; }

    public bool HasIssues => IssueCount > 0;

    public string SummaryText => !WasRun
        ? "保存前检查未运行。"
        : HasIssues
            ? $"保存前检查发现 {IssueCount} 个可能问题。"
            : "保存前检查完成，未发现问题。";

    public string SourceSummaryText => SourceSummaries.Count == 0
        ? "来源汇总：无。"
        : string.Join(Environment.NewLine, SourceSummaries.Select(summary => $"{summary.DisplayName}：{summary.Count}"));

    public string SeveritySummaryText
    {
        get
        {
            List<string> parts = [];
            if (ErrorCount > 0)
                parts.Add($"错误：{ErrorCount}");
            if (WarningCount > 0)
                parts.Add($"警告：{WarningCount}");
            if (InfoCount > 0)
                parts.Add($"信息：{InfoCount}");

            return parts.Count == 0 ? "严重度汇总：无。" : string.Join("    ", parts);
        }
    }

    public static Ra2SavePreflightResult NotRun()
        => new(false, []);

    public static Ra2SavePreflightResult FromIssues(IReadOnlyList<IdeDiagnosticIssueViewModel> issues)
        => new(true, issues ?? []);

    private static IReadOnlyList<Ra2SavePreflightSourceSummary> BuildSourceSummaries(
        IReadOnlyList<IdeDiagnosticIssueViewModel> issues)
        => issues
            .GroupBy(issue => NormalizeSourceKind(issue.SourceKind), StringComparer.OrdinalIgnoreCase)
            .Select(group => new Ra2SavePreflightSourceSummary(group.Key, GetSourceDisplayName(group.Key), group.Count()))
            .OrderBy(summary => GetSourceSortOrder(summary.SourceKind))
            .ThenBy(summary => summary.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static string NormalizeSourceKind(string? sourceKind)
        => sourceKind switch
        {
            "CoreParser" or "CoreValidator" or "CoreParserValidator" => "Structure",
            "Field" => "Field",
            "Reference" => "Reference",
            "DiagnosticService" => "DiagnosticService",
            _ => "Other"
        };

    private static string GetSourceDisplayName(string sourceKind)
        => sourceKind switch
        {
            "Structure" => "结构问题",
            "Field" => "字段问题",
            "Reference" => "引用问题",
            "DiagnosticService" => "诊断服务问题",
            _ => "其他问题"
        };

    private static int GetSourceSortOrder(string sourceKind)
        => sourceKind switch
        {
            "Structure" => 0,
            "Field" => 1,
            "Reference" => 2,
            "DiagnosticService" => 3,
            _ => 4
        };
}

internal sealed record Ra2SavePreflightSourceSummary(string SourceKind, string DisplayName, int Count);
