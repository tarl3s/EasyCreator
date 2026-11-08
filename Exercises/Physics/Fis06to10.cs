namespace EasyCreator.Exercises;

/// <summary>Oscillations: pendulum, mass-spring, springs in series/parallel, accelerating elevator.</summary>
[Exercises("fis-06-pendulo")]
public sealed class Fis06Pendulo : IExerciseGenerator
{
    const double G = P.G;

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                double len = Ex.Step(r, 0.1, 4, 0.1), T = 2 * Math.PI * Math.Sqrt(len / G);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Qual o período de um pêndulo simples de {0} m de comprimento? (g = 9,8 m/s²)", P.N(len)), T, "s", $"T = 2π √(L/g) = 2π √({P.N(len)} / {P.N(G)}) = {P.N(T)} s.");
                double f = 1 / T;
                return Ex.Num(L.F("Um pêndulo simples de {0} m oscila na Terra (g = 9,8 m/s²). Qual a sua frequência?", P.N(len)), f, "Hz", $"T = 2π √(L/g) = {P.N(T)} s; f = 1 / T = {P.N(f)} Hz.");
            }
            case Difficulty.Medium:
            {
                double T = Ex.Step(r, 0.5, 3, 0.1);
                if (r.Next(2) == 0)
                {
                    double len = G * T * T / (4 * Math.PI * Math.PI);
                    return Ex.Num(L.F("Que comprimento deve ter um pêndulo simples para ter período de {0} s? (g = 9,8 m/s²)", P.N(T)), len, "m", $"T = 2π √(L/g) → L = g T² / 4π² = {P.N(G)}·{P.N(T)}² / 4π² = {P.N(len, 3)} m.");
                }
                double m = Ex.Step(r, 0.1, 5, 0.1), k = Ex.Step(r, 10, 500, 10), Tm = 2 * Math.PI * Math.Sqrt(m / k);
                return Ex.Num(L.F("Um bloco de {0} kg preso a uma mola de {1} N/m oscila na horizontal, sem atrito. Qual o período?", P.N(m), P.N(k)), Tm, "s", $"T = 2π √(m/k) = 2π √({P.N(m)} / {P.N(k)}) = {P.N(Tm)} s.");
            }
            case Difficulty.Hard:
            {
                var (place, g) = Ex.Pick(r, (L.T("na Lua"), 1.62), (L.T("em Marte"), 3.71), (L.T("em Júpiter"), 24.8), (L.T("em Vênus"), 8.87));
                double len = Ex.Step(r, 0.5, 3, 0.1);
                if (r.Next(2) == 0)
                {
                    double T = 2 * Math.PI * Math.Sqrt(len / g);
                    return Ex.Num(L.F("Um pêndulo de {0} m é levado {1}, onde g = {2} m/s². Qual o seu período?", P.N(len), place, P.N(g)), T, "s", $"T = 2π √(L/g) = 2π √({P.N(len)} / {P.N(g)}) = {P.N(T)} s.");
                }
                double Ts = Ex.Step(r, 1, 4, 0.5), gl = 4 * Math.PI * Math.PI * len / (Ts * Ts);
                return Ex.Num(L.F("Um astronauta mede o período de um pêndulo de {0} m e encontra {1} s. Qual é a aceleração da gravidade local?", P.N(len), P.N(Ts)), gl, "m/s²", $"T = 2π √(L/g) → g = 4π² L / T² = 4π²·{P.N(len)} / {P.N(Ts)}² = {P.N(gl)} m/s².");
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    double len = Ex.Step(r, 0.5, 2, 0.1), a = Ex.Step(r, 1, 6, 0.5), T = 2 * Math.PI * Math.Sqrt(len / (G + a));
                    return Ex.Num(L.F("Um pêndulo de {0} m está dentro de um elevador que sobe com aceleração de {1} m/s². Qual o período das oscilações? (g = 9,8 m/s²)", P.N(len), P.N(a)),
                        T, "s", $"Na gravidade aparente g' = g + a = {P.N(G + a)} m/s²: T = 2π √(L/g') = 2π √({P.N(len)} / {P.N(G + a)}) = {P.N(T)} s.");
                }
                double k1 = Ex.Step(r, 20, 200, 10), k2 = Ex.Step(r, 20, 200, 10), m = Ex.Step(r, 0.5, 4, 0.5);
                if (r.Next(2) == 0)
                {
                    double keq = k1 + k2, T = 2 * Math.PI * Math.Sqrt(m / keq);
                    return Ex.Num(L.F("Um bloco de {0} kg é preso entre duas molas, uma de cada lado, de {1} N/m e {2} N/m (as duas esticam ou comprimem juntas). Qual o período de oscilação?", P.N(m), P.N(k1), P.N(k2)),
                        T, "s", $"As forças somam: k = k₁ + k₂ = {P.N(keq)} N/m; T = 2π √(m/k) = {P.N(T)} s.");
                }
                double ks = k1 * k2 / (k1 + k2), Ts = 2 * Math.PI * Math.Sqrt(m / ks);
                return Ex.Num(L.F("Um bloco de {0} kg está preso a duas molas ligadas em série, de {1} N/m e {2} N/m. Qual o período de oscilação?", P.N(m), P.N(k1), P.N(k2)),
                    Ts, "s", $"Em série: 1/k = 1/k₁ + 1/k₂ → k = {P.N(ks)} N/m; T = 2π √(m/k) = {P.N(Ts)} s.");
            }
        }
    }
}

