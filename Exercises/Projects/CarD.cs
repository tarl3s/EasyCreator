using static EasyCreator.Exercises.Pv;

namespace EasyCreator.Exercises;

/// <summary>Cars 16-20: advanced suspension, EV on the road, combustion engine, wing setup, the final sports-car design.</summary>
[Exercises("car-16-suspensao-avancada")]
public sealed class Car16 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double ks = Ex.Step(r, 30, 90, 5) * 1000, mr = Ex.Step(r, 0.6, 1.0, 0.05), m = Ex.Step(r, 250, 450, 10);
        double kw = ks * mr * mr, f = Math.Sqrt(kw / m) / (2 * Math.PI);
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma mola de {0} N/m trabalha numa alavanca com relação de movimento (motion ratio) de {1}. Qual a rigidez efetiva na roda, em N/m?", N(ks), N(mr)), kw, "N/m",
                        L.F("k_roda = k_mola·MR² = {0}·{1}² = {2} N/m.", N(ks), N(mr), N(kw)));
                return Ex.Num(L.F("Uma suspensão precisa de {0} N/m na roda e a mola trabalha com motion ratio de {1}. Que rigidez a mola deve ter, em N/m?", N(kw), N(mr)), ks, "N/m",
                    L.F("k_mola = k_roda / MR² = {0} / {1}² = {2} N/m.", N(kw), N(mr), N(ks)));
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("A roda sustenta {0} kg e a mola de {1} N/m tem motion ratio {2}. Qual a frequência de pulo (ride), em Hz?", N(m), N(ks), N(mr)), f, "Hz",
                        L.F("k_roda = {0}·{1}² = {2} N/m; f = (1/2π)·√({2}/{3}) = {4} Hz.", N(ks), N(mr), N(kw), N(m), N(f)));
                return Ex.Num(L.F("Para uma frequência de {0} Hz com {1} kg por roda, qual a rigidez na roda, em N/m?", N(f, 3), N(m)), kw, "N/m",
                    L.F("k = m·(2π·f)² = {0}·(2π·{1})² = {2} N/m.", N(m), N(f, 3), N(kw)));
            case Difficulty.Hard:
            {
                double z = Ex.Step(r, 0.3, 0.8, 0.05), cw = 2 * z * Math.Sqrt(kw * m), cd = cw / (mr * mr);
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Que amortecimento na roda c (N·s/m) dá ζ = {0} a uma roda de {1} kg com rigidez de {2} N/m?", N(z), N(m), N(kw)), cw, "N·s/m",
                        L.F("c = 2·ζ·√(k·m) = 2·{0}·√({1}·{2}) = {3} N·s/m.", N(z), N(kw), N(m), N(cw)));
                    case 1: return Ex.Num(L.F("O amortecedor trabalha na mesma alavanca da mola (motion ratio {0}). Para ter c = {1} N·s/m na roda, qual o c do amortecedor, em N·s/m?", N(mr), N(cw)), cd, "N·s/m",
                        L.F("c_roda = c_amort·MR² → c_amort = {0} / {1}² = {2} N·s/m.", N(cw), N(mr), N(cd)));
                    default:
                    {
                        double zeta = cw / (2 * Math.Sqrt(kw * m));
                        return Ex.Num(L.F("Uma roda de {0} kg tem rigidez de {1} N/m e amortecimento de {2} N·s/m (na roda). Qual a razão de amortecimento ζ?", N(m), N(kw), N(cw)), zeta, "",
                            L.F("ζ = c / (2·√(k·m)) = {0} / (2·√({1}·{2})) = {3}.", N(cw), N(kw), N(m), N(zeta, 3)), 3);
                    }
                }
            }
            default:
            {
                double z = Ex.Step(r, 0.3, 0.7, 0.05), ft = Ex.Step(r, 1.2, 2.5, 0.1), kt = m * Math.Pow(2 * Math.PI * ft, 2), cwt = 2 * z * Math.Sqrt(kt * m);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Projete uma suspensão: roda de {0} kg, frequência de pulo {1} Hz, ζ = {2} e motion ratio {3} (mola e amortecedor na mesma alavanca). Qual o c do amortecedor, em N·s/m?", N(m), N(ft), N(z), N(mr)), cwt / (mr * mr), "N·s/m",
                        L.F("k na roda = m·(2π·f)² = {0} N/m; c na roda = 2·ζ·√(k·m) = {1} N·s/m; no amortecedor: c / MR² = {2} N·s/m.", N(kt), N(cwt), N(cwt / (mr * mr))), 3);
                return Ex.Num(L.F("Projete uma suspensão: roda de {0} kg, frequência de pulo {1} Hz e motion ratio {2}. Qual a rigidez da mola, em N/m?", N(m), N(ft), N(mr)), kt / (mr * mr), "N/m",
                    L.F("k na roda = m·(2π·f)² = {0} N/m; k da mola = k_roda / MR² = {0} / {1}² = {2} N/m.", N(kt), N(mr), N(kt / (mr * mr))), 3);
            }
        }
    }
}

