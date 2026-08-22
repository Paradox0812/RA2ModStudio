using System.Windows;
using RA2IniEditor.IDE.Diagnostics;

namespace RA2IniEditor.IDE.Services.SavePreflight;

internal interface IRa2SavePreflightConfirmationService
{
    bool ConfirmContinue(Window owner, Ra2SavePreflightResult result);
}
