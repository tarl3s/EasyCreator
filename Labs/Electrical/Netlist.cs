using System.Globalization;
using System.Text.RegularExpressions;
using Complex = System.Numerics.Complex;

namespace EasyCreator.Electrical;

/*
 Netlist format (labParams "netlist") — one element per statement, statements separated by ';' or new lines:

     <Name> <nodeA> <nodeB> [value] [flags] [key=value ...]

 The first letter of the name is the element kind:
     R  resistor (Ω)                      R1 n1 n2 4.7k        flags: lamp (drawn as a bulb), w=60 (bulb rated watts)
     C  capacitor (F)                     C1 n2 0 100u
     L  inductor (H)                      L1 n2 n3 10m
     V  voltage source, + at nodeA        V1 n1 0 12           DC (dc/transient) or RMS phasor in AC mode, ph=-120 (degrees)
                                          V1 a b SIN 17 60     transient sine: peak 17 V, 60 Hz (ph= optional)
                                          V1 a 0 SQ 5 2        transient square wave 0..5 V at 2 Hz
     I  current source: pushes the current OUT of nodeA into the circuit (back in through nodeB)
     D  diode, anode = nodeA, cathode = nodeB (ideal switch + vf + ron)  flags: led (vf 2.0, ron 10), vf=0.7 ron=0.1 color=red
     S  switch                            S1 n1 n2 off         (on = closed, off = open)
     M  voltmeter (infinite impedance)    M1 a b               shows the voltage A-B on the drawing
     A  ammeter (0 Ω)                     A1 n1 n2             shows the current A→B on the drawing
     W  wire (zero ohm; joins two drawing points into one electrical node). Wires may all be called just "W".

 Values take SI prefixes and optional units: 4.7k, 4k7 is NOT accepted, 100u / 100µ, 10m (milli), 1M or 1meg (mega),
 2.2nF, 60Hz, 220V. Decimal comma or point. Ground is node "0"; every name starting with "0" (0a, 0b…) or "gnd" is
 ground too, so a drawing can have several ground points. label=Chuveiro_elétrico sets the drawn name (_ = space);
 side=up|down|left|right puts the name on that side of the symbol (default: away from the drawing center).

 Drawing (labParams "layout"): "n1=0,0 n2=3,0 0b=6,2" grid coordinates of the drawing points (1 unit ≈ 40–60 px,
 y grows downwards). Every element is a straight segment between its two points (diagonals are fine, e.g. a diode
 bridge). An element going to a ground name WITHOUT a position is drawn 2 units straight down with a ground symbol.
 Points without a position are placed in a row (good enough for a simple ladder).
*/

public enum ElemKind { R, C, L, V, I, D, S, M, A, W }

/// <summary>One netlist element plus the solver's state and its last results.</summary>
public sealed class Elem
{
    public string Name = "", A = "", B = "";
    public ElemKind Kind;
    /// <summary>Ω, F, H; V/A for sources (DC level, transient peak/high level, or RMS in AC mode).</summary>
    public double Value;
    public string Wave = "dc";      // dc | sin | sq (transient only)
    public double Freq;             // sin/sq frequency (transient)
    public double PhaseDeg;         // AC phasor angle / sine phase
    public double Vf = 0.7, Ron = 0.1;
    public bool Led, Lamp;
    public double Watts;            // lamp rated power (full brightness)
    public string Color = "";
    public bool On = true;          // switch closed
    public string Label = "";
    /// <summary>Drawing hint: which side of the symbol the name goes (up, down, left, right); empty = away from the center.</summary>
    public string Side = "";

    // Solver bookkeeping.
    internal int Na = -1, Nb = -1, Br = -1;
    internal bool DOn;
    internal double Vprev, Iprev, Vprev2, Iprev2, Geq, Jeq;   // the last two points (BDF2) and the companion model

    /// <summary>Results (DC / end of transient): voltage A−B and current A→B (sources: current delivered out of +).</summary>
    public double Vd, Id;
    /// <summary>AC results: phasors (RMS) of the voltage A−B and of the current A→B (sources: delivered).</summary>
    public Complex Vac, Iac;

    public bool IsSource => Kind is ElemKind.V or ElemKind.I;

    public string Unit => Kind switch
    {
        ElemKind.R => "Ω", ElemKind.C => "F", ElemKind.L => "H", ElemKind.V => "V", ElemKind.I => "A", _ => "",
    };

    public string DisplayName => Label.Length > 0 ? Label : Name;
}

public sealed class Netlist
{
    public List<Elem> Elems { get; } = new();
    public List<string> Errors { get; } = new();
    /// <summary>Drawing point name → electrical node index (−1 = ground).</summary>
    public Dictionary<string, int> Node { get; } = new(StringComparer.Ordinal);
    /// <summary>Number of non-ground electrical nodes.</summary>
    public int NodeCount { get; private set; }
    /// <summary>Every point name, in order of first appearance.</summary>
    public List<string> Names { get; } = new();

