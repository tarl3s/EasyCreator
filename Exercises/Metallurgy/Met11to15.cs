namespace EasyCreator.Exercises;

[Exercises("met-11-ttt")]
public sealed class Met11Ttt : PoolGen
{
    static Exercise Extra(Random r)
    {
        double v = S(r, 3, 400, 1), t = 300 / v;
        return Q(XF("A ZTA esfria a {0} °C/s entre 800 e 500 °C. Quanto vale o t8/5, em s?", "The HAZ cools at {0} °C/s between 800 and 500 °C. What is t8/5, in s?", "La ZAT se enfría a {0} °C/s entre 800 y 500 °C. ¿Cuánto vale el t8/5, en s?", N(v, 0)), t, "s", YF("t8/5 = 300 / v = 300 / {0} = {1} s.", N(v, 0), N(t, 3)));
    }

    static Exercise Micro(Random r)
    {
        double rate = Ex.Pick(r, 0.5, 1, 2, 5, 10, 20, 40, 80, 150, 300, 500), t = 300 / rate;
        // Illustrative thresholds for a medium-carbon steel: > 100 C/s martensite, 5-100 bainite, < 5 pearlite+ferrite.
        string right = rate > 100 ? X("Martensita", "Martensite", "Martensita") : rate >= 5 ? X("Bainita", "Bainite", "Bainita") : X("Ferrita + perlita", "Ferrite + pearlite", "Ferrita + perlita");
        string[] all = { X("Martensita", "Martensite", "Martensita"), X("Bainita", "Bainite", "Bainita"), X("Ferrita + perlita", "Ferrite + pearlite", "Ferrita + perlita") };
        return Fact(r, XF("Em um aço de médio carbono (CCT didática: acima de 100 °C/s só martensita; de 5 a 100 °C/s, bainita; abaixo de 5 °C/s, ferrita + perlita), a ZTA esfria de 800 a 500 °C em {0} s. Qual a estrutura principal?", "In a medium-carbon steel (teaching CCT: above 100 °C/s only martensite; 5 to 100 °C/s, bainite; below 5 °C/s, ferrite + pearlite), the HAZ cools from 800 to 500 °C in {0} s. What is the main structure?", "En un acero de medio carbono (CCT didáctica: por encima de 100 °C/s solo martensita; de 5 a 100 °C/s, bainita; por debajo de 5 °C/s, ferrita + perlita), la ZAT se enfría de 800 a 500 °C en {0} s. ¿Cuál es la estructura principal?", N(t, 2)), right,
            XF("Velocidade de resfriamento = 300 °C / {0} s = {1} °C/s.", "Cooling rate = 300 °C / {0} s = {1} °C/s.", "Velocidad de enfriamiento = 300 °C / {0} s = {1} °C/s.", N(t, 2), N(rate, 1)), all.Where(a => a != right).ToArray());
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Extra, r =>
        {
            double t = S(r, 2, 60, 1), v = 300 / t;
            return Q(XF("A ZTA esfria de 800 °C a 500 °C em t8/5 = {0} s. Qual a velocidade média de resfriamento, em °C/s?", "The HAZ cools from 800 °C to 500 °C in t8/5 = {0} s. What is the average cooling rate, in °C/s?", "La ZAT se enfría de 800 °C a 500 °C en t8/5 = {0} s. ¿Cuál es la velocidad media de enfriamiento, en °C/s?", N(t, 0)), v, "°C/s", YF("v = (800 − 500) / t8/5 = 300 / {0} = {1} °C/s.", N(t, 0), N(v, 2)));
        }, Micro),
        Difficulty.Medium => F(r =>
        {
            double c = S(r, 0.1, 0.8, 0.01), mn = S(r, 0.3, 1.5, 0.05), ni = P(r, 0.0, 0.5, 1.0, 2.0), cr = P(r, 0.0, 0.3, 0.8, 1.2), mo = P(r, 0.0, 0.2, 0.3);
            double ms = 539 - 423 * c - 30.4 * mn - 17.7 * ni - 12.1 * cr - 7.5 * mo;
            return Q(XF("Ms (°C) = 539 − 423·C − 30,4·Mn − 17,7·Ni − 12,1·Cr − 7,5·Mo. Aço: C {0}, Mn {1}, Ni {2}, Cr {3}, Mo {4} (%). Qual a temperatura Ms?", "Ms (°C) = 539 − 423·C − 30.4·Mn − 17.7·Ni − 12.1·Cr − 7.5·Mo. Steel: C {0}, Mn {1}, Ni {2}, Cr {3}, Mo {4} (%). What is the Ms temperature?", "Ms (°C) = 539 − 423·C − 30,4·Mn − 17,7·Ni − 12,1·Cr − 7,5·Mo. Acero: C {0}, Mn {1}, Ni {2}, Cr {3}, Mo {4} (%). ¿Cuál es la temperatura Ms?", N(c), N(mn), N(ni), N(cr), N(mo)), ms, "°C", YF("Ms = 539 − {0} − {1} − {2} − {3} − {4} = {5} °C.", N(423 * c, 1), N(30.4 * mn, 1), N(17.7 * ni, 1), N(12.1 * cr, 1), N(7.5 * mo, 1), N(ms, 1)), 1);
        }, Micro),
        Difficulty.Hard => F(r =>
        {
            double ms = S(r, 250, 450, 5), tq = P(r, 20.0, 25, 100, 150), f = (1 - Exp(-0.011 * (ms - tq))) * 100;
            return Q(XF("Koistinen-Marburger: f = 1 − exp(−0,011·(Ms − T)). Um aço com Ms = {0} °C é resfriado até {1} °C. Qual a fração de martensita formada, em %?", "Koistinen-Marburger: f = 1 − exp(−0.011·(Ms − T)). A steel with Ms = {0} °C is cooled to {1} °C. What fraction of martensite forms, in %?", "Koistinen-Marburger: f = 1 − exp(−0,011·(Ms − T)). Un acero con Ms = {0} °C se enfría hasta {1} °C. ¿Qué fracción de martensita se forma, en %?", N(ms, 0), N(tq, 0)), f, "%", YF("f = 1 − exp(−0,011 · ({0} − {1})) = 1 − exp(−{2}) = {3}%.", N(ms, 0), N(tq, 0), N(0.011 * (ms - tq), 3), N(f, 2)));
        }, r =>
        {
            double tn = S(r, 1, 20, 1), t1 = P(r, 750.0, 800), tnose = P(r, 500.0, 550, 600), v = (t1 - tnose) / tn;
            return Q(XF("Em uma curva CCT, o nariz da perlita fica a {0} °C e é atingido em {1} s se o aço esfriar de {2} °C. Qual a velocidade crítica de resfriamento (mínima) para evitar a perlita, em °C/s?", "On a CCT curve, the pearlite nose lies at {0} °C and is reached in {1} s if the steel cools from {2} °C. What is the critical (minimum) cooling rate to avoid pearlite, in °C/s?", "En una curva CCT, la nariz de la perlita está a {0} °C y se alcanza en {1} s si el acero se enfría desde {2} °C. ¿Cuál es la velocidad crítica (mínima) de enfriamiento para evitar la perlita, en °C/s?", N(tnose, 0), N(tn, 0), N(t1, 0)), v, "°C/s", YF("v = ({0} − {1}) / {2} s = {3} °C/s.", N(t1, 0), N(tnose, 0), N(tn, 0), N(v, 2)));
        }),
        _ => F(r =>
        {
            double c = S(r, 0.3, 0.7, 0.01), mn = S(r, 0.5, 1.5, 0.05), tq = P(r, 20.0, 25, 100), ms = 539 - 423 * c - 30.4 * mn, ra = Exp(-0.011 * (ms - tq)) * 100;
            return Q(XF("Aço com C = {0}% e Mn = {1}% (Ms = 539 − 423·C − 30,4·Mn) é temperado até {2} °C. Pela relação de Koistinen-Marburger (f = 1 − exp(−0,011·(Ms − T))), qual a austenita retida, em %?", "A steel with C = {0}% and Mn = {1}% (Ms = 539 − 423·C − 30.4·Mn) is quenched to {2} °C. By the Koistinen-Marburger relation (f = 1 − exp(−0.011·(Ms − T))), what is the retained austenite, in %?", "Un acero con C = {0}% y Mn = {1}% (Ms = 539 − 423·C − 30,4·Mn) se templa hasta {2} °C. Por la relación de Koistinen-Marburger (f = 1 − exp(−0,011·(Ms − T))), ¿cuál es la austenita retenida, en %?", N(c), N(mn), N(tq, 0)), ra, "%", YF("Ms = {0} °C; martensita = {1}%; austenita retida = exp(−0,011·({0} − {2})) = {3}%.", N(ms, 1), N((1 - ra / 100) * 100, 2), N(tq, 0), N(ra, 2)), 3);
        }, r =>
        {
            double ms = S(r, 250, 400, 5), want = P(r, 90.0, 95, 98, 99), tf = ms + Ln(1 - want / 100) / 0.011;
            return Q(XF("Para um aço com Ms = {0} °C, até que temperatura é preciso esfriar para formar {1}% de martensita? (f = 1 − exp(−0,011·(Ms − T)))", "For a steel with Ms = {0} °C, to what temperature must it be cooled to form {1}% martensite? (f = 1 − exp(−0.011·(Ms − T)))", "Para un acero con Ms = {0} °C, ¿hasta qué temperatura hay que enfriar para formar {1}% de martensita? (f = 1 − exp(−0,011·(Ms − T)))", N(ms, 0), N(want, 0)), tf, "°C", YF("T = Ms + ln(1 − f)/0,011 = {0} + ln({1})/0,011 = {2} °C.", N(ms, 0), N(1 - want / 100, 2), N(tf, 1)), 3);
        }, Micro)
    };
}

