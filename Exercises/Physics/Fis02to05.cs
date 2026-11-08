namespace EasyCreator.Exercises;

/// <summary>Newton's laws: F = ma, the incline with and without friction, the Atwood-incline system.</summary>
[Exercises("fis-02-newton")]
public sealed class Fis02Newton : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = P.G;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double m = Ex.Step(r, 2, 80, 1), a = Ex.Step(r, 1, 12, 0.5), F = m * a;
                return r.Next(3) switch
                {
                    0 => Ex.Num(L.F("Uma força resultante de {0} N age sobre um corpo de {1} kg. Qual a aceleração?", P.N(F), P.N(m)), a, "m/s²", $"a = F / m = {P.N(F)} / {P.N(m)} = {P.N(a)} m/s²."),
                    1 => Ex.Num(L.F("Um corpo de {0} kg acelera a {1} m/s². Qual a força resultante sobre ele?", P.N(m), P.N(a)), F, "N", $"F = m·a = {P.N(m)}·{P.N(a)} = {P.N(F)} N."),
                    _ => Ex.Num(L.F("Uma força resultante de {0} N produz aceleração de {1} m/s². Qual a massa do corpo?", P.N(F), P.N(a)), m, "kg", $"m = F / a = {P.N(F)} / {P.N(a)} = {P.N(m)} kg."),
                };
            }
            case Difficulty.Medium:
            {
                double th = Ex.Pick(r, 15.0, 20, 25, 30, 35, 40, 45, 50), len = Ex.Step(r, 2, 12, 1);
                double a = G * Math.Sin(P.Rad(th)), t = Math.Sqrt(2 * len / a), v = a * t;
                string intro = L.F("Um bloco é solto do repouso no topo de uma rampa sem atrito, inclinada {0}° e com {1} m de comprimento.", P.N(th), P.N(len));
                return r.Next(3) switch
                {
                    0 => Ex.Num(intro + " " + L.T("Qual a aceleração do bloco?"), a, "m/s²", L.F("a = g·sen θ = {0}·sen {1}° = {2} m/s².", P.N(G), P.N(th), P.N(a))),
                    1 => Ex.Num(intro + " " + L.T("Quanto tempo ele leva para chegar embaixo?"), t, "s", L.F("a = g·sen θ = {0} m/s²; L = a t²/2 → t = √(2L/a) = √(2·{1}/{2}) = {3} s.", P.N(a), P.N(len), P.N(a), P.N(t))),
                    _ => Ex.Num(intro + " " + L.T("Com que velocidade ele chega embaixo?"), v, "m/s", $"a = {P.N(a)} m/s²; v = √(2aL) = √(2·{P.N(a)}·{P.N(len)}) = {P.N(v)} m/s."),
                };
            }
            case Difficulty.Hard:
            {
                double th = Ex.Pick(r, 20.0, 25, 30, 35, 40), mu = Ex.Step(r, 0.1, 0.35, 0.05), m = Ex.Step(r, 2, 30, 1);
                double s = Math.Sin(P.Rad(th)), c = Math.Cos(P.Rad(th));
                double a = G * (s - mu * c), F = m * G * (s + mu * c);
                string ramp = L.F("Um bloco de {0} kg está numa rampa de {1}° com coeficiente de atrito cinético {2}.", P.N(m), P.N(th), P.N(mu));
                if (r.Next(2) == 0)
                    return Ex.Num(ramp + " " + L.T("Ele desce escorregando. Qual a aceleração?"), a, "m/s²",
                        L.F("a = g (sen θ − μ cos θ) = {0}·({1} − {2}·{3}) = {4} m/s².", P.N(G), P.N(s, 3), P.N(mu), P.N(c, 3), P.N(a)));
                return Ex.Num(ramp + " " + L.T("Que força paralela à rampa, para cima, faz o bloco subir com velocidade constante?"), F, "N",
                    L.F("F = m g (sen θ + μ cos θ) = {0}·{1}·({2} + {3}·{4}) = {5} N.", P.N(m), P.N(G), P.N(s, 3), P.N(mu), P.N(c, 3), P.N(F)));
            }
            default:
            {
                double th = Ex.Pick(r, 20.0, 30, 40), m1 = Ex.Step(r, 2, 8, 1), m2 = Math.Round(m1 * Math.Sin(P.Rad(th)) + Ex.Step(r, 1, 6, 1));
                double a = (m2 - m1 * Math.Sin(P.Rad(th))) * G / (m1 + m2), T = m2 * (G - a);
                string intro = L.F("Um bloco de {0} kg sobe uma rampa sem atrito de {1}° puxado por um fio, que passa por uma polia ideal e segura um bloco pendurado de {2} kg.", P.N(m1), P.N(th), P.N(m2));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Qual a aceleração do sistema?"), a, "m/s²",
                        L.F("a = (m₂ − m₁ sen θ)·g / (m₁ + m₂) = ({0} − {1}·sen {2}°)·{3} / {4} = {5} m/s².", P.N(m2), P.N(m1), P.N(th), P.N(G), P.N(m1 + m2), P.N(a)));
                return Ex.Num(intro + " " + L.T("Qual a tensão no fio?"), T, "N",
                    $"a = {P.N(a)} m/s². m₂ g − T = m₂ a → T = m₂ (g − a) = {P.N(m2)}·({P.N(G)} − {P.N(a)}) = {P.N(T)} N.");
            }
        }
    }
}

