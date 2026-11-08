namespace EasyCreator.Exercises;

/// <summary>Submarines 1-5: density and Archimedes, neutral buoyancy, ballast, pressure, hull design.</summary>
[Exercises("sub-01-arquimedes")]
public sealed class Sub01 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double m = Ex.Step(r, 2, 80, 1), V = Ex.Step(r, 1, 40, 0.5), rho = m / (V / 1000), E = Pk.RhoW * G * V / 1000;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um bloco de {0} kg ocupa {1} litros. Qual a densidade dele? (1 m³ = 1000 L)", Ex.N(m), Ex.N(V)), rho, "kg/m³", L.F("ρ = m / V = {0} / {1} m³ = {2} kg/m³.", Ex.N(m), Ex.N(V / 1000, 4), Ex.N(rho)));
                    case 1: return Ex.Num(L.F("Um ROV de {0} litros de volume está totalmente submerso em água doce (1000 kg/m³). Qual o empuxo sobre ele? (g = 9,8 m/s²)", Ex.N(V)), E, "N", L.F("E = ρ·g·V = 1000·9,8·{0} m³ = {1} N.", Ex.N(V / 1000, 4), Ex.N(E)));
                    default:
                        bool floats = rho < 1000;
                        return Ex.Choice(r, L.F("Um objeto de {0} kg ocupa {1} litros. Colocado em água doce (1000 kg/m³), o que acontece?", Ex.N(m), Ex.N(V)),
                            floats ? L.T("Flutua, pois é menos denso que a água.") : L.T("Afunda, pois é mais denso que a água."),
                            new[] { floats ? L.T("Afunda, pois é mais denso que a água.") : L.T("Flutua, pois é menos denso que a água."), L.T("Fica parado no meio da água, sempre.") },
                            L.F("Densidade = {0} / {1} m³ = {2} kg/m³, que é {3} que 1000 kg/m³.", Ex.N(m), Ex.N(V / 1000, 4), Ex.N(rho), floats ? L.T("menor") : L.T("maior")));
                }
            }
            case Difficulty.Medium:
            {
                double m = Ex.Step(r, 5, 100, 1), V = Ex.Step(r, 2, 60, 1), rf = Ex.Pick(r, 1000.0, 1025.0);
                double app = (m - rf * V / 1000) * G;
                string fl = rf == 1000 ? L.T("água doce (1000 kg/m³)") : L.T("água do mar (1025 kg/m³)");
                if (r.Next(2) == 0 && app > 0)
                    return Ex.Num(L.F("Um peso de {0} kg e {1} litros é pendurado por um cabo dentro d'água: {2}. Qual a tração no cabo? (g = 9,8 m/s²)", Ex.N(m), Ex.N(V), fl), app, "N", L.F("Peso = {0} N; empuxo = ρ·g·V = {1} N; tração = peso − empuxo = {2} N.", Ex.N(m * G), Ex.N(rf * G * V / 1000), Ex.N(app)));
                double rho = Ex.Pick(r, 500.0, 600, 700, 800, 900, 920);
                double fr = rho / rf * 100;
                return Ex.Num(L.F("Um bloco de madeira com densidade de {0} kg/m³ flutua em {1}. Que porcentagem do volume fica submersa?", Ex.N(rho), fl), fr, "%", L.F("Equilíbrio: ρ_objeto·V = ρ_fluido·V_submerso → fração = {0}/{1} = {2}%.", Ex.N(rho), Ex.N(rf), Ex.N(fr)));
            }
            case Difficulty.Hard:
            {
                double Ln = Ex.Step(r, 0.5, 3, 0.1), W = Ex.Step(r, 0.3, 1.5, 0.1), H = Ex.Step(r, 0.2, 0.8, 0.05), rf = Ex.Pick(r, 1000.0, 1025.0);
                double V = Ln * W * H, A = Ln * W, mb = Math.Round(rf * V * Ex.Step(r, 0.1, 0.6, 0.05)), cap = rf * V - mb, draft = mb / (rf * A);
                string intro = L.F("Uma balsa-caixa de {0} m × {1} m × {2} m (comprimento × largura × altura) pesa {3} kg vazia e flutua em água de {4} kg/m³.", Ex.N(Ln), Ex.N(W), Ex.N(H), Ex.N(mb), Ex.N(rf));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Qual o calado (profundidade submersa) em centímetros, vazia?"), draft * 100, "cm", L.F("calado = m / (ρ·A) = {0} / ({1}·{2}) = {3} m.", Ex.N(mb), Ex.N(rf), Ex.N(A, 3), Ex.N(draft, 4)));
                return Ex.Num(intro + " " + L.T("Que carga máxima, em kg, ela suporta antes de afundar (totalmente submersa)?"), cap, "kg", L.F("Empuxo máximo = ρ·V = {0}·{1} = {2} kg de massa deslocada; carga = {2} − {3} = {4} kg.", Ex.N(rf), Ex.N(V, 4), Ex.N(rf * V), Ex.N(mb), Ex.N(cap)));
            }
            default:
            {
                double rho = Ex.Step(r, 2500, 9000, 100), V = Ex.Step(r, 0.2, 2, 0.1) / 1000, wa = rho * V * G, ww = (rho - Pk.RhoW) * V * G;
                return Ex.Num(L.F("Uma peça maciça pesa {0} N no ar e {1} N dentro d'água doce (1000 kg/m³). Qual a densidade do material?", Ex.N(wa), Ex.N(ww)),
                    rho, "kg/m³", L.F("Empuxo = {0} − {1} = {2} N = ρ_água·g·V → V = {3} m³. ρ = P/(g·V) = {4} kg/m³. (Atalho: ρ = ρ_água·P_ar / (P_ar − P_água).)", Ex.N(wa), Ex.N(ww), Ex.N(wa - ww), Ex.N(V, 6), Ex.N(rho)));
            }
        }
    }
}

