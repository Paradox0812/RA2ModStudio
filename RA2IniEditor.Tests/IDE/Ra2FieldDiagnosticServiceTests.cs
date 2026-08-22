using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Diagnostics;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.BuiltIn;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FieldDiagnosticServiceTests
{
    [Fact]
    public void Analyze_ReportsUnknownKey_WhenSectionHasKnownField()
    {
        Ra2FieldDefinition armor = Define("Armor", Ra2FieldValueMetadata.Unknown);

        var issues = Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            Armor=light
            ArmorX=paper
            """,
            [armor]);

        var issue = Assert.Single(issues);
        Assert.Equal(Ra2FieldDiagnosticService.UnknownKeyCode, issue.Code);
        Assert.Equal("Field", issue.SourceKind);
        Assert.Equal("Field", issue.SourceText);
        Assert.Equal(IniIssueSeverity.Warning, issue.Severity);
        Assert.Equal(5, issue.LineNumber);
        Assert.Equal("E1", issue.SectionId);
        Assert.Equal("ArmorX", issue.Key);
        Assert.Contains("未知字段", issue.Message, StringComparison.Ordinal);
        Assert.Contains("字段库", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_SkipsUnknownKey_WhenProviderIsNull()
    {
        CurrentFileReadonlyDiagnosticService service = new();
        CurrentSourceSnapshot snapshot = CreateSnapshot(
            """
            [InfantryTypes]
            0=E1
            [E1]
            ArmorX=light
            """);

        IReadOnlyList<IdeDiagnosticIssueViewModel> issues = service.Analyze(snapshot);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_SkipsNumericKeys()
    {
        var issues = Analyze(
            """
            [InfantryTypes]
            0=E1
            """,
            []);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_SkipsUnknownKey_WhenSectionKindUnknown()
    {
        var issues = Analyze(
            """
            [E1]
            ArmorX=light
            """,
            []);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_SkipsUnknownKey_WhenNoFieldsKnownInSection()
    {
        var issues = Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            CustomOne=light
            CustomTwo=heavy
            """,
            []);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_DoesNotReportUnknownKey_WhenAliasMatches()
    {
        Ra2FieldDefinition strength = Define("Strength", Ra2FieldValueMetadata.Unknown, aliases: ["HitPoints"]);

        var issues = Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            HitPoints=100
            """,
            [strength]);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_ReportsInvalidBooleanValue()
    {
        Ra2FieldDefinition field = Define("IsBaseDefense", new Ra2FieldValueMetadata(Ra2FieldValueKind.Boolean));

        var issue = Assert.Single(Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            IsBaseDefense=maybe
            """,
            [field]));

        Assert.Equal(Ra2FieldDiagnosticService.InvalidBooleanValueCode, issue.Code);
        Assert.Equal(IniIssueSeverity.Warning, issue.Severity);
        Assert.Contains("布尔值可能无效", issue.Message, StringComparison.Ordinal);
        Assert.Contains("yes", issue.Message, StringComparison.Ordinal);
        Assert.Equal(4, issue.LineNumber);
    }

    [Fact]
    public void Analyze_ReportsInvalidEnumValue()
    {
        Ra2FieldDefinition field = Define("Armor", new Ra2FieldValueMetadata(
            Ra2FieldValueKind.Enum,
            allowedValues: [new Ra2FieldAllowedValue("light"), new Ra2FieldAllowedValue("heavy")]));

        var issue = Assert.Single(Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            Armor=paper
            """,
            [field]));

        Assert.Equal(Ra2FieldDiagnosticService.InvalidEnumValueCode, issue.Code);
        Assert.Equal(IniIssueSeverity.Warning, issue.Severity);
        Assert.Contains("枚举值可能无效", issue.Message, StringComparison.Ordinal);
        Assert.Equal(4, issue.LineNumber);
    }

    [Fact]
    public void Analyze_MergesInvalidEnumListItemsIntoSingleIssue()
    {
        Ra2FieldDefinition field = Define("VeteranAbilities", new Ra2FieldValueMetadata(
            Ra2FieldValueKind.EnumList,
            allowedValues: [new Ra2FieldAllowedValue("STRONG"), new Ra2FieldAllowedValue("FIREPOWER")]));

        var issue = Assert.Single(Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            VeteranAbilities=STRONG,BOGUS,OTHER,
            """,
            [field]));

        Assert.Equal(Ra2FieldDiagnosticService.InvalidEnumListValueCode, issue.Code);
        Assert.Equal(IniIssueSeverity.Warning, issue.Severity);
        Assert.Contains("列表中存在无效项", issue.Message, StringComparison.Ordinal);
        Assert.Contains("BOGUS", issue.Message, StringComparison.Ordinal);
        Assert.Contains("OTHER", issue.Message, StringComparison.Ordinal);
        Assert.Contains("<empty>", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_TruncatesLongInvalidEnumListMessage()
    {
        Ra2FieldDefinition field = Define("VeteranAbilities", new Ra2FieldValueMetadata(
            Ra2FieldValueKind.EnumList,
            allowedValues: [new Ra2FieldAllowedValue("STRONG")]));

        var issue = Assert.Single(Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            VeteranAbilities=A,B,C,D,E,F,G
            """,
            [field]));

        Assert.Contains("等 7 项", issue.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("F、G", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_DoesNotReportEnumValue_WhenAllowedValuesAreEmpty()
    {
        Ra2FieldDefinition field = Define("Armor", new Ra2FieldValueMetadata(Ra2FieldValueKind.Enum));

        var issues = Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            Armor=paper
            """,
            [field]);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_UsesSpecificBuiltInFallbackWhenProjectFallbackFieldExists()
    {
        IRa2FieldDefinitionProvider provider = new CompositeRa2FieldDefinitionProvider([
            new LocalRa2FieldDefinitionProvider([
                new Ra2FieldDefinition("Armor", [Ra2SectionKind.Unknown], FieldEditorKind.Text, Ra2FieldSourceKind.User)
            ]),
            new BuiltInRa2FieldDefinitionProvider()
        ]);

        IReadOnlyList<IdeDiagnosticIssueViewModel> issues = Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            Armor=light
            """,
            provider);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_UsesBuiltInSchemaWhenExactUserFieldIsWeakAndBuiltInIsAbstractYuriField()
    {
        IRa2FieldDefinitionProvider provider = new CompositeRa2FieldDefinitionProvider([
            new LocalRa2FieldDefinitionProvider([
                new Ra2FieldDefinition("IsBaseDefense", [Ra2SectionKind.Vehicle], FieldEditorKind.Text, Ra2FieldSourceKind.User)
            ]),
            new LocalRa2FieldDefinitionProvider([
                new Ra2FieldDefinition(
                    "IsBaseDefense",
                    [Ra2SectionKind.Techno],
                    FieldEditorKind.Boolean,
                    Ra2FieldSourceKind.Yuri,
                    valueMetadata: new Ra2FieldValueMetadata(Ra2FieldValueKind.Boolean))
            ])
        ]);

        var issue = Assert.Single(Analyze(
            """
            [VehicleTypes]
            0=MTNK
            [MTNK]
            IsBaseDefense=maybe
            """,
            provider));

        Assert.Equal(Ra2FieldDiagnosticService.InvalidBooleanValueCode, issue.Code);
    }

    [Fact]
    public void Analyze_ReportsInvalidInteger_WhenSchemaIsInteger()
    {
        Ra2FieldDefinition field = Define("Strength", new Ra2FieldValueMetadata(Ra2FieldValueKind.Integer));

        var issue = Assert.Single(Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            Strength=ten
            """,
            [field]));

        Assert.Equal(Ra2FieldDiagnosticService.InvalidNumberValueCode, issue.Code);
        Assert.Contains("数字值可能无效", issue.Message, StringComparison.Ordinal);
        Assert.Contains("整数", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_DoesNotReportNumberIssues_WhenValuesHaveInlineSemicolonComments()
    {
        Ra2FieldDefinition damage = Define(
            "Damage",
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Integer),
            sectionKind: Ra2SectionKind.Weapon);
        Ra2FieldDefinition rof = Define(
            "ROF",
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Integer),
            sectionKind: Ra2SectionKind.Weapon);
        Ra2FieldDefinition range = Define(
            "Range",
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Float),
            sectionKind: Ra2SectionKind.Weapon);

        var issues = Analyze(
            """
            [WeaponTypes]
            0=120mm
            [120mm]
            Damage=175;125
            ROF=15;20
            Range=7;5
            """,
            [damage, rof, range]);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_DoesNotReportBooleanIssue_WhenValueHasInlineSemicolonComment()
    {
        Ra2FieldDefinition field = Define("IsBaseDefense", new Ra2FieldValueMetadata(Ra2FieldValueKind.Boolean));

        var issues = Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            IsBaseDefense=yes ; comment
            """,
            [field]);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_DoesNotReportEnumIssue_WhenValueHasInlineSemicolonComment()
    {
        Ra2FieldDefinition field = Define("Armor", new Ra2FieldValueMetadata(
            Ra2FieldValueKind.Enum,
            allowedValues: [new Ra2FieldAllowedValue("light"), new Ra2FieldAllowedValue("heavy")]));

        var issues = Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            Armor=light;old armor
            """,
            [field]);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_DoesNotReportEnumListIssue_WhenValueHasInlineSemicolonComment()
    {
        Ra2FieldDefinition field = Define("VeteranAbilities", new Ra2FieldValueMetadata(
            Ra2FieldValueKind.EnumList,
            allowedValues: [new Ra2FieldAllowedValue("STRONG"), new Ra2FieldAllowedValue("FIREPOWER")]));

        var issues = Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            VeteranAbilities=STRONG,FIREPOWER;ELITE
            """,
            [field]);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_ReportsNumberIssue_WhenEffectiveValueIsInvalid()
    {
        Ra2FieldDefinition field = Define(
            "Damage",
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Integer),
            sectionKind: Ra2SectionKind.Weapon);

        var issue = Assert.Single(Analyze(
            """
            [WeaponTypes]
            0=120mm
            [120mm]
            Damage=abc;175
            """,
            [field]));

        Assert.Equal(Ra2FieldDiagnosticService.InvalidNumberValueCode, issue.Code);
        Assert.Contains("abc", issue.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("abc;175", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_DoesNotReportNumberIssue_WhenSchemaIsString()
    {
        Ra2FieldDefinition field = Define("Strength", new Ra2FieldValueMetadata(Ra2FieldValueKind.String));

        var issues = Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            Strength=10%
            """,
            [field]);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_AllowsInvariantFloatValues()
    {
        Ra2FieldDefinition field = Define("Speed", new Ra2FieldValueMetadata(Ra2FieldValueKind.Float));

        var issues = Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            Speed=-1.5
            """,
            [field]);

        Assert.Empty(issues);
    }

    [Fact]
    public void Analyze_ReportsWrongContext_WhenGlobalOnlyFieldIsUsedInObjectSection()
    {
        Ra2FieldDefinition field = new(
            "BaseDefenseDelay",
            [Ra2SectionKind.Global],
            FieldEditorKind.Float,
            Ra2FieldSourceKind.Yuri,
            "AI base defense delay",
            new Ra2FieldValueMetadata(Ra2FieldValueKind.Float),
            displayName: null,
            aliases: null,
            registryQuality: "source-verified-general-ai-control-test");

        var issue = Assert.Single(Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            BaseDefenseDelay=.25
            """,
            [field]));

        Assert.Equal(Ra2FieldDiagnosticService.WrongContextKeyCode, issue.Code);
        Assert.Equal(IniIssueSeverity.Warning, issue.Severity);
        Assert.Contains("Global", issue.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Analyze_BuiltInV32ReportsVehicleAaGuardrailAsWrongContext()
    {
        IRa2FieldDefinitionProvider provider = CreateV32Provider();

        IdeDiagnosticIssueViewModel issue = Assert.Single(Analyze(
            """
            [VehicleTypes]
            0=MTNK
            [MTNK]
            AA=yes
            """,
            provider));

        Assert.Equal(Ra2FieldDiagnosticService.WrongContextKeyCode, issue.Code);
        Assert.Equal("AA", issue.Key);
    }

    [Fact]
    public void Analyze_BuiltInV32ReportsRepresentativeQuarantinedRowsAsUnknown()
    {
        IRa2FieldDefinitionProvider provider = CreateV32Provider();

        IReadOnlyList<IdeDiagnosticIssueViewModel> issues = Analyze(
            """
            [VehicleTypes]
            0=MTNK
            [MTNK]
            Strength=300
            AllowWeaponSelectAgainstWalls=yes
            AARate=5
            AirstrikeTeam=1
            """,
            provider);

        string[] unknownKeys = issues
            .Where(issue => issue.Code == Ra2FieldDiagnosticService.UnknownKeyCode)
            .Select(issue => issue.Key!)
            .ToArray();
        Assert.Equal(["AllowWeaponSelectAgainstWalls", "AARate", "AirstrikeTeam"], unknownKeys);
    }

    [Fact]
    public void Analyze_DoesNotReportIssue_WhenDefinitionIsOnlyInferred()
    {
        Ra2FieldDefinition field = new(
            "SomeLooseKey",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Text,
            Ra2FieldSourceKind.BuiltIn,
            "推断型字段。",
            registryQuality: "name-inferred-test");

        var issues = Analyze(
            """
            [InfantryTypes]
            0=E1
            [E1]
            SomeLooseKey=yes
            """,
            [field]);

        Assert.Empty(issues);
    }

    private static IReadOnlyList<IdeDiagnosticIssueViewModel> Analyze(
        string text,
        IReadOnlyList<Ra2FieldDefinition> definitions)
    {
        return Analyze(text, new LocalRa2FieldDefinitionProvider(definitions));
    }

    private static IReadOnlyList<IdeDiagnosticIssueViewModel> Analyze(
        string text,
        IRa2FieldDefinitionProvider provider)
    {
        CurrentFileReadonlyDiagnosticService service = new();
        return service.Analyze(CreateSnapshot(text), provider);
    }

    private static CurrentSourceSnapshot CreateSnapshot(string text)
        => new(
            "C:\\mod",
            "C:\\mod\\rules.ini",
            "rules.ini",
            text,
            99,
            SourceEditorState.Loaded);

    private static IRa2FieldDefinitionProvider CreateV32Provider()
        => new LocalRa2FieldDefinitionProvider(new BuiltInFieldRegistryPackLoader().Load().Definitions);

    private static Ra2FieldDefinition Define(
        string key,
        Ra2FieldValueMetadata valueMetadata,
        IReadOnlyCollection<string>? aliases = null,
        Ra2SectionKind sectionKind = Ra2SectionKind.Infantry)
        => new(
            key,
            [sectionKind],
            FieldEditorKind.Text,
            Ra2FieldSourceKind.User,
            valueMetadata: valueMetadata,
            aliases: aliases);
}
