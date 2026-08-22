using System.IO;
using RA2IniEditor.IDE.Models;

namespace RA2IniEditor.IDE.Services;

/// <summary>
/// Opens an INI project folder in readonly mode for the IDE shell.
/// </summary>
public sealed class ProjectOpenService
{
    private static readonly string[] PreferredFileOrder =
    [
        "rulesmd.ini",
        "rules.ini",
        "artmd.ini",
        "art.ini",
        "aimd.ini",
        "ai.ini",
        "soundmd.ini",
        "sound.ini"
    ];

    /// <summary>
    /// Opens INI files from the specified folder in readonly mode.
    /// </summary>
    public ProjectOpenResult OpenFolderReadonly(string folderPath)
    {
        string[] files = Directory.EnumerateFiles(folderPath, "*.ini", SearchOption.TopDirectoryOnly)
            .OrderBy(GetFileOrder)
            .ThenBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        ReadonlyIniFileDescriptor[] descriptors = files
            .Select(filePath => new ReadonlyIniFileDescriptor(
                Path.GetFileName(filePath),
                filePath,
                new FileInfo(filePath).Length))
            .ToArray();

        return new ProjectOpenResult(folderPath, descriptors);
    }

    private static int GetFileOrder(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        int index = Array.FindIndex(
            PreferredFileOrder,
            preferred => string.Equals(preferred, fileName, StringComparison.OrdinalIgnoreCase));

        return index >= 0 ? index : PreferredFileOrder.Length;
    }
}
