namespace RA2IniEditor.IDE.Editing;

internal enum Ra2SaveCurrentFileOrchestrationStage
{
    None,
    SavePlanBuilt,
    BackupPlanBuilt,
    BackupCompleted,
    StoppedBeforeWrite,
    FailedBeforeWrite
}
