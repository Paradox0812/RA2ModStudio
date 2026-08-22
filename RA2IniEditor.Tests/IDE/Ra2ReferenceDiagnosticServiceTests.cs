using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Diagnostics;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.FieldRegistry;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2ReferenceDiagnosticServiceTests
{
    [Fact]
    public void Analyze_ReportsMissingSingleReference()
    {
        var issue = Assert.Single(Analyze(
            """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=MissingWeapon
            """));

        Assert.Equal(Ra2ReferenceDiagnosticService.MissingTargetCode, issue.Code);
        Assert.Equal("Reference", issue.SourceKind);
        Assert.Equal("Reference", issue.SourceKind);
        Assert.Equal(IniIssueSeverity.Warning, issue.Severity);
        Assert.Equal(5, issue.LineNumber);
        Assert.Equal("E1", issue.SectionId);
        Assert.Equal("Primary", issue.Key);
        Assert.Contains("引用目标可能不存在", issue.Message, StringComparison.Ordinal);
        Assert.Contains("MissingWeapon", issue.Message, StringComparison.Ordinal);
        Assert.Contains("当前文件", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_DoesNotReportExistingSingleReference()
    {
        var issues = Analyze(
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
    public void Analyze_DoesNotReportReferenceWithInlineSemicolonComment()
    {
        var issues = Analyze(
            """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=GoodWeapon;old weapon

            [GoodWeapon]
            Damage=90
            """);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_ReportsMissingEffectiveReferenceBeforeInlineComment()
    {
        var issue = Assert.Single(Analyze(
            """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=MissingWeapon;GoodWeapon

            [GoodWeapon]
            Damage=90
            """));

        Assert.Equal(Ra2ReferenceDiagnosticService.MissingTargetCode, issue.Code);
        Assert.Contains("MissingWeapon", issue.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("MissingWeapon;GoodWeapon", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_UsesCaseInsensitiveSectionLookup()
    {
        var issues = Analyze(
            """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=goodweapon

            [GoodWeapon]
            Damage=90
            """);

        Assert.Empty(issues);
    }

    [Theory]
    [InlineData("none")]
    [InlineData("<none>")]
    [InlineData("-1")]
    [InlineData("0")]
    [InlineData("%DYNAMIC_WEAPON%")]
    [InlineData("$WeaponVariable")]
    [InlineData("{WeaponMacro}")]
    public void Analyze_SkipsNeutralOrComplexReferenceValues(string value)
    {
        var issues = Analyze(
            $$"""
              [InfantryTypes]
              0=E1

              [E1]
              Primary={{value}}
              """);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_SkipsAllowedValues()
    {
        LocalRa2FieldDefinitionProvider provider = CreateProvider(new Ra2FieldValueMetadata(
            Ra2FieldValueKind.Enum,
            allowedValues: [new Ra2FieldAllowedValue("AllowedBySchema")]));

        var issues = Analyze(
            """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=AllowedBySchema
            """,
            provider);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_SkipsWhenReferenceMetadataMissing()
    {
        var issues = Analyze(
            """
            [InfantryTypes]
            0=E1

            [E1]
            CustomReference=MissingWeapon
            """);

        Assert.Empty(issues);
    }

    [Fact]
    public void CurrentFileDiagnostics_IncludesReferenceDiagnostics()
    {
        CurrentFileReadonlyDiagnosticService service = new();
        CurrentSourceSnapshot snapshot = CreateSnapshot(
            """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=MissingWeapon
            """);

        var issue = Assert.Single(service.Analyze(snapshot, CreateProvider()));

        Assert.Equal(Ra2ChainDiagnosticService.MissingWeaponCode, issue.Code);
        Assert.Equal(Ra2ChainDiagnosticService.SourceKind, issue.SourceKind);
    }

    [Fact]
    public void CurrentFileDiagnostics_UsesCurrentDocumentCatalog_NotProjectCatalog()
    {
        CurrentFileReadonlyDiagnosticService service = new();
        CurrentSourceSnapshot snapshot = CreateSnapshot(
            """
            [InfantryTypes]
            0=E1

            [E1]
            Primary=WeaponInAnotherFile
            """);

        var issue = Assert.Single(service.Analyze(snapshot, CreateProvider()));

        Assert.Equal(Ra2ChainDiagnosticService.MissingWeaponCode, issue.Code);
        Assert.Contains("当前文件", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void CatalogBuilder_BuildsProjectCatalogFromMultipleDocuments()
    {
        Ra2ReferenceDiagnosticCatalog catalog = BuildCatalog(
            ("C:\\mod\\rules.ini", "[InfantryTypes]\n0=E1\n[E1]\nPrimary=GoodWeapon\n"),
            ("C:\\mod\\weapons.ini", "[GoodWeapon]\nDamage=90\n"));

        Assert.True(catalog.ContainsSection("GoodWeapon"));
        Assert.True(catalog.TryGetSection("GoodWeapon", out Ra2ReferenceDiagnosticCatalogEntry entry));
        Assert.Equal("C:\\mod\\weapons.ini", entry.FilePath);
    }

    [Fact]
    public void CatalogBuilder_UsesCaseInsensitiveSectionLookupAcrossDocuments()
    {
        Ra2ReferenceDiagnosticCatalog catalog = BuildCatalog(
            ("C:\\mod\\weapons.ini", "[GoodWeapon]\nDamage=90\n"));

        Assert.True(catalog.ContainsSection("goodweapon"));
    }

    [Fact]
    public void CatalogBuilder_AllowsDuplicateSectionNamesAcrossFiles()
    {
        Ra2ReferenceDiagnosticCatalog catalog = BuildCatalog(
            ("C:\\mod\\rules.ini", "[GoodWeapon]\nDamage=90\n"),
            ("C:\\mod\\art.ini", "[GoodWeapon]\nImage=WEAPON\n"));

        Assert.True(catalog.ContainsSection("GoodWeapon"));
    }

    private static IReadOnlyList<Ra2DiagnosticFact> Analyze(
        string text,
        IRa2FieldDefinitionProvider? provider = null)
    {
        provider ??= CreateProvider();
        CurrentSourceSnapshot snapshot = CreateSnapshot(text);
        Ra2DocumentSnapshot documentSnapshot = new(snapshot.FilePath, snapshot.Text, snapshot.Version);
        Ra2DocumentSemanticModel model = new Ra2DocumentSemanticModelBuilder().Build(documentSnapshot, provider);
        Ra2ReferenceDiagnosticCatalog catalog = new Ra2ReferenceDiagnosticCatalogBuilder().BuildFromCurrentDocument(snapshot.FilePath, model);
        return new Ra2ReferenceDiagnosticService().AnalyzeCurrentDocument(documentSnapshot, model, provider, catalog);
    }

    private static CurrentSourceSnapshot CreateSnapshot(string text)
        => new(
            "C:\\mod",
            "C:\\mod\\rules.ini",
            "rules.ini",
            text,
            99,
            SourceEditorState.Loaded);

    private static LocalRa2FieldDefinitionProvider CreateProvider(Ra2FieldValueMetadata? primaryMetadata = null)
        => new(
        [
            new Ra2FieldDefinition(
                "Primary",
                [Ra2SectionKind.Infantry],
                FieldEditorKind.Reference,
                Ra2FieldSourceKind.User,
                valueMetadata: primaryMetadata ?? Ra2FieldValueMetadata.Unknown)
        ]);

    private static Ra2ReferenceDiagnosticCatalog BuildCatalog(params (string FilePath, string Text)[] documents)
    {
        LocalRa2FieldDefinitionProvider provider = CreateProvider();
        return new Ra2ReferenceDiagnosticCatalogBuilder().BuildFromDocuments(documents.Select(document =>
        {
            Ra2DocumentSemanticModel model = new Ra2DocumentSemanticModelBuilder().Build(
                new Ra2DocumentSnapshot(document.FilePath, document.Text, 1),
                provider);
            return new Ra2ReferenceCatalogDocument(document.FilePath, model);
        }));
    }
}
