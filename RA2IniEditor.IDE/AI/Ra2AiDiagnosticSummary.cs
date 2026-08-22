namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2AiDiagnosticSummary
{
    public Ra2AiDiagnosticSummary(
        string code,
        string severity,
        string message,
        int? lineNumber,
        string? sectionName,
        string? keyName,
        string? source,
        string matchReason)
    {
        Code = string.IsNullOrWhiteSpace(code) ? string.Empty : code.Trim();
        Severity = string.IsNullOrWhiteSpace(severity) ? string.Empty : severity.Trim();
        Message = string.IsNullOrWhiteSpace(message) ? string.Empty : message.Trim();
        LineNumber = lineNumber;
        SectionName = string.IsNullOrWhiteSpace(sectionName) ? null : sectionName.Trim();
        KeyName = string.IsNullOrWhiteSpace(keyName) ? null : keyName.Trim();
        Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim();
        MatchReason = string.IsNullOrWhiteSpace(matchReason) ? "file" : matchReason.Trim();
    }

    public string Code { get; }

    public string Severity { get; }

    public string Message { get; }

    public int? LineNumber { get; }

    public string? SectionName { get; }

    public string? KeyName { get; }

    public string? Source { get; }

    public string MatchReason { get; }
}
