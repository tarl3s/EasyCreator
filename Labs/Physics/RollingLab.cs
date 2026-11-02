using System.Windows;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>A round body rolling without slipping down a ramp (g = 9.81 m/s², length from labParams "length", default
/// 2 m), released from rest at the top. With I = k·m·r² (solid sphere k = 2/5, solid cylinder 1/2, thin hoop 1, thin
/// spherical shell 2/3; "block" = a frictionless sliding block, k = 0): a = g·sin θ / (1 + k), t = √(2L/a),
/// v = √(2·g·h / (1 + k)) at the bottom, ω = v / r. Static friction needed to roll: μmin = k·tan θ / (1 + k).
/// Metrics: acceleration (m/s²), time (s), speed (m/s), omega (rad/s), height (m), inertiaFactor (k), inertia (kg·m²),
/// rotFraction (% of the kinetic energy that is rotation), minFriction, plus the inputs (shape, mass, radius, angle).</summary>
[Lab("rolling")]
public sealed class RollingLab : PhysLab
{
    const double G = 9.81;
    double _length = 2, _k, _a, _t, _v;

    public RollingLab()
    {
        Opt("shape", "Corpo", "hoop", ("block", "Bloco (desliza, sem atrito)"), ("sphere", "Esfera maciça"),
            ("cylinder", "Cilindro maciço"), ("shell", "Esfera oca"), ("hoop", "Aro (anel)"));
        Num("angle", "Inclinação da rampa", "°", 5, 60, 30, 1, "0");
        Num("mass", "Massa", "kg", 0.1, 10, 1, 0.1, "0.0");
        Num("radius", "Raio", "m", 0.02, 0.3, 0.1, 0.01, "0.00");
    }

    public static double InertiaFactor(string shape) => shape switch
    {
        "sphere" => 0.4, "cylinder" => 0.5, "shell" => 2.0 / 3, "hoop" => 1, _ => 0,
    };

    protected override void OnLoad(IReadOnlyDictionary<string, string> p)
    {
        if (p.TryGetValue("length", out var l) && Question.TryNum(l, out var v)) _length = Math.Clamp(v, 0.5, 20);
    }

    protected override void Compute()
    {
        double th = Rad(V("angle")), m = V("mass"), r = V("radius");
        _k = InertiaFactor(C("shape"));
        _a = G * Math.Sin(th) / (1 + _k);
        _t = Math.Sqrt(2 * _length / _a);
        _v = _a * _t;
        M["acceleration"] = _a;
        M["time"] = _t;
        M["speed"] = _v;
        M["omega"] = C("shape") == "block" ? 0 : _v / r;
        M["height"] = _length * Math.Sin(th);
        M["inertiaFactor"] = _k;
        M["inertia"] = _k * m * r * r;
        M["rotFraction"] = 100 * _k / (1 + _k);
        M["minFriction"] = _k * Math.Tan(th) / (1 + _k);
        AnimSeconds = Math.Clamp(_t * 1.5, 1.2, 5);
    }

    double T => HasResult ? Progress * _t : 0;
    double S => _a * T * T / 2;   // distance travelled along the ramp

    protected override string LiveText() =>
        L.F("t = {0} s   v = {1} m/s   distância = {2} m", F(T, "0.00"), F(_a * T, "0.00"), F(S, "0.00"));

    protected override IEnumerable<(string, string)> Results()
    {
        yield return (L.T("Aceleração"), F(_a, "0.00") + " m/s²");
        yield return (L.T("Tempo na rampa"), F(_t, "0.000") + " s");
        yield return (L.T("Vel. final"), F(_v, "0.00") + " m/s");
        yield return ("I", F(M["inertia"], "0.#####") + " kg·m²");
        yield return (L.T("Energia de rotação"), F(M["rotFraction"], "0.0") + " %");
        yield return (L.T("μ mínimo para rolar"), F(M["minFriction"], "0.000"));
    }

    protected override void Paint(DrawingContext dc)
    {
        double th = Rad(V("angle")), c = Math.Cos(th), sn = Math.Sin(th);
        double x0 = 30, yb = H - 24;
        double s = Math.Min(400 / (_length * c), 170 / (_length * sn));
        // ramp from the top (left) down to the bottom (right)
        double topX = x0, topY = yb - _length * sn * s, botX = x0 + _length * c * s;
        var g = new StreamGeometry();
        using (var ctx = g.Open())
        {
            ctx.BeginFigure(new Point(topX, topY), true, true);
            ctx.LineTo(new Point(botX, yb), true, false);
            ctx.LineTo(new Point(topX, yb), true, false);
        }
        g.Freeze();
        dc.DrawGeometry(Ui.Bg3, P(Faint, 1), g);
        Text(dc, $"{F(V("angle"), "0")}°", botX - 46, yb - 18, 11, Ui.TextDim);
        Text(dc, $"L = {F(_length, "0.##")} m", 8, 6, 12, Ui.TextDim);

        double d = HasResult ? S : 0;
        double rad = 12 + 40 * V("radius");
        // centre of the body: on the ramp surface, offset by its radius along the normal
        double px = topX + d * c * s, py = topY + d * sn * s;
        double nx = sn, ny = -c;   // outward normal of the ramp surface (up-right)
        double cx = px + nx * rad, cy = py + ny * rad;
        string shape = C("shape");
        if (shape == "block")
        {
            var rt = new RotateTransform(V("angle"), cx, cy);
            dc.PushTransform(rt);
            dc.DrawRectangle(Orange, null, new Rect(cx - rad, cy - rad, 2 * rad, 2 * rad));
            dc.Pop();
        }
        else
        {
            double turn = HasResult ? d / Math.Max(1e-6, V("radius")) : 0;   // rolled angle (rad)
            if (shape is "hoop" or "shell") dc.DrawEllipse(null, P(Orange, shape == "hoop" ? 4 : 3), new Point(cx, cy), rad - 2, rad - 2);
            else dc.DrawEllipse(Orange, null, new Point(cx, cy), rad, rad);
            Line(dc, shape is "hoop" or "shell" ? Orange : Ui.Code, cx, cy, cx + Math.Cos(turn) * (rad - 2), cy + Math.Sin(turn) * (rad - 2), 2);
        }

        // energy split bar
        double bx = 470, by = 40, bw = 150, bh = 16;
        double rot = HasResult ? M["rotFraction"] / 100 : KPreview() / (1 + KPreview());
        Text(dc, L.T("energia cinética"), bx, by - 18, 11, Ui.TextDim);
        dc.DrawRectangle(Blue, null, new Rect(bx, by, bw * (1 - rot), bh));
        dc.DrawRectangle(Purple, null, new Rect(bx + bw * (1 - rot), by, bw * rot, bh));
        Text(dc, L.T("translação"), bx, by + bh + 3, 11, Blue);
        Text(dc, L.T("rotação"), bx + bw, by + bh + 3, 11, Purple, 1);
        Text(dc, $"I = {F(KPreview(), "0.###")}·m·r²", bx, by + 46, 12, Ui.Text);
        if (HasResult && Progress >= 1)
            Text(dc, $"t = {F(_t, "0.000")} s", bx, by + 70, 13, Ui.Text, 0, true);
    }

    double KPreview() => InertiaFactor(C("shape"));
}
