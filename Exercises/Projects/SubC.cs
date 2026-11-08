namespace EasyCreator.Exercises;

/// <summary>Submarines 11-15: payload foam, vertical terminal speed, hull thickness, sonar, energy and endurance.</summary>
[Exercises("sub-11-espuma-carga")]
public sealed class Sub11 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double rhoP = Ex.Pick(r, 2700.0, 4500, 7800, 8900), mp = Ex.Step(r, 1, 30, 0.5), vp = mp / rhoP * 1000, app = mp - Pk.RhoW * vp / 1000;
        string pl = L.F("uma carga de {0} kg de um material de {1} kg/m³", Ex.N(mp), Ex.N(rhoP));
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Qual o volume, em litros, de {0}?", pl), vp, "L", L.F("V = m / ρ = {0} / {1} = {2} m³ = {3} L.", Ex.N(mp), Ex.N(rhoP), Ex.N(vp / 1000, 5), Ex.N(vp, 3)));
                return Ex.Num(L.F("Dentro d'água doce (1000 kg/m³), qual a 'massa aparente' (massa − massa de água deslocada), em kg, de {0}?", pl), app, "kg", L.F("V = {0} L; água deslocada = {1} kg; massa aparente = {2} − {1} = {3} kg.", Ex.N(vp, 3), Ex.N(vp, 3), Ex.N(mp), Ex.N(app, 3)));
            case Difficulty.Medium:
            {
                double rf = Ex.Pick(r, 350.0, 400, 450, 500, 550), vf = app / ((Pk.RhoW - rf) / 1000);
                return Ex.Num(L.F("Um ROV neutro precisa levar {0} sem perder o equilíbrio em água doce (1000 kg/m³). Que volume de espuma de flutuação de {1} kg/m³ deve ser adicionado, em litros?", pl, Ex.N(rf)), vf, "L", L.F("Peso aparente da carga = {0} kg. Cada litro de espuma sustenta (1000 − {1})/1000 = {2} kg: V = {0}/{2} = {3} L.", Ex.N(app, 3), Ex.N(rf), Ex.N((Pk.RhoW - rf) / 1000, 3), Ex.N(vf, 3)));
            }
            case Difficulty.Hard:
            {
                double rf = Ex.Pick(r, 400.0, 450, 500, 550), vf = Ex.Step(r, 2, 20, 0.5), lift = vf * (Pk.RhoW - rf) / 1000, mmax = lift / (1 - Pk.RhoW / rhoP);
                return Ex.Num(L.F("Um ROV tem {0} L de espuma de {1} kg/m³ de reserva de flutuação em água doce (1000 kg/m³). Que massa máxima de um material de {2} kg/m³ ele pode carregar mantendo-se neutro?", Ex.N(vf), Ex.N(rf), Ex.N(rhoP)),
                    mmax, "kg", L.F("Sustentação da espuma = {0}·(1000 − {1})/1000 = {2} kg. Massa aparente = m·(1 − 1000/{3}); então m = {2} / (1 − 1000/{3}) = {4} kg.", Ex.N(vf), Ex.N(rf), Ex.N(lift, 3), Ex.N(rhoP), Ex.N(mmax, 3)));
            }
            default:
            {
                double M = Ex.Step(r, 10, 60, 1), F = app * G, a = F / (1.5 * M);
                return Ex.Num(L.F("Um ROV de {0} kg (já sem a carga) está neutro com {1}, compensada por espuma. Ao soltar a carga no fundo, a espuma passa a puxá-lo para cima. Com a massa de água adicional de 50% da massa do ROV, qual a aceleração inicial para cima? (a = F / (1,5·M); água doce, g = 9,8 m/s²; ignore o arrasto)", Ex.N(M), pl),
                    a, "m/s²", L.F("Força líquida = massa aparente · g = {0}·9,8 = {1} N; a = {1} / (1,5·{2}) = {3} m/s².", Ex.N(app, 3), Ex.N(F, 3), Ex.N(M), Ex.N(a, 3)));
            }
        }
    }
}

