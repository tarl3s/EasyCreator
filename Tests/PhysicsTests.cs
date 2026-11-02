using System.IO;
using System.Reflection;

namespace EasyCreator;

/// <summary>Physics labs against textbook values: each lab is created headless (LabRegistry.Create → Load → SetInputs →
/// RunToEnd) and its metrics compared with closed-form results computed here, independently of the lab code
/// (projectile range/height/time, quadratic drag against an independent fine-step integration and the exact vertical
/// solution, incline with static/kinetic friction, spring and damped spring periods, the exact large-amplitude pendulum
/// period (elliptic integral by AGM), collisions with restitution, orbits by vis-viva (ISS, GEO, escape), thin lens,
/// ideal-gas processes). Also recomputes every numeric answer of the physics track. Runs on the UI thread.</summary>
public static class PhysicsTests
{
    public static int Run(TextWriter log)
    {
        int fails = 0, passes = 0;
        void Check(string name, bool ok, string detail = "")
        {
            if (ok) { passes++; log.WriteLine("  ok   " + name); }
            else { fails++; log.WriteLine($"  FAIL {name} {detail}"); }
        }
        void Near(string name, double got, double want, double tolPct = 0.1)
        {
            bool ok = !double.IsNaN(got) && (want == 0 ? Math.Abs(got) < tolPct / 100 : Math.Abs(got - want) <= Math.Abs(want) * tolPct / 100);
            Check(name, ok, $"(got {got:G7}, want {want:G7})");
        }
        ILab Lab(string id, params (string Key, string Value)[] inputs)
        {
            var lab = LabRegistry.Create(id) ?? throw new InvalidOperationException("lab " + id + " not registered");
            var load = new Dictionary<string, string>();
            var set = new Dictionary<string, string>();
            foreach (var (k, v) in inputs) (k is "targetX" or "targetWidth" or "length" ? load : set)[k] = v;
            lab.Load(load);
            lab.SetInputs(set);
            lab.RunToEnd();
            return lab;
        }
        double M(ILab l, string k) => l.Metrics.TryGetValue(k, out var v) ? v : double.NaN;
        const double g = 9.81;
        double Deg(double d) => d * Math.PI / 180;

        try
        {
            // ---------------------------------------------------------------- projectile
            log.WriteLine("== Projectile (vacuum: closed form)");
            {
                var l = Lab("projectile", ("speed", "20"), ("angle", "30"), ("height", "0"));
                Near("range = v²·sin2θ/g (20 m/s, 30°)", M(l, "range"), 400 * Math.Sin(Deg(60)) / g, 0.01);
                Near("max height = (v·sinθ)²/2g", M(l, "maxHeight"), 100 / (2 * g), 0.01);
                Near("flight time = 2·v·sinθ/g", M(l, "flightTime"), 2 * 10 / g, 0.01);
                Near("impact speed = launch speed (same level)", M(l, "impactSpeed"), 20, 0.01);
                var l60 = Lab("projectile", ("speed", "20"), ("angle", "60"));
                Near("complementary angle 60° has the same range", M(l60, "range"), M(l, "range"), 0.01);
                var l45 = Lab("projectile", ("speed", "17.2"), ("angle", "45"));
                Near("45°: range = v²/g (lesson 1: 17,2 m/s → 30,2 m)", M(l45, "range"), 17.2 * 17.2 / g, 0.01);
                var lh = Lab("projectile", ("speed", "10"), ("angle", "0"), ("height", "20"));
                Near("horizontal from 20 m: t = √(2h/g)", M(lh, "flightTime"), Math.Sqrt(40 / g), 0.01);
                Near("horizontal from 20 m: x = v·t", M(lh, "range"), 10 * Math.Sqrt(40 / g), 0.01);
                Near("horizontal from 20 m: impact = √(v² + 2gh)", M(lh, "impactSpeed"), Math.Sqrt(100 + 2 * g * 20), 0.01);
                var lr = Lab("projectile", ("speed", "15"), ("angle", "40"), ("height", "10"));
                double vx = 15 * Math.Cos(Deg(40)), vy = 15 * Math.Sin(Deg(40)), tf = (vy + Math.Sqrt(vy * vy + 2 * g * 10)) / g;
                Near("from 10 m at 40°: range = vx·(vy + √(vy² + 2gh))/g", M(lr, "range"), vx * tf, 0.01);
                Near("from 10 m at 40°: max height = h + vy²/2g", M(lr, "maxHeight"), 10 + vy * vy / (2 * g), 0.01);
                var lm = Lab("projectile", ("speed", "10"), ("angle", "45"), ("planet", "moon"));
                Near("Moon (g = 1,62): range = v²/g", M(lm, "range"), 100 / 1.62, 0.01);
                var lt = Lab("projectile", ("targetX", "30"), ("targetWidth", "2"), ("speed", "17.2"), ("angle", "45"));
                Check("hitTarget = 1 inside the target", M(lt, "hitTarget") == 1);
                var lt2 = Lab("projectile", ("targetX", "30"), ("targetWidth", "2"), ("speed", "16"), ("angle", "45"));
                Check("hitTarget = 0 outside the target", M(lt2, "hitTarget") == 0, $"(range {M(lt2, "range"):0.00})");
            }

            log.WriteLine("== Projectile with quadratic drag (RK4)");
            {
                // Independent reference: classical RK4 on the 4-vector with a 20× smaller step.
                (double range, double time, double top, double impact) Reference(double v0, double deg, double h0, double k)
                {
                    double[] s = { 0, h0, v0 * Math.Cos(Deg(deg)), v0 * Math.Sin(Deg(deg)) };
                    double[] D(double[] q) { double sp = Math.Sqrt(q[2] * q[2] + q[3] * q[3]); return new[] { q[2], q[3], -k * sp * q[2], -g - k * sp * q[3] }; }
                    double[] Add(double[] a, double[] b, double f) => new[] { a[0] + b[0] * f, a[1] + b[1] * f, a[2] + b[2] * f, a[3] + b[3] * f };
                    double dt = 2.5e-5, t = 0, top = h0;
                    while (true)
                    {
                        var k1 = D(s); var k2 = D(Add(s, k1, dt / 2)); var k3 = D(Add(s, k2, dt / 2)); var k4 = D(Add(s, k3, dt));
                        var n = new double[4];
                        for (int i = 0; i < 4; i++) n[i] = s[i] + dt * (k1[i] + 2 * k2[i] + 2 * k3[i] + k4[i]) / 6;
                        if (n[1] < 0)
                        {
                            double f = s[1] / (s[1] - n[1]);
                            var e = Add(s, new[] { n[0] - s[0], n[1] - s[1], n[2] - s[2], n[3] - s[3] }, f);
                            return (e[0], t + f * dt, top, Math.Sqrt(e[2] * e[2] + e[3] * e[3]));
                        }
                        s = n; t += dt; top = Math.Max(top, s[1]);
                    }
                }
                var r = Reference(35.8, 36, 0, 0.03);
                var l = Lab("projectile", ("speed", "35.8"), ("angle", "36"), ("drag", "0.03"));
                Near("k = 0,03, 35,8 m/s, 36°: range vs fine-step reference", M(l, "range"), r.range, 0.05);
                Near("k = 0,03: flight time vs reference", M(l, "flightTime"), r.time, 0.05);
                Near("k = 0,03: max height vs reference", M(l, "maxHeight"), r.top, 0.05);
                Near("k = 0,03: impact speed vs reference", M(l, "impactSpeed"), r.impact, 0.05);
                Check("drag shortens the range (vs vacuum)", M(l, "range") < M(l, "rangeNoDrag") * 0.6, $"({M(l, "range"):0.0} vs {M(l, "rangeNoDrag"):0.0})");
                Check("drag: descent steeper — impact slower than launch", M(l, "impactSpeed") < 35.8);
                var r2 = Reference(20, 60, 15, 0.01);
                var l2 = Lab("projectile", ("speed", "20"), ("angle", "60"), ("height", "15"), ("drag", "0.01"));
                Near("k = 0,01 from 15 m at 60°: range vs reference", M(l2, "range"), r2.range, 0.05);
                // Exact vertical solution with quadratic drag: up to h = ln(1 + k·v0²/g)/(2k), then falls from rest:
                // v = vt·√(1 − e^(−2k·d)), vt = √(g/k).
                double k = 0.05, v0 = 10, hTop = Math.Log(1 + k * v0 * v0 / g) / (2 * k), d = 30 + hTop, vt = Math.Sqrt(g / k);
                var lv = Lab("projectile", ("speed", "10"), ("angle", "90"), ("height", "30"), ("drag", "0.05"));
                Near("vertical throw with drag: max height = h0 + ln(1 + k·v0²/g)/2k", M(lv, "maxHeight"), 30 + hTop, 0.05);
                Near("vertical fall with drag: v = vt·√(1 − e^(−2kd))", M(lv, "impactSpeed"), vt * Math.Sqrt(1 - Math.Exp(-2 * k * d)), 0.05);
                // Lesson 10 (target 40 ± 1 m, speed ≤ 35,3 m/s): 45° can't reach the target, ~36° can; 45° needs ~37 m/s for 40 m.
                var l45 = Lab("projectile", ("speed", "35.3"), ("angle", "45"), ("drag", "0.03"));
                Check("lesson 10: at 45°, 35,3 m/s falls short of 39 m", M(l45, "range") < 39, $"(range {M(l45, "range"):0.00})");
                var l36 = Lab("projectile", ("targetX", "40"), ("speed", "35.3"), ("angle", "36"), ("drag", "0.03"));
                Check("lesson 10: at 36°, 35,3 m/s hits 40 ± 1 m", M(l36, "hitTarget") == 1, $"(range {M(l36, "range"):0.00})");

                Near("lesson 10 hint: 36°, 35,3 m/s lands near 39,7 m", M(l36, "range"), 39.7, 0.5);
                var l45b = Lab("projectile", ("speed", "37"), ("angle", "45"), ("drag", "0.03"));
                Near("lesson 10 hint: at 45° about 37 m/s reaches 40 m", M(l45b, "range"), 40, 1.5);
                double best = 0, bestAngle = 0;
                for (int a = 20; a <= 60; a++) { var la = Lab("projectile", ("speed", "36"), ("angle", a.ToString()), ("drag", "0.03")); if (M(la, "range") > best) { best = M(la, "range"); bestAngle = a; } }
                Check("lesson 10: with air the best angle is below 45°", bestAngle < 45 && bestAngle >= 30, $"(best {bestAngle}°)");
            }

            // ---------------------------------------------------------------- incline
            log.WriteLine("== Incline (Newton's 2nd law, static vs kinetic friction)");
            {
                double N30 = 5 * g * Math.Cos(Deg(30)), P30 = 5 * g * Math.Sin(Deg(30));
                var l = Lab("incline", ("mass", "5"), ("angle", "30"), ("friction", "0.2"), ("force", "0"));
                Near("normal = m·g·cosθ", M(l, "normalForce"), N30, 0.01);
                Near("weight along = m·g·sinθ", M(l, "weightAlong"), P30, 0.01);
                Near("slides down: a = −(m·g·sinθ − μ·m·g·cosθ)/m", M(l, "acceleration"), -(P30 - 0.2 * N30) / 5, 0.01);
                Near("kinetic friction = μ·N", M(l, "frictionForce"), 0.2 * N30, 0.01);
                Near("net force = m·a", M(l, "netForce"), 5 * M(l, "acceleration"), 0.01);
                double a = Math.Abs(M(l, "acceleration"));
                Near("time to the end of a 6 m ramp from the middle: √(2·3/a)", M(l, "time"), Math.Sqrt(6 / a), 0.01);
                Near("final speed = √(2·a·3)", M(l, "finalSpeed"), Math.Sqrt(6 * a), 0.01);
                var up = Lab("incline", ("mass", "5"), ("angle", "30"), ("friction", "0.2"), ("force", "43"));
                Near("pulled up with 43 N: a = (F − P·sinθ − μN)/m (lesson 2)", M(up, "acceleration"), (43 - P30 - 0.2 * N30) / 5, 0.01);
                var st = Lab("incline", ("mass", "5"), ("angle", "30"), ("friction", "0.6"), ("force", "0"));
                Check("tan30° = 0,577 < μ = 0,6: the block stays (static)", M(st, "slides") == 0 && M(st, "acceleration") == 0);
                Near("static friction only balances the weight: Fat = m·g·sinθ", M(st, "frictionForce"), P30, 0.01);
                var edge = Lab("incline", ("mass", "5"), ("angle", "30"), ("friction", "0.55"), ("force", "0"));
                Check("tan30° = 0,577 > μ = 0,55: it slides", M(edge, "slides") == 1);
                var push = Lab("incline", ("mass", "5"), ("angle", "30"), ("friction", "0.2"), ("force", "20"));
                Check("F = 20 N between P·sinθ − μN and P·sinθ + μN: at rest", M(push, "slides") == 0);
                Near("…and static friction = P·sinθ − F", M(push, "frictionForce"), P30 - 20, 0.01);
                var free = Lab("incline", ("mass", "2"), ("angle", "45"), ("friction", "0"), ("force", "0"));
                Near("no friction: a = −g·sinθ (mass independent)", M(free, "acceleration"), -g * Math.Sin(Deg(45)), 0.01);
                var down = Lab("incline", ("mass", "5"), ("angle", "30"), ("friction", "0.2"), ("force", "-50"));
                Near("pushed down: a = (F − P·sinθ + μN)/m", M(down, "acceleration"), (-50 - P30 + 0.2 * N30) / 5, 0.01);
            }

            // ---------------------------------------------------------------- spring
            log.WriteLine("== Spring (Hooke, SHM, damping)");
            {
                var l = Lab("spring", ("mass", "1"), ("k", "100"), ("amplitude", "0.1"), ("damping", "0"));
                Near("T = 2π·√(m/k)", M(l, "period"), 2 * Math.PI * Math.Sqrt(0.01), 0.05);
                Near("ω = √(k/m)", M(l, "omega"), 10, 0.01);
                Near("v_max = A·ω", M(l, "maxSpeed"), 1, 0.1);
                Near("E = k·A²/2", M(l, "energy"), 0.5, 0.01);
                Near("energy conserved without damping (RK4)", M(l, "energyEnd"), 0.5, 0.01);
                var l2 = Lab("spring", ("mass", "4"), ("k", "25"), ("amplitude", "0.3"));
                Near("heavy, soft spring: T = 2π·√(4/25)", M(l2, "period"), 2 * Math.PI * 0.4, 0.05);
                Near("frequency = 1/T", M(l2, "frequency"), 1 / (2 * Math.PI * 0.4), 0.05);
                var sol = Lab("spring", ("mass", "1"), ("k", "200"), ("amplitude", "0.28"));
                Near("lesson 3 solution: v_max = 0,28·√200", M(sol, "maxSpeed"), 0.28 * Math.Sqrt(200), 0.1);
                var d = Lab("spring", ("mass", "1"), ("k", "100"), ("amplitude", "0.1"), ("damping", "0.5"));
                Near("damped: T = 2π/√(k/m − (b/2m)²)", M(d, "period"), 2 * Math.PI / Math.Sqrt(100 - 0.0625), 0.05);
                Check("damped: energy at the end below the start", M(d, "energyEnd") < M(d, "energy") * 0.5, $"({M(d, "energyEnd"):0.000} J)");
            }

            // ---------------------------------------------------------------- pendulum
            log.WriteLine("== Pendulum (non-linear, exact period by elliptic integral)");
            {
                // Exact period: T = T0 / AGM(1, cos(θ0/2)), T0 = 2π√(L/g).
                double Exact(double L, double deg, double gg)
                {
                    double a = 1, b = Math.Cos(Deg(deg) / 2);
                    for (int i = 0; i < 30; i++) (a, b) = ((a + b) / 2, Math.Sqrt(a * b));
                    return 2 * Math.PI * Math.Sqrt(L / gg) / a;
                }
                var s = Lab("pendulum", ("length", "1"), ("amplitude", "2"));
                Near("small angle (2°): T ≈ 2π·√(L/g)", M(s, "period"), 2 * Math.PI * Math.Sqrt(1 / g), 0.05);
                Near("smallAnglePeriod = 2π·√(L/g)", M(s, "smallAnglePeriod"), 2 * Math.PI * Math.Sqrt(1 / g), 0.01);
                foreach (var deg in new[] { 20, 40, 60, 80 })
                {
                    var l = Lab("pendulum", ("length", "2"), ("amplitude", deg.ToString()));
                    Near($"{deg}°: exact period 4·√(L/g)·K(sin θ0/2)", M(l, "period"), Exact(2, deg, g), 0.05);
                }
                var l40 = Lab("pendulum", ("length", "2"), ("amplitude", "40"));
                Near("40°: about 3% longer than small-angle (lesson 6 theory)", M(l40, "period") / M(l40, "smallAnglePeriod"), 1 + Math.Pow(Deg(40), 2) / 16, 0.2);
                Near("energy = m·g·L·(1 − cosθ0)", M(l40, "energy"), 1 * g * 2 * (1 - Math.Cos(Deg(40))), 0.01);
                Near("v_max = √(2·g·L·(1 − cosθ0))", M(l40, "maxSpeed"), Math.Sqrt(2 * g * 2 * (1 - Math.Cos(Deg(40)))), 0.1);
                var heavy = Lab("pendulum", ("length", "2"), ("amplitude", "40"), ("mass", "5"));
                Near("period independent of mass", M(heavy, "period"), M(l40, "period"), 0.001);
                var moon = Lab("pendulum", ("length", "1"), ("amplitude", "3"), ("planet", "moon"));
                Near("Moon: T = 2π·√(1/1,62)", M(moon, "period"), Exact(1, 3, 1.62), 0.05);
                var sec = Lab("pendulum", ("length", "0.99"), ("amplitude", "5"));
                Near("lesson 6: L = 0,99 m beats seconds (T ≈ 2 s)", M(sec, "period"), Exact(0.99, 5, g), 0.05);
                var damp = Lab("pendulum", ("length", "1"), ("amplitude", "10"), ("damping", "0.1"));
                Check("damping lowers the amplitude", M(damp, "amplitudeEnd") < 10 * Math.Exp(-0.1 / 2 * 6) * 1.05, $"(end {M(damp, "amplitudeEnd"):0.00}°)");
                var free = Lab("pendulum", ("length", "1"), ("amplitude", "10"));
                Near("no damping: amplitude kept", M(free, "amplitudeEnd"), 10, 0.1);
            }

            // ---------------------------------------------------------------- collisions
            log.WriteLine("== Collisions (momentum, restitution)");
            {
                var el = Lab("collisions", ("m1", "1"), ("v1", "2"), ("m2", "2"), ("v2", "0"), ("e", "1"));
                Near("elastic: v1' = (m1 − m2)·v1/(m1 + m2)", M(el, "v1After"), -2.0 / 3, 0.01);
                Near("elastic: v2' = 2·m1·v1/(m1 + m2)", M(el, "v2After"), 4.0 / 3, 0.01);
                Near("elastic: kinetic energy kept", M(el, "keLostPct"), 0, 0.01);
                Near("momentum conserved", M(el, "pAfter"), M(el, "pBefore"), 0.001);
                var eq = Lab("collisions", ("m1", "2"), ("v1", "2"), ("m2", "2"), ("v2", "0"), ("e", "1"));
                Check("equal masses, elastic: velocities swap (lesson 4)", Math.Abs(M(eq, "v1After")) < 1e-9 && Math.Abs(M(eq, "v2After") - 2) < 1e-9);
                var st = Lab("collisions", ("m1", "2"), ("v1", "3"), ("m2", "1"), ("v2", "0"), ("e", "0"));
                Near("perfectly inelastic: common velocity = p/(m1 + m2)", M(st, "v1After"), 2, 0.01);
                Near("perfectly inelastic: both move together", M(st, "v2After"), 2, 0.01);
                Near("perfectly inelastic: 3 J lost (lesson 4 question)", M(st, "keLost"), 3, 0.01);
                Near("perfectly inelastic: lost = m2/(m1 + m2) of the KE", M(st, "keLostPct"), 100.0 / 3, 0.01);
                var half = Lab("collisions", ("m1", "1"), ("v1", "4"), ("m2", "1"), ("v2", "-2"), ("e", "0.5"));
                double e = (M(half, "v2After") - M(half, "v1After")) / (4 - (-2));
                Near("e = 0,5: (v2' − v1')/(v1 − v2) = e", e, 0.5, 0.01);
                Near("e = 0,5: momentum conserved", M(half, "pAfter"), 2, 0.01);
                Near("e = 0,5: KE lost = μ·(1 − e²)·Δv²/2", M(half, "keLost"), 0.5 * 0.5 * (1 - 0.25) * 36, 0.01);
                var miss = Lab("collisions", ("m1", "1"), ("v1", "1"), ("m2", "1"), ("v2", "2"));
                Check("cart 1 slower than cart 2: no collision", M(miss, "collided") == 0 && M(miss, "v1After") == 1);
                var back = Lab("collisions", ("m1", "1"), ("v1", "-1"), ("m2", "1"), ("v2", "-3"));
                Check("both moving left, 2 faster: they collide", M(back, "collided") == 1);
            }

            // ---------------------------------------------------------------- orbits
            log.WriteLine("== Orbits (vis-viva, Kepler, escape)");
            {
                const double mu = 398600.4, R = 6371;
                double r = R + 408, vc = Math.Sqrt(mu / r);
                var iss = Lab("orbit", ("altitude", "408"), ("speed", vc.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)));
                Near("ISS at 408 km: period ≈ 92,7 min", M(iss, "period"), 92.7, 0.5);
                Near("ISS: period = 2π·√(r³/GM)", M(iss, "period"), 2 * Math.PI * Math.Sqrt(r * r * r / mu) / 60, 0.1);
                Check("ISS: closed, nearly circular", M(iss, "orbitType") == 1 && M(iss, "eccentricity") < 0.002, $"(e {M(iss, "eccentricity"):0.0000})");
                Near("circular speed √(GM/r)", M(iss, "circularSpeed"), vc, 0.01);
                Near("escape speed = √2 · circular", M(iss, "escapeSpeed"), Math.Sqrt(2) * vc, 0.01);
                double vGeo = Math.Sqrt(mu / (R + 35786));
                var geo = Lab("orbit", ("altitude", "35786"), ("speed", vGeo.ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture)));
                Near("GEO (35 786 km, circular): period ≈ 1 sidereal day (1436 min)", M(geo, "period"), 1436, 0.3);
                // vis-viva: v² = GM·(2/r − 1/a); launched horizontally, r is a periapsis (v > vc) or apoapsis (v < vc).
                double v = 1.2 * vc, a = 1 / (2 / r - v * v / mu);
                var ell = Lab("orbit", ("altitude", "408"), ("speed", v.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture)));
                double vUsed = Math.Round(v, 3); a = 1 / (2 / r - vUsed * vUsed / mu);
                Near("1,2·vc: eccentricity = r·v²/GM − 1", M(ell, "eccentricity"), r * vUsed * vUsed / mu - 1, 0.1);
                Near("1,2·vc: perigee at the launch altitude", M(ell, "periapsisAlt"), 408, 0.1);
                Near("1,2·vc: apogee = 2a − r (vis-viva)", M(ell, "apoapsisAlt"), 2 * a - r - R, 0.1);
                Near("1,2·vc: period = 2π·√(a³/GM)", M(ell, "period"), 2 * Math.PI * Math.Sqrt(a * a * a / mu) / 60, 0.1);
                var slow = Lab("orbit", ("altitude", "408"), ("speed", "7.6"));
                double as_ = 1 / (2 / r - 7.6 * 7.6 / mu);
                Near("7,6 km/s: launch point is the apogee", M(slow, "apoapsisAlt"), 408, 0.1);
                Near("7,6 km/s: perigee = 2a − r", M(slow, "periapsisAlt"), 2 * as_ - r - R, 0.2);
                Check("7,6 km/s stays above the ground (closed)", M(slow, "orbitType") == 1);
                var fall = Lab("orbit", ("altitude", "408"), ("speed", "5"));
                Check("5 km/s at 408 km: falls on the planet", M(fall, "orbitType") == 0);
                var esc = Lab("orbit", ("altitude", "408"), ("speed", "11.00"));
                Check("11 km/s (> v_esc 10,85): escapes", M(esc, "orbitType") == 2 && double.IsInfinity(M(esc, "period")));
                var almost = Lab("orbit", ("altitude", "408"), ("speed", "10.80"));
                Check("10,8 km/s (< v_esc): still bound", M(almost, "orbitType") == 1 && M(almost, "eccentricity") < 1);
                var moon = Lab("orbit", ("altitude", "100"), ("speed", "1.63"), ("planet", "moon"));
                Near("Moon, 100 km: circular speed √(4902,8/1837,4)", M(moon, "circularSpeed"), Math.Sqrt(4902.8 / 1837.4), 0.01);
                // The drawn path (RK4) must close on itself after one period.
                var path = (System.Collections.IList?)typeof(OrbitLab).GetField("_s", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(ell);
                if (path != null && path.Count > 1)
                {
                    var last = ((double t, double x, double y))path[path.Count - 1]!;
                    double dx = last.x - r, dy = last.y;
                    Check("RK4 path closes after one period (error < 0,1% of r)", Math.Sqrt(dx * dx + dy * dy) < r * 1e-3, $"(gap {Math.Sqrt(dx * dx + dy * dy):0.00} km)");
                }
                else Check("orbit path readable", false);
            }

