using EasyCreator.Runners;
using System.Windows;
using System.Windows.Controls;

namespace EasyCreator;

/// <summary>The 💡 exercise generator page of a finished lesson: pick a difficulty, get a new exercise on each click.</summary>
public sealed class ExerciseView : Grid
{
    static Difficulty _difficulty = Difficulty.Medium;
    static readonly (Difficulty d, string pt)[] Levels =
        { (Difficulty.Easy, "Fácil"), (Difficulty.Medium, "Médio"), (Difficulty.Hard, "Difícil"), (Difficulty.Genius, "Extremo / Genial") };

    readonly MainWindow _w;
    readonly Track _track;
    readonly Lesson _lesson;
    readonly StackPanel _levelRow = new() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 12, 0, 0) };
    readonly StackPanel _body = new();
    CancellationTokenSource? _cts;

    /// <summary>The 💡 button shown next to a finished lesson (null when the lesson has no generator yet).</summary>
    public static Button? Button(MainWindow w, Track track, Lesson lesson)
    {
        if (!ExerciseRegistry.Has(lesson)) return null;
        var b = Ui.Button(L.T("💡 Gerar exercício"), Ui.Accent, Ui.OnAccent);
        b.HorizontalAlignment = HorizontalAlignment.Left;
        b.ToolTip = L.T("Um exercício novo sobre esta lição, a cada clique.");
        b.Click += (_, _) => w.Go(() => new ExerciseView(w, track, lesson));
        return b;
    }

    public ExerciseView(MainWindow w, Track track, Lesson lesson)
    {
        _w = w; _track = track; _lesson = lesson;
        Background = Ui.Bg0;
        var p = new StackPanel { Margin = new Thickness(24), MaxWidth = 1000 };
        var back = Ui.Button(L.T("← Lição"));
        back.HorizontalAlignment = HorizontalAlignment.Left;
        back.Click += (_, _) => { _cts?.Cancel(); w.Back(); };
        p.Children.Add(back);
        p.Children.Add(Ui.Label(L.F("💡 Exercício · {0}", lesson.Title), 22, Ui.Accent, true));
        p.Children.Add(_levelRow);
        p.Children.Add(_body);
        Children.Add(new ScrollViewer { Content = p, VerticalScrollBarVisibility = ScrollBarVisibility.Auto });
        BuildLevels();
        Generate();
    }

    void BuildLevels()
    {
        _levelRow.Children.Clear();
        foreach (var (d, pt) in Levels)
        {
            var b = Ui.Button(L.T(pt), d == _difficulty ? Ui.Accent : null, d == _difficulty ? Ui.OnAccent : null);
            b.Margin = new Thickness(0, 0, 8, 0);
            b.Click += (_, _) => { _difficulty = d; BuildLevels(); Generate(); };
            _levelRow.Children.Add(b);
        }
        var again = Ui.Button(L.T("🔄 Outro exercício"));
        again.Margin = new Thickness(16, 0, 0, 0);
        again.Click += (_, _) => Generate();
        _levelRow.Children.Add(again);
    }

    void Generate()
    {
        _cts?.Cancel();
        _body.Children.Clear();
        Exercise? ex;
        try { ex = ExerciseRegistry.Next(_lesson, _difficulty); }
        catch (Exception e) { _body.Children.Add(Ui.Label(L.F("Erro ao gerar o exercício: {0}", e.Message), 14, Ui.Bad)); return; }
        if (ex == null) return;

        var card = new StackPanel();
        card.Children.Add(Markdown.Render(ex.Prompt));
        if (ex.Code.Length > 0)
        {
            var code = Ui.CodeBox(readOnly: true);
            code.Text = ex.Code;
            code.Margin = new Thickness(0, 6, 0, 6);
            card.Children.Add(code);
        }
        var c = Ui.Card(card);
        c.Margin = new Thickness(0, 14, 0, 0);
        _body.Children.Add(c);

        if (ex.IsCode) BuildCode(ex); else BuildAnswer(ex);
    }

    void BuildAnswer(Exercise ex)
    {
        var q = ex.Question!;
        var p = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };
        Func<string> answer;
        if (q.Kind == "choice")
        {
            var group = "ex" + GetHashCode();
            var radios = q.Options.Select((o, i) => new RadioButton
            {
                Content = new TextBlock { Text = o, TextWrapping = TextWrapping.Wrap }, GroupName = group,
                Foreground = Ui.Text, FontSize = 14, Margin = new Thickness(0, 4, 0, 0), Tag = i.ToString(),
            }).ToList();
            radios.ForEach(r => p.Children.Add(r));
            answer = () => radios.FirstOrDefault(r => r.IsChecked == true)?.Tag as string ?? "";
        }
        else if (q.Kind == "text")
        {
            p.Children.Add(Ui.Label(L.T("O que o programa escreve?"), 13, Ui.TextDim));
            var box = Ui.CodeBox();
            box.MinHeight = 80;
            p.Children.Add(box);
            answer = () => box.Text;
        }
        else
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            var box = new TextBox
            {
                Width = 160, FontSize = 15, Background = Ui.Code, Foreground = Ui.Text, CaretBrush = Ui.Text,
                BorderBrush = Ui.Border, Padding = new Thickness(6, 4, 6, 4),
            };
            row.Children.Add(box);
            if (q.Unit.Length > 0) row.Children.Add(new TextBlock { Text = q.Unit, Foreground = Ui.TextDim, FontSize = 15, Margin = new Thickness(8, 4, 0, 0) });
            p.Children.Add(row);
            answer = () => box.Text;
        }

        var feedback = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
        var check = Ui.Button(L.T("✔ Verificar"), Ui.Accent, Ui.OnAccent);
        var show = Ui.Button(L.T("👀 Ver solução"));
        show.Margin = new Thickness(8, 0, 0, 0);
        check.Click += (_, _) =>
        {
            var given = answer();
            feedback.Children.Clear();
            if (given.Trim().Length == 0) { feedback.Children.Add(Ui.Label(L.T("Responda primeiro."), 14, Ui.Bad)); return; }
            bool ok = q.IsCorrect(given);
            feedback.Children.Add(Ui.Label(ok ? L.T("✓ Correto!") : L.T("✗ Ainda não. Tente de novo ou veja a solução."), 15, ok ? Ui.Good : Ui.Bad, true));
            if (ok) feedback.Children.Add(Markdown.Render(ex.Explanation));
        };
        show.Click += (_, _) =>
        {
            feedback.Children.Clear();
            var right = q.Kind == "choice" && int.TryParse(q.Answer, out var i) && i < q.Options.Count ? q.Options[i]
                : q.Kind == "number" && Question.TryNum(q.Answer, out var v) ? Ex.N(v, 4) + (q.Unit.Length > 0 ? " " + q.Unit : "")
                : q.Answer;
            feedback.Children.Add(Ui.Label(L.T("Resposta:"), 14, Ui.TextDim, true));
            if (q.Kind == "text") { var b = Ui.CodeBox(readOnly: true); b.Text = right; feedback.Children.Add(b); }
            else feedback.Children.Add(Ui.Label(right, 15, Ui.Accent, true));
            feedback.Children.Add(Markdown.Render(ex.Explanation));
        };
        buttons.Children.Add(check);
        buttons.Children.Add(show);
        p.Children.Add(buttons);
        p.Children.Add(feedback);
        _body.Children.Add(p);
    }

    void BuildCode(Exercise ex)
    {
        var runner = RunnerRegistry.Default(_track.Language);
        var p = new StackPanel { Margin = new Thickness(0, 12, 0, 0) };
        var code = Ui.CodeBox();
        code.MinHeight = 220;
        code.Text = ex.Starter;
        p.Children.Add(code);
        var results = new StackPanel { Margin = new Thickness(0, 8, 0, 0) };
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
        var check = Ui.Button(L.T("✔ Corrigir"), Ui.Accent, Ui.OnAccent);
        var show = Ui.Button(L.T("👀 Ver solução"));
        show.Margin = new Thickness(8, 0, 0, 0);
        check.Click += async (_, _) =>
        {
            results.Children.Clear();
            if (runner == null) { results.Children.Add(Ui.Label(L.T("Nenhum executor disponível para esta linguagem."), 13, Ui.Bad)); return; }
            check.IsEnabled = false;
            results.Children.Add(Ui.Label(L.T("Corrigindo…"), 13, Ui.TextDim));
            _cts = new CancellationTokenSource();
            try
            {
                var r = await Grader.GradeAsync(runner, new Lesson { Tests = ex.Tests }, code.Text, _cts.Token);
                results.Children.Clear();
                bool all = Grader.AllPassed(r);
                results.Children.Add(Ui.Label(all ? L.T("🎉 Tudo certo!") : L.F("{0} de {1} testes passaram", r.Count(t => t.Passed), r.Count), 16, all ? Ui.Good : Ui.Bad, true));
                foreach (var t in r.Where(t => !t.Passed).Take(1))
                {
                    if (!t.Hidden)
                    {
                        if (t.Input.Length > 0) results.Children.Add(Mono(L.T("Entrada:") + "\n" + t.Input));
                        results.Children.Add(Mono(L.T("Esperado:") + "\n" + t.Expected));
                        results.Children.Add(Mono(L.T("Seu programa:") + "\n" + t.Actual));
                    }
                    if (t.Error.Length > 0) results.Children.Add(Ui.Label(t.Error, 12, Ui.Bad));
                }
                if (all) results.Children.Add(Markdown.Render(ex.Explanation));
            }
            finally { check.IsEnabled = true; }
        };
        show.Click += (_, _) =>
        {
            if (MessageBox.Show(L.T("Tentar mais um pouco ensina mais. Quer mesmo ver a solução?"), L.T("Solução"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            results.Children.Clear();
            var b = Ui.CodeBox(readOnly: true);
            b.Text = ex.Solution;
            results.Children.Add(b);
            results.Children.Add(Markdown.Render(ex.Explanation));
        };
        buttons.Children.Add(check);
        buttons.Children.Add(show);
        p.Children.Add(buttons);
        p.Children.Add(results);
        _body.Children.Add(p);
    }

    static TextBlock Mono(string s) => new()
    {
        Text = s, FontFamily = Ui.Mono, FontSize = 12, Foreground = Ui.TextDim, TextWrapping = TextWrapping.Wrap,
        Margin = new Thickness(12, 2, 0, 0),
    };
}