/// <summary>Energy: kinetic, potential, springs, friction, the loop.</summary>
[Exercises("fis-03-energia")]
public sealed class Fis03Energia : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = P.G;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double m = Ex.Step(r, 1, 20, 1), v = Ex.Step(r, 2, 30, 1), h = Ex.Step(r, 2, 50, 1);
                return r.Next(3) switch
                {
                    0 => Ex.Num(L.F("Qual a energia cinética de um corpo de {0} kg a {1} m/s?", P.N(m), P.N(v)), m * v * v / 2, "J", $"Ec = m v² / 2 = {P.N(m)}·{P.N(v)}² / 2 = {P.N(m * v * v / 2)} J."),
                    1 => Ex.Num(L.F("Qual a energia potencial gravitacional de um corpo de {0} kg a {1} m de altura?", P.N(m), P.N(h)), m * G * h, "J", $"Ep = m g h = {P.N(m)}·{P.N(G)}·{P.N(h)} = {P.N(m * G * h)} J."),
                    _ => Ex.Num(L.F("Um objeto é solto, sem resistência do ar, de {0} m de altura. Com que velocidade chega ao chão?", P.N(h)), Math.Sqrt(2 * G * h), "m/s", $"m g h = m v² / 2 → v = √(2 g h) = √(2·{P.N(G)}·{P.N(h)}) = {P.N(Math.Sqrt(2 * G * h))} m/s."),
                };
            }
            case Difficulty.Medium:
            {
                double k = Ex.Step(r, 100, 2000, 50), x = Ex.Step(r, 0.05, 0.4, 0.01), m = Ex.Step(r, 0.1, 3, 0.1);
                double e = k * x * x / 2, v = x * Math.Sqrt(k / m);
                string spring = L.F("Uma mola de constante {0} N/m é comprimida {1} m.", P.N(k), P.N(x));
                return r.Next(3) switch
                {
                    0 => Ex.Num(spring + " " + L.T("Qual a energia armazenada?"), e, "J", $"Ee = k x² / 2 = {P.N(k)}·{P.N(x)}² / 2 = {P.N(e)} J."),
                    1 => Ex.Num(spring + " " + L.F("Ela lança um bloco de {0} kg numa superfície sem atrito. Qual a velocidade do bloco ao sair da mola?", P.N(m)), v, "m/s",
                        $"k x² / 2 = m v² / 2 → v = x √(k/m) = {P.N(x)}·√({P.N(k)}/{P.N(m)}) = {P.N(v)} m/s."),
                    _ => Ex.Num(L.F("Uma mola armazena {0} J quando comprimida {1} m. Qual a sua constante elástica?", P.N(e), P.N(x)), k, "N/m", $"k = 2E / x² = 2·{P.N(e)} / {P.N(x)}² = {P.N(k)} N/m."),
                };
            }
            case Difficulty.Hard:
            {
                double k = Ex.Step(r, 200, 1500, 50), x = Ex.Step(r, 0.1, 0.4, 0.01), m = Ex.Step(r, 0.5, 3, 0.1);
                double e = k * x * x / 2;
                if (r.Next(2) == 0)
                {
                    double h = e / (m * G);
                    return Ex.Num(L.F("Uma mola de {0} N/m, comprimida {1} m, lança um bloco de {2} kg para cima de uma rampa sem atrito. Que altura vertical o bloco alcança?", P.N(k), P.N(x), P.N(m)),
                        h, "m", $"k x² / 2 = m g h → h = {P.N(e)} / ({P.N(m)}·{P.N(G)}) = {P.N(h)} m.");
                }
                double mu = Ex.Step(r, 0.1, 0.5, 0.05), dist = e / (mu * m * G);
                return Ex.Num(L.F("Uma mola de {0} N/m, comprimida {1} m, lança um bloco de {2} kg numa superfície horizontal com atrito cinético {3}. Que distância o bloco percorre até parar?", P.N(k), P.N(x), P.N(m), P.N(mu)),
                    dist, "m", $"{P.N(e)} = μ m g d → d = {P.N(e)} / ({P.N(mu)}·{P.N(m)}·{P.N(G)}) = {P.N(dist)} m.");
            }
            default:
            {
                // Loop-the-loop: at the top the weight alone supplies the centripetal force (v² = gR).
                double R = Ex.Step(r, 0.2, 2, 0.1);
                if (r.Next(2) == 0)
                {
                    double h = 2.5 * R;
                    return Ex.Num(L.F("Um carrinho sem atrito é solto do repouso de uma pista e entra num looping circular de raio {0} m. De que altura mínima, medida a partir da base do looping, ele deve ser solto para não cair no topo?", P.N(R)),
                        h, "m", L.F("v² = g R (topo); m g h = m g (2R) + m v²/2 → h = {0} R = {1}·{2} = {3} m.", P.N(2.5), P.N(2.5), P.N(R), P.N(h)));
                }
                double k = Ex.Step(r, 500, 3000, 100), m = Ex.Step(r, 0.1, 1, 0.1);
                double x = Math.Sqrt(5 * m * G * R / k);
                return Ex.Num(L.F("Uma mola de {0} N/m lança uma bolinha de {1} kg que entra, sem atrito, num looping de raio {2} m (o início da pista está na altura da base do looping). Qual a compressão mínima da mola para a bolinha completar a volta?", P.N(k), P.N(m), P.N(R)),
                    x, "m", L.F("v² = g R (topo); k x² / 2 = m g (2R) + m g R / 2 → x = √(5 m g R / k) = √(5·{0}·{1}·{2} / {3}) = {4} m.", P.N(m), P.N(G), P.N(R), P.N(k), P.N(x, 3)));
            }
        }
    }
}

