using System.Text;

namespace EasyCreator.Exercises;

/// <summary>Ultrasonic sensor: echo time to distance, temperature, alarm thresholds.</summary>
[Exercises("ard-08-ultrassom")]
public sealed class Ard08Ultra : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                int us = Ex.Int(r, 2, 60) * 100;
                double cm = us * 0.0343 / 2;
                return Ex.Num(L.F("O HC-SR04 mede um eco de {0} μs (ida e volta). Qual a distância até o obstáculo, em cm? (som a 343 m/s = 0,0343 cm/μs)", us), cm, "cm", $"d = t·v / 2 = {us}·0,0343 / 2 = {P.N(cm)} cm (divide por 2 porque o som vai e volta).".Replace("0,0343", P.N(0.0343, 4)));
            }
            case Difficulty.Medium:
            {
                double cm = Ex.Step(r, 5, 300, 5);
                if (r.Next(2) == 0)
                {
                    double us = 2 * cm / 0.0343;
                    return Ex.Num(L.F("Um obstáculo está a {0} cm do sensor ultrassônico. Quanto tempo, em μs, leva o eco? (som a 343 m/s)", P.N(cm, 0)), us, "μs", $"t = 2d / v = 2·{P.N(cm, 0)} / {P.N(0.0343, 4)} = {P.N(us, 0)} μs.");
                }
                double T = Ex.Step(r, -10, 40, 5), c = 331.3 + 0.606 * T;
                return Ex.Num(L.F("Qual a velocidade do som no ar a {0} °C, em m/s? (use v = 331,3 + 0,606·T)", P.N(T, 0)), c, "m/s", $"v = 331,3 + 0,606·{P.N(T, 0)} = {P.N(c, 1)} m/s.".Replace("331,3", P.N(331.3, 1)).Replace("0,606", P.N(0.606, 3)));
            }
            case Difficulty.Hard:
            {
                double T = Ex.Step(r, -10, 40, 5), c = 331.3 + 0.606 * T, us = Ex.Int(r, 5, 40) * 100;
                double dTrue = us * c / 1e6 / 2 * 100, dAssumed = us * 0.0343 / 2;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("A {0} °C, um eco de {1} μs chega ao sensor. Qual a distância real, em cm? (v = 331,3 + 0,606·T m/s)", P.N(T, 0), us), dTrue, "cm", $"v = {P.N(c, 1)} m/s = {P.N(c / 1e4, 5)} cm/μs; d = {us}·{P.N(c / 1e4, 5)} / 2 = {P.N(dTrue)} cm.");
                return Ex.Num(L.F("Um sketch supõe som a 343 m/s e lê um eco de {0} μs, mas a temperatura é {1} °C (v = 331,3 + 0,606·T m/s). Qual o erro da distância calculada (calculada − real), em cm?", us, P.N(T, 0)), dAssumed - dTrue, "cm", $"Calculada: {us}·0,0343/2 = {P.N(dAssumed)} cm. Real: {us}·{P.N(c / 1e4, 5)}/2 = {P.N(dTrue)} cm. Erro = {P.N(dAssumed - dTrue)} cm.".Replace("0,0343", P.N(0.0343, 4)), 5);
            }
            default:
            {
                // Typical alarm program: echo time to centimetres with integer math (duration / 58), count the alarms.
                int lim = Ex.Pick(r, 20, 30, 50);
                var echo = Enumerable.Range(0, 6).Select(_ => Ex.Int(r, 3, 40) * 100).ToList();
                var cms = echo.Select(e => e / 58).ToList();
                string code = Cx.Prog($"    int eco[] = {{{Cx.Ints(echo)}}};\n    int alarmes = 0;\n    for (int i = 0; i < 6; i++) {{\n        int cm = eco[i] / 58;\n        printf(\"%d\\n\", cm);\n        if (cm < {lim}) {{\n            alarmes++;\n        }}\n    }}\n    printf(\"alarmes=%d\\n\", alarmes);\n");
                return Cx.Output(L.T("O sketch converte o tempo de eco em cm com a regra prática eco / 58 (divisão inteira) e liga o alarme abaixo de um limite. O que o programa imprime?"), code, string.Join("\n", cms) + "\nalarmes=" + cms.Count(c => c < lim),
                    L.F("Cada eco vira cm = eco / 58 (inteiro). O alarme conta as distâncias menores que {0} cm.", lim));
            }
        }
    }
}

