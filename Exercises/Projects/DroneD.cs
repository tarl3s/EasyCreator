using static EasyCreator.Exercises.Pv;

namespace EasyCreator.Exercises;

/// <summary>Drones 16-20: sensor fusion, heavy attitude control, best-range speed, rotor theory, the mapping drone.</summary>
[Exercises("drone-16-fusao-sensores")]
public sealed class Drone16 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double tau = Ex.Step(r, 0.1, 2, 0.05), dt = Ex.Pick(r, 0.002, 0.005, 0.01, 0.02), alpha = tau / (tau + dt);
        double p = Ex.Step(r, 0.5, 10, 0.5), rn = Ex.Step(r, 0.5, 10, 0.5), k = p / (p + rn);
        double est = Ex.Step(r, -10, 30, 1), meas = est + Ex.Step(r, -5, 5, 0.5);
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um filtro complementar com τ = {0} s roda a cada dt = {1} s. Qual o α = τ/(τ + dt)?", N(tau), N(dt, 3)), alpha, "",
                        L.F("α = {0} / ({0} + {1}) = {2}.", N(tau), N(dt, 3), N(alpha, 4)), 3);
                return Ex.Num(L.F("Num filtro de Kalman escalar, a incerteza da estimativa é P = {0} e a do sensor é R = {1}. Qual o ganho K = P/(P + R)?", N(p), N(rn)), k, "",
                    L.F("K = {0} / ({0} + {1}) = {2}.", N(p), N(rn), N(k, 4)), 3);
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um filtro de Kalman com ganho K = {0} tem estimativa {1}° e o sensor mede {2}°. Qual a nova estimativa, em graus? (nova = estimativa + K·(medida − estimativa))", N(k, 3), N(est), N(meas)), est + k * (meas - est), "°",
                        L.F("nova = {0} + {1}·({2} − {0}) = {3}°.", N(est), N(k, 3), N(meas), N(est + k * (meas - est), 3)), 3);
                {
                    double acc = est + Ex.Step(r, -5, 5, 0.5), gyro = Ex.Step(r, -30, 30, 1), upd = alpha * (est + gyro * dt) + (1 - alpha) * acc;
                    return Ex.Num(L.F("Filtro complementar com α = {0}: ângulo {1}°, giroscópio {2} °/s, dt = {3} s, acelerômetro {4}°. Qual o novo ângulo, em graus?", N(alpha, 4), N(est), N(gyro), N(dt, 3), N(acc)), upd, "°",
                        L.F("novo = α·(ângulo + giro·dt) + (1 − α)·acelerômetro = {0}·({1} + {2}·{3}) + {4}·{5} = {6}°.", N(alpha, 4), N(est), N(gyro), N(dt, 3), N(1 - alpha, 4), N(acc), N(upd, 3)), 3);
                }
            case Difficulty.Hard:
            {
                double pn = (1 - k) * p;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Num filtro de Kalman escalar com P = {0} e R = {1}, depois de uma atualização a incerteza vira P' = (1 − K)·P. Qual o P'?", N(p), N(rn)), pn, "",
                        L.F("K = {0}/({0} + {1}) = {2}; P' = (1 − {2})·{0} = {3}.", N(p), N(rn), N(k, 4), N(pn, 4)), 3);
                    default:
                    {
                        double m2 = est + Ex.Step(r, -4, 4, 0.5), e1 = est + k * (meas - est), k2 = pn / (pn + rn), e2 = e1 + k2 * (m2 - e1);
                        return Ex.Num(L.F("Kalman escalar: estimativa inicial {0}°, P = {1}, R = {2}. Primeira medida {3}°, segunda medida {4}° (sem mudança de estado entre elas). Qual a estimativa após as duas atualizações, em graus? (K = P/(P + R), P' = (1 − K)·P)", N(est), N(p), N(rn), N(meas), N(m2)), e2, "°",
                            L.F("1ª: K = {0}, estimativa = {1}°, P' = {2}. 2ª: K = {3}, estimativa = {4}°.", N(k, 4), N(e1, 3), N(pn, 4), N(k2, 4), N(e2, 3)), 3);
                    }
                }
            }
            default:
            {
                double fc = Ex.Step(r, 0.2, 5, 0.1), tauF = 1 / (2 * Math.PI * fc), alF = tauF / (tauF + dt);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um filtro complementar deve cruzar giroscópio e acelerômetro em {0} Hz (τ = 1/(2π·f)) e roda a cada dt = {1} s. Qual o α = τ/(τ + dt)?", N(fc), N(dt, 3)), alF, "",
                        L.F("τ = 1/(2π·{0}) = {1} s; α = {1}/({1} + {2}) = {3}.", N(fc), N(tauF, 4), N(dt, 3), N(alF, 4)), 3);
                double q = Ex.Step(r, 0.01, 1, 0.01), pp = 1.0, kk = 0;
                for (int i = 0; i < 200; i++) { double pm = pp + q; kk = pm / (pm + rn); pp = (1 - kk) * pm; }
                return Ex.Num(L.F("Kalman escalar com ruído de processo Q = {0} e ruído do sensor R = {1}. A cada passo: P⁻ = P + Q; K = P⁻/(P⁻ + R); P = (1 − K)·P⁻. Qual o ganho K em regime (depois de muitos passos)?", N(q), N(rn)), kk, "",
                    L.F("Repetindo as três contas até estabilizar: K = {0} (quanto mais ruído do sensor R, menor o K: confia-se menos na medida).", N(kk, 4)), 3);
            }
        }
    }
}

