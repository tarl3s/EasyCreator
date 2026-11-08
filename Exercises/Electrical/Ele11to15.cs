namespace EasyCreator.Exercises;

[Exercises("ele-11-superposicao")]
public sealed class Ele11Superposition : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double i = S(r, 0.5, 5, 0.5), R = P(r, 10.0, 22, 47, 100);
            return Q(XF("Uma fonte de corrente de {0} A alimenta um resistor de {1} Ω. Qual a tensão sobre ele?", "A {0} A current source feeds a {1} Ω resistor. What is the voltage across it?", "Una fuente de corriente de {0} A alimenta una resistencia de {1} Ω. ¿Cuál es la tensión en ella?", N(i), N(R, 0)), i * R, "V", YF("V = I·R = {0} · {1} = {2} V.", N(i), N(R, 0), N(i * R)));
        }, r =>
        {
            double i = S(r, 0.5, 5, 0.5), r1 = P(r, 10.0, 20, 30, 60), r2 = P(r, 20.0, 40, 60, 90), v = i * r1 * r2 / (r1 + r2);
            return Q(XF("Uma fonte de corrente de {0} A alimenta dois resistores em paralelo, {1} Ω e {2} Ω. Qual a tensão sobre eles?", "A {0} A current source feeds two resistors in parallel, {1} Ω and {2} Ω. What is the voltage across them?", "Una fuente de corriente de {0} A alimenta dos resistencias en paralelo, {1} Ω y {2} Ω. ¿Cuál es la tensión en ellas?", N(i), N(r1, 0), N(r2, 0)), v, "V", YF("Rp = {0} Ω; V = I·Rp = {1} V.", N(r1 * r2 / (r1 + r2), 3), N(v, 3)));
        }),
        Difficulty.Medium => F(r =>
        {
            double v1 = S(r, 6, 30, 1), v2 = S(r, 3, 20, 1), r1 = P(r, 10.0, 20, 40), r2 = P(r, 10.0, 30, 50), r3 = P(r, 10.0, 25, 60);
            // Mesh equations: (R1+R3) I1 - R3 I2 = V1 ; -R3 I1 + (R2+R3) I2 = -V2 (I2 clockwise on the right)
            double a = r1 + r3, b = -r3, c = -r3, e = r2 + r3, det = a * e - b * c;
            double i1 = (v1 * e - b * (-v2)) / det, i2 = (a * (-v2) - c * v1) / det;
            string intro = XF("Duas malhas dividem o resistor R₃ = {0} Ω. Na esquerda, V₁ = {1} V com R₁ = {2} Ω; na direita, V₂ = {3} V com R₂ = {4} Ω, em oposição (as duas correntes de malha no sentido horário).", "Two loops share resistor R₃ = {0} Ω. On the left, V₁ = {1} V with R₁ = {2} Ω; on the right, V₂ = {3} V with R₂ = {4} Ω, opposing (both mesh currents clockwise).", "Dos mallas comparten la resistencia R₃ = {0} Ω. A la izquierda, V₁ = {1} V con R₁ = {2} Ω; a la derecha, V₂ = {3} V con R₂ = {4} Ω, en oposición (ambas corrientes de malla en sentido horario).", N(r3, 0), N(v1, 0), N(r1, 0), N(v2, 0), N(r2, 0));
            string st = YF("Malha 1: ({0}+{1})·I₁ − {1}·I₂ = {2}. Malha 2: −{1}·I₁ + ({3}+{1})·I₂ = −{4}. Resolvendo: I₁ = {5} A, I₂ = {6} A.", N(r1, 0), N(r3, 0), N(v1, 0), N(r2, 0), N(v2, 0), N(i1, 4), N(i2, 4));
            return r.Next(2) == 0
                ? Q(intro + " " + X("Qual a corrente de malha I₁?", "What is mesh current I₁?", "¿Cuál es la corriente de malla I₁?"), i1, "A", st)
                : Q(intro + " " + X("Qual a corrente em R₃ (I₁ − I₂), em módulo?", "What is the current in R₃ (I₁ − I₂), in magnitude?", "¿Cuál es la corriente en R₃ (I₁ − I₂), en módulo?"), Math.Abs(i1 - i2), "A", st + " " + YF("|I₁ − I₂| = {0} A.", N(Math.Abs(i1 - i2), 4)));
        }),
        Difficulty.Hard => F(r =>
        {
            double vs = S(r, 6, 30, 1), i = S(r, 0.2, 2, 0.1), r1 = P(r, 10.0, 20, 40), r2 = P(r, 20.0, 30, 60), rp = r1 * r2 / (r1 + r2);
            double vn = (vs / r1 + i) / (1 / r1 + 1 / r2);
            return Q(XF("Uma fonte de {0} V com R₁ = {1} Ω alimenta um nó que tem R₂ = {2} Ω para o terra e recebe também uma fonte de corrente de {3} A. Qual a tensão no nó?", "A {0} V source with R₁ = {1} Ω feeds a node that has R₂ = {2} Ω to ground and also receives a {3} A current source. What is the node voltage?", "Una fuente de {0} V con R₁ = {1} Ω alimenta un nodo que tiene R₂ = {2} Ω a tierra y recibe además una fuente de corriente de {3} A. ¿Cuál es la tensión del nodo?", N(vs, 0), N(r1, 0), N(r2, 0), N(i)), vn, "V",
                YF("Fonte de tensão sozinha: Vs·R₂/(R₁+R₂) = {0} V. Fonte de corrente sozinha: I·(R₁∥R₂) = {1} V. Soma (superposição): {2} V.", N(vs * r2 / (r1 + r2), 3), N(i * rp, 3), N(vn, 3)));
        }),
        _ => F(r =>
        {
            double vs = S(r, 6, 30, 1), i = S(r, 0.2, 2, 0.1), r1 = P(r, 10.0, 20, 40), r2 = P(r, 20.0, 30, 60), rp = r1 * r2 / (r1 + r2);
            double vn = (vs / r1 + i) / (1 / r1 + 1 / r2), iR2 = vn / r2;
            return Q(XF("Uma fonte de {0} V com R₁ = {1} Ω alimenta um nó que tem R₂ = {2} Ω para o terra e recebe também {3} A de uma fonte de corrente. Quanta potência R₂ dissipa?", "A {0} V source with R₁ = {1} Ω feeds a node that has R₂ = {2} Ω to ground and also receives {3} A from a current source. How much power does R₂ dissipate?", "Una fuente de {0} V con R₁ = {1} Ω alimenta un nodo que tiene R₂ = {2} Ω a tierra y recibe además {3} A de una fuente de corriente. ¿Cuánta potencia disipa R₂?", N(vs, 0), N(r1, 0), N(r2, 0), N(i)), vn * vn / r2, "W",
                YF("V_nó = {0} + {1} = {2} V (superposição); P = V²/R₂ = {2}²/{3} = {4} W. (Atenção: a potência não se soma por superposição!)", N(vs * r2 / (r1 + r2), 3), N(i * rp, 3), N(vn, 3), N(r2, 0), N(vn * vn / r2, 3)));
        }, r =>
        {
            double v1 = S(r, 10, 30, 1), v2 = S(r, 4, 12, 1), r1 = P(r, 10.0, 20, 40), r2 = P(r, 20.0, 40, 80), r3 = P(r, 10.0, 25, 50);
            double rp = r2 * r3 / (r2 + r3), c1 = v1 * rp / (r1 + rp);
            return Q(XF("Dois ramos alimentam um nó: V₁ = {0} V com R₁ = {1} Ω e V₂ = {2} V com R₃ = {3} Ω; o nó tem R₂ = {4} Ω para o terra. Pelo teorema da superposição, quanto V₁ sozinha (V₂ em curto) contribui para a tensão do nó?", "Two branches feed a node: V₁ = {0} V with R₁ = {1} Ω and V₂ = {2} V with R₃ = {3} Ω; the node has R₂ = {4} Ω to ground. By superposition, how much does V₁ alone (V₂ shorted) contribute to the node voltage?", "Dos ramas alimentan un nodo: V₁ = {0} V con R₁ = {1} Ω y V₂ = {2} V con R₃ = {3} Ω; el nodo tiene R₂ = {4} Ω a tierra. Por superposición, ¿cuánto aporta V₁ sola (V₂ en corto) a la tensión del nodo?", N(v1, 0), N(r1, 0), N(v2, 0), N(r3, 0), N(r2, 0)), c1, "V",
                YF("Com V₂ em curto, R₂∥R₃ = {0} Ω forma um divisor com R₁: V = V₁·{0}/(R₁ + {0}) = {1} V.", N(rp, 3), N(c1, 3)));
        })
    };
}

