using Complex = System.Numerics.Complex;

namespace EasyCreator.Electrical;

/// <summary>Modified Nodal Analysis of a <see cref="Netlist"/>: DC operating point (diodes by iterating their on/off
/// state), transient (BDF2 / Gear with backward-Euler steps at discontinuities; capacitors and inductors as companion
/// models) and
/// AC steady state with complex phasors. Never throws: a failure sets <see cref="Error"/> (in Portuguese).</summary>
public sealed class CircuitSim
{
    const double Gmin = 1e-9;     // tiny conductance from every node to ground: floating nodes still have a solution
    const double Gswitch = 1e3;   // closed switch = 1 mΩ
    const double Goff = 1e-9;     // diode off

    enum Mode { Dc, Init, Tran }

    readonly Netlist _n;
    public string Error { get; private set; } = "";
    /// <summary>Node voltages by electrical index (DC / current transient time).</summary>
    public double[] NodeV { get; private set; } = Array.Empty<double>();
    public Complex[] NodeVac { get; private set; } = Array.Empty<Complex>();
    public bool DiodesConverged { get; private set; } = true;

    public CircuitSim(Netlist n) { _n = n; }

    public double V(string node) { int i = _n.IndexOf(node); return i < 0 || i >= NodeV.Length ? 0 : NodeV[i]; }
    public Complex Vac(string node) { int i = _n.IndexOf(node); return i < 0 || i >= NodeVac.Length ? Complex.Zero : NodeVac[i]; }

    // ───────────────────────── DC and transient (real) ─────────────────────────

    public bool SolveDc()
    {
        foreach (var e in _n.Elems) { e.Vprev = e.Iprev = 0; }
        return SolveNonlinear(Mode.Dc, 0, false, 0);
    }

    /// <summary>Runs from t = 0 (capacitors discharged, inductors without current) to <paramref name="tstop"/>.
    /// <paramref name="record"/> is called at t = 0 and after every step (read <see cref="NodeV"/> and the elements).
    /// Integration: BDF2 (Gear, 2nd order). It is L-stable, so a step much longer than a time constant settles instead of
    /// ringing (the trapezoidal rule rings there: a stiff RC driven by a 0–5 V square wave swung from −0.3 V to 5.3 V).
    /// Backward Euler takes the first two steps and the two steps after any discontinuity (a square-wave edge, a diode
    /// switching), where the history of the previous points no longer describes a smooth curve.</summary>
    public bool RunTransient(double tstop, int steps, Action<double> record)
    {
        if (!(tstop > 0) || steps < 1) { Error = L.T("Tempo de simulação inválido."); return false; }
        foreach (var e in _n.Elems) { e.Vprev = e.Iprev = e.Vprev2 = e.Iprev2 = 0; e.DOn = false; }
        double dt = tstop / steps;
        if (!SolveNonlinear(Mode.Init, 0, false, 0))
        {
            // A capacitor straight across a voltage source has no "t = 0" state: show the first step instead.
            Error = "";
            if (!SolveNonlinear(Mode.Tran, dt, false, 0)) return false;
            foreach (var e in _n.Elems) e.DOn = false;
        }
        record(0);
        int euler = 2;  // backward-Euler steps still to take
        for (int k = 1; k <= steps; k++)
        {
            double t = k * dt;
            if (SquareEdge(t - dt, t)) euler = 2;
            bool bdf2 = euler == 0;
            var before = SaveDiodes();
            if (!SolveNonlinear(Mode.Tran, dt, bdf2, t)) return false;
            if (bdf2 && DiodesChanged(before))
            {
                // A diode switched during the step: redo it (and the next one) with backward Euler.
                RestoreDiodes(before);
                if (!SolveNonlinear(Mode.Tran, dt, false, t)) return false;
                euler = 1;
            }
            else if (euler > 0) euler--;
            foreach (var e in _n.Elems)
                if (e.Kind is ElemKind.C or ElemKind.L) { e.Vprev2 = e.Vprev; e.Iprev2 = e.Iprev; e.Vprev = e.Vd; e.Iprev = e.Id; }
            record(t);
        }
        return true;
    }

    /// <summary>True when a square-wave source changes level between <paramref name="t0"/> and <paramref name="t1"/>.</summary>
    bool SquareEdge(double t0, double t1)
    {
        foreach (var e in _n.Elems)
            if (e.Wave == "sq" && e.IsSource && SourceValue(e, t0) != SourceValue(e, t1)) return true;
        return false;
    }

