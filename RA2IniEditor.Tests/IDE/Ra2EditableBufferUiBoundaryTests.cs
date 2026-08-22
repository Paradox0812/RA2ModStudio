using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditableBufferUiBoundaryTests
{
    private static readonly string ShellWindowXamlPath = Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        "RA2IniEditor.IDE",
        "Views",
        "ShellWindow.xaml");

    private static readonly string ShellWindowCodeBehindPath = Path.Combine(
        AppContext.BaseDirectory,
        "..",
        "..",
        "..",
        "..",
        "RA2IniEditor.IDE",
        "Views",
        "ShellWindow.xaml.cs");

    [Fact]
    public void ShellWindow_ExposesExplicitEditModeControls()
    {
        string xaml = File.ReadAllText(ShellWindowXamlPath);

        Assert.Contains("Shell.SourceEditor.EnterEditModeButton", xaml);
        Assert.Contains("Shell.SourceEditor.SaveCurrentFileButton", xaml);
        Assert.Contains("Shell.SourceEditor.RevertInMemoryChangesButton", xaml);
        Assert.Contains("Shell.SourceEditor.EditorStateText", xaml);
        Assert.Contains("Visibility=\"Collapsed\"", xaml);
        Assert.DoesNotContain("编辑状态：只读预览", xaml);
    }

    [Fact]
    public void ShellWindow_SourceEditorDefaultsToReadonlyAndTracksTextChanges()
    {
        string xaml = File.ReadAllText(ShellWindowXamlPath);

        Assert.Contains("IsReadOnly=\"True\"", xaml);
        Assert.Contains("TextChanged=\"SourceTextEditor_OnTextChanged\"", xaml);
    }

    [Fact]
    public void ShellWindow_SaveUsesCurrentFileServiceWithoutLegacyProjectServices()
    {
        string codeBehind = File.ReadAllText(ShellWindowCodeBehindPath);

        Assert.DoesNotContain("ProjectSaveService", codeBehind);
        Assert.DoesNotContain("ProjectLoader", codeBehind);
        Assert.DoesNotContain("ObjectAggregator", codeBehind);
        Assert.DoesNotContain("SaveAll", codeBehind);
        Assert.DoesNotContain("File.WriteAllText", codeBehind);
        Assert.DoesNotContain("File.WriteAllBytes", codeBehind);
        Assert.DoesNotContain("WriteText", codeBehind);
        Assert.Contains("IRa2SaveCurrentFileService", codeBehind);
        Assert.Contains("_saveCurrentFileService.Save", codeBehind);
        Assert.Contains("KeyGesture(Key.S, ModifierKeys.Control)", codeBehind);
    }

    [Fact]
    public void ShellWindow_CompletionDropdownStillDoesNotCommitText()
    {
        string codeBehind = File.ReadAllText(ShellWindowCodeBehindPath);

        Assert.Contains("Ra2CompletionCommitPlanner", codeBehind);
        Assert.Contains("Ra2TextChangeApplier", codeBehind);
        Assert.Contains("TryCommitSelectedCompletionOrClose", codeBehind);
        Assert.Contains("key is Key.Enter or Key.Tab", codeBehind);
        Assert.Contains("if (_editableSession is null)", codeBehind);
    }
}
