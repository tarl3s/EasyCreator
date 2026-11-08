namespace EasyCreator.Exercises;

/// <summary>Rockets 11-15: water rocket optimum, over-stable rockets, nozzles, propellants, optimal staging.</summary>
[Exercises("rkt-11-agua-otima")]
public sealed class Rkt11 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double V = Ex.Pick(r, 1.0, 1.5, 2.0, 3.0), f = Ex.Step(r, 0.2, 0.5, 0.05), Vw = V * f;
        switch (d)
        {
            case Difficulty.Easy:
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Uma garrafa de {0} L é preenchida com {1}% de água. Quantos mL de água?", Ex.N(V), Ex.N(f * 100)), Vw * 1000, "mL", L.F("{0} L · {1}% = {2} L = {3} mL.", Ex.N(V), Ex.N(f * 100), Ex.N(Vw, 3), Ex.N(Vw * 1000)));
                    case 1: return Ex.Num(L.F("Uma garrafa de {0} L tem {1} mL de água. Quanto de ar (em mL) sobra para comprimir?", Ex.N(V), Ex.N(Vw * 1000)), (V - Vw) * 1000, "mL", L.F("Ar = {0} − {1} = {2} mL.", Ex.N(V * 1000), Ex.N(Vw * 1000), Ex.N((V - Vw) * 1000)));
                    default: return Ex.Num(L.F("Uma garrafa de {0} L tem {1}% de água (1 L de água = 1 kg). Qual a massa de água?", Ex.N(V), Ex.N(f * 100)), Vw, "kg", L.F("m = {0} L · 1 kg/L = {1} kg.", Ex.N(Vw, 3), Ex.N(Vw, 3)));
                }
            case Difficulty.Medium:
            {
                double pg = Ex.Step(r, 300, 600, 50), p1 = pg + 101.325, va1 = V - Vw, dV = Vw * Ex.Step(r, 0.3, 0.6, 0.1), va2 = va1 + dV, p2 = p1 * va1 / va2;
                return Ex.Num(L.F("Uma garrafa de {0} L com {1}% de água é pressurizada a {2} kPa manométricos (pressão atmosférica 101,3 kPa). Depois de sair {3} mL de água, qual a pressão manométrica do ar? (temperatura constante: P·V = constante, com pressão ABSOLUTA)", Ex.N(V), Ex.N(f * 100), Ex.N(pg), Ex.N(dV * 1000)),
                    p2 - 101.325, "kPa", L.F("P₁ = {0} + 101,3 = {1} kPa; V₁ = {2} L; V₂ = {2} + {3} = {4} L; P₂ = P₁·V₁/V₂ = {5} kPa absolutos → {6} kPa manométricos.", Ex.N(pg), Ex.N(p1), Ex.N(va1, 3), Ex.N(dV, 3), Ex.N(va2, 3), Ex.N(p2), Ex.N(p2 - 101.325)));
            }
            case Difficulty.Hard:
            {
                double mb = Ex.Step(r, 40, 120, 5) / 1000, ve = Ex.Step(r, 12, 30, 1), mw = Vw, dv = ve * Math.Log((mb + mw) / mb), h = dv * dv / (2 * G);
                string intro = L.F("Um foguete de água tem garrafa vazia de {0} g e leva {1} kg de água, expulsa a uma velocidade média de {2} m/s.", Ex.N(mb * 1000), Ex.N(mw, 3), Ex.N(ve));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Qual o Δv ideal?"), dv, "m/s", L.F("Δv = vₑ·ln((m_b + m_w)/m_b) = {0}·ln({1}/{2}) = {3} m/s.", Ex.N(ve), Ex.N(mb + mw, 3), Ex.N(mb, 3), Ex.N(dv)));
                return Ex.Num(intro + " " + L.T("Sem arrasto e com expulsão instantânea, qual a altura máxima? (g = 9,8 m/s²)"), h, "m", L.F("Δv = {0} m/s; h = Δv²/(2g) = {0}²/19,6 = {1} m.", Ex.N(dv), Ex.N(h)));
            }
            default:
            {
                double mb = Ex.Step(r, 50, 120, 5) / 1000, ve = Ex.Step(r, 14, 28, 1), dvt = Math.Round(ve * Math.Log(1 + Vw / mb)), mw = mb * (Math.Exp(dvt / ve) - 1), pct = mw / V * 100;
                return Ex.Num(L.F("Uma garrafa de {0} L pesa {1} g vazia. Para chegar a Δv = {2} m/s com água saindo a {3} m/s, que porcentagem do volume da garrafa deve ser água? (1 L de água = 1 kg)", Ex.N(V), Ex.N(mb * 1000), Ex.N(dvt), Ex.N(ve)),
                    pct, "%", L.F("m_w = m_b·(e^(Δv/vₑ) − 1) = {0}·(e^({1}/{2}) − 1) = {3} kg = {4} L → {4}/{5} = {6}% da garrafa.", Ex.N(mb, 3), Ex.N(dvt), Ex.N(ve), Ex.N(mw, 3), Ex.N(mw, 3), Ex.N(V), Ex.N(pct)));
            }
        }
    }
}

