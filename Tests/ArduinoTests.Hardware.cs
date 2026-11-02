using EasyCreator.Arduino;
using EasyCreator.CInterp;

namespace EasyCreator.Tests;

/// <summary>Regression tests for the hardware model: the simulator must behave like a real Arduino Uno wherever the
/// robotics lessons rely on it (pull-ups, PWM pins and values, ADC, map(), HC-SR04 timing, servo speed, H-bridge,
/// differential drive, Serial float formatting).</summary>
public static partial class ArduinoTests
{
    /// <summary>Runs setup() and loop() on a fresh board until <paramref name="untilMs"/> of virtual time.</summary>
    static ArduinoBoard RunBoard(List<ComponentSpec> comps, string sketch, double untilMs, Action<ArduinoBoard>? before = null)
    {
        var b = new ArduinoBoard(comps);
        var ip = new CInterpreter();
        b.Attach(ip);
        ip.Compile(sketch);
        before?.Invoke(b);
        b.AfterAdvance = () => { if (b.TimeUs >= untilMs * 1000) throw new CHalt(); };
        try { ip.Call("setup"); while (true) { ip.Call("loop"); b.Advance(b.LoopOverheadUs); } } catch (CHalt) { }
        return b;
    }

    static void RunHardwareModelTests()
    {
        var button = new List<ComponentSpec> { Comp("btn1", "button", ("pin", 2)) };
        const string printPin2 = "void setup(){ pinMode(2, INPUT); Serial.begin(9600); } void loop(){ Serial.print(digitalRead(2)); delay(3); }";

        // a button wired to GND on a plain INPUT pin: released = floating (noise + warning), pressed = LOW
        {
            var b = RunBoard(button, printPin2, 300);
            string o = b.SerialOut.ToString();
            Check("hw: botão em INPUT sem pull-up flutua (lê 0 e 1) e avisa",
                o.Contains('0') && o.Contains('1') && b.Warnings.Any(w => w.Contains("INPUT_PULLUP")), o.Length > 40 ? o[..40] : o);
            var p = RunBoard(button, printPin2, 300, x => x.SetButton("btn1", true));
            string po = p.SerialOut.ToString();
            Check("hw: botão apertado liga o pino ao GND (LOW)", po.Length > 10 && po.All(ch => ch == '0'), po.Length > 40 ? po[..40] : po);
        }

        // INPUT_PULLUP: released HIGH, pressed LOW (inverted logic)
        {
            const string s = "void setup(){ pinMode(2, INPUT_PULLUP); Serial.begin(9600); } void loop(){ Serial.print(digitalRead(2)); delay(10); }";
            string up = RunBoard(button, s, 100).SerialOut.ToString();
            string down = RunBoard(button, s, 100, x => x.SetButton("btn1", true)).SerialOut.ToString();
            Check("hw: INPUT_PULLUP solto = HIGH, apertado = LOW", up.Length > 3 && up.All(c => c == '1') && down.Length > 3 && down.All(c => c == '0'), up + " / " + down);
        }

        // analogWrite: PWM only on 3, 5, 6, 9, 10, 11; out-of-range values behave like the real timers
        {
            var b = RunBoard(new List<ComponentSpec>(),
                "void setup(){ analogWrite(3, 300); analogWrite(5, 256); analogWrite(6, 128); analogWrite(9, 300); analogWrite(11, -1); analogWrite(4, 100); analogWrite(7, 200); } void loop(){ delay(10); }", 20);
            Check("hw: analogWrite 300 no pino 3 dá a volta (44)", b.EffOut(3) == 44 && b.Pins[3].Pwm, b.EffOut(3).ToString());
            Check("hw: analogWrite 256 no pino 5 vira 0", b.EffOut(5) == 0, b.EffOut(5).ToString());
            Check("hw: analogWrite 128 no pino 6 é PWM 50%", b.EffOut(6) == 128 && b.Pins[6].Pwm);
            Check("hw: analogWrite 300 no pino 9 (Timer1, 16 bits) fica no máximo", b.EffOut(9) == 255, b.EffOut(9).ToString());
            Check("hw: analogWrite -1 no pino 11 vira 255", b.EffOut(11) == 255, b.EffOut(11).ToString());
            Check("hw: pino sem PWM: <128 desliga, ≥128 liga", b.EffOut(4) == 0 && b.EffOut(7) == 255 && !b.Pins[7].Pwm);
            Check("hw: analogWrite fora de 0–255 avisa", b.Warnings.Any(w => w.Contains("0 a 255")), string.Join(";", b.Warnings));
        }

        // Servo library takes Timer1: analogWrite on 9/10 warns
        {
            var b = RunBoard(new List<ComponentSpec> { Comp("servo1", "servo", ("pin", 9)) },
                "#include <Servo.h>\nServo s; void setup(){ s.attach(9); analogWrite(10, 100); } void loop(){ delay(10); }", 20);
            Check("hw: Servo + analogWrite no pino 10 avisa (Timer1)", b.Warnings.Any(w => w.Contains("Timer1")), string.Join(";", b.Warnings));
        }

        // ADC: 10 bits (0–1023), map() with integer math like Arduino's
        {
            var r = Grade(new List<ComponentSpec> { Comp("pot1", "pot", ("pin", 14)) },
                new List<SimCheck> { Chk("serial", "", "1023 255 127 63 5 -49", 200, "pot:pot1=1023@0") },
                "void setup(){ Serial.begin(9600); } void loop(){ int v = analogRead(A0); Serial.print(v); Serial.print(' '); Serial.print(map(v, 0, 1023, 0, 255)); Serial.print(' ');" +
                " Serial.print(map(512, 0, 1023, 0, 255)); Serial.print(' '); Serial.print(map(256, 0, 1023, 0, 255)); Serial.print(' '); Serial.print(map(5, 0, 10, 10, 0)); Serial.print(' ');" +
                " Serial.println(map(-200, 0, 1023, 0, 255)); delay(1000); }");
            Check("hw: analogRead 0–1023 e map() inteiro (trunca)", r[0].Passed, r[0].Detail);
        }

        // HC-SR04: echo time = distance × 58 µs (ida e volta a 343 m/s); lesson formula duration × 0.0343 / 2
        {
            var us = new List<ComponentSpec> { Comp("us1", "ultrasonic", ("trig", 7), ("echo", 6)) };
            const string s = "void setup(){ pinMode(7, OUTPUT); pinMode(6, INPUT); Serial.begin(9600); }\n" +
                             "void loop(){ digitalWrite(7, LOW); delayMicroseconds(2); digitalWrite(7, HIGH); delayMicroseconds(10); digitalWrite(7, LOW);\n" +
                             " long d = pulseIn(6, HIGH); Serial.print(d); Serial.print(\" \"); Serial.println(d * 0.0343 / 2); delay(100); }";
            var r = Grade(us, new List<SimCheck> { Chk("serial", "", "1160 19.89", 300, "ultrasonic:us1=20@0") }, s);
            Check("hw: HC-SR04 a 20 cm devolve 1160 µs (≈19,9 cm pela fórmula)", r[0].Passed, r[0].Detail);
            const string far = "void setup(){ pinMode(7, OUTPUT); Serial.begin(9600); }\n" +
                               "void loop(){ digitalWrite(7, HIGH); delayMicroseconds(10); digitalWrite(7, LOW); Serial.print(pulseIn(6, HIGH)); Serial.print(' ');\n" +
                               " digitalWrite(7, HIGH); delayMicroseconds(10); digitalWrite(7, LOW); Serial.println(pulseIn(6, HIGH, 30000)); delay(100); }";
            var rf = Grade(us, new List<SimCheck> { Chk("serial", "", "38000 0\n", 300, "ultrasonic:us1=450@0") }, far);
            Check("hw: HC-SR04 sem eco: ECHO fica ~38 ms em HIGH; com timeout 30000 o pulseIn dá 0", rf[0].Passed, rf[0].Detail);
            var none = RunBoard(us, "void setup(){ Serial.begin(9600); } void loop(){ Serial.println(pulseIn(6, HIGH, 30000)); delay(50); }", 200);
            Check("hw: pulseIn sem pulso no TRIG devolve 0 e avisa", none.SerialOut.ToString().StartsWith("0") && none.Warnings.Any(w => w.Contains("TRIG")), none.SerialOut.ToString());
        }

        // Serial.print(float): 2 decimals by default, or the number asked for
        {
            var r = Grade(new List<ComponentSpec>(), new List<SimCheck> { Chk("serial", "", "0.33 -1.50 3.142 3 2.00", 100) },
                "void setup(){ Serial.begin(9600); Serial.print(1.0 / 3); Serial.print(' '); Serial.print(-1.5); Serial.print(' '); Serial.print(3.14159, 3); Serial.print(' ');" +
                " Serial.print(3.14159, 0); Serial.print(' '); float x = 2; Serial.println(x); } void loop(){ }");
            Check("hw: Serial.print de float com 2 casas", r[0].Passed, r[0].Detail);
        }

        // servo speed ~0.6°/ms (SG90: 0.1 s per 60°): commanded angle jumps, the shaft follows
        {
            var r = Grade(new List<ComponentSpec> { Comp("servo1", "servo", ("pin", 9)) },
                new List<SimCheck> { Chk("component", "servo1", "180", 100), Chk("component", "servo1.pos", "140..160", 100), Chk("component", "servo1.pos", "180", 400) },
                "#include <Servo.h>\nServo s; void setup(){ s.attach(9); s.write(180); } void loop(){ }");
            Check("hw: servo gira ~60° em 0,1 s e chega ao alvo", r.All(x => x.Passed), string.Join(" / ", r.Select(x => x.Detail)));
        }

        // L298N H-bridge: IN1/IN2 set the direction, EN (PWM) the speed, equal inputs stop the motor
        {
            var m = new List<ComponentSpec> { Comp("motor1", "motor", ("in1", 8), ("in2", 7), ("en", 5)) };
            const string s = "void setup(){ pinMode(8, OUTPUT); pinMode(7, OUTPUT); analogWrite(5, 200); digitalWrite(8, HIGH); digitalWrite(7, LOW); delay(100);\n" +
                             " digitalWrite(8, LOW); digitalWrite(7, HIGH); delay(100); digitalWrite(8, HIGH); } void loop(){ }";
            var r = Grade(m, new List<SimCheck> { Chk("component", "motor1", "200", 50), Chk("component", "motor1.dir", "backward", 150), Chk("component", "motor1", "0", 250) }, s);
            Check("hw: ponte H (sentido, velocidade e parada)", r.All(x => x.Passed), string.Join(" / ", r.Select(x => x.Detail)));
        }

        // differential drive: spinning in place has average speed 0 but is NOT stopped
        {
            var robot = new List<ComponentSpec> { new() { Id = "robo", Type = "robot", Color = "open", Pins = new() { ["in1"] = 8, ["in2"] = 7, ["ena"] = 5, ["in3"] = 4, ["in4"] = 2, ["enb"] = 6, ["trig"] = 10, ["echo"] = 11, ["lineL"] = 14, ["lineR"] = 15 } } };
            const string spin = "void setup(){ pinMode(8,OUTPUT); pinMode(7,OUTPUT); pinMode(4,OUTPUT); pinMode(2,OUTPUT);\n" +
                                " digitalWrite(8,HIGH); digitalWrite(7,LOW); digitalWrite(4,LOW); digitalWrite(2,HIGH); analogWrite(5,200); analogWrite(6,200); delay(1000);\n" +
                                " analogWrite(5,0); analogWrite(6,0); } void loop(){ }";
            var r = Grade(robot, new List<SimCheck> { Chk("robot", "speed", "<1", 800), Chk("robot", "stopped", "false", 800), Chk("robot", "stopped", "true", 1600) }, spin);
            Check("hw: robô girando no lugar não conta como parado", r.All(x => x.Passed), string.Join(" / ", r.Select(x => x.Detail)));
            // right wheel faster → turns left (counter-clockwise on screen, angle decreases)
            const string left = "void setup(){ pinMode(8,OUTPUT); pinMode(7,OUTPUT); pinMode(4,OUTPUT); pinMode(2,OUTPUT);\n" +
                                " digitalWrite(8,HIGH); digitalWrite(7,LOW); digitalWrite(4,HIGH); digitalWrite(2,LOW); analogWrite(5,100); analogWrite(6,220); } void loop(){ }";
            var t = Grade(robot, new List<SimCheck> { Chk("robot", "angle", "<-10", 1000) }, left);
            Check("hw: roda direita mais rápida vira o robô para a esquerda", t[0].Passed, t[0].Detail);
        }
    }
}
