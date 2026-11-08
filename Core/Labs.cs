using System.Reflection;
using System.Text.Json.Serialization;
using System.Windows;

namespace EasyCreator;

/// <summary>The learning areas shown on the home page, in order. A track's <see cref="Track.Area"/> is one of these ids.</summary>
public static class Areas
{
    public sealed record Info(string Id, string Icon, string Title, string Description);

    public static readonly Info[] All =
    {
        new("programming", "💻", "Programação", "Escolha uma linguagem e aprenda do primeiro print até algoritmos de faculdade."),
        new("robotics", "🤖", "Robótica", "Monte circuitos num Arduino simulado e evolua até robôs autônomos."),
        new("physics", "⚛️", "Física", "Movimento, forças, energia, ondas e eletricidade, com laboratórios para experimentar."),
        new("electrical", "⚡", "Engenharia Elétrica", "De Ohm a circuitos AC, potência e eletrônica, num simulador de circuitos."),
        new("metallurgy", "🔥", "Metalurgia e Solda", "Metais, tratamentos térmicos e processos de solda, com um simulador de soldagem."),
        new("projects", "🚀", "Projetos", "Foguetes, submarinos, drones e outros projetos de engenharia, do conceito ao cálculo."),
    };

    public static Info Get(string id) => All.FirstOrDefault(a => a.Id == id) ?? new(id, "📘", id, "");
}

/// <summary>A question in a quiz or lab lesson. Kind "choice": <see cref="Answer"/> is the 0-based index of the right
/// option. Kind "number": <see cref="Answer"/> is the value, accepted within <see cref="Tolerance"/> percent (default 2%).</summary>
public sealed class Question
{
    [JsonPropertyName("prompt")] public string Prompt { get; set; } = "";
    [JsonPropertyName("kind")] public string Kind { get; set; } = "choice";
    [JsonPropertyName("options")] public List<string> Options { get; set; } = new();
    [JsonPropertyName("answer")] public string Answer { get; set; } = "";
    [JsonPropertyName("tolerance")] public double Tolerance { get; set; } = 2;
    [JsonPropertyName("unit")] public string Unit { get; set; } = "";
    /// <summary>Shown after answering (right or wrong): why this is the answer.</summary>
    [JsonPropertyName("explanation")] public string Explanation { get; set; } = "";

    /// <summary>Accepts "3,5" or "3.5"; never throws.</summary>
    public bool IsCorrect(string given)
    {
        given = (given ?? "").Trim();
        if (Kind == "choice") return given == Answer.Trim();
        if (Kind == "text") return Grader.Normalize(given) == Grader.Normalize(Answer);
        if (!TryNum(given, out var g) || !TryNum(Answer, out var a)) return false;
        if (a == 0) return Math.Abs(g) < 1e-9 + Tolerance / 100;
        return Math.Abs(g - a) <= Math.Abs(a) * Math.Max(Tolerance, 0.01) / 100;
    }

    public static bool TryNum(string s, out double v) =>
        double.TryParse((s ?? "").Trim().Replace(',', '.'), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out v);
}

/// <summary>A lab goal checked against a metric the simulator reports after a run.
/// Op: "&lt;", "&lt;=", "&gt;", "&gt;=", "=" (within <see cref="Tolerance"/> percent), "between" (Value..Max).</summary>
public sealed class SimGoal
{
    [JsonPropertyName("metric")] public string Metric { get; set; } = "";
    [JsonPropertyName("op")] public string Op { get; set; } = "=";
    [JsonPropertyName("value")] public double Value { get; set; }
    [JsonPropertyName("max")] public double Max { get; set; }
    [JsonPropertyName("tolerance")] public double Tolerance { get; set; } = 5;
    [JsonPropertyName("description")] public string Description { get; set; } = "";

    public bool IsMet(IReadOnlyDictionary<string, double> metrics)
    {
        if (!metrics.TryGetValue(Metric, out var v) || double.IsNaN(v)) return false;
        return Op switch
        {
            "<" => v < Value, "<=" => v <= Value, ">" => v > Value, ">=" => v >= Value,
            "between" => v >= Value && v <= Max,
            _ => Value == 0 ? Math.Abs(v) <= Tolerance / 100 : Math.Abs(v - Value) <= Math.Abs(Value) * Tolerance / 100,
        };
    }
}

/// <summary>An interactive simulator embedded in a lab lesson. Implementations are WPF controls found by
/// <see cref="LabAttribute"/>; they must also work headless (tests call SetInputs + RunToEnd without showing them).</summary>
public interface ILab
{
    /// <summary>The control shown in the lesson (sliders, drawing, buttons).</summary>
    FrameworkElement View { get; }
    /// <summary>Lesson setup (target distance, material, circuit…), from <see cref="Lesson.LabParams"/>. Called once.</summary>
    void Load(IReadOnlyDictionary<string, string> labParams);
    /// <summary>Sets the student's controls (names are the lab's own; tests use <see cref="Lesson.SolutionInputs"/>).</summary>
    void SetInputs(IReadOnlyDictionary<string, string> inputs);
    /// <summary>Headless: runs the whole experiment instantly so <see cref="Metrics"/> holds the final values.</summary>
    void RunToEnd();
    /// <summary>Values after the last run (e.g. "range" m, "maxHeight" m, "porosity" 0..1). Goals read these.</summary>
    IReadOnlyDictionary<string, double> Metrics { get; }
    /// <summary>Raised when a run finishes in the UI, so the lesson can re-check the goals.</summary>
    event Action? Finished;
    /// <summary>Stops animations/timers (the lesson page closed).</summary>
    void Stop();
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class LabAttribute(string id) : Attribute
{
    public string Id { get; } = id;
}

public static class LabRegistry
{
    static Dictionary<string, Type>? _types;

    public static IReadOnlyDictionary<string, Type> Types => _types ??= typeof(LabRegistry).Assembly.GetTypes()
        .Where(t => typeof(ILab).IsAssignableFrom(t) && !t.IsAbstract && t.GetCustomAttribute<LabAttribute>() != null)
        .ToDictionary(t => t.GetCustomAttribute<LabAttribute>()!.Id, t => t);

    public static ILab? Create(string id) =>
        Types.TryGetValue(id ?? "", out var t) ? (ILab?)Activator.CreateInstance(t) : null;
}
