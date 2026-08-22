namespace RA2IniEditor.IDE.Models;

/// <summary>
/// Describes an INI file discovered by the readonly IDE project opener.
/// </summary>
public sealed class ReadonlyIniFileDescriptor
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ReadonlyIniFileDescriptor"/> class.
    /// </summary>
    public ReadonlyIniFileDescriptor(string fileName, string filePath, long fileSizeBytes)
    {
        FileName = fileName;
        FilePath = filePath;
        FileSizeBytes = fileSizeBytes;
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
    /// Gets the file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; }
}
