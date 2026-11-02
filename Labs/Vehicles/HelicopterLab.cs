using System.Windows;
using System.Windows.Media;

namespace EasyCreator.Vehicles;

/// <summary>Helicopter main rotor by blade-element/momentum theory.
/// Assumptions: untwisted blades, lift slope a = 5,73 /rad, uniform inflow; CT = (σa/2)(θ/3 − λ/2) with
/// λ = λclimb + λinduced and CT = 2·λi·(λc + λi); blades stall above CT/σ = 0,13. Power = T·Vc + T·vi/FM, plus 10 % for
/// the tail rotor and gearbox; engine power falls with the density ratio. Hover power (ideal) = T^1,5 / √(2ρA).
/// Torque Q = P_rotor / Ω must be cancelled by the tail rotor: T_tail = Q / arm. Autorotation (vertical, engine off):
/// the rotor works like a parachute with drag coefficient 1,2 over the disc; for the flare the rotor stores
/// ½·I·Ω², blades of 24·c·R kg each, and may lose up to 30 % of its rpm.</summary>
[Lab("helicopter")]
public sealed class HelicopterLab : VehicleLab
{
    const double LiftSlope = 5.73, CtSigmaMax = 0.13, Extra = 1.10, Rho0 = 1.225;

    public HelicopterLab()
    {
        Num("mass", L.T("Massa"), "kg", 100, 6000, 10, 1100);
        Num("rotorRadius", L.T("Raio do rotor"), "m", 2, 10, 0.1, 4.0);
        Num("blades", L.T("Número de pás"), "", 2, 6, 1, 2);
        Num("chord", L.T("Corda da pá"), "m", 0.1, 0.6, 0.01, 0.25);
        Num("rpm", L.T("Rotação do rotor"), "rpm", 150, 700, 5, 400);
        Num("collective", L.T("Passo coletivo"), "°", 0, 16, 0.2, 8);
        Num("power", L.T("Potência do motor"), "kW", 20, 3000, 5, 150);
        Num("altitude", L.T("Altitude"), "m", 0, 6000, 100, 0);
        Num("fm", L.T("Figura de mérito"), "", 0.5, 0.85, 0.01, 0.70);
        Num("tailArm", L.T("Braço do rotor de cauda"), "m", 2, 10, 0.1, 4.5);
        Num("tailMax", L.T("Empuxo máx. da cauda"), "N", 100, 10000, 50, 1000);
    }

    protected override string Title => Mode switch
    {
        "climb" => L.T("🚁 Subida vertical"),
        "tail" => L.T("🚁 Torque e rotor de cauda (vista de cima)"),
        "auto" => L.T("🚁 Autorrotação (motor desligado)"),
        _ => L.T("🚁 Pairar: empuxo do rotor"),
    };

    protected override string[] DefaultShown(string mode) => mode switch
    {
        "climb" => new[] { "power", "collective", "mass", "rotorRadius", "altitude", "rpm" },
        "tail" => new[] { "tailArm", "tailMax", "power", "rpm", "mass", "rotorRadius" },
        "auto" => new[] { "rotorRadius", "mass", "rpm", "blades", "chord" },
        _ => new[] { "rotorRadius", "rpm", "blades", "chord", "collective", "mass", "power", "altitude" },
    };

    double _m, _w, _r, _a, _rho, _omega, _vtip, _sigma, _theta, _fm, _pAvail;

    void Setup(double altitude)
    {
        _m = V("mass");
        _w = _m * G;
        _r = V("rotorRadius");
        _a = Math.PI * _r * _r;
        _rho = Density(altitude);
        _omega = V("rpm") * 2 * Math.PI / 60;
        _vtip = _omega * _r;
        _sigma = V("blades") * V("chord") / (Math.PI * _r);
        _theta = V("collective") * Math.PI / 180;
        _fm = V("fm");
        _pAvail = V("power") * 1000 * _rho / Rho0;
    }

