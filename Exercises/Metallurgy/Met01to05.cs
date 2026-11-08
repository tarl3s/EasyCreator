namespace EasyCreator.Exercises;

[Exercises("met-01-estrutura")]
public sealed class Met01Structure : PoolGen
{
    static Exercise Extra(Random r)
    {
        double a = S(r, 0.25, 0.6, 0.005), A = S(r, 20, 200, 0.5); int n = P(r, 2, 4);
        double rho = n * A / (a * a * a * 1e-21 * 6.022e23);
        return Q(XF("Um metal hipotético tem célula cúbica de aresta {0} nm, {1} átomos por célula e massa atômica {2} g/mol. Qual a sua densidade? (N = 6,022·10²³ átomos/mol)", "A hypothetical metal has a cubic cell of edge {0} nm, {1} atoms per cell and atomic mass {2} g/mol. What is its density? (N = 6.022·10²³ atoms/mol)", "Un metal hipotético tiene celda cúbica de arista {0} nm, {1} átomos por celda y masa atómica {2} g/mol. ¿Cuál es su densidad? (N = 6,022·10²³ átomos/mol)", N(a, 3), n, N(A, 1)), rho, "g/cm³", YF("ρ = n·A / (a³·N) = {0}·{1} / ({2}·10⁻²¹ · 6,022·10²³) = {3} g/cm³.", n, N(A, 1), N(a * a * a, 5), N(rho, 3)));
    }

    // (pt name, en name, es name, symbol-free) crystal structures: atoms per cell and coordination number.
    static Exercise Cells(Random r)
    {
        int k = r.Next(3), cells = Ex.Int(r, 5, 900);
        int[] atoms = { 2, 4, 6 }, coord = { 8, 12, 12 };
        string name = k switch { 0 => X("cúbica de corpo centrado (CCC)", "body-centred cubic (BCC)", "cúbica centrada en el cuerpo (CCC)"), 1 => X("cúbica de faces centradas (CFC)", "face-centred cubic (FCC)", "cúbica centrada en las caras (CFC)"), _ => X("hexagonal compacta (HC)", "hexagonal close-packed (HCP)", "hexagonal compacta (HC)") };
        if (r.Next(2) == 0)
            return Q(XF("Quantos átomos pertencem a uma célula unitária da estrutura {0}?", "How many atoms belong to one unit cell of the {0} structure?", "¿Cuántos átomos pertenecen a una celda unitaria de la estructura {0}?", name) + " " + XF("(Considere {0} células iguais: quantos átomos ao todo?)", "(Consider {0} equal cells: how many atoms in total?)", "(Considere {0} celdas iguales: ¿cuántos átomos en total?)", cells), atoms[k] * cells, X("átomos", "atoms", "átomos"),
                XF("Cada célula tem {0} átomos (CCC: 1 + 8·⅛ = 2; CFC: 8·⅛ + 6·½ = 4; HC: 6). Total = {0} · {1} = {2}.", "Each cell has {0} atoms (BCC: 1 + 8·⅛ = 2; FCC: 8·⅛ + 6·½ = 4; HCP: 6). Total = {0} · {1} = {2}.", "Cada celda tiene {0} átomos (CCC: 1 + 8·⅛ = 2; CFC: 8·⅛ + 6·½ = 4; HC: 6). Total = {0} · {1} = {2}.", atoms[k], cells, atoms[k] * cells));
        return Q(XF("Qual o número de coordenação (vizinhos mais próximos de cada átomo) da estrutura {0}?", "What is the coordination number (nearest neighbours of each atom) of the {0} structure?", "¿Cuál es el número de coordinación (vecinos más cercanos de cada átomo) de la estructura {0}?", name), coord[k], "",
            X("CCC: 8 vizinhos; CFC e HC (empacotamentos compactos): 12 vizinhos.", "BCC: 8 neighbours; FCC and HCP (close-packed): 12 neighbours.", "CCC: 8 vecinos; CFC y HC (empaquetamientos compactos): 12 vecinos."));
    }

    static readonly (string pt, string en, string es, int s)[] Metals =
    {
        ("ferro (à temperatura ambiente)", "iron (at room temperature)", "hierro (a temperatura ambiente)", 0), ("cromo", "chromium", "cromo", 0), ("tungstênio", "tungsten", "wolframio", 0), ("molibdênio", "molybdenum", "molibdeno", 0),
        ("cobre", "copper", "cobre", 1), ("alumínio", "aluminium", "aluminio", 1), ("níquel", "nickel", "níquel", 1), ("ouro", "gold", "oro", 1), ("ferro acima de 912 °C (austenita)", "iron above 912 °C (austenite)", "hierro por encima de 912 °C (austenita)", 1), ("chumbo", "lead", "plomo", 1),
        ("zinco", "zinc", "zinc", 2), ("magnésio", "magnesium", "magnesio", 2), ("titânio (fase α)", "titanium (α phase)", "titanio (fase α)", 2), ("cobalto", "cobalt", "cobalto", 2)
    };