[Exercises("met-12-temperabilidade")]
public sealed class Met12Hardenability : PoolGen
{
    static Exercise Extra(Random r)
    {
        double mm = S(r, 1.5, 50, 0.5), j = mm / 25.4 * 16;
        return Q(XF("No ensaio Jominy, um ponto está a {0} mm da extremidade temperada. Em qual posição J (1/16 de polegada) ele fica? (1 pol = 25,4 mm)", "In the Jominy test, a point is {0} mm from the quenched end. At which J position (1/16 inch) is it? (1 in = 25.4 mm)", "En el ensayo Jominy, un punto está a {0} mm del extremo templado. ¿En qué posición J (1/16 de pulgada) queda? (1 pulg = 25,4 mm)", N(mm)), j, "J", YF("J = {0}/25,4 · 16 = {1}.", N(mm), N(j, 2)));
    }
    static Exercise Extra2(Random r)
    {
        double di = S(r, 0.5, 4, 0.05), mm = di * 25.4;
        return Q(XF("Um aço tem diâmetro crítico ideal DI = {0} pol. Quanto é isso em mm? (1 pol = 25,4 mm)", "A steel has an ideal critical diameter DI = {0} in. How much is that in mm? (1 in = 25.4 mm)", "Un acero tiene un diámetro crítico ideal DI = {0} pulg. ¿Cuánto es eso en mm? (1 pulg = 25,4 mm)", N(di)), mm, "mm", YF("DI = {0} · 25,4 = {1} mm.", N(di), N(mm, 1)));
    }

    static Exercise Facts(Random r)
    {
        var bank = new (string q, string right, string[] wrong)[]
        {
            (X("O que mede o ensaio Jominy?", "What does the Jominy test measure?", "¿Qué mide el ensayo Jominy?"), X("A temperabilidade: como a dureza cai com a distância da extremidade temperada com água", "Hardenability: how hardness falls with distance from the water-quenched end", "La templabilidad: cómo cae la dureza con la distancia al extremo templado con agua"), new[] { X("A resistência à fadiga", "Fatigue strength", "La resistencia a la fatiga"), X("A tenacidade ao impacto", "Impact toughness", "La tenacidad al impacto"), X("A condutividade elétrica", "Electrical conductivity", "La conductividad eléctrica") }),
            (X("Qual elemento mais determina a dureza MÁXIMA da martensita?", "Which element mostly determines the MAXIMUM hardness of martensite?", "¿Qué elemento determina más la dureza MÁXIMA de la martensita?"), X("O carbono", "Carbon", "El carbono"), new[] { X("O cromo", "Chromium", "El cromo"), X("O níquel", "Nickel", "El níquel"), X("O molibdênio", "Molybdenum", "El molibdeno") }),
            (X("Elementos de liga como Cr, Mo e Mn, em um aço, principalmente:", "Alloying elements such as Cr, Mo and Mn in a steel mainly:", "Los elementos de aleación como Cr, Mo y Mn en un acero principalmente:"), X("deslocam o nariz da curva TTT para a direita e aumentam a profundidade temperável", "shift the TTT nose to the right and increase the hardened depth", "desplazan la nariz de la curva TTT a la derecha y aumentan la profundidad templable"), new[] { X("reduzem a dureza máxima a zero", "reduce the maximum hardness to zero", "reducen la dureza máxima a cero"), X("fazem a peça esfriar mais rápido", "make the part cool faster", "hacen que la pieza se enfríe más rápido"), X("diminuem a temperabilidade", "decrease hardenability", "disminuyen la templabilidad") }),
            (X("Uma peça muito grossa de aço-carbono simples, temperada, fica dura:", "A very thick part of plain carbon steel, once quenched, is hard:", "Una pieza muy gruesa de acero al carbono simple, templada, queda dura:"), X("só na superfície; o núcleo esfria devagar e não vira martensita", "only at the surface; the core cools slowly and does not become martensite", "solo en la superficie; el núcleo se enfría lentamente y no se vuelve martensita"), new[] { X("por igual até o centro", "evenly to the centre", "por igual hasta el centro"), X("só no centro", "only at the centre", "solo en el centro"), X("em nenhum ponto", "nowhere", "en ningún punto") }),
        };
        var b = bank[r.Next(bank.Length)];
        return Fact(r, b.q, b.right, X("Temperabilidade = capacidade de formar martensita em profundidade; o carbono dá a dureza máxima e os elementos de liga dão a profundidade.", "Hardenability = ability to form martensite in depth; carbon gives the maximum hardness and alloying elements give the depth.", "Templabilidad = capacidad de formar martensita en profundidad; el carbono da la dureza máxima y los elementos de aleación dan la profundidad."), b.wrong);
    }

