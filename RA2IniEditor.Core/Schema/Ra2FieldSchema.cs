namespace RA2IniEditor.Core.Schema;

/// <summary>
/// Describes the basic editor/display category for a known RA2 INI field.
/// </summary>
public enum FieldEditorKind
{
    Text,
    Integer,
    Float,
    Percent,
    Boolean,
    Enum,
    Color,
    ColorDefinition,
    AbilityFlags,
    Coordinate,
    Reference,
    MultiSelect,
    Verses
}

/// <summary>
/// Describes the source of a field definition.
/// </summary>
public enum Ra2FieldSourceKind
{
    Unknown,
    BuiltIn,
    Ra2,
    Yuri,
    Ares,
    Phobos,
    Custom,
    External,
    ExternalDictionary,
    User,
    UserDictionary
}

/// <summary>
/// Describes a coarse RA2 INI section category.
/// </summary>
public enum Ra2SectionKind
{
    Unknown,
    Global,
    Techno,
    Unit,
    Infantry,
    Vehicle,
    Aircraft,
    Building,
    Weapon,
    Projectile,
    Warhead,
    Animation,
    VoxelAnimation,
    VoxelAnim = VoxelAnimation,
    SuperWeapon,
    Terrain,
    Overlay,
    Smudge,
    Particle,
    ParticleSystem,
    Sound,
    TaskForce,
    Script,
    TeamType,
    AITrigger,
    AI,
    Country,
    ArtObject,
    Side,
    AttachEffect,
    Shield,
    LaserTrail,
    DigitalDisplay,
    Banner,
    Insignia,
    Radiation,
    Eva,
    Tiberium,
    MiscObject
}

/// <summary>
/// Describes the value schema advertised by a field registry definition.
/// </summary>
public enum Ra2FieldValueKind
{
    Unknown,
    String,
    Boolean,
    Integer,
    Float,
    Enum,
    EnumList,
    Reference,
    ReferenceList
}

/// <summary>
/// Describes the preferred textual representation for boolean values.
/// </summary>
public enum Ra2FieldBooleanValueStyle
{
    Unknown,
    YesNo,
    TrueFalse,
    Custom
}

/// <summary>
/// Represents one allowed value declared by a field registry definition.
/// </summary>
public sealed class Ra2FieldAllowedValue
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Ra2FieldAllowedValue"/> class.
    /// </summary>
    public Ra2FieldAllowedValue(
        string value,
        string? displayName = null,
        string? description = null,
        int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Allowed value cannot be empty.", nameof(value));

        Value = value.Trim();
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Priority = priority;
    }

    /// <summary>
    /// Gets the raw value inserted into the INI text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets an optional display label for the value.
    /// </summary>
    public string? DisplayName { get; }

    /// <summary>
    /// Gets an optional value description.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets the ordering priority inside completion lists.
    /// </summary>
    public int Priority { get; }
}

/// <summary>
/// Represents one example value shown in field details.
/// </summary>
public sealed class Ra2FieldExample
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Ra2FieldExample"/> class.
    /// </summary>
    public Ra2FieldExample(string value, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Field example value cannot be empty.", nameof(value));

        Value = value.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    /// <summary>
    /// Gets the example raw value.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Gets an optional example description.
    /// </summary>
    public string? Description { get; }
}

