using static EasyCreator.Exercises.Pv;

namespace EasyCreator.Exercises;

/// <summary>Drones 1-5: parts, thrust-to-weight, battery and flight time, propellers, IMU and sensors.</summary>
[Exercises("drone-01-partes")]
public sealed class Drone01 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int cells = Ex.Int(r, 2, 6), motors = Ex.Pick(r, 4, 6, 8), mass = (int)Ex.Step(r, 300, 2500, 50), thr = (int)Ex.Step(r, 400, 1500, 50);
        switch (d)
        {
            case Difficulty.Easy:
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um drone de {1} g usa uma bateria LiPo {0}S, com 3,7 V de tensão nominal por célula. Qual a tensão nominal do pacote, em V?", N(cells, 0), N(mass, 0)), cells * 3.7, "V",
                        L.F("{0} células em série × 3,7 V = {1} V.", N(cells, 0), N(cells * 3.7)));
                    case 1: return Ex.Num(L.F("A bateria de um drone de {1} g tem tensão nominal de {0} V (3,7 V por célula). Quantas células em série (S) ela tem?", N(cells * 3.7), N(mass, 0)), cells, "S",
                        L.F("{0} / 3,7 = {1} células.", N(cells * 3.7), N(cells, 0)));
                    default:
                    {
                        string[][] fn = {
                            new[] { L.T("lê os sensores e calcula quanto cada motor deve girar"), L.T("a controladora de voo"), },
                            new[] { L.T("converte a corrente da bateria em pulsos trifásicos para o motor"), L.T("o ESC") },
                            new[] { L.T("transforma a rotação em empuxo"), L.T("a hélice") },
                            new[] { L.T("guarda a energia de todo o voo"), L.T("a bateria") },
                            new[] { L.T("recebe os comandos do piloto pelo rádio"), L.T("o receptor") },
                            new[] { L.T("informa a posição na Terra por satélite"), L.T("o GPS") },
                        };
                        var pick = fn[r.Next(fn.Length)];
                        var wrong = fn.Where(f => f != pick).Select(f => f[1]).OrderBy(_ => r.Next()).Take(3);
                        return Ex.Choice(r, L.F("Num quadricóptero de {0} g com bateria {1}S e {2} motores, qual componente {3}?", N(mass, 0), N(cells, 0), N(motors, 0), pick[0]), pick[1], wrong,
                            L.F("Cada peça tem seu papel: {0} é o componente que {1}.", pick[1], pick[0]));
                    }
                }
            case Difficulty.Medium:
            {
                double total = motors * thr;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um drone de {0} motores, cada um com empuxo máximo de {1} g, tem quanto empuxo total máximo, em g?", N(motors, 0), N(thr, 0)), total, "g",
                        L.F("{0} × {1} = {2} g.", N(motors, 0), N(thr, 0), N(total, 0)));
                    case 1: return Ex.Num(L.F("Um drone de {0} g precisa que cada um dos seus {1} motores dê, em pairado, quantos gramas de empuxo?", N(mass, 0), N(motors, 0)), (double)mass / motors, "g",
                        L.F("{0} / {1} = {2} g por motor.", N(mass, 0), N(motors, 0), N((double)mass / motors)));
                    default:
                    {
                        double mah = Ex.Step(r, 1000, 6000, 250), wh = cells * 3.7 * mah / 1000;
                        return Ex.Num(L.F("Uma bateria {0}S de {1} mAh guarda quantos Wh? (3,7 V por célula)", N(cells, 0), N(mah, 0)), wh, "Wh",
                            L.F("{0} × 3,7 = {1} V; {1} V × {2} Ah = {3} Wh.", N(cells, 0), N(cells * 3.7), N(mah / 1000), N(wh)));
                    }
                }
            }
            case Difficulty.Hard:
            {
                double frame = Ex.Step(r, 300, 900, 50), mot = Ex.Step(r, 30, 90, 5), esc = Ex.Step(r, 10, 25, 1), fc = Ex.Step(r, 15, 40, 5), bat = Ex.Step(r, 200, 700, 25), prop = Ex.Step(r, 5, 15, 1), cam = Ex.Step(r, 0, 300, 50);
                int n = Ex.Pick(r, 4, 6);
                double sum = frame + n * (mot + esc + prop) + fc + bat + cam;
                string parts = L.F("quadro {0} g, controladora {1} g, bateria {2} g, câmera {3} g e, para cada um dos {4} motores, motor {5} g + ESC {6} g + hélice {7} g", N(frame, 0), N(fc, 0), N(bat, 0), N(cam, 0), N(n, 0), N(mot, 0), N(esc, 0), N(prop, 0));
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone tem: ") + parts + L.T(". Qual a massa total, em g?"), sum, "g",
                        L.F("Massa total = quadro + controladora + bateria + câmera + {0}·(motor + ESC + hélice) = {1} g.", N(n, 0), N(sum, 0)));
                double thrM = Ex.Step(r, 500, 1200, 50);
                return Ex.Num(L.F("Um drone tem: ") + parts + L.F(". Cada motor dá {0} g de empuxo máximo. Qual a relação empuxo/peso?", N(thrM, 0)), n * thrM / sum, "",
                    L.F("Massa total = {0} g; T/W = {1}·{2} / {0} = {3}.", N(sum, 0), N(n, 0), N(thrM, 0), N(n * thrM / sum, 3)), 3);
            }
            default:
            {
                double mTot = Ex.Step(r, 800, 2500, 50), per = Ex.Step(r, 500, 1200, 50);
                int n = Ex.Pick(r, 4, 6, 8);
                double tw = n * per / mTot, pay = n * per / 2 - mTot;
                if (r.Next(2) == 0 && tw > 1.4)
                    return Ex.Num(L.F("Um drone de {0} g com {1} motores de {2} g de empuxo máximo cada quer manter T/W ≥ 2 para ter boa manobra. Qual a carga extra máxima, em g?", N(mTot, 0), N(n, 0), N(per, 0)), pay, "g",
                        L.F("Empuxo total = {0}·{1} = {2} g; massa máxima com T/W = 2: {2}/2 = {3} g; carga extra = {3} − {4} = {5} g.", N(n, 0), N(per, 0), N(n * per, 0), N(n * per / 2, 0), N(mTot, 0), N(pay, 0)), 3);
                double hov = 100.0 / tw;
                return Ex.Num(L.F("Um drone de {0} g com {1} motores de {2} g de empuxo máximo cada. Em que % do acelerador (supondo empuxo proporcional ao acelerador) ele paira?", N(mTot, 0), N(n, 0), N(per, 0)), hov, "%",
                    L.F("T/W = {0}·{1}/{2} = {3}; acelerador de pairar = 1/(T/W) = {4}%.", N(n, 0), N(per, 0), N(mTot, 0), N(tw, 3), N(hov)), 3);
            }
        }
    }
}

