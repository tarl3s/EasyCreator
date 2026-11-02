using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyCreator.Electrical;

/// <summary>Draws a <see cref="Netlist"/> as a schematic (IEC symbols: resistor = rectangle) from the grid layout, and
/// lets the student click a node or a component (the virtual multimeter) or a switch (toggles it).</summary>
public sealed class SchematicView : FrameworkElement
{
    public sealed class Seg { public Elem E = null!; public Point P, Q; }

    public List<Seg> Segs { get; } = new();
    /// <summary>Clickable non-ground drawing points (grid units).</summary>
    public Dictionary<string, Point> Points { get; } = new(StringComparer.Ordinal);
    public List<Point> Grounds { get; } = new();
    readonly Dictionary<string, int> _degree = new(StringComparer.Ordinal);

    public HashSet<string> Adjustable { get; } = new(StringComparer.OrdinalIgnoreCase);
    /// <summary>Selected element name, or "@node".</summary>
    public string? Selected { get; set; }
    public Func<Elem, string> ValueText { get; set; } = _ => "";
    /// <summary>0..1 brightness for lamps and LEDs.</summary>
    public Func<Elem, double> Glow { get; set; } = _ => 0;
    public bool AcSources { get; set; }
    /// <summary>Raised with an element name or "@node" when the student clicks something.</summary>
    public event Action<string>? Picked;

    static readonly Pen WirePen = MakePen("#8C8C99", 2), SymPen = MakePen("#E6E6EA", 2), SelPen = MakePen("#18B98A", 2.6),
        ThinPen = MakePen("#E6E6EA", 1.4), SelThin = MakePen("#18B98A", 1.6);
    static readonly Brush BodyFill = Ui.Bg1;

    static Pen MakePen(string hex, double w)
    {
        var p = new Pen(Ui.Make(hex), w) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
        p.Freeze();
        return p;
    }

    // ───────────────────────── layout ─────────────────────────

