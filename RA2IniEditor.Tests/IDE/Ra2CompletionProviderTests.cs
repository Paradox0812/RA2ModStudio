using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.Infrastructure.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.BuiltIn;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionProviderTests
{
    [Fact]
    public void GetCompletions_KeyPrefixReturnsFieldDefinitionsForCurrentSectionKind()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Str
            """;
        TestFieldProvider fieldProvider = new(
            new Ra2FieldDefinition("Strength", [Ra2SectionKind.Infantry], FieldEditorKind.Integer, Ra2FieldSourceKind.BuiltIn, "Hit points"),
            new Ra2FieldDefinition("Sight", [Ra2SectionKind.Infantry], FieldEditorKind.Integer, Ra2FieldSourceKind.BuiltIn));
        Ra2CompletionResult result = GetCompletions(text, text.IndexOf("Str", StringComparison.Ordinal) + 3, fieldProvider);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("Strength", item.Label);
        Assert.Equal(Ra2CompletionItemKind.Key, item.Kind);
        Assert.Equal(Ra2CompletionItemSourceKind.FieldRegistry, item.SourceKind);
        Assert.Equal("Strength=", item.InsertText);
        Assert.Equal("Str", Slice(text, result.ReplacementSpan));
        Assert.Equal("Hit points", item.Documentation);
    }

    [Fact]
    public void GetCompletions_KeyPrefixExcludesDiagnosticOnlyTrustLevels()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Field
            """;
        TestFieldProvider fieldProvider = new(
            CreateField("FieldVerified", "source-verified-test"),
            CreateField("FieldInferred", "community-reference-inferred-test"),
            CreateField("FieldManual", "manual-curated-test"),
            CreateField("FieldUnknown", "community-reviewed-test"),
            CreateField("FieldGuardrail", "source-verified-guardrail-test"),
            CreateField("FieldObsolete", "community-verified-obsolete-test"),
            CreateField("FieldNonExistent", "source-verified-non-existent-test"),
            CreateField("FieldPseudo", "community-reviewed-pseudo-field-test"));

        Ra2CompletionResult result = GetCompletions(
            text,
            text.IndexOf("Field", StringComparison.Ordinal) + "Field".Length,
            fieldProvider);

        string[] labels = result.Items.Select(item => item.Label).ToArray();
        Assert.Contains("FieldVerified", labels);
        Assert.Contains("FieldInferred", labels);
        Assert.Contains("FieldManual", labels);
        Assert.Contains("FieldUnknown", labels);
        Assert.DoesNotContain("FieldGuardrail", labels);
        Assert.DoesNotContain("FieldObsolete", labels);
        Assert.DoesNotContain("FieldNonExistent", labels);
        Assert.DoesNotContain("FieldPseudo", labels);
    }

    [Fact]
    public void GetCompletions_BuiltInAaAgStayDiagnosticForUnitsAndAvailableForProjectiles()
    {
        LocalFieldRegistryLoadResult loadResult = new BuiltInFieldRegistryPackLoader().Load();
        LocalRa2FieldDefinitionProvider fieldProvider = new(loadResult.Definitions);
        const string unitText = """
            [VehicleTypes]
            0=MTNK

            [MTNK]
            A
            """;
        const string projectileText = """
            [ProjectileTypes]
            0=CANNON

            [CANNON]
            A
            """;

        Ra2CompletionResult unitResult = GetCompletions(
            unitText,
            unitText.LastIndexOf('A') + 1,
            fieldProvider);
        Ra2CompletionResult projectileResult = GetCompletions(
            projectileText,
            projectileText.LastIndexOf('A') + 1,
            fieldProvider);

        Assert.DoesNotContain(unitResult.Items, item => item.Label is "AA" or "AG");
        Assert.Contains(projectileResult.Items, item => item.Label == "AA");
        Assert.Contains(projectileResult.Items, item => item.Label == "AG");
        Assert.True(fieldProvider.TryGetField(Ra2SectionKind.Vehicle, "AA", out Ra2FieldDefinition aaGuardrail));
        Assert.Contains("guardrail", aaGuardrail.RegistryQuality, StringComparison.OrdinalIgnoreCase);
        Assert.True(fieldProvider.TryGetField(Ra2SectionKind.Vehicle, "AG", out Ra2FieldDefinition agGuardrail));
        Assert.Contains("guardrail", agGuardrail.RegistryQuality, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetCompletions_KeyPrefixBeforeExistingEqualsDoesNotAppendSecondEquals()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Pr=120mm
            """;
        TestFieldProvider fieldProvider = new(
            new Ra2FieldDefinition("Primary", [Ra2SectionKind.Infantry], FieldEditorKind.Reference, Ra2FieldSourceKind.BuiltIn));
        int caretOffset = text.IndexOf("Pr=120mm", StringComparison.Ordinal) + "Pr".Length;

        Ra2CompletionResult result = GetCompletions(text, caretOffset, fieldProvider);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("Primary", item.Label);
        Assert.Equal("Primary", item.InsertText);
        Assert.Equal("Pr", Slice(text, result.ReplacementSpan));
    }

    [Fact]
    public void GetCompletions_UnknownSectionKindDoesNotReturnAggressiveKeyCompletions()
    {
        const string text = """
            [MYSTERY]
            Str
            """;
        TestFieldProvider fieldProvider = new(
            new Ra2FieldDefinition("Strength", [Ra2SectionKind.Infantry], FieldEditorKind.Integer, Ra2FieldSourceKind.BuiltIn));

        Ra2CompletionResult result = GetCompletions(text, text.IndexOf("Str", StringComparison.Ordinal) + 3, fieldProvider);

        Assert.Empty(result.Items);
    }

    [Fact]
    public void GetCompletions_PrimaryValueReturnsCurrentDocumentWeaponSections()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF
            1=OTHERINF

            [NEWINF]
            Primary=

            [OTHERINF]
            Primary=120mm

            [120mm]
            Damage=90
            """;

        Ra2CompletionResult result = GetCompletions(text, text.IndexOf("Primary=", StringComparison.Ordinal) + "Primary=".Length);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("120mm", item.Label);
        Assert.Equal(Ra2CompletionItemKind.Reference, item.Kind);
        Assert.Equal(Ra2CompletionItemSourceKind.CurrentDocumentSection, item.SourceKind);
        Assert.Equal("120mm", item.InsertText);
        Assert.Equal(0, result.ReplacementSpan.Length);
    }

    [Fact]
    public void GetCompletions_ReferenceTargetCandidateUsesSectionInlineCommentAsDocumentation()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=

            [M60];GIWeapon
            Damage=15
            """;

        Ra2CompletionResult result = GetCompletions(text, text.IndexOf("Primary=", StringComparison.Ordinal) + "Primary=".Length);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("M60", item.Label);
        Assert.Equal("M60", item.InsertText);
        Assert.Equal("GIWeapon", item.Documentation);
        Assert.DoesNotContain(";", item.InsertText, StringComparison.Ordinal);
    }

    [Fact]
    public void GetCompletions_ReferenceTargetCandidateUsesPrecedingSectionCommentAsDocumentation()
    {
        const string text = """
            [InfantryTypes]
            0=E1
            1=OTHER

            [E1]
            Primary=

            [OTHER]
            Primary=120mm

            ; GI Weapon
            [120mm]
            Damage=15
            """;

        Ra2CompletionResult result = GetCompletions(text, text.IndexOf("Primary=", StringComparison.Ordinal) + "Primary=".Length);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("120mm", item.Label);
        Assert.Equal("120mm", item.InsertText);
        Assert.Equal("GI Weapon", item.Documentation);
    }

    [Fact]
    public void GetCompletions_FieldKeyCandidateDoesNotUseSectionDisplayNote()
    {
        const string text = """
            [InfantryTypes]
            0=E1

            ; GI
            [E1]
            Str
            """;
        TestFieldProvider fieldProvider = new(
            new Ra2FieldDefinition("Strength", [Ra2SectionKind.Infantry], FieldEditorKind.Integer, Ra2FieldSourceKind.BuiltIn, "Hit points"));

        Ra2CompletionResult result = GetCompletions(text, text.IndexOf("Str", StringComparison.Ordinal) + 3, fieldProvider);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("Strength", item.Label);
        Assert.Equal("Hit points", item.Documentation);
    }

    [Fact]
    public void GetCompletions_ReferenceTargetCandidateTruncatesLongSectionInlineComment()
    {
        string longComment = new('A', 90);
        string text = $"""
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=

            [M60];{longComment}
            Damage=15
            """;

        Ra2CompletionResult result = GetCompletions(text, text.IndexOf("Primary=", StringComparison.Ordinal) + "Primary=".Length);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal(83, item.Documentation!.Length);
        Assert.EndsWith("...", item.Documentation, StringComparison.Ordinal);
        Assert.Equal("M60", item.InsertText);
    }

    [Fact]
    public void GetCompletions_ProjectileValueReturnsProjectileSections()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF
            1=OTHERINF

            [NEWINF]
            Primary=120mm

            [OTHERINF]
            Primary=OtherWeapon

            [120mm]
            Projectile=

            [OtherWeapon]
            Projectile=Cannon

            [Cannon]
            Image=CANNON
            """;

        Ra2CompletionResult result = GetCompletions(text, text.IndexOf("Projectile=", StringComparison.Ordinal) + "Projectile=".Length);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("Cannon", item.Label);
        Assert.Contains("Projectile", item.Detail);
        Assert.Equal(Ra2CompletionItemSourceKind.CurrentDocumentSection, item.SourceKind);
    }

    [Fact]
    public void GetCompletions_WarheadValueReturnsWarheadSections()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF
            1=OTHERINF

            [NEWINF]
            Primary=120mm

            [OTHERINF]
            Primary=OtherWeapon

            [120mm]
            Warhead=

            [OtherWeapon]
            Warhead=AP

            [AP]
            Verses=100%,100%,100%
            """;

        Ra2CompletionResult result = GetCompletions(text, text.IndexOf("Warhead=", StringComparison.Ordinal) + "Warhead=".Length);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("AP", item.Label);
        Assert.Contains("Warhead", item.Detail);
        Assert.Equal(Ra2CompletionItemSourceKind.CurrentDocumentSection, item.SourceKind);
    }

    [Fact]
    public void GetCompletions_UnknownFallbackReferenceIsMarkedAsUnclassified()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=MYS

            [MYSTERY]
            Custom=yes
            """;
        int caretOffset = text.IndexOf("Primary=MYS", StringComparison.Ordinal) + "Primary=MYS".Length;

        Ra2CompletionResult result = GetCompletions(text, caretOffset);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("MYSTERY", item.Label);
        Assert.Equal(Ra2CompletionItemSourceKind.CurrentDocumentUnknownFallback, item.SourceKind);
        Assert.Contains("fallback", item.Detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetCompletions_ReferencePrefixReplacementSpanCoversOnlyCurrentPrefix()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=12

            [120mm]
            Damage=90
            """;
        int caretOffset = text.IndexOf("Primary=12", StringComparison.Ordinal) + "Primary=12".Length;

        Ra2CompletionResult result = GetCompletions(text, caretOffset);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("120mm", item.Label);
        Assert.Equal("12", Slice(text, result.ReplacementSpan));
    }

    [Fact]
    public void GetCompletions_DuplicateSectionsAreDeduplicated()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=

            [120mm]
            Damage=90

            [120mm]
            Damage=120
            """;

        Ra2CompletionResult result = GetCompletions(text, text.IndexOf("Primary=", StringComparison.Ordinal) + "Primary=".Length);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("120mm", item.Label);
    }

    [Fact]
    public void GetCompletions_ItemsUseDeterministicPriorityAndLabelOrdering()
    {
        const string text = "[InfantryTypes]\n0=NEWINF\n\n[NEWINF]\n";
        TestFieldProvider fieldProvider = new(
            new Ra2FieldDefinition("Sight", [Ra2SectionKind.Infantry], FieldEditorKind.Integer, Ra2FieldSourceKind.BuiltIn),
            new Ra2FieldDefinition("Strength", [Ra2SectionKind.Infantry], FieldEditorKind.Integer, Ra2FieldSourceKind.BuiltIn),
            new Ra2FieldDefinition("Armor", [Ra2SectionKind.Infantry], FieldEditorKind.Enum, Ra2FieldSourceKind.BuiltIn));
        int caretOffset = text.Length;

        Ra2CompletionResult result = GetCompletions(text, caretOffset, fieldProvider);

        Assert.Equal(["Armor", "Sight", "Strength"], result.Items.Select(item => item.Label).ToArray());
    }

    [Fact]
    public void GetCompletions_IncludesSpecificBuiltInFieldWhenProjectFallbackFieldExists()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Arm
            """;
        IRa2FieldDefinitionProvider fieldProvider = new CompositeRa2FieldDefinitionProvider([
            new TestFieldProvider(new Ra2FieldDefinition("Armor", [Ra2SectionKind.Unknown], FieldEditorKind.Text, Ra2FieldSourceKind.User)),
            new BuiltInRa2FieldDefinitionProvider()
        ]);

        Ra2CompletionResult result = GetCompletions(text, text.IndexOf("Arm", StringComparison.Ordinal) + 3, fieldProvider);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("Armor", item.Label);
        Assert.Equal("Type: Enum", item.Detail);
    }

    [Fact]
    public void GetCompletions_UsesBuiltInDetailsWhenExactUserFieldIsWeakAndBuiltInIsAbstractYuriField()
    {
        const string text = """
            [VehicleTypes]
            0=MTNK

            [MTNK]
            Pri
            """;
        IRa2FieldDefinitionProvider fieldProvider = new CompositeRa2FieldDefinitionProvider([
            new TestFieldProvider(new Ra2FieldDefinition("Primary", [Ra2SectionKind.Vehicle], FieldEditorKind.Text, Ra2FieldSourceKind.User)),
            new TestFieldProvider(new Ra2FieldDefinition(
                "Primary",
                [Ra2SectionKind.Techno],
                FieldEditorKind.Reference,
                Ra2FieldSourceKind.Yuri,
                "Primary weapon reference.",
                new Ra2FieldValueMetadata(Ra2FieldValueKind.Reference)))
        ]);

        Ra2CompletionResult result = GetCompletions(text, text.IndexOf("Pri", StringComparison.Ordinal) + 3, fieldProvider);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("Primary", item.Label);
        Assert.Equal("Type: Reference", item.Detail);
        Assert.Equal("Primary weapon reference.", item.Documentation);
    }

    private static Ra2CompletionResult GetCompletions(
        string text,
        int caretOffset,
        IRa2FieldDefinitionProvider? fieldProvider = null)
    {
        fieldProvider ??= new TestFieldProvider();
        Ra2DocumentSnapshot snapshot = new("rulesmd.ini", text, 1);
        Ra2DocumentSemanticModel model = new Ra2DocumentSemanticModelBuilder().Build(snapshot, fieldProvider);
        Ra2CaretContext context = new Ra2CaretContextService().GetContext(model, caretOffset);
        return new Ra2CompletionProvider().GetCompletions(new Ra2CompletionRequest(
            snapshot,
            model,
            context,
            caretOffset,
            fieldProvider));
    }

    private static string Slice(string text, Ra2TextSpan span)
        => text.Substring(span.Start, span.Length);

    private static Ra2FieldDefinition CreateField(string key, string quality)
        => new(
            key,
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Text,
            Ra2FieldSourceKind.BuiltIn,
            registryQuality: quality);

    private sealed class TestFieldProvider : IRa2FieldDefinitionProvider
    {
        private readonly IReadOnlyList<Ra2FieldDefinition> _definitions;

        public TestFieldProvider(params Ra2FieldDefinition[] definitions)
        {
            _definitions = definitions;
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = _definitions.FirstOrDefault(field =>
                AppliesTo(field, sectionKind) &&
                string.Equals(field.Key, key, StringComparison.OrdinalIgnoreCase))!;
            return definition is not null;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => _definitions.Where(field => AppliesTo(field, sectionKind)).ToArray();

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => TryGetField(sectionKind, key, out _);

        private static bool AppliesTo(Ra2FieldDefinition definition, Ra2SectionKind sectionKind)
        {
            if (definition.AppliesTo.Count == 0 ||
                definition.AppliesTo.Contains(sectionKind) ||
                definition.AppliesTo.Contains(Ra2SectionKind.Global) ||
                definition.AppliesTo.Contains(Ra2SectionKind.Unknown))
            {
                return true;
            }

            if (definition.AppliesTo.Contains(Ra2SectionKind.Unit) &&
                sectionKind is Ra2SectionKind.Infantry or Ra2SectionKind.Vehicle or Ra2SectionKind.Aircraft)
            {
                return true;
            }

            return definition.AppliesTo.Contains(Ra2SectionKind.Techno) &&
                   sectionKind is Ra2SectionKind.Infantry or
                       Ra2SectionKind.Vehicle or
                       Ra2SectionKind.Aircraft or
                       Ra2SectionKind.Building or
                       Ra2SectionKind.Unit;
        }
    }
}
