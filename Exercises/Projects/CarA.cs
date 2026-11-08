using static EasyCreator.Exercises.Pv;

namespace EasyCreator.Exercises;

// Markers Q/Qf/Qn (pt,en,es) are rewritten to L.T/L.F by the conversion script (translations go to Lang/exercises-projects.*.json).

/// <summary>Cars 1-5: forces on a car, engine, gearbox, traction and weight transfer, braking.</summary>
[Exercises("car-01-forcas")]
public sealed class Car01 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G, Rho = Pk.Rho;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double m = Ex.Step(r, 800, 2200, 50), a = Ex.Step(r, 1, 6, 0.5), F = m * a;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um carro de {0} kg recebe uma força resultante de {1} N. Qual a sua aceleração?", N(m), N(F)), a, "m/s²",
                        L.F("a = F / m = {0} / {1} = {2} m/s².", N(F), N(m), N(a)));
                    case 1: return Ex.Num(L.F("Para um carro de {0} kg acelerar a {1} m/s², qual deve ser a força resultante?", N(m), N(a)), F, "N",
                        L.F("F = m · a = {0} · {1} = {2} N.", N(m), N(a), N(F)));
                    default: return Ex.Num(L.F("Um kart sob uma força resultante de {0} N acelera a {1} m/s². Qual a massa do kart com o piloto?", N(F), N(a)), m, "kg",
                        L.F("m = F / a = {0} / {1} = {2} kg.", N(F), N(a), N(m)));
                }
            }
            case Difficulty.Medium:
            {
                double m = Ex.Step(r, 900, 1800, 50), cd = Ex.Step(r, 0.25, 0.40, 0.01), A = Ex.Step(r, 1.8, 2.6, 0.1), crr = Ex.Step(r, 0.010, 0.016, 0.001);
                double v = Ex.Step(r, 10, 30, 1), P = Ex.Step(r, 40, 120, 5) * 1000;
                double Ft = P / v, Fd = 0.5 * Rho * cd * A * v * v, Fr = crr * m * G, a = (Ft - Fd - Fr) / m;
                string intro = L.F("Um carro de {0} kg (Cd = {1}, área frontal {2} m², coeficiente de rolamento {3}) anda a {4} m/s, com {5} kW chegando às rodas. Use ρ = 1,2 kg/m³ e g = 9,8 m/s².", N(m), N(cd), N(A), N(crr, 3), N(v), N(P / 1000));
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(intro + " " + L.T("Qual a força de tração nas rodas?"), Ft, "N",
                        L.F("F = P / v = {0} / {1} = {2} N.", N(P), N(v), N(Ft)));
                    case 1: return Ex.Num(intro + " " + L.T("Qual a soma do arrasto do ar com a resistência ao rolamento?"), Fd + Fr, "N",
                        L.F("Arrasto = ½·ρ·Cd·A·v² = {0} N; rolamento = Crr·m·g = {1} N; soma = {2} N.", N(Fd), N(Fr), N(Fd + Fr)));
                    default: return Ex.Num(intro + " " + L.T("Qual a aceleração do carro nesse instante?"), a, "m/s²",
                        L.F("F_tração = P/v = {0} N; resistências = {1} + {2} = {3} N; a = (F − R) / m = {4} m/s².", N(Ft), N(Fd), N(Fr), N(Fd + Fr), N(a)));
                }
            }
            case Difficulty.Hard:
            {
                double m = Ex.Step(r, 900, 1800, 50), cd = Ex.Step(r, 0.25, 0.40, 0.01), A = Ex.Step(r, 1.8, 2.6, 0.1), crr = Ex.Step(r, 0.010, 0.016, 0.001);
                double kmh = Ex.Step(r, 60, 160, 10), v = Ms(kmh), eta = Ex.Pick(r, 0.85, 0.9, 0.95);
                double Fd = 0.5 * Rho * cd * A * v * v, Fr = crr * m * G, Pw = (Fd + Fr) * v;
                string intro = L.F("Um carro de {0} kg (Cd = {1}, área frontal {2} m², coeficiente de rolamento {3}) viaja a {4} km/h em reta plana, a velocidade constante. Use ρ = 1,2 kg/m³ e g = 9,8 m/s².", N(m), N(cd), N(A), N(crr, 3), N(kmh));
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(intro + " " + L.T("Qual a força de arrasto aerodinâmico?"), Fd, "N",
                        L.F("v = {0} km/h = {1} m/s; F_ar = ½·ρ·Cd·A·v² = {2} N.", N(kmh), N(v), N(Fd)));
                    case 1: return Ex.Num(intro + " " + L.T("Qual a potência (em kW) que as rodas precisam receber?"), Pw / 1000, "kW",
                        L.F("A velocidade é constante: F_tração = arrasto + rolamento = {0} + {1} = {2} N; P = F·v = {2}·{3} = {4} kW.", N(Fd), N(Fr), N(Fd + Fr), N(v), N(Pw / 1000)));
                    default: return Ex.Num(intro + " " + L.F("A transmissão tem rendimento de {0}%. Qual a potência (em kW) que o motor deve produzir?", N(eta * 100, 0)), Pw / eta / 1000, "kW",
                        L.F("Nas rodas: P = (F_ar + F_rol)·v = {0} kW. No motor: {0} / {1} = {2} kW.", N(Pw / 1000), N(eta), N(Pw / eta / 1000)));
                }
            }
            default:
            {
                switch (r.Next(3))
                {
                    case 0:
                    {
                        double v1 = Ex.Step(r, 150, 220, 5), k = Ex.Pick(r, 1.5, 2, 3, 4, 5), v2 = v1 * Math.Pow(k, 1.0 / 3);
                        return Ex.Num(L.F("Um carro chega a {0} km/h de velocidade máxima, quando só o arrasto do ar importa (despreze o rolamento). Se a potência nas rodas fosse {1} vezes maior, qual seria a nova velocidade máxima, em km/h?", N(v1), N(k)), v2, "km/h",
                            L.F("P = F·v = ½·ρ·Cd·A·v³, então v ∝ P^(1/3): v₂ = {0} · {1}^(1/3) = {2} km/h. Dobrar a potência só dá +26% de velocidade.", N(v1), N(k), N(v2)));
                    }
                    case 1:
                    {
                        double m = Ex.Step(r, 900, 1600, 50), P = Ex.Step(r, 60, 200, 10) * 1000, kmh = Ex.Pick(r, 80.0, 100, 120), v = Ms(kmh), t = 0.5 * m * v * v / P;
                        return Ex.Num(L.F("Um carro de {0} kg entrega potência constante de {1} kW às rodas. Despreze o ar e o rolamento e imagine que não há limite de tração. Quanto tempo leva de 0 a {2} km/h?", N(m), N(P / 1000), N(kmh)), t, "s",
                            L.F("Com potência constante a força cai com a velocidade, então a aceleração não é constante; use a energia: P·t = ½·m·v². v = {0} m/s → t = ½·{1}·{0}² / {2} = {3} s.", N(v), N(m), N(P), N(t)));
                    }
                    default:
                    {
                        double m = Ex.Step(r, 1000, 1800, 100), cd = 0.32, A = 2.2, crr = 0.012, P = Ex.Step(r, 60, 120, 10) * 1000, kmh = Ex.Pick(r, 40.0, 50, 60, 70), v = Ms(kmh);
                        double Fa = P / v - 0.5 * Rho * cd * A * v * v;
                        double th = Solve(x => m * G * (Math.Sin(x) + crr * Math.Cos(x)) - Fa, 0, 0.9), grade = Math.Tan(th) * 100;
                        return Ex.Num(L.F("Um carro de {0} kg (Cd = 0,32, área 2,2 m², coeficiente de rolamento 0,012) recebe {1} kW nas rodas. Qual a inclinação máxima da subida (em %, tg θ·100) que ele vence a {2} km/h constantes? Use ρ = 1,2 kg/m³ e g = 9,8 m/s².", N(m), N(P / 1000), N(kmh)), grade, "%",
                            L.F("Sobra para subir: F = P/v − arrasto = {0} N. Equilíbrio: m·g·(sen θ + Crr·cos θ) = F → θ = {1}°, e a inclinação é tg θ = {2}%.", N(Fa), N(th * 180 / Math.PI), N(grade)), 3);
                    }
                }
            }
        }
    }
}

