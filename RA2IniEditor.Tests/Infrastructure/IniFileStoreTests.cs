using System.Text;
using RA2IniEditor.Infrastructure.IO;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class IniFileStoreTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly IniFileStore _store = new();

    public IniFileStoreTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), $"RA2IniEditorTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDirectory);
    }

    [Fact]
    public void ReadText_ReturnsTextFilePathEncodingAndNewLine()
    {
        string path = Path.Combine(_tempDirectory, "rules.ini");
        string text = "[Tank]\r\nName=Grizzly";
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        File.WriteAllText(path, text, encoding);

        IniTextReadResult result = _store.ReadText(path);

        Assert.Equal(path, result.FilePath);
        Assert.Equal(text, result.Text);
        Assert.Equal("utf-8", result.Encoding.WebName);
        Assert.Empty(result.Encoding.GetPreamble());
        Assert.Equal("\r\n", result.NewLine);
    }

    [Theory]
    [InlineData("[Tank]\nName=Grizzly", "\n")]
    [InlineData("[Tank]\rName=Grizzly", "\r")]
    public void ReadText_DetectsLfAndCrNewLines(string text, string expectedNewLine)
    {
        string path = Path.Combine(_tempDirectory, $"{Guid.NewGuid():N}.ini");
        File.WriteAllText(path, text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        IniTextReadResult result = _store.ReadText(path);

        Assert.Equal(text, result.Text);
        Assert.Equal(expectedNewLine, result.NewLine);
    }

    [Fact]
    public void ReadText_WhenUtf8BomIsPresent_DoesNotIncludeBomCharacterInText()
    {
        string path = Path.Combine(_tempDirectory, "rules.ini");
        File.WriteAllBytes(path, new byte[] { 0xEF, 0xBB, 0xBF, (byte)'[', (byte)'A', (byte)']' });

        IniTextReadResult result = _store.ReadText(path);

        Assert.Equal("[A]", result.Text);
        Assert.DoesNotContain('\uFEFF', result.Text);
        Assert.Equal(3, result.Encoding.GetPreamble().Length);
    }

    [Fact]
    public void ReadText_WhenUtf16LittleEndianBomIsPresent_ReturnsDecodedTextWithoutBom()
    {
        string path = Path.Combine(_tempDirectory, "utf16le.ini");
        byte[] payload = Encoding.Unicode.GetBytes("[A]");
        File.WriteAllBytes(path, Encoding.Unicode.GetPreamble().Concat(payload).ToArray());

        IniTextReadResult result = _store.ReadText(path);

        Assert.Equal("[A]", result.Text);
        Assert.DoesNotContain('\uFEFF', result.Text);
        Assert.Equal(Encoding.Unicode.CodePage, result.Encoding.CodePage);
    }

    [Fact]
    public void ReadText_WhenUtf16BigEndianBomIsPresent_ReturnsDecodedTextWithoutBom()
    {
        string path = Path.Combine(_tempDirectory, "utf16be.ini");
        byte[] payload = Encoding.BigEndianUnicode.GetBytes("[A]");
        File.WriteAllBytes(path, Encoding.BigEndianUnicode.GetPreamble().Concat(payload).ToArray());

        IniTextReadResult result = _store.ReadText(path);

        Assert.Equal("[A]", result.Text);
        Assert.DoesNotContain('\uFEFF', result.Text);
        Assert.Equal(Encoding.BigEndianUnicode.CodePage, result.Encoding.CodePage);
    }

    [Fact]
    public void WriteText_UsesProvidedEncodingAndReturnsSuccess()
    {
        string path = Path.Combine(_tempDirectory, "rules.ini");
        Encoding encoding = Encoding.Unicode;

        IniTextWriteResult result = _store.WriteText(path, "[Tank]\nName=Grizzly", encoding);
        byte[] bytes = File.ReadAllBytes(path);
        byte[] preamble = encoding.GetPreamble();

        Assert.True(result.Success);
        Assert.Equal(path, result.FilePath);
        Assert.Equal(preamble, bytes[..preamble.Length]);
        Assert.Equal("[Tank]\nName=Grizzly", encoding.GetString(bytes[preamble.Length..]));
    }

    [Fact]
    public void WriteText_WhenTargetIsExistingDirectory_ReturnsFailureResult()
    {
        string directoryPath = Path.Combine(_tempDirectory, "ExistingDirectory");
        Directory.CreateDirectory(directoryPath);

        IniTextWriteResult result = _store.WriteText(directoryPath, "text", Encoding.UTF8);

        Assert.False(result.Success);
        Assert.Equal(directoryPath, result.FilePath);
        Assert.NotEmpty(result.ErrorMessage);
        Assert.NotNull(result.Exception);
        Assert.Equal(result.Exception.Message, result.ErrorMessage);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDirectory))
            Directory.Delete(_tempDirectory, recursive: true);
    }
}
