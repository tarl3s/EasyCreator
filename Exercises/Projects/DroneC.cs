using static EasyCreator.Exercises.Pv;

namespace EasyCreator.Exercises;

/// <summary>Drones 11-15: the LiPo battery, ideal battery size, motor KV, heavy lifters, PID for a heavy drone.</summary>
[Exercises("drone-11-lipo")]
public sealed class Drone11 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int cells = Ex.Int(r, 3, 6);
        double mah = Ex.Step(r, 1000, 10000, 250), c = Ex.Pick(r, 20.0, 25, 30, 40, 45, 60, 75, 100), rm = Ex.Step(r, 5, 25, 1);
        double v0 = cells * 3.7, imax = c * mah / 1000, rtot = cells * rm / 1000;
        string bat = L.F("Bateria LiPo {0}S de {1} mAh, {2}C, com resistência interna de {3} mΩ por célula.", N(cells, 0), N(mah, 0), N(c, 0), N(rm, 0));
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma bateria de {0} mAh e {1}C. Qual a corrente máxima contínua, em A?", N(mah, 0), N(c, 0)), imax, "A",
                        L.F("I_max = C × Ah = {0} × {1} = {2} A.", N(c, 0), N(mah / 1000), N(imax)));
                return Ex.Num(L.F("Uma bateria de {0} mAh entrega no máximo {1} A contínuos. Qual o seu C?", N(mah, 0), N(imax)), c, "C",
                    L.F("C = I / Ah = {0} / {1} = {2}.", N(imax), N(mah / 1000), N(c, 1)));
            case Difficulty.Medium:
            {
                double I = Ex.Step(r, 10, 60, 2);
                if (r.Next(2) == 0)
                    return Ex.Num(bat + " " + L.F("Qual a resistência interna total do pacote, em mΩ? (células em série somam)"), rtot * 1000, "mΩ",
                        L.F("R = {0} × {1} = {2} mΩ.", N(cells, 0), N(rm, 0), N(rtot * 1000, 0)));
                return Ex.Num(bat + " " + L.F("Com {0} A saindo, de quantos volts cai a tensão por causa da resistência interna? (ΔV = I·R)", N(I)), I * rtot, "V",
                    L.F("R = {0} mΩ = {1} Ω; ΔV = {2} × {1} = {3} V.", N(rtot * 1000, 0), N(rtot, 4), N(I), N(I * rtot, 3)), 3);
            }
            case Difficulty.Hard:
            {
                double P = Ex.Step(r, 100, 1200, 25), I = P / v0, need = I / (mah / 1000), heat = I * I * rtot;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(bat + " " + L.F("O drone consome {0} W. Qual a corrente (aproximando V pela tensão nominal), em A?", N(P)), I, "A",
                        L.F("V = {0}·3,7 = {1} V; I = P/V = {2}/{1} = {3} A.", N(cells, 0), N(v0), N(P), N(I)));
                    case 1: return Ex.Num(bat + " " + L.F("O drone consome {0} W (use a tensão nominal). Que C mínimo a bateria precisa ter para fornecer essa corrente?", N(P)), need, "C",
                        L.F("I = {0}/{1} = {2} A; C = I / Ah = {2} / {3} = {4}.", N(P), N(v0), N(I), N(mah / 1000), N(need)), 3);
                    default: return Ex.Num(bat + " " + L.F("O drone consome {0} W (corrente da tensão nominal). Quantos watts viram calor na resistência interna? (P = I²·R)", N(P)), heat, "W",
                        L.F("I = {0} A; P_calor = I²·R = {0}²·{1} = {2} W.", N(I), N(rtot, 4), N(heat, 3)), 3);
                }
            }
            default:
            {
                double P = Ex.Step(r, 200, 1500, 25);
                double disc = v0 * v0 - 4 * rtot * P;
                if (disc <= 0) { P = Math.Floor(v0 * v0 / (4 * rtot) * 0.6 / 25) * 25; disc = v0 * v0 - 4 * rtot * P; }
                double I = (v0 - Math.Sqrt(disc)) / (2 * rtot), Vl = v0 - I * rtot, eff = P / (v0 * I);
                if (r.Next(2) == 0)
                    return Ex.Num(bat + " " + L.F("A tensão sem carga é a nominal. O drone precisa de {0} W nos terminais. Resolvendo P = (V₀ − I·R)·I, qual a corrente real, em A?", N(P)), I, "A",
                        L.F("R·I² − V₀·I + P = 0 → I = (V₀ − √(V₀² − 4·R·P)) / (2R) = ({0} − √({0}² − 4·{1}·{2})) / (2·{1}) = {3} A.", N(v0), N(rtot, 4), N(P), N(I, 3)), 3);
                return Ex.Num(bat + " " + L.F("O drone precisa de {0} W nos terminais (tensão sem carga = nominal). Qual a eficiência da bateria, P / (V₀·I), em %?", N(P)), eff * 100, "%",
                    L.F("I = {0} A; tensão nos terminais = {1} V; eficiência = {2}/({3}·{0}) = {4}%.", N(I, 3), N(Vl, 3), N(P), N(v0), N(eff * 100)), 3);
            }
        }
    }
}

