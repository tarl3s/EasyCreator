namespace EasyCreator;

/// <summary>Simplified Fe–Fe₃C heat treatment of a plain carbon steel: heat to a temperature (equilibrium phases by the
/// lever rule on straight-line A3/Acm), cool in a medium (furnace, air, oil, water) and optionally temper.
/// Martensite fraction from a hardenability rule (more carbon = easier to harden), hardness by the rule of mixtures,
/// HRC ≈ 116 − 1500/√HV. A teaching model for small parts, not a CCT diagram.</summary>
public static class HeatTreatModel
{
    public const double A1 = 727, Eutectoid = 0.76, MaxCSteel = 2.11, Eutectic = 1148, FeMelt = 1538, A3Pure = 912, CFerrite = 0.022, CCementite = 6.67;

    public static double A3(double c) => A3Pure - (A3Pure - A1) * Math.Min(c, Eutectoid) / Eutectoid;
    public static double Acm(double c) => A1 + (Eutectic - A1) * (Math.Max(c, Eutectoid) - Eutectoid) / (MaxCSteel - Eutectoid);
    public static double Solidus(double c) => FeMelt - (FeMelt - Eutectic) * Math.Min(c, MaxCSteel) / MaxCSteel;
    /// <summary>The temperature above which the steel is fully austenite (A3 for hypo-, Acm for hypereutectoid).</summary>
    public static double Upper(double c) => c <= Eutectoid ? A3(c) : Acm(c);

    public static readonly (string id, string name, double severity)[] Media =
    {
        ("furnace", "Forno (recozimento)", 0.02), ("air", "Ar calmo (normalização)", 0.1), ("oil", "Óleo (têmpera)", 0.5), ("water", "Água (têmpera)", 1.0),
    };

    public static double Severity(string medium)
    {
        foreach (var m in Media) if (m.id == medium) return m.severity;
        return 0.1;
    }

    public sealed class Result
    {
        public double AustAtTemp, CGamma, Ferrite, Pearlite, Cementite, Bainite, Martensite, RetainedAustenite;
        public double HV, HRC, Toughness, QuenchCrack;
        public bool Overheated, Melted, Tempered, FinePearlite, CoarseGrain;
        public string PhasesAtTemp = "", Structure = "", Tip = "";
        public Dictionary<string, double> Metrics = new();
    }

    /// <summary>Equilibrium at temperature T: fraction of austenite and its carbon content.</summary>
    public static (double fGamma, double cGamma, string phases) AtTemperature(double c, double T)
    {
        if (T >= Solidus(c)) return (1, c, L.T("líquido + austenita (γ): o aço começou a fundir!"));
        if (T < A1) return (0, 0, c < Eutectoid ? L.T("ferrita (α) + perlita") : c > Eutectoid ? L.T("perlita + cementita (Fe₃C)") : L.T("perlita"));
        if (c <= Eutectoid)
        {
            if (T >= A3(c)) return (1, c, L.T("austenita (γ)"));
            double cg = Eutectoid * (A3Pure - T) / (A3Pure - A1);
            double f = Math.Clamp((c - CFerrite) / Math.Max(1e-6, cg - CFerrite), 0, 1);
            return (f, cg, L.T("ferrita (α) + austenita (γ)"));
        }
        if (T >= Acm(c)) return (1, c, L.T("austenita (γ)"));
        double cg2 = Eutectoid + (T - A1) / (Eutectic - A1) * (MaxCSteel - Eutectoid);
        double f2 = Math.Clamp((CCementite - c) / (CCementite - cg2), 0, 1);
        return (f2, cg2, L.T("austenita (γ) + cementita (Fe₃C)"));
    }

    public static double MartensiteHV(double c) => 884 * c * (1 - 0.3 * c * c) + 294;

    public static double Hrc(double hv) => 116 - 1500 / Math.Sqrt(Math.Max(hv, 1));