[Exercises("sub-12-velocidade-vertical")]
public sealed class Sub12 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double dia = Ex.Step(r, 20, 80, 5), A = Pk.Area(dia / 100), cd = Ex.Step(r, 0.6, 1.2, 0.1), dm = Ex.Step(r, 0.2, 5, 0.1), Wn = dm * G, k = 0.5 * Pk.RhoW * cd * A, vt = Math.Sqrt(Wn / k);
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um ROV tem {0} kg a mais de peso do que de empuxo e desce na vertical. Qual o peso líquido, em N? (g = 9,8 m/s²)", Ex.N(dm)), Wn, "N", L.F("P_líquido = {0}·9,8 = {1} N.", Ex.N(dm), Ex.N(Wn)));
                return Ex.Num(L.F("Um ROV desce na vertical com velocidade constante de {0} m/s, com Cd = {1} e área frontal (vista de cima) de {2} m², em água de 1000 kg/m³. Qual o arrasto, igual ao peso líquido?", Ex.N(vt, 3), Ex.N(cd), Ex.N(A, 4)), Wn, "N", L.F("F = ½·ρ·Cd·A·v² = 0,5·1000·{0}·{1}·{2}² = {3} N.", Ex.N(cd), Ex.N(A, 4), Ex.N(vt, 3), Ex.N(Wn)), 3);
            case Difficulty.Medium:
                return Ex.Num(L.F("Um ROV tem {0} kg a mais de peso do que de empuxo. Com Cd = {1} e área horizontal de {2} cm de diâmetro, qual a velocidade terminal de descida na água (1000 kg/m³)? (g = 9,8 m/s²)", Ex.N(dm), Ex.N(cd), Ex.N(dia)), vt, "m/s", L.F("P_líquido = {0} N; v = √(P / (½·ρ·Cd·A)) = √({0} / {1}) = {2} m/s.", Ex.N(Wn), Ex.N(k, 3), Ex.N(vt, 3)));
            case Difficulty.Hard:
            {
                double D = Ex.Step(r, 20, 300, 10), t = D / vt;
                return Ex.Num(L.F("Um ROV com {0} kg de excesso de peso e Cd = {1}, com {2} cm de diâmetro horizontal, desce {3} m na água (1000 kg/m³), já na velocidade terminal. Quantos segundos leva? (g = 9,8 m/s²)", Ex.N(dm), Ex.N(cd), Ex.N(dia), Ex.N(D)),
                    t, "s", L.F("v = √(P / (½·ρ·Cd·A)) = {0} m/s; t = D / v = {1} / {0} = {2} s.", Ex.N(vt, 3), Ex.N(D), Ex.N(t)));
            }
            default:
            {
                double vtar = Ex.Step(r, 0.1, 1.5, 0.05), mx = k * vtar * vtar / G * 1000;
                return Ex.Num(L.F("Um ROV de {0} cm de diâmetro horizontal e Cd = {1} deve descer com calma, a {2} m/s constantes, na água (1000 kg/m³). Que excesso de peso (em gramas de massa) o lastro deve dar? (g = 9,8 m/s²)", Ex.N(dia), Ex.N(cd), Ex.N(vtar)), mx, "g", L.F("Equilíbrio: Δm·g = ½·ρ·Cd·A·v² → Δm = {0}·{1}² / 9,8 = {2} kg = {3} g.", Ex.N(k, 3), Ex.N(vtar), Ex.N(mx / 1000, 5), Ex.N(mx, 2)));
            }
        }
    }
}

