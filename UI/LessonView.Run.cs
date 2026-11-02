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

        _runBtn = Ui.Button("▶ Rodar");
        _runBtn.Click += async (_, _) => await RunOnce();
        _checkBtn = Ui.Button("✔ Corrigir", Ui.Accent);
        _checkBtn.Click += async (_, _) => await Check();
        var reset = Ui.Button("↺ Recomeçar");
        reset.Click += (_, _) =>
        {
            if (MessageBox.Show("Voltar ao código inicial da lição? O que você escreveu será perdido.", "Recomeçar",
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes) _code.Text = _lesson.Starter;
        };
        var with = Ui.Label("Executar com:", 13, Ui.TextDim);
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
            if (Runner is { } r) Progress.Current.Runner[_track.Language] = r.DisplayName;
        };
        RunnerRegistry.Changed += OnRunnersChanged;
    }

    void OnRunnersChanged() => Dispatcher.BeginInvoke(FillRunners);

    void FillRunners()
    {
        var current = Runner;
        Progress.Current.Runner.TryGetValue(_track.Language, out var preferred);
        _runnerPick.Items.Clear();
        foreach (var r in RunnerRegistry.For(_track.Language))
        {
            var item = new ComboBoxItem { Content = r.DisplayName + (r.IsExternal ? "  · instalado" : "  · offline"), Tag = r };
            _runnerPick.Items.Add(item);
            if (r == current || (current == null && r.DisplayName == preferred)) _runnerPick.SelectedItem = item;
        }
        if (_runnerPick.SelectedItem == null && _runnerPick.Items.Count > 0) _runnerPick.SelectedIndex = 0;
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
        if (runner == null) { _output.Text = "Nenhum executor disponível para esta linguagem."; return; }
        Busy(true);
        _output.Text = "Rodando…";
        _cts = new CancellationTokenSource();
        try
        {
            var r = await runner.RunAsync(_code.Text, _stdin.Text, TimeSpan.FromSeconds(5), _cts.Token);
            _output.Foreground = r.Ok ? Ui.Text : Ui.Bad;
            _output.Text = r.Stdout + (r.Error.Length > 0 ? (r.Stdout.Length > 0 ? "\n" : "") + r.Error : "")
                           + $"\n\n— terminou em {r.Elapsed.TotalMilliseconds:0} ms";
        }
        finally { Busy(false); }
    }

    async Task Check()
    {
        var runner = Runner;
        if (runner == null) return;
        Busy(true);
        _results.Children.Clear();
        _results.Children.Add(Ui.Label("Corrigindo…", 13, Ui.TextDim));
        _cts = new CancellationTokenSource();
        try
        {
            var results = await Grader.GradeAsync(runner, _lesson, _code.Text, _cts.Token);
            _results.Children.Clear();
            bool all = Grader.AllPassed(results);
            _results.Children.Add(Ui.Label(all ? "🎉 Tudo certo!" : $"{results.Count(t => t.Passed)} de {results.Count} testes passaram",
                16, all ? Ui.Good : Ui.Bad, true));
            foreach (var t in results)
            {
                var p = new StackPanel { Margin = new Thickness(0, 6, 0, 0) };
                p.Children.Add(Ui.Label($"{(t.Passed ? "✓" : "✗")} Teste {t.Index + 1}{(t.Hidden ? " (oculto)" : "")}",
                    14, t.Passed ? Ui.Good : Ui.Bad, true));
                if (!t.Passed && !t.Hidden)
                {
                    if (t.Input.Length > 0) p.Children.Add(Mono("Entrada:\n" + t.Input));
                    p.Children.Add(Mono("Esperado:\n" + t.Expected));
                    p.Children.Add(Mono("Seu programa:\n" + t.Actual));
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
