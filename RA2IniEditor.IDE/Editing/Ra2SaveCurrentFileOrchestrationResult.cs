namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2SaveCurrentFileOrchestrationResult
{
    private Ra2SaveCurrentFileOrchestrationResult(
        bool success,
        bool readyToWrite,
        Ra2SaveCurrentFileOrchestrationStage stage,
        Ra2SaveCurrentFileOrchestrationStatus status,
        Ra2EditorSavePlan? savePlan,
        Ra2BackupPlan? backupPlan,
        Ra2BackupResult? backupResult,
        string message)
    {
        Success = success;
        ReadyToWrite = readyToWrite;
        Stage = stage;
        Status = status;
        SavePlan = savePlan;
        BackupPlan = backupPlan;
        BackupResult = backupResult;
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Orchestration result message cannot be empty.", nameof(message))
            : message;
    }

    public bool Success { get; }

    public bool ReadyToWrite { get; }

    public Ra2SaveCurrentFileOrchestrationStage Stage { get; }

    public Ra2SaveCurrentFileOrchestrationStatus Status { get; }

    public Ra2EditorSavePlan? SavePlan { get; }

    public Ra2BackupPlan? BackupPlan { get; }

    public Ra2BackupResult? BackupResult { get; }

    public string Message { get; }

    public static Ra2SaveCurrentFileOrchestrationResult SavePlanCannotSave(Ra2EditorSavePlan savePlan)
        => new(
            success: false,
            readyToWrite: false,
            Ra2SaveCurrentFileOrchestrationStage.SavePlanBuilt,
            Ra2SaveCurrentFileOrchestrationStatus.SavePlanCannotSave,
            savePlan,
            backupPlan: null,
            backupResult: null,
            savePlan.Message);

    public static Ra2SaveCurrentFileOrchestrationResult BackupPlanCannotBackup(
        Ra2EditorSavePlan savePlan,
        Ra2BackupPlan backupPlan)
        => new(
            success: false,
            readyToWrite: false,
            Ra2SaveCurrentFileOrchestrationStage.BackupPlanBuilt,
            Ra2SaveCurrentFileOrchestrationStatus.BackupPlanCannotBackup,
            savePlan,
            backupPlan,
            backupResult: null,
            backupPlan.Message);

    public static Ra2SaveCurrentFileOrchestrationResult StoppedBeforeWrite(
        Ra2EditorSavePlan savePlan,
        Ra2BackupPlan backupPlan)
        => new(
            success: true,
            readyToWrite: false,
            Ra2SaveCurrentFileOrchestrationStage.StoppedBeforeWrite,
            Ra2SaveCurrentFileOrchestrationStatus.StoppedBeforeWrite,
            savePlan,
            backupPlan,
            backupResult: null,
            "Save current file orchestration stopped before backup and write.");

    public static Ra2SaveCurrentFileOrchestrationResult ReadyForWrite(
        Ra2EditorSavePlan savePlan,
        Ra2BackupPlan backupPlan,
        Ra2BackupResult backupResult)
        => new(
            success: true,
            readyToWrite: true,
            Ra2SaveCurrentFileOrchestrationStage.BackupCompleted,
            Ra2SaveCurrentFileOrchestrationStatus.ReadyToWrite,
            savePlan,
            backupPlan,
            backupResult,
            "Backup completed. The save operation is ready for the future write stage.");

    public static Ra2SaveCurrentFileOrchestrationResult BackupFailed(
        Ra2EditorSavePlan savePlan,
        Ra2BackupPlan backupPlan,
        Ra2BackupResult backupResult)
        => new(
            success: false,
            readyToWrite: false,
            Ra2SaveCurrentFileOrchestrationStage.FailedBeforeWrite,
            Ra2SaveCurrentFileOrchestrationStatus.BackupFailed,
            savePlan,
            backupPlan,
            backupResult,
            backupResult.Message);
}