[Exercises("car-17-ev-estrada")]
public sealed class Car17 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G, Rho = Pk.Rho;
        double m = Ex.Step(r, 1300, 2300, 100), cd = Ex.Step(r, 0.22, 0.32, 0.01), A = Ex.Step(r, 2.1, 2.7, 0.1), crr = Ex.Step(r, 0.008, 0.012, 0.001), eta = Ex.Pick(r, 0.85, 0.88, 0.9), cap = Ex.Step(r, 45, 100, 5);
        Func<double, double> F = v => 0.5 * Rho * cd * A * v * v + crr * m * G;
        Func<double, double> cons = v => F(v) / eta * 1000 / 3.6e6 * 100;
        string car = L.F("Um elétrico de {0} kg (Cd = {1}, área {2} m², Crr = {3}, rendimento bateria–roda {4}, bateria de {5} kWh).", N(m), N(cd), N(A), N(crr, 3), N(eta), N(cap));
        switch (d)
        {
            case Difficulty.Easy:
            {
                double kmh = Ex.Step(r, 60, 130, 10), v = Ms(kmh);
                if (r.Next(2) == 0)
                    return Ex.Num(car + " " + L.F("Qual a força total de resistência a {0} km/h, em N? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", N(kmh)), F(v), "N",
                        L.F("F = ½·ρ·Cd·A·v² + Crr·m·g = {0} + {1} = {2} N.", N(0.5 * Rho * cd * A * v * v), N(crr * m * G), N(F(v))));
                return Ex.Num(car + " " + L.F("Qual a potência nas rodas a {0} km/h, em kW? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", N(kmh)), F(v) * v / 1000, "kW",
                    L.F("F = {0} N; P = F·v = {0}·{1} = {2} kW.", N(F(v)), N(v), N(F(v) * v / 1000)));
            }
            case Difficulty.Medium:
            {
                double kmh = Ex.Step(r, 60, 140, 10), v = Ms(kmh);
                if (r.Next(2) == 0)
                    return Ex.Num(car + " " + L.F("Qual o consumo a {0} km/h, em kWh/100 km? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", N(kmh)), cons(v), "kWh/100 km",
                        L.F("F = {0} N; energia por 100 km = F·100 000 / η = {1} kWh.", N(F(v)), N(cons(v))), 3);
                return Ex.Num(car + " " + L.F("Qual a autonomia a {0} km/h, em km, usando 100% da bateria? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", N(kmh)), cap / cons(v) * 100, "km",
                    L.F("Consumo = {0} kWh/100 km → autonomia = {1}/{0}·100 = {2} km.", N(cons(v)), N(cap), N(cap / cons(v) * 100)), 3);
            }
            case Difficulty.Hard:
            {
                double k1 = Ex.Step(r, 70, 100, 10), k2 = Ex.Step(r, 120, 140, 10), r1 = cap / cons(Ms(k1)) * 100, r2 = cap / cons(Ms(k2)) * 100;
                if (r.Next(2) == 0)
                    return Ex.Num(car + " " + L.F("Quantos km de autonomia se perdem indo a {0} km/h em vez de {1} km/h (100% da bateria)?", N(k2), N(k1)), r1 - r2, "km",
                        L.F("Autonomia a {0} km/h: {1} km; a {2} km/h: {3} km; perda: {4} km.", N(k1), N(r1), N(k2), N(r2), N(r1 - r2)), 3);
                double dist = Ex.Step(r, 150, 400, 25), v = Ms(k1), need = dist * cons(v) / 100;
                return Ex.Num(car + " " + L.F("Numa viagem de {0} km a {1} km/h, que energia, em kWh, sai da bateria?", N(dist), N(k1)), need, "kWh",
                    L.F("Consumo = {0} kWh/100 km; E = {0}·{1}/100 = {2} kWh.", N(cons(v)), N(dist), N(need)), 3);
            }
            default:
            {
                double dcap = Ex.Step(r, 10, 40, 5), wh = Ex.Pick(r, 140.0, 160, 180), dm = dcap * 1000 / wh;
                double kmh = Ex.Step(r, 90, 120, 10), v = Ms(kmh);
                Func<double, double> cons2 = vv => (0.5 * Rho * cd * A * vv * vv + crr * (m + dm) * G) / eta * 1000 / 3.6e6 * 100;
                double r0 = cap / cons(v) * 100, r1 = (cap + dcap) / cons2(v) * 100;
                if (r.Next(2) == 0)
                    return Ex.Num(car + " " + L.F("Aumenta-se a bateria em {0} kWh, num pacote de {1} Wh/kg (que pesa mais). A {2} km/h, qual a nova autonomia, em km? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", N(dcap), N(wh), N(kmh)), r1, "km",
                        L.F("Massa extra = {0} kWh / {1} Wh/kg = {2} kg; só o rolamento muda (Crr·m·g). Novo consumo = {3} kWh/100 km; autonomia = ({4}/{3})·100 = {5} km.", N(dcap), N(wh), N(dm), N(cons2(v)), N(cap + dcap), N(r1)), 3);
                return Ex.Num(car + " " + L.F("Aumenta-se a bateria em {0} kWh, num pacote de {1} Wh/kg. A {2} km/h, quantos km de autonomia a mais isso dá, contando o peso extra? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", N(dcap), N(wh), N(kmh)), r1 - r0, "km",
                    L.F("Antes: {0} km. Depois, com {1} kg a mais: {2} km. Ganho: {3} km.", N(r0), N(dm), N(r1), N(r1 - r0)), 3);
            }
        }
    }
}

