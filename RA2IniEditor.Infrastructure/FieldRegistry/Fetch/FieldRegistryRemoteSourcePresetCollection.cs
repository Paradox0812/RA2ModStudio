using System.Text.Json.Serialization;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

internal sealed class FieldRegistryRemoteSourcePresetCollection
{
    public FieldRegistryRemoteSourcePresetCollection()
        : this([])
    {
    }

    [JsonConstructor]
    public FieldRegistryRemoteSourcePresetCollection(IReadOnlyList<FieldRegistryRemoteSourcePreset>? presets)
    {
        Presets = presets ?? [];
    }

    public IReadOnlyList<FieldRegistryRemoteSourcePreset> Presets { get; }
}