    bool[] SaveDiodes() => _n.Elems.Select(e => e.DOn).ToArray();
    bool DiodesChanged(bool[] s) { for (int i = 0; i < s.Length; i++) if (_n.Elems[i].Kind == ElemKind.D && _n.Elems[i].DOn != s[i]) return true; return false; }
    void RestoreDiodes(bool[] s) { for (int i = 0; i < s.Length; i++) _n.Elems[i].DOn = s[i]; }

    bool SolveNonlinear(Mode mode, double dt, bool bdf2, double t)
    {
        DiodesConverged = true;
        var diodes = _n.Elems.Where(e => e.Kind == ElemKind.D).ToList();
        for (int it = 0; it < 100; it++)
        {
            if (!SolveReal(mode, dt, bdf2, t, out var x)) return false;
            Results(mode, x, t);
            Elem? worst = null; double worstAmt = 0;
            var wrong = new List<Elem>();
            foreach (var d in diodes)
            {
                bool want = d.Vd > d.Vf + (d.DOn ? -1e-9 : 1e-9);
                if (want == d.DOn) continue;
                wrong.Add(d);
                double amt = Math.Abs(d.Vd - d.Vf);
                if (amt > worstAmt) { worstAmt = amt; worst = d; }
            }
            if (wrong.Count == 0) return true;
            if (it < 8) foreach (var d in wrong) d.DOn = !d.DOn;   // flip every wrong diode first…
            else worst!.DOn = !worst.DOn;                          // …then one at a time, so a bridge can't oscillate
        }
        DiodesConverged = false; // keep the last solution: it is close enough to show
        return true;
    }

    static double SourceValue(Elem e, double t) => e.Wave switch
    {
        "sin" => e.Value * Math.Sin(2 * Math.PI * e.Freq * t + e.PhaseDeg * Math.PI / 180),
        "sq" => e.Freq <= 0 || (t * e.Freq) % 1.0 < 0.5 ? e.Value : 0,
        _ => e.Value,
    };

    static double Gr(Elem e) => 1 / Math.Max(Math.Abs(e.Value), 1e-3);

    bool SolveReal(Mode mode, double dt, bool bdf2, double t, out double[] x)
    {
        int nN = _n.NodeCount, nb = 0;
        foreach (var e in _n.Elems)
        {
            bool branch = e.Kind is ElemKind.V or ElemKind.A
                || (e.Kind == ElemKind.L && mode == Mode.Dc)
                || (e.Kind == ElemKind.C && mode == Mode.Init && e.Value > 0);
            e.Br = branch ? nN + nb++ : -1;
        }
        int size = nN + nb;
        var A = new double[size, size];
        var b = new double[size];
        for (int i = 0; i < nN; i++) A[i, i] += Gmin;

        void G(Elem e, double g)
        {
            int a = e.Na, c = e.Nb;
            if (a >= 0) A[a, a] += g;
            if (c >= 0) A[c, c] += g;
            if (a >= 0 && c >= 0) { A[a, c] -= g; A[c, a] -= g; }
        }
        void J(int into, int from, double v) { if (into >= 0) b[into] += v; if (from >= 0) b[from] -= v; }
        void Branch(Elem e, double value)
        {
            int k = e.Br;
            if (e.Na >= 0) { A[e.Na, k] += 1; A[k, e.Na] += 1; }
            if (e.Nb >= 0) { A[e.Nb, k] -= 1; A[k, e.Nb] -= 1; }
            b[k] = value;
        }

        foreach (var e in _n.Elems)
        {
            switch (e.Kind)
            {
                case ElemKind.R: G(e, Gr(e)); break;
                case ElemKind.S: if (e.On) G(e, Gswitch); break;
                case ElemKind.D:
                    if (e.DOn) { G(e, 1 / e.Ron); J(e.Na, e.Nb, e.Vf / e.Ron); }
                    else G(e, Goff);
                    break;
                case ElemKind.I: J(e.Na, e.Nb, SourceValue(e, t)); break;
                case ElemKind.V: Branch(e, SourceValue(e, t)); break;
                case ElemKind.A: Branch(e, 0); break;
                case ElemKind.C:
                    if (mode == Mode.Init) { if (e.Br >= 0) Branch(e, e.Vprev); }
                    else if (mode == Mode.Tran && e.Value > 0)
                    {
                        // i = C·dv/dt. Backward Euler: i = C/dt·(v − v₁). BDF2: i = C/dt·(1.5v − 2v₁ + 0.5v₂).
                        double cdt = e.Value / dt;
                        e.Geq = bdf2 ? 1.5 * cdt : cdt;
                        e.Jeq = bdf2 ? cdt * (2 * e.Vprev - 0.5 * e.Vprev2) : cdt * e.Vprev;
                        G(e, e.Geq); J(e.Na, e.Nb, e.Jeq);
                    }
                    else e.Geq = e.Jeq = 0;
                    break;
                case ElemKind.L:
                    if (mode == Mode.Dc) Branch(e, 0);
                    else if (mode == Mode.Init) { e.Geq = 0; e.Jeq = e.Iprev; J(e.Nb, e.Na, e.Jeq); }
                    else
                    {
                        double l = Math.Max(e.Value, 1e-12);
                        // v = L·di/dt. Backward Euler: i = i₁ + dt/L·v. BDF2: i = (4i₁ − i₂)/3 + 2dt/(3L)·v.
                        e.Geq = bdf2 ? 2 * dt / (3 * l) : dt / l;
                        e.Jeq = bdf2 ? (4 * e.Iprev - e.Iprev2) / 3 : e.Iprev;
                        G(e, e.Geq); J(e.Nb, e.Na, e.Jeq);
                    }
                    break;
            }
        }
        if (!Gauss(A, b, out x)) { Error = SingularMessage(); return false; }
        NodeV = x.Take(nN).ToArray();
        return true;
    }

