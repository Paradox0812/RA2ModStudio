using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Automation.Experimental;

public enum Ra2AutomationEditOperationKind
{
    UpsertField = 0,
    ReplaceFieldValue = 1
}

public sealed class Ra2AutomationEditOperation
{
    public const int MaximumSectionNameLength = 256;
    public const int MaximumKeyLength = 256;
    public const int MaximumValueLength = 8192;

    public Ra2AutomationEditOperation(
        Ra2AutomationEditOperationKind kind,
        string sectionName,
        string key,
        string value)
    {
        if (!Enum.IsDefined(kind))
            throw new ArgumentOutOfRangeException(nameof(kind));

        Kind = kind;
        SectionName = NormalizeIdentifier(
            sectionName,
            MaximumSectionNameLength,
            nameof(sectionName),
            allowBrackets: false);
        Key = NormalizeIdentifier(
            key,
            MaximumKeyLength,
            nameof(key),
            allowBrackets: true);
        Value = ValidateValue(value);
    }

    public Ra2AutomationEditOperationKind Kind { get; }
    public string SectionName { get; }
    public string Key { get; }
    public string Value { get; }

    private static string NormalizeIdentifier(
        string value,
        int maximumLength,
        string parameterName,
        bool allowBrackets)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Structured edit identifier cannot be empty.", parameterName);

        string normalized = value.Trim();
        if (normalized.Length > maximumLength ||
            ContainsLineBreakOrNull(normalized) ||
            normalized.Contains('=') ||
            (!allowBrackets && normalized.IndexOfAny(['[', ']']) >= 0))
        {
            throw new ArgumentException("Structured edit identifier is too long or contains unsupported characters.", parameterName);
        }

        return normalized;
    }

    private static string ValidateValue(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Length > MaximumValueLength || ContainsLineBreakOrNull(value))
            throw new ArgumentException("Structured field value is too long or contains unsupported characters.", nameof(value));

        return value;
    }

    private static bool ContainsLineBreakOrNull(string value)
        => value.IndexOfAny(['\r', '\n', '\0']) >= 0;
}

public sealed class Ra2AutomationEditPlan
{
    public const int MaximumOperationCount = 128;
    public const int MaximumSummaryLength = 512;
    public const int MaximumOriginLength = 128;

    public Ra2AutomationEditPlan(
        Guid planId,
        Guid expectedDocumentId,
        int expectedVersion,
        long expectedFieldRegistryRevision,
        IEnumerable<Ra2AutomationEditOperation> operations,
        string summary,
        string origin)
    {
        if (planId == Guid.Empty)
            throw new ArgumentException("Plan identity cannot be empty.", nameof(planId));
        if (expectedDocumentId == Guid.Empty)
            throw new ArgumentException("Expected document identity cannot be empty.", nameof(expectedDocumentId));
        if (expectedVersion < 0)
            throw new ArgumentOutOfRangeException(nameof(expectedVersion));
        if (expectedFieldRegistryRevision <= 0)
            throw new ArgumentOutOfRangeException(nameof(expectedFieldRegistryRevision));

        ArgumentNullException.ThrowIfNull(operations);
        Ra2AutomationEditOperation[] operationArray = operations.ToArray();
        if (operationArray.Length is < 1 or > MaximumOperationCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(operations),
                $"A plan must contain between 1 and {MaximumOperationCount} operations.");
        }

        if (operationArray.Any(operation => operation is null))
            throw new ArgumentException("Plan operations cannot contain null entries.", nameof(operations));

