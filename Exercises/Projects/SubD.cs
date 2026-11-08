namespace EasyCreator.Exercises;

/// <summary>Submarines 16-20: PID saturation, deep dives, hydrodynamics, fatigue, the final AUV project.</summary>
[Exercises("sub-16-pid-saturacao")]
public sealed class Sub16 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double kp = Ex.Step(r, 10, 100, 5), umax = Ex.Step(r, 10, 80, 5), e = Ex.Step(r, 0.2, 5, 0.1), ki = Ex.Step(r, 1, 20, 1);
        switch (d)
        {
            case Difficulty.Easy:
            {
                double u = Math.Min(kp * e, umax);
                return Ex.Num(L.F("O controlador de profundidade de um ROV tem Kp = {0} N/m, mas os propulsores só entregam no máximo {1} N. Com erro de {2} m, qual a força realmente aplicada?", Ex.N(kp), Ex.N(umax), Ex.N(e)), u, "N", L.F("Pedido: {0}·{1} = {2} N; limite = {3} N; aplicado = {4} N.", Ex.N(kp), Ex.N(e), Ex.N(kp * e), Ex.N(umax), Ex.N(u)));
            }
            case Difficulty.Medium:
                return Ex.Num(L.F("Um controlador P com Kp = {0} N/m comanda propulsores limitados a {1} N. A partir de que erro de profundidade (em m) o comando satura?", Ex.N(kp), Ex.N(umax)), umax / kp, "m", L.F("Kp·erro = limite → erro = {0} / {1} = {2} m.", Ex.N(umax), Ex.N(kp), Ex.N(umax / kp, 3)));
            case Difficulty.Hard:
            {
                double t = Ex.Step(r, 2, 30, 1), I = ki * e * t;
                return Ex.Num(L.F("O ROV está preso (saturado) com erro constante de {0} m por {1} s, e o termo integral continua somando (sem anti-windup), com Ki = {2} N/(m·s). Quanto vale o termo integral ao final?", Ex.N(e), Ex.N(t), Ex.N(ki)), I, "N", L.F("I = Ki·erro·t = {0}·{1}·{2} = {3} N, muito além do limite: ao chegar à meta o ROV passa direto (windup).", Ex.N(ki), Ex.N(e), Ex.N(t), Ex.N(I)));
            }
            default:
            {
                double W = Ex.Step(r, 10, 60, 2), t = W / (ki * e);
                return Ex.Num(L.F("Um ROV pesado precisa de {0} N de empuxo constante para manter a profundidade. Se o erro ficar em {1} m, com Ki = {2} N/(m·s), quantos segundos o termo integral leva para sozinho chegar a esse valor?", Ex.N(W), Ex.N(e), Ex.N(ki)),
                    t, "s", L.F("I = Ki·erro·t = {0} → t = {0} / ({1}·{2}) = {3} s. Quando I sustenta o peso, o erro vai a zero: é por isso que a integral elimina o erro permanente.", Ex.N(W), Ex.N(ki), Ex.N(e), Ex.N(t, 3)));
            }
        }
    }
}

