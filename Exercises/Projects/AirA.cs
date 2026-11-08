using static EasyCreator.Exercises.Pv;

namespace EasyCreator.Exercises;

/// <summary>Aircraft 1-5: lift, angle of attack, the drag polar, stall and flaps, takeoff and climb.</summary>
[Exercises("aero-01-sustentacao")]
public sealed class Air01 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double h = Ex.Pick(r, 0.0, 500, 1000, 1500, 2000, 3000, 4000), rho = Isa.Rho(h);
        double S = Ex.Step(r, 8, 40, 1), v = Ex.Step(r, 35, 90, 1), CL = Ex.Step(r, 0.3, 1.3, 0.05), L1 = 0.5 * rho * v * v * S * CL;
        string air = L.F("(ρ = {0} kg/m³ na altitude do voo)", N(rho, 3));
        switch (d)
        {
            case Difficulty.Easy:
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Uma asa de {0} m² voa a {1} m/s com CL = {2} {3}. Qual a sustentação, em N?", N(S), N(v), N(CL), air), L1, "N",
                        L.F("L = ½·ρ·v²·S·CL = ½·{0}·{1}²·{2}·{3} = {4} N.", N(rho, 3), N(v), N(S), N(CL), N(L1)));
                    case 1: return Ex.Num(L.F("Uma asa de {0} m² a {1} m/s gera {2} N de sustentação {3}. Qual o CL?", N(S), N(v), N(L1), air), CL, "",
                        L.F("CL = 2·L / (ρ·v²·S) = 2·{0} / ({1}·{2}²·{3}) = {4}.", N(L1), N(rho, 3), N(v), N(S), N(CL, 3)), 3);
                    default: return Ex.Num(L.F("Uma asa de {0} m² com CL = {1} gera {2} N de sustentação {3}. Qual a velocidade, em m/s?", N(S), N(CL), N(L1), air), v, "m/s",
                        L.F("v = √(2·L / (ρ·S·CL)) = √(2·{0} / ({1}·{2}·{3})) = {4} m/s.", N(L1), N(rho, 3), N(S), N(CL), N(v)));
                }
            case Difficulty.Medium:
            {
                double m = Ex.Step(r, 600, 3000, 50), W = m * G, Sw = Ex.Step(r, 10, 30, 1), clc = Ex.Step(r, 0.4, 1.2, 0.05);
                double vl = Math.Sqrt(2 * W / (rho * Sw * clc)), Sneed = 2 * W / (rho * v * v * clc), clneed = 2 * W / (rho * v * v * Sw);
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um avião de {0} kg, com asa de {1} m², voa em nível com CL = {2} {3}. Qual a velocidade, em m/s? (g = 9,8 m/s²)", N(m), N(Sw), N(clc), air), vl, "m/s",
                        L.F("Em voo nivelado L = W: v = √(2·m·g / (ρ·S·CL)) = √(2·{0}·9,8 / ({1}·{2}·{3})) = {4} m/s.", N(m), N(rho, 3), N(Sw), N(clc), N(vl)));
                    case 1: return Ex.Num(L.F("Um avião de {0} kg precisa voar em nível a {1} m/s com CL = {2} {3}. Qual a área de asa necessária, em m²? (g = 9,8 m/s²)", N(m), N(v), N(clc), air), Sneed, "m²",
                        L.F("L = W → S = 2·m·g / (ρ·v²·CL) = 2·{0}·9,8 / ({1}·{2}²·{3}) = {4} m².", N(m), N(rho, 3), N(v), N(clc), N(Sneed)));
                    default: return Ex.Num(L.F("Um avião de {0} kg, com asa de {1} m², voa em nível a {2} m/s {3}. Que CL a asa precisa ter? (g = 9,8 m/s²)", N(m), N(Sw), N(v), air), clneed, "",
                        L.F("L = W → CL = 2·m·g / (ρ·v²·S) = 2·{0}·9,8 / ({1}·{2}²·{3}) = {4}.", N(m), N(rho, 3), N(v), N(Sw), N(clneed, 3)), 3);
                }
            }
            case Difficulty.Hard:
            {
                double k = Ex.Pick(r, 1.1, 1.2, 1.3, 1.5, 2.0);
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("A sustentação de uma asa é {0} N. Se a velocidade for multiplicada por {1} (mesmo CL, mesma densidade), qual a nova sustentação, em N?", N(L1), N(k)), L1 * k * k, "N",
                        L.F("L ∝ v²: {0} · {1}² = {2} N.", N(L1), N(k), N(L1 * k * k)));
                    case 1:
                    {
                        double h2 = Ex.Pick(r, 2000.0, 3000, 4000, 5000), rho2 = Isa.Rho(h2), v2 = v * Math.Sqrt(Isa.Rho(0) / rho2);
                        return Ex.Num(L.F("Um avião voa a {0} m/s ao nível do mar (ρ = 1,225 kg/m³) com um certo CL. Para manter a mesma sustentação e o mesmo CL a {1} m de altitude (ρ = {2} kg/m³), que velocidade verdadeira, em m/s, ele precisa?", N(v), N(h2), N(rho2, 3)), v2, "m/s",
                            L.F("ρ·v² constante: v₂ = v₁·√(ρ₁/ρ₂) = {0}·√(1,225/{1}) = {2} m/s.", N(v), N(rho2, 3), N(v2)));
                    }
                    default:
                    {
                        double h2 = Ex.Pick(r, 2000.0, 3000, 4000, 5000), rho2 = Isa.Rho(h2), L2 = 0.5 * rho2 * v * v * S * CL;
                        return Ex.Num(L.F("Uma asa de {0} m² voa a {1} m/s com CL = {2}. Quantos N de sustentação ela perde subindo do nível do mar (ρ = 1,225 kg/m³) para {3} m (ρ = {4} kg/m³), mantendo velocidade e CL?", N(S), N(v), N(CL), N(h2), N(rho2, 3)), 0.5 * 1.225 * v * v * S * CL - L2, "N",
                            L.F("L = ½·ρ·v²·S·CL: ao nível do mar {0} N; a {1} m: {2} N; perda: {3} N.", N(0.5 * 1.225 * v * v * S * CL), N(h2), N(L2), N(0.5 * 1.225 * v * v * S * CL - L2)));
                    }
                }
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    double kk = Ex.Pick(r, 4.0, 6, 9, 16), vm = Ex.Step(r, 10, 25, 1), vf = vm * Math.Sqrt(kk);
                    return Ex.Num(L.F("Um modelo em escala 1:{0} (todas as dimensões lineares {0} vezes menores, mesma densidade média) voa em nível a {1} m/s com certo CL. A que velocidade, em m/s, o avião em tamanho real voa com o mesmo CL e o mesmo ar?", N(kk, 0), N(vm)), vf, "m/s",
                        L.F("Peso ∝ escala³, área ∝ escala²: v² = 2W/(ρ·S·CL) ∝ escala. Então v = {0}·√{1} = {2} m/s.", N(vm), N(kk, 0), N(vf)));
                }
                double m1 = Ex.Step(r, 800, 2000, 100), add = Ex.Step(r, 100, 500, 50), v1 = Ex.Step(r, 40, 70, 1), v2 = v1 * Math.Sqrt((m1 + add) / m1);
                return Ex.Num(L.F("Um avião de {0} kg voa em nível a {1} m/s com um certo CL. Depois de carregar mais {2} kg, a que velocidade, em m/s, ele voa com o mesmo CL e na mesma altitude?", N(m1), N(v1), N(add)), v2, "m/s",
                    L.F("L = W e v² ∝ W: v₂ = v₁·√((m + Δm)/m) = {0}·√({1}/{2}) = {3} m/s.", N(v1), N(m1 + add), N(m1), N(v2)));
            }
        }
    }
}

