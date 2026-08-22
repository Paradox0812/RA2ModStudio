using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionDropdownFocusTests
{
    [Fact]
    public void CompletionPopup_StaysOpenForExplicitShellControl()
    {
        string root = TestRepositoryRoot.Find();
        string shellXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));

        Assert.Contains("StaysOpen=\"True\"", shellXaml);
        Assert.DoesNotContain("StaysOpen=\"False\"", shellXaml);
    }

    [Fact]
    public void SourceEditorLostFocus_DoesNotCloseWhenFocusMovesInsideCompletionDropdown()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("IsFocusMovingInsideCompletionDropdown(e.NewFocus)", shellCode);
        Assert.Contains("CompletionDropdownView.IsKeyboardFocusWithin", shellCode);
        Assert.Contains("CompletionDropdownView.IsMouseOver", shellCode);
        Assert.Contains("CompletionDropdownView.IsAncestorOf", shellCode);
        Assert.DoesNotContain("VisualTreeHelper", shellCode);
    }

    [Fact]
    public void DropdownList_HandlesInternalMouseAndKeyboardRoutes()
    {
        string root = TestRepositoryRoot.Find();
        string dropdownXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2CompletionDropdownView.xaml"));
        string dropdownCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2CompletionDropdownView.xaml.cs"));

        Assert.Contains("x:Name=\"ItemsList\"", dropdownXaml);
        Assert.Contains("PreviewMouseDown=\"ItemsList_OnPreviewMouseDown\"", dropdownXaml);
        Assert.Contains("MouseDoubleClick=\"ItemsList_OnMouseDoubleClick\"", dropdownXaml);
        Assert.Contains("PreviewKeyDown=\"ItemsList_OnPreviewKeyDown\"", dropdownXaml);
        Assert.Contains("CompletionDropdownInteracted", dropdownCode);
        Assert.Contains("CompletionCommitRequested", dropdownCode);
        Assert.Contains("CompletionCloseRequested", dropdownCode);
    }
}

