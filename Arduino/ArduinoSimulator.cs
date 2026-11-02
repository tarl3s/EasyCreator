using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using EasyCreator.CInterp;

namespace EasyCreator.Arduino;

/// <summary>
/// Headless grader for robotics lessons. Each group of checks that shares the same stimuli gets its own fresh
/// simulation: setup() runs once, then loop() forever on a virtual clock until the last check's time.
/// Check kinds and values are documented in <see cref="SimCheck"/>; extra syntax accepted in <c>value</c>:
/// comparisons (">=100", "&lt;5", "!=0"), ranges ("80..120"), words (HIGH/LOW, on/off, true/false, pressed/released,
/// colour names for RGB LEDs). Component targets may name a property: "motor1.dir", "lcd1.line0", "rgb1.r", "servo1.pos".
/// Robot targets: x, y, angle, goal, collisions, distance (travelled), lineRatio, sonar, speed, stopped.
/// Serial values: text the output must contain; "!text" = must NOT contain; "re:pattern" = regex.
/// Stimuli: "button:id=pressed@ms", "pot:id=512@ms", "ldr:id=200@ms", "ultrasonic:id=15@ms",
/// "obstacle:robot=60@ms" (box on the line 60 cm from its start), "serial:=text@ms", "arena:robot=obstacles".
/// </summary>
public sealed class ArduinoSimulator : IArduinoSimulator
{
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>Wall-clock limit for one simulation group.</summary>
    public TimeSpan WallLimit { get; set; } = TimeSpan.FromSeconds(20);

    public Task<List<CheckOutcome>> GradeAsync(Lesson lesson, string sketch, CancellationToken ct = default)
    {
        var tcs = new TaskCompletionSource<List<CheckOutcome>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var th = new Thread(() =>
        {
            try { tcs.TrySetResult(Grade(lesson.Components, lesson.Checks, sketch, ct)); }
            catch (Exception e) { tcs.TrySetResult(lesson.Checks.Select(c => new CheckOutcome(false, Describe(c), "Erro interno do simulador: " + e.Message)).ToList()); }
        }, 256 * 1024 * 1024) { IsBackground = true, Name = "Arduino grader" };
        th.Start();
        return tcs.Task;
    }

    /// <summary>Synchronous grading (run on a big-stack thread; <see cref="GradeAsync"/> does).</summary>
    public List<CheckOutcome> Grade(IList<ComponentSpec> components, IList<SimCheck> checks, string sketch, CancellationToken ct = default)
    {
        var results = new CheckOutcome?[checks.Count];
        if (checks.Count == 0) return new List<CheckOutcome>();

        // compile once up front to report syntax errors clearly
        {
            var probe = new CInterpreter();
            new ArduinoBoard(components).Attach(probe);
            string? err = null;
            try
            {
                probe.Compile(sketch);
                if (!probe.HasFunction("setup") || !probe.HasFunction("loop"))
                    err = "O sketch precisa ter as duas funções: void setup() { } e void loop() { }.";
            }
            catch (CError e) { err = e.Friendly; }
            catch (Exception e) { err = "Erro ao ler o código: " + e.Message; }
            if (err != null) return checks.Select(c => new CheckOutcome(false, Describe(c), err)).ToList();
        }

        var groups = Enumerable.Range(0, checks.Count)
            .GroupBy(i => string.Join("|", checks[i].Stimulus ?? new List<string>()))
            .ToList();
        foreach (var g in groups)
        {
            var idx = g.ToList();
            RunGroup(components, checks, idx, sketch, results, ct);
        }
        return results.Select((r, i) => r ?? new CheckOutcome(false, Describe(checks[i]), "A verificação não foi avaliada.")).ToList();
    }