[Exercises("car-02-motor")]
public sealed class Car02 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                double T = Ex.Step(r, 80, 400, 10), rpm = Ex.Step(r, 2000, 7000, 250), w = rpm * 2 * Math.PI / 60, P = T * w;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um motor entrega {0} N·m a {1} rpm. Qual a potência, em kW?", N(T), N(rpm)), P / 1000, "kW",
                        L.F("ω = {0}·2π/60 = {1} rad/s; P = T·ω = {2} · {1} = {3} kW.", N(rpm), N(w), N(T), N(P / 1000)));
                    case 1: return Ex.Num(L.F("Um motor produz {0} kW a {1} rpm. Qual o torque, em N·m?", N(P / 1000), N(rpm)), T, "N·m",
                        L.F("ω = {0} rad/s; T = P / ω = {1} / {0} = {2} N·m.", N(w), N(P), N(T)));
                    default: return Ex.Num(L.F("Um motor de {0} N·m entrega {1} kW. A quantas rpm ele está girando?", N(T), N(P / 1000)), rpm, "rpm",
                        L.F("ω = P / T = {0} / {1} = {2} rad/s; rpm = ω·60/2π = {3}.", N(P), N(T), N(w), N(rpm)));
                }
            }
            case Difficulty.Medium:
            {
                double Pm = Ex.Step(r, 60, 250, 10), rp = Ex.Step(r, 5000, 8500, 250), x = Ex.Step(r, 0.3, 0.9, 0.1), rpm = x * rp;
                double P = Pm * (x + x * x - x * x * x);
                string intro = L.F("A potência de um motor segue P = Pmáx·(x + x² − x³), com x = rpm / rpm da potência máxima. Um motor tem Pmáx = {0} kW em {1} rpm.", N(Pm), N(rp));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.F("Qual a potência, em kW, a {0} rpm?", N(rpm)), P, "kW",
                        L.F("x = {0}/{1} = {2}; P = {3}·({2} + {2}² − {2}³) = {4} kW.", N(rpm), N(rp), N(x), N(Pm), N(P)));
                double T = P * 1000 / (rpm * 2 * Math.PI / 60);
                return Ex.Num(intro + " " + L.F("Qual o torque, em N·m, a {0} rpm?", N(rpm)), T, "N·m",
                    L.F("P = {0} kW (com x = {1}); ω = {2} rad/s; T = P/ω = {3} N·m.", N(P), N(x), N(rpm * 2 * Math.PI / 60), N(T)));
            }
            case Difficulty.Hard:
            {
                double Pm = Ex.Step(r, 60, 250, 10), rp = Ex.Step(r, 5000, 8500, 250), wp = rp * 2 * Math.PI / 60, Tp = Pm * 1000 / wp;
                double x = Ex.Step(r, 0.3, 0.8, 0.1), Tx = Pm * 1000 * (1 + x - x * x) / wp;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um motor tem potência máxima de {0} kW a {1} rpm. Qual o torque nesse ponto, em N·m?", N(Pm), N(rp)), Tp, "N·m",
                        L.F("ω = {0} rad/s; T = P/ω = {1} / {0} = {2} N·m.", N(wp), N(Pm * 1000), N(Tp)));
                    case 1: return Ex.Num(L.F("Num motor com P = Pmáx·(x + x² − x³), x = rpm/rpm de Pmáx, a potência máxima é {0} kW a {1} rpm. Qual o torque a {2} rpm, em N·m?", N(Pm), N(rp), N(x * rp)), Tx, "N·m",
                        L.F("T = P/ω = Pmáx·(x + x² − x³) / (x·ωp) = Pmáx·(1 + x − x²)/ωp. Com x = {0}: T = {1}·{2} / {3} = {4} N·m.", N(x), N(Pm * 1000), N(1 + x - x * x), N(wp), N(Tx)));
                    default: return Ex.Num(L.F("Um motor entrega {0} N·m a {1} rpm. Qual a potência nesse ponto, em kW?", N(Tx), N(x * rp)), Tx * x * wp / 1000, "kW",
                        L.F("P = T·ω = {0} · ({1}·2π/60) = {2} kW.", N(Tx), N(x * rp), N(Tx * x * wp / 1000)));
                }
            }
            default:
            {
                double Pm = Ex.Step(r, 60, 250, 10), rp = Ex.Step(r, 5000, 8500, 250), wp = rp * 2 * Math.PI / 60;
                string intro = L.F("A potência de um motor é P = Pmáx·(x + x² − x³), com x = rpm / rpm de Pmáx. Seu Pmáx é {0} kW a {1} rpm.", N(Pm), N(rp));
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(intro + " " + L.T("Em quantas rpm o TORQUE é máximo?"), rp / 2, "rpm",
                        L.F("T = P/ω ∝ (1 + x − x²). Esse polinômio é máximo em x = 1/2, ou seja, {0} rpm (bem antes do pico de potência).", N(rp / 2)));
                    case 1: return Ex.Num(intro + " " + L.T("Qual o torque máximo, em N·m?"), Pm * 1000 * 1.25 / wp, "N·m",
                        L.F("T = Pmáx·(1 + x − x²)/ωp é máximo em x = 1/2, onde vale 1,25. T = 1,25·{0} / {1} = {2} N·m.", N(Pm * 1000), N(wp), N(Pm * 1000 * 1.25 / wp)));
                    default: return Ex.Num(intro + " " + L.T("Quantas vezes o torque máximo é maior que o torque no ponto de potência máxima?"), 1.25, "×",
                        L.T("No pico de potência x = 1 e 1 + x − x² = 1; no pico de torque x = 1/2 e vale 1,25. A razão é 1,25, qualquer que seja o motor."));
                }
            }
        }
    }
}

