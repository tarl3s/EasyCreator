using System.Windows;
using System.Windows.Media;

namespace EasyCreator.ProjectLabs;

/// <summary>Submarine / ROV buoyancy and depth-control lab. Physics in <see cref="SubmarineSim"/>; this class maps the
/// controls and draws a side view with a depth gauge and the depth-versus-time plot.</summary>
[Lab("submarine")]
public sealed class SubmarineLab : ProjectLab
{
    readonly SubmarineSim _sim = new();
    readonly SceneCanvas _scene;
    protected override FrameworkElement Scene => _scene;
    protected override string[] DefaultControls => new[] { "hullVolume", "hullMass", "ballastVolume", "fill", "water" };
    protected override string RunLabel => "▶ Mergulhar";

    static readonly Brush WaterTop = Ui.Make("#1B4F72"), WaterDeep = Ui.Make("#0B1B2E"), SkyB = Ui.Make("#20283A"),
        Sand = Ui.Make("#5D4E37"), Hull = Ui.Make("#F2C14E"), HullDark = Ui.Make("#B8892C"), CbB = Ui.Make("#4FC3F7"),
        CgB = Ui.Make("#FF8A65"), Crush = Ui.Make("#FF5C6C"), Bubble = Ui.Make("#66FFFFFF"), Target = Ui.Make("#FFCA28");

    public SubmarineLab()
    {
        AddNum("hullVolume", "Volume do casco", "L", 1, 200, 0.5, 20, tip: "Volume de água que o casco inteiro desloca quando está submerso.");
        AddNum("hullMass", "Massa do casco", "kg", 0.5, 200, 0.1, 21, tip: "Tudo o que é fixo: casco, motores, baterias, eletrônica (sem água no lastro).");
        AddNum("payload", "Carga útil", "kg", 0, 50, 0.1, 0, tip: "Câmera, garra, sensores extras.");
        AddNum("ballastVolume", "Tanque de lastro", "L", 0, 50, 0.1, 2, tip: "Volume do tanque que pode ser inundado (fica dentro do volume do casco).");
        AddNum("fill", "Lastro cheio", "%", 0, 100, 1, 0, tip: "Quanto do tanque de lastro está cheio de água.");
        AddChoice("water", "Água", new[] { ("fresh", "Doce · 1000 kg/m³"), ("salt", "Salgada · 1025 kg/m³") }, "fresh");
        AddNum("trim", "Chumbo no centro", "g", 0, 5000, 10, 0, tip: "Lastro fixo de ajuste fino, na altura do centro do casco.");
        AddNum("keel", "Chumbo na quilha", "g", 0, 5000, 10, 0, tip: "Lastro fixo no fundo do casco: abaixa o centro de gravidade.");
        AddNum("blowAt", "Esvaziar lastro aos", "s", 0, 300, 1, 0, v => v <= 0 ? "nunca" : Fmt.F(v, 0) + " s", "Ar comprimido expulsa a água do tanque (leva ~4 s). 0 = nunca.");
        AddChoice("controller", "Controle vertical", new[] { ("manual", "Manual (força fixa)"), ("pid", "PID de profundidade") }, "manual");
        AddNum("thrust", "Propulsor vertical", "N", -50, 50, 0.5, 0, tip: "Manual: força fixa (+ empurra para baixo). PID: força máxima do propulsor.");
        AddNum("target", "Profundidade-alvo", "m", 0, 100, 0.5, 10);
        AddNum("kp", "Kp", "N/m", 0, 100, 0.5, 0);
        AddNum("ki", "Ki", "N/(m·s)", 0, 20, 0.05, 0);
        AddNum("kd", "Kd", "N·s/m", 0, 200, 1, 0);
        AddNum("hullRating", "Resistência do casco", "bar", 0.5, 50, 0.1, 3, tip: "Diferença de pressão (água fora × 1 atm dentro) que o casco aguenta.");
        AddNum("forwardThrust", "Propulsor de avanço", "N", 0, 100, 0.5, 10);
        AddChoice("shape", "Formato", new[] { ("torpedo", "Torpedo"), ("cylinder", "Cilindro reto"), ("box", "Caixa (ROV aberto)") }, "torpedo");
        _scene = new SceneCanvas(Draw);
    }

