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

        // HLI-1B-3 replaces this buildable contract skeleton with the canonical semantic engine.
        Ra2AutomationEditPreviewFailureKind failureKind = cancellationToken.IsCancellationRequested
            ? Ra2AutomationEditPreviewFailureKind.Canceled
            : Ra2AutomationEditPreviewFailureKind.UnexpectedFailure;
        string message = cancellationToken.IsCancellationRequested
            ? "The edit preview was canceled."
            : "The edit preview engine is not available in this stage.";

        return new Ra2AutomationEditPreviewResult(
            snapshot,
            plan,
            failureKind,
            message,
            Guid.Empty,
            candidateText: null,
            changes: [],
            operationPreviews: [],
            addedDiagnostics: [],
            removedDiagnostics: []);
    }
}
