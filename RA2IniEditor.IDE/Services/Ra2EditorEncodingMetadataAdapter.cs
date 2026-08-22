using RA2IniEditor.IDE.Editing;
using RA2IniEditor.Infrastructure.IO;

namespace RA2IniEditor.IDE.Services;

internal sealed class Ra2EditorEncodingMetadataAdapter
{
    private readonly Ra2EditorTextEncodingMetadataProvider _provider;

    public Ra2EditorEncodingMetadataAdapter()
        : this(new Ra2EditorTextEncodingMetadataProvider())
    {
    }

    public Ra2EditorEncodingMetadataAdapter(Ra2EditorTextEncodingMetadataProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public Ra2EditorTextEncodingMetadata FromReadResult(IniTextReadResult? readResult)
    {
        if (readResult is null)
            return Ra2EditorTextEncodingMetadata.Unknown;

        bool hasBom = readResult.Encoding.GetPreamble().Length > 0;
        return _provider.FromEncoding(readResult.Encoding, hasBom);
    }
}
