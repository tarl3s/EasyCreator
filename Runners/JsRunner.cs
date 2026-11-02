using System.Diagnostics;
using System.Globalization;
using Jint;
using Jint.Native;
using Jint.Runtime;

namespace EasyCreator.Runners;

/// <summary>Offline JavaScript on Jint. <c>console.log</c> prints (Node-like formatting), <c>prompt()</c> /
/// <c>readline()</c> return the next stdin line (or null); the prompt message is NOT echoed so tests stay exact.
/// Limits: the timeout, statements, memory and recursion depth.</summary>
public sealed class JsRunner : ILanguageRunner
{
    public string LanguageId => "javascript";
    public string DisplayName => "JavaScript (embutido)";
    public bool IsExternal => false;
    public bool IsAvailable => true;

    /// <summary>Jint counts every byte the run allocates (not what stays alive), so this is generous; the timeout
    /// stops ordinary runaway loops first.</summary>
    public const long MemoryLimitBytes = 2L * 1024 * 1024 * 1024;
    public const int RecursionLimit = 2000;
    public const int StatementLimit = 200_000_000;

    // Runs before the student's script (its own source, so line numbers in errors stay right).
    private const string Prelude = """
(function () {
  const write = __ec_write, writeErr = __ec_write_err, nextLine = __ec_read;
  const isId = k => /^[A-Za-z_$][\w$]*$/.test(k);
  function fmt(v, depth, nested, seen) {
    const t = typeof v;
    if (t === 'string') return nested ? "'" + v.replace(/\\/g, '\\\\').replace(/'/g, "\\'").replace(/\n/g, '\\n') + "'" : v;
    if (t === 'number') return Object.is(v, -0) ? '-0' : String(v);
    if (t === 'bigint') return String(v) + 'n';
    if (t === 'undefined' || v === null || t === 'boolean') return String(v);
    if (t === 'symbol') return v.toString();
    if (t === 'function') {
      const src = Function.prototype.toString.call(v);
      if (/^class\b/.test(src)) return '[class ' + (v.name || '(anonymous)') + ']';
      return v.name ? '[Function: ' + v.name + ']' : '[Function (anonymous)]';
    }
    if (seen.includes(v)) return '[Circular *1]';
    if (v instanceof Error) return v.stack && !nested ? String(v.stack) : (v.name + ': ' + v.message);
    if (v instanceof Date) return isNaN(v) ? 'Invalid Date' : v.toISOString();
    if (v instanceof RegExp) return String(v);
    const inner = seen.concat([v]);
    if (Array.isArray(v)) {
      if (v.length === 0) return '[]';
      if (depth > 2) return '[Array]';
      const parts = [];
      for (let i = 0; i < v.length; i++) parts.push(i in v ? fmt(v[i], depth + 1, true, inner) : '<empty>');
      return '[ ' + parts.join(', ') + ' ]';
    }
    if (v instanceof Map) {
      if (depth > 2) return '[Map]';
      const parts = [...v].map(([k, x]) => fmt(k, depth + 1, true, inner) + ' => ' + fmt(x, depth + 1, true, inner));
      return 'Map(' + v.size + ') ' + (parts.length ? '{ ' + parts.join(', ') + ' }' : '{}');
    }
    if (v instanceof Set) {
      if (depth > 2) return '[Set]';
      const parts = [...v].map(x => fmt(x, depth + 1, true, inner));
      return 'Set(' + v.size + ') ' + (parts.length ? '{ ' + parts.join(', ') + ' }' : '{}');
    }
    const ctor = v.constructor && v.constructor !== Object && typeof v.constructor.name === 'string' && v.constructor.name ? v.constructor.name + ' ' : '';
    const keys = Object.keys(v);
    if (keys.length === 0) return ctor + '{}';
    if (depth > 2) return '[' + (ctor.trim() || 'Object') + ']';
    const parts = keys.map(k => (isId(k) ? k : "'" + k + "'") + ': ' + fmt(v[k], depth + 1, true, inner));
    return ctor + '{ ' + parts.join(', ') + ' }';
  }
  const line = args => args.map(a => fmt(a, 0, false, [])).join(' ') + '\n';
  const out = (...a) => write(line(a));
  const err = (...a) => writeErr(line(a));
  globalThis.console = { log: out, info: out, debug: out, warn: err, error: err };
  globalThis.prompt = () => nextLine();
  globalThis.readline = globalThis.prompt;
})();
""";

