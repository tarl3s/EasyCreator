namespace EasyCreator.ProjectLabs;

/// <summary>Vertical motion of a small submarine / ROV: Archimedes buoyancy (partly out of the water at the surface),
/// weight including flooded ballast, quadratic drag, added mass, a vertical thruster (fixed force or a PID depth
/// controller), the sea floor, and hull collapse when the water pressure passes the hull rating.
/// Semi-implicit Euler with a 10 ms step. The hull is a cylinder 6 diameters long, lying horizontally.</summary>
public sealed class SubmarineSim
{
    public const double G = 9.81, P0 = 101325, LeadDensity = 11340;

    // ---- inputs ----
    public double HullL = 20, HullKg = 21, BallastL = 2, FillPct, TrimG, KeelG, PayloadKg, ThrustN, TargetM = 10,
        Kp, Ki, Kd, RatingBar = 3, BlowAt, ForwardN = 10, Floor = 30, Start, Duration = 120, HullCgCm = -0.5;
    public bool Salt, Pid;
    public string Shape = "torpedo";

    // ---- results ----
    public readonly List<(double t, double y)> Depth = new();
    public double NetBuoyancy, Buoyancy, Weight, MassKg, MeanDensity, FinalDepth, MaxDepth, PressureBar, CrushDepth,
        SafetyFactor, SettlingTime, Overshoot, MaxSpeed, CruiseSpeed, PropPower, BgCm, CrushTime = double.NaN, DiameterM, LengthM;
    public bool Crushed;

    public double Rho => Salt ? 1025 : 1000;
    double V => HullL / 1000;

    public double FillAt(double t) =>
        BlowAt > 0 && t >= BlowAt ? FillPct / 100 * Math.Max(0, 1 - (t - BlowAt) / 4) : FillPct / 100; // blowing takes ~4 s

    public double MassAt(double t) => HullKg + PayloadKg + (TrimG + KeelG) / 1000 + Rho * BallastL / 1000 * Math.Min(FillAt(t), 1);

    public double AxialCd => Shape switch { "box" => 1.05, "cylinder" => 0.85, _ => 0.10 };

    public void ComputeStatic()
    {
        DiameterM = Math.Pow(4 * V / (6 * Math.PI), 1.0 / 3);
        LengthM = 6 * DiameterM;
        MassKg = MassAt(0);
        Buoyancy = Rho * G * V;          // lead is inside the hull: it adds mass, not volume
        Weight = MassKg * G;
        NetBuoyancy = Buoyancy - Weight; // > 0 rises, < 0 sinks
        MeanDensity = MassKg / V;
        CrushDepth = RatingBar * 1e5 / (Rho * G);
        double aFront = Math.PI * DiameterM * DiameterM / 4;
        CruiseSpeed = ForwardN <= 0 ? 0 : Math.Sqrt(2 * ForwardN / (Rho * AxialCd * aFront));
        PropPower = ForwardN * CruiseSpeed / 0.4; // ~40 % from battery to useful thrust power
        // Heights (cm, + up) from the center of buoyancy (the hull's center): hull+payload at HullCgCm, trim lead and
        // ballast water at the center, keel lead at the bottom of the hull.
        double keelH = -0.5 * DiameterM * 100;
        double cg = ((HullKg + PayloadKg) * HullCgCm + KeelG / 1000 * keelH) / MassKg;
        BgCm = -cg; // > 0: CG below CB = stable
    }

    public void Run()
    {
        ComputeStatic();
        Depth.Clear();
        Crushed = false; CrushTime = double.NaN;
        double rho = Rho, d = DiameterM, aSide = LengthM * d, cdSide = 1.1, added = rho * V;
        double zMax = Floor - d / 2, k = rho * G * V / d;
        double z = Math.Clamp(Start, -d / 2, zMax), v = 0, integ = 0, tMax = Math.Abs(ThrustN);
        const double dt = 0.01;
        MaxDepth = Math.Max(z, 0); MaxSpeed = 0;
        Depth.Add((0, Math.Max(z, 0)));
        double t = 0;
        for (int i = 1; t < Duration - 1e-9; i++)
        {
            t = i * dt;
            double m = MassAt(t), mt = m + added;
            double sub = Math.Clamp((z + d / 2) / d, 0, 1);
            double force = m * G - rho * G * V * sub;                       // + down
            double thrust = ThrustN;
            if (Pid)
            {
                double e = TargetM - z;
                double u = Kp * e + Ki * integ - Kd * v;
                thrust = Math.Clamp(u, -tMax, tMax);
                if (Math.Abs(u) < tMax || Math.Sign(e) != Math.Sign(u)) integ += e * dt; // no wind-up while saturated
            }
            thrust *= sub; // a thruster out of the water pushes nothing
            force += thrust;
            force -= 0.5 * rho * cdSide * aSide * v * Math.Abs(v);
            if (sub < 1) force -= Math.Sqrt(k * mt) * v;                   // waves damp the bobbing at the surface
            v += force / mt * dt;
            z += v * dt;
            if (z >= zMax) { z = zMax; if (v > 0) v = 0; }
            MaxDepth = Math.Max(MaxDepth, z);
            MaxSpeed = Math.Max(MaxSpeed, Math.Abs(v));
            if (rho * G * Math.Max(z, 0) > RatingBar * 1e5) { Crushed = true; CrushTime = t; Depth.Add((t, z)); break; }
            if (i % 10 == 0) Depth.Add((t, Math.Max(z, 0)));
        }
        FinalDepth = Math.Max(z, 0);
        PressureBar = (P0 + rho * G * MaxDepth) / 1e5;
        SafetyFactor = MaxDepth < 0.01 ? 99 : Math.Min(99, RatingBar * 1e5 / (rho * G * MaxDepth));
        double reference = Pid ? TargetM : FinalDepth, band = Math.Max(0.02 * Math.Abs(reference), 0.1);
        SettlingTime = 0;
        foreach (var (ts, y) in Depth) if (Math.Abs(y - reference) > band) SettlingTime = ts;
        if (Crushed || (Depth.Count > 0 && Math.Abs(Depth[^1].y - reference) > band)) SettlingTime = 999;
        Overshoot = Pid ? (TargetM > 0 ? Math.Max(0, MaxDepth - TargetM) / TargetM * 100 : 0)
                        : (FinalDepth > 0.5 ? Math.Max(0, MaxDepth - FinalDepth) / FinalDepth * 100 : 0);
    }
}