[Exercises("ele-12-rl")]
public sealed class Ele12Rl : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double L = S(r, 10, 1000, 10), R = S(r, 5, 220, 1), tau = L / R;
            return Q(XF("Um indutor de {0} mH está em série com um resistor de {1} Ω. Qual a constante de tempo τ?", "A {0} mH inductor is in series with a {1} Ω resistor. What is the time constant τ?", "Un inductor de {0} mH está en serie con una resistencia de {1} Ω. ¿Cuál es la constante de tiempo τ?", N(L, 0), N(R, 0)), tau, "ms", YF("τ = L / R = {0} mH / {1} Ω = {2} ms.", N(L, 0), N(R, 0), N(tau, 3)));
        }, r =>
        {
            double V = S(r, 3, 48, 1), R = S(r, 5, 100, 1);
            return Q(XF("Um circuito RL série é ligado a {0} V; R = {1} Ω. Qual a corrente final (em regime permanente)?", "A series RL circuit is connected to {0} V; R = {1} Ω. What is the final (steady-state) current?", "Un circuito RL serie se conecta a {0} V; R = {1} Ω. ¿Cuál es la corriente final (en régimen permanente)?", N(V, 0), N(R, 0)), V / R, "A",
                XF("Em regime o indutor é um fio (sem tensão): I = V / R = {0} / {1} = {2} A.", "At steady state the inductor is a wire (no voltage): I = V / R = {0} / {1} = {2} A.", "En régimen el inductor es un cable (sin tensión): I = V / R = {0} / {1} = {2} A.", N(V, 0), N(R, 0), N(V / R, 3)));
        }),
        Difficulty.Medium => F(r =>
        {
            double V = P(r, 5.0, 12, 24), R = P(r, 10.0, 22, 47), L = P(r, 100.0, 220, 470, 1000), tau = L / R, t = Math.Round(tau * S(r, 0.3, 3, 0.1), 1), i = V / R * (1 - Exp(-t / tau));
            return Q(XF("Um indutor de {0} mH em série com {1} Ω é ligado a {2} V. Qual a corrente {3} ms depois?", "A {0} mH inductor in series with {1} Ω is connected to {2} V. What is the current {3} ms later?", "Un inductor de {0} mH en serie con {1} Ω se conecta a {2} V. ¿Cuál es la corriente {3} ms después?", N(L, 0), N(R, 0), N(V, 0), N(t)), i, "A",
                YF("τ = L/R = {0} ms; i = (V/R)·(1 − e^(−t/τ)) = {1}·(1 − e^(−{2})) = {3} A.", N(tau, 3), N(V / R, 3), N(t / tau, 3), N(i, 4)));
        }, r =>
        {
            double V = P(r, 5.0, 12, 24), R = P(r, 10.0, 22, 47), L = P(r, 100.0, 220, 470, 1000), tau = L / R, f = P(r, 0.5, 0.632, 0.8, 0.9), t = -tau * Ln(1 - f), If = V / R;
            return Q(XF("Um indutor de {0} mH em série com {1} Ω é ligado a {2} V. Em quantos ms a corrente chega a {3} A?", "A {0} mH inductor in series with {1} Ω is connected to {2} V. In how many ms does the current reach {3} A?", "Un inductor de {0} mH en serie con {1} Ω se conecta a {2} V. ¿En cuántos ms llega la corriente a {3} A?", N(L, 0), N(R, 0), N(V, 0), N(If * f, 4)), t, "ms",
                YF("I_final = {0} A; τ = {1} ms; t = −τ·ln(1 − i/I_final) = −{1}·ln(1 − {2}) = {3} ms.", N(If, 3), N(tau, 3), N(f, 3), N(t, 3)));
        }),
        Difficulty.Hard => F(r =>
        {
            double L = P(r, 10.0, 50, 100, 500), I = P(r, 0.5, 1, 2, 5), e = 0.5 * L * 1e-3 * I * I * 1000;
            return Q(XF("Quanta energia está armazenada em um indutor de {0} mH percorrido por {1} A?", "How much energy is stored in a {0} mH inductor carrying {1} A?", "¿Cuánta energía está almacenada en un inductor de {0} mH recorrido por {1} A?", N(L, 0), N(I)), e, "mJ", YF("E = ½·L·I² = ½ · {0}·10⁻³ · {1}² = {2} mJ.", N(L, 0), N(I), N(e, 3)));
        }, r =>
        {
            double V = P(r, 12.0, 24, 48), R = P(r, 10.0, 22, 47), L = P(r, 100.0, 220, 470), tau = L / R, t = Math.Round(tau * S(r, 0.3, 2, 0.1), 1), vl = V * Exp(-t / tau);
            return Q(XF("Um indutor de {0} mH em série com {1} Ω é ligado a {2} V. Qual a tensão sobre o indutor {3} ms depois de ligar?", "A {0} mH inductor in series with {1} Ω is connected to {2} V. What is the voltage across the inductor {3} ms after switching on?", "Un inductor de {0} mH en serie con {1} Ω se conecta a {2} V. ¿Cuál es la tensión en el inductor {3} ms después de conectar?", N(L, 0), N(R, 0), N(V, 0), N(t)), vl, "V",
                YF("τ = {0} ms; vL = V·e^(−t/τ) = {1}·e^(−{2}) = {3} V (começa em V e cai a zero).", N(tau, 3), N(V, 0), N(t / tau, 3), N(vl, 3)));
        }),
        _ => F(r =>
        {
            double N1 = P(r, 100.0, 200, 500, 1000), A = P(r, 1.0, 2.0, 4.0, 10.0), len = P(r, 5.0, 10, 20), R = P(r, 1.0, 2, 5, 10);
            double L = 4e-7 * Pi * N1 * N1 * (A * 1e-4) / (len * 1e-2), tau = L / R * 1000;
            return Q(XF("Um solenoide de ar com {0} espiras, seção de {1} cm² e comprimento de {2} cm (L = μ₀·N²·A/ℓ, μ₀ = 4π·10⁻⁷) está em série com {3} Ω. Qual a constante de tempo, em ms?", "An air-core solenoid with {0} turns, {1} cm² cross-section and {2} cm length (L = μ₀·N²·A/ℓ, μ₀ = 4π·10⁻⁷) is in series with {3} Ω. What is the time constant, in ms?", "Un solenoide de aire con {0} espiras, sección de {1} cm² y longitud de {2} cm (L = μ₀·N²·A/ℓ, μ₀ = 4π·10⁻⁷) está en serie con {3} Ω. ¿Cuál es la constante de tiempo, en ms?", N(N1, 0), N(A, 0), N(len, 0), N(R, 0)), tau, "ms",
                YF("L = 4π·10⁻⁷ · {0}² · {1}·10⁻⁴ / {2}·10⁻² = {3} mH; τ = L/R = {4} ms.", N(N1, 0), N(A, 0), N(len, 0), N(L * 1000, 4), N(tau, 4)));
        }, r =>
        {
            double L = P(r, 100.0, 220, 470, 1000), R = P(r, 10.0, 22, 47, 100), tau = L / R, t = tau * Ln(2) / 2;
            return Q(XF("Uma bobina de {0} mH e {1} Ω, com corrente inicial I₀, é curto-circuitada (a corrente decai por um laço R–L). Em quantos ms a energia armazenada cai pela metade?", "A {0} mH, {1} Ω coil carrying an initial current I₀ is short-circuited (the current decays through an R–L loop). In how many ms does the stored energy drop to half?", "Una bobina de {0} mH y {1} Ω, con corriente inicial I₀, se cortocircuita (la corriente decae por un lazo R–L). ¿En cuántos ms la energía almacenada cae a la mitad?", N(L, 0), N(R, 0)), t, "ms",
                YF("i = I₀·e^(−t/τ) e E ∝ i² → E = E₀/2 quando e^(−2t/τ) = ½ → t = (τ/2)·ln 2 = ({0}/2) · 0,693 = {1} ms.", N(tau, 3), N(t, 3)));
        })
    };
}

