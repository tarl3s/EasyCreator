using System.Text;

namespace EasyCreator.Exercises;

/// <summary>Pieces shared by the Arduino generators: C programs for "what does it print?" and a few unit helpers.</summary>
static class Cx
{
    public static string Prog(string body, string globals = "") =>
        "#include <stdio.h>\n\n" + (globals.Length > 0 ? globals + "\n\n" : "") + "int main() {\n" + body + "    return 0;\n}\n";

    public static Exercise Output(string prompt, string code, string expected, string explanation) =>
        Ex.Output(prompt, code, expected, explanation);
    public static string Ints(IEnumerable<int> items) => string.Join(", ", items);
}

/// <summary>LED and resistor: Ohm's law, blink timing, LED series, "what does the blink loop print".</summary>
[Exercises("ard-01-pisca-led")]
public sealed class Ard01Led : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                double vs = Ex.Pick(r, 5.0, 3.3, 9, 12), vf = Ex.Pick(r, 1.8, 2.0, 2.2, 3.0), i = Ex.Pick(r, 10.0, 15, 20) / 1000;
                if (vs - vf < 1) vs = 5;
                if (vs == 3.3 && vf > 2.2) vf = 2.0;
                double R = (vs - vf) / i;
                return Ex.Num(L.F("Um LED de {0} V de queda de tensão deve conduzir {1} mA numa fonte de {2} V. Qual o resistor em série necessário, em ohms?", P.N(vf, 1), P.N(i * 1000, 0), P.N(vs, 1)), R, "Ω", L.F("R = (Vfonte − Vled) / I = ({0} − {1}) / {2} = {3} Ω.", P.N(vs, 1), P.N(vf, 1), P.N(i, 3), P.N(R, 0)));
            }
            case Difficulty.Medium:
            {
                int on = Ex.Int(r, 1, 20) * 100, off = Ex.Int(r, 1, 20) * 100;
                double per = on + off;
                switch (r.Next(3))
                {
                    case 0:
                        return Ex.Num(L.F("Um sketch liga o LED, espera {0} ms, desliga e espera {1} ms (delay), repetindo. Qual a frequência do pisca, em Hz?", on, off), 1000.0 / per, "Hz", L.F("Período = {0} + {1} = {2} ms; f = 1000 / {3} = {4} Hz.", on, off, per, per, P.N(1000.0 / per, 3)));
                    case 1:
                        int secs = Ex.Int(r, 5, 60);
                        double n = secs * 1000.0 / per;
                        return Ex.Num(L.F("Um LED fica {0} ms aceso e {1} ms apagado, em repetição. Quantas vezes ele acende em {2} segundos?", on, off, secs), n, "vezes", L.F("Período = {0} ms; {1}·1000 / {2} = {3} vezes.", per, secs, per, P.N(n)));
                    default:
                        double duty = on / per * 100;
                        return Ex.Num(L.F("Um LED fica {0} ms aceso e {1} ms apagado em cada ciclo. Qual a fração do tempo em que ele está aceso, em %?", on, off), duty, "%", $"{on} / ({on} + {off}) = {P.N(duty)}%.");
                }
            }
            case Difficulty.Hard:
            {
                double vs = 5, vf = Ex.Pick(r, 1.8, 2.0, 2.2, 2.4, 3.0), R = Ex.Step(r, 100, 1000, 10);
                double i = (vs - vf) / R;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um LED de {0} V é ligado em 5 V com um resistor de {1} Ω. Qual a corrente, em mA?", P.N(vf, 1), P.N(R, 0)), i * 1000, "mA", $"I = (5 − {P.N(vf, 1)}) / {P.N(R, 0)} = {P.N(i, 4)} A = {P.N(i * 1000)} mA.");
                double p = i * i * R;
                return Ex.Num(L.F("Um LED de {0} V é ligado em 5 V com um resistor de {1} Ω. Qual a potência dissipada no resistor, em mW?", P.N(vf, 1), P.N(R, 0)), p * 1000, "mW", $"I = {P.N(i * 1000)} mA; P = I²·R = {P.N(i, 4)}²·{P.N(R, 0)} = {P.N(p * 1000)} mW.");
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    // N identical LEDs in series from a higher voltage: only one resistor, and the drops add up.
                    int n = Ex.Int(r, 2, 4);
                    double vf = Ex.Pick(r, 1.8, 2.0, 2.2), vs = Ex.Pick(r, 9.0, 12), i = Ex.Pick(r, 10.0, 15, 20) / 1000;
                    double R = (vs - n * vf) / i;
                    return Ex.Num(L.F("{0} LEDs iguais de {1} V ficam em série com um único resistor, ligados a uma fonte de {2} V. Para circular {3} mA, qual o valor do resistor, em ohms?", n, P.N(vf, 1), P.N(vs, 0), P.N(i * 1000, 0)), R, "Ω", L.F("As quedas somam: {0}·{1} = {2} V. R = ({3} − {4}) / {5} = {6} Ω.", n, P.N(vf, 1), P.N(n * vf, 1), P.N(vs, 0), P.N(n * vf, 1), P.N(i, 3), P.N(R, 0)));
                }
                int times = Ex.Int(r, 4, 9), pin = Ex.Int(r, 1, 3);
                var sb = new StringBuilder();
                for (int k = 0; k < times; k++) sb.Append(k % 2 == 0 ? "1" : "0");
                string code = Cx.Prog($"    int led = 0;\n    for (int i = 0; i < {times}; i++) {{\n        led = !led;\n        printf(\"%d\", led);\n    }}\n    printf(\"\\n\");\n");
                // led starts at 0 and toggles first, so it prints 1,0,1,0...
                return Cx.Output(L.T("Este programa simula o LED a cada vez que o loop() roda (1 = aceso). O que ele imprime?"), code, sb.ToString(), L.T("led começa em 0 e é invertido (!) a cada volta, antes de imprimir: 1, 0, 1, 0, ... na quantidade de voltas do for."));
            }
        }
    }
}

