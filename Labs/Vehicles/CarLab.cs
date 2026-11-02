using System.Windows;
using System.Windows.Media;

namespace EasyCreator.Vehicles;

/// <summary>Car longitudinal + simple lateral dynamics.
/// Assumptions (stated to the student in the lessons):
/// - Engine power curve P(x) = Pmax·(x + x² − x³), x = rpm / rpm of peak power (classic petrol-engine fit), so the
///   torque is T = (Pmax/ωpk)·(1 + x − x²): peak torque 1,25·Pmax/ωpk at half the peak-power rpm; redline 1,15·rpm_pk.
/// - The driver always uses the gear that gives the most force (gear ratios in geometric progression), 0,2 s per shift;
///   launch with the clutch slipping at the max-torque rpm. Drivetrain efficiency 90 % (EV: 95 % reduction gear).
/// - Traction limit = μ × load on the driven axle, with longitudinal weight transfer m·a·h/L; spinning tyres only
///   transmit 85 % of it (kinetic friction). Downforce = ½ρ·ClA·v², split like the static weight; wings add drag ClA/3.
/// - Braking: total brake force split by the bias; an axle locks when its demand passes μ·N (locked tyre: 75 % of μ);
///   ABS keeps each axle at 95 % of its limit. Cornering: v = √(μ·g·r) (with downforce solved for v); the car tips
///   over before sliding when μ &gt; track / (2·h).
/// - Fuel: petrol 8,9 kWh/L; EV battery pack 160 Wh/kg added to the mass; motor max 16 000 rpm; air 1,2 kg/m³.</summary>
[Lab("car")]
public sealed class CarLab : VehicleLab
{
    const double Rho = 1.2, ShiftTime = 0.2, PetrolKWhPerL = 8.9, PackKWhPerKg = 0.16;

    public CarLab()
    {
        Num("mass", "Massa", "kg", 200, 2500, 10, 1300);
        Num("power", "Potência máxima", "kW", 10, 400, 1, 80);
        Num("peakRpm", "Rpm da potência máx.", "rpm", 3000, 12000, 100, 5800);
        Num("gears", "Nº de marchas", "", 1, 8, 1, 5);
        Num("gear1", "Relação da 1ª", "", 1.5, 5.0, 0.05, 3.4);
        Num("gearTop", "Relação da última", "", 0.5, 1.5, 0.05, 0.85);
        Num("finalDrive", "Diferencial", "", 2.0, 12.0, 0.05, 4.0);
        Num("wheelRadius", "Raio da roda", "m", 0.2, 0.4, 0.005, 0.30);
        Num("cd", "Coef. de arrasto Cd", "", 0.15, 1.2, 0.01, 0.33);
        Num("area", "Área frontal", "m²", 0.8, 3.0, 0.05, 2.1);
        Num("crr", "Resist. ao rolamento", "", 0.005, 0.03, 0.001, 0.012);
        Num("mu", "Atrito do pneu μ", "", 0.3, 1.8, 0.05, 0.9);
        Choice("drive", "Tração", new[] { "FWD", "RWD", "AWD" }, new[] { "Dianteira (FWD)", "Traseira (RWD)", "Integral (AWD)" });
        Num("weightFront", "Peso na frente", "%", 30, 70, 1, 60);
        Num("cgHeight", "Altura do CG", "m", 0.2, 1.2, 0.01, 0.55);
        Num("wheelbase", "Entre-eixos", "m", 1.5, 3.2, 0.05, 2.6);
        Num("track", "Bitola", "m", 0.8, 1.9, 0.05, 1.5);
        Num("brakeForce", "Força de freio", "kN", 2, 30, 0.5, 8);
        Num("brakeBias", "Freio na dianteira", "%", 30, 95, 1, 70);
        Choice("abs", "ABS", new[] { "off", "on" }, new[] { "Desligado", "Ligado" });
        Num("reaction", "Tempo de reação", "s", 0, 2.5, 0.1, 0);
        Num("speed", "Velocidade", "km/h", 10, 250, 5, 100);
        Num("cornerRadius", "Raio da curva", "m", 5, 300, 1, 50);
        Num("downforce", "Asa: ClA (downforce)", "m²", 0, 5, 0.1, 0);
        Num("engineEff", "Eficiência do motor", "%", 15, 45, 1, 25);
        Num("tank", "Tanque", "L", 5, 100, 1, 50);
        Num("battery", "Bateria", "kWh", 5, 120, 1, 40);
        Num("motorEff", "Eficiência bateria→roda", "%", 60, 97, 1, 85);
        Num("motorTorque", "Torque do motor elétrico", "N·m", 50, 1000, 10, 250);
        Num("aux", "Consumo auxiliar", "kW", 0, 5, 0.1, 0.5);
        Num("straight", "Reta", "m", 20, 400, 5, 75);
    }

    protected override string Title => Mode switch
    {
        "topspeed" => "🚗 Velocidade máxima",
        "brake" => "🚗 Frenagem",
        "corner" => "🚗 Curva (vista de cima)",
        "consumption" => "🚗 Consumo na estrada",
        "ev" => "🔋 Carro elétrico: aceleração e autonomia",
        "lap" => "🏁 Tempo de volta (pista oval)",
        _ => "🚗 Aceleração 0–100 km/h",
    };

