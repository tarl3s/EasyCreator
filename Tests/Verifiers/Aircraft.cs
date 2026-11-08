namespace EasyCreator.Verifiers;

/// <summary>Aeronautics lessons 01..20 solved from the prompt text (CD0 is read as "CDz" so its 0 is not a number).</summary>
[Verifies("aero-01-sustentacao", "aero-02-angulo-ataque", "aero-03-arrasto-polar", "aero-04-estol", "aero-05-decolagem", "aero-06-estabilidade",
    "aero-07-propulsao", "aero-08-helicoptero-pairar", "aero-09-cauda-autorrotacao", "aero-10-projeto-breguet", "aero-11-planador",
    "aero-12-curvas-fator-carga", "aero-13-teto", "aero-14-curva-potencia", "aero-15-alcance-jato", "aero-16-peso-balanceamento",
    "aero-17-helicoptero-altitude", "aero-18-rotor-avanco", "aero-19-autorrotacao", "aero-20-projeto-mato")]
public sealed class VAircraft : IExerciseVerifier
{
    static double Rad(double d) => d * Math.PI / 180;
    static double Deg(double r) => r * 180 / Math.PI;
    static double Lift(double rho, double v, double s, double cl) => 0.5 * rho * v * v * s * cl;
    static double Vs(double m, double s, double rho, double clm, double g) => Math.Sqrt(2 * m * g / (rho * s * clm));
    static double K(double ar, double e) => 1 / (Math.PI * e * ar);
    static double LdMax(double cd0, double ar, double e) => 1 / (2 * Math.Sqrt(K(ar, e) * cd0));
    static double Rpm(double r) => r * 2 * Math.PI / 60;

    public double? Solve(string p0)
    {
        var p = p0.Replace("CD0", "CDz");
        try { return Run(p, Pn.Nums(p)); } catch (ArgumentOutOfRangeException) { return null; }
    }

