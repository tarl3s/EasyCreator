using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace EasyCreator.Vehicles;

/// <summary>A tiny immediate-mode drawing surface: <c>InvalidateVisual()</c> calls the callback with a DrawingContext.</summary>
public sealed class DrawSurface : FrameworkElement
{
    readonly Action<DrawingContext, Size> _draw;

    public DrawSurface(Action<DrawingContext, Size> draw)
    {
        _draw = draw;
        ClipToBounds = true;
    }

    protected override void OnRender(DrawingContext dc)
    {
        var size = RenderSize;
        if (size.Width < 4 || size.Height < 4) return;
        try { Draw.Dip = VisualTreeHelper.GetDpi(this).PixelsPerDip; } catch { }
        dc.DrawRectangle(Ui.Code, null, new Rect(size));
        try { _draw(dc, size); } catch { /* drawing never breaks the lesson */ }
    }
}

/// <summary>Drawing helpers shared by the vehicle labs: cached pens, text, gauges, arrows and a simple 2D plot.</summary>
public static class Draw
{
    internal static double Dip = 1;
    static readonly Typeface Face = new("Segoe UI");
    static readonly Typeface BoldFace = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
    static readonly CultureInfo Br = CultureInfo.GetCultureInfo("pt-BR");

    public static readonly Brush Sky = Ui.Make("#17202B"), SkyLow = Ui.Make("#1F2B38"), Ground = Ui.Make("#2B2B31"),
        Road = Ui.Make("#33333B"), GridLine = Ui.Make("#24242C"), Axis = Ui.Make("#5C5C68"), Blue = Ui.Make("#4DA3FF"),
        Orange = Ui.Make("#FFA94D"), Yellow = Ui.Make("#FFD84D"), Red = Ui.Bad, Green = Ui.Accent, White = Ui.Text,
        Dim = Ui.TextDim, Body = Ui.Make("#3D7BD9"), Body2 = Ui.Make("#2C5DAA"), Glass = Ui.Make("#9CC8F0"),
        Tire = Ui.Make("#111114"), Metal = Ui.Make("#8C8C96"), Grass = Ui.Make("#1F3A2A"), Purple = Ui.Make("#B57CFF"),
        Smoke = Ui.Make("#66C8C8D0"), Shade = Ui.Make("#AA000000");

    static readonly Dictionary<(Brush, double, bool), Pen> Pens = new();

