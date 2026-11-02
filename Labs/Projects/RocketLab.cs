using System.Windows;
using System.Windows.Media;

namespace EasyCreator.ProjectLabs;

/// <summary>Rocket designer + vertical flight simulator (model rockets, PET water rockets, sounding rockets).
/// Physics in <see cref="RocketSim"/>; this class maps the controls to it and draws the flight.</summary>
[Lab("rocket")]
public sealed class RocketLab : ProjectLab
{
    readonly RocketSim _sim = new();
    readonly SceneCanvas _scene;
    protected override FrameworkElement Scene => _scene;
    protected override string[] DefaultControls => new[] { "motor", "dryMass", "diameter", "nose", "finSpan", "chute", "delay" };

    static readonly Brush Sky = Ui.Make("#162033"), Ground = Ui.Make("#2C3A2A"), Body = Ui.Make("#D8DCE4"), FinB = Ui.Make("#18B98A"),
        Flame = Ui.Make("#FFA726"), MotorB = Ui.Make("#8D6E63"), CgB = Ui.Make("#4FC3F7"), CpB = Ui.Make("#FF5C6C"),
        Chute = Ui.Make("#FF8A65"), WaterB = Ui.Make("#4FA3E0"), Trail = Ui.Make("#55FFFFFF");

    public RocketLab()
    {
        var motors = new List<(string, string)> { ("water", "Garrafa PET (água + ar)") };
        motors.AddRange(MotorSpec.Catalog.Select(m => (m.Id, m.Label)));
        motors.Add(("custom", "Personalizado (Isp + vazão)"));
        var motors2 = motors.Where(m => m.Item1 != "water").ToArray();

        AddChoice("motor", "Motor", motors.ToArray(), "B6", "Classe do motor (cada letra tem até o dobro do impulso da anterior), garrafa PET ou motor descrito por Isp e vazão.");
        AddNum("pressure", "Pressão (manométrica)", "bar", 0.5, 6, 0.1, 3, tip: "Pressão de ar acima da atmosfera dentro da garrafa.");
        AddNum("water", "Água na garrafa", "L", 0, 1.9, 0.05, 0.3);
        AddNum("isp", "Isp", "s", 50, 450, 1, 200, tip: "Impulso específico: segundos que 1 kg de propelente produz 9,8 N de empuxo.");
        AddNum("massFlow", "Vazão de massa", "kg/s", 0.001, 1, 0.001, 0.02);
        AddNum("propMass", "Massa de propelente", "kg", 0.01, 1, 0.01, 0.05, Fmt.Mass);
        AddChoice("stages", "Estágios", new[] { ("1", "1 estágio"), ("2", "2 estágios") }, "1");
        AddChoice("motor2", "Motor do 2º estágio", motors2, "C6");
        AddNum("isp2", "Isp (2º estágio)", "s", 50, 450, 1, 300);
        AddNum("massFlow2", "Vazão (2º estágio)", "kg/s", 0.001, 1, 0.001, 0.02);
        AddNum("propMass2", "Propelente (2º estágio)", "kg", 0.01, 1, 0.01, 0.05, Fmt.Mass);
        AddNum("boosterMass", "Estrutura do 1º estágio", "kg", 0.005, 1, 0.005, 0.02, Fmt.Mass, "Massa vazia do estágio de baixo (sem motor), descartada na separação.");
        AddNum("dryMass", "Massa seca", "kg", 0.01, 2, 0.005, 0.06, Fmt.Mass, "Foguete pronto sem motor (ou sem propelente): corpo, aletas, nariz, paraquedas, carga.");
        AddNum("noseWeight", "Peso no nariz", "g", 0, 100, 1, 0, tip: "Massinha ou chumbo dentro do nariz: puxa o CG para a frente.");
        AddNum("diameter", "Diâmetro", "mm", 10, 200, 1, 25);
        AddNum("length", "Comprimento", "mm", 100, 2000, 10, 400);
        AddChoice("nose", "Nariz", new[] { ("ogive", "Ogiva"), ("cone", "Cone"), ("round", "Arredondado"), ("flat", "Reto (sem nariz)") }, "ogive");
        AddChoice("fins", "Aletas", new[] { ("3", "3 aletas"), ("4", "4 aletas") }, "3");
        AddNum("finSpan", "Envergadura da aleta", "mm", 0, 200, 1, 40, tip: "Quanto cada aleta sai para fora do tubo.");
        AddNum("chute", "Paraquedas (diâmetro)", "cm", 0, 200, 1, 30, tip: "0 = sem paraquedas.");
        AddNum("delay", "Atraso de ejeção", "s", 0, 15, 0.5, 4, tip: "Tempo entre o fim da queima e a abertura do paraquedas.");
        _scene = new SceneCanvas(Draw);
    }

