using System.Globalization;
using System.Text;
using EasyCreator.CInterp;

namespace EasyCreator.Arduino;

public enum PinMode { Input, Output, InputPullup }

/// <summary>State of one Arduino Uno pin (0–13 digital, 14–19 = A0–A5).</summary>
public sealed class PinState
{
    public int Number;
    public PinMode Mode = PinMode.Input;
    public bool ModeSet;
    /// <summary>Value written by the sketch: 0–255 (digitalWrite HIGH = 255).</summary>
    public int Value;
    public bool Pwm;
    public bool TrigPulsed;   // a HIGH was written (used by ultrasonic sensors)
}

/// <summary>A component on the virtual breadboard, with its live state.</summary>
internal sealed class Comp
{
    public ComponentSpec Spec = new();
    public string Type => Spec.Type.ToLowerInvariant();
    public string Id => Spec.Id;
    // inputs
    public bool Pressed;
    public int Analog = 512;
    public double DistanceCm = 100;
    // outputs / internal
    public int ToneFreq;
    public double ToneUntilUs = double.MaxValue;
    public double ServoPos = 90;
    public LcdObj? Lcd;

    public int Pin(string name, int def = -1)
    {
        foreach (var kv in Spec.Pins)
            if (string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase)) return kv.Value;
        return def;
    }

    /// <summary>Analog pins can be given as 14–19 or 0–5.</summary>
    public int AnalogPin(string name)
    {
        int p = Pin(name);
        return p >= 0 && p < 6 ? p + 14 : p;
    }
}

internal sealed class ServoObj
{
    public int Pin = -1;
    public int Angle = 90;
    public bool Attached;
}

internal sealed class LcdObj
{
    public int Cols = 16, Rows = 2;
    public char[][] Text;
    public int Col, Row;
    public bool Begun, Backlight = true, Visible = true;
    public int RsPin = -1;

    public LcdObj() { Text = Blank(2, 16); }

    public static char[][] Blank(int rows, int cols) =>
        Enumerable.Range(0, rows).Select(_ => Enumerable.Repeat(' ', cols).ToArray()).ToArray();

    public void Resize(int cols, int rows)
    {
        Cols = Math.Clamp(cols, 1, 40);
        Rows = Math.Clamp(rows, 1, 4);
        Text = Blank(Rows, Cols);
        Col = Row = 0;
    }

    public void Write(string s)
    {
        foreach (char c in s)
        {
            if (c == '\n' || c == '\r') continue;
            if (Row < Rows && Col < Cols) Text[Row][Col] = c;
            Col++;
        }
    }

    public string[] Lines => Text.Select(r => new string(r)).ToArray();
}

/// <summary>
/// Virtual Arduino Uno: pins, components, virtual clock, Serial, and the native functions/classes the sketch calls.
/// Not thread-safe: the sketch thread and the reader must take turns (see <see cref="ArduinoSession"/>).
/// </summary>
public sealed class ArduinoBoard
{
    public const int PinCount = 20;
    static readonly int[] PwmPins = { 3, 5, 6, 9, 10, 11 };
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public readonly PinState[] Pins = new PinState[PinCount];
    /// <summary>Virtual time in microseconds since the sketch started.</summary>
    public double TimeUs;
    /// <summary>Virtual microseconds charged per interpreted statement.</summary>
    public double StepUs = 1.0;
    public double LoopOverheadUs = 10;
    public readonly StringBuilder SerialOut = new();
    public int MaxSerial = 200_000;
    public bool SerialBegun;
    public readonly Queue<char> SerialIn = new();
    public readonly List<string> Warnings = new();
    readonly HashSet<string> _warned = new();
    internal readonly List<Comp> Comps = new();
    readonly Dictionary<int, List<Comp>> _byPin = new();
    internal readonly List<ServoObj> Servos = new();
    internal readonly List<LcdObj> Lcds = new();
    public RobotSim? Robot;
    internal Comp? RobotComp;
    uint _rand = 12345;
    double _physAcc;

    readonly List<(double t, int seq, Action a)> _events = new();
    int _seq;
    /// <summary>Called after each time slice (grading stops here; the live session pauses here).</summary>
    public Action? AfterAdvance;
    public CInterpreter? Ip { get; private set; }

    public ArduinoBoard(IEnumerable<ComponentSpec> comps, string? arenaPreset = null)
    {
        for (int i = 0; i < PinCount; i++) Pins[i] = new PinState { Number = i };
        foreach (var spec in comps ?? Enumerable.Empty<ComponentSpec>())
        {
            var c = new Comp { Spec = spec };
            Comps.Add(c);
            switch (c.Type)
            {
                case "pot": c.Analog = 512; break;
                case "ldr": c.Analog = 600; break;
            }
            foreach (var kv in spec.Pins)
            {
                int p = (c.Type is "pot" or "ldr") ? c.AnalogPin(kv.Key) : kv.Value;
                if (c.Type == "robot" && kv.Key.StartsWith("line", StringComparison.OrdinalIgnoreCase) && p >= 0 && p < 6) p += 14;
                if (!_byPin.TryGetValue(p, out var l)) _byPin[p] = l = new List<Comp>();
                l.Add(c);
            }
            if (c.Type == "robot" && Robot == null)
            {
                Robot = new RobotSim(Arena.Create(arenaPreset ?? (string.IsNullOrWhiteSpace(spec.Color) ? "line" : spec.Color)));
                RobotComp = c;
            }
        }
    }

