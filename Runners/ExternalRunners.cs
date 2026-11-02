using System.IO;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyCreator.Runners;

/// <summary>Result of running an external process.</summary>
internal sealed record ProcResult(int ExitCode, string Stdout, string Stderr, bool TimedOut, bool Canceled, bool OutputOverflow, TimeSpan Elapsed);

/// <summary>Starting processes with stdin, a timeout (kills the whole tree) and capped output.</summary>
internal static class Proc
{
    public static async Task<ProcResult> RunAsync(string exe, IEnumerable<string> args, string? workDir, string stdin,
        TimeSpan timeout, CancellationToken ct, IDictionary<string, string>? env = null)
    {
        var sw = Stopwatch.StartNew();
        var psi = new ProcessStartInfo(exe)
        {
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = new UTF8Encoding(false),
            StandardErrorEncoding = new UTF8Encoding(false),
            StandardInputEncoding = new UTF8Encoding(false),
        };
        if (workDir != null) psi.WorkingDirectory = workDir;
        foreach (var a in args) psi.ArgumentList.Add(a);
        if (env != null) foreach (var kv in env) psi.Environment[kv.Key] = kv.Value;

        using var p = new Process { StartInfo = psi };
        var stdout = new CappedWriter();
        var stderr = new CappedWriter(50_000);
        bool overflow = false;
        void OnOut(string? s, CappedWriter w)
        {
            if (s == null) return;
            try { w.Write(s); w.Write('\n'); }
            catch (OutputLimitException) { overflow = true; Kill(p); }
        }
        p.OutputDataReceived += (_, e) => OnOut(e.Data, stdout);
        p.ErrorDataReceived += (_, e) => OnOut(e.Data, stderr);
        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        try
        {
            await p.StandardInput.WriteAsync(stdin ?? "").ConfigureAwait(false);
            p.StandardInput.Close();
        }
        catch { /* the program may exit without reading its input */ }

        bool timedOut = false, canceled = false;
        using (var cts = CancellationTokenSource.CreateLinkedTokenSource(ct))
        {
            cts.CancelAfter(timeout);
            try { await p.WaitForExitAsync(cts.Token).ConfigureAwait(false); }
            catch (OperationCanceledException)
            {
                if (ct.IsCancellationRequested) canceled = true; else timedOut = true;
                Kill(p);
                try { await p.WaitForExitAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(false); } catch { }
            }
        }
        try { p.WaitForExit(2000); } catch { } // flush the async readers
        int code;
        try { code = p.HasExited ? p.ExitCode : -1; } catch { code = -1; }
        return new ProcResult(code, stdout.ToString(), stderr.ToString(), timedOut, canceled, overflow, sw.Elapsed);
    }

    private static void Kill(Process p)
    {
        try { if (!p.HasExited) p.Kill(entireProcessTree: true); } catch { }
    }

    /// <summary>Runs "exe --version"-like probes; returns the combined output or null if it can't run.</summary>
    public static string? Probe(string exe, params string[] args)
    {
        try
        {
            var r = RunAsync(exe, args, null, "", TimeSpan.FromSeconds(10), CancellationToken.None).GetAwaiter().GetResult();
            if (r.TimedOut || r.ExitCode != 0) return null;
            return (r.Stdout + "\n" + r.Stderr).Trim();
        }
        catch { return null; }
    }