    /// <summary>Thrust from the collective pitch at climb speed vc (blade element + momentum), capped by blade stall.</summary>
    double ThrustColl(double theta, double vc, out bool stall)
    {
        double lc = Math.Max(0, vc) / _vtip, sa = _sigma * LiftSlope;
        double lo = 0, hi = 0.4;
        if (sa / 2 * (theta / 3 - lc / 2) <= 0) { stall = false; return 0; }
        for (int i = 0; i < 50; i++)
        {
            double li = 0.5 * (lo + hi);
            double f = sa / 2 * (theta / 3 - (lc + li) / 2) - 2 * li * (lc + li);
            if (f > 0) lo = li; else hi = li;
        }
        double l = 0.5 * (lo + hi);
        double ct = 2 * l * (lc + l), ctMax = CtSigmaMax * _sigma;
        stall = ct > ctMax;
        return Math.Min(ct, ctMax) * _rho * _a * _vtip * _vtip;
    }

    double Vi(double t, double vc) { vc = Math.Max(0, vc); return -vc / 2 + Math.Sqrt(vc * vc / 4 + Math.Max(0, t) / (2 * _rho * _a)); }
    double PowerFor(double t, double vc) => (t * Math.Max(0, vc) + t * Vi(t, vc) / _fm) * Extra;

    /// <summary>Largest thrust the engine can drive at climb speed vc.</summary>
    double ThrustPower(double vc)
    {
        double lo = 0, hi = 60 * Math.Max(_w, 1000);
        for (int i = 0; i < 60; i++)
        {
            double t = 0.5 * (lo + hi);
            if (PowerFor(t, vc) < _pAvail) lo = t; else hi = t;
        }
        return lo;
    }

    double ThrustAt(double vc, double theta, out bool stall) => Math.Min(ThrustColl(theta, vc, out stall), ThrustPower(vc));
    double BodyDrag(double vz) => 0.5 * _rho * (2 + 0.002 * _m) * vz * Math.Abs(vz);

    readonly List<double[]> _vert = new(), _yaw = new(), _auto = new();
    double _t0, _tColl, _pHover, _climb, _q, _tailNeed, _vd, _td;
    bool _stall;

    protected override void Compute()
    {
        Setup(V("altitude"));
        _tColl = ThrustColl(_theta, 0, out _stall);
        _t0 = Math.Min(_tColl, ThrustPower(0));
        _pHover = PowerFor(_w, 0);
        bool canHover = _t0 >= _w;
        Calc["thrust"] = _t0;
        Calc["thrustToWeight"] = _t0 / _w;
        Calc["hoverPower"] = _pHover / 1000;
        Calc["idealHoverPower"] = Math.Pow(_w, 1.5) / Math.Sqrt(2 * _rho * _a) / 1000;
        Calc["inducedVelocity"] = Vi(_w, 0);
        Calc["diskLoading"] = _w / _a;
        Calc["canHover"] = canHover ? 1 : 0;
        Calc["bladeStall"] = _stall ? 1 : 0;
        Calc["tipMach"] = _vtip / 340;
        Calc["solidity"] = _sigma;

        // steady vertical climb: thrust(Vc) = weight + body drag
        _climb = 0;
        if (canHover)
        {
            double lo = 0, hi = 40;
            if (ThrustAt(hi, _theta, out _) - _w - BodyDrag(hi) > 0) _climb = hi;
            else
            {
                for (int i = 0; i < 50; i++)
                {
                    double vc = 0.5 * (lo + hi);
                    if (ThrustAt(vc, _theta, out _) - _w - BodyDrag(vc) > 0) lo = vc; else hi = vc;
                }
                _climb = lo;
            }
        }
        Calc["climbRate"] = _climb;

        // hover ceiling with the collective at its maximum: only power and blade stall limit it
        double ceiling = 0, thetaMax = P("collective").Max * Math.PI / 180;
        for (double h = 0; h <= 8000; h += 50)
        {
            Setup(h);
            if (ThrustAt(0, thetaMax, out _) < _w) break;
            ceiling = h;
        }
        Setup(V("altitude"));
        Calc["hoverCeiling"] = ceiling;

        // torque and tail rotor
        double tHold = Math.Min(_w, _t0);
        _q = PowerFor(tHold, 0) / Extra / _omega;
        _tailNeed = _q / V("tailArm");
        Calc["torque"] = _q;
        Calc["tailThrust"] = _tailNeed;
        Calc["yawBalanced"] = V("tailMax") >= _tailNeed ? 1 : 0;

        // autorotation
        _vd = Math.Sqrt(2 * _w / (_rho * _a * 1.2));
        double inertia = 8 * V("blades") * V("chord") * Math.Pow(_r, 3);
        double eFlare = 0.5 * inertia * _omega * _omega * (1 - 0.7 * 0.7);
        _td = Math.Sqrt(Math.Max(0, _vd * _vd - 2 * eFlare / _m));
        Calc["autoDescent"] = _vd;
        Calc["rotorEnergy"] = 0.5 * inertia * _omega * _omega / 1000;
        Calc["touchdownSpeed"] = _td;
        Calc["safeLanding"] = _td <= 3 ? 1 : 0;

        SimVertical();
        SimYaw();
        SimAuto();
        switch (Mode)
        {
            case "tail": Duration = _yaw[^1][0]; RealSeconds = 6; break;
            case "auto": Duration = _auto[^1][0]; RealSeconds = Math.Clamp(Duration / 3, 4, 8); break;
            default: Duration = _vert[^1][0]; RealSeconds = Mode == "climb" ? 7 : 6; break;
        }
    }

