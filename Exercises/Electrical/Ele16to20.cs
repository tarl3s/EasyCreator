namespace EasyCreator.Exercises;

[Exercises("ele-16-transformador")]
public sealed class Ele16Transformer : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double vp = P(r, 127.0, 220, 380), np = P(r, 500.0, 1000, 2000), ns = P(r, 50.0, 100, 150, 250, 400), vs = vp * ns / np;
            return Q(XF("Um transformador ideal tem {0} espiras no primário e {1} no secundário, ligado a {2} V. Qual a tensão no secundário?", "An ideal transformer has {0} primary turns and {1} secondary turns, connected to {2} V. What is the secondary voltage?", "Un transformador ideal tiene {0} espiras en el primario y {1} en el secundario, conectado a {2} V. ¿Cuál es la tensión en el secundario?", N(np, 0), N(ns, 0), N(vp, 0)), vs, "V", YF("Vs = Vp·Ns/Np = {0} · {1}/{2} = {3} V.", N(vp, 0), N(ns, 0), N(np, 0), N(vs, 2)));
        }, r =>
        {
            double vp = P(r, 127.0, 220), vs = P(r, 6.0, 9, 12, 24, 48), np = P(r, 500.0, 1000, 1100), ns = np * vs / vp;
            return Q(XF("Quantas espiras deve ter o secundário de um transformador ideal de {0} V para {1} V, se o primário tem {2} espiras?", "How many turns must the secondary of an ideal {0} V to {1} V transformer have, if the primary has {2} turns?", "¿Cuántas espiras debe tener el secundario de un transformador ideal de {0} V a {1} V, si el primario tiene {2} espiras?", N(vp, 0), N(vs, 0), N(np, 0)), ns, "", YF("Ns = Np·Vs/Vp = {0} · {1}/{2} = {3}.", N(np, 0), N(vs, 0), N(vp, 0), N(ns, 1)));
        }),
        Difficulty.Medium => F(r =>
        {
            double vp = P(r, 127.0, 220), vs = P(r, 6.0, 12, 24, 48), is_ = P(r, 1.0, 2, 5, 10), ip = is_ * vs / vp;
            return Q(XF("Um transformador ideal de {0} V para {1} V alimenta uma carga de {2} A. Qual a corrente no primário?", "An ideal {0} V to {1} V transformer feeds a {2} A load. What is the primary current?", "Un transformador ideal de {0} V a {1} V alimenta una carga de {2} A. ¿Cuál es la corriente en el primario?", N(vp, 0), N(vs, 0), N(is_, 0)), ip, "A", YF("Vp·Ip = Vs·Is → Ip = {0}·{1}/{2} = {3} A.", N(vs, 0), N(is_, 0), N(vp, 0), N(ip, 3)));
        }, r =>
        {
            double vp = P(r, 127.0, 220), vs = P(r, 12.0, 24, 48), p = P(r, 50.0, 100, 250, 500), eta = P(r, 0.85, 0.9, 0.95), pin = p / eta;
            return Q(XF("Um transformador de {0} V para {1} V, com rendimento de {2}%, alimenta uma carga de {3} W. Qual a corrente no primário?", "A {0} V to {1} V transformer with {2}% efficiency feeds a {3} W load. What is the primary current?", "Un transformador de {0} V a {1} V, con rendimiento de {2}%, alimenta una carga de {3} W. ¿Cuál es la corriente en el primario?", N(vp, 0), N(vs, 0), N(eta * 100, 0), N(p, 0)), pin / vp, "A", YF("Pentrada = {0}/{1} = {2} W; Ip = Pentrada/Vp = {2}/{3} = {4} A.", N(p, 0), N(eta, 2), N(pin, 2), N(vp, 0), N(pin / vp, 4)));
        }),
        Difficulty.Hard => F(r =>
        {
            double a = S(r, 1.5, 20, 0.5), zl = S(r, 2, 60, 1), zp = a * a * zl;
            return Q(XF("Um transformador ideal com relação de espiras {0}:1 tem uma carga de {1} Ω no secundário. Que impedância o primário enxerga?", "An ideal transformer with a {0}:1 turns ratio has a {1} Ω load on the secondary. What impedance does the primary see?", "Un transformador ideal con relación de espiras {0}:1 tiene una carga de {1} Ω en el secundario. ¿Qué impedancia ve el primario?", N(a, 0), N(zl, 0)), zp, "Ω", YF("Zp = (Np/Ns)²·ZL = {0}² · {1} = {2} Ω.", N(a, 0), N(zl, 0), N(zp, 0)));
        }, r =>
        {
            double rs = P(r, 4.0, 8, 16), rp = P(r, 400.0, 800, 1600, 3200), a = Sqrt(rp / rs);
            return Q(XF("Para casar uma carga de {0} Ω com uma fonte que pede {1} Ω, qual deve ser a relação de espiras Np/Ns do transformador?", "To match a {0} Ω load to a source that wants {1} Ω, what must the transformer's turns ratio Np/Ns be?", "Para adaptar una carga de {0} Ω a una fuente que pide {1} Ω, ¿cuál debe ser la relación de espiras Np/Ns del transformador?", N(rs, 0), N(rp, 0)), a, "", YF("Zp = (Np/Ns)²·ZL → Np/Ns = √({0}/{1}) = {2}.", N(rp, 0), N(rs, 0), N(a, 3)));
        }),
        _ => F(r =>
        {
            double f = P(r, 50.0, 60), np = S(r, 300, 1200, 10), vp = P(r, 127.0, 220), phi = vp / (4.44 * f * np) * 1000;
            return Q(XF("Um primário de {0} espiras ligado a {1} V / {2} Hz. Qual o fluxo magnético máximo no núcleo, em mWb? (V = 4,44·f·N·Φmáx)", "A {0}-turn primary connected to {1} V / {2} Hz. What is the peak magnetic flux in the core, in mWb? (V = 4.44·f·N·Φmax)", "Un primario de {0} espiras conectado a {1} V / {2} Hz. ¿Cuál es el flujo magnético máximo en el núcleo, en mWb? (V = 4,44·f·N·Φmáx)", N(np, 0), N(vp, 0), N(f, 0)), phi, "mWb", YF("Φmáx = V / (4,44·f·N) = {0} / (4,44 · {1} · {2}) = {3} mWb.", N(vp, 0), N(f, 0), N(np, 0), N(phi, 3)));
        }, r =>
        {
            double vp = 220, vs = S(r, 6, 60, 1), RL = S(r, 1, 20, 0.5), a = vp / vs, ip = vs / RL * vs / vp;
            return Q(XF("Um transformador ideal de 220 V para {0} V alimenta um resistor de {1} Ω. Qual a corrente no primário?", "An ideal 220 V to {0} V transformer feeds a {1} Ω resistor. What is the primary current?", "Un transformador ideal de 220 V a {0} V alimenta una resistencia de {1} Ω. ¿Cuál es la corriente en el primario?", N(vs, 0), N(RL, 0)), ip, "A", YF("Is = {0}/{1} = {2} A; Ip = Is·Vs/Vp = {2} · {0}/220 = {3} A (a = {4}; equivale a Zp = a²·R = {5} Ω).", N(vs, 0), N(RL, 0), N(vs / RL, 3), N(ip, 4), N(a, 3), N(a * a * RL, 1)));
        })
    };
}

