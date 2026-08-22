using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2CompletionInteractionControllerBoundaryTests
{
    [Fact]
    public void Controller_DoesNotDependOnWpfAvalonEditSaveChainOrObjectAggregator()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Controllers",
            "Completion",
            "Ra2CompletionInteractionController.cs"));

        Assert.DoesNotContain("System.Windows", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AvalonEdit", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TextEditor", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Window", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectSaveService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IniFileService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ObjectAggregator", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Document.Replace", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_DelegatesCompletionBusinessToController()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Views",
            "ShellWindow.xaml.cs"));

        Assert.Contains("_completionInteractionController.OpenCompletions", source, StringComparison.Ordinal);
        Assert.Contains("_completionInteractionController.TryCommit", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_completionProvider", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_completionDisplayEnhancer", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_completionCommitCoordinator", source, StringComparison.Ordinal);
    }
}

