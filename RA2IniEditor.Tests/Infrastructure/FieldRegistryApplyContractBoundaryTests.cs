using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryApplyContractBoundaryTests
{
    [Fact]
    public void ApplyLayerDoesNotContainNetworkUiSaveOrReloadEntrypoints()
    {
        string root = TestRepositoryRoot.Find();
        string applyDirectory = Path.Combine(root, "RA2IniEditor.Infrastructure", "FieldRegistry", "Apply");
        Assert.True(Directory.Exists(applyDirectory), $"Apply directory not found: {applyDirectory}");

        string[] forbiddenPatterns =
        [
            "ApplyCommand",
            "RollbackCommand",
            "HttpClient",
            "WebRequest",
            "ProjectSaveService",
            "ProjectLoader",
            "ObjectAggregator",
            "MainWindowViewModel",
            "Completion",
            "ReloadLocalFieldRegistry"
        ];

        string[] files = Directory.GetFiles(applyDirectory, "*.cs", SearchOption.TopDirectoryOnly);
        Assert.NotEmpty(files);

        foreach (string file in files)
        {
            string text = File.ReadAllText(file);
            foreach (string pattern in forbiddenPatterns)
                Assert.DoesNotContain(pattern, text, StringComparison.Ordinal);
        }
    }
}

