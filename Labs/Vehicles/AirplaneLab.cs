using System.Windows;
using System.Windows.Media;

namespace EasyCreator.Vehicles;

/// <summary>Fixed-wing aircraft performance.
/// Assumptions: ISA atmosphere; lift slope of a finite wing a = 2π·AR/(AR+2) per rad, zero-lift angle −2° (cambered
/// airfoil); parabolic polar CD = CD0 + CL²/(π·e·AR); flaps 10/20/30° add ΔCL 0,25/0,45/0,60 and ΔCD0 0,005/0,012/0,025
/// (they are used for stall, lift and takeoff; cruise, polar and range are clean). Propeller thrust = ηp·P/v, limited
/// below 30 m/s (static thrust); engine power and jet thrust fall with the air density ratio σ. Takeoff: rolling
/// friction 0,03, ground attitude α = 0°, lift-off at 1,2·Vstall, then a steady climb at that speed to 15 m.
/// Range: Breguet at the chosen speed and altitude (average weight). Stability: wing aerodynamic centre at 25 % MAC
/// and the tail moves the neutral point back by 40,5 % MAC per unit of tail volume (VH).</summary>
[Lab("airplane")]
public sealed class AirplaneLab : VehicleLab
{
    static readonly double[] FlapDCL = { 0, 0.25, 0.45, 0.60 }, FlapDCD = { 0, 0.005, 0.012, 0.025 };

    public AirplaneLab()
    {
        Num("mass", "Massa sem combustível", "kg", 150, 3000, 10, 1000);
        Num("wingArea", "Área da asa", "m²", 4, 60, 0.5, 16);
        Num("aspectRatio", "Alongamento (AR)", "", 4, 16, 0.1, 7.5);
        Num("clmax", "CL máximo do perfil", "", 1.0, 2.0, 0.05, 1.5);
        Choice("flaps", "Flapes", new[] { "0", "10", "20", "30" }, new[] { "0°", "10°", "20°", "30°" });
        Num("cd0", "Arrasto parasita CD0", "", 0.015, 0.06, 0.001, 0.032);
        Num("oswald", "Oswald e", "", 0.6, 0.95, 0.01, 0.75);
        Choice("propulsion", "Propulsão", new[] { "prop", "jet" }, new[] { "Hélice", "Jato" });
        Num("power", "Potência do motor", "kW", 20, 1000, 5, 120);
        Num("propEff", "Eficiência da hélice", "", 0.5, 0.9, 0.01, 0.8);
        Num("thrust", "Empuxo do jato", "kN", 0.5, 300, 0.5, 20);
        Num("aoa", "Ângulo de ataque", "°", -4, 20, 0.5, 2);
        Num("speed", "Velocidade", "km/h", 50, 950, 5, 150);
        Num("altitude", "Altitude", "m", 0, 11000, 100, 0);
        Num("runway", "Pista", "m", 100, 3000, 10, 400);
        Num("airportAlt", "Altitude do aeroporto", "m", 0, 4500, 100, 0);
        Num("cg", "Posição do CG", "% CMA", 5, 70, 1, 45);
        Num("tailVolume", "Volume de cauda VH", "", 0.2, 1.2, 0.01, 0.45);
        Num("fuel", "Combustível", "kg", 5, 60000, 5, 100);
        Num("sfc", "Consumo específico", "kg/kWh", 0.18, 0.45, 0.01, 0.30);
        Num("tsfc", "TSFC do jato", "g/(kN·s)", 8, 30, 0.5, 18);
    }

    protected override string Title => Mode switch
    {
        "polar" => "✈ Polar de arrasto e L/D",
        "stall" => "✈ Estol",
        "takeoff" => "✈ Decolagem e subida",
        "stability" => "✈ Estabilidade longitudinal",
        "range" => "✈ Alcance (Breguet)",
        _ => "✈ Sustentação da asa",
    };

    protected override string[] DefaultShown(string mode) => mode switch
    {
        "polar" => new[] { "aspectRatio", "oswald", "cd0", "aoa" },
        "stall" => new[] { "mass", "wingArea", "clmax", "flaps" },
        "takeoff" => new[] { "mass", "wingArea", "flaps", "power", "propEff", "airportAlt" },
        "stability" => new[] { "cg", "tailVolume" },
        "range" => new[] { "fuel", "mass", "speed", "altitude", "aspectRatio", "wingArea", "flaps", "power", "cd0" },
        _ => new[] { "speed", "aoa", "wingArea", "mass", "altitude" },
    };

    // ---- aerodynamics ---------------------------------------------------------------------------------------

    double _m, _w, _s, _ar, _k, _cd0, _rho, _sig, _cla, _dcl, _dcd, _clmaxT, _vs, _vsClean;
    bool _jet;

