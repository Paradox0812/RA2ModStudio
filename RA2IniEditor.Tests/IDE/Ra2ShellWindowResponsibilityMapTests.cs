using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2ShellWindowResponsibilityMapTests
{
    [Fact]
    public void ResponsibilityMap_DocumentsControllerCandidatesAndGuardrails()
    {
        string document = File.ReadAllText(GetResponsibilityMapPath());

        Assert.Contains("Ra2SourceEditorController", document);
        Assert.Contains("Ra2EditorSessionController", document);
        Assert.Contains("Ra2CompletionInteractionController", document);
        Assert.Contains("Ra2SourceEditorHoverController", document);
        Assert.Contains("Ra2LanguageNavigationController", document);
        Assert.Contains("Ra2FieldBrowserController", document);
        Assert.Contains("Ra2ProjectShellController", document);
        Assert.Contains("Ra2FieldRegistryManagerController", document);
        Assert.Contains("Do not add Save Current File", document);
        Assert.Contains("ProjectSaveService", document);
        Assert.Contains("This version should only add documentation and guardrail tests", document);
    }

    [Fact]
    public void ResponsibilityMap_DocumentsExtractionOrder()
    {
        string document = File.ReadAllText(GetResponsibilityMapPath());

        AssertInOrder(
            document,
            "1. `Ra2LanguageNavigationController`",
            "2. `Ra2SourceEditorHoverController`",
            "3. `Ra2CompletionInteractionController`",
            "4. `Ra2FieldBrowserController`",
            "5. `Ra2FieldRegistryManagerController`",
            "6. `Ra2ProjectShellController`",
            "7. `Ra2EditorSessionController`",
            "8. `Ra2SourceEditorController`");
    }

    [Fact]
    public void ShellWindow_DoesNotGainSaveChainDependenciesDuringMapWork()
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

    [Fact]
    public void ShellWindow_StillOwnsExistingFeatureHandlersUntilExtraction()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("AddProperty_OnClick", shellCode);
        Assert.Contains("HandleCompletionPreviewKeyDown", shellCode);
        Assert.Contains("TryCommitCompletionItemOrClose", shellCode);
        Assert.Contains("TryShowSourceEditorHoverAtOffset", shellCode);
        Assert.Contains("GoToDefinition_OnClick", shellCode);
        Assert.Contains("EnterEditMode_OnClick", shellCode);
        Assert.Contains("RevertInMemoryChanges_OnClick", shellCode);
    }

    private static void AssertInOrder(string text, params string[] expectedFragments)
    {
        int previousIndex = -1;
        foreach (string fragment in expectedFragments)
        {
            int index = text.IndexOf(fragment, StringComparison.Ordinal);
            Assert.True(index > previousIndex, $"Expected '{fragment}' after index {previousIndex}, actual index {index}.");
            previousIndex = index;
        }
    }

    private static string GetResponsibilityMapPath()
        => Path.Combine(
            TestRepositoryRoot.Find(),
            "Docs",
            "RA2IniEditor_IDE_ShellWindow_Responsibility_Map_v0.4.48.md");
}

