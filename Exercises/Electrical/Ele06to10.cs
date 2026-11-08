namespace EasyCreator.Exercises;

[Exercises("ele-06-rc")]
public sealed class Ele06Rc : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double R = P(r, 1.0, 2.2, 4.7, 10, 22, 47, 100), C = P(r, 1.0, 4.7, 10, 22, 47, 100, 470), tau = R * C;
            return Q(XF("Em um circuito RC, R = {0} kΩ e C = {1} µF. Qual a constante de tempo τ?", "In an RC circuit, R = {0} kΩ and C = {1} µF. What is the time constant τ?", "En un circuito RC, R = {0} kΩ y C = {1} µF. ¿Cuál es la constante de tiempo τ?", N(R), N(C)), tau, "ms",
                YF("τ = R·C = {0} kΩ · {1} µF = {2} ms.", N(R), N(C), N(tau)));
        }, r =>
        {
            double c1 = P(r, 10.0, 22, 47, 100), c2 = P(r, 4.7, 33, 68, 220), ser = c1 * c2 / (c1 + c2);
            return r.Next(2) == 0
                ? Q(XF("Dois capacitores de {0} µF e {1} µF são ligados em paralelo. Qual a capacitância equivalente?", "Two capacitors of {0} µF and {1} µF are connected in parallel. What is the equivalent capacitance?", "Dos condensadores de {0} µF y {1} µF se conectan en paralelo. ¿Cuál es la capacitancia equivalente?", N(c1), N(c2)), c1 + c2, "µF", X("Em paralelo as capacitâncias somam.", "In parallel, capacitances add.", "En paralelo las capacitancias se suman."))
                : Q(XF("Dois capacitores de {0} µF e {1} µF são ligados em série. Qual a capacitância equivalente?", "Two capacitors of {0} µF and {1} µF are connected in series. What is the equivalent capacitance?", "Dos condensadores de {0} µF y {1} µF se conectan en serie. ¿Cuál es la capacitancia equivalente?", N(c1), N(c2)), ser, "µF", YF("Em série: C = C₁·C₂/(C₁+C₂) = {0}·{1}/{2} = {3} µF.", N(c1), N(c2), N(c1 + c2), N(ser, 3)));
        }),
        Difficulty.Medium => F(r =>
        {
            double V = P(r, 5.0, 9, 12, 24), R = P(r, 1.0, 2.2, 4.7, 10), C = P(r, 10.0, 47, 100, 220), tau = R * C, t = Math.Round(tau * S(r, 0.3, 3, 0.1), 1);
            double vc = V * (1 - Exp(-t / tau));
            return Q(XF("Um capacitor descarregado de {0} µF é carregado por {1} V através de {2} kΩ. Qual a sua tensão após {3} ms?", "A discharged {0} µF capacitor is charged from {1} V through {2} kΩ. What is its voltage after {3} ms?", "Un condensador descargado de {0} µF se carga con {1} V a través de {2} kΩ. ¿Cuál es su tensión tras {3} ms?", N(C), N(V, 0), N(R), N(t)), vc, "V",
                YF("τ = {0} ms; Vc = V·(1 − e^(−t/τ)) = {1}·(1 − e^(−{2})) = {3} V.", N(tau), N(V, 0), N(t / tau, 3), N(vc, 3)));
        }, r =>
        {
            double V = P(r, 5.0, 9, 12, 24), R = P(r, 1.0, 2.2, 4.7, 10), C = P(r, 10.0, 47, 100, 220), tau = R * C, f = P(r, 0.5, 0.632, 0.8, 0.9, 0.95);
            double t = -tau * Ln(1 - f);
            return Q(XF("Um capacitor descarregado de {0} µF é carregado por {1} kΩ a partir de uma fonte de {2} V. Em quantos ms ele chega a {3} V?", "A discharged {0} µF capacitor is charged through {1} kΩ from a {2} V source. In how many ms does it reach {3} V?", "Un condensador descargado de {0} µF se carga a través de {1} kΩ desde una fuente de {2} V. ¿En cuántos ms llega a {3} V?", N(C), N(R), N(V, 0), N(V * f, 3)), t, "ms",
                YF("τ = {0} ms; t = −τ·ln(1 − Vc/V) = −{0}·ln(1 − {1}) = {2} ms.", N(tau), N(f, 3), N(t)));
        }),
        Difficulty.Hard => F(r =>
        {
            double V0 = P(r, 9.0, 12, 24, 50), R = P(r, 1.0, 2.2, 4.7, 10), C = P(r, 10.0, 47, 100, 220), tau = R * C, t = Math.Round(tau * S(r, 0.3, 3, 0.1), 1);
            double vc = V0 * Exp(-t / tau);
            return Q(XF("Um capacitor de {0} µF carregado com {1} V descarrega através de {2} kΩ. Qual a tensão após {3} ms?", "A {0} µF capacitor charged to {1} V discharges through {2} kΩ. What is the voltage after {3} ms?", "Un condensador de {0} µF cargado con {1} V se descarga a través de {2} kΩ. ¿Cuál es la tensión tras {3} ms?", N(C), N(V0, 0), N(R), N(t)), vc, "V",
                YF("τ = {0} ms; Vc = V₀·e^(−t/τ) = {1}·e^(−{2}) = {3} V.", N(tau), N(V0, 0), N(t / tau, 3), N(vc, 3)));
        }, r =>
        {
            double V0 = P(r, 9.0, 12, 24, 50), R = P(r, 1.0, 2.2, 4.7, 10), C = P(r, 10.0, 47, 100, 220), tau = R * C, f = P(r, 0.5, 0.3, 0.2, 0.1, 0.05), t = -tau * Ln(f);
            return Q(XF("Um capacitor de {0} µF carregado com {1} V descarrega através de {2} kΩ. Em quantos ms a tensão cai a {3} V?", "A {0} µF capacitor charged to {1} V discharges through {2} kΩ. In how many ms does the voltage drop to {3} V?", "Un condensador de {0} µF cargado con {1} V se descarga a través de {2} kΩ. ¿En cuántos ms la tensión cae a {3} V?", N(C), N(V0, 0), N(R), N(V0 * f, 3)), t, "ms",
                YF("t = −τ·ln(V/V₀) = −{0}·ln({1}) = {2} ms.", N(tau), N(f, 3), N(t)));
        }, r =>
        {
            double C = P(r, 10.0, 47, 100, 470), V = P(r, 5.0, 12, 24, 50), e = 0.5 * C * 1e-6 * V * V * 1000;
            return Q(XF("Um capacitor de {0} µF é carregado a {1} V. Quanta energia ele armazena?", "A {0} µF capacitor is charged to {1} V. How much energy does it store?", "Un condensador de {0} µF se carga a {1} V. ¿Cuánta energía almacena?", N(C), N(V, 0)), e, "mJ",
                YF("E = ½·C·V² = ½ · {0}·10⁻⁶ · {1}² = {2} mJ.", N(C), N(V, 0), N(e, 3)));
        }),
        _ => F(r =>
        {
            double Vf = P(r, 10.0, 12, 24), V0 = S(r, 1, 6, 1), R = P(r, 2.2, 4.7, 10), C = P(r, 10.0, 47, 100), tau = R * C, t = Math.Round(tau * S(r, 0.5, 2, 0.1), 1);
            double vc = Vf + (V0 - Vf) * Exp(-t / tau);
            return Q(XF("Um capacitor de {0} µF já tem {1} V quando é ligado a uma fonte de {2} V através de {3} kΩ. Qual a tensão {4} ms depois?", "A {0} µF capacitor already holds {1} V when it is connected to a {2} V source through {3} kΩ. What is its voltage {4} ms later?", "Un condensador de {0} µF ya tiene {1} V cuando se conecta a una fuente de {2} V a través de {3} kΩ. ¿Cuál es la tensión {4} ms después?", N(C), N(V0, 0), N(Vf, 0), N(R), N(t)), vc, "V",
                YF("Vc = Vf + (V₀ − Vf)·e^(−t/τ) = {0} + ({1} − {0})·e^(−{2}) = {3} V.", N(Vf, 0), N(V0, 0), N(t / tau, 3), N(vc, 3)));
        }, r =>
        {
            double V = P(r, 5.0, 9, 12), C = P(r, 10.0, 47, 100), R = P(r, 1.0, 2.2, 4.7, 10), tau = R * C, f = P(r, 0.5, 0.7, 0.8, 0.9), t = -tau * Ln(1 - f);
            return Q(XF("Um temporizador RC usa C = {0} µF e uma fonte de {1} V. Ele deve disparar quando a tensão do capacitor chega a {2} V, {3} ms depois de ligado. Qual o valor de R?", "An RC timer uses C = {0} µF and a {1} V supply. It must trigger when the capacitor reaches {2} V, {3} ms after power-on. What is R?", "Un temporizador RC usa C = {0} µF y una fuente de {1} V. Debe dispararse cuando el condensador llega a {2} V, {3} ms después de encender. ¿Cuál es R?", N(C), N(V, 0), N(V * f, 3), N(t)), R, "kΩ",
                YF("τ = −t/ln(1 − Vc/V) = −{0}/ln(1 − {1}) = {2} ms; R = τ/C = {2}/{3} = {4} kΩ.", N(t), N(f, 3), N(tau), N(C), N(R)));
        })
    };
}