[Exercises("drone-12-bateria-ideal")]
public sealed class Drone12 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double m0 = Ex.Step(r, 0.5, 3, 0.1), p0 = Ex.Step(r, 80, 400, 10), wk = Ex.Step(r, 140, 220, 10);
        switch (d)
        {
            case Difficulty.Easy:
            {
                double mb = Ex.Step(r, 0.1, 1.5, 0.05), e = mb * wk;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma bateria de {0} kg com densidade de energia de {1} Wh/kg guarda quantos Wh?", N(mb), N(wk, 0)), e, "Wh",
                        L.F("E = {0} × {1} = {2} Wh.", N(mb), N(wk, 0), N(e)));
                return Ex.Num(L.F("Que massa de bateria, em kg, guarda {0} Wh a {1} Wh/kg?", N(e), N(wk, 0)), mb, "kg",
                    L.F("m = E / (Wh/kg) = {0} / {1} = {2} kg.", N(e), N(wk, 0), N(mb)));
            }
            case Difficulty.Medium:
            {
                double k = Ex.Pick(r, 1.2, 1.5, 2.0, 2.5), p1 = p0 * Math.Pow(k, 1.5);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone pairando gasta {0} W. Se a massa total for multiplicada por {1} (P ∝ massa^1,5), quantos watts passa a gastar?", N(p0), N(k)), p1, "W",
                        L.F("P₂ = P₁·k^1,5 = {0}·{1}^1,5 = {2} W.", N(p0), N(k), N(p1)));
                double mb = Ex.Step(r, 0.2, 1.2, 0.05), wh = mb * wk, pn = p0 * Math.Pow((m0 + mb) / m0, 1.5), t = 0.8 * wh / pn * 60;
                return Ex.Num(L.F("Um drone sem bateria pesa {0} kg e gastaria {1} W pairando com essa massa (P ∝ massa^1,5). Com uma bateria de {2} kg ({3} Wh/kg), quantos minutos paira, usando 80% da energia?", N(m0), N(p0), N(mb), N(wk, 0)), t, "min",
                    L.F("Massa total = {0} kg; P = {1}·(({0})/{2})^1,5 = {3} W; E = {4} Wh; t = 0,8·{4}/{3} = {5} min.", N(m0 + mb), N(p0), N(m0), N(pn), N(wh), N(t)), 3);
            }
            case Difficulty.Hard:
            {
                double mb = Ex.Step(r, 0.2, 1.5, 0.05), mb2 = mb + Ex.Step(r, 0.1, 0.6, 0.05);
                Func<double, double> time = b => 0.8 * b * wk / (p0 * Math.Pow((m0 + b) / m0, 1.5)) * 60;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um drone sem bateria pesa {0} kg e gastaria {1} W pairando com essa massa (P ∝ massa^1,5). Qual o tempo de pairado, em minutos, com uma bateria de {2} kg ({3} Wh/kg, 80% utilizável)?", N(m0), N(p0), N(mb), N(wk, 0)), time(mb), "min",
                        L.F("P = {0}·(({1}+{2})/{1})^1,5 = {3} W; t = 0,8·{2}·{4} / {3} = {5} min.", N(p0), N(m0), N(mb), N(p0 * Math.Pow((m0 + mb) / m0, 1.5)), N(wk, 0), N(time(mb))), 3);
                    default: return Ex.Num(L.F("Um drone sem bateria pesa {0} kg e gastaria {1} W pairando com essa massa (P ∝ massa^1,5). Quantos minutos A MAIS (ou a menos, se negativo) de pairado se ganha trocando uma bateria de {2} kg por uma de {3} kg ({4} Wh/kg, 80% utilizável)?", N(m0), N(p0), N(mb), N(mb2), N(wk, 0)), time(mb2) - time(mb), "min",
                        L.F("t({0} kg) = {1} min; t({2} kg) = {3} min; diferença = {4} min.", N(mb), N(time(mb)), N(mb2), N(time(mb2)), N(time(mb2) - time(mb))), 3);
                }
            }
            default:
            {
                double mbOpt = 2 * m0, tOpt = 0.8 * mbOpt * wk / (p0 * Math.Pow((m0 + mbOpt) / m0, 1.5)) * 60;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone sem bateria pesa {0} kg (P ∝ massa^1,5; a bateria tem {1} Wh/kg). Que massa de bateria, em kg, dá o MAIOR tempo de pairado? (dica: t ∝ m_b / (m₀ + m_b)^1,5)", N(m0), N(wk, 0)), mbOpt, "kg",
                        L.F("d/dm [m·(m₀ + m)^−1,5] = 0 → m₀ + m = 1,5·m → m = 2·m₀ = 2·{0} = {1} kg.", N(m0), N(mbOpt)), 3);
                return Ex.Num(L.F("Um drone sem bateria pesa {0} kg e gastaria {1} W pairando com essa massa (P ∝ massa^1,5). Com a bateria ideal (de massa 2·m₀, a {2} Wh/kg e 80% utilizável), qual o tempo máximo de pairado, em minutos?", N(m0), N(p0), N(wk, 0)), tOpt, "min",
                    L.F("Massa total = 3·m₀ = {0} kg; P = {1}·3^1,5 = {2} W; E = {3} Wh; t = 0,8·{3}/{2} = {4} min.", N(3 * m0), N(p0), N(p0 * Math.Pow(3, 1.5)), N(mbOpt * wk), N(tOpt)), 3);
            }
        }
    }
}

