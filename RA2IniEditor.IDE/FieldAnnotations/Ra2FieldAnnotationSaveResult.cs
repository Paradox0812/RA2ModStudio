namespace RA2IniEditor.IDE.FieldAnnotations;

internal sealed class Ra2FieldAnnotationSaveResult
{
    private Ra2FieldAnnotationSaveResult(bool success, string? errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }

    public string? ErrorMessage { get; }

    public static Ra2FieldAnnotationSaveResult Succeeded()
        => new(true, null);

    public static Ra2FieldAnnotationSaveResult Failed(string errorMessage)
        => new(false, errorMessage);
}