[Exercises("rkt-12-estavel-demais")]
public sealed class Rkt12 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double v = Ex.Step(r, 15, 80, 5), w = Ex.Step(r, 2, 12, 1), al = Math.Atan(w / v), deg = al * 180 / Math.PI;
        switch (d)
        {
            case Difficulty.Easy:
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um foguete sobe a {0} m/s e sofre um vento lateral de {1} m/s. Qual o ângulo de ataque (tan α = vento / velocidade)?", Ex.N(v), Ex.N(w)), deg, "°", L.F("α = arctan({0}/{1}) = {2}°.", Ex.N(w), Ex.N(v), Ex.N(deg)));
                    case 1: return Ex.Num(L.F("Um foguete sobe a {0} m/s e o vento lateral cria um ângulo de ataque de {1}°. Qual a velocidade do vento? ", Ex.N(v), Ex.N(deg, 2)), w, "m/s", L.F("w = v·tan α = {0}·tan({1}°) = {2} m/s.", Ex.N(v), Ex.N(deg, 2), Ex.N(w)));
                    default: return Ex.Choice(r, L.T("Um foguete com margem estática muito grande (por exemplo 5 calibres) voa com vento lateral. O que tende a acontecer?"),
                        L.T("Vira o nariz para o vento (cata-vento) e se afasta da vertical."),
                        new[] { L.T("Fica mais reto e alcança mais altura."), L.T("Cai de lado por ser instável."), L.T("Nada: o vento não afeta foguetes.") },
                        L.T("Quanto maior a estabilidade, mais forte o torque que alinha o foguete com o vento relativo; com vento lateral isso inclina a trajetória (cata-vento) e custa altitude."));
                }
            case Difficulty.Medium:
            {
                double dia = Ex.Step(r, 4, 10, 0.5), Cd = Ex.Step(r, 0.35, 0.6, 0.05), nf = Ex.Pick(r, 3.0, 4.0), af = Ex.Step(r, 10, 40, 5), cdf = 0.02 * Ex.Step(r, 1, 3, 0.5);
                double A = Pk.Area(dia / 100), body = Cd * A, fins = nf * 2 * (af / 10000) * cdf * 10; // see explanation: each fin has two faces
                // Keep it simple: a fin set adds CdA_f = n · Cdf · Af (Af = area of one fin).
                fins = nf * cdf * (af / 10000);
                double q = 0.5 * Pk.Rho * v * v, F = q * (body + fins), pct = fins / body * 100;
                string intro = L.F("Um foguete de {0} cm de diâmetro (Cd do corpo = {1}) tem {2} aletas de {3} cm² cada, com Cd = {4} cada uma. Voa a {5} m/s (ρ = 1,2 kg/m³).", Ex.N(dia), Ex.N(Cd), Ex.N(nf), Ex.N(af), Ex.N(cdf, 3), Ex.N(v));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Qual o arrasto total (corpo + aletas)?"), F, "N", L.F("CdA corpo = {0} m²; CdA aletas = n·Cd·A_f = {1} m²; F = ½·ρ·v²·(soma) = {2} N.", Ex.N(body, 6), Ex.N(fins, 6), Ex.N(F, 3)));
                return Ex.Num(intro + " " + L.T("Em quantos por cento as aletas aumentam o arrasto do corpo sozinho?"), pct, "%", L.F("O arrasto é proporcional ao CdA: aletas / corpo = {0} / {1} = {2}%.", Ex.N(fins, 6), Ex.N(body, 6), Ex.N(pct)));
            }
            case Difficulty.Hard:
            {
                double dia = Ex.Step(r, 4, 10, 0.5), A = Pk.Area(dia / 100), cna = Ex.Step(r, 8, 16, 1), mg = Ex.Step(r, 1.5, 5, 0.5), q = 0.5 * Pk.Rho * v * v;
                double N = q * A * cna * al, arm = mg * dia / 100, M = N * arm;
                return Ex.Num(L.F("Um foguete de {0} cm de diâmetro voa a {1} m/s com vento lateral de {2} m/s. Sua força normal vale N = q·A·CNα·α (q = ½ρv², CNα = {3} por rad, ρ = 1,2 kg/m³) e a margem estática é {4} calibres. Qual o torque restaurador (N × distância CP–CG)?", Ex.N(dia), Ex.N(v), Ex.N(w), Ex.N(cna), Ex.N(mg)),
                    M, "N·m", L.F("α = arctan({0}/{1}) = {2} rad; q = {3} Pa; N = q·A·CNα·α = {4} N; braço = {5}·d = {6} m; torque = {7} N·m.", Ex.N(w), Ex.N(v), Ex.N(al, 4), Ex.N(q), Ex.N(N, 3), Ex.N(mg), Ex.N(arm, 3), Ex.N(M, 3)));
            }
            default:
            {
                double v0 = Ex.Step(r, 60, 150, 5), th = Ex.Step(r, 5, 25, 1), rad = th * Math.PI / 180, hv = v0 * v0 / (2 * G), h = hv * Math.Cos(rad) * Math.Cos(rad), x = v0 * v0 * Math.Sin(rad) * Math.Cos(rad) / G;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um foguete super estável vira para o vento e sai da queima a {0} m/s inclinado {1}° em relação à vertical. Sem arrasto (g = 9,8 m/s²), quanto de altura ele perde em relação a um voo perfeitamente vertical?", Ex.N(v0), Ex.N(th)), hv - h, "m", L.F("Vertical: v²/(2g) = {0} m. Inclinado: só vem de v·cos θ → v²cos²θ/(2g) = {1} m. Perda = {2} m.", Ex.N(hv), Ex.N(h), Ex.N(hv - h)), 3);
                return Ex.Num(L.F("Um foguete sai da queima a {0} m/s inclinado {1}° em relação à vertical. Sem arrasto (g = 9,8 m/s²), qual a distância horizontal percorrida até o apogeu?", Ex.N(v0), Ex.N(th)), x, "m", L.F("t = v·cos θ / g = {0} s; vₓ = v·sen θ; x = vₓ·t = v²·sen θ·cos θ / g = {1} m.", Ex.N(v0 * Math.Cos(rad) / G), Ex.N(x)));
            }
        }
    }
}

