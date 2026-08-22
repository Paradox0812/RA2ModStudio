using System.Text;
using System.Text.Json;

namespace RA2IniEditor.Infrastructure.FieldRegistry.Fetch;

internal sealed class FieldRegistryRemoteSourceHistoryStore : IFieldRegistryRemoteSourceHistoryStore
{
    public const int MaxEntries = 20;
    public const int MaxCachedTextBytes = FieldRegistryRawFetchRequest.DefaultMaxBytes;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string? LastWarning { get; private set; }

    public FieldRegistryRemoteSourceHistory Load(string globalFieldRegistryRootPath)
    {
        LastWarning = null;
        string historyPath = ResolveHistoryFilePath(globalFieldRegistryRootPath);
        if (!File.Exists(historyPath))
            return new FieldRegistryRemoteSourceHistory();

        try
        {
            string json = File.ReadAllText(historyPath, Encoding.UTF8);
            FieldRegistryRemoteSourceHistory? history = JsonSerializer.Deserialize<FieldRegistryRemoteSourceHistory>(json, JsonOptions);
            return Normalize(history ?? new FieldRegistryRemoteSourceHistory());
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException or ArgumentException)
        {
            LastWarning = $"Failed to load remote source history: {ex.Message}";
            return new FieldRegistryRemoteSourceHistory();
        }
    }

    public void Save(string globalFieldRegistryRootPath, FieldRegistryRemoteSourceHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);
        LastWarning = null;
        string historyPath = ResolveHistoryFilePath(globalFieldRegistryRootPath);
        Directory.CreateDirectory(Path.GetDirectoryName(historyPath)!);
        string json = JsonSerializer.Serialize(Normalize(history), JsonOptions);
        File.WriteAllText(historyPath, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    public void AddOrUpdate(string globalFieldRegistryRootPath, FieldRegistryRemoteSourceHistoryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        FieldRegistryRemoteSourceHistory history = Load(globalFieldRegistryRootPath);
        List<FieldRegistryRemoteSourceHistoryEntry> entries = history.Entries
            .Where(existing => !string.Equals(existing.ResolvedUrl, entry.ResolvedUrl, StringComparison.OrdinalIgnoreCase))
            .ToList();
        entries.Insert(0, NormalizeEntry(entry));
        Save(globalFieldRegistryRootPath, new FieldRegistryRemoteSourceHistory(entries));
    }

    public void Clear(string globalFieldRegistryRootPath)
    {
        LastWarning = null;
        string historyPath = ResolveHistoryFilePath(globalFieldRegistryRootPath);
        Directory.CreateDirectory(Path.GetDirectoryName(historyPath)!);
        File.WriteAllText(historyPath, JsonSerializer.Serialize(new FieldRegistryRemoteSourceHistory(), JsonOptions), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
    }

    public static string ResolveHistoryFilePath(string globalFieldRegistryRootPath)
    {
        if (string.IsNullOrWhiteSpace(globalFieldRegistryRootPath))
            throw new ArgumentException("Global field registry root path cannot be empty.", nameof(globalFieldRegistryRootPath));

        return Path.Combine(globalFieldRegistryRootPath, "remote-sources", "history.json");
    }

    private static FieldRegistryRemoteSourceHistory Normalize(FieldRegistryRemoteSourceHistory history)
    {
        List<FieldRegistryRemoteSourceHistoryEntry> entries = history.Entries
            .Select(NormalizeEntry)
            .GroupBy(entry => entry.ResolvedUrl, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderByDescending(entry => entry.FetchedAtUtc).First())
            .OrderByDescending(entry => entry.FetchedAtUtc)
            .Take(MaxEntries)
            .ToList();
        return new FieldRegistryRemoteSourceHistory(entries);
    }

    private static FieldRegistryRemoteSourceHistoryEntry NormalizeEntry(FieldRegistryRemoteSourceHistoryEntry entry)
    {
        string? cachedText = entry.CachedText;
        if (cachedText is not null && Encoding.UTF8.GetByteCount(cachedText) > MaxCachedTextBytes)
            cachedText = null;

        return new FieldRegistryRemoteSourceHistoryEntry(
            entry.Url,
            entry.ResolvedUrl,
            entry.SourceName,
            entry.FetchedAtUtc,
            entry.ByteCount,
            cachedText);
    }
}
