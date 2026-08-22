using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.FieldAnnotations;

internal sealed class Ra2EffectiveFieldCatalog : IRa2EffectiveFieldCatalog
{
    private static readonly Ra2SectionKind[] CatalogKinds = Enum.GetValues<Ra2SectionKind>()
        .Where(kind => kind != Ra2SectionKind.Unknown)
        .ToArray();

    private readonly IRa2FieldDisplayResolver _displayResolver;
    private readonly Dictionary<Ra2SectionKind, IReadOnlyList<Ra2FieldDisplayInfo>> _fieldsByKind = new();
    private readonly Dictionary<string, int> _keyKindCounts;

    public Ra2EffectiveFieldCatalog(IRa2FieldDisplayResolver displayResolver)
    {
        _displayResolver = displayResolver ?? throw new ArgumentNullException(nameof(displayResolver));
        foreach (Ra2SectionKind kind in CatalogKinds)
            _fieldsByKind[kind] = Deduplicate(_displayResolver.GetFields(kind));

        _keyKindCounts = BuildKeyKindCounts(_fieldsByKind);
    }

    public IReadOnlyList<Ra2EffectiveFieldItem> GetApplicableFields(Ra2SectionKind sectionKind)
    {
        if (!_fieldsByKind.TryGetValue(sectionKind, out IReadOnlyList<Ra2FieldDisplayInfo>? fields))
            return [];

        return fields
            .Select(info => new Ra2EffectiveFieldItem(sectionKind, Classify(sectionKind, info), info))
            .OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public IReadOnlyList<Ra2EffectiveFieldItem> GetCommonFields()
    {
        Dictionary<string, Ra2EffectiveFieldItem> result = new(StringComparer.OrdinalIgnoreCase);
        foreach (Ra2SectionKind kind in CatalogKinds)
        {
            foreach (Ra2EffectiveFieldItem item in GetApplicableFields(kind))
            {
                if (item.Applicability == Ra2FieldApplicabilityKind.Common)
                    result.TryAdd(item.Key, item);
            }
        }

        return Sort(result.Values);
    }

    public IReadOnlyList<Ra2EffectiveFieldItem> GetSpecificFields(Ra2SectionKind sectionKind)
        => Sort(GetApplicableFields(sectionKind).Where(item => item.Applicability == Ra2FieldApplicabilityKind.SectionSpecific));

    public IReadOnlyList<Ra2EffectiveFieldItem> GetAllFields()
    {
        Dictionary<string, Ra2EffectiveFieldItem> result = new(StringComparer.OrdinalIgnoreCase);
        foreach (Ra2SectionKind kind in CatalogKinds)
        {
            foreach (Ra2EffectiveFieldItem item in GetApplicableFields(kind))
                result.TryAdd(item.Key, item);
        }

        return Sort(result.Values);
    }

    private Ra2FieldApplicabilityKind Classify(Ra2SectionKind sectionKind, Ra2FieldDisplayInfo info)
    {
        if (string.Equals(info.TypeDisplay, "Unknown", StringComparison.OrdinalIgnoreCase))
            return Ra2FieldApplicabilityKind.Unknown;

        return _keyKindCounts.TryGetValue(info.Key, out int count) && count > 1
            ? Ra2FieldApplicabilityKind.Common
            : Ra2FieldApplicabilityKind.SectionSpecific;
    }

    private static IReadOnlyList<Ra2FieldDisplayInfo> Deduplicate(IReadOnlyList<Ra2FieldDisplayInfo> fields)
    {
        Dictionary<string, Ra2FieldDisplayInfo> result = new(StringComparer.OrdinalIgnoreCase);
        foreach (Ra2FieldDisplayInfo field in fields)
            result.TryAdd(field.Key, field);

        return Sort(result.Values);
    }

    private static Dictionary<string, int> BuildKeyKindCounts(
        Dictionary<Ra2SectionKind, IReadOnlyList<Ra2FieldDisplayInfo>> fieldsByKind)
    {
        Dictionary<string, HashSet<Ra2SectionKind>> kindsByKey = new(StringComparer.OrdinalIgnoreCase);
        foreach ((Ra2SectionKind kind, IReadOnlyList<Ra2FieldDisplayInfo> fields) in fieldsByKind)
        {
            foreach (Ra2FieldDisplayInfo field in fields)
            {
                if (!kindsByKey.TryGetValue(field.Key, out HashSet<Ra2SectionKind>? kinds))
                {
                    kinds = new HashSet<Ra2SectionKind>();
                    kindsByKey[field.Key] = kinds;
                }

                kinds.Add(kind);
            }
        }

        return kindsByKey.ToDictionary(pair => pair.Key, pair => pair.Value.Count, StringComparer.OrdinalIgnoreCase);
    }

    private static IReadOnlyList<Ra2EffectiveFieldItem> Sort(IEnumerable<Ra2EffectiveFieldItem> items)
        => items.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase).ToArray();

    private static IReadOnlyList<Ra2FieldDisplayInfo> Sort(IEnumerable<Ra2FieldDisplayInfo> items)
        => items.OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase).ToArray();
}
