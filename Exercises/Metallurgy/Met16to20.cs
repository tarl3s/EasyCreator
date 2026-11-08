namespace EasyCreator.Exercises;

[Exercises("met-16-aluminio")]
public sealed class Met16Aluminium : PoolGen
{
    static Exercise Extra(Random r)
    {
        double sy = S(r, 100, 300, 5), dT = sy / (70000 * 23.6e-6);
        return Q(XF("Uma peça de alumínio travada (E = 70 GPa; α = 23,6·10⁻⁶ /°C) tem limite de escoamento {0} MPa. Que aquecimento ΔT (°C) já a faz escoar?", "A restrained aluminium part (E = 70 GPa; α = 23.6·10⁻⁶ /°C) has a yield strength of {0} MPa. What temperature rise ΔT (°C) already makes it yield?", "Una pieza de aluminio trabada (E = 70 GPa; α = 23,6·10⁻⁶ /°C) tiene límite elástico {0} MPa. ¿Qué calentamiento ΔT (°C) ya la hace fluir?", N(sy, 0)), dT, "°C", YF("ΔT = σe / (E·α) = {0} / (70 000 · 23,6·10⁻⁶) = {1} °C.", N(sy, 0), N(dT, 1)));
    }

    static Exercise Facts(Random r)
    {
        var bank = new (string q, string right, string[] wrong)[]
        {
            (X("Por que o alumínio exige limpeza do óxido antes de soldar?", "Why must aluminium oxide be removed before welding?", "¿Por qué el aluminio exige limpiar el óxido antes de soldar?"), X("O óxido (Al₂O₃) funde a cerca de 2050 °C, bem acima dos 660 °C do metal", "The oxide (Al₂O₃) melts at about 2050 °C, far above the metal's 660 °C", "El óxido (Al₂O₃) funde a unos 2050 °C, muy por encima de los 660 °C del metal"), new[] { X("O óxido funde antes do metal", "The oxide melts before the metal", "El óxido funde antes que el metal"), X("O óxido é magnético", "The oxide is magnetic", "El óxido es magnético"), X("O óxido conduz melhor o calor", "The oxide conducts heat better", "El óxido conduce mejor el calor") }),
            (X("A principal causa de porosidade na solda de alumínio é:", "The main cause of porosity in aluminium welds is:", "La principal causa de porosidad en la soldadura de aluminio es:"), X("hidrogênio (umidade, óleo, óxido hidratado) dissolvido na poça", "hydrogen (moisture, oil, hydrated oxide) dissolved in the pool", "hidrógeno (humedad, aceite, óxido hidratado) disuelto en el baño"), new[] { X("carbono em excesso", "excess carbon", "exceso de carbono"), X("nitrogênio do aço", "nitrogen from steel", "nitrógeno del acero"), X("falta de pré-aquecimento apenas", "lack of preheat only", "solo falta de precalentamiento") }),
            (X("Por que o alumínio pede mais corrente que o aço na mesma espessura?", "Why does aluminium need more current than steel at the same thickness?", "¿Por qué el aluminio pide más corriente que el acero con el mismo espesor?"), X("Conduz muito melhor o calor, que se espalha pela peça", "It conducts heat much better, which spreads through the part", "Conduce mucho mejor el calor, que se dispersa por la pieza"), new[] { X("Tem ponto de fusão mais alto", "It has a higher melting point", "Tiene un punto de fusión más alto"), X("É mais denso", "It is denser", "Es más denso"), X("Tem óxido magnético", "It has a magnetic oxide", "Tiene un óxido magnético") }),
            (X("A trinca a quente em ligas de alumínio da série 6xxx é combatida usando metal de adição:", "Hot cracking in 6xxx aluminium alloys is countered by using filler metal:", "La fisura en caliente en aleaciones de aluminio de la serie 6xxx se combate usando metal de aporte:"), X("com silício (4043) ou magnésio (5356), nunca sem adição", "with silicon (4043) or magnesium (5356), never autogenous", "con silicio (4043) o magnesio (5356), nunca sin aporte"), new[] { X("de aço inox", "of stainless steel", "de acero inoxidable"), X("de cobre puro", "of pure copper", "de cobre puro"), X("nenhum: soldar sem adição", "none: weld without filler", "ninguno: soldar sin aporte") }),
            (X("O alumínio não muda de cor ao esquentar. O risco disso é:", "Aluminium does not change colour when heated. The risk is:", "El aluminio no cambia de color al calentarse. El riesgo es:"), X("a peça pode derreter e desabar sem aviso visual", "the part can melt and collapse with no visual warning", "la pieza puede derretirse y colapsar sin aviso visual"), new[] { X("o cordão sai sempre azul", "the bead always turns blue", "el cordón siempre sale azul"), X("o arco se apaga", "the arc goes out", "el arco se apaga"), X("nenhum risco", "no risk", "ningún riesgo") }),
        };
        var b = bank[r.Next(bank.Length)];
        return Fact(r, b.q, b.right, X("Alumínio: óxido refratário (2050 °C), alta condutividade, hidrogênio gera poros, sensível a trinca a quente; limpe, use argônio e metal de adição adequado.", "Aluminium: refractory oxide (2050 °C), high conductivity, hydrogen causes pores, prone to hot cracking; clean, use argon and the right filler.", "Aluminio: óxido refractario (2050 °C), alta conductividad, el hidrógeno causa poros, sensible a la fisura en caliente; limpie, use argón y metal de aporte adecuado."), b.wrong);
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Facts, r =>
        {
            double L = S(r, 0.5, 6, 0.1), dT = S(r, 50, 400, 10), dl = 23.6e-6 * L * 1000 * dT;
            return Q(XF("Uma barra de alumínio de {0} m (α = 23,6·10⁻⁶ /°C) esquenta {1} °C. Quantos mm ela se alonga livremente?", "An aluminium bar {0} m long (α = 23.6·10⁻⁶ /°C) is heated by {1} °C. By how many mm does it lengthen freely?", "Una barra de aluminio de {0} m (α = 23,6·10⁻⁶ /°C) se calienta {1} °C. ¿Cuántos mm se alarga libremente?", N(L, 1), N(dT, 0)), dl, "mm", YF("ΔL = α·L·ΔT = 23,6·10⁻⁶ · {0} mm · {1} = {2} mm.", N(L * 1000, 0), N(dT, 0), N(dl, 3)));
        }),
        Difficulty.Medium => F(Facts, r =>
        {
            double L = S(r, 0.5, 4, 0.1), dT = S(r, 100, 400, 10), da = 23.6e-6 * L * 1000 * dT, ds = 12e-6 * L * 1000 * dT;
            return Q(XF("Duas barras de {0} m, uma de alumínio (23,6·10⁻⁶ /°C) e outra de aço (12·10⁻⁶ /°C), esquentam {1} °C. Quantos mm a mais o alumínio se alonga?", "Two bars {0} m long, one of aluminium (23.6·10⁻⁶ /°C) and one of steel (12·10⁻⁶ /°C), are heated by {1} °C. How many mm more does the aluminium lengthen?", "Dos barras de {0} m, una de aluminio (23,6·10⁻⁶ /°C) y otra de acero (12·10⁻⁶ /°C), se calientan {1} °C. ¿Cuántos mm más se alarga el aluminio?", N(L, 1), N(dT, 0)), da - ds, "mm", YF("Alumínio: {0} mm; aço: {1} mm; diferença = {2} mm.", N(da, 3), N(ds, 3), N(da - ds, 3)));
        }),
        Difficulty.Hard => F(Extra, r =>
        {
            double dT = S(r, 50, 300, 5), s = 70000 * 23.6e-6 * dT;
            return Q(XF("Uma peça de alumínio (E = 70 GPa; α = 23,6·10⁻⁶ /°C) totalmente travada esquenta {0} °C. Qual a tensão de compressão que surge, em MPa?", "A fully restrained aluminium part (E = 70 GPa; α = 23.6·10⁻⁶ /°C) is heated by {0} °C. What compressive stress arises, in MPa?", "Una pieza de aluminio (E = 70 GPa; α = 23,6·10⁻⁶ /°C) totalmente trabada se calienta {0} °C. ¿Qué tensión de compresión aparece, en MPa?", N(dT, 0)), s, "MPa", YF("σ = E·α·ΔT = 70 000 · 23,6·10⁻⁶ · {0} = {1} MPa.", N(dT, 0), N(s, 1)));
        }, Facts),
        _ => F(r =>
        {
            double sy = S(r, 100, 300, 5), dT = sy / (70000 * 23.6e-6);
            return Q(XF("Uma peça de alumínio travada (E = 70 GPa; α = 23,6·10⁻⁶ /°C) tem limite de escoamento {0} MPa. Que aquecimento ΔT (°C) já a faz escoar?", "A restrained aluminium part (E = 70 GPa; α = 23.6·10⁻⁶ /°C) has a yield strength of {0} MPa. What temperature rise ΔT (°C) already makes it yield?", "Una pieza de aluminio trabada (E = 70 GPa; α = 23,6·10⁻⁶ /°C) tiene límite elástico {0} MPa. ¿Qué calentamiento ΔT (°C) ya la hace fluir?", N(sy, 0)), dT, "°C", YF("ΔT = σe / (E·α) = {0} / (70 000 · 23,6·10⁻⁶) = {1} °C.", N(sy, 0), N(dT, 1)));
        }, r =>
        {
            double dT = S(r, 50, 300, 10), ra = 70000 * 23.6e-6 * dT, rs = 200000 * 12e-6 * dT;
            return Q(XF("Para o mesmo aquecimento de {0} °C com a peça travada, quantas vezes a tensão térmica do aço (E = 200 GPa, α = 12·10⁻⁶ /°C) é maior que a do alumínio (E = 70 GPa, α = 23,6·10⁻⁶ /°C)?", "For the same {0} °C rise with the part fully restrained, how many times larger is the steel's thermal stress (E = 200 GPa, α = 12·10⁻⁶ /°C) than aluminium's (E = 70 GPa, α = 23.6·10⁻⁶ /°C)?", "Para el mismo calentamiento de {0} °C con la pieza trabada, ¿cuántas veces es mayor la tensión térmica del acero (E = 200 GPa, α = 12·10⁻⁶ /°C) que la del aluminio (E = 70 GPa, α = 23,6·10⁻⁶ /°C)?", N(dT, 0)), rs / ra, "", YF("Aço: {0} MPa; alumínio: {1} MPa; razão = {2}.", N(rs, 1), N(ra, 1), N(rs / ra, 3)));
        }, Facts)
    };
}

