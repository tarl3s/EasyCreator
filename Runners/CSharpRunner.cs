using System.IO;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;

namespace EasyCreator.Runners;

/// <summary>Offline C# with Roslyn: compiles a whole program (a Main method or top-level statements, with the SDK's
/// implicit usings) into memory, loads it in a collectible AssemblyLoadContext and runs it with Console.In/Out
/// redirected. Console redirection is process-wide, so runs are serialized. Every loop body and method gets a
/// call to a tiny guard that stops the program at the deadline and turns runaway recursion into a catchable error
/// (instead of a stack overflow that would kill the app). Compile time does not count toward the timeout.</summary>
public sealed class CSharpRunner : ILanguageRunner
{
    public string LanguageId => "csharp";
    public string DisplayName => "C# (embutido)";
    public bool IsExternal => false;
    public bool IsAvailable => true;

    private const string FileName = "Program.cs";
    /// <summary>Stack of the run thread. Runaway recursion stops when it is nearly full; unwinding a very deep
    /// stack grows quadratically slow (measured: 2 MB ≈ 30k calls, unwound in well under a second; 8 MB ≈ 130k
    /// calls, 30 s), so it is kept small.</summary>
    private const int StackBytes = 2 * 1024 * 1024;
    private static readonly SemaphoreSlim RunGate = new(1, 1);
    private static readonly object CacheLock = new();
    private static readonly LinkedList<(string Hash, Compiled Result)> Cache = new();
    private const int CacheSize = 8;

