using System.Windows;
using RA2IniEditor.IDE.Diagnostics;

namespace RA2IniEditor.IDE.Views.SavePreflight;

internal partial class SavePreflightConfirmationDialog : Window
{
    public SavePreflightConfirmationDialog(Ra2SavePreflightResult result)
    {
        InitializeComponent();
        SummaryTextBlock.Text = result.SummaryText;
        DetailTextBlock.Text = $"{result.SourceSummaryText}{Environment.NewLine}{result.SeveritySummaryText}";
    }

    private void ContinueButton_OnClick(object sender, RoutedEventArgs e)
        => DialogResult = true;

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        => DialogResult = false;
}
