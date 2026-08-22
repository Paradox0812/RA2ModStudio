using RA2IniEditor.IDE.Services;

namespace RA2IniEditor.IDE.Language;

/// <summary>
/// 表示一次与编辑器状态解耦的只读语言分析请求。
/// </summary>
internal sealed class Ra2LanguageAnalysisRequest
{
    internal Ra2LanguageAnalysisRequest(
        string projectRootPath,
        string filePath,
        string fileName,
        string text,
        int analysisVersion,
        Ra2FieldRegistryProviderSnapshot fieldRegistry)
    {
        ProjectRootPath = projectRootPath ?? throw new ArgumentNullException(nameof(projectRootPath));
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        Text = text ?? throw new ArgumentNullException(nameof(text));
        AnalysisVersion = analysisVersion;
        FieldRegistry = fieldRegistry ?? throw new ArgumentNullException(nameof(fieldRegistry));
    }

    public string ProjectRootPath { get; }

    public string FilePath { get; }

    public string FileName { get; }

    public string Text { get; }

    public int AnalysisVersion { get; }

    public Ra2FieldRegistryProviderSnapshot FieldRegistry { get; }
}
