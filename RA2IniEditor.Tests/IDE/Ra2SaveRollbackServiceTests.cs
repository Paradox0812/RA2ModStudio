using RA2IniEditor.IDE.Editing;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SaveRollbackServiceTests
{
    private readonly Ra2SaveRollbackService _service = new();

    [Fact]
    public void RestoreFromBackup_WhenBackupExistsRestoresOriginalFile()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string originalPath = workspace.WriteFile("rulesmd.ini", "corrupted");
        string backupPath = workspace.WriteFile(Path.Combine("backup", "rulesmd.ini"), "original");
        Ra2BackupPlan plan = CreatePlan(originalPath, backupPath);

        Ra2RollbackResult result = _service.RestoreFromBackup(plan);

        Assert.True(result.Attempted);
        Assert.True(result.Success, result.Message);
        Assert.Equal(backupPath, result.RestoredFromPath);
        Assert.Equal(originalPath, result.RestoredToPath);
        Assert.Equal("original", File.ReadAllText(originalPath));
    }

    [Fact]
    public void RestoreFromBackup_WhenBackupMissingReturnsFailure()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string originalPath = workspace.WriteFile("rulesmd.ini", "corrupted");
        string backupPath = Path.Combine(workspace.Root, "backup", "missing.ini");
        Ra2BackupPlan plan = CreatePlan(originalPath, backupPath);

        Ra2RollbackResult result = _service.RestoreFromBackup(plan);

        Assert.True(result.Attempted);
        Assert.False(result.Success);
        Assert.Null(result.Exception);
        Assert.Contains("backup file is missing", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(backupPath, result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("corrupted", File.ReadAllText(originalPath));
    }

    [Fact]
    public void RestoreFromBackup_WhenOriginalPathInvalidReturnsFailure()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string backupPath = workspace.WriteFile(Path.Combine("backup", "rulesmd.ini"), "original");
        Ra2BackupPlan plan = CreatePlan(sourcePath: string.Empty, backupPath);

        Ra2RollbackResult result = _service.RestoreFromBackup(plan);

        Assert.True(result.Attempted);
        Assert.False(result.Success);
        Assert.Null(result.Exception);
        Assert.Contains("original file path is invalid", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(backupPath, result.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RestoreFromBackup_WhenCopyFailsReturnsFailureResult()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string backupPath = workspace.WriteFile(Path.Combine("backup", "rulesmd.ini"), "original");
        string invalidOriginalPath = Path.Combine(workspace.Root, "invalid\0rules.ini");
        Ra2BackupPlan plan = CreatePlan(invalidOriginalPath, backupPath);

        Ra2RollbackResult result = _service.RestoreFromBackup(plan);

        Assert.True(result.Attempted);
        Assert.False(result.Success);
        Assert.NotNull(result.Exception);
        Assert.Contains(backupPath, result.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static Ra2BackupPlan CreatePlan(string sourcePath, string backupPath)
        => new(
            sourcePath,
            backupPath,
            Path.GetDirectoryName(backupPath) ?? string.Empty,
            Ra2BackupPlanStatus.CanBackup,
            "Backup plan is ready.");

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
