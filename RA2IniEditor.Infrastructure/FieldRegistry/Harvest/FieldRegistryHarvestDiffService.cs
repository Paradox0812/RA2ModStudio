using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal sealed class FieldRegistryHarvestDiffService : IFieldRegistryHarvestDiffService
{
    public FieldRegistryHarvestDiffResult Compare(
        FieldRegistryHarvestPreviewDraft previewDraft,
        IRa2FieldDefinitionProvider effectiveProvider)
    {
        ArgumentNullException.ThrowIfNull(previewDraft);
        ArgumentNullException.ThrowIfNull(effectiveProvider);

        List<FieldRegistryHarvestDiffRow> rows = new();
        foreach (Ra2FieldDefinition definition in previewDraft.Definitions)
        {
            if (string.IsNullOrWhiteSpace(definition.Key) || definition.AppliesTo.Count == 0)
            {
                rows.Add(CreateInvalidRow(definition, "Preview definition cannot be compared."));
                continue;
            }

            foreach (Ra2SectionKind appliesTo in definition.AppliesTo)
                rows.Add(CompareDefinition(definition, appliesTo, effectiveProvider));
        }

        return new FieldRegistryHarvestDiffResult(Array.AsReadOnly(rows.ToArray()));
    }

    public FieldRegistryHarvestDiffResult Compare(
        FieldRegistryHarvestPreviewDraft previewDraft,
        IFieldRegistryProvenanceProvider provenanceProvider)
    {
        ArgumentNullException.ThrowIfNull(previewDraft);
        ArgumentNullException.ThrowIfNull(provenanceProvider);

        List<FieldRegistryHarvestDiffRow> rows = new();
        foreach (Ra2FieldDefinition definition in previewDraft.Definitions)
        {
            if (string.IsNullOrWhiteSpace(definition.Key) || definition.AppliesTo.Count == 0)
            {
                rows.Add(CreateInvalidRow(definition, "Preview definition cannot be compared."));
                continue;
            }

            foreach (Ra2SectionKind appliesTo in definition.AppliesTo)
                rows.Add(CompareDefinition(definition, appliesTo, provenanceProvider));
        }

        return new FieldRegistryHarvestDiffResult(Array.AsReadOnly(rows.ToArray()));
    }

    private static FieldRegistryHarvestDiffRow CompareDefinition(
        Ra2FieldDefinition preview,
        Ra2SectionKind appliesTo,
        IRa2FieldDefinitionProvider effectiveProvider)
    {
        if (!effectiveProvider.TryGetField(appliesTo, preview.Key, out Ra2FieldDefinition existing))
        {
            return new FieldRegistryHarvestDiffRow(
                preview.Key,
                appliesTo,
                FieldRegistryHarvestDiffKind.Added,
                preview.EditorKind,
                null,
                preview.SourceKind,
                null,
                FieldRegistryProvenanceScope.None,
                "None",
                null,
                preview.Description,
                null,
                "New field candidate.");
        }

        List<string> differences = new();
        if (preview.EditorKind != existing.EditorKind)
            differences.Add("EditorKind differs");

        if (preview.SourceKind != existing.SourceKind)
            differences.Add("SourceKind differs");

        if (!DescriptionsEqual(preview.Description, existing.Description))
            differences.Add("Description differs");

        FieldRegistryHarvestDiffKind kind = differences.Count == 0
            ? FieldRegistryHarvestDiffKind.Same
            : FieldRegistryHarvestDiffKind.Changed;
        string message = differences.Count == 0
            ? "Already matches effective registry."
            : string.Join("; ", differences) + ".";

        return new FieldRegistryHarvestDiffRow(
            preview.Key,
            appliesTo,
            kind,
            preview.EditorKind,
            existing.EditorKind,
            preview.SourceKind,
            existing.SourceKind,
            FieldRegistryProvenanceScope.Unknown,
            "Unknown",
            null,
            preview.Description,
            existing.Description,
            message);
    }

    private static FieldRegistryHarvestDiffRow CompareDefinition(
        Ra2FieldDefinition preview,
        Ra2SectionKind appliesTo,
        IFieldRegistryProvenanceProvider provenanceProvider)
    {
        FieldRegistryProvenanceLookupResult lookup = provenanceProvider.TryGetFieldWithProvenance(appliesTo, preview.Key);
        if (!lookup.Found || lookup.Definition is null)
        {
            return new FieldRegistryHarvestDiffRow(
                preview.Key,
                appliesTo,
                FieldRegistryHarvestDiffKind.Added,
                preview.EditorKind,
                null,
                preview.SourceKind,
                null,
                FieldRegistryProvenanceScope.None,
                "None",
                null,
                preview.Description,
                null,
                "New field candidate.");
        }

        Ra2FieldDefinition existing = lookup.Definition;
        List<string> differences = new();
        if (preview.EditorKind != existing.EditorKind)
            differences.Add("EditorKind differs");

        if (preview.SourceKind != existing.SourceKind)
            differences.Add("SourceKind differs");

        if (!DescriptionsEqual(preview.Description, existing.Description))
            differences.Add("Description differs");

        FieldRegistryHarvestDiffKind kind = differences.Count == 0
            ? FieldRegistryHarvestDiffKind.Same
            : FieldRegistryHarvestDiffKind.Changed;
        string message = differences.Count == 0
            ? "Already matches effective registry."
            : string.Join("; ", differences) + ".";

        return new FieldRegistryHarvestDiffRow(
            preview.Key,
            appliesTo,
            kind,
            preview.EditorKind,
            existing.EditorKind,
            preview.SourceKind,
            existing.SourceKind,
            lookup.Scope,
            lookup.SourceName,
            lookup.SourcePath,
            preview.Description,
            existing.Description,
            message);
    }

    private static FieldRegistryHarvestDiffRow CreateInvalidRow(Ra2FieldDefinition definition, string message)
    {
        return new FieldRegistryHarvestDiffRow(
            definition.Key,
            definition.AppliesTo.FirstOrDefault(),
            FieldRegistryHarvestDiffKind.Invalid,
            definition.EditorKind,
            null,
            definition.SourceKind,
            null,
            FieldRegistryProvenanceScope.None,
            "None",
            null,
            definition.Description,
            null,
            message);
    }

    private static bool DescriptionsEqual(string? left, string? right)
        => string.Equals(NormalizeDescription(left), NormalizeDescription(right), StringComparison.Ordinal);

    private static string NormalizeDescription(string? value)
        => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
}