            // ---------------------------------------------------------------- lens
            log.WriteLine("== Thin lens (1/f = 1/do + 1/di, m = −di/do)");
            {
                foreach (var (kind, f, d) in new[] { ("converging", 10.0, 30.0), ("converging", 10, 15), ("converging", 10, 5), ("converging", 20, 60), ("diverging", 10, 30), ("diverging", 15, 6) })
                {
                    var l = Lab("lens", ("kind", kind), ("focal", f.ToString()), ("objDist", d.ToString()), ("objHeight", "2"));
                    double fs = kind == "diverging" ? -f : f, di = 1 / (1 / fs - 1 / d), m = -di / d;
                    Near($"{kind} f={f}, do={d}: di", M(l, "imageDistance"), di, 0.01);
                    Near($"{kind} f={f}, do={d}: m", M(l, "magnification"), m, 0.01);
                    Near($"{kind} f={f}, do={d}: 1/do + 1/di = 1/f", 1 / d + 1 / M(l, "imageDistance"), 1 / fs, 0.01);
                    Check($"{kind} f={f}, do={d}: real={(di > 0 ? 1 : 0)} inverted={(m < 0 ? 1 : 0)}", M(l, "real") == (di > 0 ? 1 : 0) && M(l, "inverted") == (m < 0 ? 1 : 0));
                    Near($"{kind} f={f}, do={d}: image height = m·h", M(l, "imageHeight"), 2 * m, 0.01);
                }
                var foc = Lab("lens", ("focal", "10"), ("objDist", "10"));
                Check("object at the focus: image at infinity, not real", double.IsInfinity(M(foc, "imageDistance")) && M(foc, "real") == 0);
            }