    void Setup()
    {
        _m = V("mass") + V("fuel");
        _w = _m * G;
        _s = V("wingArea");
        _ar = V("aspectRatio");
        _k = 1 / (Math.PI * V("oswald") * _ar);
        _cd0 = V("cd0");
        _rho = Density(V("altitude"));
        _sig = _rho / 1.225;
        _cla = 2 * Math.PI * _ar / (_ar + 2);
        int f = Math.Clamp((int)V("flaps"), 0, 3);
        _dcl = FlapDCL[f];
        _dcd = FlapDCD[f];
        _clmaxT = V("clmax") + _dcl;
        _jet = C("propulsion") == "jet";
        _vs = Math.Sqrt(2 * _w / (1.225 * _s * _clmaxT));
        _vsClean = Math.Sqrt(2 * _w / (1.225 * _s * V("clmax")));
    }

    double Thrust(double v, double sig) => _jet ? V("thrust") * 1000 * sig : V("propEff") * V("power") * 1000 * sig / Math.Max(v, 30);
    double CLofAlpha(double aDeg, bool flaps) => _cla * (aDeg + 2) * Math.PI / 180 + (flaps ? _dcl : 0);
    double DragClean(double v, double w, double rho)
    {
        double cl = 2 * w / (rho * v * v * _s);
        return 0.5 * rho * v * v * _s * (_cd0 + _k * cl * cl);
    }

    double LDmax => 0.5 / Math.Sqrt(_k * _cd0);

    // ---- traces -----------------------------------------------------------------------------------------------

    readonly List<double[]> _lift = new(), _sweep = new(), _stall = new(), _takeoff = new(), _pitch = new(), _route = new();
    readonly List<Point> _curve = new();
    double _groundRoll, _total, _rangeKm, _ldCruise, _sm, _hn, _climb, _vmax;

    protected override void Compute()
    {
        Setup();
        Calc["stallSpeed"] = _vs * 3.6;
        Calc["stallSpeedClean"] = _vsClean * 3.6;
        Calc["approachSpeed"] = 1.3 * _vs * 3.6;
        Calc["LDmax"] = LDmax;
        Calc["wingLoading"] = _m / _s;

        SimLift();
        SimTakeoff();
        Climb();
        Range();
        Stability();
        SimStall();
        Sweep();

        switch (Mode)
        {
            case "polar": Duration = _sweep[^1][0]; RealSeconds = 6; break;
            case "stall": Duration = _stall[^1][0]; RealSeconds = Math.Clamp(Duration / 2, 4, 8); break;
            case "takeoff": Duration = _takeoff[^1][0]; RealSeconds = Math.Clamp(Duration / 3, 4, 9); break;
            case "stability": Duration = _pitch[^1][0]; RealSeconds = 6; break;
            case "range": Duration = _route[^1][0]; RealSeconds = 5; break;
            default: Duration = _lift[^1][0]; RealSeconds = 5; break;
        }
    }

    void SimLift()
    {
        _lift.Clear();
        double v = V("speed") / 3.6, a = V("aoa");
        double cl = CLofAlpha(a, true);
        bool stalled = cl > _clmaxT;
        if (stalled) cl = 0.7 * _clmaxT;
        double lift = 0.5 * _rho * v * v * _s * cl;
        double cd = _cd0 + _dcd + _k * cl * cl + (stalled ? 0.08 : 0);
        Calc["CL"] = cl;
        Calc["lift"] = lift;
        Calc["weight"] = _w;
        Calc["liftToWeight"] = lift / _w;
        Calc["stalled"] = stalled ? 1 : 0;
        Calc["drag"] = 0.5 * _rho * v * v * _s * cd;
        Calc["LD"] = cl / cd;
        double z = 0, vz = 0, x = 0;
        for (double t = 0; t <= 8 + 1e-9; t += 0.02)
        {
            _lift.Add(new[] { t, x, z, vz });
            double az = (lift - _w) / _m - 0.6 * vz;
            vz += az * 0.02;
            z += vz * 0.02;
            x += v * 0.02;
        }
    }

    void Sweep()
    {
        _sweep.Clear();
        for (double t = 0; t <= 6 + 1e-9; t += 0.05) _sweep.Add(new[] { t, -4 + 20 * t / 6 });
    }

