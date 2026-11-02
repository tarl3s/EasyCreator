namespace EasyCreator;

/// <summary>One graded test. For hidden tests Input, Expected and Actual are empty (only Passed and Error are shown).</summary>
public sealed record TestResult(int Index, bool Passed, bool Hidden, string Input, string Expected, string Actual, string Error, TimeSpan Elapsed);

/// <summary>Runs a lesson's io tests against student code.</summary>
public static class Grader
{
    public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

    /// <summary>Runs every test of <paramref name="lesson"/> in order. After a timeout the remaining tests are not
    /// run (they would almost surely time out too) and are reported as failed.</summary>
    public static async Task<List<TestResult>> GradeAsync(ILanguageRunner runner, Lesson lesson, string code,
        CancellationToken ct = default, TimeSpan? timeout = null)
    {
        var limit = timeout ?? DefaultTimeout;
        var results = new List<TestResult>();
        string? skipReason = null;
        for (var i = 0; i < lesson.Tests.Count; i++)
        {
            var t = lesson.Tests[i];
            if (skipReason != null || ct.IsCancellationRequested)
            {
                results.Add(Make(i, t, false, "", skipReason ?? Runners.RunnerUtil.CanceledMessage, TimeSpan.Zero));
                continue;
            }
            RunResult r;
            try { r = await runner.RunAsync(code, t.Input ?? "", limit, ct).ConfigureAwait(false); }
            catch (Exception ex) { r = new RunResult(false, "", L.F("Erro interno: {0}", ex.Message), TimeSpan.Zero); }

            var passed = r.Ok && OutputMatches(t.Expected, r.Stdout);
            var error = r.Ok ? (passed ? "" : MismatchHint(t, r.Stdout)) : r.Error;
            results.Add(Make(i, t, passed, r.Stdout, error, r.Elapsed));
            // The runners' timeout text is translated, so recognise it by the message itself, not by a fixed prefix.
            if (!r.Ok && Runners.RunnerUtil.IsTimeout(r.Error, limit))
                skipReason = L.T("Não executado: o teste anterior estourou o tempo.");
        }
        return results;
    }

    private static TestResult Make(int i, IoTest t, bool passed, string actual, string error, TimeSpan elapsed) =>
        t.Hidden
            ? new TestResult(i, passed, true, "", "", "", error, elapsed)
            : new TestResult(i, passed, false, t.Input ?? "", Normalize(t.Expected), Normalize(actual), error, elapsed);

    private static string MismatchHint(IoTest t, string actual)
    {
        if (Normalize(actual).Length == 0) return L.T("O programa não escreveu nada.");
        if (t.Hidden) return L.T("A saída não é a esperada para este teste oculto.");
        var exp = Normalize(t.Expected).Split('\n');
        var act = Normalize(actual).Split('\n');
        for (var i = 0; i < Math.Max(exp.Length, act.Length); i++)
        {
            var e = i < exp.Length ? exp[i] : null;
            var a = i < act.Length ? act[i] : null;
            if (e == a) continue;
            if (e == null) return L.F("Linha {0}: sobrou texto na saída (\"{1}\").", i + 1, a);
            if (a == null) return L.F("Linha {0}: faltou escrever \"{1}\".", i + 1, e);
            return L.F("Linha {0}: esperado \"{1}\", mas saiu \"{2}\".", i + 1, e, a);
        }
        return L.T("A saída não é a esperada.");
    }

    /// <summary>Line endings normalised, trailing spaces of each line removed, blank lines at both ends removed.</summary>
    public static string Normalize(string? s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var lines = s.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').Select(l => l.TrimEnd());
        return string.Join("\n", lines).Trim('\n', ' ', '\t');
    }

    public static bool OutputMatches(string? expected, string? actual) => Normalize(expected) == Normalize(actual);

    /// <summary>True when every test passed (and there was at least one).</summary>
    public static bool AllPassed(IReadOnlyCollection<TestResult> results) => results.Count > 0 && results.All(r => r.Passed);
}