[Exercises("sub-13-espessura-casco")]
public sealed class Sub13 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double depth = Ex.Step(r, 100, 4000, 100), p = Pk.RhoSea * G * depth, rad = Ex.Step(r, 0.1, 1, 0.05);
        switch (d)
        {
            case Difficulty.Easy:
            {
                double t = Ex.Step(r, 5, 60, 1) / 1000, sig = p * rad / (2 * t);
                return Ex.Num(L.F("Uma esfera de raio {0} m e parede de {1} mm está a {2} m no mar (1025 kg/m³). Qual a tensão na parede, σ = p·r/(2t)? (g = 9,8 m/s²; use a pressão manométrica)", Ex.N(rad), Ex.N(t * 1000), Ex.N(depth)), sig / 1e6, "MPa", L.F("p = {0} MPa; σ = {0}·{1} / (2·{2}) = {3} MPa.", Ex.N(p / 1e6, 3), Ex.N(rad), Ex.N(t, 4), Ex.N(sig / 1e6)));
            }
            case Difficulty.Medium:
            {
                double sy = Ex.Pick(r, 300.0, 500, 800, 1000), sf = Ex.Pick(r, 1.5, 2.0, 2.5), ts = p * rad * sf / (2 * sy * 1e6), tc = 2 * ts;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma esfera de raio {0} m deve operar a {1} m no mar (1025 kg/m³), com material de escoamento {2} MPa e FS = {3}. Qual a espessura mínima, em mm? (g = 9,8 m/s²)", Ex.N(rad), Ex.N(depth), Ex.N(sy), Ex.N(sf)), ts * 1000, "mm", L.F("t = p·r·FS / (2σ) = {0}·{1}·{2} / (2·{3}) = {4} mm.", Ex.N(p / 1e6, 3), Ex.N(rad), Ex.N(sf), Ex.N(sy), Ex.N(ts * 1000, 3)));
                return Ex.Num(L.F("Um cilindro de raio {0} m deve operar a {1} m no mar (1025 kg/m³), com material de escoamento {2} MPa e FS = {3}. Qual a espessura mínima, em mm? (σ = p·r/t; g = 9,8 m/s²)", Ex.N(rad), Ex.N(depth), Ex.N(sy), Ex.N(sf)), tc * 1000, "mm", L.F("t = p·r·FS / σ = {0}·{1}·{2} / {3} = {4} mm.", Ex.N(p / 1e6, 3), Ex.N(rad), Ex.N(sf), Ex.N(sy), Ex.N(tc * 1000, 3)));
            }
            case Difficulty.Hard:
            {
                var mat = Ex.Pick(r, (E: 200e9, N: 0), (E: 70e9, N: 1), (E: 110e9, N: 2));
                double t = Ex.Step(r, 8, 40, 1) / 1000, nu = 0.3, pcr = 2 * mat.E / Math.Sqrt(3 * (1 - nu * nu)) * Math.Pow(t / rad, 2), dcr = pcr / (Pk.RhoSea * G);
                string mn = mat.N == 0 ? L.T("aço") : mat.N == 1 ? L.T("alumínio") : L.T("titânio");
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("A pressão crítica de colapso por flambagem de uma esfera fina é p = 2E/√(3(1−ν²))·(t/r)². Para uma esfera de {0} com E = {1} GPa, ν = 0,3, raio {2} m e parede {3} mm, qual essa pressão, em MPa?", mn, Ex.N(mat.E / 1e9), Ex.N(rad), Ex.N(t * 1000)), pcr / 1e6, "MPa", L.F("(t/r)² = ({0}/{1})² = {2}; p = 2·{3}/√(3·0,91)·{2} = {4} MPa.", Ex.N(t, 4), Ex.N(rad), Ex.N(Math.Pow(t / rad, 2), 6), Ex.N(mat.E), Ex.N(pcr / 1e6, 3)));
                return Ex.Num(L.F("A pressão crítica de colapso por flambagem de uma esfera fina é p = 2E/√(3(1−ν²))·(t/r)². Para uma esfera de {0} com E = {1} GPa, ν = 0,3, raio {2} m e parede {3} mm, a que profundidade no mar (1025 kg/m³) ela colapsaria? (g = 9,8 m/s²)", mn, Ex.N(mat.E / 1e9), Ex.N(rad), Ex.N(t * 1000)), dcr, "m", L.F("p_cr = {0} MPa; profundidade = p_cr / (ρ·g) = {1} / (1025·9,8) = {2} m.", Ex.N(pcr / 1e6, 3), Ex.N(pcr, 0), Ex.N(dcr)), 3);
            }
            default:
            {
                double sy = Ex.Pick(r, 500.0, 800, 1000), sf = 2.0, rhoM = Ex.Pick(r, 2700.0, 4500, 7850), ts = p * rad * sf / (2 * sy * 1e6), ms = 4 * Math.PI * rad * rad * ts * rhoM, md = Pk.RhoSea * 4.0 / 3 * Math.PI * Math.Pow(rad, 3), ratio = ms / md * 100;
                return Ex.Num(L.F("Uma esfera de pressão de raio {0} m, feita de um material de {1} kg/m³ e escoamento {2} MPa (FS = 2), opera a {3} m no mar (1025 kg/m³). Que porcentagem da massa de água que ela desloca é a massa do próprio casco? (casca fina: m = 4π·r²·t·ρ; g = 9,8 m/s²) Quanto menor, mais carga útil sobra.", Ex.N(rad), Ex.N(rhoM), Ex.N(sy), Ex.N(depth)),
                    ratio, "%", L.F("t = p·r·FS/(2σ) = {0} mm; m_casco = 4π·{1}²·{2}·{3} = {4} kg; água deslocada = {5} kg; razão = {6}%.", Ex.N(ts * 1000, 3), Ex.N(rad), Ex.N(ts, 5), Ex.N(rhoM), Ex.N(ms, 3), Ex.N(md, 3), Ex.N(ratio, 3)));
            }
        }
    }
}