    void RunGroup(IList<ComponentSpec> components, IList<SimCheck> checks, List<int> idx, string sketch, CheckOutcome?[] results, CancellationToken ct)
    {
        var stimuli = checks[idx[0]].Stimulus ?? new List<string>();
        string? arena = null;
        foreach (var s in stimuli)
            if (ArduinoBoard.TryParseStimulus(s, out var k, out _, out var v, out _) && k.Equals("arena", StringComparison.OrdinalIgnoreCase)) arena = v;

        var board = new ArduinoBoard(components, arena);
        var ip = new CInterpreter { Cancel = ct, Timeout = WallLimit, MaxSteps = long.MaxValue };
        board.Attach(ip);
        try { ip.Compile(sketch); }
        catch (CError e) { foreach (var i in idx) results[i] = new CheckOutcome(false, Describe(checks[i]), e.Friendly); return; }

        string? stimErr = null;
        foreach (var s in stimuli)
        {
            if (!ArduinoBoard.TryParseStimulus(s, out var kind, out var id, out var value, out var at)) { stimErr = "estímulo mal escrito: " + s; continue; }
            board.Schedule(at * 1000, () => { var e = board.ApplyStimulus(kind, id, value); if (e != null) stimErr = e; });
        }
        double end = 0;
        foreach (var i in idx)
        {
            var c = checks[i];
            double t = Math.Max(0, c.AtMs) * 1000.0;
            end = Math.Max(end, t);
            int ci = i;
            board.Schedule(t, () => results[ci] = Evaluate(board, c));
        }
        end += 1;
        board.AfterAdvance = () => { if (board.TimeUs >= end) throw new CHalt("fim"); };

        string? runError = null;
        var sw = Stopwatch.StartNew();
        try
        {
            ip.Call("setup");
            while (true)
            {
                ip.Call("loop");
                board.Advance(board.LoopOverheadUs);
            }
        }
        catch (CHalt) { }
        catch (CExit)
        {
            // exit() on an Arduino just stops the program; time keeps passing
            try { board.Advance(end - board.TimeUs + 1); } catch (CHalt) { }
        }
        catch (CError e)
        {
            runError = e.Message.Contains("demorou demais")
                ? "A simulação demorou demais para chegar aos " + (end / 1000).ToString("0", Inv) + " ms (o código é pesado demais ou travou)."
                : e.Friendly;
        }
        catch (InsufficientExecutionStackException) { runError = "Erro de execução: recursão profunda demais."; }
        catch (Exception e) { runError = "Erro interno do simulador perto da linha " + ip.Line + ": " + e.Message; }

        foreach (var i in idx)
        {
            if (results[i] != null && runError == null) continue;
            if (results[i] != null && runError != null) continue; // evaluated before the error happened
            results[i] = new CheckOutcome(false, Describe(checks[i]), runError ?? "A simulação terminou antes do tempo da verificação.");
        }
        if (stimErr != null)
            foreach (var i in idx)
                if (results[i]!.Passed) results[i] = new CheckOutcome(false, Describe(checks[i]), "Problema no estímulo da lição: " + stimErr);
    }

