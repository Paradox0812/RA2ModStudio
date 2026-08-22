using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using RA2IniEditor.Core;

namespace RA2IniEditor.IDE.ViewModels;

/// <summary>
/// Provides readonly display state for the IDE Issues panel.
/// </summary>
public sealed class IssuesViewModel : INotifyPropertyChanged
{
    private readonly List<IdeDiagnosticIssueViewModel> _allItems = [];
    private IdeDiagnosticIssueViewModel? _selectedIssue;
    private string _statusText = IssuesStatusMessages.NoIssuesFound;
    private string _selectedSeverityFilter = IssuesSeverityFilterNames.All;
    private string _sourceFilterText = string.Empty;
    private string _searchText = string.Empty;
    private int _totalCount;
    private int _filteredCount;
    private int _errorCount;
    private int _warningCount;
    private int _infoCount;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the current filtered issue items.
    /// </summary>
    public ObservableCollection<IdeDiagnosticIssueViewModel> Items { get; } = [];

    /// <summary>
    /// Gets available severity filter labels.
    /// </summary>
    public IReadOnlyList<string> SeverityFilterOptions { get; } =
    [
        IssuesSeverityFilterNames.All,
        IssuesSeverityFilterNames.Error,
        IssuesSeverityFilterNames.Warning,
        IssuesSeverityFilterNames.Info
    ];

    /// <summary>
    /// Gets or sets the selected issue.
    /// </summary>
    public IdeDiagnosticIssueViewModel? SelectedIssue
    {
        get => _selectedIssue;
        set => SetProperty(ref _selectedIssue, value);
    }

    /// <summary>
    /// Gets or sets the selected severity filter.
    /// </summary>
    public string SelectedSeverityFilter
    {
        get => _selectedSeverityFilter;
        set
        {
            if (SetProperty(ref _selectedSeverityFilter, string.IsNullOrWhiteSpace(value) ? IssuesSeverityFilterNames.All : value))
                ApplyFilters();
        }
    }

    /// <summary>
    /// Gets or sets the source text filter.
    /// </summary>
    public string SourceFilterText
    {
        get => _sourceFilterText;
        set
        {
            if (SetProperty(ref _sourceFilterText, value ?? string.Empty))
                ApplyFilters();
        }
    }

