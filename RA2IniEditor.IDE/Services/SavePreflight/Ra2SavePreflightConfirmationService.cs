using System.Windows;
using RA2IniEditor.IDE.Diagnostics;
using RA2IniEditor.IDE.Views.SavePreflight;

namespace RA2IniEditor.IDE.Services.SavePreflight;

internal sealed class Ra2SavePreflightConfirmationService : IRa2SavePreflightConfirmationService
{
    public bool ConfirmContinue(Window owner, Ra2SavePreflightResult result)
    {
        SavePreflightConfirmationDialog dialog = new(result)
        {
            Owner = owner
        };

        bool? dialogResult = dialog.ShowDialog();
        return dialogResult == true;
    }
}