    void Results(Mode mode, double[] x, double t)
    {
        double Nv(int i) => i < 0 ? 0 : x[i];
        foreach (var e in _n.Elems)
        {
            double v = Nv(e.Na) - Nv(e.Nb);
            e.Vd = v;
            e.Id = e.Kind switch
            {
                ElemKind.R => v * Gr(e),
                ElemKind.S => e.On ? v * Gswitch : 0,
                ElemKind.D => e.DOn ? (v - e.Vf) / e.Ron : v * Goff,
                ElemKind.I => SourceValue(e, t),
                ElemKind.V => -x[e.Br],
                ElemKind.A => x[e.Br],
                ElemKind.C => mode == Mode.Init ? (e.Br >= 0 ? x[e.Br] : 0) : mode == Mode.Tran ? e.Geq * v - e.Jeq : 0,
                ElemKind.L => mode == Mode.Dc ? x[e.Br] : e.Geq * v + e.Jeq,
                _ => 0,
            };
        }
    }

    string SingularMessage()
    {
        var vs = _n.Elems.Count(e => e.Kind == ElemKind.V);
        return vs > 1
            ? L.T("O circuito não tem solução: há fontes de tensão em paralelo ou um laço só de fontes/fios (curto-circuito).")
            : L.T("O circuito não tem solução: verifique se há um curto-circuito direto na fonte ou um laço só de fios e fontes.");
    }

    // ───────────────────────── AC (phasors) ─────────────────────────

    /// <summary>Steady state at frequency <paramref name="f"/> (Hz). Sources are RMS phasors (Value ∠ PhaseDeg);
    /// diodes count as open.</summary>
    public bool SolveAc(double f)
    {
        double w = 2 * Math.PI * Math.Max(f, 0);
        int nN = _n.NodeCount, nb = 0;
        foreach (var e in _n.Elems)
        {
            bool branch = e.Kind is ElemKind.V or ElemKind.A || (e.Kind == ElemKind.L && w * e.Value < 1e-9);
            e.Br = branch ? nN + nb++ : -1;
        }
        int size = nN + nb;
        var A = new Complex[size, size];
        var b = new Complex[size];
        for (int i = 0; i < nN; i++) A[i, i] += Gmin;

        void Y(Elem e, Complex y)
        {
            int a = e.Na, c = e.Nb;
            if (a >= 0) A[a, a] += y;
            if (c >= 0) A[c, c] += y;
            if (a >= 0 && c >= 0) { A[a, c] -= y; A[c, a] -= y; }
        }
        void Branch(Elem e, Complex value)
        {
            int k = e.Br;
            if (e.Na >= 0) { A[e.Na, k] += 1; A[k, e.Na] += 1; }
            if (e.Nb >= 0) { A[e.Nb, k] -= 1; A[k, e.Nb] -= 1; }
            b[k] = value;
        }
        Complex Phasor(Elem e) => Complex.FromPolarCoordinates(e.Value, e.PhaseDeg * Math.PI / 180);
        Complex Adm(Elem e) => e.Kind switch
        {
            ElemKind.R => Gr(e),
            ElemKind.S => e.On ? Gswitch : 0,
            ElemKind.D => Goff,
            ElemKind.C => new Complex(0, w * e.Value),
            ElemKind.L when e.Br < 0 => 1 / new Complex(0, w * e.Value),
            _ => 0,
        };

        foreach (var e in _n.Elems)
        {
            switch (e.Kind)
            {
                case ElemKind.R: case ElemKind.S: case ElemKind.D: case ElemKind.C: Y(e, Adm(e)); break;
                case ElemKind.L: if (e.Br < 0) Y(e, Adm(e)); else Branch(e, 0); break;
                case ElemKind.V: Branch(e, Phasor(e)); break;
                case ElemKind.A: Branch(e, 0); break;
                case ElemKind.I:
                    var j = Phasor(e);
                    if (e.Na >= 0) b[e.Na] += j;
                    if (e.Nb >= 0) b[e.Nb] -= j;
                    break;
            }
        }
        if (!GaussC(A, b, out var x)) { Error = SingularMessage(); return false; }
        NodeVac = x.Take(nN).ToArray();
        Complex Nv(int i) => i < 0 ? Complex.Zero : x[i];
        foreach (var e in _n.Elems)
        {
            var v = Nv(e.Na) - Nv(e.Nb);
            e.Vac = v;
            e.Iac = e.Kind switch
            {
                ElemKind.V => -x[e.Br],
                ElemKind.A => x[e.Br],
                ElemKind.L when e.Br >= 0 => x[e.Br],
                ElemKind.I => Phasor(e),
                ElemKind.M or ElemKind.W => Complex.Zero,
                _ => v * Adm(e),
            };
        }
        return true;
    }

