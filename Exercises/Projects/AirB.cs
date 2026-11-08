using static EasyCreator.Exercises.Pv;

namespace EasyCreator.Exercises;

/// <summary>Aircraft 6-10: stability and CG, propulsion, helicopter hover, tail rotor, Breguet range.</summary>
[Exercises("aero-06-estabilidade")]
public sealed class Air06 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double np = Ex.Step(r, 35, 55, 1), cg = Ex.Step(r, 20, 40, 1), sm = np - cg;
        double c = Ex.Step(r, 1.0, 2.2, 0.1), xle = Ex.Step(r, 1.5, 3.5, 0.1);
        switch (d)
        {
            case Difficulty.Easy:
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("O ponto neutro de um avião está a {0}% da corda média (CMA) e o CG a {1}%. Qual a margem estática, em % da CMA?", N(np), N(cg)), sm, "%",
                        L.F("Margem = ponto neutro − CG = {0} − {1} = {2}%.", N(np), N(cg), N(sm)));
                    case 1: return Ex.Num(L.F("Um avião tem margem estática de {0}% da CMA e o CG a {1}%. Onde está o ponto neutro, em % da CMA?", N(sm), N(cg)), np, "%",
                        L.F("Ponto neutro = CG + margem = {0} + {1} = {2}%.", N(cg), N(sm), N(np)));
                    default: return Ex.Num(L.F("O ponto neutro está a {0}% da CMA e a margem estática deve ser {1}%. Onde deve ficar o CG, em % da CMA?", N(np), N(sm)), cg, "%",
                        L.F("CG = ponto neutro − margem = {0} − {1} = {2}%.", N(np), N(sm), N(cg)));
                }
            case Difficulty.Medium:
            {
                double xcg = xle + cg / 100 * c;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("A CMA de um avião tem {0} m e começa na estação {1} m (medida de uma referência na frente). O CG está na estação {2} m. Em que % da CMA está o CG?", N(c), N(xle), N(xcg, 3)), cg, "%",
                        L.F("({0} − {1}) / {2} × 100 = {3}%.", N(xcg, 3), N(xle), N(c), N(cg)), 3);
                return Ex.Num(L.F("A CMA de um avião tem {0} m e começa na estação {1} m. O CG fica a {2}% da CMA. Em que estação, em m, está o CG?", N(c), N(xle), N(cg)), xcg, "m",
                    L.F("{0} + {1}/100 × {2} = {3} m.", N(xle), N(cg), N(c), N(xcg, 3)), 3);
            }
            case Difficulty.Hard:
            {
                double S = Ex.Step(r, 10, 20, 1), cm = Ex.Step(r, 1.0, 1.8, 0.1), St = Ex.Step(r, 2, 5, 0.25), lt = Ex.Step(r, 3.5, 6, 0.25), vh = St * lt / (S * cm), npT = 25 + 40.5 * vh;
                string ac = L.F("Um avião tem asa de {0} m² (CMA {1} m), estabilizador horizontal de {2} m² a {3} m do centro aerodinâmico da asa.", N(S), N(cm), N(St), N(lt));
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(ac + " " + L.T("Qual o volume de cauda V_H = S_t·l_t/(S·c)?"), vh, "",
                        L.F("V_H = {0}·{1} / ({2}·{3}) = {4}.", N(St), N(lt), N(S), N(cm), N(vh, 3)), 3);
                    case 1: return Ex.Num(ac + " " + L.T("Use ponto neutro = 25% + 40,5% × V_H. Onde está o ponto neutro, em % da CMA?"), npT, "%",
                        L.F("V_H = {0}; ponto neutro = 25 + 40,5·{0} = {1}%.", N(vh, 3), N(npT)), 3);
                    default:
                    {
                        double cg2 = Ex.Step(r, 20, 35, 1);
                        return Ex.Num(ac + " " + L.F("Com ponto neutro = 25% + 40,5% × V_H e CG a {0}% da CMA, qual a margem estática, em %?", N(cg2)), npT - cg2, "%",
                            L.F("Ponto neutro = {0}%; margem = {0} − {1} = {2}%.", N(npT), N(cg2), N(npT - cg2)), 3);
                    }
                }
            }
            default:
            {
                double S = Ex.Step(r, 10, 20, 1), cm = Ex.Step(r, 1.0, 1.8, 0.1), lt = Ex.Step(r, 3.5, 6, 0.25), minSM = Ex.Pick(r, 5.0, 8, 10), npTarget = Ex.Step(r, 42, 55, 1);
                double vhN = (npTarget - 25) / 40.5, stN = vhN * S * cm / lt, xle2 = xle, aft = xle2 + (npTarget - minSM) / 100 * cm;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um avião (asa de {0} m², CMA {1} m, estabilizador a {2} m do centro aerodinâmico da asa) precisa de ponto neutro a {3}% da CMA (ponto neutro = 25% + 40,5% × V_H). Que área de estabilizador, em m², ele precisa?", N(S), N(cm), N(lt), N(npTarget)), stN, "m²",
                        L.F("V_H = ({0} − 25)/40,5 = {1}; S_t = V_H·S·c / l_t = {1}·{2}·{3} / {4} = {5} m².", N(npTarget), N(vhN, 3), N(S), N(cm), N(lt), N(stN)), 3);
                return Ex.Num(L.F("O ponto neutro de um avião está a {0}% da CMA (CMA de {1} m, começando na estação {2} m). Para manter pelo menos {3}% de margem estática, qual a estação mais traseira permitida para o CG, em m?", N(npTarget), N(cm), N(xle2), N(minSM)), aft, "m",
                    L.F("CG máx = {0} − {1} = {2}% da CMA → estação = {3} + {2}/100·{4} = {5} m.", N(npTarget), N(minSM), N(npTarget - minSM), N(xle2), N(cm), N(aft)), 3);
            }
        }
    }
}

