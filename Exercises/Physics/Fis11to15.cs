namespace EasyCreator.Exercises;

/// <summary>Rotation: torque, moment of inertia, rolling, angular momentum.</summary>
[Exercises("fis-11-rotacao")]
public sealed class Fis11Rotacao : IExerciseGenerator
{
    const double G = P.G;

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                if (r.Next(2) == 0)
                {
                    double F = Ex.Step(r, 5, 200, 5), len = Ex.Step(r, 0.1, 1.5, 0.05), tau = F * len;
                    return Ex.Num(L.F("Uma força de {0} N é aplicada perpendicularmente na ponta de uma chave de {1} m. Qual o torque?", P.N(F, 0), P.N(len)), tau, "N·m", $"τ = F·r = {P.N(F, 0)}·{P.N(len)} = {P.N(tau)} N·m.");
                }
                double rpm = Ex.Step(r, 60, 3000, 60), w = rpm * 2 * Math.PI / 60;
                return Ex.Num(L.F("Um motor gira a {0} rpm. Qual a velocidade angular em rad/s?", P.N(rpm, 0)), w, "rad/s", $"ω = rpm·2π / 60 = {P.N(rpm, 0)}·2π / 60 = {P.N(w)} rad/s.");
            }
            case Difficulty.Medium:
            {
                double M = Ex.Step(r, 1, 20, 1), R = Ex.Step(r, 0.1, 1, 0.05), I = 0.5 * M * R * R;
                switch (r.Next(3))
                {
                    case 0:
                        return Ex.Num(L.F("Qual o momento de inércia de um disco maciço de {0} kg e {1} m de raio, girando em torno do seu eixo?", P.N(M, 0), P.N(R)), I, "kg·m²", $"I = ½ M R² = 0,5·{P.N(M, 0)}·{P.N(R)}² = {P.N(I, 3)} kg·m².");
                    case 1:
                        double tau = Ex.Step(r, 1, 30, 1), al = tau / I;
                        return Ex.Num(L.F("Um torque de {0} N·m age num disco maciço de {1} kg e {2} m de raio, em torno do seu eixo. Qual a aceleração angular?", P.N(tau, 0), P.N(M, 0), P.N(R)), al, "rad/s²", $"I = ½ M R² = {P.N(I, 3)} kg·m²; α = τ / I = {P.N(tau, 0)} / {P.N(I, 3)} = {P.N(al)} rad/s².");
                    default:
                        double w = Ex.Step(r, 2, 40, 1), Ek = 0.5 * I * w * w;
                        return Ex.Num(L.F("Um disco maciço de {0} kg e {1} m de raio gira a {2} rad/s. Qual a sua energia cinética de rotação?", P.N(M, 0), P.N(R), P.N(w, 0)), Ek, "J", $"I = {P.N(I, 3)} kg·m²; Ec = ½ I ω² = 0,5·{P.N(I, 3)}·{P.N(w, 0)}² = {P.N(Ek)} J.");
                }
            }
            case Difficulty.Hard:
            {
                var (name, k) = Ex.Pick(r, (L.T("um disco maciço"), 0.5), (L.T("uma esfera maciça"), 0.4), (L.T("um aro (anel fino)"), 1.0), (L.T("uma casca esférica"), 2.0 / 3));
                double h = Ex.Step(r, 0.2, 3, 0.1), v = Math.Sqrt(2 * G * h / (1 + k));
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("{0} rola sem deslizar, a partir do repouso, por uma rampa e desce {1} m na vertical. Qual a sua velocidade no fim da descida?", Cap(name), P.N(h)), v, "m/s",
                        $"m g h = ½ m v² + ½ I ω² = ½ m v² (1 + k), com k = I/(mR²) = {P.N(k, 3)} → v = √(2 g h / (1 + k)) = √(2·{P.N(G)}·{P.N(h)} / {P.N(1 + k, 3)}) = {P.N(v)} m/s.");
                double th = Ex.Pick(r, 15.0, 20, 30, 40), a = G * Math.Sin(P.Rad(th)) / (1 + k);
                return Ex.Num(L.F("{0} rola sem deslizar por uma rampa de {1}°. Qual a aceleração do centro de massa?", Cap(name), P.N(th)), a, "m/s²",
                    $"a = g sen θ / (1 + k), com k = I/(mR²) = {P.N(k, 3)} → a = {P.N(G)}·sen {P.N(th)}° / {P.N(1 + k, 3)} = {P.N(a)} m/s².");
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    double I1 = Ex.Step(r, 2, 6, 0.5), I2 = I1 / Ex.Pick(r, 2.0, 2.5, 4.0), w1 = Ex.Step(r, 1, 4, 0.5), w2 = I1 * w1 / I2;
                    return Ex.Num(L.F("Uma patinadora gira a {0} rad/s com momento de inércia de {1} kg·m². Ao encolher os braços, o momento de inércia cai para {2} kg·m². Qual a nova velocidade angular?", P.N(w1), P.N(I1), P.N(I2)),
                        w2, "rad/s", $"Sem torque externo L = I ω se conserva: ω₂ = I₁ ω₁ / I₂ = {P.N(I1)}·{P.N(w1)} / {P.N(I2)} = {P.N(w2)} rad/s.");
                }
                double m1 = Ex.Step(r, 1, 5, 1), m2 = m1 + Ex.Step(r, 1, 5, 1), M = Ex.Step(r, 1, 6, 1);
                double a = (m2 - m1) * G / (m1 + m2 + M / 2);
                return Ex.Num(L.F("Duas massas de {0} kg e {1} kg estão presas às pontas de um fio que passa por uma polia (disco maciço) de {2} kg, sem deslizar. Qual a aceleração das massas?", P.N(m1, 0), P.N(m2, 0), P.N(M, 0)),
                    a, "m/s²", $"A polia contribui com I/R² = M/2: a = (m₂ − m₁) g / (m₁ + m₂ + M/2) = ({P.N(m2, 0)} − {P.N(m1, 0)})·{P.N(G)} / {P.N(m1 + m2 + M / 2)} = {P.N(a)} m/s².");
            }
        }
    }

    static string Cap(string s) => s.Length == 0 ? s : char.ToUpper(s[0]) + s[1..];
}

