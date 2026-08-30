using System.IO;

namespace RA2IniEditor.IDE.Startup;

internal static class Ra2LaunchRequestParser
{
    internal const string AutomationOpenFolderArgument = "--automation-open-folder";

    public static Ra2LaunchRequest Parse(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Count == 0)
            return Ra2LaunchRequest.None();

        if (args.Count == 1)
        {
            string argument = args[0] ?? string.Empty;
            return argument.StartsWith("--", StringComparison.Ordinal)
                ? Ra2LaunchRequest.Invalid($"无法识别启动参数：{argument}")
                : ParseIniFile(argument);
        }

        if (args.Count != 2)
            return Ra2LaunchRequest.Invalid("一次启动只支持打开一个 INI 文件或一个项目文件夹。");

        string option = args[0] ?? string.Empty;
        if (string.Equals(option, AutomationOpenFolderArgument, StringComparison.OrdinalIgnoreCase))
            return ParseProjectFolder(args[1]);

        return Ra2LaunchRequest.Invalid($"无法识别启动参数：{option}");
    }

    private static Ra2LaunchRequest ParseProjectFolder(string? rawPath)
    {
        if (!TryNormalizePath(rawPath, out string? fullPath, out string? failure))
            return Ra2LaunchRequest.Invalid(failure!);
        if (!Directory.Exists(fullPath))
            return Ra2LaunchRequest.Invalid($"项目文件夹不存在：{fullPath}");

        return Ra2LaunchRequest.ProjectFolder(fullPath);
    }

    private static Ra2LaunchRequest ParseIniFile(string? rawPath)
    {
        if (!TryNormalizePath(rawPath, out string? fullPath, out string? failure))
            return Ra2LaunchRequest.Invalid(failure!);
        if (!string.Equals(Path.GetExtension(fullPath), ".ini", StringComparison.OrdinalIgnoreCase))
            return Ra2LaunchRequest.Invalid("启动目标必须是 .ini 文件。");
        if (!File.Exists(fullPath))
            return Ra2LaunchRequest.Invalid($"INI 文件不存在：{fullPath}");

        string? projectFolderPath = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrWhiteSpace(projectFolderPath) || !Directory.Exists(projectFolderPath))
            return Ra2LaunchRequest.Invalid("无法确定 INI 文件所在的项目文件夹。");

        return Ra2LaunchRequest.IniFile(projectFolderPath, fullPath);
    }

    private static bool TryNormalizePath(
        string? rawPath,
        out string? fullPath,
        out string? failure)
    {
        fullPath = null;
        failure = null;
        if (string.IsNullOrWhiteSpace(rawPath))
        {
            failure = "启动路径不能为空。";
            return false;
        }

        try
        {
            string value = rawPath.Trim();
            if (value.Length >= 2 && value[0] == '"' && value[^1] == '"')
                value = value[1..^1];
            fullPath = Path.GetFullPath(value);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or NotSupportedException or PathTooLongException)
        {
            failure = "启动路径格式无效。";
            return false;
        }
    }
}
