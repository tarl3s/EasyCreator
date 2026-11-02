using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Complex = System.Numerics.Complex;

namespace EasyCreator.Electrical;

/// <summary>Circuit simulator lab. labParams:
/// <list type="bullet">
/// <item>"netlist" — the circuit (format in Netlist.cs), "layout" — drawing positions ("n1=0,0 n2=3,0").</item>
/// <item>"mode" — "dc" (default), "tran" (transient: "tstop" s, "steps", oscilloscope "scope" = "V(n2);I(R1)" up to 3,
///   "probe" = trace used for tau/ripple, default the first) or "ac" (phasors at "f" Hz; "scope" shows the waves of
///   two periods; "sweep" = "I(V1)" plots that magnitude from "fmin" to "fmax" Hz instead).</item>
/// <item>"adjust" — the student's controls, ';'-separated: "R1 10..100k" (log slider, E24 values for R/C/L),
///   "C1 0..300u lin step=5u", "f 100..10k", "S1" (switch), "Rch 2.93=Inverno|4.61=Verão" (choices).
///   Input keys = element names or "f"; values with SI prefixes ("4.7k"), switches "on"/"off".</item>
/// <item>"meter" — what the multimeter shows first (element name or "@node").</item>
/// </list>
/// Metrics: V(node), V(X), I(X), P(X) for every node/element (DC: values; transient: at the end; AC: RMS magnitudes,
/// plus ph(node)/ph(X) degrees and Q(X) var), Ptotal (power delivered by the sources), Req (first V source, DC),
/// AC: Qtotal, S, pf, phase (angle of the impedance seen by the first source, + = inductive), Z, f;
/// transient probe: tau (time to 63.2 % of the change), Vfinal, and over the last 30 %: Vmax, Vmin, Vavg, ripple.</summary>
[Lab("circuit")]
public sealed class CircuitLab : ILab
{
    sealed class Adjust
    {
        public string Key = "";
        public string Kind = "range";   // range | choice | switch
        public double Min, Max, Step;
        public bool Log, E24;
        public List<(double value, string label)> Choices = new();
        public Elem? Elem;
        public Action? Refresh;
    }

    readonly Border _root = new();
    readonly SchematicView _sch = new();
    readonly ScopeView _scope = new();
    readonly TextBlock _status = Ui.Label("", 12.5, Ui.TextDim);
    readonly TextBlock _readout = new() { FontSize = 13, Foreground = Ui.Text, TextWrapping = TextWrapping.Wrap };
    readonly List<Adjust> _adj = new();
    DispatcherTimer? _anim;

    Netlist _net = new();
    string _mode = "dc", _probe = "", _sweep = "", _error = "";
    double _f = 60, _tstop = 1, _fmin = 10, _fmax = 100_000;
    int _steps = 4000;
    List<string> _traces = new();
    Dictionary<string, double> _metrics = new();

    public FrameworkElement View => _root;
    public IReadOnlyDictionary<string, double> Metrics => _metrics;
    public event Action? Finished;

    bool HasPlot => _mode == "tran" || (_mode == "ac" && (_traces.Count > 0 || _sweep.Length > 0));

    // ───────────────────────── setup ─────────────────────────

