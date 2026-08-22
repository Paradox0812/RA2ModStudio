using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2IniLanguageAnalysisBoundaryTests
{
    [Fact]
    public void ContractSources_DoNotReferenceUiShellMutableRuntimeOrWriterTypes()
    {
        string root = TestRepositoryRoot.Find();
        string[] contractPaths =
        [
            Path.Combine(root, "RA2IniEditor.IDE", "Services", "Ra2FieldRegistryProviderSnapshot.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Language", "Ra2LanguageAnalysisRequest.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Language", "Ra2DiagnosticFact.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Language", "Ra2IniLanguageAnalysisResult.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Language", "IRa2IniLanguageAnalysisService.cs")
        ];
        string[] forbiddenTokens =
        [
            "System.Windows",
            "ICSharpCode.AvalonEdit",
            "ShellWindow",
            "ViewModels",
            "FieldRegistryRuntimeService",
            "CurrentSourceSnapshot",
            "SourceEditorState",
            "Writer",
            "File.Write",
            "Directory."
        ];

        foreach (string path in contractPaths)
        {
            string source = File.ReadAllText(path);
            foreach (string token in forbiddenTokens)
                Assert.DoesNotContain(token, source, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void FacadeSource_DoesNotReadRuntimeServiceOrPerformIo()
    {
        string root = TestRepositoryRoot.Find();
        string path = Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Language",
            "Ra2IniLanguageAnalysisService.cs");
        string source = File.ReadAllText(path);

        Assert.DoesNotContain("FieldRegistryRuntimeService", source, StringComparison.Ordinal);
        Assert.DoesNotContain("System.Windows", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ICSharpCode.AvalonEdit", source, StringComparison.Ordinal);
        Assert.DoesNotContain("ShellWindow", source, StringComparison.Ordinal);
        Assert.DoesNotContain("File.", source, StringComparison.Ordinal);
        Assert.DoesNotContain("Directory.", source, StringComparison.Ordinal);
    }
}
