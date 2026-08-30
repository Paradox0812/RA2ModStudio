using System.Text;
using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Diagnostics;
using RA2IniEditor.IDE.Models;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.FieldRegistry;
using RA2IniEditor.Infrastructure.IO;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class ManualFullDiagnosticsServiceTests
{
    [Fact]
    public void Analyze_UsesInMemoryOverridesForInactiveProjectDocuments()
    {
        FakeIniFileStore store = new();
        store.Add("C:\\mod\\rules.ini", "[E1]\nName=Disk\n");
        store.Add("C:\\mod\\art.ini", "[ART]\nName=Disk\n");
        ManualFullDiagnosticsService service = new(store, new CurrentFileReadonlyDiagnosticService());
        ManualFullDiagnosticsRequest request = new(
            "C:\\mod",
            [
                new ReadonlyIniFileDescriptor("rules.ini", "C:\\mod\\rules.ini", 20),
                new ReadonlyIniFileDescriptor("art.ini", "C:\\mod\\art.ini", 20)
            ],
            currentSnapshot: null,
            currentEditorText: string.Empty,
            documentOverrides: new Dictionary<string, ManualFullDiagnosticsDocumentOverride>(StringComparer.OrdinalIgnoreCase)
            {
                ["C:\\mod\\rules.ini"] = new("[E1]\nName=One\nName=Duplicate\n", 7),
                ["C:\\mod\\art.ini"] = new("[ART]\nName=One\nName=Duplicate\n", 8)
            });

        ManualFullDiagnosticsResult result = service.Analyze(request);

        Assert.Equal(2, result.AnalyzedFileCount);
        Assert.Contains(result.Issues, issue => issue.FilePath == "C:\\mod\\rules.ini" && issue.Version == 7);
        Assert.Contains(result.Issues, issue => issue.FilePath == "C:\\mod\\art.ini" && issue.Version == 8);
    }

    [Fact]
    public void Analyze_UsesCurrentEditorTextForCurrentFileAndReadsOtherFiles()
    {
        FakeIniFileStore store = new();
        store.Add("C:\\mod\\art.ini", "[ART]\nName=Art\nName=Duplicate\n");
        ManualFullDiagnosticsService service = new(store, new CurrentFileReadonlyDiagnosticService());
        CurrentSourceSnapshot currentSnapshot = new(
            "C:\\mod",
            "C:\\mod\\rules.ini",
            "rules.ini",
            "[E1]\nName=Disk\n",
            42,
            SourceEditorState.Loaded);
        ManualFullDiagnosticsRequest request = new(
            "C:\\mod",
            [
                new ReadonlyIniFileDescriptor("rules.ini", "C:\\mod\\rules.ini", 20),
                new ReadonlyIniFileDescriptor("art.ini", "C:\\mod\\art.ini", 32)
            ],
            currentSnapshot,
            "[E1]\nName=GI\nName=Duplicate\n");

        ManualFullDiagnosticsResult result = service.Analyze(request);

        Assert.Equal(2, result.AnalyzedFileCount);
        Assert.Empty(result.SkippedFiles);
        Assert.Equal(2, result.Issues.Count);
        Assert.Contains(result.Issues, issue => issue.FilePath == "C:\\mod\\rules.ini" && issue.Version == 42);
        Assert.Contains(result.Issues, issue => issue.FilePath == "C:\\mod\\art.ini" && issue.Version == 0);
        Assert.Contains("Manual diagnostics complete", result.StatusText);
    }

    [Fact]
    public void Analyze_SkipsLargeAndFailedFilesWithoutThrowing()
    {
        FakeIniFileStore store = new();
        store.Fail("C:\\mod\\broken.ini", new IOException("locked"));
        ManualFullDiagnosticsService service = new(store, new CurrentFileReadonlyDiagnosticService());
        ManualFullDiagnosticsRequest request = new(
            "C:\\mod",
            [
                new ReadonlyIniFileDescriptor("huge.ini", "C:\\mod\\huge.ini", ManualFullDiagnosticsService.MaxAnalyzedFileSizeBytes + 1),
                new ReadonlyIniFileDescriptor("broken.ini", "C:\\mod\\broken.ini", 10)
            ],
            currentSnapshot: null,
            currentEditorText: string.Empty);

        ManualFullDiagnosticsResult result = service.Analyze(request);

        Assert.Empty(result.Issues);
        Assert.Equal(0, result.AnalyzedFileCount);
        Assert.Equal(2, result.SkippedFiles.Count);
        Assert.Contains("skipped 2", result.StatusText);
    }

    [Fact]
    public void Analyze_IncludesFieldDiagnosticsAndUsesCurrentEditorText()
    {
        FakeIniFileStore store = new();
        store.Add("C:\\mod\\art.ini", "[InfantryTypes]\n0=E1\n[E1]\nArmor=paper\n");
        ManualFullDiagnosticsService service = new(store, new CurrentFileReadonlyDiagnosticService());
        var provider = new LocalRa2FieldDefinitionProvider([
            new Ra2FieldDefinition(
                "Armor",
                [Ra2SectionKind.Infantry],
                FieldEditorKind.Enum,
                Ra2FieldSourceKind.User,
                valueMetadata: new Ra2FieldValueMetadata(
                    Ra2FieldValueKind.Enum,
                    allowedValues: [new Ra2FieldAllowedValue("light")]))
        ]);
        CurrentSourceSnapshot currentSnapshot = new(
            "C:\\mod",
            "C:\\mod\\rules.ini",
            "rules.ini",
            "[InfantryTypes]\n0=E1\n[E1]\nArmor=light\n",
            42,
            SourceEditorState.Loaded);
        ManualFullDiagnosticsRequest request = new(
            "C:\\mod",
            [
                new ReadonlyIniFileDescriptor("rules.ini", "C:\\mod\\rules.ini", 34),
                new ReadonlyIniFileDescriptor("art.ini", "C:\\mod\\art.ini", 34)
            ],
            currentSnapshot,
            "[InfantryTypes]\n0=E1\n[E1]\nArmor=invalidFromEditor\n",
            provider);

        ManualFullDiagnosticsResult result = service.Analyze(request);

        Assert.Equal(2, result.AnalyzedFileCount);
        IdeDiagnosticIssueViewModel editorIssue = Assert.Single(result.Issues, issue =>
            issue.Code == Ra2FieldDiagnosticService.InvalidEnumValueCode &&
            issue.FilePath == "C:\\mod\\rules.ini" &&
            issue.Version == 42 &&
            issue.Message.Contains("invalidFromEditor", StringComparison.Ordinal));
        Assert.Equal("Field", editorIssue.SourceKind);
        Assert.Equal(IniIssueSeverity.Warning, editorIssue.Severity);

        Assert.Contains(result.Issues, issue =>
            issue.Code == Ra2FieldDiagnosticService.InvalidEnumValueCode &&
            issue.FilePath == "C:\\mod\\art.ini" &&
            issue.Version == 0 &&
            issue.Message.Contains("paper", StringComparison.Ordinal));
    }

    [Fact]
    public void Analyze_DoesNotReportFieldNumberIssueForInlineSemicolonComment()
    {
        FakeIniFileStore store = new();
        store.Add("C:\\mod\\rules.ini", "[WeaponTypes]\n0=120mm\n[120mm]\nDamage=175;125\n");
        ManualFullDiagnosticsService service = new(store, new CurrentFileReadonlyDiagnosticService());
        var provider = new LocalRa2FieldDefinitionProvider([
            new Ra2FieldDefinition(
                "Damage",
                [Ra2SectionKind.Weapon],
                FieldEditorKind.Text,
                Ra2FieldSourceKind.User,
                valueMetadata: new Ra2FieldValueMetadata(Ra2FieldValueKind.Integer))
        ]);
        ManualFullDiagnosticsRequest request = new(
            "C:\\mod",
            [new ReadonlyIniFileDescriptor("rules.ini", "C:\\mod\\rules.ini", 64)],
            currentSnapshot: null,
            currentEditorText: string.Empty,
            provider);

        ManualFullDiagnosticsResult result = service.Analyze(request);

        Assert.DoesNotContain(result.Issues, issue => issue.Code == Ra2FieldDiagnosticService.InvalidNumberValueCode);
    }

    [Fact]
    public void Analyze_IncludesReferenceDiagnosticsForUserTriggeredFullDiagnostics()
    {
        FakeIniFileStore store = new();
        store.Add("C:\\mod\\rules.ini", "[InfantryTypes]\n0=E1\n\n[E1]\nPrimary=MissingWeapon\n");
        ManualFullDiagnosticsService service = new(store, new CurrentFileReadonlyDiagnosticService());
        var provider = new LocalRa2FieldDefinitionProvider([
            new Ra2FieldDefinition(
                "Primary",
                [Ra2SectionKind.Infantry],
                FieldEditorKind.Reference,
                Ra2FieldSourceKind.User)
        ]);
        ManualFullDiagnosticsRequest request = new(
            "C:\\mod",
            [new ReadonlyIniFileDescriptor("rules.ini", "C:\\mod\\rules.ini", 64)],
            currentSnapshot: null,
            currentEditorText: string.Empty,
            provider);

        ManualFullDiagnosticsResult result = service.Analyze(request);

        IdeDiagnosticIssueViewModel issue = Assert.Single(result.Issues, issue =>
            issue.Code == Ra2ChainDiagnosticService.MissingWeaponCode);
        Assert.Equal(Ra2ChainDiagnosticService.SourceKind, issue.SourceKind);
        Assert.Equal(IniIssueSeverity.Warning, issue.Severity);
        Assert.Equal("Primary", issue.Key);
    }

    [Fact]
    public void Analyze_UsesProjectReferenceCatalog_ForCrossFileReferences()
    {
        FakeIniFileStore store = new();
        store.Add("C:\\mod\\rules.ini", "[InfantryTypes]\n0=E1\n\n[E1]\nPrimary=GoodWeapon\n");
        store.Add("C:\\mod\\weapons.ini", "[GoodWeapon]\nDamage=90\n");
        ManualFullDiagnosticsService service = new(store, new CurrentFileReadonlyDiagnosticService());
        ManualFullDiagnosticsRequest request = new(
            "C:\\mod",
            [
                new ReadonlyIniFileDescriptor("rules.ini", "C:\\mod\\rules.ini", 64),
                new ReadonlyIniFileDescriptor("weapons.ini", "C:\\mod\\weapons.ini", 24)
            ],
            currentSnapshot: null,
            currentEditorText: string.Empty,
            CreateReferenceProvider());

        ManualFullDiagnosticsResult result = service.Analyze(request);

        Assert.Equal(2, result.AnalyzedFileCount);
        Assert.DoesNotContain(result.Issues, issue => issue.Code == Ra2ReferenceDiagnosticService.MissingTargetCode);
    }

    [Fact]
    public void Analyze_ReportsMissingReference_WhenTargetMissingFromProjectCatalog()
    {
        FakeIniFileStore store = new();
        store.Add("C:\\mod\\rules.ini", "[InfantryTypes]\n0=E1\n\n[E1]\nPrimary=MissingWeapon\n");
        ManualFullDiagnosticsService service = new(store, new CurrentFileReadonlyDiagnosticService());
        ManualFullDiagnosticsRequest request = new(
            "C:\\mod",
            [new ReadonlyIniFileDescriptor("rules.ini", "C:\\mod\\rules.ini", 64)],
            currentSnapshot: null,
            currentEditorText: string.Empty,
            CreateReferenceProvider());

        ManualFullDiagnosticsResult result = service.Analyze(request);

        IdeDiagnosticIssueViewModel issue = Assert.Single(result.Issues, issue =>
            issue.Code == Ra2ChainDiagnosticService.MissingWeaponCode);
        Assert.Contains("当前项目", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_UsesEditorCurrentText_WhenOpenedFileContributesReferenceTarget()
    {
        FakeIniFileStore store = new();
        store.Add("C:\\mod\\rules.ini", "[InfantryTypes]\n0=E1\n\n[E1]\nPrimary=DirtyWeapon\n");
        store.Add("C:\\mod\\weapons.ini", "[OldWeapon]\nDamage=90\n");
        CurrentSourceSnapshot currentSnapshot = new(
            "C:\\mod",
            "C:\\mod\\weapons.ini",
            "weapons.ini",
            "[OldWeapon]\nDamage=90\n",
            42,
            SourceEditorState.Loaded);
        ManualFullDiagnosticsService service = new(store, new CurrentFileReadonlyDiagnosticService());
        ManualFullDiagnosticsRequest request = new(
            "C:\\mod",
            [
                new ReadonlyIniFileDescriptor("rules.ini", "C:\\mod\\rules.ini", 64),
                new ReadonlyIniFileDescriptor("weapons.ini", "C:\\mod\\weapons.ini", 24)
            ],
            currentSnapshot,
            "[DirtyWeapon]\nDamage=100\n",
            CreateReferenceProvider());

        ManualFullDiagnosticsResult result = service.Analyze(request);

        Assert.Equal(2, result.AnalyzedFileCount);
        Assert.DoesNotContain(result.Issues, issue => issue.Code == Ra2ReferenceDiagnosticService.MissingTargetCode);
    }

    [Fact]
    public void Analyze_ProjectReferenceCatalog_IgnoresBackupArtifactsBinObjFolders()
    {
        FakeIniFileStore store = new();
        store.Add("C:\\mod\\rules.ini", "[InfantryTypes]\n0=E1\n\n[E1]\nPrimary=BackupOnlyWeapon\n");
        store.Add("C:\\mod\\Backups\\rules.ini", "[BackupOnlyWeapon]\nDamage=90\n");
        ManualFullDiagnosticsService service = new(store, new CurrentFileReadonlyDiagnosticService());
        ManualFullDiagnosticsRequest request = new(
            "C:\\mod",
            [
                new ReadonlyIniFileDescriptor("rules.ini", "C:\\mod\\rules.ini", 64),
                new ReadonlyIniFileDescriptor("backup.ini", "C:\\mod\\Backups\\rules.ini", 24)
            ],
            currentSnapshot: null,
            currentEditorText: string.Empty,
            CreateReferenceProvider());

        ManualFullDiagnosticsResult result = service.Analyze(request);

        Assert.Equal(1, result.AnalyzedFileCount);
        Assert.Contains(result.SkippedFiles, item => item.Contains("ignored path skipped", StringComparison.Ordinal));
        Assert.Contains(result.Issues, issue => issue.Code == Ra2ChainDiagnosticService.MissingWeaponCode);
    }

    [Fact]
    public void Analyze_IncludesChainDiagnosticsForWeaponProjectileAndWarhead()
    {
        FakeIniFileStore store = new();
        store.Add(
            "C:\\mod\\weapons.ini",
            """
            [WeaponTypes]
            0=SomeWeapon

            [SomeWeapon]
            Projectile=MissingProjectile
            Warhead=MissingWarhead
            """);
        ManualFullDiagnosticsService service = new(store, new CurrentFileReadonlyDiagnosticService());
        ManualFullDiagnosticsRequest request = new(
            "C:\\mod",
            [new ReadonlyIniFileDescriptor("weapons.ini", "C:\\mod\\weapons.ini", 96)],
            currentSnapshot: null,
            currentEditorText: string.Empty,
            CreateReferenceProvider());

        ManualFullDiagnosticsResult result = service.Analyze(request);

        Assert.Contains(result.Issues, issue => issue.Code == Ra2ChainDiagnosticService.MissingProjectileCode);
        Assert.Contains(result.Issues, issue => issue.Code == Ra2ChainDiagnosticService.MissingWarheadCode);
        Assert.DoesNotContain(result.Issues, issue => issue.Code == Ra2ReferenceDiagnosticService.MissingTargetCode);
    }

    [Fact]
    public void Analyze_UsesProjectCatalog_ForCrossFileWeaponChainTargets()
    {
        FakeIniFileStore store = new();
        store.Add("C:\\mod\\rules.ini", "[InfantryTypes]\n0=E1\n\n[E1]\nPrimary=GoodWeapon\n");
        store.Add("C:\\mod\\weapons.ini", "[GoodWeapon]\nProjectile=GoodProjectile\nWarhead=GoodWarhead\n");
        store.Add("C:\\mod\\art.ini", "[GoodProjectile]\nImage=CANON\n[GoodWarhead]\nVerses=100%,100%,100%\n");
        ManualFullDiagnosticsService service = new(store, new CurrentFileReadonlyDiagnosticService());
        ManualFullDiagnosticsRequest request = new(
            "C:\\mod",
            [
                new ReadonlyIniFileDescriptor("rules.ini", "C:\\mod\\rules.ini", 64),
                new ReadonlyIniFileDescriptor("weapons.ini", "C:\\mod\\weapons.ini", 64),
                new ReadonlyIniFileDescriptor("art.ini", "C:\\mod\\art.ini", 80)
            ],
            currentSnapshot: null,
            currentEditorText: string.Empty,
            CreateReferenceProvider());

        ManualFullDiagnosticsResult result = service.Analyze(request);

        Assert.DoesNotContain(result.Issues, issue => issue.SourceKind == Ra2ChainDiagnosticService.SourceKind);
    }

    private static LocalRa2FieldDefinitionProvider CreateReferenceProvider()
        => new(
        [
            new Ra2FieldDefinition(
                "Primary",
                [Ra2SectionKind.Infantry],
                FieldEditorKind.Reference,
                Ra2FieldSourceKind.User),
            new Ra2FieldDefinition(
                "Projectile",
                [Ra2SectionKind.Weapon],
                FieldEditorKind.Reference,
                Ra2FieldSourceKind.User),
            new Ra2FieldDefinition(
                "Warhead",
                [Ra2SectionKind.Weapon],
                FieldEditorKind.Reference,
                Ra2FieldSourceKind.User)
        ]);

    private sealed class FakeIniFileStore : IIniFileStore
    {
        private readonly Dictionary<string, string> _textByPath = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Exception> _failureByPath = new(StringComparer.OrdinalIgnoreCase);

        public void Add(string path, string text)
            => _textByPath[path] = text;

        public void Fail(string path, Exception exception)
            => _failureByPath[path] = exception;

        public IniTextReadResult ReadText(string path)
        {
            if (_failureByPath.TryGetValue(path, out Exception? exception))
                throw exception;

            return new IniTextReadResult(path, _textByPath[path], Encoding.UTF8, "\n");
        }

        public IniTextWriteResult WriteText(string path, string text, Encoding encoding)
            => throw new NotSupportedException();
    }
}
