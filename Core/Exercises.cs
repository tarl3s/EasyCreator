using System.Globalization;
using System.Reflection;

namespace EasyCreator;

/// <summary>The four levels the student picks when generating an exercise. Genius = olympiad style: combines ideas and
/// needs one extra insight.</summary>
public enum Difficulty { Easy, Medium, Hard, Genius }

/// <summary>One generated exercise (no AI: a generator draws values and builds the text and answer from formulas).
/// Answered kinds use <see cref="Question"/>: "number", "choice", or "text" (predict the output of <see cref="Code"/>).
/// A code exercise has <see cref="Tests"/> and is checked like a programming lesson.</summary>
public sealed class Exercise
{
    /// <summary>Markdown, already in the current language.</summary>
    public string Prompt { get; init; } = "";
    public Question? Question { get; init; }
    /// <summary>"text" questions: the program whose output is asked (shown under the prompt).</summary>
    public string Code { get; init; } = "";
    public string Starter { get; init; } = "";
    public string Solution { get; init; } = "";
    public List<IoTest> Tests { get; init; } = new();
    /// <summary>Shown after answering, or with "show solution": the worked reasoning.</summary>
    public string Explanation { get; init; } = "";
    public bool IsCode => Tests.Count > 0;
    /// <summary>What makes two draws different exercises (the same wording with another program or other test inputs counts).</summary>
    public string Key => Prompt + "\n" + Code + "\n" + string.Join("\n", Tests.Select(t => t.Input + "\u0001" + t.Expected));
}

/// <summary>Makes exercises for the lessons named in its <see cref="ExercisesAttribute"/>. Must be deterministic for a
/// given Random (tests replay seeds) and must vary a lot between draws: several situations per lesson, each asking for a
/// different unknown (e.g. find the time, then the speed, then the distance).</summary>
public interface IExerciseGenerator
{
    Exercise Make(Lesson lesson, Difficulty difficulty, Random rng);
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class ExercisesAttribute : Attribute
{
    public ExercisesAttribute(params string[] lessonIds) => LessonIds = lessonIds;
    public string[] LessonIds { get; }
}

public static class ExerciseRegistry
{
    static Dictionary<string, IExerciseGenerator>? _byLesson;
    static readonly Dictionary<string, string> LastPrompt = new();

    public static IReadOnlyDictionary<string, IExerciseGenerator> ByLesson => _byLesson ??= Build();

    static Dictionary<string, IExerciseGenerator> Build()
    {
        var d = new Dictionary<string, IExerciseGenerator>();
        foreach (var t in typeof(ExerciseRegistry).Assembly.GetTypes())
        {
            var a = t.GetCustomAttribute<ExercisesAttribute>();
            if (a == null || t.IsAbstract || !typeof(IExerciseGenerator).IsAssignableFrom(t)) continue;
            var g = (IExerciseGenerator)Activator.CreateInstance(t)!;
            foreach (var id in a.LessonIds) d[id] = g;
        }
        return d;
    }

    public static bool Has(Lesson lesson) => ByLesson.ContainsKey(lesson.Id);

    /// <summary>A fresh exercise, never the same text as the previous one for this lesson.</summary>
    public static Exercise? Next(Lesson lesson, Difficulty d)
    {
        if (!ByLesson.TryGetValue(lesson.Id, out var g)) return null;
        var rng = new Random();
        LastPrompt.TryGetValue(lesson.Id, out var last);
        Exercise ex = g.Make(lesson, d, rng);
        for (int i = 0; i < 10 && ex.Key == last; i++) ex = g.Make(lesson, d, rng);
        LastPrompt[lesson.Id] = ex.Key;
        return ex;
    }
}

/// <summary>Helpers for generators: drawing values, formatting numbers for the current language, building exercises.</summary>
public static class Ex
{
    /// <summary>A number for display: comma decimals in pt/es, a point in en; at most <paramref name="decimals"/>
    /// decimals, trailing zeros dropped.</summary>
    public static string N(double v, int decimals = 2)
    {
        var s = Math.Round(v, decimals).ToString("0." + new string('#', Math.Max(decimals, 0)), CultureInfo.InvariantCulture);
        return L.Lang == "en" ? s : s.Replace('.', ',');
    }

    /// <summary>An integer in [min, max].</summary>
    public static int Int(Random r, int min, int max) => r.Next(min, max + 1);
    /// <summary>A value in [min, max] rounded to <paramref name="step"/> (so the prompt shows clean numbers).</summary>
    public static double Step(Random r, double min, double max, double step) =>
        Math.Round((min + r.NextDouble() * (max - min)) / step) * step;
    public static T Pick<T>(Random r, params T[] items) => items[r.Next(items.Length)];

    /// <summary>Number answer within <paramref name="tolPct"/> percent. The stored answer keeps full precision.</summary>
    public static Exercise Num(string prompt, double answer, string unit, string explanation, double tolPct = 2) => new()
    {
        Prompt = prompt, Explanation = explanation,
        Question = new Question { Kind = "number", Answer = answer.ToString("R", CultureInfo.InvariantCulture), Unit = unit, Tolerance = tolPct, Explanation = explanation },
    };

    /// <summary>Multiple choice; the options are shuffled and the right one tracked.</summary>
    public static Exercise Choice(Random r, string prompt, string right, IEnumerable<string> wrong, string explanation)
    {
        var opts = wrong.Distinct().Where(w => w != right).Prepend(right).OrderBy(_ => r.Next()).ToList();
        return new()
        {
            Prompt = prompt, Explanation = explanation,
            Question = new Question { Kind = "choice", Options = opts, Answer = opts.IndexOf(right).ToString(), Explanation = explanation },
        };
    }

    /// <summary>"What does this program print?" — <paramref name="expected"/> must be exactly what the built-in runner
    /// prints for <paramref name="code"/> (the tests run it).</summary>
    public static Exercise Output(string prompt, string code, string expected, string explanation) => new()
    {
        Prompt = prompt, Code = code, Explanation = explanation,
        Question = new Question { Kind = "text", Answer = expected, Explanation = explanation },
    };

    /// <summary>Write a program; graded by <paramref name="tests"/> (the tests check that the solution passes them).</summary>
    public static Exercise Code(string prompt, string starter, string solution, List<IoTest> tests, string explanation) => new()
    {
        Prompt = prompt, Starter = starter, Solution = solution, Tests = tests, Explanation = explanation,
    };
}