    protected override string[] DefaultShown(string mode) => mode switch
    {
        "topspeed" => new[] { "power", "cd", "area", "mass", "gearTop", "finalDrive" },
        "brake" => new[] { "speed", "brakeForce", "brakeBias", "abs", "mu", "reaction" },
        "corner" => new[] { "speed", "cornerRadius", "mu", "cgHeight", "track", "downforce" },
        "consumption" => new[] { "speed", "cd", "area", "mass", "crr", "engineEff" },
        "ev" => new[] { "battery", "mass", "motorEff", "power", "motorTorque", "finalDrive", "speed", "cd", "area" },
        "lap" => new[] { "mass", "power", "downforce", "mu", "gear1", "finalDrive" },
        _ => new[] { "mass", "power", "peakRpm", "gear1", "finalDrive", "drive" },
    };

    protected override void OnMode()
    {
        if (Mode == "ev") { P("mass").Label = "Massa sem bateria"; P("power").Label = "Potência do motor"; P("finalDrive").Label = "Redução"; P("finalDrive").Value = 9; }
        if (Mode == "brake") P("speed").Label = "Velocidade inicial";
        if (Mode == "consumption") P("speed").Label = "Velocidade de cruzeiro";
    }

    // ---- vehicle state for the current inputs ----------------------------------------------------------------

    bool _ev;
    double _m, _pmax, _wpk, _rpk, _rw, _cdA, _clA, _crr, _mu, _h, _l, _wf, _fd, _tm;
    double[] _ratios = { 1 };
    string _drive = "FWD";

    void Setup()
    {
        _ev = Mode == "ev";
        _m = V("mass") + (_ev ? V("battery") / PackKWhPerKg : 0);
        _pmax = V("power") * 1000;
        _rpk = V("peakRpm");
        _wpk = _rpk * 2 * Math.PI / 60;
        _rw = V("wheelRadius");
        _clA = V("downforce");
        _cdA = V("cd") * V("area") + _clA / 3;
        _crr = V("crr");
        _mu = V("mu");
        _h = V("cgHeight");
        _l = V("wheelbase");
        _wf = V("weightFront") / 100;
        _fd = V("finalDrive");
        _tm = V("motorTorque");
        _drive = C("drive");
        int n = Math.Max(1, (int)Math.Round(V("gears")));
        double g1 = V("gear1"), gt = Math.Min(V("gearTop"), g1);
        _ratios = new double[n];
        for (int k = 0; k < n; k++) _ratios[k] = n == 1 ? g1 : g1 * Math.Pow(gt / g1, (double)k / (n - 1));
    }

    double EngineTorque(double rpm)
    {
        double x = rpm / _rpk;
        return x > 1.15 ? 0 : _pmax / _wpk * (1 + x - x * x);
    }

    double RpmOf(double v, int gear) => v / _rw * _ratios[gear] * _fd * 60 / (2 * Math.PI);

    /// <summary>Most force the powertrain can put at the wheels at speed v (best gear), before the tyre limit.</summary>
    double PowerForce(double v, out int gear)
    {
        gear = 0;
        if (_ev)
        {
            double motorRpm = v / _rw * _fd * 60 / (2 * Math.PI);
            if (motorRpm > 16000) return 0;
            return Math.Min(_tm * _fd * 0.95 / _rw, _pmax * 0.95 / Math.Max(v, 0.1));
        }
        double best = 0;
        for (int k = 0; k < _ratios.Length; k++)
        {
            double rpm = RpmOf(v, k);
            if (rpm > 1.15 * _rpk) continue;
            double rpmE = Math.Max(rpm, k == 0 ? 0.5 * _rpk : 1000);
            double f = EngineTorque(rpmE) * _ratios[k] * _fd * 0.9 / _rw;
            if (f > best) { best = f; gear = k; }
        }
        return best;
    }

    double Downforce(double v) => 0.5 * Rho * _clA * v * v;
    double Resist(double v) => 0.5 * Rho * _cdA * v * v + _crr * (_m * G + Downforce(v));

    /// <summary>Force the driven tyres can transmit (with longitudinal weight transfer).</summary>
    double TractionLimit(double v, double resist)
    {
        double wt = _m * G + Downforce(v), hl = _h / _l;
        switch (_drive)
        {
            case "AWD": return _mu * wt;
            case "RWD":
                double den = Math.Max(0.05, 1 - _mu * hl);
                return Math.Max(0, _mu * (wt * (1 - _wf) - hl * resist) / den);
            default:
                return _mu * (wt * _wf + hl * resist) / (1 + _mu * hl);
        }
    }

    /// <summary>Largest steady speed on a curve of radius r (tyre limit, with downforce), m/s.</summary>
    double CornerSpeed(double r)
    {
        double den = _m / r - _mu * 0.5 * Rho * _clA;
        return den <= 0 ? 120 : Math.Sqrt(_mu * _m * G / den);
    }

    // ---- simulations ----------------------------------------------------------------------------------------

    readonly List<double[]> _accel = new(), _brake = new(), _corner = new(), _lap = new(), _cruise = new();
    readonly List<Point> _lapProfile = new(), _curve = new();
    double _t100, _top, _stop, _brakeDist, _vmaxCorner, _lapTime, _consumption, _range, _lapLen, _lapR, _lapStraight;
    bool _rollRisk;

