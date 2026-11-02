using System.Windows;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>Mass on a horizontal ideal spring (massless spring, no friction with the floor), released from rest at
/// x = amplitude, optional viscous damping F = −b·v. m·x'' = −k·x − b·x', integrated with RK4.
/// Metrics: period (s, measured from zero crossings), frequency (Hz), omega (rad/s, √(k/m)), energy (J, ½·k·A²),
/// maxSpeed (m/s, measured), energyEnd (J, at the end of the run), overshoot (% of the amplitude the mass
/// passes beyond x = 0), settleTime (s, last time |x| was above 2 % of the amplitude; ∞ if it never settles in the run), plus the inputs (mass, k, amplitude, damping).</summary>
[Lab("spring")]
public sealed class SpringLab : PhysLab
{
    readonly List<(double t, double x, double v)> _s = new();
    double _tSim, _period, _vMax;

    public SpringLab()
    {
        Num("mass", "Massa", "kg", 0.1, 5, 1, 0.1, "0.0");
        Num("k", "Constante da mola k", "N/m", 10, 500, 50, 5, "0");
        Num("amplitude", "Amplitude (puxão inicial)", "m", 0.02, 0.5, 0.1, 0.01, "0.00");
        Num("damping", "Amortecimento b", "kg/s", 0, 5, 0, 0.1, "0.0");
    }

    protected override void Compute()
    {
        double m = V("mass"), k = V("k"), A = V("amplitude"), b = V("damping");
        double w0 = Math.Sqrt(k / m), t0 = 2 * Math.PI / w0;
        _tSim = Math.Clamp(3 * t0, 2, 15);
        double x = A, v = 0, t = 0, dt = 0.0005;
        _s.Clear();
        _s.Add((0, x, v));
        var crossings = new List<double>();
        _vMax = 0;
        double xMin = 0, lastOut = 0;
        int n = (int)(_tSim / dt), every = Math.Max(1, n / 900);
        double Acc(double px, double pv) => (-k * px - b * pv) / m;
        for (int i = 1; i <= n; i++)
        {
            double k1x = v, k1v = Acc(x, v);
            double k2x = v + k1v * dt / 2, k2v = Acc(x + k1x * dt / 2, v + k1v * dt / 2);
            double k3x = v + k2v * dt / 2, k3v = Acc(x + k2x * dt / 2, v + k2v * dt / 2);
            double k4x = v + k3v * dt, k4v = Acc(x + k3x * dt, v + k3v * dt);
            double nx = x + dt * (k1x + 2 * k2x + 2 * k3x + k4x) / 6, nv = v + dt * (k1v + 2 * k2v + 2 * k3v + k4v) / 6;
            if (x != 0 && Math.Sign(nx) != Math.Sign(x)) crossings.Add(t + dt * x / (x - nx));
            x = nx; v = nv; t += dt;
            _vMax = Math.Max(_vMax, Math.Abs(v));
            xMin = Math.Min(xMin, x);
            if (Math.Abs(x) > 0.02 * A) lastOut = t;
            if (i % every == 0) _s.Add((t, x, v));
        }
        _period = PeriodFromCrossings(crossings);
        M["period"] = _period;
        M["frequency"] = double.IsInfinity(_period) ? 0 : 1 / _period;
        M["omega"] = w0;
        M["energy"] = 0.5 * k * A * A;
        M["maxSpeed"] = _vMax;
        M["energyEnd"] = 0.5 * k * x * x + 0.5 * m * v * v;
        M["overshoot"] = 100 * -xMin / A;
        // settled only if the remaining energy can no longer take |x| above 2 % (a zero crossing at the end of the run is not settling)
        bool settled = Math.Sqrt(x * x + m * v * v / k) <= 0.02 * A;
        M["settleTime"] = settled ? lastOut : double.PositiveInfinity;
        AnimSeconds = Math.Min(_tSim, 6);
    }

    (double t, double x, double v) Now()
    {
        if (!HasResult) return (0, V("amplitude"), 0);
        double t = Progress * _tSim;
        int i = (int)Math.Clamp(Math.Round(t / _tSim * (_s.Count - 1)), 0, _s.Count - 1);
        return _s[i];
    }