/// <summary>Digital input: pull-up, pull-down, edge counting.</summary>
[Exercises("ard-02-botao")]
public sealed class Ard02Botao : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                if (r.Next(2) == 0)
                    return Ex.Choice(r, L.F("Com pinMode({0}, INPUT_PULLUP) e o botão ligando o pino {0} ao GND, o que digitalRead({0}) devolve com o botão APERTADO?", r.Next(2, 13)), "LOW", new[] { "HIGH", L.T("Um valor entre 0 e 1023") },
                        L.T("O resistor interno puxa o pino para HIGH quando solto. Apertado, o pino é ligado ao GND e lê LOW."));
                double v = Ex.Pick(r, 5.0, 3.3), R = Ex.Step(r, 10, 50, 1) * 1000, I = v / R;
                return Ex.Num(L.F("Quando o botão é apertado, um pino de {0} V é ligado ao GND através de um resistor de pull-up de {1} kΩ. Que corrente circula por esse resistor, em microampères?", P.N(v, 1), P.N(R / 1000, 1)), I * 1e6, "μA", $"I = V / R = {P.N(v, 1)} / {P.N(R, 0)} = {P.N(I * 1e6)} μA.");
            }
            case Difficulty.Medium:
            {
                int ms = Ex.Int(r, 1, 10) * 100, hold = Ex.Int(r, 2, 20) * 100;
                if (r.Next(2) == 0)
                {
                    int n = Ex.Int(r, 3, 12), secs = Ex.Int(r, 10, 60);
                    return Ex.Num(L.F("Uma pessoa aperta o botão uma vez a cada {0} segundos. Quantos apertos acontecem em {1} segundos?", n, n * secs), secs, "apertos", L.F("{0} / {1} = {2} apertos.", n * secs, n, secs));
                }
                double scans = hold / (double)ms;
                return Ex.Num(L.F("O loop() lê o botão a cada {0} ms (por causa de um delay). O botão fica apertado por {1} ms. Quantas leituras vão ver o botão apertado?", ms, hold), Math.Floor(scans), "leituras", L.F("{0} / {1} = {2} → {3} leituras (inteiras).", hold, ms, P.N(scans), Math.Floor(scans)), 0.1);
            }
            case Difficulty.Hard:
            {
                var s = Enumerable.Range(0, Ex.Int(r, 10, 14)).Select(_ => r.Next(2)).ToList();
                int rises = 0, pv = 0; foreach (var x in s) { if (x == 1 && pv == 0) rises++; pv = x; }
                string code = Cx.Prog($"    int leitura[] = {{{Cx.Ints(s)}}};\n    int anterior = 0;\n    int contagem = 0;\n    for (int i = 0; i < {s.Count}; i++) {{\n        if (leitura[i] == 1 && anterior == 0) {{\n            contagem++;\n        }}\n        anterior = leitura[i];\n    }}\n    printf(\"%d\\n\", contagem);\n");
                return Cx.Output(L.T("O programa conta apertos de um botão a partir das leituras de cada ciclo. O que ele imprime?"), code, rises.ToString(), L.F("Conta só as passagens de 0 para 1 (bordas de subida): {0}.", rises));
            }
            default:
            {
                // Toggle on each press: count the final state after a sequence of readings.
                var s = Enumerable.Range(0, Ex.Int(r, 12, 16)).Select(_ => r.Next(2)).ToList();
                int led = 0, prev = 0;
                for (int i = 0; i < s.Count; i++) { if (s[i] == 1 && prev == 0) led = 1 - led; prev = s[i]; }
                string code = Cx.Prog($"    int leitura[] = {{{Cx.Ints(s)}}};\n    int anterior = 0, led = 0;\n    for (int i = 0; i < {s.Count}; i++) {{\n        if (leitura[i] == 1 && anterior == 0) {{\n            led = 1 - led;\n        }}\n        anterior = leitura[i];\n    }}\n    printf(\"%d\\n\", led);\n");
                return Cx.Output(L.T("Um botão liga e desliga um LED a cada aperto (alterna na borda de subida). Dadas as leituras abaixo, qual o estado final do LED?"), code, led.ToString(), L.T("Cada passagem de 0 para 1 inverte o LED; leituras seguidas em 1 não contam de novo."));
            }
        }
    }
}

