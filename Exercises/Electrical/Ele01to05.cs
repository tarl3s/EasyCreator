namespace EasyCreator.Exercises;

[Exercises("ele-01-ohm")]
public sealed class Ele01Ohm : PoolGen
{
    protected override string NoteFor(string prompt) =>
        prompt.Contains("20 °C") && !prompt.Contains("α") ? L.T("(Cobre: α = 0,0039 /°C.)") : "";

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double R = P(r, 4.7, 10, 22, 33, 47, 100), V = S(r, 5, 48, 1), I = V / R;
            return r.Next(3) switch
            {
                0 => Q(XF("Um resistor de {0} Ω está ligado a uma fonte de {1} V. Qual a corrente que passa por ele?", "A {0} Ω resistor is connected to a {1} V source. What current flows through it?", "Una resistencia de {0} Ω está conectada a una fuente de {1} V. ¿Qué corriente pasa por ella?", N(R), N(V)), I, "A",
                    YF("I = V / R = {0} / {1} = {2} A.", N(V), N(R), N(I))),
                1 => Q(XF("Por um resistor de {0} Ω passa uma corrente de {1} A. Qual a tensão sobre ele?", "A current of {1} A flows through a {0} Ω resistor. What is the voltage across it?", "Por una resistencia de {0} Ω pasa una corriente de {1} A. ¿Cuál es la tensión en ella?", N(R), N(I)), V, "V",
                    YF("V = R · I = {0} · {1} = {2} V.", N(R), N(I), N(V))),
                _ => Q(XF("Um componente submetido a {0} V é percorrido por {1} A. Qual a sua resistência?", "A component with {0} V across it carries {1} A. What is its resistance?", "Un componente sometido a {0} V es recorrido por {1} A. ¿Cuál es su resistencia?", N(V), N(I)), V / I, "Ω",
                    YF("R = V / I = {0} / {1} = {2} Ω.", N(V), N(I), N(V / I)))
            };
        }),
        Difficulty.Medium => F(r =>
        {
            double I = S(r, 0.5, 8, 0.5), t = Ex.Int(r, 2, 90), qc = I * t * 60;
            return r.Next(2) == 0
                ? Q(XF("Uma corrente de {0} A circula por {1} min. Que carga elétrica atravessa o fio?", "A current of {0} A flows for {1} min. What charge goes through the wire?", "Una corriente de {0} A circula durante {1} min. ¿Qué carga eléctrica atraviesa el cable?", N(I), N(t)), qc, "C",
                    YF("Q = I · t = {0} · ({1} · 60 s) = {2} C.", N(I), N(t), N(qc)))
                : Q(XF("Uma corrente de {0} A circula por {1} min. Quantos elétrons atravessam o fio? (e = 1,6·10⁻¹⁹ C)", "A current of {0} A flows for {1} min. How many electrons go through the wire? (e = 1.6·10⁻¹⁹ C)", "Una corriente de {0} A circula durante {1} min. ¿Cuántos electrones atraviesan el cable? (e = 1,6·10⁻¹⁹ C)", N(I), N(t)), qc / 1.6e-19, X("elétrons", "electrons", "electrones"),
                    XF("Q = I · t = {0} C; N = Q / e = {0} / 1,6·10⁻¹⁹ = {1}.", "Q = I · t = {0} C; N = Q / e = {0} / 1.6·10⁻¹⁹ = {1}.", "Q = I · t = {0} C; N = Q / e = {0} / 1,6·10⁻¹⁹ = {1}.", N(qc), Sci(qc / 1.6e-19)));
        }, r =>
        {
            double L = S(r, 5, 200, 5), s = P(r, 0.5, 0.75, 1, 1.5, 2.5, 4, 6), R = 0.0168 * L / s;
            string f = X("Resistência: R = ρ·L / S, com ρ(cobre) = 0,0168 Ω·mm²/m.", "Resistance: R = ρ·L / S, with ρ(copper) = 0.0168 Ω·mm²/m.", "Resistencia: R = ρ·L / S, con ρ(cobre) = 0,0168 Ω·mm²/m.");
            return r.Next(2) == 0
                ? Q(XF("Um fio de cobre (ρ = 0,0168 Ω·mm²/m) tem {0} m e seção de {1} mm². Qual a sua resistência?", "A copper wire (ρ = 0.0168 Ω·mm²/m) is {0} m long with a {1} mm² cross-section. What is its resistance?", "Un cable de cobre (ρ = 0,0168 Ω·mm²/m) mide {0} m y tiene sección de {1} mm². ¿Cuál es su resistencia?", N(L), N(s)), R, "Ω",
                    f + " " + XF("R = 0,0168 · {0} / {1} = {2} Ω.", "R = 0.0168 · {0} / {1} = {2} Ω.", "R = 0,0168 · {0} / {1} = {2} Ω.", N(L), N(s), N(R)))
                : Q(XF("Um fio de cobre (ρ = 0,0168 Ω·mm²/m) de seção {0} mm² deve ter resistência de {1} Ω. Qual o seu comprimento?", "A copper wire (ρ = 0.0168 Ω·mm²/m) with {0} mm² cross-section must have a resistance of {1} Ω. How long must it be?", "Un cable de cobre (ρ = 0,0168 Ω·mm²/m) de sección {0} mm² debe tener una resistencia de {1} Ω. ¿Qué longitud debe tener?", N(s), N(R, 3)), L, "m",
                    f + " " + XF("L = R·S / ρ = {0} · {1} / 0,0168 = {2} m.", "L = R·S / ρ = {0} · {1} / 0.0168 = {2} m.", "L = R·S / ρ = {0} · {1} / 0,0168 = {2} m.", N(R, 3), N(s), N(L)));
        }),
        Difficulty.Hard => F(r =>
        {
            double V = P(r, 127.0, 220), Pw = S(r, 800, 6000, 100), R = V * V / Pw, I = Pw / V;
            return r.Next(3) switch
            {
                0 => Q(XF("Um chuveiro de {0} W / {1} V. Qual a resistência do seu elemento de aquecimento?", "A {0} W / {1} V shower heater. What is the resistance of its heating element?", "Una ducha eléctrica de {0} W / {1} V. ¿Cuál es la resistencia de su elemento calefactor?", N(Pw, 0), N(V, 0)), R, "Ω",
                    YF("P = V²/R → R = V²/P = {0}² / {1} = {2} Ω.", N(V, 0), N(Pw, 0), N(R))),
                1 => Q(XF("Um aquecedor de {0} W / {1} V está ligado corretamente. Qual a corrente que ele consome?", "A {0} W / {1} V heater is connected correctly. What current does it draw?", "Un calentador de {0} W / {1} V está bien conectado. ¿Qué corriente consume?", N(Pw, 0), N(V, 0)), I, "A",
                    YF("P = V·I → I = P/V = {0} / {1} = {2} A.", N(Pw, 0), N(V, 0), N(I))),
                _ => Q(XF("Um aquecedor de resistência {0} Ω é ligado a {1} V. Que potência ele dissipa?", "A heater of resistance {0} Ω is connected to {1} V. What power does it dissipate?", "Un calentador de resistencia {0} Ω se conecta a {1} V. ¿Qué potencia disipa?", N(R), N(V, 0)), Pw, "W",
                    YF("P = V²/R = {0}² / {1} = {2} W.", N(V, 0), N(R), N(Pw)))
            };
        }),
        _ => F(r =>
        {
            double R0 = S(r, 5, 40, 1), n = P(r, 2.0, 3, 4, 1.5, 2.5), Rn = R0 * n * n;
            return Q(XF("Um fio de resistência {0} Ω é esticado até ficar {1} vezes mais comprido (o volume do metal não muda, então a seção diminui). Qual a nova resistência?", "A wire with a resistance of {0} Ω is stretched until it is {1} times longer (the volume of metal does not change, so the cross-section shrinks). What is the new resistance?", "Un cable de resistencia {0} Ω se estira hasta quedar {1} veces más largo (el volumen del metal no cambia, así que la sección disminuye). ¿Cuál es la nueva resistencia?", N(R0), N(n)), Rn, "Ω",
                YF("L' = {0}·L e, como o volume é constante, S' = S/{0}. R' = ρL'/S' = {0}²·R = {1} · {2} = {3} Ω.", N(n), N(n * n), N(R0), N(Rn)));
        }, r =>
        {
            double R0 = S(r, 5, 40, 0.5), dT = S(r, 20, 80, 5), Rh = R0 * (1 + 0.0039 * dT), V = P(r, 12.0, 24, 48);
            string f = X("Para o cobre, R = R₀·(1 + α·ΔT) com α = 0,0039 /°C.", "For copper, R = R₀·(1 + α·ΔT) with α = 0.0039 /°C.", "Para el cobre, R = R₀·(1 + α·ΔT) con α = 0,0039 /°C.");
            return r.Next(2) == 0
                ? Q(XF("O enrolamento de cobre de um motor mede {0} Ω a 20 °C. Depois de funcionar, ele aquece {1} °C acima disso. Ligado a {2} V (constantes), qual a corrente nele agora?", "A motor's copper winding measures {0} Ω at 20 °C. After running, it is {1} °C hotter. Connected to {2} V (constant), what is the current in it now?", "El bobinado de cobre de un motor mide {0} Ω a 20 °C. Tras funcionar, se calienta {1} °C más. Conectado a {2} V (constantes), ¿cuál es ahora la corriente?", N(R0), N(dT, 0), N(V, 0)), V / Rh, "A",
                    f + " " + XF("R = {0}·(1 + 0,0039·{1}) = {2} Ω; I = V/R = {3} / {2} = {4} A.", "R = {0}·(1 + 0.0039·{1}) = {2} Ω; I = V/R = {3} / {2} = {4} A.", "R = {0}·(1 + 0,0039·{1}) = {2} Ω; I = V/R = {3} / {2} = {4} A.", N(R0), N(dT, 0), N(Rh, 3), N(V, 0), N(V / Rh, 3)))
                : Q(XF("O enrolamento de cobre de um motor mede {0} Ω a 20 °C e, depois de funcionar, {1} Ω. Quantos graus ele esquentou?", "A motor's copper winding measures {0} Ω at 20 °C and {1} Ω after running. By how many degrees did it heat up?", "El bobinado de cobre de un motor mide {0} Ω a 20 °C y, tras funcionar, {1} Ω. ¿Cuántos grados se calentó?", N(R0), N(Rh, 3)), dT, "°C",
                    f + " " + XF("ΔT = (R/R₀ − 1)/α = ({0}/{1} − 1)/0,0039 = {2} °C.", "ΔT = (R/R₀ − 1)/α = ({0}/{1} − 1)/0.0039 = {2} °C.", "ΔT = (R/R₀ − 1)/α = ({0}/{1} − 1)/0,0039 = {2} °C.", N(Rh, 3), N(R0), N(dT, 1)));
        })
    };
}