[Exercises("ele-17-ampop")]
public sealed class Ele17OpAmp : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double rin = S(r, 1, 10, 0.1), rf = S(r, 10, 100, 1), g = -rf / rin;
            return Q(XF("Amplificador inversor com Rin = {0} kΩ e Rf = {1} kΩ. Qual o ganho de tensão?", "Inverting amplifier with Rin = {0} kΩ and Rf = {1} kΩ. What is the voltage gain?", "Amplificador inversor con Rin = {0} kΩ y Rf = {1} kΩ. ¿Cuál es la ganancia de tensión?", N(rin), N(rf)), g, "", YF("G = −Rf / Rin = −{0} / {1} = {2}.", N(rf), N(rin), N(g, 3)));
        }, r =>
        {
            double rg = S(r, 1, 10, 0.1), rf = S(r, 10, 100, 1), g = 1 + rf / rg;
            return Q(XF("Amplificador não inversor com Rg = {0} kΩ (ao terra) e Rf = {1} kΩ. Qual o ganho de tensão?", "Non-inverting amplifier with Rg = {0} kΩ (to ground) and Rf = {1} kΩ. What is the voltage gain?", "Amplificador no inversor con Rg = {0} kΩ (a tierra) y Rf = {1} kΩ. ¿Cuál es la ganancia de tensión?", N(rg), N(rf)), g, "", YF("G = 1 + Rf / Rg = 1 + {0} / {1} = {2}.", N(rf), N(rg), N(g, 3)));
        }),
        Difficulty.Medium => F(r =>
        {
            double rin = P(r, 1.0, 2.2, 5.0, 10), rf = P(r, 10.0, 20, 47, 100), vin = S(r, 0.1, 1, 0.05), vo = -rf / rin * vin;
            return Q(XF("Amplificador inversor com Rin = {0} kΩ e Rf = {1} kΩ recebe {2} V. Qual a tensão de saída?", "Inverting amplifier with Rin = {0} kΩ and Rf = {1} kΩ receives {2} V. What is the output voltage?", "Amplificador inversor con Rin = {0} kΩ y Rf = {1} kΩ recibe {2} V. ¿Cuál es la tensión de salida?", N(rin), N(rf), N(vin)), vo, "V", YF("Vo = −(Rf/Rin)·Vin = −({0}/{1}) · {2} = {3} V.", N(rf), N(rin), N(vin), N(vo, 3)));
        }, r =>
        {
            double rg = P(r, 1.0, 2.2, 5.0, 10), rf = P(r, 10.0, 20, 47, 100), vin = S(r, 0.1, 1, 0.05), vo = (1 + rf / rg) * vin;
            return Q(XF("Amplificador não inversor com Rg = {0} kΩ e Rf = {1} kΩ recebe {2} V. Qual a tensão de saída?", "Non-inverting amplifier with Rg = {0} kΩ and Rf = {1} kΩ receives {2} V. What is the output voltage?", "Amplificador no inversor con Rg = {0} kΩ y Rf = {1} kΩ recibe {2} V. ¿Cuál es la tensión de salida?", N(rg), N(rf), N(vin)), vo, "V", YF("Vo = (1 + Rf/Rg)·Vin = (1 + {0}/{1}) · {2} = {3} V.", N(rf), N(rg), N(vin), N(vo, 3)));
        }),
        Difficulty.Hard => F(r =>
        {
            double rf = P(r, 10.0, 22, 47, 100), r1 = P(r, 10.0, 22, 47), r2 = P(r, 10.0, 33, 47), v1 = S(r, 0.1, 1, 0.1), v2 = S(r, 0.1, 1, 0.1), vo = -(rf / r1 * v1 + rf / r2 * v2);
            return Q(XF("Somador inversor com Rf = {0} kΩ e duas entradas: V₁ = {1} V através de {2} kΩ e V₂ = {3} V através de {4} kΩ. Qual a saída?", "Inverting summer with Rf = {0} kΩ and two inputs: V₁ = {1} V through {2} kΩ and V₂ = {3} V through {4} kΩ. What is the output?", "Sumador inversor con Rf = {0} kΩ y dos entradas: V₁ = {1} V a través de {2} kΩ y V₂ = {3} V a través de {4} kΩ. ¿Cuál es la salida?", N(rf), N(v1), N(r1), N(v2), N(r2)), vo, "V", YF("Vo = −Rf·(V₁/R₁ + V₂/R₂) = −{0}·({1}/{2} + {3}/{4}) = {5} V.", N(rf), N(v1), N(r1), N(v2), N(r2), N(vo, 3)));
        }, r =>
        {
            double R = P(r, 10.0, 22, 47), g = P(r, 2.0, 5, 10, 20), rf = g * R, vin = S(r, 0.1, 0.5, 0.05);
            return Q(XF("Quer-se um amplificador inversor de ganho −{0} e Rin = {1} kΩ. Que valor de Rf, em kΩ?", "An inverting amplifier of gain −{0} with Rin = {1} kΩ is wanted. What value of Rf, in kΩ?", "Se quiere un amplificador inversor de ganancia −{0} con Rin = {1} kΩ. ¿Qué valor de Rf, en kΩ?", N(g, 0), N(R)), rf, "kΩ", YF("Rf = |G|·Rin = {0} · {1} = {2} kΩ.", N(g, 0), N(R), N(rf)));
        }),
        _ => F(r =>
        {
            double rin = P(r, 1.0, 2.0, 5.0), rf = P(r, 10.0, 20, 50), vin = S(r, 0.5, 2, 0.1), vcc = P(r, 9.0, 12, 15), vo = -rf / rin * vin, vsat = Math.Max(-vcc, Math.Min(vcc, vo));
            return Q(XF("Amplificador inversor com ganho Rf/Rin = {0}/{1} alimentado com ±{2} V (considere a saturação em ±{2} V). Entrada de {3} V contínuos. Qual a tensão de saída real?", "Inverting amplifier with gain Rf/Rin = {0}/{1} powered by ±{2} V (assume saturation at ±{2} V). DC input of {3} V. What is the actual output voltage?", "Amplificador inversor con ganancia Rf/Rin = {0}/{1} alimentado con ±{2} V (considere la saturación en ±{2} V). Entrada de {3} V continuos. ¿Cuál es la tensión de salida real?", N(rf), N(rin), N(vcc, 0), N(vin)), vsat, "V", YF("Sem saturar: Vo = −{0}/{1} · {2} = {3} V. Como o amp-op não passa de ±{4} V, a saída real é {5} V.", N(rf), N(rin), N(vin), N(vo, 2), N(vcc, 0), N(vsat, 2)));
        }, r =>
        {
            double r1 = P(r, 10.0, 20), r2 = P(r, 10.0, 20), rf = P(r, 20.0, 40, 100), v1 = S(r, 0.5, 2, 0.1), v2 = S(r, 0.1, 1.5, 0.1);
            double vo = (rf / r1) * (v2 - v1);
            r2 = r1;
            vo = (rf / r1) * (v2 - v1);
            return Q(XF("Amplificador diferencial com R₁ = R₂ = {0} kΩ nas entradas e Rf = R₃ = {1} kΩ. As entradas são V₁ = {2} V (inversora) e V₂ = {3} V (não inversora). Qual a saída?", "Difference amplifier with R₁ = R₂ = {0} kΩ at the inputs and Rf = R₃ = {1} kΩ. The inputs are V₁ = {2} V (inverting) and V₂ = {3} V (non-inverting). What is the output?", "Amplificador diferencial con R₁ = R₂ = {0} kΩ en las entradas y Rf = R₃ = {1} kΩ. Las entradas son V₁ = {2} V (inversora) y V₂ = {3} V (no inversora). ¿Cuál es la salida?", N(r1), N(rf), N(v1), N(v2)), vo, "V", YF("Vo = (Rf/R₁)·(V₂ − V₁) = ({0}/{1}) · ({2} − {3}) = {4} V.", N(rf), N(r1), N(v2), N(v1), N(vo, 3)));
        })
    };
}

