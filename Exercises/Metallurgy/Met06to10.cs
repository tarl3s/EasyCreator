namespace EasyCreator.Exercises;

[Exercises("met-06-eletrodo")]
public sealed class Met06Smaw : PoolGen
{
    static Exercise Extra(Random r)
    {
        double i = S(r, 60, 250, 5), u = S(r, 18, 40, 0.5);
        return Q(XF("Um arco de {0} V com corrente de {1} A. Qual a potência elétrica do arco, em kW?", "An arc of {0} V at a current of {1} A. What is the arc's electrical power, in kW?", "Un arco de {0} V con corriente de {1} A. ¿Cuál es la potencia eléctrica del arco, en kW?", N(u), N(i, 0)), u * i / 1000, "kW", YF("P = U·I = {0} · {1} = {2} kW.", N(u), N(i, 0), N(u * i / 1000, 3)));
    }

    static Exercise Range(Random r)
    {
        bool e7018 = r.Next(2) == 0; int k = r.Next(3);
        string[] dia = { N(2.5, 2), N(3.25, 2), N(4.0, 1) };
        string[] a6013 = { "60-90 A", "90-130 A", "130-180 A" }, a7018 = { "70-100 A", "100-145 A", "140-190 A" };
        var tab = e7018 ? a7018 : a6013;
        return Ex.Choice(r, XF("Na tabela de correntes, qual a faixa recomendada para o eletrodo {0} de {1} mm?", "In the current table, what is the recommended range for the {0} electrode of {1} mm?", "En la tabla de corrientes, ¿cuál es el rango recomendado para el electrodo {0} de {1} mm?", e7018 ? "E7018" : "E6013", N(double.Parse(dia[k].Replace(',', '.'), System.Globalization.CultureInfo.InvariantCulture), 2)), tab[k], tab.Where((_, i) => i != k).Concat(new[] { (e7018 ? a6013 : a7018)[k] }),
            X("E6013: 2,5 mm 60-90 A, 3,25 mm 90-130 A, 4,0 mm 130-180 A. E7018: 2,5 mm 70-100 A, 3,25 mm 100-145 A, 4,0 mm 140-190 A. Eletrodo mais grosso, corrente maior.", "E6013: 2.5 mm 60-90 A, 3.25 mm 90-130 A, 4.0 mm 130-180 A. E7018: 2.5 mm 70-100 A, 3.25 mm 100-145 A, 4.0 mm 140-190 A. Thicker electrode, higher current.", "E6013: 2,5 mm 60-90 A, 3,25 mm 90-130 A, 4,0 mm 130-180 A. E7018: 2,5 mm 70-100 A, 3,25 mm 100-145 A, 4,0 mm 140-190 A. Electrodo más grueso, mayor corriente."));
    }