/// <summary>DC motor with an H-bridge: PWM speed, direction logic, wheel speed, power.</summary>
[Exercises("ard-09-motor-dc")]
public sealed class Ard09Motor : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                if (r.Next(4) == 0)
                {
                    bool in1 = r.Next(2) == 0, in2 = r.Next(2) == 0;
                    string right = in1 == in2 ? L.T("Parado (freio)") : in1 ? L.T("Gira num sentido (frente)") : L.T("Gira no sentido oposto (ré)");
                    return Ex.Choice(r, L.F("Numa ponte H, com IN1 = {0} e IN2 = {1} (e o pino de habilitação em PWM alto), o que o motor faz?", in1 ? "HIGH" : "LOW", in2 ? "HIGH" : "LOW"), right,
                        new[] { L.T("Parado (freio)"), L.T("Gira num sentido (frente)"), L.T("Gira no sentido oposto (ré)") },
                        L.T("Níveis iguais nos dois pinos não geram diferença de tensão no motor (parado). Níveis diferentes aplicam tensão num sentido ou no outro."));
                }
                double rpm = Ex.Step(r, 50, 300, 10), duty = Ex.Step(r, 10, 90, 5);
                return Ex.Num(L.F("Um motor gira a {0} rpm com PWM de 100%. Supondo velocidade proporcional ao PWM, quantos rpm com {1}%?", P.N(rpm, 0), P.N(duty, 0)), rpm * duty / 100, "rpm", $"{P.N(rpm, 0)}·{P.N(duty, 0)}/100 = {P.N(rpm * duty / 100)} rpm.");
            }
            case Difficulty.Medium:
            {
                double rpm = Ex.Pick(r, 60.0, 90, 120, 150, 200, 240), D = Ex.Pick(r, 4.0, 5, 6.5, 8, 10);
                double v = rpm / 60 * Math.PI * D / 100;
                return Ex.Num(L.F("Uma roda de {0} cm de diâmetro gira a {1} rpm. Qual a velocidade linear do robô, em m/s?", P.N(D, 1), P.N(rpm, 0)), v, "m/s", $"v = (rpm/60)·π·D = ({P.N(rpm, 0)}/60)·π·{P.N(D, 1)} cm = {P.N(v * 100)} cm/s = {P.N(v, 3)} m/s.");
            }
            case Difficulty.Hard:
            {
                double V = Ex.Pick(r, 6.0, 9, 12), I = Ex.Step(r, 0.2, 2, 0.1);
                switch (r.Next(3))
                {
                    case 0:
                        return Ex.Num(L.F("Um motor de {0} V consome {1} A. Qual a potência elétrica, em watts?", P.N(V, 0), P.N(I, 1)), V * I, "W", $"P = V·I = {P.N(V, 0)}·{P.N(I, 1)} = {P.N(V * I)} W.");
                    case 1:
                        double C = Ex.Pick(r, 1000.0, 1500, 2000, 2500), h = C / (I * 1000);
                        return Ex.Num(L.F("Uma bateria de {0} mAh alimenta um motor que consome {1} A de forma contínua. Quanto tempo ela dura, em horas?", P.N(C, 0), P.N(I, 1)), h, "h", $"t = capacidade / corrente = {P.N(C / 1000, 1)} Ah / {P.N(I, 1)} A = {P.N(h)} h.");
                    default:
                        double duty = Ex.Pick(r, 25.0, 50, 75, 80), vm = V * duty / 100;
                        return Ex.Num(L.F("Uma ponte H alimentada com {0} V aplica PWM de {1}% ao motor. Qual a tensão média no motor, em volts?", P.N(V, 0), P.N(duty, 0)), vm, "V", $"Vmédia = {P.N(V, 0)}·{P.N(duty, 0)}/100 = {P.N(vm)} V.");
                }
            }
            default:
            {
                // Speed command from -255..255 to a (pwm, direction) pair.
                var cmds = Enumerable.Range(0, 5).Select(_ => Ex.Int(r, -255, 255)).ToList();
                var sb = new List<string>();
                foreach (var c in cmds) sb.Add((c >= 0 ? "F " : "R ") + Math.Abs(c));
                string code = Cx.Prog($"    int comando[] = {{{Cx.Ints(cmds)}}};\n    for (int i = 0; i < 5; i++) {{\n        int c = comando[i];\n        if (c >= 0) {{\n            printf(\"F %d\\n\", c);\n        }} else {{\n            printf(\"R %d\\n\", -c);\n        }}\n    }}\n");
                return Cx.Output(L.T("Uma função recebe velocidades de −255 a 255 e decide a direção (F = frente, R = ré) e o PWM. O que o programa imprime?"), code, string.Join("\n", sb), L.T("Valor positivo ou zero: frente com o próprio valor; negativo: ré com o valor sem o sinal (−c)."));
            }
        }
    }
}