    protected override void Compute()
    {
        Setup();
        SimAccel();
        _top = TopSpeed();
        SimBrake();
        SimCorner();
        Cruise();
        SimLap();

        Calc["peakTorque"] = _ev ? _tm : 1.25 * _pmax / _wpk;
        Calc["topSpeed"] = _top * 3.6;
        Calc["maxCornerSpeed"] = _vmaxCorner * 3.6;
        Calc["totalMass"] = _m;

        switch (Mode)
        {
            case "topspeed": Duration = Math.Min(90, _accel[^1][0]); RealSeconds = 7; break;
            case "brake": Duration = _brake[^1][0]; RealSeconds = Math.Clamp(Duration * 1.2, 3, 8); break;
            case "corner": Duration = _corner[^1][0]; RealSeconds = 5; break;
            case "consumption": Duration = _cruise[^1][0]; RealSeconds = 5; break;
            case "lap": Duration = _lapTime; RealSeconds = Math.Clamp(_lapTime / 2.5, 4, 9); break;
            default:
                Duration = Math.Min(_accel[^1][0], (_t100 < 90 ? _t100 : 60) + 1.5);
                RealSeconds = Math.Clamp(Duration / 1.5, 3, 8);
                break;
        }
    }

    void SimAccel()
    {
        _accel.Clear();
        double v = 0, x = 0, t = 0, shift = 0, spin = 0, peakA = 0, dt = 0.005, nextRec = 0, d100 = 0;
        int gear = 0;
        _t100 = 99;
        double tMax = Mode == "topspeed" ? 90 : 60;
        while (t <= tMax + 1e-9)
        {
            double r = Resist(v);
            double fp = PowerForce(v, out int best);
            if (!_ev && best > gear) { gear = best; shift = ShiftTime; }
            if (shift > 0) { fp = 0; shift -= dt; }
            double lim = TractionLimit(v, r);
            bool spinning = fp > lim * 1.001;
            double f = spinning ? 0.85 * lim : fp;
            if (spinning) spin += dt;
            double a = (f - r) / _m;
            if (v <= 0 && a < 0) a = 0;
            peakA = Math.Max(peakA, a);
            if (t >= nextRec - 1e-9)
            {
                double rpm = _ev ? v / _rw * _fd * 60 / (2 * Math.PI) : Math.Max(RpmOf(v, gear), gear == 0 ? 0.5 * _rpk : 1000);
                if (shift > 0) rpm *= 0.8;
                _accel.Add(new[] { t, x, v, rpm, gear + 1, spinning ? 1 : 0, a });
                nextRec += 0.05;
            }
            if (_t100 >= 99 && v >= 100 / 3.6) { _t100 = t; d100 = x; }
            // after 0-100 the "accel" run only needs a little more for the animation
            if (Mode != "topspeed" && _t100 < 99 && t > _t100 + 2) break;
            v += a * dt;
            x += v * dt;
            t += dt;
        }
        Calc["t0to100"] = _t100;
        Calc["distance0to100"] = _t100 < 99 ? d100 : 0;
        Calc["wheelspin"] = Math.Round(spin, 3);
        Calc["peakAccelG"] = peakA / G;
        Calc["reached100"] = _t100 < 99 ? 1 : 0;
    }

    double TopSpeed()
    {
        double v = 1;
        for (; v < 120; v += 0.05)
            if (PowerForce(v, out _) < Resist(v)) break;
        return v;
    }

    void SimBrake()
    {
        _brake.Clear();
        double v0 = V("speed") / 3.6, v = v0, x = 0, t = 0, dt = 0.002, a = 0, nextRec = 0;
        double bf = V("brakeForce") * 1000, bias = V("brakeBias") / 100, react = V("reaction"), hl = _h / _l;
        bool abs = C("abs") == "on", fLock = false, rLock = false;
        double xBrakeStart = 0, sumA = 0, nA = 0;
        while (v > 0 && t < 60)
        {
            double r = Resist(v), ff = 0, fr = 0;
            bool braking = t >= react;
            bool fl = false, rl = false;
            if (braking)
            {
                if (xBrakeStart == 0 && x > 0 && react > 0 && nA == 0) xBrakeStart = x;
                double wt = _m * G + Downforce(v);
                for (int it = 0; it < 4; it++)
                {
                    double nf = Math.Max(0, wt * _wf + _m * a * hl), nr = Math.Max(0, wt * (1 - _wf) - _m * a * hl);
                    double df = bf * bias, dr = bf * (1 - bias);
                    if (abs) { ff = Math.Min(df, 0.95 * _mu * nf); fr = Math.Min(dr, 0.95 * _mu * nr); fl = rl = false; }
                    else
                    {
                        fl = df > _mu * nf; rl = dr > _mu * nr;
                        ff = fl ? 0.75 * _mu * nf : df;
                        fr = rl ? 0.75 * _mu * nr : dr;
                    }
                    a = (ff + fr + r) / _m;
                }
                fLock |= fl; rLock |= rl;
                sumA += a; nA++;
            }
            else a = r / _m;
            if (t >= nextRec - 1e-9) { _brake.Add(new[] { t, x, v, fl ? 1 : 0, rl ? 1 : 0 }); nextRec += 0.02; }
            v -= a * dt;
            x += Math.Max(0, v) * dt;
            t += dt;
        }
        _brake.Add(new[] { t, x, 0.0, _brake.Count > 0 ? _brake[^1][3] : 0, _brake.Count > 0 ? _brake[^1][4] : 0 });
        double reactDist = v0 * react;
        _stop = x;
        _brakeDist = Math.Max(0, x - reactDist);
        Calc["brakingDistance"] = _brakeDist;
        Calc["stoppingDistance"] = _stop;
        Calc["reactionDistance"] = reactDist;
        Calc["frontLock"] = fLock ? 1 : 0;
        Calc["rearLock"] = rLock ? 1 : 0;
        Calc["brakeDecelG"] = nA > 0 ? sumA / nA / G : 0;
        Calc["stopTime"] = t;
    }

