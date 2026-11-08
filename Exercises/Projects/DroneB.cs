using static EasyCreator.Exercises.Pv;

namespace EasyCreator.Exercises;

/// <summary>Drones 6-10: PID altitude, attitude, GPS navigation, the rules, the delivery drone.</summary>
[Exercises("drone-06-pid-altura")]
public sealed class Drone06 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double kp = Ex.Step(r, 0.1, 2.0, 0.05), ki = Ex.Step(r, 0.01, 0.5, 0.01), kd = Ex.Step(r, 0.05, 1.0, 0.05), e = Ex.Step(r, -3, 3, 0.1), integ = Ex.Step(r, -2, 2, 0.1), de = Ex.Step(r, -2, 2, 0.1);
        switch (d)
        {
            case Difficulty.Easy:
            {
                double target = Ex.Step(r, 2, 30, 1), alt = Ex.Step(r, 0, 30, 1), er = target - alt;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("O alvo de altura é {0} m e o drone está a {1} m. Qual o erro e = alvo − medido, em m?", N(target), N(alt)), er, "m",
                        L.F("e = {0} − {1} = {2} m.", N(target), N(alt), N(er)));
                return Ex.Num(L.F("Num controlador P com Kp = {0}, o erro de altura é {1} m. Qual a saída (Kp·e)?", N(kp), N(e)), kp * e, "",
                    L.F("saída = Kp·e = {0}·{1} = {2}.", N(kp), N(e), N(kp * e, 3)), 3);
            }
            case Difficulty.Medium:
            {
                double out_ = kp * e + ki * integ + kd * de;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("PID com Kp = {0}, Ki = {1}, Kd = {2}. Erro atual {3} m, erro acumulado (integral) {4} m·s e derivada do erro {5} m/s. Qual a saída?", N(kp), N(ki), N(kd), N(e), N(integ), N(de)), out_, "",
                        L.F("saída = Kp·e + Ki·∫e + Kd·de/dt = {0}·{1} + {2}·{3} + {4}·{5} = {6}.", N(kp), N(e), N(ki), N(integ), N(kd), N(de), N(out_, 3)), 3);
                double e1 = Ex.Step(r, -2, 2, 0.1), e2 = e1 + Ex.Step(r, -1, 1, 0.1), dt = Ex.Pick(r, 0.01, 0.02, 0.05, 0.1), der = (e2 - e1) / dt;
                return Ex.Num(L.F("O erro de altura passou de {0} m para {1} m em {2} s. Qual a derivada do erro, em m/s?", N(e1), N(e2), N(dt)), der, "m/s",
                    L.F("de/dt = ({1} − {0}) / {2} = {3} m/s.", N(e1), N(e2), N(dt), N(der)), 3);
            }
            case Difficulty.Hard:
            {
                double hov = Ex.Step(r, 0.3, 0.6, 0.01), ess = hov / kp;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um drone paira com {0}% do acelerador. Com controle só P (Kp = {1} por metro, saída em fração do acelerador), qual o erro de altura que sobra em regime, em m?", N(hov * 100, 0), N(kp)), ess, "m",
                        L.F("Em regime a saída precisa igualar o acelerador de pairar: Kp·e = {0} → e = {0}/{1} = {2} m (o drone fica abaixo do alvo).", N(hov), N(kp), N(ess)), 3);
                    default:
                    {
                        double emax = Ex.Step(r, 0.05, 0.3, 0.05), kpn = hov / emax;
                        return Ex.Num(L.F("Um drone paira com {0}% do acelerador. Que Kp (saída em fração do acelerador por metro) mantém o erro de regime só-P em no máximo {1} m?", N(hov * 100, 0), N(emax)), kpn, "",
                            L.F("Kp = acelerador de pairar / erro = {0} / {1} = {2}.", N(hov), N(emax), N(kpn)), 3);
                    }
                }
            }
            default:
            {
                double hov = Ex.Step(r, 0.3, 0.6, 0.01), er = Ex.Step(r, 0.2, 2, 0.1), tt = hov / (ki * er);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone paira com {0}% do acelerador e o erro de altura fica parado em {1} m (por exemplo, com carga nova). Só o termo integral (Ki = {2} por m·s) vai sozinho cobrir esse acelerador. Quanto tempo leva, em s, para Ki·∫e igualar o acelerador de pairar?", N(hov * 100, 0), N(er), N(ki)), tt, "s",
                        L.F("∫e = e·t → Ki·e·t = {0} → t = {0} / ({1}·{2}) = {3} s.", N(hov), N(ki), N(er), N(tt)), 3);
                double tw = Ex.Step(r, 1.6, 3.0, 0.1), m = Ex.Step(r, 0.6, 3, 0.1), K = 9.8 * tw, kpp = Ex.Step(r, 0.5, 4, 0.1), kdc = 2 * Math.Sqrt(kpp / K);
                return Ex.Num(L.F("Modelo simples da altura: o erro obedece e'' + (Kd·K)·e' + (Kp·K)·e = 0, com K = g·(T/W). Para T/W = {0} e Kp = {1}, que Kd dá amortecimento crítico (ζ = 1)? (g = 9,8 m/s²; ωn = √(Kp·K), ζ = Kd·K / (2·ωn))", N(tw), N(kpp)), kdc, "",
                    L.F("K = 9,8·{0} = {1}; ωn = √({2}·{1}) = {3}; Kd = 2·ωn / K = {4}.", N(tw), N(K), N(kpp), N(Math.Sqrt(kpp * K), 3), N(kdc, 3)), 3);
            }
        }
    }
}

