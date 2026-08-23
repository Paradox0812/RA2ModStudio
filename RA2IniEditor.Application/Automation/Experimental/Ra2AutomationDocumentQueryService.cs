using RA2IniEditor.Application.Diagnostics;
using RA2IniEditor.Application.FieldTrust;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.Application.Language;

namespace RA2IniEditor.Application.Automation.Experimental;

public sealed class Ra2AutomationDocumentQueryService : IRa2AutomationDocumentQueryService
{
    public const int MaximumDocumentCharacters = 8 * 1024 * 1024;

    public const int MaximumResultItems = 10_000;
    public const int MaximumFieldSchemaAllowedValues = 1_024;
    public const int MaximumFieldSchemaAliases = 256;
    public const int MaximumFieldSchemaTextCharacters = 64 * 1024;
    public const int MaximumReferenceTokenLength = 256;
    public const int MaximumReferenceListTokens = 10_000;

    private readonly Ra2DocumentDiagnosticService _diagnosticService = new();

    public Ra2AutomationFieldSchemaQueryResult GetFieldSchema(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationFieldSchemaQuery request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(request);

        if (snapshot.Text.Length > MaximumDocumentCharacters)
        {
            return CreateFieldSchemaFailure(
                snapshot,
                Ra2AutomationFieldSchemaQueryFailureKind.DocumentTooLarge,
                "The document exceeds the supported character limit.");
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!snapshot.FieldRegistry.Provider.TryGetField(
                    request.SectionKind,
                    request.Key,
                    out Ra2FieldDefinition? definition))
            {
                return CreateFieldSchemaFailure(
                    snapshot,
                    Ra2AutomationFieldSchemaQueryFailureKind.NotFound,
                    "The requested field schema was not found.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            if (definition.ValueMetadata.AllowedValues.Count > MaximumFieldSchemaAllowedValues ||
                definition.Aliases.Count > MaximumFieldSchemaAliases)
            {
                return CreateFieldSchemaFailure(
                    snapshot,
                    Ra2AutomationFieldSchemaQueryFailureKind.ResultLimitExceeded,
                    "The field schema exceeds the supported item limit.");
            }

            string[] allowedValues = definition.ValueMetadata.AllowedValues
                .Select(item => item.Value)
                .ToArray();
            string[] aliases = definition.Aliases.ToArray();
            if (allowedValues.Any(value => value.Length > Ra2AutomationEditOperation.MaximumValueLength) ||
                aliases.Any(alias => alias.Length > Ra2AutomationFieldSchemaQuery.MaximumKeyLength) ||
                CalculateSchemaTextLength(definition, allowedValues, aliases) > MaximumFieldSchemaTextCharacters)
            {
                return CreateFieldSchemaFailure(
                    snapshot,
                    Ra2AutomationFieldSchemaQueryFailureKind.ResultLimitExceeded,
                    "The field schema exceeds the supported text limit.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            Ra2AutomationFieldTrustLevel trustLevel = Ra2AutomationFieldTrustMapper.ToAutomationLevel(
                Ra2FieldTrustClassifier.Classify(definition).Level);
            Ra2AutomationFieldSchemaFact fact = new(
                definition.Key,
                request.SectionKind,
                definition.AppliesTo,
                definition.EditorKind,
                definition.ValueMetadata.ValueKind,
                definition.ValueMetadata.BooleanStyle,
                allowedValues,
                definition.ValueMetadata.EnumName,
                definition.ValueMetadata.Separator,
                definition.DisplayName,
                definition.Description,
                aliases,
                definition.SourceKind,
                trustLevel,
                Ra2AutomationFieldTrustMapper.ToAuthoringDisposition(trustLevel));
            cancellationToken.ThrowIfCancellationRequested();
            return new Ra2AutomationFieldSchemaQueryResult(
                snapshot,
                Ra2AutomationFieldSchemaQueryFailureKind.None,
                "The field schema query succeeded.",
                fact);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CreateFieldSchemaFailure(
                snapshot,
                Ra2AutomationFieldSchemaQueryFailureKind.Canceled,
                "The field schema query was canceled.");
        }
        catch (Exception exception) when (!IsFatalException(exception))
        {
            return CreateFieldSchemaFailure(
                snapshot,
                Ra2AutomationFieldSchemaQueryFailureKind.AnalysisFailed,
                "The field schema query could not be completed.");
        }
    }

    public Ra2AutomationReferenceResolveResult ResolveReference(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationReferenceResolveQuery request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(request);
        if (snapshot.Text.Length > MaximumDocumentCharacters)
        {
            return CreateReferenceResolveFailure(
                snapshot,
                Ra2AutomationReferenceResolveFailureKind.DocumentTooLarge,
                "The document exceeds the supported character limit.");
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            Ra2DocumentSemanticModel model = BuildModel(snapshot);
            Ra2SectionSymbol[] matchingSections = model.Sections
                .Where(section => string.Equals(section.Name, request.SectionName, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (!TrySelectOccurrence(
                    matchingSections,
                    request.SectionOccurrence,
                    out Ra2SectionSymbol? section,
                    out int sectionOccurrence))
            {
                return CreateReferenceResolveFailure(
                    snapshot,
                    matchingSections.Length == 0 || request.SectionOccurrence is not null
                        ? Ra2AutomationReferenceResolveFailureKind.SectionNotFound
                        : Ra2AutomationReferenceResolveFailureKind.AmbiguousSection,
                    matchingSections.Length == 0 || request.SectionOccurrence is not null
                        ? "The requested section occurrence was not found."
                        : "The requested section name has multiple occurrences.");
            }

            Ra2KeyValueSymbol[] matchingFields = model.KeyValues
                .Where(field =>
                    ContainsSpan(section!.BodySpan, field.LineSpan) &&
                    string.Equals(field.Key, request.Key, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (!TrySelectOccurrence(
                    matchingFields,
                    request.FieldOccurrence,
                    out Ra2KeyValueSymbol? field,
                    out int fieldOccurrence))
            {
                return CreateReferenceResolveFailure(
                    snapshot,
                    matchingFields.Length == 0 || request.FieldOccurrence is not null
                        ? Ra2AutomationReferenceResolveFailureKind.FieldNotFound
                        : Ra2AutomationReferenceResolveFailureKind.AmbiguousField,
                    matchingFields.Length == 0 || request.FieldOccurrence is not null
                        ? "The requested field occurrence was not found."
                        : "The requested field has multiple occurrences.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            bool schemaDeclared = snapshot.FieldRegistry.Provider.TryGetField(
                    section!.Kind,
                    field!.Key,
                    out Ra2FieldDefinition? definition) &&
                definition.ValueMetadata.ValueKind is Ra2FieldValueKind.Reference or Ra2FieldValueKind.ReferenceList;

            Ra2ValueReferenceSymbol? semanticReference = model.References.FirstOrDefault(reference =>
                reference.LineNumber == field.LineNumber &&
                string.Equals(reference.SourceSectionName, section.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(reference.SourceKey, field.Key, StringComparison.OrdinalIgnoreCase));

            string token;
            Ra2TextSpan tokenSpan;
            Ra2SectionKind declaredTargetKind;
            Ra2AutomationReferenceResolutionBasis basis;
            if (semanticReference is not null)
            {
                if (request.ReferenceIndex != 0)
                {
                    return CreateReferenceResolveFailure(
                        snapshot,
                        Ra2AutomationReferenceResolveFailureKind.ReferenceIndexOutOfRange,
                        "The semantic reference has no token at the requested index.");
                }

                token = semanticReference.TargetSectionName;
                tokenSpan = semanticReference.ValueSpan;
                declaredTargetKind = semanticReference.TargetSectionKind;
                basis = Ra2AutomationReferenceResolutionBasis.SemanticKnown;
            }
            else
            {
                if (!schemaDeclared || definition is null)
                {
                    return CreateReferenceResolveFailure(
                        snapshot,
                        Ra2AutomationReferenceResolveFailureKind.UnsupportedReference,
                        "The field is not a known or schema-declared reference.");
                }

                ReferenceTokenization tokenization = TokenizeReferenceValue(
                    snapshot.Text,
                    field,
                    definition.ValueMetadata.Separator,
                    cancellationToken);
                if (tokenization.ExceedsLimit)
                {
                    return CreateReferenceResolveFailure(
                        snapshot,
                        Ra2AutomationReferenceResolveFailureKind.ResultLimitExceeded,
                        "The reference list exceeds the supported token limit.");
                }
                if (request.ReferenceIndex >= tokenization.Tokens.Count)
                {
                    return CreateReferenceResolveFailure(
                        snapshot,
                        Ra2AutomationReferenceResolveFailureKind.ReferenceIndexOutOfRange,
                        "The reference list has no token at the requested index.");
                }

                ReferenceToken selected = tokenization.Tokens[request.ReferenceIndex];
                if (string.IsNullOrWhiteSpace(selected.Value))
                {
                    return CreateReferenceResolveFailure(
                        snapshot,
                        Ra2AutomationReferenceResolveFailureKind.EmptyReference,
                        "The requested reference token is empty.");
                }

                token = selected.Value;
                tokenSpan = selected.Span;
                declaredTargetKind = Ra2SectionKind.Unknown;
                basis = Ra2AutomationReferenceResolutionBasis.FieldSchemaDeclared;
            }

            if (token.Length > MaximumReferenceTokenLength)
            {
                return CreateReferenceResolveFailure(
                    snapshot,
                    Ra2AutomationReferenceResolveFailureKind.ResultLimitExceeded,
                    "The reference token exceeds the supported length.");
            }

            cancellationToken.ThrowIfCancellationRequested();
            Ra2SectionSymbol[] targets = model.Sections
                .Where(candidate => string.Equals(candidate.Name, token, StringComparison.OrdinalIgnoreCase))
                .ToArray();
            if (targets.Length > MaximumResultItems)
            {
                return CreateReferenceResolveFailure(
                    snapshot,
                    Ra2AutomationReferenceResolveFailureKind.ResultLimitExceeded,
                    "The reference target count exceeds the supported limit.");
            }

            Ra2SectionKind targetKind = targets.Length == 1 ? targets[0].Kind : declaredTargetKind;
            Ra2AutomationReferenceResolutionFact fact = new(
                section.Name,
                sectionOccurrence,
                field.Key,
                fieldOccurrence,
                field.LineNumber,
                ToAutomationSpan(tokenSpan),
                token,
                request.ReferenceIndex,
                token,
                targetKind,
                basis,
                targets.Length > 0,
                targets.Length,
                schemaDeclared);
            cancellationToken.ThrowIfCancellationRequested();
            return new Ra2AutomationReferenceResolveResult(
                snapshot,
                Ra2AutomationReferenceResolveFailureKind.None,
                "The reference resolution succeeded.",
                fact);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CreateReferenceResolveFailure(
                snapshot,
                Ra2AutomationReferenceResolveFailureKind.Canceled,
                "The reference resolution was canceled.");
        }
        catch (Exception exception) when (!IsFatalException(exception))
        {
            return CreateReferenceResolveFailure(
                snapshot,
                Ra2AutomationReferenceResolveFailureKind.AnalysisFailed,
                "The reference resolution could not be completed.");
        }
    }

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

    private static Ra2AutomationFieldSchemaQueryResult CreateFieldSchemaFailure(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationFieldSchemaQueryFailureKind failureKind,
        string message)
        => new(snapshot, failureKind, message, null);

    private static Ra2AutomationReferenceResolveResult CreateReferenceResolveFailure(
        Ra2AutomationDocumentSnapshot snapshot,
        Ra2AutomationReferenceResolveFailureKind failureKind,
        string message)
        => new(snapshot, failureKind, message, null);

    private static bool TrySelectOccurrence<T>(
        IReadOnlyList<T> values,
        int? requestedOccurrence,
        out T? value,
        out int occurrence)
        where T : class
    {
        value = null;
        occurrence = -1;
        if (requestedOccurrence is int requested)
        {
            if (requested >= values.Count)
                return false;
            value = values[requested];
            occurrence = requested;
            return true;
        }

        if (values.Count != 1)
            return false;
        value = values[0];
        occurrence = 0;
        return true;
    }

    private static ReferenceTokenization TokenizeReferenceValue(
        string sourceText,
        Ra2KeyValueSymbol field,
        string separator,
        CancellationToken cancellationToken)
    {
        if (field.ValueSpan is not Ra2TextSpan valueSpan)
            return new ReferenceTokenization([new ReferenceToken(string.Empty, field.LineSpan)], false);

        string raw = sourceText.Substring(valueSpan.Start, valueSpan.Length);
        string effective = Ra2IniLineParser.GetEffectiveValue(raw);
        int effectiveOffset = raw.IndexOf(effective, StringComparison.Ordinal);
        if (effectiveOffset < 0)
            effectiveOffset = 0;

        string delimiter = string.IsNullOrEmpty(separator) ? "," : separator;
        List<ReferenceToken> tokens = [];
        int segmentStart = 0;
        while (true)
        {
            if (tokens.Count >= MaximumReferenceListTokens)
                return new ReferenceTokenization([], true);
            cancellationToken.ThrowIfCancellationRequested();

            int delimiterIndex = effective.IndexOf(delimiter, segmentStart, StringComparison.Ordinal);
            int segmentEnd = delimiterIndex < 0 ? effective.Length : delimiterIndex;
            string segment = effective[segmentStart..segmentEnd];
            int leading = 0;
            while (leading < segment.Length && char.IsWhiteSpace(segment[leading]))
                leading++;
            int trailing = segment.Length;
            while (trailing > leading && char.IsWhiteSpace(segment[trailing - 1]))
                trailing--;
            string token = segment[leading..trailing];
            tokens.Add(new ReferenceToken(
                token,
                new Ra2TextSpan(
                    valueSpan.Start + effectiveOffset + segmentStart + leading,
                    trailing - leading)));

            if (delimiterIndex < 0)
                break;
            segmentStart = delimiterIndex + delimiter.Length;
        }

        return new ReferenceTokenization(tokens, false);
    }

    private static long CalculateSchemaTextLength(
        Ra2FieldDefinition definition,
        IReadOnlyCollection<string> allowedValues,
        IReadOnlyCollection<string> aliases)
    {
        long length = definition.Key.Length +
            (definition.ValueMetadata.EnumName?.Length ?? 0) +
            definition.ValueMetadata.Separator.Length +
            (definition.DisplayName?.Length ?? 0) +
            (definition.Description?.Length ?? 0);
        foreach (string value in allowedValues)
            length += value.Length;
        foreach (string alias in aliases)
            length += alias.Length;
        return length;
    }

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

    private readonly record struct ReferenceToken(string Value, Ra2TextSpan Span);

    private readonly record struct ReferenceTokenization(
        IReadOnlyList<ReferenceToken> Tokens,
        bool ExceedsLimit);
}