[Exercises("ele-07-fasores")]
public sealed class Ele07Phasors : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double f = P(r, 50.0, 60, 400, 1000), L = S(r, 5, 500, 5), xl = 2 * Pi * f * L / 1000;
            return Q(XF("Qual a reatância de um indutor de {0} mH em {1} Hz?", "What is the reactance of a {0} mH inductor at {1} Hz?", "¿Cuál es la reactancia de un inductor de {0} mH a {1} Hz?", N(L, 0), N(f, 0)), xl, "Ω",
                YF("XL = 2π·f·L = 2π · {0} · {1} mH = {2} Ω.", N(f, 0), N(L, 0), N(xl)));
        }, r =>
        {
            double f = P(r, 50.0, 60, 400, 1000), C = S(r, 1, 100, 0.5), xc = 1e6 / (2 * Pi * f * C);
            return Q(XF("Qual a reatância de um capacitor de {0} µF em {1} Hz?", "What is the reactance of a {0} µF capacitor at {1} Hz?", "¿Cuál es la reactancia de un condensador de {0} µF a {1} Hz?", N(C), N(f, 0)), xc, "Ω",
                YF("XC = 1/(2π·f·C) = 1/(2π · {0} · {1}·10⁻⁶) = {2} Ω.", N(f, 0), N(C), N(xc)));
        }),
        Difficulty.Medium => F(r =>
        {
            double f = P(r, 50.0, 60), R = P(r, 10.0, 22, 47, 100), L = P(r, 50.0, 100, 200, 500), V = P(r, 12.0, 24, 127, 220), xl = 2 * Pi * f * L / 1000, z = Sqrt(R * R + xl * xl);
            string intro = XF("Um resistor de {0} Ω em série com um indutor de {1} mH é ligado a {2} V / {3} Hz.", "A {0} Ω resistor in series with a {1} mH inductor is connected to {2} V / {3} Hz.", "Una resistencia de {0} Ω en serie con un inductor de {1} mH se conecta a {2} V / {3} Hz.", N(R, 0), N(L, 0), N(V, 0), N(f, 0));
            string st = YF("XL = 2π·{0}·{1} mH = {2} Ω; Z = √(R² + XL²) = {3} Ω.", N(f, 0), N(L, 0), N(xl), N(z));
            return r.Next(2) == 0
                ? Q(intro + " " + X("Qual a impedância?", "What is the impedance?", "¿Cuál es la impedancia?"), z, "Ω", st)
                : Q(intro + " " + X("Qual a corrente?", "What is the current?", "¿Cuál es la corriente?"), V / z, "A", st + " " + YF("I = V/Z = {0} A.", N(V / z, 3)));
        }),
        Difficulty.Hard => F(r =>
        {
            double f = 60, R = P(r, 10.0, 22, 47, 100), L = P(r, 100.0, 200, 300, 500), C = P(r, 22.0, 47, 100, 150), V = P(r, 24.0, 127, 220);
            double xl = 2 * Pi * f * L / 1000, xc = 1e6 / (2 * Pi * f * C), x = xl - xc, z = Sqrt(R * R + x * x), ang = Math.Atan2(x, R) * 180 / Pi;
            string intro = XF("Circuito RLC série em {0} Hz: R = {1} Ω, L = {2} mH, C = {3} µF, alimentado por {4} V.", "Series RLC circuit at {0} Hz: R = {1} Ω, L = {2} mH, C = {3} µF, fed by {4} V.", "Circuito RLC serie a {0} Hz: R = {1} Ω, L = {2} mH, C = {3} µF, alimentado con {4} V.", N(f, 0), N(R, 0), N(L, 0), N(C, 0), N(V, 0));
            string st = YF("XL = {0} Ω; XC = {1} Ω; X = {2} Ω; Z = √(R² + X²) = {3} Ω.", N(xl), N(xc), N(x), N(z));
            return r.Next(3) switch
            {
                0 => Q(intro + " " + X("Qual a impedância?", "What is the impedance?", "¿Cuál es la impedancia?"), z, "Ω", st),
                1 => Q(intro + " " + X("Qual a corrente?", "What is the current?", "¿Cuál es la corriente?"), V / z, "A", st + " " + YF("I = V/Z = {0} A.", N(V / z, 3))),
                _ => Q(intro + " " + X("Qual o módulo do ângulo de fase entre tensão e corrente, em graus?", "What is the magnitude of the phase angle between voltage and current, in degrees?", "¿Cuál es el módulo del ángulo de fase entre tensión y corriente, en grados?"), Math.Abs(ang), "°", st + " " + YF("φ = arctan(X/R) = {0}°.", N(ang, 2)))
            };
        }),
        _ => F(r =>
        {
            double L = P(r, 10.0, 22, 47, 100, 220), C = P(r, 0.1, 0.22, 0.47, 1, 2.2, 10), f0 = 1000 / (2 * Pi * Sqrt(L * C * 1e-3));
            return Q(XF("Qual a frequência de ressonância de um circuito LC com L = {0} mH e C = {1} µF?", "What is the resonant frequency of an LC circuit with L = {0} mH and C = {1} µF?", "¿Cuál es la frecuencia de resonancia de un circuito LC con L = {0} mH y C = {1} µF?", N(L, 0), N(C)), f0, "Hz",
                YF("f₀ = 1/(2π√(LC)) = 1/(2π√({0}·10⁻³ · {1}·10⁻⁶)) = {2} Hz.", N(L, 0), N(C), N(f0, 1)));
        }, r =>
        {
            double R = P(r, 2.0, 5, 10, 20), L = P(r, 10.0, 22, 47, 100), C = P(r, 0.1, 0.47, 1, 2.2), V = P(r, 1.0, 2, 5, 10);
            double w0 = 1 / Sqrt(L * 1e-3 * C * 1e-6), xl = w0 * L * 1e-3, q = xl / R, vc = q * V;
            return Q(XF("Um circuito RLC série (R = {0} Ω, L = {1} mH, C = {2} µF) é alimentado com {3} V na frequência de ressonância. Qual a tensão sobre o capacitor?", "A series RLC circuit (R = {0} Ω, L = {1} mH, C = {2} µF) is fed with {3} V at the resonant frequency. What is the voltage across the capacitor?", "Un circuito RLC serie (R = {0} Ω, L = {1} mH, C = {2} µF) se alimenta con {3} V a la frecuencia de resonancia. ¿Cuál es la tensión en el condensador?", N(R, 0), N(L, 0), N(C), N(V, 0)), vc, "V",
                YF("ω₀ = 1/√(LC) = {0} rad/s; XL = XC = {1} Ω; na ressonância Z = R, I = V/R = {2} A; Vc = I·XC = Q·V = {3} · {4} = {5} V.", N(w0, 0), N(xl), N(V / R, 3), N(q, 2), N(V, 0), N(vc, 2)));
        }, r =>
        {
            double L = P(r, 10.0, 47, 100, 220), f = P(r, 500.0, 1000, 5000, 10000), C = 1e6 / (Pow2(2 * Pi * f) * L * 1e-3);
            return Q(XF("Que capacitor, em µF, deve ser ligado a um indutor de {0} mH para ressoar em {1} Hz?", "What capacitor, in µF, must be connected to a {0} mH inductor to resonate at {1} Hz?", "¿Qué condensador, en µF, hay que conectar a un inductor de {0} mH para resonar a {1} Hz?", N(L, 0), N(f, 0)), C, "µF",
                YF("f₀ = 1/(2π√(LC)) → C = 1/((2π·f)²·L) = 1/((2π · {0})² · {1}·10⁻³) = {2} µF.", N(f, 0), N(L, 0), N(C, 4)));
        })
    };
    static double Pow2(double v) => v * v;
}