    void SimCorner()
    {
        _corner.Clear();
        double r = V("cornerRadius"), v = V("speed") / 3.6;
        _vmaxCorner = Math.Min(CornerSpeed(r), _top);
        double aLat = v * v / r, aMax = _mu * (G + Downforce(v) / _m);
        double tipG = V("track") / (2 * _h);
        _rollRisk = _mu > tipG;
        bool ok = v <= CornerSpeed(r) + 1e-9;
        bool rolls = _rollRisk && aLat > tipG * G;
        Calc["lateralG"] = aLat / G;
        Calc["cornerOk"] = ok && !rolls ? 1 : 0;
        Calc["rollover"] = _rollRisk ? 1 : 0;
        Calc["rolloverG"] = tipG;

        // Trace: angle travelled and the actual path radius (grows when the tyres can't hold the car).
        double ang = 0, rad = r, t = 0, dt = 0.02, dur = Math.Max(4, 2.2 * Math.PI * r / Math.Max(v, 1));
        dur = Math.Min(dur, 30);
        while (t <= dur)
        {
            _corner.Add(new[] { t, ang, rad, v, rolls && rad > r + 2 ? 1 : 0 });
            if (!ok || rolls)
            {
                double target = v * v / Math.Max(0.1, rolls ? tipG * G : aMax);
                rad += (target - rad) * Math.Min(1, dt * 1.5);
            }
            ang += v / rad * dt;
            t += dt;
        }
    }

    double CruisePower(double v) => Resist(v) * v;

    void Cruise()
    {
        _cruise.Clear();
        double v = V("speed") / 3.6, aux = V("aux") * 1000;
        double cons, rng, fuel;
        if (_ev)
        {
            double pBatt = CruisePower(v) / (V("motorEff") / 100) + aux;
            cons = pBatt / v * 100000 / 3.6e6;           // kWh/100 km
            fuel = V("battery");
            rng = cons > 0 ? fuel / cons * 100 : 0;
        }
        else
        {
            double pFuel = CruisePower(v) / 0.9 / (V("engineEff") / 100) + aux / 0.5;
            cons = pFuel / v * 100000 / 3.6e6 / PetrolKWhPerL; // L/100 km
            fuel = V("tank");
            rng = cons > 0 ? fuel / cons * 100 : 0;
        }
        if (v > _top + 0.01) { rng = 0; }
        _consumption = cons;
        _range = rng;
        Calc["consumption"] = cons;
        Calc["range"] = rng;
        Calc["cruisePower"] = CruisePower(v) / 1000;
        Calc["canCruise"] = v <= _top + 0.01 ? 1 : 0;

        // a 100 km trip (or the whole range, if shorter)
        double trip = Math.Max(1000, Math.Min(100000, rng * 1000));
        for (int i = 0; i <= 100; i++)
        {
            double d = trip * i / 100;
            _cruise.Add(new[] { d / Math.Max(v, 0.1), d, v, rng > 0 ? 1 - d / (rng * 1000) : 0 });
        }

        _curve.Clear();
        for (double kmh = 20; kmh <= 200; kmh += 5)
        {
            double vv = kmh / 3.6, y;
            if (_ev) y = (CruisePower(vv) / (V("motorEff") / 100) + aux) / vv * 100000 / 3.6e6;
            else y = (CruisePower(vv) / 0.9 / (V("engineEff") / 100) + aux / 0.5) / vv * 100000 / 3.6e6 / PetrolKWhPerL;
            _curve.Add(new Point(kmh, _ev ? (y > 0 ? V("battery") / y * 100 : 0) : y));
        }
    }

    /// <summary>Oval: two straights joined by two half circles; corners at the tyre limit, straights at full
    /// throttle then full braking (ideal brake balance, μ on all four wheels).</summary>
    void SimLap()
    {
        _lap.Clear();
        _lapProfile.Clear();
        _lapStraight = V("straight");
        _lapR = LPNum("lapRadius", 15);
        double vc = Math.Min(CornerSpeed(_lapR), _top);
        double dx = 0.25;
        int n = (int)Math.Ceiling(_lapStraight / dx);
        var fwd = new double[n + 1];
        var bwd = new double[n + 1];
        fwd[0] = vc;
        for (int i = 0; i < n; i++)
        {
            double v = fwd[i], r = Resist(v);
            double f = Math.Min(PowerForce(v, out _), TractionLimit(v, r));
            double a = (f - r) / _m;
            fwd[i + 1] = Math.Sqrt(Math.Max(0.01, v * v + 2 * a * dx));
        }
        bwd[n] = vc;
        for (int i = n; i > 0; i--)
        {
            double v = bwd[i];
            double a = (_mu * (_m * G + Downforce(v)) + Resist(v)) / _m;
            bwd[i - 1] = Math.Sqrt(v * v + 2 * a * dx);
        }
        double tStraight = 0, vTop = 0;
        var vs = new double[n + 1];
        for (int i = 0; i <= n; i++) { vs[i] = Math.Min(fwd[i], bwd[i]); vTop = Math.Max(vTop, vs[i]); }
        var tAt = new double[n + 1];
        for (int i = 1; i <= n; i++) { tStraight += dx / (0.5 * (vs[i] + vs[i - 1])); tAt[i] = tStraight; }
        double tCorner = Math.PI * _lapR / vc;
        _lapTime = 2 * (tStraight + tCorner);
        _lapLen = 2 * (_lapStraight + Math.PI * _lapR);
        Calc["lapTime"] = _lapTime;
        Calc["lapTopSpeed"] = vTop * 3.6;
        Calc["lapCornerSpeed"] = vc * 3.6;

        // trace [t, s, v] for the whole lap and the speed profile over distance
        double t0 = 0, s0 = 0;
        for (int lapPart = 0; lapPart < 2; lapPart++)
        {
            for (int i = 0; i <= n; i += 4)
            {
                _lap.Add(new[] { t0 + tAt[i], s0 + i * dx, vs[i] });
                _lapProfile.Add(new Point(s0 + i * dx, vs[i] * 3.6));
            }
            t0 += tStraight; s0 += _lapStraight;
            _lap.Add(new[] { t0, s0, vc });
            t0 += tCorner; s0 += Math.PI * _lapR;
            _lap.Add(new[] { t0, s0, vc });
            _lapProfile.Add(new Point(s0, vc * 3.6));
        }
    }

