using System.IO;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.FieldTrust;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.TextModel;

namespace RA2IniEditor.IDE.Editing;

/// <summary>
/// 将受限字段操作解析为不修改编辑会话的确定性文本预览。
/// </summary>
internal sealed class Ra2IniEditPreviewService : IRa2IniEditPreviewService
{
    private const string AuthoringReasonPrefix = "Authoring";

    private readonly IRa2IniLanguageAnalysisService _languageAnalysisService;
    private readonly Ra2AddPropertyInsertPlanner _insertPlanner;

    public Ra2IniEditPreviewService(
        IRa2IniLanguageAnalysisService languageAnalysisService,
        Ra2AddPropertyInsertPlanner insertPlanner)
    {
        _languageAnalysisService = languageAnalysisService ??
            throw new ArgumentNullException(nameof(languageAnalysisService));
        _insertPlanner = insertPlanner ?? throw new ArgumentNullException(nameof(insertPlanner));
    }

    public Ra2IniEditPreview Preview(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(plan);

        try
        {
            Ra2IniEditPreview? preconditionFailure = ValidatePreconditions(snapshot, plan);
            if (preconditionFailure is not null)
                return preconditionFailure;

            cancellationToken.ThrowIfCancellationRequested();
            Ra2IniLanguageAnalysisResult currentAnalysis = Analyze(snapshot, snapshot.Text);
            if (!currentAnalysis.Succeeded)
            {
                return Failed(
                    snapshot,
                    plan,
                    Ra2IniEditPreviewFailureKind.CurrentAnalysisFailed,
                    "无法分析当前文档，未生成编辑预览。");
            }

            cancellationToken.ThrowIfCancellationRequested();
            PlanningResult planning = PlanChanges(snapshot, plan, currentAnalysis);
            if (!planning.Succeeded)
                return Failed(snapshot, plan, planning.FailureKind, planning.FailureMessage!);

            Ra2TextChangeSet changeSet;
            string candidateText;
            try
            {
                changeSet = new Ra2TextChangeSet(CoalesceInsertions(planning.Changes));
                candidateText = changeSet.Apply(snapshot.Text);
            }
            catch (ArgumentException)
            {
                return Failed(
                    snapshot,
                    plan,
                    Ra2IniEditPreviewFailureKind.OverlappingChanges,
                    "结构化操作生成了重叠或越界的文本变更。");
            }

            if (string.Equals(candidateText, snapshot.Text, StringComparison.Ordinal))
            {
                return Failed(
                    snapshot,
                    plan,
                    Ra2IniEditPreviewFailureKind.NoChanges,
                    "编辑计划不会改变当前文档。");
            }

            cancellationToken.ThrowIfCancellationRequested();
            Ra2IniLanguageAnalysisResult candidateAnalysis = Analyze(snapshot, candidateText);
            if (!candidateAnalysis.Succeeded)
            {
                return Failed(
                    snapshot,
                    plan,
                    Ra2IniEditPreviewFailureKind.CandidateAnalysisFailed,
                    "无法分析候选文档，未生成可确认的编辑预览。");
            }

            cancellationToken.ThrowIfCancellationRequested();
            return Ra2IniEditPreview.FromSuccess(
                snapshot,
                plan,
                changeSet,
                candidateText,
                planning.OperationPreviews,
                currentAnalysis,
                candidateAnalysis);
        }
        catch (OperationCanceledException)
        {
            return Failed(
                snapshot,
                plan,
                Ra2IniEditPreviewFailureKind.Canceled,
                "编辑预览已取消。");
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and
                                          not StackOverflowException and
                                          not AccessViolationException)
        {
            return Failed(
                snapshot,
                plan,
                Ra2IniEditPreviewFailureKind.UnexpectedFailure,
                "生成结构化编辑预览时发生意外错误。");
        }
    }

    private static Ra2IniEditPreview? ValidatePreconditions(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan)
    {
        if (!snapshot.IsEditable)
        {
            return Failed(
                snapshot,
                plan,
                Ra2IniEditPreviewFailureKind.ReadOnly,
                "当前文档不可编辑。");
        }

        if (plan.ExpectedDocumentId != snapshot.DocumentId ||
            plan.ExpectedEditRevision != snapshot.EditRevision ||
            plan.ExpectedFieldRegistryRevision != snapshot.FieldRegistry.Revision)
        {
            return Failed(
                snapshot,
                plan,
                Ra2IniEditPreviewFailureKind.StalePlanTarget,
                "编辑计划与当前文档或字段库版本不一致。");
        }

        return null;
    }

