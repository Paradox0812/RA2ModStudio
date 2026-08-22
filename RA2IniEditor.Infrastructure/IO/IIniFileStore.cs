using System.Text;

namespace RA2IniEditor.Infrastructure.IO;

/// <summary>
/// INI 文本文件读写契约。该接口只描述文本 IO 边界，不负责结构化保存、备份、dirty 状态或 UI 提示。
/// </summary>
public interface IIniFileStore
{
    /// <summary>
    /// 读取 INI 文件文本，并返回编码、换行符和路径等元数据。
    /// </summary>
    IniTextReadResult ReadText(string path);

    /// <summary>
    /// 写入 INI 文件文本。具体实现必须保持调用方传入的编码，并在未来复用原子写入语义。
    /// </summary>
    IniTextWriteResult WriteText(string path, string text, Encoding encoding);
}
