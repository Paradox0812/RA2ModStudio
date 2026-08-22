using System.IO;
using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.Classification;

internal sealed class Ra2SectionClassifier : IRa2SectionClassifier
{
    private static readonly HashSet<string> WeaponReferenceKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "Primary",
        "Secondary",
        "ElitePrimary",
        "EliteSecondary",
        "DeathWeapon",
        "OpenToppedWeapon"
    };

    private static readonly HashSet<string> IgnoredReferenceValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "none",
        "<none>",
        "null",
        "empty",
        "yes",
        "no",
        "true",
        "false"
    };

    static Ra2SectionClassifier()
    {
        for (int index = 1; index <= 10; index++)
            WeaponReferenceKeys.Add($"Weapon{index}");
    }

    public Ra2SectionClassificationResult Classify(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new Ra2SectionClassificationResult(
                new Dictionary<string, Ra2SectionKind>(StringComparer.OrdinalIgnoreCase),
                Array.Empty<Ra2SectionClassificationWarning>());

        List<ParsedSection> sections = ParseSections(text);
        Dictionary<string, ClassificationEntry> classifications = new(StringComparer.OrdinalIgnoreCase);
        List<Ra2SectionClassificationWarning> warnings = [];

        ApplyDirectAndRegistryClassifications(sections, classifications, warnings);
        ApplyReferenceClassifications(sections, classifications, warnings);

        Dictionary<string, Ra2SectionKind> result = new(StringComparer.OrdinalIgnoreCase);
        foreach (KeyValuePair<string, ClassificationEntry> pair in classifications.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
            result[pair.Key] = pair.Value.Kind;

        return new Ra2SectionClassificationResult(result, warnings);
    }

    private static void ApplyDirectAndRegistryClassifications(
        IEnumerable<ParsedSection> sections,
        Dictionary<string, ClassificationEntry> classifications,
        List<Ra2SectionClassificationWarning> warnings)
    {
        foreach (ParsedSection section in sections)
        {
            Ra2SectionKind directKind = InferDirectSectionKind(section.SectionId);
            if (directKind != Ra2SectionKind.Unknown)
                SetClassification(classifications, warnings, section.SectionId, directKind, ClassificationSource.Direct, section.LineNumber);

            if (!TryGetRegistryEntryKind(section.SectionId, out Ra2SectionKind registryEntryKind))
                continue;

            foreach (string value in section.NumericValues)
            {
                if (TryGetReferenceToken(value, out string? referenceId))
                    SetClassification(classifications, warnings, referenceId, registryEntryKind, ClassificationSource.ExplicitRegistry, section.LineNumber);
            }
        }
    }

    private static void ApplyReferenceClassifications(
        IReadOnlyList<ParsedSection> sections,
        Dictionary<string, ClassificationEntry> classifications,
        List<Ra2SectionClassificationWarning> warnings)
    {
        bool changed;
        do
        {
            changed = false;
            foreach (ParsedSection section in sections)
            {
                Ra2SectionKind sectionKind = ResolveKnownKind(section.SectionId, classifications);
                if (IsWeaponOwnerKind(sectionKind))
                    changed |= ApplyWeaponReferenceClassifications(section, classifications, warnings);

                if (sectionKind == Ra2SectionKind.Weapon)
                    changed |= ApplyWeaponChildReferenceClassifications(section, classifications, warnings);
            }
        }
        while (changed);
    }

    private static bool ApplyWeaponReferenceClassifications(
        ParsedSection section,
        Dictionary<string, ClassificationEntry> classifications,
        List<Ra2SectionClassificationWarning> warnings)
    {
        bool changed = false;
        foreach (KeyValuePair<string, string> pair in section.Keys)
        {
            if (!WeaponReferenceKeys.Contains(pair.Key) || !TryGetReferenceToken(pair.Value, out string? weaponId))
                continue;

            changed |= SetClassification(classifications, warnings, weaponId, Ra2SectionKind.Weapon, ClassificationSource.Reference, section.LineNumber);
        }

        return changed;
    }

    private static bool ApplyWeaponChildReferenceClassifications(
        ParsedSection section,
        Dictionary<string, ClassificationEntry> classifications,
        List<Ra2SectionClassificationWarning> warnings)
    {
        bool changed = false;
        if (section.Keys.TryGetValue("Projectile", out string? projectileValue) &&
            TryGetReferenceToken(projectileValue, out string? projectileId))
        {
            changed |= SetClassification(classifications, warnings, projectileId, Ra2SectionKind.Projectile, ClassificationSource.Reference, section.LineNumber);
        }

        if (section.Keys.TryGetValue("Warhead", out string? warheadValue) &&
            TryGetReferenceToken(warheadValue, out string? warheadId))
        {
            changed |= SetClassification(classifications, warnings, warheadId, Ra2SectionKind.Warhead, ClassificationSource.Reference, section.LineNumber);
        }

        return changed;
    }

    private static bool SetClassification(
        Dictionary<string, ClassificationEntry> classifications,
        List<Ra2SectionClassificationWarning> warnings,
        string sectionName,
        Ra2SectionKind kind,
        ClassificationSource source,
        int lineNumber)
    {
        if (string.IsNullOrWhiteSpace(sectionName) || kind == Ra2SectionKind.Unknown)
            return false;

        if (!classifications.TryGetValue(sectionName, out ClassificationEntry? existing))
        {
            classifications[sectionName] = new ClassificationEntry(kind, source, lineNumber);
            return true;
        }

        if (existing.Kind == kind)
            return false;

        if (source == ClassificationSource.ExplicitRegistry &&
            existing.Source != ClassificationSource.ExplicitRegistry)
        {
            classifications[sectionName] = new ClassificationEntry(kind, source, lineNumber);
            return true;
        }

        if (existing.Source == ClassificationSource.ExplicitRegistry)
            return false;

        if (existing.Source == ClassificationSource.Reference && source == ClassificationSource.Reference)
        {
            string message = $"Reference classification conflict: keeping {existing.Kind}, ignored {kind}.";
            bool alreadyWarned = warnings.Any(warning =>
                warning.LineNumber == lineNumber &&
                string.Equals(warning.SectionName, sectionName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(warning.Message, message, StringComparison.Ordinal));
            if (!alreadyWarned)
                warnings.Add(new Ra2SectionClassificationWarning(sectionName, message, lineNumber));
        }

        return false;
    }

    private static Ra2SectionKind ResolveKnownKind(
        string sectionId,
        IReadOnlyDictionary<string, ClassificationEntry> classifications)
    {
        if (classifications.TryGetValue(sectionId, out ClassificationEntry? entry))
            return entry.Kind;

        return InferDirectSectionKind(sectionId);
    }

    private static bool IsWeaponOwnerKind(Ra2SectionKind kind)
        => kind is Ra2SectionKind.Infantry or
            Ra2SectionKind.Vehicle or
            Ra2SectionKind.Aircraft or
            Ra2SectionKind.Building or
            Ra2SectionKind.Unknown;

    private static Ra2SectionKind InferDirectSectionKind(string sectionId)
    {
        ReadOnlySpan<char> value = sectionId.AsSpan().Trim();
        if (value.Equals("General".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
            value.Equals("AudioVisual".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
            value.Equals("CombatDamage".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
            value.Equals("Countries".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
            value.Equals("Sides".AsSpan(), StringComparison.OrdinalIgnoreCase) ||
            IsRegistrySection(value))
        {
            return Ra2SectionKind.Global;
        }

        return Ra2SectionKind.Unknown;
    }

    private static bool TryGetRegistryEntryKind(string sectionId, out Ra2SectionKind entryKind)
    {
        entryKind = sectionId.AsSpan().Trim() switch
        {
            var value when value.Equals("InfantryTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Infantry,
            var value when value.Equals("VehicleTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Vehicle,
            var value when value.Equals("AircraftTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Aircraft,
            var value when value.Equals("BuildingTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Building,
            var value when value.Equals("WeaponTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Weapon,
            var value when value.Equals("SuperWeaponTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.SuperWeapon,
            var value when value.Equals("Warheads".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Warhead,
            var value when value.Equals("WarheadTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Warhead,
            var value when value.Equals("Projectiles".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Projectile,
            var value when value.Equals("ProjectileTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Projectile,
            var value when value.Equals("Animations".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Animation,
            var value when value.Equals("VoxelAnims".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.VoxelAnim,
            var value when value.Equals("Particles".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Particle,
            var value when value.Equals("ParticleSystems".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.ParticleSystem,
            var value when value.Equals("TerrainTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Terrain,
            var value when value.Equals("OverlayTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Overlay,
            var value when value.Equals("AITriggerTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.AITrigger,
            var value when value.Equals("TaskForces".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.TaskForce,
            var value when value.Equals("ScriptTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Script,
            var value when value.Equals("TeamTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.TeamType,
            var value when value.Equals("ShieldTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Shield,
            var value when value.Equals("AttachEffectTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.AttachEffect,
            var value when value.Equals("LaserTrailTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.LaserTrail,
            var value when value.Equals("DigitalDisplayTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.DigitalDisplay,
            var value when value.Equals("DigitalDisplays".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.DigitalDisplay,
            var value when value.Equals("BannerTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Banner,
            var value when value.Equals("InsigniaTypes".AsSpan(), StringComparison.OrdinalIgnoreCase) => Ra2SectionKind.Insignia,
            _ => Ra2SectionKind.Unknown
        };

        return entryKind != Ra2SectionKind.Unknown;
    }

    private static bool IsRegistrySection(ReadOnlySpan<char> sectionId)
        => TryGetRegistryEntryKind(sectionId.ToString(), out _);

    private static bool TryGetReferenceToken(string rawValue, out string referenceId)
    {
        referenceId = string.Empty;
        string token = StripInlineComment(rawValue)
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(token) ||
            IgnoredReferenceValues.Contains(token) ||
            IsNumeric(token))
        {
            return false;
        }

        referenceId = token;
        return true;
    }

    private static List<ParsedSection> ParseSections(string sourceText)
    {
        List<ParsedSection> sections = [];
        ParsedSection? current = null;
        using StringReader reader = new(sourceText);

        int lineNumber = 0;
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            lineNumber++;
            string trimmed = line.Trim();
            if (TryReadSectionId(trimmed, out string? sectionId))
            {
                if (current is not null)
                    sections.Add(current);

                current = new ParsedSection(sectionId!, lineNumber);
                continue;
            }

            if (current is not null)
                TryCaptureKeyValue(current, trimmed);
        }

        if (current is not null)
            sections.Add(current);

        return sections;
    }

    private static bool TryReadSectionId(string trimmedLine, out string? sectionId)
    {
        sectionId = null;
        if (trimmedLine.StartsWith(';') || trimmedLine.StartsWith('#'))
            return false;

        if (!trimmedLine.StartsWith('['))
            return false;

        int closeBracketIndex = trimmedLine.IndexOf(']');
        if (closeBracketIndex <= 1)
            return false;

        string suffix = trimmedLine[(closeBracketIndex + 1)..].TrimStart();
        if (suffix.Length > 0 && suffix[0] is not ';' and not '#')
            return false;

        string candidate = trimmedLine[1..closeBracketIndex].Trim();
        if (string.IsNullOrWhiteSpace(candidate))
            return false;

        sectionId = candidate;
        return true;
    }

    private static void TryCaptureKeyValue(ParsedSection section, string trimmedLine)
    {
        if (trimmedLine.Length == 0 || trimmedLine.StartsWith(';') || trimmedLine.StartsWith('#'))
            return;

        int separatorIndex = trimmedLine.IndexOf('=');
        if (separatorIndex <= 0)
            return;

        string key = trimmedLine[..separatorIndex].Trim();
        string value = StripInlineComment(trimmedLine[(separatorIndex + 1)..]).Trim();
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(value))
            return;

        section.Keys[key] = value;
        if (IsNumeric(key))
            section.NumericValues.Add(value);
    }

    private static bool IsNumeric(string value)
    {
        foreach (char character in value)
        {
            if (!char.IsDigit(character))
                return false;
        }

        return value.Length > 0;
    }

    private static string StripInlineComment(string value)
    {
        int semicolonIndex = value.IndexOf(';');
        int hashIndex = value.IndexOf('#');
        int commentIndex = semicolonIndex >= 0 && hashIndex >= 0
            ? Math.Min(semicolonIndex, hashIndex)
            : Math.Max(semicolonIndex, hashIndex);

        return commentIndex >= 0 ? value[..commentIndex] : value;
    }

    private enum ClassificationSource
    {
        Direct,
        Reference,
        ExplicitRegistry
    }

    private sealed class ClassificationEntry
    {
        public ClassificationEntry(Ra2SectionKind kind, ClassificationSource source, int lineNumber)
        {
            Kind = kind;
            Source = source;
            LineNumber = lineNumber;
        }

        public Ra2SectionKind Kind { get; }

        public ClassificationSource Source { get; }

        public int LineNumber { get; }
    }

    private sealed class ParsedSection
    {
        public ParsedSection(string sectionId, int lineNumber)
        {
            SectionId = sectionId;
            LineNumber = lineNumber;
        }

        public string SectionId { get; }

        public int LineNumber { get; }

        public Dictionary<string, string> Keys { get; } = new(StringComparer.OrdinalIgnoreCase);

        public List<string> NumericValues { get; } = [];
    }
}
