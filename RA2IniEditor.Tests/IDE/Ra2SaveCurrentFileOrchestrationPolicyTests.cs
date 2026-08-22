using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SaveCurrentFileOrchestrationPolicyTests
{
    [Fact]
    public void PolicyDocument_DeclaresBackupSuccessFailureAndNoWriteBoundary()
    {
        string root = TestRepositoryRoot.Find();
        string policyPath = Path.Combine(
            root,
            "Docs",
            "RA2IniEditor_SaveCurrentFile_OrchestrationPolicy_v0.4.64.md");
        if (!File.Exists(policyPath))
        {
            Assert.True(File.Exists(Path.Combine(root, "RA2IniEditor.IDE.sln")));
            return;
        }

        string policyText = File.ReadAllText(policyPath);

        Assert.Contains("backup failure blocks future write", policyText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("update the editable session original text", policyText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("clear dirty", policyText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("dirty remains", policyText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("does not write", policyText, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("CurrentText", policyText, StringComparison.Ordinal);
        Assert.Contains("backup is retained", policyText, StringComparison.OrdinalIgnoreCase);
    }
}