[Exercises("aero-02-angulo-ataque")]
public sealed class Air02 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double a0 = Ex.Step(r, 0.09, 0.115, 0.005), al0 = Ex.Pick(r, 0.0, -1, -2, -3, -4), al = Ex.Step(r, 0, 12, 0.5), CL = a0 * (al - al0);
        string polar = L.F("Um perfil tem inclinação da curva CL = {0} por grau e ângulo de sustentação nula α₀ = {1}°.", N(a0, 3), N(al0));
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(polar + " " + L.F("Qual o CL a α = {0}°?", N(al)), CL, "",
                        L.F("CL = a·(α − α₀) = {0}·({1} − ({2})) = {3}.", N(a0, 3), N(al), N(al0), N(CL, 3)), 3);
                return Ex.Num(polar + " " + L.F("Que ângulo de ataque, em graus, dá CL = {0}?", N(CL, 3)), al, "°",
                    L.F("α = α₀ + CL / a = {0} + {1} / {2} = {3}°.", N(al0), N(CL, 3), N(a0, 3), N(al)), 3);
            case Difficulty.Medium:
            {
                double S = Ex.Step(r, 10, 25, 1), v = Ex.Step(r, 40, 80, 1), rho = 1.225, Lf = 0.5 * rho * v * v * S * CL;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(polar + " " + L.F("A asa tem {0} m² e voa a {1} m/s (ρ = 1,225 kg/m³) com α = {2}°. Qual a sustentação, em N?", N(S), N(v), N(al)), Lf, "N",
                        L.F("CL = {0}; L = ½·ρ·v²·S·CL = ½·1,225·{1}²·{2}·{0} = {3} N.", N(CL, 3), N(v), N(S), N(Lf)), 3);
                    default:
                    {
                        double m = Lf / G;
                        return Ex.Num(polar + " " + L.F("A asa tem {0} m² e voa a {1} m/s (ρ = 1,225 kg/m³) com α = {2}°. Que massa, em kg, ela sustenta em voo nivelado? (g = 9,8 m/s²)", N(S), N(v), N(al)), m, "kg",
                            L.F("CL = {0}; L = {1} N; m = L / g = {1} / 9,8 = {2} kg.", N(CL, 3), N(Lf), N(m)), 3);
                    }
                }
            }
            case Difficulty.Hard:
            {
                double m = Ex.Step(r, 800, 2500, 50), S = Ex.Step(r, 12, 25, 1), v = Ex.Step(r, 45, 80, 1), clN = 2 * m * G / (1.225 * v * v * S), alN = al0 + clN / a0;
                return Ex.Num(polar + " " + L.F("Um avião de {0} kg, com asa de {1} m², voa em nível a {2} m/s (ρ = 1,225 kg/m³). Que ângulo de ataque, em graus, ele precisa? (g = 9,8 m/s²)", N(m), N(S), N(v)), alN, "°",
                    L.F("CL = 2·m·g / (ρ·v²·S) = {0}; α = α₀ + CL/a = {1}°.", N(clN, 3), N(alN)), 3);
            }
            default:
            {
                double m = Ex.Step(r, 800, 2500, 50), S = Ex.Step(r, 12, 25, 1), v1 = Ex.Step(r, 55, 80, 1), k = Ex.Step(r, 0.7, 0.9, 0.05), v2 = v1 * k;
                double cl1 = 2 * m * G / (1.225 * v1 * v1 * S), cl2 = cl1 / (k * k), a2 = al0 + cl2 / a0, a1 = al0 + cl1 / a0;
                if (r.Next(2) == 0)
                    return Ex.Num(polar + " " + L.F("Um avião de {0} kg, com asa de {1} m², voa em nível a {2} m/s e depois reduz para {3} m/s sem mudar de altitude (ρ = 1,225 kg/m³). Quantos graus o ângulo de ataque aumenta? (g = 9,8 m/s²)", N(m), N(S), N(v1), N(v2)), a2 - a1, "°",
                        L.F("CL₁ = {0}, CL₂ = CL₁·(v₁/v₂)² = {1}; α = α₀ + CL/a: {2}° e {3}°; diferença = {4}°.", N(cl1, 3), N(cl2, 3), N(a1), N(a2), N(a2 - a1)), 3);
                double stall = Ex.Step(r, 14, 18, 1), clmax = a0 * (stall - al0);
                double vs = Math.Sqrt(2 * m * G / (1.225 * S * clmax));
                return Ex.Num(polar + " " + L.F("O perfil estola a α = {0}°. Para um avião de {1} kg, asa de {2} m², em ar de ρ = 1,225 kg/m³, qual a velocidade de estol em voo nivelado, em m/s? (g = 9,8 m/s²)", N(stall), N(m), N(S)), vs, "m/s",
                    L.F("CLmáx = a·(α_estol − α₀) = {0}; V_estol = √(2·m·g / (ρ·S·CLmáx)) = {1} m/s.", N(clmax, 3), N(vs)), 3);
            }
        }
    }
}

