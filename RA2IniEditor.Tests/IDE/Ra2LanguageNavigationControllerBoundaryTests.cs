using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2LanguageNavigationControllerBoundaryTests
{
    [Fact]
    public void Controller_DoesNotDependOnWpfAvalonEditSaveChainOrObjectAggregator()
    {
        string source = File.ReadAllText(GetControllerPath());

        Assert.DoesNotContain("System.Windows", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AvalonEdit", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextEditor", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectSaveService", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniFileService", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllText", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllBytes", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Document.Replace", source, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", source, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShellWindow_DelegatesLanguageNavigationBusinessToController()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("_languageNavigationController.GoToDefinition", shellCode);
        Assert.Contains("_languageNavigationController.PeekDefinition", shellCode);
        Assert.Contains("_languageNavigationController.FindReferences", shellCode);
        Assert.Contains("TryBuildLanguageNavigationRequest", shellCode);
        Assert.Contains("ShowPeekDefinitionWindow", shellCode);
        Assert.Contains("ShowFindReferencesWindow", shellCode);
        Assert.Contains("TryScrollSourceEditorToLanguageTarget", shellCode);
        Assert.DoesNotContain("private readonly IRa2DefinitionProvider _definitionProvider", shellCode);
        Assert.DoesNotContain("private readonly IRa2ReferenceFinder _referenceFinder", shellCode);
        Assert.DoesNotContain("TryGetDefinitionAtCaret", shellCode);
    }

    [Fact]
    public void ShellWindow_DoesNotGainSaveChainDependenciesDuringLanguageExtraction()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string shellXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string combined = shellCode + Environment.NewLine + shellXaml;

        Assert.DoesNotContain("ProjectSaveService", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniFileService", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SaveAll", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllText", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllBytes", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText(", combined, StringComparison.OrdinalIgnoreCase);
    }

    private static string GetControllerPath()
        => Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Controllers",
            "Language",
            "Ra2LanguageNavigationController.cs");
}

