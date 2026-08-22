using System.Text;

namespace RA2IniEditor.IDE.Editing;

internal sealed class Ra2EditorTextEncodingMetadataProvider
{
    public Ra2EditorTextEncodingMetadata FromEncoding(Encoding? encoding, bool hasBom)
    {
        if (encoding is null)
            return Ra2EditorTextEncodingMetadata.Unknown;

        return encoding.CodePage switch
        {
            65001 => hasBom
                ? new Ra2EditorTextEncodingMetadata(Ra2EditorTextEncodingKind.Utf8Bom, "UTF-8 BOM", true, encoding.WebName)
                : new Ra2EditorTextEncodingMetadata(Ra2EditorTextEncodingKind.Utf8, "UTF-8", false, encoding.WebName),
            1200 => new Ra2EditorTextEncodingMetadata(Ra2EditorTextEncodingKind.Utf16Le, "UTF-16 LE", hasBom, encoding.WebName),
            1201 => new Ra2EditorTextEncodingMetadata(Ra2EditorTextEncodingKind.Utf16Be, "UTF-16 BE", hasBom, encoding.WebName),
            _ => new Ra2EditorTextEncodingMetadata(
                Ra2EditorTextEncodingKind.SystemDefault,
                string.IsNullOrWhiteSpace(encoding.EncodingName) ? encoding.WebName : encoding.EncodingName,
                hasBom,
                encoding.WebName)
        };
    }
}