    static double Di(double dib, double mn, double si, double cr, double ni, double mo) => dib * (1 + 3.333 * mn) * (1 + 0.7 * si) * (1 + 0.363 * ni) * (1 + 2.16 * cr) * (1 + 3 * mo);

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Extra, Facts, r =>
        {
            int n = Ex.Int(r, 1, 32);
            double mm = n / 16.0 * 25.4;
            return Q(XF("No ensaio Jominy a posição é dada em 1/16 de polegada a partir da extremidade temperada. Em que distância, em mm, fica a posição J{0}? (1 pol = 25,4 mm)", "In the Jominy test the position is given in 1/16 inch from the quenched end. At what distance, in mm, is position J{0}? (1 in = 25.4 mm)", "En el ensayo Jominy la posición se da en 1/16 de pulgada desde el extremo templado. ¿A qué distancia, en mm, está la posición J{0}? (1 pulg = 25,4 mm)", n), mm, "mm", YF("d = {0}/16 pol · 25,4 = {1} mm.", n, N(mm, 2)));
        }),
        Difficulty.Medium => F(Facts, r =>
        {
            double hb = S(r, 120, 450, 5), sr = 3.45 * hb;
            return Q(XF("Para aços, a resistência à tração é aproximadamente σr ≈ 3,45 × HB (MPa). Qual a estimativa de σr para dureza Brinell {0} HB?", "For steels, the tensile strength is roughly σr ≈ 3.45 × HB (MPa). What is the estimate of σr for a Brinell hardness of {0} HB?", "Para aceros, la resistencia a la tracción es aproximadamente σr ≈ 3,45 × HB (MPa). ¿Cuál es la estimación de σr para una dureza Brinell de {0} HB?", N(hb, 0)), sr, "MPa", YF("σr ≈ 3,45 · {0} = {1} MPa.", N(hb, 0), N(sr, 0)));
        }, r =>
        {
            double sr = S(r, 400, 1500, 10), hb = sr / 3.45;
            return Q(XF("Um aço tem resistência à tração de {0} MPa. Qual a dureza Brinell estimada? (σr ≈ 3,45 × HB)", "A steel has a tensile strength of {0} MPa. What is the estimated Brinell hardness? (σr ≈ 3.45 × HB)", "Un acero tiene resistencia a la tracción de {0} MPa. ¿Cuál es la dureza Brinell estimada? (σr ≈ 3,45 × HB)", N(sr, 0)), hb, "HB", YF("HB ≈ σr / 3,45 = {0} / 3,45 = {1}.", N(sr, 0), N(hb, 1)));
        }),
        Difficulty.Hard => F(Extra2, r =>
        {
            double dib = S(r, 0.15, 0.30, 0.01), mn = S(r, 0.5, 1.5, 0.05), cr = P(r, 0.0, 0.5, 0.8, 1.0), mo = P(r, 0.0, 0.15, 0.25), di = Di(dib, mn, 0, cr, 0, mo);
            return Q(XF("Diâmetro crítico ideal: DI = DI_base × (1 + 3,333·Mn) × (1 + 2,16·Cr) × (1 + 3·Mo) (% de cada elemento). Aço com DI_base = {0} pol, Mn {1}%, Cr {2}%, Mo {3}%. Qual o DI, em polegadas?", "Ideal critical diameter: DI = DI_base × (1 + 3.333·Mn) × (1 + 2.16·Cr) × (1 + 3·Mo) (% of each element). Steel with DI_base = {0} in, Mn {1}%, Cr {2}%, Mo {3}%. What is DI, in inches?", "Diámetro crítico ideal: DI = DI_base × (1 + 3,333·Mn) × (1 + 2,16·Cr) × (1 + 3·Mo) (% de cada elemento). Acero con DI_base = {0} pulg, Mn {1}%, Cr {2}%, Mo {3}%. ¿Cuál es el DI, en pulgadas?", N(dib, 2), N(mn), N(cr), N(mo)), di, X("pol", "in", "pulg"), YF("DI = {0} · {1} · {2} · {3} = {4} pol.", N(dib, 2), N(1 + 3.333 * mn, 3), N(1 + 2.16 * cr, 3), N(1 + 3 * mo, 3), N(di, 3)));
        }, Facts),
        _ => F(r =>
        {
            double dib = S(r, 0.15, 0.30, 0.01), mn = S(r, 0.5, 1.5, 0.05), cr = P(r, 0.0, 0.5, 0.8), di = Di(dib, mn, 0, cr, 0, 0), di2 = Di(dib, mn + 0.5, 0, cr, 0, 0);
            return Q(XF("Com DI = DI_base × (1 + 3,333·Mn) × (1 + 2,16·Cr), DI_base = {0} pol, Mn {1}% e Cr {2}%, quanto aumenta o DI (em %) se o Mn subir 0,5 ponto percentual?", "With DI = DI_base × (1 + 3.333·Mn) × (1 + 2.16·Cr), DI_base = {0} in, Mn {1}% and Cr {2}%, by what percentage does DI rise if Mn goes up by 0.5 percentage points?", "Con DI = DI_base × (1 + 3,333·Mn) × (1 + 2,16·Cr), DI_base = {0} pulg, Mn {1}% y Cr {2}%, ¿cuánto aumenta el DI (en %) si el Mn sube 0,5 puntos porcentuales?", N(dib, 2), N(mn), N(cr)), (di2 / di - 1) * 100, "%", YF("DI₁ = {0}; DI₂ = {1}; aumento = ({1}/{0} − 1)·100 = {2}%.", N(di, 3), N(di2, 3), N((di2 / di - 1) * 100, 2)));
        }, r =>
        {
            double dib = S(r, 0.15, 0.30, 0.01), mn = S(r, 0.5, 1.5, 0.05), cr = P(r, 0.5, 0.8, 1.0), mo = P(r, 0.15, 0.25), di = Math.Round(Di(dib, mn, 0, cr, 0, mo), 2), mnx = ((di / (dib * (1 + 2.16 * cr) * (1 + 3 * mo))) - 1) / 3.333;
            return Q(XF("Um aço tem DI_base = {0} pol, Cr {1}% e Mo {2}%. Que teor de Mn (%) leva o DI = DI_base × (1 + 3,333·Mn) × (1 + 2,16·Cr) × (1 + 3·Mo) a {3} pol?", "A steel has DI_base = {0} in, Cr {1}% and Mo {2}%. What Mn content (%) brings DI = DI_base × (1 + 3.333·Mn) × (1 + 2.16·Cr) × (1 + 3·Mo) to {3} in?", "Un acero tiene DI_base = {0} pulg, Cr {1}% y Mo {2}%. ¿Qué contenido de Mn (%) lleva el DI = DI_base × (1 + 3,333·Mn) × (1 + 2,16·Cr) × (1 + 3·Mo) a {3} pulg?", N(dib, 2), N(cr), N(mo), N(di, 2)), mnx, "%", YF("(1 + 3,333·Mn) = DI / (DI_base·(1 + 2,16·Cr)·(1 + 3·Mo)) = {0}; Mn = ({0} − 1)/3,333 = {1}%.", N(di / (dib * (1 + 2.16 * cr) * (1 + 3 * mo)), 4), N(mnx, 3)), 4);
        })
    };
}