    // ---- drawing --------------------------------------------------------------------------------------------

    protected override void DrawScene(DrawingContext dc, Size s)
    {
        switch (Mode)
        {
            case "corner": DrawCorner(dc, s); return;
            case "lap": DrawLap(dc, s); return;
            case "brake": DrawBrake(dc, s); return;
            case "consumption": DrawCruise(dc, s); return;
            default: DrawAccel(dc, s); return;
        }
    }

    void Road(DrawingContext dc, Size s, double xMeters, double pxPerM, out double roadY)
    {
        roadY = s.Height - 46;
        dc.DrawRectangle(Draw.Sky, null, new Rect(0, 0, s.Width, roadY));
        dc.DrawRectangle(Draw.Grass, null, new Rect(0, roadY - 6, s.Width, 6));
        dc.DrawRectangle(Draw.Road, null, new Rect(0, roadY, s.Width, s.Height - roadY));
        double off = -(xMeters * pxPerM) % 40;
        for (double x = off - 40; x < s.Width + 40; x += 40)
            dc.DrawRectangle(Draw.Dim, null, new Rect(x, roadY + 22, 20, 3));
    }

    static void CarSide(DrawingContext dc, double x, double groundY, double wheelAngle, double pitch, bool smoke, bool skidF, bool skidR)
    {
        // x = rear of the car; 112 px long
        dc.PushTransform(new RotateTransform(-pitch, x + 56, groundY - 14));
        var body = new Rect(x, groundY - 34, 112, 20);
        dc.DrawRoundedRectangle(Draw.Body, null, body, 6, 6);
        var cab = new StreamGeometry();
        using (var c = cab.Open())
        {
            c.BeginFigure(new Point(x + 26, groundY - 34), true, true);
            c.LineTo(new Point(x + 40, groundY - 50), true, false);
            c.LineTo(new Point(x + 76, groundY - 50), true, false);
            c.LineTo(new Point(x + 92, groundY - 34), true, false);
        }
        cab.Freeze();
        dc.DrawGeometry(Draw.Glass, null, cab);
        dc.DrawLine(Draw.Pen(Draw.Body, 3), new Point(x + 58, groundY - 50), new Point(x + 58, groundY - 34));
        dc.DrawRectangle(Draw.Yellow, null, new Rect(x + 106, groundY - 30, 6, 4));
        dc.DrawRectangle(Draw.Red, null, new Rect(x, groundY - 30, 4, 4));
        dc.Pop();
        foreach (var wx in new[] { x + 22, x + 90 })
        {
            dc.DrawEllipse(Draw.Tire, null, new Point(wx, groundY - 11), 11, 11);
            dc.DrawEllipse(Draw.Metal, null, new Point(wx, groundY - 11), 5, 5);
            for (int k = 0; k < 3; k++)
            {
                double a = wheelAngle + k * 2 * Math.PI / 3;
                dc.DrawLine(Draw.Pen(Draw.Tire, 1.5), new Point(wx, groundY - 11), new Point(wx + 5 * Math.Cos(a), groundY - 11 + 5 * Math.Sin(a)));
            }
        }
        if (smoke)
            for (int k = 0; k < 4; k++)
                dc.DrawEllipse(Draw.Smoke, null, new Point(x + 8 - k * 12, groundY - 8 - k * 3), 7 + k * 3, 5 + k * 2);
        if (skidF) dc.DrawEllipse(Draw.Smoke, null, new Point(x + 98, groundY - 6), 9, 5);
        if (skidR) dc.DrawEllipse(Draw.Smoke, null, new Point(x + 14, groundY - 6), 9, 5);
    }

    void DrawAccel(DrawingContext dc, Size s)
    {
        var st = At(_accel, T);
        double x = st[1], v = st[2], rpm = st[3];
        Road(dc, s, x, 6, out double roadY);
        CarSide(dc, 40, roadY + 8, x / 0.3, Math.Clamp(st[6] / G * 2.5, 0, 3), st[5] > 0.5 && v < 40, false, false);
        double gx = s.Width - 120;
        Draw.Gauge(dc, new Point(gx, 50), 32, v * 3.6, Mode == "ev" ? 200 : 240, Draw.N(v * 3.6, "0") + " km/h", "velocidade");
        if (_ev)
        {
            Draw.Gauge(dc, new Point(gx + 76, 50), 32, rpm, 16000, Draw.N(rpm / 1000, "0.0") + " mil", "rpm motor", Draw.Blue, 15000);
            Draw.Bar(dc, new Rect(14, 24, 120, 12), 1 - Math.Min(1, x / Math.Max(1, _range * 1000)), "Bateria " + Draw.N(V("battery"), "0") + " kWh");
        }
        else
        {
            Draw.Gauge(dc, new Point(gx + 76, 50), 32, rpm, _rpk * 1.3, Draw.N(rpm / 1000, "0.0") + " mil", "rpm · " + Draw.N(st[4], "0") + "ª", Draw.Blue, _rpk * 1.15);
        }
        Draw.Text(dc, $"t = {Draw.N(T, "0.0")} s", 14, 46, 13, Draw.White, 0, true);
        if (st[5] > 0.5) Draw.Text(dc, "patinando!", 14, 64, 12, Draw.Orange, 0, true);
        if (Mode == "topspeed") Draw.Text(dc, $"máx. {Draw.N(_top * 3.6, "0")} km/h", 14, 64, 12, Draw.Dim);
    }

