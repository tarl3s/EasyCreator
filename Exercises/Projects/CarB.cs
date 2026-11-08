using static EasyCreator.Exercises.Pv;

namespace EasyCreator.Exercises;

/// <summary>Cars 6-10: corners and rollover, aerodynamics and fuel use, suspension, electric cars, the Formula SAE lap.</summary>
[Exercises("car-06-curvas")]
public sealed class Car06 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double rad = Ex.Step(r, 20, 150, 5), mu = Ex.Step(r, 0.5, 1.1, 0.05), m = Ex.Step(r, 800, 1800, 50);
        switch (d)
        {
            case Difficulty.Easy:
            {
                double kmh = Ex.Step(r, 30, 90, 5), v = Ms(kmh), ac = v * v / rad;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um carro faz uma curva de raio {0} m a {1} km/h. Qual a aceleração lateral, em m/s²?", N(rad), N(kmh)), ac, "m/s²",
                        L.F("v = {0} m/s; a = v² / r = {0}² / {1} = {2} m/s².", N(v), N(rad), N(ac)));
                    case 1: return Ex.Num(L.F("Um carro de {0} kg faz uma curva de raio {1} m a {2} km/h. Que força lateral, em N, os pneus precisam fazer?", N(m), N(rad), N(kmh)), m * ac, "N",
                        L.F("F = m·v²/r = {0}·{1}² / {2} = {3} N.", N(m), N(v), N(rad), N(m * ac)));
                    default: return Ex.Num(L.F("Um carro mantém {0} m/s² de aceleração lateral numa curva de raio {1} m. A que velocidade, em km/h, ele está?", N(ac), N(rad)), kmh, "km/h",
                        L.F("v = √(a·r) = √({0}·{1}) = {2} m/s = {3} km/h.", N(ac), N(rad), N(v), N(kmh)));
                }
            }
            case Difficulty.Medium:
            {
                double vmax = Math.Sqrt(mu * G * rad);
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Numa curva plana de raio {0} m, com μ = {1} entre pneu e pista, qual a velocidade máxima sem derrapar, em km/h? (g = 9,8 m/s²)", N(rad), N(mu)), Kmh(vmax), "km/h",
                        L.F("v = √(μ·g·r) = √({0}·9,8·{1}) = {2} m/s = {3} km/h.", N(mu), N(rad), N(vmax), N(Kmh(vmax))));
                    case 1: return Ex.Num(L.F("Para fazer uma curva a {0} km/h sem derrapar, com μ = {1}, qual o menor raio possível, em metros? (g = 9,8 m/s²)", N(Kmh(vmax)), N(mu)), rad, "m",
                        L.F("r = v² / (μ·g) = {0}² / ({1}·9,8) = {2} m.", N(vmax), N(mu), N(rad)));
                    default: return Ex.Num(L.F("Um carro faz uma curva de raio {0} m a {1} km/h sem derrapar, no limite. Qual o coeficiente de atrito mínimo necessário entre pneu e pista? (g = 9,8 m/s²)", N(rad), N(Kmh(vmax))), mu, "",
                        L.F("μ = v² / (g·r) = {0}² / (9,8·{1}) = {2}.", N(vmax), N(rad), N(mu)));
                }
            }
            case Difficulty.Hard:
            {
                double tw = Ex.Step(r, 1.3, 1.7, 0.05), h = Ex.Step(r, 0.45, 0.8, 0.05), aRoll = G * tw / (2 * h), vRoll = Math.Sqrt(aRoll * rad), vSkid = Math.Sqrt(mu * G * rad);
                string intro = L.F("Um veículo tem bitola de {0} m e centro de gravidade a {1} m de altura. A curva tem raio {2} m e μ = {3} (g = 9,8 m/s²).", N(tw), N(h), N(rad), N(mu));
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(intro + " " + L.T("Qual a aceleração lateral que faz o veículo começar a capotar, em m/s²?"), aRoll, "m/s²",
                        L.F("a_capota = g·bitola / (2·h) = 9,8·{0} / (2·{1}) = {2} m/s².", N(tw), N(h), N(aRoll)));
                    case 1: return Ex.Num(intro + " " + L.T("Qual a velocidade, em km/h, em que ele começaria a capotar?"), Kmh(vRoll), "km/h",
                        L.F("a = {0} m/s²; v = √(a·r) = √({0}·{1}) = {2} m/s = {3} km/h.", N(aRoll), N(rad), N(vRoll), N(Kmh(vRoll))));
                    default: return Ex.Num(intro + " " + L.T("Qual a velocidade, em km/h, em que ele realmente perde o controle — o menor valor entre derrapar e capotar?"), Kmh(Math.Min(vRoll, vSkid)), "km/h",
                        L.F("Derrapa a {0} km/h e capota a {1} km/h. O que acontece primeiro é o menor: {2} km/h ({3}).", N(Kmh(vSkid)), N(Kmh(vRoll)), N(Kmh(Math.Min(vRoll, vSkid))), vSkid < vRoll ? L.T("derrapagem") : L.T("capotamento")));
                }
            }
            default:
            {
                double th = Ex.Step(r, 4, 20, 1), t = Math.Tan(th * Math.PI / 180), rb = Ex.Step(r, 50, 150, 10);
                if (r.Next(2) == 0)
                {
                    double v = Math.Sqrt(G * rb * (t + mu) / (1 - mu * t));
                    return Ex.Num(L.F("Uma pista tem curva de raio {0} m com inclinação lateral (sobrelevação) de {1}° e μ = {2} entre pneu e asfalto. Qual a velocidade máxima sem derrapar para fora, em km/h? (g = 9,8 m/s²)", N(rb), N(th), N(mu)), Kmh(v), "km/h",
                        L.F("Na pista inclinada a normal ajuda: v² = g·r·(tg θ + μ) / (1 − μ·tg θ). Com tg θ = {0}: v = {1} m/s = {2} km/h.", N(t, 3), N(v), N(Kmh(v))), 3);
                }
                double kmh = Ex.Step(r, 60, 120, 5), vv = Ms(kmh), ang = Math.Atan(vv * vv / (G * rb)) * 180 / Math.PI;
                return Ex.Num(L.F("Em quanto graus deve ser inclinada uma curva de raio {0} m para que um carro a {1} km/h a faça SEM depender do atrito (μ = 0)? (g = 9,8 m/s²)", N(rb), N(kmh)), ang, "°",
                    L.F("Só a normal faz a força centrípeta: tg θ = v² / (g·r) = {0}² / (9,8·{1}) = {2} → θ = {3}°.", N(vv), N(rb), N(vv * vv / (G * rb), 3), N(ang)), 3);
            }
        }
    }
}