[Exercises("ele-02-serie-paralelo")]
public sealed class Ele02SeriesParallel : PoolGen
{
    protected override string NoteFor(string prompt) =>
        prompt.Contains("20 °C") && !prompt.Contains("α") ? L.T("(Cobre: α = 0,0039 /°C.)") : "";

    static double Par(double a, double b) => a * b / (a + b);

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double a = P(r, 10.0, 22, 47, 100, 150, 220), b = P(r, 33.0, 68, 120, 330, 470), c = P(r, 15.0, 56, 82, 180, 390);
            return Q(XF("Três resistores de {0} Ω, {1} Ω e {2} Ω são ligados em série. Qual a resistência equivalente?", "Three resistors of {0} Ω, {1} Ω and {2} Ω are connected in series. What is the equivalent resistance?", "Tres resistencias de {0} Ω, {1} Ω y {2} Ω se conectan en serie. ¿Cuál es la resistencia equivalente?", N(a, 0), N(b, 0), N(c, 0)), a + b + c, "Ω",
                YF("Em série as resistências somam: {0} + {1} + {2} = {3} Ω.", N(a, 0), N(b, 0), N(c, 0), N(a + b + c, 0)));
        }, r =>
        {
            double a = P(r, 10.0, 22, 47, 100, 150, 220), b = P(r, 33.0, 68, 120, 330, 470);
            return Q(XF("Dois resistores, {0} Ω e {1} Ω, são ligados em paralelo. Qual a resistência equivalente?", "Two resistors, {0} Ω and {1} Ω, are connected in parallel. What is the equivalent resistance?", "Dos resistencias, {0} Ω y {1} Ω, se conectan en paralelo. ¿Cuál es la resistencia equivalente?", N(a, 0), N(b, 0)), Par(a, b), "Ω",
                YF("Req = R₁·R₂ / (R₁ + R₂) = {0}·{1} / {2} = {3} Ω.", N(a, 0), N(b, 0), N(a + b, 0), N(Par(a, b))));
        }, r =>
        {
            double R = P(r, 100.0, 120, 220, 330, 470, 600, 1000); int n = Ex.Int(r, 3, 8);
            return Q(XF("{0} resistores iguais de {1} Ω são ligados em paralelo. Qual a resistência equivalente?", "{0} identical {1} Ω resistors are connected in parallel. What is the equivalent resistance?", "{0} resistencias iguales de {1} Ω se conectan en paralelo. ¿Cuál es la resistencia equivalente?", n, N(R, 0)), R / n, "Ω",
                YF("Resistores iguais em paralelo: Req = R / n = {0} / {1} = {2} Ω.", N(R, 0), n, N(R / n)));
        }),
        Difficulty.Medium => F(r =>
        {
            double V = S(r, 6, 48, 2), a = P(r, 10.0, 22, 47, 100), b = P(r, 15.0, 33, 68, 120);
            return r.Next(2) == 0
                ? Q(XF("Uma fonte de {0} V alimenta {1} Ω e {2} Ω em série. Qual a corrente?", "A {0} V source feeds {1} Ω and {2} Ω in series. What is the current?", "Una fuente de {0} V alimenta {1} Ω y {2} Ω en serie. ¿Cuál es la corriente?", N(V, 0), N(a, 0), N(b, 0)), V / (a + b), "A",
                    YF("I = V / (R₁ + R₂) = {0} / {1} = {2} A.", N(V, 0), N(a + b, 0), N(V / (a + b), 3)))
                : Q(XF("Uma fonte de {0} V alimenta {1} Ω e {2} Ω em paralelo. Qual a corrente total fornecida pela fonte?", "A {0} V source feeds {1} Ω and {2} Ω in parallel. What total current does the source supply?", "Una fuente de {0} V alimenta {1} Ω y {2} Ω en paralelo. ¿Qué corriente total entrega la fuente?", N(V, 0), N(a, 0), N(b, 0)), V / a + V / b, "A",
                    YF("Cada ramo vê {0} V: {0}/{1} + {0}/{2} = {3} A.", N(V, 0), N(a, 0), N(b, 0), N(V / a + V / b, 3)));
        }, r =>
        {
            double V = S(r, 5, 24, 1), r1 = P(r, 1000.0, 2200, 4700, 10000), r2 = P(r, 1000.0, 3300, 6800, 10000), vo = V * r2 / (r1 + r2);
            return r.Next(2) == 0
                ? Q(XF("Divisor de tensão: {0} V em R₁ = {1} Ω em série com R₂ = {2} Ω. Qual a tensão sobre R₂?", "Voltage divider: {0} V across R₁ = {1} Ω in series with R₂ = {2} Ω. What is the voltage across R₂?", "Divisor de tensión: {0} V sobre R₁ = {1} Ω en serie con R₂ = {2} Ω. ¿Cuál es la tensión en R₂?", N(V, 0), N(r1, 0), N(r2, 0)), vo, "V",
                    YF("Vout = V·R₂/(R₁+R₂) = {0}·{1}/{2} = {3} V.", N(V, 0), N(r2, 0), N(r1 + r2, 0), N(vo, 3)))
                : Q(XF("Divisor de tensão: {0} V em R₁ = {1} Ω em série com R₂ = {2} Ω. Qual a corrente?", "Voltage divider: {0} V across R₁ = {1} Ω in series with R₂ = {2} Ω. What is the current?", "Divisor de tensión: {0} V sobre R₁ = {1} Ω en serie con R₂ = {2} Ω. ¿Cuál es la corriente?", N(V, 0), N(r1, 0), N(r2, 0)), V / (r1 + r2) * 1000, "mA",
                    YF("I = V/(R₁+R₂) = {0}/{1} = {2} mA.", N(V, 0), N(r1 + r2, 0), N(V / (r1 + r2) * 1000, 3)));
        }),
        Difficulty.Hard => F(r =>
        {
            double V = S(r, 6, 48, 2), a = P(r, 10.0, 22, 47, 100), b = P(r, 30.0, 60, 90, 120), c = P(r, 60.0, 120, 180, 240);
            double rp = Par(b, c), req = a + rp, I = V / req;
            string intro = XF("R₁ = {0} Ω está em série com o par R₂ = {1} Ω e R₃ = {2} Ω ligados em paralelo, tudo alimentado por {3} V.", "R₁ = {0} Ω is in series with the pair R₂ = {1} Ω and R₃ = {2} Ω connected in parallel, all fed by {3} V.", "R₁ = {0} Ω está en serie con el par R₂ = {1} Ω y R₃ = {2} Ω conectados en paralelo, todo alimentado por {3} V.", N(a, 0), N(b, 0), N(c, 0), N(V, 0));
            string st = YF("R₂ ∥ R₃ = {0} Ω; Req = R₁ + R₂∥R₃ = {1} Ω; I = V/Req = {2} A.", N(rp), N(req), N(I, 3));
            return r.Next(3) switch
            {
                0 => Q(intro + " " + X("Qual a resistência equivalente?", "What is the equivalent resistance?", "¿Cuál es la resistencia equivalente?"), req, "Ω", st),
                1 => Q(intro + " " + X("Qual a corrente fornecida pela fonte?", "What current does the source supply?", "¿Qué corriente entrega la fuente?"), I, "A", st),
                _ => Q(intro + " " + X("Qual a tensão sobre o par em paralelo?", "What is the voltage across the parallel pair?", "¿Cuál es la tensión en el par en paralelo?"), I * rp, "V", st + " " + YF("V∥ = I·(R₂∥R₃) = {0} V.", N(I * rp, 3)))
            };
        }),
        _ => F(r =>
        {
            double R = P(r, 60.0, 100, 120, 150, 220, 300), rt = Math.Round(R * S(r, 0.3, 0.8, 0.05), 1), x = R * rt / (R - rt);
            return Q(XF("Que resistor se deve colocar em paralelo com um de {0} Ω para obter uma resistência equivalente de {1} Ω?", "What resistor must be placed in parallel with a {0} Ω one to get an equivalent resistance of {1} Ω?", "¿Qué resistencia hay que poner en paralelo con una de {0} Ω para obtener una resistencia equivalente de {1} Ω?", N(R, 0), N(rt)), x, "Ω",
                YF("1/x = 1/Req − 1/R → x = Req·R/(R − Req) = {0}·{1}/({1} − {0}) = {2} Ω.", N(rt), N(R, 0), N(x)));
        }, r =>
        {
            double Vs = P(r, 12.0, 24), V = P(r, 2.0, 3, 1.5, 3.3), I = P(r, 0.02, 0.05, 0.1, 0.2); int n = Ex.Int(r, 2, 4);
            if (n * V >= Vs - 1) n = 1 + (int)((Vs - 2) / V) - 1;
            n = Math.Max(n, 1);
            double Rs = (Vs - n * V) / I;
            return Q(XF("{0} lâmpadas de {1} V / {2} A são ligadas em série e alimentadas por {3} V. Que resistor em série é preciso para que cada lâmpada trabalhe na tensão nominal?", "{0} lamps rated {1} V / {2} A are connected in series and fed by {3} V. What series resistor is needed so each lamp runs at its rated voltage?", "{0} lámparas de {1} V / {2} A se conectan en serie y se alimentan con {3} V. ¿Qué resistencia en serie se necesita para que cada lámpara trabaje a su tensión nominal?", n, N(V), N(I), N(Vs, 0)), Rs, "Ω",
                YF("As lâmpadas ficam com {0}·{1} = {2} V; sobram {3} V para o resistor, com {4} A: R = {3}/{4} = {5} Ω.", n, N(V), N(n * V), N(Vs - n * V), N(I), N(Rs)));
        })
    };
}