    public static Result Compute(double c, double T, string medium, double temper)
    {
        var r = new Result();
        c = Math.Clamp(c, 0.01, MaxCSteel);
        var (fg, cg, phases) = AtTemperature(c, T);
        r.AustAtTemp = fg; r.CGamma = cg; r.PhasesAtTemp = phases;
        r.Melted = T >= Solidus(c);
        r.Overheated = !r.Melted && T > Upper(c) + 200;
        r.CoarseGrain = !r.Melted && T > Upper(c) + 120;

        // What was not austenitized stays as it was (annealed start: ferrite + pearlite or pearlite + cementite).
        double oldFerrite = 0, oldCementite = 0, oldPearlite = 0;
        double startPearlite = c <= Eutectoid ? Math.Clamp((c - CFerrite) / (Eutectoid - CFerrite), 0, 1) : (CCementite - c) / (CCementite - Eutectoid);
        if (fg < 1)
        {
            if (fg <= 0) { oldPearlite = startPearlite; if (c <= Eutectoid) oldFerrite = 1 - startPearlite; else oldCementite = 1 - startPearlite; }
            else if (c <= Eutectoid) oldFerrite = 1 - fg;
            else oldCementite = 1 - fg;
        }

        double S = Severity(medium);
        // Hardenability of plain carbon steel: the severity needed for full martensite drops with carbon.
        double cH = fg > 0 ? cg : c;
        double sReq = 0.9 - 0.6 * Math.Min(cH, 0.8);
        double fM = fg > 0 ? MetalKit.Clamp01((S - 0.6 * sReq) / (0.4 * sReq)) : 0;
        double ra = fM > 0 ? MetalKit.Clamp01((cH - 0.6) * 0.25) * fM : 0;
        double fromGamma = fg;
        double mart = Math.Max(0, fromGamma * (fM - ra));
        double rest = fromGamma * (1 - fM);
        double newFerrite = 0, newPearlite = 0, newCementite = 0, bainite = 0;
        if (rest > 0)
        {
            if (S >= 0.3) bainite = rest;
            else
            {
                // equilibrium-like split of the γ (composition cg) into proeutectoid phase + pearlite
                double pg = cg <= Eutectoid ? Math.Clamp((cg - CFerrite) / (Eutectoid - CFerrite), 0, 1) : (CCementite - cg) / (CCementite - Eutectoid);
                if (S >= 0.08) pg = Math.Min(1, pg * 1.15); // normalizing: a bit more (and finer) pearlite
                newPearlite = rest * pg;
                if (cg <= Eutectoid) newFerrite = rest * (1 - pg); else newCementite = rest * (1 - pg);
                r.FinePearlite = S >= 0.08;
            }
        }
        r.Ferrite = oldFerrite + newFerrite;
        r.Pearlite = oldPearlite + newPearlite;
        r.Cementite = oldCementite + newCementite;
        r.Bainite = bainite;
        r.Martensite = mart;
        r.RetainedAustenite = fromGamma * ra;
        if (fg <= 0) r.FinePearlite = false;

        // hardness by the rule of mixtures
        double hvM = MartensiteHV(cH);
        r.Tempered = temper >= 150 && mart > 0;
        if (r.Tempered)
        {
            double f = MetalKit.Clamp01(Math.Pow(Math.Max(0, temper - 120) / 560, 1.1));
            hvM = 220 + (hvM - 220) * (1 - f);
        }
        double hvPearlite = r.FinePearlite ? 270 : 210;
        double hvBainite = 300 + 250 * Math.Min(cH, 0.8);
        r.HV = r.Ferrite * 90 + r.Pearlite * hvPearlite + r.Cementite * 800 + r.Bainite * hvBainite + r.Martensite * hvM + r.RetainedAustenite * 220;
        if (r.Melted) r.HV *= 0.8;
        r.HRC = Hrc(r.HV);

        // toughness 0..100 (relative): soft phases and tempered martensite are tough; fresh martensite and cementite are brittle
        double toughM = r.Tempered ? 25 + 60 * MetalKit.Clamp01((temper - 150) / 500) : 8;
        r.Toughness = r.Ferrite * 95 + r.Pearlite * (r.FinePearlite ? 60 : 45) + r.Cementite * 5 + r.Bainite * 55 + r.Martensite * toughM + r.RetainedAustenite * 40;
        if (r.CoarseGrain) r.Toughness *= 0.7;
        if (r.Melted) r.Toughness *= 0.3;
        r.QuenchCrack = MetalKit.Clamp01((S - 0.4) * 1.2 * Math.Max(0, cH - 0.35) * 3 * (r.Tempered ? 0.9 : 1)) * (mart > 0 ? 1 : 0);
        if (r.QuenchCrack <= 0) r.QuenchCrack = 0; // no "-0" in the goals panel

        var parts = new List<string>();
        void P(double f, string name) { if (f >= 0.01) parts.Add($"{L.T(name)} {f * 100:0}%"); }
        P(r.Ferrite, "ferrita");
        P(r.Pearlite, r.FinePearlite ? "perlita fina" : "perlita");
        P(r.Cementite, "cementita");
        P(r.Bainite, "bainita");
        P(r.Martensite, r.Tempered ? "martensita revenida" : "martensita");
        P(r.RetainedAustenite, "austenita retida");
        r.Structure = string.Join(" + ", parts);

        r.Tip = r.Melted ? L.T("Passou da linha solidus: o aço começou a fundir (queimou). Fique abaixo de ~1100 °C.")
            : r.Overheated ? L.T("Temperatura alta demais: o grão cresce muito e o aço fica frágil. Use ≈ 30–50 °C acima de A3 (ou de A1 nos aços hipereutetoides).")
            : fg <= 0 ? L.F("Abaixo de A1 ({0:0} °C) nada vira austenita, então o resfriamento não muda quase nada.", A1)
            : fg < 0.999 && c <= Eutectoid && fM > 0 ? L.F("Só {0:0}% virou austenita (está entre A1 e A3 = {1:0} °C): sobra ferrita mole no aço temperado.", fg * 100, A3(c))
            : mart > 0.5 && !r.Tempered ? L.T("Martensita sem revenimento: muito dura mas frágil como vidro. Faça um revenimento.")
            : r.QuenchCrack > 0.5 ? L.T("Água num aço com tanto carbono pode trincar a peça: prefira óleo.")
            : L.T("Veja no diagrama onde o ponto ficou e compare a microestrutura.");

        r.Metrics = new()
        {
            ["austeniteAtTemp"] = Math.Round(r.AustAtTemp, 3), ["ferrite"] = Math.Round(r.Ferrite, 3), ["pearlite"] = Math.Round(r.Pearlite, 3),
            ["cementite"] = Math.Round(r.Cementite, 3), ["bainite"] = Math.Round(r.Bainite, 3), ["martensite"] = Math.Round(r.Martensite, 3),
            ["austenite"] = Math.Round(r.RetainedAustenite, 3), ["hardnessHV"] = Math.Round(r.HV), ["hrc"] = Math.Round(Math.Max(0, r.HRC), 1),
            ["toughness"] = Math.Round(r.Toughness), ["quenchCrack"] = Math.Round(r.QuenchCrack, 3),
            ["overheated"] = r.Overheated || r.Melted ? 1 : 0, ["temperature"] = T, ["carbon"] = c,
            ["upperCritical"] = Math.Round(Upper(c)),
        };
        return r;
    }
}
