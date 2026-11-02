using System.IO;
using System.Diagnostics;
using System.Text.Json;
using EasyCreator.Runners;

namespace EasyCreator;

/// <summary>Headless checks for the runners, the grader and the programming tracks.</summary>
public static class RunnerTests
{
    private sealed record Probe(string Name, string Code, string Stdin, Func<RunResult, bool> Check, double TimeoutS = 5);

    private static readonly Dictionary<string, Probe[]> Probes = new()
    {
        ["python"] = new Probe[]
        {
            new("hello", "print('Olá, mundo!')", "", r => r.Ok && r.Stdout.Trim() == "Olá, mundo!"),
            new("stdin", "a = int(input())\nb = int(input())\nprint(a + b)\nprint(f'{a / b:.2f}')", "7\n2\n", r => r.Ok && Grader.Normalize(r.Stdout) == "9\n3.50"),
            new("runtime error", "x = 1\ny = 0\nprint(x / y)", "", r => !r.Ok && r.Error.Contains("linha 3") && r.Error.Contains("ZeroDivisionError")),
            new("syntax error", "print('a')\nif True\n    print('b')", "", r => !r.Ok && r.Error.Contains("linha 2")),
            new("infinite loop", "i = 0\nwhile True:\n    i += 1", "", r => !r.Ok && r.Error.StartsWith("Tempo esgotado"), 2),
            new("print loop", "while True:\n    print('x' * 100)", "", r => !r.Ok && r.Stdout.Length <= RunnerUtil.MaxOutputChars, 3),
            new("deep recursion", "def f(n):\n    return f(n + 1)\nf(0)", "", r => !r.Ok && r.Error.Contains("recurs", StringComparison.OrdinalIgnoreCase)),
            new("recursion 900", "def f(n):\n    return 0 if n == 0 else 1 + f(n - 1)\nprint(f(900))", "", r => r.Ok && r.Stdout.Trim() == "900"),
            new("stdlib", "import math, random, collections, json\nprint(math.sqrt(16))\nprint(json.dumps({'a': 1}))", "", r => r.Ok && Grader.Normalize(r.Stdout) == "4.0\n{\"a\": 1}"),
            new("input past end", "input()\ninput()", "1\n", r => !r.Ok && r.Error.Contains("EOFError")),
            new("sys.exit", "import sys\nprint('a')\nsys.exit(0)\nprint('b')", "", r => r.Ok && r.Stdout.Trim() == "a"),
        },
        ["javascript"] = new Probe[]
        {
            new("hello", "console.log('Olá, mundo!');", "", r => r.Ok && r.Stdout.Trim() == "Olá, mundo!"),
            new("stdin", "const a = Number(prompt());\nconst b = Number(readline());\nconsole.log(a + b);\nconsole.log((a / b).toFixed(2));\nconsole.log(prompt());", "7\n2\n", r => r.Ok && Grader.Normalize(r.Stdout) == "9\n3.50\nnull"),
            new("formatting", "console.log([1, 'a', [2]], {x: 1, y: 'b'}, null, undefined, 1.5)", "", r => r.Ok && r.Stdout.Trim() == "[ 1, 'a', [ 2 ] ] { x: 1, y: 'b' } null undefined 1.5"),
            new("runtime error", "let x = 1;\nlet y = null;\nconsole.log(y.foo);", "", r => !r.Ok && r.Error.Contains("linha 3") && r.Error.Contains("TypeError")),
            new("syntax error", "console.log('a');\nlet = = 3;", "", r => !r.Ok && r.Error.Contains("linha 2")),
            new("infinite loop", "let i = 0;\nwhile (true) { i++; }", "", r => !r.Ok && r.Error.StartsWith("Tempo esgotado"), 2),
            new("deep recursion", "function f(n) { return f(n + 1); }\nf(0);", "", r => !r.Ok && r.Error.Contains("Recursão")),
            new("recursion 1500", "function f(n) { return n === 0 ? 0 : 1 + f(n - 1); }\nconsole.log(f(1500));", "", r => r.Ok && r.Stdout.Trim() == "1500"),
        },
        ["csharp"] = new Probe[]
        {
            new("hello", "Console.WriteLine(\"Olá, mundo!\");", "", r => r.Ok && r.Stdout.Trim() == "Olá, mundo!"),
            new("hello Main", "using System;\nclass Program\n{\n    static void Main()\n    {\n        Console.WriteLine(\"oi\");\n    }\n}", "", r => r.Ok && r.Stdout.Trim() == "oi"),
            new("stdin", "int a = int.Parse(Console.ReadLine());\nint b = int.Parse(Console.ReadLine());\nConsole.WriteLine(a + b);\nConsole.WriteLine((a / (double)b).ToString(\"F2\"));\nConsole.WriteLine(Console.ReadLine() == null);", "7\n2\n", r => r.Ok && Grader.Normalize(r.Stdout) == "9\n3.50\nTrue"),
            new("runtime error", "int x = 1;\nint y = 0;\nConsole.WriteLine(x / y);", "", r => !r.Ok && r.Error.Contains("linha 3") && r.Error.Contains("DivideByZero")),
            new("compile error", "int x = 1\nConsole.WriteLine(x);", "", r => !r.Ok && r.Error.Contains("Linha 1") && r.Error.Contains("CS100")),
            new("infinite loop", "int i = 0;\nwhile (true) { i++; }", "", r => !r.Ok && r.Error.StartsWith("Tempo esgotado"), 2),
            new("infinite for no braces", "long s = 0;\nfor (;;) s++;", "", r => !r.Ok && r.Error.StartsWith("Tempo esgotado"), 2),
            new("deep recursion", "static int F(int n) => F(n + 1) + 1;\nConsole.WriteLine(F(0));", "", r => !r.Ok && r.Error.Contains("Recursão")),
            new("expression body line", "static int Div(int a, int b) => a / b;\nConsole.WriteLine(\"a\");\nConsole.WriteLine(Div(1, 0));", "", r => !r.Ok && r.Error.Contains("linha 1") && r.Stdout.Trim() == "a"),
            new("sleep timeout", "System.Threading.Thread.Sleep(60000);", "", r => !r.Ok && r.Error.StartsWith("Tempo esgotado"), 2),
            new("environment exit refused", "Environment.Exit(0);", "", r => !r.Ok && r.Error.Contains("Environment.Exit")),
            new("async main", "await Task.Delay(10);\nConsole.WriteLine(\"ok\");", "", r => r.Ok && r.Stdout.Trim() == "ok"),
        },
        ["c"] = new Probe[]
        {
            new("hello", "#include <stdio.h>\nint main(void) {\n    printf(\"Ola, mundo!\\n\");\n    return 0;\n}", "", r => r.Ok && r.Stdout.Trim() == "Ola, mundo!"),
            new("stdin", "#include <stdio.h>\nint main(void) {\n    int a, b;\n    scanf(\"%d\", &a);\n    scanf(\"%d\", &b);\n    printf(\"%d\\n%.2f\\n\", a + b, 1.0 * a / b);\n    return 0;\n}", "7\n2\n", r => r.Ok && Grader.Normalize(r.Stdout) == "9\n3.50"),
            new("compile error", "#include <stdio.h>\nint main(void) {\n    int x = 1\n    return 0;\n}", "", r => !r.Ok),
            new("infinite loop", "int main(void) {\n    volatile int i = 0;\n    while (1) { i++; }\n    return 0;\n}", "", r => !r.Ok && r.Error.StartsWith("Tempo esgotado"), 2),
        },
    };

