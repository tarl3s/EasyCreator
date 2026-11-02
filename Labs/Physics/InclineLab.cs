using System.Windows;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>Block on an inclined plane (g = 9.81 m/s²), starting at rest in the middle of a ramp (labParams "length",
/// default 6 m). Applied force parallel to the ramp (positive = up the slope); one friction coefficient μ used both
/// as static and kinetic. The run ends when the block reaches an end of the ramp or after 4 s.
/// Metrics: acceleration (m/s², + = up the slope), normalForce, frictionForce, netForce, weightAlong (N), slides (1/0),
/// finalSpeed (m/s), time (s), plus the inputs (mass, angle, friction, force).</summary>
[Lab("incline")]
public sealed class InclineLab : PhysLab
{
    const double G = 9.81;
    double _length = 6, _a, _tEnd, _n, _fr, _wPar, _vEnd;
    bool _slides;

    public InclineLab()
    {
        Num("mass", "Massa do bloco", "kg", 1, 20, 5, 0.5, "0.0");
        Num("angle", "Inclinação da rampa", "°", 0, 60, 30, 1, "0");
        Num("friction", "Coeficiente de atrito μ", "", 0, 1, 0.2, 0.01, "0.00");
        Num("force", "Força aplicada (rampa acima)", "N", -100, 200, 0, 1, "0");
    }

    protected override void OnLoad(IReadOnlyDictionary<string, string> p)
    {
        if (p.TryGetValue("length", out var l) && Question.TryNum(l, out var v)) _length = Math.Clamp(v, 1, 50);
    }

    void Forces()
    {
        double m = V("mass"), th = Rad(V("angle")), mu = V("friction"), f = V("force");
        _n = m * G * Math.Cos(th);
        _wPar = m * G * Math.Sin(th);
        double drive = f - _wPar, fsMax = mu * _n;
        if (Math.Abs(drive) <= fsMax + 1e-9) { _slides = false; _fr = Math.Abs(drive); _a = 0; }
        else { _slides = true; _fr = fsMax; _a = (drive - Math.Sign(drive) * fsMax) / m; }
    }

    protected override void Compute()
    {
        Forces();
        double half = _length / 2;
        _tEnd = _a == 0 ? 4 : Math.Min(4, Math.Sqrt(2 * half / Math.Abs(_a)));
        _vEnd = Math.Abs(_a) * _tEnd;
        M["acceleration"] = _a;
        M["normalForce"] = _n;
        M["frictionForce"] = _fr;
        M["netForce"] = V("mass") * _a;
        M["weightAlong"] = _wPar;
        M["slides"] = _slides ? 1 : 0;
        M["finalSpeed"] = _vEnd;
        M["time"] = _tEnd;
        AnimSeconds = Math.Clamp(_tEnd, 1.5, 4);
    }

    double T => HasResult ? Progress * _tEnd : 0;
    double Pos => _length / 2 + _a * T * T / 2;   // distance from the bottom of the ramp

    protected override string LiveText() =>
        $"t = {F(T, "0.00")} s   v = {F(_a * T, "0.00")} m/s   posição = {F(Pos, "0.00")} m da base";

    protected override IEnumerable<(string, string)> Results()
    {
        yield return ("Aceleração", F(_a, "0.00") + " m/s²");
        yield return ("Normal", F(_n, "0.0") + " N");
        yield return ("Atrito", F(_fr, "0.0") + " N");
        yield return ("Resultante", F(V("mass") * _a, "0.0") + " N");
        yield return ("Vel. final", F(_vEnd, "0.00") + " m/s");
        yield return ("Movimento", !_slides ? "parado" : _a > 0 ? "sobe a rampa" : "desce a rampa");
    }

    protected override void Paint(DrawingContext dc)
    {
        if (!HasResult) Forces();
        double th = Rad(V("angle")), c = Math.Cos(th), sn = Math.Sin(th);
        double x0 = 24, yb = H - 22;
        double s = Math.Min(370 / (_length * c + 1e-9), 175 / (_length * sn + 1e-9));
        s = Math.Min(s, 370 / _length);
        var top = new Point(x0 + _length * c * s, yb - _length * sn * s);
        // ground and ramp
        Line(dc, Ui.TextDim, 0, yb, 430, yb, 2);
        var ramp = new StreamGeometry();
        using (var g = ramp.Open())
        {
            g.BeginFigure(new Point(x0, yb), true, true);
            g.LineTo(top, true, false);
            g.LineTo(new Point(top.X, yb), true, false);
        }
        ramp.Freeze();
        dc.DrawGeometry(Ui.Bg3, P(Faint, 1.5), ramp);
        Text(dc, $"{F(V("angle"), "0")}°", x0 + 46, yb - 18, 12, Ui.TextDim);
        Text(dc, $"rampa de {F(_length, "0.#")} m", (x0 + top.X) / 2, yb + 4, 11, Ui.TextDim, 0.5);

        // block (clamped to the ramp)
        double bs = 30, edge = Math.Min(_length / 2, bs / 2 / s);
        double pos = Math.Clamp(Pos, edge, _length - edge);
        var foot = new Point(x0 + pos * c * s, yb - pos * sn * s);
        var center = new Point(foot.X - sn * bs / 2, foot.Y - c * bs / 2);
        dc.PushTransform(new RotateTransform(-V("angle"), center.X, center.Y));
        dc.DrawRoundedRectangle(Blue, P(Ui.Text, 1), new Rect(center.X - bs / 2, center.Y - bs / 2, bs, bs), 3, 3);
        Text(dc, $"{F(V("mass"), "0.#")} kg", center.X, center.Y - 8, 11, Ui.Code, 0.5, true);
        dc.Pop();

        // free-body diagram on the right
        double cx = 540, cy = 112;
        dc.DrawRoundedRectangle(Ui.Bg1, P(GridLine, 1), new Rect(440, 6, 194, H - 12), 6, 6);
        Text(dc, "Forças no bloco", 537, 10, 12, Ui.TextDim, 0.5, true);
        double m = V("mass"), f = V("force");
        double max = Math.Max(1, new[] { m * G, _n, _fr, Math.Abs(f) }.Max()), k = 64 / max;
        double ux = c, uy = -sn;          // unit vector up the slope (screen coordinates)
        double nx = -sn, ny = -c;         // normal, away from the ramp
        dc.PushTransform(new RotateTransform(-V("angle"), cx, cy));
        dc.DrawRectangle(Faint, null, new Rect(cx - 8, cy - 8, 16, 16));
        dc.Pop();
        Arrow(dc, Orange, cx, cy, cx, cy + m * G * k);
        Text(dc, $"P = {F(m * G, "0")} N", cx + 6, cy + m * G * k - 14, 11, Orange);
        Arrow(dc, Blue, cx, cy, cx + nx * _n * k, cy + ny * _n * k);
        Text(dc, $"N = {F(_n, "0")} N", cx + nx * _n * k - 4, cy + ny * _n * k - 16, 11, Blue, 0.5);
        if (Math.Abs(f) > 0.01)
        {
            Arrow(dc, Ui.Accent, cx, cy, cx + ux * f * k, cy + uy * f * k);
            Text(dc, $"F = {F(f, "0")} N", Math.Min(cx + ux * f * k, 600), cy + uy * f * k - 18, 11, Ui.Accent, 0.5);
        }
        if (_fr > 0.01)
        {
            // friction opposes motion (or the tendency to move): it points against (F − P·sinθ)
            double dir = -Math.Sign(f - _wPar);
            Arrow(dc, Ui.Bad, cx, cy, cx + ux * dir * _fr * k, cy + uy * dir * _fr * k);
            Text(dc, $"Fat = {F(_fr, "0")} N", Math.Clamp(cx + ux * dir * _fr * k, 480, 600), cy + uy * dir * _fr * k + 8, 11, Ui.Bad, 0.5);
        }
        if (HasResult)
        {
            string mv = !_slides ? "em repouso" : $"a = {F(_a, "0.00")} m/s²";
            Text(dc, mv, 537, H - 26, 12, _slides ? Yellow : Ui.TextDim, 0.5, true);
            if (_slides)
            {
                double dir = Math.Sign(_a);
                Arrow(dc, Yellow, center.X + nx * 28, center.Y + ny * 28, center.X + nx * 28 + ux * dir * 34, center.Y + ny * 28 + uy * dir * 34, 2);
            }
        }
    }
}
