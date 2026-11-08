using System.Text.RegularExpressions;

namespace EasyCreator.Verifiers;

/// <summary>Submarines track sub-01..20: every numeric prompt solved from its Portuguese text with textbook formulas.</summary>
[Verifies("sub-01-arquimedes", "sub-02-flutuabilidade-neutra", "sub-03-lastro", "sub-04-pressao", "sub-05-casco-materiais",
    "sub-06-propulsao-arrasto", "sub-07-estabilidade", "sub-08-sensores-navegacao", "sub-09-controle-pid", "sub-10-projeto-rov",
    "sub-11-espuma-carga", "sub-12-velocidade-vertical", "sub-13-espessura-casco", "sub-14-sonar", "sub-15-energia-autonomia",
    "sub-16-pid-saturacao", "sub-17-mergulho-profundo", "sub-18-hidrodinamica", "sub-19-fadiga-seguranca", "sub-20-projeto-auv")]
public sealed class VSubmarines : IExerciseVerifier
{
    const double G = 9.8, Atm = 101325;
    static double Sq(double x) => x * x;
    /// <summary>Circle area in m² from a diameter in cm.</summary>
    static double Area(double dCm) => Math.PI * Sq(dCm / 100) / 4;

    public double? Solve(string prompt)
    {
        try { return Run(prompt); } catch (ArgumentOutOfRangeException) { return null; }
    }

    static double Rx(string p, string pattern) => Pn.Nums(Regex.Match(p, pattern).Groups[1].Value)[0];