    public static string NewTempDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "EasyCreator", "run-" + Guid.NewGuid().ToString("N")[..12]);
        Directory.CreateDirectory(dir);
        return dir;
    }

    public static async Task DeleteDirAsync(string dir)
    {
        for (var i = 0; i < 5; i++)
        {
            try
            {
                if (Directory.Exists(dir)) Directory.Delete(dir, true);
                return;
            }
            catch { await Task.Delay(200).ConfigureAwait(false); }
        }
    }

    /// <summary>Every directory on PATH plus extra folders; yields existing files named <paramref name="names"/>.</summary>
    public static IEnumerable<string> Find(IEnumerable<string> names, IEnumerable<string> extraDirs)
    {
        var dirs = new List<string>();
        dirs.AddRange(extraDirs);
        dirs.AddRange((Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator));
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in dirs)
        {
            if (string.IsNullOrWhiteSpace(d)) continue;
            string dir;
            try { dir = Path.GetFullPath(Environment.ExpandEnvironmentVariables(d.Trim().Trim('"'))); } catch { continue; }
            foreach (var n in names)
            {
                var f = Path.Combine(dir, n);
                if (seen.Add(f) && File.Exists(f)) yield return f;
            }
        }
    }

    /// <summary>Expands simple wildcards in the last segments of folder patterns (e.g. C:\Python3*\).</summary>
    public static IEnumerable<string> Glob(string pattern)
    {
        pattern = Environment.ExpandEnvironmentVariables(pattern);
        if (!pattern.Contains('*')) { yield return pattern; yield break; }
        var parts = pattern.Split('\\');
        IEnumerable<string> current = new[] { parts[0] + "\\" };
        foreach (var part in parts.Skip(1))
        {
            if (part.Length == 0) continue;
            var next = new List<string>();
            foreach (var c in current)
            {
                if (!part.Contains('*')) { next.Add(Path.Combine(c, part)); continue; }
                try { if (Directory.Exists(c)) next.AddRange(Directory.GetDirectories(c, part).OrderByDescending(x => x)); } catch { }
            }
            current = next;
        }
        foreach (var c in current) yield return c;
    }

    public static string Clean(string text, string dir)
    {
        if (string.IsNullOrEmpty(text)) return "";
        text = text.Replace(dir + Path.DirectorySeparatorChar, "").Replace(dir, "");
        return RunnerUtil.Truncate(text.TrimEnd());
    }

    /// <summary>A friendly description of a failed process run.</summary>
    public static string Describe(ProcResult r, TimeSpan timeout, string dir, Regex? lineRegex = null)
    {
        if (r.Canceled) return RunnerUtil.CanceledMessage;
        if (r.TimedOut) return RunnerUtil.TimeoutMessage(timeout);
        if (r.OutputOverflow) return RunnerUtil.OutputLimitMessage;
        var reason = (uint)r.ExitCode switch
        {
            0xC0000005 => L.T(" — acesso inválido à memória (ponteiro inválido ou índice fora do vetor)"),
            0xC00000FD => L.T(" — estouro de pilha (recursão sem fim?)"),
            0xC0000094 => L.T(" — divisão inteira por zero"),
            0xC0000409 => L.T(" — o programa foi abortado (memória corrompida?)"),
            _ => "",
        };
        var err = Clean(r.Stderr, dir);
        var line = 0;
        if (lineRegex != null)
            foreach (Match m in lineRegex.Matches(r.Stderr)) line = int.Parse(m.Groups[1].Value); // the innermost frame is last
        var syntax = Regex.IsMatch(r.Stderr, @"\b(SyntaxError|IndentationError|TabError)\b");
        var head = line > 0
            ? (syntax ? L.F("Erro de sintaxe na linha {0} (o programa terminou com código {1}){2}.", line, r.ExitCode, reason)
                      : L.F("Erro na linha {0} (o programa terminou com código {1}){2}.", line, r.ExitCode, reason))
            : L.F("O programa terminou com erro (código {0}){1}.", r.ExitCode, reason);
        return err.Length > 0 ? head + "\n" + err : head;
    }
}

/// <summary>Base for runners that call an installed tool.</summary>
public abstract class ExternalRunner : ILanguageRunner
{
    public abstract string LanguageId { get; }
    public abstract string DisplayName { get; }
    public bool IsExternal => true;
    public bool IsAvailable => File.Exists(ExePath);
    /// <summary>Full path of the tool.</summary>
    public string ExePath { get; }
    /// <summary>Version text reported by the tool (e.g. "3.12.1").</summary>
    public string Version { get; }

    protected ExternalRunner(string exePath, string version)
    {
        ExePath = exePath;
        Version = version;
    }

