using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using RA2IniEditor.IDE.Services;
using RA2IniEditor.Infrastructure.FieldRegistry.Cleanup;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.Rollback;

namespace RA2IniEditor.IDE.ViewModels;

internal sealed class FieldRegistryManagerViewModel : INotifyPropertyChanged
{
    private readonly IFieldRegistryApplyBackupManifestReader _manifestReader;
    private readonly IFieldRegistryRollbackService _rollbackService;
    private readonly FieldRegistryGeneralizationCleanupPlanner _cleanupPlanner;
    private readonly FieldRegistryGeneralizationCleanupApplyWriter _cleanupApplyWriter;
    private bool _hasProject;
    private string _statusText = "本地字段库状态尚未加载。";
    private string _rollbackStatusText = "刷新备份后可查看可回滚项。";
    private string _cleanupStatusText = "构建清理计划后，可预览能概括到 Unit 或 Techno 下的重复具体字段。";
    private FieldRegistryRollbackManifestViewModel? _selectedRollbackManifest;
    private string _lastRollbackOperation = string.Empty;
    private string _lastRollbackTargetFilePath = string.Empty;
    private string _lastRollbackBackupFilePath = string.Empty;
    private string _lastRollbackManifestFilePath = string.Empty;
    private string _lastRollbackSummaryText = string.Empty;
    private FieldRegistryGeneralizationRepairPreview _repairPreview = FieldRegistryGeneralizationRepairPreview.Empty("user-import.fields.json");
    private string? _projectRootPath;
    private string _globalFieldRegistryRootPath = string.Empty;
    private bool _isRollbackInProgress;

    public FieldRegistryManagerViewModel()
        : this(
            new FieldRegistryApplyBackupManifestReader(),
            new FieldRegistryRollbackService(),
            new FieldRegistryGeneralizationCleanupPlanner(),
            new FieldRegistryGeneralizationCleanupApplyWriter())
    {
    }

