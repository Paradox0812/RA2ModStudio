using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2ShellWindowSourceEditorBoundaryTests
{
    [Fact]
    public void ShellWindow_StillOwnsSourceEditorProgrammaticSyncGuard()
    {
        string source = ReadShellWindowSource();

        Assert.Contains("SetEditorTextFromProgram", source, StringComparison.Ordinal);
        Assert.Contains("_isSynchronizingEditorText = true", source, StringComparison.Ordinal);
        Assert.Contains("_isSynchronizingEditorText = false", source, StringComparison.Ordinal);
        Assert.Contains("finally", source, StringComparison.Ordinal);
        Assert.Contains("SourceTextEditor.Document.Text = text", source, StringComparison.Ordinal);
        Assert.Contains("SourceTextEditor.TextArea.Caret.Offset", source, StringComparison.Ordinal);
        Assert.Contains("SourceTextEditor.IsReadOnly", source, StringComparison.Ordinal);
        Assert.Contains("RestoreSourceEditorFocusAtCaret", source, StringComparison.Ordinal);
        Assert.Contains("CloseCompletionDropdown", source, StringComparison.Ordinal);
        Assert.Contains("CloseSourceEditorHoverToolTip", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_SavePathOnlyUsesCurrentFileSaveService()
    {
        string source = ReadShellWindowSource();

        Assert.DoesNotContain("Ra2SourceEditorController", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectSaveService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IniFileService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("File.WriteAllText", source, StringComparison.Ordinal);
        Assert.DoesNotContain("File.WriteAllBytes", source, StringComparison.Ordinal);
        Assert.DoesNotContain("WriteText", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveAll", source, StringComparison.Ordinal);
        Assert.Contains("IRa2SaveCurrentFileService", source, StringComparison.Ordinal);
        Assert.Contains("_saveCurrentFileService.Save", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_CurrentFileReplaceUsesStaleGateAndExistingSemanticUndoWithoutWritingDisk()
    {
        string source = ReadShellWindowSource();
        int previewStart = source.IndexOf(
            "private void SearchToolView_OnReplacePreviewRequested",
            StringComparison.Ordinal);
        int applyStart = source.IndexOf(
            "private void SearchToolView_OnReplaceApplyRequested",
            StringComparison.Ordinal);
        int nextMethod = source.IndexOf(
            "private LayoutAnchorable[] GetBottomTools",
            applyStart,
            StringComparison.Ordinal);

        Assert.True(previewStart >= 0);
        Assert.True(applyStart > previewStart);
        Assert.True(nextMethod > applyStart);
        string replaceSection = source[previewStart..nextMethod];
        Assert.Contains("_currentFileReplacePlanner.Plan", replaceSection, StringComparison.Ordinal);
        Assert.Contains("plan.IsCurrentFor(_editableSession)", replaceSection, StringComparison.Ordinal);
        Assert.Contains("plan.OriginalText", replaceSection, StringComparison.Ordinal);
        Assert.Contains("CreateProgrammaticSemanticUndoState", replaceSection, StringComparison.Ordinal);
        Assert.Contains("ApplyProgrammaticSemanticText", replaceSection, StringComparison.Ordinal);
        Assert.DoesNotContain("Save(", replaceSection, StringComparison.Ordinal);
        Assert.DoesNotContain("WriteText", replaceSection, StringComparison.Ordinal);
        Assert.DoesNotContain("File.Write", replaceSection, StringComparison.Ordinal);
    }

    private static string ReadShellWindowSource()
        => File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Views",
            "ShellWindow.xaml.cs"));
}

