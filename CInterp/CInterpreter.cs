using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace EasyCreator.CInterp;

/// <summary>A native (host) function: receives the evaluated arguments (with their C types).</summary>
public delegate CVal NativeFn(CInterpreter ip, CVal[] args);

/// <summary>A method of a native object (Serial, Servo, LiquidCrystal…); <paramref name="handle"/> identifies the instance.</summary>
public delegate CVal NativeMethod(CInterpreter ip, long handle, CVal[] args);

/// <summary>A library class the host exposes to C code (C++ style: <c>Servo s; s.attach(9);</c>).</summary>
public sealed class NativeClass
{
    public string Name = "";
    /// <summary>Creates an instance; receives constructor arguments (may be empty). Returns a non-zero handle.</summary>
    public Func<CInterpreter, CVal[], long> Create = (_, _) => 1;
    public readonly Dictionary<string, NativeMethod> Methods = new();
    CType? _type;
    public CType Type => _type ??= new CType(TK.Native) { N = this };
}

/// <summary>Thrown by a host to stop the program quietly (e.g. the simulation reached its end time).</summary>
public sealed class CHalt : Exception
{
    public CHalt(string reason = "") : base(reason) { }
}

/// <summary>exit() called by the program.</summary>
public sealed class CExit : Exception
{
    public int Code { get; }
    public CExit(int code) : base("exit") { Code = code; }
}

public enum Flow { Normal, Break, Continue, Return }

public readonly struct LVal
{
    public readonly long A;
    public readonly CType T;
    public LVal(long a, CType t) { A = a; T = t; }
}

/// <summary>
/// Tree-walking interpreter for a practical subset of C (plus the C++ bits Arduino sketches use:
/// library objects with methods). Never throws anything but <see cref="CError"/>, <see cref="CHalt"/> or
/// <see cref="CExit"/> out of <see cref="Compile"/>/<see cref="RunMain"/>/<see cref="Call"/> for program faults.
/// Run it on a thread with a large stack (see <see cref="RunOnBigStack{T}"/>).
/// </summary>
public sealed class CInterpreter
{
    // ---------- memory ----------
    public long[] Mem = new long[1 << 15];
    public long MemLimit = 8L << 20;       // cells
    public long GlobalTop = 1;             // cell 0 = NULL
    public long StackBase, StackLimit, SP, FP;
    public long StackCells = 1L << 20;
    public long HeapBase, HeapTop;
    readonly Dictionary<long, long> _heapBlocks = new();
    readonly List<(long addr, long cells)> _freeBlocks = new();

    // ---------- program ----------
    public readonly Dictionary<string, FuncDef> Funcs = new();
    public readonly List<Stmt> GlobalInits = new();
    public readonly Dictionary<string, NativeFn> Natives = new();
    public readonly Dictionary<string, NativeClass> Classes = new();
    public readonly Dictionary<string, Macro> Macros = new();
    internal readonly List<(string name, NativeClass cls)> NativeObjects = new();
    internal readonly Dictionary<string, long> StringPool = new();
    bool _compiled, _initialized;

    // ---------- I/O ----------
    public readonly StringBuilder Out = new();
    public int MaxOutput = 1 << 20;
    public string In = "";
    public int InPos;
    /// <summary>Called whenever the program writes to stdout (after appending to <see cref="Out"/>).</summary>
    public Action<string>? OnOutput;

    // ---------- execution control ----------
    public int Line;
    public long Steps;
    public long MaxSteps = 500_000_000;
    public TimeSpan Timeout = TimeSpan.FromSeconds(10);
    public CancellationToken Cancel;
    public int Depth, MaxDepth = 8000;
    /// <summary>Called every <see cref="PeriodicEvery"/> steps (the Arduino layer advances virtual time here).</summary>
    public Action? Periodic;
    public int PeriodicEvery = 64;
    long _nextCheck = 64;
    int _checkCount;
    readonly Stopwatch _clock = new();
    internal CVal RetVal;
    public uint RandState = 1;
    /// <summary>Free-form slot for the host (e.g. the Arduino board).</summary>
    public object? Host;

    public CInterpreter()
    {
        CStdLib.Register(this);
    }

