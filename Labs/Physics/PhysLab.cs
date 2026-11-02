using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>Base for the physics labs: number inputs (slider + value label) and option inputs (toggle buttons), a
/// fixed 640×230 drawing surface that scales down with the column, a ▶ run button that replays an experiment computed
/// up front, live readouts while it plays and result chips at the end.
/// Subclasses declare inputs in their constructor (<see cref="Num"/>, <see cref="Opt"/>), compute everything in
/// <see cref="Compute"/> (fill <see cref="M"/> and their own samples) and draw in <see cref="Paint"/> using
/// <see cref="Progress"/> (0..1 of the animation) and <see cref="HasResult"/>.
/// labParams: any input key sets that input's starting value; "hide" and "lock" take a comma list of input keys.
/// Texts: input labels and option labels are declared in Portuguese and translated when shown (<see cref="L.T"/>);
/// subclasses translate their own result chips and drawing texts. Numbers follow the language (<see cref="Pt"/>).</summary>
public abstract class PhysLab : ILab
{
    public const double W = 640, H = 230;
    static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR"), EnUs = CultureInfo.GetCultureInfo("en-US"),
        EsEs = CultureInfo.GetCultureInfo("es-ES");
    /// <summary>Number format of the current language (decimal comma in Portuguese and Spanish, point in English).</summary>
    protected static CultureInfo Pt => L.Lang switch { "en" => EnUs, "es" => EsEs, _ => PtBr };

    protected static readonly Brush Blue = Ui.Make("#5AA9FF"), Orange = Ui.Make("#FFA94D"), Yellow = Ui.Make("#FFD166"),
        Purple = Ui.Make("#C39BFF"), GridLine = Ui.Make("#2A2A33"), Faint = Ui.Make("#4A4A55"),
        AccentSoft = Ui.Make("#3318B98A"), BlueSoft = Ui.Make("#335AA9FF");

    protected sealed class Input
    {
        public string Key = "", Label = "", Unit = "", Format = "0.##";
        public double Min, Max, Step, Value;
        public List<(string Value, string Label)>? Options;
        public string Choice = "";
        public Slider? Slider;
        public TextBlock? ValueText, NameText;
        public List<Button>? Buttons;
        public FrameworkElement? Cell;
        public bool Hidden, Locked;
        public bool IsOption => Options != null;
    }

    readonly List<Input> _inputs = new();
    /// <summary>Metrics of the last run (cleared when an input changes).</summary>
    protected readonly Dictionary<string, double> M = new();
    public IReadOnlyDictionary<string, double> Metrics => M;
    public event Action? Finished;

    FrameworkElement? _view;
    readonly Surface _surface;
    readonly WrapPanel _readouts = new() { VerticalAlignment = VerticalAlignment.Center };
    Button? _run;
    bool _animating, _suppress;
    readonly Stopwatch _clock = new();

    /// <summary>True after a run (animated or <see cref="RunToEnd"/>) until an input changes.</summary>
    protected bool HasResult { get; private set; }
    /// <summary>0..1 position of the replay; 1 when idle.</summary>
    protected double Progress { get; private set; } = 1;
    /// <summary>Seconds the replay lasts on screen (set it in <see cref="Compute"/>).</summary>
    protected double AnimSeconds { get; set; } = 3;

    protected PhysLab()
    {
        _surface = new Surface(this);
    }

    // ---- declaring inputs -------------------------------------------------------------------------------------------

    protected Input Num(string key, string label, string unit, double min, double max, double value, double step, string format = "0.##")
    {
        var i = new Input { Key = key, Label = label, Unit = unit, Min = min, Max = max, Value = value, Step = step, Format = format };
        _inputs.Add(i);
        return i;
    }

    protected Input Opt(string key, string label, string value, params (string Value, string Label)[] options)
    {
        var i = new Input { Key = key, Label = label, Options = options.ToList(), Choice = value };
        _inputs.Add(i);
        return i;
    }

