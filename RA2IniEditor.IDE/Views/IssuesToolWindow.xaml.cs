using System.Windows;
using System.Windows.Input;
using RA2IniEditor.IDE.ViewModels;

namespace RA2IniEditor.IDE.Views;

public partial class IssuesToolWindow : Window
{
    public IssuesToolWindow()
    {
        InitializeComponent();
    }

    public event EventHandler<IdeDiagnosticIssueViewModel?>? IssueNavigateRequested;
    public event EventHandler? ClearIssuesRequested;
    public event EventHandler? ClearIssueFiltersRequested;
    public event EventHandler? RefreshCurrentFileDiagnosticsRequested;
    public event EventHandler? RunManualFullDiagnosticsRequested;

    private void IssuesGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        IdeDiagnosticIssueViewModel? issue = (DataContext as ShellViewModel)?.Issues.SelectedIssue;
        IssueNavigateRequested?.Invoke(this, issue);
    }

    private void ClearIssues(object sender, RoutedEventArgs e)
        => ClearIssuesRequested?.Invoke(this, EventArgs.Empty);

    private void ClearIssueFilters(object sender, RoutedEventArgs e)
        => ClearIssueFiltersRequested?.Invoke(this, EventArgs.Empty);

    private void RefreshCurrentFileDiagnostics(object sender, RoutedEventArgs e)
        => RefreshCurrentFileDiagnosticsRequested?.Invoke(this, EventArgs.Empty);

    private void RunManualFullDiagnostics(object sender, RoutedEventArgs e)
        => RunManualFullDiagnosticsRequested?.Invoke(this, EventArgs.Empty);
}