    void DrawBrake(DrawingContext dc, Size s)
    {
        var st = At(_brake, T);
        double total = Math.Max(10, _stop);
        double ppm = (s.Width - 150) / total;
        double roadY = s.Height - 46;
        dc.DrawRectangle(Draw.Sky, null, new Rect(0, 0, s.Width, roadY));
        dc.DrawRectangle(Draw.Grass, null, new Rect(0, roadY - 6, s.Width, 6));
        dc.DrawRectangle(Draw.Road, null, new Rect(0, roadY, s.Width, s.Height - roadY));
        double step = Draw.Nice(total / 5);
        for (double m = 0; m <= total * 1.15; m += step)
        {
            double px = 130 + m * ppm;
            if (px > s.Width - 16) break;
            dc.DrawLine(Draw.Pen(Draw.Axis, 1), new Point(px, roadY + 4), new Point(px, roadY + 14));
            Draw.Text(dc, Draw.N(m, "0") + " m", px, roadY + 16, 9.5, Draw.Dim, 1);
        }
        double react = Calc.TryGetValue("reactionDistance", out var rd) ? rd : 0;
        if (react > 0)
            dc.DrawRectangle(Ui.Make("#33FFD84D"), null, new Rect(130, roadY + 1, react * ppm, 3));
        double carX = 130 + st[1] * ppm - 112;
        bool lockF = st[3] > 0.5, lockR = st[4] > 0.5;
        if (lockF || lockR) dc.DrawRectangle(Draw.Tire, null, new Rect(130 + react * ppm, roadY + 5, Math.Max(0, st[1] - react) * ppm, 3));
        CarSide(dc, carX, roadY + 8, st[1] / 0.3, T > V("reaction") && st[2] > 0 ? -1.5 : 0, false, lockF, lockR);
        Draw.Gauge(dc, new Point(s.Width - 48, 50), 32, st[2] * 3.6, Draw.Nice(V("speed")), Draw.N(st[2] * 3.6, "0") + " km/h", "velocidade");
        Draw.Text(dc, $"t = {Draw.N(T, "0.00")} s   distância = {Draw.N(st[1], "0.0")} m", 12, 10, 12, Draw.White, 0, true);
        if (T < V("reaction")) Draw.Text(dc, "reagindo…", 12, 28, 12, Draw.Yellow, 0, true);
        else if (lockR) Draw.Text(dc, "rodas traseiras travadas: o carro pode rodar!", 12, 28, 12, Draw.Red, 0, true);
        else if (lockF) Draw.Text(dc, "rodas dianteiras travadas: sem direção!", 12, 28, 12, Draw.Orange, 0, true);
        else if (st[2] > 0 && T > 0) Draw.Text(dc, C("abs") == "on" ? "freando (ABS)" : "freando", 12, 28, 12, Draw.Green, 0, true);
    }

    void DrawCruise(DrawingContext dc, Size s)
    {
        var st = At(_cruise, T);
        Road(dc, s, st[1], 6, out double roadY);
        CarSide(dc, 60, roadY + 8, st[1] / 0.3, 0, false, false, false);
        Draw.Bar(dc, new Rect(14, 24, 130, 12), st[3], "Tanque");
        Draw.Gauge(dc, new Point(s.Width - 48, 50), 32, st[2] * 3.6, 240, Draw.N(st[2] * 3.6, "0") + " km/h", "velocidade");
        Draw.Text(dc, $"{Draw.N(st[1] / 1000, "0.0")} km percorridos", 14, 46, 12, Draw.White, 0, true);
        Draw.Text(dc, $"potência na roda: {Draw.N(CruisePower(V("speed") / 3.6) / 1000, "0.0")} kW", 14, 64, 11, Draw.Dim);
        if (Calc.TryGetValue("canCruise", out var cc) && cc < 0.5) Draw.Text(dc, "o carro não chega a essa velocidade!", 14, 82, 12, Draw.Red, 0, true);
    }