/// <summary>Line-following robot: P control and differential speeds.</summary>
[Exercises("ard-10-robo-seguidor")]
public sealed class Ard10Seguidor : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int basePwm = Ex.Pick(r, 100, 120, 150, 180);
        double kp = Ex.Pick(r, 0.5, 1.0, 1.5, 2.0, 3.0);
        switch (d)
        {
            case Difficulty.Easy:
            {
                int err = Ex.Int(r, -40, 40);
                return Ex.Num(L.F("Num controle proporcional, a correção é Kp·erro. Com Kp = {0} e erro = {1}, qual a correção?", P.N(kp, 1), err), kp * err, "", $"correção = Kp·erro = {P.N(kp, 1)}·{err} = {P.N(kp * err)}.");
            }
            case Difficulty.Medium:
            {
                int err = Ex.Int(r, -10, 10) * 5;
                double corr = kp * err;
                bool left = r.Next(2) == 0;
                double val = left ? basePwm + corr : basePwm - corr;
                return Ex.Num(L.F("Um robô tem PWM base {0} e Kp = {1}. A velocidade do motor {2} é base {3} correção (correção = Kp·erro). Com erro = {4}, qual o PWM desse motor?", basePwm, P.N(kp, 1), left ? L.T("esquerdo") : L.T("direito"), left ? "+" : "−", err), val, "", $"correção = {P.N(kp, 1)}·{err} = {P.N(corr)}; PWM = {basePwm} {(left ? "+" : "−")} ({P.N(corr)}) = {P.N(val)}.");
            }
            case Difficulty.Hard:
            {
                // Five sensors with weights -2,-1,0,1,2: the error is the weighted mean of the sensors on the line.
                var s = new int[5]; do { for (int i = 0; i < 5; i++) s[i] = r.Next(2); } while (s.Sum() == 0);
                int sum = s.Sum(), wsum = 0; for (int i = 0; i < 5; i++) wsum += s[i] * (i - 2);
                double pos = wsum / (double)sum;
                string sens = string.Join(" ", s);
                return Ex.Num(L.F("Cinco sensores de linha (da esquerda para a direita, pesos −2, −1, 0, 1, 2) leem: {0} (1 = vê a linha). Qual a posição da linha, calculada como a média ponderada dos sensores que veem a linha?", sens), pos, "", $"Soma dos pesos dos sensores ativos = {wsum}; número de sensores ativos = {sum}; posição = {wsum} / {sum} = {P.N(pos)}.", 3);
            }
            default:
            {
                // Print the left and right PWM for a list of errors, with limits 0..255.
                var errs = Enumerable.Range(0, 5).Select(_ => Ex.Int(r, -20, 20) * 5).ToList();
                int kpi = Ex.Pick(r, 1, 2, 3);
                var sb = new List<string>();
                foreach (var e in errs)
                {
                    int l = Math.Clamp(basePwm + kpi * e, 0, 255), rr = Math.Clamp(basePwm - kpi * e, 0, 255);
                    sb.Add(l + " " + rr);
                }
                string code = Cx.Prog($"    int erros[] = {{{Cx.Ints(errs)}}};\n    for (int i = 0; i < 5; i++) {{\n        int correcao = {kpi} * erros[i];\n        int esq = {basePwm} + correcao;\n        int dir = {basePwm} - correcao;\n        if (esq < 0) esq = 0;\n        if (esq > 255) esq = 255;\n        if (dir < 0) dir = 0;\n        if (dir > 255) dir = 255;\n        printf(\"%d %d\\n\", esq, dir);\n    }}\n");
                return Cx.Output(L.T("O controle P do seguidor de linha soma e subtrai a correção do PWM base, limitando cada motor a 0..255. O que o programa imprime?"), code, string.Join("\n", sb), L.T("esq = base + correção, dir = base − correção, ambos cortados entre 0 e 255."));
            }
        }
    }
}

