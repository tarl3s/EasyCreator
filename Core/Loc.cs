using System.Globalization;
using System.IO;
using System.Text.Json;

namespace EasyCreator;

/// <summary>Languages: Portuguese (the source text), English and Spanish.
/// UI: every visible text goes through <c>L.T("texto em português")</c>; translations live in Lang/*.en.json and
/// Lang/*.es.json (flat {"português": "translation"}; one file per area so parallel work doesn't collide).
/// Lessons: Content/i18n/&lt;lang&gt;/&lt;trackId&gt;.json overlays only the texts (see <see cref="TrackText"/>).</summary>
public static class L
{
    public static readonly (string Code, string Name)[] Languages = { ("pt", "Português"), ("en", "English"), ("es", "Español") };

    static Dictionary<string, string> _dict = new();
    public static string Lang { get; private set; } = "pt";

    /// <summary>Raised after the language changes (pages rebuild themselves).</summary>
    public static event Action? Changed;

    public static void Init()
    {
        var saved = Progress.Current.Language;
        if (string.IsNullOrEmpty(saved))
        {
            var sys = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            saved = sys is "pt" or "es" ? sys : "en";
        }
        Set(saved, save: false);
    }

    public static void Set(string lang, bool save = true)
    {
        Lang = Languages.Any(l => l.Code == lang) ? lang : "pt";
        _dict = Load(Lang);
        if (save) { Progress.Current.Language = Lang; Progress.Current.Save(); }
        ContentStore.Reload();
        Changed?.Invoke();
    }

    static Dictionary<string, string> Load(string lang)
    {
        var d = new Dictionary<string, string>();
        if (lang == "pt") return d;
        var dir = Path.Combine(AppContext.BaseDirectory, "Lang");
        if (!Directory.Exists(dir)) return d;
        foreach (var f in Directory.EnumerateFiles(dir, $"*.{lang}.json").OrderBy(f => f))
        {
            try
            {
                var part = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(f),
                    new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
                if (part != null) foreach (var kv in part) d[kv.Key] = kv.Value;
            }
            catch { /* a broken language file falls back to Portuguese */ }
        }
        return d;
    }

    /// <summary>Texts asked for in a non-Portuguese language that have no translation (written by --snap).</summary>
    public static readonly HashSet<string> Missing = new();

    public static string T(string pt)
    {
        if (Lang == "pt" || string.IsNullOrWhiteSpace(pt)) return pt;
        if (_dict.TryGetValue(pt, out var s) && s.Length > 0) return s;
        lock (Missing) Missing.Add(pt);
        return pt;
    }

    public static string F(string ptFormat, params object?[] args)
    {
        try { return string.Format(T(ptFormat), args); }
        catch (FormatException) { return string.Format(ptFormat, args); }
    }

    /// <summary>For tests: the loaded dictionary of a language.</summary>
    public static IReadOnlyDictionary<string, string> Dictionary(string lang) => Load(lang);
}

/// <summary>Translated texts of one track (Content/i18n/&lt;lang&gt;/&lt;trackId&gt;.json). Lessons are matched by id;
/// lists (hints, questions, options, goals) by position. Anything missing keeps the Portuguese text.</summary>
public sealed class TrackText
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public Dictionary<string, LessonText_> Lessons { get; set; } = new();

    public sealed class LessonText_
    {
        public string? Title { get; set; }
        public string? Theory { get; set; }
        public string? Task { get; set; }
        public string? Safety { get; set; }
        public List<string>? Hints { get; set; }
        public List<QuestionText>? Questions { get; set; }
        /// <summary>Goal descriptions, in order.</summary>
        public List<string>? Goals { get; set; }
        /// <summary>Check descriptions of robotics lessons, in order.</summary>
        public List<string>? Checks { get; set; }
    }

    public sealed class QuestionText
    {
        public string? Prompt { get; set; }
        public List<string>? Options { get; set; }
        public string? Explanation { get; set; }
        public string? Unit { get; set; }
    }

    public void ApplyTo(Track t)
    {
        t.Title = Pick(Title, t.Title);
        t.Description = Pick(Description, t.Description);
        foreach (var l in t.Lessons)
        {
            if (!Lessons.TryGetValue(l.Id, out var x)) continue;
            l.Title = Pick(x.Title, l.Title);
            l.Theory = Pick(x.Theory, l.Theory);
            l.Task = Pick(x.Task, l.Task);
            l.Safety = Pick(x.Safety, l.Safety);
            if (x.Hints != null) for (int i = 0; i < Math.Min(x.Hints.Count, l.Hints.Count); i++) l.Hints[i] = Pick(x.Hints[i], l.Hints[i]);
            if (x.Goals != null) for (int i = 0; i < Math.Min(x.Goals.Count, l.Goals.Count); i++) l.Goals[i].Description = Pick(x.Goals[i], l.Goals[i].Description);
            if (x.Checks != null) for (int i = 0; i < Math.Min(x.Checks.Count, l.Checks.Count); i++) l.Checks[i].Description = Pick(x.Checks[i], l.Checks[i].Description);
            if (x.Questions != null)
                for (int i = 0; i < Math.Min(x.Questions.Count, l.Questions.Count); i++)
                {
                    var (q, tq) = (l.Questions[i], x.Questions[i]);
                    q.Prompt = Pick(tq.Prompt, q.Prompt);
                    q.Explanation = Pick(tq.Explanation, q.Explanation);
                    q.Unit = Pick(tq.Unit, q.Unit);
                    if (tq.Options != null) for (int k = 0; k < Math.Min(tq.Options.Count, q.Options.Count); k++) q.Options[k] = Pick(tq.Options[k], q.Options[k]);
                }
        }
    }

    static string Pick(string? translated, string original) => string.IsNullOrEmpty(translated) ? original : translated;
}
