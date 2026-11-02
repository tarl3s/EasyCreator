using System.Windows;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>Satellite launched horizontally (perpendicular to the radius) at a given altitude. Newtonian gravity of a
/// spherical planet, no atmosphere, no other bodies. Orbit elements come from energy and angular momentum; the path
/// shown is integrated with RK4.
/// Metrics: orbitType (0 = falls on the planet, 1 = closed orbit, 2 = escapes), eccentricity, period (min; ∞ when it
/// escapes), periapsisAlt / apoapsisAlt (km above the surface), circularSpeed and escapeSpeed (km/s at the launch
/// altitude), plus the inputs (altitude, speed, planet).</summary>
[Lab("orbit")]
public sealed class OrbitLab : PhysLab
{
    readonly List<(double t, double x, double y)> _s = new();
    double _mu, _r, _e, _period, _rp, _ra, _tEnd;
    int _type;

    public OrbitLab()
    {
        Num("altitude", "Altitude do lançamento", "km", 100, 40000, 400, 50, "#,0");
        Num("speed", "Velocidade horizontal", "km/s", 0, 12, 6, 0.01, "0.00");
        Opt("planet", "Planeta", "earth", ("earth", "Terra"), ("moon", "Lua"), ("mars", "Marte"));
    }

    static (double R, double Mu) Body(string p) => p switch
    {
        "moon" => (1737.4, 4902.8),
        "mars" => (3389.5, 42828),
        _ => (6371, 398600.4),
    };

    protected override string ValueLabel(Input i) =>
        i.Key == "planet" ? $"R = {F(Body(i.Choice).R, "#,0")} km" : base.ValueLabel(i);

    protected override void Compute()
    {
        var (R, mu) = Body(C("planet"));
        _mu = mu; _r = R + V("altitude");
        double v = V("speed"), r = _r;
        double energy = v * v / 2 - mu / r, h = r * v;
        _e = Math.Sqrt(Math.Max(0, 1 + 2 * energy * h * h / (mu * mu)));
        if (energy < 0)
        {
            double a = -mu / (2 * energy);
            _rp = a * (1 - _e); _ra = a * (1 + _e);
            _period = 2 * Math.PI * Math.Sqrt(a * a * a / mu);
            _type = _rp < R ? 0 : 1;
        }
        else
        {
            _rp = h * h / (mu * (1 + _e));
            _ra = double.PositiveInfinity;
            _period = double.PositiveInfinity;
            _type = _rp < R ? 0 : 2;
        }

        // Path for the drawing: launch at (r, 0) moving +y; RK4 with a step tied to the local orbital time scale.
        _s.Clear();
        double x = r, y = 0, vx = 0, vy = v, t = 0;
        _s.Add((0, x, y));
        double limit = _type == 1 ? _period : double.IsInfinity(_period) ? 1e9 : _period;
        for (int i = 0; i < 40000; i++)
        {
            double rr = Math.Sqrt(x * x + y * y);
            double dt = 0.0015 * Math.Sqrt(rr * rr * rr / mu);
            if (_type == 1) dt = Math.Min(dt, limit - t);
            (double, double) Acc(double px, double py) { double d = Math.Sqrt(px * px + py * py); double f = -mu / (d * d * d); return (f * px, f * py); }
            var (a1x, a1y) = Acc(x, y);
            var (a2x, a2y) = Acc(x + vx * dt / 2, y + vy * dt / 2);
            var (a3x, a3y) = Acc(x + (vx + a1x * dt / 2) * dt / 2, y + (vy + a1y * dt / 2) * dt / 2);
            var (a4x, a4y) = Acc(x + (vx + a2x * dt / 2) * dt, y + (vy + a2y * dt / 2) * dt);
            double nx = x + dt * (vx + 2 * (vx + a1x * dt / 2) + 2 * (vx + a2x * dt / 2) + (vx + a3x * dt)) / 6;
            double ny = y + dt * (vy + 2 * (vy + a1y * dt / 2) + 2 * (vy + a2y * dt / 2) + (vy + a3y * dt)) / 6;
            vx += dt * (a1x + 2 * a2x + 2 * a3x + a4x) / 6;
            vy += dt * (a1y + 2 * a2y + 2 * a3y + a4y) / 6;
            x = nx; y = ny; t += dt;
            double nr = Math.Sqrt(x * x + y * y);
            if (nr <= R)
            {
                x *= R / nr; y *= R / nr;
                _s.Add((t, x, y));
                break;
            }
            _s.Add((t, x, y));
            if (_type == 1 && t >= limit - 1e-9) break;
            if (_type == 2 && nr > 7 * r) break;
        }
        _tEnd = t;
        M["orbitType"] = _type;
        M["eccentricity"] = _e;
        M["period"] = _period / 60;
        M["periapsisAlt"] = _rp - R;
        M["apoapsisAlt"] = _ra - R;
        M["circularSpeed"] = Math.Sqrt(mu / r);
        M["escapeSpeed"] = Math.Sqrt(2 * mu / r);
        AnimSeconds = _type == 1 ? 5 : 3.5;
    }

