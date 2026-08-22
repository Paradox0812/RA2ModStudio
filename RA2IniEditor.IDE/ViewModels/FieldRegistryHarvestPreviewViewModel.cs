using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.Infrastructure.FieldRegistry;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply;
using RA2IniEditor.Infrastructure.FieldRegistry.Apply.IO;
using RA2IniEditor.Infrastructure.FieldRegistry.Fetch;
using RA2IniEditor.Infrastructure.FieldRegistry.Generalization;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest;
using RA2IniEditor.Infrastructure.FieldRegistry.Harvest.Ini;
using RA2IniEditor.Infrastructure.FieldRegistry.Provenance;

namespace RA2IniEditor.IDE.ViewModels;

internal sealed class FieldRegistryHarvestPreviewViewModel : INotifyPropertyChanged
{
    private readonly IFieldRegistryHarvestParser _parser;
    private readonly IFieldRegistryHarvestNormalizer _normalizer;
    private readonly IFieldRegistryHarvestPreviewBuilder _previewBuilder;
    private readonly IFieldRegistryHarvestDiffService _diffService;
    private readonly Func<IFieldRegistryProvenanceProvider> _provenanceProviderAccessor;
    private readonly IFieldRegistryApplyPlanBuilder _applyPlanBuilder;
    private readonly IFieldRegistryApplyWriter _applyWriter;
    private readonly IFieldRegistryRawFetcher _rawFetcher;
    private readonly IFieldRegistryRemoteSourceHistoryStore _remoteSourceHistoryStore;
    private readonly IFieldRegistryRemoteSourcePresetStore _remoteSourcePresetStore;
    private readonly IRa2IniFieldHarvester _iniFieldHarvester;
    private readonly IRa2FieldImportDraftBuilder _iniDraftBuilder;
    private readonly Ra2FieldDraftGeneralizationPipeline _generalizationPipeline = new();
    private readonly Func<string?> _projectRootPathAccessor;
    private readonly Func<string> _globalFieldRegistryRootPathAccessor;
    private readonly Action? _reloadAfterApply;
    private CancellationTokenSource? _fetchCancellationTokenSource;
    private string _sourceName = "pasted-field-doc";
    private string _rawText = string.Empty;
    private string _fetchUrl = string.Empty;
    private string _fetchStatusText = "获取原始文本是可选操作。输入 GitHub blob 或 raw.githubusercontent.com URL，然后点击“获取原始文本”。";
    private string _remoteHistoryStatusText = "远程来源历史保存在本地。获取或刷新历史后可加载最近来源。";
    private string _remotePresetStatusText = "远程预设是本地 URL 书签。“使用预设 URL”不会发起网络请求。";
    private string _currentIniHarvestStatusText = "尚未加载当前 INI 的字段采集结果。";
    private string _statusText = "粘贴字段文档、INI 风格行、项目符号列表或 Markdown 表格。“解析并预览”会验证候选字段、对比当前字段库，并且只有在构建计划和确认后才会应用。";
    private string _applyStatusText = "请先解析并预览，再构建应用计划。";
    private string _targetFilePreviewText = "目标文件：user-import.fields.json";
    private string _lastApplyTargetFilePath = string.Empty;
    private string _lastApplyBackupManifestPath = string.Empty;
    private string _lastApplySummaryText = "本次预览会话尚未完成应用。";
    private bool _canApplyInFuture;
    private FieldRegistryApplyTargetScope _selectedTargetScope = FieldRegistryApplyTargetScope.Global;
    private FieldRegistryApplyMode _selectedApplyMode = FieldRegistryApplyMode.AppendOrUpdate;
    private FieldRegistryHarvestPreviewDraft? _currentPreviewDraft;
    private FieldRegistryHarvestDiffResult? _currentDiffResult;
    private FieldRegistryApplyPlan? _currentApplyPlan;
    private bool _isFetchingRawText;
    private FieldRegistryRemoteSourceHistoryEntryViewModel? _selectedRemoteHistoryEntry;
    private FieldRegistryRemoteSourcePresetViewModel? _selectedRemotePreset;

    private const string SampleText = """
        # Basic sample: INI-like lines and bullet fields
        Owner=
        Strength=600
        Cost=500
        - MyCustomKey: custom local field

        # Table sample: markdown field table
        | Key | AppliesTo | Type | Description |
        | --- | --- | --- | --- |
        | Ares.CustomTag | Infantry | Text | Example imported field |
        """;

    public FieldRegistryHarvestPreviewViewModel()
        : this(
            new MarkdownFieldRegistryHarvestParser(),
            new FieldRegistryHarvestNormalizer(),
            new FieldRegistryHarvestPreviewBuilder(),
            new FieldRegistryHarvestDiffService(),
            () => new FieldRegistryProvenanceSnapshotBuilder().Build(
                new LocalFieldRegistryLoadResult([], []),
                null,
                new BuiltInRa2FieldDefinitionProvider()),
            new FieldRegistryApplyPlanBuilder(),
            new FieldRegistryApplyWriter(),
            () => null,
            CreateDefaultGlobalFieldRegistryRootPath,
            null)
    {
    }

    public FieldRegistryHarvestPreviewViewModel(
        IFieldRegistryHarvestParser parser,
        IFieldRegistryHarvestNormalizer normalizer,
        IFieldRegistryHarvestPreviewBuilder previewBuilder,
        IFieldRegistryHarvestDiffService diffService,
        Func<IFieldRegistryProvenanceProvider> provenanceProviderAccessor)
        : this(
            parser,
            normalizer,
            previewBuilder,
            diffService,
            provenanceProviderAccessor,
            new FieldRegistryApplyPlanBuilder(),
            new FieldRegistryApplyWriter(),
            () => null,
            CreateDefaultGlobalFieldRegistryRootPath,
            null)
    {
    }

