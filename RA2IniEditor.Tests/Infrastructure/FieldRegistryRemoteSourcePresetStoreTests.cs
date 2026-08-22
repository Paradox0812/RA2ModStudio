using RA2IniEditor.Infrastructure.FieldRegistry.Fetch;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryRemoteSourcePresetStoreTests
{
    [Fact]
    public void SaveThenLoad_PreservesPresets()
    {
        string root = CreateTempRoot();
        FieldRegistryRemoteSourcePresetStore store = new();
        FieldRegistryRemoteSourcePreset preset = CreatePreset("one", "Ares Docs");

        store.Save(root, new FieldRegistryRemoteSourcePresetCollection([preset]));

        FieldRegistryRemoteSourcePreset loaded = Assert.Single(store.Load(root).Presets);
        Assert.Equal("one", loaded.Id);
        Assert.Equal("Ares Docs", loaded.Name);
        Assert.Equal(preset.Url, loaded.Url);
        Assert.Equal(["ares", "docs"], loaded.Tags);
        Assert.True(loaded.IsEnabled);
    }

    [Fact]
    public void AddOrUpdate_SameIdUpdatesAndDifferentIdAdds()
    {
        string root = CreateTempRoot();
        FieldRegistryRemoteSourcePresetStore store = new();

        store.AddOrUpdate(root, CreatePreset("one", "Old"));
        store.AddOrUpdate(root, CreatePreset("one", "New"));
        store.AddOrUpdate(root, CreatePreset("two", "Second"));

        FieldRegistryRemoteSourcePresetCollection loaded = store.Load(root);
        Assert.Equal(2, loaded.Presets.Count);
        Assert.Contains(loaded.Presets, preset => preset.Id == "one" && preset.Name == "New");
        Assert.Contains(loaded.Presets, preset => preset.Id == "two" && preset.Name == "Second");
    }

    [Fact]
    public void Remove_ExistingAndMissingIdsDoNotCrash()
    {
        string root = CreateTempRoot();
        FieldRegistryRemoteSourcePresetStore store = new();
        store.AddOrUpdate(root, CreatePreset("one", "Ares Docs"));

        store.Remove(root, "missing");
        Assert.Single(store.Load(root).Presets);

        store.Remove(root, "one");
        Assert.Empty(store.Load(root).Presets);
    }

    [Fact]
    public void ImportFromFile_MergesUpdatesAndSkipsInvalidUrls()
    {
        string root = CreateTempRoot();
        string importPath = Path.Combine(CreateTempRoot(), "import.json");
        FieldRegistryRemoteSourcePresetStore store = new();
        store.AddOrUpdate(root, CreatePreset("one", "Old"));
        File.WriteAllText(importPath, """
            {
              "presets": [
                {
                  "id": "one",
                  "name": "Updated",
                  "url": "https://github.com/owner/repo/blob/main/updated.md",
                  "description": "updated",
                  "tags": ["a"],
                  "isEnabled": true,
                  "createdAtUtc": "2026-01-01T00:00:00.0000000+00:00",
                  "updatedAtUtc": "2026-01-02T00:00:00.0000000+00:00"
                },
                {
                  "id": "two",
                  "name": "Invalid",
                  "url": "https://example.com/not-supported.md",
                  "description": null,
                  "tags": [],
                  "isEnabled": true,
                  "createdAtUtc": "2026-01-01T00:00:00.0000000+00:00",
                  "updatedAtUtc": "2026-01-02T00:00:00.0000000+00:00"
                }
              ]
            }
            """);

        store.ImportFromFile(root, importPath, replaceExisting: false);
        string? importWarning = store.LastWarning;

        FieldRegistryRemoteSourcePreset preset = Assert.Single(store.Load(root).Presets);
        Assert.Equal("one", preset.Id);
        Assert.Equal("Updated", preset.Name);
        Assert.Contains("Skipped 1 invalid", importWarning, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ExportToFile_WritesReadablePresetsJson()
    {
        string root = CreateTempRoot();
        string targetRoot = CreateTempRoot();
        string exportPath = Path.Combine(targetRoot, "export.json");
        FieldRegistryRemoteSourcePresetStore store = new();
        store.AddOrUpdate(root, CreatePreset("one", "Ares Docs"));

        store.ExportToFile(root, exportPath);
        store.ImportFromFile(targetRoot, exportPath, replaceExisting: true);

        FieldRegistryRemoteSourcePreset loaded = Assert.Single(store.Load(targetRoot).Presets);
        Assert.Equal("Ares Docs", loaded.Name);
    }

    [Fact]
    public void Load_InvalidJsonReturnsEmptyAndWarning()
    {
        string root = CreateTempRoot();
        string presetsPath = FieldRegistryRemoteSourcePresetStore.ResolvePresetsFilePath(root);
        Directory.CreateDirectory(Path.GetDirectoryName(presetsPath)!);
        File.WriteAllText(presetsPath, "{ broken json");
        FieldRegistryRemoteSourcePresetStore store = new();

        FieldRegistryRemoteSourcePresetCollection loaded = store.Load(root);

        Assert.Empty(loaded.Presets);
        Assert.Contains("Failed to load", store.LastWarning, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("https://example.com/fields.md")]
    [InlineData("http://raw.githubusercontent.com/owner/repo/main/fields.md")]
    [InlineData("https://github.com/owner/repo/tree/main/docs")]
    public void Save_RejectsInvalidPresetUrls(string url)
    {
        string root = CreateTempRoot();
        FieldRegistryRemoteSourcePresetStore store = new();
        FieldRegistryRemoteSourcePreset preset = CreatePreset("one", "Invalid", url);

        Assert.ThrowsAny<Exception>(() => store.Save(root, new FieldRegistryRemoteSourcePresetCollection([preset])));
    }

    private static FieldRegistryRemoteSourcePreset CreatePreset(
        string id,
        string name,
        string url = "https://github.com/owner/repo/blob/main/fields.md")
        => new(
            id,
            name,
            url,
            "Field docs",
            ["ares", "docs"],
            true,
            "2026-01-01T00:00:00.0000000+00:00",
            "2026-01-02T00:00:00.0000000+00:00");

    private static string CreateTempRoot()
    {
        string path = Path.Combine(Path.GetTempPath(), "RA2IniEditor.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }
}
