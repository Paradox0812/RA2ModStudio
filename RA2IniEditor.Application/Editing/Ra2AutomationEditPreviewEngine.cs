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
        HashSet<string> logicalTargets = new(StringComparer.OrdinalIgnoreCase);
        List<IndexedChange> changes = [];
        List<Ra2AutomationEditOperationPreview> operationPreviews = [];

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
            Ra2AutomationFieldTrustLevel trustLevel = ToAutomationTrustLevel(
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

        return PlanningResult.Success(changes, operationPreviews);
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

    private static Ra2AutomationFieldTrustLevel ToAutomationTrustLevel(Ra2FieldTrustLevel level)
        => level switch
        {
            Ra2FieldTrustLevel.Verified => Ra2AutomationFieldTrustLevel.Verified,
            Ra2FieldTrustLevel.VerifiedGuardrail => Ra2AutomationFieldTrustLevel.VerifiedGuardrail,
            Ra2FieldTrustLevel.Inferred => Ra2AutomationFieldTrustLevel.Inferred,
            Ra2FieldTrustLevel.ManualCurated => Ra2AutomationFieldTrustLevel.ManualCurated,
            Ra2FieldTrustLevel.AutoExtracted => Ra2AutomationFieldTrustLevel.AutoExtracted,
            Ra2FieldTrustLevel.Obsolete => Ra2AutomationFieldTrustLevel.Obsolete,
            Ra2FieldTrustLevel.NonExistent => Ra2AutomationFieldTrustLevel.NonExistent,
            Ra2FieldTrustLevel.PseudoField => Ra2AutomationFieldTrustLevel.PseudoField,
            _ => Ra2AutomationFieldTrustLevel.Unknown
        };

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
        => new(snapshot, plan, failureKind, message, Guid.Empty, null, [], [], [], []);

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
            IReadOnlyList<Ra2AutomationEditOperationPreview> operationPreviews)
        {
            FailureKind = failureKind;
            FailureMessage = failureMessage;
            Changes = changes;
            OperationPreviews = operationPreviews;
        }

        public bool Succeeded => FailureKind == Ra2AutomationEditPreviewFailureKind.None;
        public Ra2AutomationEditPreviewFailureKind FailureKind { get; }
        public string? FailureMessage { get; }
        public IReadOnlyList<IndexedChange> Changes { get; }
        public IReadOnlyList<Ra2AutomationEditOperationPreview> OperationPreviews { get; }

        public static PlanningResult Success(
            IReadOnlyList<IndexedChange> changes,
            IReadOnlyList<Ra2AutomationEditOperationPreview> operationPreviews)
            => new(Ra2AutomationEditPreviewFailureKind.None, null, changes, operationPreviews);

        public static PlanningResult Failed(
            Ra2AutomationEditPreviewFailureKind failureKind,
            string failureMessage)
            => new(failureKind, failureMessage, [], []);
    }
}
