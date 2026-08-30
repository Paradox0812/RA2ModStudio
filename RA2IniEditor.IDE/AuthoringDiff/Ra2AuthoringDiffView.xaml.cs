using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RA2IniEditor.IDE.Highlighting;

namespace RA2IniEditor.IDE.AuthoringDiff;

public partial class Ra2AuthoringDiffView : UserControl
{
    private readonly Ra2AuthoringResultChangeRenderer _changeRenderer = new();
    private Ra2KnownFieldHighlightingTransformer? _resultHighlighter;
    private Ra2KnownFieldHighlightingTransformer? _contextHighlighter;
    private Ra2AuthoringDiffViewModel? _subscribedViewModel;
    private bool _compactOutlineInitialized;

    public Ra2AuthoringDiffView() => InitializeComponent();
    internal event EventHandler? ApplyAllRequested;
    internal event EventHandler? DismissRequested;
    internal event EventHandler? ReturnToSourceRequested;

    private void ApplyButton_OnClick(object sender, RoutedEventArgs e) => ApplyAllRequested?.Invoke(this, EventArgs.Empty);
    private void DismissButton_OnClick(object sender, RoutedEventArgs e) => DismissRequested?.Invoke(this, EventArgs.Empty);
    private void ReturnButton_OnClick(object sender, RoutedEventArgs e) => ReturnToSourceRequested?.Invoke(this, EventArgs.Empty);

    private void View_OnLoaded(object sender, RoutedEventArgs e)
    {
        if (FindVisualChild<ScrollViewer>(RowsList) is { } scrollViewer)
            AutomationProperties.SetAutomationId(scrollViewer, "Shell.AuthoringDiff.ScrollViewer");
        AutomationProperties.SetAutomationId(ResultEditor.TextArea, "Shell.AuthoringDiff.ResultEditor.TextArea");
        ResultEditor.TextArea.TextView.BackgroundRenderers.Add(_changeRenderer);
        SubscribeViewModel();
        ApplyResponsiveState(ActualWidth);
        RefreshEditorsAndNavigate();
    }

    private void View_OnUnloaded(object sender, RoutedEventArgs e)
    {
        if (_subscribedViewModel is not null)
            _subscribedViewModel.PropertyChanged -= ViewModel_OnPropertyChanged;
        _subscribedViewModel = null;
        ResultEditor.TextArea.TextView.BackgroundRenderers.Remove(_changeRenderer);
        RemoveHighlighters();
    }

    private void SubscribeViewModel()
    {
        if (ReferenceEquals(_subscribedViewModel, DataContext))
            return;
        if (_subscribedViewModel is not null)
            _subscribedViewModel.PropertyChanged -= ViewModel_OnPropertyChanged;
        _subscribedViewModel = DataContext as Ra2AuthoringDiffViewModel;
        if (_subscribedViewModel is not null)
            _subscribedViewModel.PropertyChanged += ViewModel_OnPropertyChanged;
    }