    void SimTakeoff()
    {
        _takeoff.Clear();
        double rhoF = Density(V("airportAlt")), sigF = rhoF / 1.225;
        double vlo = 1.2 * Math.Sqrt(2 * _w / (rhoF * _s * _clmaxT)), clg = CLofAlpha(0, true), cdg = _cd0 + _dcd + _k * clg * clg, runway = V("runway");
        double v = 0, x = 0, z = 0, t = 0, dt = 0.02;
        bool lifted = false, climbs = true;
        _groundRoll = 99999; _total = 99999;
        while (t < 240)
        {
            double pitch = 0;
            if (!lifted)
            {
                double q = 0.5 * rhoF * v * v * _s;
                double f = Thrust(v, sigF) - q * cdg - 0.03 * Math.Max(0, _w - q * clg);
                double a = f / _m;
                if (a <= 0 && v < vlo * 0.98 && t > 5) break;     // can't reach lift-off speed
                v += a * dt;
                x += v * dt;
                if (v >= vlo) { lifted = true; _groundRoll = x; }
                if (x > Math.Max(runway * 2.5, 3000)) break;
            }
            else
            {
                double cl = 2 * _w / (rhoF * v * v * _s);
                double d = 0.5 * rhoF * v * v * _s * (_cd0 + _dcd + _k * cl * cl);
                double sinG = Math.Clamp((Thrust(v, sigF) - d) / _w, -0.3, 0.35);
                if (sinG <= 0.002) climbs = false;
                pitch = Math.Asin(sinG) * 180 / Math.PI + 4;
                z += v * sinG * dt;
                x += v * Math.Sqrt(1 - sinG * sinG) * dt;
                if (_total >= 99999 && z >= 15) _total = x;
                if (z >= 60 || !climbs) { _takeoff.Add(new[] { t, x, z, v, pitch }); break; }
            }
            _takeoff.Add(new[] { t, x, z, v, pitch });
            t += dt;
        }
        if (_takeoff.Count == 0) _takeoff.Add(new double[] { 0, 0, 0, 0, 0 });
        bool ok = lifted && climbs && _groundRoll <= runway;
        Calc["takeoffDistance"] = _groundRoll;
        Calc["takeoffTotal"] = _total;
        Calc["liftoffSpeed"] = vlo * 3.6;
        Calc["tookOff"] = ok ? 1 : 0;
    }

    double MaxClimb(double rho, out double vBest)
    {
        double sig = rho / 1.225, vs = Math.Sqrt(2 * _w / (rho * _s * V("clmax"))), best = double.NegativeInfinity;
        vBest = vs;
        for (double v = 1.2 * vs; v <= 340; v += 0.5)
        {
            double rc = (Thrust(v, sig) - DragClean(v, _w, rho)) * v / _w;
            if (rc > best) { best = rc; vBest = v; }
        }
        return best;
    }

    void Climb()
    {
        _climb = MaxClimb(Density(V("airportAlt")), out double vy);
        Calc["climbRate"] = _climb;
        Calc["bestClimbSpeed"] = vy * 3.6;
        double ceiling = 0;
        for (double h = 0; h <= 11000; h += 100)
        {
            if (MaxClimb(Density(h), out _) < 0.5) break;
            ceiling = h;
        }
        Calc["ceiling"] = _climb < 0.5 ? 0 : ceiling;
        _vmax = 0;
        for (double v = _vsClean; v <= 340; v += 0.5)
            if (Thrust(v, _sig) >= DragClean(v, _w, _rho)) _vmax = v;
        Calc["maxSpeed"] = _vmax * 3.6;
    }

    double RangeAt(double v, out double ld, out bool ok)
    {
        double fuel = Math.Min(V("fuel"), 0.9 * _m), w0 = _w, w1 = (_m - fuel) * G, wAvg = 0.5 * (w0 + w1);
        double cl = 2 * wAvg / (_rho * v * v * _s);
        ld = cl / (_cd0 + _k * cl * cl);
        ok = cl <= 0.9 * V("clmax") && wAvg / ld <= Thrust(v, _sig);
        if (!ok) return 0;
        double lnW = Math.Log(w0 / w1);
        double r = _jet ? v / (G * V("tsfc") * 1e-6) * ld * lnW
                        : V("propEff") / (G * V("sfc") / 3.6e6) * ld * lnW;
        return r / 1000;
    }

    void Range()
    {
        _route.Clear();
        double v = V("speed") / 3.6;
        _rangeKm = RangeAt(v, out _ldCruise, out bool ok);
        Calc["range"] = _rangeKm;
        Calc["LDcruise"] = _ldCruise;
        Calc["cruiseOk"] = ok ? 1 : 0;
        Calc["fuelFraction"] = Math.Min(V("fuel"), 0.9 * _m) / _m;
        double hours = _rangeKm / Math.Max(1, v * 3.6);
        Calc["endurance"] = hours;
        double dur = Math.Max(0.1, hours);
        for (int i = 0; i <= 60; i++)
        {
            double f = i / 60.0;
            _route.Add(new[] { dur * f, _rangeKm * f, 1 - f });
        }
        _curve.Clear();
        for (double kmh = Math.Max(60, _vsClean * 3.6 * 1.1); kmh <= Math.Max(V("speed") * 1.6, 300); kmh += 5)
        {
            double r = RangeAt(kmh / 3.6, out _, out bool ok2);
            _curve.Add(new Point(kmh, ok2 ? r : double.NaN));
        }
    }