[Exercises("met-13-simbologia")]
public sealed class Met13Symbols : PoolGen
{
    static Exercise Symbol(Random r)
    {
        var bank = new (string q, string right, string[] wrong)[]
        {
            (X("Na simbologia AWS, o símbolo de solda (ex.: triângulo de filete) colocado ABAIXO da linha de referência indica a solda:", "In AWS symbology, the weld symbol (e.g. the fillet triangle) placed BELOW the reference line indicates the weld:", "En la simbología AWS, el símbolo de soldadura (p. ej., el triángulo de filete) colocado DEBAJO de la línea de referencia indica la soldadura:"), X("no lado da seta", "on the arrow side", "en el lado de la flecha"), new[] { X("no lado oposto ao da seta", "on the other side", "en el lado opuesto al de la flecha"), X("nos dois lados", "on both sides", "en ambos lados"), X("em campo", "in the field", "en campo") }),
            (X("Na simbologia AWS, o símbolo ACIMA da linha de referência indica a solda:", "In AWS symbology, the symbol ABOVE the reference line indicates the weld:", "En la simbología AWS, el símbolo ENCIMA de la línea de referencia indica la soldadura:"), X("no lado oposto ao da seta", "on the other side", "en el lado opuesto al de la flecha"), new[] { X("no lado da seta", "on the arrow side", "en el lado de la flecha"), X("em todo o contorno", "all around", "en todo el contorno"), X("de fábrica", "in the shop", "de taller") }),
            (X("Um círculo na junção da seta com a linha de referência significa:", "A circle at the junction of the arrow and the reference line means:", "Un círculo en la unión de la flecha con la línea de referencia significa:"), X("solda em todo o contorno (all around)", "weld all around", "soldadura en todo el contorno (all around)"), new[] { X("solda de campo", "field weld", "soldadura de campo"), X("solda intermitente", "intermittent weld", "soldadura intermitente"), X("solda sem metal de adição", "weld without filler metal", "soldadura sin metal de aporte") }),
            (X("Uma bandeira no símbolo de solda indica:", "A flag on the weld symbol indicates:", "Una bandera en el símbolo de soldadura indica:"), X("solda de campo (feita na obra/montagem)", "a field weld (made on site)", "soldadura de campo (hecha en obra/montaje)"), new[] { X("solda em todo o contorno", "weld all around", "soldadura en todo el contorno"), X("solda de fábrica", "a shop weld", "soldadura de taller"), X("solda com acabamento esmerilhado", "a ground finish", "soldadura con acabado esmerilado") }),
            (X("A cauda (fork) do símbolo de soldagem serve para indicar:", "The tail (fork) of the welding symbol is used to indicate:", "La cola del símbolo de soldadura sirve para indicar:"), X("o processo, a especificação (EPS) ou notas", "the process, the specification (WPS) or notes", "el proceso, la especificación (EPS) o notas"), new[] { X("o diâmetro do eletrodo apenas", "the electrode diameter only", "solo el diámetro del electrodo"), X("a posição da seta", "the arrow position", "la posición de la flecha"), X("o comprimento do cordão", "the bead length", "la longitud del cordón") }),
            (X("Qual dos símbolos representa uma solda de filete?", "Which symbol stands for a fillet weld?", "¿Cuál de los símbolos representa una soldadura de filete?"), X("Triângulo", "Triangle", "Triángulo"), new[] { X("Semicírculo (tampão)", "Half circle (plug)", "Semicírculo (tapón)"), X("V aberto", "Open V", "V abierto"), X("Quadrado", "Square", "Cuadrado") }),
        };
        var b = bank[r.Next(bank.Length)];
        return Fact(r, b.q, b.right, X("Resumo AWS: símbolo abaixo da linha = lado da seta; acima = lado oposto; círculo = contorno; bandeira = campo; cauda = processo/especificação.", "AWS summary: symbol below the line = arrow side; above = other side; circle = all around; flag = field; tail = process/specification.", "Resumen AWS: símbolo debajo de la línea = lado de la flecha; encima = lado opuesto; círculo = contorno; bandera = campo; cola = proceso/especificación."), b.wrong);
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Symbol, r =>
        {
            double z = S(r, 3, 20, 0.1), a = 0.707 * z;
            return Q(XF("O símbolo indica um filete de 'tamanho' (perna) {0} mm. Qual a garganta teórica a = 0,707·z?", "The symbol shows a fillet of 'size' (leg) {0} mm. What is the theoretical throat a = 0.707·z?", "El símbolo indica un filete de 'tamaño' (cateto) {0} mm. ¿Cuál es la garganta teórica a = 0,707·z?", N(z, 1)), a, "mm", YF("a = 0,707 · {0} = {1} mm.", N(z, 1), N(a, 2)));
        }),
        Difficulty.Medium => F(Symbol, r =>
        {
            double z = S(r, 3, 16, 0.5), L = Ex.Int(r, 20, 150), p = L + Ex.Int(r, 30, 150), frac = L / p * 100;
            return Q(XF("Um filete intermitente é especificado '{0} ◣ {1}-{2}' (perna, comprimento – passo, em mm). Que porcentagem do comprimento da junta é efetivamente soldada?", "An intermittent fillet is specified '{0} ◣ {1}-{2}' (leg, length – pitch, in mm). What percentage of the joint length is actually welded?", "Un filete intermitente se especifica '{0} ◣ {1}-{2}' (cateto, longitud – paso, en mm). ¿Qué porcentaje de la longitud de la junta se suelda realmente?", N(z, 1), N(L, 0), N(p, 0)), frac, "%", YF("Fração = comprimento / passo = {0}/{1} = {2}%.", N(L, 0), N(p, 0), N(frac, 2)));
        }),
        Difficulty.Hard => F(r =>
        {
            double L = P(r, 50.0, 60, 75, 100), p = L + P(r, 50.0, 75, 100, 150); int n = Ex.Int(r, 4, 20); double total = (n - 1) * p + L;
            return Q(XF("Um filete intermitente '{0}-{1}' (comprimento – passo, em mm, medido de centro a centro) tem {2} segmentos. Qual o comprimento total da junta ocupada, do início do 1º ao fim do último segmento, em mm?", "An intermittent fillet '{0}-{1}' (length – pitch, in mm, measured centre to centre) has {2} segments. What is the total joint length covered, from the start of the 1st to the end of the last segment, in mm?", "Un filete intermitente '{0}-{1}' (longitud – paso, en mm, medido de centro a centro) tiene {2} segmentos. ¿Cuál es la longitud total de junta ocupada, del inicio del 1.º al final del último segmento, en mm?", N(L, 0), N(p, 0), n), total, "mm", YF("({0} − 1) passos de {1} mm + o último segmento de {2} mm = {3} mm.", n, N(p, 0), N(L, 0), N(total, 0)));
        }, r =>
        {
            double z = S(r, 4, 14, 0.5), len = S(r, 0.5, 10, 0.5), kg = z * z / 2 * len * 1000 * 7.85e-6;
            return Q(XF("Quantos kg de metal de solda tem um filete de perna {0} mm em {1} m de junta? (seção = z²/2; aço 7,85 g/cm³; um lado só)", "How many kg of weld metal are in a fillet of leg {0} mm over {1} m of joint? (section = z²/2; steel 7.85 g/cm³; one side only)", "¿Cuántos kg de metal de soldadura tiene un filete de cateto {0} mm en {1} m de junta? (sección = z²/2; acero 7,85 g/cm³; un solo lado)", N(z, 1), N(len)), kg, "kg", YF("A = {0}²/2 = {1} mm²; V = {1} · {2} mm = {3} mm³; massa = {3} · 7,85·10⁻⁶ = {4} kg.", N(z, 1), N(z * z / 2, 2), N(len * 1000, 0), N(z * z / 2 * len * 1000, 0), N(kg, 4)));
        }),
        _ => F(r =>
        {
            double L = P(r, 60.0, 75, 100), p = L + P(r, 40.0, 60, 100), z = S(r, 5, 12, 0.5), plate = S(r, 600, 3000, 100), n = Math.Floor((plate - L) / p) + 1, len = n * L * 2;
            return Q(XF("Dois filetes intermitentes '{0} ◣ {1}-{2}' (um em cada lado da chapa; perna, comprimento – passo em mm) numa junta de {3} mm. Quantos mm de cordão ao todo (os dois lados)? Considere segmentos inteiros começando na borda.", "Two intermittent fillets '{0} ◣ {1}-{2}' (one on each side of the plate; leg, length – pitch in mm) on a {3} mm joint. How many mm of bead in total (both sides)? Assume whole segments starting at the edge.", "Dos filetes intermitentes '{0} ◣ {1}-{2}' (uno a cada lado de la chapa; cateto, longitud – paso en mm) en una junta de {3} mm. ¿Cuántos mm de cordón en total (los dos lados)? Considere segmentos enteros empezando en el borde.", N(z, 1), N(L, 0), N(p, 0), N(plate, 0)), len, "mm", YF("Segmentos por lado = ⌊({0} − {1})/{2}⌋ + 1 = {3}; total = 2 · {3} · {1} = {4} mm.", N(plate, 0), N(L, 0), N(p, 0), N(n, 0), N(len, 0)));
        }, Symbol)
    };
}