[Exercises("sub-17-mergulho-profundo")]
public sealed class Sub17 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double D = Ex.Step(r, 100, 4000, 100), vd = Ex.Step(r, 0.3, 2, 0.1), sf = Ex.Pick(r, 1.25, 1.5, 2.0);
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um submersível opera até {0} m e foi projetado com fator de segurança {1} em profundidade. A que profundidade o casco colapsaria?", Ex.N(D), Ex.N(sf)), D * sf, "m", L.F("Colapso = FS · profundidade de operação = {0}·{1} = {2} m.", Ex.N(sf), Ex.N(D), Ex.N(D * sf)));
                return Ex.Num(L.F("Um submersível de colapso a {0} m de profundidade deve ter fator de segurança {1}. Qual sua profundidade máxima de operação?", Ex.N(D * sf), Ex.N(sf)), D, "m", L.F("Operação = colapso / FS = {0} / {1} = {2} m.", Ex.N(D * sf), Ex.N(sf), Ex.N(D)));
            case Difficulty.Medium:
            {
                double ta = D / vd / 60;
                return Ex.Num(L.F("Um submersível desce {0} m a {1} m/s constantes. Quantos minutos dura a descida?", Ex.N(D), Ex.N(vd)), ta, "min", L.F("t = D / v = {0} / {1} = {2} s = {3} min.", Ex.N(D), Ex.N(vd), Ex.N(D / vd), Ex.N(ta)));
            }
            case Difficulty.Hard:
            {
                double va = Ex.Step(r, 0.3, 1.5, 0.1), tb = Ex.Step(r, 10, 240, 10), tot = D / vd / 60 + tb + D / va / 60;
                return Ex.Num(L.F("Um mergulho vai a {0} m, descendo a {1} m/s, fica {2} min no fundo e sobe a {3} m/s. Qual a duração total, em minutos?", Ex.N(D), Ex.N(vd), Ex.N(tb), Ex.N(va)), tot, "min", L.F("Descida = {0} min; fundo = {1} min; subida = {2} min; total = {3} min.", Ex.N(D / vd / 60, 3), Ex.N(tb), Ex.N(D / va / 60, 3), Ex.N(tot, 3)));
            }
            default:
            {
                double dia = Ex.Step(r, 40, 100, 5), cd = Ex.Step(r, 0.6, 1.2, 0.1), A = Pk.Area(dia / 100), dm = Ex.Step(r, 2, 20, 1), v = Math.Sqrt(dm * G / (0.5 * Pk.RhoW * cd * A)), t = D / v / 60, tb = Ex.Step(r, 20, 120, 10), tot = D / vd / 60 + tb + t;
                return Ex.Num(L.F("Um AUV desce a {0} m/s até {1} m, trabalha {2} min no fundo e emerge soltando {3} kg de lastro (sobe na velocidade terminal: peso líquido = ½·ρ·Cd·A·v², água 1000 kg/m³, Cd = {4}, {5} cm de diâmetro horizontal, g = 9,8 m/s²). Qual a duração total do mergulho, em minutos?", Ex.N(vd), Ex.N(D), Ex.N(tb), Ex.N(dm), Ex.N(cd), Ex.N(dia)),
                    tot, "min", L.F("v_subida = √(Δm·g / (½·ρ·Cd·A)) = {0} m/s; subida = {1} min; descida = {2} min; total = {2} + {3} + {1} = {4} min.", Ex.N(v, 3), Ex.N(t, 3), Ex.N(D / vd / 60, 3), Ex.N(tb), Ex.N(tot, 3)));
            }
        }
    }
}

[Exercises("sub-18-hidrodinamica")]
public sealed class Sub18 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double len = Ex.Step(r, 0.3, 5, 0.1), v = Ex.Step(r, 0.3, 4, 0.1);
        switch (d)
        {
            case Difficulty.Easy:
            {
                double re = Pk.RhoW * v * len / 1e-3;
                return Ex.Num(L.F("Um veículo de {0} m de comprimento se move a {1} m/s na água (ρ = 1000 kg/m³, viscosidade 10⁻³ Pa·s). Qual o número de Reynolds Re = ρ·v·L/μ?", Ex.N(len), Ex.N(v)), re, "", L.F("Re = 1000·{0}·{1} / 0,001 = {2}.", Ex.N(v), Ex.N(len), Ex.N(re, 0)));
            }
            case Difficulty.Medium:
            {
                double cd = Ex.Step(r, 0.2, 1, 0.05), dia = Ex.Step(r, 15, 60, 5), P = 0.5 * Pk.RhoW * cd * Pk.Area(dia / 100) * v * v * v;
                return Ex.Num(L.F("Um veículo de {0} cm de diâmetro frontal, Cd = {1}, a {2} m/s na água (1000 kg/m³). Qual a potência gasta contra o arrasto, P = ½·ρ·Cd·A·v³?", Ex.N(dia), Ex.N(cd), Ex.N(v)), P, "W", L.F("A = {0} m²; P = 0,5·1000·{1}·{0}·{2}³ = {3} W.", Ex.N(Pk.Area(dia / 100), 5), Ex.N(cd), Ex.N(v), Ex.N(P, 3)));
            }
            case Difficulty.Hard:
            {
                double depth = Ex.Step(r, 1, 100, 1), pv = 2300, pabs = Pk.P0 + Pk.RhoSea * G * depth, vc = Math.Sqrt(2 * (pabs - pv) / Pk.RhoSea);
                return Ex.Num(L.F("A água ao redor da ponta de uma hélice a {0} m de profundidade no mar (1025 kg/m³) vaporiza (cavita) quando a pressão local cai à pressão de vapor, 2300 Pa. Pela equação de Bernoulli, p_local = p_abs − ½·ρ·v². Qual a velocidade da ponta que começa a cavitar? (atmosfera 101325 Pa; g = 9,8 m/s²)", Ex.N(depth)),
                    vc, "m/s", L.F("p_abs = {0} Pa; v = √(2·(p_abs − 2300)/ρ) = √(2·{1}/1025) = {2} m/s.", Ex.N(pabs, 0), Ex.N(pabs - pv, 0), Ex.N(vc, 3)));
            }
            default:
            {
                double lam = Ex.Pick(r, 2.0, 4.0, 5.0, 8.0, 10.0), vp = Ex.Step(r, 0.5, 3, 0.1), lp = Ex.Step(r, 2, 8, 0.5), vm = vp * lam;
                return Ex.Num(L.F("Um protótipo de {0} m de comprimento navegará a {1} m/s. Um modelo em escala 1:{2}, testado na mesma água, deve ter o mesmo número de Reynolds. A que velocidade o modelo deve ser ensaiado, em m/s? (Re = ρ·v·L/μ)", Ex.N(lp), Ex.N(vp), Ex.N(lam)),
                    vm, "m/s", L.F("Mesmo Re com o mesmo fluido: v_m·L_m = v_p·L_p → v_m = v_p·(L_p/L_m) = {0}·{1} = {2} m/s (rápido demais para um tanque comum: por isso se usa água pressurizada ou túnel de vento).", Ex.N(vp), Ex.N(lam), Ex.N(vm)));
            }
        }
    }
}