    private void ViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(Ra2AuthoringDiffViewModel.SelectedDocument) or
            nameof(Ra2AuthoringDiffViewModel.SelectedOutlineItem) or
            nameof(Ra2AuthoringDiffViewModel.IsLoading))
        {
            Dispatcher.BeginInvoke(RefreshEditorsAndNavigate);
        }
    }

    private void ResultModeButton_OnClick(object sender, RoutedEventArgs e) => SetMode(Ra2AuthoringReviewMode.Result);
    private void ChangesModeButton_OnClick(object sender, RoutedEventArgs e) => SetMode(Ra2AuthoringReviewMode.Changes);
    private void ContextModeButton_OnClick(object sender, RoutedEventArgs e) => SetMode(Ra2AuthoringReviewMode.ObjectContext);

    private void SetMode(Ra2AuthoringReviewMode mode)
    {
        if (DataContext is not Ra2AuthoringDiffViewModel viewModel)
            return;
        viewModel.SetMode(mode);
        if (mode == Ra2AuthoringReviewMode.Result)
            NavigateToSelectedOutline();
        else if (mode == Ra2AuthoringReviewMode.ObjectContext)
            ContextEditor.Focus();
    }

    private void DocumentSelector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        => RefreshEditorsAndNavigate();

    private void OutlineList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        => RefreshEditorsAndNavigate();

    private void PreviousChangeButton_OnClick(object sender, RoutedEventArgs e) => MoveChange(-1);
    private void NextChangeButton_OnClick(object sender, RoutedEventArgs e) => MoveChange(1);

    private void MoveChange(int delta)
    {
        if (DataContext is not Ra2AuthoringDiffViewModel viewModel)
            return;
        if (viewModel.IsChangesMode)
        {
            Ra2AuthoringDiffRow[] rows = viewModel.Rows.Where(row => row.Kind == Ra2AuthoringDiffRowKind.HunkHeader).ToArray();
            if (rows.Length == 0)
                return;
            int current = Array.IndexOf(rows, RowsList.SelectedItem as Ra2AuthoringDiffRow);
            int next = (current + delta) % rows.Length;
            if (next < 0)
                next += rows.Length;
            RowsList.SelectedItem = rows[next];
            RowsList.ScrollIntoView(rows[next]);
            return;
        }

        if (viewModel.MoveChange(delta) is { } location)
            NavigateResult(location.CandidateOffset, location.CandidateLength, location.CandidateLineNumber);
    }

    private void RefreshEditorsAndNavigate()
    {
        if (DataContext is not Ra2AuthoringDiffViewModel viewModel || viewModel.SelectedDocument is null)
            return;
        ConfigureHighlighters(viewModel.SelectedDocument);
        _changeRenderer.SetLocations(viewModel.SelectedDocument.ChangedLocations);
        ResultEditor.TextArea.TextView.InvalidateLayer(ICSharpCode.AvalonEdit.Rendering.KnownLayer.Background);
        if (viewModel.SelectedOutlineItem?.Kind == Ra2AuthoringReviewOutlineKind.Related)
            ContextEditor.ScrollToLine(1);
        NavigateToSelectedOutline();
    }

    private void ConfigureHighlighters(Ra2AuthoringReviewDocument document)
    {
        RemoveHighlighters();
        ReadonlyIniHighlightTokenizer tokenizer = new(document.FieldProvider);
        _resultHighlighter = new Ra2KnownFieldHighlightingTransformer(tokenizer);
        _contextHighlighter = new Ra2KnownFieldHighlightingTransformer(tokenizer);
        ResultEditor.TextArea.TextView.LineTransformers.Add(_resultHighlighter);
        ContextEditor.TextArea.TextView.LineTransformers.Add(_contextHighlighter);
    }

    private void RemoveHighlighters()
    {
        if (_resultHighlighter is not null)
            ResultEditor.TextArea.TextView.LineTransformers.Remove(_resultHighlighter);
        if (_contextHighlighter is not null)
            ContextEditor.TextArea.TextView.LineTransformers.Remove(_contextHighlighter);
        _resultHighlighter = null;
        _contextHighlighter = null;
    }

    private void NavigateToSelectedOutline()
    {
        if (DataContext is not Ra2AuthoringDiffViewModel viewModel ||
            !viewModel.IsResultMode ||
            viewModel.SelectedOutlineItem is not { IsExecutableChange: true } item)
            return;
        NavigateResult(item.CandidateOffset, item.CandidateLength, item.CandidateLineNumber);
    }

    private void NavigateResult(int offset, int length, int lineNumber)
    {
        int boundedOffset = Math.Clamp(offset, 0, ResultEditor.Document.TextLength);
        int boundedLength = Math.Clamp(length, 0, ResultEditor.Document.TextLength - boundedOffset);
        ResultEditor.Select(boundedOffset, boundedLength);
        ResultEditor.ScrollToLine(Math.Max(1, lineNumber));
        ResultEditor.Focus();
    }

    private void View_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control && e.Key is Key.D1 or Key.NumPad1)
        {
            SetMode(Ra2AuthoringReviewMode.Result); e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key is Key.D2 or Key.NumPad2)
        {
            SetMode(Ra2AuthoringReviewMode.Changes); e.Handled = true;
        }
        else if (Keyboard.Modifiers == ModifierKeys.Control && e.Key is Key.D3 or Key.NumPad3)
        {
            SetMode(Ra2AuthoringReviewMode.ObjectContext); e.Handled = true;
        }
        else if (e.Key == Key.F7)
        {
            MoveChange(Keyboard.Modifiers.HasFlag(ModifierKeys.Shift) ? -1 : 1); e.Handled = true;
        }
    }

    private void View_OnSizeChanged(object sender, SizeChangedEventArgs e) => ApplyResponsiveState(e.NewSize.Width);

    private void ApplyResponsiveState(double width)
    {
        DiagnosticSummaryText.Visibility = Visibility.Visible;
        ReturnButton.Content = width < 640 ? "↩" : "返回源文件";
        bool compact = width < 900;
        OutlineToggle.Visibility = compact ? Visibility.Visible : Visibility.Collapsed;
        if (!compact)
        {
            Grid.SetColumnSpan(OutlinePanel, 1);
            OutlinePanel.Width = double.NaN;
            OutlinePanel.HorizontalAlignment = HorizontalAlignment.Stretch;
            OutlineColumn.Width = new GridLength(220);
            OutlineSplitterColumn.Width = new GridLength(5);
            OutlinePanel.Visibility = Visibility.Visible;
            OutlineSplitter.Visibility = Visibility.Visible;
            _compactOutlineInitialized = false;
        }
        else
        {
            if (!_compactOutlineInitialized)
            {
                OutlineToggle.IsChecked = false;
                _compactOutlineInitialized = true;
            }
            ApplyCompactOutline(width);
        }
        if (DataContext is Ra2AuthoringDiffViewModel viewModel)
            viewModel.SetCompactLayout(width < 640);
    }

    private void OutlineToggle_OnChanged(object sender, RoutedEventArgs e)
        => ApplyCompactOutline(ActualWidth);

    private void ApplyCompactOutline(double width)
    {
        bool show = OutlineToggle.IsChecked == true;
        bool overlay = width < 640;
        Grid.SetColumnSpan(OutlinePanel, overlay ? 3 : 1);
        OutlinePanel.HorizontalAlignment = overlay ? HorizontalAlignment.Left : HorizontalAlignment.Stretch;
        OutlinePanel.Width = overlay && show ? Math.Min(280, width * 0.72) : double.NaN;
        Panel.SetZIndex(OutlinePanel, overlay ? 10 : 0);
        OutlineColumn.Width = show && !overlay ? new GridLength(180) : new GridLength(0);
        OutlineSplitterColumn.Width = show && !overlay ? new GridLength(5) : new GridLength(0);
        OutlinePanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        OutlineSplitter.Visibility = show && !overlay ? Visibility.Visible : Visibility.Collapsed;
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match) return match;
            if (FindVisualChild<T>(child) is { } descendant) return descendant;
        }
        return null;
    }
}
