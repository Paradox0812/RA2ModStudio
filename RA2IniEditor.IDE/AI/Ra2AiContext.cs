using RA2IniEditor.IDE.Language;

namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2AiContext
{
    public Ra2AiContext(
        string? documentDisplayName,
        int caretOffset,
        int lineNumber,
        Ra2CaretRegion caretRegion,
        string? sectionName,
        string? sectionKind,
        string? keyName,
        string? valueText,
        string? selectedText,
        string nearbyText,
        int nearbyLineCount,
        bool hasSemanticContext,
        IReadOnlyList<Ra2AiFieldEvidence>? fieldEvidence = null,
        IReadOnlyList<Ra2AiDiagnosticSummary>? diagnostics = null)
    {
        DocumentDisplayName = documentDisplayName;
        CaretOffset = Math.Max(0, caretOffset);
        LineNumber = Math.Max(0, lineNumber);
        CaretRegion = caretRegion;
        SectionName = sectionName;
        SectionKind = sectionKind;
        KeyName = keyName;
        ValueText = valueText;
        SelectedText = selectedText;
        NearbyText = nearbyText ?? string.Empty;
        NearbyLineCount = Math.Max(0, nearbyLineCount);
        HasSemanticContext = hasSemanticContext;
        FieldEvidence = Array.AsReadOnly((fieldEvidence ?? []).ToArray());
        Diagnostics = Array.AsReadOnly((diagnostics ?? []).ToArray());
    }

    public string? DocumentDisplayName { get; }

    public int CaretOffset { get; }

    public int LineNumber { get; }

    public Ra2CaretRegion CaretRegion { get; }

    public string? SectionName { get; }

    public string? SectionKind { get; }

    public string? KeyName { get; }

    public string? ValueText { get; }

    public string? SelectedText { get; }

    public string NearbyText { get; }

    public int NearbyLineCount { get; }

    public bool HasExplicitSelection => !string.IsNullOrWhiteSpace(SelectedText);

    public bool HasSemanticContext { get; }

    public bool HasDocumentContext => HasSemanticContext && !string.IsNullOrWhiteSpace(DocumentDisplayName);

    public IReadOnlyList<Ra2AiFieldEvidence> FieldEvidence { get; }

    public int FieldEvidenceCount => FieldEvidence.Count;

    public string FieldEvidenceTopKeysText
        => string.Join(", ", FieldEvidence
            .Select(evidence => evidence.Key)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Take(5));

    public IReadOnlyList<Ra2AiDiagnosticSummary> Diagnostics { get; }

    public int DiagnosticCount => Diagnostics.Count;
}
