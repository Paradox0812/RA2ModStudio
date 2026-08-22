using RA2IniEditor.Core;

namespace RA2IniEditor.IDE.ViewModels;

/// <summary>
/// Represents one readonly diagnostic result shown in the IDE Issues panel.
/// </summary>
public sealed class IdeDiagnosticIssueViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdeDiagnosticIssueViewModel"/> class.
    /// </summary>
    public IdeDiagnosticIssueViewModel(
        string code,
        string sourceKind,
        IniIssueSeverity severity,
        string message,
        string filePath,
        int? lineNumber,
        int? columnNumber,
        string? sectionId,
        string? key,
        int version)
    {
        Code = code;
        SourceKind = sourceKind;
        Severity = severity;
        Message = message;
        FilePath = filePath;
        LineNumber = lineNumber;
        ColumnNumber = columnNumber;
        SectionId = sectionId;
        Key = key;
        Version = version;
    }

    public string Code { get; }

    public string SourceKind { get; }

    public IniIssueSeverity Severity { get; }

    public string SeverityText => Severity.ToString();

    public string SeverityMarker => Severity switch
    {
        IniIssueSeverity.Error => "E",
        IniIssueSeverity.Warning => "W",
        _ => "I"
    };

    public string Message { get; }

    public string FilePath { get; }

    public int? LineNumber { get; }

    public int? ColumnNumber { get; }

    public string? SectionId { get; }

    public string? Key { get; }

    public int Version { get; }

    public string SourceText => SourceKind switch
    {
        "CoreParser" => "Parser",
        "CoreValidator" => "Validator",
        "CoreParserValidator" => "Parser / Validator",
        "DiagnosticService" => "Diagnostic Service",
        _ => SourceKind
    };

    public string LocationText => LineNumber is null
        ? "-"
        : ColumnNumber is null
            ? $"Line {LineNumber}"
            : $"Line {LineNumber}, Col {ColumnNumber}";
}
