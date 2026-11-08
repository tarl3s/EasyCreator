namespace EasyCreator.Exercises;

/// <summary>Shared constants and small helpers for the rocket and submarine generators.</summary>
static class Pk
{
    public const double G = 9.8, Rho = 1.2, RhoW = 1000, RhoSea = 1025, P0 = 101325, Mu = 3.986e14, Re = 6371e3;
    public static string Cap(string s) => s.Length == 0 ? s : char.ToUpper(s[0]) + s[1..];
    public static double Area(double dm) => Math.PI * dm * dm / 4;
}