[Exercises("car-18-motor-termico")]
public sealed class Car18 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double Lhv = 44e6, Gamma = 1.4, FuelRho = 0.74;
        double cr = Ex.Step(r, 8, 14, 0.5), eOtto = 1 - Math.Pow(cr, 1 - Gamma);
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Qual a eficiência térmica ideal (ciclo Otto, γ = 1,4), em %, de um motor com taxa de compressão {0}:1?", N(cr)), eOtto * 100, "%",
                        L.F("η = 1 − r^(1 − γ) = 1 − {0}^(−0,4) = {1}%.", N(cr), N(eOtto * 100)));
                double w = Ex.Step(r, 40, 150, 10), q = w / eOtto;
                return Ex.Num(L.F("Um motor ideal (Otto, taxa de compressão {0}:1, γ = 1,4) produz {1} kJ de trabalho por ciclo. Quantos kJ de calor o combustível fornece?", N(cr), N(w)), q, "kJ",
                    L.F("η = {0}; Q = W / η = {1} / {0} = {2} kJ.", N(eOtto, 3), N(w), N(q)));
            case Difficulty.Medium:
            {
                double bsfc = Ex.Step(r, 230, 330, 2) / 1000, eta = 3.6e6 / (bsfc * Lhv);
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um motor a gasolina (PCI 44 MJ/kg) tem BSFC de {0} g/kWh. Qual sua eficiência térmica real, em %?", N(bsfc * 1000, 0)), eta * 100, "%",
                        L.F("η = 3,6·10⁶ / (BSFC·PCI) = 3,6·10⁶ / ({0}·44·10⁶) = {1}%.", N(bsfc, 3), N(eta * 100)));
                    default: return Ex.Num(L.F("Um motor a gasolina (PCI 44 MJ/kg) tem eficiência térmica de {0}%. Qual o BSFC, em g/kWh?", N(eta * 100, 1)), bsfc * 1000, "g/kWh",
                        L.F("BSFC = 3,6·10⁶ / (η·PCI) = 3,6·10⁶ / ({0}·44·10⁶) = {1} kg/kWh = {2} g/kWh.", N(eta, 3), N(bsfc, 3), N(bsfc * 1000, 0)));
                }
            }
            case Difficulty.Hard:
            {
                double bsfc = Ex.Step(r, 240, 320, 10) / 1000, P = Ex.Step(r, 8, 60, 2), kmh = Ex.Step(r, 60, 120, 10), flow = P * bsfc, l100 = flow / kmh * 100 / FuelRho;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um motor com BSFC de {0} g/kWh entrega {1} kW. Quantos kg de gasolina ele queima por hora?", N(bsfc * 1000, 0), N(P)), flow, "kg/h",
                        L.F("ṁ = P·BSFC = {0}·{1} = {2} kg/h.", N(P), N(bsfc, 3), N(flow, 3)));
                    case 1: return Ex.Num(L.F("Um carro precisa de {0} kW do motor para andar a {1} km/h. O BSFC é {2} g/kWh e a gasolina tem 0,74 kg/L. Qual o consumo, em L/100 km?", N(P), N(kmh), N(bsfc * 1000, 0)), l100, "L/100 km",
                        L.F("Combustível por hora: {0}·{1} = {2} kg/h = {3} L/h. Em 100 km (leva 100/{4} h): {5} L.", N(P), N(bsfc, 3), N(flow, 3), N(flow / FuelRho, 2), N(kmh), N(l100)), 3);
                    default:
                    {
                        double tank = Ex.Step(r, 40, 70, 5), range = tank / l100 * 100;
                        return Ex.Num(L.F("Um carro de tanque com {0} L precisa de {1} kW do motor a {2} km/h, com BSFC de {3} g/kWh (gasolina 0,74 kg/L). Qual a autonomia, em km?", N(tank), N(P), N(kmh), N(bsfc * 1000, 0)), range, "km",
                            L.F("L/100 km = {0}·{1}/0,74/{2}·100 = {3}; autonomia = {4}/{3}·100 = {5} km.", N(P), N(bsfc, 3), N(kmh), N(l100), N(tank), N(range)), 3);
                    }
                }
            }
            default:
            {
                double frac = Ex.Step(r, 0.6, 0.75, 0.05), etaR = eOtto * frac, bsfc = 3.6e6 / (etaR * Lhv) * 1000;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um motor com taxa de compressão {0}:1 (Otto, γ = 1,4) real atinge só {1}% da eficiência ideal. Qual seu BSFC, em g/kWh, com gasolina de 44 MJ/kg?", N(cr), N(frac * 100, 0)), bsfc, "g/kWh",
                        L.F("η ideal = 1 − r^(−0,4) = {0}; real = {1}·{0} = {2}; BSFC = 3,6·10⁶ / (η·44·10⁶) = {3} g/kWh.", N(eOtto, 3), N(frac), N(etaR, 3), N(bsfc)), 3);
                    default:
                    {
                        double cr2 = cr + Ex.Step(r, 1, 3, 0.5), e2 = 1 - Math.Pow(cr2, 1 - Gamma);
                        return Ex.Num(L.F("Subir a taxa de compressão de {0}:1 para {1}:1 (Otto, γ = 1,4) aumenta a eficiência ideal em quantos pontos percentuais?", N(cr), N(cr2)), (e2 - eOtto) * 100, "pp",
                            L.F("η₁ = {0}%, η₂ = {1}% → diferença = {2} pontos.", N(eOtto * 100), N(e2 * 100), N((e2 - eOtto) * 100)), 3);
                    }
                }
            }
        }
    }
}

