using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SaveCurrentFileMinimalBoundaryTests
{
    [Fact]
    public void MinimalSaveSources_DoNotCallLegacyProjectSaveUiOrDictionarySerialization()
    {
        string root = TestRepositoryRoot.Find();
        string editingRoot = Path.Combine(root, "RA2IniEditor.IDE", "Editing");
        string[] files =
        [
            Path.Combine(editingRoot, "Ra2TextFileWriteResult.cs"),
            Path.Combine(editingRoot, "IRa2TextFirstFileWriter.cs"),
            Path.Combine(editingRoot, "Ra2TextFirstFileWriter.cs"),
            Path.Combine(editingRoot, "Ra2SaveCurrentFileResult.cs"),
            Path.Combine(editingRoot, "IRa2SaveCurrentFileService.cs"),
            Path.Combine(editingRoot, "Ra2SaveCurrentFileService.cs")
        ];
        string combinedText = string.Join(Environment.NewLine, files.Select(File.ReadAllText));

        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniFileService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniSerializer", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("SaveAll", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ctrl+S", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ShellWindow", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllBytes", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("FileStream", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("plan.Text", combinedText, StringComparison.Ordinal);
        Assert.Contains("WriteText", combinedText, StringComparison.Ordinal);
    }
}

