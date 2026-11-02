namespace EasyCreator.CInterp;

public enum BinKind { Add, Sub, Mul, Div, Mod, Shl, Shr, And, Or, Xor, Eq, Ne, Lt, Le, Gt, Ge, None }

// ======================= symbols =======================

public abstract class Sym { public string Name = ""; public int Line; }

public sealed class VarSym : Sym
{
    public CType T = CType.Int;
    public bool Global;
    public long Addr;        // global address
    public long Offset;      // local frame offset
    public bool IsConst;
    public long? ConstValue; // known compile-time value (const int with constant initializer)
    // variable-length array (local): frame cells = [data address, dim0, dim1, ...]
    public List<Expr?>? VlaDims;
    public CType? VlaBase;
    Dictionary<(long, long, long), CType>? _vlaCache;

    public CType VlaType(CInterpreter ip, long baseAddr)
    {
        int n = VlaDims!.Count;
        long d0 = ip.Mem[baseAddr + 1], d1 = n > 1 ? ip.Mem[baseAddr + 2] : 0, d2 = n > 2 ? ip.Mem[baseAddr + 3] : 0;
        _vlaCache ??= new();
        if (_vlaCache.TryGetValue((d0, d1, d2), out var t)) return t;
        t = VlaBase!;
        for (int i = n - 1; i >= 0; i--) t = CType.ArrayOf(t, ip.Mem[baseAddr + 1 + i]);
        _vlaCache[(d0, d1, d2)] = t;
        return t;
    }
}

public sealed class EnumSym : Sym { public long Value; }
public sealed class TypedefSym : Sym { public CType T = CType.Int; public bool FromTag; }
public sealed class FuncSym : Sym { public FuncDef F = null!; }

public sealed class FuncDef
{
    public string Name = "";
    public CType Ret = CType.Int;
    public List<VarSym> Params = new();
    public Block? Body;
    public long FrameCells;
    public int Line;
    public bool OldStyle;   // declared as f() — any arguments accepted at link time
}

// ======================= expressions =======================

public abstract class Expr
{
    public int Line;
    public abstract CVal Eval(CInterpreter ip);
    public virtual LVal Addr(CInterpreter ip) =>
        throw ip.RtErr(Line, "esta expressão não é uma variável (não pode receber valor nem ter seu endereço pego)");
    public virtual CType TypeOf(CInterpreter ip) => Eval(ip).T;
    public virtual bool IsLValue => false;
}

public sealed class ConstExpr : Expr
{
    public CVal V;
    public override CVal Eval(CInterpreter ip) => V;
    public override CType TypeOf(CInterpreter ip) => V.T;
}

public sealed class StrLit : Expr
{
    public long A;
    public CType T = CType.Char;
    public string Text = "";
    public override CVal Eval(CInterpreter ip) => new() { T = CType.Char.Pointer, L = A };
    public override LVal Addr(CInterpreter ip) => new(A, T);
    public override CType TypeOf(CInterpreter ip) => T;
    public override bool IsLValue => true;
}

public sealed class IdentExpr : Expr
{
    public VarSym S = null!;

    public override LVal Addr(CInterpreter ip)
    {
        if (S.Global) return new LVal(S.Addr, S.T);
        long a = ip.FP + S.Offset;
        if (S.VlaDims != null) return new LVal(ip.Mem[a], S.VlaType(ip, a));
        return new LVal(a, S.T);
    }

    public override CVal Eval(CInterpreter ip)
    {
        var lv = Addr(ip);
        return ip.Load(lv.A, lv.T);
    }

    public override CType TypeOf(CInterpreter ip) => S.VlaDims != null ? Addr(ip).T : S.T;
    public override bool IsLValue => true;
}

public enum UnKind { Neg, Plus, Not, BitNot }

public sealed class UnaryExpr : Expr
{
    public UnKind Op;
    public Expr E = null!;

