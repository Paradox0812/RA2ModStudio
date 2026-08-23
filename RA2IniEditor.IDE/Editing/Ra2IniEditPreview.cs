using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.Editing;

/// <summary>
/// 表示单个结构化操作在 Host 当前快照上的展示证据。
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
/// 保留 A3/A4 Host 所需的活动快照、计划和展示投影；不包含语义规划算法。
/// </summary>
internal sealed class Ra2IniEditPreview
{
    private Ra2IniEditPreview(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        Ra2AutomationEditPreviewResult automationResult,
        string message,
        Ra2TextChangeSet? changeSet,
        IReadOnlyList<Ra2IniEditOperationPreview> operationPreviews,
        IReadOnlyList<Ra2DiagnosticFact> addedDiagnostics,
        IReadOnlyList<Ra2DiagnosticFact> removedDiagnostics)
    {
        Snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
        Plan = plan ?? throw new ArgumentNullException(nameof(plan));
        AutomationResult = automationResult ?? throw new ArgumentNullException(nameof(automationResult));
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Preview message cannot be empty.", nameof(message))
            : message;

        bool succeeded = automationResult.Succeeded;
        if (succeeded != (changeSet is not null) ||
            succeeded != (automationResult.CandidateText is not null) ||
            (!succeeded && (operationPreviews.Count != 0 ||
                automationResult.SectionCreationPreviews.Count != 0 ||
                addedDiagnostics.Count != 0 || removedDiagnostics.Count != 0)))
        {
            throw new ArgumentException("Host preview projection state is inconsistent.");
        }

        ChangeSet = changeSet;
        OperationPreviews = Array.AsReadOnly(operationPreviews.ToArray());
        AddedDiagnostics = Array.AsReadOnly(addedDiagnostics.ToArray());
        RemovedDiagnostics = Array.AsReadOnly(removedDiagnostics.ToArray());
    }

    public bool Succeeded => AutomationResult.Succeeded;
    public Ra2AuthoringSnapshot Snapshot { get; }
    public Ra2IniEditPlan Plan { get; }
    public Ra2IniEditPreviewFailureKind FailureKind => AutomationResult.FailureKind;
    public string Message { get; }
    public Guid PreviewId => AutomationResult.PreviewId;
    public Ra2TextChangeSet? ChangeSet { get; }
    public string? CandidateText => AutomationResult.CandidateText;
    public IReadOnlyList<Ra2IniEditOperationPreview> OperationPreviews { get; }
    public IReadOnlyList<Ra2AutomationSectionCreatePreview> SectionCreationPreviews
        => AutomationResult.SectionCreationPreviews;
    public IReadOnlyList<Ra2DiagnosticFact> AddedDiagnostics { get; }
    public IReadOnlyList<Ra2DiagnosticFact> RemovedDiagnostics { get; }
    public int AddedErrorCount => AutomationResult.AddedErrorCount;
    public int AddedWarningCount => AutomationResult.AddedWarningCount;
    public bool RequiresExplicitConfirmation => AutomationResult.RequiresExplicitConfirmation;
    internal Ra2AutomationEditPreviewResult AutomationResult { get; }

    public static Ra2IniEditPreview FromAutomation(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        Ra2AutomationEditPreviewResult result)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(result);
        if (result.DocumentId != snapshot.DocumentId ||
            result.Version != snapshot.EditRevision ||
            result.FieldRegistryRevision != snapshot.FieldRegistry.Revision ||
            result.PlanId != plan.PlanId ||
            !string.Equals(result.FilePath, snapshot.FilePath, StringComparison.Ordinal))
        {
            throw new ArgumentException("Automation preview identity does not match the Host snapshot and plan.", nameof(result));
        }

        if (!result.Succeeded)
        {
            return new Ra2IniEditPreview(
                snapshot,
                plan,
                result,
                LocalizeFailure(result.FailureKind),
                null,
                [],
                [],
                []);
        }

        ValidateOperationEvidence(snapshot, plan, result);
        ValidateSectionCreationEvidence(snapshot, plan, result);

        Ra2TextChangeSet changeSet = new(result.Changes.Select(change => new Ra2TextChange(
            new Ra2TextSpan(change.Span.Start, change.Span.Length),
            change.NewText,
            change.Reason)));
        string projectedText;
        try
        {
            projectedText = changeSet.Apply(snapshot.Text);
        }
        catch (ArgumentException exception)
        {
            throw new ArgumentException(
                "Automation preview changes are outside the Host snapshot.",
                nameof(result),
                exception);
        }

