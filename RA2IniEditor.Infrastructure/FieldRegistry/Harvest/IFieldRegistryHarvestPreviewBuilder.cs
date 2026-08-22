namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal interface IFieldRegistryHarvestPreviewBuilder
{
    FieldRegistryHarvestPreviewDraft BuildPreview(FieldRegistryHarvestNormalizeResult normalizeResult);
}