    double? Run(string p, List<double> n)
    {
        bool H(params string[] s) => Vx.Has(p, s);

        // ---- 01
        if (H("Que CL a asa precisa ter")) return n[0] * n[4] / (0.5 * n[3] * n[2] * n[2] * n[1]);
        if (H("gera", "N de sustentação", "Qual o CL?")) return 2 * n[2] / (n[3] * n[1] * n[1] * n[0]);
        if (H("velocidade for multiplicada por")) return n[0] * n[1] * n[1];
        if (H("Qual a sustentação, em N?") && H("CL = ") && !H("inclinação da curva")) return Lift(n[3], n[1], n[0], n[2]);
        if (H("Quantos N de sustentação ela perde")) return Lift(n[3], n[1], n[0], n[2]) - Lift(n[5], n[1], n[0], n[2]);
        if (H("depois de carregar mais")) return n[1] * Math.Sqrt((n[0] + n[2]) / n[0]);
        if (H("voa em nível com CL")) return Math.Sqrt(2 * n[0] * n[4] / (n[3] * n[1] * n[2]));
        if (H("velocidade verdadeira")) return n[0] * Math.Sqrt(n[1] / n[3]);
        if (H("Um modelo em escala")) return n[3] * Math.Sqrt(n[1]);
        if (H("gera", "N de sustentação", "Qual a velocidade, em m/s?")) return Math.Sqrt(2 * n[2] / (n[3] * n[0] * n[1]));
        if (H("Qual a área de asa necessária")) return 2 * n[0] * n[4] / (n[3] * n[1] * n[1] * n[2]);

        // ---- 02
        if (H("inclinação da curva CL"))
        {
            double a = n[0], a0 = n[1];
            if (H("Qual o CL a")) return a * (n[2] - a0);
            if (H("Qual a sustentação, em N?")) return Lift(n[4], n[3], n[2], a * (n[5] - a0));
            if (H("Que massa, em kg")) return Lift(n[4], n[3], n[2], a * (n[5] - a0)) / n[6];
            if (H("velocidade de estol em voo nivelado")) return Vs(n[3], n[4], n[5], a * (n[2] - a0), n[6]);
            if (H("Que ângulo de ataque, em graus, dá CL")) return n[2] / a + a0;
            if (H("Que ângulo de ataque, em graus, ele precisa"))
                return 2 * n[2] * n[6] / (n[5] * n[4] * n[4] * n[3]) / a + a0;
            if (H("Quantos graus o ângulo de ataque aumenta"))
            {
                double m = n[2], s = n[3], v1 = n[4], v2 = n[5], rho = n[6], g = n[7];
                double Cl(double v) => 2 * m * g / (rho * v * v * s);
                return (Cl(v2) - Cl(v1)) / a;
            }
        }

        // ---- 03
        if (H("fator de Oswald"))
        {
            double cd0 = n[0], ar = n[1], e = n[2], k = K(ar, e);
            if (H("De quantos por cento aumenta")) return (Math.Sqrt(n[3] / ar) - 1) * 100;
            if (H("Em qual CL")) return Math.Sqrt(cd0 / k);
            if (H("Qual a máxima razão L/D")) return LdMax(cd0, ar, e);
            if (H("Qual a razão sustentação/arrasto (L/D) com CL")) return n[3] / (cd0 + k * n[3] * n[3]);
            if (H("Qual o arrasto total CD")) return cd0 + k * n[3] * n[3];
            if (H("coeficiente de arrasto induzido")) return k * n[3] * n[3];
            if (H("O avião pesa")) return n[3] / LdMax(cd0, ar, e);
            if (H("voa na velocidade de melhor L/D"))
            {
                double w = n[3] * n[6], s = n[4], rho = n[5], cl = Math.Sqrt(cd0 / k);
                double v = Math.Sqrt(2 * w / (rho * s * cl));
                return w / LdMax(cd0, ar, e) * v / 1000;
            }
        }

        // ---- 04
        if (H("CLmáx") || H("Os flapes") || H("flapes que elevam") || H("velocidade de aproximação"))
        {
            if (H("Os flapes elevam o CLmáx para")) return (1 - Math.Sqrt(n[2] / n[5])) * 100;
            if (H("Qual o CLmáx necessário")) { double vs = n[5] / n[4]; return 2 * n[0] * n[3] / (n[2] * n[1] * vs * vs); }
            if (H("Qual a carga alar")) return n[0] * n[4] / n[1];
            if (H("Qual a velocidade de estol, em km/h")) return Vs(n[0], n[1], n[3], n[2], n[4]) * 3.6;
            if (H("Com flapes totalmente baixados")) return Vs(n[0], n[1], n[3], n[5], n[4]);
            if (H("mais alto")) return Vs(n[0], n[1], n[6], n[2], n[4]);
            if (H("inclinação, qual a velocidade de estol")) return Vs(n[0], n[1], n[3], n[2], n[4]) / Math.Sqrt(Math.Cos(Rad(n[5])));
            if (H("Qual a velocidade de aproximação")) return n[5] * Vs(n[0], n[1], n[3], n[2], n[4]);
            if (H("Qual a velocidade de estol, em m/s?")) return Vs(n[0], n[1], n[3], n[2], n[4]);
            if (H("área de asa mínima")) { double vs = n[5] / n[4]; return 2 * n[0] * n[3] / (n[2] * n[1] * vs * vs); }
        }
        // ---- 05
        if (H("velocidade de decolagem (", "em km/h")) return n[5] * Vs(n[0], n[1], n[3], n[2], n[4]) * 3.6;
        if (H("A aceleração média na corrida é"))
        {
            double vlo = n[7] * Vs(n[0], n[1], n[3], n[2], n[4]); return (vlo * vlo - n[6] * n[6]) / (2 * n[5]);
        }
        if (H("A corrida tem aceleração média", "vento de proa")) { double vlo = n[7] * Vs(n[0], n[1], n[3], n[2], n[4]) - n[6]; return vlo * vlo / (2 * n[5]); }
        if (H("passar um obstáculo")) return n[7] / ((n[5] - n[6]) / (n[0] * n[4]));
        if (H("razão de subida, em m/s") && H("hélice de rendimento")) return (n[4] * n[3] * 1000 - n[2] * n[1]) / (n[0] * n[5]);
        if (H("ângulo de subida, em graus") && H("hélice de rendimento"))
            return Deg(Math.Asin((n[4] * n[3] * 1000 - n[2] * n[1]) / (n[0] * n[5]) / n[1]));
        if (H("Qual a velocidade de decolagem (", "em m/s")) return n[5] * Vs(n[0], n[1], n[3], n[2], n[4]);
        if (H("Qual a aceleração média, em m/s²") && H("atrito de rolamento")) return (n[5] - n[6] * n[0] * n[4]) / n[0];

        // ---- 06
        if (H("Em que % da CMA está o CG") && H("começa na estação")) return (n[2] - n[1]) / n[0] * 100;
        if (H("margem estática, em %?") && H("ponto neutro = ")) { double vh = n[2] * n[3] / (n[0] * n[1]); return n[4] + n[5] * vh - n[6]; }
        if (H("Qual a margem estática, em % da CMA?", "corda média")) return n[0] - n[1];
        if (H("Onde deve ficar o CG")) return n[0] - n[1];
        if (H("Onde está o ponto neutro") && H("Use ponto neutro = ")) { double vh = n[2] * n[3] / (n[0] * n[1]); return n[4] + n[5] * vh; }
        if (H("Onde está o ponto neutro")) return n[0] + n[1];
        if (H("volume de cauda")) return n[2] * n[3] / (n[0] * n[1]);
        if (H("Em que estação, em m, está o CG")) return n[1] + n[0] * n[2] / 100;
        if (H("estação mais traseira")) return n[2] + n[1] * (n[0] - n[3]) / 100;
        if (H("Que área de estabilizador"))
        {
            double s = n[0], c = n[1], lt = n[2], np = n[3], a = n[4], b = n[5]; double vh = (np - a) / b; return vh * s * c / lt;
        }

        // ---- 07
        if (H("potência cinética dada ao ar")) return 2 * n[1] / (n[1] + n[2]) * 100;
        if (H("eficiência propulsiva") && H("η = ")) return 2 / (1 + n[0] / n[1]) * 100;
        if (H("Qual o empuxo, em N")) return n[0] * (n[2] - n[1]);
        if (H("potência cinética, em kW")) return n[0] * (n[2] * n[2] - n[1] * n[1]) / 2 / 1000;
        if (H("Qual a potência útil")) return n[0] * n[1] / 1000;
        if (H("quantos kg/s de ar")) return n[0] / (n[2] - n[1]);
        if (H("Qual a velocidade de saída do motor B"))
        {
            double v0 = n[0], ma = n[1], ve = n[2], k = n[3]; return v0 + ma * (ve - v0) / (k * ma);
        }
        if (H("A que velocidade, em m/s, o ar sai")) return n[1] + n[2] / n[0];
        if (H("pontos percentuais"))
        {
            double v0 = n[0], va = n[1], k = n[2], vb = v0 + (va - v0) / k;
            return (2 * v0 / (v0 + vb) - 2 * v0 / (v0 + va)) * 100;
        }

        // ---- 08
        if (H("Um helicóptero de") && H("rotor de raio") && !System.Text.RegularExpressions.Regex.IsMatch(p, @"figure of merit [0-9]"))
        {
            double m = n[0], r = n[1], rho = n[2], g = n[3], t = m * g, a = Math.PI * r * r;
            double vi = Math.Sqrt(t / (2 * rho * a)), pid = t * vi;
            if (H("figure of merit?")) return pid / (n[4] * 1000);
            if (H("carga de disco")) return t / a;
            if (H("potência real no eixo")) return pid / n[4] / 1000;
            if (H("massa multiplicada por")) return pid * Math.Pow(n[4], 1.5) / 1000;
            if (H("raio do rotor multiplicado por")) return pid / n[4] / 1000;
            if (H("Qual a potência ideal para pairar")) return pid / 1000;
            if (H("consegue pairar com")) return m * Math.Pow(n[5] * 1000 * n[4] / pid, 2.0 / 3);
            if (H("velocidade induzida")) return vi;
            if (H("área do disco")) return a;
        }

        // ---- 09
        if (H("O rotor principal de um helicóptero gira"))
        {
            double w = Rpm(n[0]), q = n[1] * 1000 / w;
            if (H("margem sobra")) return (n[3] - q / n[2]) / n[3] * 100;
            if (H("empuxo necessário na cauda")) return q / n[2];
            if (H("Qual o torque no eixo")) return q;
            if (H("novo torque")) return n[1] * 1000 / Rpm(n[2]);
            if (H("máxima potência")) return n[3] * n[2] * w / 1000;
            if (H("A que distância do eixo")) return q / n[2];
            if (H("velocidade angular")) return w;
        }
        if (H("Em autorrotação, um helicóptero de", "velocidade de descida"))
            return Math.Sqrt(n[2] * n[0] * n[5] / (n[4] * Math.PI * n[1] * n[1] * n[3]));
        if (H("Quantos segundos leva para descer"))
            return n[6] / Math.Sqrt(n[2] * n[0] * n[5] / (n[4] * Math.PI * n[1] * n[1] * n[3]));

        // ---- 10
        if (H("fração do peso inicial")) return (n[0] - n[1]) / n[0] * 100;
        if (H("razão W₀/W₁") && H("queima")) return n[0] / (n[0] - n[1]);
        if (H("hélice (η_hélice"))
        {
            double eta = n[0], c = n[1] / 1000 / 3.6e6, ld = n[2];
            double R(double w0, double w1, double l = double.NaN) => eta / (c * 9.8) * (double.IsNaN(l) ? ld : l) * Math.Log(w0 / w1) / 1000;
            if (H("Quantos kg de combustível ele precisa"))
            {
                double g = n[5], x = n[3] * 1000 * c * g / (eta * ld); return n[4] * (1 - Math.Exp(-x));
            }
            double g2 = n[^1]; double Rg(double w0, double w1, double l) => eta / (c * g2) * l * Math.Log(w0 / w1) / 1000;
            if (H("quantos km de alcance perde")) return Rg(n[3], n[4], ld) - Rg(n[3] + n[5], n[4] + n[5], ld);
            if (H("quantos km de alcance ganha")) return Rg(n[3] + n[5], n[4], ld) - Rg(n[3], n[4], ld);
            if (H("L/D melhora")) return Rg(n[3], n[4], ld * (1 + n[5] / 100));
            if (H("alcance de Breguet")) return Rg(n[3], n[4], ld);
        }

        // ---- 11
        if (H("Qual o melhor L/D")) return LdMax(n[0], n[1], n[2]);
        if (H("Um planador desce a")) return n[1] / n[0];
        if (H("Um planador planou")) return n[0] * 1000 / n[1];
        if (H("Que distância horizontal, em km, ele percorre")) return n[0] * n[1] / 1000;
        if (H("Que distância sobre o solo"))
        {
            double ld = n[0], v = n[1], w = n[2], h = n[3];
            return h * ld * ((H("cauda") ? v + w : v - w) / v) / 1000;
        }
        if (H("De que altura, em m, ele precisa começar")) return n[1] * 1000 / n[0];
        if (H("De que altura, em m, precisa partir"))
        {
            double ld = n[0], v = n[1], w = n[2], d = n[3];
            return d * 1000 / ld * v / (H("cauda") ? v + w : v - w);
        }
        if (H("razão de descida no melhor planeio") || H("velocidade de melhor planeio"))
        {
            double cd0 = n[0], ar = n[1], e = n[2], m = n[3], s = n[4], rho = n[5], g = n[6];
            double cl = Math.Sqrt(cd0 * Math.PI * e * ar), v = Math.Sqrt(2 * m * g / (rho * s * cl));
            return H("razão de descida") ? v / LdMax(cd0, ar, e) : v;
        }
        if (H("Um planador voa a", "razão de descida, em m/s")) return n[0] / n[1];
        if (H("para descer", "minutos")) return n[2] / (n[0] / n[1]) / 60;

        // ---- 12
        if (H("Qual o fator de carga n?")) return 1 / Math.Cos(Rad(n[0]));
        if (H("sustentação necessária")) return n[0] / Math.Cos(Rad(n[1]));
        if (H("peso aparente")) return n[0] * n[1];
        if (H("Qual o raio da curva")) return n[1] * n[1] / (n[2] * Math.Tan(Rad(n[0])));
        if (H("curva mais fechada possível")) return n[0] * n[0] * n[1] / (n[2] * Math.Sqrt(n[1] * n[1] - 1));
        if (H("velocidade de estol numa curva nivelada")) return n[0] / Math.Sqrt(Math.Cos(Rad(n[1])));
        if (H("velocidade de manobra")) return n[0] * Math.Sqrt(n[1]);
        if (H("volta completa")) return 2 * Math.PI * n[0] / (n[3] * Math.Tan(Rad(n[2]))) * n[1] / 360;
        if (H("Que inclinação, em graus")) return Deg(Math.Acos(1 / n[0]));
        if (H("taxa de curva")) return Deg(n[2] * Math.Tan(Rad(n[0])) / n[1]);

        // ---- 13
        if (H("pés por minuto")) return (n[1] - n[2]) * 1000 / (n[0] * n[6]) * n[5] / n[4];
        if (H("Qual o teto absoluto")) return n[1] * n[0] / (n[0] - n[2]);
        if (H("O teto de serviço é onde"))
        {
            double rs0 = n[0], rsa = n[1], ha = n[2], rss = n[3]; return (rs0 - rss) / (rs0 - rsa) * ha;
        }
        if (H("Qual a razão de subida a")) return n[0] + (n[1] - n[0]) * n[3] / n[2];
        if (H("potência disponível (já com a hélice)")) return (n[2] - n[3]) * 1000 / (n[0] * n[4]);
        if (H("Quantos minutos leva para subir do solo")) return n[2] / n[0] * Math.Log(1 / (1 - n[3] / n[2])) / 60;
        if (H("razão de subida constante")) return n[1] / n[0] / 60;
        if (H("Qual o ângulo de subida, em graus")) return Deg(Math.Asin(n[0] / n[1]));

        // ---- 14
        if (H("Um avião de", "asa de", "AR = ", "e = "))
        {
            double m = n[0], s = n[1], cd0 = n[2], ar = n[3], e = n[4], rho = n[5], g = n[6], k = K(ar, e), w = m * g;
            double clmd = Math.Sqrt(cd0 / k), vmd = Math.Sqrt(2 * w / (rho * s * clmd)), ld = LdMax(cd0, ar, e);
            if (H("é maior que o arrasto mínimo")) return (1 / n[7] - 1) * 100;
            if (H("Qual o (L/D) máximo")) return ld;
            if (H("Qual o CL de mínimo arrasto")) return clmd;
            if (H("Qual o fator k")) return k;
            if (H("Qual o arrasto mínimo")) return w / ld;
            if (H("potência mínima necessária")) { double f = n[7]; return w / (f * ld) * vmd / Math.Pow(3, 0.25) / 1000; }
            if (H("Qual a velocidade de mínima potência")) return vmd / Math.Pow(n[7], n[8] / n[9]);
            if (H("Qual a velocidade de mínimo arrasto")) return vmd;
        }

        // ---- 15
        if (H("Qual a razão W₀/W₁")) return n[3] / n[4];
        if (H("Qual a massa de combustível"))
        {
            double c = n[0], ld = n[1], v = n[2], r = n[3], w0 = n[4], g = n[5]; return w0 * (1 - Math.Exp(-r * 1000 * c * g / (v * ld)));
        }
        if (H("Se o L/D passar de")) return n[2] / (n[0] * n[7]) * n[6] * Math.Log(n[3] / n[4]) / 1000;
        if (H("Use o peso médio"))
        {
            double h = n[0], rho = n[1], v = n[2], w0 = n[3], w1 = n[4], s = n[5], cd0 = n[6], ar = n[7], e = n[8], c = n[9], g = n[10];
            double cl = (w0 + w1) / 2 * g / (0.5 * rho * v * v * s), ld = cl / (cd0 + K(ar, e) * cl * cl);
            return v / (c * g) * ld * Math.Log(w0 / w1) / 1000;
        }
        if (H("cruza em Mach"))
        {
            double mach = n[0], w0 = n[1], w1 = n[2], c = n[3], ld = n[4], h1 = n[5], a1 = n[6], h2 = n[7], a2 = n[8], hx = n[9], hy = n[10], g = n[11];
            double A(double h) => Math.Abs(h - h1) < 1e-9 ? a1 : a2;
            return mach * (A(hx) - A(hy)) / (c * g) * ld * Math.Log(w0 / w1) / 1000;
        }
        if (H("temperatura padrão")) return Math.Sqrt(n[2] * n[3] * n[1]);
        if (H("em Mach")) return n[1] * n[2];
        if (H("Cruzando de", "qual o alcance"))
            return n[2] / (n[0] * n[5]) * n[1] * Math.Log(n[3] / n[4]) / 1000;

        // ---- 16
        if (p.StartsWith("Avião vazio"))
        {
            double[] ms, xs;
            if (H("bagagem", "A CMA"))
            {
                ms = new[] { n[0], n[2], n[4], n[6] }; xs = new[] { n[1], n[3], n[5], n[7] };
                double mt = ms.Sum(), cg = ms.Zip(xs, (a, b) => a * b).Sum() / mt;
                if (H("Qual a massa total")) return mt;
                if (H("Onde fica o CG")) return cg;
                double c = n[8], x0 = n[9];
                if (H("Em que % da CMA está o CG")) return (cg - x0) / c * 100;
                if (H("margem estática")) return n[10] - (cg - x0) / c * 100;
            }
            if (H("Qual a massa total"))
                return n[0] + n[2] + n[4] + n[6];
            if (H("Onde fica o CG")) return (n[0] * n[1] + n[2] * n[3] + n[4] * n[5] + n[6] * n[7]) / (n[0] + n[2] + n[4] + n[6]);
            if (H("maior bagagem"))
            {
                double sm = n[0] + n[2] + n[4], smx = n[0] * n[1] + n[2] * n[3] + n[4] * n[5], lim = n[6], xb = n[7];
                return (lim * sm - smx) / (xb - lim);
            }
        }
        if (H("se desloca para trás")) return (n[0] * n[1] + n[2] * n[3]) / (n[0] + n[2]) - n[1];
        if (H("Um piloto de")) return (n[0] * n[1] + n[2] * n[3]) / (n[0] + n[2]);

        // ---- 17
        if (H("De quantos por cento a potência real para pairar aumenta")) return (Math.Sqrt(n[4] / n[6]) - 1) * 100;
        if (H("solidez")) return n[1] * n[2] / (Math.PI * n[0]);
        if (H("Qual a potência ideal para pairar a") || H("Qual a potência real para pairar a"))
        {
            double m = n[0], r = n[1], fm = n[2], g = n[3], rho = n[5], t = m * g;
            double pid = Math.Pow(t, 1.5) / Math.Sqrt(2 * rho * Math.PI * r * r);
            return H("potência real") ? pid / fm / 1000 : pid / 1000;
        }
        if (H("Qual a massa máxima, em kg"))
        {
            double r = n[0], fm = n[1], pw = n[2], rho = n[4], g = n[5];
            double t = Math.Pow(pw * 1000 * fm * Math.Sqrt(2 * rho * Math.PI * r * r), 2.0 / 3); return t / g;
        }
        if (H("teto de pairar"))
        {
            double pr0 = n[4], pe = n[5], sigma = Math.Pow(pr0 / pe, 2.0 / 3);
            return Altitude(sigma * n[6]);
        }

        // ---- 18
        if (H("Um rotor de raio") && H("gira a"))
        {
            double r = n[0], w = Rpm(n[1]), tip = w * r;
            if (H("fluxo reverso, como")) return n[2] / tip * 100;
            if (H("Mach da ponta")) return (tip + n[2]) / n[3];
            if (H("parâmetro de avanço")) return n[2] / tip;
            if (H("Até que raio")) return n[2] / w;
            if (H("velocidade máxima do helicóptero")) return n[2] * n[3] - tip;
            if (H("Ω·R), em m/s")) return tip;
            if (H("pá que avança, em m/s")) return tip + n[2];
            if (H("pá que recua")) return tip - n[2];
        }

        // ---- 19
        if (H("autorrotação") || H("flare") || H("Um rotor tem"))
        {
            if (H("Um rotor tem I", "desacelera até")) { double w = Rpm(n[1]), f = n[2] / 100; return 0.5 * n[0] * w * w * (1 - f * f) / 1000; }
            if (H("Um rotor tem momento de inércia")) { double w = Rpm(n[1]); return 0.5 * n[0] * w * w / 1000; }
            if (H("Na flare, um rotor de I"))
            {
                double w = Rpm(n[1]), f = n[3], e = 0.5 * n[0] * w * w * (1 - f * f); return e / (n[4] * n[5]);
            }
            double m = n[0];
            double V(int ir, int ik1, int ik2, int irho, int ig) => Math.Sqrt(n[ik1] * m * n[ig] / (n[irho] * Math.PI * n[ir] * n[ir] * n[ik2]));
            if (H("Que razão E"))
            {
                double v = V(1, 2, 3, 4, 5), w = Rpm(n[7]), f = n[9], e = 0.5 * n[6] * w * w * (1 - f * f); return e / (0.5 * m * v * v);
            }
            if (H("energia cinética da descida (")) { double v = V(1, 2, 3, 4, 5); return 0.5 * m * v * v / 1000; }
            if (H("momento de inércia I"))
            {
                double v = V(1, 2, 3, 4, 5), w = Rpm(n[6]), f = n[8]; return m * v * v / (w * w * (1 - f * f));
            }
            if (H("Quantos segundos leva para descer")) return n[6] / V(1, 2, 3, 4, 5);
            if (H("velocidade de descida")) return V(1, 2, 3, 4, 5);
        }

        // ---- 20
        if (H("avião de mato") || H("carga alar, em kg/m²"))
        {
            if (H("Qual a carga alar")) return n[0] / n[1];
            double m = n[0], s = n[1], clm = n[2], rho = n[3], g = n[4];
            double vs = Vs(m, s, rho, clm, g);
            if (H("Que empuxo médio mínimo")) { double vlo = n[7] * vs; return m * vlo * vlo / (2 * n[5]) + n[6] * m * g; }
            if (H("velocidade de decolagem (")) return n[5] * vs * 3.6;
            if (H("nova corrida"))
            {
                double t2 = n[5] * n[6] / rho, vlo = n[8] * Vs(m, s, n[6], clm, g), a = (t2 - n[7] * m * g) / m; return vlo * vlo / (2 * a);
            }
            if (H("distância da corrida de decolagem")) { double vlo = n[7] * vs, a = (n[5] - n[6] * m * g) / m; return vlo * vlo / (2 * a); }
            if (H("vento de proa")) { double vlo = n[8] * vs - n[7], a = (n[5] - n[6] * m * g) / m; return vlo * vlo / (2 * a); }
            if (H("Qual a velocidade de estol")) return vs;
            if (H("Qual a aceleração média")) return (n[5] - n[6] * m * g) / m;
        }
        return null;
    }

    /// <summary>ISA troposphere altitude (m) for a density (kg/m³).</summary>
    static double Altitude(double rho)
    {
        double lo = 0, hi = 11000;
        for (int i = 0; i < 100; i++)
        {
            double h = (lo + hi) / 2, d = 1.225 * Math.Pow(1 - 0.0065 * h / 288.15, 4.2559);
            if (d > rho) lo = h; else hi = h;
        }
        return lo;
    }
}