    protected double V(string key) => Find(key)?.Value ?? 0;
    protected string C(string key) => Find(key)?.Choice ?? "";
    protected bool IsHidden(string key) => Find(key)?.Hidden ?? true;
    Input? Find(string key) => _inputs.FirstOrDefault(i => i.Key == key);

    // ---- ILab ---------------------------------------------------------------------------------------------------------

    public FrameworkElement View => _view ??= BuildView();

    public void Load(IReadOnlyDictionary<string, string> labParams)
    {
        foreach (var (k, v) in labParams)
        {
            var i = Find(k);
            if (i != null) Apply(i, v);
        }
        foreach (var (key, flag) in new[] { ("hide", 0), ("lock", 1) })
            if (labParams.TryGetValue(key, out var list))
                foreach (var name in list.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    var i = Find(name);
                    if (i == null) continue;
                    if (flag == 0) i.Hidden = true; else i.Locked = true;
                }
        OnLoad(labParams);
        if (_view != null) { foreach (var i in _inputs) SyncControl(i); }
        InputsChanged();
    }

    public void SetInputs(IReadOnlyDictionary<string, string> inputs)
    {
        foreach (var (k, v) in inputs)
        {
            var i = Find(k);
            if (i != null) Apply(i, v);
        }
        InputsChanged();
    }

    public void RunToEnd()
    {
        StopAnimation();
        DoCompute();
        Progress = 1;
        ShowResults();
        _surface.InvalidateVisual();
    }

    public void Stop() => StopAnimation();

    // ---- what each lab provides ---------------------------------------------------------------------------------------

    /// <summary>Lab-specific labParams (target distance, ramp length…).</summary>
    protected virtual void OnLoad(IReadOnlyDictionary<string, string> labParams) { }
    /// <summary>Runs the whole experiment: fill <see cref="M"/>, keep samples for <see cref="Paint"/>, set <see cref="AnimSeconds"/>.</summary>
    protected abstract void Compute();
    /// <summary>Draws the scene on a 640×230 surface (background already painted).</summary>
    protected abstract void Paint(DrawingContext dc);
    /// <summary>One line shown while the replay plays (time, position…).</summary>
    protected virtual string LiveText() => "";
    /// <summary>Result chips shown after a run: label and formatted value.</summary>
    protected abstract IEnumerable<(string Label, string Value)> Results();

    // ---- engine ---------------------------------------------------------------------------------------------------------

    void DoCompute()
    {
        M.Clear();
        Compute();
        foreach (var i in _inputs)
            if (!M.ContainsKey(i.Key)) M[i.Key] = i.IsOption ? Math.Max(0, i.Options!.FindIndex(o => o.Value == i.Choice)) : i.Value;
        HasResult = true;
    }

    void Apply(Input i, string text)
    {
        if (i.IsOption)
        {
            var o = i.Options!.FirstOrDefault(o => string.Equals(o.Value, text?.Trim(), StringComparison.OrdinalIgnoreCase));
            if (o.Value != null) i.Choice = o.Value;
        }
        else if (Question.TryNum(text, out var v)) i.Value = Math.Clamp(v, i.Min, i.Max);
        SyncControl(i);
    }

    void SyncControl(Input i)
    {
        _suppress = true;
        try
        {
            if (i.Slider != null) { i.Slider.Value = i.Value; i.Slider.IsEnabled = !i.Locked; }
            if (i.Buttons != null)
                foreach (var b in i.Buttons)
                {
                    bool on = (string)b.Tag == i.Choice;
                    b.Background = on ? Ui.Accent : Ui.Bg3;
                    b.Foreground = on ? Ui.OnAccent : Ui.Text;
                    b.IsEnabled = !i.Locked;
                }
            if (i.ValueText != null) i.ValueText.Text = ValueLabel(i);
            if (i.Cell != null) { i.Cell.Visibility = i.Hidden ? Visibility.Collapsed : Visibility.Visible; i.Cell.Opacity = i.Locked ? 0.6 : 1; }
            if (i.NameText != null) i.NameText.Text = i.Locked ? L.F("{0} (travado)", L.T(i.Label)) : L.T(i.Label);
        }
        finally { _suppress = false; }
    }