    void Apply()
    {
        var s = _sim;
        s.Motor = Pick("motor"); s.Motor2 = Pick("motor2"); s.Nose = Pick("nose");
        s.Stages = Pick("stages") == "2" ? 2 : 1; s.Fins = Pick("fins") == "4" ? 4 : 3;
        s.DryKg = Num("dryMass"); s.NoseWeightG = Num("noseWeight"); s.DiameterMm = Num("diameter"); s.LengthMm = Num("length");
        s.FinSpanMm = Num("finSpan"); s.ChuteCm = Num("chute"); s.DelayS = Num("delay"); s.BoosterKg = Num("boosterMass");
        s.Isp = Num("isp"); s.MassFlow = Num("massFlow"); s.PropKg = Num("propMass");
        s.Isp2 = Num("isp2"); s.MassFlow2 = Num("massFlow2"); s.PropKg2 = Num("propMass2");
        s.PressureBar = Num("pressure"); s.WaterL = Num("water");
        s.BottleL = Setting("bottle", 2); s.NozzleMm = Setting("nozzle", 22); s.Guided = Setting("guided", 0) > 0;
    }

    protected override void InputsChanged() { Apply(); _sim.ComputeStatic(); }

    protected override void Compute()
    {
        Apply();
        _sim.Run();
        var s = _sim;
        RunDuration = s.FlightTime;
        M["apogee"] = s.Apogee;
        M["maxSpeed"] = s.MaxSpeed;
        M["maxAccelG"] = s.MaxAccelG;
        M["burnout"] = s.BurnoutTime;
        M["stabilityMargin"] = s.Margin;
        M["landingSpeed"] = s.Escaped ? double.NaN : s.LandingSpeed;
        M["flightTime"] = s.FlightTime;
        M["deltaV"] = s.DeltaV;
        M["thrustToWeight"] = s.ThrustToWeight;
        M["deploySpeed"] = double.IsNaN(s.DeployTime) ? double.NaN : s.DeploySpeed;
        M["totalImpulse"] = s.TotalImpulse;
        M["liftoffMass"] = s.LiftoffKg;
        M["cd"] = s.Cd;
        M["apogeeTime"] = s.ApogeeTime;
    }

    protected override double PlaySeconds(double simSeconds) => Math.Clamp(simSeconds, 3, 9);

