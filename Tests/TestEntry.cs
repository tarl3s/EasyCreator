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
        var metLog = new StringWriter();
        int met = MetallurgyTests.Run(metLog);
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "easycreator_metallurgy.log"), metLog.ToString());
        log.WriteLine($"Metallurgy: {met} failures (see easycreator_metallurgy.log)");
        f += met;
        var phLog = new StringWriter();
        int ph = PhysicsTests.Run(phLog);
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "easycreator_physics.log"), phLog.ToString());
        log.WriteLine($"Physics: {ph} failures (see easycreator_physics.log)");
        f += ph;
        var vehLog = new StringWriter();
        int veh = VehiclesTests.Run(vehLog);
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "easycreator_vehicles.log"), vehLog.ToString());
        log.WriteLine($"Vehicles: {veh} failures (see easycreator_vehicles.log)");
        f += veh;
        var i18nLog = new StringWriter();
        int i18n = I18nTests.Run(i18nLog);
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "easycreator_i18n.log"), i18nLog.ToString());
        log.WriteLine($"Translations: {i18n} failures (see easycreator_i18n.log)");
        f += i18n;
        return f;
    }
}
