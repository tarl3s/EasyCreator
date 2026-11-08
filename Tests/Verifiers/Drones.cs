using System.Text.RegularExpressions;

namespace EasyCreator.Verifiers;

/// <summary>Drone lessons 01..20 solved from the prompt text. Inches are 0,0254 m; a LiPo cell is 3,7 V nominal when the prompt gives no voltage.</summary>
[Verifies("drone-01-partes", "drone-02-empuxo-peso", "drone-03-bateria", "drone-04-helice", "drone-05-imu-sensores", "drone-06-pid-altura",
    "drone-07-atitude", "drone-08-gps-navegacao", "drone-09-regras", "drone-10-projeto-entrega", "drone-11-lipo", "drone-12-bateria-ideal",
    "drone-13-motor-kv", "drone-14-carga-pesada", "drone-15-pid-pesado", "drone-16-fusao-sensores", "drone-17-atitude-pesado",
    "drone-18-velocidade-alcance", "drone-19-teoria-rotor", "drone-20-projeto-mapeamento")]
public sealed class VDrones : IExerciseVerifier
{
    const double In = 0.0254;
    static double Rad(double d) => d * Math.PI / 180;
    static double Deg(double r) => r * 180 / Math.PI;
    static double Area(double dIn) => Math.PI * Math.Pow(dIn * In / 2, 2);
    static double Pid(double t, double rho, double a) => Math.Pow(t, 1.5) / Math.Sqrt(2 * rho * a);

    public double? Solve(string p0)
    {
        var p = Regex.Replace(p0, @"(?<=\d) (?=\d{3}\b)", "");
        try { return Run(p, Pn.Nums(p)); } catch (ArgumentOutOfRangeException) { return null; }
    }

