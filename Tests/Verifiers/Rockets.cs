using System.Text.RegularExpressions;

namespace EasyCreator.Verifiers;

/// <summary>Rockets track rkt-01..20: every numeric prompt solved from its Portuguese text with textbook formulas.</summary>
[Verifies("rkt-01-garrafa-pet", "rkt-02-empuxo-impulso", "rkt-03-classes-empuxo-peso", "rkt-04-estabilidade", "rkt-05-arrasto",
    "rkt-06-recuperacao", "rkt-07-estagios", "rkt-08-tsiolkovsky", "rkt-09-orbita", "rkt-10-missao-sondagem", "rkt-11-agua-otima",
    "rkt-12-estavel-demais", "rkt-13-bocal", "rkt-14-propelentes", "rkt-15-estagios-otimos", "rkt-16-perdas-gravidade",
    "rkt-17-reentrada", "rkt-18-mecanica-orbital", "rkt-19-competicao", "rkt-20-lancador-orbital")]
public sealed class VRockets : IExerciseVerifier
{
    const double G = 9.8;
    static double Sq(double x) => x * x;
    /// <summary>Circle area in m² from a diameter in cm.</summary>
    static double Area(double dCm) => Math.PI * Sq(dCm / 100) / 4;

    public double? Solve(string prompt)
    {
        try { return Run(prompt); } catch (ArgumentOutOfRangeException) { return null; }
    }

