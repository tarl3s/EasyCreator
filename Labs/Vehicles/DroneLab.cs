using System.Windows;
using System.Windows.Media;

namespace EasyCreator.Vehicles;

/// <summary>Multirotor (quad/hexa/octo) sizing and control.
/// Assumptions: thrust proportional to throttle (hover throttle = weight / max thrust); LiPo 3,7 V per cell,
/// 150 Wh/kg, 80 % of the capacity usable; momentum theory per propeller P = T^1,5/√(2ρA) with figure of merit 0,55
/// and motor+ESC efficiency 0,75 (so a bigger propeller needs less power for the same thrust); motors answer with a
/// first-order lag τ = 0,03·(D/5")^1,5 s. Altitude hold: throttle % = Kp·e + Ki·∫e − Kd·(climb speed), no hover
/// feed-forward (so P alone leaves an error), integral frozen while the output saturates. Attitude hold: torque %
/// = Kp·e + Ki·∫e − Kd·(roll rate) against a constant disturbance of 5 % of the maximum torque (wind, CG offset).
/// Forward flight: body drag ½ρ·CdA·v² with CdA = 0,02·m^(2/3), induced velocity from Glauert's formula.</summary>
[Lab("drone")]
public sealed class DroneLab : VehicleLab
{
    const double Rho = 1.225, Fm = 0.55, EtaMotor = 0.75, CellV = 3.7, WhPerKg = 150, Usable = 0.8;

    public DroneLab()
    {
        Num("frameMass", L.T("Drone sem bateria"), "g", 50, 8000, 10, 600);
        Choice("motors", L.T("Motores"), new[] { "4", "6", "8" }, new[] { L.T("4 (quad)"), L.T("6 (hexa)"), L.T("8 (octo)") });
        Num("motorThrust", L.T("Empuxo máx. por motor"), "g", 50, 6000, 10, 350);
        Num("propSize", L.T("Hélice"), "pol", 2, 24, 0.5, 5);
        Num("cells", L.T("Células da bateria"), "S", 1, 12, 1, 3);
        Num("capacity", L.T("Capacidade"), "mAh", 200, 30000, 50, 1300);
        Num("payload", L.T("Carga útil"), "g", 0, 10000, 10, 0);
        Num("kp", L.T("Kp"), "", 0, 50, 0.1, 5);
        Num("ki", L.T("Ki"), "", 0, 30, 0.1, 0);
        Num("kd", L.T("Kd"), "", 0, 30, 0.05, 0);
        Num("target", L.T("Altura alvo"), "m", 1, 50, 0.5, 10);
        Num("angle", L.T("Ângulo alvo"), "°", 5, 45, 1, 20);
        Num("speed", L.T("Velocidade de cruzeiro"), "m/s", 1, 30, 0.5, 8);
        Num("route", L.T("Distância até o ponto"), "km", 0.1, 30, 0.1, 2);
    }

    protected override string Title => Mode switch
    {
        "altitude" => L.T("🛸 Controle PID de altura"),
        "attitude" => L.T("🛸 Controle PID de atitude (rolagem)"),
        "mission" => L.T("🛸 Missão: ida e volta"),
        _ => L.T("🛸 Empuxo, bateria e tempo de voo"),
    };

    protected override string[] DefaultShown(string mode) => mode switch
    {
        "altitude" => new[] { "kp", "ki", "kd", "target" },
        "attitude" => new[] { "kp", "ki", "kd", "angle" },
        "mission" => new[] { "speed", "route", "capacity", "cells", "payload", "propSize" },
        _ => new[] { "frameMass", "motorThrust", "motors", "propSize", "cells", "capacity", "payload" },
    };

    protected override void OnMode()
    {
        if (Mode == "attitude")
        {
            P("kp").Max = 10; P("kp").Step = 0.05; P("kp").Value = 0.5;
            P("ki").Max = 10; P("ki").Step = 0.05;
            P("kd").Max = 2; P("kd").Step = 0.01;
        }
    }

    double _mBat, _m, _w, _tMax, _e, _v, _a, _tau, _pHover, _arm;
    int _n;