    static Exercise Fact(Random r)
    {
        var bank = new (string q, string right, string[] wrong)[]
        {
            (X("O que significa o 18 final em E7018?", "What does the final 18 mean in E7018?", "¿Qué significa el 18 final en E7018?"), X("Revestimento básico (baixo hidrogênio), todas as posições, CA ou CC+", "Basic coating (low hydrogen), all positions, AC or DC+", "Revestimiento básico (bajo hidrógeno), todas las posiciones, CA o CC+"), new[] { X("Diâmetro de 1,8 mm", "A diameter of 1.8 mm", "Diámetro de 1,8 mm"), X("Revestimento celulósico, só plana", "Cellulosic coating, flat position only", "Revestimiento celulósico, solo plana"), X("Resistência de 18 ksi", "A strength of 18 ksi", "Resistencia de 18 ksi") }),
            (X("Por que o E7018 deve ser guardado em estufa e secado antes do uso?", "Why must E7018 be kept in an oven and dried before use?", "¿Por qué el E7018 debe guardarse en estufa y secarse antes de usar?"), X("O revestimento absorve umidade e libera hidrogênio, que causa trincas", "The coating absorbs moisture and releases hydrogen, which causes cracks", "El revestimiento absorbe humedad y libera hidrógeno, que causa grietas"), new[] { X("Para aumentar a corrente", "To raise the current", "Para aumentar la corriente"), X("Para o eletrodo ficar mais flexível", "To make the electrode more flexible", "Para que el electrodo sea más flexible"), X("Para dar cor ao cordão", "To colour the bead", "Para dar color al cordón") }),
            (X("Qual a função principal do revestimento do eletrodo?", "What is the main role of the electrode coating?", "¿Cuál es la función principal del revestimiento del electrodo?"), X("Estabilizar o arco, proteger a poça (gás e escória) e adicionar elementos", "Stabilise the arc, shield the pool (gas and slag) and add alloying elements", "Estabilizar el arco, proteger el baño (gas y escoria) y añadir elementos"), new[] { X("Apenas isolar o eletrodo na mão", "Only to insulate the electrode in the hand", "Solo aislar el electrodo en la mano"), X("Esfriar a peça", "Cool the workpiece", "Enfriar la pieza"), X("Aumentar o diâmetro do arame", "Increase the wire diameter", "Aumentar el diámetro del alambre") }),
            (X("Um arco muito longo no eletrodo revestido causa:", "A very long arc with a coated electrode causes:", "Un arco muy largo con electrodo revestido causa:"), X("arco instável, respingos e porosidade", "an unstable arc, spatter and porosity", "arco inestable, salpicaduras y porosidad"), new[] { X("penetração maior e cordão mais limpo", "deeper penetration and a cleaner bead", "mayor penetración y cordón más limpio"), X("menos tensão no arco", "less arc voltage", "menos tensión en el arco"), X("escória mais fácil de sair", "easier slag removal", "escoria más fácil de quitar") }),
        };
        var b = bank[r.Next(bank.Length)];
        return Fact(r, b.q, b.right, X("Os eletrodos básicos (E7018) dão solda de baixo hidrogênio e alta tenacidade, mas pedem estufa; os rutílicos (E6013) são fáceis de soldar.", "Basic electrodes (E7018) give low-hydrogen, tough welds but need an oven; rutile ones (E6013) are easy to weld with.", "Los electrodos básicos (E7018) dan soldaduras de bajo hidrógeno y alta tenacidad, pero requieren estufa; los rutílicos (E6013) son fáciles de soldar."), b.wrong);
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Extra, Range, Fact, r =>
        {
            double ksi = P(r, 60.0, 70, 80, 90, 100, 110), mpa = ksi * 6.895;
            return Q(XF("Em um eletrodo E{0}XX, o número indica a resistência mínima em ksi. Quantos MPa são {0} ksi? (1 ksi = 6,895 MPa)", "In an E{0}XX electrode, the number gives the minimum strength in ksi. How many MPa is {0} ksi? (1 ksi = 6.895 MPa)", "En un electrodo E{0}XX, el número indica la resistencia mínima en ksi. ¿Cuántos MPa son {0} ksi? (1 ksi = 6,895 MPa)", N(ksi, 0)), mpa, "MPa", YF("{0} ksi · 6,895 = {1} MPa.", N(ksi, 0), N(mpa, 1)));
        }),
        Difficulty.Medium => F(r =>
        {
            double u = S(r, 20, 28, 0.5), i = S(r, 90, 190, 5), v = S(r, 1.5, 5, 0.1), q = 0.8 * u * i / v / 1000;
            return Q(XF("Eletrodo revestido: U = {0} V, I = {1} A, v = {2} mm/s, η = 0,8. Qual o aporte térmico em kJ/mm?", "Coated electrode: U = {0} V, I = {1} A, v = {2} mm/s, η = 0.8. What is the heat input in kJ/mm?", "Electrodo revestido: U = {0} V, I = {1} A, v = {2} mm/s, η = 0,8. ¿Cuál es el aporte térmico en kJ/mm?", N(u), N(i, 0), N(v)), q, "kJ/mm", YF("Q = η·U·I / v / 1000 = 0,8 · {0} · {1} / {2} / 1000 = {3} kJ/mm.", N(u), N(i, 0), N(v), N(q, 3)));
        }, r =>
        {
            double u = S(r, 20, 28, 0.5), i = S(r, 90, 190, 5), v = S(r, 1.5, 5, 0.1), q = 0.8 * u * i / v / 1000, e = q * 1000;
            return Q(XF("Eletrodo revestido a {0} V e {1} A: soldando a {2} mm/s, qual a energia por metro de cordão, em kJ? (η = 0,8)", "Coated electrode at {0} V and {1} A: welding at {2} mm/s, how much energy per metre of bead, in kJ? (η = 0.8)", "Electrodo revestido a {0} V y {1} A: soldando a {2} mm/s, ¿cuánta energía por metro de cordón, en kJ? (η = 0,8)", N(u), N(i, 0), N(v)), e, "kJ", YF("Q = 0,8 · {0} · {1} / {2} / 1000 = {3} kJ/mm; por metro (1000 mm): {4} kJ.", N(u), N(i, 0), N(v), N(q, 3), N(e, 1)));
        }),
        Difficulty.Hard => F(r =>
        {
            double u = S(r, 20, 28, 0.5), i = S(r, 100, 190, 5), q = S(r, 0.8, 2.0, 0.1), v = 0.8 * u * i / (q * 1000);
            return Q(XF("Com U = {0} V e I = {1} A, que velocidade de soldagem (mm/s) dá um aporte térmico de {2} kJ/mm? (η = 0,8)", "With U = {0} V and I = {1} A, what travel speed (mm/s) gives a heat input of {2} kJ/mm? (η = 0.8)", "Con U = {0} V e I = {1} A, ¿qué velocidad de soldadura (mm/s) da un aporte térmico de {2} kJ/mm? (η = 0,8)", N(u), N(i, 0), N(q)), v, "mm/s", YF("v = η·U·I / (Q·1000) = 0,8 · {0} · {1} / ({2} · 1000) = {3} mm/s.", N(u), N(i, 0), N(q), N(v, 3)));
        }, r =>
        {
            double u = S(r, 20, 28, 0.5), v = S(r, 2, 5, 0.1), q = S(r, 0.8, 2.0, 0.1), i = q * 1000 * v / (0.8 * u);
            return Q(XF("Com U = {0} V e velocidade de {1} mm/s, que corrente (A) dá um aporte térmico de {2} kJ/mm? (η = 0,8)", "With U = {0} V and a travel speed of {1} mm/s, what current (A) gives a heat input of {2} kJ/mm? (η = 0.8)", "Con U = {0} V y una velocidad de {1} mm/s, ¿qué corriente (A) da un aporte térmico de {2} kJ/mm? (η = 0,8)", N(u), N(v), N(q)), i, "A", YF("I = Q·v·1000 / (η·U) = {0} · {1} · 1000 / (0,8 · {2}) = {3} A.", N(q), N(v), N(u), N(i, 1)));
        }),
        _ => F(r =>
        {
            double t = S(r, 40, 90, 5), v = S(r, 2, 5, 0.1), L = S(r, 1, 12, 0.5), bead = v * t / 1000, n = Math.Ceiling(L / bead);
            return Q(XF("Cada eletrodo queima durante {0} s de arco e a velocidade é {1} mm/s. Quantos eletrodos são necessários, no mínimo, para um cordão contínuo de {2} m?", "Each electrode burns for {0} s of arc and the travel speed is {1} mm/s. At least how many electrodes are needed for a continuous bead of {2} m?", "Cada electrodo se quema durante {0} s de arco y la velocidad es {1} mm/s. ¿Cuántos electrodos se necesitan, como mínimo, para un cordón continuo de {2} m?", N(t, 0), N(v), N(L)), n, "", YF("Cordão por eletrodo = v·t = {0} · {1} = {2} mm = {3} m; n = {4} / {3} = {5} → {6}.", N(v), N(t, 0), N(v * t, 0), N(bead, 3), N(L), N(L / bead, 2), N(n, 0)), 0.01);
        }, r =>
        {
            double i = S(r, 100, 180, 10), v = S(r, 2, 4, 0.2), q = S(r, 0.8, 2.0, 0.1), u = q * 1000 * v / (0.8 * i);
            return Q(XF("Um passe de eletrodo revestido com I = {0} A a {1} mm/s deve dar {2} kJ/mm (η = 0,8). Que tensão de arco, em V, isso exige?", "A coated-electrode pass with I = {0} A at {1} mm/s must give {2} kJ/mm (η = 0.8). What arc voltage, in V, does that require?", "Un pase de electrodo revestido con I = {0} A a {1} mm/s debe dar {2} kJ/mm (η = 0,8). ¿Qué tensión de arco, en V, exige eso?", N(i, 0), N(v), N(q)), u, "V", YF("U = Q·v·1000 / (η·I) = {0} · {1} · 1000 / (0,8 · {2}) = {3} V.", N(q), N(v), N(i, 0), N(u, 2)));
        })
    };
}

