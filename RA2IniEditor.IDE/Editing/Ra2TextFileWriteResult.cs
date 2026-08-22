namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2TextFileWriteResult
{
    private Ra2TextFileWriteResult(bool success, string message, Exception? exception)
    {
        Success = success;
        Message = string.IsNullOrWhiteSpace(message)
            ? throw new ArgumentException("Text file write result message cannot be empty.", nameof(message))
            : message;
        Exception = exception;
    }

    public bool Success { get; }

    public string Message { get; }

    public Exception? Exception { get; }

    public static Ra2TextFileWriteResult Succeeded()
        => new(success: true, "Text-first INI file write completed.", exception: null);

    public static Ra2TextFileWriteResult Failed(string message, Exception? exception = null)
        => new(success: false, message, exception);
}
