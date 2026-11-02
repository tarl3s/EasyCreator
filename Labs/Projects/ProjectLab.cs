using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace EasyCreator.ProjectLabs;

/// <summary>One control of a project lab: a number (slider) or a choice (combo box). Lessons set its starting value
/// with a labParams key equal to <see cref="Id"/>, its range with "id.min"/"id.max"/"id.step", and which controls are
/// visible with "controls" (comma list).</summary>
public sealed class LabParam
{
    public string Id = "", Label = "", Unit = "";
    public double Min, Max = 1, Step = 0.1, Value;
    public (string Value, string Label)[]? Choices;
    public string Choice = "";
    public Func<double, string>? Format;
    public bool Visible = true;
    public string Tip = "";
    internal Action? RefreshUi;

    public bool IsChoice => Choices != null;

    public string Shown => IsChoice
        ? Choices!.FirstOrDefault(c => c.Value == Choice).Label ?? Choice
        : (Format?.Invoke(Value) ?? Fmt.Num(Value, Step)) + (Unit.Length > 0 && Format == null ? " " + Unit : "");

    /// <summary>Accepts "3,5" or "3.5" for numbers and either the value or the label of a choice. Numbers are clamped
    /// to the range and snapped to the step.</summary>
    public void Set(string text)
    {
        text = (text ?? "").Trim();
        if (IsChoice)
        {
            var c = Choices!.FirstOrDefault(c => string.Equals(c.Value, text, StringComparison.OrdinalIgnoreCase)
                                                 || string.Equals(c.Label, text, StringComparison.OrdinalIgnoreCase));
            if (c.Value != null) Choice = c.Value;
            return;
        }
        if (Question.TryNum(text, out var v)) SetNum(v);
    }

    public void SetNum(double v)
    {
        if (double.IsNaN(v)) return;
        v = Math.Clamp(v, Min, Max);
        if (Step > 0) v = Math.Round((v - Min) / Step) * Step + Min;
        Value = Math.Clamp(Math.Round(v, 9), Min, Max);
    }
}

/// <summary>Number formatting shared by the project labs (Brazilian style: decimal comma).</summary>
public static class Fmt
{
    static readonly CultureInfo Br = CultureInfo.GetCultureInfo("pt-BR");

    public static string Num(double v, double step = 0.01)
    {
        int dec = step >= 1 ? 0 : step >= 0.1 ? 1 : step >= 0.01 ? 2 : 3;
        return v.ToString("N" + dec, Br);
    }

    public static string F(double v, int dec) =>
        double.IsNaN(v) || double.IsInfinity(v) ? "—" : (Math.Round(v, Math.Clamp(dec, 0, 15)) + 0.0).ToString("N" + dec, Br); // + 0.0: no "-0"

    /// <summary>Mass in kg shown in g below 1 kg, in t above 10 t.</summary>
    public static string Mass(double kg) =>
        kg < 1 ? F(kg * 1000, kg < 0.1 ? 1 : 0) + " g" : kg < 10000 ? F(kg, kg < 10 ? 2 : 0) + " kg" : F(kg / 1000, 1) + " t";

    /// <summary>Length in m shown in km above 10 km.</summary>
    public static string Dist(double m) => Math.Abs(m) < 10000 ? F(m, Math.Abs(m) < 100 ? 1 : 0) + " m" : F(m / 1000, 1) + " km";
}

/// <summary>A thin dark slider (click or drag; mouse wheel steps). WPF's default slider looks out of place on the dark theme.</summary>
public sealed class FlatSlider : FrameworkElement
{
    public double Min, Max = 1, Value;
    public event Action<double>? Dragged;

    public FlatSlider()
    {
        Height = 22;
        Cursor = Cursors.Hand;
        Focusable = false;
    }