[Exercises("aero-03-arrasto-polar")]
public sealed class Air03 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double cd0 = Ex.Step(r, 0.02, 0.04, 0.001), e = Ex.Step(r, 0.7, 0.95, 0.01), ar = Ex.Step(r, 5, 14, 0.5), k = 1 / (Math.PI * e * ar), cl = Ex.Step(r, 0.2, 1.2, 0.05);
        string wing = L.F("Uma asa tem CD0 = {0}, alongamento AR = {1} e fator de Oswald e = {2}.", N(cd0, 3), N(ar), N(e));
        double cdi = k * cl * cl, cd = cd0 + cdi, ldmax = 0.5 * Math.Sqrt(Math.PI * e * ar / cd0), clbest = Math.Sqrt(cd0 * Math.PI * e * ar);
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(wing + " " + L.F("Qual o coeficiente de arrasto induzido com CL = {0}?", N(cl)), cdi, "",
                        L.F("CDi = CL² / (π·e·AR) = {0}² / (π·{1}·{2}) = {3}.", N(cl), N(e), N(ar), N(cdi, 4)), 3);
                return Ex.Num(wing + " " + L.F("Qual o arrasto total CD com CL = {0}?", N(cl)), cd, "",
                    L.F("CD = CD0 + CL²/(π·e·AR) = {0} + {1} = {2}.", N(cd0, 3), N(cdi, 4), N(cd, 4)), 3);
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(wing + " " + L.F("Qual a razão sustentação/arrasto (L/D) com CL = {0}?", N(cl)), cl / cd, "",
                        L.F("CD = {0}; L/D = CL/CD = {1}/{0} = {2}.", N(cd, 4), N(cl), N(cl / cd)), 3);
                return Ex.Num(wing + " " + L.F("Em qual CL o arrasto induzido iguala o parasita (CD0)?"), clbest, "",
                    L.F("CL²/(π·e·AR) = CD0 → CL = √(CD0·π·e·AR) = √({0}·π·{1}·{2}) = {3}.", N(cd0, 3), N(e), N(ar), N(clbest, 3)), 3);
            case Difficulty.Hard:
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(wing + " " + L.T("Qual a máxima razão L/D?"), ldmax, "",
                        L.F("(L/D)máx = ½·√(π·e·AR/CD0) = ½·√(π·{0}·{1}/{2}) = {3}.", N(e), N(ar), N(cd0, 3), N(ldmax)), 3);
                    default:
                    {
                        double W = Ex.Step(r, 5000, 30000, 500);
                        return Ex.Num(wing + " " + L.F("O avião pesa {0} N e voa em L/D máximo. Qual o arrasto, em N?", N(W)), W / ldmax, "N",
                            L.F("(L/D)máx = {0}; D = W / (L/D) = {1} / {0} = {2} N.", N(ldmax), N(W), N(W / ldmax)), 3);
                    }
                }
            default:
            {
                double ar2 = ar * Ex.Step(r, 1.3, 2.0, 0.1), ld2 = 0.5 * Math.Sqrt(Math.PI * e * ar2 / cd0);
                if (r.Next(2) == 0)
                    return Ex.Num(wing + " " + L.F("Num projeto novo o alongamento sobe para {0} (mesmos CD0 e e). De quantos por cento aumenta o (L/D) máximo?", N(ar2)), (ld2 / ldmax - 1) * 100, "%",
                        L.F("(L/D)máx ∝ √AR: {0} → {1}; aumento = √({2}/{3}) − 1 = {4}%.", N(ldmax), N(ld2), N(ar2), N(ar), N((ld2 / ldmax - 1) * 100)), 3);
                double m = Ex.Step(r, 600, 2500, 50), W = m * 9.8, S = Ex.Step(r, 10, 25, 1), rho = 1.225;
                double vb = Math.Sqrt(2 * W / (rho * S * clbest)), D = W / ldmax;
                return Ex.Num(wing + " " + L.F("Um avião de {0} kg, com asa de {1} m² (ρ = 1,225 kg/m³, g = 9,8 m/s²), voa na velocidade de melhor L/D. Quantos kW de potência de arrasto (D·V) ele precisa vencer?", N(m), N(S)), D * vb / 1000, "kW",
                    L.F("CL ótimo = {0}; V = √(2W/(ρ·S·CL)) = {1} m/s; D = W/(L/D)máx = {2} N; P = D·V = {3} kW.", N(clbest, 3), N(vb), N(D), N(D * vb / 1000)), 3);
            }
        }
    }
}