/// <summary>Fluids: pressure, buoyancy, continuity, Torricelli, Bernoulli.</summary>
[Exercises("fis-12-fluidos")]
public sealed class Fis12Fluidos : IExerciseGenerator
{
    const double G = P.G;

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                if (r.Next(2) == 0)
                {
                    double h = Ex.Step(r, 1, 50, 1), p = 1000 * G * h;
                    return Ex.Num(L.F("Qual a pressão manométrica (acima da atmosférica), em kPa, a {0} m de profundidade na água? (ρ = 1000 kg/m³)", P.N(h, 0)), p / 1000, "kPa", $"P = ρ g h = 1000·{P.N(G)}·{P.N(h, 0)} = {P.N(p, 0)} Pa = {P.N(p / 1000)} kPa.");
                }
                double V = Ex.Step(r, 1, 50, 1), E = 1000 * G * V / 1000;
                return Ex.Num(L.F("Um objeto de {0} litros está totalmente submerso na água (ρ = 1000 kg/m³). Qual o empuxo?", P.N(V, 0)), E, "N", $"E = ρ g V = 1000·{P.N(G)}·{P.N(V / 1000, 3)} = {P.N(E)} N.");
            }
            case Difficulty.Medium:
            {
                switch (r.Next(3))
                {
                    case 0:
                        double rho = Ex.Step(r, 300, 900, 50), frac = rho / 1000;
                        return Ex.Num(L.F("Um bloco de densidade {0} kg/m³ flutua na água (1000 kg/m³). Que porcentagem do seu volume fica submersa?", P.N(rho, 0)), frac * 100, "%", $"Flutuando: ρ_bloco V g = ρ_água V_sub g → V_sub/V = {P.N(rho, 0)} / 1000 = {P.N(frac, 2)} = {P.N(frac * 100, 0)}%.");
                    case 1:
                        double A1 = Ex.Step(r, 2, 20, 1), A2 = Ex.Step(r, 1, 10, 1), v1 = Ex.Step(r, 1, 6, 0.5), v2 = A1 * v1 / A2;
                        return Ex.Num(L.F("A água escoa a {0} m/s por um cano de {1} cm² de área, que se estreita para {2} cm². Qual a velocidade na parte estreita?", P.N(v1), P.N(A1, 0), P.N(A2, 0)), v2, "m/s", $"A₁ v₁ = A₂ v₂ → v₂ = {P.N(A1, 0)}·{P.N(v1)} / {P.N(A2, 0)} = {P.N(v2)} m/s.");
                    default:
                        double h = Ex.Step(r, 0.2, 5, 0.1), v = Math.Sqrt(2 * G * h);
                        return Ex.Num(L.F("Um tanque aberto tem um furo {0} m abaixo da superfície da água. Com que velocidade a água sai? (Torricelli)", P.N(h)), v, "m/s", $"v = √(2 g h) = √(2·{P.N(G)}·{P.N(h)}) = {P.N(v)} m/s.");
                }
            }
            case Difficulty.Hard:
            {
                if (r.Next(2) == 0)
                {
                    double H = Ex.Step(r, 1, 3, 0.2), h = Ex.Step(r, 0.2, H - 0.2, 0.1), x = 2 * Math.Sqrt(h * (H - h));
                    return Ex.Num(L.F("Um tanque cheio de água está sobre o chão, com o nível a {0} m de altura. Um furo está a {1} m abaixo da superfície. A que distância horizontal do tanque o jato atinge o chão?", P.N(H), P.N(h)), x, "m",
                        $"v = √(2 g h); queda de altura H−h = {P.N(H - h)} m em t = √(2(H−h)/g); alcance x = v t = 2√(h (H−h)) = 2√({P.N(h)}·{P.N(H - h)}) = {P.N(x)} m.");
                }
                double a1 = Ex.Step(r, 1, 10, 1), a2 = a1 * Ex.Step(r, 5, 30, 5), F1 = Ex.Step(r, 10, 200, 10), F2 = F1 * a2 / a1;
                return Ex.Num(L.F("Numa prensa hidráulica, o êmbolo pequeno tem {0} cm² e o grande {1} cm². Que força é preciso aplicar no pequeno para o grande levantar {2} N?", P.N(a1, 0), P.N(a2, 0), P.N(F2, 0)), F1, "N",
                    $"Pressão igual: F₁/A₁ = F₂/A₂ → F₁ = F₂·A₁/A₂ = {P.N(F2, 0)}·{P.N(a1, 0)}/{P.N(a2, 0)} = {P.N(F1)} N.");
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    // Venturi: pressure drop in the narrow part of a horizontal pipe.
                    double A1 = Ex.Step(r, 10, 40, 5), ratio = Ex.Pick(r, 2.0, 3, 4), v1 = Ex.Step(r, 1, 4, 0.5);
                    double v2 = v1 * ratio, dP = 0.5 * 1000 * (v2 * v2 - v1 * v1);
                    return Ex.Num(L.F("A água escoa a {0} m/s num cano horizontal de {1} cm². Num estrangulamento de área {2} vezes menor a pressão cai. Qual a queda de pressão, em kPa? (ρ = 1000 kg/m³)", P.N(v1), P.N(A1, 0), P.N(ratio, 0)), dP / 1000, "kPa",
                        $"Continuidade: v₂ = {P.N(ratio, 0)}·{P.N(v1)} = {P.N(v2)} m/s. Bernoulli: ΔP = ½ ρ (v₂² − v₁²) = 0,5·1000·({P.N(v2)}² − {P.N(v1)}²) = {P.N(dP, 0)} Pa = {P.N(dP / 1000)} kPa.");
                }
                double m = Ex.Step(r, 1, 10, 1), rho = Ex.Pick(r, 2700.0, 7800, 8900, 11300), Wap = m * G * (1 - 1000 / rho);
                return Ex.Num(L.F("Um bloco de {0} kg e densidade {1} kg/m³ é pendurado por um fio e mergulhado totalmente na água (1000 kg/m³). Qual a tração no fio?", P.N(m, 0), P.N(rho, 0)), Wap, "N",
                    $"Peso aparente = peso − empuxo = m g (1 − ρ_água/ρ) = {P.N(m, 0)}·{P.N(G)}·(1 − 1000/{P.N(rho, 0)}) = {P.N(Wap)} N.");
            }
        }
    }
}

