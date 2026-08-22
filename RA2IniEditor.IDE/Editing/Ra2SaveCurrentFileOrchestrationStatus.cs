namespace RA2IniEditor.IDE.Editing;

internal enum Ra2SaveCurrentFileOrchestrationStatus
{
    ReadyToWrite,
    SavePlanCannotSave,
    BackupPlanCannotBackup,
    BackupFailed,
    StoppedBeforeWrite,
    UnknownFailure
}