    public override CVal Eval(CInterpreter ip)
    {
        var v = E.Eval(ip);
        switch (Op)
        {
            case UnKind.Not:
                if (!v.T.IsScalar) throw ip.RtErr(Line, "operador ! não funciona com " + v.T);
                return CVal.BoolV(!v.IsTrue);
            case UnKind.Neg:
            case UnKind.Plus:
                if (!v.T.IsArith) throw ip.RtErr(Line, "sinal " + (Op == UnKind.Neg ? "-" : "+") + " só funciona com números");
                if (v.T.IsFloat) return new CVal { T = v.T, D = Op == UnKind.Neg ? -v.D : v.D };
                {
                    var t = v.T.Promoted;
                    return new CVal { T = t, L = Conv.Norm(Op == UnKind.Neg ? unchecked(-v.L) : v.L, t) };
                }
            default:
                if (!v.T.IsInteger) throw ip.RtErr(Line, "operador ~ só funciona com inteiros");
                {
                    var t = v.T.Promoted;
                    return new CVal { T = t, L = Conv.Norm(~v.L, t) };
                }
        }
    }
}

public sealed class DerefExpr : Expr
{
    public Expr E = null!;

    public override LVal Addr(CInterpreter ip)
    {
        var v = E.Eval(ip);
        if (!v.T.IsPtr) throw ip.RtErr(Line, "o operador * só pode ser usado com ponteiros (aqui foi usado com " + v.T + ")");
        if (v.T.Elem!.K == TK.Void) throw ip.RtErr(Line, "não é possível usar * em um ponteiro void* (converta antes, ex.: (int*)p)");
        if (v.L == 0) throw ip.RtErr(Line, "acesso a um ponteiro NULL (o ponteiro não aponta para nada)");
        return new LVal(v.L, v.T.Elem);
    }

    public override CVal Eval(CInterpreter ip)
    {
        var lv = Addr(ip);
        return ip.Load(lv.A, lv.T);
    }

    public override CType TypeOf(CInterpreter ip)
    {
        var t = E.TypeOf(ip);
        if (t.K == TK.Array || t.K == TK.Ptr) return t.Elem!;
        throw ip.RtErr(Line, "o operador * só pode ser usado com ponteiros");
    }

    public override bool IsLValue => true;
}

public sealed class AddrOfExpr : Expr
{
    public Expr E = null!;

    public override CVal Eval(CInterpreter ip)
    {
        var lv = E.Addr(ip);
        return new CVal { T = lv.T.Pointer, L = lv.A };
    }

    public override CType TypeOf(CInterpreter ip) => E.TypeOf(ip).Pointer;
}

public sealed class IncDecExpr : Expr
{
    public Expr E = null!;
    public int Delta;     // +1 / -1
    public bool Prefix;

    public override CVal Eval(CInterpreter ip)
    {
        var lv = E.Addr(ip);
        var cur = ip.Load(lv.A, lv.T);
        CVal nv;
        if (lv.T.IsPtr) nv = new CVal { T = lv.T, L = cur.L + Delta * Math.Max(1, lv.T.Elem!.K == TK.Void ? 1 : lv.T.Elem.Cells) };
        else if (lv.T.IsFloat) nv = new CVal { T = lv.T, D = cur.D + Delta };
        else if (lv.T.IsInteger) nv = new CVal { T = lv.T, L = cur.L + Delta };
        else throw ip.RtErr(Line, (Delta > 0 ? "++" : "--") + " não funciona com " + lv.T);
        nv = ip.Store(lv.A, lv.T, nv);
        return Prefix ? nv : cur;
    }
}

public sealed class SizeofExpr : Expr
{
    public Expr? E;
    public CType? T;
    public override CVal Eval(CInterpreter ip) => CVal.Of(CType.ULong, (T ?? E!.TypeOf(ip)).Bytes);
}

public sealed class CastExpr : Expr
{
    public CType T = CType.Int;
    public Expr E = null!;

    public override CVal Eval(CInterpreter ip)
    {
        var v = E.Eval(ip);
        if (T.K == TK.Void) return CVal.VoidV;
        if (T.K == TK.Struct || T.K == TK.Array) throw ip.RtErr(Line, "conversão (cast) para " + T + " não é permitida");
        if (!v.T.IsScalar) throw ip.RtErr(Line, "não é possível converter " + v.T + " para " + T);
        return Conv.To(v, T);
    }

    public override CType TypeOf(CInterpreter ip) => T;
}

public sealed class BinExpr : Expr
{
    public BinKind Op;
    public Expr A = null!, B = null!;
    public override CVal Eval(CInterpreter ip) => ip.BinOp(Op, A.Eval(ip), B.Eval(ip), Line);
}

