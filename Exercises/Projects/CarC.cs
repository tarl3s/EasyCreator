using static EasyCreator.Exercises.Pv;

namespace EasyCreator.Exercises;

/// <summary>Cars 11-15: top speed, gear spacing, brake balance, braking energy, downforce.</summary>
[Exercises("car-11-velocidade-maxima")]
public sealed class Car11 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G, Rho = Pk.Rho;
        double cd = Ex.Step(r, 0.25, 0.40, 0.01), A = Ex.Step(r, 1.8, 2.6, 0.1), P = Ex.Step(r, 60, 400, 10) * 1000;
        double vP = Math.Cbrt(2 * P / (Rho * cd * A));
        switch (d)
        {
            case Difficulty.Easy:
            {
                double kmh = Ex.Step(r, 100, 300, 10), v = Ms(kmh), Pa = 0.5 * Rho * cd * A * v * v * v;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um carro com Cd = {0} e área frontal de {1} m² anda a {2} km/h. Quantos kW o ar consome? (ρ = 1,2 kg/m³)", N(cd), N(A), N(kmh)), Pa / 1000, "kW",
                        L.F("v = {0} m/s; P = ½·ρ·Cd·A·v³ = ½·1,2·{1}·{2}·{0}³ = {3} kW.", N(v), N(cd), N(A), N(Pa / 1000)));
                    default: return Ex.Num(L.F("Um carro gasta {0} kW só contra o ar a {1} km/h, com área frontal de {2} m². Qual o seu Cd? (ρ = 1,2 kg/m³)", N(Pa / 1000), N(kmh), N(A)), cd, "",
                        L.F("Cd = 2·P / (ρ·A·v³) = 2·{0} / (1,2·{1}·{2}³) = {3}.", N(Pa), N(A), N(v), N(cd, 3)), 3);
                }
            }
            case Difficulty.Medium:
            {
                string intro = L.F("Um carro com Cd = {0} e área frontal de {1} m² recebe {2} kW nas rodas. Despreze o rolamento e o limite de giro do motor (ρ = 1,2 kg/m³).", N(cd), N(A), N(P / 1000));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Qual a velocidade máxima, em km/h?"), Kmh(vP), "km/h",
                        L.F("P = ½·ρ·Cd·A·v³ → v = ∛(2·P / (ρ·Cd·A)) = ∛(2·{0} / (1,2·{1}·{2})) = {3} m/s = {4} km/h.", N(P), N(cd), N(A), N(vP), N(Kmh(vP))));
                return Ex.Num(intro + " " + L.T("Qual a força de arrasto nessa velocidade máxima, em N?"), P / vP, "N",
                    L.F("v = {0} m/s; na velocidade máxima toda a potência vira arrasto: F = P / v = {1} / {0} = {2} N.", N(vP), N(P), N(P / vP)));
            }
            case Difficulty.Hard:
            {
                double m = Ex.Step(r, 1000, 1700, 100), crr = 0.012, vT = TopSpeed(P, cd, A, m, crr);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um carro de {0} kg (Cd = {1}, área {2} m², Crr = 0,012) recebe {3} kW nas rodas. Qual a velocidade máxima, em km/h, contando o rolamento? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", N(m), N(cd), N(A), N(P / 1000)), Kmh(vT), "km/h",
                        L.F("Equilíbrio: P = (½·ρ·Cd·A·v² + Crr·m·g)·v. Resolvendo por tentativas: v = {0} m/s = {1} km/h.", N(vT), N(Kmh(vT))), 3);
                double rpmP = Ex.Step(r, 5500, 7500, 250), rw = Ex.Step(r, 0.30, 0.35, 0.01), itot = rpmP * 2 * Math.PI / 60 * rw / vP;
                return Ex.Num(L.F("Um carro tem {0} kW e pneus de raio {1} m. Despreze o rolamento. Qual deve ser a relação total (marcha × diferencial) para que, na velocidade máxima aerodinâmica (Cd = {2}, área {3} m²), o motor gire a {4} rpm, onde está a potência máxima? (ρ = 1,2 kg/m³)", N(P / 1000), N(rw), N(cd), N(A), N(rpmP)), itot, "",
                    L.F("v_máx = ∛(2P/(ρ·Cd·A)) = {0} m/s; i = ω·r / v = {1}·{2} / {0} = {3}.", N(vP), N(rpmP * 2 * Math.PI / 60), N(rw), N(itot)), 3);
            }
            default:
            {
                double k = Ex.Step(r, 0.05, 0.20, 0.05), P2 = P * 1.0, v1 = vP;
                if (r.Next(2) == 0)
                {
                    double v2 = Kmh(vP) * Math.Cbrt(1 / (1 - k));
                    return Ex.Num(L.F("Um carro chega a {0} km/h de velocidade máxima (só arrasto). Uma carenagem reduz o Cd em {1}%, com a mesma potência. Qual a nova velocidade máxima, em km/h?", N(Kmh(vP)), N(k * 100, 0)), v2, "km/h",
                        L.F("v ∝ (1/Cd)^(1/3): {0} · (1/(1 − {1}))^(1/3) = {2} km/h.", N(Kmh(vP)), N(k), N(v2)));
                }
                double target = Ex.Step(r, 250, 350, 10), Pn = 0.5 * Rho * cd * A * Math.Pow(Ms(target), 3);
                return Ex.Num(L.F("Um carro com Cd = {0} e área frontal de {1} m² precisa de quantos kW nas rodas para chegar a {2} km/h só contra o ar? (ρ = 1,2 kg/m³)", N(cd), N(A), N(target)), Pn / 1000, "kW",
                    L.F("P = ½·ρ·Cd·A·v³ = ½·1,2·{0}·{1}·{2}³ = {3} kW (a potência cresce com o CUBO da velocidade).", N(cd), N(A), N(Ms(target)), N(Pn / 1000)));
            }
        }
    }
}

