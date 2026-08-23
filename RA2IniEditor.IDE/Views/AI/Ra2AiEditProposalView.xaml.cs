using System.Windows;
using System.Windows.Controls;

namespace RA2IniEditor.IDE.Views.AI;

public partial class Ra2AiEditProposalView : UserControl
{
    public Ra2AiEditProposalView()
        => InitializeComponent();

    internal event EventHandler? ApplyRequested;

    internal event EventHandler? DismissRequested;

    internal event EventHandler? OpenDiffRequested;

    private void ApplyButton_OnClick(object sender, RoutedEventArgs e)
        => ApplyRequested?.Invoke(this, EventArgs.Empty);

    private void DismissButton_OnClick(object sender, RoutedEventArgs e)
        => DismissRequested?.Invoke(this, EventArgs.Empty);

    private void OpenDiffButton_OnClick(object sender, RoutedEventArgs e)
        => OpenDiffRequested?.Invoke(this, EventArgs.Empty);
}