public sealed class LogicExpr : Expr
{
    public bool IsAnd;
    public Expr A = null!, B = null!;

    public override CVal Eval(CInterpreter ip)
    {
        bool a = A.Eval(ip).IsTrue;
        if (IsAnd) return CVal.BoolV(a && B.Eval(ip).IsTrue);
        return CVal.BoolV(a || B.Eval(ip).IsTrue);
    }
}

public sealed class CondExpr : Expr
{
    public Expr C = null!, A = null!, B = null!;

    public override CVal Eval(CInterpreter ip)
    {
        bool c = C.Eval(ip).IsTrue;
        var v = c ? A.Eval(ip) : B.Eval(ip);
        // result type: common arithmetic type of both branches (when cheap to know)
        if (v.T.IsArith)
        {
            var other = (c ? B : A) is ConstExpr k ? k.V.T : null;
            if (other != null && other.IsArith) return Conv.To(v, CType.Common(v.T, other));
        }
        return v;
    }
}

public sealed class CommaExpr : Expr
{
    public Expr A = null!, B = null!;
    public override CVal Eval(CInterpreter ip) { A.Eval(ip); return B.Eval(ip); }
}

public sealed class AssignExpr : Expr
{
    public BinKind Op = BinKind.None;
    public Expr L = null!, R = null!;

    public override CVal Eval(CInterpreter ip)
    {
        var lv = L.Addr(ip);
        var v = R.Eval(ip);
        if (Op != BinKind.None) v = ip.BinOp(Op, ip.Load(lv.A, lv.T), v, Line);
        if (lv.T.K == TK.Array)
            throw ip.RtErr(Line, "não é possível atribuir a um vetor inteiro (para textos use strcpy; para números copie elemento por elemento)");
        if (lv.T.IsScalar && !v.T.IsScalar)
            throw ip.RtErr(Line, "não é possível guardar " + v.T + " em uma variável " + lv.T);
        return ip.Store(lv.A, lv.T, v);
    }
}

public sealed class IndexExpr : Expr
{
    public Expr Base = null!, Index = null!;

    public override LVal Addr(CInterpreter ip)
    {
        long baseAddr;
        CType elem;
        if (Base.IsLValue)
        {
            var b = Base.Addr(ip);
            if (b.T.K == TK.Array)
            {
                var iv = Index.Eval(ip);
                if (!iv.T.IsInteger) throw ip.RtErr(Line, "o índice de um vetor precisa ser inteiro");
                long i = iv.L;
                if (b.T.Len >= 0 && (i < 0 || i >= b.T.Len))
                    throw ip.RtErr(Line, "índice " + i + " fora dos limites do vetor (tamanho " + b.T.Len + ", índices válidos de 0 a " + (b.T.Len - 1) + ")");
                elem = b.T.Elem!;
                return new LVal(b.A + i * elem.Cells, elem);
            }
            if (b.T.K != TK.Ptr) throw ip.RtErr(Line, "[ ] só pode ser usado com vetores ou ponteiros");
            baseAddr = ip.Read(b.A);
            elem = b.T.Elem!;
        }
        else
        {
            var v = Base.Eval(ip);
            if (!v.T.IsPtr) throw ip.RtErr(Line, "[ ] só pode ser usado com vetores ou ponteiros");
            baseAddr = v.L;
            elem = v.T.Elem!;
        }
        var ix = Index.Eval(ip);
        if (!ix.T.IsInteger) throw ip.RtErr(Line, "o índice de um vetor precisa ser inteiro");
        if (baseAddr == 0) throw ip.RtErr(Line, "acesso a um ponteiro NULL (o ponteiro não aponta para nada)");
        if (elem.K == TK.Void) throw ip.RtErr(Line, "não é possível indexar um ponteiro void*");
        return new LVal(baseAddr + ix.L * elem.Cells, elem);
    }

    public override CVal Eval(CInterpreter ip)
    {
        var lv = Addr(ip);
        return ip.Load(lv.A, lv.T);
    }

    public override CType TypeOf(CInterpreter ip)
    {
        var t = Base.TypeOf(ip);
        if (t.K == TK.Array || t.K == TK.Ptr) return t.Elem!;
        throw ip.RtErr(Line, "[ ] só pode ser usado com vetores ou ponteiros");
    }

    public override bool IsLValue => true;
}