    void Stability()
    {
        _pitch.Clear();
        _hn = 25 + 40.5 * V("tailVolume");
        _sm = _hn - V("cg");
        Calc["neutralPoint"] = _hn;
        Calc["staticMargin"] = _sm;
        Calc["stable"] = _sm > 0 ? 1 : 0;
        Calc["controllable"] = _sm <= 25 ? 1 : 0;
        double th = 5, w = 0, k = 40 * _sm / 100, c = 2.4 * V("tailVolume");
        for (double t = 0; t <= 10 + 1e-9; t += 0.02)
        {
            _pitch.Add(new[] { t, th });
            double acc = -k * th - c * w;
            w += acc * 0.02;
            th = Math.Clamp(th + w * 0.02, -60, 60);
            if (Math.Abs(th) >= 60) w = 0;
        }
        Calc["pitchAfter10s"] = Math.Abs(_pitch[^1][1]);
    }

    void SimStall()
    {
        _stall.Clear();
        double v = 1.6 * _vs, z = 0, vz = 0, t = 0, tStall = -1, rho0 = 1.225;
        while (t < 30)
        {
            double clReq = 2 * _w / (rho0 * v * v * _s);
            bool stalled = tStall >= 0 || clReq > _clmaxT;
            if (stalled && tStall < 0) tStall = t;
            double alpha = stalled ? 18 - Math.Min(18, (t - tStall) * 10) : (clReq - _dcl) / _cla * 180 / Math.PI - 2;
            if (stalled)
            {
                double az = -G * 0.4 - 0.3 * vz;
                vz += az * 0.02; z += vz * 0.02; v += 2 * 0.02;
            }
            else v -= 1.5 * 0.02;
            _stall.Add(new[] { t, v, alpha, z, stalled ? 1 : 0 });
            if (tStall >= 0 && t - tStall > 3) break;
            t += 0.02;
        }
    }

    // ---- drawing --------------------------------------------------------------------------------------------

    static void PlaneSide(DrawingContext dc, Point c, double sc, double pitchDeg, bool jet, double spin = 0)
    {
        dc.PushTransform(new RotateTransform(-pitchDeg, c.X, c.Y));
        dc.DrawEllipse(Draw.Body, null, c, 34 * sc, 7 * sc);
        var tail = new StreamGeometry();
        using (var g = tail.Open())
        {
            g.BeginFigure(new Point(c.X - 26 * sc, c.Y - 3 * sc), true, true);
            g.LineTo(new Point(c.X - 36 * sc, c.Y - 18 * sc), true, false);
            g.LineTo(new Point(c.X - 30 * sc, c.Y - 18 * sc), true, false);
            g.LineTo(new Point(c.X - 16 * sc, c.Y - 4 * sc), true, false);
        }
        tail.Freeze();
        dc.DrawGeometry(Draw.Body2, null, tail);
        dc.DrawRectangle(Draw.Body2, null, new Rect(c.X - 36 * sc, c.Y - 2 * sc, 14 * sc, 3 * sc));
        dc.DrawRoundedRectangle(Draw.Body2, null, new Rect(c.X - 8 * sc, c.Y - 1 * sc, 22 * sc, 4 * sc), 2 * sc, 2 * sc);
        dc.DrawEllipse(Draw.Glass, null, new Point(c.X + 18 * sc, c.Y - 3 * sc), 7 * sc, 3.5 * sc);
        if (jet) dc.DrawRoundedRectangle(Draw.Metal, null, new Rect(c.X - 2 * sc, c.Y + 3 * sc, 14 * sc, 5 * sc), 2 * sc, 2 * sc);
        else
        {
            double h = 12 * sc * Math.Abs(Math.Cos(spin));
            dc.DrawLine(Draw.Pen(Draw.Metal, 2), new Point(c.X + 35 * sc, c.Y - h), new Point(c.X + 35 * sc, c.Y + h));
        }
        dc.Pop();
    }

    protected override void DrawScene(DrawingContext dc, Size s)
    {
        switch (Mode)
        {
            case "polar": ScenePolar(dc, s); return;
            case "stall": SceneStall(dc, s); return;
            case "takeoff": SceneTakeoff(dc, s); return;
            case "stability": SceneStability(dc, s); return;
            case "range": SceneRange(dc, s); return;
            default: SceneLift(dc, s); return;
        }
    }

