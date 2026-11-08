using static EasyCreator.Exercises.Pv;

namespace EasyCreator.Exercises;

/// <summary>Aircraft 11-15: gliding, turns and load factor, climb and ceiling, the power curve, jet range.</summary>
[Exercises("aero-11-planador")]
public sealed class Air11 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double ld = Ex.Step(r, 10, 45, 1), h = Ex.Step(r, 300, 3000, 100), v = Ex.Step(r, 20, 40, 1);
        switch (d)
        {
            case Difficulty.Easy:
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um planador com L/D = {0} começa a planar de {1} m de altura, em ar parado. Que distância horizontal, em km, ele percorre?", N(ld), N(h)), h * ld / 1000, "km",
                        L.F("distância = altura × (L/D) = {0} × {1} = {2} km.", N(h), N(ld), N(h * ld / 1000)));
                    case 1: return Ex.Num(L.F("Um planador planou {0} km a partir de {1} m de altura, em ar parado. Qual o seu L/D?", N(h * ld / 1000), N(h)), ld, "",
                        L.F("L/D = distância / altura = {0} / {1} = {2}.", N(h * ld), N(h), N(ld)));
                    default: return Ex.Num(L.F("Um planador com L/D = {0} precisa planar {1} km em ar parado. De que altura, em m, ele precisa começar?", N(ld), N(h * ld / 1000)), h, "m",
                        L.F("altura = distância / (L/D) = {0} / {1} = {2} m.", N(h * ld), N(ld), N(h)));
                }
            case Difficulty.Medium:
            {
                double sink = v / ld;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um planador voa a {0} m/s com L/D = {1}. Qual a razão de descida, em m/s?", N(v), N(ld)), sink, "m/s",
                        L.F("razão de descida = V / (L/D) = {0} / {1} = {2} m/s.", N(v), N(ld), N(sink, 3)), 3);
                    case 1: return Ex.Num(L.F("Um planador voa a {0} m/s com L/D = {1}, descendo a razão constante. Quanto tempo leva, em minutos, para descer {2} m?", N(v), N(ld), N(h)), h / sink / 60, "min",
                        L.F("Razão de descida = {0}/{1} = {2} m/s; t = {3}/{2} = {4} s = {5} min.", N(v), N(ld), N(sink, 3), N(h), N(h / sink), N(h / sink / 60)), 3);
                    default:
                    {
                        double sk = Ex.Step(r, 0.5, 1.5, 0.05), dist = h * ld;
                        return Ex.Num(L.F("Um planador desce a {0} m/s voando a {1} m/s. Qual o seu L/D?", N(sk), N(v)), v / sk, "",
                            L.F("L/D = V / razão de descida = {0} / {1} = {2}.", N(v), N(sk), N(v / sk)));
                    }
                }
            }
            case Difficulty.Hard:
            {
                double cd0 = Ex.Step(r, 0.012, 0.022, 0.001), e = Ex.Step(r, 0.8, 0.95, 0.01), ar = Ex.Step(r, 15, 35, 1), ldm = 0.5 * Math.Sqrt(Math.PI * e * ar / cd0), clb = Math.Sqrt(cd0 * Math.PI * e * ar);
                double m = Ex.Step(r, 300, 700, 10), S = Ex.Step(r, 10, 18, 0.5), vb = Math.Sqrt(2 * m * 9.8 / (1.225 * S * clb));
                string gl = L.F("Um planador com CD0 = {0}, alongamento {1} e fator de Oswald {2}.", N(cd0, 3), N(ar), N(e));
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(gl + " " + L.T("Qual o melhor L/D?"), ldm, "",
                        L.F("(L/D)máx = ½·√(π·e·AR/CD0) = ½·√(π·{0}·{1}/{2}) = {3}.", N(e), N(ar), N(cd0, 3), N(ldm)), 3);
                    case 1: return Ex.Num(gl + " " + L.F("Ele pesa {0} kg e tem {1} m² de asa (ρ = 1,225 kg/m³, g = 9,8 m/s²). Qual a velocidade de melhor planeio, em m/s?", N(m), N(S)), vb, "m/s",
                        L.F("CL no melhor L/D = √(CD0·π·e·AR) = {0}; V = √(2·m·g / (ρ·S·CL)) = {1} m/s.", N(clb, 3), N(vb)), 3);
                    default: return Ex.Num(gl + " " + L.F("Ele pesa {0} kg e tem {1} m² de asa (ρ = 1,225 kg/m³, g = 9,8 m/s²). Qual a razão de descida no melhor planeio, em m/s?", N(m), N(S)), vb / ldm, "m/s",
                        L.F("(L/D)máx = {0}; V = {1} m/s; descida = V / (L/D) = {2} m/s.", N(ldm), N(vb), N(vb / ldm, 3)), 3);
                }
            }
            default:
            {
                double wind = Ex.Step(r, 3, 10, 1), vt = Ex.Step(r, 25, 35, 1);
                bool head = r.Next(2) == 0;
                double gs = head ? vt - wind : vt + wind, ldg = ld * gs / vt, dist = h * ldg;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um planador com L/D = {0} voa a {1} m/s (velocidade no ar) com {2} de {3} m/s, a partir de {4} m de altura. Que distância sobre o solo, em km, ele percorre?", N(ld), N(vt), head ? L.T("vento de proa") : L.T("vento de cauda"), N(wind), N(h)), dist / 1000, "km",
                        L.F("O ar tem a descida de {0} m/s e velocidade sobre o solo de {1} m/s: L/D efetivo = (L/D)·({1}/{2}) = {3}; distância = {4} × {3} = {5} km.", N(vt / ld, 3), N(gs), N(vt), N(ldg), N(h), N(dist / 1000)), 3);
                double need = Ex.Step(r, 10, 40, 2) * 1000, hneed = need / ldg;
                return Ex.Num(L.F("Um planador com L/D = {0} voa a {1} m/s (velocidade no ar) com {2} de {3} m/s. De que altura, em m, precisa partir para planar {4} km sobre o solo?", N(ld), N(vt), head ? L.T("vento de proa") : L.T("vento de cauda"), N(wind), N(need / 1000)), hneed, "m",
                    L.F("L/D efetivo = (L/D)·(V ∓ vento)/V = {0}·{1}/{2} = {3}; altura = {4} / {3} = {5} m.", N(ld), N(gs), N(vt), N(ldg), N(need), N(hneed)), 3);
            }
        }
    }
}