    protected override void OnRender(DrawingContext dc)
    {
        double w = ActualWidth, h = ActualHeight, y = h / 2, r = 7;
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, w, h));
        double t = Max > Min ? Math.Clamp((Value - Min) / (Max - Min), 0, 1) : 0;
        double x = r + t * Math.Max(0, w - 2 * r);
        dc.DrawRoundedRectangle(Ui.Bg3, null, new Rect(r, y - 2.5, Math.Max(0, w - 2 * r), 5), 2.5, 2.5);
        dc.DrawRoundedRectangle(Ui.Accent, null, new Rect(r, y - 2.5, Math.Max(0, x - r), 5), 2.5, 2.5);
        dc.DrawEllipse(IsMouseOver || IsMouseCaptured ? Ui.AccentHover : Ui.Text, new Pen(Ui.Bg0, 2), new Point(x, y), r, r);
    }

    void FromMouse(MouseEventArgs e)
    {
        double r = 7, w = Math.Max(1, ActualWidth - 2 * r);
        double t = Math.Clamp((e.GetPosition(this).X - r) / w, 0, 1);
        Dragged?.Invoke(Min + t * (Max - Min));
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) { CaptureMouse(); FromMouse(e); e.Handled = true; }
    protected override void OnMouseMove(MouseEventArgs e) { if (IsMouseCaptured) FromMouse(e); }
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) { ReleaseMouseCapture(); InvalidateVisual(); }
    protected override void OnMouseEnter(MouseEventArgs e) => InvalidateVisual();
    protected override void OnMouseLeave(MouseEventArgs e) => InvalidateVisual();
}

/// <summary>Base of the Projects labs: parameter list → control column, a drawing on the right, ▶ to animate a run
/// that was fully computed first (the animation replays the same numbers <see cref="RunToEnd"/> produces).</summary>
public abstract class ProjectLab : ILab
{
    protected readonly List<LabParam> Params = new();
    /// <summary>labParams that are not controls (e.g. "floor", "duration").</summary>
    protected readonly Dictionary<string, string> Settings = new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, double> _metrics = new();
    public IReadOnlyDictionary<string, double> Metrics => _metrics;
    protected Dictionary<string, double> M => _metrics;
    public event Action? Finished;

    FrameworkElement? _view;
    public FrameworkElement View => _view ??= BuildView();

    DispatcherTimer? _timer;
    readonly Stopwatch _clock = new();
    Button? _runBtn;
    readonly TextBlock _status = Ui.Label("", 12, Ui.TextDim);
    protected readonly TextBlock Info = new() { FontSize = 12.5, Foreground = Ui.Text, TextWrapping = TextWrapping.Wrap, LineHeight = 18 };

    /// <summary>Animation position in simulated seconds (NaN = show the final state of the last run, or the setup).</summary>
    protected double PlayTime = double.NaN;
    /// <summary>True once a run has been computed (the drawing can show its trajectory).</summary>
    protected bool HasRun;
    /// <summary>Simulated duration of the last run (what the animation plays).</summary>
    protected double RunDuration;
    /// <summary>True when the results of a run can be shown (it ran and its animation, if any, has finished).</summary>
    protected bool ShowResults => HasRun && double.IsNaN(PlayTime);

    /// <summary>The ids shown when the lesson's labParams has no "controls" key.</summary>
    protected abstract string[] DefaultControls { get; }
    /// <summary>Runs the experiment with the current params and fills <see cref="M"/> (and whatever the drawing needs).</summary>
    protected abstract void Compute();
    /// <summary>The drawing (redrawn on <see cref="Redraw"/>).</summary>
    protected abstract FrameworkElement Scene { get; }
    /// <summary>Text under the drawing: setup values before a run, results after.</summary>
    protected abstract string Readout();
    /// <summary>Static checks that don't need a run (stability, buoyancy…), refreshed when an input changes.</summary>
    protected virtual void InputsChanged() { }

    protected LabParam AddNum(string id, string label, string unit, double min, double max, double step, double def,
        Func<double, string>? fmt = null, string tip = "")
    {
        var p = new LabParam { Id = id, Label = L.T(label), Unit = unit, Min = min, Max = max, Step = step, Value = def, Format = fmt, Tip = L.T(tip) };
        Params.Add(p);
        return p;
    }