[Exercises("rkt-13-bocal")]
public sealed class Rkt13 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                double dn = Ex.Step(r, 6, 24, 1), v = Ex.Step(r, 8, 30, 1), A = Pk.Area(dn / 1000), md = Pk.RhoW * A * v;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Água sai por um bocal de {0} mm de diâmetro a {1} m/s. Qual a vazão de massa? (ρ = 1000 kg/m³)", Ex.N(dn), Ex.N(v)), md, "kg/s", L.F("A = π·d²/4 = {0} m²; ṁ = ρ·A·v = 1000·{0}·{1} = {2} kg/s.", Ex.N(A, 6), Ex.N(v), Ex.N(md, 3)));
                return Ex.Num(L.F("A água sai de um bocal de {0} mm de diâmetro a {1} m/s. Qual o empuxo (F = ṁ·v, ρ = 1000 kg/m³)?", Ex.N(dn), Ex.N(v)), md * v, "N", L.F("ṁ = ρ·A·v = {0} kg/s; F = ṁ·v = {0}·{1} = {2} N.", Ex.N(md, 3), Ex.N(v), Ex.N(md * v)));
            }
            case Difficulty.Medium:
            {
                double d1 = Ex.Step(r, 20, 40, 1), d2 = Ex.Step(r, 6, 18, 1), v1 = Ex.Step(r, 0.5, 4, 0.5), v2 = v1 * d1 * d1 / (d2 * d2);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("A água corre a {0} m/s num gargalo de {1} mm de diâmetro e é acelerada num bocal de {2} mm. Qual a velocidade na saída (conservação da vazão)?", Ex.N(v1), Ex.N(d1), Ex.N(d2)), v2, "m/s", L.F("A₁·v₁ = A₂·v₂ → v₂ = v₁·(d₁/d₂)² = {0}·({1}/{2})² = {3} m/s.", Ex.N(v1), Ex.N(d1), Ex.N(d2), Ex.N(v2)));
                double dp = Ex.Step(r, 100, 500, 25), ve = Math.Sqrt(2 * dp * 1000 / Pk.RhoW);
                return Ex.Num(L.F("A água num foguete está sob {0} kPa de pressão manométrica. Com que velocidade ela sai do bocal? (ρ = 1000 kg/m³, Bernoulli)", Ex.N(dp)), ve, "m/s", L.F("v = √(2·ΔP/ρ) = √(2·{0}/1000) = {1} m/s.", Ex.N(dp * 1000), Ex.N(ve)));
            }
            case Difficulty.Hard:
            {
                double md = Ex.Step(r, 0.5, 3, 0.1), ve = Ex.Step(r, 1800, 2600, 50), pe = Ex.Step(r, 40, 100, 5), pa = Ex.Step(r, 20, 101, 1), ae = Ex.Step(r, 20, 100, 5);
                double F = md * ve + (pe - pa) * 1000 * ae / 10000;
                return Ex.Num(L.F("Um motor tem vazão de gás de {0} kg/s, velocidade de saída {1} m/s, pressão na saída do bocal {2} kPa, pressão ambiente {3} kPa e área de saída {4} cm². Qual o empuxo? (F = ṁ·vₑ + (pₑ − pₐ)·Aₑ)", Ex.N(md), Ex.N(ve), Ex.N(pe), Ex.N(pa), Ex.N(ae)),
                    F, "N", L.F("Termo de momento: {0}·{1} = {2} N. Termo de pressão: ({3} − {4}) kPa · {5} cm² = {6} N. F = {7} N.", Ex.N(md), Ex.N(ve), Ex.N(md * ve), Ex.N(pe), Ex.N(pa), Ex.N(ae), Ex.N((pe - pa) * ae / 10, 3), Ex.N(F)), 2);
            }
            default:
            {
                double gam = Ex.Pick(r, 1.15, 1.2, 1.25, 1.3), mol = Ex.Step(r, 18, 28, 1), tc = Ex.Step(r, 2800, 3500, 100), pc = Ex.Step(r, 3, 7, 0.5), pe = Ex.Step(r, 0.05, 0.1, 0.01);
                double Rs = 8314.0 / mol, ve = Math.Sqrt(2 * gam / (gam - 1) * Rs * tc * (1 - Math.Pow(pe / pc, (gam - 1) / gam)));
                return Ex.Num(L.F("Gases de combustão com γ = {0} e massa molar {1} g/mol saem de uma câmara a {2} K e {3} MPa por um bocal ideal até {4} MPa. Qual a velocidade de exaustão? (vₑ = √(2γ/(γ−1)·R·T·[1 − (pₑ/p_c)^((γ−1)/γ)]), R = 8314/M J/(kg·K) com M em g/mol)", Ex.N(gam), Ex.N(mol), Ex.N(tc), Ex.N(pc), Ex.N(pe)),
                    ve, "m/s", L.F("R = 8314/{0} = {1} J/(kg·K); razão de pressão (pₑ/p_c) = {2}; fator = 1 − {2}^{3} = {4}; vₑ = {5} m/s.", Ex.N(mol), Ex.N(Rs), Ex.N(pe / pc, 5), Ex.N((gam - 1) / gam, 4), Ex.N(1 - Math.Pow(pe / pc, (gam - 1) / gam), 4), Ex.N(ve)));
            }
        }
    }
}

