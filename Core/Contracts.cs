using System.Text.Json.Serialization;

namespace EasyCreator;

/// <summary>Result of running a piece of student code once.</summary>
public sealed record RunResult(bool Ok, string Stdout, string Error, TimeSpan Elapsed);

/// <summary>Runs code for one language. Offline runners live in the app; external ones call installed tools.</summary>
public interface ILanguageRunner
{
    string LanguageId { get; }          // "python", "javascript", "c", "csharp"
    string DisplayName { get; }         // "Python (built-in)", "Python (installed: 3.12)"
    bool IsExternal { get; }
    bool IsAvailable { get; }
    /// <summary>Runs <paramref name="code"/> feeding <paramref name="stdin"/>; must honour the timeout and never throw.</summary>
    Task<RunResult> RunAsync(string code, string stdin, TimeSpan timeout, CancellationToken ct = default);
}

/// <summary>A programming exercise test: stdin in, expected stdout out (compared trimmed, line endings normalised).</summary>
public sealed class IoTest
{
    [JsonPropertyName("input")] public string Input { get; set; } = "";
    [JsonPropertyName("expected")] public string Expected { get; set; } = "";
    [JsonPropertyName("hidden")] public bool Hidden { get; set; }
}

/// <summary>A robotics check, evaluated by the simulator after running the sketch for <see cref="AtMs"/> virtual ms.
/// Kind: "pin" (Target = pin number, Value = expected HIGH/LOW/0-255), "component" (Target = component id, Value = state),
/// "serial" (Value = text the Serial output must contain), "robot" (Target = "x"/"y"/"angle"/"goal", Value, Tolerance).</summary>
public sealed class SimCheck
{
    [JsonPropertyName("kind")] public string Kind { get; set; } = "pin";
    [JsonPropertyName("target")] public string Target { get; set; } = "";
    [JsonPropertyName("value")] public string Value { get; set; } = "";
    [JsonPropertyName("atMs")] public int AtMs { get; set; } = 1000;
    [JsonPropertyName("tolerance")] public double Tolerance { get; set; }
    /// <summary>Optional input stimulus applied before the check: "button:btn1=pressed@500", "pot:pot1=512@0".</summary>
    [JsonPropertyName("stimulus")] public List<string> Stimulus { get; set; } = new();
    [JsonPropertyName("description")] public string Description { get; set; } = "";
}

/// <summary>A component placed on the virtual breadboard.</summary>
public sealed class ComponentSpec
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("type")] public string Type { get; set; } = "led"; // led, rgbled, button, pot, servo, buzzer, ultrasonic, ldr, motor, lcd, robot
    [JsonPropertyName("pins")] public Dictionary<string, int> Pins { get; set; } = new();
    [JsonPropertyName("color")] public string Color { get; set; } = "";
}

public sealed class Lesson
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    /// <summary>Explanation in simple Markdown (#, **bold**, `code`, ``` blocks, - lists).</summary>
    [JsonPropertyName("theory")] public string Theory { get; set; } = "";
    [JsonPropertyName("task")] public string Task { get; set; } = "";
    [JsonPropertyName("starter")] public string Starter { get; set; } = "";
    [JsonPropertyName("solution")] public string Solution { get; set; } = "";
    [JsonPropertyName("hints")] public List<string> Hints { get; set; } = new();
    [JsonPropertyName("tests")] public List<IoTest> Tests { get; set; } = new();
    [JsonPropertyName("components")] public List<ComponentSpec> Components { get; set; } = new();
    [JsonPropertyName("checks")] public List<SimCheck> Checks { get; set; } = new();
    /// <summary>1 (beginner) … 5 (university).</summary>
    [JsonPropertyName("level")] public int Level { get; set; } = 1;
}

/// <summary>One track = one Content/*/&lt;id&gt;.json file. Lessons unlock in order.</summary>
public sealed class Track
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("title")] public string Title { get; set; } = "";
    [JsonPropertyName("area")] public string Area { get; set; } = "programming"; // programming | robotics
    [JsonPropertyName("language")] public string Language { get; set; } = "";    // runner language id; robotics = "c"
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("lessons")] public List<Lesson> Lessons { get; set; } = new();
}

public sealed record CheckOutcome(bool Passed, string Description, string Detail);

/// <summary>Runs a robotics lesson: compiles the sketch with the C interpreter, simulates, evaluates checks.</summary>
public interface IArduinoSimulator
{
    /// <summary>Headless: simulate and grade (used by the "Check" button and tests).</summary>
    Task<List<CheckOutcome>> GradeAsync(Lesson lesson, string sketch, CancellationToken ct = default);
}
