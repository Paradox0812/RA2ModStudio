using System.IO;
using System.Windows;
using RA2IniEditor.IDE.Services.DirtyNavigation;

namespace RA2IniEditor.IDE.Views.DirtyNavigation;

internal partial class Ra2DirtyNavigationDialog : Window
{
    public Ra2DirtyNavigationDialog(string filePath)
    {
        InitializeComponent();
        Decision = Ra2DirtyNavigationDecision.Cancel;
        FilePathTextBlock.Text = string.IsNullOrWhiteSpace(filePath)
            ? "当前文件"
            : Path.GetFileName(filePath);
        FilePathTextBlock.ToolTip = filePath;
    }

    internal Ra2DirtyNavigationDecision Decision { get; private set; }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        => Complete(Ra2DirtyNavigationDecision.Save);

    private void DiscardButton_OnClick(object sender, RoutedEventArgs e)
        => Complete(Ra2DirtyNavigationDecision.Discard);

    private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        => Complete(Ra2DirtyNavigationDecision.Cancel);

    private void Complete(Ra2DirtyNavigationDecision decision)
    {
        Decision = decision;
        DialogResult = true;
    }
}
