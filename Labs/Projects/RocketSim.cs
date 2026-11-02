namespace EasyCreator.ProjectLabs;

/// <summary>A model-rocket motor from the catalog (approximate published values of common Estes/AeroTech motors).
/// The thrust curve is the classic shape — a spike at ignition, then a plateau — scaled so its area is exactly
/// <see cref="Impulse"/>. Propellant burns in proportion to the impulse delivered (constant Isp).</summary>
public sealed record MotorSpec(string Id, string Label, double Impulse, double BurnTime, double Peak, double PropKg,
    double TotalKg, double LenMm, double DiaMm)
{
    public double CaseKg => TotalKg - PropKg;
    public double AvgThrust => Impulse / BurnTime;
    public double Isp => Impulse / (PropKg * RocketSim.G0);

    public static readonly MotorSpec[] Catalog =
    {
        new("A8", "A8 · 2,5 N·s", 2.5, 0.73, 10.7, 0.00312, 0.0162, 70, 18),
        new("B6", "B6 · 5 N·s", 5.0, 0.86, 12.0, 0.0062, 0.0198, 70, 18),
        new("C6", "C6 · 10 N·s", 10.0, 1.86, 14.0, 0.0123, 0.0250, 70, 18),
        new("D12", "D12 · 16,8 N·s", 16.8, 1.65, 29.7, 0.0211, 0.0440, 70, 24),
        new("E9", "E9 · 28,5 N·s", 28.5, 3.0, 19.5, 0.0358, 0.0570, 95, 24),
        new("F15", "F15 · 49,6 N·s", 49.6, 3.45, 25.3, 0.060, 0.1016, 114, 29),
        new("G40", "G40 · 120 N·s", 120, 3.0, 65, 0.062, 0.125, 124, 29),
    };

    public static MotorSpec? Find(string id) => Catalog.FirstOrDefault(m => m.Id == id);
}

/// <summary>A burning stage: either a catalog motor (thrust curve) or a "custom" engine (Isp + mass flow, constant thrust).</summary>
public sealed class Engine
{
    public double BurnTime, PropKg, CaseKg, LenMm;
    readonly double[] _tk, _fk;   // thrust curve knots (s, N)
    readonly double[] _ck;        // cumulative impulse at each knot

    public double Impulse => _ck[^1];

    Engine(double[] tk, double[] fk)
    {
        _tk = tk; _fk = fk;
        _ck = new double[tk.Length];
        for (int i = 1; i < tk.Length; i++) _ck[i] = _ck[i - 1] + (fk[i] + fk[i - 1]) / 2 * (tk[i] - tk[i - 1]);
    }

    public static Engine FromMotor(MotorSpec m)
    {
        // Area of the shape (0,0)→(0.08,peak)→(0.25,p·peak)→(0.9,p·peak)→(1,0) = peak·tb·(0.125 + 0.785·p).
        double p = Math.Clamp((m.Impulse / (m.Peak * m.BurnTime) - 0.125) / 0.785, 0.05, 1);
        double tb = m.BurnTime, pk = m.Peak;
        var e = new Engine(new[] { 0, 0.08 * tb, 0.25 * tb, 0.9 * tb, tb }, new[] { 0, pk, p * pk, p * pk, 0 })
        { BurnTime = tb, PropKg = m.PropKg, CaseKg = m.CaseKg, LenMm = m.LenMm };
        double k = m.Impulse / e.Impulse; // exact catalog impulse
        for (int i = 0; i < e._fk.Length; i++) e._fk[i] *= k;
        for (int i = 0; i < e._ck.Length; i++) e._ck[i] *= k;
        return e;
    }

    /// <summary>Constant thrust F = Isp·g0·ṁ until the propellant is gone (burn time = propellant / ṁ).</summary>
    public static Engine Custom(double isp, double massFlow, double propKg, double lenMm)
    {
        massFlow = Math.Max(massFlow, 1e-6);
        double tb = Math.Max(propKg / massFlow, 1e-3), f = isp * RocketSim.G0 * massFlow;
        return new Engine(new[] { 0, tb }, new[] { f, f }) { BurnTime = tb, PropKg = propKg, CaseKg = 0, LenMm = lenMm };
    }

    public double Thrust(double t)
    {
        if (t < 0 || t >= BurnTime) return 0;
        int i = 1;
        while (i < _tk.Length - 1 && _tk[i] < t) i++;
        double a = (t - _tk[i - 1]) / Math.Max(_tk[i] - _tk[i - 1], 1e-12);
        return _fk[i - 1] + a * (_fk[i] - _fk[i - 1]);
    }

