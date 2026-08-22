using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2IniTextDocumentLineSpanTests
{
    [Fact]
    public void Parse_SectionNameSpanCoversOnlySectionNameInOriginalText()
    {
        const string text = "prefix\n   [ NEWINF ] ; comment";

        Ra2IniDocumentLine line = new Ra2IniTextDocumentParser().Parse(text).Lines[1];

        Assert.NotNull(line.SectionNameSpan);
        Assert.Equal("NEWINF", Slice(text, line.SectionNameSpan.Value));
    }

    [Fact]
    public void Parse_KeyValueSpansCoverOriginalTokens()
    {
        const string text = "[NEWINF]\n  Primary = 120mm ; comment";

        Ra2IniDocumentLine line = new Ra2IniTextDocumentParser().Parse(text).Lines[1];

        Assert.NotNull(line.KeySpan);
        Assert.NotNull(line.ValueSpan);
        Assert.NotNull(line.InlineCommentSpan);
        Assert.Equal("Primary", Slice(text, line.KeySpan.Value));
        Assert.Equal("120mm", Slice(text, line.ValueSpan.Value));
        Assert.Equal("; comment", Slice(text, line.InlineCommentSpan.Value));
    }

    [Fact]
    public void Parse_LineSpanIsBasedOnOriginalTextAndExcludesLineBreak()
    {
        const string text = "[A]\r\n  Key=Value\r\n";

        Ra2IniTextDocument document = new Ra2IniTextDocumentParser().Parse(text);

        Assert.Equal(0, document.Lines[0].Span.Start);
        Assert.Equal(3, document.Lines[0].Span.Length);
        Assert.Equal("[A]", Slice(text, document.Lines[0].Span));
        Assert.Equal(5, document.Lines[1].Span.Start);
        Assert.Equal("  Key=Value", Slice(text, document.Lines[1].Span));
    }

    private static string Slice(string text, RA2IniEditor.Application.Language.Ra2TextSpan span)
        => text.Substring(span.Start, span.Length);
}
