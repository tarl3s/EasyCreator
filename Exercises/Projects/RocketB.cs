namespace EasyCreator.Exercises;

/// <summary>Rockets 6-10: recovery, staging, Tsiolkovsky, orbits, the sounding-rocket mission.</summary>
[Exercises("rkt-06-recuperacao")]
public sealed class Rkt06 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double M = Ex.Step(r, 0.2, 2, 0.05);
        switch (d)
        {
            case Difficulty.Easy:
            {
                double H = Ex.Step(r, 50, 600, 10), v = Ex.Step(r, 3, 10, 0.5), t = H / v;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um foguete abre o paraquedas a {0} m de altura e desce a {1} m/s constantes. Quanto tempo leva até o chão?", Ex.N(H), Ex.N(v)), t, "s", L.F("t = H / v = {0} / {1} = {2} s.", Ex.N(H), Ex.N(v), Ex.N(t)));
                    case 1: return Ex.Num(L.F("Um foguete abre o paraquedas a {0} m de altura e leva {1} s para chegar ao chão, descendo com velocidade constante. Qual a velocidade de descida?", Ex.N(H), Ex.N(t, 2)), v, "m/s", L.F("v = H / t = {0} / {1} = {2} m/s.", Ex.N(H), Ex.N(t, 2), Ex.N(v)));
                    default: return Ex.Num(L.F("Um foguete de {0} kg pousa com paraquedas a {1} m/s. Qual a energia cinética no pouso?", Ex.N(M), Ex.N(v)), 0.5 * M * v * v, "J", L.F("E = ½·m·v² = 0,5·{0}·{1}² = {2} J.", Ex.N(M), Ex.N(v), Ex.N(0.5 * M * v * v)));
                }
            }
            case Difficulty.Medium:
            {
                double vb = Ex.Step(r, 30, 150, 5), Cd = 0.8, dia = Ex.Step(r, 0.3, 1.2, 0.05), A = Pk.Area(dia), vt = Math.Sqrt(2 * M * G / (Pk.Rho * Cd * A));
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um foguete termina a queima subindo a {0} m/s. Sem arrasto, quanto tempo depois ele chega ao apogeu — o atraso ideal para ejetar o paraquedas? (g = 9,8 m/s²)", Ex.N(vb)), vb / G, "s", L.F("No apogeu v = 0: t = v / g = {0} / 9,8 = {1} s.", Ex.N(vb), Ex.N(vb / G)));
                    case 1: return Ex.Num(L.F("Um foguete de {0} kg tem um paraquedas de {1} m de diâmetro (Cd = 0,8). Qual a velocidade de descida em equilíbrio? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", Ex.N(M), Ex.N(dia)), vt, "m/s", L.F("A = π·d²/4 = {0} m²; v = √(2·M·g / (ρ·Cd·A)) = √(2·{1}·9,8 / (1,2·0,8·{0})) = {2} m/s.", Ex.N(A, 4), Ex.N(M), Ex.N(vt)));
                    default: return Ex.Num(L.F("Um foguete sobe a {0} m/s ao fim da queima. Sem arrasto, quanto mais ele sobe até o apogeu? (g = 9,8 m/s²)", Ex.N(vb)), vb * vb / (2 * G), "m", L.F("h = v² / (2g) = {0}² / 19,6 = {1} m.", Ex.N(vb), Ex.N(vb * vb / (2 * G))));
                }
            }
            case Difficulty.Hard:
            {
                double vt = Ex.Step(r, 4, 8, 0.5), Cd = 0.75, A = 2 * M * G / (Pk.Rho * Cd * vt * vt), dia = Math.Sqrt(4 * A / Math.PI);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Qual o diâmetro do paraquedas (Cd = 0,75) para um foguete de {0} kg pousar a {1} m/s? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", Ex.N(M), Ex.N(vt)), dia, "m", L.F("Arrasto = peso: A = 2·M·g / (ρ·Cd·v²) = {0} m²; d = √(4A/π) = {1} m.", Ex.N(A, 4), Ex.N(dia, 3)));
                double mx = vt * vt * Pk.Rho * Cd * Pk.Area(dia) / (2 * G);
                return Ex.Num(L.F("Um paraquedas de {0} m de diâmetro (Cd = 0,75) está dimensionado para pouso a {1} m/s. Qual a maior massa de foguete que ele sustenta nessa velocidade? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", Ex.N(dia, 3), Ex.N(vt)), mx, "kg", L.F("M = ρ·Cd·A·v² / (2g) = 1,2·0,75·{0}·{1}² / 19,6 = {2} kg.", Ex.N(Pk.Area(dia), 4), Ex.N(vt), Ex.N(mx, 3)));
            }
            default:
            {
                double Cd = 0.75, dia = Ex.Step(r, 0.4, 1.2, 0.05), A = Pk.Area(dia), vt = Math.Sqrt(2 * M * G / (Pk.Rho * Cd * A));
                double H = Ex.Step(r, 200, 800, 50), w = Ex.Step(r, 2, 10, 1), drift = w * H / vt;
                return Ex.Num(L.F("Um foguete de {0} kg abre um paraquedas de {1} m de diâmetro (Cd = 0,75) a {2} m de altura. Ele desce na velocidade de equilíbrio e é levado por um vento horizontal de {3} m/s. A que distância horizontal do ponto de abertura ele pousa? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", Ex.N(M), Ex.N(dia), Ex.N(H), Ex.N(w)),
                    drift, "m", L.F("v = √(2·M·g/(ρ·Cd·A)) = {0} m/s; tempo de descida = H/v = {1} s; deriva = vento·tempo = {2}·{1} = {3} m.", Ex.N(vt), Ex.N(H / vt), Ex.N(w), Ex.N(drift)));
            }
        }
    }
}