/// <summary>PWM: duty cycle, average voltage, analogWrite values, fade loops.</summary>
[Exercises("ard-03-pwm-fade")]
public sealed class Ard03Pwm : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                int v = Ex.Int(r, 0, 255);
                double duty = v / 255.0 * 100;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Com analogWrite(pino, {0}), qual o ciclo de trabalho (duty cycle), em %?", v), duty, "%", $"{v} / 255 = {P.N(duty / 100, 3)} → {P.N(duty)}%.", 3);
                double vm = 5 * v / 255.0;
                return Ex.Num(L.F("Um pino de 5 V recebe analogWrite(pino, {0}). Qual a tensão média na saída, em volts?", v), vm, "V", L.F("Vmédia = 5·{0} / 255 = {1} V.", v, P.N(vm, 3)), 3);
            }
            case Difficulty.Medium:
            {
                double duty = Ex.Step(r, 5, 95, 1);
                double f = Ex.Pick(r, 490.0, 980);
                switch (r.Next(3))
                {
                    case 0:
                        double w = Math.Round(duty / 100 * 255);
                        return Ex.Num(L.F("Que valor passar ao analogWrite para um ciclo de trabalho de {0}%? (arredonde para o inteiro mais próximo)", P.N(duty, 0)), w, "", $"{P.N(duty, 0)}% de 255 = {P.N(duty / 100 * 255, 2)} ≈ {P.N(w, 0)}.", 1);
                    case 1:
                        double T = 1000.0 / f;
                        return Ex.Num(L.F("O PWM do Arduino roda a {0} Hz. Qual o período de cada ciclo, em ms?", P.N(f, 0)), T, "ms", $"T = 1 / f = 1 / {P.N(f, 0)} Hz = {P.N(T, 3)} ms.");
                    default:
                        double Tm = 1000.0 / f, ton = Tm * duty / 100;
                        return Ex.Num(L.F("Com PWM a {0} Hz e ciclo de trabalho de {1}%, por quantos milissegundos a saída fica em HIGH em cada ciclo?", P.N(f, 0), P.N(duty, 0)), ton, "ms", L.F("T = {0} ms; ton = T·duty = {1}·{2} = {3} ms.", P.N(Tm, 3), P.N(Tm, 3), P.N(duty / 100), P.N(ton, 3)));
                }
            }
            case Difficulty.Hard:
            {
                int step = Ex.Pick(r, 5, 10, 15, 17, 51), delay = Ex.Pick(r, 5, 10, 20, 30);
                int steps = (int)Math.Ceiling(255.0 / step);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um fade soma {0} ao brilho a cada {1} ms, de 0 até chegar a 255 ou mais (então fica em 255). Quantos passos até o brilho máximo?", step, delay), steps, "passos", L.F("255 / {0} = {1} → {2} passos.", step, P.N(255.0 / step, 2), steps), 0.1);
                return Ex.Num(L.F("Um fade soma {0} ao brilho a cada {1} ms, de 0 até chegar a 255 ou mais. Quanto tempo leva até o brilho máximo, em ms?", step, delay), steps * delay, "ms", L.F("Passos = ⌈255 / {0}⌉ = {1}; tempo = {2}·{3} = {4} ms.", step, steps, steps, delay, steps * delay), 0.1);
            }
            default:
            {
                // A fade that bounces: print the brightness each step.
                int step = Ex.Int(r, 30, 120), max = 255;
                var vals = new List<int>(); int b = 0, dir = 1;
                for (int i = 0; i < 9; i++)
                {
                    vals.Add(b);
                    b += step * dir;
                    if (b >= max) { b = max; dir = -1; }
                    else if (b <= 0) { b = 0; dir = 1; }
                }
                string code = Cx.Prog($"    int brilho = 0, passo = {step};\n    for (int i = 0; i < 9; i++) {{\n        printf(\"%d\\n\", brilho);\n        brilho = brilho + passo;\n        if (brilho >= 255) {{\n            brilho = 255;\n            passo = -passo;\n        }} else if (brilho <= 0) {{\n            brilho = 0;\n            passo = -passo;\n        }}\n    }}\n");
                return Cx.Output(L.T("Este fade vai e volta entre 0 e 255. O que o programa imprime nas 9 voltas?"), code, string.Join("\n", vals), L.T("O brilho sobe pelo passo até bater em 255 (limitado a 255), então o passo muda de sinal e ele desce, até 0."));
            }
        }
    }
}

