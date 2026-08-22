using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using RA2IniEditor.IDE.ViewModels.FieldBrowser;

namespace RA2IniEditor.IDE.Views.FieldBrowser;

internal partial class Ra2AddPropertyWindow : Window
{
    public Ra2AddPropertyWindow(Ra2AddPropertyViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        Loaded += Ra2AddPropertyWindow_Loaded;
    }

    internal Ra2AddPropertyViewModel ViewModel => (Ra2AddPropertyViewModel)DataContext;

    public event EventHandler? EditAnnotationRequested;

    private void Ra2AddPropertyWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(
            () =>
            {
                SearchTextBox.Focus();
                Keyboard.Focus(SearchTextBox);
            },
            DispatcherPriority.Input);
    }

    private void AddSelectedButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!ViewModel.CanConfirm)
            return;

        DialogResult = true;
        Close();
    }

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void EditAnnotationButton_OnClick(object sender, RoutedEventArgs e)
        => EditAnnotationRequested?.Invoke(this, EventArgs.Empty);

    private void FieldsGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!ViewModel.CanConfirm)
            return;

        DialogResult = true;
        Close();
    }

    private void SearchTextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            if (!ViewModel.ClearSearchForEscape())
            {
                DialogResult = false;
                Close();
            }

            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter)
        {
            if (ViewModel.TryConfirmFromKeyboard())
            {
                DialogResult = true;
                Close();
            }

            e.Handled = true;
        }
    }
}
