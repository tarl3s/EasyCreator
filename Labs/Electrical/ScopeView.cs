using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyCreator.Electrical;

/// <summary>A small oscilloscope / plotter: up to 3 traces against time (or frequency, log axis). Traces with the same
/// unit share a vertical scale (left axis = first unit, right axis = second). <see cref="Reveal"/> animates a sweep.</summary>
public sealed class ScopeView : FrameworkElement
{
    public sealed class Series { public string Name = ""; public string Unit = ""; public Brush Color = Ui.Accent; public double[] Y = Array.Empty<double>(); }

    public double[] X { get; set; } = Array.Empty<double>();
    public List<Series> Data { get; } = new();
    public bool LogX { get; set; }
    public string XUnit { get; set; } = "s";
    public double? Marker { get; set; }
    public double Reveal { get; set; } = 1;
    public string Empty { get; set; } = "Clique em ▶ para simular.";

    public static readonly Brush[] Colors = { Ui.Make("#18B98A"), Ui.Make("#FFC857"), Ui.Make("#5AA9FF") };
    static readonly Pen GridPen = Frozen(new Pen(Ui.Make("#2A2A33"), 1)), AxisPen = Frozen(new Pen(Ui.Make("#3A3A45"), 1)),
        MarkerPen = Frozen(new Pen(Ui.Make("#FF5C6C"), 1.4) { DashStyle = DashStyles.Dash });

    static Pen Frozen(Pen p) { p.Freeze(); return p; }

    protected override void OnRender(DrawingContext dc)
    {
        double w = ActualWidth, h = ActualHeight;
        dc.DrawRoundedRectangle(Ui.Code, new Pen(Ui.Border, 1), new Rect(0.5, 0.5, Math.Max(w - 1, 0), Math.Max(h - 1, 0)), 6, 6);
        if (X.Length < 2 || Data.Count == 0)
        {
            var ft = Text(Empty, 13, Ui.TextDim);
            dc.DrawText(ft, new Point((w - ft.Width) / 2, (h - ft.Height) / 2));
            return;
        }
        var units = Data.Select(d => d.Unit).Distinct().Take(2).ToList();
        double left = 56, right = units.Count > 1 ? 56 : 14, top = 22, bottom = 18;
        var plot = new Rect(left, top, Math.Max(w - left - right, 10), Math.Max(h - top - bottom, 10));
        for (int i = 0; i <= 4; i++) dc.DrawLine(GridPen, new Point(plot.Left, plot.Top + plot.Height * i / 4), new Point(plot.Right, plot.Top + plot.Height * i / 4));
        dc.DrawRectangle(null, AxisPen, plot);

        double x0 = X[0], x1 = X[^1];
        double Fx(double x) => LogX
            ? plot.Left + plot.Width * (Math.Log10(Math.Max(x, 1e-12)) - Math.Log10(x0)) / Math.Max(Math.Log10(x1) - Math.Log10(x0), 1e-9)
            : plot.Left + plot.Width * (x - x0) / Math.Max(x1 - x0, 1e-15);

        // X grid and labels.
        if (LogX)
        {
            for (double d = Math.Pow(10, Math.Ceiling(Math.Log10(x0))); d <= x1 * 1.0001; d *= 10)
            {
                double px = Fx(d);
                dc.DrawLine(GridPen, new Point(px, plot.Top), new Point(px, plot.Bottom));
                var ft = Text(SI.Format(d, XUnit, 2), 10, Ui.TextDim);
                dc.DrawText(ft, new Point(Math.Clamp(px - ft.Width / 2, 0, w - ft.Width), plot.Bottom + 2));
            }
        }
        else
        {
            for (int i = 1; i < 5; i++) dc.DrawLine(GridPen, new Point(plot.Left + plot.Width * i / 5, plot.Top), new Point(plot.Left + plot.Width * i / 5, plot.Bottom));
            var a = Text(SI.Format(x0, XUnit, 2), 10, Ui.TextDim);
            var b = Text(SI.Format(x1, XUnit, 3), 10, Ui.TextDim);
            dc.DrawText(a, new Point(plot.Left, plot.Bottom + 2));
            dc.DrawText(b, new Point(plot.Right - b.Width, plot.Bottom + 2));
        }

        // One vertical scale per unit.
        var scale = new Dictionary<string, (double lo, double hi)>();
        foreach (var u in units)
        {
            double lo = 0, hi = 0;
            foreach (var s in Data.Where(d => d.Unit == u))
                foreach (var y in s.Y) if (!double.IsNaN(y)) { lo = Math.Min(lo, y); hi = Math.Max(hi, y); }
            if (hi - lo < 1e-12) hi = lo + 1;
            double pad = (hi - lo) * 0.08;
            scale[u] = (lo - (lo < 0 ? pad : 0), hi + pad);
        }
        for (int k = 0; k < units.Count; k++)
        {
            var (lo, hi) = scale[units[k]];
            var top1 = Text(SI.Format(hi, units[k], 2), 10, Ui.TextDim);
            var bot1 = Text(SI.Format(lo, units[k], 2), 10, Ui.TextDim);
            double xl = k == 0 ? plot.Left - 4 - top1.Width : plot.Right + 4;
            double xb = k == 0 ? plot.Left - 4 - bot1.Width : plot.Right + 4;
            dc.DrawText(top1, new Point(xl, plot.Top - 2));
            dc.DrawText(bot1, new Point(xb, plot.Bottom - bot1.Height + 2));
            if (lo < 0 && hi > 0)
            {
                double zy = plot.Bottom - plot.Height * (0 - lo) / (hi - lo);
                dc.DrawLine(AxisPen, new Point(plot.Left, zy), new Point(plot.Right, zy));
            }
        }

        // Traces.
        int n = X.Length, last = (int)Math.Round((n - 1) * Math.Clamp(Reveal, 0, 1));
        dc.PushClip(new RectangleGeometry(plot));
        foreach (var s in Data)
        {
            if (s.Y.Length != n || last < 1) continue;
            var (lo, hi) = scale[s.Unit];
            var pen = new Pen(s.Color, 1.8) { LineJoin = PenLineJoin.Round };
            pen.Freeze();
            var g = new StreamGeometry();
            using (var c = g.Open())
            {
                c.BeginFigure(new Point(Fx(X[0]), plot.Bottom - plot.Height * (s.Y[0] - lo) / (hi - lo)), false, false);
                int stride = Math.Max(1, n / 1500);
                for (int i = stride; i <= last; i += stride)
                    c.LineTo(new Point(Fx(X[i]), plot.Bottom - plot.Height * (s.Y[i] - lo) / (hi - lo)), true, false);
            }
            g.Freeze();
            dc.DrawGeometry(null, pen, g);
        }
        if (Marker is double m && m >= Math.Min(x0, x1) && m <= Math.Max(x0, x1))
        {
            double px = Fx(m);
            dc.DrawLine(MarkerPen, new Point(px, plot.Top), new Point(px, plot.Bottom));
        }
        dc.Pop();

        // Legend.
        double lx = plot.Left + 4;
        foreach (var s in Data)
        {
            dc.DrawRectangle(s.Color, null, new Rect(lx, 8, 12, 3));
            var ft = Text(s.Name, 11, Ui.Text);
            dc.DrawText(ft, new Point(lx + 16, 2));
            lx += 16 + ft.Width + 14;
        }
        if (Marker is double mk)
        {
            var ft = Text("f atual: " + SI.Format(mk, XUnit), 11, Ui.Bad);
            dc.DrawText(ft, new Point(plot.Right - ft.Width - 2, 2));
        }
    }