/// <summary>Momentum and collisions.</summary>
[Exercises("fis-04-colisoes")]
public sealed class Fis04Colisoes : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                double m = Ex.Step(r, 1, 80, 1), v = Ex.Step(r, 1, 30, 1);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Qual a quantidade de movimento de um corpo de {0} kg a {1} m/s?", P.N(m), P.N(v)), m * v, "kg·m/s", $"p = m v = {P.N(m)}·{P.N(v)} = {P.N(m * v)} kg·m/s.");
                double m2 = Ex.Step(r, 1, 20, 1), vf = m * v / (m + m2);
                return Ex.Num(L.F("Um vagão de {0} kg a {1} m/s bate num vagão parado de {2} kg e os dois ficam engatados. Qual a velocidade final?", P.N(m), P.N(v), P.N(m2)),
                    vf, "m/s", $"m₁ v₁ = (m₁ + m₂) v → v = {P.N(m)}·{P.N(v)} / {P.N(m + m2)} = {P.N(vf)} m/s.");
            }
            case Difficulty.Medium:
            {
                double m1 = Ex.Step(r, 1, 10, 1), m2 = Ex.Step(r, 1, 10, 1), v1 = Ex.Step(r, 2, 15, 1), v2 = Ex.Step(r, 1, 10, 1);
                if (r.Next(2) == 0)
                {
                    double vf = (m1 * v1 - m2 * v2) / (m1 + m2);
                    return Ex.Num(L.F("Dois carrinhos vão um contra o outro: {0} kg a {1} m/s e {2} kg a {3} m/s. Depois do choque ficam grudados. Qual a velocidade final (positiva no sentido do primeiro carrinho, negativa no sentido contrário)?", P.N(m1), P.N(v1), P.N(m2), P.N(v2)),
                        vf, "m/s", $"m₁ v₁ − m₂ v₂ = (m₁ + m₂) v → v = ({P.N(m1)}·{P.N(v1)} − {P.N(m2)}·{P.N(v2)}) / {P.N(m1 + m2)} = {P.N(vf)} m/s.");
                }
                double v1p = (m1 - m2) / (m1 + m2) * v1, v2p = 2 * m1 / (m1 + m2) * v1;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma bola de {0} kg a {1} m/s bate frontalmente, num choque perfeitamente elástico, numa bola parada de {2} kg. Qual a velocidade da bola que estava parada depois do choque?", P.N(m1), P.N(v1), P.N(m2)),
                        v2p, "m/s", $"v₂' = 2 m₁ v₁ / (m₁ + m₂) = 2·{P.N(m1)}·{P.N(v1)} / {P.N(m1 + m2)} = {P.N(v2p)} m/s.");
                return Ex.Num(L.F("Uma bola de {0} kg a {1} m/s bate frontalmente, num choque perfeitamente elástico, numa bola parada de {2} kg. Qual a velocidade da primeira bola depois do choque (negativa se ela voltar)?", P.N(m1), P.N(v1), P.N(m2)),
                    v1p, "m/s", $"v₁' = (m₁ − m₂) v₁ / (m₁ + m₂) = ({P.N(m1)} − {P.N(m2)})·{P.N(v1)} / {P.N(m1 + m2)} = {P.N(v1p)} m/s.");
            }
            case Difficulty.Hard:
            {
                if (r.Next(2) == 0)
                {
                    double M = Ex.Step(r, 500, 3000, 100), m = Ex.Step(r, 5, 50, 5), v = Ex.Step(r, 100, 600, 20);
                    double vr = m * v / M;
                    return Ex.Num(L.F("Um canhão de {0} kg, solto sobre rodas, dispara um projétil de {1} kg a {2} m/s na horizontal. Qual a velocidade de recuo do canhão?", P.N(M), P.N(m), P.N(v)),
                        vr, "m/s", $"M V = m v → V = {P.N(m)}·{P.N(v)} / {P.N(M)} = {P.N(vr, 3)} m/s.");
                }
                double m1 = Ex.Step(r, 1, 8, 1), m2 = Ex.Step(r, 1, 8, 1), v1 = Ex.Step(r, 4, 15, 1);
                double vf = m1 * v1 / (m1 + m2), loss = m1 * v1 * v1 / 2 - (m1 + m2) * vf * vf / 2;
                return Ex.Num(L.F("Um bloco de {0} kg a {1} m/s bate num bloco parado de {2} kg e os dois seguem juntos. Quanta energia cinética é perdida no choque?", P.N(m1), P.N(v1), P.N(m2)),
                    loss, "J", L.F("v = {0}·{1} / {2} = {3} m/s; Ec antes = {4} J; depois = {5} J; perda = {6} J.", P.N(m1), P.N(v1), P.N(m1 + m2), P.N(vf), P.N(m1 * v1 * v1 / 2), P.N((m1 + m2) * vf * vf / 2), P.N(loss)));
            }
            default:
            {
                // Ballistic pendulum: the collision conserves momentum, the swing conserves energy.
                double m = Ex.Step(r, 0.005, 0.03, 0.005), M = Ex.Step(r, 1, 5, 0.5), h = Ex.Step(r, 0.05, 0.3, 0.01);
                double vp = Math.Sqrt(2 * P.G * h), v = (m + M) / m * vp;
                return Ex.Num(L.F("Uma bala de {0} kg entra num bloco de madeira de {1} kg pendurado por fios e fica cravada nele. O conjunto sobe {2} m. Qual a velocidade da bala antes do impacto?", P.N(m, 3), P.N(M), P.N(h)),
                    v, "m/s", $"v' = √(2 g h) = {P.N(vp, 3)} m/s; m v = (m + M) v' → v = {P.N(m + M, 3)}·{P.N(vp, 3)} / {P.N(m, 3)} = {P.N(v)} m/s.");
            }
        }
    }
}

