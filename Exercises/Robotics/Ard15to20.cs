using System.Text;

namespace EasyCreator.Exercises;

/// <summary>Moving average filter and hysteresis.</summary>
[Exercises("ard-15-histerese")]
public sealed class Ard15Histerese : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                var v = Enumerable.Range(0, Ex.Int(r, 3, 5)).Select(_ => Ex.Int(r, 100, 900)).ToList();
                double avg = v.Average();
                return Ex.Num(L.F("Um filtro de média faz a média de {0} leituras: {1}. Qual o resultado?", v.Count, string.Join(", ", v)), avg, "", $"({string.Join(" + ", v)}) / {v.Count} = {v.Sum()} / {v.Count} = {P.N(avg)}.");
            }
            case Difficulty.Medium:
            {
                int on = Ex.Pick(r, 400, 500, 600), off = on + Ex.Pick(r, 50, 100, 200);
                // Dark light turns the lamp on below `on`, and off above `off`.
                int x = Ex.Int(r, on - 100, off + 100);
                bool wasOn = r.Next(2) == 0;
                bool now = x < on ? true : x > off ? false : wasOn;
                string right = now ? L.T("Acesa") : L.T("Apagada");
                return Ex.Choice(r, L.F("Um poste liga a lâmpada quando a luz fica abaixo de {0} e desliga quando passa de {1} (histerese). Se a luz agora é {2} e a lâmpada estava {3}, como ela fica?", on, off, x, wasOn ? L.T("acesa") : L.T("apagada")),
                    right, new[] { L.T("Acesa"), L.T("Apagada") },
                    x < on ? L.F("{0} < {1}: liga.", x, on) : x > off ? L.F("{0} > {1}: desliga.", x, off) : L.F("{0} ≤ {1} ≤ {2}: zona de histerese, mantém o estado anterior.", on, x, off));
            }
            case Difficulty.Hard:
            {
                int n = Ex.Pick(r, 4, 5, 8);
                var window = Enumerable.Range(0, n).Select(_ => Ex.Int(r, 200, 800)).ToList();
                int newV = Ex.Int(r, 200, 800);
                double before = window.Average();
                var after = window.Skip(1).Append(newV).ToList();
                return Ex.Num(L.F("Uma média móvel de {0} leituras guarda: {1}. Chega a leitura {2} (a mais antiga sai). Qual a nova média?", n, string.Join(", ", window), newV), after.Average(), "", L.F("Soma antiga = {0}; nova soma = {1} − {2} + {3} = {4}; média = {5} / {6} = {7}.", window.Sum(), window.Sum(), window[0], newV, after.Sum(), after.Sum(), n, P.N(after.Average())));
            }
            default:
            {
                int lo = Ex.Pick(r, 300, 400), hi = lo + Ex.Pick(r, 100, 150);
                var readings = Enumerable.Range(0, 10).Select(_ => Ex.Int(r, 250, 550)).ToList();
                var sb = new StringBuilder(); int state = 0;
                foreach (var x in readings)
                {
                    if (x < lo) state = 1; else if (x > hi) state = 0;
                    sb.Append(state);
                }
                string code = Cx.Prog($"    int leitura[] = {{{Cx.Ints(readings)}}};\n    int ligado = 0;\n    for (int i = 0; i < 10; i++) {{\n        if (leitura[i] < {lo}) {{\n            ligado = 1;\n        }} else if (leitura[i] > {hi}) {{\n            ligado = 0;\n        }}\n        printf(\"%d\", ligado);\n    }}\n    printf(\"\\n\");\n");
                return Cx.Output(L.T("A lâmpada liga abaixo do limite inferior, desliga acima do superior e, entre os dois, mantém o estado. O que o programa imprime?"), code, sb.ToString(), L.F("Liga se a leitura for menor que {0}, desliga se for maior que {1}; entre os dois mantém o estado anterior.", lo, hi));
            }
        }
    }
}

/// <summary>Alarm state machine: arm, trigger, disarm.</summary>
[Exercises("ard-16-alarme")]
public sealed class Ard16Alarme : IExerciseGenerator
{
    // States: 0 desarmado, 1 armado, 2 disparado. Events: A (arma), D (desarma), S (sensor).
    static int Next(int st, char ev) => (st, ev) switch
    {
        (0, 'A') => 1,
        (1, 'S') => 2,
        (1, 'D') => 0,
        (2, 'D') => 0,
        _ => st,
    };

