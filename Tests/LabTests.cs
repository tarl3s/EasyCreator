using System.IO;

namespace EasyCreator;

/// <summary>Checks every quiz/lab lesson of every track: fields filled, questions well-formed, the lab exists, its
/// solution inputs meet every goal and its default inputs miss at least one. Runs on the UI thread (labs are controls).</summary>
public static class LabTests
{
    public static int Run(TextWriter log)
    {
        int fails = 0, passes = 0;
        void Ok(string m) { passes++; log.WriteLine("  ok   " + m); }
        void Fail(string m) { fails++; log.WriteLine("  FAIL " + m); }

        log.WriteLine($"== Labs registered: {string.Join(", ", LabRegistry.Types.Keys.OrderBy(k => k))}");
        foreach (var t in ContentStore.Tracks)
        foreach (var l in t.Lessons.Where(l => l.Kind is "quiz" or "lab"))
        {
            string id = $"{t.Id}/{l.Id}";
            if (string.IsNullOrWhiteSpace(l.Title) || string.IsNullOrWhiteSpace(l.Theory)) Fail($"{id}: title/theory empty");
            if (l.Kind == "quiz" && l.Questions.Count == 0) Fail($"{id}: quiz without questions");
            foreach (var (q, i) in l.Questions.Select((q, i) => (q, i)))
            {
                bool good = q.Kind == "number"
                    ? Question.TryNum(q.Answer, out _)
                    : int.TryParse(q.Answer, out var a) && a >= 0 && a < q.Options.Count && q.Options.Count >= 2;
                if (!good) Fail($"{id}: question {i + 1} answer/options invalid");
                else if (!q.IsCorrect(q.Answer)) Fail($"{id}: question {i + 1} rejects its own answer");
                if (string.IsNullOrWhiteSpace(q.Explanation)) Fail($"{id}: question {i + 1} has no explanation");
            }
            if (l.Kind != "lab") { Ok($"{id}: quiz with {l.Questions.Count} questions"); continue; }

            try
            {
                var lab = LabRegistry.Create(l.Lab);
                if (lab == null) { Fail($"{id}: lab '{l.Lab}' not found"); continue; }
                if (l.Goals.Count == 0) { Fail($"{id}: lab without goals"); continue; }
                lab.Load(l.LabParams);
                lab.RunToEnd();
                var missed = l.Goals.Where(g => !g.IsMet(lab.Metrics)).ToList();
                if (missed.Count == 0) Fail($"{id}: default inputs already meet every goal");
                lab.SetInputs(l.SolutionInputs);
                lab.RunToEnd();
                var bad = l.Goals.Where(g => !g.IsMet(lab.Metrics)).ToList();
                if (bad.Count > 0)
                    Fail($"{id}: solution misses " + string.Join("; ", bad.Select(g =>
                        $"{g.Metric} {g.Op} {g.Value} (got {(lab.Metrics.TryGetValue(g.Metric, out var v) ? v.ToString("0.###") : "none")})")));
                else Ok($"{id}: lab {l.Lab}, solution meets {l.Goals.Count} goals");
                lab.Stop();
            }
            catch (Exception ex) { Fail($"{id}: crash {ex.GetType().Name}: {ex.Message}"); }
        }
        log.WriteLine($"== {passes} passed, {fails} failed");
        return fails;
    }
}
