using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

internal sealed class FieldRegistryProvenanceSnapshot : IFieldRegistryProvenanceProvider
{
    private readonly IReadOnlyList<FieldRegistryProvenanceEntry> _projectEntries;
    private readonly IReadOnlyList<FieldRegistryProvenanceEntry> _globalEntries;
    private readonly IRa2FieldDefinitionProvider _builtInProvider;

    public FieldRegistryProvenanceSnapshot(
        IReadOnlyList<FieldRegistryProvenanceEntry> projectEntries,
        IReadOnlyList<FieldRegistryProvenanceEntry> globalEntries,
        IRa2FieldDefinitionProvider builtInProvider)
    {
        _projectEntries = projectEntries ?? throw new ArgumentNullException(nameof(projectEntries));
        _globalEntries = globalEntries ?? throw new ArgumentNullException(nameof(globalEntries));
        _builtInProvider = builtInProvider ?? throw new ArgumentNullException(nameof(builtInProvider));
        Entries = Array.AsReadOnly(_projectEntries.Concat(_globalEntries).ToArray());
    }

    public IReadOnlyList<FieldRegistryProvenanceEntry> Entries { get; }

    public FieldRegistryProvenanceLookupResult TryGetFieldWithProvenance(
        Ra2SectionKind sectionKind,
        string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return FieldRegistryProvenanceLookupResult.NotFound;

        List<ProvenanceMatch> matches = [];
        TryFindBestLocalEntry(_projectEntries, sectionKind, key, providerIndex: 0, matches);
        TryFindBestLocalEntry(_globalEntries, sectionKind, key, providerIndex: 1, matches);
        TryFindBestBuiltInEntry(sectionKind, key, providerIndex: 2, matches);

        ProvenanceMatch? effectiveMatch = BuildEffectiveMatch(matches);
        if (effectiveMatch is null)
            return FieldRegistryProvenanceLookupResult.NotFound;

        return effectiveMatch.Scope == FieldRegistryProvenanceScope.BuiltIn
            ? FieldRegistryProvenanceLookupResult.BuiltIn(effectiveMatch.Definition)
            : FieldRegistryProvenanceLookupResult.FromEntry(new FieldRegistryProvenanceEntry(
                effectiveMatch.Definition.Key,
                ResolveSingleAppliesTo(effectiveMatch.Definition, sectionKind),
                effectiveMatch.Scope,
                effectiveMatch.SourceName,
                effectiveMatch.SourcePath,
                effectiveMatch.Definition));
    }

    private void TryFindBestBuiltInEntry(
        Ra2SectionKind sectionKind,
        string key,
        int providerIndex,
        List<ProvenanceMatch> matches)
    {
        foreach (Ra2FieldDefinition definition in _builtInProvider.GetFields(sectionKind))
        {
            if (!string.Equals(definition.Key, key.Trim(), StringComparison.OrdinalIgnoreCase))
                continue;

            ProvenanceMatch match = new(
                definition,
                FieldRegistryProvenanceScope.BuiltIn,
                "BuiltIn",
                null,
                GetMatchScore(sectionKind, definition.AppliesTo),
                providerIndex);
            if (match.MatchScore > 0)
                matches.Add(match);
        }
    }

    private static void TryFindBestLocalEntry(
        IReadOnlyList<FieldRegistryProvenanceEntry> entries,
        Ra2SectionKind sectionKind,
        string key,
        int providerIndex,
        List<ProvenanceMatch> matches)
    {
        string normalizedKey = key.Trim();
        foreach (FieldRegistryProvenanceEntry candidate in entries)
        {
            if (!string.Equals(candidate.Key, normalizedKey, StringComparison.OrdinalIgnoreCase))
                continue;

            ProvenanceMatch match = new(
                candidate.Definition,
                candidate.Scope,
                candidate.SourceName,
                candidate.SourcePath,
                GetMatchScore(sectionKind, [candidate.AppliesTo]),
                providerIndex);
            if (match.MatchScore > 0)
                matches.Add(match);
        }
    }

    private static bool IsBetterMatch(ProvenanceMatch candidate, ProvenanceMatch? current)
    {
        if (candidate.MatchScore <= 0)
            return false;

        if (current is null)
            return true;

        if (candidate.MatchScore != current.MatchScore)
            return candidate.MatchScore > current.MatchScore;

        return candidate.ProviderIndex < current.ProviderIndex;
    }

    private static ProvenanceMatch? BuildEffectiveMatch(IReadOnlyList<ProvenanceMatch> matches)
    {
        if (matches.Count == 0)
            return null;

        ProvenanceMatch primary = matches
            .OrderByDescending(match => match.MatchScore)
            .ThenBy(match => match.ProviderIndex)
            .First();

        Ra2FieldDefinition effectiveDefinition = primary.Definition;
        foreach (ProvenanceMatch fallback in matches
            .Where(match => match.ProviderIndex > primary.ProviderIndex &&
                match.Scope == FieldRegistryProvenanceScope.BuiltIn &&
                match.MatchScore > 0)
            .OrderByDescending(match => match.MatchScore)
            .ThenBy(match => match.ProviderIndex))
        {
            effectiveDefinition = EnrichWeakDefinition(effectiveDefinition, fallback.Definition);
        }

        return primary with { Definition = effectiveDefinition };
    }

