using static EasyCreator.Exercises.Pv;

namespace EasyCreator.Exercises;

/// <summary>Aircraft 16-20: weight and balance, helicopter at altitude, rotor in forward flight, autorotation, the bush plane.</summary>
[Exercises("aero-16-peso-balanceamento")]
public sealed class Air16 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double m0 = Ex.Step(r, 500, 900, 10), x0 = Ex.Step(r, 1.0, 1.4, 0.01);
        double mp = Ex.Step(r, 60, 100, 5), xp = Ex.Step(r, 1.8, 2.2, 0.05), mf = Ex.Step(r, 40, 120, 5), xf = Ex.Step(r, 1.5, 1.9, 0.05), mb = Ex.Step(r, 10, 40, 2), xb = Ex.Step(r, 2.8, 3.6, 0.05);
        switch (d)
        {
            case Difficulty.Easy:
            {
                double cg = (m0 * x0 + mp * xp) / (m0 + mp);
                return Ex.Num(L.F("Um avião vazio de {0} kg tem o CG a {1} m da referência. Um piloto de {2} kg senta a {3} m. Onde fica o CG, em m?", N(m0), N(x0), N(mp), N(xp)), cg, "m",
                    L.F("CG = (m₁x₁ + m₂x₂)/(m₁ + m₂) = ({0}·{1} + {2}·{3})/({0} + {2}) = {4} m.", N(m0), N(x0), N(mp), N(xp), N(cg, 3)), 3);
            }
            case Difficulty.Medium:
            {
                double M = m0 + mp + mf + mb, mom = m0 * x0 + mp * xp + mf * xf + mb * xb, cg = mom / M;
                string load = L.F("Avião vazio {0} kg a {1} m; piloto {2} kg a {3} m; combustível {4} kg a {5} m; bagagem {6} kg a {7} m.", N(m0), N(x0), N(mp), N(xp), N(mf), N(xf), N(mb), N(xb));
                if (r.Next(2) == 0)
                    return Ex.Num(load + " " + L.T("Qual a massa total, em kg?"), M, "kg", L.F("{0} + {1} + {2} + {3} = {4} kg.", N(m0), N(mp), N(mf), N(mb), N(M)));
                return Ex.Num(load + " " + L.T("Onde fica o CG (em m da referência)?"), cg, "m",
                    L.F("Momento total = {0} kg·m; massa total = {1} kg; CG = {0}/{1} = {2} m.", N(mom), N(M), N(cg, 3)), 3);
            }
            case Difficulty.Hard:
            {
                double M = m0 + mp + mf + mb, cg = (m0 * x0 + mp * xp + mf * xf + mb * xb) / M, xle = Ex.Step(r, 1.4, 1.8, 0.05), cma = Ex.Step(r, 1.3, 1.7, 0.05), pc = (cg - xle) / cma * 100;
                string load = L.F("Avião vazio {0} kg a {1} m; piloto {2} kg a {3} m; combustível {4} kg a {5} m; bagagem {6} kg a {7} m. A CMA tem {8} m e começa a {9} m da referência.", N(m0), N(x0), N(mp), N(xp), N(mf), N(xf), N(mb), N(xb), N(cma), N(xle));
                if (r.Next(2) == 0)
                    return Ex.Num(load + " " + L.T("Em que % da CMA está o CG?"), pc, "%",
                        L.F("CG = {0} m; % CMA = (CG − início da CMA) / CMA = ({0} − {1}) / {2} = {3}%.", N(cg, 3), N(xle), N(cma), N(pc)), 3);
                double np = 25 + Ex.Step(r, 8, 20, 1);
                return Ex.Num(load + " " + L.F("O ponto neutro está a {0}% da CMA. Qual a margem estática, em % da CMA?", N(np)), np - pc, "%",
                    L.F("CG = {0}% da CMA; margem = {1} − {0} = {2}%.", N(pc), N(np), N(np - pc)), 3);
            }
            default:
            {
                double M = m0 + mp + mf, cg = (m0 * x0 + mp * xp + mf * xf) / M, limit = cg + Ex.Step(r, 0.02, 0.06, 0.01);
                double xb2 = Ex.Step(r, 3.0, 3.6, 0.05);
                double maxBag = M * (limit - cg) / (xb2 - limit);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Avião vazio {0} kg a {1} m; piloto {2} kg a {3} m; combustível {4} kg a {5} m. O limite traseiro do CG é {6} m. Qual a maior bagagem, em kg, que pode ir no porta-malas a {7} m?", N(m0), N(x0), N(mp), N(xp), N(mf), N(xf), N(limit, 3), N(xb2)), maxBag, "kg",
                        L.F("CG sem bagagem = {0} m (massa {1} kg). Com bagagem m_b: ({1}·{0} + m_b·{2}) / ({1} + m_b) = {3} → m_b = {1}·({3} − {0}) / ({2} − {3}) = {4} kg.", N(cg, 3), N(M), N(xb2), N(limit, 3), N(maxBag)), 3);
                double dm = Ex.Step(r, 5, 25, 1), shift = dm * (xb2 - xp) / (M + dm);
                return Ex.Num(L.F("Um avião com {0} kg totais tem o CG a {1} m. Colocando mais {2} kg de bagagem a {3} m, de quantos metros o CG se desloca para trás? (m_novo = massa + bagagem)", N(M), N(cg, 3), N(dm), N(xb2)), dm * (xb2 - cg) / (M + dm), "m",
                    L.F("CG novo = ({0}·{1} + {2}·{3}) / ({0} + {2}) = {4} m; deslocamento = {5} m.", N(M), N(cg, 3), N(dm), N(xb2), N(cg + dm * (xb2 - cg) / (M + dm), 4), N(dm * (xb2 - cg) / (M + dm), 4)), 3);
            }
        }
    }
}

