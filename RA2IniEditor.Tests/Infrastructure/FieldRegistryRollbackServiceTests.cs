using System.Text.Json;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.Rollback;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryRollbackServiceTests
{
    private static readonly DateTimeOffset Timestamp = new(2026, 5, 25, 12, 34, 56, TimeSpan.Zero);

    [Fact]
    public void RollbackRestoresExistingTargetFromBackup()
    {
        using TempDirectory temp = TempDirectory.Create();
        WriteText(temp.ProjectActivePackPath, "new content");
        string backupPath = temp.ProjectBackupPackPath("20260525-123456");
        WriteText(backupPath, "old content");
        string manifestPath = temp.WriteProjectManifest("20260525-123456", targetFileExisted: true, backupPath);

        FieldRegistryRollbackResult result = new FieldRegistryRollbackService().Rollback(temp.CreateRequest(manifestPath));

        Assert.True(result.Succeeded);
        Assert.Equal(FieldRegistryRollbackOperationKind.RestoreBackup, result.OperationKind);
        Assert.Equal("old content", File.ReadAllText(temp.ProjectActivePackPath));
        Assert.True(File.Exists(backupPath));
        Assert.True(File.Exists(manifestPath));
    }

    [Fact]
    public void RollbackDeletesCreatedTarget()
    {
        using TempDirectory temp = TempDirectory.Create();
        WriteText(temp.ProjectActivePackPath, "created content");
        string manifestPath = temp.WriteProjectManifest("20260525-123456", targetFileExisted: false, backupPath: null);

        FieldRegistryRollbackResult result = new FieldRegistryRollbackService().Rollback(temp.CreateRequest(manifestPath));

        Assert.True(result.Succeeded);
        Assert.Equal(FieldRegistryRollbackOperationKind.DeleteCreatedTarget, result.OperationKind);
        Assert.False(File.Exists(temp.ProjectActivePackPath));
        Assert.True(File.Exists(manifestPath));
    }

    [Fact]
    public void RollbackCreatedTargetAlreadyMissingReturnsNoOp()
    {
        using TempDirectory temp = TempDirectory.Create();
        string manifestPath = temp.WriteProjectManifest("20260525-123456", targetFileExisted: false, backupPath: null);

        FieldRegistryRollbackResult result = new FieldRegistryRollbackService().Rollback(temp.CreateRequest(manifestPath));

        Assert.True(result.Succeeded);
        Assert.Equal(FieldRegistryRollbackOperationKind.NoOp, result.OperationKind);
        Assert.False(File.Exists(temp.ProjectActivePackPath));
    }

    [Fact]
    public void MissingBackupFileBlocksRestoreAndLeavesTargetUntouched()
    {
        using TempDirectory temp = TempDirectory.Create();
        WriteText(temp.ProjectActivePackPath, "new content");
        string missingBackupPath = temp.ProjectBackupPackPath("20260525-123456");
        string manifestPath = temp.WriteProjectManifest("20260525-123456", targetFileExisted: true, missingBackupPath);

        Assert.Throws<FileNotFoundException>(() => new FieldRegistryRollbackService().Rollback(temp.CreateRequest(manifestPath)));
        Assert.Equal("new content", File.ReadAllText(temp.ProjectActivePackPath));
    }

    [Fact]
    public void ManifestPathOutsideBackupRootIsRejected()
    {
        using TempDirectory temp = TempDirectory.Create();
        string manifestDirectory = Path.Combine(temp.Path, "outside-manifest");
        Directory.CreateDirectory(manifestDirectory);
        string manifestPath = Path.Combine(manifestDirectory, "manifest.json");
        temp.WriteManifestToPath(manifestPath, temp.CreateProjectManifest(targetFileExisted: false, backupPath: null));

        Assert.Throws<InvalidOperationException>(() => new FieldRegistryRollbackService().Rollback(temp.CreateRequest(manifestPath)));
    }

    [Fact]
    public void TargetPathOutsideActiveRootIsRejected()
    {
        using TempDirectory temp = TempDirectory.Create();
        string outsideTarget = Path.Combine(temp.Path, "outside", FieldRegistryApplyWriteRequest.DefaultTargetPackFileName);
        WriteText(outsideTarget, "outside content");
        string manifestPath = temp.WriteProjectManifest(
            "20260525-123456",
            temp.CreateProjectManifest(targetFileExisted: false, backupPath: null, targetPath: outsideTarget));

        Assert.Throws<InvalidOperationException>(() => new FieldRegistryRollbackService().Rollback(temp.CreateRequest(manifestPath)));
        Assert.Equal("outside content", File.ReadAllText(outsideTarget));
    }

    [Fact]
    public void UnsupportedTargetFileNameIsRejected()
    {
        using TempDirectory temp = TempDirectory.Create();
        string unsupportedTarget = Path.Combine(
            temp.ProjectRootPath,
            ".ra2inieditor",
            "field-registry",
            "active",
            "other.fields.json");
        string manifestPath = temp.WriteProjectManifest(
            "20260525-123456",
            temp.CreateProjectManifest(targetFileExisted: false, backupPath: null, targetPath: unsupportedTarget));

        Assert.Throws<NotSupportedException>(() => new FieldRegistryRollbackService().Rollback(temp.CreateRequest(manifestPath)));
    }

    [Fact]
    public void BackupPathForCreatedTargetIsRejected()
    {
        using TempDirectory temp = TempDirectory.Create();
        string backupPath = temp.ProjectBackupPackPath("20260525-123456");
        WriteText(backupPath, "old content");
        string manifestPath = temp.WriteProjectManifest("20260525-123456", targetFileExisted: false, backupPath);

        Assert.Throws<InvalidOperationException>(() => new FieldRegistryRollbackService().Rollback(temp.CreateRequest(manifestPath)));
    }

    [Fact]
    public void BackupPathOutsideManifestBatchIsRejected()
    {
        using TempDirectory temp = TempDirectory.Create();
        WriteText(temp.ProjectActivePackPath, "new content");
        string backupPath = temp.ProjectBackupPackPath("20260525-123455");
        WriteText(backupPath, "old content");
        string manifestPath = temp.WriteProjectManifest("20260525-123456", targetFileExisted: true, backupPath);

        Assert.Throws<InvalidOperationException>(() => new FieldRegistryRollbackService().Rollback(temp.CreateRequest(manifestPath)));
        Assert.Equal("new content", File.ReadAllText(temp.ProjectActivePackPath));
    }

    [Fact]
    public void WriterCreatedTargetManifestRollsBackByDeletingTarget()
    {
        using TempDirectory temp = TempDirectory.Create();
        FieldRegistryApplyPlan plan = BuildPlan(
            [Definition("MyNewKey", Ra2SectionKind.Infantry)],
            [Row("MyNewKey", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Added)]);
        FieldRegistryApplyWriteResult writeResult = new FieldRegistryApplyWriter().Write(new FieldRegistryApplyWriteRequest(
            plan,
            temp.ProjectRootPath,
            temp.GlobalRootPath,
            timestamp: Timestamp));

        FieldRegistryRollbackResult rollbackResult = new FieldRegistryRollbackService().Rollback(temp.CreateRequest(writeResult.ManifestFilePath!));

        Assert.Equal(FieldRegistryRollbackOperationKind.DeleteCreatedTarget, rollbackResult.OperationKind);
        Assert.False(File.Exists(writeResult.TargetFilePath));
        Assert.True(File.Exists(writeResult.ManifestFilePath));
    }

    [Fact]
    public void WriterExistingTargetManifestRollsBackToOldContent()
    {
        using TempDirectory temp = TempDirectory.Create();
        WriteText(temp.ProjectActivePackPath, """
            {
              "fields": [
                { "key": "Owner", "appliesTo": ["Infantry"], "editorKind": "Text", "sourceKind": "User", "description": "old description" }
              ]
            }
            """);
        FieldRegistryApplyPlan plan = BuildPlan(
            [Definition("Owner", Ra2SectionKind.Infantry, FieldEditorKind.Float, "new description")],
            [Row("Owner", Ra2SectionKind.Infantry, FieldRegistryHarvestDiffKind.Changed, FieldRegistryProvenanceScope.Project, "project.fields.json")]);
        FieldRegistryApplyWriteResult writeResult = new FieldRegistryApplyWriter().Write(new FieldRegistryApplyWriteRequest(
            plan,
            temp.ProjectRootPath,
            temp.GlobalRootPath,
            timestamp: Timestamp));
        File.WriteAllText(writeResult.TargetFilePath, "later content");

        FieldRegistryRollbackResult rollbackResult = new FieldRegistryRollbackService().Rollback(temp.CreateRequest(writeResult.ManifestFilePath!));

        Assert.Equal(FieldRegistryRollbackOperationKind.RestoreBackup, rollbackResult.OperationKind);
        string restored = File.ReadAllText(writeResult.TargetFilePath);
        Assert.Contains("old description", restored, StringComparison.Ordinal);
        Assert.DoesNotContain("later content", restored, StringComparison.Ordinal);
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

    private static void WriteText(string path, string text)
    {
        string? directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        File.WriteAllText(path, text);
    }

    private sealed class TempDirectory : IDisposable
    {
        private TempDirectory(string path)
        {
            Path = path;
            ProjectRootPath = System.IO.Path.Combine(path, "project");
            GlobalRootPath = System.IO.Path.Combine(path, "global", "FieldRegistry");
            ProjectBackupRootPath = System.IO.Path.Combine(ProjectRootPath, ".ra2inieditor", "field-registry", "backups");
            Directory.CreateDirectory(ProjectRootPath);
            Directory.CreateDirectory(GlobalRootPath);
        }

        public string Path { get; }

        public string ProjectRootPath { get; }

        public string GlobalRootPath { get; }

        public string ProjectBackupRootPath { get; }

        public string ProjectActivePackPath => System.IO.Path.Combine(
            ProjectRootPath,
            ".ra2inieditor",
            "field-registry",
            "active",
            FieldRegistryApplyWriteRequest.DefaultTargetPackFileName);

        public string ProjectBackupPackPath(string batchName)
            => System.IO.Path.Combine(ProjectBackupRootPath, batchName, FieldRegistryApplyWriteRequest.DefaultTargetPackFileName);

        public static TempDirectory Create()
        {
            string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return new TempDirectory(path);
        }

        public FieldRegistryRollbackRequest CreateRequest(string manifestPath)
            => new(manifestPath, ProjectRootPath, GlobalRootPath);

        public FieldRegistryApplyBackupManifest CreateProjectManifest(
            bool targetFileExisted,
            string? backupPath,
            string? targetPath = null)
        {
            return new FieldRegistryApplyBackupManifest(
                "Project",
                targetPath ?? ProjectActivePackPath,
                backupPath,
                targetFileExisted,
                "2026-05-25T12:34:56.0000000Z",
                1,
                0,
                0,
                "AppendOrUpdate");
        }

        public string WriteProjectManifest(string batchName, bool targetFileExisted, string? backupPath)
            => WriteProjectManifest(batchName, CreateProjectManifest(targetFileExisted, backupPath));

        public string WriteProjectManifest(string batchName, FieldRegistryApplyBackupManifest manifest)
        {
            string manifestPath = System.IO.Path.Combine(ProjectBackupRootPath, batchName, "manifest.json");
            WriteManifestToPath(manifestPath, manifest);
            return manifestPath;
        }

        public void WriteManifestToPath(string manifestPath, FieldRegistryApplyBackupManifest manifest)
        {
            string? directory = System.IO.Path.GetDirectoryName(manifestPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest));
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