    void Setup(double payloadG, double capacity, double prop)
    {
        _n = int.TryParse(C("motors"), out var n) ? n : 4;
        _e = V("cells") * CellV * capacity / 1000;                  // Wh
        _mBat = _e / WhPerKg;                                        // kg
        _m = V("frameMass") / 1000 + _mBat + payloadG / 1000;
        _w = _m * G;
        _tMax = _n * V("motorThrust") / 1000 * G;
        _v = V("cells") * CellV;
        double d = prop * 0.0254;
        _a = Math.PI * d * d / 4;
        _tau = 0.03 * Math.Pow(prop / 5, 1.5);
        _arm = d * (_n == 4 ? 0.85 : _n == 6 ? 1.2 : 1.57);
        _pHover = HoverPower(_w);
    }

    void Setup() => Setup(V("payload"), V("capacity"), V("propSize"));

    double HoverPower(double weight)
    {
        double t = weight / _n;
        return _n * Math.Pow(t, 1.5) / Math.Sqrt(2 * Rho * _a) / (Fm * EtaMotor);
    }

    double FlightMinutes(double pElec) => pElec > 0 ? Usable * _e / pElec * 60 : 0;

    readonly List<double[]> _hover = new(), _step = new(), _mission = new();
    readonly List<Point> _curve = new();
    double _flight, _settle, _overshoot, _missionMin, _energyPct;

    protected override void Compute()
    {
        Setup();
        double twr = _tMax / _w;
        _flight = twr >= 1 ? FlightMinutes(_pHover) : 0;
        Calc["totalMass"] = _m * 1000;
        Calc["batteryMass"] = _mBat * 1000;
        Calc["batteryWh"] = _e;
        Calc["thrustToWeight"] = twr;
        Calc["hoverThrottle"] = Math.Min(100, 100 / Math.Max(twr, 1e-9));
        Calc["canFly"] = twr > 1 ? 1 : 0;
        Calc["hoverPower"] = _pHover;
        Calc["hoverCurrent"] = _pHover / _v;
        Calc["flightTime"] = _flight;
        Calc["gramsPerWatt"] = _w / G * 1000 / _pHover;

        SimHover();
        if (Mode == "attitude") SimAttitude(); else SimAltitude();
        SimMission();
        BuildCurve();
        switch (Mode)
        {
            case "altitude": case "attitude": Duration = _step[^1][0]; RealSeconds = 7; break;
            case "mission": Duration = _mission[^1][0]; RealSeconds = 6; break;
            default: Duration = _hover[^1][0]; RealSeconds = 6; break;
        }
    }

    void SimHover()
    {
        _hover.Clear();
        double twr = _tMax / _w, flightS = Math.Max(1, _flight * 60);
        // [t (s of real flight), height, battery fraction]
        for (int i = 0; i <= 100; i++)
        {
            double t = flightS * i / 100, f = i / 100.0;
            double h = twr <= 1 ? 0 : f < 0.06 ? 3 * f / 0.06 : f > 0.94 ? 3 * (1 - f) / 0.06 : 3;
            _hover.Add(new[] { t, h, twr <= 1 ? 1 : 1 - Usable * f });
        }
    }

    void SimAltitude()
    {
        _step.Clear();
        double kp = V("kp"), ki = V("ki"), kd = V("kd"), target = V("target");
        double z = 0, vz = 0, integ = 0, f = 0, dt = 0.002, dur = 15, nextRec = 0, zMax = 0, cdA = 0.02 * Math.Pow(_m, 2.0 / 3);
        double lastOut = 0, rise10 = -1, rise90 = -1;
        for (double t = 0; t <= dur + 1e-9; t += dt)
        {
            double e = target - z;
            double u = kp * e + ki * integ - kd * vz;
            double cmd = Math.Clamp(u / 100, 0, 1);
            bool sat = u >= 100 && e > 0 || u <= 0 && e < 0;
            if (!sat) integ += e * dt;
            f += (cmd - f) * dt / _tau;
            double thrust = f * _tMax;
            double az = (thrust - _w - 0.5 * Rho * cdA * vz * Math.Abs(vz)) / _m;
            vz += az * dt;
            z += vz * dt;
            if (z <= 0) { z = 0; if (vz < 0) vz = 0; }
            zMax = Math.Max(zMax, z);
            if (rise10 < 0 && z >= 0.1 * target) rise10 = t;
            if (rise90 < 0 && z >= 0.9 * target) rise90 = t;
            lastOut = cmd;
            if (t >= nextRec - 1e-9) { _step.Add(new[] { t, z, vz, f, cmd }); nextRec += 0.02; }
        }
        Finish(target, zMax, rise10, rise90, "reachedAltitude");
        Calc["finalThrottle"] = lastOut * 100;
    }

