namespace RA2IniEditor.Application.Automation.Experimental;

public interface IRa2AutomationTemplateService
{
    IReadOnlyList<Ra2AutomationTemplateDescriptor> GetTemplates();

    Ra2AutomationTemplateExpansionResult ExpandTemplate(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationTemplateExpansionRequest request,
        CancellationToken cancellationToken = default);
}
