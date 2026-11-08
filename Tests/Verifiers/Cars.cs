namespace EasyCreator.Verifiers;

/// <summary>Shared vehicle formulas for the verifiers (SI units inside).</summary>
static class Cv
{
    public const double G = 9.8, Rho = 1.2;
    public static double Ms(double kmh) => kmh / 3.6;
    public static double Aero(double rho, double cd, double a, double v) => 0.5 * rho * cd * a * v * v;
    /// <summary>Top speed (m/s) where P (W) = aero·v + Crr·m·g·v; crr = 0 gives the pure-drag speed.</summary>
    public static double Vmax(double p, double rho, double cd, double a, double crr, double m, double g)
    {
        double lo = 0, hi = 400;
        for (int i = 0; i < 200; i++)
        {
            double v = (lo + hi) / 2;
            if (Aero(rho, cd, a, v) * v + crr * m * g * v < p) lo = v; else hi = v;
        }
        return lo;
    }
    public static double Rpm2W(double rpm) => rpm * 2 * Math.PI / 60;
}

/// <summary>Cars lessons 01..20 solved from the prompt text.</summary>
[Verifies("car-01-forcas", "car-02-motor", "car-03-cambio", "car-04-tracao", "car-05-frenagem", "car-06-curvas", "car-07-aerodinamica",
    "car-08-suspensao", "car-09-eletrico", "car-10-projeto-fsae", "car-11-velocidade-maxima", "car-12-escalonamento", "car-13-balanco-freio",
    "car-14-energia-freios", "car-15-downforce", "car-16-suspensao-avancada", "car-17-ev-estrada", "car-18-motor-termico",
    "car-19-acerto-asa", "car-20-projeto-esportivo")]
public sealed class VCars : IExerciseVerifier
{
    const double G = Cv.G;
    static double Ms(double k) => k / 3.6;
    static double Aero(double rho, double cd, double a, double v) => Cv.Aero(rho, cd, a, v);

    public double? Solve(string p)
    {
        try { return Run(p); } catch (ArgumentOutOfRangeException) { return null; }
    }

