using RA2IniEditor.IDE.Models;

namespace RA2IniEditor.IDE.ViewModels;

/// <summary>
/// Represents a lightweight INI file item shown in the file switcher.
/// </summary>
public sealed class SourceFileItemViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SourceFileItemViewModel"/> class.
    /// </summary>
    public SourceFileItemViewModel(ReadonlyIniFileDescriptor descriptor)
    {
        FileName = descriptor.FileName;
        FilePath = descriptor.FilePath;
        FileSizeBytes = descriptor.FileSizeBytes;
        DisplaySize = ReadonlyIniContentResult.FormatBytes(descriptor.FileSizeBytes);
    }

    /// <summary>
    /// Gets the display file name.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the full file path, when available.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public long FileSizeBytes { get; }

    /// <summary>
    /// Gets the concise display size.
    /// </summary>
    public string DisplaySize { get; }

    /// <summary>
    /// Converts the item back to a readonly file descriptor.
    /// </summary>
    public ReadonlyIniFileDescriptor ToDescriptor() => new(FileName, FilePath, FileSizeBytes);
}
