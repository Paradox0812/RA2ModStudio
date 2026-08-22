namespace RA2IniEditor.IDE.AI;

/// <summary>
/// 单次 AI 请求在构建出站 prompt 时发生的安全处理事实。
/// </summary>
[Flags]
internal enum Ra2AiRequestPreparationFlags
{
    None = 0,
    SensitiveContentRedacted = 1,
    SelectedTextTruncated = 2,
    ContextTruncated = 4,
    TotalPromptTruncated = 8
}
