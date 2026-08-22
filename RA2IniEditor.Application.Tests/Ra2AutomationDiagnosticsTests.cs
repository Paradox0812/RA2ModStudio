using RA2IniEditor.Application.Automation.Experimental;
using RA2IniEditor.Application.Diagnostics;
using RA2IniEditor.Application.Language;
using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using Xunit;

namespace RA2IniEditor.Application.Tests;

public sealed class Ra2AutomationDiagnosticsTests
{
    private const int MaximumDocumentCharacters = 8 * 1024 * 1024;

    [Fact]
    public void Validate_EmptyDocument_ReturnsSuccessAndPropagatesIdentity()
    {
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(string.Empty, version: 12);
        Ra2AutomationDocumentDiagnosticsResult result = new Ra2AutomationDocumentQueryService().Validate(snapshot);

        Assert.True(result.Succeeded);
        Assert.Equal(Ra2AutomationDocumentDiagnosticsFailureKind.None, result.FailureKind);
        Assert.Equal(snapshot.DocumentId, result.DocumentId);
        Assert.Equal(snapshot.Version, result.Version);
        Assert.Equal(snapshot.FilePath, result.FilePath);
        Assert.Equal(snapshot.FieldRegistry.Revision, result.FieldRegistryRevision);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void DiagnosticsResult_DefensivelyCopiesFactsAndRejectsFailurePayload()
    {
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(string.Empty);
        List<Ra2AutomationDiagnosticFact> source =
        [
            new("CODE", "Source", IniIssueSeverity.Warning, "message", snapshot.FilePath, 2, 3, "E1", "Key", snapshot.Version)
        ];

        Ra2AutomationDocumentDiagnosticsResult result = new(
            snapshot,
            Ra2AutomationDocumentDiagnosticsFailureKind.None,
            "ok",
            source);
        source.Clear();

        Ra2AutomationDiagnosticFact fact = Assert.Single(result.Diagnostics);
        Assert.Equal("CODE", fact.Code);
        Assert.Throws<ArgumentException>(() => new Ra2AutomationDocumentDiagnosticsResult(
            snapshot,
            Ra2AutomationDocumentDiagnosticsFailureKind.AnalysisFailed,
            "failed",
            [fact]));
    }

    [Fact]
    public void Validate_PreservesStructureFieldAndChainOrderAndFactProperties()
    {
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(
            """
            [InfantryTypes]
            0=E1
            [E1]
            Flag=maybe
            Primary=MissingWeapon
            [E1]
            """,
            CreateProvider(),
            filePath: "rulesmd.ini",
            version: 23);

        Ra2AutomationDocumentDiagnosticsResult result = new Ra2AutomationDocumentQueryService().Validate(snapshot);

        Assert.True(result.Succeeded);
        string[] codes = result.Diagnostics.Select(fact => fact.Code).ToArray();
        Assert.Equal("INI_STRUCTURE", codes[0]);
        Assert.Equal("FIELD_BOOLEAN_INVALID", codes[1]);
        Assert.Equal("CHAIN_WEAPON_MISSING", codes[2]);
        Assert.DoesNotContain("REF_MISSING_TARGET", codes);

        Ra2AutomationDiagnosticFact chain = result.Diagnostics[2];
        Assert.Equal("Chain", chain.SourceKind);
        Assert.Equal(IniIssueSeverity.Warning, chain.Severity);
        Assert.Equal(snapshot.FilePath, chain.FilePath);
        Assert.Equal(5, chain.LineNumber);
        Assert.NotNull(chain.ColumnNumber);
        Assert.Equal("E1", chain.SectionId);
        Assert.Equal("Primary", chain.Key);
        Assert.Equal(snapshot.Version, chain.AnalysisVersion);
    }

    [Fact]
    public void Validate_CoreStructureDiagnosticsPreserveDuplicateSectionAndKeyFacts()
    {
        Ra2AutomationDocumentDiagnosticsResult result = new Ra2AutomationDocumentQueryService().Validate(
            AutomationTestSupport.Snapshot("[E1]\nKey=1\nKey=2\n[E1]\n"));

        Assert.True(result.Succeeded);
        Assert.All(result.Diagnostics, fact => Assert.Equal("INI_STRUCTURE", fact.Code));
        Assert.Contains(result.Diagnostics, fact => fact.Message.Contains("重复", StringComparison.Ordinal));
        Assert.All(result.Diagnostics, fact => Assert.Equal("CoreParserValidator", fact.SourceKind));
    }

    [Fact]
    public void Validate_FieldAliasValueKindsAndInlineCommentsPreserveExistingSemantics()
    {
        IRa2FieldDefinitionProvider provider = new TestFieldDefinitionProvider(
        [
            Define("Known"),
            Define("Strength", new Ra2FieldValueMetadata(Ra2FieldValueKind.Integer), aliases: ["HitPoints"]),
            Define("Flag", new Ra2FieldValueMetadata(Ra2FieldValueKind.Boolean)),
            Define("Mode", new Ra2FieldValueMetadata(
                Ra2FieldValueKind.Enum,
                allowedValues: [new Ra2FieldAllowedValue("A"), new Ra2FieldAllowedValue("B")])),
            Define("Abilities", new Ra2FieldValueMetadata(
                Ra2FieldValueKind.EnumList,
                allowedValues: [new Ra2FieldAllowedValue("A"), new Ra2FieldAllowedValue("B")])),
            Define("Count", new Ra2FieldValueMetadata(Ra2FieldValueKind.Integer)),
            Define("Ratio", new Ra2FieldValueMetadata(Ra2FieldValueKind.Float))
        ]);
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(
            """
            [InfantryTypes]
            0=E1
            [E1]
            Known=x
            HitPoints=100
            Flag=yes; valid inline comment
            Mode=C
            Abilities=A,C
            Count=1.5
            Ratio=abc
            UnknownField=x
            """,
            provider);

        Ra2AutomationDocumentDiagnosticsResult result = new Ra2AutomationDocumentQueryService().Validate(snapshot);

        Assert.True(result.Succeeded);
        Assert.Equal(
            ["FIELD_ENUM_INVALID", "FIELD_ENUMLIST_INVALID", "FIELD_NUMBER_INVALID", "FIELD_NUMBER_INVALID", "FIELD_UNKNOWN_KEY"],
            result.Diagnostics.Select(fact => fact.Code));
        Assert.DoesNotContain(result.Diagnostics, fact => fact.Key is "HitPoints" or "Flag");
    }

    [Fact]
    public void Validate_FieldTrustLevelsPreserveGuardrailsAndLeaveInferredQuiet()
    {
        IRa2FieldDefinitionProvider provider = new TestFieldDefinitionProvider(
        [
            Define("Guard", quality: "source-verified-wrong-context"),
            Define("Old", quality: "obsolete"),
            Define("Missing", quality: "non-existent"),
            Define("Pseudo", quality: "pseudo-field"),
            Define("Inferred", quality: "name-inferred")
        ]);
        Ra2AutomationDocumentDiagnosticsResult result = new Ra2AutomationDocumentQueryService().Validate(
            AutomationTestSupport.Snapshot(
                "[InfantryTypes]\n0=E1\n[E1]\nGuard=x\nOld=x\nMissing=x\nPseudo=x\nInferred=x\n",
                provider));

        Assert.Equal(
            ["FIELD_WRONG_CONTEXT", "FIELD_OBSOLETE_KEY", "FIELD_NON_EXISTENT_KEY", "FIELD_PSEUDO_FIELD"],
            result.Diagnostics.Select(fact => fact.Code));
        Assert.DoesNotContain(result.Diagnostics, fact => fact.Key == "Inferred");
    }

    [Fact]
    public void HeadlessReferenceRulePreservesMissingNeutralAllowedAndCaseInsensitiveCatalogBehavior()
    {
        Ra2FieldDefinition primary = new(
            "Primary",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Reference,
            Ra2FieldSourceKind.User,
            valueMetadata: new Ra2FieldValueMetadata(
                Ra2FieldValueKind.Enum,
                allowedValues: [new Ra2FieldAllowedValue("AllowedExternal")]));
        Ra2FieldDefinition weapon2 = new(
            "Weapon2",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Reference,
            Ra2FieldSourceKind.User,
            valueMetadata: new Ra2FieldValueMetadata(
                Ra2FieldValueKind.Enum,
                allowedValues: [new Ra2FieldAllowedValue("AllowedExternal")]));
        IRa2FieldDefinitionProvider provider = new TestFieldDefinitionProvider([primary, weapon2]);
        Ra2DocumentSnapshot snapshot = new(
            "rulesmd.ini",
            "[InfantryTypes]\n0=E1\n[E1]\nPrimary=MissingWeapon\nElitePrimary=none\nWeapon2=AllowedExternal\n[GoodWeapon]\n",
            8);
        Ra2DocumentSemanticModel model = new Ra2DocumentSemanticModelBuilder().Build(snapshot, provider);
        Ra2ReferenceDiagnosticCatalog catalog = new Ra2ReferenceDiagnosticCatalogBuilder()
            .BuildFromCurrentDocument(snapshot.FilePath!, model);

        IReadOnlyList<Ra2DiagnosticFact> missing = new Ra2ReferenceDiagnosticService()
            .AnalyzeCurrentDocument(snapshot, model, provider, catalog);

        Ra2DiagnosticFact fact = Assert.Single(missing);
        Assert.Equal("REF_MISSING_TARGET", fact.Code);
        Assert.Equal("MissingWeapon", model.References.Single(reference => reference.SourceKey == "Primary").TargetSectionName);
        Assert.True(catalog.ContainsSection("goodweapon"));
        Assert.DoesNotContain(missing, issue => issue.Message.Contains("none", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(missing, issue => issue.Message.Contains("AllowedExternal", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Validate_ProjectileAndWarheadChainsPreserveSourceOrder()
    {
        IRa2FieldDefinitionProvider provider = new TestFieldDefinitionProvider(
        [
            new Ra2FieldDefinition("Projectile", [Ra2SectionKind.Weapon], FieldEditorKind.Reference, Ra2FieldSourceKind.User),
            new Ra2FieldDefinition("Warhead", [Ra2SectionKind.Weapon], FieldEditorKind.Reference, Ra2FieldSourceKind.User)
        ]);
        Ra2AutomationDocumentDiagnosticsResult result = new Ra2AutomationDocumentQueryService().Validate(
            AutomationTestSupport.Snapshot(
                "[WeaponTypes]\n0=Gun\n[Gun]\nProjectile=MissingProjectile\nWarhead=MissingWarhead\n",
                provider));

        Assert.Equal(
            ["CHAIN_PROJECTILE_MISSING", "CHAIN_WARHEAD_MISSING"],
            result.Diagnostics.Select(fact => fact.Code));
    }

    [Fact]
    public void Validate_ReferenceCatalogUsesOnlyCurrentDocument()
    {
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(
            """
            [InfantryTypes]
            0=E1
            [E1]
            Primary=OtherDocumentWeapon
            """,
            CreateProvider());

        Ra2AutomationDocumentDiagnosticsResult result = new Ra2AutomationDocumentQueryService().Validate(snapshot);

        Ra2AutomationDiagnosticFact fact = Assert.Single(result.Diagnostics);
        Assert.Equal("CHAIN_WEAPON_MISSING", fact.Code);
        Assert.Contains("当前文件", fact.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_CharacterLimit_IsInclusiveAndFailureHasNoPartialFacts()
    {
        Ra2AutomationDocumentQueryService service = new();
        Ra2AutomationDocumentSnapshot atLimit = AutomationTestSupport.Snapshot(new string(';', MaximumDocumentCharacters));
        Ra2AutomationDocumentSnapshot overLimit = AutomationTestSupport.Snapshot(new string(';', MaximumDocumentCharacters + 1));

        Assert.True(service.Validate(atLimit).Succeeded);

        Ra2AutomationDocumentDiagnosticsResult result = service.Validate(overLimit);
        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AutomationDocumentDiagnosticsFailureKind.DocumentTooLarge, result.FailureKind);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Validate_ResultLimit_ReturnsTypedFailureWithoutPartialFacts()
    {
        string text = "[InfantryTypes]\n0=E1\n[E1]\n" + string.Concat(
            Enumerable.Repeat("Flag=maybe\n", Ra2AutomationDocumentQueryService.MaximumResultItems + 1));

        Ra2AutomationDocumentDiagnosticsResult result = new Ra2AutomationDocumentQueryService().Validate(
            AutomationTestSupport.Snapshot(text, CreateProvider()));

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AutomationDocumentDiagnosticsFailureKind.ResultLimitExceeded, result.FailureKind);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Validate_PreCanceledToken_ReturnsCanceledWithoutPartialFacts()
    {
        using CancellationTokenSource source = new();
        source.Cancel();

        Ra2AutomationDocumentDiagnosticsResult result = new Ra2AutomationDocumentQueryService().Validate(
            AutomationTestSupport.Snapshot("[Broken\n"),
            source.Token);

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AutomationDocumentDiagnosticsFailureKind.Canceled, result.FailureKind);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Validate_MidAnalysisCancellation_ReturnsCanceledWithoutPartialFacts()
    {
        using CancellationTokenSource source = new();
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(
            "[InfantryTypes]\n0=E1\n[E1]\nFlag=maybe\n",
            new AutomationTestSupport.CancelingFieldDefinitionProvider(source));

        Ra2AutomationDocumentDiagnosticsResult result = new Ra2AutomationDocumentQueryService().Validate(snapshot, source.Token);

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AutomationDocumentDiagnosticsFailureKind.Canceled, result.FailureKind);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Validate_NonFatalProviderException_ReturnsSafeAnalysisFailure()
    {
        InvalidOperationException exception = new("provider-secret");
        Ra2AutomationDocumentDiagnosticsResult result = new Ra2AutomationDocumentQueryService().Validate(
            AutomationTestSupport.Snapshot(
                "[InfantryTypes]\n0=E1\n[E1]\nFlag=maybe\n",
                new AutomationTestSupport.ThrowingFieldDefinitionProvider(exception)));

        Assert.False(result.Succeeded);
        Assert.Equal(Ra2AutomationDocumentDiagnosticsFailureKind.AnalysisFailed, result.FailureKind);
        Assert.DoesNotContain("provider-secret", result.Message, StringComparison.Ordinal);
        Assert.Empty(result.Diagnostics);
    }

    [Fact]
    public void Validate_FatalProviderException_IsRethrown()
    {
        OutOfMemoryException exception = new("fatal");
        Assert.Same(exception, Assert.Throws<OutOfMemoryException>(() =>
            new Ra2AutomationDocumentQueryService().Validate(
                AutomationTestSupport.Snapshot(
                    "[InfantryTypes]\n0=E1\n[E1]\nFlag=maybe\n",
                    new AutomationTestSupport.ThrowingFieldDefinitionProvider(exception)))));
    }

    [Fact]
    public void Validate_RepeatedConcurrentInvocationsAreStateless()
    {
        Ra2AutomationDocumentQueryService service = new();
        Ra2AutomationDocumentSnapshot snapshot = AutomationTestSupport.Snapshot(
            "[InfantryTypes]\n0=E1\n[E1]\nFlag=maybe\n",
            CreateProvider(),
            version: 31);

        Ra2AutomationDocumentDiagnosticsResult[] results = Enumerable.Range(0, 16)
            .AsParallel()
            .Select(_ => service.Validate(snapshot))
            .ToArray();

        Assert.All(results, result =>
        {
            Assert.True(result.Succeeded);
            Assert.Equal("FIELD_BOOLEAN_INVALID", Assert.Single(result.Diagnostics).Code);
            Assert.Equal(31, result.Version);
        });
    }

    private static IRa2FieldDefinitionProvider CreateProvider()
        => new TestFieldDefinitionProvider(
        [
            new Ra2FieldDefinition(
                "Flag",
                [Ra2SectionKind.Infantry],
                FieldEditorKind.Text,
                Ra2FieldSourceKind.User,
                valueMetadata: new Ra2FieldValueMetadata(Ra2FieldValueKind.Boolean)),
            new Ra2FieldDefinition(
                "Primary",
                [Ra2SectionKind.Infantry],
                FieldEditorKind.Reference,
                Ra2FieldSourceKind.User),
        ]);

    private static Ra2FieldDefinition Define(
        string key,
        Ra2FieldValueMetadata? metadata = null,
        IReadOnlyCollection<string>? aliases = null,
        string? quality = null)
        => new(
            key,
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Text,
            Ra2FieldSourceKind.User,
            valueMetadata: metadata,
            aliases: aliases,
            registryQuality: quality);

    private sealed class TestFieldDefinitionProvider : IRa2FieldDefinitionProvider
    {
        private readonly IReadOnlyList<Ra2FieldDefinition> _definitions;

        public TestFieldDefinitionProvider(IReadOnlyList<Ra2FieldDefinition> definitions)
        {
            _definitions = definitions;
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = _definitions.FirstOrDefault(candidate =>
                candidate.AppliesTo.Contains(sectionKind) &&
                string.Equals(candidate.Key, key, StringComparison.OrdinalIgnoreCase))!;
            return definition is not null;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => _definitions.Where(definition => definition.AppliesTo.Contains(sectionKind)).ToArray();

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => TryGetField(sectionKind, key, out _);
    }
}
