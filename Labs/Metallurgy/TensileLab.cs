using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static EasyCreator.MetalKit;

namespace EasyCreator;

/// <summary>A tensile test: choose the material and the specimen (diameter, gauge length), pull it to fracture and read the
/// engineering stress–strain curve: Young's modulus, yield strength (0.2% offset), UTS, elongation, maximum force.</summary>
[Lab("tensile")]
public sealed class TensileLab : ILab
{
    public sealed record Material(string Id, string Name, double Yield, double Uts, double E, double Elong, double UniformFrac, bool Luders, bool Brittle);

    public static readonly Material[] Materials =
    {
        new("aco1020", "Aço SAE 1020 (laminado)", 210, 380, 200, 25, 0.62, true, false),
        new("aco1045", "Aço SAE 1045 (laminado)", 310, 565, 205, 16, 0.6, true, false),
        new("aco4140", "Aço SAE 4140 temperado e revenido (~425 °C)", 1100, 1250, 205, 12, 0.5, false, false),
        new("inox304", "Aço inox 304 (recozido)", 205, 515, 193, 40, 0.8, false, false),
        new("al6061", "Alumínio 6061-T6", 276, 310, 69, 12, 0.6, false, false),
        new("cobre", "Cobre recozido", 69, 220, 115, 45, 0.75, false, false),
        new("titanio", "Titânio Ti-6Al-4V", 880, 950, 114, 14, 0.6, false, false),
        new("fofo", "Ferro fundido cinzento (frágil)", 0, 200, 100, 0.4, 1, false, true),
    };

