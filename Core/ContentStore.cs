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

    public static IEnumerable<Track> InArea(string area) => Tracks.Where(t => t.Area == area);

    public static Track? Find(string id) => Tracks.FirstOrDefault(t => t.Id == id);

    static List<Track> Load()
    {
        var list = new List<Track>();
        if (!Directory.Exists(Root)) { Errors.Add("Pasta Content não encontrada."); return list; }
        foreach (var file in Directory.EnumerateFiles(Root, "*.json", SearchOption.AllDirectories).OrderBy(f => f))
        {
            try
            {
                var t = JsonSerializer.Deserialize<Track>(File.ReadAllText(file), Opts);
                if (t == null) continue;
                if (string.IsNullOrEmpty(t.Id)) t.Id = Path.GetFileNameWithoutExtension(file);
                list.Add(t);
            }
            catch (Exception ex) { Errors.Add($"{Path.GetFileName(file)}: {ex.Message}"); }
        }
        return list;
    }
}