    protected virtual string ValueLabel(Input i)
    {
        if (i.IsOption) return "";
        var s = i.Value.ToString(i.Format, Pt);
        return i.Unit.Length == 0 ? s : i.Unit == "°" ? s + "°" : s + " " + i.Unit;
    }

    void InputsChanged()
    {
        StopAnimation();
        HasResult = false;
        Progress = 1;
        M.Clear();
        OnInputsChanged();
        ShowPrompt();
        _surface.InvalidateVisual();
    }

    /// <summary>Called when any input changes (before redrawing the preview).</summary>
    protected virtual void OnInputsChanged() { }

    void StartRun()
    {
        StopAnimation();
        DoCompute();
        Progress = 0;
        _animating = true;
        _clock.Restart();
        CompositionTarget.Rendering += OnFrame;
        if (_run != null) _run.Content = L.T("↻ Rodar de novo");
        _surface.InvalidateVisual();
    }

    void OnFrame(object? sender, EventArgs e)
    {
        if (!_animating) return;
        Progress = Math.Min(1, _clock.Elapsed.TotalSeconds / Math.Max(0.2, AnimSeconds));
        _readouts.Children.Clear();
        _readouts.Children.Add(Ui.Label(LiveText(), 13, Ui.Text));
        _surface.InvalidateVisual();
        if (Progress >= 1)
        {
            StopAnimation();
            ShowResults();
            Finished?.Invoke();
        }
    }

    void StopAnimation()
    {
        if (_animating) CompositionTarget.Rendering -= OnFrame;
        _animating = false;
        _clock.Stop();
    }

    void ShowPrompt()
    {
        if (_view == null) return;
        if (_run != null) _run.Content = L.T("▶ Rodar");
        _readouts.Children.Clear();
        _readouts.Children.Add(Ui.Label(L.T("Ajuste os controles e clique em ▶ Rodar."), 13, Ui.TextDim));
    }

    void ShowResults()
    {
        if (_view == null) return;
        _readouts.Children.Clear();
        foreach (var (label, value) in Results())
        {
            var tb = new TextBlock { FontSize = 12.5, Foreground = Ui.TextDim };
            tb.Inlines.Add(new Run(label + ": "));
            tb.Inlines.Add(new Run(value) { Foreground = Ui.Text, FontWeight = FontWeights.SemiBold });
            _readouts.Children.Add(new Border
            {
                Child = tb, Background = Ui.Bg2, CornerRadius = new CornerRadius(5), Padding = new Thickness(7, 3, 7, 3),
                Margin = new Thickness(0, 2, 6, 2), BorderBrush = Ui.Border, BorderThickness = new Thickness(1),
            });
        }
    }

    FrameworkElement BuildView()
    {
        var root = new StackPanel();
        root.Children.Add(new Border
        {
            CornerRadius = new CornerRadius(6), ClipToBounds = true, Background = Ui.Code, BorderBrush = Ui.Border, BorderThickness = new Thickness(1),
            Child = new Viewbox { Child = _surface, Stretch = Stretch.Uniform, MaxHeight = H },
        });

        var bar = new Grid { Margin = new Thickness(0, 8, 0, 6) };
        bar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        bar.ColumnDefinitions.Add(new ColumnDefinition());
        _run = Ui.Button(L.T("▶ Rodar"), Ui.Accent);
        _run.VerticalAlignment = VerticalAlignment.Top;
        _run.Margin = new Thickness(0, 0, 10, 0);
        _run.Click += (_, _) => StartRun();
        bar.Children.Add(_run);
        Grid.SetColumn(_readouts, 1);
        _readouts.MinHeight = 34;
        bar.Children.Add(_readouts);
        root.Children.Add(bar);

        var grid = new System.Windows.Controls.Primitives.UniformGrid { Columns = 2 };
        foreach (var i in _inputs)
        {
            i.Cell = BuildCell(i);
            grid.Children.Add(i.Cell);
            SyncControl(i);
        }
        root.Children.Add(grid);
        if (HasResult) ShowResults(); else ShowPrompt();
        return root;
    }