[Exercises("sub-02-flutuabilidade-neutra")]
public sealed class Sub02 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double V = Ex.Step(r, 5, 200, 1), rf = Ex.Pick(r, 1000.0, 1025.0);
        string fl = rf == 1000 ? L.T("água doce (1000 kg/m³)") : L.T("água do mar (1025 kg/m³)");
        double mn = rf * V / 1000;
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um ROV de {0} litros de volume vai trabalhar em {1}. Qual deve ser sua massa para flutuar neutro?", Ex.N(V), fl), mn, "kg", L.F("Neutro: massa = ρ·V = {0}·{1} m³ = {2} kg.", Ex.N(rf), Ex.N(V / 1000, 4), Ex.N(mn)));
                return Ex.Num(L.F("Um ROV tem massa de {0} kg e vai operar em {1}. Qual deve ser seu volume, em litros, para flutuar neutro?", Ex.N(mn), fl), V, "L", L.F("Neutro: V = m / ρ = {0} / {1} = {2} m³ = {3} L.", Ex.N(mn), Ex.N(rf), Ex.N(mn / rf, 5), Ex.N(V)));
            case Difficulty.Medium:
            {
                double m = Math.Round(mn * Ex.Step(r, 0.85, 1.15, 0.01), 1), dm = mn - m;
                if (Math.Abs(dm) < 0.3) { m = Math.Round(mn * 0.9, 1); dm = mn - m; }
                bool heavy = dm < 0;
                switch (r.Next(2))
                {
                    case 0:
                        return Ex.Num(L.F("Um ROV com {0} litros e {1} kg opera em {2}. Quantos kg de {3} precisam ser colocados para ficar neutro?", Ex.N(V), Ex.N(m), fl, heavy ? L.T("retirados") : L.T("adicionados de lastro")), Math.Abs(dm), "kg", L.F("Massa neutra = ρ·V = {0} kg; diferença = |{0} − {1}| = {2} kg.", Ex.N(mn), Ex.N(m), Ex.N(Math.Abs(dm))));
                    default:
                        double Fn = dm * G;
                        return Ex.Num(L.F("Um ROV de {0} kg e {1} litros está em {2}. Qual a força líquida vertical, em N, positiva para cima? (g = 9,8 m/s²)", Ex.N(m), Ex.N(V), fl), Fn, "N", L.F("Empuxo = ρ·g·V = {0} N; peso = {1} N; líquida = {2} N.", Ex.N(mn * G), Ex.N(m * G), Ex.N(Fn)));
                }
            }
            case Difficulty.Hard:
            {
                double dm = (1025 - 1000) * V / 1000;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um ROV de {0} litros foi balanceado em água doce (1000 kg/m³) para ficar neutro. Levado ao mar (1025 kg/m³), quantos kg de lastro precisa ganhar para voltar a ficar neutro?", Ex.N(V)), dm, "kg", L.F("Massa neutra no mar = 1025·{0} = {1} kg; na água doce = {2} kg; diferença = {3} kg.", Ex.N(V / 1000, 4), Ex.N(1025 * V / 1000), Ex.N(V), Ex.N(dm)));
                return Ex.Num(L.F("Um ROV de {0} litros foi balanceado neutro no mar (1025 kg/m³). Levado a um lago de água doce (1000 kg/m³), qual a força líquida para baixo, em N? (g = 9,8 m/s²)", Ex.N(V)), dm * G, "N", L.F("Perde empuxo = (1025 − 1000)·{0} m³·9,8 = {1} N.", Ex.N(V / 1000, 4), Ex.N(dm * G)));
            }
            default:
            {
                // Hull compression with depth: the volume shrinks, losing buoyancy.
                double c = Ex.Step(r, 0.05, 0.5, 0.05), depth = Ex.Step(r, 100, 1000, 50), pbar = depth / 10 + 1, lost = mn * c / 100 * (depth / 100) ;
                lost = rf * V / 1000 * (c / 100) * (depth / 100);
                return Ex.Num(L.F("Um ROV de {0} litros foi balanceado neutro na superfície em {1}. O volume do casco diminui {2}% a cada 100 m de profundidade (compressão). Na profundidade de {3} m, qual a força de empuxo perdida, em N? (g = 9,8 m/s²; ignore a mudança da densidade da água)", Ex.N(V), fl, Ex.N(c), Ex.N(depth)),
                    lost * G, "N", L.F("Empuxo inicial = {0} kg·g; redução do volume = {1}% · {2} = {3}%; perda = {4}·{3}% · 9,8 = {5} N, e o ROV passa a afundar.", Ex.N(mn), Ex.N(c), Ex.N(depth / 100), Ex.N(c * depth / 100, 3), Ex.N(mn), Ex.N(lost * G)));
            }
        }
    }
}