[Exercises("drone-13-motor-kv")]
public sealed class Drone13 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double kv = Ex.Step(r, 300, 3000, 50), cells = Ex.Int(r, 3, 8), v = cells * 3.7, rpm = kv * v;
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um motor de {0} KV com bateria de {1} V. Qual a rotação sem carga, em rpm?", N(kv, 0), N(v)), rpm, "rpm",
                        L.F("rpm = KV × V = {0} × {1} = {2} rpm.", N(kv, 0), N(v), N(rpm, 0)));
                return Ex.Num(L.F("Um motor gira a {0} rpm sem carga com {1} V. Qual o seu KV?", N(rpm, 0), N(v)), kv, "KV",
                    L.F("KV = rpm / V = {0} / {1} = {2}.", N(rpm, 0), N(v), N(kv, 1)));
            case Difficulty.Medium:
            {
                double kt = 60 / (2 * Math.PI * kv), I = Ex.Step(r, 2, 40, 1);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um motor de {0} KV. Qual a constante de torque Kt = 60 / (2π·KV), em N·m/A?", N(kv, 0)), kt, "N·m/A",
                        L.F("Kt = 60 / (2π·{0}) = {1} N·m/A.", N(kv, 0), N(kt, 5)), 3);
                return Ex.Num(L.F("Um motor de {0} KV puxa {1} A. Qual o torque, em N·m? (T = Kt·I, Kt = 60/(2π·KV))", N(kv, 0), N(I)), kt * I, "N·m",
                    L.F("Kt = {0} N·m/A; T = Kt·I = {0}·{1} = {2} N·m.", N(kt, 5), N(I), N(kt * I, 4)), 3);
            }
            case Difficulty.Hard:
            {
                double pitch = Ex.Step(r, 3, 8, 0.5), load = Ex.Step(r, 0.7, 0.85, 0.01), vp = pitch * 0.0254 * rpm * load / 60;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um motor de {0} KV com {1} V gira a {2}% da rotação sem carga sob carga, com hélice de passo {3}\". Qual a velocidade de passo, em m/s? (v = passo·rpm/60)", N(kv, 0), N(v), N(load * 100, 0), N(pitch)), vp, "m/s",
                        L.F("rpm = {0}·{1}·{2} = {3}; v = {4}·0,0254·{3}/60 = {5} m/s.", N(kv, 0), N(v), N(load), N(rpm * load, 0), N(pitch), N(vp)), 3);
                    default:
                    {
                        double vx = Ms(Ex.Step(r, 60, 160, 10)), rpmN = vx / (pitch * 0.0254) * 60 / load, kvN = rpmN / v;
                        return Ex.Num(L.F("Um drone com bateria de {0} V e hélice de passo {1}\" quer velocidade de passo de {2} m/s com o motor a {3}% da rotação sem carga. Que KV o motor precisa?", N(v), N(pitch), N(vx), N(load * 100, 0)), kvN, "KV",
                            L.F("rpm = v·60 / (passo·0,0254) = {0}; sem carga = {0} / {1} = {2}; KV = {2} / {3} = {4}.", N(vx * 60 / (pitch * 0.0254), 0), N(load), N(rpmN, 0), N(v), N(kvN, 1)), 3);
                    }
                }
            }
            default:
            {
                int n = Ex.Pick(r, 4, 6, 8);
                double mTot = Ex.Step(r, 1.0, 5.0, 0.25), tw = Ex.Step(r, 1.8, 3, 0.1), T = mTot * 9.8 * tw / n, mt = Ex.Step(r, 0.02, 0.1, 0.005), eff = Ex.Step(r, 5, 9, 0.5);
                double Pel = T * 1000 / 9.8 / eff;
                double kt = 60 / (2 * Math.PI * kv), Itarget = Ex.Step(r, 10, 50, 2), tq = kt * Itarget, om = rpm * 0.8 * 2 * Math.PI / 60, Pm = tq * om;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um motor de {0} KV com {1} V, sob carga a 80% da rotação sem carga, puxa {2} A. Qual a potência mecânica no eixo, em W? (P = torque·ω, torque = Kt·I)", N(kv, 0), N(v), N(Itarget)), Pm, "W",
                        L.F("Kt = {0} N·m/A; torque = {1} N·m; ω = 0,8·{2}·2π/60 = {3} rad/s; P = {4} W.", N(kt, 5), N(tq, 4), N(rpm, 0), N(om), N(Pm)), 3);
                return Ex.Num(L.F("Cada um dos {0} motores de um drone de {1} kg precisa dar empuxo de T/W = {2} no total. Com eficiência de {3} gf/W, qual a potência elétrica, em W, que cada motor consome ao empuxo máximo? (g = 9,8 m/s²)", N(n, 0), N(mTot), N(tw), N(eff)), Pel, "W",
                    L.F("Empuxo por motor = {0} N = {1} gf; P = gf / (gf/W) = {1} / {2} = {3} W.", N(T, 3), N(T * 1000 / 9.8, 1), N(eff), N(Pel)), 3);
            }
        }
    }
}

