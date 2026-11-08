namespace EasyCreator.Exercises;

/// <summary>Rockets 16-20: gravity and drag losses, re-entry, orbital mechanics, competition flights, the light orbital launcher.</summary>
[Exercises("rkt-16-perdas-gravidade")]
public sealed class Rkt16 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double tb = Ex.Step(r, 10, 150, 5), isp = Ex.Step(r, 220, 360, 10), ve = isp * G;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double loss = G * tb;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um foguete sobe na vertical com a queima durando {0} s. Qual a perda por gravidade (g·t)? (g = 9,8 m/s²)", Ex.N(tb)), loss, "m/s", L.F("Perda = g·t = 9,8·{0} = {1} m/s.", Ex.N(tb), Ex.N(loss)));
                return Ex.Num(L.F("Num voo vertical a gravidade 'rouba' {0} m/s do Δv durante a queima. Quanto dura a queima? (g = 9,8 m/s²)", Ex.N(loss)), tb, "s", L.F("t = perda / g = {0} / 9,8 = {1} s.", Ex.N(loss), Ex.N(tb)));
            }
            case Difficulty.Medium:
            {
                double R = Ex.Step(r, 2, 5, 0.25), dv = ve * Math.Log(R), vbo = dv - G * tb;
                return Ex.Num(L.F("Um foguete com Isp = {0} s e razão de massa {1} queima por {2} s na vertical. Qual a velocidade no fim da queima (Δv ideal − g·t)? (g₀ = g = 9,8 m/s²)", Ex.N(isp), Ex.N(R), Ex.N(tb)),
                    vbo, "m/s", L.F("vₑ = {0}·9,8 = {1} m/s; Δv = {1}·ln({2}) = {3} m/s; perda = {4} m/s; v = {5} m/s.", Ex.N(isp), Ex.N(ve), Ex.N(R), Ex.N(dv), Ex.N(G * tb), Ex.N(vbo)));
            }
            case Difficulty.Hard:
            {
                double F = Ex.Step(r, 20, 400, 10) * 1000, mp = F / (ve) * tb, md = F / ve;
                mp = Math.Round(mp / 10) * 10; double t2 = mp / md;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um motor de {0} kN com Isp = {1} s queima {2} kg de propelente. Quanto dura a queima? (g₀ = 9,8 m/s²)", Ex.N(F / 1000), Ex.N(isp), Ex.N(mp)), t2, "s", L.F("ṁ = F / (Isp·g₀) = {0} kg/s; t = m / ṁ = {1} s.", Ex.N(md, 3), Ex.N(t2)));
                return Ex.Num(L.F("Um motor de {0} kN com Isp = {1} s queima {2} kg de propelente na vertical. Qual a perda por gravidade durante a queima? (g₀ = g = 9,8 m/s²)", Ex.N(F / 1000), Ex.N(isp), Ex.N(mp)), G * t2, "m/s", L.F("ṁ = {0} kg/s; t = {1} s; perda = g·t = {2} m/s.", Ex.N(md, 3), Ex.N(t2), Ex.N(G * t2)));
            }
            default:
            {
                double vorb = Ex.Step(r, 7600, 7900, 50), dl = Ex.Step(r, 100, 300, 25), tg = Ex.Step(r, 120, 180, 10), req = vorb + 0.5 * G * tg + dl, R = Math.Exp(req / ve);
                return Ex.Num(L.F("Para chegar a uma órbita de {0} m/s, um lançador gasta em média metade da perda vertical (½·g·t) com a queima de {1} s e ainda perde {2} m/s por arrasto. Com Isp = {3} s, que razão de massa é necessária? (g₀ = g = 9,8 m/s²)", Ex.N(vorb), Ex.N(tg), Ex.N(dl), Ex.N(isp)),
                    R, "", L.F("Perda por gravidade = 0,5·9,8·{0} = {1} m/s; Δv necessário = {2} + {1} + {3} = {4} m/s; vₑ = {5} m/s; R = e^(Δv/vₑ) = {6}.", Ex.N(tg), Ex.N(0.5 * G * tg), Ex.N(vorb), Ex.N(dl), Ex.N(req), Ex.N(ve), Ex.N(R, 3)));
            }
        }
    }
}