    private Ra2IniLanguageAnalysisResult Analyze(
        Ra2AuthoringSnapshot snapshot,
        string text)
        => _languageAnalysisService.Analyze(new Ra2LanguageAnalysisRequest(
            snapshot.ProjectRootPath,
            snapshot.FilePath,
            Path.GetFileName(snapshot.FilePath) ?? string.Empty,
            text,
            snapshot.EditRevision,
            snapshot.FieldRegistry));

    private PlanningResult PlanChanges(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        Ra2IniLanguageAnalysisResult currentAnalysis)
    {
        Ra2DocumentSemanticModel model = currentAnalysis.SemanticModel!;
        Ra2IniTextDocument document = currentAnalysis.TextDocument!;
        HashSet<string> logicalTargets = new(StringComparer.OrdinalIgnoreCase);
        List<IndexedChange> changes = [];
        List<Ra2IniEditOperationPreview> operationPreviews = [];

        for (int index = 0; index < plan.Operations.Count; index++)
        {
            Ra2IniEditOperation operation = plan.Operations[index];
            string logicalTarget = $"{operation.SectionName}\0{operation.Key}";
            if (!logicalTargets.Add(logicalTarget))
            {
                return PlanningResult.Failed(
                    Ra2IniEditPreviewFailureKind.ConflictingOperations,
                    $"编辑计划多次修改同一字段：[{operation.SectionName}] {operation.Key}。");
            }

            Ra2SectionSymbol[] sections = model.Sections
                .Where(section => string.Equals(
                    section.Name,
                    operation.SectionName,
                    StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (sections.Length == 0)
            {
                return PlanningResult.Failed(
                    Ra2IniEditPreviewFailureKind.SectionNotFound,
                    $"未找到目标 Section：[{operation.SectionName}]。");
            }

            if (sections.Length > 1)
            {
                return PlanningResult.Failed(
                    Ra2IniEditPreviewFailureKind.AmbiguousSection,
                    $"目标 Section 存在重复定义：[{operation.SectionName}]。");
            }

            Ra2SectionSymbol section = sections[0];
            Ra2KeyValueSymbol[] fields = model.KeyValues
                .Where(field =>
                    string.Equals(field.SectionName, section.Name, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(field.Key, operation.Key, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (fields.Length > 1)
            {
                return PlanningResult.Failed(
                    Ra2IniEditPreviewFailureKind.AmbiguousField,
                    $"目标字段存在重复定义：[{section.Name}] {operation.Key}。");
            }

            Ra2TextChange change;
            Ra2IniEditOperationOutcomeKind outcome;
            if (fields.Length == 1)
            {
                Ra2KeyValueSymbol field = fields[0];
                if (string.Equals(field.Value ?? string.Empty, operation.Value, StringComparison.Ordinal))
                {
                    return PlanningResult.Failed(
                        Ra2IniEditPreviewFailureKind.NoChanges,
                        $"字段值没有变化：[{section.Name}] {operation.Key}。");
                }

                Ra2TextSpan valueSpan = field.ValueSpan ??
                    new Ra2TextSpan(FindValueInsertionOffset(snapshot.Text, field), 0);
                change = new Ra2TextChange(
                    valueSpan,
                    operation.Value,
                    $"{AuthoringReasonPrefix}:{operation.Kind}");
                outcome = Ra2IniEditOperationOutcomeKind.Replaced;
            }
            else
            {
                if (operation.Kind == Ra2IniEditOperationKind.ReplaceFieldValue)
                {
                    return PlanningResult.Failed(
                        Ra2IniEditPreviewFailureKind.FieldNotFound,
                        $"未找到要替换的字段：[{section.Name}] {operation.Key}。");
                }

                Ra2IniDocumentLine anchor = ResolveInsertionAnchor(document, model, section);
                Ra2AddPropertyInsertPlan insertion = _insertPlanner.PlanInsert(
                    document,
                    anchor.Span.End,
                    operation.Key,
                    operation.Value);
                change = new Ra2TextChange(
                    insertion.Change.Span,
                    insertion.Change.NewText,
                    $"{AuthoringReasonPrefix}:{operation.Kind}");
                outcome = Ra2IniEditOperationOutcomeKind.Inserted;
            }

            bool isKnown = snapshot.FieldRegistry.Provider.TryGetField(
                section.Kind,
                operation.Key,
                out Ra2FieldDefinition? definition);
            Ra2FieldTrustLevel trustLevel = Ra2FieldTrustClassifier.Classify(
                isKnown ? definition : null).Level;

            changes.Add(new IndexedChange(index, change));
            operationPreviews.Add(new Ra2IniEditOperationPreview(
                index,
                operation,
                outcome,
                section.Kind,
                isKnown,
                trustLevel,
                change.Span,
                outcome == Ra2IniEditOperationOutcomeKind.Inserted
                    ? $"将在 [{section.Name}] 插入 {operation.Key}。"
                    : $"将替换 [{section.Name}] {operation.Key} 的值。"));
        }

        return PlanningResult.Success(changes, operationPreviews);
    }

    private static int FindValueInsertionOffset(string text, Ra2KeyValueSymbol field)
    {
        int searchStart = Math.Max(field.KeySpan.End, field.LineSpan.Start);
        int searchLength = Math.Max(0, field.LineSpan.End - searchStart);
        int equalsOffset = searchLength == 0
            ? -1
            : text.IndexOf('=', searchStart, searchLength);
        if (equalsOffset < 0)
            throw new InvalidOperationException("Key/value line does not contain '='.");

        return equalsOffset + 1;
    }

    private static Ra2IniDocumentLine ResolveInsertionAnchor(
        Ra2IniTextDocument document,
        Ra2DocumentSemanticModel model,
        Ra2SectionSymbol section)
    {
        Ra2KeyValueSymbol? lastField = model.KeyValues
            .Where(field => string.Equals(
                field.SectionName,
                section.Name,
                StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(field => field.LineNumber)
            .FirstOrDefault();
        int lineNumber = lastField?.LineNumber ?? section.HeaderLineNumber;
        return document.Lines.Single(line => line.LineNumber == lineNumber);
    }

    private static IReadOnlyList<Ra2TextChange> CoalesceInsertions(
        IReadOnlyList<IndexedChange> indexedChanges)
    {
        List<Ra2TextChange> results = [];
        foreach (IGrouping<(int Start, int Length), IndexedChange> group in indexedChanges
                     .GroupBy(item => (item.Change.Span.Start, item.Change.Span.Length))
                     .OrderBy(group => group.Key.Start)
                     .ThenBy(group => group.Key.Length))
        {
            IndexedChange[] ordered = group.OrderBy(item => item.OperationIndex).ToArray();
            if (group.Key.Length == 0 && ordered.Length > 1)
            {
                results.Add(new Ra2TextChange(
                    ordered[0].Change.Span,
                    string.Concat(ordered.Select(item => item.Change.NewText)),
                    $"{AuthoringReasonPrefix}:BatchInsert"));
            }
            else
            {
                results.AddRange(ordered.Select(item => item.Change));
            }
        }

        return results;
    }

    private static Ra2IniEditPreview Failed(
        Ra2AuthoringSnapshot snapshot,
        Ra2IniEditPlan plan,
        Ra2IniEditPreviewFailureKind failureKind,
        string message)
        => Ra2IniEditPreview.Failed(snapshot, plan, failureKind, message);

    private readonly record struct IndexedChange(int OperationIndex, Ra2TextChange Change);

    private sealed class PlanningResult
    {
        private PlanningResult(
            Ra2IniEditPreviewFailureKind failureKind,
            string? failureMessage,
            IReadOnlyList<IndexedChange> changes,
            IReadOnlyList<Ra2IniEditOperationPreview> operationPreviews)
        {
            FailureKind = failureKind;
            FailureMessage = failureMessage;
            Changes = changes;
            OperationPreviews = operationPreviews;
        }

        public bool Succeeded => FailureKind == Ra2IniEditPreviewFailureKind.None;

        public Ra2IniEditPreviewFailureKind FailureKind { get; }

        public string? FailureMessage { get; }

        public IReadOnlyList<IndexedChange> Changes { get; }

        public IReadOnlyList<Ra2IniEditOperationPreview> OperationPreviews { get; }

        public static PlanningResult Success(
            IReadOnlyList<IndexedChange> changes,
            IReadOnlyList<Ra2IniEditOperationPreview> operationPreviews)
            => new(Ra2IniEditPreviewFailureKind.None, null, changes, operationPreviews);

        public static PlanningResult Failed(
            Ra2IniEditPreviewFailureKind failureKind,
            string failureMessage)
            => new(failureKind, failureMessage, [], []);
    }
}
