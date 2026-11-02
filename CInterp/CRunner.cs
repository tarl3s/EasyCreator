using System.Diagnostics;

namespace EasyCreator.CInterp;

/// <summary>Runs C programs with the built-in interpreter (no compiler needed).</summary>
public sealed class CRunner : ILanguageRunner
{
    public string LanguageId => "c";
    public string DisplayName => "C (embutido)";
    public bool IsExternal => false;
    public bool IsAvailable => true;

    public Task<RunResult> RunAsync(string code, string stdin, TimeSpan timeout, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<RunResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var th = new Thread(() => tcs.TrySetResult(Run(code, stdin, timeout, ct)), 256 * 1024 * 1024)
        {
            IsBackground = true,
            Name = "C runner"
        };
        th.Start();
        return tcs.Task;
    }

    /// <summary>Synchronous run (call it on a thread with a big stack; <see cref="RunAsync"/> does that).</summary>
    public static RunResult Run(string code, string stdin, TimeSpan timeout, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var ip = new CInterpreter
        {
            In = (stdin ?? "").Replace("\r\n", "\n"),
            Timeout = timeout <= TimeSpan.Zero ? TimeSpan.FromSeconds(10) : timeout,
            Cancel = ct
        };
        try
        {
            ip.Compile(code ?? "");
            int exit = ip.RunMain();
            string err = exit == 0 ? "" : "O programa terminou com código " + exit + " (return " + exit + " no main ou exit(" + exit + ")).";
            return new RunResult(exit == 0, ip.Out.ToString(), err, sw.Elapsed);
        }
        catch (CError e)
        {
            return new RunResult(false, ip.Out.ToString(), e.Friendly, sw.Elapsed);
        }
        catch (CExit e)
        {
            return new RunResult(e.Code == 0, ip.Out.ToString(), e.Code == 0 ? "" : "O programa terminou com código " + e.Code + ".", sw.Elapsed);
        }
        catch (CHalt)
        {
            return new RunResult(false, ip.Out.ToString(), "Execução interrompida.", sw.Elapsed);
        }
        catch (InsufficientExecutionStackException)
        {
            return new RunResult(false, ip.Out.ToString(), "Erro de execução (linha " + ip.Line + "): recursão profunda demais (estouro de pilha).", sw.Elapsed);
        }
        catch (Exception e)
        {
            return new RunResult(false, ip.Out.ToString(),
                "Erro interno do interpretador perto da linha " + ip.Line + ": " + e.Message + " (o seu programa pode usar algo ainda não suportado).", sw.Elapsed);
        }
    }
}