[Exercises("met-07-mig")]
public sealed class Met07Mig : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double dm = P(r, 0.8, 0.9, 1.0, 1.2, 1.6), a = Pi * dm * dm / 4;
            return Q(XF("Qual a área da seção de um arame MIG de {0} mm de diâmetro?", "What is the cross-sectional area of a {0} mm MIG wire?", "¿Cuál es el área de la sección de un alambre MIG de {0} mm de diámetro?", N(dm, 1)), a, "mm²", YF("A = π·d²/4 = π · {0}² / 4 = {1} mm².", N(dm, 1), N(a, 4)));
        }, r =>
        {
            var bank = new (string q, string right, string[] wrong)[]
            {
                (X("Qual o gás mais comum para MIG/MAG em aço carbono?", "Which gas is the most common for MIG/MAG on carbon steel?", "¿Cuál es el gas más común para MIG/MAG en acero al carbono?"), "Ar + 15-25% CO₂", new[] { X("Argônio puro", "Pure argon", "Argón puro"), X("Oxigênio puro", "Pure oxygen", "Oxígeno puro"), X("Hidrogênio", "Hydrogen", "Hidrógeno") }),
                (X("Qual gás se usa em MIG de alumínio?", "Which gas is used for MIG on aluminium?", "¿Qué gas se usa en MIG de aluminio?"), X("Argônio (ou Ar + He)", "Argon (or Ar + He)", "Argón (o Ar + He)"), new[] { "CO₂", "Ar + 25% CO₂", X("Nitrogênio", "Nitrogen", "Nitrógeno") }),
                (X("Qual a vazão típica de gás de proteção no MIG/MAG?", "What is the typical shielding gas flow in MIG/MAG?", "¿Cuál es el caudal típico de gas de protección en MIG/MAG?"), "12-18 L/min", new[] { "1-3 L/min", "40-60 L/min", "100 L/min" }),
                (X("Qual a transferência metálica em baixa tensão e corrente, que faz curtos-circuitos e serve para chapas finas?", "Which metal transfer mode, at low voltage and current, makes short circuits and suits thin plate?", "¿Qué transferencia metálica, con baja tensión y corriente, hace cortocircuitos y sirve para chapas finas?"), X("Curto-circuito", "Short-circuit", "Cortocircuito"), new[] { X("Spray (aerossol)", "Spray", "Spray (aerosol)"), X("Globular puro", "Pure globular", "Globular puro"), X("Arco submerso", "Submerged arc", "Arco sumergido") }),
            };
            var b = bank[r.Next(bank.Length)];
            return Fact(r, b.q, b.right, X("MIG/MAG: Ar + 15-25% CO₂ em aço; argônio puro (ou com hélio) em alumínio; vazão de 12-18 L/min; curto-circuito para chapas finas e spray para chapas grossas.", "MIG/MAG: Ar + 15-25% CO₂ on steel; pure argon (or with helium) on aluminium; flow 12-18 L/min; short-circuit for thin plate and spray for thick plate.", "MIG/MAG: Ar + 15-25% CO₂ en acero; argón puro (o con helio) en aluminio; caudal 12-18 L/min; cortocircuito para chapas finas y spray para chapas gruesas."), b.wrong);
        }, r =>
        {
            double wfs = S(r, 3, 15, 0.5), dm = P(r, 0.8, 1.0, 1.2), l = S(r, 8, 20, 1), t = l / wfs * 60;
            return Q(XF("Um arame de alimentação com rolo de {0} m e velocidade de arame de {1} m/min dura quantos segundos de arco?", "A feed wire spool with {0} m and a wire feed speed of {1} m/min lasts how many seconds of arc?", "Un alambre de alimentación de {0} m con velocidad de alambre de {1} m/min dura ¿cuántos segundos de arco?", N(l, 0), N(wfs)), t, "s", YF("t = comprimento / velocidade = {0} m / {1} m/min = {2} min = {3} s.", N(l, 0), N(wfs), N(l / wfs, 3), N(t, 1)));
        }),
        Difficulty.Medium => F(r =>
        {
            double wfs = S(r, 3, 15, 0.5), dm = P(r, 0.8, 1.0, 1.2, 1.6), a = Pi * dm * dm / 4, kgh = 7.85e-3 * a * wfs * 1000 * 60 / 1000;
            return Q(XF("Arame de aço de {0} mm de diâmetro alimentado a {1} m/min (densidade 7,85 g/cm³). Quantos kg de arame fundem por hora de arco aberto?", "A {0} mm diameter steel wire fed at {1} m/min (density 7.85 g/cm³). How many kg of wire melt per hour of arc-on time?", "Alambre de acero de {0} mm de diámetro alimentado a {1} m/min (densidad 7,85 g/cm³). ¿Cuántos kg de alambre se funden por hora de arco abierto?", N(dm, 1), N(wfs)), kgh, "kg/h", YF("A = {0} mm²; massa por mm = 7,85·10⁻³ g/mm³ · {0} = {1} g/m·10⁻³; por hora: {2} m/min · 60 · {3} g/m = {4} kg/h.", N(a, 4), N(7.85e-3 * a * 1000, 3), N(wfs), N(7.85 * a, 3), N(kgh, 3)));
        }, r =>
        {
            double u = S(r, 18, 32, 0.5), i = S(r, 120, 300, 5), v = S(r, 4, 10, 0.2), q = 0.8 * u * i / v / 1000;
            return Q(XF("MIG/MAG: U = {0} V, I = {1} A, v = {2} mm/s, η = 0,8. Qual o aporte térmico em kJ/mm?", "MIG/MAG: U = {0} V, I = {1} A, v = {2} mm/s, η = 0.8. What is the heat input in kJ/mm?", "MIG/MAG: U = {0} V, I = {1} A, v = {2} mm/s, η = 0,8. ¿Cuál es el aporte térmico en kJ/mm?", N(u), N(i, 0), N(v)), q, "kJ/mm", YF("Q = 0,8 · {0} · {1} / {2} / 1000 = {3} kJ/mm.", N(u), N(i, 0), N(v), N(q, 3)));
        }),
        Difficulty.Hard => F(r =>
        {
            double fl = S(r, 12, 18, 1), L = S(r, 0.5, 10, 0.5), v = S(r, 3, 8, 0.5), t = L * 1000 / v / 60, g = fl * t;
            return Q(XF("Em uma costura de {0} m a {1} mm/s com vazão de gás de {2} L/min, quantos litros de gás são gastos só com o arco aberto?", "In a {0} m seam at {1} mm/s with a gas flow of {2} L/min, how many litres of gas are used during arc-on time alone?", "En una costura de {0} m a {1} mm/s con caudal de gas de {2} L/min, ¿cuántos litros de gas se gastan solo con el arco abierto?", N(L), N(v), N(fl, 0)), g, "L", YF("t = {0} mm / {1} mm/s = {2} s = {3} min; gás = {4} L/min · {3} min = {5} L.", N(L * 1000, 0), N(v), N(L * 1000 / v, 0), N(t, 3), N(fl, 0), N(g, 1)));
        }, r =>
        {
            double a = S(r, 20, 90, 5), L = S(r, 0.5, 6, 0.5), eff = P(r, 0.92, 0.95, 0.97), kg = a * L * 1000 * 7.85e-6 / eff;
            return Q(XF("Uma junta tem {0} mm² de área de metal depositado e {1} m de comprimento. Com rendimento de deposição de {2}% (o resto vira respingo), quantos kg de arame de aço são consumidos? (densidade 7,85 g/cm³)", "A joint has {0} mm² of deposited metal area and is {1} m long. With a deposition efficiency of {2}% (the rest becomes spatter), how many kg of steel wire are used? (density 7.85 g/cm³)", "Una junta tiene {0} mm² de área de metal depositado y {1} m de longitud. Con un rendimiento de deposición de {2}% (el resto se vuelve salpicadura), ¿cuántos kg de alambre de acero se consumen? (densidad 7,85 g/cm³)", N(a, 0), N(L), N(eff * 100, 0)), kg, "kg", YF("Volume = {0} mm² · {1} mm = {2} mm³ = {3} cm³; massa = {3} · 7,85 g = {4} kg; arame = {4} / {5} = {6} kg.", N(a, 0), N(L * 1000, 0), N(a * L * 1000, 0), N(a * L, 2), N(a * L * 1000 * 7.85e-6, 4), N(eff, 2), N(kg, 4)));
        }),
        _ => F(r =>
        {
            double dm = P(r, 1.0, 1.2), wfs = S(r, 5, 12, 0.5), a = Pi * dm * dm / 4, kgh = 7.85e-3 * a * wfs * 60, dep = kgh * 0.95, area = S(r, 30, 80, 5), mm = area * 7.85e-6 * 1000;
            double t = mm / dep * 60;
            return Q(XF("Arame de {0} mm a {1} m/min (rendimento de deposição 95%). Qual a taxa de deposição, em kg/h, e quanto tempo de arco, em minutos, leva para depositar 1 m de uma junta de {2} mm² de área? Responda o TEMPO (min).", "{0} mm wire at {1} m/min (95% deposition efficiency). What is the deposition rate in kg/h, and how long does it take in arc-on minutes to deposit 1 m of a joint of {2} mm² area? Answer the TIME (min).", "Alambre de {0} mm a {1} m/min (rendimiento de deposición 95%). ¿Cuál es la tasa de deposición en kg/h y cuántos minutos de arco lleva depositar 1 m de una junta de {2} mm² de área? Responda el TIEMPO (min).", N(dm, 1), N(wfs), N(area, 0)), t, "min", YF("Arame: {0} kg/h; deposição = 0,95 · {0} = {1} kg/h. Metal da junta: {2} mm² · 1000 mm = {3} kg; t = {3}/{1} · 60 = {4} min.", N(kgh, 3), N(dep, 3), N(area, 0), N(mm / 1000, 4), N(t, 2)), 3);
        }, r =>
        {
            double u = S(r, 20, 30, 0.5), i = S(r, 150, 280, 10), q = S(r, 0.5, 1.5, 0.1), v = 0.8 * u * i / (q * 1000);
            return Q(XF("MIG/MAG a {0} V e {1} A: que velocidade de soldagem (mm/s) mantém o aporte térmico em {2} kJ/mm? (η = 0,8)", "MIG/MAG at {0} V and {1} A: what travel speed (mm/s) keeps the heat input at {2} kJ/mm? (η = 0.8)", "MIG/MAG a {0} V y {1} A: ¿qué velocidad de soldadura (mm/s) mantiene el aporte térmico en {2} kJ/mm? (η = 0,8)", N(u), N(i, 0), N(q)), v, "mm/s", YF("v = 0,8 · {0} · {1} / ({2} · 1000) = {3} mm/s.", N(u), N(i, 0), N(q), N(v, 3)));
        })
    };
}

