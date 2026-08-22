using System.Text;

namespace RA2IniEditor.Infrastructure.IO;

/// <summary>
/// INI 文件编码检测器，保留 legacy 读取顺序和中文编码 fallback。
/// </summary>
internal static class EncodingDetector
{
    static EncodingDetector()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public static Encoding DetectEncoding(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);

        if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
            return Encoding.Unicode;

        if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
            return Encoding.BigEndianUnicode;

        byte[] payload = SkipBom(bytes);
        if (IsValidUtf8(payload))
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        return GetLegacyChineseEncoding();
    }

    public static byte[] SkipBom(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            return bytes[3..];

        if (bytes.Length >= 2 && ((bytes[0] == 0xFF && bytes[1] == 0xFE) || (bytes[0] == 0xFE && bytes[1] == 0xFF)))
            return bytes[2..];

        return bytes;
    }

    private static bool IsValidUtf8(byte[] bytes)
    {
        try
        {
            _ = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true).GetString(bytes);
            return true;
        }
        catch (DecoderFallbackException)
        {
            return false;
        }
    }

    private static Encoding GetLegacyChineseEncoding()
    {
        try
        {
            return Encoding.GetEncoding("GB18030");
        }
        catch (ArgumentException)
        {
            return Encoding.GetEncoding(936);
        }
    }
}