    static double? Run(string prompt)
    {
        var p = Regex.Replace(prompt, @"\d[º]", "X");
        var n = Pn.Nums(p);
        bool H(params string[] s) => Vx.Has(p, s);

        // ---- 01 bottle rocket
        if (H("sem a água tem massa", "velocidade a água")) return n[0] * n[2] / n[1];
        if (H("depois de soltar a água", "massa de água")) return n[0] * n[2] / n[1];
        if (H("depois de expulsar a água", "Partindo do repouso")) return n[1] * n[2] / n[0];
        if (H("Um foguete de água tem", "Qual o empuxo")) return n[1] * n[2];
        if (H("aceleração inicial na vertical")) return n[1] * n[2] / n[0] - G;
        if (H("Quanto tempo dura a expulsão")) return n[3] / n[2];
        if (H("pressurizada", "bocal", "empuxo inicial")) return 2 * n[0] * 1000 * Area(n[1] / 10);
        if (H("pressurizada", "vazão de massa")) return n[2] * Area(n[1] / 10) * Math.Sqrt(2 * n[0] * 1000 / n[2]);
        if (H("pressurizada", "velocidade de saída")) return Math.Sqrt(2 * n[0] * 1000 / n[2]);
        if (H("pesa", "com a garrafa cheia"))
        {
            double dv = n[2] * Math.Log(n[0] / (n[0] - n[1]));
            return dv * dv / (2 * G);
        }

        // ---- 02 thrust and impulse
        if (H("impulso total de", "Qual o empuxo?")) return n[0] / n[1];
        if (H("impulso total de", "Quanto tempo")) return n[0] / n[1];
        if (H("empuxo constante de", "Qual o impulso total")) return n[0] * n[1];
        if (H("queima", "g de propelente", "empuxo médio")) return n[0] / 1000 * n[1] * n[2] / n[3];
        if (H("precisa entregar")) return n[1] / (n[0] * n[2]) * 1000;
        if (H("g de propelente", "Qual o impulso total")) return n[0] / 1000 * n[1] * n[2];
        if (H("dois trechos"))
        {
            double imp = n[0] * n[1] + n[2] * n[3];
            if (H("impulso total?")) return imp;
            if (H("empuxo médio")) return imp / (n[1] + n[3]);
            if (H("massa de propelente")) return imp / (n[4] * n[5]) * 1000;
        }
        if (H("aproximação", "altura máxima"))
        {
            double a = n[1] / n[0] - n[3], v = a * n[2];
            return 0.5 * a * n[2] * n[2] + v * v / (2 * n[3]);
        }

        // ---- 03 classes
        if (H("qual deve ser o empuxo")) return n[2] * n[0] * n[1];
        if (H("Qual a relação empuxo/peso?")) return n[2] / (n[0] * n[1]);
        if (H("maior massa de decolagem")) return n[0] / (n[1] * n[2]);
        if (H("Qual a aceleração inicial")) return n[1] / n[0] - n[2];
        if (H("haste", "Quanto tempo")) return Math.Sqrt(2 * n[2] / (n[1] / n[0] - n[3]));
        if (H("haste", "velocidade ao sair")) return Math.Sqrt(2 * (n[1] / n[0] - n[3]) * n[2]);
        if (H("antes do fim da queima")) return n[3] / ((n[0] + (n[1] - n[2]) / 1000) * n[4]);
        if (H("Quantas vezes")) return (n[0] + n[1] / 1000) / (n[0] + (n[1] - n[2]) / 1000);

        // ---- 04 stability
        if (H("a que distância da ponta deve estar o CP")) return n[1] + n[2] * n[0];
        if (H("Qual a margem estática em calibres?") && H("CP a")) return (n[2] - n[1]) / n[0];
        if (H("duas contribuições"))
        {
            double cp = (n[0] * n[1] + n[2] * n[3]) / (n[0] + n[2]);
            return H("Onde fica o CP") ? cp : (cp - n[5]) / n[4];
        }
        if (H("lastro colocado"))
        {
            double m = n[0], cg = n[1], cgNew = n[2] - n[5] * n[3], xb = n[4];
            return m * (cg - cgNew) / (cgNew - xb);
        }
        if (H("depois que o propelente acaba"))
        {
            double cg2 = (n[0] * n[1] - n[4] * n[5]) / (n[0] - n[4]);
            return (n[2] - cg2) / n[3];
        }

        // ---- 05 drag
        if (H("Se a velocidade dobrar")) return 0.5 * n[3] * n[1] * Area(n[0]) * Sq(2 * n[2]);
        if (H("Qual a força de arrasto")) return 0.5 * n[3] * n[1] * Area(n[0]) * Sq(n[2]);
        if (H("túnel de vento")) return 2 * n[1] / (n[3] * Area(n[0]) * Sq(n[2]));
        if (H("desaceleração total")) return 0.5 * n[5] * n[2] * Area(n[1]) * Sq(n[3]) / n[0] + n[4];
        if (H("potência dissipada")) return 0.5 * n[3] * n[1] * Area(n[0]) * Math.Pow(n[2], 3);
        if (H("percentual de F₀")) return 100 * (1 - n[2] / 100);
        if (H("cai de bico para baixo")) return Math.Sqrt(2 * n[0] * n[3] / (n[4] * n[2] * Area(n[1])));
        if (H("quanto ainda sobe contra o ar"))
        {
            double k = 0.5 * n[4] * n[2] * Area(n[1]), M = n[0];
            return M / (2 * k) * Math.Log(1 + k * Sq(n[3]) / (M * n[5]));
        }

        // ---- 06 recovery
        if (H("Qual a velocidade de descida?") && H("para chegar ao chão")) return n[0] / n[1];
        if (H("energia cinética no pouso")) return 0.5 * n[0] * Sq(n[1]);
        if (H("Quanto tempo leva até o chão")) return n[0] / n[1];
        if (H("velocidade de descida em equilíbrio")) return Math.Sqrt(2 * n[0] * n[4] / (n[3] * n[2] * Area(n[1] * 100)));
        if (H("quanto mais ele sobe até o apogeu")) return Sq(n[0]) / (2 * n[1]);
        if (H("o atraso ideal")) return n[0] / n[1];
        if (H("maior massa de foguete")) return 0.5 * n[3] * n[1] * Area(n[0] * 100) * Sq(n[2]) / n[4];
        if (H("Qual o diâmetro do paraquedas"))
            return Math.Sqrt(4 * (2 * n[1] * n[4] / (n[3] * n[0] * Sq(n[2]))) / Math.PI);
        if (H("distância horizontal", "paraquedas"))
        {
            double v = Math.Sqrt(2 * n[0] * n[6] / (n[5] * n[2] * Area(n[1] * 100)));
            return n[4] * n[3] / v;
        }

        // ---- 07 staging
        if (H("Qual o Δv que ele fornece")) return n[0] * Math.Log(n[1] / n[2]);
        if (H("Qual a razão de massa?") && H("começa com")) return n[0] / n[1];
        if (H("Qual o Δv total?") && H("dois estágios:") && H("razão de massa")) return n[0] * Math.Log(n[1]) + n[2] * Math.Log(n[3]);
        if (H("Quanto do Δv total vem")) return n[2] * Math.Log(n[3]);
        if (H("carga útil de", "Qual o Δv do"))
        {
            double pl = n[0], s2 = n[1], p2 = n[2], ve2 = n[3];
            return ve2 * Math.Log((pl + s2 + p2) / (pl + s2));
        }
        if (H("carga útil de", "Δv total do foguete"))
        {
            double pl = n[0], s2 = n[1], p2 = n[2], ve2 = n[3], s1 = n[4], p1 = n[5], ve1 = n[6];
            double d2 = ve2 * Math.Log((pl + s2 + p2) / (pl + s2));
            double m1 = pl + s2 + p2 + s1;
            return d2 + ve1 * Math.Log((m1 + p1) / m1);
        }
        if (H("Que massa de propelente o"))
        {
            double R = Math.Exp((n[0] - n[2]) / n[4]);
            return (R - 1) * (n[1] + n[3]);
        }

        // ---- 08 Tsiolkovsky
        if (H("razão de massa", "Qual o Δv?") && H("vₑ =")) return n[0] * Math.Log(n[1]);
        if (H("razão de massa necessária") && H("velocidade de exaustão")) return Math.Exp(n[0] / n[1]);
        if (H("massa inicial", "final") && !H("Foguete de sondagem")) return n[0] * Math.Log(n[1] / n[2]);
        if (H("razão de massa necessária") && H("Isp")) return Math.Exp(n[0] / (n[1] * n[2]));
        if (H("tem razão de massa")) return n[0] * n[2] * Math.Log(n[1]);
        if (H("velocidade de exaustão de um motor")) return n[0] * n[1];
        if (H("são propelente")) return n[2] * n[3] * Math.Log(n[0] / (n[0] - n[1]));
        if (H("Que massa de propelente ele deve carregar")) return n[0] * (Math.Exp(n[1] / (n[2] * n[3])) - 1);
        if (H("Que carga útil"))
        {
            double st = n[0] * n[1] / 100, R = Math.Exp(n[3] / n[2]);
            return (n[0] - R * st) / (R - 1);
        }

        // ---- 09 orbit
        if (H("A que altitude uma órbita circular")) return n[1] / Sq(n[0] * 1000) / 1000 - n[2];
        if (H("Qual a velocidade de um satélite em órbita circular")) return Math.Sqrt(n[1] / ((n[2] + n[0]) * 1000)) / 1000;
        if (H("voltas por dia")) return 86400 / (2 * Math.PI * Math.Sqrt(Math.Pow((n[^1] + n[0]) * 1000, 3) / n[^2]));
        if (H("Qual o período de uma órbita circular")) return 2 * Math.PI * Math.Sqrt(Math.Pow((n[2] + n[0]) * 1000, 3) / n[1]) / 60;
        if (H("transferência de Hohmann", "Δv"))
        {
            double mu = n[2], r1 = (n[3] + n[0]) * 1000, r2 = (n[3] + n[1]) * 1000;
            double dv1 = Math.Sqrt(mu / r1) * (Math.Sqrt(2 * r2 / (r1 + r2)) - 1);
            double dv2 = Math.Sqrt(mu / r2) * (1 - Math.Sqrt(2 * r1 / (r1 + r2)));
            if (H("segunda queima")) return dv2;
            if (H("primeira queima")) return dv1;
            return dv1 + dv2;
        }
        if (H("excentricidade"))
        {
            double r1 = (n[3] + n[0]) * 1000, r2 = (n[3] + n[1]) * 1000;
            return (r2 - r1) / (r2 + r1);
        }
        if (H("Quanto tempo dura uma transferência"))
        {
            double a = ((n[3] + n[0]) + (n[3] + n[1])) * 1000 / 2;
            return Math.PI * Math.Sqrt(a * a * a / n[2]) / 3600;
        }

        // ---- 10 sounding mission
        if (H("Foguete de sondagem"))
        {
            double m0 = n[0], mf = n[1], ve = n[2] * n[4], t = n[3], R = m0 / mf;
            double dv = ve * Math.Log(R), v = dv - n[4] * t;
            double hb = ve * t * (1 - Math.Log(R) / (R - 1)) - 0.5 * n[4] * t * t;
            if (H("Qual o Δv ideal")) return dv;
            if (H("velocidade no fim da queima")) return v;
            if (H("apogeu total")) return hb + v * v / (2 * n[4]);
            if (H("altitude no fim da queima")) return hb;
        }

        // ---- 11 water rocket optimum
        if (H("Qual a massa de água?")) return n[0] * n[1] / 100;
        if (H("sobra para comprimir")) return n[0] * 1000 - n[1];
        if (H("Quantos mL de água")) return n[0] * 1000 * n[1] / 100;
        if (H("pressão manométrica do ar"))
        {
            double v0 = n[0] * 1000 * (1 - n[1] / 100), p1 = n[2] + n[3];
            return p1 * v0 / (v0 + n[4]) - n[3];
        }
        if (H("garrafa vazia de", "Δv ideal")) return n[2] * Math.Log((n[0] / 1000 + n[1]) / (n[0] / 1000));
        if (H("garrafa vazia de", "altura máxima"))
        {
            double dv = n[2] * Math.Log((n[0] / 1000 + n[1]) / (n[0] / 1000));
            return dv * dv / (2 * G);
        }
        if (H("que porcentagem do volume"))
        {
            double dry = n[1] / 1000, R = Math.Exp(n[2] / n[3]);
            return (R - 1) * dry / n[0] * 100;
        }

        // ---- 12 too stable
        if (H("Qual a velocidade do vento")) return n[0] * Math.Tan(Pn.Rad(n[1]));
        if (H("Qual o ângulo de ataque")) return Pn.Deg(Math.Atan(n[1] / n[0]));
        if (H("arrasto total (corpo + aletas)"))
            return 0.5 * n[6] * Sq(n[5]) * (n[1] * Area(n[0]) + n[2] * n[3] * 1e-4 * n[4]);
        if (H("aletas aumentam o arrasto")) return 100 * n[2] * n[3] * 1e-4 * n[4] / (n[1] * Area(n[0]));
        if (H("torque restaurador"))
        {
            double q = 0.5 * n[4] * Sq(n[1]), alpha = Math.Atan(n[2] / n[1]);
            double N = q * Area(n[0]) * n[3] * alpha;
            return N * n[5] * n[0] / 100;
        }
        if (H("quanto de altura ele perde")) return Sq(n[0] * Math.Sin(Pn.Rad(n[1]))) / (2 * G);
        if (H("distância horizontal percorrida")) return Sq(n[0]) * Math.Sin(Pn.Rad(n[1])) * Math.Cos(Pn.Rad(n[1])) / G;

        // ---- 13 nozzle
        if (H("Qual o empuxo (F = ṁ·v")) return n[2] * Area(n[0] / 10) * Sq(n[1]);
        if (H("Qual a vazão de massa?") && H("bocal")) return n[2] * Area(n[0] / 10) * n[1];
        if (H("pressão manométrica", "Bernoulli")) return Math.Sqrt(2 * n[0] * 1000 / n[1]);
        if (H("conservação da vazão")) return n[0] * Sq(n[1] / n[2]);
        if (H("área de saída")) return n[0] * n[1] + (n[2] - n[3]) * 1000 * n[4] * 1e-4;
        if (H("bocal ideal"))
        {
            double g = n[0], r = 8314 / n[1], t = n[2], pc = n[3], pe = n[4];
            return Math.Sqrt(2 * g / (g - 1) * r * t * (1 - Math.Pow(pe / pc, (g - 1) / g)));
        }

        // ---- 14 propellants
        if (H("Qual o impulso específico?")) return n[0] / n[1];
        if (H("Qual a velocidade de exaustão?") && H("impulso específico de")) return n[0] * n[1];
        if (H("Quanto tempo ele pode ficar ligado")) return n[2] / (n[0] / (n[1] * n[3]));
        if (H("Qual a vazão de massa de propelente")) return n[0] / (n[1] * n[2]);
        if (H("volume total de tanques"))
        {
            double ox = n[2] * n[0] / (n[0] + n[1]), fu = n[2] - ox;
            return (ox / n[3] + fu / n[4]) * 1000;
        }
        if (H("massa de oxigênio líquido")) return n[2] * n[0] / (n[0] + n[1]);
        if (H("mesmo tanque"))
        {
            double V = n[0], dry = n[1], g0 = n[6];
            double a = n[3] * g0 * Math.Log((dry + n[2] * V) / dry), b = n[5] * g0 * Math.Log((dry + n[4] * V) / dry);
            return Math.Abs(a - b);
        }

        // ---- 15 optimal staging
        if (H("toneladas são de propelente")) return n[1] * (1 - n[0]);
        if (H("fração estrutural ε = m_estrutura")) return n[0] / (n[0] + n[1]);
        if (H("Qual a razão de massa R = (1 + λ)/(ε + λ)?")) return (1 + n[1]) / (n[0] + n[1]);
        if (H("estágios idênticos tem")) return n[0] * n[3] * Math.Log((1 + n[2]) / (n[1] + n[2]));
        if (H("porcentagem da massa de decolagem"))
        {
            double R = Math.Exp(n[3] / (n[0] * n[2])), lam = (1 - R * n[1]) / (R - 1);
            return 100 / Math.Pow(1 + lam, n[0]);
        }

        // ---- 16 losses
        if (H("rouba")) return n[0] / n[1];
        if (H("Qual a perda por gravidade (g·t)")) return n[0] * n[1];
        if (H("Δv ideal − g·t")) return n[0] * n[3] * Math.Log(n[1]) - n[3] * n[2];
        if (H("Qual a perda por gravidade durante a queima")) return n[3] * n[2] / (n[0] * 1000 / (n[1] * n[3]));
        if (H("Quanto dura a queima?")) return n[2] / (n[0] * 1000 / (n[1] * n[3]));
        if (H("razão de massa é necessária")) return Math.Exp((n[0] + 0.5 * G * n[1] + n[2]) / (n[3] * n[4]));

        // ---- 17 reentry
        if (H("energia cinética por quilograma")) return 0.5 * Sq(n[0]) / 1e6;
        if (H("Qual a energia cinética total")) return 0.5 * n[0] * Sq(n[1]) / 1e6;
        if (H("coeficiente balístico", "m de diâmetro")) return n[0] / (n[2] * Area(n[1] * 100));
        if (H("dobro da área frontal")) return n[0] / 2;
        if (H("Qual a velocidade terminal") || H("velocidade terminal (arrasto = peso)")) return Math.Sqrt(2 * n[0] * n[4] / (n[3] * n[2] * Area(n[1] * 100)));
        if (H("desaceleração máxima")) return Sq(n[0]) * Math.Sin(Pn.Rad(n[1])) / (2 * n[4] * n[3]) / G;

        // ---- 18 orbital mechanics (μ and radius given in the prompt)
        if (H("órbita circular a") && H("em torno do astro")) return Math.Sqrt(n[1] / ((n[2] + n[0]) * 1000)) / 1000;
        if (H("velocidade de escape")) return Math.Sqrt(2 * n[1] / ((n[2] + n[0]) * 1000)) / 1000;
        if (H("3ª lei de Kepler")) return n[1] * Math.Pow(n[^1] / n[0], 1.5);
        if (H("volta completa em torno do astro"))
            return Math.Cbrt(n[1] * Sq(n[0] * 3600) / (4 * Sq(Math.PI))) / 1000 - n[2];
        if (H("razão entre a velocidade no perigeu")) return (n[2] + n[1]) / (n[2] + n[0]);
        if (H("Qual sua velocidade no apogeu"))
        {
            double rp = (n[^1] + n[0]) * 1000, ra = (n[^1] + n[1]) * 1000, a = (rp + ra) / 2;
            return Math.Sqrt(n[^2] * (2 / ra - 1 / a)) / 1000;
        }

        // ---- 19 competition
        if (H("Qual o apogeu?")) return n[0] + Sq(n[1]) / (2 * G);
        if (H("tempo total de voo"))
        {
            double apo = n[1] + Sq(n[2]) / (2 * G);
            return n[0] + n[2] / G + apo / n[3];
        }
        if (H("contando o arrasto quadrático"))
        {
            double k = 0.5 * n[5] * n[2] * Area(n[1]), M = n[0];
            return n[3] + M / (2 * k) * Math.Log(1 + k * Sq(n[4]) / (M * G));
        }
        if (H("Que diâmetro de paraquedas"))
        {
            double apo = n[3] + Sq(n[4]) / (2 * G), tFall = n[0] - n[2] - n[4] / G;
            double vp = apo / tFall;
            return Math.Sqrt(4 * (2 * n[1] * G / (n[6] * n[5] * Sq(vp))) / Math.PI);
        }

        // ---- 20 orbital launcher
        if (H("velocidade orbital de um satélite leve")) return Math.Sqrt(n[1] / ((n[2] + n[0]) * 1000));
        if (H("Qual o Δv que o lançador precisa fornecer"))
            return Math.Sqrt(n[4] / ((n[5] + n[0]) * 1000)) + n[2] - n[3] * Math.Cos(Pn.Rad(n[1]));
        if (H("Que massa de propelente ele carrega")) return n[2] * (Math.Exp(n[1] / (n[0] * n[3])) - 1);
        if (H("Qual a razão de massa necessária")) return Math.Exp(n[0] / (n[1] * n[2]));
        if (H("lançador leve de dois estágios"))
        {
            double pl = n[0], s2 = n[1], p2 = n[2], ve2 = n[3], s1 = n[4], p1 = n[5], ve1 = n[6];
            double m1 = pl + s2 + p2 + s1;
            return ve2 * Math.Log((pl + s2 + p2) / (pl + s2)) + ve1 * Math.Log((m1 + p1) / m1);
        }
        return null;
    }
}
