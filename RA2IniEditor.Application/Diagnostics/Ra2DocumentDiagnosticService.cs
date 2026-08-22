using RA2IniEditor.Application.Language;
using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Diagnostics;

/// <summary>
/// Composes the canonical current-document diagnostic rules without host or UI dependencies.
/// </summary>
internal sealed class Ra2DocumentDiagnosticService
{
    internal const int CancellationCheckInterval = 256;

    private const string StructureIssueCode = "INI_STRUCTURE";
    private const string StructureSourceKind = "CoreParserValidator";

    private readonly IRa2DocumentSemanticModelBuilder _semanticModelBuilder;
    private readonly Ra2FieldDiagnosticService _fieldDiagnosticService;
    private readonly Ra2ReferenceDiagnosticCatalogBuilder _referenceCatalogBuilder;
    private readonly Ra2ReferenceDiagnosticService _referenceDiagnosticService;
    private readonly Ra2ChainDiagnosticService _chainDiagnosticService;

    public Ra2DocumentDiagnosticService()
        : this(
            new Ra2DocumentSemanticModelBuilder(),
            new Ra2FieldDiagnosticService(),
            new Ra2ReferenceDiagnosticCatalogBuilder(),
            new Ra2ReferenceDiagnosticService(),
            new Ra2ChainDiagnosticService())
    {
    }

    internal Ra2DocumentDiagnosticService(
        IRa2DocumentSemanticModelBuilder semanticModelBuilder,
        Ra2FieldDiagnosticService fieldDiagnosticService,
        Ra2ReferenceDiagnosticCatalogBuilder referenceCatalogBuilder,
        Ra2ReferenceDiagnosticService referenceDiagnosticService,
        Ra2ChainDiagnosticService chainDiagnosticService)
    {
        _semanticModelBuilder = semanticModelBuilder ?? throw new ArgumentNullException(nameof(semanticModelBuilder));
        _fieldDiagnosticService = fieldDiagnosticService ?? throw new ArgumentNullException(nameof(fieldDiagnosticService));
        _referenceCatalogBuilder = referenceCatalogBuilder ?? throw new ArgumentNullException(nameof(referenceCatalogBuilder));
        _referenceDiagnosticService = referenceDiagnosticService ?? throw new ArgumentNullException(nameof(referenceDiagnosticService));
        _chainDiagnosticService = chainDiagnosticService ?? throw new ArgumentNullException(nameof(chainDiagnosticService));
    }

    public IReadOnlyList<Ra2DiagnosticFact> Analyze(
        Ra2DocumentSnapshot snapshot,
        IRa2FieldDefinitionProvider? fieldProvider = null,
        Ra2ReferenceDiagnosticCatalog? referenceCatalog = null,
        string referenceScopeLabel = "当前文件",
        CancellationToken cancellationToken = default,
        int maximumResultItems = int.MaxValue)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentOutOfRangeException.ThrowIfNegative(maximumResultItems);

        cancellationToken.ThrowIfCancellationRequested();
        IniDocument document = IniParser.Parse(snapshot.Text, snapshot.FilePath);
        List<IniIssue> structureIssues = IniValidator.Validate(document);
        cancellationToken.ThrowIfCancellationRequested();

        List<Ra2DiagnosticFact> result = [];
        for (int index = 0; index < structureIssues.Count; index++)
        {
            CheckCancellation(index, cancellationToken);
            IniIssue issue = structureIssues[index];
            Add(result, new Ra2DiagnosticFact(
                StructureIssueCode,
                StructureSourceKind,
                issue.Severity,
                issue.Message,
                issue.FilePath ?? snapshot.FilePath ?? string.Empty,
                issue.IsNavigable ? issue.LineNumber : null,
                null,
                issue.SectionName,
                issue.Key,
                snapshot.Version), maximumResultItems);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (fieldProvider is null)
            return result;

        Ra2DocumentSemanticModel semanticModel = _semanticModelBuilder.Build(snapshot, fieldProvider);
        cancellationToken.ThrowIfCancellationRequested();

        Append(result, _fieldDiagnosticService.AnalyzeCurrentDocument(
            snapshot,
            semanticModel,
            fieldProvider,
            cancellationToken,
            maximumResultItems), maximumResultItems, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        Ra2ReferenceDiagnosticCatalog catalog = referenceCatalog ??
            _referenceCatalogBuilder.BuildFromCurrentDocument(snapshot.FilePath ?? string.Empty, semanticModel);
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<Ra2DiagnosticFact> referenceIssues = _referenceDiagnosticService.AnalyzeCurrentDocument(
            snapshot,
            semanticModel,
            fieldProvider,
            catalog,
            referenceScopeLabel,
            cancellationToken,
            maximumResultItems);
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<Ra2DiagnosticFact> chainIssues = _chainDiagnosticService.AnalyzeCurrentDocument(
            snapshot,
            semanticModel,
            catalog,
            referenceScopeLabel,
            cancellationToken,
            maximumResultItems);

        int referenceIndex = 0;
        foreach (Ra2DiagnosticFact referenceIssue in referenceIssues)
        {
            CheckCancellation(referenceIndex++, cancellationToken);
            if (!HasMatchingChainIssue(referenceIssue, chainIssues))
                Add(result, referenceIssue, maximumResultItems);
        }

        Append(result, chainIssues, maximumResultItems, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    private static bool HasMatchingChainIssue(
        Ra2DiagnosticFact referenceIssue,
        IReadOnlyCollection<Ra2DiagnosticFact> chainIssues)
        => referenceIssue.Code == Ra2ReferenceDiagnosticService.MissingTargetCode &&
           chainIssues.Any(chainIssue =>
               chainIssue.LineNumber == referenceIssue.LineNumber &&
               chainIssue.ColumnNumber == referenceIssue.ColumnNumber &&
               string.Equals(chainIssue.SectionId, referenceIssue.SectionId, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(chainIssue.Key, referenceIssue.Key, StringComparison.OrdinalIgnoreCase));

    private static void Append(
        List<Ra2DiagnosticFact> target,
        IEnumerable<Ra2DiagnosticFact> source,
        int maximumResultItems,
        CancellationToken cancellationToken)
    {
        int index = 0;
        foreach (Ra2DiagnosticFact fact in source)
        {
            CheckCancellation(index++, cancellationToken);
            Add(target, fact, maximumResultItems);
        }
    }

    private static void Add(
        List<Ra2DiagnosticFact> target,
        Ra2DiagnosticFact fact,
        int maximumResultItems)
    {
        Ra2DiagnosticLimitGuard.ThrowIfAdditionExceeds(target.Count, maximumResultItems);
        target.Add(fact);
    }

    private static void CheckCancellation(int index, CancellationToken cancellationToken)
    {
        if (index % CancellationCheckInterval == 0)
            cancellationToken.ThrowIfCancellationRequested();
    }
}
