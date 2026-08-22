using System.IO;
using RA2IniEditor.Core;
using RA2IniEditor.IDE.ViewModels;

namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2CurrentFileAiDiagnosticSummaryProvider : IRa2AiDiagnosticSummaryProvider
{
    internal const int DefaultMaxDiagnosticCount = 5;
    internal const int HardMaxDiagnosticCount = 8;

    public IReadOnlyList<Ra2AiDiagnosticSummary> Summarize(
        IReadOnlyList<IdeDiagnosticIssueViewModel>? issues,
        string? currentFilePath,
        string? currentFileDisplayName,
        int currentVersion,
        int caretLineNumber,
        string? sectionName,
        string? keyName,
        int maxCount)
    {
        int effectiveMaxCount = NormalizeMaxCount(maxCount);
        if (issues is null || issues.Count == 0 || effectiveMaxCount <= 0)
            return [];

        List<Candidate> candidates = [];
        foreach (IdeDiagnosticIssueViewModel issue in issues)
        {
            if (!MatchesCurrentFile(issue, currentFilePath, currentFileDisplayName))
                continue;

            if (currentVersion > 0 && issue.Version > 0 && issue.Version != currentVersion)
                continue;

            Candidate? candidate = BuildCandidate(issue, caretLineNumber, sectionName, keyName);
            if (candidate is not null)
                candidates.Add(candidate);
        }

        return Array.AsReadOnly(candidates
            .OrderBy(candidate => candidate.Priority)
            .ThenBy(candidate => GetSeverityOrder(candidate.Issue.Severity))
            .ThenBy(candidate => candidate.Issue.LineNumber ?? int.MaxValue)
            .ThenBy(candidate => candidate.Issue.ColumnNumber ?? int.MaxValue)
            .ThenBy(candidate => candidate.Issue.Code, StringComparer.OrdinalIgnoreCase)
            .Take(effectiveMaxCount)
            .Select(CreateSummary)
            .ToArray());
    }

    private static int NormalizeMaxCount(int maxCount)
    {
        if (maxCount < 0)
            return 0;

        if (maxCount == 0)
            return DefaultMaxDiagnosticCount;

        return Math.Min(maxCount, HardMaxDiagnosticCount);
    }

    private static bool MatchesCurrentFile(
        IdeDiagnosticIssueViewModel issue,
        string? currentFilePath,
        string? currentFileDisplayName)
    {
        if (!string.IsNullOrWhiteSpace(currentFilePath) &&
            string.Equals(issue.FilePath, currentFilePath, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(currentFileDisplayName))
            return false;

        string issueFileName = Path.GetFileName(issue.FilePath);
        return string.Equals(issueFileName, currentFileDisplayName, StringComparison.OrdinalIgnoreCase);
    }

    private static Candidate? BuildCandidate(
        IdeDiagnosticIssueViewModel issue,
        int caretLineNumber,
        string? sectionName,
        string? keyName)
    {
        if (caretLineNumber > 0 && issue.LineNumber == caretLineNumber)
            return new Candidate(issue, Priority: 0, "current line");

        if (!string.IsNullOrWhiteSpace(keyName) &&
            string.Equals(issue.Key, keyName, StringComparison.OrdinalIgnoreCase))
        {
            return new Candidate(issue, Priority: 1, "current key");
        }

        if (!string.IsNullOrWhiteSpace(sectionName) &&
            string.Equals(issue.SectionId, sectionName, StringComparison.OrdinalIgnoreCase))
        {
            return new Candidate(issue, Priority: 2, "current section");
        }

        return new Candidate(issue, Priority: 3, "current file");
    }

    private static Ra2AiDiagnosticSummary CreateSummary(Candidate candidate)
    {
        IdeDiagnosticIssueViewModel issue = candidate.Issue;
        return new Ra2AiDiagnosticSummary(
            issue.Code,
            issue.SeverityText,
            issue.Message,
            issue.LineNumber,
            issue.SectionId,
            issue.Key,
            issue.SourceText,
            candidate.MatchReason);
    }

    private static int GetSeverityOrder(IniIssueSeverity severity) => severity switch
    {
        IniIssueSeverity.Error => 0,
        IniIssueSeverity.Warning => 1,
        _ => 2
    };

    private sealed record Candidate(
        IdeDiagnosticIssueViewModel Issue,
        int Priority,
        string MatchReason);
}
