using System.IO;
using System.Diagnostics;
using System.Text;

namespace EasyCreator.Runners;

/// <summary>Thrown by <see cref="CappedWriter"/> when a program prints too much (usually a runaway loop).</summary>
public sealed class OutputLimitException : Exception
{
    public OutputLimitException() : base("Saída grande demais") { }
}

/// <summary>A thread-safe TextWriter that keeps at most <see cref="Limit"/> characters and then throws, so a
/// runaway print loop can't eat the machine's memory.</summary>
public sealed class CappedWriter : TextWriter
{
    private readonly StringBuilder _sb = new();
    private readonly object _lock = new();
    public int Limit { get; }
    public bool Overflowed { get; private set; }

    public CappedWriter(int limit = RunnerUtil.MaxOutputChars) { Limit = limit; }

    public override Encoding Encoding => Encoding.UTF8;

    public override void Write(char value)
    {
        lock (_lock) { Check(1); _sb.Append(value); }
    }

    public override void Write(string? value)
    {
        if (string.IsNullOrEmpty(value)) return;
        lock (_lock) { Check(value.Length); _sb.Append(value); }
    }

    public override void Write(char[] buffer, int index, int count)
    {
        lock (_lock) { Check(count); _sb.Append(buffer, index, count); }
    }

    private void Check(int extra)
    {
        if (_sb.Length + extra <= Limit) return;
        Overflowed = true;
        throw new OutputLimitException();
    }

    public override string ToString()
    {
        lock (_lock) return _sb.ToString();
    }
}

internal static class RunnerUtil
{
    public const int MaxOutputChars = 200_000;
    /// <summary>Stack for the threads that run student code (deep recursion must hit the language's own limit first).</summary>
    public const int StackBytes = 64 * 1024 * 1024;

    public static string TimeoutMessage(TimeSpan timeout) =>
        $"Tempo esgotado: o programa rodou por mais de {timeout.TotalSeconds:0.#} s e foi interrompido. " +
        "Verifique se existe um laço infinito (uma repetição cuja condição nunca fica falsa) " +
        "ou se o programa está esperando mais entrada do que o teste fornece.";

    public const string OutputLimitMessage =
        "O programa escreveu texto demais (mais de 200 mil caracteres) e foi interrompido. " +
        "Provavelmente há um laço infinito com print dentro.";

    public const string CanceledMessage = "Execução cancelada.";

    /// <summary>Runs <paramref name="body"/> on a dedicated background thread with a big stack and the invariant
    /// culture (dot as decimal separator, like the lessons expect).</summary>
    public static Task<T> RunOnThread<T>(Func<T> body, string name, out Thread thread, int stackBytes = StackBytes)
    {
        var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        thread = new Thread(() =>
        {
            try { tcs.TrySetResult(body()); }
            catch (Exception ex) { tcs.TrySetException(ex); }
        }, stackBytes)
        {
            IsBackground = true,
            Name = name,
            CurrentCulture = System.Globalization.CultureInfo.InvariantCulture,
            CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture,
        };
        thread.Start();
        return tcs.Task;
    }

    /// <summary>Waits for <paramref name="task"/> up to <paramref name="wait"/> (or cancellation). True if it finished.</summary>
    public static async Task<bool> WaitAsync(Task task, TimeSpan wait, CancellationToken ct)
    {
        try
        {
            await task.WaitAsync(wait, ct).ConfigureAwait(false);
            return true;
        }
        catch (TimeoutException) { return false; }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { return false; }
        catch { return true; } // the task itself faulted: it did finish
    }

    public static string Truncate(string s, int max = 4000) => s.Length <= max ? s : s[..max] + "\n…";

    public static string NormalizeNewlines(string s) => s.Replace("\r\n", "\n").Replace('\r', '\n');

    /// <summary>Lines of stdin as a program would read them (a trailing newline does not make an extra empty line).</summary>
    public static List<string> StdinLines(string stdin)
    {
        var text = NormalizeNewlines(stdin ?? "");
        var lines = text.Split('\n').ToList();
        if (lines.Count > 0 && lines[^1].Length == 0) lines.RemoveAt(lines.Count - 1);
        return lines;
    }

    public static RunResult Fail(string error, string stdout, Stopwatch sw) => new(false, stdout, error, sw.Elapsed);
}
