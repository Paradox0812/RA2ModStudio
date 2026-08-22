using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2BackupPlanBuilderTests
{
    private static readonly DateTime FixedTimestamp = new(2026, 5, 28, 23, 15, 30);
    private readonly Ra2BackupPlanBuilder _builder = new();

    [Fact]
    public void Build_CanSavePlanCreatesTimestampedBackupPath()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[E1]\nStrength=100");

        Ra2BackupPlan plan = _builder.Build(CreateSavePlan(sourcePath), workspace.Root, FixedTimestamp);

        Assert.True(plan.CanBackup);
        Assert.Equal(Ra2BackupPlanStatus.CanBackup, plan.Status);
        Assert.Equal(Path.GetFullPath(sourcePath), plan.SourceFilePath);
        Assert.EndsWith(
            Path.Combine("backup", "20260528_231530", "rulesmd.ini"),
            plan.BackupFilePath,
            StringComparison.OrdinalIgnoreCase);
        Assert.False(string.IsNullOrWhiteSpace(plan.Message));
    }

    [Fact]
    public void Build_CannotSavePlanDoesNotCreateBackupPlan()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[E1]\nStrength=100");

        Ra2BackupPlan plan = _builder.Build(
            CreateSavePlan(sourcePath, canSave: false),
            workspace.Root,
            FixedTimestamp);

        Assert.False(plan.CanBackup);
        Assert.Equal(Ra2BackupPlanStatus.SavePlanCannotSave, plan.Status);
        Assert.Empty(plan.BackupFilePath);
    }

    [Fact]
    public void Build_EmptySourcePathReportsInvalidSourcePath()
    {
        using TestWorkspace workspace = TestWorkspace.Create();

        Ra2BackupPlan plan = _builder.Build(CreateSavePlan(string.Empty), workspace.Root, FixedTimestamp);

        Assert.False(plan.CanBackup);
        Assert.Equal(Ra2BackupPlanStatus.InvalidSourcePath, plan.Status);
        Assert.Empty(plan.SourceFilePath);
        Assert.Empty(plan.BackupFilePath);
    }

    [Fact]
    public void Build_MissingSourceFileReportsSourceFileMissing()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = Path.Combine(workspace.Root, "missing.ini");

        Ra2BackupPlan plan = _builder.Build(CreateSavePlan(sourcePath), workspace.Root, FixedTimestamp);

        Assert.False(plan.CanBackup);
        Assert.Equal(Ra2BackupPlanStatus.SourceFileMissing, plan.Status);
        Assert.Equal(Path.GetFullPath(sourcePath), plan.SourceFilePath);
    }

    [Fact]
    public void Build_ProjectFileInSubdirectoryPreservesRelativePath()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile(Path.Combine("INI", "rulesmd.ini"), "[E1]\nStrength=100");

        Ra2BackupPlan plan = _builder.Build(CreateSavePlan(sourcePath), workspace.Root, FixedTimestamp);

        Assert.True(plan.CanBackup);
        Assert.EndsWith(
            Path.Combine("backup", "20260528_231530", "INI", "rulesmd.ini"),
            plan.BackupFilePath,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_BackupPathStaysUnderBackupRoot()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile(Path.Combine("INI", "rulesmd.ini"), "[E1]\nStrength=100");
        string expectedBackupRoot = Path.GetFullPath(Path.Combine(workspace.Root, "backup", "20260528_231530"));

        Ra2BackupPlan plan = _builder.Build(CreateSavePlan(sourcePath), workspace.Root, FixedTimestamp);

        Assert.True(plan.CanBackup);
        Assert.True(IsPathUnderDirectory(plan.BackupFilePath, expectedBackupRoot));
    }

    [Fact]
    public void Build_NullProjectRootFallsBackToSourceDirectoryBackupFolder()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile(Path.Combine("INI", "rulesmd.ini"), "[E1]\nStrength=100");

        Ra2BackupPlan plan = _builder.Build(CreateSavePlan(sourcePath), projectRoot: null, FixedTimestamp);

        Assert.True(plan.CanBackup);
        Assert.EndsWith(
            Path.Combine("INI", "backup", "20260528_231530", "rulesmd.ini"),
            plan.BackupFilePath,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_SourceOutsideProjectRootFallsBackToSourceDirectoryBackupFolder()
    {
        using TestWorkspace projectWorkspace = TestWorkspace.Create();
        using TestWorkspace sourceWorkspace = TestWorkspace.Create();
        string sourcePath = sourceWorkspace.WriteFile("rulesmd.ini", "[E1]\nStrength=100");

        Ra2BackupPlan plan = _builder.Build(CreateSavePlan(sourcePath), projectWorkspace.Root, FixedTimestamp);

        Assert.True(plan.CanBackup);
        Assert.EndsWith(
            Path.Combine("backup", "20260528_231530", "rulesmd.ini"),
            plan.BackupFilePath,
            StringComparison.OrdinalIgnoreCase);
        Assert.True(IsPathUnderDirectory(plan.BackupFilePath, Path.Combine(sourceWorkspace.Root, "backup", "20260528_231530")));
    }

    private static Ra2EditorSavePlan CreateSavePlan(string filePath, bool canSave = true)
        => new(
            filePath,
            "[E1]\nStrength=125",
            Ra2IniNewLineKind.Lf,
            Ra2EditorNewLineSavePolicy.PreserveCurrentText,
            canSave,
            canSave ? "Dry-run save plan can be backed up." : "Dry-run save plan cannot be saved.");

    private static bool IsPathUnderDirectory(string path, string directory)
    {
        string fullPath = EnsureTrailingSeparator(Path.GetFullPath(path));
        string fullDirectory = EnsureTrailingSeparator(Path.GetFullPath(directory));
        return fullPath.StartsWith(fullDirectory, StringComparison.OrdinalIgnoreCase);
    }

    private static string EnsureTrailingSeparator(string path)
        => path.EndsWith(Path.DirectorySeparatorChar)
            ? path
            : path + Path.DirectorySeparatorChar;

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
