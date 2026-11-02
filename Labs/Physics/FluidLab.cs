using System.Windows;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>Torricelli's tank: a wide open tank of water with its surface at height "level" above the floor and a
/// small hole in its side at height "hole" (both m). Ideal fluid, the level held constant: the jet leaves horizontally
/// at v = √(2·g·h) with h = level − hole (Bernoulli), falls for t = √(2·hole/g) and lands at x = v·t = 2·√(h·hole).
/// Flow Q = A·v (hole diameter in mm). Gauge pressure at the hole p = ρ·g·h. labParams "targetX"/"targetWidth" (m)
/// place a bucket on the floor. Metrics: depth (m), exitSpeed (m/s), range (m), fallTime (s), flowRate (L/s),
/// pressure (Pa, gauge), hitTarget (1/0, with a target), plus the inputs (level, hole, diameter).</summary>
[Lab("fluid")]
public sealed class FluidLab : PhysLab
{
    const double G = 9.81, Rho = 1000;
    double _targetX = double.NaN, _targetW = 0.06, _h, _v, _x, _tf;

    public FluidLab()
    {
        Num("level", "Altura da água (nível)", "m", 0.2, 2, 1, 0.01, "0.00");
        Num("hole", "Altura do furo", "m", 0, 2, 0.5, 0.01, "0.00");
        Num("diameter", "Diâmetro do furo", "mm", 1, 20, 5, 0.5, "0.0");
    }

    protected override void OnLoad(IReadOnlyDictionary<string, string> p)
    {
        if (p.TryGetValue("targetX", out var t) && Question.TryNum(t, out var tx)) _targetX = tx;
        if (p.TryGetValue("targetWidth", out var w) && Question.TryNum(w, out var tw)) _targetW = Math.Max(0.01, tw);
    }

    protected override void Compute()
    {
        double level = V("level"), hole = Math.Min(V("hole"), level);
        _h = level - hole;
        _v = Math.Sqrt(2 * G * _h);
        _tf = Math.Sqrt(2 * hole / G);
        _x = _v * _tf;
        double area = Math.PI * Math.Pow(V("diameter") / 1000, 2) / 4;
        M["depth"] = _h;
        M["exitSpeed"] = _v;
        M["range"] = _x;
        M["fallTime"] = _tf;
        M["flowRate"] = area * _v * 1000;
        M["pressure"] = Rho * G * _h;
        if (!double.IsNaN(_targetX)) M["hitTarget"] = Math.Abs(_x - _targetX) <= _targetW / 2 ? 1 : 0;
        AnimSeconds = 2;
    }

    protected override string LiveText() =>
        L.F("v = √(2·g·h) = {0} m/s   alcance = {1} m", F(_v, "0.00"), F(_x * Math.Min(1, Progress * 1.5), "0.00"));

    protected override IEnumerable<(string, string)> Results()
    {
        yield return (L.T("Profundidade do furo"), F(_h, "0.00") + " m");
        yield return (L.T("Vel. de saída"), F(_v, "0.00") + " m/s");
        yield return (L.T("Alcance"), F(_x, "0.000") + " m");
        yield return (L.T("Vazão"), F(M["flowRate"], "0.000") + " L/s");
        yield return (L.T("Pressão no furo"), F(M["pressure"] / 1000, "0.00") + " kPa");
        if (!double.IsNaN(_targetX)) yield return (L.T("Alvo"), M.GetValueOrDefault("hitTarget") > 0 ? L.T("acertou ✓") : L.T("errou"));
    }

    protected override void Paint(DrawingContext dc)
    {
        double level = V("level"), hole = Math.Min(V("hole"), level);
        double floor = H - 22, x0 = 40;
        double xmax = Math.Max(1.2 * level, double.IsNaN(_targetX) ? 1.2 * level : _targetX + 0.3);
        double s = Math.Min(185 / (level + 0.1), (W - x0 - 70 - 30) / xmax);
        double tankW = 70, tankR = x0 + tankW;
        Line(dc, Faint, 0, floor, W, floor, 1);
        // tank walls and water
        double topY = floor - (level + 0.1) * s;
        dc.DrawRectangle(BlueSoft, null, new Rect(x0, floor - level * s, tankW, level * s));
        Line(dc, Ui.Text, x0, topY, x0, floor, 2);
        Line(dc, Ui.Text, tankR, topY, tankR, floor, 2);
        Line(dc, Ui.Text, x0, floor, tankR, floor, 2);
        Line(dc, Blue, x0, floor - level * s, tankR, floor - level * s, 1.5);
        Text(dc, L.F("nível {0} m", F(level, "0.00")), x0 + 2, floor - level * s - 16, 11, Blue);
        // hole and depth arrow
        double hy = floor - hole * s;
        Dot(dc, Ui.Code, tankR, hy, 3);
        Line(dc, Faint, tankR - 12, floor - level * s, tankR - 12, hy, 1, true);
        Text(dc, $"h = {F(level - hole, "0.00")} m", x0 + 4, (hy + floor - level * s) / 2 - 7, 11, Ui.TextDim);
        // target bucket
        if (!double.IsNaN(_targetX))
        {
            double a = tankR + (_targetX - _targetW / 2) * s, b = tankR + (_targetX + _targetW / 2) * s;
            dc.DrawRectangle(AccentSoft, P(Ui.Accent, 1.5), new Rect(a, floor - 14, Math.Max(4, b - a), 14));
            Text(dc, L.F("alvo {0} m", F(_targetX, "0.00")), (a + b) / 2, floor - 32, 11, Ui.Accent, 0.5);
        }
        // jet: x = v·t, y = g·t²/2 below the hole
        double v = Math.Sqrt(2 * G * (level - hole)), tf = Math.Sqrt(2 * hole / G);
        double frac = HasResult ? Math.Min(1, Progress * 1.5) : 1;
        var pts = new List<Point>();
        for (int i = 0; i <= 60; i++)
        {
            double t = tf * frac * i / 60;
            pts.Add(new Point(tankR + v * t * s, hy + G * t * t / 2 * s));
        }
        Polyline(dc, P(Blue, HasResult ? 3 : 1.2, !HasResult), pts, pts.Count);
        if (HasResult && Progress >= 0.66)
            Text(dc, $"{F(v * tf, "0.000")} m", tankR + v * tf * s, floor + 3, 12, Ui.Text, 0.5, true);
    }
}