[Exercises("met-17-fratura")]
public sealed class Met17Fracture : PoolGen
{
    static Exercise Facts(Random r)
    {
        var bank = new (string q, string right, string[] wrong)[]
        {
            (X("O que o ensaio Charpy mede?", "What does the Charpy test measure?", "¿Qué mide el ensayo Charpy?"), X("A energia absorvida na fratura por impacto de um corpo entalhado", "The energy absorbed in impact fracture of a notched specimen", "La energía absorbida en la fractura por impacto de una probeta entallada"), new[] { X("A dureza superficial", "Surface hardness", "La dureza superficial"), X("A vida em fadiga", "Fatigue life", "La vida a fatiga"), X("O módulo de elasticidade", "The elastic modulus", "El módulo de elasticidad") }),
            (X("Os aços ferríticos (CCC) em baixa temperatura costumam:", "Ferritic (BCC) steels at low temperature tend to:", "Los aceros ferríticos (CCC) a baja temperatura suelen:"), X("ter transição dúctil-frágil: a energia Charpy cai bruscamente", "show a ductile-brittle transition: Charpy energy drops sharply", "tener transición dúctil-frágil: la energía Charpy cae bruscamente"), new[] { X("ficar mais dúcteis", "become more ductile", "volverse más dúctiles"), X("não mudar nada", "not change at all", "no cambiar nada"), X("virar CFC", "turn FCC", "volverse CFC") }),
            (X("Qual estrutura NÃO tem transição dúctil-frágil marcante?", "Which structure does NOT show a marked ductile-brittle transition?", "¿Qué estructura NO tiene una transición dúctil-frágil marcada?"), X("CFC (inox austenítico, alumínio, cobre)", "FCC (austenitic stainless, aluminium, copper)", "CFC (inox austenítico, aluminio, cobre)"), new[] { X("CCC (aço ferrítico)", "BCC (ferritic steel)", "CCC (acero ferrítico)"), X("Martensita não revenida", "Untempered martensite", "Martensita sin revenir"), X("Ferro fundido cinzento", "Grey cast iron", "Hierro fundido gris") }),
            (X("Uma trinca é perigosa quando o fator de intensidade de tensões K chega a:", "A crack becomes dangerous when the stress intensity factor K reaches:", "Una grieta se vuelve peligrosa cuando el factor de intensidad de tensiones K llega a:"), X("a tenacidade à fratura do material, K_IC", "the material's fracture toughness, K_IC", "la tenacidad a la fractura del material, K_IC"), new[] { X("o limite de escoamento", "the yield strength", "el límite elástico"), X("a dureza Vickers", "the Vickers hardness", "la dureza Vickers"), X("o módulo E", "the modulus E", "el módulo E") }),
        };
        var b = bank[r.Next(bank.Length)];
        return Fact(r, b.q, b.right, X("Charpy = energia de impacto; CCC tem transição dúctil-frágil, CFC não; fratura quando K = K_IC.", "Charpy = impact energy; BCC has a ductile-brittle transition, FCC does not; fracture when K = K_IC.", "Charpy = energía de impacto; CCC tiene transición dúctil-frágil, CFC no; fractura cuando K = K_IC."), b.wrong);
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Facts, r =>
        {
            double m = S(r, 10, 30, 1), h0 = S(r, 1.0, 2.0, 0.05), h1 = h0 - S(r, 0.2, 0.9, 0.05), e = m * 9.81 * (h0 - h1);
            return Q(XF("Um pêndulo Charpy de {0} kg cai de {1} m e, depois de quebrar o corpo de prova, sobe a {2} m. Qual a energia absorvida? (g = 9,81 m/s²)", "A {0} kg Charpy pendulum falls from {1} m and, after breaking the specimen, rises to {2} m. What energy was absorbed? (g = 9.81 m/s²)", "Un péndulo Charpy de {0} kg cae desde {1} m y, tras romper la probeta, sube a {2} m. ¿Qué energía se absorbió? (g = 9,81 m/s²)", N(m, 0), N(h0), N(h1)), e, "J", YF("E = m·g·(h₀ − h₁) = {0} · 9,81 · ({1} − {2}) = {3} J.", N(m, 0), N(h0), N(h1), N(e, 1)));
        }),
        Difficulty.Medium => F(Facts, r =>
        {
            double y = S(r, 1.0, 1.3, 0.05), s = S(r, 50, 250, 5), a = S(r, 1, 20, 0.5) / 1000, k = y * s * Sqrt(Pi * a);
            return Q(XF("K = Y·σ·√(π·a). Para Y = {0}, σ = {1} MPa e uma trinca de a = {2} mm, qual o K, em MPa·√m?", "K = Y·σ·√(π·a). For Y = {0}, σ = {1} MPa and a crack of a = {2} mm, what is K, in MPa·√m?", "K = Y·σ·√(π·a). Para Y = {0}, σ = {1} MPa y una grieta de a = {2} mm, ¿cuál es K, en MPa·√m?", N(y), N(s, 0), N(a * 1000)), k, "MPa·√m", YF("K = {0} · {1} · √(π · {2}) = {3} MPa·√m.", N(y), N(s, 0), N(a, 4), N(k, 2)));
        }),
        Difficulty.Hard => F(r =>
        {
            double y = S(r, 1.0, 1.3, 0.05), s = S(r, 100, 300, 10), kic = S(r, 30, 120, 5), a = Pow2(kic / (y * s)) / Pi * 1000;
            return Q(XF("Aço com K_IC = {0} MPa·√m sob tensão de {1} MPa (Y = {2}). Qual o tamanho crítico de trinca a_c = (1/π)·(K_IC/(Y·σ))², em mm?", "Steel with K_IC = {0} MPa·√m under a stress of {1} MPa (Y = {2}). What is the critical crack size a_c = (1/π)·(K_IC/(Y·σ))², in mm?", "Acero con K_IC = {0} MPa·√m bajo una tensión de {1} MPa (Y = {2}). ¿Cuál es el tamaño crítico de grieta a_c = (1/π)·(K_IC/(Y·σ))², en mm?", N(kic, 0), N(s, 0), N(y)), a, "mm", YF("a_c = (1/π)·({0}/({1}·{2}))² = {3} m = {4} mm.", N(kic, 0), N(y), N(s, 0), N(a / 1000, 5), N(a, 2)));
        }, r =>
        {
            double y = S(r, 1.0, 1.3, 0.05), s = S(r, 50, 250, 5), a = S(r, 1, 20, 0.5) / 1000, k = y * s * Sqrt(Pi * a);
            return Q(XF("K = Y·σ·√(π·a). Para Y = {0}, σ = {1} MPa e uma trinca de a = {2} mm, qual o K, em MPa·√m?", "K = Y·σ·√(π·a). For Y = {0}, σ = {1} MPa and a crack of a = {2} mm, what is K, in MPa·√m?", "K = Y·σ·√(π·a). Para Y = {0}, σ = {1} MPa y una grieta de a = {2} mm, ¿cuál es K, en MPa·√m?", N(y), N(s, 0), N(a * 1000)), k, "MPa·√m", YF("K = {0} · {1} · √(π · {2}) = {3} MPa·√m.", N(y), N(s, 0), N(a, 4), N(k, 2)));
        }),
        _ => F(r =>
        {
            double y = S(r, 1.0, 1.3, 0.05), a = S(r, 2, 30, 1) / 1000, kic = S(r, 30, 120, 5), sc = kic / (y * Sqrt(Pi * a));
            return Q(XF("Uma trinca de {0} mm em aço com K_IC = {1} MPa·√m (Y = {2}). Qual a tensão que leva à fratura, em MPa?", "A {0} mm crack in steel with K_IC = {1} MPa·√m (Y = {2}). What stress causes fracture, in MPa?", "Una grieta de {0} mm en acero con K_IC = {1} MPa·√m (Y = {2}). ¿Qué tensión lleva a la fractura, en MPa?", N(a * 1000, 0), N(kic, 0), N(y)), sc, "MPa", YF("σc = K_IC / (Y·√(π·a)) = {0} / ({1} · √(π · {2})) = {3} MPa.", N(kic, 0), N(y), N(a, 4), N(sc, 1)));
        }, r =>
        {
            double y = 1.12, sy = S(r, 250, 700, 10), kic = S(r, 40, 120, 5), a = S(r, 2, 15, 1) / 1000, sc = kic / (y * Sqrt(Pi * a)), nf = sc / sy;
            return Q(XF("Aço com σe = {0} MPa e K_IC = {1} MPa·√m tem uma trinca de {2} mm (Y = 1,12). Qual a razão σ_fratura / σe? (maior que 1: escoa antes de fraturar)", "Steel with σy = {0} MPa and K_IC = {1} MPa·√m has a {2} mm crack (Y = 1.12). What is the ratio σ_fracture / σy? (greater than 1: it yields before fracturing)", "Acero con σe = {0} MPa y K_IC = {1} MPa·√m tiene una grieta de {2} mm (Y = 1,12). ¿Cuál es la razón σ_fractura / σe? (mayor que 1: fluye antes de fracturar)", N(sy, 0), N(kic, 0), N(a * 1000, 0)), nf, "", YF("σ_fratura = {0} / (1,12·√(π·{1})) = {2} MPa; razão = {2}/{3} = {4}.", N(kic, 0), N(a, 4), N(sc, 1), N(sy, 0), N(nf, 3)));
        })
    };
    static double Pow2(double v) => v * v;
}