[Exercises("met-08-tig")]
public sealed class Met08Tig : PoolGen
{
    static Exercise Extra(Random r)
    {
        double i = S(r, 40, 250, 5), u = S(r, 9, 18, 0.5);
        return Q(XF("Um arco de {0} V com corrente de {1} A. Qual a potência elétrica do arco, em kW?", "An arc of {0} V at a current of {1} A. What is the arc's electrical power, in kW?", "Un arco de {0} V con corriente de {1} A. ¿Cuál es la potencia eléctrica del arco, en kW?", N(u), N(i, 0)), u * i / 1000, "kW", YF("P = U·I = {0} · {1} = {2} kW.", N(u), N(i, 0), N(u * i / 1000, 3)));
    }

    static Exercise Fact(Random r)
    {
        var bank = new (string q, string right, string[] wrong)[]
        {
            (X("Que corrente se usa em TIG de alumínio para quebrar a camada de óxido?", "Which current is used in TIG of aluminium to break the oxide layer?", "¿Qué corriente se usa en TIG de aluminio para romper la capa de óxido?"), X("CA (corrente alternada)", "AC (alternating current)", "CA (corriente alterna)"), new[] { X("CC- com eletrodo negativo", "DC- with the negative electrode", "CC- con electrodo negativo"), X("CC pulsada só", "pulsed DC only", "CC pulsada solo"), X("Nenhuma, o óxido sai sozinho", "None, the oxide comes off by itself", "Ninguna, el óxido sale solo") }),
            (X("Que polaridade se usa em TIG de aço e inox?", "Which polarity is used in TIG of steel and stainless steel?", "¿Qué polaridad se usa en TIG de acero e inox?"), X("CC com eletrodo negativo (CC-)", "DC with the negative electrode (DCEN)", "CC con electrodo negativo (CC-)"), new[] { X("CC com eletrodo positivo", "DC with the positive electrode", "CC con electrodo positivo"), X("CA sempre", "AC always", "CA siempre"), X("Qualquer uma, não muda nada", "Either, it makes no difference", "Cualquiera, no cambia nada") }),
            (X("Qual o gás de proteção típico do TIG?", "What is the typical shielding gas for TIG?", "¿Cuál es el gas de protección típico del TIG?"), X("Argônio puro", "Pure argon", "Argón puro"), new[] { "CO₂", X("Ar + 25% CO₂", "Ar + 25% CO₂", "Ar + 25% CO₂"), X("Oxigênio", "Oxygen", "Oxígeno") }),
            (X("O eletrodo de tungstênio do TIG:", "The tungsten electrode in TIG:", "El electrodo de tungsteno del TIG:"), X("não se consome e só serve para o arco; o metal vem da vareta", "is not consumed and only carries the arc; the metal comes from the filler rod", "no se consume y solo sirve para el arco; el metal viene de la varilla"), new[] { X("funde e vira o metal de adição", "melts and becomes the filler metal", "se funde y se convierte en metal de aporte"), X("deve tocar a poça para abrir o arco", "must touch the pool to strike the arc", "debe tocar el baño para abrir el arco"), X("é revestido por fluxo", "is coated with flux", "está revestido con fundente") }),
        };
        var b = bank[r.Next(bank.Length)];
        return Fact(r, b.q, b.right, X("TIG: eletrodo de tungstênio não consumível, gás argônio; CA para alumínio (limpeza catódica do óxido), CC- para aço e inox.", "TIG: non-consumable tungsten electrode, argon gas; AC for aluminium (cathodic oxide cleaning), DCEN for steel and stainless.", "TIG: electrodo de tungsteno no consumible, gas argón; CA para aluminio (limpieza catódica del óxido), CC- para acero e inox."), b.wrong);
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Extra, Fact, r =>
        {
            double e = S(r, 0.5, 6, 0.1), i = 40 * e;
            return Q(XF("Pela regra prática do TIG no aço (~1 A para cada 0,025 mm de espessura, ou seja, 40 A por mm), que corrente se usa para uma chapa de {0} mm?", "By the TIG rule of thumb on steel (~1 A per 0.025 mm of thickness, i.e. 40 A per mm), what current is used for a {0} mm plate?", "Según la regla práctica del TIG en acero (~1 A por cada 0,025 mm de espesor, es decir, 40 A por mm), ¿qué corriente se usa para una chapa de {0} mm?", N(e, 1)), i, "A", YF("I ≈ 40 A/mm · {0} mm = {1} A.", N(e, 1), N(i, 0)));
        }),
        Difficulty.Medium => F(Extra, r =>
        {
            double u = S(r, 10, 16, 0.5), i = S(r, 50, 250, 5), v = S(r, 0.8, 3, 0.1), q = 0.6 * u * i / v / 1000;
            return Q(XF("TIG: U = {0} V, I = {1} A, v = {2} mm/s, η = 0,6. Qual o aporte térmico em kJ/mm?", "TIG: U = {0} V, I = {1} A, v = {2} mm/s, η = 0.6. What is the heat input in kJ/mm?", "TIG: U = {0} V, I = {1} A, v = {2} mm/s, η = 0,6. ¿Cuál es el aporte térmico en kJ/mm?", N(u), N(i, 0), N(v)), q, "kJ/mm", YF("Q = 0,6 · {0} · {1} / {2} / 1000 = {3} kJ/mm.", N(u), N(i, 0), N(v), N(q, 3)));
        }, Fact),
        Difficulty.Hard => F(r =>
        {
            double fl = S(r, 7, 12, 0.5), L = S(r, 100, 3000, 50), v = S(r, 0.8, 3, 0.1), t = L / v / 60, g = fl * t;
            return Q(XF("Uma solda TIG de {0} mm de comprimento a {1} mm/s com argônio a {2} L/min: quantos litros de argônio gasta (só com arco aberto)?", "A {0} mm TIG weld at {1} mm/s with argon at {2} L/min: how many litres of argon does it use (arc-on time only)?", "Una soldadura TIG de {0} mm de longitud a {1} mm/s con argón a {2} L/min: ¿cuántos litros de argón gasta (solo con arco abierto)?", N(L, 0), N(v), N(fl)), g, "L", YF("t = {0}/{1} = {2} s = {3} min; gás = {4} · {3} = {5} L.", N(L, 0), N(v), N(L / v, 1), N(t, 3), N(fl), N(g, 1)));
        }, r =>
        {
            double u = S(r, 10, 16, 0.5), i = S(r, 60, 200, 5), q = S(r, 0.2, 0.8, 0.05), v = 0.6 * u * i / (q * 1000);
            return Q(XF("TIG a {0} V e {1} A: que velocidade (mm/s) mantém o aporte térmico em {2} kJ/mm? (η = 0,6)", "TIG at {0} V and {1} A: what speed (mm/s) keeps the heat input at {2} kJ/mm? (η = 0.6)", "TIG a {0} V y {1} A: ¿qué velocidad (mm/s) mantiene el aporte térmico en {2} kJ/mm? (η = 0,6)", N(u), N(i, 0), N(q)), v, "mm/s", YF("v = 0,6 · {0} · {1} / ({2} · 1000) = {3} mm/s.", N(u), N(i, 0), N(q), N(v, 3)));
        }),
        _ => F(r =>
        {
            double u = S(r, 12, 15, 0.5), i = S(r, 100, 200, 10), v = S(r, 1, 3, 0.1), qt = 0.6 * u * i / v / 1000, um = S(r, 22, 28, 0.5), im = S(r, 150, 250, 10), vm = S(r, 4, 8, 0.2), qm = 0.8 * um * im / vm / 1000;
            return Q(XF("TIG ({0} V, {1} A, {2} mm/s, η = 0,6) contra MIG ({3} V, {4} A, {5} mm/s, η = 0,8): quantas vezes o aporte do MIG é maior que o do TIG?", "TIG ({0} V, {1} A, {2} mm/s, η = 0.6) against MIG ({3} V, {4} A, {5} mm/s, η = 0.8): how many times larger is the MIG heat input than the TIG one?", "TIG ({0} V, {1} A, {2} mm/s, η = 0,6) contra MIG ({3} V, {4} A, {5} mm/s, η = 0,8): ¿cuántas veces es mayor el aporte del MIG que el del TIG?", N(u), N(i, 0), N(v), N(um), N(im, 0), N(vm)), qm / qt, "", YF("Q_TIG = {0} kJ/mm; Q_MIG = {1} kJ/mm; razão = {2}.", N(qt, 3), N(qm, 3), N(qm / qt, 2)));
        }, r =>
        {
            double e = S(r, 1, 6, 0.1), i = 40 * e, f = S(r, 1.1, 1.3, 0.05), ia = i * f;
            return Q(XF("Para uma chapa de aço de {0} mm usa-se ~40 A por mm. No alumínio, que espalha o calor, a corrente em CA é cerca de {1}× maior. Que corrente se usa em uma chapa de alumínio de mesma espessura?", "For a {0} mm steel plate ~40 A per mm is used. On aluminium, which spreads heat, the AC current is about {1}× higher. What current is used on an aluminium plate of the same thickness?", "Para una chapa de acero de {0} mm se usan ~40 A por mm. En aluminio, que dispersa el calor, la corriente en CA es aproximadamente {1}× mayor. ¿Qué corriente se usa en una chapa de aluminio del mismo espesor?", N(e, 1), N(f, 2)), ia, "A", YF("Aço: 40 · {0} = {1} A; alumínio: {1} · {2} = {3} A.", N(e, 1), N(i, 0), N(f, 2), N(ia, 0)));
        })
    };
}