[Exercises("rkt-17-reentrada")]
public sealed class Rkt17 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double v = Ex.Step(r, 3000, 11000, 200), M = Ex.Step(r, 100, 3000, 100), cd = Ex.Step(r, 0.8, 1.6, 0.1), dia = Ex.Step(r, 0.8, 3, 0.1), A = Pk.Area(dia), beta = M / (cd * A);
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma cápsula volta do espaço a {0} m/s. Qual a energia cinética por quilograma?", Ex.N(v)), v * v / 2 / 1e6, "MJ/kg", L.F("e = v²/2 = {0}²/2 = {1} MJ/kg.", Ex.N(v), Ex.N(v * v / 2 / 1e6, 3)));
                return Ex.Num(L.F("Uma cápsula de {0} kg entra na atmosfera a {1} m/s. Qual a energia cinética total que a atmosfera precisa dissipar?", Ex.N(M), Ex.N(v)), 0.5 * M * v * v / 1e6, "MJ", L.F("E = ½·m·v² = 0,5·{0}·{1}² = {2} MJ.", Ex.N(M), Ex.N(v), Ex.N(0.5 * M * v * v / 1e6)));
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma cápsula de {0} kg tem {1} m de diâmetro e Cd = {2}. Qual o coeficiente balístico β = m/(Cd·A)?", Ex.N(M), Ex.N(dia), Ex.N(cd)), beta, "kg/m²", L.F("A = π·d²/4 = {0} m²; β = {1} / ({2}·{0}) = {3} kg/m².", Ex.N(A, 4), Ex.N(M), Ex.N(cd), Ex.N(beta)));
                return Ex.Num(L.F("Uma cápsula tem β = {0} kg/m². Dois designs têm a mesma massa; o 2º tem o dobro da área frontal. Qual o β do 2º?", Ex.N(beta)), beta / 2, "kg/m²", L.F("β = m/(Cd·A): dobrar A divide β por 2 → {0} kg/m².", Ex.N(beta / 2)));
            case Difficulty.Hard:
            {
                double vt = Math.Sqrt(2 * beta * G / 1.225);
                return Ex.Num(L.F("Uma cápsula de {0} kg, {1} m de diâmetro e Cd = {2} cai no nível do mar (ρ = 1,225 kg/m³). Qual a velocidade terminal (arrasto = peso)? (g = 9,8 m/s²)", Ex.N(M), Ex.N(dia), Ex.N(cd)),
                    vt, "m/s", L.F("β = {0} kg/m²; v = √(2·β·g/ρ) = √(2·{0}·9,8/1,225) = {1} m/s.", Ex.N(beta), Ex.N(vt)));
            }
            default:
            {
                double ve = Ex.Step(r, 7200, 11200, 200), gam = Ex.Step(r, 3, 20, 1), H = 7200, a = ve * ve * Math.Sin(gam * Math.PI / 180) / (2 * Math.E * H);
                return Ex.Num(L.F("Uma cápsula entra na atmosfera a {0} m/s com ângulo de {1}° abaixo da horizontal. A desaceleração máxima numa entrada balística é a = v²·sen γ / (2·e·H), com H = 7200 m (escala da atmosfera) e e = 2,718. Qual essa desaceleração máxima, em g? (g = 9,8 m/s²) Note que ela não depende da massa nem do formato.", Ex.N(ve), Ex.N(gam)),
                    a / G, "g", L.F("a = {0}² · sen({1}°) / (2·2,718·7200) = {2} m/s² = {3} g.", Ex.N(ve), Ex.N(gam), Ex.N(a), Ex.N(a / G)));
            }
        }
    }
}

