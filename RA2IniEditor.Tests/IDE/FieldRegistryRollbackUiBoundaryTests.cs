using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class FieldRegistryRollbackUiBoundaryTests
{
    [Fact]
    public void RollbackUiDoesNotReferenceNetworkCompletionSaveEditOrLegacyProjectServices()
    {
        string root = TestRepositoryRoot.Find();
        string[] files =
        [
            Path.Combine(root, "RA2IniEditor.IDE", "Views", "ShellWindow.xaml.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryManagerWindow.xaml"),
            Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryManagerWindow.xaml.cs"),
            Path.Combine(root, "RA2IniEditor.IDE", "ViewModels", "FieldRegistryManagerViewModel.cs")
        ];
        string text = string.Join(Environment.NewLine, files.Select(File.ReadAllText));

        Assert.Contains("IFieldRegistryRollbackService", text, StringComparison.Ordinal);
        Assert.Contains("RollbackSelectedConfirmed", text, StringComparison.Ordinal);
        Assert.Contains("MessageBox.Show", text, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RollbackPanel", text, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RollbackSelectedButton", text, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.OpenRollbackTargetFolderButton", text, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.OpenRollbackManifestFolderButton", text, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.OpenRollbackBackupFolderButton", text, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RollbackDisabledReason", text, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RollbackStatusText", text, StringComparison.Ordinal);
        Assert.Contains("备份清单", text, StringComparison.Ordinal);
        Assert.Contains("恢复或删除", text, StringComparison.Ordinal);
        Assert.Contains("目标", text, StringComparison.Ordinal);
        Assert.Contains("字段库", text, StringComparison.Ordinal);
        Assert.Contains("UnknownKey 仅表示", text, StringComparison.Ordinal);
        Assert.Contains("WarningsStatusText", text, StringComparison.Ordinal);
        Assert.Contains("UseShellExecute = true", text, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RollbackDetails", text, StringComparison.Ordinal);
        Assert.Contains("IsChecked=\"{Binding TargetFileExisted, Mode=OneWay}\"", text, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding StatusMessage}\"", text, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding Mode}\"", text, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding TargetFilePath}\"", text, StringComparison.Ordinal);
        Assert.Contains("Text=\"{Binding ManifestFilePath}\"", text, StringComparison.Ordinal);
        Assert.DoesNotContain("HttpClient", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("GitHub", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("CompletionWindow", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectSaveService", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ObjectAggregator", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("ProjectLoader", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("TextChanged=\"Rollback", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("BatchRollback", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("AutoRollback", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FieldRegistryManagerWindow_UsesChineseIdeSectionsAndReadableLayout()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(root, "RA2IniEditor.IDE", "Views", "FieldRegistryManagerWindow.xaml"));

        Assert.Contains("FieldRegistryManager.HeaderArea", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.Toolbar", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.ActivePacksPanel", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RollbackPanel", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.CleanupPreviewPanel", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.WarningsPanel", xaml, StringComparison.Ordinal);
        Assert.Contains("MinHeight=\"170\"", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.CleanupPreviewExpander", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.RollbackDetails", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.CleanupDetails", xaml, StringComparison.Ordinal);
        Assert.Contains("FieldRegistryManager.CleanupPreviewScrollHost", xaml, StringComparison.Ordinal);
        Assert.Contains("MaxHeight=\"260\"", xaml, StringComparison.Ordinal);
        Assert.Contains("MinHeight=\"110\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"重新加载本地字段库\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"打开字段导入预览\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"构建清理计划\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"应用清理\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Content=\"重新学习当前 INI\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"当前 active 字段库\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"最近导入备份\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"字段库概括清理预览\"", xaml, StringComparison.Ordinal);
        Assert.Contains("Text=\"警告\"", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Reload Local Field Registry", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Recent Import Backups", xaml, StringComparison.Ordinal);
        Assert.DoesNotContain("Status Message", xaml, StringComparison.Ordinal);
    }
}

