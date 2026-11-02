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
        double M(ILab l, string k) => l.Metrics.TryGetValue(k, out var v) ? v : double.NaN;
        const double g = 9.81;

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
            };
            foreach (var id in new[] { "cars", "aircraft", "drones" })
            {
                var track = ContentStore.Find(id);
                if (track == null) { Check(id + " loaded", false); continue; }
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
        catch (Exception ex) { Check("no exception", false, ex.ToString()); }
        log.WriteLine($"Vehicles: {passes} passed, {fails} failed");
        return fails;
    }
}
