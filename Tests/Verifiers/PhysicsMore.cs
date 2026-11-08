using System.Globalization;
using System.Text.RegularExpressions;

namespace EasyCreator.Verifiers;

/// <summary>Number reader for verifiers: "1,5", "3,986E+14", "3·10⁸", bare "10⁸", "−13,6"; skips element superscripts.</summary>
public static class Pn
{
    const string Sup = "⁰¹²³⁴⁵⁶⁷⁸⁹";
    static readonly Regex Rx = new(
        @"(?<neg>(?<![p{L}d])[−-])?(?:(?<m>\d+(?:[.,]\d+)?)\s*·\s*)?10(?<e>[⁻⁰¹²³⁴⁵⁶⁷⁸⁹]+)|(?<neg2>(?<![p{L}d])[−-])?(?<a>\d+(?:[.,]\d+)?)E\+?(?<ee>-?\d+)|(?<neg3>(?<![p{L}d])[−-])?(?<p>\d+(?:[.,]\d+)?)",
        RegexOptions.Compiled);
    static double D(string s) => double.Parse(s.Replace(',', '.'), CultureInfo.InvariantCulture);

    public static List<double> Nums(string text)
    {
        var l = new List<double>();
        foreach (Match m in Rx.Matches(text))
        {
            if (m.Groups["e"].Success)
            {
                var e = m.Groups["e"].Value; int sign = e.StartsWith('⁻') ? -1 : 1;
                int ex = int.Parse(string.Concat(e.TrimStart('⁻').Select(c => (char)('0' + Sup.IndexOf(c)))));
                double mant = m.Groups["m"].Success ? D(m.Groups["m"].Value) : 1;
                l.Add((m.Groups["neg"].Success ? -1 : 1) * mant * Math.Pow(10, sign * ex));
            }
            else if (m.Groups["a"].Success)
                l.Add((m.Groups["neg2"].Success ? -1 : 1) * D(m.Groups["a"].Value) * Math.Pow(10, int.Parse(m.Groups["ee"].Value)));
            else l.Add((m.Groups["neg3"].Success ? -1 : 1) * D(m.Groups["p"].Value));
        }
        return l;
    }
    public static double Rad(double d) => d * Math.PI / 180;
    public static double Deg(double r) => r * 180 / Math.PI;
}

/// <summary>Physics lessons 02..20: every number prompt solved from its text with textbook formulas (g = 9,8 unless stated).</summary>
[Verifies("fis-02-newton", "fis-03-energia", "fis-04-colisoes", "fis-05-orbitas", "fis-06-pendulo", "fis-07-optica", "fis-08-termodinamica",
    "fis-09-eletromagnetismo", "fis-10-arrasto-numerico", "fis-11-rotacao", "fis-12-fluidos", "fis-13-ondas-som", "fis-14-calor",
    "fis-15-kepler-hohmann", "fis-16-amortecimento", "fis-17-espectrometro", "fis-18-inducao", "fis-19-relatividade", "fis-20-quantica")]
public sealed class VFisRest : IExerciseVerifier
{
    const double G = 9.8, Qe = 1.602177e-19, U = 1.66054e-27;
    static double Sin(double d) => Math.Sin(Pn.Rad(d));
    static double Cos(double d) => Math.Cos(Pn.Rad(d));
    static double Gm(double b) => 1 / Math.Sqrt(1 - b * b);

