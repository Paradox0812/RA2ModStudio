using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SaveCurrentFileOrchestratorTests
{
    private static readonly DateTime FixedTimestamp = new(2026, 5, 28, 23, 15, 30);
    private readonly Ra2EditableDocumentSessionService _sessionService = new(
        new Ra2IniTextDocumentParser(),
        new Ra2DirtyStateService());

    [Fact]
    public void PrepareToSave_DirtySessionWithBackupDryRunStopsBeforeWrite()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[E1]\nStrength=100");
        Ra2EditableDocumentSession session = CreateDirtySession(sourcePath, "[E1]\nStrength=125");
        Ra2SaveCurrentFileOrchestrator orchestrator = new();

        Ra2SaveCurrentFileOrchestrationResult result = orchestrator.PrepareToSave(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp,
            executeBackup: false);

        Assert.True(result.Success);
        Assert.False(result.ReadyToWrite);
        Assert.Equal(Ra2SaveCurrentFileOrchestrationStatus.StoppedBeforeWrite, result.Status);
        Assert.Equal(Ra2SaveCurrentFileOrchestrationStage.StoppedBeforeWrite, result.Stage);
        Assert.NotNull(result.SavePlan);
        Assert.NotNull(result.BackupPlan);
        Assert.Null(result.BackupResult);
        Assert.False(File.Exists(result.BackupPlan!.BackupFilePath));
        Assert.Equal("[E1]\nStrength=100", File.ReadAllText(sourcePath));
        Assert.Equal("[E1]\nStrength=100", session.DocumentState.OriginalText);
        Assert.Equal("[E1]\nStrength=125", session.DocumentState.CurrentText);
        Assert.Equal(Ra2EditorDocumentState.EditableDirty, session.DocumentState.State);
    }

    [Fact]
    public void PrepareToSave_DirtySessionWithExecuteBackupCreatesBackupAndReturnsReadyToWrite()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[E1]\nStrength=100");
        Ra2EditableDocumentSession session = CreateDirtySession(sourcePath, "[E1]\nStrength=125");
        Ra2SaveCurrentFileOrchestrator orchestrator = new();

        Ra2SaveCurrentFileOrchestrationResult result = orchestrator.PrepareToSave(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp,
            executeBackup: true);

        Assert.True(result.Success);
        Assert.True(result.ReadyToWrite);
        Assert.Equal(Ra2SaveCurrentFileOrchestrationStatus.ReadyToWrite, result.Status);
        Assert.Equal(Ra2SaveCurrentFileOrchestrationStage.BackupCompleted, result.Stage);
        Assert.NotNull(result.BackupResult);
        Assert.True(result.BackupResult!.Success);
        Assert.Equal(result.BackupPlan!.BackupFilePath, result.BackupResult.BackupFilePath);
        Assert.Equal("[E1]\nStrength=100", File.ReadAllText(result.BackupPlan.BackupFilePath));
        Assert.Equal("[E1]\nStrength=100", File.ReadAllText(sourcePath));
        Assert.Equal("[E1]\nStrength=100", session.DocumentState.OriginalText);
        Assert.Equal("[E1]\nStrength=125", session.DocumentState.CurrentText);
        Assert.Equal(Ra2EditorDocumentState.EditableDirty, session.DocumentState.State);
    }

    [Fact]
    public void PrepareToSave_NoSessionReturnsSavePlanCannotSaveAndDoesNotBuildBackup()
    {
        RecordingBackupPlanBuilder backupPlanBuilder = new();
        RecordingBackupService backupService = new(Ra2BackupResult.Succeeded("unused"));
        Ra2SaveCurrentFileOrchestrator orchestrator = new(
            new Ra2SaveCurrentFilePlanBuilder(),
            backupPlanBuilder,
            backupService);

        Ra2SaveCurrentFileOrchestrationResult result = orchestrator.PrepareToSave(
            new Ra2SaveCurrentFilePlanRequest(null, isReadOnlyPreview: false),
            projectRoot: null,
            FixedTimestamp,
            executeBackup: true);

        Assert.False(result.Success);
        Assert.False(result.ReadyToWrite);
        Assert.Equal(Ra2SaveCurrentFileOrchestrationStatus.SavePlanCannotSave, result.Status);
        Assert.Equal(Ra2SaveCurrentFileOrchestrationStage.SavePlanBuilt, result.Stage);
        Assert.Equal(0, backupPlanBuilder.CallCount);
        Assert.Equal(0, backupService.CallCount);
    }

    [Fact]
    public void PrepareToSave_ReadOnlyPreviewReturnsSavePlanCannotSave()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[E1]\nStrength=100");
        Ra2EditableDocumentSession session = _sessionService.StartEditing(sourcePath, "[E1]\nStrength=100");
        Ra2SaveCurrentFileOrchestrator orchestrator = new();

        Ra2SaveCurrentFileOrchestrationResult result = orchestrator.PrepareToSave(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: true),
            workspace.Root,
            FixedTimestamp,
            executeBackup: true);

        Assert.False(result.Success);
        Assert.Equal(Ra2SaveCurrentFileOrchestrationStatus.SavePlanCannotSave, result.Status);
        Assert.Equal(Ra2SaveCurrentFilePlanStatus.ReadOnlyPreview, result.SavePlan!.Status);
        Assert.Null(result.BackupPlan);
        Assert.Null(result.BackupResult);
    }

    [Fact]
    public void PrepareToSave_CleanSessionReturnsSavePlanCannotSave()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[E1]\nStrength=100");
        Ra2EditableDocumentSession session = _sessionService.StartEditing(sourcePath, "[E1]\nStrength=100");
        Ra2SaveCurrentFileOrchestrator orchestrator = new();

        Ra2SaveCurrentFileOrchestrationResult result = orchestrator.PrepareToSave(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp,
            executeBackup: true);

        Assert.False(result.Success);
        Assert.Equal(Ra2SaveCurrentFileOrchestrationStatus.SavePlanCannotSave, result.Status);
        Assert.Equal(Ra2SaveCurrentFilePlanStatus.NotDirty, result.SavePlan!.Status);
    }

    [Fact]
    public void PrepareToSave_MissingFilePathReturnsSavePlanCannotSave()
    {
        Ra2EditableDocumentSession session = CreateDirtySession(string.Empty, "[E1]\nStrength=125");
        Ra2SaveCurrentFileOrchestrator orchestrator = new();

        Ra2SaveCurrentFileOrchestrationResult result = orchestrator.PrepareToSave(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            projectRoot: null,
            FixedTimestamp,
            executeBackup: true);

        Assert.False(result.Success);
        Assert.Equal(Ra2SaveCurrentFileOrchestrationStatus.SavePlanCannotSave, result.Status);
        Assert.Equal(Ra2SaveCurrentFilePlanStatus.MissingFilePath, result.SavePlan!.Status);
        Assert.Null(result.BackupPlan);
    }

    [Fact]
    public void PrepareToSave_BackupPlanCannotBackupBlocksWriteReadiness()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string missingSourcePath = Path.Combine(workspace.Root, "missing.ini");
        Ra2EditableDocumentSession session = CreateDirtySession(missingSourcePath, "[E1]\nStrength=125");
        Ra2SaveCurrentFileOrchestrator orchestrator = new();

        Ra2SaveCurrentFileOrchestrationResult result = orchestrator.PrepareToSave(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp,
            executeBackup: true);

        Assert.False(result.Success);
        Assert.False(result.ReadyToWrite);
        Assert.Equal(Ra2SaveCurrentFileOrchestrationStatus.BackupPlanCannotBackup, result.Status);
        Assert.Equal(Ra2SaveCurrentFileOrchestrationStage.BackupPlanBuilt, result.Stage);
        Assert.Equal(Ra2BackupPlanStatus.SourceFileMissing, result.BackupPlan!.Status);
        Assert.Null(result.BackupResult);
    }

    [Fact]
    public void PrepareToSave_BackupServiceFailureBlocksWriteReadiness()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[E1]\nStrength=100");
        Ra2EditableDocumentSession session = CreateDirtySession(sourcePath, "[E1]\nStrength=125");
        Ra2SaveCurrentFileOrchestrator orchestrator = new();

        Ra2SaveCurrentFileOrchestrationResult first = orchestrator.PrepareToSave(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp,
            executeBackup: true);
        Ra2SaveCurrentFileOrchestrationResult second = orchestrator.PrepareToSave(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp,
            executeBackup: true);

        Assert.True(first.ReadyToWrite);
        Assert.False(second.Success);
        Assert.False(second.ReadyToWrite);
        Assert.Equal(Ra2SaveCurrentFileOrchestrationStatus.BackupFailed, second.Status);
        Assert.Equal(Ra2SaveCurrentFileOrchestrationStage.FailedBeforeWrite, second.Stage);
        Assert.NotNull(second.BackupResult);
        Assert.False(second.BackupResult!.Success);
        Assert.Equal("[E1]\nStrength=100", File.ReadAllText(sourcePath));
    }

    [Fact]
    public void PrepareToSave_ReturnsBackupResultFromBackupService()
    {
        Ra2EditorSavePlan savePlan = CreateSavePlan("rulesmd.ini");
        Ra2BackupPlan backupPlan = new(
            "rulesmd.ini",
            "backup.ini",
            Path.GetTempPath(),
            Ra2BackupPlanStatus.CanBackup,
            "Backup plan is ready.");
        Ra2BackupResult backupResult = Ra2BackupResult.Succeeded("backup.ini");
        Ra2SaveCurrentFileOrchestrator orchestrator = new(
            new StubSavePlanBuilder(savePlan),
            new StubBackupPlanBuilder(backupPlan),
            new RecordingBackupService(backupResult));

        Ra2SaveCurrentFileOrchestrationResult result = orchestrator.PrepareToSave(
            new Ra2SaveCurrentFilePlanRequest(null, isReadOnlyPreview: false),
            projectRoot: null,
            FixedTimestamp,
            executeBackup: true);

        Assert.Same(backupResult, result.BackupResult);
        Assert.Same(backupPlan, result.BackupPlan);
        Assert.Same(savePlan, result.SavePlan);
        Assert.True(result.ReadyToWrite);
    }

    private Ra2EditableDocumentSession CreateDirtySession(string filePath, string currentText)
    {
        Ra2EditableDocumentSession session = _sessionService.StartEditing(filePath, "[E1]\nStrength=100");
        return _sessionService.UpdateText(session, currentText);
    }

    private static Ra2EditorSavePlan CreateSavePlan(string filePath)
        => new(
            filePath,
            "[E1]\nStrength=125",
            Ra2IniNewLineKind.Lf,
            Ra2EditorNewLineSavePolicy.PreserveCurrentText,
            canSave: true,
            "Dry-run save plan can be backed up.");

    private sealed class StubSavePlanBuilder : IRa2SaveCurrentFilePlanBuilder
    {
        private readonly Ra2EditorSavePlan _savePlan;

        public StubSavePlanBuilder(Ra2EditorSavePlan savePlan)
        {
            _savePlan = savePlan;
        }

        public Ra2EditorSavePlan BuildDryRun(Ra2SaveCurrentFilePlanRequest request)
            => _savePlan;
    }

    private sealed class StubBackupPlanBuilder : IRa2BackupPlanBuilder
    {
        private readonly Ra2BackupPlan _backupPlan;

        public StubBackupPlanBuilder(Ra2BackupPlan backupPlan)
        {
            _backupPlan = backupPlan;
        }

        public Ra2BackupPlan Build(Ra2EditorSavePlan savePlan, string? projectRoot, DateTime timestamp)
            => _backupPlan;
    }

    private sealed class RecordingBackupPlanBuilder : IRa2BackupPlanBuilder
    {
        public int CallCount { get; private set; }

        public Ra2BackupPlan Build(Ra2EditorSavePlan savePlan, string? projectRoot, DateTime timestamp)
        {
            CallCount++;
            return new Ra2BackupPlan(
                savePlan.FilePath,
                Path.Combine(Path.GetTempPath(), "backup.ini"),
                Path.GetTempPath(),
                Ra2BackupPlanStatus.CanBackup,
                "Backup plan is ready.");
        }
    }

    private sealed class RecordingBackupService : IRa2BackupService
    {
        private readonly Ra2BackupResult _result;

        public RecordingBackupService(Ra2BackupResult result)
        {
            _result = result;
        }

        public int CallCount { get; private set; }

        public Ra2BackupResult CreateBackup(Ra2BackupPlan plan)
        {
            CallCount++;
            return _result;
        }
    }

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
