using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SaveCurrentFileServiceTests
{
    private static readonly DateTime FixedTimestamp = new(2026, 5, 28, 23, 15, 30);
    private readonly Ra2EditableDocumentSessionService _sessionService = new(
        new Ra2IniTextDocumentParser(),
        new Ra2DirtyStateService());

    [Fact]
    public void Save_DirtySessionBacksUpOriginalWritesCurrentTextAndClearsDirty()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[HTNK]\nStrength=400\n");
        Ra2EditableDocumentSession session = CreateDirtySession(
            sourcePath,
            "[HTNK]\nStrength=500\n");
        Ra2SaveCurrentFileService service = new();

        Ra2SaveCurrentFileResult result = service.Save(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp);

        Assert.True(result.Success, result.Message);
        Assert.NotNull(result.UpdatedSession);
        Assert.Equal("[HTNK]\nStrength=500\n", File.ReadAllText(sourcePath));
        Assert.Equal("[HTNK]\nStrength=400\n", File.ReadAllText(result.BackupPlan!.BackupFilePath));
        Assert.Equal(Ra2EditorDocumentState.EditableClean, result.UpdatedSession!.DocumentState.State);
        Assert.False(result.UpdatedSession.DocumentState.IsDirty);
        Assert.Equal("[HTNK]\nStrength=500\n", result.UpdatedSession.DocumentState.OriginalText);
        Assert.Equal("[HTNK]\nStrength=500\n", result.UpdatedSession.DocumentState.CurrentText);
        Assert.Equal(Ra2IniNewLineKind.Lf, result.UpdatedSession.TextDocument.NewLineKind);
    }

    [Fact]
    public void Save_SuccessPreservesEncodingMetadataAndRevertBaseline()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[HTNK]\nStrength=400\n");
        Ra2EditorTextEncodingMetadata metadata = new(
            Ra2EditorTextEncodingKind.Utf8Bom,
            "UTF-8 BOM",
            hasBom: true);
        Ra2EditableDocumentSession session = _sessionService.StartEditing(
            sourcePath,
            "[HTNK]\nStrength=400\n",
            metadata);
        session = _sessionService.UpdateText(session, "[HTNK]\nStrength=500\n");
        Ra2SaveCurrentFileService service = new();

        Ra2SaveCurrentFileResult result = service.Save(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp);
        Ra2EditableDocumentSession reverted = _sessionService.Revert(result.UpdatedSession!);

        Assert.True(result.Success, result.Message);
        Assert.Same(metadata, result.UpdatedSession!.DocumentState.EncodingMetadata);
        Assert.Equal("[HTNK]\nStrength=500\n", reverted.DocumentState.CurrentText);
        Assert.Equal(Ra2EditorDocumentState.EditableClean, reverted.DocumentState.State);
    }

    [Fact]
    public void Save_BackupFailureDoesNotWriteOrClearDirty()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string missingPath = Path.Combine(workspace.Root, "missing.ini");
        Ra2EditableDocumentSession session = CreateDirtySession(missingPath, "[HTNK]\nStrength=500\n");
        Ra2SaveCurrentFileService service = new();

        Ra2SaveCurrentFileResult result = service.Save(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp);

        Assert.False(result.Success);
        Assert.Equal(Ra2SaveCurrentFileFailureKind.BackupFailed, result.FailureKind);
        Assert.True(result.DirtyShouldRemain);
        Assert.False(result.OriginalFileMayBeCorrupted);
        Assert.NotNull(result.RollbackResult);
        Assert.False(result.RollbackResult!.Attempted);
        Assert.Null(result.WriteResult);
        Assert.Same(session, result.UpdatedSession);
        Assert.Equal(Ra2EditorDocumentState.EditableDirty, result.UpdatedSession!.DocumentState.State);
        Assert.Equal("[HTNK]\nStrength=400\n", result.UpdatedSession.DocumentState.OriginalText);
        Assert.Equal("[HTNK]\nStrength=500\n", result.UpdatedSession.DocumentState.CurrentText);
        Assert.False(File.Exists(missingPath));
    }

    [Fact]
    public void Save_WriteFailureKeepsDirtyCurrentTextAndBackup()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[HTNK]\nStrength=400\n");
        Ra2EditableDocumentSession session = CreateDirtySession(
            sourcePath,
            "[HTNK]\nStrength=500\n");
        FailingWriter writer = new();
        Ra2SaveCurrentFileService service = new(
            new Ra2SaveCurrentFileOrchestrator(),
            writer,
            _sessionService);

        Ra2SaveCurrentFileResult result = service.Save(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp);

        Assert.False(result.Success);
        Assert.Equal(Ra2SaveCurrentFileFailureKind.WriteFailed, result.FailureKind);
        Assert.True(result.DirtyShouldRemain);
        Assert.False(result.OriginalFileMayBeCorrupted);
        Assert.NotNull(result.WriteResult);
        Assert.False(result.WriteResult!.Success);
        Assert.NotNull(result.RollbackResult);
        Assert.True(result.RollbackResult!.Attempted);
        Assert.True(result.RollbackResult.Success);
        Assert.Same(session, result.UpdatedSession);
        Assert.Equal("[HTNK]\nStrength=400\n", File.ReadAllText(sourcePath));
        Assert.True(File.Exists(result.BackupPlan!.BackupFilePath));
        Assert.Equal("[HTNK]\nStrength=400\n", File.ReadAllText(result.BackupPlan.BackupFilePath));
        Assert.Equal(Ra2EditorDocumentState.EditableDirty, result.UpdatedSession!.DocumentState.State);
        Assert.Equal("[HTNK]\nStrength=500\n", result.UpdatedSession.DocumentState.CurrentText);
    }

    [Fact]
    public void Save_SavePlanCannotSaveDoesNotBackupOrWrite()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[HTNK]\nStrength=400\n");
        Ra2EditableDocumentSession session = _sessionService.StartEditing(sourcePath, "[HTNK]\nStrength=400\n");
        RecordingWriter writer = new();
        Ra2SaveCurrentFileService service = new(
            new Ra2SaveCurrentFileOrchestrator(),
            writer,
            _sessionService);

        Ra2SaveCurrentFileResult result = service.Save(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp);

        Assert.False(result.Success);
        Assert.Equal(Ra2SaveCurrentFileFailureKind.SavePlanCannotSave, result.FailureKind);
        Assert.Equal(0, writer.CallCount);
        Assert.Null(result.BackupPlan);
        Assert.Null(result.BackupResult);
        Assert.Same(session, result.UpdatedSession);
        Assert.Equal("[HTNK]\nStrength=400\n", File.ReadAllText(sourcePath));
        Assert.False(Directory.Exists(Path.Combine(workspace.Root, "backup")));
    }

    [Fact]
    public void Save_WriteFailureAfterPartialWriteRestoresOriginalFileFromBackup()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[HTNK]\nStrength=400\n");
        Ra2EditableDocumentSession session = CreateDirtySession(
            sourcePath,
            "[HTNK]\nStrength=500\n");
        PartialFailingWriter writer = new("partial-corrupt");
        Ra2SaveCurrentFileService service = new(
            new Ra2SaveCurrentFileOrchestrator(),
            writer,
            _sessionService);

        Ra2SaveCurrentFileResult result = service.Save(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp);

        Assert.False(result.Success);
        Assert.Equal(Ra2SaveCurrentFileFailureKind.WriteFailed, result.FailureKind);
        Assert.True(result.RollbackResult!.Success);
        Assert.Equal("[HTNK]\nStrength=400\n", File.ReadAllText(sourcePath));
        Assert.Equal("[HTNK]\nStrength=500\n", result.UpdatedSession!.DocumentState.CurrentText);
        Assert.Equal(Ra2EditorDocumentState.EditableDirty, result.UpdatedSession.DocumentState.State);
    }

    [Fact]
    public void Save_WriteFailureWhenRollbackFailsMarksOriginalFileMayBeCorrupted()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[HTNK]\nStrength=400\n");
        Ra2EditableDocumentSession session = CreateDirtySession(
            sourcePath,
            "[HTNK]\nStrength=500\n");
        FailingWriter writer = new();
        FailingRollbackService rollbackService = new();
        Ra2SaveCurrentFileService service = new(
            new Ra2SaveCurrentFileOrchestrator(),
            writer,
            _sessionService,
            rollbackService);

        Ra2SaveCurrentFileResult result = service.Save(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp);

        Assert.False(result.Success);
        Assert.Equal(Ra2SaveCurrentFileFailureKind.RollbackFailed, result.FailureKind);
        Assert.True(result.DirtyShouldRemain);
        Assert.True(result.OriginalFileMayBeCorrupted);
        Assert.NotNull(result.RollbackResult);
        Assert.True(result.RollbackResult!.Attempted);
        Assert.False(result.RollbackResult.Success);
        Assert.Contains(result.BackupPlan!.BackupFilePath, result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("manual recovery", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("[HTNK]\nStrength=500\n", result.UpdatedSession!.DocumentState.CurrentText);
        Assert.Equal(Ra2EditorDocumentState.EditableDirty, result.UpdatedSession.DocumentState.State);
    }

    [Fact]
    public void Save_WriteFailureDoesNotCallMarkSaved()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[HTNK]\nStrength=400\n");
        Ra2EditableDocumentSession session = CreateDirtySession(sourcePath, "[HTNK]\nStrength=500\n");
        RecordingSessionService sessionService = new(_sessionService);
        Ra2SaveCurrentFileService service = new(
            new Ra2SaveCurrentFileOrchestrator(),
            new FailingWriter(),
            sessionService);

        Ra2SaveCurrentFileResult result = service.Save(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp);

        Assert.False(result.Success);
        Assert.Equal(0, sessionService.MarkSavedCallCount);
    }

    [Fact]
    public void Save_WriteSuccessDoesNotCallRollbackAndCallsMarkSaved()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[HTNK]\nStrength=400\n");
        Ra2EditableDocumentSession session = CreateDirtySession(sourcePath, "[HTNK]\nStrength=500\n");
        RecordingRollbackService rollbackService = new();
        RecordingSessionService sessionService = new(_sessionService);
        Ra2SaveCurrentFileService service = new(
            new Ra2SaveCurrentFileOrchestrator(),
            new Ra2TextFirstFileWriter(),
            sessionService,
            rollbackService);

        Ra2SaveCurrentFileResult result = service.Save(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp);

        Assert.True(result.Success, result.Message);
        Assert.Equal(0, rollbackService.CallCount);
        Assert.Equal(1, sessionService.MarkSavedCallCount);
        Assert.Equal(Ra2SaveCurrentFileFailureKind.None, result.FailureKind);
        Assert.False(result.DirtyShouldRemain);
        Assert.False(result.OriginalFileMayBeCorrupted);
    }

    [Fact]
    public void Save_TextFirstPreservesDuplicateSectionsKeysCommentsAndBlankLines()
    {
        using TestWorkspace workspace = TestWorkspace.Create();
        string sourcePath = workspace.WriteFile("rulesmd.ini", "[HTNK]\nStrength=400\n");
        string currentText = "; heading\n\n[DUP]\nStrength=100\nStrength=125\n\n[DUP]\n; duplicate section intentionally preserved\nName=Test\n";
        Ra2EditableDocumentSession session = CreateDirtySession(sourcePath, currentText);
        Ra2SaveCurrentFileService service = new();

        Ra2SaveCurrentFileResult result = service.Save(
            new Ra2SaveCurrentFilePlanRequest(session, isReadOnlyPreview: false),
            workspace.Root,
            FixedTimestamp);

        Assert.True(result.Success, result.Message);
        Assert.Equal(currentText, File.ReadAllText(sourcePath));
    }

    private Ra2EditableDocumentSession CreateDirtySession(string filePath, string currentText)
    {
        Ra2EditableDocumentSession session = _sessionService.StartEditing(filePath, "[HTNK]\nStrength=400\n");
        return _sessionService.UpdateText(session, currentText);
    }

    private sealed class FailingWriter : IRa2TextFirstFileWriter
    {
        public Ra2TextFileWriteResult Write(Ra2EditorSavePlan plan)
            => Ra2TextFileWriteResult.Failed("simulated writer failure", new IOException("simulated writer failure"));
    }

    private sealed class PartialFailingWriter : IRa2TextFirstFileWriter
    {
        private readonly string _partialText;

        public PartialFailingWriter(string partialText)
        {
            _partialText = partialText;
        }

        public Ra2TextFileWriteResult Write(Ra2EditorSavePlan plan)
        {
            File.WriteAllText(plan.FilePath, _partialText);
            return Ra2TextFileWriteResult.Failed("simulated partial writer failure", new IOException("simulated partial writer failure"));
        }
    }

    private sealed class RecordingWriter : IRa2TextFirstFileWriter
    {
        public int CallCount { get; private set; }

        public Ra2TextFileWriteResult Write(Ra2EditorSavePlan plan)
        {
            CallCount++;
            return Ra2TextFileWriteResult.Succeeded();
        }
    }

    private sealed class FailingRollbackService : IRa2SaveRollbackService
    {
        public Ra2RollbackResult RestoreFromBackup(Ra2BackupPlan backupPlan)
            => Ra2RollbackResult.Failed(
                backupPlan.BackupFilePath,
                backupPlan.SourceFilePath,
                $"simulated rollback failure. Backup path: {backupPlan.BackupFilePath}",
                new IOException("simulated rollback failure"));
    }

    private sealed class RecordingRollbackService : IRa2SaveRollbackService
    {
        public int CallCount { get; private set; }

        public Ra2RollbackResult RestoreFromBackup(Ra2BackupPlan backupPlan)
        {
            CallCount++;
            return Ra2RollbackResult.Succeeded(backupPlan.BackupFilePath, backupPlan.SourceFilePath);
        }
    }

    private sealed class RecordingSessionService : IRa2EditableDocumentSessionService
    {
        private readonly IRa2EditableDocumentSessionService _inner;

        public RecordingSessionService(IRa2EditableDocumentSessionService inner)
        {
            _inner = inner;
        }

        public int MarkSavedCallCount { get; private set; }

        public Ra2EditableDocumentSession StartEditing(string filePath, string text)
            => _inner.StartEditing(filePath, text);

        public Ra2EditableDocumentSession StartEditing(
            string filePath,
            string text,
            Ra2EditorTextEncodingMetadata encodingMetadata)
            => _inner.StartEditing(filePath, text, encodingMetadata);

        public Ra2EditableDocumentSession UpdateText(Ra2EditableDocumentSession session, string currentText)
            => _inner.UpdateText(session, currentText);

        public Ra2EditableDocumentSession MarkSaved(Ra2EditableDocumentSession session, string savedText)
        {
            MarkSavedCallCount++;
            return _inner.MarkSaved(session, savedText);
        }

        public Ra2EditableDocumentSession Revert(Ra2EditableDocumentSession session)
            => _inner.Revert(session);
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