    static double? Run(string prompt)
    {
        var p = Regex.Replace(prompt, @"\d[º]", "X");
        var n = Pn.Nums(p);
        bool H(params string[] s) => Vx.Has(p, s);

        // ---- 01 Archimedes
        if (H("Qual o empuxo sobre ele")) return n[1] * G * n[0] / 1000;
        if (H("Qual a densidade dele")) return n[0] / (n[1] / 1000);
        if (H("Que porcentagem do volume fica submersa")) return n[0] / n[1] * 100;
        if (H("Qual a tração no cabo")) return n[0] * G - n[2] * G * n[1] / 1000;
        if (H("Que carga máxima")) return n[4] * n[0] * n[1] * n[2] - n[3];
        if (H("Qual o calado")) return n[3] / (n[4] * n[0] * n[1]) * 100;
        if (H("Qual a densidade do material")) return n[0] / (n[0] - n[1]) * n[2];

        // ---- 02 neutral buoyancy
        if (H("Qual deve ser seu volume")) return n[0] / n[1] * 1000;
        if (H("Qual deve ser sua massa")) return n[0] * n[1] / 1000;
        if (H("força líquida vertical")) return n[2] * G * n[1] / 1000 - n[0] * G;
        if (H("precisam ser colocados para ficar neutro")) return Math.Abs(n[0] * n[2] / 1000 - n[1]);
        if (H("força líquida para baixo")) return (n[1] - n[2]) * n[0] / 1000 * G;
        if (H("voltar a ficar neutro")) return n[0] * (n[2] - n[1]) / 1000;
        if (H("força de empuxo perdida")) return n[1] * G * n[0] / 1000 * (n[2] / 100 * n[4] / n[3]);

        // ---- 03 ballast
        if (H("volume dos tanques de lastro")) return n[0] - n[2] * 1000 / n[1];
        if (H("massa de água, em toneladas")) return (n[1] * n[0] - n[2] * 1000) / 1000;
        if (H("Quanto tempo, em segundos")) return (n[0] - n[2] * 1000 / n[1]) * 1000 / n[3];
        if (H("flutuabilidade de reserva")) return (n[1] - n[0] * 1000 / n[2]) / n[1] * 100;
        if (H("volume do cilindro"))
        {
            double amb = n[3] + n[4] * G * n[0] / 1e5;
            return n[1] * amb / (n[2] + n[3]);
        }

        // ---- 04 pressure
        if (H("Qual a pressão absoluta a")) return n[2] + n[1] * G * n[0] / 1000;
        if (H("Qual a pressão manométrica")) return n[1] * G * n[0] / 1000;
        if (H("escotilha circular")) return n[2] * G * n[1] * Area(n[0]) / 1000;
        if (H("sensor marca")) return (n[0] - n[2]) * 1e5 / (n[1] * G);
        if (H("pressão absoluta chega a")) return (n[1] - 1) * n[3] / (n[0] * G);
        if (H("janela circular")) return n[2] * G * n[1] * Area(n[0]) / 1000;
        if (H("sino de mergulho"))
        {
            double p2 = Atm + n[2] * G * n[1];
            return Atm / p2 * 100;
        }

        // ---- 05 hull and materials
        if (H("tensão circunferencial")) return n[2] * n[0] / (n[1] / 1000);
        if (H("Qual a massa desse trecho")) return 2 * Math.PI * n[1] * n[0] * (n[2] / 1000) * n[3];
        if (H("Qual a espessura mínima da parede (mm) de um cilindro"))
            return n[3] > 0 ? n[2] * G * n[1] * n[0] * n[4] / (n[3] * 1e6) * 1000 : null;
        if (H("quanto mais espesso"))
        {
            double pr = n[6] * G * n[1];
            return pr * n[0] * n[3] / (n[2] * 1e6) / 2 * 1000;
        }
        if (H("esfera de pressão de raio", "Qual a espessura mínima da parede em mm")) return n[2] * G * n[1] * n[0] * n[4] / (2 * n[3] * 1e6) * 1000;
        if (H("profundidade máxima de operação no mar"))
        {
            double pmax = 2 * n[2] * 1e6 * (n[1] / 1000) / (n[0] * n[3]);
            return (pmax - Atm) / (n[4] * G);
        }

        // ---- 06 propulsion and drag
        if (H("Se a velocidade dobrar")) return 0.5 * n[3] * n[1] * Area(n[0]) * Sq(2 * n[2]);
        if (H("Qual o Cd?")) return 2 * n[3] / (n[2] * Area(n[0]) * Sq(n[1]));
        if (H("Qual o arrasto?")) return 0.5 * n[3] * n[1] * Area(n[0]) * Sq(n[2]);
        if (H("Por qual fator a potência aumenta")) return Math.Pow(n[4], 3);
        if (H("Qual a potência mecânica")) return 0.5 * n[3] * n[1] * Area(n[0]) * Math.Pow(n[2], 3);
        if (H("potência elétrica consumida")) return 0.5 * n[3] * n[1] * Area(n[0]) * Math.Pow(n[2], 3) / n[4];
        if (H("quantos minutos ele aguenta"))
        {
            double pe = 0.5 * n[3] * n[1] * Area(n[0]) * Math.Pow(n[2], 3) / n[4];
            return n[5] / pe * 60;
        }
        if (H("Qual a velocidade máxima na água"))
            return Math.Cbrt(n[2] * n[3] / (0.5 * n[4] * n[1] * Area(n[0])));

        // ---- 07 stability
        if (H("Qual a distância h entre o CB e o CG")) return n[2] / (n[0] * G * Math.Sin(Pn.Rad(n[1]))) * 100;
        if (H("Qual o torque restaurador τ")) return n[0] * G * n[1] / 100 * Math.Sin(Pn.Rad(n[2]));
        if (H("Um empurrão o inclina")) return n[0] * G * n[1] / 100 * Math.Sin(Pn.Rad(n[2]));
        if (H("maior torque restaurador possível")) return n[0] * G * n[1] / 100;
        if (H("período de oscilação")) return 2 * Math.PI * Math.Sqrt(n[1] / (n[0] * G * n[2] / 100));
        if (H("maior deslocamento lateral")) return n[0] * n[1] * Math.Tan(Pn.Rad(n[^1])) / n[2];
        if (H("ângulo de adernamento")) return Pn.Deg(Math.Atan(n[2] * n[^1] / (n[0] * n[1])));

        // ---- 08 sensors and navigation
        if (H("Quantos milissegundos leva o eco")) return 2 * n[0] / n[1] * 1000;
        if (H("a que distância está o obstáculo")) return n[1] * n[0] / 1000 / 2;
        if (H("marca", "kPa (absoluta)")) return (n[1] - n[2]) * 1000 / (n[0] * G);
        if (H("viés não corrigido")) return n[0] * n[1];
        if (H("erro lateral")) return n[0] * Math.Sin(Pn.Rad(n[1]));
        if (H("supondo água doce")) return n[1] * n[0] / n[2];

        // ---- 09 PID
        if (H("Qual o ganho Kp")) return n[0] / n[1];
        if (H("força de comando u = Kp·erro")) return n[0] * (n[1] - n[2]);
        if (H("controlador PD")) return n[0] * n[3] + n[1] * (n[3] - n[2]) / n[4];
        if (H("controlador PI")) return n[0] * n[5] + n[1] * (n[3] + n[4] + n[5]) * n[2];
        if (H("Que ganho Kp mínimo")) return n[0] / (n[1] / 100);
        if (H("erro de profundidade permanente")) return n[0] / n[1] * 100;

        // ---- 10 ROV design
        if (H("Qual a potência total máxima")) return n[0] * n[1] + n[2] + n[3] + n[4];
        if (H("guarda quantos Wh")) return n[0] * n[1];
        if (H("usa tudo no máximo")) return n[5] * n[6] / (n[0] * n[1] + n[2] + n[3] + n[4]) * 60;
        if (H("volume de espuma", "flutuação neutra")) return (n[0] - n[1]) / (n[3] / 1000 - n[2] / 1000);
        if (H("minutos de missão"))
        {
            double pw = n[0] * n[1] * n[2] / 100 + n[3];
            return n[4] * n[5] * n[6] / 100 / pw * 60;
        }

        // ---- 11 foam and payload
        if (H("massa aparente")) return n[1] * (1 - n[0] / n[2]);
        if (H("Qual o volume, em litros, de uma carga")) return n[0] / n[1] * 1000;
        if (H("Que volume de espuma de flutuação")) return n[0] * (1 - n[2] / n[1]) / (n[2] - n[3]) * 1000;
        if (H("Que massa máxima")) return n[0] * (n[2] - n[1]) / 1000 / (1 - n[2] / n[3]);
        if (H("aceleração inicial para cima")) return n[1] * (1 - 1000 / n[2]) * G / (1.5 * n[0]);

        // ---- 12 vertical speed
        if (H("Qual o arrasto, igual ao peso líquido")) return 0.5 * n[3] * n[1] * n[2] * Sq(n[0]);
        if (H("Qual o peso líquido, em N")) return n[0] * G;
        if (H("velocidade terminal de descida")) return Math.Sqrt(2 * n[0] * G / (n[3] * n[1] * Area(n[2])));
        if (H("Quantos segundos leva")) return n[3] / Math.Sqrt(2 * n[0] * G / (n[4] * n[1] * Area(n[2])));
        if (H("excesso de peso (em gramas")) return 0.5 * n[3] * n[1] * Area(n[0]) * Sq(n[2]) / G * 1000;

        // ---- 13 hull thickness
        if (H("Qual a tensão na parede")) return n[3] * G * n[2] * n[0] / (2 * n[1] / 1000) / 1e6;
        if (H("Uma esfera de raio", "Qual a espessura mínima"))
            return n[2] * G * n[1] * n[0] * n[4] / (2 * n[3] * 1e6) * 1000;
        if (H("Um cilindro de raio", "Qual a espessura mínima"))
            return n[2] * G * n[1] * n[0] * n[4] / (n[3] * 1e6) * 1000;
        if (H("flambagem"))
        {
            var q = Pn.Nums(p[p.IndexOf("Para uma esfera", StringComparison.Ordinal)..]);
            double pc = 2 * q[0] * 1e9 / Math.Sqrt(3 * (1 - q[1] * q[1])) * Sq(q[3] / 1000 / q[2]);
            return H("colapsaria") ? pc / (q[4] * G) : pc / 1e6;
        }
        if (H("massa de água que ela desloca"))
        {
            double t = n[5] * G * n[4] * n[0] * n[3] / (2 * n[2] * 1e6);
            return 4 * Math.PI * Sq(n[0]) * t * n[1] / (4.0 / 3 * Math.PI * Math.Pow(n[0], 3) * n[5]) * 100;
        }

        // ---- 14 sonar
        if (H("Quantos milissegundos leva para ouvir o eco")) return 2 * n[0] / n[1] * 1000;
        if (H("Qual a distância do alvo")) return n[1] * n[0] / 1000 / 2;
        if (H("maior número de pulsos por segundo")) return n[1] / (2 * n[0]);
        if (H("alcance máximo não ambíguo")) return n[1] / (2 * n[0]);
        if (H("qual a perda em dB"))
        {
            var q = Pn.Nums(p[p.IndexOf("Para r =", StringComparison.Ordinal)..]);
            return 20 * Math.Log10(q[0]) + q[1] * q[0] / 1000;
        }
        if (H("RSR em dB"))
        {
            double nf = Rx(p, @"NF = (-?[\d,.]+) dB"), fa = Rx(p, @"FA = ([−-]?[\d,.]+) dB"), nr = Rx(p, @"NR = ([\d,.]+) dB");
            double al = Rx(p, @"α = ([\d,.]+) dB/km"), r = Rx(p, @"alvo a ([\d,.]+) m");
            double pt = 20 * Math.Log10(r) + al * r / 1000;
            return nf - 2 * pt + fa - nr;
        }

        // ---- 15 energy and endurance
        if (H("energia mínima da bateria")) return n[0] * n[1];
        if (H("Quantas horas dura")) return n[0] / n[1];
        if (H("Qual o alcance, em km?") && !H("carga fixa")) return n[0] / n[1] * 3600 * n[2] / 1000;
        if (H("Qual o alcance, em km?"))
        {
            double k = 0.5 * n[6] * n[1] * Area(n[0]) / n[3];
            double pw = k * Math.Pow(n[2], 3) + n[4];
            return n[5] / pw * 3600 * n[2] / 1000;
        }
        if (H("maximiza o alcance"))
        {
            double k = 0.5 * n[4] * n[1] * Area(n[0]) / n[2];
            return Math.Cbrt(n[3] / (2 * k));
        }

        // ---- 16 PID saturation
        if (H("força realmente aplicada")) return Math.Min(n[0] * n[2], n[1]);
        if (H("o comando satura")) return n[1] / n[0];
        if (H("termo integral ao final")) return n[2] * n[0] * n[1];
        if (H("para sozinho chegar a esse valor")) return n[0] / (n[2] * n[1]);

        // ---- 17 deep dive
        if (H("Qual sua profundidade máxima de operação")) return n[0] / n[1];
        if (H("A que profundidade o casco colapsaria")) return n[0] * n[1];
        if (H("Quantos minutos dura a descida")) return n[0] / n[1] / 60;
        if (H("no fundo", "sobe a")) return n[0] / n[1] / 60 + n[2] + n[0] / n[3] / 60;
        if (H("soltando 12 kg de lastro") || H("emerge soltando"))
        {
            double v = Math.Sqrt(2 * n[3] * G / (n[4] * n[5] * Area(n[6])));
            return n[1] / n[0] / 60 + n[2] + n[1] / v / 60;
        }

        // ---- 18 hydrodynamics
        if (H("Qual o número de Reynolds") || H("Re = ρ·v·L/μ") && !H("o modelo deve")) return n[2] * n[1] * n[0] / n[3];
        if (H("potência gasta contra o arrasto")) return 0.5 * n[3] * n[1] * Area(n[0]) * Math.Pow(n[2], 3);
        if (H("começa a cavitar"))
        {
            double pabs = n[3] + n[1] * G * n[0];
            return Math.Sqrt(2 * (pabs - n[2]) / n[1]);
        }
        if (H("o modelo deve ser ensaiado")) return n[1] * n[3];

        // ---- 19 fatigue
        if (H("Quantos ciclos ainda restam")) return n[0] - n[1];
        if (H("Quantos mergulhos ainda")) return Math.Floor((n[0] - n[1]) / n[2]);
        if (H("dano acumulado"))
        {
            var q = Pn.Nums(p[p.IndexOf("Um casco sofreu", StringComparison.Ordinal)..]);
            return q[0] / q[1] + q[2] / q[3];
        }
        if (H("Basquin"))
        {
            var q = Pn.Nums(p[p.IndexOf("Com tensão", StringComparison.Ordinal)..]);
            return q[1] * Math.Pow(q[0] / q[2], Rx(p, @"S\^\(−(\d+)\)"));
        }

        // ---- 20 AUV mission
        if (H("comprimento total da trilha")) return n[1] / n[0];
        if (H("Quantas linhas (inteiro")) return Math.Ceiling(n[2] / n[0]);
        if (H("Quantos Wh gasta na varredura"))
        {
            double lines = Math.Ceiling(n[1] / n[2]);
            double track = lines * n[0] + (lines - 1) * n[4];
            return track / n[5] * n[6] / 3600;
        }
        if (H("quantas linhas completas"))
        {
            double per = (n[0] + n[1]) / n[2] * n[3] / 3600;
            return Math.Floor(n[4] / per);
        }
        return null;
    }
}
