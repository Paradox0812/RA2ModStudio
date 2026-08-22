using System.Windows.Controls;
using System.Windows.Input;
using RA2IniEditor.IDE.ViewModels.Language;

namespace RA2IniEditor.IDE.Views.Language;

public partial class Ra2CompletionDropdownView : UserControl
{
    internal event EventHandler? CompletionDropdownInteracted;

    internal event EventHandler<Ra2CompletionDropdownItemViewModel>? CompletionItemDoubleClicked;

    internal event EventHandler? CompletionCommitRequested;

    internal event EventHandler? CompletionCloseRequested;

    public Ra2CompletionDropdownView()
    {
        InitializeComponent();
    }

    private void ItemsList_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        CompletionDropdownInteracted?.Invoke(this, EventArgs.Empty);
    }

    private void ItemsList_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListBox { SelectedItem: Ra2CompletionDropdownItemViewModel item })
        {
            CompletionItemDoubleClicked?.Invoke(this, item);
            e.Handled = true;
        }
    }

    private void ItemsList_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is Key.Enter or Key.Tab)
        {
            CompletionCommitRequested?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Escape)
        {
            CompletionCloseRequested?.Invoke(this, EventArgs.Empty);
            e.Handled = true;
        }
    }
}
