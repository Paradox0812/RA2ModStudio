namespace RA2IniEditor.Infrastructure.IO;

/// <summary>
/// INI 文本写入结果。失败时只承载错误信息，不负责回滚或 UI 展示。
/// </summary>
public sealed record IniTextWriteResult(
    bool Success,
    string FilePath,
    string ErrorMessage = "",
    Exception? Exception = null);