        PlanId = planId;
        ExpectedDocumentId = expectedDocumentId;
        ExpectedVersion = expectedVersion;
        ExpectedFieldRegistryRevision = expectedFieldRegistryRevision;
        Operations = Array.AsReadOnly(operationArray);
        Summary = ValidateDisplayText(summary, MaximumSummaryLength, nameof(summary));
        Origin = ValidateDisplayText(origin, MaximumOriginLength, nameof(origin));
    }

    public Guid PlanId { get; }
    public Guid ExpectedDocumentId { get; }
    public int ExpectedVersion { get; }
    public long ExpectedFieldRegistryRevision { get; }
    public IReadOnlyList<Ra2AutomationEditOperation> Operations { get; }
    public string Summary { get; }
    public string Origin { get; }

    private static string ValidateDisplayText(string value, int maximumLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Plan display text cannot be empty.", parameterName);

        string normalized = value.Trim();
        if (normalized.Length > maximumLength || normalized.IndexOfAny(['\r', '\n', '\0']) >= 0)
            throw new ArgumentException("Plan display text is too long or contains unsupported characters.", parameterName);

        return normalized;
    }
}

public enum Ra2AutomationEditPreviewFailureKind
{
    None = 0,
    InvalidPlan = 1,
    StalePlanTarget = 2,
    ReadOnly = 3,
    UnsupportedOperation = 4,
    InvalidSection = 5,
    SectionNotFound = 6,
    AmbiguousSection = 7,
    FieldNotFound = 8,
    AmbiguousField = 9,
    ConflictingOperations = 10,
    OverlappingChanges = 11,
    NoChanges = 12,
    Canceled = 13,
    CurrentAnalysisFailed = 14,
    CandidateAnalysisFailed = 15,
    UnexpectedFailure = 16,
    DocumentTooLarge = 17,
    ResultLimitExceeded = 18
}

public enum Ra2AutomationEditOperationOutcomeKind
{
    Inserted = 0,
    Replaced = 1
}

public enum Ra2AutomationFieldTrustLevel
{
    Verified = 0,
    VerifiedGuardrail = 1,
    Inferred = 2,
    ManualCurated = 3,
    AutoExtracted = 4,
    Obsolete = 5,
    NonExistent = 6,
    PseudoField = 7,
    Unknown = 8
}

public sealed class Ra2AutomationTextChange
{
    internal Ra2AutomationTextChange(Ra2AutomationTextSpan span, string newText, string reason)
    {
        Span = span;
        NewText = newText ?? throw new ArgumentNullException(nameof(newText));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A text change reason is required.", nameof(reason));

        Reason = reason;
    }

    public Ra2AutomationTextSpan Span { get; }
    public string NewText { get; }
    public string Reason { get; }
}

public sealed class Ra2AutomationEditOperationPreview
{
    internal Ra2AutomationEditOperationPreview(
        int operationIndex,
        Ra2AutomationEditOperation operation,
        Ra2AutomationEditOperationOutcomeKind outcomeKind,
        Ra2SectionKind resolvedSectionKind,
        bool isKnownField,
        Ra2AutomationFieldTrustLevel fieldTrustLevel,
        Ra2AutomationTextSpan affectedOriginalSpan,
        string summary)
    {
        if (operationIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(operationIndex));
        ArgumentNullException.ThrowIfNull(operation);
        if (!Enum.IsDefined(outcomeKind))
            throw new ArgumentOutOfRangeException(nameof(outcomeKind));
        if (!Enum.IsDefined(resolvedSectionKind))
            throw new ArgumentOutOfRangeException(nameof(resolvedSectionKind));
        if (!Enum.IsDefined(fieldTrustLevel))
            throw new ArgumentOutOfRangeException(nameof(fieldTrustLevel));
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("An operation preview summary is required.", nameof(summary));

        OperationIndex = operationIndex;
        Operation = operation;
        OutcomeKind = outcomeKind;
        ResolvedSectionKind = resolvedSectionKind;
        IsKnownField = isKnownField;
        FieldTrustLevel = fieldTrustLevel;
        AffectedOriginalSpan = affectedOriginalSpan;
        Summary = summary;
    }

    public int OperationIndex { get; }
    public Ra2AutomationEditOperation Operation { get; }
    public Ra2AutomationEditOperationOutcomeKind OutcomeKind { get; }
    public Ra2SectionKind ResolvedSectionKind { get; }
    public bool IsKnownField { get; }
    public Ra2AutomationFieldTrustLevel FieldTrustLevel { get; }
    public Ra2AutomationTextSpan AffectedOriginalSpan { get; }
    public string Summary { get; }
}

