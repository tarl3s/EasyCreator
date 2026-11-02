using System.Windows;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>Thin lens in air, paraxial rays (Gaussian optics): 1/f = 1/do + 1/di, m = −di/do. Real-is-positive
/// convention: di &gt; 0 is a real image on the other side of the lens, di &lt; 0 a virtual image on the object's side;
/// f &lt; 0 for a diverging lens. The ray diagram stretches heights (vertical scale bigger than horizontal) so the
/// arrows are readable; straight lines and their crossings are preserved.
/// Metrics: imageDistance (cm, signed; ∞ when the object is at the focus), magnification, imageHeight (cm, signed),
/// real (1/0), inverted (1/0), focal (cm, signed), plus the inputs (kind as index, focal length, objDist, objHeight).</summary>
[Lab("lens")]
public sealed class LensLab : PhysLab
{
    double _f, _do, _di, _m;
    bool _inf;

    public LensLab()
    {
        Opt("kind", "Tipo de lente", "converging", ("converging", "Convergente"), ("diverging", "Divergente"));
        Num("focal", "Distância focal |f|", "cm", 5, 30, 10, 1, "0");
        Num("objDist", "Distância do objeto", "cm", 3, 60, 30, 0.5, "0.0");
        Num("objHeight", "Altura do objeto", "cm", 1, 4, 2, 0.5, "0.0");
    }

    void Solve()
    {
        _f = (C("kind") == "diverging" ? -1 : 1) * V("focal");
        _do = V("objDist");
        _inf = Math.Abs(_do - _f) < 1e-9;
        _di = _inf ? double.PositiveInfinity : _f * _do / (_do - _f);
        _m = _inf ? double.PositiveInfinity : -_di / _do;
    }

    protected override void Compute()
    {
        Solve();
        M["imageDistance"] = _di;
        M["magnification"] = _m;
        M["imageHeight"] = _inf ? double.PositiveInfinity : _m * V("objHeight");
        M["real"] = !_inf && _di > 0 ? 1 : 0;
        M["inverted"] = !_inf && _m < 0 ? 1 : 0;
        M["focal"] = _f;
        AnimSeconds = 1.8;
    }

    protected override string LiveText() => "A luz sai do topo do objeto e atravessa a lente…";

    protected override IEnumerable<(string, string)> Results()
    {
        if (_inf) { yield return ("Imagem", "no infinito (objeto no foco)"); yield break; }
        yield return ("Distância da imagem", F(_di, "0.0") + " cm");
        yield return ("Aumento", F(_m, "0.00") + "×");
        yield return ("Altura da imagem", F(_m * V("objHeight"), "0.0") + " cm");
        yield return ("Imagem", (_di > 0 ? "real" : "virtual") + ", " + (_m < 0 ? "invertida" : "direita") + ", " +
                                (Math.Abs(_m) > 1.001 ? "maior" : Math.Abs(_m) < 0.999 ? "menor" : "mesmo tamanho"));
    }

    protected override void Paint(DrawingContext dc)
    {
        Solve();
        // px per cm: fit the object, both focal points and (when not too far) the image
        double ho0 = V("objHeight"), extent = Math.Max(_do + 4, 2 * Math.Abs(_f) + 4);
        if (!_inf && Math.Abs(_di) < 90) extent = Math.Max(extent, Math.Abs(_di) + 4);
        double lx = 320, ay = 118, sx = Math.Clamp(300 / extent, 3, 12);
        double sy = Math.Clamp(95 / Math.Max(ho0, _inf ? ho0 : Math.Min(Math.Abs(_m * ho0), 12)), 6, 24);
        Point S(double x, double y) => new(lx + x * sx, ay - y * sy);
        Line(dc, Faint, 0, ay, W, ay, 1);
        // focal points
        foreach (var (x, name) in new[] { (-Math.Abs(_f), "F"), (Math.Abs(_f), "F'"), (-2 * Math.Abs(_f), "2F"), (2 * Math.Abs(_f), "2F'") })
        {
            var p = S(x, 0);
            Dot(dc, name.StartsWith("2") ? Faint : Yellow, p.X, p.Y, 3);
            Text(dc, name, p.X, ay + 6, 11, name.StartsWith("2") ? Faint : Yellow, 0.5);
        }
        // lens
        bool conv = _f > 0;
        Line(dc, Blue, lx, ay - 105, lx, ay + 105, 2.5);
        foreach (int d in new[] { -1, 1 })
        {
            double yy = ay + d * 105, k = conv ? -d : d;
            Line(dc, Blue, lx, yy, lx - 8, yy + k * 8, 2.5);
            Line(dc, Blue, lx, yy, lx + 8, yy + k * 8, 2.5);
        }
        Text(dc, conv ? "lente convergente" : "lente divergente", lx + 14, 4, 11, Blue);
        Text(dc, $"f = {F(_f, "0")} cm", lx + 14, 18, 11, Blue);

        // object
        double ho = V("objHeight");
        var ob = S(-_do, 0); var ot = S(-_do, ho);
        Arrow(dc, Ui.Accent, ob.X, ob.Y, ot.X, ot.Y, 3);
        Text(dc, "objeto", ob.X, ay + 20, 11, Ui.Accent, 0.5);

        if (!HasResult) return;
        // rays revealed from left to right
        dc.PushClip(new RectangleGeometry(new Rect(0, 0, 40 + Progress * (W - 40), H)));
        var ray = P(Orange, 1.5);
        var back = P(Orange, 1, true);
        double farRight = (W - lx) / sx + 5, farLeft = -lx / sx - 5;
        // 1) parallel to the axis, then through F' (or away from F for a diverging lens)
        dc.DrawLine(ray, ot, S(0, ho));
        double slope1 = -ho / _f;
        dc.DrawLine(ray, S(0, ho), S(farRight, ho + slope1 * farRight));
        // 2) through the centre, undeviated
        double slope2 = -ho / _do;
        dc.DrawLine(ray, ot, S(farRight, ho + slope2 * (farRight + _do)));
        // 3) towards F (front focus), leaves parallel
        if (!_inf)
        {
            double yl = -ho * _f / (_do - _f);
            if (Math.Abs(yl) * sy < 400)
            {
                dc.DrawLine(ray, ot, S(0, yl));
                dc.DrawLine(ray, S(0, yl), S(farRight, yl));
                if (_di < 0) dc.DrawLine(back, S(0, yl), S(farLeft, yl));
            }
        }
        if (!_inf && _di < 0)
        {
            // virtual image: extend the outgoing rays backwards (dashed)
            dc.DrawLine(back, S(0, ho), S(farLeft, ho + slope1 * farLeft));
            dc.DrawLine(back, S(0, 0), S(farLeft, slope2 * farLeft));
        }
        dc.Pop();

        if (Progress >= 1 && !_inf)
        {
            double hi = _m * ho;
            var ib = S(_di, 0); var it = S(_di, hi);
            if (ib.X > -20 && ib.X < W + 20)
            {
                var pen = _di > 0 ? Ui.Bad : Purple;
                Arrow(dc, pen, ib.X, ib.Y, it.X, Math.Clamp(it.Y, -50, H + 50), 3);
                Text(dc, _di > 0 ? "imagem real" : "imagem virtual", ib.X, hi > 0 ? ay + 20 : ay - 34, 11, pen, 0.5, true);
            }
            else Text(dc, "a imagem fica fora da tela →", W - 8, H - 18, 11, Ui.TextDim, 1);
        }
        else if (Progress >= 1) Text(dc, "raios saem paralelos: imagem no infinito", W - 8, H - 18, 11, Ui.TextDim, 1);
    }
}
