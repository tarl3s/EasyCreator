using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>Home, area and track pages. The lesson page lives in LessonView.</summary>
public static class Pages
{
    static Grid Frame(MainWindow w, string title, string subtitle, UIElement body, bool back = true)
    {
        var grid = new Grid { Margin = new Thickness(32, 24, 32, 24) };
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new RowDefinition());
        var head = new StackPanel { Margin = new Thickness(0, 0, 0, 20) };
        if (back)
        {
            var b = Ui.Button("← Voltar");
            b.HorizontalAlignment = HorizontalAlignment.Left;
            b.Margin = new Thickness(0, 0, 0, 12);
            b.Click += (_, _) => w.Back();
            head.Children.Add(b);
        }
        head.Children.Add(Ui.Label(title, 28, bold: true));
        if (subtitle.Length > 0) head.Children.Add(Ui.Label(subtitle, 15, Ui.TextDim));
        grid.Children.Add(head);
        var scroll = new ScrollViewer { Content = body, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        Grid.SetRow(scroll, 1);
        grid.Children.Add(scroll);
        return grid;
    }

    public static UIElement Home(MainWindow w)
    {
        var body = new StackPanel();
        var cards = Areas.All.Select(a => AreaCard(w, a.Id, $"{a.Icon} {a.Title}", a.Description)).ToArray();
        for (int i = 0; i < cards.Length; i += 3)
        {
            var row = Columns(cards.Skip(i).Take(3).Cast<FrameworkElement>().ToArray());
            row.Margin = new Thickness(0, 0, 0, 16);
            body.Children.Add(row);
        }
        if (ContentStore.Errors.Count > 0)
            body.Children.Add(Ui.Label("⚠ " + string.Join("\n", ContentStore.Errors), 13, Ui.Bad));
        return Frame(w, "EasyCreator", "Aprenda programação, robótica e engenharia, um desafio de cada vez.", body, back: false);
    }

    static Border AreaCard(MainWindow w, string area, string title, string text)
    {
        var tracks = ContentStore.InArea(area).ToList();
        int done = tracks.Sum(t => Progress.Current.DoneCount(t)), total = tracks.Sum(t => t.Lessons.Count);
        var p = new StackPanel();
        p.Children.Add(Ui.Label(title, 24, Ui.AreaBrush(area), true));
        p.Children.Add(new Border { Height = 10 });
        p.Children.Add(Ui.Label(text, 15, Ui.TextDim));
        p.Children.Add(new Border { Height = 16 });
        p.Children.Add(Bar(done, total, Ui.AreaBrush(area)));
        p.Children.Add(Ui.Label($"{done} de {total} desafios", 13, Ui.TextDim));
        var card = Clickable(p, () => w.Go(() => Area(w, area)));
        card.MinHeight = 200;
        return card;
    }

    public static UIElement Area(MainWindow w, string area)
    {
        var list = new StackPanel();
        foreach (var t in ContentStore.InArea(area))
        {
            var p = new StackPanel();
            p.Children.Add(Ui.Label(t.Title, 20, bold: true));
            p.Children.Add(Ui.Label(t.Description, 14, Ui.TextDim));
            p.Children.Add(new Border { Height = 10 });
            int done = Progress.Current.DoneCount(t);
            p.Children.Add(Bar(done, t.Lessons.Count, Ui.AreaBrush(area)));
            p.Children.Add(Ui.Label($"{done} de {t.Lessons.Count} lições", 13, Ui.TextDim));
            var card = Clickable(p, () => w.Go(() => TrackPage(w, t)));
            card.Margin = new Thickness(0, 0, 0, 12);
            list.Children.Add(card);
        }
        if (list.Children.Count == 0) list.Children.Add(Ui.Label("Nenhuma trilha encontrada.", 15, Ui.TextDim));
        var info = Areas.Get(area);
        return Frame(w, $"{info.Icon} {info.Title}", info.Description, list);
    }

    static readonly string[] LevelNames = { "", "Iniciante", "Básico", "Intermediário", "Avançado", "Faculdade" };

    public static UIElement TrackPage(MainWindow w, Track t)
    {
        var list = new StackPanel();
        for (int i = 0; i < t.Lessons.Count; i++)
        {
            var l = t.Lessons[i];
            bool done = Progress.Current.IsDone(t, l), open = Progress.Current.IsUnlocked(t, i);
            var row = new DockPanel();
            var badge = Ui.Label(done ? "✓" : open ? (i + 1).ToString() : "🔒", 18, done ? Ui.OnAccent : Ui.Text, true);
            badge.TextAlignment = TextAlignment.Center;
            badge.VerticalAlignment = VerticalAlignment.Center;
            row.Children.Add(new Border
            {
                Width = 40, Height = 40, CornerRadius = new CornerRadius(20), Child = badge,
                Background = done ? Ui.Good : Ui.Bg3, Margin = new Thickness(0, 0, 14, 0),
            });
            var info = new StackPanel();
            info.Children.Add(Ui.Label(l.Title, 17, open ? Ui.Text : Ui.TextDim, true));
            info.Children.Add(Ui.Label($"Nível {l.Level} · {LevelNames[Math.Clamp(l.Level, 1, 5)]}", 13, Ui.TextDim));
            row.Children.Add(info);
            int index = i;
            var card = open ? Clickable(row, () => w.Go(() => LessonText.Open(w, t, index))) : Ui.Card(row);
            card.Margin = new Thickness(0, 0, 0, 10);
            if (!open) card.Opacity = 0.6;
            list.Children.Add(card);
        }
        return Frame(w, t.Title, t.Description, list);
    }

    static Border Clickable(UIElement child, Action onClick)
    {
        var card = Ui.Card(child);
        card.Cursor = System.Windows.Input.Cursors.Hand;
        card.MouseEnter += (_, _) => card.Background = Ui.Bg2;
        card.MouseLeave += (_, _) => card.Background = Ui.Bg1;
        card.MouseLeftButtonUp += (_, _) => onClick();
        return card;
    }

    public static Border Bar(int value, int max, Brush fill)
    {
        var inner = new Border { Background = fill, CornerRadius = new CornerRadius(4), HorizontalAlignment = HorizontalAlignment.Left };
        var outer = new Border { Background = Ui.Bg3, CornerRadius = new CornerRadius(4), Height = 8, Child = inner, Margin = new Thickness(0, 0, 0, 6) };
        outer.SizeChanged += (_, _) => inner.Width = max == 0 ? 0 : outer.ActualWidth * value / max;
        return outer;
    }

    /// <summary>Equal-width columns with a gap between them.</summary>
    public static Grid Columns(params FrameworkElement[] items)
    {
        var g = new Grid();
        for (int i = 0; i < items.Length; i++)
        {
            g.ColumnDefinitions.Add(new ColumnDefinition());
            items[i].Margin = new Thickness(i == 0 ? 0 : 8, 0, i == items.Length - 1 ? 0 : 8, 0);
            Grid.SetColumn(items[i], i);
            g.Children.Add(items[i]);
        }
        return g;
    }
}
