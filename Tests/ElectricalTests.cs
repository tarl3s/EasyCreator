using System.IO;
using EasyCreator.Electrical;
using Complex = System.Numerics.Complex;

namespace EasyCreator;

/// <summary>Unit tests of the circuit simulator (Labs/Electrical): SI values, DC (divider, Wheatstone, sources, LED,
/// switch), errors, transient (RC/RL time constants, rectifier ripple), AC (RLC resonance, power factor, three-phase).
/// Runs on the UI thread (the lab is a WPF control).</summary>
public static partial class ElectricalTests
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
            Check("format 4,70 kΩ", SI.Format(4700, "Ω").Replace('.', ',') == "4,70 kΩ", SI.Format(4700, "Ω"));
            Check("format 20,0 mA", SI.Format(0.02, "A").Replace('.', ',') == "20,0 mA", SI.Format(0.02, "A"));
            Check("format 999.7 → 1,00 kV", SI.Format(999.7, "V").Replace('.', ',') == "1,00 kV", SI.Format(999.7, "V"));
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
            s = Dc("V1 n1 0 9; R1 n1 n2 350; D1 n2 0 led", out n);
            Check("lesson LED: 350 Ω from 9 V gives about 20 mA", Math.Abs(n.Find("D1")!.Id - 0.02) < 0.002, $"(I = {n.Find("D1")!.Id:0.#####})");
            s = Dc("I1 n1 0 3m; R1 n1 0 1k; R2 n1 0 2k", out n);
            Near("current source into 1k ∥ 2k = 2 V", s.V("n1"), 2);

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
            Check("parallel voltage sources → friendly error", !bad.SolveDc() && bad.Error.Length > 10, bad.Error);
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

            TextbookCases((a, b, c) => Check(a, b, c), (a, b, c, d) => Near(a, b, c, d), p => Lab(p), log);
            AdvancedLessons((a, b, c) => Check(a, b, c), (a, b, c, d) => Near(a, b, c, d), p => Lab(p), log);
        }
        catch (Exception ex) { fails++; log.WriteLine("CRASH " + ex); }
        log.WriteLine($"== Electrical: {passes} passed, {fails} failed");
        return fails;
    }

    /// <summary>Textbook reference cases: transient curves against the analytic solution (RC charge and discharge, RL,
    /// underdamped RLC overshoot and peak time, LC amplitude), integrator stability on stiff circuits, transient vs phasor
    /// agreement and RMS, reactive-power signs and the power triangle, power-factor correction by the formula,
    /// unbalanced three-phase loads with and without neutral (Millman), the rectifier ripple estimate, Thévenin/Norton.</summary>
    static void TextbookCases(Action<string, bool, string> check, Action<string, double, double, double> near,
        Func<Dictionary<string, string>, CircuitLab> Lab, TextWriter log)
    {
        void Check(string name, bool ok, string detail = "") => check(name, ok, detail);
        void Near(string name, double got, double want, double tolPct = 1) => near(name, got, want, tolPct);
        double M(ILab l, string k) => l.Metrics.TryGetValue(k, out var v) ? v : double.NaN;
        // Runs a transient and samples one probe at every step.
        (List<double> t, List<double> y) Tran(string netlist, double tstop, int steps, Func<Netlist, double> probe)
        {
            var n = Netlist.Parse(netlist);
            var s = new CircuitSim(n);
            var ts = new List<double>(); var ys = new List<double>();
            if (!s.RunTransient(tstop, steps, t => { ts.Add(t); ys.Add(probe(n)); })) log.WriteLine("    (transient failed: " + s.Error + ")");
            return (ts, ys);
        }
        double At((List<double> t, List<double> y) r, double time)
        {
            int i = r.t.FindIndex(x => x >= time - 1e-12);
            if (i <= 0) return i == 0 ? r.y[0] : double.NaN;
            double f = (time - r.t[i - 1]) / (r.t[i] - r.t[i - 1]);
            return r.y[i - 1] + f * (r.y[i] - r.y[i - 1]);
        }
        string Inv(double v) => v.ToString("R", System.Globalization.CultureInfo.InvariantCulture);

        log.WriteLine("== Textbook: transient against the analytic solution");
        // RC charge: vC = V(1 − e^(−t/τ)), τ = 1 kΩ · 100 µF = 0.1 s.
        var rc = Tran("V1 a 0 5; R1 a b 1k; C1 b 0 100u", 0.5, 2000, n => n.Find("C1")!.Vd);
        foreach (var k in new[] { 0.5, 1, 2, 3 })
            Near($"RC charge vC({k}τ) = 5(1 − e^−{k})", At(rc, 0.1 * k), 5 * (1 - Math.Exp(-k)), 0.3);
        var rci = Tran("V1 a 0 5; R1 a b 1k; C1 b 0 100u", 0.5, 2000, n => n.Find("C1")!.Id);
        Near("RC charge current after 1τ = 5 mA · e^−1", At(rci, 0.1), 0.005 * Math.Exp(-1), 0.5);
        // RC discharge: a 0..5 V square wave at 1 Hz goes low at 0.5 s; vC(0.5 s + τ) = vC(0.5 s)·e^−1.
        var dis = Tran("V1 a 0 SQ 5 1; R1 a b 1k; C1 b 0 100u", 1, 4000, n => n.Find("C1")!.Vd);
        double v05 = At(dis, 0.5);
        Near("RC discharge: vC(T/2) ≈ 5(1 − e^−5)", v05, 5 * (1 - Math.Exp(-5)), 0.3);
        Near("RC discharge: vC(T/2 + τ) = vC(T/2)·e^−1", At(dis, 0.6), v05 * Math.Exp(-1), 0.5);
        Near("RC discharge: vC(T/2 + 3τ) = vC(T/2)·e^−3", At(dis, 0.8), v05 * Math.Exp(-3), 1);
        // RL: i = (V/R)(1 − e^(−t/τ)), τ = L/R = 1 H / 100 Ω = 10 ms.
        var rl = Tran("V1 a 0 10; R1 a b 100; L1 b 0 1", 0.05, 2000, n => n.Find("L1")!.Id);
        foreach (var k in new[] { 1, 2, 3 })
            Near($"RL current i({k}τ) = 0.1(1 − e^−{k})", At(rl, 0.01 * k), 0.1 * (1 - Math.Exp(-k)), 0.3);
        var rlv = Tran("V1 a 0 10; R1 a b 100; L1 b 0 1", 0.05, 2000, n => n.Find("L1")!.Vd);
        Near("RL inductor voltage after 1τ = 10·e^−1", At(rlv, 0.01), 10 * Math.Exp(-1), 0.5);

        // Series RLC step response (underdamped): α = R/2L, ωd = √(1/LC − α²), overshoot e^(−απ/ωd), first peak at π/ωd.
        {
            double R = 2, L = 1e-3, C = 100e-6, a = R / (2 * L), wd = Math.Sqrt(1 / (L * C) - a * a);
            var r = Tran("V1 a 0 1; R1 a b 2; L1 b c 1m; C1 c 0 100u", 4e-3, 8000, n => n.Find("C1")!.Vd);
            int ip = r.y.IndexOf(r.y.Max());
            Near("RLC overshoot = e^(−απ/ωd)", r.y[ip] - 1, Math.Exp(-a * Math.PI / wd), 1);
            Near("RLC first peak at t = π/ωd", r.t[ip], Math.PI / wd, 1);
            double tt = 1.234e-3;
            double exact = 1 - Math.Exp(-a * tt) * (Math.Cos(wd * tt) + a / wd * Math.Sin(wd * tt));
            Near("RLC vC(t) matches the analytic curve", At(r, tt), exact, 0.5);
        }
        // Almost lossless LC: the oscillation keeps its amplitude (the integrator must not damp it away).
        {
            var r = Tran("V1 a 0 1; R1 a b 1m; L1 b c 10m; C1 c 0 100u", 0.1, 20000, n => n.Find("C1")!.Vd);
            double early = r.y.Take(r.y.Count / 5).Max(), late = r.y.Skip(r.y.Count * 4 / 5).Max();
            Check("LC (16 periods): amplitude kept within 3 %", late > 0.97 * early && late < 1.03 * early, $"(first {early:0.###}, last {late:0.###})");
        }

        log.WriteLine("== Textbook: integrator stability on stiff circuits");
        // τ = 10 Ω · 1 µF = 10 µs, but the step is 250 µs (25 τ). The answer must settle at 5 V without ringing.
        {
            var r = Tran("V1 a 0 5; R1 a b 10; C1 b 0 1u", 1, 4000, n => n.Find("C1")!.Vd);
            Check("stiff RC (dt = 25τ): no ringing overshoot (< 0.2 %)", r.y.Max() <= 5.01, $"(max {r.y.Max():0.####} V)");
            Check("stiff RC: settled (±0.1 %) after 4 steps", r.y.Skip(4).All(v => Math.Abs(v - 5) < 0.005), $"(v[4] = {r.y[4]:0.####})");
            var sq = Tran("V1 a 0 SQ 5 10; R1 a b 10; C1 b 0 1u", 0.2, 800, n => n.Find("C1")!.Vd);
            Check("stiff RC with a square wave: stays within 0..5 V (±0.2 %)", sq.y.All(v => v > -0.01 && v < 5.01), $"(min {sq.y.Min():0.###}, max {sq.y.Max():0.###})");
            var sl = Tran("V1 a 0 SQ 1 10; R1 a b 100; L1 b 0 1m", 0.2, 800, n => n.Find("L1")!.Id);
            Check("stiff RL with a square wave: current stays within 0..10 mA (±0.2 %)", sl.y.All(v => v > -2e-5 && v < 0.01002), $"(min {sl.y.Min():0.#####}, max {sl.y.Max():0.#####})");
        }

        log.WriteLine("== Textbook: transient sine vs phasors, RMS");
        {
            // 10 V peak at 1 kHz on R = 1k, C = 159.15 nF (fc = 1 kHz): output peak 10/√2, lagging 45°.
            var r = Tran("V1 a 0 SIN 10 1k; R1 a b 1k; C1 b 0 159.15n", 10e-3, 10000, n => n.Find("C1")!.Vd);
            var last = Enumerable.Range(0, r.t.Count).Where(i => r.t[i] >= 9e-3 - 1e-12).ToList();
            double pk = last.Max(i => r.y[i]);
            Near("transient RC at fc: output peak = 10/√2 (same as the phasor)", pk, 10 / Math.Sqrt(2), 0.5);
            double rms = Math.Sqrt(last.Skip(1).Average(i => r.y[i] * r.y[i]));
            Near("transient RC at fc: output RMS = peak/√2 = 5 V", rms, 5, 0.5);
            int ipk = last.First(i => r.y[i] == pk);
            // The input peaks 0.25 ms into each period; 45° of lag = 0.125 ms later.
            Near("transient RC at fc: output peaks 45° (0.125 ms) after the input", r.t[ipk] - 9e-3, 0.375e-3, 2);
            var ac = Lab(new() { ["netlist"] = "V1 a 0 7.0711; R1 a b 1k; C1 b 0 159.15n", ["mode"] = "ac", ["f"] = "1k" });
            Near("AC sources are RMS: 7.07 V RMS in → 5 V RMS out", M(ac, "V(b)"), 5, 0.3);
        }

        log.WriteLine("== Textbook: AC power signs and the power triangle");
        {
            const string rlNet = "V1 a 0 100; R1 a b 30; L1 b 0 106.1m";
            var n = Netlist.Parse(rlNet);
            var s = new CircuitSim(n);
            s.SolveAc(60);
            var i = n.Find("V1")!.Iac;
            double xl = 2 * Math.PI * 60 * 0.1061, im = 100 / Math.Sqrt(900 + xl * xl);
            Near("RL: |I| = V/|Z|", i.Magnitude, im, 0.2);
            Near("RL: current lags the voltage by atan(XL/R)", i.Phase * 180 / Math.PI, -Math.Atan2(xl, 30) * 180 / Math.PI, 0.5);
            var lab = Lab(new() { ["netlist"] = rlNet, ["mode"] = "ac", ["f"] = "60" });
            Check("inductor absorbs Q > 0", M(lab, "Q(L1)") > 0, $"(Q {M(lab, "Q(L1)"):0.##})");
            Near("resistor absorbs no Q", M(lab, "Q(R1)"), 0, 1e-4);
            Near("source P = I²R", M(lab, "Ptotal"), im * im * 30, 0.2);
            Near("source Q = I²XL", M(lab, "Qtotal"), im * im * xl, 0.2);
            Near("S² = P² + Q²", M(lab, "S"), Math.Sqrt(Math.Pow(M(lab, "Ptotal"), 2) + Math.Pow(M(lab, "Qtotal"), 2)), 0.01);
            Near("pf = P/S = R/|Z| ≈ 0.6", M(lab, "pf"), 30 / Math.Sqrt(900 + xl * xl), 0.2);
            Check("inductive load: phase ≈ +53°", M(lab, "phase") > 52 && M(lab, "phase") < 54, $"(phase {M(lab, "phase"):0.##})");
            var cap = Lab(new() { ["netlist"] = "V1 a 0 100; R1 a b 30; C1 b 0 66.3u", ["mode"] = "ac", ["f"] = "60" });
            Check("capacitor absorbs Q < 0 (it supplies reactive power)", M(cap, "Q(C1)") < 0, $"(Q {M(cap, "Q(C1)"):0.##})");
            Check("capacitive load: phase ≈ −53°", M(cap, "phase") < -52 && M(cap, "phase") > -54, $"(phase {M(cap, "phase"):0.##})");
            var c10 = Lab(new() { ["netlist"] = "V1 a 0 127; C1 a 0 10u", ["mode"] = "ac", ["f"] = "60" });
            Near("ideal capacitor: P = 0", M(c10, "P(C1)"), 0, 1e-4);
            Near("ideal capacitor: I = V·ωC", M(c10, "I(C1)"), 127 * 2 * Math.PI * 60 * 10e-6, 0.1);
        }

        log.WriteLine("== Textbook: power-factor correction by the formula");
        {
            // Qc = P(tan φ1 − tan φ2), C = Qc / (ωV²) must land exactly on the target pf.
            double R = 8, X = 2 * Math.PI * 60 * 0.016, z2 = R * R + X * X, V = 220;
            double P = V * V * R / z2, Q = V * V * X / z2;
            double qc = P * (Q / P - Math.Tan(Math.Acos(0.95))), c = qc / (2 * Math.PI * 60 * V * V);
            Near("lesson motor: P ≈ 3857 W", P, 3857, 0.1);
            Near("lesson motor: Q ≈ 2908 var", Q, 2908, 0.1);
            Near("lesson motor: C for pf 0.95 ≈ 89.9 µF", c, 89.9e-6, 0.3);
            var lab = Lab(new() { ["netlist"] = $"V1 a 0 220; Rm a b 8; Lm b 0 16m; C1 a 0 {Inv(c)}", ["mode"] = "ac", ["f"] = "60" });
            Near("formula C gives pf 0.95", M(lab, "pf"), 0.95, 0.05);
            Near("formula C: line current = P/(V·0.95)", M(lab, "I(V1)"), P / (V * 0.95), 0.1);
            Near("capacitor supplies −Qc", M(lab, "Q(C1)"), -qc, 0.2);
            var over = Lab(new() { ["netlist"] = "V1 a 0 220; Rm a b 8; Lm b 0 16m; C1 a 0 250u", ["mode"] = "ac", ["f"] = "60" });
            Check("too much capacitor: load turns capacitive (phase < 0, Q < 0)", M(over, "phase") < 0 && M(over, "Qtotal") < 0, $"(phase {M(over, "phase"):0.#})");
        }

        log.WriteLine("== Textbook: three-phase Y, unbalanced");
        {
            Complex Ph(double mag, double deg) => Complex.FromPolarCoordinates(mag, deg * Math.PI / 180);
            Complex va = Ph(127, 0), vb = Ph(127, -120), vc = Ph(127, 120);
            // Unbalanced loads (phase B is R + L) with a 0.5 Ω neutral: Millman's theorem for the star point.
            double ra = 10, rb = 20, rcc = 15, rn = 0.5, w = 2 * Math.PI * 60;
            Complex za = ra, zb = new(rb, w * 0.02), zc = rcc;
            Complex vn = (va / za + vb / zb + vc / zc) / (1 / za + 1 / zb + 1 / zc + 1 / rn);
            Complex ia = (va - vn) / za, ib = (vb - vn) / zb, ic = (vc - vn) / zc;
            var lab = Lab(new() { ["netlist"] = "Va a 0 127; Vb b 0 127 ph=-120; Vc c 0 127 ph=120; Ra a n 10; Rb b x 20; Lb x n 20m; Rc c n 15; Rn n 0 0.5", ["mode"] = "ac", ["f"] = "60" });
            Near("unbalanced Y: neutral current = |Ia + Ib + Ic| (Millman)", M(lab, "I(Rn)"), (ia + ib + ic).Magnitude, 0.2);
            Near("unbalanced Y: Ia", M(lab, "I(Ra)"), ia.Magnitude, 0.2);
            Near("unbalanced Y: Ib", M(lab, "I(Rb)"), ib.Magnitude, 0.2);
            Near("unbalanced Y: star-point voltage", M(lab, "V(n)"), vn.Magnitude, 0.2);
            double pSum = ia.Magnitude * ia.Magnitude * ra + ib.Magnitude * ib.Magnitude * rb + ic.Magnitude * ic.Magnitude * rcc + Math.Pow((ia + ib + ic).Magnitude, 2) * rn;
            Near("unbalanced Y: total P = Σ I²R", M(lab, "Ptotal"), pSum, 0.2);
            // Without a neutral the star point floats and the three load voltages become unequal.
            Complex vn0 = (va / ra + vb / rb + vc / rcc) / (1 / ra + 1 / rb + 1 / rcc);
            var open = Lab(new() { ["netlist"] = "Va a 0 127; Vb b 0 127 ph=-120; Vc c 0 127 ph=120; Ra a n 10; Rb b n 20; Rc c n 15", ["mode"] = "ac", ["f"] = "60" });
            Near("no neutral: star point shifts (Millman)", M(open, "V(n)"), vn0.Magnitude, 0.2);
            Near("no neutral: phase-A load voltage = |Va − Vn|", M(open, "V(Ra)"), (va - vn0).Magnitude, 0.2);
            Near("no neutral: phase-C load voltage = |Vc − Vn|", M(open, "V(Rc)"), (vc - vn0).Magnitude, 0.2);
            var bal = Lab(new() { ["netlist"] = "Va a 0 127; Vb b 0 127 ph=-120; Vc c 0 127 ph=120; Ra a n 10; Rb b n 10; Rc c n 10; M1 a b", ["mode"] = "ac", ["f"] = "60" });
            Near("line voltage Vab = Va − Vb has angle +30°", M(bal, "ph(M1)"), 30, 0.5);
            Near("balanced Y: P = √3·VL·IL", M(bal, "Ptotal"), Math.Sqrt(3) * 127 * Math.Sqrt(3) * 12.7, 0.2);
            Near("balanced Y without neutral: star point stays at 0 V", M(bal, "V(n)"), 0, 1e-3);
        }

        log.WriteLine("== Textbook: rectifier ripple vs I/(2fC)");
        foreach (var cap in new[] { 1000e-6, 2200e-6, 4700e-6 })
        {
            var lab = Lab(new() { ["netlist"] = $"V1 s1 s2 SIN 17 60; D1 s1 p; D2 0 s1; D3 s2 p; D4 0 s2; C1 p 0 {Inv(cap)}; R1 p 0 100", ["mode"] = "tran", ["tstop"] = "0.2", ["steps"] = "8000", ["probe"] = "V(p)" });
            double est = M(lab, "Vavg") / 100 / (120 * cap), rip = M(lab, "ripple");
            // The classic estimate assumes the capacitor discharges for a whole half period, so it overestimates a little.
            Check($"C = {cap * 1e6:0} µF: ripple {rip:0.###} V within 0.7–1.05 × I/(2fC) = {est:0.###} V", rip > 0.7 * est && rip < 1.05 * est, $"(ripple {rip:0.###})");
            Near($"C = {cap * 1e6:0} µF: average ≈ peak − ripple/2", M(lab, "Vavg"), M(lab, "Vmax") - rip / 2, 1.5);
        }
        {
            var half = Lab(new() { ["netlist"] = "V1 a 0 SIN 17 60; D1 a p; C1 p 0 2200u; R1 p 0 100", ["mode"] = "tran", ["tstop"] = "0.2", ["steps"] = "8000", ["probe"] = "V(p)" });
            double est = M(half, "Vavg") / 100 / (60 * 2200e-6);
            Check($"half-wave: ripple {M(half, "ripple"):0.###} V ≈ I/(fC) = {est:0.###} V (twice the full-wave one)", M(half, "ripple") > 0.7 * est && M(half, "ripple") < 1.05 * est, $"(ripple {M(half, "ripple"):0.###})");
        }

        log.WriteLine("== Textbook: Thévenin, Norton, maximum power, Kirchhoff");
        {
            const string th = "V1 n1 0 12; R1 n1 n2 6k; R2 n2 0 3k; R3 n2 n3 1k; RL n3 0 {R}";
            double P(string r) => M(Lab(new() { ["netlist"] = th.Replace("{R}", r) }), "P(RL)");
            Near("open-circuit voltage = Vth = 4 V", M(Lab(new() { ["netlist"] = th.Replace("{R}", "1G") }), "V(n3)"), 4, 0.1);
            Near("short-circuit current = In = Vth/Rth = 1.333 mA", M(Lab(new() { ["netlist"] = th.Replace("{R}", "1m") }), "I(RL)"), 4 / 3000.0, 0.1);
            Near("Pmax = Vth²/(4 Rth) at RL = Rth", P("3k"), 16 / 12000.0, 0.1);
            Check("RL = Rth beats 2.7k and 3.3k", P("3k") > P("2.7k") && P("3k") > P("3.3k"));
            var norton = Lab(new() { ["netlist"] = "I1 a 0 1.3333333m; Rn a 0 3k; RL a 0 3k" });
            Near("Norton equivalent gives the same load current", M(norton, "I(RL)"), 4 / 6000.0, 0.1);
            var k3 = Lab(new() { ["netlist"] = "V1 n1 0 10; R1 n1 n2 1k; R3 n2 0 1k; R2 n3 n2 1k; V2 n3 0 8" });
            Near("KCL at n2: I(R1) + I(R2) = I(R3)", M(k3, "I(R1)") + M(k3, "I(R2)"), M(k3, "I(R3)"), 0.01);
            Near("energy balance: Σ P(sources) = Σ P(resistors)", M(k3, "Ptotal"), M(k3, "P(R1)") + M(k3, "P(R2)") + M(k3, "P(R3)"), 0.01);
        }

        log.WriteLine("== τ metric");
        {
            var cut = Lab(new() { ["netlist"] = "V1 n1 0 5; R1 n1 n2 4.7k; C1 n2 0 100u", ["mode"] = "tran", ["tstop"] = "1", ["probe"] = "V(n2)" });
            Near("RC cut before settling (τ = 0.47 s, 1 s run): τ from the exponential fit", M(cut, "tau"), 0.47, 1);
            var slow = Lab(new() { ["netlist"] = "V1 n1 0 5; R1 n1 n2 100k; C1 n2 0 100u", ["mode"] = "tran", ["tstop"] = "1", ["probe"] = "V(n2)" });
            Near("RC ten times slower than the run (τ = 10 s): still τ = RC", M(slow, "tau"), 10, 2);
            var cur = Lab(new() { ["netlist"] = "V1 n1 0 5; R1 n1 n2 4.7k; C1 n2 0 100u", ["mode"] = "tran", ["tstop"] = "1", ["probe"] = "I(C1)" });
            Near("decaying current (unsettled): τ = RC", M(cur, "tau"), 0.47, 1);
            var osc = Lab(new() { ["netlist"] = "V1 a 0 1; R1 a b 1m; L1 b c 10m; C1 c 0 100u", ["mode"] = "tran", ["tstop"] = "0.02", ["probe"] = "V(C1)" });
            Check("an oscillation has no τ", !osc.Metrics.ContainsKey("tau"), $"(tau {M(osc, "tau"):0.####})");
        }

        log.WriteLine("== Lessons: hints and answers agree with the simulator");
        var track = ContentStore.Find("electrical");
        if (track == null) { Check("electrical track loaded", false); return; }
        void Lesson(string id, string key, string value, bool meets)
        {
            var lesson = track.Lessons.FirstOrDefault(l => l.Id == id);
            if (lesson == null) { Check($"{id} exists", false); return; }
            var lab = new CircuitLab();
            lab.Load(lesson.LabParams);
            lab.SetInputs(new Dictionary<string, string> { [key] = value });
            lab.RunToEnd();
            var missed = lesson.Goals.Where(g => !g.IsMet(lab.Metrics)).Select(g => $"{g.Metric}={M(lab, g.Metric):0.####}").ToList();
            Check($"{id}: {key} = {value} {(meets ? "meets every goal" : "misses a goal")}", (missed.Count == 0) == meets,
                missed.Count == 0 ? "(all met)" : "(missed " + string.Join(", ", missed) + ")");
        }
        Lesson("ele-01-ohm", "R1", "430", true);
        Lesson("ele-01-ohm", "R1", "470", true);
        Lesson("ele-01-ohm", "R1", "390", false);
        Lesson("ele-01-ohm", "R1", "510", false);
        Lesson("ele-02-serie-paralelo", "R3", "200", true);
        Lesson("ele-02-serie-paralelo", "R3", "220", false);
        Lesson("ele-03-kirchhoff", "V2", "8", true);
        Lesson("ele-05-thevenin", "RL", "3k", true);
        Lesson("ele-06-rc", "R1", "1k", true);
        Lesson("ele-09-retificador", "C1", "2.2m", true);
        Lesson("ele-10-trifasico", "Rc", "10", true);
        Lesson("ele-03-kirchhoff", "V2", "7.5", false);
        Lesson("ele-05-thevenin", "RL", "2k", false);
        Lesson("ele-06-rc", "R1", "910", false);
        Lesson("ele-06-rc", "R1", "1.1k", false);
        Lesson("ele-07-fasores", "f", "1592", true);
        Lesson("ele-07-fasores", "f", "1500", false);
        Lesson("ele-08-fator-potencia", "C1", "90u", true);
        Lesson("ele-08-fator-potencia", "C1", "150u", true);
        Lesson("ele-08-fator-potencia", "C1", "85u", false);
        Lesson("ele-08-fator-potencia", "C1", "165u", false);
        Lesson("ele-09-retificador", "C1", "1.5m", true);
        Lesson("ele-09-retificador", "C1", "1m", false);
        Lesson("ele-10-trifasico", "Rc", "11", false);
        {
            var l6 = track.Lessons.First(l => l.Id == "ele-06-rc");
            var lab = new CircuitLab();
            lab.Load(l6.LabParams);
            lab.RunToEnd();
            Near("ele-06 default R1 = 4.7k: the lab shows τ = RC = 0.47 s", M(lab, "tau"), 0.47, 1);
        }
    }
}