[Exercises("ele-18-motores")]
public sealed class Ele18Motors : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double f = P(r, 50.0, 60), p = P(r, 2.0, 4, 6, 8), ns = 120 * f / p;
            return Q(XF("Qual a velocidade síncrona de um motor de indução de {0} polos em {1} Hz?", "What is the synchronous speed of a {0}-pole induction motor at {1} Hz?", "¿Cuál es la velocidad síncrona de un motor de inducción de {0} polos a {1} Hz?", N(p, 0), N(f, 0)), ns, "rpm", YF("ns = 120·f / p = 120 · {0} / {1} = {2} rpm.", N(f, 0), N(p, 0), N(ns, 0)));
        }, r =>
        {
            double ns = S(r, 900, 3600, 50), s = S(r, 0.01, 0.08, 0.005), n = ns * (1 - s);
            return Q(XF("Um motor de indução com velocidade síncrona de {0} rpm trabalha com escorregamento de {1}%. Qual a velocidade do eixo?", "An induction motor with a synchronous speed of {0} rpm runs with {1}% slip. What is the shaft speed?", "Un motor de inducción con velocidad síncrona de {0} rpm trabaja con deslizamiento de {1}%. ¿Cuál es la velocidad del eje?", N(ns, 0), N(s * 100, 0)), n, "rpm", YF("n = ns·(1 − s) = {0}·(1 − {1}) = {2} rpm.", N(ns, 0), N(s, 2), N(n, 1)));
        }),
        Difficulty.Medium => F(r =>
        {
            double ns = S(r, 900, 3600, 50), n = ns - S(r, 10, 100, 1), s = (ns - n) / ns * 100;
            return Q(XF("Um motor de indução de velocidade síncrona {0} rpm gira a {1} rpm. Qual o escorregamento, em %?", "An induction motor with a synchronous speed of {0} rpm turns at {1} rpm. What is the slip, in %?", "Un motor de inducción de velocidad síncrona {0} rpm gira a {1} rpm. ¿Cuál es el deslizamiento, en %?", N(ns, 0), N(n, 0)), s, "%", YF("s = (ns − n)/ns = ({0} − {1}) / {0} = {2}%.", N(ns, 0), N(n, 0), N(s, 3)));
        }, r =>
        {
            double p = P(r, 0.75, 1.5, 3, 5.5, 7.5, 11, 15), n = S(r, 500, 3600, 10), t = p * 1000 / (2 * Pi * n / 60);
            return Q(XF("Um motor de {0} kW gira a {1} rpm. Qual o torque no eixo?", "A {0} kW motor turns at {1} rpm. What is the shaft torque?", "Un motor de {0} kW gira a {1} rpm. ¿Cuál es el par en el eje?", N(p, 2), N(n, 0)), t, "N·m", YF("ω = 2π·n/60 = {0} rad/s; T = P/ω = {1} / {0} = {2} N·m.", N(2 * Pi * n / 60, 2), N(p * 1000, 0), N(t, 2)));
        }),
        Difficulty.Hard => F(r =>
        {
            double pm = P(r, 3.0, 5.5, 7.5, 11, 15, 22), eta = P(r, 0.85, 0.88, 0.9, 0.93), pe = pm / eta, loss = pe - pm;
            return Q(XF("Um motor entrega {0} kW no eixo com rendimento de {1}%. Quanta potência elétrica ele absorve da rede, em kW?", "A motor delivers {0} kW at the shaft with {1}% efficiency. How much electrical power does it draw from the supply, in kW?", "Un motor entrega {0} kW en el eje con rendimiento de {1}%. ¿Cuánta potencia eléctrica absorbe de la red, en kW?", N(pm), N(eta * 100, 0)), pe, "kW", YF("Pent = Peixo / η = {0} / {1} = {2} kW (perdas = {3} kW).", N(pm), N(eta, 2), N(pe, 3), N(loss, 3)));
        }, r =>
        {
            double V = P(r, 12.0, 24, 48, 110, 220), Ra = P(r, 0.5, 1, 2, 4), Ia = P(r, 2.0, 5, 10, 20), eb = V - Ia * Ra;
            return Q(XF("Um motor CC de {0} V tem resistência de armadura de {1} Ω e consome {2} A em regime. Qual a força contraeletromotriz?", "A {0} V DC motor has an armature resistance of {1} Ω and draws {2} A at steady state. What is the back-EMF?", "Un motor CC de {0} V tiene resistencia de armadura de {1} Ω y consume {2} A en régimen. ¿Cuál es la fuerza contraelectromotriz?", N(V, 0), N(Ra), N(Ia, 0)), eb, "V", YF("Eb = V − Ia·Ra = {0} − {1}·{2} = {3} V.", N(V, 0), N(Ia, 0), N(Ra), N(eb, 2)));
        }),
        _ => F(r =>
        {
            double V = P(r, 12.0, 24, 48, 110, 220), Ra = P(r, 0.5, 1, 2, 4), ist = V / Ra;
            return Q(XF("Um motor CC de {0} V com Ra = {1} Ω é ligado diretamente à fonte, parado (sem força contraeletromotriz). Qual a corrente de partida?", "A {0} V DC motor with Ra = {1} Ω is connected directly to the supply at standstill (no back-EMF). What is the starting current?", "Un motor CC de {0} V con Ra = {1} Ω se conecta directamente a la fuente, parado (sin fuerza contraelectromotriz). ¿Cuál es la corriente de arranque?", N(V, 0), N(Ra)), ist, "A", YF("Parado, Eb = 0: Ipart = V / Ra = {0} / {1} = {2} A.", N(V, 0), N(Ra), N(ist, 1)));
        }, r =>
        {
            double V = P(r, 110.0, 220), Ra = P(r, 0.5, 1, 2), n1 = P(r, 1000.0, 1500, 1750), Ia = P(r, 5.0, 10, 20), eb1 = V - Ia * Ra, f = P(r, 1.2, 1.5), n2 = n1 * f, eb2 = eb1 * f;
            return Q(XF("Um motor CC de excitação constante tem Ra = {0} Ω, {1} V e gira a {2} rpm com {3} A (fluxo constante). Se a carga cai e ele passa a {4} rpm, qual a força contraeletromotriz nessa nova velocidade?", "A constant-excitation DC motor has Ra = {0} Ω, {1} V and turns at {2} rpm with {3} A (constant flux). If the load drops and it speeds up to {4} rpm, what is the back-EMF at the new speed?", "Un motor CC de excitación constante tiene Ra = {0} Ω, {1} V y gira a {2} rpm con {3} A (flujo constante). Si la carga baja y pasa a {4} rpm, ¿cuál es la fuerza contraelectromotriz a la nueva velocidad?", N(Ra), N(V, 0), N(n1, 0), N(Ia, 0), N(n2, 0)), eb2, "V", YF("Eb₁ = V − Ia·Ra = {0} V; com fluxo constante Eb ∝ n: Eb₂ = Eb₁·n₂/n₁ = {0} · {1}/{2} = {3} V.", N(eb1, 2), N(n2, 0), N(n1, 0), N(eb2, 2)));
        })
    };
}