    void SceneLift(DrawingContext dc, Size s)
    {
        dc.DrawRectangle(Draw.Sky, null, new Rect(s));
        var st = At(_lift, T);
        double y = Math.Clamp(s.Height * 0.5 - st[2] * 4, 30, s.Height - 30);
        double off = -(st[1] * 2) % 60;
        for (double x = off; x < s.Width + 60; x += 60) dc.DrawEllipse(Draw.SkyLow, null, new Point(x, s.Height * 0.25 + ((int)(x / 60) % 2) * 50), 22, 7);
        var c = new Point(s.Width * 0.45, y);
        double sc = 1.4;
        PlaneSide(dc, c, sc, Math.Clamp(V("aoa"), -10, 25), _jet, T * 40);
        double lw = Calc.GetValueOrDefault("liftToWeight");
        double baseLen = 45;
        Draw.Arrow(dc, new Point(c.X, c.Y - 4), new Point(c.X, c.Y - 4 - baseLen * Math.Min(2.5, lw)), Draw.Green, 3);
        Draw.Arrow(dc, new Point(c.X + 6, c.Y + 4), new Point(c.X + 6, c.Y + 4 + baseLen), Draw.Red, 3);
        Draw.Text(dc, "L", c.X + 6, c.Y - 18 - baseLen * Math.Min(2.5, lw), 12, Draw.Green, 0, true);
        Draw.Text(dc, "P", c.X + 12, c.Y + baseLen - 6, 12, Draw.Red, 0, true);
        Draw.Text(dc, $"sustentação {Draw.N(Calc.GetValueOrDefault("lift") / 1000, "0.0")} kN", 10, 8, 12, Draw.Green, 0, true);
        Draw.Text(dc, $"peso {Draw.N(_w / 1000, "0.0")} kN", 10, 26, 12, Draw.Red, 0, true);
        Draw.Text(dc, $"CL = {Draw.N(Calc.GetValueOrDefault("CL"), "0.00")}  ρ = {Draw.N(_rho, "0.000")} kg/m³", 10, 44, 11, Draw.Dim);
        if (Calc.GetValueOrDefault("stalled") > 0.5) Draw.Text(dc, "ESTOL! ângulo de ataque alto demais", s.Width - 10, 8, 12, Draw.Red, 2, true);
        else if (T > 0.5) Draw.Text(dc, lw >= 1 ? "sobe ↑" : "desce ↓", s.Width - 10, 8, 13, lw >= 1 ? Draw.Green : Draw.Orange, 2, true);
    }

    void ScenePolar(DrawingContext dc, Size s)
    {
        dc.DrawRectangle(Draw.Sky, null, new Rect(s));
        double a = T > 0 && T < Duration ? At(_sweep, T)[1] : V("aoa");
        double cl = CLofAlpha(a, false);
        bool stalled = cl > V("clmax");
        if (stalled) cl = 0.7 * V("clmax");
        double cd = _cd0 + _k * cl * cl + (stalled ? 0.08 : 0);
        var c = new Point(s.Width * 0.42, s.Height * 0.55);
        // airflow lines
        for (int i = -3; i <= 3; i++)
        {
            double y0 = c.Y + i * 18;
            var g = new StreamGeometry();
            using (var ctx = g.Open())
            {
                ctx.BeginFigure(new Point(10, y0), false, false);
                for (double x = 10; x < s.Width - 10; x += 6)
                {
                    double dx = (x - c.X) / 80, bump = Math.Exp(-dx * dx) * (i <= 0 ? -1 : 0.4) * (6 + a * 0.6) * Math.Exp(-Math.Abs(i) * 0.25);
                    double wobble = stalled && i <= 0 && x > c.X ? Math.Sin(x * 0.15 + T * 8) * 5 : 0;
                    ctx.LineTo(new Point(x, y0 + bump + wobble + (x > c.X ? (x - c.X) * Math.Sin(a * Math.PI / 180) * 0.15 : 0)), true, true);
                }
            }
            g.Freeze();
            dc.DrawGeometry(null, Draw.Pen(Draw.SkyLow, 1.2), g);
        }
        dc.PushTransform(new RotateTransform(-a, c.X, c.Y));
        var foil = new StreamGeometry();
        using (var g = foil.Open())
        {
            g.BeginFigure(new Point(c.X - 70, c.Y), true, true);
            g.BezierTo(new Point(c.X - 70, c.Y - 18), new Point(c.X + 10, c.Y - 16), new Point(c.X + 70, c.Y + 1), true, true);
            g.BezierTo(new Point(c.X + 10, c.Y + 4), new Point(c.X - 70, c.Y + 10), new Point(c.X - 70, c.Y), true, true);
        }
        foil.Freeze();
        dc.DrawGeometry(Draw.Metal, Draw.Pen(Draw.White, 1), foil);
        dc.Pop();
        double lpx = 70 * Math.Max(0, cl), dpx = 700 * cd;
        Draw.Arrow(dc, new Point(c.X - 10, c.Y - 6), new Point(c.X - 10, c.Y - 6 - lpx), Draw.Green, 3);
        Draw.Arrow(dc, new Point(c.X - 10, c.Y - 6), new Point(c.X - 10 + dpx, c.Y - 6), Draw.Red, 3);
        Draw.Text(dc, "sustentação", c.X - 4, c.Y - 18 - lpx, 10, Draw.Green);
        Draw.Text(dc, "arrasto (×10)", c.X - 4 + dpx, c.Y - 2, 10, Draw.Red);
        Draw.Text(dc, $"α = {Draw.N(a, "0.0")}°  CL = {Draw.N(cl, "0.00")}  CD = {Draw.N(cd, "0.000")}  L/D = {Draw.N(cl / cd, "0.0")}", 10, 8, 12, Draw.White, 0, true);
        if (stalled) Draw.Text(dc, "escoamento descolado: ESTOL", 10, 26, 12, Draw.Red, 0, true);
    }

