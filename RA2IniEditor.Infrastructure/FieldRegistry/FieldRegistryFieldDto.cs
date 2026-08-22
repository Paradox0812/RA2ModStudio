using System.Text.Json.Serialization;

namespace RA2IniEditor.Infrastructure.FieldRegistry;

internal sealed class FieldRegistryFieldDto
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("appliesTo")]
    public List<string>? AppliesTo { get; set; }

    [JsonPropertyName("editorKind")]
    public string? EditorKind { get; set; }

    [JsonPropertyName("sourceKind")]
    public string? SourceKind { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("quality")]
    public string? Quality { get; set; }

    [JsonPropertyName("aliases")]
    public List<string>? Aliases { get; set; }

    [JsonPropertyName("examples")]
    public List<FieldRegistryExampleDto>? Examples { get; set; }

    [JsonPropertyName("schema")]
    public FieldRegistryValueSchemaDto? Schema { get; set; }
}

internal sealed class FieldRegistryExampleDto
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

internal sealed class FieldRegistryValueSchemaDto
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("booleanStyle")]
    public string? BooleanStyle { get; set; }

    [JsonPropertyName("allowedValues")]
    public List<FieldRegistryAllowedValueDto>? AllowedValues { get; set; }

    [JsonPropertyName("enumName")]
    public string? EnumName { get; set; }

    [JsonPropertyName("separator")]
    public string? Separator { get; set; }
}

internal sealed class FieldRegistryAllowedValueDto
{
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("priority")]
    public int? Priority { get; set; }
}