[Exercises("ele-03-kirchhoff")]
public sealed class Ele03Kirchhoff : PoolGen
{
    protected override string NoteFor(string prompt) =>
        prompt.Contains("20 °C") && !prompt.Contains("α") ? L.T("(Cobre: α = 0,0039 /°C.)") : "";

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double i1 = S(r, 1, 6, 0.5), i2 = S(r, 1, 6, 0.5), i3 = S(r, 0.5, 4, 0.5), i4 = i1 + i2 - i3;
            return Q(XF("Em um nó entram {0} A e {1} A e saem {2} A por um terceiro fio. Quanta corrente sai pelo quarto fio?", "At a node, {0} A and {1} A come in and {2} A leaves through a third wire. How much current leaves through the fourth wire?", "En un nodo entran {0} A y {1} A y salen {2} A por un tercer cable. ¿Cuánta corriente sale por el cuarto cable?", N(i1), N(i2), N(i3)), i4, "A",
                YF("Lei dos nós: entra = sai → {0} + {1} = {2} + I → I = {3} A.", N(i1), N(i2), N(i3), N(i4)));
        }),
        Difficulty.Medium => F(r =>
        {
            double v1 = S(r, 12, 30, 1), v2 = S(r, 3, 10, 1), r1 = P(r, 10.0, 20, 40), r2 = P(r, 10.0, 30, 50), I = (v1 - v2) / (r1 + r2);
            string intro = XF("Uma malha simples tem uma bateria de {0} V, outra de {1} V em oposição, e dois resistores de {2} Ω e {3} Ω.", "A single loop has a {0} V battery, another of {1} V opposing it, and two resistors of {2} Ω and {3} Ω.", "Una malla simple tiene una batería de {0} V, otra de {1} V en oposición y dos resistencias de {2} Ω y {3} Ω.", N(v1, 0), N(v2, 0), N(r1, 0), N(r2, 0));
            string st = YF("Lei das malhas: {0} − {1} − I·({2} + {3}) = 0 → I = {4} A.", N(v1, 0), N(v2, 0), N(r1, 0), N(r2, 0), N(I, 3));
            return r.Next(2) == 0
                ? Q(intro + " " + X("Qual a corrente?", "What is the current?", "¿Cuál es la corriente?"), I, "A", st)
                : Q(intro + " " + XF("Qual a tensão sobre o resistor de {0} Ω?", "What is the voltage across the {0} Ω resistor?", "¿Cuál es la tensión en la resistencia de {0} Ω?", N(r1, 0)), I * r1, "V", st + " " + YF("V = I·R = {0} V.", N(I * r1, 3)));
        }),
        Difficulty.Hard => F(r =>
        {
            double v1 = S(r, 10, 30, 1), v2 = S(r, 4, 9, 1), r1 = P(r, 10.0, 20, 40), r2 = P(r, 20.0, 40, 80), r3 = P(r, 10.0, 25, 50);
            double vn = (v1 / r1 + v2 / r3) / (1 / r1 + 1 / r2 + 1 / r3);
            return Q(XF("Duas fontes (V₁ = {0} V com R₁ = {1} Ω e V₂ = {2} V com R₃ = {3} Ω) alimentam o mesmo nó, que tem R₂ = {4} Ω ligado ao terra. Qual a tensão no nó?", "Two sources (V₁ = {0} V with R₁ = {1} Ω and V₂ = {2} V with R₃ = {3} Ω) feed the same node, which has R₂ = {4} Ω to ground. What is the node voltage?", "Dos fuentes (V₁ = {0} V con R₁ = {1} Ω y V₂ = {2} V con R₃ = {3} Ω) alimentan el mismo nodo, que tiene R₂ = {4} Ω a tierra. ¿Cuál es la tensión del nodo?", N(v1, 0), N(r1, 0), N(v2, 0), N(r3, 0), N(r2, 0)), vn, "V",
                YF("Nó: (V₁−V)/R₁ + (V₂−V)/R₃ = V/R₂ → V = (V₁/R₁ + V₂/R₃)/(1/R₁ + 1/R₂ + 1/R₃) = {0} V.", N(vn, 3)));
        }),
        _ => F(r =>
        {
            double v1 = S(r, 15, 30, 1), v2 = S(r, 4, 12, 1), r1 = P(r, 10.0, 20, 40), r2 = P(r, 20.0, 40, 80), r3 = P(r, 10.0, 25, 50);
            double vn = (v1 / r1 + v2 / r3) / (1 / r1 + 1 / r2 + 1 / r3), i1 = (v1 - vn) / r1;
            string intro = XF("V₁ = {0} V com R₁ = {1} Ω e V₂ = {2} V com R₃ = {3} Ω alimentam um nó ligado ao terra por R₂ = {4} Ω.", "V₁ = {0} V with R₁ = {1} Ω and V₂ = {2} V with R₃ = {3} Ω feed a node tied to ground by R₂ = {4} Ω.", "V₁ = {0} V con R₁ = {1} Ω y V₂ = {2} V con R₃ = {3} Ω alimentan un nodo conectado a tierra por R₂ = {4} Ω.", N(v1, 0), N(r1, 0), N(v2, 0), N(r3, 0), N(r2, 0));
            string st = YF("V_nó = (V₁/R₁ + V₂/R₃)/(1/R₁ + 1/R₂ + 1/R₃) = {0} V.", N(vn, 3));
            return r.Next(2) == 0
                ? Q(intro + " " + X("Qual a corrente que sai da fonte V₁?", "What current leaves source V₁?", "¿Qué corriente sale de la fuente V₁?"), i1, "A", st + " " + YF("I₁ = (V₁ − V)/R₁ = {0} A.", N(i1, 3)))
                : Q(intro + " " + X("Qual a potência dissipada em R₂?", "What power is dissipated in R₂?", "¿Qué potencia se disipa en R₂?"), vn * vn / r2, "W", st + " " + YF("P = V²/R₂ = {0} W.", N(vn * vn / r2, 3)));
        })
    };
}

