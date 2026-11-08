using static System.Math;

namespace EasyCreator.Verifiers;

static class Ev
{
    public const double Rho = 1.0 / 56;
    public static double Deg(double rad) => rad * 180 / PI;
    /// <summary>Prompt text with every parenthesised remark removed (constants like "(e = 1,6·10⁻¹⁹ C)").</summary>
    public static string NoParen(string p) => System.Text.RegularExpressions.Regex.Replace(p, @"\([^)]*\)", " ");
}

[Verifies("ele-01-ohm")]
public sealed class VEle01 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        p = p.Split(" (Cobre:")[0]; // the constant note added by the generator
        var n = Vx.Nums(p);
        if (Vx.Has(p, "Por um resistor de", "Qual a tensão")) return n[0] * n[1];
        if (Vx.Has(p, "submetido a", "resistência")) return n[0] / n[1];
        if (Vx.Has(p, "está ligado a uma fonte", "corrente")) return n[1] / n[0];
        if (Vx.Has(p, "deve ter resistência", "comprimento")) return n[2] * n[1] / n[0];
        if (Vx.Has(p, "Quantos elétrons")) return n[0] * n[1] * 60 / n[2];
        if (Vx.Has(p, "Que carga elétrica")) return n[0] * n[1] * 60;
        if (Vx.Has(p, "tem", "seção de", "Qual a sua resistência")) return n[0] * n[1] / n[2];
        if (Vx.Has(p, "aquecedor de", "W /", "corrente que ele consome")) return n[0] / n[1];
        if (Vx.Has(p, "aquecedor de resistência", "potência")) return n[1] * n[1] / n[0];
        if (Vx.Has(p, "chuveiro")) return n[1] * n[1] / n[0];
        if (Vx.Has(p, "enrolamento", "corrente nele agora"))
        {
            double r0 = n[0], dT = n[2], v = n[3];
            return v / (r0 * (1 + 0.0039 * dT));
        }
        if (Vx.Has(p, "esticado")) return n[0] * n[1] * n[1];
        if (Vx.Has(p, "enrolamento", "Quantos graus")) return (n[2] / n[0] - 1) / 0.0039;
        return null;
    }
}

[Verifies("ele-02-serie-paralelo")]
public sealed class VEle02 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "resistores iguais", "paralelo")) return n[1] / n[0];
        if (Vx.Has(p, "Três resistores", "série")) return n[0] + n[1] + n[2];
        if (Vx.Has(p, "Dois resistores", "paralelo")) return n[0] * n[1] / (n[0] + n[1]);
        if (Vx.Has(p, "Divisor de tensão", "tensão sobre R₂")) return n[0] * n[2] / (n[1] + n[2]);
        if (Vx.Has(p, "Divisor de tensão", "corrente")) return n[0] / (n[1] + n[2]) * 1000;
        if (Vx.Has(p, "Uma fonte de", "alimenta ", "em paralelo", "corrente total")) return n[0] * (1 / n[1] + 1 / n[2]);
        if (Vx.Has(p, "Uma fonte de", "alimenta ", "em série", "Qual a corrente")) return n[0] / (n[1] + n[2]);
        if (Vx.Has(p, "em série com o par"))
        {
            double r1 = n[0], par = n[1] * n[2] / (n[1] + n[2]), v = n[3], req = r1 + par;
            if (Vx.Has(p, "corrente fornecida")) return v / req;
            if (Vx.Has(p, "resistência equivalente")) return req;
            if (Vx.Has(p, "tensão sobre o par")) return v / req * par;
            return null;
        }
        if (Vx.Has(p, "lâmpadas", "resistor em série")) return (n[3] - n[0] * n[1]) / n[2];
        if (Vx.Has(p, "em paralelo com um de")) return n[0] * n[1] / (n[0] - n[1]);
        return null;
    }
}