[Exercises("rkt-18-mecanica-orbital")]
public sealed class Rkt18 : IExerciseGenerator
{
    static readonly (string Name, string En, string Es, double Mu, double R, double Day)[] Bodies =
    {
        ("a Terra", "Earth", "la Tierra", 3.986e14, 6371e3, 86164),
        ("Marte", "Mars", "Marte", 4.283e13, 3390e3, 88642),
        ("a Lua", "the Moon", "la Luna", 4.905e12, 1737e3, 2360600),
    };

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double Mu = Pk.Mu, Re = Pk.Re;
        switch (d)
        {
            case Difficulty.Easy:
            {
                var b = Bodies[r.Next(3)];
                double h = Ex.Step(r, 0, 1000, 50), rr = b.R + h * 1000, ve = Math.Sqrt(2 * b.Mu / rr), vc = Math.Sqrt(b.Mu / rr);
                string name = Bodies[0].Mu == b.Mu ? L.T("Terra") : Bodies[1].Mu == b.Mu ? L.T("Marte") : L.T("Lua");
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Qual a velocidade de escape a {3} km de altitude sobre o astro {0} (μ = {1}·10¹² m³/s², raio {2} km)?", name, Ex.N(b.Mu / 1e12, 3), Ex.N(b.R / 1000), Ex.N(h)), ve / 1000, "km/s", L.F("r = {0} + {1} = {2} km; vₑₛ = √(2μ/r) = {3} km/s.", Ex.N(b.R / 1000), Ex.N(h), Ex.N(rr / 1000), Ex.N(ve / 1000, 3)));
                return Ex.Num(L.F("Qual a velocidade de uma órbita circular a {3} km de altitude em torno do astro {0} (μ = {1}·10¹² m³/s², raio {2} km)?", name, Ex.N(b.Mu / 1e12, 3), Ex.N(b.R / 1000), Ex.N(h)), vc / 1000, "km/s", L.F("r = {0} + {1} = {2} km; v = √(μ/r) = {3} km/s (a de escape é √2 vezes maior).", Ex.N(b.R / 1000), Ex.N(h), Ex.N(rr / 1000), Ex.N(vc / 1000, 3)));
            }
            case Difficulty.Medium:
            {
                double a1 = Ex.Step(r, 7000, 9000, 100), a2 = Ex.Step(r, 10000, 30000, 500), t1 = 2 * Math.PI * Math.Sqrt(Math.Pow(a1 * 1000, 3) / Mu), t2 = t1 * Math.Pow(a2 / a1, 1.5);
                return Ex.Num(L.F("Um satélite em órbita circular de raio {0} km tem período de {1} min. Pela 3ª lei de Kepler (T² ∝ a³), qual o período de outro satélite na mesma região em órbita de raio {2} km?", Ex.N(a1), Ex.N(t1 / 60, 2), Ex.N(a2)),
                    t2 / 60, "min", L.F("T₂ = T₁·(a₂/a₁)^1,5 = {0}·({1}/{2})^1,5 = {3} min.", Ex.N(t1 / 60, 2), Ex.N(a2), Ex.N(a1), Ex.N(t2 / 60)));
            }
            case Difficulty.Hard:
            {
                bool earth = r.Next(2) == 0;
                var b = earth ? Bodies[0] : Bodies[1];
                double hrs = Ex.Step(r, 2, 30, 0.5), per = hrs * 3600, rs = Math.Pow(b.Mu * per * per / (4 * Math.PI * Math.PI), 1.0 / 3), alt = (rs - b.R) / 1000;
                return Ex.Num(L.F("Um satélite deve dar uma volta completa em torno do astro {0} a cada {1} h (μ = {2}·10¹² m³/s², raio {3} km). Em que altitude circular ele deve ficar?",
                    earth ? L.T("Terra") : L.T("Marte"), Ex.N(hrs), Ex.N(b.Mu / 1e12, 3), Ex.N(b.R / 1000)),
                    alt, "km", L.F("T = 2π·√(r³/μ) → r = (μ·T²/4π²)^(1/3) = {0} km; altitude = r − R = {1} km. (Se T for o dia do astro, é a órbita estacionária.)", Ex.N(rs / 1000), Ex.N(alt)), 1.5);
            }
            default:
            {
                double hp = Ex.Step(r, 200, 800, 50), ha = Ex.Step(r, 5000, 35000, 1000), rp = Re + hp * 1000, ra = Re + ha * 1000, a = (rp + ra) / 2;
                double vp = Math.Sqrt(Mu * (2 / rp - 1 / a)), va = Math.Sqrt(Mu * (2 / ra - 1 / a));
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um satélite terrestre tem perigeu a {0} km e apogeu a {1} km de altitude. Qual sua velocidade no apogeu? (vis-viva: v = √(μ·(2/r − 1/a)); μ = 3,986·10¹⁴ m³/s², raio da Terra 6371 km)", Ex.N(hp), Ex.N(ha)),
                        va / 1000, "km/s", L.F("a = (r_p + r_a)/2 = {0} km; v_a = √(μ·(2/r_a − 1/a)) = {1} km/s.", Ex.N(a / 1000), Ex.N(va / 1000, 3)));
                return Ex.Num(L.F("Um satélite terrestre tem perigeu a {0} km e apogeu a {1} km de altitude. Qual a razão entre a velocidade no perigeu e no apogeu? (use a conservação do momento angular: v_p·r_p = v_a·r_a; raio da Terra 6371 km)", Ex.N(hp), Ex.N(ha)),
                    ra / rp, "", L.F("v_p / v_a = r_a / r_p = {0} / {1} = {2}.", Ex.N(ra / 1000), Ex.N(rp / 1000), Ex.N(ra / rp, 3)));
            }
        }
    }
}

