using System.ComponentModel;
using System.Windows;
using RA2IniEditor.IDE.ViewModels.FieldAnnotations;

namespace RA2IniEditor.IDE.Views.FieldAnnotations;

internal partial class Ra2FieldAnnotationEditorWindow : Window
{
    public Ra2FieldAnnotationEditorWindow(Ra2FieldAnnotationEditorViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
    }

    public event EventHandler? AnnotationSaved;

    internal Ra2FieldAnnotationEditorViewModel ViewModel => (Ra2FieldAnnotationEditorViewModel)DataContext;

    private void SaveAndCloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!TrySaveAnnotation())
            return;

        Close();
    }

    private void ApplyButton_OnClick(object sender, RoutedEventArgs e)
        => TrySaveAnnotation();

    private bool TrySaveAnnotation()
    {
        if (!ViewModel.Save())
            return false;

        AnnotationSaved?.Invoke(this, EventArgs.Empty);
        return true;
    }

    private void CreateLibraryButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (ViewModel.CreateLibrary())
            AnnotationSaved?.Invoke(this, EventArgs.Empty);
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        => Close();

    protected override void OnClosing(CancelEventArgs e)
    {
        if (ViewModel.IsDirty)
        {
            MessageBoxResult result = MessageBox.Show(
                this,
                "字段注释有未保存修改，是否放弃并关闭？",
                "字段注释编辑",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
                return;
            }
        }

        base.OnClosing(e);
    }
}
