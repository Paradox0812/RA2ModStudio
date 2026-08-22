using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionCommitUiBoundaryTests
{
    [Fact]
    public void ShellWindow_UsesCoordinatorForEditableCompletionCommit()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string completionController = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Controllers",
            "Completion",
            "Ra2CompletionInteractionController.cs"));
        string dropdownXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2CompletionDropdownView.xaml"));
        string dropdownCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2CompletionDropdownView.xaml.cs"));
        string combinedText = shellCode + Environment.NewLine + completionController;

        Assert.Contains("IRa2CompletionCommitCoordinator", combinedText);
        Assert.Contains("_completionInteractionController.TryCommit", shellCode);
        Assert.Contains("TryCommitSelectedCompletionOrClose", shellCode);
        Assert.Contains("TryCommitCompletionItemOrClose", shellCode);
        Assert.Contains("SetEditorTextFromProgram(", shellCode);
        Assert.Contains("result.CaretOffset", shellCode);
        Assert.Contains("RestoreSourceEditorFocusAtCaret(result.CaretOffset)", shellCode);
        Assert.Contains("CompletionItemDoubleClicked", dropdownCode);
        Assert.Contains("MouseDoubleClick=\"ItemsList_OnMouseDoubleClick\"", dropdownXaml);
    }

    [Fact]
    public void ShellWindow_RestoresAvalonEditFocusAfterCompletionCommit()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("private void RestoreSourceEditorFocusAtCaret(int caretOffset)", shellCode);
        Assert.Contains("DispatcherPriority.ContextIdle", shellCode);
        Assert.Contains("SourceTextEditor.Focus();", shellCode);
        Assert.Contains("SourceTextEditor.TextArea.Focus();", shellCode);
        Assert.Contains("Keyboard.Focus(SourceTextEditor.TextArea);", shellCode);
        Assert.Contains("SourceTextEditor.TextArea.Caret.Offset = normalizedOffset", shellCode);
    }

    [Fact]
    public void ShellWindow_NoEditableSessionCompletionCommitClosesDropdownWithoutTextApply()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string completionController = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Controllers",
            "Completion",
            "Ra2CompletionInteractionController.cs"));

        Assert.Contains("Completion commit skipped: no editable file is currently open.", completionController);
        Assert.Contains("CloseCompletionDropdown();", shellCode);
        Assert.Contains("return;", shellCode);
    }

    [Fact]
    public void CompletionCommitUi_DoesNotReferenceSaveDiskOrAvalonEditCompletionWindow()
    {
        string root = TestRepositoryRoot.Find();
        string combinedText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs")) +
                              Environment.NewLine +
                              File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Controllers", "Completion", "Ra2CompletionInteractionController.cs")) +
                              Environment.NewLine +
                              File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2CompletionDropdownView.xaml")) +
                              Environment.NewLine +
                              File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2CompletionDropdownView.xaml.cs"));

        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniFileService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllBytes", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CompletionWindow", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
    }
}