    /// <summary>
    /// Gets or sets the free-text issue search filter.
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value ?? string.Empty))
                ApplyFilters();
        }
    }

    /// <summary>
    /// Gets the current issues status text.
    /// </summary>
    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    /// <summary>
    /// Gets the total issue count before filtering.
    /// </summary>
    public int TotalCount
    {
        get => _totalCount;
        private set => SetProperty(ref _totalCount, value);
    }

    /// <summary>
    /// Gets the visible issue count after filtering.
    /// </summary>
    public int FilteredCount
    {
        get => _filteredCount;
        private set => SetProperty(ref _filteredCount, value);
    }

    public int ErrorCount
    {
        get => _errorCount;
        private set => SetProperty(ref _errorCount, value);
    }

    public int WarningCount
    {
        get => _warningCount;
        private set => SetProperty(ref _warningCount, value);
    }

    public int InfoCount
    {
        get => _infoCount;
        private set => SetProperty(ref _infoCount, value);
    }

    /// <summary>
    /// Gets a compact issue count summary.
    /// </summary>
    public string CountText => TotalCount == 0
        ? "0 issues"
        : $"Showing {FilteredCount} / {TotalCount} issues    Errors: {ErrorCount}    Warnings: {WarningCount}    Info: {InfoCount}";

    /// <summary>
    /// Clears the current issue list.
    /// </summary>
    public void Clear(string statusText = IssuesStatusMessages.NoIssuesFound)
    {
        _allItems.Clear();
        Items.Clear();
        SelectedIssue = null;
        TotalCount = 0;
        FilteredCount = 0;
        ErrorCount = 0;
        WarningCount = 0;
        InfoCount = 0;
        StatusText = statusText;
        OnPropertyChanged(nameof(CountText));
    }

    /// <summary>
    /// Clears all active filters without clearing issue results.
    /// </summary>
    public void ClearFilters()
    {
        _selectedSeverityFilter = IssuesSeverityFilterNames.All;
        _sourceFilterText = string.Empty;
        _searchText = string.Empty;
        OnPropertyChanged(nameof(SelectedSeverityFilter));
        OnPropertyChanged(nameof(SourceFilterText));
        OnPropertyChanged(nameof(SearchText));
        ApplyFilters();
    }

    /// <summary>
    /// Replaces the current issue list.
    /// </summary>
    public void ReplaceIssues(IEnumerable<IdeDiagnosticIssueViewModel> issues, string? statusText = null)
    {
        _allItems.Clear();
        _allItems.AddRange(SortAndDeduplicate(issues));
        ApplyFilters(statusText);
    }

    private void ApplyFilters(string? statusText = null)
    {
        IdeDiagnosticIssueViewModel? previousSelection = SelectedIssue;
        Items.Clear();

        foreach (IdeDiagnosticIssueViewModel issue in _allItems.Where(MatchesFilters))
            Items.Add(issue);

        SelectedIssue = previousSelection is not null && Items.Contains(previousSelection)
            ? previousSelection
            : null;
        TotalCount = _allItems.Count;
        FilteredCount = Items.Count;
        ErrorCount = _allItems.Count(issue => issue.Severity == IniIssueSeverity.Error);
        WarningCount = _allItems.Count(issue => issue.Severity == IniIssueSeverity.Warning);
        InfoCount = TotalCount - ErrorCount - WarningCount;
        StatusText = statusText ?? BuildStatusText();
        OnPropertyChanged(nameof(CountText));
    }

    private bool MatchesFilters(IdeDiagnosticIssueViewModel issue)
    {
        if (!MatchesSeverity(issue))
            return false;

        if (!string.IsNullOrWhiteSpace(SourceFilterText) &&
            !ContainsIgnoreCase(issue.SourceText, SourceFilterText) &&
            !ContainsIgnoreCase(Path.GetFileName(issue.FilePath), SourceFilterText))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(SearchText) &&
            !ContainsIgnoreCase(issue.Code, SearchText) &&
            !ContainsIgnoreCase(issue.Message, SearchText) &&
            !ContainsIgnoreCase(issue.LocationText, SearchText) &&
            !ContainsIgnoreCase(issue.SourceText, SearchText) &&
            !ContainsIgnoreCase(Path.GetFileName(issue.FilePath), SearchText) &&
            !ContainsIgnoreCase(issue.SectionId, SearchText) &&
            !ContainsIgnoreCase(issue.Key, SearchText))
        {
            return false;
        }

        return true;
    }

    private bool MatchesSeverity(IdeDiagnosticIssueViewModel issue)
        => SelectedSeverityFilter switch
        {
            IssuesSeverityFilterNames.Error => issue.Severity == IniIssueSeverity.Error,
            IssuesSeverityFilterNames.Warning => issue.Severity == IniIssueSeverity.Warning,
            IssuesSeverityFilterNames.Info => issue.Severity != IniIssueSeverity.Error &&
                                              issue.Severity != IniIssueSeverity.Warning,
            _ => true
        };

    private string BuildStatusText()
    {
        if (TotalCount == 0)
            return IssuesStatusMessages.NoIssuesFound;

        if (FilteredCount == TotalCount)
            return TotalCount == 1 ? "Found 1 issue." : $"Found {TotalCount} issues.";

        return $"Showing {FilteredCount} of {TotalCount} issues.";
    }

    private static IReadOnlyList<IdeDiagnosticIssueViewModel> SortAndDeduplicate(IEnumerable<IdeDiagnosticIssueViewModel> issues)
    {
        HashSet<string> seen = new(StringComparer.Ordinal);
        List<IdeDiagnosticIssueViewModel> results = [];
        foreach (IdeDiagnosticIssueViewModel issue in issues
                     .OrderBy(issue => GetSeverityOrder(issue.Severity))
                     .ThenBy(issue => issue.SourceText, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(issue => issue.FilePath, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(issue => issue.LineNumber ?? int.MaxValue)
                     .ThenBy(issue => issue.ColumnNumber ?? int.MaxValue)
                     .ThenBy(issue => issue.Code, StringComparer.OrdinalIgnoreCase)
                     .ThenBy(issue => issue.Message, StringComparer.OrdinalIgnoreCase))
        {
            string key = string.Join(
                "\u001f",
                issue.Code,
                issue.SourceText,
                issue.FilePath,
                issue.LineNumber?.ToString() ?? string.Empty,
                issue.ColumnNumber?.ToString() ?? string.Empty,
                issue.Message);
            if (seen.Add(key))
                results.Add(issue);
        }

        return results;
    }

    private static int GetSeverityOrder(IniIssueSeverity severity) => severity switch
    {
        IniIssueSeverity.Error => 0,
        IniIssueSeverity.Warning => 1,
        _ => 2
    };

    private static bool ContainsIgnoreCase(string? text, string value)
        => !string.IsNullOrEmpty(text) &&
           text.Contains(value, StringComparison.OrdinalIgnoreCase);

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

internal static class IssuesSeverityFilterNames
{
    public const string All = "All";
    public const string Error = "Error";
    public const string Warning = "Warning";
    public const string Info = "Info";
}
