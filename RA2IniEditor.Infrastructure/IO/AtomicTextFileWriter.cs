using System.Text;

namespace RA2IniEditor.Infrastructure.IO;

/// <summary>
/// 同目录临时文件原子写入器，保持 legacy 保存的替换和 fallback 语义。
/// </summary>
internal static class AtomicTextFileWriter
{
    public static void WriteText(string filePath, string text, Encoding encoding)
        => WriteAtomically(filePath, text, encoding);

    /// <summary>以同目录临时文件和替换操作提交完整文本，供非编辑器持久化场景复用。</summary>
    public static void WriteAtomically(string filePath, string text, Encoding encoding)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        string tempPath = Path.Combine(directory ?? Environment.CurrentDirectory, $".{Path.GetFileName(filePath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            File.WriteAllText(tempPath, text, encoding);
            if (File.Exists(filePath))
            {
                try
                {
                    File.Replace(tempPath, filePath, null, ignoreMetadataErrors: true);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or PlatformNotSupportedException)
                {
                    File.Move(tempPath, filePath, overwrite: true);
                }
            }
            else
            {
                File.Move(tempPath, filePath);
            }
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch
                {
                    // 临时文件清理失败不能覆盖真正的写入异常。
                }
            }
        }
    }
}
