using System.IO;
using EasyCreator.Electrical;
using Complex = System.Numerics.Complex;

namespace EasyCreator;

/// <summary>Unit tests of the circuit simulator (Labs/Electrical): SI values, DC (divider, Wheatstone, sources, LED,
/// switch), errors, transient (RC/RL time constants, rectifier ripple), AC (RLC resonance, power factor, three-phase).
/// Runs on the UI thread (the lab is a WPF control).</summary>
public static class ElectricalTests
{
    public static int Run(TextWriter log)
    {
        int fails = 0, passes = 0;
        void Check(string name, bool ok, string detail = "")
        {
            if (ok) { passes++; log.WriteLine("  ok   " + name); }
            else { fails++; log.WriteLine($"  FAIL {name} {detail}"); }
        }
        void Near(string name, double got, double want, double tolPct = 1)
        {
            bool ok = !double.IsNaN(got) && (want == 0 ? Math.Abs(got) < tolPct / 100 : Math.Abs(got - want) <= Math.Abs(want) * tolPct / 100);
            Check(name, ok, $"(got {got:G6}, want {want:G6})");
        }
        CircuitSim Dc(string netlist, out Netlist n)
        {
            n = Netlist.Parse(netlist);
            var s = new CircuitSim(n);
            if (!s.SolveDc()) log.WriteLine("    (solve failed: " + s.Error + ")");
            return s;
        }
        CircuitLab Lab(Dictionary<string, string> p) { var l = new CircuitLab(); l.Load(p); l.RunToEnd(); return l; }
        double M(ILab l, string k) => l.Metrics.TryGetValue(k, out var v) ? v : double.NaN;

        try
        {
            log.WriteLine("== SI values");
            Near("parse 4.7k", SI.Parse("4.7k"), 4700, 0.001);
            Near("parse 100u", SI.Parse("100u"), 1e-4, 0.001);
            Near("parse 2,2mF", SI.Parse("2,2mF"), 2.2e-3, 0.001);
            Near("parse 1M", SI.Parse("1M"), 1e6, 0.001);
            Near("parse 10m", SI.Parse("10m"), 0.01, 0.001);
            Near("parse 60Hz", SI.Parse("60Hz"), 60, 0.001);
            Near("parse 1F (unit, not femto)", SI.Parse("1F"), 1, 0.001);
            Check("format 4,70 kΩ", SI.Format(4700, "Ω") == "4,70 kΩ", SI.Format(4700, "Ω"));
            Check("format 20,0 mA", SI.Format(0.02, "A") == "20,0 mA", SI.Format(0.02, "A"));
            Check("format 999.7 → 1,00 kV", SI.Format(999.7, "V") == "1,00 kV", SI.Format(999.7, "V"));
            Near("E24 snap 450 → 470", SI.SnapE24(450), 470, 0.001);
            Near("E24 step 1k → 1.1k", SI.StepE24(1000, 1), 1100, 0.001);
            Near("E24 step 1k ← 910", SI.StepE24(1000, -1), 910, 0.001);

            log.WriteLine("== DC");
            var s = Dc("V1 n1 0 12; R1 n1 n2 1k; R2 n2 0 2k", out var n);
            Near("divider V(n2) = 8 V", s.V("n2"), 8);
            Near("divider I(V1) = 4 mA (delivered)", n.Find("V1")!.Id, 0.004);
            Near("divider P(R2) = 32 mW", n.Find("R2")!.Vd * n.Find("R2")!.Id, 0.032);

            s = Dc("V1 t 0 10; R1 t a 1k; R2 a 0 2k; R3 t b 1.5k; R4 b 0 3k; R5 a b 500", out n);
            Near("Wheatstone balanced: I(R5) = 0", n.Find("R5")!.Id, 0, 1e-4);
            s = Dc("V1 t 0 10; R1 t a 1k; R2 a 0 2k; R3 t b 1.5k; R4 b 0 1k; R5 a b 500", out n);
            Near("Wheatstone unbalanced: I(R5) = 1.509 mA (Thevenin)", n.Find("R5")!.Id, 2.6667 / 1766.67, 0.1);

            s = Dc("I1 n1 0 2m; R1 n1 0 1k", out n);
            Near("current source 2 mA into 1k = 2 V", s.V("n1"), 2);
            Near("current source delivers 4 mW", n.Find("I1")!.Vd * n.Find("I1")!.Id, 0.004);

            s = Dc("V1 n1 0 10; R1 n1 n2 1k; V2 n3 0 5; R2 n3 n2 1k; R3 n2 0 1k", out n);
            Near("two sources (superposition) V(n2) = 5 V", s.V("n2"), 5);

            s = Dc("V1 n1 0 5; R1 n1 n2 150; D1 n2 0 led", out n);
            Near("LED current (5-2)/(150+10) = 18.75 mA", n.Find("D1")!.Id, 0.01875);
            s = Dc("V1 n1 0 5; R1 n1 n2 150; D1 0 n2 led", out n);
            Check("reversed LED does not conduct", Math.Abs(n.Find("D1")!.Id) < 1e-6, $"(I = {n.Find("D1")!.Id})");
            s = Dc("V1 n1 0 5; R1 n1 n2 1k; D1 n2 n3; D2 n3 0", out n);
            Near("two silicon diodes: I = (5-1.4)/1k", n.Find("R1")!.Id, 0.0036, 2);

            s = Dc("V1 n1 0 5; S1 n1 n2 off; R1 n2 0 1k", out n);
            Check("open switch: no current", Math.Abs(n.Find("R1")!.Id) < 1e-6);
            s = Dc("V1 n1 0 5; S1 n1 n2 on; R1 n2 0 1k", out n);
            Near("closed switch: 5 mA", n.Find("R1")!.Id, 0.005);

            s = Dc("V1 n1 0 9; A1 n1 n2; R1 n2 0 450; M1 n2 0", out n);
            Near("ammeter reads 20 mA", n.Find("A1")!.Id, 0.02);
            Near("voltmeter reads 9 V", n.Find("M1")!.Vd, 9);

            s = Dc("V1 a 0 6; W a b; R1 b 0 3", out n);
            Near("wire joins nodes: I = 2 A", n.Find("R1")!.Id, 2);
            s = Dc("V1 a 0 6; W a b; W b c; R1 c 0 3", out n);
            Check("several wires may all be called W", n.Errors.Count == 0 && Math.Abs(n.Find("R1")!.Id - 2) < 1e-6, string.Join("; ", n.Errors));
            Near("DC: inductor is a short", Dc("V1 a 0 6; L1 a b 1; R1 b 0 3", out n).V("b"), 6);
            Near("DC: capacitor is open", Dc("V1 a 0 6; R1 a b 1k; C1 b 0 1u", out n).V("b"), 6);

            log.WriteLine("== Errors never throw");
            var bad = new CircuitSim(Netlist.Parse("V1 n1 0 5; V2 n1 0 3"));
            Check("parallel voltage sources → friendly error", !bad.SolveDc() && bad.Error.Contains("solução"), bad.Error);
            bad = new CircuitSim(Netlist.Parse("V1 n1 0 5; W n1 0"));
            Check("shorted source → friendly error", !bad.SolveDc() && bad.Error.Length > 0, bad.Error);
            var badLab = Lab(new() { ["netlist"] = "X1 a b 3; R1 a" });
            Check("bad netlist: lab shows an error, no metrics", badLab.Metrics.Count == 0);
            var floating = Dc("V1 a 0 5; R1 a b 1k; R2 c d 1k", out n);
            Check("floating part still solves", Math.Abs(floating.V("c")) < 1e-6);

            log.WriteLine("== Transient");
            var rc = Lab(new() { ["netlist"] = "V1 n1 0 5; R1 n1 n2 1k; C1 n2 0 100u", ["mode"] = "tran", ["tstop"] = "0.6", ["scope"] = "V(n2)" });
            Near("RC tau = RC = 0.1 s", M(rc, "tau"), 0.1, 1);
            Near("RC after 6τ ≈ 4.988 V", M(rc, "V(n2)"), 5 * (1 - Math.Exp(-6)), 0.5);
            var rl = Lab(new() { ["netlist"] = "V1 n1 0 10; R1 n1 n2 100; L1 n2 0 1", ["mode"] = "tran", ["tstop"] = "60m", ["scope"] = "I(L1)" });
            Near("RL tau = L/R = 10 ms", M(rl, "tau"), 0.01, 1);
            Near("RL final current 100 mA", M(rl, "Vfinal"), 0.1, 0.5);
            var lc = Lab(new() { ["netlist"] = "V1 a 0 SQ 1 0.01; R1 a b 0.02; L1 b c 1m; C1 c 0 1m", ["mode"] = "tran", ["tstop"] = "40m", ["steps"] = "8000", ["scope"] = "V(C1)" });
            Check("underdamped RLC overshoots (trapezoidal keeps the oscillation)", M(lc, "Vmax") > 1.5, $"(Vmax {M(lc, "Vmax"):0.###})");

            string bridge = "V1 s1 s2 SIN 17 60; D1 s1 p; D2 0 s1; D3 s2 p; D4 0 s2; C1 p 0 {C}; R1 p 0 100";
            var big = Lab(new() { ["netlist"] = bridge.Replace("{C}", "2200u"), ["mode"] = "tran", ["tstop"] = "0.1", ["probe"] = "V(p)" });
            Near("bridge rectifier peak ≈ 17 − 2·0.7", M(big, "Vmax"), 15.6, 2);
            Check("2200 µF: ripple < 1 V", M(big, "ripple") < 1, $"(ripple {M(big, "ripple"):0.###})");
            Near("ripple ≈ I/(2fC)", M(big, "ripple"), 15.2 / 100 / (120 * 2200e-6), 20);
            var small = Lab(new() { ["netlist"] = bridge.Replace("{C}", "100u"), ["mode"] = "tran", ["tstop"] = "0.1", ["probe"] = "V(p)" });
            Check("100 µF: ripple > 5 V", M(small, "ripple") > 5, $"(ripple {M(small, "ripple"):0.###})");
            var half = Lab(new() { ["netlist"] = "V1 a 0 SIN 10 50; D1 a b; R1 b 0 1k", ["mode"] = "tran", ["tstop"] = "40m", ["probe"] = "V(b)" });
            Check("half-wave: output never negative", M(half, "Vmin") > -0.01, $"(Vmin {M(half, "Vmin")})");

            log.WriteLine("== AC");
            string rlc = "V1 n1 0 10; R1 n1 n2 10; L1 n2 n3 10m; C1 n3 0 1u";
            double f0 = 1 / (2 * Math.PI * Math.Sqrt(10e-3 * 1e-6));
            var res = Lab(new() { ["netlist"] = rlc, ["mode"] = "ac", ["f"] = f0.ToString(System.Globalization.CultureInfo.InvariantCulture) });
            Near("RLC at resonance: I = V/R = 1 A", M(res, "I(V1)"), 1, 0.5);
            Near("RLC at resonance: phase 0°", M(res, "phase"), 0, 10);
            Near("RLC at resonance: pf = 1", M(res, "pf"), 1, 0.1);
            Near("RLC at resonance: V(L1) = Q·V = 100 V", M(res, "V(L1)"), 10 * 2 * Math.PI * f0 * 10e-3 / 10, 0.5);
            var below = Lab(new() { ["netlist"] = rlc, ["mode"] = "ac", ["f"] = "1k" });
            Check("RLC below resonance is capacitive (phase < 0)", M(below, "phase") < -30, $"(phase {M(below, "phase"):0.#})");
            var lowpass = Lab(new() { ["netlist"] = "V1 a 0 1; R1 a b 1k; C1 b 0 159.15n", ["mode"] = "ac", ["f"] = "1k" });
            Near("RC low-pass at fc: gain 0.707", M(lowpass, "V(b)"), Math.Sqrt(0.5), 0.5);
            Near("RC low-pass at fc: −45°", M(lowpass, "ph(b)"), -45, 1);

            var motor = "V1 n1 0 220; Rm n1 n2 8; Lm n2 0 16m; C1 n1 0 {C}";
            var pf0 = Lab(new() { ["netlist"] = motor.Replace("{C}", "0"), ["mode"] = "ac", ["f"] = "60" });
            double xl = 2 * Math.PI * 60 * 0.016;
            Near("motor pf = R/|Z|", M(pf0, "pf"), 8 / Math.Sqrt(64 + xl * xl), 0.2);
            Near("motor P = V²R/|Z|²", M(pf0, "Ptotal"), 220 * 220 * 8 / (64 + xl * xl), 0.2);
            var pf1 = Lab(new() { ["netlist"] = motor.Replace("{C}", "120u"), ["mode"] = "ac", ["f"] = "60" });
            Check("120 µF corrects pf above 0.95", M(pf1, "pf") > 0.95, $"(pf {M(pf1, "pf"):0.###})");
            Near("capacitor does not change P", M(pf1, "Ptotal"), M(pf0, "Ptotal"), 0.1);
            Check("corrected line current is lower", M(pf1, "I(V1)") < M(pf0, "I(V1)"));

            string tri = "Va a 0 127; Vb b 0 127 ph=-120; Vc c 0 127 ph=120; Ra a n 10; Rb b n 10; Rc c n {R}; Rn n 0 0.5; M1 a b";
            var bal = Lab(new() { ["netlist"] = tri.Replace("{R}", "10"), ["mode"] = "ac", ["f"] = "60" });
            Near("balanced three-phase: no neutral current", M(bal, "I(Rn)"), 0, 1e-4);
            Near("line voltage = √3 · 127 V", M(bal, "V(M1)"), 127 * Math.Sqrt(3), 0.2);
            Near("three-phase P = 3·V²/R", M(bal, "Ptotal"), 3 * 127 * 127 / 10.0, 0.2);
            var unb = Lab(new() { ["netlist"] = tri.Replace("{R}", "30"), ["mode"] = "ac", ["f"] = "60" });
            Check("unbalanced: neutral current flows", M(unb, "I(Rn)") > 3, $"(I {M(unb, "I(Rn)"):0.##})");

            log.WriteLine("== Lab controls");
            var lab = Lab(new() { ["netlist"] = "V1 n1 0 9; R1 n1 0 1k", ["adjust"] = "R1 100..10k" });
            Near("default I(R1) = 9 mA", M(lab, "I(R1)"), 0.009);
            lab.SetInputs(new Dictionary<string, string> { ["R1"] = "450" });
            lab.RunToEnd();
            Near("SetInputs snaps to E24 (450 → 470 Ω)", M(lab, "I(R1)"), 9 / 470.0, 0.1);
            lab.SetInputs(new Dictionary<string, string> { ["R1"] = "1M" });
            lab.RunToEnd();
            Near("SetInputs clamps to the range (10 kΩ)", M(lab, "I(R1)"), 9 / 10000.0, 0.1);
            var sw = Lab(new() { ["netlist"] = "V1 a 0 5; S1 a b off; R1 b 0 1k", ["adjust"] = "S1" });
            sw.SetInputs(new Dictionary<string, string> { ["S1"] = "on" });
            sw.RunToEnd();
            Near("switch input on → 5 mA", M(sw, "I(R1)"), 0.005);
            var ch = Lab(new() { ["netlist"] = "V1 a 0 127; Rch a 0 2.93", ["adjust"] = "Rch 2.93=Inverno|4.61=Verão" });
            ch.SetInputs(new Dictionary<string, string> { ["Rch"] = "4.61" });
            ch.RunToEnd();
            Near("choice input → 3.5 kW", M(ch, "P(Rch)"), 127 * 127 / 4.61, 0.1);
            var fr = Lab(new() { ["netlist"] = rlc, ["mode"] = "ac", ["f"] = "100", ["adjust"] = "f 100..10k", ["sweep"] = "I(V1)" });
            fr.SetInputs(new Dictionary<string, string> { ["f"] = "1590" });
            fr.RunToEnd();
            Check("frequency input + sweep: near resonance", Math.Abs(M(fr, "phase")) < 3, $"(phase {M(fr, "phase"):0.##})");
        }
        catch (Exception ex) { fails++; log.WriteLine("CRASH " + ex); }
        log.WriteLine($"== Electrical: {passes} passed, {fails} failed");
        return fails;
    }
}
