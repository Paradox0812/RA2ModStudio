using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2IniTextDocumentParserTests
{
    [Fact]
    public void Parse_RecognizesBasicLineKinds()
    {
        const string text = "\n; comment\n[NEWINF]\nStrength=200\nnot a valid line\n";

        Ra2IniTextDocument document = new Ra2IniTextDocumentParser().Parse(text);

        Assert.Equal(
            [
                Ra2IniDocumentLineKind.Blank,
                Ra2IniDocumentLineKind.Comment,
                Ra2IniDocumentLineKind.SectionHeader,
                Ra2IniDocumentLineKind.KeyValue,
                Ra2IniDocumentLineKind.Raw
            ],
            document.Lines.Select(line => line.Kind).ToArray());
    }

    [Fact]
    public void Parse_SectionHeaderPreservesTextAndInlineComment()
    {
        const string text = "   [NEWINF]   ; infantry section";

        Ra2IniDocumentLine line = Assert.Single(new Ra2IniTextDocumentParser().Parse(text).Lines);

        Assert.Equal(Ra2IniDocumentLineKind.SectionHeader, line.Kind);
        Assert.Equal(text, line.Text);
        Assert.Equal("NEWINF", line.SectionName);
        Assert.Equal("; infantry section", line.InlineComment);
    }

    [Fact]
    public void Parse_KeyValueTrimsTokensButPreservesOriginalTextAndInlineComment()
    {
        const string text = "  Primary = 120mm ; weapon comment";

        Ra2IniDocumentLine line = Assert.Single(new Ra2IniTextDocumentParser().Parse(text).Lines);

        Assert.Equal(Ra2IniDocumentLineKind.KeyValue, line.Kind);
        Assert.Equal(text, line.Text);
        Assert.Equal("Primary", line.Key);
        Assert.Equal("120mm", line.Value);
        Assert.Equal("; weapon comment", line.InlineComment);
    }

    [Theory]
    [InlineData("[broken")]
    [InlineData("=missingKey")]
    [InlineData("not a valid line without equals")]
    public void Parse_RawLinesDoNotThrow(string text)
    {
        Ra2IniDocumentLine line = Assert.Single(new Ra2IniTextDocumentParser().Parse(text).Lines);

        Assert.Equal(Ra2IniDocumentLineKind.Raw, line.Kind);
        Assert.Equal(text, line.Text);
    }

    [Fact]
    public void Parse_EmptyTextDoesNotThrow()
    {
        Ra2IniTextDocument document = new Ra2IniTextDocumentParser().Parse(string.Empty);

        Assert.Empty(document.Lines);
        Assert.Equal(Ra2IniNewLineKind.Unknown, document.NewLineKind);
        Assert.Equal(string.Empty, document.Text);
    }

    [Fact]
    public void Parse_DuplicateSectionsAndKeysAreNotMerged()
    {
        const string text = """
            [120mm]
            Damage=90
            Damage=100

            [120mm]
            ROF=60
            """;

        Ra2IniTextDocument document = new Ra2IniTextDocumentParser().Parse(text);

        Assert.Equal(2, document.SectionHeaders.Count(line => line.SectionName == "120mm"));
        Assert.Equal(2, document.KeyValues.Count(line => line.Key == "Damage"));
        Assert.Single(document.KeyValues, line => line.Key == "ROF");
    }

    [Fact]
    public void Parse_LineTextAndLineBreakCanReconstructOriginalText()
    {
        const string text = "[A]\r\nKey=1\n; comment\rRaw";

        Ra2IniTextDocument document = new Ra2IniTextDocumentParser().Parse(text);
        string reconstructed = string.Concat(document.Lines.Select(line => line.Text + line.LineBreak));

        Assert.Equal(text, reconstructed);
    }
}