    static string Name(int s) => s == 0 ? L.T("Desarmado") : s == 1 ? L.T("Armado") : L.T("Disparado");

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        string rules = L.T("Estados: Desarmado, Armado, Disparado. Evento A (arma): Desarmado → Armado. Evento S (sensor): Armado → Disparado. Evento D (senha): Armado ou Disparado → Desarmado. Qualquer outro evento não muda nada.");
        switch (d)
        {
            case Difficulty.Easy:
            {
                int st = r.Next(3); var evl = Enumerable.Range(0, 3).Select(_ => Ex.Pick(r, (char)65, (char)83, (char)68)).ToList();
                int nx = st; foreach (var e in evl) nx = Next(nx, e);
                return Ex.Choice(r, rules + " " + L.F("Estando {0}, chegam os eventos {1}, nessa ordem. Qual o estado no final?", Name(st).ToLowerInvariant(), string.Join(" ", evl)), Name(nx), new[] { Name(0), Name(1), Name(2) }, $"{Name(st)} + {string.Join(" ", evl)} → {Name(nx)}.");
            }
            case Difficulty.Medium:
            case Difficulty.Hard:
            {
                int len = d == Difficulty.Medium ? 5 : 9;
                var evs = Enumerable.Range(0, len).Select(_ => Ex.Pick(r, 'A', 'S', 'D', 'S')).ToList();
                int st = 0; var trace = new List<string> { Name(0) };
                foreach (var e in evs) { st = Next(st, e); trace.Add(Name(st)); }
                return Ex.Choice(r, rules + " " + L.F("O sistema começa desarmado e recebe os eventos {0}. Em que estado termina?", string.Join(" ", evs)), Name(st), new[] { Name(0), Name(1), Name(2) }, string.Join(" → ", trace) + ".");
            }
            default:
            {
                int len = 14;
                var evs = Enumerable.Range(0, len).Select(_ => Ex.Pick(r, 'A', 'S', 'D', 'S', 'A')).ToList();
                int st = 0, triggers = 0;
                foreach (var e in evs) { int n = Next(st, e); if (n == 2 && st != 2) triggers++; st = n; }
                string code = Cx.Prog($"    char eventos[] = \"{new string(evs.ToArray())}\";\n    int estado = 0, disparos = 0;\n    for (int i = 0; eventos[i] != 0; i++) {{\n        char e = eventos[i];\n        if (estado == 0 && e == 'A') {{\n            estado = 1;\n        }} else if (estado == 1 && e == 'S') {{\n            estado = 2;\n            disparos++;\n        }} else if (estado != 0 && e == 'D') {{\n            estado = 0;\n        }}\n    }}\n    printf(\"%d %d\\n\", estado, disparos);\n");
                return Cx.Output(L.T("A máquina de estados (0 = desarmado, 1 = armado, 2 = disparado) processa a sequência de eventos A, S e D. O programa imprime o estado final e quantas vezes disparou. O que ele imprime?"), code, $"{st} {triggers}", L.T("Cada evento só tem efeito no estado certo: A arma (de 0), S dispara (de 1), D desarma (de 1 ou 2)."));
            }
        }
    }
}

