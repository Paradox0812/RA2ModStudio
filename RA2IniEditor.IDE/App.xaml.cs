using System.Windows;
using RA2IniEditor.IDE.Startup;
using RA2IniEditor.IDE.Views;

namespace RA2IniEditor.IDE;

public partial class App : Application
{
    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        Ra2LaunchRequest launchRequest = Ra2LaunchRequestParser.Parse(e.Args);
        ShellWindow shellWindow = new();
        MainWindow = shellWindow;
        shellWindow.Show();
        await shellWindow.OpenLaunchRequestAsync(launchRequest);
    }
}
