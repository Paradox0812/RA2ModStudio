namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2RollbackResult
{
    private Ra2RollbackResult(
        bool attempted,
        bool success,
        string? restoredFromPath,
        string? restoredToPath,
        string message,
        Exception? exception)
    {
        Attempted = attempted;
        Success = success;
        RestoredFromPath = restoredFromPath;
        RestoredToPath = restoredToPath;
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Rollback result message cannot be empty.", nameof(message))
            : message;
        Exception = exception;
    }

    public bool Attempted { get; }

    public bool Success { get; }

    public string? RestoredFromPath { get; }

    public string? RestoredToPath { get; }

    public string Message { get; }

    public Exception? Exception { get; }

    public static Ra2RollbackResult NotAttempted(string message)
        => new(attempted: false, success: false, null, null, message, exception: null);

    public static Ra2RollbackResult Succeeded(string restoredFromPath, string restoredToPath)
        => new(
            attempted: true,
            success: true,
            restoredFromPath,
            restoredToPath,
            $"Rollback restored original file from backup: {restoredFromPath}",
            exception: null);

    public static Ra2RollbackResult Failed(
        string? restoredFromPath,
        string? restoredToPath,
        string message,
        Exception? exception = null)
        => new(
            attempted: true,
            success: false,
            restoredFromPath,
            restoredToPath,
            message,
            exception);
}