    public Elem? Find(string name) => Elems.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public static bool IsGround(string n) => n.StartsWith('0') || n.StartsWith("gnd", StringComparison.OrdinalIgnoreCase);

    public int IndexOf(string node) => Node.TryGetValue(node, out var i) ? i : -1;

    public static Netlist Parse(string text)
    {
        var net = new Netlist();
        foreach (var raw in (text ?? "").Split(';', '\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('*') || line.StartsWith("//")) continue;
            var tok = Regex.Split(line, @"\s+");
            if (tok.Length < 3) { net.Errors.Add(L.F("Linha incompleta: \"{0}\"", line)); continue; }
            var e = new Elem { Name = tok[0], A = tok[1], B = tok[2] };
            switch (char.ToUpperInvariant(tok[0][0]))
            {
                case 'R': e.Kind = ElemKind.R; break;
                case 'C': e.Kind = ElemKind.C; break;
                case 'L': e.Kind = ElemKind.L; break;
                case 'V': e.Kind = ElemKind.V; break;
                case 'I': e.Kind = ElemKind.I; break;
                case 'D': e.Kind = ElemKind.D; break;
                case 'S': e.Kind = ElemKind.S; break;
                case 'M': e.Kind = ElemKind.M; break;
                case 'A': e.Kind = ElemKind.A; break;
                case 'W': e.Kind = ElemKind.W; break;
                default: net.Errors.Add(L.F("Componente desconhecido: \"{0}\"", tok[0])); continue;
            }
            var nums = new List<double>();
            foreach (var t in tok.Skip(3))
            {
                int eq = t.IndexOf('=');
                if (eq > 0)
                {
                    string k = t[..eq].ToLowerInvariant(), v = t[(eq + 1)..];
                    SI.TryParse(v, out var d);
                    switch (k)
                    {
                        case "ph": e.PhaseDeg = d; break;
                        case "vf": e.Vf = d; break;
                        case "ron": e.Ron = Math.Max(d, 1e-3); break;
                        case "w": e.Watts = d; break;
                        case "color": e.Color = v; break;
                        case "label": e.Label = v.Replace('_', ' '); break;
                        case "side": e.Side = v.ToLowerInvariant(); break;
                    }
                    continue;
                }
                switch (t.ToLowerInvariant())
                {
                    case "lamp": e.Lamp = true; continue;
                    case "led": e.Led = true; e.Vf = 2.0; e.Ron = 10; if (e.Color.Length == 0) e.Color = "red"; continue;
                    case "on": e.On = true; continue;
                    case "off": e.On = false; continue;
                    case "sin": e.Wave = "sin"; continue;
                    case "sq": e.Wave = "sq"; continue;
                    case "dc": e.Wave = "dc"; continue;
                }
                if (SI.TryParse(t, out var n)) nums.Add(n);
                else net.Errors.Add(L.F("{0}: valor \"{1}\" não entendido", e.Name, t));
            }
            if (e.Kind is ElemKind.R or ElemKind.C or ElemKind.L or ElemKind.V or ElemKind.I)
            {
                if (nums.Count == 0) { net.Errors.Add(L.F("{0}: falta o valor", e.Name)); continue; }
                e.Value = nums[0];
                if (e.Wave != "dc" && nums.Count > 1) e.Freq = nums[1];
            }
            if (e.Kind != ElemKind.W && net.Find(e.Name) != null) { net.Errors.Add(L.F("Nome repetido: {0}", e.Name)); continue; }
            net.Elems.Add(e);
        }
        net.BuildNodes();
        return net;
    }

    void BuildNodes()
    {
        var parent = new Dictionary<string, string>(StringComparer.Ordinal);
        string Root(string x)
        {
            while (parent[x] != x) x = parent[x] = parent[parent[x]];
            return x;
        }
        void Add(string x)
        {
            if (parent.ContainsKey(x)) return;
            parent[x] = x;
            Names.Add(x);
        }
        foreach (var e in Elems) { Add(e.A); Add(e.B); }
        foreach (var e in Elems.Where(e => e.Kind == ElemKind.W))
        {
            string ra = Root(e.A), rb = Root(e.B);
            if (ra != rb) parent[ra] = rb;
        }
        var groundRoots = new HashSet<string>(Names.Where(IsGround).Select(Root));
        var index = new Dictionary<string, int>();
        foreach (var n in Names)
        {
            var r = Root(n);
            if (groundRoots.Contains(r)) { Node[n] = -1; continue; }
            if (!index.TryGetValue(r, out var i)) index[r] = i = index.Count;
            Node[n] = i;
        }
        NodeCount = index.Count;
        foreach (var e in Elems) { e.Na = Node[e.A]; e.Nb = Node[e.B]; }
    }
}

/// <summary>SI prefixes: parse "4.7k", "100u", "2,2 mF"; format 0.0047 → "4,70 m".</summary>
public static class SI
{
    static readonly CultureInfo Br = CultureInfo.GetCultureInfo("pt-BR");
    /// <summary>Decimal comma in Portuguese and Spanish, decimal point in English.</summary>
    static CultureInfo Culture => L.Lang == "en" ? CultureInfo.InvariantCulture : Br;