    protected override string Readout()
    {
        var s = _sim;
        string avg = Fmt.F(s.AvgThrust, s.AvgThrust < 100 ? 1 : 0);
        string setup = (s.IsWater
                ? L.F("Massa na decolagem {0} · empuxo médio {1} N (inicial) · empuxo/peso {2} · Cd ≈ {3}", Fmt.Mass(s.LiftoffKg), avg, Fmt.F(s.ThrustToWeight, 1), Fmt.F(s.Cd, 2))
                : L.F("Massa na decolagem {0} · empuxo médio {1} N · empuxo/peso {2} · Cd ≈ {3}", Fmt.Mass(s.LiftoffKg), avg, Fmt.F(s.ThrustToWeight, 1), Fmt.F(s.Cd, 2))) + "\n" +
            L.F("Margem de estabilidade {0} calibres ({1})", Fmt.F(s.Margin, 2), L.T(s.Stability));
        if (!ShowResults) return setup;
        string res = s.NeverLifted ? L.T("O foguete não saiu da base: o empuxo é menor que o peso.")
            : L.F("Apogeu {0} aos {1} s · v máx {2} m/s · a máx {3} g · queima {4} s · Δv ideal {5} m/s", Fmt.Dist(s.Apogee), Fmt.F(s.ApogeeTime, 1), Fmt.F(s.MaxSpeed, 1), Fmt.F(s.MaxAccelG, 1), Fmt.F(s.BurnoutTime, 2), Fmt.F(s.DeltaV, 0)) + "\n" +
              (double.IsNaN(s.DeployTime) ? L.T("Sem paraquedas") : L.F("Paraquedas abre a {0} m/s", Fmt.F(s.DeploySpeed, 1))) +
              (s.Escaped ? L.T(" · escapou da gravidade da Terra!") : L.F(" · pouso a {0} m/s · voo de {1} s", Fmt.F(s.LandingSpeed, 1), Fmt.F(s.FlightTime, 1)));
        return setup + "\n" + res;
    }

    // ---------------------------------------------------------------- drawing

    void Draw(DrawingContext dc, SceneCanvas c)
    {
        double w = c.ActualWidth, h = c.ActualHeight;
        if (w < 50 || h < 50) return;
        DrawProfile(dc, c, new Rect(0, 0, w, 78));
        DrawFlight(dc, c, new Rect(0, 82, w, h - 82));
    }