[Exercises("car-19-acerto-asa")]
public sealed class Car19 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double ls = Ex.Step(r, 400, 900, 50), lc = Ex.Step(r, 300, 800, 50);
        double vs = Ex.Step(r, 90, 160, 5), vc = Ex.Step(r, 50, 90, 5);
        double t = ls / Ms(vs) + lc / Ms(vc);
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma pista tem {0} m de retas, percorridas a {1} km/h em média. Quanto tempo se gasta nas retas, em s?", N(ls), N(vs)), ls / Ms(vs), "s",
                        L.F("t = L / v = {0} / {1} = {2} s.", N(ls), N(Ms(vs)), N(ls / Ms(vs))));
                return Ex.Num(L.F("Uma pista tem {0} m de curvas, percorridas a {1} km/h em média. Quanto tempo se gasta nas curvas, em s?", N(lc), N(vc)), lc / Ms(vc), "s",
                    L.F("t = L / v = {0} / {1} = {2} s.", N(lc), N(Ms(vc)), N(lc / Ms(vc))));
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma pista tem {0} m de retas a {1} km/h médios e {2} m de curvas a {3} km/h médios. Qual o tempo de volta, em s?", N(ls), N(vs), N(lc), N(vc)), t, "s",
                        L.F("t = {0}/{1} + {2}/{3} = {4} + {5} = {6} s.", N(ls), N(Ms(vs)), N(lc), N(Ms(vc)), N(ls / Ms(vs)), N(lc / Ms(vc)), N(t)));
                return Ex.Num(L.F("A volta de {0} m (retas de {1} m a {2} km/h médios) leva {3} s. Com que velocidade média, em km/h, são feitas as curvas?", N(ls + lc), N(ls), N(vs), N(t)), vc, "km/h",
                    L.F("Tempo nas curvas = {0} − {1}/{2} = {3} s; v = {4}/{3} = {5} m/s = {6} km/h.", N(t), N(ls), N(Ms(vs)), N(lc / Ms(vc)), N(lc), N(Ms(vc)), N(vc)), 3);
            case Difficulty.Hard:
            {
                double vsB = vs + Ex.Step(r, 8, 20, 1), vcB = vc - Ex.Step(r, 3, 8, 1);
                double tA = t, tB = ls / Ms(vsB) + lc / Ms(vcB);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Asa A: retas a {0} km/h e curvas a {1} km/h. Asa B: retas a {2} km/h e curvas a {3} km/h. Numa pista com {4} m de retas e {5} m de curvas, quantos segundos a asa A é mais rápida por volta que a B? (negativo se a B for mais rápida)", N(vs), N(vc), N(vsB), N(vcB), N(ls), N(lc)), tB - tA, "s",
                        L.F("t_A = {0} s; t_B = {1} s; diferença = t_B − t_A = {2} s.", N(tA), N(tB), N(tB - tA)), 3);
                return Ex.Choice(r, L.F("Asa A: retas a {0} km/h e curvas a {1} km/h. Asa B: retas a {2} km/h e curvas a {3} km/h. A pista tem {4} m de retas e {5} m de curvas. Qual asa dá a volta mais rápida?", N(vs), N(vc), N(vsB), N(vcB), N(ls), N(lc)),
                    tA < tB ? L.T("Asa A") : L.T("Asa B"), new[] { tA < tB ? L.T("Asa B") : L.T("Asa A"), L.T("Dá o mesmo tempo") },
                    L.F("t_A = {0} s; t_B = {1} s. Vence a de menor tempo.", N(tA), N(tB)));
            }
            default:
            {
                double vsB = vs + Ex.Step(r, 8, 20, 1);
                double tA = t, tStrB = ls / Ms(vsB), needCorner = lc / (tA - tStrB), pct = (Ms(vc) / needCorner - 1);
                // Asa B is faster on the straights; how fast must the corners stay to tie? Percentage drop allowed.
                double allowed = (1 - needCorner / Ms(vc)) * 100;
                return Ex.Num(L.F("Com a asa A, a volta tem retas de {0} m a {1} km/h e curvas de {2} m a {3} km/h. A asa B tira arrasto: retas a {4} km/h, mas perde aderência nas curvas. Quantos por cento a velocidade nas curvas pode cair, no máximo, para a asa B ainda empatar o tempo de volta?", N(ls), N(vs), N(lc), N(vc), N(vsB)), allowed, "%",
                    L.F("t_A = {0} s. Com B as retas levam {1} s, sobram {2} s para as curvas: v = {3}/{2} = {4} m/s, que é {5}% abaixo dos {6} m/s da asa A.", N(tA), N(tStrB), N(tA - tStrB), N(lc), N(needCorner), N(allowed), N(Ms(vc))), 3);
            }
        }
    }
}

