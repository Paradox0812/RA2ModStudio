using System.Text.Json;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.Rollback;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryApplyBackupManifestReaderTests
{
    [Fact]
    public void ReadReturnsManifestFields()
    {
        using TempDirectory temp = TempDirectory.Create();
        string targetPath = temp.ProjectActivePackPath;
        string backupPath = temp.ProjectBackupPackPath("20260525-123456");
        string manifestPath = temp.WriteManifest("20260525-123456", new FieldRegistryApplyBackupManifest(
            "Project",
            targetPath,
            backupPath,
            true,
            "2026-05-25T12:34:56.0000000Z",
            2,
            1,
            3,
            "AppendOrUpdate"));

        FieldRegistryApplyBackupManifest manifest = new FieldRegistryApplyBackupManifestReader().Read(manifestPath);

        Assert.Equal("Project", manifest.TargetScope);
        Assert.Equal(targetPath, manifest.TargetFilePath);
        Assert.Equal(backupPath, manifest.BackupFilePath);
        Assert.True(manifest.TargetFileExisted);
        Assert.Equal(2, manifest.AddCount);
        Assert.Equal(1, manifest.UpdateCount);
        Assert.Equal(3, manifest.SkipCount);
        Assert.Equal("AppendOrUpdate", manifest.Mode);
    }

    [Fact]
    public void FindManifestFilesReturnsNewestPathFirst()
    {
        using TempDirectory temp = TempDirectory.Create();
        string older = temp.WriteManifest("20260101-120000", temp.CreateManifest(targetFileExisted: false));
        string newer = temp.WriteManifest("20260101-120001", temp.CreateManifest(targetFileExisted: false));

        IReadOnlyList<string> manifests = new FieldRegistryApplyBackupManifestReader().FindManifestFiles(temp.ProjectBackupRootPath);

        Assert.Equal([newer, older], manifests);
    }

    [Fact]
    public void FindManifestFilesMissingDirectoryReturnsEmptyList()
    {
        using TempDirectory temp = TempDirectory.Create();

        IReadOnlyList<string> manifests = new FieldRegistryApplyBackupManifestReader().FindManifestFiles(
            Path.Combine(temp.ProjectRootPath, ".ra2inieditor", "field-registry", "missing"));

        Assert.Empty(manifests);
    }

    [Fact]
    public void MalformedManifestThrows()
    {
        using TempDirectory temp = TempDirectory.Create();
        string manifestDirectory = Path.Combine(temp.ProjectBackupRootPath, "20260525-123456");
        Directory.CreateDirectory(manifestDirectory);
        string manifestPath = Path.Combine(manifestDirectory, "manifest.json");
        File.WriteAllText(manifestPath, "{ not-json");

        Assert.Throws<JsonException>(() => new FieldRegistryApplyBackupManifestReader().Read(manifestPath));
    }

    [Fact]
    public void MissingRequiredManifestFieldsThrows()
    {
        using TempDirectory temp = TempDirectory.Create();
        string manifestDirectory = Path.Combine(temp.ProjectBackupRootPath, "20260525-123456");
        Directory.CreateDirectory(manifestDirectory);
        string manifestPath = Path.Combine(manifestDirectory, "manifest.json");
        File.WriteAllText(manifestPath, "{}");

        Assert.ThrowsAny<Exception>(() => new FieldRegistryApplyBackupManifestReader().Read(manifestPath));
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

        public FieldRegistryApplyBackupManifest CreateManifest(bool targetFileExisted)
        {
            return new FieldRegistryApplyBackupManifest(
                "Project",
                ProjectActivePackPath,
                targetFileExisted ? ProjectBackupPackPath("20260525-123456") : null,
                targetFileExisted,
                "2026-05-25T12:34:56.0000000Z",
                1,
                0,
                0,
                "AppendOrUpdate");
        }

        public string WriteManifest(string batchName, FieldRegistryApplyBackupManifest manifest)
        {
            string manifestDirectory = System.IO.Path.Combine(ProjectBackupRootPath, batchName);
            Directory.CreateDirectory(manifestDirectory);
            string manifestPath = System.IO.Path.Combine(manifestDirectory, "manifest.json");
            File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest));
            return manifestPath;
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