[Exercises("sub-19-fadiga-seguranca")]
public sealed class Sub19 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double N = Ex.Step(r, 1000, 20000, 500), n = Math.Round(N * Ex.Step(r, 0.1, 0.8, 0.05) / 10) * 10, perDive = Ex.Pick(r, 1.0, 2.0, 4.0);
        switch (d)
        {
            case Difficulty.Easy:
                return Ex.Num(L.F("Um casco aguenta {0} ciclos de pressurização até a falha por fadiga e já fez {1}. Quantos ciclos ainda restam, em teoria?", Ex.N(N, 0), Ex.N(n, 0)), N - n, "", L.F("Restam {0} − {1} = {2} ciclos.", Ex.N(N, 0), Ex.N(n, 0), Ex.N(N - n, 0)));
            case Difficulty.Medium:
            {
                double dives = (N - n) / perDive;
                return Ex.Num(L.F("Um casco aguenta {0} ciclos e já usou {1}. Cada mergulho aplica {2} ciclo(s) de pressão. Quantos mergulhos ainda podem ser feitos?", Ex.N(N, 0), Ex.N(n, 0), Ex.N(perDive)), dives, "", L.F("({0} − {1}) / {2} = {3} mergulhos.", Ex.N(N, 0), Ex.N(n, 0), Ex.N(perDive), Ex.N(dives, 1)));
            }
            case Difficulty.Hard:
            {
                double N1 = Ex.Step(r, 5000, 50000, 1000), n1 = Math.Round(N1 * Ex.Step(r, 0.1, 0.5, 0.05)), N2 = Ex.Step(r, 500, 5000, 100), n2 = Math.Round(N2 * Ex.Step(r, 0.05, 0.4, 0.05)), D = n1 / N1 + n2 / N2;
                return Ex.Num(L.F("Pela regra de Miner, o dano é D = Σ nᵢ/Nᵢ (falha em D = 1). Um casco sofreu {0} ciclos de um nível de tensão que o romperia em {1}, e {2} ciclos de outro nível que o romperia em {3}. Qual o dano acumulado D?", Ex.N(n1, 0), Ex.N(N1, 0), Ex.N(n2, 0), Ex.N(N2, 0)), D, "", L.F("D = {0}/{1} + {2}/{3} = {4}. Ainda restam {5}% da vida.", Ex.N(n1, 0), Ex.N(N1, 0), Ex.N(n2, 0), Ex.N(N2, 0), Ex.N(D, 4), Ex.N((1 - D) * 100, 1)), 2);
            }
            default:
            {
                double m = Ex.Pick(r, 3.0, 4.0, 5.0), s1 = Ex.Step(r, 200, 400, 10), f = Ex.Step(r, 0.7, 0.95, 0.05), s2 = s1 * f, N1 = Ex.Step(r, 2000, 20000, 1000), N2 = N1 * Math.Pow(s1 / s2, m);
                return Ex.Num(L.F("Para um aço, a vida à fadiga segue N = C·S^(−{0}) (Basquin). Com tensão de {1} MPa o casco dura {2} ciclos. Se a profundidade de operação for reduzida e a tensão cair para {3} MPa, quantos ciclos ele passa a durar?", Ex.N(m), Ex.N(s1), Ex.N(N1, 0), Ex.N(s2, 1)),
                    N2, "", L.F("N₂ = N₁·(S₁/S₂)^{0} = {1}·({2}/{3})^{0} = {4} ciclos: uma pequena redução de tensão multiplica muito a vida.", Ex.N(m), Ex.N(N1, 0), Ex.N(s1), Ex.N(s2, 1), Ex.N(N2, 0)), 3);
            }
        }
    }
}

