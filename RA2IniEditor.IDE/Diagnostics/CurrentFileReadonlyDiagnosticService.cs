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

    private const string StructureIssueCode = "INI_STRUCTURE";
    private const string SourceKind = "CoreParserValidator";
    private const string DiagnosticServiceSourceKind = "DiagnosticService";
    private readonly IRa2DocumentSemanticModelBuilder _semanticModelBuilder;
    private readonly Ra2FieldDiagnosticService _fieldDiagnosticService;
    private readonly Ra2ReferenceDiagnosticCatalogBuilder _referenceCatalogBuilder;
    private readonly Ra2ReferenceDiagnosticService _referenceDiagnosticService;
    private readonly Ra2ChainDiagnosticService _chainDiagnosticService;

    public CurrentFileReadonlyDiagnosticService()
        : this(
            new Ra2DocumentSemanticModelBuilder(),
            new Ra2FieldDiagnosticService(),
            new Ra2ReferenceDiagnosticCatalogBuilder(),
            new Ra2ReferenceDiagnosticService(),
            new Ra2ChainDiagnosticService())
    {
    }

    internal CurrentFileReadonlyDiagnosticService(
        IRa2DocumentSemanticModelBuilder semanticModelBuilder,
        Ra2FieldDiagnosticService fieldDiagnosticService,
        Ra2ReferenceDiagnosticCatalogBuilder referenceCatalogBuilder,
        Ra2ReferenceDiagnosticService referenceDiagnosticService,
        Ra2ChainDiagnosticService? chainDiagnosticService = null)
    {
        _semanticModelBuilder = semanticModelBuilder ?? throw new ArgumentNullException(nameof(semanticModelBuilder));
        _fieldDiagnosticService = fieldDiagnosticService ?? throw new ArgumentNullException(nameof(fieldDiagnosticService));
        _referenceCatalogBuilder = referenceCatalogBuilder ?? throw new ArgumentNullException(nameof(referenceCatalogBuilder));
        _referenceDiagnosticService = referenceDiagnosticService ?? throw new ArgumentNullException(nameof(referenceDiagnosticService));
        _chainDiagnosticService = chainDiagnosticService ?? new Ra2ChainDiagnosticService();
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
            IniDocument document = IniParser.Parse(snapshot.Text, snapshot.FilePath);
            List<IniIssue> issues = IniValidator.Validate(document);
            List<IdeDiagnosticIssueViewModel> result = issues
                .Select(issue => new IdeDiagnosticIssueViewModel(
                    StructureIssueCode,
                    SourceKind,
                    issue.Severity,
                    issue.Message,
                    issue.FilePath ?? snapshot.FilePath,
                    issue.IsNavigable ? issue.LineNumber : null,
                    null,
                    issue.SectionName,
                    issue.Key,
                    snapshot.Version))
                .ToList();

            if (fieldProvider is not null)
            {
                Ra2DocumentSnapshot documentSnapshot = new(snapshot.FilePath, snapshot.Text, snapshot.Version);
                Ra2DocumentSemanticModel semanticModel = _semanticModelBuilder.Build(documentSnapshot, fieldProvider);
                result.AddRange(_fieldDiagnosticService.AnalyzeCurrentDocument(snapshot, semanticModel, fieldProvider));
                Ra2ReferenceDiagnosticCatalog catalog = referenceCatalog ?? _referenceCatalogBuilder.BuildFromCurrentDocument(snapshot.FilePath, semanticModel);
                List<IdeDiagnosticIssueViewModel> referenceIssues = _referenceDiagnosticService
                    .AnalyzeCurrentDocument(snapshot, semanticModel, fieldProvider, catalog, referenceScopeLabel)
                    .ToList();
                List<IdeDiagnosticIssueViewModel> chainIssues = _chainDiagnosticService
                    .AnalyzeCurrentDocument(snapshot, semanticModel, catalog, referenceScopeLabel)
                    .ToList();
                result.AddRange(referenceIssues.Where(issue => !HasMatchingChainIssue(issue, chainIssues)));
                result.AddRange(chainIssues);
            }

            return result;
        }
        catch (Exception ex)
        {
            return [CreateDiagnosticExceptionIssue(snapshot, ex)];
        }
    }

    private static bool HasMatchingChainIssue(
        IdeDiagnosticIssueViewModel referenceIssue,
        IReadOnlyCollection<IdeDiagnosticIssueViewModel> chainIssues)
        => referenceIssue.Code == Ra2ReferenceDiagnosticService.MissingTargetCode &&
           chainIssues.Any(chainIssue =>
               chainIssue.LineNumber == referenceIssue.LineNumber &&
               chainIssue.ColumnNumber == referenceIssue.ColumnNumber &&
               string.Equals(chainIssue.SectionId, referenceIssue.SectionId, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(chainIssue.Key, referenceIssue.Key, StringComparison.OrdinalIgnoreCase));

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
