using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace EasyCreator;

/// <summary>Sandbox mode: free programming in any language, or a free Arduino bench where you pick the parts and
/// their pins. Both reuse LessonView with a lesson that has no tests; code and parts are kept in Progress.</summary>
public static class Sandbox
{
    const string PartsKey = "sandbox-robotics/components";

    static readonly (string Id, string Title, string Starter)[] Languages =
    {
        ("python", "Python", "nome = input(\"Seu nome: \")\nprint(\"Olá,\", nome)\n"),
        ("javascript", "JavaScript", "const nome = prompt(\"Seu nome: \");\nconsole.log(\"Olá, \" + nome);\n"),
        ("c", "C", "#include <stdio.h>\n\nint main(void) {\n    printf(\"Olá!\\n\");\n    return 0;\n}\n"),
        ("csharp", "C#", "Console.WriteLine(\"Olá!\");\n"),
    };

    const string ArduinoStarter =
        "void setup() {\n  pinMode(13, OUTPUT);\n  Serial.begin(9600);\n}\n\nvoid loop() {\n  digitalWrite(13, HIGH);\n  delay(500);\n" +
        "  digitalWrite(13, LOW);\n  delay(500);\n  Serial.println(millis());\n}\n";

    /// <summary>Pin names each part type uses, in the order they are shown.</summary>
    public static readonly (string Type, string Name, string[] Pins)[] PartTypes =
    {
        ("led", "LED", new[] { "pin" }),
        ("rgbled", "LED RGB", new[] { "r", "g", "b" }),
        ("button", "Botão", new[] { "pin" }),
        ("pot", "Potenciômetro", new[] { "pin" }),
        ("ldr", "LDR (sensor de luz)", new[] { "pin" }),
        ("servo", "Servo motor", new[] { "pin" }),
        ("buzzer", "Buzzer", new[] { "pin" }),
        ("ultrasonic", "Sensor ultrassônico", new[] { "trig", "echo" }),
        ("motor", "Motor DC (ponte H)", new[] { "in1", "in2", "en" }),
        ("lcd", "LCD 16x2", new[] { "rs", "en", "d4", "d5", "d6", "d7" }),
        ("robot", "Robô com rodas", new[] { "in1", "in2", "ena", "in3", "in4", "enb", "trig", "echo", "lineL", "lineR" }),
    };

    static readonly Dictionary<string, int[]> DefaultPins = new()
    {
        ["led"] = new[] { 13 }, ["rgbled"] = new[] { 9, 10, 11 }, ["button"] = new[] { 2 }, ["pot"] = new[] { 14 },
        ["ldr"] = new[] { 15 }, ["servo"] = new[] { 9 }, ["buzzer"] = new[] { 8 }, ["ultrasonic"] = new[] { 10, 11 },
        ["motor"] = new[] { 8, 7, 5 }, ["lcd"] = new[] { 12, 11, 10, 9, 4, 3 },
        ["robot"] = new[] { 8, 7, 5, 4, 2, 6, 10, 11, 14, 15 },
    };

    public static UIElement Page(MainWindow w)
    {
        var p = new StackPanel();
        p.Children.Add(Ui.Label("Programação livre", 18, bold: true));
        p.Children.Add(Ui.Label("Escreva qualquer programa e rode na hora, sem desafio nem correção.", 14, Ui.TextDim));
        var langs = new WrapPanel { Margin = new Thickness(0, 10, 0, 24) };
        foreach (var (id, title, _) in Languages)
        {
            var b = Ui.Button("💻 " + title);
            b.Margin = new Thickness(0, 0, 10, 10);
            b.Click += (_, _) => w.Go(() => Programming(w, id));
            langs.Children.Add(b);
        }
        p.Children.Add(langs);
        p.Children.Add(Ui.Label("Bancada de robótica", 18, bold: true));
        p.Children.Add(Ui.Label("Escolha as peças, ligue nos pinos que quiser e programe o Arduino no simulador.", 14, Ui.TextDim));
        var robo = Ui.Button("🤖 Abrir bancada", Ui.Accent);
        robo.HorizontalAlignment = HorizontalAlignment.Left;
        robo.Margin = new Thickness(0, 10, 0, 0);
        robo.Click += (_, _) => w.Go(() => Robotics(w));
        p.Children.Add(robo);
        return Pages.Framed(w, "🧪 Sandbox", "Teste o que quiser, do seu jeito.", p);
    }