/// <summary>Waves and optics: v = f·λ, Snell, thin lenses, two-lens systems.</summary>
[Exercises("fis-07-optica")]
public sealed class Fis07Optica : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                if (r.Next(2) == 0)
                {
                    double f = Ex.Step(r, 50, 2000, 50), lam = Ex.Step(r, 0.2, 5, 0.1), v = f * lam;
                    return Ex.Num(L.F("Uma onda tem frequência de {0} Hz e comprimento de onda de {1} m. Qual a sua velocidade?", P.N(f), P.N(lam)), v, "m/s", $"v = f·λ = {P.N(f)}·{P.N(lam)} = {P.N(v)} m/s.");
                }
                double f2 = Ex.Step(r, 100, 900, 50), l2 = 3e8 / (f2 * 1e6);
                return Ex.Num(L.F("Uma estação de rádio transmite em {0} MHz. Qual o comprimento de onda no ar? (c = 3·10⁸ m/s)", P.N(f2)), l2, "m", $"λ = c / f = 3·10⁸ / ({P.N(f2)}·10⁶) = {P.N(l2)} m.");
            }
            case Difficulty.Medium:
            {
                double n2 = Ex.Step(r, 1.3, 2.5, 0.01), th1 = Ex.Step(r, 10, 70, 5), th2 = P.Deg(Math.Asin(Math.Sin(P.Rad(th1)) / n2));
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um raio de luz no ar incide a {0}° da normal sobre um material de índice de refração {1}. Qual o ângulo de refração?", P.N(th1), P.N(n2)), th2, "°",
                        $"n₁ sen θ₁ = n₂ sen θ₂ → sen θ₂ = sen {P.N(th1)}° / {P.N(n2)} = {P.N(Math.Sin(P.Rad(th1)) / n2, 3)} → θ₂ = {P.N(th2)}°.");
                double v = 3e8 / n2;
                return Ex.Num(L.F("A luz atravessa um material de índice de refração {0}. Qual a velocidade da luz nele, em 10⁸ m/s? (c = 3·10⁸ m/s)", P.N(n2)), v / 1e8, "·10⁸ m/s", $"v = c / n = 3 / {P.N(n2)} = {P.N(v / 1e8)}·10⁸ m/s.");
            }
            case Difficulty.Hard:
            {
                double f = Ex.Step(r, 5, 30, 1), p = f * Ex.Step(r, 1.5, 4, 0.5);
                double q = p * f / (p - f), A = -q / p, h = Ex.Step(r, 1, 10, 1);
                switch (r.Next(3))
                {
                    case 0:
                        return Ex.Num(L.F("Uma lente convergente de distância focal {0} cm forma a imagem de um objeto colocado a {1} cm dela. A que distância da lente (em cm) fica a imagem?", P.N(f), P.N(p)), q, "cm",
                            $"1/f = 1/p + 1/q → 1/q = 1/{P.N(f)} − 1/{P.N(p)} → q = {P.N(q)} cm.");
                    case 1:
                        return Ex.Num(L.F("Uma lente convergente de {0} cm de foco forma a imagem de um objeto de {1} cm de altura colocado a {2} cm dela. Qual a altura da imagem (em módulo, em cm)?", P.N(f), P.N(h), P.N(p)), Math.Abs(A) * h, "cm",
                            $"q = {P.N(q)} cm; |A| = q / p = {P.N(Math.Abs(A), 3)}; altura = {P.N(h)}·{P.N(Math.Abs(A), 3)} = {P.N(Math.Abs(A) * h)} cm.");
                    default:
                        double c = P.Deg(Math.Asin(1 / Ex.Pick(r, 1.33, 1.5, 1.6, 2.0)));
                        double n = Ex.Pick(r, 1.33, 1.5, 1.6, 2.0);
                        c = P.Deg(Math.Asin(1 / n));
                        return Ex.Num(L.F("Qual o ângulo crítico para reflexão total na fronteira de um material de índice {0} com o ar?", P.N(n)), c, "°", $"sen θc = n₂ / n₁ = 1 / {P.N(n)} → θc = {P.N(c)}°.");
                }
            }
            default:
            {
                // Two thin lenses: the first image becomes the object of the second.
                double f1 = Ex.Step(r, 8, 20, 1), p1 = f1 * Ex.Step(r, 1.5, 3, 0.5), q1 = p1 * f1 / (p1 - f1);
                double f2 = Ex.Step(r, 5, 20, 1), gap = Math.Round(q1) + Ex.Step(r, 5, 20, 1);
                double p2 = gap - q1, q2 = p2 * f2 / (p2 - f2);
                if (Math.Abs(p2 - f2) < 2) { f2 = Math.Round(p2 / 2); q2 = p2 * f2 / (p2 - f2); }
                return Ex.Num(L.F("Duas lentes convergentes, de {0} cm e {1} cm de foco, estão a {2} cm uma da outra. Um objeto fica a {3} cm da primeira. A que distância da SEGUNDA lente (em cm) se forma a imagem final?", P.N(f1), P.N(f2), P.N(gap), P.N(p1)),
                    q2, "cm", $"1ª lente: q₁ = {P.N(q1)} cm. Esse ponto é o objeto da 2ª: p₂ = {P.N(gap)} − {P.N(q1)} = {P.N(p2)} cm. 1/q₂ = 1/{P.N(f2)} − 1/{P.N(p2)} → q₂ = {P.N(q2)} cm.");
            }
        }
    }
}

