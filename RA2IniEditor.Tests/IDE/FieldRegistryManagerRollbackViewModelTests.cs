using RA2IniEditor.IDE.ViewModels;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.Rollback;
using Xunit;

namespace RA2IniEditor.Tests.IDE;

public sealed class FieldRegistryManagerRollbackViewModelTests
{
    [Fact]
    public void RefreshRollbackManifestsLoadsProjectAndGlobalManifests()
    {
        FakeManifestReader reader = new();
        reader.Add("project", ProjectManifest("Project", "2026-05-25T12:00:00.0000000Z"));
        reader.Add("global", ProjectManifest("Global", "2026-05-25T13:00:00.0000000Z"));
        FieldRegistryManagerViewModel viewModel = new(reader, new FakeRollbackService());

        viewModel.RefreshRollbackManifests("project", "global");

        Assert.Equal(2, viewModel.RollbackManifests.Count);
        Assert.Equal("Global", viewModel.RollbackManifests[0].Scope);
        Assert.Equal("Project", viewModel.RollbackManifests[1].Scope);
        Assert.Contains("已加载 2 个回滚备份清单", viewModel.RollbackStatusText);
    }

    [Fact]
    public void RefreshRollbackManifestsWithoutProjectSkipsProjectManifests()
    {
        FakeManifestReader reader = new();
        reader.Add("project", ProjectManifest("Project", "2026-05-25T12:00:00.0000000Z"));
        reader.Add("global", ProjectManifest("Global", "2026-05-25T13:00:00.0000000Z"));
        FieldRegistryManagerViewModel viewModel = new(reader, new FakeRollbackService());

        viewModel.RefreshRollbackManifests(null, "global");

        FieldRegistryRollbackManifestViewModel manifest = Assert.Single(viewModel.RollbackManifests);
        Assert.Equal("Global", manifest.Scope);
    }

    [Fact]
    public void SelectedManifestEnablesRollback()
    {
        FakeManifestReader reader = new();
        reader.Add("project", ProjectManifest("Project", "2026-05-25T12:00:00.0000000Z"));
        FieldRegistryManagerViewModel viewModel = new(reader, new FakeRollbackService());
        viewModel.RefreshRollbackManifests("project", "global");

        Assert.False(viewModel.CanRollbackSelected);
        viewModel.SelectedRollbackManifest = viewModel.RollbackManifests[0];

        Assert.True(viewModel.CanRollbackSelected);
    }

    [Fact]
    public void RollbackSuccessCallsRollbackServiceOnceAndUpdatesStatus()
    {
        FakeManifestReader reader = new();
        reader.Add("project", ProjectManifest("Project", "2026-05-25T12:00:00.0000000Z"));
        FakeRollbackService rollbackService = new()
        {
            ResultOperationKind = FieldRegistryRollbackOperationKind.RestoreBackup,
            ResultTargetFilePath = "C:\\project\\.ra2inieditor\\field-registry\\active\\user-import.fields.json",
            ResultBackupFilePath = "C:\\project\\.ra2inieditor\\field-registry\\backups\\batch\\user-import.fields.json"
        };
        FieldRegistryManagerViewModel viewModel = new(reader, rollbackService);
        viewModel.RefreshRollbackManifests("project", "global");
        viewModel.SelectedRollbackManifest = viewModel.RollbackManifests[0];

        FieldRegistryRollbackResult? result = viewModel.RollbackSelectedConfirmed();

        Assert.NotNull(result);
        Assert.Equal(1, rollbackService.CallCount);
        Assert.Contains("回滚已完成", viewModel.RollbackStatusText);
        Assert.Equal("RestoreBackup", viewModel.LastRollbackOperation);
        Assert.Equal(rollbackService.ResultTargetFilePath, viewModel.LastRollbackTargetFilePath);
        Assert.Equal(rollbackService.ResultBackupFilePath, viewModel.LastRollbackBackupFilePath);
        Assert.Equal(viewModel.RollbackManifests[0].ManifestFilePath, viewModel.LastRollbackManifestFilePath);
        Assert.Contains("操作：RestoreBackup", viewModel.LastRollbackSummaryText);
        Assert.Contains("清单：", viewModel.LastRollbackSummaryText);
    }