    // ======================= evaluation =======================
    public static CheckOutcome Evaluate(ArduinoBoard board, SimCheck c)
    {
        string desc = Describe(c);
        string when = " (aos " + c.AtMs.ToString(Inv) + " ms)";
        switch ((c.Kind ?? "").ToLowerInvariant())
        {
            case "pin":
                {
                    int pin = ParsePin(c.Target);
                    if (pin < 0) return new CheckOutcome(false, desc, "Pino inválido na lição: " + c.Target);
                    var ps = board.Pins[pin];
                    int v = ps.Mode == PinMode.Output ? ps.Value : board.DigitalLevel(pin) * 255;
                    string text = v == 0 ? "LOW" : v == 255 ? "HIGH" : "PWM " + v;
                    if (ps.Mode != PinMode.Output && ps.Value > 0) text += " (sem pinMode OUTPUT)";
                    bool ok = Match(c.Value, text, v, c.Tolerance, false, out _);
                    return new CheckOutcome(ok, desc, ok ? "OK: pino " + ArduinoBoard.PinName(pin) + " = " + text + when
                        : "Esperado " + c.Value + ", mas o pino " + ArduinoBoard.PinName(pin) + " estava " + text + when + ".");
                }
            case "serial":
                {
                    string all = board.SerialOut.ToString().Replace("\r\n", "\n");
                    string exp = c.Value ?? "";
                    bool ok;
                    if (exp.StartsWith("re:")) { try { ok = Regex.IsMatch(all, exp[3..], RegexOptions.None, TimeSpan.FromSeconds(1)); } catch { ok = false; } }
                    else if (exp.StartsWith("!")) ok = !all.Contains(exp[1..].Replace("\\n", "\n"), StringComparison.Ordinal);
                    else ok = all.Contains(exp.Replace("\\n", "\n"), StringComparison.Ordinal);
                    string tail = all.Length > 160 ? "…" + all[^160..] : all;
                    return new CheckOutcome(ok, desc, ok ? "OK" + when
                        : (all.Length == 0 ? "O Monitor Serial está vazio" + when + (board.SerialBegun ? "." : " — faltou Serial.begin(9600)?")
                                           : "O Monitor Serial mostrou: \"" + tail.Replace("\n", "⏎") + "\"" + when + "."));
                }
            case "component":
                {
                    string id = c.Target, prop = "";
                    int dot = c.Target.IndexOf('.');
                    if (dot > 0) { id = c.Target[..dot]; prop = c.Target[(dot + 1)..].ToLowerInvariant(); }
                    var comp = board.FindComp(id);
                    if (comp == null) return new CheckOutcome(false, desc, "Componente '" + id + "' não existe no circuito da lição.");
                    if (comp.Type == "robot") return EvalRobot(board, c, prop.Length > 0 ? prop : "goal", desc, when);
                    var (text, num, contains) = ComponentValue(board, comp, prop);
                    bool ok = Match(c.Value, text, num, c.Tolerance, contains, out _, comp.Type == "rgbled" && prop.Length == 0);
                    return new CheckOutcome(ok, desc, ok ? "OK: " + id + " = " + Show(text) + when
                        : "Esperado " + c.Value + ", mas " + id + " estava " + Show(text) + when + ".");
                }
            case "robot":
                return EvalRobot(board, c, c.Target.ToLowerInvariant(), desc, when);
            default:
                return new CheckOutcome(false, desc, "Tipo de verificação desconhecido: " + c.Kind);
        }
    }

    static string Show(string s) => "\"" + s.Replace("\n", " | ").TrimEnd() + "\"";

    static CheckOutcome EvalRobot(ArduinoBoard board, SimCheck c, string target, string desc, string when)
    {
        var r = board.Robot;
        if (r == null) return new CheckOutcome(false, desc, "Não há robô no circuito da lição.");
        string text;
        double? num;
        switch (target)
        {
            case "x": num = r.X; text = r.X.ToString("0.0", Inv); break;
            case "y": num = r.Y; text = r.Y.ToString("0.0", Inv); break;
            case "angle": num = r.AngleDeg; text = r.AngleDeg.ToString("0", Inv) + "°"; break;
            case "goal": num = r.ReachedGoal ? 1 : 0; text = r.ReachedGoal ? "true" : "false"; break;
            case "collisions": num = r.Collisions; text = r.Collisions.ToString(Inv); break;
            case "distance": case "travelled": num = r.Travelled; text = r.Travelled.ToString("0", Inv) + " cm"; break;
            case "lineratio": num = r.LineRatio; text = (r.LineRatio * 100).ToString("0", Inv) + "% do tempo na linha"; break;
            case "sonar": num = r.Sonar(); text = num.Value.ToString("0", Inv) + " cm"; break;
            case "speed": num = Math.Abs(r.Speed); text = Math.Abs(r.Speed).ToString("0.0", Inv) + " cm/s"; break;
            case "stopped": num = Math.Abs(r.Speed) < 1 ? 1 : 0; text = Math.Abs(r.Speed) < 1 ? "true" : "false"; break;
            default: return new CheckOutcome(false, desc, "Alvo de robô desconhecido: " + target);
        }
        bool ok = Match(c.Value, text, num, c.Tolerance, false, out _);
        string label = target switch
        {
            "goal" => r.ReachedGoal ? "o robô chegou ao objetivo" : "o robô não chegou ao objetivo",
            "collisions" => "o robô bateu " + r.Collisions + " vez(es)",
            "stopped" => Math.Abs(r.Speed) < 1 ? "o robô estava parado" : "o robô estava andando",
            "lineratio" => "o robô ficou sobre a linha " + text,
            "distance" or "travelled" => "o robô andou " + text,
            "sonar" => "o ultrassom media " + text,
            "speed" => "a velocidade era " + text,
            _ => target + " = " + text
        };
        return new CheckOutcome(ok, desc, (ok ? "OK: " : "Esperado " + c.Value + ", mas ") + label + when + ".");
    }

