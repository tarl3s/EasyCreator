namespace EasyCreator.Runners;

/// <summary>Every runner the app knows. The built-in (offline) runners are always there; installed tools are found
/// in the background the first time anything asks (or when <see cref="DetectExternalAsync"/> is awaited) and then
/// <see cref="Changed"/> fires. Other runners (the C interpreter) are added with <see cref="Register"/>.</summary>
public static class RunnerRegistry
{
    private static readonly object Lock = new();
    private static readonly List<ILanguageRunner> Runners = new() { new PythonRunner(), new JsRunner(), new CSharpRunner() };
    private static Task? _detection;

    /// <summary>Raised (on a background thread) when runners are added — e.g. after external detection finishes.</summary>
    public static event Action? Changed;

    /// <summary>True once the search for installed tools has finished.</summary>
    public static bool ExternalDetectionDone => _detection is { IsCompleted: true };

    /// <summary>All registered runners that are available, offline ones first.</summary>
    public static IReadOnlyList<ILanguageRunner> All
    {
        get
        {
            StartDetection();
            lock (Lock) return Order(Runners).ToList();
        }
    }

    /// <summary>The available runners for a language ("python", "javascript", "c", "csharp"), offline first.</summary>
    public static IReadOnlyList<ILanguageRunner> For(string languageId)
    {
        StartDetection();
        lock (Lock) return Order(Runners.Where(r => Matches(r, languageId))).ToList();
    }

    /// <summary>The runner to use by default: the first offline one, else the first installed one, else null.</summary>
    public static ILanguageRunner? Default(string languageId) => For(languageId).FirstOrDefault();

    /// <summary>Adds a runner (e.g. the C interpreter). A runner of the same type and language replaces the old one.</summary>
    public static void Register(ILanguageRunner runner)
    {
        ArgumentNullException.ThrowIfNull(runner);
        lock (Lock)
        {
            Runners.RemoveAll(r => r.GetType() == runner.GetType() && Matches(r, runner.LanguageId) && !r.IsExternal);
            if (!Runners.Contains(runner)) Runners.Add(runner);
        }
        Changed?.Invoke();
    }

    /// <summary>Searches for installed tools (node, python, gcc/clang/tcc, the .NET SDK) once; later calls return
    /// the same task.</summary>
    public static Task DetectExternalAsync()
    {
        lock (Lock)
            return _detection ??= DetectCore();
    }

    private static void StartDetection()
    {
        if (_detection == null) _ = DetectExternalAsync();
    }

    private static async Task DetectCore()
    {
        List<ILanguageRunner> found;
        try { found = await ExternalRunners.DetectAsync().ConfigureAwait(false); }
        catch { return; }
        if (found.Count == 0) return;
        lock (Lock)
            foreach (var r in found)
                if (!Runners.Any(x => x.IsExternal && x.GetType() == r.GetType()))
                    Runners.Add(r);
        Changed?.Invoke();
    }

    private static bool Matches(ILanguageRunner r, string languageId) =>
        string.Equals(r.LanguageId, languageId, StringComparison.OrdinalIgnoreCase);

    private static IEnumerable<ILanguageRunner> Order(IEnumerable<ILanguageRunner> runners) =>
        runners.Where(r => r.IsAvailable).OrderBy(r => r.IsExternal ? 1 : 0);
}