        if (!string.Equals(projectedText, result.CandidateText, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Automation preview changes do not reproduce the candidate text.",
                nameof(result));
        }

        IReadOnlyList<Ra2IniEditOperationPreview> operationPreviews = result.OperationPreviews
            .Select(MapOperationPreview)
            .ToArray();
        IReadOnlyList<Ra2DiagnosticFact> addedDiagnostics = result.AddedDiagnostics
            .Select(MapDiagnostic)
            .ToArray();
        IReadOnlyList<Ra2DiagnosticFact> removedDiagnostics = result.RemovedDiagnostics
            .Select(MapDiagnostic)
            .ToArray();

        return new Ra2IniEditPreview(
            snapshot,
            plan,
            result,
            $"已生成 {operationPreviews.Count + result.SectionCreationPreviews.Count} 项结构化编辑预览，尚未修改文档。",
            changeSet,
            operationPreviews,
            addedDiagnostics,
            removedDiagnostics);
    }

    private static void ValidateOperationEvidence(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        Ra2AutomationEditPreviewResult result)
    {
        if (result.OperationPreviews.Count != plan.Operations.Count)
        {
            throw new ArgumentException(
                "Automation preview operation evidence does not match the Host plan.",
                nameof(result));
        }

        for (int index = 0; index < plan.Operations.Count; index++)
        {
            Ra2IniEditOperation expected = plan.Operations[index];
            Ra2AutomationEditOperationPreview actual = result.OperationPreviews[index];
            if (actual.OperationIndex != index ||
                actual.Operation.Kind != expected.Kind ||
                !string.Equals(actual.Operation.SectionName, expected.SectionName, StringComparison.Ordinal) ||
                !string.Equals(actual.Operation.Key, expected.Key, StringComparison.Ordinal) ||
                !string.Equals(actual.Operation.Value, expected.Value, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Automation preview operation evidence does not match the Host plan.",
                    nameof(result));
            }

            if (actual.AffectedOriginalSpan.Start > snapshot.Text.Length ||
                actual.AffectedOriginalSpan.End > snapshot.Text.Length)
            {
                throw new ArgumentException(
                    "Automation preview operation evidence is outside the Host snapshot.",
                    nameof(result));
            }
        }
    }

    private static void ValidateSectionCreationEvidence(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        Ra2AutomationEditPreviewResult result)
    {
        if (result.SectionCreationPreviews.Count != plan.SectionCreations.Count)
        {
            throw new ArgumentException(
                "Automation preview section creation evidence does not match the Host plan.",
                nameof(result));
        }

        for (int index = 0; index < plan.SectionCreations.Count; index++)
        {
            Ra2AutomationSectionCreateOperation expected = plan.SectionCreations[index];
            Ra2AutomationSectionCreatePreview actual = result.SectionCreationPreviews[index];
            if (actual.OperationIndex != index ||
                !string.Equals(actual.Operation.SectionName, expected.SectionName, StringComparison.Ordinal) ||
                actual.Operation.ExpectedSectionKind != expected.ExpectedSectionKind ||
                actual.AffectedOriginalSpan.Start > snapshot.Text.Length ||
                actual.AffectedOriginalSpan.End > snapshot.Text.Length)
            {
                throw new ArgumentException(
                    "Automation preview section creation evidence does not match the Host plan.",
                    nameof(result));
            }
        }
    }

    public static Ra2IniEditPreview Failed(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        Ra2IniEditPreviewFailureKind failureKind,
        string message)
    {
        if (failureKind == Ra2IniEditPreviewFailureKind.None)
            throw new ArgumentOutOfRangeException(nameof(failureKind));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Preview message cannot be empty.", nameof(message));

        Ra2AutomationEditPreviewResult result = new(
            snapshot.ToAutomationSnapshot(),
            plan,
            failureKind,
            message,
            Guid.Empty,
            null,
            [],
            [],
            [],
            []);
        return new Ra2IniEditPreview(snapshot, plan, result, message, null, [], [], []);
    }

    private static Ra2IniEditOperationPreview MapOperationPreview(
        Ra2AutomationEditOperationPreview preview)
    {
        Ra2FieldTrustLevel trustLevel = preview.FieldTrustLevel switch
        {
            Ra2AutomationFieldTrustLevel.Verified => Ra2FieldTrustLevel.Verified,
            Ra2AutomationFieldTrustLevel.VerifiedGuardrail => Ra2FieldTrustLevel.VerifiedGuardrail,
            Ra2AutomationFieldTrustLevel.Inferred => Ra2FieldTrustLevel.Inferred,
            Ra2AutomationFieldTrustLevel.ManualCurated => Ra2FieldTrustLevel.ManualCurated,
            Ra2AutomationFieldTrustLevel.AutoExtracted => Ra2FieldTrustLevel.AutoExtracted,
            Ra2AutomationFieldTrustLevel.Obsolete => Ra2FieldTrustLevel.Obsolete,
            Ra2AutomationFieldTrustLevel.NonExistent => Ra2FieldTrustLevel.NonExistent,
            Ra2AutomationFieldTrustLevel.PseudoField => Ra2FieldTrustLevel.PseudoField,
            _ => Ra2FieldTrustLevel.Unknown
        };
        string summary = preview.OutcomeKind == Ra2IniEditOperationOutcomeKind.Inserted
            ? $"将在 [{preview.Operation.SectionName}] 插入 {preview.Operation.Key}。"
            : $"将替换 [{preview.Operation.SectionName}] {preview.Operation.Key} 的值。";

        return new Ra2IniEditOperationPreview(
            preview.OperationIndex,
            preview.Operation,
            preview.OutcomeKind,
            preview.ResolvedSectionKind,
            preview.IsKnownField,
            trustLevel,
            new Ra2TextSpan(preview.AffectedOriginalSpan.Start, preview.AffectedOriginalSpan.Length),
            summary);
    }

    private static Ra2DiagnosticFact MapDiagnostic(Ra2AutomationDiagnosticFact diagnostic)
        => new(
            diagnostic.Code,
            diagnostic.SourceKind,
            diagnostic.Severity,
            diagnostic.Message,
            diagnostic.FilePath,
            diagnostic.LineNumber,
            diagnostic.ColumnNumber,
            diagnostic.SectionId,
            diagnostic.Key,
            diagnostic.AnalysisVersion);

    private static string LocalizeFailure(Ra2IniEditPreviewFailureKind failureKind)
        => failureKind switch
        {
            Ra2IniEditPreviewFailureKind.InvalidPlan => "编辑计划无效，未生成预览。",
            Ra2IniEditPreviewFailureKind.StalePlanTarget => "编辑计划与当前文档或字段库版本不一致。",
            Ra2IniEditPreviewFailureKind.ReadOnly => "当前文档不可编辑。",
            Ra2IniEditPreviewFailureKind.UnsupportedOperation => "编辑计划包含不支持的操作。",
            Ra2IniEditPreviewFailureKind.InvalidSection => "目标 Section 无效。",
            Ra2IniEditPreviewFailureKind.SectionNotFound => "未找到目标 Section。",
            Ra2IniEditPreviewFailureKind.AmbiguousSection => "目标 Section 存在重复定义。",
            Ra2IniEditPreviewFailureKind.FieldNotFound => "未找到要替换的字段。",
            Ra2IniEditPreviewFailureKind.AmbiguousField => "目标字段存在重复定义。",
            Ra2IniEditPreviewFailureKind.ConflictingOperations => "编辑计划多次修改同一字段。",
            Ra2IniEditPreviewFailureKind.OverlappingChanges => "结构化操作生成了重叠或越界的文本变更。",
            Ra2IniEditPreviewFailureKind.NoChanges => "编辑计划不会改变当前文档。",
            Ra2IniEditPreviewFailureKind.Canceled => "编辑预览已取消。",
            Ra2IniEditPreviewFailureKind.CurrentAnalysisFailed => "无法分析当前文档，未生成编辑预览。",
            Ra2IniEditPreviewFailureKind.CandidateAnalysisFailed => "无法分析候选文档，未生成可确认的编辑预览。",
            Ra2IniEditPreviewFailureKind.DocumentTooLarge => "文档超过结构化编辑预览的资源上限。",
            Ra2IniEditPreviewFailureKind.ResultLimitExceeded => "诊断结果超过结构化编辑预览的资源上限。",
            Ra2IniEditPreviewFailureKind.SectionAlreadyExists => "要创建的 Section 已存在。",
            Ra2IniEditPreviewFailureKind.ConflictingSectionCreations => "编辑计划重复创建同一 Section。",
            Ra2IniEditPreviewFailureKind.SectionClassificationMismatch => "新 Section 的实际分类与计划不一致。",
            Ra2IniEditPreviewFailureKind.BlockedFieldTrust => "新 Section 包含字段库阻止自动写入的字段。",
            _ => "生成结构化编辑预览时发生意外错误。"
        };
}
