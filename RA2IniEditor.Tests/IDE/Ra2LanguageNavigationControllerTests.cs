using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Controllers.Language;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2LanguageNavigationControllerTests
{
    [Fact]
    public void GoToDefinition_OnValueReferenceReturnsJumpResult()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=120mm

            [120mm]
            Damage=90
            """;
        Ra2LanguageNavigationRequest request = CreateRequest(text, text.IndexOf("120mm", StringComparison.Ordinal) + 1);

        Ra2GoToDefinitionResult result = CreateController().GoToDefinition(request);

        Assert.True(result.Success);
        Assert.Equal(Ra2GoToDefinitionAction.JumpToDefinition, result.Action);
        Assert.Equal("[120mm]", result.Target!.Title);
        Assert.Equal(text.IndexOf("[120mm]", StringComparison.Ordinal), result.TargetOffset);
        Assert.Equal("120mm", result.SectionName);
        Assert.Contains("Jumped to definition [120mm]", result.Message);
    }

    [Fact]
    public void GoToDefinition_UsesEffectiveValueBeforeInlineComment()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=120mm;main weapon

            [120mm];Grizzly cannon
            Damage=90
            """;
        Ra2LanguageNavigationRequest request = CreateRequest(text, text.IndexOf("120mm;main", StringComparison.Ordinal) + 1);

        Ra2GoToDefinitionResult result = CreateController().GoToDefinition(request);

        Assert.True(result.Success);
        Assert.Equal(Ra2GoToDefinitionAction.JumpToDefinition, result.Action);
        Assert.Equal(text.LastIndexOf("[120mm]", StringComparison.Ordinal), result.TargetOffset);
        Assert.Equal("120mm", result.SectionName);
    }

    [Fact]
    public void PeekDefinition_OnKnownKeyReturnsFieldDefinitionPreview()
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
            "Hit points.");
        Ra2LanguageNavigationRequest request = CreateRequest(
            text,
            text.IndexOf("Strength", StringComparison.Ordinal) + 2,
            new TestFieldProvider(definition),
            new TestProvenanceProvider(definition));

        Ra2PeekDefinitionResult result = CreateController().PeekDefinition(request);

        Assert.True(result.Success);
        Assert.Equal(Ra2DefinitionTargetKind.FieldDefinition, result.Target!.Kind);
        Assert.Equal("Strength", result.Target.Title);
        Assert.Contains("Opened definition preview", result.Message);
    }

    [Fact]
    public void FindReferences_OnSectionHeaderReturnsReferencesResult()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF
            1=TANK

            [NEWINF]
            Primary=120mm

            [TANK]
            Secondary=120mm

            [120mm]
            Damage=90
            """;
        Ra2LanguageNavigationRequest request = CreateRequest(text, text.LastIndexOf("[120mm]", StringComparison.Ordinal) + 1);

        Ra2FindReferencesNavigationResult result = CreateController().FindReferences(request);

        Assert.True(result.Success);
        Assert.Equal("120mm", result.References!.TargetName);
        Assert.Equal(2, result.References.Items.Count);
        Assert.Contains("Found 2 reference(s) for [120mm]", result.Message);
    }

    [Fact]
    public void FindReferences_UsesSelectionAsReferenceCandidate()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=ParaBomb
            Secondary=OtherWeapon

            [ParaBomb]
            Damage=90
            """;
        int selectionStart = text.IndexOf("ParaBomb", StringComparison.Ordinal);
        Ra2LanguageNavigationRequest request = CreateRequest(
            text,
            text.IndexOf("OtherWeapon", StringComparison.Ordinal) + 1,
            selectionSpan: new Ra2TextSpan(selectionStart, "ParaBomb".Length));

        Ra2FindReferencesNavigationResult result = CreateController().FindReferences(request);

        Assert.True(result.Success);
        Assert.Equal("ParaBomb", result.References!.TargetName);
        Assert.Single(result.References.Items);
    }

    [Fact]
    public void GoToDefinition_OnCommentReturnsFailure()
    {
        const string text = "; comment\n[NEWINF]\nStrength=300";
        Ra2LanguageNavigationRequest request = CreateRequest(text, text.IndexOf("comment", StringComparison.Ordinal));

        Ra2GoToDefinitionResult result = CreateController().GoToDefinition(request);

        Assert.False(result.Success);
        Assert.Equal(Ra2GoToDefinitionAction.None, result.Action);
        Assert.Equal("No definition is available at the current caret position.", result.Message);
    }

    [Fact]
    public void GoToDefinition_OnSecondCommaValueTokenDoesNotJump()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=120mm,SomethingElse

            [120mm]
            Damage=90
            """;
        Ra2LanguageNavigationRequest request = CreateRequest(text, text.IndexOf("SomethingElse", StringComparison.Ordinal) + 1);

        Ra2GoToDefinitionResult result = CreateController().GoToDefinition(request);

        Assert.False(result.Success);
        Assert.Equal("No definition is available at the current caret position.", result.Message);
    }

    private static Ra2LanguageNavigationController CreateController()
        => new(new Ra2DefinitionProvider(), new Ra2ReferenceFinder());

    private static Ra2LanguageNavigationRequest CreateRequest(
        string text,
        int offset,
        IRa2FieldDefinitionProvider? fieldProvider = null,
        IFieldRegistryProvenanceProvider? provenanceProvider = null,
        Ra2TextSpan? selectionSpan = null)
    {
        IRa2FieldDefinitionProvider effectiveFieldProvider = fieldProvider ?? new TestFieldProvider();
        Ra2DocumentSemanticModel model = new Ra2DocumentSemanticModelBuilder().Build(
            new Ra2DocumentSnapshot("rulesmd.ini", text, 1),
            effectiveFieldProvider);
        Ra2CaretContext context = new Ra2CaretContextService().GetContext(model, offset);
        return new Ra2LanguageNavigationRequest(
            model,
            context,
            effectiveFieldProvider,
            provenanceProvider ?? new TestProvenanceProvider(),
            selectionSpan);
    }

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