[Exercises("met-14-zta")]
public sealed class Met14Haz : PoolGen
{
    static Exercise Extra(Random r)
    {
        double v = S(r, 3, 100, 1), t = 300 / v;
        return Q(XF("A ZTA esfria a {0} °C/s entre 800 e 500 °C. Quanto vale o t8/5, em s?", "The HAZ cools at {0} °C/s between 800 and 500 °C. What is t8/5, in s?", "La ZAT se enfría a {0} °C/s entre 800 y 500 °C. ¿Cuánto vale el t8/5, en s?", N(v, 0)), t, "s", YF("t8/5 = 300 / v = 300 / {0} = {1} s.", N(v, 0), N(t, 3)));
    }

    static double T85(double q, double t0, double f3) => (6700 - 5 * t0) * q * (1 / (500 - t0) - 1 / (800 - t0)) * f3;
    static double Duren(double c, double ce, double t) => 2019 * (c * (1 - 0.5 * Log(t)) + 0.3 * (ce - c)) + 66 * (1 - 0.8 * Log(t));

    static Exercise Facts(Random r)
    {
        var bank = new (string q, string right, string[] wrong)[]
        {
            (X("O que acontece com o t8/5 quando se aumenta o pré-aquecimento?", "What happens to t8/5 when the preheat is increased?", "¿Qué pasa con el t8/5 cuando se aumenta el precalentamiento?"), X("Aumenta (a ZTA esfria mais devagar)", "It increases (the HAZ cools more slowly)", "Aumenta (la ZAT se enfría más despacio)"), new[] { X("Diminui", "It decreases", "Disminuye"), X("Não muda", "It does not change", "No cambia"), X("Fica negativo", "It becomes negative", "Se vuelve negativo") }),
            (X("O que acontece com o t8/5 quando se aumenta o aporte térmico?", "What happens to t8/5 when the heat input is increased?", "¿Qué pasa con el t8/5 cuando se aumenta el aporte térmico?"), X("Aumenta", "It increases", "Aumenta"), new[] { X("Diminui", "It decreases", "Disminuye"), X("Não muda", "It does not change", "No cambia"), X("Zera", "It drops to zero", "Se anula") }),
            (X("Um t8/5 muito curto na ZTA de um aço de alto CE leva a:", "A very short t8/5 in the HAZ of a high-CE steel leads to:", "Un t8/5 muy corto en la ZAT de un acero de alto CE lleva a:"), X("martensita dura e frágil, com risco de trinca a frio", "hard, brittle martensite with a cold-cracking risk", "martensita dura y frágil, con riesgo de fisura en frío"), new[] { X("ferrita macia e dúctil", "soft, ductile ferrite", "ferrita blanda y dúctil"), X("grão grosseiro e mole", "coarse, soft grains", "grano grueso y blando"), X("nenhuma mudança", "no change", "ningún cambio") }),
            (X("Uma chapa mais grossa, para o mesmo aporte térmico, resfria a ZTA:", "A thicker plate, for the same heat input, cools the HAZ:", "Una chapa más gruesa, para el mismo aporte térmico, enfría la ZAT:"), X("mais rápido (menor t8/5)", "faster (lower t8/5)", "más rápido (menor t8/5)"), new[] { X("mais devagar (maior t8/5)", "more slowly (higher t8/5)", "más despacio (mayor t8/5)"), X("igual", "the same", "igual"), X("só se for inox", "only if stainless", "solo si es inox") }),
        };
        var b = bank[r.Next(bank.Length)];
        return Fact(r, b.q, b.right, X("t8/5 cresce com o aporte térmico e o pré-aquecimento e diminui com a espessura (mais fluxo de calor para a chapa).", "t8/5 grows with heat input and preheat and falls with thickness (more heat flows into the plate).", "El t8/5 crece con el aporte térmico y el precalentamiento y disminuye con el espesor (más flujo de calor hacia la chapa)."), b.wrong);
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Facts, r =>
        {
            double u = S(r, 20, 28, 0.5), i = S(r, 120, 260, 5), v = S(r, 3, 8, 0.2), q = 0.8 * u * i / v / 1000;
            return Q(XF("MIG/MAG a {0} V, {1} A, {2} mm/s (η = 0,8): qual o aporte térmico Q, em kJ/mm?", "MIG/MAG at {0} V, {1} A, {2} mm/s (η = 0.8): what is the heat input Q, in kJ/mm?", "MIG/MAG a {0} V, {1} A, {2} mm/s (η = 0,8): ¿cuál es el aporte térmico Q, en kJ/mm?", N(u), N(i, 0), N(v)), q, "kJ/mm", YF("Q = 0,8 · {0} · {1} / {2} / 1000 = {3} kJ/mm.", N(u), N(i, 0), N(v), N(q, 3)));
        }),
        Difficulty.Medium => F(Extra, r =>
        {
            double q = S(r, 0.5, 2.5, 0.1), t0 = P(r, 20.0, 50, 100, 150, 200), t = T85(q, t0, 1);
            return Q(XF("Chapa grossa (fluxo 3D, F3 = 1): t8/5 = (6700 − 5·T0)·Q·[1/(500 − T0) − 1/(800 − T0)]. Para Q = {0} kJ/mm e T0 = {1} °C, qual o t8/5, em s?", "Thick plate (3D flow, F3 = 1): t8/5 = (6700 − 5·T0)·Q·[1/(500 − T0) − 1/(800 − T0)]. For Q = {0} kJ/mm and T0 = {1} °C, what is t8/5, in s?", "Chapa gruesa (flujo 3D, F3 = 1): t8/5 = (6700 − 5·T0)·Q·[1/(500 − T0) − 1/(800 − T0)]. Para Q = {0} kJ/mm y T0 = {1} °C, ¿cuál es el t8/5, en s?", N(q), N(t0, 0)), t, "s", YF("t8/5 = (6700 − {0}) · {1} · [1/{2} − 1/{3}] = {4} s.", N(5 * t0, 0), N(q), N(500 - t0, 0), N(800 - t0, 0), N(t, 3)));
        }, Facts),
        Difficulty.Hard => F(r =>
        {
            double c = S(r, 0.10, 0.30, 0.01), ce = c + S(r, 0.15, 0.30, 0.01), t = S(r, 3, 30, 1), hv = Duren(c, ce, t);
            return Q(XF("Fórmula de Düren: HV = 2019·[C·(1 − 0,5·log t8/5) + 0,3·(CE − C)] + 66·(1 − 0,8·log t8/5). Aço com C = {0}%, CE = {1} e t8/5 = {2} s. Qual a dureza máxima prevista na ZTA?", "Düren formula: HV = 2019·[C·(1 − 0.5·log t8/5) + 0.3·(CE − C)] + 66·(1 − 0.8·log t8/5). Steel with C = {0}%, CE = {1} and t8/5 = {2} s. What is the predicted maximum HAZ hardness?", "Fórmula de Düren: HV = 2019·[C·(1 − 0,5·log t8/5) + 0,3·(CE − C)] + 66·(1 − 0,8·log t8/5). Acero con C = {0}%, CE = {1} y t8/5 = {2} s. ¿Cuál es la dureza máxima prevista en la ZAT?", N(c), N(ce), N(t, 0)), hv, "HV", YF("log {0} = {1}; HV = 2019·[{2}·(1 − {3}) + 0,3·({4} − {2})] + 66·(1 − {5}) = {6}.", N(t, 0), N(Log(t), 4), N(c), N(0.5 * Log(t), 4), N(ce), N(0.8 * Log(t), 4), N(hv, 1)));
        }, r =>
        {
            double t0 = P(r, 20.0, 50, 100), tw = S(r, 6, 20, 1), q = tw / ((6700 - 5 * t0) * (1 / (500 - t0) - 1 / (800 - t0)));
            return Q(XF("Chapa grossa (3D, F3 = 1), T0 = {0} °C. Que aporte térmico Q (kJ/mm) dá t8/5 = {1} s? (t8/5 = (6700 − 5·T0)·Q·[1/(500 − T0) − 1/(800 − T0)])", "Thick plate (3D, F3 = 1), T0 = {0} °C. What heat input Q (kJ/mm) gives t8/5 = {1} s? (t8/5 = (6700 − 5·T0)·Q·[1/(500 − T0) − 1/(800 − T0)])", "Chapa gruesa (3D, F3 = 1), T0 = {0} °C. ¿Qué aporte térmico Q (kJ/mm) da t8/5 = {1} s? (t8/5 = (6700 − 5·T0)·Q·[1/(500 − T0) − 1/(800 − T0)])", N(t0, 0), N(tw, 0)), q, "kJ/mm", YF("Q = t8/5 / [(6700 − 5·T0)·(1/(500 − T0) − 1/(800 − T0))] = {0} / {1} = {2} kJ/mm.", N(tw, 0), N((6700 - 5 * t0) * (1 / (500 - t0) - 1 / (800 - t0)), 4), N(q, 3)));
        }),
        _ => F(r =>
        {
            double u = S(r, 22, 28, 0.5), i = S(r, 150, 260, 10), v = S(r, 3, 7, 0.2), t0 = P(r, 20.0, 100, 150), q = 0.8 * u * i / v / 1000, t = T85(q, t0, 1);
            return Q(XF("MIG/MAG em chapa grossa: {0} V, {1} A, {2} mm/s (η = 0,8), pré-aquecimento {3} °C, F3 = 1. Calcule o t8/5 (fórmula 3D da EN 1011-2), em s.", "MIG/MAG on thick plate: {0} V, {1} A, {2} mm/s (η = 0.8), preheat {3} °C, F3 = 1. Calculate t8/5 (EN 1011-2 3D formula), in s.", "MIG/MAG en chapa gruesa: {0} V, {1} A, {2} mm/s (η = 0,8), precalentamiento {3} °C, F3 = 1. Calcule el t8/5 (fórmula 3D de la EN 1011-2), en s.", N(u), N(i, 0), N(v), N(t0, 0)), t, "s", YF("Q = {0} kJ/mm; t8/5 = (6700 − {1}) · {0} · [1/{2} − 1/{3}] = {4} s.", N(q, 3), N(5 * t0, 0), N(500 - t0, 0), N(800 - t0, 0), N(t, 2)));
        }, r =>
        {
            double c = S(r, 0.12, 0.25, 0.01), ce = c + S(r, 0.15, 0.30, 0.01), q = S(r, 0.8, 2.0, 0.1), t0 = P(r, 20.0, 100, 150), t = T85(q, t0, 1), hv = Duren(c, ce, t);
            return Q(XF("Chapa grossa com C = {0}% e CE = {1}, soldada com Q = {2} kJ/mm e pré-aquecimento {3} °C (F3 = 1). Calcule o t8/5 (3D) e a dureza máxima da ZTA pela fórmula de Düren (HV = 2019·[C·(1 − 0,5·log t) + 0,3·(CE − C)] + 66·(1 − 0,8·log t)). Responda a DUREZA, em HV.", "Thick plate with C = {0}% and CE = {1}, welded with Q = {2} kJ/mm and preheat {3} °C (F3 = 1). Calculate t8/5 (3D) and the maximum HAZ hardness with the Düren formula (HV = 2019·[C·(1 − 0.5·log t) + 0.3·(CE − C)] + 66·(1 − 0.8·log t)). Answer the HARDNESS, in HV.", "Chapa gruesa con C = {0}% y CE = {1}, soldada con Q = {2} kJ/mm y precalentamiento {3} °C (F3 = 1). Calcule el t8/5 (3D) y la dureza máxima de la ZAT con la fórmula de Düren (HV = 2019·[C·(1 − 0,5·log t) + 0,3·(CE − C)] + 66·(1 − 0,8·log t)). Responda la DUREZA, en HV.", N(c), N(ce), N(q), N(t0, 0)), hv, "HV", YF("t8/5 = (6700 − {0}) · {1} · [1/{2} − 1/{3}] = {4} s; HV = {5}.", N(5 * t0, 0), N(q), N(500 - t0, 0), N(800 - t0, 0), N(t, 3), N(hv, 1)), 3);
        })
    };
}

