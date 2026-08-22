using System.Text;

namespace RA2IniEditor.Infrastructure.IO;

/// <summary>
/// 基于真实文件系统的 INI 文本 IO 实现。不负责备份、结构化保存或 UI 状态。
/// </summary>
public sealed class IniFileStore : IIniFileStore
{
    public IniTextReadResult ReadText(string path)
    {
        byte[] bytes = File.ReadAllBytes(path);
        Encoding encoding = EncodingDetector.DetectEncoding(bytes);
        string text = encoding.GetString(EncodingDetector.SkipBom(bytes));
        return new IniTextReadResult(path, text, encoding, DetectNewLine(text));
    }

    public IniTextWriteResult WriteText(string path, string text, Encoding encoding)
    {
        try
        {
            AtomicTextFileWriter.WriteText(path, text, encoding);
            return new IniTextWriteResult(true, path);
        }
        catch (Exception ex)
        {
            return new IniTextWriteResult(false, path, ex.Message, ex);
        }
    }

    private static string DetectNewLine(string text)
    {
        int crlfIndex = text.IndexOf("\r\n", StringComparison.Ordinal);
        int lfIndex = text.IndexOf('\n');
        int crIndex = text.IndexOf('\r');

        if (crlfIndex >= 0 && (lfIndex < 0 || crlfIndex <= lfIndex) && (crIndex < 0 || crlfIndex <= crIndex))
            return "\r\n";

        if (lfIndex >= 0 && (crIndex < 0 || lfIndex < crIndex))
            return "\n";

        return crIndex >= 0 ? "\r" : Environment.NewLine;
    }
}