    internal Comp? FindComp(string id) => Comps.FirstOrDefault(c => string.Equals(c.Id, id, StringComparison.OrdinalIgnoreCase));

    public void Warn(string msg)
    {
        if (_warned.Add(msg)) Warnings.Add(msg);
    }

    public double TimeMs => TimeUs / 1000.0;

    // ===================== time =====================
    public void Schedule(double timeUs, Action a)
    {
        _events.Add((timeUs, _seq++, a));
        _events.Sort((x, y) => x.t != y.t ? x.t.CompareTo(y.t) : x.seq.CompareTo(y.seq));
    }

    /// <summary>Advances virtual time, updating physics and firing scheduled events on the way.</summary>
    public void Advance(double us)
    {
        if (!(us > 0)) { RunDue(); return; }
        double target = TimeUs + us;
        while (TimeUs < target)
        {
            double next = target;
            if (_events.Count > 0) next = Math.Min(next, Math.Max(TimeUs, _events[0].t));
            double dt = Math.Min(next - TimeUs, 1000);
            if (dt > 0)
            {
                Physics(dt);
                TimeUs += dt;
                if (target - TimeUs < 1e-6) TimeUs = target;
            }
            RunDue();
            AfterAdvance?.Invoke();
        }
    }

    void RunDue()
    {
        while (_events.Count > 0 && _events[0].t <= TimeUs + 1e-6)
        {
            var e = _events[0];
            _events.RemoveAt(0);
            e.a();
        }
    }

    void Physics(double dtUs)
    {
        // tones with a duration
        foreach (var c in Comps)
            if (c.ToneFreq > 0 && TimeUs + dtUs >= c.ToneUntilUs) { c.ToneFreq = 0; c.ToneUntilUs = double.MaxValue; }
        _physAcc += dtUs;
        if (_physAcc < 1000) return;
        double dt = _physAcc;
        _physAcc = 0;
        // servos move at ~0.6 degree per ms (SG90)
        foreach (var c in Comps)
        {
            if (c.Type != "servo") continue;
            int target = ServoAngle(c);
            double step = 0.6 * dt / 1000;
            if (Math.Abs(c.ServoPos - target) <= step) c.ServoPos = target;
            else c.ServoPos += Math.Sign(target - c.ServoPos) * step;
        }
        if (Robot != null && RobotComp != null)
        {
            Robot.LeftCmd = MotorCommand(RobotComp, "in1", "in2", "ena");
            Robot.RightCmd = MotorCommand(RobotComp, "in3", "in4", "enb");
            Robot.Step(dt / 1e6, (TimeUs + dtUs) / 1000);
        }
    }

    // ===================== pin helpers =====================
    /// <summary>What the pin actually outputs (0–255): 0 unless it is an OUTPUT.</summary>
    public int EffOut(int pin) => pin >= 0 && pin < PinCount && Pins[pin].Mode == PinMode.Output ? Pins[pin].Value : 0;

    internal int MotorCommand(Comp c, string a, string b, string en)
    {
        int pa = c.Pin(a), pb = c.Pin(b), pe = c.Pin(en);
        bool ha = EffOut(pa) >= 128, hb = EffOut(pb) >= 128;
        int mag = pe >= 0 ? EffOut(pe) : 255;
        if (ha == hb) return 0;
        return ha ? mag : -mag;
    }

    internal int ServoAngle(Comp c)
    {
        int pin = c.Pin("pin", c.Pin("signal"));
        foreach (var s in Servos)
            if (s.Attached && s.Pin == pin) return s.Angle;
        return (int)Math.Round(c.ServoPos);
    }

    internal bool ServoAttached(Comp c)
    {
        int pin = c.Pin("pin", c.Pin("signal"));
        return Servos.Any(s => s.Attached && s.Pin == pin);
    }

    internal int ToneOn(Comp c) => c.ToneFreq;

    int CheckPin(CVal v, string fn)
    {
        long p = v.AsLong;
        if (p < 0 || p >= PinCount)
            throw Ip!.RtErr(fn + "(): o pino " + p + " não existe no Arduino Uno (use 0–13 ou A0–A5)");
        return (int)p;
    }

    public int DigitalLevel(int pin)
    {
        var ps = Pins[pin];
        if (_byPin.TryGetValue(pin, out var comps))
        {
            foreach (var c in comps)
            {
                switch (c.Type)
                {
                    case "button":
                        if (ps.Mode == PinMode.Output) break;
                        if (ps.Mode == PinMode.InputPullup) return c.Pressed ? 0 : 1;
                        return c.Pressed ? 1 : 0;
                    case "robot":
                        if (Robot != null && (pin == c.AnalogPin("lineL") || pin == c.AnalogPin("lineR")))
                            return Robot.LineSensor(pin == c.AnalogPin("lineL")) > 512 ? 1 : 0;
                        break;
                    case "pot":
                    case "ldr":
                        return c.Analog > 512 ? 1 : 0;
                }
            }
        }
        if (ps.Mode == PinMode.Output) return ps.Value >= 128 ? 1 : 0;
        if (ps.Mode == PinMode.InputPullup) return 1;
        return 0;
    }