[Exercises("car-12-escalonamento")]
public sealed class Car12 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double rw = Ex.Step(r, 0.28, 0.36, 0.01), dif = Ex.Step(r, 3.2, 4.5, 0.1);
        int n = Ex.Int(r, 4, 6);
        double i1 = Ex.Step(r, 2.8, 3.8, 0.1), iN = Ex.Step(r, 0.65, 0.95, 0.05), q = Math.Pow(i1 / iN, 1.0 / (n - 1));
        double red = Ex.Step(r, 6000, 8000, 250);
        switch (d)
        {
            case Difficulty.Easy:
            {
                double rpm = Ex.Step(r, 2000, 7000, 250), g = Ex.Step(r, 0.8, 3.8, 0.1), v = rpm * 2 * Math.PI / 60 * rw / (g * dif);
                return Ex.Num(L.F("Com pneus de raio {0} m e diferencial {1}, qual a velocidade, em km/h, a {2} rpm numa marcha de relação {3}?", N(rw), N(dif), N(rpm), N(g)), Kmh(v), "km/h",
                    L.F("v = rpm·2π/60·r / (i·d) = {0}·{1} / ({2}·{3}) = {4} m/s = {5} km/h.", N(rpm * 2 * Math.PI / 60), N(rw), N(g), N(dif), N(v), N(Kmh(v))));
            }
            case Difficulty.Medium:
            {
                double ia = Ex.Step(r, 2.0, 3.5, 0.1), ib = ia * Ex.Step(r, 0.6, 0.8, 0.05), rpm = Ex.Step(r, 5500, 7500, 250), after = rpm * ib / ia;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("O motor muda de uma marcha de relação {0} para a seguinte, de relação {1}, a {2} rpm. A quantas rpm ele cai logo após a troca?", N(ia), N(ib), N(rpm)), after, "rpm",
                        L.F("A velocidade não muda na troca: rpm₂ = rpm₁·i₂/i₁ = {0}·{1}/{2} = {3} rpm.", N(rpm), N(ib), N(ia), N(after)));
                    default: return Ex.Num(L.F("Numa troca de marcha, o motor cai de {0} para {1} rpm. A marcha anterior tem relação {2}. Qual a relação da marcha seguinte?", N(rpm), N(after), N(ia)), ib, "",
                        L.F("i₂ = i₁·rpm₂/rpm₁ = {0}·{1}/{2} = {3}.", N(ia), N(after), N(rpm), N(ib)));
                }
            }
            case Difficulty.Hard:
            {
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um câmbio de {0} marchas tem 1ª com relação {1} e última com relação {2}, em progressão geométrica. Qual a razão q = i_k / i_(k+1) entre marchas consecutivas?", N(n, 0), N(i1), N(iN)), q, "",
                        L.F("q = (i₁/i_última)^(1/(N−1)) = ({0}/{1})^(1/{2}) = {3}.", N(i1), N(iN), N(n - 1, 0), N(q, 3)), 3);
                    default:
                    {
                        int k = Ex.Int(r, 2, n - 1);
                        double ik = i1 / Math.Pow(q, k - 1);
                        return Ex.Num(L.F("Um câmbio de {0} marchas em progressão geométrica tem 1ª com relação {1} e {0}ª com relação {2}. Qual a relação da {3}ª marcha?", N(n, 0), N(i1), N(iN), N(k, 0)), ik, "",
                            L.F("q = ({0}/{1})^(1/{2}) = {3}; i_{4} = i₁ / q^({4}−1) = {5}.", N(i1), N(iN), N(n - 1, 0), N(q, 3), N(k, 0), N(ik)), 3);
                    }
                }
            }
            default:
            {
                // Speed at the redline in each gear of a geometric box, and the rpm after the shift.
                int k = Ex.Int(r, 1, n - 1);
                double ik = i1 / Math.Pow(q, k - 1), ik1 = ik / q, vShift = red * 2 * Math.PI / 60 * rw / (ik * dif), after = red / q;
                string intro = L.F("Um câmbio de {0} marchas em progressão geométrica tem 1ª com relação {1} e última com relação {2}; o diferencial é {3}, os pneus têm raio {4} m e o limitador está em {5} rpm.", N(n, 0), N(i1), N(iN), N(dif), N(rw), N(red));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.F("Subindo da {0}ª para a {1}ª no limitador, a quantas rpm o motor cai?", N(k, 0), N(k + 1, 0)), after, "rpm",
                        L.F("A queda de giro é a mesma em toda troca: rpm₂ = rpm₁ / q, com q = ({0}/{1})^(1/{2}) = {3}. rpm₂ = {4}.", N(i1), N(iN), N(n - 1, 0), N(q, 3), N(after)), 3);
                return Ex.Num(intro + " " + L.F("A que velocidade, em km/h, o motor atinge o limitador na {0}ª marcha?", N(k, 0)), Kmh(vShift), "km/h",
                    L.F("i_{0} = {1}·q^−({0}−1) = {2}; v = {3}·2π/60·{4} / ({2}·{5}) = {6} m/s = {7} km/h.", N(k, 0), N(i1), N(ik), N(red), N(rw), N(dif), N(Ms(Kmh(vShift))), N(Kmh(vShift))), 3);
            }
        }
    }
}

