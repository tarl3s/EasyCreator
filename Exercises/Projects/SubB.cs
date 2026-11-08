namespace EasyCreator.Exercises;

/// <summary>Submarines 6-10: propulsion and drag, stability, sensors, depth PID, the ROV budget.</summary>
[Exercises("sub-06-propulsao-arrasto")]
public sealed class Sub06 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double dia = Ex.Step(r, 15, 80, 5), A = Pk.Area(dia / 100), cd = Ex.Step(r, 0.2, 1.0, 0.05), v = Ex.Step(r, 0.5, 4, 0.1), k = 0.5 * Pk.RhoW * cd * A, F = k * v * v;
        string intro = L.F("Um veículo submarino com {0} cm de diâmetro frontal tem Cd = {1} e se move a {2} m/s na água (1000 kg/m³).", Ex.N(dia), Ex.N(cd), Ex.N(v));
        switch (d)
        {
            case Difficulty.Easy:
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(intro + " " + L.T("Qual o arrasto?"), F, "N", L.F("A = π·d²/4 = {0} m²; F = ½·ρ·Cd·A·v² = 0,5·1000·{1}·{0}·{2}² = {3} N.", Ex.N(A, 5), Ex.N(cd), Ex.N(v), Ex.N(F, 3)));
                    case 1: return Ex.Num(intro + " " + L.T("Se a velocidade dobrar, qual será o arrasto?"), 4 * F, "N", L.F("O arrasto cresce com v²: F = {0} N → 4·F = {1} N.", Ex.N(F, 3), Ex.N(4 * F, 3)));
                    default: return Ex.Num(L.F("Um veículo submarino com {0} cm de diâmetro frontal, a {1} m/s na água (1000 kg/m³), sofre {2} N de arrasto. Qual o Cd?", Ex.N(dia), Ex.N(v), Ex.N(F, 3)), cd, "", L.F("Cd = 2·F / (ρ·A·v²) = 2·{0} / (1000·{1}·{2}²) = {3}.", Ex.N(F, 3), Ex.N(A, 5), Ex.N(v), Ex.N(cd)));
                }
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.T("Qual a potência mecânica necessária para vencer o arrasto (P = F·v)?"), F * v, "W", L.F("F = {0} N; P = F·v = {0}·{1} = {2} W.", Ex.N(F, 3), Ex.N(v), Ex.N(F * v, 3)));
                return Ex.Num(intro + " " + L.T("Por qual fator a potência aumenta se a velocidade for multiplicada por 2? (responda só o número)"), 8, "", L.T("P = F·v e F ∝ v², então P ∝ v³: 2³ = 8."));
            case Difficulty.Hard:
            {
                double eta = Ex.Step(r, 0.3, 0.7, 0.05), pe = F * v / eta;
                if (r.Next(2) == 0)
                    return Ex.Num(intro + " " + L.F("A eficiência do conjunto motor + hélice é {0}. Qual a potência elétrica consumida?", Ex.N(eta)), pe, "W", L.F("P_mec = F·v = {0} W; P_elétrica = P_mec / η = {0} / {1} = {2} W.", Ex.N(F * v, 3), Ex.N(eta), Ex.N(pe, 3)));
                double Wh = Ex.Step(r, 50, 300, 10), t = Wh * 3600 / pe / 60;
                return Ex.Num(intro + " " + L.F("Com eficiência {0} e bateria de {1} Wh dedicada à propulsão, quantos minutos ele aguenta nessa velocidade?", Ex.N(eta), Ex.N(Wh)), t, "min", L.F("P_elétrica = F·v/η = {0} W; t = E/P = {1} Wh / {0} W = {2} h = {3} min.", Ex.N(pe, 3), Ex.N(Wh), Ex.N(Wh / pe, 3), Ex.N(t)));
            }
            default:
            {
                double eta = Ex.Step(r, 0.3, 0.6, 0.05), P = Ex.Step(r, 20, 400, 10), vmax = Math.Cbrt(2 * P * eta / (Pk.RhoW * cd * A));
                return Ex.Num(L.F("Um AUV com {0} cm de diâmetro frontal e Cd = {1} tem {2} W elétricos disponíveis para propulsão, com eficiência {3}. Qual a velocidade máxima na água (1000 kg/m³)? (P·η = ½·ρ·Cd·A·v³)", Ex.N(dia), Ex.N(cd), Ex.N(P), Ex.N(eta)),
                    vmax, "m/s", L.F("P_útil = {0}·{1} = {2} W; v³ = 2·P_útil / (ρ·Cd·A) = {3}; v = {4} m/s.", Ex.N(P), Ex.N(eta), Ex.N(P * eta), Ex.N(vmax * vmax * vmax, 4), Ex.N(vmax, 3)));
            }
        }
    }
}

