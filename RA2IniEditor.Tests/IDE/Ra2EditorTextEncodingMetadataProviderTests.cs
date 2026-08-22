using System.Text;
using RA2IniEditor.IDE.Editing;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditorTextEncodingMetadataProviderTests
{
    private readonly Ra2EditorTextEncodingMetadataProvider _provider = new();

    [Fact]
    public void FromEncoding_NullReturnsUnknown()
    {
        Ra2EditorTextEncodingMetadata metadata = _provider.FromEncoding(null, hasBom: false);

        Assert.Same(Ra2EditorTextEncodingMetadata.Unknown, metadata);
    }

    [Fact]
    public void FromEncoding_Utf8WithoutBomReturnsUtf8()
    {
        Ra2EditorTextEncodingMetadata metadata = _provider.FromEncoding(new UTF8Encoding(false), hasBom: false);

        Assert.Equal(Ra2EditorTextEncodingKind.Utf8, metadata.Kind);
        Assert.False(metadata.HasBom);
        Assert.Equal("UTF-8", metadata.DisplayName);
    }

    [Fact]
    public void FromEncoding_Utf8WithBomReturnsUtf8Bom()
    {
        Ra2EditorTextEncodingMetadata metadata = _provider.FromEncoding(new UTF8Encoding(true), hasBom: true);

        Assert.Equal(Ra2EditorTextEncodingKind.Utf8Bom, metadata.Kind);
        Assert.True(metadata.HasBom);
        Assert.Equal("UTF-8 BOM", metadata.DisplayName);
    }

    [Fact]
    public void FromEncoding_Utf16LeReturnsUtf16Le()
    {
        Ra2EditorTextEncodingMetadata metadata = _provider.FromEncoding(Encoding.Unicode, hasBom: true);

        Assert.Equal(Ra2EditorTextEncodingKind.Utf16Le, metadata.Kind);
        Assert.True(metadata.HasBom);
        Assert.Equal("UTF-16 LE", metadata.DisplayName);
    }

    [Fact]
    public void FromEncoding_Utf16BeReturnsUtf16Be()
    {
        Ra2EditorTextEncodingMetadata metadata = _provider.FromEncoding(Encoding.BigEndianUnicode, hasBom: true);

        Assert.Equal(Ra2EditorTextEncodingKind.Utf16Be, metadata.Kind);
        Assert.True(metadata.HasBom);
        Assert.Equal("UTF-16 BE", metadata.DisplayName);
    }

    [Fact]
    public void FromEncoding_LegacyCodePageReturnsSystemDefault()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        Encoding gb18030 = Encoding.GetEncoding("GB18030");

        Ra2EditorTextEncodingMetadata metadata = _provider.FromEncoding(gb18030, hasBom: false);

        Assert.Equal(Ra2EditorTextEncodingKind.SystemDefault, metadata.Kind);
        Assert.False(metadata.HasBom);
        Assert.False(string.IsNullOrWhiteSpace(metadata.DisplayName));
        Assert.Equal(gb18030.WebName, metadata.CodePageName);
    }
}
