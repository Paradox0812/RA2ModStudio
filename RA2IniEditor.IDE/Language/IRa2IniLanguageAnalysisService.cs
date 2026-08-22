namespace RA2IniEditor.IDE.Language;

/// <summary>
/// 提供与 UI 状态解耦的单文档只读语言分析。
/// </summary>
internal interface IRa2IniLanguageAnalysisService
{
    Ra2IniLanguageAnalysisResult Analyze(Ra2LanguageAnalysisRequest request);
}
