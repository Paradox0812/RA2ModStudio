using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class Ra2DirtyNavigationDialogBoundaryTests
{
    [Fact]
    public void DirtyNavigationDialog_UsesChineseTextAndAutomationIds()
    {
        string root = TestRepositoryRoot.Find();
        string xaml = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Views",
            "DirtyNavigation",
            "Ra2DirtyNavigationDialog.xaml"));

        Assert.Contains("DirtyNavigation.Dialog", xaml, StringComparison.Ordinal);
        Assert.Contains("DirtyNavigation.SaveButton", xaml, StringComparison.Ordinal);
        Assert.Contains("DirtyNavigation.DiscardButton", xaml, StringComparison.Ordinal);
        Assert.Contains("DirtyNavigation.CancelButton", xaml, StringComparison.Ordinal);
        Assert.Contains("未保存的修改", xaml, StringComparison.Ordinal);
        Assert.Contains("当前文件有未保存的修改。是否先保存？", xaml, StringComparison.Ordinal);
        Assert.Contains("保存", xaml, StringComparison.Ordinal);
        Assert.Contains("放弃修改", xaml, StringComparison.Ordinal);
        Assert.Contains("取消", xaml, StringComparison.Ordinal);
    }

    [Fact]
    public void DirtyNavigationService_DoesNotWriteFilesOrTouchSaveCore()
    {
        string root = TestRepositoryRoot.Find();
        string serviceSource = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Services",
            "DirtyNavigation",
            "Ra2DirtyNavigationDialogService.cs"));
        string dialogCode = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Views",
            "DirtyNavigation",
            "Ra2DirtyNavigationDialog.xaml.cs"));
        string combined = serviceSource + Environment.NewLine + dialogCode;

        Assert.Contains("ShowDirtyNavigationDialog", combined, StringComparison.Ordinal);
        Assert.Contains("Ra2DirtyNavigationDecision.Cancel", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("File.WriteAllText", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("File.WriteAllBytes", combined, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IRa2SaveCurrentFileService", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("ProjectSaveService", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("IniFileService", combined, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveAll", combined, StringComparison.Ordinal);
    }

    [Fact]
    public void DirtyNavigationDecision_ExposesOnlySaveDiscardCancel()
    {
        string root = TestRepositoryRoot.Find();
        string source = File.ReadAllText(Path.Combine(
            root,
            "RA2IniEditor.IDE",
            "Services",
            "DirtyNavigation",
            "Ra2DirtyNavigationDecision.cs"));

        Assert.Contains("Save", source, StringComparison.Ordinal);
        Assert.Contains("Discard", source, StringComparison.Ordinal);
        Assert.Contains("Cancel", source, StringComparison.Ordinal);
        Assert.DoesNotContain("SaveAll", source, StringComparison.Ordinal);
        Assert.DoesNotContain("AutoSave", source, StringComparison.Ordinal);
    }
}

