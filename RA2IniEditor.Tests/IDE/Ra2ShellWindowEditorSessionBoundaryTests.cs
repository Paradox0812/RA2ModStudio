using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2ShellWindowEditorSessionBoundaryTests
{
    [Fact]
    public void ShellWindow_StillOwnsAvalonEditUiGlueForEditorSession()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Views",
            "ShellWindow.xaml.cs"));

        Assert.Contains("EnterEditMode_OnClick", source, StringComparison.Ordinal);
        Assert.Contains("StartEditableSessionForCurrentSnapshot", source, StringComparison.Ordinal);
        Assert.Contains("RevertInMemoryChanges_OnClick", source, StringComparison.Ordinal);
        Assert.Contains("SourceTextEditor_OnTextChanged", source, StringComparison.Ordinal);
        Assert.Contains("_editorSessionController.UpdateTextFromUser", source, StringComparison.Ordinal);
        Assert.Contains("SetEditorTextFromProgram", source, StringComparison.Ordinal);
        Assert.Contains("_isSynchronizingEditorText", source, StringComparison.Ordinal);
        Assert.Contains("SourceTextEditor.Document.Text", source, StringComparison.Ordinal);
        Assert.Contains("SourceTextEditor.IsReadOnly", source, StringComparison.Ordinal);
        Assert.Contains("UpdateEditorStateControls", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_AutoStartsEditableSessionAndUsesDirtyNavigationDecisionDialog()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Views",
            "ShellWindow.xaml.cs"));

        Assert.Contains("StartEditableSessionForCurrentSnapshot(viewModel);", source, StringComparison.Ordinal);
        Assert.Contains("_editorSessionController.EnterEditMode", source, StringComparison.Ordinal);
        Assert.Contains("SourceTextEditor.IsReadOnly = !result.ShouldSetEditable", source, StringComparison.Ordinal);
        Assert.Contains("TryResolveDirtyNavigationBeforeLeavingCurrentFile", source, StringComparison.Ordinal);
        Assert.Contains("_dirtyNavigationDialogService.ShowDirtyNavigationDialog", source, StringComparison.Ordinal);
        Assert.Contains("TrySaveDirtyFileBeforeNavigation", source, StringComparison.Ordinal);
        Assert.Contains("TryDiscardDirtyFileBeforeNavigation", source, StringComparison.Ordinal);
        Assert.Contains("CancelDirtyNavigation", source, StringComparison.Ordinal);
        Assert.Contains("_editableSession?.DocumentState.IsDirty", source, StringComparison.Ordinal);
        Assert.Contains("RestoreProjectExplorerSelectionToCurrentFile(viewModel);", source, StringComparison.Ordinal);
        Assert.Contains("_isRestoringProjectExplorerSelection", source, StringComparison.Ordinal);
        Assert.Contains("SelectProjectExplorerItem(currentFileItem);", source, StringComparison.Ordinal);
        Assert.Contains("TryGetProjectExplorerTreeViewItem(item)", source, StringComparison.Ordinal);
        Assert.Contains("TryNavigateToProjectExplorerSectionAsync", source, StringComparison.Ordinal);
        Assert.Contains("await viewModel.LoadProjectExplorerFileAsync(", source, StringComparison.Ordinal);
        Assert.Contains("FindProjectExplorerFileItem(viewModel, section.FilePath)", source, StringComparison.Ordinal);
        Assert.Contains("FindProjectExplorerSectionItem", source, StringComparison.Ordinal);
        Assert.Contains("SelectProjectExplorerItem(matchingSection);", source, StringComparison.Ordinal);
        Assert.Contains("已取消导航，当前未保存修改仍保留。", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TryAllowNavigationThatDiscardsEditorText", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_SaveChainStaysBehindCurrentFileSaveService()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Views",
            "ShellWindow.xaml.cs"));

        Assert.DoesNotContain("ProjectSaveService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IniFileService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveAll", source, StringComparison.Ordinal);
        Assert.DoesNotContain("File.WriteAllText", source, StringComparison.Ordinal);
        Assert.DoesNotContain("File.WriteAllBytes", source, StringComparison.Ordinal);
        Assert.DoesNotContain("WriteText", source, StringComparison.Ordinal);
        Assert.Contains("IRa2SaveCurrentFileService", source, StringComparison.Ordinal);
        Assert.Contains("_saveCurrentFileService.Save", source, StringComparison.Ordinal);
        Assert.Contains("new Ra2SaveCurrentFilePlanRequest(_editableSession, SourceTextEditor.IsReadOnly)", source, StringComparison.Ordinal);
    }
}