[Exercises("ele-04-potencia-energia")]
public sealed class Ele04Power : PoolGen
{
    protected override string NoteFor(string prompt) =>
        prompt.Contains("20 °C") && !prompt.Contains("α") ? L.T("(Cobre: α = 0,0039 /°C.)") : "";

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double V = P(r, 5.0, 12, 24, 127, 220), I = S(r, 0.5, 10, 0.5), p = V * I;
            return r.Next(2) == 0
                ? Q(XF("Um aparelho ligado a {0} V puxa {1} A. Qual a sua potência?", "A device connected to {0} V draws {1} A. What is its power?", "Un aparato conectado a {0} V consume {1} A. ¿Cuál es su potencia?", N(V, 0), N(I)), p, "W", YF("P = V·I = {0} · {1} = {2} W.", N(V, 0), N(I), N(p)))
                : Q(XF("Um aparelho de {0} W é ligado a {1} V. Qual a corrente?", "A {0} W device is connected to {1} V. What is the current?", "Un aparato de {0} W se conecta a {1} V. ¿Cuál es la corriente?", N(p), N(V, 0)), I, "A", YF("I = P / V = {0} / {1} = {2} A.", N(p), N(V, 0), N(I)));
        }, r =>
        {
            double p = P(r, 60.0, 100, 500, 1000, 1500, 2000), h = S(r, 0.5, 10, 0.5), e = p * h / 1000;
            return Q(XF("Um aparelho de {0} W fica ligado por {1} h. Quantos kWh de energia ele consome?", "A {0} W device stays on for {1} h. How many kWh of energy does it use?", "Un aparato de {0} W queda encendido {1} h. ¿Cuántos kWh de energía consume?", N(p, 0), N(h)), e, "kWh",
                YF("E = P·t = {0} kW · {1} h = {2} kWh.", N(p / 1000, 3), N(h), N(e, 3)));
        }),
        Difficulty.Medium => F(r =>
        {
            double p = P(r, 100.0, 300, 800, 1200, 1500, 2200), h = S(r, 0.5, 6, 0.5), tar = S(r, 0.6, 1.2, 0.05), cost = p / 1000 * h * 30 * tar;
            return Q(XF("Um aparelho de {0} W é usado {1} h por dia durante 30 dias. Com a tarifa de R$ {2} por kWh, quanto ele custa no mês?", "A {0} W device is used {1} h per day for 30 days. With a tariff of {2} per kWh, how much does it cost per month?", "Un aparato de {0} W se usa {1} h por día durante 30 días. Con una tarifa de {2} por kWh, ¿cuánto cuesta al mes?", N(p, 0), N(h), N(tar)), cost, "R$",
                YF("E = {0} kW · {1} h · 30 = {2} kWh; custo = {2} · {3} = R$ {4}.", N(p / 1000, 3), N(h), N(p / 1000 * h * 30, 2), N(tar), N(cost)));
        }, r =>
        {
            double V = P(r, 3.7, 7.4, 12.0), Ah = P(r, 2.0, 5, 10, 20, 60), p = P(r, 2.0, 5, 10, 20, 50), wh = V * Ah;
            return Q(XF("Uma bateria de {0} V e {1} Ah alimenta uma carga de {2} W. Quantas horas ela dura (sem perdas)?", "A {0} V, {1} Ah battery powers a {2} W load. How many hours does it last (no losses)?", "Una batería de {0} V y {1} Ah alimenta una carga de {2} W. ¿Cuántas horas dura (sin pérdidas)?", N(V), N(Ah, 0), N(p, 0)), wh / p, "h",
                YF("Energia = V·Ah = {0} Wh; tempo = E/P = {0}/{1} = {2} h.", N(wh), N(p, 0), N(wh / p)));
        }),
        Difficulty.Hard => F(r =>
        {
            double m = P(r, 50.0, 100, 150, 200), dT = P(r, 15.0, 20, 25, 30), pw = P(r, 1500.0, 2000, 3000, 4500), eta = P(r, 0.8, 0.9, 0.95);
            double t = m * 4.18 * dT / (pw * eta / 1000);
            return Q(XF("Um aquecedor de {0} W com rendimento de {1}% esquenta {2} kg de água em {3} °C (c = 4,18 kJ/kg·°C). Quantos segundos leva?", "A {0} W heater with {1}% efficiency heats {2} kg of water by {3} °C (c = 4.18 kJ/kg·°C). How many seconds does it take?", "Un calentador de {0} W con rendimiento de {1}% calienta {2} kg de agua {3} °C (c = 4,18 kJ/kg·°C). ¿Cuántos segundos tarda?", N(pw, 0), N(eta * 100, 0), N(m, 0), N(dT, 0)), t, "s",
                YF("Calor útil = m·c·ΔT = {0}·4,18·{1} = {2} kJ; potência útil = {3} kW; t = {2}/{3} = {4} s.", N(m, 0), N(dT, 0), N(m * 4.18 * dT), N(pw * eta / 1000, 3), N(t)));
        }, r =>
        {
            double v = 127, pn = P(r, 1000.0, 1500, 2200, 4400), vv = P(r, 110.0, 115, 120, 100), pp = pn * vv * vv / (v * v);
            return Q(XF("Um aquecedor de {0} W / 127 V (resistência constante) é ligado a {1} V. Que potência ele passa a dissipar?", "A {0} W / 127 V heater (constant resistance) is connected to {1} V. What power does it now dissipate?", "Un calentador de {0} W / 127 V (resistencia constante) se conecta a {1} V. ¿Qué potencia disipa ahora?", N(pn, 0), N(vv, 0)), pp, "W",
                YF("R = 127²/{0} = {1} Ω; P' = V'²/R = {2}²/{1} = {3} W (P ∝ V²).", N(pn, 0), N(v * v / pn, 3), N(vv, 0), N(pp)));
        }),
        _ => F(r =>
        {
            double pi = P(r, 60.0, 75, 100), pl = P(r, 7.0, 9, 10, 12), h = P(r, 10000.0, 15000, 20000), tar = S(r, 0.6, 1.1, 0.05), hi = P(r, 1000.0, 1200), pr = S(r, 8, 25, 1);
            double nInc = h / hi, cInc = pi / 1000 * h * tar + nInc * 3, cLed = pl / 1000 * h * tar + pr;
            return Q(XF("Em {0} h de uso: uma lâmpada incandescente de {1} W (dura {2} h e custa R$ 3 cada) ou um LED de {3} W (dura toda a vida e custa R$ {4}). Com a tarifa de R$ {5}/kWh, quanto se economiza com o LED?", "Over {0} h of use: an incandescent bulb of {1} W (lasts {2} h and costs 3 each) or a {3} W LED (lasts the whole time and costs {4}). With a tariff of {5}/kWh, how much does the LED save?", "En {0} h de uso: una bombilla incandescente de {1} W (dura {2} h y cuesta 3 cada una) o un LED de {3} W (dura todo el tiempo y cuesta {4}). Con una tarifa de {5}/kWh, ¿cuánto se ahorra con el LED?", N(h, 0), N(pi, 0), N(hi, 0), N(pl, 0), N(pr, 0), N(tar)), cInc - cLed, "R$",
                YF("Incandescente: {0} kWh·{1} + {2} lâmpadas·R$ 3 = R$ {3}. LED: {4} kWh·{1} + R$ {5} = R$ {6}. Economia = R$ {7}.", N(pi / 1000 * h, 1), N(tar), N(nInc, 1), N(cInc), N(pl / 1000 * h, 1), N(pr, 0), N(cLed), N(cInc - cLed)));
        })
    };
}