[Exercises("ele-19-condutores")]
public sealed class Ele19Conductors : PoolGen
{
    const double Rho = 1.0 / 56;

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double s = S(r, 1.5, 35, 0.5), L = S(r, 5, 150, 1), R = Rho * L / s;
            return Q(XF("Qual a resistência de um condutor de cobre (ρ = 1/56 Ω·mm²/m) com {0} m e seção de {1} mm²?", "What is the resistance of a copper conductor (ρ = 1/56 Ω·mm²/m) that is {0} m long with a {1} mm² cross-section?", "¿Cuál es la resistencia de un conductor de cobre (ρ = 1/56 Ω·mm²/m) de {0} m y sección de {1} mm²?", N(L, 0), N(s)), R, "Ω", YF("R = ρ·L/S = (1/56) · {0} / {1} = {2} Ω.", N(L, 0), N(s), N(R, 4)));
        }),
        Difficulty.Medium => F(r =>
        {
            double s = P(r, 1.5, 2.5, 4.0, 6.0, 10.0), L = P(r, 10.0, 20, 30, 50, 100), I = P(r, 10.0, 15, 20, 25, 32), V = P(r, 127.0, 220), dv = 2 * Rho * L * I / s;
            return Q(XF("Circuito monofásico de {0} V alimenta uma carga de {1} A por um par de condutores de cobre de {2} mm², com {3} m de distância até a carga. Qual a queda de tensão total? (fio de ida e de volta)", "A {0} V single-phase circuit feeds a {1} A load through a pair of {2} mm² copper conductors, {3} m from the source to the load. What is the total voltage drop? (outgoing and return wire)", "Un circuito monofásico de {0} V alimenta una carga de {1} A por un par de conductores de cobre de {2} mm², a {3} m de distancia de la carga. ¿Cuál es la caída de tensión total? (cable de ida y de vuelta)", N(V, 0), N(I, 0), N(s), N(L, 0)), dv, "V", YF("ΔV = 2·ρ·L·I / S = 2 · (1/56) · {0} · {1} / {2} = {3} V ({4}% de {5} V).", N(L, 0), N(I, 0), N(s), N(dv, 3), N(dv / V * 100, 2), N(V, 0)));
        }, r =>
        {
            double s = P(r, 1.5, 2.5, 4.0, 6.0, 10.0), L = P(r, 20.0, 30, 50, 80), I = P(r, 10.0, 15, 20, 25), pl = 2 * Rho * L / s * I * I;
            return Q(XF("Um par de condutores de cobre de {0} mm² e {1} m (ida e volta) leva {2} A. Quantos watts são perdidos como calor nos fios?", "A pair of {0} mm² copper conductors, {1} m long (out and back), carries {2} A. How many watts are lost as heat in the wires?", "Un par de conductores de cobre de {0} mm² y {1} m (ida y vuelta) lleva {2} A. ¿Cuántos vatios se pierden como calor en los cables?", N(s), N(L, 0), N(I, 0)), pl, "W", YF("R = 2·ρ·L/S = {0} Ω; P = R·I² = {0} · {1}² = {2} W.", N(2 * Rho * L / s, 4), N(I, 0), N(pl, 2)));
        }),
        Difficulty.Hard => F(r =>
        {
            double V = P(r, 127.0, 220), I = P(r, 15.0, 20, 25, 32), L = P(r, 20.0, 30, 50, 80), pct = P(r, 2.0, 3, 4), dvmax = V * pct / 100, s = 2 * Rho * L * I / dvmax;
            return Q(XF("Circuito monofásico de {0} V, {1} A, {2} m até a carga (ida e volta de cobre). Qual a seção mínima, em mm², para a queda de tensão ficar em {3}%?", "A {0} V single-phase circuit, {1} A, {2} m to the load (copper out and back). What is the minimum cross-section, in mm², for a voltage drop of {3}%?", "Circuito monofásico de {0} V, {1} A, {2} m hasta la carga (cobre de ida y vuelta). ¿Cuál es la sección mínima, en mm², para una caída de tensión de {3}%?", N(V, 0), N(I, 0), N(L, 0), N(pct, 0)), s, "mm²", YF("ΔVmáx = {0}% de {1} = {2} V; S = 2·ρ·L·I / ΔV = 2 · (1/56) · {3} · {4} / {2} = {5} mm².", N(pct, 0), N(V, 0), N(dvmax, 2), N(L, 0), N(I, 0), N(s, 3)));
        }, r =>
        {
            double V = P(r, 220.0, 380, 440), I = P(r, 20.0, 40, 63, 100), L = P(r, 20.0, 50, 100, 150), s = P(r, 4.0, 6, 10, 16, 25, 35), dv = Sqrt(3) * Rho * L * I / s;
            return Q(XF("Circuito trifásico de {0} V (linha), {1} A, {2} m de cabo de cobre de {3} mm² por fase. Qual a queda de tensão de linha? (ΔV = √3·ρ·L·I/S, fp = 1)", "Three-phase {0} V (line) circuit, {1} A, {2} m of {3} mm² copper cable per phase. What is the line voltage drop? (ΔV = √3·ρ·L·I/S, pf = 1)", "Circuito trifásico de {0} V (línea), {1} A, {2} m de cable de cobre de {3} mm² por fase. ¿Cuál es la caída de tensión de línea? (ΔV = √3·ρ·L·I/S, fp = 1)", N(V, 0), N(I, 0), N(L, 0), N(s, 0)), dv, "V", YF("ΔV = √3 · (1/56) · {0} · {1} / {2} = {3} V ({4}% de {5} V).", N(L, 0), N(I, 0), N(s, 0), N(dv, 3), N(dv / V * 100, 2), N(V, 0)));
        }),
        _ => F(r =>
        {
            double s = P(r, 2.5, 4.0, 6.0, 10.0), I = P(r, 15.0, 20, 25, 32), V = P(r, 127.0, 220), pct = P(r, 3.0, 4), L = pct / 100 * V * s / (2 * Rho * I);
            return Q(XF("Um circuito monofásico de {0} V alimenta {1} A com condutores de cobre de {2} mm². Qual o comprimento máximo (da fonte até a carga) para a queda não passar de {3}%?", "A {0} V single-phase circuit feeds {1} A with {2} mm² copper conductors. What is the maximum length (source to load) so that the drop does not exceed {3}%?", "Un circuito monofásico de {0} V alimenta {1} A con conductores de cobre de {2} mm². ¿Cuál es la longitud máxima (de la fuente a la carga) para que la caída no pase de {3}%?", N(V, 0), N(I, 0), N(s), N(pct, 0)), L, "m", YF("ΔVmáx = {0} V; L = ΔV·S / (2·ρ·I) = {0} · {1} / (2 · (1/56) · {2}) = {3} m.", N(V * pct / 100, 2), N(s), N(I, 0), N(L, 1)));
        }, r =>
        {
            double s = P(r, 2.5, 4.0, 6.0), L = P(r, 20.0, 30, 50), I = P(r, 15.0, 20, 25), cu = 2 * Rho * L * I / s, V = 127, pct = cu / V * 100, s2 = s * 2, cu2 = 2 * Rho * L * I / s2;
            return Q(XF("Um par de condutores de cobre de {0} mm² e {1} m leva {2} A em 127 V. Se a seção for dobrada, por quantos volts a queda de tensão diminui?", "A pair of {0} mm² copper conductors, {1} m long, carries {2} A at 127 V. If the cross-section is doubled, by how many volts does the voltage drop decrease?", "Un par de conductores de cobre de {0} mm² y {1} m lleva {2} A a 127 V. Si se duplica la sección, ¿cuántos voltios disminuye la caída de tensión?", N(s), N(L, 0), N(I, 0)), cu - cu2, "V", YF("ΔV = 2·ρ·L·I/S: {0} V com {1} mm²; {2} V com {3} mm²; diferença = {4} V (a queda cai à metade).", N(cu, 3), N(s), N(cu2, 3), N(s2), N(cu - cu2, 3)));
        })
    };
}

