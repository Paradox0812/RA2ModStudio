using RA2IniEditor.Core;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.AI;
using RA2IniEditor.IDE.Language;
using RA2IniEditor.IDE.ViewModels;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AiContextProviderTests
{
    [Fact]
    public void BuildContext_OnKeyValueLineResolvesSectionKeyAndValue()
    {
        const string text = "[HTNK]\nStrength=400\nPrimary=120mm";
        Ra2AiContext context = Build(text, text.IndexOf("400", StringComparison.Ordinal));

        Assert.True(context.HasSemanticContext);
        Assert.Equal("rulesmd.ini", context.DocumentDisplayName);
        Assert.Equal("HTNK", context.SectionName);
        Assert.Equal(Ra2SectionKind.Unknown.ToString(), context.SectionKind);
        Assert.Equal("Strength", context.KeyName);
        Assert.Equal("400", context.ValueText);
        Assert.Equal(2, context.LineNumber);
        Assert.Equal(Ra2CaretRegion.Value, context.CaretRegion);
    }

    [Fact]
    public void BuildContext_IncludesBoundedNearbyLines()
    {
        string text = string.Join("\n", Enumerable.Range(1, 21).Select(index => $"Line{index}=Value{index}"));
        Ra2AiContext context = Build(text, text.IndexOf("Line11", StringComparison.Ordinal), nearbyLineRadius: 2);

        Assert.Equal(5, context.NearbyLineCount);
        Assert.Contains("Line9=Value9", context.NearbyText);
        Assert.Contains("Line13=Value13", context.NearbyText);
        Assert.DoesNotContain("Line1=Value1", context.NearbyText);
        Assert.DoesNotContain("Line21=Value21", context.NearbyText);
    }

    [Fact]
    public void BuildContext_DoesNotIncludeEntireLargeFile()
    {
        string text = string.Join("\n", Enumerable.Range(1, 100).Select(index => $"Key{index}=Value{index}"));
        Ra2AiContext context = Build(text, text.IndexOf("Key50", StringComparison.Ordinal));

        Assert.True(context.NearbyLineCount <= 11);
        Assert.DoesNotContain("Key1=Value1", context.NearbyText);
        Assert.DoesNotContain("Key100=Value100", context.NearbyText);
    }

    [Fact]
    public void BuildContext_IncludesSelectedTextOnlyWhenProvided()
    {
        const string text = "[HTNK]\nStrength=400\nPrimary=120mm";

        Ra2AiContext withoutSelection = Build(text, text.IndexOf("Strength", StringComparison.Ordinal));
        Ra2AiContext withSelection = Build(text, text.IndexOf("Strength", StringComparison.Ordinal), selectedText: "Strength=400");

        Assert.False(withoutSelection.HasExplicitSelection);
        Assert.Null(withoutSelection.SelectedText);
        Assert.True(withSelection.HasExplicitSelection);
        Assert.Equal("Strength=400", withSelection.SelectedText);
    }

    [Fact]
    public void BuildContext_OnCommentLineFallsBackWithoutKeyValue()
    {
        const string text = "[HTNK]\n; comment\nStrength=400";
        Ra2AiContext context = Build(text, text.IndexOf("comment", StringComparison.Ordinal));

        Assert.Equal("HTNK", context.SectionName);
        Assert.Null(context.KeyName);
        Assert.Null(context.ValueText);
        Assert.Equal(Ra2CaretRegion.Comment, context.CaretRegion);
    }

    [Fact]
    public void BuildContext_OnBlankLineFallsBackWithoutKeyValue()
    {
        const string text = "[HTNK]\n   \nStrength=400";
        Ra2AiContext context = Build(text, text.IndexOf("   ", StringComparison.Ordinal) + 1);

        Assert.Equal("HTNK", context.SectionName);
        Assert.Null(context.KeyName);
        Assert.Null(context.ValueText);
        Assert.Equal(Ra2CaretRegion.Whitespace, context.CaretRegion);
    }

    [Fact]
    public void BuildContext_WithoutSemanticModelReturnsSafeFallback()
    {
        Ra2CurrentDocumentAiContextProvider provider = new();

        Ra2AiContext context = provider.BuildContext(new Ra2AiContextRequest(
            "rulesmd.ini",
            semanticModel: null,
            caretOffset: 200,
            selectedText: "Selected"));

        Assert.False(context.HasSemanticContext);
        Assert.Equal("rulesmd.ini", context.DocumentDisplayName);
        Assert.Equal(200, context.CaretOffset);
        Assert.Equal(0, context.LineNumber);
        Assert.Equal("Selected", context.SelectedText);
        Assert.Equal(string.Empty, context.NearbyText);
    }

    [Fact]
    public void BuildContext_IncludesFieldEvidenceForCurrentKey()
    {
        const string text = "[HTNK]\nStrength=400\nPrimary=120mm";
        Ra2FieldDefinition strength = new(
            "Strength",
            [Ra2SectionKind.Unknown],
            FieldEditorKind.Integer,
            Ra2FieldSourceKind.BuiltIn,
            "Object hit points.");

        Ra2AiContext context = Build(
            text,
            text.IndexOf("Strength", StringComparison.Ordinal),
            fieldProvider: new SingleFieldDefinitionProvider(strength));

        Assert.Equal(1, context.FieldEvidenceCount);
        Assert.Equal("Strength", Assert.Single(context.FieldEvidence).Key);
        Assert.Equal("Strength", context.FieldEvidenceTopKeysText);
    }

    [Fact]
    public void BuildContext_FieldEvidenceCountIsZeroWhenKeyAndPromptDoNotMatch()
    {
        const string text = "[HTNK]\nUnknownField=400\nPrimary=120mm";

        Ra2AiContext context = Build(
            text,
            text.IndexOf("UnknownField", StringComparison.Ordinal),
            promptText: "unmatched prompt",
            fieldProvider: new SingleFieldDefinitionProvider(new Ra2FieldDefinition(
                "Strength",
                [Ra2SectionKind.Unknown],
                FieldEditorKind.Integer,
                Ra2FieldSourceKind.BuiltIn,
                "Object hit points.")));

        Assert.Equal(0, context.FieldEvidenceCount);
    }

    [Fact]
    public void BuildContext_FieldEvidenceUsesPromptWithoutExpandingNearbyText()
    {
        string text = string.Join("\n", Enumerable.Range(1, 40).Select(index => $"Key{index}=Value{index}"));
        Ra2AiContext context = Build(
            text,
            text.IndexOf("Key20", StringComparison.Ordinal),
            nearbyLineRadius: 1,
            promptText: "armor",
            fieldProvider: new SingleFieldDefinitionProvider(new Ra2FieldDefinition(
                "Armor",
                [Ra2SectionKind.Unknown],
                FieldEditorKind.Enum,
                Ra2FieldSourceKind.BuiltIn,
                "Armor type.")));

        Assert.Equal(1, context.FieldEvidenceCount);
        Assert.Equal("Armor", Assert.Single(context.FieldEvidence).Key);
        Assert.Equal(3, context.NearbyLineCount);
        Assert.DoesNotContain("Key1=Value1", context.NearbyText);
        Assert.DoesNotContain("Key40=Value40", context.NearbyText);
    }

    [Fact]
    public void BuildContext_IncludesBoundedDiagnosticSummaries()
    {
        const string text = "[HTNK]\nStrength=400\nPrimary=120mm";
        Ra2AiContext context = Build(
            text,
            text.IndexOf("Strength", StringComparison.Ordinal),
            diagnosticIssues:
            [
                CreateIssue("CURRENT_LINE", IniIssueSeverity.Warning, line: 2, section: "HTNK", key: "Strength"),
                CreateIssue("CURRENT_SECTION", IniIssueSeverity.Warning, line: 3, section: "HTNK", key: "Primary")
            ]);

        Assert.Equal(2, context.DiagnosticCount);
        Assert.Equal("CURRENT_LINE", context.Diagnostics[0].Code);
        Assert.Equal("current line", context.Diagnostics[0].MatchReason);
    }

    [Fact]
    public void BuildContext_DiagnosticsRemainBoundedAndFieldEvidenceStillWorks()
    {
        const string text = "[HTNK]\nStrength=400\nPrimary=120mm";
        Ra2FieldDefinition strength = new(
            "Strength",
            [Ra2SectionKind.Unknown],
            FieldEditorKind.Integer,
            Ra2FieldSourceKind.BuiltIn,
            "Object hit points.");

        Ra2AiContext context = Build(
            text,
            text.IndexOf("Strength", StringComparison.Ordinal),
            fieldProvider: new SingleFieldDefinitionProvider(strength),
            diagnosticIssues: Enumerable.Range(1, 20)
                .Select(index => CreateIssue($"ISSUE_{index:00}", IniIssueSeverity.Warning, index, section: "HTNK", key: $"Key{index}"))
                .ToArray(),
            maxDiagnosticCount: 5);

        Assert.Equal(1, context.FieldEvidenceCount);
        Assert.Equal("Strength", Assert.Single(context.FieldEvidence).Key);
        Assert.Equal(5, context.DiagnosticCount);
    }

    [Fact]
    public void BuildContext_DiagnosticsDoNotExpandNearbyTextOrImplicitSelection()
    {
        string text = string.Join("\n", Enumerable.Range(1, 40).Select(index => $"Key{index}=Value{index}"));
        Ra2AiContext context = Build(
            text,
            text.IndexOf("Key20", StringComparison.Ordinal),
            nearbyLineRadius: 1,
            diagnosticIssues:
            [
                CreateIssue("CURRENT_LINE", IniIssueSeverity.Warning, line: 20, section: null, key: null)
            ]);

        Assert.Equal(1, context.DiagnosticCount);
        Assert.Equal(3, context.NearbyLineCount);
        Assert.False(context.HasExplicitSelection);
        Assert.DoesNotContain("Key1=Value1", context.NearbyText);
        Assert.DoesNotContain("Key40=Value40", context.NearbyText);
    }

    private static Ra2AiContext Build(
        string text,
        int caretOffset,
        string? selectedText = null,
        int nearbyLineRadius = 5,
        string? promptText = null,
        IRa2FieldDefinitionProvider? fieldProvider = null,
        IReadOnlyList<IdeDiagnosticIssueViewModel>? diagnosticIssues = null,
        int maxDiagnosticCount = Ra2CurrentFileAiDiagnosticSummaryProvider.DefaultMaxDiagnosticCount)
    {
        fieldProvider ??= new EmptyFieldDefinitionProvider();
        Ra2DocumentSemanticModel model = new Ra2DocumentSemanticModelBuilder().Build(
            new Ra2DocumentSnapshot("rulesmd.ini", text, 1),
            fieldProvider);
        return new Ra2CurrentDocumentAiContextProvider().BuildContext(new Ra2AiContextRequest(
            "rulesmd.ini",
            model,
            caretOffset,
            selectedText,
            nearbyLineRadius,
            promptText: promptText,
            fieldDefinitionProvider: fieldProvider,
            diagnosticIssues: diagnosticIssues,
            documentFilePath: "rulesmd.ini",
            documentVersion: 1,
            maxDiagnosticCount: maxDiagnosticCount));
    }

    private static IdeDiagnosticIssueViewModel CreateIssue(
        string code,
        IniIssueSeverity severity,
        int? line,
        string? section = null,
        string? key = null)
        => new(
            code,
            "DiagnosticService",
            severity,
            $"{code} message",
            "rulesmd.ini",
            line,
            columnNumber: null,
            section,
            key,
            version: 1);

    private sealed class EmptyFieldDefinitionProvider : IRa2FieldDefinitionProvider
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

    private sealed class SingleFieldDefinitionProvider : IRa2FieldDefinitionProvider
    {
        private readonly Ra2FieldDefinition _definition;

        public SingleFieldDefinitionProvider(Ra2FieldDefinition definition)
        {
            _definition = definition;
        }

        public bool TryGetField(Ra2SectionKind sectionKind, string key, out Ra2FieldDefinition definition)
        {
            definition = string.Equals(_definition.Key, key, StringComparison.OrdinalIgnoreCase)
                ? _definition
                : null!;
            return definition is not null;
        }

        public IReadOnlyList<Ra2FieldDefinition> GetFields(Ra2SectionKind sectionKind)
            => [_definition];

        public bool IsKnownField(Ra2SectionKind sectionKind, string key)
            => TryGetField(sectionKind, key, out _);
    }
}