/// <summary>Thermodynamics: ideal gas, first law, work, Carnot.</summary>
[Exercises("fis-08-termodinamica")]
public sealed class Fis08Termo : IExerciseGenerator
{
    const double R = 8.314;

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                if (r.Next(2) == 0)
                {
                    double n = Ex.Step(r, 0.5, 5, 0.5), T = Ex.Step(r, 250, 500, 10), V = Ex.Step(r, 5, 50, 1) / 1000, p = n * R * T / V;
                    return Ex.Num(L.F("{0} mol de um gás ideal ocupam {1} litros a {2} K. Qual a pressão, em kPa? (R = 8,314 J/mol·K)", P.N(n), P.N(V * 1000, 0), P.N(T, 0)), p / 1000, "kPa", $"P = nRT / V = {P.N(n)}·8,314·{P.N(T, 0)} / {P.N(V, 3)} = {P.N(p, 0)} Pa = {P.N(p / 1000)} kPa.".Replace("8,314", P.N(R, 3)));
                }
                double Q = Ex.Step(r, 100, 1000, 50), W = Ex.Step(r, 20, 90, 10);
                return Ex.Num(L.F("Um gás recebe {0} J de calor e realiza {1} J de trabalho. Qual a variação da energia interna?", P.N(Q), P.N(W)), Q - W, "J", $"ΔU = Q − W = {P.N(Q)} − {P.N(W)} = {P.N(Q - W)} J.");
            }
            case Difficulty.Medium:
            {
                double n = Ex.Step(r, 0.5, 4, 0.5);
                switch (r.Next(3))
                {
                    case 0:
                        double p = Ex.Step(r, 1, 5, 0.5) * 1e5, dV = Ex.Step(r, 1, 10, 1) / 1000;
                        return Ex.Num(L.F("Um gás se expande a pressão constante de {0} kPa, aumentando o volume em {1} litros. Qual o trabalho realizado?", P.N(p / 1000, 0), P.N(dV * 1000, 0)), p * dV, "J", $"W = P ΔV = {P.N(p, 0)}·{P.N(dV, 3)} = {P.N(p * dV)} J.");
                    case 1:
                        double dT = Ex.Step(r, 20, 200, 10);
                        return Ex.Num(L.F("{0} mol de um gás ideal monoatômico são aquecidos em {1} K. Qual a variação de energia interna? (R = 8,314 J/mol·K)", P.N(n), P.N(dT, 0)), 1.5 * n * R * dT, "J", $"ΔU = (3/2) n R ΔT = 1,5·{P.N(n)}·{P.N(R, 3)}·{P.N(dT, 0)} = {P.N(1.5 * n * R * dT)} J.");
                    default:
                        double T = Ex.Step(r, 250, 450, 10), ratio = Ex.Pick(r, 2.0, 3, 4);
                        double W = n * R * T * Math.Log(ratio);
                        return Ex.Num(L.F("{0} mol de gás ideal a {1} K se expandem isotermicamente até {2} vezes o volume inicial. Qual o trabalho realizado? (R = 8,314 J/mol·K)", P.N(n), P.N(T, 0), P.N(ratio, 0)), W, "J", $"W = n R T ln(V₂/V₁) = {P.N(n)}·{P.N(R, 3)}·{P.N(T, 0)}·ln {P.N(ratio, 0)} = {P.N(W)} J.");
                }
            }
            case Difficulty.Hard:
            {
                if (r.Next(2) == 0)
                {
                    double Th = Ex.Step(r, 400, 800, 50), Tc = Ex.Step(r, 250, 350, 10), e = 1 - Tc / Th, Q = Ex.Step(r, 500, 5000, 100);
                    return Ex.Num(L.F("Uma máquina de Carnot opera entre {0} K e {1} K e recebe {2} J de calor da fonte quente por ciclo. Que trabalho realiza por ciclo?", P.N(Th, 0), P.N(Tc, 0), P.N(Q, 0)), e * Q, "J", $"η = 1 − Tc/Th = 1 − {P.N(Tc, 0)}/{P.N(Th, 0)} = {P.N(e, 3)}; W = η Q = {P.N(e * Q)} J.");
                }
                double T1 = Ex.Step(r, 280, 400, 10), ratio = Ex.Pick(r, 2.0, 4, 8), gamma = 5.0 / 3;
                double T2 = T1 * Math.Pow(1 / ratio, gamma - 1);
                return Ex.Num(L.F("Um gás ideal monoatômico (γ = 5/3) a {0} K se expande adiabaticamente até {1} vezes o volume inicial. Qual a temperatura final, em K?", P.N(T1, 0), P.N(ratio, 0)), T2, "K", $"T V^(γ−1) = constante → T₂ = T₁ (V₁/V₂)^(2/3) = {P.N(T1, 0)}·(1/{P.N(ratio, 0)})^{P.N(2.0 / 3, 3)} = {P.N(T2)} K.");
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    double Th = Ex.Step(r, 450, 900, 50), Tc = Ex.Step(r, 280, 350, 10), P1 = Ex.Step(r, 1, 20, 1) * 1000;
                    double e = 1 - Tc / Th, Qh = P1 / e, Qc = Qh - P1;
                    return Ex.Num(L.F("Uma máquina de Carnot entre {0} K e {1} K fornece {2} W de potência útil. Quanto calor ela joga na fonte fria a cada segundo, em watts?", P.N(Th, 0), P.N(Tc, 0), P.N(P1, 0)), Qc, "W",
                        $"η = 1 − {P.N(Tc, 0)}/{P.N(Th, 0)} = {P.N(e, 3)}; potência recebida = {P.N(P1, 0)} / {P.N(e, 3)} = {P.N(Qh)} W; rejeitado = {P.N(Qh)} − {P.N(P1, 0)} = {P.N(Qc)} W.");
                }
                double Th2 = Ex.Step(r, 500, 800, 50), Tc2 = Ex.Step(r, 250, 300, 10), dT = Ex.Step(r, 20, 50, 10);
                double eUp = 1 - Tc2 / (Th2 + dT), eDown = 1 - (Tc2 - dT) / Th2;
                bool coldWins = eDown > eUp;
                return Ex.Choice(r, L.F("Uma máquina de Carnot trabalha entre {0} K e {1} K. Para aumentar o rendimento, é melhor subir a fonte quente em {2} K ou baixar a fonte fria em {2} K?", P.N(Th2, 0), P.N(Tc2, 0), P.N(dT, 0)),
                    coldWins ? L.T("Baixar a fonte fria") : L.T("Subir a fonte quente"), new[] { coldWins ? L.T("Subir a fonte quente") : L.T("Baixar a fonte fria"), L.T("Dá exatamente o mesmo rendimento") },
                    $"Subir: η = 1 − {P.N(Tc2, 0)}/{P.N(Th2 + dT, 0)} = {P.N(eUp, 4)}. Baixar: η = 1 − {P.N(Tc2 - dT, 0)}/{P.N(Th2, 0)} = {P.N(eDown, 4)}.");
            }
        }
    }
}