[Exercises("drone-14-carga-pesada")]
public sealed class Drone14 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        int n = Ex.Pick(r, 4, 6, 8);
        double m = Ex.Step(r, 3, 25, 0.5), tw = Ex.Pick(r, 1.5, 2.0, 2.5), per = m * tw / n;
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone de {0} kg com {1} motores e T/W = {2}. Quantos kg de empuxo máximo cada motor precisa dar?", N(m), N(n, 0), N(tw)), per, "kg",
                        L.F("empuxo por motor = massa × T/W / n = {0}·{1}/{2} = {3} kg.", N(m), N(tw), N(n, 0), N(per, 3)), 3);
                return Ex.Num(L.F("Um drone de {0} motores, cada um com {1} kg de empuxo máximo, tem T/W = {2}. Qual a massa total, em kg?", N(n, 0), N(per, 3), N(tw)), m, "kg",
                    L.F("massa = n × empuxo / (T/W) = {0}·{1}/{2} = {3} kg.", N(n, 0), N(per, 3), N(tw), N(m)), 3);
            case Difficulty.Medium:
            {
                double P = Ex.Step(r, 500, 6000, 100), v4 = 4 * 3.7, v6 = 6 * 3.7;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("O drone consome {0} W. Qual a corrente numa bateria 4S (14,8 V)?", N(P)), P / v4, "A",
                        L.F("I = P/V = {0} / {1} = {2} A.", N(P), N(v4), N(P / v4)));
                return Ex.Num(L.F("O drone consome {0} W. Quantos amperes a menos passam ao trocar uma bateria 4S (14,8 V) por uma 6S (22,2 V)?", N(P)), P / v4 - P / v6, "A",
                    L.F("{0}/{1} − {0}/{2} = {3} − {4} = {5} A.", N(P), N(v4), N(v6), N(P / v4), N(P / v6), N(P / v4 - P / v6)));
            }
            case Difficulty.Hard:
            {
                double cap = (n - 1.0) / n;
                int fail = Ex.Pick(r, 1, 1, 2);
                double capF = (n - (double)fail) / n, twF = tw * capF;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um drone de {0} kg tem {1} motores iguais e T/W = {2}. Se {3} motor(es) falhar(em), qual o T/W que sobra?", N(m), N(n, 0), N(tw), N(fail, 0)), twF, "",
                        L.F("T/W = {0} × ({1} − {2}) / {1} = {3}.", N(tw), N(n, 0), N(fail, 0), N(twF, 3)), 3);
                    default: return Ex.Choice(r, L.F("Um drone de {0} kg tem {1} motores iguais e T/W = {2}. Se {3} motor(es) falhar(em), ele ainda consegue pairar (T/W precisa ser ≥ 1, supondo controle capaz de compensar)?", N(m), N(n, 0), N(tw), N(fail, 0)),
                        twF >= 1 ? L.T("Sim, T/W restante ≥ 1") : L.T("Não, T/W restante < 1"),
                        new[] { twF >= 1 ? L.T("Não, T/W restante < 1") : L.T("Sim, T/W restante ≥ 1") },
                        L.F("T/W restante = {0} × ({1} − {2}) / {1} = {3}.", N(tw), N(n, 0), N(fail, 0), N(twF, 3)));
                }
            }
            default:
            {
                double perMotor = Ex.Step(r, 3, 9, 0.5), twT = Ex.Pick(r, 1.5, 2.0, 2.5), frame = Ex.Step(r, 1.5, 5, 0.25);
                double massMax = n * perMotor / twT, pay = massMax - frame;
                if (r.Next(2) == 0 && pay > 0.2)
                    return Ex.Num(L.F("Um drone de {0} motores de {1} kg de empuxo máximo cada, mantendo T/W = {2}, tem {3} kg de quadro, motores e bateria. Qual a carga útil máxima, em kg?", N(n, 0), N(perMotor), N(twT), N(frame)), pay, "kg",
                        L.F("massa máxima = {0}·{1}/{2} = {3} kg; carga útil = {3} − {4} = {5} kg.", N(n, 0), N(perMotor), N(twT), N(massMax), N(frame), N(pay)), 3);
                int failed = 1;
                double massOne = (n - failed) * perMotor / 1.0;
                return Ex.Num(L.F("Um drone de {0} motores de {1} kg de empuxo máximo cada precisa conseguir pairar (T/W = 1) mesmo com 1 motor falhado. Qual a massa máxima, em kg?", N(n, 0), N(perMotor)), massOne, "kg",
                    L.F("({0} − 1) × {1} / 1 = {2} kg.", N(n, 0), N(perMotor), N(massOne)));
            }
        }
    }
}