[Exercises("rkt-19-competicao")]
public sealed class Rkt19 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double hb = Ex.Step(r, 20, 120, 5), vb = Ex.Step(r, 40, 120, 5), tb = Ex.Step(r, 0.5, 3, 0.1);
        switch (d)
        {
            case Difficulty.Easy:
            {
                double up = vb * vb / (2 * G);
                return Ex.Num(L.F("Num torneio, o foguete acaba a queima a {0} m de altura subindo a {1} m/s. Sem arrasto (g = 9,8 m/s²), qual o apogeu?", Ex.N(hb), Ex.N(vb)), hb + up, "m", L.F("Subida livre = v²/(2g) = {0} m; apogeu = {1} + {0} = {2} m.", Ex.N(up), Ex.N(hb), Ex.N(hb + up)));
            }
            case Difficulty.Medium:
            {
                double up = vb * vb / (2 * G), ap = hb + up, vd = Ex.Step(r, 3, 8, 0.5), t = tb + vb / G + ap / vd;
                return Ex.Num(L.F("Um foguete queima por {0} s, termina a {1} m subindo a {2} m/s, sobe até o apogeu sem arrasto e depois desce com paraquedas a {3} m/s constantes (ignore o tempo de abertura). Qual o tempo total de voo? (g = 9,8 m/s²)", Ex.N(tb), Ex.N(hb), Ex.N(vb), Ex.N(vd)),
                    t, "s", L.F("Subida inercial: v/g = {0} s; apogeu = {1} m; descida = {1}/{2} = {3} s; total = {4} + {0} + {3} = {5} s.", Ex.N(vb / G), Ex.N(ap), Ex.N(vd), Ex.N(ap / vd), Ex.N(tb), Ex.N(t)));
            }
            case Difficulty.Hard:
            {
                double M = Ex.Step(r, 0.1, 0.6, 0.05), dia = Ex.Step(r, 3, 8, 0.5), Cd = Ex.Step(r, 0.4, 0.7, 0.05), k = 0.5 * Pk.Rho * Cd * Pk.Area(dia / 100);
                double up = M / (2 * k) * Math.Log(1 + k * vb * vb / (M * G));
                return Ex.Num(L.F("Um foguete de {0} kg, {1} cm de diâmetro e Cd = {2} acaba a queima a {3} m de altura subindo a {4} m/s. Qual o apogeu, contando o arrasto quadrático? (ρ = 1,2 kg/m³, g = 9,8 m/s²; subida = M/(2k)·ln(1 + k·v²/(M·g)), k = ½ρCdA)", Ex.N(M), Ex.N(dia), Ex.N(Cd), Ex.N(hb), Ex.N(vb)),
                    hb + up, "m", L.F("k = {0} kg/m; subida = {1} m (sem arrasto: {2} m); apogeu = {3} + {1} = {4} m.", Ex.N(k, 5), Ex.N(up), Ex.N(vb * vb / (2 * G)), Ex.N(hb), Ex.N(hb + up)));
            }
            default:
            {
                double M = Ex.Step(r, 0.2, 1, 0.05), Cd = 0.75, up = vb * vb / (2 * G), ap = hb + up, tup = tb + vb / G, td = Ex.Step(r, 25, 70, 5) - 0, T = tup + td;
                // The contest asks for an exact total time T: the parachute descent speed follows from the descent time.
                double vd = ap / td, A = 2 * M * G / (Pk.Rho * Cd * vd * vd), dia = Math.Sqrt(4 * A / Math.PI);
                return Ex.Num(L.F("Numa prova, o voo deve durar exatamente {0} s. O foguete de {1} kg queima por {2} s, termina a {3} m subindo a {4} m/s e sobe sem arrasto até o apogeu. Que diâmetro de paraquedas (Cd = 0,75, velocidade constante) faz o tempo total ser exatamente esse? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", Ex.N(T), Ex.N(M), Ex.N(tb), Ex.N(hb), Ex.N(vb)),
                    dia, "m", L.F("Subida: {0} + {1} = {2} s; apogeu = {3} m; tempo de descida = {4} − {2} = {5} s → v = {3}/{5} = {6} m/s; A = 2Mg/(ρ·Cd·v²) = {7} m²; d = √(4A/π) = {8} m.", Ex.N(tb), Ex.N(vb / G), Ex.N(tup), Ex.N(ap), Ex.N(T), Ex.N(td), Ex.N(vd), Ex.N(A, 4), Ex.N(dia, 3)), 3);
            }
        }
    }
}

