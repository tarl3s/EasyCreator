using System.Globalization;
using System.Text.RegularExpressions;
using static System.Math;

namespace EasyCreator.Verifiers;

static class Mv
{
    /// <summary>Removes digits that are part of names (t8/5, F3, T0, 3D, EN 1011-2) so the numbers are the data only.</summary>
    public static string C(string p) => p.Replace("t8/5", "tt").Replace("F3", "FF").Replace("T0", "TT").Replace("3D", "D3")
        .Replace("EN 1011-2", "EN").Replace("A2.4", "A").Replace("ISO 2553", "ISO");

    /// <summary>The number written after "name =" (first occurrence).</summary>
    public static double V(string p, string name)
    {
        var m = Regex.Match(p, Regex.Escape(name) + @"\s*=\s*([−-]?\d+(?:[.,]\d+)?)");
        return m.Success ? double.Parse(m.Groups[1].Value.Replace('−', '-').Replace(',', '.'), CultureInfo.InvariantCulture) : double.NaN;
    }

    public static double[] Groups(string p, string pattern)
    {
        var m = Regex.Match(p, pattern);
        if (!m.Success) throw new InvalidOperationException("pattern " + pattern);
        return m.Groups.Cast<Group>().Skip(1).Select(g => double.Parse(g.Value.Replace(',', '.'), CultureInfo.InvariantCulture)).ToArray();
    }

    /// <summary>Heat input in kJ/mm: eta·U·I/(v·1000).</summary>
    public static double Q(double eta, double u, double i, double v) => eta * u * i / (v * 1000);

    /// <summary>EN 1011-2 3D t8/5 (s) for a thick plate.</summary>
    public static double T85(double q, double t0) => (6700 - 5 * t0) * q * (1 / (500 - t0) - 1 / (800 - t0));
}

[Verifies("met-01-estrutura")]
public sealed class VMet01 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        p = Mv.C(p);
        var n = Vx.Nums(p);
        if (Vx.Has(p, "aresta a =", "volume")) return n[0] * n[0] * n[0];
        if (Vx.Has(p, "número de coordenação")) return Vx.Has(p, "(CCC)") ? 8 : 12;
        if (Vx.Has(p, "Quantos átomos pertencem"))
        {
            int per = Vx.Has(p, "(CCC)") ? 2 : Vx.Has(p, "(CFC)") ? 4 : 6;
            return per * n[0];
        }
        if (Vx.Has(p, "fator de empacotamento")) return Vx.Has(p, "CCC") ? PI * Sqrt(3) / 8 : PI * Sqrt(2) / 6;
        if (Vx.Has(p, "parâmetro de rede a (aresta"))
            return Vx.Has(p, "CCC") ? 4 * n[0] / Sqrt(3) : 4 * n[0] / Sqrt(2);
        if (Vx.Has(p, "densidade"))
        {
            double a = n[0] * 1e-7, atoms, A, N;
            if (Vx.Has(p, "hipotético")) { atoms = n[1]; A = n[2]; N = n[3]; }
            else { atoms = Vx.Has(p, "(CCC)") ? 2 : 4; A = n[1]; N = n[2]; }
            return atoms * A / (a * a * a * N);
        }
        if (Vx.Has(p, "ASTM")) return Pow(2, n[^1] - 1);
        if (Vx.Has(p, "Que tamanho de grão")) { double d = Pow(n[1] / (n[2] - n[0]), 2); return d * 1e6; }
        if (Vx.Has(p, "limite de escoamento com grãos")) return n[0] + n[1] / Sqrt(n[2] * 1e-6);
        return null;
    }
}

[Verifies("met-02-tracao")]
public sealed class VMet02 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "alongamento percentual")) return n[1] / n[0] * 100;
        if (Vx.Has(p, "barra redonda", "tensão normal")) return n[1] * 1000 / (PI * n[0] * n[0] / 4);
        if (Vx.Has(p, "retangular", "limite de resistência")) return n[2] * 1000 / (n[0] * n[1]);
        if (Vx.Has(p, "Quantos mm ela se alonga")) return n[2] / (n[0] * 1000) * n[1] * 1000;
        if (Vx.Has(p, "módulo de elasticidade")) return n[0] / (n[1] * 1e-3) / 1000;
        if (Vx.Has(p, "redução de área")) return (1 - n[1] * n[1] / (n[0] * n[0])) * 100;
        if (Vx.Has(p, "tirante")) return Sqrt(4 * n[1] * 1000 * n[2] / (PI * n[0]));
        if (Vx.Has(p, "inicia o escoamento")) return n[0] * PI * n[1] * n[1] / 4 / 1000;
        if (Vx.Has(p, "tensão verdadeira de ruptura")) return n[1] * 1000 / (PI * n[2] * n[2] / 4);
        if (Vx.Has(p, "tensão verdadeira nesse ponto")) return n[0] * (1 + n[1] / 100);
        if (Vx.Has(p, "deformação verdadeira")) return Log(1 + n[0] / 100) * 100;
        return null;
    }
}