    readonly StackPanel _root = new();
    readonly Paint _canvas;
    readonly TextBlock _out = new() { FontSize = 12.5, Foreground = Ui.Text, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 6) };
    readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
    readonly Stopwatch _clock = new();
    ChoiceRow? _mat;
    SliderRow? _dia, _len;
    Button? _run;
    Material _m = Materials[4];
    double _d = 10, _l0 = 50, _phase = -1;
    bool _running, _syncing;
    Dictionary<string, double> _metrics = new();
    const double RunSeconds = 4;

    public TensileLab()
    {
        _canvas = new Paint(Draw) { Height = 240 };
        _timer.Tick += (_, _) => Tick();
    }

    public FrameworkElement View => _root;
    public IReadOnlyDictionary<string, double> Metrics => _metrics;
    public event Action? Finished;

    static Material Find(string id) => Materials.FirstOrDefault(m => m.Id == id) ?? Materials[4];

    public void Load(IReadOnlyDictionary<string, string> p)
    {
        _m = Find(Str(p, "material", "al6061"));
        _d = Math.Clamp(Num(p, "diameter", 10), 4, 20);
        _l0 = Math.Clamp(Num(p, "gaugeLength", 50), 20, 100);
        _root.Children.Clear();
        var (bar, run, _) = Header(L.T("🔧 Ensaio de tração · corpo de prova cilíndrico"), L.T("▶ Tracionar"));
        _run = run;
        run.Click += (_, _) => { if (_running) StopRun(); else StartRun(); };
        _root.Children.Add(bar);
        _root.Children.Add(new Border { Child = _canvas, CornerRadius = new CornerRadius(8), Background = Ui.Code, ClipToBounds = true });
        _root.Children.Add(_out);
        var form = new Form();
        _mat = new ChoiceRow(L.T("Material"), Materials.Select(m => (m.Id, L.T(m.Name))), _m.Id, 80);
        _dia = new SliderRow(L.T("Diâmetro"), 4, 20, 0.5, _d, v => $"{F(v, "0.0")} mm (A₀ = {F(Math.PI * v * v / 4, "0.0")} mm²)", 80, 150);
        _len = new SliderRow(L.T("Comprimento útil"), 20, 100, 5, _l0, v => $"L₀ = {v:0} mm", 110, 80);
        _mat.Changed += Changed; _dia.Changed += Changed; _len.Changed += Changed;
        form.Add(_mat.Row);
        form.Add(_dia.Row);
        form.Add(_len.Row);
        _root.Children.Add(form.Root);
        ShowText();
    }

    void Read()
    {
        if (_mat == null) return;
        _m = Find(_mat.Value); _d = _dia!.Value; _l0 = _len!.Value;
    }

    void Changed()
    {
        if (_syncing) return;
        Read();
        if (_running) return;
        _phase = -1;
        ShowText();
        _canvas.InvalidateVisual();
    }

    public void SetInputs(IReadOnlyDictionary<string, string> d)
    {
        _m = Find(Str(d, "material", _m.Id)); _d = Num(d, "diameter", _d); _l0 = Num(d, "gaugeLength", _l0);
        if (_mat == null) return;
        _syncing = true;
        _mat.Value = _m.Id; _dia!.Value = _d; _len!.Value = _l0;
        _syncing = false;
        Changed();
    }

    // ---------------- the curve ----------------

    double Ef => _m.Brittle ? 2 * _m.Uts / (_m.E * 1000) : _m.Elong / 100;
    double EpsU => _m.Brittle ? Ef : Ef * _m.UniformFrac;

    /// <summary>Engineering stress (MPa) at engineering strain e.</summary>
    public double Stress(double e)
    {
        var m = _m;
        double E = m.E * 1000;
        if (m.Brittle) { double x = Math.Clamp(e / Ef, 0, 1); return m.Uts * (1 - (1 - x) * (1 - x)); }
        double upper = m.Luders ? 1.06 * m.Yield : m.Yield;
        double ey = upper / E;
        if (e <= ey) return E * e;
        double eL = m.Luders ? m.Yield / E + 0.015 : ey;
        if (e <= eL) return e < ey + 0.0015 ? upper - (upper - m.Yield) * (e - ey) / 0.0015 : m.Yield;
        if (e <= EpsU) { double x = (e - eL) / Math.Max(1e-6, EpsU - eL); return m.Yield + (m.Uts - m.Yield) * (1 - (1 - x) * (1 - x)); }
        double sf = m.Uts * (Ef > 0.2 ? 0.75 : 0.88);
        double xn = Math.Clamp((e - EpsU) / Math.Max(1e-6, Ef - EpsU), 0, 1);
        return m.Uts - (m.Uts - sf) * Math.Pow(xn, 1.6);
    }

    Dictionary<string, double> Compute()
    {
        double E = _m.E * 1000, area = Math.PI * _d * _d / 4;
        // 0.2 % offset yield
        double yield = 0;
        if (!_m.Brittle)
            for (double e = 0.002; e < Ef; e += 0.00005)
                if (Stress(e) <= E * (e - 0.002)) { yield = Stress(e); break; }
        double tough = 0, prev = 0;
        for (int i = 1; i <= 400; i++) { double e = Ef * i / 400, s = Stress(e); tough += (s + prev) / 2 * Ef / 400; prev = s; }
        double sf = Stress(Ef), plastic = Math.Max(0, Ef - sf / E);
        double ra = _m.Brittle ? 0 : Math.Clamp(0.35 + Ef * 1.2, 0.3, 0.8) * 100;
        return new()
        {
            ["youngModulus"] = _m.E, ["yield"] = Math.Round(yield), ["uts"] = _m.Uts, ["elongation"] = Math.Round(plastic * 100, 1),
            ["maxForce"] = Math.Round(_m.Uts * area / 1000, 2), ["yieldForce"] = Math.Round(yield * area / 1000, 2),
            ["finalLength"] = Math.Round(_l0 * (1 + plastic), 1), ["reductionArea"] = Math.Round(ra), ["toughness"] = Math.Round(tough, 1),
            ["area"] = Math.Round(area, 2), ["diameter"] = _d,
        };
    }

    void StartRun()
    {
        Read();
        _phase = 0; _running = true;
        if (_run != null) _run.Content = L.T("■ Parar");
        ShowText();
        _clock.Restart();
        _timer.Start();
    }

    void StopRun()
    {
        _timer.Stop(); _running = false; _phase = -1;
        if (_run != null) _run.Content = L.T("▶ Tracionar");
        ShowText();
        _canvas.InvalidateVisual();
    }

    void Tick()
    {
        _phase = Math.Min(1, _clock.Elapsed.TotalSeconds / RunSeconds);
        _canvas.InvalidateVisual();
        if (_phase < 1) { ShowLive(); return; }
        _timer.Stop(); _running = false;
        if (_run != null) _run.Content = L.T("▶ Tracionar");
        _metrics = Compute();
        ShowText();
        Finished?.Invoke();
    }

    public void RunToEnd()
    {
        _timer.Stop(); _running = false;
        Read();
        _phase = 1;
        _metrics = Compute();
        if (_run != null) _run.Content = L.T("▶ Tracionar");
        ShowText();
        _canvas.InvalidateVisual();
    }

    public void Stop() { _timer.Stop(); _running = false; }

    /// <summary>Strain at animation phase p: the elastic part is tiny, so it gets the first 15 % of the time.</summary>
    double StrainAt(double p)
    {
        double ey = _m.Brittle ? Ef : (_m.Luders ? 1.06 : 1) * _m.Yield / (_m.E * 1000);
        return p < 0.15 ? ey * p / 0.15 : ey + (Ef - ey) * (p - 0.15) / 0.85;
    }

    void ShowLive()
    {
        double e = StrainAt(_phase), s = Stress(e), area = Math.PI * _d * _d / 4;
        _out.Text = L.F("Tracionando… ε = {0}% · σ = {1:0} MPa · F = {2} kN", F(e * 100, "0.00"), s, F(s * area / 1000, "0.0"));
    }

    void ShowText()
    {
        if (_phase < 1)
        {
            _out.Text = _running ? L.T("Tracionando…") : L.F("{0}: clique em ▶ Tracionar para puxar o corpo de prova até romper.", L.T(_m.Name));
            return;
        }
        var m = _metrics;
        var parts = new List<string>
        {
            $"E = {m["youngModulus"]:0} GPa",
            _m.Brittle ? L.T("sem escoamento definido (frágil)") : L.F("escoamento σe = {0:0} MPa", m["yield"]),
            L.F("resistência σr = {0:0} MPa", m["uts"]),
            L.F("alongamento {0}%", F(m["elongation"], "0.#")),
            L.F("força máxima {0} kN", F(m["maxForce"], "0.0")),
            L.F("L final {0} mm", F(m["finalLength"], "0.0")),
        };
        if (!_m.Brittle) parts.Add(L.F("estricção {0:0}%", m["reductionArea"]));
        _out.Text = string.Join(" · ", parts) + (_m.Brittle ? " — " + L.T("rompeu sem avisar!") : "");
    }

    // ---------------- drawing ----------------

    static readonly Pen AxisPen = Pen(Brush("#A0A0AA", 0.6), 1), GridPen = Pen(Brush("#A0A0AA", 0.12), 1), CurvePen = Pen(Brush("#18B98A"), 2.2),
        DashPen = Pen(Brush("#A0A0AA", 0.6), 1, true), Outline = Pen(Brush("#000000", 0.5), 1);
    static readonly Brush Steel = Brush("#9AA3AE"), Grip = Brush("#3A3A44"), Mark = Brush("#F0B44C"), Red = Brush("#FF5C6C");

    void Draw(DrawingContext dc, double w, double h)
    {
        double split = Math.Round(w * 0.3);
        DrawSpecimen(dc, new Rect(0, 0, split, h));
        dc.DrawRectangle(Ui.Border, null, new Rect(split, 6, 1, h - 12));
        DrawPlot(dc, new Rect(split + 1, 0, w - split - 1, h));
    }

    void DrawSpecimen(DrawingContext dc, Rect a)
    {
        dc.DrawText(Text(L.T("Corpo de prova"), 11, Ui.TextDim, _canvas), new Point(a.X + 10, a.Y + 3));
        double e = _phase >= 0 ? StrainAt(Math.Min(_phase, 1)) : 0;
        bool broken = _phase >= 1;
        double cx = a.X + a.Width / 2, top = a.Y + 22, bottom = a.Bottom - 10;
        double gauge0 = (bottom - top) * 0.42, grow = gauge0 * Math.Min(e, 0.6);
        double gauge = gauge0 + grow, shoulder = 14, gripH = 26;
        double total = gripH * 2 + shoulder * 2 + gauge, y0 = top + (bottom - top - total) / 2;
        double wGrip = Math.Min(a.Width * 0.42, 54), wBody = wGrip * 0.55;
        double neck = !_m.Brittle && e > EpsU ? (e - EpsU) / Math.Max(1e-6, Ef - EpsU) : 0;
        double neckW = wBody * (1 - 0.45 * neck * (broken ? 1 : 0.9));
        double gTop = y0 + gripH + shoulder, gBot = gTop + gauge, mid = (gTop + gBot) / 2;

        dc.DrawRectangle(Grip, null, new Rect(cx - wGrip / 2 - 6, y0 - 6, wGrip + 12, gripH + 6));
        dc.DrawRectangle(Grip, null, new Rect(cx - wGrip / 2 - 6, gBot + shoulder, wGrip + 12, gripH + 6));
        Geometry Half(bool upper)
        {
            var g = new StreamGeometry();
            using (var c = g.Open())
            {
                double yEnd = upper ? y0 : gBot + shoulder + gripH, ySh = upper ? gTop - shoulder : gBot + shoulder, yG = upper ? gTop : gBot;
                double gap = broken ? (upper ? -3 : 3) : 0, yMid = mid + gap;
                c.BeginFigure(new Point(cx - wGrip / 2, yEnd), true, true);
                c.LineTo(new Point(cx + wGrip / 2, yEnd), true, false);
                c.LineTo(new Point(cx + wGrip / 2, ySh), true, false);
                c.QuadraticBezierTo(new Point(cx + wBody / 2, ySh), new Point(cx + wBody / 2, yG), true, false);
                c.QuadraticBezierTo(new Point(cx + wBody / 2, (yG + yMid) / 2), new Point(cx + neckW / 2, yMid), true, false);
                c.LineTo(new Point(cx - neckW / 2, yMid), true, false);
                c.QuadraticBezierTo(new Point(cx - wBody / 2, (yG + yMid) / 2), new Point(cx - wBody / 2, yG), true, false);
                c.QuadraticBezierTo(new Point(cx - wBody / 2, ySh), new Point(cx - wGrip / 2, ySh), true, false);
            }
            g.Freeze();
            return g;
        }
        dc.DrawGeometry(Steel, Outline, Half(true));
        dc.DrawGeometry(Steel, Outline, Half(false));
        // gauge marks
        dc.DrawLine(Pen(Mark, 1.5), new Point(cx - wBody / 2 - 6, gTop + 4), new Point(cx + wBody / 2 + 6, gTop + 4));
        dc.DrawLine(Pen(Mark, 1.5), new Point(cx - wBody / 2 - 6, gBot - 4), new Point(cx + wBody / 2 + 6, gBot - 4));
        dc.DrawText(Text($"L = {F(_l0 * (1 + e), "0.0")} mm", 10, Mark, _canvas), new Point(a.X + 4, mid - 8));
        double area = Math.PI * _d * _d / 4, f = _phase >= 0 ? Stress(e) * area / 1000 : 0;
        if (broken) f = 0;
        dc.DrawText(Text($"F = {F(f, "0.0")} kN ↕", 11, Ui.Text, _canvas, true), new Point(a.X + 8, a.Bottom - 20));
        if (broken) dc.DrawText(Text(L.T("rompeu!"), 11, Red, _canvas, true), new Point(cx + wBody / 2 + 4, mid - 7));
    }

    void DrawPlot(DrawingContext dc, Rect a)
    {
        dc.DrawText(Text(L.T("Tensão × deformação (curva de engenharia)"), 11, Ui.TextDim, _canvas), new Point(a.X + 10, a.Y + 3));
        var plot = new Rect(a.X + 48, a.Y + 36, a.Width - 64, a.Height - 62);
        double eMax = Math.Max(0.004, Ef * 1.1), sMax = Math.Ceiling(_m.Uts * 1.18 / 100) * 100;
        Point P(double e, double s) => new(plot.X + plot.Width * e / eMax, plot.Bottom - plot.Height * s / sMax);
        for (int i = 1; i <= 4; i++)
        {
            double s = sMax * i / 4;
            dc.DrawLine(GridPen, P(0, s), P(eMax, s));
            dc.DrawText(Text($"{s:0}", 9.5, Ui.TextDim, _canvas), new Point(a.X + 8, P(0, s).Y - 7));
            double e = eMax * i / 4;
            dc.DrawText(Text($"{F(e * 100, e < 0.02 ? "0.0#" : "0")}%", 9.5, Ui.TextDim, _canvas), new Point(P(e, 0).X - 10, plot.Bottom + 2));
        }
        dc.DrawLine(AxisPen, new Point(plot.X, plot.Bottom), new Point(plot.Right, plot.Bottom));
        dc.DrawLine(AxisPen, new Point(plot.X, plot.Y), new Point(plot.X, plot.Bottom));
        dc.DrawText(Text("σ (MPa)", 9.5, Ui.TextDim, _canvas), new Point(plot.X + 6, a.Y + 20));
        dc.DrawText(Text("ε", 11, Ui.TextDim, _canvas), new Point(plot.Right - 6, plot.Bottom + 12));
        if (_phase < 0) return;
        double eNow = StrainAt(Math.Min(_phase, 1));
        var g = new StreamGeometry();
        using (var c = g.Open())
        {
            c.BeginFigure(P(0, 0), false, false);
            int n = 500;
            for (int i = 1; i <= n; i++) { double e = eNow * i / n; c.LineTo(P(e, Stress(e)), true, false); }
        }
        g.Freeze();
        dc.DrawGeometry(null, CurvePen, g);
        if (_phase >= 1)
        {
            var m = _metrics;
            if (!_m.Brittle && m["yield"] > 0)
            {
                double ey = m["yield"] / (_m.E * 1000) + 0.002;
                dc.DrawLine(DashPen, P(0.002, 0), P(0.002 + 1.05 * m["yield"] / (_m.E * 1000), m["yield"] * 1.05));
                dc.DrawEllipse(Mark, null, P(ey, m["yield"]), 3.5, 3.5);
                dc.DrawText(Text(L.F("σe = {0:0} MPa", m["yield"]), 10.5, Mark, _canvas, true), new Point(P(ey, 0).X + 8, P(0, m["yield"]).Y + 2));
            }
            var pu = P(EpsU, _m.Uts);
            dc.DrawEllipse(Ui.Accent, null, pu, 3.5, 3.5);
            dc.DrawText(Text(L.F("σr = {0:0} MPa", _m.Uts), 10.5, Ui.Accent, _canvas, true), new Point(Math.Min(pu.X - 30, plot.Right - 100), pu.Y - 18));
            var pf = P(Ef, Stress(Ef));
            dc.DrawText(Text(L.T("✕ ruptura"), 10.5, Red, _canvas), new Point(Math.Min(pf.X - 4, plot.Right - 58), pf.Y + 4));
        }
    }
}