[Exercises("rkt-20-lancador-orbital")]
public sealed class Rkt20 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G, Mu = Pk.Mu, Re = Pk.Re;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double h = Ex.Step(r, 300, 800, 10), v = Math.Sqrt(Mu / (Re + h * 1000));
                return Ex.Num(L.F("Qual a velocidade orbital de um satélite leve numa órbita circular a {0} km de altitude? (μ = 3,986·10¹⁴ m³/s², raio da Terra 6371 km)", Ex.N(h)), v, "m/s", L.F("v = √(μ/r) = √(3,986·10¹⁴ / {0}) = {1} m/s.", Ex.N(Re + h * 1000), Ex.N(v)));
            }
            case Difficulty.Medium:
            {
                double h = Ex.Step(r, 300, 700, 50), v = Math.Sqrt(Mu / (Re + h * 1000)), lat = Ex.Pick(r, 0.0, 5.2, 28.5, 45.0, 52.0), bonus = 465.1 * Math.Cos(lat * Math.PI / 180), loss = Ex.Step(r, 1200, 1800, 100);
                double budget = v + loss - bonus;
                return Ex.Num(L.F("Um lançador vai para uma órbita circular a {0} km de altitude, partindo de latitude {1}° para o leste. Perdas por gravidade e arrasto: {2} m/s. A rotação da Terra dá um bônus de 465·cos(latitude) m/s. Qual o Δv que o lançador precisa fornecer? (μ = 3,986·10¹⁴ m³/s², raio da Terra 6371 km)", Ex.N(h), Ex.N(lat), Ex.N(loss)),
                    budget, "m/s", L.F("v_orbital = {0} m/s; bônus = 465·cos({1}°) = {2} m/s; Δv = {0} + {3} − {2} = {4} m/s.", Ex.N(v), Ex.N(lat), Ex.N(bonus), Ex.N(loss), Ex.N(budget)));
            }
            case Difficulty.Hard:
            {
                double isp = Ex.Step(r, 280, 350, 10), ve = isp * G, dv = Ex.Step(r, 9000, 9600, 100), mf = Ex.Step(r, 100, 800, 50), R = Math.Exp(dv / ve);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um lançador de um só estágio precisa de Δv = {0} m/s com Isp = {1} s. Qual a razão de massa necessária? (g₀ = 9,8 m/s²)", Ex.N(dv), Ex.N(isp)), R, "", L.F("vₑ = {0}·9,8 = {1} m/s; R = e^(Δv/vₑ) = e^({2}/{1}) = {3}.", Ex.N(isp), Ex.N(ve), Ex.N(dv), Ex.N(R, 3)));
                return Ex.Num(L.F("Um lançador de um só estágio (Isp = {0} s) precisa de Δv = {1} m/s e chega à órbita com {2} kg (estrutura + carga útil). Que massa de propelente ele carrega, ignorando perdas de descarte? (g₀ = 9,8 m/s²)", Ex.N(isp), Ex.N(dv), Ex.N(mf)), mf * (R - 1), "kg", L.F("R = e^({0}/{1}) = {2}; propelente = m_f·(R − 1) = {3}·{4} = {5} kg.", Ex.N(dv), Ex.N(ve), Ex.N(R, 3), Ex.N(mf), Ex.N(R - 1, 3), Ex.N(mf * (R - 1))));
            }
            default:
            {
                double p = Ex.Step(r, 20, 100, 10), s2 = Ex.Step(r, 60, 150, 10), mp2 = Ex.Step(r, 600, 1200, 50), s1 = Ex.Step(r, 300, 700, 50), mp1 = Ex.Step(r, 3000, 6000, 250);
                double ve1 = Ex.Step(r, 2800, 3100, 50), ve2 = Ex.Step(r, 3200, 3500, 50);
                double m0 = p + s2 + mp2 + s1 + mp1, m1 = m0 - mp1, m2 = p + s2 + mp2, mf = p + s2;
                double d1 = ve1 * Math.Log(m0 / m1), d2 = ve2 * Math.Log(m2 / mf), tot = d1 + d2;
                return Ex.Num(L.F("Um lançador leve de dois estágios tem: carga útil {0} kg; 2º estágio com estrutura {1} kg, propelente {2} kg e vₑ = {3} m/s; 1º estágio com estrutura {4} kg, propelente {5} kg e vₑ = {6} m/s. A estrutura do 1º é descartada antes do 2º acender. Qual o Δv total ideal?", Ex.N(p), Ex.N(s2), Ex.N(mp2), Ex.N(ve2), Ex.N(s1), Ex.N(mp1), Ex.N(ve1)),
                    tot, "m/s", L.F("Estágio 1: {0} → {1} kg: Δv₁ = {2}·ln({0}/{1}) = {3} m/s. Estágio 2: {4} → {5} kg: Δv₂ = {6}·ln({4}/{5}) = {7} m/s. Total = {8} m/s.", Ex.N(m0), Ex.N(m1), Ex.N(ve1), Ex.N(d1), Ex.N(m2), Ex.N(mf), Ex.N(ve2), Ex.N(d2), Ex.N(tot)));
            }
        }
    }
}
