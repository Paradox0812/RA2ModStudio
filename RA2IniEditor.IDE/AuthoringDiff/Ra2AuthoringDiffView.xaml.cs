using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;

namespace RA2IniEditor.IDE.AuthoringDiff;

public partial class Ra2AuthoringDiffView : UserControl
{
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
        ApplyResponsiveState(ActualWidth);
    }
    private void View_OnSizeChanged(object sender, SizeChangedEventArgs e) => ApplyResponsiveState(e.NewSize.Width);
    private void ApplyResponsiveState(double width)
    {
        // This control contains only the compact error/warning counts. More detailed
        // diagnostic prose is intentionally absent, so the required counts stay visible.
        DiagnosticSummaryText.Visibility = Visibility.Visible;
        ReturnButton.Content = width < 640 ? "↩" : "返回源文件";
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