[Exercises("drone-07-atitude")]
public sealed class Drone07 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double kp = Ex.Step(r, 0.5, 6, 0.1), err = Ex.Step(r, -30, 30, 1), dT = Ex.Step(r, 0.1, 2, 0.1), arm = Ex.Step(r, 0.08, 0.3, 0.01), m = Ex.Step(r, 0.5, 4, 0.1), La = Ex.Step(r, 0.12, 0.4, 0.01);
        double inertia = 0.4 * m * La * La;
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone inclinado {0}° quer ficar nivelado (alvo 0°). Qual o erro de rolagem, em graus?", N(err)), -err, "°",
                        L.F("e = alvo − medido = 0 − ({0}) = {1}°.", N(err), N(-err)));
                return Ex.Num(L.F("No PID de rolagem, Kp = {0} e o erro é {1}°. Qual a saída P, em unidades de controle?", N(kp), N(err)), kp * err, "",
                    L.F("P = Kp·e = {0}·{1} = {2}.", N(kp), N(err), N(kp * err)));
            case Difficulty.Medium:
            {
                double tau = 4 * dT * arm;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Num quadricóptero em X, para rolar, os 2 motores de um lado ganham ΔT = {0} N cada e os 2 do outro perdem ΔT cada. Cada motor está a {1} m do eixo de rolagem. Qual o torque total, em N·m?", N(dT), N(arm)), tau, "N·m",
                        L.F("τ = 4·ΔT·d = 4·{0}·{1} = {2} N·m.", N(dT), N(arm), N(tau, 3)), 3);
                return Ex.Num(L.F("O momento de inércia de um drone é aproximadamente I = 0,4·m·L². Para m = {0} kg e L = {1} m (braço), qual o I, em kg·m²?", N(m), N(La)), inertia, "kg·m²",
                    L.F("I = 0,4·{0}·{1}² = {2} kg·m².", N(m), N(La), N(inertia, 5)), 3);
            }
            case Difficulty.Hard:
            {
                double tau = 4 * dT * arm, alpha = tau / inertia;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um drone de {0} kg com braço de {1} m (I = 0,4·m·L²) recebe um torque de rolagem de {2} N·m. Qual a aceleração angular, em rad/s²?", N(m), N(La), N(tau)), tau / inertia, "rad/s²",
                        L.F("I = {0} kg·m²; α = τ / I = {1} / {0} = {2} rad/s².", N(inertia, 5), N(tau), N(tau / inertia)), 3);
                    default: return Ex.Num(L.F("Um drone de {0} kg com braço de {1} m (I = 0,4·m·L²) recebe um torque de {2} N·m. Quantos graus por segundo ele ganha a cada segundo (aceleração angular em °/s²)?", N(m), N(La), N(tau)), alpha * 180 / Math.PI, "°/s²",
                        L.F("α = τ/I = {0} rad/s² = {1} °/s².", N(alpha), N(alpha * 180 / Math.PI)), 3);
                }
            }
            default:
            {
                double tau = 4 * dT * arm, alpha = tau / inertia, th = Ex.Step(r, 5, 40, 1), thr = th * Math.PI / 180, t = Math.Sqrt(2 * thr / alpha), om = alpha * t * 180 / Math.PI;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone de {0} kg (braço {1} m, I = 0,4·m·L²) parte do repouso e recebe um torque constante de {2} N·m. Quanto tempo leva, em s, para girar {3}°? (θ = ½·α·t²)", N(m), N(La), N(tau), N(th)), t, "s",
                        L.F("α = τ/I = {0} rad/s²; θ = {1} rad; t = √(2θ/α) = {2} s.", N(alpha), N(thr, 4), N(t, 4)), 3);
                return Ex.Num(L.F("Um drone de {0} kg (braço {1} m, I = 0,4·m·L²) parte do repouso com torque constante de {2} N·m. Que velocidade angular, em °/s, ele tem ao completar {3}°?", N(m), N(La), N(tau), N(th)), om, "°/s",
                    L.F("α = {0} rad/s²; ω² = 2·α·θ → ω = √(2·{0}·{1}) = {2} rad/s = {3} °/s.", N(alpha), N(thr, 4), N(alpha * t, 4), N(om)), 3);
            }
        }
    }
}