[Exercises("drone-17-atitude-pesado")]
public sealed class Drone17 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double dIn = Ex.Step(r, 5, 30, 1), tau = 0.03 * Math.Pow(dIn / 5, 1.5), m = Ex.Step(r, 1, 25, 0.5), La = Ex.Step(r, 0.2, 1.0, 0.05), inertia = 0.4 * m * La * La;
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("O motor de um drone com hélice de {0}\" tem constante de tempo τ ≈ 0,03·(D/5\")^1,5 s. Qual o τ, em s?", N(dIn, 0)), tau, "s",
                        L.F("τ = 0,03 · ({0}/5)^1,5 = {1} s.", N(dIn, 0), N(tau, 4)), 3);
                return Ex.Num(L.F("O momento de inércia de um drone é I ≈ 0,4·m·L². Para m = {0} kg e braço L = {1} m, qual o I, em kg·m²?", N(m), N(La)), inertia, "kg·m²",
                    L.F("I = 0,4·{0}·{1}² = {2} kg·m².", N(m), N(La), N(inertia, 4)), 3);
            case Difficulty.Medium:
            {
                double f = Ex.Step(r, 1, 20, 1), w = 2 * Math.PI * f, lag = Math.Atan(w * tau) * 180 / Math.PI;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um motor com constante de tempo τ = {0} s recebe um comando em degrau. Quanto tempo, em s, leva para chegar a 95% do valor final? (≈ 3·τ)", N(tau, 4)), 3 * tau, "s",
                            L.F("t ≈ 3·τ = 3·{0} = {1} s.", N(tau, 4), N(3 * tau, 4)), 3);
                    default: return Ex.Num(L.F("Um motor com τ = {0} s recebe um comando senoidal de {1} Hz. Qual o atraso de fase, em graus? (φ = atan(2π·f·τ))", N(tau, 4), N(f)), lag, "°",
                        L.F("ω = 2π·{0} = {1} rad/s; φ = atan({1}·{2}) = {3}°.", N(f), N(w), N(tau, 4), N(lag)), 3);
                }
            }
            default:
            {
                double dT = Ex.Step(r, 0.5, 10, 0.5), arm = Ex.Step(r, 0.1, 0.6, 0.02);
                if (d == Difficulty.Hard)
                {
                    double torque = 2 * dT * arm, alpha = torque / inertia;
                    if (r.Next(2) == 0)
                        return Ex.Num(L.F("Num hexa de {0} kg com braço de {1} m (I ≈ 0,4·m·L²), os motores de um lado ganham ΔT = {2} N e os do outro perdem ΔT, a {3} m do eixo (duas forças, torque = 2·ΔT·d). Qual a aceleração angular, em rad/s²?", N(m), N(La), N(dT), N(arm)), alpha, "rad/s²",
                            L.F("τ = 2·ΔT·d = {0} N·m; I = {1}; α = τ/I = {2} rad/s².", N(torque, 3), N(inertia, 4), N(alpha, 3)), 3);
                    return Ex.Num(L.F("Um hexa de {0} kg (braço {1} m, I ≈ 0,4·m·L²) com torque de {2} N·m. Quanto vale o ângulo, em graus, após 0,2 s a partir do repouso (θ = ½·α·t²)?", N(m), N(La), N(torque)), 0.5 * alpha * 0.04 * 180 / Math.PI, "°",
                        L.F("α = {0} rad/s²; θ = ½·{0}·0,2² = {1} rad = {2}°.", N(alpha, 3), N(0.5 * alpha * 0.04, 4), N(0.5 * alpha * 0.04 * 180 / Math.PI)), 3);
                }
                double torque2 = 2 * dT * arm, alpha2 = torque2 / inertia, th = Ex.Step(r, 10, 40, 5), thr = th * Math.PI / 180, tIdeal = Math.Sqrt(2 * thr / alpha2), tTot = tIdeal + tau;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um hexa de {0} kg (braço {1} m, I ≈ 0,4·m·L²) com hélice de {2}\" recebe um torque de {3} N·m. Sem atraso, o tempo para girar {4}° é √(2θ/α). Somando o atraso do motor (τ ≈ 0,03·(D/5\")^1,5 s), qual o tempo total, em s?", N(m), N(La), N(dIn, 0), N(torque2, 3), N(th)), tTot, "s",
                        L.F("α = τ_torque/I = {0} rad/s²; t = √(2θ/α) = {1} s; atraso do motor = {2} s; total = {3} s.", N(alpha2, 3), N(tIdeal, 4), N(tau, 4), N(tTot, 4)), 3);
                double ratio = tau / tIdeal * 100;
                return Ex.Num(L.F("Um hexa de {0} kg (braço {1} m, I ≈ 0,4·m·L²) com hélice de {2}\" recebe um torque de {3} N·m. Que fração do tempo para girar {4}° (√(2θ/α)) o atraso do motor (τ ≈ 0,03·(D/5\")^1,5 s) representa, em %?", N(m), N(La), N(dIn, 0), N(torque2, 3), N(th)), ratio, "%",
                    L.F("t = {0} s; τ = {1} s; τ/t = {2}%.", N(tIdeal, 4), N(tau, 4), N(ratio)), 3);
            }
        }
    }
}