[Exercises("car-03-cambio")]
public sealed class Car03 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double rw = Ex.Step(r, 0.28, 0.36, 0.01), dif = Ex.Step(r, 3.2, 4.5, 0.1), g = Ex.Step(r, 0.8, 3.6, 0.1), rpm = Ex.Step(r, 2000, 6500, 250);
        double v = rpm * 2 * Math.PI / 60 * rw / (g * dif);
        switch (d)
        {
            case Difficulty.Easy:
            {
                string intro = L.F("Um carro tem pneus de raio {0} m, diferencial {1} e a marcha escolhida tem relação {2}.", N(rw), N(dif), N(g));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.F("Qual a velocidade, em km/h, a {0} rpm?", N(rpm)), Kmh(v), "km/h",
                        L.F("v = (rpm·2π/60)·r / (i·d) = {0}·{1} / {2} = {3} m/s = {4} km/h.", N(rpm * 2 * Math.PI / 60), N(rw), N(g * dif), N(v), N(Kmh(v))));
                return Ex.Num(intro + " " + L.T("Qual a relação total (marcha × diferencial)?"), g * dif, "",
                    L.F("Relação total = {0} × {1} = {2}.", N(g), N(dif), N(g * dif)));
            }
            case Difficulty.Medium:
            {
                double T = Ex.Step(r, 100, 400, 10), eta = Ex.Pick(r, 0.9, 0.92, 0.95), Tw = T * g * dif * eta, Fw = Tw / rw;
                string intro = L.F("Um motor entrega {0} N·m, a marcha tem relação {1}, o diferencial {2}, o rendimento da transmissão é {3} e o raio do pneu é {4} m.", N(T), N(g), N(dif), N(eta), N(rw));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Qual o torque nas rodas, em N·m?"), Tw, "N·m",
                        L.F("T_roda = T·i·d·η = {0}·{1}·{2}·{3} = {4} N·m.", N(T), N(g), N(dif), N(eta), N(Tw)));
                return Ex.Num(intro + " " + L.T("Qual a força de tração na estrada, em N?"), Fw, "N",
                    L.F("T_roda = {0} N·m; F = T_roda / r = {0} / {1} = {2} N.", N(Tw), N(rw), N(Fw)));
            }
            case Difficulty.Hard:
            {
                double kmh = Ex.Step(r, 40, 140, 5), vv = Ms(kmh);
                switch (r.Next(3))
                {
                    case 0:
                    {
                        double n = vv * g * dif / rw * 60 / (2 * Math.PI);
                        return Ex.Num(L.F("Com pneus de raio {0} m, diferencial {1} e uma marcha de relação {2}, a quantas rpm o motor gira a {3} km/h?", N(rw), N(dif), N(g), N(kmh)), n, "rpm",
                            L.F("v = {0} m/s; ω_motor = v·i·d / r = {1} rad/s; rpm = {2}.", N(vv), N(vv * g * dif / rw), N(n)));
                    }
                    case 1:
                    {
                        double iNeeded = rpm * 2 * Math.PI / 60 * rw / (dif * vv);
                        return Ex.Num(L.F("Que relação de marcha faz o motor girar a {0} rpm a {1} km/h, com diferencial {2} e pneus de raio {3} m?", N(rpm), N(kmh), N(dif), N(rw)), iNeeded, "",
                            L.F("i·d = ω·r / v = {0}·{1} / {2} = {3}; i = {3} / {4} = {5}.", N(rpm * 2 * Math.PI / 60), N(rw), N(vv), N(iNeeded * dif), N(dif), N(iNeeded)));
                    }
                    default:
                    {
                        double m = Ex.Step(r, 900, 1600, 50), T = Ex.Step(r, 120, 350, 10), eta = 0.92, a = T * g * dif * eta / rw / m;
                        return Ex.Num(L.F("Um carro de {0} kg usa um motor de {1} N·m numa marcha de relação {2}, com diferencial {3}, rendimento 0,92 e pneus de raio {4} m. Desprezando as resistências, qual a aceleração, em m/s²?", N(m), N(T), N(g), N(dif), N(rw)), a, "m/s²",
                            L.F("F = T·i·d·η / r = {0} N; a = F / m = {0} / {1} = {2} m/s².", N(T * g * dif * eta / rw), N(m), N(a)));
                    }
                }
            }
            default:
            {
                double P = Ex.Step(r, 60, 180, 10) * 1000, cd = Ex.Step(r, 0.28, 0.36, 0.01), A = Ex.Step(r, 1.9, 2.4, 0.1), m = Ex.Step(r, 1000, 1600, 50), crr = 0.012;
                double vP = TopSpeed(P, cd, A, m, crr), redline = Ex.Step(r, 5500, 7500, 250), iTop = Ex.Step(r, 0.6, 0.9, 0.05);
                double vR = redline * 2 * Math.PI / 60 * rw / (iTop * dif);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um carro de {0} kg (Cd = {1}, área {2} m², Crr = 0,012) tem {3} kW nas rodas. Sua última marcha tem relação {4}, o diferencial é {5}, o pneu tem raio {6} m e o limitador corta em {7} rpm. Qual a velocidade máxima real, em km/h? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", N(m), N(cd), N(A), N(P / 1000), N(iTop), N(dif), N(rw), N(redline)), Kmh(Math.Min(vP, vR)), "km/h",
                        L.F("Limite do ar (P = F·v): {0} km/h. Limite do giro: v = {1}·2π/60·{2} / ({3}·{4}) = {5} km/h. A velocidade máxima é a MENOR das duas: {6} km/h.", N(Kmh(vP)), N(redline), N(rw), N(iTop), N(dif), N(Kmh(vR)), N(Kmh(Math.Min(vP, vR)))), 3);
                double kmhT = Ex.Step(r, 160, 260, 10), dNeed = redline * 2 * Math.PI / 60 * rw / (iTop * Ms(kmhT));
                return Ex.Num(L.F("Um carro tem a última marcha com relação {0}, pneus de raio {1} m e limitador em {2} rpm. Que diferencial faz a velocidade máxima, limitada pelo giro, ser exatamente {3} km/h?", N(iTop), N(rw), N(redline), N(kmhT)), dNeed, "",
                    L.F("v = ω·r / (i·d) → d = ω·r / (i·v) = {0}·{1} / ({2}·{3}) = {4}.", N(redline * 2 * Math.PI / 60), N(rw), N(iTop), N(Ms(kmhT)), N(dNeed)), 3);
            }
        }
    }
}

