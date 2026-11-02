using System.Globalization;
using System.IO;

namespace EasyCreator;

/// <summary>Lessons 11–20 of the metallurgy track: every number a quiz answer or hint quotes is recomputed from its
/// formula (or from the models the labs use), and every lab is played with its solution and with the neighbouring
/// choices a student would try (which must miss a goal).</summary>
public static partial class MetallurgyTests
{
    static void AdvancedLessons(Track track, Action<string, bool, string> check, Action<string, double, double, double> near,
        Action<string, double, double, double> between, TextWriter log)
    {
        void Check(string name, bool ok, string detail = "") => check(name, ok, detail);
        void Near(string name, double got, double want, double tolPct) => near(name, got, want, tolPct);
        void Between(string name, double got, double lo, double hi) => between(name, got, lo, hi);
        Lesson? Find(string id) => track.Lessons.FirstOrDefault(l => l.Id == id);
        double Ans(string id, int q) => Question.TryNum(Find(id)?.Questions[q].Answer ?? "", out var v) ? v : double.NaN;

        log.WriteLine("== Lessons 11–20");
        Check($"metallurgy track has 20 lessons ({track.Lessons.Count})", track.Lessons.Count == 20);
        Check("lesson ids are unique", track.Lessons.Select(l => l.Id).Distinct().Count() == track.Lessons.Count);
        Check("levels never go down and end at 5", track.Lessons.Zip(track.Lessons.Skip(1)).All(p => p.First.Level <= p.Second.Level) && track.Lessons[^1].Level == 5);
        foreach (var l in track.Lessons.Skip(10).Where(l => l.Kind == "lab"))
            Check($"{l.Id}: lab with a safety note", l.Safety.Length > 0);
        foreach (var l in track.Lessons.Skip(10))
            foreach (var (q, i) in l.Questions.Select((q, i) => (q, i)).Where(x => x.q.Kind == "number"))
                Check($"{l.Id} q{i + 1}: 10 % off is rejected", !q.IsCorrect((Ans(l.Id, i) * 1.1).ToString(CultureInfo.InvariantCulture)));

        // Plays a lesson's lab: the solution inputs, overridden by the given keys.
        IReadOnlyDictionary<string, double> Play(string id, params (string k, string v)[] over)
        {
            var l = Find(id)!;
            var lab = LabRegistry.Create(l.Lab)!;
            lab.Load(l.LabParams);
            var inputs = new Dictionary<string, string>(l.SolutionInputs);
            foreach (var (k, v) in over) inputs[k] = v;
            lab.SetInputs(inputs);
            lab.RunToEnd();
            var m = new Dictionary<string, double>(lab.Metrics);
            lab.Stop();
            return m;
        }
        void Try(string id, bool meets, params (string k, string v)[] over)
        {
            var l = Find(id);
            if (l == null) { Check($"{id} exists", false); return; }
            var m = Play(id, over);
            var missed = l.Goals.Where(g => !g.IsMet(m)).Select(g => $"{g.Metric}={(m.TryGetValue(g.Metric, out var v) ? v : double.NaN):0.###}").ToList();
            string what = over.Length == 0 ? "solution" : string.Join(", ", over.Select(o => $"{o.k}={o.v}"));
            Check($"{id}: {what} {(meets ? "meets every goal" : "misses a goal")}", (missed.Count == 0) == meets,
                missed.Count == 0 ? "(all met)" : "(missed " + string.Join(", ", missed) + ")");
        }

        // ---------- met-11 TTT/CCT ----------
        Near("met-11 q3: Andrews Ms of 1045 = 539 − 423·0.45 − 30.4·0.75", Ans("met-11-ttt", 2), 539 - 423 * 0.45 - 30.4 * 0.75, 0.2);
        Near("met-11 theory: A3 of 1030 ≈ 839 °C", HeatTreatModel.A3(0.30), 839, 0.1);
        Try("met-11-ttt", true);
        Try("met-11-ttt", false, ("cooling", "water"));
        Try("met-11-ttt", false, ("cooling", "air"));
        Try("met-11-ttt", false, ("temperature", "800"));

        // ---------- met-12 hardenability ----------
        Near("met-12 q3: CE(IIW) of 4140", Ans("met-12-temperabilidade", 2), WeldModel.CeIIW(0.40, 0.85, cr: 1.0, mo: 0.20), 0.5);
        Between("met-12 theory: 0.20 % C martensite ≈ 47 HRC", HeatTreatModel.Hrc(HeatTreatModel.MartensiteHV(0.20)), 45, 49);
        Between("met-12 theory: 0.45 % C martensite ≈ 58 HRC", HeatTreatModel.Hrc(HeatTreatModel.MartensiteHV(0.45)), 56, 60);
        Between("met-12 theory: 0.80 % C martensite ≈ 65 HRC", HeatTreatModel.Hrc(HeatTreatModel.MartensiteHV(0.80)), 63, 67);
        Try("met-12-temperabilidade", true);
        Try("met-12-temperabilidade", true, ("carbon", "0.65"));
        Try("met-12-temperabilidade", true, ("carbon", "0.75"));
        Try("met-12-temperabilidade", false, ("carbon", "0.45"));
        Try("met-12-temperabilidade", false, ("carbon", "0.60"));
        Try("met-12-temperabilidade", false, ("carbon", "0.80"));
        Try("met-12-temperabilidade", false, ("carbon", "0.45"), ("temperature", "840"), ("cooling", "water"));

        // ---------- met-13 symbols ----------
        Near("met-13 q5: throat = z·cos 45° = 8·0.707", Ans("met-13-simbologia", 4), 8 * Math.Cos(Math.PI / 4), 0.1);

        // ---------- met-14 HAZ ----------
        Near("met-14 q1: Düren HV (C 0.20, CE 0.45, t8/5 10 s)", Ans("met-14-zta", 0), WeldModel.HazHardness(0.20, 0.45, 10), 0.2);
        Near("met-14 q2: t8/5 thick plate, Q 1.0, T0 20 °C", Ans("met-14-zta", 1), WeldModel.CoolingTime(1.0, 20, 40, 1, 1), 0.2);
        {
            // Hint: 350 HV needs t8/5 above ~12 s for this steel.
            double lo = WeldModel.HazHardness(0.20, 0.45, 11), hi = WeldModel.HazHardness(0.20, 0.45, 12.5);
            Check($"met-14 hint: 350 HV is crossed between t8/5 = 11 and 12.5 s ({lo:0} → {hi:0} HV)", lo > 350 && hi < 350);
            var sol = Play("met-14-zta");
            Check($"met-14 solution: t8/5 above 12 s ({sol["coolingTime"]:0.0} s)", sol["coolingTime"] >= 12);
        }
        Try("met-14-zta", true);
        Try("met-14-zta", false, ("travelSpeed", "6"));
        Try("met-14-zta", true, ("travelSpeed", "6"), ("preheat", "200"));
        Try("met-14-zta", false, ("travelSpeed", "6"), ("preheat", "150"));

        // ---------- met-15 stainless ----------
        Near("met-15 q3: Schaeffler Cr eq = 18.5 + 1.5·0.5", Ans("met-15-inox", 2), 18.5 + 0 + 1.5 * 0.5 + 0.5 * 0, 0.01);
        Between("met-15 hint: 1.0 mm wire at 6 m/min ≈ 165 A", WeldModel.MigCurrent(new WeldInputs { WireDiameter = 1.0, WireFeed = 6, Stickout = 15 }), 160, 170);
        Between("met-15 hint: ideal voltage ≈ 22.5 V", WeldModel.MigBestVoltage(WeldModel.MigCurrent(new WeldInputs { WireDiameter = 1.0, WireFeed = 6, Stickout = 15 })), 22, 23);
        Between("met-15 hint: heat input ≈ 0.6 kJ/mm", Play("met-15-inox")["heatInput"], 0.55, 0.65);
        Try("met-15-inox", true);
        Try("met-15-inox", false, ("gas", "ar-co2"));
        Try("met-15-inox", false, ("wireFeed", "7"), ("voltage", "23.5"), ("travelSpeed", "3"));

        // ---------- met-16 aluminium ----------
        Near("met-16 q4: 0.60 × 310 MPa", Ans("met-16-aluminio", 3), 0.60 * 310, 0.01);
        Between("met-16 hint: 1.0 mm wire at 8 m/min ≈ 210 A", WeldModel.MigCurrent(new WeldInputs { WireDiameter = 1.0, WireFeed = 8, Stickout = 15 }), 200, 215);
        Between("met-16 hint: ideal voltage ≈ 24.5 V", WeldModel.MigBestVoltage(WeldModel.MigCurrent(new WeldInputs { WireDiameter = 1.0, WireFeed = 8, Stickout = 15 })), 24, 25);
        Try("met-16-aluminio", true);
        Try("met-16-aluminio", false, ("gas", "ar-co2"));
        Try("met-16-aluminio", false, ("wireFeed", "5"), ("voltage", "21.5"), ("travelSpeed", "7"));

        // ---------- met-17 fracture ----------
        Near("met-17 q1: K = 1.12·200·√(π·0.004)", Ans("met-17-fratura", 0), 1.12 * 200 * Math.Sqrt(Math.PI * 0.004), 0.2);
        Near("met-17 q2: a_c = (1/π)(50/250)² in mm", Ans("met-17-fratura", 1), 1000 / Math.PI * Math.Pow(50.0 / 250, 2), 0.3);
        Near("met-17 q3: E = 20·9.81·0.8", Ans("met-17-fratura", 2), 20 * 9.81 * 0.8, 0.1);

        // ---------- met-18 fatigue ----------
        Near("met-18 q1: N = 2e6·(80/160)³", Ans("met-18-fadiga", 0), 2e6 * Math.Pow(80.0 / 160, 3), 0.01);
        Near("met-18 q2: Miner 100k/500k + 50k/200k", Ans("met-18-fadiga", 1), 100e3 / 500e3 + 50e3 / 200e3, 0.01);
        Near("met-18 q3: 2³", Ans("met-18-fadiga", 2), Math.Pow(2, 3), 0.01);

        // ---------- met-19 qualification ----------
        Near("met-19 q2: 110 kN / (19·10 mm²)", Ans("met-19-qualificacao", 1), 110e3 / 190, 0.1);
        Near("met-19 q5: F = 565·π·9²/4 (kN)", Ans("met-19-qualificacao", 4), 565 * Math.PI * 81 / 4 / 1000, 0.2);
        Near("met-19 q5 agrees with the tensile lab", Ans("met-19-qualificacao", 4), Play("met-19-qualificacao")["maxForce"], 0.2);
        {
            // Hint: A0 between 53 and 71 mm² ↔ d between ~8.2 and ~9.5 mm.
            Near("met-19 hint: 30 kN / 565 MPa ≈ 53 mm²", 30e3 / 565, 53, 0.5);
            Near("met-19 hint: 40 kN / 565 MPa ≈ 71 mm²", 40e3 / 565, 71, 0.5);
            Near("met-19 hint: d for 53 mm² ≈ 8.2 mm", Math.Sqrt(4 * 30e3 / 565 / Math.PI), 8.2, 0.5);
            Near("met-19 hint: d for 71 mm² ≈ 9.5 mm", Math.Sqrt(4 * 40e3 / 565 / Math.PI), 9.5, 0.5);
        }
        Try("met-19-qualificacao", true);
        Try("met-19-qualificacao", true, ("diameter", "8.5"));
        Try("met-19-qualificacao", false, ("diameter", "9.5"));
        Try("met-19-qualificacao", false, ("diameter", "8"));
        Try("met-19-qualificacao", false, ("material", "inox304"));
        Try("met-19-qualificacao", false, ("material", "aco4140"), ("diameter", "6"));

        // ---------- met-20 PWHT ----------
        Near("met-20 q1: σ = 200 000·12e-6·100", Ans("met-20-ttps", 0), 200e3 * 12e-6 * 100, 0.01);
        Near("met-20 q2: 38 / 25 h", Ans("met-20-ttps", 1), 38 / 25.0, 0.01);
        Near("met-20 q3: Hollomon–Jaffe 893.15·(20 + log 1)", Ans("met-20-ttps", 2), (620 + 273.15) * (20 + Math.Log10(1)), 0.01);
        Between("met-20 hint: lesson 4's 375 °C temper ≈ 48 HRC", HeatTreatModel.Compute(0.45, 840, "water", 375).HRC, 46.5, 49);
        Between("met-20 theory: 1045 as quenched ≈ 58 HRC", HeatTreatModel.Compute(0.45, 840, "water", 0).HRC, 56, 60);
        Try("met-20-ttps", true);
        Try("met-20-ttps", false, ("temper", "625"));
        Try("met-20-ttps", false, ("temper", "375"));
        Try("met-20-ttps", false, ("cooling", "air"), ("temper", "0"));
    }
}