    static readonly Regex Num = new(@"^([-+]?(?:\d+\.?\d*|\.\d+)(?:[eE][-+]?\d+)?)\s*(meg|[TGMkKmuµnpf])?\s*(Ω|ohms?|VA|var|Hz|V|A|F|H|W|s)?$",
        RegexOptions.Compiled);

    public static bool TryParse(string s, out double v)
    {
        v = 0;
        var m = Num.Match((s ?? "").Trim().Replace(',', '.'));
        if (!m.Success) return false;
        if (!double.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out v)) return false;
        v *= m.Groups[2].Value switch
        {
            "T" => 1e12, "G" => 1e9, "M" or "meg" => 1e6, "k" or "K" => 1e3, "m" => 1e-3,
            "u" or "µ" => 1e-6, "n" => 1e-9, "p" => 1e-12, "f" => 1e-15, _ => 1,
        };
        return true;
    }

    public static double Parse(string s, double fallback = 0) => TryParse(s, out var v) ? v : fallback;

    /// <summary>"4,70 kΩ" (English: "4.70 kΩ") with <paramref name="sig"/> significant digits, decimal comma.</summary>
    public static string Format(double v, string unit, int sig = 3)
    {
        if (double.IsNaN(v) || double.IsInfinity(v)) return "—";
        if (Math.Abs(v) < 1e-12) return "0 " + unit; // solver noise (fV, fA) reads as zero
        string[] pre = { "f", "p", "n", "µ", "m", "", "k", "M", "G", "T" };
        int exp = (int)Math.Floor(Math.Log10(Math.Abs(v)) / 3);
        exp = Math.Clamp(exp, -5, 4);
        double m = v / Math.Pow(1000, exp);
        int Digits(double x) => Math.Max(0, sig - 1 - (int)Math.Floor(Math.Log10(Math.Abs(x))));
        m = Math.Round(m, Digits(m));
        if (Math.Abs(m) >= 1000 && exp < 4) { exp++; m /= 1000; } // 999.7 → 1,00 k
        return m.ToString("F" + Digits(m), Culture) + " " + pre[exp + 5] + unit;
    }

    /// <summary>Plain number with the language's decimal separator ("0,83" / "0.83").</summary>
    public static string Plain(double v, int decimals = 2) =>
        double.IsNaN(v) ? "—" : v.ToString("F" + decimals, Culture);

    /// <summary>E24 series (the resistor values sold in shops).</summary>
    public static readonly double[] E24 = { 1.0, 1.1, 1.2, 1.3, 1.5, 1.6, 1.8, 2.0, 2.2, 2.4, 2.7, 3.0, 3.3, 3.6, 3.9, 4.3, 4.7, 5.1, 5.6, 6.2, 6.8, 7.5, 8.2, 9.1 };

    /// <summary>Nearest E24 value (log distance).</summary>
    public static double SnapE24(double v)
    {
        if (v <= 0) return v;
        double dec = Math.Pow(10, Math.Floor(Math.Log10(v)));
        double best = v, bestD = double.MaxValue;
        foreach (var e in E24.Append(10.0))
        {
            double c = e * dec, d = Math.Abs(Math.Log(c / v));
            if (d < bestD) { bestD = d; best = c; }
        }
        return Round3(best);
    }

    /// <summary>The next (dir = +1) or previous (−1) E24 value.</summary>
    public static double StepE24(double v, int dir)
    {
        v = SnapE24(v);
        if (v <= 0) return v;
        double dec = Math.Pow(10, Math.Floor(Math.Log10(v) + 1e-9));
        double m = v / dec;
        int i = Array.FindIndex(E24, e => Math.Abs(e - m) < 0.05);
        if (i < 0) i = 0;
        i += dir;
        if (i >= E24.Length) { i = 0; dec *= 10; }
        if (i < 0) { i = E24.Length - 1; dec /= 10; }
        return Round3(E24[i] * dec);
    }

    /// <summary>Rounds to 3 significant digits (removes 4.7000000001e-6 noise).</summary>
    public static double Round3(double v)
    {
        if (v == 0 || double.IsNaN(v)) return v;
        double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(v))) - 2);
        return Math.Round(v / scale) * scale;
    }
}