[Exercises("rkt-07-estagios")]
public sealed class Rkt07 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                double ve = Ex.Step(r, 1500, 4000, 100), m0 = Ex.Step(r, 200, 2000, 50), mf = Math.Round(m0 / Ex.Step(r, 1.5, 4, 0.5)), dv = ve * Math.Log(m0 / mf);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um estágio de foguete começa com {0} kg e termina com {1} kg depois de queimar o propelente. Qual a razão de massa?", Ex.N(m0), Ex.N(mf)), m0 / mf, "", L.F("R = m₀ / m_f = {0} / {1} = {2}.", Ex.N(m0), Ex.N(mf), Ex.N(m0 / mf)));
                return Ex.Num(L.F("Um estágio com velocidade de exaustão de {0} m/s vai de {1} kg a {2} kg. Qual o Δv que ele fornece?", Ex.N(ve), Ex.N(m0), Ex.N(mf)), dv, "m/s", L.F("Δv = vₑ·ln(m₀/m_f) = {0}·ln({1}/{2}) = {3} m/s.", Ex.N(ve), Ex.N(m0), Ex.N(mf), Ex.N(dv)));
            }
            case Difficulty.Medium:
            {
                double ve1 = Ex.Step(r, 2000, 3000, 100), ve2 = Ex.Step(r, 2800, 4200, 100), r1 = Ex.Step(r, 1.8, 3, 0.1), r2 = Ex.Step(r, 1.8, 3.5, 0.1);
                double d1 = ve1 * Math.Log(r1), d2 = ve2 * Math.Log(r2);
                string intro = L.F("Um foguete de dois estágios: o 1º tem vₑ = {0} m/s e razão de massa {1}; o 2º tem vₑ = {2} m/s e razão de massa {3}.", Ex.N(ve1), Ex.N(r1), Ex.N(ve2), Ex.N(r2));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Qual o Δv total?"), d1 + d2, "m/s", L.F("Δv₁ = {0}·ln({1}) = {2}; Δv₂ = {3}·ln({4}) = {5}; total = {6} m/s.", Ex.N(ve1), Ex.N(r1), Ex.N(d1), Ex.N(ve2), Ex.N(r2), Ex.N(d2), Ex.N(d1 + d2)));
                return Ex.Num(intro + " " + L.T("Quanto do Δv total vem do 2º estágio?"), d2, "m/s", L.F("Δv₂ = vₑ₂·ln(R₂) = {0}·ln({1}) = {2} m/s.", Ex.N(ve2), Ex.N(r2), Ex.N(d2)));
            }
            case Difficulty.Hard:
            {
                double p = Ex.Step(r, 5, 30, 5), s2 = Ex.Step(r, 20, 60, 5), mp2 = Ex.Step(r, 150, 400, 10), s1 = Ex.Step(r, 80, 200, 10), mp1 = Ex.Step(r, 600, 1500, 50);
                double ve1 = Ex.Step(r, 2200, 3000, 100), ve2 = Ex.Step(r, 3000, 3800, 100);
                double m0 = p + s2 + mp2 + s1 + mp1, a1 = m0 - mp1, m02 = p + s2 + mp2, mf2 = p + s2;
                double d1 = ve1 * Math.Log(m0 / a1), d2 = ve2 * Math.Log(m02 / mf2);
                string intro = L.F("Um foguete de dois estágios leva carga útil de {0} kg. 2º estágio: estrutura {1} kg, propelente {2} kg, vₑ = {3} m/s. 1º estágio: estrutura {4} kg, propelente {5} kg, vₑ = {6} m/s. A estrutura do 1º é descartada ao acabar o seu propelente.", Ex.N(p), Ex.N(s2), Ex.N(mp2), Ex.N(ve2), Ex.N(s1), Ex.N(mp1), Ex.N(ve1));
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(intro + " " + L.T("Qual o Δv do 1º estágio?"), d1, "m/s", L.F("m₀ = {0} kg; no fim da queima, m = {0} − {1} = {2} kg; Δv₁ = {3}·ln({0}/{2}) = {4} m/s.", Ex.N(m0), Ex.N(mp1), Ex.N(a1), Ex.N(ve1), Ex.N(d1)));
                    case 1: return Ex.Num(intro + " " + L.T("Qual o Δv do 2º estágio?"), d2, "m/s", L.F("Início: {0} + {1} + {2} = {3} kg; fim: {0} + {1} = {4} kg; Δv₂ = {5}·ln({3}/{4}) = {6} m/s.", Ex.N(p), Ex.N(s2), Ex.N(mp2), Ex.N(m02), Ex.N(mf2), Ex.N(ve2), Ex.N(d2)));
                    default: return Ex.Num(intro + " " + L.T("Qual o Δv total do foguete?"), d1 + d2, "m/s", L.F("Δv₁ = {0}·ln({1}/{2}) = {3} m/s; Δv₂ = {4}·ln({5}/{6}) = {7} m/s; total = {8} m/s.", Ex.N(ve1), Ex.N(m0), Ex.N(a1), Ex.N(d1), Ex.N(ve2), Ex.N(m02), Ex.N(mf2), Ex.N(d2), Ex.N(d1 + d2)));
                }
            }
            default:
            {
                double p = Ex.Step(r, 5, 30, 5), s2 = Ex.Step(r, 20, 60, 5), ve2 = Ex.Step(r, 3000, 3800, 100), d1 = Ex.Step(r, 2000, 3200, 100), dt = d1 + Ex.Step(r, 2500, 4500, 100);
                double d2 = dt - d1, R = Math.Exp(d2 / ve2), mp2 = (R - 1) * (p + s2);
                return Ex.Num(L.F("Uma missão precisa de Δv total de {0} m/s com carga útil de {1} kg. O 1º estágio já entrega {2} m/s. O 2º tem estrutura de {3} kg e vₑ = {4} m/s. Que massa de propelente o 2º estágio precisa levar?", Ex.N(dt), Ex.N(p), Ex.N(d1), Ex.N(s2), Ex.N(ve2)),
                    mp2, "kg", L.F("Δv₂ = {0} − {1} = {2} m/s; R = e^(Δv₂/vₑ) = {3}; m_f = {4} + {5} = {6} kg; propelente = m_f·(R − 1) = {7} kg.", Ex.N(dt), Ex.N(d1), Ex.N(d2), Ex.N(R, 3), Ex.N(p), Ex.N(s2), Ex.N(p + s2), Ex.N(mp2)));
            }
        }
    }
}