[Exercises("drone-08-gps-navegacao")]
public sealed class Drone08 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double v = Ex.Step(r, 5, 20, 1), dist = Ex.Step(r, 500, 5000, 100), w = Ex.Step(r, 1, 6, 0.5);
        switch (d)
        {
            case Difficulty.Easy:
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um drone voa a {0} m/s até um ponto a {1} m. Quanto tempo leva, em s?", N(v), N(dist)), dist / v, "s",
                        L.F("t = d / v = {0} / {1} = {2} s.", N(dist), N(v), N(dist / v)));
                    case 1: return Ex.Num(L.F("Um drone voa {0} m em {1} s. Qual a velocidade média, em m/s?", N(dist), N(dist / v)), v, "m/s",
                        L.F("v = d / t = {0} / {1} = {2} m/s.", N(dist), N(dist / v), N(v)));
                    default: return Ex.Num(L.F("Um drone voa a {0} m/s por {1} min. Que distância percorre, em m?", N(v), N(dist / v / 60)), dist, "m",
                        L.F("d = v·t = {0} · {1}·60 = {2} m.", N(v), N(dist / v / 60), N(dist)), 3);
                }
            case Difficulty.Medium:
            {
                double lat = Ex.Step(r, -30, -5, 1), dlat = Ex.Step(r, 0.001, 0.02, 0.001), dlon = Ex.Step(r, 0.001, 0.02, 0.001);
                double dy = dlat * 111320, dx = dlon * 111320 * Math.Cos(lat * Math.PI / 180), dd = Math.Sqrt(dx * dx + dy * dy);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um ponto está {0}° ao norte de onde o drone está. Quantos metros isso vale? (1° de latitude ≈ 111 320 m)", N(dlat, 3)), dy, "m",
                        L.F("{0} × 111 320 = {1} m.", N(dlat, 3), N(dy)));
                return Ex.Num(L.F("Um drone está a {0}° de latitude e o ponto de destino fica {1}° ao norte e {2}° ao leste. Qual a distância em linha reta, em m? (1° de latitude ≈ 111 320 m; 1° de longitude ≈ 111 320·cos(lat) m)", N(lat), N(dlat, 3), N(dlon, 3)), dd, "m",
                    L.F("Norte: {0} m; leste: {1} m (com cos(lat) = {2}); distância = √({0}² + {1}²) = {3} m.", N(dy), N(dx), N(Math.Cos(lat * Math.PI / 180), 3), N(dd)), 3);
            }
            case Difficulty.Hard:
            {
                double up = Ex.Step(r, 20, 100, 5), vc = Ex.Step(r, 1, 4, 0.5), hold = Ex.Step(r, 30, 300, 10);
                double tTotal = up / vc + 2 * dist / v + up / vc + hold;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Uma missão sobe {0} m a {1} m/s, voa {2} m até o ponto a {3} m/s, volta o mesmo caminho e desce {0} m a {1} m/s. Quanto tempo dura, em minutos, sem contar nada parado?", N(up), N(vc), N(dist), N(v)), (tTotal - hold) / 60, "min",
                        L.F("Subir + descer = {0}·2 / {1} = {2} s; ida e volta = 2·{3} / {4} = {5} s; total = {6} s = {7} min.", N(up), N(vc), N(2 * up / vc), N(dist), N(v), N(2 * dist / v), N(tTotal - hold), N((tTotal - hold) / 60)), 3);
                    default: return Ex.Num(L.F("Uma missão sobe {0} m a {1} m/s, voa {2} m até o ponto a {3} m/s, fica {4} s parado fotografando, volta e desce {0} m. Quantos minutos dura no total?", N(up), N(vc), N(dist), N(v), N(hold)), tTotal / 60, "min",
                        L.F("Subir e descer: {0} s; ida e volta: {1} s; parado: {2} s; total = {3} s = {4} min.", N(2 * up / vc), N(2 * dist / v), N(hold), N(tTotal), N(tTotal / 60)), 3);
                }
            }
            default:
            {
                double vair = Ex.Step(r, 10, 18, 1), tw = dist / (vair - w) + dist / (vair + w), tn = 2 * dist / vair;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone voa a {0} m/s em relação ao ar até um ponto a {1} m, com vento de {2} m/s contra na ida (a favor na volta). Quanto tempo leva ida e volta, em s?", N(vair), N(dist), N(w)), tw, "s",
                        L.F("Ida: {0}/({1} − {2}) = {3} s; volta: {0}/({1} + {2}) = {4} s; total = {5} s.", N(dist), N(vair), N(w), N(dist / (vair - w)), N(dist / (vair + w)), N(tw)), 3);
                return Ex.Num(L.F("Um drone voa a {0} m/s em relação ao ar, ida e volta a um ponto a {1} m, com vento de {2} m/s sempre na direção da linha (contra na ida, a favor na volta). Quantos por cento o tempo total é maior do que seria sem vento?", N(vair), N(dist), N(w)), (tw / tn - 1) * 100, "%",
                    L.F("Sem vento: {0} s; com vento: {1} s; o vento contra rouba mais do que o a favor devolve: +{2}%.", N(tn), N(tw), N((tw / tn - 1) * 100)), 3);
            }
        }
    }
}