[Exercises("sub-03-lastro")]
public sealed class Sub03 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double V = Ex.Step(r, 20, 400, 10), rf = Pk.RhoSea;
        double mdry = Math.Round(rf * V / 1000 * Ex.Step(r, 0.7, 0.92, 0.01), 1);
        double dm = rf * V / 1000 - mdry, vb = dm / rf * 1000;
        string intro = L.F("Um submarino desloca {0} m³ de água do mar (1025 kg/m³) e, com os tanques de lastro vazios, tem {1} t.", Ex.N(V), Ex.N(mdry, 1));
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Qual a massa de água, em toneladas, que os tanques devem receber para ele ficar com flutuabilidade neutra submerso?"), dm, "t", L.F("Neutro: massa total = ρ·V = 1,025·{0} = {1} t; água = {1} − {2} = {3} t.", Ex.N(V), Ex.N(rf * V / 1000), Ex.N(mdry, 1), Ex.N(dm)));
                return Ex.Num(intro + " " + L.T("Qual o volume dos tanques de lastro (em m³) que precisa ser enchido para ficar neutro submerso?"), vb, "m³", L.F("Água necessária = {0} t = {1} m³ (1025 kg/m³).", Ex.N(dm), Ex.N(vb)));
            case Difficulty.Medium:
            {
                double qs = Ex.Step(r, 20, 200, 10), t = vb * 1000 / qs;
                return Ex.Num(intro + " " + L.F("Uma bomba enche os tanques a {0} L/s. Quanto tempo, em segundos, leva para ficar neutro submerso?", Ex.N(qs)), t, "s", L.F("Volume = {0} m³ = {1} L; t = {1} / {2} = {3} s.", Ex.N(vb), Ex.N(vb * 1000), Ex.N(qs), Ex.N(t)));
            }
            case Difficulty.Hard:
            {
                double res = Ex.Step(r, 10, 30, 1) / 100, Vh = V * (1 + res);
                double mfull = Math.Round(rf * V * Ex.Step(r, 0.97, 1, 0.005) / 1000, 1);
                // On the surface the whole hull volume (incl. tanks) is V*(1+res)... define reserve buoyancy = (displaceable volume above the waterline) / watertight volume.
                double sub = mfull * 1000 / rf, reserve = (Vh - sub) / Vh * 100;
                return Ex.Num(L.F("Um submarino com tanques cheios pesa {0} t; seu volume estanque total é {1} m³ (água do mar, 1025 kg/m³). Ao emergir, boiando, qual a flutuabilidade de reserva, isto é, a porcentagem do volume estanque que fica acima da água?", Ex.N(mfull, 1), Ex.N(Vh)),
                    reserve, "%", L.F("Volume submerso = massa / ρ = {0} / 1,025 = {1} m³; acima da água = {2} − {1} = {3} m³; reserva = {3}/{2} = {4}%.", Ex.N(mfull, 1), Ex.N(sub), Ex.N(Vh), Ex.N(Vh - sub), Ex.N(reserve)));
            }
            default:
            {
                double depth = Ex.Step(r, 50, 300, 10), pbar = (Pk.P0 + rf * G * depth) / 1e5, tank = Ex.Step(r, 150, 250, 10), vbl = vb * 1000, air = vbl * pbar / (tank + 1.013);
                // vbl litres of water must be blown out against the pressure at depth, using a tank at tank bar (gauge); isothermal.
                return Ex.Num(L.F("Para emergir a {0} m, o submarino precisa soprar {1} L de água dos tanques usando ar de um cilindro a {2} bar manométricos (1,013 bar = pressão atmosférica; água do mar 1025 kg/m³; g = 9,8 m/s²). Supondo que o ar expandido fique à pressão ambiente e temperatura constante, que volume do cilindro, em litros, é necessário?", Ex.N(depth), Ex.N(vbl), Ex.N(tank)),
                    air, "L", L.F("Pressão ambiente = 1,013 + 1,025·9,8·{0}/100 = {1} bar abs.; ar na pressão ambiente = {2} L; cilindro a {3} bar abs.: P₁V₁ = P₂V₂ → V = {2}·{1}/{3} = {4} L.", Ex.N(depth), Ex.N(pbar, 3), Ex.N(vbl), Ex.N(tank + 1.013, 3), Ex.N(air)));
            }
        }
    }
}

