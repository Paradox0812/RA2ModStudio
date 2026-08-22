namespace RA2IniEditor.IDE.Editing;

internal enum Ra2BackupPlanStatus
{
    CanBackup,
    SourceFileMissing,
    InvalidSourcePath,
    InvalidBackupPath,
    BackupDirectoryUnavailable,
    SavePlanCannotSave,
    UnknownFailure
}
