using System.Windows;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>Projectile launched from a height, optional quadratic air drag (a = −k·|v|·v, integrated with RK4).
/// Metrics: range, maxHeight, flightTime, impactSpeed (m, s, m/s), hitTarget (1/0, when labParams has targetX),
/// rangeNoDrag (vacuum range for comparison), plus every input (speed, angle, height, drag).</summary>
[Lab("projectile")]
public sealed class ProjectileLab : PhysLab
{
    double _targetX = double.NaN, _targetW = 2;
    readonly List<(double t, double x, double y, double v)> _s = new();
    double _flight, _range, _maxH, _rangeVacuum;

    public ProjectileLab()
    {
        Num("speed", "Velocidade inicial", "m/s", 1, 50, 10, 0.1, "0.0");
        Num("angle", "Ângulo de lançamento", "°", 0, 90, 30, 1, "0");
        Num("height", "Altura de lançamento", "m", 0, 30, 0, 0.5, "0.0");
        Num("drag", "Arrasto do ar k", "1/m", 0, 0.05, 0, 0.001, "0.000");
        Opt("planet", "Planeta", "earth", Planets);
    }

    protected override string ValueLabel(Input i) =>
        i.Key == "planet" ? $"g = {F(Gravity(i.Choice), "0.00")} m/s²" : base.ValueLabel(i);

    protected override void OnLoad(IReadOnlyDictionary<string, string> p)
    {
        if (p.TryGetValue("targetX", out var t) && Question.TryNum(t, out var tx)) _targetX = tx;
        if (p.TryGetValue("targetWidth", out var w) && Question.TryNum(w, out var tw)) _targetW = Math.Max(0.2, tw);
    }

    protected override void Compute()
    {
        double g = Gravity(C("planet")), v0 = V("speed"), a = Rad(V("angle")), h0 = V("height"), k = V("drag");
        double vx = v0 * Math.Cos(a), vy = v0 * Math.Sin(a);
        _s.Clear();
        // Vacuum: closed form.
        double tVac = (vy + Math.Sqrt(vy * vy + 2 * g * h0)) / g;
        _rangeVacuum = vx * tVac;
        if (k <= 0 || (h0 <= 0 && vy <= 0))
        {
            _flight = tVac;
            _range = _rangeVacuum;
            _maxH = h0 + (vy > 0 ? vy * vy / (2 * g) : 0);
            int n = 400;
            for (int i = 0; i <= n; i++)
            {
                double t = _flight * i / n, y = Math.Max(0, h0 + vy * t - g * t * t / 2), uy = vy - g * t;
                _s.Add((t, vx * t, y, Math.Sqrt(vx * vx + uy * uy)));
            }
        }
        else
        {
            // RK4 on (x, y, vx, vy) with a = (−k|v|vx, −g − k|v|vy)
            double x = 0, y = h0, ux = vx, uy = vy, t = 0, dt = 0.0005;
            _maxH = h0;
            _s.Add((0, 0, h0, v0));
            int step = 0;
            while (t < 300)
            {
                (double ax, double ay) Acc(double px, double py)
                {
                    double sp = Math.Sqrt(px * px + py * py);
                    return (-k * sp * px, -g - k * sp * py);
                }
                var (a1x, a1y) = Acc(ux, uy);
                var (a2x, a2y) = Acc(ux + a1x * dt / 2, uy + a1y * dt / 2);
                var (a3x, a3y) = Acc(ux + a2x * dt / 2, uy + a2y * dt / 2);
                var (a4x, a4y) = Acc(ux + a3x * dt, uy + a3y * dt);
                double nx = x + dt * (ux + 2 * (ux + a1x * dt / 2) + 2 * (ux + a2x * dt / 2) + (ux + a3x * dt)) / 6;
                double ny = y + dt * (uy + 2 * (uy + a1y * dt / 2) + 2 * (uy + a2y * dt / 2) + (uy + a3y * dt)) / 6;
                double nux = ux + dt * (a1x + 2 * a2x + 2 * a3x + a4x) / 6;
                double nuy = uy + dt * (a1y + 2 * a2y + 2 * a3y + a4y) / 6;
                if (ny < 0 && t > 0)
                {
                    double f = y / (y - ny);
                    x += (nx - x) * f; ux += (nux - ux) * f; uy += (nuy - uy) * f; t += dt * f; y = 0;
                    _s.Add((t, x, 0, Math.Sqrt(ux * ux + uy * uy)));
                    break;
                }
                x = nx; y = ny; ux = nux; uy = nuy; t += dt;
                _maxH = Math.Max(_maxH, y);
                if (++step % 20 == 0) _s.Add((t, x, y, Math.Sqrt(ux * ux + uy * uy)));
            }
            _flight = t;
            _range = x;
        }
        var last = _s[^1];
        M["range"] = _range;
        M["maxHeight"] = _maxH;
        M["flightTime"] = _flight;
        M["impactSpeed"] = last.v;
        M["rangeNoDrag"] = _rangeVacuum;
        if (!double.IsNaN(_targetX)) M["hitTarget"] = Math.Abs(_range - _targetX) <= _targetW / 2 ? 1 : 0;
        AnimSeconds = Math.Clamp(_flight, 1.2, 4);
    }