    void SimVertical()
    {
        _vert.Clear();
        bool climb = Mode == "climb";
        double z = 0, vz = 0, dt = 0.01, dur = climb ? 20 : 12, nextRec = 0;
        for (double t = 0; t <= dur + 1e-9; t += dt)
        {
            double avail = ThrustAt(vz, _theta, out _);
            double thrust = climb ? avail : Math.Min(avail, _w + _m * (1.2 * (10 - z) - 2.0 * vz));
            thrust = Math.Max(0, thrust);
            double az = (thrust - _w - BodyDrag(vz)) / _m;
            if (t >= nextRec - 1e-9) { _vert.Add(new[] { t, z, vz, thrust }); nextRec += 0.04; }
            vz += az * dt;
            z += vz * dt;
            if (z <= 0) { z = 0; if (vz < 0) vz = 0; }
        }
        Calc["heightAfterRun"] = _vert[^1][1];
    }

    void SimYaw()
    {
        _yaw.Clear();
        double iz = 0.08 * _m * Math.Pow(V("tailArm"), 2), psi = 0, w = 0;
        double tail = Math.Min(_tailNeed, V("tailMax"));
        for (double t = 0; t <= 10 + 1e-9; t += 0.02)
        {
            _yaw.Add(new[] { t, psi, w });
            double acc = (_q - tail * V("tailArm")) / iz - 0.3 * w;
            w += acc * 0.02;
            psi += w * 0.02;
        }
        Calc["yawRate"] = Math.Abs(_yaw[^1][2]) * 180 / Math.PI;
    }

    void SimAuto()
    {
        _auto.Clear();
        double z = 300, vz = 0, t = 0, flareH = 20, vFlare = -1;
        while (t < 120)
        {
            double rpmF = 1;
            if (z > flareH) { vz += (-_vd - vz) * 0.02 / 1.5; }
            else
            {
                double f = Math.Clamp(z / flareH, 0, 1);
                if (vFlare < 0) vFlare = Math.Max(Math.Abs(vz), _td);
                vz = -Math.Sqrt(_td * _td + (vFlare * vFlare - _td * _td) * f);
                if (vz > -0.3) vz = -0.3;
                rpmF = 0.7 + 0.3 * f;
            }
            _auto.Add(new[] { t, z, vz, rpmF });
            if (z <= 0) break;
            z += vz * 0.02;
            if (z < 0) z = 0;
            t += 0.02;
        }
    }

    // ---- drawing --------------------------------------------------------------------------------------------

