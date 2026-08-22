using RA2IniEditor.Core;

namespace RA2IniEditor.Application.Automation.Experimental;

public enum Ra2AutomationDocumentDiagnosticsFailureKind
{
    None = 0,
    DocumentTooLarge = 1,
    ResultLimitExceeded = 2,
    Canceled = 3,
    AnalysisFailed = 4
}

public sealed class Ra2AutomationDocumentDiagnosticsResult
{
    internal Ra2AutomationDocumentDiagnosticsResult(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationDocumentDiagnosticsFailureKind failureKind,
        string message,
        IReadOnlyList<Ra2AutomationDiagnosticFact> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("A result message is required.", nameof(message));

        ArgumentNullException.ThrowIfNull(diagnostics);
        bool succeeded = failureKind == Ra2AutomationDocumentDiagnosticsFailureKind.None;
        if (!succeeded && diagnostics.Count != 0)
            throw new ArgumentException("Failure results cannot contain diagnostics.", nameof(diagnostics));

        Succeeded = succeeded;
        FailureKind = failureKind;
        Message = message;
        DocumentId = snapshot.DocumentId;
        Version = snapshot.Version;
        FilePath = snapshot.FilePath;
        FieldRegistryRevision = snapshot.FieldRegistry.Revision;
        Diagnostics = Array.AsReadOnly(diagnostics.ToArray());
    }

    public bool Succeeded { get; }
    public Ra2AutomationDocumentDiagnosticsFailureKind FailureKind { get; }
    public string Message { get; }
    public Guid DocumentId { get; }
    public int Version { get; }
    public string FilePath { get; }
    public long FieldRegistryRevision { get; }
    public IReadOnlyList<Ra2AutomationDiagnosticFact> Diagnostics { get; }
}

public sealed class Ra2AutomationDiagnosticFact
{
    internal Ra2AutomationDiagnosticFact(
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
