using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using static EasyCreator.MetalKit;

namespace EasyCreator;

/// <summary>Heat treatment lab: pick the steel (%C), the austenitizing temperature, the cooling medium and an optional
/// tempering. Draws the simplified Fe–Fe₃C diagram with the chosen point, the time–temperature curve and a micrograph
/// of the resulting microstructure. Physics in <see cref="HeatTreatModel"/>.</summary>
[Lab("heattreat")]
public sealed class HeatTreatLab : ILab
{
    readonly StackPanel _root = new();
    readonly Paint _canvas;
    readonly TextBlock _structure = new() { FontSize = 12.5, Foreground = Ui.Text, TextWrapping = TextWrapping.Wrap };
    readonly TextBlock _tip = new() { FontSize = 12.5, Foreground = Ui.TextDim, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 0) };
    readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
    readonly Stopwatch _clock = new();
    SliderRow? _carbon, _temp, _temper;
    ChoiceRow? _medium;
    Button? _run;
    double _c = 0.20, _t = 700, _tempering;
    string _med = "air";
    HeatTreatModel.Result? _shown;
    double _phase = -1;  // -1 = not run; 0..1 = animation; 1 = done
    bool _running, _syncing;
    Dictionary<string, double> _metrics = new();
    readonly Dictionary<string, BitmapSource> _micros = new();
    const double RunSeconds = 4.2;

    public HeatTreatLab()
    {
        _canvas = new Paint(Draw) { Height = 236 };
        _timer.Tick += (_, _) => Tick();
    }

    public FrameworkElement View => _root;
    public IReadOnlyDictionary<string, double> Metrics => _metrics;
    public event Action? Finished;

    public void Load(IReadOnlyDictionary<string, string> p)
    {
        _c = Math.Clamp(Num(p, "carbon", 0.20), 0.02, 1.5);
        _t = Num(p, "temperature", 700);
        _med = Str(p, "cooling", "air");
        _tempering = Num(p, "temper", 0);
        string steel = Str(p, "steelName", "");
        bool lockC = Str(p, "lock", "").Contains("carbon");

        _root.Children.Clear();
        var title = steel.Length > 0 ? $"🔥 Tratamento térmico · aço {steel.ToUpperInvariant()}" : "🔥 Tratamento térmico de um aço carbono";
        var (bar, run, _) = Header(title, "▶ Tratar");
        _run = run;
        run.Click += (_, _) => { if (_running) StopRun(); else StartRun(); };
        _root.Children.Add(bar);
        _root.Children.Add(new Border { Child = _canvas, CornerRadius = new CornerRadius(8), Background = Ui.Code, ClipToBounds = true });
        var res = new StackPanel { Margin = new Thickness(0, 6, 0, 6) };
        res.Children.Add(_structure);
        res.Children.Add(_tip);
        _root.Children.Add(res);

        var form = new Form();
        _carbon = new SliderRow("Carbono", 0.05, 1.4, 0.01, _c, v => $"{F(v, "0.00")} % C ({Kind(v)})", 80, 150);
        if (lockC) _carbon.Enabled = false;
        _temp = new SliderRow("Aquecer a", 500, 1300, 5, _t, v => $"{v:0} °C ({(_c <= HeatTreatModel.Eutectoid ? "A3" : "Acm")} {HeatTreatModel.Upper(_c):0})", 80, 150);
        _medium = new ChoiceRow("Resfriar em", HeatTreatModel.Media.Select(m => (m.id, m.name)), _med, 80);
        _temper = new SliderRow("Revenir a", 0, 650, 25, _tempering, v => v < 150 ? "sem revenimento" : $"{v:0} °C", 80, 150);
        foreach (var s in new[] { _carbon, _temp, _temper }) s.Changed += Changed;
        _medium.Changed += Changed;
        form.Add(_carbon.Row);
        form.Add(_temp.Row);
        form.Add(_medium.Row);
        form.Add(_temper.Row);
        _root.Children.Add(form.Root);
        ShowText();
    }

    static string Kind(double c) => c < 0.7 ? "hipoeutetoide" : c <= 0.82 ? "eutetoide" : "hipereutetoide";

    void Read()
    {
        if (_carbon == null) return;
        _c = _carbon.Value; _t = _temp!.Value; _med = _medium!.Value; _tempering = _temper!.Value;
    }

    void Changed()
    {
        if (_syncing) return;
        Read();
        _temp?.Refresh();
        if (!_running) { if (_phase >= 1) _phase = -1; _shown = null; ShowText(); _canvas.InvalidateVisual(); }
    }

    public void SetInputs(IReadOnlyDictionary<string, string> d)
    {
        _c = Num(d, "carbon", _c); _t = Num(d, "temperature", _t); _med = Str(d, "cooling", _med); _tempering = Num(d, "temper", _tempering);
        if (_carbon == null) return;
        _syncing = true;
        _carbon.Value = _c; _temp!.Value = _t; _medium!.Value = _med; _temper!.Value = _tempering;
        _syncing = false;
        Read();
        Changed();
    }

    double TemperOrZero => _tempering >= 150 ? _tempering : 0;

    void StartRun()
    {
        Read();
        _shown = HeatTreatModel.Compute(_c, _t, _med, TemperOrZero);
        _phase = 0; _running = true;
        if (_run != null) _run.Content = "■ Parar";
        ShowText();
        _clock.Restart();
        _timer.Start();
    }

    void StopRun()
    {
        _timer.Stop(); _running = false; _phase = -1; _shown = null;
        if (_run != null) _run.Content = "▶ Tratar";
        ShowText();
        _canvas.InvalidateVisual();
    }

    void Tick()
    {
        _phase = Math.Min(1, _clock.Elapsed.TotalSeconds / RunSeconds);
        _canvas.InvalidateVisual();
        if (_phase < 1) return;
        _timer.Stop(); _running = false;
        if (_run != null) _run.Content = "▶ Tratar";
        _metrics = new Dictionary<string, double>(_shown!.Metrics);
        ShowText();
        Finished?.Invoke();
    }

    public void RunToEnd()
    {
        _timer.Stop(); _running = false;
        Read();
        _shown = HeatTreatModel.Compute(_c, _t, _med, TemperOrZero);
        _phase = 1;
        _metrics = new Dictionary<string, double>(_shown.Metrics);
        if (_run != null) _run.Content = "▶ Tratar";
        ShowText();
        _canvas.InvalidateVisual();
    }

    public void Stop() { _timer.Stop(); _running = false; }

    void ShowText()
    {
        var r = _shown;
        if (r == null || _phase < 1)
        {
            var (fg, _, phases) = HeatTreatModel.AtTemperature(_c, _t);
            _structure.Text = _running ? "Aquecendo, mantendo na temperatura e resfriando…"
                : $"A {_t:0} °C este aço fica: {phases}" + (fg > 0 && fg < 1 ? $" ({fg * 100:0}% austenita)" : "") + ".";
            _tip.Text = "Escolha a temperatura no diagrama, o meio de resfriamento e clique em ▶ Tratar.";
            _tip.Foreground = Ui.TextDim;
            return;
        }
        _structure.Text = $"Resultado: {r.Structure}.  Dureza ≈ {r.HV:0} HV" + (r.HRC >= 20 ? $" ≈ {F(r.HRC, "0")} HRC" : " (abaixo da escala HRC)") +
                          $" · tenacidade {r.Toughness:0}/100" + (r.QuenchCrack > 0.3 ? $" · risco de trinca de têmpera {r.QuenchCrack * 100:0}%" : "");
        _tip.Text = "💡 " + r.Tip;
        _tip.Foreground = Ui.Text;
    }

    // ---------------- drawing ----------------

    const double CMax = 2.2, TMin = 400, TMax = 1600;
    static readonly Pen LinePen = Pen(Brush("#E6E6EA", 0.85), 1.3), DashPen = Pen(Brush("#A0A0AA", 0.7), 1, true), AxisPen = Pen(Brush("#A0A0AA", 0.6), 1);
    static readonly Brush GammaFill = Brush("#E0A030", 0.16), AlphaGammaFill = Brush("#4A90D9", 0.16), GammaCemFill = Brush("#B070D0", 0.16),
        LowFill = Brush("#A0A0AA", 0.08), LiquidFill = Brush("#FF5C6C", 0.16), MarkFill = Brush("#18B98A"), Hot = Brush("#FF9A3C");

    void Draw(DrawingContext dc, double w, double h)
    {
        double split = Math.Round(w * 0.5);
        DrawDiagram(dc, new Rect(0, 0, split, h));
        dc.DrawRectangle(Ui.Border, null, new Rect(split, 6, 1, h - 12));
        DrawCurve(dc, new Rect(split + 1, 0, w - split - 1, h * 0.5));
        DrawMicro(dc, new Rect(split + 1, h * 0.5, w - split - 1, h * 0.5));
    }

    /// <summary>Temperature of the part at animation phase p (heat, soak, cool, temper).</summary>
    double TempAt(double p)
    {
        const double room = 25;
        if (p < 0.28) return room + (_t - room) * (p / 0.28);
        if (p < 0.4) return _t;
        double coolEnd = TemperOrZero > 0 ? 0.82 : 1.0;
        if (p < coolEnd)
        {
            double u = (p - 0.4) / (coolEnd - 0.4);
            double k = _med switch { "water" => 3.0, "oil" => 1.2, "air" => 0.25, _ => 0.06 };
            return room + (_t - room) * Math.Exp(-k * u * 5);
        }
        double v = (p - coolEnd) / (1 - coolEnd);
        return room + (TemperOrZero - room) * Math.Sin(Math.PI * Math.Min(1, v));
    }

    void DrawDiagram(DrawingContext dc, Rect a)
    {
        dc.DrawText(Text("Diagrama Fe–Fe₃C (simplificado)", 11, Ui.TextDim, _canvas), new Point(a.X + 10, a.Y + 3));
        var plot = new Rect(a.X + 40, a.Y + 22, a.Width - 52, a.Height - 44);
        Point P(double c, double t) => new(plot.X + plot.Width * c / CMax, plot.Bottom - plot.Height * (t - TMin) / (TMax - TMin));
        void Poly(Brush b, params (double c, double t)[] pts)
        {
            var g = new StreamGeometry();
            using (var x = g.Open())
            {
                x.BeginFigure(P(pts[0].c, pts[0].t), true, true);
                for (int i = 1; i < pts.Length; i++) x.LineTo(P(pts[i].c, pts[i].t), false, false);
            }
            g.Freeze();
            dc.DrawGeometry(b, null, g);
        }
        double liqAtEdge = 1538 - (1538 - 1148) * CMax / 4.3;
        Poly(LowFill, (0, TMin), (CMax, TMin), (CMax, 727), (0, 727));
        Poly(AlphaGammaFill, (0, 912), (0, 727), (0.76, 727));
        Poly(GammaFill, (0, 912), (0.76, 727), (2.11, 1148), (0, 1538));
        Poly(GammaCemFill, (0.76, 727), (CMax, 727), (CMax, 1148), (2.11, 1148));
        Poly(LiquidFill, (0, 1538), (2.11, 1148), (CMax, 1148), (CMax, liqAtEdge));
        dc.DrawLine(LinePen, P(0, 912), P(0.76, 727));
        dc.DrawLine(LinePen, P(0.76, 727), P(2.11, 1148));
        dc.DrawLine(LinePen, P(0.022, 727), P(CMax, 727));
        dc.DrawLine(LinePen, P(0, 1538), P(2.11, 1148));
        dc.DrawLine(LinePen, P(0, 1538), P(CMax, liqAtEdge));
        dc.DrawLine(LinePen, P(2.11, 1148), P(CMax, 1148));
        dc.DrawLine(DashPen, P(0.76, TMin), P(0.76, 727));
        // axes
        dc.DrawLine(AxisPen, new Point(plot.X, plot.Bottom), new Point(plot.Right, plot.Bottom));
        dc.DrawLine(AxisPen, new Point(plot.X, plot.Y), new Point(plot.X, plot.Bottom));
        foreach (var t in new[] { 400, 727, 912, 1148, 1538 })
            dc.DrawText(Text(t.ToString(), 9.5, Ui.TextDim, _canvas), new Point(a.X + 6, P(0, t).Y - 7));
        foreach (var c in new[] { 0.0, 0.76, 1.5 })
            dc.DrawText(Text(F(c, "0.##"), 9.5, Ui.TextDim, _canvas), new Point(P(c, 0).X - 8, plot.Bottom + 2));
        dc.DrawText(Text("% C →", 9.5, Ui.TextDim, _canvas), new Point(plot.Right - 30, plot.Bottom + 2));
        var lab = Brush("#E6E6EA", 0.8);
        dc.DrawText(Text("γ austenita", 11, lab, _canvas, true), P(0.62, 1010));
        dc.DrawText(Text("α+γ", 10, lab, _canvas), P(0.04, 790));
        dc.DrawText(Text("γ+Fe₃C", 10, lab, _canvas), P(1.4, 860));
        dc.DrawText(Text("α + Fe₃C (ferrita + perlita / perlita + cementita)", 9.5, lab, _canvas), P(0.05, 640));
        dc.DrawText(Text("L+γ", 10, lab, _canvas), P(1.75, 1320));
        dc.DrawText(Text("A1 727 °C", 9.5, Hot, _canvas), P(1.5, 755));
        dc.DrawText(Text("A3", 9.5, Hot, _canvas), P(0.3, 900));
        dc.DrawText(Text("Acm", 9.5, Hot, _canvas), P(1.55, 1060));

        // the steel: dashed vertical line and the point (moves during the run)
        dc.DrawLine(Pen(MarkFill, 1, true), P(_c, TMin), P(_c, TMax));
        double tNow = _phase is >= 0 and < 1 ? TempAt(_phase) : _t;
        var pt = P(_c, Math.Clamp(tNow, TMin, TMax));
        dc.DrawEllipse(_running ? Hot : MarkFill, Pen(Brushes.White, 1.2), pt, 5, 5);
        dc.DrawText(Text($"{F(_c, "0.00")}% C · {tNow:0} °C", 10, Brush("#E6E6EA"), _canvas, true), new Point(Math.Min(pt.X + 7, plot.Right - 90), pt.Y - 16));
    }

    void DrawCurve(DrawingContext dc, Rect a)
    {
        dc.DrawText(Text("Temperatura × tempo (escala ilustrativa)", 11, Ui.TextDim, _canvas), new Point(a.X + 10, a.Y + 3));
        var plot = new Rect(a.X + 36, a.Y + 22, a.Width - 46, a.Height - 30);
        Point P(double p, double t) => new(plot.X + plot.Width * p, plot.Bottom - plot.Height * Math.Clamp(t / 1300, 0, 1));
        dc.DrawLine(AxisPen, new Point(plot.X, plot.Bottom), new Point(plot.Right, plot.Bottom));
        dc.DrawLine(AxisPen, new Point(plot.X, plot.Y), new Point(plot.X, plot.Bottom));
        dc.DrawLine(DashPen, P(0, 727), P(1, 727));
        dc.DrawText(Text("A1", 9.5, Hot, _canvas), new Point(plot.X - 22, P(0, 727).Y - 7));
        dc.DrawText(Text("25", 9.5, Ui.TextDim, _canvas), new Point(plot.X - 18, plot.Bottom - 8));
        if (_phase < 0) return;
        var g = new StreamGeometry();
        double end = Math.Min(1, _phase);
        using (var x = g.Open())
        {
            x.BeginFigure(P(0, TempAt(0)), false, false);
            for (double p = 0.005; p <= end; p += 0.005) x.LineTo(P(p, TempAt(p)), true, false);
        }
        g.Freeze();
        dc.DrawGeometry(null, Pen(Hot, 2), g);
        var color = _med switch { "water" => "#4A90D9", "oil" => "#C9A25E", "air" => "#A0A0AA", _ => "#FF5C6C" };
        if (end > 0.42) dc.DrawText(Text(HeatTreatModel.Media.First(m => m.id == _med).name, 10, Brush(color), _canvas), new Point(P(0.44, 0).X, plot.Bottom - 15));
        if (TemperOrZero > 0 && end > 0.9) dc.DrawText(Text($"revenimento {TemperOrZero:0} °C", 10, Ui.TextDim, _canvas), new Point(P(0.74, 0).X, P(0, TemperOrZero).Y - 16));
    }

    void DrawMicro(DrawingContext dc, Rect a)
    {
        double d = Math.Min(a.Height - 12, 110), cx = a.X + 12 + d / 2, cy = a.Y + a.Height / 2;
        string title; BitmapSource img;
        if (_shown != null && _phase >= 1) { img = Micro(_shown, false); title = "Microestrutura final"; }
        else if (_phase >= 0 && _shown != null && TempAt(_phase) >= HeatTreatModel.A1 && _phase < 0.42) { img = Micro(_shown, true); title = $"A {TempAt(_phase):0} °C"; }
        else { img = Micro(HeatTreatModel.Compute(_c, 25, "furnace", 0), false); title = "Antes (recozido)"; }
        dc.PushClip(new EllipseGeometry(new Point(cx, cy), d / 2, d / 2));
        dc.DrawImage(img, new Rect(cx - d / 2, cy - d / 2, d, d));
        dc.Pop();
        dc.DrawEllipse(null, Pen(Brush("#35353F"), 2), new Point(cx, cy), d / 2, d / 2);
        double lx = cx + d / 2 + 12;
        dc.DrawText(Text(title, 11, Ui.Text, _canvas, true), new Point(lx, a.Y + 6));
        var r = _shown != null && _phase >= 1 ? _shown : null;
        var rows = r == null ? new List<(string, double, Color)>() : Legend(r);
        if (r == null && _phase >= 0 && _shown != null && TempAt(_phase) >= HeatTreatModel.A1)
            rows.Add(("austenita (γ)", _shown.AustAtTemp, PhaseColor("A")));
        double y = a.Y + 26;
        foreach (var (name, f, col) in rows)
        {
            dc.DrawRectangle(Brush(col), null, new Rect(lx, y + 3, 10, 10));
            dc.DrawText(Text($"{name} {f * 100:0}%", 10.5, Ui.Text, _canvas), new Point(lx + 15, y));
            y += 16;
        }
        if (r != null) dc.DrawText(Text($"{r.HV:0} HV" + (r.HRC >= 20 ? $" · {r.HRC:0} HRC" : ""), 12, Ui.Accent, _canvas, true), new Point(lx, Math.Max(y + 2, a.Bottom - 22)));
    }

    static List<(string, double, Color)> Legend(HeatTreatModel.Result r)
    {
        var l = new List<(string, double, Color)>();
        if (r.Ferrite >= 0.01) l.Add(("ferrita", r.Ferrite, PhaseColor("F")));
        if (r.Pearlite >= 0.01) l.Add((r.FinePearlite ? "perlita fina" : "perlita", r.Pearlite, PhaseColor("P")));
        if (r.Cementite >= 0.01) l.Add(("cementita", r.Cementite, PhaseColor("C")));
        if (r.Bainite >= 0.01) l.Add(("bainita", r.Bainite, PhaseColor("B")));
        if (r.Martensite >= 0.01) l.Add((r.Tempered ? "martensita revenida" : "martensita", r.Martensite, PhaseColor(r.Tempered ? "T" : "M")));
        if (r.RetainedAustenite >= 0.01) l.Add(("austenita retida", r.RetainedAustenite, PhaseColor("A")));
        return l;
    }

    static Color PhaseColor(string k) => k switch
    {
        "F" => Color.FromRgb(0xE8, 0xE4, 0xDA), "P" => Color.FromRgb(0x6E, 0x62, 0x58), "C" => Color.FromRgb(0xFF, 0xFF, 0xF4),
        "B" => Color.FromRgb(0x7A, 0x6A, 0x5A), "M" => Color.FromRgb(0x9A, 0x8C, 0x7C), "T" => Color.FromRgb(0x5A, 0x4C, 0x44),
        _ => Color.FromRgb(0xD8, 0xC8, 0x9C),
    };

    /// <summary>A procedural micrograph: Voronoi grains, each given a phase by the fractions; pearlite is striped,
    /// martensite needle-like, bainite feathery; proeutectoid cementite forms a bright network at the grain boundaries.</summary>
    BitmapSource Micro(HeatTreatModel.Result r, bool hot)
    {
        string key = hot ? $"hot{r.AustAtTemp:0.00}{_c:0.00}" :
            $"{r.Ferrite:0.00}{r.Pearlite:0.00}{r.Cementite:0.00}{r.Bainite:0.00}{r.Martensite:0.00}{r.RetainedAustenite:0.00}{r.Tempered}{r.FinePearlite}{r.CoarseGrain}";
        if (_micros.TryGetValue(key, out var cached)) return cached;
        const int N = 128;
        var rng = new Random(1234);
        int grains = r.CoarseGrain && !hot ? 14 : 34;
        var seeds = new (double x, double y, double ang)[grains];
        for (int i = 0; i < grains; i++) seeds[i] = (rng.NextDouble() * N, rng.NextDouble() * N, rng.NextDouble() * Math.PI);
        // phase per grain from the cumulative fractions (cementite is drawn as boundary network instead)
        var phase = new char[grains];
        var order = Enumerable.Range(0, grains).OrderBy(_ => rng.Next()).ToArray();
        var fr = hot
            ? new List<(char, double)> { ('A', r.AustAtTemp), (_c <= HeatTreatModel.Eutectoid ? 'F' : 'P', 1 - r.AustAtTemp) }
            : new List<(char, double)> { ('F', r.Ferrite), ('P', r.Pearlite), ('B', r.Bainite), (r.Tempered ? 'T' : 'M', r.Martensite), ('A', r.RetainedAustenite) };
        double total = fr.Sum(f => f.Item2);
        double acc = 0; int gi = 0;
        foreach (var (ph, f) in fr)
        {
            acc += total > 0 ? f / total : 0;
            while (gi < grains && (gi + 0.5) / grains <= acc + 1e-9) phase[order[gi++]] = ph;
        }
        while (gi < grains) phase[order[gi++]] = fr.OrderByDescending(f => f.Item2).First().Item1;
        double cemW = hot ? 0 : Math.Min(3.5, r.Cementite * 28);
        var px = new byte[N * N * 4];
        for (int y = 0; y < N; y++)
        for (int x = 0; x < N; x++)
        {
            double d1 = 1e9, d2 = 1e9; int g = 0;
            for (int i = 0; i < grains; i++)
            {
                double dx = x - seeds[i].x, dy = y - seeds[i].y, dd = dx * dx + dy * dy;
                if (dd < d1) { d2 = d1; d1 = dd; g = i; } else if (dd < d2) d2 = dd;
            }
            double edge = Math.Sqrt(d2) - Math.Sqrt(d1);
            Color c;
            double ang = seeds[g].ang, u = x * Math.Cos(ang) + y * Math.Sin(ang), v = -x * Math.Sin(ang) + y * Math.Cos(ang);
            switch (phase[g])
            {
                case 'F': c = PhaseColor("F"); break;
                case 'P':
                    double period = r.FinePearlite ? 2.2 : 3.4;
                    c = Math.Sin(u * Math.PI * 2 / period) > 0.2 ? Color.FromRgb(0x3E, 0x34, 0x2E) : Color.FromRgb(0xC8, 0xC0, 0xB0);
                    break;
                case 'B':
                    c = Math.Sin(u * 0.9 + 2.5 * Math.Sin(v * 0.25)) > 0.3 ? Color.FromRgb(0x4A, 0x3E, 0x34) : Color.FromRgb(0x9C, 0x90, 0x80);
                    break;
                case 'M':
                case 'T':
                    double a2 = ang + ((int)(v / 9) % 2 == 0 ? 1.1 : -0.6);
                    double u2 = x * Math.Cos(a2) + y * Math.Sin(a2);
                    bool needle = Math.Sin(u2 * 1.6) > 0.55 && Math.Sin((x * Math.Sin(a2) - y * Math.Cos(a2)) * 0.18) > -0.3;
                    c = phase[g] == 'M'
                        ? (needle ? Color.FromRgb(0x5A, 0x50, 0x48) : Color.FromRgb(0xB8, 0xAE, 0xA0))
                        : (needle ? Color.FromRgb(0x2E, 0x26, 0x22) : Color.FromRgb(0x6A, 0x5C, 0x52));
                    break;
                default:
                    c = PhaseColor("A");
                    if (hot && Math.Abs(Math.Sin(u * 0.35)) < 0.06) c = Mix(c, Colors.White, 0.5); // annealing twins
                    break;
            }
            if (cemW > 0 && edge < cemW) c = PhaseColor("C");
            else if (edge < 1.1) c = Mix(c, Colors.Black, 0.55);
            int k = (y * N + x) * 4;
            px[k] = c.B; px[k + 1] = c.G; px[k + 2] = c.R; px[k + 3] = 255;
        }
        var bmp = BitmapSource.Create(N, N, 96, 96, PixelFormats.Bgra32, null, px, N * 4);
        bmp.Freeze();
        _micros[key] = bmp;
        return bmp;
    }
}