    protected LabParam AddChoice(string id, string label, (string, string)[] choices, string def, string tip = "")
    {
        var p = new LabParam { Id = id, Label = L.T(label), Choices = choices.Select(c => (c.Item1, L.T(c.Item2))).ToArray(), Choice = def, Tip = L.T(tip) };
        Params.Add(p);
        return p;
    }

    protected LabParam P(string id) => Params.First(p => p.Id == id);
    protected double Num(string id) => P(id).Value;
    protected string Pick(string id) => P(id).Choice;
    protected double Setting(string key, double def) =>
        Settings.TryGetValue(key, out var s) && Question.TryNum(s, out var v) ? v : def;

    public void Load(IReadOnlyDictionary<string, string> labParams)
    {
        var visible = DefaultControls;
        // Ranges first, so a starting value outside the default range is not clamped away.
        foreach (var (k, v) in labParams)
        {
            int dot = k.LastIndexOf('.');
            if (dot <= 0) continue;
            var p = Params.FirstOrDefault(p => p.Id == k[..dot]);
            if (p == null || !Question.TryNum(v, out var n)) continue;
            switch (k[(dot + 1)..])
            {
                case "min": p.Min = n; break;
                case "max": p.Max = n; break;
                case "step": p.Step = n; break;
            }
        }
        foreach (var (k, v) in labParams)
        {
            if (k.Contains('.')) continue;
            if (k == "controls") { visible = v.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries); continue; }
            var p = Params.FirstOrDefault(p => p.Id == k);
            if (p != null) p.Set(v);
            else Settings[k] = v;
        }
        foreach (var p in Params) p.Visible = visible.Contains(p.Id);
        OnLoaded();
        InputsChanged();
    }

    protected virtual void OnLoaded() { }

    public void SetInputs(IReadOnlyDictionary<string, string> inputs)
    {
        StopAnimation();
        foreach (var (k, v) in inputs) Params.FirstOrDefault(p => p.Id == k)?.Set(v);
        foreach (var p in Params) p.RefreshUi?.Invoke();
        HasRun = false;
        InputsChanged();
        Redraw();
    }

    public void RunToEnd()
    {
        StopAnimation();
        M.Clear();
        Compute();
        HasRun = true;
        PlayTime = double.NaN;
        Redraw();
    }

    public void Stop() => StopAnimation();

    void StopAnimation()
    {
        if (_timer == null) return;
        _timer.Stop();
        _timer = null;
        if (_runBtn != null) _runBtn.Content = RunLabel;
        PlayTime = double.NaN;
    }

    /// <summary>Real seconds the animation of a run lasts (long flights are sped up).</summary>
    protected virtual double PlaySeconds(double simSeconds) => Math.Clamp(simSeconds, 2, 9);
    protected virtual string RunLabel => L.T("▶ Lançar");

    void Play()
    {
        if (_timer != null) { StopAnimation(); Redraw(); return; }
        M.Clear();
        Compute();
        HasRun = true;
        double real = PlaySeconds(RunDuration), speed = RunDuration / Math.Max(real, 0.1);
        PlayTime = 0;
        _clock.Restart();
        _timer = new DispatcherTimer(DispatcherPriority.Render) { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += (_, _) =>
        {
            PlayTime = _clock.Elapsed.TotalSeconds * speed;
            _status.Text = speed > 1.05 ? L.F("tempo: {0} s  (×{1})", Fmt.F(Math.Min(PlayTime, RunDuration), 1), Fmt.F(speed, speed < 10 ? 1 : 0))
                                        : L.F("tempo: {0} s", Fmt.F(Math.Min(PlayTime, RunDuration), 1));
            if (PlayTime >= RunDuration)
            {
                StopAnimation();
                Redraw();
                Finished?.Invoke();
                return;
            }
            Redraw();
        };
        _timer.Start();
        if (_runBtn != null) _runBtn.Content = L.T("■ Parar");
        Redraw();
    }

    protected void Redraw()
    {
        if (_view == null) return;
        Scene.InvalidateVisual();
        Info.Text = Readout();
        if (double.IsNaN(PlayTime) && !HasRun) _status.Text = "";
    }

    FrameworkElement BuildView()
    {
        var root = new Grid { MaxHeight = 450 };
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(262) });
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
        root.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 220 });

        var left = new StackPanel();
        foreach (var p in Params.Where(p => p.Visible)) left.Children.Add(Row(p));
        var buttons = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
        _runBtn = Ui.Button(RunLabel, Ui.Accent);
        _runBtn.Click += (_, _) => Play();
        buttons.Children.Add(_runBtn);
        _status.VerticalAlignment = VerticalAlignment.Center;
        buttons.Children.Add(_status);
        left.Children.Add(buttons);
        root.Children.Add(new ScrollViewer { Content = left, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, MaxHeight = 450 });

        var right = new Grid();
        right.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        right.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var scene = Scene;
        scene.MinHeight = 250;
        right.Children.Add(new Border { Child = scene, Background = Ui.Code, CornerRadius = new CornerRadius(6), ClipToBounds = true });
        Info.Margin = new Thickness(2, 8, 0, 0);
        Grid.SetRow(Info, 1);
        right.Children.Add(Info);
        Grid.SetColumn(right, 2);
        root.Children.Add(right);
        root.Height = 430;
        Info.Text = Readout();
        return root;
    }

    UIElement Row(LabParam p)
    {
        var g = new Grid { Margin = new Thickness(0, 2, 0, 2), ToolTip = p.Tip.Length > 0 ? p.Tip : null };
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(104) });
        g.ColumnDefinitions.Add(new ColumnDefinition());
        var label = new TextBlock { Text = p.Label, FontSize = 12.5, Foreground = Ui.TextDim, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
        g.Children.Add(label);
        if (p.IsChoice)
        {
            var box = new ComboBox { FontSize = 12.5, Height = 28 };
            foreach (var (v, l) in p.Choices!) box.Items.Add(new ComboBoxItem { Content = l, Tag = v });
            void Sync() => box.SelectedIndex = Array.FindIndex(p.Choices!, c => c.Value == p.Choice);
            Sync();
            box.SelectionChanged += (_, _) =>
            {
                if (box.SelectedItem is ComboBoxItem it && it.Tag is string v && v != p.Choice) { p.Choice = v; Changed(); }
            };
            p.RefreshUi = Sync;
            Grid.SetColumn(box, 1);
            g.Children.Add(box);
        }
        else
        {
            var inner = new Grid();
            inner.ColumnDefinitions.Add(new ColumnDefinition());
            inner.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70) });
            var slider = new FlatSlider { Min = p.Min, Max = p.Max, Value = p.Value, VerticalAlignment = VerticalAlignment.Center };
            var value = new TextBlock { FontSize = 12.5, Foreground = Ui.Text, VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Right };
            void Sync() { slider.Min = p.Min; slider.Max = p.Max; slider.Value = p.Value; slider.InvalidateVisual(); value.Text = p.Shown; }
            Sync();
            slider.Dragged += v => { double old = p.Value; p.SetNum(v); if (p.Value != old) { Sync(); Changed(); } };
            slider.MouseWheel += (_, e) => { p.SetNum(p.Value + Math.Sign(e.Delta) * p.Step); Sync(); Changed(); e.Handled = true; };
            p.RefreshUi = Sync;
            inner.Children.Add(slider);
            Grid.SetColumn(value, 1);
            inner.Children.Add(value);
            Grid.SetColumn(inner, 1);
            g.Children.Add(inner);
        }
        return g;
    }

    void Changed()
    {
        StopAnimation();
        HasRun = false;
        InputsChanged();
        Redraw();
    }

    // ---- drawing helpers shared by the scenes ----

    protected static Pen PenOf(Brush b, double w) { var p = new Pen(b, w); p.Freeze(); return p; }

    protected static void Text(DrawingContext dc, Visual v, string s, double x, double y, double size, Brush b,
        bool bold = false, TextAlignment align = TextAlignment.Left)
    {
        var ft = new FormattedText(s, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, bold ? FontWeights.SemiBold : FontWeights.Normal, FontStretches.Normal),
            size, b, VisualTreeHelper.GetDpi(v).PixelsPerDip);
        if (align == TextAlignment.Right) x -= ft.Width;
        else if (align == TextAlignment.Center) x -= ft.Width / 2;
        dc.DrawText(ft, new Point(x, y));
    }

    /// <summary>"Nice" axis maximum (1, 2, 5 × 10^n) at or above <paramref name="v"/>.</summary>
    protected static double NiceMax(double v)
    {
        if (v <= 0 || double.IsNaN(v)) return 1;
        double p = Math.Pow(10, Math.Floor(Math.Log10(v)));
        foreach (var m in new[] { 1.0, 2, 2.5, 5, 10 }) if (m * p >= v * 0.999) return m * p;
        return 10 * p;
    }

    /// <summary>Draws a line plot of (t, y) samples in <paramref name="area"/> up to time <paramref name="tEnd"/>.</summary>
    protected static void Plot(DrawingContext dc, Visual v, Rect area, IReadOnlyList<(double t, double y)> pts, double tMax,
        double yMin, double yMax, double tEnd, Brush line, string yLabel, Func<double, string> yFmt, bool yDown = false)
    {
        var axis = PenOf(Ui.Border, 1);
        dc.DrawRectangle(null, axis, area);
        for (int i = 1; i < 4; i++)
        {
            double gy = area.Top + area.Height * i / 4;
            dc.DrawLine(PenOf(new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)), 1), new Point(area.Left, gy), new Point(area.Right, gy));
        }
        double Y(double y) => yDown ? area.Top + (y - yMin) / (yMax - yMin) * area.Height : area.Bottom - (y - yMin) / (yMax - yMin) * area.Height;
        Text(dc, v, yFmt(yDown ? yMin : yMax), area.Left + 3, area.Top + 2, 10.5, Ui.TextDim);
        Text(dc, v, yFmt(yDown ? yMax : yMin), area.Left + 3, area.Bottom - 15, 10.5, Ui.TextDim);
        Text(dc, v, L.T(yLabel), area.Right - 3, area.Top + 2, 10.5, Ui.TextDim, align: TextAlignment.Right);
        Text(dc, v, Fmt.F(tMax, tMax < 10 ? 1 : 0) + " s", area.Right - 3, area.Bottom + 1, 10.5, Ui.TextDim, align: TextAlignment.Right);
        if (pts.Count < 2) return;
        var geo = new StreamGeometry();
        using (var g = geo.Open())
        {
            bool first = true;
            double lastX = double.NegativeInfinity;
            foreach (var (t, y) in pts)
            {
                if (t > tEnd) break;
                double x = area.Left + t / tMax * area.Width;
                if (x - lastX < 0.5 && !first) continue;
                lastX = x;
                var pt = new Point(x, Math.Clamp(Y(y), area.Top - 2, area.Bottom + 2));
                if (first) { g.BeginFigure(pt, false, false); first = false; } else g.LineTo(pt, true, false);
            }
        }
        geo.Freeze();
        dc.DrawGeometry(null, PenOf(line, 2), geo);
    }

    /// <summary>Value of a recorded series at time t (linear interpolation).</summary>
    protected static double At(IReadOnlyList<(double t, double y)> pts, double t)
    {
        if (pts.Count == 0) return 0;
        if (t <= pts[0].t) return pts[0].y;
        int lo = 0, hi = pts.Count - 1;
        if (t >= pts[hi].t) return pts[hi].y;
        while (hi - lo > 1) { int mid = (lo + hi) / 2; if (pts[mid].t <= t) lo = mid; else hi = mid; }
        var (t0, y0) = pts[lo]; var (t1, y1) = pts[hi];
        return t1 > t0 ? y0 + (y1 - y0) * (t - t0) / (t1 - t0) : y0;
    }
}

/// <summary>A FrameworkElement that draws through a callback (the lab's scene).</summary>
public sealed class SceneCanvas(Action<DrawingContext, SceneCanvas> draw) : FrameworkElement
{
    protected override void OnRender(DrawingContext dc)
    {
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, ActualWidth, ActualHeight));
        draw(dc, this);
    }
}
