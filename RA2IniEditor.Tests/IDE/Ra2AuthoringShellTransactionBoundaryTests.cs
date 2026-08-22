using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AuthoringShellTransactionBoundaryTests
{
    [Fact]
    public void ShellTransactionPort_UsesCurrencyGateControllerAndSemanticUndo()
    {
        string shellCode = ReadShellCode();
        string transactionMethod = ExtractMethod(
            shellCode,
            "ApplyAuthoringPreviewTransaction");

        Assert.Contains("ShellEditorTransactionPort : IRa2EditorTransactionPort", shellCode, StringComparison.Ordinal);
        Assert.Contains("new Ra2IniAuthoringWorkspace(", shellCode, StringComparison.Ordinal);
        AssertInOrder(
            transactionMethod,
            "_authoringPreviewCurrencyEvaluator.Evaluate(",
            "_editorSessionController.ApplyProgrammaticText(",
            "Ra2IniEditApplyResult.Applied(",
            "SetEditorTextFromProgram(",
            "_editableSession = sessionResult.Session;",
            "_programmaticSemanticUndoState = semanticUndoAfterApply;",
            "ClearAvalonEditUndoStackOnly();");
        Assert.Contains("TryRestoreEditorAfterAuthoringFailure", transactionMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("_saveCurrentFileService", transactionMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("File.", transactionMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("RA2IniEditor.IDE.AI", transactionMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("_projectSearchService", transactionMethod, StringComparison.Ordinal);
    }

    [Fact]
    public void Shell_InvalidatesActivePreviewAtEveryApprovedStateBoundary()
    {
        string shellCode = ReadShellCode();
        const string invalidationCall =
            "InvalidateActiveAiEditProposal(markSuperseded: false);";

        Assert.Contains(
            invalidationCall,
            ExtractMethod(shellCode, "SourceTextEditor_OnTextChanged"),
            StringComparison.Ordinal);
        Assert.Contains(
            invalidationCall,
            ExtractMethod(shellCode, "SetEditorTextFromProgram"),
            StringComparison.Ordinal);
        Assert.Contains(
            invalidationCall,
            ExtractMethod(shellCode, "ResetEditableSessionToReadOnly"),
            StringComparison.Ordinal);
        Assert.Contains(
            invalidationCall,
            ExtractMethod(shellCode, "StartEditableSessionForCurrentSnapshot"),
            StringComparison.Ordinal);
        Assert.Contains(
            invalidationCall,
            ExtractMethod(shellCode, "InstallReadonlySourceHighlighting"),
            StringComparison.Ordinal);
        Assert.Contains(
            invalidationCall,
            ExtractMethod(shellCode, "ReloadReadonlySourceHighlighting"),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Shell_RefreshesCurrentFileDiagnosticsOnlyAfterSuccessfulAuthoringApply()
    {
        string shellCode = ReadShellCode();
        string applyHandler = ExtractMethod(
            shellCode,
            "AiEditProposalView_OnApplyRequested");
        string transactionMethod = ExtractMethod(
            shellCode,
            "ApplyAuthoringPreviewTransaction");

        AssertInOrder(
            applyHandler,
            "_aiAuthoringCoordinator.ApplyConfirmed(",
            "if (result.Succeeded)",
            "viewModel.MarkApplied(result.Message);",
            "result.AuthoringResult?.TextToSyncToEditor",
            "shellViewModel.RefreshCurrentFileDiagnostics(",
            "_fieldRegistryRuntimeService.CurrentProvider",
            "else if (result.FailureKind == Ra2AiEditProposalFailureKind.RequestContextStale)",
            "DetachActiveAiEditProposalView();");
        Assert.Equal(
            1,
            CountOccurrences(applyHandler, "RefreshCurrentFileDiagnostics("));
        Assert.Contains(
            "catch (Exception exception) when (exception is not OutOfMemoryException",
            applyHandler,
            StringComparison.Ordinal);
        Assert.DoesNotContain("RefreshCurrentFileDiagnostics(", transactionMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("_saveCurrentFileService", applyHandler, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellXaml_HasNoA3UserEntryOrAutomationSurface()
    {
        string root = TestRepositoryRoot.Find();
        string shellXaml = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Views",
            "ShellWindow.xaml"));

        Assert.DoesNotContain("AuthoringWorkspace", shellXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("ApplyAuthoringPreview", shellXaml, StringComparison.Ordinal);
        Assert.DoesNotContain("AGENT-AUTHORING", shellXaml, StringComparison.Ordinal);
    }

    private static string ReadShellCode()
    {
        string root = TestRepositoryRoot.Find();
        return File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Views",
            "ShellWindow.xaml.cs"));
    }

    private static string ExtractMethod(string source, string methodName)
    {
        string[] declarationPrefixes =
        [
            $"private void {methodName}(",
            $"private bool {methodName}(",
            $"private Ra2IniEditApplyResult {methodName}("
        ];
        int start = declarationPrefixes
            .Select(prefix => source.IndexOf(prefix, StringComparison.Ordinal))
            .Where(index => index >= 0)
            .DefaultIfEmpty(-1)
            .Min();
        Assert.True(start >= 0, $"Method {methodName} declaration was not found.");
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
            int nextIndex = source.IndexOf(
                fragment,
                currentIndex + 1,
                StringComparison.Ordinal);
            Assert.True(
                nextIndex > currentIndex,
                $"Expected to find '{fragment}' after index {currentIndex}.");
            currentIndex = nextIndex;
        }
    }

    private static int CountOccurrences(string source, string fragment)
    {
        int count = 0;
        int index = 0;
        while ((index = source.IndexOf(fragment, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += fragment.Length;
        }

        return count;
    }
}