[Exercises("drone-02-empuxo-peso")]
public sealed class Drone02 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        int n = Ex.Pick(r, 4, 6, 8);
        double per = Ex.Step(r, 400, 2000, 50), m = Ex.Step(r, 400, 3000, 50), tw = n * per / m;
        string dr = L.F("Um drone de {0} g com {1} motores de {2} g de empuxo máximo cada.", N(m, 0), N(n, 0), N(per, 0));
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(dr + " " + L.T("Qual a relação empuxo/peso (T/W)?"), tw, "",
                        L.F("T/W = {0}·{1} / {2} = {3}.", N(n, 0), N(per, 0), N(m, 0), N(tw, 3)), 3);
                return Ex.Num(dr + " " + L.T("Quantos g de empuxo total ele tem?"), n * per, "g",
                    L.F("{0} × {1} = {2} g.", N(n, 0), N(per, 0), N(n * per, 0)));
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(dr + " " + L.T("Em que % do acelerador ele paira (empuxo proporcional ao acelerador)?"), 100 / tw, "%",
                        L.F("T/W = {0}; acelerador de pairar = 1/(T/W) = {1}%.", N(tw, 3), N(100 / tw)), 3);
                return Ex.Num(dr + " " + L.T("Qual a aceleração vertical máxima, em m/s², com o acelerador no máximo? (g = 9,8 m/s²)"), G * (tw - 1), "m/s²",
                    L.F("a = g·(T/W − 1) = 9,8·({0} − 1) = {1} m/s².", N(tw, 3), N(G * (tw - 1))), 3);
            case Difficulty.Hard:
            {
                double target = Ex.Pick(r, 1.5, 2.0, 2.5, 3.0), payMax = n * per / target - m;
                switch (r.Next(2))
                {
                    case 0:
                        if (payMax > 20)
                            return Ex.Num(dr + " " + L.F("Qual a carga extra máxima, em g, para manter T/W = {0}?", N(target)), payMax, "g",
                                L.F("Massa máxima = empuxo total / T/W = {0} / {1} = {2} g; carga extra = {2} − {3} = {4} g.", N(n * per, 0), N(target), N(n * per / target), N(m, 0), N(payMax)), 3);
                        goto default;
                    default:
                    {
                        double tilt = Math.Acos(Math.Min(1, 1 / tw)) * 180 / Math.PI;
                        return Ex.Num(dr + " " + L.T("Qual a maior inclinação, em graus, em que ele ainda consegue manter a altitude com empuxo total? (T·cos θ = W)"), tilt, "°",
                            L.F("cos θ = 1/(T/W) = 1/{0} → θ = {1}°.", N(tw, 3), N(tilt)), 3);
                    }
                }
            }
            default:
            {
                double ah = G * Math.Sqrt(Math.Max(tw * tw - 1, 0)), hgt = Ex.Step(r, 5, 40, 1), a = G * (tw - 1), t = Math.Sqrt(2 * hgt / a);
                if (r.Next(2) == 0)
                    return Ex.Num(dr + " " + L.T("Qual a maior aceleração horizontal, em m/s², mantendo a altitude, com empuxo total? (g = 9,8 m/s²)"), ah, "m/s²",
                        L.F("A parte vertical do empuxo vale o peso: T·cos θ = W; horizontal: a = (T·sen θ)/m = g·√((T/W)² − 1) = 9,8·√({0}² − 1) = {1} m/s².", N(tw, 3), N(ah)), 3);
                return Ex.Num(dr + " " + L.F("Subindo na vertical com empuxo total a partir do repouso, quanto tempo leva, em s, para chegar a {0} m? (g = 9,8 m/s²)", N(hgt)), t, "s",
                    L.F("a = g·(T/W − 1) = {0} m/s²; h = ½·a·t² → t = √(2·{1}/{0}) = {2} s.", N(a), N(hgt), N(t)), 3);
            }
        }
    }
}

