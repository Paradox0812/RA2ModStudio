using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SourceEditorHoverBoundaryTests
{
    [Fact]
    public void ShellWindow_SourceEditorHoverUsesDisplayResolverWithoutSaveOrCaretMutation()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string hoverDisplay = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "Language", "Ra2HoverDisplayViewModel.cs"));
        string combined = shellCode + Environment.NewLine + hoverDisplay;

        Assert.Contains("SourceTextEditor.MouseMove += SourceTextEditor_OnMouseMove", shellCode);
        Assert.Contains("SourceTextEditor.MouseLeave += SourceTextEditor_OnMouseLeave", shellCode);
        Assert.Contains("_sourceEditorHoverController.ResolveHover", shellCode);
        Assert.Contains("CreateFieldDisplayResolver", shellCode);
        Assert.Contains("SourceTextEditor.GetPositionFromPoint", shellCode);
        Assert.Contains("ShowSourceEditorHoverToolTip(result.Display)", shellCode);
        Assert.Contains("private Popup? _currentHoverPopup;", shellCode);
        Assert.Contains("private readonly DispatcherTimer _sourceEditorHoverTimer;", shellCode);
        Assert.Contains("SourceEditorHoverTimer_OnTick", shellCode);
        Assert.Contains("CloseSourceEditorHoverToolTip();", shellCode);
        Assert.DoesNotContain("_hoverProvider", shellCode, StringComparison.Ordinal);
        Assert.DoesNotContain("TryCreateKeyHoverContext", shellCode, StringComparison.Ordinal);
        Assert.DoesNotContain("IsKeyHoverHitCandidate", shellCode, StringComparison.Ordinal);
        Assert.DoesNotContain("SourceTextEditor.ToolTip = _currentHover", shellCode, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextArea.Caret.Offset = offset", shellCode, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniFileService", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText(", combined, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShellWindow_SourceEditorHoverClosesOnLifecycleChanges()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("private void ShowSourceEditorHoverToolTip(Ra2HoverDisplayViewModel display)", shellCode);
        Assert.Contains("_sourceEditorHoverTimer.Stop();", ExtractMethod(shellCode, "CloseSourceEditorHoverToolTip"));
        Assert.Contains("CloseSourceEditorHoverToolTip();", ExtractMethod(shellCode, "SourceTextEditor_OnTextChanged"));
        Assert.Contains("CloseSourceEditorHoverToolTip();", ExtractMethod(shellCode, "SourceTextEditorCaret_OnPositionChanged"));
        Assert.Contains("CloseSourceEditorHoverToolTip();", ExtractMethod(shellCode, "SourceTextEditorTextView_OnScrollOffsetChanged"));
        Assert.Contains("CloseSourceEditorHoverToolTip();", ExtractMethod(shellCode, "SourceTextEditor_OnLostKeyboardFocus"));
        Assert.Contains("CloseSourceEditorHoverToolTip();", ExtractMethod(shellCode, "ShowCompletionDropdown"));
        Assert.Contains("CloseSourceEditorHoverToolTip();", ExtractMethod(shellCode, "AddProperty_OnClick"));
        Assert.Contains("CloseSourceEditorHoverToolTip();", ExtractMethod(shellCode, "SetEditorTextFromProgram"));
    }

    [Fact]
    public void ShellWindow_SourceEditorHoverUsesStableNonInteractivePopup()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string showHoverMethod = ExtractMethod(shellCode, "ShowSourceEditorHoverToolTip");

        Assert.Contains("private const int SourceEditorHoverDelayMilliseconds = 300;", shellCode);
        Assert.Contains("private const double SourceEditorHoverHorizontalOffset = 12.0;", shellCode);
        Assert.Contains("private const double SourceEditorHoverVerticalOffset = 18.0;", shellCode);
        Assert.Contains("_sourceEditorHoverController.MarkHoverShown(offset)", shellCode);
        Assert.Contains("Placement = PlacementMode.Relative", showHoverMethod);
        Assert.Contains("PlacementTarget = this", showHoverMethod);
        Assert.Contains("Focusable = false", showHoverMethod);
        Assert.Contains("IsHitTestVisible = false", showHoverMethod);
        Assert.Contains("HorizontalOffset = popupPoint.X", showHoverMethod);
        Assert.Contains("StaysOpen = true", showHoverMethod);
        Assert.Contains("VerticalOffset = popupPoint.Y", showHoverMethod);
        Assert.DoesNotContain("SourceTextEditor.TextArea.Caret.Offset", showHoverMethod, StringComparison.Ordinal);
        Assert.DoesNotContain("Document.Text =", showHoverMethod, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_SourceEditorHoverUsesCompactReadableCard()
    {
        string root = TestRepositoryRoot.Find();
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string hoverDisplay = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "Language", "Ra2HoverDisplayViewModel.cs"));
        string workspaceStyles = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Themes", "IdeWorkspaceStyles.xaml"));

        Assert.Contains("IdeHoverCardStyle", shellCode);
        Assert.Contains("IdeHoverCodePillStyle", shellCode);
        Assert.Contains("UiAccentBrush", shellCode);
        Assert.Contains("UiTextPrimaryBrush", shellCode);
        Assert.Contains("UiSuccessBrush", shellCode);
        Assert.Contains("x:Key=\"IdeHoverCardStyle\"", workspaceStyles);
        Assert.Contains("x:Key=\"IdeHoverCodePillStyle\"", workspaceStyles);
        Assert.DoesNotContain("CreateFrozenBrush", shellCode);
        Assert.DoesNotContain("Color.FromRgb", ExtractMethod(shellCode, "ShowSourceEditorHoverToolTip"));
        Assert.Contains("CreateSourceEditorHoverCard", shellCode);
        Assert.Contains("display.FieldTypeText,", shellCode);
        Assert.Contains("CreateHoverInlineText(", shellCode);
        Assert.Contains("AddHoverMetadataPair(metadata, \"示例\"", shellCode);
        Assert.Contains("AddHoverMetadataPair(metadata, \"来源\"", shellCode);
        Assert.Contains("AddHoverMetadataPair(metadata, \"适用\"", shellCode);
        Assert.Contains("TextTrimming = TextTrimming.CharacterEllipsis", shellCode);
        Assert.Contains("CompactCommentText", hoverDisplay);
        Assert.DoesNotContain("CreateSourceEditorHoverMetaRow", shellCode);
        Assert.DoesNotContain("AddHoverExamplePair", shellCode);
        Assert.DoesNotContain("display.ExampleDescriptionText", shellCode);
        Assert.DoesNotContain("Text = text", ExtractMethod(shellCode, "ShowSourceEditorHoverToolTip"));
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
}