    public static async Task<int> RunAsync(TextWriter log)
    {
        var failures = 0;
        var passes = 0;
        void Ok(string what) { passes++; log.WriteLine("  ok   " + what); }
        void Fail(string what) { failures++; log.WriteLine("  FAIL " + what); }

        log.WriteLine("== Grader");
        if (Grader.OutputMatches("a\nb", "a  \r\nb\r\n\r\n")) Ok("normalize"); else Fail("normalize");
        if (!Grader.OutputMatches("a\nb", "a\nc")) Ok("mismatch"); else Fail("mismatch");

        log.WriteLine("== Built-in runners");
        var builtIn = new ILanguageRunner[] { new PythonRunner(), new JsRunner(), new CSharpRunner() };
        foreach (var runner in builtIn)
            failures += await RunProbes(runner, log, () => passes++).ConfigureAwait(false);

        log.WriteLine("== Hidden test grading");
        {
            var lesson = new Lesson
            {
                Tests =
                {
                    new IoTest { Input = "1\n", Expected = "2" },
                    new IoTest { Input = "5\n", Expected = "6", Hidden = true },
                    new IoTest { Input = "9\n", Expected = "10", Hidden = true },
                },
            };
            var res = await Grader.GradeAsync(new PythonRunner(), lesson, "n = int(input())\nprint(n + 1 if n < 9 else 0)").ConfigureAwait(false);
            if (res.Count == 3 && res[0].Passed && res[1].Passed && !res[2].Passed && res[2].Expected == "" && res[2].Input == "" && res[2].Actual == "")
                Ok("hidden tests don't reveal input/expected"); else Fail("hidden tests: " + string.Join(" | ", res));
            var loop = await Grader.GradeAsync(new PythonRunner(), lesson, "while True:\n    pass", default, TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            if (loop.All(r => !r.Passed) && loop[1].Error.StartsWith("Não executado")) Ok("grading stops after a timeout"); else Fail("grading after timeout: " + string.Join(" | ", loop));
        }

        log.WriteLine("== Tracks");
        var dir = Path.Combine(AppContext.BaseDirectory, "Content", "programming");
        foreach (var lang in new[] { "python", "javascript", "csharp", "c" })
        {
            var file = Path.Combine(dir, lang + ".json");
            Track? track = null;
            try { track = JsonSerializer.Deserialize<Track>(File.ReadAllText(file)); }
            catch (Exception ex) { Fail($"{lang}.json: {ex.Message}"); continue; }
            if (track == null) { Fail($"{lang}.json is empty"); continue; }
            var shape = CheckShape(track, lang);
            if (shape.Count == 0) Ok($"{lang}.json: {track.Lessons.Count} lessons, {track.Lessons.Sum(l => l.Tests.Count)} tests, fields filled");
            else foreach (var s in shape) Fail($"{lang}.json: {s}");
            if (lang == "c") { log.WriteLine("  skip c lessons: the C interpreter is not part of this branch"); continue; }

            var runner = builtIn.First(r => r.LanguageId == lang);
            foreach (var lesson in track.Lessons)
            {
                var sw = Stopwatch.StartNew();
                var sol = await Grader.GradeAsync(runner, lesson, lesson.Solution, default, TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                if (Grader.AllPassed(sol)) Ok($"{lesson.Id} solution passes {sol.Count} tests ({sw.ElapsedMilliseconds} ms; runs {string.Join("/", sol.Select(r => (int)r.Elapsed.TotalMilliseconds))})");
                else
                    foreach (var r in sol.Where(r => !r.Passed))
                    {
                        var t = lesson.Tests[r.Index];
                        Fail($"{lesson.Id} solution test {r.Index + 1}: expected [{Grader.Normalize(t.Expected)}] got [{Grader.Normalize(r.Actual)}] {r.Error}");
                    }
                var starter = await Grader.GradeAsync(runner, lesson, lesson.Starter, default, TimeSpan.FromSeconds(5)).ConfigureAwait(false);
                if (!Grader.AllPassed(starter)) Ok($"{lesson.Id} starter fails"); else Fail($"{lesson.Id} starter passes every test");
            }
        }

        log.WriteLine("== External runners");
        var sw2 = Stopwatch.StartNew();
        var external = await ExternalRunners.DetectAsync().ConfigureAwait(false);
        log.WriteLine($"  detected in {sw2.ElapsedMilliseconds} ms: " + (external.Count == 0 ? "(none)" : string.Join(", ", external.Select(r => $"{r.DisplayName} [{((ExternalRunner)r).ExePath}]"))));
        foreach (var lang in new[] { "javascript", "python", "c", "csharp" })
            if (!external.Any(r => r.LanguageId == lang)) log.WriteLine($"  skip external {lang}: no installed tool found");
        foreach (var runner in external)
            failures += await RunProbes(runner, log, () => passes++).ConfigureAwait(false);
        // One real lesson per external language that has a track (proves the I/O conventions match).
        foreach (var runner in external.Where(r => r.LanguageId is "python" or "javascript"))
        {
            var track = JsonSerializer.Deserialize<Track>(File.ReadAllText(Path.Combine(dir, runner.LanguageId + ".json")))!;
            foreach (var lesson in track.Lessons)
            {
                var res = await Grader.GradeAsync(runner, lesson, lesson.Solution, default, TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                if (Grader.AllPassed(res)) Ok($"{runner.DisplayName}: {lesson.Id} solution passes");
                else Fail($"{runner.DisplayName}: {lesson.Id} solution: " + string.Join(" | ", res.Where(r => !r.Passed).Select(r => r.Error)));
            }
        }

        log.WriteLine("== Registry");
        RunnerRegistry.Register(new FakeRunner());
        if (RunnerRegistry.Default("python") is PythonRunner) Ok("default python is built-in"); else Fail("default python");
        if (RunnerRegistry.Default("c") is FakeRunner) Ok("registered C runner is the default"); else Fail("registered C runner");
        await RunnerRegistry.DetectExternalAsync().ConfigureAwait(false);
        if (RunnerRegistry.All.Count >= 4 && RunnerRegistry.For("python")[0] is PythonRunner) Ok($"registry lists {RunnerRegistry.All.Count} runners"); else Fail("registry list");

        log.WriteLine($"== {passes} passed, {failures} failed");
        return failures;
    }

    private sealed class FakeRunner : ILanguageRunner
    {
        public string LanguageId => "c";
        public string DisplayName => "C (teste)";
        public bool IsExternal => false;
        public bool IsAvailable => true;
        public Task<RunResult> RunAsync(string code, string stdin, TimeSpan timeout, CancellationToken ct = default) =>
            Task.FromResult(new RunResult(true, "", "", TimeSpan.Zero));
    }

    private static async Task<int> RunProbes(ILanguageRunner runner, TextWriter log, Action passed)
    {
        var failures = 0;
        if (!Probes.TryGetValue(runner.LanguageId, out var probes)) return 0;
        foreach (var p in probes)
        {
            // Probes that rely on built-in-only behaviour (Store-safe Environment.Exit, recursion limits) are skipped for tools.
            if (runner.IsExternal && p.Name is "environment exit refused" or "deep recursion" or "recursion 900" or "recursion 1500" or "print loop" or "formatting" or "sleep timeout" or "expression body line") continue;
            var timeout = TimeSpan.FromSeconds(p.TimeoutS);
            var sw = Stopwatch.StartNew();
            RunResult r;
            try { r = await runner.RunAsync(p.Code, p.Stdin, timeout).ConfigureAwait(false); }
            catch (Exception ex) { r = new RunResult(false, "", "THREW " + ex, TimeSpan.Zero); }
            var wall = sw.Elapsed;
            var ok = p.Check(r);
            // A timeout must come back promptly (the runner may need a little time to stop the program).
            if (ok && r.Error.StartsWith("Tempo esgotado") && wall > timeout + TimeSpan.FromSeconds(runner.IsExternal ? 30 : 4)) ok = false;
            var line = $"{runner.DisplayName} / {p.Name} ({wall.TotalMilliseconds:0} ms)";
            if (ok) { passed(); log.WriteLine("  ok   " + line); }
            else
            {
                failures++;
                log.WriteLine($"  FAIL {line}\n       ok={r.Ok} stdout=[{RunnerUtil.Truncate(r.Stdout, 300)}]\n       error=[{RunnerUtil.Truncate(r.Error, 600)}]");
            }
        }
        return failures;
    }

    private static List<string> CheckShape(Track t, string lang)
    {
        var p = new List<string>();
        if (t.Id != lang) p.Add($"id is '{t.Id}'");
        if (t.Language != lang) p.Add($"language is '{t.Language}'");
        if (t.Area != "programming") p.Add("area is not programming");
        if (string.IsNullOrWhiteSpace(t.Title) || string.IsNullOrWhiteSpace(t.Description)) p.Add("title/description empty");
        if (t.Lessons.Count != 10) p.Add($"{t.Lessons.Count} lessons (expected 10)");
        if (t.Lessons.Select(l => l.Id).Distinct().Count() != t.Lessons.Count) p.Add("duplicate lesson ids");
        foreach (var l in t.Lessons)
        {
            if (new[] { l.Id, l.Title, l.Theory, l.Task, l.Starter, l.Solution }.Any(string.IsNullOrWhiteSpace)) p.Add($"{l.Id}: empty field");
            if (l.Hints.Count < 2) p.Add($"{l.Id}: {l.Hints.Count} hints");
            if (l.Tests.Count < 3) p.Add($"{l.Id}: {l.Tests.Count} tests");
            if (!l.Tests.Any(x => x.Hidden)) p.Add($"{l.Id}: no hidden test");
            if (l.Tests.Any(x => string.IsNullOrWhiteSpace(x.Expected))) p.Add($"{l.Id}: empty expected output");
            if (l.Level is < 1 or > 5) p.Add($"{l.Id}: level {l.Level}");
            if (l.Theory.Length < 400) p.Add($"{l.Id}: theory too short ({l.Theory.Length} chars)");
        }
        return p;
    }
}