[Verifies("ele-03-kirchhoff")]
public sealed class VEle03 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "Em um nó entram")) return n[0] + n[1] - n[2];
        if (Vx.Has(p, "Uma malha simples"))
        {
            double i = Abs(n[0] - n[1]) / (n[2] + n[3]);
            if (Vx.Has(p, "Qual a corrente")) return i;
            if (Vx.Has(p, "tensão sobre o resistor")) return i * n[4];
            return null;
        }
        if (Vx.Has(p, "R₂", "nó"))
        {
            double v1 = n[0], r1 = n[1], v2 = n[2], r3 = n[3], r2 = n[4];
            double vn = (v1 / r1 + v2 / r3) / (1 / r1 + 1 / r2 + 1 / r3);
            if (Vx.Has(p, "tensão no nó")) return vn;
            if (Vx.Has(p, "potência dissipada em R₂")) return vn * vn / r2;
            if (Vx.Has(p, "corrente que sai da fonte V₁")) return (v1 - vn) / r1;
        }
        return null;
    }
}

[Verifies("ele-04-potencia-energia")]
public sealed class VEle04 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "kWh de energia")) return n[0] * n[1] / 1000;
        if (Vx.Has(p, "Um aparelho de", "Qual a corrente")) return n[0] / n[1];
        if (Vx.Has(p, "puxa", "potência")) return n[0] * n[1];
        if (Vx.Has(p, "bateria", "Quantas horas")) return n[0] * n[1] / n[2];
        if (Vx.Has(p, "por dia durante")) return n[0] * n[1] * n[2] / 1000 * n[3];
        if (Vx.Has(p, "resistência constante")) return n[0] * Pow(n[2] / n[1], 2);
        if (Vx.Has(p, "esquenta", "Quantos segundos")) return n[2] * n[4] * 1000 * n[3] / (n[0] * n[1] / 100);
        if (Vx.Has(p, "LED"))
        {
            double h = n[0], p1 = n[1], life = n[2], c1 = n[3], p2 = n[4], c2 = n[5], t = n[6];
            double inc = h * p1 / 1000 * t + Ceiling(h / life) * c1;
            double led = h * p2 / 1000 * t + c2;
            return inc - led;
        }
        return null;
    }
}

[Verifies("ele-05-thevenin")]
public sealed class VEle05 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "divisor com", "tensão de Thévenin")) return n[0] * n[2] / (n[1] + n[2]);
        if (Vx.Has(p, "divisor com", "resistência de Thévenin")) return n[1] * n[2] / (n[1] + n[2]);
        if (Vx.Has(p, "equivale a Vth", "corrente na carga")) return n[0] / (n[1] + n[2]);
        if (Vx.Has(p, "equivale a Vth", "tensão na carga")) return n[0] * n[2] / (n[1] + n[2]);
        if (Vx.Has(p, "carga variável", "máxima potência")) return n[0] * n[0] / (4 * n[1]);
        if (Vx.Has(p, "carga variável", "resistência de carga")) return n[1];
        if (Vx.Has(p, "resistência interna")) return n[2] * (n[0] - n[1]) / n[1];
        if (Vx.Has(p, "curto-circuito")) return n[0] * n[1] / 4;
        return null;
    }
}

[Verifies("ele-06-rc")]
public sealed class VEle06 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "em série", "capacitância equivalente")) return n[0] * n[1] / (n[0] + n[1]);
        if (Vx.Has(p, "em paralelo", "capacitância equivalente")) return n[0] + n[1];
        if (Vx.Has(p, "constante de tempo")) return n[0] * n[1];
        if (Vx.Has(p, "descarregado", "Em quantos ms")) return n[0] * n[1] * Log(n[2] / (n[2] - n[3]));
        if (Vx.Has(p, "descarregado", "tensão após")) return n[1] * (1 - Exp(-n[3] / (n[0] * n[2])));
        if (Vx.Has(p, "Quanta energia")) return 0.5 * n[0] * n[1] * n[1] / 1000;
        if (Vx.Has(p, "descarrega", "tensão após")) return n[1] * Exp(-n[3] / (n[0] * n[2]));
        if (Vx.Has(p, "descarrega", "Em quantos ms")) return n[0] * n[2] * Log(n[1] / n[3]);
        if (Vx.Has(p, "temporizador")) return n[3] / (n[0] * Log(n[1] / (n[1] - n[2])));
        if (Vx.Has(p, "já tem")) return n[2] + (n[1] - n[2]) * Exp(-n[4] / (n[0] * n[3]));
        return null;
    }
}

