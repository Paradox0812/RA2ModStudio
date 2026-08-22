using RA2IniEditor.Core;

namespace RA2IniEditor.Application.Diagnostics;

/// <summary>
/// 表示不依赖 UI ViewModel 的只读诊断事实。
/// </summary>
internal sealed class Ra2DiagnosticFact
{
    internal Ra2DiagnosticFact(
        string code,
        string sourceKind,
        IniIssueSeverity severity,
        string message,
        string filePath,
        int? lineNumber,
        int? columnNumber,
        string? sectionId,
        string? key,
        int analysisVersion)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        SourceKind = sourceKind ?? throw new ArgumentNullException(nameof(sourceKind));
        Severity = severity;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        LineNumber = lineNumber;
        ColumnNumber = columnNumber;
        SectionId = sectionId;
        Key = key;
        AnalysisVersion = analysisVersion;
    }

    public string Code { get; }

    public string SourceKind { get; }

    public IniIssueSeverity Severity { get; }

    public string Message { get; }

    public string FilePath { get; }

    public int? LineNumber { get; }

    public int? ColumnNumber { get; }

    public string? SectionId { get; }

    public string? Key { get; }

    public int AnalysisVersion { get; }
}
