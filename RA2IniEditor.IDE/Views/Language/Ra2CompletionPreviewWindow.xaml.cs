using System.Windows;
using RA2IniEditor.IDE.ViewModels.Language;

namespace RA2IniEditor.IDE.Views.Language;

public partial class Ra2CompletionPreviewWindow : Window
{
    internal Ra2CompletionPreviewWindow(Ra2CompletionPreviewViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    internal void Update(Ra2CompletionPreviewViewModel viewModel)
    {
        DataContext = viewModel;
    }
}
