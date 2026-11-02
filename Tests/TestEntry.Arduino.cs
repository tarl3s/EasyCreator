using System.IO;
namespace EasyCreator.Tests;

/// <summary>
/// Command-line entry for the headless Arduino/C tests. The App calls it at startup:
/// <code>
/// if (ArduinoTestEntry.TryRun(e.Args, out int code)) { Shutdown(code); return; }
/// </code>
/// With <c>--test-arduino</c> (or <c>--test</c>, if the App routes it here) it runs <see cref="ArduinoTests.Run"/>,
/// writes the log to <c>%TEMP%\easycreator_arduino_tests.log</c> (also to the console when one is attached)
/// and returns true with the number of failures as exit code.
/// </summary>
public static class ArduinoTestEntry
{
    public const string Flag = "--test-arduino";

    public static bool TryRun(string[] args, out int exitCode)
    {
        exitCode = 0;
        if (args == null || !args.Any(a => string.Equals(a, Flag, StringComparison.OrdinalIgnoreCase))) return false;
        exitCode = RunToLog();
        return true;
    }

    /// <summary>Runs the tests and writes the log file; returns the number of failures.</summary>
    public static int RunToLog()
    {
        string path = Path.Combine(Path.GetTempPath(), "easycreator_arduino_tests.log");
        var sw = new StringWriter();
        int failures;
        try { failures = ArduinoTests.Run(sw); }
        catch (Exception e) { sw.WriteLine("FAIL test entry — " + e); failures = 1; }
        string text = sw.ToString();
        try { File.WriteAllText(path, text); } catch { /* log is best effort */ }
        try { Console.Out.Write(text); } catch { }
        return failures;
    }
}