    void SimAttitude()
    {
        _step.Clear();
        double kp = V("kp"), ki = V("ki"), kd = V("kd"), target = V("angle");
        double tauMax = 0.32 * _tMax * _arm, inertia = 0.4 * _m * _arm * _arm, dist = 0.05 * tauMax;
        double th = 0, w = 0, integ = 0, f = 0, dt = 0.001, dur = 4, nextRec = 0, thMax = 0, rise10 = -1, rise90 = -1;
        bool crashed = false;
        for (double t = 0; t <= dur + 1e-9; t += dt)
        {
            double e = target - th;
            double u = kp * e + ki * integ - kd * w * 180 / Math.PI;
            double cmd = Math.Clamp(u / 100, -1, 1);
            bool sat = u >= 100 && e > 0 || u <= -100 && e < 0;
            if (!sat) integ += e * dt;
            f += (cmd - f) * dt / _tau;
            double acc = (f * tauMax - dist) / inertia - 0.5 * w;
            w += acc * dt;
            th += w * dt * 180 / Math.PI;
            if (Math.Abs(th) > 90) { crashed = true; th = Math.Sign(th) * 90; w = 0; }
            thMax = Math.Max(thMax, th);
            if (rise10 < 0 && th >= 0.1 * target) rise10 = t;
            if (rise90 < 0 && th >= 0.9 * target) rise90 = t;
            if (t >= nextRec - 1e-9) { _step.Add(new[] { t, th, w, f, cmd }); nextRec += 0.01; }
        }
        Finish(target, thMax, rise10, rise90, "reachedAngle");
        Calc["flipped"] = crashed ? 1 : 0;
        if (crashed) Calc["reachedAngle"] = 0;
    }

    void Finish(double target, double peak, double r10, double r90, string reachedKey)
    {
        double end = _step[^1][0], band = 0.05 * target, sum = 0;
        int cnt = 0;
        _settle = 99;
        for (int i = _step.Count - 1; i >= 0; i--)
            if (Math.Abs(_step[i][1] - target) > band) { _settle = i == _step.Count - 1 ? 99 : _step[Math.Min(i + 1, _step.Count - 1)][0]; break; }
            else if (i == 0) _settle = 0;
        foreach (var r in _step) if (r[0] >= end - 1) { sum += r[1]; cnt++; }
        double steady = cnt > 0 ? Math.Abs(target - sum / cnt) : target;
        _overshoot = Math.Max(0, (peak - target) / target * 100);
        Calc["overshoot"] = _overshoot;
        Calc["settlingTime"] = _settle;
        Calc["steadyError"] = steady;
        Calc["riseTime"] = r10 >= 0 && r90 >= 0 ? r90 - r10 : 99;
        Calc[reachedKey] = steady <= band && _settle < 99 ? 1 : 0;
    }

    /// <summary>Electric power in forward flight at speed v with weight w.</summary>
    double ForwardPower(double weight, double mass, double v, out double tiltDeg, out double thrust)
    {
        double cdA = 0.02 * Math.Pow(mass, 2.0 / 3);
        double drag = 0.5 * Rho * cdA * v * v;
        thrust = Math.Sqrt(weight * weight + drag * drag);
        tiltDeg = Math.Atan2(drag, weight) * 180 / Math.PI;
        double aTot = _n * _a, vh = Math.Sqrt(thrust / (2 * Rho * aTot)), vi = vh;
        for (int i = 0; i < 60; i++) vi = 0.5 * vi + 0.5 * vh * vh / Math.Sqrt(v * v + vi * vi);
        return (thrust * vi / Fm + drag * v) / EtaMotor;
    }