[Exercises("car-04-tracao")]
public sealed class Car04 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double m = Ex.Step(r, 900, 1800, 50), h = Ex.Step(r, 0.45, 0.65, 0.01), L2 = Ex.Step(r, 2.4, 2.9, 0.05), wf = Ex.Step(r, 0.45, 0.62, 0.01), mu = Ex.Step(r, 0.7, 1.1, 0.05);
        switch (d)
        {
            case Difficulty.Easy:
            {
                double a = Ex.Step(r, 2, 6, 0.5), dW = m * a * h / L2;
                return Ex.Num(L.F("Um carro de {0} kg, com centro de gravidade a {1} m de altura e distância entre eixos de {2} m, acelera a {3} m/s². Quantos newtons de carga passam do eixo dianteiro para o traseiro?", N(m), N(h), N(L2), N(a)), dW, "N",
                    L.F("ΔW = m·a·h / L = {0}·{1}·{2} / {3} = {4} N.", N(m), N(a), N(h), N(L2), N(dW)));
            }
            case Difficulty.Medium:
            {
                double a = Ex.Step(r, 2, 6, 0.5), dW = m * a * h / L2;
                string intro = L.F("Um carro de {0} kg tem {1}% do peso sobre o eixo dianteiro parado, CG a {2} m de altura e distância entre eixos de {3} m. Ele acelera a {4} m/s². Use g = 9,8 m/s².", N(m), N(wf * 100, 0), N(h), N(L2), N(a));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Qual a carga total, em N, sobre o eixo traseiro durante a aceleração?"), m * G * (1 - wf) + dW, "N",
                        L.F("Parado: {0}·9,8·{1} = {2} N. Transferência: ΔW = m·a·h/L = {3} N. Total: {4} N.", N(m), N(1 - wf), N(m * G * (1 - wf)), N(dW), N(m * G * (1 - wf) + dW)));
                return Ex.Num(intro + " " + L.T("Qual a carga, em N, que sobra no eixo dianteiro durante a aceleração?"), m * G * wf - dW, "N",
                    L.F("Parado: {0}·9,8·{1} = {2} N. Menos a transferência ΔW = {3} N: {4} N.", N(m), N(wf), N(m * G * wf), N(dW), N(m * G * wf - dW)));
            }
            case Difficulty.Hard:
            {
                double aR = mu * G * (1 - wf) / (1 - mu * h / L2), aF = mu * G * wf / (1 + mu * h / L2);
                string intro = L.F("Um carro tem {0}% do peso na frente, distância entre eixos de {1} m, CG a {2} m de altura e pneus com μ = {3}. Use g = 9,8 m/s².", N(wf * 100, 0), N(L2), N(h), N(mu));
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(intro + " " + L.T("Com tração traseira, qual a aceleração máxima (m/s²) antes de as rodas patinarem?"), aR, "m/s²",
                        L.F("N_tras = m·g·(1 − wf) + m·a·h/L e m·a = μ·N_tras → a = μ·g·(1 − wf) / (1 − μ·h/L) = {0} m/s².", N(aR)));
                    case 1: return Ex.Num(intro + " " + L.T("Com tração dianteira, qual a aceleração máxima (m/s²) antes de as rodas patinarem?"), aF, "m/s²",
                        L.F("N_diant = m·g·wf − m·a·h/L e m·a = μ·N_diant → a = μ·g·wf / (1 + μ·h/L) = {0} m/s².", N(aF)));
                    default: return Ex.Num(intro + " " + L.T("Com tração integral (4×4), qual a aceleração máxima (m/s²) antes de as rodas patinarem?"), mu * G, "m/s²",
                        L.F("Todas as rodas empurram, então toda a carga conta: m·a = μ·m·g → a = μ·g = {0} m/s².", N(mu * G)));
                }
            }
            default:
            {
                double aW = G * wf * L2 / h;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um carro 4×4 tem {0}% do peso na frente, distância entre eixos de {1} m e CG a {2} m de altura. Com que aceleração (m/s²) as rodas dianteiras se levantam do chão (empinada)? Use g = 9,8 m/s².", N(wf * 100, 0), N(L2), N(h)), aW, "m/s²",
                        L.F("A carga dianteira zera quando m·g·wf = m·a·h/L → a = g·wf·L/h = 9,8·{0}·{1} / {2} = {3} m/s².", N(wf), N(L2), N(h), N(aW)));
                return Ex.Num(L.F("Um carro 4×4 tem {0}% do peso na frente, distância entre eixos de {1} m, CG a {2} m de altura e pneus com μ = {3}. Qual a maior aceleração (m/s²) que ele consegue sem patinar e sem empinar? Use g = 9,8 m/s².", N(wf * 100, 0), N(L2), N(h), N(mu)), Math.Min(mu * G, aW), "m/s²",
                    L.F("Limite de aderência: μ·g = {0} m/s². Limite de empinada: g·wf·L/h = {1} m/s². Vale o menor: {2} m/s².", N(mu * G), N(aW), N(Math.Min(mu * G, aW))));
            }
        }
    }
}

