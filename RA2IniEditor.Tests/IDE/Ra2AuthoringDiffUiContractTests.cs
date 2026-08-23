using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2AuthoringDiffUiContractTests
{
    [Fact]
    public void DiffView_UsesApprovedLayoutVirtualizationAndAutomationContract()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(
            root, "RA2IniEditor.IDE", "AuthoringDiff", "Ra2AuthoringDiffView.xaml"));
        string code = File.ReadAllText(Path.Combine(
            root, "RA2IniEditor.IDE", "AuthoringDiff", "Ra2AuthoringDiffView.xaml.cs"));

        Assert.Contains("MinHeight=\"32\"", xaml, StringComparison.Ordinal);
        Assert.Contains("MinHeight=\"36\"", xaml, StringComparison.Ordinal);
        Assert.Contains("<ColumnDefinition Width=\"44\"", xaml, StringComparison.Ordinal);
        Assert.Contains("<ColumnDefinition Width=\"24\"", xaml, StringComparison.Ordinal);
        Assert.Contains("VirtualizingPanel.IsVirtualizing=\"True\"", xaml, StringComparison.Ordinal);
        Assert.Contains("VirtualizingPanel.VirtualizationMode=\"Recycling\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("<DataGrid", xaml, StringComparison.Ordinal);
        foreach (string automationId in RequiredAutomationIds)
            Assert.Contains(automationId, xaml + code, StringComparison.Ordinal);
        Assert.Contains("DiagnosticSummaryText.Visibility = Visibility.Visible", code, StringComparison.Ordinal);
        Assert.Contains("width < 640", code, StringComparison.Ordinal);
        Assert.Contains("ReturnButton.Content = width < 640 ? \"↩\"", code, StringComparison.Ordinal);
    }

    [Fact]
    public void Shell_UsesTemporaryDocumentLifecycleWithoutLayoutPersistenceOrParallelApplyAuthority()
    {
        string root = TestRepositoryRoot.Find();
        string shell = File.ReadAllText(Path.Combine(
            root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("new()\n        {\n            Title = viewModel.Title,\n            ContentId = \"Document.AuthoringDiff\"", shell.Replace("\r\n", "\n"), StringComparison.Ordinal);
        Assert.Contains("document.Closed += AuthoringDiffDocument_OnClosed", shell, StringComparison.Ordinal);
        Assert.Contains("ReleaseAuthoringDiffView(closeDocument: false)", shell, StringComparison.Ordinal);
        Assert.Contains("_dockLayoutSession.FindContent(\"Document.Source\") as LayoutDocument", shell, StringComparison.Ordinal);
        Assert.Contains("FindCurrentSourceDocument()?.Parent is not LayoutDocumentPane pane", shell, StringComparison.Ordinal);
        Assert.DoesNotContain("SourceDocumentAnchorable.Parent as LayoutDocumentPane", shell, StringComparison.Ordinal);
        Assert.DoesNotContain("Descendents().OfType<LayoutDocumentPane>().FirstOrDefault()", shell, StringComparison.Ordinal);
        Assert.Contains("view.OpenDiffRequested += AiEditProposalView_OnOpenDiffRequested", shell, StringComparison.Ordinal);
        Assert.Contains("_aiAuthoringCoordinator.ApplyConfirmed(viewModel.Proposal)", shell, StringComparison.Ordinal);
        Assert.Contains("_aiAuthoringCoordinator.Dismiss(viewModel.Proposal)", shell, StringComparison.Ordinal);
        Assert.Contains("result.AuthoringResult?.RedoCaretOffset", shell, StringComparison.Ordinal);
        Assert.DoesNotContain("SerializeLayout", shell, StringComparison.Ordinal);
        Assert.DoesNotContain("DeserializeLayout", shell, StringComparison.Ordinal);
    }

    private static readonly string[] RequiredAutomationIds =
    [
        "Shell.AuthoringDiff.Document",
        "Shell.AuthoringDiff.StatusBar",
        "Shell.AuthoringDiff.Stats",
        "Shell.AuthoringDiff.DiagnosticSummary",
        "Shell.AuthoringDiff.ReturnToSourceButton",
        "Shell.AuthoringDiff.DismissButton",
        "Shell.AuthoringDiff.ApplyAllButton",
        "Shell.AuthoringDiff.ScrollViewer",
        "Shell.AuthoringDiff.Rows",
        "Shell.AuthoringDiff.StaleNotice"
    ];
}
