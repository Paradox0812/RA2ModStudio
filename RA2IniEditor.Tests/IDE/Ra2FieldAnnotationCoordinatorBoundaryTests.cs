using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2FieldAnnotationCoordinatorBoundaryTests
{
    [Fact]
    public void Coordinator_DoesNotDependOnWpfAvalonEditSaveChainOrEditorText()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Controllers",
            "FieldAnnotations",
            "Ra2FieldAnnotationCoordinator.cs"));

        Assert.DoesNotContain("System.Windows", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AvalonEdit", source, StringComparison.Ordinal);
        Assert.DoesNotContain("TextEditor", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Window", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectSaveService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("IniFileService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("File.WriteAllText", source, StringComparison.Ordinal);
        Assert.DoesNotContain("CurrentText", source, StringComparison.Ordinal);
        Assert.DoesNotContain("EditableDirty", source, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_DelegatesAnnotationRefreshToCoordinator()
    {
        string source = File.ReadAllText(Path.Combine(
            TestRepositoryRoot.Find(),
            "RA2IniEditor.IDE",
            "Views",
            "ShellWindow.xaml.cs"));

        Assert.Contains("IRa2FieldAnnotationCoordinator", source, StringComparison.Ordinal);
        Assert.Contains("_fieldAnnotationCoordinator.Refresh", source, StringComparison.Ordinal);
        Assert.Contains("_fieldAnnotationCoordinator.GetProjectAnnotationPath", source, StringComparison.Ordinal);
        Assert.DoesNotContain("new Ra2FieldDisplayResolver(\r\n                    _fieldRegistryRuntimeService.CurrentProvider", source, StringComparison.Ordinal);
    }
}

