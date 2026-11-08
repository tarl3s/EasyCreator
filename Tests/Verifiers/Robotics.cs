using System.Text.RegularExpressions;

namespace EasyCreator.Verifiers;

/// <summary>Arduino / robotics lessons ard-01..20: every number prompt solved from its text.</summary>
[Verifies("ard-01-pisca-led", "ard-02-botao", "ard-03-pwm-fade", "ard-04-potenciometro", "ard-05-serial-ldr", "ard-06-semaforo",
    "ard-07-servo", "ard-08-ultrassom", "ard-09-motor-dc", "ard-10-robo-seguidor", "ard-11-debounce", "ard-12-multitarefa",
    "ard-13-rampa-pwm", "ard-14-protocolo-serial", "ard-15-histerese", "ard-16-alarme", "ard-17-seguidor-solar",
    "ard-18-pid-distancia", "ard-19-odometria", "ard-20-missao-final")]
public sealed class VArd : IExerciseVerifier
{
    static long Gcd(long a, long b) => b == 0 ? a : Gcd(b, a % b);

    public double? Solve(string p0)
    {
        // drop part names that carry digits (R1, R2, HC-SR04, 8N1) so they don't pollute the numbers
        var p = Regex.Replace(p0, @"R\d =|HC-SR04|8N1", m => m.Value.StartsWith("R") ? "R =" : "");
        var n = Pn.Nums(p);
        bool H(params string[] s) => Vx.Has(p, s);

        // ---- 01 LED
        if (H("resistor em série necessário")) return (n[2] - n[0]) / (n[1] / 1000);
        if (H("fração do tempo")) return n[0] / (n[0] + n[1]) * 100;
        if (H("Quantas vezes ele acende")) return n[2] * 1000 / (n[0] + n[1]);
        if (H("frequência do pisca")) return 1000 / (n[0] + n[1]);
        if (H("potência dissipada no resistor")) { double i = (n[1] - n[0]) / n[2]; return i * i * n[2] * 1000; }
        if (H("LED de") && H("Qual a corrente, em mA")) return (n[1] - n[0]) / n[2] * 1000;
        if (H("LEDs iguais")) return (n[2] - n[0] * n[1]) / (n[3] / 1000);
        // ---- 02 Botão
        if (H("pull-up")) return n[0] / (n[1] * 1000) * 1e6;
        if (H("Quantas leituras")) return Math.Floor(n[1] / n[0]);
        if (H("Quantos apertos acontecem")) return Math.Floor(n[1] / n[0]);
        // ---- 03 PWM
        if (H("tensão média na saída")) return n[0] * n[1] / 255;
        if (H("ciclo de trabalho (duty cycle)")) return n[0] / 255 * 100;
        if (H("fica em HIGH")) return 1000 / n[0] * n[1] / 100;
        if (H("período de cada ciclo")) return 1000 / n[0];
        if (H("Que valor passar ao analogWrite")) return Math.Round(n[0] * 255 / 100);
        if (H("fade soma"))
        {
            double steps = Math.Ceiling((n[3] - n[2]) / n[0]);
            return H("Quantos passos") ? steps : steps * n[1];
        }
        // ---- 04 Potenciômetro
        if (H("valor lido por analogRead")) return Math.Round(n[0] / n[1] * (Math.Pow(2, n[2]) - 1));
        if (H("devolveu")) return n[0] / (Math.Pow(2, n[1]) - 1) * n[2];
        if (H("map("))
        {
            double x = n[0], a = n[1], b = n[2], c = n[3], d = n[4];
            return Math.Truncate((x - a) * (d - c) / (b - a)) + c;
        }
        // ---- 05 Serial / LDR
        if (H("divisor de tensão tem")) return n[1] * n[2] / (n[0] + n[2]);
        if (H("Que valor o analogRead devolve")) return Math.Round(n[2] / (n[0] + n[2]) * 1023);
        if (H("Com o LDR em")) return n[0] * n[1] / (n[1] + n[2]);
        if (H("caracteres por segundo")) return n[0] / n[1];
        if (H("leva para enviar uma linha")) return n[^1] * n[1] / n[0] * 1000;
        // ---- 06 Semáforo
        if (H("semáforo"))
        {
            double g = n[0], y = n[1], r = n[2], cyc = g + y + r;
            if (H("ciclos completos")) return Math.Floor(3600 * n[4] / cyc);
            if (H("dura um ciclo")) return cyc;
            if (H("acende pela")) return (n[4] - 1) * cyc + g + y;
            if (H("no total"))
            {
                double T = n[4], full = Math.Floor(T / cyc), rem = T - full * cyc;
                return full * r + Math.Clamp(rem - (g + y), 0, r);
            }
        }
        // ---- 07 Servo
        if (H("Que largura de pulso")) return n[0] + (n[2] - n[0]) * (n[4] - n[1]) / (n[3] - n[1]);
        if (H("Para qual ângulo")) return n[1] + (n[4] - n[0]) * (n[3] - n[1]) / (n[2] - n[0]);
        if (H("Quanto tempo leva uma ida")) return (n[1] - n[0]) / n[2] * n[3];
        if (H("Quantos passos tem uma ida")) return (n[1] - n[0]) / n[2];
        if (H("Quanto dura a varredura")) return (n[1] - n[0]) * n[^1] / 1000;
        // ---- 08 Ultrassom
        if (H("distância até o obstáculo")) return n[0] * n[2] / 2;
        if (H("velocidade do som no ar a")) return n[1] + n[2] * n[0];
        if (H("leva o eco")) return 2 * n[0] / (n[1] * 100) * 1e6;
        if (H("Qual o erro da distância")) return n[1] * (n[0] - (n[3] + n[4] * n[2])) / 1e4 / 2;
        if (H("distância real")) return n[1] * (n[2] + n[3] * n[0]) / 1e4 / 2;
        // ---- 09 Motor DC
        if (H("proporcional ao PWM")) return n[0] * n[2] / n[1];
        if (H("velocidade linear")) return Math.PI * n[0] / 100 * n[1] / 60;
        if (H("tensão média no motor")) return n[0] * n[1] / 100;
        if (H("Quanto tempo ela dura")) return n[0] / (n[1] * 1000);
        if (H("potência elétrica")) return n[0] * n[1];
        // ---- 10 Seguidor de linha
        if (H("Qual a correção")) return n[0] * n[1];
        if (H("motor direito")) return n[0] - n[1] * n[2];
        if (H("motor esquerdo")) return n[0] + n[1] * n[2];
        if (H("Cinco sensores"))
        {
            double num = 0, den = 0;
            for (int i = 0; i < 5; i++) { num += n[i] * n[5 + i]; den += n[5 + i]; }
            return num / den;
        }
        // ---- 11 Debounce
        if (H("sem debounce")) return n[0] * n[1];
        if (H("apertos por segundo")) return 1000 / n[0];
        // ---- 12 Multitarefa
        if (H("será a próxima")) return n[0] + n[1];
        if (H("usando millis()")) return Math.Floor(n[1] * 1000 / n[0]);
        if (H("somando os dois")) return Math.Floor(n[2] * 1000 / n[0]) + Math.Floor(n[2] * 1000 / n[1]);
        if (H("MESMO instante"))
        {
            long l = (long)n[0] / Gcd((long)n[0], (long)n[1]) * (long)n[1];
            return Math.Floor(n[4] / l) + 1;
        }
        if (H("coincidem pela primeira vez")) return (long)n[0] / Gcd((long)n[0], (long)n[1]) * (long)n[1];
        // ---- 13 Rampa PWM
        if (H("Quantos passos são necessários")) return Math.Ceiling((n[2] - n[1]) / n[0]);
        if (H("Quanto tempo leva a partida")) return Math.Ceiling((n[3] - n[2]) / n[0]) * n[1];
        if (H("unidades de PWM por segundo")) return (n[1] - n[0]) / n[2];
        if (H("Quanto somar a cada atualização")) return (n[1] - n[0]) / (n[2] * 1000 / n[3]);
        // ---- 14 Protocolo serial
        if (H("bytes a serial envia")) return n[0] / n[1] * n[2];
        if (H("println adiciona")) return (n[0] + n[2]) * n[3] / n[1] * 1000;
        // ---- 15 Filtro de média
        if (H("média móvel"))
        {
            int k = (int)n[0]; var v = n.Skip(1).Take(k).ToList(); v.RemoveAt(0); v.Add(n[1 + k]);
            return v.Average();
        }
        if (H("filtro de média")) return n.Skip(1).Average();
        // ---- 17 Seguidor solar
        if (H("diferença (esquerdo")) return n[0] - n[1];
        if (H("ângulo maior")) return Math.Min(n[3], n[0] + n[1] * n[4]);
        if (H("ângulo menor")) return Math.Max(n[2], n[0] - n[1] * n[4]);
        // ---- 18 PID
        if (H("Qual o erro (medida")) return n[1] - n[0];
        if (H("controle P com Kp")) return n[2] * (n[0] - n[1]);
        if (H("Um PID"))
        {
            double kp = n[0], ki = n[1], kd = n[2], dt = n[3], prev = n[4], e = n[5], integ = n[6] + e * dt;
            return kp * e + ki * integ + kd * (e - prev) / dt;
        }
        if (H("erro que sobra")) return n[1] / n[0];
        // ---- 19 Odometria
        if (H("corresponde a cada pulso")) return Math.PI * n[0] / n[1];
        if (H("O robô contou")) return Math.PI * n[0] / n[1] * n[2];
        if (H("andou o seu centro")) return (n[1] + n[2]) / 2;
        if (H("graus (positivo")) return Pn.Deg((n[2] - n[1]) / n[0]);
        if (H("roda de FORA")) return (n[2] + n[0] / 2) * Pn.Rad(n[1]);
        // ---- 20 Missão final
        if (H("Em quantos segundos")) return n[1] / n[0];
        if (H("contorno retangular")) return 3 * n[0] / n[1];
        if (H("desvio inteiro")) return 2 * n[2] + n[0] + 2 * n[1];
        return null;
    }
}