[Exercises("met-15-inox")]
public sealed class Met15Stainless : PoolGen
{
    static Exercise Extra(Random r)
    {
        double ni = S(r, 6, 14, 0.5), c = S(r, 0.02, 0.08, 0.01), mn = S(r, 1, 2, 0.1), nie = ni + 30 * c + 0.5 * mn;
        return Q(XF("Ni eq = Ni + 30·C + 0,5·Mn. Para Ni = {0}%, C = {1}% e Mn = {2}%, qual o Ni equivalente?", "Ni eq = Ni + 30·C + 0.5·Mn. For Ni = {0}%, C = {1}% and Mn = {2}%, what is the equivalent Ni?", "Ni eq = Ni + 30·C + 0,5·Mn. Para Ni = {0}%, C = {1}% y Mn = {2}%, ¿cuál es el Ni equivalente?", N(ni), N(c, 2), N(mn)), nie, "%", YF("Ni eq = {0} + 30·{1} + 0,5·{2} = {3}%.", N(ni), N(c, 2), N(mn), N(nie, 3)));
    }

    static Exercise Facts(Random r)
    {
        var bank = new (string q, string right, string[] wrong)[]
        {
            (X("Em que faixa de temperatura ocorre a sensitização dos inox austeníticos (precipitação de carbonetos de cromo)?", "In what temperature range does sensitisation of austenitic stainless steels (chromium carbide precipitation) occur?", "¿En qué rango de temperatura ocurre la sensibilización de los inox austeníticos (precipitación de carburos de cromo)?"), "450-850 °C", new[] { "100-200 °C", "900-1200 °C", "1300-1500 °C" }),
            (X("Qual solução evita a corrosão intergranular na soldagem do 304?", "Which solution avoids intergranular corrosion when welding 304?", "¿Qué solución evita la corrosión intergranular al soldar el 304?"), X("Usar 304L ou 321/347 (baixo C ou estabilizados) e baixo aporte", "Use 304L or 321/347 (low C or stabilised) and low heat input", "Usar 304L o 321/347 (bajo C o estabilizados) y bajo aporte"), new[] { X("Resfriar bem devagar a 600 °C", "Cool very slowly at 600 °C", "Enfriar muy lentamente a 600 °C"), X("Usar mais aporte térmico", "Use more heat input", "Usar más aporte térmico"), X("Soldar com CO₂ puro", "Weld with pure CO₂", "Soldar con CO₂ puro") }),
            (X("Qual a causa da sensitização?", "What causes sensitisation?", "¿Cuál es la causa de la sensibilización?"), X("O cromo se combina com o carbono nos contornos de grão, empobrecendo a região em Cr", "Chromium combines with carbon at the grain boundaries, depleting the region of Cr", "El cromo se combina con el carbono en los bordes de grano, empobreciendo la zona en Cr"), new[] { X("O níquel evapora na solda", "Nickel evaporates in the weld", "El níquel se evapora en la soldadura"), X("O oxigênio dissolve o ferro", "Oxygen dissolves the iron", "El oxígeno disuelve el hierro"), X("O hidrogênio gera ferrita", "Hydrogen produces ferrite", "El hidrógeno genera ferrita") }),
            (X("Qual inox é martensítico e temperável?", "Which stainless steel is martensitic and hardenable?", "¿Qué inox es martensítico y templable?"), "410", new[] { "304", "430", "316" }),
            (X("Qual inox é ferrítico (CCC, magnético, não temperável)?", "Which stainless steel is ferritic (BCC, magnetic, not hardenable)?", "¿Qué inox es ferrítico (CCC, magnético, no templable)?"), "430", new[] { "304", "410", "316" }),
            (X("Qual inox é austenítico com adição de molibdênio, mais resistente a cloretos?", "Which austenitic stainless steel has molybdenum added and resists chlorides better?", "¿Qué inox austenítico lleva molibdeno y resiste mejor los cloruros?"), "316", new[] { "304", "410", "430" }),
        };
        var b = bank[r.Next(bank.Length)];
        return Fact(r, b.q, b.right, X("Austeníticos (304, 316): CFC, não magnéticos, soldáveis; ferríticos (430): CCC; martensíticos (410): temperáveis. Sensitização entre 450 e 850 °C: use baixo C (L), estabilizados (321/347) e baixo aporte.", "Austenitic (304, 316): FCC, non-magnetic, weldable; ferritic (430): BCC; martensitic (410): hardenable. Sensitisation between 450 and 850 °C: use low C (L), stabilised (321/347) and low heat input.", "Austeníticos (304, 316): CFC, no magnéticos, soldables; ferríticos (430): CCC; martensíticos (410): templables. Sensibilización entre 450 y 850 °C: use bajo C (L), estabilizados (321/347) y bajo aporte."), b.wrong);
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Extra, Facts, Facts, r =>
        {
            double cr = S(r, 16, 26, 0.5), mo = P(r, 0.0, 2.0, 2.5, 3.0, 4.0), n = S(r, 0.05, 0.3, 0.01), pren = cr + 3.3 * mo + 16 * n;
            return Q(XF("PREN = Cr + 3,3·Mo + 16·N mede a resistência à corrosão por pites. Para Cr = {0}%, Mo = {1}% e N = {2}%, qual o PREN?", "PREN = Cr + 3.3·Mo + 16·N measures pitting corrosion resistance. For Cr = {0}%, Mo = {1}% and N = {2}%, what is the PREN?", "PREN = Cr + 3,3·Mo + 16·N mide la resistencia a la corrosión por picaduras. Para Cr = {0}%, Mo = {1}% y N = {2}%, ¿cuál es el PREN?", N(cr), N(mo), N(n)), pren, "", YF("PREN = {0} + 3,3·{1} + 16·{2} = {3}.", N(cr), N(mo), N(n), N(pren, 2)));
        }),
        Difficulty.Medium => F(Facts, r =>
        {
            double cr = S(r, 17, 26, 0.5), mo = P(r, 0.0, 2.0, 2.5), si = S(r, 0.3, 1.0, 0.05), nb = P(r, 0.0, 0.5, 0.8), cre = cr + mo + 1.5 * si + 0.5 * nb;
            return Q(XF("Cr eq = Cr + Mo + 1,5·Si + 0,5·Nb. Para Cr = {0}%, Mo = {1}%, Si = {2}% e Nb = {3}%, qual o Cr equivalente?", "Cr eq = Cr + Mo + 1.5·Si + 0.5·Nb. For Cr = {0}%, Mo = {1}%, Si = {2}% and Nb = {3}%, what is the equivalent Cr?", "Cr eq = Cr + Mo + 1,5·Si + 0,5·Nb. Para Cr = {0}%, Mo = {1}%, Si = {2}% y Nb = {3}%, ¿cuál es el Cr equivalente?", N(cr), N(mo), N(si), N(nb)), cre, "%", YF("Cr eq = {0} + {1} + 1,5·{2} + 0,5·{3} = {4}%.", N(cr), N(mo), N(si), N(nb), N(cre, 3)));
        }, r =>
        {
            double ni = S(r, 6, 14, 0.5), c = S(r, 0.02, 0.08, 0.01), mn = S(r, 1, 2, 0.1), nie = ni + 30 * c + 0.5 * mn;
            return Q(XF("Ni eq = Ni + 30·C + 0,5·Mn. Para Ni = {0}%, C = {1}% e Mn = {2}%, qual o Ni equivalente?", "Ni eq = Ni + 30·C + 0.5·Mn. For Ni = {0}%, C = {1}% and Mn = {2}%, what is the equivalent Ni?", "Ni eq = Ni + 30·C + 0,5·Mn. Para Ni = {0}%, C = {1}% y Mn = {2}%, ¿cuál es el Ni equivalente?", N(ni), N(c, 2), N(mn)), nie, "%", YF("Ni eq = {0} + 30·{1} + 0,5·{2} = {3}%.", N(ni), N(c, 2), N(mn), N(nie, 3)));
        }),
        Difficulty.Hard => F(r =>
        {
            double cr = S(r, 17, 24, 0.5), si = S(r, 0.3, 0.9, 0.05), ni = S(r, 7, 13, 0.5), c = S(r, 0.03, 0.08, 0.01), mn = S(r, 1, 2, 0.1);
            double cre = cr + 1.5 * si, nie = ni + 30 * c + 0.5 * mn;
            return Q(XF("Metal de solda: Cr {0}%, Si {1}%, Ni {2}%, C {3}%, Mn {4}% (sem Mo nem Nb). Com Cr eq = Cr + 1,5·Si e Ni eq = Ni + 30·C + 0,5·Mn, qual a razão Cr eq / Ni eq?", "Weld metal: Cr {0}%, Si {1}%, Ni {2}%, C {3}%, Mn {4}% (no Mo or Nb). With Cr eq = Cr + 1.5·Si and Ni eq = Ni + 30·C + 0.5·Mn, what is the Cr eq / Ni eq ratio?", "Metal de soldadura: Cr {0}%, Si {1}%, Ni {2}%, C {3}%, Mn {4}% (sin Mo ni Nb). Con Cr eq = Cr + 1,5·Si y Ni eq = Ni + 30·C + 0,5·Mn, ¿cuál es la razón Cr eq / Ni eq?", N(cr), N(si), N(ni), N(c, 2), N(mn)), cre / nie, "", YF("Cr eq = {0}; Ni eq = {1}; razão = {2}.", N(cre, 3), N(nie, 3), N(cre / nie, 3)));
        }, r =>
        {
            double cr = S(r, 17, 26, 0.5), ni = S(r, 6, 14, 0.5), ratio = cr / ni;
            string a = X("Austenítico puro (risco de trinca a quente)", "Fully austenitic (hot-crack risk)", "Austenítico puro (riesgo de fisura en caliente)"), b = X("Austenita com pouca ferrita (zona boa)", "Austenite with a little ferrite (good zone)", "Austenita con poca ferrita (zona buena)"), c = X("Muita ferrita (fase sigma/fragilização possível)", "A lot of ferrite (sigma phase/embrittlement possible)", "Mucha ferrita (posible fase sigma/fragilización)");
            string right = ratio < 1.25 ? a : ratio <= 1.95 ? b : c;
            return Fact(r, XF("Regra didática: Cr eq / Ni eq < 1,25 → austenita pura (risco de trinca a quente); de 1,25 a 1,95 → austenita com 3-8% de ferrita (boa); > 1,95 → muita ferrita. Para Cr eq = {0} e Ni eq = {1}, o que se espera?", "Teaching rule: Cr eq / Ni eq < 1.25 → fully austenitic (hot-crack risk); 1.25 to 1.95 → austenite with 3-8% ferrite (good); > 1.95 → a lot of ferrite. For Cr eq = {0} and Ni eq = {1}, what is expected?", "Regla didáctica: Cr eq / Ni eq < 1,25 → austenita pura (riesgo de fisura en caliente); de 1,25 a 1,95 → austenita con 3-8% de ferrita (buena); > 1,95 → mucha ferrita. Para Cr eq = {0} y Ni eq = {1}, ¿qué se espera?", N(cr), N(ni)), right, YF("Razão = {0}/{1} = {2}.", N(cr), N(ni), N(ratio, 3)), new[] { a, b, c }.Where(x => x != right).ToArray());
        }),
        _ => F(r =>
        {
            double cr = S(r, 17, 26, 0.5), mo = P(r, 0.0, 2.0, 3.0), n = S(r, 0.05, 0.3, 0.01), pren = cr + 3.3 * mo + 16 * n, want = Math.Round(pren + S(r, 1, 6, 1)), crx = want - 3.3 * mo - 16 * n;
            return Q(XF("Um inox com Mo = {0}% e N = {1}% deve alcançar PREN = {2}. Que teor de Cr (%) é necessário? (PREN = Cr + 3,3·Mo + 16·N)", "A stainless steel with Mo = {0}% and N = {1}% must reach PREN = {2}. What Cr content (%) is needed? (PREN = Cr + 3.3·Mo + 16·N)", "Un inox con Mo = {0}% y N = {1}% debe alcanzar un PREN = {2}. ¿Qué contenido de Cr (%) se necesita? (PREN = Cr + 3,3·Mo + 16·N)", N(mo), N(n), N(want, 0)), crx, "%", YF("Cr = PREN − 3,3·Mo − 16·N = {0} − {1} − {2} = {3}%.", N(want, 0), N(3.3 * mo, 2), N(16 * n, 2), N(crx, 2)));
        }, r =>
        {
            double cr = S(r, 17, 24, 0.5), si = S(r, 0.3, 0.9, 0.05), ni = S(r, 8, 13, 0.5), c = S(r, 0.03, 0.08, 0.01), mn = S(r, 1, 2, 0.1), nie = ni + 30 * c + 0.5 * mn, cre = cr + 1.5 * si, ni2 = cre / 1.5 - 30 * c - 0.5 * mn;
            return Q(XF("Com Cr eq = Cr + 1,5·Si = {0} e a razão desejada Cr eq / Ni eq = 1,5, que teor de Ni (%) deve ter o metal de solda, sendo C = {1}% e Mn = {2}%? (Ni eq = Ni + 30·C + 0,5·Mn)", "With Cr eq = Cr + 1.5·Si = {0} and the desired ratio Cr eq / Ni eq = 1.5, what Ni content (%) must the weld metal have, given C = {1}% and Mn = {2}%? (Ni eq = Ni + 30·C + 0.5·Mn)", "Con Cr eq = Cr + 1,5·Si = {0} y la razón deseada Cr eq / Ni eq = 1,5, ¿qué contenido de Ni (%) debe tener el metal de soldadura, siendo C = {1}% y Mn = {2}%? (Ni eq = Ni + 30·C + 0,5·Mn)", N(cre, 2), N(c, 2), N(mn)), ni2, "%", YF("Ni eq = {0}/1,5 = {1}; Ni = Ni eq − 30·C − 0,5·Mn = {1} − {2} − {3} = {4}%.", N(cre, 2), N(cre / 1.5, 3), N(30 * c, 2), N(0.5 * mn, 2), N(ni2, 3)), 3);
        })
    };
}