    public FieldRegistryHarvestPreviewViewModel(
        IFieldRegistryHarvestParser parser,
        IFieldRegistryHarvestNormalizer normalizer,
        IFieldRegistryHarvestPreviewBuilder previewBuilder,
        IFieldRegistryHarvestDiffService diffService,
        Func<IFieldRegistryProvenanceProvider> provenanceProviderAccessor,
        IFieldRegistryApplyPlanBuilder applyPlanBuilder,
        IFieldRegistryApplyWriter applyWriter,
        Func<string?> projectRootPathAccessor,
        Func<string> globalFieldRegistryRootPathAccessor,
        Action? reloadAfterApply,
        IFieldRegistryRawFetcher? rawFetcher = null,
        IFieldRegistryRemoteSourceHistoryStore? remoteSourceHistoryStore = null,
        IFieldRegistryRemoteSourcePresetStore? remoteSourcePresetStore = null,
        IRa2IniFieldHarvester? iniFieldHarvester = null,
        IRa2FieldImportDraftBuilder? iniDraftBuilder = null)
    {
        _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        _normalizer = normalizer ?? throw new ArgumentNullException(nameof(normalizer));
        _previewBuilder = previewBuilder ?? throw new ArgumentNullException(nameof(previewBuilder));
        _diffService = diffService ?? throw new ArgumentNullException(nameof(diffService));
        _provenanceProviderAccessor = provenanceProviderAccessor ?? throw new ArgumentNullException(nameof(provenanceProviderAccessor));
        _applyPlanBuilder = applyPlanBuilder ?? throw new ArgumentNullException(nameof(applyPlanBuilder));
        _applyWriter = applyWriter ?? throw new ArgumentNullException(nameof(applyWriter));
        _rawFetcher = rawFetcher ?? FieldRegistryRawFetcherFactory.CreateDefault();
        _remoteSourceHistoryStore = remoteSourceHistoryStore ?? new FieldRegistryRemoteSourceHistoryStore();
        _remoteSourcePresetStore = remoteSourcePresetStore ?? new FieldRegistryRemoteSourcePresetStore();
        _iniFieldHarvester = iniFieldHarvester ?? new Ra2IniFieldHarvester();
        _iniDraftBuilder = iniDraftBuilder ?? new Ra2FieldImportDraftBuilder();
        _projectRootPathAccessor = projectRootPathAccessor ?? throw new ArgumentNullException(nameof(projectRootPathAccessor));
        _globalFieldRegistryRootPathAccessor = globalFieldRegistryRootPathAccessor ?? throw new ArgumentNullException(nameof(globalFieldRegistryRootPathAccessor));
        _reloadAfterApply = reloadAfterApply;
        UpdateTargetFilePreviewText();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string SourceName
    {
        get => _sourceName;
        set
        {
            if (SetProperty(ref _sourceName, value))
            {
                OnPropertyChanged(nameof(LearningWindowTitle));
                OnPropertyChanged(nameof(LearningSourceSummaryText));
            }
        }
    }

    public string LearningWindowTitle
    {
        get
        {
            string sourceName = NormalizeSourceNameForDisplay(SourceName);
            return string.IsNullOrEmpty(sourceName)
                ? "字段学习"
                : $"字段学习 - {sourceName}";
        }
    }

    public string LearningSourceSummaryText
    {
        get
        {
            string sourceName = NormalizeSourceNameForDisplay(SourceName);
            return string.IsNullOrEmpty(sourceName)
                ? "学习来源：未选择"
                : $"学习来源：{sourceName}";
        }
    }
    public string RawText
    {
        get => _rawText;
        set => SetProperty(ref _rawText, value);
    }

    public string FetchUrl
    {
        get => _fetchUrl;
        set
        {
            if (SetProperty(ref _fetchUrl, value))
                NotifyFetchStateChanged();
        }
    }

    public string FetchStatusText
    {
        get => _fetchStatusText;
        private set => SetProperty(ref _fetchStatusText, value);
    }

    public string RemoteHistoryStatusText
    {
        get => _remoteHistoryStatusText;
        private set => SetProperty(ref _remoteHistoryStatusText, value);
    }

    public string RemotePresetStatusText
    {
        get => _remotePresetStatusText;
        private set => SetProperty(ref _remotePresetStatusText, value);
    }

    public string CurrentIniHarvestStatusText
    {
        get => _currentIniHarvestStatusText;
        private set => SetProperty(ref _currentIniHarvestStatusText, value);
    }

    public ObservableCollection<FieldRegistryHarvestCandidateViewModel> Candidates { get; } = new();

    public ObservableCollection<FieldRegistryHarvestIssueViewModel> Issues { get; } = new();

    public ObservableCollection<FieldRegistryHarvestDefinitionPreviewViewModel> Definitions { get; } = new();

    public ObservableCollection<FieldRegistryHarvestWarningViewModel> RawWarnings { get; } = new();

    public ObservableCollection<FieldRegistryHarvestDiffRowViewModel> DiffRows { get; } = new();

    public ObservableCollection<FieldRegistryApplyPlanItemViewModel> ApplyPlanItems { get; } = new();

    public ObservableCollection<FieldRegistryGeneralizationMessageViewModel> GeneralizationMessages { get; } = new();

    public ObservableCollection<FieldRegistryRemoteSourceHistoryEntryViewModel> RemoteHistoryEntries { get; } = new();

    public ObservableCollection<FieldRegistryRemoteSourcePresetViewModel> RemotePresets { get; } = new();

    public ObservableCollection<FieldRegistryIniDraftRowViewModel> CurrentIniDraftRows { get; } = new();

    public FieldRegistryRemoteSourceHistoryEntryViewModel? SelectedRemoteHistoryEntry
    {
        get => _selectedRemoteHistoryEntry;
        set
        {
            if (SetProperty(ref _selectedRemoteHistoryEntry, value))
                NotifyRemoteHistoryStateChanged();
        }
    }

    public FieldRegistryRemoteSourcePresetViewModel? SelectedRemotePreset
    {
        get => _selectedRemotePreset;
        set
        {
            if (SetProperty(ref _selectedRemotePreset, value))
                NotifyRemotePresetStateChanged();
        }
    }

    public IReadOnlyList<FieldRegistryDisplayOption<FieldRegistryApplyTargetScope>> TargetScopeOptions { get; } =
    [
        new(FieldRegistryApplyTargetScope.Project, "项目 active 字段库"),
        new(FieldRegistryApplyTargetScope.Global, "全局 active 字段库")
    ];

    public IReadOnlyList<FieldRegistryDisplayOption<FieldRegistryApplyMode>> ApplyModeOptions { get; } =
    [
        new(FieldRegistryApplyMode.AppendOnly, "仅追加"),
        new(FieldRegistryApplyMode.AppendOrUpdate, "追加或更新"),
        new(FieldRegistryApplyMode.SkipExisting, "跳过已有字段")
    ];

    public int CandidateCount => Candidates.Count;

    public int DefinitionCount => Definitions.Count;

    public int IssueCount => Issues.Count;

    public int ErrorCount => Issues.Count(issue => issue.Severity == FieldRegistryHarvestValidationSeverity.Error.ToString());

    public int WarningCount => Issues.Count(issue => issue.Severity == FieldRegistryHarvestValidationSeverity.Warning.ToString());

    public int AddedCount => DiffRows.Count(row => row.Kind == FieldRegistryHarvestDiffKind.Added.ToString());

    public int SameCount => DiffRows.Count(row => row.Kind == FieldRegistryHarvestDiffKind.Same.ToString());

    public int ChangedCount => DiffRows.Count(row => row.Kind == FieldRegistryHarvestDiffKind.Changed.ToString());

    public int ConflictCount => DiffRows.Count(row => row.Kind == FieldRegistryHarvestDiffKind.Conflict.ToString());

    public int InvalidCount => DiffRows.Count(row => row.Kind == FieldRegistryHarvestDiffKind.Invalid.ToString());

    public int GeneralizationNoticeCount => GeneralizationMessages.Count(message => message.Severity == "提示");

    public int GeneralizationWarningCount => GeneralizationMessages.Count(message => message.Severity == "警告");

    public int GeneralizedFieldCount => GeneralizationNoticeCount;

    public int GeneralizedToTechnoCount => GeneralizationMessages.Count(message =>
        message.Severity == "提示" &&
        string.Equals(message.TargetKind, Ra2SectionKind.Techno.ToString(), StringComparison.Ordinal));

    public int GeneralizedToUnitCount => GeneralizationMessages.Count(message =>
        message.Severity == "提示" &&
        string.Equals(message.TargetKind, Ra2SectionKind.Unit.ToString(), StringComparison.Ordinal));

    public bool HasGeneralizationWarnings => GeneralizationWarningCount > 0;

    public bool HasCurrentIniDraftRows => CurrentIniDraftRows.Count > 0;

    public bool CanApplyInFuture
    {
        get => _canApplyInFuture;
        private set => SetProperty(ref _canApplyInFuture, value);
    }

    public FieldRegistryApplyTargetScope SelectedTargetScope
    {
        get => _selectedTargetScope;
        set
        {
            if (SetProperty(ref _selectedTargetScope, value))
            {
                ClearApplyPlan("目标范围已变更，应用计划已清空。");
                UpdateTargetFilePreviewText();
            }
        }
    }

    public FieldRegistryApplyMode SelectedApplyMode
    {
        get => _selectedApplyMode;
        set
        {
            if (SetProperty(ref _selectedApplyMode, value))
                ClearApplyPlan("应用模式已变更，应用计划已清空。");
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string ApplyStatusText
    {
        get => _applyStatusText;
        private set => SetProperty(ref _applyStatusText, value);
    }

    public string TargetFilePreviewText
    {
        get => _targetFilePreviewText;
        private set => SetProperty(ref _targetFilePreviewText, value);
    }

    public string LastApplyTargetFilePath
    {
        get => _lastApplyTargetFilePath;
        private set => SetProperty(ref _lastApplyTargetFilePath, value);
    }

    public string LastApplyBackupManifestPath
    {
        get => _lastApplyBackupManifestPath;
        private set => SetProperty(ref _lastApplyBackupManifestPath, value);
    }

    public string LastApplySummaryText
    {
        get => _lastApplySummaryText;
        private set => SetProperty(ref _lastApplySummaryText, value);
    }

    public bool CanBuildApplyPlan => _currentPreviewDraft is not null && _currentDiffResult is not null && DefinitionCount > 0;

    public bool CanApply => _currentApplyPlan is { CanApplyInFuture: true, ErrorCount: 0, RejectCount: 0 } &&
        (_currentApplyPlan.AddCount > 0 || _currentApplyPlan.UpdateCount > 0) &&
        (SelectedTargetScope != FieldRegistryApplyTargetScope.Project || !string.IsNullOrWhiteSpace(GetProjectRootPath()));

    public string ApplyDisabledReason => ResolveApplyAvailabilityMessage();

    public bool IsFetchingRawText
    {
        get => _isFetchingRawText;
        private set
        {
            if (SetProperty(ref _isFetchingRawText, value))
                NotifyFetchStateChanged();
        }
    }

    public bool CanFetchRawText => !IsFetchingRawText && !string.IsNullOrWhiteSpace(FetchUrl);

    public bool CanCancelFetch => IsFetchingRawText;

    public bool CanUseCachedText => SelectedRemoteHistoryEntry?.HasCachedText == true && !IsFetchingRawText;

    public bool CanRefetchSelected => SelectedRemoteHistoryEntry is not null && !IsFetchingRawText;

    public bool CanClearRemoteHistory => RemoteHistoryEntries.Count > 0 && !IsFetchingRawText;

    public bool CanUsePresetUrl => SelectedRemotePreset is not null && !IsFetchingRawText;

    public bool CanFetchSelectedPreset => SelectedRemotePreset?.IsEnabled == true && !IsFetchingRawText;

    public bool CanEditSelectedPreset => SelectedRemotePreset is not null && !IsFetchingRawText;

    public bool CanRemoveSelectedPreset => SelectedRemotePreset is not null && !IsFetchingRawText;

    public bool CanExportPresets => RemotePresets.Count > 0 && !IsFetchingRawText;

    public int PlanAddCount => _currentApplyPlan?.AddCount ?? 0;

    public int PlanUpdateCount => _currentApplyPlan?.UpdateCount ?? 0;

    public int PlanSkipCount => _currentApplyPlan?.SkipCount ?? 0;

    public int PlanRejectCount => _currentApplyPlan?.RejectCount ?? 0;

    public int PlanWarningCount => _currentApplyPlan?.WarningCount ?? 0;

    public int PlanErrorCount => _currentApplyPlan?.ErrorCount ?? 0;

    internal FieldRegistryApplyPlan? CurrentApplyPlan => _currentApplyPlan;

    public string SummaryText
        => $"字段：{CandidateCount}    草稿：{DefinitionCount}    问题：{IssueCount}    错误：{ErrorCount}    警告：{WarningCount}    新增：{AddedCount}    相同：{SameCount}    变更：{ChangedCount}    冲突：{ConflictCount}    无效：{InvalidCount}    未来可应用：{(CanApplyInFuture ? "是" : "否")}";

    public string ApplySummaryText
        => $"计划新增：{PlanAddCount}    更新：{PlanUpdateCount}    跳过：{PlanSkipCount}    拒绝：{PlanRejectCount}    计划警告：{PlanWarningCount}";

    public string GeneralizationSummaryText
        => GeneralizationMessages.Count == 0
            ? "草稿概括：未应用抽象 Section 合并。"
            : $"草稿概括：已应用 {GeneralizationNoticeCount} 项，{GeneralizationWarningCount} 条警告。";

    public string GeneralizationApplySummaryText
    {
        get
        {
            if (GeneralizedFieldCount == 0)
                return "字段归纳摘要：本次没有字段需要归纳。";

            return
                $"字段归纳摘要：本次将 {GeneralizedFieldCount} 个字段归纳为抽象字段。" +
                $"Techno：{GeneralizedToTechnoCount} 个；Unit：{GeneralizedToUnitCount} 个；警告：{GeneralizationWarningCount} 条。";
        }
    }

    public string GeneralizationWarningSummaryText
        => HasGeneralizationWarnings
            ? $"应用前请确认字段归纳警告：{GeneralizationWarningCount} 条。归纳后的字段将写入 Unit / Techno，而不是多个具体对象类型。"
            : "应用前请确认字段归纳结果。归纳后的字段将写入 Unit / Techno，而不是多个具体对象类型。";
    public void ParseAndPreview()
    {
        ClearResults();
        if (string.IsNullOrWhiteSpace(RawText))
        {
            StatusText = "没有可解析的原始文本。";
            return;
        }

        try
        {
            string sourceName = string.IsNullOrWhiteSpace(SourceName)
                ? "pasted-field-doc"
                : SourceName.Trim();
            FieldRegistryHarvestParseResult parseResult = _parser.Parse(new FieldRegistryHarvestDocument(sourceName, RawText));
            FieldRegistryHarvestNormalizeResult normalizeResult = _normalizer.Normalize(
                parseResult.Candidates,
                FieldRegistryHarvestNormalizeOptions.Default);
            FieldRegistryHarvestPreviewDraft previewDraft = ApplyGeneralization(_previewBuilder.BuildPreview(normalizeResult));
            FieldRegistryHarvestDiffResult diffResult = _diffService.Compare(previewDraft, _provenanceProviderAccessor());
            _currentPreviewDraft = previewDraft;
            _currentDiffResult = diffResult;

            foreach (FieldRegistryHarvestCandidate candidate in parseResult.Candidates)
                Candidates.Add(new FieldRegistryHarvestCandidateViewModel(candidate));

            foreach (FieldRegistryHarvestWarning warning in parseResult.Warnings)
                RawWarnings.Add(new FieldRegistryHarvestWarningViewModel(warning));

            foreach (FieldRegistryHarvestValidationIssue issue in previewDraft.Issues)
                Issues.Add(new FieldRegistryHarvestIssueViewModel(issue));

            foreach (Ra2FieldDefinition definition in previewDraft.Definitions)
                Definitions.Add(new FieldRegistryHarvestDefinitionPreviewViewModel(definition));

            foreach (FieldRegistryHarvestDiffRow row in diffResult.Rows)
                DiffRows.Add(new FieldRegistryHarvestDiffRowViewModel(row));

            CanApplyInFuture = previewDraft.CanApplyInFuture;
            StatusText = previewDraft.ErrorCount > 0
                ? "预览已生成，但存在验证错误；当前预览不能应用。"
                : $"预览已生成。字段：{CandidateCount}，草稿：{DefinitionCount}，问题：{IssueCount}，新增：{AddedCount}，变更：{ChangedCount}。";
            NotifyCountsChanged();
            NotifyApplyStateChanged();
        }
        catch (Exception ex)
        {
            Issues.Add(new FieldRegistryHarvestIssueViewModel(
                FieldRegistryHarvestValidationSeverity.Error.ToString(),
                string.Empty,
                string.IsNullOrWhiteSpace(SourceName) ? "pasted-field-doc" : SourceName.Trim(),
                0,
                $"预览失败：{ex.Message}"));
            CanApplyInFuture = false;
            StatusText = $"预览失败：{ex.Message}";
            NotifyCountsChanged();
            NotifyApplyStateChanged();
        }
    }

    public void LoadCurrentIniHarvestPreview(string sourceName, string text)
    {
        ClearResults();
        string normalizedSourceName = string.IsNullOrWhiteSpace(sourceName)
            ? "current.ini"
            : sourceName.Trim();
        SourceName = normalizedSourceName;

        if (string.IsNullOrWhiteSpace(text))
        {
            CurrentIniHarvestStatusText = "当前 INI 文本为空，未采集到字段候选。";
            StatusText = CurrentIniHarvestStatusText;
            return;
        }

        try
        {
            Ra2IniFieldHarvestResult harvestResult = _iniFieldHarvester.HarvestCurrentText(
                new Ra2IniFieldHarvestRequest(normalizedSourceName, text, []));
            IReadOnlyList<Ra2FieldImportDraftRow> draftRows = _iniDraftBuilder.BuildDraft(harvestResult);
            foreach (Ra2FieldImportDraftRow row in draftRows)
                CurrentIniDraftRows.Add(new FieldRegistryIniDraftRowViewModel(row));

            BuildPreviewFromCurrentIniDraftRows();
            foreach (FieldRegistryHarvestValidationIssue issue in harvestResult.Issues)
                Issues.Add(new FieldRegistryHarvestIssueViewModel(issue));

            string skippedSuffix = harvestResult.SkippedNumericKeyCount > 0
                ? $" 已跳过 {harvestResult.SkippedNumericKeyCount} 个数字或列表项。"
                : string.Empty;
            CurrentIniHarvestStatusText = CurrentIniDraftRows.Count == 0
                ? $"当前 INI 采集已完成：{normalizedSourceName}；未发现字段候选。{skippedSuffix}"
                : $"当前 INI 采集已完成：{normalizedSourceName}；发现 {CurrentIniDraftRows.Count} 条草稿。{skippedSuffix}";
            StatusText = CurrentIniDraftRows.Count == 0
                ? CurrentIniHarvestStatusText
                : $"当前 INI 预览已生成。草稿：{DefinitionCount}，问题：{IssueCount}，新增：{AddedCount}，变更：{ChangedCount}。";
            NotifyCurrentIniStateChanged();
            NotifyCountsChanged();
            NotifyApplyStateChanged();
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            Issues.Add(new FieldRegistryHarvestIssueViewModel(
                FieldRegistryHarvestValidationSeverity.Error.ToString(),
                string.Empty,
                normalizedSourceName,
                0,
                $"当前 INI 采集失败：{ex.Message}"));
            CanApplyInFuture = false;
            CurrentIniHarvestStatusText = $"当前 INI 采集失败：{ex.Message}";
            StatusText = CurrentIniHarvestStatusText;
            NotifyCurrentIniStateChanged();
            NotifyCountsChanged();
            NotifyApplyStateChanged();
        }
    }

    public async Task FetchRawTextAsync()
        => await FetchRawTextAsync(sourceNameOverride: null);

    private async Task FetchRawTextAsync(string? sourceNameOverride)
    {
        if (IsFetchingRawText)
            return;

        if (string.IsNullOrWhiteSpace(FetchUrl))
        {
            FetchStatusText = "获取失败：URL 不能为空。";
            StatusText = FetchStatusText;
            NotifyFetchStateChanged();
            return;
        }

        _fetchCancellationTokenSource?.Dispose();
        _fetchCancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = _fetchCancellationTokenSource.Token;
        IsFetchingRawText = true;
        FetchStatusText = "正在获取原始文本...";

        try
        {
            FieldRegistryRawFetchResult result = await _rawFetcher.FetchAsync(
                new FieldRegistryRawFetchRequest(FetchUrl),
                cancellationToken);
            if (cancellationToken.IsCancellationRequested)
                return;

            RawText = result.Text;
            SourceName = string.IsNullOrWhiteSpace(sourceNameOverride)
                ? result.SourceName
                : sourceNameOverride.Trim();
            ClearResults();
            string historySuffix = SaveRemoteHistory(result);
            FetchStatusText = $"已从 {result.SourceName} 获取 {result.ByteCount} 字节。{historySuffix}点击“解析并预览”进行分析。";
            StatusText = "原始文本已获取。点击“解析并预览”进行分析。";
        }
        catch (OperationCanceledException)
        {
            FetchStatusText = "获取已取消。";
            StatusText = FetchStatusText;
        }
        catch (Exception ex) when (ex is InvalidOperationException or HttpRequestException or TaskCanceledException or ArgumentException)
        {
            FetchStatusText = $"获取失败：{ex.Message}";
            StatusText = FetchStatusText;
        }
        finally
        {
            IsFetchingRawText = false;
            _fetchCancellationTokenSource?.Dispose();
            _fetchCancellationTokenSource = null;
        }
    }

    public void RefreshRemotePresets()
    {
        string? selectedId = SelectedRemotePreset?.Id;
        try
        {
            FieldRegistryRemoteSourcePresetCollection presets = _remoteSourcePresetStore.Load(GetGlobalFieldRegistryRootPath());
            RemotePresets.Clear();
            foreach (FieldRegistryRemoteSourcePreset preset in presets.Presets)
                RemotePresets.Add(new FieldRegistryRemoteSourcePresetViewModel(preset));
            SelectedRemotePreset = string.IsNullOrWhiteSpace(selectedId)
                ? null
                : RemotePresets.FirstOrDefault(preset => string.Equals(preset.Id, selectedId, StringComparison.OrdinalIgnoreCase));

            if (_remoteSourcePresetStore.LastWarning is { Length: > 0 } warning)
                RemotePresetStatusText = warning;
            else
                RemotePresetStatusText = RemotePresets.Count == 0
                    ? "没有远程来源预设。可以新增预设来保存本地 URL 书签。"
                    : $"已加载 {RemotePresets.Count} 个远程来源预设。“使用预设 URL”不会发起获取。";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            RemotePresets.Clear();
            SelectedRemotePreset = null;
            RemotePresetStatusText = $"加载远程来源预设失败：{ex.Message}";
        }
        finally
        {
            NotifyRemotePresetStateChanged();
        }
    }

    public void UsePresetUrl()
    {
        if (SelectedRemotePreset is null)
        {
            RemotePresetStatusText = "请先选择一个远程预设。";
            return;
        }

        FetchUrl = SelectedRemotePreset.Url;
        RemotePresetStatusText = $"已从 {SelectedRemotePreset.Name} 载入预设 URL。未发起网络请求。";
        StatusText = "预设 URL 已载入。点击“获取所选预设”或“获取原始文本”手动获取。";
    }

    public async Task FetchSelectedPresetAsync()
    {
        if (SelectedRemotePreset is null)
        {
            RemotePresetStatusText = "请先选择一个远程预设。";
            return;
        }

        if (!SelectedRemotePreset.IsEnabled)
        {
            RemotePresetStatusText = "所选预设已禁用。请先启用再获取。";
            NotifyRemotePresetStateChanged();
            return;
        }

        FetchUrl = SelectedRemotePreset.Url;
        RemotePresetStatusText = $"正在获取所选预设：{SelectedRemotePreset.Name}。";
        await FetchRawTextAsync(SelectedRemotePreset.Name);
    }

    public void AddPreset(FieldRegistryRemoteSourcePresetEditModel editModel)
    {
        ArgumentNullException.ThrowIfNull(editModel);
        try
        {
            FieldRegistryRemoteSourcePreset preset = CreatePresetFromEditModel(editModel, existing: null);
            _remoteSourcePresetStore.AddOrUpdate(GetGlobalFieldRegistryRootPath(), preset);
            RefreshRemotePresets();
            SelectPresetById(preset.Id);
            RemotePresetStatusText = $"已新增远程来源预设：{preset.Name}。";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        {
            RemotePresetStatusText = $"新增预设失败：{ex.Message}";
        }
    }

    public void EditSelectedPreset(FieldRegistryRemoteSourcePresetEditModel editModel)
    {
        ArgumentNullException.ThrowIfNull(editModel);
        if (SelectedRemotePreset is null)
        {
            RemotePresetStatusText = "请先选择远程预设再编辑。";
            return;
        }

        try
        {
            FieldRegistryRemoteSourcePreset preset = CreatePresetFromEditModel(editModel, SelectedRemotePreset.Preset);
            _remoteSourcePresetStore.AddOrUpdate(GetGlobalFieldRegistryRootPath(), preset);
            RefreshRemotePresets();
            SelectPresetById(preset.Id);
            RemotePresetStatusText = $"已更新远程来源预设：{preset.Name}。";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        {
            RemotePresetStatusText = $"更新预设失败：{ex.Message}";
        }
    }

    public void RemoveSelectedPreset(bool confirmed)
    {
        if (SelectedRemotePreset is null)
        {
            RemotePresetStatusText = "请先选择远程预设再移除。";
            return;
        }

        if (!confirmed)
        {
            RemotePresetStatusText = "已取消移除预设。";
            return;
        }

        string presetName = SelectedRemotePreset.Name;
        try
        {
            _remoteSourcePresetStore.Remove(GetGlobalFieldRegistryRootPath(), SelectedRemotePreset.Id);
            SelectedRemotePreset = null;
            RefreshRemotePresets();
            RemotePresetStatusText = $"已移除远程来源预设：{presetName}。";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            RemotePresetStatusText = $"移除预设失败：{ex.Message}";
        }
    }

    public void ImportPresets(string filePath, bool replaceExisting)
    {
        try
        {
            _remoteSourcePresetStore.ImportFromFile(GetGlobalFieldRegistryRootPath(), filePath, replaceExisting);
            string? importWarning = _remoteSourcePresetStore.LastWarning;
            RefreshRemotePresets();
            RemotePresetStatusText = importWarning is { Length: > 0 } warning
                ? $"已导入预设，但有警告：{warning}"
                : "已导入远程来源预设。";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        {
            RemotePresetStatusText = $"导入预设失败：{ex.Message}";
        }
    }

    public void ExportPresets(string filePath)
    {
        try
        {
            _remoteSourcePresetStore.ExportToFile(GetGlobalFieldRegistryRootPath(), filePath);
            RemotePresetStatusText = $"已将远程来源预设导出到 {filePath}。";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            RemotePresetStatusText = $"导出预设失败：{ex.Message}";
        }
    }

    public void CancelFetch()
    {
        if (!IsFetchingRawText)
            return;

        _fetchCancellationTokenSource?.Cancel();
        FetchStatusText = "正在取消获取...";
    }

    public void RefreshRemoteHistory()
    {
        try
        {
            FieldRegistryRemoteSourceHistory history = _remoteSourceHistoryStore.Load(GetGlobalFieldRegistryRootPath());
            RemoteHistoryEntries.Clear();
            foreach (FieldRegistryRemoteSourceHistoryEntry entry in history.Entries)
                RemoteHistoryEntries.Add(new FieldRegistryRemoteSourceHistoryEntryViewModel(entry));

            if (_remoteSourceHistoryStore.LastWarning is { Length: > 0 } warning)
                RemoteHistoryStatusText = warning;
            else
                RemoteHistoryStatusText = RemoteHistoryEntries.Count == 0
                    ? "没有远程来源历史。"
                    : $"已加载 {RemoteHistoryEntries.Count} 条远程来源历史。";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            RemoteHistoryEntries.Clear();
            RemoteHistoryStatusText = $"加载远程来源历史失败：{ex.Message}";
        }
        finally
        {
            NotifyRemoteHistoryStateChanged();
        }
    }

    public void UseCachedTextFromHistory()
    {
        if (SelectedRemoteHistoryEntry is null)
        {
            RemoteHistoryStatusText = "请先选择一条远程来源历史。";
            return;
        }

        if (!SelectedRemoteHistoryEntry.HasCachedText)
        {
            RemoteHistoryStatusText = "所选来源没有缓存文本。";
            return;
        }

        RawText = SelectedRemoteHistoryEntry.CachedText!;
        SourceName = SelectedRemoteHistoryEntry.SourceName;
        FetchUrl = SelectedRemoteHistoryEntry.Url;
        ClearResults();
        RemoteHistoryStatusText = $"已从 {SelectedRemoteHistoryEntry.SourceName} 载入缓存文本。点击“解析并预览”进行分析。";
        StatusText = "已载入缓存的远程来源文本。点击“解析并预览”进行分析。";
    }

    public async Task RefetchSelectedRemoteHistoryAsync()
    {
        if (SelectedRemoteHistoryEntry is null)
        {
            RemoteHistoryStatusText = "请先选择一条远程来源历史。";
            return;
        }

        FetchUrl = SelectedRemoteHistoryEntry.Url;
        RemoteHistoryStatusText = $"正在重新获取 {SelectedRemoteHistoryEntry.SourceName}。";
        await FetchRawTextAsync();
    }

    public void ClearRemoteHistory(bool confirmed)
    {
        if (!confirmed)
        {
            RemoteHistoryStatusText = "已取消清空历史。";
            return;
        }

        try
        {
            _remoteSourceHistoryStore.Clear(GetGlobalFieldRegistryRootPath());
            RemoteHistoryEntries.Clear();
            SelectedRemoteHistoryEntry = null;
            RemoteHistoryStatusText = "远程来源历史已清空。原始文本和 active 字段库未被修改。";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
        {
            RemoteHistoryStatusText = $"清空远程来源历史失败：{ex.Message}";
        }
        finally
        {
            NotifyRemoteHistoryStateChanged();
        }
    }

    public void BuildApplyPlan()
    {
        if (HasCurrentIniDraftRows)
            BuildPreviewFromCurrentIniDraftRows();

        if (!CanBuildApplyPlan || _currentPreviewDraft is null || _currentDiffResult is null)
        {
            ClearApplyPlan("请先解析并预览，再构建应用计划。");
            return;
        }

        if (SelectedTargetScope == FieldRegistryApplyTargetScope.Project &&
            string.IsNullOrWhiteSpace(GetProjectRootPath()))
        {
            const string message = "应用到 Project 范围前，请先打开项目目录。";
            ClearApplyPlan(message);
            StatusText = message;
            return;
        }

        FieldRegistryApplyPlan plan = _applyPlanBuilder.BuildPlan(new FieldRegistryApplyPlanRequest(
            _currentPreviewDraft,
            _currentDiffResult,
            SelectedTargetScope,
            SelectedApplyMode));

        _currentApplyPlan = plan;
        ApplyPlanItems.Clear();
        foreach (FieldRegistryApplyPlanItem item in plan.Items)
            ApplyPlanItems.Add(new FieldRegistryApplyPlanItemViewModel(item));

        ApplyStatusText = $"应用计划已构建。新增：{plan.AddCount}，更新：{plan.UpdateCount}，跳过：{plan.SkipCount}，拒绝：{plan.RejectCount}，警告：{plan.WarningCount}。";
        if (plan.AddCount + plan.UpdateCount == 0)
        {
            StatusText = "没有可应用的新增或更新操作。";
            ApplyStatusText += " 没有可应用的新增或更新操作。";
        }
        else if (plan.ErrorCount > 0 || plan.RejectCount > 0 || !plan.CanApplyInFuture)
        {
            StatusText = "应用已阻止：计划包含错误或被拒绝的条目。";
            ApplyStatusText += " 计划包含错误或被拒绝的条目，无法应用。";
        }

        NotifyApplyStateChanged();
    }

    public FieldRegistryApplyConfirmationViewModel? CreateApplyConfirmation()
    {
        if (_currentApplyPlan is null)
        {
            ApplyStatusText = "应用前请先构建应用计划。";
            return null;
        }

        string targetFilePath = GetTargetFilePath();
        bool hasBuiltInOverride = _currentApplyPlan.Items.Any(item =>
            item.OperationKind is FieldRegistryApplyOperationKind.Add or FieldRegistryApplyOperationKind.Update &&
            item.ExistingScope == FieldRegistryProvenanceScope.BuiltIn);
        bool hasProjectOverGlobal = _currentApplyPlan.Items.Any(item =>
            item.OperationKind is FieldRegistryApplyOperationKind.Add or FieldRegistryApplyOperationKind.Update &&
            item.TargetScope == FieldRegistryApplyTargetScope.Project &&
            item.ExistingScope == FieldRegistryProvenanceScope.Global);

        return new FieldRegistryApplyConfirmationViewModel(
            _currentApplyPlan.TargetScope.ToString(),
            _currentApplyPlan.Mode.ToString(),
            targetFilePath,
            _currentApplyPlan.AddCount,
            _currentApplyPlan.UpdateCount,
            _currentApplyPlan.SkipCount,
            _currentApplyPlan.RejectCount,
            _currentApplyPlan.WarningCount,
            hasBuiltInOverride,
            hasProjectOverGlobal,
            GeneralizationApplySummaryText,
            HasGeneralizationWarnings ? GeneralizationWarningSummaryText : null);
    }

    public FieldRegistryApplyWriteResult? ApplyConfirmed()
    {
        if (!CanApply || _currentApplyPlan is null)
        {
            ApplyStatusText = ResolveApplyBlockedMessage();
            StatusText = ApplyStatusText;
            NotifyApplyStateChanged();
            return null;
        }

        try
        {
            FieldRegistryApplyWriteResult result = _applyWriter.Write(new FieldRegistryApplyWriteRequest(
                _currentApplyPlan,
                GetProjectRootPath(),
                GetGlobalFieldRegistryRootPath()));
            _reloadAfterApply?.Invoke();
            LastApplyTargetFilePath = result.TargetFilePath;
            LastApplyBackupManifestPath = result.ManifestFilePath ?? string.Empty;
            LastApplySummaryText = $"应用已完成。目标：{result.TargetFilePath} | 备份清单：{FormatOptionalPath(result.ManifestFilePath)} | 新增：{result.AddedCount} | 更新：{result.UpdatedCount} | 跳过：{result.SkippedCount} | {GeneralizationApplySummaryText}";
            ApplyStatusText =
                "应用已完成。\n" +
                $"目标：{result.TargetFilePath}\n" +
                $"清单：{FormatOptionalPath(result.ManifestFilePath)}\n" +
                $"新增：{result.AddedCount} 更新：{result.UpdatedCount} 跳过：{result.SkippedCount}\n" +
                GeneralizationApplySummaryText;
            StatusText = $"应用已完成。本地字段库已重新加载，用于只读高亮。{GeneralizationApplySummaryText}";
            NotifyApplyStateChanged();
            return result;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException or ArgumentException)
        {
            ApplyStatusText = $"应用失败：{ex.Message}";
            StatusText = ApplyStatusText;
            NotifyApplyStateChanged();
            return null;
        }
    }

    public void Clear()
    {
        RawText = string.Empty;
        ClearResults();
        StatusText = "预览已清空。";
    }

    public void InsertSample()
    {
        RawText = SampleText;
        ClearResults();
        StatusText = "示例已插入。点击“解析并预览”进行分析。";
    }

    private void ClearResults()
    {
        Candidates.Clear();
        Issues.Clear();
        Definitions.Clear();
        RawWarnings.Clear();
        DiffRows.Clear();
        ApplyPlanItems.Clear();
        GeneralizationMessages.Clear();
        CurrentIniDraftRows.Clear();
        CanApplyInFuture = false;
        _currentPreviewDraft = null;
        _currentDiffResult = null;
        _currentApplyPlan = null;
        CurrentIniHarvestStatusText = "尚未加载当前 INI 的字段采集结果。";
        ApplyStatusText = "请先解析并预览，再构建应用计划。";
        LastApplyTargetFilePath = string.Empty;
        LastApplyBackupManifestPath = string.Empty;
        LastApplySummaryText = "本次预览会话尚未完成应用。";
        NotifyCurrentIniStateChanged();
        NotifyCountsChanged();
        NotifyApplyStateChanged();
    }

    private void BuildPreviewFromCurrentIniDraftRows()
    {
        FieldRegistryHarvestPreviewDraft previewDraft = ApplyGeneralization(_iniDraftBuilder.BuildPreviewFromDraft(
            CurrentIniDraftRows.Select(row => row.ToDraftRow()).ToArray()));
        FieldRegistryHarvestDiffResult diffResult = _diffService.Compare(previewDraft, _provenanceProviderAccessor());
        _currentPreviewDraft = previewDraft;
        _currentDiffResult = diffResult;

        Issues.Clear();
        Definitions.Clear();
        DiffRows.Clear();
        ApplyPlanItems.Clear();
        _currentApplyPlan = null;

        foreach (FieldRegistryHarvestValidationIssue issue in previewDraft.Issues)
            Issues.Add(new FieldRegistryHarvestIssueViewModel(issue));

        foreach (Ra2FieldDefinition definition in previewDraft.Definitions)
            Definitions.Add(new FieldRegistryHarvestDefinitionPreviewViewModel(definition));

        foreach (FieldRegistryHarvestDiffRow row in diffResult.Rows)
            DiffRows.Add(new FieldRegistryHarvestDiffRowViewModel(row));

        CanApplyInFuture = previewDraft.CanApplyInFuture;
        NotifyCountsChanged();
        NotifyApplyStateChanged();
    }

    private FieldRegistryHarvestPreviewDraft ApplyGeneralization(FieldRegistryHarvestPreviewDraft previewDraft)
    {
        Ra2FieldDraftGeneralizationResult result = _generalizationPipeline.Generalize(previewDraft);
        GeneralizationMessages.Clear();

        foreach (Ra2FieldDraftGeneralizationNotice notice in result.Notices)
            GeneralizationMessages.Add(FieldRegistryGeneralizationMessageViewModel.FromNotice(notice));

        foreach (Ra2FieldDraftGeneralizationWarning warning in result.Warnings)
            GeneralizationMessages.Add(FieldRegistryGeneralizationMessageViewModel.FromWarning(warning));

        OnPropertyChanged(nameof(GeneralizationNoticeCount));
        OnPropertyChanged(nameof(GeneralizationWarningCount));
        OnPropertyChanged(nameof(GeneralizedFieldCount));
        OnPropertyChanged(nameof(GeneralizedToTechnoCount));
        OnPropertyChanged(nameof(GeneralizedToUnitCount));
        OnPropertyChanged(nameof(HasGeneralizationWarnings));
        OnPropertyChanged(nameof(GeneralizationSummaryText));
        OnPropertyChanged(nameof(GeneralizationApplySummaryText));
        OnPropertyChanged(nameof(GeneralizationWarningSummaryText));
        return result.PreviewDraft;
    }

    private void ClearApplyPlan(string statusText)
    {
        _currentApplyPlan = null;
        ApplyPlanItems.Clear();
        ApplyStatusText = statusText;
        NotifyApplyStateChanged();
    }

    private void NotifyCountsChanged()
    {
        OnPropertyChanged(nameof(CandidateCount));
        OnPropertyChanged(nameof(DefinitionCount));
        OnPropertyChanged(nameof(IssueCount));
        OnPropertyChanged(nameof(ErrorCount));
        OnPropertyChanged(nameof(WarningCount));
        OnPropertyChanged(nameof(AddedCount));
        OnPropertyChanged(nameof(SameCount));
        OnPropertyChanged(nameof(ChangedCount));
        OnPropertyChanged(nameof(ConflictCount));
        OnPropertyChanged(nameof(InvalidCount));
        OnPropertyChanged(nameof(SummaryText));
        OnPropertyChanged(nameof(GeneralizationNoticeCount));
        OnPropertyChanged(nameof(GeneralizationWarningCount));
        OnPropertyChanged(nameof(GeneralizedFieldCount));
        OnPropertyChanged(nameof(GeneralizedToTechnoCount));
        OnPropertyChanged(nameof(GeneralizedToUnitCount));
        OnPropertyChanged(nameof(HasGeneralizationWarnings));
        OnPropertyChanged(nameof(GeneralizationSummaryText));
        OnPropertyChanged(nameof(GeneralizationApplySummaryText));
        OnPropertyChanged(nameof(GeneralizationWarningSummaryText));
    }

    private void NotifyCurrentIniStateChanged()
    {
        OnPropertyChanged(nameof(HasCurrentIniDraftRows));
    }

    private void NotifyApplyStateChanged()
    {
        OnPropertyChanged(nameof(CanBuildApplyPlan));
        OnPropertyChanged(nameof(CanApply));
        OnPropertyChanged(nameof(PlanAddCount));
        OnPropertyChanged(nameof(PlanUpdateCount));
        OnPropertyChanged(nameof(PlanSkipCount));
        OnPropertyChanged(nameof(PlanRejectCount));
        OnPropertyChanged(nameof(PlanWarningCount));
        OnPropertyChanged(nameof(PlanErrorCount));
        OnPropertyChanged(nameof(ApplySummaryText));
        OnPropertyChanged(nameof(ApplyDisabledReason));
    }

    private void NotifyFetchStateChanged()
    {
        OnPropertyChanged(nameof(CanFetchRawText));
        OnPropertyChanged(nameof(CanCancelFetch));
        NotifyRemoteHistoryStateChanged();
        NotifyRemotePresetStateChanged();
    }

    private void NotifyRemoteHistoryStateChanged()
    {
        OnPropertyChanged(nameof(CanUseCachedText));
        OnPropertyChanged(nameof(CanRefetchSelected));
        OnPropertyChanged(nameof(CanClearRemoteHistory));
    }

    private void NotifyRemotePresetStateChanged()
    {
        OnPropertyChanged(nameof(CanUsePresetUrl));
        OnPropertyChanged(nameof(CanFetchSelectedPreset));
        OnPropertyChanged(nameof(CanEditSelectedPreset));
        OnPropertyChanged(nameof(CanRemoveSelectedPreset));
        OnPropertyChanged(nameof(CanExportPresets));
    }

    private static FieldRegistryRemoteSourcePreset CreatePresetFromEditModel(
        FieldRegistryRemoteSourcePresetEditModel editModel,
        FieldRegistryRemoteSourcePreset? existing)
    {
        string now = DateTimeOffset.UtcNow.ToString("O");
        return new FieldRegistryRemoteSourcePreset(
            string.IsNullOrWhiteSpace(editModel.Id)
                ? existing?.Id ?? Guid.NewGuid().ToString("N")
                : editModel.Id,
            editModel.Name,
            editModel.Url,
            editModel.Description,
            FieldRegistryRemoteSourcePresetEditModel.ParseTags(editModel.TagsText),
            editModel.IsEnabled,
            existing?.CreatedAtUtc ?? now,
            now);
    }

    private void SelectPresetById(string presetId)
    {
        SelectedRemotePreset = RemotePresets.FirstOrDefault(preset =>
            string.Equals(preset.Id, presetId, StringComparison.OrdinalIgnoreCase));
    }

    private string SaveRemoteHistory(FieldRegistryRawFetchResult result)
    {
        try
        {
            _remoteSourceHistoryStore.AddOrUpdate(
                GetGlobalFieldRegistryRootPath(),
                new FieldRegistryRemoteSourceHistoryEntry(
                    result.Url,
                    result.ResolvedUrl,
                    result.SourceName,
                    DateTimeOffset.UtcNow,
                    result.ByteCount,
                    result.Text));
            RefreshRemoteHistory();
            return "已保存到远程来源历史。";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or InvalidOperationException)
        {
            RemoteHistoryStatusText = $"已获取文本，但保存远程来源历史失败：{ex.Message}";
            return "已获取文本，但保存远程来源历史失败。";
        }
    }

    private void UpdateTargetFilePreviewText()
    {
        try
        {
            TargetFilePreviewText = $"目标文件：{GetTargetFilePath()}";
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            TargetFilePreviewText = $"目标文件不可用：{ex.Message}";
        }
    }

    private string GetTargetFilePath()
    {
        FieldRegistryApplyPathResolver resolver = new();
        return resolver.ResolveTargetPackPath(
            SelectedTargetScope,
            GetProjectRootPath(),
            GetGlobalFieldRegistryRootPath(),
            FieldRegistryApplyWriteRequest.DefaultTargetPackFileName);
    }

    private string? GetProjectRootPath()
        => _projectRootPathAccessor();

    private string GetGlobalFieldRegistryRootPath()
        => _globalFieldRegistryRootPathAccessor();

    private string ResolveApplyBlockedMessage()
    {
        string message = ResolveApplyAvailabilityMessage();
        return message == "已准备好应用。"
            ? "已准备好应用。"
            : message;
    }

    private string ResolveApplyAvailabilityMessage()
    {
        if (_currentPreviewDraft is null || _currentDiffResult is null)
            return "尚未生成预览。请先点击“解析并预览”。";

        if (SelectedTargetScope == FieldRegistryApplyTargetScope.Project &&
            string.IsNullOrWhiteSpace(GetProjectRootPath()))
        {
            return "应用到 Project 范围前，请先打开项目目录。";
        }

        if (_currentApplyPlan is null)
            return "尚未构建应用计划。请检查预览后点击“构建应用计划”。";

        if (_currentApplyPlan.AddCount + _currentApplyPlan.UpdateCount == 0)
            return "没有可应用的新增或更新操作。";

        if (_currentApplyPlan.ErrorCount > 0)
            return "计划包含错误。";

        if (_currentApplyPlan.RejectCount > 0 || !_currentApplyPlan.CanApplyInFuture)
            return "计划包含被拒绝的条目。";

        return "已准备好应用。";
    }

    private static string FormatOptionalPath(string? path)
        => string.IsNullOrWhiteSpace(path) ? "无" : path;

    private static string CreateDefaultGlobalFieldRegistryRootPath()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "RA2IniEditor", "FieldRegistry");
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

    private static string NormalizeSourceNameForDisplay(string? sourceName)
        => string.IsNullOrWhiteSpace(sourceName) ? string.Empty : sourceName.Trim();
}

internal sealed class FieldRegistryDisplayOption<T>
{
    public FieldRegistryDisplayOption(T value, string displayName)
    {
        Value = value;
        DisplayName = displayName;
    }

    public T Value { get; }

    public string DisplayName { get; }

    public override string ToString() => DisplayName;
}

internal sealed class FieldRegistryApplyPlanItemViewModel
{
    public FieldRegistryApplyPlanItemViewModel(FieldRegistryApplyPlanItem item)
    {
        Operation = item.OperationKind.ToString();
        Key = item.Key;
        AppliesTo = item.AppliesTo.ToString();
        TargetScope = item.TargetScope.ToString();
        ExistingScope = item.ExistingScope.ToString();
        ExistingSource = string.IsNullOrWhiteSpace(item.ExistingSourceName)
            ? "无"
            : item.ExistingSourceName;
        Message = item.Message;
    }

    public string Operation { get; }

    public string Key { get; }

    public string AppliesTo { get; }

    public string TargetScope { get; }

    public string ExistingScope { get; }

    public string ExistingSource { get; }

    public string Message { get; }
}

internal sealed class FieldRegistryGeneralizationMessageViewModel
{
    private FieldRegistryGeneralizationMessageViewModel(
        string severity,
        string key,
        string targetKind,
        string sourceKinds,
        string message)
    {
        Severity = severity;
        Key = key;
        TargetKind = targetKind;
        SourceKinds = sourceKinds;
        Message = message;
    }

    public string Severity { get; }

    public string Key { get; }

    public string TargetKind { get; }

    public string SourceKinds { get; }

    public string Message { get; }

    public static FieldRegistryGeneralizationMessageViewModel FromNotice(Ra2FieldDraftGeneralizationNotice notice)
    {
        return new FieldRegistryGeneralizationMessageViewModel(
            "提示",
            notice.Key,
            notice.TargetKind.ToString(),
            string.Join(", ", notice.SourceKinds),
            notice.Message);
    }

    public static FieldRegistryGeneralizationMessageViewModel FromWarning(Ra2FieldDraftGeneralizationWarning warning)
    {
        return new FieldRegistryGeneralizationMessageViewModel(
            "警告",
            warning.Key,
            warning.TargetKind.ToString(),
            string.Join(", ", warning.SourceKinds),
            warning.Message);
    }
}

internal sealed class FieldRegistryApplyConfirmationViewModel
{
    public FieldRegistryApplyConfirmationViewModel(
        string targetScope,
        string applyMode,
        string targetFilePath,
        int addCount,
        int updateCount,
        int skipCount,
        int rejectCount,
        int warningCount,
        bool hasBuiltInOverride,
        bool hasProjectOverGlobal,
        string generalizationSummaryText,
        string? generalizationWarningSummaryText)
    {
        Title = "应用字段库导入";
        Message =
            $"是否应用字段库导入？\n\n" +
            $"目标范围：{targetScope}\n" +
            $"目标文件：{targetFilePath}\n" +
            $"应用模式：{applyMode}\n" +
            $"新增：{addCount}，更新：{updateCount}，跳过：{skipCount}，拒绝：{rejectCount}，警告：{warningCount}\n\n" +
            $"{generalizationSummaryText}\n" +
            (string.IsNullOrWhiteSpace(generalizationWarningSummaryText) ? string.Empty : $"{generalizationWarningSummaryText}\n") +
            "\n" +
            "如果目标字段包已存在，写入前会创建备份清单。" +
            (hasBuiltInOverride ? "\n\n警告：BuiltIn 字段不会被直接修改；本次会创建本地覆盖。" : string.Empty) +
            (hasProjectOverGlobal ? "\n\n警告：Project 目标会以更高优先级覆盖已有 Global 定义。" : string.Empty);
    }

    public string Title { get; }

    public string Message { get; }
}

internal sealed class FieldRegistryRemoteSourceHistoryEntryViewModel
{
    public FieldRegistryRemoteSourceHistoryEntryViewModel(FieldRegistryRemoteSourceHistoryEntry entry)
    {
        Entry = entry;
        Url = entry.Url;
        ResolvedUrl = entry.ResolvedUrl;
        SourceName = entry.SourceName;
        FetchedAtUtc = entry.FetchedAtUtc.ToString("u");
        ByteCount = entry.ByteCount;
        CachedText = entry.CachedText;
    }

    internal FieldRegistryRemoteSourceHistoryEntry Entry { get; }

    public string Url { get; }

    public string ResolvedUrl { get; }

    public string SourceName { get; }

    public string FetchedAtUtc { get; }

    public int ByteCount { get; }

    public string? CachedText { get; }

    public bool HasCachedText => !string.IsNullOrEmpty(CachedText);
}

internal sealed class FieldRegistryRemoteSourcePresetEditModel
{
    public FieldRegistryRemoteSourcePresetEditModel(
        string? id,
        string name,
        string url,
        string? description,
        string? tagsText,
        bool isEnabled)
    {
        Id = string.IsNullOrWhiteSpace(id) ? null : id.Trim();
        Name = name?.Trim() ?? string.Empty;
        Url = url?.Trim() ?? string.Empty;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        TagsText = tagsText?.Trim() ?? string.Empty;
        IsEnabled = isEnabled;
    }

    public string? Id { get; }

    public string Name { get; }

    public string Url { get; }

    public string? Description { get; }

    public string TagsText { get; }

    public bool IsEnabled { get; }

    public static IReadOnlyList<string> ParseTags(string? tagsText)
    {
        if (string.IsNullOrWhiteSpace(tagsText))
            return [];

        return tagsText
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .ToArray();
    }
}

internal sealed class FieldRegistryRemoteSourcePresetViewModel
{
    public FieldRegistryRemoteSourcePresetViewModel(FieldRegistryRemoteSourcePreset preset)
    {
        Preset = preset;
        Id = preset.Id;
        Name = preset.Name;
        Url = preset.Url;
        Description = preset.Description ?? string.Empty;
        TagsText = string.Join(", ", preset.Tags);
        IsEnabled = preset.IsEnabled;
        CreatedAtUtc = preset.CreatedAtUtc;
        UpdatedAtUtc = preset.UpdatedAtUtc;
    }

    public FieldRegistryRemoteSourcePreset Preset { get; }

    public string Id { get; }

    public string Name { get; }

    public string Url { get; }

    public string Description { get; }

    public string TagsText { get; }

    public bool IsEnabled { get; }

    public string CreatedAtUtc { get; }

    public string UpdatedAtUtc { get; }
}

internal sealed class FieldRegistryHarvestCandidateViewModel
{
    public FieldRegistryHarvestCandidateViewModel(FieldRegistryHarvestCandidate candidate)
    {
        Key = candidate.Key;
        AppliesToRaw = candidate.AppliesToRaw ?? string.Empty;
        EditorKindRaw = candidate.EditorKindRaw ?? string.Empty;
        Confidence = candidate.Confidence.ToString();
        SourceName = candidate.SourceName;
        LineNumber = candidate.LineNumber;
    }

    public string Key { get; }

    public string AppliesToRaw { get; }

    public string EditorKindRaw { get; }

    public string Confidence { get; }

    public string SourceName { get; }

    public int LineNumber { get; }
}

internal sealed class FieldRegistryIniDraftRowViewModel : INotifyPropertyChanged
{
    private string _allowedValuesText;
    private string? _displayName;
    private string? _description;

    public FieldRegistryIniDraftRowViewModel(Ra2FieldImportDraftRow row)
    {
        ArgumentNullException.ThrowIfNull(row);
        IsEnabled = row.IsEnabled;
        Key = row.Key;
        SectionKindValue = row.SectionKind;
        SectionKind = row.SectionKind.ToString();
        OccurrenceCount = row.OccurrenceCount;
        SampleValueSummary = row.SampleValueSummary;
        EditorKindValue = row.EditorKind;
        EditorKind = row.EditorKind.ToString();
        ValueKindValue = row.ValueKind;
        ValueKind = row.ValueKind.ToString();
        BooleanStyleValue = row.BooleanStyle;
        BooleanStyle = row.BooleanStyle.ToString();
        _allowedValuesText = row.AllowedValuesText;
        ScannedAllowedValuesText = row.AllowedValuesText;
        _displayName = row.DisplayName;
        _description = row.Description;
        SourceNote = row.SourceNote ?? string.Empty;
        IssueSummary = row.IssueSummary;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsEnabled { get; set; }

    public string Key { get; }

    public Ra2SectionKind SectionKindValue { get; }

    public string SectionKind { get; }

    public int OccurrenceCount { get; }

    public string SampleValueSummary { get; }

    public FieldEditorKind EditorKindValue { get; }

    public string EditorKind { get; }

    public Ra2FieldValueKind ValueKindValue { get; }

    public string ValueKind { get; }

    public Ra2FieldBooleanValueStyle BooleanStyleValue { get; }

    public string BooleanStyle { get; }

    public string AllowedValuesText
    {
        get => _allowedValuesText;
        set => SetProperty(ref _allowedValuesText, value ?? string.Empty);
    }

    public string ScannedAllowedValuesText { get; }

    public string? DisplayName
    {
        get => _displayName;
        set => SetProperty(ref _displayName, value);
    }

    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string SourceNote { get; }

    public string IssueSummary { get; }

    internal Ra2FieldImportDraftRow ToDraftRow()
    {
        return new Ra2FieldImportDraftRow(
            IsEnabled,
            Key,
            SectionKindValue,
            OccurrenceCount,
            SampleValueSummary,
            EditorKindValue,
            ValueKindValue,
            BooleanStyleValue,
            AllowedValuesText,
            DisplayName,
            Description,
            SourceNote,
            IssueSummary);
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

internal sealed class FieldRegistryHarvestIssueViewModel
{
    public FieldRegistryHarvestIssueViewModel(FieldRegistryHarvestValidationIssue issue)
        : this(issue.Severity.ToString(), issue.Key ?? string.Empty, issue.SourceName, issue.LineNumber, issue.Message)
    {
    }

    public FieldRegistryHarvestIssueViewModel(
        string severity,
        string key,
        string sourceName,
        int lineNumber,
        string message)
    {
        Severity = severity;
        Key = key;
        SourceName = sourceName;
        LineNumber = lineNumber;
        Message = message;
    }

    public string Severity { get; }

    public string Key { get; }

    public string SourceName { get; }

    public int LineNumber { get; }

    public string Message { get; }
}

internal sealed class FieldRegistryHarvestDefinitionPreviewViewModel
{
    public FieldRegistryHarvestDefinitionPreviewViewModel(Ra2FieldDefinition definition)
    {
        Key = definition.Key;
        AppliesTo = string.Join(", ", definition.AppliesTo);
        EditorKind = definition.EditorKind.ToString();
        SourceKind = definition.SourceKind.ToString();
        Description = definition.Description ?? string.Empty;
    }

    public string Key { get; }

    public string AppliesTo { get; }

    public string EditorKind { get; }

    public string SourceKind { get; }

    public string Description { get; }
}

internal sealed class FieldRegistryHarvestWarningViewModel
{
    public FieldRegistryHarvestWarningViewModel(FieldRegistryHarvestWarning warning)
    {
        SourceName = warning.SourceName;
        LineNumber = warning.LineNumber;
        Message = warning.Message;
    }

    public string SourceName { get; }

    public int LineNumber { get; }

    public string Message { get; }
}

internal sealed class FieldRegistryHarvestDiffRowViewModel
{
    public FieldRegistryHarvestDiffRowViewModel(FieldRegistryHarvestDiffRow row)
    {
        Kind = row.Kind.ToString();
        Key = row.Key;
        AppliesTo = row.AppliesTo.ToString();
        PreviewEditorKind = row.PreviewEditorKind?.ToString() ?? string.Empty;
        ExistingEditorKind = row.ExistingEditorKind?.ToString() ?? string.Empty;
        PreviewSourceKind = row.PreviewSourceKind?.ToString() ?? string.Empty;
        ExistingSourceKind = row.ExistingSourceKind?.ToString() ?? string.Empty;
        ExistingScope = row.ExistingScope.ToString();
        ExistingSourceName = row.ExistingSourceName;
        ExistingSourcePath = row.ExistingSourcePath ?? string.Empty;
        PreviewDescription = row.PreviewDescription ?? string.Empty;
        ExistingDescription = row.ExistingDescription ?? string.Empty;
        Message = row.Message;
    }

    public string Kind { get; }

    public string Key { get; }

    public string AppliesTo { get; }

    public string PreviewEditorKind { get; }

    public string ExistingEditorKind { get; }

    public string PreviewSourceKind { get; }

    public string ExistingSourceKind { get; }

    public string ExistingScope { get; }

    public string ExistingSourceName { get; }

    public string ExistingSourcePath { get; }

    public string PreviewDescription { get; }

    public string ExistingDescription { get; }

    public string Message { get; }
}



