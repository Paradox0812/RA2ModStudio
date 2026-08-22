using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SourceEditorHoverControllerBoundaryTests
{
    [Fact]
    public void Controller_DoesNotDependOnWindowAvalonEditSaveChainOrObjectAggregator()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Controllers",
            "Hover",
            "Ra2SourceEditorHoverController.cs"));

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
    public void ShellWindow_DelegatesHoverBusinessToController()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Views",
            "ShellWindow.xaml.cs"));

        Assert.Contains("_sourceEditorHoverController.OnPointerMoved", source, StringComparison.Ordinal);
        Assert.Contains("_sourceEditorHoverController.ConsumePendingOffset", source, StringComparison.Ordinal);
        Assert.Contains("_sourceEditorHoverController.ResolveHover", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TryCreateKeyHoverContext", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IsKeyHoverHitCandidate", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_hoverProvider", source, StringComparison.Ordinal);
    }
}

