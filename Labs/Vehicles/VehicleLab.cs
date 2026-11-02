using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace EasyCreator.Vehicles;

/// <summary>Shared base of the vehicle labs (car, airplane, helicopter, drone): a list of numeric/choice inputs shown as
/// sliders and combo boxes, a scene and a plot drawn with a DrawingContext, and a "▶ Rodar" animation that replays the
/// trace computed by <see cref="Compute"/>. <see cref="RunToEnd"/> uses the very same <see cref="Compute"/>, so the
/// headless metrics are exactly what the animation shows.
/// labParams: "mode", "show" (comma list of visible inputs), "&lt;input&gt;" (default value), "&lt;input&gt;.min"/".max"
/// (slider range); anything else is read by the lab itself.</summary>
public abstract class VehicleLab : ILab
{
    protected static readonly CultureInfo Br = CultureInfo.GetCultureInfo("pt-BR");
    protected const double G = 9.81;

    protected sealed class Param
    {
        public string Name = "", Label = "", Unit = "";
        public double Min, Max, Step = 1, Value;
        public string[]? Keys, Labels;
        internal Action? Sync;
        public bool IsChoice => Keys != null;
        int Index => Keys == null ? 0 : Math.Clamp((int)Math.Round(Value), 0, Keys.Length - 1);
        public string Key => Keys == null ? "" : Keys[Index];
        string Format => Step >= 1 ? "0" : Step >= 0.1 - 1e-9 ? "0.0" : Step >= 0.01 - 1e-9 ? "0.00" : "0.000";
        public string Show() => IsChoice ? Labels![Index] : Value.ToString(Format, Br) + (Unit.Length > 0 ? " " + Unit : "");

        public double Snap(double v)
        {
            if (double.IsNaN(v) || double.IsInfinity(v)) v = Min;
            v = Math.Clamp(v, Min, Max);
            if (Step > 0 && !IsChoice) v = Min + Math.Round((v - Min) / Step) * Step;
            return Math.Clamp(Math.Round(v, 6), Min, Max);
        }
    }

    readonly List<Param> _params = new();
    readonly Dictionary<string, Param> _byName = new(StringComparer.OrdinalIgnoreCase);
    readonly Dictionary<string, double> _metrics = new();
    /// <summary>Metrics of the last <see cref="Compute"/> (published to <see cref="Metrics"/> when a run ends).</summary>
    protected readonly Dictionary<string, double> Calc = new();
    protected IReadOnlyDictionary<string, string> LabParams = new Dictionary<string, string>();
    protected string Mode = "";
    /// <summary>Simulated seconds of the last computation, and how many real seconds its animation should take.</summary>
    protected double Duration = 1, RealSeconds = 5;
    /// <summary>Current animation time (simulated seconds): 0 before a run, <see cref="Duration"/> after it.</summary>
    protected double T;
    protected bool Ran;

    FrameworkElement? _view;
    DrawSurface? _scene, _plot;
    TextBlock? _summary;
    Button? _run;
    readonly Stopwatch _clock = new();
    double _lastClock;
    bool _animating, _syncing;

    public event Action? Finished;
    public IReadOnlyDictionary<string, double> Metrics => _metrics;
    public FrameworkElement View => _view ??= BuildView();

    // ---- what each lab provides -------------------------------------------------------------------------------

    protected abstract string Title { get; }
    protected abstract string[] DefaultShown(string mode);
    /// <summary>Runs the whole physics for the current inputs: fills <see cref="Calc"/>, the lab's trace,
    /// <see cref="Duration"/> and <see cref="RealSeconds"/>. Must be cheap (it also runs when a slider moves).</summary>
    protected abstract void Compute();
    protected abstract void DrawScene(DrawingContext dc, Size s);
    protected abstract void DrawPlot(DrawingContext dc, Size s);
    /// <summary>One or two lines under the pictures; <see cref="Ran"/> says whether a run has finished.</summary>
    protected abstract string Summary();
    /// <summary>Called by <see cref="Load"/> after the mode is known (rename labels, change defaults per mode…).</summary>
    protected virtual void OnMode() { }

    // ---- inputs ---------------------------------------------------------------------------------------------

    protected void Num(string name, string label, string unit, double min, double max, double step, double def)
    {
        var p = new Param { Name = name, Label = label, Unit = unit, Min = min, Max = max, Step = step, Value = def };
        _params.Add(p);
        _byName[name] = p;
    }

    protected void Choice(string name, string label, string[] keys, string[] labels, int def = 0)
    {
        var p = new Param { Name = name, Label = label, Min = 0, Max = keys.Length - 1, Step = 1, Value = def, Keys = keys, Labels = labels };
        _params.Add(p);
        _byName[name] = p;
    }