    static void HeliSide(DrawingContext dc, Point c, double sc, double rotorPx, double phase, bool downwash)
    {
        // c = centre of the cabin
        dc.DrawLine(Draw.Pen(Draw.Body2, 4 * sc), new Point(c.X - 10 * sc, c.Y - 2 * sc), new Point(c.X - 60 * sc, c.Y - 6 * sc));
        dc.DrawEllipse(Draw.Metal, null, new Point(c.X - 62 * sc, c.Y - 7 * sc), 2.5 * sc, 9 * sc * Math.Abs(Math.Cos(phase * 3)) + 1);
        dc.DrawEllipse(Draw.Body, null, c, 22 * sc, 13 * sc);
        dc.DrawEllipse(Draw.Glass, null, new Point(c.X + 10 * sc, c.Y - 3 * sc), 9 * sc, 7 * sc);
        dc.DrawLine(Draw.Pen(Draw.Metal, 2), new Point(c.X - 16 * sc, c.Y + 17 * sc), new Point(c.X + 18 * sc, c.Y + 17 * sc));
        dc.DrawLine(Draw.Pen(Draw.Metal, 1.5), new Point(c.X - 8 * sc, c.Y + 12 * sc), new Point(c.X - 10 * sc, c.Y + 17 * sc));
        dc.DrawLine(Draw.Pen(Draw.Metal, 1.5), new Point(c.X + 8 * sc, c.Y + 12 * sc), new Point(c.X + 10 * sc, c.Y + 17 * sc));
        dc.DrawLine(Draw.Pen(Draw.Metal, 3), new Point(c.X, c.Y - 13 * sc), new Point(c.X, c.Y - 19 * sc));
        double half = rotorPx * Math.Abs(Math.Cos(phase));
        dc.DrawLine(Draw.Pen(Draw.SkyLow, 2), new Point(c.X - rotorPx, c.Y - 19 * sc), new Point(c.X + rotorPx, c.Y - 19 * sc));
        dc.DrawLine(Draw.Pen(Draw.White, 3), new Point(c.X - half, c.Y - 19 * sc), new Point(c.X + half, c.Y - 19 * sc));
        if (downwash)
            for (int i = -2; i <= 2; i++)
                Draw.Arrow(dc, new Point(c.X + i * rotorPx * 0.4, c.Y - 14 * sc), new Point(c.X + i * rotorPx * 0.36, c.Y + 26 * sc), Draw.SkyLow, 1.5);
    }

    protected override void DrawScene(DrawingContext dc, Size s)
    {
        if (Mode == "tail") { SceneTail(dc, s); return; }
        dc.DrawRectangle(Draw.Sky, null, new Rect(s));
        double gy = s.Height - 22;
        dc.DrawRectangle(Draw.Grass, null, new Rect(0, gy, s.Width, s.Height - gy));
        double rotorPx = Math.Min(s.Width * 0.3, 18 + _r * 9);
        if (Mode == "auto")
        {
            var st = At(_auto, T);
            double zpx = (gy - 50) / 300;
            var c = new Point(s.Width * 0.4, gy - 20 - st[1] * zpx);
            HeliSide(dc, c, 1, rotorPx, T * 18 * st[3], false);
            Draw.Text(dc, L.F("altura {0} m · descendo a {1} m/s", Draw.N(st[1], "0"), Draw.N(-st[2], "0.0")), 10, 8, 12, Draw.White, 0, true);
            Draw.Text(dc, L.F("rotor a {0} % da rotação", Draw.N(st[3] * 100, "0")), 10, 26, 11, Draw.Dim);
            if (st[1] <= 20 && st[1] > 0) Draw.Text(dc, L.T("arremetida (flare): a energia do rotor freia a descida"), s.Width - 10, 8, 11, Draw.Yellow, 2, true);
            if (T >= Duration - 1e-6 && T > 0)
                Draw.Text(dc, _td <= 3 ? L.F("pouso suave ✓ ({0} m/s)", Draw.N(_td, "0.0")) : L.F("pouso duro! {0} m/s", Draw.N(_td, "0.0")), s.Width - 10, 26, 13, _td <= 3 ? Draw.Green : Draw.Red, 2, true);
            return;
        }
        var v = At(_vert, T);
        double top = Mode == "climb" ? Math.Max(20, Draw.Nice(_vert[^1][1] * 1.1)) : 20;
        double zp = (gy - 60) / top;
        var hc = new Point(s.Width * 0.4, gy - 18 - v[1] * zp);
        HeliSide(dc, hc, 1, rotorPx, T * 20, v[3] > 0.2 * _w);
        for (double m = 0; m <= top; m += Draw.Nice(top / 4))
        {
            double y = gy - m * zp;
            dc.DrawLine(Draw.Pen(Draw.Axis, 1), new Point(s.Width - 34, y), new Point(s.Width - 26, y));
            Draw.Text(dc, Draw.N(m, "0") + " m", s.Width - 38, y - 7, 9.5, Draw.Dim, 2);
        }
        Draw.Text(dc, L.F("empuxo {0} kN · peso {1} kN", Draw.N(v[3] / 1000, "0.00"), Draw.N(_w / 1000, "0.00")), 10, 8, 12, Draw.White, 0, true);
        Draw.Text(dc, L.F("altura {0} m · subida {1} m/s", Draw.N(v[1], "0.0"), Draw.N(Math.Abs(v[2]) < 0.05 ? 0 : v[2], "0.0")), 10, 26, 11, Draw.Dim);
        if (T > 1 && Calc.GetValueOrDefault("canHover") < 0.5)
            Draw.Text(dc, _stall ? L.T("as pás estolaram: empuxo insuficiente") : L.T("empuxo menor que o peso: não decola"), 10, 44, 12, Draw.Red, 0, true);
    }