[Exercises("aero-17-helicoptero-altitude")]
public sealed class Air17 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double m = Ex.Step(r, 600, 3000, 50), R = Ex.Step(r, 3, 7, 0.25), A = Math.PI * R * R, T = m * G, fm = Ex.Step(r, 0.62, 0.75, 0.01);
        double h = Ex.Pick(r, 0.0, 1000, 2000, 3000, 4000), rho = Isa.Rho(h), Pid = Math.Pow(T, 1.5) / Math.Sqrt(2 * rho * A), Preal = Pid / fm;
        double Psl = Math.Pow(T, 1.5) / Math.Sqrt(2 * 1.225 * A) / fm;
        string heli = L.F("Um helicóptero de {0} kg, rotor de raio {1} m, figure of merit {2} (g = 9,8 m/s²).", N(m), N(R), N(fm));
        switch (d)
        {
            case Difficulty.Easy:
                return Ex.Num(heli + " " + L.F("Qual a potência ideal para pairar a {0} m de altitude (ρ = {1} kg/m³), em kW? (P = T^1,5 / √(2ρA))", N(h), N(rho, 3)), Pid / 1000, "kW",
                    L.F("T = {0} N; A = {1} m²; P = T^1,5 / √(2·{2}·{1}) = {3} kW.", N(T), N(A), N(rho, 3), N(Pid / 1000)), 3);
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(heli + " " + L.F("Qual a potência real para pairar a {0} m (ρ = {1} kg/m³), em kW?", N(h), N(rho, 3)), Preal / 1000, "kW",
                        L.F("P_ideal = {0} kW; P_real = P_ideal / FM = {0} / {1} = {2} kW.", N(Pid / 1000), N(fm), N(Preal / 1000)), 3);
                {
                    double sol = Ex.Step(r, 2, 4, 1), chord = Ex.Step(r, 0.2, 0.4, 0.05), sigma = sol * chord / (Math.PI * R);
                    return Ex.Num(L.F("Um rotor de raio {0} m tem {1} pás de corda {2} m. Qual a solidez σ = (nº de pás × corda) / (π × R)?", N(R), N(sol, 0), N(chord)), sigma, "",
                        L.F("σ = {0}·{1} / (π·{2}) = {3}.", N(sol, 0), N(chord), N(R), N(sigma, 4)), 3);
                }
            case Difficulty.Hard:
            {
                double hh = Ex.Step(r, 2000, 5000, 500), rh = Isa.Rho(hh), Ph = Math.Pow(T, 1.5) / Math.Sqrt(2 * rh * A) / fm;
                if (r.Next(2) == 0)
                    return Ex.Num(heli + " " + L.F("De quantos por cento a potência real para pairar aumenta indo do nível do mar (ρ = 1,225 kg/m³) para {0} m (ρ = {1} kg/m³)?", N(hh), N(rh, 3)), (Ph / Psl - 1) * 100, "%",
                        L.F("P ∝ 1/√ρ: √(1,225/{0}) = {1}; aumento = {2}%.", N(rh, 3), N(Math.Sqrt(1.225 / rh), 4), N((Ph / Psl - 1) * 100)), 3);
                double Pav = Math.Round(Psl * 1.3 / 1000 / 5) * 5, sigma = Math.Pow(Psl / 1000 / Pav, 2.0 / 3), ceil = Isa.AltOfSigma(sigma);
                return Ex.Num(heli + " " + L.F("Ao nível do mar a potência real para pairar é {0} kW. O motor entrega {1} kW ao nível do mar, e a potência disponível cai com a densidade (P_disp = P₀·σ, σ = ρ/1,225). A potência necessária cresce como 1/√σ. Em que altitude, em m, a potência disponível iguala a necessária (teto de pairar)? (atmosfera padrão)", N(Psl / 1000), N(Pav)), ceil, "m",
                    L.F("P₀·σ = P_sl / √σ → σ^1,5 = P_sl/P₀ = {0}/{1} → σ = {2}; ρ = {3} kg/m³ ↔ h = {4} m (atmosfera padrão).", N(Psl / 1000), N(Pav), N(sigma, 4), N(sigma * 1.225, 3), N(ceil)), 3);
            }
            default:
            {
                double hh = Ex.Step(r, 2000, 4500, 500), rh = Isa.Rho(hh), Pav = Psl * Ex.Step(r, 0.75, 0.95, 0.05) * Isa.Sigma(hh);
                // Pav is the available power at altitude hh; find the max mass that can hover: P = T^1.5 / (sqrt(2 rho A) FM)
                double Tmax = Math.Pow(Pav * fm * Math.Sqrt(2 * rh * A), 2.0 / 3), mmax = Tmax / G;
                return Ex.Num(L.F("Um helicóptero com rotor de raio {0} m e figure of merit {1} tem {2} kW disponíveis a {3} m de altitude (ρ = {4} kg/m³, g = 9,8 m/s²). Qual a massa máxima, em kg, que ele consegue pairar?", N(R), N(fm), N(Pav / 1000), N(hh), N(rh, 3)), mmax, "kg",
                    L.F("P·FM = T^1,5/√(2ρA) → T = (P·FM·√(2ρA))^(2/3) = {0} N → m = T/g = {1} kg.", N(Tmax), N(mmax)), 3);
            }
        }
    }
}