[Exercises("car-13-balanco-freio")]
public sealed class Car13 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double m = Ex.Step(r, 900, 1800, 50), h = Ex.Step(r, 0.45, 0.65, 0.01), L2 = Ex.Step(r, 2.4, 2.9, 0.05), wf = Ex.Step(r, 0.45, 0.62, 0.01), mu = Ex.Step(r, 0.7, 1.1, 0.05);
        string car = L.F("Um carro de {0} kg tem {1}% do peso na frente, distância entre eixos de {2} m e CG a {3} m de altura (g = 9,8 m/s²).", N(m), N(wf * 100, 0), N(L2), N(h));
        switch (d)
        {
            case Difficulty.Easy:
            {
                double a = Ex.Step(r, 3, 9, 0.5), dW = m * a * h / L2;
                return Ex.Num(car + " " + L.F("Frenando a {0} m/s², quantos newtons de carga passam para o eixo dianteiro?", N(a)), dW, "N",
                    L.F("ΔW = m·a·h / L = {0}·{1}·{2} / {3} = {4} N.", N(m), N(a), N(h), N(L2), N(dW)));
            }
            case Difficulty.Medium:
            {
                double a = Ex.Step(r, 3, 9, 0.5), Nf = m * G * wf + m * a * h / L2;
                if (r.Next(2) == 0)
                    return Ex.Num(car + " " + L.F("Frenando a {0} m/s², qual a carga total no eixo dianteiro, em N?", N(a)), Nf, "N",
                        L.F("Parada: {0} N; transferência: m·a·h/L = {1} N; total: {2} N.", N(m * G * wf), N(m * a * h / L2), N(Nf)));
                return Ex.Num(car + " " + L.F("Frenando a {0} m/s², que fração do peso total (em %) está no eixo traseiro?", N(a)), (1 - Nf / (m * G)) * 100, "%",
                    L.F("Dianteiro: {0}% do peso; traseiro: 100 − {0} = {1}%.", N(Nf / (m * G) * 100), N((1 - Nf / (m * G)) * 100)));
            }
            case Difficulty.Hard:
            {
                double ideal = wf + mu * h / L2;
                if (r.Next(2) == 0)
                    return Ex.Num(car + " " + L.F("Os pneus têm μ = {0}. Qual a fração ideal da força de frenagem na frente (em %) para as 4 rodas travarem juntas?", N(mu)), ideal * 100, "%",
                        L.F("A frenagem máxima é a = μ·g. Carga na frente = m·g·(wf + μ·h/L), então o balanço ideal = wf + μ·h/L = {0} + {1}·{2}/{3} = {4}.", N(wf), N(mu), N(h), N(L2), N(ideal, 3)));
                double Ff = m * mu * G * ideal;
                return Ex.Num(car + " " + L.F("Os pneus têm μ = {0}. Na frenagem máxima, qual a força de frenagem total dos freios dianteiros, em N, no balanço ideal?", N(mu)), Ff, "N",
                    L.F("Força total = m·μ·g = {0} N; frente = {0} × {1} = {2} N.", N(m * mu * G), N(ideal, 3), N(Ff)));
            }
            default:
            {
                double b = Ex.Step(r, 0.55, 0.85, 0.05);
                double aF = mu * G * wf / (b - mu * h / L2), aR = mu * G * (1 - wf) / ((1 - b) + mu * h / L2);
                bool frontFirst = aF < aR;
                double amax = Math.Min(aF, aR);
                string intro = car + " " + L.F("Os freios mandam {0}% da força para a frente e os pneus têm μ = {1}.", N(b * 100, 0), N(mu));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Qual a maior desaceleração, em m/s², antes de algum eixo travar?"), amax, "m/s²",
                        L.F("Frente trava em a = μ·g·wf / (b − μ·h/L) = {0}; traseira trava em a = μ·g·(1 − wf) / ((1 − b) + μ·h/L) = {1}. Vale o menor: {2} m/s².", N(aF), N(aR), N(amax)), 3);
                return Ex.Choice(r, intro + " " + L.T("Pisando cada vez mais forte, qual eixo trava primeiro?"),
                    frontFirst ? L.T("O dianteiro") : L.T("O traseiro"),
                    new[] { frontFirst ? L.T("O traseiro") : L.T("O dianteiro"), L.T("Os dois ao mesmo tempo") },
                    L.F("Limite da frente: a = {0} m/s²; limite de trás: a = {1} m/s². Trava primeiro o eixo com o MENOR limite.", N(aF), N(aR)));
            }
        }
    }
}

