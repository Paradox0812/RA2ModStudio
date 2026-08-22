using System.IO;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Classification;

namespace RA2IniEditor.IDE.Services;

/// <summary>
/// Builds readonly Project Explorer groups from the currently loaded INI source text.
/// </summary>
public sealed class ReadonlyProjectExplorerGroupingService
{
    private readonly IRa2SectionClassifier _sectionClassifier;

    private static readonly IReadOnlyDictionary<string, string> RegistryTypeGroups =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["InfantryTypes"] = "Infantry",
            ["VehicleTypes"] = "Vehicle",
            ["AircraftTypes"] = "Aircraft",
            ["BuildingTypes"] = "Building",
            ["WeaponTypes"] = "Weapon",
            ["Warheads"] = "Warhead",
            ["WarheadTypes"] = "Warhead",
            ["ProjectileTypes"] = "Projectile",
            ["Animations"] = "Animation",
            ["VoxelAnims"] = "VoxelAnim",
            ["Particles"] = "Particle",
            ["ParticleSystems"] = "Particle",
            ["SuperWeaponTypes"] = "SuperWeapon",
            ["AITriggerTypes"] = "AI",
            ["TaskForces"] = "AI",
            ["ScriptTypes"] = "AI",
            ["TeamTypes"] = "AI",
            ["TerrainTypes"] = "Terrain / Overlay",
            ["OverlayTypes"] = "Terrain / Overlay"
        };

    private static readonly HashSet<string> GlobalRegistrySections = new(StringComparer.OrdinalIgnoreCase)
    {
        "General",
        "AudioVisual",
        "CombatDamage",
        "Radiation",
        "ElevationModel",
        "WallModel",
        "Countries",
        "Sides",
        "Houses",
        "MultiplayerDialogSettings",
        "SpecialWeapons",
        "JumpjetControls",
        "AI",
        "IQ",
        "Easy",
        "Normal",
        "Difficult",
        "CrateRules",
        "Powerups"
    };

    private static readonly HashSet<string> FactionTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Infantry",
        "Vehicle",
        "Aircraft",
        "Building",
        "SuperWeapon"
    };

    public ReadonlyProjectExplorerGroupingService()
        : this(new Ra2SectionClassifier())
    {
    }

    internal ReadonlyProjectExplorerGroupingService(IRa2SectionClassifier sectionClassifier)
    {
        _sectionClassifier = sectionClassifier ?? throw new ArgumentNullException(nameof(sectionClassifier));
    }

    /// <summary>
    /// Builds grouped section classifications without reading any files.
    /// </summary>
    public IReadOnlyList<ReadonlySectionClassificationResult> BuildGroups(string sourceText)
    {
        List<ParsedSection> sections = ParseSections(sourceText);
        IReadOnlyDictionary<string, Ra2SectionKind> sectionKinds = _sectionClassifier.Classify(sourceText).SectionKindsByName;

        List<ReadonlySectionClassificationResult> results = [];
        foreach (ParsedSection section in GetFirstHeaderSections(sections))
        {
            string typeGroup = ClassifyType(section, sectionKinds);
            string? factionGroup = FactionTypes.Contains(typeGroup)
                ? ClassifyFaction(section)
                : null;

            results.Add(new ReadonlySectionClassificationResult(
                section.SectionId,
                section.LineNumber,
                section.DisplayName,
                typeGroup,
                factionGroup));
        }

        return results;
    }

    private static IEnumerable<ParsedSection> GetFirstHeaderSections(IEnumerable<ParsedSection> sections)
    {
        HashSet<string> seen = new(StringComparer.OrdinalIgnoreCase);
        foreach (ParsedSection section in sections)
        {
            if (seen.Add(section.SectionId))
                yield return section;
        }
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

    private static Dictionary<string, string> BuildRegisteredTypes(IEnumerable<ParsedSection> sections)
    {
        Dictionary<string, string> registeredTypes = new(StringComparer.OrdinalIgnoreCase);
        foreach (ParsedSection section in sections)
        {
            if (!RegistryTypeGroups.TryGetValue(section.SectionId, out string? typeGroup))
                continue;

            foreach (string value in section.Values)
            {
                if (!registeredTypes.ContainsKey(value))
                    registeredTypes[value] = typeGroup;
            }
        }

        return registeredTypes;
    }

    private static string ClassifyType(ParsedSection section, IReadOnlyDictionary<string, Ra2SectionKind> sectionKinds)
    {
        if (sectionKinds.TryGetValue(section.SectionId, out Ra2SectionKind sectionKind))
            return ToTypeGroup(sectionKind);

        if (section.Keys.ContainsKey("Animates") || section.Keys.ContainsKey("LoopStart") || section.Keys.ContainsKey("LoopEnd"))
            return "Animation";

        return "Unknown";
    }

    private static string ToTypeGroup(Ra2SectionKind sectionKind) => sectionKind switch
    {
        Ra2SectionKind.Global => "Global / Registry",
        Ra2SectionKind.Infantry => "Infantry",
        Ra2SectionKind.Vehicle => "Vehicle",
        Ra2SectionKind.Aircraft => "Aircraft",
        Ra2SectionKind.Building => "Building",
        Ra2SectionKind.Weapon => "Weapon",
        Ra2SectionKind.Warhead => "Warhead",
        Ra2SectionKind.Projectile => "Projectile",
        Ra2SectionKind.Animation => "Animation",
        Ra2SectionKind.VoxelAnim => "VoxelAnim",
        Ra2SectionKind.Particle => "Particle",
        Ra2SectionKind.ParticleSystem => "Particle",
        Ra2SectionKind.SuperWeapon => "SuperWeapon",
        Ra2SectionKind.AI or Ra2SectionKind.AITrigger or Ra2SectionKind.TaskForce or Ra2SectionKind.Script or Ra2SectionKind.TeamType => "AI",
        Ra2SectionKind.Terrain or Ra2SectionKind.Overlay => "Terrain / Overlay",
        _ => "Unknown"
    };

    private static string ClassifyFaction(ParsedSection section)
    {
        HashSet<string> factions = new(StringComparer.OrdinalIgnoreCase);
        AddFactionFromValue(section.GetValue("Owner"), factions);
        AddFactionFromValue(section.GetValue("RequiredHouses"), factions);
        AddFactionFromPrerequisite(section.GetValue("Prerequisite"), factions);

        if (factions.Count == 0)
            return "Unknown";

        if (factions.Count == 1)
            return factions.Single();

        return "Common";
    }

    private static void AddFactionFromValue(string? rawValue, ISet<string> factions)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return;

        foreach (string token in SplitTokens(rawValue))
        {
            switch (token)
            {
                case "British":
                case "French":
                case "Germans":
                case "Americans":
                case "Alliance":
                    factions.Add("Allied");
                    break;
                case "Russians":
                case "Confederation":
                case "Africans":
                case "Arabs":
                    factions.Add("Soviet");
                    break;
                case "YuriCountry":
                case "Yuri":
                    factions.Add("Yuri");
                    break;
                case "Neutral":
                case "Civilian":
                case "Special":
                    factions.Add("Neutral");
                    break;
            }
        }
    }

    private static void AddFactionFromPrerequisite(string? rawValue, ISet<string> factions)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return;

        foreach (string token in SplitTokens(rawValue))
        {
            string upper = token.ToUpperInvariant();
            if (upper.StartsWith("GA", StringComparison.Ordinal))
                factions.Add("Allied");
            else if (upper.StartsWith("NA", StringComparison.Ordinal))
                factions.Add("Soviet");
            else if (upper.StartsWith("YA", StringComparison.Ordinal))
                factions.Add("Yuri");
        }
    }

    private static IEnumerable<string> SplitTokens(string value)
    {
        return StripInlineComment(value)
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
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
        if (IsNumericKey(key))
            section.Values.Add(value);
    }

    private static bool IsNumericKey(string key)
    {
        foreach (char character in key)
        {
            if (!char.IsDigit(character))
                return false;
        }

        return key.Length > 0;
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

        public List<string> Values { get; } = [];

        public string? DisplayName => GetValue("Name") ?? GetValue("UIName") ?? GetValue("Image");

        public string? GetValue(string key) => Keys.TryGetValue(key, out string? value) ? value : null;
    }
}