[Exercises("car-07-aerodinamica")]
public sealed class Car07 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G, Rho = Pk.Rho;
        double cd = Ex.Step(r, 0.25, 0.45, 0.01), A = Ex.Step(r, 1.8, 2.8, 0.1), kmh = Ex.Step(r, 60, 160, 10), v = Ms(kmh);
        double Fd = 0.5 * Rho * cd * A * v * v;
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um carro com Cd = {0} e área frontal de {1} m² anda a {2} km/h. Qual a força de arrasto do ar, em N? (ρ = 1,2 kg/m³)", N(cd), N(A), N(kmh)), Fd, "N",
                        L.F("v = {0} m/s; F = ½·ρ·Cd·A·v² = ½·1,2·{1}·{2}·{0}² = {3} N.", N(v), N(cd), N(A), N(Fd)));
                return Ex.Num(L.F("Um carro com Cd = {0} e área frontal de {1} m² anda a {2} km/h. Que potência, em kW, só o arrasto do ar consome? (ρ = 1,2 kg/m³)", N(cd), N(A), N(kmh)), Fd * v / 1000, "kW",
                    L.F("F = {0} N; P = F·v = {0}·{1} = {2} kW.", N(Fd), N(v), N(Fd * v / 1000)));
            case Difficulty.Medium:
            {
                double k = Ex.Pick(r, 1.25, 1.5, 2.0);
                string intro = L.F("Um carro gasta {0} kW só para vencer o ar a {1} km/h.", N(Fd * v / 1000), N(kmh));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.F("Se a velocidade for multiplicada por {0}, quantos kW o ar consumirá?", N(k)), Fd * v * Math.Pow(k, 3) / 1000, "kW",
                        L.F("P ∝ v³: {0} · {1}³ = {2} kW.", N(Fd * v / 1000), N(k), N(Fd * v * Math.Pow(k, 3) / 1000)));
                return Ex.Num(intro + " " + L.F("Se a velocidade for multiplicada por {0}, quanto vale a força de arrasto, em N, sabendo que a atual é {1} N?", N(k), N(Fd)), Fd * k * k, "N",
                    L.F("F ∝ v²: {0} · {1}² = {2} N.", N(Fd), N(k), N(Fd * k * k)));
            }
            case Difficulty.Hard:
            {
                double eta = Ex.Pick(r, 0.25, 0.28, 0.30, 0.33), m = Ex.Step(r, 1000, 1600, 100), crr = 0.012, Ftot = Fd + crr * m * G;
                double lkm = Ftot * 1000 / eta / 3.6e6 / 8.9 * 100;
                string intro = L.F("Um carro de {0} kg (Cd = {1}, área {2} m², Crr = 0,012) anda a {3} km/h constantes numa reta plana. O motor tem eficiência de {4}% e a gasolina tem 8,9 kWh por litro. Use ρ = 1,2 kg/m³ e g = 9,8 m/s².", N(m), N(cd), N(A), N(kmh), N(eta * 100, 0));
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(intro + " " + L.T("Quanta energia mecânica, em kWh, as rodas gastam por quilômetro?"), Ftot * 1000 / 3.6e6, "kWh",
                        L.F("F = {0} + {1} = {2} N; E = F·d = {2}·1000 / 3,6·10⁶ = {3} kWh.", N(Fd), N(crr * m * G), N(Ftot), N(Ftot * 1000 / 3.6e6, 4)), 3);
                    default: return Ex.Num(intro + " " + L.T("Qual o consumo, em litros por 100 km?"), lkm, "L/100 km",
                        L.F("Energia nas rodas por km: {0} kWh. No motor: {0}/{1} = {2} kWh. Litros: {2}/8,9 por km → {3} L/100 km.", N(Ftot * 1000 / 3.6e6, 4), N(eta), N(Ftot * 1000 / 3.6e6 / eta, 4), N(lkm)), 3);
                }
            }
            default:
            {
                double eta = 0.3, m = Ex.Step(r, 1100, 1500, 100), crr = 0.012, v1 = Ms(Ex.Step(r, 80, 100, 10)), v2 = Ms(Ex.Step(r, 120, 150, 10));
                Func<double, double> cons = vv => (0.5 * Rho * cd * A * vv * vv + crr * m * G) * 1000 / eta / 3.6e6 / 8.9 * 100;
                double c1 = cons(v1), c2 = cons(v2);
                string intro = L.F("Um carro de {0} kg (Cd = {1}, área {2} m², Crr = 0,012, motor com 30% de eficiência, gasolina com 8,9 kWh/L) viaja a {3} km/h ou a {4} km/h (ρ = 1,2 kg/m³, g = 9,8 m/s²).", N(m), N(cd), N(A), N(Kmh(v1)), N(Kmh(v2)));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("De quantos por cento o consumo (L/100 km) aumenta na velocidade maior?"), (c2 / c1 - 1) * 100, "%",
                        L.F("Consumo = (arrasto + rolamento)·1000 / (η·8,9 kWh) → {0} L/100 km na menor e {1} L/100 km na maior. Aumento: {2}%. O ar cresce com v², o rolamento não.", N(c1), N(c2), N((c2 / c1 - 1) * 100)), 3);
                double dist = Ex.Step(r, 200, 600, 50), save = (c2 - c1) * dist / 100;
                return Ex.Num(intro + " " + L.F("Numa viagem de {0} km, quantos litros a menos ele gasta indo na velocidade menor?", N(dist)), save, "L",
                    L.F("Consumos: {0} e {1} L/100 km. Diferença em {2} km: ({1} − {0})·{2}/100 = {3} L.", N(c1), N(c2), N(dist), N(save)), 3);
            }
        }
    }
}

