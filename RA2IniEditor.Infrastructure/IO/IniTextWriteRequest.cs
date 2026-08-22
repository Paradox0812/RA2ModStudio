using System.Text;

namespace RA2IniEditor.Infrastructure.IO;

/// <summary>
/// INI 文本写入请求。只携带写入所需的路径、文本和编码，不携带 UI 或 dirty 状态。
/// </summary>
public sealed record IniTextWriteRequest(
    string FilePath,
    string Text,
    Encoding Encoding);
