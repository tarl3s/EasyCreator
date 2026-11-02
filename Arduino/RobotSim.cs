namespace EasyCreator.Arduino;

/// <summary>A 2D point/vector in centimetres (arena coordinates: x to the right, y DOWN, like the screen).</summary>
public readonly record struct Vec2(double X, double Y)
{
    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vec2 operator *(Vec2 a, double k) => new(a.X * k, a.Y * k);
    public double Length => Math.Sqrt(X * X + Y * Y);
    public static double Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Y * b.Y;
}

public readonly record struct Segment(Vec2 A, Vec2 B);

/// <summary>An axis-aligned box obstacle (top-left corner + size, cm).</summary>
public readonly record struct Box(double X, double Y, double W, double H);

/// <summary>The robot's world: walls, box obstacles, a black line to follow and a goal circle.</summary>
public sealed class Arena
{
    public string Name = "line";
    public double Width = 240, Height = 160;
    public List<Segment> Walls = new();
    public List<Box> Obstacles = new();
    /// <summary>Polyline of the black line (empty = no line).</summary>
    public List<Vec2> Line = new();
    public double LineWidth = 3;
    public Vec2? Goal;
    public double GoalRadius = 10;
    public Vec2 Start = new(30, 120);
    /// <summary>Degrees; 0 = facing +x (right); positive turns clockwise on screen.</summary>
    public double StartAngleDeg;

    /// <summary>Presets: "line" (curvy line from start to goal), "open" (empty room with a goal),
    /// "obstacles" (room with boxes), "loop" (closed oval line).</summary>
    public static Arena Create(string? preset)
    {
        var a = new Arena { Name = string.IsNullOrWhiteSpace(preset) ? "line" : preset.Trim().ToLowerInvariant() };
        a.Border();
        switch (a.Name)
        {
            case "open":
                a.Start = new Vec2(30, 80);
                a.Goal = new Vec2(210, 80);
                break;
            case "obstacles":
                a.Start = new Vec2(30, 80);
                a.Goal = new Vec2(215, 80);
                a.Obstacles.Add(new Box(90, 55, 20, 50));
                a.Obstacles.Add(new Box(150, 0, 15, 45));
                a.Obstacles.Add(new Box(150, 115, 15, 45));
                break;
            case "loop":
                {
                    for (int i = 0; i <= 120; i++)
                    {
                        double t = i / 120.0 * Math.PI * 2;
                        a.Line.Add(new Vec2(120 + 80 * Math.Cos(t), 80 + 50 * Math.Sin(t)));
                    }
                    a.Start = a.Line[0];
                    a.StartAngleDeg = 90;
                    break;
                }
            default:
                {
                    a.Name = "line";
                    const int n = 200;
                    for (int i = 0; i <= n; i++)
                    {
                        double t = i / (double)n;
                        double x = 30 + 190 * t;
                        double y = 120 - 40 * (1 - Math.Cos(Math.PI * t)) + 10 * Math.Sin(2 * Math.PI * 1.5 * t);
                        a.Line.Add(new Vec2(x, y));
                    }
                    a.Start = a.Line[0];
                    var d = a.Line[3] - a.Line[0];
                    a.StartAngleDeg = Math.Atan2(d.Y, d.X) * 180 / Math.PI;
                    a.Goal = a.Line[^1];
                    break;
                }
        }
        return a;
    }

    void Border()
    {
        var p0 = new Vec2(0, 0); var p1 = new Vec2(Width, 0); var p2 = new Vec2(Width, Height); var p3 = new Vec2(0, Height);
        Walls.Add(new Segment(p0, p1)); Walls.Add(new Segment(p1, p2)); Walls.Add(new Segment(p2, p3)); Walls.Add(new Segment(p3, p0));
    }

    /// <summary>Point on the line at arc length <paramref name="dist"/> cm from its start.</summary>
    public Vec2 PointAlongLine(double dist)
    {
        if (Line.Count == 0) return Start + new Vec2(Math.Cos(StartAngleDeg * Math.PI / 180), Math.Sin(StartAngleDeg * Math.PI / 180)) * dist;
        double acc = 0;
        for (int i = 1; i < Line.Count; i++)
        {
            double l = (Line[i] - Line[i - 1]).Length;
            if (acc + l >= dist) return Line[i - 1] + (Line[i] - Line[i - 1]) * ((dist - acc) / Math.Max(1e-9, l));
            acc += l;
        }
        return Line[^1];
    }

    public double DistanceToLine(Vec2 p)
    {
        double best = double.MaxValue;
        for (int i = 1; i < Line.Count; i++) best = Math.Min(best, DistToSeg(p, Line[i - 1], Line[i]));
        return best;
    }

    public static double DistToSeg(Vec2 p, Vec2 a, Vec2 b)
    {
        var ab = b - a;
        double l2 = Vec2.Dot(ab, ab);
        double t = l2 < 1e-12 ? 0 : Math.Clamp(Vec2.Dot(p - a, ab) / l2, 0, 1);
        return (p - (a + ab * t)).Length;
    }

    public IEnumerable<Segment> AllSolidSegments()
    {
        foreach (var w in Walls) yield return w;
        foreach (var b in Obstacles)
        {
            var p0 = new Vec2(b.X, b.Y); var p1 = new Vec2(b.X + b.W, b.Y); var p2 = new Vec2(b.X + b.W, b.Y + b.H); var p3 = new Vec2(b.X, b.Y + b.H);
            yield return new Segment(p0, p1); yield return new Segment(p1, p2); yield return new Segment(p2, p3); yield return new Segment(p3, p0);
        }
    }
}

