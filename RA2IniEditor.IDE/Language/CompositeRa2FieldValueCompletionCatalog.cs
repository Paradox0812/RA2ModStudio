namespace RA2IniEditor.IDE.Language;

internal sealed class CompositeRa2FieldValueCompletionCatalog : IRa2FieldValueCompletionCatalog
{
    private readonly IReadOnlyList<IRa2FieldValueCompletionCatalog> _catalogs;

    public CompositeRa2FieldValueCompletionCatalog(IEnumerable<IRa2FieldValueCompletionCatalog> catalogs)
    {
        ArgumentNullException.ThrowIfNull(catalogs);
        _catalogs = catalogs.Where(catalog => catalog is not null).ToArray();
    }

    public IReadOnlyList<Ra2FieldValueCompletionCandidate> GetCandidates(
        Ra2FieldValueCompletionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        Dictionary<string, Ra2FieldValueCompletionCandidate> candidates = new(StringComparer.OrdinalIgnoreCase);
        foreach (IRa2FieldValueCompletionCatalog catalog in _catalogs)
        {
            foreach (Ra2FieldValueCompletionCandidate candidate in catalog.GetCandidates(request))
            {
                if (!candidates.TryGetValue(candidate.Value, out Ra2FieldValueCompletionCandidate? existing) ||
                    candidate.Priority > existing.Priority)
                {
                    candidates[candidate.Value] = candidate;
                }
            }
        }

        return candidates.Values
            .OrderByDescending(candidate => candidate.Priority)
            .ThenBy(candidate => candidate.Value, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
