namespace EasyCreator.Exercises;

/// <summary>Small helpers for the cars, aircraft and drones generators.</summary>
static class Pv
{
    public static string N(double v, int decimals = 2) => Ex.N(v, decimals);

    /// <summary>Root of a monotonic function by bisection on [lo, hi] (assumes f(lo) and f(hi) have opposite signs).</summary>
    public static double Solve(Func<double, double> f, double lo, double hi)
    {
        double flo = f(lo);
        for (int i = 0; i < 100; i++)
        {
            double mid = (lo + hi) / 2, fm = f(mid);
            if ((fm < 0) == (flo < 0)) { lo = mid; flo = fm; } else hi = mid;
        }
        return (lo + hi) / 2;
    }

    /// <summary>Steady speed (m/s) where wheel power P (W) balances air drag and rolling resistance.</summary>
    public static double TopSpeed(double p, double cd, double a, double m, double crr) =>
        Solve(v => p - (0.5 * Pk.Rho * cd * a * v * v + crr * m * Pk.G) * v, 0.1, 400);

    public static double Kmh(double ms) => ms * 3.6;
    public static double Ms(double kmh) => kmh / 3.6;
}

/// <summary>Standard-atmosphere helpers for the aircraft generators.</summary>
static class Isa
{
    /// <summary>Air density (kg/m³) at a geopotential altitude in metres (troposphere, ISA).</summary>
    public static double Rho(double h) => 1.225 * Math.Pow(1 - 2.25577e-5 * h, 4.25588);
    public static double Sigma(double h) => Rho(h) / 1.225;
    /// <summary>Altitude (m) where the density ratio is sigma.</summary>
    public static double AltOfSigma(double sigma) => (1 - Math.Pow(sigma, 1 / 4.25588)) / 2.25577e-5;
    public static double Temp(double h) => 288.15 - 0.0065 * h;
    public static double Sound(double h) => Math.Sqrt(1.4 * 287.05 * Temp(h));
}

/// <summary>Multirotor helpers for the drone generators.</summary>
static class Mr
{
    public const double Inch = 0.0254, CellV = 3.7;
    /// <summary>Disc area (m²) of a propeller of the given diameter in inches.</summary>
    public static double PropArea(double dIn) => Math.PI * Math.Pow(dIn * Inch / 2, 2);
    /// <summary>Ideal hover power (W) for total thrust t (N) over total disc area a (m²) at density rho.</summary>
    public static double IdealP(double t, double a, double rho = 1.2) => Math.Pow(t, 1.5) / Math.Sqrt(2 * rho * a);
    /// <summary>Electrical hover power (W) of a multirotor of mass m (kg) with n props of d inches.</summary>
    public static double HoverP(double m, int n, double dIn, double fm = 0.6, double eta = 0.85) =>
        IdealP(m * Pk.G, n * PropArea(dIn)) / (fm * eta);
}