[Exercises("drone-18-velocidade-alcance")]
public sealed class Drone18 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double P = Ex.Step(r, 60, 500, 10), v = Ex.Step(r, 4, 25, 1), whkm = P / v / 3.6, cap = Ex.Step(r, 30, 300, 5);
        double p0 = Ex.Step(r, 80, 400, 10), k = Ex.Step(r, 0.005, 0.05, 0.005), vopt = Math.Cbrt(p0 / (2 * k)), Pvv = p0 + k * v * v * v, Pvo = p0 + k * vopt * vopt * vopt;
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone voa a {0} m/s gastando {1} W. Quantos Wh por quilômetro?", N(v), N(P)), whkm, "Wh/km",
                        L.F("Wh/km = P / v × 1000 / 3600 = {0} / {1} × 1000 / 3600 = {2}.", N(P), N(v), N(whkm, 3)), 3);
                return Ex.Num(L.F("Um drone voa a {0} m/s por {1} min. Que distância percorre, em km?", N(v), N(cap / 10)), v * cap / 10 * 60 / 1000, "km",
                    L.F("d = {0} × {1}·60 / 1000 = {2} km.", N(v), N(cap / 10), N(v * cap / 10 * 60 / 1000)), 3);
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone com {0} Wh utilizáveis gasta {1} Wh/km. Qual o alcance, em km?", N(cap), N(whkm, 3)), cap / whkm, "km",
                        L.F("alcance = {0} / {1} = {2} km.", N(cap), N(whkm, 3), N(cap / whkm)), 3);
                return Ex.Num(L.F("Uma bateria de {0} Wh (80% utilizável) alimenta um drone que gasta {1} W a {2} m/s. Qual o alcance, em km?", N(cap), N(P), N(v)), 0.8 * cap / whkm, "km",
                    L.F("Wh/km = {0}; alcance = 0,8·{1} / {0} = {2} km.", N(whkm, 3), N(cap), N(0.8 * cap / whkm)), 3);
            case Difficulty.Hard:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("A potência de um drone é P(v) = {0} + {1}·v³ (W, v em m/s). Qual a potência a {2} m/s, em W?", N(p0), N(k, 3), N(v)), Pvv, "W",
                        L.F("P = {0} + {1}·{2}³ = {3} W.", N(p0), N(k, 3), N(v), N(Pvv)), 3);
                return Ex.Num(L.F("A potência de um drone é P(v) = {0} + {1}·v³ (W, v em m/s). Em que velocidade, em m/s, o consumo por km é mínimo? (dica: v³ = P₀ / (2k))", N(p0), N(k, 3)), vopt, "m/s",
                    L.F("Wh/km ∝ P(v)/v = P₀/v + k·v²; derivando: −P₀/v² + 2k·v = 0 → v = (P₀/(2k))^(1/3) = ({0}/(2·{1}))^(1/3) = {2} m/s.", N(p0), N(k, 3), N(vopt)), 3);
            default:
            {
                double eOpt = Pvo / vopt / 3.6, eAt = Pvv / v / 3.6;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("A potência de um drone é P(v) = {0} + {1}·v³. Com uma bateria de {2} Wh utilizáveis, qual o alcance máximo, em km? (use a velocidade de menor Wh/km, v³ = P₀/(2k))", N(p0), N(k, 3), N(cap)), cap / eOpt, "km",
                        L.F("v = {0} m/s; P = {1} W; Wh/km = {1}/{0}/3,6 = {2}; alcance = {3}/{2} = {4} km.", N(vopt, 3), N(Pvo), N(eOpt, 3), N(cap), N(cap / eOpt)), 3);
                return Ex.Num(L.F("A potência de um drone é P(v) = {0} + {1}·v³. Quantos por cento de energia por km se economiza voando na velocidade ideal (v³ = P₀/(2k)) em vez de {2} m/s?", N(p0), N(k, 3), N(v)), (1 - eOpt / eAt) * 100, "%",
                    L.F("Ideal: v = {0} m/s → {1} Wh/km; a {2} m/s → {3} Wh/km; economia = {4}%.", N(vopt, 3), N(eOpt, 3), N(v), N(eAt, 3), N((1 - eOpt / eAt) * 100)), 3);
            }
        }
    }
}

