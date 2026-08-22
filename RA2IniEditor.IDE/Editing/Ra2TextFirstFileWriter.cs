using System.Text;
using RA2IniEditor.Infrastructure.IO;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2TextFirstFileWriter : IRa2TextFirstFileWriter
{
    private readonly IIniFileStore _fileStore;

    public Ra2TextFirstFileWriter()
        : this(new IniFileStore())
    {
    }

    public Ra2TextFirstFileWriter(IIniFileStore fileStore)
    {
        _fileStore = fileStore ?? throw new ArgumentNullException(nameof(fileStore));
    }

    public Ra2TextFileWriteResult Write(Ra2EditorSavePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        Encoding encoding = ResolveEncoding(plan.EncodingMetadata);
        IniTextWriteResult result = _fileStore.WriteText(plan.FilePath, plan.Text, encoding);
        return result.Success
            ? Ra2TextFileWriteResult.Succeeded()
            : Ra2TextFileWriteResult.Failed(
                string.IsNullOrWhiteSpace(result.ErrorMessage)
                    ? "Text-first INI file write failed."
                    : result.ErrorMessage,
                result.Exception);
    }

    private static Encoding ResolveEncoding(Ra2EditorTextEncodingMetadata metadata)
    {
        return metadata.Kind switch
        {
            Ra2EditorTextEncodingKind.Utf8Bom => new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
            Ra2EditorTextEncodingKind.Utf16Le => Encoding.Unicode,
            Ra2EditorTextEncodingKind.Utf16Be => Encoding.BigEndianUnicode,
            Ra2EditorTextEncodingKind.SystemDefault => ResolveSystemDefaultEncoding(metadata),
            _ => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)
        };
    }

    private static Encoding ResolveSystemDefaultEncoding(Ra2EditorTextEncodingMetadata metadata)
    {
        if (!string.IsNullOrWhiteSpace(metadata.CodePageName))
        {
            try
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding(metadata.CodePageName);
            }
            catch (ArgumentException)
            {
            }
        }

        return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    }
}
