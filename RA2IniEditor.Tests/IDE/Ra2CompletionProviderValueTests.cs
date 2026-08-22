using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionProviderValueTests
{
    [Fact]
    public void GetCompletions_ArmorEmptyValueReturnsEnumValues()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Armor=
            """;

        Ra2CompletionResult result = GetCompletions(text, text.IndexOf("Armor=", StringComparison.Ordinal) + "Armor=".Length);

        Assert.Contains(result.Items, item => item.Label == "heavy" && item.InsertText == "heavy");
        Assert.All(result.Items, item => Assert.Equal(Ra2CompletionItemKind.Value, item.Kind));
        Assert.Equal(0, result.ReplacementSpan.Length);
    }

    [Fact]
    public void GetCompletions_ArmorPrefixReplacesOnlyPrefixWithRawValue()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Armor=he
            """;
        int caretOffset = text.IndexOf("Armor=he", StringComparison.Ordinal) + "Armor=he".Length;

        Ra2CompletionResult result = GetCompletions(text, caretOffset);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("heavy", item.Label);
        Assert.Equal("heavy", item.InsertText);
        Assert.Equal("he", Slice(text, result.ReplacementSpan));
    }

    [Fact]
    public void GetCompletions_BooleanFieldReturnsYesNo()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Crusher=
            """;
        int caretOffset = text.IndexOf("Crusher=", StringComparison.Ordinal) + "Crusher=".Length;

        Ra2CompletionResult result = GetCompletions(text, caretOffset);

        Assert.Equal(["no", "yes"], result.Items.Select(item => item.InsertText).Order(StringComparer.OrdinalIgnoreCase));
        Assert.All(result.Items, item => Assert.Equal("Type: Boolean", item.Detail));
    }

    [Fact]
    public void GetCompletions_MultiSelectFieldReplacesOnlyCurrentToken()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Owner=Americans,Bri
            """;
        int caretOffset = text.IndexOf("Owner=Americans,Bri", StringComparison.Ordinal) + "Owner=Americans,Bri".Length;

        Ra2CompletionResult result = GetCompletions(text, caretOffset);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("British", item.InsertText);
        Assert.Equal("Bri", Slice(text, result.ReplacementSpan));
        Assert.DoesNotContain(result.Items, candidate => candidate.InsertText == "Americans");
    }

    [Fact]
    public void GetCompletions_MultiSelectFieldPreservesWhitespaceBeforeCurrentToken()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Owner=Americans, Bri
            """;
        int caretOffset = text.IndexOf("Owner=Americans, Bri", StringComparison.Ordinal) + "Owner=Americans, Bri".Length;

        Ra2CompletionResult result = GetCompletions(text, caretOffset);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("British", item.InsertText);
        Assert.Equal("Bri", Slice(text, result.ReplacementSpan));
    }

    [Fact]
    public void GetCompletions_MultiSelectEmptyTokenAfterCommaReturnsCandidates()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Owner=Americans,
            """;
        int caretOffset = text.IndexOf("Owner=Americans,", StringComparison.Ordinal) + "Owner=Americans,".Length;

        Ra2CompletionResult result = GetCompletions(text, caretOffset);

        Assert.Contains(result.Items, item => item.InsertText == "British");
        Assert.DoesNotContain(result.Items, item => item.InsertText == "Americans");
        Assert.Equal(0, result.ReplacementSpan.Length);
    }

    [Fact]
    public void GetCompletions_UnknownKeyDoesNotReturnValueCompletion()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            UnknownField=he
            """;
        int caretOffset = text.IndexOf("UnknownField=he", StringComparison.Ordinal) + "UnknownField=he".Length;

        Ra2CompletionResult result = GetCompletions(text, caretOffset);

        Assert.Empty(result.Items);
    }

    [Fact]
    public void GetCompletions_InlineCommentDoesNotReturnValueCompletion()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Armor=he ; comment
            """;
        int caretOffset = text.IndexOf("comment", StringComparison.Ordinal) + 2;

        Ra2CompletionResult result = GetCompletions(text, caretOffset);

        Assert.Empty(result.Items);
    }

    [Fact]
    public void GetCompletions_ReferenceCompletionStillTakesPriority()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=

            [120mm]
            Damage=90
            """;

        Ra2CompletionResult result = GetCompletions(text, text.IndexOf("Primary=", StringComparison.Ordinal) + "Primary=".Length);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal(Ra2CompletionItemKind.Reference, item.Kind);
        Assert.Equal("120mm", item.InsertText);
    }

    [Fact]
    public void GetCompletions_FieldRegistryMetadataKeepsLocalValuesAndBackfillsBuiltInKnownValues()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Armor=co
            """;
        int caretOffset = text.IndexOf("Armor=co", StringComparison.Ordinal) + "Armor=co".Length;
        IRa2FieldDefinitionProvider fieldProvider = new SingleFieldProvider(new Ra2FieldDefinition(
            "Armor",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.Enum,
            Ra2FieldSourceKind.User,
            valueMetadata: new Ra2FieldValueMetadata(
                Ra2FieldValueKind.Enum,
                allowedValues:
                [
                    new Ra2FieldAllowedValue("composite", "Composite armor", "From local field registry.")
                ])));

        Ra2CompletionResult result = GetCompletions(text, caretOffset, fieldProvider);

        Assert.Equal(["composite", "concrete"], result.Items.Select(item => item.Label).Order(StringComparer.OrdinalIgnoreCase));
        Ra2CompletionItem localItem = Assert.Single(result.Items, item => item.Label == "composite");
        Assert.Equal("composite", localItem.InsertText);
        Assert.Equal("Type: Composite armor", localItem.Detail);
        Assert.Equal("From local field registry.", localItem.Documentation);
        Assert.Equal(Ra2CompletionItemSourceKind.FieldRegistry, localItem.SourceKind);

        Ra2CompletionItem builtInItem = Assert.Single(result.Items, item => item.Label == "concrete");
        Assert.Equal(Ra2CompletionItemSourceKind.BuiltInValueCatalog, builtInItem.SourceKind);
    }

    [Fact]
    public void GetCompletions_FieldRegistryMetadataListOnlyReplacesCurrentToken()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Owner=Americans,Yu
            """;
        int caretOffset = text.IndexOf("Owner=Americans,Yu", StringComparison.Ordinal) + "Owner=Americans,Yu".Length;
        IRa2FieldDefinitionProvider fieldProvider = new SingleFieldProvider(new Ra2FieldDefinition(
            "Owner",
            [Ra2SectionKind.Infantry],
            FieldEditorKind.MultiSelect,
            Ra2FieldSourceKind.User,
            valueMetadata: new Ra2FieldValueMetadata(
                Ra2FieldValueKind.EnumList,
                allowedValues:
                [
                    new Ra2FieldAllowedValue("Americans"),
                    new Ra2FieldAllowedValue("YuriCountry", "Yuri country")
                ])));

        Ra2CompletionResult result = GetCompletions(text, caretOffset, fieldProvider);

        Ra2CompletionItem item = Assert.Single(result.Items);
        Assert.Equal("YuriCountry", item.InsertText);
        Assert.Equal("Yu", Slice(text, result.ReplacementSpan));
        Assert.DoesNotContain(result.Items, candidate => candidate.InsertText == "Americans");
    }

    private static Ra2CompletionResult GetCompletions(
        string text,
        int caretOffset,
        IRa2FieldDefinitionProvider? fieldProvider = null)
    {
        fieldProvider ??= new BuiltInRa2FieldDefinitionProvider();
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

    private sealed class SingleFieldProvider : IRa2FieldDefinitionProvider
    {
        private readonly Ra2FieldDefinition _definition;

        public SingleFieldProvider(Ra2FieldDefinition definition)
        {
            _definition = definition;
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = null!;
            if (!_definition.AppliesTo.Contains(sectionKind) &&
                !_definition.AppliesTo.Contains(Ra2SectionKind.Unknown) &&
                !_definition.AppliesTo.Contains(Ra2SectionKind.Global))
            {
                return false;
            }

            if (!string.Equals(_definition.Key, key, StringComparison.OrdinalIgnoreCase))
                return false;

            definition = _definition;
            return true;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => _definition.AppliesTo.Contains(sectionKind) ? [_definition] : [];

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => TryGetField(sectionKind, key, out _);
    }
}
