namespace EasyCreator.Exercises;

/// <summary>Projectile launch without drag (g = 9,8 m/s²). Each draw picks a situation and an unknown: flight time,
/// maximum height, range, launch speed from the range, or (Genius) the two angles that hit a target.</summary>
[Exercises("fis-01-cinematica")]
public sealed class Fis01Projectile : IExerciseGenerator
{
    const double G = 9.8;

    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        string thing = Ex.Pick(r, L.T("uma bola de futebol"), L.T("uma pedra"), L.T("uma bola de golfe"), L.T("uma laranja"), L.T("uma bolinha de tênis"));
        switch (d)
        {
            case Difficulty.Easy:
            {
                // Thrown straight up: time to the top or the maximum height.
                double v = Ex.Step(r, 5, 30, 1);
                if (r.Next(2) == 0)
                    return Ex.Num(L.F("Você lança {0} verticalmente para cima a {1} m/s. Quanto tempo ela leva para chegar ao ponto mais alto?", thing, Ex.N(v)),
                        v / G, "s", L.F("No topo a velocidade é zero: t = v / g = {0} / 9,8 = {1} s.", Ex.N(v), Ex.N(v / G)));
                return Ex.Num(L.F("Você lança {0} verticalmente para cima a {1} m/s. Que altura máxima ela atinge?", thing, Ex.N(v)),
                    v * v / (2 * G), "m", L.F("h = v² / (2g) = {0}² / 19,6 = {1} m.", Ex.N(v), Ex.N(v * v / (2 * G))));
            }
            case Difficulty.Medium:
            {
                // Horizontal launch from a height: fall time, range or landing speed.
                double h = Ex.Step(r, 2, 80, 1), vx = Ex.Step(r, 2, 25, 1);
                double t = Math.Sqrt(2 * h / G);
                string intro = L.F("{0} sai na horizontal a {1} m/s do alto de um prédio de {2} m.", Cap(thing), Ex.N(vx), Ex.N(h));
                return r.Next(3) switch
                {
                    0 => Ex.Num(intro + " " + L.T("Quanto tempo leva para chegar ao chão?"), t, "s",
                        L.F("A queda é como se fosse solta: t = √(2h/g) = √(2·{0}/9,8) = {1} s.", Ex.N(h), Ex.N(t))),
                    1 => Ex.Num(intro + " " + L.T("A que distância horizontal do prédio ela cai?"), vx * t, "m",
                        L.F("t = √(2h/g) = {0} s; x = vₓ·t = {1}·{0} = {2} m.", Ex.N(t), Ex.N(vx), Ex.N(vx * t))),
                    _ => Ex.Num(intro + " " + L.T("Com que velocidade ela chega ao chão?"), Math.Sqrt(vx * vx + 2 * G * h), "m/s",
                        L.F("vᵧ = √(2gh) = {0} m/s; v = √(vₓ² + vᵧ²) = {1} m/s.", Ex.N(Math.Sqrt(2 * G * h)), Ex.N(Math.Sqrt(vx * vx + 2 * G * h)))),
                };
            }
            case Difficulty.Hard:
            {
                // Oblique launch from the ground: range, flight time, or the speed needed for a range.
                double v = Ex.Step(r, 8, 40, 1), a = Ex.Pick(r, 15.0, 20, 25, 30, 35, 40, 50, 55, 60, 65, 70);
                double rad = a * Math.PI / 180, range = v * v * Math.Sin(2 * rad) / G, tf = 2 * v * Math.Sin(rad) / G;
                switch (r.Next(3))
                {
                    case 0:
                        return Ex.Num(L.F("{0} é lançada do chão a {1} m/s, {2}° acima da horizontal. Qual o alcance?", Cap(thing), Ex.N(v), Ex.N(a)),
                            range, "m", L.F("A = v²·sen(2θ)/g = {0}²·sen({1}°)/9,8 = {2} m.", Ex.N(v), Ex.N(2 * a), Ex.N(range)));
                    case 1:
                        return Ex.Num(L.F("{0} é lançada do chão a {1} m/s, {2}° acima da horizontal. Quanto tempo fica no ar?", Cap(thing), Ex.N(v), Ex.N(a)),
                            tf, "s", L.F("t = 2·v·sen θ / g = 2·{0}·sen {1}° / 9,8 = {2} s.", Ex.N(v), Ex.N(a), Ex.N(tf)));
                    default:
                        double want = Ex.Step(r, 10, 90, 5);
                        double need = Math.Sqrt(want * G / Math.Sin(2 * rad));
                        return Ex.Num(L.F("Com que velocidade é preciso lançar {0} a {1}° do chão para que ela caia a {2} m?", thing, Ex.N(a), Ex.N(want)),
                            need, "m/s", L.F("A = v²·sen(2θ)/g → v = √(A·g / sen 2θ) = √({0}·9,8 / sen {1}°) = {2} m/s.", Ex.N(want), Ex.N(2 * a), Ex.N(need)));
                }
            }
            default:
            {
                // Genius: two angles reach the same target; ask the larger one, given a reachable range.
                double v = Ex.Step(r, 12, 35, 1);
                double max = v * v / G, want = Math.Round(max * Ex.Step(r, 0.4, 0.9, 0.05));
                double low = 0.5 * Math.Asin(want * G / (v * v)) * 180 / Math.PI, high = 90 - low;
                return Ex.Num(L.F("Um canhão de brinquedo lança bolinhas a {0} m/s a partir do chão. Para acertar um alvo no chão a {1} m existem dois ângulos possíveis. Qual é o MAIOR deles?", Ex.N(v), Ex.N(want)),
                    high, "°", L.F("sen(2θ) = A·g / v² = {0}. Os ângulos são complementares: θ₁ = {1}° e θ₂ = 90° − θ₁ = {2}°.",
                        Ex.N(want * G / (v * v), 3), Ex.N(low), Ex.N(high)), 1);
            }
        }
    }

    static string Cap(string s) => s.Length == 0 ? s : char.ToUpper(s[0]) + s[1..];
}
