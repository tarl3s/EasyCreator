using System.Windows;
using System.Windows.Controls;

namespace EasyCreator;

/// <summary>What every lesson page shares: the left column (theory, safety, task, hints, solution), opening a lesson
/// with the right page for its kind, and the "finished" step.</summary>
public static class LessonText
{
    public static UIElement Open(MainWindow w, Track t, int index) =>
        t.Lessons[index].Kind is "quiz" or "lab" ? new LabView(w, t, index) : new LessonView(w, t, index);

    public static StackPanel Left(MainWindow w, Track track, int index)
    {
        var lesson = track.Lessons[index];
        var p = new StackPanel { Margin = new Thickness(0, 0, 12, 0) };
        var back = Ui.Button(L.T("← Trilha"));
        back.HorizontalAlignment = HorizontalAlignment.Left;
        back.Margin = new Thickness(0, 0, 0, 12);
        back.Click += (_, _) => w.Back();
        p.Children.Add(back);
        p.Children.Add(Ui.Label(L.F("{0} · Lição {1} de {2}", track.Title, index + 1, track.Lessons.Count), 13, Ui.TextDim));
        p.Children.Add(Ui.Label(lesson.Title, 24, Ui.Accent, true));
        if (Progress.Current.Done.Contains(Progress.Key(track, lesson)) && ExerciseView.Button(w, track, lesson) is { } ex)
        {
            ex.Margin = new Thickness(0, 8, 0, 0);
            p.Children.Add(ex);
        }
        p.Children.Add(new Border { Height = 12 });

        if (lesson.Safety.Length > 0)
        {
            var s = new StackPanel();
            s.Children.Add(Ui.Label(L.T("⚠ Segurança"), 15, Ui.Bad, true));
            s.Children.Add(Markdown.Render(lesson.Safety));
            var card = Ui.Card(s, Ui.Bg2, 12);
            card.BorderBrush = Ui.Bad;
            card.Margin = new Thickness(0, 0, 0, 12);
            p.Children.Add(card);
        }

        p.Children.Add(Markdown.Render(lesson.Theory));

        if (lesson.Task.Length > 0)
        {
            var task = new StackPanel();
            task.Children.Add(Ui.Label(L.T("🎯 Desafio"), 16, bold: true));
            task.Children.Add(Markdown.Render(lesson.Task));
            var taskCard = Ui.Card(task, Ui.Bg2);
            taskCard.Margin = new Thickness(0, 12, 0, 12);
            p.Children.Add(taskCard);
        }

        if (lesson.Hints.Count > 0)
        {
            var hints = new StackPanel();
            int shown = 0;
            var hintBtn = Ui.Button(L.T("💡 Mostrar dica"));
            hintBtn.HorizontalAlignment = HorizontalAlignment.Left;
            hintBtn.Click += (_, _) =>
            {
                if (shown >= lesson.Hints.Count) return;
                var h = Ui.Card(Markdown.Render(lesson.Hints[shown++]), Ui.Bg2, 10);
                h.Margin = new Thickness(0, 8, 0, 0);
                hints.Children.Add(h);
                hintBtn.IsEnabled = shown < lesson.Hints.Count;
            };
            p.Children.Add(hintBtn);
            p.Children.Add(hints);
        }

        if (lesson.Solution.Length > 0)
        {
            var sol = Ui.Button(L.T("👀 Ver solução"));
            sol.HorizontalAlignment = HorizontalAlignment.Left;
            sol.Margin = new Thickness(0, 12, 0, 0);
            sol.Click += (_, _) =>
            {
                if (MessageBox.Show(L.T("Tentar mais um pouco ensina mais. Quer mesmo ver a solução?"), L.T("Solução"),
                        MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
                var box = Ui.CodeBox(readOnly: true);
                box.Text = lesson.Solution;
                box.Margin = new Thickness(0, 8, 0, 0);
                p.Children.Add(box);
                sol.IsEnabled = false;
            };
            p.Children.Add(sol);
        }
        return p;
    }

    /// <summary>Marks the lesson done, saves, and offers the next lesson inside <paramref name="into"/>.</summary>
    public static void Finished(MainWindow w, Track track, int index, Panel into)
    {
        Progress.Current.Done.Add(Progress.Key(track, track.Lessons[index]));
        Progress.Current.Save();
        var p = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
        if (index + 1 < track.Lessons.Count)
        {
            var next = Ui.Button(L.T("Próxima lição →"), Ui.Accent);
            next.Click += (_, _) => { w.Back(); w.Go(() => Open(w, track, index + 1)); };
            p.Children.Add(next);
        }
        else p.Children.Add(Ui.Label(L.T("🏆 Você terminou a trilha!"), 15, Ui.Good, true));
        if (ExerciseView.Button(w, track, track.Lessons[index]) is { } ex)
        {
            ex.Margin = new Thickness(10, 0, 0, 0);
            p.Children.Add(ex);
        }
        into.Children.Add(p);
    }
}