[Verifies("ele-07-fasores")]
public sealed class VEle07 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "reatância de um capacitor")) return 1 / (2 * PI * n[1] * n[0] * 1e-6);
        if (Vx.Has(p, "reatância de um indutor")) return 2 * PI * n[1] * n[0] * 1e-3;
        if (Vx.Has(p, "em série com um indutor"))
        {
            double z = Sqrt(n[0] * n[0] + Pow(2 * PI * n[3] * n[1] * 1e-3, 2));
            if (Vx.Has(p, "impedância")) return z;
            if (Vx.Has(p, "Qual a corrente")) return n[2] / z;
            return null;
        }
        if (Vx.Has(p, "RLC série em"))
        {
            double f = n[0], r = n[1], xl = 2 * PI * f * n[2] * 1e-3, xc = 1 / (2 * PI * f * n[3] * 1e-6), v = n[4];
            double z = Sqrt(r * r + (xl - xc) * (xl - xc));
            if (Vx.Has(p, "Qual a corrente")) return v / z;
            if (Vx.Has(p, "impedância")) return z;
            if (Vx.Has(p, "ângulo de fase")) return Abs(Ev.Deg(Atan((xl - xc) / r)));
            return null;
        }
        if (Vx.Has(p, "Que capacitor", "ressoar")) { double w = 2 * PI * n[1]; return 1 / (w * w * n[0] * 1e-3) * 1e6; }
        if (Vx.Has(p, "Qual a frequência de ressonância")) return 1 / (2 * PI * Sqrt(n[0] * 1e-3 * n[1] * 1e-6));
        if (Vx.Has(p, "tensão sobre o capacitor")) return n[3] / n[0] * Sqrt(n[1] * 1e-3 / (n[2] * 1e-6));
        return null;
    }
}

[Verifies("ele-08-fator-potencia")]
public sealed class VEle08 : IExerciseVerifier
{
    static double Tn(double pf) => Sqrt(1 - pf * pf) / pf;
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "potência aparente", "monofásica")) return n[0] * n[1];
        if (Vx.Has(p, "potência ativa", "monofásica")) return n[0] * n[1] * n[2];
        if (Vx.Has(p, "Uma instalação consome")) return n[0] / n[1];
        if (Vx.Has(p, "potência reativa")) return n[0] * Tn(n[1]);
        if (Vx.Has(p, "Uma carga ativa", "potência aparente")) return n[0] / n[1];
        if (Vx.Has(p, "kvar de capacitores")) return n[0] * (Tn(n[1]) - Tn(n[2]));
        if (Vx.Has(p, "ampères")) return n[0] * 1000 / n[1] * (1 / n[2] - 1 / n[3]);
        if (Vx.Has(p, "capacitor, em µF"))
        {
            double qc = n[0] * 1000 * (Tn(n[3]) - Tn(n[4]));
            return qc / (2 * PI * n[2] * n[1] * n[1]) * 1e6;
        }
        return null;
    }
}

[Verifies("ele-09-retificador")]
public sealed class VEle09 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "tensão média na saída")) return 2 * n[0] / PI;
        if (Vx.Has(p, "tensão de pico")) return n[0] * Sqrt(2);
        if (Vx.Has(p, "corrente média na carga")) return 2 * n[0] / (PI * n[1]) * 1000;
        if (Vx.Has(p, "pico da tensão de saída")) return n[2] * Sqrt(2) - 2 * n[1];
        if (Vx.Has(p, "ondulação", "filtro de") && !Vx.Has(p, "ponte")) return n[1] / (2 * n[0] * n[2] * 1e-6);
        if (Vx.Has(p, "tensão contínua média"))
        {
            double vp = n[1] * Sqrt(2) - 2 * n[0], rip = n[4] / (2 * n[2] * n[3] * 1e-6);
            return vp - rip / 2;
        }
        if (Vx.Has(p, "capacitor de filtro")) return n[3] / (2 * n[2] * n[4]) * 1e6;
        return null;
    }
}