[Exercises("aero-12-curvas-fator-carga")]
public sealed class Air12 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double bank = Ex.Step(r, 15, 70, 5), n = 1 / Math.Cos(bank * Math.PI / 180), tan = Math.Tan(bank * Math.PI / 180), v = Ex.Step(r, 40, 120, 5), vs = Ex.Step(r, 25, 45, 1);
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um avião faz uma curva nivelada com {0}° de inclinação. Qual o fator de carga n?", N(bank)), n, "",
                        L.F("n = 1 / cos φ = 1 / cos({0}°) = {1}.", N(bank), N(n, 3)), 3);
                {
                    double W = Ex.Step(r, 8000, 30000, 1000);
                    return Ex.Num(L.F("Um avião de {0} N de peso faz uma curva nivelada com {1}° de inclinação. Qual a sustentação necessária, em N?", N(W), N(bank)), W * n, "N",
                        L.F("L = n·W = {0} · {1} = {2} N.", N(n, 3), N(W), N(W * n)));
                }
            case Difficulty.Medium:
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("A velocidade de estol de um avião em voo reto é {0} m/s. Qual a velocidade de estol numa curva nivelada de {1}° de inclinação, em m/s?", N(vs), N(bank)), vs * Math.Sqrt(n), "m/s",
                        L.F("n = {0}; V = V_estol·√n = {1}·√{0} = {2} m/s.", N(n, 3), N(vs), N(vs * Math.Sqrt(n))));
                    case 1: return Ex.Num(L.F("Um avião faz uma curva nivelada de {0}° de inclinação a {1} m/s. Qual o raio da curva, em m? (g = 9,8 m/s²)", N(bank), N(v)), v * v / (G * tan), "m",
                        L.F("r = v² / (g·tg φ) = {0}² / (9,8·{1}) = {2} m.", N(v), N(tan, 3), N(v * v / (G * tan))));
                    default: return Ex.Num(L.F("Um avião faz uma curva nivelada de {0}° de inclinação a {1} m/s. Qual a taxa de curva, em graus por segundo? (g = 9,8 m/s²)", N(bank), N(v)), G * tan / v * 180 / Math.PI, "°/s",
                        L.F("ω = g·tg φ / v = 9,8·{0} / {1} = {2} rad/s = {3}°/s.", N(tan, 3), N(v), N(G * tan / v, 4), N(G * tan / v * 180 / Math.PI)));
                }
            case Difficulty.Hard:
            {
                double nn = Ex.Step(r, 1.5, 4, 0.25), phi = Math.Acos(1 / nn) * 180 / Math.PI, t360 = 2 * Math.PI * v / (G * tan);
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Que inclinação, em graus, produz um fator de carga {0} numa curva nivelada?", N(nn)), phi, "°",
                        L.F("cos φ = 1/n = 1/{0} → φ = {1}°.", N(nn), N(phi)));
                    case 1: return Ex.Num(L.F("Quanto tempo, em s, um avião a {0} m/s leva para dar uma volta completa (360°) numa curva nivelada de {1}° de inclinação? (g = 9,8 m/s²)", N(v), N(bank)), t360, "s",
                        L.F("t = 2π·r / v = 2π·v / (g·tg φ) = 2π·{0} / (9,8·{1}) = {2} s.", N(v), N(tan, 3), N(t360)));
                    default:
                    {
                        double W = Ex.Step(r, 8000, 30000, 1000);
                        return Ex.Num(L.F("Um avião de {0} N faz uma curva nivelada com fator de carga {1}. Qual o peso aparente sentido pelos ocupantes, em N (o mesmo que a sustentação)?", N(W), N(nn)), W * nn, "N",
                            L.F("L = n·W = {0}·{1} = {2} N.", N(nn), N(W), N(W * nn)));
                    }
                }
            }
            default:
            {
                double nn = Ex.Step(r, 2, 4.5, 0.5), vt = vs * Math.Sqrt(nn), rr = vt * vt / (G * Math.Sqrt(nn * nn - 1));
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um avião tem velocidade de estol {0} m/s em voo reto e faz a curva mais fechada possível no limite do estol, com fator de carga {1}. Qual o raio, em m? (g = 9,8 m/s²)", N(vs), N(nn)), rr, "m",
                        L.F("V = V_estol·√n = {0} m/s; tg φ = √(n² − 1) = {1}; r = V² / (g·tg φ) = {2} m.", N(vt), N(Math.Sqrt(nn * nn - 1), 3), N(rr)), 3);
                double nmax = Ex.Pick(r, 3.8, 4.4, 6.0), vcorner = vs * Math.Sqrt(nmax);
                return Ex.Num(L.F("Um avião tem velocidade de estol {0} m/s em voo reto e limite estrutural de {1} g. Abaixo de que velocidade, em m/s (velocidade de manobra), ele estola antes de quebrar?", N(vs), N(nmax)), vcorner, "m/s",
                    L.F("A sustentação máxima aerodinâmica cresce com V²; o limite estrutural é n = {0}: V_manobra = V_estol·√n = {1}·√{0} = {2} m/s.", N(nmax), N(vs), N(vcorner)), 3);
            }
        }
    }
}

