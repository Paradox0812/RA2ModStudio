using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

internal sealed class FieldRegistryProvenanceEntry
{
    public FieldRegistryProvenanceEntry(
        string key,
        Ra2SectionKind appliesTo,
        FieldRegistryProvenanceScope scope,
        string sourceName,
        string? sourcePath,
        Ra2FieldDefinition definition)
    {
        Key = string.IsNullOrWhiteSpace(key)
            ? throw new ArgumentException("Provenance key cannot be empty.", nameof(key))
            : key.Trim();
        AppliesTo = appliesTo;
        Scope = scope;
        SourceName = string.IsNullOrWhiteSpace(sourceName)
            ? throw new ArgumentException("Source name cannot be empty.", nameof(sourceName))
            : sourceName;
        SourcePath = string.IsNullOrWhiteSpace(sourcePath) ? null : sourcePath;
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
    }

    public string Key { get; }

    public Ra2SectionKind AppliesTo { get; }

    public FieldRegistryProvenanceScope Scope { get; }

    public string SourceName { get; }

    public string? SourcePath { get; }

    public Ra2FieldDefinition Definition { get; }
}
