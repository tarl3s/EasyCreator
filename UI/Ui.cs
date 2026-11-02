using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EasyCreator;

/// <summary>Palette and small factory helpers shared by every page (dark theme).</summary>
public static class Ui
{
    // Same dark palette and green accent as EasyEngine (engine/Theme.cs) so the family looks alike.
    public static readonly Brush Bg0 = Make("#141418"), Bg1 = Make("#1C1C22"), Bg2 = Make("#24242B"), Bg3 = Make("#2E2E37"),
        Border = Make("#35353F"), Text = Make("#E6E6EA"), TextDim = Make("#A0A0AA"),
        Accent = Make("#18B98A"), AccentHover = Make("#1ED29B"),
        Prog = Accent, Robo = Accent, Good = Accent, Bad = Make("#FF5C6C"),
        Code = Make("#0F0F13"), OnAccent = Make("#062018");

    public static readonly FontFamily Mono = new("Cascadia Mono, Consolas");

    public static SolidColorBrush Make(string hex)
    {
        var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        b.Freeze();
        return b;
    }

    public static Brush AreaBrush(string area) => area == "robotics" ? Robo : Prog;

    public static TextBlock Label(string text, double size = 14, Brush? fg = null, bool bold = false) => new()
    {
        Text = text, FontSize = size, Foreground = fg ?? Text, TextWrapping = TextWrapping.Wrap,
        FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal,
    };

    public static Button Button(string text, Brush? bg = null, Brush? fg = null)
    {
        var b = new Button
        {
            Content = text, Padding = new Thickness(14, 7, 14, 7), Margin = new Thickness(0, 0, 8, 0),
            Background = bg ?? Bg3, Foreground = fg ?? (bg == null ? Text : OnAccent), BorderThickness = new Thickness(0),
            FontSize = 14, Cursor = System.Windows.Input.Cursors.Hand,
        };
        b.Template = RoundTemplate();
        return b;
    }

    static ControlTemplate? _round;
    static ControlTemplate RoundTemplate()
    {
        if (_round != null) return _round;
        var border = new FrameworkElementFactory(typeof(System.Windows.Controls.Border));
        border.SetValue(System.Windows.Controls.Border.CornerRadiusProperty, new CornerRadius(6));
        border.SetBinding(System.Windows.Controls.Border.BackgroundProperty,
            new System.Windows.Data.Binding("Background") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        border.SetBinding(System.Windows.Controls.Border.PaddingProperty,
            new System.Windows.Data.Binding("Padding") { RelativeSource = System.Windows.Data.RelativeSource.TemplatedParent });
        var cp = new FrameworkElementFactory(typeof(ContentPresenter));
        cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
        cp.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
        border.AppendChild(cp);
        _round = new ControlTemplate(typeof(Button)) { VisualTree = border };
        var hover = new Trigger { Property = UIElement.IsMouseOverProperty, Value = true };
        hover.Setters.Add(new Setter(UIElement.OpacityProperty, 0.85));
        var dim = new Trigger { Property = UIElement.IsEnabledProperty, Value = false };
        dim.Setters.Add(new Setter(UIElement.OpacityProperty, 0.45));
        _round.Triggers.Add(hover);
        _round.Triggers.Add(dim);
        return _round;
    }

    public static Border Card(UIElement child, Brush? bg = null, double pad = 16) => new()
    {
        Child = child, Background = bg ?? Bg1, CornerRadius = new CornerRadius(10), Padding = new Thickness(pad),
        BorderBrush = Border, BorderThickness = new Thickness(1),
    };

    public static TextBox CodeBox(bool readOnly = false) => new()
    {
        FontFamily = Mono, FontSize = 14, Background = Code, Foreground = Text, CaretBrush = Text,
        BorderBrush = Border, AcceptsReturn = true, AcceptsTab = false, IsReadOnly = readOnly,
        TextWrapping = readOnly ? TextWrapping.Wrap : TextWrapping.NoWrap, Padding = new Thickness(8),
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
    };
}
