using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionBoundaryTests
{
    [Fact]
    public void CompletionProvider_IsPureLanguageLayerWithoutUiSaveDirtyNetworkOrLegacyDependencies()
    {
        string root = TestRepositoryRoot.Find();
        string providerPath = Path.Combine(root, "RA2IniEditor.IDE", "Language", "Ra2CompletionProvider.cs");
        string providerText = File.ReadAllText(providerPath);

        Assert.DoesNotContain("AvalonEdit", providerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Window", providerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextChanged", providerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HttpClient", providerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GitHub", providerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", providerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", providerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", providerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.", providerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Directory.", providerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Dirty", providerText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ValueCompletionCandidates_AreCentralizedOutsideCompletionProvider()
    {
        string root = TestRepositoryRoot.Find();
        string providerText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Language", "Ra2CompletionProvider.cs"));
        string catalogText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Language", "BuiltInRa2FieldValueCompletionCatalog.cs"));
        string compositeText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Language", "CompositeRa2FieldValueCompletionCatalog.cs"));

        Assert.Contains("IRa2FieldValueCompletionCatalog", providerText);
        Assert.Contains("BuiltInRa2FieldValueCompletionCatalog", providerText);
        Assert.Contains("CompositeRa2FieldValueCompletionCatalog", providerText);
        Assert.DoesNotContain("\"Armor\"", providerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"Crusher\"", providerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"Owner\"", providerText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"Armor\"", catalogText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"Crusher\"", catalogText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"Owner\"", catalogText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("candidate.Priority > existing.Priority", compositeText);
    }

    [Fact]
    public void ValueCompletionCatalogs_DoNotReferenceUiSaveDirtyNetworkOrLegacyDependencies()
    {
        string root = TestRepositoryRoot.Find();
        string languageRoot = Path.Combine(root, "RA2IniEditor.IDE", "Language");
        string combinedCatalogText = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(languageRoot, "*ValueCompletion*.cs")
                .Select(File.ReadAllText));

        Assert.DoesNotContain("AvalonEdit", combinedCatalogText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Window", combinedCatalogText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HttpClient", combinedCatalogText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GitHub", combinedCatalogText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedCatalogText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedCatalogText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", combinedCatalogText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.", combinedCatalogText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Directory.", combinedCatalogText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Dirty", combinedCatalogText, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("; Str", "Str")]
    [InlineData("[NEWINF]\nPrimary=120mm ; Str", "Str")]
    [InlineData("[NEWINF] ; Str\nPrimary=120mm", "Str")]
    public void GetCompletions_CommentContextsReturnEmpty(string text, string token)
    {
        Ra2CompletionResult result = GetCompletions(text, text.IndexOf(token, StringComparison.Ordinal) + 1);

        Assert.Empty(result.Items);
    }

    [Fact]
    public void GetCompletions_CommaSecondTokenDoesNotTriggerReferenceCompletion()
    {
        const string text = """
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary=120mm,

            [120mm]
            Damage=90
            """;

        Ra2CompletionResult result = GetCompletions(text, text.IndexOf("Primary=120mm,", StringComparison.Ordinal) + "Primary=120mm,".Length);

        Assert.Empty(result.Items);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("+1")]
    [InlineData("1.5")]
    [InlineData("0.5")]
    [InlineData(".5")]
    public void GetCompletions_NumericLiteralDoesNotTriggerReferenceCompletion(string literal)
    {
        string text = $$"""
            [InfantryTypes]
            0=NEWINF

            [NEWINF]
            Primary={{literal}}

            [120mm]
            Damage=90
            """;

        Ra2CompletionResult result = GetCompletions(text, text.IndexOf(literal, StringComparison.Ordinal) + literal.Length);

        Assert.Empty(result.Items);
    }

    [Fact]
    public void EmptyResult_UsesCaretOffsetZeroLengthReplacementSpan()
    {
        const string text = "; comment";
        int caretOffset = text.Length;

        Ra2CompletionResult result = GetCompletions(text, caretOffset);

        Assert.Empty(result.Items);
        Assert.Equal(caretOffset, result.ReplacementSpan.Start);
        Assert.Equal(0, result.ReplacementSpan.Length);
    }

    private static Ra2CompletionResult GetCompletions(string text, int caretOffset)
    {
        EmptyFieldProvider fieldProvider = new();
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