    // ───────────────────────── linear algebra ─────────────────────────

    /// <summary>Gaussian elimination with partial pivoting; false when the matrix is (numerically) singular.</summary>
    public static bool Gauss(double[,] A, double[] b, out double[] x)
    {
        int n = b.Length;
        x = new double[n];
        double max = 0;
        foreach (var a in A) max = Math.Max(max, Math.Abs(a));
        double tiny = Math.Max(max, 1e-30) * 1e-14;
        for (int c = 0; c < n; c++)
        {
            int p = c;
            for (int r = c + 1; r < n; r++) if (Math.Abs(A[r, c]) > Math.Abs(A[p, c])) p = r;
            if (Math.Abs(A[p, c]) < tiny) return false;
            if (p != c)
            {
                for (int k = 0; k < n; k++) (A[c, k], A[p, k]) = (A[p, k], A[c, k]);
                (b[c], b[p]) = (b[p], b[c]);
            }
            for (int r = c + 1; r < n; r++)
            {
                double f = A[r, c] / A[c, c];
                if (f == 0) continue;
                for (int k = c; k < n; k++) A[r, k] -= f * A[c, k];
                b[r] -= f * b[c];
            }
        }
        for (int r = n - 1; r >= 0; r--)
        {
            double s = b[r];
            for (int k = r + 1; k < n; k++) s -= A[r, k] * x[k];
            x[r] = s / A[r, r];
            if (double.IsNaN(x[r]) || Math.Abs(x[r]) > 1e12) return false;
        }
        return true;
    }

    public static bool GaussC(Complex[,] A, Complex[] b, out Complex[] x)
    {
        int n = b.Length;
        x = new Complex[n];
        double max = 0;
        foreach (var a in A) max = Math.Max(max, Complex.Abs(a));
        double tiny = Math.Max(max, 1e-30) * 1e-14;
        for (int c = 0; c < n; c++)
        {
            int p = c;
            for (int r = c + 1; r < n; r++) if (Complex.Abs(A[r, c]) > Complex.Abs(A[p, c])) p = r;
            if (Complex.Abs(A[p, c]) < tiny) return false;
            if (p != c)
            {
                for (int k = 0; k < n; k++) (A[c, k], A[p, k]) = (A[p, k], A[c, k]);
                (b[c], b[p]) = (b[p], b[c]);
            }
            for (int r = c + 1; r < n; r++)
            {
                var f = A[r, c] / A[c, c];
                if (f == Complex.Zero) continue;
                for (int k = c; k < n; k++) A[r, k] -= f * A[c, k];
                b[r] -= f * b[c];
            }
        }
        for (int r = n - 1; r >= 0; r--)
        {
            var s = b[r];
            for (int k = r + 1; k < n; k++) s -= A[r, k] * x[k];
            x[r] = s / A[r, r];
            if (double.IsNaN(x[r].Real) || double.IsNaN(x[r].Imaginary) || Complex.Abs(x[r]) > 1e12) return false;
        }
        return true;
    }
}