    FrameworkElement BuildCell(Input i)
    {
        var cell = new Grid { Margin = new Thickness(0, 0, 14, 4) };
        cell.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        cell.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        var name = i.NameText = Ui.Label(L.T(i.Label), 12.5, Ui.TextDim);
        cell.Children.Add(name);
        i.ValueText = Ui.Label("", 12.5, Ui.Accent, true);
        i.ValueText.HorizontalAlignment = HorizontalAlignment.Right;
        cell.Children.Add(i.ValueText);
        FrameworkElement control;
        if (i.IsOption)
        {
            var wrap = new WrapPanel { Margin = new Thickness(0, 3, 0, 0) };
            i.Buttons = new();
            foreach (var (value, label) in i.Options!)
            {
                var b = Ui.Button(L.T(label));
                b.Padding = new Thickness(9, 3, 9, 3);
                b.FontSize = 12.5;
                b.Margin = new Thickness(0, 0, 5, 3);
                b.Tag = value;
                b.Click += (_, _) => { if (_suppress) return; i.Choice = value; SyncControl(i); InputsChanged(); };
                i.Buttons.Add(b);
                wrap.Children.Add(b);
            }
            name.Margin = new Thickness(0, 0, 0, 0);
            control = wrap;
        }
        else
        {
            var s = new Slider
            {
                Minimum = i.Min, Maximum = i.Max, Value = i.Value, SmallChange = i.Step, LargeChange = i.Step * 10,
                Margin = new Thickness(0, 2, 0, 0), Focusable = true,
            };
            s.ValueChanged += (_, e) =>
            {
                if (_suppress) return;
                double v = Math.Round((e.NewValue - i.Min) / i.Step) * i.Step + i.Min;
                i.Value = Math.Clamp(Math.Round(v, 6), i.Min, i.Max);
                if (i.ValueText != null) i.ValueText.Text = ValueLabel(i);
                InputsChanged();
            };
            i.Slider = s;
            control = s;
        }
        Grid.SetRow(control, 1);
        cell.Children.Add(control);
        return cell;
    }

    sealed class Surface : FrameworkElement
    {
        readonly PhysLab _lab;
        public Surface(PhysLab lab) { _lab = lab; Width = W; Height = H; ClipToBounds = true; }
        protected override void OnRender(DrawingContext dc)
        {
            _lab._dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            dc.DrawRectangle(Ui.Code, null, new Rect(0, 0, W, H));
            try { _lab.Paint(dc); }
            catch (Exception ex) { _lab.Text(dc, L.F("Erro ao desenhar: {0}", ex.Message), 10, 10, 12, Ui.Bad); }
        }
    }

    // ---- drawing helpers ------------------------------------------------------------------------------------------------