    void SceneStall(DrawingContext dc, Size s)
    {
        dc.DrawRectangle(Draw.Sky, null, new Rect(s));
        var st = At(_stall, T);
        double y = Math.Clamp(s.Height * 0.4 - st[3] * 3, 25, s.Height - 25);
        var c = new Point(s.Width * 0.45, y);
        PlaneSide(dc, c, 1.4, st[4] > 0.5 ? st[2] - 15 : st[2], _jet, T * 30);
        Draw.Text(dc, $"velocidade {Draw.N(st[1] * 3.6, "0")} km/h", 10, 8, 12, Draw.White, 0, true);
        Draw.Text(dc, $"ângulo de ataque {Draw.N(st[2], "0.0")}°", 10, 26, 12, Draw.Dim);
        Draw.Text(dc, $"estol a {Draw.N(_vs * 3.6, "0")} km/h", s.Width - 10, 8, 12, Draw.Orange, 2, true);
        if (st[4] > 0.5) Draw.Text(dc, "ESTOL: o nariz cai e o avião perde altura", s.Width - 10, 26, 12, Draw.Red, 2, true);
    }

    void SceneTakeoff(DrawingContext dc, Size s)
    {
        dc.DrawRectangle(Draw.Sky, null, new Rect(s));
        double runway = V("runway"), maxX = 0;
        foreach (var r in _takeoff) maxX = Math.Max(maxX, r[1]);
        double span = Math.Max(runway * 1.2, maxX * 1.05);
        double ppm = (s.Width - 30) / span, gy = s.Height - 30, zpm = Math.Min(ppm * 3, (gy - 60) / 60);
        dc.DrawRectangle(Draw.Grass, null, new Rect(0, gy, s.Width, s.Height - gy));
        dc.DrawRectangle(Draw.Road, null, new Rect(15, gy, runway * ppm, 6));
        dc.DrawRectangle(Draw.Red, null, new Rect(15 + runway * ppm - 2, gy - 8, 3, 14));
        Draw.Text(dc, $"fim da pista ({Draw.N(runway, "0")} m)", 15 + runway * ppm, gy + 8, 10, Draw.Red, 2);
        dc.DrawLine(Draw.Pen(Draw.Dim, 1, true), new Point(0, gy - 15 * zpm), new Point(s.Width, gy - 15 * zpm));
        Draw.Text(dc, "15 m", 4, gy - 15 * zpm - 13, 9.5, Draw.Dim);
        var path = new List<Point>();
        foreach (var r in _takeoff) { if (r[0] > T) break; path.Add(new Point(15 + r[1] * ppm, gy - r[2] * zpm)); }
        for (int i = 1; i < path.Count; i++) dc.DrawLine(Draw.Pen(Draw.Yellow, 1.2), path[i - 1], path[i]);
        var st = At(_takeoff, T);
        PlaneSide(dc, new Point(15 + st[1] * ppm, gy - st[2] * zpm - 8), 0.7, st[4], _jet, T * 40);
        Draw.Text(dc, $"{Draw.N(st[3] * 3.6, "0")} km/h · {Draw.N(st[1], "0")} m · altura {Draw.N(st[2], "0")} m", 10, 8, 12, Draw.White, 0, true);
        Draw.Text(dc, $"decola a {Draw.N(Calc.GetValueOrDefault("liftoffSpeed"), "0")} km/h", 10, 26, 11, Draw.Dim);
        if (T >= Duration - 1e-6 && T > 0)
        {
            bool ok = Calc.GetValueOrDefault("tookOff") > 0.5;
            Draw.Text(dc, ok ? "decolou ✓" : "não decolou dentro da pista!", 10, 44, 13, ok ? Draw.Green : Draw.Red, 0, true);
        }
    }

    void SceneStability(DrawingContext dc, Size s)
    {
        dc.DrawRectangle(Draw.Sky, null, new Rect(s));
        var st = At(_pitch, T);
        var c = new Point(s.Width * 0.45, s.Height * 0.42);
        PlaneSide(dc, c, 1.6, st[1], _jet, T * 40);
        // chord bar with CG and neutral point
        double x0 = 40, x1 = s.Width - 40, y = s.Height - 34;
        dc.DrawRoundedRectangle(Draw.Metal, null, new Rect(x0, y - 4, x1 - x0, 8), 4, 4);
        Draw.Text(dc, "bordo de ataque", x0, y + 8, 9.5, Draw.Dim);
        Draw.Text(dc, "bordo de fuga", x1, y + 8, 9.5, Draw.Dim, 2);
        double Xp(double pct) => x0 + (x1 - x0) * Math.Clamp(pct, 0, 100) / 100;
        dc.DrawEllipse(Draw.Yellow, Draw.Pen(Ui.Code, 1), new Point(Xp(V("cg")), y), 7, 7);
        Draw.Text(dc, "CG", Xp(V("cg")), y - 22, 11, Draw.Yellow, 1, true);
        var np = new Point(Xp(_hn), y);
        var tri = new StreamGeometry();
        using (var g = tri.Open()) { g.BeginFigure(new Point(np.X, np.Y - 7), true, true); g.LineTo(new Point(np.X - 7, np.Y + 6), true, false); g.LineTo(new Point(np.X + 7, np.Y + 6), true, false); }
        tri.Freeze();
        dc.DrawGeometry(Draw.Blue, null, tri);
        Draw.Text(dc, "PN", np.X, y + 8, 11, Draw.Blue, 1, true);
        Draw.Text(dc, $"arfagem {Draw.N(st[1], "0.0")}°", 10, 8, 12, Draw.White, 0, true);
        Draw.Text(dc, $"margem estática {Draw.N(_sm, "0.0")} % CMA", 10, 26, 12, _sm > 0 ? Draw.Green : Draw.Red, 0, true);
        if (T > 1) Draw.Text(dc, _sm > 0 ? "estável: volta sozinho" : "instável: diverge!", s.Width - 10, 8, 13, _sm > 0 ? Draw.Green : Draw.Red, 2, true);
    }