[Exercises("ele-13-led-zener")]
public sealed class Ele13LedZener : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double Vs = P(r, 5.0, 9, 12, 24), Vf = P(r, 1.8, 2.0, 2.1, 3.0, 3.3), I = P(r, 10.0, 15, 20), R = (Vs - Vf) / I * 1000;
            return Q(XF("Um LED de {0} V (queda direta) deve operar com {1} mA em uma fonte de {2} V. Qual o resistor em série?", "A {0} V (forward drop) LED must run at {1} mA from a {2} V supply. What series resistor is needed?", "Un LED de {0} V (caída directa) debe operar con {1} mA en una fuente de {2} V. ¿Qué resistencia en serie se necesita?", N(Vf), N(I, 0), N(Vs, 0)), R, "Ω",
                YF("R = (Vs − Vf) / I = ({0} − {1}) / {2} mA = {3} Ω.", N(Vs, 0), N(Vf), N(I, 0), N(R)));
        }),
        Difficulty.Medium => F(r =>
        {
            double Vs = S(r, 5, 24, 1), Vf = P(r, 1.8, 2.0, 2.1), I = S(r, 5, 30, 1), R = (Vs - Vf) / I * 1000, pr = (Vs - Vf) * I;
            return Q(XF("Um LED de {0} V é alimentado a {1} mA por {2} V através de um resistor em série. Quantos mW esse resistor dissipa?", "A {0} V LED is run at {1} mA from {2} V through a series resistor. How many mW does that resistor dissipate?", "Un LED de {0} V se alimenta a {1} mA con {2} V a través de una resistencia en serie. ¿Cuántos mW disipa esa resistencia?", N(Vf), N(I, 0), N(Vs, 0)), pr, "mW",
                YF("VR = {0} − {1} = {2} V; P = VR·I = {2} · {3} mA = {4} mW (R = {5} Ω).", N(Vs, 0), N(Vf), N(Vs - Vf), N(I, 0), N(pr, 2), N(R)));
        }, r =>
        {
            double Vz = P(r, 3.3, 4.7, 5.1, 6.2, 9.1, 12.0), Iz = S(r, 2, 40, 1), Pz = Vz * Iz;
            return Q(XF("Um Zener de {0} V conduz {1} mA. Qual a potência que ele dissipa?", "A {0} V Zener conducts {1} mA. What power does it dissipate?", "Un Zener de {0} V conduce {1} mA. ¿Qué potencia disipa?", N(Vz), N(Iz, 0)), Pz, "mW", YF("P = Vz·Iz = {0} · {1} = {2} mW.", N(Vz), N(Iz, 0), N(Pz, 2)));
        }),
        Difficulty.Hard => F(r =>
        {
            double Vin = P(r, 12.0, 15, 18, 24), Vz = P(r, 5.1, 6.2, 9.1), IL = P(r, 10.0, 20, 50), Iz = P(r, 5.0, 10), Rs = (Vin - Vz) / (IL + Iz) * 1000;
            return Q(XF("Regulador Zener de {0} V com entrada de {1} V. A carga puxa {2} mA e o Zener precisa de pelo menos {3} mA. Qual o maior resistor em série Rs?", "{0} V Zener regulator with a {1} V input. The load draws {2} mA and the Zener needs at least {3} mA. What is the largest series resistor Rs?", "Regulador Zener de {0} V con entrada de {1} V. La carga consume {2} mA y el Zener necesita al menos {3} mA. ¿Cuál es la mayor resistencia en serie Rs?", N(Vz), N(Vin, 0), N(IL, 0), N(Iz, 0)), Rs, "Ω",
                YF("Rs = (Vin − Vz) / (IL + Iz) = ({0} − {1}) / {2} mA = {3} Ω.", N(Vin, 0), N(Vz), N(IL + Iz, 0), N(Rs)));
        }),
        _ => F(r =>
        {
            double Vin = P(r, 12.0, 15, 18, 24), Vz = P(r, 5.1, 6.2, 9.1), Rs = P(r, 100.0, 150, 220, 330), Iz = (Vin - Vz) / Rs * 1000, Pz = Vz * Iz;
            return Q(XF("Regulador Zener de {0} V, entrada {1} V, Rs = {2} Ω. Se a carga for desligada (circuito aberto), quantos mW o Zener dissipa?", "{0} V Zener regulator, {1} V input, Rs = {2} Ω. If the load is disconnected (open circuit), how many mW does the Zener dissipate?", "Regulador Zener de {0} V, entrada {1} V, Rs = {2} Ω. Si se desconecta la carga (circuito abierto), ¿cuántos mW disipa el Zener?", N(Vz), N(Vin, 0), N(Rs, 0)), Pz, "mW",
                YF("Sem carga toda a corrente passa pelo Zener: Iz = (Vin − Vz)/Rs = {0} mA; P = Vz·Iz = {1} mW.", N(Iz, 2), N(Pz, 2)));
        }, r =>
        {
            double Vz = P(r, 5.1, 6.2, 9.1), RL = P(r, 100.0, 220, 470, 1000), Rs = P(r, 100.0, 150, 220), Vmin = Vz + (Vz / RL) * Rs + 0.0;
            return Q(XF("Um Zener de {0} V alimenta uma carga de {1} Ω através de Rs = {2} Ω. Qual a tensão mínima de entrada para o Zener ainda regular (corrente nele ≈ 0)?", "A {0} V Zener feeds a {1} Ω load through Rs = {2} Ω. What is the minimum input voltage for the Zener to still regulate (its current ≈ 0)?", "Un Zener de {0} V alimenta una carga de {1} Ω a través de Rs = {2} Ω. ¿Cuál es la tensión mínima de entrada para que el Zener aún regule (su corriente ≈ 0)?", N(Vz), N(RL, 0), N(Rs, 0)), Vmin, "V",
                YF("IL = Vz/RL = {0} mA; no limite toda a corrente vai para a carga: Vin = Vz + IL·Rs = {1} + {0} mA · {2} = {3} V.", N(Vz / RL * 1000, 2), N(Vz), N(Rs, 0), N(Vmin, 3)));
        })
    };
}

