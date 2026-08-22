using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2IniTextDocumentNewLineTests
{
    [Theory]
    [InlineData("[A]\nKey=1\n", (int)Ra2IniNewLineKind.Lf)]
    [InlineData("[A]\r\nKey=1\r\n", (int)Ra2IniNewLineKind.CrLf)]
    [InlineData("[A]\rKey=1\r", (int)Ra2IniNewLineKind.Cr)]
    [InlineData("[A]\nKey=1\r\n", (int)Ra2IniNewLineKind.Mixed)]
    [InlineData("[A]", (int)Ra2IniNewLineKind.Unknown)]
    public void Parse_DetectsDocumentNewLineKind(string text, int expected)
    {
        Ra2IniTextDocument document = new Ra2IniTextDocumentParser().Parse(text);

        Assert.Equal((Ra2IniNewLineKind)expected, document.NewLineKind);
    }

    [Fact]
    public void Parse_PreservesLineBreakPerLine()
    {
        const string text = "[A]\r\nKey=1\nRaw\r";

        Ra2IniTextDocument document = new Ra2IniTextDocumentParser().Parse(text);

        Assert.Equal(["\r\n", "\n", "\r"], document.Lines.Select(line => line.LineBreak).ToArray());
    }
}