[Exercises("met-09-defeitos")]
public sealed class Met09Defects : PoolGen
{
    static Exercise Defect(Random r)
    {
        var bank = new (string q, string right, string[] wrong)[]
        {
            (X("Poros esféricos distribuídos no cordão são causados principalmente por:", "Spherical pores spread through the bead are mainly caused by:", "Los poros esféricos distribuidos en el cordón se deben principalmente a:"), X("gás de proteção insuficiente ou contaminação (umidade, óleo, ferrugem)", "insufficient shielding gas or contamination (moisture, oil, rust)", "gas de protección insuficiente o contaminación (humedad, aceite, óxido)"), new[] { X("corrente muito baixa só", "low current only", "solo corriente muy baja"), X("excesso de pré-aquecimento", "too much preheating", "exceso de precalentamiento"), X("velocidade muito baixa só", "low travel speed only", "solo velocidad muy baja") }),
            (X("Falta de fusão entre o cordão e o metal base resulta tipicamente de:", "Lack of fusion between the bead and the base metal typically results from:", "La falta de fusión entre el cordón y el metal base resulta típicamente de:"), X("pouco calor (corrente baixa/velocidade alta) ou ângulo errado da tocha", "too little heat (low current/high speed) or a wrong torch angle", "poco calor (corriente baja/velocidad alta) o ángulo incorrecto de la antorcha"), new[] { X("excesso de gás", "too much gas", "exceso de gas"), X("arame muito fino", "a wire that is too thin", "alambre muy delgado"), X("pré-aquecimento alto", "high preheating", "precalentamiento alto") }),
            (X("A mordedura (undercut) é um sulco na borda do cordão causado por:", "Undercut is a groove at the bead edge caused by:", "La mordedura (undercut) es un surco en el borde del cordón causado por:"), X("corrente/tensão excessiva ou velocidade alta", "excessive current/voltage or high speed", "corriente/tensión excesiva o velocidad alta"), new[] { X("corrente baixa e arco curto", "low current and a short arc", "corriente baja y arco corto"), X("uso de argônio", "using argon", "uso de argón"), X("pré-aquecimento", "preheating", "precalentamiento") }),
            (X("A trinca a frio (induzida por hidrogênio) aparece:", "Cold cracking (hydrogen-induced) appears:", "La fisura en frío (inducida por hidrógeno) aparece:"), X("depois que a solda esfria, na ZTA de aços de alta temperabilidade com hidrogênio", "after the weld cools, in the HAZ of high-hardenability steels with hydrogen", "después de que la soldadura se enfría, en la ZAT de aceros de alta templabilidad con hidrógeno"), new[] { X("durante a solidificação, no centro do cordão", "during solidification, at the bead centre", "durante la solidificación, en el centro del cordón"), X("só em alumínio", "only in aluminium", "solo en aluminio"), X("nunca em aços", "never in steels", "nunca en aceros") }),
            (X("Inclusão de escória no cordão geralmente vem de:", "Slag inclusion in the bead usually comes from:", "La inclusión de escoria en el cordón suele venir de:"), X("limpeza ruim entre passes e manipulação incorreta", "poor cleaning between passes and incorrect manipulation", "limpieza deficiente entre pases y manipulación incorrecta"), new[] { X("gás de proteção em excesso", "too much shielding gas", "exceso de gas de protección"), X("arame TIG", "TIG wire", "alambre TIG"), X("velocidade alta só", "high speed only", "solo velocidad alta") }),
            (X("Qual posição de soldagem de chapas é a vertical?", "Which plate welding position is the vertical one?", "¿Qué posición de soldadura de chapas es la vertical?"), "3G", new[] { "1G", "2G", "4G" }),
            (X("Qual posição de soldagem de chapas é a sobrecabeça?", "Which plate welding position is the overhead one?", "¿Qué posición de soldadura de chapas es la sobrecabeza?"), "4G", new[] { "1G", "2G", "3G" }),
            (X("Qual posição de chapas é a plana (a mais fácil)?", "Which plate position is the flat one (the easiest)?", "¿Qué posición de chapas es la plana (la más fácil)?"), "1G", new[] { "2G", "3G", "4G" }),
        };
        var b = bank[r.Next(bank.Length)];
        return Fact(r, b.q, b.right, X("Defeito → causa: porosidade (gás/contaminação), falta de fusão (pouco calor), mordedura (excesso de corrente/velocidade), escória (limpeza), trinca a frio (hidrogênio). Posições: 1G plana, 2G horizontal, 3G vertical, 4G sobrecabeça.", "Defect → cause: porosity (gas/contamination), lack of fusion (too little heat), undercut (excess current/speed), slag (cleaning), cold crack (hydrogen). Positions: 1G flat, 2G horizontal, 3G vertical, 4G overhead.", "Defecto → causa: porosidad (gas/contaminación), falta de fusión (poco calor), mordedura (exceso de corriente/velocidad), escoria (limpieza), fisura en frío (hidrógeno). Posiciones: 1G plana, 2G horizontal, 3G vertical, 4G sobrecabeza."), b.wrong);
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Defect, Defect, r =>
        {
            double z = S(r, 3, 20, 0.5), a = 0.707 * z;
            return Q(XF("Uma solda em ângulo (filete) tem perna z = {0} mm. Qual a garganta efetiva a?", "A fillet weld has leg z = {0} mm. What is the effective throat a?", "Una soldadura en ángulo (filete) tiene cateto z = {0} mm. ¿Cuál es la garganta efectiva a?", N(z, 1)), a, "mm", YF("a = 0,707·z = 0,707 · {0} = {1} mm.", N(z, 1), N(a, 2)));
        }),
        Difficulty.Medium => F(Defect, r =>
        {
            double a = S(r, 2, 14, 0.5), z = 1.414 * a;
            return Q(XF("O projeto pede garganta efetiva a = {0} mm em uma solda de filete. Qual a perna z necessária?", "The design calls for an effective throat a = {0} mm in a fillet weld. What leg z is needed?", "El proyecto pide una garganta efectiva a = {0} mm en una soldadura de filete. ¿Qué cateto z se necesita?", N(a, 1)), z, "mm", YF("z = 1,414·a = 1,414 · {0} = {1} mm.", N(a, 1), N(z, 2)));
        }, r =>
        {
            double z = S(r, 4, 14, 0.5), L = S(r, 50, 400, 10), tau = P(r, 80.0, 100, 120, 140), f = 0.707 * z * L * tau / 1000;
            return Q(XF("Um filete de perna {0} mm e {1} mm de comprimento tem tensão admissível ao cisalhamento de {2} MPa na garganta. Que força, em kN, ele suporta?", "A fillet weld with a {0} mm leg and {1} mm length has an allowable shear stress of {2} MPa in the throat. What force, in kN, can it carry?", "Un filete de cateto {0} mm y {1} mm de longitud tiene tensión admisible de cizallamiento de {2} MPa en la garganta. ¿Qué fuerza, en kN, soporta?", N(z, 1), N(L, 0), N(tau, 0)), f, "kN", YF("a = 0,707 · {0} = {1} mm; F = a·L·τ = {1} · {2} · {3} = {4} kN.", N(z, 1), N(0.707 * z, 3), N(L, 0), N(tau, 0), N(f, 2)));
        }),
        Difficulty.Hard => F(r =>
        {
            double f = S(r, 20, 200, 5), L = S(r, 80, 300, 10), tau = P(r, 80.0, 100, 120), a = f * 1000 / (L * tau), z = a / 0.707;
            return Q(XF("Um filete de {0} mm de comprimento deve suportar {1} kN com tensão admissível de {2} MPa na garganta. Qual a perna mínima, em mm?", "A fillet weld {0} mm long must carry {1} kN with an allowable stress of {2} MPa in the throat. What is the minimum leg, in mm?", "Un filete de {0} mm de longitud debe soportar {1} kN con tensión admisible de {2} MPa en la garganta. ¿Cuál es el cateto mínimo, en mm?", N(L, 0), N(f, 0), N(tau, 0)), z, "mm", YF("a = F / (L·τ) = {0} / ({1} · {2}) = {3} mm; z = a / 0,707 = {4} mm.", N(f * 1000, 0), N(L, 0), N(tau, 0), N(a, 3), N(z, 2)));
        }, Defect),
        _ => F(r =>
        {
            double f = S(r, 60, 300, 10), L = S(r, 80, 250, 10), tau = P(r, 80.0, 100, 120), a = f * 1000 / (2 * L * tau), z = a / 0.707;
            return Q(XF("Uma chapa é presa a outra por uma junta sobreposta com dois filetes longitudinais (um de cada lado), cada um com {0} mm de comprimento. Ela deve transmitir {1} kN (τ admissível {2} MPa na garganta). Qual a perna mínima de cada filete, em mm?", "A plate is attached to another by a lap joint with two longitudinal fillets (one on each side), each {0} mm long. It must transmit {1} kN (allowable τ {2} MPa in the throat). What is the minimum leg of each fillet, in mm?", "Una chapa se une a otra mediante una junta solapada con dos filetes longitudinales (uno a cada lado), cada uno de {0} mm de longitud. Debe transmitir {1} kN (τ admisible {2} MPa en la garganta). ¿Cuál es el cateto mínimo de cada filete, en mm?", N(L, 0), N(f, 0), N(tau, 0)), z, "mm", YF("Área resistente = 2·a·L; a = F / (2·L·τ) = {0} / (2 · {1} · {2}) = {3} mm; z = a/0,707 = {4} mm.", N(f * 1000, 0), N(L, 0), N(tau, 0), N(a, 3), N(z, 2)));
        }, r =>
        {
            double z = S(r, 4, 10, 0.5), k = P(r, 1.5, 2.0, 2.5, 3.0), f0 = z * z, f1 = (k * z) * (k * z);
            return Q(XF("A área da seção de um filete é aproximadamente z²/2. Se a perna passa de {0} mm para {1} mm, quantas vezes aumenta o volume de metal depositado por metro de junta?", "The cross-section area of a fillet is about z²/2. If the leg goes from {0} mm to {1} mm, how many times does the deposited metal volume per metre of joint increase?", "El área de la sección de un filete es aproximadamente z²/2. Si el cateto pasa de {0} mm a {1} mm, ¿cuántas veces aumenta el volumen de metal depositado por metro de junta?", N(z, 1), N(k * z, 2)), f1 / f0, "", YF("Volume ∝ z²: ({1}/{0})² = {2}.", N(z, 1), N(k * z, 2), N(f1 / f0, 3)));
        })
    };
}