[Exercises("car-05-frenagem")]
public sealed class Car05 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double kmh = Ex.Step(r, 40, 130, 10), v = Ms(kmh), t = Ex.Step(r, 0.5, 1.6, 0.1), mu = Ex.Step(r, 0.4, 1.0, 0.05), a = mu * G;
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um motorista leva {0} s para reagir e o carro está a {1} km/h. Quantos metros o carro percorre só durante a reação?", N(t), N(kmh)), v * t, "m",
                        L.F("v = {0} m/s; distância = v·t = {0}·{1} = {2} m.", N(v), N(t), N(v * t)));
                return Ex.Num(L.F("Um carro a {0} km/h freia com desaceleração constante de {1} m/s². Qual a distância de frenagem, em metros?", N(kmh), N(a)), v * v / (2 * a), "m",
                    L.F("v = {0} m/s; d = v² / (2a) = {0}² / (2·{1}) = {2} m.", N(v), N(a), N(v * v / (2 * a))));
            case Difficulty.Medium:
            {
                double db = v * v / (2 * a), dr = v * t;
                string intro = L.F("Um carro anda a {0} km/h numa pista com μ = {1} entre pneu e chão (g = 9,8 m/s²). O motorista reage em {2} s.", N(kmh), N(mu), N(t));
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(intro + " " + L.T("Qual a desaceleração máxima, em m/s²?"), a, "m/s²",
                        L.F("a = μ·g = {0}·9,8 = {1} m/s².", N(mu), N(a)));
                    case 1: return Ex.Num(intro + " " + L.T("Qual a distância só de frenagem, em metros?"), db, "m",
                        L.F("a = μ·g = {0} m/s²; d = v²/(2a) = {1}² / (2·{0}) = {2} m.", N(a), N(v), N(db)));
                    default: return Ex.Num(intro + " " + L.T("Qual a distância total para parar (reação + frenagem), em metros?"), dr + db, "m",
                        L.F("Reação: {0}·{1} = {2} m; frenagem: {3} m; total: {4} m.", N(v), N(t), N(dr), N(db), N(dr + db)));
                }
            }
            case Difficulty.Hard:
            {
                double D = Ex.Step(r, 25, 90, 5), vmax = -a * t + Math.Sqrt(a * a * t * t + 2 * a * D);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Há um obstáculo parado a {0} m. O motorista reage em {1} s e os pneus dão μ = {2} (g = 9,8 m/s²). Qual a maior velocidade inicial, em km/h, que ainda permite parar antes dele?", N(D), N(t), N(mu)), Kmh(vmax), "km/h",
                        L.F("v·t + v²/(2a) = D com a = {0} m/s². Resolvendo a equação do 2º grau: v = −a·t + √(a²·t² + 2·a·D) = {1} m/s = {2} km/h.", N(a), N(vmax), N(Kmh(vmax))));
                double mu2 = Ex.Step(r, 0.3, 0.5, 0.05), a2 = mu2 * G;
                return Ex.Num(L.F("A {0} km/h, em pista seca (μ = {1}) o carro freia numa certa distância; na chuva (μ = {2}) precisa de mais. Quantos metros A MAIS são necessários para frear na chuva? (g = 9,8 m/s², sem tempo de reação)", N(kmh), N(mu), N(mu2)), v * v / (2 * a2) - v * v / (2 * a), "m",
                    L.F("Seco: {0}²/(2·{1}) = {2} m. Chuva: {0}²/(2·{3}) = {4} m. Diferença: {5} m.", N(v), N(a), N(v * v / (2 * a)), N(a2), N(v * v / (2 * a2)), N(v * v / (2 * a2) - v * v / (2 * a))));
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    double slope = Ex.Step(r, 4, 12, 1), th = Math.Atan(slope / 100), aD = G * (mu * Math.Cos(th) - Math.Sin(th)), aU = G * (mu * Math.Cos(th) + Math.Sin(th));
                    if (r.Next(2) == 0)
                        return Ex.Num(L.F("Um carro a {0} km/h freia numa DESCIDA de {1}% (tg θ), com μ = {2} (g = 9,8 m/s²). Qual a distância de frenagem, em metros?", N(kmh), N(slope), N(mu)), v * v / (2 * aD), "m",
                            L.F("Na descida o peso continua empurrando o carro: a = g·(μ·cos θ − sen θ) = {0} m/s²; d = v²/(2a) = {1} m.", N(aD), N(v * v / (2 * aD))));
                    return Ex.Num(L.F("Um carro a {0} km/h freia numa SUBIDA de {1}% (tg θ), com μ = {2} (g = 9,8 m/s²). Qual a distância de frenagem, em metros?", N(kmh), N(slope), N(mu)), v * v / (2 * aU), "m",
                        L.F("Na subida o peso ajuda a frear: a = g·(μ·cos θ + sen θ) = {0} m/s²; d = v²/(2a) = {1} m.", N(aU), N(v * v / (2 * aU))));
                }
                double dist = Ex.Step(r, 40, 80, 5), v0 = Ms(Ex.Step(r, 90, 130, 10)), rest = Math.Max(0, dist - v0 * t), v2 = v0 * v0 - 2 * a * rest;
                string intro = L.F("Um carro vem a {0} km/h e o motorista vê um obstáculo parado a {1} m. Ele reage em {2} s e freia com μ = {3} (g = 9,8 m/s²).", N(Kmh(v0)), N(dist), N(t), N(mu));
                if (v2 > 0)
                    return Ex.Num(intro + " " + L.T("Se não consegue parar, bate. Qual a velocidade do impacto, em km/h?"), Kmh(Math.Sqrt(v2)), "km/h",
                        L.F("Durante a reação percorre {0} m; restam {1} m. v² = v₀² − 2·a·d = {2} → v = {3} km/h.", N(v0 * t), N(rest), N(v2), N(Kmh(Math.Sqrt(v2)))));
                double need = v0 * t + v0 * v0 / (2 * a);
                return Ex.Num(intro + " " + L.T("Qual a distância total necessária para parar, em metros?"), need, "m",
                    L.F("Reação: {0} m; frenagem: {1}²/(2·{2}) = {3} m; total: {4} m (compare com os {5} m disponíveis).", N(v0 * t), N(v0), N(a), N(v0 * v0 / (2 * a)), N(need), N(dist)));
            }
        }
    }
}