[Exercises("aero-04-estol")]
public sealed class Air04 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double m = Ex.Step(r, 600, 3000, 50), S = Ex.Step(r, 10, 30, 1), clmax = Ex.Step(r, 1.2, 1.7, 0.05), h = Ex.Pick(r, 0.0, 500, 1000, 2000, 3000), rho = Isa.Rho(h);
        double vs = Math.Sqrt(2 * m * G / (rho * S * clmax));
        string ac = L.F("Um avião de {0} kg, asa de {1} m², CLmáx = {2}, em ar de ρ = {3} kg/m³ (g = 9,8 m/s²).", N(m), N(S), N(clmax), N(rho, 3));
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(ac + " " + L.T("Qual a velocidade de estol, em m/s?"), vs, "m/s",
                        L.F("V_estol = √(2·m·g / (ρ·S·CLmáx)) = √(2·{0}·9,8 / ({1}·{2}·{3})) = {4} m/s.", N(m), N(rho, 3), N(S), N(clmax), N(vs)));
                return Ex.Num(ac + " " + L.T("Qual a velocidade de estol, em km/h?"), Kmh(vs), "km/h",
                    L.F("V_estol = √(2·m·g / (ρ·S·CLmáx)) = {0} m/s = {1} km/h.", N(vs), N(Kmh(vs))));
            case Difficulty.Medium:
            {
                double ws = m * G / S;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(ac + " " + L.T("Qual a carga alar (peso/área), em N/m²?"), ws, "N/m²",
                        L.F("W/S = {0}·9,8 / {1} = {2} N/m².", N(m), N(S), N(ws)));
                    case 1:
                    {
                        double clf = clmax + Ex.Step(r, 0.4, 0.9, 0.05), vf = Math.Sqrt(2 * m * G / (rho * S * clf));
                        return Ex.Num(ac + " " + L.F("Com flapes totalmente baixados o CLmáx sobe para {0}. Qual a nova velocidade de estol, em m/s?", N(clf)), vf, "m/s",
                            L.F("V = √(2·m·g / (ρ·S·CLmáx)) = √(2·{0}·9,8 / ({1}·{2}·{3})) = {4} m/s.", N(m), N(rho, 3), N(S), N(clf), N(vf)));
                    }
                    default: return Ex.Num(ac + " " + L.T("Qual a velocidade de aproximação (1,3 × V_estol), em m/s?"), 1.3 * vs, "m/s",
                        L.F("V_estol = {0} m/s; 1,3 × {0} = {1} m/s.", N(vs), N(1.3 * vs)));
                }
            }
            case Difficulty.Hard:
            {
                double bank = Ex.Step(r, 20, 60, 5), n = 1 / Math.Cos(bank * Math.PI / 180);
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(ac + " " + L.F("Numa curva nivelada com {0}° de inclinação, qual a velocidade de estol, em m/s?", N(bank)), vs * Math.Sqrt(n), "m/s",
                        L.F("n = 1/cos φ = {0}; V = V_estol·√n = {1}·√{0} = {2} m/s.", N(n, 3), N(vs), N(vs * Math.Sqrt(n))));
                    case 1:
                    {
                        double rho2 = Isa.Rho(h + 2000), v2 = Math.Sqrt(2 * m * G / (rho2 * S * clmax));
                        return Ex.Num(ac + " " + L.F("Em um aeródromo 2000 m mais alto (ρ = {0} kg/m³), qual a velocidade de estol (verdadeira), em m/s?", N(rho2, 3)), v2, "m/s",
                            L.F("V ∝ 1/√ρ: V = {0}·√({1}/{2}) = {3} m/s.", N(vs), N(rho, 3), N(rho2, 3), N(v2)));
                    }
                    default:
                    {
                        double clf = clmax + Ex.Step(r, 0.4, 0.9, 0.05), vf = Math.Sqrt(2 * m * G / (rho * S * clf));
                        return Ex.Num(ac + " " + L.F("Os flapes elevam o CLmáx para {0}. De quantos por cento a velocidade de estol diminui?", N(clf)), (1 - vf / vs) * 100, "%",
                            L.F("V ∝ 1/√CLmáx: {0} → {1} m/s; redução = {2}%.", N(vs), N(vf), N((1 - vf / vs) * 100)));
                    }
                }
            }
            default:
            {
                double vap = Ex.Step(r, 30, 45, 1), clNeeded = 2 * m * G / (rho * S * Math.Pow(vap / 1.3, 2));
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um avião de {0} kg, asa de {1} m², em ar de ρ = {2} kg/m³ (g = 9,8 m/s²), precisa de velocidade de aproximação (1,3 × V_estol) de {3} m/s. Qual o CLmáx necessário, com flapes?", N(m), N(S), N(rho, 3), N(vap)), clNeeded, "",
                        L.F("V_estol = {0}/1,3 = {1} m/s; CLmáx = 2·m·g / (ρ·S·V²) = {2}.", N(vap), N(vap / 1.3), N(clNeeded, 3)), 3);
                    default:
                    {
                        double clf = clmax + 0.6, Sred = m * G * 2 / (rho * clf * Math.Pow(vap / 1.3, 2));
                        return Ex.Num(L.F("Um avião de {0} kg usa flapes que elevam o CLmáx para {1}. Em ar de ρ = {2} kg/m³ (g = 9,8 m/s²), qual a área de asa mínima, em m², para uma velocidade de aproximação (1,3 × V_estol) de {3} m/s?", N(m), N(clf), N(rho, 3), N(vap)), Sred, "m²",
                            L.F("V_estol = {0}/1,3 = {1} m/s; S = 2·m·g / (ρ·CLmáx·V²) = {2} m².", N(vap), N(vap / 1.3), N(Sred)));
                    }
                }
            }
        }
    }
}

