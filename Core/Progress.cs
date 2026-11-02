using System.IO;
using System.Text.Json;

namespace EasyCreator;

/// <summary>What the student finished and the code they last wrote per lesson; %APPDATA%\EasyCreator\progress.json.</summary>
public sealed class Progress
{
    public HashSet<string> Done { get; set; } = new();
    public Dictionary<string, string> Code { get; set; } = new();
    public Dictionary<string, string> Runner { get; set; } = new(); // language id -> preferred runner display name
    public string Language { get; set; } = "";

    static Progress? _current;
    public static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasyCreator", "progress.json");

    public static Progress Current => _current ??= Load();

    static Progress Load()
    {
        try { if (File.Exists(FilePath)) return JsonSerializer.Deserialize<Progress>(File.ReadAllText(FilePath)) ?? new(); }
        catch { /* a broken file starts fresh rather than crashing the app */ }
        return new();
    }

    /// <summary>Set by --snap so the visual review never writes into the real progress file.</summary>
    public static bool ReadOnly { get; set; }

    public void Save()
    {
        if (ReadOnly) return;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            var tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(this));
            File.Move(tmp, FilePath, true);
        }
        catch { }
    }

    public static string Key(Track t, Lesson l) => t.Id + "/" + l.Id;
    public bool IsDone(Track t, Lesson l) => Done.Contains(Key(t, l));

    /// <summary>Lessons unlock in order: the first one, and any after a finished lesson.</summary>
    public bool IsUnlocked(Track t, int index) => index == 0 || IsDone(t, t.Lessons[index - 1]) || IsDone(t, t.Lessons[index]);

    public int DoneCount(Track t) => t.Lessons.Count(l => IsDone(t, l));
}
