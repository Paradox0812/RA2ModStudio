namespace RA2IniEditor.IDE.FieldAnnotations;

internal sealed class Ra2FieldAnnotationEntry
{
    public Ra2FieldAnnotationEntry(
        string sectionKind,
        string key,
        string displayName,
        IReadOnlyList<string>? aliases = null,
        string? note = null)
    {
        if (string.IsNullOrWhiteSpace(sectionKind))
            throw new ArgumentException("Section kind cannot be empty.", nameof(sectionKind));

        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Field key cannot be empty.", nameof(key));

        SectionKind = sectionKind.Trim();
        Key = key.Trim();
        DisplayName = displayName?.Trim() ?? string.Empty;
        Aliases = Array.AsReadOnly((aliases ?? []).Where(alias => !string.IsNullOrWhiteSpace(alias)).Select(alias => alias.Trim()).ToArray());
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    public string SectionKind { get; }

    public string Key { get; }

    public string DisplayName { get; }

    public IReadOnlyList<string> Aliases { get; }

    public string? Note { get; }
}