    public async Task<RunResult> RunAsync(string code, string stdin, TimeSpan timeout, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            return await RunCoreAsync(code ?? "", stdin ?? "", timeout, ct, sw).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return new RunResult(false, "", L.F("Não foi possível rodar {0}: {1}", DisplayName, ex.Message), sw.Elapsed);
        }
    }

    protected abstract Task<RunResult> RunCoreAsync(string code, string stdin, TimeSpan timeout, CancellationToken ct, Stopwatch sw);

    /// <summary>Finds the line of the student's file in an error message (group 1), e.g. "main.py", line 3.</summary>
    private protected virtual Regex? ErrorLine => null;

    private protected RunResult FromProc(ProcResult r, TimeSpan timeout, string dir, Stopwatch sw)
    {
        var ok = r.ExitCode == 0 && !r.TimedOut && !r.Canceled && !r.OutputOverflow;
        return ok
            ? new RunResult(true, r.Stdout, Proc.Clean(r.Stderr, dir), sw.Elapsed)
            : new RunResult(false, r.Stdout, Proc.Describe(r, timeout, dir, ErrorLine), sw.Elapsed);
    }

    public override string ToString() => DisplayName;
}

/// <summary>JavaScript with an installed Node.js. A preloaded module defines prompt()/readline() over stdin.</summary>
public sealed class NodeRunner : ExternalRunner
{
    public NodeRunner(string exe, string version) : base(exe, version) { }
    public override string LanguageId => "javascript";
    public override string DisplayName => L.F("Node.js (instalado: {0})", Version);
    private protected override Regex ErrorLine { get; } = new(@"main\.js:(\d+)");

    private const string Prelude = """
const __lines = (() => { try { return require('fs').readFileSync(0, 'utf8'); } catch { return ''; } })().replace(/\r\n/g, '\n').split('\n');
if (__lines.length && __lines[__lines.length - 1] === '') __lines.pop();
let __i = 0;
globalThis.prompt = () => (__i < __lines.length ? __lines[__i++] : null);
globalThis.readline = globalThis.prompt;
""";

    protected override async Task<RunResult> RunCoreAsync(string code, string stdin, TimeSpan timeout, CancellationToken ct, Stopwatch sw)
    {
        var dir = Proc.NewTempDir();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(dir, "prelude.js"), Prelude, ct).ConfigureAwait(false);
            await File.WriteAllTextAsync(Path.Combine(dir, "main.js"), code, ct).ConfigureAwait(false);
            var r = await Proc.RunAsync(ExePath, new[] { "--stack-size=4000", "-r", "./prelude.js", "main.js" }, dir, stdin, timeout, ct).ConfigureAwait(false);
            return FromProc(r, timeout, dir, sw);
        }
        finally { await Proc.DeleteDirAsync(dir).ConfigureAwait(false); }
    }
}

/// <summary>Python with an installed CPython (never the Microsoft Store stub).</summary>
public sealed class CPythonRunner : ExternalRunner
{
    public CPythonRunner(string exe, string version) : base(exe, version) { }
    public override string LanguageId => "python";
    public override string DisplayName => L.F("Python (instalado: {0})", Version);
    private protected override Regex ErrorLine { get; } = new(@"main\.py"", line (\d+)");

    protected override async Task<RunResult> RunCoreAsync(string code, string stdin, TimeSpan timeout, CancellationToken ct, Stopwatch sw)
    {
        var dir = Proc.NewTempDir();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(dir, "main.py"), code, new UTF8Encoding(false), ct).ConfigureAwait(false);
            var env = new Dictionary<string, string> { ["PYTHONIOENCODING"] = "utf-8", ["PYTHONUTF8"] = "1", ["PYTHONDONTWRITEBYTECODE"] = "1" };
            var r = await Proc.RunAsync(ExePath, new[] { "-X", "utf8", "main.py" }, dir, stdin, timeout, ct, env).ConfigureAwait(false);
            var res = FromProc(r, timeout, dir, sw);
            if (!res.Ok && !r.TimedOut && !r.Canceled)
            {
                var m = Regex.Matches(r.Stderr, @"^(\w+Error)\b", RegexOptions.Multiline);
                var tip = m.Count > 0 ? PythonRunner.Explain(m[^1].Groups[1].Value) : "";
                if (tip.Length > 0) res = res with { Error = res.Error + "\n" + tip };
            }
            return res;
        }
        finally { await Proc.DeleteDirAsync(dir).ConfigureAwait(false); }
    }
}