    void SimMission()
    {
        _mission.Clear();
        double v = V("speed"), d = V("route") * 1000;
        bool deliver = LP("deliver") == "1";
        double mOut = _m, mBack = deliver ? _m - V("payload") / 1000 : _m;
        double pOut = ForwardPower(mOut * G, mOut, v, out double tilt1, out double th1);
        double pBack = ForwardPower(mBack * G, mBack, v, out double tilt2, out double th2);
        double pHoverBack = HoverPower(mBack * G);
        bool feasible = _tMax >= 1.5 * _w && Math.Max(tilt1, tilt2) <= 40 && Math.Max(th1, th2) <= 0.85 * _tMax;
        double tLeg = d / v, tHover = 20;
        double eUsed = (_pHover * tHover + pOut * tLeg + pBack * tLeg + pHoverBack * tHover) / 3600;  // Wh
        _energyPct = _e > 0 ? eUsed / _e * 100 : 999;
        _missionMin = (2 * tLeg + 2 * tHover) / 60;
        bool ok = feasible && eUsed <= Usable * _e;
        Calc["missionTime"] = _missionMin;
        Calc["energyUsed"] = _energyPct;
        Calc["missionOk"] = ok ? 1 : 0;
        Calc["cruisePower"] = pOut;
        Calc["tiltAngle"] = tilt1;
        double whPerKm = (pOut + pBack) / 2 / v * 1000 / 3600;
        Calc["maxRange"] = whPerKm > 0 ? Math.Max(0, (Usable * _e - (_pHover + pHoverBack) * tHover / 3600) / whPerKm / 2) : 0;

        // [t, position along the route 0..1..0, battery fraction, height]
        double t = 0, batt = 1, total = 2 * tLeg + 2 * tHover;
        for (int i = 0; i <= 200; i++)
        {
            t = total * i / 200;
            double pos, h, used;
            if (t < tHover) { pos = 0; h = 15 * t / tHover; used = _pHover * t; }
            else if (t < tHover + tLeg) { pos = (t - tHover) / tLeg; h = 15; used = _pHover * tHover + pOut * (t - tHover); }
            else if (t < tHover + 2 * tLeg) { pos = 1 - (t - tHover - tLeg) / tLeg; h = 15; used = _pHover * tHover + pOut * tLeg + pBack * (t - tHover - tLeg); }
            else { pos = 0; h = 15 * (1 - (t - tHover - 2 * tLeg) / tHover); used = _pHover * tHover + (pOut + pBack) * tLeg + pHoverBack * (t - tHover - 2 * tLeg); }
            batt = 1 - used / 3600 / Math.Max(1e-9, _e);
            _mission.Add(new[] { t, pos, batt, h });
        }
    }

    void BuildCurve()
    {
        _curve.Clear();
        bool byProp = LP("plot") == "prop" || Mode == "mission";
        if (Mode == "mission")
        {
            double save = V("speed");
            for (double v = 1; v <= 25; v += 0.5)
            {
                double p = (ForwardPower(_w, _m, v, out _, out _)) / v * 1000 / 3600;
                _curve.Add(new Point(v, p));
            }
            return;
        }
        if (byProp)
        {
            for (double d = Math.Max(2, P("propSize").Min); d <= P("propSize").Max + 1e-9; d += 0.5)
            {
                Setup(V("payload"), V("capacity"), d);
                _curve.Add(new Point(d, _tMax > _w ? FlightMinutes(_pHover) : 0));
            }
        }
        else
        {
            double max = Math.Min(P("capacity").Max, Math.Max(4 * V("capacity"), 3000));
            for (double c = 200; c <= max + 1e-9; c += max / 60)
            {
                Setup(V("payload"), c, V("propSize"));
                _curve.Add(new Point(c, _tMax > _w ? FlightMinutes(_pHover) : 0));
            }
        }
        Setup();
    }