    static UIElement Programming(MainWindow w, string language)
    {
        var (_, title, starter) = Languages.First(l => l.Id == language);
        var track = new Track { Id = "sandbox-" + language, Title = "Sandbox · " + title, Area = "programming", Language = language };
        track.Lessons.Add(new Lesson { Id = "sandbox", Title = $"{title} livre", Starter = starter });
        return new LessonView(w, track, 0, sandbox: true);
    }

    static UIElement Robotics(MainWindow w)
    {
        var track = new Track { Id = "sandbox-robotics", Title = "Sandbox · Robótica", Area = "robotics", Language = "c" };
        track.Lessons.Add(new Lesson { Id = "sandbox", Title = "Bancada livre", Starter = ArduinoStarter, Components = LoadParts() });
        return new LessonView(w, track, 0, sandbox: true);
    }

    internal static void SnapOpenRobotics(MainWindow w) => w.Go(() => Robotics(w));

    static List<ComponentSpec> LoadParts()
    {
        try
        {
            if (Progress.Current.Code.TryGetValue(PartsKey, out var json))
                return JsonSerializer.Deserialize<List<ComponentSpec>>(json) ?? new();
        }
        catch { }
        return new() { new ComponentSpec { Id = "led1", Type = "led", Color = "red", Pins = new() { ["pin"] = 13 } } };
    }

