using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2EditorSessionControllerBoundaryTests
{
    [Fact]
    public void Controller_DoesNotDependOnWpfAvalonEditSaveChainOrTextChangedPath()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Controllers",
            "EditorSession",
            "Ra2EditorSessionController.cs"));

        Assert.DoesNotContain("System.Windows", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AvalonEdit", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TextEditor", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Window", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectSaveService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IniFileService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("File.WriteAllText", source, StringComparison.Ordinal);
        Assert.DoesNotContain("File.WriteAllBytes", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Completion", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AddProperty", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Hover", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_isSynchronizingEditorText", source, StringComparison.Ordinal);
        Assert.Contains("_sessionService.UpdateText", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_DelegatesEnterAndRevertDecisionButKeepsAvalonEditGlue()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Views",
            "ShellWindow.xaml.cs"));

        Assert.Contains("IRa2EditorSessionController", source, StringComparison.Ordinal);
        Assert.Contains("_editorSessionController.EnterEditMode", source, StringComparison.Ordinal);
        Assert.Contains("_editorSessionController.Revert", source, StringComparison.Ordinal);
        Assert.Contains("_editorSessionController.UpdateTextFromUser", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_editableSessionService.StartEditing", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_editableSessionService.Revert", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_editableSessionService.UpdateText", source, StringComparison.Ordinal);
        Assert.Contains("SourceTextEditor.Document.Text", source, StringComparison.Ordinal);
        Assert.Contains("SourceTextEditor.IsReadOnly", source, StringComparison.Ordinal);
        Assert.Contains("_isSynchronizingEditorText", source, StringComparison.Ordinal);
        Assert.Contains("SetEditorTextFromProgram", source, StringComparison.Ordinal);
    }
}

