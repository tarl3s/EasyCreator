using System.Windows;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>Two carts on a frictionless straight track (1D). They collide once if cart 1 is faster than cart 2;
/// the coefficient of restitution e decides the kind of collision (1 = elastic, 0 = perfectly inelastic: they stick).
/// v1' = (m1·v1 + m2·v2 + m2·e·(v2 − v1)) / (m1 + m2), v2' = (m1·v1 + m2·v2 + m1·e·(v1 − v2)) / (m1 + m2).
/// Metrics: v1After, v2After (m/s), pBefore, pAfter (kg·m/s), keBefore, keAfter, keLost (J), keLostPct (%),
/// collided (1/0), plus the inputs (m1, v1, m2, v2, e).</summary>
[Lab("collisions")]
public sealed class CollisionLab : PhysLab
{
    const double X1 = 2.5, X2 = 6.5, Len = 0.8;   // initial centres and cart length (m)
    double _tc = double.PositiveInfinity, _tSim, _u1, _u2;
    bool _hit;

    public CollisionLab()
    {
        Num("m1", "Massa do carrinho 1", "kg", 0.5, 5, 1, 0.5, "0.0");
        Num("v1", "Velocidade do carrinho 1", "m/s", -3, 5, 2, 0.1, "0.0");
        Num("m2", "Massa do carrinho 2", "kg", 0.5, 5, 2, 0.5, "0.0");
        Num("v2", "Velocidade do carrinho 2", "m/s", -5, 3, 0, 0.1, "0.0");
        Num("e", "Coef. de restituição e (1 = elástica)", "", 0, 1, 1, 0.05, "0.00");
    }

    protected override void Compute()
    {
        double m1 = V("m1"), v1 = V("v1"), m2 = V("m2"), v2 = V("v2"), e = V("e");
        _hit = v1 > v2;
        _tc = _hit ? (X2 - X1 - Len) / (v1 - v2) : double.PositiveInfinity;
        if (_hit)
        {
            double p = m1 * v1 + m2 * v2;
            _u1 = (p + m2 * e * (v2 - v1)) / (m1 + m2);
            _u2 = (p + m1 * e * (v1 - v2)) / (m1 + m2);
        }
        else { _u1 = v1; _u2 = v2; }
        // Stop the replay a little after the crash, or when a cart reaches the end of the 10 m track.
        double t0 = _hit ? _tc : 0, c1 = X1 + v1 * t0, c2 = X2 + v2 * t0;
        _tSim = t0 + Math.Max(0.5, Math.Min(Math.Min(ToEdge(c1, _u1), ToEdge(c2, _u2)), _hit ? 2.5 : 4));

        double keB = 0.5 * m1 * v1 * v1 + 0.5 * m2 * v2 * v2, keA = 0.5 * m1 * _u1 * _u1 + 0.5 * m2 * _u2 * _u2;
        M["v1After"] = _u1;
        M["v2After"] = _u2;
        M["pBefore"] = m1 * v1 + m2 * v2;
        M["pAfter"] = m1 * _u1 + m2 * _u2;
        M["keBefore"] = keB;
        M["keAfter"] = keA;
        M["keLost"] = keB - keA;
        M["keLostPct"] = keB > 1e-12 ? (keB - keA) / keB * 100 : 0;
        M["collided"] = _hit ? 1 : 0;
        AnimSeconds = Math.Clamp(_tSim * 0.75, 2, 6);
    }

    static double ToEdge(double x, double v) => v > 1e-9 ? (9.6 - x) / v : v < -1e-9 ? (x - 0.4) / -v : double.PositiveInfinity;

    double T => HasResult ? Progress * _tSim : 0;

    (double x1, double x2, double v1, double v2) State()
    {
        double t = T, v1 = V("v1"), v2 = V("v2");
        if (t <= _tc) return (X1 + v1 * t, X2 + v2 * t, v1, v2);
        double c1 = X1 + v1 * _tc, c2 = X2 + v2 * _tc, d = t - _tc;
        return (c1 + _u1 * d, c2 + _u2 * d, _u1, _u2);
    }

    protected override string LiveText()
    {
        var s = State();
        double p = V("m1") * s.v1 + V("m2") * s.v2;
        return L.F("t = {0} s   v1 = {1} m/s   v2 = {2} m/s   p total = {3} kg·m/s", F(T, "0.00"), F(s.v1, "0.00"), F(s.v2, "0.00"), F(p, "0.00"));
    }

    protected override IEnumerable<(string, string)> Results()
    {
        if (!_hit) { yield return (L.T("Colisão"), L.T("não bateram (o 1 não alcança o 2)")); }
        yield return (L.T("v1 depois"), F(_u1, "0.00") + " m/s");
        yield return (L.T("v2 depois"), F(_u2, "0.00") + " m/s");
        yield return (L.T("p antes/depois"), $"{F(M["pBefore"], "0.00")} / {F(M["pAfter"], "0.00")} kg·m/s");
        yield return (L.T("Ec antes/depois"), $"{F(M["keBefore"], "0.00")} / {F(M["keAfter"], "0.00")} J");
        yield return (L.T("Energia perdida"), F(M["keLostPct"], "0.0") + " %");
    }

