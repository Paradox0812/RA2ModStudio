using System.Text.Json.Serialization;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

internal sealed class FieldRegistryRemoteSourcePreset
{
    [JsonConstructor]
    public FieldRegistryRemoteSourcePreset(
        string id,
        string name,
        string url,
        string? description,
        IReadOnlyList<string>? tags,
        bool isEnabled,
        string createdAtUtc,
        string updatedAtUtc)
    {
        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id.Trim();
        Name = name?.Trim() ?? string.Empty;
        Url = url?.Trim() ?? string.Empty;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Tags = tags?.Where(tag => !string.IsNullOrWhiteSpace(tag)).Select(tag => tag.Trim()).ToArray() ?? [];
        IsEnabled = isEnabled;
        CreatedAtUtc = string.IsNullOrWhiteSpace(createdAtUtc) ? DateTimeOffset.UtcNow.ToString("O") : createdAtUtc;
        UpdatedAtUtc = string.IsNullOrWhiteSpace(updatedAtUtc) ? CreatedAtUtc : updatedAtUtc;
    }

    public string Id { get; }

    public string Name { get; }

    public string Url { get; }

    public string? Description { get; }

    public IReadOnlyList<string> Tags { get; }

    public bool IsEnabled { get; }

    public string CreatedAtUtc { get; }

    public string UpdatedAtUtc { get; }
}
