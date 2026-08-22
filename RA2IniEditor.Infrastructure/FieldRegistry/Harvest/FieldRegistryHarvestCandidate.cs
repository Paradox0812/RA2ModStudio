namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal sealed class FieldRegistryHarvestCandidate
{
    public FieldRegistryHarvestCandidate(
        string key,
        string? appliesToRaw,
        string? editorKindRaw,
        string? description,
        string sourceName,
        int lineNumber,
        string rawLine,
        FieldRegistryHarvestConfidence confidence)
    {
        Key = key?.Trim() ?? string.Empty;
        AppliesToRaw = string.IsNullOrWhiteSpace(appliesToRaw) ? null : appliesToRaw.Trim();
        EditorKindRaw = string.IsNullOrWhiteSpace(editorKindRaw) ? null : editorKindRaw.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SourceName = sourceName;
        LineNumber = lineNumber;
        RawLine = rawLine;
        Confidence = confidence;
    }

    public string Key { get; }

    public string? AppliesToRaw { get; }

    public string? EditorKindRaw { get; }

    public string? Description { get; }

    public string SourceName { get; }

    public int LineNumber { get; }

    public string RawLine { get; }

    public FieldRegistryHarvestConfidence Confidence { get; }
}