[Exercises("aero-13-teto")]
public sealed class Air13 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double m = Ex.Step(r, 700, 3000, 50), W = m * G, v = Ex.Step(r, 40, 80, 1), D = W / Ex.Step(r, 8, 14, 0.5), Preq = D * v;
        double rc0 = Ex.Step(r, 2, 8, 0.25), Pav = Math.Round((Preq + W * rc0) / 1000) * 1000, rc = (Pav - Preq) / W;
        switch (d)
        {
            case Difficulty.Easy:
                return Ex.Num(L.F("Um avião de {0} kg voa a {1} m/s com {2} kW de potência disponível (já com a hélice) e precisa de {3} kW para voar nível. Qual a razão de subida, em m/s? (g = 9,8 m/s²)", N(m), N(v), N(Pav / 1000), N(Preq / 1000)), rc, "m/s",
                    L.F("RS = (P_disp − P_nec) / W = ({0} − {1})·1000 / {2} = {3} m/s.", N(Pav / 1000), N(Preq / 1000), N(W), N(rc, 3)), 3);
            case Difficulty.Medium:
            {
                double hh = Ex.Step(r, 300, 2000, 100);
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um avião sobe a {0} m/s com velocidade de {1} m/s no ar. Qual o ângulo de subida, em graus?", N(rc), N(v)), Math.Asin(rc / v) * 180 / Math.PI, "°",
                        L.F("sen γ = RS / V = {0} / {1}; γ = {2}°.", N(rc, 3), N(v), N(Math.Asin(rc / v) * 180 / Math.PI)), 3);
                    case 1: return Ex.Num(L.F("Um avião sobe a {0} m/s (razão de subida constante). Quantos minutos leva para subir {1} m?", N(rc), N(hh)), hh / rc / 60, "min",
                        L.F("t = h / RS = {0} / {1} = {2} s = {3} min.", N(hh), N(rc, 3), N(hh / rc), N(hh / rc / 60)), 3);
                    default: return Ex.Num(L.F("Um avião de {0} kg tem {1} kW de potência disponível e {2} kW de potência necessária a {3} m/s. Qual a razão de subida, em pés por minuto? (1 m/s = 196,85 ft/min; g = 9,8 m/s²)", N(m), N(Pav / 1000), N(Preq / 1000), N(v)), rc * 196.85, "ft/min",
                        L.F("RS = ({0} − {1})·1000 / {2} = {3} m/s = {4} ft/min.", N(Pav / 1000), N(Preq / 1000), N(W), N(rc, 3), N(rc * 196.85)), 3);
                }
            }
            case Difficulty.Hard:
            {
                double habs = Ex.Step(r, 3500, 7000, 250), rcS = 0.5, hsvc = habs * (1 - rcS / rc0);
                double hq = Ex.Step(r, 0.3, 0.8, 0.1) * habs, rcq = rc0 * (1 - hq / habs);
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("A razão de subida de um avião cai linearmente com a altitude, de {0} m/s ao nível do mar até 0 m/s no teto absoluto de {1} m. Qual a razão de subida a {2} m, em m/s?", N(rc0), N(habs), N(hq)), rcq, "m/s",
                        L.F("RS(h) = RS₀·(1 − h/h_abs) = {0}·(1 − {1}/{2}) = {3} m/s.", N(rc0), N(hq), N(habs), N(rcq, 3)), 3);
                    case 1: return Ex.Num(L.F("A razão de subida de um avião cai linearmente com a altitude, de {0} m/s ao nível do mar até 0 m/s no teto absoluto de {1} m. O teto de serviço é onde ela chega a 0,5 m/s. Qual é ele, em m?", N(rc0), N(habs)), hsvc, "m",
                        L.F("0,5 = {0}·(1 − h/{1}) → h = {1}·(1 − 0,5/{0}) = {2} m.", N(rc0), N(habs), N(hsvc)), 3);
                    default:
                    {
                        double rc1 = Ex.Step(r, 1, 3, 0.5), h1 = habs * (1 - rc1 / rc0);
                        return Ex.Num(L.F("A razão de subida de um avião cai linearmente com a altitude e é {0} m/s no nível do mar. A {1} m ela é {2} m/s. Qual o teto absoluto, em m?", N(rc0), N(h1), N(rc1)), habs, "m",
                            L.F("RS = RS₀·(1 − h/h_abs) → h_abs = h / (1 − RS/RS₀) = {0} / (1 − {1}/{2}) = {3} m.", N(h1), N(rc1), N(rc0), N(habs)), 3);
                    }
                }
            }
            default:
            {
                double habs = Ex.Step(r, 4000, 7000, 250), ht = Ex.Step(r, 0.5, 0.9, 0.05) * habs, t = habs / rc0 * Math.Log(1 / (1 - ht / habs));
                return Ex.Num(L.F("A razão de subida de um avião cai linearmente com a altitude, de {0} m/s ao nível do mar a 0 m/s no teto absoluto de {1} m. Quantos minutos leva para subir do solo até {2} m? (use t = (h_abs/RS₀)·ln(1/(1 − h/h_abs)))", N(rc0), N(habs), N(ht)), t / 60, "min",
                    L.F("t = ({0}/{1})·ln(1/(1 − {2}/{0})) = {3} s = {4} min.", N(habs), N(rc0), N(ht), N(t), N(t / 60)), 3);
            }
        }
    }
}