/// <summary>Potentiometer and analogRead: 10-bit conversion, map().</summary>
[Exercises("ard-04-potenciometro")]
public sealed class Ard04Pot : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                int raw = Ex.Int(r, 0, 1023);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("analogRead devolveu {0} (ADC de 10 bits, referência de 5 V). Qual a tensão no pino, em volts?", raw), raw * 5.0 / 1023, "V", $"V = {raw}·5 / 1023 = {P.N(raw * 5.0 / 1023, 3)} V.", 3);
                double v = Ex.Step(r, 0.5, 4.5, 0.5);
                return Ex.Num(L.F("O pino analógico está em {0} V (referência de 5 V, ADC de 10 bits). Qual o valor lido por analogRead (inteiro mais próximo)?", P.N(v, 1)), Math.Round(v * 1023 / 5), "", L.F("leitura = V·1023 / 5 = {0}·1023 / 5 = {1} ≈ {2}.", P.N(v, 1), P.N(v * 1023 / 5, 1), P.N(Math.Round(v * 1023 / 5), 0)), 1);
            }
            case Difficulty.Medium:
            {
                int raw = Ex.Int(r, 0, 1023);
                int outMax = Ex.Pick(r, 255, 180, 100);
                long mapped = raw * outMax / 1023; // map(raw,0,1023,0,outMax) with integer math
                return Ex.Num(L.F("Com a leitura {0}, qual o resultado de map(leitura, 0, 1023, 0, {1})? (a divisão é inteira, trunca)", raw, outMax), mapped, "", L.F("map = (x − 0)·({0} − 0) / (1023 − 0) + 0 = {1}·{2} / 1023 = {3} (divisão inteira).", outMax, raw, outMax, mapped), 0.1);
            }
            case Difficulty.Hard:
            {
                int lo = Ex.Pick(r, 200, 300, 400), hi = Ex.Pick(r, 700, 800, 900), omin = Ex.Pick(r, 0, 10), omax = Ex.Pick(r, 100, 255);
                int x = Ex.Int(r, lo, hi);
                int m = (x - lo) * (omax - omin) / (hi - lo) + omin;
                return Ex.Num(L.F("Qual o resultado de map({0}, {1}, {2}, {3}, {4})? (divisão inteira)", x, lo, hi, omin, omax), m, "", $"({x} − {lo})·({omax} − {omin}) / ({hi} − {lo}) + {omin} = {(x - lo) * (omax - omin)} / {hi - lo} + {omin} = {m}.", 0.1);
            }
            default:
            {
                // A dimmer: print analogWrite values for a few pot readings, mapped with integer math and constrained.
                var readings = Enumerable.Range(0, 5).Select(_ => Ex.Int(r, 0, 1023)).ToList();
                int lo = Ex.Pick(r, 100, 200), hi = 900;
                var outs = readings.Select(x => Math.Clamp((x - lo) * 255 / (hi - lo), 0, 255)).ToList();
                string code = Cx.Prog($"    int leituras[] = {{{Cx.Ints(readings)}}};\n    for (int i = 0; i < 5; i++) {{\n        int x = leituras[i];\n        int pwm = (x - {lo}) * 255 / ({hi} - {lo});\n        if (pwm < 0) pwm = 0;\n        if (pwm > 255) pwm = 255;\n        printf(\"%d\\n\", pwm);\n    }}\n");
                return Cx.Output(L.T("Este dimmer converte a leitura do potenciômetro em PWM, limitando entre 0 e 255. O que ele imprime?"), code, string.Join("\n", outs), L.T("Cada leitura passa pela regra de três; valores fora do intervalo são cortados em 0 e 255 (constrain). Atenção: a divisão é inteira, com truncamento. Em C, a conta com negativo trunca em direção a zero antes do corte."));
            }
        }
    }
}