[Exercises("car-08-suspensao")]
public sealed class Car08 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double m = Ex.Step(r, 250, 450, 10), k = Ex.Step(r, 15, 40, 1) * 1000, f = Math.Sqrt(k / m) / (2 * Math.PI);
        switch (d)
        {
            case Difficulty.Easy:
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Uma roda sustenta {0} kg com uma mola de {1} N/m. Qual a frequência natural, em Hz?", N(m), N(k)), f, "Hz",
                        L.F("f = (1/2π)·√(k/m) = (1/2π)·√({0}/{1}) = {2} Hz.", N(k), N(m), N(f)));
                    case 1: return Ex.Num(L.F("Uma suspensão com frequência natural de {0} Hz sustenta {1} kg por roda. Qual a rigidez da mola, em N/m?", N(f, 3), N(m)), k, "N/m",
                        L.F("k = m·(2π·f)² = {0}·(2π·{1})² = {2} N/m.", N(m), N(f, 3), N(k)));
                    default: return Ex.Num(L.F("Uma mola de {0} N/m deve dar frequência natural de {1} Hz. Que massa por roda ela pode sustentar, em kg?", N(k), N(f, 3)), m, "kg",
                        L.F("m = k / (2π·f)² = {0} / (2π·{1})² = {2} kg.", N(k), N(f, 3), N(m)));
                }
            case Difficulty.Medium:
            {
                double M = Ex.Step(r, 1000, 1800, 50), sprung = M * 0.85 / 4;
                double fr = Ex.Step(r, 1.0, 2.0, 0.1), kk = sprung * Math.Pow(2 * Math.PI * fr, 2), x = sprung * G / kk;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um carro de {0} kg tem 85% da massa suspensa, repartida igualmente nas 4 rodas. Qual a rigidez de cada mola, em N/m, para uma frequência de {1} Hz?", N(M), N(fr)), kk, "N/m",
                        L.F("m_roda = 0,85·{0}/4 = {1} kg; k = m·(2π·f)² = {1}·(2π·{2})² = {3} N/m.", N(M), N(sprung), N(fr), N(kk)), 3);
                return Ex.Num(L.F("Uma roda suporta {0} kg numa mola de {1} N/m. De quantos milímetros a mola comprime só com esse peso? (g = 9,8 m/s²)", N(sprung), N(kk)), x * 1000, "mm",
                    L.F("x = m·g / k = {0}·9,8 / {1} = {2} mm.", N(sprung), N(kk), N(x * 1000)), 3);
            }
            case Difficulty.Hard:
            {
                double c = Ex.Step(r, 1000, 2500, 100), zeta = c / (2 * Math.Sqrt(k * m)), extra = Ex.Step(r, 60, 200, 10);
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um amortecedor tem c = {0} N·s/m numa roda de {1} kg com mola de {2} N/m. Qual a razão de amortecimento ζ = c / (2·√(k·m))?", N(c), N(m), N(k)), zeta, "",
                        L.F("ζ = {0} / (2·√({1}·{2})) = {3}.", N(c), N(k), N(m), N(zeta, 3)), 3);
                    case 1:
                    {
                        double f2 = f * Math.Sqrt(m / (m + extra));
                        return Ex.Num(L.F("A frequência natural de uma roda é {0} Hz com {1} kg. Quando entram passageiros e bagagem, a roda passa a sustentar mais {2} kg. Qual a nova frequência, em Hz?", N(f, 3), N(m), N(extra)), f2, "Hz",
                            L.F("f ∝ 1/√m: f₂ = {0}·√({1}/{2}) = {3} Hz.", N(f, 3), N(m), N(m + extra), N(f2, 3)), 3);
                    }
                    default:
                    {
                        double cNeed = 2 * 0.3 * Math.Sqrt(k * m);
                        return Ex.Num(L.F("Que amortecimento c (N·s/m) dá ζ = 0,3 a uma roda de {0} kg com mola de {1} N/m?", N(m), N(k)), cNeed, "N·s/m",
                            L.F("c = 2·ζ·√(k·m) = 2·0,3·√({0}·{1}) = {2} N·s/m.", N(k), N(m), N(cNeed)), 3);
                    }
                }
            }
            default:
            {
                double wl = Ex.Step(r, 4, 12, 1);
                switch (r.Next(2))
                {
                    case 0:
                    {
                        double vres = wl * f * 3.6;
                        return Ex.Num(L.F("Uma estrada de terra tem ondulações (costelas) espaçadas de {0} m. A roda de {1} kg com mola de {2} N/m ressoa quando as ondulações a atingem na sua frequência natural. A que velocidade, em km/h, ocorre a ressonância?", N(wl), N(m), N(k)), vres, "km/h",
                            L.F("f = {0} Hz; frequência de impacto = v / λ = f → v = λ·f = {1}·{0} = {2} m/s = {3} km/h.", N(f, 3), N(wl), N(wl * f), N(vres)), 3);
                    }
                    default:
                    {
                        double M = Ex.Step(r, 1000, 1800, 50), sprung = M * 0.85 / 4, kk = sprung * Math.Pow(2 * Math.PI * f, 2), dz = Ex.Step(r, 0.6, 0.8, 0.05);
                        double need = 2 * dz * Math.Sqrt(kk * sprung);
                        return Ex.Num(L.F("Num carro de {0} kg (85% da massa suspensa, igualmente nas 4 rodas) a suspensão tem frequência de {1} Hz e deve ter ζ = {2}. Qual o amortecimento c necessário em cada roda, em N·s/m?", N(M), N(f, 3), N(dz)), need, "N·s/m",
                            L.F("m = {0} kg; k = m·(2π·f)² = {1} N/m; c = 2·ζ·√(k·m) = {2} N·s/m.", N(sprung), N(kk), N(need)), 3);
                    }
                }
            }
        }
    }
}

