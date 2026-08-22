using System.Windows;
using RA2IniEditor.IDE.Views.DirtyNavigation;

namespace RA2IniEditor.IDE.Services.DirtyNavigation;

internal sealed class Ra2DirtyNavigationDialogService : IRa2DirtyNavigationDialogService
{
    public Ra2DirtyNavigationDecision ShowDirtyNavigationDialog(Window owner, string filePath)
    {
        Ra2DirtyNavigationDialog dialog = new(filePath)
        {
            Owner = owner
        };

        bool? result = dialog.ShowDialog();
        return result == true ? dialog.Decision : Ra2DirtyNavigationDecision.Cancel;
    }
}