    private static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.CSharp12);

    private const string UsingsSource = """
global using System;
global using System.Collections.Generic;
global using System.IO;
global using System.Linq;
global using System.Net.Http;
global using System.Threading;
global using System.Threading.Tasks;
""";

    private const string GuardSource = """
internal static class __EcGuard
{
    public static long Deadline = long.MaxValue;
    public static volatile bool Stop;
    public static volatile bool TimedOut;
    public static void T()
    {
        if (Stop || System.Environment.TickCount64 > Deadline) { TimedOut = true; throw new __EcTimeout(); }
    }
    public static void E()
    {
        T();
        System.Runtime.CompilerServices.RuntimeHelpers.EnsureSufficientExecutionStack();
    }
}
internal sealed class __EcTimeout : System.Exception
{
    public __EcTimeout() : base("Tempo esgotado") { }
}
""";

    private static readonly Lazy<List<MetadataReference>> References = new(LoadReferences, true);

    private sealed record Compiled(byte[]? Image, string Error);

    private static List<MetadataReference> LoadReferences()
    {
        // The .NET runtime's own assemblies (not WPF, not this app).
        var coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var tpa = (AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string ?? "").Split(Path.PathSeparator);
        var list = new List<MetadataReference>();
        foreach (var path in tpa)
        {
            if (string.IsNullOrEmpty(path) || !path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) continue;
            if (!string.Equals(Path.GetDirectoryName(path), coreDir, StringComparison.OrdinalIgnoreCase)) continue;
            try { list.Add(MetadataReference.CreateFromFile(path)); } catch { }
        }
        if (list.Count == 0)
            foreach (var path in Directory.GetFiles(coreDir, "*.dll"))
                try { list.Add(MetadataReference.CreateFromFile(path)); } catch { }
        return list;
    }

    /// <summary>Warms up Roslyn (first compile takes a few seconds) so the first student run is quick.</summary>
    public static Task WarmUpAsync() => Task.Run(() => Compile("System.Console.WriteLine(1);"));

    public async Task<RunResult> RunAsync(string code, string stdin, TimeSpan timeout, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        Compiled compiled;
        try
        {
            compiled = await Task.Run(() => Compile(code ?? ""), ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return RunnerUtil.Fail(RunnerUtil.CanceledMessage, "", sw);
        }
        catch (Exception ex)
        {
            return RunnerUtil.Fail(L.F("Erro interno ao compilar C#: {0}", ex.Message), "", sw);
        }
        if (compiled.Image == null) return RunnerUtil.Fail(compiled.Error, "", sw);

        try
        {
            await RunGate.WaitAsync(ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return RunnerUtil.Fail(RunnerUtil.CanceledMessage, "", sw);
        }
        try
        {
            return await Execute(compiled.Image, stdin ?? "", timeout, ct, sw).ConfigureAwait(false);
        }
        finally
        {
            RunGate.Release();
        }
    }

    private static async Task<RunResult> Execute(byte[] image, string stdin, TimeSpan timeout, CancellationToken ct, Stopwatch sw)
    {
        var stdout = new CappedWriter();
        var stderr = new CappedWriter(20_000);
        var alc = new AssemblyLoadContext("ec-csharp", isCollectible: true);
        TextWriter oldOut = Console.Out, oldErr = Console.Error;
        TextReader oldIn = Console.In;
        try
        {
            var asm = alc.LoadFromStream(new MemoryStream(image));
            var guard = asm.GetType("__EcGuard")!;
            var entry = asm.EntryPoint;
            if (entry == null) return RunnerUtil.Fail(L.T("O programa não tem ponto de entrada: escreva comandos soltos (top-level) ou um método static void Main()."), "", sw);
            var stopField = guard.GetField("Stop")!;
            var timedOutField = guard.GetField("TimedOut")!;

            Console.SetOut(stdout);
            Console.SetError(stderr);
            Console.SetIn(new StringReader(stdin));

            guard.GetField("Deadline")!.SetValue(null, Environment.TickCount64 + (long)timeout.TotalMilliseconds);
            var task = RunnerUtil.RunOnThread<Exception?>(() =>
            {
                try
                {
                    var args = entry.GetParameters().Length == 1 ? new object?[] { Array.Empty<string>() } : null;
                    var ret = entry.Invoke(null, args);
                    if (ret is Task t) t.GetAwaiter().GetResult();
                    return null;
                }
                catch (Exception ex)
                {
                    return ex is TargetInvocationException { InnerException: { } inner } ? inner : ex;
                }
            }, "ec-csharp", out var thread, StackBytes);

            using var reg = ct.Register(() => stopField.SetValue(null, true));
            var finished = await RunnerUtil.WaitAsync(task, timeout + TimeSpan.FromSeconds(1), CancellationToken.None).ConfigureAwait(false);
            if (!finished)
            {
                // Blocked outside a loop (Thread.Sleep, a lock…): ask it to stop and wake it up.
                stopField.SetValue(null, true);
                try { thread.Interrupt(); } catch { }
                await RunnerUtil.WaitAsync(task, TimeSpan.FromSeconds(2), CancellationToken.None).ConfigureAwait(false);
                return RunnerUtil.Fail(ct.IsCancellationRequested ? RunnerUtil.CanceledMessage : RunnerUtil.TimeoutMessage(timeout), stdout.ToString(), sw);
            }

            var error = await task.ConfigureAwait(false);
            var err = stderr.ToString().TrimEnd();
            if (error == null) return new RunResult(true, stdout.ToString(), err, sw.Elapsed);
            if ((bool)timedOutField.GetValue(null)! || error.GetType().Name == "__EcTimeout" || error is ThreadInterruptedException)
                return RunnerUtil.Fail(ct.IsCancellationRequested ? RunnerUtil.CanceledMessage : RunnerUtil.TimeoutMessage(timeout), stdout.ToString(), sw);
            var text = Describe(error);
            return RunnerUtil.Fail(err.Length > 0 ? err + "\n" + text : text, stdout.ToString(), sw);
        }
        catch (Exception ex)
        {
            return RunnerUtil.Fail(L.F("Erro interno ao rodar C#: {0}", ex.Message), stdout.ToString(), sw);
        }
        finally
        {
            Console.SetOut(oldOut);
            Console.SetError(oldErr);
            Console.SetIn(oldIn);
            alc.Unload();
        }
    }

    // ------------------------------------------------------------------ compile

    private static string Hash(string code) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code)));

    private static Compiled Compile(string code)
    {
        var hash = Hash(code);
        lock (CacheLock)
        {
            for (var n = Cache.First; n != null; n = n.Next)
                if (n.Value.Hash == hash)
                {
                    Cache.Remove(n);
                    Cache.AddFirst(n);
                    return n.Value.Result;
                }
        }
        var result = CompileUncached(code);
        lock (CacheLock)
        {
            Cache.AddFirst((hash, result));
            while (Cache.Count > CacheSize) Cache.RemoveLast();
        }
        return result;
    }

    private static Compiled CompileUncached(string code)
    {
        var userTree = CSharpSyntaxTree.ParseText(code, ParseOptions, FileName, Encoding.UTF8);
        var forbidden = FindForbidden(userTree);
        if (forbidden != null) return new Compiled(null, forbidden);

        var guarded = new GuardRewriter().Visit(userTree.GetRoot());
        var runTree = CSharpSyntaxTree.Create((CSharpSyntaxNode)guarded, ParseOptions, FileName, Encoding.UTF8);
        var usings = CSharpSyntaxTree.ParseText(UsingsSource, ParseOptions, "Usings.g.cs", Encoding.UTF8);
        var guardTree = CSharpSyntaxTree.ParseText(GuardSource, ParseOptions, "Guard.g.cs", Encoding.UTF8);
        var options = new CSharpCompilationOptions(OutputKind.ConsoleApplication,
            optimizationLevel: OptimizationLevel.Debug, nullableContextOptions: NullableContextOptions.Disable);
        var name = "EcProgram_" + Guid.NewGuid().ToString("N");
        var compilation = CSharpCompilation.Create(name, new[] { runTree, usings, guardTree }, References.Value, options);

        using var pe = new MemoryStream();
        var emit = compilation.Emit(pe, options: new EmitOptions(debugInformationFormat: DebugInformationFormat.Embedded));
        if (emit.Success) return new Compiled(pe.ToArray(), "");

        // Report errors against the student's original text (exact lines and columns).
        var original = CSharpCompilation.Create(name, new[] { userTree, usings, guardTree }, References.Value, options);
        var diags = original.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        if (diags.Count == 0) diags = emit.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
        return new Compiled(null, FormatDiagnostics(diags));
    }

    private static string FormatDiagnostics(List<Diagnostic> diags)
    {
        // Compiler messages in the app's language (Roslyn ships pt-BR and es resources; English is the neutral one).
        var culture = CultureInfo.GetCultureInfo(L.Lang switch { "pt" => "pt-BR", "es" => "es-ES", _ => "en-US" });
        var sb = new StringBuilder(L.T("O código não compilou:") + "\n");
        foreach (var d in diags.OrderBy(d => d.Location.SourceSpan.Start).Take(10))
        {
            var span = d.Location.GetLineSpan();
            var pos = d.Location.IsInSource && span.Path == FileName
                ? L.F("Linha {0}, coluna {1}: ", span.StartLinePosition.Line + 1, span.StartLinePosition.Character + 1) : "";
            sb.Append("- ").Append(pos).Append(d.GetMessage(culture)).Append(" [").Append(d.Id).Append("]\n");
        }
        if (diags.Count > 10) sb.Append(L.F("… e mais {0} erro(s).", diags.Count - 10)).Append('\n');
        if (diags.Any(d => d.Id == "CS1002")) sb.Append(L.T("Dica: em C# todo comando termina com ponto e vírgula (;).")).Append('\n');
        if (diags.Any(d => d.Id == "CS0103")) sb.Append(L.T("Dica: um nome não existe neste ponto — confira a grafia (C# diferencia maiúsculas de minúsculas) e se a variável foi declarada.")).Append('\n');
        if (diags.Any(d => d.Id is "CS0029" or "CS0266" or "CS1503")) sb.Append(L.T("Dica: os tipos não combinam. Console.ReadLine() devolve texto: converta com int.Parse ou double.Parse.")).Append('\n');
        return sb.ToString().TrimEnd();
    }

    private static string? FindForbidden(SyntaxTree tree)
    {
        foreach (var m in tree.GetRoot().DescendantNodes().OfType<MemberAccessExpressionSyntax>())
        {
            var member = m.Name.Identifier.ValueText;
            if (member is not ("Exit" or "FailFast")) continue;
            var target = m.Expression.ToString();
            if (target == "Environment" || target.EndsWith(".Environment", StringComparison.Ordinal))
            {
                var line = m.GetLocation().GetLineSpan().StartLinePosition.Line + 1;
                return L.F("Linha {0}: Environment.{1} não é permitido aqui porque fecharia o EasyCreator. Para encerrar o programa, use return dentro do Main.", line, member);
            }
        }
        return null;
    }

    /// <summary>Adds <c>__EcGuard.T()</c> at the start of every loop body and <c>__EcGuard.E()</c> at the start of
    /// every method / local function. Inserted tokens carry no line breaks, so line numbers stay the same.</summary>
    private sealed class GuardRewriter : CSharpSyntaxRewriter
    {
        private static StatementSyntax Tick() => SyntaxFactory.ParseStatement("global::__EcGuard.T();");
        private static StatementSyntax Enter() => SyntaxFactory.ParseStatement("global::__EcGuard.E();");

        private static BlockSyntax Prepend(StatementSyntax body, StatementSyntax first) =>
            body is BlockSyntax b ? b.WithStatements(b.Statements.Insert(0, first)) : SyntaxFactory.Block(first, body);

        public override SyntaxNode? VisitWhileStatement(WhileStatementSyntax node)
        {
            var n = (WhileStatementSyntax)base.VisitWhileStatement(node)!;
            return n.WithStatement(Prepend(n.Statement, Tick()));
        }

        public override SyntaxNode? VisitDoStatement(DoStatementSyntax node)
        {
            var n = (DoStatementSyntax)base.VisitDoStatement(node)!;
            return n.WithStatement(Prepend(n.Statement, Tick()));
        }

        public override SyntaxNode? VisitForStatement(ForStatementSyntax node)
        {
            var n = (ForStatementSyntax)base.VisitForStatement(node)!;
            return n.WithStatement(Prepend(n.Statement, Tick()));
        }

        public override SyntaxNode? VisitForEachStatement(ForEachStatementSyntax node)
        {
            var n = (ForEachStatementSyntax)base.VisitForEachStatement(node)!;
            return n.WithStatement(Prepend(n.Statement, Tick()));
        }

        public override SyntaxNode? VisitForEachVariableStatement(ForEachVariableStatementSyntax node)
        {
            var n = (ForEachVariableStatementSyntax)base.VisitForEachVariableStatement(node)!;
            return n.WithStatement(Prepend(n.Statement, Tick()));
        }

        public override SyntaxNode? VisitLabeledStatement(LabeledStatementSyntax node)
        {
            // goto-based loops jump to a label: check there too.
            var n = (LabeledStatementSyntax)base.VisitLabeledStatement(node)!;
            return n.WithStatement(Prepend(n.Statement, Tick()));
        }

        public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var n = (MethodDeclarationSyntax)base.VisitMethodDeclaration(node)!;
            if (n.Body != null) return n.WithBody(Prepend(n.Body, Enter()));
            if (n.ExpressionBody == null) return n;
            var block = ExpressionToBlock(n.ExpressionBody, n.SemicolonToken, ReturnsValue(n.ReturnType, n.Modifiers));
            return n.WithExpressionBody(null).WithSemicolonToken(default).WithBody(block);
        }

        public override SyntaxNode? VisitLocalFunctionStatement(LocalFunctionStatementSyntax node)
        {
            var n = (LocalFunctionStatementSyntax)base.VisitLocalFunctionStatement(node)!;
            if (n.Body != null) return n.WithBody(Prepend(n.Body, Enter()));
            if (n.ExpressionBody == null) return n;
            var block = ExpressionToBlock(n.ExpressionBody, n.SemicolonToken, ReturnsValue(n.ReturnType, n.Modifiers));
            return n.WithExpressionBody(null).WithSemicolonToken(default).WithBody(block);
        }

        private static bool ReturnsValue(TypeSyntax returnType, SyntaxTokenList modifiers)
        {
            if (returnType is PredefinedTypeSyntax p && p.Keyword.IsKind(SyntaxKind.VoidKeyword)) return false;
            if (modifiers.Any(SyntaxKind.AsyncKeyword))
            {
                var t = returnType.ToString().Trim();
                if (t is "Task" or "ValueTask" or "System.Threading.Tasks.Task" or "System.Threading.Tasks.ValueTask") return false;
            }
            return true;
        }

        private static BlockSyntax ExpressionToBlock(ArrowExpressionClauseSyntax arrow, SyntaxToken semicolon, bool returns)
        {
            var expr = arrow.Expression;
            StatementSyntax stmt = returns
                ? SyntaxFactory.ReturnStatement(SyntaxFactory.Token(SyntaxKind.ReturnKeyword).WithTrailingTrivia(SyntaxFactory.Space), expr, SyntaxFactory.Token(SyntaxKind.SemicolonToken))
                : SyntaxFactory.ExpressionStatement(expr);
            var open = SyntaxFactory.Token(SyntaxKind.OpenBraceToken).WithLeadingTrivia(arrow.ArrowToken.LeadingTrivia).WithTrailingTrivia(arrow.ArrowToken.TrailingTrivia);
            var close = SyntaxFactory.Token(SyntaxKind.CloseBraceToken).WithLeadingTrivia(semicolon.LeadingTrivia).WithTrailingTrivia(semicolon.TrailingTrivia);
            return SyntaxFactory.Block(open, SyntaxFactory.List(new[] { Enter(), stmt }), close);
        }
    }

    // ------------------------------------------------------------------ runtime errors

    private static string Describe(Exception ex)
    {
        if (ex is OutputLimitException) return RunnerUtil.OutputLimitMessage;
        if (ex is InsufficientExecutionStackException)
            return L.T("Recursão profunda demais: o método chamou a si mesmo vezes demais e a pilha acabou. Verifique o caso base (a condição que para a recursão).");

        int line = 0;
        try
        {
            foreach (var f in new StackTrace(ex, true).GetFrames())
            {
                var file = f.GetFileName();
                if (file != null && Path.GetFileName(file) == FileName && f.GetFileLineNumber() > 0) { line = f.GetFileLineNumber(); break; }
            }
        }
        catch { }
        var name = ex.GetType().Name;
        var tip = ex switch
        {
            DivideByZeroException => L.T("Divisão por zero: o divisor ficou 0. Teste o valor antes de dividir."),
            IndexOutOfRangeException => L.T("Índice fora do vetor: as posições vão de 0 até Length - 1."),
            ArgumentOutOfRangeException => L.T("Índice ou valor fora do intervalo permitido (em listas, as posições vão de 0 até Count - 1)."),
            ArgumentNullException => L.T("Um valor nulo foi usado — muitas vezes Console.ReadLine() devolveu null porque a entrada acabou."),
            NullReferenceException => L.T("Você usou um objeto que vale null (não foi criado com new ou a entrada acabou)."),
            FormatException => L.T("O texto não pôde ser convertido em número. Confira o que foi lido da entrada."),
            OverflowException => L.T("O número ficou grande demais para o tipo (tente long ou double)."),
            InvalidCastException => L.T("Conversão de tipo inválida."),
            KeyNotFoundException => L.T("Essa chave não existe no dicionário. Use ContainsKey ou TryGetValue antes."),
            InvalidOperationException => L.T("Operação inválida no estado atual (por exemplo, modificar uma lista enquanto percorre com foreach)."),
            _ => "",
        };
        var head = line > 0 ? L.F("Erro na linha {0}: {1}: {2}", line, name, ex.Message) : L.F("Erro: {0}: {1}", name, ex.Message);
        return tip.Length > 0 ? head + "\n" + tip : head;
    }
}