public sealed class MemberExpr : Expr
{
    public Expr Base = null!;
    public string Name = "";
    public bool Arrow;
    StructDef? _s;
    Field? _f;

    Field FieldOf(CInterpreter ip, CType t)
    {
        if (t.K != TK.Struct)
        {
            if (t.K == TK.Native) throw ip.RtErr(Line, t.N!.Name + "." + Name + " não existe (talvez faltem os parênteses: " + Name + "(...))");
            throw ip.RtErr(Line, (Arrow ? "->" : ".") + Name + ": isto não é uma struct" + (Arrow ? "" : " (se for ponteiro, use ->)"));
        }
        if (_s == t.S) return _f!;
        if (!t.S!.ByName.TryGetValue(Name, out var f))
            throw ip.RtErr(Line, "a struct " + t.S.Name + " não tem um campo chamado '" + Name + "'");
        _s = t.S;
        _f = f;
        return f;
    }

    public override LVal Addr(CInterpreter ip)
    {
        var v = Base.Eval(ip);
        if (Arrow)
        {
            if (!v.T.IsPtr) throw ip.RtErr(Line, "-> só pode ser usado com ponteiros para struct (para variáveis use .)");
            var f = FieldOf(ip, v.T.Elem!);
            if (v.L == 0) throw ip.RtErr(Line, "acesso a um ponteiro NULL com ->");
            return new LVal(v.L + f.Offset, f.T);
        }
        else
        {
            if (v.T.IsPtr && v.T.Elem!.K == TK.Struct) throw ip.RtErr(Line, "'" + Name + "': isto é um ponteiro para struct, use -> em vez de .");
            var f = FieldOf(ip, v.T);
            return new LVal(v.L + f.Offset, f.T);
        }
    }

    public override CVal Eval(CInterpreter ip)
    {
        var lv = Addr(ip);
        return ip.Load(lv.A, lv.T);
    }

    public override CType TypeOf(CInterpreter ip)
    {
        var t = Base.TypeOf(ip);
        if (Arrow) t = t.Elem ?? t;
        return FieldOf(ip, t).T;
    }

    public override bool IsLValue => true;
}

public sealed class CallExpr : Expr
{
    public string Name = "";
    public Expr[] Args = Array.Empty<Expr>();
    public FuncDef? F;
    public NativeFn? N;

    public override CVal Eval(CInterpreter ip)
    {
        var args = new CVal[Args.Length];
        for (int i = 0; i < args.Length; i++) args[i] = Args[i].Eval(ip);
        if (F != null) return ip.CallFunc(F, args, Line);
        ip.Line = Line;
        var r = N!(ip, args);
        ip.Line = Line;
        return r.T == null ? CVal.VoidV : r;
    }
}

public sealed class MethodCallExpr : Expr
{
    public Expr Obj = null!;
    public string Name = "";
    public Expr[] Args = Array.Empty<Expr>();

    public override CVal Eval(CInterpreter ip)
    {
        var lv = Obj.Addr(ip);
        if (lv.T.K != TK.Native)
            throw ip.RtErr(Line, "." + Name + "(...): só objetos de bibliotecas (como Serial, Servo, LiquidCrystal) têm funções com ponto");
        var cls = lv.T.N!;
        if (!cls.Methods.TryGetValue(Name, out var m))
            throw ip.RtErr(Line, "o objeto " + cls.Name + " não tem a função " + Name + "()");
        long h = ip.Read(lv.A);
        if (h == 0)
        {
            h = cls.Create(ip, Array.Empty<CVal>());
            ip.Write(lv.A, h);
        }
        var args = new CVal[Args.Length];
        for (int i = 0; i < args.Length; i++) args[i] = Args[i].Eval(ip);
        ip.Line = Line;
        var r = m(ip, h, args);
        ip.Line = Line;
        return r.T == null ? CVal.VoidV : r;
    }
}

/// <summary>{ a, b, .x = c, [2] = d } — only valid as an initializer.</summary>
public sealed class InitList : Expr
{
    public List<InitItem> Items = new();
    public override CVal Eval(CInterpreter ip) => throw ip.RtErr(Line, "uma lista { ... } só pode ser usada para inicializar uma variável");
}

public sealed class InitItem
{
    public string? Field;
    public Expr? Index;
    public Expr Value = null!;
}