/// <summary>Serial Monitor and the LDR: voltage divider, thresholds, baud rate.</summary>
[Exercises("ard-05-serial-ldr")]
public sealed class Ard05Serial : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                double r1 = Ex.Step(r, 1, 50, 1) * 1000, r2 = Ex.Step(r, 1, 50, 1) * 1000, vo = 5 * r2 / (r1 + r2);
                return Ex.Num(L.F("Um divisor de tensão tem R1 = {0} kΩ (ligado a 5 V) e R2 = {1} kΩ (ligado ao GND). Qual a tensão na junção, em volts?", P.N(r1 / 1000, 1), P.N(r2 / 1000, 1)), vo, "V", L.F("Vsaída = 5·R2 / (R1 + R2) = 5·{0} / ({1} + {2}) = {3} V.", P.N(r2 / 1000, 1), P.N(r1 / 1000, 1), P.N(r2 / 1000, 1), P.N(vo, 3)));
            }
            case Difficulty.Medium:
            {
                double rl = Ex.Step(r, 1, 100, 1) * 1000, rf = 10000, vo = 5 * rf / (rf + rl);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um LDR (ligado a 5 V) forma um divisor com um resistor fixo de 10 kΩ (ligado ao GND). Com o LDR em {0} kΩ, qual a tensão lida no pino, em volts?", P.N(rl / 1000, 0)), vo, "V", L.F("Vsaída = 5·10 / (10 + {0}) = {1} V.", P.N(rl / 1000, 0), P.N(vo, 3)));
                int raw = (int)Math.Round(vo / 5 * 1023);
                return Ex.Num(L.F("Um LDR de {0} kΩ (ligado a 5 V) e um resistor fixo de 10 kΩ (ao GND) formam um divisor. Que valor o analogRead devolve (inteiro mais próximo)?", P.N(rl / 1000, 0)), raw, "", L.F("Vsaída = {0} V → leitura = {1}·1023 / 5 ≈ {2}.", P.N(vo, 3), P.N(vo, 3), raw), 1);
            }
            case Difficulty.Hard:
            {
                int baud = Ex.Pick(r, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200), n = Ex.Int(r, 5, 40);
                double t = n * 10.0 / baud * 1000;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("A {0} baud, cada caractere gasta 10 bits (início + 8 dados + fim). Quanto tempo, em ms, leva para enviar uma linha de {1} caracteres?", baud, n), t, "ms", L.F("{0}·10 bits = {1} bits; {2} / {3} = {4} ms.", n, n * 10, n * 10, baud, P.N(t, 3)));
                double cps = baud / 10.0;
                return Ex.Num(L.F("A {0} baud, cada caractere usa 10 bits no total. Quantos caracteres por segundo a porta serial transmite?", baud), cps, "caract./s", L.F("{0} / 10 = {1} caracteres/s.", baud, P.N(cps, 0)));
            }
            default:
            {
                int thr = Ex.Pick(r, 300, 400, 500, 600);
                var rd = Enumerable.Range(0, 7).Select(_ => Ex.Int(r, 100, 900)).ToList();
                var sb = new List<string>();
                foreach (var x in rd) sb.Add(x < thr ? "ESCURO" : "CLARO");
                string code = Cx.Prog($"    int luz[] = {{{Cx.Ints(rd)}}};\n    int escuro = 0;\n    for (int i = 0; i < 7; i++) {{\n        if (luz[i] < {thr}) {{\n            escuro++;\n        }}\n    }}\n    printf(\"%d\\n\", escuro);\n");
                int cnt = rd.Count(x => x < thr);
                return Cx.Output(L.T("Um poste liga a lâmpada quando a luz lida fica abaixo do limiar. O programa conta quantas leituras da lista são escuras. O que ele imprime?"), code, cnt.ToString(), L.F("Contam as leituras menores que {0}: {1}.", thr, cnt));
            }
        }
    }
}