[Verifies("ele-10-trifasico")]
public sealed class VEle10 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "tensão de linha", "Qual a tensão de fase")) return n[0] / Sqrt(3);
        if (Vx.Has(p, "tensão de fase", "Qual a tensão de linha")) return n[0] * Sqrt(3);
        if (Vx.Has(p, "Carga trifásica equilibrada:")) return Sqrt(3) * n[0] * n[1] * n[2] / 1000;
        if (Vx.Has(p, "Três resistores"))
        {
            double r = n[0], v = n[1];
            if (Vx.Has(p, "Em triângulo", "corrente de linha")) return Sqrt(3) * v / r;
            if (Vx.Has(p, "Em triângulo", "potência total")) return 3 * v * v / r / 1000;
            if (Vx.Has(p, "Em estrela", "corrente de linha")) return v / Sqrt(3) / r;
            return null;
        }
        if (Vx.Has(p, "potência reativa total")) return n[0] * Sqrt(1 - n[1] * n[1]);
        if (Vx.Has(p, "motor trifásico")) return n[0] * 1000 / (n[1] / 100) / (Sqrt(3) * n[3] * n[2]);
        return null;
    }
}

[Verifies("ele-11-superposicao")]
public sealed class VEle11 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "fonte de corrente de", "dois resistores em paralelo")) return n[0] * n[1] * n[2] / (n[1] + n[2]);
        if (Vx.Has(p, "fonte de corrente de", "um resistor")) return n[0] * n[1];
        if (Vx.Has(p, "Duas malhas"))
        {
            double r3 = n[0], v1 = n[1], r1 = n[2], v2 = n[3], r2 = n[4];
            double a = r1 + r3, b = -r3, c = -r3, d = r2 + r3, e1 = v1, e2 = -v2;
            double det = a * d - b * c, i1 = (e1 * d - b * e2) / det, i2 = (a * e2 - c * e1) / det;
            if (Vx.Has(p, "corrente de malha I₁")) return i1;
            if (Vx.Has(p, "Qual a corrente em R₃")) return Abs(i1 - i2);
            return null;
        }
        if (Vx.Has(p, "superposição"))
        {
            double v1 = n[0], r1 = n[1], r3 = n[3], r2 = n[4], par = r2 * r3 / (r2 + r3);
            return v1 * par / (r1 + par);
        }
        if (Vx.Has(p, "fonte de corrente") && Vx.Has(p, "R₂"))
        {
            double v = n[0], r1 = n[1], r2 = n[2], i = n[3];
            double vn = (v / r1 + i) / (1 / r1 + 1 / r2);
            if (Vx.Has(p, "tensão no nó")) return vn;
            if (Vx.Has(p, "potência R₂")) return vn * vn / r2;
            return null;
        }
        return null;
    }
}

[Verifies("ele-12-rl")]
public sealed class VEle12 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "corrente final")) return n[0] / n[1];
        if (Vx.Has(p, "constante de tempo τ?") && Vx.Has(p, "mH está em série")) return n[0] / n[1];
        if (Vx.Has(p, "Em quantos ms a corrente chega")) { double im = n[2] / n[1]; return n[0] / n[1] * Log(im / (im - n[3])); }
        if (Vx.Has(p, "Qual a corrente", "depois")) return n[2] / n[1] * (1 - Exp(-n[3] * n[1] / n[0]));
        if (Vx.Has(p, "tensão sobre o indutor")) return n[2] * Exp(-n[3] * n[1] / n[0]);
        if (Vx.Has(p, "Quanta energia")) return 0.5 * n[0] * n[1] * n[1];
        if (Vx.Has(p, "energia armazenada cai pela metade")) return n[0] / n[1] * Log(2) / 2;
        if (Vx.Has(p, "solenoide"))
        {
            var m = Vx.Nums(Ev.NoParen(p));
            double l = 4 * PI * 1e-7 * m[0] * m[0] * m[1] * 1e-4 / (m[2] * 1e-2);
            return l / m[3] * 1000;
        }
        return null;
    }
}

