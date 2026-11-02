using System.Windows;
using System.Windows.Controls;

namespace EasyCreator;

/// <summary>Quiz and lab lessons: theory on the left; on the right the simulator (lab), its goals, and the questions.
/// The lesson is done when every goal is met and every question is answered right.</summary>
public sealed partial class LabView : Grid, IDisposable
{
    readonly MainWindow _w;
    readonly Track _track;
    readonly Lesson _lesson;
    readonly int _index;
    readonly ILab? _lab;
    readonly StackPanel _goals = new();
    readonly List<(Question q, Func<string> answer, TextBlock feedback)> _answers = new();
    readonly StackPanel _result = new() { Margin = new Thickness(0, 10, 0, 0) };
    bool _labRan;

    public LabView(MainWindow w, Track track, int index)
    {
        _w = w; _track = track; _index = index; _lesson = track.Lessons[index];
        Margin = new Thickness(20);
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star), MinWidth = 320 });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

        Children.Add(new ScrollViewer { Content = LessonText.Left(w, track, index), VerticalScrollBarVisibility = ScrollBarVisibility.Auto });

        var right = new StackPanel();
        if (_lesson.Kind == "lab")
        {
            _lab = LabRegistry.Create(_lesson.Lab);
            if (_lab == null) right.Children.Add(Ui.Label($"Simulador \"{_lesson.Lab}\" não encontrado.", 14, Ui.Bad));
            else
            {
                _lab.Load(_lesson.LabParams);
                _lab.Finished += () => Dispatcher.BeginInvoke(() => { _labRan = true; ShowGoals(); });
                var view = Ui.Card(_lab.View, Ui.Bg1, 12);
                view.Margin = new Thickness(0, 0, 0, 12);
                right.Children.Add(view);
            }
            if (_lesson.Goals.Count > 0)
            {
                var g = new StackPanel();
                g.Children.Add(Ui.Label("🎯 Metas do experimento", 16, bold: true));
                g.Children.Add(_goals);
                right.Children.Add(Ui.Card(g, Ui.Bg2));
                ShowGoals();
            }
        }
        for (int i = 0; i < _lesson.Questions.Count; i++) right.Children.Add(QuestionCard(_lesson.Questions[i], i));

        var check = Ui.Button("✔ Verificar", Ui.Accent);
        check.HorizontalAlignment = HorizontalAlignment.Left;
        check.Margin = new Thickness(0, 12, 0, 0);
        check.Click += (_, _) => Verify();
        right.Children.Add(check);
        right.Children.Add(_result);

        var scroll = new ScrollViewer { Content = right, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        Grid.SetColumn(scroll, 2);
        Children.Add(scroll);
    }

    void ShowGoals()
    {
        _goals.Children.Clear();
        foreach (var g in _lesson.Goals)
        {
            bool met = _labRan && _lab != null && g.IsMet(_lab.Metrics);
            string value = _labRan && _lab != null && _lab.Metrics.TryGetValue(g.Metric, out var v) ? $"  (agora: {v:G4})" : "";
            _goals.Children.Add(Ui.Label($"{(met ? "✓" : "○")} {g.Description}{value}", 14, met ? Ui.Good : Ui.Text));
        }
    }

    UIElement QuestionCard(Question q, int n)
    {
        var p = new StackPanel();
        p.Children.Add(Ui.Label($"Pergunta {n + 1}", 13, Ui.TextDim, true));
        p.Children.Add(Markdown.Render(q.Prompt));
        var feedback = Ui.Label("", 13);
        Func<string> answer;
        if (q.Kind == "number")
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
        else
        {
            var group = "q" + n + "_" + GetHashCode();
            var radios = new List<RadioButton>();
            for (int i = 0; i < q.Options.Count; i++)
            {
                var r = new RadioButton { Content = new TextBlock { Text = q.Options[i], TextWrapping = TextWrapping.Wrap }, GroupName = group,
                    Foreground = Ui.Text, FontSize = 14, Margin = new Thickness(0, 4, 0, 0), Tag = i.ToString() };
                radios.Add(r);
                p.Children.Add(r);
            }
            answer = () => radios.FirstOrDefault(r => r.IsChecked == true)?.Tag as string ?? "";
        }
        p.Children.Add(feedback);
        _answers.Add((q, answer, feedback));
        var card = Ui.Card(p);
        card.Margin = new Thickness(0, 12, 0, 0);
        return card;
    }

    void Verify()
    {
        int right = 0;
        foreach (var (q, answer, fb) in _answers)
        {
            var given = answer();
            bool ok = q.IsCorrect(given);
            if (ok) right++;
            fb.Foreground = ok ? Ui.Good : Ui.Bad;
            fb.Margin = new Thickness(0, 6, 0, 0);
            fb.Text = given.Length == 0 ? "Responda esta pergunta." :
                (ok ? "✓ Correto! " : "✗ Ainda não. ") + (ok ? q.Explanation : "Revise a teoria e tente de novo.");
        }
        bool goalsOk = _lesson.Goals.Count == 0 || (_labRan && _lab != null && _lesson.Goals.All(g => g.IsMet(_lab.Metrics)));
        ShowGoals();
        _result.Children.Clear();
        bool all = goalsOk && right == _answers.Count;
        var msg = all ? "🎉 Tudo certo!" :
            !goalsOk ? (_labRan ? "Algumas metas do experimento ainda não foram cumpridas." : "Rode o experimento primeiro.")
            : $"{right} de {_answers.Count} respostas certas.";
        _result.Children.Add(Ui.Label(msg, 16, all ? Ui.Good : Ui.Bad, true));
        if (all) LessonText.Finished(_w, _track, _index, _result);
    }

    public void Dispose()
    {
        _lab?.Stop();
        Progress.Current.Save();
    }
}
