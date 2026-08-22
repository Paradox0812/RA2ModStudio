using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.IDE.FieldAnnotations;

internal sealed class Ra2FieldDisplayResolver : IRa2FieldDisplayResolver
{
    private readonly IRa2FieldDefinitionProvider _fieldDefinitionProvider;
    private readonly IRa2FieldAnnotationProvider _annotationProvider;

    public Ra2FieldDisplayResolver(
        IRa2FieldDefinitionProvider fieldDefinitionProvider,
        IRa2FieldAnnotationProvider annotationProvider)
    {
        _fieldDefinitionProvider = fieldDefinitionProvider ?? throw new ArgumentNullException(nameof(fieldDefinitionProvider));
        _annotationProvider = annotationProvider ?? throw new ArgumentNullException(nameof(annotationProvider));
    }

    public Ra2FieldDisplayInfo Resolve(Ra2SectionKind sectionKind, string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return CreateRawFallback(string.Empty);

        _fieldDefinitionProvider.TryGetField(sectionKind, key, out Ra2FieldDefinition? definition);
        Ra2FieldAnnotationEntry? annotation = _annotationProvider.Find(sectionKind, definition?.Key ?? key);
        string rawKey = definition?.Key ?? key.Trim();
        string displayName = !string.IsNullOrWhiteSpace(annotation?.DisplayName)
            ? annotation.DisplayName
            : rawKey;
        string? description = definition?.Description;
        string typeDisplay = definition?.EditorKind.ToString() ?? "Unknown";
        string appliesToDisplay = definition is null || definition.AppliesTo.Count == 0
            ? "Common"
            : string.Join(", ", definition.AppliesTo);
        string sourceDisplay = definition?.SourceKind.ToString() ?? "Unknown";

        return new Ra2FieldDisplayInfo(
            rawKey,
            displayName,
            annotation?.Aliases ?? [],
            annotation?.Note,
            description,
            typeDisplay,
            appliesToDisplay,
            sourceDisplay,
            annotation is not null,
            definition);
    }

    public IReadOnlyList<Ra2FieldDisplayInfo> GetFields(Ra2SectionKind sectionKind)
        => _fieldDefinitionProvider
            .GetFields(sectionKind)
            .Select(definition => Resolve(sectionKind, definition.Key))
            .OrderBy(info => info.Key, StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static Ra2FieldDisplayInfo CreateRawFallback(string key)
        => new(key, key, [], null, null, "Unknown", "Unknown", "Unknown", hasUserAnnotation: false);
}