    double _dpi = 1;
    static readonly Dictionary<(Brush, double, bool), Pen> Pens = new();
    static readonly Typeface Face = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
    static readonly Typeface FaceBold = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);

    protected static Pen P(Brush b, double thickness = 1.5, bool dashed = false)
    {
        if (Pens.TryGetValue((b, thickness, dashed), out var p)) return p;
        p = new Pen(b, thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
        if (dashed) p.DashStyle = new DashStyle(new double[] { 4, 3 }, 0);
        p.Freeze();
        return Pens[(b, thickness, dashed)] = p;
    }

    /// <summary>Text at (x, y) = top-left; align 0.5 centres it on x, 1 right-aligns it.</summary>
    protected void Text(DrawingContext dc, string text, double x, double y, double size, Brush brush, double align = 0, bool bold = false)
    {
        var ft = new FormattedText(text, Pt, FlowDirection.LeftToRight, bold ? FaceBold : Face, size, brush, _dpi);
        dc.DrawText(ft, new Point(x - ft.Width * align, y));
    }

    protected static void Line(DrawingContext dc, Brush b, double x1, double y1, double x2, double y2, double t = 1.5, bool dashed = false) =>
        dc.DrawLine(P(b, t, dashed), new Point(x1, y1), new Point(x2, y2));

    protected static void Arrow(DrawingContext dc, Brush b, double x1, double y1, double x2, double y2, double t = 2.2)
    {
        double dx = x2 - x1, dy = y2 - y1, len = Math.Sqrt(dx * dx + dy * dy);
        if (len < 1) return;
        dc.DrawLine(P(b, t), new Point(x1, y1), new Point(x2, y2));
        double ux = dx / len, uy = dy / len, head = Math.Min(9, len * 0.6);
        var g = new StreamGeometry();
        using (var c = g.Open())
        {
            c.BeginFigure(new Point(x2, y2), true, true);
            c.LineTo(new Point(x2 - ux * head - uy * head * 0.5, y2 - uy * head + ux * head * 0.5), true, false);
            c.LineTo(new Point(x2 - ux * head + uy * head * 0.5, y2 - uy * head - ux * head * 0.5), true, false);
        }
        g.Freeze();
        dc.DrawGeometry(b, null, g);
    }

    protected static void Polyline(DrawingContext dc, Pen pen, IReadOnlyList<Point> pts, int count)
    {
        count = Math.Min(count, pts.Count);
        if (count < 2) return;
        var g = new StreamGeometry();
        using (var c = g.Open())
        {
            c.BeginFigure(pts[0], false, false);
            for (int k = 1; k < count; k++) c.LineTo(pts[k], true, false);
        }
        g.Freeze();
        dc.DrawGeometry(null, pen, g);
    }

    protected static void Dot(DrawingContext dc, Brush b, double x, double y, double r) => dc.DrawEllipse(b, null, new Point(x, y), r, r);

    /// <summary>A "nice" grid step (1, 2, 5 × 10ⁿ) giving about <paramref name="count"/> divisions of <paramref name="range"/>.</summary>
    protected static double NiceStep(double range, int count = 6)
    {
        if (range <= 0 || double.IsNaN(range) || double.IsInfinity(range)) return 1;
        double raw = range / count, mag = Math.Pow(10, Math.Floor(Math.Log10(raw))), n = raw / mag;
        return (n < 1.5 ? 1 : n < 3.5 ? 2 : n < 7.5 ? 5 : 10) * mag;
    }

    /// <summary>Simple x(t)-style chart in the box (x, y, w, h): axes, zero line, labels (already translated).</summary>
    protected void Chart(DrawingContext dc, double x, double y, double w, double h, string title, string xLabel)
    {
        dc.DrawRectangle(null, P(GridLine, 1), new Rect(x, y, w, h));
        Line(dc, Faint, x, y + h / 2, x + w, y + h / 2, 1, true);
        Text(dc, title, x + 2, y - 15, 11, Ui.TextDim);
        Text(dc, xLabel, x + w - 2, y + h + 1, 11, Ui.TextDim, 1);
    }

    /// <summary>Number in the current language's format; ∞ and NaN read "∞" and "—".</summary>
    protected static string F(double v, string fmt = "0.##")
    {
        if (double.IsNaN(v)) return "—";
        if (double.IsPositiveInfinity(v)) return "∞";
        if (double.IsNegativeInfinity(v)) return "−∞";
        return v.ToString(fmt, Pt);
    }

    protected static double Rad(double deg) => deg * Math.PI / 180;

    /// <summary>Planets offered by the gravity labs (g in m/s²).</summary>
    protected static readonly (string Value, string Label)[] Planets =
        { ("earth", "Terra"), ("moon", "Lua"), ("mars", "Marte"), ("jupiter", "Júpiter") };

    protected static double Gravity(string planet) => planet switch
    {
        "moon" => 1.62, "mars" => 3.71, "jupiter" => 24.79, _ => 9.81,
    };

    /// <summary>Times (interpolated) where samples cross zero; used to measure periods of oscillations.</summary>
    protected static double PeriodFromCrossings(List<double> crossings)
    {
        if (crossings.Count < 2) return double.PositiveInfinity;
        // consecutive crossings are half a period apart
        return 2 * (crossings[^1] - crossings[0]) / (crossings.Count - 1);
    }
}
