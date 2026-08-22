using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Generalization;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class Ra2FieldDraftGeneralizationPipelineTests
{
    [Fact]
    public void Generalize_InfantryVehicleAircraftBuildingToTechno()
    {
        FieldRegistryHarvestPreviewDraft draft = Draft(
            Definition("Armor", Ra2SectionKind.Infantry, "flak"),
            Definition("Armor", Ra2SectionKind.Vehicle, "heavy"),
            Definition("Armor", Ra2SectionKind.Aircraft, "light"),
            Definition("Armor", Ra2SectionKind.Building, "concrete"));

        Ra2FieldDraftGeneralizationResult result = new Ra2FieldDraftGeneralizationPipeline().Generalize(draft);

        Ra2FieldDefinition definition = Assert.Single(result.PreviewDraft.Definitions);
        Assert.Equal("Armor", definition.Key);
        Assert.Equal(Ra2SectionKind.Techno, Assert.Single(definition.AppliesTo));
        Assert.Equal(["concrete", "flak", "heavy", "light"], definition.ValueMetadata.AllowedValues.Select(value => value.Value).ToArray());
        Ra2FieldDraftGeneralizationNotice notice = Assert.Single(result.Notices);
        Assert.Equal(Ra2SectionKind.Techno, notice.TargetKind);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Generalize_InfantryVehicleAircraftToUnitWhenBuildingIsMissing()
    {
        FieldRegistryHarvestPreviewDraft draft = Draft(
            Definition("Speed", Ra2SectionKind.Infantry),
            Definition("Speed", Ra2SectionKind.Vehicle),
            Definition("Speed", Ra2SectionKind.Aircraft));

        Ra2FieldDraftGeneralizationResult result = new Ra2FieldDraftGeneralizationPipeline().Generalize(draft);

        Ra2FieldDefinition definition = Assert.Single(result.PreviewDraft.Definitions);
        Assert.Equal(Ra2SectionKind.Unit, Assert.Single(definition.AppliesTo));
        Assert.Equal(Ra2SectionKind.Unit, Assert.Single(result.Notices).TargetKind);
    }

    [Fact]
    public void Generalize_MergesExistingAbstractTargetAndRemovesConcreteRows()
    {
        FieldRegistryHarvestPreviewDraft draft = Draft(
            Definition("Armor", Ra2SectionKind.Techno, "wood"),
            Definition("Armor", Ra2SectionKind.Infantry, "flak"),
            Definition("Armor", Ra2SectionKind.Vehicle, "heavy"),
            Definition("Armor", Ra2SectionKind.Aircraft, "light"),
            Definition("Armor", Ra2SectionKind.Building, "concrete"));

        Ra2FieldDraftGeneralizationResult result = new Ra2FieldDraftGeneralizationPipeline().Generalize(draft);

        Ra2FieldDefinition definition = Assert.Single(result.PreviewDraft.Definitions);
        Assert.Equal(Ra2SectionKind.Techno, Assert.Single(definition.AppliesTo));
        Assert.Equal(["concrete", "flak", "heavy", "light", "wood"], definition.ValueMetadata.AllowedValues.Select(value => value.Value).ToArray());
    }

    [Fact]
    public void Generalize_IncompatibleDefinitionsAreSkippedWithWarning()
    {
        FieldRegistryHarvestPreviewDraft draft = Draft(
            Definition("Armor", Ra2SectionKind.Infantry, FieldEditorKind.Enum),
            Definition("Armor", Ra2SectionKind.Vehicle, FieldEditorKind.Text),
            Definition("Armor", Ra2SectionKind.Aircraft, FieldEditorKind.Enum),
            Definition("Armor", Ra2SectionKind.Building, FieldEditorKind.Enum));

        Ra2FieldDraftGeneralizationResult result = new Ra2FieldDraftGeneralizationPipeline().Generalize(draft);

        Assert.Equal(4, result.PreviewDraft.Definitions.Count);
        Assert.Empty(result.Notices);
        Assert.Single(result.Warnings);
    }

    [Fact]
    public void Generalize_DoesNotGeneralizePartialTwoSectionGroups()
    {
        FieldRegistryHarvestPreviewDraft draft = Draft(
            Definition("Armor", Ra2SectionKind.Infantry),
            Definition("Armor", Ra2SectionKind.Vehicle));

        Ra2FieldDraftGeneralizationResult result = new Ra2FieldDraftGeneralizationPipeline().Generalize(draft);

        Assert.Equal(2, result.PreviewDraft.Definitions.Count);
        Assert.Empty(result.Notices);
        Assert.Empty(result.Warnings);
    }

    private static FieldRegistryHarvestPreviewDraft Draft(params Ra2FieldDefinition[] definitions)
        => new(Array.AsReadOnly(definitions), []);

    private static Ra2FieldDefinition Definition(string key, Ra2SectionKind sectionKind, string? allowedValue = null)
        => Definition(key, sectionKind, FieldEditorKind.Enum, allowedValue);

    private static Ra2FieldDefinition Definition(
        string key,
        Ra2SectionKind sectionKind,
        FieldEditorKind editorKind,
        string? allowedValue = null)
    {
        IReadOnlyList<Ra2FieldAllowedValue> allowedValues = string.IsNullOrWhiteSpace(allowedValue)
            ? []
            : [new Ra2FieldAllowedValue(allowedValue)];

        Ra2FieldValueMetadata metadata = editorKind == FieldEditorKind.Enum
            ? new Ra2FieldValueMetadata(Ra2FieldValueKind.Enum, allowedValues: allowedValues)
            : Ra2FieldValueMetadata.Unknown;

        return new Ra2FieldDefinition(
            key,
            [sectionKind],
            editorKind,
            Ra2FieldSourceKind.User,
            "Imported",
            metadata);
    }
}