[Exercises("ele-08-fator-potencia")]
public sealed class Ele08PowerFactor : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double V = P(r, 127.0, 220), I = S(r, 2, 40, 1), fp = P(r, 0.6, 0.7, 0.8, 0.85, 0.9, 0.95), s = V * I;
            string intro = XF("Uma carga monofásica em {0} V consome {1} A com fator de potência {2}.", "A single-phase load at {0} V draws {1} A with power factor {2}.", "Una carga monofásica a {0} V consume {1} A con factor de potencia {2}.", N(V, 0), N(I, 0), N(fp));
            return r.Next(2) == 0
                ? Q(intro + " " + X("Qual a potência aparente?", "What is the apparent power?", "¿Cuál es la potencia aparente?"), s, "VA", YF("S = V·I = {0} · {1} = {2} VA.", N(V, 0), N(I, 0), N(s, 0)))
                : Q(intro + " " + X("Qual a potência ativa?", "What is the real power?", "¿Cuál es la potencia activa?"), s * fp, "W", YF("S = V·I = {0} VA; P = S·cos φ = {0} · {1} = {2} W.", N(s, 0), N(fp), N(s * fp, 1)));
        }),
        Difficulty.Medium => F(r =>
        {
            double P1 = P(r, 2.0, 3, 5, 8, 10, 15) * 1000, fp = P(r, 0.6, 0.7, 0.75, 0.8, 0.85), s = P1 / fp, q = Sqrt(s * s - P1 * P1);
            string intro = XF("Uma carga ativa de {0} kW tem fator de potência {1}.", "A load with {0} kW of real power has power factor {1}.", "Una carga con {0} kW de potencia activa tiene factor de potencia {1}.", N(P1 / 1000, 0), N(fp, 2));
            return r.Next(2) == 0
                ? Q(intro + " " + X("Qual a potência aparente?", "What is the apparent power?", "¿Cuál es la potencia aparente?"), s / 1000, "kVA", YF("S = P / cos φ = {0} / {1} = {2} kVA.", N(P1 / 1000, 0), N(fp, 2), N(s / 1000, 3)))
                : Q(intro + " " + X("Qual a potência reativa?", "What is the reactive power?", "¿Cuál es la potencia reactiva?"), q / 1000, "kvar", YF("S = {0} kVA; Q = √(S² − P²) = {1} kvar.", N(s / 1000, 3), N(q / 1000, 3)));
        }, r =>
        {
            double P1 = P(r, 5.0, 8, 10, 20, 30) * 1000, S1 = P1 / P(r, 0.6, 0.7, 0.8);
            return Q(XF("Uma instalação consome {0} kW e {1} kVA. Qual o seu fator de potência?", "An installation uses {0} kW and {1} kVA. What is its power factor?", "Una instalación consume {0} kW y {1} kVA. ¿Cuál es su factor de potencia?", N(P1 / 1000, 0), N(S1 / 1000, 2)), P1 / S1, "",
                YF("fp = P / S = {0} / {1} = {2}.", N(P1 / 1000, 0), N(S1 / 1000, 2), N(P1 / S1, 3)));
        }),
        Difficulty.Hard => F(r =>
        {
            double P1 = P(r, 5.0, 10, 20, 40, 60), f1 = P(r, 0.6, 0.7, 0.75, 0.8), f2 = P(r, 0.92, 0.95, 0.98, 1.0);
            double qc = P1 * (Math.Tan(Math.Acos(f1)) - Math.Tan(Math.Acos(f2)));
            return Q(XF("Uma carga de {0} kW tem fator de potência {1}. Quantos kvar de capacitores são necessários para levar o fator de potência a {2}?", "A {0} kW load has power factor {1}. How many kvar of capacitors are needed to bring the power factor to {2}?", "Una carga de {0} kW tiene factor de potencia {1}. ¿Cuántos kvar de condensadores se necesitan para llevar el factor de potencia a {2}?", N(P1, 0), N(f1, 2), N(f2, 2)), qc, "kvar",
                YF("Qc = P·(tan φ₁ − tan φ₂) = {0}·({1} − {2}) = {3} kvar.", N(P1, 0), N(Math.Tan(Math.Acos(f1)), 4), N(Math.Tan(Math.Acos(f2)), 4), N(qc, 3)));
        }),
        _ => F(r =>
        {
            double V = P(r, 220.0, 380, 127), P1 = P(r, 3.0, 5, 8, 12) * 1000, f1 = P(r, 0.6, 0.7, 0.75, 0.8), f2 = P(r, 0.95, 0.98), f = 60;
            double qc = P1 * (Math.Tan(Math.Acos(f1)) - Math.Tan(Math.Acos(f2))), c = qc / (2 * Pi * f * V * V) * 1e6;
            return Q(XF("Uma carga monofásica de {0} kW em {1} V / 60 Hz tem fator de potência {2}. Que capacitor, em µF, ligado em paralelo, leva o fator de potência a {3}?", "A single-phase {0} kW load at {1} V / 60 Hz has power factor {2}. What capacitor, in µF, connected in parallel, brings the power factor to {3}?", "Una carga monofásica de {0} kW a {1} V / 60 Hz tiene factor de potencia {2}. ¿Qué condensador, en µF, conectado en paralelo, lleva el factor de potencia a {3}?", N(P1 / 1000, 0), N(V, 0), N(f1, 2), N(f2, 2)), c, "µF",
                YF("Qc = P·(tan φ₁ − tan φ₂) = {0} var; C = Qc / (2π·f·V²) = {0} / (2π · 60 · {1}²) = {2} µF.", N(qc, 0), N(V, 0), N(c, 1)));
        }, r =>
        {
            double V = P(r, 220.0, 380), P1 = P(r, 5.0, 10, 20) * 1000, f1 = P(r, 0.6, 0.7, 0.75), f2 = P(r, 0.95, 1.0), i1 = P1 / (V * f1), i2 = P1 / (V * f2);
            return Q(XF("Uma carga monofásica de {0} kW em {1} V tem fator de potência {2}. Quantos ampères a linha deixa de conduzir depois que o fator de potência é corrigido para {3}?", "A single-phase {0} kW load at {1} V has power factor {2}. By how many amperes does the line current drop after the power factor is corrected to {3}?", "Una carga monofásica de {0} kW a {1} V tiene factor de potencia {2}. ¿Cuántos amperios deja de conducir la línea después de corregir el factor de potencia a {3}?", N(P1 / 1000, 0), N(V, 0), N(f1, 2), N(f2, 2)), i1 - i2, "A",
                YF("I = P/(V·fp): antes {0} A; depois {1} A; redução = {2} A.", N(i1, 2), N(i2, 2), N(i1 - i2, 2)));
        })
    };
}

