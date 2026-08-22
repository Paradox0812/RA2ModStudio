using System.Text.Json.Serialization;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

internal sealed class FieldRegistryRemoteSourceHistory
{
    public FieldRegistryRemoteSourceHistory()
        : this([])
    {
    }

    [JsonConstructor]
    public FieldRegistryRemoteSourceHistory(IReadOnlyList<FieldRegistryRemoteSourceHistoryEntry> entries)
    {
        Entries = entries ?? [];
    }

    public IReadOnlyList<FieldRegistryRemoteSourceHistoryEntry> Entries { get; }
}