    public static Pen Pen(Brush b, double w = 1, bool dash = false)
    {
        if (Pens.TryGetValue((b, w, dash), out var p)) return p;
        p = new Pen(b, w) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round };
        if (dash) p.DashStyle = new DashStyle(new double[] { 3, 3 }, 0);
        p.Freeze();
        Pens[(b, w, dash)] = p;
        return p;
    }

    /// <summary>align: 0 = left, 1 = centre, 2 = right; y is the top of the text.</summary>
    public static Size Text(DrawingContext dc, string s, double x, double y, double size = 11, Brush? b = null, int align = 0, bool bold = false)
    {
        var ft = new FormattedText(s ?? "", Br, FlowDirection.LeftToRight, bold ? BoldFace : Face, size, b ?? White, Dip);
        double dx = align == 1 ? -ft.Width / 2 : align == 2 ? -ft.Width : 0;
        dc.DrawText(ft, new Point(x + dx, y));
        return new Size(ft.Width, ft.Height);
    }

    public static void Arrow(DrawingContext dc, Point a, Point b, Brush brush, double w = 2.5)
    {
        double len = (b - a).Length;
        if (len < 2) return;
        var pen = Pen(brush, w);
        dc.DrawLine(pen, a, b);
        var d = (b - a) / len;
        var n = new Vector(-d.Y, d.X);
        double h = Math.Min(8, len * 0.5);
        dc.DrawLine(pen, b, b - d * h + n * h * 0.55);
        dc.DrawLine(pen, b, b - d * h - n * h * 0.55);
    }

    public static void Arc(DrawingContext dc, Point c, double r, double fromDeg, double toDeg, Pen pen)
    {
        if (Math.Abs(fromDeg - toDeg) < 0.3) return;
        Point Pt(double d) => new(c.X + r * Math.Cos(d * Math.PI / 180), c.Y - r * Math.Sin(d * Math.PI / 180));
        var g = new StreamGeometry();
        using (var ctx = g.Open())
        {
            ctx.BeginFigure(Pt(fromDeg), false, false);
            ctx.ArcTo(Pt(toDeg), new Size(r, r), 0, Math.Abs(fromDeg - toDeg) > 180,
                toDeg < fromDeg ? SweepDirection.Clockwise : SweepDirection.Counterclockwise, true, false);
        }
        g.Freeze();
        dc.DrawGeometry(null, pen, g);
    }

    /// <summary>Round gauge (270° sweep) with a needle; <paramref name="text"/> is printed in the middle.</summary>
    public static void Gauge(DrawingContext dc, Point c, double r, double value, double max, string text, string label, Brush? color = null, double redFrom = -1)
    {
        dc.DrawEllipse(Shade, Pen(Axis, 1), c, r + 6, r + 6);
        Arc(dc, c, r, 225, -45, Pen(GridLine, 5));
        if (redFrom > 0 && redFrom < max) Arc(dc, c, r, 225 - 270 * redFrom / max, -45, Pen(Red, 5));
        double f = max > 0 ? Math.Clamp(value / max, 0, 1) : 0;
        Arc(dc, c, r, 225, 225 - 270 * f, Pen(color ?? Green, 5));
        double ang = (225 - 270 * f) * Math.PI / 180;
        dc.DrawLine(Pen(White, 2), c, new Point(c.X + (r - 4) * Math.Cos(ang), c.Y - (r - 4) * Math.Sin(ang)));
        dc.DrawEllipse(White, null, c, 2.5, 2.5);
        Text(dc, text, c.X, c.Y + r * 0.25, 11, White, 1, true);
        Text(dc, label, c.X, c.Y + r + 2, 9.5, Dim, 1);
    }

    /// <summary>Horizontal bar 0..1 (battery, fuel…).</summary>
    public static void Bar(DrawingContext dc, Rect r, double f, string label, Brush? color = null)
    {
        f = double.IsNaN(f) ? 0 : Math.Clamp(f, 0, 1);
        dc.DrawRoundedRectangle(GridLine, Pen(Axis, 1), r, 3, 3);
        var fill = f < 0.2 ? Red : color ?? Green;
        if (f > 0) dc.DrawRoundedRectangle(fill, null, new Rect(r.X + 1, r.Y + 1, Math.Max(0, (r.Width - 2) * f), r.Height - 2), 2, 2);
        Text(dc, label, r.X, r.Y - 14, 10, Dim);
    }

    public static string N(double v, string fmt = "0.#")
    {
        if (double.IsNaN(v) || double.IsInfinity(v)) return "—";
        var s = v.ToString(fmt, Br);
        return s.Length > 1 && s[0] == '-' && s.Trim('-', '0', ',', '.').Length == 0 ? s[1..] : s;
    }

    /// <summary>A "nice" upper bound for an axis (1, 2, 2.5, 5 × 10ⁿ).</summary>
    public static double Nice(double v)
    {
        if (!(v > 0) || double.IsInfinity(v)) return 1;
        double e = Math.Pow(10, Math.Floor(Math.Log10(v)));
        foreach (var m in new[] { 1, 1.2, 1.5, 2, 2.5, 3, 4, 5, 6, 8, 10 })
            if (m * e >= v * 0.999) return m * e;
        return 10 * e;
    }
}