    // ---- drawing --------------------------------------------------------------------------------------------

    void DroneSide(DrawingContext dc, Point c, double sc, double rollDeg, double spin, bool thrustArrows)
    {
        dc.PushTransform(new RotateTransform(rollDeg, c.X, c.Y));
        double arm = 46 * sc;
        dc.DrawLine(Draw.Pen(Draw.Metal, 4 * sc), new Point(c.X - arm, c.Y), new Point(c.X + arm, c.Y));
        dc.DrawRoundedRectangle(Draw.Body, null, new Rect(c.X - 16 * sc, c.Y - 8 * sc, 32 * sc, 14 * sc), 4, 4);
        dc.DrawEllipse(Draw.Green, null, new Point(c.X + 8 * sc, c.Y - 2 * sc), 2.5 * sc, 2.5 * sc);
        foreach (var x in new[] { c.X - arm, c.X + arm })
        {
            dc.DrawRectangle(Draw.Tire, null, new Rect(x - 4 * sc, c.Y - 9 * sc, 8 * sc, 9 * sc));
            double half = (12 + V("propSize") * 1.3) * sc * (0.35 + 0.65 * Math.Abs(Math.Cos(spin)));
            dc.DrawLine(Draw.Pen(Draw.White, 2), new Point(x - half, c.Y - 11 * sc), new Point(x + half, c.Y - 11 * sc));
            if (thrustArrows) Draw.Arrow(dc, new Point(x, c.Y - 12 * sc), new Point(x, c.Y - 34 * sc), Draw.Green, 2);
        }
        dc.DrawLine(Draw.Pen(Draw.Metal, 2), new Point(c.X - 12 * sc, c.Y + 6 * sc), new Point(c.X - 18 * sc, c.Y + 16 * sc));
        dc.DrawLine(Draw.Pen(Draw.Metal, 2), new Point(c.X + 12 * sc, c.Y + 6 * sc), new Point(c.X + 18 * sc, c.Y + 16 * sc));
        if (V("payload") > 0 && Mode != "attitude") dc.DrawRectangle(Draw.Orange, null, new Rect(c.X - 7 * sc, c.Y + 8 * sc, 14 * sc, 9 * sc));
        dc.Pop();
    }

