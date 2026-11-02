using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace EasyCreator;

/// <summary>--snap: opens the real window, visits key pages (a simulation runs for a moment) and saves
/// %TEMP%\easycreator_snap_*.png, then exits. Used for visual review without capturing the screen.</summary>
public static partial class TestEntry
{
    static async void Snapshots(MainWindow w)
    {
        async Task Shot(string name, double waitMs = 400)
        {
            await Task.Delay((int)waitMs);
            w.UpdateLayout();
            var root = (FrameworkElement)w.Content;
            var bmp = new RenderTargetBitmap((int)root.ActualWidth, (int)root.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            var bg = new DrawingVisual();
            using (var dc = bg.RenderOpen()) dc.DrawRectangle(Ui.Bg0, null, new Rect(0, 0, root.ActualWidth, root.ActualHeight));
            bmp.Render(bg);
            bmp.Render(root);
            var enc = new PngBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(bmp));
            using var f = File.Create(Path.Combine(Path.GetTempPath(), $"easycreator_snap_{name}.png"));
            enc.Save(f);
        }
        try
        {
            await Shot("home");
            w.Go(() => Pages.Area(w, "programming"));
            await Shot("programming");
            var py = ContentStore.Find("python")!;
            w.Go(() => Pages.TrackPage(w, py));
            await Shot("track");
            w.Go(() => new LessonView(w, py, 0));
            await Shot("lesson_python");
            var ard = ContentStore.InArea("robotics").First();
            foreach (int i in new[] { 0, 6, 9 })
            {
                var view = new LessonView(w, ard, i);
                w.Go(() => view);
                view.SnapStartSolution();
                await Shot($"robotics_{i + 1}", 2500);
            }
            w.Go(() => Sandbox.Page(w));
            await Shot("sandbox");
            Sandbox.SnapOpenRobotics(w);
            await Shot("sandbox_robotics", 1500);
            // Every lab lesson (or only the area given after --snap-labs), with its solution inputs already run.
            if (Environment.GetCommandLineArgs().Contains("--snap-labs"))
            {
                var args = Environment.GetCommandLineArgs();
                int at = Array.IndexOf(args, "--snap-labs");
                string? only = at + 1 < args.Length ? args[at + 1] : null;
                foreach (var t in ContentStore.Tracks.Where(t => only == null || t.Area == only))
                    for (int i = 0; i < t.Lessons.Count; i++)
                    {
                        if (t.Lessons[i].Kind != "lab") continue;
                        var view = new LabView(w, t, i);
                        w.Go(() => view);
                        view.SnapSolve();
                        await Shot($"lab_{t.Id}_{i + 1}", 1500);
                    }
            }
        }
        catch (Exception ex) { File.WriteAllText(Path.Combine(Path.GetTempPath(), "easycreator_snap_error.txt"), ex.ToString()); }
        Application.Current.Shutdown();
    }
}

public sealed partial class LessonView
{
    /// <summary>Snapshot helper: loads the solution and starts the live simulation.</summary>
    internal void SnapStartSolution()
    {
        _code.Text = _lesson.Solution;
        if (IsRobotics) StartSim();
    }
}

public sealed partial class LabView
{
    /// <summary>Snapshot helper: applies the solution inputs and runs the experiment.</summary>
    internal void SnapSolve()
    {
        if (_lab == null) return;
        _lab.SetInputs(_lesson.SolutionInputs);
        _lab.RunToEnd();
        _labRan = true;
        ShowGoals();
    }
}