    int Index()
    {
        if (!HasResult) return 0;
        double t = Progress * _flight;
        int lo = 0, hi = _s.Count - 1;
        while (lo < hi) { int mid = (lo + hi + 1) / 2; if (_s[mid].t <= t) lo = mid; else hi = mid - 1; }
        return lo;
    }

    protected override string LiveText()
    {
        var p = _s[Index()];
        return $"t = {F(p.t, "0.00")} s   x = {F(p.x, "0.0")} m   y = {F(p.y, "0.0")} m   v = {F(p.v, "0.0")} m/s";
    }

    protected override IEnumerable<(string, string)> Results()
    {
        yield return (L.T("Alcance"), F(_range, "0.0") + " m");
        yield return (L.T("Altura máx."), F(_maxH, "0.0") + " m");
        yield return (L.T("Tempo de voo"), F(_flight, "0.00") + " s");
        yield return (L.T("Impacto"), F(_s[^1].v, "0.0") + " m/s");
        if (V("drag") > 0) yield return (L.T("Sem ar"), F(_rangeVacuum, "0.0") + " m");
        if (!double.IsNaN(_targetX)) yield return (L.T("Alvo"), M.GetValueOrDefault("hitTarget") > 0 ? L.T("acertou ✓") : L.T("errou"));
    }

