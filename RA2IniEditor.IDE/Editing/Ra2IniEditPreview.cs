using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.Editing;

internal enum Ra2IniEditPreviewFailureKind
{
    None = 0,
    InvalidPlan,
    StalePlanTarget,
    ReadOnly,
    UnsupportedOperation,
    InvalidSection,
    SectionNotFound,
    AmbiguousSection,
    FieldNotFound,
    AmbiguousField,
    ConflictingOperations,
    OverlappingChanges,
    NoChanges,
    Canceled,
    CurrentAnalysisFailed,
    CandidateAnalysisFailed,
    UnexpectedFailure
}

internal enum Ra2IniEditOperationOutcomeKind
{
    Inserted = 0,
    Replaced
}

/// <summary>
/// 表示单个结构化操作在当前快照上的解析证据。
/// </summary>
internal sealed class Ra2IniEditOperationPreview
{
    public Ra2IniEditOperationPreview(
        int operationIndex,
        Ra2IniEditOperation operation,
        Ra2IniEditOperationOutcomeKind outcomeKind,
        Ra2SectionKind resolvedSectionKind,
        bool isKnownField,
        Ra2FieldTrustLevel fieldTrustLevel,
        Ra2TextSpan affectedOriginalSpan,
        string summary)
    {
        if (operationIndex < 0)
            throw new ArgumentOutOfRangeException(nameof(operationIndex));
        if (!Enum.IsDefined(outcomeKind))
            throw new ArgumentOutOfRangeException(nameof(outcomeKind));
        if (!Enum.IsDefined(resolvedSectionKind))
            throw new ArgumentOutOfRangeException(nameof(resolvedSectionKind));
        if (!Enum.IsDefined(fieldTrustLevel))
            throw new ArgumentOutOfRangeException(nameof(fieldTrustLevel));
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("Operation preview summary cannot be empty.", nameof(summary));

        OperationIndex = operationIndex;
        Operation = operation ?? throw new ArgumentNullException(nameof(operation));
        OutcomeKind = outcomeKind;
        ResolvedSectionKind = resolvedSectionKind;
        IsKnownField = isKnownField;
        FieldTrustLevel = fieldTrustLevel;
        AffectedOriginalSpan = affectedOriginalSpan;
        Summary = summary;
    }

    public int OperationIndex { get; }

    public Ra2IniEditOperation Operation { get; }

    public Ra2IniEditOperationOutcomeKind OutcomeKind { get; }

    public Ra2SectionKind ResolvedSectionKind { get; }

    public bool IsKnownField { get; }

    public Ra2FieldTrustLevel FieldTrustLevel { get; }

    public Ra2TextSpan AffectedOriginalSpan { get; }

    public string Summary { get; }
}

/// <summary>
/// 表示由 IDE 生成、不可直接应用的单文档编辑预览。
/// </summary>
internal sealed class Ra2IniEditPreview
{
    private Ra2IniEditPreview(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        Ra2IniEditPreviewFailureKind failureKind,
        string message,
        Guid previewId,
        Ra2TextChangeSet? changeSet,
        string? candidateText,
        IReadOnlyList<Ra2IniEditOperationPreview> operationPreviews,
        Ra2IniLanguageAnalysisResult? currentAnalysis,
        Ra2IniLanguageAnalysisResult? candidateAnalysis,
        IReadOnlyList<Ra2DiagnosticFact> addedDiagnostics,
        IReadOnlyList<Ra2DiagnosticFact> removedDiagnostics)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Plan = plan ?? throw new ArgumentNullException(nameof(plan));
        FailureKind = failureKind;
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Preview message cannot be empty.", nameof(message))
            : message;

        bool succeeded = failureKind == Ra2IniEditPreviewFailureKind.None;
        if (succeeded)
        {
            if (previewId == Guid.Empty || changeSet is null || candidateText is null ||
                currentAnalysis is null || candidateAnalysis is null)
            {
                throw new ArgumentException("Successful preview evidence is incomplete.");
            }
        }
        else if (previewId != Guid.Empty || changeSet is not null || candidateText is not null ||
                 currentAnalysis is not null || candidateAnalysis is not null ||
                 operationPreviews.Count != 0 || addedDiagnostics.Count != 0 ||
                 removedDiagnostics.Count != 0)
        {
            throw new ArgumentException("Failed previews cannot carry applicable evidence.");
        }

