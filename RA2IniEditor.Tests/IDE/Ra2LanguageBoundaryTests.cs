using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2LanguageBoundaryTests
{
    [Fact]
    public void LanguageService_IsCurrentDocumentOnlyWithoutUiSaveDirtyNetworkOrLegacyProjectServices()
    {
        string root = TestRepositoryRoot.Find();
        string languageRoot = Path.Combine(root, "RA2IniEditor.Application", "Language");
        string classificationRoot = Path.Combine(root, "RA2IniEditor.Application", "Classification");
        string languageText = string.Join(
            Environment.NewLine,
            Directory.GetFiles(languageRoot, "*.cs")
                .Concat(Directory.GetFiles(classificationRoot, "*.cs"))
                .Select(File.ReadAllText));

        Assert.Contains("Ra2DocumentSnapshot", languageText, StringComparison.Ordinal);
        Assert.Contains("Ra2DocumentSemanticModel", languageText, StringComparison.Ordinal);
        Assert.Contains("Ra2CaretContext", languageText, StringComparison.Ordinal);
        Assert.Contains("IRa2SectionClassifier", languageText, StringComparison.Ordinal);
        Assert.DoesNotContain("AvalonEdit", languageText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Window", languageText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextChanged", languageText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("HttpClient", languageText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GitHub", languageText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", languageText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", languageText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", languageText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.ReadAllText", languageText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Directory.GetFiles", languageText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AutoApply", languageText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AutoFetch", languageText, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Dirty", languageText, StringComparison.OrdinalIgnoreCase);
    }
}