[Exercises("rkt-14-propelentes")]
public sealed class Rkt14 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double isp = Ex.Step(r, 60, 460, 10), ve = isp * G;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um propelente tem impulso específico de {0} s. Qual a velocidade de exaustão? (g₀ = 9,8 m/s²)", Ex.N(isp)), ve, "m/s", L.F("vₑ = Isp·g₀ = {0}·9,8 = {1} m/s.", Ex.N(isp), Ex.N(ve)));
                return Ex.Num(L.F("Um motor tem velocidade de exaustão de {0} m/s. Qual o impulso específico? (g₀ = 9,8 m/s²)", Ex.N(ve)), isp, "s", L.F("Isp = vₑ / g₀ = {0} / 9,8 = {1} s.", Ex.N(ve), Ex.N(isp)));
            }
            case Difficulty.Medium:
            {
                double isp = Ex.Step(r, 200, 450, 10), F = Ex.Step(r, 500, 20000, 500), mp = Ex.Step(r, 50, 2000, 50), md = F / (isp * G);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um motor de {0} N de empuxo usa propelente com Isp = {1} s. Qual a vazão de massa de propelente? (g₀ = 9,8 m/s²)", Ex.N(F), Ex.N(isp)), md, "kg/s", L.F("ṁ = F / (Isp·g₀) = {0} / ({1}·9,8) = {2} kg/s.", Ex.N(F), Ex.N(isp), Ex.N(md, 3)));
                return Ex.Num(L.F("Um motor de {0} N e Isp = {1} s tem {2} kg de propelente. Quanto tempo ele pode ficar ligado? (g₀ = 9,8 m/s²)", Ex.N(F), Ex.N(isp), Ex.N(mp)), mp / md, "s", L.F("ṁ = F/(Isp·g₀) = {0} kg/s; t = m / ṁ = {1} / {0} = {2} s.", Ex.N(md, 4), Ex.N(mp), Ex.N(mp / md)));
            }
            case Difficulty.Hard:
            {
                double mp = Ex.Step(r, 100, 3000, 100), of = Ex.Step(r, 2, 2.8, 0.1), mo = mp * of / (1 + of), mc = mp - mo, vo = mo / 1141, vk = mc / 810;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um motor LOX/querosene (oxidante : combustível = {0} : 1, em massa) queima {1} kg de propelente. Que massa de oxigênio líquido é carregada?", Ex.N(of), Ex.N(mp)), mo, "kg", L.F("Fração de oxidante = O/F / (1 + O/F) = {0}/(1 + {0}) = {1}; m_ox = {1}·{2} = {3} kg.", Ex.N(of), Ex.N(of / (1 + of), 4), Ex.N(mp), Ex.N(mo)));
                return Ex.Num(L.F("Um motor LOX/querosene (oxidante : combustível = {0} : 1, em massa) queima {1} kg de propelente. Densidades: LOX 1141 kg/m³, querosene 810 kg/m³. Qual o volume total de tanques necessário, em litros?", Ex.N(of), Ex.N(mp)), (vo + vk) * 1000, "L", L.F("m_ox = {0} kg → {1} L; m_comb = {2} kg → {3} L; total = {4} L.", Ex.N(mo), Ex.N(vo * 1000), Ex.N(mc), Ex.N(vk * 1000), Ex.N((vo + vk) * 1000)));
            }
            default:
            {
                double V = Ex.Step(r, 0.5, 3, 0.25), md = Ex.Step(r, 100, 500, 25), ispA = Ex.Step(r, 380, 450, 10), rhoA = Ex.Step(r, 60, 120, 10), ispB = Ex.Step(r, 260, 330, 10), rhoB = Ex.Step(r, 1000, 1400, 50);
                double dvA = ispA * G * Math.Log((md + rhoA * V) / md), dvB = ispB * G * Math.Log((md + rhoB * V) / md);
                return Ex.Num(L.F("Dois propelentes enchem o mesmo tanque de {0} m³ de um estágio com massa seca de {1} kg. A: baixa densidade ({2} kg/m³) e Isp = {3} s. B: densidade {4} kg/m³ e Isp = {5} s. Qual a diferença (em módulo) entre os Δv dos dois? (g₀ = 9,8 m/s²)", Ex.N(V), Ex.N(md), Ex.N(rhoA), Ex.N(ispA), Ex.N(rhoB), Ex.N(ispB)),
                    Math.Abs(dvA - dvB), "m/s", L.F("A: m_prop = {0} kg; Δv = {1}·9,8·ln(({2}+{0})/{2}) = {3} m/s. B: m_prop = {4} kg; Δv = {5} m/s. Diferença = {6} m/s.", Ex.N(rhoA * V), Ex.N(ispA), Ex.N(md), Ex.N(dvA), Ex.N(rhoB * V), Ex.N(dvB), Ex.N(Math.Abs(dvA - dvB))));
            }
        }
    }
}

