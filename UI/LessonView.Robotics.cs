using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using EasyCreator.Arduino;

namespace EasyCreator;

/// <summary>Robotics lessons: live Arduino simulation (components you can press/turn, robot arena) and grading.</summary>
public sealed partial class LessonView
{
    ArduinoSession? _session;
    readonly WrapPanel _parts = new();
    readonly Canvas _arena = new() { ClipToBounds = true, Background = Ui.Code };
    readonly TextBlock _simStatus = Ui.Label(L.T("Parado"), 13, Ui.TextDim);
    readonly Dictionary<string, Action<ComponentSnapshot>> _painters = new();
    Button? _startBtn;
    DateTime _lastFrame;
    bool HasRobot => _lesson.Components.Any(c => c.Type == "robot");

    void BuildRoboticsActions()
    {
        _startBtn = Ui.Button(L.T("▶ Simular"));
        _startBtn.Click += (_, _) => { if (_session?.IsRunning == true) StopRobotics(); else StartSim(); };
        _checkBtn = Ui.Button(L.T("✔ Corrigir"), Ui.Accent);
        _checkBtn.Click += async (_, _) => await CheckRobotics();
        var reset = Ui.Button(L.T("↺ Recomeçar"));
        reset.Click += (_, _) =>
        {
            if (MessageBox.Show(L.T("Voltar ao código inicial da lição? O que você escreveu será perdido."), L.T("Recomeçar"),
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes) _code.Text = _lesson.Starter;
        };
        var speed = new ComboBox { FontSize = 14, Margin = new Thickness(8, 0, 8, 0) };
        foreach (var s in new[] { 0.25, 0.5, 1.0, 2.0, 4.0 }) speed.Items.Add(new ComboBoxItem { Content = $"{s}x", Tag = s });
        speed.SelectedIndex = 2;
        speed.SelectionChanged += (_, _) => { if (_session != null) _session.SpeedFactor = SpeedOf(speed); };
        speed.Tag = "speed";
        _simStatus.VerticalAlignment = VerticalAlignment.Center;
        _actions.Children.Add(_startBtn);
        if (!_sandbox) _actions.Children.Add(_checkBtn);
        _actions.Children.Add(reset);
        _actions.Children.Add(speed);
        _actions.Children.Add(_simStatus);

        var sim = new Grid { Height = HasRobot ? 300 : 190, Margin = new Thickness(0, 10, 0, 0) };
        if (HasRobot)
        {
            sim.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            sim.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            var arenaCard = Ui.Card(_arena, Ui.Code, 0);
            arenaCard.Margin = new Thickness(0, 0, 8, 0);
            sim.Children.Add(arenaCard);
            _arena.SizeChanged += (_, _) => DrawArena();
        }
        var partsScroll = new ScrollViewer { Content = _parts, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        Grid.SetColumn(partsScroll, HasRobot ? 1 : 0);
        sim.Children.Add(partsScroll);
        Grid.SetRow(sim, 2);
        _right.Children.Add(sim);
        BuildParts();
        DrawArena();
    }

    static double SpeedOf(ComboBox c) => (c.SelectedItem as ComboBoxItem)?.Tag is double d ? d : 1;

    // ---------- components ----------

    void BuildParts()
    {
        _parts.Children.Clear();
        _painters.Clear();
        foreach (var spec in _lesson.Components.Where(c => c.Type != "robot"))
        {
            var body = new StackPanel { Width = 150 };
            var pins = string.Join(" ", spec.Pins.Select(p => $"{p.Key}:{p.Value}"));
            body.Children.Add(Ui.Label(spec.Id, 13, bold: true));
            body.Children.Add(Ui.Label($"{L.T(TypeName(spec.Type))} · {pins}", 11, Ui.TextDim));
            var visual = new Grid { Height = 64, Margin = new Thickness(0, 6, 0, 0) };
            body.Children.Add(visual);
            var state = Ui.Label("", 12, Ui.TextDim);
            body.Children.Add(state);
            _painters[spec.Id] = MakePart(spec, visual, body);
            var paint = _painters[spec.Id];
            _painters[spec.Id] = s => { paint(s); state.Text = s.Text; };
            var card = Ui.Card(body, Ui.Bg1, 10);
            card.Margin = new Thickness(0, 0, 8, 8);
            _parts.Children.Add(card);
        }
    }

    static string TypeName(string t) => t switch
    {
        "led" => "LED", "rgbled" => "LED RGB", "button" => "Botão", "pot" => "Potenciômetro", "servo" => "Servo",
        "buzzer" => "Buzzer", "ultrasonic" => "Ultrassom", "ldr" => "LDR (luz)", "motor" => "Motor DC", "lcd" => "LCD",
        _ => t,
    };

    Action<ComponentSnapshot> MakePart(ComponentSpec spec, Grid visual, StackPanel body)
    {
        switch (spec.Type)
        {
            case "led":
            case "rgbled":
            {
                var bulb = new Ellipse { Width = 44, Height = 44, Stroke = Ui.Border, StrokeThickness = 2, Fill = Ui.Bg3 };
                visual.Children.Add(bulb);
                var baseColor = LedColor(spec.Color);
                return s =>
                {
                    Color c = spec.Type == "rgbled" ? Color.FromRgb((byte)s.R, (byte)s.G, (byte)s.B)
                        : Scale(baseColor, s.Brightness / 255.0);
                    bulb.Fill = s.On ? new SolidColorBrush(c) : Ui.Bg3;
                    bulb.Effect = s.On ? new System.Windows.Media.Effects.DropShadowEffect { Color = c, BlurRadius = 24, ShadowDepth = 0, Opacity = 0.9 } : null;
                };
            }
            case "button":
            {
                var b = Ui.Button(L.T("Segure"));
                b.Margin = new Thickness(0);
                b.PreviewMouseLeftButtonDown += (_, _) => _session?.PressButton(spec.Id, true);
                b.PreviewMouseLeftButtonUp += (_, _) => _session?.PressButton(spec.Id, false);
                b.MouseLeave += (_, _) => _session?.PressButton(spec.Id, false);
                visual.Children.Add(b);
                return s => b.Background = s.Pressed ? Ui.Accent : Ui.Bg3;
            }
            case "pot":
            case "ldr":
            case "ultrasonic":
            {
                bool dist = spec.Type == "ultrasonic";
                var slider = new Slider { Minimum = dist ? 2 : 0, Maximum = dist ? 300 : 1023, Value = dist ? 100 : 512, VerticalAlignment = VerticalAlignment.Center };
                slider.ValueChanged += (_, e) =>
                {
                    if (dist) _session?.SetDistance(spec.Id, e.NewValue);
                    else _session?.SetAnalog(spec.Id, (int)e.NewValue);
                };
                visual.Children.Add(slider);
                return _ => { };
            }
            case "servo":
            {
                var canvas = new Canvas { Width = 120, Height = 64 };
                canvas.Children.Add(new Rectangle { Width = 40, Height = 20, Fill = Ui.Bg3, RadiusX = 3, RadiusY = 3, Margin = new Thickness(40, 44, 0, 0) });
                var arm = new Line { X1 = 60, Y1 = 54, Stroke = Ui.Accent, StrokeThickness = 5, StrokeStartLineCap = PenLineCap.Round, StrokeEndLineCap = PenLineCap.Round };
                canvas.Children.Add(arm);
                visual.Children.Add(canvas);
                return s =>
                {
                    double a = Math.PI * s.Position / 180.0;
                    arm.X2 = 60 + Math.Cos(Math.PI - a) * 46;
                    arm.Y2 = 54 - Math.Sin(Math.PI - a) * 46;
                };
            }
            case "motor":
            {
                var wheel = new Grid { Width = 54, Height = 54 };
                wheel.Children.Add(new Ellipse { Stroke = Ui.Border, StrokeThickness = 3, Fill = Ui.Bg3 });
                var spoke = new Rectangle { Width = 4, Height = 46, Fill = Ui.Accent };
                var rot = new RotateTransform();
                spoke.RenderTransform = rot;
                spoke.RenderTransformOrigin = new Point(0.5, 0.5);
                wheel.Children.Add(spoke);
                visual.Children.Add(wheel);
                return s => rot.Angle += s.Speed * 0.05;
            }
            case "lcd":
            {
                var screen = new TextBlock { FontFamily = Ui.Mono, FontSize = 13, Foreground = Ui.Make("#0B2A10"), Padding = new Thickness(6) };
                visual.Children.Add(new Border { Background = Ui.Make("#7FD15A"), CornerRadius = new CornerRadius(4), Child = screen });
                body.Width = 190;
                return s => screen.Text = string.Join("\n", s.LcdLines);
            }
            case "buzzer":
            {
                var icon = Ui.Label("🔈", 32);
                icon.HorizontalAlignment = HorizontalAlignment.Center;
                visual.Children.Add(icon);
                return s => icon.Text = s.Frequency > 0 ? "🔊" : "🔈";
            }
            default:
                return _ => { };
        }
    }

    static Color LedColor(string name) => name.ToLowerInvariant() switch
    {
        "green" or "verde" => Color.FromRgb(60, 230, 110),
        "yellow" or "amarelo" => Color.FromRgb(255, 215, 60),
        "blue" or "azul" => Color.FromRgb(70, 140, 255),
        "white" or "branco" => Colors.White,
        _ => Color.FromRgb(255, 60, 60),
    };

    static Color Scale(Color c, double k)
    {
        k = 0.35 + 0.65 * Math.Clamp(k, 0, 1);
        return Color.FromRgb((byte)(c.R * k), (byte)(c.G * k), (byte)(c.B * k));
    }

    // ---------- arena ----------

    void DrawArena(RobotSnapshot? robot = null)
    {
        if (!HasRobot || _arena.ActualWidth < 10) return;
        var arena = _session?.Arena ?? Arena.Create(_lesson.Components.First(c => c.Type == "robot").Color);
        double k = Math.Min(_arena.ActualWidth / arena.Width, _arena.ActualHeight / arena.Height);
        Point P(Vec2 v) => new(v.X * k, v.Y * k);
        _arena.Children.Clear();
        if (arena.Line.Count > 1)
            _arena.Children.Add(new Polyline
            {
                Points = new PointCollection(arena.Line.Select(P)), Stroke = Ui.Make("#E6E6EA"),
                StrokeThickness = arena.LineWidth * k, StrokeLineJoin = PenLineJoin.Round,
            });
        foreach (var w in arena.Walls)
            _arena.Children.Add(new Line { X1 = w.A.X * k, Y1 = w.A.Y * k, X2 = w.B.X * k, Y2 = w.B.Y * k, Stroke = Ui.TextDim, StrokeThickness = 3 });
        foreach (var b in arena.Obstacles)
        {
            var r = new Rectangle { Width = b.W * k, Height = b.H * k, Fill = Ui.Bad, RadiusX = 2, RadiusY = 2 };
            Canvas.SetLeft(r, b.X * k); Canvas.SetTop(r, b.Y * k);
            _arena.Children.Add(r);
        }
        if (arena.Goal is { } g)
        {
            var e = new Ellipse { Width = arena.GoalRadius * 2 * k, Height = arena.GoalRadius * 2 * k, Stroke = Ui.Accent, StrokeThickness = 2, StrokeDashArray = new DoubleCollection { 3, 2 } };
            Canvas.SetLeft(e, (g.X - arena.GoalRadius) * k); Canvas.SetTop(e, (g.Y - arena.GoalRadius) * k);
            _arena.Children.Add(e);
        }
        robot ??= _session?.GetRobot();
        var pos = robot != null ? new Vec2(robot.X, robot.Y) : arena.Start;
        double angle = robot?.AngleDeg ?? arena.StartAngleDeg, radius = robot?.Radius ?? 8;
        if (robot?.Trail.Count > 1)
            _arena.Children.Add(new Polyline { Points = new PointCollection(robot.Trail.Select(P)), Stroke = Ui.Accent, StrokeThickness = 1.5, Opacity = 0.5 });
        var body = new Ellipse { Width = radius * 2 * k, Height = radius * 2 * k, Fill = Ui.Bg3, Stroke = Ui.Accent, StrokeThickness = 2 };
        Canvas.SetLeft(body, (pos.X - radius) * k); Canvas.SetTop(body, (pos.Y - radius) * k);
        _arena.Children.Add(body);
        double a = angle * Math.PI / 180;
        _arena.Children.Add(new Line
        {
            X1 = pos.X * k, Y1 = pos.Y * k, X2 = (pos.X + Math.Cos(a) * radius * 1.4) * k, Y2 = (pos.Y + Math.Sin(a) * radius * 1.4) * k,
            Stroke = Ui.Accent, StrokeThickness = 3,
        });
    }

    // ---------- run / grade ----------

    void StartSim()
    {
        StopRobotics();
        _session = new ArduinoSession();
        var robot = _lesson.Components.FirstOrDefault(c => c.Type == "robot");
        if (!_session.Start(_code.Text, _lesson.Components, robot?.Color))
        {
            _output.Foreground = Ui.Bad;
            _output.Text = _session.Error ?? L.T("Erro ao compilar.");
            _session = null;
            return;
        }
        if (_actions.Children.OfType<ComboBox>().FirstOrDefault() is { } speed) _session.SpeedFactor = SpeedOf(speed);
        _output.Foreground = Ui.Text;
        _startBtn!.Content = L.T("■ Parar");
        _lastFrame = DateTime.Now;
        CompositionTarget.Rendering += Frame;
    }

    void Frame(object? sender, EventArgs e)
    {
        var s = _session;
        if (s == null) return;
        var now = DateTime.Now;
        s.Tick(Math.Min(100, (now - _lastFrame).TotalMilliseconds));
        _lastFrame = now;
        foreach (var c in s.GetComponents())
            if (_painters.TryGetValue(c.Id, out var paint)) paint(c);
        if (HasRobot) DrawArena(s.GetRobot());
        var serial = s.SerialText;
        if (s.Error != null) serial += (serial.Length > 0 ? "\n" : "") + s.Error;
        else if (s.Warnings.Count > 0) serial += "\n⚠ " + string.Join("\n⚠ ", s.Warnings);
        if (_output.Text != serial) { _output.Text = serial; _output.ScrollToEnd(); }
        _output.Foreground = s.Error != null ? Ui.Bad : Ui.Text;
        _simStatus.Text = s.CurrentLine > 0 ? L.F("t = {0:0.0} s · linha {1}", s.TimeMs / 1000, s.CurrentLine) : $"t = {s.TimeMs / 1000:0.0} s";
        if (!s.IsRunning || s.HasFinished) StopRobotics(keepOutput: true);
    }

    void StopRobotics() => StopRobotics(false);

    void StopRobotics(bool keepOutput)
    {
        CompositionTarget.Rendering -= Frame;
        _session?.Dispose();
        _session = null;
        if (_startBtn != null) _startBtn.Content = L.T("▶ Simular");
        if (!keepOutput) _simStatus.Text = L.T("Parado");
    }

    async Task CheckRobotics()
    {
        StopRobotics();
        _checkBtn!.IsEnabled = false;
        _results.Children.Clear();
        _results.Children.Add(Ui.Label(L.T("Simulando e corrigindo…"), 13, Ui.TextDim));
        _cts = new CancellationTokenSource();
        try
        {
            var outcomes = await new ArduinoSimulator().GradeAsync(_lesson, _code.Text, _cts.Token);
            _results.Children.Clear();
            bool all = outcomes.Count > 0 && outcomes.All(o => o.Passed);
            _results.Children.Add(Ui.Label(all ? L.T("🎉 Tudo certo!") : L.F("{0} de {1} verificações passaram", outcomes.Count(o => o.Passed), outcomes.Count),
                16, all ? Ui.Good : Ui.Bad, true));
            foreach (var o in outcomes)
            {
                var p = new StackPanel { Margin = new Thickness(0, 6, 0, 0) };
                p.Children.Add(Ui.Label($"{(o.Passed ? "✓" : "✗")} {o.Description}", 13, o.Passed ? Ui.Good : Ui.Bad, true));
                if (!o.Passed && o.Detail.Length > 0) p.Children.Add(Ui.Label(o.Detail, 12, Ui.TextDim));
                _results.Children.Add(p);
            }
            if (all) Finished();
        }
        finally { _checkBtn.IsEnabled = true; }
    }
}