[Exercises("aero-07-propulsao")]
public sealed class Air07 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double md = Ex.Step(r, 20, 400, 10), v0 = Ex.Step(r, 60, 250, 5), ve = v0 + Ex.Step(r, 20, 400, 10), T = md * (ve - v0);
        switch (d)
        {
            case Difficulty.Easy:
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um motor processa {0} kg/s de ar, que entra a {1} m/s e sai a {2} m/s. Qual o empuxo, em N?", N(md), N(v0), N(ve)), T, "N",
                        L.F("T = ṁ·(Ve − V₀) = {0}·({1} − {2}) = {3} N.", N(md), N(ve), N(v0), N(T)));
                    case 1: return Ex.Num(L.F("Um motor com {0} kg/s de ar, que entra a {1} m/s, produz {2} N de empuxo. A que velocidade, em m/s, o ar sai?", N(md), N(v0), N(T)), ve, "m/s",
                        L.F("Ve = V₀ + T/ṁ = {0} + {1}/{2} = {3} m/s.", N(v0), N(T), N(md), N(ve)));
                    default: return Ex.Num(L.F("Para dar {0} N de empuxo acelerando o ar de {1} m/s para {2} m/s, quantos kg/s de ar o motor precisa processar?", N(T), N(v0), N(ve)), md, "kg/s",
                        L.F("ṁ = T / (Ve − V₀) = {0} / ({1} − {2}) = {3} kg/s.", N(T), N(ve), N(v0), N(md)));
                }
            case Difficulty.Medium:
            {
                double eta = 2 / (1 + ve / v0), Pu = T * v0;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um jato expele o ar a {0} m/s com o avião a {1} m/s. Qual a eficiência propulsiva η = 2 / (1 + Ve/V₀), em %?", N(ve), N(v0)), eta * 100, "%",
                        L.F("η = 2 / (1 + {0}/{1}) = {2}%.", N(ve), N(v0), N(eta * 100)));
                return Ex.Num(L.F("Um motor dá {0} N de empuxo com o avião a {1} m/s. Qual a potência útil (T·V₀), em kW?", N(T), N(v0)), Pu / 1000, "kW",
                    L.F("P = T·V₀ = {0}·{1} = {2} kW.", N(T), N(v0), N(Pu / 1000)));
            }
            case Difficulty.Hard:
            {
                double pj = md * (ve * ve - v0 * v0) / 2, Pu = T * v0;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um jato acelera {0} kg/s de ar de {1} m/s para {2} m/s. Que potência cinética, em kW, o jato transfere ao ar? (ṁ·(Ve² − V₀²)/2)", N(md), N(v0), N(ve)), pj / 1000, "kW",
                        L.F("P = ṁ·(Ve² − V₀²)/2 = {0}·({1}² − {2}²)/2 = {3} kW.", N(md), N(ve), N(v0), N(pj / 1000)));
                    default: return Ex.Num(L.F("Um jato acelera {0} kg/s de ar de {1} m/s para {2} m/s. Que fração da potência cinética dada ao ar vira potência útil de propulsão (T·V₀), em %?", N(md), N(v0), N(ve)), Pu / pj * 100, "%",
                        L.F("T = {0} N; T·V₀ = {1} kW; potência dada ao ar = {2} kW; fração = {3}% (= 2/(1 + Ve/V₀)).", N(T), N(Pu / 1000), N(pj / 1000), N(Pu / pj * 100)));
                }
            }
            default:
            {
                double k = Ex.Pick(r, 2.0, 3.0, 4.0, 5.0), ve2 = v0 + (ve - v0) / k, eta1 = 2 / (1 + ve / v0), eta2 = 2 / (1 + ve2 / v0);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Dois motores dão o mesmo empuxo a {0} m/s de voo. O motor A processa {1} kg/s e expele o ar a {2} m/s. O motor B processa {3} vezes mais ar. Qual a velocidade de saída do motor B, em m/s?", N(v0), N(md), N(ve), N(k, 0)), ve2, "m/s",
                        L.F("Mesmo empuxo T = ṁ·(Ve − V₀): Ve_B = V₀ + (Ve_A − V₀)/{0} = {1} + ({2} − {1})/{0} = {3} m/s.", N(k, 0), N(v0), N(ve), N(ve2)));
                return Ex.Num(L.F("Dois motores dão o mesmo empuxo a {0} m/s de voo. O motor A expele o ar a {1} m/s. O motor B processa {2} vezes mais ar (e por isso o expele mais devagar). Quantos pontos percentuais a eficiência propulsiva de B é maior que a de A?", N(v0), N(ve), N(k, 0)), (eta2 - eta1) * 100, "pp",
                    L.F("Mesmo empuxo: Ve_B = V₀ + (Ve_A − V₀)/{0} = {1} m/s. η = 2/(1 + Ve/V₀): A = {2}%, B = {3}%; diferença = {4} pontos.", N(k, 0), N(ve2), N(eta1 * 100), N(eta2 * 100), N((eta2 - eta1) * 100)), 3);
            }
        }
    }
}