    double? Run(string p)
    {
        var n = Pn.Nums(p);
        bool H(params string[] s) => Vx.Has(p, s);
        if (H("tanque")) return n[0] * n[4] / (n[1] * n[3] / 1000) * n[2];
        if (System.Text.RegularExpressions.Regex.IsMatch(p, @"bateria–roda [0-9,]+, bateria de"))
        {
            double m = n[0], cd = n[1], a = n[2], crr = n[3], eta = n[4], c = n[5];
            double F(double mass, double kmh, double rho, double g) => Aero(rho, cd, a, Ms(kmh)) + crr * mass * g;
            if (H("Qual a força total de resistência")) return F(m, n[6], n[7], n[8]);
            if (H("Qual a potência nas rodas")) return F(m, n[6], n[7], n[8]) * Ms(n[6]) / 1000;
            if (H("que energia, em kWh, sai da bateria")) return F(m, n[7], Cv.Rho, G) * n[6] * 1000 / 3.6e6 / eta;
            if (H("Qual o consumo a")) return F(m, n[6], n[8], n[9]) * n[7] * 1000 / 3.6e6 / eta;
            if (H("Aumenta-se a bateria"))
            {
                double dc = n[6], sp = n[7], v = n[8], rho = n[9], g = n[10];
                double r0 = c * eta * 3.6e6 / F(m, v, rho, g) / 1000;
                double r1 = (c + dc) * eta * 3.6e6 / F(m + dc * 1000 / sp, v, rho, g) / 1000;
                return H("autonomia a mais") ? r1 - r0 : r1;
            }
            if (H("Qual a autonomia a")) return c * n[7] / 100 * eta * 3.6e6 / F(m, n[6], n[8], n[9]) / 1000;
            if (H("km de autonomia se perdem"))
            {
                double u = n[8] / 100;
                double R(double v) => c * u * eta * 3.6e6 / F(m, v, Cv.Rho, G) / 1000;
                return R(n[7]) - R(n[6]);
            }
        }

        // ---------------- 01
        if (H("inclinação máxima da subida"))
        {
            double m = n[0], cd = n[1], a = n[2], crr = n[3], pw = n[4] * 1000, k = n[5], v = Ms(n[6]), rho = n[7], g = n[8];
            double f = pw / v - Aero(rho, cd, a, v);
            double r = f / (m * g), amp = Math.Sqrt(1 + crr * crr), phi = Math.Atan(crr);   // sinθ + crr·cosθ = r
            return Math.Tan(Math.Asin(r / amp) - phi) * k;
        }
        if (H("força resultante", "deve ser")) return n[0] * n[1];
        if (H("força de tração nas rodas")) return n[5] * 1000 / n[4];
        if (H("soma do arrasto do ar com a resistência")) return Aero(n[6], n[1], n[2], n[4]) + n[3] * n[0] * n[7];
        if (H("Qual a força de arrasto aerodinâmico")) return Aero(n[5], n[1], n[2], Ms(n[4]));
        if (H("velocidade constante", "rendimento de", "Qual a potência (em kW) que o motor"))
        {
            double v = Ms(n[4]); return (Aero(n[5], n[1], n[2], v) + n[3] * n[0] * n[6]) * v / 1000 / (n[7] / 100);
        }
        if (H("potência (em kW) que as rodas precisam"))
        {
            double v = Ms(n[4]); return (Aero(n[5], n[1], n[2], v) + n[3] * n[0] * n[6]) * v / 1000;
        }
        if (H("Qual a massa do kart")) return n[0] / n[1];
        if (H("nova velocidade máxima", "vezes maior")) return n[0] * Math.Cbrt(n[1]);
        if (H("Qual a aceleração do carro nesse instante"))
        {
            double v = n[4]; return (n[5] * 1000 / v - Aero(n[6], n[1], n[2], v) - n[3] * n[0] * n[7]) / n[0];
        }
        if (H("Qual a sua aceleração?", "força resultante de")) return n[1] / n[0];
        if (H("potência constante de", "Quanto tempo leva de")) return n[0] * (Math.Pow(Ms(n[3]), 2) - Math.Pow(Ms(n[2]), 2)) / (2 * n[1] * 1000);

        // ---------------- 02
        if (H("x + x²")) return Engine(p, n);
        if (H("Um motor produz", "Qual o torque")) return n[0] * 1000 / Cv.Rpm2W(n[1]);
        if (H("Um motor tem potência máxima de", "Qual o torque nesse ponto")) return n[0] * 1000 / Cv.Rpm2W(n[1]);
        if (H("Um motor entrega", "Qual a potência")) return n[0] * Cv.Rpm2W(n[1]) / 1000;
        if (H("Um motor de", "A quantas rpm ele está girando")) return n[1] * 1000 / n[0] * 60 / (2 * Math.PI);

        // ---------------- 03
        if (H("Que relação de marcha faz o motor girar")) return Cv.Rpm2W(n[0]) * n[3] / Ms(n[1]) / n[2];
        if (H("Que diferencial faz a velocidade máxima")) return Cv.Rpm2W(n[2]) * n[1] / Ms(n[3]) / n[0];
        if (H("Qual a relação total (marcha")) return n[1] * n[2];
        if (H("Qual a força de tração na estrada")) return n[0] * n[1] * n[2] * n[3] / n[4];
        if (H("Qual o torque nas rodas")) return n[0] * n[1] * n[2] * n[3];
        if (H("velocidade máxima real"))
        {
            double m = n[0], cd = n[1], a = n[2], crr = n[3], pw = n[4] * 1000, gear = n[5], d = n[6], r = n[7], rpm = n[8], rho = n[9], g = n[10];
            double vp = Cv.Vmax(pw, rho, cd, a, crr, m, g), vr = Cv.Rpm2W(rpm) * r / (gear * d);
            return Math.Min(vp, vr) * 3.6;
        }
        if (H("km/h, a", "marcha escolhida")) return Cv.Rpm2W(n[3]) * n[0] / (n[1] * n[2]) * 3.6;
        if (H("Desprezando as resistências")) return n[1] * n[2] * n[3] * n[4] / n[5] / n[0];
        if (H("a quantas rpm o motor gira a")) return Ms(n[3]) / n[0] * n[1] * n[2] * 60 / (2 * Math.PI);

        // ---------------- 04
        if (H("peso sobre o eixo dianteiro parado"))
        {
            double m = n[0], f = n[1] / 100, h = n[2], L = n[3], a = n[4], g = n[5];
            if (H("sobre o eixo traseiro")) return m * g * (1 - f) + m * a * h / L;
            return m * g * f - m * a * h / L;
        }
        if (H("passam do eixo dianteiro para o traseiro")) return n[0] * n[3] * n[1] / n[2];
        if (H("empinada")) return n[5] * (n[2] / 100) * n[3] / n[4];
        if (H("sem patinar e sem empinar"))
        {
            double f = n[2] / 100, L = n[3], h = n[4], mu = n[5], g = n[6];
            return Math.Min(mu * g, g * f * L / h);
        }
        if (H("antes de as rodas patinarem"))
        {
            double f = n[0] / 100, L = n[1], h = n[2], mu = n[3], g = n[4];
            if (H("tração dianteira")) return mu * g * f / (1 + mu * h / L);
            if (H("tração traseira")) return mu * g * (1 - f) / (1 - mu * h / L);
            if (H("tração integral")) return mu * g;
        }

        // ---------------- 05
        if (H("Há um obstáculo parado a", "maior velocidade inicial"))
        {
            double d = n[0], t = n[1], mu = n[2], g = n[3], b = 1 / (2 * mu * g);
            return (-t + Math.Sqrt(t * t + 4 * b * d)) / (2 * b) * 3.6;
        }
        if (H("velocidade do impacto"))
        {
            double v = Ms(n[0]), d = n[1], t = n[2], mu = n[3], g = n[4];
            double rem = d - v * t; if (rem < 0) return v * 3.6;
            double v2 = v * v - 2 * mu * g * rem; return v2 <= 0 ? 0 : Math.Sqrt(v2) * 3.6;
        }
        if (H("A MAIS")) return Math.Pow(Ms(n[0]), 2) / (2 * n[3]) * (1 / n[2] - 1 / n[1]);
        if (H("desaceleração constante de")) return Math.Pow(Ms(n[0]), 2) / (2 * n[1]);
        if (H("DESCIDA de") || H("SUBIDA de"))
        {
            double v = Ms(n[0]), th = Math.Atan(n[1] / 100), mu = n[2], g = n[3];
            double dec = H("DESCIDA") ? g * (mu * Math.Cos(th) - Math.Sin(th)) : g * (mu * Math.Cos(th) + Math.Sin(th));
            return v * v / (2 * dec);
        }
        if (H("distância total para parar (reação + frenagem)")) return Ms(n[0]) * n[3] + Math.Pow(Ms(n[0]), 2) / (2 * n[1] * n[2]);
        if (H("distância só de frenagem")) return Math.Pow(Ms(n[0]), 2) / (2 * n[1] * n[2]);
        if (H("distância total necessária para parar")) return Ms(n[0]) * n[2] + Math.Pow(Ms(n[0]), 2) / (2 * n[3] * n[4]);
        if (H("só durante a reação")) return Ms(n[1]) * n[0];
        if (H("desaceleração máxima, em m/s²")) return n[1] * n[2];

        // ---------------- 06
        if (H("coeficiente de atrito mínimo")) return Math.Pow(Ms(n[1]), 2) / (n[0] * n[2]);
        if (H("força lateral, em N")) return n[0] * Math.Pow(Ms(n[2]), 2) / n[1];
        if (H("curva plana de raio")) return Math.Sqrt(n[1] * n[2] * n[0]) * 3.6;
        if (H("mantém", "aceleração lateral")) return Math.Sqrt(n[0] * n[1]) * 3.6;
        if (H("bitola"))
        {
            double t = n[0], h = n[1], r = n[2], mu = n[3], g = n[4];
            double vcap = Math.Sqrt(g * r * t / (2 * h)), vsk = Math.Sqrt(mu * g * r);
            if (H("começaria a capotar")) return vcap * 3.6;
            if (H("menor valor")) return Math.Min(vcap, vsk) * 3.6;
            if (H("aceleração lateral que faz")) return g * t / (2 * h);
        }
        if (H("sobrelevação"))
        {
            double r = n[0], th = Pn.Rad(n[1]), mu = n[2], g = n[3];
            return Math.Sqrt(r * g * (Math.Sin(th) + mu * Math.Cos(th)) / (Math.Cos(th) - mu * Math.Sin(th))) * 3.6;
        }
        if (H("menor raio possível")) return Math.Pow(Ms(n[0]), 2) / (n[1] * n[2]);
        if (H("Qual a aceleração lateral, em m/s²")) return Math.Pow(Ms(n[1]), 2) / n[0];
        if (H("SEM depender do atrito")) return Pn.Deg(Math.Atan(Math.Pow(Ms(n[1]), 2) / (n[0] * n[3])));

        // ---------------- 07
        if (H("De quantos por cento o consumo"))
        {
            double m = n[0], cd = n[1], a = n[2], crr = n[3], v1 = Ms(n[6]), v2 = Ms(n[7]), rho = n[8], g = n[9];
            return ((Aero(rho, cd, a, v2) + crr * m * g) / (Aero(rho, cd, a, v1) + crr * m * g) - 1) * 100;
        }
        if (H("quantos litros a menos"))
        {
            double m = n[0], cd = n[1], a = n[2], crr = n[3], eta = n[4] / 100, e = n[5], v1 = Ms(n[6]), v2 = Ms(n[7]), rho = n[8], g = n[9], d = n[10];
            double L(double v) => (Aero(rho, cd, a, v) + crr * m * g) * d * 1000 / 3.6e6 / (eta * e);
            return L(Math.Max(v1, v2)) - L(Math.Min(v1, v2));
        }
        if (H("litros por"))
        {
            double m = n[0], cd = n[1], a = n[2], crr = n[3], v = Ms(n[4]), eta = n[5] / 100, e = n[6], rho = n[7], g = n[8], d = n[9];
            return (Aero(rho, cd, a, v) + crr * m * g) * d * 1000 / 3.6e6 / (eta * e);
        }
        if (H("Qual a força de arrasto do ar, em N")) return Aero(n[3], n[0], n[1], Ms(n[2]));
        if (H("só para vencer o ar", "força de arrasto, em N")) return n[3] * n[2] * n[2];
        if (H("só o arrasto do ar consome")) return Aero(n[3], n[0], n[1], Ms(n[2])) * Ms(n[2]) / 1000;
        if (H("só para vencer o ar", "quantos kW o ar consumirá")) return n[0] * n[2] * n[2] * n[2];
        if (H("Quanta energia mecânica, em kWh"))
        {
            double m = n[0], cd = n[1], a = n[2], crr = n[3], v = Ms(n[4]), rho = n[7], g = n[8];
            return (Aero(rho, cd, a, v) + crr * m * g) * 1000 / 3.6e6;
        }

        // ---------------- 08
        if (H("razão de amortecimento ζ = c")) return n[0] / (n[3] * Math.Sqrt(n[2] * n[1]));
        if (H("passa a sustentar mais")) return n[0] * Math.Sqrt(n[1] / (n[1] + n[2]));
        if (H("Qual a frequência natural, em Hz")) return Math.Sqrt(n[1] / n[0]) / (2 * Math.PI);
        if (H("Qual a rigidez da mola, em N/m", "sustenta")) return n[1] * Math.Pow(2 * Math.PI * n[0], 2);
        if (H("rigidez de cada mola")) { double m = n[0] * n[1] / 100 / n[2]; return m * Math.Pow(2 * Math.PI * n[3], 2); }
        if (H("amortecimento c necessário em cada roda")) { double m = n[0] * n[1] / 100 / n[2]; return 2 * n[4] * m * 2 * Math.PI * n[3]; }
        if (H("dá ζ =")) return 2 * n[0] * Math.Sqrt(n[2] * n[1]);
        if (H("Que massa por roda ela pode sustentar")) return n[0] / Math.Pow(2 * Math.PI * n[1], 2);
        if (H("ressonância")) return Math.Sqrt(n[2] / n[1]) / (2 * Math.PI) * n[0] * 3.6;
        if (H("a mola comprime só com esse peso")) return n[0] * n[2] / n[1] * 1000;

        // ---------------- 09
        if (H("Qual o custo de uma viagem")) return n[0] * n[1] / n[2] / n[3] * n[4];
        if (H("Quantas horas leva para carregar")) return (n[1] - n[0]) / 100 * n[2] / (n[3] * n[4]);
        if (H("Quanta potência, em kW, sai da bateria")) return n[0] / (n[2] / 100);
        if (H("Que energia, em kWh, ele gasta numa viagem")) return n[0] * n[2] / n[1];
        if (H("nas rodas para andar") && H("Qual o consumo")) return n[0] / (n[2] / 100) / n[1] * n[3];
        if (H("rendimento bateria–roda", "Qual o consumo"))
        {
            double m = n[0], cd = n[1], a = n[2], crr = n[3], eta = n[4], v = Ms(n[5]), rho = n[6], g = n[7], d = n[8];
            return (Aero(rho, cd, a, v) + crr * m * g) * d * 1000 / 3.6e6 / eta;
        }
        if (H("descarregando-a por completo")) return n[1] / n[0] * n[2];
        if (H("Qual a autonomia, em km?", "Um carro elétrico tem bateria")) return n[0] / n[1] * n[2];
        if (H("o resto é reserva")) return n[0] * n[1] / 100 / n[2] * n[3];
        if (H("rendimento bateria–roda", "qual a autonomia"))
        {
            double m = n[0], cd = n[1], a = n[2], crr = n[3], eta = n[4], v = Ms(n[5]), rho = n[6], g = n[7], c = n[8], u = n[9] / 100;
            return c * u * eta * 3.6e6 / (Aero(rho, cd, a, v) + crr * m * g) / 1000;
        }

        // ---------------- 10
        if (H("de quantos por cento a velocidade máxima da curva"))
            return (WingV(n[0], n[1], n[2], n[3], n[4], n[5]) / Math.Sqrt(n[1] * n[5] * n[2]) - 1) * 100;
        if (H("Qual a força de downforce a")) return 0.5 * n[3] * n[1] * Math.Pow(Ms(n[2]), 2);
        if (H("sem aerodinâmica", "numa curva de raio")) return Math.Sqrt(n[0] * n[2] * n[1]) * 3.6;
        if (H("completa uma volta de")) return n[0] / n[1] * 3.6;
        if (H("contando o downforce")) return WingV(n[0], n[1], n[3], n[2], n[4], n[5]) * 3.6;
        if (H("freia no limite dos pneus")) return Math.Pow(Ms(n[2]), 2) / (2 * n[0] * n[1]);
        if (H("prova de resistência")) return n[0] * n[2] / 60;
        if (H("skidpad", "downforce cresce")) return 2 * Math.PI * n[0] / WingV(n[1], n[2], n[0], n[3], n[4], n[5]);
        if (H("skidpad")) return 2 * Math.PI * n[0] / Math.Sqrt(n[1] * n[2] * n[0]);
        if (H("Quanto tempo leva uma volta")) return n[0] / Ms(n[1]);

        // ---------------- 11
        if (H("Qual o seu Cd?")) return n[0] * 1000 / (0.5 * n[3] * n[2] * Math.Pow(Ms(n[1]), 3));
        if (H("Qual deve ser a relação total"))
        {
            double pw = n[0] * 1000, r = n[1], cd = n[2], a = n[3], rpm = n[4], rho = n[5];
            return Cv.Rpm2W(rpm) * r / Math.Cbrt(2 * pw / (rho * cd * a));
        }
        if (H("Despreze o rolamento e o limite", "velocidade máxima, em km/h"))
            return Math.Cbrt(2 * n[2] * 1000 / (n[3] * n[0] * n[1])) * 3.6;
        if (H("Qual a força de arrasto nessa velocidade máxima"))
            return n[2] * 1000 / Math.Cbrt(2 * n[2] * 1000 / (n[3] * n[0] * n[1]));
        if (H("Quantos kW o ar consome") || H("nas rodas para chegar a")) return Aero(n[3], n[0], n[1], Ms(n[2])) * Ms(n[2]) / 1000;
        if (H("Uma carenagem reduz o Cd")) return n[0] / Math.Cbrt(1 - n[1] / 100);
        if (H("contando o rolamento")) return Cv.Vmax(n[4] * 1000, n[5], n[1], n[2], n[3], n[0], n[6]) * 3.6;

        // ---------------- 12
        if (H("Numa troca de marcha")) return n[2] * n[1] / n[0];
        if (H("Qual a relação da") && H("marchas em progressão geométrica"))
        {
            double i = n[1], gi = n[2], j = n[3], gj = n[4], k = n[5];
            return gi * Math.Pow(Math.Pow(gj / gi, 1 / (j - i)), k - i);
        }
        if (H("Qual a razão q")) return Math.Pow(n[2] / n[3], 1 / (n[0] - 1));
        if (H("Com pneus de raio", "numa marcha de relação")) return Cv.Rpm2W(n[2]) * n[0] / (n[1] * n[3]) * 3.6;
        if (H("A que velocidade, em km/h, o motor atinge o limitador"))
        {
            double N = n[0], g1 = n[2], gN = n[3], d = n[4], r = n[5], rpm = n[6], k = n[7];
            double g = g1 * Math.Pow(Math.Pow(gN / g1, 1 / (N - 1)), k - 1);
            return Cv.Rpm2W(rpm) * r / (g * d) * 3.6;
        }
        if (H("A quantas rpm ele cai logo após a troca")) return n[2] * n[1] / n[0];
        if (H("a quantas rpm o motor cai")) return n[6] * Math.Pow(n[3] / n[2], 1 / (n[0] - 1));

        // ---------------- 13
        if (H("fração do peso total (em %) está no eixo traseiro"))
            return (1 - n[1] / 100 - n[5] * n[3] / (n[4] * n[2])) * 100;
        if (H("fração ideal da força de frenagem")) return (n[1] / 100 + n[5] * n[3] / n[2]) * 100;
        if (H("carga total no eixo dianteiro")) return n[0] * n[4] * n[1] / 100 + n[0] * n[5] * n[3] / n[2];
        if (H("quantos newtons de carga passam")) return n[0] * n[5] * n[3] / n[2];
        if (H("freios dianteiros, em N, no balanço ideal"))
        {
            double m = n[0], f = n[1] / 100, L = n[2], h = n[3], g = n[4], mu = n[5]; return mu * (m * g * f + m * mu * g * h / L);
        }
        if (H("antes de algum eixo travar"))
        {
            double f = n[1] / 100, L = n[2], h = n[3], g = n[4], b = n[5] / 100, mu = n[6];
            double den = b - mu * h / L, af = den <= 0 ? double.PositiveInfinity : mu * g * f / den;
            return Math.Min(af, mu * g * (1 - f) / ((1 - b) + mu * h / L));
        }

        // ---------------- 14
        if (H("recuperando")) return 0.5 * n[0] * Math.Pow(Ms(n[1]), 2) * n[2] / 100 / 3600;
        if (H("transformar em calor") || H("Qual a energia cinética")) return 0.5 * n[0] * Math.Pow(Ms(n[1]), 2) / 1000;
        if (H("cada disco dianteiro absorve")) return 0.5 * n[0] * Math.Pow(Ms(n[1]), 2) * n[2] / 100 / n[3] / 1000;
        if (H("os freios dissipam")) return 0.5 * n[0] * (Math.Pow(Ms(n[1]), 2) - Math.Pow(Ms(n[2]), 2)) / 1000;
        if (H("potência média de frenagem")) return 0.5 * n[0] * Math.Pow(Ms(n[1]), 2) / n[2] / 1000;
        if (H("frenagem regenerativa")) return n[0] * n[5] * n[1] * n[2] / 100 / 3.6e6 / n[3] * n[4];
        if (H("frenagens seguidas"))
            return n[7] + n[1] * 0.5 * n[0] * Math.Pow(Ms(n[2]), 2) * n[3] / 100 / n[4] / (n[5] * n[6]);
        if (H("De quantos °C sobe")) return 0.5 * n[0] * Math.Pow(Ms(n[1]), 2) * n[2] / 100 / n[3] / (n[4] * n[5]);

        // ---------------- 15
        if (H("De quantos por cento a aderência aumenta")) return 0.5 * n[4] * n[2] * Math.Pow(Ms(n[3]), 2) / (n[0] * n[5]) * 100;
        if (H("Qual o downforce, em N?") && H("Um carro com asas")) return 0.5 * n[2] * n[0] * Math.Pow(Ms(n[1]), 2);
        if (H("carga total sobre os pneus")) return n[0] * n[5] + 0.5 * n[4] * n[2] * Math.Pow(Ms(n[3]), 2);
        if (H("força máxima de aderência total")) return n[1] * (n[0] * n[5] + 0.5 * n[4] * n[2] * Math.Pow(Ms(n[3]), 2));
        if (H("N de downforce a", "Qual o downforce, em N, a")) return n[0] * Math.Pow(n[2] / n[1], 2);
        if (H("razão sustentação/arrasto")) return n[0] / n[2] * Ms(n[1]) / 1000;
        if (H("no limite. Qual a velocidade máxima")) return WingV(n[0], n[1], n[3], n[2], n[4], n[5]) * 3.6;
        if (H("De quantos km/h a velocidade máxima da curva aumenta"))
            return (WingV(n[0], n[1], n[2], n[3], n[4], n[5]) - Math.Sqrt(n[1] * n[5] * n[2])) * 3.6;
        if (H("Qual o seu Cl·A")) return 2 * n[0] / (n[2] * Math.Pow(Ms(n[1]), 2));

        // ---------------- 16
        if (H("Qual a razão de amortecimento ζ?")) return n[2] / (2 * Math.Sqrt(n[1] * n[0]));
        if (H("frequência de pulo (ride)")) return Math.Sqrt(n[1] * n[2] * n[2] / n[0]) / (2 * Math.PI);
        if (H("Para uma frequência de")) return n[1] * Math.Pow(2 * Math.PI * n[0], 2);
        if (H("Projete uma suspensão", "Qual a rigidez da mola")) return n[0] * Math.Pow(2 * Math.PI * n[1], 2) / (n[2] * n[2]);
        if (H("Qual a rigidez efetiva na roda")) return n[0] * n[1] * n[1];
        if (H("Que rigidez a mola deve ter")) return n[0] / (n[1] * n[1]);
        if (H("O amortecedor trabalha")) return n[1] / (n[0] * n[0]);
        if (H("Projete uma suspensão", "c do amortecedor")) return 2 * n[2] * n[0] * 2 * Math.PI * n[1] / (n[3] * n[3]);
        if (H("Que amortecimento na roda c")) return 2 * n[0] * Math.Sqrt(n[2] * n[1]);

        // ---------------- 17

        // ---------------- 18
        if (H("eficiência térmica ideal")) return (1 - Math.Pow(n[1], 1 - n[0])) * 100;
        if (H("eficiência térmica real")) return 3.6 / (n[1] / 1000 * n[0]) * 100;
        if (H("Qual o consumo, em L/")) return n[0] * n[2] / 1000 * (n[4] / n[1]) / n[3];
        if (H("Qual o BSFC") && H("tem eficiência térmica de")) return 3600 / (n[1] / 100 * n[0]);
        if (H("real atinge só")) return 3600 / (n[3] / 100 * (1 - Math.Pow(n[0], 1 - n[2])) * n[4]);
        if (H("Quantos kJ de calor")) return n[3] / (1 - Math.Pow(n[0], 1 - n[2]));
        if (H("Quantos kg de gasolina ele queima por hora")) return n[0] * n[1] / 1000;
        if (H("tanque")) return n[0] * n[4] / (n[1] * n[3] / 1000) * n[2];
        if (H("pontos percentuais") || H("aumenta a eficiência ideal")) return (Math.Pow(n[0], 1 - n[4]) - Math.Pow(n[2], 1 - n[4])) * 100;

        // ---------------- 19
        if (H("Quantos por cento a velocidade nas curvas"))
        {
            double ls = n[0], vsA = n[1], lc = n[2], vcA = n[3], vsB = n[4];
            double tA = ls / Ms(vsA) + lc / Ms(vcA), vcB = lc / (tA - ls / Ms(vsB));
            return (1 - vcB / Ms(vcA)) * 100;
        }
        if (H("Com que velocidade média, em km/h, são feitas as curvas"))
            return (n[0] - n[1]) / (n[3] - n[1] / Ms(n[2])) * 3.6;
        if (H("a asa A é mais rápida"))
            return (n[4] / Ms(n[2]) + n[5] / Ms(n[3])) - (n[4] / Ms(n[0]) + n[5] / Ms(n[1]));
        if (H("se gasta nas curvas") || H("se gasta nas retas")) return n[0] / Ms(n[1]);
        if (H("Qual o tempo de volta")) return n[0] / Ms(n[1]) + n[2] / Ms(n[3]);

        // ---------------- 20
        if (H("Qual o consumo a"))
        {
            double m = n[0], cd = n[1], a = n[2], crr = n[3], em = n[4] / 100, et = n[5] / 100, e = n[6], v = Ms(n[7]), d = n[8], rho = n[9], g = n[10];
            return (Aero(rho, cd, a, v) + crr * m * g) * d * 1000 / 3.6e6 / (em * et * e);
        }
        if (H("Que potência, em kW, um carro de")) return n[0] * (Math.Pow(Ms(n[2]), 2) - Math.Pow(Ms(n[1]), 2)) / 2 / n[3] / (n[4] / 100) / 1000;
        if (H("relação potência/peso")) return n[1] / (n[0] / 1000);
        if (H("se o giro não limita")) return Cv.Vmax(n[4] * 1000 * n[5] / 100, n[6], n[1], n[2], n[3], n[0], n[7]) * 3.6;
        if (H("para reagir")) return Ms(n[2]) * n[3] + Math.Pow(Ms(n[2]), 2) / (2 * n[1] * n[4]);
        if (H("freia da velocidade máxima"))
        {
            double cd = n[0], a = n[1], pw = n[2] * 1000 * n[3] / 100, m = n[4], crr = n[5], mu = n[6], rho = n[7], g = n[8];
            double v = Cv.Vmax(pw, rho, cd, a, crr, m, g); return v * v / (2 * mu * g);
        }
        if (H("Um esportivo freia de")) return Math.Pow(Ms(n[0]), 2) / (2 * n[1] * n[2]);
        if (H("Quantos segundos ele ganha"))
        {
            double m = n[0], pw = n[1] * 1000, v1 = Ms(n[6]), v2 = Ms(n[7]), pc = n[8] / 100, dm = n[9] / 100;
            double T(double mass) => mass * (v2 * v2 - v1 * v1) / 2 / (pw * pc);
            return T(m) - T(m * (1 - dm));
        }
        if (H("Estime o tempo")) return n[2] * (Math.Pow(Ms(n[1]), 2) - Math.Pow(Ms(n[0]), 2)) / 2 / (n[3] * 1000 * n[4] / 100);
        return null;
    }

    static double WingV(double m, double mu, double r, double cla, double rho, double g)
    {
        double den = m / r - 0.5 * mu * rho * cla;
        return den <= 0 ? double.PositiveInfinity : Math.Sqrt(mu * m * g / den);
    }

    static double Engine(string p, List<double> n)
    {
        bool H(params string[] s) => Vx.Has(p, s);
        double pm = n[0] * 1000, rp = n[1];
        double T(double rpm) { double x = rpm / rp; return pm * (x + x * x - x * x * x) / Cv.Rpm2W(rpm); }
        if (H("Em quantas rpm o TORQUE")) return rp * 0.5;
        if (H("Quantas vezes")) return T(rp * 0.5) / T(rp);
        if (H("torque máximo")) return T(rp * 0.5);
        if (H("Qual a potência, em kW")) { double x = n[2] / rp; return n[0] * (x + x * x - x * x * x); }
        return T(n[2]);
    }
}