[Exercises("rkt-08-tsiolkovsky")]
public sealed class Rkt08 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double ve = Ex.Step(r, 1500, 4500, 100), mf = Ex.Step(r, 100, 1000, 50), R = Ex.Step(r, 1.5, 6, 0.5), dv = ve * Math.Log(R);
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um foguete com velocidade de exaustão {0} m/s tem massa inicial {1} kg e final {2} kg. Qual o Δv?", Ex.N(ve), Ex.N(mf * R), Ex.N(mf)), dv, "m/s", L.F("Δv = vₑ·ln(m₀/m_f) = {0}·ln({1}) = {2} m/s.", Ex.N(ve), Ex.N(R), Ex.N(dv)));
                    case 1: return Ex.Num(L.F("Um foguete tem vₑ = {0} m/s e razão de massa {1}. Qual o Δv?", Ex.N(ve), Ex.N(R)), dv, "m/s", L.F("Δv = vₑ·ln(R) = {0}·ln({1}) = {2} m/s.", Ex.N(ve), Ex.N(R), Ex.N(dv)));
                    default: return Ex.Num(L.F("Um foguete precisa de Δv = {0} m/s com velocidade de exaustão de {1} m/s. Qual a razão de massa necessária?", Ex.N(dv), Ex.N(ve)), R, "", L.F("R = e^(Δv/vₑ) = e^({0}/{1}) = {2}.", Ex.N(dv), Ex.N(ve), Ex.N(R)));
                }
            }
            case Difficulty.Medium:
            {
                double isp = Ex.Step(r, 150, 450, 10), ve = isp * G, R = Ex.Step(r, 1.5, 5, 0.25), dv = ve * Math.Log(R);
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Qual a velocidade de exaustão de um motor com impulso específico de {0} s? (g₀ = 9,8 m/s²)", Ex.N(isp)), ve, "m/s", L.F("vₑ = Isp·g₀ = {0}·9,8 = {1} m/s.", Ex.N(isp), Ex.N(ve)));
                    case 1: return Ex.Num(L.F("Um motor tem Isp = {0} s e o foguete tem razão de massa {1}. Qual o Δv? (g₀ = 9,8 m/s²)", Ex.N(isp), Ex.N(R)), dv, "m/s", L.F("vₑ = {0}·9,8 = {1} m/s; Δv = vₑ·ln(R) = {1}·ln({2}) = {3} m/s.", Ex.N(isp), Ex.N(ve), Ex.N(R), Ex.N(dv)));
                    default: return Ex.Num(L.F("Para um Δv de {0} m/s com um motor de Isp = {1} s, qual a razão de massa necessária? (g₀ = 9,8 m/s²)", Ex.N(dv), Ex.N(isp)), R, "", L.F("vₑ = {0}·9,8 = {1} m/s; R = e^(Δv/vₑ) = e^({2}/{1}) = {3}.", Ex.N(isp), Ex.N(ve), Ex.N(dv), Ex.N(R)));
                }
            }
            case Difficulty.Hard:
            {
                double isp = Ex.Step(r, 200, 450, 10), ve = isp * G, mf = Ex.Step(r, 50, 800, 10), R = Ex.Step(r, 1.8, 6, 0.1), dv = ve * Math.Log(R);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um satélite com estrutura e carga totalizando {0} kg precisa de Δv = {1} m/s com motor de Isp = {2} s. Que massa de propelente ele deve carregar? (g₀ = 9,8 m/s²)", Ex.N(mf), Ex.N(dv), Ex.N(isp)), mf * (R - 1), "kg", L.F("vₑ = {0} m/s; R = e^(Δv/vₑ) = {1}; propelente = m_f·(R − 1) = {2}·{3} = {4} kg.", Ex.N(ve), Ex.N(R, 3), Ex.N(mf), Ex.N(R - 1, 3), Ex.N(mf * (R - 1))));
                double mp = mf * (R - 1);
                return Ex.Num(L.F("Uma sonda tem {0} kg ao todo, dos quais {1} kg são propelente (motor com Isp = {2} s). Qual o Δv máximo? (g₀ = 9,8 m/s²)", Ex.N(mf + mp), Ex.N(mp), Ex.N(isp)), dv, "m/s", L.F("m_f = {0} − {1} = {2} kg; Δv = {3}·ln({0}/{2}) = {4} m/s.", Ex.N(mf + mp), Ex.N(mp), Ex.N(mf), Ex.N(ve), Ex.N(dv)));
            }
            default:
            {
                // Payload of a single stage with structural fraction eps of the (propellant + structure) mass.
                double ve = Ex.Step(r, 2800, 4200, 100), R = Ex.Step(r, 2, 4, 0.25), eps = Ex.Step(r, 0.05, 0.2, 0.01);
                if (eps * R >= 0.85) eps = Math.Round(0.8 / R, 2);
                double dv = ve * Math.Log(R), Tk = Ex.Step(r, 1000, 5000, 100), pay = Tk * (1 - eps * R) / (R - 1);
                return Ex.Num(L.F("Um estágio único tem tanques (propelente + estrutura) com {0} kg, sendo {1}% disso estrutura. O motor tem vₑ = {2} m/s e precisa dar Δv = {3} m/s. Que carga útil ele consegue levar?", Ex.N(Tk), Ex.N(eps * 100), Ex.N(ve), Ex.N(dv)),
                    pay, "kg", L.F("R = e^(Δv/vₑ) = {0}. Com carga P: R = (P + T)/(P + ε·T) → P = T·(1 − ε·R)/(R − 1) = {1}·(1 − {2}·{0})/({0} − 1) = {3} kg.", Ex.N(R, 3), Ex.N(Tk), Ex.N(eps, 3), Ex.N(pay)));
            }
        }
    }
}

