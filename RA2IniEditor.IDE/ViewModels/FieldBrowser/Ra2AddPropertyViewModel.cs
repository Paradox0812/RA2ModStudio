using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RA2IniEditor.Core.Schema;
using RA2IniEditor.IDE.Editing;
using RA2IniEditor.IDE.FieldAnnotations;
using RA2IniEditor.IDE.TextModel;
using RA2IniEditor.IDE.ViewModels.FieldAnnotations;

namespace RA2IniEditor.IDE.ViewModels.FieldBrowser;

internal sealed class Ra2AddPropertyViewModel : INotifyPropertyChanged
{
    private IRa2FieldDisplayResolver _displayResolver;
    private IRa2EffectiveFieldCatalog _effectiveFieldCatalog;
    private readonly Ra2FieldDiscoveryMatcher _matcher;
    private readonly Ra2EditorDocumentState _editorState;
    private Ra2FieldAnnotationStatusViewModel _annotationStatus;
    private readonly Ra2RecentFieldUsageTracker? _recentFieldUsageTracker;
    private readonly Ra2AddPropertyValueHintProvider _valueHintProvider;
    private readonly Ra2DuplicateKeyDetector _duplicateKeyDetector;
    private readonly Ra2IniTextDocument? _document;
    private readonly int _caretOffset;
    private readonly Ra2SectionKindDisplayNameProvider _sectionDisplayNameProvider = new();
    private Ra2AddPropertyDuplicateActionViewModel _duplicateAction;
    private string _searchText = string.Empty;
    private Ra2SectionKind? _selectedSectionKind;
    private Ra2SectionKindOptionViewModel? _selectedSectionKindOption;
    private Ra2FieldBrowserSearchMode _selectedSearchMode = Ra2FieldBrowserSearchMode.Applicable;
    private Ra2FieldBrowserSearchModeOptionViewModel? _selectedSearchModeOption;
    private Ra2AddPropertyItemViewModel? _selectedItem;
    private string _optionText = string.Empty;
    private string _valueText = string.Empty;
    private string _statusText = string.Empty;
    private string _valueHintText = "请手动输入字段值。";
    private string _insertPreviewText = "预览：";
    private string _duplicateWarningText = string.Empty;