/// <summary>Axes, grid and data→pixel mapping for one plot.</summary>
public readonly struct Plot
{
    public readonly Rect R;
    public readonly double X0, X1, Y0, Y1;

    public Plot(DrawingContext dc, Size s, double x0, double x1, double y0, double y1, string xLabel, string yLabel)
    {
        if (!(x1 > x0)) x1 = x0 + 1;
        if (!(y1 > y0)) y1 = y0 + 1;
        X0 = x0; X1 = x1; Y0 = y0; Y1 = y1;
        R = new Rect(40, 20, Math.Max(10, s.Width - 50), Math.Max(10, s.Height - 46));
        Draw.Text(dc, yLabel, 6, 3, 10, Draw.Dim);
        Draw.Text(dc, xLabel, R.Right, R.Bottom + 13, 10, Draw.Dim, 2);
        double sx = Step(X1 - X0), sy = Step(Y1 - Y0);
        for (double fx = Math.Ceiling(X0 / sx - 1e-9) * sx; fx <= X1 + sx * 1e-6; fx += sx)
        {
            double px = X(fx);
            dc.DrawLine(Draw.Pen(Draw.GridLine, 1), new Point(px, R.Top), new Point(px, R.Bottom));
            if (px < R.Right - 40) Draw.Text(dc, Label(fx, sx), px, R.Bottom + 2, 9.5, Draw.Dim, px - R.Left < 8 ? 0 : 1);
        }
        for (double fy = Math.Ceiling(Y0 / sy - 1e-9) * sy; fy <= Y1 + sy * 1e-6; fy += sy)
        {
            double py = Y(fy);
            dc.DrawLine(Draw.Pen(Draw.GridLine, 1), new Point(R.Left, py), new Point(R.Right, py));
            Draw.Text(dc, Label(fy, sy), R.Left - 3, py - 6, 9.5, Draw.Dim, 2);
        }
        dc.DrawLine(Draw.Pen(Draw.Axis, 1), R.BottomLeft, R.BottomRight);
        dc.DrawLine(Draw.Pen(Draw.Axis, 1), R.BottomLeft, R.TopLeft);
    }

    /// <summary>Grid step of about a quarter of the span: 1, 2, 2,5 or 5 × 10ⁿ.</summary>
    static double Step(double span)
    {
        double raw = Math.Max(1e-9, span / 4), e = Math.Pow(10, Math.Floor(Math.Log10(raw)));
        foreach (var m in new[] { 1, 2, 2.5, 5, 10 })
            if (m * e >= raw * 0.999) return m * e;
        return 10 * e;
    }

    static string Label(double v, double step) =>
        Draw.N(Math.Abs(v) < step * 1e-6 ? 0 : v, "0.###");

    public double X(double x) => R.Left + (x - X0) / (X1 - X0) * R.Width;
    public double Y(double y) => R.Bottom - (y - Y0) / (Y1 - Y0) * R.Height;
    public Point P(double x, double y) => new(X(x), Y(y));

    /// <summary>Polyline through (x, y) data points, clipped to the plot area.</summary>
    public void Line(DrawingContext dc, IReadOnlyList<Point> data, Brush b, double w = 2, bool dash = false)
    {
        if (data.Count < 2) return;
        var g = new StreamGeometry();
        using (var ctx = g.Open())
        {
            bool started = false;
            foreach (var d in data)
            {
                if (double.IsNaN(d.X) || double.IsNaN(d.Y) || double.IsInfinity(d.Y)) continue;
                var p = P(d.X, Math.Clamp(d.Y, Y0 - (Y1 - Y0), Y1 + (Y1 - Y0)));
                if (!started) { ctx.BeginFigure(p, false, false); started = true; }
                else ctx.LineTo(p, true, true);
            }
        }
        g.Freeze();
        dc.PushClip(new RectangleGeometry(new Rect(R.X - 1, R.Y - 1, R.Width + 2, R.Height + 2)));
        dc.DrawGeometry(null, Draw.Pen(b, w, dash), g);
        dc.Pop();
    }

    public void HLine(DrawingContext dc, double y, Brush b, string label = "", bool labelLeft = false, bool labelBelow = false)
    {
        if (y < Y0 || y > Y1) return;
        dc.DrawLine(Draw.Pen(b, 1.2, true), new Point(R.Left, Y(y)), new Point(R.Right, Y(y)));
        if (label.Length > 0)
            Draw.Text(dc, label, labelLeft ? R.Left + 3 : R.Right - 2, Y(y) + (labelBelow ? 1 : -14), 9.5, b, labelLeft ? 0 : 2);
    }

    public void VLine(DrawingContext dc, double x, Brush b, string label = "")
    {
        if (x < X0 || x > X1) return;
        dc.DrawLine(Draw.Pen(b, 1.2, true), new Point(X(x), R.Top), new Point(X(x), R.Bottom));
        if (label.Length > 0) Draw.Text(dc, label, X(x) + 3, R.Top, 9.5, b);
    }

    public void Dot(DrawingContext dc, double x, double y, Brush b, double r = 4)
    {
        if (double.IsNaN(x) || double.IsNaN(y)) return;
        dc.DrawEllipse(b, Draw.Pen(Ui.Code, 1.5), P(Math.Clamp(x, X0, X1), Math.Clamp(y, Y0, Y1)), r, r);
    }
}