[Exercises("met-18-fadiga")]
public sealed class Met18Fatigue : PoolGen
{
    static Exercise Extra(Random r)
    {
        double mx = S(r, 80, 300, 5), mn = S(r, 0, 70, 5), rr = mn / mx;
        return Q(XF("Em um ciclo de fadiga a tensão varia de {0} MPa a {1} MPa. Qual a razão de tensões R = σmín/σmáx?", "In a fatigue cycle the stress varies from {0} MPa to {1} MPa. What is the stress ratio R = σmin/σmax?", "En un ciclo de fatiga la tensión varía de {0} MPa a {1} MPa. ¿Cuál es la razón de tensiones R = σmín/σmáx?", N(mn, 0), N(mx, 0)), rr, "", YF("R = σmín / σmáx = {0} / {1} = {2}.", N(mn, 0), N(mx, 0), N(rr, 3)), 3);
    }

    static Exercise Facts(Random r)
    {
        var bank = new (string q, string right, string[] wrong)[]
        {
            (X("Em estruturas soldadas, a vida em fadiga depende principalmente de:", "In welded structures, fatigue life depends mainly on:", "En estructuras soldadas, la vida a fatiga depende principalmente de:"), X("a variação de tensão Δσ e o detalhe da junta, quase não da resistência do aço", "the stress range Δσ and the joint detail, hardly on the steel strength", "la variación de tensión Δσ y el detalle de la junta, casi no de la resistencia del acero"), new[] { X("só da resistência à tração do aço", "only the steel's tensile strength", "solo de la resistencia a la tracción del acero"), X("só da cor do cordão", "only the bead colour", "solo del color del cordón"), X("do tipo de gás apenas", "the type of gas only", "solo del tipo de gas") }),
            (X("Onde as trincas de fadiga mais comumente começam em uma solda?", "Where do fatigue cracks most commonly start in a weld?", "¿Dónde comienzan más comúnmente las grietas de fatiga en una soldadura?"), X("No pé do cordão, onde há concentração de tensão", "At the weld toe, where stress concentrates", "En el pie del cordón, donde se concentra la tensión"), new[] { X("No centro do metal base", "At the middle of the base metal", "En el centro del metal base"), X("Dentro do eletrodo", "Inside the electrode", "Dentro del electrodo"), X("Longe da solda, sempre", "Always far from the weld", "Siempre lejos de la soldadura") }),
            (X("Qual tratamento melhora a vida em fadiga do pé do cordão?", "Which treatment improves the fatigue life of the weld toe?", "¿Qué tratamiento mejora la vida a fatiga del pie del cordón?"), X("Esmerilhar o pé ou martelar (peening) para suavizar e comprimir", "Grinding the toe or peening it to smooth and compress it", "Esmerilar el pie o martillar (peening) para suavizar y comprimir"), new[] { X("Deixar o respingo", "Leaving the spatter", "Dejar la salpicadura"), X("Pintar de preto", "Painting it black", "Pintarlo de negro"), X("Aumentar o reforço do cordão", "Increasing the bead reinforcement", "Aumentar el refuerzo del cordón") }),
        };
        var b = bank[r.Next(bank.Length)];
        return Fact(r, b.q, b.right, X("Fadiga de soldas: o que importa é Δσ (faixa de tensão) e a categoria do detalhe (FAT); o pé do cordão é o ponto crítico.", "Weld fatigue: what matters is Δσ (stress range) and the detail category (FAT); the weld toe is the critical point.", "Fatiga de soldaduras: lo que importa es Δσ (rango de tensión) y la categoría del detalle (FAT); el pie del cordón es el punto crítico."), b.wrong);
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Extra, Facts, r =>
        {
            double mx = S(r, 60, 300, 5), mn = S(r, -60, 100, 5), ds = mx - mn;
            if (mn >= mx) mn = mx - 40;
            return Q(XF("Em um ciclo de fadiga a tensão varia de {0} MPa a {1} MPa. Qual a faixa de tensão Δσ?", "In a fatigue cycle the stress varies from {0} MPa to {1} MPa. What is the stress range Δσ?", "En un ciclo de fatiga la tensión varía de {0} MPa a {1} MPa. ¿Cuál es el rango de tensión Δσ?", N(mn, 0), N(mx, 0)), mx - mn, "MPa", YF("Δσ = σmáx − σmín = {0} − ({1}) = {2} MPa.", N(mx, 0), N(mn, 0), N(mx - mn, 0)));
        }),
        Difficulty.Medium => F(Extra, r =>
        {
            double fat = P(r, 36.0, 45, 56, 71, 80, 90, 100), ds = S(r, 20, 120, 5), n = 2e6 * Math.Pow(fat / ds, 3);
            return Q(XF("Curva S-N de projeto: N = 2·10⁶ × (FAT/Δσ)³. Para FAT = {0} MPa e Δσ = {1} MPa, quantos ciclos a junta suporta?", "Design S-N curve: N = 2·10⁶ × (FAT/Δσ)³. For FAT = {0} MPa and Δσ = {1} MPa, how many cycles does the joint withstand?", "Curva S-N de proyecto: N = 2·10⁶ × (FAT/Δσ)³. Para FAT = {0} MPa y Δσ = {1} MPa, ¿cuántos ciclos soporta la junta?", N(fat, 0), N(ds, 0)), n, X("ciclos", "cycles", "ciclos"), YF("N = 2·10⁶ · ({0}/{1})³ = {2}.", N(fat, 0), N(ds, 0), Sci(n)));
        }, Facts),
        Difficulty.Hard => F(r =>
        {
            double fat = P(r, 45.0, 56, 71, 80, 90), n1 = S(r, 50, 400, 10) * 1000, n2 = S(r, 50, 400, 10) * 1000, ds = S(r, 30, 80, 5);
            double N1 = 2e6 * Math.Pow(fat / ds, 3), D = n1 / N1 + n2 / N1;
            double ds2 = ds * P(r, 0.5, 0.7, 1.2), N2 = 2e6 * Math.Pow(fat / ds2, 3); D = n1 / N1 + n2 / N2;
            return Q(XF("Regra de Miner: D = Σ nᵢ/Nᵢ (falha em D = 1). FAT = {0} MPa; {1} ciclos a Δσ = {2} MPa e {3} ciclos a Δσ = {4} MPa (N = 2·10⁶·(FAT/Δσ)³). Qual o dano D acumulado?", "Miner's rule: D = Σ nᵢ/Nᵢ (failure at D = 1). FAT = {0} MPa; {1} cycles at Δσ = {2} MPa and {3} cycles at Δσ = {4} MPa (N = 2·10⁶·(FAT/Δσ)³). What is the accumulated damage D?", "Regla de Miner: D = Σ nᵢ/Nᵢ (falla en D = 1). FAT = {0} MPa; {1} ciclos a Δσ = {2} MPa y {3} ciclos a Δσ = {4} MPa (N = 2·10⁶·(FAT/Δσ)³). ¿Cuál es el daño D acumulado?", N(fat, 0), N(n1, 0), N(ds, 0), N(n2, 0), N(ds2, 1)), D, "", YF("N₁ = {0}; N₂ = {1}; D = {2}/{0} + {3}/{1} = {4}.", N(N1, 0), N(N2, 0), N(n1, 0), N(n2, 0), N(D, 4)), 3);
        }, r =>
        {
            double fat = P(r, 45.0, 56, 71, 80, 90), n = S(r, 0.5, 20, 0.5) * 1e6, ds = fat * Math.Pow(2e6 / n, 1.0 / 3);
            return Q(XF("Que faixa de tensão Δσ máxima uma junta de categoria FAT {0} MPa suporta por {1} milhões de ciclos? (N = 2·10⁶·(FAT/Δσ)³)", "What maximum stress range Δσ can a joint of category FAT {0} MPa withstand for {1} million cycles? (N = 2·10⁶·(FAT/Δσ)³)", "¿Qué rango de tensión Δσ máximo soporta una junta de categoría FAT {0} MPa durante {1} millones de ciclos? (N = 2·10⁶·(FAT/Δσ)³)", N(fat, 0), N(n / 1e6)), ds, "MPa", YF("Δσ = FAT · (2·10⁶/N)^(1/3) = {0} · (2/{1})^(1/3) = {2} MPa.", N(fat, 0), N(n / 1e6), N(ds, 2)));
        }),
        _ => F(r =>
        {
            double fat = P(r, 45.0, 56, 71, 80), ds = S(r, 25, 70, 5), cpd = S(r, 200, 3000, 100), n = 2e6 * Math.Pow(fat / ds, 3), yrs = n / cpd / 365;
            return Q(XF("Um detalhe soldado FAT {0} MPa sofre Δσ = {1} MPa, {2} ciclos por dia (N = 2·10⁶·(FAT/Δσ)³). Quantos anos dura até a falha prevista? (365 dias por ano)", "A welded detail of FAT {0} MPa is subjected to Δσ = {1} MPa, {2} cycles per day (N = 2·10⁶·(FAT/Δσ)³). How many years until the predicted failure? (365 days per year)", "Un detalle soldado FAT {0} MPa sufre Δσ = {1} MPa, {2} ciclos por día (N = 2·10⁶·(FAT/Δσ)³). ¿Cuántos años dura hasta la falla prevista? (365 días por año)", N(fat, 0), N(ds, 0), N(cpd, 0)), yrs, X("anos", "years", "años"), YF("N = {0} ciclos; anos = N / ({1} · 365) = {2}.", Sci(n), N(cpd, 0), N(yrs, 2)));
        }, r =>
        {
            double fat = P(r, 56.0, 71, 80), ds = S(r, 30, 70, 5), f = P(r, 2.0, 1.5, 0.8, 0.5), n1 = 2e6 * Math.Pow(fat / ds, 3), n2 = 2e6 * Math.Pow(fat / (ds * f), 3);
            return Q(XF("Se Δσ passa de {0} MPa para {1} MPa em um detalhe FAT {2} MPa, quantas vezes a vida em ciclos (N = 2·10⁶·(FAT/Δσ)³) fica maior (se >1) ou menor (se <1)? Dê N₂/N₁.", "If Δσ goes from {0} MPa to {1} MPa in a FAT {2} MPa detail, how many times larger (if >1) or smaller (if <1) does the life in cycles (N = 2·10⁶·(FAT/Δσ)³) become? Give N₂/N₁.", "Si Δσ pasa de {0} MPa a {1} MPa en un detalle FAT {2} MPa, ¿cuántas veces mayor (si >1) o menor (si <1) queda la vida en ciclos (N = 2·10⁶·(FAT/Δσ)³)? Dé N₂/N₁.", N(ds, 0), N(ds * f, 1), N(fat, 0)), n2 / n1, "", YF("N₂/N₁ = (Δσ₁/Δσ₂)³ = (1/{0})³ = {1}.", N(f, 2), N(n2 / n1, 4)));
        })
    };
}