/// <summary>Electricity and magnetism: Ohm, Coulomb, fields, resistors, forces.</summary>
[Exercises("fis-09-eletromagnetismo")]
public sealed class Fis09Eletro : IExerciseGenerator
{
    const double K = 8.99e9;

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                double Rr = Ex.Step(r, 10, 500, 10), I = Ex.Step(r, 0.1, 3, 0.1), V = Rr * I;
                return r.Next(3) switch
                {
                    0 => Ex.Num(L.F("Um resistor de {0} Ω é percorrido por uma corrente de {1} A. Qual a tensão sobre ele?", P.N(Rr, 0), P.N(I)), V, "V", $"V = R·I = {P.N(Rr, 0)}·{P.N(I)} = {P.N(V)} V."),
                    1 => Ex.Num(L.F("Uma tensão de {0} V é aplicada a um resistor de {1} Ω. Qual a corrente?", P.N(V), P.N(Rr, 0)), I, "A", $"I = V / R = {P.N(V)} / {P.N(Rr, 0)} = {P.N(I)} A."),
                    _ => Ex.Num(L.F("Um resistor ligado a {0} V é percorrido por {1} A. Qual a potência dissipada?", P.N(V), P.N(I)), V * I, "W", $"P = V·I = {P.N(V)}·{P.N(I)} = {P.N(V * I)} W."),
                };
            }
            case Difficulty.Medium:
            {
                double q1 = Ex.Step(r, 1, 20, 1), q2 = Ex.Step(r, 1, 20, 1), dist = Ex.Step(r, 0.1, 2, 0.1);
                switch (r.Next(3))
                {
                    case 0:
                        double F = K * q1 * q2 * 1e-12 / (dist * dist);
                        return Ex.Num(L.F("Duas cargas de {0} μC e {1} μC estão separadas por {2} m no vácuo. Qual a força entre elas? (k = 8,99·10⁹ N·m²/C²)", P.N(q1, 0), P.N(q2, 0), P.N(dist)), F, "N", $"F = k q₁ q₂ / r² = 8,99·10⁹·{P.N(q1, 0)}·10⁻⁶·{P.N(q2, 0)}·10⁻⁶ / {P.N(dist)}² = {P.N(F)} N.".Replace("8,99", P.N(8.99)));
                    case 1:
                        double E = K * q1 * 1e-6 / (dist * dist);
                        return Ex.Num(L.F("Qual o campo elétrico, em kN/C, a {0} m de uma carga pontual de {1} μC? (k = 8,99·10⁹ N·m²/C²)", P.N(dist), P.N(q1, 0)), E / 1e3, "kN/C", $"E = k q / r² = 8,99·10⁹·{P.N(q1, 0)}·10⁻⁶ / {P.N(dist)}² = {P.N(E)} N/C = {P.N(E / 1e3)} kN/C.".Replace("8,99", P.N(8.99)));
                    default:
                        double r1 = Ex.Step(r, 10, 200, 10), r2 = Ex.Step(r, 10, 200, 10), rp = r1 * r2 / (r1 + r2), V = Ex.Step(r, 5, 24, 1);
                        return Ex.Num(L.F("Dois resistores de {0} Ω e {1} Ω estão em paralelo, ligados a {2} V. Qual a corrente total fornecida pela fonte?", P.N(r1, 0), P.N(r2, 0), P.N(V, 0)), V / rp, "A", $"R = R₁R₂/(R₁+R₂) = {P.N(rp)} Ω; I = V / R = {P.N(V, 0)} / {P.N(rp)} = {P.N(V / rp, 3)} A.");
                }
            }
            case Difficulty.Hard:
            {
                double B = Ex.Step(r, 0.1, 2, 0.1), I = Ex.Step(r, 1, 20, 1), len = Ex.Step(r, 0.1, 1, 0.1);
                switch (r.Next(3))
                {
                    case 0:
                        return Ex.Num(L.F("Um fio reto de {0} m, percorrido por {1} A, está perpendicular a um campo magnético de {2} T. Qual a força sobre o fio?", P.N(len), P.N(I, 0), P.N(B)), B * I * len, "N", $"F = B I L = {P.N(B)}·{P.N(I, 0)}·{P.N(len)} = {P.N(B * I * len)} N.");
                    case 1:
                        double dist = Ex.Step(r, 0.01, 0.5, 0.01), Bw = 4e-7 * Math.PI * I / (2 * Math.PI * dist);
                        return Ex.Num(L.F("Qual o campo magnético, em μT, a {0} m de um fio longo e reto percorrido por {1} A? (μ₀ = 4π·10⁻⁷ T·m/A)", P.N(dist), P.N(I, 0)), Bw * 1e6, "μT", $"B = μ₀ I / (2π r) = 4π·10⁻⁷·{P.N(I, 0)} / (2π·{P.N(dist)}) = {P.N(Bw * 1e6)} μT.");
                    default:
                        double q = Ex.Step(r, 1, 10, 1), v = Ex.Step(r, 1, 50, 1) * 1e3;
                        return Ex.Num(L.F("Uma carga de {0} μC move-se a {1} km/s perpendicularmente a um campo magnético de {2} T. Qual a força magnética, em mN?", P.N(q, 0), P.N(v / 1e3, 0), P.N(B)), q * 1e-6 * v * B * 1e3, "mN", $"F = q v B = {P.N(q, 0)}·10⁻⁶·{P.N(v, 0)}·{P.N(B)} = {P.N(q * 1e-6 * v * B * 1e3)} mN.");
                }
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    // Where between two like charges is the field zero? Closer to the smaller one.
                    double q1 = Ex.Step(r, 1, 9, 1), q2 = q1 * Ex.Pick(r, 4.0, 9, 16, 25), dist = Ex.Step(r, 0.2, 2, 0.1);
                    double x = dist * Math.Sqrt(q1) / (Math.Sqrt(q1) + Math.Sqrt(q2));
                    return Ex.Num(L.F("Duas cargas positivas, de {0} μC e {1} μC, estão a {2} m uma da outra. A que distância da carga MENOR (em m), sobre a reta que as une, o campo elétrico resultante é nulo?", P.N(q1, 0), P.N(q2, 0), P.N(dist)), x, "m",
                        $"k q₁/x² = k q₂/(d−x)² → (d−x)/x = √(q₂/q₁) = {P.N(Math.Sqrt(q2 / q1), 0)} → x = d / (1 + {P.N(Math.Sqrt(q2 / q1), 0)}) = {P.N(x, 3)} m.");
                }
                // Maximum power transfer from a battery with internal resistance happens when R = r.
                double eps = Ex.Step(r, 6, 24, 1), ri = Ex.Step(r, 1, 10, 1), pmax = eps * eps / (4 * ri);
                return Ex.Num(L.F("Uma bateria de {0} V com resistência interna de {1} Ω alimenta um resistor variável. Qual a potência máxima que se consegue entregar ao resistor?", P.N(eps, 0), P.N(ri, 0)), pmax, "W",
                    $"A potência é máxima quando R = r = {P.N(ri, 0)} Ω: I = ε / (2r) = {P.N(eps / (2 * ri))} A; P = I²R = {P.N(pmax)} W (= ε²/4r).");
            }
        }
    }
}

