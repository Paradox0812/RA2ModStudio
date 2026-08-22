using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SaveCurrentFileOrchestrationBoundaryTests
{
    [Fact]
    public void OrchestrationSources_DoNotWriteIniOrCallSaveChain()
    {
        string root = TestRepositoryRoot.Find();
        string editingRoot = Path.Combine(root, "RA2IniEditor.IDE", "Editing");
        string[] files =
        [
            Path.Combine(editingRoot, "Ra2SaveCurrentFileOrchestrationStage.cs"),
            Path.Combine(editingRoot, "Ra2SaveCurrentFileOrchestrationStatus.cs"),
            Path.Combine(editingRoot, "Ra2SaveCurrentFileOrchestrationResult.cs"),
            Path.Combine(editingRoot, "IRa2SaveCurrentFileOrchestrator.cs"),
            Path.Combine(editingRoot, "Ra2SaveCurrentFileOrchestrator.cs")
        ];
        string combinedText = string.Join(Environment.NewLine, files.Select(File.ReadAllText));

        Assert.DoesNotContain("File.", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Directory.", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("FileStream", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IniFileService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IIniFileStore", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllBytes", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Ra2EditableDocumentState", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("OriginalText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CurrentText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("UpdateText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Revert(", combinedText, StringComparison.OrdinalIgnoreCase);
    }
}

