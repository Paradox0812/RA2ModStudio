using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.Infrastructure.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.BuiltIn;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2HoverProviderTests
{
    [Fact]
    public void GetHover_OnKnownKeyReturnsFieldInfo()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Strength=300
            """;
        Ra2FieldDefinition definition = new(
            "Strength",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Integer,
            Ra2FieldSourceKind.BuiltIn,
            "Hit points");
        TestFieldProvider fieldProvider = new(definition);
        Ra2DocumentSemanticModel model = Build(text, fieldProvider);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("Strength", StringComparison.Ordinal) + 1);

        Ra2HoverInfo? hover = new Ra2HoverProvider().GetHover(
            model,
            context,
            fieldProvider,
            new TestProvenanceProvider(definition));

        Assert.NotNull(hover);
        Assert.Equal("Strength", hover.Title);
        Assert.Equal("Field", hover.Kind);
        Assert.Contains("Integer", hover.Detail);
        Assert.Contains("Infantry", hover.Detail);
        Assert.Equal("Hit points", hover.Description);
        Assert.Equal("Project", hover.Source);
    }

    [Fact]
    public void GetHover_OnKnownKeyIncludesOnlyFirstShortExample()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Armor=heavy
            """;
        Ra2FieldDefinition definition = new(
            "Armor",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Enum,
            Ra2FieldSourceKind.Yuri,
            "Armor type.",
            valueMetadata: null,
            displayName: "装甲类型",
            aliases: null,
            examples:
            [
                new Ra2FieldExample("heavy", "重甲"),
                new Ra2FieldExample("light", "轻甲")
            ]);
        TestFieldProvider fieldProvider = new(definition);
        Ra2DocumentSemanticModel model = Build(text, fieldProvider);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("Armor", StringComparison.Ordinal) + 1);

        Ra2HoverInfo? hover = new Ra2HoverProvider().GetHover(
            model,
            context,
            fieldProvider,
            new TestProvenanceProvider(definition));

        Assert.NotNull(hover);
        Assert.Contains("Armor type.", hover.Description);
        Assert.Contains("示例：heavy - 重甲", hover.Description);
        Assert.DoesNotContain("light", hover.Description);
    }

    [Fact]
    public void GetHover_ResolvesSpecificBuiltInFieldWhenProjectFallbackFieldExists()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Armor=heavy
            """;
        IRa2FieldDefinitionProvider fieldProvider = new CompositeRa2FieldDefinitionProvider([
            new TestFieldProvider(new Ra2FieldDefinition("Armor", [Ra2SectionKind.Unknown], FieldEditorKind.Text, Ra2FieldSourceKind.User)),
            new BuiltInRa2FieldDefinitionProvider()
        ]);
        Ra2DocumentSemanticModel model = Build(text, fieldProvider);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("Armor", StringComparison.Ordinal) + 1);

        Ra2HoverInfo? hover = new Ra2HoverProvider().GetHover(
            model,
            context,
            fieldProvider,
            new TestProvenanceProvider());

        Assert.NotNull(hover);
        Assert.Contains("Type: Enum", hover.Detail);
        Assert.DoesNotContain("Type: Text", hover.Detail);
    }

    [Fact]
    public void GetHover_UsesBuiltInDetailsWhenExactProjectFieldIsWeakAndBuiltInIsAbstractYuriField()
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
        Ra2CaretContext context = ContextAt(model, text.IndexOf("Primary", StringComparison.Ordinal) + 1);

        Ra2HoverInfo? hover = new Ra2HoverProvider().GetHover(
            model,
            context,
            fieldProvider,
            new TestProvenanceProvider());

        Assert.NotNull(hover);
        Assert.Contains("Type: Reference", hover.Detail);
        Assert.DoesNotContain("Type: Text", hover.Detail);
        Assert.Contains("Primary weapon reference.", hover.Description);
        Assert.Contains("120mm", hover.Description);
    }

    [Fact]
    public void GetHover_UsesV3DescriptionForProjectileAA()
    {
        const string text = """
            [ProjectileTypes]
            0=Cannon

            [Cannon]
            AA=no
            """;
        IRa2FieldDefinitionProvider fieldProvider = CreateV3Provider();
        Ra2DocumentSemanticModel model = Build(text, fieldProvider);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("AA", StringComparison.Ordinal) + 1);

        Ra2HoverInfo? hover = new Ra2HoverProvider().GetHover(
            model,
            context,
            fieldProvider,
            new TestProvenanceProvider());

        Assert.NotNull(hover);
        Assert.Contains("Type: Boolean", hover.Detail);
        Assert.Contains("攻击空中目标", hover.Description);
        Assert.Contains("yes", hover.Description);
    }

    [Fact]
    public void GetHover_RetainsBuiltInDiagnosticGuardrailForVehicleAA()
    {
        const string text = """
            [VehicleTypes]
            0=MTNK

            [MTNK]
            AA=yes
            """;
        IRa2FieldDefinitionProvider fieldProvider = CreateV3Provider();
        Ra2DocumentSemanticModel model = Build(text, fieldProvider);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("AA", StringComparison.Ordinal) + 1);

        Ra2HoverInfo? hover = new Ra2HoverProvider().GetHover(
            model,
            context,
            fieldProvider,
            new TestProvenanceProvider());

        Assert.NotNull(hover);
        Assert.Contains("不是 Techno 字段", hover.Description);
        Assert.Contains("诊断：疑似上下文错误或保护性字段", hover.Description);
    }

    [Fact]
    public void GetHover_OnVerifiedFieldDoesNotAddTrustFootnote()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Strength=300
            """;
        Ra2FieldDefinition definition = new(
            "Strength",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Integer,
            Ra2FieldSourceKind.BuiltIn,
            "Hit points",
            registryQuality: "source-verified-test");
        TestFieldProvider fieldProvider = new(definition);
        Ra2DocumentSemanticModel model = Build(text, fieldProvider);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("Strength", StringComparison.Ordinal) + 1);

        Ra2HoverInfo? hover = new Ra2HoverProvider().GetHover(
            model,
            context,
            fieldProvider,
            new TestProvenanceProvider(definition));

        Assert.NotNull(hover);
        Assert.Equal("Hit points", hover.Description);
    }

    [Fact]
    public void GetHover_OnInferredFieldAddsOnlyLightweightFootnote()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            LooseKey=yes
            """;
        Ra2FieldDefinition definition = new(
            "LooseKey",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Boolean,
            Ra2FieldSourceKind.BuiltIn,
            "推断型字段说明。",
            registryQuality: "name-inferred-test");
        TestFieldProvider fieldProvider = new(definition);
        Ra2DocumentSemanticModel model = Build(text, fieldProvider);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("LooseKey", StringComparison.Ordinal) + 1);

        Ra2HoverInfo? hover = new Ra2HoverProvider().GetHover(
            model,
            context,
            fieldProvider,
            new TestProvenanceProvider(definition));

        Assert.NotNull(hover);
        Assert.Contains("推断型字段说明。", hover.Description);
        Assert.Contains("可信度：推断说明，仅供参考", hover.Description);
        Assert.DoesNotContain("name-inferred-test", hover.Description);
    }

    [Fact]
    public void GetHover_OnValueReferenceReturnsTargetSectionInfo()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=120mm;Grizzly cannon

            [120mm];Cannon weapon
            Damage=90
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("120mm", StringComparison.Ordinal) + 1);

        Ra2HoverInfo? hover = new Ra2HoverProvider().GetHover(
            model,
            context,
            new TestFieldProvider(),
            new TestProvenanceProvider());

        Assert.NotNull(hover);
        Assert.Equal("120mm", hover.Title);
        Assert.Equal("Weapon", hover.Kind);
        Assert.Contains("Weapon reference target", hover.Detail);
        Assert.Equal("Cannon weapon", hover.DisplayName);
        Assert.Contains("\u5f15\u7528\u5907\u6ce8: Grizzly cannon", hover.Description);
        Assert.DoesNotContain("\u76ee\u6807\u5907\u6ce8", hover.Description);
        Assert.DoesNotContain("Damage=90", hover.Description);
        Assert.DoesNotContain("\u4f4d\u7f6e", hover.Description);
        Assert.Equal("Current document", hover.Source);
    }

    [Fact]
    public void GetHover_OnValueReferenceWithoutNotesOmitsEmptyDescription()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=120mm

            [120mm]
            Damage=90
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("120mm", StringComparison.Ordinal) + 1);

        Ra2HoverInfo? hover = new Ra2HoverProvider().GetHover(
            model,
            context,
            new TestFieldProvider(),
            new TestProvenanceProvider());

        Assert.NotNull(hover);
        Assert.Equal("120mm", hover.Title);
        Assert.Null(hover.DisplayName);
        Assert.Null(hover.Description);
        Assert.Equal("Current document", hover.Source);
    }

    [Fact]
    public void GetHover_OnSectionHeaderReturnsInlineComment()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=M60

            [M60];GIWeapon
            Damage=15
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("[M60]", StringComparison.Ordinal) + 1);

        Ra2HoverInfo? hover = new Ra2HoverProvider().GetHover(
            model,
            context,
            new TestFieldProvider(),
            new TestProvenanceProvider());

        Assert.NotNull(hover);
        Assert.Equal("[M60]", hover.Title);
        Assert.Contains("Weapon", hover.Kind);
        Assert.Equal("\u5907\u6ce8: GIWeapon", hover.Description);
    }

    [Fact]
    public void GetHover_OnSectionHeaderReturnsPrecedingCommentDisplayNote()
    {
        const string text = """
            [VehicleTypes]
            0=MTNK

            ; Medium Tank
            [MTNK]
            Primary=120mm
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("[MTNK]", StringComparison.Ordinal) + 1);

        Ra2HoverInfo? hover = new Ra2HoverProvider().GetHover(
            model,
            context,
            new TestFieldProvider(),
            new TestProvenanceProvider());

        Assert.NotNull(hover);
        Assert.Equal("[MTNK]", hover.Title);
        Assert.Contains("Vehicle", hover.Kind);
        Assert.Equal("\u5907\u6ce8: Medium Tank", hover.Description);
    }

    [Fact]
    public void GetHover_OnValueReferenceUsesTargetDisplayNoteFromPrecedingComment()
    {
        const string text = """
            [VehicleTypes]
            0=MTNK

            ; Medium Tank
            [MTNK]
            Primary=120mm

            [E1]
            Primary=MTNK;temporary test
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.LastIndexOf("MTNK", StringComparison.Ordinal) + 1);

        Ra2HoverInfo? hover = new Ra2HoverProvider().GetHover(
            model,
            context,
            new TestFieldProvider(),
            new TestProvenanceProvider());

        Assert.NotNull(hover);
        Assert.Equal("MTNK", hover.Title);
        Assert.Equal("Medium Tank", hover.DisplayName);
        Assert.Contains("\u5f15\u7528\u5907\u6ce8: temporary test", hover.Description);
        Assert.DoesNotContain("Primary=120mm", hover.Description);
    }

    [Fact]
    public void GetHover_OnWhitespaceReturnsNull()
    {
        const string text = "[NEWINF]\n   \nStrength=300";
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("   ", StringComparison.Ordinal) + 1);

        Ra2HoverInfo? hover = new Ra2HoverProvider().GetHover(
            model,
            context,
            new TestFieldProvider(),
            new TestProvenanceProvider());

        Assert.Null(hover);
    }

    private static Ra2DocumentSemanticModel Build(string text, IRa2FieldDefinitionProvider? provider = null)
        => new Ra2DocumentSemanticModelBuilder().Build(
            new Ra2DocumentSnapshot("rulesmd.ini", text, 1),
            provider ?? new TestFieldProvider());

    private static Ra2CaretContext ContextAt(Ra2DocumentSemanticModel model, int offset)
        => new Ra2CaretContextService().GetContext(model, offset);

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
                FieldRegistryProvenanceScope.Project,
                "Project",
                "project.fields.json",
                _definition));
        }
    }
}