    internal static (string text, double? num, bool contains) ComponentValue(ArduinoBoard b, Comp c, string prop)
    {
        var s = b.Snapshot(c);
        switch (c.Type)
        {
            case "led":
                if (prop is "brightness" or "value") return (s.Brightness.ToString(Inv), s.Brightness, false);
                return (s.On ? "on" : "off", s.Brightness, false);
            case "rgbled":
                switch (prop)
                {
                    case "r": return (s.R.ToString(Inv), s.R, false);
                    case "g": return (s.G.ToString(Inv), s.G, false);
                    case "b": return (s.B.ToString(Inv), s.B, false);
                }
                return (s.R + "," + s.G + "," + s.B, null, false);
            case "button": return (s.Pressed ? "pressed" : "released", s.Pressed ? 1 : 0, false);
            case "pot":
            case "ldr": return (s.AnalogValue.ToString(Inv), s.AnalogValue, false);
            case "servo":
                if (prop is "pos" or "position") return (s.Position.ToString("0", Inv), s.Position, false);
                return (s.Angle.ToString(Inv), s.Angle, false);
            case "buzzer":
                if (prop is "freq" or "frequency") return (s.Frequency.ToString(Inv), s.Frequency, false);
                return (s.On ? "on" : "off", s.Frequency, false);
            case "ultrasonic": return (s.DistanceCm.ToString("0.#", Inv), s.DistanceCm, false);
            case "motor":
                if (prop is "dir" or "direction") return (s.Speed > 0 ? "forward" : s.Speed < 0 ? "backward" : "stopped", Math.Sign(s.Speed), false);
                if (prop is "pwm") return (Math.Abs(s.Speed).ToString(Inv), Math.Abs(s.Speed), false);
                return (s.Speed.ToString(Inv), s.Speed, false);
            case "lcd":
                if (prop.StartsWith("line") && int.TryParse(prop[4..], out int ln))
                    return (ln >= 0 && ln < s.LcdLines.Length ? s.LcdLines[ln] : "", null, true);
                return (string.Join("\n", s.LcdLines), null, true);
        }
        return (s.Text, null, false);
    }

    static readonly Dictionary<string, (bool r, bool g, bool b)> Colors = new()
    {
        ["red"] = (true, false, false), ["vermelho"] = (true, false, false),
        ["green"] = (false, true, false), ["verde"] = (false, true, false),
        ["blue"] = (false, false, true), ["azul"] = (false, false, true),
        ["yellow"] = (true, true, false), ["amarelo"] = (true, true, false),
        ["cyan"] = (false, true, true), ["ciano"] = (false, true, true),
        ["magenta"] = (true, false, true), ["purple"] = (true, false, true), ["roxo"] = (true, false, true),
        ["white"] = (true, true, true), ["branco"] = (true, true, true),
        ["off"] = (false, false, false), ["apagado"] = (false, false, false)
    };

    static readonly HashSet<string> TrueWords = new() { "high", "on", "true", "pressed", "ligado", "aceso", "1" };
    static readonly HashSet<string> FalseWords = new() { "low", "off", "false", "released", "desligado", "apagado", "0" };