    // ================= errors =================
    public CError RtErr(string msg) => new(Line, msg, true);
    public CError RtErr(int line, string msg) => new(line > 0 ? line : Line, msg, true);

    // ================= registration API (hooks) =================
    public void AddNative(string name, NativeFn fn) => Natives[name] = fn;

    public NativeClass AddClass(string name, Func<CInterpreter, CVal[], long> create)
    {
        var c = new NativeClass { Name = name, Create = create };
        Classes[name] = c;
        return c;
    }

    /// <summary>Declares a predefined global object such as <c>Serial</c>.</summary>
    public void AddNativeObject(string name, NativeClass cls) => NativeObjects.Add((name, cls));

    /// <summary>Defines an object-like macro (e.g. HIGH → 1).</summary>
    public void DefineMacro(string name, string body)
    {
        var m = new Macro { Name = name };
        CLexer.LexLine(body, 0, m.Body);
        Macros[name] = m;
    }

    public void DefineFunctionMacro(string name, string[] pars, string body)
    {
        var m = new Macro { Name = name, Params = pars.ToList() };
        CLexer.LexLine(body, 0, m.Body);
        Macros[name] = m;
    }

    // ================= compile / run =================
    /// <summary>Parses the program. Throws <see cref="CError"/> on syntax/semantic errors.</summary>
    public void Compile(string source)
    {
        if (_compiled) throw new InvalidOperationException("already compiled");
        _compiled = true;
        var macros = new Dictionary<string, Macro>(Macros);
        var toks = CLexer.Tokenize(source ?? "", macros);
        var parser = new CParser(toks, this);
        try { parser.ParseProgram(); }
        catch (InsufficientExecutionStackException) { throw new CError(Line, "o código tem blocos ou expressões aninhados demais", false); }
        StackBase = GlobalTop;
        StackLimit = StackBase + StackCells;
        SP = FP = StackBase;
        HeapBase = HeapTop = StackLimit;
        EnsureMem(StackBase + 4096);
    }

    /// <summary>Runs global initializers once (called automatically by <see cref="RunMain"/>/<see cref="Call"/>).</summary>
    public void InitGlobals()
    {
        if (_initialized) return;
        _initialized = true;
        _clock.Restart();
        foreach (var s in GlobalInits) s.Exec(this);
    }

    public bool HasFunction(string name) => Funcs.TryGetValue(name, out var f) && f.Body != null;

    /// <summary>Calls a user function by name (used by the Arduino layer for setup/loop).</summary>
    public CVal Call(string name, params CVal[] args)
    {
        InitGlobals();
        if (!Funcs.TryGetValue(name, out var f) || f.Body == null)
            throw new CError(0, "a função " + name + "() não foi encontrada", false);
        return CallFunc(f, args, f.Line);
    }

    /// <summary>Runs main(); returns its exit code.</summary>
    public int RunMain()
    {
        InitGlobals();
        if (!Funcs.TryGetValue("main", out var f) || f.Body == null)
            throw new CError(0, "o programa precisa de uma função main()", false);
        try
        {
            CVal[] args;
            if (f.Params.Count >= 2)
            {
                long name = InternString("programa");
                long argv = Alloc(2);
                Mem[argv] = name;
                Mem[argv + 1] = 0;
                args = new[] { CVal.Int(1), CVal.Of(CType.Char.Pointer.Pointer, argv) };
            }
            else args = Array.Empty<CVal>();
            var r = CallFunc(f, args, f.Line);
            return f.Ret.K == TK.Void ? 0 : (int)r.AsLong;
        }
        catch (CExit e) { return e.Code; }
    }

    /// <summary>Runs <paramref name="work"/> on a fresh thread with a big stack (deep recursion safe).</summary>
    public static T RunOnBigStack<T>(Func<T> work, int stackMb = 256)
    {
        T result = default!;
        Exception? error = null;
        var th = new Thread(() =>
        {
            try { result = work(); }
            catch (Exception e) { error = e; }
        }, stackMb * 1024 * 1024) { IsBackground = true, Name = "C interpreter" };
        th.Start();
        th.Join();
        if (error != null) throw new AggregateException(error);
        return result;
    }

