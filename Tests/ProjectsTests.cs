using System.IO;
using EasyCreator.ProjectLabs;

namespace EasyCreator;

/// <summary>Rocket and submarine project simulators against closed-form results: motor catalog vs impulse classes,
/// vacuum-like ballistic flight of a constant-thrust rocket, Tsiolkovsky Δv, parachute terminal speed, neutral
/// buoyancy, pressure at 100 m and the steady-state error of a P depth controller. Headless.</summary>
public static class ProjectsTests
{
    public static int Run(TextWriter log)
    {
        int fails = 0, passes = 0;
        void Check(string name, bool ok, string detail = "")
        {
            if (ok) { passes++; log.WriteLine("  ok   " + name); }
            else { fails++; log.WriteLine($"  FAIL {name} {detail}"); }
        }
        void Near(string name, double got, double want, double tolPct)
            => Check(name, !double.IsNaN(got) && Math.Abs(got - want) <= Math.Abs(want) * tolPct / 100, $"(got {got:G6}, want {want:G6})");

        // ---- motor catalog: impulse inside its letter class (A ≤ 2.5, each letter doubles), curve area exact
        foreach (var m in MotorSpec.Catalog)
        {
            double hi = 2.5 * Math.Pow(2, m.Id[0] - 'A');
            Check($"motor {m.Id} in class {m.Id[0]}", m.Impulse > hi / 2 && m.Impulse <= hi + 1e-9, $"({m.Impulse} N·s)");
            Near($"motor {m.Id} curve impulse", Engine.FromMotor(m).Impulse, m.Impulse, 0.01);
            Check($"motor {m.Id} Isp plausible", m.Isp > 60 && m.Isp < 260, $"({m.Isp:F0} s)");
        }

        // ---- constant-thrust rocket with negligible drag (needle-thin), no chute: analytic burnout + coast
        {
            double isp = 100, flow = 0.01, prop = 0.01, dry = 0.05, g = RocketSim.G0;
            var s = new RocketSim { Motor = "custom", Isp = isp, MassFlow = flow, PropKg = prop, DryKg = dry,
                DiameterMm = 0.01, ChuteCm = 0, Guided = true };
            s.Run();
            double ve = isp * g, tb = prop / flow, m0 = dry + prop, mf = dry;
            double vbo = ve * Math.Log(m0 / mf) - g * tb;
            double hbo = ve * tb - 0.5 * g * tb * tb - ve * (mf / flow) * Math.Log(m0 / mf);
            Near("ballistic apogee", s.Apogee, hbo + vbo * vbo / (2 * g), 0.5);
            Near("ballistic landing speed (free fall)", s.LandingSpeed, Math.Sqrt(2 * g * s.Apogee), 0.5);
            Near("Tsiolkovsky Δv", s.DeltaV, ve * Math.Log(m0 / mf), 0.01);
        }

        // ---- parachute descent: v = √(2mg / (ρ·Cd·A))
        {
            var s = new RocketSim { Motor = "B6", DryKg = 0.05, ChuteCm = 60, DelayS = 4, Guided = true };
            s.Run();
            double m = 0.05 + MotorSpec.Find("B6")!.CaseKg, a = Math.PI * 0.3 * 0.3;
            Near("chute terminal speed", s.LandingSpeed, Math.Sqrt(2 * m * RocketSim.G0 / (RocketSim.Rho0 * 0.8 * a)), 3);
        }

        // ---- submarine: neutral buoyancy holds depth; pressure at 100 m; P controller offset = ΔF / Kp
        {
            var s = new SubmarineSim { HullL = 20, HullKg = 20, BallastL = 0, Start = 5, Floor = 50, Duration = 30, ForwardN = 0 };
            s.Run();
            Near("neutral: net buoyancy 0", s.NetBuoyancy + 1, 1, 0.01);
            Near("neutral: stays at 5 m", s.FinalDepth, 5, 0.5);
        }
        {
            var s = new SubmarineSim { HullL = 20, HullKg = 20, BallastL = 0, Start = 100, Floor = 200, RatingBar = 50, Duration = 1 };
            s.Run();
            Near("pressure at 100 m (abs, bar)", s.PressureBar, (101325 + 1000 * 9.81 * 100) / 1e5, 0.2);
        }
        {
            double light = 0.2; // kg lighter than neutral → ΔF = 0.2·g up
            var s = new SubmarineSim { HullL = 20, HullKg = 20 - light, BallastL = 0, Pid = true, Kp = 4, Ki = 0, Kd = 20,
                ThrustN = 50, TargetM = 10, Floor = 50, Duration = 400 };
            s.Run();
            Near("P-only steady error", s.FinalDepth, 10 - light * 9.81 / 4, 0.3);
            s.Ki = 0.5; s.Run();
            Near("PI removes the error", s.FinalDepth, 10, 0.3);
        }

        // ---- lessons 11–20 of rockets and submarines: every numeric answer recomputed (NaN = choice question)
        {
            const double g = 9.81, g0 = RocketSim.G0, GM = 3.986e14, R = 6.371e6, N = double.NaN;
            double Vis(double r, double a) => Math.Sqrt(GM * (2 / r - 1 / a));
            var want = new Dictionary<string, double[]>
            {
                ["rkt-11-agua-otima"] = new[] { 2 * Math.PI * 0.011 * 0.011 * 5e5, Math.Sqrt(2 * 5e5 / 1000), N },
                ["rkt-12-estavel-demais"] = new[] { (380 - 290) / 30.0, N, 1.5 * 41 },
                ["rkt-13-bocal"] = new[] { N, 100 * 2500 + (50000 - 101325) * 0.5, 100 * 2500 + 50000 * 0.5, 275000 / (100 * g0), Math.Pow(0.8 / 0.2, 2) },
                ["rkt-14-propelentes"] = new[] { N, 100 * 2.56 / 3.56, Math.Sqrt(2), 10000 / 71.0, N },
                ["rkt-15-estagios-otimos"] = new[] { 2 * 300 * g0 * Math.Log(3), Math.Exp(4500 / (350 * g0)), 1 / 0.1, N, N },
                ["rkt-16-perdas-gravidade"] = new[] { g * 100, 0.5 * 0.38 * 1000 * 1000 * 0.5 * 0.07, N },
                ["rkt-17-reentrada"] = new[] { 0.5 * 7800 * 7800 / 1e6, 1000 / (1.2 * Math.PI), Math.Sqrt(2 * 1000 / (1.2 * Math.PI) * g / 1.225), N, N },
                ["rkt-18-mecanica-orbital"] = new[] { 2 * Math.PI * Math.Sqrt(Math.Pow(R + 400e3, 3) / GM) / 60, Math.Sqrt(2 * GM / R),
                    Math.Cbrt(GM * 86164.0 * 86164 / (4 * Math.PI * Math.PI)) / 1000,
                    Vis(6.771e6, (6.771e6 + 42164e3) / 2) - Math.Sqrt(GM / 6.771e6), N },
                ["rkt-19-competicao"] = new[] { 250 / 6.0, 8 + 240 / 7.0, N },
                ["rkt-20-lancador-orbital"] = new[] { 300 * g0 * Math.Log(50 / 20.0), 280 * 250 / 53500.0, N },
                ["sub-11-espuma-carga"] = new[] { 1 / (1000 - 400.0) * 1000, 1 / (1025 - 400.0) * 1000, N },
                ["sub-12-velocidade-vertical"] = new[] { Math.Sqrt(2 * 10 / (1000 * 1.1 * 0.16)), 40, N },
                ["sub-13-espessura-casco"] = new[] { 60e6 * 1 / (2 * 900e6) * 1000, 60e6 / (1025 * g), 1e6 * 0.1 / (250e6 / 2) * 1000, N, N },
                ["sub-14-sonar"] = new[] { 1500 * 0.8 / 2, 1500 / 200000.0 * 1000, 2 * 5 * 10000 / 1500.0, 20 * Math.Log10(1000), N },
                ["sub-15-energia-autonomia"] = new[] { 2000 / 200.0, 2000 / 200.0 * 3600 * 1 / 1000, 2000 / (50 + 150 * 8.0) * 3600 * 2 / 1000, Math.Cbrt(50 / (2 * 150.0)), N },
                ["sub-16-pid-saturacao"] = new[] { 2 * g / 30, 30 - 2 * g, N },
                ["sub-17-mergulho-profundo"] = new[] { 1025 * g * 90 / 1e5, 1.5 * 1025 * g * 90 / 1e5, 90 / 0.42 },
                ["sub-18-hidrodinamica"] = new[] { 2 * 1 / 1e-6, 0.5 * 1025 * 0.1 * 0.03 * 27, 8, N, N },
                ["sub-19-fadiga-seguranca"] = new[] { 1025 * g * 4000 / 1e6, 1.5 * 1025 * g * 4000 / 1e6, 50 / 200.0 + 30 / 100.0, N, N },
                ["sub-20-projeto-auv"] = new[] { 1.5 * 1025 * g * 60 / 1e5, 33 * 1.025 - 30, 500 / 50.0 * 3600 * 2 / 1000 },
            };
            foreach (var id in new[] { "rockets", "submarines" })
            {
                var track = ContentStore.Find(id);
                if (track == null) { Check(id + " loaded", false); continue; }
                Check($"{id}: 20 lessons", track.Lessons.Count == 20, $"({track.Lessons.Count})");
                foreach (var l in track.Lessons.Skip(10))
                {
                    if (!want.TryGetValue(l.Id, out var ans)) { Check($"{l.Id}: expected answers listed", false); continue; }
                    Check($"{l.Id}: {l.Questions.Count} questions", l.Questions.Count == ans.Length);
                    for (int i = 0; i < Math.Min(ans.Length, l.Questions.Count); i++)
                    {
                        var q = l.Questions[i];
                        if (double.IsNaN(ans[i])) { Check($"{l.Id} q{i + 1}: choice", q.Kind == "choice"); continue; }
                        Question.TryNum(q.Answer, out var given);
                        Near($"{l.Id} q{i + 1}: answer {q.Answer} {q.Unit}", given, ans[i], 0.5);
                        Check($"{l.Id} q{i + 1}: exact value accepted", q.IsCorrect(ans[i].ToString(System.Globalization.CultureInfo.InvariantCulture)));
                    }
                }
            }

            // answers the simulators must reproduce
            var w = new RocketSim { Motor = "water", PressureBar = 5, WaterL = 0.6, DryKg = 0.15, NozzleMm = 22 };
            w.ComputeStatic();
            Near("rkt-11: water rocket thrust at ignition = 2·A·ΔP", w.AvgThrust, 2 * Math.PI * 0.011 * 0.011 * 5e5, 0.1);
            var lv = new RocketSim { Motor = "custom", Motor2 = "custom", Stages = 2, Guided = true, Isp = 280, MassFlow = 250, PropKg = 40000,
                Isp2 = 330, MassFlow2 = 40, PropKg2 = 8000, DryKg = 1500, BoosterKg = 4000, DiameterMm = 2000, LengthMm = 30000, FinSpanMm = 0, ChuteCm = 0 };
            lv.ComputeStatic();
            Near("rkt-20: liftoff mass 53.5 t", lv.LiftoffKg, 53500, 0.01);
            Near("rkt-20: liftoff T/W", lv.ThrustToWeight, 280 * 250 / 53500.0, 0.1);
            var foam = new SubmarineSim { HullL = 20 + 1 / 0.6, HullKg = 18 + 0.4 / 0.6, PayloadKg = 3 };
            foam.ComputeStatic();
            Near("sub-11: 1.67 L of 400 kg/m³ foam makes it neutral", foam.NetBuoyancy + 1, 1, 0.01);
            var auv = new SubmarineSim { HullL = 33, HullKg = 27, PayloadKg = 3, Salt = true, TrimG = (33 * 1.025 - 30) * 1000 };
            auv.ComputeStatic();
            Near("sub-20: 3.825 kg of lead makes the AUV neutral", auv.NetBuoyancy + 1, 1, 0.01);
            var deep = new SubmarineSim { Salt = true, RatingBar = 1025 * g * 90 / 1e5 };
            deep.ComputeStatic();
            Near("sub-17: a 9.05 bar hull collapses at 90 m in the sea", deep.CrushDepth, 90, 0.1);
            var down = new SubmarineSim { HullL = 20, HullKg = 20, BallastL = 0, ThrustN = 10, Start = 2, Floor = 60, Duration = 120 };
            down.Run();
            Near("sub-12: vertical terminal speed √(2F/(ρ·1.1·L·d))", down.MaxSpeed, Math.Sqrt(2 * 10 / (1000 * 1.1 * down.LengthM * down.DiameterM)), 1);
            var heavy = new SubmarineSim { HullL = 20, HullKg = 20, PayloadKg = 2, BallastL = 0, Pid = true, Kp = 30, Ki = 0, Kd = 120,
                ThrustN = 30, TargetM = 25, Floor = 40, RatingBar = 5, Duration = 400 };
            heavy.Run();
            Near("sub-16: P-only offset with 2 kg extra = 19.62 N / Kp", heavy.FinalDepth, 25 + 2 * g / 30, 0.3);
        }

        log.WriteLine($"{passes} passed, {fails} failed");
        return fails;
    }
}