[Exercises("drone-03-bateria")]
public sealed class Drone03 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int cells = Ex.Int(r, 2, 6);
        double mah = Ex.Step(r, 1000, 8000, 250), v = cells * 3.7, wh = v * mah / 1000;
        string bat = L.F("Uma bateria LiPo {0}S de {1} mAh (3,7 V por célula).", N(cells, 0), N(mah, 0));
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(bat + " " + L.T("Qual a energia, em Wh?"), wh, "Wh",
                        L.F("E = V × Ah = {0} × {1} = {2} Wh.", N(v), N(mah / 1000), N(wh)));
                return Ex.Num(bat + " " + L.T("Qual a energia, em kJ? (1 Wh = 3,6 kJ)"), wh * 3.6, "kJ",
                    L.F("{0} Wh × 3,6 = {1} kJ.", N(wh), N(wh * 3.6)));
            case Difficulty.Medium:
            {
                double I = Ex.Step(r, 5, 40, 1), t = 0.8 * mah / 1000 / I * 60;
                if (r.Next(2) == 0)
                    return Ex.Num(bat + " " + L.F("O drone consome {0} A em média e só 80% da capacidade é utilizável. Quantos minutos de voo?", N(I)), t, "min",
                        L.F("t = 0,8 × {0} Ah / {1} A = {2} h = {3} min.", N(mah / 1000), N(I), N(t / 60, 3), N(t)), 3);
                double P = Ex.Step(r, 80, 500, 10);
                return Ex.Num(bat + " " + L.F("O drone consome {0} W. Quantos minutos de voo com 80% da energia utilizável?", N(P)), 0.8 * wh / P * 60, "min",
                    L.F("t = 0,8 × {0} Wh / {1} W = {2} h = {3} min.", N(wh), N(P), N(0.8 * wh / P, 3), N(0.8 * wh / P * 60)), 3);
            }
            case Difficulty.Hard:
            {
                double P = Ex.Step(r, 100, 600, 10), I = P / v, t = 0.8 * mah / 1000 / I * 60;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(bat + " " + L.F("O drone consome {0} W. Qual a corrente média, em A?", N(P)), I, "A",
                        L.F("I = P / V = {0} / {1} = {2} A.", N(P), N(v), N(I)));
                    case 1: return Ex.Num(bat + " " + L.F("O drone consome {0} W. Com 80% da capacidade utilizável, qual o tempo de voo, em minutos?", N(P)), t, "min",
                        L.F("I = P/V = {0} A; t = 0,8·Ah / I = 0,8·{1} / {0} = {2} h = {3} min.", N(I), N(mah / 1000), N(t / 60, 3), N(t)), 3);
                    default:
                    {
                        double want = Ex.Step(r, 10, 35, 1), cap = want / 60 * I / 0.8 * 1000;
                        return Ex.Num(L.F("Um drone de {0} S consome {1} W. Que capacidade, em mAh, a bateria precisa ter para {2} minutos de voo com 80% utilizável? (3,7 V por célula)", N(cells, 0), N(P), N(want)), cap, "mAh",
                            L.F("I = P/V = {0} A; Ah = I·t / 0,8 = {0}·{1}/60 / 0,8 = {2} Ah = {3} mAh.", N(I), N(want), N(cap / 1000, 3), N(cap)), 3);
                    }
                }
            }
            default:
            {
                int n = Ex.Pick(r, 4, 6), nb = Ex.Pick(r, 1, 2), dIn = Ex.Pick(r, 5, 7, 8, 9, 10, 12, 13);
                double m = Ex.Step(r, 0.6, 3.0, 0.1), P = Mr.HoverP(m, n, dIn), packWh = nb * wh, t = 0.8 * packWh / P * 60;
                return Ex.Num(L.F("Um drone de {0} kg com {1} hélices de {2}\" pairando (FM 0,6, motor e ESC 85%, ρ = 1,2 kg/m³, g = 9,8 m/s²) usa {3} baterias {4}S de {5} mAh em paralelo. Quantos minutos paira, com 80% da energia utilizável? (potência ideal = T^1,5 / √(2ρA), A total dos discos)", N(m), N(n, 0), N(dIn, 0), N(nb, 0), N(cells, 0), N(mah, 0)), t, "min",
                    L.F("P_ideal = T^1,5/√(2ρA) com T = {0} N e A = {1} m²; P_elétrica = P_ideal/(0,6·0,85) = {2} W; E = {3} Wh; t = 0,8·{3}/{2} = {4} min.", N(m * 9.8), N(n * Mr.PropArea(dIn), 4), N(P), N(packWh), N(t)), 3);
            }
        }
    }
}

