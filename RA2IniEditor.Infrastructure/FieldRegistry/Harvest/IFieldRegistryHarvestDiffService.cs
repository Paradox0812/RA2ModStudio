using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal interface IFieldRegistryHarvestDiffService
{
    FieldRegistryHarvestDiffResult Compare(
        FieldRegistryHarvestPreviewDraft previewDraft,
        IRa2FieldDefinitionProvider effectiveProvider);

    FieldRegistryHarvestDiffResult Compare(
        FieldRegistryHarvestPreviewDraft previewDraft,
        IFieldRegistryProvenanceProvider provenanceProvider);
}
