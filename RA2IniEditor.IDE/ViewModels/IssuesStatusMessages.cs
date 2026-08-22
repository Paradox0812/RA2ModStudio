namespace RA2IniEditor.IDE.ViewModels;

/// <summary>
/// Centralized status text used by the readonly IDE Issues surface.
/// </summary>
public static class IssuesStatusMessages
{
    public const string NoFileSelected = "未选择文件。";
    public const string Pending = "正在准备诊断...";
    public const string NoIssuesFound = "未发现问题。";
    public const string SkippedDeferredLargeFile = "已跳过诊断：大文件预览已延迟加载。";
    public const string SkippedReadFailed = "已跳过诊断：文件读取失败。";
    public const string SkippedProjectFolderOpenFailed = "已跳过诊断：项目文件夹打开失败。";
    public const string SkippedSourceNotLoaded = "已跳过诊断：源文本尚未加载。";
    public const string Failed = "诊断失败。";
    public const string SkippedStaleResult = "已跳过诊断：结果已过期。";
    public const string ManualFullDiagnosticsPending = "正在运行手动全量诊断...";
    public const string IssuesCleared = "问题列表已清空。";
}