[Exercises("drone-04-helice")]
public sealed class Drone04 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double dIn = Ex.Step(r, 5, 18, 1), A = Mr.PropArea(dIn), T = Ex.Step(r, 3, 40, 1);
        double Pid = Mr.IdealP(T, A), vi = Math.Sqrt(T / (2 * 1.2 * A));
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Qual a área do disco de uma hélice de {0} polegadas, em m²? (1\" = 0,0254 m; A = π·D²/4)", N(dIn, 0)), A, "m²",
                        L.F("D = {0}·0,0254 = {1} m; A = π·{1}²/4 = {2} m².", N(dIn, 0), N(dIn * 0.0254, 4), N(A, 5)), 3);
                {
                    double rp = Ex.Step(r, 3000, 15000, 500), tipv = rp * 2 * Math.PI / 60 * dIn * 0.0254 / 2;
                    return Ex.Num(L.F("Uma hélice de {0} polegadas gira a {1} rpm. Qual a velocidade da ponta, em m/s? (v = rpm·2π/60·D/2)", N(dIn, 0), N(rp, 0)), tipv, "m/s",
                        L.F("Ω = {0}·2π/60 = {1} rad/s; v = Ω·D/2 = {1}·{2} = {3} m/s.", N(rp, 0), N(rp * 2 * Math.PI / 60), N(dIn * 0.0254 / 2, 4), N(tipv)), 3);
                }
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma hélice de {0}\" gera {1} N de empuxo. Qual a potência ideal, em W? (ρ = 1,2 kg/m³; P = T^1,5 / √(2ρA))", N(dIn, 0), N(T)), Pid, "W",
                        L.F("A = {0} m²; P = {1}^1,5 / √(2·1,2·{0}) = {2} W.", N(A, 5), N(T), N(Pid)), 3);
                return Ex.Num(L.F("Uma hélice de {0}\" gera {1} N de empuxo (ρ = 1,2 kg/m³). Qual a velocidade induzida, em m/s? (vi = √(T/(2ρA)))", N(dIn, 0), N(T)), vi, "m/s",
                    L.F("A = {0} m²; vi = √({1}/(2·1,2·{0})) = {2} m/s.", N(A, 5), N(T), N(vi)), 3);
            case Difficulty.Hard:
            {
                double d2 = dIn + Ex.Step(r, 1, 4, 1), A2 = Mr.PropArea(d2), P2 = Mr.IdealP(T, A2);
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Trocar uma hélice de {0}\" por uma de {1}\" para o mesmo empuxo de {2} N. Quantos watts de potência ideal isso economiza? (ρ = 1,2 kg/m³)", N(dIn, 0), N(d2, 0), N(T)), Pid - P2, "W",
                        L.F("P₁ = {0} W; P₂ = {1} W; economia = {2} W.", N(Pid), N(P2), N(Pid - P2)), 3);
                    case 1: return Ex.Num(L.F("Uma hélice de {0}\" gera {1} N com FM = 0,6. Qual a potência real, em W? (ρ = 1,2 kg/m³)", N(dIn, 0), N(T)), Pid / 0.6, "W",
                        L.F("P_ideal = {0} W; P_real = P_ideal / 0,6 = {1} W.", N(Pid), N(Pid / 0.6)), 3);
                    default: return Ex.Num(L.F("Uma hélice de {0}\" gera {1} N de empuxo consumindo a potência ideal. Qual a eficiência, em gramas-força por watt? (1 N = 101,97 gf; ρ = 1,2 kg/m³)", N(dIn, 0), N(T)), T * 101.97 / Pid, "gf/W",
                        L.F("P = {0} W; eficiência = {1} N × 101,97 / {0} = {2} gf/W.", N(Pid), N(T), N(T * 101.97 / Pid)), 3);
                }
            }
            default:
            {
                double rpm = Ex.Step(r, 6000, 20000, 500), pitch = Ex.Step(r, 3, 8, 0.5), vp = pitch * 0.0254 * rpm / 60;
                int n = Ex.Pick(r, 4, 6, 8);
                double mTot = Ex.Step(r, 0.8, 4, 0.1), perT = mTot * 9.8 / n, d1 = Ex.Step(r, 7, 15, 1);
                double Pn = n * Mr.IdealP(perT, Mr.PropArea(d1)), slip = Ex.Pick(r, 0.7, 0.8, 0.85), vfly = Ex.Step(r, 10, 30, 1), rpmNeed = vfly / (slip * pitch * 0.0254) * 60;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma hélice de passo {0}\" só consegue empurrar o ar até {1}% da velocidade de passo (v = passo·rpm/60) por causa do escorregamento. Para o drone chegar a {2} m/s, a quantas rpm ela precisa girar?", N(pitch), N(slip * 100, 0), N(vfly)), rpmNeed, "rpm",
                        L.F("Velocidade de passo necessária = {0} / {1} = {2} m/s; rpm = v·60 / passo = {2}·60 / {3} = {4} rpm.", N(vfly), N(slip), N(vfly / slip), N(pitch * 0.0254, 4), N(rpmNeed)), 3);
                return Ex.Num(L.F("Um drone de {0} kg tem {1} hélices de {2}\" (ρ = 1,2 kg/m³, g = 9,8 m/s²). Qual a potência ideal total para pairar (soma dos {1} rotores, T^1,5 / √(2ρA) em cada um), em W?", N(mTot), N(n, 0), N(d1, 0)), Pn, "W",
                    L.F("Cada rotor: T = {0} N → P = T^1,5/√(2ρA); a soma dos rotores dá {1} W.", N(perT), N(Pn)), 3);
            }
        }
    }
}