    void DrawProfile(DrawingContext dc, SceneCanvas c, Rect r)
    {
        var s = _sim;
        double L = s.LengthMm, d = s.DiameterMm;
        var (cr, ct, xr) = s.FinGeometry();
        double sx = (r.Width - 40) / L, x0 = r.Left + 14, cy = r.Top + 34;
        double sy = Math.Min(Math.Max(sx, 5 / (d / 2)), 26 / (d / 2 + Math.Max(s.FinSpanMm, 1)));
        double hb = d / 2 * sy;
        double X(double mm) => x0 + mm * sx;
        double ln = s.Nose == "flat" ? 0 : s.Nose == "round" ? Math.Min(d, 0.3 * L) : Math.Min(3 * d, 0.3 * L);

        // fins
        double span = s.FinSpanMm * sy;
        foreach (int dir in new[] { -1, 1 })
        {
            var g = new StreamGeometry();
            using (var gc = g.Open())
            {
                gc.BeginFigure(new Point(X(L - cr), cy + dir * hb), true, true);
                gc.LineTo(new Point(X(L - cr + xr), cy + dir * (hb + span)), true, false);
                gc.LineTo(new Point(X(L - cr + xr + ct), cy + dir * (hb + span)), true, false);
                gc.LineTo(new Point(X(L), cy + dir * hb), true, false);
            }
            g.Freeze();
            dc.DrawGeometry(FinB, null, g);
        }
        // body + nose
        dc.DrawRectangle(Body, null, new Rect(X(ln), cy - hb, Math.Max(0, X(L) - X(ln)), 2 * hb));
        if (ln > 0)
        {
            var g = new StreamGeometry();
            using (var gc = g.Open())
            {
                gc.BeginFigure(new Point(X(0), cy), true, true);
                if (s.Nose == "cone") gc.LineTo(new Point(X(ln), cy - hb), true, false);
                else gc.QuadraticBezierTo(new Point(X(s.Nose == "round" ? 0 : ln * 0.35), cy - hb), new Point(X(ln), cy - hb), true, false);
                gc.LineTo(new Point(X(ln), cy + hb), true, false);
                if (s.Nose == "cone") gc.LineTo(new Point(X(0), cy), true, false);
                else gc.QuadraticBezierTo(new Point(X(s.Nose == "round" ? 0 : ln * 0.35), cy + hb), new Point(X(0), cy), true, false);
            }
            g.Freeze();
            dc.DrawGeometry(Body, null, g);
        }
        // what sits inside: motor / water / propellant, nose weight
        if (s.IsWater)
        {
            double hw = Math.Min(s.WaterL * 1e6 / (Math.PI * d * d / 4), 0.8 * L);
            dc.DrawRectangle(WaterB, null, new Rect(X(L - hw), cy - hb + 1.5, hw * sx, 2 * hb - 3));
        }
        else
        {
            bool custom = (s.Stages == 2 ? s.Motor2 : s.Motor) == "custom";
            double ml = custom ? 0.44 * L : (MotorSpec.Find(s.Stages == 2 ? s.Motor2 : s.Motor)?.LenMm ?? 70);
            dc.DrawRectangle(MotorB, null, new Rect(X(L - ml), cy - hb * 0.7, ml * sx, hb * 1.4));
        }
        if (s.NoseWeightG > 0) dc.DrawEllipse(Ui.TextDim, null, new Point(X(Math.Max(0.6 * ln, 0.3 * d)), cy), Math.Min(hb * 0.7, 5), Math.Min(hb * 0.7, 5));

        // CG and CP markers
        double cg = Math.Clamp(s.CgMm, 0, L), cp = Math.Clamp(s.CpMm, -0.2 * L, 1.2 * L);
        var thin = PenOf(Ui.Bg0, 1.5);
        dc.DrawEllipse(CgB, thin, new Point(X(cg), cy), 6, 6);
        dc.DrawLine(PenOf(Ui.Bg0, 1.5), new Point(X(cg) - 6, cy), new Point(X(cg) + 6, cy));
        dc.DrawLine(PenOf(Ui.Bg0, 1.5), new Point(X(cg), cy - 6), new Point(X(cg), cy + 6));
        Text(dc, c, "CG", X(cg), r.Top + 2, 11, CgB, true, TextAlignment.Center);
        if (s.Guided)
        {
            Text(dc, c, global::EasyCreator.L.T("sem aletas: guiado por empuxo vetorado"), r.Left + 8, r.Top + 60, 11.5, Ui.TextDim, true);
            return;
        }
        dc.DrawEllipse(CpB, thin, new Point(X(cp), cy), 4.5, 4.5);
        Text(dc, c, "CP", X(cp), r.Top + 58, 11, CpB, true, TextAlignment.Center);
        var col = s.Margin < 0 ? Ui.Bad : s.Margin < 1 || s.Margin > 3 ? Flame : Ui.Good;
        Text(dc, c, global::EasyCreator.L.F("margem {0} cal · {1}", Fmt.F(s.Margin, 2), global::EasyCreator.L.T(s.Stability)), r.Left + 8, r.Top + 2, 11.5, col, true);
    }