/// <summary>Solar tracker: difference of two light sensors, dead zone, servo steps.</summary>
[Exercises("ard-17-seguidor-solar")]
public sealed class Ard17Solar : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int zone = Ex.Pick(r, 20, 30, 50);
        switch (d)
        {
            case Difficulty.Easy:
            {
                int a = Ex.Int(r, 200, 800), b = Ex.Int(r, 200, 800);
                return Ex.Num(L.F("Os sensores de luz esquerdo e direito leem {0} e {1}. Qual a diferença (esquerdo − direito)?", a, b), a - b, "", $"{a} − {b} = {a - b}.", 0.1);
            }
            case Difficulty.Medium:
            {
                int a = Ex.Int(r, 300, 700), b = Ex.Int(r, 300, 700), diff = a - b;
                string right = Math.Abs(diff) <= zone ? L.T("Fica parado") : diff > 0 ? L.T("Gira para o lado esquerdo") : L.T("Gira para o lado direito");
                return Ex.Choice(r, L.F("Um seguidor solar compara o sensor esquerdo ({0}) com o direito ({1}); se a diferença for menor ou igual a {2} em módulo (zona morta), ele fica parado; senão gira para o lado mais claro. O que faz?", a, b, zone),
                    right, new[] { L.T("Fica parado"), L.T("Gira para o lado esquerdo"), L.T("Gira para o lado direito") },
                    L.F("Diferença = {0} − {1} = {2}; |{2}| ", a, b, diff) + (Math.Abs(diff) <= zone ? L.F("≤ {0}: zona morta.", zone) : L.F("> {0}: gira para o lado com mais luz.", zone)));
            }
            case Difficulty.Hard:
            {
                int step = Ex.Pick(r, 1, 2, 5), a0 = Ex.Int(r, 60, 120), n = Ex.Int(r, 5, 25);
                bool toLeft = r.Next(2) == 0;
                int ang = Math.Clamp(a0 + (toLeft ? 1 : -1) * step * n, 0, 180);
                return Ex.Num(L.F("O servo está em {0}° e, a cada ciclo, o rastreador o move {1}° para {2} (limitado a 0..180°). Em que ângulo ele está depois de {3} ciclos?", a0, step, toLeft ? L.T("o lado do ângulo maior") : L.T("o lado do ângulo menor"), n), ang, "°", $"{a0} {(toLeft ? "+" : "−")} {step}·{n} = {a0 + (toLeft ? 1 : -1) * step * n}" + (ang != a0 + (toLeft ? 1 : -1) * step * n ? L.F(" → limitado a {0}°.", ang) : "°."), 0.1);
            }
            default:
            {
                // Simulate: the light source is at `sun`; the sensors read more when the servo points closer to it.
                int sun = Ex.Int(r, 40, 140), start = Ex.Int(r, 20, 160), step = Ex.Pick(r, 5, 10), zoneDeg = Ex.Pick(r, 4, 6);
                var angles = new List<int>(); int a = start;
                for (int i = 0; i < 10; i++)
                {
                    int diff = sun - a;
                    if (Math.Abs(diff) > zoneDeg) a += diff > 0 ? step : -step;
                    a = Math.Clamp(a, 0, 180);
                    angles.Add(a);
                }
                string code = Cx.Prog($"    int sol = {sun}, angulo = {start};\n    for (int i = 0; i < 10; i++) {{\n        int dif = sol - angulo;\n        if (dif > {zoneDeg}) {{\n            angulo = angulo + {step};\n        }} else if (dif < -{zoneDeg}) {{\n            angulo = angulo - {step};\n        }}\n        if (angulo < 0) angulo = 0;\n        if (angulo > 180) angulo = 180;\n        printf(\"%d\\n\", angulo);\n    }}\n");
                return Cx.Output(L.F("O rastreador gira o servo {0}° por ciclo em direção ao sol (a {1}°), mas fica parado dentro da zona morta de ±{2}°. O que o programa imprime nos 10 ciclos?", step, sun, zoneDeg), code, string.Join("\n", angles), L.T("Enquanto a diferença for maior que a zona morta, o ângulo anda um passo em direção ao sol; dentro dela, fica parado."));
            }
        }
    }
}

