using RA2IniEditor.IDE.FieldAnnotations;
using System.IO;

namespace RA2IniEditor.IDE.ViewModels.FieldBrowser;

internal sealed class Ra2FieldAnnotationStatusViewModel
{
    public Ra2FieldAnnotationStatusViewModel(
        string statusText,
        bool isLoaded,
        bool hasWarnings,
        IReadOnlyList<string>? warnings = null)
    {
        StatusText = string.IsNullOrWhiteSpace(statusText) ? "Annotations: Unknown." : statusText;
        IsLoaded = isLoaded;
        HasWarnings = hasWarnings;
        Warnings = warnings ?? [];
    }

    public string StatusText { get; }

    public bool IsLoaded { get; }

    public bool HasWarnings { get; }

    public IReadOnlyList<string> Warnings { get; }

    public static Ra2FieldAnnotationStatusViewModel FromLoadResult(
        string path,
        Ra2FieldAnnotationLoadResult loadResult)
    {
        ArgumentNullException.ThrowIfNull(loadResult);

        string displayPath = ToDisplayPath(path);
        if (!loadResult.Success)
        {
            return new Ra2FieldAnnotationStatusViewModel(
                "字段注释：加载失败，已回退到字段库。",
                isLoaded: false,
                hasWarnings: true,
                loadResult.Warnings);
        }

        bool notFound = loadResult.Warnings.Any(warning =>
            warning.Contains("not found", StringComparison.OrdinalIgnoreCase));
        if (notFound)
        {
            return new Ra2FieldAnnotationStatusViewModel(
                "字段注释：未找到项目注释库，已回退到字段库。",
                isLoaded: false,
                hasWarnings: loadResult.Warnings.Count > 0,
                loadResult.Warnings);
        }

        string text = string.IsNullOrWhiteSpace(displayPath)
            ? "字段注释：已加载。"
            : $"字段注释：已加载 {displayPath}";
        return new Ra2FieldAnnotationStatusViewModel(
            text,
            isLoaded: true,
            hasWarnings: loadResult.Warnings.Count > 0,
            loadResult.Warnings);
    }

    private static string ToDisplayPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        string normalized = path.Replace('\\', '/');
        int markerIndex = normalized.LastIndexOf("/.ra2ide/", StringComparison.OrdinalIgnoreCase);
        if (markerIndex >= 0)
            return normalized[(markerIndex + 1)..];

        return Path.GetFileName(path);
    }
}
