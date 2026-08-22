using RA2IniEditor.Core;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

/// <summary>
/// 锁定 Core 解析器与 IDE Span 感知解析器当前可观察到的兼容性事实。
/// 这些测试描述现状，不表示其中任一路径应当被另一条路径替代。
/// </summary>
public sealed class Ra2IniParserConsistencyCharacterizationTests
{
    [Fact]
    public void Parse_SlashSlashComment_IsCoreCommentAndIdeRawLine()
    {
        const string text = "// full-line comment";

        IniLine coreLine = Assert.Single(IniParser.Parse(text).Lines);
        Ra2IniDocumentLine ideLine = Assert.Single(new Ra2IniTextDocumentParser().Parse(text).Lines);

        Assert.IsType<IniCommentLine>(coreLine);
        Assert.Equal(Ra2IniDocumentLineKind.Raw, ideLine.Kind);
    }

    [Fact]
    public void Parse_SectionWithUnsupportedTrailingText_IsCoreUnknownAndIdeSectionHeader()
    {
        const string text = "[E1] trailing text";

        IniLine coreLine = Assert.Single(IniParser.Parse(text).Lines);
        Ra2IniDocumentLine ideLine = Assert.Single(new Ra2IniTextDocumentParser().Parse(text).Lines);

        Assert.IsType<IniUnknownLine>(coreLine);
        Assert.Equal(Ra2IniDocumentLineKind.SectionHeader, ideLine.Kind);
        Assert.Equal("E1", ideLine.SectionName);
    }

    [Fact]
    public void Parse_ImmediateSemicolonComment_IsExcludedFromBothValues()
    {
        const string text = "[E1]\nPrimary=120mm; weapon comment";

        IniKeyValueLine coreLine = Assert.Single(IniParser.Parse(text).Lines.OfType<IniKeyValueLine>());
        Ra2IniDocumentLine ideLine = Assert.Single(new Ra2IniTextDocumentParser().Parse(text).KeyValues);

        Assert.Equal("120mm", coreLine.Value);
        Assert.Equal("; weapon comment", coreLine.InlineCommentSuffix);
        Assert.Equal("120mm", ideLine.Value);
        Assert.Equal("; weapon comment", ideLine.InlineComment);
    }

    [Fact]
    public void Parse_WhitespacePrefixedHashComment_RemainsInCoreValueAndIsIdeInlineComment()
    {
        const string text = "[E1]\nPrimary=120mm # weapon comment";

        IniKeyValueLine coreLine = Assert.Single(IniParser.Parse(text).Lines.OfType<IniKeyValueLine>());
        Ra2IniDocumentLine ideLine = Assert.Single(new Ra2IniTextDocumentParser().Parse(text).KeyValues);

        Assert.Equal("120mm # weapon comment", coreLine.Value);
        Assert.Equal(string.Empty, coreLine.InlineCommentSuffix);
        Assert.Equal("120mm", ideLine.Value);
        Assert.Equal("# weapon comment", ideLine.InlineComment);
    }

    [Fact]
    public void Parse_TrailingNewLine_ProducesCoreBlankLineButNoSyntheticIdeLine()
    {
        const string text = "[E1]\n";

        IniDocument coreDocument = IniParser.Parse(text);
        Ra2IniTextDocument ideDocument = new Ra2IniTextDocumentParser().Parse(text);

        Assert.Collection(
            coreDocument.Lines,
            line => Assert.IsType<IniSectionLine>(line),
            line => Assert.IsType<IniBlankLine>(line));
        Ra2IniDocumentLine ideLine = Assert.Single(ideDocument.Lines);
        Assert.Equal(Ra2IniDocumentLineKind.SectionHeader, ideLine.Kind);
        Assert.Equal("\n", ideLine.LineBreak);
    }

    [Fact]
    public void Parse_MixedNewLines_CoreUsesFirstDetectedKindAndIdeReportsMixed()
    {
        const string text = "[E1]\r\nPrimary=120mm\n";

        IniDocument coreDocument = IniParser.Parse(text);
        Ra2IniTextDocument ideDocument = new Ra2IniTextDocumentParser().Parse(text);

        Assert.Equal("\r\n", coreDocument.NewLine);
        Assert.Equal(Ra2IniNewLineKind.Mixed, ideDocument.NewLineKind);
    }

    [Fact]
    public void Parse_CoveredFieldComment_IsCoreCoveredKeyValueAndIdeComment()
    {
        const string text = """
            [E1]
            ; RA2IniEditor: covered by higher priority split INI: Strength=90
            """;

        IniDocument coreDocument = IniParser.Parse(text);
        Ra2IniTextDocument ideDocument = new Ra2IniTextDocumentParser().Parse(text);

        IniCoveredKeyValueLine coveredLine = Assert.Single(coreDocument.Lines.OfType<IniCoveredKeyValueLine>());
        Assert.Equal("Strength", coveredLine.Key);
        Assert.Equal("90", coveredLine.Value);
        Assert.Equal(Ra2IniDocumentLineKind.Comment, ideDocument.Lines[1].Kind);
    }

    [Fact]
    public void Parse_EmptyKey_IsCoreKeyValueAndIdeRawLine()
    {
        const string text = "[E1]\n=missingKey";

        IniDocument coreDocument = IniParser.Parse(text);
        Ra2IniTextDocument ideDocument = new Ra2IniTextDocumentParser().Parse(text);

        IniKeyValueLine coreLine = Assert.Single(coreDocument.Lines.OfType<IniKeyValueLine>());
        Assert.Equal(string.Empty, coreLine.Key);
        Assert.Equal("missingKey", coreLine.Value);
        Assert.Equal(Ra2IniDocumentLineKind.Raw, ideDocument.Lines[1].Kind);
    }
}