/// <summary>C with an installed gcc, clang or tcc: compile to a temporary .exe, then run it.</summary>
public sealed class NativeCRunner : ExternalRunner
{
    /// <summary>"gcc", "clang" or "tcc".</summary>
    public string Compiler { get; }
    public NativeCRunner(string exe, string version, string compiler) : base(exe, version) { Compiler = compiler; }
    public override string LanguageId => "c";
    public override string DisplayName => L.F("C ({0} instalado: {1})", Compiler, Version);

    public static readonly TimeSpan CompileTimeout = TimeSpan.FromSeconds(60);

    protected override async Task<RunResult> RunCoreAsync(string code, string stdin, TimeSpan timeout, CancellationToken ct, Stopwatch sw)
    {
        var dir = Proc.NewTempDir();
        try
        {
            await File.WriteAllTextAsync(Path.Combine(dir, "main.c"), code, new UTF8Encoding(false), ct).ConfigureAwait(false);
            var binDir = Path.GetDirectoryName(ExePath)!;
            // The compiler's folder must be on PATH for its helper programs and the runtime DLLs (MinGW).
            var env = new Dictionary<string, string> { ["PATH"] = binDir + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH") };
            var args = Compiler == "tcc"
                ? new[] { "main.c", "-o", "main.exe" }
                : new[] { "main.c", "-o", "main.exe", "-std=c11", "-O0", "-lm", "-fdiagnostics-color=never" };
            var c = await Proc.RunAsync(ExePath, args, dir, "", CompileTimeout, ct, env).ConfigureAwait(false);
            if (c.Canceled) return new RunResult(false, "", RunnerUtil.CanceledMessage, sw.Elapsed);
            if (c.ExitCode != 0 || !File.Exists(Path.Combine(dir, "main.exe")))
            {
                var msg = c.TimedOut ? L.T("A compilação demorou demais.") : Proc.Clean(c.Stderr + c.Stdout, dir);
                return new RunResult(false, "", L.T("O código não compilou:") + "\n" + msg, sw.Elapsed);
            }
            var r = await Proc.RunAsync(Path.Combine(dir, "main.exe"), Array.Empty<string>(), dir, stdin, timeout, ct, env).ConfigureAwait(false);
            return FromProc(r, timeout, dir, sw);
        }
        finally { await Proc.DeleteDirAsync(dir).ConfigureAwait(false); }
    }
}

/// <summary>C# with an installed .NET SDK: one cached console project (built only when the code changes).</summary>
public sealed class DotnetSdkRunner : ExternalRunner
{
    private readonly string _tfm;
    private static readonly SemaphoreSlim Gate = new(1, 1);
    private static string? _builtHash; // one shared project folder, so one shared "what is built"
    public DotnetSdkRunner(string exe, string sdkVersion) : base(exe, sdkVersion)
    {
        var major = int.TryParse(sdkVersion.Split('.')[0], out var m) ? m : 8;
        _tfm = $"net{major}.0";
    }
    public override string LanguageId => "csharp";
    public override string DisplayName => L.F(".NET SDK (instalado: {0})", Version);
    private protected override Regex ErrorLine { get; } = new(@"Program\.cs:line (\d+)");

    public static readonly TimeSpan BuildTimeout = TimeSpan.FromSeconds(180);

    private static string ProjectDir => Path.Combine(Path.GetTempPath(), "EasyCreator", "dotnet-runner");

