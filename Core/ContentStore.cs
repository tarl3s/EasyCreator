using System.IO;
using System.Text.Json;

namespace EasyCreator;

/// <summary>Loads every track from Content/programming and Content/robotics next to the exe.</summary>
public static class ContentStore
{
    static List<Track>? _tracks;
    public static string Root => Path.Combine(AppContext.BaseDirectory, "Content");
    public static List<string> Errors { get; } = new();

    static readonly JsonSerializerOptions Opts = new()
    {
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
    };

    public static IReadOnlyList<Track> Tracks => _tracks ??= Load();

    /// <summary>Loads again (the language changed: lesson texts come from Content/i18n/<lang>/).</summary>
    public static void Reload() { _tracks = null; Errors.Clear(); }

    public static IEnumerable<Track> InArea(string area) => Tracks.Where(t => t.Area == area);

    public static Track? Find(string id) => Tracks.FirstOrDefault(t => t.Id == id);

    static void ApplyTranslation(Track t)
    {
        if (L.Lang == "pt") return;
        var file = Path.Combine(Root, "i18n", L.Lang, t.Id + ".json");
        if (!File.Exists(file)) return;
        try { JsonSerializer.Deserialize<TrackText>(File.ReadAllText(file), Opts)?.ApplyTo(t); }
        catch (Exception ex) { Errors.Add($"i18n/{L.Lang}/{t.Id}.json: {ex.Message}"); }
    }

    static List<Track> Load()
    {
        var list = new List<Track>();
        if (!Directory.Exists(Root)) { Errors.Add(L.T("Pasta Content não encontrada.")); return list; }
        var i18n = Path.Combine(Root, "i18n");
        foreach (var file in Directory.EnumerateFiles(Root, "*.json", SearchOption.AllDirectories).Where(f => !f.StartsWith(i18n, StringComparison.OrdinalIgnoreCase)).OrderBy(f => f))
        {
            try
            {
                var t = JsonSerializer.Deserialize<Track>(File.ReadAllText(file), Opts);
                if (t == null) continue;
                if (string.IsNullOrEmpty(t.Id)) t.Id = Path.GetFileNameWithoutExtension(file);
                ApplyTranslation(t);
                list.Add(t);
            }
            catch (Exception ex) { Errors.Add($"{Path.GetFileName(file)}: {ex.Message}"); }
        }
        return list;
    }
}
