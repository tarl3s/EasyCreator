namespace EasyCreator.Exercises;

/// <summary>Base for generators made of a pool of situations per difficulty: each draw runs one random situation.
/// <c>X(pt, en, es)</c> / <c>XF(pt, en, es, args)</c> are <c>L.T</c> / <c>L.F</c> on the Portuguese text; the en/es
/// arguments are only read by a build-time script (they are copied to Lang/exercises-*.json).</summary>
public abstract class PoolGen : IExerciseGenerator
{
    protected abstract Func<Random, Exercise>[] Pool(Difficulty d);

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        var p = Pool(d);
        var ex = p[r.Next(p.Length)](r);
        var note = ex.Question?.Kind == "number" ? NoteFor(ex.Prompt) : "";
        return note.Length == 0 ? ex : new Exercise
        {
            Prompt = ex.Prompt + " " + note, Question = ex.Question, Code = ex.Code, Starter = ex.Starter,
            Solution = ex.Solution, Tests = ex.Tests, Explanation = ex.Explanation,
        };
    }

    /// <summary>A constant the answer needs but the prompt does not state, added to number prompts ("" = none).</summary>
    protected virtual string NoteFor(string prompt) => "";

    protected static string X(string pt, string en, string es) => L.T(pt);
    protected static string XF(string pt, string en, string es, params object?[] a) => L.F(pt, a);
    /// <summary>Same text in every language (pure formulas, no static decimals).</summary>
    protected static string Y(string pt) => L.T(pt);
    protected static string YF(string pt, params object?[] a) => L.F(pt, a);
    protected static string N(double v, int d = 2) => Ex.N(v, d);
    protected static string Sci(double v) { if (v == 0) return "0"; int e = (int)Math.Floor(Math.Log10(Math.Abs(v))); return Ex.N(v / Math.Pow(10, e), 2) + "·10^" + e; }
    protected static double S(Random r, double a, double b, double s) => Ex.Step(r, a, b, s);
    protected static T P<T>(Random r, params T[] items) => Ex.Pick(r, items);
    protected static Exercise Q(string p, double a, string u, string e, double tol = 2) => Ex.Num(p, a, u, e, tol);
    protected static Func<Random, Exercise>[] F(params Func<Random, Exercise>[] f) => f;
    protected static double Ln(double v) => Math.Log(v);
    protected static double Log(double v) => Math.Log10(v);
    protected static double Sqrt(double v) => Math.Sqrt(v);
    protected static double Exp(double v) => Math.Exp(v);
    protected const double Pi = Math.PI;

    /// <summary>Multiple choice from a table of (question, right, wrong...) facts.</summary>
    protected static Exercise Fact(Random r, string prompt, string right, string explanation, params string[] wrong) =>
        Ex.Choice(r, prompt, right, wrong, explanation);

    /// <summary>Picks the unknown among several: each tuple is (prompt tail, answer, unit, explanation).</summary>
    protected static Exercise Ask(Random r, string intro, params (string q, double a, string u, string e)[] opts)
    {
        var o = opts[r.Next(opts.Length)];
        return Ex.Num(intro + " " + o.q, o.a, o.u, o.e);
    }
}
