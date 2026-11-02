namespace EasyCreator;

/// <summary>The joint being welded (fixed by the lesson): process, base metal, thickness, joint and position.</summary>
public sealed class WeldSetup
{
    public string Process = "smaw";     // smaw | gmaw | gtaw
    public string Metal = "carbon";     // carbon | stainless | aluminum
    public double Thickness = 6;        // mm
    public string Joint = "fillet";     // butt | fillet | lap
    public string Position = "flat";    // flat | horizontal | vertical | overhead
    public double Carbon = 0.18;        // %C of the steel (HAZ hardness)
    public double CE = 0.38;            // carbon equivalent CE(IIW)

    public static string NormProcess(string s) => s switch
    {
        "mig" or "mag" or "mig/mag" or "gmaw" => "gmaw",
        "tig" or "gtaw" => "gtaw",
        _ => "smaw",
    };
    public static string NormMetal(string s) => s switch
    {
        "inox" or "stainless" or "aço inox" or "aco inox" => "stainless",
        "aluminio" or "alumínio" or "aluminum" or "aluminium" => "aluminum",
        _ => "carbon",
    };
    public static string NormJoint(string s) => s switch
    {
        "topo" or "butt" => "butt",
        "sobreposta" or "lap" => "lap",
        _ => "fillet",
    };
    public static string NormPosition(string s) => s switch
    {
        "horizontal" => "horizontal",
        "vertical" => "vertical",
        "sobrecabeça" or "sobrecabeca" or "overhead" => "overhead",
        _ => "flat",
    };

    public string ProcessName => Process switch
    {
        "gmaw" => "MIG/MAG (GMAW)",
        "gtaw" => "TIG (GTAW)",
        _ => L.T("Eletrodo revestido (SMAW)"),
    };
    public string MetalName => Metal switch { "stainless" => L.T("aço inox 304"), "aluminum" => L.T("alumínio"), _ => L.T("aço carbono") };
    public string JointName => Joint switch { "butt" => L.T("junta de topo"), "lap" => L.T("junta sobreposta"), _ => L.T("filete em T") };
    public string PositionName => Position switch
    {
        "horizontal" => L.T("horizontal"), "vertical" => L.T("vertical ascendente"), "overhead" => L.T("sobrecabeça"), _ => L.T("plana"),
    };
}

/// <summary>What the student sets on the machine and with the hands.</summary>
public sealed class WeldInputs
{
    public double Current = 110;        // A (SMAW, TIG); in MIG it comes from the wire feed
    public double ArcLength = 3;        // mm (SMAW, TIG) — sets the arc voltage
    public double Voltage = 22;         // V (MIG, set on the machine)
    public double WireFeed = 5;         // m/min (MIG)
    public double WireDiameter = 1.0;   // mm (MIG)
    public double Stickout = 15;        // mm (MIG)
    public double TravelSpeed = 4;      // mm/s
    public double Angle = 15;           // travel angle, degrees from perpendicular
    public string Gas = "ar-co2";       // co2 | ar-co2 | ar-o2 | ar | ar-he | none
    public double GasFlow = 14;         // L/min
    public string Electrode = "E6013";  // E6013 | E7018 | E6010 | E308L
    public double Diameter = 3.25;      // mm (SMAW electrode)
    public string Polarity = "dcen";    // dcen | dcep | ac (TIG)
    public double Preheat = 20;         // °C
}

public sealed class WeldResult
{
    public double Current, Voltage, Efficiency, HeatInput, Penetration, Required, Width, Height, Deposit, HazWidth;
    public double CoolingTime, HazHardness, PreheatNeeded;
    public double Porosity, Undercut, LackOfFusion, BurnThrough, Spatter, Crack, Inclusion, Quality;
    public double ILow, IHigh;          // recommended current window (A)
    public string Transfer = "";        // MIG metal transfer mode
    public string Tip = "";
    public Dictionary<string, double> Metrics = new();
}

/// <summary>A simplified, physically-motivated welding model. Heat input Q = η·U·I/v; penetration grows with current
/// (≈ I^1.4) and falls with speed; deposit from the consumable's melting rate; cooling time t8/5 from the EN 1011-2
/// thin/thick plate formulas; HAZ hardness from Düren's formula (CE, t8/5); defect likelihoods (0..1) from the
/// distance of each parameter to its good window. Numbers are for teaching, not for a real WPS.</summary>
public static class WeldModel
{
    public static readonly string[] Electrodes = { "E6013", "E7018", "E6010", "E308L" };