[Exercises("ele-09-retificador")]
public sealed class Ele09Rectifier : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double v = P(r, 6.0, 9, 12, 15, 18, 24, 127, 220), vp = v * Sqrt(2);
            return Q(XF("Um transformador entrega {0} V RMS no secundário. Qual a tensão de pico?", "A transformer delivers {0} V RMS at the secondary. What is the peak voltage?", "Un transformador entrega {0} V RMS en el secundario. ¿Cuál es la tensión de pico?", N(v, 0)), vp, "V",
                YF("Vp = Vrms·√2 = {0} · 1,414 = {1} V.", N(v, 0), N(vp, 2)));
        }, r =>
        {
            double vp = S(r, 5, 40, 1);
            return Q(XF("Um retificador de onda completa recebe uma senoide de pico {0} V (diodos ideais). Qual a tensão média na saída?", "A full-wave rectifier receives a sine with {0} V peak (ideal diodes). What is the average output voltage?", "Un rectificador de onda completa recibe una senoide de pico {0} V (diodos ideales). ¿Cuál es la tensión media de salida?", N(vp, 0)), 2 * vp / Pi, "V",
                YF("Vmed = 2·Vp/π = 2 · {0} / π = {1} V.", N(vp, 0), N(2 * vp / Pi, 3)));
        }),
        Difficulty.Medium => F(r =>
        {
            double v = S(r, 5, 30, 0.5), vp = v * Sqrt(2), vo = vp - 1.4;
            return Q(XF("Uma ponte de 4 diodos (0,7 V cada) é ligada a um secundário de {0} V RMS. Qual o pico da tensão de saída?", "A 4-diode bridge (0.7 V each) is connected to a {0} V RMS secondary. What is the peak output voltage?", "Un puente de 4 diodos (0,7 V cada uno) se conecta a un secundario de {0} V RMS. ¿Cuál es el pico de la tensión de salida?", N(v, 0)), vo, "V",
                YF("Vp = {0}·√2 = {1} V; em cada meio-ciclo conduzem 2 diodos: Vsaída = {1} − 1,4 = {2} V.", N(v, 0), N(vp, 2), N(vo, 2)));
        }, r =>
        {
            double vp = S(r, 10, 40, 1), vm = 2 * vp / Pi - 0.0, rl = P(r, 100.0, 220, 470, 1000), i = vm / rl * 1000;
            return Q(XF("Um retificador de onda completa ideal de pico {0} V alimenta uma carga de {1} Ω. Qual a corrente média na carga?", "An ideal full-wave rectifier with {0} V peak feeds a {1} Ω load. What is the average load current?", "Un rectificador de onda completa ideal de pico {0} V alimenta una carga de {1} Ω. ¿Cuál es la corriente media en la carga?", N(vp, 0), N(rl, 0)), i, "mA",
                YF("Vmed = 2·Vp/π = {0} V; I = Vmed/R = {0}/{1} = {2} mA.", N(vm, 3), N(rl, 0), N(i, 2)));
        }),
        Difficulty.Hard => F(r =>
        {
            double i = S(r, 0.1, 1.5, 0.05), c = S(r, 470, 4700, 10), f = P(r, 50.0, 60), vr = i / (2 * f * c * 1e-6);
            return Q(XF("Um retificador de onda completa em {0} Hz alimenta uma carga de {1} A com um filtro de {2} µF. Qual a ondulação (ripple) pico a pico aproximada?", "A full-wave rectifier at {0} Hz feeds a {1} A load with a {2} µF filter. What is the approximate peak-to-peak ripple?", "Un rectificador de onda completa a {0} Hz alimenta una carga de {1} A con un filtro de {2} µF. ¿Cuál es el rizado pico a pico aproximado?", N(f, 0), N(i), N(c, 0)), vr, "V",
                YF("A ondulação tem frequência 2f = {0} Hz: Vr = I / (2·f·C) = {1} / ({0} · {2}·10⁻⁶) = {3} V.", N(2 * f, 0), N(i), N(c, 0), N(vr, 3)));
        }),
        _ => F(r =>
        {
            double v = P(r, 12.0, 15, 18, 24), i = P(r, 0.5, 1.0, 1.5, 2.0), f = 60, vrT = P(r, 1.0, 1.5, 2.0), c = i / (2 * f * vrT) * 1e6;
            double vp = v * Sqrt(2) - 1.4;
            return Q(XF("Fonte com ponte retificadora (0,7 V por diodo), secundário de {0} V RMS / 60 Hz e carga de {1} A. Que capacitor de filtro, em µF, mantém a ondulação pico a pico em {2} V?", "Supply with a bridge rectifier (0.7 V per diode), a {0} V RMS / 60 Hz secondary and a {1} A load. What filter capacitor, in µF, keeps the peak-to-peak ripple at {2} V?", "Fuente con puente rectificador (0,7 V por diodo), secundario de {0} V RMS / 60 Hz y carga de {1} A. ¿Qué condensador de filtro, en µF, mantiene el rizado pico a pico en {2} V?", N(v, 0), N(i), N(vrT)), c, "µF",
                YF("C = I / (2·f·Vr) = {0} / (120 · {1}) = {2} µF. (Pico após os diodos: {3} V; tensão média ≈ {4} V.)", N(i), N(vrT), N(c, 0), N(vp, 2), N(vp - vrT / 2, 2)));
        }, r =>
        {
            double v = P(r, 12.0, 15, 18, 24), i = P(r, 0.5, 1.0, 2.0), c = P(r, 2200.0, 4700, 10000), vp = v * Sqrt(2) - 1.4, vr = i / (2 * 60 * c * 1e-6), vdc = vp - vr / 2;
            return Q(XF("Fonte com ponte retificadora (0,7 V por diodo), secundário de {0} V RMS / 60 Hz, filtro de {1} µF e carga de {2} A. Qual a tensão contínua média aproximada na carga?", "Supply with a bridge rectifier (0.7 V per diode), a {0} V RMS / 60 Hz secondary, a {1} µF filter and a {2} A load. What is the approximate average DC voltage at the load?", "Fuente con puente rectificador (0,7 V por diodo), secundario de {0} V RMS / 60 Hz, filtro de {1} µF y carga de {2} A. ¿Cuál es la tensión continua media aproximada en la carga?", N(v, 0), N(c, 0), N(i)), vdc, "V",
                YF("Vp = {0}·√2 − 1,4 = {1} V; ripple Vr = I/(2fC) = {2} V; Vdc ≈ Vp − Vr/2 = {3} V.", N(v, 0), N(vp, 2), N(vr, 2), N(vdc, 2)));
        })
    };
}

