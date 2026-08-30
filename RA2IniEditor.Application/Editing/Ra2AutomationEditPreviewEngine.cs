using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Application.Diagnostics;
using RA2IniEditor.Application.FieldTrust;
using RA2IniEditor.Application.Language;
using RA2IniEditor.Application.TextModel;
using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Application.Editing;

internal sealed class Ra2AutomationEditPreviewEngine
{
    private const string AuthoringReasonPrefix = "Authoring";

    public Ra2AutomationEditPreviewResult Preview(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan,
        int maximumDocumentCharacters,
        int maximumDiagnosticItems,
        CancellationToken cancellationToken)
    {
        if (maximumDocumentCharacters <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumDocumentCharacters));
        if (maximumDiagnosticItems <= 0)
            throw new ArgumentOutOfRangeException(nameof(maximumDiagnosticItems));

        if (snapshot.Text.Length > maximumDocumentCharacters)
        {
            return Failed(
                snapshot,
                plan,
                Ra2AutomationEditPreviewFailureKind.DocumentTooLarge,
                "The document exceeds the supported character limit.");
        }

        if (!snapshot.IsEditable)
        {
            return Failed(
                snapshot,
                plan,
                Ra2AutomationEditPreviewFailureKind.ReadOnly,
                "The document is read-only.");
        }

