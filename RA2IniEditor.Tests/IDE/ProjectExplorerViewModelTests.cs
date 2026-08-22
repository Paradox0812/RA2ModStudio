using RA2IniEditor.IDE.Models;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.ViewModels;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class ProjectExplorerViewModelTests
{
    [Fact]
    public void ShowFiles_CreatesFileNodesWithoutSections()
    {
        ProjectExplorerViewModel viewModel = new();

        viewModel.ShowFiles(
        [
            new ReadonlyIniFileDescriptor("rules.ini", "C:\\game\\rules.ini", 100),
            new ReadonlyIniFileDescriptor("art.ini", "C:\\game\\art.ini", 200)
        ]);

        Assert.Equal(2, viewModel.Items.Count);
        Assert.All(viewModel.Items, item =>
        {
            Assert.Equal(ProjectExplorerItemKind.File, item.Kind);
            Assert.Equal("INI", item.IconText);
            Assert.Empty(item.Children);
        });
        Assert.Equal("2 INI file(s)", viewModel.StatusText);
    }

    [Fact]
    public void ShowGroupedSectionsForCurrentFile_BuildsTypeFactionSectionTree()
    {
        ProjectExplorerViewModel viewModel = CreateExplorerWithTwoFiles();

        viewModel.ShowGroupedSectionsForCurrentFile(
            "C:\\game\\rules.ini",
            [
                new ReadonlySectionClassificationResult("General", 1, null, "Global / Registry", null),
                new ReadonlySectionClassificationResult("E1", 8, "GI", "Infantry", "Allied")
            ]);

        ProjectExplorerItemViewModel rules = viewModel.Items[0];
        ProjectExplorerItemViewModel art = viewModel.Items[1];
        Assert.Equal(2, rules.Children.Count);
        Assert.Empty(art.Children);
        Assert.True(rules.IsExpanded);

        ProjectExplorerItemViewModel global = rules.Children[0];
        Assert.Equal(ProjectExplorerItemKind.TypeGroup, global.Kind);
        Assert.Equal("Global / Registry", global.DisplayText);
        Assert.Equal(1, global.SectionCount);
        Assert.Equal("Global / Registry (1)", global.DisplayTextWithCount);
        Assert.Equal("Global / Registry: 1 section(s)", global.ToolTipText);
        Assert.Equal("Reg", global.IconText);
        ProjectExplorerItemViewModel general = Assert.Single(global.Children);
        Assert.Equal(ProjectExplorerItemKind.Section, general.Kind);
        Assert.Equal("[General]", general.DisplayText);
        Assert.Equal("[General]", general.DisplayTextWithCount);
        Assert.Contains("Line 1", general.ToolTipText);
        Assert.Equal("Reg", general.IconText);
        Assert.Equal(1, general.LineNumber);

        ProjectExplorerItemViewModel infantry = rules.Children[1];
        ProjectExplorerItemViewModel allied = Assert.Single(infantry.Children);
        ProjectExplorerItemViewModel e1 = Assert.Single(allied.Children);
        Assert.Equal(1, infantry.SectionCount);
        Assert.Equal("Infantry (1)", infantry.DisplayTextWithCount);
        Assert.Equal("Inf", infantry.IconText);
        Assert.Equal(ProjectExplorerItemKind.FactionGroup, allied.Kind);
        Assert.Equal("Allied", allied.DisplayText);
        Assert.Equal(1, allied.SectionCount);
        Assert.Equal("Allied (1)", allied.DisplayTextWithCount);
        Assert.Equal("Allied: 1 section(s)", allied.ToolTipText);
        Assert.Equal("A", allied.IconText);
        Assert.Equal(ProjectExplorerItemKind.Section, e1.Kind);
        Assert.Equal("[E1]  GI", e1.DisplayText);
        Assert.Contains("Line 8", e1.ToolTipText);
        Assert.Equal("Inf", e1.IconText);
        Assert.Equal("C:\\game\\rules.ini", e1.FilePath);
        Assert.Equal(8, e1.LineNumber);
        Assert.Equal("E1", e1.SectionId);
        Assert.True(e1.CanNavigateToSource);
    }

    [Fact]
    public void ShowGroupedSectionsForCurrentFile_PreservesPreviouslyLoadedOtherFileNodes()
    {
        ProjectExplorerViewModel viewModel = CreateExplorerWithTwoFiles();
        viewModel.ShowGroupedSectionsForCurrentFile(
            "C:\\game\\rules.ini",
            [new ReadonlySectionClassificationResult("E1", 8, "GI", "Infantry", "Allied")]);

        viewModel.ShowGroupedSectionsForCurrentFile(
            "C:\\game\\art.ini",
            [new ReadonlySectionClassificationResult("Animations", 3, null, "Global / Registry", null)]);

        ProjectExplorerItemViewModel rulesType = Assert.Single(viewModel.Items[0].Children);
        ProjectExplorerItemViewModel rulesFaction = Assert.Single(rulesType.Children);
        ProjectExplorerItemViewModel rulesSection = Assert.Single(rulesFaction.Children);
        Assert.Equal("[E1]  GI", rulesSection.DisplayText);
        ProjectExplorerItemViewModel artType = Assert.Single(viewModel.Items[1].Children);
        ProjectExplorerItemViewModel artSection = Assert.Single(artType.Children);
        Assert.Equal("[Animations]", artSection.DisplayText);
        Assert.Equal(3, artSection.LineNumber);
    }

    [Fact]
    public void ShowGroupedSectionsForCurrentFile_ReplacesOnlyTargetFileGroups()
    {
        ProjectExplorerViewModel viewModel = CreateExplorerWithTwoFiles();
        viewModel.ShowGroupedSectionsForCurrentFile(
            "C:\\game\\rules.ini",
            [new ReadonlySectionClassificationResult("E1", 8, "GI", "Infantry", "Allied")]);
        viewModel.ShowGroupedSectionsForCurrentFile(
            "C:\\game\\art.ini",
            [new ReadonlySectionClassificationResult("Animations", 3, null, "Global / Registry", null)]);

        viewModel.ShowGroupedSectionsForCurrentFile(
            "C:\\game\\rules.ini",
            [new ReadonlySectionClassificationResult("GHOST", 20, "SEAL", "Infantry", "Allied")]);

        ProjectExplorerItemViewModel rulesType = Assert.Single(viewModel.Items[0].Children);
        ProjectExplorerItemViewModel rulesFaction = Assert.Single(rulesType.Children);
        ProjectExplorerItemViewModel rulesSection = Assert.Single(rulesFaction.Children);
        Assert.Equal("[GHOST]  SEAL", rulesSection.DisplayText);
        ProjectExplorerItemViewModel artType = Assert.Single(viewModel.Items[1].Children);
        ProjectExplorerItemViewModel artSection = Assert.Single(artType.Children);
        Assert.Equal("[Animations]", artSection.DisplayText);
    }

    [Fact]
    public void ShowPlaceholderForCurrentFile_ReplacesSectionsWithNonNavigablePlaceholder()
    {
        ProjectExplorerViewModel viewModel = CreateExplorerWithTwoFiles();
        viewModel.ShowGroupedSectionsForCurrentFile(
            "C:\\game\\rules.ini",
            [new ReadonlySectionClassificationResult("E1", 8, "GI", "Infantry", "Allied")]);

        viewModel.ShowPlaceholderForCurrentFile("C:\\game\\rules.ini", "Sections skipped: large file preview is deferred.");

        ProjectExplorerItemViewModel placeholder = Assert.Single(viewModel.Items[0].Children);
        Assert.Equal(ProjectExplorerItemKind.Placeholder, placeholder.Kind);
        Assert.Equal("...", placeholder.IconText);
        Assert.False(placeholder.CanNavigateToSource);
        Assert.Equal("Sections skipped: large file preview is deferred.", placeholder.DisplayText);
    }

    [Fact]
    public void MarkCurrentFile_OnlyMarksTargetFileWithoutClearingChildren()
    {
        ProjectExplorerViewModel viewModel = CreateExplorerWithTwoFiles();
        ProjectExplorerItemViewModel rules = viewModel.Items[0];
        rules.Children.Add(new ProjectExplorerItemViewModel(ProjectExplorerItemKind.TypeGroup, "Infantry", rules.FilePath));

        viewModel.MarkCurrentFile("C:\\game\\rules.ini");

        Assert.True(rules.IsCurrentFile);
        Assert.False(viewModel.Items[1].IsCurrentFile);
        Assert.True(rules.IsExpanded);
        Assert.Single(rules.Children);
    }

    [Fact]
    public void MarkCurrentSection_OnlyMarksTargetSectionAndClearsPreviousCurrentSection()
    {
        ProjectExplorerViewModel viewModel = CreateExplorerWithTwoFiles();
        viewModel.ShowGroupedSectionsForCurrentFile(
            "C:\\game\\rules.ini",
            [
                new ReadonlySectionClassificationResult("E1", 8, "GI", "Infantry", "Allied"),
                new ReadonlySectionClassificationResult("GHOST", 20, "SEAL", "Infantry", "Allied")
            ]);
        ProjectExplorerItemViewModel allied = Assert.Single(viewModel.Items[0].Children[0].Children);
        ProjectExplorerItemViewModel e1 = allied.Children[0];
        ProjectExplorerItemViewModel ghost = allied.Children[1];

        viewModel.MarkCurrentSection("C:\\game\\rules.ini", "E1");
        viewModel.MarkCurrentSection("C:\\game\\rules.ini", "GHOST");

        Assert.False(e1.IsCurrentSection);
        Assert.True(ghost.IsCurrentSection);
        Assert.True(viewModel.Items[0].IsExpanded);
        Assert.True(viewModel.Items[0].Children[0].IsExpanded);
        Assert.True(allied.IsExpanded);
    }

    [Fact]
    public void MarkCurrentSection_DoesNothingWhenTargetDoesNotExist()
    {
        ProjectExplorerViewModel viewModel = CreateExplorerWithTwoFiles();
        viewModel.ShowGroupedSectionsForCurrentFile(
            "C:\\game\\rules.ini",
            [new ReadonlySectionClassificationResult("E1", 8, "GI", "Infantry", "Allied")]);
        ProjectExplorerItemViewModel e1 = Assert.Single(Assert.Single(viewModel.Items[0].Children).Children.Single().Children);

        viewModel.MarkCurrentSection("C:\\game\\rules.ini", "MISSING");

        Assert.False(e1.IsCurrentSection);
    }

    [Theory]
    [InlineData("Vehicle", "Veh")]
    [InlineData("Building", "Bld")]
    [InlineData("Weapon", "Wpn")]
    [InlineData("Warhead", "WH")]
    [InlineData("Unknown", "?")]
    public void ShowGroupedSectionsForCurrentFile_UsesLightweightTypeBadges(string typeGroup, string expectedIconText)
    {
        ProjectExplorerViewModel viewModel = CreateExplorerWithTwoFiles();

        viewModel.ShowGroupedSectionsForCurrentFile(
            "C:\\game\\rules.ini",
            [new ReadonlySectionClassificationResult("TEST", 8, null, typeGroup, null)]);

        ProjectExplorerItemViewModel typeNode = Assert.Single(viewModel.Items[0].Children);
        ProjectExplorerItemViewModel sectionNode = Assert.Single(typeNode.Children);
        Assert.Equal(expectedIconText, typeNode.IconText);
        Assert.Equal(expectedIconText, sectionNode.IconText);
        Assert.Equal("TEST", sectionNode.SectionId);
    }

    [Theory]
    [InlineData("Soviet", "S")]
    [InlineData("Yuri", "Y")]
    [InlineData("Neutral", "N")]
    [InlineData("Common", "C")]
    [InlineData("Unknown", "?")]
    public void ShowGroupedSectionsForCurrentFile_UsesLightweightFactionBadges(string factionGroup, string expectedIconText)
    {
        ProjectExplorerViewModel viewModel = CreateExplorerWithTwoFiles();

        viewModel.ShowGroupedSectionsForCurrentFile(
            "C:\\game\\rules.ini",
            [new ReadonlySectionClassificationResult("TEST", 8, null, "Infantry", factionGroup)]);

        ProjectExplorerItemViewModel typeNode = Assert.Single(viewModel.Items[0].Children);
        ProjectExplorerItemViewModel factionNode = Assert.Single(typeNode.Children);
        Assert.Equal(expectedIconText, factionNode.IconText);
    }

    private static ProjectExplorerViewModel CreateExplorerWithTwoFiles()
    {
        ProjectExplorerViewModel viewModel = new();
        viewModel.ShowFiles(
        [
            new ReadonlyIniFileDescriptor("rules.ini", "C:\\game\\rules.ini", 100),
            new ReadonlyIniFileDescriptor("art.ini", "C:\\game\\art.ini", 200)
        ]);
        return viewModel;
    }
}