[Exercises("sub-07-estabilidade")]
public sealed class Sub07 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        double M = Ex.Step(r, 5, 200, 1), h = Ex.Step(r, 1, 15, 0.5) / 100, th = Ex.Step(r, 5, 30, 1), W = M * G, tau = W * h * Math.Sin(th * Math.PI / 180);
        switch (d)
        {
            case Difficulty.Easy:
                switch (r.Next(3))
                {
                    case 0: return Ex.Num(L.F("Um ROV de {0} kg totalmente submerso tem o centro de empuxo {1} cm acima do centro de gravidade e está inclinado {2}°. Qual o torque restaurador τ = P·h·sen θ? (g = 9,8 m/s²)", Ex.N(M), Ex.N(h * 100), Ex.N(th)), tau, "N·m", L.F("P = {0} N; τ = {0}·{1}·sen({2}°) = {3} N·m.", Ex.N(W), Ex.N(h), Ex.N(th), Ex.N(tau, 3)));
                    case 1: return Ex.Choice(r, L.F("Um ROV de {0} kg totalmente submerso tem o centro de gravidade {1} cm {2} do centro de empuxo. Ele é estável?", Ex.N(M), Ex.N(h * 100), r.Next(2) == 0 ? L.T("abaixo") : L.T("acima")),
                        L.T("Depende só de qual está em cima: o CG abaixo do CB é estável; acima, é instável."),
                        new[] { L.T("Sempre estável, porque está submerso."), L.T("Sempre instável, porque tem massa."), L.T("A estabilidade só depende do volume.") },
                        L.T("Quando o CG fica abaixo do centro de empuxo, uma inclinação cria um torque que devolve o veículo à posição; com o CG acima, o torque aumenta a inclinação."));
                    default: return Ex.Num(L.F("O torque restaurador de um ROV de {0} kg inclinado {1}° é {2} N·m. Qual a distância h entre o CB e o CG, em cm? (g = 9,8 m/s²)", Ex.N(M), Ex.N(th), Ex.N(tau, 3)), h * 100, "cm", L.F("h = τ / (P·sen θ) = {0} / ({1}·sen {2}°) = {3} m = {4} cm.", Ex.N(tau, 3), Ex.N(W), Ex.N(th), Ex.N(h, 4), Ex.N(h * 100)));
                }
            case Difficulty.Medium:
            {
                double th2 = Ex.Step(r, 10, 40, 5), tau2 = W * h * Math.Sin(th2 * Math.PI / 180), ang = th * 2;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um ROV de {0} kg tem h = {1} cm entre CB e CG. Um empurrão o inclina {2}°. Qual o torque restaurador em N·m? (g = 9,8 m/s²)", Ex.N(M), Ex.N(h * 100), Ex.N(th2)), tau2, "N·m", L.F("τ = P·h·sen θ = {0}·{1}·sen({2}°) = {3} N·m.", Ex.N(W), Ex.N(h), Ex.N(th2), Ex.N(tau2, 3)));
                double tau3 = W * h * Math.Sin(th * Math.PI / 180), tauMax = W * h;
                return Ex.Num(L.F("Um ROV de {0} kg tem h = {1} cm entre CB e CG. Qual o maior torque restaurador possível, e em que ângulo ele ocorre? Responda o torque máximo em N·m. (g = 9,8 m/s²)", Ex.N(M), Ex.N(h * 100)), tauMax, "N·m", L.F("τ = P·h·sen θ é máximo em θ = 90° (sen = 1): τ = {0}·{1} = {2} N·m.", Ex.N(W), Ex.N(h), Ex.N(tauMax, 3)));
            }
            case Difficulty.Hard:
            {
                double I = Ex.Step(r, 0.2, 5, 0.1), T = 2 * Math.PI * Math.Sqrt(I / (W * h));
                return Ex.Num(L.F("Um ROV de {0} kg, momento de inércia {1} kg·m² em torno do eixo de rolagem e h = {2} cm entre CB e CG, é solto inclinado de um pequeno ângulo. Qual o período de oscilação T = 2π·√(I / (P·h))? (g = 9,8 m/s²)", Ex.N(M), Ex.N(I), Ex.N(h * 100)),
                    T, "s", L.F("P·h = {0}·{1} = {2} N·m; T = 2π·√({3}/{2}) = {4} s.", Ex.N(W), Ex.N(h), Ex.N(W * h, 3), Ex.N(I), Ex.N(T, 3)));
            }
            default:
            {
                double m = Ex.Step(r, 0.2, 2, 0.1), y = Ex.Step(r, 5, 30, 1) / 100, ang = Math.Atan(m * y / (M * h)) * 180 / Math.PI, tmax = Ex.Step(r, 3, 10, 1), ymax = M * h * Math.Tan(tmax * Math.PI / 180) / m;
                if (m * y / (M * h) > 0.6) { m = Math.Max(0.05, Math.Round(0.5 * M * h / y, 2)); ang = Math.Atan(m * y / (M * h)) * 180 / Math.PI; ymax = M * h * Math.Tan(tmax * Math.PI / 180) / m; }
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Num ROV de {0} kg com h = {1} cm entre CB e CG, um peso de {2} kg (já incluído nos {0} kg) é deslocado {3} cm para o lado. Qual o ângulo de adernamento de equilíbrio? (tan θ = m·y / (M·h))", Ex.N(M), Ex.N(h * 100), Ex.N(m), Ex.N(y * 100)), ang, "°", L.F("tan θ = {0}·{1} / ({2}·{3}) = {4}; θ = {5}°.", Ex.N(m), Ex.N(y), Ex.N(M), Ex.N(h), Ex.N(m * y / (M * h), 4), Ex.N(ang)), 2.5);
                return Ex.Num(L.F("Um ROV de {0} kg tem h = {1} cm entre CB e CG. Qual o maior deslocamento lateral, em cm, de uma carga de {2} kg (já incluída nos {0} kg) que mantém o adernamento abaixo de {3}°? (tan θ = m·y / (M·h))", Ex.N(M), Ex.N(h * 100), Ex.N(m), Ex.N(tmax)), ymax * 100, "cm", L.F("y = M·h·tan θ / m = {0}·{1}·tan({2}°) / {3} = {4} m = {5} cm.", Ex.N(M), Ex.N(h), Ex.N(tmax), Ex.N(m), Ex.N(ymax, 4), Ex.N(ymax * 100)));
            }
        }
    }
}

