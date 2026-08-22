using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

internal sealed class FieldRegistryProvenanceSnapshotBuilder
{
    public FieldRegistryProvenanceSnapshot Build(
        LocalFieldRegistryLoadResult globalResult,
        LocalFieldRegistryLoadResult? projectResult,
        IRa2FieldDefinitionProvider builtInProvider)
    {
        ArgumentNullException.ThrowIfNull(globalResult);
        ArgumentNullException.ThrowIfNull(builtInProvider);

        return new FieldRegistryProvenanceSnapshot(
            BuildEntries(projectResult?.LoadedDefinitions ?? [], FieldRegistryProvenanceScope.Project),
            BuildEntries(globalResult.LoadedDefinitions, FieldRegistryProvenanceScope.Global),
            builtInProvider);
    }

    private static IReadOnlyList<FieldRegistryProvenanceEntry> BuildEntries(
        IReadOnlyList<LocalFieldRegistryLoadedDefinition> loadedDefinitions,
        FieldRegistryProvenanceScope scope)
    {
        List<FieldRegistryProvenanceEntry> entries = new();
        foreach (LocalFieldRegistryLoadedDefinition loaded in loadedDefinitions)
        {
            IReadOnlyCollection<Ra2SectionKind> appliesTo = loaded.Definition.AppliesTo.Count == 0
                ? [Ra2SectionKind.Unknown]
                : loaded.Definition.AppliesTo;

            foreach (Ra2SectionKind kind in appliesTo)
            {
                entries.Add(new FieldRegistryProvenanceEntry(
                    loaded.Definition.Key,
                    kind,
                    scope,
                    loaded.SourceFileName,
                    loaded.SourceFilePath,
                    loaded.Definition));
            }
        }

        return Array.AsReadOnly(entries.ToArray());
    }
}
