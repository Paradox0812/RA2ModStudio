using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2IniTextModelBoundaryTests
{
    [Fact]
    public void TextModelSources_DoNotDependOnUiSaveProjectOrDictionaryModel()
    {
        string root = TestRepositoryRoot.Find();
        string textModelRoot = Path.Combine(root, "RA2IniEditor.IDE", "TextModel");
        string combinedText = string.Join(
            Environment.NewLine,
            Directory.GetFiles(textModelRoot, "*.cs").Select(File.ReadAllText));

        Assert.DoesNotContain("Dictionary<", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AvalonEdit", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextEditor", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Document.Replace", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextArea.Document.Insert", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Directory.", combinedText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShellWindow_UsesTextModelOnlyForEditablePreviewWithoutSavePipeline()
    {
        string root = TestRepositoryRoot.Find();
        string shellXaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml"));
        string shellCode = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"));
        string combinedText = shellXaml + Environment.NewLine + shellCode;

        Assert.Contains("IsReadOnly=\"True\"", shellXaml);
        Assert.Contains("Ra2IniTextDocumentParser", combinedText);
        Assert.DoesNotContain("Document.Replace", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextArea.Document.Insert", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", combinedText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("WriteText", combinedText, StringComparison.OrdinalIgnoreCase);
    }
}