// ======================= statements =======================

public abstract class Stmt
{
    public int Line;
    public abstract Flow Exec(CInterpreter ip);
}

public sealed class ExprStmt : Stmt
{
    public Expr E = null!;
    public override Flow Exec(CInterpreter ip) { ip.Step(Line); E.Eval(ip); return Flow.Normal; }
}

public sealed class EmptyStmt : Stmt
{
    public override Flow Exec(CInterpreter ip) => Flow.Normal;
}

public sealed class Block : Stmt
{
    public Stmt[] S = Array.Empty<Stmt>();

    public override Flow Exec(CInterpreter ip)
    {
        long sp = ip.SP;
        var s = S;
        for (int i = 0; i < s.Length; i++)
        {
            var f = s[i].Exec(ip);
            if (f != Flow.Normal) { ip.SP = sp; return f; }
        }
        ip.SP = sp;
        return Flow.Normal;
    }
}

public sealed class IfStmt : Stmt
{
    public Expr C = null!;
    public Stmt Then = null!;
    public Stmt? Else;

    public override Flow Exec(CInterpreter ip)
    {
        ip.Step(Line);
        if (C.Eval(ip).IsTrue) return Then.Exec(ip);
        return Else?.Exec(ip) ?? Flow.Normal;
    }
}

public sealed class WhileStmt : Stmt
{
    public Expr C = null!;
    public Stmt Body = null!;

    public override Flow Exec(CInterpreter ip)
    {
        while (true)
        {
            ip.Step(Line);
            if (!C.Eval(ip).IsTrue) return Flow.Normal;
            var f = Body.Exec(ip);
            if (f == Flow.Break) return Flow.Normal;
            if (f == Flow.Return) return f;
        }
    }
}

public sealed class DoStmt : Stmt
{
    public Expr C = null!;
    public Stmt Body = null!;

    public override Flow Exec(CInterpreter ip)
    {
        while (true)
        {
            ip.Step(Line);
            var f = Body.Exec(ip);
            if (f == Flow.Break) return Flow.Normal;
            if (f == Flow.Return) return f;
            if (!C.Eval(ip).IsTrue) return Flow.Normal;
        }
    }
}

public sealed class ForStmt : Stmt
{
    public Stmt? Init;
    public Expr? C, Next;
    public Stmt Body = null!;

    public override Flow Exec(CInterpreter ip)
    {
        long sp = ip.SP;
        Init?.Exec(ip);
        while (true)
        {
            ip.Step(Line);
            if (C != null && !C.Eval(ip).IsTrue) break;
            var f = Body.Exec(ip);
            if (f == Flow.Break) break;
            if (f == Flow.Return) { ip.SP = sp; return f; }
            Next?.Eval(ip);
        }
        ip.SP = sp;
        return Flow.Normal;
    }
}

public sealed class CaseLabel : Stmt
{
    public Expr? Value;   // null = default
    public override Flow Exec(CInterpreter ip) => Flow.Normal;
}

public sealed class SwitchStmt : Stmt
{
    public Expr E = null!;
    public Block Body = null!;
    Dictionary<long, int>? _table;
    int _default = -1;

    public override Flow Exec(CInterpreter ip)
    {
        ip.Step(Line);
        var v = E.Eval(ip);
        if (!v.T.IsInteger) throw ip.RtErr(Line, "switch só funciona com valores inteiros (int, char...)");
        if (_table == null)
        {
            var t = new Dictionary<long, int>();
            for (int i = 0; i < Body.S.Length; i++)
            {
                if (Body.S[i] is not CaseLabel c) continue;
                if (c.Value == null) { _default = i; continue; }
                var cv = c.Value.Eval(ip);
                if (!cv.T.IsInteger) throw ip.RtErr(c.Line, "o valor de um case precisa ser inteiro constante");
                if (!t.TryAdd(cv.L, i)) throw ip.RtErr(c.Line, "case " + cv.L + " repetido");
            }
            _table = t;
        }
        if (!_table.TryGetValue(v.L, out int start)) start = _default;
        if (start < 0) return Flow.Normal;
        long sp = ip.SP;
        for (int i = start; i < Body.S.Length; i++)
        {
            var f = Body.S[i].Exec(ip);
            if (f == Flow.Break) break;
            if (f != Flow.Normal) { ip.SP = sp; return f; }
        }
        ip.SP = sp;
        return Flow.Normal;
    }
}

