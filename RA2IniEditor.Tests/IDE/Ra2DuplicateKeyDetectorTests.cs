using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.TextModel;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2DuplicateKeyDetectorTests
{
    [Fact]
    public void ContainsKeyInCurrentSection_ReturnsTrueForExistingKeyIgnoringCase()
    {
        const string text = "[HTNK]\nStrength=400\n";
        Ra2IniTextDocument document = Parse(text);

        Assert.True(new Ra2DuplicateKeyDetector().ContainsKeyInCurrentSection(
            document,
            text.IndexOf("Strength", StringComparison.Ordinal),
            "strength"));
    }

    [Fact]
    public void FindInCurrentSection_ReturnsLineNumberExistingValueAndValueSpan()
    {
        const string text = "[HTNK]\nStrength=400 ; hp\n";
        Ra2IniTextDocument document = Parse(text);

        Ra2DuplicateKeyMatch? match = new Ra2DuplicateKeyDetector().FindInCurrentSection(
            document,
            text.IndexOf("Strength", StringComparison.Ordinal),
            "Strength");

        Assert.NotNull(match);
        Assert.Equal("Strength", match.Key);
        Assert.Equal(2, match.LineNumber);
        Assert.Equal("400", match.ExistingValue);
        Assert.Equal("400", text.Substring(match.ValueSpan.Start, match.ValueSpan.Length));
        Assert.Equal("Strength=400 ; hp", text.Substring(match.LineSpan.Start, match.LineSpan.Length));
    }

    [Fact]
    public void ContainsKeyInCurrentSection_IgnoresSameKeyInOtherSection()
    {
        const string text = "[HTNK]\nName=Heavy\n\n[LTNK]\nStrength=300\n";
        Ra2IniTextDocument document = Parse(text);

        Assert.False(new Ra2DuplicateKeyDetector().ContainsKeyInCurrentSection(
            document,
            text.IndexOf("Name", StringComparison.Ordinal),
            "Strength"));
    }

    [Fact]
    public void ContainsKeyInCurrentSection_IgnoresCommentsContainingSameKey()
    {
        const string text = "[HTNK]\n; Strength=400\nName=Heavy\n";
        Ra2IniTextDocument document = Parse(text);

        Assert.False(new Ra2DuplicateKeyDetector().ContainsKeyInCurrentSection(
            document,
            text.IndexOf("Name", StringComparison.Ordinal),
            "Strength"));
    }

    [Fact]
    public void FindInCurrentSection_IgnoresRawLineContainingSameKey()
    {
        const string text = "[HTNK]\nStrength 400\nName=Heavy\n";
        Ra2IniTextDocument document = Parse(text);

        Assert.Null(new Ra2DuplicateKeyDetector().FindInCurrentSection(
            document,
            text.IndexOf("Name", StringComparison.Ordinal),
            "Strength"));
    }

    [Fact]
    public void FindInCurrentSection_UsesCurrentSectionBoundsForDuplicateSectionNames()
    {
        const string text = "[HTNK]\nStrength=400\n\n[HTNK]\nName=Later\n";
        Ra2IniTextDocument document = Parse(text);

        Assert.Null(new Ra2DuplicateKeyDetector().FindInCurrentSection(
            document,
            text.IndexOf("Name=Later", StringComparison.Ordinal),
            "Strength"));
    }

    [Fact]
    public void FindInCurrentSection_ReturnsEmptyValueSpan()
    {
        const string text = "[HTNK]\nStrength= ; hp\n";
        Ra2IniTextDocument document = Parse(text);

        Ra2DuplicateKeyMatch? match = new Ra2DuplicateKeyDetector().FindInCurrentSection(
            document,
            text.IndexOf("Strength", StringComparison.Ordinal),
            "Strength");

        Assert.NotNull(match);
        Assert.Equal(string.Empty, match.ExistingValue);
        Assert.Equal(0, match.ValueSpan.Length);
        Assert.Equal(text.IndexOf("=", StringComparison.Ordinal) + 1, match.ValueSpan.Start);
    }

    private static Ra2IniTextDocument Parse(string text)
        => new Ra2IniTextDocumentParser().Parse(text);
}
