using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class IdeShellFieldRegistryApplyTests
{
    [Fact]
    public void ShellWindow_PassesApplyReloadCallbackToHarvestPreview()
    {
        string root = TestRepositoryRoot.Find();
        string code = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("new FieldRegistryHarvestPreviewWindow(", code, StringComparison.Ordinal);
        Assert.Contains("GetGlobalRootDirectoryPath", code, StringComparison.Ordinal);
        Assert.Contains("ReloadLocalFieldRegistryForReadonlyHighlighting", code, StringComparison.Ordinal);
        Assert.Contains("CurrentProjectRootPath", code, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellWindow_ApplyPathDoesNotUseLegacyProjectServices()
    {
        string root = TestRepositoryRoot.Find();
        string code = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.DoesNotContain("ProjectSaveService", code, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", code, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", code, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CompletionWindow", code, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShellWindow_RollbackSuccessReusesLocalFieldRegistryReloadPath()
    {
        string root = TestRepositoryRoot.Find();
        string code = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Contains("RollbackCompleted", code, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManagerWindow_OnRollbackCompleted", code, StringComparison.Ordinal);
        Assert.Contains("ReloadLocalFieldRegistryForReadonlyHighlighting(viewModel);", code, StringComparison.Ordinal);
        Assert.Equal(1, CountOccurrences(code, "LineTransformers.Add("));
        Assert.Equal(1, CountOccurrences(code, "new Ra2KnownFieldHighlightingTransformer("));
    }

    [Fact]
    public void ShellWindow_HighlightingTransformerIsAddedThroughSingleHelper()
    {
        string root = TestRepositoryRoot.Find();
        string code = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));

        Assert.Equal(1, CountOccurrences(code, "LineTransformers.Add("));
        Assert.Equal(1, CountOccurrences(code, "new Ra2KnownFieldHighlightingTransformer("));
        Assert.Contains("LineTransformers.RemoveAt", code, StringComparison.Ordinal);
        Assert.Contains("ReloadLocalFieldRegistryForReadonlyHighlighting", code, StringComparison.Ordinal);
    }

    private static int CountOccurrences(string text, string value)
    {
        int count = 0;
        int index = 0;
        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }
}

