using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Diagnostics;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.FieldRegistry;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2ChainDiagnosticServiceTests
{
    [Fact]
    public void Analyze_ReportsMissingPrimaryWeapon()
    {
        IdeDiagnosticIssueViewModel issue = Assert.Single(Analyze(
            """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=MissingWeapon
            """));

        Assert.Equal(Ra2ChainDiagnosticService.MissingWeaponCode, issue.Code);
        Assert.Equal(Ra2ChainDiagnosticService.SourceKind, issue.SourceKind);
        Assert.Equal(IniIssueSeverity.Warning, issue.Severity);
        Assert.Equal("Primary", issue.Key);
        Assert.Equal("E1", issue.SectionId);
        Assert.Contains("MissingWeapon", issue.Message, StringComparison.Ordinal);
        Assert.Contains("武器", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_DoesNotReportExistingPrimaryWeapon()
    {
        IReadOnlyList<IdeDiagnosticIssueViewModel> issues = Analyze(
            """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=GoodWeapon

            [GoodWeapon]
            Damage=90
            """);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_DoesNotReportReferencesWithInlineSemicolonComments()
    {
        IReadOnlyList<IdeDiagnosticIssueViewModel> issues = Analyze(
            """
            [InfantryTypes]
            0=MTNK

            [MTNK]
            Primary=GoodWeapon;old weapon note
            ElitePrimary=EliteWeapon;90mmE
            Weapon10=ExtraWeapon;Desolator

            [GoodWeapon]
            Projectile=GoodProjectile;projectile note
            Warhead=GoodWarhead;warhead note

            [EliteWeapon]
            Projectile=GoodProjectile
            Warhead=GoodWarhead

            [ExtraWeapon]
            Projectile=GoodProjectile
            Warhead=GoodWarhead

            [GoodProjectile]
            AA=no

            [GoodWarhead]
            Verses=100%,100%,100%
            """);

        Assert.DoesNotContain(issues, issue => issue.Code == Ra2ChainDiagnosticService.MissingWeaponCode);
        Assert.DoesNotContain(issues, issue => issue.Code == Ra2ChainDiagnosticService.MissingProjectileCode);
        Assert.DoesNotContain(issues, issue => issue.Code == Ra2ChainDiagnosticService.MissingWarheadCode);
    }

    [Fact]
    public void Analyze_StillReportsMissingReference_WhenEffectiveTargetIsMissing()
    {
        IdeDiagnosticIssueViewModel issue = Assert.Single(Analyze(
            """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=MissingWeapon;GoodWeapon

            [GoodWeapon]
            Damage=90
            """));

        Assert.Equal(Ra2ChainDiagnosticService.MissingWeaponCode, issue.Code);
        Assert.Contains("MissingWeapon", issue.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("MissingWeapon;GoodWeapon", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_ReportsMissingProjectileFromWeapon()
    {
        IdeDiagnosticIssueViewModel issue = Assert.Single(Analyze(
            """
            [WeaponTypes]
            0=SomeWeapon

            [SomeWeapon]
            Projectile=MissingProjectile
            """));

        Assert.Equal(Ra2ChainDiagnosticService.MissingProjectileCode, issue.Code);
        Assert.Equal("SomeWeapon", issue.SectionId);
        Assert.Equal("Projectile", issue.Key);
        Assert.Contains("MissingProjectile", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_ReportsMissingWarheadFromWeapon()
    {
        IdeDiagnosticIssueViewModel issue = Assert.Single(Analyze(
            """
            [WeaponTypes]
            0=SomeWeapon

            [SomeWeapon]
            Warhead=MissingWarhead
            """));

        Assert.Equal(Ra2ChainDiagnosticService.MissingWarheadCode, issue.Code);
        Assert.Equal("SomeWeapon", issue.SectionId);
        Assert.Equal("Warhead", issue.Key);
        Assert.Contains("MissingWarhead", issue.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("none")]
    [InlineData("<none>")]
    [InlineData("-1")]
    [InlineData("0")]
    [InlineData("%DYNAMIC_WEAPON%")]
    [InlineData("$WeaponVariable")]
    [InlineData("{WeaponMacro}")]
    public void Analyze_SkipsNeutralTokens(string value)
    {
        IReadOnlyList<IdeDiagnosticIssueViewModel> issues = Analyze(
            $$"""
              [InfantryTypes]
              0=E1

              [E1]
              Primary={{value}}
              """);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_SkipsUnknownSectionKind()
    {
        IReadOnlyList<IdeDiagnosticIssueViewModel> issues = Analyze(
            """
            [E1]
            Primary=MissingWeapon
            """);

        Assert.Empty(issues);
    }

    [Fact]
    public void CurrentFileDiagnostics_PrefersChainIssueOverGenericReferenceIssueOnSameLine()
    {
        CurrentFileReadonlyDiagnosticService service = new();
        CurrentSourceSnapshot snapshot = CreateSnapshot(
            """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=MissingWeapon
            """);

        IdeDiagnosticIssueViewModel issue = Assert.Single(service.Analyze(snapshot, CreateProvider()));

        Assert.Equal(Ra2ChainDiagnosticService.MissingWeaponCode, issue.Code);
        Assert.Equal(Ra2ChainDiagnosticService.SourceKind, issue.SourceKind);
    }

    private static IReadOnlyList<IdeDiagnosticIssueViewModel> Analyze(string text, Ra2ReferenceDiagnosticCatalog? catalog = null)
    {
        CurrentSourceSnapshot snapshot = CreateSnapshot(text);
        LocalRa2FieldDefinitionProvider provider = CreateProvider();
        Ra2DocumentSemanticModel model = new Ra2DocumentSemanticModelBuilder().Build(
            new Ra2DocumentSnapshot(snapshot.FilePath, snapshot.Text, snapshot.Version),
            provider);
        catalog ??= new Ra2ReferenceDiagnosticCatalogBuilder().BuildFromCurrentDocument(snapshot.FilePath, model);
        return new Ra2ChainDiagnosticService().AnalyzeCurrentDocument(snapshot, model, catalog);
    }

    private static CurrentSourceSnapshot CreateSnapshot(string text)
        => new("C:\\mod", "C:\\mod\\rules.ini", "rules.ini", text, 99, SourceEditorState.Loaded);

    private static LocalRa2FieldDefinitionProvider CreateProvider()
        => new(
        [
            new Ra2FieldDefinition("Primary", [Ra2SectionKind.Infantry], FieldEditorKind.Reference, Ra2FieldSourceKind.User),
            new Ra2FieldDefinition("Projectile", [Ra2SectionKind.Weapon], FieldEditorKind.Reference, Ra2FieldSourceKind.User),
            new Ra2FieldDefinition("Warhead", [Ra2SectionKind.Weapon], FieldEditorKind.Reference, Ra2FieldSourceKind.User)
        ]);
}
