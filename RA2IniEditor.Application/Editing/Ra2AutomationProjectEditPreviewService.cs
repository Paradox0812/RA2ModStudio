using RA2IniEditor.Application.Automation.Experimental;

namespace RA2IniEditor.Application.Editing;

internal sealed class Ra2AutomationProjectEditPreviewService
{
    private readonly Ra2AutomationEditPreviewService _documentPreviewService = new();

    public Ra2AutomationProjectEditPreviewResult Preview(
        Ra2AutomationProjectSnapshot snapshot,
        Ra2AutomationProjectEditPlan plan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(plan);

        if (cancellationToken.IsCancellationRequested)
            return Failure(snapshot, plan, Ra2AutomationProjectEditPreviewFailureKind.Canceled, "Project preview was canceled.");
        if (snapshot.ProjectSessionId != plan.ExpectedProjectSessionId ||
            snapshot.ProjectRevision != plan.ExpectedProjectRevision)
        {
            return Failure(snapshot, plan, Ra2AutomationProjectEditPreviewFailureKind.StaleProject, "Project preview target is stale.");
        }

        Dictionary<Guid, Ra2AutomationDocumentSnapshot> documents = snapshot.Documents.ToDictionary(document => document.DocumentId);
        List<Ra2AutomationEditPreviewResult> previews = new(plan.DocumentPlans.Count);
        foreach (Ra2AutomationEditPlan documentPlan in plan.DocumentPlans)
        {
            if (cancellationToken.IsCancellationRequested)
                return Failure(snapshot, plan, Ra2AutomationProjectEditPreviewFailureKind.Canceled, "Project preview was canceled.");
            if (!documents.TryGetValue(documentPlan.ExpectedDocumentId, out Ra2AutomationDocumentSnapshot? document))
            {
                return Failure(
                    snapshot,
                    plan,
                    Ra2AutomationProjectEditPreviewFailureKind.DocumentNotFound,
                    "A project edit target is not part of the captured project snapshot.",
                    documentPlan.ExpectedDocumentId);
            }

            Ra2AutomationEditPreviewResult preview;
            try
            {
                preview = _documentPreviewService.Preview(document, documentPlan, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return Failure(snapshot, plan, Ra2AutomationProjectEditPreviewFailureKind.Canceled, "Project preview was canceled.");
            }
            catch (Exception)
            {
                return Failure(
                    snapshot,
                    plan,
                    Ra2AutomationProjectEditPreviewFailureKind.UnexpectedFailure,
                    "Project preview failed unexpectedly.",
                    document.DocumentId,
                    document.FilePath);
            }

            if (!preview.Succeeded)
            {
                Ra2AutomationProjectEditPreviewFailureKind failureKind =
                    preview.FailureKind == Ra2AutomationEditPreviewFailureKind.Canceled
                        ? Ra2AutomationProjectEditPreviewFailureKind.Canceled
                        : Ra2AutomationProjectEditPreviewFailureKind.DocumentPreviewFailed;
                return Failure(
                    snapshot,
                    plan,
                    failureKind,
                    "A document preview failed; no project preview payload was produced.",
                    document.DocumentId,
                    document.FilePath,
                    preview.FailureKind);
            }

            previews.Add(preview);
            if (cancellationToken.IsCancellationRequested)
                return Failure(snapshot, plan, Ra2AutomationProjectEditPreviewFailureKind.Canceled, "Project preview was canceled.");
        }

        return new Ra2AutomationProjectEditPreviewResult(
            snapshot,
            plan,
            Ra2AutomationProjectEditPreviewFailureKind.None,
            "Project edit preview is ready for explicit confirmation.",
            Guid.NewGuid(),
            previews);
    }

    private static Ra2AutomationProjectEditPreviewResult Failure(
        Ra2AutomationProjectSnapshot snapshot,
        Ra2AutomationProjectEditPlan plan,
        Ra2AutomationProjectEditPreviewFailureKind failureKind,
        string message,
        Guid? failedDocumentId = null,
        string? failedFilePath = null,
        Ra2AutomationEditPreviewFailureKind failedDocumentFailureKind = Ra2AutomationEditPreviewFailureKind.None)
        => new(
            snapshot,
            plan,
            failureKind,
            message,
            Guid.Empty,
            [],
            failedDocumentId,
            failedFilePath,
            failedDocumentFailureKind);
}