[Exercises("car-20-projeto-esportivo")]
public sealed class Car20 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G, Rho = Pk.Rho;
        double m = Ex.Step(r, 1000, 1700, 50), P = Ex.Step(r, 80, 300, 10) * 1000, cd = Ex.Step(r, 0.27, 0.36, 0.01), A = Ex.Step(r, 1.9, 2.3, 0.1);
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um esportivo de {0} kg tem motor de {1} kW. Qual a relação potência/peso, em kW por tonelada?", N(m), N(P / 1000)), P / 1000 / (m / 1000), "kW/t",
                        L.F("{0} / ({1}/1000) = {2} kW/t.", N(P / 1000), N(m), N(P / m)));
                {
                    double kmh = Ex.Step(r, 80, 130, 10), v = Ms(kmh), mu = Ex.Step(r, 0.8, 1.0, 0.05), dist = v * v / (2 * mu * G);
                    return Ex.Num(L.F("Um esportivo freia de {0} km/h até parar com μ = {1}. Qual a distância de frenagem, em m? (g = 9,8 m/s²)", N(kmh), N(mu)), dist, "m",
                        L.F("d = v² / (2·μ·g) = {0}² / (2·{1}·9,8) = {2} m.", N(v), N(mu), N(dist)));
                }
            case Difficulty.Medium:
            {
                double eff = Ex.Step(r, 0.6, 0.8, 0.05), v = Ms(100), t = 0.5 * m * v * v / (P * eff);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Estime o tempo de 0 a 100 km/h de um carro de {0} kg e {1} kW, supondo que em média {2}% da potência vira aceleração (sem tração limitada). Resposta em s.", N(m), N(P / 1000), N(eff * 100, 0)), t, "s",
                        L.F("E = ½·m·v² = ½·{0}·{1}² = {2} kJ; t = E / (η·P) = {2}·1000 / ({3}·{4}) = {5} s.", N(m), N(v), N(0.5 * m * v * v / 1000), N(eff), N(P), N(t)));
                double tt = Ex.Step(r, 4, 9, 0.5), need = 0.5 * m * v * v / (tt * eff);
                return Ex.Num(L.F("Que potência, em kW, um carro de {0} kg precisa ter para ir de 0 a 100 km/h em {1} s, se em média {2}% da potência vira aceleração?", N(m), N(tt), N(eff * 100, 0)), need / 1000, "kW",
                    L.F("P = ½·m·v² / (t·η) = ½·{0}·{1}² / ({2}·{3}) = {4} kW.", N(m), N(v), N(tt), N(eff), N(need / 1000)));
            }
            case Difficulty.Hard:
            {
                double vt = TopSpeed(P * 0.9, cd, A, m, 0.012);
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um esportivo de {0} kg (Cd = {1}, área {2} m², Crr = 0,012) tem {3} kW no motor e 90% de rendimento da transmissão. Qual sua velocidade máxima, em km/h, se o giro não limita? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", N(m), N(cd), N(A), N(P / 1000)), Kmh(vt), "km/h",
                        L.F("P_rodas = 0,9·{0} = {1} kW; (½·ρ·Cd·A·v² + Crr·m·g)·v = P_rodas → v = {2} m/s = {3} km/h.", N(P / 1000), N(0.9 * P / 1000), N(vt), N(Kmh(vt))), 3);
                    default:
                    {
                        double mu = Ex.Step(r, 0.9, 1.1, 0.05), v0 = vt, dist = v0 * v0 / (2 * mu * G);
                        return Ex.Num(L.F("O mesmo tipo de esportivo (Cd = {0}, área {1} m², {2} kW, 90% de rendimento, {3} kg, Crr = 0,012) freia da velocidade máxima até parar, com μ = {4}. Qual a distância, em m? (ρ = 1,2 kg/m³, g = 9,8 m/s²; despreze o ar na frenagem)", N(cd), N(A), N(P / 1000), N(m), N(mu)), dist, "m",
                            L.F("v_máx = {0} m/s; d = v² / (2·μ·g) = {0}² / (2·{1}·9,8) = {2} m.", N(v0), N(mu), N(dist)), 3);
                    }
                }
            }
            default:
            {
                double eff = 0.7, v = Ms(100), t100 = 0.5 * m * v * v / (P * eff), vt = TopSpeed(P * 0.9, cd, A, m, 0.012);
                double mu = Ex.Step(r, 0.9, 1.1, 0.05), dBrake = v * v / (2 * mu * G);
                double fuelEta = 0.3, cons = (0.5 * Rho * cd * A * Ms(120) * Ms(120) + 0.012 * m * G) * 1000 / 0.9 / fuelEta / 3.6e6 / 8.9 * 100;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Esportivo: {0} kg, {1} kW (90% de rendimento da transmissão), Cd = {2}, área {3} m², Crr = 0,012. Quantos segundos ele ganha na aceleração 0–100 km/h (70% da potência vira aceleração) se a massa cair 10%? (a potência não muda)", N(m), N(P / 1000), N(cd), N(A)), t100 * 0.1, "s",
                            L.F("t ∝ m: t = {0} s; com 0,9·m: {1} s; ganho: {2} s.", N(t100), N(t100 * 0.9), N(t100 * 0.1)), 3);
                    case 1: return Ex.Num(L.F("Esportivo: {0} kg, Cd = {1}, área {2} m², Crr = 0,012, motor com 30% de eficiência e transmissão de 90%, gasolina a 8,9 kWh/L. Qual o consumo a 120 km/h, em L/100 km? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", N(m), N(cd), N(A)), cons, "L/100 km",
                        L.F("F = ½·ρ·Cd·A·v² + Crr·m·g = {0} N; energia por 100 km no combustível = F·100 000 / (0,9·0,3) = {1} kWh; litros = {1}/8,9 = {2} L.", N(0.5 * Rho * cd * A * Ms(120) * Ms(120) + 0.012 * m * G), N(cons * 8.9), N(cons)), 3);
                    default:
                    {
                        double perfect = Ms(100) / Math.Sqrt(2 * mu * G * dBrake) ;
                        double dist = dBrake + v * 1.0;
                        return Ex.Num(L.F("Esportivo de {0} kg com pneus μ = {1}, freando de 100 km/h. O motorista demora 1 s para reagir. Qual a distância total para parar, em m? (g = 9,8 m/s²)", N(m), N(mu)), dist, "m",
                            L.F("Reação: 27,8 m/s · 1 s = {0} m; frenagem: v²/(2·μ·g) = {1} m; total: {2} m (a massa não importa).", N(v), N(dBrake), N(dist)), 3);
                    }
                }
            }
        }
    }
}