/// <summary>Debounce: bounce time, confirmed presses.</summary>
[Exercises("ard-11-debounce")]
public sealed class Ard11Debounce : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                int bounce = Ex.Int(r, 1, 20);
                return Ex.Choice(r, L.F("Um botão mecânico quica por até {0} ms quando apertado. Qual tempo de debounce é o MÍNIMO seguro?", bounce),
                    L.F("{0} ms ou mais", bounce + 5 - 5), new[] { L.F("{0} ms ou menos", bounce), L.T("Zero: o debounce é desnecessário"), L.T("Só funciona com 1000 ms") },
                    L.T("O sinal só é confiável depois que o ruído (quique) termina, então a espera tem de cobrir todo o tempo de quique."));
            }
            case Difficulty.Medium:
            {
                int deb = Ex.Int(r, 10, 200), t = 0;
                if (r.Next(2) == 0)
                {
                    double maxHz = 1000.0 / deb;
                    return Ex.Num(L.F("Com um debounce de {0} ms, quantos apertos por segundo, no máximo, o programa consegue contar?", deb), maxHz, "apertos/s", $"1000 / {deb} = {P.N(maxHz)} apertos por segundo.");
                }
                int n = Ex.Int(r, 3, 40);
                return Ex.Num(L.F("O botão quica {0} vezes (cada quique é lido como um aperto) em cada aperto real, se não houver debounce. Em 6 apertos reais, quantos apertos o programa conta sem debounce?", n), 6.0 * n, "apertos", $"6·{n} = {6 * n} contagens, e com um debounce correto seriam apenas 6.");
            }
            case Difficulty.Hard:
            {
                // Which of the timestamps of "state changes" are accepted as presses when changes closer than deb ms are ignored?
                int deb = Ex.Pick(r, 20, 30, 50);
                var times = new List<int>(); int t = 0;
                for (int i = 0; i < 8; i++) { t += Ex.Pick(r, 3, 5, 8, 40, 60, 120); times.Add(t); }
                int accepted = 0, last = -100000;
                foreach (var x in times) if (x - last >= deb) { accepted++; last = x; }
                string code = Cx.Prog($"    int instante[] = {{{Cx.Ints(times)}}};\n    int ultimo = -100000;\n    int aceitos = 0;\n    for (int i = 0; i < 8; i++) {{\n        if (instante[i] - ultimo >= {deb}) {{\n            aceitos++;\n            ultimo = instante[i];\n        }}\n    }}\n    printf(\"%d\\n\", aceitos);\n");
                return Cx.Output(L.F("Cada número é o instante (ms) de uma mudança lida no botão. Mudanças a menos de {0} ms da última aceita são ignoradas. O que o programa imprime?", deb), code, accepted.ToString(), L.T("Só conta a mudança se já passou o tempo de debounce desde a última aceita; as outras são ruído."));
            }
            default:
            {
                // Debounce by stable reading: the state is accepted after N consecutive equal samples.
                int need = Ex.Pick(r, 3, 4);
                var s = Enumerable.Range(0, 16).Select(_ => r.Next(3) == 0 ? 0 : 1).ToList();
                int state = 0, run = 0, prev = 0, presses = 0;
                foreach (var x in s)
                {
                    if (x == prev) run++; else { run = 1; prev = x; }
                    if (run >= need && state != x) { state = x; if (x == 1) presses++; }
                }
                string code = Cx.Prog($"    int leitura[] = {{{Cx.Ints(s)}}};\n    int estavel = 0, anterior = 0, repeticoes = 0, apertos = 0;\n    for (int i = 0; i < 16; i++) {{\n        if (leitura[i] == anterior) {{\n            repeticoes++;\n        }} else {{\n            repeticoes = 1;\n            anterior = leitura[i];\n        }}\n        if (repeticoes >= {need} && estavel != leitura[i]) {{\n            estavel = leitura[i];\n            if (estavel == 1) apertos++;\n        }}\n    }}\n    printf(\"%d\\n\", apertos);\n");
                return Cx.Output(L.F("Debounce por amostras: a leitura só vale depois de {0} amostras iguais seguidas. O que o programa imprime (apertos confirmados)?", need), code, presses.ToString(), L.T("Uma mudança só é aceita depois de ficar estável pelas amostras exigidas; só as aceitas mudando para 1 contam como aperto."));
            }
        }
    }
}