[Exercises("car-14-energia-freios")]
public sealed class Car14 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double m = Ex.Step(r, 900, 2200, 50), kmh = Ex.Step(r, 60, 200, 10), v = Ms(kmh), E = 0.5 * m * v * v;
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Qual a energia cinética, em kJ, de um carro de {0} kg a {1} km/h?", N(m), N(kmh)), E / 1000, "kJ",
                        L.F("v = {0} m/s; E = ½·m·v² = ½·{1}·{0}² = {2} kJ.", N(v), N(m), N(E / 1000)));
                return Ex.Num(L.F("Para parar um carro de {0} kg que anda a {1} km/h, quanta energia, em kJ, os freios precisam transformar em calor?", N(m), N(kmh)), E / 1000, "kJ",
                    L.F("E = ½·m·v² = ½·{0}·{1}² = {2} kJ.", N(m), N(v), N(E / 1000)));
            case Difficulty.Medium:
            {
                double front = Ex.Step(r, 0.6, 0.75, 0.05), md = Ex.Step(r, 8, 24, 1), c = 460, dT = front * E / (md * c);
                string intro = L.F("Um carro de {0} kg para de {1} km/h. {2}% da energia vai para os 2 discos dianteiros, de {3} kg de ferro fundido cada um (c = 460 J/kg·K); despreze o resfriamento.", N(m), N(kmh), N(front * 100, 0), N(md));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Quanta energia, em kJ, cada disco dianteiro absorve?"), front * E / 2000, "kJ",
                        L.F("E = {0} kJ; frente: {1}·{0} = {2} kJ; por disco: {3} kJ.", N(E / 1000), N(front), N(front * E / 1000), N(front * E / 2000)));
                return Ex.Num(intro + " " + L.T("De quantos °C sobe a temperatura de cada disco dianteiro?"), dT / 2, "°C",
                    L.F("Por disco: {0}·{1}/2 = {2} J; ΔT = Q / (m·c) = {2} / ({3}·460) = {4} °C.", N(front), N(E), N(front * E / 2), N(md), N(dT / 2)));
            }
            case Difficulty.Hard:
            {
                double t = Ex.Step(r, 3, 8, 0.5), Pavg = E / t, eff = Ex.Pick(r, 0.5, 0.6, 0.65, 0.7), rec = E * eff;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um carro de {0} kg para de {1} km/h em {2} s, com desaceleração constante. Qual a potência média de frenagem, em kW?", N(m), N(kmh), N(t)), Pavg / 1000, "kW",
                        L.F("E = {0} kJ; P = E / t = {0} / {1} = {2} kW.", N(E / 1000), N(t), N(Pavg / 1000)));
                    case 1: return Ex.Num(L.F("Um elétrico de {0} kg freia de {1} km/h até parar, recuperando {2}% da energia cinética na bateria. Quantos Wh voltam para a bateria?", N(m), N(kmh), N(eff * 100, 0)), rec / 3600, "Wh",
                        L.F("E = {0} kJ; recuperado: {1}·{0} = {2} kJ = {3} Wh (1 Wh = 3600 J).", N(E / 1000), N(eff), N(rec / 1000), N(rec / 3600)));
                    default:
                    {
                        double v2 = Ms(kmh * Ex.Step(r, 0.3, 0.6, 0.1)), dE = 0.5 * m * (v * v - v2 * v2);
                        return Ex.Num(L.F("Um carro de {0} kg reduz de {1} km/h para {2} km/h freando. Quanta energia, em kJ, os freios dissipam?", N(m), N(kmh), N(Kmh(v2))), dE / 1000, "kJ",
                            L.F("ΔE = ½·m·(v₁² − v₂²) = ½·{0}·({1}² − {2}²) = {3} kJ.", N(m), N(v), N(v2), N(dE / 1000)));
                    }
                }
            }
            default:
            {
                double hh = Ex.Step(r, 100, 600, 50), eff = Ex.Pick(r, 0.6, 0.65, 0.7), cons = Ex.Step(r, 14, 20, 1);
                if (r.Next(2) == 0)
                {
                    double Eg = m * G * hh * eff, dist = Eg / 3.6e6 / cons * 100;
                    return Ex.Num(L.F("Um elétrico de {0} kg desce uma serra de {1} m de desnível e recupera {2}% da energia potencial com a frenagem regenerativa. Consumindo {3} kWh/100 km, quantos quilômetros em plano essa energia paga? (g = 9,8 m/s²)", N(m), N(hh), N(eff * 100, 0), N(cons)), dist, "km",
                        L.F("E = {0}·{1}·9,8·{2} = {3} kWh; km = {3}/{4}·100 = {5} km.", N(eff), N(m), N(hh), N(Eg / 3.6e6, 3), N(cons), N(dist)), 3);
                }
                int stops = Ex.Int(r, 3, 8);
                double front = 0.7, mdisc = Ex.Step(r, 8, 20, 1), dTstop = front * E / (2 * mdisc * 460), T0 = 40;
                return Ex.Num(L.F("Um carro de {0} kg faz {1} frenagens seguidas de {2} km/h até parar. 70% da energia vai para os 2 discos dianteiros de {3} kg (c = 460 J/kg·K) e, sem tempo de esfriar, o calor se acumula. Partindo de 40 °C, qual a temperatura dos discos dianteiros ao fim?", N(m), N(stops, 0), N(kmh), N(mdisc)), T0 + stops * dTstop, "°C",
                    L.F("Por frenagem: ΔT = 0,7·{0} / (2·{1}·460) = {2} °C; após {3}: 40 + {3}·{2} = {4} °C.", N(E), N(mdisc), N(dTstop), N(stops, 0), N(T0 + stops * dTstop)), 3);
            }
        }
    }
}

