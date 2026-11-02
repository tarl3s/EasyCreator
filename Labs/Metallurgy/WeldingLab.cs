using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using static EasyCreator.MetalKit;

namespace EasyCreator;

/// <summary>Welding simulator: the lesson fixes the process, metal, thickness, joint and position; the student sets the
/// machine (current / voltage / wire feed / gas…) and the hands (speed, angle, arc length). ▶ lays a bead on a plate seen
/// from above and then shows the cross-section (fusion zone, HAZ, defects). Physics in <see cref="WeldModel"/>.</summary>
[Lab("welding")]
public sealed class WeldingLab : ILab
{
    readonly StackPanel _root = new();
    readonly WeldSetup _setup = new();
    readonly WeldInputs _in = new();
    readonly Dictionary<string, SliderRow> _sliders = new();
    readonly Dictionary<string, ChoiceRow> _choices = new();
    readonly Paint _canvas;
    readonly TextBlock _numbers = new() { FontSize = 12.5, Foreground = Ui.Text, TextWrapping = TextWrapping.Wrap };
    readonly WrapPanel _chips = new() { Margin = new Thickness(0, 3, 0, 0) };
    readonly TextBlock _tip = new() { FontSize = 12.5, Foreground = Ui.TextDim, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 0) };
    readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
    readonly Stopwatch _clock = new();
    Button? _run;
    WeldResult? _shown;          // drawn on the canvas (during and after a run)
    double _progress;            // 0..1 along the plate
    bool _running, _stale, _syncing;
    int _seed = 1;
    Dictionary<string, double> _metrics = new();
    const double RunSeconds = 3.2;

    public WeldingLab()
    {
        _canvas = new Paint(Draw) { Height = 178 };
        _timer.Tick += (_, _) => Tick();
    }

    public FrameworkElement View => _root;
    public IReadOnlyDictionary<string, double> Metrics => _metrics;
    public event Action? Finished;

    public void Load(IReadOnlyDictionary<string, string> p)
    {
        _setup.Process = WeldSetup.NormProcess(Str(p, "process", "smaw"));
        _setup.Metal = WeldSetup.NormMetal(Str(p, "metal", "carbon"));
        _setup.Thickness = Math.Clamp(Num(p, "thickness", 6), 0.8, 50);
        _setup.Joint = WeldSetup.NormJoint(Str(p, "joint", "fillet"));
        _setup.Position = WeldSetup.NormPosition(Str(p, "position", "flat"));
        _setup.Carbon = Num(p, "carbon", 0.18);
        _setup.CE = Num(p, "ce", 0.38);

        // Deliberately poor starting values: the student has to find the window.
        switch (_setup.Process)
        {
            case "gmaw":
                _in.WireDiameter = 1.0; _in.WireFeed = 3; _in.Voltage = 26; _in.Stickout = 28; _in.TravelSpeed = 9;
                _in.Angle = 35; _in.Gas = "ar-co2"; _in.GasFlow = 5;
                break;
            case "gtaw":
                _in.Current = 60; _in.ArcLength = 6; _in.TravelSpeed = 4; _in.Angle = 35; _in.Gas = "ar"; _in.GasFlow = 3; _in.Polarity = "dcen";
                break;
            default:
                _in.Electrode = "E6013"; _in.Diameter = 3.25; _in.Current = 75; _in.ArcLength = 6; _in.TravelSpeed = 6; _in.Angle = 35;
                break;
        }
        _in.Preheat = 20;
        Apply(p, _in);   // a lesson may start some inputs elsewhere (same keys as SetInputs)
        Build(Str(p, "lock", ""));
    }

    static void Apply(IReadOnlyDictionary<string, string> d, WeldInputs x)
    {
        x.Current = Num(d, "current", x.Current);
        x.ArcLength = Num(d, "arcLength", x.ArcLength);
        x.Voltage = Num(d, "voltage", x.Voltage);
        x.WireFeed = Num(d, "wireFeed", x.WireFeed);
        x.WireDiameter = Num(d, "wireDiameter", x.WireDiameter);
        x.Stickout = Num(d, "stickout", x.Stickout);
        x.TravelSpeed = Num(d, "travelSpeed", x.TravelSpeed);
        x.Angle = Num(d, "angle", x.Angle);
        x.GasFlow = Num(d, "gasFlow", x.GasFlow);
        x.Diameter = Num(d, "diameter", x.Diameter);
        x.Preheat = Num(d, "preheat", x.Preheat);
        x.Gas = Str(d, "gas", x.Gas);
        x.Polarity = Str(d, "polarity", x.Polarity);
        if (d.TryGetValue("electrode", out var e) && !string.IsNullOrWhiteSpace(e)) x.Electrode = e.Trim().ToUpperInvariant();
    }

    // ---------------- UI ----------------

    void Build(string locked)
    {
        _root.Children.Clear();
        _sliders.Clear();
        _choices.Clear();
        var title = L.F("🔥 {0} · {1} {2} mm · {3} · posição {4}", _setup.ProcessName, _setup.MetalName, F(_setup.Thickness, "0.#"), _setup.JointName, _setup.PositionName);
        var (bar, run, _) = Header(title, L.T("▶ Soldar"));
        _run = run;
        run.Click += (_, _) => { if (_running) StopRun(); else StartRun(); };
        _root.Children.Add(bar);

        var canvasCard = new Border { Child = _canvas, CornerRadius = new CornerRadius(8), Background = Ui.Code, ClipToBounds = true };
        _root.Children.Add(canvasCard);

        var results = new StackPanel { Margin = new Thickness(0, 6, 0, 6) };
        results.Children.Add(_numbers);
        results.Children.Add(_chips);
        results.Children.Add(_tip);
        _root.Children.Add(results);

        var form = new Form();
        var lockSet = locked.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToHashSet(StringComparer.OrdinalIgnoreCase);
        void S(string key, string label, double min, double max, double step, double value, Func<double, string> fmt)
        {
            var row = new SliderRow(L.T(label), min, max, step, value, fmt, 100, 136);
            row.Changed += () => { if (_syncing) return; Read(); RefreshLabels(); MarkStale(); };
            if (lockSet.Contains(key)) row.Enabled = false;
            _sliders[key] = row;
            form.Add(row.Row);
        }
        void C(string key, string label, (string, string)[] options, string value, bool translate = true)
        {
            var row = new ChoiceRow(L.T(label), options.Select(o => (o.Item1, translate ? L.T(o.Item2) : o.Item2)), value, 100);
            row.Changed += () => { if (_syncing) return; Read(); RefreshLabels(); MarkStale(); };
            if (lockSet.Contains(key)) { row.Combo.IsEnabled = false; row.Row.Opacity = 0.55; }
            _choices[key] = row;
            form.Add(row.Row);
        }

        var gases = _setup.Process == "gtaw"
            ? new[] { ("ar", "Argônio puro"), ("ar-he", "Argônio + hélio"), ("ar-co2", "Ar + 20% CO₂"), ("none", "Sem gás") }
            : new[] { ("ar-co2", "Ar + 20% CO₂ (MAG)"), ("co2", "CO₂ puro (MAG)"), ("ar-o2", "Ar + 2% O₂"), ("ar", "Argônio puro (MIG)"), ("ar-he", "Ar + hélio (MIG)"), ("none", "Sem gás") };
        switch (_setup.Process)
        {
            case "gmaw":
                C("wireDiameter", "Arame", Mm("0.8", "1.0", "1.2"), WireKey(_in.WireDiameter), false);
                C("gas", "Gás", gases, _in.Gas);
                S("wireFeed", "Arame (veloc.)", 1.5, 15, 0.25, _in.WireFeed, v => $"{F(v, "0.0#")} m/min ≈ {WeldModel.MigCurrent(_in):0} A");
                S("voltage", "Tensão", 14, 34, 0.5, _in.Voltage, v => $"{F(v)} V");
                S("stickout", "Stick-out", 5, 35, 1, _in.Stickout, v => $"{v:0} mm");
                S("gasFlow", "Vazão de gás", 0, 30, 0.5, _in.GasFlow, v => $"{F(v)} L/min");
                break;
            case "gtaw":
                C("polarity", "Polaridade", new[] { ("dcen", "CC− (eletrodo negativo)"), ("ac", "CA (alternada)"), ("dcep", "CC+ (eletrodo positivo)") }, _in.Polarity);
                C("gas", "Gás", gases, _in.Gas);
                S("current", "Corrente", 10, 300, 5, _in.Current, CurrentText);
                S("arcLength", "Arco (compr.)", 0.5, 8, 0.25, _in.ArcLength, v => $"{F(v, "0.0#")} mm ≈ {F(WeldModel.ArcVoltage(_setup, _in))} V");
                S("gasFlow", "Vazão de gás", 0, 30, 0.5, _in.GasFlow, v => $"{F(v)} L/min");
                break;
            default:
                C("electrode", "Eletrodo", new[] { ("E6013", "E6013 (rutílico)"), ("E7018", "E7018 (básico, baixo H)"), ("E6010", "E6010 (celulósico)"), ("E308L", "E308L-16 (inox)") }, _in.Electrode);
                C("diameter", "Diâmetro", Mm("2.5", "3.25", "4.0"), DiaKey(_in.Diameter), false);
                S("current", "Corrente", 40, 220, 5, _in.Current, CurrentText);
                S("arcLength", "Arco (compr.)", 0.5, 8, 0.25, _in.ArcLength, v => $"{F(v, "0.0#")} mm ≈ {F(WeldModel.ArcVoltage(_setup, _in))} V");
                break;
        }
        S("travelSpeed", "Velocidade", 0.5, 15, 0.25, _in.TravelSpeed, v => $"{F(v, "0.0#")} mm/s ({v * 6:0} cm/min)");
        S("angle", _setup.Process == "smaw" ? "Ângulo eletrodo" : "Ângulo da tocha", 0, 45, 1, _in.Angle, v => _setup.Process == "smaw" ? L.F("{0:0}° (puxando)", v) : L.F("{0:0}° (empurrando)", v));
        S("preheat", "Pré-aquecer", 20, 300, 10, _in.Preheat, v => v <= 20 ? L.T("não (20 °C)") : $"{v:0} °C");
        _root.Children.Add(form.Root);
        ShowResults();
    }

    string CurrentText(double v)
    {
        var (lo, hi) = WeldModel.CurrentWindow(_setup, _in);
        return L.F("{0:0} A (faixa {1:0}–{2:0})", v, lo, hi);
    }

    /// <summary>Diameter options: value "3.25" shown as "3,25 mm" (or "3.25 mm" in English).</summary>
    static (string, string)[] Mm(params string[] values) => values.Select(v => (v, $"{F(double.Parse(v, System.Globalization.CultureInfo.InvariantCulture), "0.0#")} mm")).ToArray();

    static string WireKey(double d) => d < 0.9 ? "0.8" : d < 1.1 ? "1.0" : "1.2";
    static string DiaKey(double d) => d < 2.9 ? "2.5" : d < 3.6 ? "3.25" : "4.0";

    /// <summary>Controls → inputs.</summary>
    void Read()
    {
        foreach (var (k, row) in _sliders)
            switch (k)
            {
                case "current": _in.Current = row.Value; break;
                case "arcLength": _in.ArcLength = row.Value; break;
                case "voltage": _in.Voltage = row.Value; break;
                case "wireFeed": _in.WireFeed = row.Value; break;
                case "stickout": _in.Stickout = row.Value; break;
                case "travelSpeed": _in.TravelSpeed = row.Value; break;
                case "angle": _in.Angle = row.Value; break;
                case "gasFlow": _in.GasFlow = row.Value; break;
                case "preheat": _in.Preheat = row.Value; break;
            }
        foreach (var (k, row) in _choices)
            switch (k)
            {
                case "electrode": _in.Electrode = row.Value; break;
                case "diameter": _in.Diameter = Question.TryNum(row.Value, out var d) ? d : _in.Diameter; break;
                case "wireDiameter": _in.WireDiameter = Question.TryNum(row.Value, out var w) ? w : _in.WireDiameter; break;
                case "gas": _in.Gas = row.Value; break;
                case "polarity": _in.Polarity = row.Value; break;
            }
    }

    /// <summary>Inputs → controls (without firing a stale mark).</summary>
    void Write()
    {
        _syncing = true;
        void S(string k, double v) { if (_sliders.TryGetValue(k, out var r)) r.Value = v; }
        void C(string k, string v) { if (_choices.TryGetValue(k, out var r) && r.Has(v)) r.Value = v; }
        S("current", _in.Current); S("arcLength", _in.ArcLength); S("voltage", _in.Voltage); S("wireFeed", _in.WireFeed);
        S("stickout", _in.Stickout); S("travelSpeed", _in.TravelSpeed); S("angle", _in.Angle); S("gasFlow", _in.GasFlow);
        S("preheat", _in.Preheat);
        C("electrode", _in.Electrode); C("diameter", DiaKey(_in.Diameter)); C("wireDiameter", WireKey(_in.WireDiameter));
        C("gas", _in.Gas); C("polarity", _in.Polarity);
        _syncing = false;
        Read();
        RefreshLabels();
    }

    void RefreshLabels() { foreach (var r in _sliders.Values) r.Refresh(); }

    void MarkStale()
    {
        if (_shown == null || _running) return;
        _stale = true;
        ShowResults();
    }

    public void SetInputs(IReadOnlyDictionary<string, string> inputs)
    {
        Apply(inputs, _in);
        Write();
        _stale = _shown != null;
        ShowResults();
    }

    // ---------------- running ----------------

    WeldResult Compute()
    {
        Read();
        var r = WeldModel.Compute(_setup, _in);
        _seed = HashCode.Combine(Math.Round(r.Current), Math.Round(r.Voltage * 2), Math.Round(_in.TravelSpeed * 4), Math.Round(_in.Angle), _in.Gas, Math.Round(_in.GasFlow));
        return r;
    }

    void StartRun()
    {
        _shown = Compute();
        _progress = 0;
        _running = true;
        _stale = false;
        if (_run != null) _run.Content = L.T("■ Parar");
        ShowResults();
        _clock.Restart();
        _timer.Start();
    }

    void StopRun()
    {
        _timer.Stop();
        _running = false;
        if (_run != null) _run.Content = L.T("▶ Soldar");
        if (_progress < 1) { _shown = null; _progress = 0; }
        ShowResults();
        _canvas.InvalidateVisual();
    }

    void Tick()
    {
        _progress = Math.Min(1, _clock.Elapsed.TotalSeconds / RunSeconds);
        _canvas.InvalidateVisual();
        if (_progress < 1) return;
        _timer.Stop();
        _running = false;
        if (_run != null) _run.Content = L.T("▶ Soldar");
        _metrics = new Dictionary<string, double>(_shown!.Metrics);
        ShowResults();
        Finished?.Invoke();
    }

    public void RunToEnd()
    {
        _timer.Stop();
        _running = false;
        _shown = Compute();
        _progress = 1;
        _stale = false;
        _metrics = new Dictionary<string, double>(_shown.Metrics);
        if (_run != null) _run.Content = L.T("▶ Soldar");
        ShowResults();
        _canvas.InvalidateVisual();
    }

    public void Stop()
    {
        _timer.Stop();
        _running = false;
    }

    // ---------------- results text ----------------

    void ShowResults()
    {
        _chips.Children.Clear();
        var r = _shown;
        if (r == null || (_running && _progress < 1))
        {
            _numbers.Text = _running ? L.T("Soldando… observe o arco, a poça e os respingos.") : L.T("Ajuste a máquina e as mãos e clique em ▶ Soldar.");
            _tip.Text = _setup.Process switch
            {
                "gmaw" => L.T("No MIG/MAG a corrente vem da velocidade do arame; a tensão define o comprimento do arco."),
                "gtaw" => L.T("No TIG o arco é curto (≈ 1,5–3 mm) e o metal de adição entra pela outra mão."),
                _ => L.T("No eletrodo revestido o comprimento do arco (≈ diâmetro do eletrodo) define a tensão."),
            };
            return;
        }
        var parts = new List<string>
        {
            L.F("Aporte térmico {0} kJ/mm", F(r.HeatInput, "0.00")),
            $"{r.Current:0} A × {F(r.Voltage)} V",
            L.F("penetração {0} mm (precisa {1})", F(r.Penetration), F(r.Required)),
            L.F("largura {0} mm", F(r.Width)),
            L.F("altura {0} mm", F(r.Height)),
            $"t8/5 {F(r.CoolingTime)} s",
        };
        if (_setup.Metal == "carbon") parts.Add(L.F("ZTA {0:0} HV", r.HazHardness));
        if (r.Transfer.Length > 0) parts.Add(L.F("transferência {0}", L.T(r.Transfer)));
        _numbers.Text = string.Join(" · ", parts);
        Chip(L.F("Qualidade {0:0}/100", r.Quality), r.Quality >= 80 ? 0 : r.Quality >= 60 ? 0.3 : 1, true);
        Chip(L.F("Porosidade {0:0}%", r.Porosity * 100), r.Porosity);
        Chip(L.F("Mordedura {0:0}%", r.Undercut * 100), r.Undercut);
        Chip(L.F("Falta de fusão {0:0}%", r.LackOfFusion * 100), r.LackOfFusion);
        Chip(L.F("Perfuração {0:0}%", r.BurnThrough * 100), r.BurnThrough);
        Chip(L.F("Respingos {0:0}%", r.Spatter * 100), r.Spatter);
        Chip(L.F("Trinca {0:0}%", r.Crack * 100), r.Crack);
        if (r.Inclusion > 0.01) Chip(L.F("Inclusão {0:0}%", r.Inclusion * 100), r.Inclusion);
        string tip = _stale ? L.F("Você mudou os parâmetros: clique em ▶ Soldar de novo. {0}", r.Tip) : "💡 " + r.Tip;
        if (_setup.Metal == "carbon" && r.PreheatNeeded > r.Metrics.GetValueOrDefault("preheat", 20))
            tip += " " + L.F("(Pré-aquecimento sugerido pelo modelo: ≈ {0:0} °C.)", r.PreheatNeeded);
        _tip.Text = tip;
        _tip.Foreground = _stale ? Ui.TextDim : Ui.Text;
    }

    static readonly Brush ChipGood = Brush("#18B98A", 0.18), ChipWarn = Brush("#E0A030", 0.2), ChipBad = Brush("#FF5C6C", 0.2);
    static readonly Brush WarnText = Brush("#F0B44C");

    void Chip(string text, double level, bool bold = false)
    {
        var (bg, fg) = level < 0.15 ? (ChipGood, Ui.Good) : level < 0.4 ? (ChipWarn, WarnText) : (ChipBad, Ui.Bad);
        _chips.Children.Add(new Border
        {
            Background = bg, CornerRadius = new CornerRadius(4), Padding = new Thickness(6, 1, 6, 2), Margin = new Thickness(0, 2, 5, 2),
            Child = new TextBlock { Text = text, FontSize = 12, Foreground = fg, FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal },
        });
    }

    // ---------------- drawing ----------------

    static readonly Brush DimText = Ui.TextDim;
    static readonly Pen SectionPen = Pen(Brush("#A0A0AA", 0.8), 1, true);
    static readonly Pen EdgePen = Pen(Brush("#000000", 0.45), 1);
    static readonly Brush Pore = Brush("#15151A"), Hole = Brush("#050507"), SpatterDot = Brush("#2A2A30"), Slag = Brush("#55524A");
    static readonly Pen CrackPen = Pen(Brush("#FF5C6C"), 1.4), LofPen = Pen(Brush("#FF5C6C"), 2), UndercutPen = Pen(Brush("#0B0B0E"), 2.2);
    static readonly Brush WeldMetal = Brush("#C9A25E"), HazFill = Brush("#8A5FB8", 0.55), HotRim = Brush("#FF8A2A", 0.85);
    static readonly Pen WeldOutline = Pen(Brush("#6B4E1F"), 1), PlateOutline = Pen(Brush("#000000", 0.5), 1);

    Color PlateColor => _setup.Metal switch
    {
        "stainless" => Color.FromRgb(0x8C, 0x91, 0x9A), "aluminum" => Color.FromRgb(0xAE, 0xB5, 0xBF), _ => Color.FromRgb(0x5E, 0x66, 0x73),
    };

    void Draw(DrawingContext dc, double w, double h)
    {
        double split = Math.Round(w * 0.57);
        DrawTop(dc, new Rect(0, 0, split, h));
        dc.DrawRectangle(Ui.Border, null, new Rect(split, 6, 1, h - 12));
        DrawSection(dc, new Rect(split + 1, 0, w - split - 1, h));
    }

    void DrawTop(DrawingContext dc, Rect a)
    {
        var plateC = PlateColor;
        var plate = new Rect(a.X + 8, a.Y + 20, a.Width - 16, a.Height - 28);
        dc.DrawText(Text(L.T("Vista de cima"), 11, DimText, _canvas), new Point(a.X + 10, a.Y + 3));
        double cy = plate.Y + plate.Height / 2, mm = (plate.Width) / 80.0; // 80 mm of plate across
        var lighter = Brush(Mix(plateC, Colors.White, 0.12));
        var darker = Brush(Mix(plateC, Colors.Black, 0.25));
        var pb = Brush(plateC);
        switch (_setup.Joint)
        {
            case "butt":
                dc.DrawRectangle(pb, PlateOutline, new Rect(plate.X, plate.Y, plate.Width, plate.Height / 2 - 1.5));
                dc.DrawRectangle(pb, PlateOutline, new Rect(plate.X, cy + 1.5, plate.Width, plate.Height / 2 - 1.5));
                break;
            case "lap":
                dc.DrawRectangle(pb, PlateOutline, plate);
                dc.DrawRectangle(lighter, PlateOutline, new Rect(plate.X, plate.Y, plate.Width, plate.Height / 2));
                dc.DrawRectangle(darker, null, new Rect(plate.X, cy, plate.Width, 2));
                break;
            default: // fillet: the standing plate seen from above is the lighter band above the joint line
                dc.DrawRectangle(pb, PlateOutline, plate);
                double band = Math.Clamp(_setup.Thickness * mm, 6, plate.Height / 2 - 4);
                dc.DrawRectangle(lighter, PlateOutline, new Rect(plate.X, cy - band, plate.Width, band));
                dc.DrawText(Text(L.T("chapa em pé"), 10, Brush("#FFFFFF", 0.6), _canvas), new Point(plate.X + 4, cy - band + 1));
                break;
        }

        // section mark A–A at 60 % of the bead
        double x0 = plate.X + 6, x1 = plate.Right - 6, xs = x0 + (x1 - x0) * 0.6;
        dc.DrawLine(SectionPen, new Point(xs, plate.Y - 4), new Point(xs, plate.Bottom + 2));
        dc.DrawText(Text("A", 10, DimText, _canvas), new Point(xs + 3, plate.Y - 15));

        var r = _shown;
        if (r == null) return;
        double tip = x0 + (x1 - x0) * _progress;
        double bw = Math.Clamp(r.Width * mm, 4, plate.Height * 0.8), half = bw / 2;
        var rng = new Random(_seed);
        double hazPx = Math.Min(r.HazWidth * mm, 30);

        // heat tint (straw → blue) around the bead, behind it
        if (tip > x0)
        {
            var tint = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };
            tint.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0x4A, 0x6F, 0xB5), 0));
            tint.GradientStops.Add(new GradientStop(Color.FromArgb(110, 0x4A, 0x6F, 0xB5), 0.18));
            tint.GradientStops.Add(new GradientStop(Color.FromArgb(150, 0xC8, 0xA0, 0x50), 0.4));
            tint.GradientStops.Add(new GradientStop(Color.FromArgb(150, 0xC8, 0xA0, 0x50), 0.6));
            tint.GradientStops.Add(new GradientStop(Color.FromArgb(110, 0x4A, 0x6F, 0xB5), 0.82));
            tint.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0x4A, 0x6F, 0xB5), 1));
            dc.DrawRectangle(tint, null, new Rect(x0, cy - half - hazPx, tip - x0, bw + 2 * hazPx));
        }

        // bead: body with temperature gradient (hot near the arc), ripples every few px
        if (tip > x0 + 1)
        {
            var cold = Mix(PlateColor, Color.FromRgb(0xD8, 0xD0, 0xC0), 0.35);
            var body = new LinearGradientBrush { StartPoint = new Point(x0, 0), EndPoint = new Point(tip, 0), MappingMode = BrushMappingMode.Absolute };
            double span = Math.Max(1, tip - x0), glowLen = Math.Min(110, span) / span;
            body.GradientStops.Add(new GradientStop(cold, 0));
            body.GradientStops.Add(new GradientStop(cold, Math.Max(0, 1 - glowLen)));
            body.GradientStops.Add(new GradientStop(Color.FromRgb(0xB5, 0x45, 0x2B), Math.Max(0, 1 - glowLen * 0.45)));
            body.GradientStops.Add(new GradientStop(Color.FromRgb(0xFF, 0x9A, 0x3C), Math.Max(0, 1 - glowLen * 0.15)));
            body.GradientStops.Add(new GradientStop(Color.FromRgb(0xFF, 0xF3, 0xB0), 1));
            double lofShrink = r.LackOfFusion * 0.25 * half;
            dc.DrawRoundedRectangle(body, EdgePen, new Rect(x0, cy - half + lofShrink, span, bw - 2 * lofShrink), half * 0.6, half * 0.6);
            var ripple = Pen(Brush("#000000", 0.18), 1);
            double stepPx = Math.Max(3, bw * 0.28);
            for (double x = x0 + stepPx; x < tip - 2; x += stepPx)
            {
                var g = new StreamGeometry();
                using (var c = g.Open())
                {
                    c.BeginFigure(new Point(x - stepPx * 1.4, cy - half * 0.85 + lofShrink), false, false);
                    c.QuadraticBezierTo(new Point(x + stepPx * 0.6, cy), new Point(x - stepPx * 1.4, cy + half * 0.85 - lofShrink), true, false);
                }
                g.Freeze();
                dc.DrawGeometry(null, ripple, g);
            }
        }

        // defects (only where the bead has already passed)
        int pores = (int)Math.Round(r.Porosity * 16);
        for (int i = 0; i < pores; i++)
        {
            double x = x0 + rng.NextDouble() * (x1 - x0), y = cy + (rng.NextDouble() - 0.5) * bw * 0.6, rad = 1.2 + rng.NextDouble() * 1.8;
            if (x < tip - 12) dc.DrawEllipse(Pore, Pen(Brush("#E6E6EA", 0.35), 0.6), new Point(x, y), rad, rad);
        }
        int undercuts = (int)Math.Round(r.Undercut * 9);
        for (int i = 0; i < undercuts; i++)
        {
            double x = x0 + rng.NextDouble() * (x1 - x0), len = 8 + rng.NextDouble() * 22, side = rng.Next(2) == 0 ? -1 : 1;
            double xe = Math.Min(x + len, tip - 10);
            if (xe > x) dc.DrawLine(UndercutPen, new Point(x, cy + side * (half + 1.5)), new Point(xe, cy + side * (half + 1.5)));
        }
        if (r.LackOfFusion > 0.25)
        {
            int n = (int)Math.Round(r.LackOfFusion * 6);
            for (int i = 0; i < n; i++)
            {
                double x = x0 + rng.NextDouble() * (x1 - x0), len = 12 + rng.NextDouble() * 30, side = rng.Next(2) == 0 ? -1 : 1;
                double xe = Math.Min(x + len, tip - 10), y = cy + side * (half - r.LackOfFusion * 0.25 * half);
                if (xe > x) dc.DrawLine(Pen(Brush("#FF5C6C", 0.8), 1.3), new Point(x, y), new Point(xe, y));
            }
        }
        int slag = (int)Math.Round(r.Inclusion * 6);
        for (int i = 0; i < slag; i++)
        {
            double x = x0 + rng.NextDouble() * (x1 - x0), y = cy + (rng.NextDouble() - 0.5) * bw * 0.4;
            if (x < tip - 12) dc.DrawEllipse(Slag, null, new Point(x, y), 3.5, 1.4);
        }
        if (r.BurnThrough > 0.25)
        {
            int n = Math.Max(1, (int)Math.Round(r.BurnThrough * 4));
            for (int i = 0; i < n; i++)
            {
                double x = x0 + 20 + rng.NextDouble() * (x1 - x0 - 40), rad = Math.Max(3, half * (0.35 + 0.4 * r.BurnThrough));
                if (x < tip - 14)
                {
                    dc.DrawEllipse(HotRim, null, new Point(x, cy), rad + 2, rad * 0.8 + 2);
                    dc.DrawEllipse(Hole, null, new Point(x, cy), rad, rad * 0.8);
                }
            }
        }
        if (r.Crack > 0.35)
        {
            double len = (x1 - x0) * Math.Min(0.8, r.Crack * 0.7), start = x0 + (x1 - x0) * 0.1;
            double end = Math.Min(start + len, tip - 12);
            var g = new StreamGeometry();
            using (var c = g.Open())
            {
                c.BeginFigure(new Point(start, cy + half + hazPx * 0.35), false, false);
                for (double x = start; x < end; x += 5) c.LineTo(new Point(x, cy + half + hazPx * 0.35 + (rng.NextDouble() - 0.5) * 3), true, false);
            }
            g.Freeze();
            if (end > start) dc.DrawGeometry(null, CrackPen, g);
        }
        int dots = (int)Math.Round(r.Spatter * 70);
        for (int i = 0; i < dots; i++)
        {
            double x = x0 + rng.NextDouble() * (x1 - x0), side = rng.Next(2) == 0 ? -1 : 1;
            double y = cy + side * (half + 3 + rng.NextDouble() * 28);
            if (x < tip - 6 && y > plate.Y && y < plate.Bottom) dc.DrawEllipse(SpatterDot, null, new Point(x, y), 1.3, 1.3);
        }

        // the arc, the electrode/torch and the sparks
        if (_running && _progress < 1)
        {
            var flicker = 0.85 + 0.3 * Random.Shared.NextDouble();
            double rad = (14 + bw * 0.4) * flicker;
            var glow = new RadialGradientBrush();
            glow.GradientStops.Add(new GradientStop(Colors.White, 0));
            glow.GradientStops.Add(new GradientStop(Color.FromRgb(0xFF, 0xF0, 0xA0), 0.25));
            glow.GradientStops.Add(new GradientStop(Color.FromArgb(120, 0xFF, 0x9A, 0x3C), 0.55));
            glow.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0xFF, 0x9A, 0x3C), 1));
            var tool = Pen(Brush(_setup.Process == "smaw" ? "#7C6A4A" : "#3A3A44"), _setup.Process == "smaw" ? 4 : 9);
            dc.DrawLine(tool, new Point(tip - 26, cy - 46), new Point(tip - 3, cy - 4));
            dc.DrawEllipse(glow, null, new Point(tip, cy), rad, rad);
            int sparks = 3 + (int)(r.Spatter * 30);
            var sparkPen = Pen(Brush("#FFB347"), 1.2);
            for (int i = 0; i < sparks; i++)
            {
                double ang = Random.Shared.NextDouble() * Math.PI * 2, d0 = rad * 0.5, d1 = d0 + 6 + Random.Shared.NextDouble() * (10 + 40 * r.Spatter);
                dc.DrawLine(sparkPen, new Point(tip + Math.Cos(ang) * d0, cy + Math.Sin(ang) * d0), new Point(tip + Math.Cos(ang) * d1, cy + Math.Sin(ang) * d1));
            }
        }
    }

    void DrawSection(DrawingContext dc, Rect a)
    {
        dc.DrawText(Text(L.T("Corte A–A"), 11, DimText, _canvas), new Point(a.X + 10, a.Y + 3));
        var r = _shown;
        double t = _setup.Thickness;
        bool showWeld = r != null && _progress >= 0.6;
        double W = r?.Width ?? 6, H = showWeld ? Math.Max(0.3, r!.Height) : 0;
        double leg = Math.Sqrt(2 * Math.Max(1, r?.Deposit ?? 8)) * 0.95;
        bool fillet = _setup.Joint is "fillet" or "lap";
        double widthMm = Math.Max(fillet ? 3.2 * leg + t : 3.2 * W, 18);
        double aboveMm = _setup.Joint switch { "fillet" => Math.Max(leg * 1.6, t * 1.2), "lap" => Math.Max(leg, t) * 1.4, _ => Math.Max(H, 1) + 1 };
        double belowMm = t + (r != null && r.Penetration > t ? 2 : 0.8);
        double s = Math.Min((a.Width - 24) / widthMm, (a.Height - 40) / (aboveMm + belowMm));
        double spare = Math.Max(0, a.Height - 40 - (aboveMm + belowMm) * s);
        double ox = a.X + a.Width / 2 + (fillet ? -leg * s * 0.4 : 0), oy = a.Y + 20 + spare / 2 + aboveMm * s;
        var plateG = PlateGeometry(t, widthMm, aboveMm, leg);
        var toPx = new MatrixTransform(s, 0, 0, s, ox, oy);
        Geometry Px(Geometry g) { var c = g.Clone(); c.Transform = toPx; return c; }

        dc.PushClip(new RectangleGeometry(new Rect(a.X, a.Y + 16, a.Width, a.Height - 16)));
        if (!showWeld)
        {
            dc.DrawGeometry(Brush(PlateColor), PlateOutline, Px(plateG));
            if (r == null || _running)
                dc.DrawText(Text(r == null ? L.T("o corte aparece depois de soldar") : L.T("esperando o cordão passar por A…"), 10.5, DimText, _canvas), new Point(a.X + 10, a.Bottom - 16));
            dc.Pop();
            return;
        }

        var rng = new Random(_seed + 7);
        var weld = WeldGeometry(r!, t, leg, plateG);
        var haz = Geometry.Combine(weld.GetWidenedPathGeometry(new Pen(Brushes.Black, 2 * r!.HazWidth)), plateG, GeometryCombineMode.Intersect, null);
        dc.DrawGeometry(Brush(PlateColor), PlateOutline, Px(plateG));
        dc.DrawGeometry(HazFill, null, Px(haz));
        // burn-through: the weld sags out under the plate and leaves a hole
        dc.DrawGeometry(WeldMetal, WeldOutline, Px(weld));
        if (r.BurnThrough > 0.4)
        {
            var hole = new EllipseGeometry(new Point(fillet ? leg * 0.3 : 0, t * 0.55), Math.Max(0.6, W * 0.22), t * 0.5);
            dc.DrawGeometry(Hole, null, Px(hole));
        }

        // lack of fusion: the unfused root line
        if (r.Penetration < r.Required * 0.95)
        {
            double pe = Math.Min(r.Penetration, t);
            var (p0, p1) = _setup.Joint switch
            {
                "butt" => (new Point(0, pe), new Point(0, t)),
                "fillet" => (new Point(-t, 0), new Point(-pe * 0.7, 0)),
                _ => (new Point(-leg * 1.2, 0), new Point(-pe * 0.5, 0)),
            };
            dc.DrawLine(LofPen, toPx.Transform(p0), toPx.Transform(p1));
        }
        // pores inside the weld metal
        var bounds = weld.Bounds;
        int pores = (int)Math.Round(r.Porosity * 8);
        for (int i = 0, tries = 0; i < pores && tries < 200; tries++)
        {
            var pt = new Point(bounds.X + rng.NextDouble() * bounds.Width, bounds.Y + rng.NextDouble() * bounds.Height);
            if (!weld.FillContains(pt)) continue;
            i++;
            double rad = Math.Clamp((0.2 + rng.NextDouble() * 0.35) * s, 1.2, Math.Max(1.2, Math.Min(bounds.Height, bounds.Width) * s * 0.18));
            dc.DrawEllipse(Pore, null, toPx.Transform(pt), rad, rad);
        }
        if (r.Inclusion > 0.3)
        {
            var c = toPx.Transform(new Point(bounds.X + bounds.Width * 0.45, bounds.Y + bounds.Height * 0.7));
            dc.DrawEllipse(Slag, null, c, Math.Max(2.5, 0.5 * s), Math.Max(1.2, 0.2 * s));
        }
        // undercut notches at the toes
        if (r.Undercut > 0.15)
        {
            double d = Math.Max(0.25, r.Undercut * 0.9);
            var toes = _setup.Joint switch
            {
                "butt" => new[] { new Point(-W / 2, 0), new Point(W / 2, 0) },
                "lap" => new[] { new Point(leg, 0) },
                _ => new[] { new Point(leg, 0) },
            };
            foreach (var toe in toes)
            {
                var g = new StreamGeometry();
                using (var c = g.Open())
                {
                    c.BeginFigure(toPx.Transform(new Point(toe.X - d * 1.2, 0)), true, true);
                    c.LineTo(toPx.Transform(new Point(toe.X + d * 1.2, 0)), true, false);
                    c.LineTo(toPx.Transform(new Point(toe.X, d)), true, false);
                }
                g.Freeze();
                dc.DrawGeometry(Ui.Code, null, g);
            }
        }
        // underbead crack in the HAZ
        if (r.Crack > 0.35)
        {
            double y0 = Math.Min(r.Penetration, t) + r.HazWidth * 0.5, x = fillet ? leg * 0.4 : -W * 0.25;
            var g = new StreamGeometry();
            using (var c = g.Open())
            {
                c.BeginFigure(toPx.Transform(new Point(x, y0)), false, false);
                for (int i = 1; i <= 6; i++) c.LineTo(toPx.Transform(new Point(x + i * W * 0.1, y0 + (i % 2 == 0 ? -0.25 : 0.25))), true, false);
            }
            g.Freeze();
            dc.DrawGeometry(null, CrackPen, g);
        }
        dc.Pop();

        // labels
        var lbl = Brush("#E6E6EA", 0.9);
        var zf = toPx.Transform(fillet ? new Point(leg * 0.3, -leg * 0.3) : new Point(0, -H * 0.4));
        dc.DrawText(Text(L.T("ZF"), 10, Brush("#2A1E08"), _canvas, true), new Point(zf.X - 7, zf.Y - 7));
        var zt = toPx.Transform(new Point(fillet ? leg + r.HazWidth * 0.6 : W / 2 + r.HazWidth * 0.5, Math.Min(t * 0.25 + 0.4, t - 0.3)));
        dc.DrawText(Text(L.T("ZTA"), 10, lbl, _canvas, true), new Point(zt.X + 2, zt.Y - 6));
        dc.DrawText(Text(L.T("MB"), 10, lbl, _canvas), toPx.Transform(new Point(-widthMm / 2 + 1, t * 0.5 - 0.4)));
        dc.DrawText(Text(L.F("p = {0} mm · e = {1} mm", F(r.Penetration), F(t, "0.#")), 10.5, DimText, _canvas), new Point(a.X + 10, a.Bottom - 16));
    }

    /// <summary>The plates around the joint in mm (origin at the weld root/surface, y down into the base plate).</summary>
    Geometry PlateGeometry(double t, double widthMm, double aboveMm, double leg)
    {
        double half = widthMm / 2 + 2;
        var g = new GeometryGroup { FillRule = FillRule.Nonzero };
        switch (_setup.Joint)
        {
            case "butt":
                g.Children.Add(new RectangleGeometry(new Rect(-half, 0, half - 0.05, t)));
                g.Children.Add(new RectangleGeometry(new Rect(0.05, 0, half, t)));
                break;
            case "lap":
                g.Children.Add(new RectangleGeometry(new Rect(-half, 0, 2 * half, t)));
                g.Children.Add(new RectangleGeometry(new Rect(-half, -t, half, t)));
                break;
            default:
                g.Children.Add(new RectangleGeometry(new Rect(-half, 0, 2 * half, t)));
                g.Children.Add(new RectangleGeometry(new Rect(-t, -aboveMm - 4, t, aboveMm + 4)));
                break;
        }
        return Geometry.Combine(g, Geometry.Empty, GeometryCombineMode.Union, null);
    }

    /// <summary>Weld metal (reinforcement + fusion zone) in mm.</summary>
    Geometry WeldGeometry(WeldResult r, double t, double leg, Geometry plates)
    {
        double W = r.Width, H = Math.Max(0.3, r.Height), P = Math.Max(0.2, r.Penetration);
        Geometry result;
        if (_setup.Joint == "butt")
        {
            var cap = new StreamGeometry();
            using (var c = cap.Open())
            {
                c.BeginFigure(new Point(-W / 2, 0.01), true, true);
                c.QuadraticBezierTo(new Point(0, -2 * H), new Point(W / 2, 0.01), true, false);
            }
            Geometry fusion;
            if (t <= 6) fusion = new EllipseGeometry(new Point(0, 0), W / 2 * 0.85, Math.Min(P, t + 0.01));
            else
            {
                double g = (t - 1.5) * Math.Tan(Math.PI / 6) + 0.75, m = Math.Clamp(P * 0.3, 0.3, 1.5);
                double rootBottom = t - 1.5 + Math.Clamp(P / 2.0, 0, 1) * 1.5;
                var v = new StreamGeometry();
                using (var c = v.Open())
                {
                    c.BeginFigure(new Point(-g - m, 0), true, true);
                    c.LineTo(new Point(g + m, 0), true, false);
                    c.LineTo(new Point(0.75 + m, t - 1.5), true, false);
                    c.LineTo(new Point(0.75, rootBottom), true, false);
                    c.LineTo(new Point(-0.75, rootBottom), true, false);
                    c.LineTo(new Point(-0.75 - m, t - 1.5), true, false);
                }
                fusion = v;
                W = Math.Max(W, 2 * (g + m));
                cap = new StreamGeometry();
                using (var c = cap.Open())
                {
                    c.BeginFigure(new Point(-g - m - 0.5, 0.01), true, true);
                    c.QuadraticBezierTo(new Point(0, -2 * H), new Point(g + m + 0.5, 0.01), true, false);
                }
            }
            result = Geometry.Combine(cap, Geometry.Combine(fusion, plates, GeometryCombineMode.Intersect, null), GeometryCombineMode.Union, null);
            if (P > t)
            {
                var root = new EllipseGeometry(new Point(0, t), Math.Min(W * 0.3, 2.5), Math.Min(1.2, (P - t) * 0.8 + 0.3));
                result = Geometry.Combine(result, root, GeometryCombineMode.Union, null);
            }
            return result;
        }
        // fillet (T) and lap: a triangular bead in the corner + fusion into both members
        double up = _setup.Joint == "lap" ? Math.Min(leg, t) : leg;
        double bulge = Math.Clamp((H / Math.Max(1, W) - 0.25) * leg, -0.15 * leg, 0.35 * leg);
        var bead = new StreamGeometry();
        using (var c = bead.Open())
        {
            c.BeginFigure(new Point(0, 0), true, true);
            c.LineTo(new Point(leg, 0), true, false);
            c.QuadraticBezierTo(new Point(leg / 2 + bulge, -up / 2 - bulge), new Point(0, -up), true, false);
        }
        var fz = new GeometryGroup { FillRule = FillRule.Nonzero };
        fz.Children.Add(new EllipseGeometry(new Point(0, 0), Math.Min(P, t * 1.1), Math.Min(P, t * 1.1)));
        fz.Children.Add(new EllipseGeometry(new Point(leg / 2, 0), leg / 2 + 0.2, Math.Min(P * 0.7, t * 1.05)));
        fz.Children.Add(new EllipseGeometry(new Point(0, -up / 2), Math.Min(P * 0.7, t), up / 2));
        return Geometry.Combine(bead, Geometry.Combine(fz, plates, GeometryCombineMode.Intersect, null), GeometryCombineMode.Union, null);
    }
}