    private static readonly Dictionary<string, string> Env = new()
    {
        ["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1",
        ["DOTNET_NOLOGO"] = "1",
        ["DOTNET_SKIP_FIRST_TIME_EXPERIENCE"] = "1",
        ["DOTNET_SYSTEM_GLOBALIZATION_INVARIANT"] = "1",
        ["MSBUILDDISABLENODEREUSE"] = "1",
    };

    private const string ConsoleSetup = """
internal static class __EcConsoleSetup
{
    [System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Init()
    {
        System.Console.OutputEncoding = new System.Text.UTF8Encoding(false);
        System.Console.InputEncoding = new System.Text.UTF8Encoding(false);
    }
}
""";

    protected override async Task<RunResult> RunCoreAsync(string code, string stdin, TimeSpan timeout, CancellationToken ct, Stopwatch sw)
    {
        await Gate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var dir = ProjectDir;
            Directory.CreateDirectory(dir);
            var csproj = Path.Combine(dir, "ecrun.csproj");
            var proj = $"""
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>{_tfm}</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>disable</Nullable>
    <UseAppHost>false</UseAppHost>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
    <NoWarn>$(NoWarn);CS8321;CS0168;CS0219</NoWarn>
  </PropertyGroup>
</Project>
""";
            if (!File.Exists(csproj) || await File.ReadAllTextAsync(csproj, ct).ConfigureAwait(false) != proj)
            {
                await File.WriteAllTextAsync(csproj, proj, ct).ConfigureAwait(false);
                _builtHash = null;
            }
            var hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(code)));
            var outDir = Path.Combine(dir, "out");
            var dll = Path.Combine(outDir, "ecrun.dll");
            if (_builtHash != hash || !File.Exists(dll))
            {
                _builtHash = null;
                await File.WriteAllTextAsync(Path.Combine(dir, "Program.cs"), code, new UTF8Encoding(false), ct).ConfigureAwait(false);
                await File.WriteAllTextAsync(Path.Combine(dir, "EcConsoleSetup.g.cs"), ConsoleSetup, ct).ConfigureAwait(false);
                var b = await Proc.RunAsync(ExePath, new[] { "build", "-nologo", "-v", "q", "-clp:NoSummary;ErrorsOnly", "-o", "out" },
                    dir, "", BuildTimeout, ct, Env).ConfigureAwait(false);
                if (b.Canceled) return new RunResult(false, "", RunnerUtil.CanceledMessage, sw.Elapsed);
                if (b.ExitCode != 0 || b.TimedOut)
                    return new RunResult(false, "", b.TimedOut ? L.T("A compilação demorou demais.") : FormatBuildErrors(b.Stdout + "\n" + b.Stderr, dir), sw.Elapsed);
                _builtHash = hash;
            }
            var r = await Proc.RunAsync(ExePath, new[] { dll }, dir, stdin, timeout, ct, Env).ConfigureAwait(false);
            return FromProc(r, timeout, dir, sw);
        }
        finally { Gate.Release(); }
    }

    private static string FormatBuildErrors(string output, string dir)
    {
        var sb = new StringBuilder(L.T("O código não compilou:") + "\n");
        var seen = new HashSet<string>();
        foreach (Match m in Regex.Matches(output, @"Program\.cs\((\d+),(\d+)\): error (CS\d+): (.*?)(?: \[[^\]]*\])?\r?$", RegexOptions.Multiline))
        {
            var line = "- " + L.F("Linha {0}, coluna {1}: ", m.Groups[1].Value, m.Groups[2].Value) + $"{m.Groups[4].Value} [{m.Groups[3].Value}]";
            if (seen.Add(line)) sb.Append(line).Append('\n');
        }
        if (seen.Count == 0) sb.Append(Proc.Clean(output, dir));
        return sb.ToString().TrimEnd();
    }
}

/// <summary>Finds installed tools. Every candidate is verified by running it (the Windows Store "python" stub fails).</summary>
public static class ExternalRunners
{
    private static string LocalAppData => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private static string ProgramFiles => Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
    private static string UserProfile => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

    public static Task<List<ILanguageRunner>> DetectAsync() => Task.Run(() =>
    {
        var probes = new Func<IEnumerable<ILanguageRunner>>[] { DetectNode, DetectPython, DetectC, DetectDotnet };
        var results = new List<ILanguageRunner>[probes.Length];
        Parallel.For(0, probes.Length, i =>
        {
            try { results[i] = probes[i]().ToList(); } catch { results[i] = new(); }
        });
        return results.SelectMany(r => r).ToList();
    });

