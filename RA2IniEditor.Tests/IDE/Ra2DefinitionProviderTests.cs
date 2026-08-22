using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2DefinitionProviderTests
{
    [Fact]
    public void GetDefinition_OnKnownKeyReturnsFieldDefinition()
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
        Ra2CaretContext context = ContextAt(model, text.IndexOf("Strength", StringComparison.Ordinal) + 2);

        Ra2DefinitionTarget? target = new Ra2DefinitionProvider().GetDefinition(
            model,
            context,
            fieldProvider,
            new TestProvenanceProvider(definition));

        Assert.NotNull(target);
        Assert.Equal(Ra2DefinitionTargetKind.FieldDefinition, target.Kind);
        Assert.Equal("Strength", target.Title);
        Assert.Equal("BuiltIn", target.SourceName);
        Assert.Contains("Integer", target.Detail);
        Assert.Equal("Hit points", target.Description);
    }

    [Fact]
    public void GetDefinition_OnValueReferenceReturnsTargetSection()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=120mm

            [120mm];Cannon weapon
            Damage=90
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("120mm", StringComparison.Ordinal) + 1);

        Ra2DefinitionTarget? target = new Ra2DefinitionProvider().GetDefinition(
            model,
            context,
            new TestFieldProvider(),
            new TestProvenanceProvider());

        Assert.NotNull(target);
        Assert.Equal(Ra2DefinitionTargetKind.SectionDefinition, target.Kind);
        Assert.Equal("[120mm]", target.Title);
        Assert.Equal(7, target.TargetLineNumber);
        Assert.Equal("[120mm]", text.Substring(target.TargetSpan!.Value.Start, target.TargetSpan.Value.Length));
        Assert.Equal("\u5907\u6ce8: Cannon weapon", target.Description);
    }

    [Fact]
    public void GetDefinition_OnValueReferenceReturnsTargetPrecedingComment()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=M60

            ; GI Weapon
            [M60]
            Damage=15
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("M60", StringComparison.Ordinal) + 1);

        Ra2DefinitionTarget? target = new Ra2DefinitionProvider().GetDefinition(
            model,
            context,
            new TestFieldProvider(),
            new TestProvenanceProvider());

        Assert.NotNull(target);
        Assert.Equal("[M60]", target.Title);
        Assert.Equal("\u5907\u6ce8: GI Weapon", target.Description);
    }

    [Fact]
    public void GetDefinition_OnInlineCommentReferenceReturnsTargetSection()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=120mm ; main weapon

            [120mm]
            Damage=90
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("120mm ;", StringComparison.Ordinal) + 1);

        Ra2DefinitionTarget? target = new Ra2DefinitionProvider().GetDefinition(
            model,
            context,
            new TestFieldProvider(),
            new TestProvenanceProvider());

        Assert.NotNull(target);
        Assert.Equal(Ra2DefinitionTargetKind.SectionDefinition, target.Kind);
        Assert.Equal("[120mm]", target.Title);
    }

    [Fact]
    public void GetDefinition_OnSecondCommaValueTokenReturnsNull()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=120mm,SomethingElse

            [120mm]
            Damage=90
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("SomethingElse", StringComparison.Ordinal) + 1);

        Ra2DefinitionTarget? target = new Ra2DefinitionProvider().GetDefinition(
            model,
            context,
            new TestFieldProvider(),
            new TestProvenanceProvider());

        Assert.Null(target);
    }

    [Fact]
    public void GetDefinition_DuplicateSectionPreviewStageJumpsToFirstSection()
    {
        const string text = """
            [NEWINF]
            Primary=120mm

            [120mm]
            Damage=90

            [120mm]
            Damage=120
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("120mm", StringComparison.Ordinal) + 1);

        Ra2DefinitionTarget? target = new Ra2DefinitionProvider().GetDefinition(
            model,
            context,
            new TestFieldProvider(),
            new TestProvenanceProvider());

        Assert.NotNull(target);
        Assert.Equal("[120mm]", target.Title);
        Assert.Equal(4, target.TargetLineNumber);
        Assert.Equal(text.IndexOf("[120mm]", StringComparison.Ordinal), target.TargetSpan!.Value.Start);
    }

    [Fact]
    public void GetDefinition_OnCommentReturnsNull()
    {
        const string text = "; comment\n[NEWINF]\nStrength=300";
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("comment", StringComparison.Ordinal));

        Ra2DefinitionTarget? target = new Ra2DefinitionProvider().GetDefinition(
            model,
            context,
            new TestFieldProvider(),
            new TestProvenanceProvider());

        Assert.Null(target);
    }

    private static Ra2DocumentSemanticModel Build(string text, IRa2FieldDefinitionProvider? provider = null)
        => new Ra2DocumentSemanticModelBuilder().Build(
            new Ra2DocumentSnapshot("rulesmd.ini", text, 2),
            provider ?? new TestFieldProvider());

    private static Ra2CaretContext ContextAt(Ra2DocumentSemanticModel model, int offset)
        => new Ra2CaretContextService().GetContext(model, offset);

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
            => _definition is not null && string.Equals(_definition.Key, key, StringComparison.OrdinalIgnoreCase)
                ? FieldRegistryProvenanceLookupResult.BuiltIn(_definition)
                : FieldRegistryProvenanceLookupResult.NotFound;
    }
}