/// <summary>Multitasking with millis(): periods, coincidences.</summary>
[Exercises("ard-12-multitarefa")]
public sealed class Ard12Multi : IExerciseGenerator
{
    static int Gcd(int a, int b) => b == 0 ? a : Gcd(b, a % b);

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int p1 = Ex.Pick(r, 200, 250, 300, 400, 500, 600, 1000), p2 = Ex.Pick(r, 300, 400, 500, 750, 800, 1200);
        switch (d)
        {
            case Difficulty.Easy:
            {
                int T = Ex.Int(r, 2, 20);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Uma tarefa roda a cada {0} ms usando millis(). Quantas vezes ela roda em {1} segundos?", p1, T), Math.Floor(T * 1000.0 / p1), "vezes", $"{T}·1000 / {p1} = {P.N(T * 1000.0 / p1)} → {Math.Floor(T * 1000.0 / p1)} vezes.", 0.1);
                return Ex.Num(L.F("Uma tarefa roda a cada {0} ms. Se a última vez foi em millis() = {1}, em que valor de millis() será a próxima?", p1, p1 * T), p1 * T + p1, "ms", $"{p1 * T} + {p1} = {p1 * T + p1} ms.");
            }
            case Difficulty.Medium:
            {
                int T = Ex.Int(r, 2, 12);
                int n1 = T * 1000 / p1, n2 = T * 1000 / p2;
                return Ex.Num(L.F("Dois LEDs piscam sem bloquear: o A inverte a cada {0} ms e o B a cada {1} ms. Quantas inversões acontecem, somando os dois, em {2} segundos?", p1, p2, T), n1 + n2, "inversões", $"A: {T * 1000} / {p1} → {n1}; B: {T * 1000} / {p2} → {n2}; total {n1 + n2} (divisões inteiras).", 0.1);
            }
            case Difficulty.Hard:
            {
                int g = Gcd(p1, p2), lcm = p1 / g * p2;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Duas tarefas rodam a cada {0} ms e a cada {1} ms, começando juntas em 0. Depois de quantos ms elas coincidem pela primeira vez?", p1, p2), lcm, "ms", $"É o mínimo múltiplo comum (MMC): mdc({p1}, {p2}) = {g}; MMC = {p1}·{p2} / {g} = {lcm} ms.", 0.1);
                int T = Ex.Int(r, 1, 3) * lcm / 1000 + 1;
                int coinc = (T * 1000) / lcm + 1;
                return Ex.Num(L.F("Duas tarefas rodam a cada {0} ms e a cada {1} ms, ambas já em 0 ms. Quantas vezes elas rodam no MESMO instante entre 0 e {2} ms (incluindo o 0)?", p1, p2, T * 1000), coinc, "vezes", $"Coincidem a cada MMC = {lcm} ms: 0, {lcm}, {2 * lcm}, ... até {T * 1000}: {T * 1000} / {lcm} = {(T * 1000) / lcm} → {coinc} vezes contando o 0.", 0.1);
            }
            default:
            {
                // Simulate the loop in 100 ms steps; print the timeline of events.
                int a = Ex.Pick(r, 200, 300, 400, 500, 600), b = Ex.Pick(r, 500, 600, 700, 800, 900, 1000);
                var sb = new StringBuilder();
                for (int t = 0; t <= 1500; t += 100)
                {
                    bool A = t > 0 && t % a == 0, B = t > 0 && t % b == 0;
                    sb.Append(A && B ? 'X' : A ? 'A' : B ? 'B' : '.');
                }
                string code = Cx.Prog($"    for (int t = 0; t <= 1500; t += 100) {{\n        int a = (t > 0 && t % {a} == 0);\n        int b = (t > 0 && t % {b} == 0);\n        if (a && b) {{\n            printf(\"X\");\n        }} else if (a) {{\n            printf(\"A\");\n        }} else if (b) {{\n            printf(\"B\");\n        }} else {{\n            printf(\".\");\n        }}\n    }}\n    printf(\"\\n\");\n");
                return Cx.Output(L.T("A tarefa A e a tarefa B disparam em múltiplos dos seus períodos, e o programa mostra a linha do tempo de 100 em 100 ms (X = as duas juntas). O que ele imprime?"), code, sb.ToString(), L.F("A dispara em múltiplos de {0} ms e B em múltiplos de {1} ms (o instante 0 não conta).", a, b));
            }
        }
    }
}