/// <summary>Traffic light: millis() state machine.</summary>
[Exercises("ard-06-semaforo")]
public sealed class Ard06Semaforo : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int tv = Ex.Int(r, 2, 8) * 1000, ta = Ex.Int(r, 1, 3) * 1000, tr = Ex.Int(r, 2, 8) * 1000, cycle = tv + ta + tr;
        string cfg = L.F("Um semáforo fica {0} s no verde, {1} s no amarelo e {2} s no vermelho, nessa ordem, repetindo (começa no verde em millis() = 0).", tv / 1000, ta / 1000, tr / 1000);
        switch (d)
        {
            case Difficulty.Easy:
            {
                if (r.Next(2) == 0)
                    return Ex.Num(cfg + " " + L.T("Quanto dura um ciclo completo, em segundos?"), cycle / 1000.0, "s", $"{tv / 1000} + {ta / 1000} + {tr / 1000} = {cycle / 1000} s.");
                double cars = 3600.0 / (cycle / 1000.0);
                return Ex.Num(cfg + " " + L.T("Quantos ciclos completos acontecem em 1 hora?"), cars, "ciclos", L.F("3600 / {0} = {1} ciclos.", cycle / 1000, P.N(cars)));
            }
            case Difficulty.Medium:
            {
                int t = Ex.Int(r, 1, 3 * cycle / 1000) * 1000 + Ex.Int(r, 0, 9) * 100;
                int m = t % cycle;
                string state = m < tv ? L.T("verde") : m < tv + ta ? L.T("amarelo") : L.T("vermelho");
                string right = m < tv ? L.T("Verde") : m < tv + ta ? L.T("Amarelo") : L.T("Vermelho");
                return Ex.Choice(r, cfg + " " + L.F("Que luz está acesa quando millis() = {0} ms?", t), right, new[] { L.T("Verde"), L.T("Amarelo"), L.T("Vermelho") },
                    $"{t} mod {cycle} = {m} ms. " + (m < tv ? $"{m} < {tv}" : m < tv + ta ? $"{tv} ≤ {m} < {tv + ta}" : $"{m} ≥ {tv + ta}") + ".");
            }
            case Difficulty.Hard:
            {
                int k = Ex.Int(r, 2, 6);
                if (r.Next(2) == 0)
                    return Ex.Num(cfg + " " + L.F("Em que valor de millis() (em segundos) o vermelho acende pela {0}ª vez? (o primeiro verde começa em 0)", k), ((k - 1) * cycle + tv + ta) / 1000.0, "s", L.F("Em cada ciclo o vermelho começa depois de {0} + {1} = {2} s. Na {3}ª vez: ({4} − 1)·{5} + {6} = {7} s.", tv / 1000, ta / 1000, (tv + ta) / 1000, k, k, cycle / 1000, (tv + ta) / 1000, ((k - 1) * cycle + tv + ta) / 1000));
                double span = Ex.Int(r, 2, 6) * cycle / 1000.0;
                return Ex.Num(cfg + " " + L.F("Quanto tempo o vermelho fica aceso, no total, em {0} s?", P.N(span, 0)), span / (cycle / 1000.0) * tr / 1000, "s", L.F("{0} s = {1} ciclos; {2}·{3} = {4} s.", P.N(span, 0), P.N(span / (cycle / 1000.0), 0), P.N(span / (cycle / 1000.0), 0), tr / 1000, P.N(span / (cycle / 1000.0) * tr / 1000, 0)));
            }
            default:
            {
                // Simulate the state machine at fixed ticks; print the state letter at each tick.
                int tick = Ex.Pick(r, 500, 1000, 1500), n = 10;
                var sb = new StringBuilder();
                for (int i = 0; i < n; i++)
                {
                    int m = i * tick % cycle;
                    sb.Append(m < tv ? 'V' : m < tv + ta ? 'A' : 'R');
                }
                string code = Cx.Prog($"    int tempoVerde = {tv}, tempoAmarelo = {ta}, tempoVermelho = {tr};\n    int ciclo = tempoVerde + tempoAmarelo + tempoVermelho;\n    for (int i = 0; i < {n}; i++) {{\n        int agora = i * {tick};\n        int m = agora % ciclo;\n        if (m < tempoVerde) {{\n            printf(\"V\");\n        }} else if (m < tempoVerde + tempoAmarelo) {{\n            printf(\"A\");\n        }} else {{\n            printf(\"R\");\n        }}\n    }}\n    printf(\"\\n\");\n");
                return Cx.Output(L.T("O programa consulta o semáforo a cada tick (V = verde, A = amarelo, R = vermelho). O que ele imprime?"), code, sb.ToString(), L.F("Para cada instante, o resto da divisão pelo ciclo ({0} ms) diz em que parte do ciclo estamos.", cycle));
            }
        }
    }
}