    static Exercise StructureOf(Random r)
    {
        var m = Metals[r.Next(Metals.Length)];
        string[] names = { X("CCC", "BCC", "CCC"), X("CFC", "FCC", "CFC"), X("HC", "HCP", "HC") };
        string ctx = X("Qual é a estrutura cristalina de:", "What is the crystal structure of:", "¿Cuál es la estructura cristalina de:");
        return Ex.Choice(r, ctx + " " + X(m.pt, m.en, m.es) + "?", names[m.s], names.Where((_, i) => i != m.s),
            X("Os metais CFC (Cu, Al, Ni, Au, Pb e o ferro γ) são os mais dúcteis; CCC (Fe α, Cr, W, Mo) são mais resistentes; HC (Zn, Mg, Ti α, Co) têm menos planos de deslizamento.", "FCC metals (Cu, Al, Ni, Au, Pb and γ-iron) are the most ductile; BCC (α-Fe, Cr, W, Mo) are stronger; HCP (Zn, Mg, α-Ti, Co) have fewer slip planes.", "Los metales CFC (Cu, Al, Ni, Au, Pb y el hierro γ) son los más dúctiles; CCC (Fe α, Cr, W, Mo) son más resistentes; HC (Zn, Mg, Ti α, Co) tienen menos planos de deslizamiento."));
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Cells, StructureOf, r =>
        {
            double a = S(r, 0.25, 0.6, 0.005), v = a * a * a;
            return Q(XF("Uma célula unitária cúbica tem aresta a = {0} nm. Qual o seu volume em nm³?", "A cubic unit cell has edge a = {0} nm. What is its volume in nm³?", "Una celda unitaria cúbica tiene arista a = {0} nm. ¿Cuál es su volumen en nm³?", N(a, 3)), v, "nm³", YF("V = a³ = {0}³ = {1} nm³.", N(a, 3), N(v, 5)));
        }),
        Difficulty.Medium => F(r =>
        {
            double R = S(r, 0.10, 0.20, 0.001);
            bool fcc = r.Next(2) == 0;
            double a = fcc ? 4 * R / Sqrt(2) : 4 * R / Sqrt(3);
            return Q(XF("Em uma estrutura {0}, o raio atômico é {1} nm. Qual o parâmetro de rede a (aresta da célula), em nm?", "In a {0} structure, the atomic radius is {1} nm. What is the lattice parameter a (cell edge), in nm?", "En una estructura {0}, el radio atómico es {1} nm. ¿Cuál es el parámetro de red a (arista de la celda), en nm?", fcc ? X("CFC", "FCC", "CFC") : X("CCC", "BCC", "CCC"), N(R, 3)), a, "nm",
                fcc ? YF("CFC: os átomos se tocam na diagonal da face: 4R = a√2 → a = 4R/√2 = {0} nm.", N(a, 4)) : YF("CCC: os átomos se tocam na diagonal do cubo: 4R = a√3 → a = 4R/√3 = {0} nm.", N(a, 4)));
        }, r =>
        {
            bool fcc = r.Next(2) == 0; double apf = fcc ? 0.74 : 0.68;
            double R = S(r, 0.10, 0.20, 0.001), a = fcc ? 4 * R / Sqrt(2) : 4 * R / Sqrt(3), n = fcc ? 4 : 2, vat = n * 4.0 / 3 * Pi * R * R * R, vc = a * a * a;
            return Q(XF("Em uma estrutura {0} com raio atômico {1} nm, qual o fator de empacotamento (fração do volume da célula ocupada pelos átomos)?", "In a {0} structure with atomic radius {1} nm, what is the packing factor (fraction of the cell volume filled by atoms)?", "En una estructura {0} con radio atómico {1} nm, ¿cuál es el factor de empaquetamiento (fracción del volumen de la celda ocupada por átomos)?", fcc ? X("CFC", "FCC", "CFC") : X("CCC", "BCC", "CCC"), N(R, 3)), vat / vc, "",
                YF("a = {0} nm; V átomos = {1} · (4/3)πR³ = {2} nm³; V célula = a³ = {3} nm³; FEA = {4} (≈ {5}).", N(a, 4), N(n, 0), N(vat, 5), N(vc, 5), N(vat / vc, 3), N(apf, 2)), 1.5);
        }),
        Difficulty.Hard => F(Extra, r =>
        {
            var m = new (string pt, string en, string es, double a, double A, int n)[] { ("alumínio (CFC)", "aluminium (FCC)", "aluminio (CFC)", 0.4050, 26.98, 4), ("cobre (CFC)", "copper (FCC)", "cobre (CFC)", 0.3615, 63.55, 4), ("ferro α (CCC)", "α-iron (BCC)", "hierro α (CCC)", 0.2866, 55.85, 2), ("níquel (CFC)", "nickel (FCC)", "níquel (CFC)", 0.3524, 58.69, 4), ("cromo (CCC)", "chromium (BCC)", "cromo (CCC)", 0.2884, 52.00, 2), ("ouro (CFC)", "gold (FCC)", "oro (CFC)", 0.4078, 196.97, 4), ("tungstênio (CCC)", "tungsten (BCC)", "wolframio (CCC)", 0.3165, 183.84, 2) }[r.Next(7)];
            double rho = m.n * m.A / (Pow3(m.a * 1e-7) * 6.022e23);
            return Q(XF("Calcule a densidade teórica do {0}: parâmetro de rede a = {1} nm, massa atômica {2} g/mol (N = 6,022·10²³ átomos/mol).", "Calculate the theoretical density of {0}: lattice parameter a = {1} nm, atomic mass {2} g/mol (N = 6.022·10²³ atoms/mol).", "Calcule la densidad teórica del {0}: parámetro de red a = {1} nm, masa atómica {2} g/mol (N = 6,022·10²³ átomos/mol).", X(m.pt, m.en, m.es), N(m.a, 4), N(m.A, 2)), rho, "g/cm³",
                YF("ρ = n·A / (a³·N) = {0}·{1} / ({2}·10⁻²¹ cm³ · 6,022·10²³) = {3} g/cm³ (a = {4}·10⁻⁷ cm).", m.n, N(m.A, 2), N(Pow3(m.a * 1e-7) * 1e21, 4), N(rho, 3), N(m.a, 4)));
        }),
        _ => F(r =>
        {
            double d0 = S(r, 5, 100, 1), s0 = 70, k = 0.74, sy = s0 + k / Sqrt(d0 * 1e-6);
            return Q(XF("Para um aço de baixo carbono, σe = σ₀ + k/√d com σ₀ = 70 MPa e k = 0,74 MPa·√m (Hall-Petch). Qual o limite de escoamento com grãos de {0} µm?", "For a low-carbon steel, σy = σ₀ + k/√d with σ₀ = 70 MPa and k = 0.74 MPa·√m (Hall-Petch). What is the yield strength with {0} µm grains?", "Para un acero de bajo carbono, σe = σ₀ + k/√d con σ₀ = 70 MPa y k = 0,74 MPa·√m (Hall-Petch). ¿Cuál es el límite elástico con granos de {0} µm?", N(d0, 0)), sy, "MPa",
                YF("d = {0}·10⁻⁶ m; k/√d = 0,74 / {1} = {2} MPa; σe = 70 + {2} = {3} MPa.", N(d0, 0), N(Sqrt(d0 * 1e-6), 5), N(k / Sqrt(d0 * 1e-6), 2), N(sy, 1)));
        }, r =>
        {
            double d0 = S(r, 5, 60, 1), s0 = 70, k = 0.74, sy = s0 + k / Sqrt(d0 * 1e-6), want = Math.Round(sy);
            double dd = Pow2(k / (want - s0)) * 1e6;
            return Q(XF("Para um aço de baixo carbono, σe = σ₀ + k/√d com σ₀ = 70 MPa e k = 0,74 MPa·√m. Que tamanho de grão, em µm, dá um limite de escoamento de {0} MPa?", "For a low-carbon steel, σy = σ₀ + k/√d with σ₀ = 70 MPa and k = 0.74 MPa·√m. What grain size, in µm, gives a yield strength of {0} MPa?", "Para un acero de bajo carbono, σe = σ₀ + k/√d con σ₀ = 70 MPa y k = 0,74 MPa·√m. ¿Qué tamaño de grano, en µm, da un límite elástico de {0} MPa?", N(want, 0)), dd, "µm",
                YF("√d = k/(σe − σ₀) = 0,74/({0} − 70) = {1}·10⁻³ √m; d = {2} µm.", N(want, 0), N(k / (want - s0) * 1000, 4), N(dd, 2)), 3);
        }, r =>
        {
            int g = Ex.Int(r, 2, 10); double n = Math.Pow(2, g - 1);
            return Q(XF("Pela norma ASTM, o número de grãos por polegada quadrada a 100× é N = 2^(G−1). Quantos grãos por polegada quadrada tem um aço de tamanho de grão ASTM {0}?", "By the ASTM standard, the number of grains per square inch at 100× is N = 2^(G−1). How many grains per square inch does a steel of ASTM grain size {0} have?", "Según la norma ASTM, el número de granos por pulgada cuadrada a 100× es N = 2^(G−1). ¿Cuántos granos por pulgada cuadrada tiene un acero de tamaño de grano ASTM {0}?", g), n, X("grãos/pol²", "grains/in²", "granos/pulg²"),
                YF("N = 2^(G−1) = 2^({0}) = {1}.", g - 1, N(n, 0)));
        })
    };
    static double Pow2(double v) => v * v;
    static double Pow3(double v) => v * v * v;
}

