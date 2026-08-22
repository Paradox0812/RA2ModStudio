namespace RA2IniEditor.Application.Automation.Experimental;

public interface IRa2AutomationEditPreviewService
{
    Ra2AutomationEditPreviewResult Preview(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan,
        CancellationToken cancellationToken = default);
}