    protected override void Paint(DrawingContext dc)
    {
        double left = 20, s = 60, rail = 100;   // 60 px per metre, track 0..10 m
        Line(dc, Ui.TextDim, 0, rail, W, rail, 2);
        for (int m = 0; m <= 10; m++)
        {
            Line(dc, GridLine, left + m * s, rail + 2, left + m * s, rail + 8, 1);
            Text(dc, m + " m", left + m * s, rail + 9, 10, Faint, 0.5);
        }
        var st = HasResult ? State() : (X1, X2, V("v1"), V("v2"));
        Cart(dc, left + st.Item1 * s, rail, V("m1"), "1", Blue, st.Item3);
        Cart(dc, left + st.Item2 * s, rail, V("m2"), "2", Orange, st.Item4);
        if (HasResult && T >= _tc && T < _tc + 0.25)
        {
            double cx = left + (State().x1 + Len / 2) * s;
            for (int k = 0; k < 8; k++)
            {
                double ang = k * Math.PI / 4;
                Line(dc, Yellow, cx + Math.Cos(ang) * 8, rail - 20 + Math.Sin(ang) * 8, cx + Math.Cos(ang) * 16, rail - 20 + Math.Sin(ang) * 16, 2);
            }
        }

        // bars: momentum and kinetic energy, before and after
        double m1 = V("m1"), m2 = V("m2"), v1 = V("v1"), v2 = V("v2");
        double pB = m1 * v1 + m2 * v2, kB = 0.5 * m1 * v1 * v1 + 0.5 * m2 * v2 * v2;
        bool after = HasResult && T >= _tc;
        double pA = m1 * _u1 + m2 * _u2, kA = 0.5 * m1 * _u1 * _u1 + 0.5 * m2 * _u2 * _u2;
        double top = 140;
        dc.DrawRoundedRectangle(Ui.Bg1, P(GridLine, 1), new Rect(8, top, W - 16, H - top - 6), 6, 6);
        double pMax = Math.Max(1e-6, Math.Max(Math.Abs(pB), Math.Abs(pA))), kMax = Math.Max(1e-6, Math.Max(kB, kA));
        void Bar(double x, double y, string label, double val, double max, Brush b, bool show)
        {
            Text(dc, label, x, y, 11, Ui.TextDim);
            double zero = x + 150, w = 120;
            dc.DrawRectangle(Ui.Bg2, null, new Rect(zero, y + 2, w, 11));
            if (show)
            {
                double len = Math.Abs(val) / max * w;
                dc.DrawRectangle(b, null, new Rect(zero, y + 2, Math.Max(0.5, len), 11));
                Text(dc, F(val, "0.00") + (val < 0 ? " (←)" : ""), zero + w + 6, y, 11, Ui.Text);
            }
            else Text(dc, "…", zero + w + 6, y, 11, Faint);
        }
        Text(dc, L.T("Quantidade de movimento p = m·v"), 18, top + 6, 11.5, Ui.Text, 0, true);
        Bar(18, top + 26, L.T("antes (kg·m/s)"), pB, pMax, Blue, true);
        Bar(18, top + 46, L.T("depois (kg·m/s)"), pA, pMax, Blue, after);
        Text(dc, L.T("Energia cinética Ec = m·v²/2"), 330, top + 6, 11.5, Ui.Text, 0, true);
        Bar(330, top + 26, L.T("antes (J)"), kB, kMax, Orange, true);
        Bar(330, top + 46, L.T("depois (J)"), kA, kMax, Orange, after);
        if (HasResult && !_hit) Text(dc, L.T("Sem colisão: o carrinho 1 precisa ser mais rápido que o 2 (v1 > v2)."), W / 2, top + 66, 11, Ui.Bad, 0.5);
    }

    void Cart(DrawingContext dc, double cx, double rail, double mass, string name, Brush b, double v)
    {
        double w = Len * 60, h = 16 + 5 * mass;
        var r = new Rect(cx - w / 2, rail - 6 - h, w, h);
        dc.DrawRoundedRectangle(b, P(Ui.Text, 1), r, 4, 4);
        Dot(dc, Ui.Text, cx - w / 3, rail - 4, 4);
        Dot(dc, Ui.Text, cx + w / 3, rail - 4, 4);
        Text(dc, name, cx, r.Top + h / 2 - 9, 13, Ui.Code, 0.5, true);
        Text(dc, F(mass, "0.#") + " kg", cx, r.Top - 16, 11, Ui.TextDim, 0.5);
        if (Math.Abs(v) > 0.01)
        {
            double ay = r.Top - 24;
            Arrow(dc, Yellow, cx, ay, cx + v * 14, ay, 2);
            // label past the arrow tip; near an edge of the track it goes behind the arrow instead of over it
            double tip = cx + v * 14, tx = tip + (v > 0 ? 4 : -4), align = v > 0 ? 0 : 1;
            if (v > 0 && tx > W - 50) { tx = cx - 4; align = 1; }
            else if (v < 0 && tx < 50) { tx = cx + 4; align = 0; }
            Text(dc, F(v, "0.0") + " m/s", tx, ay - 8, 11, Yellow, align);
        }
    }
}