[Exercises("ele-20-protecao")]
public sealed class Ele20Protection : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double V = P(r, 127.0, 220, 380), Z = S(r, 0.03, 1.5, 0.01), isc = V / Z;
            return Q(XF("Um curto-circuito franco ocorre em {0} V; a impedância total do laço (fonte + cabos) é {1} Ω. Qual a corrente de curto?", "A solid short circuit occurs at {0} V; the total loop impedance (source + cables) is {1} Ω. What is the short-circuit current?", "Ocurre un cortocircuito franco a {0} V; la impedancia total del lazo (fuente + cables) es {1} Ω. ¿Cuál es la corriente de cortocircuito?", N(V, 0), N(Z, 2)), isc, "A", YF("Isc = V / Z = {0} / {1} = {2} A.", N(V, 0), N(Z, 2), N(isc, 1)));
        }, r =>
        {
            double V = P(r, 30.0, 50), I = S(r, 0.03, 2, 0.01), R = V / I;
            return Q(XF("Para que uma pessoa não receba mais que {0} V de tensão de toque, com uma corrente de falta à terra de {1} A, qual a resistência de aterramento máxima?", "So that a person gets no more than {0} V of touch voltage, with a ground-fault current of {1} A, what is the maximum earthing resistance?", "Para que una persona no reciba más de {0} V de tensión de contacto, con una corriente de falla a tierra de {1} A, ¿cuál es la resistencia de puesta a tierra máxima?", N(V, 0), N(I, 2)), R, "Ω", YF("R = V / I = {0} / {1} = {2} Ω.", N(V, 0), N(I, 2), N(R, 1)));
        }),
        Difficulty.Medium => F(r =>
        {
            double sn = P(r, 75.0, 112.5, 150, 225, 300, 500), vl = P(r, 220.0, 380), z = P(r, 3.5, 4.0, 4.5, 5.0, 5.5), inn = sn * 1000 / (Sqrt(3) * vl), isc = inn / (z / 100);
            return Q(XF("Um transformador trifásico de {0} kVA, {1} V, impedância de {2}%. Qual a corrente de curto-circuito trifásica nos terminais do secundário (barramento infinito no primário)?", "A three-phase transformer, {0} kVA, {1} V, {2}% impedance. What is the three-phase short-circuit current at the secondary terminals (infinite bus on the primary)?", "Un transformador trifásico de {0} kVA, {1} V, impedancia de {2}%. ¿Cuál es la corriente de cortocircuito trifásico en los terminales del secundario (barra infinita en el primario)?", N(sn, 1), N(vl, 0), N(z, 1)), isc, "A", YF("In = S / (√3·V) = {0} / (1,732 · {1}) = {2} A; Icc = In / z% = {2} / {3} = {4} A.", N(sn * 1000, 0), N(vl, 0), N(inn, 1), N(z / 100, 3), N(isc, 0)));
        }, r =>
        {
            double inn = P(r, 10.0, 16, 20, 25, 32, 40, 63), k = P(r, 5.0, 10, 20);
            return Q(XF("Um disjuntor de {0} A com disparo magnético instantâneo em {1}×In. Qual a corrente mínima de curto-circuito que o faz disparar de imediato?", "A {0} A circuit breaker with instantaneous magnetic trip at {1}×In. What is the minimum short-circuit current that makes it trip instantly?", "Un interruptor de {0} A con disparo magnético instantáneo en {1}×In. ¿Cuál es la corriente mínima de cortocircuito que lo dispara de inmediato?", N(inn, 0), N(k, 0)), inn * k, "A", YF("Idisparo = {0} × {1} A = {2} A.", N(k, 0), N(inn, 0), N(inn * k, 0)));
        }),
        Difficulty.Hard => F(r =>
        {
            double V = P(r, 127.0, 220), Rf = P(r, 0.2, 0.5, 1.0, 2.0), Rt = P(r, 5.0, 10, 20), i = V / (Rf + Rt), vt = i * Rt;
            return Q(XF("Falta fase-terra em {0} V: o laço tem resistência de {1} Ω nos condutores e {2} Ω no aterramento da carcaça. Qual a tensão de toque na carcaça (queda no aterramento)?", "Phase-to-ground fault at {0} V: the loop has {1} Ω in the conductors and {2} Ω in the frame earthing. What is the touch voltage on the frame (drop across the earthing)?", "Falla fase-tierra a {0} V: el lazo tiene {1} Ω en los conductores y {2} Ω en la puesta a tierra de la carcasa. ¿Cuál es la tensión de contacto en la carcasa (caída en la puesta a tierra)?", N(V, 0), N(Rf), N(Rt, 0)), vt, "V", YF("I = V/(Rf + Rt) = {0}/{1} = {2} A; Vtoque = I·Rt = {2} · {3} = {4} V.", N(V, 0), N(Rf + Rt, 2), N(i, 3), N(Rt, 0), N(vt, 2)));
        }, r =>
        {
            double V = P(r, 127.0, 220), In = P(r, 16.0, 20, 25, 32), k = 10, Zs = S(r, 0.4, 2.0, 0.05), isc = V / Zs;
            bool trips = isc >= k * In;
            return Fact(r, XF("Disjuntor curva C ({0} A, dispara em {1}×In) protege um circuito de {2} V cuja impedância do laço fase-terra é {3} Ω. Ele desliga de imediato numa falta à terra?", "A C-curve breaker ({0} A, trips at {1}×In) protects a {2} V circuit whose phase-to-earth loop impedance is {3} Ω. Does it trip instantly on a ground fault?", "Un interruptor curva C ({0} A, dispara en {1}×In) protege un circuito de {2} V cuya impedancia del lazo fase-tierra es {3} Ω. ¿Se desconecta de inmediato ante una falla a tierra?", N(In, 0), N(k, 0), N(V, 0), N(Zs)),
                trips ? X("Sim", "Yes", "Sí") : X("Não", "No", "No"), YF("Ifalta = V/Zs = {0}/{1} = {2} A; limite = {3}×{4} = {5} A → {6}.", N(V, 0), N(Zs), N(isc, 1), N(k, 0), N(In, 0), N(k * In, 0), trips ? X("dispara", "trips", "dispara") : X("não dispara de imediato", "does not trip instantly", "no dispara de inmediato")),
                trips ? X("Não", "No", "No") : X("Sim", "Yes", "Sí"));
        }),
        _ => F(r =>
        {
            double isc = P(r, 1000.0, 3000, 5000, 10000), t = P(r, 0.01, 0.02, 0.05, 0.1), k = P(r, 115.0, 143), s = isc * Sqrt(t) / k;
            return Q(XF("Um condutor de cobre (k = {0}) deve suportar uma corrente de curto de {1} A durante {2} s sem danos. Qual a seção mínima, em mm²? (S = I·√t / k)", "A copper conductor (k = {0}) must withstand a {1} A short-circuit current for {2} s without damage. What is the minimum cross-section, in mm²? (S = I·√t / k)", "Un conductor de cobre (k = {0}) debe soportar una corriente de cortocircuito de {1} A durante {2} s sin daño. ¿Cuál es la sección mínima, en mm²? (S = I·√t / k)", N(k, 0), N(isc, 0), N(t, 2)), s, "mm²", YF("S = I·√t / k = {0} · √{1} / {2} = {3} mm².", N(isc, 0), N(t, 2), N(k, 0), N(s, 3)));
        }, r =>
        {
            double I = P(r, 1000.0, 3000, 5000), t = P(r, 0.01, 0.02, 0.05, 0.1), t2 = t * P(r, 2.0, 3, 4), e1 = I * I * t, e2 = I * I * t2;
            return Q(XF("Uma falta de {0} A é eliminada por um disjuntor em {1} s. Se o disjuntor fosse mais lento e levasse {2} s, quantas vezes maior seria a energia específica (I²t) dissipada nos condutores?", "A {0} A fault is cleared by a breaker in {1} s. If the breaker were slower and took {2} s, how many times larger would the specific energy (I²t) let through to the conductors be?", "Una falla de {0} A es eliminada por un interruptor en {1} s. Si el interruptor fuera más lento y tardara {2} s, ¿cuántas veces mayor sería la energía específica (I²t) disipada en los conductores?", N(I, 0), N(t, 2), N(t2, 2)), e2 / e1, "", YF("I²t₁ = {0}² · {1} = {2} A²s; I²t₂ = {0}² · {3} = {4} A²s; razão = {5}.", N(I, 0), N(t, 2), N(e1, 0), N(t2, 2), N(e2, 0), N(e2 / e1, 2)));
        })
    };
}