[Exercises("met-02-tracao")]
public sealed class Met02Tensile : PoolGen
{
    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(r =>
        {
            double dm = S(r, 5, 30, 0.5), F = S(r, 5, 150, 1), a = Pi * dm * dm / 4, s = F * 1000 / a;
            return Q(XF("Uma barra redonda de {0} mm de diâmetro é puxada com {1} kN. Qual a tensão normal na barra?", "A round bar {0} mm in diameter is pulled with {1} kN. What is the normal stress in the bar?", "Una barra redonda de {0} mm de diámetro se tira con {1} kN. ¿Cuál es la tensión normal en la barra?", N(dm), N(F, 0)), s, "MPa", YF("A₀ = π·d²/4 = {0} mm²; σ = F/A₀ = {1} N / {0} mm² = {2} MPa.", N(a, 2), N(F * 1000, 0), N(s, 1)));
        }, r =>
        {
            double w = S(r, 10, 30, 1), t = S(r, 2, 12, 0.5), F = S(r, 10, 150, 1), a = w * t, s = F * 1000 / a;
            return Q(XF("Um corpo de prova retangular de {0} mm × {1} mm rompe com {2} kN. Qual o limite de resistência σr?", "A rectangular specimen {0} mm × {1} mm breaks at {2} kN. What is the tensile strength σr?", "Una probeta rectangular de {0} mm × {1} mm se rompe con {2} kN. ¿Cuál es el límite de resistencia σr?", N(w, 0), N(t), N(F, 0)), s, "MPa", YF("A₀ = {0} · {1} = {2} mm²; σr = {3} / {2} = {4} MPa.", N(w, 0), N(t), N(a, 1), N(F * 1000, 0), N(s, 1)));
        }, r =>
        {
            double l0 = P(r, 50.0, 100, 200), dl = S(r, 0.5, 20, 0.5), e = dl / l0 * 100;
            return Q(XF("Um corpo de prova com base de medida L₀ = {0} mm alonga {1} mm antes de romper (medido sobre a base). Qual o alongamento percentual?", "A specimen with gauge length L₀ = {0} mm elongates {1} mm before breaking (measured on the gauge). What is the percentage elongation?", "Una probeta con longitud de medida L₀ = {0} mm se alarga {1} mm antes de romperse (medido sobre la base). ¿Cuál es el alargamiento porcentual?", N(l0, 0), N(dl)), e, "%", YF("A = ΔL / L₀ · 100 = {0} / {1} · 100 = {2}%.", N(dl), N(l0, 0), N(e, 2)));
        }),
        Difficulty.Medium => F(r =>
        {
            double s = S(r, 20, 400, 5), e = 0.001 * S(r, 0.1, 2, 0.05), E = s / e / 1000;
            return Q(XF("Na região elástica de um metal, a tensão de {0} MPa produz uma deformação de {1}‰ (milésimos). Qual o módulo de elasticidade?", "In the elastic region of a metal, a stress of {0} MPa produces a strain of {1}‰ (thousandths). What is the elastic modulus?", "En la región elástica de un metal, la tensión de {0} MPa produce una deformación de {1}‰ (milésimas). ¿Cuál es el módulo de elasticidad?", N(s, 0), N(e * 1000, 2)), E, "GPa", YF("E = σ/ε = {0} MPa / {1} = {2} GPa.", N(s, 0), N(e, 5), N(E, 1)));
        }, r =>
        {
            double d0 = S(r, 6, 14, 0.5), df = d0 * S(r, 0.5, 0.9, 0.01), z = (1 - df * df / (d0 * d0)) * 100;
            return Q(XF("Uma barra de {0} mm de diâmetro rompe com diâmetro mínimo de {1} mm na região de estricção. Qual a redução de área?", "A bar {0} mm in diameter breaks with a minimum diameter of {1} mm in the necked region. What is the reduction of area?", "Una barra de {0} mm de diámetro se rompe con un diámetro mínimo de {1} mm en la zona de estricción. ¿Cuál es la reducción de área?", N(d0), N(df, 2)), z, "%", YF("RA = (A₀ − Af)/A₀ = 1 − (df/d₀)² = 1 − ({0}/{1})² = {2}%.", N(df, 2), N(d0), N(z, 2)));
        }, r =>
        {
            double s = S(r, 50, 400, 10), L = S(r, 0.5, 5, 0.1), dl = s * L * 1000 / 200000;
            return Q(XF("Uma barra de aço (E = 200 GPa) de {0} m trabalha com tensão de {1} MPa (dentro do regime elástico). Quantos mm ela se alonga?", "A steel bar (E = 200 GPa) {0} m long works at a stress of {1} MPa (within the elastic regime). How many mm does it stretch?", "Una barra de acero (E = 200 GPa) de {0} m trabaja con una tensión de {1} MPa (dentro del régimen elástico). ¿Cuántos mm se alarga?", N(L), N(s, 0)), dl, "mm", YF("ΔL = σ·L / E = {0} · {1}·10³ mm / 200 000 = {2} mm.", N(s, 0), N(L), N(dl, 3)));
        }),
        Difficulty.Hard => F(r =>
        {
            double sy = P(r, 250.0, 350, 450, 550), dm = S(r, 8, 30, 1), a = Pi * dm * dm / 4, F = sy * a / 1000;
            return Q(XF("Uma barra de aço com limite de escoamento σe = {0} MPa e {1} mm de diâmetro. Que força, em kN, inicia o escoamento?", "A steel bar with yield strength σy = {0} MPa and {1} mm in diameter. What force, in kN, starts yielding?", "Una barra de acero con límite elástico σe = {0} MPa y {1} mm de diámetro. ¿Qué fuerza, en kN, inicia la fluencia?", N(sy, 0), N(dm, 0)), F, "kN", YF("A₀ = {0} mm²; F = σe·A₀ = {1} · {0} = {2} kN.", N(a, 1), N(sy, 0), N(F, 2)));
        }, r =>
        {
            double F = S(r, 20, 200, 5), sy = P(r, 250.0, 350, 420), sf = P(r, 1.5, 2.0, 2.5), a = F * 1000 * sf / sy, dm = Sqrt(4 * a / Pi);
            return Q(XF("Um tirante de aço (σe = {0} MPa) deve suportar {1} kN com coeficiente de segurança {2} em relação ao escoamento. Qual o diâmetro mínimo, em mm?", "A steel tie rod (σy = {0} MPa) must carry {1} kN with a safety factor of {2} against yielding. What is the minimum diameter, in mm?", "Un tirante de acero (σe = {0} MPa) debe soportar {1} kN con coeficiente de seguridad {2} respecto a la fluencia. ¿Cuál es el diámetro mínimo, en mm?", N(sy, 0), N(F, 0), N(sf)), dm, "mm", YF("σadm = {0}/{1} = {2} MPa; A = F/σadm = {3} mm²; d = √(4A/π) = {4} mm.", N(sy, 0), N(sf), N(sy / sf, 1), N(a, 1), N(dm, 2)));
        }),
        _ => F(r =>
        {
            double s = S(r, 200, 600, 5), e = S(r, 2, 25, 0.5), st = s * (1 + e / 100);
            return Q(XF("Em um ensaio, a tensão de engenharia é {0} MPa para uma deformação de engenharia de {1}%. Qual a tensão verdadeira nesse ponto (antes da estricção)?", "In a test, the engineering stress is {0} MPa at an engineering strain of {1}%. What is the true stress at that point (before necking)?", "En un ensayo, la tensión de ingeniería es {0} MPa para una deformación de ingeniería de {1}%. ¿Cuál es la tensión verdadera en ese punto (antes de la estricción)?", N(s, 0), N(e)), st, "MPa", YF("σv = σ·(1 + ε) = {0} · (1 + {1}) = {2} MPa (o volume é constante, a área diminui).", N(s, 0), N(e / 100, 4), N(st, 1)));
        }, r =>
        {
            double e = S(r, 2, 30, 0.5), ev = Ln(1 + e / 100) * 100;
            return Q(XF("Uma deformação de engenharia de {0}% corresponde a que deformação verdadeira, em %?", "An engineering strain of {0}% corresponds to what true strain, in %?", "Una deformación de ingeniería de {0}% corresponde a ¿qué deformación verdadera, en %?", N(e)), ev, "%", YF("εv = ln(1 + ε) = ln(1 + {0}) = {1}%.", N(e / 100, 4), N(ev, 3)));
        }, r =>
        {
            double F = S(r, 20, 100, 5), dm = S(r, 8, 20, 1), df = dm * S(r, 0.5, 0.8, 0.01), sv = F * 1000 / (Pi * df * df / 4);
            return Q(XF("Na ruptura de uma barra de {0} mm de diâmetro, a força é {1} kN e o diâmetro na seção de ruptura é {2} mm. Qual a tensão verdadeira de ruptura?", "At fracture of a {0} mm diameter bar, the force is {1} kN and the diameter at the fracture section is {2} mm. What is the true fracture stress?", "En la rotura de una barra de {0} mm de diámetro, la fuerza es {1} kN y el diámetro en la sección de rotura es {2} mm. ¿Cuál es la tensión verdadera de rotura?", N(dm, 0), N(F, 0), N(df, 2)), sv, "MPa", YF("Af = π·{0}²/4 = {1} mm²; σv = F/Af = {2} / {1} = {3} MPa.", N(df, 2), N(Pi * df * df / 4, 2), N(F * 1000, 0), N(sv, 1)));
        })
    };
}

