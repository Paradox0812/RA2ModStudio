namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal sealed class FieldRegistryHarvestNormalizeResult
{
    public FieldRegistryHarvestNormalizeResult(
        IReadOnlyList<FieldRegistryHarvestNormalizedCandidate> candidates,
        IReadOnlyList<FieldRegistryHarvestValidationIssue> issues)
    {
        Candidates = candidates ?? throw new ArgumentNullException(nameof(candidates));
        Issues = issues ?? throw new ArgumentNullException(nameof(issues));
    }

    public IReadOnlyList<FieldRegistryHarvestNormalizedCandidate> Candidates { get; }

    public IReadOnlyList<FieldRegistryHarvestValidationIssue> Issues { get; }
}
