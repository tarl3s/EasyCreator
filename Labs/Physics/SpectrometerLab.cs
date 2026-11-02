using System.Windows;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>Magnetic-sector mass spectrometer: a singly charged ion (q = e) starts at rest, is accelerated through a
/// voltage V (q·V = m·v²/2 → v = √(2qV/m)) and enters a uniform field B perpendicular to its velocity, where it moves
/// on a half circle of radius r = m·v/(q·B) and lands at x = 2r from the entrance slit. labParams "targetX" (m) and
/// "targetWidth" (m, default 0.02) place the detector. Masses in atomic mass units (1 u = 1.66053907e-27 kg).
/// Metrics: speed (m/s), radius (m), landingX (m), period (s, 2πm/(qB)), timeInField (s, half a period),
/// energyEv (eV), hitTarget (1/0, with a detector), mass (u), plus the inputs (ion, voltage, field).</summary>
[Lab("spectrometer")]
public sealed class SpectrometerLab : PhysLab
{
    public const double E = 1.602176634e-19, U = 1.66053906660e-27;
    double _targetX = double.NaN, _targetW = 0.02, _v, _r;

    public SpectrometerLab()
    {
        Opt("ion", "Íon (carga +1)", "c12", ("h1", "Próton (¹H⁺)"), ("c12", "Carbono-12"), ("c14", "Carbono-14"),
            ("u235", "Urânio-235"), ("u238", "Urânio-238"));
        Num("voltage", "Tensão de aceleração V", "V", 100, 5000, 1000, 10, "0");
        Num("field", "Campo magnético B", "T", 0.01, 1.5, 0.2, 0.001, "0.000");
    }

    public static double MassU(string ion) => ion switch
    {
        "h1" => 1.007276, "c14" => 14.003242, "u235" => 235.043930, "u238" => 238.050788, _ => 12.0,
    };

    protected override void OnLoad(IReadOnlyDictionary<string, string> p)
    {
        if (p.TryGetValue("targetX", out var t) && Question.TryNum(t, out var tx)) _targetX = tx;
        if (p.TryGetValue("targetWidth", out var w) && Question.TryNum(w, out var tw)) _targetW = Math.Max(0.002, tw);
    }

    protected override void Compute()
    {
        double m = MassU(C("ion")) * U, V0 = V("voltage"), B = V("field");
        _v = Math.Sqrt(2 * E * V0 / m);
        _r = m * _v / (E * B);
        double period = 2 * Math.PI * m / (E * B);
        M["speed"] = _v;
        M["radius"] = _r;
        M["landingX"] = 2 * _r;
        M["period"] = period;
        M["timeInField"] = period / 2;
        M["energyEv"] = V0;
        M["mass"] = MassU(C("ion"));
        if (!double.IsNaN(_targetX)) M["hitTarget"] = Math.Abs(2 * _r - _targetX) <= _targetW / 2 ? 1 : 0;
        AnimSeconds = 2.5;
    }

    protected override string LiveText() =>
        L.F("v = {0} km/s   r = {1} cm", F(_v / 1000, "0.0"), F(_r * 100, "0.00"));

    protected override IEnumerable<(string, string)> Results()
    {
        yield return (L.T("Velocidade"), F(_v / 1000, "0.0") + " km/s");
        yield return (L.T("Raio"), F(_r * 100, "0.00") + " cm");
        yield return (L.T("Pousa em"), F(2 * _r * 100, "0.00") + " cm");
        yield return (L.T("Tempo no campo"), F(M["timeInField"] * 1e6, "0.00") + " µs");
        if (!double.IsNaN(_targetX)) yield return (L.T("Detector"), M.GetValueOrDefault("hitTarget") > 0 ? L.T("acertou ✓") : L.T("errou"));
    }

    protected override void Paint(DrawingContext dc)
    {
        // entrance slit at (sx, sy); the field region is the area below the slit line; x grows to the right
        double sx = 60, sy = 40, scale = 1000;   // px per metre
        dc.DrawRectangle(BlueSoft, null, new Rect(20, sy, W - 40, H - sy - 10));
        for (double gx = 40; gx < W - 20; gx += 40)
            for (double gy = sy + 20; gy < H - 10; gy += 40)
                Text(dc, "×", gx, gy - 8, 12, Blue, 0.5);
        Text(dc, $"B = {F(V("field"), "0.000")} T ({L.T("entrando na tela")})", W - 24, sy + 4, 11, Ui.TextDim, 1);
        // accelerator above the slit
        Line(dc, Ui.Text, sx - 18, 6, sx - 18, sy - 4, 2);
        Line(dc, Ui.Text, sx + 18, 6, sx + 18, sy - 4, 2);
        Text(dc, $"{F(V("voltage"), "0")} V", sx + 24, 10, 11, Ui.TextDim);
        Line(dc, Ui.Text, 20, sy, sx - 6, sy, 2);
        Line(dc, Ui.Text, sx + 6, sy, W - 20, sy, 2);
        // detector
        if (!double.IsNaN(_targetX))
        {
            double a = sx + (_targetX - _targetW / 2) * scale, b = sx + (_targetX + _targetW / 2) * scale;
            dc.DrawRectangle(Ui.Accent, null, new Rect(a, sy - 6, Math.Max(3, b - a), 6));
            Text(dc, L.F("detector {0} cm", F(_targetX * 100, "0.#")), (a + b) / 2, sy - 22, 11, Ui.Accent, 0.5);
        }
        // path: half circle of radius r starting downward at the slit, turning towards +x (positive ion, B into the screen)
        double m = MassU(C("ion")) * U, v = Math.Sqrt(2 * E * V("voltage") / m), r = m * v / (E * V("field"));
        double rp = r * scale, frac = HasResult ? Progress : 1;
        var pts = new List<Point>();
        Point last = new(sx, sy);
        for (int i = 0; i <= 120; i++)
        {
            double ang = Math.PI * frac * i / 120;
            last = new Point(sx + rp - rp * Math.Cos(ang), sy + rp * Math.Sin(ang));
            pts.Add(last);
        }
        Polyline(dc, P(Orange, HasResult ? 2.5 : 1.2, !HasResult), pts, pts.Count);
        if (HasResult) Dot(dc, Orange, last.X, last.Y, 5);
        if (HasResult && Progress >= 1)
            Text(dc, $"2r = {F(2 * r * 100, "0.00")} cm", Math.Min(W - 60, sx + 2 * rp), sy - 38, 12, Ui.Text, 0.5, true);
    }
}
