using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryHarvestParserTests
{
    [Fact]
    public void Parse_IniLikeKeys_ReturnsHighConfidenceCandidates()
    {
        const string text = """
            Owner=
            Strength=600
            Custom.Flag=yes
            """;

        FieldRegistryHarvestParseResult result = Parse(text);

        Assert.Equal(["Owner", "Strength", "Custom.Flag"], result.Candidates.Select(candidate => candidate.Key).ToArray());
        Assert.All(result.Candidates, candidate => Assert.Equal(FieldRegistryHarvestConfidence.High, candidate.Confidence));
        Assert.Equal([1, 2, 3], result.Candidates.Select(candidate => candidate.LineNumber).ToArray());
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Parse_MarkdownTable_ReturnsRawColumns()
    {
        const string text = """
            | Key | AppliesTo | Type | Description |
            | --- | --- | --- | --- |
            | Owner | Infantry | list | Owner countries |
            | Strength | Building | int | Hit points |
            """;

        FieldRegistryHarvestParseResult result = Parse(text);

        Assert.Equal(2, result.Candidates.Count);
        FieldRegistryHarvestCandidate owner = result.Candidates[0];
        FieldRegistryHarvestCandidate strength = result.Candidates[1];
        Assert.Equal("Owner", owner.Key);
        Assert.Equal("Infantry", owner.AppliesToRaw);
        Assert.Equal("list", owner.EditorKindRaw);
        Assert.Equal("Owner countries", owner.Description);
        Assert.Equal("Strength", strength.Key);
        Assert.Equal("Building", strength.AppliesToRaw);
        Assert.Equal("int", strength.EditorKindRaw);
        Assert.Equal("Hit points", strength.Description);
        Assert.All(result.Candidates, candidate => Assert.Equal(FieldRegistryHarvestConfidence.High, candidate.Confidence));
    }

    [Fact]
    public void Parse_MarkdownTableHeaderNamesAreCaseAndSpacingInsensitive()
    {
        const string text = """
            | key | applies to | editor kind | description |
            | --- | --- | --- | --- |
            | CustomKey | Vehicle | text | Description text |
            """;

        FieldRegistryHarvestCandidate candidate = Assert.Single(Parse(text).Candidates);

        Assert.Equal("CustomKey", candidate.Key);
        Assert.Equal("Vehicle", candidate.AppliesToRaw);
        Assert.Equal("text", candidate.EditorKindRaw);
        Assert.Equal("Description text", candidate.Description);
    }

    [Fact]
    public void Parse_Bullets_ReturnMediumConfidenceCandidates()
    {
        const string text = """
            - Owner: owner countries
            * Strength - hit points
            """;

        FieldRegistryHarvestParseResult result = Parse(text);

        Assert.Equal(["Owner", "Strength"], result.Candidates.Select(candidate => candidate.Key).ToArray());
        Assert.Equal(["owner countries", "hit points"], result.Candidates.Select(candidate => candidate.Description ?? string.Empty).ToArray());
        Assert.All(result.Candidates, candidate => Assert.Equal(FieldRegistryHarvestConfidence.Medium, candidate.Confidence));
    }

    [Fact]
    public void Parse_NonFieldTextIsIgnored()
    {
        const string text = """
            ## Infantry Tags

            This section describes infantry behavior.

            A normal sentence with no field.
            """;

        FieldRegistryHarvestParseResult result = Parse(text);

        Assert.Empty(result.Candidates);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Parse_DuplicateKeyKeepsHigherConfidenceCandidateAndWarns()
    {
        const string text = """
            Owner=
            - Owner: owner countries
            """;

        FieldRegistryHarvestParseResult result = Parse(text);

        FieldRegistryHarvestCandidate candidate = Assert.Single(result.Candidates);
        Assert.Equal("Owner", candidate.Key);
        Assert.Equal(FieldRegistryHarvestConfidence.High, candidate.Confidence);
        FieldRegistryHarvestWarning warning = Assert.Single(result.Warnings);
        Assert.Contains("duplicate", warning.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(2, warning.LineNumber);
    }

    [Fact]
    public void Parse_DuplicateKeyIsCaseInsensitive()
    {
        const string text = """
            Owner=
            owner=GDI
            """;

        FieldRegistryHarvestParseResult result = Parse(text);

        Assert.Single(result.Candidates);
        Assert.Single(result.Warnings);
    }

    [Fact]
    public void Parse_TableBadRowsCreateWarnings()
    {
        const string text = """
            | Key | AppliesTo |
            | --- | --- |
            |  | Infantry |
            | Strength |
            """;

        FieldRegistryHarvestParseResult result = Parse(text);

        Assert.Empty(result.Candidates);
        Assert.Equal(2, result.Warnings.Count);
        Assert.Contains(result.Warnings, warning => warning.Message.Contains("key is empty", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Warnings, warning => warning.Message.Contains("column count", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Parse_EmptyTextReturnsEmptyResult()
    {
        FieldRegistryHarvestParseResult result = Parse(string.Empty);

        Assert.Empty(result.Candidates);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Parse_CrlfAndLfLineNumbersAreOneBased()
    {
        string text = "Owner=\r\n\r\nStrength=600\nCustom.Flag=yes";

        FieldRegistryHarvestParseResult result = Parse(text);

        Assert.Equal([1, 3, 4], result.Candidates.Select(candidate => candidate.LineNumber).ToArray());
    }

    [Fact]
    public void Parse_InvalidIniLikeKeyCreatesWarning()
    {
        const string text = "Bad Key=value";

        FieldRegistryHarvestParseResult result = Parse(text);

        Assert.Empty(result.Candidates);
        FieldRegistryHarvestWarning warning = Assert.Single(result.Warnings);
        Assert.Contains("invalid", warning.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_DoesNotUseFileSystemOrNetworkApis()
    {
        string root = TestRepositoryRoot.Find();
        string harvestRoot = Path.Combine(root, "RA2IniEditor.Infrastructure", "FieldRegistry", "Harvest");
        string harvestText = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(harvestRoot, "*.cs", SearchOption.TopDirectoryOnly)
                .Select(File.ReadAllText));

        Assert.DoesNotContain("HttpClient", harvestText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WebRequest", harvestText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GitHub", harvestText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.", harvestText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Directory.", harvestText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", harvestText, StringComparison.OrdinalIgnoreCase);
    }

    private static FieldRegistryHarvestParseResult Parse(string text)
    {
        MarkdownFieldRegistryHarvestParser parser = new();
        return parser.Parse(new FieldRegistryHarvestDocument("test-doc", text));
    }
}