    /// <summary>Compares an expected value (see class docs) to the actual text/number.</summary>
    public static bool Match(string expected, string text, double? num, double tol, bool contains, out string why, bool rgb = false)
    {
        why = "";
        string e = (expected ?? "").Trim();
        string lower = e.ToLowerInvariant();
        if (rgb && Colors.TryGetValue(lower, out var col))
        {
            var parts = text.Split(',');
            if (parts.Length != 3) return false;
            bool R = int.Parse(parts[0], Inv) > 30, G = int.Parse(parts[1], Inv) > 30, B = int.Parse(parts[2], Inv) > 30;
            return R == col.r && G == col.g && B == col.b;
        }
        foreach (var op in new[] { ">=", "<=", "!=", "==", ">", "<", "=" })
        {
            if (!e.StartsWith(op)) continue;
            string rhs = e[op.Length..].Trim();
            if (num != null && double.TryParse(rhs, NumberStyles.Float, Inv, out var x))
            {
                double n = num.Value;
                return op switch
                {
                    ">=" => n >= x - tol, "<=" => n <= x + tol, ">" => n > x - tol, "<" => n < x + tol,
                    "!=" => Math.Abs(n - x) > tol, _ => Math.Abs(n - x) <= tol
                };
            }
            bool eq = TextEq(rhs, text, num, tol, contains);
            return op == "!=" ? !eq : (op is "=" or "==") && eq;
        }
        int dots = e.IndexOf("..", StringComparison.Ordinal);
        if (dots > 0 && num != null
            && double.TryParse(e[..dots], NumberStyles.Float, Inv, out var lo)
            && double.TryParse(e[(dots + 2)..], NumberStyles.Float, Inv, out var hi))
            return num.Value >= lo - tol && num.Value <= hi + tol;
        return TextEq(e, text, num, tol, contains);
    }

    static bool TextEq(string e, string text, double? num, double tol, bool contains)
    {
        string lower = e.ToLowerInvariant();
        if (!contains)
        {
            if (TrueWords.Contains(lower) || FalseWords.Contains(lower))
            {
                bool want = TrueWords.Contains(lower);
                string t = text.ToLowerInvariant();
                bool actual = TrueWords.Contains(t) ? true : FalseWords.Contains(t) ? false : (num ?? 0) > 0;
                if (t.StartsWith("pwm")) actual = (num ?? 0) >= 128;
                return want == actual;
            }
            if (num != null && double.TryParse(e, NumberStyles.Float, Inv, out var x)) return Math.Abs(num.Value - x) <= tol;
            return string.Equals(e, text.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        return text.Contains(e, StringComparison.Ordinal);
    }

    public static int ParsePin(string s)
    {
        s = (s ?? "").Trim().ToUpperInvariant();
        if (s == "LED_BUILTIN") return 13;
        if (s.StartsWith("A") && int.TryParse(s[1..], out var a) && a >= 0 && a <= 5) return 14 + a;
        if (s.StartsWith("D")) s = s[1..];
        return int.TryParse(s, out var p) && p >= 0 && p < ArduinoBoard.PinCount ? p : -1;
    }

    public static string Describe(SimCheck c)
    {
        if (!string.IsNullOrWhiteSpace(c.Description)) return c.Description;
        string at = " aos " + c.AtMs.ToString(Inv) + " ms";
        return (c.Kind ?? "").ToLowerInvariant() switch
        {
            "pin" => "Pino " + c.Target + " deve estar " + c.Value + at,
            "serial" => "O Monitor Serial deve mostrar \"" + c.Value + "\" até " + c.AtMs.ToString(Inv) + " ms",
            "component" => c.Target + " deve estar " + c.Value + at,
            "robot" => "Robô: " + c.Target + " deve ser " + c.Value + at,
            _ => c.Kind + " " + c.Target + " = " + c.Value
        };
    }
}
