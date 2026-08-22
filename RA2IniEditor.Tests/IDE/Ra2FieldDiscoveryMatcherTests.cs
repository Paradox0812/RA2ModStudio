using RA2IniEditor.IDE.FieldAnnotations;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FieldDiscoveryMatcherTests
{
    private readonly Ra2FieldDiscoveryMatcher _matcher = new();

    [Theory]
    [InlineData("Str", true)]
    [InlineData("Health", true)]
    [InlineData("HP", true)]
    [InlineData("hit points", true)]
    [InlineData("missing", false)]
    public void IsMatch_SearchesKeyDisplayNameAliasesNoteAndDescription(string query, bool expected)
    {
        Assert.Equal(expected, _matcher.IsMatch(CreateInfo(), query));
    }

    [Fact]
    public void GetPriority_OrdersPrefixBeforeContainsAndNote()
    {
        Assert.True(_matcher.GetPriority(CreateInfo(), "Str") < _matcher.GetPriority(CreateInfo(), "points"));
        Assert.True(_matcher.GetPriority(CreateInfo(), "Health") < _matcher.GetPriority(CreateInfo(), "hit points"));
        Assert.True(_matcher.GetPriority(CreateInfo(), "HP") < _matcher.GetPriority(CreateInfo(), "maximum"));
    }

    [Fact]
    public void EmptyQueryMatchesAll()
    {
        Assert.True(_matcher.IsMatch(CreateInfo(), ""));
        Assert.Equal(0, _matcher.GetPriority(CreateInfo(), ""));
        Assert.Equal(Ra2FieldBrowserMatchSource.None, _matcher.Match(CreateInfo(), "").Source);
    }

    [Theory]
    [InlineData("Str", "Key", "Strength", 1)]
    [InlineData("Health", "DisplayName", "Health", 2)]
    [InlineData("HP", "Alias", "HP", 3)]
    [InlineData("maximum", "Note", "Maximum hit points.", 7)]
    [InlineData("Object", "Description", "Object hit points.", 8)]
    public void Match_ReturnsSourceMatchedTextAndPriority(
        string query,
        string expectedSource,
        string expectedText,
        int expectedPriority)
    {
        Ra2FieldBrowserMatchResult result = _matcher.Match(CreateInfo(), query);

        Assert.True(result.IsMatch);
        Assert.Equal(expectedSource, result.Source.ToString());
        Assert.Equal(expectedText, result.MatchedText);
        Assert.Equal(expectedPriority, result.Priority);
    }

    [Fact]
    public void Match_UnknownQueryReturnsNoMatch()
    {
        Ra2FieldBrowserMatchResult result = _matcher.Match(CreateInfo(), "missing");

        Assert.False(result.IsMatch);
        Assert.Equal(Ra2FieldBrowserMatchSource.None, result.Source);
        Assert.Equal(int.MaxValue, result.Priority);
    }

    private static Ra2FieldDisplayInfo CreateInfo()
        => new(
            "Strength",
            "Health",
            ["HP", "Durability"],
            "Maximum hit points.",
            "Object hit points.",
            "Integer",
            "Vehicle",
            "BuiltIn",
            hasUserAnnotation: true);
}