[Exercises("ele-14-filtros")]
public sealed class Ele14Filters : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double R = S(r, 1, 50, 0.5), C = S(r, 10, 470, 1), fc = 1 / (2 * Pi * R * 1e3 * C * 1e-9);
            return Q(XF("Um filtro RC tem R = {0} kΩ e C = {1} nF. Qual a frequência de corte?", "An RC filter has R = {0} kΩ and C = {1} nF. What is the cutoff frequency?", "Un filtro RC tiene R = {0} kΩ y C = {1} nF. ¿Cuál es la frecuencia de corte?", N(R), N(C, 0)), fc, "Hz", YF("fc = 1 / (2π·R·C) = 1 / (2π · {0}·10³ · {1}·10⁻⁹) = {2} Hz.", N(R), N(C, 0), N(fc, 1)));
        }, r =>
        {
            double g = S(r, 0.05, 200, 0.05), db = 20 * Log(g);
            return Q(XF("Um amplificador tem ganho de tensão {0}. Qual o ganho em dB?", "An amplifier has a voltage gain of {0}. What is the gain in dB?", "Un amplificador tiene una ganancia de tensión de {0}. ¿Cuál es la ganancia en dB?", N(g)), db, "dB", YF("G = 20·log₁₀({0}) = {1} dB.", N(g), N(db, 2)));
        }),
        Difficulty.Medium => F(r =>
        {
            double fc = S(r, 100, 5000, 50), m = P(r, 0.1, 0.5, 2, 5, 10), f = fc * m, g = 1 / Sqrt(1 + m * m), db = 20 * Log(g);
            return Q(XF("Um filtro passa-baixas RC de primeira ordem tem fc = {0} Hz. Qual o ganho, em dB, em {1} Hz?", "A first-order RC low-pass filter has fc = {0} Hz. What is the gain, in dB, at {1} Hz?", "Un filtro pasa-bajas RC de primer orden tiene fc = {0} Hz. ¿Cuál es la ganancia, en dB, a {1} Hz?", N(fc, 0), N(f, 0)), db, "dB", YF("|H| = 1/√(1 + (f/fc)²) = 1/√(1 + {0}²) = {1}; G = 20·log₁₀|H| = {2} dB.", N(m, 3), N(g, 4), N(db, 3)));
        }, r =>
        {
            double fc = S(r, 100, 5000, 50), f = fc * P(r, 0.5, 2, 5, 10), vin = P(r, 1.0, 2, 5, 10), m = f / fc, vo = vin / Sqrt(1 + m * m);
            return Q(XF("Filtro passa-baixas RC com fc = {0} Hz recebe uma senoide de {1} V de pico em {2} Hz. Qual o pico na saída?", "RC low-pass filter with fc = {0} Hz receives a {1} V peak sine at {2} Hz. What is the output peak?", "Filtro pasa-bajas RC con fc = {0} Hz recibe una senoide de {1} V de pico a {2} Hz. ¿Cuál es el pico en la salida?", N(fc, 0), N(vin, 0), N(f, 0)), vo, "V", YF("|H| = 1/√(1 + ({0}/{1})²) = {2}; Vout = {3} · {2} = {4} V.", N(f, 0), N(fc, 0), N(1 / Sqrt(1 + m * m), 4), N(vin, 0), N(vo, 4)));
        }),
        Difficulty.Hard => F(r =>
        {
            double fc = S(r, 100, 5000, 50), dec = P(r, 1.0, 2, 3), f = fc * Math.Pow(10, dec), db = -20 * dec;
            return Q(XF("Filtro passa-baixas de primeira ordem com fc = {0} Hz. Em {1} Hz (bem acima do corte, a ordem de grandeza é de {2} década(s)), qual o ganho aproximado em dB, usando a assíntota de −20 dB/década?", "First-order low-pass filter with fc = {0} Hz. At {1} Hz (well above the cutoff, {2} decade(s) away), what is the approximate gain in dB, using the −20 dB/decade asymptote?", "Filtro pasa-bajas de primer orden con fc = {0} Hz. A {1} Hz (muy por encima del corte, a {2} década(s)), ¿cuál es la ganancia aproximada en dB, usando la asíntota de −20 dB/década?", N(fc, 0), N(f, 0), N(dec, 0)), db, "dB", YF("{0} década(s) acima de fc: G ≈ −20 · {0} = {1} dB.", N(dec, 0), N(db, 0)), 8);
        }, r =>
        {
            double fc = S(r, 100, 5000, 50), f = fc * P(r, 0.1, 0.2, 0.5, 2, 5), m = f / fc, g = m / Sqrt(1 + m * m), db = 20 * Log(g);
            return Q(XF("Filtro passa-altas RC de primeira ordem com fc = {0} Hz. Qual o ganho, em dB, em {1} Hz?", "First-order RC high-pass filter with fc = {0} Hz. What is the gain, in dB, at {1} Hz?", "Filtro pasa-altas RC de primer orden con fc = {0} Hz. ¿Cuál es la ganancia, en dB, a {1} Hz?", N(fc, 0), N(f, 0)), db, "dB", YF("|H| = (f/fc)/√(1 + (f/fc)²) = {0}/√(1 + {0}²) = {1}; G = {2} dB.", N(m, 3), N(g, 4), N(db, 3)), 3);
        }),
        _ => F(r =>
        {
            double fc = S(r, 100, 5000, 50), a = S(r, 6, 36, 1), f = fc * Sqrt(Math.Pow(10, a / 10) - 1);
            return Q(XF("Um filtro passa-baixas RC de primeira ordem tem fc = {0} Hz. Em que frequência o sinal fica atenuado em {1} dB?", "A first-order RC low-pass filter has fc = {0} Hz. At what frequency is the signal attenuated by {1} dB?", "Un filtro pasa-bajas RC de primer orden tiene fc = {0} Hz. ¿A qué frecuencia la señal queda atenuada {1} dB?", N(fc, 0), N(a, 0)), f, "Hz", YF("1 + (f/fc)² = 10^({0}/10) = {1} → f = fc·√({2}) = {3} Hz.", N(a, 0), N(Math.Pow(10, a / 10), 3), N(Math.Pow(10, a / 10) - 1, 3), N(f, 1)));
        }, r =>
        {
            double C = S(r, 10, 100, 1), fc = S(r, 200, 5000, 50), R = 1 / (2 * Pi * fc * C * 1e-9) / 1e3;
            return Q(XF("Que resistor, em kΩ, forma com um capacitor de {0} nF um filtro RC com frequência de corte de {1} Hz?", "What resistor, in kΩ, forms an RC filter with a cutoff frequency of {1} Hz together with a {0} nF capacitor?", "¿Qué resistencia, en kΩ, forma con un condensador de {0} nF un filtro RC con frecuencia de corte de {1} Hz?", N(C, 0), N(fc, 0)), R, "kΩ", YF("R = 1 / (2π·fc·C) = 1 / (2π · {0} · {1}·10⁻⁹) = {2} kΩ.", N(fc, 0), N(C, 0), N(R, 3)));
        })
    };
}