[Exercises("drone-19-teoria-rotor")]
public sealed class Drone19 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double Rho = 1.2;
        double dIn = Ex.Step(r, 5, 30, 1), A = Mr.PropArea(dIn), T = Ex.Step(r, 2, 80, 1), vi = Math.Sqrt(T / (2 * Rho * A)), Pid = T * vi, dl = T / A, fm = Ex.Step(r, 0.5, 0.7, 0.01);
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma hélice de {0}\" gera {1} N de empuxo. Qual a carga de disco (T/A), em N/m²?", N(dIn, 0), N(T)), dl, "N/m²",
                        L.F("A = {0} m²; T/A = {1} / {0} = {2} N/m².", N(A, 5), N(T), N(dl)), 3);
                return Ex.Num(L.F("Uma hélice de {0}\" gera {1} N (ρ = 1,2 kg/m³). Qual a velocidade induzida vi = √(T/(2ρA)), em m/s?", N(dIn, 0), N(T)), vi, "m/s",
                    L.F("A = {0} m²; vi = √({1}/(2·1,2·{0})) = {2} m/s.", N(A, 5), N(T), N(vi)), 3);
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma hélice de {0}\" gera {1} N (ρ = 1,2 kg/m³). Qual a potência ideal T·vi, em W?", N(dIn, 0), N(T)), Pid, "W",
                        L.F("vi = {0} m/s; P = T·vi = {1}·{0} = {2} W.", N(vi, 3), N(T), N(Pid)), 3);
                return Ex.Num(L.F("Uma hélice de {0}\" gera {1} N com figure of merit {2} (ρ = 1,2 kg/m³). Qual a potência real, em W?", N(dIn, 0), N(T), N(fm)), Pid / fm, "W",
                    L.F("P_ideal = {0} W; P_real = P_ideal / FM = {0} / {1} = {2} W.", N(Pid), N(fm), N(Pid / fm)), 3);
            case Difficulty.Hard:
            {
                double d2 = dIn * Ex.Step(r, 1.2, 2.0, 0.1), A2 = Mr.PropArea(d2), P2 = Mr.IdealP(T, A2);
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Uma hélice de {0}\" gera {1} N. Quantos por cento a potência ideal diminui ao trocar por uma de {2}\" para o mesmo empuxo?", N(dIn, 0), N(T), N(d2, 1)), (1 - P2 / Pid) * 100, "%",
                            L.F("P ∝ 1/√A ∝ 1/D: {0} W → {1} W; redução = {2}%.", N(Pid), N(P2), N((1 - P2 / Pid) * 100)), 3);
                    default:
                    {
                        double vInf = Ex.Step(r, 0.3, 0.9, 0.1) * vi;
                        return Ex.Num(L.F("Uma hélice de {0}\" gera {1} N (ρ = 1,2 kg/m³). Descer na vertical a {2} m/s é perigoso (anel de vórtice) quando a velocidade de descida é parecida com a velocidade induzida. Qual a razão entre a velocidade de descida e vi?", N(dIn, 0), N(T), N(vInf)), vInf / vi, "",
                            L.F("vi = {0} m/s; razão = {1} / {0} = {2}.", N(vi, 3), N(vInf), N(vInf / vi, 3)), 3);
                    }
                }
            }
            default:
            {
                int n = Ex.Pick(r, 4, 6, 8);
                double m = Ex.Step(r, 1, 12, 0.5), T1 = m * 9.8 / n, A1 = A, vi1 = Math.Sqrt(T1 / (2 * Rho * A1)), Pt = n * Mr.IdealP(T1, A1) / fm;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone de {0} kg com {1} hélices de {2}\" paira (ρ = 1,2 kg/m³, g = 9,8 m/s²). Qual a velocidade induzida em cada rotor, em m/s? (a descida vertical perigosa fica por volta desse valor)", N(m), N(n, 0), N(dIn, 0)), vi1, "m/s",
                        L.F("T por rotor = {0} N; vi = √(T/(2ρA)) = √({0}/(2·1,2·{1})) = {2} m/s.", N(T1, 3), N(A1, 5), N(vi1)), 3);
                return Ex.Num(L.F("Um drone de {0} kg com {1} hélices de {2}\" paira com figure of merit {3} (ρ = 1,2 kg/m³, g = 9,8 m/s²). Qual a potência total real, em W?", N(m), N(n, 0), N(dIn, 0), N(fm)), Pt, "W",
                    L.F("T por rotor = {0} N; P_ideal por rotor = {1} W; total ideal = {2} W; real = ideal / FM = {3} W.", N(T1, 3), N(Mr.IdealP(T1, A1)), N(n * Mr.IdealP(T1, A1)), N(Pt)), 3);
            }
        }
    }
}