/// <summary>PID control: a step of the controller.</summary>
[Exercises("ard-18-pid-distancia")]
public sealed class Ard18Pid : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double sp = 20;
        switch (d)
        {
            case Difficulty.Easy:
            {
                double dist = Ex.Step(r, 21, 120, 1), err = dist - sp;
                return Ex.Num(L.F("O robô deve parar a 20 cm da parede e mede {0} cm. Qual o erro (medida − alvo)?", P.N(dist, 0)), err, "cm", L.F("erro = {0} − 20 = {1} cm.", P.N(dist, 0), P.N(err, 0)));
            }
            case Difficulty.Medium:
            {
                double dist = Ex.Step(r, 21, 120, 1), kp = Ex.Pick(r, 0.5, 1.0, 2.0, 3.0), err = dist - sp;
                return Ex.Num(L.F("O robô mede {0} cm da parede (alvo: 20 cm) e usa um controle P com Kp = {1} sobre erro = medida − alvo. Qual a saída do controle?", P.N(dist, 0), P.N(kp, 1)), kp * err, "", L.F("erro = {0} − 20 = {1}; saída = Kp·erro = {2}·{3} = {4}.", P.N(dist, 0), P.N(err, 0), P.N(kp, 1), P.N(err, 0), P.N(kp * err)));
            }
            case Difficulty.Hard:
            {
                double kp = Ex.Pick(r, 1.0, 2.0, 3.0), ki = Ex.Pick(r, 0.1, 0.2, 0.5), kd = Ex.Pick(r, 0.5, 1.0, 2.0), dt = Ex.Pick(r, 0.1, 0.2, 0.5);
                double e0 = Ex.Step(r, 5, 40, 5), e1 = Ex.Step(r, 0, 40, 5), integral = Ex.Step(r, 0, 20, 2);
                double integ = integral + e1 * dt, der = (e1 - e0) / dt, u = kp * e1 + ki * integ + kd * der;
                return Ex.Num(L.F("Um PID (Kp = {0}, Ki = {1}, Kd = {2}) roda a cada {3} s. O erro anterior foi {4}, o erro atual é {5} e a integral até o ciclo anterior vale {6}. Qual a saída do PID neste ciclo? (integral += erro·dt; derivada = (erro − erro anterior)/dt)", P.N(kp, 1), P.N(ki, 1), P.N(kd, 1), P.N(dt, 1), P.N(e0, 0), P.N(e1, 0), P.N(integral, 0)),
                    u, "", L.F("integral = {0} + {1}·{2} = {3}; derivada = ({4} − {5}) / {6} = {7}; u = {8}·{9} + {10}·{11} + {12}·{13} = {14}.", P.N(integral, 0), P.N(e1, 0), P.N(dt, 1), P.N(integ, 2), P.N(e1, 0), P.N(e0, 0), P.N(dt, 1), P.N(der, 2), P.N(kp, 1), P.N(e1, 0), P.N(ki, 1), P.N(integ, 2), P.N(kd, 1), P.N(der, 2), P.N(u)));
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    // Steady-state error of a P-only control against a constant disturbance.
                    double kp = Ex.Step(r, 1, 10, 0.5), dist = Ex.Step(r, 5, 50, 5), err = dist / kp;
                    return Ex.Num(L.F("Num sistema, a saída do controle P (Kp = {0}) precisa vencer uma força constante que exige saída = {1} para manter o robô parado. Qual o erro que sobra em regime (o que o termo integral eliminaria)?", P.N(kp, 0), P.N(dist, 0)), err, "", L.F("Em repouso Kp·erro = {0} → erro = {1} / {2} = {3}. Um termo integral acumula esse erro até zerá-lo.", P.N(dist, 0), P.N(dist, 0), P.N(kp, 0), P.N(err)));
                }
                int kpI = Ex.Int(r, 1, 5), kiI = Ex.Int(r, 1, 3), start = Ex.Int(r, 50, 120);
                // Integer PID on a toy plant: position += output / 10.
                double pos = start; double integ = 0; var outs = new List<int>();
                for (int i = 0; i < 6; i++)
                {
                    int e = (int)pos - 20;
                    integ += e;
                    int u = kpI * e + kiI * (int)integ;
                    pos -= u / 10;
                    outs.Add(u);
                }
                string code = Cx.Prog($"    int posicao = {start}, integral = 0;\n    for (int i = 0; i < 6; i++) {{\n        int erro = posicao - 20;\n        integral = integral + erro;\n        int u = {kpI} * erro + {kiI} * integral;\n        printf(\"%d\\n\", u);\n        posicao = posicao - u / 10;\n    }}\n");
                return Cx.Output(L.T("Um PID inteiro (Kp e Ki) leva o robô para 20 cm; a cada ciclo a posição diminui de u / 10 (divisão inteira). O que o programa imprime?"), code, string.Join("\n", outs), L.T("Em cada ciclo: erro = posição − 20; a integral soma o erro; u = Kp·erro + Ki·integral; a posição muda de u / 10."));
            }
        }
    }
}