[Exercises("aero-18-rotor-avanco")]
public sealed class Air18 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double R = Ex.Step(r, 3, 8, 0.25), rpm = Ex.Step(r, 280, 500, 10), om = rpm * 2 * Math.PI / 60, tip = om * R, V = Ex.Step(r, 20, 80, 2);
        string rot = L.F("Um rotor de raio {0} m gira a {1} rpm; o helicóptero voa a {2} m/s.", N(R), N(rpm), N(V));
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(rot + " " + L.T("Qual a velocidade da ponta da pá (Ω·R), em m/s?"), tip, "m/s",
                        L.F("Ω = {0}·2π/60 = {1} rad/s; Ω·R = {1}·{2} = {3} m/s.", N(rpm), N(om), N(R), N(tip)));
                return Ex.Num(rot + " " + L.T("Qual o parâmetro de avanço μ = V / (Ω·R)?"), V / tip, "",
                    L.F("μ = {0} / {1} = {2}.", N(V), N(tip), N(V / tip, 3)), 3);
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(rot + " " + L.T("Qual a velocidade da ponta da pá que avança, em m/s?"), tip + V, "m/s",
                        L.F("Ω·R + V = {0} + {1} = {2} m/s.", N(tip), N(V), N(tip + V)));
                return Ex.Num(rot + " " + L.T("Qual a velocidade da ponta da pá que recua, em m/s?"), tip - V, "m/s",
                    L.F("Ω·R − V = {0} − {1} = {2} m/s.", N(tip), N(V), N(tip - V)));
            case Difficulty.Hard:
            {
                double a = 340, Ma = (tip + V) / a, rev = V / om;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(rot + " " + L.T("Qual o número de Mach da ponta da pá que avança? (som a 340 m/s)"), Ma, "",
                        L.F("M = (Ω·R + V) / a = ({0} + {1}) / 340 = {2}.", N(tip), N(V), N(Ma, 3)), 3);
                    default: return Ex.Num(rot + " " + L.T("A pá que recua tem uma região perto do eixo onde o ar vem 'por trás' da pá (fluxo reverso). Até que raio, em m, vai essa região? (r = V/Ω)"), rev, "m",
                        L.F("Ω·r = V → r = V/Ω = {0} / {1} = {2} m.", N(V), N(om), N(rev)), 3);
                }
            }
            default:
            {
                double vmax = 0.9 * 340 - tip, mu = V / tip, rrev = mu * R;
                if (r.Next(2) == 0 && vmax > 5)
                    return Ex.Num(L.F("Um rotor de raio {0} m gira a {1} rpm. Para a ponta da pá que avança não passar de Mach 0,9 (som a 340 m/s), qual a velocidade máxima do helicóptero, em m/s?", N(R), N(rpm)), vmax, "m/s",
                        L.F("Ω·R + V = 0,9·340 → V = 306 − {0} = {1} m/s.", N(tip), N(vmax)), 3);
                return Ex.Num(rot + " " + L.T("Que fração do disco (raio do círculo de fluxo reverso, como % do raio R) tem fluxo reverso? (r_rev = μ·R, responda μ·100)"), mu * 100, "%",
                    L.F("μ = V/(Ω·R) = {0}/{1} = {2}; r_rev = μ·R = {3} m ({4}% de R).", N(V), N(tip), N(mu, 3), N(rrev), N(mu * 100)), 3);
            }
        }
    }
}