/// <summary>PWM acceleration ramp, soft start.</summary>
[Exercises("ard-13-rampa-pwm")]
public sealed class Ard13Rampa : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int target = (int)Ex.Step(r, 100, 255, 5), step = Ex.Int(r, 5, 30), dl = Ex.Pick(r, 10, 20, 25, 30, 40, 50);
        int steps = (int)Math.Ceiling(target / (double)step);
        switch (d)
        {
            case Difficulty.Easy:
                return Ex.Num(L.F("Uma rampa aumenta o PWM em {0} a cada passo, de 0 até {1} (ou mais). Quantos passos são necessários?", step, target), steps, "passos", $"{target} / {step} = {P.N(target / (double)step)} → {steps} passos.", 0.1);
            case Difficulty.Medium:
                return Ex.Num(L.F("A rampa soma {0} ao PWM a cada {1} ms, de 0 até {2} (ou mais). Quanto tempo leva a partida, em ms?", step, dl, target), steps * dl, "ms", $"⌈{target} / {step}⌉ = {steps} passos·{dl} ms = {steps * dl} ms.", 0.1);
            case Difficulty.Hard:
            {
                double time = Ex.Pick(r, 0.5, 1.0, 2.0, 3.0) * 1000;
                double rate = target / time * 1000;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um motor deve ir de 0 a {0} (PWM) em {1} s, com aceleração constante. Quantas unidades de PWM por segundo?", target, P.N(time / 1000, 1)), rate, "PWM/s", $"{target} / {P.N(time / 1000, 1)} = {P.N(rate)} unidades/s.");
                double perStep = rate * dl / 1000;
                return Ex.Num(L.F("Um motor deve ir de 0 a {0} (PWM) em {1} s, com aceleração constante, e o programa atualiza o PWM a cada {2} ms. Quanto somar a cada atualização?", target, P.N(time / 1000, 1), dl), perStep, "", $"Taxa = {target} / {P.N(time / 1000, 1)} = {P.N(rate)} PWM/s; por atualização: {P.N(rate)}·{dl}/1000 = {P.N(perStep, 3)}.");
            }
            default:
            {
                // Soft start and stop: up, hold, down; print the values.
                int s2 = Ex.Int(r, 30, 70), top = (int)Ex.Step(r, 100, 200, 10);
                var vals = new List<int>(); int v = 0;
                while (v < top) { vals.Add(v); v = Math.Min(top, v + s2); }
                vals.Add(top);
                while (v > 0) { v = Math.Max(0, v - s2); vals.Add(v); }
                string code = Cx.Prog($"    int v = 0;\n    while (v < {top}) {{\n        printf(\"%d\\n\", v);\n        v = v + {s2};\n        if (v > {top}) v = {top};\n    }}\n    printf(\"%d\\n\", v);\n    while (v > 0) {{\n        v = v - {s2};\n        if (v < 0) v = 0;\n        printf(\"%d\\n\", v);\n    }}\n");
                return Cx.Output(L.T("O motor sobe até a velocidade máxima e depois desce até parar, sempre limitado entre 0 e o máximo. O que o programa imprime?"), code, string.Join("\n", vals), L.T("Na subida imprime antes de somar; no topo imprime o máximo uma vez; na descida subtrai e imprime, até chegar a 0."));
            }
        }
    }
}