    protected override string LiveText()
    {
        var p = Now();
        return $"t = {F(p.t, "0.00")} s   x = {F(p.x, "0.000")} m   v = {F(p.v, "0.00")} m/s";
    }

    protected override IEnumerable<(string, string)> Results()
    {
        yield return (L.T("Período"), F(_period, "0.000") + " s");
        yield return (L.T("Frequência"), F(M["frequency"], "0.00") + " Hz");
        yield return (L.T("Energia"), F(M["energy"], "0.00") + " J");
        yield return (L.T("Vel. máxima"), F(_vMax, "0.00") + " m/s");
        if (V("damping") > 0) yield return (L.T("Energia no fim"), F(M["energyEnd"], "0.00") + " J");
        if (V("damping") > 0) yield return (L.T("Ultrapassagem"), F(M["overshoot"], "0.0") + " %");
        if (V("damping") > 0) yield return (L.T("Acomoda em"), F(M["settleTime"], "0.00") + " s");
    }

    protected override void Paint(DrawingContext dc)
    {
        double m = V("mass"), k = V("k");
        var (_, x, v) = Now();
        // left: wall, floor, spring, block
        double floor = 150, wall = 18, eq = 190, scale = 230;   // px per metre
        Line(dc, Ui.TextDim, wall, floor, 330, floor, 2);
        dc.DrawRectangle(Ui.Bg3, null, new Rect(4, 40, wall - 4, floor - 40));
        Line(dc, Faint, eq, 50, eq, floor + 10, 1, true);
        Text(dc, "x = 0", eq, floor + 10, 11, Ui.TextDim, 0.5);
        double bw = 26 + 10 * Math.Cbrt(m), bx = eq + x * scale;
        double y = floor - bw / 2, x1 = wall, x2 = bx - bw / 2;
        var pts = new List<Point> { new(x1, y) };
        int coils = 14;
        for (int i = 1; i < coils; i++)
            pts.Add(new Point(x1 + 10 + (x2 - x1 - 20) * i / coils, y + (i % 2 == 0 ? -9 : 9)));
        pts.Add(new Point(x2 - 10, y));
        pts.Add(new Point(x2, y));
        Polyline(dc, P(Yellow, 2), pts, pts.Count);
        dc.DrawRoundedRectangle(Blue, P(Ui.Text, 1), new Rect(bx - bw / 2, floor - bw, bw, bw), 3, 3);
        Text(dc, $"{F(m, "0.#")} kg", bx, floor - bw / 2 - 8, 11, Ui.Code, 0.5, true);
        Text(dc, $"k = {F(k, "0")} N/m", 24, 12, 12, Ui.TextDim);

        // energy bars
        double e0 = 0.5 * k * Math.Pow(V("amplitude"), 2), ke = 0.5 * m * v * v, pe = 0.5 * k * x * x;
        double bar = 120 / Math.Max(1e-9, e0);
        void Bar(double yy, string label, double val, Brush b)
        {
            Text(dc, label, 24, yy - 2, 11, Ui.TextDim);
            dc.DrawRectangle(Ui.Bg2, null, new Rect(80, yy, 120, 10));
            dc.DrawRectangle(b, null, new Rect(80, yy, Math.Min(120, val * bar), 10));
            Text(dc, F(val, "0.00") + " J", 206, yy - 2, 11, Ui.TextDim);
        }
        Bar(176, L.T("cinética"), ke, Orange);
        Bar(192, L.T("elástica"), pe, Yellow);
        Bar(208, L.T("total"), ke + pe, Ui.Accent);

        // right: x(t)
        double cxl = 348, cw = 282, ctop = 20, ch = 188;
        Chart(dc, cxl, ctop, cw, ch, L.T("posição x(t)"), L.T("tempo"));
        if (!HasResult) return;
        double A = Math.Max(1e-9, V("amplitude"));
        int count = (int)(Progress * (_s.Count - 1)) + 1;
        var curve = _s.Select(p => new Point(cxl + p.t / _tSim * cw, ctop + ch / 2 - p.x / A * (ch / 2 - 8))).ToList();
        Polyline(dc, P(Blue, 1.8), curve, count);
        Text(dc, $"{F(_tSim, "0.#")} s", cxl + cw - 2, ctop + ch - 16, 11, Ui.TextDim, 1);
    }
}
