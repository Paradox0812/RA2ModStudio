using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry;

/// <summary>
/// Provides readonly RA2 field definitions loaded from local field registry packs.
/// </summary>
public sealed class LocalRa2FieldDefinitionProvider : IRa2FieldDefinitionProvider
{
    private readonly Dictionary<Ra2SectionKind, Dictionary<string, Ra2FieldDefinition>> _definitionsByKind;
    private readonly Dictionary<Ra2SectionKind, IReadOnlyList<Ra2FieldDefinition>> _fieldsCache = new();
    private readonly object _fieldsCacheLock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalRa2FieldDefinitionProvider"/> class.
    /// </summary>
    public LocalRa2FieldDefinitionProvider(IEnumerable<Ra2FieldDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        _definitionsByKind = BuildIndex(definitions);
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

                fields[definition.Key] = definition;
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
