namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2BackupResult
{
    private Ra2BackupResult(
        bool success,
        string? backupFilePath,
        string message,
        Exception? exception)
    {
        Success = success;
        BackupFilePath = backupFilePath;
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Backup result message cannot be empty.", nameof(message))
            : message;
        Exception = exception;
    }

    public bool Success { get; }

    public string? BackupFilePath { get; }

    public string Message { get; }

    public Exception? Exception { get; }

    public static Ra2BackupResult Succeeded(string backupFilePath)
        => new(true, backupFilePath, "Backup file was created.", null);

    public static Ra2BackupResult Failed(string message, Exception? exception = null)
        => new(false, null, message, exception);
}