[Exercises("sub-08-sensores-navegacao")]
public sealed class Sub08 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        const double G = Pk.G;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double dist = Ex.Step(r, 2, 100, 0.5), c = Ex.Pick(r, 1500.0, 1480, 1520), t = 2 * dist / c;
                switch (r.Next(2))
                {
                    case 0: return Ex.Num(L.F("Um sonar emite um pulso e ouve o eco {0} ms depois. Com velocidade do som na água de {1} m/s, a que distância está o obstáculo?", Ex.N(t * 1000, 2), Ex.N(c)), dist, "m", L.F("O som vai e volta: d = c·t / 2 = {0}·{1} / 2 = {2} m.", Ex.N(c), Ex.N(t, 5), Ex.N(dist)));
                    default: return Ex.Num(L.F("Um obstáculo está a {0} m de um sonar (som na água: {1} m/s). Quantos milissegundos leva o eco para voltar?", Ex.N(dist), Ex.N(c)), t * 1000, "ms", L.F("t = 2·d / c = 2·{0} / {1} = {2} s = {3} ms.", Ex.N(dist), Ex.N(c), Ex.N(t, 5), Ex.N(t * 1000, 2)));
                }
            }
            case Difficulty.Medium:
            {
                double depth = Ex.Step(r, 5, 300, 1), rf = Ex.Pick(r, 1000.0, 1025.0), p = Pk.P0 + rf * G * depth;
                string fl = rf == 1000 ? L.T("água doce (1000 kg/m³)") : L.T("água do mar (1025 kg/m³)");
                return Ex.Num(L.F("O sensor de pressão de um ROV em {0} marca {1} kPa (absoluta). Qual a profundidade? (atmosfera = 101,325 kPa; g = 9,8 m/s²)", fl, Ex.N(p / 1000, 3)), depth, "m", L.F("h = (p − p₀) / (ρ·g) = ({0} − 101,325)·1000 / ({1}·9,8) = {2} m.", Ex.N(p / 1000, 3), Ex.N(rf), Ex.N(depth)), 3);
            }
            case Difficulty.Hard:
            {
                double bias = Ex.Step(r, 0.005, 0.05, 0.005), tt = Ex.Step(r, 60, 600, 30), err = bias * tt, eps = Ex.Step(r, 1, 10, 0.5), dd = Ex.Step(r, 20, 500, 10), lat = dd * Math.Sin(eps * Math.PI / 180);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("O giroscópio de um AUV tem um viés não corrigido de {0} °/s. Integrando a taxa de giro por {1} s, qual o erro de rumo acumulado, em graus?", Ex.N(bias, 3), Ex.N(tt)), err, "°", L.F("Erro = viés · tempo = {0}·{1} = {2}°.", Ex.N(bias, 3), Ex.N(tt), Ex.N(err, 3)));
                return Ex.Num(L.F("Um AUV acredita seguir em linha reta por {0} m, mas seu rumo estava errado em {1}°. Qual o erro lateral na posição final? (lateral = distância · sen(erro))", Ex.N(dd), Ex.N(eps)), lat, "m", L.F("lateral = {0}·sen({1}°) = {2} m.", Ex.N(dd), Ex.N(eps), Ex.N(lat, 3)));
            }
            default:
            {
                double depth = Ex.Step(r, 20, 300, 10), pAssumed = Pk.RhoW, pTrue = Pk.RhoSea, p = Pk.P0 + pTrue * G * depth, dRead = (p - Pk.P0) / (pAssumed * G), diff = dRead - depth;
                return Ex.Num(L.F("Um ROV está em água do mar (1025 kg/m³) a {0} m de verdade, mas o software calcula a profundidade supondo água doce (1000 kg/m³). Que profundidade o software mostra? (g = 9,8 m/s²; mesma pressão absoluta, atmosfera 101325 Pa)", Ex.N(depth)),
                    dRead, "m", L.F("p_manométrica = 1025·9,8·{0} = {1} Pa; o software divide por 1000·9,8: h = {2} m (erro de {3} m, cerca de 2,5%).", Ex.N(depth), Ex.N(p - Pk.P0, 0), Ex.N(dRead, 3), Ex.N(diff, 3)));
            }
        }
    }
}

