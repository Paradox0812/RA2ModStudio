namespace RA2IniEditor.IDE.AI;

internal sealed class Ra2AiFieldEvidence
{
    public Ra2AiFieldEvidence(
        string key,
        string? displayName,
        string? sectionKind,
        string? valueKind,
        string? description,
        string? example,
        string? sourceName,
        string? provenance,
        string matchReason,
        double score)
    {
        Key = string.IsNullOrWhiteSpace(key) ? string.Empty : key.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        SectionKind = string.IsNullOrWhiteSpace(sectionKind) ? null : sectionKind.Trim();
        ValueKind = string.IsNullOrWhiteSpace(valueKind) ? null : valueKind.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Example = string.IsNullOrWhiteSpace(example) ? null : example.Trim();
        SourceName = string.IsNullOrWhiteSpace(sourceName) ? null : sourceName.Trim();
        Provenance = string.IsNullOrWhiteSpace(provenance) ? null : provenance.Trim();
        MatchReason = string.IsNullOrWhiteSpace(matchReason) ? "unknown" : matchReason.Trim();
        Score = Math.Max(0, score);
    }

    public string Key { get; }

    public string? DisplayName { get; }

    public string? SectionKind { get; }

    public string? ValueKind { get; }

    public string? Description { get; }

    public string? Example { get; }

    public string? SourceName { get; }

    public string? Provenance { get; }

    public string MatchReason { get; }

    public double Score { get; }
}