[Exercises("sub-14-sonar")]
public sealed class Sub14 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                double c = Ex.Step(r, 1450, 1550, 10), dist = Ex.Step(r, 5, 500, 5), t = 2 * dist / c;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("O eco de um sonar chega {0} ms depois do pulso, com som a {1} m/s na água. Qual a distância do alvo?", Ex.N(t * 1000, 3), Ex.N(c)), dist, "m", L.F("d = c·t / 2 = {0}·{1} / 2 = {2} m.", Ex.N(c), Ex.N(t, 6), Ex.N(dist)));
                return Ex.Num(L.F("O alvo de um sonar está a {0} m e o som viaja a {1} m/s. Quantos milissegundos leva para ouvir o eco?", Ex.N(dist), Ex.N(c)), t * 1000, "ms", L.F("t = 2·d / c = 2·{0} / {1} = {2} ms.", Ex.N(dist), Ex.N(c), Ex.N(t * 1000, 3)));
            }
            case Difficulty.Medium:
            {
                double c = Ex.Step(r, 1450, 1550, 10), prf = Ex.Step(r, 0.5, 20, 0.5), maxr = c / (2 * prf);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um sonar emite {0} pulsos por segundo (som a {1} m/s). Para não confundir ecos de pulsos diferentes, qual o alcance máximo não ambíguo, em metros?", Ex.N(prf), Ex.N(c)), maxr, "m", L.F("O eco precisa voltar antes do pulso seguinte: T = 1/{0} = {1} s; alcance = c·T/2 = {2} m.", Ex.N(prf), Ex.N(1 / prf, 4), Ex.N(maxr)));
                double rng = Ex.Step(r, 20, 300, 10);
                return Ex.Num(L.F("Um sonar precisa ver alvos até {0} m (som a {1} m/s). Qual o maior número de pulsos por segundo que ele pode emitir sem ambiguidade?", Ex.N(rng), Ex.N(c)), c / (2 * rng), "Hz", L.F("Tempo de ida e volta = 2·{0}/{1} = {2} s; taxa máxima = 1/{2} = {3} pulsos/s.", Ex.N(rng), Ex.N(c), Ex.N(2 * rng / c, 5), Ex.N(c / (2 * rng), 3)));
            }
            case Difficulty.Hard:
            {
                double dist = Ex.Step(r, 50, 2000, 50), al = Ex.Step(r, 0.05, 1.0, 0.05), tl = 20 * Math.Log10(dist) + al * dist / 1000;
                return Ex.Num(L.F("A perda de transmissão de um pulso de sonar é PT = 20·log₁₀(r) + α·r/1000, com r em metros e α em dB/km. Para r = {0} m e α = {1} dB/km, qual a perda em dB (um sentido)?", Ex.N(dist), Ex.N(al)), tl, "dB", L.F("20·log₁₀({0}) = {1} dB; absorção = {2}·{0}/1000 = {3} dB; total = {4} dB.", Ex.N(dist), Ex.N(20 * Math.Log10(dist), 3), Ex.N(al), Ex.N(al * dist / 1000, 3), Ex.N(tl, 3)));
            }
            default:
            {
                double dist = Ex.Step(r, 100, 1500, 50), al = Ex.Step(r, 0.1, 1.0, 0.1), ts = Ex.Step(r, -30, 10, 5), nl = Ex.Step(r, 40, 70, 5), snrTarget = Ex.Step(r, 8, 25, 1);
                double tl = 20 * Math.Log10(dist) + al * dist / 1000, sl = Math.Round(snrTarget + 2 * tl - ts + nl), snr = sl - 2 * tl + ts - nl;
                return Ex.Num(L.F("Num sonar ativo, a relação sinal/ruído do eco é RSR = NF − 2·PT + FA − NR, com NF = {0} dB (nível da fonte), FA = {1} dB (força do alvo) e NR = {2} dB (ruído). A perda de um sentido é PT = 20·log₁₀(r) + α·r/1000 com α = {3} dB/km. Para um alvo a {4} m, qual a RSR em dB?", Ex.N(sl), Ex.N(ts), Ex.N(nl), Ex.N(al), Ex.N(dist)),
                    snr, "dB", L.F("PT = 20·log₁₀({0}) + {1}·{0}/1000 = {2} dB; RSR = {3} − 2·{2} + ({4}) − {5} = {6} dB.", Ex.N(dist), Ex.N(al), Ex.N(tl, 3), Ex.N(sl), Ex.N(ts), Ex.N(nl), Ex.N(snr, 3)), 3);
            }
        }
    }
}