/// <summary>
/// Represents readonly value metadata declared for a field definition.
/// </summary>
public sealed class Ra2FieldValueMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Ra2FieldValueMetadata"/> class.
    /// </summary>
    public Ra2FieldValueMetadata(
        Ra2FieldValueKind valueKind,
        Ra2FieldBooleanValueStyle booleanStyle = Ra2FieldBooleanValueStyle.Unknown,
        IReadOnlyCollection<Ra2FieldAllowedValue>? allowedValues = null,
        string? enumName = null,
        string separator = ",")
    {
        ValueKind = valueKind;
        BooleanStyle = booleanStyle;
        AllowedValues = Array.AsReadOnly((allowedValues ?? []).ToArray());
        EnumName = string.IsNullOrWhiteSpace(enumName) ? null : enumName.Trim();
        Separator = string.IsNullOrEmpty(separator) ? "," : separator;
    }

    /// <summary>
    /// Gets an empty metadata instance for definitions without value schema.
    /// </summary>
    public static Ra2FieldValueMetadata Unknown { get; } = new(Ra2FieldValueKind.Unknown);

    /// <summary>
    /// Gets the declared value kind.
    /// </summary>
    public Ra2FieldValueKind ValueKind { get; }

    /// <summary>
    /// Gets the declared boolean style.
    /// </summary>
    public Ra2FieldBooleanValueStyle BooleanStyle { get; }

    /// <summary>
    /// Gets allowed raw values for enum-like fields.
    /// </summary>
    public IReadOnlyCollection<Ra2FieldAllowedValue> AllowedValues { get; }

    /// <summary>
    /// Gets an optional enum catalog name.
    /// </summary>
    public string? EnumName { get; }

    /// <summary>
    /// Gets the list value separator.
    /// </summary>
    public string Separator { get; }

    /// <summary>
    /// Gets whether this metadata contains a concrete value schema.
    /// </summary>
    public bool HasSchema =>
        ValueKind != Ra2FieldValueKind.Unknown ||
        BooleanStyle != Ra2FieldBooleanValueStyle.Unknown ||
        AllowedValues.Count > 0 ||
        !string.IsNullOrWhiteSpace(EnumName);
}

/// <summary>
/// Represents one readonly RA2 INI field definition.
/// </summary>
public sealed class Ra2FieldDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Ra2FieldDefinition"/> class.
    /// </summary>
    public Ra2FieldDefinition(
        string key,
        IReadOnlyCollection<Ra2SectionKind> appliesTo,
        FieldEditorKind editorKind,
        Ra2FieldSourceKind sourceKind,
        string? description = null,
        Ra2FieldValueMetadata? valueMetadata = null,
        string? displayName = null,
        IReadOnlyCollection<string>? aliases = null,
        string? registryQuality = null)
        : this(key, appliesTo, editorKind, sourceKind, description, valueMetadata, displayName, aliases, null, registryQuality)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Ra2FieldDefinition"/> class with display-only examples.
    /// </summary>
    public Ra2FieldDefinition(
        string key,
        IReadOnlyCollection<Ra2SectionKind> appliesTo,
        FieldEditorKind editorKind,
        Ra2FieldSourceKind sourceKind,
        string? description,
        Ra2FieldValueMetadata? valueMetadata,
        string? displayName,
        IReadOnlyCollection<string>? aliases,
        IReadOnlyCollection<Ra2FieldExample>? examples,
        string? registryQuality = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Field key cannot be empty.", nameof(key));

        if (key.Contains('='))
            throw new ArgumentException("Field key cannot contain '='.", nameof(key));

        Key = key.Trim();
        AppliesTo = appliesTo is null
            ? throw new ArgumentNullException(nameof(appliesTo))
            : Array.AsReadOnly(appliesTo.ToArray());
        EditorKind = editorKind;
        SourceKind = sourceKind;
        Description = description;
        ValueMetadata = valueMetadata ?? Ra2FieldValueMetadata.Unknown;
        DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName.Trim();
        Aliases = Array.AsReadOnly((aliases ?? [])
            .Where(alias => !string.IsNullOrWhiteSpace(alias))
            .Select(alias => alias.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray());
        Examples = Array.AsReadOnly((examples ?? [])
            .Where(example => example is not null)
            .ToArray());
        RegistryQuality = string.IsNullOrWhiteSpace(registryQuality) ? null : registryQuality.Trim();
    }

    /// <summary>
    /// Gets the INI key without an equals sign.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the section kinds this field applies to. Empty means common or not yet restricted.
    /// </summary>
    public IReadOnlyCollection<Ra2SectionKind> AppliesTo { get; }

    /// <summary>
    /// Gets the suggested editor/display kind.
    /// </summary>
    public FieldEditorKind EditorKind { get; }

    /// <summary>
    /// Gets the field definition source.
    /// </summary>
    public Ra2FieldSourceKind SourceKind { get; }

    /// <summary>
    /// Gets an optional human-readable field description.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Gets optional value metadata declared by field registries.
    /// </summary>
    public Ra2FieldValueMetadata ValueMetadata { get; }

    /// <summary>
    /// Gets an optional user-facing field label.
    /// </summary>
    public string? DisplayName { get; }

    /// <summary>
    /// Gets optional searchable aliases for the field.
    /// </summary>
    public IReadOnlyList<string> Aliases { get; }

    /// <summary>
    /// Gets example values for detail surfaces. Examples do not participate in validation, completion, or saving.
    /// </summary>
    public IReadOnlyList<Ra2FieldExample> Examples { get; }

    /// <summary>
    /// Gets the optional registry quality tag loaded from a field registry pack.
    /// </summary>
    public string? RegistryQuality { get; }
}