[Exercises("aero-08-helicoptero-pairar")]
public sealed class Air08 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double m = Ex.Step(r, 600, 6000, 50), R = Ex.Step(r, 3, 9, 0.25), rho = Isa.Rho(Ex.Pick(r, 0.0, 500, 1000, 1500)), A = Math.PI * R * R, T = m * G;
        double vi = Math.Sqrt(T / (2 * rho * A)), Pid = T * vi, fm = Ex.Step(r, 0.6, 0.75, 0.01);
        string heli = L.F("Um helicóptero de {0} kg, rotor de raio {1} m, em ar de ρ = {2} kg/m³ (g = 9,8 m/s²).", N(m), N(R), N(rho, 3));
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(heli + " " + L.T("Qual a área do disco do rotor, em m²?"), A, "m²",
                        L.F("A = π·R² = π·{0}² = {1} m².", N(R), N(A)));
                return Ex.Num(heli + " " + L.T("Qual a carga de disco (empuxo / área), em N/m², no pairado?"), T / A, "N/m²",
                    L.F("T = m·g = {0} N; carga de disco = T/A = {0}/{1} = {2} N/m².", N(T), N(A), N(T / A)));
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(heli + " " + L.T("Qual a velocidade induzida no pairado, em m/s? (vi = √(T / (2·ρ·A)))"), vi, "m/s",
                        L.F("T = {0} N; A = {1} m²; vi = √({0} / (2·{2}·{1})) = {3} m/s.", N(T), N(A), N(rho, 3), N(vi)));
                return Ex.Num(heli + " " + L.T("Qual a potência ideal para pairar, em kW? (P = T·vi)"), Pid / 1000, "kW",
                    L.F("vi = {0} m/s; P = T·vi = {1}·{0} = {2} kW.", N(vi), N(T), N(Pid / 1000)));
            case Difficulty.Hard:
            {
                double Pr = Pid / fm;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(heli + " " + L.F("O figure of merit é {0}. Qual a potência real no eixo para pairar, em kW?", N(fm)), Pr / 1000, "kW",
                        L.F("P_ideal = T^1,5/√(2·ρ·A) = {0} kW; P_real = P_ideal / FM = {0} / {1} = {2} kW.", N(Pid / 1000), N(fm), N(Pr / 1000)));
                    case 1:
                    {
                        double Pshaft = Math.Round(Pr / 1000 / 10) * 10;
                        return Ex.Num(heli + " " + L.F("O motor entrega {0} kW ao rotor para pairar. Qual o figure of merit?", N(Pshaft)), Pid / 1000 / Pshaft, "",
                            L.F("P_ideal = {0} kW; FM = P_ideal / P_real = {0} / {1} = {2}.", N(Pid / 1000), N(Pshaft), N(Pid / 1000 / Pshaft, 3)), 3);
                    }
                    default: return Ex.Num(heli + " " + L.F("Com FM = {0}, quantos kg de massa o helicóptero consegue pairar com {1} kW de potência no eixo (mesmo rotor e ar)?", N(fm), N(Pr / 1000)), m, "kg",
                        L.F("P_ideal = FM·P = {0} kW = T^1,5/√(2ρA) → T = (P_ideal·√(2ρA))^(2/3) = {1} N → m = T/g = {2} kg.", N(Pid / 1000), N(T), N(m)), 3);
                }
            }
            default:
            {
                double k = Ex.Pick(r, 1.2, 1.5, 2.0), kr = Ex.Pick(r, 1.2, 1.5, 2.0);
                if (r.Next(2) == 0)
                    return Ex.Num(heli + " " + L.F("Quantos kW de potência ideal ele precisa para pairar com a massa multiplicada por {0}? (mesmo rotor e ar)", N(k)), Pid * Math.Pow(k, 1.5) / 1000, "kW",
                        L.F("P ∝ T^1,5: {0} kW × {1}^1,5 = {2} kW.", N(Pid / 1000), N(k), N(Pid * Math.Pow(k, 1.5) / 1000)));
                return Ex.Num(heli + " " + L.F("Quantos kW de potência ideal ele precisa para pairar com o raio do rotor multiplicado por {0}? (mesma massa e ar)", N(kr)), Pid / kr / 1000, "kW",
                    L.F("P ∝ 1/√A ∝ 1/R: {0} kW / {1} = {2} kW.", N(Pid / 1000), N(kr), N(Pid / kr / 1000)));
            }
        }
    }
}