/// <summary>Differential-drive robot: two DC motors (L298N style), a front ultrasonic sensor and two line sensors.</summary>
public sealed class RobotSim
{
    public Arena Arena;
    public double X, Y, AngleDeg;
    public double MaxSpeed = 40;        // cm/s at PWM 255
    public double WheelBase = 12;       // cm
    public double Radius = 8;           // body radius, cm
    public int DeadZone = 25;           // PWM below this does not move the motor
    public double LeftV, RightV;        // actual wheel speeds, cm/s
    public int LeftCmd, RightCmd;       // commanded -255..255
    public int Collisions;
    public bool InContact;
    public bool ReachedGoal;
    public double GoalTimeMs = -1;
    public double Travelled;
    public double OnLineTime, TotalTime;
    public readonly List<Vec2> Trail = new();
    double _trailAcc;

    public RobotSim(Arena arena)
    {
        Arena = arena;
        X = arena.Start.X;
        Y = arena.Start.Y;
        AngleDeg = arena.StartAngleDeg;
        Trail.Add(new Vec2(X, Y));
    }

    public Vec2 Pos => new(X, Y);
    double Rad => AngleDeg * Math.PI / 180;
    public Vec2 Heading => new(Math.Cos(Rad), Math.Sin(Rad));
    /// <summary>Unit vector to the robot's left (y-down coordinates).</summary>
    public Vec2 LeftDir => new(Math.Sin(Rad), -Math.Cos(Rad));

    public Vec2 LineSensorPos(bool left) => Pos + Heading * 7 + LeftDir * (left ? 2 : -2);

    /// <summary>Analog reading 0–1023 of a line sensor: ~950 on the black line, ~80 on the white floor.</summary>
    public int LineSensor(bool left)
    {
        if (Arena.Line.Count < 2) return 80;
        double d = Arena.DistanceToLine(LineSensorPos(left));
        double k = Math.Clamp(1 - (d - Arena.LineWidth / 2) / 2.5, 0, 1);
        return (int)Math.Round(80 + 870 * k);
    }

    /// <summary>Distance (cm) the front ultrasonic sensor measures; 400 when nothing is in range.</summary>
    public double Sonar()
    {
        var origin = Pos + Heading * Radius;
        double best = 400;
        foreach (double off in new[] { -8.0, 0, 8.0 })
        {
            double r = (AngleDeg + off) * Math.PI / 180;
            var dir = new Vec2(Math.Cos(r), Math.Sin(r));
            foreach (var s in Arena.AllSolidSegments())
            {
                var t = Ray(origin, dir, s);
                if (t != null && t.Value < best) best = t.Value;
            }
        }
        return best;
    }

    static double? Ray(Vec2 o, Vec2 d, Segment s)
    {
        var e = s.B - s.A;
        double den = d.X * e.Y - d.Y * e.X;
        if (Math.Abs(den) < 1e-12) return null;
        var w = s.A - o;
        double t = (w.X * e.Y - w.Y * e.X) / den;
        double u = (w.X * d.Y - w.Y * d.X) / den;
        if (t < 0 || u < 0 || u > 1) return null;
        return t;
    }

    double WheelTarget(int cmd)
    {
        int m = Math.Abs(cmd);
        if (m < DeadZone) return 0;
        return Math.Sign(cmd) * (m - DeadZone) / (255.0 - DeadZone) * MaxSpeed;
    }

    public void Step(double dtSec, double nowMs)
    {
        if (dtSec <= 0) return;
        double k = Math.Min(1, dtSec / 0.06);
        LeftV += (WheelTarget(LeftCmd) - LeftV) * k;
        RightV += (WheelTarget(RightCmd) - RightV) * k;
        double v = (LeftV + RightV) / 2;
        double w = (LeftV - RightV) / WheelBase;           // rad/s, positive = clockwise on screen
        double na = AngleDeg + w * dtSec * 180 / Math.PI;
        double r = (AngleDeg + na) / 2 * Math.PI / 180;
        double nx = X + Math.Cos(r) * v * dtSec, ny = Y + Math.Sin(r) * v * dtSec;
        AngleDeg = Normalize(na);
        bool hit = Collides(new Vec2(nx, ny));
        if (hit)
        {
            if (!InContact) Collisions++;
            InContact = true;
            LeftV *= 0.5; RightV *= 0.5;
        }
        else
        {
            if (Collides(Pos) == false) InContact = false;
            Travelled += Math.Sqrt((nx - X) * (nx - X) + (ny - Y) * (ny - Y));
            X = nx; Y = ny;
        }
        if (!ReachedGoal)
        {
            TotalTime += dtSec;
            if (Arena.Line.Count > 1 && Arena.DistanceToLine(Pos) < 3) OnLineTime += dtSec;
        }
        if (!ReachedGoal && Arena.Goal is Vec2 g && (Pos - g).Length < Arena.GoalRadius)
        {
            ReachedGoal = true;
            GoalTimeMs = nowMs;
        }
        _trailAcc += dtSec;
        if (_trailAcc >= 0.05)
        {
            _trailAcc = 0;
            Trail.Add(Pos);
            if (Trail.Count > 4000) Trail.RemoveRange(0, 1000);
        }
    }

    bool Collides(Vec2 p)
    {
        foreach (var s in Arena.AllSolidSegments())
            if (Arena.DistToSeg(p, s.A, s.B) < Radius) return true;
        return false;
    }

    static double Normalize(double a)
    {
        a %= 360;
        if (a > 180) a -= 360;
        if (a <= -180) a += 360;
        return a;
    }

    public double LineRatio => TotalTime <= 0 ? 0 : OnLineTime / TotalTime;
    public double Speed => (LeftV + RightV) / 2;
}