[Exercises("sub-15-energia-autonomia")]
public sealed class Sub15 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double wh = Ex.Step(r, 100, 3000, 50), p = Ex.Step(r, 20, 500, 10), v = Ex.Step(r, 0.5, 3, 0.1);
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um AUV com bateria de {0} Wh consome {1} W em média. Quantas horas dura?", Ex.N(wh), Ex.N(p)), wh / p, "h", L.F("t = E / P = {0} / {1} = {2} h.", Ex.N(wh), Ex.N(p), Ex.N(wh / p, 3)));
                return Ex.Num(L.F("Um AUV consome {0} W e precisa operar {1} h. Qual a energia mínima da bateria, em Wh?", Ex.N(p), Ex.N(wh / p, 3)), wh, "Wh", L.F("E = P·t = {0}·{1} = {2} Wh.", Ex.N(p), Ex.N(wh / p, 3), Ex.N(wh)));
            case Difficulty.Medium:
            {
                double t = wh / p, rng = v * 3600 * t / 1000;
                return Ex.Num(L.F("Um AUV com {0} Wh consome {1} W navegando a {2} m/s. Qual o alcance, em km?", Ex.N(wh), Ex.N(p), Ex.N(v)), rng, "km", L.F("t = {0}/{1} = {2} h = {3} s; alcance = v·t = {4}·{3} = {5} m = {6} km.", Ex.N(wh), Ex.N(p), Ex.N(t, 3), Ex.N(t * 3600, 1), Ex.N(v), Ex.N(v * t * 3600), Ex.N(rng, 3)));
            }
            case Difficulty.Hard:
            {
                double ph = Ex.Step(r, 10, 100, 5), dia = Ex.Step(r, 20, 60, 5), cd = Ex.Step(r, 0.2, 0.6, 0.05), eta = Ex.Step(r, 0.3, 0.6, 0.05), k = 0.5 * Pk.RhoW * cd * Pk.Area(dia / 100) / eta;
                double ptot = ph + k * v * v * v, t = wh / ptot, rng = v * t * 3.6;
                return Ex.Num(L.F("Um AUV de {0} cm de diâmetro e Cd = {1} navega a {2} m/s, com eficiência de propulsão {3} e carga fixa de {4} W (sensores e computador). A bateria é de {5} Wh. Qual o alcance, em km? (água: 1000 kg/m³; potência de propulsão = ½·ρ·Cd·A·v³ / η)", Ex.N(dia), Ex.N(cd), Ex.N(v), Ex.N(eta), Ex.N(ph), Ex.N(wh)),
                    rng, "km", L.F("P_prop = {0} W; P_total = {0} + {1} = {2} W; t = {3}/{2} = {4} h; alcance = {5}·{4}·3,6 = {6} km.", Ex.N(k * v * v * v, 3), Ex.N(ph), Ex.N(ptot, 3), Ex.N(wh), Ex.N(t, 3), Ex.N(v), Ex.N(rng, 3)));
            }
            default:
            {
                double ph = Ex.Step(r, 10, 120, 5), dia = Ex.Step(r, 20, 60, 5), cd = Ex.Step(r, 0.2, 0.6, 0.05), eta = Ex.Step(r, 0.3, 0.6, 0.05), k = 0.5 * Pk.RhoW * cd * Pk.Area(dia / 100) / eta;
                double vopt = Math.Cbrt(ph / (2 * k)), ptot = ph + k * Math.Pow(vopt, 3), rng = wh / ptot * vopt * 3.6;
                return Ex.Num(L.F("Um AUV de {0} cm de diâmetro, Cd = {1} e eficiência de propulsão {2} tem carga fixa de {3} W. Sua potência de propulsão é k·v³, com k = ½·ρ·Cd·A/η (água 1000 kg/m³). Há uma velocidade que maximiza o alcance: v = (P_fixa / (2k))^(1/3). Qual é ela, em m/s?", Ex.N(dia), Ex.N(cd), Ex.N(eta), Ex.N(ph)),
                    vopt, "m/s", L.F("k = {0}; v = ({1}/(2·{0}))^(1/3) = {2} m/s. (Nessa velocidade a propulsão gasta metade da carga fixa; é onde a energia por metro é mínima.)", Ex.N(k, 4), Ex.N(ph), Ex.N(vopt, 3)));
            }
        }
    }
}
