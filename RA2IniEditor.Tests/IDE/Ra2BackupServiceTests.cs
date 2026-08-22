using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2BackupServiceTests
{
    private static readonly DateTime FixedTimestamp = new(2026, 5, 28, 23, 15, 30);
    private readonly Ra2BackupPlanBuilder _planBuilder = new();
    private readonly Ra2BackupService _service = new();

    [Fact]
    public void CreateBackup_CopiesOriginalDiskContent()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[E1]\nStrength=100");
        Ra2EditorSavePlan savePlan = CreateSavePlan(sourcePath, currentText: "[E1]\nStrength=125");
        Ra2BackupPlan backupPlan = _planBuilder.Build(savePlan, workspace.Root, FixedTimestamp);

        Ra2BackupResult result = _service.CreateBackup(backupPlan);

        Assert.True(result.Success, result.Message);
        Assert.True(File.Exists(result.BackupFilePath));
        Assert.Equal("[E1]\nStrength=100", File.ReadAllText(result.BackupFilePath));
        Assert.Equal("[E1]\nStrength=100", File.ReadAllText(sourcePath));
    }

    [Fact]
    public void CreateBackup_CreatesNestedBackupDirectory()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile(Path.Combine("INI", "rulesmd.ini"), "[E1]\nStrength=100");
        Ra2BackupPlan backupPlan = _planBuilder.Build(CreateSavePlan(sourcePath), workspace.Root, FixedTimestamp);

        Ra2BackupResult result = _service.CreateBackup(backupPlan);

        Assert.True(result.Success, result.Message);
        Assert.True(Directory.Exists(backupPlan.BackupDirectory));
        Assert.True(File.Exists(backupPlan.BackupFilePath));
    }

    [Fact]
    public void CreateBackup_ExistingBackupFileReturnsFailure()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[E1]\nStrength=100");
        Ra2BackupPlan backupPlan = _planBuilder.Build(CreateSavePlan(sourcePath), workspace.Root, FixedTimestamp);
        Directory.CreateDirectory(backupPlan.BackupDirectory);
        File.WriteAllText(backupPlan.BackupFilePath, "existing backup");

        Ra2BackupResult result = _service.CreateBackup(backupPlan);

        Assert.False(result.Success);
        Assert.NotNull(result.Exception);
        Assert.Equal("existing backup", File.ReadAllText(backupPlan.BackupFilePath));
    }

    [Fact]
    public void CreateBackup_SourceMissingReturnsFailureWithoutThrowing()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = Path.Combine(workspace.Root, "missing.ini");
        Ra2BackupPlan backupPlan = _planBuilder.Build(CreateSavePlan(sourcePath), workspace.Root, FixedTimestamp);

        Ra2BackupResult result = _service.CreateBackup(backupPlan);

        Assert.False(result.Success);
        Assert.Null(result.Exception);
        Assert.Equal(Ra2BackupPlanStatus.SourceFileMissing, backupPlan.Status);
    }

    [Fact]
    public void CreateBackup_InvalidPlanReturnsFailureWithoutTouchingDisk()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        Ra2BackupPlan backupPlan = _planBuilder.Build(CreateSavePlan(string.Empty), workspace.Root, FixedTimestamp);

        Ra2BackupResult result = _service.CreateBackup(backupPlan);

        Assert.False(result.Success);
        Assert.Null(result.Exception);
        Assert.False(Directory.Exists(Path.Combine(workspace.Root, "backup")));
    }

    private static Ra2EditorSavePlan CreateSavePlan(string filePath, string currentText = "[E1]\nStrength=125")
        => new(
            filePath,
            currentText,
            Ra2IniNewLineKind.Lf,
            Ra2EditorNewLineSavePolicy.PreserveCurrentText,
            canSave: true,
            "Dry-run save plan can be backed up.");

    private sealed class TestWorkspace : IDisposable
    {
        private TestWorkspace(string root)
        {
            Root = root;
        }

        public string Root { get; }

        public static TestWorkspace Create()
        {
            string root = Path.Combine(Path.GetTempPath(), "RA2IniEditor.Tests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            return new TestWorkspace(root);
        }

        public string WriteFile(string relativePath, string text)
        {
            string path = Path.Combine(Root, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(Root))
                Directory.Delete(Root, recursive: true);
        }
    }
}