/// <summary>
/// Provides readonly RA2 field definition queries.
/// </summary>
public interface IRa2FieldDefinitionProvider
{
    /// <summary>
    /// Tries to find a field by section kind and key.
    /// </summary>
    bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition);

    /// <summary>
    /// Gets known field definitions for a section kind.
    /// </summary>
    IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind);

    /// <summary>
    /// Returns whether the key is known for the section kind.
    /// </summary>
    bool IsKnownField(Ra2SectionKind sectionKind, string key);
}

/// <summary>
/// Combines multiple readonly RA2 field definition providers in priority order.
/// </summary>
public sealed class CompositeRa2FieldDefinitionProvider : IRa2FieldDefinitionProvider
{
    private readonly IReadOnlyList<IRa2FieldDefinitionProvider> _providers;
    private readonly Dictionary<Ra2SectionKind, IReadOnlyList<Ra2FieldDefinition>> _fieldsCache = new();
    private readonly object _fieldsCacheLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeRa2FieldDefinitionProvider"/> class.
    /// Providers are queried in the order they are passed in.
    /// </summary>
    public CompositeRa2FieldDefinitionProvider(IEnumerable<IRa2FieldDefinitionProvider> providers)
    {
        ArgumentNullException.ThrowIfNull(providers);
        _providers = Array.AsReadOnly(providers
            .Where(provider => provider is not null)
            .ToArray());
    }

    /// <summary>
    /// Gets the providers in priority order.
    /// </summary>
    public IReadOnlyList<IRa2FieldDefinitionProvider> Providers => _providers;

    /// <inheritdoc />
    public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
    {
        definition = null!;
        if (string.IsNullOrWhiteSpace(key))
            return false;

        List<FieldMatch> matches = [];
        string normalizedKey = key.Trim();
        for (int providerIndex = 0; providerIndex < _providers.Count; providerIndex++)
        {
            if (!_providers[providerIndex].TryGetField(sectionKind, normalizedKey, out Ra2FieldDefinition candidate))
                continue;

            FieldMatch match = new(candidate, GetMatchScore(sectionKind, candidate), providerIndex);
            if (match.MatchScore > 0)
                matches.Add(match);
        }

        FieldMatch? effectiveMatch = BuildEffectiveMatch(matches);
        definition = effectiveMatch?.Definition!;
        return effectiveMatch is not null;
    }

    /// <inheritdoc />
    public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
    {
        lock (_fieldsCacheLock)
        {
            if (_fieldsCache.TryGetValue(sectionKind, out IReadOnlyList<Ra2FieldDefinition>? cachedFields))
                return cachedFields;
        }

        Dictionary<string, List<FieldMatch>> result = new(StringComparer.OrdinalIgnoreCase);
        for (int providerIndex = 0; providerIndex < _providers.Count; providerIndex++)
        {
            foreach (Ra2FieldDefinition definition in _providers[providerIndex].GetFields(sectionKind))
            {
                FieldMatch match = new(definition, GetMatchScore(sectionKind, definition), providerIndex);
                if (match.MatchScore <= 0)
                    continue;

                if (!result.TryGetValue(definition.Key, out List<FieldMatch>? matches))
                {
                    matches = [];
                    result[definition.Key] = matches;
                }

                matches.Add(match);
            }
        }

        IReadOnlyList<Ra2FieldDefinition> fields = Array.AsReadOnly(result.Values
            .Select(BuildEffectiveMatch)
            .Where(match => match is not null)
            .Select(match => match!.Definition)
            .OrderBy(definition => definition.Key, StringComparer.OrdinalIgnoreCase)
            .ToArray());

        lock (_fieldsCacheLock)
        {
            if (!_fieldsCache.TryGetValue(sectionKind, out IReadOnlyList<Ra2FieldDefinition>? cachedFields))
            {
                _fieldsCache[sectionKind] = fields;
                return fields;
            }

            return cachedFields;
        }
    }

