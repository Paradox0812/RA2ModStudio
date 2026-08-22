using System.IO;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2BackupService : IRa2BackupService
{
    public Ra2BackupResult CreateBackup(Ra2BackupPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        if (!plan.CanBackup)
            return Ra2BackupResult.Failed(plan.Message);

        try
        {
            Directory.CreateDirectory(plan.BackupDirectory);
            File.Copy(plan.SourceFilePath, plan.BackupFilePath, overwrite: false);
            return Ra2BackupResult.Succeeded(plan.BackupFilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException or PathTooLongException)
        {
            return Ra2BackupResult.Failed($"Backup failed: {ex.Message}", ex);
        }
    }
}
