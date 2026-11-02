using System.IO;

namespace EasyCreator;

/// <summary>Headless checks of the metallurgy models against hand-computed reference values: heat input and process
/// efficiencies (EN 1011-1), CE(IIW), t8/5 (EN 1011-2), Düren's HAZ hardness, the Fe–Fe₃C diagram and the lever rule,
/// martensite hardness, the tensile material table (handbook values) and the numbers quoted by the lessons.
/// Runs on the UI thread (the tensile lab is a control).</summary>
public static class MetallurgyTests
{
    public static int Run(TextWriter log)
    {
        int fails = 0, passes = 0;
        void Check(string name, bool ok, string detail = "")
        {
            if (ok) { passes++; log.WriteLine("  ok   " + name); }
            else { fails++; log.WriteLine("  FAIL " + name + (detail.Length > 0 ? " — " + detail : "")); }
        }
        void Near(string name, double got, double want, double tolPct)
        {
            bool ok = want == 0 ? Math.Abs(got) <= tolPct / 100 : Math.Abs(got - want) <= Math.Abs(want) * tolPct / 100;
            Check(name, ok, $"got {got:0.####}, want {want:0.####} ±{tolPct}%");
        }
        void Between(string name, double got, double lo, double hi) => Check(name, got >= lo && got <= hi, $"got {got:0.###}, want {lo}..{hi}");

        try
        {
            // ---------- heat input and efficiencies (EN 1011-1 / ISO/TR 17671-1) ----------
            Near("efficiency SMAW 0.8", WeldModel.Efficiency("smaw"), 0.8, 0.1);
            Near("efficiency GMAW 0.8", WeldModel.Efficiency("gmaw"), 0.8, 0.1);
            Near("efficiency GTAW 0.6", WeldModel.Efficiency("gtaw"), 0.6, 0.1);
            Near("heat input 0.8·22·120/3 = 0.704 kJ/mm", WeldModel.HeatInput(0.8, 22, 120, 3), 0.704, 0.1);
            Near("heat input 0.8·24·160/3 = 1.024 kJ/mm", WeldModel.HeatInput(0.8, 24, 160, 3), 1.024, 0.1);
            Near("heat input TIG 0.6·12·100/2 = 0.36 kJ/mm", WeldModel.HeatInput(0.6, 12, 100, 2), 0.36, 0.1);
            {
                // the full model uses the same formula with its own current and voltage
                var s = new WeldSetup { Process = "gtaw", Metal = "aluminum", Thickness = 3, Joint = "butt" };
                var x = new WeldInputs { Current = 135, ArcLength = 2.5, TravelSpeed = 2.5, Polarity = "ac", Gas = "ar", GasFlow = 10, Angle = 15 };
                var r = WeldModel.Compute(s, x);
                Near("model heat input = η·U·I/v (TIG)", r.HeatInput, 0.6 * r.Voltage * r.Current / 2.5 / 1000, 0.01);
            }

            // ---------- carbon equivalent CE(IIW) ----------
            Near("CE(IIW) lesson example = 0.507", WeldModel.CeIIW(0.20, 1.40, cr: 0.20, mo: 0.10, ni: 0.10, cu: 0.10), 0.5067, 0.2);
            Near("CE(IIW) C 0.15 Mn 1.2 = 0.35", WeldModel.CeIIW(0.15, 1.2), 0.35, 0.1);
            Near("CE(IIW) Cr/Mo/V over 5, Ni/Cu over 15", WeldModel.CeIIW(0, 0, cr: 1, mo: 0.5, v: 0.5, ni: 1.5), 0.5, 0.1);

            // ---------- t8/5, EN 1011-2 ----------
            // 3D: (6700 − 5·20)·1.0·(1/480 − 1/780) = 5.288 s; 2D, d = 10 mm: (4300 − 86)·1e5·(1/100)·(1/480² − 1/780²) = 11.36 s
            Near("t8/5 thin plate (2D) 10 mm, Q 1.0, T0 20", WeldModel.CoolingTime(1.0, 20, 10, 1, 1), 11.364, 0.5);
            Near("t8/5 thick plate (3D) 40 mm, Q 1.0, T0 20", WeldModel.CoolingTime(1.0, 20, 40, 1, 1), 5.288, 0.5);
            Near("t8/5 3D with F3 0.67", WeldModel.CoolingTime(1.0, 20, 60, 0.67, 0.67), 5.288 * 0.67, 0.5);
            Check("t8/5 grows with preheat", WeldModel.CoolingTime(1.0, 150, 25, 0.9, 0.9) > WeldModel.CoolingTime(1.0, 20, 25, 0.9, 0.9));

            // ---------- Düren HAZ hardness ----------
            // C 0.20, CE 0.40, t8/5 10 s: 2019·(0.2·0.5 + 0.3·0.2) + 66·0.2 = 336.2 HV
            Near("Düren HV (C 0.20, CE 0.40, t8/5 10 s)", WeldModel.HazHardness(0.20, 0.40, 10), 336.2, 0.5);
            Check("HAZ hardness never above martensite (Yurioka)", WeldModel.HazHardness(0.20, 0.60, 0.5) <= HeatTreatModel.MartensiteHV(0.20) + 1e-9);

            // ---------- recommended current ranges (manufacturer tables, flat position) ----------
            void Range(string e, double d, double typical)
            {
                var (lo, hi) = WeldModel.ElectrodeRange(e, d);
                Check($"{e} Ø{d} range {lo:0}–{hi:0} A contains {typical} A", lo <= typical && typical <= hi);
            }
            Range("E6013", 2.5, 75); Range("E6013", 3.25, 110); Range("E6013", 4.0, 150);
            Range("E7018", 2.5, 85); Range("E7018", 3.25, 120); Range("E7018", 4.0, 165);
            Range("E6010", 3.25, 100); Range("E308L", 3.25, 90);
            {
                var (lo, hi) = WeldModel.CurrentWindow(new WeldSetup { Process = "smaw", Position = "vertical" }, new WeldInputs { Electrode = "E7018", Diameter = 3.25 });
                Check($"vertical window lowers the top of the range ({lo:0}–{hi:0} A)", hi < 145 && hi >= 120);
            }
            Near("MIG voltage rule 14 + 0.05·180 = 23 V", WeldModel.MigBestVoltage(180), 23, 0.1);
            Between("MIG 1.0 mm at 5 m/min ≈ 145 A", WeldModel.MigCurrent(new WeldInputs { WireDiameter = 1.0, WireFeed = 5, Stickout = 15 }), 130, 160);
            Between("MIG 1.0 mm at 8 m/min ≈ 210 A", WeldModel.MigCurrent(new WeldInputs { WireDiameter = 1.0, WireFeed = 8, Stickout = 15 }), 190, 225);
            Check("longer stick-out lowers the MIG current",
                WeldModel.MigCurrent(new WeldInputs { WireFeed = 6, Stickout = 25 }) < WeldModel.MigCurrent(new WeldInputs { WireFeed = 6, Stickout = 12 }));

            // ---------- process / gas / polarity choices ----------
            {
                var al = new WeldSetup { Process = "gtaw", Metal = "aluminum", Thickness = 3, Joint = "butt" };
                var good = new WeldInputs { Current = 135, ArcLength = 2.5, TravelSpeed = 2.5, Polarity = "ac", Gas = "ar", GasFlow = 10, Angle = 15 };
                var dcen = new WeldInputs { Current = 135, ArcLength = 2.5, TravelSpeed = 2.5, Polarity = "dcen", Gas = "ar", GasFlow = 10, Angle = 15 };
                var co2 = new WeldInputs { Current = 135, ArcLength = 2.5, TravelSpeed = 2.5, Polarity = "ac", Gas = "ar-co2", GasFlow = 10, Angle = 15 };
                Check("TIG aluminum: AC + argon is a good weld", WeldModel.Compute(al, good).Quality >= 80);
                Check("TIG aluminum on DC− fails to fuse (oxide)", WeldModel.Compute(al, dcen).LackOfFusion >= 0.5);
                Check("TIG aluminum with Ar + CO₂ is porous", WeldModel.Compute(al, co2).Porosity >= 0.5);
                var migAl = new WeldSetup { Process = "gmaw", Metal = "aluminum", Thickness = 4, Joint = "butt" };
                Check("MIG aluminum with Ar + CO₂ is porous",
                    WeldModel.Compute(migAl, new WeldInputs { WireDiameter = 1.0, WireFeed = 7, Voltage = 22, Stickout = 15, TravelSpeed = 6, Angle = 10, Gas = "ar-co2", GasFlow = 15 }).Porosity >= 0.5);
                var inox = new WeldSetup { Process = "smaw", Metal = "stainless", Thickness = 4, Joint = "fillet" };
                Check("E6013 on stainless cracks, E308L does not",
                    WeldModel.Compute(inox, new WeldInputs { Electrode = "E6013", Diameter = 3.25, Current = 100, ArcLength = 3.25, TravelSpeed = 3, Angle = 15 }).Crack >
                    WeldModel.Compute(inox, new WeldInputs { Electrode = "E308L", Diameter = 3.25, Current = 95, ArcLength = 3.25, TravelSpeed = 3, Angle = 15 }).Crack + 0.4);
            }

            // ---------- Fe–Fe₃C diagram ----------
            Near("A1 = 727 °C", HeatTreatModel.A1, 727, 0.01);
            Between("eutectoid 0.76–0.77 % C", HeatTreatModel.Eutectoid, 0.76, 0.77);
            Near("A3 of pure iron = 912 °C", HeatTreatModel.A3(0), 912, 0.01);
            Near("A3 meets A1 at the eutectoid", HeatTreatModel.A3(HeatTreatModel.Eutectoid), 727, 0.01);
            Near("Acm at 2.11 % C = 1148 °C", HeatTreatModel.Acm(2.11), 1148, 0.01);
            Between("A3 of a 0.45 % C steel ≈ 770–805 °C", HeatTreatModel.A3(0.45), 770, 805);
            Between("Acm of a 1.0 % C steel ≈ 790–830 °C", HeatTreatModel.Acm(1.0), 790, 830);

            // ---------- lever rule ----------
            var slow40 = HeatTreatModel.Compute(0.40, 25, "furnace", 0);
            Near("lever rule: 1040 annealed = 51.2 % pearlite", slow40.Pearlite, (0.40 - 0.022) / (0.76 - 0.022), 0.5);
            Near("lever rule: 1040 annealed = 48.8 % ferrite", slow40.Ferrite, 1 - (0.40 - 0.022) / (0.76 - 0.022), 0.5);
            Near("eutectoid steel annealed = 100 % pearlite", HeatTreatModel.Compute(0.76, 780, "furnace", 0).Pearlite, 1, 0.5);
            var slow12 = HeatTreatModel.Compute(1.2, 25, "furnace", 0);
            Near("lever rule: 1.2 % C = 92.6 % pearlite", slow12.Pearlite, (6.67 - 1.2) / (6.67 - 0.76), 0.5);
            Near("lever rule: 1.2 % C = 7.4 % proeutectoid cementite", slow12.Cementite, (1.2 - 0.76) / (6.67 - 0.76), 1);
            {
                var (f, cg, _) = HeatTreatModel.AtTemperature(0.40, 760);
                double cgRef = 0.76 * (912 - 760) / (912 - 727.0);
                Near("intercritical 0.40 % C at 760 °C: γ carbon on the A3 line", cg, cgRef, 0.5);
                Near("intercritical 0.40 % C at 760 °C: γ fraction by the lever rule", f, (0.40 - 0.022) / (cgRef - 0.022), 0.5);
                var (f2, _, _) = HeatTreatModel.AtTemperature(0.40, 840);
                Near("0.40 % C at 840 °C is fully austenite", f2, 1, 0.01);
                var (f3, _, _) = HeatTreatModel.AtTemperature(0.40, 700);
                Near("below A1 there is no austenite", f3, 0, 0.01);
            }

            // ---------- martensite and hardness conversion ----------
            Between("martensite of 0.20 % C ≈ 44–50 HRC", HeatTreatModel.Hrc(HeatTreatModel.MartensiteHV(0.20)), 44, 50);
            Between("martensite of 0.45 % C ≈ 56–61 HRC", HeatTreatModel.Hrc(HeatTreatModel.MartensiteHV(0.45)), 56, 61);
            Between("martensite of 0.80 % C ≈ 63–67 HRC", HeatTreatModel.Hrc(HeatTreatModel.MartensiteHV(0.80)), 63, 67);
            Between("HV 250 ≈ 22 HRC (ASTM E140)", HeatTreatModel.Hrc(250), 20, 24);
            Between("HV 500 ≈ 49 HRC (ASTM E140)", HeatTreatModel.Hrc(500), 47, 51);
            {
                var q = HeatTreatModel.Compute(0.45, 840, "water", 0);
                Check($"1045 water-quenched from 840 °C: ≥ 90 % martensite ({q.Martensite:P0})", q.Martensite >= 0.9);
                Between("1045 as quenched ≈ 55–60 HRC", q.HRC, 55, 60);
                var t600 = HeatTreatModel.Compute(0.45, 840, "water", 600);
                Check("tempering lowers hardness and raises toughness", t600.HRC < q.HRC - 15 && t600.Toughness > q.Toughness + 20);
                var part = HeatTreatModel.Compute(0.45, 760, "water", 0);
                Check("1045 quenched from 760 °C keeps soft ferrite", part.Ferrite > 0.1);
            }

            // ---------- tensile material table (Callister / ASM / MatWeb typical values) ----------
            var refs = new Dictionary<string, (double E, double ys, double uts, double el)>
            {
                ["aco1020"] = (207, 210, 380, 25), ["aco1045"] = (207, 310, 565, 16), ["aco4140"] = (205, 1140, 1250, 13),
                ["inox304"] = (193, 205, 515, 40), ["al6061"] = (69, 276, 310, 12), ["cobre"] = (115, 69, 220, 45),
                ["titanio"] = (114, 880, 950, 14), ["fofo"] = (100, 0, 200, 0.5),
            };
            foreach (var m in TensileLab.Materials)
            {
                if (!refs.TryGetValue(m.Id, out var rf)) { Check($"material {m.Id} has reference data", false); continue; }
                Near($"{m.Id}: E", m.E, rf.E, 12);
                if (!m.Brittle) Near($"{m.Id}: yield", m.Yield, rf.ys, 12);
                Near($"{m.Id}: UTS", m.Uts, rf.uts, 10);
                if (!m.Brittle) Near($"{m.Id}: elongation", m.Elong, rf.el, 20);
                Check($"{m.Id}: yield ≤ UTS", m.Yield <= m.Uts);
            }
            {
                var lab = new TensileLab();
                lab.Load(new Dictionary<string, string> { ["material"] = "aco1020", ["diameter"] = "12", ["gaugeLength"] = "50" });
                lab.RunToEnd();
                var mt = lab.Metrics;
                Near("1020 Ø12: F max = σr·A0 = 42.98 kN", mt["maxForce"], 380 * Math.PI * 144 / 4 / 1000, 0.5);
                Near("1020 Ø12: 0.2 % offset yield ≈ σe", mt["yield"], 210, 3);
                Between("1020: elongation after fracture ≈ 25 %", mt["elongation"], 22, 26);
                lab.SetInputs(new Dictionary<string, string> { ["material"] = "al6061", ["diameter"] = "10" });
                lab.RunToEnd();
                Near("6061-T6: 0.2 % offset yield ≈ 276 MPa", lab.Metrics["yield"], 276, 3);
                lab.SetInputs(new Dictionary<string, string> { ["material"] = "fofo" });
                lab.RunToEnd();
                Check("gray cast iron breaks below 1 % strain", lab.Metrics["elongation"] < 1);
                lab.Stop();
            }

            // ---------- numbers quoted by the lessons ----------
            var track = ContentStore.Find("metallurgy");
            Check("metallurgy track loaded", track != null);
            if (track != null)
            {
                string Ans(string lesson, int q) => track.Lessons.First(l => l.Id == lesson).Questions[q].Answer;
                double N(string s) => Question.TryNum(s, out var v) ? v : double.NaN;
                Near("lesson 2: σ = 20 kN / (π·10²/4)", N(Ans("met-02-tracao", 0)), 20000 / (Math.PI * 100 / 4), 0.2);
                Near("lesson 3: lever rule 1040 (%)", N(Ans("met-03-ferro-carbono", 1)), 100 * (0.40 - 0.022) / (0.76 - 0.022), 0.5);
                Near("lesson 6: heat input", N(Ans("met-06-eletrodo", 0)), WeldModel.HeatInput(0.8, 22, 120, 3), 0.2);
                Near("lesson 7: MIG voltage for 180 A", N(Ans("met-07-mig", 2)), WeldModel.MigBestVoltage(180), 0.2);
                Near("lesson 8: TIG 2 mm at 1 A per 0.025 mm", N(Ans("met-08-tig", 1)), 2 / 0.025, 0.2);
                Near("lesson 10: CE(IIW)", N(Ans("met-10-engenharia", 0)), WeldModel.CeIIW(0.20, 1.40, cr: 0.20, mo: 0.10, ni: 0.10, cu: 0.10), 0.3);
                Near("lesson 10: heat input", N(Ans("met-10-engenharia", 1)), WeldModel.HeatInput(0.8, 24, 160, 3), 0.2);
                Check("track tells to learn with a qualified instructor and PPE",
                    track.Description.Contains("SENAI") && track.Description.Contains("EPI"));
                foreach (var l in track.Lessons.Where(l => l.Lab == "welding" || l.Id == "met-05-seguranca"))
                    Check($"{l.Id}: safety text with PPE and an instructor",
                        l.Safety.Length > 0 && l.Safety.Contains("instrutor", StringComparison.OrdinalIgnoreCase));
            }
        }
        catch (Exception ex) { fails++; log.WriteLine("  FAIL crash " + ex); }
        log.WriteLine($"== {passes} passed, {fails} failed");
        return fails;
    }
}
