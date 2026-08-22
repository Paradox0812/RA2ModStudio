namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2SaveCurrentFileResult
{
    private Ra2SaveCurrentFileResult(
        bool success,
        Ra2EditorSavePlan? savePlan,
        Ra2BackupPlan? backupPlan,
        Ra2BackupResult? backupResult,
        Ra2TextFileWriteResult? writeResult,
        Ra2RollbackResult? rollbackResult,
        Ra2EditableDocumentSession? updatedSession,
        Ra2SaveCurrentFileFailureKind failureKind,
        bool dirtyShouldRemain,
        bool originalFileMayBeCorrupted,
        string message)
    {
        Success = success;
        SavePlan = savePlan;
        BackupPlan = backupPlan;
        BackupResult = backupResult;
        WriteResult = writeResult;
        RollbackResult = rollbackResult;
        UpdatedSession = updatedSession;
        FailureKind = failureKind;
        DirtyShouldRemain = dirtyShouldRemain;
        OriginalFileMayBeCorrupted = originalFileMayBeCorrupted;
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Save current file result message cannot be empty.", nameof(message))
            : message;
    }

    public bool Success { get; }

    public Ra2EditorSavePlan? SavePlan { get; }

    public Ra2BackupPlan? BackupPlan { get; }

    public Ra2BackupResult? BackupResult { get; }

    public Ra2TextFileWriteResult? WriteResult { get; }

    public Ra2RollbackResult? RollbackResult { get; }

    public Ra2EditableDocumentSession? UpdatedSession { get; }

    public Ra2SaveCurrentFileFailureKind FailureKind { get; }

    public bool DirtyShouldRemain { get; }

    public bool OriginalFileMayBeCorrupted { get; }

    public string Message { get; }

    public static Ra2SaveCurrentFileResult NotReady(
        Ra2SaveCurrentFileOrchestrationResult orchestration,
        Ra2EditableDocumentSession? session)
        => new(
            success: false,
            orchestration.SavePlan,
            orchestration.BackupPlan,
            orchestration.BackupResult,
            writeResult: null,
            rollbackResult: Ra2RollbackResult.NotAttempted("Rollback was not attempted because the save was not ready to write."),
            session,
            ResolveNotReadyFailureKind(orchestration),
            dirtyShouldRemain: session?.DocumentState.IsDirty ?? false,
            originalFileMayBeCorrupted: false,
            orchestration.Message);

    public static Ra2SaveCurrentFileResult WriteFailed(
        Ra2SaveCurrentFileOrchestrationResult orchestration,
        Ra2TextFileWriteResult writeResult,
        Ra2RollbackResult rollbackResult,
        Ra2EditableDocumentSession? session)
    {
        bool rollbackSucceeded = rollbackResult.Attempted && rollbackResult.Success;
        string backupPath = orchestration.BackupPlan?.BackupFilePath ?? string.Empty;
        string message = rollbackSucceeded
            ? $"Save failed, rollback succeeded. Backup path: {backupPath}. Write error: {writeResult.Message}"
            : $"Save failed and rollback failed. Original file may require manual recovery from backup: {backupPath}. Write error: {writeResult.Message}. Rollback error: {rollbackResult.Message}";

        return new(
            success: false,
            orchestration.SavePlan,
            orchestration.BackupPlan,
            orchestration.BackupResult,
            writeResult,
            rollbackResult,
            session,
            rollbackSucceeded
                ? Ra2SaveCurrentFileFailureKind.WriteFailed
                : Ra2SaveCurrentFileFailureKind.RollbackFailed,
            dirtyShouldRemain: session?.DocumentState.IsDirty ?? false,
            originalFileMayBeCorrupted: !rollbackSucceeded,
            message);
    }

    public static Ra2SaveCurrentFileResult WriteFailedWithoutRollback(
        Ra2SaveCurrentFileOrchestrationResult orchestration,
        Ra2TextFileWriteResult writeResult,
        Ra2EditableDocumentSession? session)
        => new(
            success: false,
            orchestration.SavePlan,
            orchestration.BackupPlan,
            orchestration.BackupResult,
            writeResult,
            rollbackResult: Ra2RollbackResult.NotAttempted("Rollback was not attempted because no valid backup plan was available."),
            session,
            Ra2SaveCurrentFileFailureKind.WriteFailed,
            dirtyShouldRemain: session?.DocumentState.IsDirty ?? false,
            originalFileMayBeCorrupted: true,
            writeResult.Message);

    public static Ra2SaveCurrentFileResult Succeeded(
        Ra2SaveCurrentFileOrchestrationResult orchestration,
        Ra2TextFileWriteResult writeResult,
        Ra2EditableDocumentSession updatedSession)
        => new(
            success: true,
            orchestration.SavePlan,
            orchestration.BackupPlan,
            orchestration.BackupResult,
            writeResult,
            rollbackResult: Ra2RollbackResult.NotAttempted("Rollback was not attempted because save completed successfully."),
            updatedSession,
            Ra2SaveCurrentFileFailureKind.None,
            dirtyShouldRemain: false,
            originalFileMayBeCorrupted: false,
            "Save current file completed.");

    private static Ra2SaveCurrentFileFailureKind ResolveNotReadyFailureKind(
        Ra2SaveCurrentFileOrchestrationResult orchestration)
        => orchestration.Status switch
        {
            Ra2SaveCurrentFileOrchestrationStatus.SavePlanCannotSave => Ra2SaveCurrentFileFailureKind.SavePlanCannotSave,
            Ra2SaveCurrentFileOrchestrationStatus.BackupPlanCannotBackup => Ra2SaveCurrentFileFailureKind.BackupFailed,
            Ra2SaveCurrentFileOrchestrationStatus.BackupFailed => Ra2SaveCurrentFileFailureKind.BackupFailed,
            _ => Ra2SaveCurrentFileFailureKind.WriteFailed
        };
}
