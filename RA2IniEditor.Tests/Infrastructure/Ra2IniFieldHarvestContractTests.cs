using System.Reflection;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest.Ini;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class Ra2IniFieldHarvestContractTests
{
    [Fact]
    public void CurrentTextHarvesterUsesRequestObjectAndDoesNotExposeFilePathParameter()
    {
        MethodInfo? method = typeof(IRa2IniFieldHarvester).GetMethod(nameof(IRa2IniFieldHarvester.HarvestCurrentText));

        Assert.NotNull(method);
        ParameterInfo parameter = Assert.Single(method.GetParameters());
        Assert.Equal(typeof(Ra2IniFieldHarvestRequest), parameter.ParameterType);
        Assert.Equal(typeof(Ra2IniFieldHarvestResult), method.ReturnType);
    }

    [Fact]
    public void CurrentTextHarvestRequestHasNoPathProperty()
    {
        string[] propertyNames = typeof(Ra2IniFieldHarvestRequest)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(property => property.Name)
            .ToArray();

        Assert.Contains(nameof(Ra2IniFieldHarvestRequest.SourceName), propertyNames);
        Assert.Contains(nameof(Ra2IniFieldHarvestRequest.Text), propertyNames);
        Assert.Contains(nameof(Ra2IniFieldHarvestRequest.ExistingDefinitions), propertyNames);
        Assert.DoesNotContain(propertyNames, name => name.Contains("FilePath", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(propertyNames, name => string.Equals(name, "Path", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ProjectHarvestContractRequiresDiscoveredFilesAndExcludedDirectories()
    {
        MethodInfo? method = typeof(IRa2IniProjectFieldHarvester).GetMethod(nameof(IRa2IniProjectFieldHarvester.HarvestProject));

        Assert.NotNull(method);
        ParameterInfo parameter = Assert.Single(method.GetParameters());
        Assert.Equal(typeof(Ra2IniProjectFieldHarvestRequest), parameter.ParameterType);
        Assert.Equal(typeof(Ra2IniFieldHarvestResult), method.ReturnType);

        Assert.NotNull(typeof(Ra2IniProjectFieldHarvestRequest).GetProperty(nameof(Ra2IniProjectFieldHarvestRequest.DiscoveredIniFilePaths)));
        Assert.NotNull(typeof(Ra2IniProjectFieldHarvestRequest).GetProperty(nameof(Ra2IniProjectFieldHarvestRequest.ExcludedDirectoryNames)));
    }

    [Fact]
    public void DraftRowKeepsKeyReadonlyButAllowsUserEditableMetadata()
    {
        Ra2FieldImportDraftRow row = new(
            isEnabled: true,
            key: "Armor",
            sectionKind: Ra2SectionKind.Infantry,
            occurrenceCount: 3,
            sampleValueSummary: "light, heavy",
            editorKind: FieldEditorKind.Enum,
            valueKind: Ra2FieldValueKind.Enum,
            booleanStyle: Ra2FieldBooleanValueStyle.Unknown,
            allowedValuesText: "light|轻甲;heavy|重甲",
            displayName: "装甲",
            description: "Armor type.",
            sourceNote: "rulesmd.ini",
            issueSummary: "样例值可能不完整");

        row.IsEnabled = false;
        row.SectionKind = Ra2SectionKind.Vehicle;
        row.EditorKind = FieldEditorKind.Text;
        row.ValueKind = Ra2FieldValueKind.String;
        row.BooleanStyle = Ra2FieldBooleanValueStyle.Custom;
        row.AllowedValuesText = "custom";
        row.DisplayName = "Custom";
        row.Description = "Updated";
        row.SourceNote = "manual";

        Assert.Equal("Armor", row.Key);
        Assert.False(row.IsEnabled);
        Assert.Equal(Ra2SectionKind.Vehicle, row.SectionKind);
        Assert.Equal(FieldEditorKind.Text, row.EditorKind);
        Assert.Equal(Ra2FieldValueKind.String, row.ValueKind);
        Assert.Equal(Ra2FieldBooleanValueStyle.Custom, row.BooleanStyle);
        Assert.Equal("custom", row.AllowedValuesText);
        Assert.Equal("Custom", row.DisplayName);
        Assert.Equal("Updated", row.Description);
        Assert.Equal("manual", row.SourceNote);
        Assert.Equal("样例值可能不完整", row.IssueSummary);
    }
}