[Exercises("sub-04-pressao")]
public sealed class Sub04 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double depth = Ex.Step(r, 20, 2000, 5), rf = Ex.Pick(r, 1000.0, 1025.0);
        string fl = rf == 1000 ? L.T("água doce (1000 kg/m³)") : L.T("água do mar (1025 kg/m³)");
        double pg = rf * G * depth, pa = pg + Pk.P0;
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Qual a pressão manométrica (acima da atmosfera) a {0} m de profundidade em {1}? (g = 9,8 m/s²)", Ex.N(depth), fl), pg / 1000, "kPa", L.F("p = ρ·g·h = {0}·9,8·{1} = {2} kPa.", Ex.N(rf), Ex.N(depth), Ex.N(pg / 1000)));
                return Ex.Num(L.F("Qual a pressão absoluta a {0} m de profundidade em {1}? (atmosfera = 101,3 kPa; g = 9,8 m/s²)", Ex.N(depth), fl), pa / 1000, "kPa", L.F("p = p₀ + ρ·g·h = 101,3 + {0} = {1} kPa.", Ex.N(pg / 1000), Ex.N(pa / 1000)));
            case Difficulty.Medium:
            {
                double dia = Ex.Step(r, 20, 80, 5), A = Pk.Area(dia / 100), F = pg * A;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um sensor marca {0} bar de pressão absoluta em {1}. Qual a profundidade? (atmosfera = 1,013 bar; g = 9,8 m/s²)", Ex.N(pa / 1e5, 4), fl), depth, "m", L.F("h = (p − p₀) / (ρ·g) = ({0} − 1,013)·10⁵ / ({1}·9,8) = {2} m.", Ex.N(pa / 1e5, 4), Ex.N(rf), Ex.N(depth)));
                    default: return Ex.Num(L.F("Uma escotilha circular de {0} cm de diâmetro fica a {1} m de profundidade em {2}, com pressão atmosférica dentro. Qual a força líquida que a água exerce sobre ela? (g = 9,8 m/s²)", Ex.N(dia), Ex.N(depth), fl), F / 1000, "kN", L.F("Pressão manométrica = {0} kPa; A = π·d²/4 = {1} m²; F = p·A = {2} kN.", Ex.N(pg / 1000), Ex.N(A, 4), Ex.N(F / 1000)));
                }
            }
            case Difficulty.Hard:
            {
                double atm = Ex.Step(r, 10, 100, 5), dep = (atm * Pk.P0 - Pk.P0) / (Pk.RhoSea * G), dia = Ex.Step(r, 10, 40, 2), A = Pk.Area(dia / 100), pgs = Pk.RhoSea * G * depth, Fw = pgs * A;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Em que profundidade no mar (1025 kg/m³) a pressão absoluta chega a {0} atm? (1 atm = 101325 Pa; g = 9,8 m/s²)", Ex.N(atm)), dep, "m", L.F("({0} − 1)·101325 = ρ·g·h → h = {1} Pa / (1025·9,8) = {2} m.", Ex.N(atm), Ex.N(atm * Pk.P0 - Pk.P0), Ex.N(dep)));
                return Ex.Num(L.F("Uma janela circular de {0} cm de diâmetro de um submersível está a {1} m no mar (1025 kg/m³), com 1 atm dentro. Qual a força líquida sobre ela, em kN? (g = 9,8 m/s²)", Ex.N(dia), Ex.N(depth)), Fw / 1000, "kN", L.F("p manométrica = 1025·9,8·{0} = {1} kPa; A = {2} m²; F = {3} kN (cerca de {4} kg-força).", Ex.N(depth), Ex.N(pgs / 1000), Ex.N(A, 4), Ex.N(Fw / 1000), Ex.N(Fw / 9.8, 0)));
            }
            default:
            {
                double v0 = Ex.Step(r, 100, 1000, 50), p2 = (Pk.P0 + Pk.RhoSea * G * depth), frac = Pk.P0 / p2 * 100;
                return Ex.Num(L.F("Um sino de mergulho fechado em baixo guarda ar a 1 atm quando está na superfície. Descendo a {0} m no mar (1025 kg/m³), a água entra e comprime o ar (temperatura constante, lei de Boyle). Que porcentagem do volume inicial de ar sobra? (1 atm = 101325 Pa; g = 9,8 m/s²)", Ex.N(depth)),
                    frac, "%", L.F("p = 101325 + 1025·9,8·{0} = {1} Pa; V₂/V₁ = p₁/p₂ = 101325 / {1} = {2}%.", Ex.N(depth), Ex.N(p2, 0), Ex.N(frac, 3)));
            }
        }
    }
}