[Exercises("aero-05-decolagem")]
public sealed class Air05 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double m = Ex.Step(r, 700, 3000, 50), S = Ex.Step(r, 12, 30, 1), clmax = Ex.Step(r, 1.3, 1.8, 0.05), rho = Isa.Rho(Ex.Pick(r, 0.0, 500, 1000, 1500, 2500));
        double vs = Math.Sqrt(2 * m * G / (rho * S * clmax)), vlof = 1.2 * vs;
        double T = m * G * Ex.Step(r, 0.25, 0.45, 0.01), mur = 0.03, a = T / m - mur * G, run = vlof * vlof / (2 * a);
        string ac = L.F("Um avião de {0} kg, asa de {1} m², CLmáx = {2}, em ar de ρ = {3} kg/m³ (g = 9,8 m/s²).", N(m), N(S), N(clmax), N(rho, 3));
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(ac + " " + L.T("Qual a velocidade de decolagem (1,2 × V_estol), em m/s?"), vlof, "m/s",
                        L.F("V_estol = √(2·m·g/(ρ·S·CLmáx)) = {0} m/s; V_dec = 1,2 × {0} = {1} m/s.", N(vs), N(vlof)));
                return Ex.Num(ac + " " + L.T("Qual a velocidade de decolagem (1,2 × V_estol), em km/h?"), Kmh(vlof), "km/h",
                    L.F("V_estol = {0} m/s; V_dec = 1,2 × {0} = {1} m/s = {2} km/h.", N(vs), N(vlof), N(Kmh(vlof))));
            case Difficulty.Medium:
            {
                if (r.Next(2) == 0)
                    return Ex.Num(ac + " " + L.F("O empuxo médio na corrida é {0} N e o atrito de rolamento é 0,03·m·g. Qual a aceleração média, em m/s²?", N(T)), a, "m/s²",
                        L.F("a = T/m − μr·g = {0}/{1} − 0,03·9,8 = {2} m/s².", N(T), N(m), N(a)));
                return Ex.Num(ac + " " + L.F("A aceleração média na corrida é {0} m/s² (de 0 até a velocidade de decolagem, 1,2 × V_estol). Qual a distância da corrida, em m?", N(a)), run, "m",
                    L.F("V_dec = {0} m/s; s = V² / (2a) = {0}² / (2·{1}) = {2} m.", N(vlof), N(a), N(run)));
            }
            case Difficulty.Hard:
            {
                double v = Ex.Step(r, 40, 65, 1), D = m * G / Ex.Step(r, 8, 14, 0.5), Preq = D * v, Pav = Math.Round((Preq + m * G * Ex.Step(r, 1, 6, 0.5)) / 0.8 / 5000) * 5000;
                double rc = (Pav * 0.8 - Preq) / (m * G);
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um avião de {0} kg voa a {1} m/s com arrasto de {2} N. A potência disponível no eixo é {3} kW, com hélice de rendimento 0,8. Qual a razão de subida, em m/s? (g = 9,8 m/s²)", N(m), N(v), N(D), N(Pav / 1000)), rc, "m/s",
                        L.F("P_disp = 0,8·{0} = {1} kW; P_nec = D·V = {2} kW; RS = (P_disp − P_nec)/W = {3} m/s.", N(Pav / 1000), N(Pav * 0.8 / 1000), N(Preq / 1000), N(rc)), 3);
                    default:
                    {
                        double gam = Math.Asin(Math.Min(1, rc / v)) * 180 / Math.PI;
                        return Ex.Num(L.F("Um avião de {0} kg voa a {1} m/s com arrasto de {2} N. A potência disponível no eixo é {3} kW, com hélice de rendimento 0,8. Qual o ângulo de subida, em graus? (g = 9,8 m/s²)", N(m), N(v), N(D), N(Pav / 1000)), gam, "°",
                            L.F("RS = (0,8·P − D·V)/W = {0} m/s; sen γ = RS/V = {1}; γ = {2}°.", N(rc, 3), N(rc / v, 4), N(gam)), 3);
                    }
                }
            }
            default:
            {
                double wind = Ex.Step(r, 0, 12, 1), runW = Math.Pow(Math.Max(vlof - wind, 1), 2) / (2 * a);
                if (r.Next(2) == 0)
                    return Ex.Num(ac + " " + L.F("A corrida tem aceleração média de {0} m/s² e o vento de proa é {1} m/s. Qual a distância da corrida até a decolagem (1,2 × V_estol no ar), em m?", N(a), N(wind)), runW, "m",
                        L.F("Velocidade em relação ao solo na decolagem: {0} − {1} = {2} m/s; s = (V − vento)² / (2a) = {3} m.", N(vlof), N(wind), N(vlof - wind), N(runW)), 3);
                double Tobs = T * 0.9, Dcl = m * G / Ex.Step(r, 8, 12, 0.5), gam = (Tobs - Dcl) / (m * G), hobs = 15, dair = hobs / Math.Tan(Math.Asin(Math.Min(0.99, Math.Max(gam, 0.01))));
                return Ex.Num(ac + " " + L.F("Depois de decolar, o avião sobe com empuxo de {0} N e arrasto de {1} N. Que distância horizontal, em m, ele percorre até passar um obstáculo de 15 m? (g = 9,8 m/s², ângulo pequeno)", N(Tobs), N(Dcl)), dair, "m",
                    L.F("sen γ = (T − D)/W = {0}; distância = h / tg γ = 15 / tg({1}°) = {2} m.", N(gam, 4), N(Math.Asin(Math.Min(0.99, Math.Max(gam, 0.01))) * 180 / Math.PI), N(dair)), 4);
            }
        }
    }
}