    protected Param P(string name) => _byName[name];
    protected double V(string name) => _byName.TryGetValue(name, out var p) ? p.Value : 0;
    protected string C(string name) => _byName.TryGetValue(name, out var p) ? p.Key : "";
    protected string LP(string key, string def = "") => LabParams.TryGetValue(key, out var v) ? v : def;
    protected double LPNum(string key, double def) => Question.TryNum(LP(key), out var v) ? v : def;

    bool SetParam(Param p, string raw)
    {
        raw = (raw ?? "").Trim();
        if (p.IsChoice)
        {
            int i = Array.FindIndex(p.Keys!, k => string.Equals(k, raw, StringComparison.OrdinalIgnoreCase));
            if (i < 0) i = Array.FindIndex(p.Labels!, k => string.Equals(k, raw, StringComparison.OrdinalIgnoreCase));
            if (i < 0 && int.TryParse(raw, out var n) && n >= 0 && n < p.Keys!.Length) i = n;
            if (i < 0) return false;
            p.Value = i;
            return true;
        }
        if (!Question.TryNum(raw, out var v)) return false;
        p.Value = p.Snap(v);
        return true;
    }

    public void Load(IReadOnlyDictionary<string, string> labParams)
    {
        try
        {
            LabParams = labParams ?? new Dictionary<string, string>();
            Mode = LP("mode", Mode).Trim().ToLowerInvariant();
            OnMode();
            foreach (var p in _params)
            {
                if (Question.TryNum(LP(p.Name + ".min"), out var mn)) p.Min = mn;
                if (Question.TryNum(LP(p.Name + ".max"), out var mx)) p.Max = mx;
                if (Question.TryNum(LP(p.Name + ".step"), out var st) && st > 0) p.Step = st;
                if (p.Max < p.Min) p.Max = p.Min;
                if (LabParams.TryGetValue(p.Name, out var raw)) SetParam(p, raw);
                p.Value = p.Snap(p.Value);
            }
            Recompute();
            T = 0;
            Ran = false;
        }
        catch { /* a lab never throws */ }
    }

    public void SetInputs(IReadOnlyDictionary<string, string> inputs)
    {
        try
        {
            foreach (var (k, v) in inputs ?? new Dictionary<string, string>())
                if (_byName.TryGetValue(k, out var p) && SetParam(p, v)) p.Sync?.Invoke();
            StopAnim();
            Recompute();
            T = 0;
            Ran = false;
            Redraw();
        }
        catch { }
    }

    public void RunToEnd()
    {
        try
        {
            StopAnim();
            Recompute();
            T = Duration;
            Publish();
            Redraw();
        }
        catch { }
    }

    public void Stop() => StopAnim();

    protected void Recompute()
    {
        Calc.Clear();
        try { Compute(); }
        catch { /* keep whatever was computed */ }
        if (!(Duration > 0) || double.IsInfinity(Duration)) Duration = 1;
        if (!(RealSeconds > 0)) RealSeconds = 4;
    }

    void Publish()
    {
        _metrics.Clear();
        foreach (var (k, v) in Calc)
            if (!double.IsNaN(v) && !double.IsInfinity(v)) _metrics[k] = Math.Round(v, 6);
        Ran = true;
    }

    // ---- view -----------------------------------------------------------------------------------------------