/// <summary>Odometry of a differential-drive robot.</summary>
[Exercises("ard-19-odometria")]
public sealed class Ard19Odometria : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double D = Ex.Pick(r, 4.0, 5, 6, 6.5, 8), N = Ex.Pick(r, 20.0, 40, 60, 100, 360);
        double perTick = Math.PI * D / N;
        switch (d)
        {
            case Difficulty.Easy:
                return Ex.Num(L.F("Uma roda de {0} cm de diâmetro tem um encoder de {1} pulsos por volta. Que distância (em cm) corresponde a cada pulso?", P.N(D, 1), P.N(N, 0)), perTick, "cm", L.F("Perímetro = π·{0} = {1} cm; por pulso: {2} / {3} = {4} cm.", P.N(D, 1), P.N(Math.PI * D), P.N(Math.PI * D), P.N(N, 0), P.N(perTick, 4)));
            case Difficulty.Medium:
            {
                int ticks = Ex.Int(r, 2, 40) * 50;
                return Ex.Num(L.F("Roda de {0} cm de diâmetro, encoder de {1} pulsos por volta. O robô contou {2} pulsos. Quantos centímetros andou?", P.N(D, 1), P.N(N, 0), ticks), ticks * perTick, "cm", L.F("d = pulsos·π·D / N = {0}·π·{1} / {2} = {3} cm.", ticks, P.N(D, 1), P.N(N, 0), P.N(ticks * perTick)));
            }
            case Difficulty.Hard:
            {
                double b = Ex.Pick(r, 8.0, 10, 12, 15), dl = Ex.Step(r, 5, 40, 1), dr = Ex.Step(r, 5, 40, 1);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um robô com {0} cm entre as rodas andou {1} cm com a roda esquerda e {2} cm com a direita. Quanto ele girou, em graus (positivo = anti-horário)?", P.N(b, 0), P.N(dl), P.N(dr)), P.Deg((dr - dl) / b), "°", $"Δθ = (dD − dE) / b = ({P.N(dr)} − {P.N(dl)}) / {P.N(b, 0)} = {P.N((dr - dl) / b, 4)} rad = {P.N(P.Deg((dr - dl) / b))}°.");
                return Ex.Num(L.F("Um robô com rodas distantes {0} cm andou {1} cm com a roda esquerda e {2} cm com a direita. Quanto andou o seu centro, em cm?", P.N(b, 0), P.N(dl), P.N(dr)), (dl + dr) / 2, "cm", $"d = (dE + dD) / 2 = ({P.N(dl)} + {P.N(dr)}) / 2 = {P.N((dl + dr) / 2)} cm.");
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    // Radius of the arc and 90-degree-turn distance: the outer wheel travels more.
                    double b = Ex.Pick(r, 10.0, 12, 15), R = Ex.Step(r, 20, 60, 5), ang = Ex.Pick(r, 90.0, 180, 60);
                    double dOuter = (R + b / 2) * P.Rad(ang);
                    return Ex.Num(L.F("Um robô com rodas distantes {0} cm faz uma curva de {1}° em volta de um arco de {2} cm de raio (medido no centro do robô). Quantos centímetros a roda de FORA percorre?", P.N(b, 0), P.N(ang, 0), P.N(R, 0)), dOuter, "cm", L.F("A roda de fora tem raio R + b/2 = {0} cm; arco = raio·ângulo = {1}·{2} = {3} cm.", P.N(R + b / 2), P.N(R + b / 2), P.N(P.Rad(ang), 4), P.N(dOuter)));
                }
                // Dead reckoning with integer math: x += d cos θ, y += d sin θ for four straight-and-turn steps (θ in multiples of 90°).
                var steps = Enumerable.Range(0, 5).Select(_ => (Ex.Int(r, 1, 9) * 10, Ex.Pick(r, 0, 90, 180, 270))).ToList();
                int x = 0, y = 0;
                var dxs = new[] { 1, 0, -1, 0 }; var dys = new[] { 0, 1, 0, -1 };
                string dist = string.Join(", ", steps.Select(s => s.Item1)), angs = string.Join(", ", steps.Select(s => s.Item2));
                foreach (var (dd, a) in steps) { x += dd * dxs[a / 90]; y += dd * dys[a / 90]; }
                string code = Cx.Prog($"    int dist[] = {{{dist}}};\n    int ang[] = {{{angs}}};\n    int dx[] = {{1, 0, -1, 0}};\n    int dy[] = {{0, 1, 0, -1}};\n    int x = 0, y = 0;\n    for (int i = 0; i < 5; i++) {{\n        int k = ang[i] / 90;\n        x = x + dist[i] * dx[k];\n        y = y + dist[i] * dy[k];\n    }}\n    printf(\"%d %d\\n\", x, y);\n");
                return Cx.Output(L.T("O robô anda trechos retos (dist, em cm) em direções múltiplas de 90° (ang) e atualiza a posição (x, y) por odometria. O que o programa imprime?"), code, $"{x} {y}", L.T("Direção 0° soma em x, 90° soma em y, 180° subtrai de x e 270° subtrai de y."));
            }
        }
    }
}