    double? Run(string p, List<double> n)
    {
        bool H(params string[] s) => Vx.Has(p, s);
        if (H("Missão: ir")) return n[3] * (2 * n[0] / n[1] + n[2]) / 3600;

        // ---- 01
        if (H("Em que % do acelerador")) return n[0] / (n[1] * n[2]) * 100;
        if (H("Um drone tem: quadro"))
        {
            double mass = n[0] + n[1] + n[2] + n[3] + n[4] * (n[5] + n[6] + n[7]);
            return H("massa total") ? mass : n[4] * n[8] / mass;
        }
        if (H("Quantas células em série")) return n[1] / n[2];
        if (H("tensão nominal do pacote")) return n[1] * n[2];
        if (H("densidade de energia", "guarda quantos Wh")) return n[0] * n[1];
        if (H("guarda quantos Wh")) return n[0] * n[2] * n[1] / 1000;
        if (H("carga extra máxima") && H("T/W ≥")) return n[1] * n[2] / n[3] - n[0];
        if (H("dê, em pairado")) return n[0] / n[1];
        if (H("empuxo total máximo")) return n[0] * n[1];

        // ---- 02
        if (H("Um drone de", "motores de", "empuxo máximo cada"))
        {
            double m = n[0], N = n[1], t = n[2], tw = N * t / m;
            if (H("Em que % do acelerador")) return m / (N * t) * 100;
            if (H("relação empuxo/peso")) return tw;
            if (H("carga extra máxima")) return N * t / n[3] - m;
            if (H("g de empuxo total")) return N * t;
            if (H("aceleração vertical máxima")) return (tw - 1) * n[3];
            if (H("aceleração horizontal")) return n[3] * Math.Sqrt(tw * tw - 1);
            if (H("quanto tempo leva")) return Math.Sqrt(2 * n[3] / ((tw - 1) * n[4]));
            if (H("maior inclinação")) return Deg(Math.Acos(1 / tw));
        }

        // ---- 03
        if (H("Qual a corrente média")) return n[3] / (n[0] * n[2]);
        if (H("Qual a energia, em Wh")) return n[0] * n[2] * n[1] / 1000;
        if (H("Qual a energia, em kJ")) return n[0] * n[2] * n[1] / 1000 * 3.6;
        if (H("Que capacidade, em mAh")) return n[1] * n[2] / 60 / (n[3] / 100) / (n[0] * n[4]) * 1000;
        if (H("Quantos minutos paira", "hélices de"))
        {
            double m = n[0], N = n[1], d = n[2], fm = n[3], eta = n[4] / 100, rho = n[5], g = n[6], nb = n[7], s = n[8], mah = n[9], u = n[10] / 100;
            double pel = N * Pid(m * g / N, rho, Area(d)) / (fm * eta);
            return nb * s * 3.7 * mah / 1000 * u / pel * 60;
        }
        if (H("O drone consome", "A em média")) return n[0] * n[1] * n[2] * 0 + n[1] / 1000 * n[4] / 100 / n[3] * 60;
        if (H("tempo de voo, em minutos") || H("Quantos minutos de voo com")) return n[0] * n[2] * n[1] / 1000 * n[4] / 100 / n[3] * 60;

        // ---- 04
        if (H("Trocar uma hélice")) return Pid(n[2], n[3], Area(n[0])) - Pid(n[2], n[3], Area(n[1]));
        if (H("potência ideal total para pairar")) return n[1] * Pid(n[0] * 9.8 / n[1] * 0 + n[0] * n[4] / n[1], n[3], Area(n[2]));
        if (H("Qual a potência real, em W")) return Pid(n[1], n[3], Area(n[0])) / n[2];
        if (H("Qual a potência ideal, em W")) return Pid(n[1], n[2], Area(n[0]));
        if (H("gramas-força por watt")) return n[1] * n[3] / n[2] / Pid(n[1], n[4], Area(n[0]));
        if (H("velocidade da ponta")) return n[1] * 2 * Math.PI / 60 * n[0] * In / 2;
        if (H("Qual a velocidade induzida, em m/s")) return Math.Sqrt(n[1] / (2 * n[2] * Area(n[0])));
        if (H("Qual a área do disco")) return Math.PI * Math.Pow(n[0] * n[2], 2) / 4;
        if (H("escorregamento")) return n[3] * 60 / (n[1] / 100 * n[0] * In);

        // ---- 05
        if (H("erro em regime do filtro complementar"))
        {
            double tau = n[0] / n[1]; return tau / (tau + n[2]);
        }
        if (H("confia mais no acelerômetro")) return n[1] / (n[2] * Math.PI * n[0]);
        if (H("Qual a constante de tempo τ")) return n[1] * n[0] / (n[2] - n[1]);
        if (H("Filtro complementar com α")) return n[0] * (n[1] + n[2] * n[3]) + (1 - n[0]) * n[4];
        if (H("Qual a inclinação, em graus")) return Deg(Math.Atan(n[0] / n[1]));
        if (H("giroscópio mede")) return n[0] * n[1];
        if (H("mantém, em regime")) return n[1] * n[2] / (1 - n[1]) * n[0];
        if (H("quantos graus de erro")) return n[0] * n[1];

        // ---- 06
        if (H("Modelo simples da altura"))
        {
            double kk = n[4] * n[1], wn = Math.Sqrt(n[2] * kk); return 2 * n[3] * wn / kk;
        }
        if (H("controlador P com Kp")) return n[0] * n[1];
        if (H("PID com Kp")) return n[0] * n[3] + n[1] * n[4] + n[2] * n[5];
        if (H("Que Kp (saída em fração")) return n[0] / 100 / n[1];
        if (H("Qual o erro e = alvo")) return n[0] - n[1];
        if (H("erro de altura que sobra em regime")) return n[0] / 100 / n[1];
        if (H("Qual a derivada do erro")) return (n[1] - n[0]) / n[2];
        if (H("igualar o acelerador de pairar")) return n[0] / 100 / (n[2] * n[1]);

        // ---- 07
        if (H("Qual a saída P")) return n[0] * n[1];
        if (H("Qual o torque total")) return 2 * n[0] * n[1] * n[3];
        if (H("Qual o I, em kg·m²")) return n[0] * n[1] * n[2] * n[2];
        if (H("Qual a aceleração angular, em rad/s²") && H("recebe um torque")) return n[3] / (n[2] * n[0] * n[1] * n[1]);
        if (H("Quanto tempo leva, em s, para girar"))
        {
            double i = n[2] * n[0] * n[1] * n[1], a = n[3] / i; return Math.Sqrt(2 * Rad(n[4]) / a);
        }
        if (H("Qual o erro de rolagem")) return n[1] - n[0];
        if (H("Que velocidade angular, em °/s")) { double i = n[2] * n[0] * n[1] * n[1], a = n[3] / i; return Deg(Math.Sqrt(2 * a * Rad(n[4]))); }
        if (H("aceleração angular em °/s²")) return Deg(n[3] / (n[2] * n[0] * n[1] * n[1]));

        // ---- 08
        if (H("Quantos por cento o tempo total é maior"))
        {
            double v = n[0], d = n[1], w = n[2]; return ((d / (v - w) + d / (v + w)) / (2 * d / v) - 1) * 100;
        }
        if (H("distância em linha reta"))
        {
            double lat = n[0], per = n[4], dn = n[1] * per, de = n[2] * per * Math.Cos(Rad(lat)); return Math.Sqrt(dn * dn + de * de);
        }
        if (H("Que distância percorre, em m")) return n[0] * n[1] * 60;
        if (H("Quantos metros isso vale")) return n[0] * n[2];
        if (H("Qual a velocidade média")) return n[0] / n[1];
        if (H("fica") && H("parado fotografando")) return (n[0] / n[1] + 2 * n[2] / n[3] * 0 + n[2] / n[3] + n[4] + n[2] / n[3] + n[5] / n[1]) / 60;
        if (H("sem contar nada parado")) return (n[0] / n[1] + 2 * n[2] / n[3] + n[4] / n[5]) / 60;
        if (H("tempo leva ida e volta, em s")) return n[1] / (n[0] - n[2]) + n[1] / (n[0] + n[2]);
        if (H("Quanto tempo leva, em s?")) return n[1] / n[0];

        // ---- 09
        if (H("perder quantos gramas")) return n[0] - n[1];
        if (H("altitude máxima")) return n[0] + n[1];
        if (H("Quantos metros são")) return n[0] * n[2] / n[1];
        if (H("Quantos pés são")) return n[0] * n[2] / n[1];

        // ---- 10
        if (H("A potência cresce com a massa")) return n[1] * Math.Pow((n[0] + n[3]) / n[0], n[2]);
        if (H("Quantos Wh gasta o voo de cruzeiro")) return 2 * n[0] / n[1] * n[2] / 3600;
        if (H("Missão: pairar")) return (n[0] * n[1] + (n[2] + n[3]) / n[4] * n[5] + n[6] * n[7]) / 3600;
        if (H("potência ∝ massa")) return n[4] / n[5] * (n[1] * Math.Pow((n[0] + n[3]) / n[0], n[2]) + n[1]) / 3600;
        if (H("a ida dura")) return n[0] * n[1] / 3600;
        if (H("paira", "na decolagem")) return (n[0] + n[1]) * n[2] / 3600;
        if (H("qual a capacidade mínima da bateria, em Wh")) return n[0] / (n[2] / 100);
        if (H("capacidade mínima de uma bateria")) return n[0] / (1 - n[1] / 100) / (n[2] * n[3]) * 1000;
        if (H("tempo leva a ida, em minutos")) return n[0] / n[1] / 60;

        // ---- 11
        if (H("mAh") && (H("LiPo") || H("Uma bateria de")))
        {
            if (H("Uma bateria de", "Qual a corrente máxima contínua")) return n[1] * n[0] / 1000;
            if (H("Uma bateria de", "Qual o seu C")) return n[1] / (n[0] / 1000);
            double s = n[0], mah = n[1], c = n[2], rc = n[3] / 1000, v0 = s * 3.7, rt = s * rc;
            if (H("Qual a eficiência da bateria"))
            {
                double pw = n[4], i = (v0 - Math.Sqrt(v0 * v0 - 4 * rt * pw)) / (2 * rt); return (v0 - i * rt) / v0 * 100;
            }
            if (H("Resolvendo P")) { double pw = n[4]; return (v0 - Math.Sqrt(v0 * v0 - 4 * rt * pw)) / (2 * rt); }
            if (H("aproximando V pela tensão nominal")) return n[4] / v0;
            if (H("Que C mínimo")) return n[4] / v0 / (mah / 1000);
            if (H("de quantos volts cai")) return n[4] * rt;
            if (H("Quantos watts viram calor")) { double i = n[4] / v0; return i * i * rt; }
            if (H("resistência interna total")) return s * n[3];
        }

        // ---- 12
        if (H("multiplicada por")) return n[0] * Math.Pow(n[1], n[2]);
        if (H("guarda quantos Wh")) return n[0] * n[1];
        if (H("Que massa de bateria, em kg, guarda")) return n[0] / n[1];
        if (H("MAIOR tempo de pairado")) return n[0] / (n[1] - 1);
        if (H("bateria ideal"))
        {
            double m0 = n[0], p0 = n[1], k = n[2], mb = n[3] * m0, e = mb * n[4] * n[5] / 100; return e / (p0 * Math.Pow((m0 + mb) / m0, k)) * 60;
        }
        if (H("A MAIS"))
        {
            double m0 = n[0], p0 = n[1], k = n[2];
            double T(double mb) => mb * n[5] * n[6] / 100 / (p0 * Math.Pow((m0 + mb) / m0, k)) * 60;
            return T(n[4]) - T(n[3]);
        }
        if (H("pairado, em minutos, com uma bateria de") || H("quantos minutos paira, usando"))
        {
            double m0 = n[0], p0 = n[1], k = n[2], mb = n[3];
            return mb * n[4] * n[5] / 100 / (p0 * Math.Pow((m0 + mb) / m0, k)) * 60;
        }

        // ---- 13
        if (H("Que KV o motor precisa")) return n[2] * 60 / (n[1] * In) / (n[3] / 100) / n[0];
        if (H("Qual o seu KV")) return n[0] / n[1];
        if (H("Qual o torque, em N·m")) return n[2] / (n[3] * Math.PI * n[0]) * n[1] * 0 + 60 / (2 * Math.PI * n[0]) * n[1];
        if (H("constante de torque")) return 60 / (2 * Math.PI * n[0]);
        if (H("potência elétrica")) return n[2] * n[1] * 1000 / n[0] / n[3];
        if (H("potência mecânica no eixo")) return n[3] * n[1] * n[2] / 100;
        if (H("Qual a velocidade de passo")) return n[0] * n[1] * n[2] / 100 * n[3] * In / 60;
        if (H("rotação sem carga, em rpm")) return n[0] * n[1];

        // ---- 14
        if (H("motor(es) falhar")) return n[2] * (n[1] - n[3]) / n[1];
        if (H("amperes a menos")) return n[0] / n[2] - n[0] / n[4];
        if (H("Qual a corrente numa bateria")) return n[0] / n[2];
        if (H("cada motor precisa dar")) return n[2] * n[0] / n[1];
        if (H("mesmo com")) return (n[0] - n[3]) * n[1] / n[2];
        if (H("carga útil máxima")) return n[0] * n[1] / n[2] - n[3];
        if (H("Qual a massa total, em kg")) return n[0] * n[1] / n[2];

        // ---- 15
        if (H("fração do acelerador (em %)") || H("Que fração do acelerador")) return 100 / n[0];
        if (H("sobressinal")) return Math.Exp(-Math.PI * n[2] / Math.Sqrt(1 - n[2] * n[2])) * 100;
        if (H("que Kd dá ζ"))
        {
            double kk = n[1] * n[2], wn = Math.Sqrt(n[3] * kk); return 2 * n[4] * wn / kk;
        }
        if (H("qual o amortecimento ζ"))
        {
            double kk = n[5] * n[1], wn = Math.Sqrt(n[2] * kk); return n[3] * kk / (2 * wn);
        }
        if (H("Qual o seu T/W")) return 100 / n[0];
        if (H("Que Kp mínimo")) return n[0] / 100 / n[1];
        if (H("qual o erro de altura em regime")) return n[0] / 100 / n[1];
        if (H("frequência natural ωn")) return Math.Sqrt(n[1] * n[2] * n[0]);

        // ---- 16
        if (H("Kalman escalar com ruído de processo"))
        {
            double q = n[0], r = n[1], pp = 1;
            for (int i = 0; i < 10000; i++) { double pm = pp + q, k = pm / (pm + r); pp = (1 - k) * pm; }
            double pm2 = pp + q; return pm2 / (pm2 + r);
        }
        if (H("a incerteza vira P'")) return (1 - n[0] / (n[0] + n[1])) * n[0];
        if (H("Qual o ganho K = P/(P + R)")) return n[0] / (n[0] + n[1]);
        if (H("Qual o α = τ/(τ + dt)?") && H("deve cruzar")) { double tau = n[1] / (n[2] * Math.PI * n[0]); return tau / (tau + n[3]); }
        if (H("Qual o α = τ/(τ + dt)?")) return n[0] / (n[0] + n[1]);
        if (H("Filtro complementar com α")) return n[0] * (n[1] + n[2] * n[3]) + (1 - n[0]) * n[4];
        if (H("Kalman escalar: estimativa inicial"))
        {
            double x = n[0], pp = n[1], r = n[2];
            foreach (var z in new[] { n[3], n[4] }) { double k = pp / (pp + r); x += k * (z - x); pp = (1 - k) * pp; }
            return x;
        }
        if (H("filtro de Kalman com ganho")) return n[1] + n[0] * (n[2] - n[1]);

        // ---- 17
        if (H("atraso do motor") && H("representa"))
        {
            double m = n[0], l = n[1], k = n[2], d = n[3], tq = n[4], th = n[5], cc = n[7], dd = n[8], pw = n[9];
            double a = tq / (k * m * l * l), t0 = Math.Sqrt(2 * Rad(th) / a), tau = cc * Math.Pow(d / dd, pw); return tau / t0 * 100;
        }
        if (H("momento de inércia de um drone")) return n[0] * n[1] * n[2] * n[2];
        if (H("duas forças")) { double i = n[2] * n[0] * n[1] * n[1]; return n[5] * n[3] * n[4] / i; }
        if (H("constante de tempo τ ≈")) return n[1] * Math.Pow(n[0] / n[2], n[3]);
        if (H("Somando o atraso"))
        {
            double m = n[0], l = n[1], k = n[2], d = n[3], tq = n[4], th = n[5], cc = n[7], dd = n[8], pw = n[9];
            double a = tq / (k * m * l * l); return Math.Sqrt(2 * Rad(th) / a) + cc * Math.Pow(d / dd, pw);
        }
        if (H("chegar a")) return n[2] * n[0];
        if (H("após") && H("a partir do repouso")) { double a = n[3] / (n[2] * n[0] * n[1] * n[1]); return Deg(0.5 * a * n[4] * n[4]); }
        if (H("atraso de fase")) return Deg(Math.Atan(n[2] * Math.PI * n[1] * n[0]));

        // ---- 18
        if (H("Quantos por cento de energia por km"))
        {
            double p0 = n[0], k = n[1], v1 = n[3], vo = Math.Cbrt(p0 / (2 * k));
            double E(double v) => (p0 + k * v * v * v) / v; return (1 - E(vo) / E(v1)) * 100;
        }
        if (H("Qual a potência a")) return n[0] + n[1] * Math.Pow(n[2], 3);
        if (H("Quantos Wh por quilômetro")) return n[1] / n[0] * 1000 / 3600;
        if (H("alcance máximo"))
        {
            double p0 = n[0], k = n[1], vo = Math.Cbrt(p0 / (2 * k)); return n[2] / ((p0 + k * vo * vo * vo) / vo * 1000 / 3600);
        }
        if (H("Wh/km. Qual o alcance")) return n[0] / n[1];
        if (H("Que distância percorre, em km")) return n[0] * n[1] * 60 / 1000;
        if (H("utilizável) alimenta")) return n[0] * n[1] / 100 / n[2] * n[3] * 3.6;
        if (H("consumo por km é mínimo")) return Math.Cbrt(n[0] / (n[2] * n[1]));

        // ---- 19
        if (H("Quantos por cento a potência ideal diminui")) return (1 - n[0] / n[2]) * 100;
        if (H("razão entre a velocidade de descida")) return n[3] / Math.Sqrt(n[1] / (2 * n[2] * Area(n[0])));
        if (H("carga de disco")) return n[1] / Area(n[0]);
        if (H("potência total real")) return n[1] * Pid(n[0] * n[5] / n[1], n[4], Area(n[2])) / n[3];
        if (H("Qual a potência ideal T·vi")) return Pid(n[1], n[2], Area(n[0]));
        if (H("figure of merit") && H("Qual a potência real")) return Pid(n[1], n[3], Area(n[0])) / n[2];
        if (H("velocidade induzida em cada rotor")) return Math.Sqrt(n[0] * n[4] / n[1] / (2 * n[3] * Area(n[2])));
        if (H("Qual a velocidade induzida vi")) return Math.Sqrt(n[1] / (2 * n[2] * Area(n[0])));

        // ---- 20
        if (H("linhas de voo são necessárias") || H("comprimento total das linhas"))
        {
            double a = n[0], b = n[1], w = n[2], along = n[3];
            double across = Math.Abs(along - a) < 1e-9 ? b : a, lines = Math.Ceiling(across / w - 1e-9);
            return H("linhas de voo são necessárias") ? lines : lines * along / 1000;
        }
        if (H("quantos Wh gasta o cruzeiro"))
        {
            double p0 = n[0], k = n[1], vo = Math.Cbrt(p0 / (2 * k)); return (p0 + k * vo * vo * vo) * 2 * n[2] / vo / 3600;
        }
        if (H("Quantos Wh se economiza"))
        {
            double p0 = n[0], k = n[1], vo = Math.Cbrt(p0 / (2 * k)), d = n[2];
            double E(double v) => (p0 + k * v * v * v) * 2 * d / v / 3600; return E(n[4]) - E(vo);
        }
        if (H("voa") && H("min no total")) return n[0] * n[1] / 60;
        if (H("Que capacidade mínima, em mAh")) return n[0] / (n[3] / 100) / (n[1] * n[4]) * 1000;
        if (H("ida e volta, em minutos")) return 2 * n[0] / n[1] / 60;
        return null;
    }
}