[Exercises("met-19-qualificacao")]
public sealed class Met19Qualification : PoolGen
{
    static Exercise Facts(Random r)
    {
        var bank = new (string q, string right, string[] wrong)[]
        {
            (X("Qual documento diz COMO soldar (processo, consumível, faixas de corrente, pré-aquecimento)?", "Which document says HOW to weld (process, consumable, current ranges, preheat)?", "¿Qué documento dice CÓMO soldar (proceso, consumible, rangos de corriente, precalentamiento)?"), X("EPS (WPS)", "WPS", "EPS (WPS)"), new[] { X("RQP (PQR)", "PQR", "RQP (PQR)"), X("Certificado do soldador", "The welder certificate", "Certificado del soldador"), X("Desenho de montagem", "The assembly drawing", "Plano de montaje") }),
            (X("Qual documento registra os valores REAIS usados e os resultados dos ensaios do corpo de prova de qualificação?", "Which document records the ACTUAL values used and the test results of the qualification coupon?", "¿Qué documento registra los valores REALES usados y los resultados de los ensayos de la probeta de calificación?"), X("RQP (PQR)", "PQR", "RQP (PQR)"), new[] { X("EPS (WPS)", "WPS", "EPS (WPS)"), X("Ordem de compra", "The purchase order", "Orden de compra"), X("Manual do eletrodo", "The electrode manual", "Manual del electrodo") }),
            (X("O que se qualifica quando um soldador faz o teste de habilidade?", "What is qualified when a welder takes the skill test?", "¿Qué se califica cuando un soldador hace la prueba de habilidad?"), X("A habilidade da pessoa para executar o procedimento (RQS/WPQ)", "The person's ability to carry out the procedure (WPQ)", "La habilidad de la persona para ejecutar el procedimiento (RQS/WPQ)"), new[] { X("O procedimento em si", "The procedure itself", "El procedimiento en sí"), X("A máquina de solda", "The welding machine", "La máquina de soldar"), X("O metal de base", "The base metal", "El metal base") }),
            (X("O que acontece com a EPS quando muda uma variável essencial (ex.: processo ou grupo do metal de base)?", "What happens to the WPS when an essential variable changes (e.g. process or base metal group)?", "¿Qué pasa con la EPS cuando cambia una variable esencial (p. ej., proceso o grupo del metal base)?"), X("É preciso requalificar (novo RQP)", "It must be requalified (new PQR)", "Hay que recalificar (nuevo RQP)"), new[] { X("Nada, continua valendo", "Nothing, it stays valid", "Nada, sigue vigente"), X("Basta avisar o cliente", "Just tell the client", "Basta avisar al cliente"), X("Só troca a cor do cordão", "Only the bead colour changes", "Solo cambia el color del cordón") }),
            (X("Os ensaios destrutivos típicos de qualificação incluem:", "Typical destructive qualification tests include:", "Los ensayos destructivos típicos de calificación incluyen:"), X("tração, dobramento, macrografia e (se exigido) Charpy e dureza", "tensile, bend, macro-etch and (if required) Charpy and hardness", "tracción, doblado, macrografía y (si se exige) Charpy y dureza"), new[] { X("apenas inspeção visual", "visual inspection only", "solo inspección visual"), X("somente líquido penetrante", "penetrant testing only", "solo líquidos penetrantes"), X("apenas medir a corrente", "just measuring current", "solo medir la corriente") }),
        };
        var b = bank[r.Next(bank.Length)];
        return Fact(r, b.q, b.right, X("EPS/WPS = a receita; RQP/PQR = a prova de que a receita funciona (valores reais e ensaios); RQS/WPQ = a habilidade do soldador.", "WPS = the recipe; PQR = the proof that the recipe works (actual values and tests); WPQ = the welder's skill.", "EPS/WPS = la receta; RQP/PQR = la prueba de que la receta funciona (valores reales y ensayos); RQS/WPQ = la habilidad del soldador."), b.wrong);
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Facts, r =>
        {
            double w = S(r, 10, 40, 1), t = S(r, 3, 25, 0.5), F = S(r, 20, 200, 5), s = F * 1000 / (w * t);
            return Q(XF("No ensaio de tração da qualificação, um corpo de prova de {0} mm × {1} mm rompe com {2} kN. Qual a resistência σr?", "In the qualification tensile test, a {0} mm × {1} mm specimen breaks at {2} kN. What is the strength σr?", "En el ensayo de tracción de la calificación, una probeta de {0} mm × {1} mm se rompe con {2} kN. ¿Cuál es la resistencia σr?", N(w, 0), N(t), N(F, 0)), s, "MPa", YF("A₀ = {0} · {1} = {2} mm²; σr = {3} / {2} = {4} MPa.", N(w, 0), N(t), N(w * t, 1), N(F * 1000, 0), N(s, 1)));
        }),
        Difficulty.Medium => F(Facts, r =>
        {
            double t = S(r, 6, 40, 1), dm = 4 * t;
            return Q(XF("No ensaio de dobramento guiado, o pino de dobra tem diâmetro de 4 vezes a espessura do corpo de prova. Para t = {0} mm, qual o diâmetro do pino?", "In the guided bend test, the bending pin diameter is 4 times the specimen thickness. For t = {0} mm, what is the pin diameter?", "En el ensayo de doblado guiado, el diámetro del pasador de doblado es 4 veces el espesor de la probeta. Para t = {0} mm, ¿cuál es el diámetro del pasador?", N(t, 0)), dm, "mm", YF("D = 4·t = 4 · {0} = {1} mm.", N(t, 0), N(dm, 0)));
        }, r =>
        {
            double sr = P(r, 380.0, 415, 485, 565), F = S(r, 20, 120, 5), w = S(r, 19, 25, 1), t = S(r, 8, 20, 1), s = F * 1000 / (w * t);
            bool ok = s >= sr;
            string y = X("Aprovado", "Pass", "Aprobado"), n = X("Reprovado", "Fail", "Reprobado");
            return Fact(r, XF("O metal de base exige σr mínimo de {0} MPa. O corpo de prova de {1} mm × {2} mm rompe com {3} kN. O ensaio de tração passa?", "The base metal requires a minimum σr of {0} MPa. The {1} mm × {2} mm specimen breaks at {3} kN. Does the tensile test pass?", "El metal base exige un σr mínimo de {0} MPa. La probeta de {1} mm × {2} mm se rompe con {3} kN. ¿Pasa el ensayo de tracción?", N(sr, 0), N(w, 0), N(t, 0), N(F, 0)), ok ? y : n, YF("σr = {0} / ({1} · {2}) = {3} MPa {4} {5} MPa.", N(F * 1000, 0), N(w, 0), N(t, 0), N(s, 1), ok ? "≥" : "<", N(sr, 0)), ok ? n : y);
        }),
        Difficulty.Hard => F(r =>
        {
            double t1 = S(r, 22, 80, 1), t2 = 2 * t1;
            return Q(XF("Regra didática: um procedimento qualificado em chapa de espessura t (de 19 a 38 mm) vale de 5 mm até 2t. Qualificado com t = {0} mm, qual a maior espessura que a EPS cobre, em mm?", "Teaching rule: a procedure qualified on plate of thickness t (19 to 38 mm) is valid from 5 mm up to 2t. Qualified at t = {0} mm, what is the greatest thickness the WPS covers, in mm?", "Regla didáctica: un procedimiento calificado en chapa de espesor t (de 19 a 38 mm) vale desde 5 mm hasta 2t. Calificado con t = {0} mm, ¿cuál es el mayor espesor que cubre la EPS, en mm?", N(Math.Round(Math.Min(t1, 38)), 0)), 2 * Math.Round(Math.Min(t1, 38)), "mm", YF("Máximo = 2·t = 2 · {0} = {1} mm.", N(Math.Round(Math.Min(t1, 38)), 0), N(2 * Math.Round(Math.Min(t1, 38)), 0)));
        }, r =>
        {
            double e1 = S(r, 15, 60, 1), e2 = S(r, 15, 60, 1), e3 = S(r, 15, 60, 1), avg = (e1 + e2 + e3) / 3, min = Math.Min(e1, Math.Min(e2, e3));
            bool ok = avg >= 27 && min >= 20;
            string y = X("Aprovado", "Pass", "Aprobado"), n = X("Reprovado", "Fail", "Reprobado");
            return Fact(r, XF("Critério Charpy: média de três corpos ≥ 27 J e nenhum individual abaixo de 20 J. Resultados: {0} J, {1} J e {2} J. O ensaio passa?", "Charpy criterion: average of three specimens ≥ 27 J and no single value below 20 J. Results: {0} J, {1} J and {2} J. Does the test pass?", "Criterio Charpy: promedio de tres probetas ≥ 27 J y ningún valor individual por debajo de 20 J. Resultados: {0} J, {1} J y {2} J. ¿Pasa el ensayo?", N(e1, 0), N(e2, 0), N(e3, 0)), ok ? y : n, YF("Média = {0} J; menor = {1} J → {2}.", N(avg, 1), N(min, 0), ok ? X("passa", "passes", "pasa") : X("não passa", "does not pass", "no pasa")), ok ? n : y);
        }),
        _ => F(r =>
        {
            double sr = P(r, 415.0, 485, 565), w = S(r, 19, 25, 1), tmin = S(r, 6, 12, 1), Fmax = 50, a = sr * w * tmin / 1000;
            return Q(XF("Corpo de prova de tração com largura {0} mm e espessura {1} mm de um aço com σr mínimo de {2} MPa. Qual a força mínima de ruptura, em kN, para aprovar?", "Tensile specimen {0} mm wide and {1} mm thick of a steel with minimum σr of {2} MPa. What is the minimum breaking force, in kN, to pass?", "Probeta de tracción de {0} mm de ancho y {1} mm de espesor de un acero con σr mínimo de {2} MPa. ¿Cuál es la fuerza mínima de rotura, en kN, para aprobar?", N(w, 0), N(tmin, 0), N(sr, 0)), a, "kN", YF("F = σr · A₀ = {0} · ({1} · {2}) = {3} N = {4} kN.", N(sr, 0), N(w, 0), N(tmin, 0), N(sr * w * tmin, 0), N(a, 2)));
        }, r =>
        {
            double d = S(r, 8, 16, 0.5), srq = P(r, 485.0, 565, 620), F = srq * Pi * d * d / 4 / 1000;
            return Q(XF("Que seção circular de corpo de prova (diâmetro, em mm) precisa ter uma barra para que, ao atingir σr = {0} MPa, a máquina indique {1} kN?", "What diameter, in mm, must a round specimen have so that, at σr = {0} MPa, the machine reads {1} kN?", "¿Qué diámetro, en mm, debe tener una probeta redonda para que, al alcanzar σr = {0} MPa, la máquina indique {1} kN?", N(srq, 0), N(F, 2)), d, "mm", YF("A₀ = F/σr = {0} / {1} = {2} mm²; d = √(4A₀/π) = {3} mm.", N(F * 1000, 0), N(srq, 0), N(F * 1000 / srq, 2), N(d, 2)));
        })
    };
}

