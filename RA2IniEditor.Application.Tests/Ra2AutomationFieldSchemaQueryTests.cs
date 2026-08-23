using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Core.Schema;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationFieldSchemaQueryTests
{
    [Theory]
    [InlineData("source-verified", Ra2AutomationFieldTrustLevel.Verified, Ra2AutomationFieldAuthoringDisposition.Normal)]
    [InlineData("guardrail", Ra2AutomationFieldTrustLevel.VerifiedGuardrail, Ra2AutomationFieldAuthoringDisposition.Blocked)]
    [InlineData("inferred", Ra2AutomationFieldTrustLevel.Inferred, Ra2AutomationFieldAuthoringDisposition.Caution)]
    [InlineData("manual-curated", Ra2AutomationFieldTrustLevel.ManualCurated, Ra2AutomationFieldAuthoringDisposition.Normal)]
    [InlineData("auto-extracted", Ra2AutomationFieldTrustLevel.AutoExtracted, Ra2AutomationFieldAuthoringDisposition.Caution)]
    [InlineData("obsolete", Ra2AutomationFieldTrustLevel.Obsolete, Ra2AutomationFieldAuthoringDisposition.Blocked)]
    [InlineData("non-existent", Ra2AutomationFieldTrustLevel.NonExistent, Ra2AutomationFieldAuthoringDisposition.Blocked)]
    [InlineData("pseudo", Ra2AutomationFieldTrustLevel.PseudoField, Ra2AutomationFieldAuthoringDisposition.Blocked)]
    [InlineData("unclassified-quality", Ra2AutomationFieldTrustLevel.Unknown, Ra2AutomationFieldAuthoringDisposition.Caution)]
    public void GetFieldSchema_MapsTrustToAuthoringDisposition(
        string quality,
        Ra2AutomationFieldTrustLevel trust,
        Ra2AutomationFieldAuthoringDisposition disposition)
    {
        StaticProvider provider = new(CreateDefinition(quality));

        Ra2AutomationFieldSchemaQueryResult result = new Ra2AutomationDocumentQueryService().GetFieldSchema(
            AutomationTestSupport.Snapshot(string.Empty, provider, version: 17),
            new Ra2AutomationFieldSchemaQuery(Ra2SectionKind.Weapon, "Verses"));

        Assert.True(result.Succeeded, result.Message);
        Assert.NotNull(result.Fact);
        Assert.Equal(trust, result.Fact!.TrustLevel);
        Assert.Equal(disposition, result.Fact.AuthoringDisposition);
        Assert.Equal(17, result.Version);
        Assert.Equal(7, result.FieldRegistryRevision);
    }

    [Fact]
    public void GetFieldSchema_ProjectsCompleteImmutableSchemaInStableOrder()
    {
        Ra2FieldDefinition definition = CreateDefinition("source-verified");
        Ra2AutomationFieldSchemaQueryResult result = new Ra2AutomationDocumentQueryService().GetFieldSchema(
            AutomationTestSupport.Snapshot("[W]\n", new StaticProvider(definition)),
            new Ra2AutomationFieldSchemaQuery(Ra2SectionKind.Weapon, "Verses"));

        Ra2AutomationFieldSchemaFact fact = Assert.IsType<Ra2AutomationFieldSchemaFact>(result.Fact);
        Assert.Equal("Verses", fact.Key);
        Assert.Equal(Ra2SectionKind.Weapon, fact.SectionKind);
        Assert.Equal([Ra2SectionKind.Weapon, Ra2SectionKind.Warhead], fact.AppliesTo);
        Assert.Equal(FieldEditorKind.Enum, fact.EditorKind);
        Assert.Equal(Ra2FieldValueKind.EnumList, fact.ValueKind);
        Assert.Equal(Ra2FieldBooleanValueStyle.Custom, fact.BooleanStyle);
        Assert.Equal(["light", "heavy"], fact.AllowedValues);
        Assert.Equal("ArmorTypes", fact.EnumName);
        Assert.Equal(",", fact.Separator);
        Assert.Equal("Verses display", fact.DisplayName);
        Assert.Equal("Verses description", fact.Description);
        Assert.Equal(["ArmorVerses", "DamageVerses"], fact.Aliases);
        Assert.Equal(Ra2FieldSourceKind.Phobos, fact.SourceKind);
        Assert.Throws<NotSupportedException>(() => ((IList<string>)fact.AllowedValues).Add("wood"));
        Assert.Throws<NotSupportedException>(() => ((IList<string>)fact.Aliases).Clear());
        Assert.Throws<NotSupportedException>(() => ((IList<Ra2SectionKind>)fact.AppliesTo).Clear());
    }

    [Fact]
    public void GetFieldSchema_NotFoundCanceledAndTooLargeHaveNoFact()
    {
        Ra2AutomationDocumentQueryService service = new();
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(string.Empty);

        Ra2AutomationFieldSchemaQueryResult missing = service.GetFieldSchema(
            snapshot,
            new Ra2AutomationFieldSchemaQuery(Ra2SectionKind.Techno, "Missing"));
        using CancellationTokenSource cancellation = new();
        cancellation.Cancel();
        Ra2AutomationFieldSchemaQueryResult canceled = service.GetFieldSchema(
            AutomationTestSupport.Snapshot(string.Empty, new StaticProvider(CreateDefinition("source-verified"))),
            new Ra2AutomationFieldSchemaQuery(Ra2SectionKind.Weapon, "Verses"),
            cancellation.Token);
        Ra2AutomationFieldSchemaQueryResult tooLarge = service.GetFieldSchema(
            AutomationTestSupport.Snapshot(new string(';', Ra2AutomationDocumentQueryService.MaximumDocumentCharacters + 1)),
            new Ra2AutomationFieldSchemaQuery(Ra2SectionKind.Techno, "Strength"));

        Assert.Equal(Ra2AutomationFieldSchemaQueryFailureKind.NotFound, missing.FailureKind);
        Assert.Equal(Ra2AutomationFieldSchemaQueryFailureKind.Canceled, canceled.FailureKind);
        Assert.Equal(Ra2AutomationFieldSchemaQueryFailureKind.DocumentTooLarge, tooLarge.FailureKind);
        Assert.Null(missing.Fact);
        Assert.Null(canceled.Fact);
        Assert.Null(tooLarge.Fact);
    }

    [Fact]
    public async Task GetFieldSchema_IsDeterministicAndSafeForParallelQueries()
    {
        Ra2AutomationDocumentQueryService service = new();
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(
            string.Empty,
            new StaticProvider(CreateDefinition("source-verified")));
        Ra2AutomationFieldSchemaQuery query = new(Ra2SectionKind.Weapon, "Verses");

        Ra2AutomationFieldSchemaQueryResult[] results = await Task.WhenAll(
            Enumerable.Range(0, 16).Select(_ => Task.Run(() => service.GetFieldSchema(snapshot, query))));

        Assert.All(results, result =>
        {
            Assert.True(result.Succeeded);
            Assert.Equal(["light", "heavy"], result.Fact!.AllowedValues);
            Assert.Equal(["ArmorVerses", "DamageVerses"], result.Fact.Aliases);
        });
    }

    private static Ra2FieldDefinition CreateDefinition(string quality)
        => new(
            "Verses",
            [Ra2SectionKind.Weapon, Ra2SectionKind.Warhead],
            FieldEditorKind.Enum,
            Ra2FieldSourceKind.Phobos,
            "Verses description",
            new Ra2FieldValueMetadata(
                Ra2FieldValueKind.EnumList,
                Ra2FieldBooleanValueStyle.Custom,
                [new Ra2FieldAllowedValue("light"), new Ra2FieldAllowedValue("heavy")],
                "ArmorTypes",
                ","),
            "Verses display",
            ["ArmorVerses", "DamageVerses"],
            quality);

    private sealed class StaticProvider : IRa2FieldDefinitionProvider
    {
        private readonly Ra2FieldDefinition _definition;

        public StaticProvider(Ra2FieldDefinition definition) => _definition = definition;

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            if (string.Equals(key, _definition.Key, StringComparison.OrdinalIgnoreCase))
            {
                definition = _definition;
                return true;
            }

            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind) => [_definition];

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => string.Equals(key, _definition.Key, StringComparison.OrdinalIgnoreCase);
    }
}
