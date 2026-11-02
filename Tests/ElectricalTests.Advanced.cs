using System.IO;
using EasyCreator.Electrical;
using Complex = System.Numerics.Complex;

namespace EasyCreator;

/// <summary>Lessons 11–20 of the electrical track: every number a lab goal, hint or quiz answer quotes is recomputed here
/// from the formula (or the simulator), and every lab is played with its solution and with the neighbouring values a
/// student would try (which must miss a goal).</summary>
public static partial class ElectricalTests
{
    static void AdvancedLessons(Action<string, bool, string> check, Action<string, double, double, double> near,
        Func<Dictionary<string, string>, CircuitLab> Lab, TextWriter log)
    {
        void Check(string name, bool ok, string detail = "") => check(name, ok, detail);
        void Near(string name, double got, double want, double tolPct = 1) => near(name, got, want, tolPct);
        double M(ILab l, string k) => l.Metrics.TryGetValue(k, out var v) ? v : double.NaN;

        log.WriteLine("== Lessons 11–20: track shape");
        var track = ContentStore.Find("electrical");
        if (track == null) { Check("electrical track loaded", false); return; }
        LessonChecks.Shape(track, check);
        foreach (var id in new[] { "ele-12-rl", "ele-16-transformador", "ele-18-motores", "ele-19-condutores", "ele-20-protecao" })
            Check($"{id}: has a safety note", track.Lessons.FirstOrDefault(l => l.Id == id)?.Safety.Length > 0);

        Lesson? Find(string id) => track.Lessons.FirstOrDefault(l => l.Id == id);
        double Ans(string id, int q) => Question.TryNum(Find(id)?.Questions[q].Answer ?? "", out var v) ? v : double.NaN;
        CircuitLab Play(string id, string key, string value)
        {
            var lab = new CircuitLab();
            lab.Load(Find(id)?.LabParams ?? throw new InvalidOperationException($"lesson {id} not found"));
            lab.SetInputs(new Dictionary<string, string> { [key] = value });
            lab.RunToEnd();
            return lab;
        }
        void Try(string id, string key, string value, bool meets)
        {
            var lesson = Find(id);
            if (lesson == null) { Check($"{id} exists", false); return; }
            var lab = Play(id, key, value);
            var missed = lesson.Goals.Where(g => !g.IsMet(lab.Metrics)).Select(g => $"{g.Metric}={M(lab, g.Metric):0.####}").ToList();
            Check($"{id}: {key} = {value} {(meets ? "meets every goal" : "misses a goal")}", (missed.Count == 0) == meets,
                missed.Count == 0 ? "(all met)" : "(missed " + string.Join(", ", missed) + ")");
        }
        LessonChecks.TenPercentOffRejected(track, 10, check);

        log.WriteLine("== ele-11 superposition");
        {
            Near("q1: V1 alone → 12·2k/3k = 8 V", Ans("ele-11-superposicao", 0), M(Lab(new() { ["netlist"] = "V1 n1 0 12; R1 n1 n2 1k; R2 n2 0 2k" }), "V(n2)"), 0.01);
            Near("q2: I1 = 6 mA alone → 6m·(1k∥2k) = 4 V", Ans("ele-11-superposicao", 1), M(Lab(new() { ["netlist"] = "R1 n2 0 1k; R2 n2 0 2k; I1 n2 0 6m" }), "V(n2)"), 0.01);
            var both = Play("ele-11-superposicao", "I1", "6m");
            Near("superposition: V(n2) = 8 + 4 = 12 V with both sources", M(both, "V(n2)"), Ans("ele-11-superposicao", 0) + Ans("ele-11-superposicao", 1), 0.01);
            Near("nodal formula of the theory: V = 8 + 666.7·I1 (I1 = 3 mA)", M(Play("ele-11-superposicao", "I1", "3m"), "V(n2)"), 8 + 3e-3 * 2000.0 / 3, 0.01);
            var mesh = Lab(new() { ["netlist"] = "V1 a 0 10; R1 a b 2; R3 b 0 4; R2 b 0 2" });
            Near("q4: mesh example, current in the shared 4 Ω = 1 A", Ans("ele-11-superposicao", 3), M(mesh, "I(R3)"), 0.01);
            Near("mesh example: Ia = 3 A", M(mesh, "I(V1)"), 3, 0.01);
            Near("mesh example: Ib = 2 A", M(mesh, "I(R2)"), 2, 0.01);
            Try("ele-11-superposicao", "I1", "6m", true);
            Try("ele-11-superposicao", "I1", "5.5m", false);
            Try("ele-11-superposicao", "I1", "6.5m", false);
        }

        log.WriteLine("== ele-12 RL transient");
        {
            Near("q1: τ = 0.2 H / 40 Ω = 5 ms", Ans("ele-12-rl", 0), 0.2 / 40 * 1000, 0.01);
            Near("q2: E = ½·0.5·0.06² = 0.9 mJ", Ans("ele-12-rl", 1), 0.5 * 0.5 * 0.06 * 0.06 * 1000, 0.01);
            Near("q3: 1 − e^−3 = 95 %", Ans("ele-12-rl", 2), 100 * (1 - Math.Exp(-3)), 0.5);
            var sol = Play("ele-12-rl", "R1", "150");
            Near("R1 = 150: lab τ = 0.5 / (150 + 50) = 2.5 ms", M(sol, "tau"), 0.5 / 200, 1);
            Near("R1 = 150: final current 12 / 200 = 60 mA", M(sol, "I(L1)"), 0.06, 0.5);
            Near("default R1 = 1k: τ = 0.5 / 1050", M(Play("ele-12-rl", "R1", "1k"), "tau"), 0.5 / 1050, 2);
            Try("ele-12-rl", "R1", "150", true);
            Try("ele-12-rl", "R1", "160", false);
            Try("ele-12-rl", "R1", "130", false);
        }

        log.WriteLine("== ele-13 LEDs and Zener");
        {
            Near("q1: (5 − 2) / 15 mA = 200 Ω", Ans("ele-13-led-zener", 0), (5 - 2) / 0.015, 0.01);
            Near("q2: (12 − 5.1) / 25 mA = 276 Ω", Ans("ele-13-led-zener", 1), (12 - 5.1) / 0.025, 0.01);
            Near("q3: Pz = 5.1·(12 − 5.1)/276 = 0.1275 W", Ans("ele-13-led-zener", 2), 5.1 * (12 - 5.1) / 276, 0.01);
            // The Zener regulator of q2 in the simulator: a diode with vf = 5.1 and a tiny ron behaves like the Zener.
            var z = Lab(new() { ["netlist"] = "V1 a 0 12; R1 a b 276; D1 b 0 vf=5.1 ron=0.001; RL b 0 255" });
            Near("q2 check: Zener keeps 5.1 V on a 20 mA load (255 Ω)", M(z, "V(b)"), 5.1, 0.1);
            Near("q2 check: 5 mA left for the Zener", M(z, "I(D1)"), 0.005, 1);
            Near("R1 = 270: LED current (12 − 3·2) / (270 + 30) = 20 mA", M(Play("ele-13-led-zener", "R1", "270"), "I(D1)"), 0.02, 0.1);
            Try("ele-13-led-zener", "R1", "270", true);
            Try("ele-13-led-zener", "R1", "240", false);
            Try("ele-13-led-zener", "R1", "300", false);
        }

        log.WriteLine("== ele-14 RC filter");
        {
            Near("q1: fc = 1/(2π·10k·10n) = 1592 Hz", Ans("ele-14-filtros", 0), 1 / (2 * Math.PI * 10e3 * 10e-9), 0.1);
            Near("q2: 20·log10(0.1) = −20 dB", Ans("ele-14-filtros", 1), 20 * Math.Log10(0.1), 0.01);
            Near("q3: 1/√(1 + 10²) = 0.0995", Ans("ele-14-filtros", 2), 1 / Math.Sqrt(101), 0.1);
            var ten = Lab(new() { ["netlist"] = "V1 n1 0 1; R1 n1 n2 1.5915k; C1 n2 0 100n", ["mode"] = "ac", ["f"] = "10k" });
            Near("q3 check: simulator gain one decade above fc", M(ten, "V(n2)"), Ans("ele-14-filtros", 2), 0.5);
            var sol = Play("ele-14-filtros", "R1", "1.6k");
            double fc = 1 / (2 * Math.PI * 1600 * 100e-9);
            Near("R1 = 1.6k: gain at 1 kHz = 1/√(1 + (f/fc)²)", M(sol, "V(n2)"), 1 / Math.Sqrt(1 + Math.Pow(1000 / fc, 2)), 0.1);
            Near("R1 = 1.6k: phase = −atan(f/fc)", M(sol, "ph(n2)"), -Math.Atan(1000 / fc) * 180 / Math.PI, 0.5);
            Near("hint: R = 1/(2π·1k·100n) ≈ 1592 Ω", 1 / (2 * Math.PI * 1000 * 100e-9), 1592, 0.05);
            Try("ele-14-filtros", "R1", "1.6k", true);
            Try("ele-14-filtros", "R1", "1.5k", false);
            Try("ele-14-filtros", "R1", "1.8k", false);
        }

        log.WriteLine("== ele-15 parallel resonance");
        {
            double f0 = 1 / (2 * Math.PI * Math.Sqrt(0.01 * 2.2e-6));
            Near("q1: f0 = 1/(2π√(10m·2.2µ)) = 1073 Hz", Ans("ele-15-ressonancia-paralela", 0), f0, 0.05);
            Near("q2: BW = 1000 / 50 = 20 Hz", Ans("ele-15-ressonancia-paralela", 1), 1000 / 50.0, 0.01);
            Near("q3: Q = 10k / 100 = 100", Ans("ele-15-ressonancia-paralela", 2), 10000 / 100.0, 0.01);
            var tank = Lab(new() { ["netlist"] = "V1 n1 0 10; R1 n1 n2 1k; L1 n2 0 10m; C1 n2 0 2.2u; Rp n2 0 10k", ["mode"] = "ac", ["f"] = f0.ToString(System.Globalization.CultureInfo.InvariantCulture) });
            Near("at f0 the tank is Rp: V = 10·10k/11k = 9.09 V", M(tank, "V(n2)"), 10 * 10000 / 11000.0, 0.1);
            Near("at f0 the source sees a resistance (phase 0°)", M(tank, "phase"), 0, 1);
            Near("theory: loaded Q = (1k ∥ 10k) / (2π·f0·L) ≈ 13.5", (1000.0 * 10000 / 11000) / (2 * Math.PI * f0 * 0.01), 13.5, 1);
            Try("ele-15-ressonancia-paralela", "f", "1070", true);
            Try("ele-15-ressonancia-paralela", "f", "1080", false);
            Try("ele-15-ressonancia-paralela", "f", "1060", false);
        }

        log.WriteLine("== ele-16 transformer (quiz)");
        {
            Near("q1: 220·60/1100 = 12 V", Ans("ele-16-transformador", 0), 220.0 * 60 / 1100, 0.01);
            Near("q2: 2·60/1100 = 0.109 A", Ans("ele-16-transformador", 1), 2.0 * 60 / 1100, 0.5);
            Near("q2: power in = power out (12 V·2 A)", Ans("ele-16-transformador", 1) * 220, 24, 0.5);
            Near("q3: 10²·8 = 800 Ω", Ans("ele-16-transformador", 2), 100 * 8, 0.01);
            double loss1 = Math.Pow(100e3 / 1e3, 2), loss10 = Math.Pow(100e3 / 10e3, 2);
            Near("q4: I² loss ratio 1 kV vs 10 kV = 100", Ans("ele-16-transformador", 3), loss1 / loss10, 0.01);
        }

        log.WriteLine("== ele-17 op-amps (quiz)");
        {
            Near("q1: −47k/4.7k · 0.3 V = −3 V", Ans("ele-17-ampop", 0), -47e3 / 4.7e3 * 0.3, 0.01);
            Near("q2: 1 + 9k/1k = 10", Ans("ele-17-ampop", 1), 1 + 9e3 / 1e3, 0.01);
            Near("q4: 20·log10(100) = 40 dB", Ans("ele-17-ampop", 3), 20 * Math.Log10(100), 0.01);
            Near("q5: −10k·(1/10k + 0.5/10k) = −1.5 V", Ans("ele-17-ampop", 4), -10e3 * (1 / 10e3 + 0.5 / 10e3), 0.01);
            Near("q6: 1 MHz / 100 = 10 kHz", Ans("ele-17-ampop", 5), 1e6 / 100, 0.01);
        }

        log.WriteLine("== ele-18 motors (quiz)");
        {
            double ns = 120 * 60 / 4.0, w = 2 * Math.PI * 1740 / 60, pin = Math.Sqrt(3) * 380 * 15 * 0.85;
            Near("q1: ns = 120·60/4 = 1800 rpm", Ans("ele-18-motores", 0), ns, 0.01);
            Near("q2: s = (1800 − 1740)/1800 = 3.33 %", Ans("ele-18-motores", 1), 100 * (ns - 1740) / ns, 0.2);
            Near("q3: T = 7500 / ω = 41.2 N·m", Ans("ele-18-motores", 2), 7500 / w, 0.2);
            Near("q4: η = 7500 / (√3·380·15·0.85) = 89.4 %", Ans("ele-18-motores", 3), 100 * 7500 / pin, 0.2);
            Near("q4 explanation: P in ≈ 8392 W", pin, 8392, 0.05);
        }

        log.WriteLine("== ele-19 conductor sizing");
        {
            const double rho = 0.0172;
            Near("q1: 0.0172·50/2.5 = 0.344 Ω", Ans("ele-19-condutores", 0), rho * 50 / 2.5, 0.01);
            Near("q2: 0.4·20/127 = 6.3 %", Ans("ele-19-condutores", 1), 100 * 0.4 * 20 / 127, 0.2);
            Near("q3: 0.344·25² = 215 W", Ans("ele-19-condutores", 2), 0.344 * 625, 0.01);
            Near("shower 5500 W / 220 V = 8.8 Ω", 220.0 * 220 / 5500, 8.8, 0.01);
            // The choices are ρ·80 m / A, rounded to 3 digits.
            var choices = new (string r, double a)[] { ("0.917", 1.5), ("0.55", 2.5), ("0.344", 4), ("0.229", 6), ("0.138", 10) };
            foreach (var (r, a) in choices) Near($"choice {a} mm² = ρ·80/A", SI.Parse(r), rho * 80 / a, 0.5);
            var sol = Play("ele-19-condutores", "Rc", "0.344");
            Near("4 mm²: V(shower) = 220·8.8/(8.8 + 0.344)", M(sol, "V(n3)"), 220 * 8.8 / 9.144, 0.01);
            Near("4 mm²: current", M(sol, "I(A1)"), 220 / 9.144, 0.01);
            Near("hint: 2.5 mm² → 220/9.35 = 23.5 A", M(Play("ele-19-condutores", "Rc", "0.55"), "I(A1)"), 23.5, 0.2);
            Near("hint: 2.5 mm² → only 207 V reach the shower", M(Play("ele-19-condutores", "Rc", "0.55"), "V(n3)"), 207, 0.1);
            Try("ele-19-condutores", "Rc", "0.344", true);
            Try("ele-19-condutores", "Rc", "0.229", true);
            Try("ele-19-condutores", "Rc", "0.55", false);
            Try("ele-19-condutores", "Rc", "0.917", false);
        }

        log.WriteLine("== ele-20 fault loop and touch voltage");
        {
            Near("q1: 127 / 0.5 = 254 A", Ans("ele-20-protecao", 0), 127 / 0.5, 0.01);
            Near("q3: 127 V / 1000 Ω = 127 mA", Ans("ele-20-protecao", 2), 127.0 / 1000 * 1000, 0.01);
            Near("phase conductor: 0.0172·40/4 = 0.172 Ω", 0.0172 * 40 / 4, 0.172, 0.01);
            foreach (var (r, a) in new (string r, double a)[] { ("0.459", 1.5), ("0.275", 2.5), ("0.172", 4), ("0.115", 6) })
                Near($"PE choice {a} mm² = 0.0172·40/A", SI.Parse(r), 0.0172 * 40 / a, 0.5);
            var sol = Play("ele-20-protecao", "Rpe", "0.172");
            Near("4 mm² PE: Ifault = 127/(0.1 + 0.172 + 0.172)", M(sol, "I(A1)"), 127 / 0.444, 0.01);
            Near("4 mm² PE: Vtouch = Ifault·Rpe", M(sol, "V(n4)"), 127 / 0.444 * 0.172, 0.01);
            Near("voltmeter shows the touch voltage", M(sol, "V(M1)"), M(sol, "V(n4)"), 0.01);
            Near("hint: 2.5 mm² PE leaves ~64 V on the enclosure", M(Play("ele-20-protecao", "Rpe", "0.275"), "V(n4)"), 63.8, 0.5);
            Try("ele-20-protecao", "Rpe", "0.172", true);
            Try("ele-20-protecao", "Rpe", "0.115", true);
            Try("ele-20-protecao", "Rpe", "0.275", false);
            Try("ele-20-protecao", "Rpe", "0.459", false);
        }
    }
}