/// <summary>Servo: pulse width to angle, sweeps.</summary>
[Exercises("ard-07-servo")]
public sealed class Ard07Servo : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                int ang = (int)Ex.Step(r, 0, 180, 5);
                double us = 1000 + ang / 180.0 * 1000;
                return Ex.Num(L.F("Um servo usa pulsos de 1000 μs (0°) a 2000 μs (180°), linearmente. Que largura de pulso leva o servo a {0}°?", ang), us, "μs", $"1000 + {ang}/180·1000 = {P.N(us, 0)} μs.");
            }
            case Difficulty.Medium:
            {
                int step = Ex.Pick(r, 1, 2, 3, 5, 6, 9), dl = Ex.Pick(r, 10, 15, 20, 30);
                int steps = 180 / step;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um sweep vai de 0° a 180° em passos de {0}°, com delay({1}) após cada passo. Quantos passos tem uma ida (de 0° a 180°)?", step, dl), steps, "passos", L.F("180 / {0} = {1} passos.", step, steps), 0.1);
                return Ex.Num(L.F("Um sweep vai de 0° a 180° em passos de {0}°, com delay({1}) após cada passo. Quanto tempo leva uma ida, em ms?", step, dl), steps * dl, "ms", L.F("{0} passos·{1} ms = {2} ms.", 180 / step, dl, steps * dl), 0.1);
            }
            case Difficulty.Hard:
            {
                int us = Ex.Int(r, 11, 19) * 100 + Ex.Pick(r, 0, 50);
                double ang = (us - 1000) / 1000.0 * 180;
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Um servo (1000 μs = 0°, 2000 μs = 180°) recebe pulsos de {0} μs. Para qual ângulo ele vai?", us), ang, "°", $"({us} − 1000) / 1000·180 = {P.N(ang)}°.");
                int a1 = Ex.Pick(r, 20, 30, 45), a2 = Ex.Pick(r, 120, 150, 160), dl = Ex.Pick(r, 15, 20, 25);
                return Ex.Num(L.F("Um servo varre de {0}° a {1}° de 1 em 1 grau, com delay({2}) a cada grau. Quanto dura a varredura, em segundos?", a1, a2, dl), (a2 - a1) * dl / 1000.0, "s", L.F("{0} − {1} = {2} graus·{3} ms = {4} ms = {5} s.", a2, a1, a2 - a1, dl, (a2 - a1) * dl, P.N((a2 - a1) * dl / 1000.0, 3)));
            }
            default:
            {
                // Sweep with bounce printed: angle at each step.
                int step = Ex.Int(r, 20, 70), n = 10;
                var vals = new List<int>(); int a = 0, dir = 1;
                for (int i = 0; i < n; i++)
                {
                    vals.Add(a);
                    a += step * dir;
                    if (a >= 180) { a = 180; dir = -1; } else if (a <= 0) { a = 0; dir = 1; }
                }
                string code = Cx.Prog($"    int angulo = 0, passo = {step};\n    for (int i = 0; i < {n}; i++) {{\n        printf(\"%d\\n\", angulo);\n        angulo = angulo + passo;\n        if (angulo >= 180) {{\n            angulo = 180;\n            passo = -passo;\n        }} else if (angulo <= 0) {{\n            angulo = 0;\n            passo = -passo;\n        }}\n    }}\n");
                return Cx.Output(L.T("Esta varredura de servo vai e volta entre 0° e 180°. O que o programa imprime?"), code, string.Join("\n", vals), L.T("O ângulo sobe pelo passo até bater em 180 (que é limitado a 180); então o passo muda de sinal e ele desce até 0."));
            }
        }
    }
}
