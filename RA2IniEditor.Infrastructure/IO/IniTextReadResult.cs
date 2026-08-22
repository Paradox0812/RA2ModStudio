using System.Text;

namespace RA2IniEditor.Infrastructure.IO;

/// <summary>
/// INI 文本读取结果，供未来 Source Editor 保留原始文本、编码和换行信息。
/// </summary>
public sealed record IniTextReadResult(
    string FilePath,
    string Text,
    Encoding Encoding,
    string NewLine);
