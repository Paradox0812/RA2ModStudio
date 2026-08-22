namespace RA2IniEditor.IDE.Models;

/// <summary>
/// Represents a readonly INI file opened by the IDE shell.
/// </summary>
public sealed class OpenedIniFile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpenedIniFile"/> class.
    /// </summary>
    public OpenedIniFile(string fileName, string filePath, string text, string? encodingName, string? newLine)
    {
        FileName = fileName;
        FilePath = filePath;
        Text = text;
        EncodingName = encodingName;
        NewLine = newLine;
    }

    /// <summary>
    /// Gets the display file name.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the full source file path.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the readonly source text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the detected encoding name, when available.
    /// </summary>
    public string? EncodingName { get; }

    /// <summary>
    /// Gets the detected newline style, when available.
    /// </summary>
    public string? NewLine { get; }
}