    [Fact]
    public void RollbackFailureUpdatesStatusAndDoesNotReturnResult()
    {
        FakeManifestReader reader = new();
        reader.Add("project", ProjectManifest("Project", "2026-05-25T12:00:00.0000000Z"));
        FakeRollbackService rollbackService = new() { ThrowOnRollback = true };
        FieldRegistryManagerViewModel viewModel = new(reader, rollbackService);
        viewModel.RefreshRollbackManifests("project", "global");
        viewModel.SelectedRollbackManifest = viewModel.RollbackManifests[0];

        FieldRegistryRollbackResult? result = viewModel.RollbackSelectedConfirmed();

        Assert.Null(result);
        Assert.Equal(1, rollbackService.CallCount);
        Assert.Contains("回滚失败", viewModel.RollbackStatusText);
        Assert.NotNull(viewModel.SelectedRollbackManifest);
    }

    [Fact]
    public void RefreshAfterRollbackReloadsManifestList()
    {
        FakeManifestReader reader = new();
        reader.Add("project", ProjectManifest("Project", "2026-05-25T12:00:00.0000000Z"));
        FieldRegistryManagerViewModel viewModel = new(reader, new FakeRollbackService());
        viewModel.RefreshRollbackManifests("project", "global");

        reader.Add("project", ProjectManifest("Project", "2026-05-25T13:00:00.0000000Z"));
        viewModel.RefreshRollbackManifests("project", "global");

        Assert.Equal(2, viewModel.RollbackManifests.Count);
    }

    [Fact]
    public void RefreshRollbackManifestsWhenManifestEnumerationFailsKeepsWindowUsable()
    {
        FakeManifestReader reader = new() { ThrowOnFindForGlobal = true };
        reader.Add("project", ProjectManifest("Project", "2026-05-25T12:00:00.0000000Z"));
        FieldRegistryManagerViewModel viewModel = new(reader, new FakeRollbackService());

        viewModel.RefreshRollbackManifests("project", "global");

        FieldRegistryRollbackManifestViewModel manifest = Assert.Single(viewModel.RollbackManifests);
        Assert.Equal("Project", manifest.Scope);
        Assert.Contains(viewModel.Warnings, warning => warning.Contains("无法读取回滚备份清单", StringComparison.OrdinalIgnoreCase));
        Assert.Contains("已加载 1 个回滚备份清单", viewModel.RollbackStatusText);
    }

    [Fact]
    public void MalformedManifestCreatesNonRollbackableRow()
    {
        FakeManifestReader reader = new();
        reader.AddBroken("project", "broken-manifest");
        FieldRegistryManagerViewModel viewModel = new(reader, new FakeRollbackService());

        viewModel.RefreshRollbackManifests("project", "global");

        FieldRegistryRollbackManifestViewModel manifest = Assert.Single(viewModel.RollbackManifests);
        Assert.Equal("Malformed", manifest.Status);
        Assert.False(manifest.CanRollback);
        Assert.Contains("无法读取", manifest.StatusMessage, StringComparison.OrdinalIgnoreCase);
        viewModel.SelectedRollbackManifest = manifest;
        Assert.False(viewModel.CanRollbackSelected);
        Assert.Null(viewModel.CreateRollbackConfirmation());
        Assert.Contains("不可回滚", viewModel.RollbackStatusText);
    }

    [Fact]
    public void MissingBackupDisablesRollbackAndDoesNotCallService()
    {
        FakeManifestReader reader = new();
        reader.Add("project", ExistingTargetManifest(
            "Project",
            "2026-05-25T12:00:00.0000000Z",
            "C:\\project\\.ra2inieditor\\field-registry\\backups\\missing\\user-import.fields.json"));
        FakeRollbackService rollbackService = new();
        FieldRegistryManagerViewModel viewModel = new(reader, rollbackService);

        viewModel.RefreshRollbackManifests("project", "global");
        viewModel.SelectedRollbackManifest = viewModel.RollbackManifests[0];
        FieldRegistryRollbackResult? result = viewModel.RollbackSelectedConfirmed();

        Assert.Null(result);
        Assert.Equal("MissingBackup", viewModel.SelectedRollbackManifest.Status);
        Assert.False(viewModel.SelectedRollbackManifest.CanRollback);
        Assert.False(viewModel.CanRollbackSelected);
        Assert.Equal(0, rollbackService.CallCount);
        Assert.Contains("不可回滚", viewModel.RollbackStatusText);
    }

