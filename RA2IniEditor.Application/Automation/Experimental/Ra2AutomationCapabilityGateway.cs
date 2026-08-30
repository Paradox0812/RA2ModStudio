using RA2IniEditor.Application.Editing;

namespace RA2IniEditor.Application.Automation.Experimental;

public sealed class Ra2AutomationCapabilityGateway : IRa2AutomationCapabilityGateway
{
    private static readonly IReadOnlyList<Ra2AutomationCapabilityDescriptor> Capabilities =
        Array.AsReadOnly<Ra2AutomationCapabilityDescriptor>(
        [
            new(
                Ra2AutomationCapabilityIds.DocumentSectionGet,
                Ra2AutomationCapabilityIds.CurrentVersion,
                Ra2AutomationCapabilityRisk.Query,
                Ra2AutomationCapabilityStability.Experimental,
                Ra2AutomationDocumentQueryService.MaximumDocumentCharacters,
                Ra2AutomationDocumentQueryService.MaximumResultItems,
                null),
            new(
                Ra2AutomationCapabilityIds.DocumentReferencesFind,
                Ra2AutomationCapabilityIds.CurrentVersion,
                Ra2AutomationCapabilityRisk.Query,
                Ra2AutomationCapabilityStability.Experimental,
                Ra2AutomationDocumentQueryService.MaximumDocumentCharacters,
                Ra2AutomationDocumentQueryService.MaximumResultItems,
                null),
            new(
                Ra2AutomationCapabilityIds.DocumentDiagnosticsValidate,
                Ra2AutomationCapabilityIds.CurrentVersion,
                Ra2AutomationCapabilityRisk.Query,
                Ra2AutomationCapabilityStability.Experimental,
                Ra2AutomationDocumentQueryService.MaximumDocumentCharacters,
                Ra2AutomationDocumentQueryService.MaximumResultItems,
                null),
            new(
                Ra2AutomationCapabilityIds.DocumentEditPreview,
                Ra2AutomationCapabilityIds.CurrentVersion,
                Ra2AutomationCapabilityRisk.Edit,
                Ra2AutomationCapabilityStability.Experimental,
                Ra2AutomationEditPreviewService.MaximumDocumentCharacters,
                Ra2AutomationEditPreviewService.MaximumDiagnosticItems,
                Ra2AutomationEditPlan.MaximumOperationCount),
            new(
                Ra2AutomationCapabilityIds.DocumentFieldSchemaGet,
                Ra2AutomationCapabilityIds.CurrentVersion,
                Ra2AutomationCapabilityRisk.Query,
                Ra2AutomationCapabilityStability.Experimental,
                Ra2AutomationDocumentQueryService.MaximumDocumentCharacters,
                1,
                null),
            new(
                Ra2AutomationCapabilityIds.DocumentReferenceResolve,
                Ra2AutomationCapabilityIds.CurrentVersion,
                Ra2AutomationCapabilityRisk.Query,
                Ra2AutomationCapabilityStability.Experimental,
                Ra2AutomationDocumentQueryService.MaximumDocumentCharacters,
                1,
                null),
            new(
                Ra2AutomationCapabilityIds.ContentTemplateExpand,
                Ra2AutomationCapabilityIds.CurrentVersion,
                Ra2AutomationCapabilityRisk.Edit,
                Ra2AutomationCapabilityStability.Experimental,
                Ra2AutomationDocumentQueryService.MaximumDocumentCharacters,
                null,
                Ra2AutomationEditPlan.MaximumOperationCount),
            new(
                Ra2AutomationCapabilityIds.ProjectEditPreview,
                Ra2AutomationCapabilityIds.CurrentVersion,
                Ra2AutomationCapabilityRisk.Edit,
                Ra2AutomationCapabilityStability.Experimental,
                Ra2AutomationEditPreviewService.MaximumDocumentCharacters,
                Ra2AutomationProjectSnapshot.MaximumDocumentCount,
                Ra2AutomationProjectEditPlan.MaximumAggregateWorkCount),
            new(
                Ra2AutomationCapabilityIds.ProjectContentTemplateExpand,
                Ra2AutomationCapabilityIds.CurrentVersion,
                Ra2AutomationCapabilityRisk.Edit,
                Ra2AutomationCapabilityStability.Experimental,
                Ra2AutomationDocumentQueryService.MaximumDocumentCharacters,
                Ra2AutomationProjectSnapshot.MaximumDocumentCount,
                Ra2AutomationProjectEditPlan.MaximumAggregateWorkCount)
        ]);

    private readonly Ra2AutomationDocumentQueryService _queryService = new();
    private readonly Ra2AutomationEditPreviewService _editPreviewService = new();
    private readonly Ra2AutomationTemplateService _templateService = new();
    private readonly Ra2AutomationProjectEditPreviewService _projectEditPreviewService = new();

    public IReadOnlyList<Ra2AutomationCapabilityDescriptor> GetCapabilities()
        => Capabilities;

    public IReadOnlyList<Ra2AutomationTemplateDescriptor> GetTemplates()
        => _templateService.GetTemplates();

    public Ra2AutomationFieldSchemaQueryResult GetFieldSchema(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationFieldSchemaQuery request,
        CancellationToken cancellationToken = default)
        => _queryService.GetFieldSchema(snapshot, request, cancellationToken);

    public Ra2AutomationReferenceResolveResult ResolveReference(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationReferenceResolveQuery request,
        CancellationToken cancellationToken = default)
        => _queryService.ResolveReference(snapshot, request, cancellationToken);

    public Ra2AutomationSectionQueryResult GetSection(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationSectionQuery request,
        CancellationToken cancellationToken = default)
        => _queryService.GetSection(snapshot, request, cancellationToken);

    public Ra2AutomationReferenceQueryResult FindReferences(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationReferenceQuery request,
        CancellationToken cancellationToken = default)
        => _queryService.FindReferences(snapshot, request, cancellationToken);

    public Ra2AutomationDocumentDiagnosticsResult Validate(
        Ra2AutomationDocumentSnapshot snapshot,
        CancellationToken cancellationToken = default)
        => _queryService.Validate(snapshot, cancellationToken);

    public Ra2AutomationEditPreviewResult Preview(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan,
        CancellationToken cancellationToken = default)
        => _editPreviewService.Preview(snapshot, plan, cancellationToken);

    public Ra2AutomationProjectEditPreviewResult PreviewProject(
        Ra2AutomationProjectSnapshot snapshot,
        Ra2AutomationProjectEditPlan plan,
        CancellationToken cancellationToken = default)
        => _projectEditPreviewService.Preview(snapshot, plan, cancellationToken);

    public Ra2AutomationTemplateExpansionResult ExpandTemplate(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationTemplateExpansionRequest request,
        CancellationToken cancellationToken = default)
        => _templateService.ExpandTemplate(snapshot, request, cancellationToken);

    public Ra2AutomationProjectTemplateExpansionResult ExpandProjectTemplate(
        Ra2AutomationProjectSnapshot snapshot,
        Ra2AutomationTemplateExpansionRequest request,
        CancellationToken cancellationToken = default)
        => _templateService.ExpandProjectTemplate(snapshot, request, cancellationToken);
}
