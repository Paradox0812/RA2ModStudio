using RA2IniEditor.IDE.Services;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class ReadonlyProjectExplorerGroupingServiceTests
{
    [Fact]
    public void BuildGroups_UsesTypeRegistriesForCoreObjectKinds()
    {
        ReadonlyProjectExplorerGroupingService service = new();

        var results = service.BuildGroups(
            """
            [InfantryTypes]
            0=E1
            [VehicleTypes]
            0=MTNK
            [AircraftTypes]
            0=ORCA
            [BuildingTypes]
            0=GAPILE
            [E1]
            Name=GI
            Owner=British,French
            [MTNK]
            Owner=Russians
            [ORCA]
            Owner=YuriCountry
            [GAPILE]
            Prerequisite=GACNST
            """);

        AssertClassification(results, "E1", "Infantry", "Allied", 9, "GI");
        AssertClassification(results, "MTNK", "Vehicle", "Soviet", 12, null);
        AssertClassification(results, "ORCA", "Aircraft", "Yuri", 14, null);
        AssertClassification(results, "GAPILE", "Building", "Allied", 16, null);
    }

    [Fact]
    public void BuildGroups_ClassifiesGlobalRegistryAndUnknownSections()
    {
        ReadonlyProjectExplorerGroupingService service = new();

        var results = service.BuildGroups(
            """
            [General]
            Name=Global
            [Countries]
            0=Americans
            [MYSTERY]
            Name=Unknown Object
            """);

        AssertClassification(results, "General", "Global / Registry", null, 1, "Global");
        AssertClassification(results, "Countries", "Global / Registry", null, 3, null);
        AssertClassification(results, "MYSTERY", "Unknown", null, 5, "Unknown Object");
    }

    [Fact]
    public void BuildGroups_CommonOwnerUsesCommonFaction()
    {
        ReadonlyProjectExplorerGroupingService service = new();

        var result = service.BuildGroups(
            """
            [InfantryTypes]
            0=HERO
            [HERO]
            Owner=British,Russians,YuriCountry
            """).Single(section => section.SectionId == "HERO");

        Assert.Equal("Infantry", result.TypeGroup);
        Assert.Equal("Common", result.FactionGroup);
    }

    [Fact]
    public void BuildGroups_UsesRealSectionHeaderLineWhenRegistryEntryLineDiffers()
    {
        ReadonlyProjectExplorerGroupingService service = new();

        var results = service.BuildGroups(
            """
            [InfantryTypes]
            100=GGI

            [General]
            Name=Global

            [GGI]
            Name=Guardian GI
            Owner=British,French,Germans,Americans,Alliance
            """);

        AssertClassification(results, "GGI", "Infantry", "Allied", 7, "Guardian GI");
        Assert.DoesNotContain(results, section => section.SectionId == "100");
    }

    [Fact]
    public void BuildGroups_DoesNotCreateSectionNodeForRegisteredEntryWithoutHeader()
    {
        ReadonlyProjectExplorerGroupingService service = new();

        var results = service.BuildGroups(
            """
            [InfantryTypes]
            100=GGI
            [General]
            Name=Global
            """);

        Assert.DoesNotContain(results, section => section.SectionId == "GGI");
    }

    [Fact]
    public void BuildGroups_DuplicateSectionsUseFirstHeaderLine()
    {
        ReadonlyProjectExplorerGroupingService service = new();

        var results = service.BuildGroups(
            """
            [InfantryTypes]
            0=E1
            [E1]
            Name=First GI
            [E1]
            Name=Duplicate GI
            """);

        ReadonlySectionClassificationResult result = Assert.Single(results, section => section.SectionId == "E1");
        Assert.Equal("Infantry", result.TypeGroup);
        Assert.Equal(3, result.LineNumber);
        Assert.Equal("First GI", result.DisplayName);
    }

    [Fact]
    public void BuildGroups_UsesReferenceBasedWeaponProjectileAndWarheadClassification()
    {
        ReadonlyProjectExplorerGroupingService service = new();

        var results = service.BuildGroups(
            """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=120mm

            [120mm]
            Projectile=Cannon
            Warhead=AP

            [Cannon]
            Image=CANNON

            [AP]
            Verses=100%,100%,100%
            """);

        AssertClassification(results, "NEWINF", "Infantry", "Unknown", 4, null);
        AssertClassification(results, "120mm", "Weapon", null, 7, null);
        AssertClassification(results, "Cannon", "Projectile", null, 11, "CANNON");
        AssertClassification(results, "AP", "Warhead", null, 14, null);
    }

    [Fact]
    public void BuildGroups_IncludesSectionHeadersWithInlineComments()
    {
        ReadonlyProjectExplorerGroupingService service = new();

        var results = service.BuildGroups(
            """
            [InfantryTypes]
            0=E1

            [E1];GI
            Primary=M60

            [M60];GIWeapon
            Damage=15
            """);

        AssertClassification(results, "E1", "Infantry", "Unknown", 4, null);
        AssertClassification(results, "M60", "Weapon", null, 7, null);
    }

    [Fact]
    public void BuildGroups_DoesNotClassifyWeaponFromArbitraryUnknownKey()
    {
        ReadonlyProjectExplorerGroupingService service = new();

        var results = service.BuildGroups(
            """
            [UNKNOWN]
            SomeKey=120mm

            [120mm]
            Damage=90
            """);

        AssertClassification(results, "120mm", "Unknown", null, 4, null);
    }

    private static void AssertClassification(
        IReadOnlyList<ReadonlySectionClassificationResult> results,
        string sectionId,
        string typeGroup,
        string? factionGroup,
        int lineNumber,
        string? displayName)
    {
        ReadonlySectionClassificationResult result = results.Single(section => section.SectionId == sectionId);
        Assert.Equal(typeGroup, result.TypeGroup);
        Assert.Equal(factionGroup, result.FactionGroup);
        Assert.Equal(lineNumber, result.LineNumber);
        Assert.Equal(displayName, result.DisplayName);
    }
}