[Exercises("met-03-ferro-carbono")]
public sealed class Met03IronCarbon : PoolGen
{
    const double Ce = 0.022, Cp = 0.76, Cc = 6.70;

    static Exercise Classify(Random r)
    {
        double c = S(r, 0.05, 1.5, 0.01);
        string right = c < 0.76 - 0.001 ? "hipo" : c > 0.76 + 0.001 ? "hiper" : "eut";
        if (Math.Abs(c - 0.76) < 0.015) { c = 0.76; right = "eut"; }
        string[] pt = { X("hipoeutetoide (ferrita + perlita)", "hypoeutectoid (ferrite + pearlite)", "hipoeutectoide (ferrita + perlita)"), X("eutetoide (100% perlita)", "eutectoid (100% pearlite)", "eutectoide (100% perlita)"), X("hipereutetoide (perlita + cementita)", "hypereutectoid (pearlite + cementite)", "hipereutectoide (perlita + cementita)") };
        int i = right == "hipo" ? 0 : right == "eut" ? 1 : 2;
        return Ex.Choice(r, XF("Um aço com {0}% de carbono, lentamente resfriado, é classificado como:", "A steel with {0}% carbon, slowly cooled, is classified as:", "Un acero con {0}% de carbono, enfriado lentamente, se clasifica como:", N(c, 2)), pt[i], pt.Where((_, k) => k != i),
            XF("O ponto eutetoide está em 0,76% C: abaixo é hipoeutetoide, igual é eutetoide (perlita pura) e acima é hipereutetoide.", "The eutectoid point is at 0.76% C: below it is hypoeutectoid, equal is eutectoid (pure pearlite) and above is hypereutectoid.", "El punto eutectoide está en 0,76% C: por debajo es hipoeutectoide, igual es eutectoide (perlita pura) y por encima es hipereutectoide."));
    }