[Verifies("met-03-ferro-carbono")]
public sealed class VMet03 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "fração aproximada de perlita")) return (n[0] - n[1]) / (n[2] - n[1]) * 100;
        if (Vx.Has(p, "fração TOTAL de ferrita")) return (n[2] - n[0]) / (n[2] - n[1]) * 100;
        if (Vx.Has(p, "ferrita proeutetoide (formada")) return (n[3] - n[0]) / (n[3] - n[2]) * 100;
        if (Vx.Has(p, "cementita proeutetoide")) return (n[0] - n[1]) / (n[2] - n[1]) * 100;
        if (Vx.Has(p, "Quanto de perlita ele tem")) return (n[2] - n[0]) / (n[2] - n[1]) * 100;
        if (Vx.Has(p, "micrografia")) return n[2] + n[0] / 100 * (n[1] - n[2]);
        if (Vx.Has(p, "Quer-se um aço")) return n[2] - n[0] / 100 * (n[2] - n[1]);
        if (Vx.Has(p, "fração TOTAL de cementita")) return (n[0] - n[1]) / (n[2] - n[1]) * 100;
        return null;
    }
}

[Verifies("met-04-tratamentos")]
public sealed class VMet04 : IExerciseVerifier
{
    static double Interp(double t, (double x, double y)[] pts)
    {
        for (int i = 0; i < pts.Length - 1; i++)
            if (t <= pts[i + 1].x || i == pts.Length - 2)
                return pts[i].y + (pts[i + 1].y - pts[i].y) * (t - pts[i].x) / (pts[i + 1].x - pts[i].x);
        return double.NaN;
    }

    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "Interpolando linearmente"))
            return Interp(n[^1], new (double x, double y)[] { (n[2], n[1]), (n[4], n[3]), (n[6], n[5]), (n[8], n[7]) });
        if (Vx.Has(p, "interpolação linear"))
        {
            var pts = new (double x, double y)[] { (n[3], n[2]), (n[5], n[4]), (n[7], n[6]) };
            double h = n[8];
            for (int i = 0; i < 2; i++)
            {
                double h0 = pts[i].y, h1 = pts[i + 1].y;
                if (h <= Max(h0, h1) && h >= Min(h0, h1) || i == 1) return pts[i].x + (h - h0) / (h1 - h0) * (pts[i + 1].x - pts[i].x);
            }
            return null;
        }
        if (Vx.Has(p, "Qual o valor de P")) return (n[1] + 273.15) * (n[0] + Log10(n[2]));
        if (Vx.Has(p, "Quantas horas")) { double c = n[3], pp = (n[0] + 273.15) * (c + Log10(n[1])); return Pow(10, pp / (n[2] + 273.15) - c); }
        if (Vx.Has(p, "que temperatura (°C)")) { double c = n[3], pp = (n[0] + 273.15) * (c + Log10(n[1])); return pp / (c + Log10(n[2])) - 273.15; }
        return null;
    }
}

[Verifies("met-05-seguranca")]
public sealed class VMet05 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "potência elétrica do arco")) return n[0] * n[1] / 1000;
        if (Vx.Has(p, "cilindro de argônio")) return n[0] * 1000 / n[1];
        if (Vx.Has(p, "espaço confinado")) return n[1] * 1000 / (n[0] * n[2]);
        if (Vx.Has(p, "tensão em vazio")) return n[0] / n[1] * 1000;
        if (Vx.Has(p, "corrente máxima")) return n[0] * Sqrt(n[1] / n[2]);
        if (Vx.Has(p, "ciclo de trabalho permitido")) return Min(n[3], n[1] * Pow(n[0] / n[2], 2));
        return null;
    }
}