public sealed class JumpStmt : Stmt
{
    public Flow Kind;
    public override Flow Exec(CInterpreter ip) { ip.Step(Line); return Kind; }
}

public sealed class ReturnStmt : Stmt
{
    public Expr? E;

    public override Flow Exec(CInterpreter ip)
    {
        ip.Step(Line);
        ip.RetVal = E != null ? E.Eval(ip) : CVal.VoidV;
        return Flow.Return;
    }
}

/// <summary>Declaration of one variable (local or global initializer).</summary>
public sealed class DeclStmt : Stmt
{
    public VarSym V = null!;
    public Expr? Init;
    public Expr[]? CtorArgs;   // native objects: Servo s(…) / LiquidCrystal lcd(…)

    public override Flow Exec(CInterpreter ip)
    {
        ip.Step(Line);
        long a;
        CType t;
        if (V.VlaDims != null)
        {
            long slot = ip.FP + V.Offset;
            long cells = V.VlaBase!.Cells;
            for (int i = 0; i < V.VlaDims.Count; i++)
            {
                var d = V.VlaDims[i]!.Eval(ip);
                if (!d.T.IsInteger || d.L <= 0) throw ip.RtErr(Line, "tamanho de vetor inválido (" + d + "): precisa ser inteiro maior que zero");
                ip.Mem[slot + 1 + i] = d.L;
                cells *= d.L;
                if (cells > ip.StackCells) throw ip.RtErr(Line, "vetor local grande demais (use malloc ou uma variável global)");
            }
            a = ip.Alloc(cells);
            ip.Mem[slot] = a;
            t = V.VlaType(ip, slot);
            if (Init != null) throw ip.RtErr(Line, "vetores de tamanho variável não podem ter inicializador");
            return Flow.Normal;
        }
        a = V.Global ? V.Addr : ip.FP + V.Offset;
        t = V.T;
        if (t.K == TK.Native)
        {
            var args = new CVal[CtorArgs?.Length ?? 0];
            for (int i = 0; i < args.Length; i++) args[i] = CtorArgs![i].Eval(ip);
            ip.Line = Line;
            ip.Write(a, t.N!.Create(ip, args));
            return Flow.Normal;
        }
        if (Init != null) Initializer.Init(ip, a, t, Init, Line);
        else if (!V.Global) ip.FillGarbage(a, t);
        return Flow.Normal;
    }
}

public static class Initializer
{
    public static void Init(CInterpreter ip, long a, CType t, Expr init, int line)
    {
        if (t.IsAggregate)
        {
            Array.Clear(ip.Mem, (int)a, (int)t.Cells);
            if (init is InitList il) { int pos = 0; Fill(ip, a, t, il.Items, ref pos, line, true); return; }
            if (init is StrLit s && t.K == TK.Array && t.Elem!.K == TK.Char) { CopyStr(ip, a, t, s, line); return; }
            if (t.K == TK.Struct)
            {
                var v = init.Eval(ip);
                ip.Store(a, t, v);
                return;
            }
            throw ip.RtErr(line, "um vetor precisa ser inicializado com { ... } ou com um texto entre aspas");
        }
        if (init is InitList l)
        {
            if (l.Items.Count == 0) { ip.Store(a, t, CVal.Int(0)); return; }
            init = l.Items[0].Value;
        }
        var val = init.Eval(ip);
        if (t.IsScalar && !val.T.IsScalar) throw ip.RtErr(line, "não é possível inicializar " + t + " com " + val.T);
        ip.Store(a, t, val);
    }

    static void CopyStr(CInterpreter ip, long a, CType t, StrLit s, int line)
    {
        long n = s.Text.Length;
        if (t.Len >= 0 && n > t.Len) throw ip.RtErr(line, "o texto \"" + s.Text + "\" não cabe no vetor de " + t.Len + " caracteres");
        for (int i = 0; i < n; i++) ip.Mem[a + i] = (sbyte)(byte)(s.Text[i] > 255 ? '?' : s.Text[i]);
        if (t.Len < 0 || n < t.Len) ip.Mem[a + n] = 0;
    }

