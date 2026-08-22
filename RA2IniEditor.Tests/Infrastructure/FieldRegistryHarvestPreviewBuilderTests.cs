using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryHarvestPreviewBuilderTests
{
    [Fact]
    public void BuildPreview_WithoutErrorsAllowsFutureApply()
    {
        FieldRegistryHarvestNormalizeResult normalizeResult = new(
            [Normalized("Owner")],
            [
                Issue(FieldRegistryHarvestValidationSeverity.Info),
                Issue(FieldRegistryHarvestValidationSeverity.Warning)
            ]);

        FieldRegistryHarvestPreviewDraft draft = new FieldRegistryHarvestPreviewBuilder().BuildPreview(normalizeResult);

        Assert.True(draft.CanApplyInFuture);
        Assert.Equal(0, draft.ErrorCount);
        Assert.Equal(1, draft.WarningCount);
        Assert.Single(draft.Definitions);
    }

    [Fact]
    public void BuildPreview_WithErrorBlocksFutureApply()
    {
        FieldRegistryHarvestNormalizeResult normalizeResult = new(
            [Normalized("Owner")],
            [Issue(FieldRegistryHarvestValidationSeverity.Error)]);

        FieldRegistryHarvestPreviewDraft draft = new FieldRegistryHarvestPreviewBuilder().BuildPreview(normalizeResult);

        Assert.False(draft.CanApplyInFuture);
        Assert.Equal(1, draft.ErrorCount);
        Assert.Equal(0, draft.WarningCount);
    }

    [Fact]
    public void BuildPreview_DefinitionsMatchNormalizedCandidates()
    {
        FieldRegistryHarvestNormalizeResult normalizeResult = new(
            [
                Normalized("Owner", [Ra2SectionKind.Infantry], FieldEditorKind.MultiSelect),
                Normalized("Strength", [Ra2SectionKind.Building], FieldEditorKind.Float)
            ],
            []);

        FieldRegistryHarvestPreviewDraft draft = new FieldRegistryHarvestPreviewBuilder().BuildPreview(normalizeResult);

        Assert.Equal(2, draft.Definitions.Count);
        Assert.Equal(["Owner", "Strength"], draft.Definitions.Select(definition => definition.Key).ToArray());
        Assert.Equal(FieldEditorKind.MultiSelect, draft.Definitions[0].EditorKind);
        Assert.Equal([Ra2SectionKind.Building], draft.Definitions[1].AppliesTo);
    }

    [Fact]
    public void BuildPreview_BooleanCandidateCreatesYesNoValueMetadata()
    {
        FieldRegistryHarvestNormalizeResult normalizeResult = new(
            [Normalized("CustomBoolean", [Ra2SectionKind.Infantry], FieldEditorKind.Boolean)],
            []);

        FieldRegistryHarvestPreviewDraft draft = new FieldRegistryHarvestPreviewBuilder().BuildPreview(normalizeResult);

        Ra2FieldDefinition definition = Assert.Single(draft.Definitions);
        Assert.Equal(Ra2FieldValueKind.Boolean, definition.ValueMetadata.ValueKind);
        Assert.Equal(Ra2FieldBooleanValueStyle.YesNo, definition.ValueMetadata.BooleanStyle);
    }

    [Fact]
    public void BuildPreview_EnumCandidateDoesNotInventValueMetadata()
    {
        FieldRegistryHarvestNormalizeResult normalizeResult = new(
            [Normalized("CustomEnum", [Ra2SectionKind.Infantry], FieldEditorKind.Enum)],
            []);

        FieldRegistryHarvestPreviewDraft draft = new FieldRegistryHarvestPreviewBuilder().BuildPreview(normalizeResult);

        Ra2FieldDefinition definition = Assert.Single(draft.Definitions);
        Assert.False(definition.ValueMetadata.HasSchema);
    }

    [Fact]
    public void Pipeline_ParseNormalizePreview_StaysInMemoryAndBuildsDraft()
    {
        const string text = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | Owner | Infantry | Text | Owner countries |
            | Strength | Building | Float | Hit points |
            """;

        MarkdownFieldRegistryHarvestParser parser = new();
        FieldRegistryHarvestParseResult parseResult = parser.Parse(new FieldRegistryHarvestDocument("raw.md", text));
        FieldRegistryHarvestNormalizeResult normalizeResult = new FieldRegistryHarvestNormalizer().Normalize(
            parseResult.Candidates,
            FieldRegistryHarvestNormalizeOptions.Default);
        FieldRegistryHarvestPreviewDraft draft = new FieldRegistryHarvestPreviewBuilder().BuildPreview(normalizeResult);

        Assert.Equal(2, parseResult.Candidates.Count);
        Assert.Equal(2, normalizeResult.Candidates.Count);
        Assert.Equal(2, draft.Definitions.Count);
        Assert.True(draft.CanApplyInFuture);
    }

    private static FieldRegistryHarvestNormalizedCandidate Normalized(
        string key,
        IReadOnlyList<Ra2SectionKind>? appliesTo = null,
        FieldEditorKind editorKind = FieldEditorKind.Text)
    {
        return new FieldRegistryHarvestNormalizedCandidate(
            key,
            appliesTo ?? [Ra2SectionKind.Unknown],
            editorKind,
            Ra2FieldSourceKind.External,
            "description",
            "test-doc",
            1,
            key,
            FieldRegistryHarvestConfidence.High,
            usedDefaultAppliesTo: false,
            usedDefaultEditorKind: false);
    }

    private static FieldRegistryHarvestValidationIssue Issue(FieldRegistryHarvestValidationSeverity severity)
        => new("test-doc", 1, "Owner", severity, $"{severity} issue");
}