/// <summary>Waves and sound: strings, pipes, speed of sound, Doppler, beats.</summary>
[Exercises("fis-13-ondas-som")]
public sealed class Fis13Som : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                if (r.Next(2) == 0)
                {
                    double f = Ex.Step(r, 100, 2000, 50), lam = 340 / f;
                    return Ex.Num(L.F("Um som de {0} Hz se propaga no ar a 340 m/s. Qual o seu comprimento de onda?", P.N(f, 0)), lam, "m", $"λ = v / f = 340 / {P.N(f, 0)} = {P.N(lam, 3)} m.");
                }
                double T = Ex.Step(r, 5, 25, 1), v = 331 + 0.6 * T;
                return Ex.Num(L.F("Qual a velocidade do som no ar a {0} °C? (use v = 331 + 0,6·T)", P.N(T, 0)), v, "m/s", $"v = 331 + 0,6·{P.N(T, 0)} = {P.N(v)} m/s.");
            }
            case Difficulty.Medium:
            {
                double len = Ex.Step(r, 0.3, 1.2, 0.1), mu = Ex.Step(r, 1, 10, 1) / 1000, T = Ex.Step(r, 20, 200, 10);
                double v = Math.Sqrt(T / mu);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma corda de {0} g/m está sob tração de {1} N. Qual a velocidade das ondas nela?", P.N(mu * 1000, 0), P.N(T, 0)), v, "m/s", $"v = √(T/μ) = √({P.N(T, 0)} / {P.N(mu, 3)}) = {P.N(v)} m/s.");
                int n = r.Next(1, 4);
                double fn = n * v / (2 * len);
                return Ex.Num(L.F("Uma corda de {0} m, fixa nas duas pontas, tem velocidade de onda de {1} m/s. Qual a frequência do {2}º harmônico?", P.N(len), P.N(v), n), fn, "Hz", $"fₙ = n v / (2L) = {n}·{P.N(v)} / (2·{P.N(len)}) = {P.N(fn)} Hz.");
            }
            case Difficulty.Hard:
            {
                if (r.Next(2) == 0)
                {
                    double fs = Ex.Step(r, 400, 1000, 50), vs = Ex.Step(r, 10, 40, 5), fo = fs * 340 / (340 - vs);
                    return Ex.Num(L.F("Uma ambulância emite uma sirene de {0} Hz e se aproxima de você, parado, a {1} m/s. Qual a frequência que você ouve? (som a 340 m/s)", P.N(fs, 0), P.N(vs, 0)), fo, "Hz", $"f' = f·v / (v − vₛ) = {P.N(fs, 0)}·340 / (340 − {P.N(vs, 0)}) = {P.N(fo)} Hz.");
                }
                double L1 = Ex.Step(r, 0.2, 1, 0.1), v = 340;
                bool open = r.Next(2) == 0;
                double f1 = open ? v / (2 * L1) : v / (4 * L1);
                return Ex.Num(L.F("Qual a frequência fundamental de um tubo {0} de {1} m de comprimento? (som a 340 m/s)", open ? L.T("aberto nas duas pontas") : L.T("fechado numa ponta"), P.N(L1)), f1, "Hz",
                    open ? $"f₁ = v / 2L = 340 / (2·{P.N(L1)}) = {P.N(f1)} Hz." : $"f₁ = v / 4L = 340 / (4·{P.N(L1)}) = {P.N(f1)} Hz.");
            }
            default:
            {
                switch (r.Next(3))
                {
                    case 0:
                        double f1 = Ex.Step(r, 200, 500, 10), f2 = f1 * Ex.Step(r, 1.05, 1.3, 0.05), T1 = Ex.Step(r, 50, 150, 10), T2 = T1 * (f2 / f1) * (f2 / f1);
                        return Ex.Num(L.F("Uma corda de violão toca {0} Hz com tração de {1} N. Que tração (em N) faz a mesma corda tocar {2} Hz?", P.N(f1, 0), P.N(T1, 0), P.N(f2, 1)), T2, "N", $"f ∝ √T → T₂ = T₁ (f₂/f₁)² = {P.N(T1, 0)}·({P.N(f2, 1)}/{P.N(f1, 0)})² = {P.N(T2)} N.");
                    case 1:
                        double L1 = Ex.Step(r, 0.2, 1, 0.1), v = 340, f3 = 3 * v / (4 * L1);
                        return Ex.Num(L.F("Um tubo fechado numa ponta tem {0} m. Qual a frequência do seu 2º modo de ressonância (o terceiro harmônico, já que só existem harmônicos ímpares)? (som a 340 m/s)", P.N(L1)), f3, "Hz", $"Só n ímpar: fₙ = n v / 4L. Com n = 3: f = 3·340 / (4·{P.N(L1)}) = {P.N(f3)} Hz.");
                    default:
                        double fs = Ex.Step(r, 400, 800, 50), vs = Ex.Step(r, 10, 30, 5), vo = Ex.Step(r, 5, 20, 5);
                        double fo = fs * (340 + vo) / (340 - vs);
                        return Ex.Num(L.F("Uma fonte de {0} Hz se move a {1} m/s em direção a um observador, que também corre na direção da fonte a {2} m/s. Que frequência ele ouve? (som a 340 m/s)", P.N(fs, 0), P.N(vs, 0), P.N(vo, 0)), fo, "Hz",
                            $"f' = f (v + v₀) / (v − vₛ) = {P.N(fs, 0)}·({340} + {P.N(vo, 0)}) / ({340} − {P.N(vs, 0)}) = {P.N(fo)} Hz.");
                }
            }
        }
    }
}

