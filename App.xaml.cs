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
        int lang = Array.IndexOf(e.Args, "--lang");
        if (lang >= 0 && lang + 1 < e.Args.Length) L.Set(e.Args[lang + 1], save: false);
        else if (!e.Args.Contains("--test")) L.Init();
        if (TestEntry.TryRun(e.Args)) return;
        MainWindow = new MainWindow();
        MainWindow.Show();
    }
}
