namespace RA2IniEditor.IDE.FieldAnnotations;

internal interface IRa2FieldAnnotationEditingService
{
    Ra2FieldAnnotationPack Upsert(
        Ra2FieldAnnotationPack pack,
        string sectionKind,
        string key,
        string displayName,
        IReadOnlyList<string> aliases,
        string? note);

    Ra2FieldAnnotationPack Remove(Ra2FieldAnnotationPack pack, string sectionKind, string key);
}

internal sealed class Ra2FieldAnnotationEditingService : IRa2FieldAnnotationEditingService
{
    public Ra2FieldAnnotationPack Upsert(
        Ra2FieldAnnotationPack pack,
        string sectionKind,
        string key,
        string displayName,
        IReadOnlyList<string> aliases,
        string? note)
    {
        ArgumentNullException.ThrowIfNull(pack);
        if (string.IsNullOrWhiteSpace(sectionKind))
            throw new ArgumentException("Section kind cannot be empty.", nameof(sectionKind));
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Field key cannot be empty.", nameof(key));

        bool hasContent = !string.IsNullOrWhiteSpace(displayName) ||
                          aliases.Any(alias => !string.IsNullOrWhiteSpace(alias)) ||
                          !string.IsNullOrWhiteSpace(note);
        if (!hasContent)
            return Remove(pack, sectionKind, key);

        List<Ra2FieldAnnotationEntry> entries = RemoveMatching(pack, sectionKind, key);
        entries.Add(new Ra2FieldAnnotationEntry(
            sectionKind,
            key,
            displayName,
            NormalizeAliases(aliases),
            note));
        return new Ra2FieldAnnotationPack(pack.Version, pack.Language, entries);
    }

    public Ra2FieldAnnotationPack Remove(Ra2FieldAnnotationPack pack, string sectionKind, string key)
    {
        ArgumentNullException.ThrowIfNull(pack);
        return new Ra2FieldAnnotationPack(pack.Version, pack.Language, RemoveMatching(pack, sectionKind, key));
    }

    private static List<Ra2FieldAnnotationEntry> RemoveMatching(
        Ra2FieldAnnotationPack pack,
        string sectionKind,
        string key)
    {
        return pack.Entries
            .Where(entry => !string.Equals(entry.SectionKind, sectionKind, StringComparison.OrdinalIgnoreCase) ||
                            !string.Equals(entry.Key, key, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static IReadOnlyList<string> NormalizeAliases(IReadOnlyList<string> aliases)
    {
        return aliases
            .Where(alias => !string.IsNullOrWhiteSpace(alias))
            .Select(alias => alias.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