/// <summary>Circular motion, gravitation, orbits (Earth's constants).</summary>
[Exercises("fis-05-orbitas")]
public sealed class Fis05Orbitas : IExerciseGenerator
{
    const double GM = 3.986e14, Re = 6.371e6;

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                double v = Ex.Step(r, 2, 30, 1), rad = Ex.Step(r, 1, 50, 1), a = v * v / rad, T = 2 * Math.PI * rad / v;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um carro faz uma curva circular de {0} m de raio a {1} m/s. Qual a aceleração centrípeta?", P.N(rad), P.N(v)), a, "m/s²", $"a = v² / r = {P.N(v)}² / {P.N(rad)} = {P.N(a)} m/s².");
                return Ex.Num(L.F("Um carrinho dá voltas numa pista circular de {0} m de raio a {1} m/s. Quanto tempo leva cada volta?", P.N(rad), P.N(v)), T, "s", $"T = 2π r / v = 2π·{P.N(rad)} / {P.N(v)} = {P.N(T)} s.");
            }
            case Difficulty.Medium:
            {
                double alt = Ex.Step(r, 200, 2000, 50) * 1e3, rr = Re + alt, v = Math.Sqrt(GM / rr), T = 2 * Math.PI * rr / v;
                string sat = L.F("Um satélite está em órbita circular a {0} km de altitude (GM da Terra = 3,986·10¹⁴ m³/s², raio da Terra = 6371 km).", P.N(alt / 1e3, 0));
                if (r.Next(2) == 0)
                    return Ex.Num(sat + " " + L.T("Qual a sua velocidade orbital?"), v / 1e3, "km/s", $"r = {P.N(rr / 1e3, 0)} km; v = √(GM/r) = {P.N(v)} m/s = {P.N(v / 1e3)} km/s.");
                return Ex.Num(sat + " " + L.T("Qual o seu período orbital?"), T / 60, "min", $"v = √(GM/r) = {P.N(v)} m/s; T = 2π r / v = {P.N(T)} s = {P.N(T / 60)} min.");
            }
            case Difficulty.Hard:
            {
                if (r.Next(2) == 0)
                {
                    double T = Ex.Step(r, 2, 24, 0.25) * 3600, rr = Math.Cbrt(GM * T * T / (4 * Math.PI * Math.PI)), alt = (rr - Re) / 1e3;
                    return Ex.Num(L.F("A que altitude (acima da superfície) um satélite circular da Terra tem período de {0} horas? (GM = 3,986·10¹⁴ m³/s², raio da Terra = 6371 km)", P.N(T / 3600, 2)), alt, "km",
                        L.F("T² = 4π² r³ / GM → r = ∛(GM T² / 4π²) = {0} km; altitude = r − R = {1} km.", P.N(rr / 1e3, 0), P.N(alt, 0)));
                }
                var (name, gm, rad) = Ex.Pick(r, (L.T("da Lua"), 4.905e12, 1.7374e6), (L.T("de Marte"), 4.283e13, 3.3895e6), (L.T("da Terra"), GM, Re), (L.T("de Mercúrio"), 2.203e13, 2.4397e6), (L.T("de Vênus"), 3.249e14, 6.0518e6), (L.T("de Júpiter"), 1.2669e17, 6.9911e7));
                double ve = Math.Sqrt(2 * gm / rad);
                return Ex.Num(L.F("Qual a velocidade de escape da superfície {0}? (GM = {1} m³/s², raio = {2} km)", name, P.Sci(gm), P.N(rad / 1e3, 1)),
                    ve / 1e3, "km/s", $"v = √(2GM/R) = √(2·{P.Sci(gm)} / {P.Sci(rad)}) = {P.N(ve)} m/s = {P.N(ve / 1e3)} km/s.");
            }
            default:
            {
                double T1 = Ex.Step(r, 90, 120, 5), k = Ex.Pick(r, 2.0, 3, 4, 5, 6);
                if (r.Next(2) == 0)
                {
                    double T2 = T1 * Math.Pow(k, 1.5);
                    return Ex.Num(L.F("Um satélite circular tem período de {0} min. Outro satélite orbita o mesmo planeta com raio {1} vezes maior. Qual o período do segundo, em minutos?", P.N(T1, 0), P.N(k, 0)),
                        T2, "min", $"T² ∝ r³ → T₂ = T₁·k^(3/2) = {P.N(T1, 0)}·{P.N(k, 0)}^{P.N(1.5, 1)} = {P.N(T2)} min.");
                }
                double v1 = Ex.Step(r, 5, 8, 0.1), v2 = v1 / Math.Sqrt(k);
                return Ex.Num(L.F("Um satélite em órbita circular tem velocidade de {0} km/s. Se ele fosse levado para uma órbita de raio {1} vezes maior, qual seria sua velocidade orbital (em km/s)?", P.N(v1, 1), P.N(k, 0)),
                    v2, "km/s", $"v = √(GM/r) ∝ 1/√r → v₂ = v₁ / √k = {P.N(v1, 1)} / √{P.N(k, 0)} = {P.N(v2)} km/s.");
            }
        }
    }
}