[Exercises("rkt-09-orbita")]
public sealed class Rkt09 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double Mu = Pk.Mu, Re = Pk.Re;
        double h = Ex.Step(r, 200, 2000, 50), rr = Re + h * 1000, vc = Math.Sqrt(Mu / rr), T = 2 * Math.PI * Math.Sqrt(rr * rr * rr / Mu);
        string consts = L.T("(μ da Terra = 3,986·10¹⁴ m³/s²; raio da Terra = 6371 km)");
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Qual a velocidade de um satélite em órbita circular a {0} km de altitude?", Ex.N(h)) + " " + consts, vc / 1000, "km/s", L.F("r = 6371 + {0} = {1} km; v = √(μ/r) = {2} km/s.", Ex.N(h), Ex.N(rr / 1000), Ex.N(vc / 1000, 3)));
                return Ex.Num(L.F("A que altitude uma órbita circular tem velocidade de {0} km/s?", Ex.N(vc / 1000, 3)) + " " + consts, h, "km", L.F("r = μ / v² = {0} km; altitude = r − 6371 = {1} km.", Ex.N(Mu / (vc * vc) / 1000), Ex.N(h)), 3);
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Qual o período de uma órbita circular a {0} km de altitude?", Ex.N(h)) + " " + consts, T / 60, "min", L.F("r = {0} km; T = 2π·√(r³/μ) = {1} s = {2} min.", Ex.N(rr / 1000), Ex.N(T), Ex.N(T / 60)));
                return Ex.Num(L.F("Um satélite em órbita circular a {0} km de altitude dá quantas voltas por dia (24 h)?", Ex.N(h)) + " " + consts, 86400 / T, "", L.F("T = 2π·√(r³/μ) = {0} s; voltas = 86400 / T = {1}.", Ex.N(T), Ex.N(86400 / T)));
            case Difficulty.Hard:
            {
                double h2 = Ex.Step(r, 1000, 36000, 500), r1 = Re + 300e3, r2 = Re + h2 * 1000;
                double v1 = Math.Sqrt(Mu / r1), vp = Math.Sqrt(Mu / r1 * 2 * r2 / (r1 + r2)), va = Math.Sqrt(Mu / r2 * 2 * r1 / (r1 + r2)), v2 = Math.Sqrt(Mu / r2);
                string intro = L.F("Um satélite em órbita circular a 300 km de altitude será levado por uma transferência de Hohmann para uma órbita circular a {0} km.", Ex.N(h2));
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(intro + " " + L.T("Qual o Δv da primeira queima (no perigeu)?") + " " + consts, vp - v1, "m/s", L.F("v₁ = √(μ/r₁) = {0} m/s; v_perigeu = √(μ/r₁·2r₂/(r₁+r₂)) = {1} m/s; Δv₁ = {2} m/s.", Ex.N(v1), Ex.N(vp), Ex.N(vp - v1)));
                    case 1: return Ex.Num(intro + " " + L.T("Qual o Δv da segunda queima (no apogeu), que circulariza a órbita?") + " " + consts, v2 - va, "m/s", L.F("v₂ = √(μ/r₂) = {0} m/s; v_apogeu = √(μ/r₂·2r₁/(r₁+r₂)) = {1} m/s; Δv₂ = {2} m/s.", Ex.N(v2), Ex.N(va), Ex.N(v2 - va)));
                    default: return Ex.Num(intro + " " + L.T("Qual o Δv total das duas queimas?") + " " + consts, vp - v1 + v2 - va, "m/s", L.F("Δv₁ = {0} m/s; Δv₂ = {1} m/s; total = {2} m/s.", Ex.N(vp - v1), Ex.N(v2 - va), Ex.N(vp - v1 + v2 - va)));
                }
            }
            default:
            {
                double h2 = Ex.Step(r, 5000, 36000, 500), r1 = Re + h * 1000, r2 = Re + h2 * 1000, a = (r1 + r2) / 2, tt = Math.PI * Math.Sqrt(a * a * a / Mu);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Quanto tempo dura uma transferência de Hohmann de uma órbita circular a {0} km para outra a {1} km de altitude? (meia elipse)", Ex.N(h), Ex.N(h2)) + " " + consts, tt / 3600, "h", L.F("a = (r₁ + r₂)/2 = {0} km; t = π·√(a³/μ) = {1} s = {2} h.", Ex.N(a / 1000), Ex.N(tt), Ex.N(tt / 3600, 3)));
                double e = (r2 - r1) / (r1 + r2);
                return Ex.Num(L.F("A órbita de transferência de Hohmann entre circulares de {0} km e {1} km de altitude tem qual excentricidade?", Ex.N(h), Ex.N(h2)) + " " + consts, e, "", L.F("r₁ = {0} km (perigeu), r₂ = {1} km (apogeu); e = (r₂ − r₁)/(r₂ + r₁) = {2}.", Ex.N(r1 / 1000), Ex.N(r2 / 1000), Ex.N(e, 3)), 3);
            }
        }
    }
}

