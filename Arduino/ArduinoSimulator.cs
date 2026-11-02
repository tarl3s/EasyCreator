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
            catch (Exception e) { tcs.TrySetResult(lesson.Checks.Select(c => new CheckOutcome(false, Describe(c), L.F("Erro interno do simulador: {0}", e.Message))).ToList()); }
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
                    err = L.T("O sketch precisa ter as duas funções: void setup() { } e void loop() { }.");
            }
            catch (CError e) { err = e.Friendly; }
            catch (Exception e) { err = L.F("Erro ao ler o código: {0}", e.Message); }
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
        return results.Select((r, i) => r ?? new CheckOutcome(false, Describe(checks[i]), L.T("A verificação não foi avaliada."))).ToList();
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
            if (!ArduinoBoard.TryParseStimulus(s, out var kind, out var id, out var value, out var at)) { stimErr = L.F("estímulo mal escrito: {0}", s); continue; }
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
            // the interpreter's wall-clock limit (decided by time, not by its message, which may be translated)
            bool timedOut = !ct.IsCancellationRequested && sw.Elapsed.TotalSeconds >= WallLimit.TotalSeconds * 0.9;
            runError = timedOut
                ? L.F("A simulação demorou demais para chegar aos {0} ms (o código é pesado demais ou travou).", (end / 1000).ToString("0", Inv))
                : e.Friendly;
        }
        catch (InsufficientExecutionStackException) { runError = L.T("Erro de execução: recursão profunda demais."); }
        catch (Exception e) { runError = L.F("Erro interno do simulador perto da linha {0}: {1}", ip.Line, e.Message); }

        foreach (var i in idx)
        {
            if (results[i] != null && runError == null) continue;
            if (results[i] != null && runError != null) continue; // evaluated before the error happened
            results[i] = new CheckOutcome(false, Describe(checks[i]), runError ?? L.T("A simulação terminou antes do tempo da verificação."));
        }
        if (stimErr != null)
            foreach (var i in idx)
                if (results[i]!.Passed) results[i] = new CheckOutcome(false, Describe(checks[i]), L.F("Problema no estímulo da lição: {0}", stimErr));
    }

    // ======================= evaluation =======================
    public static CheckOutcome Evaluate(ArduinoBoard board, SimCheck c)
    {
        string desc = Describe(c);
        string when = L.F(" (aos {0} ms)", c.AtMs.ToString(Inv));
        switch ((c.Kind ?? "").ToLowerInvariant())
        {
            case "pin":
                {
                    int pin = ParsePin(c.Target);
                    if (pin < 0) return new CheckOutcome(false, desc, L.F("Pino inválido na lição: {0}", c.Target));
                    var ps = board.Pins[pin];
                    int v = ps.Mode == PinMode.Output ? ps.Value : board.DigitalLevel(pin) * 255;
                    string text = v == 0 ? "LOW" : v == 255 ? "HIGH" : "PWM " + v;
                    string shown = ps.Mode != PinMode.Output && ps.Value > 0 ? text + L.T(" (sem pinMode OUTPUT)") : text;
                    bool ok = Match(c.Value, text, v, c.Tolerance, false, out _);
                    string name = ArduinoBoard.PinName(pin);
                    return new CheckOutcome(ok, desc, ok ? L.F("OK: pino {0} = {1}{2}", name, shown, when)
                        : L.F("Esperado {0}, mas o pino {1} estava {2}{3}.", c.Value, name, shown, when));
                }
            case "serial":
                {
                    string all = board.SerialOut.ToString().Replace("\r\n", "\n");
                    string exp = c.Value ?? "";
                    bool ok;
                    if (exp.StartsWith("re:")) { try { ok = Regex.IsMatch(all, exp[3..], RegexOptions.None, TimeSpan.FromSeconds(1)); } catch { ok = false; } }
                    else if (exp.StartsWith("!")) ok = !all.Contains(exp[1..].Replace("\n", "\n"), StringComparison.Ordinal);
                    else ok = all.Contains(exp.Replace("\n", "\n"), StringComparison.Ordinal);
                    string tail = all.Length > 160 ? "…" + all[^160..] : all;
                    return new CheckOutcome(ok, desc, ok ? "OK" + when
                        : (all.Length == 0
                            ? (board.SerialBegun ? L.F("O Monitor Serial está vazio{0}.", when) : L.F("O Monitor Serial está vazio{0} — faltou Serial.begin(9600)?", when))
                            : L.F("O Monitor Serial mostrou: \"{0}\"{1}.", tail.Replace("\n", "⏎"), when)));
                }
            case "component":
                {
                    string id = c.Target, prop = "";
                    int dot = c.Target.IndexOf('.');
                    if (dot > 0) { id = c.Target[..dot]; prop = c.Target[(dot + 1)..].ToLowerInvariant(); }
                    var comp = board.FindComp(id);
                    if (comp == null) return new CheckOutcome(false, desc, L.F("Componente '{0}' não existe no circuito da lição.", id));
                    if (comp.Type == "robot") return EvalRobot(board, c, prop.Length > 0 ? prop : "goal", desc, when);
                    var (text, num, contains) = ComponentValue(board, comp, prop);
                    bool ok = Match(c.Value, text, num, c.Tolerance, contains, out _, comp.Type == "rgbled" && prop.Length == 0);
                    return new CheckOutcome(ok, desc, ok ? L.F("OK: {0} = {1}{2}", id, Show(text), when)
                        : L.F("Esperado {0}, mas {1} estava {2}{3}.", c.Value, id, Show(text), when));
                }
            case "robot":
                return EvalRobot(board, c, c.Target.ToLowerInvariant(), desc, when);
            default:
                return new CheckOutcome(false, desc, L.F("Tipo de verificação desconhecido: {0}", c.Kind));
        }
    }

    static string Show(string s) => "\"" + s.Replace("\n", " | ").TrimEnd() + "\"";

    static CheckOutcome EvalRobot(ArduinoBoard board, SimCheck c, string target, string desc, string when)
    {
        var r = board.Robot;
        if (r == null) return new CheckOutcome(false, desc, L.T("Não há robô no circuito da lição."));
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
            case "lineratio": num = r.LineRatio; text = L.F("{0}% do tempo na linha", (r.LineRatio * 100).ToString("0", Inv)); break;
            case "sonar": num = r.Sonar(); text = num.Value.ToString("0", Inv) + " cm"; break;
            case "speed": num = Math.Abs(r.Speed); text = Math.Abs(r.Speed).ToString("0.0", Inv) + " cm/s"; break;
            // both wheels still: a robot spinning in place (average speed 0) is not stopped
            case "stopped": num = r.IsStopped ? 1 : 0; text = r.IsStopped ? "true" : "false"; break;
            default: return new CheckOutcome(false, desc, L.F("Alvo de robô desconhecido: {0}", target));
        }
        bool ok = Match(c.Value, text, num, c.Tolerance, false, out _);
        string label = target switch
        {
            "goal" => r.ReachedGoal ? L.T("o robô chegou ao objetivo") : L.T("o robô não chegou ao objetivo"),
            "collisions" => L.F("o robô bateu {0} vez(es)", r.Collisions),
            "stopped" => r.IsStopped ? L.T("o robô estava parado") : L.T("o robô estava andando"),
            "lineratio" => L.F("o robô ficou sobre a linha {0}", text),
            "distance" or "travelled" => L.F("o robô andou {0}", text),
            "sonar" => L.F("o ultrassom media {0}", text),
            "speed" => L.F("a velocidade era {0}", text),
            _ => target + " = " + text
        };
        return new CheckOutcome(ok, desc, ok ? L.F("OK: {0}{1}.", label, when) : L.F("Esperado {0}, mas {1}{2}.", c.Value, label, when));
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
        string at = c.AtMs.ToString(Inv);
        return (c.Kind ?? "").ToLowerInvariant() switch
        {
            "pin" => L.F("Pino {0} deve estar {1} aos {2} ms", c.Target, c.Value, at),
            "serial" => L.F("O Monitor Serial deve mostrar \"{0}\" até {1} ms", c.Value, at),
            "component" => L.F("{0} deve estar {1} aos {2} ms", c.Target, c.Value, at),
            "robot" => L.F("Robô: {0} deve ser {1} aos {2} ms", c.Target, c.Value, at),
            _ => c.Kind + " " + c.Target + " = " + c.Value
        };
    }
}