    private static Ra2SectionKind ResolveSingleAppliesTo(Ra2FieldDefinition definition, Ra2SectionKind sectionKind)
        => definition.AppliesTo.Count == 1
            ? definition.AppliesTo.First()
            : sectionKind;

    private static Ra2FieldDefinition EnrichWeakDefinition(Ra2FieldDefinition primary, Ra2FieldDefinition fallback)
    {
        bool primaryIsWeak = IsWeakLearnedDefinition(primary);
        FieldEditorKind editorKind = primaryIsWeak && fallback.EditorKind != FieldEditorKind.Text
            ? fallback.EditorKind
            : primary.EditorKind;
        Ra2FieldValueMetadata valueMetadata = primaryIsWeak && IsStrongerValueMetadata(fallback.ValueMetadata, primary.ValueMetadata)
            ? fallback.ValueMetadata
            : primary.ValueMetadata;
        string? description = string.IsNullOrWhiteSpace(primary.Description)
            ? fallback.Description
            : primary.Description;
        string? displayName = HasMeaningfulDisplayName(primary)
            ? primary.DisplayName
            : fallback.DisplayName;
        IReadOnlyList<string> aliases = MergeAliases(primary.Aliases, fallback.Aliases);
        IReadOnlyList<Ra2FieldExample> examples = primary.Examples.Count > 0
            ? primary.Examples
            : fallback.Examples;

        return new Ra2FieldDefinition(
            primary.Key,
            primary.AppliesTo,
            editorKind,
            primary.SourceKind,
            description,
            valueMetadata,
            displayName,
            aliases,
            examples);
    }

    private static bool IsWeakLearnedDefinition(Ra2FieldDefinition definition)
    {
        if (definition.SourceKind == Ra2FieldSourceKind.BuiltIn)
            return false;

        return definition.EditorKind == FieldEditorKind.Text &&
            IsWeakValueMetadata(definition.ValueMetadata) &&
            !HasMeaningfulDisplayName(definition) &&
            string.IsNullOrWhiteSpace(definition.Description) &&
            definition.Aliases.Count == 0 &&
            definition.Examples.Count == 0;
    }

    private static bool IsWeakValueMetadata(Ra2FieldValueMetadata metadata)
        => !metadata.HasSchema ||
            metadata.ValueKind is Ra2FieldValueKind.Unknown or Ra2FieldValueKind.String;

    private static bool IsStrongerValueMetadata(Ra2FieldValueMetadata fallback, Ra2FieldValueMetadata primary)
    {
        if (!fallback.HasSchema || !IsWeakValueMetadata(primary))
            return false;

        return fallback.ValueKind is not Ra2FieldValueKind.Unknown and not Ra2FieldValueKind.String ||
            fallback.AllowedValues.Count > 0 ||
            fallback.BooleanStyle != Ra2FieldBooleanValueStyle.Unknown ||
            !string.IsNullOrWhiteSpace(fallback.EnumName);
    }

    private static bool HasMeaningfulDisplayName(Ra2FieldDefinition definition)
        => !string.IsNullOrWhiteSpace(definition.DisplayName) &&
            !string.Equals(definition.DisplayName, definition.Key, StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<string> MergeAliases(
        IReadOnlyCollection<string> primaryAliases,
        IReadOnlyCollection<string> fallbackAliases)
    {
        if (fallbackAliases.Count == 0)
            return Array.AsReadOnly(primaryAliases.ToArray());

        return Array.AsReadOnly(primaryAliases
            .Concat(fallbackAliases)
            .Where(alias => !string.IsNullOrWhiteSpace(alias))
            .Select(alias => alias.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray());
    }

    private static int GetMatchScore(Ra2SectionKind sectionKind, IReadOnlyCollection<Ra2SectionKind> appliesTo)
    {
        IReadOnlyCollection<Ra2SectionKind> normalizedKinds = appliesTo.Count == 0
            ? [Ra2SectionKind.Unknown]
            : appliesTo;

        int bestScore = 0;
        foreach (Ra2SectionKind kind in normalizedKinds)
            bestScore = Math.Max(bestScore, GetMatchScore(sectionKind, kind));

        return bestScore;
    }

    private static int GetMatchScore(Ra2SectionKind sectionKind, Ra2SectionKind candidateKind)
    {
        if (candidateKind == sectionKind)
            return 400;

        if (EnumerateAbstractLookupKinds(sectionKind).Contains(candidateKind))
            return 300;

        if (candidateKind == Ra2SectionKind.Global)
            return 200;

        return candidateKind == Ra2SectionKind.Unknown ? 100 : 0;
    }

    private static IEnumerable<Ra2SectionKind> EnumerateAbstractLookupKinds(Ra2SectionKind sectionKind)
    {
        if (sectionKind is Ra2SectionKind.Infantry or Ra2SectionKind.Vehicle or Ra2SectionKind.Aircraft)
            yield return Ra2SectionKind.Unit;

        if (sectionKind is Ra2SectionKind.Infantry or
            Ra2SectionKind.Vehicle or
            Ra2SectionKind.Aircraft or
            Ra2SectionKind.Building or
            Ra2SectionKind.Unit)
        {
            yield return Ra2SectionKind.Techno;
        }
    }

    private sealed record ProvenanceMatch(
        Ra2FieldDefinition Definition,
        FieldRegistryProvenanceScope Scope,
        string SourceName,
        string? SourcePath,
        int MatchScore,
        int ProviderIndex);
}