    static Exercise Facts(Random r)
    {
        var bank = new (string q, string right, string[] wrong)[]
        {
            (X("Qual a temperatura da transformação eutetoide do sistema Fe-C?", "What is the eutectoid transformation temperature of the Fe-C system?", "¿Cuál es la temperatura de la transformación eutectoide del sistema Fe-C?"), "727 °C", new[] { "912 °C", "1148 °C", "1538 °C", "570 °C" }),
            (X("Qual a temperatura de fusão do ferro puro?", "What is the melting temperature of pure iron?", "¿Cuál es la temperatura de fusión del hierro puro?"), "1538 °C", new[] { "727 °C", "912 °C", "1148 °C", "1394 °C" }),
            (X("Em que temperatura o ferro puro passa de CCC (α) para CFC (γ)?", "At what temperature does pure iron change from BCC (α) to FCC (γ)?", "¿A qué temperatura el hierro puro pasa de CCC (α) a CFC (γ)?"), "912 °C", new[] { "727 °C", "1148 °C", "1394 °C", "770 °C" }),
            (X("Quanto carbono dissolve, no máximo, a austenita (a 1148 °C)?", "What is the maximum carbon the austenite can dissolve (at 1148 °C)?", "¿Cuál es el máximo de carbono que disuelve la austenita (a 1148 °C)?"), "2,11%", new[] { "0,022%", "0,76%", "4,3%", "6,7%" }),
            (X("Quanto carbono dissolve, no máximo, a ferrita α (a 727 °C)?", "What is the maximum carbon the α-ferrite can dissolve (at 727 °C)?", "¿Cuál es el máximo de carbono que disuelve la ferrita α (a 727 °C)?"), "0,022%", new[] { "0,76%", "2,11%", "4,3%", "6,7%" }),
            (X("Qual a composição da cementita (Fe₃C), em % de carbono?", "What is the composition of cementite (Fe₃C), in % carbon?", "¿Cuál es la composición de la cementita (Fe₃C), en % de carbono?"), "6,7%", new[] { "0,76%", "2,11%", "4,3%", "0,022%" }),
            (X("O que é a perlita?", "What is pearlite?", "¿Qué es la perlita?"), X("Mistura lamelar de ferrita e cementita", "A lamellar mixture of ferrite and cementite", "Mezcla laminar de ferrita y cementita"), new[] { X("Solução sólida de carbono no ferro γ", "Solid solution of carbon in γ-iron", "Solución sólida de carbono en hierro γ"), X("Carboneto de ferro puro (Fe₃C)", "Pure iron carbide (Fe₃C)", "Carburo de hierro puro (Fe₃C)"), X("Estrutura formada só por resfriamento brusco", "A structure formed only by sudden cooling", "Estructura formada solo por enfriamiento brusco") }),
            (X("Acima de 2,11% de carbono a liga Fe-C é chamada de:", "Above 2.11% carbon the Fe-C alloy is called:", "Por encima de 2,11% de carbono la aleación Fe-C se llama:"), X("ferro fundido", "cast iron", "hierro fundido"), new[] { X("aço baixo carbono", "low-carbon steel", "acero de bajo carbono"), X("aço eutetoide", "eutectoid steel", "acero eutectoide"), X("ferro puro", "pure iron", "hierro puro") }),
        };
        var b = bank[r.Next(bank.Length)];
        return Fact(r, b.q, b.right, X("Valores do diagrama Fe-Fe₃C: eutetoide 727 °C e 0,76% C; máxima solubilidade 0,022% (ferrita) e 2,11% (austenita); cementita 6,7% C; ferro funde a 1538 °C; α→γ a 912 °C.", "Fe-Fe₃C diagram values: eutectoid 727 °C and 0.76% C; maximum solubility 0.022% (ferrite) and 2.11% (austenite); cementite 6.7% C; iron melts at 1538 °C; α→γ at 912 °C.", "Valores del diagrama Fe-Fe₃C: eutectoide 727 °C y 0,76% C; máxima solubilidad 0,022% (ferrita) y 2,11% (austenita); cementita 6,7% C; el hierro funde a 1538 °C; α→γ a 912 °C."), b.wrong);
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Classify, Facts, r =>
        {
            double c = S(r, 0.1, 0.7, 0.01), p = (c - Ce) / (Cp - Ce) * 100;
            return Q(XF("Um aço com {0}% de C é resfriado lentamente. Qual a fração aproximada de perlita (regra da alavanca, ferrita 0,022% e perlita 0,76%)?", "A steel with {0}% C is cooled slowly. What is the approximate fraction of pearlite (lever rule, ferrite 0.022% and pearlite 0.76%)?", "Un acero con {0}% de C se enfría lentamente. ¿Cuál es la fracción aproximada de perlita (regla de la palanca, ferrita 0,022% y perlita 0,76%)?", N(c, 2)), p, "%", YF("W perlita = (C − 0,022)/(0,76 − 0,022) = ({0} − 0,022)/0,738 = {1}%.", N(c, 2), N(p, 1)));
        }),
        Difficulty.Medium => F(r =>
        {
            double c = S(r, 0.1, 0.7, 0.01), f = (Cp - c) / (Cp - Ce) * 100;
            return Q(XF("Um aço de {0}% de C esfria lentamente até abaixo de 727 °C. Quanto de ferrita proeutetoide (formada antes da perlita) existe, em %? (regra da alavanca entre 0,022% e 0,76%)", "A steel with {0}% C is cooled slowly below 727 °C. How much proeutectoid ferrite (formed before the pearlite) is there, in %? (lever rule between 0.022% and 0.76%)", "Un acero de {0}% de C se enfría lentamente por debajo de 727 °C. ¿Cuánta ferrita proeutectoide (formada antes de la perlita) hay, en %? (regla de la palanca entre 0,022% y 0,76%)", N(c, 2)), f, "%", YF("W ferrita pro. = (0,76 − C)/(0,76 − 0,022) = (0,76 − {0})/0,738 = {1}%.", N(c, 2), N(f, 1)));
        }, r =>
        {
            double c = S(r, 0.9, 1.5, 0.01), f = (c - Cp) / (Cc - Cp) * 100;
            return Q(XF("Um aço hipereutetoide de {0}% de C esfria lentamente. Qual a fração de cementita proeutetoide, em %? (perlita 0,76% C; cementita 6,70% C)", "A hypereutectoid steel with {0}% C is cooled slowly. What is the proeutectoid cementite fraction, in %? (pearlite 0.76% C; cementite 6.70% C)", "Un acero hipereutectoide de {0}% de C se enfría lentamente. ¿Cuál es la fracción de cementita proeutectoide, en %? (perlita 0,76% C; cementita 6,70% C)", N(c, 2)), f, "%", YF("W Fe₃C pro. = (C − 0,76)/(6,70 − 0,76) = ({0} − 0,76)/5,94 = {1}%.", N(c, 2), N(f, 2)));
        }, r =>
        {
            double c = S(r, 0.1, 0.7, 0.01), p = (c - Ce) / (Cp - Ce) * 100;
            return Q(XF("Em um aço de {0}% de C esfriado lentamente, qual a fração de ferrita proeutetoide + a ferrita dentro da perlita? Ou seja, a fração TOTAL de ferrita (ferrita 0,022% C; cementita 6,70% C), em %?", "In a slowly cooled steel with {0}% C, what is the TOTAL ferrite fraction (proeutectoid ferrite plus the ferrite inside the pearlite) (ferrite 0.022% C; cementite 6.70% C), in %?", "En un acero de {0}% de C enfriado lentamente, ¿cuál es la fracción TOTAL de ferrita (ferrita proeutectoide más la ferrita dentro de la perlita) (ferrita 0,022% C; cementita 6,70% C), en %?", N(c, 2)), (Cc - c) / (Cc - Ce) * 100, "%", YF("W α = (6,70 − C)/(6,70 − 0,022) = (6,70 − {0})/6,678 = {1}%. (Cementita total = {2}%; perlita = {3}%.)", N(c, 2), N((Cc - c) / (Cc - Ce) * 100, 2), N((c - Ce) / (Cc - Ce) * 100, 2), N(p, 1)));
        }),
        Difficulty.Hard => F(r =>
        {
            double c = S(r, 0.1, 0.7, 0.01), f = (Cp - c) / (Cp - Ce) * 100, p = 100 - f;
            return Q(XF("Uma micrografia de aço recozido mostra {0}% de área de perlita (o resto é ferrita). Qual o teor de carbono do aço? (perlita 0,76% C; ferrita 0,022% C)", "A micrograph of annealed steel shows {0}% pearlite area (the rest is ferrite). What is the steel's carbon content? (pearlite 0.76% C; ferrite 0.022% C)", "Una micrografía de acero recocido muestra {0}% de área de perlita (el resto es ferrita). ¿Cuál es el contenido de carbono del acero? (perlita 0,76% C; ferrita 0,022% C)", N(p, 0)), c, "%", YF("p = (C − 0,022)/0,738 → C = 0,022 + {0}·0,738 = {1}%.", N(p / 100, 3), N(Ce + p / 100 * (Cp - Ce), 3)), 3);
        }, r =>
        {
            double c = S(r, 0.9, 1.5, 0.01), p = (Cc - c) / (Cc - Cp) * 100;
            return Q(XF("Um aço de {0}% de C (hipereutetoide) é resfriado lentamente. Quanto de perlita ele tem, em %? (perlita 0,76% C; cementita 6,70% C)", "A hypereutectoid steel with {0}% C is cooled slowly. How much pearlite does it have, in %? (pearlite 0.76% C; cementite 6.70% C)", "Un acero hipereutectoide de {0}% de C se enfría lentamente. ¿Cuánta perlita tiene, en %? (perlita 0,76% C; cementita 6,70% C)", N(c, 2)), p, "%", YF("W perlita = (6,70 − C)/(6,70 − 0,76) = (6,70 − {0})/5,94 = {1}%.", N(c, 2), N(p, 2)));
        }),
        _ => F(r =>
        {
            double c = S(r, 0.9, 1.6, 0.01), pro = (c - Cp) / (Cc - Cp) * 100, total = (c - Ce) / (Cc - Ce) * 100;
            return Q(XF("Um aço de {0}% de C esfria lentamente. Qual a fração TOTAL de cementita (proeutetoide + a das lamelas de perlita), em %? (ferrita 0,022%; cementita 6,70%)", "A steel with {0}% C is cooled slowly. What is the TOTAL cementite fraction (proeutectoid + that in the pearlite lamellae), in %? (ferrite 0.022%; cementite 6.70%)", "Un acero de {0}% de C se enfría lentamente. ¿Cuál es la fracción TOTAL de cementita (proeutectoide + la de las láminas de perlita), en %? (ferrita 0,022%; cementita 6,70%)", N(c, 2)), total, "%", YF("W Fe₃C total = (C − 0,022)/(6,70 − 0,022) = ({0} − 0,022)/6,678 = {1}% (sendo {2}% proeutetoide).", N(c, 2), N(total, 2), N(pro, 2)));
        }, r =>
        {
            double c = S(r, 0.2, 0.7, 0.01), fpro = (Cp - c) / (Cp - Ce) * 100;
            double want = Math.Round(fpro);
            double c2 = Cp - want / 100 * (Cp - Ce);
            return Q(XF("Quer-se um aço recozido com {0}% de ferrita proeutetoide (o resto, perlita). Que teor de carbono ele deve ter, em %? (ferrita 0,022%; perlita 0,76%)", "An annealed steel with {0}% proeutectoid ferrite (the rest pearlite) is wanted. What carbon content must it have, in %? (ferrite 0.022%; pearlite 0.76%)", "Se quiere un acero recocido con {0}% de ferrita proeutectoide (el resto, perlita). ¿Qué contenido de carbono debe tener, en %? (ferrita 0,022%; perlita 0,76%)", N(want, 0)), c2, "%", YF("Wα = (0,76 − C)/0,738 → C = 0,76 − {0}·0,738 = {1}%.", N(want / 100, 2), N(c2, 3)), 3);
        })
    };
}