    internal FieldRegistryManagerViewModel(
        IFieldRegistryApplyBackupManifestReader manifestReader,
        IFieldRegistryRollbackService rollbackService,
        FieldRegistryGeneralizationCleanupPlanner? cleanupPlanner = null,
        FieldRegistryGeneralizationCleanupApplyWriter? cleanupApplyWriter = null)
    {
        _manifestReader = manifestReader ?? throw new ArgumentNullException(nameof(manifestReader));
        _rollbackService = rollbackService ?? throw new ArgumentNullException(nameof(rollbackService));
        _cleanupPlanner = cleanupPlanner ?? new FieldRegistryGeneralizationCleanupPlanner();
        _cleanupApplyWriter = cleanupApplyWriter ?? new FieldRegistryGeneralizationCleanupApplyWriter();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<FieldRegistryPackStatusViewModel> Packs { get; } = new();

    public ObservableCollection<string> Warnings { get; } = new();

    public ObservableCollection<FieldRegistryRollbackManifestViewModel> RollbackManifests { get; } = new();

    public ObservableCollection<FieldRegistryCleanupPlanRowViewModel> CleanupPlanRows { get; } = new();

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string RollbackStatusText
    {
        get => _rollbackStatusText;
        private set => SetProperty(ref _rollbackStatusText, value);
    }

    public string CleanupStatusText
    {
        get => _cleanupStatusText;
        private set => SetProperty(ref _cleanupStatusText, value);
    }

    public bool HasProject
    {
        get => _hasProject;
        private set => SetProperty(ref _hasProject, value);
    }

    public string SourcePriorityText => "Project > Global > BuiltIn";

    public string LoadedPackSummaryText => Packs.Count == 0
        ? "尚未加载本地 active 字段库。"
        : $"已加载 {Packs.Count} 个本地 active 来源，{Warnings.Count} 条警告。";

    public string ProjectRegistryDisplayText
    {
        get
        {
            FieldRegistryPackStatusViewModel? projectPack = FindPack("Project");
            return projectPack is null
                ? "未检测到项目 active 字段库；将使用全局 active fields 和 BuiltIn fallback。"
                : $"{projectPack.DirectoryPath}（{projectPack.FieldCount} 个字段，{projectPack.WarningCount} 条警告）";
        }
    }

    public string ProjectRegistryShortDisplayText
    {
        get
        {
            FieldRegistryPackStatusViewModel? projectPack = FindPack("Project");
            return projectPack is null ? "未检测到项目 active" : projectPack.ShortDirectoryPath;
        }
    }

    public string ProjectRegistryFullPathToolTip
    {
        get
        {
            FieldRegistryPackStatusViewModel? projectPack = FindPack("Project");
            return projectPack is null ? ProjectRegistryDisplayText : projectPack.DirectoryPathToolTip;
        }
    }

    public string GlobalRegistryDisplayText
    {
        get
        {
            FieldRegistryPackStatusViewModel? globalPack = FindPack("Global");
            return globalPack is null
                ? "全局 active 字段库尚未加载。"
                : $"{globalPack.DirectoryPath}（{globalPack.FieldCount} 个字段，{globalPack.WarningCount} 条警告）";
        }
    }

    public string GlobalRegistryShortDisplayText
    {
        get
        {
            FieldRegistryPackStatusViewModel? globalPack = FindPack("Global");
            return globalPack is null ? "全局 active 未加载" : globalPack.ShortDirectoryPath;
        }
    }

    public string GlobalRegistryFullPathToolTip
    {
        get
        {
            FieldRegistryPackStatusViewModel? globalPack = FindPack("Global");
            return globalPack is null ? GlobalRegistryDisplayText : globalPack.DirectoryPathToolTip;
        }
    }

    public string BuiltInFallbackDisplayText => "BuiltIn 是内置参考与 fallback 来源，不计入本地 active pack 数。";

    public string ActiveSourceChipText => $"active 来源 {Packs.Count}";

    public string WarningChipText => $"警告 {Warnings.Count}";

    public string BuiltInChipText => "内置保底";

    public string CenterFieldListSummaryText => "双击或选择后编辑；此处不改变来源优先级。";

    public string WarningsStatusText => Warnings.Count == 0
        ? "无警告。"
        : $"{Warnings.Count} 条警告。";

    public string WarningSummaryText => Warnings.Count == 0
        ? "字段库警告：无警告。"
        : $"字段库警告：{Warnings.Count} 条，请查看警告列表。";

    public string WarningsEmptyStateText => Warnings.Count == 0
        ? "当前没有字段库警告。"
        : $"当前有 {Warnings.Count} 条警告，详见列表。";

    public string CleanupPreviewEmptyStateText => CleanupPlanRows.Count == 0
        ? "尚未构建清理预览；先使用“构建清理计划”。"
        : $"清理预览包含 {CleanupPlanRows.Count} 个候选项。";

    public bool HasCleanupPreviewDetails =>
        CleanupPlanRows.Count > 0 ||
        RepairPreview.AbstractFields.Count > 0 ||
        RepairPreview.RemovedConcreteFields.Count > 0 ||
        RepairPreview.SkippedFields.Count > 0 ||
        RepairPreview.Warnings.Count > 0;

    public string RollbackEmptyStateText => RollbackManifests.Count == 0
        ? "尚未加载备份清单；使用“刷新备份”查看可回滚项。"
        : $"已加载 {RollbackManifests.Count} 个备份清单。";

    public string CleanupWriteWarningText => "应用清理会写入 active 字段包，并保留现有确认与备份流程。";

    public string ProjectFolderDisabledReason => HasProject
        ? "项目字段库目录可用。"
        : "未打开项目或当前项目没有 project active fields 目录。";

    public string RollbackDisabledReason
    {
        get
        {
            if (CanRollbackSelected)
                return "所选备份可回滚；执行前仍会显示确认。";

            return SelectedRollbackManifest is null
                ? "请选择一个状态为 Ready 的回滚备份清单。"
                : $"当前备份不可回滚：{SelectedRollbackManifest.StatusMessage}";
        }
    }

    public FieldRegistryGeneralizationRepairPreview RepairPreview
    {
        get => _repairPreview;
        private set => SetProperty(ref _repairPreview, value);
    }

    public FieldRegistryRollbackManifestViewModel? SelectedRollbackManifest
    {
        get => _selectedRollbackManifest;
        set
        {
            if (SetProperty(ref _selectedRollbackManifest, value))
            {
                OnPropertyChanged(nameof(CanRollbackSelected));
                OnPropertyChanged(nameof(CanOpenRollbackTargetFolder));
                OnPropertyChanged(nameof(CanOpenRollbackManifestFolder));
                OnPropertyChanged(nameof(CanOpenRollbackBackupFolder));
                OnPropertyChanged(nameof(RollbackDisabledReason));
            }
        }
    }

    public bool CanRollbackSelected => SelectedRollbackManifest is { CanRollback: true } && !_isRollbackInProgress;

    public bool CanOpenRollbackTargetFolder => TryGetSelectedTargetFolderPath() is not null;

    public bool CanOpenRollbackManifestFolder => TryGetSelectedManifestFolderPath() is not null;

    public bool CanOpenRollbackBackupFolder => TryGetSelectedBackupFolderPath() is not null;

    public string LastRollbackOperation
    {
        get => _lastRollbackOperation;
        private set => SetProperty(ref _lastRollbackOperation, value);
    }

    public string LastRollbackTargetFilePath
    {
        get => _lastRollbackTargetFilePath;
        private set => SetProperty(ref _lastRollbackTargetFilePath, value);
    }

    public string LastRollbackBackupFilePath
    {
        get => _lastRollbackBackupFilePath;
        private set => SetProperty(ref _lastRollbackBackupFilePath, value);
    }

    public string LastRollbackManifestFilePath
    {
        get => _lastRollbackManifestFilePath;
        private set => SetProperty(ref _lastRollbackManifestFilePath, value);
    }

    public string LastRollbackSummaryText
    {
        get => _lastRollbackSummaryText;
        private set => SetProperty(ref _lastRollbackSummaryText, value);
    }

    public void RefreshFromState(FieldRegistryRuntimeState state)
    {
        Packs.Clear();
        Packs.Add(new FieldRegistryPackStatusViewModel(state.Global));
        if (state.Project is not null)
            Packs.Add(new FieldRegistryPackStatusViewModel(state.Project));

        Warnings.Clear();
        foreach (string warning in state.Warnings)
            Warnings.Add(warning);

        HasProject = state.Project is not null;
        StatusText = state.Warnings.Count > 0
            ? $"已加载 {state.TotalLocalFieldCount} 个本地字段，包含 {state.Warnings.Count} 条警告。"
            : $"已加载 {state.TotalLocalFieldCount} 个本地字段。";
        OnRegistryDisplayPropertiesChanged();
    }

    public void RefreshRollbackManifests(string? projectRootPath, string globalFieldRegistryRootPath)
    {
        _projectRootPath = projectRootPath;
        _globalFieldRegistryRootPath = globalFieldRegistryRootPath ?? throw new ArgumentNullException(nameof(globalFieldRegistryRootPath));
        RollbackManifests.Clear();
        SelectedRollbackManifest = null;

        List<FieldRegistryRollbackManifestViewModel> manifests = new();
        if (!string.IsNullOrWhiteSpace(projectRootPath))
            LoadManifestRows("项目", Path.Combine(projectRootPath, ".ra2inieditor", "field-registry", "backups"), manifests);

        LoadManifestRows("全局", Path.Combine(globalFieldRegistryRootPath, "backups"), manifests);

        foreach (FieldRegistryRollbackManifestViewModel manifest in manifests
            .OrderByDescending(manifest => manifest.TimestampUtc, StringComparer.OrdinalIgnoreCase)
            .ThenByDescending(manifest => manifest.ManifestFilePath, StringComparer.OrdinalIgnoreCase))
        {
            RollbackManifests.Add(manifest);
        }

        RollbackStatusText = RollbackManifests.Count == 0
            ? "未找到可回滚备份清单。"
            : $"已加载 {RollbackManifests.Count} 个回滚备份清单。";
        OnPropertyChanged(nameof(CanRollbackSelected));
        OnPropertyChanged(nameof(RollbackDisabledReason));
        OnPropertyChanged(nameof(RollbackEmptyStateText));
        OnPropertyChanged(nameof(WarningsStatusText));
        OnPropertyChanged(nameof(WarningSummaryText));
        OnPropertyChanged(nameof(WarningChipText));
        OnPropertyChanged(nameof(WarningsEmptyStateText));
    }

    public void BuildGeneralizationCleanupPlan()
    {
        CleanupPlanRows.Clear();
        if (string.IsNullOrWhiteSpace(_globalFieldRegistryRootPath))
        {
            RepairPreview = FieldRegistryGeneralizationRepairPreview.Empty("user-import.fields.json");
            CleanupStatusText = "构建清理计划前，请先重新加载或刷新字段库状态。";
            OnPropertyChanged(nameof(CleanupPreviewEmptyStateText));
            OnPropertyChanged(nameof(HasCleanupPreviewDetails));
            return;
        }

        string globalActiveDirectoryPath = Path.Combine(_globalFieldRegistryRootPath, "active");
        string? projectActiveDirectoryPath = string.IsNullOrWhiteSpace(_projectRootPath)
            ? null
            : Path.Combine(_projectRootPath, ".ra2inieditor", "field-registry", "active");
        FieldRegistryGeneralizationCleanupPlan plan = _cleanupPlanner.BuildPlan(
            new FieldRegistryGeneralizationCleanupRequest(globalActiveDirectoryPath, projectActiveDirectoryPath));

        foreach (FieldRegistryGeneralizationCleanupRow row in plan.Rows)
            CleanupPlanRows.Add(new FieldRegistryCleanupPlanRowViewModel(row));

        RepairPreview = _cleanupApplyWriter.BuildGlobalPreview(_globalFieldRegistryRootPath);

        foreach (string warning in plan.Warnings)
            Warnings.Add($"清理计划：{warning}");

        CleanupStatusText = CleanupPlanRows.Count == 0
            ? "未找到可安全概括到 Unit/Techno 的候选字段。"
            : $"已生成预览：{CleanupPlanRows.Count} 个清理候选项。此操作不会修改 active pack。";
        OnPropertyChanged(nameof(CleanupPreviewEmptyStateText));
        OnPropertyChanged(nameof(HasCleanupPreviewDetails));
        OnPropertyChanged(nameof(WarningsStatusText));
        OnPropertyChanged(nameof(WarningSummaryText));
        OnPropertyChanged(nameof(WarningChipText));
        OnPropertyChanged(nameof(WarningsEmptyStateText));
    }

    public string ApplyGeneralizationCleanupPlan()
    {
        if (CleanupPlanRows.Count == 0)
            BuildGeneralizationCleanupPlan();

        if (CleanupPlanRows.Count == 0)
            return "没有可用的清理候选项。";

        List<FieldRegistryGeneralizationCleanupApplyResult> results = new();
        results.Add(_cleanupApplyWriter.ApplyGlobal(_globalFieldRegistryRootPath));

        foreach (string warning in results.SelectMany(result => result.Warnings))
            Warnings.Add($"应用清理：{warning}");

        int added = results.Sum(result => result.AddedCount);
        int updated = results.Sum(result => result.UpdatedCount);
        int removed = results.Sum(result => result.RemovedCount);
        int skipped = results.Sum(result => result.SkippedCount);
        FieldRegistryGeneralizationCleanupApplyResult primaryResult = results[0];
        string backupText = string.IsNullOrWhiteSpace(primaryResult.BackupDirectoryPath)
            ? "无"
            : primaryResult.BackupDirectoryPath;
        string manifestText = string.IsNullOrWhiteSpace(primaryResult.ManifestFilePath)
            ? "无"
            : primaryResult.ManifestFilePath;
        CleanupStatusText = "字段库概括修复已完成。" +
                            $" 新增/更新抽象字段：{added + updated}；" +
                            $" 移除具体重复项：{removed}；跳过：{skipped}。" +
                            " 本轮仅处理默认 active pack：user-import.fields.json。" +
                            $" 备份：{backupText}。清单：{manifestText}。";
        RepairPreview = _cleanupApplyWriter.BuildGlobalPreview(_globalFieldRegistryRootPath);
        OnPropertyChanged(nameof(CleanupPreviewEmptyStateText));
        OnPropertyChanged(nameof(HasCleanupPreviewDetails));
        OnPropertyChanged(nameof(WarningsStatusText));
        OnPropertyChanged(nameof(WarningSummaryText));
        OnPropertyChanged(nameof(WarningChipText));
        OnPropertyChanged(nameof(WarningsEmptyStateText));
        return CleanupStatusText;
    }

    public FieldRegistryRollbackConfirmationViewModel? CreateRollbackConfirmation()
    {
        if (SelectedRollbackManifest is null)
        {
            RollbackStatusText = "请先选择一个回滚备份清单。";
            return null;
        }

        if (!SelectedRollbackManifest.CanRollback)
        {
            RollbackStatusText = $"当前备份不可回滚：{SelectedRollbackManifest.StatusMessage}";
            return null;
        }

        return new FieldRegistryRollbackConfirmationViewModel(SelectedRollbackManifest);
    }

    public FieldRegistryRollbackResult? RollbackSelectedConfirmed()
    {
        if (SelectedRollbackManifest is null)
        {
            RollbackStatusText = "请先选择一个回滚备份清单。";
            return null;
        }

        if (!SelectedRollbackManifest.CanRollback)
        {
            RollbackStatusText = $"当前备份不可回滚：{SelectedRollbackManifest.StatusMessage}";
            return null;
        }

        try
        {
            _isRollbackInProgress = true;
            OnPropertyChanged(nameof(CanRollbackSelected));
            OnPropertyChanged(nameof(RollbackDisabledReason));
            FieldRegistryRollbackResult result = _rollbackService.Rollback(new FieldRegistryRollbackRequest(
                SelectedRollbackManifest.ManifestFilePath,
                _projectRootPath,
                _globalFieldRegistryRootPath));
            ShowRollbackCompleted(result);
            return result;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException or NotSupportedException)
        {
            RollbackStatusText = $"回滚失败：{ex.Message}";
            return null;
        }
        finally
        {
            _isRollbackInProgress = false;
            OnPropertyChanged(nameof(CanRollbackSelected));
            OnPropertyChanged(nameof(RollbackDisabledReason));
        }
    }

    internal void ShowRollbackCompleted(FieldRegistryRollbackResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        LastRollbackOperation = result.OperationKind.ToString();
        LastRollbackTargetFilePath = result.TargetFilePath;
        LastRollbackBackupFilePath = result.BackupFilePath ?? string.Empty;
        LastRollbackManifestFilePath = result.ManifestFilePath;
        LastRollbackSummaryText =
            "回滚已完成。\n" +
            $"操作：{LastRollbackOperation}\n" +
            $"目标：{LastRollbackTargetFilePath}\n" +
            $"备份：{FormatOptionalPath(LastRollbackBackupFilePath)}\n" +
            $"清单：{LastRollbackManifestFilePath}\n" +
            result.Message;
        RollbackStatusText = LastRollbackSummaryText;
    }

    internal string? TryGetSelectedTargetFolderPath()
        => TryGetDirectoryName(SelectedRollbackManifest?.TargetFilePath);

    internal string? TryGetSelectedManifestFolderPath()
        => TryGetDirectoryName(SelectedRollbackManifest?.ManifestFilePath);

    internal string? TryGetSelectedBackupFolderPath()
    {
        if (SelectedRollbackManifest is null)
            return null;

        string? backupFolderPath = TryGetDirectoryName(SelectedRollbackManifest.BackupFilePath);
        return backupFolderPath ?? TryGetSelectedManifestFolderPath();
    }

    internal void ShowRollbackFolderOpenFailed(string label, Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        RollbackStatusText = $"打开{label}目录失败：{exception.Message}";
    }

    internal void ShowRollbackFolderOpened(string label, string directoryPath)
        => RollbackStatusText = $"已打开{label}目录：{directoryPath}";

    private void LoadManifestRows(
        string fallbackScope,
        string backupRootDirectoryPath,
        List<FieldRegistryRollbackManifestViewModel> rows)
    {
        IReadOnlyList<string> manifestFilePaths;
        try
        {
            manifestFilePaths = _manifestReader.FindManifestFiles(backupRootDirectoryPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException)
        {
            Warnings.Add($"无法读取回滚备份清单：{backupRootDirectoryPath}（{ex.Message}）");
            return;
        }

        foreach (string manifestFilePath in manifestFilePaths)
        {
            try
            {
                FieldRegistryApplyBackupManifest manifest = _manifestReader.Read(manifestFilePath);
                rows.Add(new FieldRegistryRollbackManifestViewModel(manifestFilePath, manifest, fallbackScope));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException)
            {
                rows.Add(FieldRegistryRollbackManifestViewModel.CreateInvalid(
                    fallbackScope,
                    manifestFilePath,
                    FieldRegistryRollbackManifestStatus.Malformed,
                    $"清单无法读取：{ex.Message}"));
            }
        }
    }

    private static string? TryGetDirectoryName(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            return Path.GetDirectoryName(Path.GetFullPath(path));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return null;
        }
    }

    private static string FormatOptionalPath(string path)
        => string.IsNullOrWhiteSpace(path) ? "无" : path;

    private FieldRegistryPackStatusViewModel? FindPack(string scope)
        => Packs.FirstOrDefault(pack => string.Equals(pack.Scope, scope, StringComparison.OrdinalIgnoreCase));

    private void OnRegistryDisplayPropertiesChanged()
    {
        OnPropertyChanged(nameof(SourcePriorityText));
        OnPropertyChanged(nameof(LoadedPackSummaryText));
        OnPropertyChanged(nameof(ProjectRegistryDisplayText));
        OnPropertyChanged(nameof(ProjectRegistryShortDisplayText));
        OnPropertyChanged(nameof(ProjectRegistryFullPathToolTip));
        OnPropertyChanged(nameof(GlobalRegistryDisplayText));
        OnPropertyChanged(nameof(GlobalRegistryShortDisplayText));
        OnPropertyChanged(nameof(GlobalRegistryFullPathToolTip));
        OnPropertyChanged(nameof(BuiltInFallbackDisplayText));
        OnPropertyChanged(nameof(ActiveSourceChipText));
        OnPropertyChanged(nameof(WarningChipText));
        OnPropertyChanged(nameof(BuiltInChipText));
        OnPropertyChanged(nameof(CenterFieldListSummaryText));
        OnPropertyChanged(nameof(WarningsStatusText));
        OnPropertyChanged(nameof(WarningSummaryText));
        OnPropertyChanged(nameof(WarningsEmptyStateText));
        OnPropertyChanged(nameof(CleanupPreviewEmptyStateText));
        OnPropertyChanged(nameof(HasCleanupPreviewDetails));
        OnPropertyChanged(nameof(RollbackEmptyStateText));
        OnPropertyChanged(nameof(ProjectFolderDisabledReason));
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

internal sealed class FieldRegistryRollbackManifestViewModel
{
    private const string TargetPackFileName = "user-import.fields.json";

    public FieldRegistryRollbackManifestViewModel(
        string manifestFilePath,
        FieldRegistryApplyBackupManifest manifest,
        string fallbackScope)
    {
        ManifestFilePath = manifestFilePath ?? throw new ArgumentNullException(nameof(manifestFilePath));
        Scope = string.IsNullOrWhiteSpace(manifest.TargetScope) ? fallbackScope : manifest.TargetScope;
        TimestampUtc = manifest.TimestampUtc;
        TargetFilePath = manifest.TargetFilePath;
        BackupFilePath = manifest.BackupFilePath ?? string.Empty;
        TargetFileExisted = manifest.TargetFileExisted;
        AddCount = manifest.AddCount;
        UpdateCount = manifest.UpdateCount;
        SkipCount = manifest.SkipCount;
        Mode = manifest.Mode;
        (Status, StatusMessage, CanRollback) = DetermineStatus(this);
    }

    private FieldRegistryRollbackManifestViewModel(
        string fallbackScope,
        string manifestFilePath,
        FieldRegistryRollbackManifestStatus status,
        string statusMessage)
    {
        ManifestFilePath = manifestFilePath ?? throw new ArgumentNullException(nameof(manifestFilePath));
        Scope = fallbackScope;
        TimestampUtc = string.Empty;
        TargetFilePath = string.Empty;
        BackupFilePath = string.Empty;
        TargetFileExisted = false;
        AddCount = 0;
        UpdateCount = 0;
        SkipCount = 0;
        Mode = string.Empty;
        Status = status.ToString();
        StatusMessage = statusMessage ?? throw new ArgumentNullException(nameof(statusMessage));
        CanRollback = false;
    }

    public string Scope { get; }

    public string Status { get; }

    public string StatusMessage { get; }

    public bool CanRollback { get; }

    public string TimestampUtc { get; }

    public string TargetFilePath { get; }

    public string BackupFilePath { get; }

    public bool TargetFileExisted { get; }

    public int AddCount { get; }

    public int UpdateCount { get; }

    public int SkipCount { get; }

    public string Mode { get; }

    public string ManifestFilePath { get; }

    public string TargetFileName => Path.GetFileName(TargetFilePath);

    public static FieldRegistryRollbackManifestViewModel CreateInvalid(
        string fallbackScope,
        string manifestFilePath,
        FieldRegistryRollbackManifestStatus status,
        string statusMessage)
        => new(fallbackScope, manifestFilePath, status, statusMessage);

    private static (string Status, string StatusMessage, bool CanRollback) DetermineStatus(
        FieldRegistryRollbackManifestViewModel manifest)
    {
        if (string.IsNullOrWhiteSpace(manifest.ManifestFilePath))
            return (FieldRegistryRollbackManifestStatus.InvalidPath.ToString(), "清单路径为空。", false);

        if (string.IsNullOrWhiteSpace(manifest.TargetFilePath))
            return (FieldRegistryRollbackManifestStatus.InvalidPath.ToString(), "目标文件路径为空。", false);

        string targetFileName = Path.GetFileName(manifest.TargetFilePath);
        if (!string.Equals(targetFileName, TargetPackFileName, StringComparison.OrdinalIgnoreCase))
        {
            return (
                FieldRegistryRollbackManifestStatus.UnsupportedTarget.ToString(),
                $"回滚仅支持 {TargetPackFileName}。",
                false);
        }

        if (manifest.TargetFileExisted)
        {
            if (string.IsNullOrWhiteSpace(manifest.BackupFilePath))
                return (FieldRegistryRollbackManifestStatus.MissingBackup.ToString(), "备份文件路径缺失。", false);

            if (!File.Exists(manifest.BackupFilePath))
                return (FieldRegistryRollbackManifestStatus.MissingBackup.ToString(), "备份文件不存在。", false);
        }
        else if (!File.Exists(manifest.TargetFilePath))
        {
            return (
                FieldRegistryRollbackManifestStatus.Ready.ToString(),
                "就绪。目标文件已不存在，回滚将不会写入内容。",
                true);
        }

        return (FieldRegistryRollbackManifestStatus.Ready.ToString(), "就绪。", true);
    }
}

internal sealed class FieldRegistryCleanupPlanRowViewModel
{
    public FieldRegistryCleanupPlanRowViewModel(FieldRegistryGeneralizationCleanupRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        Scope = row.Scope;
        Key = row.Key;
        TargetSection = row.TargetSectionKind.ToString();
        SourceSections = string.Join(", ", row.SourceSectionKinds);
        SourceFiles = string.Join(", ", row.SourceFileNames);
        EditorKind = row.EditorKind.ToString();
        ValueKind = row.ValueKind.ToString();
        SourceFieldCount = row.SourceFieldCount;
        MergedAllowedValueCount = row.MergedAllowedValueCount;
        ActionText = row.ActionText;
    }

    public string Scope { get; }

    public string Key { get; }

    public string TargetSection { get; }

    public string SourceSections { get; }

    public string SourceFiles { get; }

    public string EditorKind { get; }

    public string ValueKind { get; }

    public int SourceFieldCount { get; }

    public int MergedAllowedValueCount { get; }

    public string ActionText { get; }
}

internal enum FieldRegistryRollbackManifestStatus
{
    Ready,
    Malformed,
    MissingBackup,
    UnsupportedTarget,
    InvalidPath,
    MissingTarget,
    UnknownError
}

internal sealed class FieldRegistryRollbackConfirmationViewModel
{
    public FieldRegistryRollbackConfirmationViewModel(FieldRegistryRollbackManifestViewModel manifest)
    {
        Title = "回滚字段库导入";
        Message =
            "即将回滚一次字段库导入。\n\n" +
            $"范围：{manifest.Scope}\n" +
            $"目标文件：{manifest.TargetFilePath}\n" +
            $"备份文件：{FormatBackupFile(manifest.BackupFilePath)}\n" +
            $"目标文件原本存在：{manifest.TargetFileExisted}\n" +
            $"时间戳：{manifest.TimestampUtc}\n\n" +
            "该操作会根据所选清单恢复或删除目标 active pack。\n\n" +
            (manifest.TargetFileExisted
                ? "本次回滚会从备份恢复目标文件。"
                : "本次回滚会删除应用操作创建的目标文件。");
    }

    public string Title { get; }

    public string Message { get; }

    private static string FormatBackupFile(string backupFilePath)
        => string.IsNullOrWhiteSpace(backupFilePath) ? "无" : backupFilePath;
}