    /// <inheritdoc />
    public bool IsKnownField(Ra2SectionKind sectionKind, string key)
        => TryGetField(sectionKind, key, out _);

    private static bool IsBetterMatch(FieldMatch candidate, FieldMatch? current)
    {
        if (current is null)
            return true;

        if (candidate.MatchScore != current.MatchScore)
            return candidate.MatchScore > current.MatchScore;

        return candidate.ProviderIndex < current.ProviderIndex;
    }

    private static FieldMatch? BuildEffectiveMatch(IReadOnlyList<FieldMatch> matches)
    {
        if (matches.Count == 0)
            return null;

        FieldMatch primary = SelectPrimaryMatch(matches);

        Ra2FieldDefinition effectiveDefinition = primary.Definition;
        foreach (FieldMatch fallback in matches
            .Where(match => match.ProviderIndex > primary.ProviderIndex &&
                IsBuiltInFallbackSource(match.Definition) &&
                match.MatchScore > 0)
            .OrderByDescending(match => match.MatchScore)
            .ThenBy(match => match.ProviderIndex))
        {
            effectiveDefinition = EnrichWeakDefinition(effectiveDefinition, fallback.Definition);
        }

        return primary with { Definition = effectiveDefinition };
    }

    private static FieldMatch SelectPrimaryMatch(IReadOnlyList<FieldMatch> matches)
    {
        IReadOnlyList<FieldMatch> localStrongMatches = matches
            .Where(match => !IsBuiltInFallbackSource(match.Definition) && match.MatchScore > 100)
            .ToArray();

        if (localStrongMatches.Count > 0)
        {
            return localStrongMatches
                .OrderByDescending(match => match.MatchScore)
                .ThenBy(match => match.ProviderIndex)
                .First();
        }

        return matches
            .OrderByDescending(match => match.MatchScore)
            .ThenBy(match => match.ProviderIndex)
            .First();
    }

    private static bool IsBuiltInFallbackSource(Ra2FieldDefinition definition)
        => definition.SourceKind is
            Ra2FieldSourceKind.BuiltIn or
            Ra2FieldSourceKind.Ra2 or
            Ra2FieldSourceKind.Yuri or
            Ra2FieldSourceKind.Ares or
            Ra2FieldSourceKind.Phobos;

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
        string? registryQuality = string.IsNullOrWhiteSpace(primary.RegistryQuality)
            ? fallback.RegistryQuality
            : primary.RegistryQuality;

        if (editorKind == primary.EditorKind &&
            ReferenceEquals(valueMetadata, primary.ValueMetadata) &&
            string.Equals(description, primary.Description, StringComparison.Ordinal) &&
            string.Equals(displayName, primary.DisplayName, StringComparison.Ordinal) &&
            aliases.SequenceEqual(primary.Aliases, StringComparer.OrdinalIgnoreCase) &&
            examples.SequenceEqual(primary.Examples) &&
            string.Equals(registryQuality, primary.RegistryQuality, StringComparison.Ordinal))
        {
            return primary;
        }

        return new Ra2FieldDefinition(
            primary.Key,
            primary.AppliesTo,
            editorKind,
            primary.SourceKind,
            description,
            valueMetadata,
            displayName,
            aliases,
            examples,
            registryQuality);
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

    private static int GetMatchScore(Ra2SectionKind sectionKind, Ra2FieldDefinition definition)
    {
        IReadOnlyCollection<Ra2SectionKind> appliesTo = definition.AppliesTo.Count == 0
            ? [Ra2SectionKind.Unknown]
            : definition.AppliesTo;

        int bestScore = 0;
        foreach (Ra2SectionKind kind in appliesTo)
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

    private sealed record FieldMatch(Ra2FieldDefinition Definition, int MatchScore, int ProviderIndex);
}

/// <summary>
/// Provides a minimal built-in RA2 field definition set for readonly IDE features.
/// </summary>
public sealed class BuiltInRa2FieldDefinitionProvider : IRa2FieldDefinitionProvider
{
    private static readonly Ra2SectionKind[] TechnoKinds =
    [
        Ra2SectionKind.Techno
    ];

