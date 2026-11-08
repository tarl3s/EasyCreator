namespace EasyCreator.Exercises;

/// <summary>Damped oscillations: natural frequency, critical damping, decay, quality factor.</summary>
[Exercises("fis-16-amortecimento")]
public sealed class Fis16Amortecimento : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double m = Ex.Step(r, 0.5, 5, 0.5), k = Ex.Step(r, 20, 500, 10);
        double w0 = Math.Sqrt(k / m), bc = 2 * Math.Sqrt(k * m);
        string sys = L.F("Um sistema massa-mola tem m = {0} kg e k = {1} N/m", P.N(m), P.N(k, 0));
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(sys + ". " + L.T("Qual a frequência angular natural (sem amortecimento)?"), w0, "rad/s", $"ω₀ = √(k/m) = √({P.N(k, 0)} / {P.N(m)}) = {P.N(w0)} rad/s.");
                return Ex.Num(sys + ". " + L.T("Qual o coeficiente de amortecimento crítico b?"), bc, "kg/s", $"b_crít = 2√(k m) = 2√({P.N(k, 0)}·{P.N(m)}) = {P.N(bc)} kg/s.");
            case Difficulty.Medium:
            {
                if (r.Next(2) == 0)
                {
                    double b = bc * Ex.Pick(r, 0.3, 0.6, 1.5, 2.0);
                    b = Math.Round(b * 10) / 10;
                    string right = b < bc - 1e-9 ? L.T("Subamortecido") : b > bc + 1e-9 ? L.T("Superamortecido") : L.T("Criticamente amortecido");
                    return Ex.Choice(r, sys + L.F(" e amortecimento b = {0} kg/s. Como é o movimento?", P.N(b, 1)),
                        right, new[] { L.T("Subamortecido"), L.T("Superamortecido"), L.T("Criticamente amortecido") },
                        $"b_crít = 2√(k m) = {P.N(bc, 3)} kg/s. b = {P.N(b, 1)}: " + (b < bc - 1e-9 ? "b < b_crít" : b > bc + 1e-9 ? "b > b_crít" : "b = b_crít") + ".");
                }
                double b2 = Ex.Step(r, 0.2, 2, 0.1), t = Ex.Step(r, 1, 10, 1), A0 = Ex.Step(r, 2, 20, 1);
                double A = A0 * Math.Exp(-b2 * t / (2 * m));
                return Ex.Num(sys + L.F(" e amortecimento b = {0} kg/s (fraco). A amplitude inicial é {1} cm. Qual a amplitude depois de {2} s (em cm)?", P.N(b2, 1), P.N(A0, 0), P.N(t, 0)), A, "cm",
                    $"A(t) = A₀ e^(−b t / 2m) = {P.N(A0, 0)}·e^(−{P.N(b2 * t / (2 * m), 3)}) = {P.N(A)} cm.");
            }
            case Difficulty.Hard:
            {
                double b = Math.Round(bc * Ex.Pick(r, 0.1, 0.2, 0.3) * 100) / 100, g = b / (2 * m);
                if (r.Next(2) == 0)
                {
                    double wd = Math.Sqrt(w0 * w0 - g * g);
                    return Ex.Num(sys + L.F(" e b = {0} kg/s. Qual a frequência angular das oscilações amortecidas?", P.N(b, 2)), wd, "rad/s", $"γ = b/2m = {P.N(g, 3)} s⁻¹; ω_d = √(ω₀² − γ²) = √({P.N(w0 * w0, 3)} − {P.N(g * g, 3)}) = {P.N(wd)} rad/s.");
                }
                double th = Math.Log(2) / g;
                return Ex.Num(sys + L.F(" e b = {0} kg/s. Em quanto tempo a amplitude cai à metade?", P.N(b, 2)), th, "s", $"A = A₀ e^(−γ t), γ = b/2m = {P.N(g, 3)} s⁻¹ → t = ln 2 / γ = {P.N(th)} s.");
            }
            default:
            {
                double b = Math.Round(bc * Ex.Pick(r, 0.05, 0.1, 0.2) * 100) / 100;
                if (r.Next(2) == 0)
                {
                    double Q = m * w0 / b;
                    return Ex.Num(sys + L.F(" e b = {0} kg/s. Qual o fator de qualidade Q = m·ω₀ / b?", P.N(b, 2)), Q, "", $"Q = m ω₀ / b = {P.N(m)}·{P.N(w0)} / {P.N(b, 2)} = {P.N(Q)}.");
                }
                // The energy decays as e^(-b t / m): it falls to 1/e after t = m / b.
                double te = m / b;
                return Ex.Num(sys + L.F(" e b = {0} kg/s (fraco). Em quanto tempo a ENERGIA cai a 1/e (≈ 37%) do valor inicial?", P.N(b, 2)), te, "s", $"A energia vai com A², então E = E₀ e^(−b t / m). Cai a 1/e quando b t / m = 1 → t = m / b = {P.N(m)} / {P.N(b, 2)} = {P.N(te)} s.");
            }
        }
    }
}