[Exercises("rkt-10-missao-sondagem")]
public sealed class Rkt10 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double isp = Ex.Step(r, 180, 280, 10), ve = isp * G, m0 = Ex.Step(r, 50, 400, 10), R = Ex.Step(r, 1.6, 3, 0.1), mf = m0 / R, tb = Ex.Step(r, 5, 30, 1);
        double lnR = Math.Log(R), dv = ve * lnR, vbo = dv - G * tb, hb = ve * tb * (1 - lnR / (R - 1)) - 0.5 * G * tb * tb, hc = vbo * vbo / (2 * G);
        string intro = L.F("Foguete de sondagem: massa inicial {0} kg, massa final {1} kg, motor com Isp = {2} s queimando por {3} s, subindo na vertical (g = 9,8 m/s², sem arrasto).", Ex.N(m0), Ex.N(mf), Ex.N(isp), Ex.N(tb));
        switch (d)
        {
            case Difficulty.Easy:
                return Ex.Num(intro + " " + L.T("Qual o Δv ideal (equação de Tsiolkovsky)?"), dv, "m/s", L.F("vₑ = Isp·g₀ = {0} m/s; Δv = vₑ·ln(m₀/m_f) = {0}·ln({1}) = {2} m/s.", Ex.N(ve), Ex.N(m0 / mf, 3), Ex.N(dv)));
            case Difficulty.Medium:
                return Ex.Num(intro + " " + L.T("Qual a velocidade no fim da queima, descontando a perda por gravidade (g·t_queima)?"), vbo, "m/s", L.F("Δv = {0} m/s; perda = g·t = 9,8·{1} = {2} m/s; v = {3} m/s.", Ex.N(dv), Ex.N(tb), Ex.N(G * tb), Ex.N(vbo)));
            case Difficulty.Hard:
                return Ex.Num(intro + " " + L.T("Qual a altitude no fim da queima? (fórmula: h = vₑ·t·[1 − ln R/(R − 1)] − ½·g·t²)"), hb, "m", L.F("R = {0}; ln R = {1}; h = {2}·{3}·[1 − {1}/({0} − 1)] − 0,5·9,8·{3}² = {4} m.", Ex.N(R, 3), Ex.N(lnR, 4), Ex.N(ve), Ex.N(tb), Ex.N(hb)));
            default:
                return Ex.Num(intro + " " + L.T("Qual o apogeu total (altitude no fim da queima + subida por inércia)? Use h_queima = vₑ·t·[1 − ln R/(R − 1)] − ½·g·t²."), hb + hc, "m", L.F("h_queima = {0} m; v_fim = ve·ln R − g·t = {1} m/s; subida = v²/(2g) = {2} m; apogeu = {3} m.", Ex.N(hb), Ex.N(vbo), Ex.N(hc), Ex.N(hb + hc)));
        }
    }
}