[Exercises("drone-09-regras")]
public sealed class Drone09 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double Ft = 0.3048;
        double mass = Ex.Step(r, 100, 3000, 25), pay = Ex.Step(r, 0, 800, 50);
        string rules = L.T("Regras da lição: drones com mais de 250 g devem ser cadastrados na ANAC (SISANT); altura de referência de até 120 m (400 pés) acima do solo; distância horizontal mínima de 30 m de pessoas que não concordaram com a operação.");
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone voa a {0} pés acima do solo. Quantos metros são? (1 pé = 0,3048 m)", N(Math.Round(mass / 5) * 5, 0)), Math.Round(mass / 5) * 5 * Ft, "m",
                        L.F("{0} × 0,3048 = {1} m.", N(Math.Round(mass / 5) * 5, 0), N(Math.Round(mass / 5) * 5 * Ft)));
                {
                    double alt = Ex.Step(r, 20, 200, 5);
                    return Ex.Choice(r, rules + " " + L.F("Um piloto quer voar a {0} m acima do solo. Isso respeita a altura de referência?", N(alt)),
                        alt <= 120 ? L.T("Sim, está dentro de 120 m") : L.T("Não, passa de 120 m"),
                        new[] { alt <= 120 ? L.T("Não, passa de 120 m") : L.T("Sim, está dentro de 120 m") },
                        L.F("O limite é 120 m acima do solo; {0} m {1}.", N(alt), alt <= 120 ? L.T("está dentro") : L.T("passa dele")));
                }
            case Difficulty.Medium:
            {
                bool reg = mass > 250;
                if (r.Next(2) == 0)
                    return Ex.Choice(r, rules + " " + L.F("Um drone pesa {0} g. Precisa de cadastro na ANAC?", N(mass, 0)),
                        reg ? L.T("Sim, pesa mais de 250 g") : L.T("Não, pesa até 250 g"),
                        new[] { reg ? L.T("Não, pesa até 250 g") : L.T("Sim, pesa mais de 250 g") },
                        L.F("O corte é 250 g: {0} g {1}.", N(mass, 0), reg ? L.T("passa dele (cadastro)") : L.T("está dentro dele (sem cadastro)")));
                double ft = Ex.Step(r, 100, 500, 10), m = ft * Ft;
                return Ex.Num(L.F("Um drone voa a {0} m acima do solo. Quantos pés são? (1 m = 3,2808 pés)", N(m)), m * 3.2808, "pés",
                    L.F("{0} × 3,2808 = {1} ft.", N(m), N(m * 3.2808)), 3);
            }
            case Difficulty.Hard:
            {
                double total = mass + pay;
                switch (r.Next(2))
                {
                    case 0: return Ex.Choice(r, rules + " " + L.F("Um drone de {0} g leva uma câmera de {1} g. Com a câmera, precisa de cadastro?", N(mass, 0), N(pay, 0)),
                        total > 250 ? L.T("Sim, o conjunto passa de 250 g") : L.T("Não, o conjunto tem até 250 g"),
                        new[] { total > 250 ? L.T("Não, o conjunto tem até 250 g") : L.T("Sim, o conjunto passa de 250 g") },
                        L.F("Vale o peso decolando: {0} + {1} = {2} g, que {3} 250 g.", N(mass, 0), N(pay, 0), N(total, 0), total > 250 ? L.T("passa de") : L.T("não passa de")));
                    default:
                    {
                        double ground = Ex.Step(r, 0, 1200, 50);
                        return Ex.Num(L.F("O terreno da decolagem fica a {0} m acima do nível do mar. Para voar a 120 m acima do solo, qual a altitude máxima, em m acima do nível do mar, que o altímetro (referido ao mar) pode mostrar?", N(ground)), ground + 120, "m",
                            L.F("altitude = {0} + 120 = {1} m.", N(ground), N(ground + 120)));
                    }
                }
            }
            default:
            {
                double slope = Ex.Step(r, 10, 60, 5), tall = Ex.Step(r, 20, 90, 5);
                if (r.Next(2) == 0)
                {
                    double hh = Ex.Step(r, 60, 200, 5), hill = Ex.Step(r, 10, 100, 5), agl = hh - hill;
                    return Ex.Choice(r, rules + " " + L.F("O drone voa a {0} m acima do ponto de decolagem, sobre um morro que está {1} m acima desse ponto. Qual a altura acima do solo sob ele, e respeita os 120 m?", N(hh), N(hill)),
                        L.F("{0} m acima do solo: {1}", N(agl), agl <= 120 ? L.T("respeita") : L.T("não respeita")),
                        new[] { L.F("{0} m acima do solo: {1}", N(hh), hh <= 120 ? L.T("respeita") : L.T("não respeita")), L.F("{0} m acima do solo: {1}", N(agl + 30), agl + 30 <= 120 ? L.T("respeita") : L.T("não respeita")) },
                        L.F("Altura sobre o solo = {0} − {1} = {2} m.", N(hh), N(hill), N(agl)));
                }
                double mm = Ex.Step(r, 150, 450, 10);
                return Ex.Num(L.F("Um drone de {0} g precisa perder quantos gramas para ficar em 250 g (o limite sem cadastro na ANAC)?", N(mm + 250, 0)), mm, "g",
                    L.F("{0} − 250 = {1} g.", N(mm + 250, 0), N(mm)));
            }
        }
    }
}

