using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EasyCreator;

/// <summary>Every track has an English and a Spanish text overlay covering every lesson (same number of hints,
/// questions, options, goals and checks), and every UI translation keeps the {0} placeholders of its source.</summary>
public static class I18nTests
{
    public static int Run(TextWriter log)
    {
        int fails = 0, passes = 0;
        void Fail(string m) { fails++; log.WriteLine("  FAIL " + m); }
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };

        foreach (var lang in new[] { "en", "es" })
        {
            foreach (var t in ContentStore.Tracks)
            {
                var file = Path.Combine(ContentStore.Root, "i18n", lang, t.Id + ".json");
                if (!File.Exists(file)) { Fail($"{lang}/{t.Id}.json missing"); continue; }
                TrackText? x;
                try { x = JsonSerializer.Deserialize<TrackText>(File.ReadAllText(file), opts); }
                catch (Exception ex) { Fail($"{lang}/{t.Id}.json: {ex.Message}"); continue; }
                if (x == null || string.IsNullOrWhiteSpace(x.Title) || string.IsNullOrWhiteSpace(x.Description)) Fail($"{lang}/{t.Id}: title/description");
                int before = fails;
                foreach (var l in t.Lessons)
                {
                    string id = $"{lang}/{t.Id}/{l.Id}";
                    if (x == null || !x.Lessons.TryGetValue(l.Id, out var tl)) { Fail($"{id}: lesson missing"); continue; }
                    if (string.IsNullOrWhiteSpace(tl.Title) || string.IsNullOrWhiteSpace(tl.Theory)) Fail($"{id}: title/theory");
                    if (l.Task.Length > 0 && string.IsNullOrWhiteSpace(tl.Task)) Fail($"{id}: task");
                    if (l.Safety.Length > 0 && string.IsNullOrWhiteSpace(tl.Safety)) Fail($"{id}: safety");
                    if ((tl.Hints?.Count ?? 0) != l.Hints.Count) Fail($"{id}: hints {tl.Hints?.Count ?? 0}/{l.Hints.Count}");
                    if ((tl.Goals?.Count ?? 0) != l.Goals.Count) Fail($"{id}: goals {tl.Goals?.Count ?? 0}/{l.Goals.Count}");
                    if ((tl.Checks?.Count ?? 0) != l.Checks.Count) Fail($"{id}: checks {tl.Checks?.Count ?? 0}/{l.Checks.Count}");
                    if ((tl.Questions?.Count ?? 0) != l.Questions.Count) { Fail($"{id}: questions {tl.Questions?.Count ?? 0}/{l.Questions.Count}"); continue; }
                    for (int i = 0; i < l.Questions.Count; i++)
                    {
                        var (q, tq) = (l.Questions[i], tl.Questions![i]);
                        if (string.IsNullOrWhiteSpace(tq.Prompt) || string.IsNullOrWhiteSpace(tq.Explanation)) Fail($"{id}: question {i + 1} prompt/explanation");
                        if ((tq.Options?.Count ?? 0) != q.Options.Count) Fail($"{id}: question {i + 1} options");
                    }
                }
                if (fails == before) { passes++; log.WriteLine($"  ok   {lang}/{t.Id}: {t.Lessons.Count} lessons"); }
            }

            var dict = L.Dictionary(lang);
            int bad = 0;
            foreach (var (pt, tr) in dict)
            {
                var a = Regex.Matches(pt, @"\{\d+[^}]*\}").Select(m => m.Value).OrderBy(s => s);
                var b = Regex.Matches(tr, @"\{\d+[^}]*\}").Select(m => m.Value).OrderBy(s => s);
                if (!a.SequenceEqual(b)) { bad++; Fail($"{lang} UI text keeps placeholders: \"{pt}\" -> \"{tr}\""); }
            }
            if (bad == 0) { passes++; log.WriteLine($"  ok   {lang}: {dict.Count} UI texts, placeholders intact"); }
        }
        log.WriteLine($"== {passes} passed, {fails} failed");
        return fails;
    }
}