[Exercises("aero-19-autorrotacao")]
public sealed class Air19 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G, Rho = 1.225;
        double m = Ex.Step(r, 600, 3000, 50), R = Ex.Step(r, 3.5, 7, 0.25), A = Math.PI * R * R, W = m * G, vd = Math.Sqrt(2 * W / (Rho * A * 1.2));
        double rpm = Ex.Step(r, 300, 450, 10), om = rpm * 2 * Math.PI / 60, I = Ex.Step(r, 150, 900, 10), E = 0.5 * I * om * om * (1 - 0.49);
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Em autorrotação, um helicóptero de {0} kg com rotor de raio {1} m desce a v ≈ √(2·W / (ρ·A·1,2)). Com ρ = 1,225 kg/m³ e g = 9,8 m/s², qual a velocidade de descida, em m/s?", N(m), N(R)), vd, "m/s",
                        L.F("W = {0} N; A = {1} m²; v = √(2·{0} / (1,225·{1}·1,2)) = {2} m/s.", N(W), N(A), N(vd)), 3);
                {
                    double hh = Ex.Step(r, 200, 1000, 50);
                    return Ex.Num(L.F("Um helicóptero de {0} kg (rotor de raio {1} m) em autorrotação desce a v ≈ √(2·W / (ρ·A·1,2)) com ρ = 1,225 kg/m³ e g = 9,8 m/s². Quantos segundos leva para descer {2} m?", N(m), N(R), N(hh)), hh / vd, "s",
                        L.F("v = {0} m/s; t = h / v = {1} / {0} = {2} s.", N(vd), N(hh), N(hh / vd)), 3);
                }
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um rotor tem momento de inércia de {0} kg·m² e gira a {1} rpm. Qual a energia cinética de rotação, em kJ? (E = ½·I·Ω²)", N(I), N(rpm)), 0.5 * I * om * om / 1000, "kJ",
                        L.F("Ω = {0} rad/s; E = ½·{1}·{0}² = {2} kJ.", N(om), N(I), N(0.5 * I * om * om / 1000)));
                return Ex.Num(L.F("Um rotor tem I = {0} kg·m² e gira a {1} rpm. Na flare, o rotor desacelera até 70% da rotação inicial. Quanta energia, em kJ, ele libera? (E = ½·I·Ω²·(1 − 0,7²))", N(I), N(rpm)), E / 1000, "kJ",
                    L.F("E = ½·{0}·{1}²·0,51 = {2} kJ.", N(I), N(om), N(E / 1000)));
            case Difficulty.Hard:
            {
                double Ek = 0.5 * m * vd * vd;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um helicóptero de {0} kg (rotor de raio {1} m) em autorrotação desce a v ≈ √(2·W / (ρ·A·1,2)) (ρ = 1,225 kg/m³, g = 9,8 m/s²). Qual a energia cinética da descida (½·m·v²), em kJ?", N(m), N(R)), Ek / 1000, "kJ",
                            L.F("v = {0} m/s; E = ½·{1}·{0}² = {2} kJ.", N(vd), N(m), N(Ek / 1000)), 3);
                    default:
                    {
                        double heq = E / W;
                        return Ex.Num(L.F("Na flare, um rotor de I = {0} kg·m² a {1} rpm libera E = ½·I·Ω²·(1 − 0,7²). Para um helicóptero de {2} kg, qual a altura equivalente h = E / (m·g), em m? (g = 9,8 m/s²)", N(I), N(rpm), N(m)), heq, "m",
                            L.F("E = {0} J; h = E / (m·g) = {0} / ({1}·9,8) = {2} m.", N(E), N(m), N(heq)), 3);
                    }
                }
            }
            default:
            {
                double Ek = 0.5 * m * vd * vd, ratio = E / Ek, Ineed = Ek / (0.5 * om * om * (1 - 0.49));
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um helicóptero de {0} kg (rotor de raio {1} m) em autorrotação desce a v ≈ √(2·W / (ρ·A·1,2)) (ρ = 1,225 kg/m³, g = 9,8 m/s²). O rotor (I = {2} kg·m², {3} rpm) libera E = ½·I·Ω²·(1 − 0,7²) na flare. Que razão E / (½·m·v²) ele tem? (quanto maior, mais folga para o pouso)", N(m), N(R), N(I), N(rpm)), ratio, "",
                        L.F("v = {0} m/s; ½·m·v² = {1} kJ; E = {2} kJ; razão = {3}.", N(vd), N(Ek / 1000), N(E / 1000), N(ratio, 3)), 3);
                return Ex.Num(L.F("Um helicóptero de {0} kg (rotor de raio {1} m) em autorrotação desce a v ≈ √(2·W / (ρ·A·1,2)) (ρ = 1,225 kg/m³, g = 9,8 m/s²). Que momento de inércia I, em kg·m², o rotor (a {2} rpm) precisa ter para que a energia liberada na flare, E = ½·I·Ω²·(1 − 0,7²), iguale a energia cinética da descida?", N(m), N(R), N(rpm)), Ineed, "kg·m²",
                    L.F("½·m·v² = {0} J; I = {0} / (½·Ω²·0,51) = {0} / (0,5·{1}²·0,51) = {2} kg·m².", N(Ek), N(om), N(Ineed)), 3);
            }
        }
    }
}

