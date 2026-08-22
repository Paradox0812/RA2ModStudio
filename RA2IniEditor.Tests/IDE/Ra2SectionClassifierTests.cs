using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Classification;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SectionClassifierTests
{
    [Fact]
    public void Classify_RegistryInfersObjectKind()
    {
        Ra2SectionClassificationResult result = Classify(
            """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Strength=300
            """);

        Assert.Equal(Ra2SectionKind.Infantry, result.SectionKindsByName["NEWINF"]);
    }

    [Fact]
    public void Classify_ForwardRegistryStillInfersObjectKind()
    {
        Ra2SectionClassificationResult result = Classify(
            """
            [NEWINF]
            Strength=300

            [InfantryTypes]
            0=NEWINF
            """);

        Assert.Equal(Ra2SectionKind.Infantry, result.SectionKindsByName["NEWINF"]);
    }

    [Fact]
    public void Classify_PrimaryReferenceInfersWeapon()
    {
        Ra2SectionClassificationResult result = Classify(
            """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=120mm

            [120mm]
            Damage=90
            ROF=65
            """);

        Assert.Equal(Ra2SectionKind.Infantry, result.SectionKindsByName["NEWINF"]);
        Assert.Equal(Ra2SectionKind.Weapon, result.SectionKindsByName["120mm"]);
    }

    [Fact]
    public void Classify_RecognizesSectionHeadersWithInlineComments()
    {
        Ra2SectionClassificationResult result = Classify(
            """
            [InfantryTypes]
            0=E1

            [E1];GI
            Primary=M60

            [M60];GIWeapon
            Damage=15
            """);

        Assert.Equal(Ra2SectionKind.Infantry, result.SectionKindsByName["E1"]);
        Assert.Equal(Ra2SectionKind.Weapon, result.SectionKindsByName["M60"]);
    }

    [Fact]
    public void Classify_WeaponReferencesInferProjectileAndWarhead()
    {
        Ra2SectionClassificationResult result = Classify(
            """
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
            """);

        Assert.Equal(Ra2SectionKind.Weapon, result.SectionKindsByName["120mm"]);
        Assert.Equal(Ra2SectionKind.Projectile, result.SectionKindsByName["Cannon"]);
        Assert.Equal(Ra2SectionKind.Warhead, result.SectionKindsByName["AP"]);
    }

    [Fact]
    public void Classify_ArbitraryUnknownKeyDoesNotInferWeapon()
    {
        Ra2SectionClassificationResult result = Classify(
            """
            [UNKNOWN]
            SomeKey=120mm

            [120mm]
            Damage=90
            """);

        Assert.False(result.SectionKindsByName.ContainsKey("120mm"));
    }

    [Fact]
    public void Classify_InvalidWeaponReferenceValuesAreIgnored()
    {
        Ra2SectionClassificationResult result = Classify(
            """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=yes
            Secondary=100
            ElitePrimary=none
            EliteSecondary=<none>
            """);

        Assert.DoesNotContain(result.SectionKindsByName, pair => pair.Value == Ra2SectionKind.Weapon);
    }

    [Fact]
    public void Classify_DoesNotClassifySidesRegistryAsSide()
    {
        Ra2SectionClassificationResult result = Classify(
            """
            [Sides]
            0=GDI
            """);

        Assert.Equal(Ra2SectionKind.Global, result.SectionKindsByName["Sides"]);
        Assert.False(result.SectionKindsByName.ContainsKey("GDI"));
    }

    [Fact]
    public void Classify_ShieldRegistryItemsAsShield()
    {
        Ra2SectionClassificationResult result = Classify(
            """
            [ShieldTypes]
            0=BasicShield

            [BasicShield]
            Strength=100
            """);

        Assert.Equal(Ra2SectionKind.Global, result.SectionKindsByName["ShieldTypes"]);
        Assert.Equal(Ra2SectionKind.Shield, result.SectionKindsByName["BasicShield"]);
    }

    [Fact]
    public void Classify_ReferenceConflictKeepsFirstReferenceAndWarns()
    {
        Ra2SectionClassificationResult result = Classify(
            """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=SharedRef

            [WeaponTypes]
            0=120mm

            [120mm]
            Projectile=SharedRef
            """);

        Assert.Equal(Ra2SectionKind.Weapon, result.SectionKindsByName["SharedRef"]);
        Assert.Single(result.Warnings);
        Assert.Contains("conflict", result.Warnings[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    private static Ra2SectionClassificationResult Classify(string text)
        => new Ra2SectionClassifier().Classify(text);
}
