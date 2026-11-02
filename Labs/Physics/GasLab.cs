using System.Windows;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>Ideal monatomic gas (Cv = 3/2·R, γ = 5/3) in a cylinder with a piston, taken from state 1 (n, T1, V1) to
/// volume V2 by a quasi-static process: isothermal (T constant), isobaric (p constant) or adiabatic (Q = 0, p·V^γ
/// constant). First law: ΔU = Q − W, with W the work done BY the gas.
/// Metrics: p1, p2 (kPa), T2 (K), work, heat, deltaU (J), plus the inputs (process as index, moles, T1, V1, V2).</summary>
[Lab("gas")]
public sealed class GasLab : PhysLab
{
    const double R = 8.314, Gamma = 5.0 / 3;
    double _p1, _p2, _t2, _w, _q, _du;

    public GasLab()
    {
        Opt("process", "Processo", "isobaric", ("isothermal", "Isotérmico"), ("isobaric", "Isobárico"), ("adiabatic", "Adiabático"));
        Num("moles", "Quantidade de gás n", "mol", 0.2, 3, 1, 0.1, "0.0");
        Num("T1", "Temperatura inicial T1", "K", 100, 800, 300, 10, "0");
        Num("V1", "Volume inicial V1", "L", 1, 50, 10, 1, "0");
        Num("V2", "Volume final V2", "L", 1, 50, 10, 1, "0");
    }

    /// <summary>Pressure (Pa) and temperature (K) at volume v (L) along the chosen process.</summary>
    (double p, double T) At(double v)
    {
        double n = V("moles"), t1 = V("T1"), v1 = V("V1") / 1000, vv = v / 1000, p1 = n * R * t1 / v1;
        return C("process") switch
        {
            "isothermal" => (n * R * t1 / vv, t1),
            "adiabatic" => (p1 * Math.Pow(v1 / vv, Gamma), t1 * Math.Pow(v1 / vv, Gamma - 1)),
            _ => (p1, p1 * vv / (n * R)),
        };
    }

    protected override void Compute()
    {
        double n = V("moles"), t1 = V("T1"), v1 = V("V1") / 1000, v2 = V("V2") / 1000;
        _p1 = n * R * t1 / v1;
        (_p2, _t2) = At(V("V2"));
        _du = 1.5 * n * R * (_t2 - t1);
        _w = C("process") switch
        {
            "isothermal" => n * R * t1 * Math.Log(v2 / v1),
            "adiabatic" => -_du,
            _ => _p1 * (v2 - v1),
        };
        _q = _du + _w;
        M["p1"] = _p1 / 1000;
        M["p2"] = _p2 / 1000;
        M["T2"] = _t2;
        M["work"] = _w;
        M["heat"] = _q;
        M["deltaU"] = _du;
        AnimSeconds = 3;
    }

    double VNow => HasResult ? V("V1") + (V("V2") - V("V1")) * Progress : V("V1");

    protected override string LiveText()
    {
        var (p, t) = At(VNow);
        return $"V = {F(VNow, "0.0")} L   p = {F(p / 1000, "0.0")} kPa   T = {F(t, "0")} K";
    }

    protected override IEnumerable<(string, string)> Results()
    {
        yield return ("p1 → p2", $"{F(_p1 / 1000, "0.0")} → {F(_p2 / 1000, "0.0")} kPa");
        yield return ("T2", F(_t2, "0") + " K");
        yield return ("Trabalho do gás W", F(_w, "0") + " J");
        yield return ("Calor Q", F(_q, "0") + " J");
        yield return ("ΔU", F(_du, "0") + " J");
    }