[Exercises("aero-14-curva-potencia")]
public sealed class Air14 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double cd0 = Ex.Step(r, 0.022, 0.04, 0.001), e = Ex.Step(r, 0.7, 0.9, 0.01), ar = Ex.Step(r, 6, 12, 0.5), k = 1 / (Math.PI * e * ar);
        double ldm = 1 / (2 * Math.Sqrt(k * cd0)), m = Ex.Step(r, 700, 2500, 50), W = m * G, S = Ex.Step(r, 10, 22, 1), rho = Isa.Rho(Ex.Pick(r, 0.0, 1000, 2000));
        double vmd = Math.Sqrt(2 * W / (rho * S)) * Math.Pow(k / cd0, 0.25), vmp = vmd / Math.Pow(3, 0.25), dmin = W / ldm, pmin = W * vmp / (0.8660254 * ldm);
        string ac = L.F("Um avião de {0} kg, asa de {1} m², CD0 = {2}, AR = {3}, e = {4}, em ar de ρ = {5} kg/m³ (g = 9,8 m/s²).", N(m), N(S), N(cd0, 3), N(ar), N(e), N(rho, 3));
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(ac + " " + L.T("Qual o fator k = 1/(π·e·AR) do arrasto induzido (CDi = k·CL²)?"), k, "",
                        L.F("k = 1 / (π·{0}·{1}) = {2}.", N(e), N(ar), N(k, 4)), 3);
                return Ex.Num(ac + " " + L.T("Qual o (L/D) máximo = 1 / (2·√(k·CD0))?"), ldm, "",
                    L.F("k = {0}; (L/D)máx = 1 / (2·√({0}·{1})) = {2}.", N(k, 4), N(cd0, 3), N(ldm)), 3);
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(ac + " " + L.T("Qual o arrasto mínimo, em N?"), dmin, "N",
                        L.F("(L/D)máx = {0}; D_mín = W / (L/D)máx = {1} / {0} = {2} N.", N(ldm), N(W), N(dmin)), 3);
                return Ex.Num(ac + " " + L.T("Qual o CL de mínimo arrasto, em √(CD0/k)?"), Math.Sqrt(cd0 / k), "",
                    L.F("CL = √(CD0/k) = √({0}/{1}) = {2}.", N(cd0, 3), N(k, 4), N(Math.Sqrt(cd0 / k), 3)), 3);
            case Difficulty.Hard:
                if (r.Next(2) == 0)
                    return Ex.Num(ac + " " + L.T("Qual a velocidade de mínimo arrasto V_md, em m/s?"), vmd, "m/s",
                        L.F("V_md = √(2W/(ρ·S))·(k/CD0)^(1/4) = √(2·{0}/({1}·{2}))·({3}/{4})^(1/4) = {5} m/s.", N(W), N(rho, 3), N(S), N(k, 4), N(cd0, 3), N(vmd)), 3);
                return Ex.Num(ac + " " + L.T("Qual a velocidade de mínima potência V_mp = V_md / 3^(1/4), em m/s?"), vmp, "m/s",
                    L.F("V_md = {0} m/s; V_mp = {0} / 3^(1/4) = {1} m/s (cerca de 76% de V_md).", N(vmd), N(vmp)), 3);
            default:
                if (r.Next(2) == 0)
                    return Ex.Num(ac + " " + L.T("Qual a potência mínima necessária para voar nivelado, em kW? (em V_mp, onde L/D = 0,866·(L/D)máx)"), pmin / 1000, "kW",
                        L.F("V_mp = {0} m/s; L/D = 0,866·{1} = {2}; P = W·V / (L/D) = {3} kW.", N(vmp), N(ldm), N(0.866 * ldm), N(pmin / 1000)), 3);
                {
                    double dAtVmd = dmin, dAtVmp = W / (0.8660254 * ldm);
                    return Ex.Num(ac + " " + L.T("De quantos por cento o arrasto em V_mp (L/D = 0,866·(L/D)máx) é maior que o arrasto mínimo?"), (dAtVmp / dAtVmd - 1) * 100, "%",
                        L.T("D_mp / D_mín = 1/0,866 = 1,155: o arrasto em V_mp é 15,5% maior (o preço de gastar menos potência)."), 3);
                }
        }
    }
}

