using RA2IniEditor.IDE.Services;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class IdeReadonlyNavigatorIndexServiceTests
{
    [Fact]
    public void BuildSectionIndex_ForEmptyTextReturnsEmptyIndex()
    {
        ReadonlyNavigatorIndexService service = new();

        var items = service.BuildSectionIndex(string.Empty);

        Assert.Empty(items);
    }

    [Fact]
    public void BuildSectionIndex_RecognizesSectionHeadersAndOneBasedLineNumbers()
    {
        ReadonlyNavigatorIndexService service = new();

        var items = service.BuildSectionIndex(
            """
            ; comment

            [E1]
            Name=GI

            [MTNK]
            Name=Grizzly Battle Tank
            """);

        Assert.Equal(2, items.Count);
        Assert.Equal("E1", items[0].SectionId);
        Assert.Equal(3, items[0].LineNumber);
        Assert.Equal("GI", items[0].DisplayName);
        Assert.Equal("MTNK", items[1].SectionId);
        Assert.Equal(6, items[1].LineNumber);
    }

    [Fact]
    public void BuildSectionIndex_RecognizesSectionHeadersWithInlineComments()
    {
        ReadonlyNavigatorIndexService service = new();

        var item = Assert.Single(service.BuildSectionIndex(
            """
            [M60];GIWeapon
            Damage=15
            """));

        Assert.Equal("M60", item.SectionId);
        Assert.Equal(1, item.LineNumber);
    }

    [Fact]
    public void BuildSectionIndex_UsesNameBeforeUiNameAndImage()
    {
        ReadonlyNavigatorIndexService service = new();

        var item = Assert.Single(service.BuildSectionIndex(
            """
            [MTNK]
            Image=GTNK
            UIName=Name:MTNK
            Name=Grizzly Battle Tank
            """));

        Assert.Equal("Grizzly Battle Tank", item.DisplayName);
    }

    [Fact]
    public void BuildSectionIndex_UsesUiNameFallbackBeforeImage()
    {
        ReadonlyNavigatorIndexService service = new();

        var item = Assert.Single(service.BuildSectionIndex(
            """
            [MTNK]
            Image=GTNK
            UIName=Name:MTNK
            """));

        Assert.Equal("Name:MTNK", item.DisplayName);
    }

    [Fact]
    public void BuildSectionIndex_UsesImageFallbackWhenNameAndUiNameAreMissing()
    {
        ReadonlyNavigatorIndexService service = new();

        var item = Assert.Single(service.BuildSectionIndex(
            """
            [MTNK]
            Image=GTNK
            """));

        Assert.Equal("GTNK", item.DisplayName);
    }

    [Fact]
    public void BuildSectionIndex_IgnoresCommentLinesAndEmptySectionNames()
    {
        ReadonlyNavigatorIndexService service = new();

        var items = service.BuildSectionIndex(
            """
            ; [CommentedSection]
            # [AnotherCommentedSection]
            []
            [   ]
            [Valid]
            Name=Real Section
            """);

        var item = Assert.Single(items);
        Assert.Equal("Valid", item.SectionId);
        Assert.Equal("Real Section", item.DisplayName);
    }

    [Fact]
    public void BuildSectionIndex_WhenNoDisplayFieldsUsesNullDisplayName()
    {
        ReadonlyNavigatorIndexService service = new();

        var item = Assert.Single(service.BuildSectionIndex("[WeaponTypes]"));

        Assert.Equal("WeaponTypes", item.SectionId);
        Assert.Null(item.DisplayName);
    }

    [Fact]
    public void BuildSectionIndex_IsCaseInsensitiveForDisplayFieldKeys()
    {
        ReadonlyNavigatorIndexService service = new();

        var item = Assert.Single(service.BuildSectionIndex(
            """
            [E1]
            name=GI
            """));

        Assert.Equal("GI", item.DisplayName);
    }
}
