using System.IO;

namespace RA2IniEditor.IDE.FieldAnnotations;

internal sealed class Ra2FieldAnnotationPathService
{
    public string GetProjectAnnotationPath(string? projectRootPath, string language = "zh-CN")
    {
        if (string.IsNullOrWhiteSpace(projectRootPath))
            return string.Empty;

        string fileName = $"field-annotations.{NormalizeLanguage(language)}.json";
        return Path.Combine(projectRootPath, ".ra2ide", fileName);
    }

    private static string NormalizeLanguage(string language)
        => string.IsNullOrWhiteSpace(language) ? "zh-CN" : language.Trim();
}