[Exercises("sub-09-controle-pid")]
public sealed class Sub09 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double kp = Ex.Step(r, 5, 100, 5), ki = Ex.Step(r, 0.5, 20, 0.5), kd = Ex.Step(r, 5, 60, 5);
        switch (d)
        {
            case Difficulty.Easy:
            {
                double target = Ex.Step(r, 1, 20, 0.5), now = target - Ex.Step(r, 0.2, 3, 0.1), e = target - now;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um controlador proporcional (Kp = {0} N/m) mantém um ROV a {1} m de profundidade. Ele está a {2} m. Qual a força de comando u = Kp·erro?", Ex.N(kp), Ex.N(target), Ex.N(now)), kp * e, "N", L.F("erro = {0} − {1} = {2} m; u = {3}·{2} = {4} N.", Ex.N(target), Ex.N(now), Ex.N(e), Ex.N(kp), Ex.N(kp * e)));
                return Ex.Num(L.F("Um controlador proporcional dá {0} N de comando quando o erro é {1} m. Qual o ganho Kp?", Ex.N(kp * e), Ex.N(e)), kp, "N/m", L.F("Kp = u / erro = {0} / {1} = {2} N/m.", Ex.N(kp * e), Ex.N(e), Ex.N(kp)));
            }
            case Difficulty.Medium:
            {
                double e = Ex.Step(r, 0.2, 3, 0.1), ePrev = e + Ex.Step(r, 0.05, 0.5, 0.05), dt = Ex.Pick(r, 0.05, 0.1, 0.2), de = (e - ePrev) / dt, u = kp * e + kd * de;
                return Ex.Num(L.F("Num controlador PD (Kp = {0}, Kd = {1}), o erro de profundidade caiu de {2} m para {3} m em {4} s. Qual o comando u = Kp·e + Kd·(de/dt)?", Ex.N(kp), Ex.N(kd), Ex.N(ePrev), Ex.N(e), Ex.N(dt)), u, "N", L.F("de/dt = ({0} − {1}) / {2} = {3} m/s; u = {4}·{0} + {5}·({3}) = {6} N.", Ex.N(e), Ex.N(ePrev), Ex.N(dt), Ex.N(de, 3), Ex.N(kp), Ex.N(kd), Ex.N(u, 3)));
            }
            case Difficulty.Hard:
            {
                double dt = Ex.Pick(r, 0.1, 0.2, 0.5), e1 = Ex.Step(r, 0.5, 2, 0.1), e2 = e1 - Ex.Step(r, 0.05, 0.3, 0.05), e3 = e2 - Ex.Step(r, 0.05, 0.3, 0.05), integ = (e1 + e2 + e3) * dt;
                double u = kp * e3 + ki * integ;
                return Ex.Num(L.F("Num controlador PI com Kp = {0} e Ki = {1}, os últimos três erros de profundidade (a cada {2} s) foram {3}, {4} e {5} m. Qual o comando atual u = Kp·e + Ki·∫e dt (integral = soma dos erros · dt; e atual = último erro)?", Ex.N(kp), Ex.N(ki), Ex.N(dt), Ex.N(e1), Ex.N(e2), Ex.N(e3)),
                    u, "N", L.F("∫e dt = ({0} + {1} + {2})·{3} = {4}; u = {5}·{2} + {6}·{4} = {7} N.", Ex.N(e1), Ex.N(e2), Ex.N(e3), Ex.N(dt), Ex.N(integ, 4), Ex.N(kp), Ex.N(ki), Ex.N(u, 3)));
            }
            default:
            {
                double Fd = Ex.Step(r, 2, 30, 1), kpp = Ex.Step(r, 20, 200, 10), ess = Fd / kpp, tol = Ex.Step(r, 1, 5, 1) / 100, need = Fd / tol;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um ROV tem um peso líquido de {0} N dentro d'água (puxa para baixo) e um controlador só proporcional com Kp = {1} N/m. Em equilíbrio, qual o erro de profundidade permanente, em cm? (Kp·erro = peso)", Ex.N(Fd), Ex.N(kpp)), ess * 100, "cm", L.F("Kp·erro = P → erro = {0} / {1} = {2} m = {3} cm. Com a ação integral o erro permanente iria a zero.", Ex.N(Fd), Ex.N(kpp), Ex.N(ess, 4), Ex.N(ess * 100)));
                return Ex.Num(L.F("Um ROV tem peso líquido de {0} N dentro d'água e usa controle só proporcional. Que ganho Kp mínimo, em N/m, mantém o erro permanente abaixo de {1} cm?", Ex.N(Fd), Ex.N(tol * 100)), need, "N/m", L.F("Kp = P / erro = {0} / {1} m = {2} N/m.", Ex.N(Fd), Ex.N(tol, 3), Ex.N(need)));
            }
        }
    }
}