public sealed class Ra2AutomationEditPreviewResult
{
    internal Ra2AutomationEditPreviewResult(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan,
        Ra2AutomationEditPreviewFailureKind failureKind,
        string message,
        Guid previewId,
        string? candidateText,
        IReadOnlyList<Ra2AutomationTextChange> changes,
        IReadOnlyList<Ra2AutomationEditOperationPreview> operationPreviews,
        IReadOnlyList<Ra2AutomationDiagnosticFact> addedDiagnostics,
        IReadOnlyList<Ra2AutomationDiagnosticFact> removedDiagnostics)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(plan);
        if (!Enum.IsDefined(failureKind))
            throw new ArgumentOutOfRangeException(nameof(failureKind));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("A result message is required.", nameof(message));
        ArgumentNullException.ThrowIfNull(changes);
        ArgumentNullException.ThrowIfNull(operationPreviews);
        ArgumentNullException.ThrowIfNull(addedDiagnostics);
        ArgumentNullException.ThrowIfNull(removedDiagnostics);

        bool succeeded = failureKind == Ra2AutomationEditPreviewFailureKind.None;
        if (succeeded)
        {
            if (previewId == Guid.Empty)
                throw new ArgumentException("A successful preview requires an identity.", nameof(previewId));
            ArgumentNullException.ThrowIfNull(candidateText);
            if (changes.Count == 0)
                throw new ArgumentException("A successful preview requires at least one text change.", nameof(changes));
            if (operationPreviews.Count != plan.Operations.Count)
                throw new ArgumentException("Operation preview count must match the edit plan.", nameof(operationPreviews));
        }
        else if (previewId != Guid.Empty || candidateText is not null || changes.Count != 0 ||
                 operationPreviews.Count != 0 || addedDiagnostics.Count != 0 || removedDiagnostics.Count != 0)
        {
            throw new ArgumentException("A failed preview cannot contain applicable or partial payload.");
        }

        Succeeded = succeeded;
        FailureKind = failureKind;
        Message = message;
        DocumentId = snapshot.DocumentId;
        Version = snapshot.Version;
        FilePath = snapshot.FilePath;
        FieldRegistryRevision = snapshot.FieldRegistry.Revision;
        PlanId = plan.PlanId;
        PreviewId = previewId;
        CandidateText = candidateText;
        Changes = Array.AsReadOnly(changes.ToArray());
        OperationPreviews = Array.AsReadOnly(operationPreviews.ToArray());
        AddedDiagnostics = Array.AsReadOnly(addedDiagnostics.ToArray());
        RemovedDiagnostics = Array.AsReadOnly(removedDiagnostics.ToArray());
        AddedErrorCount = AddedDiagnostics.Count(diagnostic => diagnostic.Severity == IniIssueSeverity.Error);
        AddedWarningCount = AddedDiagnostics.Count(diagnostic => diagnostic.Severity == IniIssueSeverity.Warning);
        RequiresExplicitConfirmation = succeeded;
    }

    public bool Succeeded { get; }
    public Ra2AutomationEditPreviewFailureKind FailureKind { get; }
    public string Message { get; }
    public Guid DocumentId { get; }
    public int Version { get; }
    public string FilePath { get; }
    public long FieldRegistryRevision { get; }
    public Guid PlanId { get; }
    public Guid PreviewId { get; }
    public string? CandidateText { get; }
    public IReadOnlyList<Ra2AutomationTextChange> Changes { get; }
    public IReadOnlyList<Ra2AutomationEditOperationPreview> OperationPreviews { get; }
    public IReadOnlyList<Ra2AutomationDiagnosticFact> AddedDiagnostics { get; }
    public IReadOnlyList<Ra2AutomationDiagnosticFact> RemovedDiagnostics { get; }
    public int AddedErrorCount { get; }
    public int AddedWarningCount { get; }
    public bool RequiresExplicitConfirmation { get; }
}