    FrameworkElement BuildView()
    {
        var root = new StackPanel();
        var head = new DockPanel { Margin = new Thickness(0, 0, 0, 8), LastChildFill = true };
        _run = Ui.Button("▶ Rodar", Ui.Accent);
        _run.Margin = new Thickness(8, 0, 0, 0);
        _run.Click += (_, _) => Play();
        DockPanel.SetDock(_run, Dock.Right);
        head.Children.Add(_run);
        var title = Ui.Label(Title, 15, bold: true);
        title.VerticalAlignment = VerticalAlignment.Center;
        head.Children.Add(title);
        root.Children.Add(head);

        var pics = new Grid { Height = 190 };
        pics.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        pics.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });
        pics.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
        _scene = new DrawSurface(DrawScene);
        _plot = new DrawSurface(DrawPlot);
        var sb = Frame(_scene);
        var pb = Frame(_plot);
        Grid.SetColumn(pb, 2);
        pics.Children.Add(sb);
        pics.Children.Add(pb);
        root.Children.Add(pics);

        _summary = Ui.Label("", 13, Ui.TextDim);
        _summary.Margin = new Thickness(0, 6, 0, 4);
        root.Children.Add(_summary);

        var shown = LP("show").Length > 0
            ? LP("show").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : DefaultShown(Mode);
        var grid = new UniformGrid { Columns = 3 };
        foreach (var name in shown)
            if (_byName.TryGetValue(name, out var p)) grid.Children.Add(Control(p));
        root.Children.Add(grid);
        root.Unloaded += (_, _) => StopAnim();
        UpdateSummary();
        return root;
    }

    static Border Frame(UIElement child) => new()
    {
        Child = child, CornerRadius = new CornerRadius(6), Background = Ui.Code, ClipToBounds = true,
        BorderBrush = Ui.Border, BorderThickness = new Thickness(1),
    };

    FrameworkElement Control(Param p)
    {
        var cell = new StackPanel { Margin = new Thickness(0, 4, 12, 4) };
        var top = new DockPanel();
        var val = new TextBlock { Text = p.Show(), Foreground = Ui.Accent, FontSize = 12, FontWeight = FontWeights.SemiBold, Margin = new Thickness(6, 0, 0, 0) };
        DockPanel.SetDock(val, Dock.Right);
        top.Children.Add(val);
        top.Children.Add(new TextBlock { Text = p.Label, Foreground = Ui.TextDim, FontSize = 12, TextTrimming = TextTrimming.CharacterEllipsis, ToolTip = p.Label });
        cell.Children.Add(top);
        if (p.IsChoice)
        {
            var combo = new ComboBox { FontSize = 13, Margin = new Thickness(0, 3, 0, 0) };
            foreach (var l in p.Labels!) combo.Items.Add(new ComboBoxItem { Content = l });
            combo.SelectedIndex = (int)p.Value;
            combo.SelectionChanged += (_, _) =>
            {
                if (_syncing || combo.SelectedIndex < 0) return;
                p.Value = combo.SelectedIndex;
                val.Text = p.Show();
                InputChanged();
            };
            p.Sync = () => { _syncing = true; combo.SelectedIndex = (int)p.Value; val.Text = p.Show(); _syncing = false; };
            cell.Children.Add(combo);
        }
        else
        {
            var slider = new Slider
            {
                Minimum = p.Min, Maximum = p.Max, Value = p.Value, SmallChange = p.Step, LargeChange = p.Step * 10,
                TickFrequency = p.Step, IsSnapToTickEnabled = true, Margin = new Thickness(0, 3, 0, 0),
            };
            slider.ValueChanged += (_, e) =>
            {
                if (_syncing) return;
                p.Value = p.Snap(e.NewValue);
                val.Text = p.Show();
                InputChanged();
            };
            p.Sync = () => { _syncing = true; slider.Value = p.Value; val.Text = p.Show(); _syncing = false; };
            cell.Children.Add(slider);
        }
        return cell;
    }

    void InputChanged()
    {
        StopAnim();
        Recompute();
        T = 0;
        Ran = false;
        Redraw();
    }

    protected void Redraw()
    {
        _scene?.InvalidateVisual();
        _plot?.InvalidateVisual();
        UpdateSummary();
    }

    void UpdateSummary()
    {
        if (_summary == null) return;
        try { _summary.Text = Summary(); } catch { _summary.Text = ""; }
        if (_run != null) _run.Content = _animating ? "↺ Reiniciar" : "▶ Rodar";
    }

    // ---- animation ------------------------------------------------------------------------------------------

    void Play()
    {
        StopAnim();
        Recompute();
        T = 0;
        Ran = false;
        _clock.Restart();
        _lastClock = 0;
        _animating = true;
        CompositionTarget.Rendering += OnFrame;
        Redraw();
    }

    void OnFrame(object? sender, EventArgs e)
    {
        try
        {
            double now = _clock.Elapsed.TotalSeconds, dt = Math.Min(0.1, now - _lastClock);
            _lastClock = now;
            T += dt * Duration / RealSeconds;
            if (T >= Duration)
            {
                T = Duration;
                StopAnim();
                Publish();
                Redraw();
                Finished?.Invoke();
                return;
            }
            Redraw();
        }
        catch { StopAnim(); }
    }

    void StopAnim()
    {
        if (!_animating) return;
        CompositionTarget.Rendering -= OnFrame;
        _animating = false;
        _clock.Stop();
        UpdateSummary();
    }

    // ---- helpers --------------------------------------------------------------------------------------------

    /// <summary>Linear interpolation in a trace whose rows start with the time.</summary>
    protected static double[] At(List<double[]> trace, double t)
    {
        if (trace.Count == 0) return new double[8];
        if (t <= trace[0][0]) return trace[0];
        if (t >= trace[^1][0]) return trace[^1];
        int lo = 0, hi = trace.Count - 1;
        while (hi - lo > 1) { int mid = (lo + hi) / 2; if (trace[mid][0] <= t) lo = mid; else hi = mid; }
        var a = trace[lo];
        var b = trace[hi];
        double f = (t - a[0]) / Math.Max(1e-12, b[0] - a[0]);
        var r = new double[a.Length];
        for (int i = 0; i < a.Length; i++) r[i] = a[i] + (b[i] - a[i]) * f;
        return r;
    }

    /// <summary>ISA (standard atmosphere) air density in kg/m³ up to 11 km.</summary>
    protected static double Density(double h)
    {
        h = Math.Clamp(h, -500, 11000);
        double temp = 288.15 - 0.0065 * h;
        return 1.225 * Math.Pow(temp / 288.15, 4.2559);
    }
}