[Verifies("met-06-eletrodo")]
public sealed class VMet06 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "potência elétrica do arco")) return n[0] * n[1] / 1000;
        if (Vx.Has(p, "Quantos MPa")) return n[1] * n[3];
        if (Vx.Has(p, "energia por metro")) return n[3] * n[0] * n[1] / n[2];
        if (Vx.Has(p, "Qual o aporte térmico")) return Mv.Q(n[3], n[0], n[1], n[2]);
        if (Vx.Has(p, "que corrente (A)")) return n[2] * 1000 * n[1] / (n[3] * n[0]);
        if (Vx.Has(p, "que velocidade de soldagem")) return n[3] * n[0] * n[1] / (n[2] * 1000);
        if (Vx.Has(p, "Que tensão de arco")) return n[2] * 1000 * n[1] / (n[3] * n[0]);
        if (Vx.Has(p, "eletrodos são necessários")) return Ceiling(n[2] * 1000 / (n[0] * n[1]));
        return null;
    }
}

[Verifies("met-07-mig")]
public sealed class VMet07 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "rolo de")) return n[0] / n[1] * 60;
        if (Vx.Has(p, "área da seção")) return PI * n[0] * n[0] / 4;
        if (Vx.Has(p, "Qual o aporte térmico")) return Mv.Q(n[3], n[0], n[1], n[2]);
        if (Vx.Has(p, "kg de arame fundem")) return PI * n[0] * n[0] / 4 * 1e-6 * n[1] * 60 * n[2] * 1000;
        if (Vx.Has(p, "kg de arame de aço são consumidos")) return n[0] * 1e-6 * n[1] * n[3] * 1000 / (n[2] / 100);
        if (Vx.Has(p, "litros de gás")) return n[2] * (n[0] * 1000 / n[1]) / 60;
        if (Vx.Has(p, "que velocidade")) return n[3] * n[0] * n[1] / (n[2] * 1000);
        if (Vx.Has(p, "Responda o TEMPO"))
        {
            double d = n[0], wfs = n[1], eff = n[2] / 100, len = n[3], area = n[4];
            double rate = PI * d * d / 4 * 1e-6 * wfs * 60 * 7850 * eff;
            return area * 1e-6 * len * 7850 / rate * 60;
        }
        return null;
    }
}

[Verifies("met-08-tig")]
public sealed class VMet08 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "regra prática")) return n[2] * n[3];
        if (Vx.Has(p, "potência elétrica do arco")) return n[0] * n[1] / 1000;
        if (Vx.Has(p, "Qual o aporte") && !Vx.Has(p, "contra")) return Mv.Q(n[3], n[0], n[1], n[2]);
        if (Vx.Has(p, "que velocidade")) return n[3] * n[0] * n[1] / (n[2] * 1000);
        if (Vx.Has(p, "litros de argônio")) return n[2] * (n[0] / n[1]) / 60;
        if (Vx.Has(p, "alumínio de mesma espessura")) return n[0] * n[1] * n[2];
        if (Vx.Has(p, "contra MIG")) return Mv.Q(n[7], n[4], n[5], n[6]) / Mv.Q(n[3], n[0], n[1], n[2]);
        return null;
    }
}

[Verifies("met-09-defeitos")]
public sealed class VMet09 : IExerciseVerifier
{
    const double K = 0.707;
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "garganta efetiva a?")) return K * n[0];
        if (Vx.Has(p, "Que força")) return n[2] * K * n[0] * n[1] / 1000;
        if (Vx.Has(p, "Qual a perna z necessária")) return n[0] / K;
        if (Vx.Has(p, "Qual a perna mínima, em mm?") && Vx.Has(p, "Um filete de")) return n[1] * 1000 / (K * n[0] * n[2]);
        if (Vx.Has(p, "quantas vezes aumenta o volume")) return Pow(n[2] / n[1], 2);
        if (Vx.Has(p, "dois filetes longitudinais")) return n[1] * 1000 / (K * 2 * n[0] * n[2]);
        return null;
    }
}

