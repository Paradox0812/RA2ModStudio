using RA2IniEditor.IDE.ViewModels;

namespace RA2IniEditor.IDE.AI;

internal interface IRa2AiDiagnosticSummaryProvider
{
    IReadOnlyList<Ra2AiDiagnosticSummary> Summarize(
        IReadOnlyList<IdeDiagnosticIssueViewModel>? issues,
        string? currentFilePath,
        string? currentFileDisplayName,
        int currentVersion,
        int caretLineNumber,
        string? sectionName,
        string? keyName,
        int maxCount);
}