            // ---------------------------------------------------------------- gas
            log.WriteLine("== Ideal gas (pV = nRT, first law, γ = 5/3)");
            {
                const double Rg = 8.314;
                double nrt = Rg * 300, p1 = nrt / 0.010;
                var iso = Lab("gas", ("process", "isothermal"), ("moles", "1"), ("T1", "300"), ("V1", "10"), ("V2", "20"));
                Near("p1 = nRT/V (kPa)", M(iso, "p1"), p1 / 1000, 0.01);
                Near("isothermal: W = nRT·ln(V2/V1)", M(iso, "work"), nrt * Math.Log(2), 0.01);
                Near("isothermal: ΔU = 0", M(iso, "deltaU"), 0, 0.01);
                Near("isothermal: Q = W", M(iso, "heat"), nrt * Math.Log(2), 0.01);
                Near("isothermal: p2 = p1·V1/V2 (Boyle)", M(iso, "p2"), p1 / 2000, 0.01);
                var bar = Lab("gas", ("process", "isobaric"), ("moles", "1"), ("T1", "300"), ("V1", "10"), ("V2", "20"));
                Near("isobaric: W = p·ΔV", M(bar, "work"), p1 * 0.010, 0.01);
                Near("isobaric: T2 = T1·V2/V1 (Charles)", M(bar, "T2"), 600, 0.01);
                Near("isobaric: ΔU = (3/2)·nR·ΔT", M(bar, "deltaU"), 1.5 * Rg * 300, 0.01);
                Near("isobaric: Q = (5/2)·nR·ΔT", M(bar, "heat"), 2.5 * Rg * 300, 0.01);
                var ad = Lab("gas", ("process", "adiabatic"), ("moles", "1"), ("T1", "300"), ("V1", "10"), ("V2", "20"));
                double t2 = 300 * Math.Pow(0.5, 2.0 / 3), p2 = p1 * Math.Pow(0.5, 5.0 / 3);
                Near("adiabatic: T·V^(γ−1) constant", M(ad, "T2"), t2, 0.01);
                Near("adiabatic: p·V^γ constant", M(ad, "p2"), p2 / 1000, 0.01);
                Near("adiabatic: Q = 0", M(ad, "heat"), 0, 0.01);
                Near("adiabatic: W = (p1V1 − p2V2)/(γ − 1)", M(ad, "work"), (p1 * 0.010 - p2 * 0.020) / (2.0 / 3), 0.01);
                Near("adiabatic: ΔU = −W", M(ad, "deltaU"), -M(ad, "work"), 0.01);
                var comp = Lab("gas", ("process", "adiabatic"), ("moles", "2"), ("T1", "300"), ("V1", "20"), ("V2", "10"));
                Check("adiabatic compression heats the gas and needs work (W < 0)", M(comp, "T2") > 300 && M(comp, "work") < 0);
                var sol = Lab("gas", ("process", "isothermal"), ("moles", "1"), ("T1", "300"), ("V1", "10"), ("V2", "15"));
                Near("lesson 8 solution: W = 2494·ln 1,5 ≈ 1011 J", M(sol, "work"), nrt * Math.Log(1.5), 0.01);
            }

