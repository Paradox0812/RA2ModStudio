using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditingBoundaryGuardrailTests
{
    [Fact]
    public void EditingContracts_DoNotDependOnAvalonEditUiSaveDirtyOrLegacyServices()
    {
        string root = TestRepositoryRoot.Find();
        string editingRoot = Path.Combine(root, "RA2IniEditor.IDE", "Editing");
        string combinedText = string.Join(
            Environment.NewLine,
            Directory.GetFiles(editingRoot, "*.cs").Select(File.ReadAllText));

        Assert.DoesNotContain("AvalonEdit", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextEditor", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Document.Replace", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextArea.Document.Insert", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CompletionWindow", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShellWindow_StartsReadonlyButCommitsCompletionOnlyThroughEditableSession()
    {
        string root = TestRepositoryRoot.Find();
        string shellXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string completionController = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Controllers",
            "Completion",
            "Ra2CompletionInteractionController.cs"));
        string combinedText = shellXaml + Environment.NewLine + shellCode + Environment.NewLine + completionController;

        Assert.Contains("IsReadOnly=\"True\"", shellXaml);
        Assert.Contains("Ra2CompletionCommitPlanner", combinedText);
        Assert.Contains("IRa2CompletionCommitCoordinator", combinedText);
        Assert.Contains("Completion commit skipped: no editable file is currently open.", combinedText);
        Assert.DoesNotContain("Document.Replace", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextArea.Document.Insert", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CompletionWindow", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
    }
}

