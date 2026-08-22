using System.Windows;
using RA2IniEditor.IDE.Views;

namespace RA2IniEditor.IDE;

public partial class App : Application
{
    private const string AutomationOpenFolderArgument = "--automation-open-folder";

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        ShellWindow shellWindow = new();
        MainWindow = shellWindow;
        shellWindow.Show();

        string? automationOpenFolderPath = TryGetAutomationOpenFolderPath(e.Args);
        if (automationOpenFolderPath is null)
            return;

        await shellWindow.OpenProjectFolderForAutomationAsync(automationOpenFolderPath);
    }

    private static string? TryGetAutomationOpenFolderPath(IReadOnlyList<string> args)
    {
        for (int index = 0; index < args.Count; index++)
        {
            if (!string.Equals(args[index], AutomationOpenFolderArgument, StringComparison.OrdinalIgnoreCase))
                continue;

            return index + 1 < args.Count ? args[index + 1] : string.Empty;
        }

        return null;
    }
}
