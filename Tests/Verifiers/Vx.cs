using System.Globalization;
using System.Text.RegularExpressions;

namespace EasyCreator.Verifiers;

/// <summary>Helpers for verifiers: read the numbers of a Portuguese prompt ("1,5", "2.000", "1,6·10⁻¹⁹", "−3") in order,
/// and match the prompt against patterns.</summary>
public static class Vx
{
    static readonly Regex Number = new(@"[−-]?\d+(?:[.,]\d+)*(?:\s*·\s*10[⁻⁰¹²³⁴⁵⁶⁷⁸⁹]+)?", RegexOptions.Compiled);
    const string Sup = "⁰¹²³⁴⁵⁶⁷⁸⁹";

    /// <summary>Every number in the text, in order. "1,5" = 1.5; a comma followed by exactly three digits after a
    /// number with no other separators is still read as a decimal (prompts never use thousands separators in pt).</summary>
    public static List<double> Nums(string text)
    {
        var list = new List<double>();
        foreach (Match m in Number.Matches(text ?? ""))
        {
            var s = m.Value.Replace('−', '-').Replace(" ", "");
            double exp = 0;
            int dot = s.IndexOf("·10", StringComparison.Ordinal);
            if (dot >= 0)
            {
                var e = s[(dot + 3)..];
                int sign = e.StartsWith('⁻') ? -1 : 1;
                exp = sign * int.Parse(string.Concat(e.TrimStart('⁻').Select(c => (char)('0' + Sup.IndexOf(c)))), CultureInfo.InvariantCulture);
                s = s[..dot];
            }
            s = s.Replace(',', '.');
            if (double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) list.Add(v * Math.Pow(10, exp));
        }
        return list;
    }

    /// <summary>True when the prompt contains every one of the given fragments.</summary>
    public static bool Has(string prompt, params string[] parts) => parts.All(p => prompt.Contains(p, StringComparison.OrdinalIgnoreCase));

    /// <summary>Numbers captured by a regex with groups written as (?&lt;n&gt;…) or plain groups, read like <see cref="Nums"/>.</summary>
    public static double[]? Match(string prompt, string pattern)
    {
        var m = Regex.Match(prompt, pattern, RegexOptions.Singleline);
        if (!m.Success) return null;
        return m.Groups.Cast<Group>().Skip(1).Select(g => Nums(g.Value).FirstOrDefault()).ToArray();
    }
}
