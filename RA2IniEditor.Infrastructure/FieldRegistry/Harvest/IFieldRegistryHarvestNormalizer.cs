namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal interface IFieldRegistryHarvestNormalizer
{
    FieldRegistryHarvestNormalizeResult Normalize(
        IReadOnlyList<FieldRegistryHarvestCandidate> candidates,
        FieldRegistryHarvestNormalizeOptions options);
}
