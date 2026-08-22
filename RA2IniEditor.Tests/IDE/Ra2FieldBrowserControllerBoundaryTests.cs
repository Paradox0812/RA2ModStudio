using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FieldBrowserControllerBoundaryTests
{
    [Fact]
    public void Controller_DoesNotDependOnWpfAvalonEditSaveChainOrObjectAggregator()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Controllers",
            "FieldBrowser",
            "Ra2FieldBrowserController.cs"));

        Assert.DoesNotContain("System.Windows", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AvalonEdit", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TextEditor", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Window", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectSaveService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IniFileService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ObjectAggregator", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Document.Replace", source, StringComparison.Ordinal);
        Assert.Contains("_textChangeApplier.Apply", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_DelegatesFieldBrowserBusinessToController()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Views",
            "ShellWindow.xaml.cs"));

        Assert.Contains("_fieldBrowserController.CreateAddPropertyViewModel", source, StringComparison.Ordinal);
        Assert.Contains("_fieldBrowserController.ConfirmAddProperty", source, StringComparison.Ordinal);
        Assert.Contains("_fieldBrowserController.ApplyInsertDuplicate", source, StringComparison.Ordinal);
        Assert.Contains("_fieldBrowserController.ApplyReplaceExisting", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_addPropertyInsertPlanner", source, StringComparison.Ordinal);
        Assert.DoesNotContain("_textChangeApplier.Apply", source, StringComparison.Ordinal);
    }
}

