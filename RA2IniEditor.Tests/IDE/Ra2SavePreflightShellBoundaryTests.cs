using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2SavePreflightShellBoundaryTests
{
    [Fact]
    public void ShellWindow_RunsSavePreflightBeforeCallingSaveService()
    {
        string root = TestRepositoryRoot.Find();
        string shellPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");
        string shellText = File.ReadAllText(shellPath);

        int preflightIndex = shellText.IndexOf("if (!TryRunSavePreflight(viewModel))", StringComparison.Ordinal);
        int saveIndex = shellText.IndexOf("_saveCurrentFileService.Save(", StringComparison.Ordinal);

        Assert.True(preflightIndex >= 0);
        Assert.True(saveIndex >= 0);
        Assert.True(preflightIndex < saveIndex);
    }

    [Fact]
    public void ShellWindow_DirtyNavigationSaveUsesSamePreflightSavePath()
    {
        string root = TestRepositoryRoot.Find();
        string shellPath = Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs");
        string shellText = File.ReadAllText(shellPath);

        Assert.Contains("private bool TrySaveDirtyFileBeforeNavigation(ShellViewModel viewModel)", shellText);
        Assert.Contains("=> TrySaveCurrentFileWithPreflight(viewModel);", shellText);
    }

    [Fact]
    public void SavePreflightDialog_DefinesAutomationIdsForSmokeTests()
    {
        string root = TestRepositoryRoot.Find();
        string dialogPath = Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Views",
            "SavePreflight",
            "SavePreflightConfirmationDialog.xaml");
        string dialogText = File.ReadAllText(dialogPath);

        Assert.Contains("AutomationProperties.AutomationId=\"SavePreflight.Dialog\"", dialogText);
        Assert.Contains("AutomationProperties.AutomationId=\"SavePreflight.SummaryText\"", dialogText);
        Assert.Contains("AutomationProperties.AutomationId=\"SavePreflight.ContinueButton\"", dialogText);
        Assert.Contains("AutomationProperties.AutomationId=\"SavePreflight.CancelButton\"", dialogText);
        Assert.Contains("不会阻止保存", dialogText, StringComparison.Ordinal);
        Assert.Contains("问题”面板", dialogText, StringComparison.Ordinal);
        Assert.Contains("取消，返回编辑", dialogText, StringComparison.Ordinal);
    }
}