    void DrawCorner(DrawingContext dc, Size s)
    {
        dc.DrawRectangle(Draw.Grass, null, new Rect(s));
        var st = At(_corner, T);
        double r = V("cornerRadius");
        double maxR = 0;
        foreach (var row in _corner) maxR = Math.Max(maxR, row[2]);
        double ppm = Math.Min(s.Height * 0.42 / r, (s.Height * 0.47) / Math.Max(r, Math.Min(maxR, r * 1.6)));
        var c = new Point(s.Width * 0.42, s.Height / 2);
        double roadW = Math.Max(10, 8 * ppm);
        dc.DrawEllipse(null, Draw.Pen(Draw.Road, roadW), c, r * ppm, r * ppm);
        dc.DrawEllipse(null, Draw.Pen(Draw.Dim, 1, true), c, r * ppm, r * ppm);
        // path so far
        var path = new StreamGeometry();
        using (var g = path.Open())
        {
            bool first = true;
            foreach (var row in _corner)
            {
                if (row[0] > T) break;
                var p = new Point(c.X + row[2] * ppm * Math.Cos(row[1]), c.Y + row[2] * ppm * Math.Sin(row[1]));
                if (first) { g.BeginFigure(p, false, false); first = false; } else g.LineTo(p, true, false);
            }
        }
        path.Freeze();
        dc.DrawGeometry(null, Draw.Pen(Draw.Yellow, 1.5), path);
        var car = new Point(c.X + st[2] * ppm * Math.Cos(st[1]), c.Y + st[2] * ppm * Math.Sin(st[1]));
        double heading = st[1] * 180 / Math.PI + 90;
        double len = Math.Max(14, 4.2 * ppm), wid = Math.Max(7, 1.8 * ppm);
        dc.PushTransform(new RotateTransform(heading, car.X, car.Y));
        dc.DrawRoundedRectangle(st[4] > 0.5 ? Draw.Red : Draw.Body, Draw.Pen(Draw.White, 1), new Rect(car.X - wid / 2, car.Y - len / 2, wid, len), 3, 3);
        dc.DrawRectangle(Draw.Glass, null, new Rect(car.X - wid / 2 + 1.5, car.Y + len * 0.05, wid - 3, len * 0.2));
        dc.Pop();
        double aLat = Calc.TryGetValue("lateralG", out var lg) ? lg : 0;
        bool ok = Calc.TryGetValue("cornerOk", out var co) && co > 0.5;
        Draw.Text(dc, $"{Draw.N(V("speed"), "0")} km/h · {Draw.N(aLat, "0.00")} g lateral", s.Width - 10, 10, 12, Draw.White, 2, true);
        Draw.Text(dc, $"limite: {Draw.N(_vmaxCorner * 3.6, "0")} km/h", s.Width - 10, 28, 12, Draw.Dim, 2);
        if (T > 0.5)
            Draw.Text(dc, st[4] > 0.5 ? "capotou!" : ok ? "fez a curva ✓" : "saiu de frente!", s.Width - 10, 46, 13, ok ? Draw.Green : Draw.Red, 2, true);
    }

    void DrawLap(DrawingContext dc, Size s)
    {
        dc.DrawRectangle(Draw.Grass, null, new Rect(s));
        double totalW = _lapStraight + 2 * _lapR, totalH = 2 * _lapR;
        double ppm = Math.Min((s.Width - 40) / totalW, (s.Height - 50) / totalH);
        double x0 = (s.Width - totalW * ppm) / 2 + _lapR * ppm, yMid = s.Height / 2 + 6;
        double rp = _lapR * ppm;
        var rail = Draw.Pen(Draw.Road, Math.Max(8, 6 * ppm));
        var straightEnd = x0 + _lapStraight * ppm;
        dc.DrawLine(rail, new Point(x0, yMid + rp), new Point(straightEnd, yMid + rp));
        dc.DrawLine(rail, new Point(x0, yMid - rp), new Point(straightEnd, yMid - rp));
        Draw.Arc(dc, new Point(straightEnd, yMid), rp, -90, 90, rail);
        Draw.Arc(dc, new Point(x0, yMid), rp, 90, 270, rail);
        dc.DrawLine(Draw.Pen(Draw.White, 2), new Point(x0 + 4, yMid + rp - 6), new Point(x0 + 4, yMid + rp + 6));
        var st = At(_lap, T);
        double sPos = st[1] % Math.Max(1, _lapLen);
        Point pos; double heading;
        double c1 = _lapStraight, c2 = c1 + Math.PI * _lapR, c3 = c2 + _lapStraight;
        if (sPos < c1) { pos = new Point(x0 + sPos * ppm, yMid + rp); heading = 0; }
        else if (sPos < c2) { double a = (sPos - c1) / _lapR; pos = new Point(straightEnd + rp * Math.Sin(a), yMid + rp * Math.Cos(a)); heading = -a * 180 / Math.PI; }
        else if (sPos < c3) { pos = new Point(straightEnd - (sPos - c2) * ppm, yMid - rp); heading = 180; }
        else { double a = (sPos - c3) / _lapR; pos = new Point(x0 - rp * Math.Sin(a), yMid - rp * Math.Cos(a)); heading = 180 - a * 180 / Math.PI; }
        dc.PushTransform(new RotateTransform(heading, pos.X, pos.Y));
        dc.DrawRoundedRectangle(Draw.Orange, Draw.Pen(Draw.White, 1), new Rect(pos.X - 8, pos.Y - 4, 16, 8), 2, 2);
        dc.Pop();
        Draw.Text(dc, $"t = {Draw.N(T, "0.00")} s   {Draw.N(st[2] * 3.6, "0")} km/h", 10, 6, 12, Draw.White, 0, true);
        Draw.Text(dc, $"curvas R = {Draw.N(_lapR, "0")} m · retas {Draw.N(_lapStraight, "0")} m", s.Width - 10, 6, 11, Draw.Dim, 2);
    }