[Verifies("ele-13-led-zener")]
public sealed class VEle13 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "queda direta")) return (n[2] - n[0]) / n[1] * 1000;
        if (Vx.Has(p, "Zener de", "potência que ele dissipa")) return n[0] * n[1];
        if (Vx.Has(p, "mW esse resistor dissipa")) return (n[2] - n[0]) * n[1];
        if (Vx.Has(p, "maior resistor")) return (n[1] - n[0]) / (n[2] + n[3]) * 1000;
        if (Vx.Has(p, "tensão mínima de entrada")) return n[0] + n[2] * n[0] / n[1];
        if (Vx.Has(p, "carga for desligada")) return (n[1] - n[0]) / n[2] * n[0] * 1000;
        return null;
    }
}

[Verifies("ele-14-filtros")]
public sealed class VEle14 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "ganho de tensão", "em dB")) return 20 * Log10(n[0]);
        if (Vx.Has(p, "frequência de corte") && Vx.Has(p, "R =")) return 1 / (2 * PI * n[0] * 1e3 * n[1] * 1e-9);
        if (Vx.Has(p, "pico na saída")) return n[1] / Sqrt(1 + Pow(n[2] / n[0], 2));
        if (Vx.Has(p, "assíntota")) return -20 * n[2];
        if (Vx.Has(p, "passa-baixas", "ganho, em dB")) return -10 * Log10(1 + Pow(n[1] / n[0], 2));
        if (Vx.Has(p, "passa-altas", "ganho, em dB")) return -10 * Log10(1 + Pow(n[0] / n[1], 2));
        if (Vx.Has(p, "Que resistor")) return 1 / (2 * PI * n[1] * n[0] * 1e-9) / 1e3;
        if (Vx.Has(p, "atenuado")) return n[0] * Sqrt(Pow(10, n[1] / 10) - 1);
        return null;
    }
}

[Verifies("ele-15-ressonancia-paralela")]
public sealed class VEle15 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "LC paralelo", "frequência de ressonância")) return 1 / (2 * PI * Sqrt(n[0] * 1e-3 * n[1] * 1e-6));
        if (Vx.Has(p, "largura de banda de", "fator de qualidade")) return n[0] / n[1];
        if (Vx.Has(p, "Qual a largura de banda (")) return n[0] / n[1];
        if (Vx.Has(p, "resistor de", "fator de qualidade Q")) return n[2] * 1000 / Sqrt(n[0] * 1e-3 / (n[1] * 1e-6));
        if (Vx.Has(p, "Projete")) return n[0] / n[1] * 2 * PI * n[0] * n[2] * 1e-3 / 1000;
        if (Vx.Has(p, "largura de banda, em Hz"))
        {
            double f0 = 1 / (2 * PI * Sqrt(n[0] * 1e-3 * n[1] * 1e-6)), q = n[2] * 1000 / Sqrt(n[0] * 1e-3 / (n[1] * 1e-6));
            return f0 / q;
        }
        return null;
    }
}

[Verifies("ele-16-transformador")]
public sealed class VEle16 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "Quantas espiras")) return n[2] * n[1] / n[0];
        if (Vx.Has(p, "no primário e", "no secundário, ligado")) return n[2] * n[1] / n[0];
        if (Vx.Has(p, "rendimento")) return n[3] / (n[2] / 100) / n[0];
        if (Vx.Has(p, "ideal de", "carga de", "corrente no primário") && Vx.Has(p, "A. Qual")) return n[2] * n[1] / n[0];
        if (Vx.Has(p, "casar")) return Sqrt(n[1] / n[0]);
        if (Vx.Has(p, "relação de espiras", "enxerga")) return n[0] * n[0] / (n[1] * n[1]) * n[2];
        if (Vx.Has(p, "resistor de", "corrente no primário")) return n[1] / n[2] * n[1] / n[0];
        if (Vx.Has(p, "fluxo magnético")) return n[1] / (n[3] * n[2] * n[0]) * 1000;
        return null;
    }
}

