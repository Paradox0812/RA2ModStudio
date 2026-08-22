using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FieldValueCompletionCatalogTests
{
    [Fact]
    public void BuiltInCatalog_BooleanKnownYesNoFieldReturnsRawYesNoValues()
    {
        BuiltInRa2FieldValueCompletionCatalog catalog = new();

        IReadOnlyList<Ra2FieldValueCompletionCandidate> candidates = catalog.GetCandidates(Request(
            "Crusher",
            FieldEditorKind.Boolean,
            new Ra2ValueCompletionContext(string.Empty, string.Empty, false, [])));

        Assert.Equal(["no", "yes"], candidates.Select(candidate => candidate.Value).Order(StringComparer.OrdinalIgnoreCase));
        Assert.All(candidates, candidate =>
        {
            Assert.Equal(Ra2CompletionItemKind.Value, candidate.Kind);
            Assert.Equal(Ra2FieldValueCompletionSourceKind.BuiltIn, candidate.SourceKind);
            Assert.Equal("Boolean", candidate.DisplayName);
        });
    }

    [Theory]
    [InlineData("t", "true")]
    [InlineData("tr", "true")]
    [InlineData("f", "false")]
    [InlineData("y", "yes")]
    [InlineData("n", "no")]
    public void BuiltInCatalog_UnknownBooleanStyleUsesConservativePrefixFallback(string prefix, string expected)
    {
        BuiltInRa2FieldValueCompletionCatalog catalog = new();

        IReadOnlyList<Ra2FieldValueCompletionCandidate> candidates = catalog.GetCandidates(Request(
            "CustomBool",
            FieldEditorKind.Boolean,
            new Ra2ValueCompletionContext(prefix, prefix, false, [])));

        Ra2FieldValueCompletionCandidate candidate = Assert.Single(candidates);
        Assert.Equal(expected, candidate.Value);
    }

    [Fact]
    public void BuiltInCatalog_UnknownBooleanStyleWithoutPrefixReturnsEmpty()
    {
        BuiltInRa2FieldValueCompletionCatalog catalog = new();

        IReadOnlyList<Ra2FieldValueCompletionCandidate> candidates = catalog.GetCandidates(Request(
            "CustomBool",
            FieldEditorKind.Boolean,
            new Ra2ValueCompletionContext(string.Empty, string.Empty, false, [])));

        Assert.Empty(candidates);
    }

    [Fact]
    public void BuiltInCatalog_ArmorEnumFiltersByPrefix()
    {
        BuiltInRa2FieldValueCompletionCatalog catalog = new();

        IReadOnlyList<Ra2FieldValueCompletionCandidate> candidates = catalog.GetCandidates(Request(
            "Armor",
            FieldEditorKind.Enum,
            new Ra2ValueCompletionContext("he", "he", false, [])));

        Ra2FieldValueCompletionCandidate candidate = Assert.Single(candidates);
        Assert.Equal("heavy", candidate.Value);
        Assert.Equal("Enum", candidate.DisplayName);
    }

    [Fact]
    public void BuiltInCatalog_ListCandidatesSkipExistingTokens()
    {
        BuiltInRa2FieldValueCompletionCatalog catalog = new();

        IReadOnlyList<Ra2FieldValueCompletionCandidate> candidates = catalog.GetCandidates(Request(
            "Owner",
            FieldEditorKind.MultiSelect,
            new Ra2ValueCompletionContext("Americans,Bri", "Bri", true, ["Americans"])));

        Assert.Contains(candidates, candidate => candidate.Value == "British");
        Assert.DoesNotContain(candidates, candidate => candidate.Value == "Americans");
    }

    [Fact]
    public void CompositeCatalog_DeduplicatesByValueAndKeepsHigherPriorityCandidate()
    {
        CompositeRa2FieldValueCompletionCatalog catalog = new([
            new FakeCatalog(new Ra2FieldValueCompletionCandidate(
                "heavy",
                "Low",
                "Low priority",
                Ra2CompletionItemKind.Value,
                10,
                Ra2FieldValueCompletionSourceKind.BuiltIn)),
            new FakeCatalog(new Ra2FieldValueCompletionCandidate(
                "heavy",
                "High",
                "High priority",
                Ra2CompletionItemKind.Value,
                100,
                Ra2FieldValueCompletionSourceKind.User))
        ]);

        Ra2FieldValueCompletionCandidate candidate = Assert.Single(catalog.GetCandidates(Request(
            "Armor",
            FieldEditorKind.Enum,
            new Ra2ValueCompletionContext("he", "he", false, []))));

        Assert.Equal("High", candidate.DisplayName);
        Assert.Equal(Ra2FieldValueCompletionSourceKind.User, candidate.SourceKind);
    }

    [Fact]
    public void FieldRegistryCatalog_MetadataEnumReturnsAllowedRawValues()
    {
        FieldRegistryRa2FieldValueCompletionCatalog catalog = new();
        Ra2FieldValueMetadata metadata = new(
            Ra2FieldValueKind.Enum,
            allowedValues:
            [
                new Ra2FieldAllowedValue("heavy", "Heavy armor", "Tank armor.", priority: 3),
                new Ra2FieldAllowedValue("light", "Light armor")
            ]);

        IReadOnlyList<Ra2FieldValueCompletionCandidate> candidates = catalog.GetCandidates(Request(
            "Armor",
            FieldEditorKind.Enum,
            new Ra2ValueCompletionContext("he", "he", false, []),
            metadata));

        Ra2FieldValueCompletionCandidate candidate = Assert.Single(candidates);
        Assert.Equal("heavy", candidate.Value);
        Assert.Equal("Heavy armor", candidate.DisplayName);
        Assert.Equal("Tank armor.", candidate.Description);
        Assert.Equal(Ra2FieldValueCompletionSourceKind.FieldRegistry, candidate.SourceKind);
        Assert.True(candidate.Priority > 200);
    }

    [Fact]
    public void FieldRegistryCatalog_MetadataBooleanStyleGeneratesTrueFalseValues()
    {
        FieldRegistryRa2FieldValueCompletionCatalog catalog = new();
        Ra2FieldValueMetadata metadata = new(
            Ra2FieldValueKind.Boolean,
            Ra2FieldBooleanValueStyle.TrueFalse);

        IReadOnlyList<Ra2FieldValueCompletionCandidate> candidates = catalog.GetCandidates(Request(
            "CustomBool",
            FieldEditorKind.Boolean,
            new Ra2ValueCompletionContext(string.Empty, string.Empty, false, []),
            metadata));

        Assert.Equal(["false", "true"], candidates.Select(candidate => candidate.Value).Order(StringComparer.OrdinalIgnoreCase));
        Assert.All(candidates, candidate => Assert.Equal(Ra2FieldValueCompletionSourceKind.FieldRegistry, candidate.SourceKind));
    }

    [Fact]
    public void FieldRegistryCatalog_MetadataEnumListSkipsExistingTokens()
    {
        FieldRegistryRa2FieldValueCompletionCatalog catalog = new();
        Ra2FieldValueMetadata metadata = new(
            Ra2FieldValueKind.EnumList,
            allowedValues:
            [
                new Ra2FieldAllowedValue("Americans"),
                new Ra2FieldAllowedValue("British")
            ]);

        IReadOnlyList<Ra2FieldValueCompletionCandidate> candidates = catalog.GetCandidates(Request(
            "Owner",
            FieldEditorKind.MultiSelect,
            new Ra2ValueCompletionContext("Americans,Bri", "Bri", true, ["Americans"]),
            metadata));

        Ra2FieldValueCompletionCandidate candidate = Assert.Single(candidates);
        Assert.Equal("British", candidate.Value);
        Assert.DoesNotContain(candidates, item => item.Value == "Americans");
    }

    [Fact]
    public void CompositeCatalog_FieldRegistryMetadataBeatsBuiltInFallback()
    {
        CompositeRa2FieldValueCompletionCatalog catalog = new([
            new FieldRegistryRa2FieldValueCompletionCatalog(),
            new BuiltInRa2FieldValueCompletionCatalog()
        ]);
        Ra2FieldValueMetadata metadata = new(
            Ra2FieldValueKind.Enum,
            allowedValues: [new Ra2FieldAllowedValue("heavy", "Pack heavy", "From metadata.")]);

        Ra2FieldValueCompletionCandidate candidate = Assert.Single(catalog.GetCandidates(Request(
            "Armor",
            FieldEditorKind.Enum,
            new Ra2ValueCompletionContext("he", "he", false, []),
            metadata)));

        Assert.Equal("heavy", candidate.Value);
        Assert.Equal("Pack heavy", candidate.DisplayName);
        Assert.Equal(Ra2FieldValueCompletionSourceKind.FieldRegistry, candidate.SourceKind);
    }

    [Fact]
    public void CompositeCatalog_BuiltInKnownEnumCompletesValuesMissingFromLocalSchema()
    {
        CompositeRa2FieldValueCompletionCatalog catalog = new([
            new FieldRegistryRa2FieldValueCompletionCatalog(),
            new BuiltInRa2FieldValueCompletionCatalog()
        ]);
        Ra2FieldValueMetadata metadata = new(
            Ra2FieldValueKind.Enum,
            allowedValues:
            [
                new Ra2FieldAllowedValue("light", "Local light", "From current INI samples."),
                new Ra2FieldAllowedValue("special_2", "Local special", "From current INI samples.")
            ]);

        IReadOnlyList<Ra2FieldValueCompletionCandidate> candidates = catalog.GetCandidates(Request(
            "Armor",
            FieldEditorKind.Enum,
            new Ra2ValueCompletionContext(string.Empty, string.Empty, false, []),
            metadata));

        Assert.Contains(candidates, candidate => candidate.Value == "heavy" && candidate.SourceKind == Ra2FieldValueCompletionSourceKind.BuiltIn);
        Assert.Contains(candidates, candidate => candidate.Value == "concrete" && candidate.SourceKind == Ra2FieldValueCompletionSourceKind.BuiltIn);
        Ra2FieldValueCompletionCandidate localLight = Assert.Single(candidates, candidate => candidate.Value == "light");
        Assert.Equal("Local light", localLight.DisplayName);
        Assert.Equal(Ra2FieldValueCompletionSourceKind.FieldRegistry, localLight.SourceKind);
    }

    private static Ra2FieldValueCompletionRequest Request(
        string key,
        FieldEditorKind editorKind,
        Ra2ValueCompletionContext context,
        Ra2FieldValueMetadata? valueMetadata = null)
    {
        Ra2FieldDefinition definition = new(
            key,
            [Ra2SectionKind.Infantry],
            editorKind,
            valueMetadata is null ? Ra2FieldSourceKind.BuiltIn : Ra2FieldSourceKind.User,
            valueMetadata: valueMetadata);
        return new Ra2FieldValueCompletionRequest(Ra2SectionKind.Infantry, key, definition, context);
    }

    private sealed class FakeCatalog : IRa2FieldValueCompletionCatalog
    {
        private readonly IReadOnlyList<Ra2FieldValueCompletionCandidate> _candidates;

        public FakeCatalog(params Ra2FieldValueCompletionCandidate[] candidates)
        {
            _candidates = candidates;
        }

        public IReadOnlyList<Ra2FieldValueCompletionCandidate> GetCandidates(
            Ra2FieldValueCompletionRequest request)
            => _candidates;
    }
}
