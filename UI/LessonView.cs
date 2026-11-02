using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EasyCreator;

/// <summary>One lesson: theory/task/hints on the left, code editor + output (and the simulator for robotics) on the right.
/// Running and grading live in LessonView.Run.cs.</summary>
public sealed partial class LessonView : Grid, IDisposable
{
    readonly MainWindow _w;
    readonly Track _track;
    readonly Lesson _lesson;
    readonly int _index;
    readonly TextBox _code = Ui.CodeBox();
    readonly TextBox _stdin = Ui.CodeBox();
    readonly TextBox _output = Ui.CodeBox(readOnly: true);
    readonly StackPanel _results = new();
    readonly StackPanel _actions = new() { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
    readonly Grid _right = new();
    int _hintsShown;

    bool IsRobotics => _track.Area == "robotics";

    public LessonView(MainWindow w, Track track, int index)
    {
        _w = w; _track = track; _index = index; _lesson = track.Lessons[index];
        Margin = new Thickness(20);
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star), MinWidth = 320 });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
        ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });

        var left = BuildLeft();
        Children.Add(left);

        Grid.SetColumn(_right, 2);
        Children.Add(_right);
        BuildRight();

        _code.Text = Progress.Current.Code.TryGetValue(Progress.Key(_track, _lesson), out var saved) ? saved : _lesson.Starter;
        _code.PreviewKeyDown += CodeKeys;
        _code.TextChanged += (_, _) => Progress.Current.Code[Progress.Key(_track, _lesson)] = _code.Text;
    }

    UIElement BuildLeft()
    {
        var p = new StackPanel { Margin = new Thickness(0, 0, 12, 0) };
        var back = Ui.Button("← Trilha");
        back.HorizontalAlignment = HorizontalAlignment.Left;
        back.Margin = new Thickness(0, 0, 0, 12);
        back.Click += (_, _) => _w.Back();
        p.Children.Add(back);
        p.Children.Add(Ui.Label($"{_track.Title} · Lição {_index + 1} de {_track.Lessons.Count}", 13, Ui.TextDim));
        p.Children.Add(Ui.Label(_lesson.Title, 24, Ui.AreaBrush(_track.Area), true));
        p.Children.Add(new Border { Height = 12 });
        p.Children.Add(Markdown.Render(_lesson.Theory));

        var task = new StackPanel();
        task.Children.Add(Ui.Label("🎯 Desafio", 16, bold: true));
        task.Children.Add(Markdown.Render(_lesson.Task));
        var taskCard = Ui.Card(task, Ui.Bg2);
        taskCard.Margin = new Thickness(0, 12, 0, 12);
        p.Children.Add(taskCard);

        var hints = new StackPanel();
        var hintBtn = Ui.Button("💡 Mostrar dica");
        hintBtn.HorizontalAlignment = HorizontalAlignment.Left;
        hintBtn.IsEnabled = _lesson.Hints.Count > 0;
        hintBtn.Click += (_, _) =>
        {
            if (_hintsShown >= _lesson.Hints.Count) return;
            var h = Ui.Card(Markdown.Render(_lesson.Hints[_hintsShown]), Ui.Bg2, 10);
            h.Margin = new Thickness(0, 8, 0, 0);
            hints.Children.Add(h);
            _hintsShown++;
            if (_hintsShown >= _lesson.Hints.Count) hintBtn.IsEnabled = false;
        };
        p.Children.Add(hintBtn);
        p.Children.Add(hints);

        var sol = Ui.Button("👀 Ver solução");
        sol.HorizontalAlignment = HorizontalAlignment.Left;
        sol.Margin = new Thickness(0, 12, 0, 0);
        sol.Click += (_, _) =>
        {
            if (MessageBox.Show("Tentar mais um pouco ensina mais. Quer mesmo ver a solução?", "Solução",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
            var box = Ui.CodeBox(readOnly: true);
            box.Text = _lesson.Solution;
            box.Margin = new Thickness(0, 8, 0, 0);
            p.Children.Add(box);
            sol.IsEnabled = false;
        };
        p.Children.Add(sol);

        return new ScrollViewer { Content = p, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    }

    void BuildRight()
    {
        _right.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });                              // actions
        _right.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3, GridUnitType.Star) });         // code
        _right.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });                              // simulator (robotics)
        _right.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });         // output / input

        _right.Children.Add(_actions);
        Grid.SetRow(_code, 1);
        _right.Children.Add(_code);

        var bottom = new Grid { Margin = new Thickness(0, 10, 0, 0) };
        bottom.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        bottom.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
        bottom.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var outPanel = Titled(IsRobotics ? "Monitor Serial" : "Saída", _output);
        var resultsScroll = new ScrollViewer { Content = _results, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        var tabs = new Grid();
        tabs.RowDefinitions.Add(new RowDefinition());
        tabs.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        tabs.Children.Add(outPanel);
        bottom.Children.Add(tabs);
        UIElement side = IsRobotics ? Titled("Correção", resultsScroll) : Stacked(Titled("Entrada (stdin)", _stdin), Titled("Correção", resultsScroll));
        Grid.SetColumn(side, 2);
        bottom.Children.Add(side);
        Grid.SetRow(bottom, 3);
        _right.Children.Add(bottom);

        BuildActions();
    }

    static Grid Titled(string title, UIElement body)
    {
        var g = new Grid();
        g.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        g.RowDefinitions.Add(new RowDefinition());
        g.Children.Add(Ui.Label(title, 13, Ui.TextDim, true));
        Grid.SetRow(body, 1);
        if (body is FrameworkElement fe) fe.Margin = new Thickness(0, 4, 0, 0);
        g.Children.Add(body);
        return g;
    }

    static Grid Stacked(UIElement top, UIElement bottom)
    {
        var g = new Grid();
        g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
        g.Children.Add(top);
        Grid.SetRow(bottom, 1);
        if (bottom is FrameworkElement fe) fe.Margin = new Thickness(0, 8, 0, 0);
        g.Children.Add(bottom);
        return g;
    }

    /// <summary>Tab indents with 4 spaces; Enter keeps the indentation of the current line.</summary>
    void CodeKeys(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Tab)
        {
            int at = _code.CaretIndex;
            _code.SelectedText = "";
            _code.Text = _code.Text.Insert(at, "    ");
            _code.CaretIndex = at + 4;
            e.Handled = true;
        }
        else if (e.Key == Key.Enter)
        {
            int at = _code.CaretIndex;
            int start = _code.Text.LastIndexOf('\n', Math.Max(0, at - 1)) + 1;
            int n = 0;
            while (start + n < at && _code.Text[start + n] == ' ') n++;
            string indent = new(' ', n);
            if (at > 0 && (_code.Text[at - 1] == '{' || _code.Text[at - 1] == ':')) indent += "    ";
            _code.SelectedText = "";
            _code.Text = _code.Text.Insert(at, "\n" + indent);
            _code.CaretIndex = at + 1 + indent.Length;
            e.Handled = true;
        }
    }

    void Finished()
    {
        Progress.Current.Done.Add(Progress.Key(_track, _lesson));
        Progress.Current.Save();
        var p = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
        if (_index + 1 < _track.Lessons.Count)
        {
            var next = Ui.Button("Próxima lição →", Ui.Good);
            next.Click += (_, _) => { _w.Back(); _w.Go(() => new LessonView(_w, _track, _index + 1)); };
            p.Children.Add(next);
        }
        else p.Children.Add(Ui.Label("🏆 Você terminou a trilha!", 15, Ui.Good, true));
        _results.Children.Add(p);
    }

    public void Dispose()
    {
        StopRun();
        Progress.Current.Save();
    }
}
