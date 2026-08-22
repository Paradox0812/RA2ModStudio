using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2ReferenceFinderTests
{
    [Fact]
    public void FindReferences_OnSectionHeaderReturnsCurrentDocumentReferences()
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
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.LastIndexOf("[120mm]", StringComparison.Ordinal) + 1);

        Ra2ReferenceResult result = new Ra2ReferenceFinder().FindReferences(model, context);

        Assert.Equal("120mm", result.TargetName);
        Assert.Equal(Ra2SectionKind.Weapon, result.TargetKind);
        Assert.Equal(2, result.Items.Count);
        Assert.Contains(result.Items, item => item.SourceSectionName == "NEWINF" && item.SourceKey == "Primary");
        Assert.Contains(result.Items, item => item.SourceSectionName == "TANK" && item.SourceKey == "Secondary");
    }

    [Fact]
    public void FindReferences_OnValueReferenceReturnsSameTargetReferences()
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
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("120mm", StringComparison.Ordinal) + 1);

        Ra2ReferenceResult result = new Ra2ReferenceFinder().FindReferences(model, context);

        Assert.Equal("120mm", result.TargetName);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.Equal("120mm", item.Value));
    }

    [Fact]
    public void FindReferences_FindsReferencesFromPrimarySecondaryAndWeaponSlots()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF
            1=TANK

            [NEWINF]
            Primary=120mm
            Weapon10=120mm

            [TANK]
            Secondary=120mm

            [120mm]
            Damage=90
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.LastIndexOf("[120mm]", StringComparison.Ordinal) + 1);

        Ra2ReferenceResult result = new Ra2ReferenceFinder().FindReferences(model, context);

        Assert.Equal(3, result.Items.Count);
        Assert.Contains(result.Items, item => item.SourceSectionName == "NEWINF" && item.SourceKey == "Primary");
        Assert.Contains(result.Items, item => item.SourceSectionName == "NEWINF" && item.SourceKey == "Weapon10");
        Assert.Contains(result.Items, item => item.SourceSectionName == "TANK" && item.SourceKey == "Secondary");
    }

    [Fact]
    public void FindReferences_UsesEffectiveValueBeforeInlineComment()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF
            1=FV

            [NEWINF]
            Primary=120mm;main weapon

            [FV]
            Secondary=120mm;backup cannon

            [120mm];Grizzly cannon
            Damage=90
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.LastIndexOf("[120mm]", StringComparison.Ordinal) + 1);

        Ra2ReferenceResult result = new Ra2ReferenceFinder().FindReferences(model, context);

        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item =>
        {
            Assert.Equal("120mm", item.Value);
            Assert.Equal("120mm", text.Substring(item.ValueSpan.Start, item.ValueSpan.Length));
        });
    }

    [Fact]
    public void FindReferences_DoesNotMatchCommentTextOnly()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=OtherWeapon;120mm

            [120mm]
            Damage=90
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.LastIndexOf("[120mm]", StringComparison.Ordinal) + 1);

        Ra2ReferenceResult result = new Ra2ReferenceFinder().FindReferences(model, context);

        Assert.Empty(result.Items);
    }

    [Fact]
    public void FindReferences_DoesNotMatchPartialNames()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=120mmE

            [120mm]
            Damage=90
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.LastIndexOf("[120mm]", StringComparison.Ordinal) + 1);

        Ra2ReferenceResult result = new Ra2ReferenceFinder().FindReferences(model, context);

        Assert.Empty(result.Items);
    }

    [Fact]
    public void FindReferences_ReturnsEmptyWhenNoUsageExists()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Strength=300

            [120mm]
            Damage=90
            """;
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.LastIndexOf("[120mm]", StringComparison.Ordinal) + 1);

        Ra2ReferenceResult result = new Ra2ReferenceFinder().FindReferences(model, context);

        Assert.Equal("120mm", result.TargetName);
        Assert.Empty(result.Items);
    }

    [Fact]
    public void FindReferences_WithSelectedReferenceValueUsesSelectionCandidate()
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
        Ra2DocumentSemanticModel model = Build(text);
        int selectionStart = text.IndexOf("ParaBomb", StringComparison.Ordinal);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("OtherWeapon", StringComparison.Ordinal) + 1);

        Ra2ReferenceResult result = new Ra2ReferenceFinder().FindReferences(
            model,
            context,
            new Ra2TextSpan(selectionStart, "ParaBomb".Length));

        Assert.Equal("ParaBomb", result.TargetName);
        Ra2ReferenceItem item = Assert.Single(result.Items);
        Assert.Equal("Primary", item.SourceKey);
    }

    [Fact]
    public void FindReferences_InlineCommentReferenceUsesReferenceTokenSpan()
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
        Ra2CaretContext context = ContextAt(model, text.LastIndexOf("[120mm]", StringComparison.Ordinal) + 1);

        Ra2ReferenceResult result = new Ra2ReferenceFinder().FindReferences(model, context);

        Ra2ReferenceItem item = Assert.Single(result.Items);
        Assert.Equal("120mm", item.Value);
        Assert.Equal("120mm", text.Substring(item.ValueSpan.Start, item.ValueSpan.Length));
    }

    [Fact]
    public void FindReferences_OnKeyReturnsEmptyResult()
    {
        const string text = "[NEWINF]\nStrength=300";
        Ra2DocumentSemanticModel model = Build(text);
        Ra2CaretContext context = ContextAt(model, text.IndexOf("Strength", StringComparison.Ordinal));

        Ra2ReferenceResult result = new Ra2ReferenceFinder().FindReferences(model, context);

        Assert.Equal(string.Empty, result.TargetName);
        Assert.Equal(Ra2SectionKind.Unknown, result.TargetKind);
        Assert.Empty(result.Items);
    }

    private static Ra2DocumentSemanticModel Build(string text)
        => new Ra2DocumentSemanticModelBuilder().Build(
            new Ra2DocumentSnapshot("rulesmd.ini", text, 3),
            new EmptyFieldProvider());

    private static Ra2CaretContext ContextAt(Ra2DocumentSemanticModel model, int offset)
        => new Ra2CaretContextService().GetContext(model, offset);

    private sealed class EmptyFieldProvider : IRa2FieldDefinitionProvider
    {
        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => [];

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => false;
    }
}
