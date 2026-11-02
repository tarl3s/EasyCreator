using System.IO;
using System.Text;

namespace EasyCreator;

/// <summary>Entry for the headless runner tests (<c>--test-runners</c>): writes
/// %TEMP%\easycreator_runnertests.log and returns the number of failures.</summary>
public static class TestEntryRunners
{
    public const string Flag = "--test-runners";

    public static int Run()
    {
        var path = Path.Combine(Path.GetTempPath(), "easycreator_runnertests.log");
        using var file = new StreamWriter(path, false, new UTF8Encoding(false)) { AutoFlush = true };
        var log = TextWriter.Synchronized(file);
        try
        {
            return Task.Run(() => RunnerTests.RunAsync(log)).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            log.WriteLine("CRASH " + ex);
            return 1000;
        }
    }
}
