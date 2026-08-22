using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SaveFailureBoundaryGuardrailTests
{
    [Fact]
    public void SaveFailureHardeningSources_DoNotCallLegacySaveUiOrDictionarySerialization()
    {
        string root = TestRepositoryRoot.Find();
        string editingRoot = Path.Combine(root, "RA2IniEditor.IDE", "Editing");
        string[] files =
        [
            Path.Combine(editingRoot, "Ra2SaveCurrentFileFailureKind.cs"),
            Path.Combine(editingRoot, "Ra2RollbackResult.cs"),
            Path.Combine(editingRoot, "IRa2SaveRollbackService.cs"),
            Path.Combine(editingRoot, "Ra2SaveRollbackService.cs"),
            Path.Combine(editingRoot, "Ra2SaveCurrentFileResult.cs"),
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
        Assert.DoesNotContain("Completion", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AddProperty", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("RestoreFromBackup", combinedText, StringComparison.Ordinal);
        Assert.Contains("File.Copy", combinedText, StringComparison.Ordinal);
    }
}

