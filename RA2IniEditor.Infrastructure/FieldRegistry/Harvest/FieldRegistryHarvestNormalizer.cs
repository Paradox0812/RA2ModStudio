using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Harvest;

internal sealed class FieldRegistryHarvestNormalizer : IFieldRegistryHarvestNormalizer
{
    private static readonly Dictionary<string, Ra2SectionKind> SectionKindAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Global"] = Ra2SectionKind.Global,
        ["Unknown"] = Ra2SectionKind.Unknown,
        ["Infantry"] = Ra2SectionKind.Infantry,
        ["Inf"] = Ra2SectionKind.Infantry,
        ["Vehicle"] = Ra2SectionKind.Vehicle,
        ["Veh"] = Ra2SectionKind.Vehicle,
        ["Building"] = Ra2SectionKind.Building,
        ["Bld"] = Ra2SectionKind.Building,
        ["Aircraft"] = Ra2SectionKind.Aircraft,
        ["Air"] = Ra2SectionKind.Aircraft,
        ["Weapon"] = Ra2SectionKind.Weapon,
        ["Warhead"] = Ra2SectionKind.Warhead,
        ["WH"] = Ra2SectionKind.Warhead,
        ["Projectile"] = Ra2SectionKind.Projectile,
        ["Proj"] = Ra2SectionKind.Projectile,
        ["Particle"] = Ra2SectionKind.Particle,
        ["Ptc"] = Ra2SectionKind.Particle,
        ["ParticleSystem"] = Ra2SectionKind.ParticleSystem,
        ["Animation"] = Ra2SectionKind.Animation,
        ["VoxelAnim"] = Ra2SectionKind.VoxelAnim,
        ["VoxelAnimation"] = Ra2SectionKind.VoxelAnimation,
        ["SuperWeapon"] = Ra2SectionKind.SuperWeapon,
        ["SW"] = Ra2SectionKind.SuperWeapon,
        ["Terrain"] = Ra2SectionKind.Terrain,
        ["Terr"] = Ra2SectionKind.Terrain
    };

    private static readonly Dictionary<string, FieldEditorKind> EditorKindAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Text"] = FieldEditorKind.Text,
        ["String"] = FieldEditorKind.Text,
        ["Int"] = FieldEditorKind.Integer,
        ["Integer"] = FieldEditorKind.Integer,
        ["Float"] = FieldEditorKind.Float,
        ["Double"] = FieldEditorKind.Float,
        ["Bool"] = FieldEditorKind.Boolean,
        ["Boolean"] = FieldEditorKind.Boolean,
        ["YesNo"] = FieldEditorKind.Boolean,
        ["Enum"] = FieldEditorKind.Enum,
        ["List"] = FieldEditorKind.MultiSelect,
        ["MultiSelect"] = FieldEditorKind.MultiSelect,
        ["Reference"] = FieldEditorKind.Reference,
        ["Percent"] = FieldEditorKind.Percent,
        ["Color"] = FieldEditorKind.Color,
        ["ColorDefinition"] = FieldEditorKind.ColorDefinition,
        ["Coordinate"] = FieldEditorKind.Coordinate,
        ["Verses"] = FieldEditorKind.Verses
    };

    public FieldRegistryHarvestNormalizeResult Normalize(
        IReadOnlyList<FieldRegistryHarvestCandidate> candidates,
        FieldRegistryHarvestNormalizeOptions options)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(options);

        List<FieldRegistryHarvestNormalizedCandidate> normalizedCandidates = new();
        List<FieldRegistryHarvestValidationIssue> issues = new();
        Dictionary<string, int> normalizedIndexes = new(StringComparer.OrdinalIgnoreCase);

        foreach (FieldRegistryHarvestCandidate candidate in candidates)
        {
            if (!TryNormalizeCandidate(candidate, options, issues, out FieldRegistryHarvestNormalizedCandidate? normalized) ||
                normalized is null)
            {
                continue;
            }

            AddDeduplicatedCandidate(normalized, normalizedCandidates, issues, normalizedIndexes);
        }

        return new FieldRegistryHarvestNormalizeResult(
            Array.AsReadOnly(normalizedCandidates.ToArray()),
            Array.AsReadOnly(issues.ToArray()));
    }

    private static bool TryNormalizeCandidate(
        FieldRegistryHarvestCandidate candidate,
        FieldRegistryHarvestNormalizeOptions options,
        List<FieldRegistryHarvestValidationIssue> issues,
        out FieldRegistryHarvestNormalizedCandidate? normalized)
    {
        normalized = null;
        string key = candidate.Key.Trim();
        if (!ValidateKey(candidate, key, issues))
            return false;

        if (!TryNormalizeAppliesTo(candidate, options, issues, out IReadOnlyList<Ra2SectionKind> appliesTo, out bool usedDefaultAppliesTo))
            return false;

        if (!TryNormalizeEditorKind(candidate, options, issues, out FieldEditorKind editorKind, out bool usedDefaultEditorKind))
            return false;

        string? description = string.IsNullOrWhiteSpace(candidate.Description)
            ? null
            : candidate.Description.Trim();

        normalized = new FieldRegistryHarvestNormalizedCandidate(
            key,
            appliesTo,
            editorKind,
            options.DefaultSourceKind,
            description,
            candidate.SourceName,
            candidate.LineNumber,
            candidate.RawLine,
            candidate.Confidence,
            usedDefaultAppliesTo,
            usedDefaultEditorKind);
        return true;
    }

    private static bool ValidateKey(
        FieldRegistryHarvestCandidate candidate,
        string key,
        List<FieldRegistryHarvestValidationIssue> issues)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            AddIssue(issues, candidate, key, FieldRegistryHarvestValidationSeverity.Error, "Field key is empty.");
            return false;
        }

        foreach (char character in key)
        {
            if (char.IsLetterOrDigit(character) || character is '_' or '.' or '-')
                continue;

            AddIssue(issues, candidate, key, FieldRegistryHarvestValidationSeverity.Error, $"Field key '{key}' contains invalid character '{character}'.");
            return false;
        }

        return true;
    }

    private static bool TryNormalizeAppliesTo(
        FieldRegistryHarvestCandidate candidate,
        FieldRegistryHarvestNormalizeOptions options,
        List<FieldRegistryHarvestValidationIssue> issues,
        out IReadOnlyList<Ra2SectionKind> appliesTo,
        out bool usedDefault)
    {
        usedDefault = false;
        appliesTo = Array.AsReadOnly(Array.Empty<Ra2SectionKind>());
        if (string.IsNullOrWhiteSpace(candidate.AppliesToRaw))
        {
            usedDefault = true;
            appliesTo = Array.AsReadOnly([options.DefaultAppliesTo]);
            AddIssue(issues, candidate, candidate.Key, FieldRegistryHarvestValidationSeverity.Info, $"Used default appliesTo '{options.DefaultAppliesTo}'.");
            return true;
        }

        List<Ra2SectionKind> result = new();
        foreach (string token in SplitAppliesToTokens(candidate.AppliesToRaw))
        {
            if (SectionKindAliases.TryGetValue(token, out Ra2SectionKind kind))
            {
                AddDistinct(result, kind);
                continue;
            }

            if (options.AllowUnknownAppliesTo)
            {
                AddDistinct(result, Ra2SectionKind.Unknown);
                AddIssue(issues, candidate, candidate.Key, FieldRegistryHarvestValidationSeverity.Warning, $"Unknown appliesTo '{token}' mapped to Unknown.");
                continue;
            }

            AddIssue(issues, candidate, candidate.Key, FieldRegistryHarvestValidationSeverity.Error, $"Unknown appliesTo '{token}'.");
            return false;
        }

        if (result.Count == 0)
        {
            usedDefault = true;
            result.Add(options.DefaultAppliesTo);
            AddIssue(issues, candidate, candidate.Key, FieldRegistryHarvestValidationSeverity.Info, $"Used default appliesTo '{options.DefaultAppliesTo}'.");
        }

        appliesTo = Array.AsReadOnly(result.ToArray());
        return true;
    }

    private static bool TryNormalizeEditorKind(
        FieldRegistryHarvestCandidate candidate,
        FieldRegistryHarvestNormalizeOptions options,
        List<FieldRegistryHarvestValidationIssue> issues,
        out FieldEditorKind editorKind,
        out bool usedDefault)
    {
        usedDefault = false;
        editorKind = options.DefaultEditorKind;
        if (string.IsNullOrWhiteSpace(candidate.EditorKindRaw))
        {
            usedDefault = true;
            AddIssue(issues, candidate, candidate.Key, FieldRegistryHarvestValidationSeverity.Info, $"Used default editorKind '{options.DefaultEditorKind}'.");
            return true;
        }

        string raw = candidate.EditorKindRaw.Trim();
        if (EditorKindAliases.TryGetValue(raw, out editorKind))
            return true;

        if (options.AllowUnknownEditorKind)
        {
            usedDefault = true;
            editorKind = options.DefaultEditorKind;
            AddIssue(issues, candidate, candidate.Key, FieldRegistryHarvestValidationSeverity.Warning, $"Unknown editorKind '{raw}' mapped to default '{options.DefaultEditorKind}'.");
            return true;
        }

        AddIssue(issues, candidate, candidate.Key, FieldRegistryHarvestValidationSeverity.Error, $"Unknown editorKind '{raw}'.");
        return false;
    }

    private static void AddDeduplicatedCandidate(
        FieldRegistryHarvestNormalizedCandidate candidate,
        List<FieldRegistryHarvestNormalizedCandidate> candidates,
        List<FieldRegistryHarvestValidationIssue> issues,
        Dictionary<string, int> indexes)
    {
        string duplicateKey = BuildDuplicateKey(candidate);
        if (!indexes.TryGetValue(duplicateKey, out int existingIndex))
        {
            indexes[duplicateKey] = candidates.Count;
            candidates.Add(candidate);
            return;
        }

        FieldRegistryHarvestNormalizedCandidate existing = candidates[existingIndex];
        if (candidate.Confidence > existing.Confidence)
            candidates[existingIndex] = candidate;

        AddIssue(
            issues,
            candidate.SourceName,
            candidate.LineNumber,
            candidate.Key,
            FieldRegistryHarvestValidationSeverity.Warning,
            $"Duplicate normalized field '{candidate.Key}' with same appliesTo was {(candidate.Confidence > existing.Confidence ? "replaced by higher confidence candidate" : "skipped")}.");
    }

    private static string BuildDuplicateKey(FieldRegistryHarvestNormalizedCandidate candidate)
    {
        string appliesToKey = string.Join(",", candidate.AppliesTo
            .Distinct()
            .OrderBy(kind => (int)kind)
            .Select(kind => kind.ToString()));
        return $"{candidate.Key}|{appliesToKey}";
    }

    private static IEnumerable<string> SplitAppliesToTokens(string raw)
    {
        return raw.Split([',', ';', '/'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }

    private static void AddDistinct(List<Ra2SectionKind> values, Ra2SectionKind value)
    {
        if (!values.Contains(value))
            values.Add(value);
    }

    private static void AddIssue(
        List<FieldRegistryHarvestValidationIssue> issues,
        FieldRegistryHarvestCandidate candidate,
        string? key,
        FieldRegistryHarvestValidationSeverity severity,
        string message)
    {
        AddIssue(issues, candidate.SourceName, candidate.LineNumber, key, severity, message);
    }

    private static void AddIssue(
        List<FieldRegistryHarvestValidationIssue> issues,
        string sourceName,
        int lineNumber,
        string? key,
        FieldRegistryHarvestValidationSeverity severity,
        string message)
    {
        issues.Add(new FieldRegistryHarvestValidationIssue(sourceName, lineNumber, key, severity, message));
    }
}
