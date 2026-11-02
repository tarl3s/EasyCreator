using System.IO;

namespace EasyCreator;

/// <summary>Vehicle labs (car, airplane, helicopter, drone) against hand-computed closed forms, plus every numeric
/// answer of the cars / aircraft / drones projects recomputed here. Runs on the UI thread.</summary>
public static class VehiclesTests
{
    public static int Run(TextWriter log)
    {
        int fails = 0, passes = 0;
        void Check(string name, bool ok, string detail = "")
        {
            if (ok) { passes++; log.WriteLine("  ok   " + name); }
            else { fails++; log.WriteLine($"  FAIL {name} {detail}"); }
        }
        void Near(string name, double got, double want, double tolPct = 0.5)
        {
            bool ok = !double.IsNaN(got) && Math.Abs(got - want) <= Math.Abs(want) * tolPct / 100;
            Check(name, ok, $"(got {got:G7}, want {want:G7})");
        }
        ILab Lab(string id, params (string Key, string Value)[] inputs)
        {
            var lab = LabRegistry.Create(id) ?? throw new InvalidOperationException("lab " + id + " not registered");
            var set = new Dictionary<string, string>();
            foreach (var (k, v) in inputs) set[k] = v;
            lab.Load(new Dictionary<string, string>());
            lab.SetInputs(set);
            lab.RunToEnd();
            return lab;
        }
        // like Lab, but every value goes in as a lesson labParam (so "mode" is applied)
        ILab LabP(string id, params (string Key, string Value)[] ps)
        {
            var lab = LabRegistry.Create(id) ?? throw new InvalidOperationException("lab " + id + " not registered");
            lab.Load(ps.ToDictionary(p => p.Key, p => p.Value));
            lab.RunToEnd();
            return lab;
        }
        double M(ILab l, string k) => l.Metrics.TryGetValue(k, out var v) ? v : double.NaN;
        const double g = 9.81;
        // wheel power (kW) to cruise: air drag + rolling resistance, ρ = 1.2
        static double CarCruiseKw(double m, double cd, double a, double crr, double kmh)
        { double v = kmh / 3.6; return (0.5 * 1.2 * cd * a * v * v + crr * m * 9.81) * v / 1000; }

        try
        {
            log.WriteLine("== Car");
            {
                var c = Lab("car", ("mode", "corner"), ("mu", "0.8"), ("cornerRadius", "100"), ("downforce", "0"), ("track", "1.5"), ("cgHeight", "0.75"));
                Near("corner v = sqrt(mu g r) (mu 0.8, r 100 m)", M(c, "maxCornerSpeed"), Math.Sqrt(0.8 * g * 100) * 3.6);
                Near("rollover limit = track/(2h)", M(c, "rolloverG"), 1.5 / 1.5);
                var b = Lab("car", ("mode", "brake"), ("speed", "100"), ("reaction", "1.5"));
                Near("reaction distance = v t", M(b, "reactionDistance"), 100 / 3.6 * 1.5);
                Check("braking distance >= v^2/(2 mu g) (tyre limit)", M(b, "brakingDistance") >= Math.Pow(100 / 3.6, 2) / (2 * 0.9 * g) * 0.99,
                    $"(got {M(b, "brakingDistance"):0.0})");
            }
            log.WriteLine("== Airplane");
            {
                var a = Lab("airplane", ("mode", "stall"), ("mass", "1000"), ("fuel", "0"), ("wingArea", "15"), ("clmax", "1.6"), ("flaps", "0"),
                    ("aspectRatio", "8"), ("oswald", "0.8"), ("cd0", "0.03"));
                double w = M(a, "wingLoading") * 15 * g;
                Near("stall speed = sqrt(2W/(rho0 S CLmax))", M(a, "stallSpeed"), Math.Sqrt(2 * w / (1.225 * 15 * 1.6)) * 3.6);
                Near("(L/D)max = 1/2 sqrt(pi AR e/CD0)", M(a, "LDmax"), 0.5 * Math.Sqrt(Math.PI * 8 * 0.8 / 0.03));
                Near("approach = 1.3 Vs", M(a, "approachSpeed"), 1.3 * M(a, "stallSpeed"), 0.01);
            }
            log.WriteLine("== Helicopter");
            {
                var h = Lab("helicopter", ("mode", "hover"), ("mass", "1000"), ("rotorRadius", "5"), ("rpm", "400"), ("altitude", "0"));
                double t = 1000 * g, a = Math.PI * 25;
                Near("ideal hover power = T^1.5/sqrt(2 rho A)", M(h, "idealHoverPower"), Math.Pow(t, 1.5) / Math.Sqrt(2 * 1.225 * a) / 1000);
                Near("induced velocity = sqrt(T/(2 rho A))", M(h, "inducedVelocity"), Math.Sqrt(t / (2 * 1.225 * a)));
                Near("tip Mach = Omega R/340", M(h, "tipMach"), 400 * 2 * Math.PI / 60 * 5 / 340);
                Near("disk loading = T/A", M(h, "diskLoading"), t / a);
                Check("real hover power > ideal (figure of merit < 1)", M(h, "hoverPower") > M(h, "idealHoverPower"));
            }
            log.WriteLine("== Drone");
            {
                var d = Lab("drone", ("mode", "hover"), ("frameMass", "600"), ("motors", "4"), ("motorThrust", "500"), ("cells", "4"),
                    ("capacity", "2200"), ("payload", "0"), ("propSize", "5"));
                double m = M(d, "totalMass") / 1000;
                Near("battery energy = S x 3.7 V x Ah", M(d, "batteryWh"), 4 * 3.7 * 2.2);
                Near("T/W = n Tmax/W", M(d, "thrustToWeight"), 4 * 0.5 / m);
                Near("hover throttle = 1/(T/W) (linear model)", M(d, "hoverThrottle"), 100 * m / 2.0);
                double ad = Math.PI * Math.Pow(5 * 0.0254, 2) / 4, tm = m * g / 4;
                double ideal = 4 * Math.Pow(tm, 1.5) / Math.Sqrt(2 * 1.225 * ad);
                Check("hover power above momentum-theory ideal", M(d, "hoverPower") > ideal, $"(got {M(d, "hoverPower"):0}, ideal {ideal:0})");
            }

            log.WriteLine("== Vehicle projects: every numeric answer recomputed");
            double N = double.NaN;
            var want = new Dictionary<string, double[]>
            {
                ["car-01-forcas"] = new[] { 3000 / 1000.0, N, 100 / 3.6 / 4 },
                ["car-02-motor"] = new[] { 200 * 3000 * 2 * Math.PI / 60 / 1000, 120 * 0.7355, N },
                ["car-03-cambio"] = new[] { 150 * 3.5 * 4.0, 2100 / 0.30, N },
                ["car-04-tracao"] = new[] { 1300 * 4 * 0.55 / 2.6, N, 0.9 * g },
                ["car-05-frenagem"] = new[] { 100 / 3.6, 400 / (2 * 0.8 * g), N },
                ["car-06-curvas"] = new[] { Math.Sqrt(0.8 * g * 100), 1.5 / (2 * 0.75), N },
                ["car-07-aerodinamica"] = new[] { 0.5 * 1.2 * 0.30 * 2.2 * 900, 0.5 * 1.2 * 0.30 * 2.2 * 27000 / 1000, N },
                ["car-08-suspensao"] = new[] { Math.Sqrt(30000 / 300.0) / (2 * Math.PI), N, N, 300 * g / 30000 * 100, N },
                ["car-09-eletrico"] = new[] { 60 / 15.0 * 100, 60000 / 160.0, N },
                ["car-10-projeto-fsae"] = new[] { Math.Sqrt(1.4 * g * 15), Math.Sqrt(1.5 * 300 * g / (300 / 15.0 - 1.5 * 1.2 * 3 / 2)), N },
                ["aero-01-sustentacao"] = new[] { 0.5 * 1.2 * 2500 * 10 * 0.5, N, N },
                ["aero-02-angulo-ataque"] = new[] { 1.5 / 0.0866 - 2, N, N },
                ["aero-03-arrasto-polar"] = new[] { 0.25 / (Math.PI * 0.8 * 8), 15.0, N },
                ["aero-04-estol"] = new[] { Math.Sqrt(2 * 10000 / (1.225 * 15 * 1.6)), N, 1.3 * 80 },
                ["aero-05-decolagem"] = new[] { 1.225 * Math.Pow((288.15 - 0.0065 * 2500) / 288.15, 4.256), N, 35 * 35 / 5.0 },
                ["aero-06-estabilidade"] = new[] { 15.0, N, N },
                ["aero-07-propulsao"] = new[] { 100 * 200.0, 2 / (1 + 400 / 200.0), 0.8 * 100000 / 50, N, N },
                ["aero-08-helicoptero-pairar"] = new[] { Math.Pow(10000, 1.5) / Math.Sqrt(2 * 1.225 * Math.PI * 25) / 1000, Math.Sqrt(10000 / (2 * 1.225 * Math.PI * 25)), N },
                ["aero-09-cauda-autorrotacao"] = new[] { 150000 / (400 * 2 * Math.PI / 60) / 5, N, N },
                ["aero-10-projeto-breguet"] = new[] { 0.8 / (g * 0.30 / 3.6e6) * 12 * Math.Log(1.2) / 1000, N, N },
                ["drone-01-partes"] = new[] { 2300 * 11.1, N, N, 4 * 3.7, N },
                ["drone-02-empuxo-peso"] = new[] { 4 * 800 / 1600.0, 50.0, N },
                ["drone-03-bateria"] = new[] { 4 * 3.7 * 2.2, 0.8 * 2.2 / 15 * 60, N },
                ["drone-04-helice"] = new[] { 0.5, N, N },
                ["drone-05-imu-sensores"] = new[] { N, N, 0.98 * (10 + 20 * 0.01) + 0.02 * 12, 0.5 * 60, N },
                ["drone-06-pid-altura"] = new[] { 50 / 10.0, N, N },
                ["drone-07-atitude"] = new[] { N, 1.0, N },
                ["drone-08-gps-navegacao"] = new[] { N, 12 * 300 / 1000.0, N },
                ["drone-09-regras"] = new[] { N, 250.0, 120.0, 30.0, N },
                ["drone-10-projeto-entrega"] = new[] { 800 * 10 / 60.0, 800 * 10 / 60.0 / 0.8, N },
                // ---- lessons 11–20
                ["car-11-velocidade-maxima"] = new[] { 0.5 * 1.2 * 0.30 * 2.2 * Math.Pow(200 / 3.6, 3) / 1000, 6670 * 2 * Math.PI / 60 * 0.30 / (0.85 * 5) * 3.6, N },
                ["car-12-escalonamento"] = new[] { 6000 * 2 * Math.PI / 60 * 0.30 / (0.8 * 3.6) * 3.6, 1000 * 2 * Math.PI / 60 * 0.30 / (0.8 * 3.6) * 3.6,
                    Math.Pow(0.8 / 3.5, 1 / 5.0), 6000 * Math.Pow(0.8 / 3.5, 1 / 5.0), N },
                ["car-13-balanco-freio"] = new[] { 1300 * g * 0.6 + 1300 * 0.8 * g * 0.55 / 2.6, (0.6 + 0.8 * 0.55 / 2.6) * 100, N },
                ["car-14-energia-freios"] = new[] { 0.5 * 1500 * Math.Pow(100 / 3.6, 2) / 1000, 1500 * 0.8 * g * 100 / 3.6 / 1000,
                    0.7 * 0.5 * 1500 * Math.Pow(100 / 3.6, 2) / (2 * 8 * 460), 0.6 * 0.5 * 1500 * Math.Pow(100 / 3.6, 2) / 3.6e6, N },
                ["car-15-downforce"] = new[] { 0.5 * 1.2 * 3 * 900, Math.Sqrt(1.3 * g * 50), Math.Sqrt(1.3 * 700 * g / (700 / 50.0 - 1.3 * 0.5 * 1.2 * 3.2)) },
                ["car-16-suspensao-avancada"] = new[] { 3000 / (2 * Math.Sqrt(30000 * 300.0)), 40000 * 0.7 * 0.7, Math.Sqrt(19600 / 250.0) / (2 * Math.PI), N, N },
                ["car-17-ev-estrada"] = new[] { CarCruiseKw(2100, 0.23, 2.2, 0.012, 110), CarCruiseKw(2100, 0.23, 2.2, 0.012, 110) / 0.88 / 110 * 100, 80 / 0.16 },
                ["car-18-motor-termico"] = new[] { (1 - Math.Pow(10, -0.4)) * 100, 3.6e6 / (0.25 * 43e6) * 100, 15 / (0.30 * 8.9), 6 * 2.3 / 100 * 1000, N },
                ["car-19-acerto-asa"] = new[] { 400 / (130 / 3.6), Math.PI * 60, N },
                ["car-20-projeto-esportivo"] = new[] { 0.5 * 1300 * Math.Pow(100 / 3.6, 2) / 1000, Math.Pow(100 / 3.6, 2) / (2 * 0.9 * g), N },
                ["aero-11-planador"] = new[] { 2000 * 25 / 1000.0, 90 / 3.6 / 30, 0.5 * Math.Sqrt(Math.PI * 0.9 * 16 / 0.016) },
                ["aero-12-curvas-fator-carga"] = new[] { 1 / Math.Cos(Math.PI / 3), 100 * Math.Sqrt(2), 2500 / (g * Math.Tan(Math.PI / 6)),
                    g * Math.Tan(Math.PI / 6) / 50 * 180 / Math.PI, N },
                ["aero-13-teto"] = new[] { 30000 / (1100 * g), 120 * Math.Pow((288.15 - 0.0065 * 6000) / 288.15, 4.256), N },
                ["aero-14-curva-potencia"] = new[] { 0.5 / Math.Sqrt(0.05 * 0.03), 10000 * 2 * Math.Sqrt(0.05 * 0.03),
                    Math.Sqrt(2 * 10000 / (1.225 * 15)) * Math.Pow(0.05 / 0.03, 0.25), Math.Sqrt(2 * 10000 / (1.225 * 15)) * Math.Pow(0.05 / 0.03, 0.25) / Math.Pow(3, 0.25), N },
                ["aero-15-alcance-jato"] = new[] { 200 / (g * 1.8e-5) * 15 * Math.Log(1.3) / 1000, 6000 / 700.0, N },
                ["aero-16-peso-balanceamento"] = new[] { 1540 / 780.0, (1540 / 780.0 - 1.5) / 1.5 * 100, 25 + 40.5 * 0.6 - 40 },
                ["aero-17-helicoptero-altitude"] = new[] { 100 / Math.Sqrt(0.742), 200 * 0.742, 3 * 0.25 / (Math.PI * 2.5) },
                ["aero-18-rotor-avanco"] = new[] { (220 + 70) / 340.0, 150.0, 70 / 220.0, 4 * 0.5 / (Math.PI * 7), N },
                ["aero-19-autorrotacao"] = new[] { Math.Sqrt(2 * 1600 * g / (1.225 * Math.PI * 5.6 * 5.6 * 1.2)),
                    0.5 * 8 * 2 * 0.25 * Math.Pow(5.6, 3) * Math.Pow(370 * 2 * Math.PI / 60, 2) / 1000, (1 - 0.7 * 0.7) * 100 },
                ["aero-20-projeto-mato"] = new[] { 920 / 16.0, Math.Sqrt(2 * 920 * g / (1.225 * 16 * 1.95)) * 3.6, N },
                ["drone-11-lipo"] = new[] { 1.5 * 100, 60 * 4 * 0.005, 60 * 60 * 4 * 0.005, 300 / 14.8, N },
                ["drone-12-bateria-ideal"] = new[] { 4 * 3.7 * 5 / 150 * 1000, Math.Pow(1.3, 1.5), N },
                ["drone-13-motor-kv"] = new[] { 920 * 14.8, 60 / (2 * Math.PI * 920), 4.3 * 0.0254 * 30000 / 60, 1.2 * 1.2, N },
                ["drone-14-carga-pesada"] = new[] { 600 / 22.2, 5300 * 2 / 8.0, N },
                ["drone-15-pid-pesado"] = new[] { 100 / 1.6, 100 / 1.6 / 5, N },
                ["drone-16-fusao-sensores"] = new[] { 0.5 / 0.51, 4 / 5.0, 10 + 0.8 * 2, 1 / Math.Sqrt(100), 2 * 200.0 },
                ["drone-17-atitude-pesado"] = new[] { 0.03 * Math.Pow(15 / 5.0, 1.5), 0.4 * 4 * 0.25, N },
                ["drone-18-velocidade-alcance"] = new[] { 170 * 1000 / 15.0 / 3600, 0.8 * 74 / (170 * 1000 / 15.0 / 3600), N },
                ["drone-19-teoria-rotor"] = new[] { Math.Pow(5, 1.5) / Math.Sqrt(2 * 1.225 * Math.PI * 0.254 * 0.254 / 4),
                    Math.Pow(5, 1.5) / Math.Sqrt(2 * 1.225 * Math.PI * 0.254 * 0.254 / 4) / 0.6, 5 / (Math.PI * 0.254 * 0.254 / 4), 1 / Math.Sqrt(0.742), N },
                ["drone-20-projeto-mapeamento"] = new[] { 180 * 12000 / 15.0 / 3600, 6 * 3.7 * 6, N },
            };
            foreach (var id in new[] { "cars", "aircraft", "drones" })
            {
                var track = ContentStore.Find(id);
                if (track == null) { Check(id + " loaded", false); continue; }
                Check($"{id}: 20 lessons", track.Lessons.Count == 20, $"({track.Lessons.Count})");
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

            log.WriteLine("== Lessons 11–20: answers that the simulators must reproduce");
            {
                var ev = LabP("car", ("mode", "ev"), ("speed", "110"), ("battery", "80"), ("mass", "1600"), ("cd", "0.23"), ("area", "2.2"), ("crr", "0.012"));
                Near("car-17: EV mass = 1600 kg + 80 kWh / 160 Wh/kg", M(ev, "totalMass"), 2100);
                Near("car-17: wheel power at 110 km/h", M(ev, "cruisePower"), CarCruiseKw(2100, 0.23, 2.2, 0.012, 110));
                var ts = LabP("car", ("mode", "topspeed"), ("power", "100"), ("finalDrive", "5"), ("gearTop", "0.85"), ("cd", "0.28"), ("area", "2"));
                Near("car-11: top speed capped at the rev limiter", M(ts, "topSpeed"), 6670 * 2 * Math.PI / 60 * 0.30 / (0.85 * 5) * 3.6);
                var c0 = LabP("car", ("mode", "corner"), ("mass", "700"), ("mu", "1.3"), ("cornerRadius", "50"), ("downforce", "0"));
                Near("car-15: corner speed without wing", M(c0, "maxCornerSpeed") / 3.6, Math.Sqrt(1.3 * g * 50));
                var c3 = LabP("car", ("mode", "corner"), ("mass", "700"), ("mu", "1.3"), ("cornerRadius", "50"), ("downforce", "3.2"));
                Near("car-15: corner speed with ClA 3.2", M(c3, "maxCornerSpeed") / 3.6, Math.Sqrt(1.3 * 700 * g / (700 / 50.0 - 1.3 * 0.5 * 1.2 * 3.2)));
                var gl = LabP("airplane", ("mode", "polar"), ("aspectRatio", "16"), ("oswald", "0.9"), ("cd0", "0.016"));
                Near("aero-11: (L/D)max", M(gl, "LDmax"), 0.5 * Math.Sqrt(Math.PI * 0.9 * 16 / 0.016));
                var wb = LabP("airplane", ("mode", "stability"), ("cg", "40"), ("tailVolume", "0.6"));
                Near("aero-16: static margin", M(wb, "staticMargin"), 9.3);
                var bush = LabP("airplane", ("mode", "takeoff"), ("mass", "700"), ("fuel", "220"), ("wingArea", "16"), ("clmax", "1.5"), ("flaps", "20"));
                Near("aero-20: stall speed with 20° flaps", M(bush, "stallSpeed"), Math.Sqrt(2 * 920 * g / (1.225 * 16 * 1.95)) * 3.6);
                var au = LabP("helicopter", ("mode", "auto"), ("mass", "1600"), ("rotorRadius", "5.6"), ("rpm", "370"), ("blades", "2"), ("chord", "0.25"));
                Near("aero-19: autorotation descent", M(au, "autoDescent"), Math.Sqrt(2 * 1600 * g / (1.225 * Math.PI * 5.6 * 5.6 * 1.2)));
                Near("aero-19: rotor energy", M(au, "rotorEnergy"), 0.5 * 8 * 2 * 0.25 * Math.Pow(5.6, 3) * Math.Pow(370 * 2 * Math.PI / 60, 2) / 1000);
                var bat = LabP("drone", ("mode", "hover"), ("cells", "4"), ("capacity", "5000"));
                Near("drone-12: 4S 5000 mAh battery mass", M(bat, "batteryMass"), 4 * 3.7 * 5 / 150 * 1000);
            }
        }
        catch (Exception ex) { Check("no exception", false, ex.ToString()); }
        log.WriteLine($"Vehicles: {passes} passed, {fails} failed");
        return fails;
    }
}
