namespace RA2IniEditor.IDE.Editing;

internal interface IRa2SaveRollbackService
{
    Ra2RollbackResult RestoreFromBackup(Ra2BackupPlan backupPlan);
}