[Exercises("rkt-15-estagios-otimos")]
public sealed class Rkt15 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double eps = Ex.Step(r, 0.06, 0.15, 0.01), lam = Ex.Step(r, 0.02, 0.2, 0.01), ve = Ex.Step(r, 2800, 4000, 100);
        double R = (1 + lam) / (eps + lam), dv = ve * Math.Log(R);
        switch (d)
        {
            case Difficulty.Easy:
            {
                double ms = Ex.Step(r, 5, 30, 1), mp = Ex.Step(r, 100, 400, 10), e = ms / (ms + mp);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um estágio tem {0} t de estrutura e {1} t de propelente. Qual a fração estrutural ε = m_estrutura / (m_estrutura + m_propelente)?", Ex.N(ms), Ex.N(mp)), e, "", L.F("ε = {0} / ({0} + {1}) = {2}.", Ex.N(ms), Ex.N(mp), Ex.N(e, 4)));
                double tank = ms + mp;
                return Ex.Num(L.F("Um estágio tem fração estrutural ε = {0} e tanques (estrutura + propelente) de {1} t. Quantas toneladas são de propelente?", Ex.N(e, 4), Ex.N(tank)), mp, "t", L.F("Propelente = (1 − ε)·total = {0}·{1} = {2} t.", Ex.N(1 - e, 4), Ex.N(tank), Ex.N(mp)));
            }
            case Difficulty.Medium:
                return Ex.Num(L.F("Um estágio tem ε = {0} e a carga que ele leva (os estágios seguintes + a carga útil) pesa {1} da massa de (estrutura + propelente) dele (razão de carga λ = {1}). Qual a razão de massa R = (1 + λ)/(ε + λ)?", Ex.N(eps), Ex.N(lam)),
                    R, "", L.F("R = (1 + {0}) / ({1} + {0}) = {2}.", Ex.N(lam), Ex.N(eps), Ex.N(R, 4)));
            case Difficulty.Hard:
            {
                int n = Ex.Pick(r, 2, 3);
                double tot = n * dv;
                return Ex.Num(L.F("Um foguete de {0} estágios idênticos tem ε = {1}, razão de carga λ = {2} em cada estágio e vₑ = {3} m/s. Qual o Δv total? (R = (1 + λ)/(ε + λ); Δv = N·vₑ·ln R)", Ex.N(n), Ex.N(eps), Ex.N(lam), Ex.N(ve)),
                    tot, "m/s", L.F("R = {0}; Δv por estágio = {1}·ln({0}) = {2} m/s; total = {3}·{2} = {4} m/s.", Ex.N(R, 4), Ex.N(ve), Ex.N(dv), Ex.N(n), Ex.N(tot)));
            }
            default:
            {
                int n = Ex.Pick(r, 2, 3, 4);
                double e = Ex.Step(r, 0.07, 0.12, 0.01), dvt = Ex.Step(r, 8000, 9500, 100), Rn = Math.Exp(dvt / (n * ve));
                if (e * Rn >= 0.95) { ve += 500; Rn = Math.Exp(dvt / (n * ve)); }
                double l = (1 - Rn * e) / (Rn - 1), frac = Math.Pow(1 / (1 + l), n) * 100;
                return Ex.Num(L.F("Um lançador com {0} estágios idênticos (ε = {1}, vₑ = {2} m/s) precisa de Δv total = {3} m/s. Que porcentagem da massa de decolagem pode ser carga útil? (Cada estágio: R = (1 + λ)/(ε + λ); massa total = (1 + λ)^N · carga)", Ex.N(n), Ex.N(e), Ex.N(ve), Ex.N(dvt)),
                    frac, "%", L.F("R = e^(Δv/(N·vₑ)) = {0}; λ = (1 − R·ε)/(R − 1) = {1}; carga/massa = 1/(1 + λ)^N = {2}%.", Ex.N(Rn, 4), Ex.N(l, 4), Ex.N(frac, 3)), 3);
            }
        }
    }
}