[Exercises("car-15-downforce")]
public sealed class Car15 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G, Rho = Pk.Rho;
        double clA = Ex.Step(r, 1.0, 3.5, 0.25), kmh = Ex.Step(r, 60, 250, 10), v = Ms(kmh), down = 0.5 * Rho * clA * v * v;
        double m = Ex.Step(r, 250, 800, 10), mu = Ex.Step(r, 1.1, 1.8, 0.1);
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um carro com asas de Cl·A = {0} m² anda a {1} km/h. Qual o downforce, em N? (ρ = 1,2 kg/m³)", N(clA), N(kmh)), down, "N",
                        L.F("v = {0} m/s; F = ½·ρ·ClA·v² = ½·1,2·{1}·{0}² = {2} N.", N(v), N(clA), N(down)));
                return Ex.Num(L.F("Um carro gera {0} N de downforce a {1} km/h. Qual o seu Cl·A, em m²? (ρ = 1,2 kg/m³)", N(down), N(kmh)), clA, "m²",
                    L.F("ClA = 2·F / (ρ·v²) = 2·{0} / (1,2·{1}²) = {2} m².", N(down), N(v), N(clA)));
            case Difficulty.Medium:
            {
                double w = m * G, grip0 = mu * w, grip1 = mu * (w + down);
                string intro = L.F("Um carro de {0} kg, com pneus μ = {1} e Cl·A = {2} m², anda a {3} km/h (ρ = 1,2 kg/m³, g = 9,8 m/s²).", N(m), N(mu), N(clA), N(kmh));
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(intro + " " + L.T("Qual a carga total sobre os pneus (peso + downforce), em N?"), w + down, "N",
                        L.F("N = m·g + F = {0} + {1} = {2} N.", N(w), N(down), N(w + down)));
                    case 1: return Ex.Num(intro + " " + L.T("Qual a força máxima de aderência total, em N?"), grip1, "N",
                        L.F("F_aderência = μ·(m·g + F_down) = {0}·({1} + {2}) = {3} N.", N(mu), N(w), N(down), N(grip1)));
                    default: return Ex.Num(intro + " " + L.T("De quantos por cento a aderência aumenta por causa do downforce?"), (grip1 / grip0 - 1) * 100, "%",
                        L.F("Sem asa: μ·m·g = {0} N; com asa: {1} N. Aumento = F_down / (m·g) = {2}%.", N(grip0), N(grip1), N((grip1 / grip0 - 1) * 100)));
                }
            }
            case Difficulty.Hard:
            {
                double rad = Ex.Step(r, 30, 120, 5), den = m / rad - mu * Rho * clA / 2;
                if (den <= 0) clA = 0.5 * 1.0;
                clA = Math.Min(clA, 0.9 * 2 * m / (rad * mu * Rho));
                den = m / rad - mu * Rho * clA / 2;
                double vc = Math.Sqrt(mu * m * G / den), vNo = Math.Sqrt(mu * G * rad);
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um carro de {0} kg, com μ = {1} e Cl·A = {2} m², faz uma curva de raio {3} m no limite. Qual a velocidade máxima, em km/h? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", N(m), N(mu), N(clA), N(rad)), Kmh(vc), "km/h",
                        L.F("m·v²/r = μ·(m·g + ½·ρ·ClA·v²) → v² = μ·m·g / (m/r − μ·ρ·ClA/2) = {0}; v = {1} m/s = {2} km/h.", N(vc * vc), N(vc), N(Kmh(vc))), 3);
                    default: return Ex.Num(L.F("Um carro de {0} kg, com μ = {1}, faz uma curva de raio {2} m. De quantos km/h a velocidade máxima da curva aumenta ao colocar asas com Cl·A = {3} m²? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", N(m), N(mu), N(rad), N(clA)), Kmh(vc - vNo), "km/h",
                        L.F("Sem asas: √(μ·g·r) = {0} km/h; com asas: {1} km/h; diferença: {2} km/h.", N(Kmh(vNo)), N(Kmh(vc)), N(Kmh(vc - vNo))), 3);
                }
            }
            default:
            {
                double ld = Ex.Step(r, 3, 8, 0.5), Pextra = down * v / ld;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um conjunto de asas gera {0} N de downforce a {1} km/h com razão sustentação/arrasto (L/D) igual a {2}. Quantos kW esse arrasto extra consome?", N(down), N(kmh), N(ld)), Pextra / 1000, "kW",
                        L.F("Arrasto da asa = F_down / (L/D) = {0}/{1} = {2} N; P = F·v = {2}·{3} = {4} kW.", N(down), N(ld), N(down / ld), N(v), N(Pextra / 1000)));
                double kk = Ex.Pick(r, 1.5, 2.0, 2.5);
                return Ex.Num(L.F("Um carro gera {0} N de downforce a {1} km/h. Qual o downforce, em N, a {2} km/h?", N(down), N(kmh), N(kmh * kk)), down * kk * kk, "N",
                    L.F("F ∝ v²: {0} · {1}² = {2} N.", N(down), N(kk), N(down * kk * kk)));
            }
        }
    }
}