[Verifies("met-10-engenharia")]
public sealed class VMet10 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "Aporte térmico:")) return Mv.Q(n[3], n[0], n[1], n[2]);
        if (Vx.Has(p, "sem outros elementos")) return n[0] + n[1] / 6;
        if (Vx.Has(p, "CE (IIW) = C + Mn/"))
        {
            double c = n[3], mn = n[4], cr = n[5], mo = n[6], v = n[7], ni = n[8], cu = n[9];
            return c + mn / n[0] + (cr + mo + v) / n[1] + (ni + cu) / n[2];
        }
        if (Vx.Has(p, "velocidade mínima")) return n[3] * n[1] * n[2] / (n[0] * 1000);
        if (Vx.Has(p, "Pcm"))
        {
            double c = n[5], si = n[6], mn = n[7], cu = n[8], cr = n[9], ni = n[10], mo = n[11], v = n[12];
            return c + si / n[0] + (mn + cu + cr) / n[1] + ni / n[2] + mo / n[3] + v / n[4];
        }
        if (Vx.Has(p, "só C e Mn")) return 6 * (n[1] - n[0]);
        if (Vx.Has(p, "CE(A) − CE(B)"))
        {
            double a = n[0] + n[1] / 6 + (n[2] + n[3]) / 5 + n[4] / 15, b = n[5] + n[6] / 6 + (n[7] + n[8]) / 5 + n[9] / 15;
            return a - b;
        }
        return null;
    }
}

[Verifies("met-11-ttt")]
public sealed class VMet11 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        p = Mv.C(p);
        var n = Vx.Nums(p);
        if (Vx.Has(p, "Quanto vale o tt")) return (n[1] - n[2]) / n[0];
        if (Vx.Has(p, "velocidade média de resfriamento")) return (n[0] - n[1]) / n[2];
        if (Vx.Has(p, "Qual a temperatura Ms")) return n[0] - n[1] * n[6] - n[2] * n[7] - n[3] * n[8] - n[4] * n[9] - n[5] * n[10];
        if (Vx.Has(p, "velocidade crítica")) return (n[2] - n[0]) / n[1];
        if (Vx.Has(p, "fração de martensita formada"))
        {
            double k = Abs(n[1]), ms = n[2], t = n[3];
            return (1 - Exp(-k * (ms - t))) * 100;
        }
        if (Vx.Has(p, "austenita retida"))
        {
            double c = n[0], mn = n[1], ms = n[2] - n[3] * c - n[4] * mn, t = n[5], k = Abs(n[7]);
            return Exp(-k * (ms - t)) * 100;
        }
        if (Vx.Has(p, "até que temperatura"))
        {
            double ms = n[0], f = n[1] / 100, k = Abs(n[3]);
            return ms + Log(1 - f) / k;
        }
        return null;
    }
}

[Verifies("met-12-temperabilidade")]
public sealed class VMet12 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "fica a posição J")) return Mv.Groups(p, @"posição J(\d+)")[0] / 16 * 25.4;
        if (Vx.Has(p, "Em qual posição J")) return n[0] / 25.4 * 16;
        if (Vx.Has(p, "dureza Brinell estimada")) return n[0] / n[1];
        if (Vx.Has(p, "estimativa de σr")) return n[0] * n[1];
        if (Vx.Has(p, "Quanto é isso em mm")) return n[0] * n[2];
        if (Vx.Has(p, "Que teor de Mn") || Vx.Has(p, "Qual o DI, em polegadas") || Vx.Has(p, "quanto aumenta o DI"))
        {
            double kMn = Mv.Groups(p, @"\(1 \+ ([\d,]+)·Mn\)")[0];
            double kCr = Mv.Groups(p, @"\(1 \+ ([\d,]+)·Cr\)")[0];
            double kMo = Vx.Has(p, "·Mo)") ? Mv.Groups(p, @"\(1 \+ ([\d,]+)·Mo\)")[0] : 0;
            if (Vx.Has(p, "Qual o DI, em polegadas"))
            {
                var g = Mv.Groups(p, @"DI_base = ([\d,]+) pol, Mn ([\d,]+)%, Cr ([\d,]+)%, Mo ([\d,]+)%");
                return g[0] * (1 + kMn * g[1]) * (1 + kCr * g[2]) * (1 + kMo * g[3]);
            }
            if (Vx.Has(p, "Que teor de Mn"))
            {
                var g = Mv.Groups(p, @"DI_base = ([\d,]+) pol, Cr ([\d,]+)% e Mo ([\d,]+)%.*? a ([\d,]+) pol");
                return (g[3] / (g[0] * (1 + kCr * g[1]) * (1 + kMo * g[2])) - 1) / kMn;
            }
            var h = Mv.Groups(p, @"DI_base = ([\d,]+) pol, Mn ([\d,]+)% e Cr ([\d,]+)%.*? subir ([\d,]+) ponto");
            return ((1 + kMn * (h[1] + h[3])) / (1 + kMn * h[1]) - 1) * 100;
        }
        return null;
    }
}

