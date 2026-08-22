using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.Language.FieldQuickPeek;
using RA2IniEditor.Infrastructure.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.BuiltIn;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FieldQuickPeekServiceTests
{
    [Fact]
    public void Resolve_OnKeyTokenReturnsSharedDetailsWithExamples()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            Armor=heavy
            """;
        Ra2FieldDefinition definition = CreateDefinition();
        Ra2DocumentSemanticModel model = Build(text, new TestFieldProvider(definition));
        int offset = text.IndexOf("Armor", StringComparison.Ordinal) + 1;
        Ra2FieldQuickPeekService service = new();

        Ra2FieldQuickPeekResult result = service.Resolve(new Ra2FieldQuickPeekRequest(
            model,
            offset,
            new TestFieldProvider(definition),
            new TestProvenanceProvider(definition)));

        Assert.Equal(Ra2FieldQuickPeekStatus.Available, result.Status);
        Assert.Equal("Armor", result.Key);
        Assert.True(result.Details.HasExamples);
        Assert.Equal("Armor", result.Details.Key);
        Assert.Equal("heavy", Assert.Single(result.Details.Examples).Value);
    }

    [Fact]
    public void Resolve_OnValueTokenStillResolvesKeyValueLineField()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            Armor=heavy
            """;
        Ra2FieldDefinition definition = CreateDefinition();
        Ra2DocumentSemanticModel model = Build(text, new TestFieldProvider(definition));
        int offset = text.IndexOf("heavy", StringComparison.Ordinal) + 1;
        Ra2FieldQuickPeekService service = new();

        Ra2FieldQuickPeekResult result = service.Resolve(new Ra2FieldQuickPeekRequest(
            model,
            offset,
            new TestFieldProvider(definition),
            new TestProvenanceProvider(definition)));

        Assert.Equal(Ra2FieldQuickPeekStatus.Available, result.Status);
        Assert.Equal("Armor", result.Key);
        Assert.Equal("heavy", Assert.Single(result.Details.Examples).Value);
    }

    [Fact]
    public void Resolve_OnSectionHeaderDoesNotReturnFieldDetails()
    {
        const string text = """
            [E1]
            Armor=heavy
            """;
        Ra2DocumentSemanticModel model = Build(text);
        int offset = text.IndexOf("[E1]", StringComparison.Ordinal) + 1;
        Ra2FieldQuickPeekService service = new();

        Assert.False(service.CanResolveKeyValueLine(model, offset));
        Ra2FieldQuickPeekResult result = service.Resolve(new Ra2FieldQuickPeekRequest(
            model,
            offset,
            new TestFieldProvider(),
            new TestProvenanceProvider()));

        Assert.Equal(Ra2FieldQuickPeekStatus.NotKeyValueLine, result.Status);
    }

    [Fact]
    public void Resolve_WhenFieldIsUnknownReturnsNotFoundDetails()
    {
        const string text = """
            [E1]
            UnknownKey=1
            """;
        Ra2DocumentSemanticModel model = Build(text);
        int offset = text.IndexOf("UnknownKey", StringComparison.Ordinal) + 1;
        Ra2FieldQuickPeekService service = new();

        Ra2FieldQuickPeekResult result = service.Resolve(new Ra2FieldQuickPeekRequest(
            model,
            offset,
            new TestFieldProvider(),
            new TestProvenanceProvider()));

        Assert.Equal(Ra2FieldQuickPeekStatus.NotFound, result.Status);
        Assert.Equal("UnknownKey", result.Key);
        Assert.True(result.Details.IsNotFound);
    }

    [Fact]
    public void Resolve_UsesSpecificBuiltInFieldWhenProjectFallbackFieldExists()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            Armor=heavy
            """;
        IRa2FieldDefinitionProvider fieldProvider = new CompositeRa2FieldDefinitionProvider([
            new TestFieldProvider(new Ra2FieldDefinition("Armor", [Ra2SectionKind.Unknown], FieldEditorKind.Text, Ra2FieldSourceKind.User)),
            new BuiltInRa2FieldDefinitionProvider()
        ]);
        Ra2DocumentSemanticModel model = Build(text, fieldProvider);
        int offset = text.IndexOf("Armor", StringComparison.Ordinal) + 1;
        Ra2FieldQuickPeekService service = new();

        Ra2FieldQuickPeekResult result = service.Resolve(new Ra2FieldQuickPeekRequest(
            model,
            offset,
            fieldProvider,
            new TestProvenanceProvider()));

        Assert.Equal(Ra2FieldQuickPeekStatus.Available, result.Status);
        Assert.Equal("Enum", result.Details.EditorKindDisplay);
        Assert.Equal("内置参考", result.Details.SourceDisplay);
    }

    [Fact]
    public void Resolve_UsesBuiltInDetailsWhenExactProjectFieldIsWeakAndBuiltInIsAbstractYuriField()
    {
        const string text = """
            [VehicleTypes]
            0=MTNK

            [MTNK]
            Primary=120mm
            """;
        IRa2FieldDefinitionProvider fieldProvider = new CompositeRa2FieldDefinitionProvider([
            new TestFieldProvider(new Ra2FieldDefinition("Primary", [Ra2SectionKind.Vehicle], FieldEditorKind.Text, Ra2FieldSourceKind.User)),
            new TestFieldProvider(new Ra2FieldDefinition(
                "Primary",
                [Ra2SectionKind.Techno],
                FieldEditorKind.Reference,
                Ra2FieldSourceKind.Yuri,
                "Primary weapon reference.",
                new Ra2FieldValueMetadata(Ra2FieldValueKind.Reference),
                displayName: null,
                aliases: null,
                examples: [new Ra2FieldExample("120mm", "Cannon weapon")]))
        ]);
        Ra2DocumentSemanticModel model = Build(text, fieldProvider);
        int offset = text.IndexOf("Primary", StringComparison.Ordinal) + 1;
        Ra2FieldQuickPeekService service = new();

        Ra2FieldQuickPeekResult result = service.Resolve(new Ra2FieldQuickPeekRequest(
            model,
            offset,
            fieldProvider,
            new TestProvenanceProvider()));

        Assert.Equal(Ra2FieldQuickPeekStatus.Available, result.Status);
        Assert.Equal("Reference", result.Details.EditorKindDisplay);
        Assert.Contains("Primary weapon reference.", result.Details.Description);
        Assert.Equal("120mm", Assert.Single(result.Details.Examples).Value);
    }

    [Fact]
    public void Resolve_ShowsV3DescriptionAndExamplesForProjectileAA()
    {
        const string text = """
            [ProjectileTypes]
            0=Cannon

            [Cannon]
            AA=no
            """;
        IRa2FieldDefinitionProvider fieldProvider = CreateV3Provider();
        Ra2DocumentSemanticModel model = Build(text, fieldProvider);
        int offset = text.IndexOf("AA", StringComparison.Ordinal) + 1;
        Ra2FieldQuickPeekService service = new();

        Ra2FieldQuickPeekResult result = service.Resolve(new Ra2FieldQuickPeekRequest(
            model,
            offset,
            fieldProvider,
            new TestProvenanceProvider()));

        Assert.Equal(Ra2FieldQuickPeekStatus.Available, result.Status);
        Assert.Equal("AA", result.Key);
        Assert.Equal("Boolean", result.Details.EditorKindDisplay);
        Assert.Contains("攻击空中目标", result.Details.Description);
        Assert.Contains(result.Details.Examples, example => example.Value == "yes");
        Assert.Contains(result.Details.Examples, example => example.Value == "no");
    }

    [Fact]
    public void Resolve_RetainsBuiltInDiagnosticGuardrailForVehicleAA()
    {
        const string text = """
            [VehicleTypes]
            0=MTNK

            [MTNK]
            AA=yes
            """;
        IRa2FieldDefinitionProvider fieldProvider = CreateV3Provider();
        Ra2DocumentSemanticModel model = Build(text, fieldProvider);
        int offset = text.IndexOf("AA", StringComparison.Ordinal) + 1;

        Ra2FieldQuickPeekResult result = new Ra2FieldQuickPeekService().Resolve(new Ra2FieldQuickPeekRequest(
            model,
            offset,
            fieldProvider,
            new TestProvenanceProvider()));

        Assert.Equal(Ra2FieldQuickPeekStatus.Available, result.Status);
        Assert.Equal("上下文保护", result.Details.TrustDisplay);
        Assert.True(result.Details.HasTrustDetail);
        Assert.Contains("不是 Techno 字段", result.Details.Description);
    }

    private static Ra2FieldDefinition CreateDefinition()
        => new(
            "Armor",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Enum,
            Ra2FieldSourceKind.Yuri,
            "Armor type.",
            valueMetadata: null,
            displayName: "装甲类型",
            aliases: null,
            examples: [new Ra2FieldExample("heavy", "重甲")]);

    private static Ra2DocumentSemanticModel Build(string text, IRa2FieldDefinitionProvider? provider = null)
        => new Ra2DocumentSemanticModelBuilder().Build(
            new Ra2DocumentSnapshot("rulesmd.ini", text, 1),
            provider ?? new TestFieldProvider());

    private static IRa2FieldDefinitionProvider CreateV3Provider()
        => new LocalRa2FieldDefinitionProvider(new BuiltInFieldRegistryPackLoader().Load().Definitions);

    private sealed class TestFieldProvider : IRa2FieldDefinitionProvider
    {
        private readonly Ra2FieldDefinition? _definition;

        public TestFieldProvider(Ra2FieldDefinition? definition = null)
        {
            _definition = definition;
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            if (_definition is not null && string.Equals(_definition.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                definition = _definition;
                return true;
            }

            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => _definition is null ? [] : [_definition];

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => _definition is not null && string.Equals(_definition.Key, key, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class TestProvenanceProvider : IFieldRegistryProvenanceProvider
    {
        private readonly Ra2FieldDefinition? _definition;

        public TestProvenanceProvider(Ra2FieldDefinition? definition = null)
        {
            _definition = definition;
        }

        public FieldRegistryProvenanceLookupResult TryGetFieldWithProvenance(Ra2SectionKind sectionKind, string key)
        {
            if (_definition is null || !string.Equals(_definition.Key, key, StringComparison.OrdinalIgnoreCase))
                return FieldRegistryProvenanceLookupResult.NotFound;

            return FieldRegistryProvenanceLookupResult.FromEntry(new FieldRegistryProvenanceEntry(
                key,
                sectionKind,
                FieldRegistryProvenanceScope.BuiltIn,
                "YR 内置参考",
                "builtin-yr-ares-phobos-fallback-v3.2.fields.json",
                _definition));
        }
    }
}
