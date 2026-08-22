using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest.Ini;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class Ra2FieldImportDraftBuilderTests
{
    private readonly Ra2FieldImportDraftBuilder _builder = new();

    [Fact]
    public void BuildDraft_UsesHarvestInferenceAndFormatsAllowedValues()
    {
        Ra2IniFieldHarvestResult harvest = new(
            [
                new Ra2IniFieldHarvestRow(
                    "Armor",
                    Ra2SectionKind.Infantry,
                    occurrenceCount: 3,
                    sampleValues: ["none", "flak"],
                    sourceNames: ["rulesmd.ini"],
                    inferredEditorKind: FieldEditorKind.Enum,
                    inferredValueKind: Ra2FieldValueKind.Enum,
                    inferredBooleanStyle: Ra2FieldBooleanValueStyle.Unknown,
                    inferredAllowedValues:
                    [
                        new Ra2FieldAllowedValue("none", "None armor", "No armor."),
                        new Ra2FieldAllowedValue("flak")
                    ],
                    issues:
                    [
                        new FieldRegistryHarvestValidationIssue(
                            "rulesmd.ini",
                            0,
                            "Armor",
                            FieldRegistryHarvestValidationSeverity.Warning,
                            "Sample values may be incomplete.")
                    ])
            ],
            []);

        IReadOnlyList<Ra2FieldImportDraftRow> rows = _builder.BuildDraft(harvest);

        Ra2FieldImportDraftRow row = Assert.Single(rows);
        Assert.True(row.IsEnabled);
        Assert.Equal("Armor", row.Key);
        Assert.Equal(Ra2SectionKind.Infantry, row.SectionKind);
        Assert.Equal("none, flak", row.SampleValueSummary);
        Assert.Equal(FieldEditorKind.Enum, row.EditorKind);
        Assert.Equal(Ra2FieldValueKind.Enum, row.ValueKind);
        Assert.Contains("none|None armor|No armor.", row.AllowedValuesText, StringComparison.Ordinal);
        Assert.Contains("flak", row.AllowedValuesText, StringComparison.Ordinal);
        Assert.Equal("rulesmd.ini", row.SourceNote);
        Assert.Contains("Sample values", row.IssueSummary, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildPreviewFromDraft_SkipsDisabledRows()
    {
        Ra2FieldImportDraftRow row = CreateDraftRow();
        row.IsEnabled = false;

        FieldRegistryHarvestPreviewDraft preview = _builder.BuildPreviewFromDraft([row]);

        Assert.Empty(preview.Definitions);
        Assert.Empty(preview.Issues);
    }

    [Fact]
    public void BuildPreviewFromDraft_UsesUserEditedMetadata()
    {
        Ra2FieldImportDraftRow row = CreateDraftRow();
        row.SectionKind = Ra2SectionKind.Vehicle;
        row.EditorKind = FieldEditorKind.Text;
        row.ValueKind = Ra2FieldValueKind.String;
        row.BooleanStyle = Ra2FieldBooleanValueStyle.Unknown;
        row.AllowedValuesText = "ignored";
        row.Description = "User edited description.";

        FieldRegistryHarvestPreviewDraft preview = _builder.BuildPreviewFromDraft([row]);

        Ra2FieldDefinition definition = Assert.Single(preview.Definitions);
        Assert.Equal("Armor", definition.Key);
        Assert.Equal([Ra2SectionKind.Vehicle], definition.AppliesTo.ToArray());
        Assert.Equal(FieldEditorKind.Text, definition.EditorKind);
        Assert.Equal(Ra2FieldSourceKind.User, definition.SourceKind);
        Assert.Equal("User edited description.", definition.Description);
        Assert.False(definition.ValueMetadata.HasSchema);
    }

    [Fact]
    public void BuildPreviewFromDraft_ParsesAllowedValuesIntoMetadata()
    {
        Ra2FieldImportDraftRow row = CreateDraftRow();
        row.AllowedValuesText = "light|Light armor;heavy|Heavy armor|Slow units";

        FieldRegistryHarvestPreviewDraft preview = _builder.BuildPreviewFromDraft([row]);

        Ra2FieldDefinition definition = Assert.Single(preview.Definitions);
        Assert.Equal(Ra2FieldValueKind.Enum, definition.ValueMetadata.ValueKind);
        Assert.Equal(["light", "heavy"], definition.ValueMetadata.AllowedValues.Select(value => value.Value).ToArray());
        Assert.Equal("Light armor", definition.ValueMetadata.AllowedValues.First().DisplayName);
        Assert.Equal("Slow units", definition.ValueMetadata.AllowedValues.Last().Description);
        Assert.Empty(preview.Issues);
    }

    [Fact]
    public void BuildPreviewFromDraft_PreservesParserWarningsAsIssues()
    {
        Ra2FieldImportDraftRow row = CreateDraftRow();
        row.AllowedValuesText = "light;;heavy";

        FieldRegistryHarvestPreviewDraft preview = _builder.BuildPreviewFromDraft([row]);

        Assert.Single(preview.Definitions);
        FieldRegistryHarvestValidationIssue issue = Assert.Single(preview.Issues);
        Assert.Equal(FieldRegistryHarvestValidationSeverity.Warning, issue.Severity);
        Assert.Contains("Empty", issue.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildPreviewFromDraft_WarnsForEnumWithoutAllowedValues()
    {
        Ra2FieldImportDraftRow row = CreateDraftRow();
        row.AllowedValuesText = "";

        FieldRegistryHarvestPreviewDraft preview = _builder.BuildPreviewFromDraft([row]);

        Assert.Single(preview.Definitions);
        Assert.Contains(preview.Issues, issue => issue.Severity == FieldRegistryHarvestValidationSeverity.Warning);
    }

    [Fact]
    public void BuildPreviewFromDraft_CustomBooleanWithoutValuesWarns()
    {
        Ra2FieldImportDraftRow row = CreateDraftRow();
        row.EditorKind = FieldEditorKind.Boolean;
        row.ValueKind = Ra2FieldValueKind.Boolean;
        row.BooleanStyle = Ra2FieldBooleanValueStyle.Custom;
        row.AllowedValuesText = "";

        FieldRegistryHarvestPreviewDraft preview = _builder.BuildPreviewFromDraft([row]);

        Ra2FieldDefinition definition = Assert.Single(preview.Definitions);
        Assert.Equal(Ra2FieldBooleanValueStyle.Custom, definition.ValueMetadata.BooleanStyle);
        Assert.Contains(preview.Issues, issue => issue.Severity == FieldRegistryHarvestValidationSeverity.Warning);
    }

    private static Ra2FieldImportDraftRow CreateDraftRow()
    {
        return new Ra2FieldImportDraftRow(
            isEnabled: true,
            key: "Armor",
            sectionKind: Ra2SectionKind.Infantry,
            occurrenceCount: 2,
            sampleValueSummary: "light, heavy",
            editorKind: FieldEditorKind.Enum,
            valueKind: Ra2FieldValueKind.Enum,
            booleanStyle: Ra2FieldBooleanValueStyle.Unknown,
            allowedValuesText: "light;heavy",
            displayName: null,
            description: "Armor type.",
            sourceNote: "rulesmd.ini",
            issueSummary: "");
    }
}