[Exercises("drone-05-imu-sensores")]
public sealed class Drone05 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double ax = Ex.Step(r, -3, 3, 0.1), az = Ex.Step(r, 8.5, 9.8, 0.1), acc = Math.Atan2(ax, az) * 180 / Math.PI;
        double gyro = Ex.Step(r, -40, 40, 1), dt = Ex.Step(r, 0.002, 0.02, 0.002), alpha = Ex.Pick(r, 0.95, 0.97, 0.98, 0.99), ang = Ex.Step(r, -20, 20, 1);
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("O acelerômetro de um drone parado mede {0} m/s² no eixo X e {1} m/s² no eixo Z. Qual a inclinação, em graus? (θ = atan(ax/az))", N(ax), N(az)), acc, "°",
                        L.F("θ = atan({0}/{1}) = {2}°.", N(ax), N(az), N(acc)), 3);
                {
                    double t = Ex.Step(r, 0.5, 5, 0.5), g0 = Ex.Step(r, 5, 40, 1);
                    return Ex.Num(L.F("O giroscópio mede {0} °/s constantes por {1} s. Quanto o ângulo mudou, em graus?", N(g0), N(t)), g0 * t, "°",
                        L.F("Δθ = ω·t = {0}·{1} = {2}°.", N(g0), N(t), N(g0 * t)));
                }
            case Difficulty.Medium:
            {
                double bias = Ex.Step(r, 0.1, 2, 0.1), t = Ex.Step(r, 10, 120, 10);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um giroscópio parado tem um erro (bias) de {0} °/s. Se o ângulo é só a integral do giroscópio, quantos graus de erro após {1} s?", N(bias), N(t)), bias * t, "°",
                        L.F("O erro se acumula: {0} °/s × {1} s = {2}° (por isso o giroscópio sozinho deriva).", N(bias), N(t), N(bias * t)));
                double upd = alpha * (ang + gyro * dt) + (1 - alpha) * acc;
                return Ex.Num(L.F("Filtro complementar com α = {0}: ângulo anterior {1}°, giroscópio {2} °/s, dt = {3} s, ângulo do acelerômetro {4}°. Qual o novo ângulo, em graus?", N(alpha), N(ang), N(gyro), N(dt, 3), N(acc)), upd, "°",
                    L.F("ângulo = α·(ângulo + giro·dt) + (1 − α)·acelerômetro = {0}·({1} + {2}·{3}) + {4}·{5} = {6}°.", N(alpha), N(ang), N(gyro), N(dt, 3), N(1 - alpha, 2), N(acc), N(upd, 3)), 3);
            }
            case Difficulty.Hard:
            {
                double tau = alpha * dt / (1 - alpha), fc = 1 / (2 * Math.PI * tau);
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um filtro complementar roda a cada dt = {0} s com α = {1}. Qual a constante de tempo τ = α·dt/(1 − α), em s?", N(dt, 3), N(alpha)), tau, "s",
                        L.F("τ = {0}·{1} / (1 − {0}) = {2} s.", N(alpha), N(dt, 3), N(tau, 4)), 3);
                    default: return Ex.Num(L.F("Um filtro complementar tem constante de tempo τ = {0} s. Abaixo de que frequência, em Hz, ele confia mais no acelerômetro que no giroscópio? (f = 1/(2π·τ))", N(tau, 3)), fc, "Hz",
                        L.F("f = 1 / (2π·{0}) = {1} Hz.", N(tau, 3), N(fc)), 3);
                }
            }
            default:
            {
                double bias = Ex.Step(r, 0.2, 2, 0.1), n = 1 / (1 - alpha);
                // steady-state error of the complementary filter with gyro bias: err = tau * bias.
                double tau = alpha * dt / (1 - alpha), err = tau * bias;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("O giroscópio tem bias de {0} °/s. Um filtro complementar (α = {1}, dt = {2} s) mantém, em regime, um erro de ângulo de aproximadamente τ·bias. Qual esse erro, em graus?", N(bias), N(alpha), N(dt, 3)), err, "°",
                        L.F("τ = α·dt/(1 − α) = {0} s; erro = τ·bias = {0}·{1} = {2}°.", N(tau, 4), N(bias), N(err, 4)), 3);
                double target = Ex.Step(r, 0.5, 5, 0.5) / 100.0 * bias, taus = target / bias, alp = taus / (taus + dt);
                return Ex.Num(L.F("Para o erro em regime do filtro complementar (τ·bias) ser {0}° com bias de {1} °/s e dt = {2} s, qual deve ser o α? (α = τ/(τ + dt))", N(target, 3), N(bias), N(dt, 3)), alp, "",
                    L.F("τ = erro / bias = {0} s; α = τ/(τ + dt) = {0}/({0} + {1}) = {2}.", N(taus, 4), N(dt, 3), N(alp, 4)), 3);
            }
        }
    }
}