    void Apply()
    {
        var s = _sim;
        s.HullL = Num("hullVolume"); s.HullKg = Num("hullMass"); s.PayloadKg = Num("payload"); s.BallastL = Math.Min(Num("ballastVolume"), Num("hullVolume"));
        s.FillPct = Num("fill"); s.Salt = Pick("water") == "salt"; s.TrimG = Num("trim"); s.KeelG = Num("keel"); s.BlowAt = Num("blowAt");
        s.Pid = Pick("controller") == "pid"; s.ThrustN = Num("thrust"); s.TargetM = Num("target");
        s.Kp = Num("kp"); s.Ki = Num("ki"); s.Kd = Num("kd"); s.RatingBar = Num("hullRating");
        s.ForwardN = Num("forwardThrust"); s.Shape = Pick("shape");
        s.Floor = Setting("floor", 30); s.Start = Setting("start", 0); s.Duration = Setting("duration", 120); s.HullCgCm = Setting("hullCg", -0.5);
    }

    protected override void InputsChanged() { Apply(); _sim.ComputeStatic(); }

    protected override void Compute()
    {
        Apply();
        _sim.Run();
        var s = _sim;
        RunDuration = s.Crushed ? s.CrushTime : s.Duration;
        M["netBuoyancy"] = Math.Round(s.NetBuoyancy, 9) + 0.0; // never "-0"
        M["finalDepth"] = s.FinalDepth;
        M["maxDepth"] = s.MaxDepth;
        M["pressureAtDepth"] = s.PressureBar;
        M["settlingTime"] = s.SettlingTime;
        M["overshoot"] = s.Overshoot;
        M["crushed"] = s.Crushed ? 1 : 0;
        M["crushDepth"] = s.CrushDepth;
        M["safetyFactor"] = s.SafetyFactor;
        M["maxSpeed"] = s.MaxSpeed;
        M["cruiseSpeed"] = s.CruiseSpeed;
        M["propPower"] = s.PropPower;
        M["bg"] = s.BgCm;
        M["totalMass"] = s.MassKg;
        M["meanDensity"] = s.MeanDensity;
    }

    protected override double PlaySeconds(double simSeconds) => Math.Clamp(simSeconds / 10, 3, 9);

    protected override string Readout()
    {
        var s = _sim;
        string dir = Math.Abs(s.NetBuoyancy) < 0.5 ? "quase neutro" : s.NetBuoyancy > 0 ? "sobe" : "afunda";
        string setup = $"Empuxo {Fmt.F(s.Buoyancy, 1)} N · peso {Fmt.F(s.Weight, 1)} N · saldo {Fmt.F(s.NetBuoyancy, 2)} N ({dir}) · densidade média {Fmt.F(s.MeanDensity, 0)} kg/m³\n" +
                       $"Casco colapsa a {Fmt.F(s.CrushDepth, 1)} m · CG {Fmt.F(Math.Abs(s.BgCm), 1)} cm {(s.BgCm >= 0 ? "abaixo" : "acima")} do centro de empuxo" +
                       (P("forwardThrust").Visible ? $" · velocidade de cruzeiro {Fmt.F(s.CruiseSpeed, 2)} m/s com {Fmt.F(s.PropPower, 0)} W" : "");
        if (!ShowResults) return setup;
        string res = s.Crushed ? $"💥 O casco implodiu a {Fmt.F(s.MaxDepth, 1)} m ({Fmt.F(s.PressureBar, 2)} bar)!"
            : $"Profundidade final {Fmt.F(s.FinalDepth, 2)} m · máxima {Fmt.F(s.MaxDepth, 2)} m ({Fmt.F(s.PressureBar, 2)} bar abs) · " +
              (!s.Pid ? $"velocidade vertical máx {Fmt.F(s.MaxSpeed, 2)} m/s"
                  : (s.SettlingTime >= 999 ? "não se acomodou" : $"acomoda em {Fmt.F(s.SettlingTime, 1)} s") + $" · sobressinal {Fmt.F(s.Overshoot, 1)} %");
        return setup + "\n" + res;
    }

