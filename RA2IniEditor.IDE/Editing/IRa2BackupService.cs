namespace RA2IniEditor.IDE.Editing;

internal interface IRa2BackupService
{
    Ra2BackupResult CreateBackup(Ra2BackupPlan plan);
}
