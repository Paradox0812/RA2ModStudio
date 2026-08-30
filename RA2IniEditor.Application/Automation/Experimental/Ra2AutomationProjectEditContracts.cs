namespace RA2IniEditor.Application.Automation.Experimental;

public sealed class Ra2AutomationProjectSnapshot
{
    public const int MaximumDocumentCount = 8;
    public const int MaximumAggregateDocumentCharacters = 16_777_216;

    public Ra2AutomationProjectSnapshot(
        Guid projectSessionId,
        long projectRevision,
        string projectRootPath,
        IEnumerable<Ra2AutomationDocumentSnapshot> documents)
    {
        if (projectSessionId == Guid.Empty)
            throw new ArgumentException("Project session identity cannot be empty.", nameof(projectSessionId));
        if (projectRevision < 0)
            throw new ArgumentOutOfRangeException(nameof(projectRevision));
        if (string.IsNullOrWhiteSpace(projectRootPath))
            throw new ArgumentException("Project root path cannot be empty.", nameof(projectRootPath));

        ArgumentNullException.ThrowIfNull(documents);
        Ra2AutomationDocumentSnapshot[] documentArray = documents.ToArray();
        if (documentArray.Length is < 1 or > MaximumDocumentCount)
            throw new ArgumentOutOfRangeException(nameof(documents));
        if (documentArray.Any(document => document is null))
            throw new ArgumentException("Project documents cannot contain null entries.", nameof(documents));
        if (documentArray.Select(document => document.DocumentId).Distinct().Count() != documentArray.Length)
            throw new ArgumentException("Project document identities must be unique.", nameof(documents));
        if (documentArray.Select(document => document.FilePath)
            .Distinct(StringComparer.OrdinalIgnoreCase).Count() != documentArray.Length)
        {
            throw new ArgumentException("Project document paths must be unique.", nameof(documents));
        }

        long aggregateCharacters = documentArray.Sum(document => (long)document.Text.Length);
        if (documentArray.Any(document => document.Text.Length > Ra2AutomationEditPreviewService.MaximumDocumentCharacters) ||
            aggregateCharacters > MaximumAggregateDocumentCharacters)
        {
            throw new ArgumentOutOfRangeException(nameof(documents));
        }

        long fieldRegistryRevision = documentArray[0].FieldRegistry.Revision;
        if (documentArray.Any(document => document.FieldRegistry.Revision != fieldRegistryRevision))
            throw new ArgumentException("Project documents must share one Field Registry revision.", nameof(documents));

        ProjectSessionId = projectSessionId;
        ProjectRevision = projectRevision;
        ProjectRootPath = projectRootPath.Trim();
        Documents = Array.AsReadOnly(documentArray);
    }

    public Guid ProjectSessionId { get; }
    public long ProjectRevision { get; }
    public string ProjectRootPath { get; }
    public IReadOnlyList<Ra2AutomationDocumentSnapshot> Documents { get; }
}

public sealed class Ra2AutomationProjectEditPlan
{
    public const int MaximumDocumentPlanCount = 8;
    public const int MaximumAggregateWorkCount = 256;

    public Ra2AutomationProjectEditPlan(
        Guid projectPlanId,
        Guid expectedProjectSessionId,
        long expectedProjectRevision,
        IEnumerable<Ra2AutomationEditPlan> documentPlans,
        string summary,
        string origin)
    {
        if (projectPlanId == Guid.Empty)
            throw new ArgumentException("Project plan identity cannot be empty.", nameof(projectPlanId));
        if (expectedProjectSessionId == Guid.Empty)
            throw new ArgumentException("Expected project session identity cannot be empty.", nameof(expectedProjectSessionId));
        if (expectedProjectRevision < 0)
            throw new ArgumentOutOfRangeException(nameof(expectedProjectRevision));

        ArgumentNullException.ThrowIfNull(documentPlans);
        Ra2AutomationEditPlan[] planArray = documentPlans.ToArray();
        if (planArray.Length is < 1 or > MaximumDocumentPlanCount)
            throw new ArgumentOutOfRangeException(nameof(documentPlans));
        if (planArray.Any(plan => plan is null))
            throw new ArgumentException("Project document plans cannot contain null entries.", nameof(documentPlans));
        if (planArray.Select(plan => plan.ExpectedDocumentId).Distinct().Count() != planArray.Length)
            throw new ArgumentException("Project document plan targets must be unique.", nameof(documentPlans));

        int aggregateWork = planArray.Sum(plan => checked(plan.SectionCreations.Count + plan.Operations.Count));
        if (aggregateWork > MaximumAggregateWorkCount)
            throw new ArgumentOutOfRangeException(nameof(documentPlans));

        ProjectPlanId = projectPlanId;
        ExpectedProjectSessionId = expectedProjectSessionId;
        ExpectedProjectRevision = expectedProjectRevision;
        DocumentPlans = Array.AsReadOnly(planArray);
        Summary = ValidateDisplayText(summary, Ra2AutomationEditPlan.MaximumSummaryLength, nameof(summary));
        Origin = ValidateDisplayText(origin, Ra2AutomationEditPlan.MaximumOriginLength, nameof(origin));
    }