    public async Task<RunResult> RunAsync(string code, string stdin, TimeSpan timeout, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var stdout = new CappedWriter();
        var stderr = new CappedWriter(20_000);
        var lines = new Queue<string>(RunnerUtil.StdinLines(stdin));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        try
        {
            var task = RunnerUtil.RunOnThread(() => Execute(code ?? "", lines, stdout, stderr, timeout, cts.Token), "ec-js", out _);
            // Jint stops itself at the timeout; the extra grace only covers a stuck host call.
            if (!await RunnerUtil.WaitAsync(task, timeout + TimeSpan.FromSeconds(2), ct).ConfigureAwait(false))
            {
                cts.Cancel();
                return RunnerUtil.Fail(ct.IsCancellationRequested ? RunnerUtil.CanceledMessage : RunnerUtil.TimeoutMessage(timeout), stdout.ToString(), sw);
            }
            var error = await task.ConfigureAwait(false);
            var err = stderr.ToString();
            if (error == null) return new RunResult(true, stdout.ToString(), err.TrimEnd(), sw.Elapsed);
            if (error == "") error = ct.IsCancellationRequested ? RunnerUtil.CanceledMessage : RunnerUtil.TimeoutMessage(timeout);
            return RunnerUtil.Fail(err.Length > 0 ? err + error : error, stdout.ToString(), sw);
        }
        catch (Exception ex)
        {
            return RunnerUtil.Fail("Erro interno ao rodar JavaScript: " + ex.Message, stdout.ToString(), sw);
        }
    }

    /// <summary>Returns null on success, "" on timeout/cancel, or a friendly error.</summary>
    private static string? Execute(string code, Queue<string> lines, CappedWriter stdout, CappedWriter stderr, TimeSpan timeout, CancellationToken ct)
    {
        var engine = new Engine(o =>
        {
            o.TimeoutInterval(timeout);
            o.CancellationToken(ct);
            o.LimitMemory(MemoryLimitBytes);
            o.LimitRecursion(RecursionLimit);
            o.MaxStatements(StatementLimit);
            o.Culture = CultureInfo.InvariantCulture;
        });
        engine.SetValue("__ec_write", new Action<string>(s => stdout.Write(s)));
        engine.SetValue("__ec_write_err", new Action<string>(s => stderr.Write(s)));
        engine.SetValue("__ec_read", new Func<JsValue>(() => lines.Count > 0 ? new JsString(lines.Dequeue()) : JsValue.Null));
        engine.Execute(Prelude, "prelude.js");
        try
        {
            engine.Execute(code, "main.js");
            return null;
        }
        catch (Exception ex)
        {
            return Describe(ex, timeout);
        }
    }

    private static string Describe(Exception ex, TimeSpan timeout)
    {
        // Our own output cap may surface wrapped inside a JavaScript error.
        for (var e = ex; e != null; e = e.InnerException)
            if (e is OutputLimitException) return RunnerUtil.OutputLimitMessage;

        var typeName = ex.GetType().Name;
        switch (typeName)
        {
            case "TimeoutException":
            case "ExecutionCanceledException":
            case "OperationCanceledException":
                return "";
            case "StatementsCountOverflowException":
                return "O programa executou instruções demais e foi interrompido (laço infinito?).";
            case "MemoryLimitExceededException":
                return "O programa usou memória demais e foi interrompido (uma lista ou texto crescendo sem parar?).";
            case "RecursionDepthOverflowException":
                return "Recursão profunda demais: a função chamou a si mesma vezes demais. Verifique o caso base (a condição que para a recursão).";
        }

        if (ex is Acornima.ParseErrorException pe)
            return $"Erro de sintaxe na linha {pe.LineNumber}, coluna {pe.Column}: {pe.Description}\n" +
                   "O código não pôde ser lido. Confira parênteses, chaves, aspas e vírgulas perto dessa linha.";

        if (ex is JavaScriptException jse)
        {
            var name = "Error";
            try
            {
                if (jse.Error is Jint.Native.Object.ObjectInstance o && o.Get("name") is JsString n) name = n.ToString();
            }
            catch { }
            var line = jse.Location.Start.Line;
            var where = line > 0 && jse.Location.SourceFile != "prelude.js" ? $" na linha {line}" : "";
            var hint = name switch
            {
                "ReferenceError" => "Você usou um nome (variável ou função) que não foi declarado. Confira a grafia e se ele foi criado antes.",
                "TypeError" => "Uma operação foi feita com um tipo de valor errado — por exemplo, chamar algo que não é função ou ler uma propriedade de undefined/null.",
                "RangeError" => "Um valor ficou fora do intervalo permitido (por exemplo, tamanho de array negativo ou recursão sem fim).",
                "SyntaxError" => "O código tem um erro de escrita. Confira parênteses, chaves, aspas e vírgulas.",
                _ => "",
            };
            var msg = jse.Message;
            var text = $"Erro{where}: {name}: {msg}";
            return hint.Length > 0 ? text + "\n" + hint : text;
        }

        return $"Erro: {ex.Message}";
    }
}