    // ---------------------------------------------------------------- drawing

    void Draw(DrawingContext dc, SceneCanvas c)
    {
        var s = _sim;
        double w = c.ActualWidth, h = c.ActualHeight;
        if (w < 50 || h < 50) return;
        double sideW = Math.Min(230, w * 0.48);
        var side = new Rect(0, 0, sideW, h);
        double surface = 26, bottom = h - 18;
        double floor = s.Floor;
        double Y(double depth) => surface + depth / floor * (bottom - surface);

        dc.DrawRectangle(SkyB, null, new Rect(0, 0, sideW, surface));
        var water = new LinearGradientBrush(((SolidColorBrush)WaterTop).Color, ((SolidColorBrush)WaterDeep).Color, 90);
        water.Freeze();
        dc.DrawRectangle(water, null, new Rect(0, surface, sideW, bottom - surface));
        dc.DrawRectangle(Sand, null, new Rect(0, bottom, sideW, h - bottom));
        Text(dc, c, s.Salt ? "mar (água salgada)" : "lago (água doce)", 6, 5, 11, Ui.TextDim);

        // depth gauge with ticks
        double gx = sideW - 34;
        dc.DrawLine(PenOf(Ui.TextDim, 1), new Point(gx, surface), new Point(gx, bottom));
        double step = NiceMax(floor / 5);
        for (double dd = 0; dd <= floor + 1e-9; dd += step)
        {
            dc.DrawLine(PenOf(Ui.TextDim, 1), new Point(gx - 4, Y(dd)), new Point(gx, Y(dd)));
            Text(dc, c, Fmt.F(dd, 0) + " m", gx + 3, Y(dd) - 7, 10, Ui.TextDim);
        }
        if (s.CrushDepth < floor)
        {
            var dash = new Pen(Crush, 1.5) { DashStyle = DashStyles.Dash };
            dash.Freeze();
            dc.DrawLine(dash, new Point(0, Y(s.CrushDepth)), new Point(gx, Y(s.CrushDepth)));
            Text(dc, c, "limite do casco", 6, Y(s.CrushDepth) - 15, 10.5, Crush);
        }
        if (s.Pid) dc.DrawLine(PenOf(Target, 1), new Point(gx - 10, Y(s.TargetM)), new Point(gx, Y(s.TargetM)));

        bool run = HasRun && s.Depth.Count > 1;
        double tEnd = run ? (s.Crushed ? s.CrushTime : s.Duration) : 1;
        double tNow = double.IsNaN(PlayTime) ? tEnd : Math.Min(PlayTime, tEnd);
        double depth = run ? At(s.Depth, tNow) : Math.Max(s.Start, 0);
        bool crushedNow = run && s.Crushed && tNow >= s.CrushTime - 1e-6;

        // the submarine
        double cx = (gx - 10) / 2 + 4, cy = Math.Min(Y(depth), bottom - 12);
        if (depth <= 0.01 && s.NetBuoyancy > 0) cy = surface - 2;
        double len = Math.Min(110, gx - 30), ht = 22;
        double tilt = s.BgCm < 0 ? 20 : 0;
        dc.PushTransform(new RotateTransform(tilt, cx, cy));
        var hullBrush = crushedNow ? Crush : Hull;
        if (crushedNow) dc.PushTransform(new ScaleTransform(1, 0.55, cx, cy));
        dc.DrawRoundedRectangle(hullBrush, PenOf(HullDark, 1.5), new Rect(cx - len / 2, cy - ht / 2, len, ht), ht / 2, ht / 2);
        dc.DrawRoundedRectangle(hullBrush, PenOf(HullDark, 1.5), new Rect(cx - 8, cy - ht / 2 - 10, 22, 11), 3, 3);
        double fill = run ? s.FillAt(tNow) : s.FillPct / 100;
        if (s.BallastL > 0)
        {
            var tank = new Rect(cx - len / 4, cy - 5, len / 2, 10);
            dc.DrawRectangle(Ui.Bg0, null, tank);
            dc.DrawRectangle(CbB, null, new Rect(tank.Left, tank.Bottom - tank.Height * fill, tank.Width, tank.Height * fill));
        }
        if (crushedNow) dc.Pop();
        // CB (blue) at the center, CG (orange) shifted by BG (exaggerated ×3, limited)
        double gOff = Math.Clamp(s.BgCm * 3, -ht / 2 + 2, ht / 2 - 2);
        dc.DrawEllipse(CbB, PenOf(Ui.Bg0, 1), new Point(cx + len / 2 - 14, cy), 3.5, 3.5);
        dc.DrawEllipse(CgB, PenOf(Ui.Bg0, 1), new Point(cx + len / 2 - 14, cy + gOff), 3.5, 3.5);
        dc.Pop();
        if (run && double.IsNaN(PlayTime) == false && !crushedNow)
            for (int i = 0; i < 4; i++)
            {
                double bx = cx - len / 2 - 8 - (tNow * 30 + i * 13) % 50, by = cy + Math.Sin(tNow * 3 + i) * 4;
                dc.DrawEllipse(null, PenOf(Bubble, 1), new Point(bx, by), 2.5, 2.5);
            }
        Text(dc, c, $"{Fmt.F(Math.Max(depth, 0), 1)} m · {Fmt.F((SubmarineSim.P0 + s.Rho * SubmarineSim.G * Math.Max(depth, 0)) / 1e5, 2)} bar",
            cx, cy + ht / 2 + 20 < bottom ? cy + ht / 2 + 6 : cy - ht / 2 - 28, 11, Ui.Text, true, TextAlignment.Center);
        if (crushedNow) Text(dc, c, "💥 implodiu!", cx, cy - 34, 13, Crush, true, TextAlignment.Center);
        Text(dc, c, "CB", 15, h - 15, 10, Ui.TextDim);
        Text(dc, c, "CG", 48, h - 15, 10, Ui.TextDim);
        dc.DrawEllipse(CbB, null, new Point(9, h - 7.5), 3.5, 3.5);
        dc.DrawEllipse(CgB, null, new Point(42, h - 7.5), 3.5, 3.5);

        // depth vs time plot
        var plot = new Rect(sideW + 12, 10, w - sideW - 20, h - 30);
        double yMax = NiceMax(Math.Max(Math.Max(run ? s.MaxDepth : 0, s.Pid ? s.TargetM : 0), 1) * 1.1);
        Plot(dc, c, plot, run ? s.Depth : Array.Empty<(double, double)>(), Math.Max(tEnd, 1), 0, yMax, tNow, Ui.Accent,
            "profundidade", v => Fmt.F(v, 0) + " m", yDown: true);
        if (s.Pid && s.TargetM <= yMax)
        {
            var dash = new Pen(Target, 1) { DashStyle = DashStyles.Dash };
            dash.Freeze();
            double ty = plot.Top + s.TargetM / yMax * plot.Height;
            dc.DrawLine(dash, new Point(plot.Left, ty), new Point(plot.Right, ty));
            Text(dc, c, "alvo", plot.Right - 4, ty + 2, 10.5, Target, align: TextAlignment.Right);
        }
        if (!run) Text(dc, c, "Ajuste e aperte ▶ Mergulhar", plot.Left + plot.Width / 2, plot.Top + plot.Height / 2, 12.5, Ui.TextDim, align: TextAlignment.Center);
    }
}
