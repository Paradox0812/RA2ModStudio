using System.Diagnostics;
using System.Text;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Language;
using Xunit;
using Xunit.Abstractions;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AutomationDependencyConeCharacterizationTests
{
    private readonly ITestOutputHelper _output;

    public Ra2AutomationDependencyConeCharacterizationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static readonly string[] QueryFoundationSources =
    [
        Path.Combine("RA2IniEditor.IDE", "Classification", "IRa2SectionClassifier.cs"),
        Path.Combine("RA2IniEditor.IDE", "Classification", "Ra2SectionClassificationResult.cs"),
        Path.Combine("RA2IniEditor.IDE", "Classification", "Ra2SectionClassificationWarning.cs"),
        Path.Combine("RA2IniEditor.IDE", "Classification", "Ra2SectionClassifier.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "IRa2CaretContextService.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "IRa2DocumentSemanticModelBuilder.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "IRa2ReferenceFinder.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "Ra2CaretContext.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "Ra2CaretContextService.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "Ra2CaretRegion.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "Ra2DocumentSemanticModel.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "Ra2DocumentSemanticModelBuilder.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "Ra2DocumentSnapshot.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "Ra2IniLineParser.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "Ra2KeyValueSymbol.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "Ra2ReferenceFinder.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "Ra2ReferenceItem.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "Ra2ReferenceResult.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "Ra2SectionSymbol.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "Ra2TextSpan.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "Ra2ValueReferenceKind.cs"),
        Path.Combine("RA2IniEditor.IDE", "Language", "Ra2ValueReferenceSymbol.cs")
    ];

    [Fact]
    public void QueryFoundationCandidateSources_AreUiNeutralAndExcludeFullTextModelParser()
    {
        string root = TestRepositoryRoot.Find();
        string[] forbiddenTokens =
        [
            "System.Windows",
            "ICSharpCode.AvalonEdit",
            "RA2IniEditor.IDE.ViewModels",
            "RA2IniEditor.IDE.Diagnostics",
            "RA2IniEditor.IDE.Editing",
            "RA2IniEditor.IDE.Services",
            "RA2IniEditor.Infrastructure",
            "FieldRegistryRuntimeService",
            "File.Read",
            "File.Write",
            "Directory.",
            "Environment.",
            "Process.",
            "Clipboard",
            "Dispatcher"
        ];

        Assert.DoesNotContain(
            Path.Combine("RA2IniEditor.IDE", "TextModel", "Ra2IniTextDocumentParser.cs"),
            QueryFoundationSources);

        foreach (string relativePath in QueryFoundationSources)
        {
            string path = Path.Combine(root, relativePath);
            Assert.True(File.Exists(path), $"Missing characterized source: {relativePath}");
            string source = File.ReadAllText(path);
            foreach (string token in forbiddenTokens)
                Assert.DoesNotContain(token, source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void SemanticModel_DuplicateSectionOccurrencesRemainInSourceOrder()
    {
        const string text = "[E1]\nStrength=100\n[E1]\nStrength=125\n";
        Ra2DocumentSemanticModel model = BuildModel(text);

        Ra2SectionSymbol[] occurrences = model.Sections
            .Where(section => string.Equals(section.Name, "E1", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.Equal(2, occurrences.Length);
        Assert.True(occurrences[0].HeaderSpan.Start < occurrences[1].HeaderSpan.Start);
        Assert.Same(occurrences[0], model.FindSectionByName("E1"));
    }

    [Fact]
    public void ReferenceFinder_DistinguishesResolvedTargetWithNoUsagesFromUnresolvedCaret()
    {
        const string text = "[Unused]\nValue=1\n[E1]\nPrimary=Gun\n";
        Ra2DocumentSemanticModel model = BuildModel(text);
        Ra2CaretContextService caretContextService = new();
        Ra2ReferenceFinder finder = new();

        int headerOffset = text.IndexOf("Unused", StringComparison.Ordinal);
        Ra2ReferenceResult resolvedEmpty = finder.FindReferences(
            model,
            caretContextService.GetContext(model, headerOffset));

        int keyOffset = text.IndexOf("Primary", StringComparison.Ordinal);
        Ra2ReferenceResult unresolved = finder.FindReferences(
            model,
            caretContextService.GetContext(model, keyOffset));

        Assert.Equal("Unused", resolvedEmpty.TargetName);
        Assert.Empty(resolvedEmpty.Items);
        Assert.Equal(string.Empty, unresolved.TargetName);
        Assert.Empty(unresolved.Items);
    }

    [Fact]
    public void LaterSlices_CurrentDiagnosticsAndHostSnapshotExposeTheKnownExtractionSeams()
    {
        string root = TestRepositoryRoot.Find();
        string diagnosticSource = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Diagnostics",
            "CurrentFileReadonlyDiagnosticService.cs"));
        string hostSnapshotSource = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Editing",
            "Ra2AuthoringSnapshot.cs"));

        Assert.Contains("RA2IniEditor.IDE.ViewModels", diagnosticSource, StringComparison.Ordinal);
        Assert.Contains("IdeDiagnosticIssueViewModel", diagnosticSource, StringComparison.Ordinal);
        Assert.Contains("RA2IniEditor.IDE.Services", hostSnapshotSource, StringComparison.Ordinal);
        Assert.Contains("Ra2EditableDocumentSession", hostSnapshotSource, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(7)]
    public void QueryFoundation_LargeDocumentBuildIsDeterministicAndDoesNotMutateInput(
        int approximateMegabytes)
    {
        string text = BuildLargeDocument(approximateMegabytes);
        Stopwatch stopwatch = Stopwatch.StartNew();

        Ra2DocumentSemanticModel first = BuildModel(text);
        Ra2DocumentSemanticModel second = BuildModel(text);

        stopwatch.Stop();
        _output.WriteLine(
            "{0} MiB two-pass characterization: {1} ms; chars={2}; sections={3}; keys={4}; refs={5}",
            approximateMegabytes,
            stopwatch.ElapsedMilliseconds,
            text.Length,
            first.Sections.Count,
            first.KeyValues.Count,
            first.References.Count);
        Assert.Equal(text, first.Snapshot.Text);
        Assert.Equal(text, second.Snapshot.Text);
        Assert.Equal(
            first.Sections.Select(section => (section.Name, section.HeaderSpan)),
            second.Sections.Select(section => (section.Name, section.HeaderSpan)));
        Assert.Equal(
            first.KeyValues.Select(keyValue => (keyValue.SectionName, keyValue.Key, keyValue.LineSpan)),
            second.KeyValues.Select(keyValue => (keyValue.SectionName, keyValue.Key, keyValue.LineSpan)));
        Assert.Equal(
            first.References.Select(reference => (reference.TargetSectionName, reference.ValueSpan)),
            second.References.Select(reference => (reference.TargetSectionName, reference.ValueSpan)));
    }

    private static Ra2DocumentSemanticModel BuildModel(string text)
        => new Ra2DocumentSemanticModelBuilder().Build(
            new Ra2DocumentSnapshot("rulesmd.ini", text, version: 1),
            new BuiltInRa2FieldDefinitionProvider());

    private static string BuildLargeDocument(int approximateMegabytes)
    {
        int targetLength = approximateMegabytes * 1024 * 1024;
        StringBuilder builder = new(targetLength + 64);
        builder.AppendLine("[InfantryTypes]");
        builder.AppendLine("0=E1");
        builder.AppendLine("[E1]");
        builder.AppendLine("Primary=Rifle");
        builder.AppendLine("[Rifle]");
        builder.AppendLine("Projectile=InvisibleLow");
        const string filler = "; query dependency cone characterization 0123456789\n";
        while (builder.Length < targetLength)
            builder.Append(filler);

        return builder.ToString();
    }
}
