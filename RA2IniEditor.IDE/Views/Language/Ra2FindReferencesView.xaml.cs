using System.Windows.Controls;
using System.Windows.Input;
using RA2IniEditor.IDE.ViewModels.Language;

namespace RA2IniEditor.IDE.Views.Language;

internal partial class Ra2FindReferencesView : UserControl
{
    public Ra2FindReferencesView()
    {
        InitializeComponent();
    }

    internal event EventHandler<Ra2ReferenceItemViewModel>? ReferenceNavigateRequested;

    private void ReferencesGrid_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (ReferencesGrid.SelectedItem is Ra2ReferenceItemViewModel item)
            ReferenceNavigateRequested?.Invoke(this, item);
    }
}