[Verifies("ele-17-ampop")]
public sealed class VEle17 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "não inversor", "ganho de tensão")) return 1 + n[1] / n[0];
        if (Vx.Has(p, "inversor", "ganho de tensão") && !Vx.Has(p, "Quer-se")) return -n[1] / n[0];
        if (Vx.Has(p, "não inversor", "tensão de saída")) return n[2] * (1 + n[1] / n[0]);
        if (Vx.Has(p, "Somador")) return -n[0] * (n[1] / n[2] + n[3] / n[4]);
        if (Vx.Has(p, "diferencial")) return n[1] / n[0] * (n[3] - n[2]);
        if (Vx.Has(p, "saturação"))
        {
            double v = -n[4] * n[0] / n[1], lim = n[3];
            return Max(-lim, Min(lim, v));
        }
        if (Vx.Has(p, "Quer-se")) return Abs(n[0]) * n[1];
        if (Vx.Has(p, "inversor", "tensão de saída")) return -n[2] * n[1] / n[0];
        return null;
    }
}

[Verifies("ele-18-motores")]
public sealed class VEle18 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "escorregamento de", "velocidade do eixo")) return n[0] * (1 - n[1] / 100);
        if (Vx.Has(p, "velocidade síncrona de um motor")) return 120 * n[1] / n[0];
        if (Vx.Has(p, "torque")) return n[0] * 1000 / (2 * PI * n[1] / 60);
        if (Vx.Has(p, "escorregamento, em %")) return (n[0] - n[1]) / n[0] * 100;
        if (Vx.Has(p, "força contraeletromotriz") && Vx.Has(p, "regime")) return n[0] - n[1] * n[2];
        if (Vx.Has(p, "absorve da rede")) return n[0] / (n[1] / 100);
        if (Vx.Has(p, "carga cai")) { double e1 = n[1] - n[0] * n[3]; return e1 * n[4] / n[2]; }
        if (Vx.Has(p, "corrente de partida")) return n[0] / n[1];
        return null;
    }
}

[Verifies("ele-19-condutores")]
public sealed class VEle19 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        p = p.Split(" (Cobre:")[0]; // the constant note added by the generator
        var n = Vx.Nums(p);
        double rho = Vx.Has(p, "ρ =") ? n[0] / n[1] : Ev.Rho;
        if (Vx.Has(p, "resistência de um condutor")) return rho * n[2] / n[3];
        if (Vx.Has(p, "watts são perdidos")) return Pow(n[2], 2) * rho * 2 * n[1] / n[0];
        if (Vx.Has(p, "queda de tensão total")) return n[1] * rho * 2 * n[3] / n[2];
        if (Vx.Has(p, "trifásico")) return Sqrt(3) * rho * n[2] * n[1] / n[3];
        if (Vx.Has(p, "seção mínima")) return 2 * rho * n[2] * n[1] / (n[3] / 100 * n[0]);
        if (Vx.Has(p, "dobrada")) return rho * 2 * n[1] * n[2] / n[0] / 2;
        if (Vx.Has(p, "comprimento máximo")) return n[3] / 100 * n[0] * n[2] / (2 * rho * n[1]);
        return null;
    }
}

[Verifies("ele-20-protecao")]
public sealed class VEle20 : IExerciseVerifier
{
    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "tensão de toque", "resistência de aterramento máxima")) return n[0] / n[1];
        if (Vx.Has(p, "curto-circuito franco")) return n[0] / n[1];
        if (Vx.Has(p, "disjuntor de", "magnético")) return n[0] * n[1];
        if (Vx.Has(p, "transformador trifásico")) return n[0] * 1000 / (Sqrt(3) * n[1]) / (n[2] / 100);
        if (Vx.Has(p, "Falta fase-terra")) return n[0] / (n[1] + n[2]) * n[2];
        if (Vx.Has(p, "I²t")) return n[2] / n[1];
        if (Vx.Has(p, "seção mínima")) return n[1] * Sqrt(n[2]) / n[0];
        return null;
    }
}
