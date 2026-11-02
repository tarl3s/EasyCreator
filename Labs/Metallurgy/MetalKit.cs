using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>Small helpers shared by the metallurgy labs (welding, heat treatment, tensile test): a click-and-drag
/// slider, labelled control rows, combo boxes with values in Tag, pt-BR number formatting and frozen drawing brushes.</summary>
public static class MetalKit
{
    public static readonly CultureInfo Pt = CultureInfo.GetCultureInfo("pt-BR");

    public static string F(double v, string fmt = "0.0") => v.ToString(fmt, Pt);

    public static double Clamp01(double v) => double.IsNaN(v) ? 0 : Math.Clamp(v, 0, 1);

    public static double Num(IReadOnlyDictionary<string, string> d, string key, double fallback) =>
        d.TryGetValue(key, out var s) && Question.TryNum(s, out var v) ? v : fallback;

    public static string Str(IReadOnlyDictionary<string, string> d, string key, string fallback) =>
        d.TryGetValue(key, out var s) && !string.IsNullOrWhiteSpace(s) ? s.Trim().ToLowerInvariant() : fallback;

    public static SolidColorBrush Brush(string hex, double opacity = 1)
    {
        var c = (Color)ColorConverter.ConvertFromString(hex);
        var b = new SolidColorBrush(c) { Opacity = opacity };
        b.Freeze();
        return b;
    }

    public static SolidColorBrush Brush(Color c)
    {
        var b = new SolidColorBrush(c);
        b.Freeze();
        return b;
    }

    public static Pen Pen(Brush b, double w, bool dashed = false)
    {
        var p = new Pen(b, w);
        if (dashed) p.DashStyle = new DashStyle(new double[] { 4, 3 }, 0);
        p.Freeze();
        return p;
    }

    public static Color Mix(Color a, Color b, double t)
    {
        t = Math.Clamp(t, 0, 1);
        return Color.FromArgb((byte)(a.A + (b.A - a.A) * t), (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t), (byte)(a.B + (b.B - a.B) * t));
    }

