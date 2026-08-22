using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.FieldRegistry;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class FieldRegistryManagerViewModelTests
{
    [Fact]
    public void RefreshFromState_PopulatesPackStatusesAndWarnings()
    {
        FieldRegistryRuntimeState state = FieldRegistryRuntimeState.FromLoadResults(
            "global",
            new LocalFieldRegistryLoadResult(
                [new Ra2FieldDefinition("GlobalKey", [Ra2SectionKind.Unknown], FieldEditorKind.Text, Ra2FieldSourceKind.User)],
                ["global warning"]),
            "project",
            new LocalFieldRegistryLoadResult(
                [new Ra2FieldDefinition("ProjectKey", [Ra2SectionKind.Building], FieldEditorKind.Text, Ra2FieldSourceKind.User)],
                ["project warning"]));
        FieldRegistryManagerViewModel viewModel = new();

        viewModel.RefreshFromState(state);

        Assert.Equal(2, viewModel.Packs.Count);
        Assert.Equal(["Global", "Project"], viewModel.Packs.Select(pack => pack.Scope).ToArray());
        Assert.Equal(["global warning", "project warning"], viewModel.Warnings.ToArray());
        Assert.True(viewModel.HasProject);
        Assert.Contains("2 个本地字段", viewModel.StatusText);
        Assert.Contains("2 条警告", viewModel.StatusText);
        Assert.Equal("2 条警告。", viewModel.WarningsStatusText);
        Assert.Equal("Project > Global > BuiltIn", viewModel.SourcePriorityText);
        Assert.Contains("2 个本地 active 来源", viewModel.LoadedPackSummaryText);
        Assert.Contains("project", viewModel.ProjectRegistryDisplayText);
        Assert.Contains("global", viewModel.GlobalRegistryDisplayText);
        Assert.Contains("BuiltIn", viewModel.BuiltInFallbackDisplayText);
        Assert.DoesNotContain("0 个字段", viewModel.BuiltInFallbackDisplayText, StringComparison.Ordinal);
        Assert.Equal("字段库警告：2 条，请查看警告列表。", viewModel.WarningSummaryText);
        Assert.Equal("警告 2", viewModel.WarningChipText);
        Assert.Equal("当前有 2 条警告，详见列表。", viewModel.WarningsEmptyStateText);
        Assert.Equal("项目字段库目录可用。", viewModel.ProjectFolderDisabledReason);
    }

    [Fact]
    public void RefreshFromState_NoProjectSetsHasProjectFalse()
    {
        FieldRegistryRuntimeState state = FieldRegistryRuntimeState.FromLoadResults(
            "global",
            new LocalFieldRegistryLoadResult([], []),
            null,
            null);
        FieldRegistryManagerViewModel viewModel = new();

        viewModel.RefreshFromState(state);

        Assert.Single(viewModel.Packs);
        Assert.False(viewModel.HasProject);
        Assert.Equal("已加载 0 个本地字段。", viewModel.StatusText);
        Assert.Equal("无警告。", viewModel.WarningsStatusText);
        Assert.Contains("未检测到项目 active 字段库", viewModel.ProjectRegistryDisplayText);
        Assert.Equal("字段库警告：无警告。", viewModel.WarningSummaryText);
        Assert.Equal("警告 0", viewModel.WarningChipText);
        Assert.Equal("当前没有字段库警告。", viewModel.WarningsEmptyStateText);
        Assert.Equal("未打开项目或当前项目没有 project active fields 目录。", viewModel.ProjectFolderDisabledReason);
    }

    [Fact]
    public void DisplayOnlySummaryProperties_DoNotFakeBuiltInPackCounts()
    {
        FieldRegistryRuntimeState state = FieldRegistryRuntimeState.FromLoadResults(
            "global",
            new LocalFieldRegistryLoadResult([], []),
            null,
            null);
        FieldRegistryManagerViewModel viewModel = new();

        viewModel.RefreshFromState(state);

        Assert.Single(viewModel.Packs);
        Assert.Equal("Project > Global > BuiltIn", viewModel.SourcePriorityText);
        Assert.Contains("BuiltIn 是内置参考与 fallback 来源", viewModel.BuiltInFallbackDisplayText);
        Assert.DoesNotContain("0", viewModel.BuiltInFallbackDisplayText, StringComparison.Ordinal);
        Assert.Equal("请选择一个状态为 Ready 的回滚备份清单。", viewModel.RollbackDisabledReason);
        Assert.Equal("内置保底", viewModel.BuiltInChipText);
        Assert.Equal("尚未构建清理预览；先使用“构建清理计划”。", viewModel.CleanupPreviewEmptyStateText);
        Assert.False(viewModel.HasCleanupPreviewDetails);
        Assert.Equal("尚未加载备份清单；使用“刷新备份”查看可回滚项。", viewModel.RollbackEmptyStateText);
        Assert.Contains("写入 active 字段包", viewModel.CleanupWriteWarningText);
    }

    [Fact]
    public void ShortPathDisplay_DoesNotReplaceFullPathTooltip()
    {
        string globalPath = Path.Combine("C:\\", "Users", "PC", "VeryLongProjectFolderName", "Nested", "GlobalFieldRegistry", "active");
        FieldRegistryRuntimeState state = FieldRegistryRuntimeState.FromLoadResults(
            globalPath,
            new LocalFieldRegistryLoadResult([], []),
            null,
            null);
        FieldRegistryManagerViewModel viewModel = new();

        viewModel.RefreshFromState(state);

        Assert.NotEqual(globalPath, viewModel.GlobalRegistryShortDisplayText);
        Assert.Equal(globalPath, viewModel.GlobalRegistryFullPathToolTip);
        FieldRegistryPackStatusViewModel pack = Assert.Single(viewModel.Packs);
        Assert.NotEqual(pack.DirectoryPath, pack.ShortDirectoryPath);
        Assert.Equal(pack.DirectoryPath, pack.DirectoryPathToolTip);
        Assert.Contains("Global", pack.StatusChipText, StringComparison.Ordinal);
    }

    [Fact]
    public void DisplayOnlyProperties_DoNotMutateCollections()
    {
        FieldRegistryRuntimeState state = FieldRegistryRuntimeState.FromLoadResults(
            "global",
            new LocalFieldRegistryLoadResult([], ["warning"]),
            null,
            null);
        FieldRegistryManagerViewModel viewModel = new();
        viewModel.RefreshFromState(state);
        int packCount = viewModel.Packs.Count;
        int warningCount = viewModel.Warnings.Count;
        int rollbackCount = viewModel.RollbackManifests.Count;
        int cleanupCount = viewModel.CleanupPlanRows.Count;

        _ = viewModel.SourcePriorityText;
        _ = viewModel.LoadedPackSummaryText;
        _ = viewModel.ProjectRegistryShortDisplayText;
        _ = viewModel.ProjectRegistryFullPathToolTip;
        _ = viewModel.GlobalRegistryShortDisplayText;
        _ = viewModel.GlobalRegistryFullPathToolTip;
        _ = viewModel.ActiveSourceChipText;
        _ = viewModel.WarningChipText;
        _ = viewModel.BuiltInChipText;
        _ = viewModel.WarningsEmptyStateText;
        _ = viewModel.CleanupPreviewEmptyStateText;
        _ = viewModel.HasCleanupPreviewDetails;
        _ = viewModel.RollbackEmptyStateText;
        _ = viewModel.CleanupWriteWarningText;

        Assert.Equal(packCount, viewModel.Packs.Count);
        Assert.Equal(warningCount, viewModel.Warnings.Count);
        Assert.Equal(rollbackCount, viewModel.RollbackManifests.Count);
        Assert.Equal(cleanupCount, viewModel.CleanupPlanRows.Count);
    }
}
