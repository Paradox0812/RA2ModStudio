using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RA2IniEditor.IDE.ViewModels;

namespace RA2IniEditor.IDE.Views;

internal partial class SearchToolView : UserControl
{
    public SearchToolView()
    {
        InitializeComponent();
    }

    public event EventHandler? SearchRequested;

    public event Action<SearchResultItemViewModel>? ResultNavigateRequested;

    public event EventHandler? ReplacePreviewRequested;

    public event EventHandler? ReplaceApplyRequested;

    internal SearchToolWindowViewModel ViewModel
        => (SearchToolWindowViewModel)DataContext;

    private void FindAllButton_OnClick(object sender, RoutedEventArgs e)
        => RequestSearch();

    private void FindPreviousButton_OnClick(object sender, RoutedEventArgs e)
        => MoveAndNavigate(-1);

    private void FindNextButton_OnClick(object sender, RoutedEventArgs e)
        => MoveAndNavigate(1);

    private void PreviewReplaceAllButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.CanPreviewReplace)
            ReplacePreviewRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ApplyReplaceAllButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.CanApplyReplace)
            ReplaceApplyRequested?.Invoke(this, EventArgs.Empty);
    }

    private void QueryTextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        RequestSearch();
        e.Handled = true;
    }

    private void ResultsList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        => NavigateSelectedResult();

    private void ResultsList_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        NavigateSelectedResult();
        e.Handled = true;
    }

    private void RequestSearch()
    {
        if (ViewModel.CanSearch)
            SearchRequested?.Invoke(this, EventArgs.Empty);
    }

    private void MoveAndNavigate(int delta)
    {
        SearchResultItemViewModel? result = ViewModel.MoveSelection(delta);
        if (result is not null)
        {
            ResultsList.ScrollIntoView(result);
            ResultNavigateRequested?.Invoke(result);
        }
    }

    private void NavigateSelectedResult()
    {
        if (ViewModel.SelectedResult is { } result)
            ResultNavigateRequested?.Invoke(result);
    }
}
