using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.FieldAnnotations;

internal sealed class Ra2FieldAnnotationProvider : IRa2FieldAnnotationProvider
{
    private readonly Dictionary<AnnotationKey, Ra2FieldAnnotationEntry> _entries = new();

    public Ra2FieldAnnotationProvider(Ra2FieldAnnotationPack? pack)
    {
        foreach (Ra2FieldAnnotationEntry entry in pack?.Entries ?? [])
        {
            _entries[CreateKey(entry.SectionKind, entry.Key)] = entry;
        }
    }

    public Ra2FieldAnnotationEntry? Find(Ra2SectionKind sectionKind, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return null;

        if (_entries.TryGetValue(CreateKey(sectionKind.ToString(), key), out Ra2FieldAnnotationEntry? exact))
            return exact;

        return _entries.TryGetValue(CreateKey("*", key), out Ra2FieldAnnotationEntry? wildcard)
            ? wildcard
            : null;
    }

    private static AnnotationKey CreateKey(string sectionKind, string key)
        => new(NormalizeSectionKind(sectionKind), key.Trim());

    private static string NormalizeSectionKind(string sectionKind)
    {
        string trimmed = sectionKind.Trim();
        if (trimmed == "*")
            return trimmed;

        return trimmed.EndsWith("Type", StringComparison.OrdinalIgnoreCase)
            ? trimmed[..^"Type".Length]
            : trimmed;
    }

    private readonly record struct AnnotationKey(string SectionKind, string Key)
    {
        public bool Equals(AnnotationKey other)
            => string.Equals(SectionKind, other.SectionKind, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(Key, other.Key, StringComparison.OrdinalIgnoreCase);

        public override int GetHashCode()
            => HashCode.Combine(
                StringComparer.OrdinalIgnoreCase.GetHashCode(SectionKind),
                StringComparer.OrdinalIgnoreCase.GetHashCode(Key));
    }
}