[Exercises("car-09-eletrico")]
public sealed class Car09 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G, Rho = Pk.Rho;
        double cap = Ex.Step(r, 30, 100, 5), cons = Ex.Step(r, 13, 24, 0.5), range = cap / cons * 100;
        switch (d)
        {
            case Difficulty.Easy:
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um carro elétrico tem bateria de {0} kWh e consome {1} kWh/100 km. Qual a autonomia, em km?", N(cap), N(cons)), range, "km",
                        L.F("Autonomia = capacidade / consumo × 100 = {0} / {1} × 100 = {2} km.", N(cap), N(cons), N(range)));
                    case 1: return Ex.Num(L.F("Um elétrico faz {0} km com uma bateria de {1} kWh, descarregando-a por completo. Qual o consumo médio, em kWh/100 km?", N(range), N(cap)), cons, "kWh/100 km",
                        L.F("{0} / {1} × 100 = {2} kWh/100 km.", N(cap), N(range), N(cons)));
                    default: return Ex.Num(L.F("Um elétrico consome {0} kWh/100 km. Que energia, em kWh, ele gasta numa viagem de {1} km?", N(cons), N(range)), cap, "kWh",
                        L.F("{0} × {1} / 100 = {2} kWh.", N(cons), N(range), N(cap)));
                }
            case Difficulty.Medium:
            {
                double kmh = Ex.Step(r, 50, 130, 10), v = Ms(kmh), Pw = Ex.Step(r, 8, 25, 1) * 1000, eta = Ex.Pick(r, 0.85, 0.88, 0.9);
                double consumption = Pw / eta / v * 1000 / 3.6e6 * 100;
                string intro = L.F("Um carro elétrico precisa de {0} kW nas rodas para andar a {1} km/h constantes. O conjunto motor e inversor tem rendimento de {2}%.", N(Pw / 1000), N(kmh), N(eta * 100, 0));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Quanta potência, em kW, sai da bateria?"), Pw / eta / 1000, "kW",
                        L.F("P_bateria = P_rodas / η = {0} / {1} = {2} kW.", N(Pw / 1000), N(eta), N(Pw / eta / 1000)));
                return Ex.Num(intro + " " + L.T("Qual o consumo, em kWh/100 km?"), consumption, "kWh/100 km",
                    L.F("P_bateria = {0} kW; tempo para 100 km = 100/{1} h; E = {0}·100/{1} = {2} kWh.", N(Pw / eta / 1000), N(kmh), N(consumption)));
            }
            case Difficulty.Hard:
            {
                double usable = Ex.Pick(r, 0.8, 0.85, 0.9), charger = Ex.Pick(r, 7.0, 11, 22, 50, 100), ce = Ex.Pick(r, 0.88, 0.9, 0.92), price = Ex.Step(r, 0.6, 1.4, 0.05);
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um elétrico de {0} kWh só usa {1}% da bateria (o resto é reserva). Consumindo {2} kWh/100 km, qual a autonomia útil, em km?", N(cap), N(usable * 100, 0), N(cons)), cap * usable / cons * 100, "km",
                        L.F("{0} × {1} / {2} × 100 = {3} km.", N(cap), N(usable), N(cons), N(cap * usable / cons * 100)));
                    case 1:
                    {
                        double tch = cap * 0.7 / (charger * ce);
                        return Ex.Num(L.F("Quantas horas leva para carregar de 10% a 80% uma bateria de {0} kWh com um carregador de {1} kW, de rendimento {2}? (potência constante)", N(cap), N(charger), N(ce)), tch, "h",
                            L.F("Energia na bateria: 0,7·{0} = {1} kWh; da rede: {1}/{2}; tempo = {1} / ({3}·{2}) = {4} h.", N(cap), N(cap * 0.7), N(ce), N(charger), N(tch)), 3);
                    }
                    default:
                    {
                        double kmT = Ex.Step(r, 100, 400, 50), cost = kmT * cons / 100 / ce * price;
                        return Ex.Num(L.F("Qual o custo de uma viagem de {0} km num elétrico que consome {1} kWh/100 km, carregado com rendimento de {2} e energia a R$ {3} por kWh?", N(kmT), N(cons), N(ce), N(price)), cost, "",
                            L.F("Energia da bateria: {0}·{1}/100 = {2} kWh; da rede: {2}/{3} = {4} kWh; custo: {4}·{5} = {6}.", N(kmT), N(cons), N(kmT * cons / 100), N(ce), N(kmT * cons / 100 / ce), N(price), N(cost)), 3);
                    }
                }
            }
            default:
            {
                double m = Ex.Step(r, 1400, 2200, 100), cd = Ex.Step(r, 0.23, 0.30, 0.01), A = Ex.Step(r, 2.1, 2.6, 0.1), crr = 0.010, eta = 0.88;
                double kmh = Ex.Step(r, 90, 130, 10), v = Ms(kmh), Fw = 0.5 * Rho * cd * A * v * v + crr * m * G;
                double cc = Fw / eta * 1000 / 3.6e6 * 100;
                string intro = L.F("Um elétrico de {0} kg (Cd = {1}, área {2} m², Crr = 0,010, rendimento bateria–roda {3}) viaja a {4} km/h em reta plana. Use ρ = 1,2 kg/m³ e g = 9,8 m/s².", N(m), N(cd), N(A), N(eta), N(kmh));
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Qual o consumo, em kWh/100 km?"), cc, "kWh/100 km",
                        L.F("Força nas rodas = ar + rolamento = {0} N. E/100 km = F·100 000 m / η = {1} kWh.", N(Fw), N(cc)), 3);
                double capacity = Ex.Step(r, 50, 90, 5);
                return Ex.Num(intro + " " + L.F("Com bateria de {0} kWh, qual a autonomia, em km, nessa velocidade (usando 100% da carga)?", N(capacity)), capacity / cc * 100, "km",
                    L.F("Consumo = {0} kWh/100 km → autonomia = {1}/{0}·100 = {2} km.", N(cc), N(capacity), N(capacity / cc * 100)), 3);
            }
        }
    }
}