    /// <summary>Thermal efficiency η (EN 1011-1 / ISO/TR 17671-1): SMAW 0.8, GMAW 0.8, GTAW 0.6.</summary>
    public static double Efficiency(string process) => process == "gtaw" ? 0.6 : 0.8;

    /// <summary>Heat input Q (kJ/mm) = η · U (V) · I (A) / v (mm/s) / 1000.</summary>
    public static double HeatInput(double eta, double volts, double amps, double mmPerSecond) =>
        eta * volts * amps / Math.Max(1e-6, mmPerSecond) / 1000;

    /// <summary>Carbon equivalent of the IIW (all in weight %): C + Mn/6 + (Cr+Mo+V)/5 + (Ni+Cu)/15.</summary>
    public static double CeIIW(double c, double mn, double cr = 0, double mo = 0, double v = 0, double ni = 0, double cu = 0) =>
        c + mn / 6 + (cr + mo + v) / 5 + (ni + cu) / 15;

    /// <summary>Recommended SMAW current range (flat position) by electrode and diameter.</summary>
    public static (double lo, double hi) ElectrodeRange(string electrode, double d)
    {
        int i = d < 2.9 ? 0 : d < 3.6 ? 1 : 2;
        var t = electrode switch
        {
            "E7018" => new[] { (70.0, 100.0), (100.0, 145.0), (140.0, 190.0) },
            "E6010" => new[] { (50.0, 85.0), (75.0, 125.0), (110.0, 165.0) },
            "E308L" => new[] { (50.0, 80.0), (75.0, 110.0), (100.0, 145.0) },
            _ => new[] { (60.0, 90.0), (90.0, 130.0), (130.0, 180.0) },
        };
        return t[i];
    }

    public static double PositionFactor(string position) => position switch
    {
        "horizontal" => 0.95, "vertical" => 0.88, "overhead" => 0.86, _ => 1.0,
    };

    /// <summary>The current window shown next to the current slider (SMAW: electrode range, corrected for position).</summary>
    public static (double lo, double hi) CurrentWindow(WeldSetup s, WeldInputs x)
    {
        double pf = PositionFactor(s.Position);
        if (s.Process == "smaw")
        {
            var (lo, hi) = ElectrodeRange(x.Electrode, x.Diameter);
            return (Math.Round(lo * (pf < 1 ? 0.95 : 1) / 5) * 5, Math.Round(hi * pf / 5) * 5);
        }
        if (s.Process == "gmaw")
            return x.WireDiameter < 0.9 ? (50, 180 * pf) : x.WireDiameter < 1.1 ? (80, 250 * pf) : (120, 320 * pf);
        // TIG: the shop rule "1 A per 0.001 in" ≈ 40 A per mm on steel; stainless (conducts little heat) a bit less,
        // aluminum (spreads the heat, AC) a bit more — within the tungsten's capacity.
        double perMm = s.Metal == "aluminum" ? 42 : s.Metal == "stainless" ? 32 : 40;
        double mid = perMm * Math.Min(s.Thickness, 6) * pf;
        return (Math.Round(mid * 0.75 / 5) * 5, Math.Round(Math.Min(mid * 1.3, 300) / 5) * 5);
    }

    /// <summary>SMAW arc voltage from arc length; TIG arc voltage from arc length.</summary>
    public static double ArcVoltage(WeldSetup s, WeldInputs x) => s.Process switch
    {
        "gtaw" => 8.5 + 0.025 * x.Current + 0.9 * x.ArcLength,
        "gmaw" => x.Voltage,
        _ => Math.Max(16, 18 + 0.035 * x.Current + 1.6 * (x.ArcLength - x.Diameter)),
    };

    /// <summary>MIG current from wire feed speed, wire diameter and stick-out (constant-voltage machine).</summary>
    public static double MigCurrent(WeldInputs x)
    {
        var (a, b) = x.WireDiameter < 0.9 ? (30.0, 16.0) : x.WireDiameter < 1.1 ? (40.0, 21.0) : (50.0, 27.0);
        return (a + b * x.WireFeed) * Math.Pow(15 / Math.Clamp(x.Stickout, 4, 40), 0.25);
    }

