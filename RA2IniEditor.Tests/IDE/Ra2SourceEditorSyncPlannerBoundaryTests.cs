using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SourceEditorSyncPlannerBoundaryTests
{
    [Fact]
    public void PlannerFiles_DoNotDependOnWpfAvalonEditOrSaveChain()
    {
        string source = ReadSourceEditorControllerFile("IRa2SourceEditorSyncPlanner.cs") +
            ReadSourceEditorControllerFile("Ra2SourceEditorSyncPlanner.cs");

        Assert.DoesNotContain("System.Windows", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AvalonEdit", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TextEditor", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Document.Text =", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectSaveService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IniFileService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("File.WriteAllText", source, StringComparison.Ordinal);
        Assert.DoesNotContain("File.WriteAllBytes", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_StillOwnsProgrammaticTextSyncAndGuard()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Views",
            "ShellWindow.xaml.cs"));

        Assert.Contains("SetEditorTextFromProgram", source, StringComparison.Ordinal);
        Assert.Contains("_isSynchronizingEditorText = true", source, StringComparison.Ordinal);
        Assert.Contains("_isSynchronizingEditorText = false", source, StringComparison.Ordinal);
        Assert.Contains("SourceTextEditor.Document.Text = text", source, StringComparison.Ordinal);
        Assert.Contains("SourceTextEditor.TextArea.Caret.Offset", source, StringComparison.Ordinal);
        Assert.Contains("RestoreSourceEditorFocusAtCaret", source, StringComparison.Ordinal);
    }

    private static string ReadSourceEditorControllerFile(string fileName)
        => File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Controllers",
            "SourceEditor",
            fileName));
}

