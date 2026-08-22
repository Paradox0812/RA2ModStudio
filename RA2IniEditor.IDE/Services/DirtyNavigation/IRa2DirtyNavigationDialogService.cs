using System.Windows;

namespace RA2IniEditor.IDE.Services.DirtyNavigation;

internal interface IRa2DirtyNavigationDialogService
{
    Ra2DirtyNavigationDecision ShowDirtyNavigationDialog(Window owner, string filePath);
}