    protected override void DrawScene(DrawingContext dc, Size s)
    {
        dc.DrawRectangle(Draw.Sky, null, new Rect(s));
        switch (Mode)
        {
            case "altitude":
            {
                double gy = s.Height - 20, target = V("target");
                double top = Draw.Nice(Math.Max(target * 1.6, _step.Max(r => r[1]) * 1.05));
                double zp = (gy - 34) / top;
                dc.DrawRectangle(Draw.Grass, null, new Rect(0, gy, s.Width, s.Height - gy));
                dc.DrawLine(Draw.Pen(Draw.Orange, 1.2, true), new Point(0, gy - target * zp), new Point(s.Width, gy - target * zp));
                Draw.Text(dc, L.F("alvo {0} m", Draw.N(target, "0.#")), s.Width - 8, gy - target * zp - 15, 10, Draw.Orange, 2);
                var st = At(_step, T);
                DroneSide(dc, new Point(s.Width * 0.4, gy - 18 - st[1] * zp), 1, 0, T * 50, st[3] > 0.05);
                Draw.Text(dc, L.F("altura {0} m · acelerador {1} %", Draw.N(st[1], "0.00"), Draw.N(st[3] * 100, "0")), 10, 8, 12, Draw.White, 0, true);
                Draw.Text(dc, L.F("pairar exige {0} % de acelerador", Draw.N(Calc.GetValueOrDefault("hoverThrottle"), "0")), 10, 26, 11, Draw.Dim);
                break;
            }
            case "attitude":
            {
                var st = At(_step, T);
                var c = new Point(s.Width * 0.42, s.Height * 0.55);
                double target = V("angle");
                dc.DrawLine(Draw.Pen(Draw.Orange, 1.2, true), new Point(c.X - 90 * Math.Cos(target * Math.PI / 180), c.Y - 90 * Math.Sin(target * Math.PI / 180)),
                    new Point(c.X + 90 * Math.Cos(target * Math.PI / 180), c.Y + 90 * Math.Sin(target * Math.PI / 180)));
                DroneSide(dc, c, 1.3, st[1], T * 50, false);
                Draw.Text(dc, L.F("rolagem {0}° (alvo {1}°)", Draw.N(st[1], "0.0"), Draw.N(target, "0")), 10, 8, 12, Draw.White, 0, true);
                Draw.Text(dc, L.F("diferença de empuxo {0} %", Draw.N(st[3] * 100, "0")), 10, 26, 11, Draw.Dim);
                if (Calc.GetValueOrDefault("flipped") > 0.5 && T > 0.5) Draw.Text(dc, L.T("capotou! ganhos instáveis"), s.Width - 10, 8, 13, Draw.Red, 2, true);
                break;
            }
            case "mission":
            {
                dc.DrawRectangle(Draw.Grass, null, new Rect(s));
                var st = At(_mission, T);
                double x0 = 40, x1 = s.Width - 40, y = s.Height * 0.62;
                dc.DrawLine(Draw.Pen(Draw.Dim, 1.5, true), new Point(x0, y), new Point(x1, y));
                dc.DrawEllipse(Draw.Blue, null, new Point(x0, y), 6, 6);
                Draw.Text(dc, "base", x0, y + 10, 10, Draw.Dim, 1);
                dc.DrawEllipse(Draw.Orange, null, new Point(x1, y), 6, 6);
                Draw.Text(dc, L.F("ponto ({0} km)", Draw.N(V("route"), "0.0")), x1, y + 10, 10, Draw.Dim, 1);
                double px = x0 + (x1 - x0) * st[1];
                DroneSide(dc, new Point(px, y - 14 - st[3] * 2.2), 0.55, st[0] > 0 && st[1] > 0 && st[1] < 1 ? 8 : 0, T * 60, false);
                Draw.Bar(dc, new Rect(14, 26, 140, 12), st[2], L.F("Bateria ({0} Wh)", Draw.N(_e, "0.0")));
                Draw.Text(dc, L.F("{0} min de voo", Draw.N(st[0] / 60, "0.0")), s.Width - 10, 8, 12, Draw.White, 2, true);
                if (T >= Duration - 1e-6 && T > 0)
                {
                    bool ok = Calc.GetValueOrDefault("missionOk") > 0.5;
                    Draw.Text(dc, ok ? L.T("missão cumprida ✓") : st[2] < 0 ? L.T("bateria acabou no caminho!") : L.T("sem reserva de 20 % / voo impossível"), s.Width - 10, 26, 12, ok ? Draw.Green : Draw.Red, 2, true);
                }
                break;
            }
            default:
            {
                double gy = s.Height - 20;
                dc.DrawRectangle(Draw.Grass, null, new Rect(0, gy, s.Width, s.Height - gy));
                var st = At(_hover, T);
                double twr = Calc.GetValueOrDefault("thrustToWeight");
                DroneSide(dc, new Point(s.Width * 0.4, gy - 18 - st[1] * 22), 1.1, 0, T * 60, twr > 1);
                Draw.Bar(dc, new Rect(14, 26, 140, 12), st[2], L.F("Bateria {0}S {1} mAh", Draw.N(V("cells"), "0"), Draw.N(V("capacity"), "0")));
                Draw.Text(dc, L.F("empuxo/peso = {0}", Draw.N(twr, "0.00")), s.Width - 10, 8, 13, twr >= 2 ? Draw.Green : twr > 1 ? Draw.Orange : Draw.Red, 2, true);
                Draw.Text(dc, L.F("massa {0} g · {1} A", Draw.N(_m * 1000, "0"), Draw.N(Calc.GetValueOrDefault("hoverCurrent"), "0.0")), s.Width - 10, 26, 11, Draw.Dim, 2);
                Draw.Text(dc, $"{Draw.N(st[0] / 60, "0.0")} min", s.Width - 10, 44, 11, Draw.Dim, 2);
                if (twr <= 1 && T > 0.3) Draw.Text(dc, L.T("não decola: empuxo menor que o peso"), 14, 48, 12, Draw.Red, 0, true);
                break;
            }
        }
    }

