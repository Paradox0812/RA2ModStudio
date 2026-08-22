using System.Windows;
using RA2IniEditor.IDE.ViewModels;

namespace RA2IniEditor.IDE.Views;

public partial class RemoteSourcePresetEditorWindow : Window
{
    internal RemoteSourcePresetEditorWindow(FieldRegistryRemoteSourcePresetEditModel initial)
    {
        InitializeComponent();
        DataContext = new RemoteSourcePresetEditorViewModel(initial);
    }

    internal FieldRegistryRemoteSourcePresetEditModel? EditModel { get; private set; }

    private void Accept(object sender, RoutedEventArgs e)
    {
        if (DataContext is not RemoteSourcePresetEditorViewModel viewModel)
            return;

        if (!viewModel.Validate())
            return;

        EditModel = viewModel.ToEditModel();
        DialogResult = true;
    }

    private void Cancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}
