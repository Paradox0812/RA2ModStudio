using RA2IniEditor.IDE.Language;
using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2CurrentDocumentAiContextProvider : IRa2AiContextProvider
{
    private const int DefaultMaxNearbyTextCharacters = 4000;

    private readonly IRa2CaretContextService _caretContextService;
    private readonly IRa2AiFieldEvidenceProvider _fieldEvidenceProvider;
    private readonly IRa2AiDiagnosticSummaryProvider _diagnosticSummaryProvider;
    private readonly int _maxNearbyTextCharacters;

    public Ra2CurrentDocumentAiContextProvider()
        : this(
            new Ra2CaretContextService(),
            new Ra2FieldRegistryAiEvidenceProvider(),
            new Ra2CurrentFileAiDiagnosticSummaryProvider(),
            DefaultMaxNearbyTextCharacters)
    {
    }

    internal Ra2CurrentDocumentAiContextProvider(
        IRa2CaretContextService caretContextService,
        IRa2AiFieldEvidenceProvider fieldEvidenceProvider,
        IRa2AiDiagnosticSummaryProvider diagnosticSummaryProvider,
        int maxNearbyTextCharacters)
    {
        _caretContextService = caretContextService ?? throw new ArgumentNullException(nameof(caretContextService));
        _fieldEvidenceProvider = fieldEvidenceProvider ?? throw new ArgumentNullException(nameof(fieldEvidenceProvider));
        _diagnosticSummaryProvider = diagnosticSummaryProvider ?? throw new ArgumentNullException(nameof(diagnosticSummaryProvider));
        _maxNearbyTextCharacters = Math.Max(256, maxNearbyTextCharacters);
    }

    public Ra2AiContext BuildContext(Ra2AiContextRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.SemanticModel is not Ra2DocumentSemanticModel model)
        {
            IReadOnlyList<Ra2AiFieldEvidence> fallbackEvidence = RetrieveFieldEvidence(
                request,
                Ra2SectionKind.Unknown,
                keyName: null);
            IReadOnlyList<Ra2AiDiagnosticSummary> fallbackDiagnostics = RetrieveDiagnostics(
                request,
                lineNumber: 0,
                sectionName: null,
                keyName: null);
            return new Ra2AiContext(
                request.DocumentDisplayName,
                Math.Max(0, request.CaretOffset),
                lineNumber: 0,
                Ra2CaretRegion.Unknown,
                sectionName: null,
                sectionKind: null,
                keyName: null,
                valueText: null,
                request.SelectedText,
                nearbyText: string.Empty,
                nearbyLineCount: 0,
                hasSemanticContext: false,
                fallbackEvidence,
                fallbackDiagnostics);
        }

        int normalizedOffset = Math.Clamp(request.CaretOffset, 0, model.Snapshot.Text.Length);
        Ra2CaretContext caretContext = _caretContextService.GetContext(model, normalizedOffset);
        int lineNumber = GetLineNumber(model.Snapshot.Text, normalizedOffset);
        NearbyTextResult nearbyText = request.IncludeNearbyText
            ? BuildNearbyText(model.Snapshot.Text, lineNumber, request.NearbyLineRadius)
            : new NearbyTextResult(string.Empty, 0);

        IReadOnlyList<Ra2AiFieldEvidence> fieldEvidence = RetrieveFieldEvidence(
            request,
            caretContext.Section?.Kind ?? Ra2SectionKind.Unknown,
            caretContext.KeyValue?.Key);
        IReadOnlyList<Ra2AiDiagnosticSummary> diagnostics = RetrieveDiagnostics(
            request,
            lineNumber,
            caretContext.Section?.Name,
            caretContext.KeyValue?.Key);

        return new Ra2AiContext(
            request.DocumentDisplayName,
            normalizedOffset,
            lineNumber,
            caretContext.Region,
            caretContext.Section?.Name,
            caretContext.Section?.Kind.ToString(),
            caretContext.KeyValue?.Key,
            caretContext.KeyValue?.Value,
            request.SelectedText,
            nearbyText.Text,
            nearbyText.LineCount,
            hasSemanticContext: true,
            fieldEvidence,
            diagnostics);
    }

    private IReadOnlyList<Ra2AiFieldEvidence> RetrieveFieldEvidence(
        Ra2AiContextRequest request,
        Ra2SectionKind sectionKind,
        string? keyName)
        => _fieldEvidenceProvider.Retrieve(
            request.FieldDefinitionProvider,
            request.FieldProvenanceProvider,
            sectionKind,
            keyName,
            request.SelectedText,
            request.PromptText,
            request.MaxFieldEvidenceCount,
            request.ConversationContext,
            request.CurrentSubject);

    private IReadOnlyList<Ra2AiDiagnosticSummary> RetrieveDiagnostics(
        Ra2AiContextRequest request,
        int lineNumber,
        string? sectionName,
        string? keyName)
        => _diagnosticSummaryProvider.Summarize(
            request.DiagnosticIssues,
            request.DocumentFilePath,
            request.DocumentDisplayName,
            request.DocumentVersion,
            lineNumber,
            sectionName,
            keyName,
            request.MaxDiagnosticCount);

    private NearbyTextResult BuildNearbyText(string text, int lineNumber, int radius)
    {
        IReadOnlyList<LineRange> lines = GetLineRanges(text);
        if (lines.Count == 0 || lineNumber <= 0)
            return new NearbyTextResult(string.Empty, 0);

        int caretLineIndex = Math.Clamp(lineNumber - 1, 0, lines.Count - 1);
        int startIndex = Math.Max(0, caretLineIndex - radius);
        int endIndex = Math.Min(lines.Count - 1, caretLineIndex + radius);
        string nearby = text.Substring(lines[startIndex].Start, lines[endIndex].End - lines[startIndex].Start);
        if (nearby.Length > _maxNearbyTextCharacters)
            nearby = nearby[.._maxNearbyTextCharacters];

        return new NearbyTextResult(nearby, endIndex - startIndex + 1);
    }

    private static IReadOnlyList<LineRange> GetLineRanges(string text)
    {
        if (text.Length == 0)
            return [];

        List<LineRange> lines = [];
        int lineStart = 0;
        while (lineStart < text.Length)
        {
            int lineEnd = lineStart;
            while (lineEnd < text.Length && text[lineEnd] is not ('\r' or '\n'))
                lineEnd++;

            int rangeEnd = lineEnd;
            if (rangeEnd < text.Length && text[rangeEnd] == '\r')
                rangeEnd++;
            if (rangeEnd < text.Length && text[rangeEnd] == '\n')
                rangeEnd++;

            lines.Add(new LineRange(lineStart, rangeEnd));
            lineStart = rangeEnd;
        }

        return lines;
    }

    private static int GetLineNumber(string text, int offset)
    {
        if (text.Length == 0)
            return 0;

        int normalizedOffset = Math.Clamp(offset, 0, text.Length);
        int lineNumber = 1;
        for (int index = 0; index < normalizedOffset; index++)
        {
            if (text[index] == '\n')
                lineNumber++;
        }

        return lineNumber;
    }

    private readonly record struct LineRange(int Start, int End);

    private sealed record NearbyTextResult(string Text, int LineCount);
}
