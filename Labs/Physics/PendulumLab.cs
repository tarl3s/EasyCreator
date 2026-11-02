using System.Windows;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>Simple pendulum (point mass on a massless rigid rod), released from rest at the amplitude angle. Solves the
/// full non-linear equation θ'' = −(g/L)·sin θ − b·θ' with RK4, so large amplitudes show the longer period.
/// Metrics: period (s, measured), frequency (Hz), smallAnglePeriod (s, 2π√(L/g)), energy (J, m·g·L·(1 − cos θ0)),
/// maxSpeed (m/s), amplitudeEnd (°, at the end of the run), g (m/s²), plus the inputs (length, amplitude, mass, damping).</summary>
[Lab("pendulum")]
public sealed class PendulumLab : PhysLab
{
    readonly List<(double t, double th, double w)> _s = new();
    double _tSim, _period, _vMax;

    public PendulumLab()
    {
        Num("length", "Comprimento do fio L", "m", 0.1, 5, 2, 0.01, "0.00");
        Num("amplitude", "Amplitude (ângulo inicial)", "°", 1, 80, 40, 1, "0");
        Num("mass", "Massa", "kg", 0.1, 5, 1, 0.1, "0.0");
        Num("damping", "Amortecimento b", "1/s", 0, 1, 0, 0.01, "0.00");
        Opt("planet", "Planeta", "earth", Planets);
    }

    protected override string ValueLabel(Input i) =>
        i.Key == "planet" ? $"g = {F(Gravity(i.Choice), "0.00")} m/s²" : base.ValueLabel(i);

    protected override void Compute()
    {
        double g = Gravity(C("planet")), L = V("length"), th0 = Rad(V("amplitude")), b = V("damping");
        double t0 = 2 * Math.PI * Math.Sqrt(L / g);
        _tSim = Math.Clamp(4 * t0, 6, 40);
        double th = th0, w = 0, t = 0, dt = Math.Min(0.001, t0 / 2000);
        _s.Clear();
        _s.Add((0, th, w));
        var crossings = new List<double>();
        _vMax = 0;
        int n = (int)(_tSim / dt), every = Math.Max(1, n / 1200);
        double Acc(double a, double av) => -(g / L) * Math.Sin(a) - b * av;
        for (int i = 1; i <= n; i++)
        {
            double k1a = w, k1w = Acc(th, w);
            double k2a = w + k1w * dt / 2, k2w = Acc(th + k1a * dt / 2, w + k1w * dt / 2);
            double k3a = w + k2w * dt / 2, k3w = Acc(th + k2a * dt / 2, w + k2w * dt / 2);
            double k4a = w + k3w * dt, k4w = Acc(th + k3a * dt, w + k3w * dt);
            double nth = th + dt * (k1a + 2 * k2a + 2 * k3a + k4a) / 6, nw = w + dt * (k1w + 2 * k2w + 2 * k3w + k4w) / 6;
            if (th != 0 && Math.Sign(nth) != Math.Sign(th)) crossings.Add(t + dt * th / (th - nth));
            th = nth; w = nw; t += dt;
            _vMax = Math.Max(_vMax, Math.Abs(w) * L);
            if (i % every == 0) _s.Add((t, th, w));
        }
        _period = PeriodFromCrossings(crossings);
        double tail = _s.Skip(_s.Count * 3 / 4).Max(p => Math.Abs(p.th));
        M["period"] = _period;
        M["frequency"] = double.IsInfinity(_period) ? 0 : 1 / _period;
        M["smallAnglePeriod"] = t0;
        M["energy"] = V("mass") * g * L * (1 - Math.Cos(th0));
        M["maxSpeed"] = _vMax;
        M["amplitudeEnd"] = tail * 180 / Math.PI;
        M["g"] = g;
        AnimSeconds = Math.Min(_tSim, 10);
    }

    (double t, double th, double w) Now()
    {
        if (!HasResult) return (0, Rad(V("amplitude")), 0);
        int i = (int)Math.Clamp(Math.Round(Progress * (_s.Count - 1)), 0, _s.Count - 1);
        return _s[i];
    }

    protected override string LiveText()
    {
        var p = Now();
        return $"t = {F(p.t, "0.00")} s   θ = {F(p.th * 180 / Math.PI, "0.0")}°   v = {F(Math.Abs(p.w) * V("length"), "0.00")} m/s";
    }

    protected override IEnumerable<(string, string)> Results()
    {
        yield return ("Período medido", F(_period, "0.000") + " s");
        yield return ("2π√(L/g)", F(M["smallAnglePeriod"], "0.000") + " s");
        yield return ("Frequência", F(M["frequency"], "0.000") + " Hz");
        yield return ("Energia", F(M["energy"], "0.00") + " J");
        yield return ("Vel. máxima", F(_vMax, "0.00") + " m/s");
    }

    protected override void Paint(DrawingContext dc)
    {
        double L = V("length"), m = V("mass");
        var (t, th, _) = Now();
        double px = 170, py = 16, len = 40 + 150 * Math.Sqrt(L / 5);
        dc.DrawRectangle(Ui.Bg3, null, new Rect(px - 50, py - 8, 100, 8));
        // amplitude guide
        double a0 = Rad(V("amplitude"));
        Line(dc, Faint, px, py, px, py + len + 14, 1, true);
        Line(dc, Faint, px, py, px + Math.Sin(a0) * len, py + Math.Cos(a0) * len, 1, true);
        Text(dc, $"{F(V("amplitude"), "0")}°", px + 8, py + 20, 11, Ui.TextDim);
        double bx = px + Math.Sin(th) * len, by = py + Math.Cos(th) * len;
        Line(dc, Ui.Text, px, py, bx, by, 1.6);
        Dot(dc, Ui.Text, px, py, 3);
        double r = 7 + 5 * Math.Cbrt(m);
        Dot(dc, Orange, bx, by, r);
        Text(dc, $"L = {F(L, "0.00")} m", 8, 4, 12, Ui.TextDim);
        Text(dc, $"g = {F(Gravity(C("planet")), "0.00")} m/s²", 8, 20, 12, Ui.TextDim);

        double cx = 330, cw = 300, ctop = 20, ch = 188;
        Chart(dc, cx, ctop, cw, ch, "ângulo θ(t)", "tempo");
        if (!HasResult) { Text(dc, "rode para ver o gráfico", cx + cw / 2, ctop + ch / 2 + 8, 12, Faint, 0.5); return; }
        int count = (int)(Progress * (_s.Count - 1)) + 1;
        var curve = _s.Select(p => new Point(cx + p.t / _tSim * cw, ctop + ch / 2 - p.th / a0 * (ch / 2 - 8))).ToList();
        Polyline(dc, P(Orange, 1.8), curve, count);
        Text(dc, $"{F(_tSim, "0.#")} s", cx + cw - 2, ctop + ch - 16, 11, Ui.TextDim, 1);
        if (Progress >= 1 && !double.IsInfinity(_period))
            Text(dc, $"T = {F(_period, "0.000")} s", cx + cw, ctop - 17, 12, Ui.Text, 1, true);
    }
}