[Exercises("sub-20-projeto-auv")]
public sealed class Sub20 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double w = Ex.Step(r, 200, 2000, 50), h = Ex.Step(r, 100, 1000, 50), sw = Ex.Step(r, 20, 100, 5), v = Ex.Step(r, 0.8, 2.5, 0.1);
        int lanes = (int)Math.Ceiling(h / sw);
        double len = lanes * w + (lanes - 1) * sw;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double area = w * h;
                return Ex.Num(L.F("Um AUV varre uma faixa de {0} m de largura de cada vez. Para cobrir uma área de {1} m² em linhas retas lado a lado, qual o comprimento total da trilha, ignorando as curvas?", Ex.N(sw), Ex.N(area, 0)), area / sw, "m", L.F("Comprimento = área / largura da faixa = {0} / {1} = {2} m.", Ex.N(area, 0), Ex.N(sw), Ex.N(area / sw)));
            }
            case Difficulty.Medium:
                return Ex.Num(L.F("Um AUV com faixa de {0} m varre um retângulo de {1} m × {2} m com linhas paralelas ao lado de {1} m, a {3} m/s. Quantas linhas (inteiro, arredondado para cima) são necessárias? Responda o número de linhas.", Ex.N(sw), Ex.N(w), Ex.N(h), Ex.N(v)), lanes, "", L.F("Linhas = ⌈{0} / {1}⌉ = {2}.", Ex.N(h), Ex.N(sw), Ex.N(lanes)), 0.001);
            case Difficulty.Hard:
            {
                double p = Ex.Step(r, 30, 200, 10), t = len / v / 3600, e = p * t;
                return Ex.Num(L.F("Um AUV varre um retângulo de {0} m × {1} m com faixa de {2} m em linhas paralelas ao lado de {0} m, ligadas por trechos de {2} m. Velocidade {3} m/s e consumo {4} W. Quantos Wh gasta na varredura? (trilha = linhas·{0} + ligações)", Ex.N(w), Ex.N(h), Ex.N(sw), Ex.N(v), Ex.N(p)), e, "Wh", L.F("Linhas = {0}; trilha = {0}·{1} + {2}·{3} = {4} m; t = {4}/{5} = {6} h; E = {7}·{6} = {8} Wh.", Ex.N(lanes), Ex.N(w), Ex.N(lanes - 1), Ex.N(sw), Ex.N(len), Ex.N(v), Ex.N(t, 4), Ex.N(p), Ex.N(e, 3)));
            }
            default:
            {
                double p = Ex.Step(r, 30, 200, 10), wh = Ex.Step(r, 100, 1500, 50), costLane = p * (w + sw) / v / 3600, nmax = Math.Floor(wh / costLane);
                return Ex.Num(L.F("Cada linha de varredura de um AUV tem {0} m mais {1} m de ligação até a seguinte, a {2} m/s, com consumo de {3} W. Com uma bateria útil de {4} Wh, quantas linhas completas ele consegue fazer?", Ex.N(w), Ex.N(sw), Ex.N(v), Ex.N(p), Ex.N(wh)),
                    nmax, "", L.F("Energia por linha = {0}·({1} + {2}) / {3} / 3600 = {4} Wh; linhas = ⌊{5} / {4}⌋ = {6}.", Ex.N(p), Ex.N(w), Ex.N(sw), Ex.N(v), Ex.N(costLane, 4), Ex.N(wh), Ex.N(nmax, 0)), 0.001);
            }
        }
    }
}
