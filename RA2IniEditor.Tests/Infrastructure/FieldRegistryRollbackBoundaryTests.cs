using Xunit;

namespace RA2IniEditor.Tests.Infrastructure;

public sealed class FieldRegistryRollbackBoundaryTests
{
    [Fact]
    public void RollbackServiceDoesNotReferenceUiNetworkCompletionSaveOrProjectLifecycle()
    {
        string root = TestRepositoryRoot.Find();
        string rollbackDirectory = Path.Combine(
            root,
            "RA2IniEditor.Infrastructure",
            "FieldRegistry",
            "Apply",
            "Rollback");
        string text = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(rollbackDirectory, "*.cs", SearchOption.AllDirectories)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .Select(File.ReadAllText));

        Assert.DoesNotContain("MessageBox", text, StringComparison.Ordinal);
        Assert.DoesNotContain("Window", text, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", text, StringComparison.Ordinal);
        Assert.DoesNotContain("GitHub", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Completion", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ObjectAggregator", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectLoader", text, StringComparison.Ordinal);
    }
}

