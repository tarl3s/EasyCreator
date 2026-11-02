using System.IO;
using System.Windows;

namespace EasyCreator;

/// <summary>Headless test modes: --test runs every suite and writes %TEMP%\easycreator_test.log, then exits.</summary>
public static partial class TestEntry
{
    public static bool TryRun(string[] args)
    {
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
        int f = TestEntryRunners.Run();
        int a = Tests.ArduinoTestEntry.RunToLog();
        log.WriteLine($"Arduino/C: {a} failures (see easycreator_arduino_tests.log)");
        f += a;
        log.WriteLine($"Runners: {f} failures (see easycreator_runnertests.log)");
        return f;
    }
}