    FormattedText Text(string s, double size, Brush b) =>
        new(s, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), size, b, VisualTreeHelper.GetDpi(this).PixelsPerDip);
}

/// <summary>A drawn slider (click or drag anywhere on it; the wheel steps). Reports a 0..1 fraction.</summary>
public sealed class SliderBar : FrameworkElement
{
    public double Fraction { get; set; }
    /// <summary>(fraction, final): final = mouse released.</summary>
    public event Action<double, bool>? Changed;
    public event Action<int>? Stepped;

    public SliderBar() { Height = 24; Cursor = Cursors.Hand; }

    protected override void OnRender(DrawingContext dc)
    {
        double w = ActualWidth, cy = ActualHeight / 2, r = 7;
        double x = r + (w - 2 * r) * Math.Clamp(Fraction, 0, 1);
        dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, w, ActualHeight));
        dc.DrawRoundedRectangle(Ui.Bg3, null, new Rect(r, cy - 2.5, Math.Max(w - 2 * r, 0), 5), 2.5, 2.5);
        dc.DrawRoundedRectangle(Ui.Accent, null, new Rect(r, cy - 2.5, Math.Max(x - r, 0), 5), 2.5, 2.5);
        dc.DrawEllipse(Ui.Text, new Pen(Ui.Accent, 2), new Point(x, cy), r, r);
    }

    double FracAt(Point p) => Math.Clamp((p.X - 7) / Math.Max(ActualWidth - 14, 1), 0, 1);

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        CaptureMouse();
        Changed?.Invoke(FracAt(e.GetPosition(this)), false);
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (IsMouseCaptured) Changed?.Invoke(FracAt(e.GetPosition(this)), false);
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!IsMouseCaptured) return;
        ReleaseMouseCapture();
        Changed?.Invoke(FracAt(e.GetPosition(this)), true);
        e.Handled = true;
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        Stepped?.Invoke(e.Delta > 0 ? 1 : -1);
        e.Handled = true;
    }
}
