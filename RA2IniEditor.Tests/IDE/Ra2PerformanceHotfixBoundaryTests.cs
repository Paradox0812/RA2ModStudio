using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2PerformanceHotfixBoundaryTests
{
    [Fact]
    public void ShellStatus_CaretAndSelectionHandlersDoNotReadWholeDocument()
    {
        string shellCode = ReadProjectFile("RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");

        string caretMethod = ExtractMethod(shellCode, "SourceTextEditorCaret_OnPositionChanged");
        string selectionMethod = ExtractMethod(shellCode, "SourceTextEditorSelection_OnChanged");

        Assert.Contains("UpdateShellCaretStatus();", caretMethod, StringComparison.Ordinal);
        Assert.Contains("UpdateShellCaretStatus();", selectionMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateShellStatusBar();", caretMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("UpdateShellStatusBar();", selectionMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("Document.Text", caretMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("Document.Text", selectionMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("SelectedText", caretMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("SelectedText", selectionMethod, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellStatus_TextStatusIsUpdatedOnlyFromSplitTextStatusMethod()
    {
        string shellCode = ReadProjectFile("RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");

        string caretStatusMethod = ExtractMethod(shellCode, "UpdateShellCaretStatus");
        string textStatusMethod = ExtractMethod(shellCode, "UpdateShellTextStatus");

        Assert.DoesNotContain("UpdateEditorTextStatus", caretStatusMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("Document.Text", caretStatusMethod, StringComparison.Ordinal);
        Assert.Contains("UpdateEditorTextStatus(SourceTextEditor.Document.Text)", textStatusMethod, StringComparison.Ordinal);
    }

    [Fact]
    public void HoverAndCompletionUseCachedFieldDisplayResolver()
    {
        string shellCode = ReadProjectFile("RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");

        string createResolverMethod = ExtractMethod(shellCode, "CreateFieldDisplayResolver");
        string hoverMethod = ExtractMethod(shellCode, "TryShowSourceEditorHoverAtOffset");
        string completionMethod = ExtractMethod(shellCode, "TryShowCompletionDropdownAtCaret");

        Assert.Contains("GetCachedFieldAnnotations(projectRootPath).DisplayResolver", createResolverMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("RefreshFieldAnnotations(projectRootPath).DisplayResolver", createResolverMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("RefreshFieldAnnotations", hoverMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("RefreshFieldAnnotations", completionMethod, StringComparison.Ordinal);
    }

    [Fact]
    public void LoadSelectedFile_ShowsDocumentBeforeRunningDiagnostics()
    {
        string shellViewModel = ReadProjectFile("RA2IniEditor.IDE", "ViewModels", "ShellViewModel.cs");
        string loadMethod = ExtractMethod(shellViewModel, "LoadSelectedFileAsync");

        int showDocumentIndex = loadMethod.IndexOf("SourceEditor.ShowDocument", StringComparison.Ordinal);
        int yieldIndex = loadMethod.IndexOf("await Task.Yield()", StringComparison.Ordinal);
        int diagnosticsIndex = loadMethod.IndexOf("_diagnosticService.Analyze", StringComparison.Ordinal);

        Assert.True(showDocumentIndex >= 0);
        Assert.True(yieldIndex > showDocumentIndex);
        Assert.True(diagnosticsIndex > yieldIndex);
        Assert.Contains("Task.Run(() =>", loadMethod, StringComparison.Ordinal);
    }

    [Fact]
    public void HighlightingTransformerCachesByDocumentVersion()
    {
        string transformer = ReadProjectFile("RA2IniEditor.IDE", "Highlighting", "Ra2KnownFieldHighlightingTransformer.cs");

        Assert.Contains("_cachedDocument", transformer, StringComparison.Ordinal);
        Assert.Contains("_cachedVersion", transformer, StringComparison.Ordinal);
        Assert.Contains("Equals(_cachedVersion, version)", transformer, StringComparison.Ordinal);
        Assert.DoesNotContain("_cachedText", transformer, StringComparison.Ordinal);
        Assert.DoesNotContain("string.Equals(_cachedText", transformer, StringComparison.Ordinal);
    }

    private static string ReadProjectFile(params string[] pathParts)
        => File.ReadAllText(Path.Combine(TestRepositoryRoot.Find(), Path.Combine(pathParts)));

    private static string ExtractMethod(string source, string methodName)
    {
        string[] patterns =
        [
            $"private void {methodName}",
            $"private bool {methodName}",
            $"private int {methodName}",
            $"private async Task {methodName}",
            $"private IRa2FieldDisplayResolver {methodName}",
            $"private Ra2FieldAnnotationRefreshResult {methodName}"
        ];

        int start = patterns
            .Select(pattern => source.IndexOf(pattern, StringComparison.Ordinal))
            .Where(index => index >= 0)
            .DefaultIfEmpty(-1)
            .Min();
        if (start < 0)
            throw new InvalidOperationException($"Method '{methodName}' was not found.");

        int braceStart = source.IndexOf('{', start);
        if (braceStart < 0)
            throw new InvalidOperationException($"Method '{methodName}' has no body.");

        int depth = 0;
        for (int index = braceStart; index < source.Length; index++)
        {
            if (source[index] == '{')
                depth++;
            else if (source[index] == '}')
                depth--;

            if (depth == 0)
                return source.Substring(start, index - start + 1);
        }

        throw new InvalidOperationException($"Method '{methodName}' body was not closed.");
    }
}