[Exercises("ele-05-thevenin")]
public sealed class Ele05Thevenin : PoolGen
{
    protected override string NoteFor(string prompt) =>
        prompt.Contains("20 °C") && !prompt.Contains("α") ? L.T("(Cobre: α = 0,0039 /°C.)") : "";

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double V = S(r, 6, 48, 2), r1 = P(r, 10.0, 20, 30, 60, 100), r2 = P(r, 20.0, 40, 60, 120, 200), vth = V * r2 / (r1 + r2), rth = r1 * r2 / (r1 + r2);
            string intro = XF("Um divisor com {0} V, R₁ = {1} Ω em série e R₂ = {2} Ω para o terra. Visto dos terminais de R₂ (retirado o resistor):", "A divider with {0} V, series R₁ = {1} Ω and R₂ = {2} Ω to ground. Seen from R₂'s terminals (resistor removed):", "Un divisor con {0} V, R₁ = {1} Ω en serie y R₂ = {2} Ω a tierra. Visto desde los terminales de R₂ (retirada la resistencia):", N(V, 0), N(r1, 0), N(r2, 0));
            return r.Next(2) == 0
                ? Q(intro + " " + X("qual a tensão de Thévenin?", "what is the Thévenin voltage?", "¿cuál es la tensión de Thévenin?"), vth, "V", YF("Vth = V·R₂/(R₁+R₂) = {0}·{1}/{2} = {3} V.", N(V, 0), N(r2, 0), N(r1 + r2, 0), N(vth, 3)))
                : Q(intro + " " + X("qual a resistência de Thévenin?", "what is the Thévenin resistance?", "¿cuál es la resistencia de Thévenin?"), rth, "Ω", YF("Zerando a fonte, R₁ fica em paralelo com R₂: Rth = R₁∥R₂ = {0} Ω.", N(rth, 3)));
        }),
        Difficulty.Medium => F(r =>
        {
            double vth = S(r, 6, 40, 1), rth = P(r, 5.0, 10, 20, 50), rl = P(r, 10.0, 22, 47, 100), i = vth / (rth + rl);
            string intro = XF("Um circuito equivale a Vth = {0} V em série com Rth = {1} Ω. Ligando uma carga de {2} Ω:", "A circuit is equivalent to Vth = {0} V in series with Rth = {1} Ω. Connecting a {2} Ω load:", "Un circuito equivale a Vth = {0} V en serie con Rth = {1} Ω. Al conectar una carga de {2} Ω:", N(vth, 0), N(rth, 0), N(rl, 0));
            return r.Next(2) == 0
                ? Q(intro + " " + X("qual a corrente na carga?", "what is the load current?", "¿cuál es la corriente en la carga?"), i, "A", YF("I = Vth/(Rth + RL) = {0}/({1} + {2}) = {3} A.", N(vth, 0), N(rth, 0), N(rl, 0), N(i, 3)))
                : Q(intro + " " + X("qual a tensão na carga?", "what is the load voltage?", "¿cuál es la tensión en la carga?"), i * rl, "V", YF("I = {0} A; V = I·RL = {1} V.", N(i, 3), N(i * rl, 3)));
        }),
        Difficulty.Hard => F(r =>
        {
            double vth = S(r, 6, 40, 1), rth = P(r, 5.0, 8, 10, 20, 50), pm = vth * vth / (4 * rth);
            return r.Next(2) == 0
                ? Q(XF("Uma fonte com Vth = {0} V e Rth = {1} Ω alimenta uma carga variável. Que resistência de carga recebe a potência máxima?", "A source with Vth = {0} V and Rth = {1} Ω feeds a variable load. Which load resistance receives maximum power?", "Una fuente con Vth = {0} V y Rth = {1} Ω alimenta una carga variable. ¿Qué resistencia de carga recibe la potencia máxima?", N(vth, 0), N(rth, 0)), rth, "Ω",
                    X("Máxima transferência de potência: RL = Rth.", "Maximum power transfer: RL = Rth.", "Máxima transferencia de potencia: RL = Rth."))
                : Q(XF("Uma fonte com Vth = {0} V e Rth = {1} Ω alimenta uma carga variável. Qual a máxima potência que a carga pode receber?", "A source with Vth = {0} V and Rth = {1} Ω feeds a variable load. What is the maximum power the load can receive?", "Una fuente con Vth = {0} V y Rth = {1} Ω alimenta una carga variable. ¿Cuál es la máxima potencia que puede recibir la carga?", N(vth, 0), N(rth, 0)), pm, "W",
                    YF("RL = Rth = {1} Ω → I = Vth/(2·Rth); Pmax = Vth²/(4·Rth) = {0}²/(4·{1}) = {2} W.", N(vth, 0), N(rth, 0), N(pm, 3)));
        }),
        _ => F(r =>
        {
            double voc = P(r, 12.0, 9, 24, 5), rth = P(r, 0.5, 1, 2, 4, 10), isc = voc / rth;
            return Q(XF("Uma fonte mede {0} V em aberto e {1} A em curto-circuito. Qual a máxima potência que ela pode entregar a uma carga?", "A source measures {0} V open-circuit and {1} A short-circuit. What is the maximum power it can deliver to a load?", "Una fuente mide {0} V en vacío y {1} A en cortocircuito. ¿Cuál es la máxima potencia que puede entregar a una carga?", N(voc, 0), N(isc, 3)), voc * isc / 4, "W",
                YF("Rth = Voc/Isc = {0} Ω; Pmax = Voc²/(4·Rth) = Voc·Isc/4 = {1} W.", N(rth, 3), N(voc * isc / 4, 3)));
        }, r =>
        {
            double voc = P(r, 12.0, 9, 24, 6), rth = P(r, 0.2, 0.5, 1, 2), rl = P(r, 2.0, 4, 5, 8, 10), vl = voc * rl / (rth + rl);
            return Q(XF("Uma bateria mede {0} V sem carga e {1} V quando alimenta um resistor de {2} Ω. Qual a sua resistência interna?", "A battery measures {0} V with no load and {1} V when feeding a {2} Ω resistor. What is its internal resistance?", "Una batería mide {0} V sin carga y {1} V cuando alimenta una resistencia de {2} Ω. ¿Cuál es su resistencia interna?", N(voc, 0), N(vl, 4), N(rl, 0)), rth, "Ω",
                YF("I = V/RL = {0}/{1} = {2} A; a queda interna é {3} − {0} = {4} V; Rth = {4}/{2} = {5} Ω.", N(vl, 4), N(rl, 0), N(vl / rl, 4), N(voc, 0), N(voc - vl, 4), N(rth, 3)), 3);
        })
    };
}