    protected override void DrawPlot(DrawingContext dc, Size s)
    {
        switch (Mode)
        {
            case "brake":
            {
                double vmax = Draw.Nice(V("speed"));
                var p = new Plot(dc, s, 0, Draw.Nice(Math.Max(10, _stop)), 0, vmax, "distância (m)", "velocidade (km/h)");
                p.Line(dc, Pts(_brake, 1, 2, 3.6), Draw.Green);
                var st = At(_brake, T);
                p.Dot(dc, st[1], st[2] * 3.6, Draw.Yellow);
                break;
            }
            case "corner":
            {
                double vmax = Draw.Nice(Math.Max(V("speed"), _vmaxCorner * 3.6) * 1.2);
                double r = V("cornerRadius");
                var curve = new List<Point>();
                for (double kmh = 0; kmh <= vmax; kmh += vmax / 60) { double v = kmh / 3.6; curve.Add(new Point(kmh, v * v / r / G)); }
                double gMax = Draw.Nice(Math.Max(_mu * 1.4, Calc.GetValueOrDefault("lateralG") * 1.1));
                var p = new Plot(dc, s, 0, vmax, 0, gMax, "velocidade (km/h)", "aceleração lateral (g)");
                p.Line(dc, curve, Draw.Blue);
                p.HLine(dc, _mu, Draw.Orange, "limite μ");
                if (_rollRisk) p.HLine(dc, V("track") / (2 * _h), Draw.Red, "capota");
                p.Dot(dc, V("speed"), Calc.GetValueOrDefault("lateralG"), Draw.Yellow);
                break;
            }
            case "consumption":
            case "ev":
            {
                double ymax = Draw.Nice(_curve.Count > 0 ? _curve.Max(c => c.Y) * 1.05 : 1);
                var p = new Plot(dc, s, 20, 200, 0, ymax, "velocidade (km/h)", _ev ? "autonomia (km)" : "consumo (L/100 km)");
                p.Line(dc, _curve, Draw.Blue);
                p.Dot(dc, V("speed"), _ev ? _range : _consumption, Draw.Yellow);
                break;
            }
            case "lap":
            {
                double vmax = Draw.Nice(_lapProfile.Count > 0 ? _lapProfile.Max(c => c.Y) * 1.1 : 100);
                var p = new Plot(dc, s, 0, Math.Max(1, _lapLen), 0, vmax, "distância na volta (m)", "velocidade (km/h)");
                p.Line(dc, _lapProfile.Where(q => q.X <= At(_lap, T)[1] + 0.01).ToList(), Draw.Green);
                break;
            }
            default:
            {
                double tEnd = Duration;
                double vmax = Draw.Nice(Math.Max(110, _accel.Count > 0 ? _accel.Max(r => r[0] <= tEnd ? r[2] : 0) * 3.6 * 1.1 : 110));
                var p = new Plot(dc, s, 0, Math.Max(1, Math.Ceiling(tEnd)), 0, vmax, "tempo (s)", "velocidade (km/h)");
                if (Mode == "topspeed") p.HLine(dc, _top * 3.6, Draw.Orange, "máx. " + Draw.N(_top * 3.6, "0"));
                else p.HLine(dc, 100, Draw.Orange, "100 km/h");
                p.Line(dc, Pts(_accel, 0, 2, 3.6, T), Draw.Green);
                break;
            }
        }
    }

    static List<Point> Pts(List<double[]> trace, int xi, int yi, double yScale, double tMax = double.MaxValue)
    {
        var list = new List<Point>(trace.Count);
        foreach (var r in trace)
        {
            if (r[0] > tMax + 1e-9) break;
            list.Add(new Point(r[xi], r[yi] * yScale));
        }
        return list;
    }

    protected override string Summary()
    {
        string tq = _ev ? $"motor {Draw.N(_tm, "0")} N·m" : $"torque máx. {Draw.N(Calc.GetValueOrDefault("peakTorque"), "0")} N·m a {Draw.N(_rpk / 2, "0")} rpm";
        switch (Mode)
        {
            case "topspeed":
                return $"Ar a {Draw.N(_top * 3.6, "0")} km/h: arrasto {Draw.N(0.5 * Rho * _cdA * _top * _top, "0")} N. " +
                       (Ran ? $"Velocidade máxima: {Draw.N(_top * 3.6, "0")} km/h." : "Clique em ▶ Rodar.");
            case "brake":
                return Ran ? $"Distância de frenagem {Draw.N(Calc["brakingDistance"], "0.0")} m · parada total {Draw.N(Calc["stoppingDistance"], "0.0")} m · desaceleração média {Draw.N(Calc["brakeDecelG"], "0.00")} g"
                    : $"Limite teórico (μ·g): {Draw.N(Math.Pow(V("speed") / 3.6, 2) / (2 * _mu * G), "0.0")} m de frenagem. Clique em ▶ Rodar.";
            case "corner":
                return $"v máx. = √(μ·g·r) = {Draw.N(_vmaxCorner * 3.6, "0")} km/h · capota acima de {Draw.N(V("track") / (2 * _h), "0.00")} g" + (Ran ? "" : ". Clique em ▶ Rodar.");
            case "consumption":
                return $"Consumo {Draw.N(_consumption, "0.0")} L/100 km ({Draw.N(_consumption > 0 ? 100 / _consumption : 0, "0.0")} km/L) · autonomia {Draw.N(_range, "0")} km";
            case "ev":
                return $"Massa total {Draw.N(_m, "0")} kg (bateria {Draw.N(V("battery") / PackKWhPerKg, "0")} kg) · {Draw.N(_consumption, "0.0")} kWh/100 km · autonomia {Draw.N(_range, "0")} km" +
                       (Ran ? $" · 0–100 em {Draw.N(_t100, "0.0")} s" : "");
            case "lap":
                return $"Curva a {Draw.N(Calc.GetValueOrDefault("lapCornerSpeed"), "0")} km/h · máx. na reta {Draw.N(Calc.GetValueOrDefault("lapTopSpeed"), "0")} km/h" +
                       (Ran ? $" · volta em {Draw.N(_lapTime, "0.00")} s" : ". Clique em ▶ Rodar.");
            default:
                return $"{tq} · relações {string.Join(" / ", _ratios.Select(r => Draw.N(r, "0.00")))}" +
                       (Ran ? $" · 0–100 km/h: {(_t100 < 99 ? Draw.N(_t100, "0.0") + " s" : "não chegou")} · patinou {Draw.N(Calc["wheelspin"], "0.0")} s" : "");
        }
    }
}