[Exercises("car-10-projeto-fsae")]
public sealed class Car10 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G, Rho = Pk.Rho;
        double mu = Ex.Step(r, 1.2, 1.8, 0.1), m = Ex.Step(r, 250, 330, 10), rad = Ex.Step(r, 8, 20, 1);
        switch (d)
        {
            case Difficulty.Easy:
            {
                double len = Ex.Step(r, 600, 1200, 50), t = Ex.Step(r, 40, 80, 2), vavg = len / t;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um Fórmula SAE completa uma volta de {0} m em {1} s. Qual a velocidade média, em km/h?", N(len), N(t)), Kmh(vavg), "km/h",
                        L.F("v = {0} / {1} = {2} m/s = {3} km/h.", N(len), N(t), N(vavg), N(Kmh(vavg))));
                    case 1: return Ex.Num(L.F("Um Fórmula SAE faz uma pista de {0} m a velocidade média de {1} km/h. Quanto tempo leva uma volta, em s?", N(len), N(Kmh(vavg))), t, "s",
                        L.F("t = {0} / {1} = {2} s.", N(len), N(vavg), N(t)));
                    default:
                    {
                        double laps = Ex.Int(r, 8, 22);
                        return Ex.Num(L.F("Numa prova de resistência de {0} voltas de {1} m, a média é de {2} s por volta. Quantos minutos dura a prova?", N(laps, 0), N(len), N(t)), laps * t / 60, "min",
                            L.F("{0} × {1} / 60 = {2} min.", N(laps, 0), N(t), N(laps * t / 60)));
                    }
                }
            }
            case Difficulty.Medium:
            {
                double vc = Math.Sqrt(mu * G * rad);
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Num carro de Fórmula SAE sem aerodinâmica, os pneus dão μ = {0}. Qual a velocidade máxima, em km/h, numa curva de raio {1} m? (g = 9,8 m/s²)", N(mu), N(rad)), Kmh(vc), "km/h",
                        L.F("v = √(μ·g·r) = √({0}·9,8·{1}) = {2} m/s = {3} km/h.", N(mu), N(rad), N(vc), N(Kmh(vc))));
                    case 1:
                    {
                        double skid = Ex.Pick(r, 15.25, 18.25), tskid = 2 * Math.PI * (skid / 2 + 1.5) / Math.Sqrt(mu * G * (skid / 2 + 1.5)) / 1;
                        double rr = skid / 2 + 1.5;
                        return Ex.Num(L.F("Na prova do skidpad o carro percorre um círculo de raio {0} m no limite de aderência (μ = {1}, g = 9,8 m/s²). Qual o tempo de uma volta, em s?", N(rr), N(mu)), tskid, "s",
                            L.F("v = √(μ·g·r) = {0} m/s; t = 2π·r / v = 2π·{1} / {0} = {2} s.", N(Math.Sqrt(mu * G * rr)), N(rr), N(tskid)));
                    }
                    default:
                    {
                        double a = mu * G, v0 = Ms(Ex.Step(r, 40, 70, 5)), dist = v0 * v0 / (2 * a);
                        return Ex.Num(L.F("Um carro de Fórmula SAE freia no limite dos pneus (μ = {0}, g = 9,8 m/s², sem aerodinâmica) de {1} km/h até parar. Qual a distância, em metros?", N(mu), N(Kmh(v0))), dist, "m",
                            L.F("a = μ·g = {0} m/s²; d = v²/(2a) = {1}² / (2·{0}) = {2} m.", N(a), N(v0), N(dist)));
                    }
                }
            }
            case Difficulty.Hard:
            {
                double clA = Ex.Step(r, 1.5, 3.5, 0.25), kmh = Ex.Step(r, 60, 110, 10), v = Ms(kmh);
                double down = 0.5 * Rho * clA * v * v;
                double den = m / rad - mu * Rho * clA / 2;
                double vc = den > 0 ? Math.Sqrt(mu * m * G / den) : double.PositiveInfinity;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um carro de {0} kg tem asas com Cl·A = {1} m². Qual a força de downforce a {2} km/h, em N? (ρ = 1,2 kg/m³)", N(m), N(clA), N(kmh)), down, "N",
                        L.F("F = ½·ρ·ClA·v² = ½·1,2·{0}·{1}² = {2} N.", N(clA), N(v), N(down)));
                    default:
                        if (double.IsFinite(vc))
                            return Ex.Num(L.F("Um carro de {0} kg, com pneus μ = {1} e asas Cl·A = {2} m², faz uma curva de raio {3} m. Qual a velocidade máxima, em km/h, contando o downforce? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", N(m), N(mu), N(clA), N(rad)), Kmh(vc), "km/h",
                                L.F("m·v²/r = μ·(m·g + ½·ρ·ClA·v²) → v² = μ·m·g / (m/r − μ·ρ·ClA/2) = {0} → v = {1} m/s = {2} km/h.", N(vc * vc), N(vc), N(Kmh(vc))), 3);
                        return Ex.Num(L.F("Um carro de {0} kg tem asas com Cl·A = {1} m². Qual a downforce a {2} km/h, em N? (ρ = 1,2 kg/m³)", N(m), N(clA), N(kmh)), down, "N",
                            L.F("F = ½·ρ·ClA·v² = ½·1,2·{0}·{1}² = {2} N.", N(clA), N(v), N(down)));
                }
            }
            default:
            {
                double clA = Ex.Step(r, 1.0, 2.2, 0.1), rr = Ex.Step(r, 10, 20, 1);
                double den = m / rr - mu * Rho * clA / 2;
                if (den <= 0) { clA = 1.0; den = m / rr - mu * Rho * clA / 2; }
                double v2 = mu * m * G / den, vNo = Math.Sqrt(mu * G * rr), ratio = Math.Sqrt(v2) / vNo;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um carro de {0} kg, μ = {1}, faz uma curva de raio {2} m. Com asas de Cl·A = {3} m², de quantos por cento a velocidade máxima da curva aumenta em relação ao carro sem asas? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", N(m), N(mu), N(rr), N(clA)), (ratio - 1) * 100, "%",
                        L.F("Sem asas: v = √(μ·g·r) = {0} m/s. Com asas: v² = μ·m·g / (m/r − μ·ρ·ClA/2) → v = {1} m/s. Aumento: {2}%.", N(vNo), N(Math.Sqrt(v2)), N((ratio - 1) * 100)), 3);
                double vs = Math.Sqrt(v2), tlap = 2 * Math.PI * rr / vs;
                return Ex.Num(L.F("Num skidpad de raio {0} m, um carro de {1} kg com μ = {2} e Cl·A = {3} m² faz o círculo no limite (a downforce cresce com a velocidade). Qual o tempo de uma volta, em s? (ρ = 1,2 kg/m³, g = 9,8 m/s²)", N(rr), N(m), N(mu), N(clA)), tlap, "s",
                    L.F("v² = μ·m·g / (m/r − μ·ρ·ClA/2) = {0} → v = {1} m/s; t = 2π·r / v = {2} s.", N(v2), N(vs), N(tlap)), 3);
            }
        }
    }
}
