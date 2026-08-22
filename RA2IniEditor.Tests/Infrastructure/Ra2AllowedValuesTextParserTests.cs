using RA2IniEditor.Infrastructure.FieldRegistry.Harvest.Ini;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class Ra2AllowedValuesTextParserTests
{
    private readonly Ra2AllowedValuesTextParser _parser = new();

    [Fact]
    public void Parse_SingleValueReturnsOneValue()
    {
        Ra2AllowedValuesTextParseResult result = _parser.Parse("light");

        Assert.Single(result.Values);
        Assert.Equal("light", result.Values[0].Value);
        Assert.Null(result.Values[0].DisplayName);
        Assert.Null(result.Values[0].Description);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Parse_ValueWithDisplayNameReturnsDisplayName()
    {
        Ra2AllowedValuesTextParseResult result = _parser.Parse("light|Light armor");

        Assert.Single(result.Values);
        Assert.Equal("light", result.Values[0].Value);
        Assert.Equal("Light armor", result.Values[0].DisplayName);
        Assert.Null(result.Values[0].Description);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Parse_ValueWithDescriptionReturnsDescription()
    {
        Ra2AllowedValuesTextParseResult result = _parser.Parse("light|Light armor|Fast units");

        Assert.Single(result.Values);
        Assert.Equal("light", result.Values[0].Value);
        Assert.Equal("Light armor", result.Values[0].DisplayName);
        Assert.Equal("Fast units", result.Values[0].Description);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Parse_ExtraPipePartsJoinIntoDescription()
    {
        Ra2AllowedValuesTextParseResult result = _parser.Parse("light|Light armor|Fast|fragile");

        Assert.Single(result.Values);
        Assert.Equal("Fast|fragile", result.Values[0].Description);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Parse_SemicolonSeparatedValuesReturnsAllValues()
    {
        Ra2AllowedValuesTextParseResult result = _parser.Parse("light;medium;heavy");

        Assert.Equal(["light", "medium", "heavy"], result.Values.Select(value => value.Value).ToArray());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Parse_NewlineSeparatedValuesReturnsAllValues()
    {
        Ra2AllowedValuesTextParseResult result = _parser.Parse("light\nmedium\nheavy");

        Assert.Equal(["light", "medium", "heavy"], result.Values.Select(value => value.Value).ToArray());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Parse_CrlfSeparatedValuesDoesNotCreateFalseEmptyWarnings()
    {
        Ra2AllowedValuesTextParseResult result = _parser.Parse("light\r\nmedium\r\nheavy");

        Assert.Equal(["light", "medium", "heavy"], result.Values.Select(value => value.Value).ToArray());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Parse_EmptyEntryBetweenSemicolonsWarnsAndSkipsEntry()
    {
        Ra2AllowedValuesTextParseResult result = _parser.Parse("light;;heavy");

        Assert.Equal(["light", "heavy"], result.Values.Select(value => value.Value).ToArray());
        string warning = Assert.Single(result.Warnings);
        Assert.Contains("Empty", warning, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_TrailingSemicolonWarnsAndSkipsEntry()
    {
        Ra2AllowedValuesTextParseResult result = _parser.Parse("light;");

        Assert.Single(result.Values);
        string warning = Assert.Single(result.Warnings);
        Assert.Contains("Empty", warning, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_DuplicateValueWarnsAndKeepsFirstValue()
    {
        Ra2AllowedValuesTextParseResult result = _parser.Parse("light;LIGHT");

        Ra2AllowedValuesTextParseResult expectedFirst = _parser.Parse("light");
        Assert.Single(result.Values);
        Assert.Equal(expectedFirst.Values[0].Value, result.Values[0].Value);
        string warning = Assert.Single(result.Warnings);
        Assert.Contains("Duplicate", warning, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_ChineseDisplayAndDescriptionIsSupported()
    {
        Ra2AllowedValuesTextParseResult result = _parser.Parse("light|轻甲|轻型装甲");

        Assert.Single(result.Values);
        Assert.Equal("light", result.Values[0].Value);
        Assert.Equal("轻甲", result.Values[0].DisplayName);
        Assert.Equal("轻型装甲", result.Values[0].Description);
        Assert.Empty(result.Warnings);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\r\n\t")]
    public void Parse_NullEmptyOrWhitespaceReturnsEmptyResult(string? text)
    {
        Ra2AllowedValuesTextParseResult result = _parser.Parse(text);

        Assert.Empty(result.Values);
        Assert.Empty(result.Warnings);
    }
}
