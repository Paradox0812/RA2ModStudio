using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal sealed class FieldRegistryHarvestDiffRow
{
    public FieldRegistryHarvestDiffRow(
        string key,
        Ra2SectionKind appliesTo,
        FieldRegistryHarvestDiffKind kind,
        FieldEditorKind? previewEditorKind,
        FieldEditorKind? existingEditorKind,
        Ra2FieldSourceKind? previewSourceKind,
        Ra2FieldSourceKind? existingSourceKind,
        FieldRegistryProvenanceScope existingScope,
        string existingSourceName,
        string? existingSourcePath,
        string? previewDescription,
        string? existingDescription,
        string message)
    {
        Key = key;
        AppliesTo = appliesTo;
        Kind = kind;
        PreviewEditorKind = previewEditorKind;
        ExistingEditorKind = existingEditorKind;
        PreviewSourceKind = previewSourceKind;
        ExistingSourceKind = existingSourceKind;
        ExistingScope = existingScope;
        ExistingSourceName = existingSourceName;
        ExistingSourcePath = existingSourcePath;
        PreviewDescription = previewDescription;
        ExistingDescription = existingDescription;
        Message = message;
    }

    public string Key { get; }

    public Ra2SectionKind AppliesTo { get; }

    public FieldRegistryHarvestDiffKind Kind { get; }

    public FieldEditorKind? PreviewEditorKind { get; }

    public FieldEditorKind? ExistingEditorKind { get; }

    public Ra2FieldSourceKind? PreviewSourceKind { get; }

    public Ra2FieldSourceKind? ExistingSourceKind { get; }

    public FieldRegistryProvenanceScope ExistingScope { get; }

    public string ExistingSourceName { get; }

    public string? ExistingSourcePath { get; }

    public string? PreviewDescription { get; }

    public string? ExistingDescription { get; }

    public string Message { get; }
}