    public Guid ProjectPlanId { get; }
    public Guid ExpectedProjectSessionId { get; }
    public long ExpectedProjectRevision { get; }
    public IReadOnlyList<Ra2AutomationEditPlan> DocumentPlans { get; }
    public string Summary { get; }
    public string Origin { get; }

    private static string ValidateDisplayText(string value, int maximumLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Project plan display text cannot be empty.", parameterName);

        string normalized = value.Trim();
        if (normalized.Length > maximumLength || normalized.IndexOfAny(['\r', '\n', '\0']) >= 0)
            throw new ArgumentException("Project plan display text is too long or contains unsupported characters.", parameterName);

        return normalized;
    }
}

public enum Ra2AutomationProjectEditPreviewFailureKind
{
    None = 0,
    InvalidProjectSnapshot = 1,
    InvalidProjectPlan = 2,
    StaleProject = 3,
    DocumentNotFound = 4,
    DuplicateDocumentTarget = 5,
    DocumentPreviewFailed = 6,
    ResourceLimitExceeded = 7,
    Canceled = 8,
    UnexpectedFailure = 9
}

public sealed class Ra2AutomationProjectEditPreviewResult
{
    internal Ra2AutomationProjectEditPreviewResult(
        Ra2AutomationProjectSnapshot snapshot,
        Ra2AutomationProjectEditPlan plan,
        Ra2AutomationProjectEditPreviewFailureKind failureKind,
        string message,
        Guid projectPreviewId,
        IReadOnlyList<Ra2AutomationEditPreviewResult> documentPreviews,
        Guid? failedDocumentId = null,
        string? failedFilePath = null,
        Ra2AutomationEditPreviewFailureKind failedDocumentFailureKind = Ra2AutomationEditPreviewFailureKind.None)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(plan);
        if (!Enum.IsDefined(failureKind))
            throw new ArgumentOutOfRangeException(nameof(failureKind));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("A project preview result message is required.", nameof(message));
        ArgumentNullException.ThrowIfNull(documentPreviews);
        if (!Enum.IsDefined(failedDocumentFailureKind))
            throw new ArgumentOutOfRangeException(nameof(failedDocumentFailureKind));

        bool succeeded = failureKind == Ra2AutomationProjectEditPreviewFailureKind.None;
        if (succeeded)
        {
            if (projectPreviewId == Guid.Empty)
                throw new ArgumentException("A successful project preview requires an identity.", nameof(projectPreviewId));
            if (documentPreviews.Count != plan.DocumentPlans.Count || documentPreviews.Any(preview => !preview.Succeeded))
                throw new ArgumentException("A successful project preview requires every document preview.", nameof(documentPreviews));
            if (failedDocumentId is not null || failedFilePath is not null ||
                failedDocumentFailureKind != Ra2AutomationEditPreviewFailureKind.None)
            {
                throw new ArgumentException("A successful project preview cannot contain failure evidence.");
            }
        }
        else if (projectPreviewId != Guid.Empty || documentPreviews.Count != 0)
        {
            throw new ArgumentException("A failed project preview cannot contain applicable or partial payload.");
        }

        Succeeded = succeeded;
        FailureKind = failureKind;
        Message = message;
        ProjectSessionId = snapshot.ProjectSessionId;
        ProjectRevision = snapshot.ProjectRevision;
        ProjectRootPath = snapshot.ProjectRootPath;
        ProjectPlanId = plan.ProjectPlanId;
        ProjectPreviewId = projectPreviewId;
        DocumentPreviews = Array.AsReadOnly(documentPreviews.ToArray());
        TotalOperationCount = succeeded ? DocumentPreviews.Sum(preview => preview.OperationPreviews.Count) : 0;
        TotalSectionCreationCount = succeeded ? DocumentPreviews.Sum(preview => preview.SectionCreationPreviews.Count) : 0;
        RequiresExplicitConfirmation = succeeded;
        FailedDocumentId = failedDocumentId;
        FailedFilePath = failedFilePath;
        FailedDocumentFailureKind = failedDocumentFailureKind;
    }

    public bool Succeeded { get; }
    public Ra2AutomationProjectEditPreviewFailureKind FailureKind { get; }
    public string Message { get; }
    public Guid ProjectSessionId { get; }
    public long ProjectRevision { get; }
    public string ProjectRootPath { get; }
    public Guid ProjectPlanId { get; }
    public Guid ProjectPreviewId { get; }
    public IReadOnlyList<Ra2AutomationEditPreviewResult> DocumentPreviews { get; }
    public int TotalOperationCount { get; }
    public int TotalSectionCreationCount { get; }
    public bool RequiresExplicitConfirmation { get; }
    public Guid? FailedDocumentId { get; }
    public string? FailedFilePath { get; }
    public Ra2AutomationEditPreviewFailureKind FailedDocumentFailureKind { get; }
}