    private static readonly Ra2SectionKind[] UnitKinds =
    [
        Ra2SectionKind.Unit
    ];

    private static readonly Ra2SectionKind[] ArtObjectKinds =
    [
        Ra2SectionKind.ArtObject
    ];

    private static readonly IReadOnlyList<Ra2FieldDefinition> Definitions =
    [
        Define("Name", FieldEditorKind.Text, "Internal object name."),
        Define("UIName", FieldEditorKind.Text, "CSF display name key."),
        Define("Image", FieldEditorKind.Reference, "Art image section reference."),
        Define("Cameo", FieldEditorKind.Reference, "Sidebar cameo resource reference.", ArtObjectKinds),
        Define("AltCameo", FieldEditorKind.Reference, "Alternate sidebar cameo resource reference.", ArtObjectKinds),
        Define("Voxel", FieldEditorKind.Boolean, "Whether this art object uses VXL/HVA voxel graphics.", ArtObjectKinds),
        Define("Remapable", FieldEditorKind.Boolean, "Whether this art object uses owner-color remapping.", ArtObjectKinds),
        Define("Owner", FieldEditorKind.MultiSelect, "Allowed owning houses.", TechnoKinds),
        Define("Prerequisite", FieldEditorKind.MultiSelect, "Build prerequisite list.", TechnoKinds),
        Define("Strength", FieldEditorKind.Integer, "Object hit points.", TechnoKinds),
        Define("Armor", FieldEditorKind.Enum, "Armor type.", TechnoKinds),
        Define("Crusher", FieldEditorKind.Boolean, "Whether this object can crush targets.", TechnoKinds),
        Define("Powered", FieldEditorKind.Boolean, "Whether this object requires power.", TechnoKinds),
        Define("Trainable", FieldEditorKind.Boolean, "Whether this object can gain veterancy.", TechnoKinds),
        Define("Selectable", FieldEditorKind.Boolean, "Whether this object can be selected.", TechnoKinds),
        Define("RadarInvisible", FieldEditorKind.Boolean, "Whether this object is hidden from radar.", TechnoKinds),
        Define("Insignificant", FieldEditorKind.Boolean, "Whether this object is ignored by some scoring and AI logic.", TechnoKinds),
        Define("IsBaseDefense", FieldEditorKind.Boolean, "Whether this object is treated as base defense.", TechnoKinds),
        Define("Landable", FieldEditorKind.Boolean, "Whether aircraft can land on this object.", TechnoKinds),
        Define("CanPassiveAquire", FieldEditorKind.Boolean, "Whether this object can passively acquire targets.", TechnoKinds),
        Define("CanRetaliate", FieldEditorKind.Boolean, "Whether this object can retaliate when attacked.", TechnoKinds),
        Define("VeteranAbilities", FieldEditorKind.MultiSelect, "Abilities granted at veteran rank.", TechnoKinds),
        Define("EliteAbilities", FieldEditorKind.MultiSelect, "Abilities granted at elite rank.", TechnoKinds),
        Define("TechLevel", FieldEditorKind.Integer, "Build tech level.", TechnoKinds),
        Define("Cost", FieldEditorKind.Integer, "Build cost.", TechnoKinds),
        Define("Soylent", FieldEditorKind.Integer, "Recycle or sell value.", TechnoKinds),
        Define("Primary", FieldEditorKind.Reference, "Primary weapon reference.", TechnoKinds),
        Define("Secondary", FieldEditorKind.Reference, "Secondary weapon reference.", TechnoKinds),
        Define("Category", FieldEditorKind.Enum, "Object category.", TechnoKinds),
        Define("Speed", FieldEditorKind.Integer, "Movement or projectile speed.", UnitKinds),
        Define("Sight", FieldEditorKind.Integer, "Sight range.", TechnoKinds)
    ];

    private readonly Dictionary<Ra2SectionKind, Dictionary<string, Ra2FieldDefinition>> _definitionsByKind;
    private readonly Dictionary<Ra2SectionKind, IReadOnlyList<Ra2FieldDefinition>> _fieldsCache = new();
    private readonly object _fieldsCacheLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BuiltInRa2FieldDefinitionProvider"/> class.
    /// </summary>
    public BuiltInRa2FieldDefinitionProvider()
    {
        _definitionsByKind = BuildIndex(Definitions);
    }

