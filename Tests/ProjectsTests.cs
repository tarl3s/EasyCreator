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

        log.WriteLine($"{passes} passed, {fails} failed");
        return fails;
    }
}
