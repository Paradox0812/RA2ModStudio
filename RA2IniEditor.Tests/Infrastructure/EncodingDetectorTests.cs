using System.Text;
using RA2IniEditor.Infrastructure.IO;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class EncodingDetectorTests
{
    static EncodingDetectorTests()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [Fact]
    public void DetectEncoding_WhenUtf8Bom_ReturnsUtf8WithPreamble()
    {
        byte[] bytes = { 0xEF, 0xBB, 0xBF, (byte)'A' };

        Encoding encoding = EncodingDetector.DetectEncoding(bytes);

        Assert.Equal("utf-8", encoding.WebName);
        Assert.Equal(3, encoding.GetPreamble().Length);
    }

    [Fact]
    public void DetectEncoding_WhenUtf8WithoutBom_ReturnsUtf8WithoutPreamble()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("中文");

        Encoding encoding = EncodingDetector.DetectEncoding(bytes);

        Assert.Equal("utf-8", encoding.WebName);
        Assert.Empty(encoding.GetPreamble());
    }

    [Fact]
    public void DetectEncoding_WhenUtf16LittleEndianBom_ReturnsUnicode()
    {
        byte[] bytes = { 0xFF, 0xFE, 0x41, 0x00 };

        Encoding encoding = EncodingDetector.DetectEncoding(bytes);

        Assert.Equal(Encoding.Unicode.CodePage, encoding.CodePage);
    }

    [Fact]
    public void DetectEncoding_WhenUtf16BigEndianBom_ReturnsBigEndianUnicode()
    {
        byte[] bytes = { 0xFE, 0xFF, 0x00, 0x41 };

        Encoding encoding = EncodingDetector.DetectEncoding(bytes);

        Assert.Equal(Encoding.BigEndianUnicode.CodePage, encoding.CodePage);
    }

    [Fact]
    public void DetectEncoding_WhenChineseBytesAreNotValidUtf8_FallsBackToGb18030Or936()
    {
        Encoding gb18030 = Encoding.GetEncoding("GB18030");
        byte[] bytes = gb18030.GetBytes("中文");

        Encoding encoding = EncodingDetector.DetectEncoding(bytes);
        string text = encoding.GetString(EncodingDetector.SkipBom(bytes));

        Assert.Contains(encoding.CodePage, new[] { 54936, 936 });
        Assert.Equal("中文", text);
    }

    [Fact]
    public void SkipBom_WhenUtf8BomIsPresent_DecodedTextDoesNotContainBomCharacter()
    {
        byte[] bytes = { 0xEF, 0xBB, 0xBF, (byte)'A' };
        Encoding encoding = EncodingDetector.DetectEncoding(bytes);

        string text = encoding.GetString(EncodingDetector.SkipBom(bytes));

        Assert.Equal("A", text);
        Assert.DoesNotContain('\uFEFF', text);
    }

    [Fact]
    public void SkipBom_WhenUtf16LittleEndianBomIsPresent_DecodedTextDoesNotContainBomCharacter()
    {
        byte[] bytes = { 0xFF, 0xFE, 0x41, 0x00 };
        Encoding encoding = EncodingDetector.DetectEncoding(bytes);

        string text = encoding.GetString(EncodingDetector.SkipBom(bytes));

        Assert.Equal("A", text);
        Assert.DoesNotContain('\uFEFF', text);
    }

    [Fact]
    public void SkipBom_WhenUtf16BigEndianBomIsPresent_DecodedTextDoesNotContainBomCharacter()
    {
        byte[] bytes = { 0xFE, 0xFF, 0x00, 0x41 };
        Encoding encoding = EncodingDetector.DetectEncoding(bytes);

        string text = encoding.GetString(EncodingDetector.SkipBom(bytes));

        Assert.Equal("A", text);
        Assert.DoesNotContain('\uFEFF', text);
    }
}
