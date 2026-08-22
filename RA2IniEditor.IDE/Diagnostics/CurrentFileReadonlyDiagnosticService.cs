using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.ViewModels;

namespace RA2IniEditor.IDE.Diagnostics;

/// <summary>
/// Runs readonly structure diagnostics against the current source editor snapshot.
/// </summary>
public sealed class CurrentFileReadonlyDiagnosticService
{
    public const string DiagnosticExceptionCode = "DIAGNOSTIC_EXCEPTION";

    private const string DiagnosticServiceSourceKind = "DiagnosticService";
    private readonly Ra2DocumentDiagnosticService _diagnosticService;

    public CurrentFileReadonlyDiagnosticService()
        : this(new Ra2DocumentDiagnosticService())
    {
    }

    internal CurrentFileReadonlyDiagnosticService(Ra2DocumentDiagnosticService diagnosticService)
    {
        _diagnosticService = diagnosticService ?? throw new ArgumentNullException(nameof(diagnosticService));
    }

    /// <summary>
    /// Analyzes the current loaded source snapshot.
    /// </summary>
    public IReadOnlyList<IdeDiagnosticIssueViewModel> Analyze(
        CurrentSourceSnapshot? snapshot,
        IRa2FieldDefinitionProvider? fieldProvider = null)
        => AnalyzeCore(snapshot, fieldProvider, referenceCatalog: null, referenceScopeLabel: "当前文件");

    internal IReadOnlyList<IdeDiagnosticIssueViewModel> AnalyzeWithReferenceCatalog(
        CurrentSourceSnapshot? snapshot,
        IRa2FieldDefinitionProvider? fieldProvider,
        Ra2ReferenceDiagnosticCatalog referenceCatalog,
        string referenceScopeLabel)
        => AnalyzeCore(snapshot, fieldProvider, referenceCatalog, referenceScopeLabel);

    private IReadOnlyList<IdeDiagnosticIssueViewModel> AnalyzeCore(
        CurrentSourceSnapshot? snapshot,
        IRa2FieldDefinitionProvider? fieldProvider,
        Ra2ReferenceDiagnosticCatalog? referenceCatalog,
        string referenceScopeLabel)
    {
        if (snapshot is null)
            return [CreateDiagnosticExceptionIssue(null, new ArgumentNullException(nameof(snapshot)))];

        if (!snapshot.CanRunDiagnostics)
            return [];

        try
        {
            Ra2DocumentSnapshot documentSnapshot = new(snapshot.FilePath, snapshot.Text, snapshot.Version);
            return _diagnosticService.Analyze(
                    documentSnapshot,
                    fieldProvider,
                    referenceCatalog,
                    referenceScopeLabel,
                    CancellationToken.None,
                    int.MaxValue)
                .Select(ToViewModel)
                .ToArray();
        }
        catch (Exception ex)
        {
            return [CreateDiagnosticExceptionIssue(snapshot, ex)];
        }
    }

    private static IdeDiagnosticIssueViewModel ToViewModel(Ra2DiagnosticFact fact)
        => new(
            fact.Code,
            fact.SourceKind,
            fact.Severity,
            fact.Message,
            fact.FilePath,
            fact.LineNumber,
            fact.ColumnNumber,
            fact.SectionId,
            fact.Key,
            fact.AnalysisVersion);

    private static IdeDiagnosticIssueViewModel CreateDiagnosticExceptionIssue(CurrentSourceSnapshot? snapshot, Exception exception)
    {
        string message = string.IsNullOrWhiteSpace(exception.Message)
            ? "Diagnostics failed."
            : $"Diagnostics failed: {exception.Message}";

        return new IdeDiagnosticIssueViewModel(
            DiagnosticExceptionCode,
            DiagnosticServiceSourceKind,
            IniIssueSeverity.Error,
            message,
            snapshot?.FilePath ?? string.Empty,
            null,
            null,
            null,
            null,
            snapshot?.Version ?? 0);
    }
}
