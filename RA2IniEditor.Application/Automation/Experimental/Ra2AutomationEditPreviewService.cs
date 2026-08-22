using RA2IniEditor.Application.Editing;

namespace RA2IniEditor.Application.Automation.Experimental;

public sealed class Ra2AutomationEditPreviewService : IRa2AutomationEditPreviewService
{
    public const int MaximumDocumentCharacters = 8_388_608;
    public const int MaximumDiagnosticItems = 10_000;

    public Ra2AutomationEditPreviewResult Preview(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(plan);

        return new Ra2AutomationEditPreviewEngine().Preview(
            snapshot,
            plan,
            MaximumDocumentCharacters,
            MaximumDiagnosticItems,
            cancellationToken);
    }

    internal Ra2AutomationEditPreviewResult PreviewForHost(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan,
        CancellationToken cancellationToken = default)
        => new Ra2AutomationEditPreviewEngine().Preview(
            snapshot,
            plan,
            int.MaxValue,
            int.MaxValue,
            cancellationToken);
}
