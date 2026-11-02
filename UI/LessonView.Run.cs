using System.Windows;
using System.Windows.Controls;
using EasyCreator.Runners;

namespace EasyCreator;

/// <summary>Run / Check for programming lessons; robotics lessons hand off to the simulator panel (LessonView.Robotics.cs).</summary>
public sealed partial class LessonView
{
    readonly ComboBox _runnerPick = new() { MinWidth = 220, Margin = new Thickness(0, 0, 8, 0), FontSize = 14 };
    CancellationTokenSource? _cts;
    Button? _runBtn, _checkBtn;

    void BuildActions()
    {
        if (IsRobotics) { BuildRoboticsActions(); return; }

        _runBtn = Ui.Button(L.T("▶ Rodar"));
        _runBtn.Click += async (_, _) => await RunOnce();
        _checkBtn = Ui.Button(L.T("✔ Corrigir"), Ui.Accent);
        _checkBtn.Click += async (_, _) => await Check();
        var reset = Ui.Button(L.T("↺ Recomeçar"));
        reset.Click += (_, _) =>
        {
            if (MessageBox.Show(L.T("Voltar ao código inicial da lição? O que você escreveu será perdido."), L.T("Recomeçar"),
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes) _code.Text = _lesson.Starter;
        };
        var with = Ui.Label(L.T("Executar com:"), 13, Ui.TextDim);
        with.VerticalAlignment = VerticalAlignment.Center;
        with.Margin = new Thickness(8, 0, 6, 0);
        _actions.Children.Add(_runBtn);
        if (!_sandbox) _actions.Children.Add(_checkBtn);
        _actions.Children.Add(reset);
        _actions.Children.Add(with);
        _actions.Children.Add(_runnerPick);
        FillRunners();
        _runnerPick.SelectionChanged += (_, _) =>
        {
            if (!_fillingRunners && Runner is { } r) Progress.Current.Runner[_track.Language] = RunnerKey(r);
        };
        RunnerRegistry.Changed += OnRunnersChanged;
    }

    void OnRunnersChanged() => Dispatcher.BeginInvoke(FillRunners);

    bool _fillingRunners;

    /// <summary>The saved preference names the runner's type (stable across languages and tool versions).</summary>
    static string RunnerKey(ILanguageRunner r) => r.GetType().Name;

    /// <summary>Fills the runner list. The saved preference wins whenever that runner is listed: installed tools are
    /// found in the background, so the list is refilled when they appear, and the student's choice must then be
    /// selected (before, the built-in runner picked as a stand-in was kept and even saved over the preference).</summary>
    void FillRunners()
    {
        var current = Runner;
        Progress.Current.Runner.TryGetValue(_track.Language, out var preferred);
        _fillingRunners = true;
        try
        {
            _runnerPick.Items.Clear();
            ComboBoxItem? pick = null, keep = null;
            foreach (var r in RunnerRegistry.For(_track.Language))
            {
                var name = r.IsExternal ? r.DisplayName : L.T(r.DisplayName);
                var item = new ComboBoxItem { Content = r.IsExternal ? L.F("{0}  · instalado", name) : L.F("{0}  · offline", name), Tag = r };
                _runnerPick.Items.Add(item);
                if (preferred != null && (RunnerKey(r) == preferred || r.DisplayName == preferred)) pick = item;
                if (r == current) keep = item;
            }
            _runnerPick.SelectedItem = pick ?? keep ?? (_runnerPick.Items.Count > 0 ? _runnerPick.Items[0] : null);
        }
        finally { _fillingRunners = false; }
    }

    ILanguageRunner? Runner => (_runnerPick.SelectedItem as ComboBoxItem)?.Tag as ILanguageRunner;

    void Busy(bool on)
    {
        if (_runBtn != null) _runBtn.IsEnabled = !on;
        if (_checkBtn != null) _checkBtn.IsEnabled = !on;
        _runnerPick.IsEnabled = !on;
    }

    async Task RunOnce()
    {
        var runner = Runner;
        if (runner == null) { _output.Text = L.T("Nenhum executor disponível para esta linguagem."); return; }
        Busy(true);
        _output.Foreground = Ui.Text;
        _output.Text = L.T("Rodando…");
        _cts = new CancellationTokenSource();
        try
        {
            var r = await runner.RunAsync(_code.Text, _stdin.Text, TimeSpan.FromSeconds(5), _cts.Token);
            _output.Foreground = r.Ok ? Ui.Text : Ui.Bad;
            _output.Text = r.Stdout + (r.Error.Length > 0 ? (r.Stdout.Length > 0 ? "\n" : "") + r.Error : "")
                           + "\n\n" + L.F("— terminou em {0:0} ms", r.Elapsed.TotalMilliseconds);
        }
        finally { Busy(false); }
    }

    async Task Check()
    {
        var runner = Runner;
        _results.Children.Clear();
        if (runner == null) { _results.Children.Add(Ui.Label(L.T("Nenhum executor disponível para esta linguagem."), 13, Ui.Bad)); return; }
        Busy(true);
        _results.Children.Add(Ui.Label(L.T("Corrigindo…"), 13, Ui.TextDim));
        _cts = new CancellationTokenSource();
        try
        {
            var results = await Grader.GradeAsync(runner, _lesson, _code.Text, _cts.Token);
            _results.Children.Clear();
            bool all = Grader.AllPassed(results);
            _results.Children.Add(Ui.Label(all ? L.T("🎉 Tudo certo!") : L.F("{0} de {1} testes passaram", results.Count(t => t.Passed), results.Count),
                16, all ? Ui.Good : Ui.Bad, true));
            foreach (var t in results)
            {
                var p = new StackPanel { Margin = new Thickness(0, 6, 0, 0) };
                p.Children.Add(Ui.Label((t.Passed ? "✓ " : "✗ ") + (t.Hidden ? L.F("Teste {0} (oculto)", t.Index + 1) : L.F("Teste {0}", t.Index + 1)),
                    14, t.Passed ? Ui.Good : Ui.Bad, true));
                if (!t.Passed && !t.Hidden)
                {
                    if (t.Input.Length > 0) p.Children.Add(Mono(L.T("Entrada:") + "\n" + t.Input));
                    p.Children.Add(Mono(L.T("Esperado:") + "\n" + t.Expected));
                    p.Children.Add(Mono(L.T("Seu programa:") + "\n" + t.Actual));
                }
                if (!t.Passed && t.Error.Length > 0) p.Children.Add(Ui.Label(t.Error, 12, Ui.Bad));
                _results.Children.Add(p);
            }
            if (all) Finished();
        }
        finally { Busy(false); }
    }

    static TextBlock Mono(string s) => new()
    {
        Text = s, FontFamily = Ui.Mono, FontSize = 12, Foreground = Ui.TextDim, TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(12, 2, 0, 0),
    };

    void StopRun()
    {
        _cts?.Cancel();
        RunnerRegistry.Changed -= OnRunnersChanged;
        StopRobotics();
    }
}
