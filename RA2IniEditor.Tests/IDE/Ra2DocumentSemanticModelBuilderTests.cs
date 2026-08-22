using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2DocumentSemanticModelBuilderTests
{
    [Fact]
    public void Build_SectionsUseClassifierInferredKinds()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=120mm

            [120mm]
            Projectile=Cannon
            Warhead=AP

            [Cannon]
            Image=CANNON

            [AP]
            Verses=100%,100%,100%
            """;

        Ra2DocumentSemanticModel model = Build(text);

        Assert.Equal(Ra2SectionKind.Infantry, model.FindSectionByName("NEWINF")!.Kind);
        Assert.Equal(Ra2SectionKind.Weapon, model.FindSectionByName("120mm")!.Kind);
        Assert.Equal(Ra2SectionKind.Projectile, model.FindSectionByName("Cannon")!.Kind);
        Assert.Equal(Ra2SectionKind.Warhead, model.FindSectionByName("AP")!.Kind);
    }

    [Fact]
    public void Build_KeyValuesIncludeLineKeyAndValueSpans()
    {
        const string text = "[NEWINF]\r\n  Strength = 300\r\nPrimary=120mm";

        Ra2DocumentSemanticModel model = Build(text);

        Ra2KeyValueSymbol strength = model.KeyValues.Single(keyValue => keyValue.Key == "Strength");
        Assert.Equal("NEWINF", strength.SectionName);
        Assert.Equal(2, strength.LineNumber);
        Assert.Equal("300", strength.Value);
        Assert.Equal("Strength", Slice(text, strength.KeySpan));
        Assert.Equal("300", Slice(text, strength.ValueSpan!.Value));

        Ra2KeyValueSymbol primary = model.KeyValues.Single(keyValue => keyValue.Key == "Primary");
        Assert.Equal(3, primary.LineNumber);
        Assert.Equal("120mm", Slice(text, primary.ValueSpan!.Value));
    }

    [Fact]
    public void Build_EmptyValueHasEmptyStringAndNoValueSpan()
    {
        const string text = "[NEWINF]\nPrimary=";

        Ra2DocumentSemanticModel model = Build(text);

        Ra2KeyValueSymbol primary = Assert.Single(model.KeyValues);
        Assert.Equal(string.Empty, primary.Value);
        Assert.Null(primary.ValueSpan);
    }

    [Fact]
    public void Build_MarksKnownKeysWithFieldProvider()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Strength=300
            UnknownKey=yes
            """;
        ExactKindFieldDefinitionProvider provider = new(
            (Ra2SectionKind.Infantry, "Strength"),
            (Ra2SectionKind.Infantry, "Primary"),
            (Ra2SectionKind.Weapon, "Damage"));

        Ra2DocumentSemanticModel model = Build(text, provider);

        Assert.True(model.KeyValues.Single(keyValue => keyValue.Key == "Strength").IsKnownKey);
        Assert.False(model.KeyValues.Single(keyValue => keyValue.Key == "UnknownKey").IsKnownKey);
    }

    [Fact]
    public void Build_ExtractsWeaponProjectileAndWarheadReferences()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=120mm

            [120mm]
            Projectile=Cannon
            Warhead=AP
            """;

        Ra2DocumentSemanticModel model = Build(text);

        AssertReference(model, "NEWINF", "Primary", "120mm", Ra2SectionKind.Weapon, Ra2ValueReferenceKind.WeaponReference);
        AssertReference(model, "120mm", "Projectile", "Cannon", Ra2SectionKind.Projectile, Ra2ValueReferenceKind.ProjectileReference);
        AssertReference(model, "120mm", "Warhead", "AP", Ra2SectionKind.Warhead, Ra2ValueReferenceKind.WarheadReference);
    }

    [Fact]
    public void Build_InlineCommentsAreExcludedFromReferenceValues()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=120mm;main weapon

            [120mm]
            Projectile=Cannon ; projectile comment
            Warhead=AP;warhead comment
            """;

        Ra2DocumentSemanticModel model = Build(text);

        Ra2KeyValueSymbol primary = model.KeyValues.Single(keyValue => keyValue.Key == "Primary");
        Ra2KeyValueSymbol projectile = model.KeyValues.Single(keyValue => keyValue.Key == "Projectile");
        Ra2KeyValueSymbol warhead = model.KeyValues.Single(keyValue => keyValue.Key == "Warhead");
        Assert.Equal("120mm", primary.Value);
        Assert.Equal("120mm;main weapon", primary.RawValue);
        Assert.Equal("main weapon", primary.InlineComment);
        Assert.Equal("Cannon", projectile.Value);
        Assert.Equal("Cannon ; projectile comment", projectile.RawValue);
        Assert.Equal("projectile comment", projectile.InlineComment);
        Assert.Equal("AP", warhead.Value);
        Assert.Equal("warhead comment", warhead.InlineComment);
        AssertReference(model, "NEWINF", "Primary", "120mm", Ra2SectionKind.Weapon, Ra2ValueReferenceKind.WeaponReference);
        AssertReference(model, "120mm", "Projectile", "Cannon", Ra2SectionKind.Projectile, Ra2ValueReferenceKind.ProjectileReference);
        AssertReference(model, "120mm", "Warhead", "AP", Ra2SectionKind.Warhead, Ra2ValueReferenceKind.WarheadReference);
    }

    [Fact]
    public void Build_ReferencesUseEffectiveValueBeforeSemicolonWithoutWhitespace()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            ElitePrimary=ATGUNE;90mmE
            Weapon10=CRRadBeamWeapon;Desolator

            [ATGUNE]
            Projectile=GoodProjectile;projectile note
            Warhead=GoodWarhead;warhead note
            """;

        Ra2DocumentSemanticModel model = Build(text);

        AssertReference(model, "NEWINF", "ElitePrimary", "ATGUNE", Ra2SectionKind.Weapon, Ra2ValueReferenceKind.WeaponReference);
        AssertReference(model, "NEWINF", "Weapon10", "CRRadBeamWeapon", Ra2SectionKind.Weapon, Ra2ValueReferenceKind.WeaponReference);
        AssertReference(model, "ATGUNE", "Projectile", "GoodProjectile", Ra2SectionKind.Projectile, Ra2ValueReferenceKind.ProjectileReference);
        AssertReference(model, "ATGUNE", "Warhead", "GoodWarhead", Ra2SectionKind.Warhead, Ra2ValueReferenceKind.WarheadReference);

        Ra2ValueReferenceSymbol elitePrimary = model.References.Single(reference =>
            reference.SourceSectionName == "NEWINF" &&
            reference.SourceKey == "ElitePrimary");
        Assert.Equal("ATGUNE", Slice(text, elitePrimary.ValueSpan));
    }

    [Fact]
    public void Build_ReferenceValueSpanCoversOnlyFirstTokenBeforeComma()
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

        Ra2ValueReferenceSymbol reference = Assert.Single(model.References);
        Assert.Equal("120mm", reference.TargetSectionName);
        Assert.Equal("120mm", Slice(text, reference.ValueSpan));
        Assert.DoesNotContain("SomethingElse", Slice(text, reference.ValueSpan), StringComparison.Ordinal);
    }

    [Fact]
    public void Build_ReferenceInferenceIgnoresNegativeAndFloatNumericLiterals()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=-1
            Secondary=1.5
            ElitePrimary=0.5
            EliteSecondary=.5
            Weapon1=+1
            """;

        Ra2DocumentSemanticModel model = Build(text);

        Assert.Empty(model.References);
    }

    [Fact]
    public void Build_SectionHeaderWithInlineCommentIsRecognizedConsistently()
    {
        const string text = """
            [InfantryTypes] ; registry section
            0=NEWINF

            [NEWINF]	; infantry section
            Strength=300
            """;

        Ra2DocumentSemanticModel model = Build(text);

        Ra2SectionSymbol infantryTypes = model.FindSectionByName("InfantryTypes")!;
        Ra2SectionSymbol newInf = model.FindSectionByName("NEWINF")!;
        Assert.Equal("[InfantryTypes]", Slice(text, infantryTypes.HeaderSpan));
        Assert.Equal("[NEWINF]", Slice(text, newInf.HeaderSpan));
        Assert.Equal("registry section", infantryTypes.InlineComment);
        Assert.Equal("infantry section", newInf.InlineComment);
        Assert.Equal(Ra2SectionKind.Global, infantryTypes.Kind);
        Assert.Equal(Ra2SectionKind.Infantry, newInf.Kind);
    }

    [Fact]
    public void Build_SectionHeaderWithoutWhitespaceCapturesInlineComment()
    {
        const string text = """
            [InfantryTypes]
            0=M60

            [M60];GIWeapon
            Primary=120mm;GI weapon reference
            """;

        Ra2DocumentSemanticModel model = Build(text);

        Ra2SectionSymbol section = model.FindSectionByName("M60")!;
        Ra2KeyValueSymbol primary = model.KeyValues.Single(keyValue => keyValue.Key == "Primary");
        Assert.Equal("GIWeapon", section.InlineComment);
        Assert.Equal("120mm", primary.Value);
        Assert.Equal("120mm;GI weapon reference", primary.RawValue);
        Assert.Equal("GI weapon reference", primary.InlineComment);
    }

    [Fact]
    public void Build_CapturesConservativePrecedingCommentForObjectSection()
    {
        const string text = """
            [VehicleTypes]
            0=MTNK

            ; Medium Tank
            [MTNK]
            Primary=120mm
            """;

        Ra2DocumentSemanticModel model = Build(text);

        Ra2SectionSymbol section = model.FindSectionByName("MTNK")!;
        Assert.Equal(Ra2SectionKind.Vehicle, section.Kind);
        Assert.Null(section.InlineComment);
        Assert.Equal("Medium Tank", section.PrecedingComment);
        Assert.Equal("Medium Tank", section.DisplayNote);
    }

    [Fact]
    public void Build_InlineCommentOverridesPrecedingCommentForDisplayNote()
    {
        const string text = """
            [InfantryTypes]
            0=M60

            ; old note
            [M60];GIWeapon
            Damage=15
            """;

        Ra2DocumentSemanticModel model = Build(text);

        Ra2SectionSymbol section = model.FindSectionByName("M60")!;
        Assert.Equal("GIWeapon", section.InlineComment);
        Assert.Equal("old note", section.PrecedingComment);
        Assert.Equal("GIWeapon", section.DisplayNote);
    }

    [Fact]
    public void Build_CombinesAtMostTwoShortPrecedingCommentLines()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            ; GI
            ; Allied GI
            [E1]
            Primary=M60
            """;

        Ra2DocumentSemanticModel model = Build(text);

        Ra2SectionSymbol section = model.FindSectionByName("E1")!;
        Assert.Equal("GI / Allied GI", section.PrecedingComment);
        Assert.Equal("GI / Allied GI", section.DisplayNote);
    }

    [Fact]
    public void Build_DoesNotAttachLongPrecedingCommentBlock()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            ; first
            ; second
            ; third
            [E1]
            Primary=M60
            """;

        Ra2DocumentSemanticModel model = Build(text);

        Ra2SectionSymbol section = model.FindSectionByName("E1")!;
        Assert.Null(section.PrecedingComment);
        Assert.Null(section.DisplayNote);
    }

    [Fact]
    public void Build_DoesNotAttachSeparatorOrParagraphPrecedingComment()
    {
        const string text = """
            [InfantryTypes]
            0=E1
            1=E2

            ; ----------------------------------------------------------------
            [E1]
            Primary=M60

            ; This section lists all of the vehicles types in the game and should not become a compact display note
            [E2]
            Primary=M60
            """;

        Ra2DocumentSemanticModel model = Build(text);

        Assert.Null(model.FindSectionByName("E1")!.DisplayNote);
        Assert.Null(model.FindSectionByName("E2")!.DisplayNote);
    }

    [Fact]
    public void Build_DoesNotAttachPrecedingCommentToGlobalOrUnknownSection()
    {
        const string text = """
            ; Global note
            [General]
            BuildSpeed=.7

            ; Mystery note
            [MYSTERY]
            Custom=yes
            """;

        Ra2DocumentSemanticModel model = Build(text);

        Assert.Null(model.FindSectionByName("General")!.DisplayNote);
        Assert.Null(model.FindSectionByName("MYSTERY")!.DisplayNote);
    }

    [Fact]
    public void Build_DoesNotCrossBlankLineForPrecedingComment()
    {
        const string text = """
            [VehicleTypes]
            0=MTNK

            ; Medium Tank

            [MTNK]
            Primary=120mm
            """;

        Ra2DocumentSemanticModel model = Build(text);

        Assert.Null(model.FindSectionByName("MTNK")!.DisplayNote);
    }

    [Fact]
    public void Build_SupportsLfAndFinalLineWithoutNewline()
    {
        const string text = "[NEWINF]\nStrength=300";

        Ra2DocumentSemanticModel model = Build(text);

        Assert.Single(model.Sections);
        Assert.Single(model.KeyValues);
        Assert.Equal(2, model.KeyValues[0].LineNumber);
        Assert.Equal("300", Slice(text, model.KeyValues[0].ValueSpan!.Value));
    }

    private static Ra2DocumentSemanticModel Build(string text, IRa2FieldDefinitionProvider? provider = null)
        => new Ra2DocumentSemanticModelBuilder().Build(
            new Ra2DocumentSnapshot(null, text, 7),
            provider ?? new ExactKindFieldDefinitionProvider());

    private static string Slice(string text, Ra2TextSpan span)
        => text.Substring(span.Start, span.Length);

    private static void AssertReference(
        Ra2DocumentSemanticModel model,
        string sourceSection,
        string sourceKey,
        string target,
        Ra2SectionKind targetKind,
        Ra2ValueReferenceKind referenceKind)
    {
        Ra2ValueReferenceSymbol reference = model.References.Single(value =>
            value.SourceSectionName == sourceSection &&
            value.SourceKey == sourceKey);
        Assert.Equal(target, reference.TargetSectionName);
        Assert.Equal(targetKind, reference.TargetSectionKind);
        Assert.Equal(referenceKind, reference.ReferenceKind);
    }

    private sealed class ExactKindFieldDefinitionProvider : IRa2FieldDefinitionProvider
    {
        private readonly HashSet<(Ra2SectionKind Kind, string Key)> _knownFields;

        public ExactKindFieldDefinitionProvider(params (Ra2SectionKind Kind, string Key)[] knownFields)
        {
            _knownFields = knownFields.ToHashSet();
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            if (_knownFields.Contains((sectionKind, key)))
            {
                definition = new Ra2FieldDefinition(key, [sectionKind], FieldEditorKind.Text, Ra2FieldSourceKind.Custom);
                return true;
            }

            definition = null!;
            return false;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => _knownFields
                .Where(field => field.Kind == sectionKind)
                .Select(field => new Ra2FieldDefinition(field.Key, [field.Kind], FieldEditorKind.Text, Ra2FieldSourceKind.Custom))
                .ToArray();

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => _knownFields.Contains((sectionKind, key));
    }
}
