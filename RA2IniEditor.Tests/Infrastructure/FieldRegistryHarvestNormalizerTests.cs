using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryHarvestNormalizerTests
{
    [Fact]
    public void Normalize_KeyTrimsAndPreservesValidCharacters()
    {
        FieldRegistryHarvestNormalizeResult result = Normalize(
        [
            Candidate(" Owner ", appliesToRaw: "Infantry", editorKindRaw: "Text"),
            Candidate("Custom.Flag", appliesToRaw: "Building", editorKindRaw: "String")
        ]);

        Assert.Equal(["Owner", "Custom.Flag"], result.Candidates.Select(candidate => candidate.Key).ToArray());
        Assert.Empty(result.Issues);
    }

    [Theory]
    [InlineData("Bad Key")]
    [InlineData("Bad=Key")]
    [InlineData("")]
    public void Normalize_InvalidKeyCreatesErrorAndSkipsCandidate(string key)
    {
        FieldRegistryHarvestNormalizeResult result = Normalize(
        [
            Candidate(key, appliesToRaw: "Infantry", editorKindRaw: "Text")
        ]);

        Assert.Empty(result.Candidates);
        FieldRegistryHarvestValidationIssue issue = Assert.Single(result.Issues);
        Assert.Equal(FieldRegistryHarvestValidationSeverity.Error, issue.Severity);
    }

    [Fact]
    public void Normalize_AppliesToMapsAliasesAndMultipleValues()
    {
        FieldRegistryHarvestNormalizeResult result = Normalize(
        [
            Candidate("Owner", appliesToRaw: "Inf", editorKindRaw: "Text"),
            Candidate("Cost", appliesToRaw: "Vehicle; Building", editorKindRaw: "Text")
        ]);

        Assert.Equal([Ra2SectionKind.Infantry], result.Candidates[0].AppliesTo);
        Assert.Equal([Ra2SectionKind.Vehicle, Ra2SectionKind.Building], result.Candidates[1].AppliesTo);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Normalize_MissingAppliesToUsesDefaultAndCreatesInfo()
    {
        FieldRegistryHarvestNormalizeResult result = Normalize(
        [
            Candidate("Owner", appliesToRaw: null, editorKindRaw: "Text")
        ],
        new FieldRegistryHarvestNormalizeOptions { DefaultAppliesTo = Ra2SectionKind.Building });

        FieldRegistryHarvestNormalizedCandidate candidate = Assert.Single(result.Candidates);
        Assert.Equal([Ra2SectionKind.Building], candidate.AppliesTo);
        Assert.True(candidate.UsedDefaultAppliesTo);
        FieldRegistryHarvestValidationIssue issue = Assert.Single(result.Issues);
        Assert.Equal(FieldRegistryHarvestValidationSeverity.Info, issue.Severity);
    }

    [Fact]
    public void Normalize_UnknownAppliesToAllowedMapsUnknownAndWarns()
    {
        FieldRegistryHarvestNormalizeResult result = Normalize(
        [
            Candidate("Owner", appliesToRaw: "Alien", editorKindRaw: "Text")
        ]);

        FieldRegistryHarvestNormalizedCandidate candidate = Assert.Single(result.Candidates);
        Assert.Equal([Ra2SectionKind.Unknown], candidate.AppliesTo);
        FieldRegistryHarvestValidationIssue issue = Assert.Single(result.Issues);
        Assert.Equal(FieldRegistryHarvestValidationSeverity.Warning, issue.Severity);
    }

    [Fact]
    public void Normalize_UnknownAppliesToDisallowedCreatesErrorAndSkipsCandidate()
    {
        FieldRegistryHarvestNormalizeResult result = Normalize(
        [
            Candidate("Owner", appliesToRaw: "Alien", editorKindRaw: "Text")
        ],
        new FieldRegistryHarvestNormalizeOptions { AllowUnknownAppliesTo = false });

        Assert.Empty(result.Candidates);
        FieldRegistryHarvestValidationIssue issue = Assert.Single(result.Issues);
        Assert.Equal(FieldRegistryHarvestValidationSeverity.Error, issue.Severity);
    }

    [Theory]
    [InlineData("Text", FieldEditorKind.Text)]
    [InlineData("String", FieldEditorKind.Text)]
    [InlineData("Float", FieldEditorKind.Float)]
    [InlineData("Double", FieldEditorKind.Float)]
    public void Normalize_EditorKindMapsAliases(string raw, FieldEditorKind expected)
    {
        FieldRegistryHarvestNormalizeResult result = Normalize(
        [
            Candidate("Owner", appliesToRaw: "Infantry", editorKindRaw: raw)
        ]);

        FieldRegistryHarvestNormalizedCandidate candidate = Assert.Single(result.Candidates);
        Assert.Equal(expected, candidate.EditorKind);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Normalize_MissingEditorKindUsesDefaultAndCreatesInfo()
    {
        FieldRegistryHarvestNormalizeResult result = Normalize(
        [
            Candidate("Owner", appliesToRaw: "Infantry", editorKindRaw: null)
        ],
        new FieldRegistryHarvestNormalizeOptions { DefaultEditorKind = FieldEditorKind.Reference });

        FieldRegistryHarvestNormalizedCandidate candidate = Assert.Single(result.Candidates);
        Assert.Equal(FieldEditorKind.Reference, candidate.EditorKind);
        Assert.True(candidate.UsedDefaultEditorKind);
        FieldRegistryHarvestValidationIssue issue = Assert.Single(result.Issues);
        Assert.Equal(FieldRegistryHarvestValidationSeverity.Info, issue.Severity);
    }

    [Fact]
    public void Normalize_UnknownEditorKindAllowedUsesDefaultAndWarns()
    {
        FieldRegistryHarvestNormalizeResult result = Normalize(
        [
            Candidate("Owner", appliesToRaw: "Infantry", editorKindRaw: "SpecialPicker")
        ]);

        FieldRegistryHarvestNormalizedCandidate candidate = Assert.Single(result.Candidates);
        Assert.Equal(FieldEditorKind.Text, candidate.EditorKind);
        Assert.True(candidate.UsedDefaultEditorKind);
        FieldRegistryHarvestValidationIssue issue = Assert.Single(result.Issues);
        Assert.Equal(FieldRegistryHarvestValidationSeverity.Warning, issue.Severity);
    }

    [Fact]
    public void Normalize_UnknownEditorKindDisallowedCreatesErrorAndSkipsCandidate()
    {
        FieldRegistryHarvestNormalizeResult result = Normalize(
        [
            Candidate("Owner", appliesToRaw: "Infantry", editorKindRaw: "SpecialPicker")
        ],
        new FieldRegistryHarvestNormalizeOptions { AllowUnknownEditorKind = false });

        Assert.Empty(result.Candidates);
        FieldRegistryHarvestValidationIssue issue = Assert.Single(result.Issues);
        Assert.Equal(FieldRegistryHarvestValidationSeverity.Error, issue.Severity);
    }

    [Fact]
    public void Normalize_DuplicateKeyAndSameAppliesToKeepsHigherConfidence()
    {
        FieldRegistryHarvestNormalizeResult result = Normalize(
        [
            Candidate("Owner", appliesToRaw: "Infantry", editorKindRaw: "Text", confidence: FieldRegistryHarvestConfidence.Low, lineNumber: 1),
            Candidate("owner", appliesToRaw: "Infantry", editorKindRaw: "Reference", confidence: FieldRegistryHarvestConfidence.High, lineNumber: 2)
        ]);

        FieldRegistryHarvestNormalizedCandidate candidate = Assert.Single(result.Candidates);
        Assert.Equal("owner", candidate.Key);
        Assert.Equal(FieldRegistryHarvestConfidence.High, candidate.Confidence);
        Assert.Equal(FieldEditorKind.Reference, candidate.EditorKind);
        FieldRegistryHarvestValidationIssue issue = Assert.Single(result.Issues);
        Assert.Equal(FieldRegistryHarvestValidationSeverity.Warning, issue.Severity);
    }

    [Fact]
    public void Normalize_SameKeyDifferentAppliesToIsNotDuplicate()
    {
        FieldRegistryHarvestNormalizeResult result = Normalize(
        [
            Candidate("Owner", appliesToRaw: "Infantry", editorKindRaw: "Text"),
            Candidate("Owner", appliesToRaw: "Vehicle", editorKindRaw: "Text")
        ]);

        Assert.Equal(2, result.Candidates.Count);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Normalize_DescriptionTrimsEmptyToNullAndUsesDefaultSourceKind()
    {
        FieldRegistryHarvestNormalizeResult result = Normalize(
        [
            Candidate("Owner", appliesToRaw: "Infantry", editorKindRaw: "Text", description: "   ")
        ],
        new FieldRegistryHarvestNormalizeOptions { DefaultSourceKind = Ra2FieldSourceKind.User });

        FieldRegistryHarvestNormalizedCandidate candidate = Assert.Single(result.Candidates);
        Assert.Null(candidate.Description);
        Assert.Equal(Ra2FieldSourceKind.User, candidate.SourceKind);
    }

    private static FieldRegistryHarvestNormalizeResult Normalize(
        IReadOnlyList<FieldRegistryHarvestCandidate> candidates,
        FieldRegistryHarvestNormalizeOptions? options = null)
    {
        FieldRegistryHarvestNormalizer normalizer = new();
        return normalizer.Normalize(candidates, options ?? FieldRegistryHarvestNormalizeOptions.Default);
    }

    private static FieldRegistryHarvestCandidate Candidate(
        string key,
        string? appliesToRaw,
        string? editorKindRaw,
        string? description = "description",
        FieldRegistryHarvestConfidence confidence = FieldRegistryHarvestConfidence.High,
        int lineNumber = 1)
    {
        return new FieldRegistryHarvestCandidate(
            key,
            appliesToRaw,
            editorKindRaw,
            description,
            "test-doc",
            lineNumber,
            key,
            confidence);
    }
}