    /// <summary>Fills an aggregate from a list, with C brace elision.</summary>
    static void Fill(CInterpreter ip, long a, CType t, List<InitItem> items, ref int pos, int line, bool braced)
    {
        if (t.K == TK.Array)
        {
            long len = t.Len < 0 ? long.MaxValue : t.Len;
            long i = 0;
            while (pos < items.Count)
            {
                var it = items[pos];
                if (it.Field != null) { if (braced) throw ip.RtErr(line, "inicializador .campo usado em um vetor"); return; }
                if (it.Index != null)
                {
                    if (!braced) return;
                    var iv = it.Index.Eval(ip);
                    if (iv.L < 0 || iv.L >= len) throw ip.RtErr(line, "índice [" + iv.L + "] fora do vetor no inicializador");
                    i = iv.L;
                }
                if (i >= len)
                {
                    if (braced) throw ip.RtErr(line, "valores demais no inicializador do vetor (tamanho " + t.Len + ")");
                    return;
                }
                FillElem(ip, a + i * t.Elem!.Cells, t.Elem, items, ref pos, line);
                i++;
            }
        }
        else
        {
            var fields = t.S!.Fields;
            int fi = 0;
            while (pos < items.Count)
            {
                var it = items[pos];
                if (it.Index != null) { if (braced) throw ip.RtErr(line, "inicializador [i] usado em uma struct"); return; }
                if (it.Field != null)
                {
                    if (!braced) return;
                    fi = fields.FindIndex(f => f.Name == it.Field);
                    if (fi < 0) throw ip.RtErr(line, "a struct " + t.S.Name + " não tem o campo '" + it.Field + "'");
                }
                if (fi >= fields.Count)
                {
                    if (braced) throw ip.RtErr(line, "valores demais no inicializador da struct " + t.S.Name);
                    return;
                }
                FillElem(ip, a + fields[fi].Offset, fields[fi].T, items, ref pos, line);
                fi++;
            }
        }
    }

    static void FillElem(CInterpreter ip, long a, CType t, List<InitItem> items, ref int pos, int line)
    {
        var it = items[pos];
        if (t.IsAggregate)
        {
            if (it.Value is InitList sub)
            {
                pos++;
                int p2 = 0;
                Fill(ip, a, t, sub.Items, ref p2, line, true);
            }
            else if (it.Value is StrLit s && t.K == TK.Array && t.Elem!.K == TK.Char)
            {
                pos++;
                CopyStr(ip, a, t, s, line);
            }
            else if (t.K == TK.Struct && it.Field == null && it.Index == null && it.Value is not InitList && IsStructValue(ip, it.Value, t))
            {
                pos++;
                ip.Store(a, t, it.Value.Eval(ip));
            }
            else
            {
                // brace elision: consume the following scalars; a designator only applies to the outer level
                var saved = (it.Field, it.Index);
                it.Field = null; it.Index = null;
                int before = pos;
                Fill(ip, a, t, items, ref pos, line, false);
                items[before].Field = saved.Field; items[before].Index = saved.Index;
            }
            return;
        }
        pos++;
        var v = it.Value is InitList l ? (l.Items.Count > 0 ? l.Items[0].Value.Eval(ip) : CVal.Int(0)) : it.Value.Eval(ip);
        if (t.IsScalar && !v.T.IsScalar) throw ip.RtErr(line, "não é possível inicializar " + t + " com " + v.T);
        ip.Store(a, t, v);
    }

    static bool IsStructValue(CInterpreter ip, Expr e, CType t)
    {
        try { var tt = e.TypeOf(ip); return tt.K == TK.Struct && tt.S == t.S; }
        catch (CError) { return false; }
    }

    /// <summary>Number of elements an initializer gives an array of unknown size.</summary>
    public static long CountFor(CType elem, Expr init)
    {
        if (init is StrLit s) return s.Text.Length + 1;
        if (init is not InitList il) return 1;
        if (!elem.IsAggregate || il.Items.All(i => i.Value is InitList || i.Value is StrLit))
        {
            long n = 0, cur = 0;
            foreach (var it in il.Items)
            {
                if (it.Index is ConstExpr c) cur = c.V.L;
                cur++;
                n = Math.Max(n, cur);
            }
            return n;
        }
        long scalars = CountScalars(elem);
        return (il.Items.Count + scalars - 1) / Math.Max(1, scalars);
    }

    static long CountScalars(CType t) => t.Cells;
}
