using System.IO;
using System.Diagnostics;
using IronPython.Hosting;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace EasyCreator.Runners;

/// <summary>Offline Python on IronPython 3.4 (Python 3.4 language plus f-strings; standard library from the
/// IronPython.StdLib package). <c>print</c>/<c>input</c> are redirected; every run gets a fresh engine on its own
/// thread. The timeout is enforced by a trace hook that raises on the next line once the deadline passes.</summary>
public sealed class PythonRunner : ILanguageRunner
{
    public string LanguageId => "python";
    public string DisplayName => "Python (embutido)";
    public bool IsExternal => false;
    public bool IsAvailable => true;

    public const int RecursionLimit = 1000;

    private const string InputShim = """
import builtins, sys
def _ec_input(prompt=''):
    if prompt:
        sys.stdout.write(str(prompt))
    line = sys.stdin.readline()
    if not line:
        raise EOFError('EOF when reading a line')
    return line[:-1] if line.endswith('\n') else line
builtins.input = _ec_input
""";

    /// <summary>sys.stdout / sys.stderr replacement (IronPython calls write/flush on it).</summary>
    public sealed class PyWriter
    {
        private readonly TextWriter _w;
        public PyWriter(TextWriter w) { _w = w; }
        public int write(string s) { _w.Write(s); return s?.Length ?? 0; }
        public void flush() { }
        public bool isatty() => false;
        public string encoding => "utf-8";
    }

    /// <summary>sys.stdin replacement: input() calls readline().</summary>
    public sealed class PyReader
    {
        private readonly Queue<string> _lines;
        public PyReader(Queue<string> lines) { _lines = lines; }
        public string readline() => _lines.Count > 0 ? _lines.Dequeue() + "\n" : "";
        public string read() { var all = string.Join("\n", _lines) + (_lines.Count > 0 ? "\n" : ""); _lines.Clear(); return all; }
        public List<string> readlines() { var l = _lines.Select(x => x + "\n").ToList(); _lines.Clear(); return l; }
        public bool isatty() => false;
        public string encoding => "utf-8";
    }

    private sealed class TimeoutSignal : Exception { public TimeoutSignal() : base("Tempo esgotado") { } }

    public async Task<RunResult> RunAsync(string code, string stdin, TimeSpan timeout, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var stdout = new CappedWriter();
        var stderr = new CappedWriter(20_000);
        var lines = new Queue<string>(RunnerUtil.StdinLines(stdin));
        var state = new RunState();
        try
        {
            var task = RunnerUtil.RunOnThread(() => Execute(code ?? "", lines, stdout, stderr, timeout, state, ct), "ec-python", out var thread);
            // The engine itself is created before the deadline starts; give that a fixed allowance.
            var finished = await RunnerUtil.WaitAsync(task, timeout + TimeSpan.FromSeconds(15), ct).ConfigureAwait(false);
            if (!finished)
            {
                state.Stop = true;
                try { thread.Interrupt(); } catch { }
                await RunnerUtil.WaitAsync(task, TimeSpan.FromSeconds(2), CancellationToken.None).ConfigureAwait(false);
                return RunnerUtil.Fail(ct.IsCancellationRequested ? RunnerUtil.CanceledMessage : RunnerUtil.TimeoutMessage(timeout), stdout.ToString(), sw);
            }
            var error = await task.ConfigureAwait(false);
            var err = stderr.ToString().TrimEnd();
            if (error == null) return new RunResult(true, stdout.ToString(), err, sw.Elapsed);
            if (error == "") error = ct.IsCancellationRequested ? RunnerUtil.CanceledMessage : RunnerUtil.TimeoutMessage(timeout);
            return RunnerUtil.Fail(err.Length > 0 ? err + "\n" + error : error, stdout.ToString(), sw);
        }
        catch (Exception ex)
        {
            return RunnerUtil.Fail("Erro interno ao rodar Python: " + ex.Message, stdout.ToString(), sw);
        }
    }

    private sealed class RunState
    {
        public volatile bool Stop;
        public long Deadline = long.MaxValue;
        public bool TimedOut;
    }

    private static ScriptEngine CreateEngine()
    {
        var engine = Python.CreateEngine(new Dictionary<string, object> { ["RecursionLimit"] = RecursionLimit });
        var paths = engine.GetSearchPaths().ToList();
        var lib = Path.Combine(AppContext.BaseDirectory, "lib");
        if (Directory.Exists(lib) && !paths.Contains(lib)) paths.Add(lib);
        var site = Path.Combine(lib, "site-packages");
        if (Directory.Exists(site) && !paths.Contains(site)) paths.Add(site);
        engine.SetSearchPaths(paths);
        return engine;
    }