[Exercises("sub-05-casco-materiais")]
public sealed class Sub05 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double depth = Ex.Step(r, 100, 3000, 50), p = Pk.RhoSea * G * depth, rad = Ex.Step(r, 0.1, 1, 0.05), t = Ex.Step(r, 3, 40, 1) / 1000;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double sig = p * rad / t;
                return Ex.Num(L.F("Um casco cilíndrico de raio {0} m e parede de {1} mm opera sob pressão externa de {2} MPa. Qual a tensão circunferencial σ = p·r/t na parede?", Ex.N(rad), Ex.N(t * 1000), Ex.N(p / 1e6, 3)), sig / 1e6, "MPa", L.F("σ = p·r/t = {0}·{1} / {2} = {3} MPa.", Ex.N(p / 1e6, 3), Ex.N(rad), Ex.N(t, 4), Ex.N(sig / 1e6)));
            }
            case Difficulty.Medium:
            {
                double sy = Ex.Pick(r, 250.0, 350, 500, 800, 1000), sf = Ex.Pick(r, 1.5, 2.0, 2.5, 3.0), tr = p * rad * sf / (sy * 1e6);
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Qual a espessura mínima da parede (mm) de um cilindro de raio {0} m para operar a {1} m no mar (1025 kg/m³), com material de escoamento {2} MPa e fator de segurança {3}? (t = p·r·FS/σ; g = 9,8 m/s²)", Ex.N(rad), Ex.N(depth), Ex.N(sy), Ex.N(sf)), tr * 1000, "mm", L.F("p = {0} MPa; t = p·r·FS/σ = {0}·{1}·{2} / {3} = {4} mm.", Ex.N(p / 1e6, 3), Ex.N(rad), Ex.N(sf), Ex.N(sy), Ex.N(tr * 1000)));
                    default:
                        double rho = Ex.Pick(r, 2700.0, 4500, 7850), mass = 2 * Math.PI * rad * 1.0 * t * rho; // 1 m length
                        return Ex.Num(L.F("Um trecho de 1 m de casco cilíndrico, raio {0} m e parede {1} mm, é de um material com densidade {2} kg/m³. Qual a massa desse trecho, aproximando a casca por área × espessura? (A = 2π·r·L)", Ex.N(rad), Ex.N(t * 1000), Ex.N(rho)), mass, "kg", L.F("A = 2π·{0}·1 = {1} m²; volume = A·t = {2} m³; massa = {3} kg.", Ex.N(rad), Ex.N(2 * Math.PI * rad, 4), Ex.N(2 * Math.PI * rad * t, 5), Ex.N(mass)));
                }
            }
            case Difficulty.Hard:
            {
                double sy = Ex.Pick(r, 350.0, 500, 800, 1000), sf = Ex.Pick(r, 1.5, 2.0, 2.5), ts = p * rad * sf / (2 * sy * 1e6), tc = 2 * ts;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma esfera de pressão de raio {0} m deve operar a {1} m no mar (1025 kg/m³) com material de escoamento {2} MPa e FS = {3}. Qual a espessura mínima da parede em mm? (σ = p·r/(2t); g = 9,8 m/s²)", Ex.N(rad), Ex.N(depth), Ex.N(sy), Ex.N(sf)), ts * 1000, "mm", L.F("p = {0} MPa; t = p·r·FS / (2σ) = {0}·{1}·{2} / (2·{3}) = {4} mm.", Ex.N(p / 1e6, 3), Ex.N(rad), Ex.N(sf), Ex.N(sy), Ex.N(ts * 1000)));
                return Ex.Num(L.F("Para o mesmo raio ({0} m), profundidade ({1} m), material ({2} MPa) e FS = {3}, quanto mais espesso (em mm) precisa ser um cilindro do que uma esfera? (cilindro: t = p·r/σ; esfera: t = p·r/(2σ); g = 9,8 m/s², mar 1025 kg/m³)", Ex.N(rad), Ex.N(depth), Ex.N(sy), Ex.N(sf)), (tc - ts) * 1000, "mm", L.F("t_cil = {0} mm; t_esf = {1} mm; diferença = {2} mm (a esfera usa metade do material por unidade de pressão).", Ex.N(tc * 1000), Ex.N(ts * 1000), Ex.N((tc - ts) * 1000)));
            }
            default:
            {
                double sy = Ex.Pick(r, 500.0, 800, 1000, 1200), sf = Ex.Pick(r, 1.5, 2.0), tt = Ex.Step(r, 20, 120, 5) / 1000, rr = Ex.Step(r, 0.3, 1, 0.05);
                double pmax = 2 * sy * 1e6 * tt / (rr * sf), dmax = (pmax - Pk.P0) / (Pk.RhoSea * G);
                return Ex.Num(L.F("Uma esfera de raio {0} m e parede de {1} mm é feita de um material de escoamento {2} MPa, com FS = {3}. Qual a profundidade máxima de operação no mar (1025 kg/m³), contando também a pressão da atmosfera na superfície? (p_máx = 2σ·t / (r·FS); g = 9,8 m/s²)", Ex.N(rr), Ex.N(tt * 1000), Ex.N(sy), Ex.N(sf)),
                    dmax, "m", L.F("p_máx = 2·{0}·{1} / ({2}·{3}) = {4} MPa absolutos; profundidade = (p_máx − p₀) / (ρ·g) = {5} m.", Ex.N(sy), Ex.N(tt, 4), Ex.N(rr), Ex.N(sf), Ex.N(pmax / 1e6, 3), Ex.N(dmax)));
            }
        }
    }
}