/// <summary>Charged particles in magnetic fields: radius, velocity selector, mass spectrometer.</summary>
[Exercises("fis-17-espectrometro")]
public sealed class Fis17Espectrometro : IExerciseGenerator
{
    const double E = 1.602e-19, Mp = 1.673e-27, U = 1.661e-27, Me = 9.109e-31;

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                double B = Ex.Step(r, 0.1, 2, 0.1), v = Ex.Step(r, 1, 50, 1) * 1e5;
                if (r.Next(2) == 0)
                {
                    double rad = Mp * v / (E * B);
                    return Ex.Num(L.F("Um próton entra a {0} km/s perpendicularmente num campo magnético de {1} T. Qual o raio da trajetória circular, em cm? (m = 1,673·10⁻²⁷ kg, q = 1,602·10⁻¹⁹ C)", P.N(v / 1e3, 0), P.N(B)), rad * 100, "cm", $"r = m v / (q B) = {P.Sci(Mp)}·{P.N(v, 0)} / ({P.Sci(E)}·{P.N(B)}) = {P.N(rad, 4)} m = {P.N(rad * 100)} cm.");
                }
                double f = E * B / (2 * Math.PI * Mp);
                return Ex.Num(L.F("Qual a frequência de ciclotron (em MHz) de um próton num campo de {0} T? (m = 1,673·10⁻²⁷ kg, q = 1,602·10⁻¹⁹ C)", P.N(B)), f / 1e6, "MHz", $"f = q B / (2π m) = {P.Sci(E)}·{P.N(B)} / (2π·{P.Sci(Mp)}) = {P.N(f / 1e6)} MHz.");
            }
            case Difficulty.Medium:
            {
                double Ef = Ex.Step(r, 1, 50, 1) * 1e3, B = Ex.Step(r, 0.1, 1, 0.1), V = Ex.Step(r, 100, 5000, 100);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Num seletor de velocidades, o campo elétrico é {0} kV/m e o magnético {1} T, perpendiculares. Que velocidade (em km/s) passa sem desviar?", P.N(Ef / 1e3, 0), P.N(B)), Ef / B / 1e3, "km/s", $"q E = q v B → v = E / B = {P.N(Ef, 0)} / {P.N(B)} = {P.N(Ef / B, 0)} m/s = {P.N(Ef / B / 1e3)} km/s.");
                double v = Math.Sqrt(2 * E * V / Mp), rad = Mp * v / (E * B);
                return Ex.Num(L.F("Um próton é acelerado do repouso por {0} V e entra num campo magnético de {1} T, perpendicular ao movimento. Qual o raio da órbita, em cm?", P.N(V, 0), P.N(B)), rad * 100, "cm",
                    $"q V = m v²/2 → v = {P.N(v, 0)} m/s; r = m v / (q B) = {P.N(rad, 4)} m = {P.N(rad * 100)} cm.");
            }
            case Difficulty.Hard:
            {
                // Two isotopes accelerated by V land at different distances: Δx = 2 (r2 − r1).
                var (n1, m1, n2, m2) = Ex.Pick(r, ("¹²C", 12.0, "¹⁴C", 14.0), ("³⁵Cl", 35.0, "³⁷Cl", 37.0), ("²³⁵U", 235.0, "²³⁸U", 238.0), ("²⁰Ne", 20.0, "²²Ne", 22.0));
                double V = Ex.Step(r, 1, 5, 0.5) * 1000, B = Ex.Step(r, 0.2, 1, 0.1);
                double r1 = Math.Sqrt(2 * m1 * U * V / E) / B, r2 = Math.Sqrt(2 * m2 * U * V / E) / B, dx = 2 * (r2 - r1);
                return Ex.Num(L.F("Íons {0} e {1} (carga +e) são acelerados por {2} kV e entram num campo magnético de {3} T. Qual a distância (em mm) entre os pontos onde batem no detector, depois de meia volta?", n1, n2, P.N(V / 1e3, 1), P.N(B)), dx * 1000, "mm",
                    $"r = (1/B)√(2 m V / q): r₁ = {P.N(r1, 4)} m, r₂ = {P.N(r2, 4)} m. Distância = 2 (r₂ − r₁) = {P.N(dx * 1000)} mm.");
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    double V = Ex.Step(r, 1, 5, 0.5) * 1000, B = Ex.Step(r, 0.2, 1, 0.1), mu = Ex.Step(r, 10, 240, 1), rad = Math.Sqrt(2 * mu * U * V / E) / B;
                    return Ex.Num(L.F("Num espectrômetro, um íon de carga +e acelerado por {0} kV descreve uma semicircunferência de raio {1} cm num campo de {2} T. Qual a massa do íon, em u (1 u = 1,661·10⁻²⁷ kg)?", P.N(V / 1e3, 1), P.N(rad * 100), P.N(B)), mu, "u",
                        $"m = q B² r² / (2V) = {P.Sci(E)}·{P.N(B)}²·{P.N(rad, 4)}² / (2·{P.N(V, 0)}) = {P.Sci(mu * U)} kg = {P.N(mu)} u.");
                }
                double Vv = Ex.Step(r, 50, 400, 10), lam = 1.226e-9 / Math.Sqrt(Vv);
                double B2 = Ex.Step(r, 1, 10, 1) * 1e-3, ve = Math.Sqrt(2 * E * Vv / Me), re = Me * ve / (E * B2);
                return Ex.Num(L.F("Um elétron é acelerado do repouso por {0} V e entra num campo magnético de {1} mT. Qual o raio da sua órbita, em mm? (m = 9,109·10⁻³¹ kg)", P.N(Vv, 0), P.N(B2 * 1000, 0)), re * 1000, "mm",
                    $"v = √(2 q V / m) = {P.N(ve, 0)} m/s; r = m v / (q B) = {P.N(re * 1000)} mm.");
            }
        }
    }
}

