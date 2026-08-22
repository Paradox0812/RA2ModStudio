using RA2IniEditor.Core;
using Xunit;

namespace RA2IniEditor.Tests.Core;

public sealed class IniParserSerializerRoundTripTests
{
    [Fact]
    public void Parse_PreservesCommentBlankUnknownSectionKeyValueInlineCommentAndCoveredLine()
    {
        const string text = "; top comment\r\n\r\n[Tank]\r\nName=Grizzly ; display name\r\nunknown text\r\n; RA2IniEditor: covered by higher priority split INI: Strength=90";

        IniDocument document = IniParser.Parse(text, "C:\\mod\\rules.ini");

        Assert.Equal("\r\n", document.NewLine);
        Assert.Equal(text, document.OriginalText);
        Assert.Equal("C:\\mod\\rules.ini", document.FilePath);
        Assert.Collection(
            document.Lines,
            line => Assert.IsType<IniCommentLine>(line),
            line => Assert.IsType<IniBlankLine>(line),
            line =>
            {
                IniSectionLine section = Assert.IsType<IniSectionLine>(line);
                Assert.Equal("Tank", section.SectionName);
            },
            line =>
            {
                IniKeyValueLine keyValue = Assert.IsType<IniKeyValueLine>(line);
                Assert.Equal("Tank", keyValue.SectionName);
                Assert.Equal("Name", keyValue.Key);
                Assert.Equal("Grizzly", keyValue.Value);
                Assert.Equal(" ; display name", keyValue.InlineCommentSuffix);
            },
            line =>
            {
                IniUnknownLine unknown = Assert.IsType<IniUnknownLine>(line);
                Assert.Equal("unknown text", unknown.RawText);
            },
            line =>
            {
                IniCoveredKeyValueLine covered = Assert.IsType<IniCoveredKeyValueLine>(line);
                Assert.True(covered.IsCovered);
                Assert.Equal("Strength", covered.Key);
                Assert.Equal("90", covered.Value);
                Assert.Equal("covered by higher priority split INI", covered.CoverReason);
            });

        IniSection section = Assert.Single(document.Sections);
        Assert.Equal("Tank", section.Name);
        Assert.Equal(2, section.KeyValues.Count);
        Assert.IsType<IniCoveredKeyValueLine>(section.KeyValues[1]);
    }

    [Fact]
    public void Serialize_AfterParse_ReturnsCurrentRoundTripText()
    {
        const string text = "; top comment\r\n\r\n[Tank]\r\nName=Grizzly ; display name\r\nunknown text\r\n; RA2IniEditor: covered by higher priority split INI: Strength=90";

        IniDocument document = IniParser.Parse(text);

        Assert.Equal(text, IniSerializer.Serialize(document));
    }

    [Fact]
    public void Serialize_AfterParse_PreservesTrailingNewLineByCurrentSplitBehavior()
    {
        const string text = "[Tank]\nName=Grizzly\n";

        IniDocument document = IniParser.Parse(text);

        Assert.Equal(text, IniSerializer.Serialize(document));
    }
}