[Verifies("met-13-simbologia")]
public sealed class VMet13 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        if (Vx.Has(p, "Qual a garganta teórica")) return 0.707 * Vx.Nums(p)[0];
        if (Vx.Has(p, "Que porcentagem"))
        {
            var g = Mv.Groups(p, @"'([\d,]+) ◣ ([\d,]+)-([\d,]+)'");
            return g[1] / g[2] * 100;
        }
        if (Vx.Has(p, "Quantos kg de metal de solda"))
        {
            var g = Mv.Groups(p, @"perna ([\d,]+) mm em ([\d,]+) m de junta");
            double areaCm2 = g[0] * g[0] / 2 / 100, lenCm = g[1] * 100;
            return areaCm2 * lenCm * 7.85 / 1000;
        }
        if (Vx.Has(p, "comprimento total da junta ocupada"))
        {
            var g = Mv.Groups(p, @"'([\d,]+)-([\d,]+)'.*? tem (\d+) segmentos");
            return (g[2] - 1) * g[1] + g[0];
        }
        if (Vx.Has(p, "Dois filetes intermitentes"))
        {
            var g = Mv.Groups(p, @"'([\d,]+) ◣ ([\d,]+)-([\d,]+)'.*? junta de ([\d,]+) mm");
            double len = g[1], pitch = g[2], total = g[3];
            double segs = Floor((total - len) / pitch) + 1;
            return 2 * segs * len;
        }
        return null;
    }
}

[Verifies("met-14-zta")]
public sealed class VMet14 : IExerciseVerifier
{
    static double Duren(double c, double ce, double t) => 2019 * (c * (1 - 0.5 * Log10(t)) + 0.3 * (ce - c)) + 66 * (1 - 0.8 * Log10(t));
    public double? Solve(string p)
    {
        p = Mv.C(p);
        var n = Vx.Nums(p);
        if (Vx.Has(p, "aporte térmico Q, em kJ/mm")) return Mv.Q(n[3], n[0], n[1], n[2]);
        if (Vx.Has(p, "esfria a", "entre")) return (n[1] - n[2]) / n[0];
        if (Vx.Has(p, "Para Q =")) return Mv.T85(Mv.V(p, "Q"), Mv.V(p, "TT"));
        if (Vx.Has(p, "Que aporte térmico Q"))
        {
            double t0 = Mv.V(p, "TT"), t = Mv.V(p, "tt");
            return t / ((6700 - 5 * t0) * (1 / (500 - t0) - 1 / (800 - t0)));
        }
        if (Vx.Has(p, "Responda a DUREZA"))
        {
            double t = Mv.T85(Mv.V(p, "Q"), Mv.Groups(p, @"pré-aquecimento ([0-9,]+) °C")[0]);
            return Duren(Mv.V(p, "C"), Mv.V(p, "CE"), t);
        }
        if (Vx.Has(p, "dureza máxima prevista")) return Duren(Mv.V(p, "C"), Mv.V(p, "CE"), Mv.V(p, "tt"));
        if (Vx.Has(p, "fórmula D3"))
        {
            var g = Mv.Groups(p, @"([\d,]+) V, ([\d,]+) A, ([\d,]+) mm/s \(η = ([\d,]+)\), pré-aquecimento ([\d,]+) °C");
            return Mv.T85(Mv.Q(g[3], g[0], g[1], g[2]), g[4]);
        }
        return null;
    }
}

[Verifies("met-15-inox")]
public sealed class VMet15 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "PREN = Cr") && Vx.Has(p, "Qual o PREN")) return n[2] + n[0] * n[3] + n[1] * n[4];
        if (Vx.Has(p, "Qual o Ni equivalente")) return n[2] + n[0] * n[3] + n[1] * n[4];
        if (Vx.Has(p, "Qual o Cr equivalente")) return n[2] + n[3] + n[0] * n[4] + n[1] * n[5];
        if (Vx.Has(p, "razão Cr eq / Ni eq?"))
        {
            double cr = n[0], si = n[1], ni = n[2], c = n[3], mn = n[4], kSi = n[5], kC = n[6], kMn = n[7];
            return (cr + kSi * si) / (ni + kC * c + kMn * mn);
        }
        if (Vx.Has(p, "que teor de Ni")) return n[1] / n[2] - n[5] * n[3] - n[6] * n[4];
        if (Vx.Has(p, "teor de Cr (%)")) return n[2] - n[3] * n[0] - n[4] * n[1];
        return null;
    }
}