    /// <summary>Propellant burned by time t (proportional to the impulse delivered so far).</summary>
    public double Burned(double t)
    {
        if (t <= 0) return 0;
        if (t >= BurnTime) return PropKg;
        int i = 1;
        while (i < _tk.Length - 1 && _tk[i] < t) i++;
        double dt = t - _tk[i - 1], f0 = _fk[i - 1];
        double slope = (_fk[i] - f0) / Math.Max(_tk[i] - _tk[i - 1], 1e-12);
        double imp = _ck[i - 1] + f0 * dt + slope * dt * dt / 2;
        return PropKg * imp / Math.Max(Impulse, 1e-12);
    }
}

/// <summary>Vertical (1-D) flight of a model or sounding rocket, integrated with RK4: gravity that weakens with altitude,
/// thrust curve, mass that drops as propellant burns, drag ½·ρ·v²·Cd·A with an exponential atmosphere and a simple
/// transonic rise, optional second stage, parachute. Stability uses a simplified Barrowman calculation.</summary>
public sealed class RocketSim
{
    public const double G0 = 9.80665, Re = 6.371e6, Rho0 = 1.225, ScaleH = 8500, Pa = 101325, RhoWater = 1000;

    // ---- inputs ----
    public string Motor = "B6", Motor2 = "C6", Nose = "ogive";
    public double DryKg = 0.05, NoseWeightG, DiameterMm = 25, LengthMm = 400, FinSpanMm = 40, ChuteCm = 30, DelayS = 4;
    public int Fins = 3, Stages = 1;
    /// <summary>Thrust-vector control (big rockets steer with the engine instead of fins): never tumbles.</summary>
    public bool Guided;
    public double BoosterKg = 0.02;
    public double Isp = 200, MassFlow = 0.02, PropKg = 0.05, Isp2 = 200, MassFlow2 = 0.02, PropKg2 = 0.05;
    public double PressureBar = 3, WaterL = 0.5, BottleL = 2, NozzleMm = 22;

    // ---- results ----
    public readonly List<(double t, double y)> Altitude = new();
    public double Apogee, ApogeeTime, MaxSpeed, MaxAccelG, BurnoutTime, Stage1Time, DeployTime = double.NaN, DeploySpeed,
        LandingSpeed, FlightTime, DeltaV, LiftoffKg, AvgThrust, ThrustToWeight, TotalImpulse, Margin, Cd, CgMm, CpMm, Isp1;
    public bool Escaped, NeverLifted;
    public string Stability = "";

    public bool IsWater => Motor == "water";
    bool TwoStage => Stages == 2 && !IsWater;
    double D => DiameterMm / 1000;
    double AreaRef => Math.PI * D * D / 4;

    public static double Gravity(double h) { double r = Re / (Re + Math.Max(h, 0)); return G0 * r * r; }
    public static double AirDensity(double h) => h > 150000 ? 0 : Rho0 * Math.Exp(-Math.Max(h, 0) / ScaleH);

    Engine MakeEngine(string id, double isp, double flow, double prop) =>
        id == "custom" || MotorSpec.Find(id) is not { } m ? Engine.Custom(isp, flow, prop, LengthMm * 0.5) : Engine.FromMotor(m);

    double NoseLenMm => Nose switch
    {
        "flat" => 0,
        "round" => Math.Min(DiameterMm, 0.3 * LengthMm),
        _ => Math.Min(3 * DiameterMm, 0.3 * LengthMm),
    };

    public (double cr, double ct, double sweep) FinGeometry()
    {
        double cr = Math.Min(2 * DiameterMm, 0.3 * LengthMm);
        return (cr, cr / 2, cr / 2);
    }

    /// <summary>Drag coefficient at low speed: nose shape + base drag + skin friction of body and fins.</summary>
    public double BaseCd()
    {
        double nose = Nose switch { "cone" => 0.15, "round" => 0.25, "flat" => 0.55, _ => 0.10 };
        double body = 0.003 * 4 * LengthMm / DiameterMm;
        var (cr, ct, _) = FinGeometry();
        double finArea = FinSpanMm * (cr + ct) / 2 * Fins;                       // mm², one side
        double fins = 0.006 * finArea / (Math.PI * DiameterMm * DiameterMm / 4);
        return nose + 0.12 + body + fins;
    }

