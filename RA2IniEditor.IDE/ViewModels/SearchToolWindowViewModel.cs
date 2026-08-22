using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using RA2IniEditor.IDE.Search;

namespace RA2IniEditor.IDE.ViewModels;

/// <summary>
/// 提供查找条件、执行状态和结果选择状态。
/// </summary>
public sealed class SearchToolWindowViewModel : INotifyPropertyChanged
{
    private string _query = string.Empty;
    private bool _isCaseSensitive;
    private bool _isWholeWord;
    private bool _useRegex;
    private int _selectedScopeIndex;
    private string _filePattern = "*.ini";
    private string _replacementText = string.Empty;
    private SearchResultItemViewModel? _selectedResult;
    private string _statusText = "输入内容后可查找当前项目。";
    private bool _isBusy;
    private Ra2CurrentFileReplacePlan? _replacePlan;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 获取当前查找文本。
    /// </summary>
    public string Query
    {
        get => _query;
        set
        {
            if (SetProperty(ref _query, value ?? string.Empty))
            {
                InvalidateReplacePlan();
                OnPropertyChanged(nameof(CanSearch));
                OnPropertyChanged(nameof(CanPreviewReplace));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether case-sensitive search is enabled.
    /// </summary>
    public bool IsCaseSensitive
    {
        get => _isCaseSensitive;
        set
        {
            if (SetProperty(ref _isCaseSensitive, value))
                InvalidateReplacePlan();
        }
    }

    /// <summary>
    /// Gets a value indicating whether whole-word search is enabled.
    /// </summary>
    public bool IsWholeWord
    {
        get => _isWholeWord;
        set
        {
            if (SetProperty(ref _isWholeWord, value))
                InvalidateReplacePlan();
        }
    }

    /// <summary>
    /// Gets a value indicating whether regular expression search is enabled.
    /// </summary>
    public bool UseRegex
    {
        get => _useRegex;
        set
        {
            if (SetProperty(ref _useRegex, value))
                InvalidateReplacePlan();
        }
    }

    public int SelectedScopeIndex
    {
        get => _selectedScopeIndex;
        set
        {
            if (!SetProperty(ref _selectedScopeIndex, value))
                return;

            InvalidateReplacePlan();
            OnPropertyChanged(nameof(CanPreviewReplace));
            OnPropertyChanged(nameof(IsCurrentFileScope));
        }
    }

    public string FilePattern
    {
        get => _filePattern;
        set => SetProperty(ref _filePattern, value ?? string.Empty);
    }

    public string ReplacementText
    {
        get => _replacementText;
        set
        {
            if (SetProperty(ref _replacementText, value ?? string.Empty))
                InvalidateReplacePlan();
        }
    }

    /// <summary>
    /// 获取当前结果集合。
    /// </summary>
    public ObservableCollection<SearchResultItemViewModel> Results { get; } =
        [];

    public SearchResultItemViewModel? SelectedResult
    {
        get => _selectedResult;
        set
        {
            if (SetProperty(ref _selectedResult, value))
                OnPropertyChanged(nameof(HasSelectedResult));
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (!SetProperty(ref _isBusy, value))
                return;

            OnPropertyChanged(nameof(CanSearch));
            OnPropertyChanged(nameof(HasSelectedResult));
            OnPropertyChanged(nameof(CanPreviewReplace));
            OnPropertyChanged(nameof(CanApplyReplace));
        }
    }

    public bool CanSearch => !IsBusy && !string.IsNullOrEmpty(Query);

    public bool HasSelectedResult => !IsBusy && SelectedResult is not null;

    public bool IsCurrentFileScope => SelectedScopeIndex == 1;

    public bool CanPreviewReplace => CanSearch && IsCurrentFileScope;

    public bool CanApplyReplace => !IsBusy && _replacePlan?.Success == true;

    internal Ra2SearchOptions CreateOptions()
        => new(
            Query,
            SelectedScopeIndex == 1 ? Ra2SearchScope.CurrentFile : Ra2SearchScope.Project,
            IsCaseSensitive,
            IsWholeWord,
            UseRegex,
            FilePattern);

    internal void BeginSearch()
    {
        InvalidateReplacePlan();
        IsBusy = true;
        StatusText = "正在查找…";
    }

    internal void ApplyReplacePlan(Ra2CurrentFileReplacePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        _replacePlan = plan.Success ? plan : null;
        StatusText = plan.Message;
        OnPropertyChanged(nameof(CanApplyReplace));
    }

    internal Ra2CurrentFileReplacePlan? CurrentReplacePlan => _replacePlan;

    internal void CompleteReplace(string statusText)
    {
        _replacePlan = null;
        Results.Clear();
        SelectedResult = null;
        StatusText = statusText;
        OnPropertyChanged(nameof(CanApplyReplace));
    }

    internal void ApplySearchResult(Ra2SearchExecutionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        Results.Clear();
        foreach (Ra2SearchMatch match in result.Matches)
            Results.Add(new SearchResultItemViewModel(match));

        SelectedResult = Results.FirstOrDefault();
        StatusText = result.StatusText;
        IsBusy = false;
    }

    internal SearchResultItemViewModel? MoveSelection(int delta)
    {
        if (Results.Count == 0)
            return null;

        int currentIndex = SelectedResult is null ? -1 : Results.IndexOf(SelectedResult);
        int nextIndex = (currentIndex + delta) % Results.Count;
        if (nextIndex < 0)
            nextIndex += Results.Count;

        SelectedResult = Results[nextIndex];
        return SelectedResult;
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

    private void InvalidateReplacePlan()
    {
        if (_replacePlan is null)
            return;

        _replacePlan = null;
        OnPropertyChanged(nameof(CanApplyReplace));
    }
}
