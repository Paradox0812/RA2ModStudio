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
                Ra2AutomationEditPlan.MaximumOperationCount)
        ]);

    private readonly Ra2AutomationDocumentQueryService _queryService = new();
    private readonly Ra2AutomationEditPreviewService _editPreviewService = new();

    public IReadOnlyList<Ra2AutomationCapabilityDescriptor> GetCapabilities()
        => Capabilities;

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
}