[Exercises("met-10-engenharia")]
public sealed class Met10Engineering : PoolGen
{
    static (double C, double Mn, double Cr, double Mo, double V, double Ni, double Cu, double Si) Steel(Random r) =>
        (S(r, 0.10, 0.40, 0.01), S(r, 0.5, 1.6, 0.05), P(r, 0.0, 0.1, 0.2, 0.5, 1.0), P(r, 0.0, 0.1, 0.2, 0.3), P(r, 0.0, 0.02, 0.05), P(r, 0.0, 0.2, 0.5, 1.0), P(r, 0.0, 0.1, 0.2), S(r, 0.15, 0.5, 0.05));

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double c = S(r, 0.10, 0.40, 0.01), mn = S(r, 0.5, 1.6, 0.05), ce = c + mn / 6;
            return Q(XF("Aço com C = {0}% e Mn = {1}% (sem outros elementos). Qual o carbono equivalente CE (IIW)?", "Steel with C = {0}% and Mn = {1}% (no other elements). What is the carbon equivalent CE (IIW)?", "Acero con C = {0}% y Mn = {1}% (sin otros elementos). ¿Cuál es el carbono equivalente CE (IIW)?", N(c), N(mn)), ce, "", YF("CE = C + Mn/6 = {0} + {1}/6 = {2}.", N(c), N(mn), N(ce, 3)));
        }, r =>
        {
            double u = S(r, 20, 28, 0.5), i = S(r, 100, 250, 5), v = S(r, 2, 6, 0.2), q = 0.8 * u * i / v / 1000;
            return Q(XF("Aporte térmico: U = {0} V, I = {1} A, v = {2} mm/s, η = 0,8. Quantos kJ/mm?", "Heat input: U = {0} V, I = {1} A, v = {2} mm/s, η = 0.8. How many kJ/mm?", "Aporte térmico: U = {0} V, I = {1} A, v = {2} mm/s, η = 0,8. ¿Cuántos kJ/mm?", N(u), N(i, 0), N(v)), q, "kJ/mm", YF("Q = 0,8 · {0} · {1} / {2} / 1000 = {3} kJ/mm.", N(u), N(i, 0), N(v), N(q, 3)));
        }),
        Difficulty.Medium => F(r =>
        {
            var s = Steel(r); double ce = s.C + s.Mn / 6 + (s.Cr + s.Mo + s.V) / 5 + (s.Ni + s.Cu) / 15;
            return Q(XF("CE (IIW) = C + Mn/6 + (Cr+Mo+V)/5 + (Ni+Cu)/15. Aço: C {0}, Mn {1}, Cr {2}, Mo {3}, V {4}, Ni {5}, Cu {6} (todos em %). Qual o CE?", "CE (IIW) = C + Mn/6 + (Cr+Mo+V)/5 + (Ni+Cu)/15. Steel: C {0}, Mn {1}, Cr {2}, Mo {3}, V {4}, Ni {5}, Cu {6} (all in %). What is the CE?", "CE (IIW) = C + Mn/6 + (Cr+Mo+V)/5 + (Ni+Cu)/15. Acero: C {0}, Mn {1}, Cr {2}, Mo {3}, V {4}, Ni {5}, Cu {6} (todos en %). ¿Cuál es el CE?", N(s.C), N(s.Mn), N(s.Cr), N(s.Mo), N(s.V), N(s.Ni), N(s.Cu)), ce, "", YF("CE = {0} + {1}/6 + ({2}+{3}+{4})/5 + ({5}+{6})/15 = {7}.", N(s.C), N(s.Mn), N(s.Cr), N(s.Mo), N(s.V), N(s.Ni), N(s.Cu), N(ce, 3)));
        }, r =>
        {
            double ce = S(r, 0.25, 0.8, 0.01);
            string right = ce < 0.40 ? X("Boa soldabilidade: em geral dispensa pré-aquecimento (chapas finas)", "Good weldability: generally needs no preheating (thin plate)", "Buena soldabilidad: en general no requiere precalentamiento (chapas finas)") : ce <= 0.60 ? X("Soldabilidade média: pede cuidados (baixo hidrogênio, pré-aquecimento conforme a espessura)", "Medium weldability: needs care (low hydrogen, preheating by thickness)", "Soldabilidad media: requiere cuidados (bajo hidrógeno, precalentamiento según el espesor)") : X("Soldabilidade difícil: pré-aquecimento e tratamento pós-soldagem", "Difficult weldability: preheating and post-weld treatment", "Soldabilidad difícil: precalentamiento y tratamiento postsoldadura");
            string[] all = { X("Boa soldabilidade: em geral dispensa pré-aquecimento (chapas finas)", "Good weldability: generally needs no preheating (thin plate)", "Buena soldabilidad: en general no requiere precalentamiento (chapas finas)"), X("Soldabilidade média: pede cuidados (baixo hidrogênio, pré-aquecimento conforme a espessura)", "Medium weldability: needs care (low hydrogen, preheating by thickness)", "Soldabilidad media: requiere cuidados (bajo hidrógeno, precalentamiento según el espesor)"), X("Soldabilidade difícil: pré-aquecimento e tratamento pós-soldagem", "Difficult weldability: preheating and post-weld treatment", "Soldabilidad difícil: precalentamiento y tratamiento postsoldadura") };
            return Fact(r, XF("Um aço tem CE = {0}. Como se classifica a sua soldabilidade? (CE < 0,40 boa; 0,40 a 0,60 média; > 0,60 difícil)", "A steel has CE = {0}. How is its weldability classified? (CE < 0.40 good; 0.40 to 0.60 medium; > 0.60 difficult)", "Un acero tiene CE = {0}. ¿Cómo se clasifica su soldabilidad? (CE < 0,40 buena; 0,40 a 0,60 media; > 0,60 difícil)", N(ce)), right, X("Quanto maior o CE, maior a temperabilidade, mais martensita na ZTA e maior o risco de trinca a frio.", "The higher the CE, the higher the hardenability, the more martensite in the HAZ and the greater the cold-cracking risk.", "Cuanto mayor el CE, mayor la templabilidad, más martensita en la ZAT y mayor el riesgo de fisura en frío."), all.Where(a => a != right).ToArray());
        }),
        Difficulty.Hard => F(r =>
        {
            var s = Steel(r); double pcm = s.C + s.Si / 30 + (s.Mn + s.Cu + s.Cr) / 20 + s.Ni / 60 + s.Mo / 15 + s.V / 10;
            return Q(XF("Pcm (Ito-Bessyo, sem boro) = C + Si/30 + (Mn+Cu+Cr)/20 + Ni/60 + Mo/15 + V/10. Aço: C {0}, Si {1}, Mn {2}, Cu {3}, Cr {4}, Ni {5}, Mo {6}, V {7} (%). Qual o Pcm?", "Pcm (Ito-Bessyo, no boron) = C + Si/30 + (Mn+Cu+Cr)/20 + Ni/60 + Mo/15 + V/10. Steel: C {0}, Si {1}, Mn {2}, Cu {3}, Cr {4}, Ni {5}, Mo {6}, V {7} (%). What is the Pcm?", "Pcm (Ito-Bessyo, sin boro) = C + Si/30 + (Mn+Cu+Cr)/20 + Ni/60 + Mo/15 + V/10. Acero: C {0}, Si {1}, Mn {2}, Cu {3}, Cr {4}, Ni {5}, Mo {6}, V {7} (%). ¿Cuál es el Pcm?", N(s.C), N(s.Si), N(s.Mn), N(s.Cu), N(s.Cr), N(s.Ni), N(s.Mo), N(s.V)), pcm, "", YF("Pcm = {0} + {1}/30 + ({2}+{3}+{4})/20 + {5}/60 + {6}/15 + {7}/10 = {8}.", N(s.C), N(s.Si), N(s.Mn), N(s.Cu), N(s.Cr), N(s.Ni), N(s.Mo), N(s.V), N(pcm, 4)));
        }, r =>
        {
            double u = S(r, 20, 28, 0.5), i = S(r, 100, 250, 5), q = S(r, 0.8, 2.5, 0.1), v = 0.8 * u * i / (q * 1000);
            return Q(XF("Para limitar o aporte térmico a {0} kJ/mm com U = {1} V e I = {2} A (η = 0,8), qual a velocidade mínima de soldagem em mm/s?", "To limit the heat input to {0} kJ/mm with U = {1} V and I = {2} A (η = 0.8), what is the minimum travel speed in mm/s?", "Para limitar el aporte térmico a {0} kJ/mm con U = {1} V e I = {2} A (η = 0,8), ¿cuál es la velocidad mínima de soldadura en mm/s?", N(q), N(u), N(i, 0)), v, "mm/s", YF("v = η·U·I / (Q·1000) = 0,8 · {0} · {1} / ({2} · 1000) = {3} mm/s.", N(u), N(i, 0), N(q), N(v, 3)));
        }),
        _ => F(r =>
        {
            var a = Steel(r); var b = Steel(r);
            double ca = a.C + a.Mn / 6 + (a.Cr + a.Mo) / 5 + a.Ni / 15, cb = b.C + b.Mn / 6 + (b.Cr + b.Mo) / 5 + b.Ni / 15;
            return Q(XF("Aço A: C {0}, Mn {1}, Cr {2}, Mo {3}, Ni {4}. Aço B: C {5}, Mn {6}, Cr {7}, Mo {8}, Ni {9} (%; V e Cu desprezíveis). Quanto vale CE(A) − CE(B) pela fórmula IIW? (positivo = A mais difícil de soldar)", "Steel A: C {0}, Mn {1}, Cr {2}, Mo {3}, Ni {4}. Steel B: C {5}, Mn {6}, Cr {7}, Mo {8}, Ni {9} (%; V and Cu negligible). What is CE(A) − CE(B) by the IIW formula? (positive = A is harder to weld)", "Acero A: C {0}, Mn {1}, Cr {2}, Mo {3}, Ni {4}. Acero B: C {5}, Mn {6}, Cr {7}, Mo {8}, Ni {9} (%; V y Cu despreciables). ¿Cuánto vale CE(A) − CE(B) por la fórmula IIW? (positivo = A más difícil de soldar)", N(a.C), N(a.Mn), N(a.Cr), N(a.Mo), N(a.Ni), N(b.C), N(b.Mn), N(b.Cr), N(b.Mo), N(b.Ni)), ca - cb, "", YF("CE(A) = {0}; CE(B) = {1}; diferença = {2}.", N(ca, 3), N(cb, 3), N(ca - cb, 3)), 5);
        }, r =>
        {
            double c = S(r, 0.10, 0.30, 0.01), mn = S(r, 0.8, 1.6, 0.05), want = Math.Round((c + mn / 6) * 100 + S(r, 2, 10, 1)) / 100, mnx = 6 * (want - c);
            return Q(XF("Um aço leva só C e Mn. Com C = {0}%, que teor de Mn, em %, daria exatamente CE = {1}? (CE = C + Mn/6)", "A steel has only C and Mn. With C = {0}%, what Mn content, in %, would give exactly CE = {1}? (CE = C + Mn/6)", "Un acero lleva solo C y Mn. Con C = {0}%, ¿qué contenido de Mn, en %, daría exactamente CE = {1}? (CE = C + Mn/6)", N(c), N(want)), mnx, "%", YF("Mn = 6·(CE − C) = 6 · ({0} − {1}) = {2}%.", N(want), N(c), N(mnx, 3)));
        })
    };
}
