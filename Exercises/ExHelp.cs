using System.Globalization;

namespace EasyCreator.Exercises;

/// <summary>Small helpers shared by the generators.</summary>
static class P
{
    public const double G = 9.8;
    public static double Rad(double deg) => deg * Math.PI / 180;
    public static double Deg(double rad) => rad * 180 / Math.PI;
    public static string N(double v, int d = 2) => Ex.N(v, d);
    /// <summary>Scientific notation for display, e.g. 3,986·10¹⁴ is shown as 3.99E+14 in the language's decimal mark.</summary>
    public static string Sci(double v)
    {
        var s = v.ToString("0.###E+0", CultureInfo.InvariantCulture);
        return L.Lang == "en" ? s : s.Replace('.', ',');
    }
}