    /// <summary>Returns null on success, "" on timeout/cancel, or a friendly error.</summary>
    private static string? Execute(string code, Queue<string> lines, CappedWriter stdout, CappedWriter stderr,
        TimeSpan timeout, RunState state, CancellationToken ct)
    {
        var engine = CreateEngine();
        var sys = engine.GetSysModule();
        var outW = new PyWriter(stdout);
        sys.SetVariable("stdout", outW);
        sys.SetVariable("__stdout__", outW);
        var errW = new PyWriter(stderr);
        sys.SetVariable("stderr", errW);
        sys.SetVariable("__stderr__", errW);
        var inR = new PyReader(lines);
        sys.SetVariable("stdin", inR);
        sys.SetVariable("__stdin__", inR);
        sys.SetVariable("argv", new IronPython.Runtime.PythonList { "main.py" });
        // IronPython's input() returns "" at the end of the input; CPython raises EOFError. Match CPython.
        engine.Execute(InputShim, engine.CreateScope());

        var source = engine.CreateScriptSourceFromString(code.Replace("\r\n", "\n"), "main.py", SourceCodeKind.File);
        var scope = engine.CreateScope();
        scope.SetVariable("__name__", "__main__");
        scope.SetVariable("__file__", "main.py");

        CompiledCode compiled;
        try
        {
            compiled = source.Compile();
        }
        catch (SyntaxErrorException se)
        {
            return DescribeSyntax(se);
        }
        catch (Exception ex)
        {
            return "Erro ao ler o código: " + ex.Message;
        }

        TracebackDelegate? trace = null;
        trace = (frame, result, payload) =>
        {
            if (state.Stop || ct.IsCancellationRequested || Environment.TickCount64 > state.Deadline)
            {
                state.TimedOut = true;
                throw new TimeoutSignal();
            }
            return trace!;
        };

        state.Deadline = Environment.TickCount64 + (long)timeout.TotalMilliseconds;
        Python.SetTrace(engine, trace);
        try
        {
            compiled.Execute(scope);
            return null;
        }
        catch (Exception ex) when (state.TimedOut || ex is TimeoutSignal || ex is ThreadInterruptedException)
        {
            return "";
        }
        catch (SystemExitException sx)
        {
            var codeObj = sx.GetExitCode(out var other);
            if (codeObj == 0 && other == null) return null;
            return other != null ? $"O programa encerrou com sys.exit: {other}" : $"O programa encerrou com sys.exit({codeObj}).";
        }
        catch (SyntaxErrorException se)
        {
            return DescribeSyntax(se);
        }
        catch (Exception ex)
        {
            for (var e = ex; e != null; e = e.InnerException)
                if (e is OutputLimitException) return RunnerUtil.OutputLimitMessage;
            return DescribeRuntime(engine, ex);
        }
        finally
        {
            try { Python.SetTrace(engine, null); } catch { }
        }
    }

    private static string DescribeSyntax(SyntaxErrorException se)
    {
        var kind = se.GetType().Name.Contains("Indentation", StringComparison.OrdinalIgnoreCase) || se.Message.Contains("indent", StringComparison.OrdinalIgnoreCase)
            ? "Erro de indentação" : "Erro de sintaxe";
        var tip = kind == "Erro de indentação"
            ? "Em Python os espaços no começo da linha definem os blocos: use sempre a mesma quantidade (4 espaços) dentro de if, for, while e def."
            : "O código não pôde ser lido. Confira parênteses, aspas, dois-pontos (:) no fim de if/for/while/def e a indentação.";
        return $"{kind} na linha {se.Line}, coluna {se.Column}: {se.Message}\n{tip}";
    }

    private static string DescribeRuntime(ScriptEngine engine, Exception ex)
    {
        string typeName = ex.GetType().Name, message = ex.Message;
        int line = 0;
        try
        {
            var ops = engine.GetService<ExceptionOperations>();
            ops.GetExceptionMessage(ex, out message, out typeName);
            foreach (var f in ops.GetStackFrames(ex))
                if (f.GetFileName() == "main.py") line = f.GetFileLineNumber();
        }
        catch { }
        var where = line > 0 ? $" na linha {line}" : "";
        var head = $"Erro{where}: {typeName}: {message}";
        var tip = Explain(typeName);
        return tip.Length > 0 ? head + "\n" + tip : head;
    }

    internal static string Explain(string pythonError) => pythonError switch
    {
        "ZeroDivisionError" => "Divisão por zero: o divisor ficou 0. Teste o valor antes de dividir.",
        "NameError" => "Você usou um nome (variável ou função) que não existe. Confira a grafia e se ele foi criado antes de ser usado.",
        "TypeError" => "Operação com tipos incompatíveis — por exemplo, somar texto com número. Lembre que input() devolve texto: converta com int() ou float().",
        "ValueError" => "Valor inválido para a operação — por exemplo, int('abc'). Confira o que foi lido da entrada.",
        "IndexError" => "Índice fora da lista: as posições vão de 0 até len(lista) - 1.",
        "KeyError" => "Essa chave não existe no dicionário. Use 'in' para testar ou dict.get().",
        "AttributeError" => "Esse objeto não tem o atributo ou método usado. Confira a grafia e o tipo do valor.",
        "EOFError" => "O programa tentou ler mais entrada (input()) do que o teste forneceu.",
        "RecursionError" or "RuntimeError" => "Pode ser recursão sem fim: confira o caso base da função recursiva.",
        "UnboundLocalError" => "A variável foi usada dentro da função antes de receber um valor.",
        "IndentationError" => "Em Python os espaços no começo da linha definem os blocos: use a mesma quantidade dentro de cada bloco.",
        _ => "",
    };
}