    /// <summary>Center of pressure (Barrowman: nose + fins, the body tube adds nothing at small angles), in mm from the tip.</summary>
    public double CenterOfPressure()
    {
        double ln = NoseLenMm, d = DiameterMm, r = d / 2, s = FinSpanMm;
        double xn = Nose switch { "cone" => 0.666 * ln, "round" => 0.333 * ln, "flat" => 0, _ => 0.466 * ln };
        const double cnN = 2;
        var (cr, ct, xr) = FinGeometry();
        double lm = Math.Sqrt(s * s + Math.Pow(xr + ct / 2 - cr / 2, 2));
        double cnF = s <= 0 ? 0 : 4 * Fins * Math.Pow(s / d, 2) / (1 + Math.Sqrt(1 + Math.Pow(2 * lm / (cr + ct), 2))) * (1 + r / (s + r));
        double xb = LengthMm - cr;
        double xf = xb + xr / 3 * (cr + 2 * ct) / (cr + ct) + (cr + ct - cr * ct / (cr + ct)) / 6;
        return (cnN * xn + cnF * xf) / (cnN + cnF);
    }

    /// <summary>Center of gravity at liftoff (mm from the tip) and liftoff mass of the upper (or only) stage.</summary>
    public (double cg, double kg) CenterOfGravity()
    {
        double L = LengthMm, nw = NoseWeightG / 1000;
        double sum = DryKg * 0.5 * L, kg = DryKg;
        if (nw > 0) { sum += nw * Math.Max(0.6 * NoseLenMm, 0.3 * DiameterMm); kg += nw; }
        // A water rocket throws its water out in about 0.1 s, still guided by the launch tube: what must be stable is the
        // empty bottle, so the water is left out of the CG.
        if (!IsWater)
        {
            string id = TwoStage ? Motor2 : Motor;
            var e = TwoStage ? MakeEngine(Motor2, Isp2, MassFlow2, PropKg2) : MakeEngine(Motor, Isp, MassFlow, PropKg);
            double m = e.PropKg + e.CaseKg;
            double x = id == "custom" ? 0.72 * L : L - e.LenMm / 2;
            sum += m * x; kg += m;
        }
        return (sum / kg, kg);
    }

    public void ComputeStatic()
    {
        Cd = BaseCd();
        CpMm = CenterOfPressure();
        (CgMm, _) = CenterOfGravity();
        Margin = (CpMm - CgMm) / DiameterMm;
        Stability = Margin < 0 ? "instável" : Margin < 1 ? "pouco estável" : Margin <= 3 ? "estável" : "estável demais";
        if (Guided) Stability = "guiado por empuxo vetorado";
        var e1 = IsWater ? null : MakeEngine(Motor, Isp, MassFlow, PropKg);
        var e2 = TwoStage ? MakeEngine(Motor2, Isp2, MassFlow2, PropKg2) : null;
        LiftoffKg = DryKg + NoseWeightG / 1000 + (IsWater ? WaterL * RhoWater / 1000
            : e1!.PropKg + e1.CaseKg + (e2 == null ? 0 : e2.PropKg + e2.CaseKg + BoosterKg));
        if (e1 != null)
        {
            AvgThrust = e1.Impulse / e1.BurnTime;
            ThrustToWeight = AvgThrust / (LiftoffKg * G0);
            TotalImpulse = e1.Impulse + (e2?.Impulse ?? 0);
            Isp1 = e1.Impulse / Math.Max(e1.PropKg * G0, 1e-12);
            // Tsiolkovsky per stage: Δv = ve·ln(m0/mf), ve = impulse / propellant (= Isp·g0)
            double m0 = LiftoffKg, ve1 = e1.Impulse / Math.Max(e1.PropKg, 1e-12);
            DeltaV = ve1 * Math.Log(m0 / (m0 - e1.PropKg));
            if (e2 != null)
            {
                double m02 = DryKg + NoseWeightG / 1000 + e2.PropKg + e2.CaseKg, ve2 = e2.Impulse / Math.Max(e2.PropKg, 1e-12);
                DeltaV += ve2 * Math.Log(m02 / (m02 - e2.PropKg));
            }
        }
        else
        {
            double dp = PressureBar * 1e5, an = Math.PI * Math.Pow(NozzleMm / 2000, 2);
            AvgThrust = 2 * an * dp; // at ignition
            ThrustToWeight = AvgThrust / (LiftoffKg * G0);
        }
    }

