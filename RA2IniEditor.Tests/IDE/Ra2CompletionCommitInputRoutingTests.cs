using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionCommitInputRoutingTests
{
    [Fact]
    public void ShellWindow_RoutesSourceEditorAndTextAreaKeysThroughSingleHandler()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("SourceTextEditor.TextArea.PreviewKeyDown += SourceTextEditorTextArea_OnPreviewKeyDown", shellCode);
        Assert.Contains("SourceTextEditorTextArea_OnPreviewKeyDown", shellCode);
        Assert.Contains("HandleCompletionPreviewKeyDown(e)", shellCode);
        Assert.Contains("GetActualKey(e)", shellCode);
        Assert.Contains("key is Key.Enter or Key.Tab", shellCode);
        Assert.Contains("TryCommitSelectedCompletionOrClose();", shellCode);
        Assert.Contains("GetSelectedCompletionItemOrFirst", shellCode);
        Assert.Contains("_completionDropdownViewModel.Items[0]", shellCode);
        Assert.Contains("e.Handled = true;", shellCode);
    }

    [Fact]
    public void DropdownKeyboardRoutesCommitAndCloseRequestsToShell()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string dropdownCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2CompletionDropdownView.xaml.cs"));

        Assert.Contains("CompletionDropdownView.CompletionCommitRequested +=", shellCode);
        Assert.Contains("CompletionDropdownView.CompletionCloseRequested +=", shellCode);
        Assert.Contains("CompletionDropdownView_OnCompletionCommitRequested", shellCode);
        Assert.Contains("CompletionDropdownView_OnCompletionCloseRequested", shellCode);
        Assert.Contains("e.Key is Key.Enter or Key.Tab", dropdownCode);
        Assert.Contains("CompletionCommitRequested?.Invoke", dropdownCode);
        Assert.Contains("CompletionCloseRequested?.Invoke", dropdownCode);
    }

    [Fact]
    public void SelectedIndexBinding_IsTwoWayWithPropertyChanged()
    {
        string root = TestRepositoryRoot.Find();
        string dropdownXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language", "Ra2CompletionDropdownView.xaml"));
        string viewModelText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "Language", "Ra2CompletionDropdownViewModel.cs"));

        Assert.Contains("SelectedIndex=\"{Binding SelectedIndex, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}", dropdownXaml);
        Assert.Contains("public int SelectedIndex", viewModelText);
        Assert.Contains("set", viewModelText);
    }
}