    public static double Current(WeldSetup s, WeldInputs x) => s.Process == "gmaw" ? MigCurrent(x) : x.Current;

    /// <summary>The MIG voltage that gives a stable arc for a current (short-circuit ↔ spray).</summary>
    public static double MigBestVoltage(double current) => 14 + 0.05 * current;

    static double C01(double v) => MetalKit.Clamp01(v);

    public static WeldResult Compute(WeldSetup s, WeldInputs x)
    {
        var r = new WeldResult();
        var causes = new List<(double weight, string text)>();
        double pf = PositionFactor(s.Position);
        double I = Current(s, x), V = ArcVoltage(s, x), v = Math.Max(0.2, x.TravelSpeed), t = Math.Max(0.5, s.Thickness);
        r.Current = I; r.Voltage = V;
        r.Efficiency = Efficiency(s.Process);
        r.HeatInput = HeatInput(r.Efficiency, V, I, v);
        var (ilo, ihi) = CurrentWindow(s, x);
        r.ILow = ilo; r.IHigh = ihi;
        double lowI = C01((ilo - I) / (0.3 * ilo)), highI = C01((I - ihi) / (0.3 * ihi));
        bool al = s.Metal == "aluminum", inox = s.Metal == "stainless", steel = s.Metal == "carbon";

        // Good travel-speed windows (mm/s) and gas-flow windows (L/min) by process.
        var (vlo, vhi) = s.Process switch { "gmaw" => (3.0, 9.0), "gtaw" => (1.0, 4.0), _ => (2.0, 5.0) };
        if (s.Position is "vertical" or "overhead") { vlo *= 0.8; vhi *= 0.85; }
        var (flo, fhi) = s.Process == "gtaw" ? (7.0, 12.0) : (12.0, 18.0);
        double angleExcess = Math.Max(0, x.Angle - 20);
        double arcRatio = s.Process == "smaw" ? x.ArcLength / Math.Max(1, x.Diameter) : x.ArcLength / 2.5;
        double dv = s.Process == "gmaw" ? V - MigBestVoltage(I) : 0;
        bool shielded = s.Process != "smaw";
        string gas = x.Gas;

        // ---- penetration ----
        double kp = s.Process switch
        {
            "gmaw" => 1.6 * gas switch { "co2" => 1.15, "ar-o2" => 0.95, "ar" => 0.85, "ar-he" => 1.1, _ => 1.0 },
            "gtaw" => 2.0 * (gas == "ar-he" ? 1.25 : 1.0) * x.Polarity switch { "ac" => 0.75, "dcep" => 0.4, _ => 1.0 },
            _ => 2.2 * x.Electrode switch { "E7018" => 1.15, "E6010" => 1.5, _ => 1.0 },
        };
        double metalPen = al ? 1.1 : inox ? 1.1 : 1.0; // Al: conducts heat away, but melts at 660 °C
        double arcF = s.Process switch
        {
            "smaw" => Math.Clamp(1.15 - 0.15 * arcRatio, 0.7, 1.05),
            "gtaw" => Math.Clamp(1.15 - 0.06 * x.ArcLength, 0.7, 1.1),
            _ => Math.Clamp(1 - 0.03 * Math.Max(0, dv), 0.75, 1),
        };
        double angleF = Math.Sqrt(Math.Cos(Math.Clamp(x.Angle, 0, 80) * Math.PI / 180));
        double preF = 1 + (x.Preheat - 20) / (al ? 900 : 1500);
        double thickF = Math.Clamp(Math.Pow(6 / t, 0.08), 0.85, 1.12);
        r.Penetration = kp * Math.Pow(I / 100, 1.4) * Math.Pow(4 / v, 0.3) * metalPen * arcF * angleF * preF * thickF;

        r.Required = s.Joint switch
        {
            "butt" => t <= 6 ? 0.8 * t : 2.0,
            "lap" => Math.Clamp(0.25 * t, 0.8, 2.0),
            _ => Math.Clamp(0.2 * t, 1.0, 2.0),
        };

        // ---- bead geometry ----
        double depositRate = s.Process switch // mm³/s of filler metal
        {
            "gmaw" => x.WireFeed * 1000 / 60 * Math.PI * x.WireDiameter * x.WireDiameter / 4 * 0.95,
            "gtaw" => 0.12 * I,
            _ => 0.35 * I * x.Electrode switch { "E7018" => 1.2, "E6010" => 0.85, _ => 1.0 },
        };
        r.Deposit = depositRate / v;
        double metalW = al ? 1.25 : inox ? 0.95 : 1.0;
        r.Width = Math.Max(1.5, 0.35 * V * Math.Sqrt(I / 100) * Math.Pow(4 / v, 0.3) * metalW);
        r.Height = 1.5 * r.Deposit / r.Width;
        r.HazWidth = (0.6 + 1.8 * r.HeatInput) * (al ? 1.8 : inox ? 0.8 : 1.0);

        // ---- cooling (EN 1011-2) and HAZ hardness ----
        // Shape factors of EN 1011-2 (Annex D): butt weld passes F2 = F3 = 0.9; single-run fillet on a T-joint
        // F2 = 0.45–0.67, F3 = 0.67 (the lap joint is treated like a fillet).
        var (f2, f3) = s.Joint switch { "butt" => (0.9, 0.9), _ => (0.67, 0.67) };
        r.CoolingTime = CoolingTime(r.HeatInput, x.Preheat, t, f2, f3);
        r.HazHardness = steel ? HazHardness(s.Carbon, s.CE, r.CoolingTime) : inox ? 210 : 75;

        void Add(ref double defect, double amount, string why)
        {
            if (amount <= 0 || double.IsNaN(amount)) return;
            defect += amount;
            causes.Add((amount, why));
        }

        // ---- lack of fusion ----
        double ratio = r.Penetration / r.Required;
        double lof = 0;
        Add(ref lof, C01((1.15 - ratio) / 0.5), L.F("Penetração de {0} mm, mas a junta precisa de {1} mm: aumente a corrente ou reduza a velocidade (falta de fusão).", MetalKit.F(r.Penetration), MetalKit.F(r.Required)));
        Add(ref lof, 0.5 * lowI, L.F("Corrente abaixo da faixa ({0:0}–{1:0} A): o metal não funde direito.", ilo, ihi));
        if (s.Process == "gmaw") Add(ref lof, 0.1 * Math.Max(0, -dv - 2.5), L.T("Tensão baixa demais para essa velocidade de arame: o arame \"bate\" na peça e o cordão fica frio (colagem)."));
        if (v < vlo * 0.6) Add(ref lof, 0.15, L.T("Velocidade muito baixa: a poça corre na frente do arco e o metal cai sobre metal frio (sobreposição)."));
        if (al && s.Process == "gtaw" && x.Polarity != "ac") Add(ref lof, x.Polarity == "dcen" ? 0.6 : 0.2, L.T("No alumínio use corrente alternada (CA): o meio ciclo positivo quebra a camada de óxido (Al₂O₃, que só funde a ≈ 2070 °C) e o negativo aquece a peça."));
        if (al && s.Process == "smaw") Add(ref lof, 0.45, L.T("Eletrodo revestido no alumínio é muito difícil: use TIG em CA ou MIG com argônio."));
        if (s.Process == "smaw" && arcRatio < 0.6) Add(ref lof, 0.2, L.T("Arco curto demais: o eletrodo gruda na peça."));
        r.LackOfFusion = C01(lof);

        // ---- burn-through ----
        double bt = 0, through = r.Penetration / t;
        Add(ref bt, s.Joint == "butt" ? C01((through - 1.25) / 0.5) : C01((through - 0.85) / 0.4),
            L.F("Penetração de {0} mm numa chapa de {1} mm: furou! Reduza a corrente ou aumente a velocidade.", MetalKit.F(r.Penetration), MetalKit.F(t)));
        if (t < 3.5)
        {
            double hiMax = 0.18 * t + 0.1;
            Add(ref bt, 0.6 * C01((r.HeatInput - hiMax) / hiMax), L.F("Chapa fina: aporte térmico de {0} kJ/mm é muito para {1} mm (máx. ≈ {2}).", MetalKit.F(r.HeatInput, "0.00"), MetalKit.F(t), MetalKit.F(hiMax, "0.00")));
        }
        if (s.Position is "vertical" or "overhead") Add(ref bt, 0.25 * highI, L.T("Fora da posição plana a poça escorre: use uns 10–15% menos corrente."));
        r.BurnThrough = C01(bt);

        // ---- undercut ----
        double u = 0, posU = s.Position switch { "horizontal" => 1.4, "vertical" => 1.3, "overhead" => 1.3, _ => 1.0 };
        Add(ref u, 0.9 * highI * posU, L.F("Corrente acima da faixa ({0:0}–{1:0} A): a borda do cordão derrete e fica um sulco (mordedura).", ilo, ihi));
        if (s.Process == "smaw") Add(ref u, 0.3 * C01((arcRatio - 1.3) / 1.0) * posU, L.T("Arco longo demais: tensão alta, o arco \"abre\" e cava as bordas (mordedura). Mantenha o arco ≈ diâmetro do eletrodo."));
        if (s.Process == "gmaw") Add(ref u, 0.08 * Math.Max(0, dv - 2) * posU, L.F("Tensão alta demais (ideal ≈ {0} V para {1:0} A): arco longo, cordão largo e mordedura.", MetalKit.F(MigBestVoltage(I)), I));
        Add(ref u, 0.35 * C01((v - vhi) / vhi) * posU, L.F("Velocidade alta demais (bom: {0}–{1} mm/s): o metal não preenche as bordas.", MetalKit.F(vlo), MetalKit.F(vhi)));
        Add(ref u, 0.012 * angleExcess * posU, L.F("Ângulo de {0:0}° é inclinado demais: mantenha 5–20°.", x.Angle));
        if (s.Process == "gmaw" && gas == "ar" && !al) Add(ref u, 0.25, L.T("Argônio puro no aço deixa o arco instável e o cordão não molha as bordas: use Ar + CO₂."));
        r.Undercut = C01(u);

        // ---- spatter ----
        double sp = 0;
        if (s.Process == "smaw")
        {
            sp += x.Electrode switch { "E6010" => 0.15, "E7018" => 0.07, _ => 0.05 };
            Add(ref sp, 0.7 * highI, L.T("Corrente alta demais: respingos para todo lado."));
            Add(ref sp, 0.25 * C01((arcRatio - 1.3) / 1.0), L.T("Arco longo: o metal atravessa um arco instável e respinga."));
        }
        else if (s.Process == "gmaw")
        {
            sp += 0.04;
            if (gas == "co2") Add(ref sp, 0.18, L.T("CO₂ puro respinga mais que a mistura Ar + CO₂ (mas penetra mais e é mais barato)."));
            if (dv < -2) Add(ref sp, 0.1 * (-dv - 2), L.F("Tensão baixa para {0:0} A (ideal ≈ {1} V): o arame entra na poça e explode em respingos.", I, MetalKit.F(MigBestVoltage(I))));
            if (dv > 3) Add(ref sp, 0.06 * (dv - 3), L.T("Tensão alta demais: gotas grandes (transferência globular) e respingos."));
            if (x.Stickout > 25) Add(ref sp, 0.1, L.T("Stick-out (arame para fora do bico) longo demais: arco instável. Use 10–20 mm."));
            if (x.Stickout < 6) Add(ref sp, 0.08, L.T("Bico encostado na poça: respingos grudam no bocal."));
        }
        else if (x.Polarity == "dcep") Add(ref sp, 0.05, L.T("TIG com eletrodo no positivo (CC+) aquece demais o tungstênio."));
        Add(ref sp, 0.1 * angleExcess / 20, L.T("Ângulo muito inclinado: o arco sopra o metal para fora."));
        r.Spatter = C01(sp);
        // Transfer mode: a key (the Portuguese name), translated where it is shown.
        if (s.Process == "gmaw")
            r.Transfer = I < 200 && V < 24 ? "curto-circuito" : I >= 230 && V >= 25 && gas is "ar-co2" or "ar-o2" or "ar" or "ar-he" ? "spray" : "globular";

        // ---- porosity ----
        double po = al ? 0.06 : 0.02;
        if (shielded)
        {
            if (gas == "none") Add(ref po, 0.95, L.T("Sem gás de proteção o ar entra na poça: porosidade total!"));
            else
            {
                if (x.GasFlow < flo) Add(ref po, 0.9 * Math.Pow((flo - x.GasFlow) / flo, 1.2), L.F("Vazão de gás baixa ({0} L/min; bom: {1:0}–{2:0}): o ar contamina a poça (poros).", MetalKit.F(x.GasFlow), flo, fhi));
                if (x.GasFlow > fhi * 1.6) Add(ref po, 0.5 * (x.GasFlow - fhi * 1.6) / fhi, L.T("Vazão de gás alta demais: turbulência puxa ar para a poça."));
                if (al && gas is "co2" or "ar-co2" or "ar-o2") Add(ref po, 0.7, L.T("Alumínio só aceita gás inerte (argônio ou Ar + He): CO₂/O₂ oxidam a poça."));
                if (s.Process == "gtaw" && gas is "co2" or "ar-co2" or "ar-o2") Add(ref po, 0.6, L.T("No TIG use só gás inerte: CO₂/O₂ queimam o tungstênio."));
            }
            if (s.Process == "gmaw" && x.Stickout > 22) Add(ref po, 0.03 * (x.Stickout - 22), L.T("Bico longe da peça: o gás não cobre a poça."));
            Add(ref po, 0.02 * angleExcess, L.T("Tocha muito inclinada: o gás não protege a poça."));
        }
        else
        {
            double longArc = C01((arcRatio - 1.2) / 1.0);
            Add(ref po, (x.Electrode == "E7018" ? 0.5 : 0.25) * longArc, x.Electrode == "E7018"
                ? L.T("E7018 com arco longo dá porosidade: o revestimento básico só protege com arco curto.")
                : L.T("Arco longo: o revestimento não protege a poça e o ar entra (poros)."));
            Add(ref po, 0.012 * angleExcess, L.T("Eletrodo muito inclinado: a escória e os gases não cobrem a poça."));
        }
        if (v > vhi * 1.3) Add(ref po, 0.1, L.T("Muito rápido: a poça solidifica antes dos gases saírem."));
        r.Porosity = C01(po);

        // ---- slag / tungsten inclusions ----
        double inc = 0;
        if (s.Process == "smaw")
        {
            Add(ref inc, 0.4 * lowI, L.T("Corrente baixa: a escória fica presa dentro do cordão (inclusão)."));
            if (arcRatio < 0.7) Add(ref inc, 0.4 * (0.7 - arcRatio) / 0.3, L.T("Arco muito curto: escória presa no cordão."));
            if (x.Angle < 5) Add(ref inc, 0.15, L.T("Eletrodo em pé (0°): a escória corre na frente do arco. Incline 10–20° (puxando)."));
        }
        else if (s.Process == "gtaw")
        {
            if (x.ArcLength < 1) Add(ref inc, 0.5 * (1 - x.ArcLength), L.T("Tungstênio quase tocando a poça: pedaços dele ficam no cordão."));
            if (x.Polarity == "dcep") Add(ref inc, 0.7, L.T("TIG em CC+ derrete a ponta do tungstênio (inclusão de tungstênio)."));
        }
        r.Inclusion = C01(inc);

        // ---- cracking ----
        double cr = 0;
        r.PreheatNeeded = 20;
        if (steel)
        {
            cr = CrackRisk(s, x, r.HazHardness, t);
            if (cr > 0) causes.Add((cr, L.F("ZTA dura ({0:0} HV) + hidrogênio: risco de trinca a frio. Use eletrodo básico (E7018), mais aporte ou pré-aquecimento.", r.HazHardness)));
            r.PreheatNeeded = 20;
            for (double t0 = 20; t0 <= 300; t0 += 10)
            {
                r.PreheatNeeded = t0;
                double hv = HazHardness(s.Carbon, s.CE, CoolingTime(r.HeatInput, t0, t, f2, f3));
                if (CrackRisk(s, x, hv, t) <= 0.2) break;
            }
        }
        else if (inox)
        {
            if (s.Process == "smaw" && x.Electrode != "E308L") Add(ref cr, 0.6, L.T("Eletrodo de aço carbono no inox: o cordão fica martensítico, trinca e enferruja. Use E308L."));
            Add(ref cr, 0.3 * C01((r.HeatInput - 1.5) / 1.0), L.T("Aporte alto no inox: risco de trinca a quente e de perder a resistência à corrosão."));
        }
        else
        {
            cr = 0.04;
            Add(ref cr, 0.25 * C01((v - vhi) / vhi), L.T("Alumínio soldado rápido demais: trinca de cratera/solidificação."));
        }
        r.Crack = C01(cr);

        // ---- quality ----
        double q = (1 - 0.8 * r.Porosity) * (1 - 0.6 * r.Undercut) * (1 - r.LackOfFusion) * (1 - r.BurnThrough)
                   * (1 - 0.35 * r.Spatter) * (1 - r.Crack) * (1 - 0.8 * r.Inclusion);
        double convex = r.Height / r.Width;
        if (convex > 0.45)
        {
            q *= Math.Max(0.7, 1 - (convex - 0.45));
            causes.Add((convex - 0.45, L.T("Cordão alto e estreito (convexo demais): aumente a velocidade ou a tensão para ele \"molhar\" as bordas.")));
        }
        if (s.Process == "gmaw" && inox && gas is "ar-co2" or "co2")
        {
            q *= 0.8;
            causes.Add((0.2, L.T("No inox use Ar + 2% O₂ (ou 2% CO₂): CO₂ demais coloca carbono no cordão e ele perde a resistência à corrosão.")));
        }
        r.Quality = Math.Round(100 * Math.Clamp(q, 0, 1));

        var worst = causes.OrderByDescending(c => c.weight).FirstOrDefault();
        r.Tip = r.Quality >= 85 ? L.T("Ótimo cordão! Parâmetros dentro da janela do processo.")
            : worst.text == null ? L.T("Ajuste os parâmetros para melhorar o cordão.")
            : r.Quality >= 80 ? L.F("Bom cordão. Para melhorar: {0}", worst.text) : worst.text;

        r.Metrics = new()
        {
            ["heatInput"] = Math.Round(r.HeatInput, 3), ["penetration"] = Math.Round(r.Penetration, 2),
            ["requiredPenetration"] = r.Required, ["beadWidth"] = Math.Round(r.Width, 2), ["beadHeight"] = Math.Round(r.Height, 2),
            ["depositArea"] = Math.Round(r.Deposit, 1), ["current"] = Math.Round(I, 1), ["voltage"] = Math.Round(V, 1),
            ["travelSpeed"] = v, ["coolingTime"] = Math.Round(r.CoolingTime, 1), ["hazHardness"] = Math.Round(r.HazHardness),
            ["hazWidth"] = Math.Round(r.HazWidth, 2), ["preheatNeeded"] = r.PreheatNeeded, ["preheat"] = x.Preheat,
            ["porosity"] = Math.Round(r.Porosity, 3), ["undercut"] = Math.Round(r.Undercut, 3),
            ["lackOfFusion"] = Math.Round(r.LackOfFusion, 3), ["burnThrough"] = Math.Round(r.BurnThrough, 3),
            ["spatter"] = Math.Round(r.Spatter, 3), ["crackRisk"] = Math.Round(r.Crack, 3), ["inclusion"] = Math.Round(r.Inclusion, 3),
            ["quality"] = r.Quality,
        };
        return r;
    }