[Exercises("aero-15-alcance-jato")]
public sealed class Air15 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double h = Ex.Step(r, 7000, 11000, 250), a = Isa.Sound(h), mach = Ex.Step(r, 0.7, 0.85, 0.01), v = mach * a;
        double tsfc = Ex.Step(r, 1.4, 2.2, 0.05) * 1e-5, ld = Ex.Step(r, 12, 20, 0.5), w0 = Ex.Step(r, 20000, 80000, 1000), ff = Ex.Step(r, 0.15, 0.35, 0.01), w1 = w0 * (1 - ff);
        double range = v / (G * tsfc) * ld * Math.Log(w0 / w1);
        string ac = L.F("Um jato (TSFC = {0}·10⁻⁵ kg/(N·s), L/D = {1}) voa a {2} m/s.", N(tsfc * 1e5), N(ld), N(v, 1));
        switch (d)
        {
            case Difficulty.Easy:
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("A {0} m de altitude a temperatura padrão é {1} K. Qual a velocidade do som, em m/s? (a = √(1,4·287·T))", N(h), N(Isa.Temp(h), 1)), Math.Sqrt(1.4 * 287 * Isa.Temp(h)), "m/s",
                        L.F("a = √(1,4·287·{0}) = {1} m/s.", N(Isa.Temp(h), 1), N(Math.Sqrt(1.4 * 287 * Isa.Temp(h)))));
                    default: return Ex.Num(L.F("A {0} m o som viaja a {1} m/s. Qual a velocidade, em m/s, de um jato em Mach {2}?", N(h), N(a), N(mach)), v, "m/s",
                        L.F("V = M·a = {0}·{1} = {2} m/s.", N(mach), N(a), N(v)));
                }
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(ac + " " + L.F("Ele decola o cruzeiro com {0} kg e termina com {1} kg. Qual a razão W₀/W₁?", N(w0), N(w1)), w0 / w1, "",
                        L.F("W₀/W₁ = {0}/{1} = {2}.", N(w0), N(w1), N(w0 / w1, 3)), 3);
                return Ex.Num(ac + " " + L.F("Cruzando de {0} kg para {1} kg, qual o alcance, em km? (g = 9,8 m/s²)", N(w0), N(w1)), range / 1000, "km",
                    L.F("R = (V/(g·TSFC))·(L/D)·ln(W₀/W₁) = ({0}/(9,8·{1}))·{2}·ln({3}) = {4} km.", N(v, 1), N(tsfc, 7), N(ld), N(w0 / w1, 3), N(range / 1000)), 3);
            case Difficulty.Hard:
                switch (r.Next(3))
                {
                    case 0:
                    {
                        double rg = Ex.Step(r, 2000, 8000, 250) * 1000, x = rg * G * tsfc / (v * ld), fuel = w0 * (1 - Math.Exp(-x));
                        return Ex.Num(ac + " " + L.F("Qual a massa de combustível, em kg, para {0} km, com massa inicial de {1} kg? (g = 9,8 m/s²)", N(rg / 1000), N(w0)), fuel, "kg",
                            L.F("ln(W₀/W₁) = R·g·TSFC / (V·L/D) = {0}; W₁ = W₀·e^(−{0}); combustível = {1} kg.", N(x, 4), N(fuel)), 3);
                    }
                    case 1:
                    {
                        double ld2 = ld * Ex.Step(r, 1.05, 1.2, 0.05);
                        return Ex.Num(ac + " " + L.F("Cruzando de {0} kg para {1} kg. Se o L/D passar de {2} para {3}, qual o novo alcance, em km? (g = 9,8 m/s²)", N(w0), N(w1), N(ld), N(ld2)), range * ld2 / ld / 1000, "km",
                            L.F("R ∝ L/D: {0} km × {1}/{2} = {3} km.", N(range / 1000), N(ld2), N(ld), N(range * ld2 / ld / 1000)), 3);
                    }
                    default:
                    {
                        double h2 = h - 3000, a2 = Isa.Sound(h2), v2 = mach * a2, range2 = v2 / (G * tsfc) * ld * Math.Log(w0 / w1);
                        return Ex.Num(L.F("Um jato cruza em Mach {0} de {1} kg para {2} kg, com TSFC = {3}·10⁻⁵ kg/(N·s) e L/D = {4}. A {5} m a velocidade do som é {6} m/s e a {7} m é {8} m/s. Quantos km de alcance a mais ele tem em {5} m do que a {7} m? (g = 9,8 m/s²)", N(mach), N(w0), N(w1), N(tsfc * 1e5), N(ld), N(h), N(a), N(h2), N(a2)), (range - range2) / 1000, "km",
                            L.F("Para o mesmo Mach, V = M·a: {0} e {1} m/s. R ∝ V: {2} km contra {3} km; diferença = {4} km.", N(v), N(v2), N(range / 1000), N(range2 / 1000), N((range - range2) / 1000)), 3);
                    }
                }
            default:
            {
                double S = Ex.Step(r, 60, 150, 5), rho = Isa.Rho(h), m = w0 - 0.5 * (w0 - w1), W = m * G, cd0 = Ex.Step(r, 0.018, 0.025, 0.001), e = 0.85, ar = Ex.Step(r, 8, 10, 0.5);
                double CL = 2 * W / (rho * v * v * S), CD = cd0 + CL * CL / (Math.PI * e * ar), ld3 = CL / CD, range3 = v / (G * tsfc) * ld3 * Math.Log(w0 / w1);
                return Ex.Num(L.F("Um jato cruza a {0} m (ρ = {1} kg/m³) a {2} m/s, de {3} kg para {4} kg, com asa de {5} m², CD0 = {6}, AR = {7}, e = 0,85 e TSFC = {8}·10⁻⁵ kg/(N·s). Use o peso médio no meio do cruzeiro para achar o CL e o L/D. Qual o alcance, em km? (g = 9,8 m/s²)", N(h), N(rho, 3), N(v, 1), N(w0), N(w1), N(S), N(cd0, 3), N(ar), N(tsfc * 1e5)), range3 / 1000, "km",
                    L.F("CL = 2·W/(ρ·V²·S) = {0}; CD = CD0 + CL²/(π·e·AR) = {1}; L/D = {2}; R = (V/(g·TSFC))·(L/D)·ln(W₀/W₁) = {3} km.", N(CL, 3), N(CD, 4), N(ld3), N(range3 / 1000)), 3);
            }
        }
    }
}