    /// <summary>Runs the flight until landing (or 40 000 s).</summary>
    public void Run()
    {
        ComputeStatic();
        if (IsWater) TotalImpulse = DeltaV = 0;
        Altitude.Clear();
        Apogee = ApogeeTime = MaxSpeed = MaxAccelG = DeploySpeed = LandingSpeed = 0;
        DeployTime = double.NaN; Escaped = NeverLifted = false;

        var e1 = IsWater ? null : MakeEngine(Motor, Isp, MassFlow, PropKg);
        var e2 = TwoStage ? MakeEngine(Motor2, Isp2, MassFlow2, PropKg2) : null;
        double nose = NoseWeightG / 1000;
        double upperKg = DryKg + nose + (e2 == null ? 0 : e2.PropKg + e2.CaseKg);
        double boosterDry = e2 == null ? 0 : BoosterKg;
        double an = Math.PI * Math.Pow(NozzleMm / 2000, 2);
        double waterL = Math.Clamp(WaterL, 0, BottleL - 0.05), air0 = (BottleL - waterL) / 1000; // m³
        double pAbs0 = Pa + PressureBar * 1e5;

        Stage1Time = IsWater ? 0 : e1!.BurnTime;
        BurnoutTime = Stage1Time + (e2?.BurnTime ?? 0);
        // Tumbling (negative margin) wastes thrust and multiplies drag; a small margin wobbles (more drag).
        double thrustEff = Margin < 0 && !Guided ? 0.25 : 1, dragMul = Guided ? 1 : Margin < 0 ? 3 : Margin < 1 ? 1 + 0.6 * (1 - Margin) : 1;
        double chuteArea = Math.PI * Math.Pow(ChuteCm / 200, 2), chuteCd = 0.8;

        double ThrustAt(double t, double w, out bool burning)
        {
            burning = false;
            if (IsWater)
            {
                if (w <= 1e-9) return 0;
                double air = Math.Max(BottleL / 1000 - w, 1e-6), p = pAbs0 * Math.Pow(air0 / air, 1.4);
                if (p <= Pa) return 0;
                burning = true;
                return 2 * an * (p - Pa);
            }
            if (t < e1!.BurnTime) { burning = true; return e1.Thrust(t); }
            if (e2 != null && t < BurnoutTime) { burning = true; return e2.Thrust(t - e1.BurnTime); }
            return 0;
        }
        double MassAt(double t, double w)
        {
            if (IsWater) return DryKg + nose + w * RhoWater;
            if (t < e1!.BurnTime) return upperKg + boosterDry + e1.CaseKg + e1.PropKg - e1.Burned(t);
            if (e2 != null) return upperKg - e2.Burned(t - e1.BurnTime);
            return DryKg + nose + e1.CaseKg;
        }
        double WaterRate(double w)
        {
            if (!IsWater || w <= 1e-9) return 0;
            double air = Math.Max(BottleL / 1000 - w, 1e-6), p = pAbs0 * Math.Pow(air0 / air, 1.4);
            return p <= Pa ? 0 : -an * Math.Sqrt(2 * (p - Pa) / RhoWater);
        }
        (double dh, double dv, double dw) Deriv(double t, double h, double v, double w, bool onPad)
        {
            double f = ThrustAt(t, w, out _) * thrustEff, m = MassAt(t, w);
            double rho = AirDensity(h), mach = Math.Abs(v) / 340;
            double cd = Cd * dragMul * (mach < 0.8 ? 1 : mach < 1.05 ? 1 + (mach - 0.8) * 4 : Math.Max(1.3, 2 - (mach - 1.05) * 0.5));
            double area = cd * AreaRef;
            if (!double.IsNaN(DeployTime) && t >= DeployTime) area += chuteCd * chuteArea * Math.Clamp((t - DeployTime) / 0.4, 0, 1);
            double drag = 0.5 * rho * v * Math.Abs(v) * area;
            double a = (f - drag) / m - Gravity(h);
            if (onPad && a < 0) a = 0;
            return (onPad && a <= 0 ? 0 : v, a, WaterRate(w));
        }

        double t = 0, hh = 0, vv = 0, ww = IsWater ? waterL / 1000 : 0, lastRec = -1;
        bool lifted = false, ascending = true, deployed = false;
        double tDeploy = BurnoutTime + DelayS;
        const double tMax = 40000;
        Altitude.Add((0, 0));
        while (t < tMax)
        {
            bool burning = ThrustAt(t, ww, out _) > 0 || (!IsWater && t < BurnoutTime);
            if (IsWater && !burning && !deployed && double.IsNaN(DeployTime) && BurnoutTime == 0 && t > 0)
            {
                BurnoutTime = t;
                tDeploy = t + DelayS;
            }
            if (!deployed && ChuteCm > 0 && t >= tDeploy && (!IsWater || BurnoutTime > 0))
            {
                deployed = true; DeployTime = t; DeploySpeed = Math.Abs(vv);
            }
            var (_, a0, _) = Deriv(t, hh, vv, ww, !lifted);
            double dtMax = burning ? (IsWater ? 0.0002 : 0.002) : hh > 80000 ? 0.5 : 0.02;
            double dt = Math.Clamp(0.05 * (Math.Abs(vv) + 1) / (Math.Abs(a0) + 0.1), 1e-4, dtMax);
            // land exactly on events (stage separation, burnout, chute)
            foreach (var ev in new[] { Stage1Time, BurnoutTime, tDeploy })
                if (ev > t + 1e-9 && ev < t + dt) dt = ev - t;
            bool pad = !lifted;
            double k1T = ThrustAt(t, ww, out _);
            var k1 = Deriv(t, hh, vv, ww, pad);
            var k2 = Deriv(t + dt / 2, hh + dt / 2 * k1.dh, vv + dt / 2 * k1.dv, ww + dt / 2 * k1.dw, pad);
            var k3 = Deriv(t + dt / 2, hh + dt / 2 * k2.dh, vv + dt / 2 * k2.dv, ww + dt / 2 * k2.dw, pad);
            var k4 = Deriv(t + dt, hh + dt * k3.dh, vv + dt * k3.dv, ww + dt * k3.dw, pad);
            double nh = hh + dt / 6 * (k1.dh + 2 * k2.dh + 2 * k3.dh + k4.dh);
            double nv = vv + dt / 6 * (k1.dv + 2 * k2.dv + 2 * k3.dv + k4.dv);
            double nw = Math.Max(0, ww + dt / 6 * (k1.dw + 2 * k2.dw + 2 * k3.dw + k4.dw));
            // water rocket: ideal Δv = thrust/mass integrated (no gravity, no drag); its exhaust speed changes as the air expands
            if (IsWater) TotalImpulse += dt / 6 * (k1T + 4 * ThrustAt(t + dt / 2, (ww + nw) / 2, out _) + ThrustAt(t + dt, nw, out _));
            if (IsWater) DeltaV += dt / 6 * (Thr(t) + 4 * Thr(t + dt / 2) + Thr(t + dt));
            double Thr(double tt) { double w2 = ww + (nw - ww) * (tt - t) / dt; return ThrustAt(tt, w2, out _) / MassAt(tt, w2); }

            if (!lifted && nh > 0) lifted = true;
            if (!lifted) { nh = 0; nv = Math.Max(nv, 0); }
            if (ascending)
            {
                MaxAccelG = Math.Max(MaxAccelG, Math.Abs(a0) / G0);
                if (lifted && nv < 0) ascending = false;
            }
            t += dt;
            if (nh > Apogee) { Apogee = nh; ApogeeTime = t; }
            MaxSpeed = Math.Max(MaxSpeed, Math.Abs(nv));
            if (lifted && nh <= 0)
            {
                double frac = hh / Math.Max(hh - nh, 1e-12);
                LandingSpeed = Math.Abs(vv + (nv - vv) * frac);
                t = t - dt + dt * frac;
                Altitude.Add((t, 0));
                break;
            }
            hh = nh; vv = nv; ww = nw;
            if (!lifted && t > BurnoutTime + 1 && t > 1) { NeverLifted = true; Altitude.Add((t, 0)); break; }
            double recDt = Math.Max(0.01, t * 0.003);
            if (t - lastRec >= recDt) { Altitude.Add((t, hh)); lastRec = t; }
            double escape = Math.Sqrt(2 * G0 * Re * Re / (Re + hh));
            if (!burning && hh > 150000 && vv > escape) { Escaped = true; break; }
        }
        if (IsWater && BurnoutTime == 0) BurnoutTime = t;
        if (IsWater && BurnoutTime > 0) AvgThrust = TotalImpulse / BurnoutTime;
        FlightTime = t;
    }
}