[Exercises("met-20-ttps")]
public sealed class Met20Pwht : PoolGen
{
    static Exercise Extra(Random r)
    {
        double e = S(r, 12, 120, 1), h = Math.Max(0.25, e / 25);
        return Q(XF("Regra didática do TTPS: patamar de 1 h para cada 25 mm de espessura (mínimo 15 min). Para chapa de {0} mm, quantas horas de patamar?", "PWHT teaching rule: 1 h soak per 25 mm of thickness (minimum 15 min). For a {0} mm plate, how many hours of soak?", "Regla didáctica del TTPS: meseta de 1 h por cada 25 mm de espesor (mínimo 15 min). Para chapa de {0} mm, ¿cuántas horas de meseta?", N(e, 0)), h, "h", YF("t = {0} / 25 = {1} h (mínimo 0,25 h).", N(e, 0), N(e / 25, 3)));
    }

    static Exercise Facts(Random r)
    {
        var bank = new (string q, string right, string[] wrong)[]
        {
            (X("Qual o objetivo principal do tratamento térmico pós-soldagem (TTPS)?", "What is the main purpose of post-weld heat treatment (PWHT)?", "¿Cuál es el objetivo principal del tratamiento térmico postsoldadura (TTPS)?"), X("Aliviar tensões residuais e amolecer/estabilizar a ZTA", "Relieve residual stresses and soften/stabilise the HAZ", "Aliviar tensiones residuales y ablandar/estabilizar la ZAT"), new[] { X("Aumentar a dureza da solda", "Increase the weld hardness", "Aumentar la dureza de la soldadura"), X("Mudar o metal de base", "Change the base metal", "Cambiar el metal base"), X("Pintar a peça", "Paint the part", "Pintar la pieza") }),
            (X("A temperatura de TTPS de aços carbono fica:", "The PWHT temperature of carbon steels is:", "La temperatura de TTPS de aceros al carbono está:"), X("abaixo de A1 (cerca de 600 °C)", "below A1 (around 600 °C)", "por debajo de A1 (alrededor de 600 °C)"), new[] { X("acima de 1000 °C", "above 1000 °C", "por encima de 1000 °C"), X("em 20 °C", "at 20 °C", "a 20 °C"), X("acima da fusão", "above the melting point", "por encima de la fusión") }),
            (X("A distorção de uma peça soldada pode ser reduzida por:", "Distortion of a welded part can be reduced by:", "La distorsión de una pieza soldada se puede reducir con:"), X("sequência balanceada, menos aporte, ponteamento e gabaritos", "balanced sequence, less heat input, tack welds and fixtures", "secuencia balanceada, menos aporte, puntos de soldadura y utillajes"), new[] { X("mais aporte térmico", "more heat input", "más aporte térmico"), X("soldar sempre de um lado só e sem pontear", "always welding on one side with no tacks", "soldar siempre de un solo lado y sin puntear"), X("resfriar de repente com água", "suddenly quenching with water", "enfriar de golpe con agua") }),
            (X("Por que a peça tem tensão residual de tração perto do cordão?", "Why does the part have tensile residual stress near the bead?", "¿Por qué la pieza tiene tensión residual de tracción cerca del cordón?"), X("O cordão quer contrair ao esfriar, mas o metal frio em volta o impede", "The bead wants to shrink while cooling, but the cold metal around it prevents it", "El cordón quiere contraerse al enfriarse, pero el metal frío a su alrededor lo impide"), new[] { X("Por causa do gás de proteção", "Because of the shielding gas", "Por el gas de protección"), X("Por causa da cor do arco", "Because of the arc colour", "Por el color del arco"), X("Não existe tensão residual", "There is no residual stress", "No existe tensión residual") }),
        };
        var b = bank[r.Next(bank.Length)];
        return Fact(r, b.q, b.right, X("Tensões residuais vêm da contração restringida; o TTPS (≈ 600 °C no aço carbono) as alivia; a distorção se controla com sequência, aporte e gabaritos.", "Residual stresses come from restrained shrinkage; PWHT (≈ 600 °C on carbon steel) relieves them; distortion is controlled with sequence, heat input and fixtures.", "Las tensiones residuales vienen de la contracción restringida; el TTPS (≈ 600 °C en acero al carbono) las alivia; la distorsión se controla con secuencia, aporte y utillajes."), b.wrong);
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Extra, Facts, r =>
        {
            double dT = S(r, 20, 150, 5), s = 200000 * 12e-6 * dT;
            return Q(XF("Uma peça de aço (E = 200 GPa; α = 12·10⁻⁶ /°C) totalmente restringida esfria {0} °C. Que tensão de tração surge, em MPa?", "A fully restrained steel part (E = 200 GPa; α = 12·10⁻⁶ /°C) cools by {0} °C. What tensile stress arises, in MPa?", "Una pieza de acero (E = 200 GPa; α = 12·10⁻⁶ /°C) totalmente restringida se enfría {0} °C. ¿Qué tensión de tracción aparece, en MPa?", N(dT, 0)), s, "MPa", YF("σ = E·α·ΔT = 200 000 · 12·10⁻⁶ · {0} = {1} MPa.", N(dT, 0), N(s, 0)));
        }),
        Difficulty.Medium => F(Extra, Facts, r =>
        {
            double sy = S(r, 250, 500, 10), dT = sy / (200000 * 12e-6);
            return Q(XF("Aço com σe = {0} MPa (E = 200 GPa; α = 12·10⁻⁶ /°C), totalmente restringido. Que queda de temperatura ΔT (°C) já leva a tensão ao limite de escoamento?", "Steel with σy = {0} MPa (E = 200 GPa; α = 12·10⁻⁶ /°C), fully restrained. What temperature drop ΔT (°C) already brings the stress to the yield strength?", "Acero con σe = {0} MPa (E = 200 GPa; α = 12·10⁻⁶ /°C), totalmente restringido. ¿Qué caída de temperatura ΔT (°C) ya lleva la tensión al límite elástico?", N(sy, 0)), dT, "°C", YF("ΔT = σe / (E·α) = {0} / (200 000 · 12·10⁻⁶) = {1} °C.", N(sy, 0), N(dT, 1)));
        }),
        Difficulty.Hard => F(Extra, r =>
        {
            double e = S(r, 12, 100, 1), h = Math.Max(0.25, e / 25);
            return Q(XF("Regra didática do TTPS: patamar de 1 h para cada 25 mm de espessura (mínimo de 15 min). Para chapa de {0} mm, quantas horas de patamar?", "PWHT teaching rule: 1 h soak per 25 mm of thickness (minimum 15 min). For a {0} mm plate, how many hours of soak?", "Regla didáctica del TTPS: meseta de 1 h por cada 25 mm de espesor (mínimo 15 min). Para chapa de {0} mm, ¿cuántas horas de meseta?", N(e, 0)), h, "h", YF("t = {0} / 25 = {1} h (mínimo 0,25 h).", N(e, 0), N(e / 25, 3)));
        }, Facts),
        _ => F(r =>
        {
            double e = S(r, 25, 100, 5), tp = P(r, 580.0, 600, 620, 650), rate = P(r, 100.0, 150, 200), tot = (tp - 20) / rate + Math.Max(0.25, e / 25) + (tp - 300) / rate;
            return Q(XF("Ciclo de TTPS em chapa de {0} mm: aquece de 20 °C até {1} °C a {2} °C/h, mantém 1 h por 25 mm (mín. 15 min) e esfria a {2} °C/h só até 300 °C (daí em ar calmo). Quantas horas dura o ciclo até 300 °C?", "PWHT cycle on a {0} mm plate: heat from 20 °C to {1} °C at {2} °C/h, soak 1 h per 25 mm (min. 15 min) and cool at {2} °C/h only down to 300 °C (then in still air). How many hours does the cycle last until 300 °C?", "Ciclo de TTPS en chapa de {0} mm: calienta de 20 °C hasta {1} °C a {2} °C/h, mantiene 1 h por 25 mm (mín. 15 min) y enfría a {2} °C/h solo hasta 300 °C (luego al aire en calma). ¿Cuántas horas dura el ciclo hasta 300 °C?", N(e, 0), N(tp, 0), N(rate, 0)), tot, "h", YF("Aquecimento = ({0} − 20)/{1} = {2} h; patamar = {3} h; resfriamento = ({0} − 300)/{1} = {4} h; total = {5} h.", N(tp, 0), N(rate, 0), N((tp - 20) / rate, 3), N(Math.Max(0.25, e / 25), 3), N((tp - 300) / rate, 3), N(tot, 3)));
        }, r =>
        {
            double sy = S(r, 300, 500, 10), reduction = P(r, 0.5, 0.6, 0.7, 0.8), dT = reduction * sy / (200000 * 12e-6);
            return Q(XF("Em um aço (E = 200 GPa; α = 12·10⁻⁶ /°C) o TTPS reduz a tensão residual para {0}% do limite de escoamento de {1} MPa em vez dos 100% iniciais. Qual a tensão residual resultante, em MPa?", "In a steel (E = 200 GPa; α = 12·10⁻⁶ /°C) PWHT reduces the residual stress to {0}% of the {1} MPa yield strength instead of the initial 100%. What is the resulting residual stress, in MPa?", "En un acero (E = 200 GPa; α = 12·10⁻⁶ /°C) el TTPS reduce la tensión residual al {0}% del límite elástico de {1} MPa en vez del 100% inicial. ¿Cuál es la tensión residual resultante, en MPa?", N((1 - reduction) * 100, 0), N(sy, 0)), (1 - reduction) * sy, "MPa", YF("σ = {0}% · {1} = {2} MPa.", N((1 - reduction) * 100, 0), N(sy, 0), N((1 - reduction) * sy, 1)));
        })
    };
}
