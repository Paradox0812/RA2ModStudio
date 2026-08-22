using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal sealed class FieldRegistryHarvestNormalizedCandidate
{
    public FieldRegistryHarvestNormalizedCandidate(
        string key,
        IReadOnlyList<Ra2SectionKind> appliesTo,
        FieldEditorKind editorKind,
        Ra2FieldSourceKind sourceKind,
        string? description,
        string sourceName,
        int lineNumber,
        string rawLine,
        FieldRegistryHarvestConfidence confidence,
        bool usedDefaultAppliesTo,
        bool usedDefaultEditorKind)
    {
        Key = string.IsNullOrWhiteSpace(key)
            ? throw new ArgumentException("Normalized field key cannot be empty.", nameof(key))
            : key.Trim();
        AppliesTo = appliesTo ?? throw new ArgumentNullException(nameof(appliesTo));
        EditorKind = editorKind;
        SourceKind = sourceKind;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SourceName = sourceName;
        LineNumber = lineNumber;
        RawLine = rawLine;
        Confidence = confidence;
        UsedDefaultAppliesTo = usedDefaultAppliesTo;
        UsedDefaultEditorKind = usedDefaultEditorKind;
    }

    public string Key { get; }

    public IReadOnlyList<Ra2SectionKind> AppliesTo { get; }

    public FieldEditorKind EditorKind { get; }

    public Ra2FieldSourceKind SourceKind { get; }

    public string? Description { get; }

    public string SourceName { get; }

    public int LineNumber { get; }

    public string RawLine { get; }

    public FieldRegistryHarvestConfidence Confidence { get; }

    public bool UsedDefaultAppliesTo { get; }

    public bool UsedDefaultEditorKind { get; }
}
