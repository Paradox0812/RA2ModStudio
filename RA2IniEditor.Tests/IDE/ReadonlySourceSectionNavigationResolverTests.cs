using RA2IniEditor.IDE.Services;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class ReadonlySourceSectionNavigationResolverTests
{
    [Fact]
    public void Resolve_FindsExactHeaderAndCharacterIndex()
    {
        ReadonlySourceSectionNavigationResolver resolver = new();
        string sourceText = "[General]\r\nName=Rules\r\n[GGI]\r\nName=Guardian GI";

        ReadonlySectionNavigationTarget? target = resolver.Resolve(sourceText, "GGI");

        Assert.NotNull(target);
        Assert.Equal("GGI", target.SectionId);
        Assert.Equal(3, target.OneBasedLineNumber);
        Assert.Equal(sourceText.IndexOf("[GGI]", StringComparison.Ordinal), target.CharacterIndex);
    }

    [Theory]
    [InlineData("[GGI] ; Guardian GI")]
    [InlineData("[GGI] # Guardian GI")]
    public void Resolve_SupportsHeaderWithInlineComment(string headerLine)
    {
        ReadonlySourceSectionNavigationResolver resolver = new();
        string sourceText = "[General]\n" + headerLine + "\nName=Guardian GI";

        ReadonlySectionNavigationTarget? target = resolver.Resolve(sourceText, "GGI");

        Assert.NotNull(target);
        Assert.Equal(2, target.OneBasedLineNumber);
        Assert.Equal(sourceText.IndexOf("[GGI]", StringComparison.Ordinal), target.CharacterIndex);
    }

    [Fact]
    public void Resolve_IgnoresCommentedHeader()
    {
        ReadonlySourceSectionNavigationResolver resolver = new();
        string sourceText = ";[GGI]\n#[GHOST]\n[GGI]\nName=Guardian GI";

        ReadonlySectionNavigationTarget? target = resolver.Resolve(sourceText, "GGI");

        Assert.NotNull(target);
        Assert.Equal(3, target.OneBasedLineNumber);
        Assert.Equal(sourceText.LastIndexOf("[GGI]", StringComparison.Ordinal), target.CharacterIndex);
    }

    [Fact]
    public void Resolve_RejectsHeaderWithInvalidTrailingText()
    {
        ReadonlySourceSectionNavigationResolver resolver = new();
        string sourceText = "[GGI] invalid trailing text\nName=NotASection";

        ReadonlySectionNavigationTarget? target = resolver.Resolve(sourceText, "GGI");

        Assert.Null(target);
    }

    [Fact]
    public void Resolve_RejectsKeyValueThatContainsSectionText()
    {
        ReadonlySourceSectionNavigationResolver resolver = new();
        string sourceText = "[General]\nSomeKey=[GGI]\nName=Rules";

        ReadonlySectionNavigationTarget? target = resolver.Resolve(sourceText, "GGI");

        Assert.Null(target);
    }

    [Theory]
    [InlineData("\r\n")]
    [InlineData("\n")]
    [InlineData("\r")]
    public void Resolve_SupportsNewLineStyles(string newLine)
    {
        ReadonlySourceSectionNavigationResolver resolver = new();
        string sourceText = "[General]" + newLine + "Name=Rules" + newLine + "[GHOST]" + newLine + "Name=SEAL";

        ReadonlySectionNavigationTarget? target = resolver.Resolve(sourceText, "GHOST");

        Assert.NotNull(target);
        Assert.Equal(3, target.OneBasedLineNumber);
        Assert.Equal(sourceText.IndexOf("[GHOST]", StringComparison.Ordinal), target.CharacterIndex);
    }

    [Fact]
    public void Resolve_SupportsFinalLineWithoutNewLine()
    {
        ReadonlySourceSectionNavigationResolver resolver = new();
        string sourceText = "[General]\nName=Rules\n[GHOST]";

        ReadonlySectionNavigationTarget? target = resolver.Resolve(sourceText, "GHOST");

        Assert.NotNull(target);
        Assert.Equal(3, target.OneBasedLineNumber);
        Assert.Equal(sourceText.IndexOf("[GHOST]", StringComparison.Ordinal), target.CharacterIndex);
    }

    [Fact]
    public void Resolve_UsesPreferredLineWhenItMatchesHeader()
    {
        ReadonlySourceSectionNavigationResolver resolver = new();
        string sourceText = "[GHOST]\nName=First\n[GHOST]\nName=Second";

        ReadonlySectionNavigationTarget? target = resolver.Resolve(sourceText, "GHOST", 3);

        Assert.NotNull(target);
        Assert.Equal(3, target.OneBasedLineNumber);
        Assert.Equal(sourceText.LastIndexOf("[GHOST]", StringComparison.Ordinal), target.CharacterIndex);
    }

    [Fact]
    public void Resolve_ScansRealHeaderWhenPreferredLineIsWrong()
    {
        ReadonlySourceSectionNavigationResolver resolver = new();
        string sourceText = "[InfantryTypes]\n100=GHOST\n[General]\nName=Rules\n[GHOST]\nName=SEAL";

        ReadonlySectionNavigationTarget? target = resolver.Resolve(sourceText, "GHOST", 2);

        Assert.NotNull(target);
        Assert.Equal(5, target.OneBasedLineNumber);
        Assert.Equal(sourceText.IndexOf("[GHOST]", StringComparison.Ordinal), target.CharacterIndex);
    }

    [Fact]
    public void Resolve_ReturnsNullWhenSectionIsMissing()
    {
        ReadonlySourceSectionNavigationResolver resolver = new();
        string sourceText = "[General]\nName=Rules\n[Other]\nName=Other";

        ReadonlySectionNavigationTarget? target = resolver.Resolve(sourceText, "GHOST");

        Assert.Null(target);
    }
}
