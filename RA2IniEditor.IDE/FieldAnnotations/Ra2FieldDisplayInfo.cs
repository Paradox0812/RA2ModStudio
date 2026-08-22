using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.FieldAnnotations;

internal sealed class Ra2FieldDisplayInfo
{
    public Ra2FieldDisplayInfo(
        string key,
        string displayName,
        IReadOnlyList<string>? aliases,
        string? note,
        string? description,
        string typeDisplay,
        string appliesToDisplay,
        string sourceDisplay,
        bool hasUserAnnotation,
        Ra2FieldDefinition? definition = null)
    {
        Key = key;
        DisplayName = displayName;
        Aliases = aliases ?? [];
        Note = note;
        Description = description;
        TypeDisplay = typeDisplay;
        AppliesToDisplay = appliesToDisplay;
        SourceDisplay = sourceDisplay;
        HasUserAnnotation = hasUserAnnotation;
        Definition = definition;
    }

    public string Key { get; }

    public string DisplayName { get; }

    public IReadOnlyList<string> Aliases { get; }

    public string? Note { get; }

    public string? Description { get; }

    public string TypeDisplay { get; }

    public string AppliesToDisplay { get; }

    public string SourceDisplay { get; }

    public bool HasUserAnnotation { get; }

    public Ra2FieldDefinition? Definition { get; }
}