        if (plan.ExpectedDocumentId != snapshot.DocumentId ||
            plan.ExpectedVersion != snapshot.Version ||
            plan.ExpectedFieldRegistryRevision != snapshot.FieldRegistry.Revision)
        {
            return Failed(
                snapshot,
                plan,
                Ra2AutomationEditPreviewFailureKind.StalePlanTarget,
                "The edit plan does not match the current document snapshot.");
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Analysis currentAnalysis;
            try
            {
                currentAnalysis = Analyze(snapshot, snapshot.Text, maximumDiagnosticItems, cancellationToken);
            }
            catch (Ra2DiagnosticResultLimitExceededException)
            {
                return Failed(
                    snapshot,
                    plan,
                    Ra2AutomationEditPreviewFailureKind.ResultLimitExceeded,
                    "The diagnostics result exceeds the supported item limit.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Canceled(snapshot, plan);
            }
            catch (Exception exception) when (!IsFatalException(exception))
            {
                return Failed(
                    snapshot,
                    plan,
                    Ra2AutomationEditPreviewFailureKind.CurrentAnalysisFailed,
                    "The current document could not be analyzed.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            PlanningResult planning = PlanChanges(snapshot, plan, currentAnalysis, cancellationToken);
            if (!planning.Succeeded)
                return Failed(snapshot, plan, planning.FailureKind, planning.FailureMessage!);

            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<IndexedChange> coalescedChanges = CoalesceInsertions(planning.Changes);
            Ra2TextChangeSet changeSet;
            string candidateText;
            try
            {
                changeSet = new Ra2TextChangeSet(coalescedChanges.Select(item => item.Change));
                cancellationToken.ThrowIfCancellationRequested();
                candidateText = changeSet.Apply(snapshot.Text);
                cancellationToken.ThrowIfCancellationRequested();
            }
            catch (ArgumentException)
            {
                return Failed(
                    snapshot,
                    plan,
                    Ra2AutomationEditPreviewFailureKind.OverlappingChanges,
                    "The edit plan produced overlapping or out-of-range text changes.");
            }

            if (candidateText.Length > maximumDocumentCharacters)
            {
                return Failed(
                    snapshot,
                    plan,
                    Ra2AutomationEditPreviewFailureKind.DocumentTooLarge,
                    "The candidate document exceeds the supported character limit.");
            }

            if (string.Equals(candidateText, snapshot.Text, StringComparison.Ordinal))
            {
                return Failed(
                    snapshot,
                    plan,
                    Ra2AutomationEditPreviewFailureKind.NoChanges,
                    "The edit plan does not change the current document.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            Analysis candidateAnalysis;
            try
            {
                candidateAnalysis = Analyze(snapshot, candidateText, maximumDiagnosticItems, cancellationToken);
            }
            catch (Ra2DiagnosticResultLimitExceededException)
            {
                return Failed(
                    snapshot,
                    plan,
                    Ra2AutomationEditPreviewFailureKind.ResultLimitExceeded,
                    "The diagnostics result exceeds the supported item limit.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return Canceled(snapshot, plan);
            }
            catch (Exception exception) when (!IsFatalException(exception))
            {
                return Failed(
                    snapshot,
                    plan,
                    Ra2AutomationEditPreviewFailureKind.CandidateAnalysisFailed,
                    "The candidate document could not be analyzed.");
            }

            SectionPreviewPlanningResult sectionPreviewPlanning = BuildSectionCreationPreviews(
                snapshot,
                plan,
                candidateAnalysis,
                planning);
            if (!sectionPreviewPlanning.Succeeded)
            {
                return Failed(
                    snapshot,
                    plan,
                    sectionPreviewPlanning.FailureKind,
                    sectionPreviewPlanning.FailureMessage!);
            }

            cancellationToken.ThrowIfCancellationRequested();
            Ra2AutomationDiagnosticDelta delta = Ra2AutomationDiagnosticDeltaCalculator.Compare(
                currentAnalysis.Diagnostics,
                candidateAnalysis.Diagnostics,
                cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();

            return new Ra2AutomationEditPreviewResult(
                snapshot,
                plan,
                Ra2AutomationEditPreviewFailureKind.None,
                "The edit preview completed.",
                Guid.NewGuid(),
                candidateText,
                coalescedChanges
                    .Select(item => new Ra2AutomationTextChange(
                        ToAutomationSpan(item.Change.Span),
                        item.Change.NewText,
                        item.Change.Reason))
                    .ToArray(),
                planning.OperationPreviews,
                sectionPreviewPlanning.Previews,
                delta.Added,
                delta.Removed);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Canceled(snapshot, plan);
        }
        catch (Exception exception) when (!IsFatalException(exception))
        {
            return Failed(
                snapshot,
                plan,
                Ra2AutomationEditPreviewFailureKind.UnexpectedFailure,
                "The edit preview could not be completed.");
        }
    }

    private static Analysis Analyze(
        Ra2AutomationDocumentSnapshot snapshot,
        string text,
        int maximumDiagnosticItems,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Ra2IniTextDocument document = new Ra2IniTextDocumentParser().Parse(text);
        cancellationToken.ThrowIfCancellationRequested();

        Ra2DocumentSnapshot languageSnapshot = new(snapshot.FilePath, text, snapshot.Version);
        Ra2DocumentSemanticModel model = new Ra2DocumentSemanticModelBuilder().Build(
            languageSnapshot,
            snapshot.FieldRegistry.Provider);
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<Ra2DiagnosticFact> facts = new Ra2DocumentDiagnosticService().Analyze(
            languageSnapshot,
            snapshot.FieldRegistry.Provider,
            cancellationToken: cancellationToken,
            maximumResultItems: maximumDiagnosticItems);
        cancellationToken.ThrowIfCancellationRequested();

        List<Ra2AutomationDiagnosticFact> diagnostics = new(facts.Count);
        for (int index = 0; index < facts.Count; index++)
        {
            CheckCancellation(index, cancellationToken);
            if (diagnostics.Count >= maximumDiagnosticItems)
                throw new Ra2DiagnosticResultLimitExceededException();

            Ra2DiagnosticFact fact = facts[index];
            diagnostics.Add(new Ra2AutomationDiagnosticFact(
                fact.Code,
                fact.SourceKind,
                fact.Severity,
                fact.Message,
                fact.FilePath,
                fact.LineNumber,
                fact.ColumnNumber,
                fact.SectionId,
                fact.Key,
                fact.AnalysisVersion));
        }

        cancellationToken.ThrowIfCancellationRequested();
        return new Analysis(document, model, diagnostics);
    }

    private static PlanningResult PlanChanges(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan,
        Analysis currentAnalysis,
        CancellationToken cancellationToken)
    {
        Dictionary<string, PlannedSectionCreation> sectionCreations = new(StringComparer.OrdinalIgnoreCase);
        for (int index = 0; index < plan.SectionCreations.Count; index++)
        {
            CheckCancellation(index, cancellationToken);
            Ra2AutomationSectionCreateOperation creation = plan.SectionCreations[index];
            if (!sectionCreations.TryAdd(
                    creation.SectionName,
                    new PlannedSectionCreation(
                        index,
                        creation,
                        creation.ExpectedSectionKind == Ra2SectionKind.Unknown
                            ? Ra2AutomationFieldAuthoringDisposition.Caution
                            : Ra2AutomationFieldAuthoringDisposition.Normal)))
            {
                return PlanningResult.Failed(
                    Ra2AutomationEditPreviewFailureKind.ConflictingSectionCreations,
                    $"The edit plan creates [{creation.SectionName}] more than once.");
            }

            if (currentAnalysis.Model.Sections.Any(section =>
                    string.Equals(section.Name, creation.SectionName, StringComparison.OrdinalIgnoreCase)))
            {
                return PlanningResult.Failed(
                    Ra2AutomationEditPreviewFailureKind.SectionAlreadyExists,
                    $"The section [{creation.SectionName}] already exists.");
            }
        }

        HashSet<string> logicalTargets = new(StringComparer.OrdinalIgnoreCase);
        List<IndexedChange> changes = [];
        List<Ra2AutomationEditOperationPreview> operationPreviews = [];
        Dictionary<string, List<Ra2AutomationEditOperation>> createdSectionFields =
            new(StringComparer.OrdinalIgnoreCase);

        for (int index = 0; index < plan.Operations.Count; index++)
        {
            CheckCancellation(index, cancellationToken);
            Ra2AutomationEditOperation operation = plan.Operations[index];
            if (!logicalTargets.Add($"{operation.SectionName}\0{operation.Key}"))
            {
                return PlanningResult.Failed(
                    Ra2AutomationEditPreviewFailureKind.ConflictingOperations,
                    $"The edit plan targets [{operation.SectionName}] {operation.Key} more than once.");
            }

            if (sectionCreations.TryGetValue(operation.SectionName, out PlannedSectionCreation? plannedCreation))
            {
                if (operation.Kind != Ra2AutomationEditOperationKind.UpsertField)
                {
                    return PlanningResult.Failed(
                        Ra2AutomationEditPreviewFailureKind.FieldNotFound,
                        $"The new section [{operation.SectionName}] cannot replace a field that does not exist.");
                }

                bool createdFieldIsKnown = snapshot.FieldRegistry.Provider.TryGetField(
                    plannedCreation.Operation.ExpectedSectionKind,
                    operation.Key,
                    out Ra2FieldDefinition? createdFieldDefinition);
                Ra2AutomationFieldTrustLevel createdFieldTrustLevel = Ra2AutomationFieldTrustMapper.ToAutomationLevel(
                    Ra2FieldTrustClassifier.Classify(createdFieldIsKnown ? createdFieldDefinition : null).Level);
                Ra2AutomationFieldAuthoringDisposition disposition =
                    Ra2AutomationFieldTrustMapper.ToAuthoringDisposition(createdFieldTrustLevel);
                if (disposition == Ra2AutomationFieldAuthoringDisposition.Blocked &&
                    plannedCreation.Operation.ExpectedSectionKind != Ra2SectionKind.Unknown)
                {
                    return PlanningResult.Failed(
                        Ra2AutomationEditPreviewFailureKind.BlockedFieldTrust,
                        $"The field [{operation.SectionName}] {operation.Key} is blocked for new-section authoring.");
                }

                if (disposition != Ra2AutomationFieldAuthoringDisposition.Normal)
                    plannedCreation.Disposition = Ra2AutomationFieldAuthoringDisposition.Caution;

                if (!createdSectionFields.TryGetValue(
                        plannedCreation.Operation.SectionName,
                        out List<Ra2AutomationEditOperation>? createdFields))
                {
                    createdFields = [];
                    createdSectionFields[plannedCreation.Operation.SectionName] = createdFields;
                }
                createdFields.Add(operation);
                operationPreviews.Add(new Ra2AutomationEditOperationPreview(
                    index,
                    operation,
                    Ra2AutomationEditOperationOutcomeKind.Inserted,
                    plannedCreation.Operation.ExpectedSectionKind,
                    createdFieldIsKnown,
                    createdFieldTrustLevel,
                    new Ra2AutomationTextSpan(snapshot.Text.Length, 0),
                    $"Will insert {operation.Key} into the new section [{operation.SectionName}]."));
                continue;
            }

            Ra2SectionSymbol[] sections = currentAnalysis.Model.Sections
                .Where(section => string.Equals(section.Name, operation.SectionName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (sections.Length == 0)
            {
                return PlanningResult.Failed(
                    Ra2AutomationEditPreviewFailureKind.SectionNotFound,
                    $"The target section [{operation.SectionName}] was not found.");
            }

            if (sections.Length > 1)
            {
                return PlanningResult.Failed(
                    Ra2AutomationEditPreviewFailureKind.AmbiguousSection,
                    $"The target section [{operation.SectionName}] is ambiguous.");
            }

            Ra2SectionSymbol section = sections[0];
            Ra2KeyValueSymbol[] fields = currentAnalysis.Model.KeyValues
                .Where(field =>
                    string.Equals(field.SectionName, section.Name, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(field.Key, operation.Key, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (fields.Length > 1)
            {
                return PlanningResult.Failed(
                    Ra2AutomationEditPreviewFailureKind.AmbiguousField,
                    $"The target field [{section.Name}] {operation.Key} is ambiguous.");
            }

            Ra2TextChange change;
            Ra2AutomationEditOperationOutcomeKind outcome;
            if (fields.Length == 1)
            {
                Ra2KeyValueSymbol field = fields[0];
                if (string.Equals(field.Value ?? string.Empty, operation.Value, StringComparison.Ordinal))
                {
                    return PlanningResult.Failed(
                        Ra2AutomationEditPreviewFailureKind.NoChanges,
                        $"The value of [{section.Name}] {operation.Key} is unchanged.");
                }

                Ra2TextSpan valueSpan = field.ValueSpan ??
                    new Ra2TextSpan(FindValueInsertionOffset(snapshot.Text, field), 0);
                change = new Ra2TextChange(
                    valueSpan,
                    operation.Value,
                    $"{AuthoringReasonPrefix}:{operation.Kind}");
                outcome = Ra2AutomationEditOperationOutcomeKind.Replaced;
            }
            else
            {
                if (operation.Kind == Ra2AutomationEditOperationKind.ReplaceFieldValue)
                {
                    return PlanningResult.Failed(
                        Ra2AutomationEditPreviewFailureKind.FieldNotFound,
                        $"The field [{section.Name}] {operation.Key} was not found.");
                }

                Ra2IniDocumentLine anchor = ResolveInsertionAnchor(
                    currentAnalysis.Document,
                    currentAnalysis.Model,
                    section);
                (Ra2TextChange insertChange, _) = Ra2LineInsertionPrimitive.PlanAfterAnchor(
                    currentAnalysis.Document,
                    anchor,
                    $"{operation.Key}={operation.Value}",
                    $"{AuthoringReasonPrefix}:{operation.Kind}");
                change = insertChange;
                outcome = Ra2AutomationEditOperationOutcomeKind.Inserted;
            }

            bool isKnown = snapshot.FieldRegistry.Provider.TryGetField(
                section.Kind,
                operation.Key,
                out Ra2FieldDefinition? definition);
            Ra2AutomationFieldTrustLevel trustLevel = Ra2AutomationFieldTrustMapper.ToAutomationLevel(
                Ra2FieldTrustClassifier.Classify(isKnown ? definition : null).Level);

            changes.Add(new IndexedChange(index, change));
            operationPreviews.Add(new Ra2AutomationEditOperationPreview(
                index,
                operation,
                outcome,
                section.Kind,
                isKnown,
                trustLevel,
                ToAutomationSpan(change.Span),
                outcome == Ra2AutomationEditOperationOutcomeKind.Inserted
                    ? $"Will insert {operation.Key} into [{section.Name}]."
                    : $"Will replace the value of [{section.Name}] {operation.Key}."));
        }

        if (plan.SectionCreations.Count > 0)
        {
            string newline = ResolveCanonicalNewLine(currentAnalysis.Document);
            string appendText = BuildSectionAppendText(
                snapshot.Text,
                plan.SectionCreations,
                createdSectionFields,
                newline);
            changes.Add(new IndexedChange(
                plan.Operations.Count + plan.SectionCreations.Count,
                new Ra2TextChange(
                    new Ra2TextSpan(snapshot.Text.Length, 0),
                    appendText,
                    $"{AuthoringReasonPrefix}:CreateSection")));
        }

        return PlanningResult.Success(changes, operationPreviews, sectionCreations.Values.ToArray());
    }

    private static SectionPreviewPlanningResult BuildSectionCreationPreviews(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan,
        Analysis candidateAnalysis,
        PlanningResult planning)
    {
        if (plan.SectionCreations.Count == 0)
            return SectionPreviewPlanningResult.Success([]);

        Dictionary<string, PlannedSectionCreation> planned = planning.SectionCreations
            .ToDictionary(item => item.Operation.SectionName, StringComparer.OrdinalIgnoreCase);
        List<Ra2AutomationSectionCreatePreview> previews = [];
        for (int index = 0; index < plan.SectionCreations.Count; index++)
        {
            Ra2AutomationSectionCreateOperation operation = plan.SectionCreations[index];
            Ra2SectionSymbol[] sections = candidateAnalysis.Model.Sections
                .Where(section => string.Equals(section.Name, operation.SectionName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (sections.Length != 1)
            {
                return SectionPreviewPlanningResult.Failed(
                    Ra2AutomationEditPreviewFailureKind.CandidateAnalysisFailed,
                    $"The candidate section [{operation.SectionName}] could not be resolved uniquely.");
            }

            Ra2SectionKind actualKind = sections[0].Kind;
            if (operation.ExpectedSectionKind != Ra2SectionKind.Unknown &&
                actualKind != Ra2SectionKind.Unknown &&
                actualKind != operation.ExpectedSectionKind)
            {
                return SectionPreviewPlanningResult.Failed(
                    Ra2AutomationEditPreviewFailureKind.SectionClassificationMismatch,
                    $"The candidate section [{operation.SectionName}] was classified as {actualKind}, not {operation.ExpectedSectionKind}.");
            }

            Ra2AutomationFieldAuthoringDisposition disposition = planned[operation.SectionName].Disposition;
            if (actualKind == Ra2SectionKind.Unknown)
                disposition = Ra2AutomationFieldAuthoringDisposition.Caution;
            previews.Add(new Ra2AutomationSectionCreatePreview(
                index,
                operation,
                actualKind,
                actualKind != Ra2SectionKind.Unknown,
                disposition,
                new Ra2AutomationTextSpan(snapshot.Text.Length, 0),
                actualKind == Ra2SectionKind.Unknown
                    ? $"Will create [{operation.SectionName}]; classification remains unresolved."
                    : $"Will create [{operation.SectionName}] as {actualKind}."));
        }

        return SectionPreviewPlanningResult.Success(previews);
    }

    private static string BuildSectionAppendText(
        string sourceText,
        IReadOnlyList<Ra2AutomationSectionCreateOperation> sectionCreations,
        IReadOnlyDictionary<string, List<Ra2AutomationEditOperation>> fieldsBySection,
        string newline)
    {
        System.Text.StringBuilder builder = new();
        if (sourceText.Length > 0)
        {
            if (EndsWithTwoLineBreaks(sourceText))
            {
                // Existing trailing blank lines are preserved without adding another separator.
            }
            else if (EndsWithLineBreak(sourceText))
            {
                builder.Append(newline);
            }
            else
            {
                builder.Append(newline).Append(newline);
            }
        }

        for (int index = 0; index < sectionCreations.Count; index++)
        {
            if (index > 0)
                builder.Append(newline);
            Ra2AutomationSectionCreateOperation creation = sectionCreations[index];
            builder.Append('[').Append(creation.SectionName).Append(']').Append(newline);
            if (fieldsBySection.TryGetValue(creation.SectionName, out List<Ra2AutomationEditOperation>? fields))
            {
                foreach (Ra2AutomationEditOperation field in fields)
                    builder.Append(field.Key).Append('=').Append(field.Value).Append(newline);
            }
        }

        return builder.ToString();
    }

    private static string ResolveCanonicalNewLine(Ra2IniTextDocument document)
        => document.NewLineKind switch
        {
            Ra2IniNewLineKind.CrLf => "\r\n",
            Ra2IniNewLineKind.Cr => "\r",
            _ => "\n"
        };

    private static bool EndsWithLineBreak(string text)
        => text.EndsWith("\r\n", StringComparison.Ordinal) ||
            text.EndsWith('\n') ||
            text.EndsWith('\r');

    private static bool EndsWithTwoLineBreaks(string text)
    {
        int firstLength = TrailingLineBreakLength(text, text.Length);
        return firstLength > 0 && TrailingLineBreakLength(text, text.Length - firstLength) > 0;
    }

    private static int TrailingLineBreakLength(string text, int end)
    {
        if (end <= 0)
            return 0;
        if (text[end - 1] == '\n')
            return end >= 2 && text[end - 2] == '\r' ? 2 : 1;
        return text[end - 1] == '\r' ? 1 : 0;
    }

    private static int FindValueInsertionOffset(string text, Ra2KeyValueSymbol field)
    {
        int searchStart = Math.Max(field.KeySpan.End, field.LineSpan.Start);
        int searchLength = Math.Max(0, field.LineSpan.End - searchStart);
        int equalsOffset = searchLength == 0 ? -1 : text.IndexOf('=', searchStart, searchLength);
        if (equalsOffset < 0)
            throw new InvalidOperationException("The key/value line does not contain an equals sign.");

        return equalsOffset + 1;
    }

    private static Ra2IniDocumentLine ResolveInsertionAnchor(
        Ra2IniTextDocument document,
        Ra2DocumentSemanticModel model,
        Ra2SectionSymbol section)
    {
        Ra2KeyValueSymbol? lastField = model.KeyValues
            .Where(field => string.Equals(field.SectionName, section.Name, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(field => field.LineNumber)
            .FirstOrDefault();
        int lineNumber = lastField?.LineNumber ?? section.HeaderLineNumber;
        return document.Lines.Single(line => line.LineNumber == lineNumber);
    }

    private static IReadOnlyList<IndexedChange> CoalesceInsertions(IReadOnlyList<IndexedChange> indexedChanges)
    {
        List<IndexedChange> results = [];
        foreach (IGrouping<(int Start, int Length), IndexedChange> group in indexedChanges
                     .GroupBy(item => (item.Change.Span.Start, item.Change.Span.Length))
                     .OrderBy(group => group.Key.Start)
                     .ThenBy(group => group.Key.Length))
        {
            IndexedChange[] ordered = group.OrderBy(item => item.OperationIndex).ToArray();
            if (group.Key.Length == 0 && ordered.Length > 1)
            {
                results.Add(new IndexedChange(
                    ordered[0].OperationIndex,
                    new Ra2TextChange(
                        ordered[0].Change.Span,
                        string.Concat(ordered.Select(item => item.Change.NewText)),
                        $"{AuthoringReasonPrefix}:BatchInsert")));
            }
            else
            {
                results.AddRange(ordered);
            }
        }

        return results;
    }

    private static Ra2AutomationTextSpan ToAutomationSpan(Ra2TextSpan span)
        => new(span.Start, span.Length);

    private static Ra2AutomationEditPreviewResult Canceled(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan)
        => Failed(snapshot, plan, Ra2AutomationEditPreviewFailureKind.Canceled, "The edit preview was canceled.");

    private static Ra2AutomationEditPreviewResult Failed(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationEditPlan plan,
        Ra2AutomationEditPreviewFailureKind failureKind,
        string message)
        => new(snapshot, plan, failureKind, message, Guid.Empty, null, [], [], [], [], []);

    private static void CheckCancellation(int index, CancellationToken cancellationToken)
    {
        if (index % Ra2DocumentDiagnosticService.CancellationCheckInterval == 0)
            cancellationToken.ThrowIfCancellationRequested();
    }

    private static bool IsFatalException(Exception exception)
        => exception is OutOfMemoryException or
            AccessViolationException or
            AppDomainUnloadedException or
            BadImageFormatException or
            StackOverflowException;

    private readonly record struct IndexedChange(int OperationIndex, Ra2TextChange Change);

    private sealed class PlannedSectionCreation
    {
        public PlannedSectionCreation(
            int operationIndex,
            Ra2AutomationSectionCreateOperation operation,
            Ra2AutomationFieldAuthoringDisposition disposition)
        {
            OperationIndex = operationIndex;
            Operation = operation;
            Disposition = disposition;
        }

        public int OperationIndex { get; }
        public Ra2AutomationSectionCreateOperation Operation { get; }
        public Ra2AutomationFieldAuthoringDisposition Disposition { get; set; }
    }

    private sealed class Analysis
    {
        public Analysis(
            Ra2IniTextDocument document,
            Ra2DocumentSemanticModel model,
            IReadOnlyList<Ra2AutomationDiagnosticFact> diagnostics)
        {
            Document = document;
            Model = model;
            Diagnostics = diagnostics;
        }

        public Ra2IniTextDocument Document { get; }
        public Ra2DocumentSemanticModel Model { get; }
        public IReadOnlyList<Ra2AutomationDiagnosticFact> Diagnostics { get; }
    }

    private sealed class PlanningResult
    {
        private PlanningResult(
            Ra2AutomationEditPreviewFailureKind failureKind,
            string? failureMessage,
            IReadOnlyList<IndexedChange> changes,
            IReadOnlyList<Ra2AutomationEditOperationPreview> operationPreviews,
            IReadOnlyList<PlannedSectionCreation> sectionCreations)
        {
            FailureKind = failureKind;
            FailureMessage = failureMessage;
            Changes = changes;
            OperationPreviews = operationPreviews;
            SectionCreations = sectionCreations;
        }

        public bool Succeeded => FailureKind == Ra2AutomationEditPreviewFailureKind.None;
        public Ra2AutomationEditPreviewFailureKind FailureKind { get; }
        public string? FailureMessage { get; }
        public IReadOnlyList<IndexedChange> Changes { get; }
        public IReadOnlyList<Ra2AutomationEditOperationPreview> OperationPreviews { get; }
        public IReadOnlyList<PlannedSectionCreation> SectionCreations { get; }

        public static PlanningResult Success(
            IReadOnlyList<IndexedChange> changes,
            IReadOnlyList<Ra2AutomationEditOperationPreview> operationPreviews,
            IReadOnlyList<PlannedSectionCreation> sectionCreations)
            => new(Ra2AutomationEditPreviewFailureKind.None, null, changes, operationPreviews, sectionCreations);

        public static PlanningResult Failed(
            Ra2AutomationEditPreviewFailureKind failureKind,
            string failureMessage)
            => new(failureKind, failureMessage, [], [], []);
    }

    private sealed class SectionPreviewPlanningResult
    {
        private SectionPreviewPlanningResult(
            Ra2AutomationEditPreviewFailureKind failureKind,
            string? failureMessage,
            IReadOnlyList<Ra2AutomationSectionCreatePreview> previews)
        {
            FailureKind = failureKind;
            FailureMessage = failureMessage;
            Previews = previews;
        }

        public bool Succeeded => FailureKind == Ra2AutomationEditPreviewFailureKind.None;
        public Ra2AutomationEditPreviewFailureKind FailureKind { get; }
        public string? FailureMessage { get; }
        public IReadOnlyList<Ra2AutomationSectionCreatePreview> Previews { get; }

        public static SectionPreviewPlanningResult Success(IReadOnlyList<Ra2AutomationSectionCreatePreview> previews)
            => new(Ra2AutomationEditPreviewFailureKind.None, null, previews);

        public static SectionPreviewPlanningResult Failed(
            Ra2AutomationEditPreviewFailureKind failureKind,
            string failureMessage)
            => new(failureKind, failureMessage, []);
    }
}
