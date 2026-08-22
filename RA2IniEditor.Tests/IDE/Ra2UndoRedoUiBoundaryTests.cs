using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2UndoRedoUiBoundaryTests
{
    [Fact]
    public void ShellWindow_ExposesUndoRedoButtonsAndShortcutsWithoutTouchingSaveServices()
    {
        string root = TestRepositoryRoot.Find();
        string shellXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("Shell.SourceEditor.UndoButton", shellXaml, StringComparison.Ordinal);
        Assert.Contains("Shell.SourceEditor.RedoButton", shellXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"UndoCurrentFile_OnClick\"", shellXaml, StringComparison.Ordinal);
        Assert.Contains("Click=\"RedoCurrentFile_OnClick\"", shellXaml, StringComparison.Ordinal);
        Assert.Contains("ApplicationCommands.Undo", shellCode, StringComparison.Ordinal);
        Assert.Contains("ApplicationCommands.Redo", shellCode, StringComparison.Ordinal);
        Assert.Contains("new KeyGesture(Key.Z, ModifierKeys.Control)", shellCode, StringComparison.Ordinal);
        Assert.Contains("new KeyGesture(Key.Y, ModifierKeys.Control)", shellCode, StringComparison.Ordinal);
        Assert.Contains("IsUndoShortcut", shellCode, StringComparison.Ordinal);
        Assert.Contains("IsRedoShortcut", shellCode, StringComparison.Ordinal);

        string undoMethod = ExtractMethod(shellCode, "UndoCurrentFileFromShell");
        string redoMethod = ExtractMethod(shellCode, "RedoCurrentFileFromShell");
        Assert.Contains("SourceTextEditor.Undo();", undoMethod, StringComparison.Ordinal);
        Assert.Contains("SourceTextEditor.Redo();", redoMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("_saveCurrentFileService.Save", undoMethod + redoMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectSaveService", undoMethod + redoMethod, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllText", undoMethod + redoMethod, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllBytes", undoMethod + redoMethod, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShellWindow_RevertClearsAvalonEditUndoStackAndProgrammaticSemanticUndoState()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("ClearSourceEditorUndoStack", shellCode, StringComparison.Ordinal);
        Assert.Contains("SourceTextEditor.Document.UndoStack.ClearAll();", shellCode, StringComparison.Ordinal);
        Assert.Contains("_programmaticSemanticUndoState = null;", shellCode, StringComparison.Ordinal);
        Assert.Contains("SetEditorTextFromProgram(result.TextToSyncToEditor);", shellCode, StringComparison.Ordinal);
        Assert.Contains("ClearSourceEditorUndoStack();", ExtractMethod(shellCode, "RevertInMemoryChanges_OnClick"), StringComparison.Ordinal);
        Assert.Contains("ClearSourceEditorUndoStack();", ExtractMethod(shellCode, "TryDiscardDirtyFileBeforeNavigation"), StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_CompletionCommitUsesSemanticUndoBeforeFallingBackToAvalonEditUndo()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        string completionCommit = ExtractMethod(shellCode, "TryCommitCompletionItemOrClose");
        string undoMethod = ExtractMethod(shellCode, "UndoCurrentFileFromShell");
        string redoMethod = ExtractMethod(shellCode, "RedoCurrentFileFromShell");

        Assert.Contains("ProgrammaticSemanticUndoState", shellCode, StringComparison.Ordinal);
        Assert.Contains("CreateCompletionSemanticUndoState", shellCode, StringComparison.Ordinal);
        Assert.Contains("CreateProgrammaticSemanticUndoState", shellCode, StringComparison.Ordinal);
        Assert.Contains("ClearAvalonEditUndoStackOnly();", ExtractMethod(shellCode, "ApplyProgrammaticSemanticText"), StringComparison.Ordinal);
        Assert.Contains("textBeforeCommit", completionCommit, StringComparison.Ordinal);
        Assert.Contains("completionResultBeforeCommit", completionCommit, StringComparison.Ordinal);
        Assert.Contains("SetEditorTextFromProgram(", completionCommit, StringComparison.Ordinal);
        Assert.Contains("TryUndoProgrammaticSemanticChange()", undoMethod, StringComparison.Ordinal);
        Assert.Contains("TryRedoProgrammaticSemanticChange()", redoMethod, StringComparison.Ordinal);
        Assert.Contains("textBeforeCommit.Remove(span.Start, span.Length)", shellCode, StringComparison.Ordinal);
        Assert.Contains("SyncEditableSessionFromProgrammaticText", shellCode, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_ProgrammaticSemanticUndoRedoRefreshesCommandStateAfterStateChange()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        string undoSemanticMethod = ExtractMethod(shellCode, "TryUndoProgrammaticSemanticChange");
        string redoSemanticMethod = ExtractMethod(shellCode, "TryRedoProgrammaticSemanticChange");
        string applySemanticMethod = ExtractMethod(shellCode, "ApplyProgrammaticSemanticText");

        AssertInOrder(
            undoSemanticMethod,
            "_programmaticSemanticUndoState = state with { IsUndone = true };",
            "UpdateEditorStateControls();",
            "CommandManager.InvalidateRequerySuggested();");
        AssertInOrder(
            redoSemanticMethod,
            "_programmaticSemanticUndoState = state with { IsUndone = false };",
            "UpdateEditorStateControls();",
            "CommandManager.InvalidateRequerySuggested();");
        Assert.DoesNotContain("UpdateEditorStateControls();", applySemanticMethod, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_AddPropertyInsertAndReplaceRegisterProgrammaticSemanticUndo()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        string insertMethod = ExtractMethod(shellCode, "ApplyAddPropertyInsertDuplicate");
        string replaceMethod = ExtractMethod(shellCode, "ApplyAddPropertyReplaceExisting");
        string resultMethod = ExtractMethod(shellCode, "ApplyFieldBrowserActionResult");

        Assert.Contains("string textBeforeApply = SourceTextEditor.Document.Text;", insertMethod, StringComparison.Ordinal);
        Assert.Contains("string textBeforeApply = SourceTextEditor.Document.Text;", replaceMethod, StringComparison.Ordinal);
        Assert.Contains("\"已撤销添加字段。\"", insertMethod, StringComparison.Ordinal);
        Assert.Contains("\"已重做添加字段。\"", insertMethod, StringComparison.Ordinal);
        Assert.Contains("\"已撤销替换字段。\"", replaceMethod, StringComparison.Ordinal);
        Assert.Contains("\"已重做替换字段。\"", replaceMethod, StringComparison.Ordinal);
        Assert.Contains("_programmaticSemanticUndoState = CreateProgrammaticSemanticUndoState(", resultMethod, StringComparison.Ordinal);
        Assert.Contains("textBeforeApply", resultMethod, StringComparison.Ordinal);
        Assert.Contains("result.UpdatedText", resultMethod, StringComparison.Ordinal);
        Assert.Contains("undoCaretOffset", resultMethod, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_NormalUndoRedoRestoresScrollOnlyWhenViewportDrifts()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string undoMethod = ExtractMethod(shellCode, "UndoCurrentFileFromShell");
        string redoMethod = ExtractMethod(shellCode, "RedoCurrentFileFromShell");

        Assert.Contains("int? topLineNumber = CaptureSourceEditorTopLineNumber();", undoMethod, StringComparison.Ordinal);
        Assert.Contains("int? topLineNumber = CaptureSourceEditorTopLineNumber();", redoMethod, StringComparison.Ordinal);
        Assert.Contains("RestoreSourceEditorTopLineIfDrifted(topLineNumber);", undoMethod, StringComparison.Ordinal);
        Assert.Contains("RestoreSourceEditorTopLineIfDrifted(topLineNumber);", redoMethod, StringComparison.Ordinal);
        string restoreMethod = ExtractMethod(shellCode, "RestoreSourceEditorTopLineIfDrifted");
        Assert.Contains("Math.Abs(currentTopLineNumber.Value - topLineNumber.Value) > 2", restoreMethod, StringComparison.Ordinal);
        Assert.Contains("SourceTextEditor.ScrollTo(topLineNumber.Value, 1);", restoreMethod, StringComparison.Ordinal);
        Assert.Contains("DispatcherPriority.ContextIdle", restoreMethod, StringComparison.Ordinal);
    }

    private static string ExtractMethod(string source, string methodName)
    {
        string[] methodPrefixes =
        [
            $"private void {methodName}",
            $"private bool {methodName}",
            $"private static void {methodName}",
            $"private static bool {methodName}"
        ];
        int start = methodPrefixes
            .Select(prefix => source.IndexOf(prefix, StringComparison.Ordinal))
            .Where(index => index >= 0)
            .DefaultIfEmpty(-1)
            .Min();
        Assert.True(start >= 0, $"Method {methodName} was not found.");
        int bodyStart = source.IndexOf('{', start);
        Assert.True(bodyStart >= 0, $"Method {methodName} body was not found.");

        int depth = 0;
        for (int index = bodyStart; index < source.Length; index++)
        {
            if (source[index] == '{')
                depth++;
            else if (source[index] == '}')
                depth--;

            if (depth == 0)
                return source.Substring(start, index - start + 1);
        }

        throw new InvalidOperationException($"Method {methodName} body was not closed.");
    }

    private static void AssertInOrder(string source, params string[] expectedFragments)
    {
        int currentIndex = -1;
        foreach (string fragment in expectedFragments)
        {
            int nextIndex = source.IndexOf(fragment, currentIndex + 1, StringComparison.Ordinal);
            Assert.True(nextIndex > currentIndex, $"Expected to find '{fragment}' after index {currentIndex}.");
            currentIndex = nextIndex;
        }
    }
}

