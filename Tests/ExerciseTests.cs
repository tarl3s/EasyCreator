using EasyCreator.Runners;
using System.Globalization;
using System.IO;

namespace EasyCreator;

/// <summary>The exercise generators: every lesson has one; for each difficulty and many seeds it never throws, gives a
/// valid answer, varies (different texts), and in English/Spanish uses no untranslated text. Code exercises: the
/// solution passes its tests in the built-in runner; "text" exercises: the built-in runner prints the expected output.
/// <c>--test-exercises [area]</c> runs only this suite (log: easycreator_exercises.log).</summary>
public static class ExerciseTests
{
    const int Seeds = 30, RunSeeds = 3;

    public static int Run(TextWriter log, string? area = null)
    {
        int fails = 0, passes = 0;
        void Check(string name, bool ok, string detail = "")
        {
            if (ok) passes++; else { fails++; log.WriteLine($"  FAIL {name} {detail}"); }
        }
        var lang0 = L.Lang;
        try
        {
            foreach (var t in ContentStore.Tracks.Where(t => area == null || t.Area == area))
            {
                int before = fails;
                foreach (var lesson in t.Lessons)
                {
                    if (!ExerciseRegistry.ByLesson.TryGetValue(lesson.Id, out var g)) { Check($"{t.Id}/{lesson.Id}: has a generator", false); continue; }
                    foreach (var d in Enum.GetValues<Difficulty>())
                        foreach (var lang in new[] { "pt", "en", "es" })
                            CheckLesson(t, lesson, g, d, lang, Check);
                }
                log.WriteLine(fails == before ? $"  ok   {t.Id}: {t.Lessons.Count} lessons" : $"  ---  {t.Id}: {fails - before} failures");
            }
        }
        finally { L.Set(lang0, save: false); }
        log.WriteLine($"== Exercises: {passes} passed, {fails} failed");
        return fails;
    }

    static void CheckLesson(Track t, Lesson lesson, IExerciseGenerator g, Difficulty d, string lang, Action<string, bool, string> check)
    {
        L.Set(lang, save: false);
        L.Missing.Clear();
        var id = $"{lang}/{lesson.Id}/{d}";
        var prompts = new HashSet<string>();
        for (int seed = 0; seed < Seeds; seed++)
        {
            Exercise ex;
            try { ex = g.Make(lesson, d, new Random(seed)); }
            catch (Exception e) { check($"{id} seed {seed}: generates", false, e.Message); return; }
            prompts.Add(ex.Prompt);
            if (ex.Prompt.Trim().Length == 0) { check($"{id} seed {seed}: has a prompt", false, ""); continue; }
            if (ex.Prompt.Contains("NaN") || ex.Prompt.Contains("∞") || ex.Prompt.Contains("Infinity")) check($"{id} seed {seed}: prompt numbers", false, ex.Prompt);
            if (ex.IsCode)
            {
                if (lang == "pt" && seed < RunSeeds) CheckCode(t, ex, $"{id} seed {seed}", check);
                continue;
            }
            var q = ex.Question;
            if (q == null) { check($"{id} seed {seed}: has a question or tests", false, ""); continue; }
            switch (q.Kind)
            {
                case "number":
                    check($"{id} seed {seed}: answer is a finite number", Question.TryNum(q.Answer, out var a) && double.IsFinite(a), q.Answer);
                    break;
                case "choice":
                    check($"{id} seed {seed}: choice answer", int.TryParse(q.Answer, out var i) && i >= 0 && i < q.Options.Count
                        && q.Options.Count >= 2 && q.Options.Distinct().Count() == q.Options.Count, string.Join(" | ", q.Options));
                    break;
                case "text":
                    if (lang == "pt" && seed < RunSeeds) CheckOutput(t, ex, $"{id} seed {seed}", check);
                    break;
                default:
                    check($"{id} seed {seed}: question kind", false, q.Kind);
                    break;
            }
        }
        // Varied: most draws give a different text (Genius can be a smaller family, but still varies).
        check($"{id}: varied ({prompts.Count} different of {Seeds})", prompts.Count >= Seeds * 2 / 3, "");
        if (lang != "pt") check($"{id}: translated", L.Missing.Count == 0, string.Join(" | ", L.Missing.Take(3)));
    }

    static ILanguageRunner? BuiltIn(Track t) => RunnerRegistry.For(t.Language).FirstOrDefault(r => !r.IsExternal);

    static void CheckCode(Track t, Exercise ex, string id, Action<string, bool, string> check)
    {
        var runner = BuiltIn(t);
        if (runner == null) { check($"{id}: built-in runner for {t.Language}", false, ""); return; }
        var r = Grader.GradeAsync(runner, new Lesson { Tests = ex.Tests }, ex.Solution).GetAwaiter().GetResult();
        check($"{id}: solution passes its tests", Grader.AllPassed(r),
            string.Join(" / ", r.Where(x => !x.Passed).Take(1).Select(x => x.Error + " got: " + x.Actual)));
        if (ex.Starter.Trim().Length > 0)
        {
            var s = Grader.GradeAsync(runner, new Lesson { Tests = ex.Tests }, ex.Starter).GetAwaiter().GetResult();
            check($"{id}: starter fails", !Grader.AllPassed(s), "");
        }
    }

    static void CheckOutput(Track t, Exercise ex, string id, Action<string, bool, string> check)
    {
        var runner = BuiltIn(t);
        if (runner == null) { check($"{id}: built-in runner for {t.Language}", false, ""); return; }
        var r = runner.RunAsync(ex.Code, "", TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        check($"{id}: program prints the expected answer", r.Ok && Grader.Normalize(r.Stdout) == Grader.Normalize(ex.Question!.Answer),
            $"expected [{ex.Question!.Answer}] got [{r.Stdout}] {r.Error}");
    }

    /// <summary>--test-exercises [area]: only this suite.</summary>
    public static bool TryRun(string[] args, out int failures)
    {
        failures = 0;
        int at = Array.IndexOf(args, "--test-exercises");
        if (at < 0) return false;
        string? area = at + 1 < args.Length && !args[at + 1].StartsWith("--") ? args[at + 1] : null;
        var log = new StringWriter();
        try { failures = Run(log, area); }
        catch (Exception e) { log.WriteLine("CRASH " + e); failures++; }
        File.WriteAllText(Path.Combine(Path.GetTempPath(), "easycreator_exercises.log"), log.ToString());
        return true;
    }
}