    void SceneRange(DrawingContext dc, Size s)
    {
        dc.DrawRectangle(Draw.Grass, null, new Rect(s));
        var st = At(_route, T);
        double maxKm = Draw.Nice(Math.Max(100, _rangeKm * 1.1));
        double x0 = 30, x1 = s.Width - 30, y = s.Height * 0.55;
        dc.DrawLine(Draw.Pen(Draw.Dim, 1.5, true), new Point(x0, y), new Point(x1, y));
        for (int i = 0; i <= 4; i++)
        {
            double km = maxKm * i / 4, px = x0 + (x1 - x0) * i / 4;
            dc.DrawEllipse(Draw.Dim, null, new Point(px, y), 3, 3);
            Draw.Text(dc, Draw.N(km, "0") + " km", px, y + 8, 10, Draw.Dim, 1);
        }
        double xp = x0 + (x1 - x0) * Math.Min(1, st[1] / maxKm);
        dc.DrawLine(Draw.Pen(Draw.Yellow, 2.5), new Point(x0, y), new Point(xp, y));
        PlaneSide(dc, new Point(xp, y - 22), 0.8, 0, _jet, T * 60);
        Draw.Bar(dc, new Rect(14, 30, 140, 12), st[2], $"Combustível ({Draw.N(Math.Min(V("fuel"), 0.9 * _m), "0")} kg)");
        Draw.Text(dc, $"{Draw.N(st[1], "0")} km · {Draw.N(st[0], "0.0")} h de voo", s.Width - 10, 8, 12, Draw.White, 2, true);
        Draw.Text(dc, $"L/D em cruzeiro {Draw.N(_ldCruise, "0.0")} (máx. {Draw.N(LDmax, "0.0")})", s.Width - 10, 26, 11, Draw.Dim, 2);
        if (Calc.GetValueOrDefault("cruiseOk") < 0.5) Draw.Text(dc, "não sustenta esse voo (CL alto demais ou falta empuxo)", 14, s.Height - 22, 12, Draw.Red, 0, true);
    }

