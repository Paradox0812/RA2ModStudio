using System.Text.Json.Serialization;

namespace RA2IniEditor.Infrastructure.FieldRegistry;

internal sealed class FieldRegistryPackDto
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("sourceUri")]
    public string? SourceUri { get; set; }

    [JsonPropertyName("sourceRevision")]
    public string? SourceRevision { get; set; }

    [JsonPropertyName("generatedAt")]
    public string? GeneratedAt { get; set; }

    [JsonPropertyName("fields")]
    public List<FieldRegistryFieldDto>? Fields { get; set; }
}