[Exercises("ele-10-trifasico")]
public sealed class Ele10ThreePhase : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double vf = S(r, 100, 450, 1);
            return r.Next(2) == 0
                ? Q(XF("Um sistema trifásico em estrela tem tensão de fase {0} V. Qual a tensão de linha?", "A wye three-phase system has a phase voltage of {0} V. What is the line voltage?", "Un sistema trifásico en estrella tiene tensión de fase {0} V. ¿Cuál es la tensión de línea?", N(vf, 0)), vf * Sqrt(3), "V", YF("VL = √3·Vf = 1,732 · {0} = {1} V.", N(vf, 0), N(vf * Sqrt(3), 1)))
                : Q(XF("Um sistema trifásico em estrela tem tensão de linha {0} V. Qual a tensão de fase?", "A wye three-phase system has a line voltage of {0} V. What is the phase voltage?", "Un sistema trifásico en estrella tiene tensión de línea {0} V. ¿Cuál es la tensión de fase?", N(vf * Sqrt(3), 0)), vf, "V", YF("Vf = VL/√3 = {0} / 1,732 = {1} V.", N(vf * Sqrt(3), 0), N(vf, 1)));
        }),
        Difficulty.Medium => F(r =>
        {
            double vl = P(r, 220.0, 380, 440, 480), il = S(r, 5, 80, 1), fp = P(r, 0.7, 0.8, 0.85, 0.9, 1.0), p = Sqrt(3) * vl * il * fp;
            return Q(XF("Carga trifásica equilibrada: tensão de linha {0} V, corrente de linha {1} A, fator de potência {2}. Qual a potência ativa total?", "Balanced three-phase load: line voltage {0} V, line current {1} A, power factor {2}. What is the total real power?", "Carga trifásica equilibrada: tensión de línea {0} V, corriente de línea {1} A, factor de potencia {2}. ¿Cuál es la potencia activa total?", N(vl, 0), N(il, 0), N(fp, 2)), p / 1000, "kW",
                YF("P = √3·VL·IL·cos φ = 1,732 · {0} · {1} · {2} = {3} kW.", N(vl, 0), N(il, 0), N(fp, 2), N(p / 1000, 3)));
        }),
        Difficulty.Hard => F(r =>
        {
            double vl = P(r, 220.0, 380, 440), R = S(r, 8, 60, 1);
            double iy = vl / Sqrt(3) / R, id = vl / R, il = Sqrt(3) * id, py = 3 * iy * iy * R, pd = 3 * id * id * R;
            string intro = XF("Três resistores de {0} Ω são ligados a uma rede trifásica de {1} V (tensão de linha).", "Three {0} Ω resistors are connected to a three-phase network of {1} V (line voltage).", "Tres resistencias de {0} Ω se conectan a una red trifásica de {1} V (tensión de línea).", N(R, 0), N(vl, 0));
            return r.Next(3) switch
            {
                0 => Q(intro + " " + X("Em estrela, qual a corrente de linha?", "In wye, what is the line current?", "En estrella, ¿cuál es la corriente de línea?"), iy, "A", YF("Vf = {0}/√3 = {1} V; IL = If = Vf/R = {2} A.", N(vl, 0), N(vl / Sqrt(3), 2), N(iy, 3))),
                1 => Q(intro + " " + X("Em triângulo, qual a corrente de linha?", "In delta, what is the line current?", "En triángulo, ¿cuál es la corriente de línea?"), il, "A", YF("If = VL/R = {0} A; IL = √3·If = {1} A.", N(id, 3), N(il, 3))),
                _ => Q(intro + " " + X("Em triângulo, qual a potência total dissipada?", "In delta, what is the total power dissipated?", "En triángulo, ¿cuál es la potencia total disipada?"), pd / 1000, "kW", YF("P = 3·If²·R = 3 · {0}² · {1} = {2} kW (em estrela seria {3} kW, 3 vezes menos).", N(id, 3), N(R, 0), N(pd / 1000, 3), N(py / 1000, 3)))
            };
        }),
        _ => F(r =>
        {
            double vl = P(r, 220.0, 380, 440), pm = P(r, 5.0, 7.5, 10, 15, 22, 30), eta = P(r, 0.85, 0.9, 0.92, 0.95), fp = P(r, 0.8, 0.85, 0.88, 0.9);
            double il = pm * 1000 / (Sqrt(3) * vl * eta * fp);
            return Q(XF("Um motor trifásico de {0} kW (potência no eixo), rendimento {1}% e fator de potência {2}, é ligado em {3} V. Qual a corrente de linha?", "A three-phase motor of {0} kW (shaft power), efficiency {1}% and power factor {2}, is connected to {3} V. What is the line current?", "Un motor trifásico de {0} kW (potencia en el eje), rendimiento {1}% y factor de potencia {2}, se conecta a {3} V. ¿Cuál es la corriente de línea?", N(pm), N(eta * 100, 0), N(fp, 2), N(vl, 0)), il, "A",
                YF("Pentrada = {0}/{1} = {2} kW; IL = Pent / (√3·VL·cos φ) = {3} / (1,732 · {4} · {5}) = {6} A.", N(pm), N(eta, 2), N(pm / eta, 3), N(pm / eta * 1000, 0), N(vl, 0), N(fp, 2), N(il, 2)));
        }, r =>
        {
            double vl = P(r, 220.0, 380, 440), s = P(r, 20.0, 40, 60, 100), fp = P(r, 0.6, 0.7, 0.8, 0.9), q = s * Sqrt(1 - fp * fp);
            return Q(XF("Uma carga trifásica equilibrada consome {0} kVA com fator de potência {1} em {2} V. Qual a potência reativa total?", "A balanced three-phase load uses {0} kVA with power factor {1} at {2} V. What is the total reactive power?", "Una carga trifásica equilibrada consume {0} kVA con factor de potencia {1} a {2} V. ¿Cuál es la potencia reactiva total?", N(s, 0), N(fp, 2), N(vl, 0)), q, "kvar",
                YF("sen φ = √(1 − {0}²) = {1}; Q = S·sen φ = {2} · {1} = {3} kvar.", N(fp, 2), N(Sqrt(1 - fp * fp), 4), N(s, 0), N(q, 3)));
        })
    };
}