    public int AnalogLevel(int pin)
    {
        if (_byPin.TryGetValue(pin, out var comps))
        {
            foreach (var c in comps)
            {
                switch (c.Type)
                {
                    case "pot":
                    case "ldr":
                        return Math.Clamp(c.Analog, 0, 1023);
                    case "robot":
                        if (Robot != null && (pin == c.AnalogPin("lineL") || pin == c.AnalogPin("lineR")))
                            return Robot.LineSensor(pin == c.AnalogPin("lineL"));
                        break;
                    case "button":
                        return DigitalLevel(pin) == 1 ? 1023 : 0;
                }
            }
        }
        return DigitalLevel(pin) == 1 && Pins[pin].Mode != PinMode.Input ? 1023 : 0;
    }

    // ===================== registration =====================
    public void Attach(CInterpreter ip)
    {
        Ip = ip;
        ip.Host = this;
        ip.Periodic = () => Advance(ip.PeriodicEvery * StepUs);

        var macros = new (string, string)[]
        {
            ("HIGH", "1"), ("LOW", "0"), ("INPUT", "0"), ("OUTPUT", "1"), ("INPUT_PULLUP", "2"), ("LED_BUILTIN", "13"),
            ("A0", "14"), ("A1", "15"), ("A2", "16"), ("A3", "17"), ("A4", "18"), ("A5", "19"),
            ("PI", "3.14159265358979323846"), ("HALF_PI", "1.57079632679489661923"), ("TWO_PI", "6.283185307179586476925"),
            ("DEG_TO_RAD", "0.017453292519943295769"), ("RAD_TO_DEG", "57.295779513082320876798"),
            ("DEC", "10"), ("HEX", "16"), ("OCT", "8"), ("BIN", "2"), ("PROGMEM", ""), ("LSBFIRST", "0"), ("MSBFIRST", "1")
        };
        foreach (var (n, v) in macros) ip.DefineMacro(n, v);
        ip.DefineFunctionMacro("F", new[] { "x" }, "x");

        ip.AddNative("pinMode", (ip, a) =>
        {
            Need(a, 2, "pinMode");
            int p = CheckPin(a[0], "pinMode");
            long m = a[1].AsLong;
            var ps = Pins[p];
            ps.ModeSet = true;
            ps.Mode = m switch
            {
                0 => PinMode.Input,
                1 => PinMode.Output,
                2 => PinMode.InputPullup,
                _ => throw ip.RtErr("pinMode(): modo inválido " + m + " (use INPUT, OUTPUT ou INPUT_PULLUP)")
            };
            if (ps.Mode != PinMode.Output) { ps.Value = 0; ps.Pwm = false; }
            return CVal.VoidV;
        });
        ip.AddNative("digitalWrite", (ip, a) =>
        {
            Need(a, 2, "digitalWrite");
            int p = CheckPin(a[0], "digitalWrite");
            var ps = Pins[p];
            bool high = a[1].IsTrue;
            if (ps.Mode != PinMode.Output)
            {
                Warn("digitalWrite(" + PinName(p) + ", ...) sem pinMode(" + PinName(p) + ", OUTPUT): no Arduino real a saída fica muito fraca. Use pinMode no setup().");
                ps.Mode = high ? PinMode.InputPullup : PinMode.Input;
            }
            ps.Value = high ? 255 : 0;
            ps.Pwm = false;
            if (high) ps.TrigPulsed = true;
            Advance(4);
            return CVal.VoidV;
        });
        ip.AddNative("digitalRead", (ip, a) =>
        {
            Need(a, 1, "digitalRead");
            int p = CheckPin(a[0], "digitalRead");
            Advance(4);
            return CVal.Int(DigitalLevel(p));
        });
        ip.AddNative("analogRead", (ip, a) =>
        {
            Need(a, 1, "analogRead");
            long p = a[0].AsLong;
            if (p >= 0 && p < 6) p += 14;
            if (p < 14 || p > 19) throw ip.RtErr("analogRead(): só funciona nos pinos analógicos A0 a A5 (recebeu " + a[0].AsLong + ")");
            Advance(100);
            return CVal.Int(AnalogLevel((int)p));
        });
        ip.AddNative("analogWrite", (ip, a) =>
        {
            Need(a, 2, "analogWrite");
            int p = CheckPin(a[0], "analogWrite");
            long v = a[1].AsLong;
            if (v < 0 || v > 255) Warn("analogWrite(" + PinName(p) + ", " + v + "): o valor deve ficar entre 0 e 255.");
            int val = (int)Math.Clamp(v, 0, 255);
            var ps = Pins[p];
            ps.Mode = PinMode.Output;
            if (Array.IndexOf(PwmPins, p) < 0)
            {
                Warn("analogWrite no pino " + PinName(p) + ": esse pino não tem PWM (use 3, 5, 6, 9, 10 ou 11). Ele só liga (≥128) ou desliga.");
                ps.Value = val >= 128 ? 255 : 0;
                ps.Pwm = false;
            }
            else
            {
                ps.Value = val;
                ps.Pwm = val != 0 && val != 255;
            }
            if (val > 0) ps.TrigPulsed = true;
            Advance(4);
            return CVal.VoidV;
        });
        ip.AddNative("delay", (ip, a) =>
        {
            Need(a, 1, "delay");
            double ms = a[0].AsDouble;
            if (ms > 0) Advance(ms * 1000);
            return CVal.VoidV;
        });
        ip.AddNative("delayMicroseconds", (ip, a) =>
        {
            Need(a, 1, "delayMicroseconds");
            double us = a[0].AsDouble;
            if (us > 0) Advance(us);
            return CVal.VoidV;
        });
        ip.AddNative("millis", (ip, a) => CVal.Of(CType.ULong, (long)(TimeUs / 1000)));
        ip.AddNative("micros", (ip, a) => CVal.Of(CType.ULong, (long)TimeUs));
        ip.AddNative("pulseIn", (ip, a) =>
        {
            Need(a, 2, "pulseIn");
            int p = CheckPin(a[0], "pulseIn");
            double timeout = a.Length > 2 ? a[2].AsDouble : 1_000_000;
            double? dist = null;
            int trig = -1;
            if (_byPin.TryGetValue(p, out var comps))
            {
                foreach (var c in comps)
                {
                    if (c.Type == "ultrasonic" && c.Pin("echo") == p) { dist = c.DistanceCm; trig = c.Pin("trig"); break; }
                    if (c.Type == "robot" && c.Pin("echo") == p && Robot != null) { dist = Robot.Sonar(); trig = c.Pin("trig"); break; }
                }
            }
            if (dist == null)
            {
                Advance(timeout);
                return CVal.Of(CType.ULong, 0);
            }
            if (trig < 0 || trig >= PinCount || !Pins[trig].TrigPulsed)
            {
                Warn("pulseIn sem pulso no pino TRIG: antes faça digitalWrite(TRIG, HIGH); delayMicroseconds(10); digitalWrite(TRIG, LOW);");
                Advance(timeout);
                return CVal.Of(CType.ULong, 0);
            }
            Pins[trig].TrigPulsed = false;
            double d = dist.Value;
            if (d < 2 || d >= 400)
            {
                Advance(Math.Min(timeout, 38000));
                return CVal.Of(CType.ULong, d < 2 ? 116 : 0);
            }
            double dur = Math.Round(d * 58.0);
            if (dur > timeout) { Advance(timeout); return CVal.Of(CType.ULong, 0); }
            Advance(dur + 400);
            return CVal.Of(CType.ULong, (long)dur);
        });
        ip.AddNative("tone", (ip, a) =>
        {
            Need(a, 2, "tone");
            int p = CheckPin(a[0], "tone");
            int f = (int)Math.Max(0, a[1].AsLong);
            foreach (var c in CompsOn(p))
            {
                c.ToneFreq = f;
                c.ToneUntilUs = a.Length > 2 ? TimeUs + a[2].AsDouble * 1000 : double.MaxValue;
            }
            Pins[p].Mode = PinMode.Output;
            return CVal.VoidV;
        });
        ip.AddNative("noTone", (ip, a) =>
        {
            Need(a, 1, "noTone");
            int p = CheckPin(a[0], "noTone");
            foreach (var c in CompsOn(p)) { c.ToneFreq = 0; c.ToneUntilUs = double.MaxValue; }
            return CVal.VoidV;
        });
        ip.AddNative("map", (ip, a) =>
        {
            Need(a, 5, "map");
            long x = a[0].AsLong, inMin = a[1].AsLong, inMax = a[2].AsLong, outMin = a[3].AsLong, outMax = a[4].AsLong;
            if (inMax == inMin) throw ip.RtErr("map(): o início e o fim do intervalo de entrada não podem ser iguais");
            return CVal.LongV((x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin);
        });
        ip.AddNative("constrain", (ip, a) =>
        {
            Need(a, 3, "constrain");
            if (a.Any(v => v.T.IsFloat)) return CVal.Dbl(Math.Min(Math.Max(a[0].AsDouble, a[1].AsDouble), a[2].AsDouble));
            var t = CType.Common(a[0].T, CType.Common(a[1].T, a[2].T));
            long x = a[0].L, lo = a[1].L, hi = a[2].L;
            return CVal.Of(t, x < lo ? lo : x > hi ? hi : x);
        });
        ip.AddNative("min", (ip, a) =>
        {
            Need(a, 2, "min");
            if (a[0].T.IsFloat || a[1].T.IsFloat) return CVal.Dbl(Math.Min(a[0].AsDouble, a[1].AsDouble));
            return CVal.Of(CType.Common(a[0].T, a[1].T), Math.Min(a[0].L, a[1].L));
        });
        ip.AddNative("max", (ip, a) =>
        {
            Need(a, 2, "max");
            if (a[0].T.IsFloat || a[1].T.IsFloat) return CVal.Dbl(Math.Max(a[0].AsDouble, a[1].AsDouble));
            return CVal.Of(CType.Common(a[0].T, a[1].T), Math.Max(a[0].L, a[1].L));
        });
        ip.AddNative("sq", (ip, a) =>
        {
            Need(a, 1, "sq");
            return a[0].T.IsFloat ? CVal.Dbl(a[0].D * a[0].D) : CVal.LongV(a[0].L * a[0].L);
        });
        ip.AddNative("random", (ip, a) =>
        {
            Need(a, 1, "random");
            long lo = a.Length > 1 ? a[0].AsLong : 0, hi = a.Length > 1 ? a[1].AsLong : a[0].AsLong;
            if (hi <= lo) return CVal.LongV(lo);
            _rand = _rand * 1103515245 + 12345;
            return CVal.LongV(lo + (long)((_rand >> 1) % (ulong)(hi - lo)));
        });
        ip.AddNative("randomSeed", (ip, a) => { Need(a, 1, "randomSeed"); _rand = (uint)a[0].AsLong; return CVal.VoidV; });
        ip.AddNative("bitRead", (ip, a) => { Need(a, 2, "bitRead"); return CVal.Int((a[0].AsLong >> (int)a[1].AsLong) & 1); });
        ip.AddNative("lowByte", (ip, a) => { Need(a, 1, "lowByte"); return CVal.Of(CType.UChar, a[0].AsLong & 0xFF); });
        ip.AddNative("highByte", (ip, a) => { Need(a, 1, "highByte"); return CVal.Of(CType.UChar, (a[0].AsLong >> 8) & 0xFF); });

        RegisterSerial(ip);
        RegisterServo(ip);
        RegisterLcd(ip);
    }

    IEnumerable<Comp> CompsOn(int pin) => _byPin.TryGetValue(pin, out var l) ? l : Enumerable.Empty<Comp>();

    void Need(CVal[] a, int n, string fn)
    {
        if (a.Length < n) throw Ip!.RtErr(fn + "() precisa de " + n + " argumento(s), mas recebeu " + a.Length);
    }

    public static string PinName(int p) => p >= 14 && p <= 19 ? "A" + (p - 14) : p.ToString(Inv);

    // ===================== Serial =====================
    string PrintText(CInterpreter ip, CVal[] a)
    {
        if (a.Length == 0) return "";
        var v = a[0];
        if (v.T.IsPtr && v.T.Elem!.K == TK.Char) return ip.ReadCString(v.L);
        if (v.T.K == TK.Char && a.Length == 1) return ((char)(byte)v.L).ToString();
        if (v.T.IsFloat)
        {
            int digits = a.Length > 1 ? (int)Math.Clamp(a[1].AsLong, 0, 10) : 2;
            if (double.IsNaN(v.D)) return "nan";
            if (double.IsInfinity(v.D)) return "inf";
            return v.D.ToString("F" + digits, Inv);
        }
        if (v.T.IsInteger || v.T.IsPtr)
        {
            int b = a.Length > 1 ? (int)a[1].AsLong : 10;
            long x = v.L;
            switch (b)
            {
                case 16: return (v.T.K == TK.Long ? (ulong)x : (ulong)(uint)x).ToString("X", Inv);
                case 8: return Convert.ToString(v.T.K == TK.Long ? x : (uint)x, 8);
                case 2: return Convert.ToString(v.T.K == TK.Long ? x : (uint)x, 2);
                default: return x.ToString(Inv);
            }
        }
        throw ip.RtErr("Serial.print não sabe imprimir um valor do tipo " + v.T);
    }

    void SerialWrite(string s)
    {
        if (!SerialBegun)
        {
            Warn("Serial.print usado sem Serial.begin(9600) no setup(): nada aparece no Monitor Serial.");
            return;
        }
        SerialOut.Append(s);
        if (SerialOut.Length > MaxSerial) SerialOut.Remove(0, SerialOut.Length - MaxSerial / 2);
        Advance(s.Length * 10);
    }

    void RegisterSerial(CInterpreter ip)
    {
        var cls = ip.AddClass("HardwareSerial", (_, _) => 1);
        cls.Methods["begin"] = (ip, h, a) => { SerialBegun = true; return CVal.VoidV; };
        cls.Methods["end"] = (ip, h, a) => { SerialBegun = false; return CVal.VoidV; };
        cls.Methods["print"] = (ip, h, a) => { var s = PrintText(ip, a); SerialWrite(s); return CVal.Of(CType.ULong, s.Length); };
        cls.Methods["println"] = (ip, h, a) => { var s = PrintText(ip, a) + "\n"; SerialWrite(s); return CVal.Of(CType.ULong, s.Length); };
        cls.Methods["write"] = (ip, h, a) =>
        {
            if (a.Length == 0) return CVal.Int(0);
            var s = a[0].T.IsPtr ? ip.ReadCString(a[0].L) : ((char)(byte)a[0].AsLong).ToString();
            SerialWrite(s);
            return CVal.Of(CType.ULong, s.Length);
        };
        cls.Methods["available"] = (ip, h, a) => CVal.Int(SerialIn.Count);
        cls.Methods["read"] = (ip, h, a) => CVal.Int(SerialIn.Count > 0 ? (byte)SerialIn.Dequeue() : -1);
        cls.Methods["peek"] = (ip, h, a) => CVal.Int(SerialIn.Count > 0 ? (byte)SerialIn.Peek() : -1);
        cls.Methods["flush"] = (ip, h, a) => CVal.VoidV;
        cls.Methods["setTimeout"] = (ip, h, a) => CVal.VoidV;
        cls.Methods["parseInt"] = (ip, h, a) =>
        {
            while (SerialIn.Count > 0 && !char.IsDigit(SerialIn.Peek()) && SerialIn.Peek() != '-') SerialIn.Dequeue();
            long v = 0; bool neg = false;
            if (SerialIn.Count > 0 && SerialIn.Peek() == '-') { neg = true; SerialIn.Dequeue(); }
            while (SerialIn.Count > 0 && char.IsDigit(SerialIn.Peek())) v = v * 10 + (SerialIn.Dequeue() - '0');
            return CVal.LongV(neg ? -v : v);
        };
        ip.AddNativeObject("Serial", cls);
    }

    // ===================== Servo =====================
    void RegisterServo(CInterpreter ip)
    {
        var cls = ip.AddClass("Servo", (_, _) => { Servos.Add(new ServoObj()); return Servos.Count; });
        ServoObj S(long h) => Servos[(int)h - 1];
        cls.Methods["attach"] = (ip, h, a) =>
        {
            Need(a, 1, "attach");
            var s = S(h);
            s.Pin = CheckPin(a[0], "attach");
            s.Attached = true;
            Pins[s.Pin].Mode = PinMode.Output;
            if (!Comps.Any(c => c.Type == "servo" && c.Pin("pin", c.Pin("signal")) == s.Pin))
                Warn("servo.attach(" + s.Pin + "): não há servo ligado no pino " + s.Pin + " no circuito.");
            return CVal.Of(CType.UChar, 1);
        };
        cls.Methods["detach"] = (ip, h, a) => { S(h).Attached = false; return CVal.VoidV; };
        cls.Methods["attached"] = (ip, h, a) => CVal.Of(CType.Bool, S(h).Attached ? 1 : 0);
        cls.Methods["write"] = (ip, h, a) =>
        {
            Need(a, 1, "write");
            var s = S(h);
            long v = a[0].AsLong;
            if (v >= 544) v = (v - 544) * 180 / (2400 - 544);
            if (v < 0 || v > 180) Warn("servo.write(" + a[0].AsLong + "): o ângulo vai de 0 a 180 graus.");
            s.Angle = (int)Math.Clamp(v, 0, 180);
            if (!s.Attached) Warn("servo.write sem servo.attach(pino) antes: o servo não se mexe.");
            return CVal.VoidV;
        };
        cls.Methods["writeMicroseconds"] = (ip, h, a) =>
        {
            Need(a, 1, "writeMicroseconds");
            long us = Math.Clamp(a[0].AsLong, 544, 2400);
            S(h).Angle = (int)((us - 544) * 180 / (2400 - 544));
            return CVal.VoidV;
        };
        cls.Methods["read"] = (ip, h, a) => CVal.Int(S(h).Angle);
    }

    // ===================== LiquidCrystal =====================
    void RegisterLcd(CInterpreter ip)
    {
        long Create(CVal[] args, bool i2c)
        {
            var lcd = new LcdObj();
            if (i2c && args.Length >= 3) lcd.Resize((int)args[1].AsLong, (int)args[2].AsLong);
            if (!i2c && args.Length > 0) lcd.RsPin = (int)args[0].AsLong;
            Lcds.Add(lcd);
            var comp = Comps.FirstOrDefault(c => c.Type == "lcd" && c.Lcd == null && lcd.RsPin >= 0 && c.Pin("rs") == lcd.RsPin)
                       ?? Comps.FirstOrDefault(c => c.Type == "lcd" && c.Lcd == null);
            if (comp != null) comp.Lcd = lcd;
            else Warn("Há um LCD no código, mas nenhum LCD no circuito.");
            return Lcds.Count;
        }
        var cls = ip.AddClass("LiquidCrystal", (_, a) => Create(a, false));
        var cls2 = ip.AddClass("LiquidCrystal_I2C", (_, a) => Create(a, true));
        LcdObj L(long h) => Lcds[(int)h - 1];
        foreach (var c in new[] { cls, cls2 })
        {
            c.Methods["begin"] = (ip, h, a) =>
            {
                var l = L(h);
                if (a.Length >= 2) l.Resize((int)a[0].AsLong, (int)a[1].AsLong);
                l.Begun = true;
                return CVal.VoidV;
            };
            c.Methods["init"] = (ip, h, a) => { L(h).Begun = true; return CVal.VoidV; };
            c.Methods["clear"] = (ip, h, a) => { var l = L(h); l.Text = LcdObj.Blank(l.Rows, l.Cols); l.Col = l.Row = 0; Advance(1500); return CVal.VoidV; };
            c.Methods["home"] = (ip, h, a) => { var l = L(h); l.Col = l.Row = 0; return CVal.VoidV; };
            c.Methods["setCursor"] = (ip, h, a) =>
            {
                Need(a, 2, "setCursor");
                var l = L(h);
                l.Col = (int)Math.Clamp(a[0].AsLong, 0, 40);
                l.Row = (int)Math.Clamp(a[1].AsLong, 0, l.Rows - 1);
                return CVal.VoidV;
            };
            c.Methods["print"] = (ip, h, a) =>
            {
                var l = L(h);
                if (!l.Begun) Warn("lcd.print antes de lcd.begin(16, 2): chame lcd.begin no setup().");
                var s = PrintText(ip, a);
                l.Write(s);
                Advance(40 * s.Length);
                return CVal.Of(CType.ULong, s.Length);
            };
            c.Methods["write"] = (ip, h, a) => { Need(a, 1, "write"); L(h).Write(((char)(byte)a[0].AsLong).ToString()); return CVal.Of(CType.ULong, 1); };
            c.Methods["backlight"] = (ip, h, a) => { L(h).Backlight = true; return CVal.VoidV; };
            c.Methods["noBacklight"] = (ip, h, a) => { L(h).Backlight = false; return CVal.VoidV; };
            c.Methods["display"] = (ip, h, a) => { L(h).Visible = true; return CVal.VoidV; };
            c.Methods["noDisplay"] = (ip, h, a) => { L(h).Visible = false; return CVal.VoidV; };
            foreach (var noop in new[] { "cursor", "noCursor", "blink", "noBlink", "scrollDisplayLeft", "scrollDisplayRight", "autoscroll", "noAutoscroll", "leftToRight", "rightToLeft", "createChar" })
                c.Methods[noop] = (ip, h, a) => CVal.VoidV;
        }
    }

    // ===================== live input =====================
    public bool SetButton(string id, bool pressed)
    {
        var c = FindComp(id);
        if (c == null || c.Type != "button") return false;
        c.Pressed = pressed;
        return true;
    }

    public bool SetAnalog(string id, int value)
    {
        var c = FindComp(id);
        if (c == null || (c.Type != "pot" && c.Type != "ldr")) return false;
        c.Analog = Math.Clamp(value, 0, 1023);
        return true;
    }

    public bool SetDistance(string id, double cm)
    {
        var c = FindComp(id);
        if (c == null || c.Type != "ultrasonic") return false;
        c.DistanceCm = Math.Clamp(cm, 0, 500);
        return true;
    }

    /// <summary>Puts a box obstacle on the robot's line at <paramref name="alongCm"/> cm from the line start (or ahead of the robot).</summary>
    public bool PlaceObstacle(double alongCm, double size = 12)
    {
        if (Robot == null) return false;
        var p = Robot.Arena.PointAlongLine(alongCm);
        Robot.Arena.Obstacles.Add(new Box(p.X - size / 2, p.Y - size / 2, size, size));
        return true;
    }

    public void ClearObstacles() => Robot?.Arena.Obstacles.Clear();

    public void SendSerial(string text)
    {
        foreach (var ch in text) SerialIn.Enqueue(ch);
    }

    /// <summary>Applies a stimulus string such as "button:btn1=pressed" (the "@ms" part is handled by the caller).</summary>
    public string? ApplyStimulus(string kind, string id, string value)
    {
        switch (kind.ToLowerInvariant())
        {
            case "button":
                {
                    string v = value.Trim().ToLowerInvariant();
                    bool pressed = v is "pressed" or "1" or "true" or "down" or "on" or "pressionado";
                    return SetButton(id, pressed) ? null : "botão '" + id + "' não existe";
                }
            case "pot":
            case "ldr":
            case "analog":
                return int.TryParse(value, NumberStyles.Integer, Inv, out var a) && SetAnalog(id, a) ? null : "estímulo analógico inválido para '" + id + "'";
            case "ultrasonic":
            case "distance":
                return double.TryParse(value, NumberStyles.Float, Inv, out var d) && SetDistance(id, d) ? null : "estímulo de distância inválido para '" + id + "'";
            case "obstacle":
                if (value.Trim().ToLowerInvariant() is "none" or "remove" or "clear") { ClearObstacles(); return null; }
                return double.TryParse(value, NumberStyles.Float, Inv, out var o) && PlaceObstacle(o) ? null : "estímulo de obstáculo inválido";
            case "serial":
                SendSerial(value.Replace("\\n", "\n"));
                return null;
            case "arena":
                return null; // handled when the board is created
            default:
                return "tipo de estímulo desconhecido '" + kind + "'";
        }
    }

    /// <summary>Parses "kind:id=value@ms" (id and @ms optional).</summary>
    public static bool TryParseStimulus(string s, out string kind, out string id, out string value, out double atMs)
    {
        kind = id = value = "";
        atMs = 0;
        if (string.IsNullOrWhiteSpace(s)) return false;
        int at = s.LastIndexOf('@');
        string main = s;
        if (at >= 0)
        {
            if (!double.TryParse(s[(at + 1)..].Trim(), NumberStyles.Float, Inv, out atMs)) return false;
            main = s[..at];
        }
        int colon = main.IndexOf(':');
        if (colon < 0) return false;
        kind = main[..colon].Trim();
        string rest = main[(colon + 1)..];
        int eq = rest.IndexOf('=');
        if (eq < 0) { value = rest.Trim(); return true; }
        id = rest[..eq].Trim();
        value = rest[(eq + 1)..].Trim();
        return true;
    }

    // ===================== snapshots =====================
    public PinSnapshot[] GetPins() => Pins.Select(p => new PinSnapshot(p.Number, PinName(p.Number), p.Mode, p.ModeSet,
        p.Mode == PinMode.Output ? p.Value : DigitalLevel(p.Number) * 255, p.Pwm,
        p.Number >= 14 ? AnalogLevel(p.Number) : -1)).ToArray();

    public List<ComponentSnapshot> GetComponents() => Comps.Select(Snapshot).ToList();

    internal ComponentSnapshot Snapshot(Comp c)
    {
        var s = new ComponentSnapshot { Id = c.Id, Type = c.Type, Color = c.Spec.Color, Pins = new Dictionary<string, int>(c.Spec.Pins) };
        switch (c.Type)
        {
            case "led":
                s.Brightness = EffOut(c.Pin("pin", c.Pin("anode")));
                s.On = s.Brightness > 0;
                s.Text = s.On ? (s.Brightness < 255 ? "aceso (" + s.Brightness + ")" : "aceso") : "apagado";
                break;
            case "rgbled":
                s.R = EffOut(c.Pin("r")); s.G = EffOut(c.Pin("g")); s.B = EffOut(c.Pin("b"));
                s.On = s.R + s.G + s.B > 0;
                s.Brightness = Math.Max(s.R, Math.Max(s.G, s.B));
                s.Text = s.R + "," + s.G + "," + s.B;
                break;
            case "button":
                s.Pressed = c.Pressed;
                s.On = c.Pressed;
                s.Text = c.Pressed ? "pressionado" : "solto";
                break;
            case "pot":
            case "ldr":
                s.AnalogValue = c.Analog;
                s.Text = c.Analog.ToString(Inv);
                break;
            case "servo":
                s.Angle = ServoAngle(c);
                s.Position = c.ServoPos;
                s.On = ServoAttached(c);
                s.Text = s.Angle + "°";
                break;
            case "buzzer":
                s.Frequency = c.ToneFreq;
                s.On = c.ToneFreq > 0 || EffOut(c.Pin("pin")) > 0;
                s.Text = s.On ? (c.ToneFreq > 0 ? c.ToneFreq + " Hz" : "tocando") : "silencioso";
                break;
            case "ultrasonic":
                s.DistanceCm = c.DistanceCm;
                s.Text = c.DistanceCm.ToString("0.#", Inv) + " cm";
                break;
            case "motor":
                s.Speed = MotorCommand(c, "in1", "in2", "en");
                s.On = s.Speed != 0;
                s.Text = s.Speed == 0 ? "parado" : (s.Speed > 0 ? "frente " : "ré ") + Math.Abs(s.Speed);
                break;
            case "lcd":
                s.LcdLines = c.Lcd?.Visible == true ? c.Lcd.Lines : LcdObj.Blank(2, 16).Select(r => new string(r)).ToArray();
                s.On = c.Lcd?.Backlight ?? false;
                s.Text = string.Join("\n", s.LcdLines);
                break;
            case "robot":
                if (Robot != null)
                {
                    s.Speed = (int)Math.Round(Robot.Speed);
                    s.On = Robot.LeftCmd != 0 || Robot.RightCmd != 0;
                    s.Text = "x=" + Robot.X.ToString("0", Inv) + " y=" + Robot.Y.ToString("0", Inv) + " ângulo=" + Robot.AngleDeg.ToString("0", Inv);
                }
                break;
        }
        return s;
    }

    public RobotSnapshot? GetRobot()
    {
        if (Robot == null) return null;
        var r = Robot;
        return new RobotSnapshot
        {
            X = r.X, Y = r.Y, AngleDeg = r.AngleDeg, Radius = r.Radius,
            LeftCommand = r.LeftCmd, RightCommand = r.RightCmd, LeftSpeed = r.LeftV, RightSpeed = r.RightV,
            Collisions = r.Collisions, ReachedGoal = r.ReachedGoal, GoalTimeMs = r.GoalTimeMs,
            SonarCm = r.Sonar(), LineLeft = r.LineSensor(true), LineRight = r.LineSensor(false),
            LineLeftPos = r.LineSensorPos(true), LineRightPos = r.LineSensorPos(false),
            Travelled = r.Travelled, LineRatio = r.LineRatio, Trail = r.Trail.ToList()
        };
    }
}

public sealed record PinSnapshot(int Number, string Name, PinMode Mode, bool ModeSet, int Value, bool Pwm, int Analog);

/// <summary>UI-friendly state of one component. Only the fields that make sense for <see cref="Type"/> are filled.</summary>
public sealed class ComponentSnapshot
{
    public string Id = "", Type = "", Color = "";
    public Dictionary<string, int> Pins = new();
    public bool On;
    public int Brightness;            // led (0–255)
    public int R, G, B;               // rgbled
    public bool Pressed;              // button
    public int AnalogValue;           // pot, ldr (0–1023)
    public int Angle;                 // servo commanded angle
    public double Position;           // servo physical position
    public int Frequency;             // buzzer (Hz)
    public double DistanceCm;         // ultrasonic
    public int Speed;                 // motor (-255..255), robot (cm/s)
    public string[] LcdLines = Array.Empty<string>();
    /// <summary>Short human description in Portuguese ("aceso", "90°", "frente 200"…).</summary>
    public string Text = "";
}

public sealed class RobotSnapshot
{
    public double X, Y, AngleDeg, Radius;
    public int LeftCommand, RightCommand;
    public double LeftSpeed, RightSpeed;
    public int Collisions;
    public bool ReachedGoal;
    public double GoalTimeMs;
    public double SonarCm;
    public int LineLeft, LineRight;
    public Vec2 LineLeftPos, LineRightPos;
    public double Travelled, LineRatio;
    public List<Vec2> Trail = new();
}
