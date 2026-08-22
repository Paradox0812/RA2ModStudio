using RA2IniEditor.IDE.Models;
using RA2IniEditor.IDE.ViewModels;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class ShellViewModelLayoutTests
{
    [Fact]
    public void ProjectExplorer_IsVisibleByDefault()
    {
        ShellViewModel viewModel = new();

        Assert.True(viewModel.IsProjectExplorerVisible);
    }

    [Fact]
    public void ToggleProjectExplorer_ChangesVisibilityWithoutClearingTreeState()
    {
        ShellViewModel viewModel = new();
        viewModel.ProjectExplorer.ShowFiles([new ReadonlyIniFileDescriptor("rules.ini", "C:\\game\\rules.ini", 42)]);
        ProjectExplorerItemViewModel fileNode = Assert.Single(viewModel.ProjectExplorer.Items);
        fileNode.Children.Add(new ProjectExplorerItemViewModel(ProjectExplorerItemKind.TypeGroup, "Infantry", "C:\\game\\rules.ini"));
        viewModel.ProjectExplorer.SelectedItem = fileNode;

        viewModel.ToggleProjectExplorer();

        Assert.False(viewModel.IsProjectExplorerVisible);
        Assert.Same(fileNode, Assert.Single(viewModel.ProjectExplorer.Items));
        Assert.Single(fileNode.Children);
        Assert.Same(fileNode, viewModel.ProjectExplorer.SelectedItem);

        viewModel.ToggleProjectExplorer();

        Assert.True(viewModel.IsProjectExplorerVisible);
        Assert.Same(fileNode, Assert.Single(viewModel.ProjectExplorer.Items));
        Assert.Single(fileNode.Children);
        Assert.Same(fileNode, viewModel.ProjectExplorer.SelectedItem);
    }

    [Fact]
    public void SetOperationStatus_UpdatesTextAndKind()
    {
        ShellViewModel viewModel = new();

        viewModel.SetOperationStatus("保存成功", "Success");

        Assert.Equal("保存成功", viewModel.StatusOperationText);
        Assert.Equal("Success", viewModel.StatusOperationKindText);
    }

    [Fact]
    public void UpdateDirtyStatus_DoesNotClearOperationStatus()
    {
        ShellViewModel viewModel = new();
        viewModel.SetOperationStatus("字段库已重新加载，包含 2 条警告", "Warning");

        viewModel.UpdateDirtyStatus("未保存");

        Assert.Equal("未保存", viewModel.StatusDirtyStateText);
        Assert.Equal("字段库已重新加载，包含 2 条警告", viewModel.StatusOperationText);
        Assert.Equal("Warning", viewModel.StatusOperationKindText);
    }
}