/// <summary>Serial protocol: baud rates, message sizes and parsing text commands.</summary>
[Exercises("ard-14-protocolo-serial")]
public sealed class Ard14Protocolo : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                int baud = Ex.Pick(r, 1200, 2400, 4800, 9600, 19200, 38400, 115200), secs = Ex.Int(r, 2, 20);
                return Ex.Num(L.F("A {0} baud (8N1: 10 bits por byte), quantos bytes a serial envia em {1} segundos?", baud, secs), baud / 10.0 * secs, "bytes", $"{baud} / 10 = {baud / 10} bytes/s; {baud / 10}·{secs} = {baud / 10 * secs} bytes.");
            }
            case Difficulty.Medium:
            {
                int baud = Ex.Pick(r, 9600, 19200, 115200), len = Ex.Int(r, 4, 30);
                double ms = (len + 1) * 10000.0 / baud;
                return Ex.Num(L.F("Um comando de {0} caracteres é enviado com println a {1} baud (println adiciona 1 byte de quebra de linha; 10 bits por byte). Quanto tempo leva, em ms?", len, baud), ms, "ms", $"({len} + 1) bytes·10 bits = {(len + 1) * 10} bits; {(len + 1) * 10} / {baud} bits/s = {P.N(ms / 1000, 5)} s = {P.N(ms, 3)} ms.");
            }
            case Difficulty.Hard:
            {
                // Parse a command of one letter plus a number.
                char cmd = Ex.Pick(r, 'M', 'S', 'L', 'V');
                int val = Ex.Int(r, 1, 255);
                string msg = cmd + val.ToString();
                string code = Cx.Prog($"    char msg[] = \"{msg}\";\n    char comando = msg[0];\n    int valor = 0;\n    for (int i = 1; msg[i] != 0; i++) {{\n        valor = valor * 10 + (msg[i] - '0');\n    }}\n    printf(\"%c=%d\\n\", comando, valor);\n");
                return Cx.Output(L.T("O Arduino recebe um texto com uma letra de comando seguida de um número, e o decodifica. O que o programa imprime?"), code, $"{cmd}={val}", L.T("O primeiro caractere é o comando; os demais são dígitos, e cada um soma (dígito − '0') ao valor acumulado × 10."));
            }
            default:
            {
                // Several commands "A10;B20;..." - sum of the values for one letter.
                int n = Ex.Int(r, 4, 6);
                var letters = Enumerable.Range(0, n).Select(_ => Ex.Pick(r, 'A', 'B')).ToList();
                var vals = Enumerable.Range(0, n).Select(_ => Ex.Int(r, 1, 99)).ToList();
                string msg = string.Join(";", letters.Select((c, i) => c + vals[i].ToString())) + ";";
                int sumA = 0, sumB = 0;
                for (int i = 0; i < n; i++) if (letters[i] == 'A') sumA += vals[i]; else sumB += vals[i];
                string code = Cx.Prog($"    char msg[] = \"{msg}\";\n    int soma[2] = {{0, 0}};\n    char atual = 0;\n    int valor = 0;\n    for (int i = 0; msg[i] != 0; i++) {{\n        char c = msg[i];\n        if (c == 'A' || c == 'B') {{\n            atual = c;\n            valor = 0;\n        }} else if (c == ';') {{\n            soma[atual - 'A'] += valor;\n        }} else {{\n            valor = valor * 10 + (c - '0');\n        }}\n    }}\n    printf(\"A=%d B=%d\\n\", soma[0], soma[1]);\n");
                return Cx.Output(L.T("O protocolo manda comandos separados por ponto e vírgula, cada um com uma letra (A ou B) e um valor. O programa soma os valores de cada letra. O que ele imprime?"), code, $"A={sumA} B={sumB}", L.T("Cada comando termina em ';': nesse momento o valor lido é somado ao total da letra atual."));
            }
        }
    }
}