[Exercises("drone-10-projeto-entrega")]
public sealed class Drone10 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double dist = Ex.Step(r, 1000, 6000, 250), v = Ex.Step(r, 8, 18, 1), Ph = Ex.Step(r, 120, 400, 10), Pc = Ph * Ex.Step(r, 0.85, 1.0, 0.05), t1 = Ex.Step(r, 20, 90, 5), t2 = Ex.Step(r, 20, 60, 5);
        double tleg = dist / v, E = (Ph * t1 + Pc * tleg * 2 + Ph * t2) / 3600;
        switch (d)
        {
            case Difficulty.Easy:
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Uma entrega fica a {0} m. A {1} m/s, quanto tempo leva a ida, em minutos?", N(dist), N(v)), tleg / 60, "min",
                        L.F("t = {0} / {1} = {2} s = {3} min.", N(dist), N(v), N(tleg), N(tleg / 60)));
                    default: return Ex.Num(L.F("Um drone gasta {0} W voando e a ida dura {1} s. Quantos Wh gasta na ida?", N(Pc), N(tleg)), Pc * tleg / 3600, "Wh",
                        L.F("E = P·t = {0}·{1} / 3600 = {2} Wh.", N(Pc), N(tleg), N(Pc * tleg / 3600)));
                }
            case Difficulty.Medium:
            {
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Entrega a {0} m, voando a {1} m/s, com {2} W em cruzeiro, ida e volta. Quantos Wh gasta o voo de cruzeiro (sem decolagem nem pouso)?", N(dist), N(v), N(Pc)), Pc * tleg * 2 / 3600, "Wh",
                        L.F("t = 2·{0}/{1} = {2} s; E = {3}·{2}/3600 = {4} Wh.", N(dist), N(v), N(2 * tleg), N(Pc), N(Pc * tleg * 2 / 3600)));
                    default: return Ex.Num(L.F("Um drone paira {0} s na decolagem e {1} s no pouso, gastando {2} W pairando. Quantos Wh gasta nessas duas etapas?", N(t1), N(t2), N(Ph)), Ph * (t1 + t2) / 3600, "Wh",
                        L.F("E = {0}·({1} + {2}) / 3600 = {3} Wh.", N(Ph), N(t1), N(t2), N(Ph * (t1 + t2) / 3600)));
                }
            }
            case Difficulty.Hard:
            {
                int cells = Ex.Pick(r, 4, 6), nS = cells;
                double need = E / 0.8, mah = need / (nS * 3.7) * 1000;
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Missão: pairar {0} s ({1} W), voar {2} m ida e {2} m volta a {3} m/s ({4} W), pairar mais {5} s para pousar ({1} W). Quantos Wh a missão gasta?", N(t1), N(Ph), N(dist), N(v), N(Pc), N(t2)), E, "Wh",
                            L.F("E = ({0}·{1} + {2}·{3} + {0}·{4}) / 3600 = {5} Wh.", N(Ph), N(t1), N(Pc), N(2 * tleg), N(t2), N(E)), 3);
                    case 1: return Ex.Num(L.F("Uma missão gasta {0} Wh. Para ter 20% de reserva (só 80% da bateria é usada), qual a capacidade mínima da bateria, em Wh?", N(E)), need, "Wh",
                            L.F("Bateria = E / 0,8 = {0} / 0,8 = {1} Wh.", N(E), N(need)), 3);
                    default: return Ex.Num(L.F("Uma missão gasta {0} Wh e a bateria deve ter 20% de reserva. Em mAh, qual a capacidade mínima de uma bateria {1}S (3,7 V por célula)?", N(E), N(nS, 0)), mah, "mAh",
                            L.F("Bateria = {0}/0,8 = {1} Wh; mAh = Wh / V × 1000 = {1} / {2} × 1000 = {3} mAh.", N(E), N(need), N(nS * 3.7), N(mah)), 3);
                }
            }
            default:
            {
                double m0 = Ex.Step(r, 1.0, 3.0, 0.1), pay = Ex.Step(r, 0.2, 1.0, 0.1);
                double fac = Math.Pow((m0 + pay) / m0, 1.5);
                double Pl = Pc * fac, Eout = Pl * tleg, Eback = Pc * tleg, Etot = (Ph * fac * t1 + Eout + Eback + Ph * t2) / 3600;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um drone de {0} kg consome {1} W em cruzeiro vazio. A potência cresce com a massa^1,5. Levando {2} kg de carga na ida, quantos W ele gasta em cruzeiro na ida?", N(m0), N(Pc), N(pay)), Pl, "W",
                        L.F("P = {0} · (({1} + {2}) / {1})^1,5 = {3} W.", N(Pc), N(m0), N(pay), N(Pl)), 3);
                return Ex.Num(L.F("Um drone de {0} kg gasta {1} W em cruzeiro vazio (potência ∝ massa^1,5) e leva {2} kg de carga na ida a {3} m, a {4} m/s; volta vazio. Quantos Wh gasta ida e volta (só cruzeiro)?", N(m0), N(Pc), N(pay), N(dist), N(v)), (Eout + Eback) / 3600, "Wh",
                    L.F("Ida com carga: {0} W × {1} s = {2} Wh; volta vazio: {3} W × {1} s = {4} Wh; total = {5} Wh.", N(Pl), N(tleg), N(Eout / 3600), N(Pc), N(Eback / 3600), N((Eout + Eback) / 3600)), 3);
            }
        }
    }
}