/// <summary>Electromagnetic induction: Faraday, motional emf, generators, transformers, transmission lines.</summary>
[Exercises("fis-18-inducao")]
public sealed class Fis18Inducao : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                if (r.Next(2) == 0)
                {
                    double Np = Ex.Step(r, 100, 1000, 50), Vp = Ex.Pick(r, 110.0, 127, 220), Ns = Ex.Step(r, 20, 500, 10), Vs = Vp * Ns / Np;
                    return Ex.Num(L.F("Um transformador ideal tem {0} espiras no primário e {1} no secundário. Aplicando {2} V no primário, qual a tensão no secundário?", P.N(Np, 0), P.N(Ns, 0), P.N(Vp, 0)), Vs, "V", $"Vs / Vp = Ns / Np → Vs = {P.N(Vp, 0)}·{P.N(Ns, 0)} / {P.N(Np, 0)} = {P.N(Vs)} V.");
                }
                double N = Ex.Step(r, 10, 200, 10), dPhi = Ex.Step(r, 1, 50, 1) / 1000, dt = Ex.Step(r, 0.1, 2, 0.1), eps = N * dPhi / dt;
                return Ex.Num(L.F("O fluxo magnético por uma bobina de {0} espiras varia {1} mWb em {2} s. Qual a fem média induzida?", P.N(N, 0), P.N(dPhi * 1000, 0), P.N(dt)), eps, "V", $"ε = N ΔΦ / Δt = {P.N(N, 0)}·{P.N(dPhi, 3)} / {P.N(dt)} = {P.N(eps)} V.");
            }
            case Difficulty.Medium:
            {
                if (r.Next(2) == 0)
                {
                    double B = Ex.Step(r, 0.1, 1.5, 0.1), len = Ex.Step(r, 0.1, 1, 0.1), v = Ex.Step(r, 1, 20, 1), eps = B * len * v;
                    return Ex.Num(L.F("Uma barra de {0} m desliza a {1} m/s, perpendicular a um campo magnético de {2} T, sobre trilhos condutores. Qual a fem induzida?", P.N(len), P.N(v, 0), P.N(B)), eps, "V", $"ε = B L v = {P.N(B)}·{P.N(len)}·{P.N(v, 0)} = {P.N(eps)} V.");
                }
                double N = Ex.Step(r, 50, 500, 50), B2 = Ex.Step(r, 0.1, 1, 0.1), A = Ex.Step(r, 0.01, 0.1, 0.01), f = Ex.Pick(r, 50.0, 60), w = 2 * Math.PI * f, em = N * B2 * A * w;
                return Ex.Num(L.F("Um gerador tem uma bobina de {0} espiras e {1} m² girando a {2} Hz num campo de {3} T. Qual a fem máxima?", P.N(N, 0), P.N(A), P.N(f, 0), P.N(B2)), em, "V", $"ε_máx = N B A ω = {P.N(N, 0)}·{P.N(B2)}·{P.N(A)}·(2π·{P.N(f, 0)}) = {P.N(em)} V.");
            }
            case Difficulty.Hard:
            {
                double Np = Ex.Step(r, 500, 2000, 100), Ns = Ex.Step(r, 50, 400, 50), Vp = Ex.Pick(r, 110.0, 220), Is = Ex.Step(r, 1, 10, 1);
                double Vs = Vp * Ns / Np, Ip = Is * Ns / Np;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um transformador ideal reduz {0} V para {1} V ({2} espiras no secundário). O secundário fornece {3} A. Qual a corrente no primário?", P.N(Vp, 0), P.N(Vs), P.N(Ns, 0), P.N(Is, 0)), Ip, "A", $"Potência igual: Vp Ip = Vs Is → Ip = Is·Ns / Np, com Np = {P.N(Np, 0)}: Ip = {P.N(Is, 0)}·{P.N(Ns, 0)} / {P.N(Np, 0)} = {P.N(Ip, 3)} A.");
                double P2 = Vs * Is;
                return Ex.Num(L.F("Um transformador ideal com {0} espiras no primário (ligado a {1} V) e {2} no secundário alimenta uma carga com {3} A. Qual a potência fornecida à carga?", P.N(Np, 0), P.N(Vp, 0), P.N(Ns, 0), P.N(Is, 0)), P2, "W", $"Vs = Vp Ns / Np = {P.N(Vs)} V; P = Vs·Is = {P.N(Vs)}·{P.N(Is, 0)} = {P.N(P2)} W.");
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    // Transmission line: raising the voltage n times cuts the current n times and the loss n² times.
                    double Pw = Ex.Step(r, 10, 100, 10) * 1000, V1 = Ex.Step(r, 1, 5, 1) * 1000, n = Ex.Pick(r, 10.0, 20, 50), Rl = Ex.Step(r, 1, 10, 1);
                    double V2 = V1 * n, I2 = Pw / V2, loss = I2 * I2 * Rl, pct = loss / Pw * 100;
                    return Ex.Num(L.F("Uma usina envia {0} kW por uma linha de {1} Ω. Um transformador eleva a tensão de {2} kV para {3} kV antes da linha. Que porcentagem da potência se perde na linha?", P.N(Pw / 1e3, 0), P.N(Rl, 0), P.N(V1 / 1e3, 0), P.N(V2 / 1e3, 0)), pct, "%",
                        $"I = P / V = {P.N(Pw, 0)} / {P.N(V2, 0)} = {P.N(I2, 3)} A; perda = I² R = {P.N(loss)} W; {P.N(loss)} / {P.N(Pw, 0)} = {P.N(pct, 3)}%.");
                }
                double m = Ex.Step(r, 0.05, 0.5, 0.05), Rr = Ex.Step(r, 0.5, 5, 0.5), B = Ex.Step(r, 0.2, 1.5, 0.1), len = Ex.Step(r, 0.1, 0.5, 0.1);
                double vt = m * P.G * Rr / (B * B * len * len);
                return Ex.Num(L.F("Uma barra de {0} kg e {1} m desliza na vertical, por gravidade, sobre trilhos verticais sem atrito ligados por um resistor de {2} Ω, num campo magnético horizontal de {3} T. Qual a velocidade terminal?", P.N(m), P.N(len), P.N(Rr), P.N(B)), vt, "m/s",
                    $"Na velocidade terminal: m g = B I L com I = B L v / R → v = m g R / (B² L²) = {P.N(m)}·{P.N(P.G)}·{P.N(Rr)} / ({P.N(B)}²·{P.N(len)}²) = {P.N(vt)} m/s.");
            }
        }
    }
}