[Exercises("sub-10-projeto-rov")]
public sealed class Sub10 : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double pt = Ex.Step(r, 30, 150, 10), nt = Ex.Pick(r, 4.0, 6.0), cam = Ex.Step(r, 5, 30, 1), el = Ex.Step(r, 5, 25, 1), lights = Ex.Step(r, 10, 60, 5);
        double hotel = cam + el + lights, total = nt * pt + hotel;
        switch (d)
        {
            case Difficulty.Easy:
                return Ex.Num(L.F("O orçamento de energia de um ROV tem {0} propulsores de {1} W cada (potência máxima), câmera de {2} W, eletrônica de {3} W e luzes de {4} W. Qual a potência total máxima?", Ex.N(nt), Ex.N(pt), Ex.N(cam), Ex.N(el), Ex.N(lights)), total, "W", L.F("{0}·{1} + {2} + {3} + {4} = {5} W.", Ex.N(nt), Ex.N(pt), Ex.N(cam), Ex.N(el), Ex.N(lights), Ex.N(total)));
            case Difficulty.Medium:
            {
                double v = Ex.Pick(r, 12.0, 14.8, 22.2, 24.0), ah = Ex.Step(r, 5, 30, 1), wh = v * ah, t = wh / total * 60;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma bateria de {0} V e {1} Ah guarda quantos Wh?", Ex.N(v), Ex.N(ah)), wh, "Wh", L.F("E = V·Ah = {0}·{1} = {2} Wh.", Ex.N(v), Ex.N(ah), Ex.N(wh)));
                return Ex.Num(L.F("Um ROV com {0} propulsores de {1} W, câmera {2} W, eletrônica {3} W e luzes {4} W usa tudo no máximo. Com bateria de {5} V e {6} Ah, quantos minutos ela dura?", Ex.N(nt), Ex.N(pt), Ex.N(cam), Ex.N(el), Ex.N(lights), Ex.N(v), Ex.N(ah)), t, "min", L.F("P = {0} W; E = {1}·{2} = {3} Wh; t = E/P = {4} h = {5} min.", Ex.N(total), Ex.N(v), Ex.N(ah), Ex.N(wh), Ex.N(wh / total, 3), Ex.N(t)));
            }
            case Difficulty.Hard:
            {
                double M = Ex.Step(r, 8, 40, 1), vf = Ex.Step(r, 3, 15, 0.5), rhof = Ex.Pick(r, 400.0, 450, 500, 550), dm = M - Pk.RhoW * vf / 1000, vfoam = dm / (Pk.RhoW - rhof) * 1000;
                if (dm < 0.5) { vf = Math.Round(M / 2); dm = M - Pk.RhoW * vf / 1000; vfoam = dm / (Pk.RhoW - rhof) * 1000; }
                return Ex.Num(L.F("Um ROV de {0} kg (sem espuma) desloca {1} L com a estrutura e os flutuadores rígidos. Que volume de espuma de {2} kg/m³ (adicionada ao veículo) é preciso para flutuação neutra em água doce (1000 kg/m³)?", Ex.N(M), Ex.N(vf), Ex.N(rhof)), vfoam, "L", L.F("Faltam {0} kg de empuxo ({1} − {2}). Cada litro de espuma dá (1000 − {3})/1000 kg líquidos: V = {0} / ({4}) = {5} L.", Ex.N(dm, 3), Ex.N(M), Ex.N(Pk.RhoW * vf / 1000, 3), Ex.N(rhof), Ex.N((Pk.RhoW - rhof) / 1000, 3), Ex.N(vfoam, 3)));
            }
            default:
            {
                double duty = Ex.Step(r, 20, 60, 5) / 100, v = Ex.Pick(r, 14.8, 22.2), ah = Ex.Step(r, 10, 40, 1), usable = Ex.Pick(r, 0.7, 0.8, 0.9), wh = v * ah * usable, avg = nt * pt * duty + hotel, t = wh / avg * 60;
                return Ex.Num(L.F("Um ROV tem {0} propulsores de {1} W (usados em média {2}% do tempo), e consumo fixo de {3} W (câmera, eletrônica e luzes). A bateria é de {4} V e {5} Ah, mas só {6}% da carga é utilizável. Quantos minutos de missão?", Ex.N(nt), Ex.N(pt), Ex.N(duty * 100), Ex.N(hotel), Ex.N(v), Ex.N(ah), Ex.N(usable * 100)),
                    t, "min", L.F("P média = {0}·{1}·{2} + {3} = {4} W; energia útil = {5}·{6}·{7} = {8} Wh; t = {8}/{4} h = {9} min.", Ex.N(nt), Ex.N(pt), Ex.N(duty, 2), Ex.N(hotel), Ex.N(avg, 3), Ex.N(v), Ex.N(ah), Ex.N(usable, 2), Ex.N(wh, 3), Ex.N(t)));
            }
        }
    }
}