    void SceneTail(DrawingContext dc, Size s)
    {
        dc.DrawRectangle(Draw.Grass, null, new Rect(s));
        var st = At(_yaw, T);
        var c = new Point(s.Width * 0.42, s.Height * 0.52);
        double arm = Math.Min(s.Height * 0.42, 60);
        double psiDeg = st[1] * 180 / Math.PI;
        dc.DrawEllipse(null, Draw.Pen(Draw.SkyLow, 1.5), c, arm * 1.1, arm * 1.1);
        dc.PushTransform(new RotateTransform(-psiDeg, c.X, c.Y));
        dc.DrawLine(Draw.Pen(Draw.Body2, 5), c, new Point(c.X - arm, c.Y));
        dc.DrawEllipse(Draw.Body, null, c, 18, 12);
        dc.DrawEllipse(Draw.Glass, null, new Point(c.X + 9, c.Y), 7, 8);
        dc.DrawLine(Draw.Pen(Draw.Metal, 3), new Point(c.X - arm, c.Y - 10), new Point(c.X - arm, c.Y + 10));
        double tail = Math.Min(_tailNeed, V("tailMax"));
        Draw.Arrow(dc, new Point(c.X - arm, c.Y), new Point(c.X - arm, c.Y + 8 + 30 * Math.Min(1.5, tail / Math.Max(1, _tailNeed))), Draw.Green, 2.5);
        dc.Pop();
        Draw.Arc(dc, c, arm * 0.55, 200, 120, Draw.Pen(Draw.Orange, 2.5));
        Draw.Text(dc, L.T("torque do rotor"), c.X, c.Y - arm * 0.55 - 18, 10, Draw.Orange, 1);
        Draw.Text(dc, L.F("torque {0} N·m → cauda precisa de {1} N", Draw.N(_q, "0"), Draw.N(_tailNeed, "0")), 10, 8, 12, Draw.White, 0, true);
        Draw.Text(dc, L.F("cauda dá até {0} N · giro {1} °/s", Draw.N(V("tailMax"), "0"), Draw.N(st[2] * 180 / Math.PI, "0")), 10, 26, 11, Draw.Dim);
        if (T > 0.5) Draw.Text(dc, _tailNeed <= V("tailMax") ? L.T("proa estável ✓") : L.T("a fuselagem gira!"), s.Width - 10, 8, 13, _tailNeed <= V("tailMax") ? Draw.Green : Draw.Red, 2, true);
    }