[Exercises("aero-09-cauda-autorrotacao")]
public sealed class Air09 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double P = Ex.Step(r, 100, 800, 25) * 1000, rpm = Ex.Step(r, 300, 500, 10), w = rpm * 2 * Math.PI / 60, Q = P / w, arm = Ex.Step(r, 4, 8, 0.25), Tt = Q / arm;
        string rot = L.F("O rotor principal de um helicóptero gira a {0} rpm e recebe {1} kW.", N(rpm), N(P / 1000));
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(rot + " " + L.T("Qual a velocidade angular, em rad/s?"), w, "rad/s",
                        L.F("Ω = rpm·2π/60 = {0}·2π/60 = {1} rad/s.", N(rpm), N(w)));
                return Ex.Num(rot + " " + L.T("Qual o torque no eixo do rotor, em N·m?"), Q, "N·m",
                    L.F("Ω = {0} rad/s; Q = P/Ω = {1}/{0} = {2} N·m.", N(w), N(P), N(Q)));
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(rot + " " + L.F("O rotor de cauda está a {0} m do eixo. Qual o empuxo necessário na cauda para anular o torque, em N?", N(arm)), Tt, "N",
                        L.F("Q = P/Ω = {0} N·m; T_cauda = Q / braço = {0}/{1} = {2} N.", N(Q), N(arm), N(Tt)));
                return Ex.Num(rot + " " + L.F("Para anular o torque, a cauda dá {0} N de empuxo. A que distância do eixo, em m, ela está?", N(Tt)), arm, "m",
                    L.F("Q = P/Ω = {0} N·m; braço = Q / T = {0}/{1} = {2} m.", N(Q), N(Tt), N(arm)));
            case Difficulty.Hard:
            {
                double Tmax = Ex.Step(r, 0.8, 1.3, 0.05) * Tt, Pmax = Tmax * arm * w;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(rot + " " + L.F("O rotor de cauda, a {0} m, dá no máximo {1} N. Qual a máxima potência, em kW, que o rotor principal pode ter sem a cauda perder o controle?", N(arm), N(Tmax)), Pmax / 1000, "kW",
                            L.F("Q_máx = T·braço = {0}·{1} = {2} N·m; P = Q·Ω = {2}·{3} = {4} kW.", N(Tmax), N(arm), N(Tmax * arm), N(w), N(Pmax / 1000)));
                    case 1: return Ex.Num(rot + " " + L.F("A cauda está a {0} m e dá no máximo {1} N. Que margem sobra de empuxo, em % do máximo, para a potência atual?", N(arm), N(Tmax)), (1 - Tt / Tmax) * 100, "%",
                            L.F("Necessário: Q/braço = {0} N de {1} N; margem = 1 − {0}/{1} = {2}%.", N(Tt), N(Tmax), N((1 - Tt / Tmax) * 100)), 3);
                    default:
                    {
                        double rpm2 = rpm * Ex.Pick(r, 0.9, 1.1), Q2 = P / (rpm2 * 2 * Math.PI / 60);
                        return Ex.Num(rot + " " + L.F("Se o rotor passa a girar a {0} rpm com a mesma potência, qual o novo torque, em N·m?", N(rpm2)), Q2, "N·m",
                            L.F("Q = P / Ω = {0} / ({1}·2π/60) = {2} N·m.", N(P), N(rpm2), N(Q2)));
                    }
                }
            }
            default:
            {
                double m = Ex.Step(r, 800, 2500, 50), R = Ex.Step(r, 3.5, 6.5, 0.25), rho = 1.225, A = Math.PI * R * R, vd = Math.Sqrt(2 * m * 9.8 / (rho * A * 1.2));
                double inertia = m * 0.12 * R * R / 8 * 3, Ee = 0.5 * inertia * w * w * (1 - 0.49);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Em autorrotação, um helicóptero de {0} kg, com rotor de raio {1} m (A = π·R²), desce a v ≈ √(2·W / (ρ·A·1,2)). Com ρ = 1,225 kg/m³ e g = 9,8 m/s², qual a velocidade de descida, em m/s?", N(m), N(R)), vd, "m/s",
                        L.F("W = {0} N; A = {1} m²; v = √(2·{0} / (1,225·{1}·1,2)) = {2} m/s.", N(m * 9.8), N(A), N(vd)), 3);
                double hh = Ex.Step(r, 300, 1200, 50), tdesc = hh / vd;
                return Ex.Num(L.F("Em autorrotação, um helicóptero de {0} kg, rotor de raio {1} m, desce a v ≈ √(2·W / (ρ·A·1,2)) (ρ = 1,225 kg/m³, g = 9,8 m/s²). Quantos segundos leva para descer {2} m?", N(m), N(R), N(hh)), tdesc, "s",
                    L.F("v = {0} m/s; t = h / v = {1} / {0} = {2} s.", N(vd), N(hh), N(tdesc)), 3);
            }
        }
    }
}