    protected override void DrawPlot(DrawingContext dc, Size s)
    {
        switch (Mode)
        {
            case "altitude":
            case "attitude":
            {
                double target = Mode == "attitude" ? V("angle") : V("target");
                double ymax = Draw.Nice(Math.Max(target * 1.5, _step.Max(r => r[1]) * 1.05));
                var p = new Plot(dc, s, 0, _step[^1][0], Mode == "attitude" && _step.Min(r => r[1]) < -0.5 ? -Draw.Nice(-_step.Min(r => r[1])) : 0, ymax,
                    L.T("tempo (s)"), Mode == "attitude" ? L.T("rolagem (°)") : L.T("altura (m)"));
                p.HLine(dc, target, Draw.Orange, "alvo");
                p.HLine(dc, target * 1.05, Draw.GridLine);
                var pts = _step.Where(r => r[0] <= T).Select(r => new Point(r[0], r[1])).ToList();
                p.Line(dc, pts, Draw.Green);
                break;
            }
            case "mission":
            {
                double ymax = Draw.Nice(_curve.Count > 0 ? _curve.Where(c => c.X >= 3).Max(c => c.Y) * 1.1 : 1);
                var p = new Plot(dc, s, 0, 25, 0, ymax, L.T("velocidade (m/s)"), L.T("energia por km (Wh/km)"));
                p.Line(dc, _curve, Draw.Blue);
                p.Dot(dc, V("speed"), Calc.GetValueOrDefault("cruisePower") / V("speed") * 1000 / 3600, Draw.Yellow);
                break;
            }
            default:
            {
                bool byProp = LP("plot") == "prop";
                double ymax = Draw.Nice(_curve.Count > 0 ? _curve.Max(c => c.Y) * 1.1 : 1);
                var p = new Plot(dc, s, _curve.Count > 0 ? _curve[0].X : 0, _curve.Count > 0 ? _curve[^1].X : 1, 0, ymax,
                    byProp ? L.T("hélice (pol)") : L.T("capacidade (mAh)"), L.T("tempo de voo (min)"));
                p.Line(dc, _curve, Draw.Blue);
                p.Dot(dc, byProp ? V("propSize") : V("capacity"), _flight, Draw.Yellow);
                break;
            }
        }
    }

    protected override string Summary()
    {
        string run = Ran ? "" : L.T(" Clique em ▶ Rodar.");
        return Mode switch
        {
            "altitude" or "attitude" => Ran
                ? L.F("Sobressinal {0} % · acomodação {1} · erro final {2} {3}", Draw.N(_overshoot, "0"), (_settle < 99 ? Draw.N(_settle, "0.0") + " s" : L.T("não acomodou")), Draw.N(Calc.GetValueOrDefault("steadyError"), "0.00"), (Mode == "attitude" ? "°" : "m"))
                : L.T("Ajuste Kp, Ki e Kd e clique em ▶ Rodar para ver a resposta ao degrau."),
            "mission" => L.F("Ida e volta de {0} km · {1} min · usa {2} % da bateria (máx. 80 %) · inclinação {3}°.{4}", Draw.N(2 * V("route"), "0.0"), Draw.N(_missionMin, "0.0"), Draw.N(_energyPct, "0"), Draw.N(Calc.GetValueOrDefault("tiltAngle"), "0"), run),
            _ => L.F("Massa {0} g (bateria {1} g) · empuxo/peso {2} · pairar: {3} % e {4} W · tempo de voo {5} min.{6}", Draw.N(_m * 1000, "0"), Draw.N(_mBat * 1000, "0"), Draw.N(Calc.GetValueOrDefault("thrustToWeight"), "0.00"), Draw.N(Calc.GetValueOrDefault("hoverThrottle"), "0"), Draw.N(_pHover, "0"), Draw.N(_flight, "0.0"), run),
        };
    }
}