    public Ra2AddPropertyViewModel(
        IRa2FieldDisplayResolver displayResolver,
        Ra2SectionKind? initialSectionKind,
        Ra2EditorDocumentState editorState,
        Ra2FieldDiscoveryMatcher? matcher = null,
        Ra2FieldAnnotationStatusViewModel? annotationStatus = null,
        Ra2RecentFieldUsageTracker? recentFieldUsageTracker = null,
        Ra2IniTextDocument? document = null,
        int caretOffset = 0,
        Ra2AddPropertyValueHintProvider? valueHintProvider = null,
        Ra2DuplicateKeyDetector? duplicateKeyDetector = null)
    {
        _displayResolver = displayResolver ?? throw new ArgumentNullException(nameof(displayResolver));
        _effectiveFieldCatalog = new Ra2EffectiveFieldCatalog(_displayResolver);
        _matcher = matcher ?? new Ra2FieldDiscoveryMatcher();
        _editorState = editorState;
        _annotationStatus = annotationStatus ?? new Ra2FieldAnnotationStatusViewModel(
            "字段注释：未找到项目注释库，已回退到字段库。",
            isLoaded: false,
            hasWarnings: false);
        _recentFieldUsageTracker = recentFieldUsageTracker;
        _document = document;
        _caretOffset = caretOffset;
        _valueHintProvider = valueHintProvider ?? new Ra2AddPropertyValueHintProvider();
        _duplicateKeyDetector = duplicateKeyDetector ?? new Ra2DuplicateKeyDetector();
        _duplicateAction = CreateDuplicateAction(null);
        SectionKinds = [null, .. Enum.GetValues<Ra2SectionKind>().Where(kind => kind != Ra2SectionKind.Unknown)];
        SectionKindOptions = SectionKinds
            .Select(kind => new Ra2SectionKindOptionViewModel(kind, _sectionDisplayNameProvider))
            .ToArray();
        SearchModeOptions =
        [
            new(Ra2FieldBrowserSearchMode.Applicable, "当前可用字段"),
            new(Ra2FieldBrowserSearchMode.Common, "通用字段"),
            new(Ra2FieldBrowserSearchMode.Specific, "当前类型独有"),
            new(Ra2FieldBrowserSearchMode.Recent, "最近使用"),
            new(Ra2FieldBrowserSearchMode.All, "全部字段")
        ];
        _selectedSectionKind = initialSectionKind;
        _selectedSectionKindOption = SectionKindOptions.FirstOrDefault(option => option.Value == initialSectionKind);
        _selectedSearchModeOption = SearchModeOptions.First(option => option.Value == _selectedSearchMode);
        RefreshItems();
        RefreshFilteredItems();
        RefreshPreviewAndWarnings();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public IReadOnlyList<Ra2SectionKind?> SectionKinds { get; }

    public IReadOnlyList<Ra2SectionKindOptionViewModel> SectionKindOptions { get; }

    public IReadOnlyList<Ra2FieldBrowserSearchModeOptionViewModel> SearchModeOptions { get; }

    public ObservableCollection<Ra2AddPropertyItemViewModel> Items { get; } = new();

    public ObservableCollection<Ra2AddPropertyItemViewModel> FilteredItems { get; } = new();

    public string AnnotationStatusText => _annotationStatus.StatusText;

    public bool HasAnnotationWarnings => _annotationStatus.HasWarnings;

    public string AnnotationWarningsText => string.Join(Environment.NewLine, _annotationStatus.Warnings);

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value ?? string.Empty))
                RefreshFilteredItems();
        }
    }

    public Ra2SectionKind? SelectedSectionKind
    {
        get => _selectedSectionKind;
        set
        {
            if (SetProperty(ref _selectedSectionKind, value))
            {
                _selectedSectionKindOption = SectionKindOptions.FirstOrDefault(option => option.Value == value);
                OnPropertyChanged(nameof(SelectedSectionKindOption));
                RefreshItems();
                RefreshFilteredItems();
            }
        }
    }

    public Ra2SectionKindOptionViewModel? SelectedSectionKindOption
    {
        get => _selectedSectionKindOption;
        set
        {
            if (SetProperty(ref _selectedSectionKindOption, value))
                SelectedSectionKind = value?.Value;
        }
    }

    public Ra2FieldBrowserSearchMode SelectedSearchMode
    {
        get => _selectedSearchMode;
        set
        {
            if (SetProperty(ref _selectedSearchMode, value))
            {
                _selectedSearchModeOption = SearchModeOptions.FirstOrDefault(option => option.Value == value);
                OnPropertyChanged(nameof(SelectedSearchModeOption));
                RefreshItems();
                RefreshFilteredItems();
            }
        }
    }

    public Ra2FieldBrowserSearchModeOptionViewModel? SelectedSearchModeOption
    {
        get => _selectedSearchModeOption;
        set
        {
            if (SetProperty(ref _selectedSearchModeOption, value))
                SelectedSearchMode = value?.Value ?? Ra2FieldBrowserSearchMode.Applicable;
        }
    }

    public Ra2AddPropertyItemViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                if (value is not null)
                    OptionText = value.InsertKey;

                ValueHintText = _valueHintProvider.GetHint(value);
                OnPropertyChanged(nameof(CanInsert));
                OnPropertyChanged(nameof(CanEditAnnotation));
            }
        }
    }

    public string OptionText
    {
        get => _optionText;
        set
        {
            if (SetProperty(ref _optionText, value ?? string.Empty))
            {
                OnPropertyChanged(nameof(CanInsert));
                RefreshPreviewAndWarnings();
            }
        }
    }

    public string ValueText
    {
        get => _valueText;
        set
        {
            if (SetProperty(ref _valueText, value ?? string.Empty))
                RefreshPreviewAndWarnings();
        }
    }

    public bool CanInsert
        => _editorState != Ra2EditorDocumentState.ReadOnlyPreview &&
           !string.IsNullOrWhiteSpace(OptionText) &&
           !OptionText.Contains('=');

    public bool CanConfirm
        => DuplicateAction.HasDuplicate
            ? DuplicateAction.CanConfirmSelectedAction
            : CanInsert;

    public string ConfirmButtonText
        => DuplicateAction.HasDuplicate ? "执行操作" : "添加选中项";

    public bool CanEditAnnotation => SelectedItem is not null;

    public Ra2AddPropertyDuplicateActionViewModel DuplicateAction
    {
        get => _duplicateAction;
        private set
        {
            if (ReferenceEquals(_duplicateAction, value))
                return;

            _duplicateAction.PropertyChanged -= DuplicateAction_OnPropertyChanged;
            _duplicateAction = value;
            _duplicateAction.PropertyChanged += DuplicateAction_OnPropertyChanged;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasDuplicate));
            OnPropertyChanged(nameof(DuplicateActions));
            OnPropertyChanged(nameof(SelectedDuplicateAction));
            OnPropertyChanged(nameof(DuplicateActionWarningText));
            OnPropertyChanged(nameof(CanConfirm));
            OnPropertyChanged(nameof(ConfirmButtonText));
        }
    }

    public bool HasDuplicate => DuplicateAction.HasDuplicate;

    public IReadOnlyList<Ra2DuplicateKeyAction> DuplicateActions => DuplicateAction.AvailableActions;

    public Ra2DuplicateKeyAction SelectedDuplicateAction
    {
        get => DuplicateAction.SelectedAction;
        set
        {
            DuplicateAction.SelectedAction = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanConfirm));
        }
    }

    public string DuplicateActionWarningText => DuplicateAction.WarningText;

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public string ValueHintText
    {
        get => _valueHintText;
        private set => SetProperty(ref _valueHintText, value);
    }

    public string InsertPreviewText
    {
        get => _insertPreviewText;
        private set => SetProperty(ref _insertPreviewText, value);
    }

    public string DuplicateWarningText
    {
        get => _duplicateWarningText;
        private set => SetProperty(ref _duplicateWarningText, value);
    }

    public string ReadOnlyHintText
        => _editorState == Ra2EditorDocumentState.ReadOnlyPreview
            ? "当前没有可编辑文件。"
            : string.Empty;

    public void RefreshDisplay(
        IRa2FieldDisplayResolver displayResolver,
        Ra2FieldAnnotationStatusViewModel annotationStatus)
    {
        _displayResolver = displayResolver ?? throw new ArgumentNullException(nameof(displayResolver));
        _effectiveFieldCatalog = new Ra2EffectiveFieldCatalog(_displayResolver);
        _annotationStatus = annotationStatus ?? throw new ArgumentNullException(nameof(annotationStatus));
        string? selectedKey = SelectedItem?.Key;
        RefreshItems();
        RefreshFilteredItems();
        SelectedItem = FilteredItems.FirstOrDefault(item =>
            string.Equals(item.Key, selectedKey, StringComparison.OrdinalIgnoreCase)) ??
            FilteredItems.FirstOrDefault();
        OnPropertyChanged(nameof(AnnotationStatusText));
        OnPropertyChanged(nameof(HasAnnotationWarnings));
        OnPropertyChanged(nameof(AnnotationWarningsText));
    }

    public void RefreshFilteredItems()
    {
        FilteredItems.Clear();
        IEnumerable<Ra2AddPropertyItemViewModel> matches = Items
            .Select(item => item.WithMatch(GetMatchResult(item)))
            .Where(item => item.MatchResult.IsMatch)
            .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderBy(item => item.MatchPriority)
                .ThenBy(item => GetApplicabilityPriority(item))
                .ThenBy(item => item.SectionKind.ToString(), StringComparer.OrdinalIgnoreCase)
                .First())
            .OrderBy(item => item.MatchPriority)
            .ThenBy(item => item.IsRecent ? 0 : 1)
            .ThenBy(item => GetApplicabilityPriority(item))
            .ThenBy(item => item.Key, StringComparer.OrdinalIgnoreCase);

        foreach (Ra2AddPropertyItemViewModel item in matches)
            FilteredItems.Add(item);

        StatusText = CreateStatusText();
    }

    private void RefreshItems()
    {
        Items.Clear();
        HashSet<string> recentKeys = GetRecentKeys();
        foreach (Ra2EffectiveFieldItem item in EnumerateFieldItems())
        {
            bool isRecent = recentKeys.Contains(item.Key);
            if (SelectedSearchMode == Ra2FieldBrowserSearchMode.Recent && !isRecent)
                continue;

            Items.Add(new Ra2AddPropertyItemViewModel(
                item.SectionKind,
                item.DisplayInfo,
                isRecent,
                item.Applicability));
        }
    }

    private Ra2FieldBrowserMatchResult GetMatchResult(Ra2AddPropertyItemViewModel item)
    {
        Ra2FieldBrowserMatchResult result = _matcher.Match(item.DisplayInfo, SearchText);
        if (SelectedSearchMode == Ra2FieldBrowserSearchMode.Recent &&
            string.IsNullOrWhiteSpace(SearchText) &&
            item.IsRecent)
        {
            return Ra2FieldBrowserMatchResult.Recent;
        }

        return result;
    }

    private string CreateStatusText()
    {
        if (FilteredItems.Count > 0)
            return $"共 {FilteredItems.Count} 个字段。";

        if (SelectedSearchMode == Ra2FieldBrowserSearchMode.Recent && string.IsNullOrWhiteSpace(SearchText))
            return "暂无最近使用字段。";

        string text = "未找到匹配字段。可以尝试输入字段英文名、中文显示名、别名或备注关键词。";
        if (SelectedSearchMode == Ra2FieldBrowserSearchMode.Specific)
            text += " 当前类型独有字段中没有匹配项，可切换到“当前可用字段”或“全部字段”。";

        if (!_annotationStatus.IsLoaded)
            text += " 当前未加载注释库，中文搜索结果可能较少。";

        return text;
    }

    public bool ClearSearchForEscape()
    {
        if (string.IsNullOrEmpty(SearchText))
            return false;

        SearchText = string.Empty;
        return true;
    }

    public bool TryConfirmFromKeyboard()
    {
        if (_editorState == Ra2EditorDocumentState.ReadOnlyPreview)
        {
            StatusText = "当前没有可编辑文件。";
            return false;
        }

        if (SelectedItem is null)
        {
            StatusText = "请先选择要添加的字段。";
            return false;
        }

        if (!CanInsert)
        {
            StatusText = "当前字段不能添加，请检查字段名。";
            return false;
        }

        if (!CanConfirm)
        {
            StatusText = DuplicateActionWarningText;
            return false;
        }

        return true;
    }

    private IEnumerable<Ra2EffectiveFieldItem> EnumerateFieldItems()
    {
        return SelectedSearchMode switch
        {
            Ra2FieldBrowserSearchMode.Common => _effectiveFieldCatalog.GetCommonFields(),
            Ra2FieldBrowserSearchMode.Specific => EnumerateSpecificFields(),
            Ra2FieldBrowserSearchMode.Recent => _effectiveFieldCatalog.GetAllFields(),
            Ra2FieldBrowserSearchMode.All => _effectiveFieldCatalog.GetAllFields(),
            _ => EnumerateApplicableFields()
        };
    }

    private IEnumerable<Ra2EffectiveFieldItem> EnumerateApplicableFields()
    {
        if (SelectedSectionKind is Ra2SectionKind kind)
            return _effectiveFieldCatalog.GetApplicableFields(kind);

        return _effectiveFieldCatalog.GetAllFields();
    }

    private IEnumerable<Ra2EffectiveFieldItem> EnumerateSpecificFields()
    {
        if (SelectedSectionKind is Ra2SectionKind kind)
            return _effectiveFieldCatalog.GetSpecificFields(kind);

        return Enum.GetValues<Ra2SectionKind>()
            .Where(kind => kind != Ra2SectionKind.Unknown)
            .SelectMany(kind => _effectiveFieldCatalog.GetSpecificFields(kind))
            .GroupBy(item => item.Key, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(item => item.Key, StringComparer.OrdinalIgnoreCase);
    }

    private HashSet<string> GetRecentKeys()
    {
        HashSet<string> result = new(StringComparer.OrdinalIgnoreCase);
        if (_recentFieldUsageTracker is null)
            return result;

        foreach (Ra2SectionKind kind in EnumerateKinds())
        {
            foreach (Ra2RecentFieldUsageItem item in _recentFieldUsageTracker.GetRecent(kind, 10))
                result.Add(item.Key);
        }

        return result;
    }

    private static int GetApplicabilityPriority(Ra2AddPropertyItemViewModel item)
    {
        return item.Applicability switch
        {
            Ra2FieldApplicabilityKind.SectionSpecific => 0,
            Ra2FieldApplicabilityKind.Common => 1,
            _ => 2
        };
    }

    private void RefreshPreviewAndWarnings()
    {
        string key = OptionText.Trim();
        InsertPreviewText = string.IsNullOrWhiteSpace(key) || key.Contains('=')
            ? "预览："
            : $"预览：{key}={ValueText}";
        RefreshDuplicateAction(key);
        DuplicateWarningText = !string.IsNullOrWhiteSpace(key) &&
                               !key.Contains('=') &&
                               _document is not null &&
                               DuplicateAction.Match is not null
            ? $"当前 Section 可能已包含字段：{key}。"
            : string.Empty;
    }

    private void RefreshDuplicateAction(string key)
    {
        Ra2DuplicateKeyMatch? match = !string.IsNullOrWhiteSpace(key) &&
                                     !key.Contains('=') &&
                                     _document is not null
            ? _duplicateKeyDetector.FindInCurrentSection(_document, _caretOffset, key)
            : null;
        DuplicateAction = CreateDuplicateAction(match);
    }

    private Ra2AddPropertyDuplicateActionViewModel CreateDuplicateAction(Ra2DuplicateKeyMatch? match)
    {
        Ra2AddPropertyDuplicateActionViewModel action = new(match, _editorState);
        action.PropertyChanged += DuplicateAction_OnPropertyChanged;
        return action;
    }

    private void DuplicateAction_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Ra2AddPropertyDuplicateActionViewModel.SelectedAction) or
            nameof(Ra2AddPropertyDuplicateActionViewModel.CanConfirmSelectedAction))
        {
            OnPropertyChanged(nameof(SelectedDuplicateAction));
            OnPropertyChanged(nameof(CanConfirm));
        }
    }

    private IEnumerable<Ra2SectionKind> EnumerateKinds()
    {
        if (SelectedSectionKind is Ra2SectionKind kind)
            return [kind];

        return Enum.GetValues<Ra2SectionKind>().Where(kind => kind != Ra2SectionKind.Unknown);
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
    {
        if (!string.IsNullOrWhiteSpace(propertyName))
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

internal enum Ra2FieldBrowserSearchMode
{
    Applicable,
    Common,
    Specific,
    Recent,
    All,
}

internal sealed class Ra2FieldBrowserSearchModeOptionViewModel
{
    public Ra2FieldBrowserSearchModeOptionViewModel(
        Ra2FieldBrowserSearchMode value,
        string displayName)
    {
        Value = value;
        DisplayName = displayName;
    }

    public Ra2FieldBrowserSearchMode Value { get; }

    public string DisplayName { get; }
}