    protected override void Paint(DrawingContext dc)
    {
        double h0 = V("height"), a = Rad(V("angle"));
        double xmax = Math.Max(20, double.IsNaN(_targetX) ? 20 : _targetX + _targetW + 6);
        double ymax = Math.Max(8, h0 * 1.25 + 2);
        if (HasResult) { xmax = Math.Max(xmax, _range * 1.06); ymax = Math.Max(ymax, _maxH * 1.12); }
        double left = 44, ground = H - 26, s = Math.Min((W - left - 16) / xmax, (ground - 14) / ymax);
        Point P2(double x, double y) => new(left + x * s, ground - y * s);

        // grid and axis labels
        double step = NiceStep(xmax, 8);
        for (double gx = 0; gx <= xmax + 1e-9; gx += step)
        {
            Line(dc, GridLine, left + gx * s, 8, left + gx * s, ground, 1);
            Text(dc, F(gx, "0.##"), left + gx * s, ground + 5, 11, Ui.TextDim, 0.5);
        }
        double ystep = NiceStep(ground / s, 4);
        for (double gy = ystep; gy * s < ground - 8; gy += ystep)
        {
            Line(dc, GridLine, left, ground - gy * s, W - 8, ground - gy * s, 1);
            Text(dc, F(gy, "0.##"), left - 6, ground - gy * s - 8, 11, Ui.TextDim, 1);
        }
        Text(dc, "m", W - 10, ground + 5, 11, Ui.TextDim, 1);
        Line(dc, Ui.TextDim, 0, ground, W, ground, 2);

        // target
        if (!double.IsNaN(_targetX))
        {
            var a1 = P2(_targetX - _targetW / 2, 0); var a2 = P2(_targetX + _targetW / 2, 0);
            dc.DrawRectangle(AccentSoft, P(Ui.Accent, 1.5), new Rect(a1.X, ground - 6, Math.Max(3, a2.X - a1.X), 6));
            Line(dc, Ui.Accent, (a1.X + a2.X) / 2, ground - 6, (a1.X + a2.X) / 2, ground - 34, 1.5);
            dc.DrawRectangle(Ui.Accent, null, new Rect((a1.X + a2.X) / 2, ground - 34, 14, 9));
            Text(dc, L.F("alvo {0} m", F(_targetX, "0.#")), (a1.X + a2.X) / 2, ground - 50, 11, Ui.Accent, 0.5);
        }

        // tower and cannon
        var launch = P2(0, h0);
        if (h0 > 0) dc.DrawRectangle(Ui.Bg3, P(Faint, 1), new Rect(launch.X - 10, launch.Y, 10, ground - launch.Y));
        Line(dc, Ui.Text, launch.X, launch.Y, launch.X + 26 * Math.Cos(a), launch.Y - 26 * Math.Sin(a), 5);

        if (!HasResult)
        {
            Arrow(dc, Yellow, launch.X, launch.Y, launch.X + (40 + V("speed") * 2.2) * Math.Cos(a), launch.Y - (40 + V("speed") * 2.2) * Math.Sin(a), 1.8);
            Text(dc, $"v₀ = {F(V("speed"), "0.0")} m/s · {F(V("angle"), "0")}°", launch.X + 30, 10, 12, Yellow);
            return;
        }

        // vacuum ghost (when drag is on)
        if (V("drag") > 0)
        {
            double g = Gravity(C("planet")), v0 = V("speed"), vx = v0 * Math.Cos(a), vy = v0 * Math.Sin(a);
            double tv = (vy + Math.Sqrt(vy * vy + 2 * g * h0)) / g;
            var ghost = new List<Point>();
            for (int i = 0; i <= 120; i++) { double t = tv * i / 120; ghost.Add(P2(vx * t, Math.Max(0, h0 + vy * t - g * t * t / 2))); }
            Polyline(dc, P(Faint, 1.5, true), ghost, ghost.Count);
            Text(dc, L.T("sem ar"), ghost[^1].X, ground - 18, 11, Faint, 0.5);
        }

        int idx = Index();
        var pts = _s.Select(p => P2(p.x, p.y)).ToList();
        Polyline(dc, P(Yellow, 2), pts, idx + 1);
        var cur = _s[idx];
        // interpolate the ball between samples for a smooth replay
        var ball = P2(cur.x, cur.y);
        if (idx + 1 < _s.Count && Progress < 1)
        {
            var nx = _s[idx + 1];
            double f = (Progress * _flight - cur.t) / Math.Max(1e-9, nx.t - cur.t);
            ball = P2(cur.x + (nx.x - cur.x) * f, cur.y + (nx.y - cur.y) * f);
        }
        Dot(dc, Ui.Text, ball.X, ball.Y, 6);

        if (Progress >= 1)
        {
            // max height marker and landing point
            var top = _s.OrderByDescending(p => p.y).First();
            var tp = P2(top.x, top.y);
            Line(dc, Blue, left, tp.Y, tp.X, tp.Y, 1, true);
            Text(dc, L.F("h máx = {0} m", F(_maxH, "0.0")), tp.X + 6, tp.Y - 16, 11, Blue);
            var land = P2(_range, 0);
            Line(dc, Ui.Bad, land.X - 5, land.Y - 5, land.X + 5, land.Y + 5, 2);
            Line(dc, Ui.Bad, land.X - 5, land.Y + 5, land.X + 5, land.Y - 5, 2);
            Text(dc, $"{F(_range, "0.0")} m", land.X, double.IsNaN(_targetX) ? land.Y - 22 : ground - 68, 12, Ui.Text, 0.5, true);
        }
    }
}