    public void Load(IReadOnlyDictionary<string, string> labParams)
    {
        string P(string k, string d = "") => labParams.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v) ? v.Trim() : d;
        _net = Netlist.Parse(P("netlist"));
        _mode = P("mode", "dc").ToLowerInvariant() switch { "tran" or "transient" or "transitorio" => "tran", "ac" => "ac", _ => "dc" };
        _f = SI.Parse(P("f", "60"), 60);
        _tstop = Math.Max(SI.Parse(P("tstop", "1"), 1), 1e-9);
        _steps = Math.Clamp(int.TryParse(P("steps", "4000"), out var st) ? st : 4000, 100, 20000);
        _traces = P("scope").Split(';').Select(s => s.Trim()).Where(s => s.Length > 0).Take(3).ToList();
        _probe = P("probe", _traces.FirstOrDefault() ?? "");
        _sweep = P("sweep");
        _fmin = SI.Parse(P("fmin", "10"), 10);
        _fmax = SI.Parse(P("fmax", "100k"), 100_000);
        _sch.Build(_net, P("layout"));
        _sch.AcSources = _mode == "ac";
        _sch.ValueText = ValueText;
        _sch.Glow = Glow;
        _sch.Picked += Pick;
        ParseAdjust(P("adjust"));
        _sch.Selected = P("meter", _adj.FirstOrDefault(a => a.Elem != null)?.Elem!.Name
            ?? _net.Elems.FirstOrDefault(e => e.Kind != ElemKind.W)?.Name ?? "");
        BuildUi();
        Recompute(false, false);
    }

    void ParseAdjust(string spec)
    {
        _adj.Clear();
        foreach (var raw in spec.Split(';'))
        {
            var item = raw.Trim();
            if (item.Length == 0) continue;
            int sp = item.IndexOfAny(new[] { ' ', '\t' });
            string key = sp < 0 ? item : item[..sp], rest = sp < 0 ? "" : item[(sp + 1)..].Trim();
            var a = new Adjust { Key = key, Elem = key.Equals("f", StringComparison.OrdinalIgnoreCase) ? null : _net.Find(key) };
            if (a.Elem == null && !key.Equals("f", StringComparison.OrdinalIgnoreCase)) continue;
            if (a.Elem != null) a.Key = a.Elem.Name;
            if (a.Elem?.Kind == ElemKind.S) a.Kind = "switch";
            else if (rest.Contains('|'))
            {
                a.Kind = "choice";
                foreach (var c in rest.Split('|'))
                {
                    int eq = c.IndexOf('=');
                    var v = SI.Parse(eq < 0 ? c : c[..eq]);
                    a.Choices.Add((v, eq < 0 ? c.Trim() : c[(eq + 1)..].Trim()));
                }
            }
            else
            {
                var m = Regex.Match(rest, @"(\S+?)\s*\.\.\s*(\S+)");
                if (!m.Success) continue;
                a.Min = SI.Parse(m.Groups[1].Value);
                a.Max = SI.Parse(m.Groups[2].Value);
                if (a.Max <= a.Min) continue;
                bool lin = Regex.IsMatch(rest, @"\blin\b");
                a.Log = !lin && a.Min > 0 && a.Max / a.Min >= 20;
                a.E24 = a.Log && a.Elem?.Kind is ElemKind.R or ElemKind.C or ElemKind.L;
                var sm = Regex.Match(rest, @"step=(\S+)");
                a.Step = sm.Success ? SI.Parse(sm.Groups[1].Value) : NiceStep((a.Max - a.Min) / 100);
            }
            _adj.Add(a);
            if (a.Elem != null) _sch.Adjustable.Add(a.Elem.Name);
        }
    }

    static double NiceStep(double raw)
    {
        double p = Math.Pow(10, Math.Floor(Math.Log10(raw)));
        double m = raw / p;
        return (m < 1.5 ? 1 : m < 3.5 ? 2 : m < 7.5 ? 5 : 10) * p;
    }

    double Get(Adjust a) => a.Elem == null ? _f : a.Elem.Kind == ElemKind.S ? (a.Elem.On ? 1 : 0) : a.Elem.Value;

    void Set(Adjust a, double v)
    {
        if (a.Kind == "range") v = Snap(a, Math.Clamp(v, a.Min, a.Max));
        if (a.Elem == null) _f = v;
        else if (a.Elem.Kind == ElemKind.S) a.Elem.On = v != 0;
        else a.Elem.Value = v;
    }

    static double Snap(Adjust a, double v)
    {
        if (a.E24) return Math.Clamp(SI.SnapE24(v), a.Min, a.Max);
        if (a.Log) return SI.Round3(v);
        if (a.Step > 0) v = a.Min + Math.Round((v - a.Min) / a.Step) * a.Step;
        return Math.Round(v, 12);
    }

    double Frac(Adjust a, double v) => a.Log
        ? Math.Log(Math.Max(v, a.Min) / a.Min) / Math.Log(a.Max / a.Min)
        : (v - a.Min) / (a.Max - a.Min);

    double FromFrac(Adjust a, double f) => a.Log ? a.Min * Math.Pow(a.Max / a.Min, f) : a.Min + f * (a.Max - a.Min);

    double StepFrom(Adjust a, double v, int dir)
    {
        if (a.E24) return SI.StepE24(v, dir);
        if (a.Log) return v * Math.Pow(1.01, dir);
        return v + dir * a.Step;
    }

    // ───────────────────────── UI ─────────────────────────

    void BuildUi()
    {
        var stack = new StackPanel();

        var header = new DockPanel { Margin = new Thickness(0, 0, 0, 6), LastChildFill = true };
        var run = Ui.Button(_mode == "tran" ? L.T("▶ Simular") : L.T("▶ Medir"), Ui.Accent);
        run.Padding = new Thickness(12, 5, 12, 5);
        run.Margin = new Thickness(8, 0, 0, 0);
        run.Click += (_, _) => Recompute(_mode == "tran", true);
        DockPanel.SetDock(run, Dock.Right);
        header.Children.Add(run);
        var titles = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        string mode = _mode switch { "tran" => L.T("transitório"), "ac" => L.T("corrente alternada"), _ => L.T("corrente contínua") };
        var title = new TextBlock { FontSize = 14.5, Foreground = Ui.Text, FontWeight = FontWeights.SemiBold, Text = L.F("⚡ Simulador de circuitos · {0}", mode) };
        titles.Children.Add(title);
        titles.Children.Add(_status);
        header.Children.Add(titles);
        stack.Children.Add(header);

        _sch.Height = HasPlot ? 200 : 250;
        _sch.ClipToBounds = true;
        stack.Children.Add(_sch);
        if (HasPlot)
        {
            _scope.Height = 110;
            _scope.Margin = new Thickness(0, 6, 0, 0);
            stack.Children.Add(_scope);
        }

        if (_adj.Count > 0)
        {
            var grid = new UniformGrid { Columns = Math.Min(2, _adj.Count), Margin = new Thickness(0, 8, 0, 0) };
            foreach (var a in _adj) grid.Children.Add(Control(a));
            stack.Children.Add(grid);
        }

        stack.Children.Add(new Border
        {
            Background = Ui.Bg2, CornerRadius = new CornerRadius(6), Padding = new Thickness(10, 6, 10, 6),
            Margin = new Thickness(0, 8, 0, 0), Child = _readout,
        });
        _root.Child = stack;
    }

    string AdjustLabel(Adjust a) => a.Elem == null ? L.T("Frequência") : a.Elem.DisplayName;

    FrameworkElement Control(Adjust a)
    {
        var row = new Grid { Margin = new Thickness(0, 2, 10, 2) };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var label = Ui.Label(AdjustLabel(a), 13, Ui.TextDim, true);
        label.VerticalAlignment = VerticalAlignment.Center;
        label.MinWidth = 34;
        label.Margin = new Thickness(0, 0, 8, 0);
        label.TextWrapping = TextWrapping.NoWrap;
        row.Children.Add(label);
        FrameworkElement body;

        if (a.Kind == "switch")
        {
            var b = Ui.Button("");
            b.Padding = new Thickness(10, 4, 10, 4);
            b.HorizontalAlignment = HorizontalAlignment.Left;
            b.Click += (_, _) => { Set(a, Get(a) != 0 ? 0 : 1); a.Refresh?.Invoke(); Changed(true); };
            a.Refresh = () => b.Content = Get(a) != 0 ? L.T("● fechada (conduz)") : L.T("○ aberta (não conduz)");
            body = b;
        }
        else if (a.Kind == "choice")
        {
            var combo = new ComboBox { FontSize = 13, MinWidth = 160, HorizontalAlignment = HorizontalAlignment.Stretch };
            foreach (var (v, l) in a.Choices) combo.Items.Add(new ComboBoxItem { Content = L.T(l), Tag = v });
            bool quiet = false;
            combo.SelectionChanged += (_, _) =>
            {
                if (quiet || combo.SelectedItem is not ComboBoxItem { Tag: double v }) return;
                Set(a, v); Changed(true);
            };
            a.Refresh = () =>
            {
                quiet = true;
                double cur = Get(a);
                int best = 0;
                for (int i = 1; i < a.Choices.Count; i++)
                    if (Math.Abs(a.Choices[i].value - cur) < Math.Abs(a.Choices[best].value - cur)) best = i;
                combo.SelectedIndex = a.Choices.Count > 0 ? best : -1;
                quiet = false;
            };
            body = combo;
        }
        else
        {
            var g = new Grid();
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 60 });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(74) });
            Button Small(string t, int dir)
            {
                var b = Ui.Button(t);
                b.Padding = new Thickness(6, 2, 6, 2);
                b.Margin = new Thickness(dir < 0 ? 0 : 4, 0, dir < 0 ? 4 : 0, 0);
                b.FontSize = 11;
                b.Click += (_, _) => { Set(a, StepFrom(a, Get(a), dir)); a.Refresh?.Invoke(); Changed(true); };
                return b;
            }
            var minus = Small("◀", -1);
            var slider = new SliderBar { VerticalAlignment = VerticalAlignment.Center };
            var plus = Small("▶", 1);
            var value = new TextBlock { FontSize = 13, Foreground = Ui.Accent, FontWeight = FontWeights.SemiBold, VerticalAlignment = VerticalAlignment.Center, TextAlignment = TextAlignment.Right };
            Grid.SetColumn(slider, 1); Grid.SetColumn(plus, 2); Grid.SetColumn(value, 3);
            g.Children.Add(minus); g.Children.Add(slider); g.Children.Add(plus); g.Children.Add(value);
            slider.Changed += (f, final) =>
            {
                Set(a, FromFrac(a, f));
                a.Refresh?.Invoke();
                if (final || _mode != "tran") Changed(final);
                else _sch.InvalidateVisual();
            };
            slider.Stepped += dir => { Set(a, StepFrom(a, Get(a), dir)); a.Refresh?.Invoke(); Changed(true); };
            a.Refresh = () =>
            {
                double v = Get(a);
                slider.Fraction = Frac(a, v);
                slider.InvalidateVisual();
                value.Text = SI.Format(v, a.Elem?.Unit ?? "Hz");
            };
            body = g;
        }
        a.Refresh?.Invoke();
        Grid.SetColumn(body, 1);
        row.Children.Add(body);
        return row;
    }

    /// <summary>A control changed: re-solve (live), and on a final change animate (transient) and raise Finished.</summary>
    void Changed(bool final)
    {
        if (!final) { Recompute(false, false); return; }
        Recompute(_mode == "tran", true);
    }

    void Pick(string what)
    {
        var el = what.StartsWith('@') ? null : _net.Find(what);
        var adj = el == null ? null : _adj.FirstOrDefault(a => a.Elem == el && a.Kind == "switch");
        if (adj != null)
        {
            Set(adj, Get(adj) != 0 ? 0 : 1);
            adj.Refresh?.Invoke();
            _sch.Selected = what;
            Changed(true);
            return;
        }
        _sch.Selected = what;
        UpdateView();
    }

    // ───────────────────────── ILab ─────────────────────────

    public void SetInputs(IReadOnlyDictionary<string, string> inputs)
    {
        foreach (var (k, raw) in inputs)
        {
            var a = _adj.FirstOrDefault(x => x.Key.Equals(k, StringComparison.OrdinalIgnoreCase));
            var el = k.Equals("f", StringComparison.OrdinalIgnoreCase) ? null : _net.Find(k);
            double v;
            var s = (raw ?? "").Trim().ToLowerInvariant();
            if (s is "on" or "true" or "fechada" or "ligada" or "1" && (el?.Kind == ElemKind.S)) v = 1;
            else if (s is "off" or "false" or "aberta" or "desligada" or "0" && (el?.Kind == ElemKind.S)) v = 0;
            else if (!SI.TryParse(raw ?? "", out v)) continue;
            if (a != null) { Set(a, v); a.Refresh?.Invoke(); }
            else if (k.Equals("f", StringComparison.OrdinalIgnoreCase)) _f = v;
            else if (el != null) { if (el.Kind == ElemKind.S) el.On = v != 0; else el.Value = v; }
        }
        _sch.InvalidateVisual();
    }

    public void RunToEnd() => Recompute(false, false);

    public void Stop() => _anim?.Stop();

    // ───────────────────────── simulation ─────────────────────────

    void Recompute(bool animate, bool notify)
    {
        _anim?.Stop();
        _error = "";
        var m = new Dictionary<string, double>();
        try
        {
            if (_net.Errors.Count > 0) _error = L.F("Erro no circuito da lição: {0}", _net.Errors[0]);
            else if (_net.Elems.Count == 0) _error = L.T("Esta lição não tem circuito.");
            else if (_mode == "ac") RunAc(m);
            else if (_mode == "tran") RunTran(m);
            else RunDc(m);
        }
        catch (Exception ex) { _error = L.F("A simulação falhou: {0}", ex.Message); }
        if (_error.Length > 0) m.Clear();
        _metrics = m;
        UpdateView();

        bool live = animate && PresentationSource.FromVisual(_root) != null && _error.Length == 0;
        if (!live)
        {
            _scope.Reveal = 1;
            _scope.InvalidateVisual();
            if (notify) Finished?.Invoke();
            return;
        }
        _scope.Reveal = 0;
        _anim ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
        _anim.Tick -= AnimTick;
        _anim.Tick += AnimTick;
        _notifyAfterAnim = notify;
        _anim.Start();
    }

    bool _notifyAfterAnim;

    void AnimTick(object? sender, EventArgs e)
    {
        _scope.Reveal = Math.Min(1, _scope.Reveal + 0.035);
        _scope.InvalidateVisual();
        if (_scope.Reveal < 1) return;
        _anim?.Stop();
        if (_notifyAfterAnim) Finished?.Invoke();
    }

    void RunDc(Dictionary<string, double> m)
    {
        var sim = new CircuitSim(_net);
        if (!sim.SolveDc()) { _error = sim.Error; return; }
        FillReal(sim, m);
        var src = _net.Elems.FirstOrDefault(e => e.Kind == ElemKind.V);
        if (src != null && Math.Abs(src.Id) > 1e-15) m["Req"] = src.Vd / src.Id;
    }

    void FillReal(CircuitSim sim, Dictionary<string, double> m)
    {
        foreach (var n in _net.Names) if (!Netlist.IsGround(n)) m[$"V({n})"] = sim.V(n);
        double ptot = 0;
        foreach (var e in _net.Elems)
        {
            if (e.Kind == ElemKind.W) continue;
            m[$"V({e.Name})"] = e.Vd;
            m[$"I({e.Name})"] = e.Id;
            double p = e.Vd * e.Id;
            m[$"P({e.Name})"] = p;
            if (e.IsSource) ptot += p;
        }
        m["Ptotal"] = ptot;
    }

    /// <summary>"V(n2)", "V(C1)", "I(R1)", "P(R1)" → reads the simulator's current state.</summary>
    Func<double>? Probe(CircuitSim sim, string expr, out string unit)
    {
        unit = "";
        var mt = Regex.Match(expr ?? "", @"^\s*([VIPvip])\(\s*([^)\s]+)\s*\)\s*$");
        if (!mt.Success) return null;
        char k = char.ToUpperInvariant(mt.Groups[1].Value[0]);
        string x = mt.Groups[2].Value;
        var el = _net.Find(x);
        unit = k == 'V' ? "V" : k == 'I' ? "A" : "W";
        if (k == 'V' && el == null) return _net.Node.ContainsKey(x) ? () => sim.V(x) : null;
        if (el == null) return null;
        return k switch { 'V' => () => el.Vd, 'I' => () => el.Id, _ => () => el.Vd * el.Id };
    }

    Func<Complex>? ProbeAc(CircuitSim sim, string expr, out string unit)
    {
        unit = "";
        var mt = Regex.Match(expr ?? "", @"^\s*([VIvi])\(\s*([^)\s]+)\s*\)\s*$");
        if (!mt.Success) return null;
        bool v = char.ToUpperInvariant(mt.Groups[1].Value[0]) == 'V';
        string x = mt.Groups[2].Value;
        var el = _net.Find(x);
        unit = v ? "V" : "A";
        if (v && el == null) return _net.Node.ContainsKey(x) ? () => sim.Vac(x) : null;
        if (el == null) return null;
        return v ? () => el.Vac : () => el.Iac;
    }

    void RunTran(Dictionary<string, double> m)
    {
        var sim = new CircuitSim(_net);
        var series = new List<(string name, string unit, Func<double> f, List<double> y)>();
        foreach (var t in _traces)
        {
            var f = Probe(sim, t, out var u);
            if (f == null) { _error = L.F("Sonda desconhecida: {0}", t); return; }
            series.Add((t, u, f, new List<double>(_steps + 1)));
        }
        var pf = _probe.Length > 0 ? Probe(sim, _probe, out _) : null;
        var py = new List<double>(_steps + 1);
        var time = new List<double>(_steps + 1);
        bool ok = sim.RunTransient(_tstop, _steps, t =>
        {
            time.Add(t);
            foreach (var s in series) s.y.Add(s.f());
            if (pf != null) py.Add(pf());
        });
        _scope.XUnit = "s"; _scope.LogX = false; _scope.Marker = null;
        _scope.Data.Clear();
        if (!ok) { _error = sim.Error; _scope.X = Array.Empty<double>(); return; }
        _scope.X = time.ToArray();
        for (int i = 0; i < series.Count; i++)
            _scope.Data.Add(new ScopeView.Series { Name = series[i].name, Unit = series[i].unit, Color = ScopeView.Colors[i], Y = series[i].y.ToArray() });
        FillReal(sim, m);
        _tauFit = false;
        if (py.Count > 2) ProbeMetrics(time, py, m, out _tauFit);
    }

    /// <summary>True when the last τ came from fitting an exponential (the curve had not settled yet).</summary>
    bool _tauFit;

    /// <summary>τ, Vfinal and, over the last 30 % of the run, Vmax/Vmin/Vavg/ripple of the probe trace.
    /// τ is the time to 63.2 % of the change only when the trace has settled by the end; otherwise 63.2 % of an
    /// unfinished change would give a τ that is too short (an RC of 0.47 s cut at 1 s read 0.38 s), so τ comes from the
    /// exponential through the points at 0, T/2 and T — exact for a first-order step — and is left out when the curve is
    /// not such an exponential.</summary>
    static void ProbeMetrics(List<double> t, List<double> y, Dictionary<string, double> m, out bool fitted)
    {
        fitted = false;
        double y0 = y[0], yF = y[^1], T = t[^1];
        m["Vfinal"] = yF;
        double At(double time)
        {
            int i = t.FindIndex(x => x >= time);
            if (i <= 0) return i == 0 ? y[0] : y[^1];
            return y[i - 1] + (time - t[i - 1]) / (t[i] - t[i - 1]) * (y[i] - y[i - 1]);
        }
        double swing = yF - y0;
        if (Math.Abs(swing) > 1e-9 * (1 + Math.Abs(yF)))
        {
            bool settled = Math.Abs(yF - At(0.9 * T)) <= 1e-3 * Math.Abs(swing);
            if (settled)
            {
                double target = y0 + 0.632 * swing, sgn = Math.Sign(swing);
                for (int i = 1; i < y.Count; i++)
                    if ((y[i] - target) * sgn >= 0)
                    {
                        double a = y[i - 1], b = y[i];
                        double frac = Math.Abs(b - a) < 1e-300 ? 0 : (target - a) / (b - a);
                        m["tau"] = t[i - 1] + frac * (t[i] - t[i - 1]);
                        break;
                    }
            }
            else
            {
                // y = y∞ + (y0 − y∞)·e^(−t/τ): with q = e^(−T/2τ), (y(T) − y0) / (y(T/2) − y0) = 1 + q.
                double half = At(T / 2) - y0, q = Math.Abs(half) < 1e-300 ? -1 : swing / half - 1;
                if (q > 1e-9 && q < 1)
                {
                    double tau = -T / 2 / Math.Log(q), full = half / (1 - q);
                    // Only a first-order curve: the fit must also pass through the trace at T/4 and 3T/4.
                    bool fits = new[] { 0.25, 0.75 }.All(k => Math.Abs(y0 + full * (1 - Math.Exp(-k * T / tau)) - At(k * T)) <= 0.01 * Math.Abs(full));
                    if (fits) { m["tau"] = tau; fitted = true; }
                }
            }
        }
        int start = (int)(y.Count * 0.7);
        double max = double.MinValue, min = double.MaxValue, sum = 0;
        for (int i = start; i < y.Count; i++) { max = Math.Max(max, y[i]); min = Math.Min(min, y[i]); sum += y[i]; }
        m["Vmax"] = max; m["Vmin"] = min; m["Vavg"] = sum / (y.Count - start); m["ripple"] = max - min;
    }

    void RunAc(Dictionary<string, double> m)
    {
        var sim = new CircuitSim(_net);
        _scope.Data.Clear(); _scope.Marker = null;
        if (_sweep.Length > 0)
        {
            var pr = ProbeAc(sim, _sweep, out var unit);
            if (pr == null) { _error = L.F("Sonda desconhecida: {0}", _sweep); return; }
            const int N = 241;
            var xs = new double[N]; var ys = new double[N];
            double lmin = Math.Log10(Math.Max(_fmin, 1e-3)), lmax = Math.Log10(Math.Max(_fmax, _fmin * 10));
            for (int i = 0; i < N; i++)
            {
                xs[i] = Math.Pow(10, lmin + (lmax - lmin) * i / (N - 1));
                if (!sim.SolveAc(xs[i])) { _error = sim.Error; return; }
                ys[i] = Complex.Abs(pr());
            }
            _scope.X = xs; _scope.LogX = true; _scope.XUnit = "Hz"; _scope.Marker = _f;
            _scope.Data.Add(new ScopeView.Series { Name = L.F("{0} × frequência", _sweep), Unit = unit, Color = ScopeView.Colors[0], Y = ys });
        }
        if (!sim.SolveAc(_f)) { _error = sim.Error; return; }
        FillAc(sim, m);
        if (_sweep.Length == 0 && _traces.Count > 0)
        {
            const int N = 241;
            double T = 2 / Math.Max(_f, 1e-6), w = 2 * Math.PI * _f;
            var xs = new double[N];
            for (int i = 0; i < N; i++) xs[i] = T * i / (N - 1);
            _scope.X = xs; _scope.LogX = false; _scope.XUnit = "s";
            for (int k = 0; k < _traces.Count; k++)
            {
                var pr = ProbeAc(sim, _traces[k], out var unit);
                if (pr == null) { _error = L.F("Sonda desconhecida: {0}", _traces[k]); return; }
                var ph = pr();
                var ys = xs.Select(t => Math.Sqrt(2) * ph.Magnitude * Math.Cos(w * t + ph.Phase)).ToArray();
                _scope.Data.Add(new ScopeView.Series { Name = _traces[k], Unit = unit, Color = ScopeView.Colors[k], Y = ys });
            }
        }
    }

    static double Deg(double rad) => rad * 180 / Math.PI;

    void FillAc(CircuitSim sim, Dictionary<string, double> m)
    {
        foreach (var n in _net.Names)
        {
            if (Netlist.IsGround(n)) continue;
            var v = sim.Vac(n);
            m[$"V({n})"] = v.Magnitude;
            m[$"ph({n})"] = Deg(v.Phase);
        }
        var stot = Complex.Zero;
        foreach (var e in _net.Elems)
        {
            if (e.Kind == ElemKind.W) continue;
            m[$"V({e.Name})"] = e.Vac.Magnitude;
            m[$"I({e.Name})"] = e.Iac.Magnitude;
            m[$"ph({e.Name})"] = Deg(e.Vac.Phase);
            var s = e.Vac * Complex.Conjugate(e.Iac);
            m[$"P({e.Name})"] = s.Real;
            m[$"Q({e.Name})"] = s.Imaginary;
            if (e.IsSource) stot += s;
        }
        m["Ptotal"] = stot.Real;
        m["Qtotal"] = stot.Imaginary;
        m["S"] = stot.Magnitude;
        m["pf"] = stot.Magnitude > 1e-12 ? Math.Abs(stot.Real) / stot.Magnitude : 1;
        m["f"] = _f;
        var src = _net.Elems.FirstOrDefault(e => e.Kind == ElemKind.V);
        if (src != null && src.Iac.Magnitude > 1e-15)
        {
            var z = src.Vac / src.Iac;
            m["Z"] = z.Magnitude;
            m["phase"] = Deg(z.Phase);
        }
    }

    // ───────────────────────── view ─────────────────────────

    string ValueText(Elem e)
    {
        bool ac = _mode == "ac";
        switch (e.Kind)
        {
            case ElemKind.M: return _error.Length > 0 ? "—" : ac ? SI.Format(e.Vac.Magnitude, "V") : SI.Format(e.Vd, "V");
            case ElemKind.A: return _error.Length > 0 ? "—" : ac ? SI.Format(e.Iac.Magnitude, "A") : SI.Format(e.Id, "A");
            case ElemKind.V:
                if (ac) return SI.Format(e.Value, "V") + (Math.Abs(e.PhaseDeg) > 0.01 ? $" ∠{e.PhaseDeg:0}°" : " ~");
                if (e.Wave == "sin") return L.F("{0} pico · {1}", SI.Format(e.Value, "V"), SI.Format(e.Freq, "Hz"));
                if (e.Wave == "sq") return $"0–{SI.Format(e.Value, "V")} · {SI.Format(e.Freq, "Hz")}";
                return SI.Format(e.Value, "V");
            case ElemKind.I: return SI.Format(e.Value, "A");
            case ElemKind.D: return e.Led ? "LED" : "";
            case ElemKind.S: return e.On ? L.T("fechada") : L.T("aberta");
            case ElemKind.W: return "";
            default: return SI.Format(e.Value, e.Unit);
        }
    }

    double Glow(Elem e)
    {
        if (_error.Length > 0) return 0;
        if (e.Kind == ElemKind.D) return (_mode == "ac" ? e.Iac.Magnitude : e.Id) / 0.02;
        double p = _mode == "ac" ? (e.Vac * Complex.Conjugate(e.Iac)).Real : e.Vd * e.Id;
        return p / (e.Watts > 0 ? e.Watts : 1);
    }

    void UpdateView()
    {
        _status.Foreground = _error.Length > 0 ? Ui.Bad : Ui.TextDim;
        _status.Text = _error.Length > 0 ? "⚠ " + _error : StatusText();
        _readout.Inlines.Clear();
        _readout.Foreground = Ui.Text;
        if (_error.Length > 0)
        {
            _readout.Foreground = Ui.Bad;
            _readout.Text = "⚠ " + _error;
        }
        else
        {
            _readout.Inlines.Add(new System.Windows.Documents.Run(L.T("🔎 Multímetro") + "  ") { Foreground = Ui.TextDim });
            _readout.Inlines.Add(new System.Windows.Documents.Run(MeterText()) { FontWeight = FontWeights.SemiBold });
        }
        _sch.InvalidateVisual();
        _scope.Empty = _error.Length > 0 ? L.T("⚠ sem resultado") : L.T("Clique em ▶ para simular.");
        _scope.InvalidateVisual();
    }

    string StatusText()
    {
        double Get(string k) => _metrics.TryGetValue(k, out var v) ? v : double.NaN;
        return _mode switch
        {
            "ac" => L.F("f = {0} · fator de potência {1} · P = {2} · Q = {3}", SI.Format(_f, "Hz"), PfText(Get("pf"), Get("Qtotal")),
                SI.Format(Get("Ptotal"), "W"), SI.Format(Get("Qtotal"), "var")),
            "tran" => TranStatus(),
            _ => L.F("Potência fornecida pelas fontes: {0}", SI.Format(Get("Ptotal"), "W")),
        };
    }

    /// <summary>"0,80 indutivo" (current lagging, Q &gt; 0), "0,95 capacitivo" (leading, Q &lt; 0) or just "1,00".</summary>
    static string PfText(double pf, double q)
    {
        string v = SI.Plain(pf);
        if (double.IsNaN(pf) || pf > 0.9995) return v;
        return q > 0 ? L.F("{0} indutivo", v) : L.F("{0} capacitivo", v);
    }

    /// <summary>Step responses (only DC sources) report τ; circuits driven by waves report average and ripple.</summary>
    string TranStatus()
    {
        double Get(string k) => _metrics.TryGetValue(k, out var v) ? v : double.NaN;
        if (_probe.Length == 0) return L.F("Simulado de 0 a {0}", SI.Format(_tstop, "s"));
        string u = UnitOf(_probe);
        bool waves = _net.Elems.Any(e => e.IsSource && e.Wave != "dc");
        if (waves) return L.F("{0} no fim: média {1} · ondulação {2} (máx − mín)", _probe, SI.Format(Get("Vavg"), u), SI.Format(Get("ripple"), u));
        if (!_metrics.ContainsKey("tau")) return L.F("{0}: valor final {1}", _probe, SI.Format(Get("Vfinal"), u));
        return _tauFit
            ? L.F("{0}: τ ≈ {1} (estimado pela curva: ainda não estabilizou) · valor no fim {2}", _probe, SI.Format(Get("tau"), "s"), SI.Format(Get("Vfinal"), u))
            : L.F("{0}: τ (63,2 %) = {1} · valor final {2}", _probe, SI.Format(Get("tau"), "s"), SI.Format(Get("Vfinal"), u));
    }

    /// <summary>" ∠-37,0°", or nothing for a (practically) zero phasor.</summary>
    static string Ang(Complex c) => c.Magnitude < 1e-9 ? "" : $" ∠{SI.Plain(Deg(c.Phase), 1)}°";

    static string UnitOf(string probe) => probe.TrimStart().StartsWith("I", StringComparison.OrdinalIgnoreCase) ? "A" : probe.TrimStart().StartsWith("P", StringComparison.OrdinalIgnoreCase) ? "W" : "V";

    string MeterText()
    {
        var sel = _sch.Selected ?? "";
        bool ac = _mode == "ac";
        string when = _mode == "tran" ? "  " + L.F("(em t = {0})", SI.Format(_tstop, "s")) : "";
        if (sel.StartsWith('@'))
        {
            var n = sel[1..];
            if (ac)
                return _metrics.TryGetValue($"V({n})", out var vm)
                    ? L.F("Nó {0}: {1} (em relação ao terra)", n, SI.Format(vm, "V") + (vm < 1e-9 ? "" : $" ∠{SI.Plain(_metrics[$"ph({n})"], 1)}°"))
                    : L.F("Nó {0}", n);
            return _metrics.TryGetValue($"V({n})", out var v) ? L.F("Nó {0}: {1} em relação ao terra", n, SI.Format(v, "V")) + when : L.F("Nó {0}", n);
        }
        var e = _net.Find(sel);
        if (e == null) return L.T("clique num nó ou componente do circuito para medir.");
        // Sources report what they deliver (current out of +); every other part, what it absorbs.
        string p = e.IsSource ? L.T("P fornecida") : "P", q = e.IsSource ? L.T("Q fornecida") : "Q";
        if (ac)
        {
            var s = e.Vac * Complex.Conjugate(e.Iac);
            return $"{e.DisplayName}: V = {SI.Format(e.Vac.Magnitude, "V")}{Ang(e.Vac)} · I = {SI.Format(e.Iac.Magnitude, "A")}{Ang(e.Iac)}"
                + $" · {p} = {SI.Format(s.Real, "W")} · {q} = {SI.Format(s.Imaginary, "var")}";
        }
        return $"{e.DisplayName}: V = {SI.Format(e.Vd, "V")} · I = {SI.Format(e.Id, "A")} · {p} = {SI.Format(e.Vd * e.Id, "W")}{when}";
    }
}