[Exercises("drone-15-pid-pesado")]
public sealed class Drone15 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double tw = Ex.Step(r, 1.5, 3.0, 0.1), hov = 1 / tw, kp = Ex.Step(r, 0.2, 3, 0.05), K = 9.8 * tw;
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone pesado tem T/W = {0}. Que fração do acelerador (em %) ele usa para pairar?", N(tw)), hov * 100, "%",
                        L.F("1 / {0} = {1}%.", N(tw), N(hov * 100)));
                return Ex.Num(L.F("Um drone paira a {0}% do acelerador. Qual o seu T/W?", N(hov * 100)), tw, "",
                    L.F("T/W = 1 / {0} = {1}.", N(hov, 3), N(tw, 3)), 3);
            case Difficulty.Medium:
            {
                double ess = hov / kp;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone pesado paira a {0}% do acelerador. Com controle só P (Kp = {1} por metro), qual o erro de altura em regime, em m?", N(hov * 100), N(kp)), ess, "m",
                        L.F("erro = acelerador de pairar / Kp = {0} / {1} = {2} m.", N(hov, 3), N(kp), N(ess, 3)), 3);
                return Ex.Num(L.F("Com T/W = {0} e Kp = {1} (por metro), qual a frequência natural ωn = √(Kp·K), com K = g·(T/W) e g = 9,8 m/s², em rad/s?", N(tw), N(kp)), Math.Sqrt(kp * K), "rad/s",
                    L.F("K = 9,8·{0} = {1}; ωn = √({2}·{1}) = {3} rad/s.", N(tw), N(K), N(kp), N(Math.Sqrt(kp * K), 3)), 3);
            }
            case Difficulty.Hard:
            {
                double emax = Ex.Step(r, 0.05, 0.4, 0.05), kpN = hov / emax, kd = Ex.Step(r, 0.1, 2.0, 0.05), wn = Math.Sqrt(kp * K), zeta = kd * K / (2 * wn);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone pesado paira a {0}% do acelerador. Que Kp mínimo (por metro) mantém o erro de regime só-P em até {1} m?", N(hov * 100), N(emax)), kpN, "",
                        L.F("Kp = acelerador de pairar / erro = {0} / {1} = {2}.", N(hov, 3), N(emax), N(kpN, 3)), 3);
                return Ex.Num(L.F("Modelo: e'' + (Kd·K)·e' + (Kp·K)·e = 0, K = g·(T/W). Com T/W = {0}, Kp = {1} e Kd = {2}, qual o amortecimento ζ = Kd·K / (2·ωn)? (g = 9,8 m/s²)", N(tw), N(kp), N(kd)), zeta, "",
                    L.F("K = {0}; ωn = √(Kp·K) = {1}; ζ = {2}·{0} / (2·{1}) = {3}.", N(K), N(wn, 3), N(kd), N(zeta, 3)), 3);
            }
            default:
            {
                double zt = Ex.Pick(r, 0.5, 0.6, 0.7, 0.8), wn = Math.Sqrt(kp * K), kd = 2 * zt * wn / K, os = Math.Exp(-Math.PI * zt / Math.Sqrt(1 - zt * zt)) * 100;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Modelo: e'' + (Kd·K)·e' + (Kp·K)·e = 0, K = g·(T/W), g = 9,8 m/s². Com T/W = {0} e Kp = {1}, que Kd dá ζ = {2}?", N(tw), N(kp), N(zt)), kd, "",
                        L.F("K = {0}; ωn = √({1}·{0}) = {2}; Kd = 2·ζ·ωn / K = 2·{3}·{2} / {0} = {4}.", N(K), N(kp), N(wn, 3), N(zt), N(kd, 3)), 3);
                return Ex.Num(L.F("Um sistema de 2ª ordem tem frequência natural de {1} rad/s e amortecimento ζ = {0}. Qual o sobressinal (overshoot) da resposta ao degrau, em %? (overshoot = e^(−π·ζ/√(1 − ζ²)))", N(zt), N(wn, 3)), os, "%",
                    L.F("overshoot = exp(−π·{0}/√(1 − {0}²)) = {1}%.", N(zt), N(os)), 3);
            }
        }
    }
}
