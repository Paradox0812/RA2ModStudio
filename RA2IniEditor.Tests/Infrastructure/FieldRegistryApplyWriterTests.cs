using System.Text.Json;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryApplyWriterTests
{
    private static readonly DateTimeOffset Timestamp = new(2026, 5, 25, 12, 34, 56, TimeSpan.Zero);

    [Fact]
    public void WriteCreatesNewProjectPackAndManifest()
    {
        using TempDirectory temp = TempDirectory.Create();
        FieldRegistryApplyPlan plan = BuildPlan(
            [Definition("MyNewKey", Ra2SectionKind.Infantry)],
            [Row("MyNewKey", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Added)]);
        FieldRegistryApplyWriter writer = new();

        FieldRegistryApplyWriteResult result = writer.Write(new FieldRegistryApplyWriteRequest(
            plan,
            temp.ProjectRootPath,
            temp.GlobalRootPath,
            timestamp: Timestamp));

        Assert.True(File.Exists(result.TargetFilePath));
        Assert.NotNull(result.BackupDirectoryPath);
        Assert.NotNull(result.ManifestFilePath);
        Assert.True(File.Exists(result.ManifestFilePath));
        Assert.Equal(1, result.AddedCount);
        Assert.Equal(0, result.UpdatedCount);
        LocalFieldRegistryLoadResult loaded = new LocalFieldRegistryLoader().LoadDirectory(Path.GetDirectoryName(result.TargetFilePath)!);
        Ra2FieldDefinition definition = Assert.Single(loaded.Definitions);
        Assert.Equal("MyNewKey", definition.Key);
        Assert.Equal([Ra2SectionKind.Infantry], definition.AppliesTo);

        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(result.ManifestFilePath));
        Assert.False(manifest.RootElement.GetProperty("targetFileExisted").GetBoolean());
        Assert.Equal("Project", manifest.RootElement.GetProperty("targetScope").GetString());
    }

    [Fact]
    public void WritePreservesValueMetadataSchemaInActivePack()
    {
        using TempDirectory temp = TempDirectory.Create();
        Ra2FieldDefinition definition = new(
            "CustomBoolean",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Boolean,
            Ra2FieldSourceKind.User,
            "Custom boolean.",
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Boolean, Ra2FieldBooleanValueStyle.YesNo));
        FieldRegistryApplyPlan plan = BuildPlan(
            [definition],
            [Row("CustomBoolean", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Added)]);

        FieldRegistryApplyWriteResult result = new FieldRegistryApplyWriter().Write(new FieldRegistryApplyWriteRequest(
            plan,
            temp.ProjectRootPath,
            temp.GlobalRootPath,
            timestamp: Timestamp));

        string json = File.ReadAllText(result.TargetFilePath);
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement schema = document.RootElement
            .GetProperty("fields")[0]
            .GetProperty("schema");
        Assert.Equal("Boolean", schema.GetProperty("type").GetString());
        Assert.Equal("YesNo", schema.GetProperty("booleanStyle").GetString());

        Ra2FieldDefinition loaded = Assert.Single(new LocalFieldRegistryLoader()
            .LoadDirectory(Path.GetDirectoryName(result.TargetFilePath)!)
            .Definitions);
        Assert.Equal(Ra2FieldValueKind.Boolean, loaded.ValueMetadata.ValueKind);
        Assert.Equal(Ra2FieldBooleanValueStyle.YesNo, loaded.ValueMetadata.BooleanStyle);
    }

    [Fact]
    public void WritePreservesDisplayNameAndAliasesInActivePack()
    {
        using TempDirectory temp = TempDirectory.Create();
        Ra2FieldDefinition definition = new(
            "Strength",
            [Ra2SectionKind.Vehicle],
            FieldEditorKind.Integer,
            Ra2FieldSourceKind.User,
            "Hit points.",
            displayName: "Health",
            aliases: ["HP", "Durability"]);
        FieldRegistryApplyPlan plan = BuildPlan(
            [definition],
            [Row("Strength", Ra2SectionKind.Vehicle, FieldRegistryHarvestDiffKind.Added)]);

        FieldRegistryApplyWriteResult result = new FieldRegistryApplyWriter().Write(new FieldRegistryApplyWriteRequest(
            plan,
            temp.ProjectRootPath,
            temp.GlobalRootPath,
            timestamp: Timestamp));

        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(result.TargetFilePath));
        JsonElement field = document.RootElement.GetProperty("fields")[0];
        Assert.Equal("Health", field.GetProperty("displayName").GetString());
        Assert.Equal(["HP", "Durability"], field.GetProperty("aliases").EnumerateArray().Select(alias => alias.GetString() ?? string.Empty).ToArray());

        Ra2FieldDefinition loaded = Assert.Single(new LocalFieldRegistryLoader()
            .LoadDirectory(Path.GetDirectoryName(result.TargetFilePath)!)
            .Definitions);
        Assert.Equal("Health", loaded.DisplayName);
        Assert.Equal(["HP", "Durability"], loaded.Aliases);
    }

    [Fact]
    public void WriteUpdatesExistingPackAndBacksUpOldContent()
    {
        using TempDirectory temp = TempDirectory.Create();
        string activeDirectory = Path.Combine(temp.ProjectRootPath, ".ra2inieditor", "field-registry", "active");
        Directory.CreateDirectory(activeDirectory);
        string targetPath = Path.Combine(activeDirectory, FieldRegistryApplyWriteRequest.DefaultTargetPackFileName);
        File.WriteAllText(targetPath, """
            {
              "name": "Existing",
              "kind": "User",
              "version": "old",
              "fields": [
                {
                  "key": "Owner",
                  "appliesTo": ["Infantry"],
                  "editorKind": "Text",
                  "sourceKind": "User",
                  "description": "old description"
                }
              ]
            }
            """);
        FieldRegistryApplyPlan plan = BuildPlan(
            [Definition("Owner", Ra2SectionKind.Infantry, FieldEditorKind.Float, "new description")],
            [Row("Owner", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Changed, FieldRegistryProvenanceScope.Project, "project.fields.json")],
            FieldRegistryApplyTargetScope.Project,
            FieldRegistryApplyMode.AppendOrUpdate);

        FieldRegistryApplyWriteResult result = new FieldRegistryApplyWriter().Write(new FieldRegistryApplyWriteRequest(
            plan,
            temp.ProjectRootPath,
            temp.GlobalRootPath,
            timestamp: Timestamp));

        Assert.Equal(0, result.AddedCount);
        Assert.Equal(1, result.UpdatedCount);
        Assert.NotNull(result.BackupDirectoryPath);
        string backupFile = Path.Combine(result.BackupDirectoryPath, FieldRegistryApplyWriteRequest.DefaultTargetPackFileName);
        Assert.True(File.Exists(backupFile));
        Assert.Contains("old description", File.ReadAllText(backupFile), StringComparison.Ordinal);
        LocalFieldRegistryLoadResult loaded = new LocalFieldRegistryLoader().LoadDirectory(activeDirectory);
        Ra2FieldDefinition definition = Assert.Single(loaded.Definitions);
        Assert.Equal(FieldEditorKind.Float, definition.EditorKind);
        Assert.Equal("new description", definition.Description);
        using JsonDocument manifest = JsonDocument.Parse(File.ReadAllText(result.ManifestFilePath!));
        Assert.True(manifest.RootElement.GetProperty("targetFileExisted").GetBoolean());
    }

    [Fact]
    public void WriteUpdateRemovesStaleSchemaWhenPreviewHasNoValueMetadata()
    {
        using TempDirectory temp = TempDirectory.Create();
        string activeDirectory = Path.Combine(temp.ProjectRootPath, ".ra2inieditor", "field-registry", "active");
        Directory.CreateDirectory(activeDirectory);
        string targetPath = Path.Combine(activeDirectory, FieldRegistryApplyWriteRequest.DefaultTargetPackFileName);
        File.WriteAllText(targetPath, """
            {
              "fields": [
                {
                  "key": "Owner",
                  "appliesTo": ["Infantry"],
                  "editorKind": "Boolean",
                  "sourceKind": "User",
                  "schema": {
                    "type": "Boolean",
                    "booleanStyle": "YesNo"
                  }
                }
              ]
            }
            """);
        FieldRegistryApplyPlan plan = BuildPlan(
            [Definition("Owner", Ra2SectionKind.Infantry, FieldEditorKind.Text, "plain text now")],
            [Row("Owner", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Changed, FieldRegistryProvenanceScope.Project, "project.fields.json")]);

        FieldRegistryApplyWriteResult result = new FieldRegistryApplyWriter().Write(new FieldRegistryApplyWriteRequest(
            plan,
            temp.ProjectRootPath,
            temp.GlobalRootPath,
            timestamp: Timestamp));

        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(result.TargetFilePath));
        JsonElement field = document.RootElement.GetProperty("fields")[0];
        Assert.False(field.TryGetProperty("schema", out _));
    }

    [Fact]
    public void WriteAddPlusUpdateReturnsActualCounts()
    {
        using TempDirectory temp = TempDirectory.Create();
        string activeDirectory = Path.Combine(temp.ProjectRootPath, ".ra2inieditor", "field-registry", "active");
        Directory.CreateDirectory(activeDirectory);
        File.WriteAllText(Path.Combine(activeDirectory, FieldRegistryApplyWriteRequest.DefaultTargetPackFileName), """
            {
              "fields": [
                { "key": "Owner", "appliesTo": ["Infantry"], "editorKind": "Text", "sourceKind": "User" }
              ]
            }
            """);
        FieldRegistryApplyPlan plan = BuildPlan(
            [
                Definition("Owner", Ra2SectionKind.Infantry, FieldEditorKind.Float),
                Definition("MyNewKey", Ra2SectionKind.Infantry)
            ],
            [
                Row("Owner", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Changed, FieldRegistryProvenanceScope.Project, "project.fields.json"),
                Row("MyNewKey", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Added)
            ],
            FieldRegistryApplyTargetScope.Project,
            FieldRegistryApplyMode.AppendOrUpdate);

        FieldRegistryApplyWriteResult result = new FieldRegistryApplyWriter().Write(new FieldRegistryApplyWriteRequest(
            plan,
            temp.ProjectRootPath,
            temp.GlobalRootPath,
            timestamp: Timestamp));

        Assert.Equal(1, result.AddedCount);
        Assert.Equal(1, result.UpdatedCount);
        Assert.Equal(["MyNewKey", "Owner"], new LocalFieldRegistryLoader()
            .LoadDirectory(activeDirectory)
            .Definitions
            .Select(definition => definition.Key)
            .OrderBy(key => key)
            .ToArray());
    }

    [Fact]
    public void RejectPlanBlocksWrite()
    {
        using TempDirectory temp = TempDirectory.Create();
        FieldRegistryApplyPlan plan = BuildPlan(
            [],
            [Row("BadKey", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Invalid)]);

        Assert.Throws<InvalidOperationException>(() => new FieldRegistryApplyWriter().Write(new FieldRegistryApplyWriteRequest(
            plan,
            temp.ProjectRootPath,
            temp.GlobalRootPath,
            timestamp: Timestamp)));

        Assert.False(Directory.Exists(Path.Combine(temp.ProjectRootPath, ".ra2inieditor")));
    }

    [Fact]
    public void ProjectTargetWithoutProjectRootBlocksWrite()
    {
        using TempDirectory temp = TempDirectory.Create();
        FieldRegistryApplyPlan plan = BuildPlan(
            [Definition("MyNewKey", Ra2SectionKind.Infantry)],
            [Row("MyNewKey", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Added)]);

        Assert.Throws<InvalidOperationException>(() => new FieldRegistryApplyWriter().Write(new FieldRegistryApplyWriteRequest(
            plan,
            null,
            temp.GlobalRootPath,
            timestamp: Timestamp)));
    }

    [Theory]
    [InlineData("../bad.fields.json")]
    [InlineData("folder/bad.fields.json")]
    public void TargetPackFileNameRejectsPathTraversal(string fileName)
    {
        using TempDirectory temp = TempDirectory.Create();
        FieldRegistryApplyPlan plan = BuildPlan(
            [Definition("MyNewKey", Ra2SectionKind.Infantry)],
            [Row("MyNewKey", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Added)]);

        Assert.Throws<ArgumentException>(() => new FieldRegistryApplyWriter().Write(new FieldRegistryApplyWriteRequest(
            plan,
            temp.ProjectRootPath,
            temp.GlobalRootPath,
            fileName,
            Timestamp)));
    }

    [Fact]
    public void AllSkipDoesNotWriteOrCreateBackup()
    {
        using TempDirectory temp = TempDirectory.Create();
        FieldRegistryApplyPlan plan = BuildPlan(
            [],
            [Row("Owner", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Same, FieldRegistryProvenanceScope.BuiltIn, "BuiltIn")]);

        FieldRegistryApplyWriteResult result = new FieldRegistryApplyWriter().Write(new FieldRegistryApplyWriteRequest(
            plan,
            temp.ProjectRootPath,
            temp.GlobalRootPath,
            timestamp: Timestamp));

        Assert.Equal(1, result.SkippedCount);
        Assert.Null(result.BackupDirectoryPath);
        Assert.Null(result.ManifestFilePath);
        Assert.False(File.Exists(result.TargetFilePath));
        Assert.False(Directory.Exists(Path.Combine(temp.ProjectRootPath, ".ra2inieditor")));
    }

    [Fact]
    public void BackupTimestampCollisionUsesSuffix()
    {
        using TempDirectory temp = TempDirectory.Create();
        FieldRegistryApplyPlan plan = BuildPlan(
            [Definition("MyNewKey", Ra2SectionKind.Infantry)],
            [Row("MyNewKey", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Added)]);
        FieldRegistryApplyWriter writer = new();

        FieldRegistryApplyWriteResult first = writer.Write(new FieldRegistryApplyWriteRequest(
            plan,
            temp.ProjectRootPath,
            temp.GlobalRootPath,
            timestamp: Timestamp));
        FieldRegistryApplyWriteResult second = writer.Write(new FieldRegistryApplyWriteRequest(
            plan,
            temp.ProjectRootPath,
            temp.GlobalRootPath,
            timestamp: Timestamp));

        Assert.EndsWith("20260525-123456", first.BackupDirectoryPath, StringComparison.Ordinal);
        Assert.EndsWith("20260525-123456-001", second.BackupDirectoryPath, StringComparison.Ordinal);
    }

    [Fact]
    public void GlobalTargetWritesUnderGlobalRoot()
    {
        using TempDirectory temp = TempDirectory.Create();
        FieldRegistryApplyPlan plan = BuildPlan(
            [Definition("GlobalKey", Ra2SectionKind.Building)],
            [Row("GlobalKey", Ra2SectionKind.Building, FieldRegistryHarvestDiffKind.Added)],
            FieldRegistryApplyTargetScope.Global);

        FieldRegistryApplyWriteResult result = new FieldRegistryApplyWriter().Write(new FieldRegistryApplyWriteRequest(
            plan,
            null,
            temp.GlobalRootPath,
            timestamp: Timestamp));

        Assert.Equal(Path.Combine(temp.GlobalRootPath, "active", FieldRegistryApplyWriteRequest.DefaultTargetPackFileName), result.TargetFilePath);
        Assert.True(File.Exists(result.TargetFilePath));
    }

    private static FieldRegistryApplyPlan BuildPlan(
        IReadOnlyList<Ra2FieldDefinition> definitions,
        IReadOnlyList<FieldRegistryHarvestDiffRow> rows,
        FieldRegistryApplyTargetScope targetScope = FieldRegistryApplyTargetScope.Project,
        FieldRegistryApplyMode mode = FieldRegistryApplyMode.AppendOrUpdate)
    {
        FieldRegistryHarvestPreviewDraft draft = new(definitions, []);
        FieldRegistryHarvestDiffResult diff = new(rows);
        return new FieldRegistryApplyPlanBuilder().BuildPlan(new FieldRegistryApplyPlanRequest(draft, diff, targetScope, mode));
    }

    private static Ra2FieldDefinition Definition(
        string key,
        Ra2SectionKind appliesTo,
        FieldEditorKind editorKind = FieldEditorKind.Text,
        string? description = null)
    {
        return new Ra2FieldDefinition(
            key,
            [appliesTo],
            editorKind,
            Ra2FieldSourceKind.User,
            description);
    }

    private static FieldRegistryHarvestDiffRow Row(
        string key,
        Ra2SectionKind appliesTo,
        FieldRegistryHarvestDiffKind kind,
        FieldRegistryProvenanceScope existingScope = FieldRegistryProvenanceScope.None,
        string existingSourceName = "None")
    {
        return new FieldRegistryHarvestDiffRow(
            key,
            appliesTo,
            kind,
            FieldEditorKind.Text,
            FieldEditorKind.Text,
            Ra2FieldSourceKind.User,
            Ra2FieldSourceKind.User,
            existingScope,
            existingSourceName,
            null,
            "Preview description.",
            "Existing description.",
            "Diff message.");
    }

    private sealed class TempDirectory : IDisposable
    {
        private TempDirectory(string path)
        {
            Path = path;
            ProjectRootPath = System.IO.Path.Combine(path, "project");
            GlobalRootPath = System.IO.Path.Combine(path, "global", "FieldRegistry");
            Directory.CreateDirectory(ProjectRootPath);
            Directory.CreateDirectory(GlobalRootPath);
        }

        public string Path { get; }

        public string ProjectRootPath { get; }

        public string GlobalRootPath { get; }

        public static TempDirectory Create()
        {
            string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return new TempDirectory(path);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
