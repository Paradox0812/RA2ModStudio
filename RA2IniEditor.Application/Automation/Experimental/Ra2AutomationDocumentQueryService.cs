using RA2IniEditor.Application.Diagnostics;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.Application.Language;

namespace RA2IniEditor.Application.Automation.Experimental;

public sealed class Ra2AutomationDocumentQueryService : IRa2AutomationDocumentQueryService
{
    public const int MaximumDocumentCharacters = 8 * 1024 * 1024;

    public const int MaximumResultItems = 10_000;

    private readonly Ra2DocumentDiagnosticService _diagnosticService = new();

    public Ra2AutomationDocumentDiagnosticsResult Validate(
        Ra2AutomationDocumentSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        if (snapshot.Text.Length > MaximumDocumentCharacters)
        {
            return CreateDiagnosticsFailure(
                snapshot,
                Ra2AutomationDocumentDiagnosticsFailureKind.DocumentTooLarge,
                "The document exceeds the supported character limit.");
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            IReadOnlyList<Ra2DiagnosticFact> facts = _diagnosticService.Analyze(
                new Ra2DocumentSnapshot(snapshot.FilePath, snapshot.Text, snapshot.Version),
                snapshot.FieldRegistry.Provider,
                cancellationToken: cancellationToken,
                maximumResultItems: MaximumResultItems);
            cancellationToken.ThrowIfCancellationRequested();

            List<Ra2AutomationDiagnosticFact> diagnostics = new(facts.Count);
            for (int index = 0; index < facts.Count; index++)
            {
                if (index > 0 && index % Ra2DocumentDiagnosticService.CancellationCheckInterval == 0)
                    cancellationToken.ThrowIfCancellationRequested();

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
            return new Ra2AutomationDocumentDiagnosticsResult(
                snapshot,
                Ra2AutomationDocumentDiagnosticsFailureKind.None,
                "The document diagnostics completed.",
                diagnostics);
        }
        catch (Ra2DiagnosticResultLimitExceededException)
        {
            return CreateDiagnosticsFailure(
                snapshot,
                Ra2AutomationDocumentDiagnosticsFailureKind.ResultLimitExceeded,
                "The diagnostics result exceeds the supported item limit.");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CreateDiagnosticsFailure(
                snapshot,
                Ra2AutomationDocumentDiagnosticsFailureKind.Canceled,
                "The document diagnostics were canceled.");
        }
        catch (Exception exception) when (!IsFatalException(exception))
        {
            return CreateDiagnosticsFailure(
                snapshot,
                Ra2AutomationDocumentDiagnosticsFailureKind.AnalysisFailed,
                "The document diagnostics could not be completed.");
        }
    }

    public Ra2AutomationSectionQueryResult GetSection(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationSectionQuery request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(request);

        if (snapshot.Text.Length > MaximumDocumentCharacters)
        {
            return CreateSectionFailure(
                snapshot,
                Ra2AutomationSectionQueryFailureKind.DocumentTooLarge,
                "The document exceeds the supported character limit.");
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2DocumentSemanticModel model = BuildModel(snapshot);
            cancellationToken.ThrowIfCancellationRequested();

            Ra2SectionSymbol[] matchingSections = model.Sections
                .Where(section => string.Equals(section.Name, request.SectionName, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            Ra2SectionSymbol selectedSection;
            int occurrence;
            if (request.Occurrence is int requestedOccurrence)
            {
                if (requestedOccurrence >= matchingSections.Length)
                {
                    return CreateSectionFailure(
                        snapshot,
                        Ra2AutomationSectionQueryFailureKind.NotFound,
                        "The requested section occurrence was not found.");
                }

                selectedSection = matchingSections[requestedOccurrence];
                occurrence = requestedOccurrence;
            }
            else
            {
                if (matchingSections.Length == 0)
                {
                    return CreateSectionFailure(
                        snapshot,
                        Ra2AutomationSectionQueryFailureKind.NotFound,
                        "The requested section was not found.");
                }

                if (matchingSections.Length > 1)
                {
                    return CreateSectionFailure(
                        snapshot,
                        Ra2AutomationSectionQueryFailureKind.AmbiguousSection,
                        "The requested section name has multiple occurrences.");
                }

                selectedSection = matchingSections[0];
                occurrence = 0;
            }

            List<Ra2AutomationFieldFact> fields = [];
            int projectedCount = 0;
            foreach (Ra2KeyValueSymbol keyValue in model.KeyValues)
            {
                if (!ContainsSpan(selectedSection.BodySpan, keyValue.LineSpan))
                    continue;

                if (projectedCount > 0 && projectedCount % 256 == 0)
                    cancellationToken.ThrowIfCancellationRequested();

                projectedCount++;
                if (projectedCount > MaximumResultItems)
                {
                    return CreateSectionFailure(
                        snapshot,
                        Ra2AutomationSectionQueryFailureKind.ResultLimitExceeded,
                        "The section result exceeds the supported item limit.");
                }

                fields.Add(new Ra2AutomationFieldFact(
                    keyValue.Key,
                    keyValue.Value ?? string.Empty,
                    keyValue.LineNumber,
                    ToAutomationSpan(keyValue.LineSpan),
                    ToAutomationSpan(keyValue.KeySpan),
                    keyValue.ValueSpan is Ra2TextSpan valueSpan
                        ? ToAutomationSpan(valueSpan)
                        : null));
            }

            cancellationToken.ThrowIfCancellationRequested();
            Ra2AutomationTextSpan headerSpan = ToAutomationSpan(selectedSection.HeaderSpan);
            Ra2AutomationTextSpan bodySpan = ToAutomationSpan(selectedSection.BodySpan);
            Ra2AutomationSectionFact fact = new(
                selectedSection.Name,
                selectedSection.Kind,
                occurrence,
                selectedSection.HeaderLineNumber,
                headerSpan,
                bodySpan,
                new Ra2AutomationTextSpan(headerSpan.Start, bodySpan.End - headerSpan.Start),
                fields);

            cancellationToken.ThrowIfCancellationRequested();
            return new Ra2AutomationSectionQueryResult(
                snapshot,
                Ra2AutomationSectionQueryFailureKind.None,
                "The section query succeeded.",
                fact);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CreateSectionFailure(
                snapshot,
                Ra2AutomationSectionQueryFailureKind.Canceled,
                "The section query was canceled.");
        }
        catch (Exception exception) when (!IsFatalException(exception))
        {
            return CreateSectionFailure(
                snapshot,
                Ra2AutomationSectionQueryFailureKind.AnalysisFailed,
                "The section query could not be completed.");
        }
    }

    public Ra2AutomationReferenceQueryResult FindReferences(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationReferenceQuery request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(request);

        if (snapshot.Text.Length > MaximumDocumentCharacters)
        {
            return CreateReferenceFailure(
                snapshot,
                Ra2AutomationReferenceQueryFailureKind.DocumentTooLarge,
                "The document exceeds the supported character limit.");
        }

        if (request.SourceOffset > snapshot.Text.Length ||
            request.SelectionSpan is Ra2AutomationTextSpan selection && selection.End > snapshot.Text.Length)
        {
            return CreateReferenceFailure(
                snapshot,
                Ra2AutomationReferenceQueryFailureKind.InvalidLocation,
                "The query location is outside the document.");
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2DocumentSemanticModel model = BuildModel(snapshot);
            cancellationToken.ThrowIfCancellationRequested();

            Ra2CaretContext context = new Ra2CaretContextService().GetContext(model, request.SourceOffset);
            Ra2TextSpan? selectionSpan = request.SelectionSpan is Ra2AutomationTextSpan selected
                ? new Ra2TextSpan(selected.Start, selected.Length)
                : null;
            Ra2ReferenceResult referenceResult = new Ra2ReferenceFinder().FindReferences(
                model,
                context,
                selectionSpan);

            if (string.IsNullOrWhiteSpace(referenceResult.TargetName))
            {
                cancellationToken.ThrowIfCancellationRequested();
                return CreateReferenceFailure(
                    snapshot,
                    Ra2AutomationReferenceQueryFailureKind.TargetNotResolved,
                    "The query location did not resolve a reference target.");
            }

            List<Ra2AutomationReferenceFact> references = [];
            for (int index = 0; index < referenceResult.Items.Count; index++)
            {
                if (index > 0 && index % 256 == 0)
                    cancellationToken.ThrowIfCancellationRequested();

                if (references.Count >= MaximumResultItems)
                {
                    return CreateReferenceFailure(
                        snapshot,
                        Ra2AutomationReferenceQueryFailureKind.ResultLimitExceeded,
                        "The reference result exceeds the supported item limit.");
                }

                Ra2ReferenceItem item = referenceResult.Items[index];
                references.Add(new Ra2AutomationReferenceFact(
                    item.SourceSectionName,
                    item.SourceKey,
                    item.LineNumber,
                    ToAutomationSpan(item.LineSpan),
                    ToAutomationSpan(item.ValueSpan)));
            }

            cancellationToken.ThrowIfCancellationRequested();
            Ra2AutomationReferenceTargetFact target = new(
                referenceResult.TargetName,
                referenceResult.TargetKind);
            cancellationToken.ThrowIfCancellationRequested();
            return new Ra2AutomationReferenceQueryResult(
                snapshot,
                Ra2AutomationReferenceQueryFailureKind.None,
                "The reference query succeeded.",
                target,
                references);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CreateReferenceFailure(
                snapshot,
                Ra2AutomationReferenceQueryFailureKind.Canceled,
                "The reference query was canceled.");
        }
        catch (Exception exception) when (!IsFatalException(exception))
        {
            return CreateReferenceFailure(
                snapshot,
                Ra2AutomationReferenceQueryFailureKind.AnalysisFailed,
                "The reference query could not be completed.");
        }
    }

    private static Ra2DocumentSemanticModel BuildModel(Ra2AutomationDocumentSnapshot snapshot)
        => new Ra2DocumentSemanticModelBuilder().Build(
            new Ra2DocumentSnapshot(snapshot.FilePath, snapshot.Text, snapshot.Version),
            snapshot.FieldRegistry.Provider);

    private static Ra2AutomationSectionQueryResult CreateSectionFailure(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationSectionQueryFailureKind failureKind,
        string message)
        => new(snapshot, failureKind, message, null);

    private static Ra2AutomationReferenceQueryResult CreateReferenceFailure(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationReferenceQueryFailureKind failureKind,
        string message)
        => new(snapshot, failureKind, message, null, Array.Empty<Ra2AutomationReferenceFact>());

    private static Ra2AutomationDocumentDiagnosticsResult CreateDiagnosticsFailure(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationDocumentDiagnosticsFailureKind failureKind,
        string message)
        => new(snapshot, failureKind, message, Array.Empty<Ra2AutomationDiagnosticFact>());

    private static Ra2AutomationTextSpan ToAutomationSpan(Ra2TextSpan span)
        => new(span.Start, span.Length);

    private static bool ContainsSpan(Ra2TextSpan container, Ra2TextSpan candidate)
        => candidate.Start >= container.Start && candidate.End <= container.End;

    private static bool IsFatalException(Exception exception)
        => exception is OutOfMemoryException or
            AccessViolationException or
            AppDomainUnloadedException or
            BadImageFormatException;
}
