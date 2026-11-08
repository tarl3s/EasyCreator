using System.Globalization;

namespace EasyCreator.Exercises;

/// <summary>Shared helpers for the programming exercise generators (C, C#): placeholders in code templates, tests built from
/// a C# oracle, and the common prompt pieces.</summary>
static class PK
{
    public static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>Replaces @key@ placeholders: Sub(tpl, "A", 5, "B", "x").</summary>
    public static string Sub(string tpl, params object[] kv)
    {
        for (int i = 0; i + 1 < kv.Length; i += 2)
            tpl = tpl.Replace("@" + kv[i] + "@", Convert.ToString(kv[i + 1], Inv));
        return tpl;
    }

    /// <summary>Raw-string templates are written indented; trims the trailing newline.</summary>
    public static string Lines(params string[] l) => string.Join("\n", l);
    public static string Join(IEnumerable<int> v, string sep = " ") => string.Join(sep, v);
    public static int[] Ints(string input) =>
        input.Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
    public static string F2(double v) => v.ToString("F2", Inv);

    /// <summary>n tests: the first <paramref name="visible"/> are shown, the others hidden. Expected comes from the oracle.</summary>
    public static List<IoTest> Tests(Random r, int n, Func<Random, string> input, Func<string, string> oracle, int visible = 2)
    {
        var list = new List<IoTest>();
        for (int i = 0; i < n; i++)
        {
            var inp = input(r);
            list.Add(new IoTest { Input = inp, Expected = oracle(inp), Hidden = i >= visible });
        }
        return list;
    }

    /// <summary>Tests for programs with no input: the same expected output twice (the second hidden).</summary>
    public static List<IoTest> Fixed(string expected) => new()
    {
        new IoTest { Input = "", Expected = expected },
        new IoTest { Input = "", Expected = expected, Hidden = true },
    };

    static string Fence(string s) => "```\n" + s.TrimEnd('\n') + "\n```";

    /// <summary>A code exercise whose prompt ends with a worked example (the first test).</summary>
    public static Exercise Write(string statement, string starter, string solution, List<IoTest> tests, string explanation)
    {
        var t = tests[0];
        string prompt = t.Input.Length == 0
            ? statement + "\n\n" + L.T("Saída esperada:") + "\n" + Fence(t.Expected)
            : statement + "\n\n" + L.T("Exemplo de entrada:") + "\n" + Fence(t.Input) + "\n" + L.T("Saída esperada:") + "\n" + Fence(t.Expected);
        return Ex.Code(prompt, starter, solution, tests, explanation);
    }

    /// <summary>"What does it print?" — <paramref name="intro"/> must differ between draws (mention a value from the program).</summary>
    public static Exercise Predict(string intro, string code, string expected, string explanation) =>
        Ex.Output(intro + " " + L.T("Escreva exatamente o que ele imprime."), code, expected, explanation);

    public const string CHead = "#include <stdio.h>\n\n";
    public const string CStarter = "#include <stdio.h>\n\nint main(void) {\n    // escreva seu código aqui\n    return 0;\n}\n";
    /// <summary>A C program: stdio include (plus extra includes) and main wrapping the body (4-space indented lines).</summary>
    public static string CMain(string body, string includes = "") => "#include <stdio.h>\n" + includes + "\nint main(void) {\n" + body + "    return 0;\n}\n";
    /// <summary>A random list of n ints.</summary>
    public static List<int> Rand(Random r, int n, int lo, int hi) => Enumerable.Range(0, n).Select(_ => r.Next(lo, hi + 1)).ToList();

    public static T Pick<T>(Random r, params T[] a) => a[r.Next(a.Length)];
    public static List<int> Distinct(Random r, int n, int lo, int hi)
    {
        var s = new List<int>();
        while (s.Count < n) { int v = r.Next(lo, hi + 1); if (!s.Contains(v)) s.Add(v); }
        return s;
    }
    public static string Cap(string s) => s.Length == 0 ? s : char.ToUpper(s[0]) + s[1..];
    /// <summary>Escapes a text for a C or C# string literal ("), optionally doubling % for printf.</summary>
    public static string Esc(string s, bool printf = false)
    {
        s = s.Replace("\\", "\\\\").Replace("\"", "\\\"");
        return printf ? s.Replace("%", "%%") : s;
    }
}
