using System.Text;
using System.Reflection;
using System.Windows.Media;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Highlighting;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class ReadonlyIniHighlightTokenizerTests
{
    [Fact]
    public void Tokenize_SectionHeader_AddsSectionHeaderToken()
    {
        const string text = "[GAPILE]\r\nName=Barracks";

        IniHighlightToken token = SingleToken(text, IniHighlightTokenKind.SectionHeader);

        Assert.Equal("[GAPILE]", Slice(text, token));
    }

    [Fact]
    public void Tokenize_WholeLineSemicolonComment_AddsCommentToken()
    {
        const string text = "; comment";

        IniHighlightToken token = SingleToken(text, IniHighlightTokenKind.Comment);

        Assert.Equal("; comment", Slice(text, token));
    }

    [Fact]
    public void Tokenize_WholeLineHashComment_AddsCommentToken()
    {
        const string text = "# comment";

        IniHighlightToken token = SingleToken(text, IniHighlightTokenKind.Comment);

        Assert.Equal("# comment", Slice(text, token));
    }

    [Fact]
    public void Tokenize_KnownKey_UsesBuiltInFieldProvider()
    {
        const string text = "[BuildingTypes]\n0=GAPILE\n[GAPILE]\nOwner=GDI";

        IniHighlightToken token = SingleToken(text, IniHighlightTokenKind.KnownKey);

        Assert.Equal("Owner", Slice(text, token));
    }

    [Fact]
    public void Tokenize_KnownKey_IsCaseInsensitive()
    {
        const string text = "[BuildingTypes]\n0=GAPILE\n[GAPILE]\nowner=GDI";

        IniHighlightToken token = SingleToken(text, IniHighlightTokenKind.KnownKey);

        Assert.Equal("owner", Slice(text, token));
    }

    [Fact]
    public void Tokenize_UnknownKey_AddsUnknownKeyToken()
    {
        const string text = "[BuildingTypes]\n0=GAPILE\n[GAPILE]\nNotAField=GDI";

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text);

        Assert.Equal(IniHighlightTokenKind.UnknownKey, TokenKindForKey(text, tokens, "NotAField"));
    }

    [Fact]
    public void Tokenize_KeyValue_AddsValueToken()
    {
        const string text = "[BuildingTypes]\n0=GAPILE\n[GAPILE]\nOwner=GDI";

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text);

        Assert.Contains(tokens, token => IsValueToken(token.Kind) && Slice(text, token) == "GDI");
    }

    [Fact]
    public void Tokenize_KeyWithWhitespace_TrimsKeyTokenRange()
    {
        const string text = "[BuildingTypes]\n0=GAPILE\n[GAPILE]\n  Owner  =  GDI";

        IniHighlightToken token = SingleToken(text, IniHighlightTokenKind.KnownKey);

        Assert.Equal("Owner", Slice(text, token));
    }

    [Fact]
    public void Tokenize_ValueWithWhitespace_TrimsValueTokenRange()
    {
        const string text = "[BuildingTypes]\n0=GAPILE\n[GAPILE]\nOwner =  GDI  ";

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text);

        Assert.Contains(tokens, token => IsValueToken(token.Kind) && Slice(text, token) == "GDI");
    }

    [Fact]
    public void Tokenize_KeyValueWithInlineComment_AddsCommentToken()
    {
        const string text = "[BuildingTypes]\n0=GAPILE\n[GAPILE]\nOwner=GDI ; inline";

        IniHighlightToken token = SingleToken(text, IniHighlightTokenKind.Comment);

        Assert.Equal("; inline", Slice(text, token));
    }

    [Fact]
    public void Tokenize_SplitsInlineCommentAfterNumberValue()
    {
        const string text = "[WeaponTypes]\n0=120mm\n[120mm]\nDamage=175;125";
        ExactKindFieldDefinitionProvider provider = new(Define(
            Ra2SectionKind.Weapon,
            "Damage",
            FieldEditorKind.Integer,
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Integer)));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.NumberValue && Slice(text, token) == "175");
        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.Comment && Slice(text, token) == ";125");
        Assert.DoesNotContain(tokens, token =>
            (token.Kind == IniHighlightTokenKind.Value || token.Kind == IniHighlightTokenKind.NumberValue) &&
            Slice(text, token) == "175;125");
    }

    [Fact]
    public void Tokenize_SplitsInlineCommentAfterReferenceValue()
    {
        const string text = "[InfantryTypes]\n0=E1\n[E1]\nPrimary=CRRadBeamWeapon;Desolator";
        ExactKindFieldDefinitionProvider provider = new(Define(
            Ra2SectionKind.Infantry,
            "Primary",
            FieldEditorKind.Reference,
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Reference)));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.ReferenceValue && Slice(text, token) == "CRRadBeamWeapon");
        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.Comment && Slice(text, token) == ";Desolator");
        Assert.DoesNotContain(tokens, token => Slice(text, token) == "CRRadBeamWeapon;Desolator");
    }

    [Fact]
    public void Tokenize_SplitsInlineCommentAfterBooleanValue()
    {
        const string text = "[InfantryTypes]\n0=E1\n[E1]\nOccupier=yes;can occupy buildings";
        ExactKindFieldDefinitionProvider provider = new(Define(
            Ra2SectionKind.Infantry,
            "Occupier",
            FieldEditorKind.Boolean,
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Boolean)));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.BooleanValue && Slice(text, token) == "yes");
        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.Comment && Slice(text, token) == ";can occupy buildings");
    }

    [Fact]
    public void Tokenize_SplitsInlineCommentAfterEnumListValue()
    {
        const string text = "[InfantryTypes]\n0=E1\n[E1]\nOwner=GDI,Nod;Allied sides";
        ExactKindFieldDefinitionProvider provider = new(Define(
            Ra2SectionKind.Infantry,
            "Owner",
            FieldEditorKind.MultiSelect,
            new Ra2FieldValueMetadata(
                Ra2FieldValueKind.EnumList,
                allowedValues: [new Ra2FieldAllowedValue("GDI"), new Ra2FieldAllowedValue("Nod")])));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.EnumValue && Slice(text, token) == "GDI,Nod");
        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.Comment && Slice(text, token) == ";Allied sides");
    }

    [Fact]
    public void Tokenize_DoesNotTreatChineseSemicolonAsComment()
    {
        const string text = "[WeaponTypes]\n0=120mm\n[120mm]\nDamage=175；125";
        ExactKindFieldDefinitionProvider provider = new(Define(
            Ra2SectionKind.Weapon,
            "Damage",
            FieldEditorKind.Integer,
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Integer)));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.DoesNotContain(tokens, token => token.Kind == IniHighlightTokenKind.Comment);
        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.NumberValue && Slice(text, token) == "175；125");
    }

    [Fact]
    public void Tokenize_SectionWithInlineHashComment_AddsCommentToken()
    {
        const string text = "[GAPILE] # inline";

        IniHighlightToken token = SingleToken(text, IniHighlightTokenKind.Comment);

        Assert.Equal("# inline", Slice(text, token));
    }

    [Fact]
    public void Tokenize_CrlfLineEndings_PreserveOffsets()
    {
        const string text = "[BuildingTypes]\r\n0=GAPILE\r\n[GAPILE]\r\nOwner=GDI";

        IniHighlightToken token = SingleToken(text, IniHighlightTokenKind.KnownKey);

        Assert.Equal(text.IndexOf("Owner", StringComparison.Ordinal), token.StartOffset);
    }

    [Fact]
    public void Tokenize_LfLineEndings_PreserveOffsets()
    {
        const string text = "[BuildingTypes]\n0=GAPILE\n[GAPILE]\nOwner=GDI";

        IniHighlightToken token = SingleToken(text, IniHighlightTokenKind.KnownKey);

        Assert.Equal(text.IndexOf("Owner", StringComparison.Ordinal), token.StartOffset);
    }

    [Fact]
    public void Tokenize_CrLineEndings_PreserveOffsets()
    {
        const string text = "[BuildingTypes]\r0=GAPILE\r[GAPILE]\rOwner=GDI";

        IniHighlightToken token = SingleToken(text, IniHighlightTokenKind.KnownKey);

        Assert.Equal(text.IndexOf("Owner", StringComparison.Ordinal), token.StartOffset);
    }

    [Fact]
    public void Tokenize_FinalLineWithoutNewLine_PreservesLastLineTokens()
    {
        const string text = "[BuildingTypes]\n0=GAPILE\n[GAPILE]\nOwner=GDI";

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text);

        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.KnownKey && Slice(text, token) == "Owner");
        Assert.Contains(tokens, token => IsValueToken(token.Kind) && Slice(text, token) == "GDI");
    }

    [Fact]
    public void Tokenize_InfantryTypesRegistry_UsesInfantryKindForObjectSection()
    {
        const string text = "[InfantryTypes]\n0=E1\n[E1]\nInfantryOnly=yes";
        ExactKindFieldDefinitionProvider provider = new((Ra2SectionKind.Infantry, "InfantryOnly"));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.KnownKey && Slice(text, token) == "InfantryOnly");
    }

    [Fact]
    public void Tokenize_InfantryFieldWithRegistryBeforeObject_IsKnownKey()
    {
        const string text = "[InfantryTypes]\n0=NEWINF\n\n[NEWINF]\nName=NEWINF\nStrength=300\nMyImportedSmokeKey=test";
        ExactKindFieldDefinitionProvider provider = new(
            (Ra2SectionKind.Infantry, "Name"),
            (Ra2SectionKind.Infantry, "Strength"),
            (Ra2SectionKind.Infantry, "MyImportedSmokeKey"));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "MyImportedSmokeKey"));
        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "Name"));
        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "Strength"));
    }

    [Fact]
    public void Tokenize_InfantryFieldWithRegistryAfterObject_IsKnownKey()
    {
        const string text = "[NEWINF]\nName=NEWINF\nStrength=300\nMyImportedSmokeKey=test\n\n[InfantryTypes]\n0=NEWINF";
        ExactKindFieldDefinitionProvider provider = new((Ra2SectionKind.Infantry, "MyImportedSmokeKey"));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "MyImportedSmokeKey"));
    }

    [Fact]
    public void Tokenize_InfantryFieldWithoutRegistry_DoesNotUseInfantryKnownKey()
    {
        const string text = "[NEWINF]\nMyImportedSmokeKey=test";
        ExactKindFieldDefinitionProvider provider = new((Ra2SectionKind.Infantry, "MyImportedSmokeKey"));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Equal(IniHighlightTokenKind.UnknownKey, TokenKindForKey(text, tokens, "MyImportedSmokeKey"));
    }

    [Fact]
    public void Tokenize_VehicleTypesRegistry_UsesVehicleKindForObjectSection()
    {
        const string text = "[VehicleTypes]\n0=MTNK\n[MTNK]\nVehicleOnly=yes";
        ExactKindFieldDefinitionProvider provider = new((Ra2SectionKind.Vehicle, "VehicleOnly"));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.KnownKey && Slice(text, token) == "VehicleOnly");
    }

    [Fact]
    public void Tokenize_VehicleFieldWithRegistryAfterObject_IsKnownKey()
    {
        const string text = "[MYTANK]\nMyVehicleKey=yes\n\n[VehicleTypes]\n0=MYTANK";
        ExactKindFieldDefinitionProvider provider = new((Ra2SectionKind.Vehicle, "MyVehicleKey"));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "MyVehicleKey"));
    }

    [Fact]
    public void Tokenize_BuildingTypesRegistry_UsesBuildingKindForObjectSection()
    {
        const string text = "[BuildingTypes]\n0=GAPOWR\n[GAPOWR]\nBuildingOnly=yes";
        ExactKindFieldDefinitionProvider provider = new((Ra2SectionKind.Building, "BuildingOnly"));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.KnownKey && Slice(text, token) == "BuildingOnly");
    }

    [Fact]
    public void Tokenize_ParticleSystemsRegistry_UsesParticleSystemKindForObjectSection()
    {
        const string text = "[ParticleSystems]\n0=SmokeTrail\n[SmokeTrail]\nMyParticleSystemKey=yes";
        ExactKindFieldDefinitionProvider provider = new((Ra2SectionKind.ParticleSystem, "MyParticleSystemKey"));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.KnownKey && Slice(text, token) == "MyParticleSystemKey");
    }

    [Fact]
    public void Tokenize_ParticleSystemsRegistryAfterObject_UsesParticleSystemKind()
    {
        const string text = "[SmokeTrail]\nMyParticleSystemKey=yes\n\n[ParticleSystems]\n0=SmokeTrail";
        ExactKindFieldDefinitionProvider provider = new((Ra2SectionKind.ParticleSystem, "MyParticleSystemKey"));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "MyParticleSystemKey"));
    }

    [Fact]
    public void Tokenize_WeaponTypesRegistry_PreservesWeaponKnownKeys()
    {
        const string text = "[WeaponTypes]\n0=120mm\n\n[120mm]\nDamage=90\nROF=65\nRange=5.75\nProjectile=Cannon\nWarhead=AP";
        ExactKindFieldDefinitionProvider provider = new(
            (Ra2SectionKind.Weapon, "Damage"),
            (Ra2SectionKind.Weapon, "ROF"),
            (Ra2SectionKind.Weapon, "Range"),
            (Ra2SectionKind.Weapon, "Projectile"),
            (Ra2SectionKind.Weapon, "Warhead"));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "Damage"));
        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "ROF"));
        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "Range"));
        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "Projectile"));
        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "Warhead"));
    }

    [Fact]
    public void Tokenize_PrimaryReference_UsesWeaponKindForReferencedSection()
    {
        const string text = "[InfantryTypes]\n0=NEWINF\n\n[NEWINF]\nPrimary=120mm\n\n[120mm]\nDamage=90\nROF=65";
        ExactKindFieldDefinitionProvider provider = new(
            (Ra2SectionKind.Weapon, "Damage"),
            (Ra2SectionKind.Weapon, "ROF"));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "Damage"));
        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "ROF"));
    }

    [Fact]
    public void Tokenize_WeaponReferences_UseProjectileAndWarheadKinds()
    {
        const string text = "[InfantryTypes]\n0=NEWINF\n\n[NEWINF]\nPrimary=120mm\n\n[120mm]\nProjectile=Cannon\nWarhead=AP\n\n[Cannon]\nImage=CANNON\n\n[AP]\nVerses=100%,100%,100%";
        ExactKindFieldDefinitionProvider provider = new(
            (Ra2SectionKind.Weapon, "Projectile"),
            (Ra2SectionKind.Weapon, "Warhead"),
            (Ra2SectionKind.Projectile, "Image"),
            (Ra2SectionKind.Warhead, "Verses"));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "Projectile"));
        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "Warhead"));
        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "Image"));
        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "Verses"));
    }

    [Fact]
    public void Tokenize_DuplicateRegistryEntries_FirstRegistrationWins()
    {
        const string text = "[InfantryTypes]\n0=THING\n\n[VehicleTypes]\n0=THING\n\n[THING]\nInfantryOnly=yes\nVehicleOnly=yes";
        ExactKindFieldDefinitionProvider provider = new(
            (Ra2SectionKind.Infantry, "InfantryOnly"),
            (Ra2SectionKind.Vehicle, "VehicleOnly"));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Equal(IniHighlightTokenKind.KnownKey, TokenKindForKey(text, tokens, "InfantryOnly"));
        Assert.Equal(IniHighlightTokenKind.UnknownKey, TokenKindForKey(text, tokens, "VehicleOnly"));
    }

    [Fact]
    public void Tokenize_UnknownSection_CanUseUnknownFallback()
    {
        const string text = "[MYSTERY]\nLooseKnown=yes";
        ExactKindFieldDefinitionProvider provider = new((Ra2SectionKind.Unknown, "LooseKnown"));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.KnownKey && Slice(text, token) == "LooseKnown");
    }

    [Fact]
    public void Tokenize_RegistrySectionItself_IsTreatedAsGlobal()
    {
        const string text = "[InfantryTypes]\nGlobalOnly=yes";
        ExactKindFieldDefinitionProvider provider = new((Ra2SectionKind.Global, "GlobalOnly"));

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text, provider);

        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.KnownKey && Slice(text, token) == "GlobalOnly");
    }

    [Fact]
    public void Tokenize_InlineCommentToken_HasExpectedOffset()
    {
        const string text = "[GAPILE]\nOwner=GDI ; inline";

        IniHighlightToken token = SingleToken(text, IniHighlightTokenKind.Comment);

        Assert.Equal(text.IndexOf(';'), token.StartOffset);
        Assert.Equal("; inline", Slice(text, token));
    }

    [Fact]
    public void HighlightingTransformer_MapsSemanticTokenKindsToExpectedBrushes()
    {
        Assert.Same(Ra2HighlightingBrushes.Comment, GetBrush(IniHighlightTokenKind.Comment));
        Assert.Same(Ra2HighlightingBrushes.NumberValue, GetBrush(IniHighlightTokenKind.NumberValue));
        Assert.Same(Ra2HighlightingBrushes.BooleanValue, GetBrush(IniHighlightTokenKind.BooleanValue));
        Assert.Same(Ra2HighlightingBrushes.ReferenceValue, GetBrush(IniHighlightTokenKind.ReferenceValue));
        Assert.Same(Ra2HighlightingBrushes.EnumValue, GetBrush(IniHighlightTokenKind.EnumValue));
        Assert.Same(Ra2HighlightingBrushes.NeutralValue, GetBrush(IniHighlightTokenKind.NeutralValue));
    }

    [Fact]
    public void HighlightingTransformer_UsesForegroundOnlyStylesForIniPracticalPalette()
    {
        string[] propertyNames = typeof(Ra2HighlightingStyle)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(property => property.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(["Brush"], propertyNames);
    }

    [Fact]
    public void HighlightingTransformer_UsesIniPracticalColorFamiliesForSemanticTokenKinds()
    {
        Assert.NotSame(GetBrush(IniHighlightTokenKind.KnownKey), GetBrush(IniHighlightTokenKind.Value));
        Assert.NotSame(GetBrush(IniHighlightTokenKind.Comment), GetBrush(IniHighlightTokenKind.Value));
        Assert.NotSame(GetBrush(IniHighlightTokenKind.UnknownKey), GetBrush(IniHighlightTokenKind.KnownKey));
        Assert.NotSame(GetBrush(IniHighlightTokenKind.NumberValue), GetBrush(IniHighlightTokenKind.Value));
        Assert.NotSame(GetBrush(IniHighlightTokenKind.SectionHeader), GetBrush(IniHighlightTokenKind.Value));
        Assert.NotSame(GetBrush(IniHighlightTokenKind.Equals), GetBrush(IniHighlightTokenKind.Value));
        AssertSameColor(GetBrush(IniHighlightTokenKind.Value), GetBrush(IniHighlightTokenKind.ReferenceValue));
        AssertSameColor(GetBrush(IniHighlightTokenKind.Value), GetBrush(IniHighlightTokenKind.BooleanValue));
        AssertSameColor(GetBrush(IniHighlightTokenKind.Value), GetBrush(IniHighlightTokenKind.EnumValue));
        AssertSameColor(GetBrush(IniHighlightTokenKind.Equals), GetBrush(IniHighlightTokenKind.NeutralValue));
    }

    [Fact]
    public void HighlightingBrushes_UseIniPracticalReadabilityPalette()
    {
        AssertBrush(Ra2HighlightingBrushes.SectionHeader, 0x1E, 0x5A, 0xA8);
        AssertBrush(Ra2HighlightingBrushes.KnownKey, 0x11, 0x18, 0x27);
        AssertBrush(Ra2HighlightingBrushes.UnknownKey, 0xC2, 0x41, 0x0C);
        AssertBrush(Ra2HighlightingBrushes.Value, 0x00, 0x98, 0xE5);
        AssertBrush(Ra2HighlightingBrushes.ReferenceValue, 0x00, 0x98, 0xE5);
        AssertBrush(Ra2HighlightingBrushes.NumberValue, 0x00, 0x88, 0xD6);
        AssertBrush(Ra2HighlightingBrushes.BooleanValue, 0x00, 0x98, 0xE5);
        AssertBrush(Ra2HighlightingBrushes.EnumValue, 0x00, 0x98, 0xE5);
        AssertBrush(Ra2HighlightingBrushes.Comment, 0x00, 0xA0, 0x00);
        AssertBrush(Ra2HighlightingBrushes.NeutralValue, 0x6B, 0x72, 0x80);
        AssertBrush(Ra2HighlightingBrushes.EqualsOperator, 0x6B, 0x72, 0x80);
    }

    [Fact]
    public void Tokenize_LargeInput_CompletesAndReturnsExpectedCategories()
    {
        const int sectionCount = 5000;
        const int keyValueCountPerSection = 4;
        StringBuilder builder = new();
        builder.AppendLine("[InfantryTypes]");
        for (int index = 0; index < sectionCount; index++)
            builder.AppendLine($"{index}=E{index}");

        for (int index = 0; index < sectionCount; index++)
        {
            builder.AppendLine($"[E{index}]");
            builder.AppendLine("Owner=GDI");
            builder.AppendLine("Strength=100");
            builder.AppendLine("UnknownLargeField=yes");
            builder.AppendLine("; comment");
        }

        string text = builder.ToString();

        IReadOnlyList<IniHighlightToken> tokens = Tokenize(text);

        Assert.True(tokens.Count >= sectionCount * keyValueCountPerSection);
        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.SectionHeader);
        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.KnownKey);
        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.UnknownKey);
        Assert.Contains(tokens, token => token.Kind == IniHighlightTokenKind.Comment);
    }

    private static IniHighlightToken SingleToken(string text, IniHighlightTokenKind kind)
        => Assert.Single(Tokenize(text), token => token.Kind == kind);

    private static IReadOnlyList<IniHighlightToken> Tokenize(string text)
        => new ReadonlyIniHighlightTokenizer(new BuiltInRa2FieldDefinitionProvider()).Tokenize(text);

    private static IReadOnlyList<IniHighlightToken> Tokenize(string text, IRa2FieldDefinitionProvider provider)
        => new ReadonlyIniHighlightTokenizer(provider).Tokenize(text);

    private static string Slice(string text, IniHighlightToken token)
        => text.Substring(token.StartOffset, token.Length);

    private static bool IsValueToken(IniHighlightTokenKind kind)
        => kind is IniHighlightTokenKind.Value or
            IniHighlightTokenKind.NumberValue or
            IniHighlightTokenKind.BooleanValue or
            IniHighlightTokenKind.ReferenceValue or
            IniHighlightTokenKind.EnumValue or
            IniHighlightTokenKind.NeutralValue;

    private static Brush GetBrush(IniHighlightTokenKind kind)
    {
        MethodInfo method = typeof(Ra2KnownFieldHighlightingTransformer).GetMethod(
            "GetBrush",
            BindingFlags.NonPublic | BindingFlags.Static)!;
        return Assert.IsAssignableFrom<Brush>(method.Invoke(null, [kind]));
    }

    private static void AssertBrush(Brush brush, byte red, byte green, byte blue)
    {
        SolidColorBrush solid = Assert.IsType<SolidColorBrush>(brush);
        Assert.Equal(Color.FromRgb(red, green, blue), solid.Color);
        Assert.True(solid.IsFrozen);
    }

    private static void AssertSameColor(Brush expected, Brush actual)
    {
        SolidColorBrush expectedSolid = Assert.IsType<SolidColorBrush>(expected);
        SolidColorBrush actualSolid = Assert.IsType<SolidColorBrush>(actual);
        Assert.Equal(expectedSolid.Color, actualSolid.Color);
    }

    private static IniHighlightTokenKind TokenKindForKey(
        string text,
        IReadOnlyList<IniHighlightToken> tokens,
        string key)
    {
        return Assert.Single(tokens, token =>
            (token.Kind == IniHighlightTokenKind.KnownKey || token.Kind == IniHighlightTokenKind.UnknownKey) &&
            Slice(text, token) == key).Kind;
    }

    private static Ra2FieldDefinition Define(
        Ra2SectionKind sectionKind,
        string key,
        FieldEditorKind editorKind,
        Ra2FieldValueMetadata valueMetadata)
        => new(
            key,
            [sectionKind],
            editorKind,
            Ra2FieldSourceKind.BuiltIn,
            valueMetadata: valueMetadata);

    private sealed class ExactKindFieldDefinitionProvider : IRa2FieldDefinitionProvider
    {
        private readonly Dictionary<(Ra2SectionKind Kind, string Key), Ra2FieldDefinition> _definitions = new();

        public ExactKindFieldDefinitionProvider(params (Ra2SectionKind Kind, string Key)[] fields)
        {
            foreach ((Ra2SectionKind kind, string key) in fields)
            {
                Add(new Ra2FieldDefinition(
                    key,
                    [kind],
                    FieldEditorKind.Text,
                    Ra2FieldSourceKind.BuiltIn));
            }
        }

        public ExactKindFieldDefinitionProvider(params Ra2FieldDefinition[] definitions)
        {
            foreach (Ra2FieldDefinition definition in definitions)
                Add(definition);
        }

        private void Add(Ra2FieldDefinition definition)
        {
            foreach (Ra2SectionKind kind in definition.AppliesTo)
                _definitions[(kind, definition.Key)] = definition;
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            return _definitions.TryGetValue((sectionKind, key), out definition!);
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
        {
            return Array.AsReadOnly(_definitions
                .Where(pair => pair.Key.Kind == sectionKind)
                .Select(pair => pair.Value)
                .ToArray());
        }

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => TryGetField(sectionKind, key, out _);
    }
}
