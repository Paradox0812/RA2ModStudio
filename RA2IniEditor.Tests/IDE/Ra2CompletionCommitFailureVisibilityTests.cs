using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionCommitFailureVisibilityTests
{
    [Fact]
    public void ShellWindow_ReportsSkippedAndFailedCompletionCommitReasons()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string completionController = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Controllers",
            "Completion",
            "Ra2CompletionInteractionController.cs"));
        string combinedText = shellCode + Environment.NewLine + completionController;

        Assert.Contains("Completion commit skipped: no editable file is currently open.", combinedText);
        Assert.Contains("Completion commit skipped: completion result is unavailable.", combinedText);
        Assert.Contains("Completion commit skipped: no completion item is selected.", combinedText);
        Assert.Contains("Completion commit failed:", combinedText);
        Assert.Contains("ShowCompletionCommitStatus", shellCode);
        Assert.Contains("EditorSaveHintTextBlock.Text = message", shellCode);
    }

    [Fact]
    public void CompletionCommitFix_DoesNotIntroduceSaveDiskOrCompletionWindow()
    {
        string root = TestRepositoryRoot.Find();
        string combinedText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs")) +
                              Environment.NewLine +
                              File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Controllers", "Completion", "Ra2CompletionInteractionController.cs")) +
                              Environment.NewLine +
                              File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml")) +
                              Environment.NewLine +
                              File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2CompletionDropdownView.xaml.cs")) +
                              Environment.NewLine +
                              File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2CompletionDropdownView.xaml"));

        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniFileService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllBytes", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CompletionWindow", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SaveAll", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CompletionPopupPositioning_DoesNotAssignCaretOffset()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.DoesNotContain("SourceTextEditor.TextArea.Caret.Offset = normalizedCaretOffset", shellCode);
        Assert.DoesNotContain("TextArea.Caret.Offset = normalizedCaretOffset", shellCode);
        Assert.Contains("caretRectangle.Left - textView.ScrollOffset.X", shellCode);
        Assert.Contains("caretRectangle.Bottom - textView.ScrollOffset.Y", shellCode);
        Assert.Contains("textView.TransformToAncestor(SourceTextEditor).Transform(caretBottom)", shellCode);
        Assert.DoesNotContain("SourceTextEditor.TextArea.TranslatePoint(caretBottom, SourceTextEditor)", shellCode);
    }

    [Fact]
    public void ShowCompletionDropdown_AssignsLastCompletionResultAfterPositioning()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        int methodIndex = shellCode.IndexOf("private void ShowCompletionDropdown(Ra2CompletionResult result)", StringComparison.Ordinal);
        int positionIndex = shellCode.IndexOf("TryGetCompletionPopupPosition", methodIndex, StringComparison.Ordinal);
        int assignmentIndex = shellCode.IndexOf("_lastCompletionResult = result", methodIndex, StringComparison.Ordinal);
        int openIndex = shellCode.IndexOf("CompletionDropdownPopup.IsOpen = true", methodIndex, StringComparison.Ordinal);

        Assert.True(methodIndex >= 0);
        Assert.True(positionIndex > methodIndex);
        Assert.True(assignmentIndex > positionIndex);
        Assert.True(openIndex > assignmentIndex);
    }

    [Fact]
    public void CloseCompletionDropdown_AllowsPreservingCompletionResult()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("CloseCompletionDropdown(bool clearCompletionResult = true)", shellCode);
        Assert.Contains("if (clearCompletionResult)", shellCode);
    }
}

