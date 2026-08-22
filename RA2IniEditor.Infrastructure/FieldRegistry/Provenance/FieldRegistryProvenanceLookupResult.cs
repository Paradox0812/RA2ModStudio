using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

internal sealed class FieldRegistryProvenanceLookupResult
{
    private FieldRegistryProvenanceLookupResult(
        bool found,
        FieldRegistryProvenanceScope scope,
        string sourceName,
        string? sourcePath,
        Ra2FieldDefinition? definition)
    {
        Found = found;
        Scope = scope;
        SourceName = sourceName;
        SourcePath = sourcePath;
        Definition = definition;
    }

    public bool Found { get; }

    public FieldRegistryProvenanceScope Scope { get; }

    public string SourceName { get; }

    public string? SourcePath { get; }

    public Ra2FieldDefinition? Definition { get; }

    public static FieldRegistryProvenanceLookupResult NotFound { get; } = new(
        found: false,
        FieldRegistryProvenanceScope.None,
        "None",
        null,
        null);

    public static FieldRegistryProvenanceLookupResult FromEntry(FieldRegistryProvenanceEntry entry)
    {
        return new FieldRegistryProvenanceLookupResult(
            found: true,
            entry.Scope,
            entry.SourceName,
            entry.SourcePath,
            entry.Definition);
    }

    public static FieldRegistryProvenanceLookupResult BuiltIn(Ra2FieldDefinition definition)
    {
        return new FieldRegistryProvenanceLookupResult(
            found: true,
            FieldRegistryProvenanceScope.BuiltIn,
            "BuiltIn",
            null,
            definition);
    }
}