[Exercises("met-04-tratamentos")]
public sealed class Met04HeatTreatments : PoolGen
{
    static Exercise Which(Random r)
    {
        var bank = new (string goal, string right)[]
        {
            (X("Obter a máxima dureza do aço (estrutura martensítica)", "Get the maximum hardness of a steel (martensitic structure)", "Obtener la máxima dureza del acero (estructura martensítica)"), "q"),
            (X("Diminuir a fragilidade e as tensões de uma peça já temperada", "Reduce the brittleness and stresses of an already quenched part", "Disminuir la fragilidad y las tensiones de una pieza ya templada"), "t"),
            (X("Amolecer o aço ao máximo para facilitar a usinagem ou a conformação", "Soften the steel as much as possible to ease machining or forming", "Ablandar el acero al máximo para facilitar el mecanizado o la conformación"), "a"),
            (X("Refinar o grão e uniformizar a estrutura de uma peça forjada, resfriando ao ar", "Refine the grain and even out the structure of a forged part by cooling in air", "Refinar el grano y uniformizar la estructura de una pieza forjada, enfriando al aire"), "n"),
            (X("Aliviar tensões internas e deixar o aço macio e dúctil com resfriamento lento no forno", "Relieve internal stresses and leave the steel soft and ductile with slow cooling in the furnace", "Aliviar tensiones internas y dejar el acero blando y dúctil con enfriamiento lento en el horno"), "a"),
            (X("Combinar dureza e tenacidade em eixos e molas, aquecendo a martensita a 400-600 °C", "Combine hardness and toughness in shafts and springs by heating martensite to 400-600 °C", "Combinar dureza y tenacidad en ejes y resortes, calentando la martensita a 400-600 °C"), "t"),
        };
        var g = bank[r.Next(bank.Length)];
        var names = new[] { ("q", X("Têmpera", "Quenching", "Temple")), ("t", X("Revenimento", "Tempering", "Revenido")), ("a", X("Recozimento", "Annealing", "Recocido")), ("n", X("Normalização", "Normalizing", "Normalizado")) };
        string right = names.First(n => n.Item1 == g.right).Item2;
        return Ex.Choice(r, XF("Qual tratamento térmico serve para: {0}?", "Which heat treatment is used to: {0}?", "¿Qué tratamiento térmico sirve para: {0}?", g.goal), right, names.Where(n => n.Item1 != g.right).Select(n => n.Item2),
            X("Têmpera: austenitizar e resfriar rápido (martensita dura e frágil). Revenimento: reaquecer a martensita abaixo de 727 °C. Recozimento: resfriar lentamente no forno (macio). Normalização: resfriar ao ar (grão fino e uniforme).", "Quenching: austenitise and cool fast (hard, brittle martensite). Tempering: reheat martensite below 727 °C. Annealing: cool slowly in the furnace (soft). Normalizing: cool in air (fine, even grain).", "Temple: austenizar y enfriar rápido (martensita dura y frágil). Revenido: recalentar la martensita por debajo de 727 °C. Recocido: enfriar lentamente en el horno (blando). Normalizado: enfriar al aire (grano fino y uniforme)."));
    }

    static double Lerp(double t)
    {
        double[] ts = { 20, 200, 400, 600 }, hs = { 57, 54, 45, 28 };
        for (int i = 0; i < 3; i++) if (t <= ts[i + 1]) return hs[i] + (hs[i + 1] - hs[i]) * (t - ts[i]) / (ts[i + 1] - ts[i]);
        return hs[3];
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Which, r =>
        {
            double t = S(r, 20, 600, 5), h = Lerp(t);
            return Q(XF("Um aço 1045 temperado em água (57 HRC, que vale como revenido a 20 °C) tem dureza 54 HRC revenido a 200 °C, 45 HRC a 400 °C e 28 HRC a 600 °C. Interpolando linearmente, qual a dureza revenido a {0} °C?", "A 1045 steel quenched in water (57 HRC, counted as tempered at 20 °C) has 54 HRC when tempered at 200 °C, 45 HRC at 400 °C and 28 HRC at 600 °C. By linear interpolation, what is the hardness when tempered at {0} °C?", "Un acero 1045 templado en agua (57 HRC, que cuenta como revenido a 20 °C) tiene 54 HRC revenido a 200 °C, 45 HRC a 400 °C y 28 HRC a 600 °C. Interpolando linealmente, ¿cuál es la dureza revenida a {0} °C?", N(t, 0)), h, "HRC", X("Interpole entre os dois pontos da tabela mais próximos (aqui: ponto a ponto, reta entre eles).", "Interpolate between the two nearest table points (a straight line between them).", "Interpole entre los dos puntos de la tabla más cercanos (una recta entre ellos)."), 3);
        }),
        Difficulty.Medium => F(r =>
        {
            double t = S(r, 150, 700, 5), th = Pick(r), p = (t + 273) * (20 + Log(th));
            return Q(XF("Parâmetro de Hollomon–Jaffe: P = T(K)·(20 + log t), t em horas. Qual o valor de P para um revenimento a {0} °C durante {1} h?", "Hollomon–Jaffe parameter: P = T(K)·(20 + log t), t in hours. What is P for tempering at {0} °C for {1} h?", "Parámetro de Hollomon–Jaffe: P = T(K)·(20 + log t), t en horas. ¿Cuál es el valor de P para un revenido a {0} °C durante {1} h?", N(t, 0), N(th)), p, "", YF("T = {0} + 273 = {1} K; P = {1}·(20 + log {2}) = {3}.", N(t, 0), N(t + 273, 0), N(th), N(p, 0)));
        }),
        Difficulty.Hard => F(r =>
        {
            double t1 = S(r, 300, 600, 5), h1 = P(r, 1.0, 2, 4, 8), t2 = t1 + P(r, 20.0, 30, 40, 50), p = (t1 + 273) * (20 + Log(h1)), th2 = Math.Pow(10, p / (t2 + 273) - 20);
            return Q(XF("Um revenimento a {0} °C por {1} h dá certa dureza. Quantas horas são necessárias a {2} °C para a mesma dureza (mesmo parâmetro de Hollomon–Jaffe, C = 20)?", "Tempering at {0} °C for {1} h gives a certain hardness. How many hours are needed at {2} °C for the same hardness (same Hollomon–Jaffe parameter, C = 20)?", "Un revenido a {0} °C durante {1} h da cierta dureza. ¿Cuántas horas se necesitan a {2} °C para la misma dureza (mismo parámetro de Hollomon–Jaffe, C = 20)?", N(t1, 0), N(h1), N(t2, 0)), th2, "h", YF("P = {0}·(20 + log {1}) = {2}; log t₂ = P/{3} − 20 = {4}; t₂ = {5} h.", N(t1 + 273, 0), N(h1), N(p, 0), N(t2 + 273, 0), N(p / (t2 + 273) - 20, 4), N(th2, 4)), 3);
        }),
        _ => F(r =>
        {
            double t1 = S(r, 300, 550, 5), h1 = P(r, 1.0, 2, 4), h2 = P(r, 0.25, 0.5, 8, 16), p = (t1 + 273) * (20 + Log(h1)), t2 = p / (20 + Log(h2)) - 273;
            return Q(XF("Para obter a mesma dureza que {0} °C por {1} h dá, em um revenimento de apenas {2} h, que temperatura (°C) é necessária? (Hollomon–Jaffe, C = 20)", "To get the same hardness as {0} °C for {1} h gives, in a tempering of only {2} h, what temperature (°C) is needed? (Hollomon–Jaffe, C = 20)", "Para obtener la misma dureza que da {0} °C durante {1} h, en un revenido de solo {2} h, ¿qué temperatura (°C) se necesita? (Hollomon–Jaffe, C = 20)", N(t1, 0), N(h1), N(h2)), t2, "°C", YF("P = {0}·(20 + log {1}) = {2}; T = P/(20 + log {3}) = {4} K = {5} °C.", N(t1 + 273, 0), N(h1), N(p, 0), N(h2), N(t2 + 273, 1), N(t2, 1)), 1);
        }, r =>
        {
            double h = S(r, 30, 52, 1), t = h > 45 ? 400 - (h - 45) / (54 - 45) * 0 : 0;
            double[] ts = { 20, 200, 400, 600 }, hs = { 57, 54, 45, 28 };
            int k = 0; while (k < 2 && h < hs[k + 1]) k++;
            t = ts[k] + (h - hs[k]) / (hs[k + 1] - hs[k]) * (ts[k + 1] - ts[k]);
            return Q(XF("Um aço 1045 temperado (57 HRC) tem 54 HRC revenido a 200 °C, 45 HRC a 400 °C e 28 HRC a 600 °C. Por interpolação linear, a que temperatura ele deve ser revenido para ficar com {0} HRC?", "A quenched 1045 steel (57 HRC) has 54 HRC tempered at 200 °C, 45 HRC at 400 °C and 28 HRC at 600 °C. By linear interpolation, at what temperature must it be tempered to reach {0} HRC?", "Un acero 1045 templado (57 HRC) tiene 54 HRC revenido a 200 °C, 45 HRC a 400 °C y 28 HRC a 600 °C. Por interpolación lineal, ¿a qué temperatura debe revenirse para quedar con {0} HRC?", N(h, 0)), t, "°C", X("Procure o trecho da tabela que contém a dureza pedida e interpole a temperatura.", "Find the table segment that contains the requested hardness and interpolate the temperature.", "Busque el tramo de la tabla que contiene la dureza pedida e interpole la temperatura."), 3);
        })
    };
    static double Pick(Random r) => Ex.Pick(r, 0.5, 1.0, 1.5, 2.0, 3.0, 4.0, 6.0, 8.0);
}