    /// <summary>Places every element: "n1=0,0 n2=3,0" positions; elements to an unplaced ground go 2 units down.</summary>
    public void Build(Netlist net, string layout)
    {
        Segs.Clear(); Points.Clear(); Grounds.Clear(); _degree.Clear();
        var pos = new Dictionary<string, Point>(StringComparer.Ordinal);
        foreach (var item in (layout ?? "").Split(new[] { ' ', ';', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries))
        {
            int eq = item.IndexOf('=');
            if (eq <= 0) continue;
            var xy = item[(eq + 1)..].Split(',');
            if (xy.Length == 2 && double.TryParse(xy[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
                && double.TryParse(xy[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y))
                pos[item[..eq]] = new Point(x, y);
        }
        // Unplaced non-ground points: a row to the right of everything else.
        double nextX = pos.Count == 0 ? 0 : pos.Values.Max(p => p.X) + 3;
        foreach (var n in net.Names)
            if (!pos.ContainsKey(n) && !Netlist.IsGround(n)) { pos[n] = new Point(nextX, 0); nextX += 3; }

        foreach (var e in net.Elems)
        {
            bool ha = pos.TryGetValue(e.A, out var pa), hb = pos.TryGetValue(e.B, out var pb);
            if (!ha && !hb) continue;                      // between two unplaced grounds: nothing to draw
            if (!ha) { pa = pb + new Vector(0, 2); Grounds.Add(pa); }
            if (!hb) { pb = pa + new Vector(0, 2); Grounds.Add(pb); }
            Segs.Add(new Seg { E = e, P = pa, Q = pb });
            foreach (var n in new[] { e.A, e.B })
                if (pos.ContainsKey(n)) _degree[n] = _degree.GetValueOrDefault(n) + 1;
        }
        foreach (var (n, p) in pos)
        {
            if (!_degree.ContainsKey(n)) continue;
            if (Netlist.IsGround(n)) Grounds.Add(p);
            else Points[n] = p;
        }
        InvalidateVisual();
    }

    // ───────────────────────── coordinates ─────────────────────────

    double _scale = 40;
    Vector _offset;
    Point _center;

    void Fit()
    {
        var all = Segs.SelectMany(s => new[] { s.P, s.Q }).Concat(Grounds).ToList();
        if (all.Count == 0) return;
        double x0 = all.Min(p => p.X), x1 = all.Max(p => p.X), y0 = all.Min(p => p.Y), y1 = all.Max(p => p.Y);
        double w = Math.Max(ActualWidth, 100), h = Math.Max(ActualHeight, 80);
        const double mx = 70, myTop = 52, myBottom = 22;
        double sx = (w - 2 * mx) / Math.Max(x1 - x0, 0.01), sy = (h - myTop - myBottom) / Math.Max(y1 - y0, 0.01);
        _scale = Math.Clamp(Math.Min(sx, sy), 12, 62);
        double cw = (x1 - x0) * _scale, ch = (y1 - y0) * _scale;
        _offset = new Vector((w - cw) / 2 - x0 * _scale, myTop + (h - myTop - myBottom - ch) / 2 - y0 * _scale);
        _center = Px(new Point((x0 + x1) / 2, (y0 + y1) / 2));
    }

    Point Px(Point g) => new(g.X * _scale + _offset.X, g.Y * _scale + _offset.Y);

    // ───────────────────────── drawing ─────────────────────────

    protected override void OnRender(DrawingContext dc)
    {
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, ActualWidth, ActualHeight)); // hit-testable
        if (Segs.Count == 0) return;
        Fit();
        foreach (var s in Segs) DrawElement(dc, s);
        foreach (var g in Grounds.Distinct()) DrawGround(dc, Px(g));
        foreach (var (n, p) in Points)
        {
            int d = _degree.GetValueOrDefault(n);
            var c = Px(p);
            if (d >= 3 || d == 1) dc.DrawEllipse(Ui.Make("#B8B8C4"), null, c, 3.2, 3.2);
            if (Selected == "@" + n) dc.DrawEllipse(null, SelPen, c, 7, 7);
        }
        foreach (var s in Segs) if (s.E.Kind != ElemKind.W) DrawLabel(dc, s);
    }

    static double HalfLen(Elem e) => e.Kind switch
    {
        ElemKind.R => e.Lamp ? 12 : 20, ElemKind.C => 5, ElemKind.L => 20, ElemKind.V or ElemKind.I => 15,
        ElemKind.D => 8, ElemKind.S => 14, ElemKind.M or ElemKind.A => 13, _ => 0,
    };

    static double HalfWidth(Elem e) => e.Kind switch
    {
        ElemKind.R => e.Lamp ? 12 : 7, ElemKind.C => 12, ElemKind.L => 6, ElemKind.V or ElemKind.I => 15,
        ElemKind.D => 9, ElemKind.S => 12, ElemKind.M or ElemKind.A => 13, _ => 2,
    };

    void DrawElement(DrawingContext dc, Seg s)
    {
        Point p = Px(s.P), q = Px(s.Q);
        var e = s.E;
        bool sel = Selected != null && Selected.Equals(e.Name, StringComparison.OrdinalIgnoreCase);
        if (e.Kind == ElemKind.W) { dc.DrawLine(sel ? SelPen : WirePen, p, q); return; }
        double len = (q - p).Length, h = Math.Min(HalfLen(e), len / 2 - 2);
        var dir = len > 0 ? (q - p) / len : new Vector(1, 0);
        var mid = p + (q - p) / 2;
        dc.DrawLine(WirePen, p, mid - dir * h);
        dc.DrawLine(WirePen, mid + dir * h, q);

        var pen = sel ? SelPen : SymPen;
        var thin = sel ? SelThin : ThinPen;
        double ang = Math.Atan2(dir.Y, dir.X) * 180 / Math.PI;
        dc.PushTransform(new TranslateTransform(mid.X, mid.Y));
        dc.PushTransform(new RotateTransform(ang));
        switch (e.Kind)
        {
            case ElemKind.R when e.Lamp:
            {
                double g = Math.Clamp(Glow(e), 0, 1);
                var fill = g > 0.02 ? new SolidColorBrush(Color.FromArgb((byte)(60 + 195 * g), 255, 214, 90)) : BodyFill;
                dc.DrawEllipse(fill, pen, new Point(0, 0), 12, 12);
                dc.DrawLine(thin, new Point(-8.5, -8.5), new Point(8.5, 8.5));
                dc.DrawLine(thin, new Point(-8.5, 8.5), new Point(8.5, -8.5));
                break;
            }
            case ElemKind.R:
                dc.DrawRoundedRectangle(BodyFill, pen, new Rect(-h, -7, 2 * h, 14), 2, 2);
                break;
            case ElemKind.C:
                dc.DrawLine(pen, new Point(-5, -12), new Point(-5, 12));
                dc.DrawLine(pen, new Point(5, -12), new Point(5, 12));
                break;
            case ElemKind.L:
            {
                var g = new StreamGeometry();
                using (var c = g.Open())
                {
                    c.BeginFigure(new Point(-h, 0), false, false);
                    double step = 2 * h / 4;
                    for (int i = 1; i <= 4; i++)
                        c.ArcTo(new Point(-h + i * step, 0), new Size(step / 2, step / 2 * 1.1), 0, false, SweepDirection.Clockwise, true, true);
                }
                g.Freeze();
                dc.DrawGeometry(null, pen, g);
                break;
            }
            case ElemKind.V:
                dc.DrawEllipse(BodyFill, pen, new Point(0, 0), 15, 15);
                break;
            case ElemKind.I:
                dc.DrawEllipse(BodyFill, pen, new Point(0, 0), 15, 15);
                dc.DrawLine(thin, new Point(8, 0), new Point(-8, 0));
                dc.DrawLine(thin, new Point(-8, 0), new Point(-3, -4));
                dc.DrawLine(thin, new Point(-8, 0), new Point(-3, 4));
                break;
            case ElemKind.D:
            {
                double g = e.Led ? Math.Clamp(Glow(e), 0, 1) : 0;
                Brush fill = BodyFill;
                if (g > 0.02)
                {
                    var c = LedColor(e.Color);
                    fill = new SolidColorBrush(Color.FromArgb((byte)(80 + 175 * g), c.R, c.G, c.B));
                }
                var tri = new StreamGeometry();
                using (var c = tri.Open())
                {
                    c.BeginFigure(new Point(-8, -9), true, true);
                    c.LineTo(new Point(-8, 9), true, true);
                    c.LineTo(new Point(8, 0), true, true);
                }
                tri.Freeze();
                dc.DrawGeometry(fill, pen, tri);
                dc.DrawLine(pen, new Point(8, -9), new Point(8, 9));
                if (e.Led)
                    foreach (double ox in new[] { -3.0, 3.0 })
                    {
                        var a = new Point(ox, -12); var b = new Point(ox + 6, -19);
                        dc.DrawLine(thin, a, b);
                        dc.DrawLine(thin, b, new Point(b.X - 4, b.Y + 0.5));
                        dc.DrawLine(thin, b, new Point(b.X - 0.5, b.Y + 4));
                    }
                break;
            }
            case ElemKind.S:
                dc.DrawEllipse(BodyFill, thin, new Point(-14, 0), 2.6, 2.6);
                dc.DrawEllipse(BodyFill, thin, new Point(14, 0), 2.6, 2.6);
                dc.DrawLine(pen, new Point(-12, 0), e.On ? new Point(12, 0) : new Point(10, -12));
                break;
            case ElemKind.M:
            case ElemKind.A:
                dc.DrawEllipse(BodyFill, pen, new Point(0, 0), 13, 13);
                break;
        }
        dc.Pop(); dc.Pop();

        // Upright details (signs, letters) drawn without rotation so they read normally.
        var brush = sel ? Ui.Accent : Ui.Text;
        if (e.Kind == ElemKind.V)
        {
            if (AcSources || e.Wave == "sin") DrawSine(dc, mid, thin);
            else if (e.Wave == "sq") DrawSquare(dc, mid, thin);
            else
            {
                var plus = mid - dir * 7; var minus = mid + dir * 7;
                dc.DrawLine(thin, plus + new Vector(-3.5, 0), plus + new Vector(3.5, 0));
                dc.DrawLine(thin, plus + new Vector(0, -3.5), plus + new Vector(0, 3.5));
                dc.DrawLine(thin, minus + new Vector(-3.5, 0), minus + new Vector(3.5, 0));
            }
        }
        if (e.Kind is ElemKind.M or ElemKind.A)
        {
            var ft = Text(e.Kind == ElemKind.M ? "V" : "A", 13, brush, true);
            dc.DrawText(ft, new Point(mid.X - ft.Width / 2, mid.Y - ft.Height / 2));
        }
    }

    static void DrawSine(DrawingContext dc, Point c, Pen pen)
    {
        var g = new StreamGeometry();
        using (var ctx = g.Open())
        {
            ctx.BeginFigure(new Point(c.X - 9, c.Y), false, false);
            for (int i = 1; i <= 18; i++)
                ctx.LineTo(new Point(c.X - 9 + i, c.Y - 5 * Math.Sin(i / 18.0 * 2 * Math.PI)), true, true);
        }
        g.Freeze();
        dc.DrawGeometry(null, pen, g);
    }

    static void DrawSquare(DrawingContext dc, Point c, Pen pen)
    {
        var pts = new[] { (-9, 4), (-9, -4), (0, -4), (0, 4), (9, 4), (9, -4) };
        for (int i = 1; i < pts.Length; i++)
            dc.DrawLine(pen, new Point(c.X + pts[i - 1].Item1, c.Y + pts[i - 1].Item2), new Point(c.X + pts[i].Item1, c.Y + pts[i].Item2));
    }

    static void DrawGround(DrawingContext dc, Point g)
    {
        dc.DrawLine(WirePen, new Point(g.X - 10, g.Y), new Point(g.X + 10, g.Y));
        dc.DrawLine(WirePen, new Point(g.X - 6, g.Y + 4.5), new Point(g.X + 6, g.Y + 4.5));
        dc.DrawLine(WirePen, new Point(g.X - 2.5, g.Y + 9), new Point(g.X + 2.5, g.Y + 9));
    }

    public static Color LedColor(string name) => (name ?? "").ToLowerInvariant() switch
    {
        "green" or "verde" => Color.FromRgb(70, 230, 110),
        "blue" or "azul" => Color.FromRgb(80, 150, 255),
        "yellow" or "amarelo" => Color.FromRgb(255, 220, 60),
        "white" or "branco" => Color.FromRgb(240, 240, 255),
        _ => Color.FromRgb(255, 70, 70),
    };

    void DrawLabel(DrawingContext dc, Seg s)
    {
        var e = s.E;
        Point p = Px(s.P), q = Px(s.Q);
        var mid = p + (q - p) / 2;
        double len = (q - p).Length;
        var dir = len > 0 ? (q - p) / len : new Vector(1, 0);
        var n = new Vector(-dir.Y, dir.X);
        var away = e.Side switch
        {
            "up" => new Vector(0, -1000), "down" => new Vector(0, 1000), "left" => new Vector(-1000, 0), "right" => new Vector(1000, 0),
            _ => mid - _center,
        };
        double side = n.X * away.X + n.Y * away.Y;
        if (Math.Abs(side) < 1) side = n.Y < 0 ? 1 : (n.Y > 0 ? -1 : (n.X > 0 ? 1 : -1)); // centered: above / right
        if (side < 0) n = -n;
        var anchor = mid + n * (HalfWidth(e) + 5);

        bool adj = Adjustable.Contains(e.Name);
        bool sel = Selected != null && Selected.Equals(e.Name, StringComparison.OrdinalIgnoreCase);
        var name = Text(e.DisplayName, 12, sel ? Ui.Accent : Ui.Text, true);
        string val = ValueText(e);
        var value = Text(val, 11.5, adj || e.Kind is ElemKind.M or ElemKind.A ? Ui.Accent : Ui.TextDim, e.Kind is ElemKind.M or ElemKind.A);
        double w = Math.Max(name.Width, val.Length > 0 ? value.Width : 0), hgt = name.Height + (val.Length > 0 ? value.Height : 0);
        double x = n.X > 0.5 ? anchor.X : n.X < -0.5 ? anchor.X - w : anchor.X - w / 2;
        double y = n.Y < -0.5 ? anchor.Y - hgt : n.Y > 0.5 ? anchor.Y : anchor.Y - hgt / 2;
        double Align(double tw) => n.X > 0.5 ? x : n.X < -0.5 ? x + w - tw : x + (w - tw) / 2;
        dc.DrawText(name, new Point(Align(name.Width), y));
        if (val.Length > 0) dc.DrawText(value, new Point(Align(value.Width), y + name.Height));
    }

    FormattedText Text(string s, double size, Brush b, bool bold = false) =>
        new(s, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, bold ? FontWeights.SemiBold : FontWeights.Normal, FontStretches.Normal),
            size, b, VisualTreeHelper.GetDpi(this).PixelsPerDip);

    // ───────────────────────── interaction ─────────────────────────

    string? HitTest(Point m)
    {
        if (Segs.Count == 0) return null;
        Fit();
        foreach (var (n, p) in Points)
            if ((Px(p) - m).Length <= 9) return "@" + n;
        string? best = null; double bestD = 12;
        foreach (var s in Segs)
        {
            if (s.E.Kind == ElemKind.W) continue;
            Point a = Px(s.P), b = Px(s.Q);
            var mid = a + (b - a) / 2;
            double len = (b - a).Length;
            var dir = len > 0 ? (b - a) / len : new Vector(1, 0);
            double h = Math.Max(HalfLen(s.E), 10);
            double d = DistToSeg(m, mid - dir * h, mid + dir * h);
            if (d < bestD) { bestD = d; best = s.E.Name; }
        }
        return best;
    }

    static double DistToSeg(Point p, Point a, Point b)
    {
        var ab = b - a;
        double t = ab.LengthSquared < 1e-9 ? 0 : Math.Clamp(((p - a) * ab) / ab.LengthSquared, 0, 1);
        return (p - (a + ab * t)).Length;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        Cursor = HitTest(e.GetPosition(this)) != null ? Cursors.Hand : Cursors.Arrow;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);
        var hit = HitTest(e.GetPosition(this));
        if (hit == null) return;
        Picked?.Invoke(hit);
        e.Handled = true;
    }
}