    protected override void DrawPlot(DrawingContext dc, Size s)
    {
        switch (Mode)
        {
            case "polar":
            {
                var pts = new List<Point>();
                for (double a = -4; a <= 16; a += 0.5)
                {
                    double cl = Math.Min(CLofAlpha(a, false), V("clmax"));
                    pts.Add(new Point(_cd0 + _k * cl * cl, cl));
                }
                double cdMax = Draw.Nice(pts.Max(p => p.X) * 1.1);
                var p = new Plot(dc, s, 0, cdMax, -0.5, 2, "CD", "CL (polar)");
                p.Line(dc, pts, Draw.Blue);
                double clStar = Math.Sqrt(_cd0 / _k);
                p.Line(dc, new List<Point> { new(0, 0), new(cdMax, cdMax * clStar / (2 * _cd0)) }, Draw.Orange, 1.2, true);
                p.Dot(dc, 2 * _cd0, clStar, Draw.Orange, 3.5);
                Draw.Text(dc, $"L/D máx = {Draw.N(LDmax, "0.0")}", p.R.Right, p.R.Bottom - 16, 10, Draw.Orange, 2);
                double aNow = T > 0 && T < Duration ? At(_sweep, T)[1] : V("aoa");
                double c2 = Math.Min(CLofAlpha(aNow, false), V("clmax"));
                p.Dot(dc, _cd0 + _k * c2 * c2, c2, Draw.Yellow);
                break;
            }
            case "stall":
            {
                double vmax = Draw.Nice(_vs * 3.6 * 2);
                var pts = new List<Point>();
                for (double kmh = _vs * 3.6 * 0.6; kmh <= vmax; kmh += 2) { double v = kmh / 3.6; pts.Add(new Point(kmh, 2 * _w / (1.225 * v * v * _s))); }
                var p = new Plot(dc, s, 0, vmax, 0, Draw.Nice(_clmaxT * 1.5), "velocidade (km/h)", "CL necessário");
                p.Line(dc, pts, Draw.Blue);
                p.HLine(dc, _clmaxT, Draw.Orange, "CL máx");
                p.VLine(dc, _vs * 3.6, Draw.Red, "Vestol");
                var st = At(_stall, T);
                if (T > 0) p.Dot(dc, st[1] * 3.6, 2 * _w / (1.225 * st[1] * st[1] * _s), Draw.Yellow);
                break;
            }
            case "takeoff":
            {
                double maxX = 0;
                foreach (var r in _takeoff) maxX = Math.Max(maxX, r[1]);
                var p = new Plot(dc, s, 0, Draw.Nice(Math.Max(V("runway") * 1.2, maxX)), 0, 60, "distância (m)", "altura (m)");
                p.VLine(dc, V("runway"), Draw.Red, "pista");
                p.HLine(dc, 15, Draw.Dim, "15 m");
                var pts = new List<Point>();
                foreach (var r in _takeoff) { if (r[0] > T) break; pts.Add(new Point(r[1], r[2])); }
                p.Line(dc, pts, Draw.Green);
                break;
            }
            case "stability":
            {
                var p = new Plot(dc, s, 0, 10, -30, 30, "tempo (s)", "arfagem (°)");
                var pts = new List<Point>();
                foreach (var r in _pitch) { if (r[0] > T) break; pts.Add(new Point(r[0], r[1])); }
                p.Line(dc, pts, _sm > 0 ? Draw.Green : Draw.Red);
                break;
            }
            case "range":
            {
                double ymax = Draw.Nice(Math.Max(100, _curve.Where(c => !double.IsNaN(c.Y)).Select(c => c.Y).DefaultIfEmpty(0).Max() * 1.1));
                double xmax = _curve.Count > 0 ? _curve[^1].X : 300;
                var p = new Plot(dc, s, _curve.Count > 0 ? _curve[0].X : 0, xmax, 0, ymax, "velocidade (km/h)", "alcance (km)");
                p.Line(dc, _curve, Draw.Blue);
                p.Dot(dc, V("speed"), _rangeKm, Draw.Yellow);
                break;
            }
            default:
            {
                double vmax = Draw.Nice(Math.Max(V("speed") * 1.5, 200));
                double cl = CLofAlpha(V("aoa"), true);
                if (cl > _clmaxT) cl = 0.7 * _clmaxT;
                var pts = new List<Point>();
                for (double kmh = 0; kmh <= vmax; kmh += vmax / 60) { double v = kmh / 3.6; pts.Add(new Point(kmh, 0.5 * _rho * v * v * _s * cl / 1000)); }
                var p = new Plot(dc, s, 0, vmax, 0, Draw.Nice(_w / 1000 * 2), "velocidade (km/h)", "sustentação (kN)");
                p.Line(dc, pts, Draw.Green);
                p.HLine(dc, _w / 1000, Draw.Red, "peso");
                p.Dot(dc, V("speed"), Calc.GetValueOrDefault("lift") / 1000, Draw.Yellow);
                break;
            }
        }
    }

    protected override string Summary()
    {
        string run = Ran ? "" : " Clique em ▶ Rodar.";
        return Mode switch
        {
            "polar" => $"L/D máx = ½·√(π·e·AR/CD0) = {Draw.N(LDmax, "0.0")} · a {Draw.N(Calc.GetValueOrDefault("LD"), "0.0")} no ângulo escolhido.{run}",
            "stall" => $"Vestol = √(2·P/(ρ·S·CLmáx)) = {Draw.N(_vs * 3.6, "0")} km/h · aproximação (1,3·Vs) {Draw.N(1.3 * _vs * 3.6, "0")} km/h.{run}",
            "takeoff" => $"Corrida no solo {(_groundRoll < 99999 ? Draw.N(_groundRoll, "0") + " m" : "—")} · até 15 m {(_total < 99999 ? Draw.N(_total, "0") + " m" : "—")} · razão de subida {Draw.N(_climb, "0.0")} m/s.{run}",
            "stability" => $"Ponto neutro em {Draw.N(_hn, "0.0")} % da corda · CG em {Draw.N(V("cg"), "0")} % · margem estática {Draw.N(_sm, "0.0")} %.{run}",
            "range" => $"Alcance {Draw.N(_rangeKm, "0")} km · L/D {Draw.N(_ldCruise, "0.0")} · {Draw.N(Calc.GetValueOrDefault("endurance"), "0.0")} h · vel. máx. {Draw.N(_vmax * 3.6, "0")} km/h.{run}",
            _ => $"L = ½·ρ·v²·S·CL = {Draw.N(Calc.GetValueOrDefault("lift"), "0")} N · peso {Draw.N(_w, "0")} N · L/P = {Draw.N(Calc.GetValueOrDefault("liftToWeight"), "0.00")}.{run}",
        };
    }
}
