using RA2IniEditor.Infrastructure.FieldRegistry.Fetch;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryRemoteSourceHistoryStoreTests
{
    [Fact]
    public void SaveThenLoad_PreservesEntries()
    {
        using TempRoot temp = new();
        FieldRegistryRemoteSourceHistoryStore store = new();
        FieldRegistryRemoteSourceHistoryEntry entry = CreateEntry("https://raw.githubusercontent.com/owner/repo/main/a.md", "a.md", 10);

        store.Save(temp.Path, new FieldRegistryRemoteSourceHistory([entry]));
        FieldRegistryRemoteSourceHistory loaded = store.Load(temp.Path);

        FieldRegistryRemoteSourceHistoryEntry loadedEntry = Assert.Single(loaded.Entries);
        Assert.Equal(entry.Url, loadedEntry.Url);
        Assert.Equal(entry.ResolvedUrl, loadedEntry.ResolvedUrl);
        Assert.Equal(entry.SourceName, loadedEntry.SourceName);
        Assert.Equal(entry.CachedText, loadedEntry.CachedText);
    }

    [Fact]
    public void AddOrUpdate_DeDuplicatesByResolvedUrlAndKeepsLatest()
    {
        using TempRoot temp = new();
        FieldRegistryRemoteSourceHistoryStore store = new();
        FieldRegistryRemoteSourceHistoryEntry first = CreateEntry("https://github.com/owner/repo/blob/main/a.md", "a.md", 10, "old");
        FieldRegistryRemoteSourceHistoryEntry second = new(
            "https://github.com/owner/repo/blob/main/a.md",
            first.ResolvedUrl,
            "a.md",
            first.FetchedAtUtc.AddMinutes(1),
            20,
            "new");

        store.AddOrUpdate(temp.Path, first);
        store.AddOrUpdate(temp.Path, second);
        FieldRegistryRemoteSourceHistory loaded = store.Load(temp.Path);

        FieldRegistryRemoteSourceHistoryEntry entry = Assert.Single(loaded.Entries);
        Assert.Equal(20, entry.ByteCount);
        Assert.Equal("new", entry.CachedText);
    }

    [Fact]
    public void AddOrUpdate_KeepsOnlyMostRecentTwentyEntries()
    {
        using TempRoot temp = new();
        FieldRegistryRemoteSourceHistoryStore store = new();

        for (int i = 0; i < 25; i++)
            store.AddOrUpdate(temp.Path, CreateEntry($"https://raw.githubusercontent.com/owner/repo/main/{i}.md", $"{i}.md", i));

        FieldRegistryRemoteSourceHistory loaded = store.Load(temp.Path);

        Assert.Equal(20, loaded.Entries.Count);
        Assert.Equal("24.md", loaded.Entries[0].SourceName);
        Assert.DoesNotContain(loaded.Entries, entry => entry.SourceName == "0.md");
    }

    [Fact]
    public void Load_BadHistoryFile_ReturnsEmptyHistoryAndWarning()
    {
        using TempRoot temp = new();
        FieldRegistryRemoteSourceHistoryStore store = new();
        string historyPath = FieldRegistryRemoteSourceHistoryStore.ResolveHistoryFilePath(temp.Path);
        Directory.CreateDirectory(Path.GetDirectoryName(historyPath)!);
        File.WriteAllText(historyPath, "{ not-json");

        FieldRegistryRemoteSourceHistory loaded = store.Load(temp.Path);

        Assert.Empty(loaded.Entries);
        Assert.Contains("Failed to load", store.LastWarning, StringComparison.OrdinalIgnoreCase);
    }

    private static FieldRegistryRemoteSourceHistoryEntry CreateEntry(
        string url,
        string sourceName,
        int byteCount,
        string cachedText = "cached")
    {
        return new FieldRegistryRemoteSourceHistoryEntry(
            url,
            url.Replace("https://github.com/owner/repo/blob/main/", "https://raw.githubusercontent.com/owner/repo/main/"),
            sourceName,
            DateTimeOffset.UtcNow.AddMinutes(byteCount),
            byteCount,
            cachedText);
    }

    private sealed class TempRoot : IDisposable
    {
        public TempRoot()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RA2IniEditor.Tests.RemoteHistory", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
