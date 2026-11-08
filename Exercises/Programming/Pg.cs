namespace EasyCreator.Exercises;

/// <summary>Shared helpers of the Python / JavaScript exercise generators: code blocks, tests built from a C# model,
/// print / read snippets for both languages, word pools.</summary>
static class Pg
{
    public static bool Js(Lesson l) => l.Id.StartsWith("js", StringComparison.Ordinal);

    public static readonly string[] Names = { "Ana", "Bruno", "Carla", "Davi", "Elisa", "Felipe", "Gabi", "Hugo", "Iara", "Joao", "Lia", "Marcos", "Nina", "Otavio" };
    public static readonly string[] Words = { "banana", "casa", "pedra", "livro", "janela", "tigre", "mesa", "sol", "luz", "rato", "queijo", "violao", "tomate", "carro", "bola", "gato", "nuvem", "pente", "lua", "mapa", "ponte", "radio" };

    public static string Code(params string[] lines) => string.Join("\n", lines);

    public static string Fence(bool js, string code) => "```" + (js ? "javascript" : "python") + "\n" + code.TrimEnd('\n') + "\n```";
    public static string Plain(string text) => "```\n" + text.TrimEnd('\n') + "\n```";

    /// <summary>Tests from raw stdin strings; the expected output comes from the C# model. The first two are visible.</summary>
    public static List<IoTest> Tests(IEnumerable<string> inputs, Func<string, string> expected) =>
        inputs.Reverse().Select((s, i) =>
        {
            var inp = s.Length == 0 || s.EndsWith('\n') ? s : s + "\n";
            return new IoTest { Input = inp, Expected = expected(inp), Hidden = i >= 2 };
        }).ToList();

    /// <summary>The first test shown as an example in the prompt.</summary>
    public static string Example(IoTest t)
    {
        var s = "";
        if (t.Input.Length > 0) s += L.T("Exemplo de entrada:") + "\n" + Plain(t.Input) + "\n";
        return s + L.T("Saída esperada:") + "\n" + Plain(t.Expected);
    }

    public static string Prompt(string text, IoTest ex) => text + "\n\n" + Example(ex);

    /// <summary>A print of a text; "{expr}" pieces are interpolated (f-string / template literal).</summary>
    public static string Pr(bool js, string text)
    {
        if (!text.Contains('{')) return js ? $"console.log(\"{text}\");" : $"print(\"{text}\")";
        if (js) return "console.log(`" + text.Replace("{", "${") + "`);";
        // Python f-strings cannot nest the same quote nor hold a slice colon in older versions: build the text with str() instead.
        var parts = new List<string>();
        int i = 0; bool fallback = false;
        while (i < text.Length)
        {
            int a = text.IndexOf('{', i);
            if (a < 0) { parts.Add("\"" + text[i..] + "\""); break; }
            if (a > i) parts.Add("\"" + text[i..a] + "\"");
            int b = text.IndexOf('}', a);
            string expr = text[(a + 1)..b];
            if (expr.Contains('"') || expr.Contains('\'') || System.Text.RegularExpressions.Regex.IsMatch(expr, @"\[[^\]]*:[^\]]*\]")) fallback = true;
            parts.Add("str(" + expr + ")");
            i = b + 1;
        }
        return fallback ? "print(" + string.Join(" + ", parts) + ")" : $"print(f\"{text}\")";
    }

    public static string ReadInt(bool js, string name) => js ? $"let {name} = Number(prompt());" : $"{name} = int(input())";
    public static string ReadStr(bool js, string name) => js ? $"let {name} = prompt();" : $"{name} = input()";
    public static string ReadInts(bool js, string name) => js ? $"let {name} = prompt().split(\" \").map(Number);" : $"{name} = list(map(int, input().split()))";
    public static string Comment(bool js, string text) => (js ? "// " : "# ") + text;
    public static string Len(bool js, string v) => js ? v + ".length" : $"len({v})";
    public static string Repeat(bool js, string s, string n) => js ? $"{s}.repeat({n})" : $"{s} * {n}";
    public static string IDiv(bool js, string a, string b) => js ? $"Math.trunc({a} / {b})" : $"{a} // {b}";
    public static string Starter(bool js, params string[] reads) => string.Join("\n", reads) + (reads.Length > 0 ? "\n" : "") + Comment(js, L.T("escreva o resto do programa aqui"));

    public static string Cap(string s) => s.Length == 0 ? s : char.ToUpper(s[0]) + s[1..];

    /// <summary>Space-separated ints.</summary>
    public static string Sp(IEnumerable<int> v) => string.Join(" ", v);
    public static int[] Arr(Random r, int n, int lo, int hi) => Enumerable.Range(0, n).Select(_ => r.Next(lo, hi + 1)).ToArray();
    public static int FloorDiv(int a, int b) => (int)Math.Floor((double)a / b);
    public static int FloorMod(int a, int b) => a - b * FloorDiv(a, b);
}

static class PgOut
{
    /// <summary>Prompt of a "what does it print" exercise (the program itself is shown by the page from Exercise.Code).
    /// Tells the size of the output, which also helps the student check the answer.</summary>
    public static string Prompt(string expected, string extra = "")
    {
        var ls = expected.Split('\n');
        return L.T("O que este programa mostra? Escreva a saída exata.") + " "
            + L.F("(A saída tem {0} linha(s) e {1} caracteres, sem contar as quebras de linha.)", ls.Length, ls.Sum(x => x.Length))
            + (extra.Length > 0 ? "\n\n" + extra : "");
    }
}