    public static FormattedText Text(string s, double size, Brush fg, Visual v, bool bold = false) =>
        new(s, Pt, FlowDirection.LeftToRight,
            new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, bold ? FontWeights.SemiBold : FontWeights.Normal, FontStretches.Normal),
            size, fg, VisualTreeHelper.GetDpi(v).PixelsPerDip);

    /// <summary>A two-column grid of compact rows: label | control | value text.</summary>
    public sealed class Form
    {
        public readonly Grid Root = new();
        int _cell;

        public Form(int columns = 2)
        {
            for (int c = 0; c < columns; c++)
            {
                Root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                if (c < columns - 1) Root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(14) });
            }
            Columns = columns;
        }

        public int Columns { get; }

        public void Add(FrameworkElement row)
        {
            int r = _cell / Columns, c = _cell % Columns;
            while (Root.RowDefinitions.Count <= r) Root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(row, r);
            Grid.SetColumn(row, c * 2);
            row.Margin = new Thickness(0, 2, 0, 2);
            Root.Children.Add(row);
            _cell++;
        }
    }

    static Grid RowGrid(string label, double labelWidth, double valueWidth)
    {
        var g = new Grid { Height = 26 };
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(labelWidth) });
        g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star), MinWidth = 60 });
        if (valueWidth > 0) g.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(valueWidth) });
        var l = new TextBlock { Text = label, FontSize = 12.5, Foreground = Ui.TextDim, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
        g.Children.Add(l);
        return g;
    }

    /// <summary>A labelled slider row. <see cref="Format"/> turns the value into the text on the right.</summary>
    public sealed class SliderRow
    {
        public readonly Grid Row;
        public readonly DragSlider Slider;
        readonly TextBlock _value;
        public Func<double, string> Format;
        public event Action? Changed;

        public SliderRow(string label, double min, double max, double step, double value, Func<double, string> format,
            double labelWidth = 92, double valueWidth = 100)
        {
            Format = format;
            Row = RowGrid(label, labelWidth, valueWidth);
            Slider = new DragSlider(min, max, step, value) { Margin = new Thickness(4, 0, 6, 0) };
            Grid.SetColumn(Slider, 1);
            _value = new TextBlock { FontSize = 12.5, Foreground = Ui.Text, VerticalAlignment = VerticalAlignment.Center, TextTrimming = TextTrimming.CharacterEllipsis };
            Grid.SetColumn(_value, 2);
            Row.Children.Add(Slider);
            Row.Children.Add(_value);
            Slider.ValueChanged += () => { Refresh(); Changed?.Invoke(); };
            Refresh();
        }

        public double Value { get => Slider.Value; set => Slider.Value = value; }
        public bool Enabled { set { Slider.IsEnabled = value; Row.Opacity = value ? 1 : 0.55; } }
        public void Refresh() => _value.Text = Format(Slider.Value);
    }

    /// <summary>A labelled combo box row; each option carries its value in Tag.</summary>
    public sealed class ChoiceRow
    {
        public readonly Grid Row;
        public readonly ComboBox Combo = new() { FontSize = 12.5, Height = 30, Margin = new Thickness(4, 0, 0, 0) };
        public event Action? Changed;

        public ChoiceRow(string label, IEnumerable<(string value, string text)> options, string selected, double labelWidth = 92)
        {
            Row = RowGrid(label, labelWidth, 0);
            Row.Height = 32;
            foreach (var (v, t) in options) Combo.Items.Add(new ComboBoxItem { Content = t, Tag = v });
            Value = selected;
            if (Combo.SelectedIndex < 0 && Combo.Items.Count > 0) Combo.SelectedIndex = 0;
            Grid.SetColumn(Combo, 1);
            Row.Children.Add(Combo);
            Combo.SelectionChanged += (_, _) => Changed?.Invoke();
        }

        public string Value
        {
            get => (Combo.SelectedItem as ComboBoxItem)?.Tag as string ?? "";
            set
            {
                for (int i = 0; i < Combo.Items.Count; i++)
                    if (string.Equals((Combo.Items[i] as ComboBoxItem)?.Tag as string, value, StringComparison.OrdinalIgnoreCase))
                    { Combo.SelectedIndex = i; return; }
            }
        }

        public bool Has(string value) => Combo.Items.Cast<ComboBoxItem>().Any(i => string.Equals(i.Tag as string, value, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>A slider drawn by hand: click anywhere on the track or drag; mouse wheel and arrow keys step it.</summary>
    public sealed class DragSlider : FrameworkElement
    {
        readonly double _min, _max, _step;
        double _value;
        public event Action? ValueChanged;
        static readonly Brush Track = Ui.Bg3;
        static readonly Brush Thumb = Brush("#E6E6EA");

        public DragSlider(double min, double max, double step, double value)
        {
            _min = min; _max = max; _step = step;
            _value = Snap(value);
            Focusable = true;
            Cursor = Cursors.Hand;
            Height = 22;
            FocusVisualStyle = null;
        }

        public double Value
        {
            get => _value;
            set
            {
                var v = Snap(value);
                if (Math.Abs(v - _value) < 1e-12) return;
                _value = v;
                InvalidateVisual();
                ValueChanged?.Invoke();
            }
        }

        double Snap(double v)
        {
            if (double.IsNaN(v)) v = _min;
            v = Math.Clamp(v, _min, _max);
            if (_step > 0) v = _min + Math.Round((v - _min) / _step) * _step;
            return Math.Round(Math.Clamp(v, _min, _max), 6);
        }

        const double Pad = 7;
        double Frac => _max > _min ? (_value - _min) / (_max - _min) : 0;

        protected override void OnRender(DrawingContext dc)
        {
            double w = ActualWidth, h = ActualHeight, y = h / 2;
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, w, h));
            if (w < 2 * Pad) return;
            double x = Pad + (w - 2 * Pad) * Frac;
            dc.DrawRoundedRectangle(Track, null, new Rect(Pad, y - 2, w - 2 * Pad, 4), 2, 2);
            dc.DrawRoundedRectangle(IsEnabled ? Ui.Accent : Ui.TextDim, null, new Rect(Pad, y - 2, Math.Max(0, x - Pad), 4), 2, 2);
            dc.DrawEllipse(Thumb, IsKeyboardFocused ? new Pen(Ui.Accent, 2) : null, new Point(x, y), 6.5, 6.5);
        }

        void SetFromX(double x)
        {
            double w = ActualWidth - 2 * Pad;
            if (w <= 0) return;
            Value = _min + Math.Clamp((x - Pad) / w, 0, 1) * (_max - _min);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            Focus();
            CaptureMouse();
            SetFromX(e.GetPosition(this).X);
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (IsMouseCaptured) SetFromX(e.GetPosition(this).X);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (IsMouseCaptured) ReleaseMouseCapture();
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Value += (e.Delta > 0 ? 1 : -1) * StepOrTiny;
            e.Handled = true;
        }

        double StepOrTiny => _step > 0 ? _step : (_max - _min) / 100;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key is Key.Left or Key.Down) { Value -= StepOrTiny; e.Handled = true; }
            else if (e.Key is Key.Right or Key.Up) { Value += StepOrTiny; e.Handled = true; }
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e) => InvalidateVisual();
        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e) => InvalidateVisual();
    }

    /// <summary>A canvas that draws itself with a callback (redrawn on resize and on <see cref="UIElement.InvalidateVisual"/>).</summary>
    public sealed class Paint : FrameworkElement
    {
        readonly Action<DrawingContext, double, double> _draw;
        public Paint(Action<DrawingContext, double, double> draw) { _draw = draw; ClipToBounds = true; }
        protected override void OnRender(DrawingContext dc) => _draw(dc, ActualWidth, ActualHeight);
    }

    /// <summary>The ▶ button and status line at the top of every metallurgy lab.</summary>
    public static (Grid bar, Button run, TextBlock status) Header(string title, string runText)
    {
        var bar = new Grid { Margin = new Thickness(0, 0, 0, 6) };
        bar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        bar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        var t = new TextBlock { Text = title, FontSize = 13.5, FontWeight = FontWeights.SemiBold, Foreground = Ui.Text, TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center };
        bar.Children.Add(t);
        var run = Ui.Button(runText, Ui.Accent);
        run.Margin = new Thickness(8, 0, 0, 0);
        run.Padding = new Thickness(12, 5, 12, 5);
        Grid.SetColumn(run, 1);
        bar.Children.Add(run);
        var status = new TextBlock { FontSize = 12.5, Foreground = Ui.TextDim, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 4) };
        return (bar, run, status);
    }
}
