using System.Windows;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>A string fixed at both ends (length L, tension T, linear density μ in g/m) vibrating in its n-th harmonic.
/// Wave speed v = √(T/μ), wavelength λ = 2L/n, frequency f = n·v/(2L); nodes = n + 1, antinodes = n.
/// Metrics: waveSpeed (m/s), wavelength (m), frequency (Hz), fundamental (Hz, n = 1), nodes, harmonic (n),
/// plus the inputs (tension, density, length).</summary>
[Lab("string")]
public sealed class StringLab : PhysLab
{
    double _v, _f;

    public StringLab()
    {
        Num("tension", "Tensão na corda T", "N", 1, 200, 20, 0.1, "0.0");
        Num("density", "Densidade linear μ", "g/m", 0.1, 10, 0.6, 0.05, "0.00");
        Num("length", "Comprimento da corda L", "m", 0.1, 2, 0.65, 0.005, "0.000");
        Num("harmonic", "Harmônico n", "", 1, 6, 1, 1, "0");
    }

    int N => (int)Math.Round(V("harmonic"));

    protected override void Compute()
    {
        double T = V("tension"), mu = V("density") / 1000, L = V("length");
        _v = Math.Sqrt(T / mu);
        _f = N * _v / (2 * L);
        M["waveSpeed"] = _v;
        M["wavelength"] = 2 * L / N;
        M["frequency"] = _f;
        M["fundamental"] = _v / (2 * L);
        M["nodes"] = N + 1;
        M["harmonic"] = N;
        AnimSeconds = 3;
    }

    protected override string LiveText() => L.F("f = {0} Hz   λ = {1} m   v = {2} m/s", F(_f, "0.0"), F(2 * V("length") / N, "0.000"), F(_v, "0.0"));

    protected override IEnumerable<(string, string)> Results()
    {
        yield return (L.T("Frequência"), F(_f, "0.0") + " Hz");
        yield return (L.T("Vel. da onda"), F(_v, "0.0") + " m/s");
        yield return ("λ", F(2 * V("length") / N, "0.000") + " m");
        yield return (L.T("Nós"), (N + 1).ToString());
        yield return (L.T("Nota"), Note(_f));
    }

    /// <summary>Nearest note of the equal-tempered scale (A4 = 440 Hz) and the deviation in cents.</summary>
    public static string Note(double f)
    {
        if (!(f > 0)) return "—";
        string[] names = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        double semis = 12 * Math.Log2(f / 440);
        int k = (int)Math.Round(semis);
        int midi = 69 + k;
        int cents = (int)Math.Round(100 * (semis - k));
        return $"{names[((midi % 12) + 12) % 12]}{(int)Math.Floor(midi / 12.0) - 1} {(cents >= 0 ? "+" : "−")}{Math.Abs(cents)} ¢";
    }

    protected override void Paint(DrawingContext dc)
    {
        double x0 = 40, x1 = W - 40, y0 = 110, amp = 60;
        int n = N;
        Dot(dc, Ui.Text, x0, y0, 5);
        Dot(dc, Ui.Text, x1, y0, 5);
        Line(dc, Faint, x0, y0, x1, y0, 1, true);
        double phase = HasResult ? Math.Cos(2 * Math.PI * Progress * 6) : 1;
        foreach (var (k, brush, th) in new[] { (phase, Orange, 2.5), (-phase, Faint, 1.0) })
        {
            if (!HasResult && brush == Faint) continue;
            var pts = new List<Point>();
            for (int i = 0; i <= 200; i++)
            {
                double u = i / 200.0;
                pts.Add(new Point(x0 + u * (x1 - x0), y0 - k * amp * Math.Sin(n * Math.PI * u)));
            }
            Polyline(dc, P(brush, th), pts, pts.Count);
        }
        for (int i = 0; i <= n; i++) Dot(dc, Blue, x0 + i * (x1 - x0) / n, y0, 3.5);
        Text(dc, $"n = {n}   L = {F(V("length"), "0.000")} m", 8, 6, 12, Ui.TextDim);
        Text(dc, $"T = {F(V("tension"), "0.0")} N   μ = {F(V("density"), "0.00")} g/m", 8, 22, 12, Ui.TextDim);
        if (HasResult) Text(dc, $"f = {F(_f, "0.0")} Hz  ({Note(_f)})", W - 8, 6, 13, Ui.Text, 1, true);
    }
}
