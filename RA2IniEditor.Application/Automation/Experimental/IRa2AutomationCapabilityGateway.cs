namespace RA2IniEditor.Application.Automation.Experimental;

public interface IRa2AutomationCapabilityGateway
{
    IReadOnlyList<Ra2AutomationCapabilityDescriptor> GetCapabilities();

    IReadOnlyList<Ra2AutomationTemplateDescriptor> GetTemplates();

    Ra2AutomationFieldSchemaQueryResult GetFieldSchema(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationFieldSchemaQuery request,
        CancellationToken cancellationToken = default);

    Ra2AutomationReferenceResolveResult ResolveReference(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationReferenceResolveQuery request,
        CancellationToken cancellationToken = default);

    Ra2AutomationSectionQueryResult GetSection(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationSectionQuery request,
        CancellationToken cancellationToken = default);

    Ra2AutomationReferenceQueryResult FindReferences(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationReferenceQuery request,
        CancellationToken cancellationToken = default);

    Ra2AutomationDocumentDiagnosticsResult Validate(
        Ra2AutomationDocumentSnapshot snapshot,
        CancellationToken cancellationToken = default);

    Ra2AutomationEditPreviewResult Preview(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan,
        CancellationToken cancellationToken = default);

    Ra2AutomationTemplateExpansionResult ExpandTemplate(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationTemplateExpansionRequest request,
        CancellationToken cancellationToken = default);
}
