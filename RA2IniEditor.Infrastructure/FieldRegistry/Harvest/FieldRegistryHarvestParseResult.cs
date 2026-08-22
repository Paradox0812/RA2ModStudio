namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal sealed class FieldRegistryHarvestParseResult
{
    public FieldRegistryHarvestParseResult(
        IReadOnlyList<FieldRegistryHarvestCandidate> candidates,
        IReadOnlyList<FieldRegistryHarvestWarning> warnings)
    {
        Candidates = candidates ?? throw new ArgumentNullException(nameof(candidates));
        Warnings = warnings ?? throw new ArgumentNullException(nameof(warnings));
    }

    public IReadOnlyList<FieldRegistryHarvestCandidate> Candidates { get; }

    public IReadOnlyList<FieldRegistryHarvestWarning> Warnings { get; }
}