/// <summary>Heat: calorimetry, phase changes, mixtures, conduction.</summary>
[Exercises("fis-14-calor")]
public sealed class Fis14Calor : IExerciseGenerator
{
    const double Cw = 4186, Lf = 334000;

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                double m = Ex.Step(r, 0.1, 5, 0.1), dT = Ex.Step(r, 5, 80, 5);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Quanto calor (em kJ) é preciso para aquecer {0} kg de água em {1} °C? (c = 4186 J/kg·°C)", P.N(m), P.N(dT, 0)), m * Cw * dT / 1000, "kJ", $"Q = m c ΔT = {P.N(m)}·4186·{P.N(dT, 0)} = {P.N(m * Cw * dT, 0)} J = {P.N(m * Cw * dT / 1000)} kJ.");
                return Ex.Num(L.F("Quanto calor (em kJ) é preciso para derreter {0} kg de gelo a 0 °C? (L = 334 kJ/kg)", P.N(m)), m * 334, "kJ", $"Q = m L = {P.N(m)}·334 = {P.N(m * 334)} kJ.");
            }
            case Difficulty.Medium:
            {
                if (r.Next(2) == 0)
                {
                    double m1 = Ex.Step(r, 0.5, 5, 0.5), m2 = Ex.Step(r, 0.5, 5, 0.5), T1 = Ex.Step(r, 60, 100, 5), T2 = Ex.Step(r, 5, 30, 5), Tf = (m1 * T1 + m2 * T2) / (m1 + m2);
                    return Ex.Num(L.F("Misturam-se {0} kg de água a {1} °C com {2} kg de água a {3} °C. Qual a temperatura final de equilíbrio (sem perdas)?", P.N(m1), P.N(T1, 0), P.N(m2), P.N(T2, 0)), Tf, "°C", $"Tf = (m₁T₁ + m₂T₂) / (m₁ + m₂) = ({P.N(m1)}·{P.N(T1, 0)} + {P.N(m2)}·{P.N(T2, 0)}) / {P.N(m1 + m2)} = {P.N(Tf)} °C.");
                }
                double k = Ex.Pick(r, 0.8, 1.0, 50.0, 200.0), A = Ex.Step(r, 0.5, 10, 0.5), len = Ex.Step(r, 0.01, 0.3, 0.01), dT = Ex.Step(r, 10, 40, 5);
                double Pw = k * A * dT / len;
                return Ex.Num(L.F("Uma parede de condutividade {0} W/m·K, {1} m² e {2} m de espessura tem uma diferença de {3} °C entre as faces. Que potência térmica a atravessa?", P.N(k), P.N(A), P.N(len), P.N(dT, 0)), Pw, "W", $"P = k A ΔT / L = {P.N(k)}·{P.N(A)}·{P.N(dT, 0)} / {P.N(len)} = {P.N(Pw)} W.");
            }
            case Difficulty.Hard:
            {
                if (r.Next(2) == 0)
                {
                    double m = Ex.Step(r, 0.1, 2, 0.1), T0 = Ex.Step(r, 5, 30, 5), Tf = Ex.Step(r, 20, 90, 10);
                    double Q = m * (2100 * T0 + Lf + Cw * Tf);
                    return Ex.Num(L.F("Quanto calor (em kJ) é preciso para transformar {0} kg de gelo a −{1} °C em água a {2} °C? (c do gelo = 2100 J/kg·°C, c da água = 4186 J/kg·°C, L = 334 kJ/kg)", P.N(m), P.N(T0, 0), P.N(Tf, 0)), Q / 1000, "kJ",
                        $"Aquecer o gelo: {P.N(m)}·2100·{P.N(T0, 0)} = {P.N(m * 2100 * T0, 0)} J; derreter: {P.N(m)}·334000 = {P.N(m * Lf, 0)} J; aquecer a água: {P.N(m)}·4186·{P.N(Tf, 0)} = {P.N(m * Cw * Tf, 0)} J. Total = {P.N(Q / 1000)} kJ.");
                }
                double mw = Ex.Step(r, 0.5, 2, 0.5), Tw = Ex.Step(r, 60, 95, 5), mi = Ex.Step(r, 0.05, 0.2, 0.05);
                double Tfin = (mw * Cw * Tw - mi * Lf) / ((mw + mi) * Cw);
                return Ex.Num(L.F("Colocam-se {0} kg de gelo a 0 °C em {1} kg de água a {2} °C. Qual a temperatura final? (c = 4186 J/kg·°C, L = 334 kJ/kg)", P.N(mi), P.N(mw), P.N(Tw, 0)), Tfin, "°C",
                    $"Calor cedido pela água esfriando até Tf = calor para derreter o gelo e aquecê-lo: {P.N(mw)}·4186·({P.N(Tw, 0)} − Tf) = {P.N(mi)}·334000 + {P.N(mi)}·4186·Tf → Tf = {P.N(Tfin)} °C.");
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    double mw = Ex.Step(r, 0.5, 2, 0.5), Tw = Ex.Step(r, 40, 90, 5), mi = Math.Ceiling(mw * Cw * Tw / Lf * 1.5 * 10) / 10 + 0.1;
                    double melted = mw * Cw * Tw / Lf;
                    return Ex.Num(L.F("Jogam-se {0} kg de gelo a 0 °C em {1} kg de água a {2} °C, num recipiente isolado. Quantos gramas de gelo derretem até o equilíbrio? (c = 4186 J/kg·°C, L = 334 kJ/kg)", P.N(mi), P.N(mw), P.N(Tw, 0)), melted * 1000, "g",
                        $"A água só pode ceder até esfriar a 0 °C: Q = {P.N(mw)}·4186·{P.N(Tw, 0)} = {P.N(mw * Cw * Tw, 0)} J, que derrete m = Q / L = {P.N(melted * 1000, 0)} g, menos que os {P.N(mi * 1000, 0)} g de gelo. O equilíbrio é a 0 °C.");
                }
                double k1 = Ex.Pick(r, 0.8, 1.0, 1.4), k2 = Ex.Pick(r, 0.04, 0.05, 0.1), l1 = Ex.Step(r, 0.1, 0.3, 0.05), l2 = Ex.Step(r, 0.02, 0.1, 0.01), A = Ex.Step(r, 5, 20, 1), dT = Ex.Step(r, 10, 30, 5);
                double Rt = l1 / k1 + l2 / k2, Pw = A * dT / Rt;
                return Ex.Num(L.F("Uma parede de {0} m² é feita de duas camadas em série: tijolo ({1} m, k = {2} W/m·K) e isolante ({3} m, k = {4} W/m·K). A diferença de temperatura total é {5} °C. Que potência térmica a atravessa?", P.N(A, 0), P.N(l1), P.N(k1), P.N(l2), P.N(k2), P.N(dT, 0)), Pw, "W",
                    $"Resistências em série: L₁/k₁ + L₂/k₂ = {P.N(l1)}/{P.N(k1)} + {P.N(l2)}/{P.N(k2)} = {P.N(Rt, 3)} m²·K/W; P = A ΔT / Σ(L/k) = {P.N(A, 0)}·{P.N(dT, 0)} / {P.N(Rt, 3)} = {P.N(Pw)} W.");
            }
        }
    }
}