        PreviewId = previewId;
        ChangeSet = changeSet;
        CandidateText = candidateText;
        OperationPreviews = Array.AsReadOnly(operationPreviews.ToArray());
        CurrentAnalysis = currentAnalysis;
        CandidateAnalysis = candidateAnalysis;
        AddedDiagnostics = Array.AsReadOnly(addedDiagnostics.ToArray());
        RemovedDiagnostics = Array.AsReadOnly(removedDiagnostics.ToArray());
    }

    public bool Succeeded => FailureKind == Ra2IniEditPreviewFailureKind.None;

    public Ra2AuthoringSnapshot Snapshot { get; }

    public Ra2IniEditPlan Plan { get; }

    public Ra2IniEditPreviewFailureKind FailureKind { get; }

    public string Message { get; }

    public Guid PreviewId { get; }

    public Ra2TextChangeSet? ChangeSet { get; }

    public string? CandidateText { get; }

    public IReadOnlyList<Ra2IniEditOperationPreview> OperationPreviews { get; }

    public Ra2IniLanguageAnalysisResult? CurrentAnalysis { get; }

    public Ra2IniLanguageAnalysisResult? CandidateAnalysis { get; }

    public IReadOnlyList<Ra2DiagnosticFact> AddedDiagnostics { get; }

    public IReadOnlyList<Ra2DiagnosticFact> RemovedDiagnostics { get; }

    public int AddedErrorCount
        => AddedDiagnostics.Count(diagnostic => diagnostic.Severity == IniIssueSeverity.Error);

    public int AddedWarningCount
        => AddedDiagnostics.Count(diagnostic => diagnostic.Severity == IniIssueSeverity.Warning);

    public bool RequiresExplicitConfirmation => Succeeded;

    public static Ra2IniEditPreview FromSuccess(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        Ra2TextChangeSet changeSet,
        string candidateText,
        IReadOnlyList<Ra2IniEditOperationPreview> operationPreviews,
        Ra2IniLanguageAnalysisResult currentAnalysis,
        Ra2IniLanguageAnalysisResult candidateAnalysis)
    {
        ArgumentNullException.ThrowIfNull(currentAnalysis);
        ArgumentNullException.ThrowIfNull(candidateAnalysis);
        ArgumentNullException.ThrowIfNull(operationPreviews);
        ArgumentNullException.ThrowIfNull(changeSet);
        ArgumentNullException.ThrowIfNull(candidateText);
        if (!currentAnalysis.Succeeded || !candidateAnalysis.Succeeded)
            throw new ArgumentException("Successful previews require two successful analyses.");
        if (changeSet.Changes.Count == 0 ||
            string.Equals(snapshot.Text, candidateText, StringComparison.Ordinal))
        {
            throw new ArgumentException("Successful previews require an effective text change.");
        }

        if (operationPreviews.Count != plan.Operations.Count)
            throw new ArgumentException("Every plan operation requires preview evidence.", nameof(operationPreviews));
        if (currentAnalysis.FieldRegistryRevision != snapshot.FieldRegistry.Revision ||
            candidateAnalysis.FieldRegistryRevision != snapshot.FieldRegistry.Revision ||
            !string.Equals(currentAnalysis.Request.Text, snapshot.Text, StringComparison.Ordinal) ||
            !string.Equals(candidateAnalysis.Request.Text, candidateText, StringComparison.Ordinal))
        {
            throw new ArgumentException("Preview analyses are not bound to the expected snapshot and candidate.");
        }

        (IReadOnlyList<Ra2DiagnosticFact> added, IReadOnlyList<Ra2DiagnosticFact> removed) =
            CompareDiagnostics(currentAnalysis.Diagnostics, candidateAnalysis.Diagnostics);

        return new Ra2IniEditPreview(
            snapshot,
            plan,
            Ra2IniEditPreviewFailureKind.None,
            $"已生成 {operationPreviews.Count} 项结构化编辑预览，尚未修改文档。",
            Guid.NewGuid(),
            changeSet,
            candidateText,
            operationPreviews,
            currentAnalysis,
            candidateAnalysis,
            added,
            removed);
    }

    public static Ra2IniEditPreview Failed(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        Ra2IniEditPreviewFailureKind failureKind,
        string message)
    {
        if (failureKind == Ra2IniEditPreviewFailureKind.None)
            throw new ArgumentOutOfRangeException(nameof(failureKind));

        return new Ra2IniEditPreview(
            snapshot,
            plan,
            failureKind,
            message,
            Guid.Empty,
            null,
            null,
            [],
            null,
            null,
            [],
            []);
    }

    internal static (
        IReadOnlyList<Ra2DiagnosticFact> Added,
        IReadOnlyList<Ra2DiagnosticFact> Removed)
        CompareDiagnostics(
            IReadOnlyList<Ra2DiagnosticFact> current,
            IReadOnlyList<Ra2DiagnosticFact> candidate)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(candidate);

        Dictionary<DiagnosticFingerprint, int> currentCounts = BuildCounts(current);
        List<Ra2DiagnosticFact> added = [];
        foreach (Ra2DiagnosticFact diagnostic in candidate)
        {
            DiagnosticFingerprint key = DiagnosticFingerprint.From(diagnostic);
            if (!TryConsume(currentCounts, key))
                added.Add(diagnostic);
        }

        Dictionary<DiagnosticFingerprint, int> candidateCounts = BuildCounts(candidate);
        List<Ra2DiagnosticFact> removed = [];
        foreach (Ra2DiagnosticFact diagnostic in current)
        {
            DiagnosticFingerprint key = DiagnosticFingerprint.From(diagnostic);
            if (!TryConsume(candidateCounts, key))
                removed.Add(diagnostic);
        }

        return (Array.AsReadOnly(added.ToArray()), Array.AsReadOnly(removed.ToArray()));
    }

    private static Dictionary<DiagnosticFingerprint, int> BuildCounts(
        IEnumerable<Ra2DiagnosticFact> diagnostics)
    {
        Dictionary<DiagnosticFingerprint, int> counts = [];
        foreach (Ra2DiagnosticFact diagnostic in diagnostics)
        {
            DiagnosticFingerprint key = DiagnosticFingerprint.From(diagnostic);
            counts.TryGetValue(key, out int count);
            counts[key] = count + 1;
        }

        return counts;
    }

    private static bool TryConsume(
        IDictionary<DiagnosticFingerprint, int> counts,
        DiagnosticFingerprint key)
    {
        if (!counts.TryGetValue(key, out int count) || count == 0)
            return false;

        counts[key] = count - 1;
        return true;
    }

    private readonly record struct DiagnosticFingerprint(
        string Code,
        string SourceKind,
        IniIssueSeverity Severity,
        string Message,
        string? SectionId,
        string? Key)
    {
        public static DiagnosticFingerprint From(Ra2DiagnosticFact fact)
            => new(
                fact.Code,
                fact.SourceKind,
                fact.Severity,
                fact.Message,
                fact.SectionId,
                fact.Key);
    }
}