    protected override void Paint(DrawingContext dc)
    {
        double v = VNow;
        var (p, t) = At(v);
        // ---- cylinder with piston (left)
        double cx = 22, cw = 150, cb = H - 16, ctop = 26, maxV = 50;
        double gasTop = cb - (cb - ctop - 14) * v / maxV;
        dc.DrawRectangle(null, P(Ui.TextDim, 2), new Rect(cx, ctop, cw, cb - ctop));
        double heat = Math.Clamp((t - 100) / 900, 0, 1);
        var gasColor = new SolidColorBrush(Color.FromArgb(70, (byte)(60 + 195 * heat), 90, (byte)(255 - 200 * heat)));
        gasColor.Freeze();
        dc.DrawRectangle(gasColor, null, new Rect(cx + 1, gasTop, cw - 2, cb - gasTop - 1));
        dc.DrawRectangle(Ui.Bg3, P(Faint, 1), new Rect(cx + 1, gasTop - 10, cw - 2, 10));
        Line(dc, Ui.TextDim, cx + cw / 2, gasTop - 10, cx + cw / 2, ctop - 4, 4);
        // molecules: deterministic paths whose speed grows with √T
        double speed = Math.Sqrt(t / 300) * 60, time = (HasResult ? Progress * AnimSeconds : 0) + 0.37;
        var rnd = new Random(3);
        double w = cw - 12, h = Math.Max(4, cb - gasTop - 12);
        for (int i = 0; i < 26; i++)
        {
            double x0 = rnd.NextDouble(), y0 = rnd.NextDouble(), ang = rnd.NextDouble() * Math.PI * 2;
            double px = Tri(x0 * w + Math.Cos(ang) * speed * time, w), py = Tri(y0 * h + Math.Sin(ang) * speed * time, h);
            Dot(dc, Yellow, cx + 6 + px, gasTop + 6 + py, 2.5);
        }
        Text(dc, $"{F(v, "0.0")} L · {F(t, "0")} K", cx + cw / 2, 4, 12, Ui.Text, 0.5, true);

        // ---- p–V diagram (right)
        double gx = 236, gw = 380, gy = 14, gh = 186;
        double v1 = V("V1"), v2 = V("V2"), vMax = Math.Max(Math.Max(v1, v2), 5) * 1.25;
        double pMax = 0;
        for (int i = 0; i <= 40; i++) pMax = Math.Max(pMax, At(v1 + (v2 - v1) * i / 40.0).p);
        pMax = Math.Max(pMax, 1) * 1.25 / 1000;   // kPa
        Point G(double vv, double pk) => new(gx + vv / vMax * gw, gy + gh - pk / pMax * gh);
        Line(dc, Ui.TextDim, gx, gy, gx, gy + gh, 1.5);
        Line(dc, Ui.TextDim, gx, gy + gh, gx + gw, gy + gh, 1.5);
        Text(dc, "p (kPa)", gx + 4, gy - 2, 11, Ui.TextDim);
        Text(dc, "V (L)", gx + gw, gy + gh - 16, 11, Ui.TextDim, 1);
        double vs = NiceStep(vMax, 6), ps = NiceStep(pMax, 4);
        for (double x = vs; x < vMax; x += vs) Text(dc, F(x, "0.#"), G(x, 0).X, gy + gh + 2, 10, Faint, 0.5);
        for (double y = ps; y < pMax; y += ps)
        {
            Line(dc, GridLine, gx, G(0, y).Y, gx + gw, G(0, y).Y, 1);
            Text(dc, F(y, "0.#"), gx - 4, G(0, y).Y - 7, 10, Faint, 1);
        }
        // isotherm through state 1 for reference
        double n = V("moles"), t1 = V("T1");
        var iso = new List<Point>();
        for (int i = 0; i <= 60; i++)
        {
            double vv = vMax * (0.04 + 0.96 * i / 60.0), pk = n * R * t1 / (vv / 1000) / 1000;
            if (pk <= pMax * 1.5) iso.Add(G(vv, Math.Min(pk, pMax * 1.2)));
        }
        Polyline(dc, P(Faint, 1, true), iso, iso.Count);
        var p1 = G(v1, At(v1).p / 1000);
        Dot(dc, Ui.Accent, p1.X, p1.Y, 4);
        Text(dc, "1", p1.X + 5, p1.Y - 16, 11, Ui.Accent, 0, true);
        if (!HasResult) return;
        var path = new List<Point>();
        int steps = 60, upto = (int)Math.Round(steps * Progress);
        for (int i = 0; i <= upto; i++) { double vv = v1 + (v2 - v1) * i / steps; path.Add(G(vv, At(vv).p / 1000)); }
        if (path.Count > 1)
        {
            // area under the curve = work
            var g = new StreamGeometry();
            using (var c = g.Open())
            {
                c.BeginFigure(new Point(path[0].X, gy + gh), true, true);
                foreach (var pt in path) c.LineTo(pt, true, false);
                c.LineTo(new Point(path[^1].X, gy + gh), true, false);
            }
            g.Freeze();
            dc.DrawGeometry(AccentSoft, null, g);
            Polyline(dc, P(Orange, 2.2), path, path.Count);
        }
        var cur = G(v, p / 1000);
        Dot(dc, Orange, cur.X, cur.Y, 4.5);
        if (Progress >= 1)
        {
            Text(dc, "2", cur.X + 5, cur.Y - 16, 11, Orange, 0, true);
            if (Math.Abs(v2 - v1) > 0.01) Text(dc, $"área = W = {F(_w, "0")} J", gx + gw - 4, gy + 4, 12, Ui.Accent, 1, true);
        }
    }

    /// <summary>Bounces a coordinate between 0 and len (triangle wave).</summary>
    static double Tri(double x, double len)
    {
        if (len <= 0) return 0;
        double m = ((x % (2 * len)) + 2 * len) % (2 * len);
        return m <= len ? m : 2 * len - m;
    }
}
