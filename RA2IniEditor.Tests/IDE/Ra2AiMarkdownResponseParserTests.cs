using RA2IniEditor.IDE.AI;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiMarkdownResponseParserTests
{
    [Fact]
    public void Parse_SingleIniFence_ReturnsTextAndCodeBlocks()
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(
            "## rulesmd.ini\n```ini\n[LAAV]\nStrength=200\n```\nDone");

        Assert.Equal(3, blocks.Count);
        Assert.Equal(Ra2AiMarkdownBlockKind.Heading, blocks[0].Kind);
        Assert.Equal(2, blocks[0].HeadingLevel);
        Assert.Equal("rulesmd.ini", blocks[0].Text);
        Assert.True(blocks[1].IsCodeBlock);
        Assert.Equal("ini", blocks[1].Language);
        Assert.Equal("[LAAV]\nStrength=200\n", blocks[1].Text);
        Assert.Equal(Ra2AiMarkdownBlockKind.Paragraph, blocks[2].Kind);
        Assert.Equal("Done", blocks[2].Text);
    }

    [Fact]
    public void Parse_MultipleFences_PreservesOrderAndLanguages()
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(
            "A\n```rules\n[Unit]\n```\nB\n```art\n[Unit]\nVoxel=yes\n```\nC");

        Assert.Equal(5, blocks.Count);
        Assert.Equal("rules", blocks[1].Language);
        Assert.Equal("[Unit]\n", blocks[1].Text);
        Assert.Equal("art", blocks[3].Language);
        Assert.Equal("[Unit]\nVoxel=yes\n", blocks[3].Text);
    }

    [Fact]
    public void Parse_UnlabeledFence_ReturnsNullLanguage()
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(
            "```\n[General]\n```\n");

        Assert.Single(blocks);
        Assert.True(blocks[0].IsCodeBlock);
        Assert.Null(blocks[0].Language);
    }

    [Fact]
    public void Parse_CodeContentExcludesFenceMarkers()
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(
            "```ini\nPrimary=LAAVMissile\n```");

        Assert.Single(blocks);
        Assert.DoesNotContain("```", blocks[0].Text);
        Assert.Equal("Primary=LAAVMissile\n", blocks[0].Text);
    }

    [Fact]
    public void Parse_PreservesTextBeforeAndAfterCodeBlock()
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(
            "Before\n```ini\n[LAAV]\n```\nAfter");

        Assert.Equal("Before", blocks[0].Text);
        Assert.Equal("After", blocks[2].Text);
    }

    [Fact]
    public void Parse_UnterminatedFenceFallsBackToPlainText()
    {
        const string response = "Before\n```ini\n[LAAV]\nStrength=200";

        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(response);

        Assert.Single(blocks);
        Assert.False(blocks[0].IsCodeBlock);
        Assert.Equal(response, blocks[0].Text);
    }

    [Fact]
    public void Parse_Headings_ReturnsHeadingBlocksWithLevels()
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(
            "# H1\n## H2\n### H3");

        Assert.Equal(3, blocks.Count);
        Assert.All(blocks, block => Assert.Equal(Ra2AiMarkdownBlockKind.Heading, block.Kind));
        Assert.Equal(1, blocks[0].HeadingLevel);
        Assert.Equal(2, blocks[1].HeadingLevel);
        Assert.Equal(3, blocks[2].HeadingLevel);
    }

    [Fact]
    public void Parse_ParagraphLines_ReturnsParagraphBlock()
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(
            "First paragraph line\ncontinued line");

        Assert.Single(blocks);
        Assert.Equal(Ra2AiMarkdownBlockKind.Paragraph, blocks[0].Kind);
        Assert.Equal("First paragraph line" + Environment.NewLine + "continued line", blocks[0].Text);
    }

    [Fact]
    public void Parse_Bullets_ReturnsBulletBlocks()
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(
            "- first\n* second");

        Assert.Equal(2, blocks.Count);
        Assert.Equal(Ra2AiMarkdownBlockKind.Bullet, blocks[0].Kind);
        Assert.Equal("first", blocks[0].Text);
        Assert.Equal(Ra2AiMarkdownBlockKind.Bullet, blocks[1].Kind);
        Assert.Equal("second", blocks[1].Text);
    }

    [Fact]
    public void Parse_NumberedList_ReturnsNumberedBlocks()
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(
            "1. first\n2. second");

        Assert.Equal(2, blocks.Count);
        Assert.Equal(Ra2AiMarkdownBlockKind.Numbered, blocks[0].Kind);
        Assert.Equal("first", blocks[0].Text);
        Assert.Equal(Ra2AiMarkdownBlockKind.Numbered, blocks[1].Kind);
        Assert.Equal("second", blocks[1].Text);
    }

    [Fact]
    public void Parse_DoesNotParseMarkdownInsideCodeBlock()
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(
            "```ini\n# NotAHeading\n- NotABullet\n```");

        Assert.Single(blocks);
        Assert.Equal(Ra2AiMarkdownBlockKind.Code, blocks[0].Kind);
        Assert.Contains("# NotAHeading", blocks[0].Text);
        Assert.Contains("- NotABullet", blocks[0].Text);
    }

    [Fact]
    public void Parse_UnsupportedMarkdownFallsBackToParagraph()
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(
            "> quoted text");

        Assert.Single(blocks);
        Assert.Equal(Ra2AiMarkdownBlockKind.Paragraph, blocks[0].Kind);
        Assert.Equal("> quoted text", blocks[0].Text);
    }

    [Fact]
    public void Parse_SimplePipeTable_ReturnsTableBlock()
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(
            "| ID | Type | Note |\n|----|------|------|\n| LAAV | Unit | Light AA |");

        Assert.Single(blocks);
        Assert.Equal(Ra2AiMarkdownBlockKind.Table, blocks[0].Kind);
        Assert.Equal(["ID", "Type", "Note"], blocks[0].TableHeaders);
        Assert.Single(blocks[0].TableRows);
        Assert.Equal(["LAAV", "Unit", "Light AA"], blocks[0].TableRows[0]);
    }

    [Fact]
    public void Parse_PipeTable_TrimsCellsAndParsesMultipleBodyRows()
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(
            "| ID | Type |\n| :--- | ---: |\n|  LAAV  |  Unit  |\n| LAAVWeapon | Weapon |");

        Assert.Single(blocks);
        Assert.Equal(["ID", "Type"], blocks[0].TableHeaders);
        Assert.Equal(2, blocks[0].TableRows.Count);
        Assert.Equal(["LAAV", "Unit"], blocks[0].TableRows[0]);
        Assert.Equal(["LAAVWeapon", "Weapon"], blocks[0].TableRows[1]);
    }

    [Fact]
    public void Parse_MalformedPipeTableFallsBackToParagraph()
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(
            "| ID | Type |\n| not separator | row |\n| LAAV | Unit |");

        Assert.Single(blocks);
        Assert.Equal(Ra2AiMarkdownBlockKind.Paragraph, blocks[0].Kind);
        Assert.Contains("| ID | Type |", blocks[0].Text);
    }

    [Fact]
    public void Parse_DoesNotParsePipeTableInsideCodeBlock()
    {
        IReadOnlyList<Ra2AiMarkdownBlock> blocks = Ra2AiMarkdownResponseParser.Parse(
            "```ini\n| ID | Type |\n|----|------|\n```");

        Assert.Single(blocks);
        Assert.Equal(Ra2AiMarkdownBlockKind.Code, blocks[0].Kind);
        Assert.Contains("| ID | Type |", blocks[0].Text);
    }
}
