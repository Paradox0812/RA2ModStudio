using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply;
using RA2IniEditor.Infrastructure.FieldRegistry.Cleanup;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryGeneralizationCleanupPlannerTests
{
    [Fact]
    public void BuildPlan_GeneralizesConcreteUnitDuplicates()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, """
            {
              "fields": [
                { "key": "Armor", "appliesTo": ["Infantry"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "flak" }] } },
                { "key": "Armor", "appliesTo": ["Vehicle"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "heavy" }] } },
                { "key": "Armor", "appliesTo": ["Aircraft"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "light" }] } }
              ]
            }
            """);

        FieldRegistryGeneralizationCleanupPlan plan = new FieldRegistryGeneralizationCleanupPlanner()
            .BuildPlan(new FieldRegistryGeneralizationCleanupRequest(temp.Path));

        FieldRegistryGeneralizationCleanupRow row = Assert.Single(plan.Rows);
        Assert.Equal("Armor", row.Key);
        Assert.Equal(Ra2SectionKind.Unit, row.TargetSectionKind);
        Assert.Equal([Ra2SectionKind.Infantry, Ra2SectionKind.Vehicle, Ra2SectionKind.Aircraft], row.SourceSectionKinds);
        Assert.Equal(FieldEditorKind.Enum, row.EditorKind);
        Assert.Equal(Ra2FieldValueKind.Enum, row.ValueKind);
        Assert.Equal(3, row.SourceFieldCount);
        Assert.Equal(3, row.MergedAllowedValueCount);
    }

    [Fact]
    public void BuildPlan_PrefersTechnoWhenBuildingIsAlsoPresent()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, """
            {
              "fields": [
                { "key": "Armor", "appliesTo": ["Infantry"], "editorKind": "Enum", "sourceKind": "User" },
                { "key": "Armor", "appliesTo": ["Vehicle"], "editorKind": "Enum", "sourceKind": "User" },
                { "key": "Armor", "appliesTo": ["Aircraft"], "editorKind": "Enum", "sourceKind": "User" },
                { "key": "Armor", "appliesTo": ["Building"], "editorKind": "Enum", "sourceKind": "User" }
              ]
            }
            """);

        FieldRegistryGeneralizationCleanupRow row = Assert.Single(new FieldRegistryGeneralizationCleanupPlanner()
            .BuildPlan(new FieldRegistryGeneralizationCleanupRequest(temp.Path))
            .Rows);

        Assert.Equal(Ra2SectionKind.Techno, row.TargetSectionKind);
        Assert.Equal(4, row.SourceFieldCount);
    }

    [Fact]
    public void BuildPlan_DoesNotGeneralizeIncompatibleEditorKinds()
    {
        using TempDirectory temp = TempDirectory.Create();
        WritePack(temp.Path, """
            {
              "fields": [
                { "key": "Armor", "appliesTo": ["Infantry"], "editorKind": "Enum", "sourceKind": "User" },
                { "key": "Armor", "appliesTo": ["Vehicle"], "editorKind": "Text", "sourceKind": "User" },
                { "key": "Armor", "appliesTo": ["Aircraft"], "editorKind": "Enum", "sourceKind": "User" }
              ]
            }
            """);

        FieldRegistryGeneralizationCleanupPlan plan = new FieldRegistryGeneralizationCleanupPlanner()
            .BuildPlan(new FieldRegistryGeneralizationCleanupRequest(temp.Path));

        Assert.Empty(plan.Rows);
    }

    [Fact]
    public void BuildPlan_AnalyzesProjectAndGlobalSeparately()
    {
        using TempDirectory global = TempDirectory.Create();
        using TempDirectory project = TempDirectory.Create();
        WritePack(global.Path, """
            {
              "fields": [
                { "key": "Speed", "appliesTo": ["Infantry"], "editorKind": "Integer", "sourceKind": "User" },
                { "key": "Speed", "appliesTo": ["Vehicle"], "editorKind": "Integer", "sourceKind": "User" },
                { "key": "Speed", "appliesTo": ["Aircraft"], "editorKind": "Integer", "sourceKind": "User" }
              ]
            }
            """);
        WritePack(project.Path, """
            {
              "fields": [
                { "key": "Armor", "appliesTo": ["Infantry"], "editorKind": "Enum", "sourceKind": "User" },
                { "key": "Armor", "appliesTo": ["Vehicle"], "editorKind": "Enum", "sourceKind": "User" },
                { "key": "Armor", "appliesTo": ["Aircraft"], "editorKind": "Enum", "sourceKind": "User" }
              ]
            }
            """);

        FieldRegistryGeneralizationCleanupPlan plan = new FieldRegistryGeneralizationCleanupPlanner()
            .BuildPlan(new FieldRegistryGeneralizationCleanupRequest(global.Path, project.Path));

        Assert.Contains(plan.Rows, row => row.Scope == "Global" && row.Key == "Speed");
        Assert.Contains(plan.Rows, row => row.Scope == "Project" && row.Key == "Armor");
    }

    [Fact]
    public void Apply_CreatesAbstractFieldRemovesConcreteDuplicatesAndWritesRollbackManifest()
    {
        using TempDirectory root = TempDirectory.Create();
        string active = System.IO.Path.Combine(root.Path, "active");
        WritePack(active, """
            {
              "fields": [
                { "key": "Armor", "appliesTo": ["Infantry"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "flak" }] } },
                { "key": "Armor", "appliesTo": ["Vehicle"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "heavy" }] } },
                { "key": "Armor", "appliesTo": ["Aircraft"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "light" }] } }
              ]
            }
            """);

        FieldRegistryGeneralizationCleanupApplyResult result = new FieldRegistryGeneralizationCleanupApplyWriter()
            .Apply(new FieldRegistryGeneralizationCleanupApplyRequest(
                FieldRegistryApplyTargetScope.Global,
                projectRootPath: null,
                root.Path,
                new DateTimeOffset(2026, 5, 30, 1, 2, 3, TimeSpan.Zero)));

        Assert.Equal(1, result.AddedCount);
        Assert.Equal(3, result.RemovedCount);
        Assert.NotNull(result.ManifestFilePath);
        Assert.True(File.Exists(result.ManifestFilePath));

        string json = File.ReadAllText(System.IO.Path.Combine(active, "user-import.fields.json"));
        Assert.Contains("\"appliesTo\": [", json, StringComparison.Ordinal);
        Assert.Contains("\"Unit\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Infantry\"", json, StringComparison.Ordinal);
        Assert.Contains("\"flak\"", json, StringComparison.Ordinal);
        Assert.Contains("\"heavy\"", json, StringComparison.Ordinal);
        Assert.Contains("\"light\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_CompleteTechnoCoverageCreatesTechnoAndMergesAllowedValues()
    {
        using TempDirectory root = TempDirectory.Create();
        string active = System.IO.Path.Combine(root.Path, "active");
        WritePack(active, """
            {
              "fields": [
                { "key": "Armor", "appliesTo": ["Infantry"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "flak" }, { "value": "light" }] } },
                { "key": "Armor", "appliesTo": ["Vehicle"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "heavy" }, { "value": "medium" }] } },
                { "key": "Armor", "appliesTo": ["Aircraft"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "special_2" }] } },
                { "key": "Armor", "appliesTo": ["Building"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "concrete" }, { "value": "steel" }, { "value": "wood" }] } }
              ]
            }
            """);

        FieldRegistryGeneralizationCleanupApplyResult result = new FieldRegistryGeneralizationCleanupApplyWriter()
            .Apply(new FieldRegistryGeneralizationCleanupApplyRequest(
                FieldRegistryApplyTargetScope.Global,
                projectRootPath: null,
                root.Path,
                new DateTimeOffset(2026, 5, 30, 2, 3, 4, TimeSpan.Zero)));

        Assert.Equal(1, result.AddedCount);
        Assert.Equal(4, result.RemovedCount);

        string json = File.ReadAllText(System.IO.Path.Combine(active, "user-import.fields.json"));
        Assert.Contains("\"Techno\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Infantry\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Vehicle\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Aircraft\"", json, StringComparison.Ordinal);
        Assert.DoesNotContain("\"Building\"", json, StringComparison.Ordinal);
        Assert.Contains("\"flak\"", json, StringComparison.Ordinal);
        Assert.Contains("\"heavy\"", json, StringComparison.Ordinal);
        Assert.Contains("\"special_2\"", json, StringComparison.Ordinal);
        Assert.Contains("\"concrete\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_ExistingTechnoKeepsAbstractDescriptionAndMergesConcreteValues()
    {
        using TempDirectory root = TempDirectory.Create();
        string active = System.IO.Path.Combine(root.Path, "active");
        WritePack(active, """
            {
              "fields": [
                { "key": "Armor", "appliesTo": ["Techno"], "editorKind": "Enum", "sourceKind": "User", "description": "Abstract armor field.", "schema": { "type": "Enum", "allowedValues": [{ "value": "concrete" }] } },
                { "key": "Armor", "appliesTo": ["Infantry"], "editorKind": "Enum", "sourceKind": "User", "description": "Infantry armor field.", "schema": { "type": "Enum", "allowedValues": [{ "value": "flak" }] } },
                { "key": "Armor", "appliesTo": ["Vehicle"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "heavy" }] } },
                { "key": "Armor", "appliesTo": ["Aircraft"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "light" }] } },
                { "key": "Armor", "appliesTo": ["Building"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "steel" }] } }
              ]
            }
            """);

        FieldRegistryGeneralizationCleanupApplyResult result = new FieldRegistryGeneralizationCleanupApplyWriter()
            .Apply(new FieldRegistryGeneralizationCleanupApplyRequest(
                FieldRegistryApplyTargetScope.Global,
                projectRootPath: null,
                root.Path));

        Assert.Equal(0, result.AddedCount);
        Assert.Equal(1, result.UpdatedCount);
        Assert.Equal(4, result.RemovedCount);
        Assert.Contains(result.Warnings, warning => warning.Contains("description conflict", StringComparison.OrdinalIgnoreCase));

        string json = File.ReadAllText(System.IO.Path.Combine(active, "user-import.fields.json"));
        Assert.Contains("Abstract armor field.", json, StringComparison.Ordinal);
        Assert.Contains("\"concrete\"", json, StringComparison.Ordinal);
        Assert.Contains("\"flak\"", json, StringComparison.Ordinal);
        Assert.Contains("\"heavy\"", json, StringComparison.Ordinal);
        Assert.Contains("\"light\"", json, StringComparison.Ordinal);
        Assert.Contains("\"steel\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void Apply_IncompatibleConcreteSchemaSkipsWithoutWriting()
    {
        using TempDirectory root = TempDirectory.Create();
        string active = System.IO.Path.Combine(root.Path, "active");
        WritePack(active, """
            {
              "fields": [
                { "key": "Armor", "appliesTo": ["Infantry"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum" } },
                { "key": "Armor", "appliesTo": ["Vehicle"], "editorKind": "Text", "sourceKind": "User", "schema": { "type": "String" } },
                { "key": "Armor", "appliesTo": ["Aircraft"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum" } }
              ]
            }
            """);
        string before = File.ReadAllText(System.IO.Path.Combine(active, "user-import.fields.json"));

        FieldRegistryGeneralizationCleanupApplyResult result = new FieldRegistryGeneralizationCleanupApplyWriter()
            .Apply(new FieldRegistryGeneralizationCleanupApplyRequest(
                FieldRegistryApplyTargetScope.Global,
                projectRootPath: null,
                root.Path));

        Assert.Equal(0, result.AddedCount);
        Assert.Equal(0, result.RemovedCount);
        Assert.Equal(1, result.SkippedCount);
        Assert.Null(result.ManifestFilePath);
        Assert.Equal(before, File.ReadAllText(System.IO.Path.Combine(active, "user-import.fields.json")));
    }

    [Fact]
    public void Apply_WritesDetailedGeneralizationRepairManifest()
    {
        using TempDirectory root = TempDirectory.Create();
        string active = System.IO.Path.Combine(root.Path, "active");
        WritePack(active, """
            {
              "fields": [
                { "key": "Speed", "appliesTo": ["Infantry"], "editorKind": "Integer", "sourceKind": "User", "schema": { "type": "Integer" } },
                { "key": "Speed", "appliesTo": ["Vehicle"], "editorKind": "Integer", "sourceKind": "User", "schema": { "type": "Integer" } },
                { "key": "Speed", "appliesTo": ["Aircraft"], "editorKind": "Integer", "sourceKind": "User", "schema": { "type": "Integer" } }
              ]
            }
            """);

        FieldRegistryGeneralizationCleanupApplyResult result = new FieldRegistryGeneralizationCleanupApplyWriter()
            .Apply(new FieldRegistryGeneralizationCleanupApplyRequest(
                FieldRegistryApplyTargetScope.Global,
                projectRootPath: null,
                root.Path));

        Assert.NotNull(result.BackupDirectoryPath);
        string repairManifestPath = System.IO.Path.Combine(result.BackupDirectoryPath!, "generalization-repair-manifest.json");
        Assert.True(File.Exists(repairManifestPath));
        string manifest = File.ReadAllText(repairManifestPath);
        Assert.Contains("GeneralizationRepair", manifest, StringComparison.Ordinal);
        Assert.Contains("Speed | Unit", manifest, StringComparison.Ordinal);
        Assert.Contains("Speed | Infantry", manifest, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildGlobalPreview_DoesNotWritePackOrCreateBackup()
    {
        using TempDirectory root = TempDirectory.Create();
        string active = System.IO.Path.Combine(root.Path, "active");
        WritePack(active, """
            {
              "fields": [
                { "key": "Armor", "appliesTo": ["Infantry"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "flak" }] } },
                { "key": "Armor", "appliesTo": ["Vehicle"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "heavy" }] } },
                { "key": "Armor", "appliesTo": ["Aircraft"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "light" }] } },
                { "key": "Armor", "appliesTo": ["Building"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "concrete" }] } }
              ]
            }
            """);
        string packPath = System.IO.Path.Combine(active, "user-import.fields.json");
        string before = File.ReadAllText(packPath);

        FieldRegistryGeneralizationRepairPreview preview = new FieldRegistryGeneralizationCleanupApplyWriter()
            .BuildGlobalPreview(root.Path);

        Assert.True(preview.HasPlan);
        FieldRegistryGeneralizationAbstractFieldPreview abstractField = Assert.Single(preview.AbstractFields);
        Assert.Equal("新增", abstractField.OperationText);
        Assert.Equal("Armor", abstractField.Key);
        Assert.Equal("Techno", abstractField.TargetSectionKind);
        Assert.Equal(4, preview.RemovedConcreteFields.Count);
        Assert.Contains(preview.RemovedConcreteFields, row => row.Key == "Armor" && row.ConcreteSectionKind == "Infantry" && row.ReplacedBySectionKind == "Techno");
        Assert.Contains("本轮仅处理默认 active pack", preview.SummaryText, StringComparison.Ordinal);
        Assert.Equal(before, File.ReadAllText(packPath));
        Assert.False(Directory.Exists(System.IO.Path.Combine(root.Path, "backups")));
    }

    [Fact]
    public void BuildGlobalPreview_ExistingTechnoShowsUpdateAndAllowedValues()
    {
        using TempDirectory root = TempDirectory.Create();
        string active = System.IO.Path.Combine(root.Path, "active");
        WritePack(active, """
            {
              "fields": [
                { "key": "Armor", "appliesTo": ["Techno"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "concrete" }] } },
                { "key": "Armor", "appliesTo": ["Infantry"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "flak" }] } },
                { "key": "Armor", "appliesTo": ["Vehicle"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "heavy" }] } },
                { "key": "Armor", "appliesTo": ["Aircraft"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "light" }] } },
                { "key": "Armor", "appliesTo": ["Building"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum", "allowedValues": [{ "value": "steel" }] } }
              ]
            }
            """);

        FieldRegistryGeneralizationRepairPreview preview = new FieldRegistryGeneralizationCleanupApplyWriter()
            .BuildGlobalPreview(root.Path);

        FieldRegistryGeneralizationAbstractFieldPreview abstractField = Assert.Single(preview.AbstractFields);
        Assert.Equal("更新", abstractField.OperationText);
        Assert.Contains("concrete", abstractField.AllowedValues);
        Assert.Contains("flak", abstractField.AllowedValues);
        Assert.Contains("heavy", abstractField.AllowedValues);
        Assert.Contains("light", abstractField.AllowedValues);
        Assert.Contains("steel", abstractField.AllowedValues);
    }

    [Fact]
    public void BuildGlobalPreview_IncompatibleSchemaShowsSkippedReason()
    {
        using TempDirectory root = TempDirectory.Create();
        string active = System.IO.Path.Combine(root.Path, "active");
        WritePack(active, """
            {
              "fields": [
                { "key": "Armor", "appliesTo": ["Infantry"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum" } },
                { "key": "Armor", "appliesTo": ["Vehicle"], "editorKind": "Text", "sourceKind": "User", "schema": { "type": "String" } },
                { "key": "Armor", "appliesTo": ["Aircraft"], "editorKind": "Enum", "sourceKind": "User", "schema": { "type": "Enum" } }
              ]
            }
            """);

        FieldRegistryGeneralizationRepairPreview preview = new FieldRegistryGeneralizationCleanupApplyWriter()
            .BuildGlobalPreview(root.Path);

        FieldRegistryGeneralizationSkippedFieldPreview skipped = Assert.Single(preview.SkippedFields);
        Assert.Equal("Armor", skipped.Key);
        Assert.Contains("incompatible", skipped.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Apply_MissingDefaultPackReturnsWarningWithoutWriting()
    {
        using TempDirectory root = TempDirectory.Create();

        FieldRegistryGeneralizationCleanupApplyResult result = new FieldRegistryGeneralizationCleanupApplyWriter()
            .Apply(new FieldRegistryGeneralizationCleanupApplyRequest(
                FieldRegistryApplyTargetScope.Global,
                projectRootPath: null,
                root.Path));

        Assert.Equal(0, result.AddedCount);
        Assert.Equal(0, result.RemovedCount);
        Assert.Null(result.ManifestFilePath);
        Assert.Single(result.Warnings);
    }

    private static void WritePack(string directoryPath, string json)
    {
        Directory.CreateDirectory(directoryPath);
        File.WriteAllText(Path.Combine(directoryPath, "user-import.fields.json"), json);
    }

    private sealed class TempDirectory : IDisposable
    {
        private TempDirectory(string path)
        {
            Path = path;
        }

        public string Path { get; }

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