    /// <summary>Cooling time from 800 to 500 °C (s): the larger of the thick-plate (3D) and thin-plate (2D) values.</summary>
    public static double CoolingTime(double q, double t0, double d, double f2, double f3)
    {
        t0 = Math.Min(t0, 450);
        double t3 = (6700 - 5 * t0) * q * (1 / (500 - t0) - 1 / (800 - t0)) * f3;
        double a = 1 / (500 - t0), b = 1 / (800 - t0);
        double t2 = (4300 - 4.3 * t0) * 1e5 * q * q / (d * d) * (a * a - b * b) * f2;
        return Math.Max(0.5, Math.Max(t3, t2));
    }

    /// <summary>Düren's HAZ maximum hardness (HV10) from %C, CE and t8/5.</summary>
    public static double HazHardness(double c, double ce, double t85)
    {
        double lg = Math.Log10(Math.Max(0.5, t85));
        double hv = 2019 * (c * (1 - 0.5 * lg) + 0.3 * (ce - c)) + 66 * (1 - 0.8 * lg);
        double martensite = 884 * c * (1 - 0.3 * c * c) + 294;
        return Math.Clamp(hv, 150, martensite);
    }

    /// <summary>Hydrogen cold-cracking risk: how far the HAZ hardness is above what the consumable's hydrogen level tolerates.</summary>
    public static double CrackRisk(WeldSetup s, WeldInputs x, double hv, double t)
    {
        double crit = s.Process switch
        {
            "gmaw" => 460, "gtaw" => 480,
            _ => x.Electrode switch { "E7018" => 440, "E6010" => 320, "E308L" => 520, _ => 340 },
        };
        double restraint = Math.Clamp(0.7 + t / 40, 0.7, 1.4);
        return MetalKit.Clamp01((hv - crit) / 120 * restraint);
    }
}
