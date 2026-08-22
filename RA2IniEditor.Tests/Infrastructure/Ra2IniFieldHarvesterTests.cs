using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest.Ini;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class Ra2IniFieldHarvesterTests
{
    private readonly Ra2IniFieldHarvester _harvester = new();

    [Fact]
    public void HarvestCurrentText_ExtractsUnknownKeyFromCurrentText()
    {
        Ra2IniFieldHarvestResult result = Harvest("""
            [CUSTOM]
            NewField=abc
            """);

        Ra2IniFieldHarvestRow row = Assert.Single(result.Rows);
        Assert.Equal("NewField", row.Key);
        Assert.Equal(Ra2SectionKind.Unknown, row.SectionKind);
        Assert.Equal(["abc"], row.SampleValues);
        Assert.Equal(1, row.OccurrenceCount);
    }

    [Fact]
    public void HarvestCurrentText_SkipsCommentsBlankLinesSectionHeadersAndRegistryEntries()
    {
        Ra2IniFieldHarvestResult result = Harvest("""
            ; comment

            [InfantryTypes]
            0=E1

            [E1]
            Strength=125
            """);

        Ra2IniFieldHarvestRow row = Assert.Single(result.Rows);
        Assert.Equal("Strength", row.Key);
        Assert.DoesNotContain(result.Rows, candidate => candidate.Key == "0");
    }

    [Fact]
    public void HarvestCurrentText_RegistrySectionMapsObjectSectionToKind()
    {
        Ra2IniFieldHarvestResult result = Harvest("""
            [InfantryTypes]
            0=E1

            [E1]
            Strength=125
            """);

        Ra2IniFieldHarvestRow row = Assert.Single(result.Rows);
        Assert.Equal(Ra2SectionKind.Infantry, row.SectionKind);
    }

    [Fact]
    public void HarvestCurrentText_SkipsNumericListKeysOutsideRegistrySections()
    {
        Ra2IniFieldHarvestResult result = Harvest("""
            [SomeListSection]
            39=TREE24,CRATER05,GEM12
            40=TREE25,CRATER06,RadBeamWarhead
            CustomField=yes
            """);

        Ra2IniFieldHarvestRow row = Assert.Single(result.Rows);
        Assert.Equal("CustomField", row.Key);
        Assert.Equal(2, result.SkippedNumericKeyCount);
        Assert.DoesNotContain(result.Rows, candidate => candidate.Key == "39" || candidate.Key == "40");
    }

    [Fact]
    public void HarvestCurrentText_DeduplicatesKeyAndSectionKindAndCountsOccurrences()
    {
        Ra2IniFieldHarvestResult result = Harvest("""
            [InfantryTypes]
            0=E1
            1=GGI

            [E1]
            Armor=none
            Armor=flak

            [GGI]
            Armor=none
            """);

        Ra2IniFieldHarvestRow row = Assert.Single(result.Rows);
        Assert.Equal("Armor", row.Key);
        Assert.Equal(Ra2SectionKind.Infantry, row.SectionKind);
        Assert.Equal(3, row.OccurrenceCount);
        Assert.Equal(["none", "flak"], row.SampleValues);
    }

    [Fact]
    public void HarvestCurrentText_InfersYesNoBooleanMetadata()
    {
        Ra2IniFieldHarvestRow row = SingleRow("""
            [BuildingTypes]
            0=GAPOWR
            1=GAPILE

            [GAPOWR]
            Powered=yes

            [GAPILE]
            Powered=no
            """);

        Assert.Equal(FieldEditorKind.Boolean, row.InferredEditorKind);
        Assert.Equal(Ra2FieldValueKind.Boolean, row.InferredValueKind);
        Assert.Equal(Ra2FieldBooleanValueStyle.YesNo, row.InferredBooleanStyle);
        Assert.Equal(["no", "yes"], row.InferredAllowedValues.Select(value => value.Value).ToArray());
    }

    [Fact]
    public void HarvestCurrentText_InfersTrueFalseBooleanMetadata()
    {
        Ra2IniFieldHarvestRow row = SingleRow("""
            [VehicleTypes]
            0=TEST1
            1=TEST2

            [TEST1]
            CustomFlag=true

            [TEST2]
            CustomFlag=false
            """);

        Assert.Equal(FieldEditorKind.Boolean, row.InferredEditorKind);
        Assert.Equal(Ra2FieldBooleanValueStyle.TrueFalse, row.InferredBooleanStyle);
        Assert.Equal(["false", "true"], row.InferredAllowedValues.Select(value => value.Value).ToArray());
    }

    [Fact]
    public void HarvestCurrentText_MixedBooleanValuesUseCustomStyleAndWarn()
    {
        Ra2IniFieldHarvestRow row = SingleRow("""
            [VehicleTypes]
            0=TEST1
            1=TEST2

            [TEST1]
            CustomFlag=yes

            [TEST2]
            CustomFlag=false
            """);

        Assert.Equal(FieldEditorKind.Boolean, row.InferredEditorKind);
        Assert.Equal(Ra2FieldBooleanValueStyle.Custom, row.InferredBooleanStyle);
        Assert.NotEmpty(row.Issues);
    }

    [Theory]
    [InlineData("Strength", "125", "300", FieldEditorKind.Integer, Ra2FieldValueKind.Integer)]
    [InlineData("Speed", "6.5", "7", FieldEditorKind.Float, Ra2FieldValueKind.Float)]
    [InlineData("Armor", "none", "flak", FieldEditorKind.Enum, Ra2FieldValueKind.Enum)]
    [InlineData("Owner", "Americans,British", "Russians", FieldEditorKind.MultiSelect, Ra2FieldValueKind.EnumList)]
    [InlineData("Name", "Guardian GI", "Guardian GI", FieldEditorKind.Text, Ra2FieldValueKind.String)]
    public void HarvestCurrentText_InfersValueKindFromSamples(
        string key,
        string firstValue,
        string secondValue,
        FieldEditorKind expectedEditorKind,
        Ra2FieldValueKind expectedValueKind)
    {
        Ra2IniFieldHarvestRow row = SingleRow($"""
            [InfantryTypes]
            0=GGI
            1=E1

            [GGI]
            {key}={firstValue}

            [E1]
            {key}={secondValue}
            """);

        Assert.Equal(expectedEditorKind, row.InferredEditorKind);
        Assert.Equal(expectedValueKind, row.InferredValueKind);
    }

    [Fact]
    public void HarvestCurrentText_ExistingDefinitionAddsInfoIssue()
    {
        Ra2FieldDefinition existing = new(
            "Armor",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Enum,
            Ra2FieldSourceKind.User);

        Ra2IniFieldHarvestResult result = Harvest("""
            [InfantryTypes]
            0=E1

            [E1]
            Armor=none
            """, [existing]);

        Ra2IniFieldHarvestRow row = Assert.Single(result.Rows);
        Assert.Contains(row.Issues, issue => issue.Severity == FieldRegistryHarvestValidationSeverity.Info);
    }

    [Fact]
    public void HarvestCurrentText_MergesEnumAllowedValuesAcrossObjectKindsWithSameKey()
    {
        Ra2IniFieldHarvestResult result = Harvest("""
            [InfantryTypes]
            0=E1

            [VehicleTypes]
            0=HTNK

            [AircraftTypes]
            0=ORCA

            [E1]
            Armor=flak

            [HTNK]
            Armor=heavy

            [ORCA]
            Armor=light
            """);

        Assert.All(result.Rows.Where(row => row.Key == "Armor"), row =>
        {
            Assert.Equal(FieldEditorKind.Enum, row.InferredEditorKind);
            Assert.Equal(["flak", "heavy", "light"], row.InferredAllowedValues.Select(value => value.Value).ToArray());
        });
    }

    [Fact]
    public void HarvestCurrentText_GeneralizesSharedUnitFieldsToSingleUnitRow()
    {
        Ra2IniFieldHarvestResult result = Harvest("""
            [InfantryTypes]
            0=E1

            [VehicleTypes]
            0=HTNK

            [AircraftTypes]
            0=ORCA

            [E1]
            Armor=flak

            [HTNK]
            Armor=heavy

            [ORCA]
            Armor=light
            """);

        Ra2IniFieldHarvestRow row = Assert.Single(result.Rows, row => row.Key == "Armor");
        Assert.Equal(Ra2SectionKind.Unit, row.SectionKind);
        Assert.Equal(3, row.OccurrenceCount);
        Assert.Equal(FieldEditorKind.Enum, row.InferredEditorKind);
        Assert.Equal(["flak", "heavy", "light"], row.InferredAllowedValues.Select(value => value.Value).ToArray());
    }

    [Fact]
    public void HarvestCurrentText_GeneralizesSharedTechnoFieldsToSingleTechnoRow()
    {
        Ra2IniFieldHarvestResult result = Harvest("""
            [InfantryTypes]
            0=E1

            [VehicleTypes]
            0=HTNK

            [AircraftTypes]
            0=ORCA

            [BuildingTypes]
            0=GAPILE

            [E1]
            Armor=flak

            [HTNK]
            Armor=heavy

            [ORCA]
            Armor=light

            [GAPILE]
            Armor=wood
            """);

        Ra2IniFieldHarvestRow row = Assert.Single(result.Rows, row => row.Key == "Armor");
        Assert.Equal(Ra2SectionKind.Techno, row.SectionKind);
        Assert.Equal(4, row.OccurrenceCount);
        Assert.Equal(FieldEditorKind.Enum, row.InferredEditorKind);
        Assert.Equal(["flak", "heavy", "light", "wood"], row.InferredAllowedValues.Select(value => value.Value).ToArray());
    }

    [Fact]
    public void HarvestCurrentText_EnumValueThresholdAddsWarningWithoutDowngradingToText()
    {
        string values = string.Join(Environment.NewLine, Enumerable.Range(1, 13).Select(index => $"""
            [E{index}]
            CustomEnum=value_{index}
            """));
        Ra2IniFieldHarvestRow row = SingleRow($"""
            [InfantryTypes]
            {string.Join(Environment.NewLine, Enumerable.Range(1, 13).Select(index => $"{index}=E{index}"))}

            {values}
            """);

        Assert.Equal(FieldEditorKind.Enum, row.InferredEditorKind);
        Assert.Equal(Ra2FieldValueKind.Enum, row.InferredValueKind);
        Assert.Equal(13, row.InferredAllowedValues.Count);
        Assert.Contains(row.Issues, issue => issue.Message.Contains("13 distinct", StringComparison.OrdinalIgnoreCase));
    }

    private Ra2IniFieldHarvestRow SingleRow(string text)
    {
        Ra2IniFieldHarvestResult result = Harvest(text);
        return Assert.Single(result.Rows);
    }

    private Ra2IniFieldHarvestResult Harvest(
        string text,
        IReadOnlyList<Ra2FieldDefinition>? existingDefinitions = null)
    {
        return _harvester.HarvestCurrentText(new Ra2IniFieldHarvestRequest(
            "rulesmd.ini",
            text,
            existingDefinitions ?? []));
    }
}
