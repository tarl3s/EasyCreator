using System.IO;
using System.Windows;

namespace EasyCreator;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        Runners.RunnerRegistry.Register(new CInterp.CRunner());
        if (TestEntry.TryRun(e.Args)) return;
        MainWindow = new MainWindow();
        MainWindow.Show();
    }
}