    /// <summary>Left column of a sandbox lesson: help for programming, the parts editor for robotics.</summary>
    public static UIElement Left(MainWindow w, Track track, Lesson lesson)
    {
        var p = new StackPanel { Margin = new Thickness(0, 0, 12, 0) };
        var back = Ui.Button("← Sandbox");
        back.HorizontalAlignment = HorizontalAlignment.Left;
        back.Margin = new Thickness(0, 0, 0, 12);
        back.Click += (_, _) => w.Back();
        p.Children.Add(back);
        p.Children.Add(Ui.Label(track.Title, 24, Ui.Accent, true));
        if (track.Area != "robotics")
        {
            p.Children.Add(Markdown.Render(
                "Escreva o que quiser e clique em **▶ Rodar**.\n\n- O que o programa lê (input, scanf, Console.ReadLine) vem da caixa **Entrada**, uma linha por leitura.\n" +
                "- Em **Executar com** você escolhe rodar dentro do app (offline) ou com o programa instalado no seu computador.\n" +
                "- Seu código fica salvo sozinho."));
            return new ScrollViewer { Content = p, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        }

        p.Children.Add(Markdown.Render(
            "Monte o circuito aqui, depois programe e clique em **▶ Simular**. Os pinos 14 a 19 são as entradas analógicas **A0 a A5**."));
        var parts = lesson.Components.Select(Clone).ToList();
        var list = new StackPanel();
        void Rebuild()
        {
            list.Children.Clear();
            foreach (var part in parts) list.Children.Add(PartEditor(part, () => { parts.Remove(part); Rebuild(); }));
        }
        Rebuild();
        p.Children.Add(list);

        var addRow = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
        var pick = new ComboBox { MinWidth = 200, FontSize = 14, Margin = new Thickness(0, 0, 8, 0) };
        foreach (var t in PartTypes) pick.Items.Add(new ComboBoxItem { Content = t.Name, Tag = t.Type });
        pick.SelectedIndex = 0;
        var add = Ui.Button("＋ Adicionar peça");
        add.Click += (_, _) =>
        {
            var type = (string)((ComboBoxItem)pick.SelectedItem).Tag;
            if (type == "robot" && parts.Any(x => x.Type == "robot")) { MessageBox.Show("A bancada aceita um robô só."); return; }
            int n = 1;
            while (parts.Any(x => x.Id == type + n)) n++;
            var info = PartTypes.First(t => t.Type == type);
            var spec = new ComponentSpec { Id = type + n, Type = type, Color = type == "led" ? "red" : type == "robot" ? "line" : "" };
            for (int i = 0; i < info.Pins.Length; i++) spec.Pins[info.Pins[i]] = DefaultPins[type][i];
            parts.Add(spec);
            Rebuild();
        };
        addRow.Children.Add(pick);
        addRow.Children.Add(add);
        p.Children.Add(addRow);

        var apply = Ui.Button("✔ Aplicar circuito", Ui.Accent);
        apply.HorizontalAlignment = HorizontalAlignment.Left;
        apply.Margin = new Thickness(0, 14, 0, 0);
        apply.Click += (_, _) =>
        {
            Progress.Current.Code[PartsKey] = JsonSerializer.Serialize(parts);
            Progress.Current.Save();
            w.Back();
            w.Go(() => Robotics(w));
        };
        p.Children.Add(apply);
        p.Children.Add(Ui.Label("Funções: pinMode, digitalWrite, digitalRead, analogRead, analogWrite, delay, millis, Serial, " +
                                "Servo, tone/noTone, pulseIn, LiquidCrystal, map, constrain.", 12, Ui.TextDim));
        return new ScrollViewer { Content = p, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    }

    static ComponentSpec Clone(ComponentSpec c) => new() { Id = c.Id, Type = c.Type, Color = c.Color, Pins = new(c.Pins) };

    static UIElement PartEditor(ComponentSpec part, Action remove)
    {
        var info = PartTypes.FirstOrDefault(t => t.Type == part.Type);
        var body = new StackPanel();
        var head = new DockPanel();
        var x = Ui.Button("✕");
        x.Padding = new Thickness(8, 2, 8, 2);
        x.Margin = new Thickness(0);
        x.Click += (_, _) => remove();
        DockPanel.SetDock(x, Dock.Right);
        head.Children.Add(x);
        head.Children.Add(Ui.Label($"{info.Name ?? part.Type} · {part.Id}", 14, bold: true));
        body.Children.Add(head);

        var pins = new WrapPanel { Margin = new Thickness(0, 6, 0, 0) };
        foreach (var pinName in info.Pins ?? part.Pins.Keys.ToArray())
        {
            var cell = new StackPanel { Margin = new Thickness(0, 0, 10, 6) };
            cell.Children.Add(Ui.Label(pinName, 11, Ui.TextDim));
            var combo = new ComboBox { Width = 70, FontSize = 13 };
            for (int n = 0; n <= 19; n++) combo.Items.Add(new ComboBoxItem { Content = n < 14 ? $"D{n}" : $"A{n - 14}", Tag = n });
            combo.SelectedIndex = part.Pins.TryGetValue(pinName, out var v) ? Math.Clamp(v, 0, 19) : 0;
            string key = pinName;
            combo.SelectionChanged += (_, _) => part.Pins[key] = (int)((ComboBoxItem)combo.SelectedItem).Tag;
            cell.Children.Add(combo);
            pins.Children.Add(cell);
        }
        body.Children.Add(pins);

        string[]? choices = part.Type == "led" ? new[] { "red", "green", "yellow", "blue", "white" }
            : part.Type == "robot" ? new[] { "line", "loop", "open", "obstacles" } : null;
        if (choices != null)
        {
            var names = part.Type == "led"
                ? new[] { "vermelho", "verde", "amarelo", "azul", "branco" }
                : new[] { "pista com linha", "linha em circuito", "sala vazia", "sala com obstáculos" };
            var combo = new ComboBox { Width = 200, FontSize = 13, HorizontalAlignment = HorizontalAlignment.Left };
            for (int i = 0; i < choices.Length; i++) combo.Items.Add(new ComboBoxItem { Content = names[i], Tag = choices[i] });
            combo.SelectedIndex = Math.Max(0, Array.IndexOf(choices, part.Color));
            combo.SelectionChanged += (_, _) => part.Color = (string)((ComboBoxItem)combo.SelectedItem).Tag;
            body.Children.Add(Ui.Label(part.Type == "led" ? "cor" : "arena", 11, Ui.TextDim));
            body.Children.Add(combo);
        }
        var card = Ui.Card(body, Ui.Bg1, 10);
        card.Margin = new Thickness(0, 0, 0, 8);
        return card;
    }
}


