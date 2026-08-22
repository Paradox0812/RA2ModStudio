using System.Text;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.Infrastructure.IO;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditorEncodingMetadataAdapterTests
{
    private readonly Ra2EditorEncodingMetadataAdapter _adapter = new();

    [Fact]
    public void FromReadResult_NullReturnsUnknown()
    {
        Ra2EditorTextEncodingMetadata metadata = _adapter.FromReadResult(null);

        Assert.Same(Ra2EditorTextEncodingMetadata.Unknown, metadata);
    }

    [Fact]
    public void FromReadResult_MapsUtf8BomFromEncodingPreamble()
    {
        IniTextReadResult readResult = new("rules.ini", "[Rules]", new UTF8Encoding(true), "\n");

        Ra2EditorTextEncodingMetadata metadata = _adapter.FromReadResult(readResult);

        Assert.Equal(Ra2EditorTextEncodingKind.Utf8Bom, metadata.Kind);
        Assert.True(metadata.HasBom);
    }

    [Fact]
    public void FromReadResult_MapsUtf16Le()
    {
        IniTextReadResult readResult = new("rules.ini", "[Rules]", Encoding.Unicode, "\n");

        Ra2EditorTextEncodingMetadata metadata = _adapter.FromReadResult(readResult);

        Assert.Equal(Ra2EditorTextEncodingKind.Utf16Le, metadata.Kind);
    }
}