/// <summary>Special relativity: gamma, time dilation, length contraction, energy, velocity addition.</summary>
[Exercises("fis-19-relatividade")]
public sealed class Fis19Relatividade : IExerciseGenerator
{
    const double C = 3e8, MeV = 0.511;

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double beta = Ex.Step(r, 0.5, 0.98, 0.01);
        double gam = 1 / Math.Sqrt(1 - beta * beta);
        switch (d)
        {
            case Difficulty.Easy:
            {
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma nave viaja a {0}c. Qual o fator de Lorentz γ?", P.N(beta)), gam, "", $"γ = 1 / √(1 − v²/c²) = 1 / √(1 − {P.N(beta)}²) = {P.N(gam, 3)}.");
                double m = Ex.Step(r, 1, 100, 1) / 1000, E = m * C * C;
                return Ex.Num(L.F("Qual a energia equivalente a {0} g de massa, em 10¹³ joules? (c = 3·10⁸ m/s)", P.N(m * 1000, 0)), E / 1e13, "·10¹³ J", $"E = m c² = {P.N(m, 3)}·(3·10⁸)² = {P.N(E / 1e13)}·10¹³ J.");
            }
            case Difficulty.Medium:
            {
                double t0 = Ex.Step(r, 1, 20, 1);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um astronauta mede {0} anos de viagem no relógio da nave, que viaja a {1}c. Quantos anos passam na Terra?", P.N(t0, 0), P.N(beta)), gam * t0, "anos", $"Δt = γ Δt₀ = {P.N(gam, 3)}·{P.N(t0, 0)} = {P.N(gam * t0)} anos.");
                double L0 = Ex.Step(r, 10, 200, 10);
                return Ex.Num(L.F("Uma nave tem {0} m de comprimento em repouso. Qual o comprimento medido por quem a vê passar a {1}c?", P.N(L0, 0), P.N(beta)), L0 / gam, "m", $"L = L₀ / γ = {P.N(L0, 0)} / {P.N(gam, 3)} = {P.N(L0 / gam)} m.");
            }
            case Difficulty.Hard:
            {
                if (r.Next(2) == 0)
                {
                    double ke = (gam - 1) * MeV;
                    return Ex.Num(L.F("Um elétron (mc² = 0,511 MeV) tem velocidade {0}c. Qual a sua energia cinética, em MeV?", P.N(beta)), ke, "MeV", $"Ec = (γ − 1) m c² = ({P.N(gam, 3)} − 1)·{P.N(MeV, 3)} = {P.N(ke, 3)} MeV.");
                }
                double tau = 2.2e-6, dist = beta * C * gam * tau;
                return Ex.Num(L.F("Múons têm vida média de 2,2 μs em repouso. Qual a distância média percorrida (em km), no referencial da Terra, por um múon a {0}c?", P.N(beta)), dist / 1e3, "km", $"τ' = γ τ = {P.N(gam, 3)}·2,2 μs; d = v τ' = {P.N(beta)}·3·10⁸·{P.N(gam * tau * 1e6, 3)}·10⁻⁶ = {P.N(dist / 1e3, 3)} km.");
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    double u = Ex.Step(r, 0.3, 0.95, 0.05), w = Ex.Step(r, 0.3, 0.95, 0.05), vr = (u + w) / (1 + u * w);
                    return Ex.Num(L.F("Uma nave se afasta da Terra a {0}c e dispara um projétil para a frente a {1}c (medido na nave). Qual a velocidade do projétil medida da Terra, em unidades de c?", P.N(u), P.N(w)), vr, "c", $"v = (u + w) / (1 + u w/c²) = ({P.N(u)} + {P.N(w)}) / (1 + {P.N(u)}·{P.N(w)}) = {P.N(vr, 3)}c (e não {P.N(u + w)}c).");
                }
                double ke = Ex.Step(r, 0.5, 5, 0.5), pc = Math.Sqrt(ke * ke + 2 * ke * MeV);
                return Ex.Num(L.F("Um elétron (mc² = 0,511 MeV) tem energia cinética de {0} MeV. Qual o seu momento, em MeV/c?", P.N(ke)), pc, "MeV/c", $"E = Ec + mc² = {P.N(ke + MeV, 3)} MeV; (pc)² = E² − (mc²)² → pc = √({P.N(ke + MeV, 3)}² − {P.N(MeV, 3)}²) = {P.N(pc)} MeV.");
            }
        }
    }
}

