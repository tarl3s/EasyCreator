using System.IO;
using System.Windows;

namespace EasyCreator;

/// <summary>Headless test modes: --test runs every suite and writes %TEMP%\easycreator_test.log, then exits.</summary>
public static partial class TestEntry
{
    public static bool TryRun(string[] args)
    {
        if (args.Contains("--snap") || args.Contains("--snap-labs"))
        {
            Progress.ReadOnly = true;
            var w = new MainWindow();
            Application.Current.MainWindow = w;
            w.Show();
            Snapshots(w);
            return true;
        }
        if (!args.Contains("--test")) return false;
        var log = new StringWriter();
        int failures = 0;
        try
        {
            log.WriteLine($"Content: {ContentStore.Tracks.Count} tracks, errors: {string.Join("; ", ContentStore.Errors)}");
            failures += ContentStore.Errors.Count;
            failures += RunSuites(log);
        }
        catch (Exception ex) { log.WriteLine("CRASH " + ex); failures++; }
        log.WriteLine(failures == 0 ? "ALL PASS" : $"FAILURES: {failures}");
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "easycreator_test.log"), log.ToString());
        Application.Current.Shutdown(failures == 0 ? 0 : 1);
        return true;
    }

    static int RunSuites(TextWriter log)
    {
        int r = TestEntryRunners.Run();
        log.WriteLine($"Runners: {r} failures (see easycreator_runnertests.log)");
        int f = r;
        int a = Tests.ArduinoTestEntry.RunToLog();
        log.WriteLine($"Arduino/C: {a} failures (see easycreator_arduino_tests.log)");
        f += a;
        var labLog = new StringWriter();
        int l = LabTests.Run(labLog);
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "easycreator_labtests.log"), labLog.ToString());
        log.WriteLine($"Labs/quizzes: {l} failures (see easycreator_labtests.log)");
        f += l;
        var elLog = new StringWriter();
        int el = ElectricalTests.Run(elLog);
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "easycreator_electrical.log"), elLog.ToString());
        log.WriteLine($"Electrical: {el} failures (see easycreator_electrical.log)");
        f += el;
        return f;
    }
}