    [Fact]
    public void UnsupportedTargetDisablesRollback()
    {
        FakeManifestReader reader = new();
        reader.Add("project", new FieldRegistryApplyBackupManifest(
            "Project",
            "C:\\project\\.ra2inieditor\\field-registry\\active\\third-party.fields.json",
            null,
            false,
            "2026-05-25T12:00:00.0000000Z",
            1,
            0,
            0,
            "AppendOrUpdate"));
        FieldRegistryManagerViewModel viewModel = new(reader, new FakeRollbackService());

        viewModel.RefreshRollbackManifests("project", "global");

        FieldRegistryRollbackManifestViewModel manifest = Assert.Single(viewModel.RollbackManifests);
        Assert.Equal("UnsupportedTarget", manifest.Status);
        Assert.False(manifest.CanRollback);
    }

    [Fact]
    public void OpenFolderStatusHelpersReportSuccessAndFailure()
    {
        FieldRegistryManagerViewModel viewModel = new(new FakeManifestReader(), new FakeRollbackService());

        viewModel.ShowRollbackFolderOpened("目标", "C:\\project");
        Assert.Contains("已打开目标目录", viewModel.RollbackStatusText);

        viewModel.ShowRollbackFolderOpenFailed("目标", new IOException("simulated open failure"));
        Assert.Contains("打开目标目录失败", viewModel.RollbackStatusText);
    }

    private static FieldRegistryApplyBackupManifest ProjectManifest(string scope, string timestamp)
    {
        return new FieldRegistryApplyBackupManifest(
            scope,
            "C:\\project\\.ra2inieditor\\field-registry\\active\\user-import.fields.json",
            null,
            false,
            timestamp,
            1,
            0,
            0,
            "AppendOrUpdate");
    }

    private static FieldRegistryApplyBackupManifest ExistingTargetManifest(string scope, string timestamp, string backupFilePath)
    {
        return new FieldRegistryApplyBackupManifest(
            scope,
            "C:\\project\\.ra2inieditor\\field-registry\\active\\user-import.fields.json",
            backupFilePath,
            true,
            timestamp,
            1,
            1,
            0,
            "AppendOrUpdate");
    }

    private sealed class FakeManifestReader : IFieldRegistryApplyBackupManifestReader
    {
        private readonly Dictionary<string, FieldRegistryApplyBackupManifest> _manifests = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _brokenManifestPaths = new(StringComparer.OrdinalIgnoreCase);

        public bool ThrowOnFindForGlobal { get; init; }

        public void Add(string rootKey, FieldRegistryApplyBackupManifest manifest)
        {
            string manifestPath = Path.Combine(rootKey, manifest.TimestampUtc, "manifest.json");
            _manifests[manifestPath] = manifest;
        }

        public void AddBroken(string rootKey, string batchName)
        {
            string manifestPath = Path.Combine(rootKey, batchName, "manifest.json");
            _brokenManifestPaths.Add(manifestPath);
        }

        public FieldRegistryApplyBackupManifest Read(string manifestFilePath)
        {
            if (_brokenManifestPaths.Contains(manifestFilePath))
                throw new InvalidOperationException("simulated malformed manifest");

            return _manifests[manifestFilePath];
        }

        public IReadOnlyList<string> FindManifestFiles(string backupRootDirectoryPath)
        {
            string rootKey = backupRootDirectoryPath.Contains("global", StringComparison.OrdinalIgnoreCase)
                ? "global"
                : "project";
            if (ThrowOnFindForGlobal && rootKey == "global")
                throw new UnauthorizedAccessException("simulated backup directory access denied");

            return _manifests.Keys.Concat(_brokenManifestPaths)
                .Where(path => path.StartsWith(rootKey, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(path => path, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }

    private sealed class FakeRollbackService : IFieldRegistryRollbackService
    {
        public int CallCount { get; private set; }

        public bool ThrowOnRollback { get; init; }

        public FieldRegistryRollbackOperationKind ResultOperationKind { get; init; } = FieldRegistryRollbackOperationKind.DeleteCreatedTarget;

        public string ResultTargetFilePath { get; init; } = "target";

        public string? ResultBackupFilePath { get; init; }

        public FieldRegistryRollbackResult Rollback(FieldRegistryRollbackRequest request)
        {
            CallCount++;
            if (ThrowOnRollback)
                throw new InvalidOperationException("simulated rollback failure");

            return new FieldRegistryRollbackResult(
                true,
                ResultOperationKind,
                request.ManifestFilePath,
                ResultTargetFilePath,
                ResultBackupFilePath,
                "simulated rollback success");
        }
    }
}