    protected override void DrawPlot(DrawingContext dc, Size s)
    {
        switch (Mode)
        {
            case "tail":
            {
                var pts = _yaw.Where(r => r[0] <= T).Select(r => new Point(r[0], Math.Abs(r[2]) * 180 / Math.PI)).ToList();
                var p = new Plot(dc, s, 0, 10, 0, Draw.Nice(Math.Max(30, Math.Abs(_yaw[^1][2]) * 180 / Math.PI)), L.T("tempo (s)"), L.T("giro da fuselagem (°/s)"));
                p.Line(dc, pts, Draw.Orange);
                break;
            }
            case "auto":
            {
                var pts = _auto.Where(r => r[0] <= T).Select(r => new Point(r[0], -r[2])).ToList();
                var p = new Plot(dc, s, 0, Math.Ceiling(_auto[^1][0]), 0, Draw.Nice(_vd * 1.2), L.T("tempo (s)"), L.T("razão de descida (m/s)"));
                p.HLine(dc, 3, Draw.Green, L.T("pouso seguro"));
                p.Line(dc, pts, Draw.Blue);
                break;
            }
            case "climb":
            {
                var pts = _vert.Where(r => r[0] <= T).Select(r => new Point(r[0], r[1])).ToList();
                var p = new Plot(dc, s, 0, 20, 0, Math.Max(20, Draw.Nice(_vert[^1][1] * 1.1)), L.T("tempo (s)"), L.T("altura (m)"));
                p.Line(dc, pts, Draw.Green);
                break;
            }
            default:
            {
                var pts = new List<Point>();
                double maxT = 0, thMax = P("collective").Max;
                for (double th = 0; th <= thMax + 1e-9; th += 0.5)
                {
                    double t = ThrustColl(th * Math.PI / 180, 0, out _) / 1000;
                    pts.Add(new Point(th, t));
                    maxT = Math.Max(maxT, t);
                }
                double tp = ThrustPower(0) / 1000;
                var p = new Plot(dc, s, 0, thMax, 0, Draw.Nice(Math.Max(maxT, _w / 1000) * 1.3), L.T("passo coletivo (°)"), L.T("empuxo (kN)"));
                p.Line(dc, pts, Draw.Blue);
                p.HLine(dc, _w / 1000, Draw.Red, L.T("peso"), labelLeft: true, labelBelow: tp >= _w / 1000);
                p.HLine(dc, tp, Draw.Orange, L.T("limite do motor"), labelBelow: tp < _w / 1000);
                p.Dot(dc, V("collective"), _t0 / 1000, Draw.Yellow);
                break;
            }
        }
    }

    protected override string Summary()
    {
        string run = Ran ? "" : L.T(" Clique em ▶ Rodar.");
        return Mode switch
        {
            "tail" => L.F("Q = P/Ω = {0} N·m · cauda: Q/braço = {1} N (disponível {2} N).{3}", Draw.N(_q, "0"), Draw.N(_tailNeed, "0"), Draw.N(V("tailMax"), "0"), run),
            "auto" => L.F("Descida em autorrotação ≈ {0} m/s · energia no rotor {1} kJ · toque {2} m/s.{3}", Draw.N(_vd, "0.0"), Draw.N(Calc.GetValueOrDefault("rotorEnergy"), "0"), Draw.N(_td, "0.0"), run),
            "climb" => L.F("Pairar exige {0} kW de {1} kW disponíveis · subida {2} m/s.{3}", Draw.N(_pHover / 1000, "0"), Draw.N(_pAvail / 1000, "0"), Draw.N(_climb, "0.0"), run),
            _ => L.F("Ponta da pá a {0} m/s (Mach {1}) · empuxo {2} N · pairar exige {3} kW (disp. {4} kW) · teto {5} m.{6}", Draw.N(_vtip, "0"), Draw.N(_vtip / 340, "0.00"), Draw.N(_t0, "0"), Draw.N(_pHover / 1000, "0"), Draw.N(_pAvail / 1000, "0"), Draw.N(Calc.GetValueOrDefault("hoverCeiling"), "0"), run),
        };
    }
}