/// <summary>Quantum physics: photons, photoelectric effect, de Broglie, Bohr levels, uncertainty.</summary>
[Exercises("fis-20-quantica")]
public sealed class Fis20Quantica : IExerciseGenerator
{
    const double HC = 1239.84, H = 6.626e-34, Me = 9.109e-31, Hbar = 1.0546e-34;

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                double lam = Ex.Step(r, 100, 900, 10);
                if (r.Next(2) == 0)
                {
                    double E = HC / lam;
                    return Ex.Num(L.F("Qual a energia, em eV, de um fóton de comprimento de onda {0} nm? (hc = 1240 eV·nm)", P.N(lam, 0)), E, "eV", $"E = hc / λ = 1240 / {P.N(lam, 0)} = {P.N(E)} eV.");
                }
                double E2 = Ex.Step(r, 1, 10, 0.5), l2 = HC / E2;
                return Ex.Num(L.F("Qual o comprimento de onda, em nm, de um fóton de {0} eV? (hc = 1240 eV·nm)", P.N(E2)), l2, "nm", $"λ = hc / E = 1240 / {P.N(E2)} = {P.N(l2)} nm.");
            }
            case Difficulty.Medium:
            {
                double phi = Ex.Step(r, 2, 5, 0.1), lam = Ex.Step(r, 150, Math.Floor(HC / phi / 10) * 10 - 10, 10);
                double E = HC / lam;
                if (r.Next(2) == 0)
                {
                    double ke = E - phi;
                    return Ex.Num(L.F("Luz de {0} nm incide num metal de função trabalho {1} eV. Qual a energia cinética máxima dos elétrons, em eV? (hc = 1240 eV·nm)", P.N(lam, 0), P.N(phi)), ke, "eV", $"E = 1240 / {P.N(lam, 0)} = {P.N(E, 3)} eV; Ec = E − φ = {P.N(E, 3)} − {P.N(phi)} = {P.N(ke)} eV.", 3);
                }
                double thr = HC / phi;
                return Ex.Num(L.F("Um metal tem função trabalho de {0} eV. Qual o maior comprimento de onda, em nm, que arranca elétrons dele? (hc = 1240 eV·nm)", P.N(phi)), thr, "nm", $"λ_limiar = hc / φ = 1240 / {P.N(phi)} = {P.N(thr)} nm.");
            }
            case Difficulty.Hard:
            {
                int n1 = r.Next(2, 8), n2 = r.Next(1, n1);
                double E = 13.6 * (1.0 / (n2 * n2) - 1.0 / (n1 * n1)), lam = HC / E;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um átomo de hidrogênio passa do nível n = {0} para n = {1}. Qual o comprimento de onda do fóton emitido, em nm? (Eₙ = −13,6/n² eV, hc = 1240 eV·nm)", n1, n2), lam, "nm", $"ΔE = 13,6·(1/{n2}² − 1/{n1}²) = {P.N(E, 3)} eV; λ = hc / ΔE = 1240 / {P.N(E, 3)} = {P.N(lam)} nm.");
                double phi = Ex.Pick(r, 2.28, 2.9, 4.08), lam2 = Ex.Step(r, 150, 250, 10), Vs = HC / lam2 - phi;
                return Ex.Num(L.F("Luz de {0} nm ilumina um metal de função trabalho {1} eV. Qual o potencial de corte (que para os fotoelétrons), em V? (hc = 1240 eV·nm)", P.N(lam2, 0), P.N(phi)), Vs, "V", $"e·V = hc/λ − φ = {P.N(HC / lam2, 3)} − {P.N(phi)} → V = {P.N(Vs)} V.", 3);
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    double V = Ex.Step(r, 50, 500, 10), lam = 1.226 / Math.Sqrt(V);
                    return Ex.Num(L.F("Qual o comprimento de onda de de Broglie, em nm, de um elétron acelerado do repouso por {0} V? (use λ ≈ 1,226 nm / √V, com V em volts)", P.N(V, 0)), lam, "nm", $"λ = h / p = 1,226 / √V = 1,226 / √{P.N(V, 0)} = {P.N(lam, 4)} nm.");
                }
                double dx = Ex.Step(r, 0.5, 5, 0.5) * 1e-10, dv = Hbar / (2 * dx * Me);
                return Ex.Num(L.F("Um elétron está confinado num átomo de {0} Å (1 Å = 10⁻¹⁰ m). Pelo princípio da incerteza (Δx·Δp ≥ ħ/2), qual a incerteza mínima na velocidade, em 10⁵ m/s?", P.N(dx * 1e10)), dv / 1e5, "·10⁵ m/s", $"Δp = ħ / (2Δx) = 1,0546·10⁻³⁴ / (2·{P.N(dx * 1e10)}·10⁻¹⁰) ; Δv = Δp / m = {P.N(dv / 1e5)}·10⁵ m/s.");
            }
        }
    }
}
