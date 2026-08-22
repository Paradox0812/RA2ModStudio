using RA2IniEditor.IDE.Editing;

namespace RA2IniEditor.IDE.Models;

/// <summary>
/// Represents readonly content loaded for a selected INI file.
/// </summary>
public sealed class ReadonlyIniContentResult
{
    private ReadonlyIniContentResult(
        string fileName,
        string filePath,
        string text,
        string metadataText,
        bool isLargeFileDeferred,
        string? errorMessage,
        Ra2EditorTextEncodingMetadata? encodingMetadata)
    {
        FileName = fileName;
        FilePath = filePath;
        Text = text;
        MetadataText = metadataText;
        IsLargeFileDeferred = isLargeFileDeferred;
        ErrorMessage = errorMessage;
        EncodingMetadata = encodingMetadata ?? Ra2EditorTextEncodingMetadata.Unknown;
    }

    /// <summary>
    /// Gets the display file name.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the full file path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets readonly text or user-facing status text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets metadata text for the source editor.
    /// </summary>
    public string MetadataText { get; }

    /// <summary>
    /// Gets a value indicating whether full preview was deferred due to file size.
    /// </summary>
    public bool IsLargeFileDeferred { get; }

    /// <summary>
    /// Gets the error message when loading failed.
    /// </summary>
    public string? ErrorMessage { get; }

    internal Ra2EditorTextEncodingMetadata EncodingMetadata { get; }

    /// <summary>
    /// Creates a successful readonly content result.
    /// </summary>
    public static ReadonlyIniContentResult Loaded(string fileName, string filePath, string text, string metadataText)
        => Loaded(fileName, filePath, text, metadataText, Ra2EditorTextEncodingMetadata.Unknown);

    internal static ReadonlyIniContentResult Loaded(
        string fileName,
        string filePath,
        string text,
        string metadataText,
        Ra2EditorTextEncodingMetadata encodingMetadata)
        => new(fileName, filePath, text, metadataText, false, null, encodingMetadata);

    /// <summary>
    /// Creates a result that defers full preview for a very large file.
    /// </summary>
    public static ReadonlyIniContentResult LargeFileDeferred(string fileName, string filePath, long fileSizeBytes)
        => new(
            fileName,
            filePath,
            $"This INI file is large. Full preview is deferred in this version.{Environment.NewLine}{Environment.NewLine}File: {fileName}{Environment.NewLine}Size: {FormatBytes(fileSizeBytes)}",
            $"Large file | {FormatBytes(fileSizeBytes)}",
            true,
            null,
            Ra2EditorTextEncodingMetadata.Unknown);

    /// <summary>
    /// Creates a failed readonly content result.
    /// </summary>
    public static ReadonlyIniContentResult Failed(string fileName, string filePath, string errorMessage)
        => new(
            fileName,
            filePath,
            $"Failed to read {fileName}:{Environment.NewLine}{Environment.NewLine}{errorMessage}",
            "Read failed",
            false,
            errorMessage,
            Ra2EditorTextEncodingMetadata.Unknown);

    /// <summary>
    /// Formats a byte size for concise display.
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        if (bytes >= 1024 * 1024)
            return $"{bytes / 1024d / 1024d:0.##} MB";

        if (bytes >= 1024)
            return $"{bytes / 1024d:0.##} KB";

        return $"{bytes} B";
    }
}