    private static bool IsStoreStub(string path) => path.Contains(@"\WindowsApps\", StringComparison.OrdinalIgnoreCase);

    public static IEnumerable<ILanguageRunner> DetectNode()
    {
        var extra = new[] { Path.Combine(ProgramFiles, "nodejs"), Path.Combine(LocalAppData, "Programs", "nodejs"), @"C:\Program Files (x86)\nodejs" };
        foreach (var exe in Proc.Find(new[] { "node.exe" }, extra))
        {
            var v = Proc.Probe(exe, "--version");
            if (v != null && v.StartsWith('v')) { yield return new NodeRunner(exe, v.TrimStart('v')); yield break; }
        }
    }

    public static IEnumerable<ILanguageRunner> DetectPython()
    {
        var extra = new List<string> { Path.Combine(LocalAppData, "Python", "bin") };
        extra.AddRange(Proc.Glob(Path.Combine(LocalAppData, @"Programs\Python\Python3*")));
        extra.AddRange(Proc.Glob(@"C:\Python3*"));
        extra.AddRange(Proc.Glob(Path.Combine(ProgramFiles, "Python3*")));
        extra.AddRange(Proc.Glob(Path.Combine(LocalAppData, @"Python\pythoncore-3*")));
        extra.Add(@"C:\Windows");
        var candidates = Proc.Find(new[] { "python.exe", "python3.exe", "py.exe" }, extra)
            .OrderBy(p => IsStoreStub(p) ? 1 : 0); // real installs first; a WindowsApps alias only if it really runs
        foreach (var exe in candidates)
        {
            var v = Proc.Probe(exe, "--version");
            var m = v == null ? null : Regex.Match(v, @"Python (3\.\d+(?:\.\d+)?)");
            if (m is { Success: true }) { yield return new CPythonRunner(exe, m.Groups[1].Value); yield break; }
        }
    }

    public static IEnumerable<ILanguageRunner> DetectC()
    {
        var extra = new List<string>
        {
            @"C:\msys64\ucrt64\bin", @"C:\msys64\mingw64\bin", @"C:\msys64\clang64\bin", @"C:\msys64\mingw32\bin",
            @"C:\MinGW\bin", @"C:\mingw64\bin", @"C:\mingw\bin", @"C:\TDM-GCC-64\bin", @"C:\TDM-GCC-32\bin",
            Path.Combine(ProgramFiles, @"LLVM\bin"), @"C:\tcc", Path.Combine(ProgramFiles, "tcc"),
            @"C:\ProgramData\chocolatey\bin", Path.Combine(UserProfile, @"scoop\shims"),
            Path.Combine(ProgramFiles, @"CodeBlocks\MinGW\bin"), @"C:\Program Files (x86)\CodeBlocks\MinGW\bin",
            @"C:\Program Files (x86)\Dev-Cpp\MinGW64\bin", Path.Combine(ProgramFiles, @"Dev-Cpp\MinGW64\bin"),
        };
        extra.AddRange(Proc.Glob(Path.Combine(ProgramFiles, @"mingw-w64\*\mingw64\bin")));
        extra.AddRange(Proc.Glob(Path.Combine(LocalAppData, @"Microsoft\WinGet\Packages\BrechtSanders.WinLibs*\mingw64\bin")));
        foreach (var (name, kind) in new[] { ("gcc.exe", "gcc"), ("clang.exe", "clang"), ("tcc.exe", "tcc") })
            foreach (var exe in Proc.Find(new[] { name }, extra))
            {
                var v = kind == "tcc" ? Proc.Probe(exe, "-v") : Proc.Probe(exe, "--version");
                if (v == null) continue;
                var m = Regex.Match(v, @"(\d+\.\d+(?:\.\d+)?)");
                yield return new NativeCRunner(exe, m.Success ? m.Groups[1].Value : "?", kind);
                break;
            }
    }

    public static IEnumerable<ILanguageRunner> DetectDotnet()
    {
        var extra = new[] { Path.Combine(ProgramFiles, "dotnet"), Path.Combine(UserProfile, ".dotnet") };
        foreach (var exe in Proc.Find(new[] { "dotnet.exe" }, extra))
        {
            var sdks = Proc.Probe(exe, "--list-sdks");
            if (string.IsNullOrWhiteSpace(sdks)) continue;
            var versions = Regex.Matches(sdks, @"^(\d+)\.(\d+)\.(\d+)", RegexOptions.Multiline)
                .Select(m => m.Value).OrderByDescending(v => Version.TryParse(v, out var x) ? x : new Version()).ToList();
            if (versions.Count == 0) continue;
            yield return new DotnetSdkRunner(exe, versions[0]);
            yield break;
        }
    }
}