[Exercises("ele-15-ressonancia-paralela")]
public sealed class Ele15ParallelResonance : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double L = S(r, 1, 100, 1), C = S(r, 0.01, 10, 0.01), f0 = 1000 / (2 * Pi * Sqrt(L * C * 1e-3));
            return Q(XF("Um circuito LC paralelo tem L = {0} mH e C = {1} µF. Qual a frequência de ressonância?", "A parallel LC circuit has L = {0} mH and C = {1} µF. What is the resonant frequency?", "Un circuito LC paralelo tiene L = {0} mH y C = {1} µF. ¿Cuál es la frecuencia de resonancia?", N(L, 0), N(C, 2)), f0, "Hz", YF("f₀ = 1/(2π√(LC)) = {0} Hz.", N(f0, 1)));
        }),
        Difficulty.Medium => F(r =>
        {
            double f0 = S(r, 400, 100000, 50), Q0 = S(r, 5, 100, 1), bw = f0 / Q0;
            return Q(XF("Um circuito ressonante tem f₀ = {0} Hz e fator de qualidade Q = {1}. Qual a largura de banda (−3 dB)?", "A resonant circuit has f₀ = {0} Hz and quality factor Q = {1}. What is the (−3 dB) bandwidth?", "Un circuito resonante tiene f₀ = {0} Hz y factor de calidad Q = {1}. ¿Cuál es el ancho de banda (−3 dB)?", N(f0, 0), N(Q0, 0)), bw, "Hz", YF("BW = f₀ / Q = {0} / {1} = {2} Hz.", N(f0, 0), N(Q0, 0), N(bw, 2)));
        }, r =>
        {
            double f0 = S(r, 400, 100000, 50), bw = S(r, 5, 500, 5), q = f0 / bw;
            return Q(XF("Um circuito ressonante com f₀ = {0} Hz tem largura de banda de {1} Hz. Qual o seu fator de qualidade?", "A resonant circuit with f₀ = {0} Hz has a bandwidth of {1} Hz. What is its quality factor?", "Un circuito resonante con f₀ = {0} Hz tiene un ancho de banda de {1} Hz. ¿Cuál es su factor de calidad?", N(f0, 0), N(bw, 0)), q, "", YF("Q = f₀ / BW = {0} / {1} = {2}.", N(f0, 0), N(bw, 0), N(q, 2)));
        }),
        Difficulty.Hard => F(r =>
        {
            double L = P(r, 1.0, 10, 22, 47), C = P(r, 0.1, 0.47, 1, 10), R = P(r, 1.0, 5, 10, 47), w0 = 1 / Sqrt(L * 1e-3 * C * 1e-6), q = R * 1000 / (w0 * L * 1e-3);
            return Q(XF("Um circuito paralelo tem L = {0} mH, C = {1} µF e um resistor de {2} kΩ em paralelo. Qual o fator de qualidade Q?", "A parallel circuit has L = {0} mH, C = {1} µF and a {2} kΩ resistor in parallel. What is the quality factor Q?", "Un circuito paralelo tiene L = {0} mH, C = {1} µF y una resistencia de {2} kΩ en paralelo. ¿Cuál es el factor de calidad Q?", N(L, 0), N(C, 2), N(R, 0)), q, "", YF("ω₀ = 1/√(LC) = {0} rad/s; XL = ω₀L = {1} Ω; Q = R / XL = {2} / {1} = {3}.", N(w0, 0), N(w0 * L * 1e-3, 3), N(R * 1000, 0), N(q, 3)));
        }),
        _ => F(r =>
        {
            double L = P(r, 1.0, 10, 22, 47), C = P(r, 0.1, 0.47, 1, 10), R = P(r, 1.0, 5, 10, 47), w0 = 1 / Sqrt(L * 1e-3 * C * 1e-6), f0 = w0 / (2 * Pi), q = R * 1000 / (w0 * L * 1e-3), bw = f0 / q;
            return Q(XF("Um circuito paralelo tem L = {0} mH, C = {1} µF e R = {2} kΩ em paralelo. Qual a sua largura de banda, em Hz?", "A parallel circuit has L = {0} mH, C = {1} µF and R = {2} kΩ in parallel. What is its bandwidth, in Hz?", "Un circuito paralelo tiene L = {0} mH, C = {1} µF y R = {2} kΩ en paralelo. ¿Cuál es su ancho de banda, en Hz?", N(L, 0), N(C, 2), N(R, 0)), bw, "Hz", YF("f₀ = {0} Hz; Q = R/(ω₀L) = {1}; BW = f₀/Q = {2} Hz (= 1/(2π·R·C)).", N(f0, 1), N(q, 3), N(bw, 2)));
        }, r =>
        {
            double f0 = P(r, 1000.0, 5000, 10000), bw = P(r, 50.0, 100, 200), L = P(r, 1.0, 5, 10), w0 = 2 * Pi * f0, C = 1 / (w0 * w0 * L * 1e-3) * 1e6, q = f0 / bw, R = q * w0 * L * 1e-3 / 1000;
            return Q(XF("Projete um circuito paralelo com f₀ = {0} Hz e largura de banda de {1} Hz usando L = {2} mH. Que resistência em paralelo (em kΩ) dá essa seletividade?", "Design a parallel circuit with f₀ = {0} Hz and a bandwidth of {1} Hz using L = {2} mH. What parallel resistance (in kΩ) gives that selectivity?", "Diseñe un circuito paralelo con f₀ = {0} Hz y un ancho de banda de {1} Hz usando L = {2} mH. ¿Qué resistencia en paralelo (en kΩ) da esa selectividad?", N(f0, 0), N(bw, 0), N(L, 0)), R, "kΩ", YF("Q = f₀/BW = {0}; C = 1/(ω₀²L) = {1} µF (não é pedido, mas fixa f₀); R = Q·ω₀·L = {0} · {2} Ω = {3} kΩ.", N(q, 2), N(C, 4), N(w0 * L * 1e-3, 3), N(R, 3)));
        })
    };
}
