using System.Text.Json;
using RA2IniEditor.Core.Schema;

namespace RA2IniEditor.Infrastructure.FieldRegistry;

/// <summary>
/// Loads local active RA2 field registry packs from disk.
/// </summary>
public sealed class LocalFieldRegistryLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <summary>
    /// Loads all <c>*.fields.json</c> packs from an active field registry directory.
    /// </summary>
    public LocalFieldRegistryLoadResult LoadDirectory(string activeDirectoryPath)
    {
        List<Ra2FieldDefinition> definitions = new();
        List<LocalFieldRegistryLoadedDefinition> loadedDefinitions = new();
        List<string> warnings = new();

        if (string.IsNullOrWhiteSpace(activeDirectoryPath) || !Directory.Exists(activeDirectoryPath))
            return CreateResult(definitions, warnings, loadedDefinitions);

        string[] files;
        try
        {
            files = Directory.GetFiles(activeDirectoryPath, "*.fields.json", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            warnings.Add($"Failed to enumerate field registry directory '{activeDirectoryPath}': {ex.Message}");
            return CreateResult(definitions, warnings, loadedDefinitions);
        }

        foreach (string filePath in files)
            LoadFile(filePath, definitions, loadedDefinitions, warnings);

        return CreateResult(definitions, warnings, loadedDefinitions);
    }

    internal LocalFieldRegistryLoadResult LoadJson(string json, string sourceName)
    {
        List<Ra2FieldDefinition> definitions = new();
        List<LocalFieldRegistryLoadedDefinition> loadedDefinitions = new();
        List<string> warnings = new();

        if (string.IsNullOrWhiteSpace(json))
        {
            warnings.Add($"Field registry source '{sourceName}' is empty.");
            return CreateResult(definitions, warnings, loadedDefinitions);
        }

        FieldRegistryPackDto? pack;
        try
        {
            pack = JsonSerializer.Deserialize<FieldRegistryPackDto>(json, JsonOptions);
        }
        catch (JsonException ex)
        {
            warnings.Add($"Failed to load field registry source '{sourceName}': {ex.Message}");
            return CreateResult(definitions, warnings, loadedDefinitions);
        }

        LoadPack(pack, sourceName, sourceName, definitions, loadedDefinitions, warnings);
        return CreateResult(definitions, warnings, loadedDefinitions);
    }

    private static void LoadFile(
        string filePath,
        List<Ra2FieldDefinition> definitions,
        List<LocalFieldRegistryLoadedDefinition> loadedDefinitions,
        List<string> warnings)
    {
        FieldRegistryPackDto? pack;
        try
        {
            string json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                warnings.Add($"Field registry file '{filePath}' is empty.");
                return;
            }

            pack = JsonSerializer.Deserialize<FieldRegistryPackDto>(json, JsonOptions);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            warnings.Add($"Failed to load field registry file '{filePath}': {ex.Message}");
            return;
        }

        LoadPack(pack, filePath, filePath, definitions, loadedDefinitions, warnings);
    }

    private static void LoadPack(
        FieldRegistryPackDto? pack,
        string sourceFileName,
        string sourceFilePath,
        List<Ra2FieldDefinition> definitions,
        List<LocalFieldRegistryLoadedDefinition> loadedDefinitions,
        List<string> warnings)
    {
        if (pack?.Fields is null || pack.Fields.Count == 0)
            return;

        for (int index = 0; index < pack.Fields.Count; index++)
        {
            FieldRegistryFieldDto field = pack.Fields[index];
            Ra2FieldDefinition? definition = TryCreateDefinition(field, sourceFilePath, index, warnings);
            if (definition is not null)
            {
                definitions.Add(definition);
                loadedDefinitions.Add(new LocalFieldRegistryLoadedDefinition(
                    definition,
                    Path.GetFileName(sourceFileName),
                    sourceFilePath));
            }
        }
    }

    private static Ra2FieldDefinition? TryCreateDefinition(
        FieldRegistryFieldDto field,
        string filePath,
        int index,
        List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(field.Key))
        {
            warnings.Add($"Skipped field #{index + 1} in '{filePath}' because key is missing.");
            return null;
        }

        FieldEditorKind editorKind = ParseEnumOrDefault(field.EditorKind, FieldEditorKind.Text);
        Ra2FieldSourceKind sourceKind = ParseEnumOrDefault(field.SourceKind, Ra2FieldSourceKind.External);
        IReadOnlyList<Ra2SectionKind>? appliesTo = ParseAppliesTo(field.AppliesTo, filePath, field.Key, warnings);
        if (appliesTo is null)
            return null;

        Ra2FieldValueMetadata valueMetadata = ParseValueMetadata(field.Schema, filePath, field.Key, warnings);
        IReadOnlyList<Ra2FieldExample> examples = ParseExamples(field.Examples, filePath, field.Key, warnings);

        try
        {
            return new Ra2FieldDefinition(
                field.Key,
                appliesTo,
                editorKind,
                sourceKind,
                field.Description,
                valueMetadata,
                field.DisplayName,
                ParseAliases(field.Aliases),
                examples,
                field.Quality);
        }
        catch (ArgumentException ex)
        {
            warnings.Add($"Skipped field '{field.Key}' in '{filePath}': {ex.Message}");
            return null;
        }
    }

    private static IReadOnlyList<Ra2SectionKind>? ParseAppliesTo(
        IReadOnlyList<string>? values,
        string filePath,
        string key,
        List<string> warnings)
    {
        if (values is null || values.Count == 0)
            return Array.AsReadOnly([Ra2SectionKind.Unknown]);

        List<Ra2SectionKind> result = new();
        foreach (string value in values)
        {
            if (Ra2FieldAppliesToNormalizer.TryNormalize(value, out IReadOnlyList<Ra2SectionKind> normalizedKinds, out string? warning))
            {
                foreach (Ra2SectionKind kind in normalizedKinds)
                {
                    if (!result.Contains(kind))
                        result.Add(kind);
                }

                continue;
            }

            warnings.Add($"Field '{key}' in '{filePath}' has {warning}");
        }

        if (result.Count > 0)
            return Array.AsReadOnly(result.ToArray());

        warnings.Add($"Skipped field '{key}' in '{filePath}' because none of its appliesTo values are supported.");
        return null;
    }

    private static Ra2FieldValueMetadata ParseValueMetadata(
        FieldRegistryValueSchemaDto? schema,
        string filePath,
        string key,
        List<string> warnings)
    {
        if (schema is null)
            return Ra2FieldValueMetadata.Unknown;

        Ra2FieldValueKind valueKind = ParseValueKind(schema.Type, filePath, key, warnings);
        Ra2FieldBooleanValueStyle booleanStyle = ParseBooleanStyle(schema.BooleanStyle, filePath, key, warnings);
        IReadOnlyList<Ra2FieldAllowedValue> allowedValues = ParseAllowedValues(schema.AllowedValues, filePath, key, warnings);

        if (valueKind == Ra2FieldValueKind.Boolean &&
            booleanStyle == Ra2FieldBooleanValueStyle.Custom &&
            allowedValues.Count == 0)
        {
            warnings.Add($"Field '{key}' in '{filePath}' declares custom booleanStyle without allowedValues.");
        }

        string separator = string.IsNullOrEmpty(schema.Separator) ? "," : schema.Separator;
        return new Ra2FieldValueMetadata(
            valueKind,
            booleanStyle,
            allowedValues,
            schema.EnumName,
            separator);
    }

    private static Ra2FieldValueKind ParseValueKind(
        string? value,
        string filePath,
        string key,
        List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Ra2FieldValueKind.Unknown;

        if (Enum.TryParse(value, ignoreCase: true, out Ra2FieldValueKind result))
            return result;

        warnings.Add($"Field '{key}' in '{filePath}' has unknown schema type '{value}', mapped to Unknown.");
        return Ra2FieldValueKind.Unknown;
    }

    private static Ra2FieldBooleanValueStyle ParseBooleanStyle(
        string? value,
        string filePath,
        string key,
        List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Ra2FieldBooleanValueStyle.Unknown;

        if (Enum.TryParse(value, ignoreCase: true, out Ra2FieldBooleanValueStyle result))
            return result;

        warnings.Add($"Field '{key}' in '{filePath}' has unknown booleanStyle value '{value}', mapped to Unknown.");
        return Ra2FieldBooleanValueStyle.Unknown;
    }

    private static IReadOnlyList<Ra2FieldAllowedValue> ParseAllowedValues(
        IReadOnlyList<FieldRegistryAllowedValueDto>? values,
        string filePath,
        string key,
        List<string> warnings)
    {
        if (values is null || values.Count == 0)
            return Array.AsReadOnly(Array.Empty<Ra2FieldAllowedValue>());

        List<Ra2FieldAllowedValue> result = new();
        for (int index = 0; index < values.Count; index++)
        {
            FieldRegistryAllowedValueDto value = values[index];
            if (string.IsNullOrWhiteSpace(value.Value))
            {
                warnings.Add($"Field '{key}' in '{filePath}' has an allowedValues entry #{index + 1} without value.");
                continue;
            }

            result.Add(new Ra2FieldAllowedValue(
                value.Value,
                value.DisplayName,
                value.Description,
                value.Priority ?? 0));
        }

        return Array.AsReadOnly(result.ToArray());
    }

    private static IReadOnlyList<Ra2FieldExample> ParseExamples(
        IReadOnlyList<FieldRegistryExampleDto>? examples,
        string filePath,
        string key,
        List<string> warnings)
    {
        if (examples is null || examples.Count == 0)
            return Array.AsReadOnly(Array.Empty<Ra2FieldExample>());

        List<Ra2FieldExample> result = new();
        HashSet<string> seenValues = new(StringComparer.OrdinalIgnoreCase);
        for (int index = 0; index < examples.Count; index++)
        {
            FieldRegistryExampleDto example = examples[index];
            if (string.IsNullOrWhiteSpace(example.Value))
            {
                warnings.Add($"Field '{key}' in '{filePath}' has an examples entry #{index + 1} without value.");
                continue;
            }

            string value = example.Value.Trim();
            if (!seenValues.Add(value))
                continue;

            result.Add(new Ra2FieldExample(value, example.Description));
        }

        return Array.AsReadOnly(result.ToArray());
    }

    private static IReadOnlyList<string> ParseAliases(IReadOnlyList<string>? aliases)
    {
        if (aliases is null || aliases.Count == 0)
            return Array.AsReadOnly(Array.Empty<string>());

        return Array.AsReadOnly(aliases
            .Where(alias => !string.IsNullOrWhiteSpace(alias))
            .Select(alias => alias.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray());
    }

    private static TEnum ParseEnumOrDefault<TEnum>(string? value, TEnum defaultValue)
        where TEnum : struct
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return Enum.TryParse(value, ignoreCase: true, out TEnum result) ? result : defaultValue;
    }

    private static LocalFieldRegistryLoadResult CreateResult(
        IReadOnlyList<Ra2FieldDefinition> definitions,
        IReadOnlyList<string> warnings,
        IReadOnlyList<LocalFieldRegistryLoadedDefinition> loadedDefinitions)
    {
        return new LocalFieldRegistryLoadResult(
            Array.AsReadOnly(definitions.ToArray()),
            Array.AsReadOnly(warnings.ToArray()),
            Array.AsReadOnly(loadedDefinitions.ToArray()));
    }
}
