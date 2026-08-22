using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditorStateShellBoundaryTests
{
    [Fact]
    public void ShellWindow_UsesEditorStateFactoryForStatusAndHintText()
    {
        string root = TestRepositoryRoot.Find();
        string shellXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("Shell.SourceEditor.EditorStateText", shellXaml);
        Assert.Contains("Shell.SourceEditor.SaveHintText", shellXaml);
        Assert.Contains("IRa2EditorStateViewModelFactory", shellCode);
        Assert.Contains("_editorStateViewModelFactory.Create(_editableSession)", shellCode);
        Assert.Contains("editorState.StateText", shellCode);
        Assert.Contains("editorState.SaveHintText", shellCode);
        Assert.DoesNotContain("editorState.CanEnterEditMode", shellCode);
        Assert.Contains("editorState.HasSession", shellCode);
        Assert.Contains("editorState.CanRevertInMemoryChanges", shellCode);
    }

    [Fact]
    public void ShellWindow_SaveButtonAndCtrlSUseCurrentFileSaveServiceOnly()
    {
        string root = TestRepositoryRoot.Find();
        string shellText = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml")) +
                           Environment.NewLine +
                           File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("Shell.SourceEditor.SaveCurrentFileButton", shellText, StringComparison.Ordinal);
        Assert.Contains("ApplicationCommands.Save", shellText, StringComparison.Ordinal);
        Assert.Contains("KeyGesture(Key.S, ModifierKeys.Control)", shellText, StringComparison.Ordinal);
        Assert.Contains("IRa2SaveCurrentFileService", shellText, StringComparison.Ordinal);
        Assert.Contains("_saveCurrentFileService.Save", shellText, StringComparison.Ordinal);
        Assert.DoesNotContain("Save All", shellText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", shellText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", shellText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllText", shellText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllBytes", shellText, StringComparison.OrdinalIgnoreCase);
    }
}