    int Index()
    {
        if (!HasResult) return 0;
        double t = Progress * _tEnd;
        int lo = 0, hi = _s.Count - 1;
        while (lo < hi) { int mid = (lo + hi + 1) / 2; if (_s[mid].t <= t) lo = mid; else hi = mid - 1; }
        return lo;
    }

    protected override string LiveText()
    {
        var p = _s[Index()];
        double alt = Math.Sqrt(p.x * p.x + p.y * p.y) - Body(C("planet")).R;
        return $"t = {F(p.t / 60, "0")} min   altitude = {F(alt, "#,0")} km";
    }

    protected override IEnumerable<(string, string)> Results()
    {
        yield return ("Resultado", _type switch { 0 => "caiu no planeta", 1 => "órbita fechada", _ => "escapou" });
        yield return ("Excentricidade", F(_e, "0.000"));
        if (_type == 1) yield return ("Período", F(_period / 60, "0.0") + " min");
        yield return ("Perigeu", F(_rp - Body(C("planet")).R, "#,0") + " km");
        if (_type == 1) yield return ("Apogeu", F(_ra - Body(C("planet")).R, "#,0") + " km");
        yield return ("v circular", F(M["circularSpeed"], "0.00") + " km/s");
        yield return ("v escape", F(M["escapeSpeed"], "0.00") + " km/s");
    }

    protected override void Paint(DrawingContext dc)
    {
        var (R, mu) = Body(C("planet"));
        double r = R + V("altitude");
        // a few fixed stars
        var rnd = new Random(7);
        for (int i = 0; i < 40; i++) Dot(dc, Faint, rnd.NextDouble() * W, rnd.NextDouble() * H, 0.9);

        double minX = -Math.Max(R, r), maxX = r, minY = -Math.Max(R, r), maxY = Math.Max(R, r);
        if (HasResult)
            foreach (var p in _s) { minX = Math.Min(minX, p.x); maxX = Math.Max(maxX, p.x); minY = Math.Min(minY, p.y); maxY = Math.Max(maxY, p.y); }
        double s = Math.Min((W - 30) / (maxX - minX), (H - 24) / (maxY - minY));
        double ox = (W - (maxX - minX) * s) / 2 - minX * s, oy = (H - (maxY - minY) * s) / 2 + maxY * s;
        Point S(double x, double y) => new(ox + x * s, oy - y * s);

        var c = S(0, 0);
        Brush planet = C("planet") switch { "moon" => Ui.Make("#9A9AA6"), "mars" => Ui.Make("#C8603C"), _ => Ui.Make("#2F7FD0") };
        dc.DrawEllipse(planet, P(Ui.Text, 0.8), c, R * s, R * s);
        // circular-orbit guide at the launch altitude
        dc.DrawEllipse(null, P(Faint, 1, true), c, r * s, r * s);
        var sat0 = S(r, 0);
        if (!HasResult)
        {
            double vc = Math.Sqrt(mu / r);
            Arrow(dc, Yellow, sat0.X, sat0.Y, sat0.X, sat0.Y - 18 - 40 * V("speed") / Math.Max(1, vc), 2);
            Dot(dc, Ui.Text, sat0.X, sat0.Y, 4);
            Text(dc, "tracejado = órbita circular nessa altitude", 8, H - 18, 11, Ui.TextDim);
            return;
        }
        int idx = Index();
        var pts = _s.Select(p => S(p.x, p.y)).ToList();
        Polyline(dc, P(Yellow, 1.8), pts, idx + 1);
        Dot(dc, Ui.Text, pts[idx].X, pts[idx].Y, 4.5);
        if (Progress >= 1 && _type == 0) { Dot(dc, Ui.Bad, pts[^1].X, pts[^1].Y, 6); Text(dc, "queda!", pts[^1].X + 8, pts[^1].Y - 8, 12, Ui.Bad, 0, true); }
    }
}
