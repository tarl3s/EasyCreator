namespace EasyCreator.Verifiers;

/// <summary>Projectile without drag, g = 9,8 m/s², solved from the prompt text.</summary>
[Verifies("fis-01-cinematica")]
public sealed class VFis01 : IExerciseVerifier
{
    const double G = 9.8;
    static double Rad(double deg) => deg * Math.PI / 180;

    public double? Solve(string p)
    {
        var n = Vx.Nums(p);
        if (Vx.Has(p, "verticalmente para cima"))
            return Vx.Has(p, "ponto mais alto") ? n[0] / G : Vx.Has(p, "altura máxima") ? n[0] * n[0] / (2 * G) : null;
        if (Vx.Has(p, "na horizontal", "prédio"))
        {
            double v = n[0], h = n[1], t = Math.Sqrt(2 * h / G);
            if (Vx.Has(p, "Quanto tempo")) return t;
            if (Vx.Has(p, "distância horizontal")) return v * t;
            if (Vx.Has(p, "Com que velocidade")) return Math.Sqrt(v * v + 2 * G * h);
            return null;
        }
        if (Vx.Has(p, "é lançada do chão"))
        {
            double v = n[0], a = Rad(n[1]);
            if (Vx.Has(p, "alcance")) return v * v * Math.Sin(2 * a) / G;
            if (Vx.Has(p, "no ar")) return 2 * v * Math.Sin(a) / G;
            return null;
        }
        if (Vx.Has(p, "Com que velocidade é preciso lançar"))
            return Math.Sqrt(n[1] * G / Math.Sin(2 * Rad(n[0])));
        if (Vx.Has(p, "dois ângulos", "MAIOR"))
            return 90 - 0.5 * Math.Asin(n[1] * G / (n[0] * n[0])) * 180 / Math.PI;
        return null;
    }
}
