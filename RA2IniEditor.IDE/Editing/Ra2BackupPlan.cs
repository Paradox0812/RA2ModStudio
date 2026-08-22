namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2BackupPlan
{
    public Ra2BackupPlan(
        string sourceFilePath,
        string backupFilePath,
        string backupDirectory,
        Ra2BackupPlanStatus status,
        string message)
    {
        SourceFilePath = sourceFilePath ?? string.Empty;
        BackupFilePath = backupFilePath ?? string.Empty;
        BackupDirectory = backupDirectory ?? string.Empty;
        Status = status;
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Backup plan message cannot be empty.", nameof(message))
            : message;
    }

    public string SourceFilePath { get; }

    public string BackupFilePath { get; }

    public string BackupDirectory { get; }

    public Ra2BackupPlanStatus Status { get; }

    public bool CanBackup => Status == Ra2BackupPlanStatus.CanBackup;

    public string Message { get; }
}