    /// <inheritdoc />
    public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
    {
        definition = null!;
        if (string.IsNullOrWhiteSpace(key))
            return false;

        string normalizedKey = key.Trim();
        foreach (Ra2SectionKind candidateKind in EnumerateLookupKinds(sectionKind))
        {
            if (_definitionsByKind.TryGetValue(candidateKind, out Dictionary<string, Ra2FieldDefinition>? fields) &&
                fields.TryGetValue(normalizedKey, out definition!))
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc />
    public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
    {
        lock (_fieldsCacheLock)
        {
            if (_fieldsCache.TryGetValue(sectionKind, out IReadOnlyList<Ra2FieldDefinition>? cachedFields))
                return cachedFields;
        }

        Dictionary<string, Ra2FieldDefinition> result = new(StringComparer.OrdinalIgnoreCase);
        foreach (Ra2SectionKind candidateKind in EnumerateLookupKinds(sectionKind))
        {
            if (!_definitionsByKind.TryGetValue(candidateKind, out Dictionary<string, Ra2FieldDefinition>? fields))
                continue;

            foreach (Ra2FieldDefinition definition in fields.Values)
                result.TryAdd(definition.Key, definition);
        }

        IReadOnlyList<Ra2FieldDefinition> effectiveFields = Array.AsReadOnly(result.Values.OrderBy(definition => definition.Key, StringComparer.OrdinalIgnoreCase).ToArray());
        lock (_fieldsCacheLock)
        {
            if (!_fieldsCache.TryGetValue(sectionKind, out IReadOnlyList<Ra2FieldDefinition>? cachedFields))
            {
                _fieldsCache[sectionKind] = effectiveFields;
                return effectiveFields;
            }

            return cachedFields;
        }
    }

    /// <inheritdoc />
    public bool IsKnownField(Ra2SectionKind sectionKind, string key)
        => TryGetField(sectionKind, key, out _);

    private static Ra2FieldDefinition Define(
        string key,
        FieldEditorKind editorKind,
        string description,
        params Ra2SectionKind[] appliesTo)
    {
        return new Ra2FieldDefinition(key, appliesTo, editorKind, Ra2FieldSourceKind.BuiltIn, description);
    }

    private static Ra2FieldDefinition Define(
        string key,
        FieldEditorKind editorKind,
        string description,
        IReadOnlyCollection<Ra2SectionKind> appliesTo)
    {
        return new Ra2FieldDefinition(key, appliesTo, editorKind, Ra2FieldSourceKind.BuiltIn, description);
    }

    private static Dictionary<Ra2SectionKind, Dictionary<string, Ra2FieldDefinition>> BuildIndex(
        IEnumerable<Ra2FieldDefinition> definitions)
    {
        Dictionary<Ra2SectionKind, Dictionary<string, Ra2FieldDefinition>> result = new();
        foreach (Ra2FieldDefinition definition in definitions)
        {
            IReadOnlyCollection<Ra2SectionKind> appliesTo = definition.AppliesTo.Count == 0
                ? [Ra2SectionKind.Unknown]
                : definition.AppliesTo;

            foreach (Ra2SectionKind kind in appliesTo)
            {
                if (!result.TryGetValue(kind, out Dictionary<string, Ra2FieldDefinition>? fields))
                {
                    fields = new Dictionary<string, Ra2FieldDefinition>(StringComparer.OrdinalIgnoreCase);
                    result[kind] = fields;
                }

                fields.TryAdd(definition.Key, definition);
            }
        }

        return result;
    }

    private static IEnumerable<Ra2SectionKind> EnumerateLookupKinds(Ra2SectionKind sectionKind)
    {
        yield return sectionKind;

        foreach (Ra2SectionKind abstractKind in EnumerateAbstractLookupKinds(sectionKind))
            yield return abstractKind;

        if (sectionKind != Ra2SectionKind.Global)
            yield return Ra2SectionKind.Global;

        if (sectionKind != Ra2SectionKind.Unknown)
            yield return Ra2SectionKind.Unknown;
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
}