[Verifies("met-16-aluminio")]
public sealed class VMet16 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "livremente")) return n[1] * n[0] * n[2] * 1000;
        if (Vx.Has(p, "mm a mais o alumínio")) return n[0] * (n[1] - n[2]) * n[3] * 1000;
        if (Vx.Has(p, "Que aquecimento")) return n[2] / (n[0] * 1000 * n[1]);
        if (Vx.Has(p, "tensão de compressão")) return n[0] * 1000 * n[1] * n[2];
        if (Vx.Has(p, "quantas vezes a tensão térmica")) return n[1] * n[2] / (n[3] * n[4]);
        return null;
    }
}

[Verifies("met-17-fratura")]
public sealed class VMet17 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "pêndulo Charpy")) return n[0] * n[3] * (n[1] - n[2]);
        if (Vx.Has(p, "qual o K")) return n[0] * n[1] * Sqrt(PI * n[2] * 1e-3);
        if (Vx.Has(p, "tamanho crítico")) return 1 / PI * Pow(n[0] / (n[2] * n[1]), 2) * 1000;
        if (Vx.Has(p, "razão σ_fratura")) return n[1] / (n[3] * Sqrt(PI * n[2] * 1e-3)) / n[0];
        if (Vx.Has(p, "tensão que leva à fratura")) return n[1] / (n[2] * Sqrt(PI * n[0] * 1e-3));
        return null;
    }
}

[Verifies("met-18-fadiga")]
public sealed class VMet18 : IExerciseVerifier
{
    static double N(double fat, double ds) => 2e6 * Pow(fat / ds, 3);
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "faixa de tensão Δσ?")) return n[1] - n[0];
        if (Vx.Has(p, "razão de tensões")) return n[0] / n[1];
        if (Vx.Has(p, "quantos ciclos a junta suporta")) return n[0] * Pow(n[1] / n[2], 3);
        if (Vx.Has(p, "milhões de ciclos")) return n[0] * Pow(2e6 / (n[1] * 1e6), 1.0 / 3);
        if (Vx.Has(p, "dano D acumulado")) return n[2] / N(n[1], n[3]) + n[4] / N(n[1], n[5]);
        if (Vx.Has(p, "N₂/N₁")) return Pow(n[0] / n[1], 3);
        if (Vx.Has(p, "anos dura")) return N(n[0], n[1]) / n[2] / n[4];
        return null;
    }
}

[Verifies("met-19-qualificacao")]
public sealed class VMet19 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "ensaio de tração da qualificação")) return n[2] * 1000 / (n[0] * n[1]);
        if (Vx.Has(p, "pino de dobra")) return n[0] * n[1];
        if (Vx.Has(p, "maior espessura que a EPS cobre"))
        {
            double f = Mv.Groups(p, @"até ([\d,]+)t")[0], t = Mv.V(p, "t");
            return f * t;
        }
        if (Vx.Has(p, "Que seção circular")) return Sqrt(4 * n[1] * 1000 / (PI * n[0]));
        if (Vx.Has(p, "força mínima de ruptura")) return n[2] * n[0] * n[1] / 1000;
        return null;
    }
}

[Verifies("met-20-ttps")]
public sealed class VMet20 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "totalmente restringida esfria")) return n[0] * 1000 * n[1] * n[2];
        if (Vx.Has(p, "patamar de", "quantas horas de patamar")) return Max(n[0] * n[3] / n[1], n[2] / 60);
        if (Vx.Has(p, "Que queda de temperatura")) return n[0] / (n[1] * 1000 * n[2]);
        if (Vx.Has(p, "tensão residual resultante")) return n[2] / 100 * n[3];
        if (Vx.Has(p, "Ciclo de TTPS"))
        {
            double t = n[0], t0 = n[1], th = n[2], r1 = n[3], hold = Max(n[4] * t / n[5], n[6] / 60), r2 = n[7], tc = n[8];
            return (th - t0) / r1 + hold + (th - tc) / r2;
        }
        return null;
    }
}
