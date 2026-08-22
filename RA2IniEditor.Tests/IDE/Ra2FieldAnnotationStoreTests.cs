using RA2IniEditor.IDE.FieldAnnotations;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FieldAnnotationStoreTests
{
    [Fact]
    public void Load_ReadsSidecarJsonAndPreservesAliasesAndNote()
    {
        using TempDirectory temp = new();
        string path = temp.Write("field-annotations.zh-CN.json", """
            {
              "version": 1,
              "language": "zh-CN",
              "entries": [
                {
                  "sectionKind": "VehicleType",
                  "key": "Strength",
                  "displayName": "Health",
                  "aliases": ["HP", "Durability"],
                  "note": "Maximum hit points."
                }
              ]
            }
            """);

        Ra2FieldAnnotationLoadResult result = new Ra2FieldAnnotationJsonStore().Load(path);

        Assert.True(result.Success);
        Ra2FieldAnnotationEntry entry = Assert.Single(result.Pack.Entries);
        Assert.Equal("VehicleType", entry.SectionKind);
        Assert.Equal("Strength", entry.Key);
        Assert.Equal("Health", entry.DisplayName);
        Assert.Equal(["HP", "Durability"], entry.Aliases);
        Assert.Equal("Maximum hit points.", entry.Note);
    }

    [Fact]
    public void Load_BadJsonReturnsControlledFailure()
    {
        using TempDirectory temp = new();
        string path = temp.Write("field-annotations.zh-CN.json", "{ bad json");

        Ra2FieldAnnotationLoadResult result = new Ra2FieldAnnotationJsonStore().Load(path);

        Assert.False(result.Success);
        Assert.Empty(result.Pack.Entries);
        Assert.NotEmpty(result.Warnings);
    }

    [Fact]
    public void Load_DuplicateEntriesProducesWarning()
    {
        using TempDirectory temp = new();
        string path = temp.Write("field-annotations.zh-CN.json", """
            {
              "version": 1,
              "language": "zh-CN",
              "entries": [
                { "sectionKind": "Vehicle", "key": "Strength", "displayName": "Old" },
                { "sectionKind": "Vehicle", "key": "Strength", "displayName": "New" }
              ]
            }
            """);

        Ra2FieldAnnotationLoadResult result = new Ra2FieldAnnotationJsonStore().Load(path);

        Assert.True(result.Success);
        Assert.Contains(result.Warnings, warning => warning.Contains("Duplicate", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Save_WritesOnlyRequestedSidecarPath()
    {
        using TempDirectory temp = new();
        string sidecarPath = Path.Combine(temp.Path, ".ra2ide", "field-annotations.zh-CN.json");
        string iniPath = temp.Write("rulesmd.ini", "[HTNK]\nStrength=400");
        Ra2FieldAnnotationPack pack = new(1, "zh-CN", [
            new Ra2FieldAnnotationEntry("Vehicle", "Strength", "Health", ["HP"], "Maximum hit points.")
        ]);

        Ra2FieldAnnotationSaveResult result = new Ra2FieldAnnotationJsonStore().Save(sidecarPath, pack);

        Assert.True(result.Success);
        Assert.True(File.Exists(sidecarPath));
        Assert.Equal("[HTNK]\nStrength=400", File.ReadAllText(iniPath));
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "RA2IniEditor.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string Write(string fileName, string text)
        {
            string path = System.IO.Path.Combine(Path, fileName);
            string? directory = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(path, text);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