/// <summary>Kepler's laws and the Hohmann transfer.</summary>
[Exercises("fis-15-kepler-hohmann")]
public sealed class Fis15Kepler : IExerciseGenerator
{
    const double GMe = 3.986e14, Re = 6.371e6;

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                double a = Ex.Step(r, 0.4, 30, 0.1);
                double T = Math.Pow(a, 1.5);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um planeta orbita o Sol com semieixo maior de {0} UA. Qual o seu período, em anos? (T² = a³, com a em UA e T em anos)", P.N(a, 1)), T, "anos", $"T = a^(3/2) = {P.N(a, 1)}^{P.N(1.5, 1)} = {P.N(T)} anos.");
                return Ex.Num(L.F("Um planeta orbita o Sol com período de {0} anos. Qual o seu semieixo maior, em UA? (T² = a³)", P.N(T)), a, "UA", $"a = T^(2/3) = {P.N(T)}^{P.N(2.0 / 3, 3)} = {P.N(a)} UA.");
            }
            case Difficulty.Medium:
            {
                double a1 = Ex.Step(r, 1, 5, 0.5), a2 = a1 * Ex.Pick(r, 2.0, 3, 4, 5), T1 = Ex.Step(r, 1, 10, 1), T2 = T1 * Math.Pow(a2 / a1, 1.5);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma lua orbita um planeta com período de {0} dias a {1} mil km. Outra lua do mesmo planeta orbita a {2} mil km. Qual o período dela, em dias?", P.N(T1, 0), P.N(a1 * 100, 0), P.N(a2 * 100, 0)), T2, "dias", $"T² ∝ a³ → T₂ = T₁ (a₂/a₁)^(3/2) = {P.N(T1, 0)}·({P.N(a2 * 100, 0)}/{P.N(a1 * 100, 0)})^{P.N(1.5, 1)} = {P.N(T2)} dias.");
                double alt = Ex.Step(r, 300, 1500, 100) * 1e3, rr = Re + alt, v = Math.Sqrt(GMe / rr);
                return Ex.Num(L.F("Qual a velocidade de um satélite em órbita circular a {0} km de altitude da Terra, em km/s? (GM = 3,986·10¹⁴ m³/s², raio da Terra = 6371 km)", P.N(alt / 1e3, 0)), v / 1e3, "km/s", $"v = √(GM / r) = √({P.Sci(GMe)} / {P.N(rr, 0)}) = {P.N(v)} m/s = {P.N(v / 1e3)} km/s.");
            }
            case Difficulty.Hard:
            {
                // Hohmann transfer between circular orbits around the Sun: half an ellipse of a = (r1 + r2) / 2.
                var (name, r1) = Ex.Pick(r, (L.T("da Terra"), 1.0), (L.T("de Vênus"), 0.723));
                double r2 = Ex.Step(r, 1.2, 10, 0.1), a = (r1 + r2) / 2;
                a = (r1 + r2) / 2;
                double t = 0.5 * Math.Pow(a, 1.5) * 365.25;
                return Ex.Num(L.F("Uma sonda faz uma transferência de Hohmann a partir da órbita {0} (raio {1} UA) para uma órbita circular de {2} UA em torno do Sol. Quantos dias dura a viagem?", name, P.N(r1, 3), P.N(r2, 3)), t, "dias",
                    $"Semieixo da elipse: a = (r₁ + r₂)/2 = {P.N(a, 3)} UA. A viagem é meia órbita: t = ½·a^(3/2) anos = 0,5·{P.N(Math.Pow(a, 1.5), 3)}·365,25 = {P.N(t, 0)} dias.".Replace("0,5·", P.N(0.5, 1) + "·").Replace("365,25", P.N(365.25)));
            }
            default:
            {
                // Delta-v of the first burn from a low Earth orbit to a higher circular orbit.
                double alt1 = Ex.Step(r, 200, 600, 50) * 1e3, r1 = Re + alt1, r2 = Ex.Pick(r, 42164e3, 26560e3, 20200e3 + Re, 10000e3 + Re);
                double v1 = Math.Sqrt(GMe / r1), dv = v1 * (Math.Sqrt(2 * r2 / (r1 + r2)) - 1);
                return Ex.Num(L.F("Um satélite está em órbita circular a {0} km de altitude da Terra. Para ir a uma órbita circular de raio {1} km por transferência de Hohmann, qual o Δv da PRIMEIRA queima (m/s)? (GM = 3,986·10¹⁴ m³/s², raio da Terra = 6371 km)", P.N(alt1 / 1e3, 0), P.N(r2 / 1e3, 0)), dv, "m/s",
                    $"v₁ = √(GM/r₁) = {P.N(v1, 0)} m/s. Na elipse, no periélio: v = v₁·√(2r₂/(r₁+r₂)) = {P.N(v1 * Math.Sqrt(2 * r2 / (r1 + r2)), 0)} m/s. Δv₁ = {P.N(dv, 0)} m/s.");
            }
        }
    }
}