[Exercises("met-05-seguranca")]
public sealed class Met05Safety : PoolGen
{
    static Exercise Extra(Random r)
    {
        double i = S(r, 60, 400, 5), u = S(r, 18, 40, 0.5);
        return Q(XF("Um arco de {0} V com corrente de {1} A. Qual a potência elétrica do arco, em kW?", "An arc of {0} V at a current of {1} A. What is the arc's electrical power, in kW?", "Un arco de {0} V con corriente de {1} A. ¿Cuál es la potencia eléctrica del arco, en kW?", N(u), N(i, 0)), u * i / 1000, "kW", YF("P = U·I = {0} · {1} = {2} kW.", N(u), N(i, 0), N(u * i / 1000, 3)));
    }

    static Exercise Facts(Random r)
    {
        var bank = new (string q, string right, string[] wrong)[]
        {
            (X("Qual a proteção obrigatória para os olhos e o rosto durante o arco elétrico?", "What is the mandatory eye and face protection during the electric arc?", "¿Cuál es la protección obligatoria para los ojos y la cara durante el arco eléctrico?"), X("Máscara com filtro de tonalidade adequada à corrente", "A helmet with a filter shade suited to the current", "Careta con filtro de tono adecuado a la corriente"), new[] { X("Óculos de sol escuros comuns", "Ordinary dark sunglasses", "Gafas de sol oscuras comunes"), X("Óculos de segurança transparentes", "Clear safety glasses", "Gafas de seguridad transparentes"), X("Nenhuma, se o arco for curto", "None, if the arc is short", "Ninguna, si el arco es corto") }),
            (X("A radiação ultravioleta do arco pode causar:", "The ultraviolet radiation from the arc can cause:", "La radiación ultravioleta del arco puede causar:"), X("queimadura da córnea (olho de soldador) e da pele", "cornea burns (arc eye) and skin burns", "quemadura de la córnea (ojo de soldador) y de la piel"), new[] { X("apenas calor, sem lesões", "only heat, with no injuries", "solo calor, sin lesiones"), X("intoxicação por gás", "gas poisoning", "intoxicación por gas"), X("choque elétrico", "electric shock", "descarga eléctrica") }),
            (X("Por que soldar tinta, galvanizado ou peças com resíduo exige ventilação/exaustão?", "Why does welding paint, galvanised steel or parts with residue need ventilation/extraction?", "¿Por qué soldar pintura, galvanizado o piezas con residuos exige ventilación/extracción?"), X("Os fumos metálicos e gases são tóxicos", "The metal fumes and gases are toxic", "Los humos metálicos y gases son tóxicos"), new[] { X("O arco apaga com a tinta", "The arc goes out with the paint", "El arco se apaga con la pintura"), X("O metal fica mais mole", "The metal gets softer", "El metal se ablanda"), X("Só por causa do cheiro", "Only because of the smell", "Solo por el olor") }),
            (X("Antes de soldar um tanque que continha combustível, o correto é:", "Before welding a tank that held fuel, the correct thing is to:", "Antes de soldar un tanque que contenía combustible, lo correcto es:"), X("Limpar, purgar e testar a atmosfera: nunca soldar sem liberação", "Clean, purge and test the atmosphere: never weld without clearance", "Limpiar, purgar y probar la atmósfera: nunca soldar sin autorización"), new[] { X("Soldar rápido para não dar tempo de explodir", "Weld fast so it has no time to explode", "Soldar rápido para que no dé tiempo a explotar"), X("Deixar a tampa fechada", "Leave the lid closed", "Dejar la tapa cerrada"), X("Encher de ar comprimido e soldar", "Fill it with compressed air and weld", "Llenar de aire comprimido y soldar") }),
            (X("Os cilindros de gás de proteção devem ser:", "Shielding gas cylinders must be:", "Los cilindros de gas de protección deben estar:"), X("Presos em pé, com a válvula protegida", "Secured upright with the valve protected", "Sujetos en posición vertical, con la válvula protegida"), new[] { X("Deitados no chão para não caírem", "Laid on the floor so they don't fall", "Acostados en el suelo para que no caigan"), X("Apoiados na peça que está sendo soldada", "Leaning on the workpiece being welded", "Apoyados en la pieza que se está soldando"), X("Aquecidos para aumentar a vazão", "Heated to increase flow", "Calentados para aumentar el caudal") }),
            (X("Como se evita o choque elétrico na soldagem?", "How is electric shock avoided in welding?", "¿Cómo se evita la descarga eléctrica en la soldadura?"), X("Luvas e roupas secas, isolar-se do piso e da peça, cabos íntegros", "Dry gloves and clothes, insulation from the floor and workpiece, intact cables", "Guantes y ropa secos, aislarse del suelo y de la pieza, cables en buen estado"), new[] { X("Soldando em piso molhado, que descarrega a corrente", "Welding on a wet floor, which drains the current", "Soldando en suelo mojado, que descarga la corriente"), X("Segurando o eletrodo com a mão nua", "Holding the electrode with a bare hand", "Sujetando el electrodo con la mano desnuda"), X("Usando cabos mais finos", "Using thinner cables", "Usando cables más delgados") }),
            (X("Qual o risco de soldar em espaço confinado sem ventilação?", "What is the risk of welding in a confined space without ventilation?", "¿Cuál es el riesgo de soldar en un espacio confinado sin ventilación?"), X("Falta de oxigênio (gases de proteção deslocam o ar) e acúmulo de fumos", "Lack of oxygen (shielding gases displace air) and fume build-up", "Falta de oxígeno (los gases de protección desplazan el aire) y acumulación de humos"), new[] { X("O arco fica instável apenas", "The arc is merely unstable", "El arco solo se vuelve inestable"), X("A peça esfria rápido demais", "The part cools too fast", "La pieza se enfría demasiado rápido"), X("Nenhum, o ar sempre se renova", "None, the air always renews itself", "Ninguno, el aire siempre se renueva") }),
            (X("Qual EPI protege contra respingos e fagulhas nas pernas e braços?", "Which PPE protects the legs and arms from spatter and sparks?", "¿Qué EPP protege las piernas y los brazos de salpicaduras y chispas?"), X("Roupa de raspa/algodão pesado, mangas e perneiras", "Leather/heavy cotton clothing, sleeves and leggings", "Ropa de cuero/algodón grueso, mangas y polainas"), new[] { X("Camiseta e bermuda de poliéster", "A polyester T-shirt and shorts", "Camiseta y bermudas de poliéster"), X("Avental de plástico", "A plastic apron", "Delantal de plástico"), X("Nenhum, se a solda for rápida", "None, if the weld is quick", "Ninguno, si la soldadura es rápida") }),
        };
        var b = bank[r.Next(bank.Length)];
        return Fact(r, b.q, b.right, X("Regra de ouro: proteja olhos e pele do arco, o pulmão dos fumos, o corpo da eletricidade e o local de fogo e explosão antes de abrir o arco.", "Golden rule: protect eyes and skin from the arc, the lungs from fumes, the body from electricity and the site from fire and explosion before striking the arc.", "Regla de oro: proteja ojos y piel del arco, los pulmones de los humos, el cuerpo de la electricidad y el lugar del fuego y la explosión antes de abrir el arco."), b.wrong);
    }