            // ---------------------------------------------------------------- lesson answers
            log.WriteLine("== Physics track: every numeric answer recomputed");
            {
                var track = ContentStore.Find("physics");
                if (track == null) Check("physics track loaded", false);
                else
                {
                    var want = new Dictionary<string, double[]>
                    {
                        ["fis-01-cinematica"] = new[] { Math.Sqrt(2 * 20 / 9.8), double.NaN, 3.0 * 4 },
                        ["fis-02-newton"] = new[] { 5 * 9.8 * Math.Cos(Deg(30)), double.NaN, Math.Atan(0.5) * 180 / Math.PI },
                        ["fis-03-energia"] = new[] { 400 * 0.1 * 0.1 / 2, Math.Sqrt(2 * 9.8 * 5), double.NaN },
                        ["fis-04-colisoes"] = new[] { 2.0 * 3 / 3, 2 * 9 / 2.0 - 3 * 4 / 2.0, double.NaN },
                        ["fis-05-orbitas"] = new[] { Math.Sqrt(4903 / 1737.0), double.NaN, 400 / 50.0 },
                        ["fis-06-pendulo"] = new[] { 2 * Math.PI * Math.Sqrt(1 / 1.62), double.NaN, 2 * Math.PI * Math.Sqrt(0.5 / 200) },
                        ["fis-07-optica"] = new[] { 340 / 680.0, 1 / (1 / 20.0 - 1 / 60.0), double.NaN },
                        ["fis-08-termodinamica"] = new[] { 8.314 * 300 / 0.0249 / 1000, (1 - 300 / 600.0) * 100, double.NaN },
                        ["fis-09-eletromagnetismo"] = new[] { 9e9 * 2e-6 * 2e-6 / 0.09, 12 / 4.0, 4 * 9.0, double.NaN, 1.67e-27 * 2e6 / (1.6e-19 * 0.5) * 100, 5e4 / 0.1, double.NaN },
                        ["fis-10-arrasto-numerico"] = new[] { Math.Sqrt(9.81 / 0.03), double.NaN, 1 / Math.Sqrt(1 - 0.36) },
                    };
                    foreach (var l in track.Lessons)
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
            }
        }
        catch (Exception ex) { Check("crash", false, ex.ToString()); }
        log.WriteLine($"== {passes} passed, {fails} failed");
        return fails;
    }
}