    void DrawFlight(DrawingContext dc, SceneCanvas c, Rect r)
    {
        var s = _sim;
        dc.DrawRectangle(Sky, null, r);
        double ground = r.Bottom - 14;
        dc.DrawRectangle(Ground, null, new Rect(r.Left, ground, r.Width, r.Bottom - ground));
        var strip = new Rect(r.Left, r.Top + 6, 74, ground - r.Top - 6);
        var plot = new Rect(r.Left + 86, r.Top + 8, r.Width - 96, ground - r.Top - 26);

        bool run = HasRun && s.Altitude.Count > 1;
        double hMax = NiceMax(Math.Max(run ? s.Apogee : 0, 10) * 1.08);
        double tMax = run ? Math.Max(s.FlightTime, 1) : 10;
        double tNow = double.IsNaN(PlayTime) ? tMax : Math.Min(PlayTime, tMax);
        double alt = run ? At(s.Altitude, tNow) : 0;

        Plot(dc, c, plot, run ? s.Altitude : Array.Empty<(double, double)>(), tMax, 0, hMax, tNow, Ui.Accent, "altitude",
            v => Fmt.Dist(v));
        if (!run) Text(dc, c, L.T("Ajuste o foguete e aperte ▶ Lançar"), plot.Left + plot.Width / 2, plot.Top + plot.Height / 2 - 8, 13, Ui.TextDim, align: TextAlignment.Center);
        else if (s.Apogee > 0)
        {
            double ax = plot.Left + s.ApogeeTime / tMax * plot.Width, ay = plot.Bottom - s.Apogee / hMax * plot.Height;
            if (tNow >= s.ApogeeTime)
            {
                dc.DrawEllipse(Ui.Text, null, new Point(ax, ay), 3, 3);
                Text(dc, c, L.F("apogeu {0}", Fmt.Dist(s.Apogee)), Math.Min(ax + 6, plot.Right - 90), Math.Max(ay - 4, plot.Top + 14), 11, Ui.Text);
            }
        }

        // the rocket climbing in the strip on the left
        double y = strip.Bottom - alt / hMax * strip.Height;
        double cx = strip.Left + strip.Width / 2;
        Text(dc, c, Fmt.Dist(alt), cx, r.Top + 4, 11, Ui.Text, true, TextAlignment.Center);
        bool burning = run && tNow < s.BurnoutTime && !s.NeverLifted;
        bool chute = run && !double.IsNaN(s.DeployTime) && tNow >= s.DeployTime && tNow < tMax;
        bool twoStage = s.Stages == 2 && !s.IsWater && (!run || tNow < s.Stage1Time);
        double len = twoStage ? 44 : 32;
        double angle = run && s.Margin < 0 && alt > 0.5 ? tNow * 540 % 360 : 0;
        dc.PushTransform(new RotateTransform(angle, cx, y - len / 2));
        if (burning)
        {
            var fl = new StreamGeometry();
            using (var g = fl.Open())
            {
                double fh = 10 + 6 * Math.Abs(Math.Sin(tNow * 40));
                g.BeginFigure(new Point(cx - 4, y), true, true);
                g.LineTo(new Point(cx, y + fh), true, false);
                g.LineTo(new Point(cx + 4, y), true, false);
            }
            fl.Freeze();
            dc.DrawGeometry(s.IsWater ? WaterB : Flame, null, fl);
        }
        dc.DrawRectangle(Body, null, new Rect(cx - 4, y - len + 8, 8, len - 8));
        var nose = new StreamGeometry();
        using (var g = nose.Open())
        {
            g.BeginFigure(new Point(cx - 4, y - len + 8), true, true);
            g.LineTo(new Point(cx, y - len), true, false);
            g.LineTo(new Point(cx + 4, y - len + 8), true, false);
        }
        nose.Freeze();
        dc.DrawGeometry(Body, null, nose);
        var fins = new StreamGeometry();
        using (var g = fins.Open())
        {
            g.BeginFigure(new Point(cx - 4, y - 9), true, true); g.LineTo(new Point(cx - 9, y), true, false); g.LineTo(new Point(cx - 4, y), true, false);
            g.BeginFigure(new Point(cx + 4, y - 9), true, true); g.LineTo(new Point(cx + 9, y), true, false); g.LineTo(new Point(cx + 4, y), true, false);
        }
        fins.Freeze();
        dc.DrawGeometry(FinB, null, fins);
        if (twoStage) dc.DrawLine(PenOf(Ui.Bg0, 1.5), new Point(cx - 4, y - 14), new Point(cx + 4, y - 14));
        dc.Pop();
        if (chute)
        {
            double top = y - len - 30;
            var dome = new StreamGeometry();
            using (var g = dome.Open())
            {
                g.BeginFigure(new Point(cx - 16, top + 10), true, true);
                g.QuadraticBezierTo(new Point(cx, top - 10), new Point(cx + 16, top + 10), true, false);
            }
            dome.Freeze();
            dc.DrawGeometry(Chute, null, dome);
            var line = PenOf(Trail, 1);
            dc.DrawLine(line, new Point(cx - 16, top + 10), new Point(cx, y - len));
            dc.DrawLine(line, new Point(cx + 16, top + 10), new Point(cx, y - len));
        }
        Text(dc, c, "0", strip.Right - 2, ground - 14, 10, Ui.TextDim, align: TextAlignment.Right);
    }
}