[Exercises("aero-20-projeto-mato")]
public sealed class Air20 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double m = Ex.Step(r, 500, 1500, 25), S = Ex.Step(r, 10, 20, 0.5), clmax = Ex.Step(r, 1.6, 2.4, 0.05), h = Ex.Pick(r, 0.0, 500, 1000, 1500, 2000), rho = Isa.Rho(h);
        double vs = Math.Sqrt(2 * m * G / (rho * S * clmax)), vlof = 1.2 * vs, T = m * G * Ex.Step(r, 0.3, 0.5, 0.01), a = T / m - 0.04 * G, run = vlof * vlof / (2 * a);
        string ac = L.F("Um avião de mato de {0} kg, asa de {1} m², CLmáx = {2} (com flapes), em ar de ρ = {3} kg/m³ (g = 9,8 m/s²).", N(m), N(S), N(clmax), N(rho, 3));
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um avião de {0} kg tem {1} m² de asa. Qual a carga alar, em kg/m²?", N(m), N(S)), m / S, "kg/m²",
                        L.F("carga alar = massa / área = {0} / {1} = {2} kg/m².", N(m), N(S), N(m / S)));
                return Ex.Num(ac + " " + L.T("Qual a velocidade de estol, em m/s?"), vs, "m/s",
                    L.F("V = √(2·m·g / (ρ·S·CLmáx)) = √(2·{0}·9,8 / ({1}·{2}·{3})) = {4} m/s.", N(m), N(rho, 3), N(S), N(clmax), N(vs)));
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(ac + " " + L.T("Qual a velocidade de decolagem (1,2 × V_estol), em km/h?"), Kmh(vlof), "km/h",
                        L.F("V_estol = {0} m/s; V_dec = 1,2 × {0} = {1} m/s = {2} km/h.", N(vs), N(vlof), N(Kmh(vlof))));
                return Ex.Num(ac + " " + L.F("O empuxo médio na corrida é {0} N, com atrito de rolamento 0,04·m·g (pista de terra). Qual a aceleração média, em m/s²?", N(T)), a, "m/s²",
                    L.F("a = T/m − 0,04·g = {0}/{1} − 0,04·9,8 = {2} m/s².", N(T), N(m), N(a)));
            case Difficulty.Hard:
                if (r.Next(2) == 0)
                    return Ex.Num(ac + " " + L.F("O empuxo médio é {0} N e o atrito de rolamento 0,04·m·g. Qual a distância da corrida de decolagem (até 1,2 × V_estol), em m?", N(T)), run, "m",
                        L.F("V_dec = {0} m/s; a = {1} m/s²; s = V² / (2a) = {2} m.", N(vlof), N(a), N(run)), 3);
                {
                    double strip = Ex.Step(r, 150, 400, 10), need = vlof * vlof / (2 * strip), Tn = (need + 0.04 * G * m);
                    double aNeed = vlof * vlof / (2 * strip), Treq = m * (aNeed + 0.04 * G);
                    return Ex.Num(ac + " " + L.F("A pista tem {0} m e o atrito de rolamento é 0,04·m·g. Que empuxo médio mínimo, em N, permite decolar (até 1,2 × V_estol) nela?", N(strip)), Treq, "N",
                        L.F("a necessária = V² / (2·s) = {0} m/s²; T = m·(a + 0,04·g) = {1} N.", N(aNeed), N(Treq)), 3);
                }
            default:
            {
                double wind = Ex.Step(r, 2, 10, 1), runW = Math.Pow(vlof - wind, 2) / (2 * a), h2 = h + Ex.Step(r, 500, 1500, 500), rho2 = Isa.Rho(h2);
                double vs2 = Math.Sqrt(2 * m * G / (rho2 * S * clmax)), vlof2 = 1.2 * vs2, T2 = T * rho2 / rho, a2 = T2 / m - 0.04 * G, run2 = vlof2 * vlof2 / (2 * a2);
                if (r.Next(2) == 0)
                    return Ex.Num(ac + " " + L.F("O empuxo médio é {0} N, o atrito de rolamento 0,04·m·g e há {1} m/s de vento de proa. Qual a corrida de decolagem, em m? (a decolagem se dá a 1,2 × V_estol no ar)", N(T), N(wind)), runW, "m",
                        L.F("Velocidade em relação ao solo: {0} − {1} = {2} m/s; s = (V − vento)² / (2a) = {2}² / (2·{3}) = {4} m.", N(vlof), N(wind), N(vlof - wind), N(a), N(runW)), 3);
                return Ex.Num(ac + " " + L.F("O empuxo médio é {0} N ao nível desta pista. Se o avião passar a uma pista mais alta, de ρ = {1} kg/m³, o empuxo cai proporcionalmente à densidade. Qual a nova corrida, em m? (atrito 0,04·m·g; decola a 1,2 × V_estol)", N(T), N(rho2, 3)), run2, "m",
                    L.F("V_estol sobe para {0} m/s, V_dec = {1} m/s; empuxo = {2} N; a = {3} m/s²; s = V² / (2a) = {4} m.", N(vs2), N(vlof2), N(T2), N(a2), N(run2)), 3);
            }
        }
    }
}