/// <summary>Air resistance and numerical methods: drag force, terminal velocity, Euler steps.</summary>
[Exercises("fis-10-arrasto-numerico")]
public sealed class Fis10Arrasto : IExerciseGenerator
{
    const double G = P.G;

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                double rho = 1.2, cd = Ex.Pick(r, 0.47, 0.8, 1.0, 1.2), A = Ex.Step(r, 0.01, 1, 0.01), v = Ex.Step(r, 2, 40, 1);
                double F = 0.5 * rho * cd * A * v * v;
                return Ex.Num(L.F("Qual a força de arrasto do ar (ρ = 1,2 kg/m³) sobre um objeto com Cd = {0} e área frontal de {1} m², a {2} m/s?", P.N(cd), P.N(A), P.N(v)), F, "N",
                    $"F = ½ ρ Cd A v² = 0,5·{P.N(1.2)}·{P.N(cd)}·{P.N(A)}·{P.N(v)}² = {P.N(F)} N.");
            }
            case Difficulty.Medium:
            {
                double m = Ex.Step(r, 0.05, 90, 0.05), cd = Ex.Pick(r, 0.47, 0.8, 1.0, 1.2), A = Ex.Step(r, 0.01, 1, 0.01);
                if (m > 5) m = Math.Round(m);
                double vt = Math.Sqrt(2 * m * G / (1.2 * cd * A));
                return Ex.Num(L.F("Qual a velocidade terminal de um corpo de {0} kg, com Cd = {1} e área frontal de {2} m², caindo no ar (ρ = 1,2 kg/m³)?", P.N(m), P.N(cd), P.N(A)), vt, "m/s",
                    $"Peso = arrasto: m g = ½ ρ Cd A v² → v = √(2 m g / (ρ Cd A)) = √(2·{P.N(m)}·{P.N(G)} / ({P.N(1.2)}·{P.N(cd)}·{P.N(A)})) = {P.N(vt)} m/s.");
            }
            case Difficulty.Hard:
            {
                // One or two Euler steps with linear drag a = g − (b/m)v.
                double m = Ex.Step(r, 1, 10, 1), b = Ex.Step(r, 1, 10, 1), dt = Ex.Pick(r, 0.1, 0.2, 0.5), v0 = Ex.Step(r, 0, 10, 1);
                double a0 = G - b / m * v0, v1 = v0 + a0 * dt, a1 = G - b / m * v1, v2 = v1 + a1 * dt;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um corpo de {0} kg cai com arrasto linear (força = {1}·v, em N, com v em m/s). Usando o método de Euler com passo de {2} s, qual a velocidade 1 passo depois de v = {3} m/s?", P.N(m), P.N(b, 0), P.N(dt), P.N(v0, 0)),
                        v1, "m/s", $"a = g − (b/m) v = {P.N(G)} − ({P.N(b, 0)}/{P.N(m)})·{P.N(v0, 0)} = {P.N(a0)} m/s²; v₁ = v₀ + a·Δt = {P.N(v0, 0)} + {P.N(a0)}·{P.N(dt)} = {P.N(v1, 3)} m/s.");
                return Ex.Num(L.F("Um corpo de {0} kg cai com arrasto linear (força = {1}·v, em N, com v em m/s). Usando o método de Euler com passo de {2} s, qual a velocidade 2 passos depois de v = {3} m/s?", P.N(m), P.N(b, 0), P.N(dt), P.N(v0, 0)),
                    v2, "m/s", $"Passo 1: a = {P.N(a0)}, v₁ = {P.N(v1, 3)}. Passo 2: a = {P.N(G)} − ({P.N(b, 0)}/{P.N(m)})·{P.N(v1, 3)} = {P.N(a1, 3)} m/s²; v₂ = {P.N(v1, 3)} + {P.N(a1, 3)}·{P.N(dt)} = {P.N(v2, 3)} m/s.");
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    // Quadratic drag: a = g (1 − (v/vt)²). At a fraction f of the terminal speed...
                    double f = Ex.Pick(r, 0.5, 0.6, 0.8, 0.9), vt = Ex.Step(r, 10, 60, 1), a = G * (1 - f * f);
                    return Ex.Num(L.F("Um paraquedista tem velocidade terminal de {0} m/s (arrasto proporcional a v²). Qual a sua aceleração no instante em que a velocidade é {1} da terminal?", P.N(vt, 0), P.N(f * 100, 0) + "%"), a, "m/s²",
                        $"Com arrasto ∝ v²: a = g (1 − (v/vₜ)²) = {P.N(G)}·(1 − {P.N(f)}²) = {P.N(a)} m/s².");
                }
                double m = Ex.Step(r, 1, 10, 1), b = Ex.Step(r, 2, 20, 1), t = Ex.Step(r, 0.2, 2, 0.1);
                double vt2 = m * G / b, v = vt2 * (1 - Math.Exp(-b * t / m));
                return Ex.Num(L.F("Um corpo de {0} kg é solto do repouso com arrasto linear (força = {1}·v, em N). Qual a velocidade exata (não numérica) depois de {2} s?", P.N(m), P.N(b, 0), P.N(t)),
                    v, "m/s", $"vₜ = m g / b = {P.N(vt2)} m/s; v(t) = vₜ (1 − e^(−b t/m)) = {P.N(vt2)}·(1 − e^(−{P.N(b * t / m, 3)})) = {P.N(v)} m/s.");
            }
        }
    }
}