[Exercises("drone-20-projeto-mapeamento")]
public sealed class Drone20 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double dist = Ex.Step(r, 2, 10, 0.5) * 1000, v = Ex.Step(r, 8, 18, 1), P = Ex.Step(r, 120, 320, 10), map = Ex.Step(r, 5, 20, 1) * 60;
        double tleg = dist / v, e = P * (2 * tleg + map) / 3600;
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um ponto de mapeamento fica a {0} m. A {1} m/s, quanto tempo leva ida e volta, em minutos?", N(dist), N(v)), 2 * tleg / 60, "min",
                        L.F("t = 2·{0} / {1} = {2} s = {3} min.", N(dist), N(v), N(2 * tleg), N(2 * tleg / 60)), 3);
                return Ex.Num(L.F("Um drone gasta {0} W e voa {1} min no total. Quantos Wh gasta?", N(P), N(2 * tleg / 60 + map / 60)), e, "Wh",
                    L.F("E = P·t = {0} × {1}·60 / 3600 = {2} Wh.", N(P), N(2 * tleg / 60 + map / 60), N(e)), 3);
            case Difficulty.Medium:
            {
                double La = Ex.Step(r, 500, 3000, 100), W = Ex.Step(r, 300, 1500, 100), s = Ex.Step(r, 40, 120, 5), lines = Math.Ceiling(W / s), lenKm = lines * La / 1000;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Para mapear uma área de {0} m × {1} m com faixas de {2} m de largura (voando ao longo dos {0} m), quantas linhas de voo são necessárias? (arredonde para cima)", N(La), N(W), N(s)), lines, "",
                        L.F("{0} / {1} = {2} → {3} linhas.", N(W), N(s), N(W / s, 2), N(lines, 0)));
                return Ex.Num(L.F("Para mapear uma área de {0} m × {1} m com faixas de {2} m (linhas ao longo dos {0} m), qual o comprimento total das linhas de voo, em km? (número de linhas arredondado para cima)", N(La), N(W), N(s)), lenKm, "km",
                    L.F("linhas = ⌈{0}/{1}⌉ = {2}; comprimento = {2}·{3} m = {4} km.", N(W), N(s), N(lines, 0), N(La), N(lenKm)));
            }
            case Difficulty.Hard:
            {
                int cells = Ex.Pick(r, 4, 6);
                double need = e / 0.8, mah = need / (cells * 3.7) * 1000;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Missão: ir {0} m a {1} m/s, mapear {2} s e voltar, a {3} W o tempo todo. Qual a energia, em Wh?", N(dist), N(v), N(map, 0), N(P)), e, "Wh",
                            L.F("t = 2·{0}/{1} + {2} = {3} s; E = {4}·{3}/3600 = {5} Wh.", N(dist), N(v), N(map, 0), N(2 * tleg + map), N(P), N(e)), 3);
                    default: return Ex.Num(L.F("Uma missão gasta {0} Wh e a bateria {1}S deve ter 20% de reserva (80% utilizável, 3,7 V por célula). Que capacidade mínima, em mAh?", N(e), N(cells, 0)), mah, "mAh",
                            L.F("Energia da bateria = {0}/0,8 = {1} Wh; mAh = {1}/({2}·3,7)·1000 = {3} mAh.", N(e), N(need), N(cells, 0), N(mah)), 3);
                }
            }
            default:
            {
                double p0 = Ex.Step(r, 100, 300, 10), k = Ex.Step(r, 0.005, 0.03, 0.005), vopt = Math.Cbrt(p0 / (2 * k)), Pvo = p0 + k * vopt * vopt * vopt;
                double tl = dist / vopt, Eopt = Pvo * 2 * tl / 3600;
                double v15 = 15, P15 = p0 + k * v15 * v15 * v15, E15 = P15 * 2 * dist / v15 / 3600;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("A potência do drone é P(v) = {0} + {1}·v³. Para ir e voltar a um ponto a {2} m na velocidade de menor Wh/km (v³ = P₀/(2k)), quantos Wh gasta o cruzeiro (sem o mapeamento)?", N(p0), N(k, 3), N(dist)), Eopt, "Wh",
                            L.F("v = {0} m/s; P = {1} W; t = 2·{2}/{0} = {3} s; E = {4} Wh.", N(vopt, 3), N(Pvo), N(dist), N(2 * tl), N(Eopt)), 3);
                    default: return Ex.Num(L.F("A potência do drone é P(v) = {0} + {1}·v³. Quantos Wh se economiza no cruzeiro de ida e volta a {2} m voando na velocidade ideal (v³ = P₀/(2k)) em vez de 15 m/s?", N(p0), N(k, 3), N(dist)), E15 - Eopt, "Wh",
                        L.F("Ideal: v = {0} m/s → {1} Wh; a 15 m/s: P = {2} W → {3} Wh; economia = {4} Wh (negativo se 15 m/s for melhor).", N(vopt, 3), N(Eopt), N(P15), N(E15), N(E15 - Eopt)), 3);
                }
            }
        }
    }
}
