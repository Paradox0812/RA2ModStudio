namespace RA2IniEditor.IDE.Editing;

internal enum Ra2SaveCurrentFileFailureKind
{
    None,
    SavePlanCannotSave,
    BackupFailed,
    WriteFailed,
    RollbackFailed
}