    // ================= steps =================
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Step(int line)
    {
        Line = line;
        if (++Steps >= _nextCheck) SlowCheck();
    }

    void SlowCheck()
    {
        _nextCheck = Steps + PeriodicEvery;
        Periodic?.Invoke();
        if (++_checkCount % 64 != 0) return;
        if (Cancel.IsCancellationRequested) throw RtErr("execução cancelada");
        if (Steps > MaxSteps)
            throw RtErr("o programa executou passos demais (" + MaxSteps.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")) +
                        ") — provavelmente um laço infinito");
        if (_clock.IsRunning && _clock.Elapsed > Timeout)
            throw RtErr("o programa demorou demais (mais de " + Timeout.TotalSeconds.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) +
                        " s) — provavelmente um laço infinito");
    }

    // ================= memory =================
    public void EnsureMem(long top)
    {
        if (top <= Mem.Length) return;
        if (top > MemLimit) throw RtErr("memória esgotada (o programa usou memória demais)");
        long n = Math.Max(top, Math.Min(MemLimit, (long)Mem.Length * 2));
        Array.Resize(ref Mem, (int)n);
    }

    /// <summary>Allocates global (static) cells at compile time.</summary>
    internal long AllocGlobal(long cells)
    {
        long a = GlobalTop;
        if (cells > MemLimit / 2 || GlobalTop + cells > MemLimit / 2)
            throw new CError(Line, "variáveis globais grandes demais (o limite é de uns " + (MemLimit / 2 / 1_000_000) + " milhões de elementos)", false);
        GlobalTop += Math.Max(1, cells);
        EnsureMem(GlobalTop + 1);
        return a;
    }

    /// <summary>Allocates temporary stack cells (freed when the current block ends).</summary>
    public long Alloc(long cells)
    {
        long a = SP;
        SP += Math.Max(1, cells);
        if (SP > StackLimit) throw RtErr("estouro de pilha: variáveis locais grandes demais (use variáveis globais ou malloc)");
        EnsureMem(SP + 1);
        Array.Clear(Mem, (int)a, (int)(SP - a));
        return a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public long Read(long a)
    {
        if ((ulong)(a - 1) >= (ulong)(Mem.Length - 1)) BadAddr(a);
        return Mem[a];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(long a, long v)
    {
        if ((ulong)(a - 1) >= (ulong)(Mem.Length - 1)) BadAddr(a);
        Mem[a] = v;
    }

    void BadAddr(long a)
    {
        if (a == 0) throw RtErr("acesso a um ponteiro NULL (o ponteiro não aponta para nada)");
        throw RtErr("acesso a um endereço de memória inválido (ponteiro ou índice fora da área válida)");
    }

    public void CheckRange(long a, long cells)
    {
        if (cells <= 0) return;
        Read(a);
        Read(a + cells - 1);
    }

    public CVal Load(long a, CType t)
    {
        switch (t.K)
        {
            case TK.Array: return new CVal { T = t.Elem!.Pointer, L = a };
            case TK.Struct: Read(a); return new CVal { T = t, L = a };
            case TK.Float:
            case TK.Double: return new CVal { T = t, D = BitConverter.Int64BitsToDouble(Read(a)) };
            case TK.Void: throw RtErr("não é possível ler um valor do tipo void");
            default: return new CVal { T = t, L = Read(a) };
        }
    }

    public CVal Store(long a, CType t, CVal v)
    {
        switch (t.K)
        {
            case TK.Struct:
                if (v.T.K != TK.Struct || v.T.S != t.S)
                    throw RtErr("não é possível atribuir " + v.T + " a uma variável do tipo " + t);
                CheckRange(a, t.Cells);
                CheckRange(v.L, t.Cells);
                if (a != v.L) Array.Copy(Mem, v.L, Mem, a, t.Cells);
                return new CVal { T = t, L = a };
            case TK.Array:
                throw RtErr("não é possível atribuir um vetor inteiro de uma vez (copie elemento por elemento ou use strcpy)");
            case TK.Float:
            case TK.Double:
                {
                    var c = Conv.To(v, t);
                    Write(a, BitConverter.DoubleToInt64Bits(c.D));
                    return c;
                }
            case TK.Native:
                Write(a, v.L);
                return new CVal { T = t, L = v.L };
            case TK.Void:
                throw RtErr("não é possível guardar um valor void");
            default:
                {
                    if (!v.T.IsScalar)
                        throw RtErr("não é possível converter " + v.T + " para " + t);
                    var c = Conv.To(v, t);
                    Write(a, c.L);
                    return c;
                }
        }
    }

    // ---------- heap ----------
    public long Malloc(long bytes)
    {
        if (bytes < 0 || bytes > MemLimit) return 0;
        long cells = Math.Max(1, bytes);
        for (int i = 0; i < _freeBlocks.Count; i++)
        {
            var (addr, size) = _freeBlocks[i];
            if (size >= cells)
            {
                _freeBlocks.RemoveAt(i);
                if (size > cells) _freeBlocks.Add((addr + cells, size - cells));
                _heapBlocks[addr] = cells;
                Array.Clear(Mem, (int)addr, (int)cells);
                return addr;
            }
        }
        if (HeapTop + cells > MemLimit) return 0;
        long a = HeapTop;
        HeapTop += cells;
        EnsureMem(HeapTop + 1);
        Array.Clear(Mem, (int)a, (int)cells);
        _heapBlocks[a] = cells;
        return a;
    }

    public void Free(long a)
    {
        if (a == 0) return;
        if (!_heapBlocks.Remove(a, out long cells))
            throw RtErr("free() recebeu um ponteiro que não veio de malloc (ou que já foi liberado)");
        _freeBlocks.Add((a, cells));
    }

    public long BlockSize(long a) => _heapBlocks.TryGetValue(a, out var c) ? c : -1;

    // ---------- strings ----------
    internal long InternString(string s)
    {
        if (StringPool.TryGetValue(s, out var a)) return a;
        a = AllocGlobal(s.Length + 1);
        for (int i = 0; i < s.Length; i++) Mem[a + i] = (sbyte)(byte)(s[i] > 255 ? '?' : s[i]);
        Mem[a + s.Length] = 0;
        StringPool[s] = a;
        return a;
    }

    public string ReadCString(long a, int max = 1 << 20)
    {
        if (a == 0) throw RtErr("texto (char*) é NULL");
        var sb = new StringBuilder();
        for (int i = 0; i < max; i++)
        {
            long c = Read(a + i);
            if (c == 0) break;
            sb.Append((char)(byte)c);
        }
        return sb.ToString();
    }

    public void WriteCString(long a, string s)
    {
        CheckRange(a, s.Length + 1);
        for (int i = 0; i < s.Length; i++) Mem[a + i] = (sbyte)(byte)(s[i] > 255 ? '?' : s[i]);
        Mem[a + s.Length] = 0;
    }

    /// <summary>Text of a value for printing (char* → string, char → char, numbers → decimal).</summary>
    public string ValueText(CVal v)
    {
        if (v.T.IsPtr && v.T.Elem!.K == TK.Char) return ReadCString(v.L);
        if (v.T.K == TK.Char) return ((char)(byte)v.L).ToString();
        return v.ToString();
    }

    public void Print(string s)
    {
        if (Out.Length + s.Length > MaxOutput) throw RtErr("o programa escreveu texto demais na saída");
        Out.Append(s);
        OnOutput?.Invoke(s);
    }

    // ================= calls =================
    internal CVal CallFunc(FuncDef f, CVal[] args, int line)
    {
        if (f.Body == null) throw RtErr(line, "a função " + f.Name + "() foi declarada mas não foi escrita (falta o corpo { })");
        if (args.Length != f.Params.Count && !(f.Params.Count == 0 && f.OldStyle))
            throw RtErr(line, "a função " + f.Name + "() espera " + f.Params.Count + " argumento(s), mas recebeu " + args.Length);
        Step(line);
        if (!RuntimeHelpers.TryEnsureSufficientExecutionStack())
        {
            Depth = 0;
            throw RtErr(line, "recursão profunda demais (estouro de pilha) — a função " + f.Name + "() chama a si mesma sem parar?");
        }
        if (++Depth > MaxDepth)
        {
            Depth = 0;
            throw RtErr(line, "recursão profunda demais (estouro de pilha) — a função " + f.Name + "() chama a si mesma sem parar?");
        }
        long savedFP = FP, savedSP = SP;
        int savedLine = Line;
        FP = SP;
        SP += Math.Max(1, f.FrameCells);
        if (SP > StackLimit) throw RtErr(line, "estouro de pilha: variáveis locais grandes demais ou recursão profunda");
        EnsureMem(SP + 1);
        Array.Clear(Mem, (int)FP, (int)(SP - FP));
        for (int i = 0; i < f.Params.Count && i < args.Length; i++)
        {
            var p = f.Params[i];
            Store(FP + p.Offset, p.T, args[i]);
        }
        RetVal = CVal.VoidV;
        f.Body.Exec(this);
        CVal ret = RetVal;
        long[]? structCopy = null;
        if (f.Ret.K == TK.Void) ret = CVal.VoidV;
        else if (f.Ret.K == TK.Struct)
        {
            if (ret.T == null || ret.T.K != TK.Struct || ret.T.S != f.Ret.S)
                throw RtErr(line, "a função " + f.Name + "() deveria retornar " + f.Ret);
            structCopy = new long[f.Ret.Cells];
            Array.Copy(Mem, ret.L, structCopy, 0, structCopy.Length);
        }
        else if (ret.T == null || ret.T.K == TK.Void) ret = Conv.To(CVal.Int(0), f.Ret);
        else
        {
            if (!ret.T.IsScalar) throw RtErr(line, "a função " + f.Name + "() retornou um valor de tipo errado (" + ret.T + ")");
            ret = Conv.To(ret, f.Ret);
        }
        FP = savedFP;
        SP = savedSP;
        Depth--;
        Line = savedLine;
        if (structCopy != null)
        {
            long t = Alloc(structCopy.Length);
            Array.Copy(structCopy, 0, Mem, t, structCopy.Length);
            ret = new CVal { T = f.Ret, L = t };
        }
        return ret;
    }

    // ================= operators =================
    public CVal BinOp(BinKind op, CVal a, CVal b, int line)
    {
        if (a.T.K == TK.Void || b.T.K == TK.Void) throw RtErr(line, "não é possível usar um valor void em uma conta");
        if (!a.T.IsScalar || !b.T.IsScalar)
            throw RtErr(line, "operador " + BinText(op) + " não funciona com " + (!a.T.IsScalar ? a.T : b.T));

        // pointer arithmetic / comparison
        if (a.T.IsPtr || b.T.IsPtr)
        {
            switch (op)
            {
                case BinKind.Add:
                    if (a.T.IsPtr && b.T.IsInteger) return new CVal { T = a.T, L = a.L + b.L * ElemCells(a.T, line) };
                    if (b.T.IsPtr && a.T.IsInteger) return new CVal { T = b.T, L = b.L + a.L * ElemCells(b.T, line) };
                    throw RtErr(line, "não é possível somar dois ponteiros");
                case BinKind.Sub:
                    if (a.T.IsPtr && b.T.IsInteger) return new CVal { T = a.T, L = a.L - b.L * ElemCells(a.T, line) };
                    if (a.T.IsPtr && b.T.IsPtr) return new CVal { T = CType.Long, L = (a.L - b.L) / Math.Max(1, ElemCells(a.T, line)) };
                    throw RtErr(line, "subtração inválida com ponteiro");
                case BinKind.Eq: return CVal.BoolV(a.L == b.L);
                case BinKind.Ne: return CVal.BoolV(a.L != b.L);
                case BinKind.Lt: return CVal.BoolV(a.L < b.L);
                case BinKind.Le: return CVal.BoolV(a.L <= b.L);
                case BinKind.Gt: return CVal.BoolV(a.L > b.L);
                case BinKind.Ge: return CVal.BoolV(a.L >= b.L);
                default: throw RtErr(line, "operador " + BinText(op) + " não funciona com ponteiros");
            }
        }

        if (op == BinKind.Shl || op == BinKind.Shr)
        {
            if (a.T.IsFloat || b.T.IsFloat) throw RtErr(line, "operadores << e >> só funcionam com inteiros");
            var rt = a.T.Promoted;
            int n = (int)(b.L & 63);
            long r;
            if (op == BinKind.Shl) r = a.L << n;
            else if (rt.Unsigned) r = (long)((rt.K == TK.Int ? (ulong)(uint)a.L : (ulong)a.L) >> n);
            else r = a.L >> n;
            return new CVal { T = rt, L = Conv.Norm(r, rt) };
        }

        var t = CType.Common(a.T, b.T);
        if (t.IsFloat)
        {
            double x = a.AsDouble, y = b.AsDouble, r;
            switch (op)
            {
                case BinKind.Add: r = x + y; break;
                case BinKind.Sub: r = x - y; break;
                case BinKind.Mul: r = x * y; break;
                case BinKind.Div: r = x / y; break;
                case BinKind.Eq: return CVal.BoolV(x == y);
                case BinKind.Ne: return CVal.BoolV(x != y);
                case BinKind.Lt: return CVal.BoolV(x < y);
                case BinKind.Le: return CVal.BoolV(x <= y);
                case BinKind.Gt: return CVal.BoolV(x > y);
                case BinKind.Ge: return CVal.BoolV(x >= y);
                case BinKind.Mod: throw RtErr(line, "o operador % só funciona com inteiros (para float use fmod)");
                default: throw RtErr(line, "o operador " + BinText(op) + " só funciona com inteiros");
            }
            if (t.K == TK.Float) r = (float)r;
            return new CVal { T = t, D = r };
        }
        else
        {
            long x = Conv.Norm(a.L, t), y = Conv.Norm(b.L, t);
            bool u = t.Unsigned;
            long r;
            switch (op)
            {
                case BinKind.Add: r = unchecked(x + y); break;
                case BinKind.Sub: r = unchecked(x - y); break;
                case BinKind.Mul: r = unchecked(x * y); break;
                case BinKind.Div:
                    if (y == 0) throw RtErr(line, "divisão por zero");
                    if (u) r = (long)((ulong)x / (ulong)y);
                    else r = y == -1 ? unchecked(-x) : x / y;
                    break;
                case BinKind.Mod:
                    if (y == 0) throw RtErr(line, "resto da divisão (%) por zero");
                    if (u) r = (long)((ulong)x % (ulong)y);
                    else r = y == -1 ? 0 : x % y;
                    break;
                case BinKind.And: r = x & y; break;
                case BinKind.Or: r = x | y; break;
                case BinKind.Xor: r = x ^ y; break;
                case BinKind.Eq: return CVal.BoolV(x == y);
                case BinKind.Ne: return CVal.BoolV(x != y);
                case BinKind.Lt: return CVal.BoolV(u ? (ulong)x < (ulong)y : x < y);
                case BinKind.Le: return CVal.BoolV(u ? (ulong)x <= (ulong)y : x <= y);
                case BinKind.Gt: return CVal.BoolV(u ? (ulong)x > (ulong)y : x > y);
                case BinKind.Ge: return CVal.BoolV(u ? (ulong)x >= (ulong)y : x >= y);
                default: throw RtErr(line, "operador desconhecido");
            }
            return new CVal { T = t, L = Conv.Norm(r, t) };
        }
    }

    long ElemCells(CType ptr, int line)
    {
        var e = ptr.Elem!;
        if (e.K == TK.Void) return 1;
        return e.Cells;
    }

    public static string BinText(BinKind k) => k switch
    {
        BinKind.Add => "+", BinKind.Sub => "-", BinKind.Mul => "*", BinKind.Div => "/", BinKind.Mod => "%",
        BinKind.Shl => "<<", BinKind.Shr => ">>", BinKind.And => "&", BinKind.Or => "|", BinKind.Xor => "^",
        BinKind.Eq => "==", BinKind.Ne => "!=", BinKind.Lt => "<", BinKind.Le => "<=", BinKind.Gt => ">", BinKind.Ge => ">=",
        _ => "?"
    };

    public int NextRand()
    {
        RandState = RandState * 1103515245 + 12345;
        return (int)((RandState / 65536) % 32768);
    }
}