[Exercises("aero-10-projeto-breguet")]
public sealed class Air10 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double w0 = Ex.Step(r, 800, 4000, 50), ff = Ex.Step(r, 0.08, 0.3, 0.01), w1 = w0 * (1 - ff), ld = Ex.Step(r, 8, 18, 0.5), eta = Ex.Step(r, 0.7, 0.85, 0.01), bsfc = Ex.Step(r, 230, 320, 5), c = bsfc / 3.6e9;
        double range = eta / (G * c) * ld * Math.Log(w0 / w1);
        string ac = L.F("Um avião a hélice (η_hélice = {0}, consumo específico {1} g/kWh, L/D = {2}).", N(eta), N(bsfc, 0), N(ld));
        switch (d)
        {
            case Difficulty.Easy:
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um avião decola com {0} kg e termina o voo com {1} kg. Qual a fração do peso inicial que era combustível, em %?", N(w0), N(w1)), ff * 100, "%",
                        L.F("(W₀ − W₁)/W₀ = ({0} − {1})/{0} = {2}%.", N(w0), N(w1), N(ff * 100)));
                    default: return Ex.Num(L.F("Um avião decola com {0} kg e queima {1} kg de combustível. Qual a razão W₀/W₁?", N(w0), N(w0 - w1)), w0 / w1, "",
                        L.F("W₁ = {0} − {1} = {2} kg; W₀/W₁ = {3}.", N(w0), N(w0 - w1), N(w1), N(w0 / w1, 3)), 3);
                }
            case Difficulty.Medium:
                return Ex.Num(ac + " " + L.F("Ele decola com {0} kg e pousa com {1} kg. Qual o alcance de Breguet, em km? (g = 9,8 m/s²)", N(w0), N(w1)), range / 1000, "km",
                    L.F("c = BSFC / 3,6·10⁹ = {0} kg/J; R = (η/(g·c))·(L/D)·ln(W₀/W₁) = {1} km.", N(c * 1e8, 3) + "·10⁻⁸", N(range / 1000)), 3);
            case Difficulty.Hard:
            {
                double rg = Ex.Step(r, 500, 2500, 100) * 1000, x = rg * G * c / (eta * ld), fuel = w0 * (1 - Math.Exp(-x));
                if (r.Next(2) == 0)
                    return Ex.Num(ac + " " + L.F("Quantos kg de combustível ele precisa para {0} km, decolando com {1} kg? (g = 9,8 m/s²)", N(rg / 1000), N(w0)), fuel, "kg",
                        L.F("ln(W₀/W₁) = R·g·c / (η·L/D) = {0}; W₁ = W₀·e^(−{0}) = {1} kg; combustível = {2} kg.", N(x, 4), N(w0 - fuel), N(fuel)), 3);
                double k = Ex.Step(r, 0.1, 0.3, 0.05), range2 = range * (1 + k);
                return Ex.Num(ac + " " + L.F("Decola com {0} kg e pousa com {1} kg. Se o L/D melhora {2}% (mesmos pesos), qual o novo alcance, em km? (g = 9,8 m/s²)", N(w0), N(w1), N(k * 100, 0)), range2 / 1000, "km",
                    L.F("R ∝ L/D: {0} km × (1 + {1}) = {2} km.", N(range / 1000), N(k), N(range2 / 1000)), 3);
            }
            default:
            {
                double extra = Ex.Step(r, 20, 100, 10), range2 = eta / (G * c) * ld * Math.Log((w0) / (w1 - 0));
                // payload trade: swap extra kg of payload for fuel, or add payload keeping fuel.
                double r2 = eta / (G * c) * ld * Math.Log((w0 + extra) / (w1 + extra));
                if (r.Next(2) == 0)
                    return Ex.Num(ac + " " + L.F("Decola com {0} kg e pousa com {1} kg. Se carregar mais {2} kg de carga (mesmo combustível), quantos km de alcance perde? (g = 9,8 m/s²)", N(w0), N(w1), N(extra)), (range - r2) / 1000, "km",
                        L.F("Antes: ln({0}/{1}) → {2} km. Depois: ln({3}/{4}) → {5} km. Perda: {6} km.", N(w0), N(w1), N(range / 1000), N(w0 + extra), N(w1 + extra), N(r2 / 1000), N((range - r2) / 1000)), 3);
                double fuelAdd = extra, r3 = eta / (G * c) * ld * Math.Log((w0 + fuelAdd) / w1);
                return Ex.Num(ac + " " + L.F("Decola com {0} kg e pousa com {1} kg. Se colocar mais {2} kg de combustível (a massa final não muda), quantos km de alcance ganha? (g = 9,8 m/s²)", N(w0), N(w1), N(fuelAdd)), (r3 - range) / 1000, "km",
                    L.F("Antes: {0} km. Com W₀ = {1} kg: {2} km. Ganho: {3} km.", N(range / 1000), N(w0 + fuelAdd), N(r3 / 1000), N((r3 - range) / 1000)), 3);
            }
        }
    }
}
