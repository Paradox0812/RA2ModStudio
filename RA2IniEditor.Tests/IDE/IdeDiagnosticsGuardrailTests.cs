using System.Xml.Linq;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class IdeDiagnosticsGuardrailTests
{
    [Fact]
    public void IdeProject_DoesNotReferenceLegacyApplicationProject()
    {
        string root = TestRepositoryRoot.Find();
        string ideProjectPath = Path.Combine(root, "RA2IniEditor.IDE", "RA2IniEditor.IDE.csproj");
        XDocument project = XDocument.Load(ideProjectPath);

        string[] projectReferences = project.Descendants()
            .Where(element => element.Name.LocalName == "ProjectReference")
            .Select(element => element.Attribute("Include")?.Value ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(projectReferences, reference => reference.EndsWith("RA2IniEditor.csproj", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void IdeProductionSources_DoNotReferenceLegacyAnalysisProjectServicesOrSavePipeline()
    {
        string root = TestRepositoryRoot.Find();
        string ideRoot = Path.Combine(root, "RA2IniEditor.IDE");
        string combinedText = string.Join(
            Environment.NewLine,
            EnumerateProductionFiles(ideRoot)
                .Where(path => !IsMinimalSaveCurrentFileSource(path))
                .Select(File.ReadAllText));

        Assert.DoesNotContain("using RA2IniEditor.Analysis", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("DiagnosticRuleRegistry", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectDiagnosticAnalyzer", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2FieldDefinitionDatabase", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2EditorMetadata", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2FieldOptionProvider", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2SchemaProvider", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("MainWindowViewModel", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("TextChanged=\"SourceTextEditor_OnTextChanged\"", combinedText);
    }

    [Fact]
    public void IdeSourceHighlighting_UsesCoreFieldProviderWithoutLegacyFieldDatabase()
    {
        string root = TestRepositoryRoot.Find();
        string shellWindowCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");
        string highlightingRoot = Path.Combine(root, "RA2IniEditor.IDE", "Highlighting");
        string highlightingText = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(highlightingRoot, "*.cs", SearchOption.TopDirectoryOnly).Select(File.ReadAllText));
        string runtimeServiceText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Services", "FieldRegistryRuntimeService.cs"));
        string combinedText = File.ReadAllText(shellWindowCodePath) + Environment.NewLine + highlightingText + Environment.NewLine + runtimeServiceText;

        Assert.Contains("BuiltInRa2FieldDefinitionProvider", combinedText);
        Assert.Contains("IRa2FieldDefinitionProvider", combinedText);
        Assert.Contains("CompositeRa2FieldDefinitionProvider", combinedText);
        Assert.Contains("FieldRegistryRuntimeService", combinedText);
        Assert.Contains("ReadonlyIniHighlightTokenizer", combinedText);
        Assert.Contains("Ra2KnownFieldHighlightingTransformer", combinedText);
        Assert.Contains("DocumentColorizingTransformer", combinedText);
        Assert.Contains("LineTransformers.Add", combinedText);
        Assert.DoesNotContain("Ra2FieldDefinitionDatabase", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2EditorMetadata", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2FieldOptionProvider", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2SchemaProvider", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("System.IO", highlightingText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.", highlightingText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Directory.", highlightingText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextChanged=", highlightingText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Dirty", highlightingText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void IdeSourceHighlightingTransformer_UsesTextCacheBeforeTokenizing()
    {
        string root = TestRepositoryRoot.Find();
        string transformerPath = Path.Combine(root, "RA2IniEditor.IDE", "Highlighting", "Ra2KnownFieldHighlightingTransformer.cs");
        string transformerText = File.ReadAllText(transformerPath);

        Assert.Contains("_cachedDocument", transformerText);
        Assert.Contains("_cachedVersion", transformerText);
        Assert.Contains("_cachedTokens", transformerText);
        Assert.Contains("Equals(_cachedVersion, version)", transformerText);
        Assert.Contains("return _cachedTokens", transformerText);
        Assert.Contains("_tokenizer.Tokenize(document.Text)", transformerText);
        Assert.DoesNotContain("string.Equals(_cachedText", transformerText, StringComparison.Ordinal);
        Assert.DoesNotContain("Document.Text =", transformerText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextChanged", transformerText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AvalonEdit_IsReferencedOnlyByIdeProject()
    {
        string root = TestRepositoryRoot.Find();
        string ideProjectText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "RA2IniEditor.IDE.csproj"));
        string coreProjectText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.Core", "RA2IniEditor.Core.csproj"));
        string infrastructureProjectText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.Infrastructure", "RA2IniEditor.Infrastructure.csproj"));
        string legacyProjectPath = Path.Combine(root, "RA2IniEditor.csproj");

        Assert.Contains("<PackageReference Include=\"AvalonEdit\" Version=\"6.3.0.90\" />", ideProjectText);
        Assert.DoesNotContain("AvalonEdit", coreProjectText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AvalonEdit", infrastructureProjectText, StringComparison.OrdinalIgnoreCase);
        if (File.Exists(legacyProjectPath))
        {
            string legacyProjectText = File.ReadAllText(legacyProjectPath);
            Assert.DoesNotContain("AvalonEdit", legacyProjectText, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            Assert.True(File.Exists(Path.Combine(root, "RA2IniEditor.IDE.sln")));
        }
    }

    [Fact]
    public void GitIgnore_ExcludesLocalIdeBuildTestAndUserArtifacts()
    {
        string root = TestRepositoryRoot.Find();
        string gitIgnorePath = Path.Combine(root, ".gitignore");
        string gitIgnoreText = File.ReadAllText(gitIgnorePath);

        Assert.Contains(".vs/", gitIgnoreText);
        Assert.Contains("bin/", gitIgnoreText);
        Assert.Contains("obj/", gitIgnoreText);
        Assert.Contains("TestResults/", gitIgnoreText);
        Assert.Contains("*.user", gitIgnoreText);
        Assert.Contains("*.suo", gitIgnoreText);
        Assert.Contains("*.vsidx", gitIgnoreText);
        Assert.Contains("*.DotSettings.user", gitIgnoreText);
    }

    [Fact]
    public void SourcePackageScript_ExcludesGeneratedArtifactsAndDoesNotBuildOrTest()
    {
        string root = TestRepositoryRoot.Find();
        string scriptPath = Path.Combine(root, "tools", "package-source.ps1");
        string scriptText = File.ReadAllText(scriptPath);

        Assert.Contains(".vs", scriptText);
        Assert.Contains("bin", scriptText);
        Assert.Contains("obj", scriptText);
        Assert.Contains("TestResults", scriptText);
        Assert.Contains("Compress-Archive", scriptText);
        Assert.DoesNotContain("dotnet build", scriptText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dotnet test", scriptText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DiagnosticsReadonlyContract_ContainsRequiredGuardrailConclusions()
    {
        string root = TestRepositoryRoot.Find();
        string contractPath = Path.Combine(root, "docs", "ide-diagnostics-readonly-contract-v0.4.9.md");
        if (!File.Exists(contractPath))
        {
            Assert.True(File.Exists(Path.Combine(root, "RA2IniEditor.IDE.sln")));
            return;
        }

        string contractText = File.ReadAllText(contractPath);

        Assert.Contains("L1", contractText);
        Assert.Contains("Current File Structure Diagnostics", contractText);
        Assert.Contains("RA2IniEditor.Core.IniParser", contractText);
        Assert.Contains("RA2IniEditor.Core.IniValidator", contractText);
        Assert.Contains("RA2IniEditor.Analysis", contractText);
        Assert.Contains("CurrentSourceSnapshot", contractText);
        Assert.Contains("SourceEditorState", contractText);
        Assert.Contains("Issues and Output", contractText);
        Assert.Contains("DeferredLargeFile", contractText);
        Assert.Contains("legacy root `Core/`", contractText);
    }

    [Fact]
    public void AvalonEditReadonlySyncDocument_DoesNotRequireXamlTextBinding()
    {
        string root = TestRepositoryRoot.Find();
        string spikeDocPath = Path.Combine(root, "docs", "ide-source-editor-avalonedit-spike-v0.4.13.md");
        string shellWindowPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml");
        string shellWindowCodePath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");

        string shellText = File.ReadAllText(shellWindowPath);
        string shellCodeText = File.ReadAllText(shellWindowCodePath);

        if (File.Exists(spikeDocPath))
        {
            string spikeDocText = File.ReadAllText(spikeDocPath);
            Assert.Contains("SourceEditorViewModel.Text -> ShellWindow code-behind -> SourceTextEditor.Document.Text", spikeDocText);
            Assert.Contains("AvalonEdit XAML `Text` binding is prohibited.", spikeDocText);
            Assert.Contains("TextChanged", spikeDocText);
            Assert.Contains("completion", spikeDocText, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            Assert.True(File.Exists(Path.Combine(root, "RA2IniEditor.IDE.sln")));
        }

        Assert.DoesNotContain("Text=\"{Binding SourceEditor.Text", shellText);
        Assert.Contains("SourceTextEditor.Document.Text = text", shellCodeText);
        Assert.Contains("nameof(SourceEditorViewModel.Text)", shellCodeText);
    }

    [Fact]
    public void DiagnosticsSmokeChecklist_CoversReliabilityHardeningScenarios()
    {
        string root = TestRepositoryRoot.Find();
        string checklistPath = Path.Combine(root, "docs", "ide-diagnostics-smoke-checklist-v0.4.11.md");
        if (!File.Exists(checklistPath))
        {
            Assert.True(File.Exists(Path.Combine(root, "RA2IniEditor.IDE.sln")));
            return;
        }

        string checklistText = File.ReadAllText(checklistPath);

        Assert.Contains("normal INI file with no structure issues", checklistText);
        Assert.Contains("known structure issue", checklistText);
        Assert.Contains("unknown or malformed line", checklistText);
        Assert.Contains("Double-click an issue that has a line number", checklistText);
        Assert.Contains("issue with no line number", checklistText);
        Assert.Contains("Stale results do not overwrite", checklistText);
        Assert.Contains("deferred large file", checklistText);
        Assert.Contains("file that cannot be read", checklistText);
        Assert.Contains("Source Editor is readonly", checklistText);
        Assert.Contains("SourceEditorViewModel.Text -> ShellWindow code-behind -> SourceTextEditor.Document.Text", checklistText);
        Assert.Contains("no AvalonEdit XAML `Text` binding", checklistText);
        Assert.DoesNotContain("Mode=OneWay", checklistText);
        Assert.Contains("Project Explorer section navigation", checklistText);
        Assert.Contains("Issues Tool Window", checklistText);
        Assert.Contains("Output and Issues responsibilities remain separate", checklistText);
        Assert.DoesNotContain("RA2IniEditor.Analysis", checklistText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", checklistText, StringComparison.OrdinalIgnoreCase);
    }

    private static IEnumerable<string> EnumerateProductionFiles(string ideRoot)
    {
        return Directory.EnumerateFiles(ideRoot, "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(".xaml", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsMinimalSaveCurrentFileSource(string path)
    {
        string fileName = Path.GetFileName(path);
        return fileName is "Ra2TextFirstFileWriter.cs"
            or "IRa2TextFirstFileWriter.cs"
            or "Ra2TextFileWriteResult.cs"
            or "Ra2SaveCurrentFileService.cs"
            or "IRa2SaveCurrentFileService.cs"
            or "Ra2SaveCurrentFileResult.cs";
    }
}

