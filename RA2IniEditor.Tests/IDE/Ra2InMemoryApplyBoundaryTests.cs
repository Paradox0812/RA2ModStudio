using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2InMemoryApplyBoundaryTests
{
    [Fact]
    public void InMemoryApplySources_DoNotDependOnUiSaveProjectOrDiskServices()
    {
        string root = TestRepositoryRoot.Find();
        string editingRoot = Path.Combine(root, "RA2IniEditor.IDE", "Editing");
        string applicationEditingRoot = Path.Combine(root, "RA2IniEditor.Application", "Editing");
        string[] inMemoryApplyFiles =
        [
            Path.Combine(editingRoot, "IRa2TextChangeApplier.cs"),
            Path.Combine(applicationEditingRoot, "Ra2TextChange.cs"),
            Path.Combine(editingRoot, "Ra2TextChangeApplier.cs"),
            Path.Combine(editingRoot, "Ra2TextChangeApplyResult.cs"),
            Path.Combine(editingRoot, "IRa2CompletionCommitCoordinator.cs"),
            Path.Combine(editingRoot, "IRa2CompletionCommitPlanner.cs"),
            Path.Combine(editingRoot, "Ra2CompletionCommitApplyResult.cs"),
            Path.Combine(editingRoot, "Ra2CompletionCommitCoordinator.cs"),
            Path.Combine(editingRoot, "Ra2CompletionCommitPlanner.cs")
        ];
        string combinedText = string.Join(Environment.NewLine, inMemoryApplyFiles.Select(File.ReadAllText));

        Assert.DoesNotContain("AvalonEdit", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextEditor", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Document.Replace", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextArea.Document.Insert", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Directory.", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShellWindow_UsesTextChangeApplierOnlyForEditableCompletionCommit()
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
        Assert.Contains("Ra2TextChangeApplier", combinedText);
        Assert.Contains("IRa2CompletionCommitCoordinator", combinedText);
        Assert.Contains("Completion commit skipped: no editable file is currently open.", combinedText);
        Assert.DoesNotContain("Document.Replace", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextArea.Document.Insert", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
    }
}