    protected override Func<Random, Exercise>[] Pool(Difficulty d) => d switch
    {
        Difficulty.Easy => F(Extra, Facts, Facts, r =>
        {
            double v = S(r, 2, 12, 0.5), fl = S(r, 8, 20, 1), t = v * 1000 / fl;
            return Q(XF("Um cilindro de argônio tem {0} m³ de gás utilizável. Com a vazão de {1} L/min, quantos minutos ele dura?", "An argon cylinder has {0} m³ of usable gas. At a flow of {1} L/min, how many minutes does it last?", "Un cilindro de argón tiene {0} m³ de gas utilizable. Con un caudal de {1} L/min, ¿cuántos minutos dura?", N(v), N(fl, 0)), t, "min", YF("V = {0} m³ = {1} L; t = V / vazão = {1} / {2} = {3} min.", N(v), N(v * 1000, 0), N(fl, 0), N(t, 1)));
        }),
        Difficulty.Medium => F(Facts, r =>
        {
            double v = S(r, 50, 120, 5), rb = P(r, 800.0, 1000, 1500, 2000), i = v / rb * 1000;
            return Q(XF("A tensão em vazio de uma máquina é {0} V. Se uma pessoa com a pele suada tem resistência de {1} Ω, que corrente (mA) passa pelo corpo em um contato? (I = V/R)", "A machine's open-circuit voltage is {0} V. If a sweaty person has a body resistance of {1} Ω, what current (mA) flows through the body in a contact? (I = V/R)", "La tensión en vacío de una máquina es {0} V. Si una persona con la piel sudada tiene una resistencia de {1} Ω, ¿qué corriente (mA) pasa por el cuerpo en un contacto? (I = V/R)", N(v, 0), N(rb, 0)), i, "mA", YF("I = V / R = {0} / {1} = {2} mA.", N(v, 0), N(rb, 0), N(i, 1)));
        }, r =>
        {
            double v = S(r, 4, 12, 0.5), l = S(r, 8, 20, 1), am = S(r, 10, 50, 5), t = v * 1000 / l;
            return Q(XF("Em um espaço confinado, a vazão do gás de proteção é {0} L/min e o cilindro tem {1} m³. Se o soldador faz arcos de, em média, {2} min por hora de trabalho, em quantas horas de trabalho o cilindro acaba?", "In a confined space, the shielding gas flow is {0} L/min and the cylinder holds {1} m³. If the welder strikes arcs for an average of {2} min per working hour, after how many working hours does the cylinder run out?", "En un espacio confinado, el caudal del gas de protección es {0} L/min y el cilindro tiene {1} m³. Si el soldador hace arcos de, en promedio, {2} min por hora de trabajo, ¿en cuántas horas de trabajo se acaba el cilindro?", N(l, 0), N(v), N(am, 0)), t / am, "h", YF("Duração em arco aberto: {0} L / {1} L/min = {2} min; horas = {2}/{3} = {4} h.", N(v * 1000, 0), N(l, 0), N(t, 1), N(am, 0), N(t / am, 2)));
        }),
        Difficulty.Hard => F(r =>
        {
            double i1 = P(r, 200.0, 250, 300, 400), d1 = P(r, 0.6, 0.35, 0.4, 0.5), d2 = P(r, 1.0, 0.8, 0.3), i2 = i1 * Sqrt(d1 / d2);
            return Q(XF("Uma máquina de solda é de {0} A com ciclo de trabalho de {1}%. Que corrente máxima pode usar com ciclo de {2}%? (I₂ = I₁·√(D₁/D₂))", "A welding machine is rated {0} A at a {1}% duty cycle. What maximum current can it use at a {2}% duty cycle? (I₂ = I₁·√(D₁/D₂))", "Una máquina de soldar es de {0} A con ciclo de trabajo de {1}%. ¿Qué corriente máxima puede usar con un ciclo de {2}%? (I₂ = I₁·√(D₁/D₂))", N(i1, 0), N(d1 * 100, 0), N(d2 * 100, 0)), i2, "A", YF("I₂ = {0}·√({1}/{2}) = {3} A.", N(i1, 0), N(d1, 2), N(d2, 2), N(i2, 1)));
        }),
        _ => F(r =>
        {
            double i1 = P(r, 200.0, 250, 300, 400, 500), d1 = P(r, 0.6, 0.35, 0.4, 0.5), i2 = S(r, 100, 180, 5), d2 = d1 * i1 * i1 / (i2 * i2);
            return Q(XF("Uma máquina de {0} A com ciclo de trabalho de {1}% é usada com apenas {2} A. Qual o ciclo de trabalho permitido nessa corrente (limitado a 100%)? (I² · D constante)", "A machine rated {0} A at a {1}% duty cycle is used at only {2} A. What duty cycle is allowed at that current (capped at 100%)? (I² · D is constant)", "Una máquina de {0} A con ciclo de trabajo de {1}% se usa con solo {2} A. ¿Qué ciclo de trabajo se permite con esa corriente (limitado a 100%)? (I² · D constante)", N(i1, 0), N(d1 * 100, 0), N(i2, 0)), Math.Min(100, d2 * 100), "%", YF("D₂ = D₁·(I₁/I₂)² = {0}%·({1}/{2})² = {3}%.", N(d1 * 100, 0), N(i1, 0), N(i2, 0), N(d2 * 100, 1)));
        }, Facts)
    };
}
