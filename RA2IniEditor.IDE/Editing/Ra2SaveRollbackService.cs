using System.IO;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2SaveRollbackService : IRa2SaveRollbackService
{
    public Ra2RollbackResult RestoreFromBackup(Ra2BackupPlan backupPlan)
    {
        ArgumentNullException.ThrowIfNull(backupPlan);

        if (string.IsNullOrWhiteSpace(backupPlan.BackupFilePath) ||
            !File.Exists(backupPlan.BackupFilePath))
        {
            return Ra2RollbackResult.Failed(
                backupPlan.BackupFilePath,
                backupPlan.SourceFilePath,
                $"Rollback failed: backup file is missing. Backup path: {backupPlan.BackupFilePath}");
        }

        if (string.IsNullOrWhiteSpace(backupPlan.SourceFilePath))
        {
            return Ra2RollbackResult.Failed(
                backupPlan.BackupFilePath,
                backupPlan.SourceFilePath,
                $"Rollback failed: original file path is invalid. Backup path: {backupPlan.BackupFilePath}");
        }

        try
        {
            string? directory = Path.GetDirectoryName(backupPlan.SourceFilePath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);

            File.Copy(backupPlan.BackupFilePath, backupPlan.SourceFilePath, overwrite: true);
            return Ra2RollbackResult.Succeeded(backupPlan.BackupFilePath, backupPlan.SourceFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or PathTooLongException)
        {
            return Ra2RollbackResult.Failed(
                backupPlan.BackupFilePath,
                backupPlan.SourceFilePath,
                $"Rollback failed: {ex.Message}. Backup path: {backupPlan.BackupFilePath}",
                ex);
        }
    }
}
