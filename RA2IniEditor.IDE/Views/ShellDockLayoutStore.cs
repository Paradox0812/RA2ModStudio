using System.IO;
using System.Security;
using System.Text;
using System.Xml;
using RA2IniEditor.Infrastructure.IO;

namespace RA2IniEditor.IDE.Views;

/// <summary>管理仅属于当前 Windows 用户的 v2 Dock 展示布局文件及只读 v1 迁移源。</summary>
internal sealed class ShellDockLayoutStore
{
    internal const int MaximumFileLength = 1024 * 1024;
    internal const string LayoutFileName = "shell-layout.v2.xml";
    internal const string InvalidLayoutFileName = "shell-layout.v2.invalid.xml";
    internal const string LegacyLayoutFileName = "shell-layout.v1.xml";
    internal const string LegacyInvalidLayoutFileName = "shell-layout.v1.invalid.xml";

    private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    private readonly string _layoutPath;
    private readonly string _invalidLayoutPath;
    private readonly string _legacyLayoutPath;
    private readonly string _legacyInvalidLayoutPath;
    private readonly Action<string, string, Encoding> _atomicWrite;

    public ShellDockLayoutStore()
        : this(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "RA2IniEditor",
            "IDE",
            "Layout"))
    {
    }

    internal ShellDockLayoutStore(
        string layoutDirectory,
        Action<string, string, Encoding>? atomicWrite = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(layoutDirectory);
        _layoutPath = Path.Combine(layoutDirectory, LayoutFileName);
        _invalidLayoutPath = Path.Combine(layoutDirectory, InvalidLayoutFileName);
        _legacyLayoutPath = Path.Combine(layoutDirectory, LegacyLayoutFileName);
        _legacyInvalidLayoutPath = Path.Combine(layoutDirectory, LegacyInvalidLayoutFileName);
        _atomicWrite = atomicWrite ?? AtomicTextFileWriter.WriteAtomically;
    }

    public ShellDockLayoutOperationResult TryRead(out string? serialized)
        => TryRead(_layoutPath, out serialized);

    public ShellDockLayoutOperationResult TryReadLegacy(out string? serialized)
        => TryRead(_legacyLayoutPath, out serialized);

    private static ShellDockLayoutOperationResult TryRead(string layoutPath, out string? serialized)
    {
        serialized = null;
        try
        {
            if (!File.Exists(layoutPath))
                return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.NotFound);

            FileInfo file = new(layoutPath);
            if (file.Length > MaximumFileLength)
                return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.TooLarge);

            byte[] bytes = File.ReadAllBytes(layoutPath);
            if (bytes.Length > MaximumFileLength)
                return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.TooLarge);
            if (bytes.AsSpan().StartsWith(Encoding.UTF8.Preamble))
                return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.UnsafeXml);

            string text;
            try
            {
                text = StrictUtf8.GetString(bytes);
            }
            catch (DecoderFallbackException)
            {
                return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.UnsafeXml);
            }

            ShellDockLayoutOperationResult validation = ValidateSafeXml(text);
            if (!validation.Succeeded)
                return validation;

            serialized = text;
            return ShellDockLayoutOperationResult.Success;
        }
        catch (Exception ex) when (IsBoundedIoFailure(ex))
        {
            return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.IoFailure);
        }
    }

    public ShellDockLayoutOperationResult TryWrite(string serialized)
    {
        if (serialized is null)
            return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.UnsafeXml);

        int byteCount;
        try
        {
            byteCount = StrictUtf8.GetByteCount(serialized);
        }
        catch (EncoderFallbackException)
        {
            return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.UnsafeXml);
        }

        if (byteCount > MaximumFileLength)
            return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.TooLarge);

        ShellDockLayoutOperationResult validation = ValidateSafeXml(serialized);
        if (!validation.Succeeded)
            return validation;

        try
        {
            _atomicWrite(_layoutPath, serialized, StrictUtf8);
            return ShellDockLayoutOperationResult.Success;
        }
        catch (Exception ex) when (IsBoundedIoFailure(ex))
        {
            return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.IoFailure);
        }
    }

    public ShellDockLayoutOperationResult TryQuarantine()
        => TryQuarantine(_layoutPath, _invalidLayoutPath);

    public ShellDockLayoutOperationResult TryQuarantineLegacy()
        => TryQuarantine(_legacyLayoutPath, _legacyInvalidLayoutPath);

    private static ShellDockLayoutOperationResult TryQuarantine(string layoutPath, string invalidLayoutPath)
    {
        try
        {
            if (!File.Exists(layoutPath))
                return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.NotFound);

            string? directory = Path.GetDirectoryName(invalidLayoutPath);
            if (!string.IsNullOrWhiteSpace(directory))
                Directory.CreateDirectory(directory);
            File.Move(layoutPath, invalidLayoutPath, overwrite: true);
            return ShellDockLayoutOperationResult.Success;
        }
        catch (Exception ex) when (IsBoundedIoFailure(ex))
        {
            return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.IoFailure);
        }
    }

    internal static ShellDockLayoutOperationResult ValidateSafeXml(string serialized)
    {
        try
        {
            XmlReaderSettings settings = new()
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = MaximumFileLength,
                IgnoreComments = false,
                IgnoreWhitespace = false
            };
            using StringReader textReader = new(serialized);
            using XmlReader reader = XmlReader.Create(textReader, settings);
            bool foundRoot = false;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.XmlDeclaration)
                {
                    string? encoding = reader.GetAttribute("encoding");
                    if (!string.IsNullOrWhiteSpace(encoding) &&
                        !string.Equals(encoding, "utf-8", StringComparison.OrdinalIgnoreCase))
                        return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.UnsafeXml);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Depth == 0)
                {
                    if (foundRoot || !string.Equals(reader.LocalName, "LayoutRoot", StringComparison.Ordinal))
                        return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.UnsafeXml);
                    foundRoot = true;
                }
            }

            return foundRoot
                ? ShellDockLayoutOperationResult.Success
                : new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.UnsafeXml);
        }
        catch (XmlException)
        {
            return new ShellDockLayoutOperationResult(false, ShellDockLayoutFailureKind.UnsafeXml);
        }
    }

    private static bool IsBoundedIoFailure(Exception exception)
        => exception is IOException or UnauthorizedAccessException or SecurityException or NotSupportedException;
}
