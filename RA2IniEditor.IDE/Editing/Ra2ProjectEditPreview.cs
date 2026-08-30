namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2ProjectEditPreview
{
    private Ra2ProjectEditPreview(
        Ra2AutomationProjectSnapshot snapshot,
        Ra2AutomationProjectEditPlan plan,
        Ra2AutomationProjectEditPreviewResult automationResult)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Plan = plan ?? throw new ArgumentNullException(nameof(plan));
        AutomationResult = automationResult ?? throw new ArgumentNullException(nameof(automationResult));
    }

    public Ra2AutomationProjectSnapshot Snapshot { get; }
    public Ra2AutomationProjectEditPlan Plan { get; }
    public Ra2AutomationProjectEditPreviewResult AutomationResult { get; }
    public bool Succeeded => AutomationResult.Succeeded;
    public Guid ProjectPreviewId => AutomationResult.ProjectPreviewId;
    public IReadOnlyList<Ra2AutomationEditPreviewResult> DocumentPreviews => AutomationResult.DocumentPreviews;

    public static Ra2ProjectEditPreview FromAutomation(
        Ra2AutomationProjectSnapshot snapshot,
        Ra2AutomationProjectEditPlan plan,
        Ra2AutomationProjectEditPreviewResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.ProjectSessionId != snapshot.ProjectSessionId ||
            result.ProjectRevision != snapshot.ProjectRevision ||
            result.ProjectPlanId != plan.ProjectPlanId)
        {
            throw new ArgumentException("Project preview result is not bound to the invocation snapshot and plan.", nameof(result));
        }
        return new Ra2ProjectEditPreview(snapshot, plan, result);
    }
}

internal interface IRa2ProjectEditPreviewService
{
    Ra2ProjectEditPreview Preview(
        Ra2AutomationProjectSnapshot snapshot,
        Ra2AutomationProjectEditPlan plan,
        CancellationToken cancellationToken = default);
}

internal sealed class Ra2ProjectEditPreviewService : IRa2ProjectEditPreviewService
{
    private readonly IRa2AutomationCapabilityGateway _gateway;

    public Ra2ProjectEditPreviewService()
        : this(new Ra2AutomationCapabilityGateway())
    {
    }

    internal Ra2ProjectEditPreviewService(IRa2AutomationCapabilityGateway gateway)
        => _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));

    public Ra2ProjectEditPreview Preview(
        Ra2AutomationProjectSnapshot snapshot,
        Ra2AutomationProjectEditPlan plan,
        CancellationToken cancellationToken = default)
        => Ra2ProjectEditPreview.FromAutomation(
            snapshot,
            plan,
            _gateway.PreviewProject(snapshot, plan, cancellationToken));
}

internal sealed record Ra2ProjectEditApplyRequest(
    Guid ProjectPreviewId,
    bool ExplicitConfirmationGranted);

internal enum Ra2ProjectEditApplyOutcomeKind
{
    Applied = 0,
    PreviewUnavailable,
    ConfirmationRequired,
    Stale,
    PrepareFailed,
    CommitFailed,
    EditorSynchronizationFailed,
    UnexpectedFailure
}

internal sealed class Ra2ProjectEditApplyResult
{
    private Ra2ProjectEditApplyResult(
        Ra2ProjectEditApplyOutcomeKind outcomeKind,
        Guid projectPreviewId,
        string message,
        IReadOnlyList<Ra2ProjectDocumentSessionReplacement> committedReplacements,
        int totalWorkCount,
        int dirtyDocumentCount)
    {
        OutcomeKind = outcomeKind;
        ProjectPreviewId = projectPreviewId;
        Message = message;
        CommittedReplacements = Array.AsReadOnly(committedReplacements.ToArray());
        TotalWorkCount = totalWorkCount;
        DirtyDocumentCount = dirtyDocumentCount;
    }

    public bool Succeeded => OutcomeKind == Ra2ProjectEditApplyOutcomeKind.Applied;
    public Ra2ProjectEditApplyOutcomeKind OutcomeKind { get; }
    public Guid ProjectPreviewId { get; }
    public string Message { get; }
    public IReadOnlyList<Ra2ProjectDocumentSessionReplacement> CommittedReplacements { get; }
    public int AffectedDocumentCount => CommittedReplacements.Count;
    public int TotalWorkCount { get; }
    public int DirtyDocumentCount { get; }

    public static Ra2ProjectEditApplyResult Applied(
        Ra2ProjectEditPreview preview,
        IReadOnlyList<Ra2ProjectDocumentSessionReplacement> replacements,
        int dirtyDocumentCount)
        => new(
            Ra2ProjectEditApplyOutcomeKind.Applied,
            preview.ProjectPreviewId,
            "Applied the project edit to all in-memory document sessions.",
            replacements,
            checked(preview.AutomationResult.TotalOperationCount + preview.AutomationResult.TotalSectionCreationCount),
            dirtyDocumentCount);

    public static Ra2ProjectEditApplyResult Failed(
        Ra2ProjectEditApplyOutcomeKind outcomeKind,
        Guid projectPreviewId,
        string message)
    {
        if (outcomeKind == Ra2ProjectEditApplyOutcomeKind.Applied)
            throw new ArgumentOutOfRangeException(nameof(outcomeKind));
        return new(outcomeKind, projectPreviewId, message, [], 0, 0);
    }
}
