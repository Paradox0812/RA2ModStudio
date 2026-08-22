using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionPreviewUiBoundaryTests
{
    [Fact]
    public void CompletionPreviewViewModels_DoNotDependOnEditorUiSaveDirtyOrLegacyServices()
    {
        string root = TestRepositoryRoot.Find();
        string viewModelRoot = Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "Language");
        string combinedText = string.Join(
            Environment.NewLine,
            Directory.GetFiles(viewModelRoot, "Ra2Completion*ViewModel.cs").Select(File.ReadAllText));

        Assert.DoesNotContain("AvalonEdit", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextEditor", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Document.Replace", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextArea.Document.Insert", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Dirty", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CompletionWindow", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CompletionPreviewWindow_DefinesAutomationIdsWithoutModalOrAvalonEditCompletionWindow()
    {
        string root = TestRepositoryRoot.Find();
        string windowRoot = Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language");
        string combinedText = File.ReadAllText(Path.Combine(windowRoot, "Ra2CompletionPreviewWindow.xaml")) +
                              Environment.NewLine +
                              File.ReadAllText(Path.Combine(windowRoot, "Ra2CompletionPreviewWindow.xaml.cs"));

        Assert.Contains("Ra2CompletionPreview.Window", combinedText);
        Assert.Contains("Ra2CompletionPreview.ItemsGrid", combinedText);
        Assert.Contains("Ra2CompletionPreview.CountText", combinedText);
        Assert.Contains("Ra2CompletionPreview.StatusText", combinedText);
        Assert.Contains("Ra2CompletionPreview.ReplacementText", combinedText);
        Assert.DoesNotContain("_completionPreviewWindow.ShowDialog", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CompletionWindow", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Document.Replace", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextArea.Document.Insert", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Document.Insert", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Save", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Dirty", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShellWindow_CompletionPreviewEntryDoesNotCommitTextOrUseAvalonEditCompletionWindow()
    {
        string root = TestRepositoryRoot.Find();
        string shellXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string completionController = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Controllers",
            "Completion",
            "Ra2CompletionInteractionController.cs"));
        string combinedText = shellXaml + Environment.NewLine + shellCode + Environment.NewLine + completionController;

        Assert.Contains("Shell.SourceEditor.ShowCompletionPreviewMenuItem", shellXaml);
        Assert.Contains("ShowCompletionPreview_OnClick", shellXaml);
        Assert.Contains("Ra2CompletionProvider", combinedText);
        Assert.Contains("Ra2CompletionRequest", combinedText);
        Assert.Contains("_completionInteractionController.OpenCompletions", shellCode);
        Assert.Contains("CompletionDropdownPopup", shellXaml);
        Assert.Contains("Ra2CompletionDropdownView", shellXaml);
        Assert.Contains("ShowCompletionDropdownAtCaret", shellCode);
        Assert.Contains("Ra2CompletionDropdownViewModel", shellCode);
        Assert.Contains("CompletionDropdownPopup.IsOpen = true", shellCode);
        Assert.DoesNotContain("CompletionWindow", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Document.Replace", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextArea.Document.Insert", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Document.Insert", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("_completionPreviewWindow.ShowDialog", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Ra2CompletionCommitPlanner", combinedText);
        Assert.Contains("Ra2TextChangeApplier", combinedText);
        Assert.Contains("TryCommitSelectedCompletionOrClose", combinedText);
        Assert.DoesNotContain("Undo", completionController, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Redo", completionController, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShellWindow_AutoCompletionUsesDebouncedEditModeOnlyTrigger()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string textChangedMethod = ExtractMethod(shellCode, "SourceTextEditor_OnTextChanged");
        string canAutoTriggerMethod = ExtractExpressionMethod(shellCode, "CanAutoTriggerCompletion");
        string timerMethod = ExtractMethod(shellCode, "SourceEditorCompletionAutoTriggerTimer_OnTick");

        Assert.Contains("private const int SourceEditorCompletionAutoTriggerDelayMilliseconds = 220;", shellCode);
        Assert.Contains("private readonly DispatcherTimer _sourceEditorCompletionAutoTriggerTimer;", shellCode);
        Assert.Contains("SourceEditorCompletionAutoTriggerTimer_OnTick", shellCode);
        Assert.Contains("ScheduleCompletionAutoTrigger();", textChangedMethod);
        Assert.Contains("_editableSession is not null", canAutoTriggerMethod);
        Assert.Contains("!SourceTextEditor.IsReadOnly", canAutoTriggerMethod);
        Assert.Contains("!CompletionDropdownPopup.IsOpen", canAutoTriggerMethod);
        Assert.Contains("SourceTextEditor.IsKeyboardFocusWithin", canAutoTriggerMethod);
        Assert.Contains("TryShowCompletionDropdownAtCaret(showOutputMessage: false)", timerMethod);
        Assert.Contains("StopCompletionAutoTrigger();", ExtractMethod(shellCode, "SetEditorTextFromProgram"));
        Assert.Contains("StopCompletionAutoTrigger();", ExtractMethod(shellCode, "ResetEditableSessionToReadOnly"));
    }

    [Fact]
    public void ShellWindow_AutoCompletionKeepsManualTriggerAndDoesNotWriteOutputOnAutoOpen()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string keyDownMethod = ExtractMethod(shellCode, "HandleCompletionPreviewKeyDown");
        string showMethod = ExtractExpressionMethod(shellCode, "TryShowCompletionDropdownAtCaret");

        Assert.Contains("Keyboard.Modifiers == ModifierKeys.Control && key == Key.Space", keyDownMethod);
        Assert.Contains("ShowCompletionDropdownAtCaret();", keyDownMethod);
        Assert.Contains("private void ShowCompletionDropdownAtCaret()", shellCode);
        Assert.Contains("TryShowCompletionDropdownAtCaret(showOutputMessage: true)", shellCode);
        Assert.Contains("if (showOutputMessage)", showMethod);
        Assert.Contains("viewModel.ShowOutputMessage(result.Message)", showMethod);
        Assert.DoesNotContain("ProjectSaveService", showMethod, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SaveCurrentFile", showMethod, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Document.Replace", showMethod, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextArea.Document.Insert", showMethod, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CompletionDropdownView_DefinesAutomationIdsWithoutTextCommitApis()
    {
        string root = TestRepositoryRoot.Find();
        string windowRoot = Path.Combine(root, "RA2IniEditor.IDE", "Views", "Language");
        string combinedText = File.ReadAllText(Path.Combine(windowRoot, "Ra2CompletionDropdownView.xaml")) +
                              Environment.NewLine +
                              File.ReadAllText(Path.Combine(windowRoot, "Ra2CompletionDropdownView.xaml.cs"));

        Assert.Contains("Ra2CompletionDropdown.View", combinedText);
        Assert.Contains("Ra2CompletionDropdown.ItemsList", combinedText);
        Assert.DoesNotContain("Ra2CompletionDropdown.CountText", combinedText);
        Assert.DoesNotContain("Ra2CompletionDropdown.ReplacementText", combinedText);
        Assert.DoesNotContain("Ra2CompletionDropdown.StatusText", combinedText);
        Assert.Contains("Style=\"{StaticResource IdeAssistPopupFrameStyle}\"", combinedText);
        Assert.Contains("Style=\"{StaticResource IdeAssistCompletionListStyle}\"", combinedText);
        Assert.Contains("ItemContainerStyle=\"{StaticResource IdeAssistCompletionItemStyle}\"", combinedText);
        Assert.DoesNotContain("MinHeight=\"42\"", combinedText);
        Assert.Contains("Width=\"520\"", combinedText);
        Assert.Contains("MaxHeight=\"220\"", combinedText);
        Assert.DoesNotContain("IdeInfoCardTightStyle", combinedText);
        Assert.DoesNotContain("IdeInspectorBadgeStyle", combinedText);
        Assert.Contains("<ColumnDefinition Width=\"132\" />", combinedText);
        Assert.Contains("<ColumnDefinition Width=\"76\" />", combinedText);
        Assert.Contains("<ColumnDefinition Width=\"*\" />", combinedText);
        Assert.Contains("<ColumnDefinition Width=\"92\" />", combinedText);
        Assert.Contains("AutomationProperties.AutomationId=\"Ra2CompletionDropdown.Header\"", combinedText);
        Assert.Contains("Style=\"{StaticResource IdeAssistBadgeStyle}\"", combinedText);
        Assert.DoesNotMatch("#[0-9A-Fa-f]{6,8}", combinedText);
        Assert.DoesNotContain("Text=\"{Binding Kind}\"", combinedText);
        Assert.Contains("Text=\"{Binding Label}\"", combinedText);
        Assert.Contains("Text=\"{Binding TypeDisplay}\"", combinedText);
        Assert.Contains("Text=\"{Binding SourceDisplayText}\"", combinedText);
        Assert.Contains("Text=\"{Binding AnnotationText}\"", combinedText);
        Assert.DoesNotContain("CompletionWindow", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Document.Replace", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextArea.Document.Insert", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Document.Insert", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    private static string ExtractMethod(string source, string methodName)
    {
        int start = source.IndexOf($"private void {methodName}", StringComparison.Ordinal);
        if (start < 0)
            throw new InvalidOperationException($"Method '{methodName}' was not found.");

        int nextMethod = source.IndexOf("\n    private ", start + methodName.Length, StringComparison.Ordinal);
        if (nextMethod < 0)
            nextMethod = source.Length;

        return source[start..nextMethod];
    }

    private static string ExtractExpressionMethod(string source, string methodName)
    {
        int start = source.IndexOf($"private bool {methodName}", StringComparison.Ordinal);
        if (start < 0)
            throw new InvalidOperationException($"Method '{methodName}' was not found.");

        int nextMethod = source.IndexOf("\n    private ", start + methodName.Length, StringComparison.Ordinal);
        if (nextMethod < 0)
            nextMethod = source.Length;

        return source[start..nextMethod];
    }
}
