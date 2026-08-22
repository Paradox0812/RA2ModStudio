using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using RA2IniEditor.IDE.ViewModels.FieldDetails;
using RA2IniEditor.IDE.ViewModels.Language;

namespace RA2IniEditor.IDE.Views.FieldQuickPeek;

internal partial class Ra2FieldQuickPeekWindow : Window
{
    public Ra2FieldQuickPeekWindow(Ra2FieldDetailsViewModel viewModel)
    {
        InitializeComponent();
        ApplyBorderlessFloatingHostOptions();
        DataContext = viewModel;
    }

    public void Update(Ra2FieldDetailsViewModel viewModel)
    {
        DataContext = viewModel;
    }

    public void PlaceNearCaret(Point caretBottomScreenDip)
    {
        Size inspectorSize = Ra2FloatingInspectorPlacement.MeasureInspector(this);
        Point placement = Ra2FloatingInspectorPlacement.PlaceNearCaret(
            caretBottomScreenDip,
            inspectorSize,
            SystemParameters.WorkArea);
        Left = placement.X;
        Top = placement.Y;
    }

    private void Window_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape)
            return;

        Close();
        e.Handled = true;
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        => Close();

    private void ApplyBorderlessFloatingHostOptions()
    {
        WindowStyle = WindowStyle.None;
        ShowInTaskbar = false;
        ResizeMode = ResizeMode.NoResize;
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.Manual;
        AllowsTransparency = true;
        Background = Brushes.Transparent;
    }
}
