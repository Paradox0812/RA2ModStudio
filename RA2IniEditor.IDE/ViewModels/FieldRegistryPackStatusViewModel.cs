using System.IO;
using RA2IniEditor.IDE.Services;

namespace RA2IniEditor.IDE.ViewModels;

public sealed class FieldRegistryPackStatusViewModel
{
    public FieldRegistryPackStatusViewModel(FieldRegistryPackLoadStatus status)
    {
        Scope = status.Scope;
        DirectoryPath = status.DirectoryPath;
        DirectoryExists = status.DirectoryExists;
        FieldCount = status.FieldCount;
        WarningCount = status.WarningCount;
        StatusText = status.StatusText;
        ShortDirectoryPath = FormatShortPath(status.DirectoryPath);
        DirectoryPathToolTip = status.DirectoryPath;
        StatusChipText = $"{status.Scope}: {status.FieldCount} 字段 / {status.WarningCount} 警告";
    }

    public string Scope { get; }

    public string DirectoryPath { get; }

    public string ShortDirectoryPath { get; }

    public string DirectoryPathToolTip { get; }

    public bool DirectoryExists { get; }

    public int FieldCount { get; }

    public int WarningCount { get; }

    public string StatusText { get; }

    public string StatusChipText { get; }

    private static string FormatShortPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "未设置";

        if (path.Length <= 64)
            return path;

        try
        {
            string leaf = Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string? parent = Path.GetFileName(Path.GetDirectoryName(path));
            return string.IsNullOrWhiteSpace(parent)
                ? $"...\\{leaf}"
                : $"...\\{parent}\\{leaf}";
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return path.Length <= 64 ? path : $"...{path[^61..]}";
        }
    }
}