    public double? Solve(string p)
    {
        var n = Pn.Nums(p);
        bool H(params string[] s) => Vx.Has(p, s);

        // ---- 02 Newton
        if (H("Qual a massa do corpo")) return n[0] / n[1];
        if (H("Qual a força resultante sobre ele")) return n[0] * n[1];
        if (H("Qual a aceleração?") && H("força resultante")) return n[0] / n[1];
        if (H("rampa sem atrito, inclinada"))
        {
            double a = G * Sin(n[0]);
            if (H("Com que velocidade")) return Math.Sqrt(2 * a * n[1]);
            if (H("Quanto tempo")) return Math.Sqrt(2 * n[1] / a);
            return a;
        }
        if (H("coeficiente de atrito cinético"))
            return H("faz o bloco subir") ? n[0] * G * (Sin(n[1]) + n[2] * Cos(n[1])) : G * (Sin(n[1]) - n[2] * Cos(n[1]));
        if (H("polia ideal"))
        {
            double m1 = n[0], m2 = n[2], a = (m2 * G - m1 * G * Sin(n[1])) / (m1 + m2);
            return H("tensão") ? m2 * (G - a) : a;
        }
        // ---- 03 Energia
        if (H("energia potencial gravitacional")) return n[0] * G * n[1];
        if (H("é solto, sem resistência")) return Math.Sqrt(2 * G * n[0]);
        if (H("energia cinética de um corpo")) return 0.5 * n[0] * n[1] * n[1];
        if (H("velocidade do bloco ao sair da mola")) return n[1] * Math.Sqrt(n[0] / n[2]);
        if (H("constante elástica")) return 2 * n[0] / (n[1] * n[1]);
        if (H("energia armazenada")) return 0.5 * n[0] * n[1] * n[1];
        if (H("atrito cinético") && H("mola")) return 0.5 * n[0] * n[1] * n[1] / (n[3] * n[2] * G);
        if (H("rampa sem atrito. Que altura")) return 0.5 * n[0] * n[1] * n[1] / (n[2] * G);
        if (H("compressão mínima")) return Math.Sqrt(5 * n[1] * G * n[2] / n[0]);
        if (H("De que altura mínima")) return 2.5 * n[0];
        // ---- 04 Colisões
        if (H("ficam engatados")) return n[0] * n[1] / (n[0] + n[2]);
        if (H("quantidade de movimento de um corpo")) return n[0] * n[1];
        if (H("um contra o outro")) return (n[0] * n[1] - n[2] * n[3]) / (n[0] + n[2]);
        if (H("perfeitamente elástico"))
            return H("da bola que estava parada") ? 2 * n[0] * n[1] / (n[0] + n[2]) : (n[0] - n[2]) * n[1] / (n[0] + n[2]);
        if (H("energia cinética é perdida")) return 0.5 * n[0] * n[1] * n[1] * n[2] / (n[0] + n[2]);
        if (H("velocidade de recuo")) return n[1] * n[2] / n[0];
        if (H("cravada nele")) return (n[0] + n[1]) / n[0] * Math.Sqrt(2 * G * n[2]);
        // ---- 05 Órbitas
        if (H("pista circular")) return 2 * Math.PI * n[0] / n[1];
        if (H("aceleração centrípeta")) return n[1] * n[1] / n[0];
        if (H("período orbital")) { double r = (n[2] + n[0]) * 1e3; return 2 * Math.PI * Math.Sqrt(r * r * r / n[1]) / 60; }
        if (H("velocidade orbital?") && H("altitude")) return Math.Sqrt(n[1] / ((n[2] + n[0]) * 1e3)) / 1e3;
        if (H("velocidade de escape")) return Math.Sqrt(2 * n[0] / (n[1] * 1e3)) / 1e3;
        if (H("A que altitude")) { double T = n[0] * 3600; return Math.Cbrt(n[1] * T * T / (4 * Math.PI * Math.PI)) / 1e3 - n[2]; }
        if (H("vezes maior, qual seria")) return n[0] / Math.Sqrt(n[1]);
        if (H("Qual o período do segundo")) return n[0] * Math.Pow(n[1], 1.5);
        // ---- 06 Pêndulo / molas
        if (H("pêndulo simples de") && H("frequência")) return 1 / (2 * Math.PI * Math.Sqrt(n[0] / n[1]));
        if (H("período de um pêndulo simples")) return 2 * Math.PI * Math.Sqrt(n[0] / n[1]);
        if (H("Que comprimento deve ter")) return n[1] * n[0] * n[0] / (4 * Math.PI * Math.PI);
        if (H("aceleração da gravidade local")) return 4 * Math.PI * Math.PI * n[0] / (n[1] * n[1]);
        if (H("elevador que sobe")) return 2 * Math.PI * Math.Sqrt(n[0] / (n[2] + n[1]));
        if (H("é levado")) return 2 * Math.PI * Math.Sqrt(n[0] / n[1]);
        if (H("preso a uma mola")) return 2 * Math.PI * Math.Sqrt(n[0] / n[1]);
        if (H("entre duas molas")) return 2 * Math.PI * Math.Sqrt(n[0] / (n[1] + n[2]));
        if (H("molas ligadas em série")) return 2 * Math.PI * Math.Sqrt(n[0] * (n[1] + n[2]) / (n[1] * n[2]));
        // ---- 07 Óptica
        if (H("estação de rádio")) return n[1] / (n[0] * 1e6);
        if (H("Qual a sua velocidade?") && H("comprimento de onda")) return n[0] * n[1];
        if (H("índice de refração") && H("velocidade da luz nele")) return n[^1] / n[0] / 1e8;
        if (H("ângulo de refração")) return Pn.Deg(Math.Asin(Sin(n[0]) / n[1]));
        if (H("altura da imagem")) return n[1] * Math.Abs(n[0] / (n[0] - n[2]));
        if (H("ângulo crítico")) return Pn.Deg(Math.Asin(1 / n[0]));
        if (H("A que distância da lente")) return n[0] * n[1] / (n[1] - n[0]);
        if (H("SEGUNDA lente"))
        {
            double d1 = n[0] * n[3] / (n[3] - n[0]), o2 = n[2] - d1;
            return n[1] * o2 / (o2 - n[1]);
        }
        // ---- 08 Termodinâmica
        if (H("variação da energia interna") && H("recebe")) return n[0] - n[1];
        if (H("ocupam")) return n[0] * n[3] * n[2] / n[1];
        if (H("isotermicamente")) return n[0] * n[3] * n[1] * Math.Log(n[2]);
        if (H("pressão constante de")) return n[0] * n[1];
        if (H("monoatômico são aquecidos")) return 1.5 * n[0] * n[2] * n[1];
        if (H("adiabaticamente")) return n[2] * Math.Pow(n[3], 1 - n[0] / n[1]);
        if (H("Carnot") && H("Que trabalho")) return n[2] * (1 - Math.Min(n[0], n[1]) / Math.Max(n[0], n[1]));
        if (H("Carnot") && H("fonte fria")) { double th = Math.Max(n[0], n[1]), tc = Math.Min(n[0], n[1]); return n[2] * tc / (th - tc); }
        // ---- 09 Eletromagnetismo
        if (H("potência dissipada") && H("resistor ligado")) return n[0] * n[1];
        if (H("Qual a corrente?") && H("aplicada")) return n[0] / n[1];
        if (H("Qual a tensão sobre ele")) return n[0] * n[1];
        if (H("campo elétrico, em kN/C")) return n[2] * n[1] * 1e-6 / (n[0] * n[0]) / 1e3;
        if (H("em paralelo")) return n[2] * (1 / n[0] + 1 / n[1]);
        if (H("separadas por")) return n[3] * n[0] * 1e-6 * n[1] * 1e-6 / (n[2] * n[2]);
        if (H("campo magnético, em μT")) return 2e-7 * n[1] / n[0] * 1e6;
        if (H("força magnética, em mN")) return n[0] * n[1] * n[2];
        if (H("fio reto de")) return n[2] * n[1] * n[0];
        if (H("potência máxima")) return n[0] * n[0] / (4 * n[1]);
        if (H("campo elétrico resultante")) { double a = Math.Max(n[0], n[1]), b = Math.Min(n[0], n[1]); return n[2] / (1 + Math.Sqrt(a / b)); }
        // ---- 10 Arrasto / numérico
        if (H("força de arrasto")) return 0.5 * n[0] * n[1] * n[2] * n[3] * n[3];
        if (H("velocidade terminal de um corpo")) return Math.Sqrt(2 * n[0] * G / (n[3] * n[1] * n[2]));
        if (H("método de Euler"))
        {
            double v = n[4]; int k = (int)Math.Round(n[3]);
            for (int i = 0; i < k; i++) v += (G - n[1] * v / n[0]) * n[2];
            return v;
        }
        if (H("velocidade exata")) return n[0] * G / n[1] * (1 - Math.Exp(-n[1] * n[2] / n[0]));
        if (H("paraquedista")) return G * (1 - Math.Pow(n[1] / 100, 2));
        // ---- 11 Rotação
        if (H("rpm") && H("velocidade angular")) return n[0] * 2 * Math.PI / 60;
        if (H("Qual o torque")) return n[0] * n[1];
        if (H("energia cinética de rotação")) return 0.5 * 0.5 * n[0] * n[1] * n[1] * n[2] * n[2];
        if (H("aceleração angular")) return n[0] / (0.5 * n[1] * n[2] * n[2]);
        if (H("momento de inércia de um disco")) return 0.5 * n[0] * n[1] * n[1];
        if (H("rola sem deslizar"))
        {
            double k = H("aro") ? 1 : H("disco") ? 0.5 : H("casca") ? 2.0 / 3 : 0.4;
            return H("rampa de") ? G * Sin(n[0]) / (1 + k) : Math.Sqrt(2 * G * n[0] / (1 + k));
        }
        if (H("polia (disco maciço)")) return Math.Abs(n[0] - n[1]) * G / (n[0] + n[1] + n[2] / 2);
        if (H("patinadora")) return n[0] * n[1] / n[2];
        // ---- 12 Fluidos
        if (H("Qual o empuxo")) return n[1] * n[0] / 1000 * G;
        if (H("pressão manométrica")) return n[1] * G * n[0] / 1000;
        if (H("Torricelli") && !H("jato")) return Math.Sqrt(2 * G * n[0]);
        if (H("porcentagem do seu volume")) return n[0] / n[1] * 100;
        if (H("parte estreita")) return n[0] * n[1] / n[2];
        if (H("prensa hidráulica")) return n[2] * n[0] / n[1];
        if (H("jato atinge o chão")) return 2 * Math.Sqrt(n[1] * (n[0] - n[1]));
        if (H("tração no fio")) return n[0] * G * (1 - n[2] / n[1]);
        if (H("queda de pressão")) { double v2 = n[0] * n[2]; return 0.5 * n[3] * (v2 * v2 - n[0] * n[0]) / 1000; }
        // ---- 13 Ondas e som
        if (H("velocidade do som no ar a")) return n[1] + n[2] * n[0];
        if (H("Qual o seu comprimento de onda")) return n[1] / n[0];
        if (H("harmônico?") && H("fixa nas duas pontas")) return n[2] * n[1] / (2 * n[0]);
        if (H("g/m está sob tração")) return Math.Sqrt(n[1] / (n[0] / 1000));
        if (H("fechado numa ponta de")) return n[1] / (4 * n[0]);
        if (H("aberto nas duas pontas")) return n[1] / (2 * n[0]);
        if (H("ambulância")) return n[0] * n[2] / (n[2] - n[1]);
        if (H("também corre")) return n[0] * (n[3] + n[2]) / (n[3] - n[1]);
        if (H("Que tração")) return n[1] * Math.Pow(n[2] / n[0], 2);
        if (H("modo de ressonância")) return (2 * n[1] - 1) * n[2] / (4 * n[0]);
        // ---- 14 Calor
        if (H("derreter")) return n[0] * n[^1];
        if (H("aquecer")) return n[0] * n[2] * n[1] / 1000;
        if (H("condutividade")) return n[0] * n[1] * n[3] / n[2];
        if (H("Misturam-se")) return (n[0] * n[1] + n[2] * n[3]) / (n[0] + n[2]);
        if (H("Colocam-se"))
        {
            double mi = n[0], mw = n[2], t = n[3], c = n[4], Lf = n[5] * 1000;
            return mw * c * t >= mi * Lf ? (mw * c * t - mi * Lf) / ((mw + mi) * c) : 0;
        }
        if (H("em água a") && H("c do gelo")) return n[0] * (n[3] * -n[1] + n[5] * 1000 + n[4] * n[2]) / 1000;
        if (H("duas camadas")) return n[0] * n[5] / (n[1] / n[2] + n[3] / n[4]);
        if (H("Quantos gramas de gelo"))
        {
            double mi = n[0], mw = n[2], t = n[3], c = n[4], Lf = n[5] * 1000;
            return Math.Min(mi, mw * c * t / Lf) * 1000;
        }
        // ---- 15 Kepler / Hohmann
        if (H("semieixo maior, em UA")) return Math.Cbrt(n[0] * n[0]);
        if (H("Qual o seu período, em anos")) return Math.Pow(n[0], 1.5);
        if (H("velocidade de um satélite em órbita circular")) return Math.Sqrt(n[1] / ((n[2] + n[0]) * 1e3)) / 1e3;
        if (H("Outra lua")) return n[0] * Math.Pow(n[2] / n[1], 1.5);
        if (H("Quantos dias dura a viagem")) return Math.Pow((n[0] + n[1]) / 2, 1.5) / 2 * 365.25;
        if (H("Δv da PRIMEIRA")) { double r1 = (n[3] + n[0]) * 1e3, r2 = n[1] * 1e3; return Math.Sqrt(n[2] / r1) * (Math.Sqrt(2 * r2 / (r1 + r2)) - 1); }
        // ---- 16 Amortecimento
        if (H("amortecimento crítico")) return 2 * Math.Sqrt(n[0] * n[1]);
        if (H("sem amortecimento")) return Math.Sqrt(n[1] / n[0]);
        if (H("amplitude inicial")) return n[3] * Math.Exp(-n[2] * n[4] / (2 * n[0]));
        if (H("cai à metade")) return 2 * n[0] * Math.Log(2) / n[2];
        if (H("oscilações amortecidas")) return Math.Sqrt(n[1] / n[0] - Math.Pow(n[2] / (2 * n[0]), 2));
        if (H("ENERGIA cai")) return n[0] / n[2];
        if (H("fator de qualidade")) return n[0] * Math.Sqrt(n[1] / n[0]) / n[2];
        // ---- 17 Espectrômetro (n = [..data.., m, q] when the prompt states the particle)
        if (H("frequência de ciclotron")) return n[2] * n[0] / (2 * Math.PI * n[1]) / 1e6;
        if (H("próton entra a")) return n[2] * n[0] * 1e3 / (n[3] * n[1]) * 100;
        if (H("próton é acelerado")) return Math.Sqrt(2 * (n.Count > 3 ? n[2] : 1.6726e-27) * n[0] / (n.Count > 3 ? n[3] : Qe)) / n[1] * 100;
        if (H("seletor de velocidades")) return n[0] / n[1];
        if (H("distância (em mm)"))
        {
            var ms = Regex.Matches(p, "([⁰¹²³⁴⁵⁶⁷⁸⁹]+)[A-Z]");
            double Mass(string s) => double.Parse(string.Concat(s.Select(c => (char)('0' + "⁰¹²³⁴⁵⁶⁷⁸⁹".IndexOf(c))))) * U;
            double m1 = Mass(ms[0].Groups[1].Value), m2 = Mass(ms[1].Groups[1].Value), V = n[0] * 1e3, B = n[1];
            return 2 * Math.Abs(Math.Sqrt(2 * m2 * V / Qe) - Math.Sqrt(2 * m1 * V / Qe)) / B * 1000;
        }
        if (H("Um elétron é acelerado")) return Math.Sqrt(2 * n[2] * n[0] / Qe) / (n[1] * 1e-3) * 1000;
        if (H("massa do íon")) return Qe * n[2] * n[2] * Math.Pow(n[1] / 100, 2) / (2 * n[0] * 1e3) / n[4];
        // ---- 18 Indução
        if (H("fem média induzida")) return n[0] * n[1] * 1e-3 / n[2];
        if (H("Qual a tensão no secundário")) return n[2] * n[1] / n[0];
        if (H("fem máxima")) return n[0] * n[3] * n[1] * 2 * Math.PI * n[2];
        if (H("fem induzida")) return n[0] * n[1] * n[2];
        if (H("Qual a corrente no primário")) return n[3] * n[1] / n[0];
        if (H("potência fornecida à carga")) return n[1] * n[2] / n[0] * n[3];
        if (H("velocidade terminal")) return n[0] * G * n[2] / (n[3] * n[3] * n[1] * n[1]);
        if (H("porcentagem da potência se perde")) { double I = n[0] / n[3]; return I * I * n[1] / (n[0] * 1000) * 100; }
        // ---- 19 Relatividade
        if (H("energia equivalente")) return n[0] * 1e-3 * n[^1] * n[^1] / 1e13;
        if (H("fator de Lorentz")) return Gm(n[0]);
        if (H("comprimento medido")) return n[0] / Gm(n[1]);
        if (H("Quantos anos passam na Terra")) return n[0] * Gm(n[1]);
        if (H("Múons")) return n[1] * 299792.458 * n[0] * 1e-6 * Gm(n[1]);
        if (H("energia cinética, em MeV")) return (Gm(n[1]) - 1) * n[0];
        if (H("momento, em MeV/c")) return Math.Sqrt(n[1] * n[1] + 2 * n[1] * n[0]);
        if (H("projétil")) return (n[0] + n[1]) / (1 + n[0] * n[1]);
        // ---- 20 Quântica
        if (H("comprimento de onda, em nm, de um fóton")) return n[1] / n[0];
        if (H("energia, em eV, de um fóton")) return n[1] / n[0];
        if (H("maior comprimento de onda")) return n[1] / n[0];
        if (H("energia cinética máxima") || H("potencial de corte")) return n[2] / n[0] - n[1];
        if (H("átomo de hidrogênio")) return n[3] / (13.6 * (1 / (n[1] * n[1]) - 1 / (n[0] * n[0])));
        if (H("incerteza mínima")) return 1.0546e-34 / (n[3] * n[0] * 1e-10 * 9.109e-31) / 1e5;
        if (H("de Broglie")) return n[1] / Math.Sqrt(n[0]);
        return null;
    }
}