/// <summary>Final mission: obstacle avoidance and return to the line.</summary>
[Exercises("ard-20-missao-final")]
public sealed class Ard20Missao : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        double v = Ex.Pick(r, 10.0, 15, 20, 25, 30);
        switch (d)
        {
            case Difficulty.Easy:
            {
                double dist = Ex.Step(r, 20, 200, 10);
                return Ex.Num(L.F("O robô anda a {0} cm/s. Em quantos segundos ele percorre {1} cm?", P.N(v, 0), P.N(dist, 0)), dist / v, "s", $"t = d / v = {P.N(dist, 0)} / {P.N(v, 0)} = {P.N(dist / v)} s.");
            }
            case Difficulty.Medium:
            {
                double side = Ex.Step(r, 10, 40, 5), turn = Ex.Pick(r, 90.0, 60);
                // Dodge: turn, go around, come back: three straight legs of the detour (a rectangle detour has 3 sides).
                double path = 2 * side + side;
                return Ex.Num(L.F("Para desviar de uma caixa, o robô faz um contorno retangular de três lados de {0} cm cada (sai da linha, passa ao lado, volta). A {1} cm/s, quanto tempo dura o contorno, ignorando as curvas?", P.N(side, 0), P.N(v, 0)), path / v, "s", L.F("Percurso = 3·{0} = {1} cm; t = {2} / {3} = {4} s.", P.N(side, 0), P.N(path, 0), P.N(path, 0), P.N(v, 0), P.N(path / v)));
            }
            case Difficulty.Hard:
            {
                double side = Ex.Step(r, 10, 60, 1), box = Ex.Step(r, 5, 40, 1);
                // Detour around a box of width `box` with a sideways step `side` out and back: sides = side + (box + 2*margin) + side.
                double margin = 5, mid = box + 2 * margin, total = 2 * side + mid;
                return Ex.Num(L.F("O robô desvia de uma caixa de {0} cm de largura (na direção do desvio) com folga de 5 cm de cada lado: anda {1} cm para o lado, depois passa pela caixa e volta {1} cm para a linha. Quantos cm ele percorre no desvio inteiro?", P.N(box, 0), P.N(side, 0)), total, "cm", L.F("Trecho ao lado da caixa: {0} + 2·5 = {1} cm; total = {2} + {3} + {4} = {5} cm.", P.N(box, 0), P.N(mid, 0), P.N(side, 0), P.N(mid, 0), P.N(side, 0), P.N(total, 0)));
            }
            default:
            {
                // State machine of the mission, printed step by step: FOLLOW until an obstacle < limit, DODGE for N ticks, RETURN until the line is seen.
                int lim = Ex.Pick(r, 15, 20), dodge = Ex.Pick(r, 2, 3);
                var dists = Enumerable.Range(0, 12).Select(i => i is >= 3 and <= 5 ? Ex.Int(r, 5, lim - 1) : Ex.Int(r, lim + 5, 80)).ToList();
                var lineSeen = Enumerable.Range(0, 12).Select(i => r.Next(2) == 0).ToList();
                var sb = new StringBuilder(); int st = 0, cnt = 0;
                for (int i = 0; i < 12; i++)
                {
                    if (st == 0 && dists[i] < lim) { st = 1; cnt = 0; }
                    else if (st == 1) { cnt++; if (cnt >= dodge) st = 2; }
                    else if (st == 2 && lineSeen[i]) st = 0;
                    sb.Append(st);
                }
                string code = Cx.Prog($"    int dist[] = {{{Cx.Ints(dists)}}};\n    int linha[] = {{{Cx.Ints(lineSeen.Select(b => b ? 1 : 0))}}};\n    int estado = 0, cont = 0;\n    for (int i = 0; i < 12; i++) {{\n        if (estado == 0 && dist[i] < {lim}) {{\n            estado = 1;\n            cont = 0;\n        }} else if (estado == 1) {{\n            cont++;\n            if (cont >= {dodge}) estado = 2;\n        }} else if (estado == 2 && linha[i] == 1) {{\n            estado = 0;\n        }}\n        printf(\"%d\", estado);\n    }}\n    printf(\"\\n\");\n");
                return Cx.Output(L.T("Estados da missão: 0 = seguir a linha, 1 = desviar, 2 = voltar à linha. O programa imprime o estado a cada ciclo. O que ele imprime?"), code, sb.ToString(), L.F("Segue (0) até uma distância menor que {0} cm; desvia (1) por {1} ciclos; volta (2) até o sensor ver a linha.", lim, dodge));
            }
        }
    }
}
