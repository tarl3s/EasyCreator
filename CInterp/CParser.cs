namespace EasyCreator.CInterp;

/// <summary>Recursive-descent parser that also resolves names (locals get frame offsets, globals get addresses).</summary>
internal sealed class CParser
{
    readonly List<Token> _t;
    int _p;
    readonly CInterpreter _ip;
    Scope _scope;
    readonly Scope _global;
    FuncDef? _fn;
    readonly Dictionary<string, StructDef> _tags = new();
    readonly List<CallExpr> _calls = new();
    int _switchDepth, _loopDepth;
    readonly Stack<int> _blockLevelOfSwitch = new();
    int _blockLevel;

    sealed class Scope
    {
        public readonly Dictionary<string, Sym> Syms = new();
        public readonly Scope? Parent;
        public Scope(Scope? parent) { Parent = parent; }

        public Sym? Find(string name)
        {
            for (var s = this; s != null; s = s.Parent)
                if (s.Syms.TryGetValue(name, out var v)) return v;
            return null;
        }
    }

    static readonly HashSet<string> BaseTypeWords = new()
    {
        "void", "char", "short", "int", "long", "float", "double", "signed", "unsigned", "_Bool", "bool", "boolean",
        "byte", "word", "size_t", "uint8_t", "int8_t", "uint16_t", "int16_t", "uint32_t", "int32_t", "uint64_t", "int64_t",
        "struct", "enum", "union", "String"
    };

    static readonly HashSet<string> Qualifiers = new()
    {
        "const", "volatile", "static", "extern", "register", "auto", "inline", "typedef", "unsigned", "signed"
    };

    static readonly HashSet<string> Keywords = new()
    {
        "if", "else", "while", "do", "for", "switch", "case", "default", "break", "continue", "return", "goto", "sizeof",
        "typedef", "struct", "union", "enum", "const", "static", "extern", "void", "char", "short", "int", "long",
        "float", "double", "signed", "unsigned", "volatile", "register", "auto", "inline"
    };

    public CParser(List<Token> toks, CInterpreter ip)
    {
        _t = toks;
        _ip = ip;
        _global = new Scope(null);
        _scope = _global;
        foreach (var (name, cls) in ip.NativeObjects)
        {
            var v = new VarSym { Name = name, T = cls.Type, Global = true, Addr = ip.AllocGlobal(1) };
            _global.Syms[name] = v;
            ip.GlobalInits.Add(new DeclStmt { V = v, CtorArgs = Array.Empty<Expr>(), Line = 0 });
        }
    }

    // ---------------- token helpers ----------------
    Token Peek(int k = 0) => _t[Math.Min(_p + k, _t.Count - 1)];
    Token Next() => _t[Math.Min(_p++, _t.Count - 1)];
    bool Is(string s, int k = 0) { var t = Peek(k); return (t.K == TokKind.Punct || t.K == TokKind.Ident) && t.Text == s; }
    bool IsPunct(string s, int k = 0) { var t = Peek(k); return t.K == TokKind.Punct && t.Text == s; }

    bool Accept(string s)
    {
        if (!IsPunct(s) && !(Peek().K == TokKind.Ident && Peek().Text == s)) return false;
        _p++;
        return true;
    }

    CError Err(string msg, Token? at = null) => new((at ?? Peek()).Line, msg, false);

    void Expect(string s, string? context = null)
    {
        if (Accept(s)) return;
        var got = Peek();
        // context: a whole Portuguese phrase ("depois de break"), translated as a unit
        string? ctx = context != null ? L.T(context) : null;
        if (s == ";")
        {
            var prev = _p > 0 ? _t[_p - 1] : got;
            string msg = ctx != null ? L.F("faltou ';' no fim da instrução ({0})", ctx) : L.T("faltou ';' no fim da instrução");
            if (got.K != TokKind.EOF) msg += L.F(" — encontrei {0}", Describe(got));
            throw new CError(prev.Line, msg, false);
        }
        throw Err(ctx != null ? L.F("esperado '{0}' {1}, mas encontrei {2}", s, ctx, Describe(got))
                              : L.F("esperado '{0}', mas encontrei {1}", s, Describe(got)));
    }

    static string Describe(Token t) => t.K switch
    {
        TokKind.EOF => L.T("o fim do arquivo (faltou fechar alguma chave '}'?)"),
        TokKind.String => L.F("o texto \"{0}\"", t.Text),
        _ => "'" + t.Text + "'"
    };

    string ExpectIdent(string what)
    {
        var t = Peek();
        if (t.K != TokKind.Ident || Keywords.Contains(t.Text)) throw Err(L.F("esperado {0}, mas encontrei {1}", L.T(what), Describe(t)));
        _p++;
        return t.Text;
    }

    // ---------------- program ----------------
    public void ParseProgram()
    {
        while (Peek().K != TokKind.EOF)
        {
            if (Accept(";")) continue;
            ParseExternal();
        }
        Link();
    }

    void Link()
    {
        foreach (var c in _calls)
        {
            if (_ip.Funcs.TryGetValue(c.Name, out var f))
            {
                if (f.Body == null && !_ip.Natives.ContainsKey(c.Name))
                    throw new CError(c.Line, L.F("a função {0}() foi declarada mas não foi escrita (falta o corpo {{ }})", c.Name), false);
                if (f.Body != null)
                {
                    if (!f.OldStyle && c.Args.Length != f.Params.Count)
                        throw new CError(c.Line, L.F("a função {0}() espera {1} argumento(s), mas recebeu {2}", c.Name, f.Params.Count, c.Args.Length), false);
                    c.F = f;
                    continue;
                }
            }
            if (_ip.Natives.TryGetValue(c.Name, out var n)) { c.N = n; continue; }
            var sug = Suggest(c.Name, _ip.Funcs.Keys.Concat(_ip.Natives.Keys));
            string msg = sug != null ? L.F("a função {0}() não existe — você quis dizer {1}()?", c.Name, sug)
                                     : L.F("a função {0}() não existe", c.Name);
            throw new CError(c.Line, msg, false);
        }
    }

    static string? Suggest(string name, IEnumerable<string> names)
    {
        string? best = null;
        int bestD = int.MaxValue;
        foreach (var n in names.Distinct())
        {
            int d = Lev(name.ToLowerInvariant(), n.ToLowerInvariant());
            if (d < bestD) { bestD = d; best = n; }
        }
        return bestD <= Math.Max(1, name.Length / 3) ? best : null;
    }

    static int Lev(string a, string b)
    {
        var d = new int[a.Length + 1, b.Length + 1];
        for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) d[0, j] = j;
        for (int i = 1; i <= a.Length; i++)
            for (int j = 1; j <= b.Length; j++)
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
        return d[a.Length, b.Length];
    }

    // ---------------- declarations ----------------
    sealed class Specs
    {
        public CType T = CType.Int;
        public bool Static, Const, Typedef, Extern;
    }

    bool IsTypeStart(int k = 0)
    {
        var t = Peek(k);
        if (t.K != TokKind.Ident) return false;
        if (BaseTypeWords.Contains(t.Text) || Qualifiers.Contains(t.Text)) return true;
        if (_ip.Classes.ContainsKey(t.Text)) return _scope.Find(t.Text) is null or TypedefSym;
        return _scope.Find(t.Text) is TypedefSym;
    }

    Specs ParseSpecs()
    {
        var sp = new Specs();
        int nLong = 0, nShort = 0, nUnsigned = 0, nSigned = 0;
        string? baseWord = null;
        CType? named = null;
        var start = Peek();
        while (true)
        {
            var t = Peek();
            if (t.K != TokKind.Ident) break;
            string w = t.Text;
            if (w == "const") { sp.Const = true; _p++; continue; }
            if (w is "volatile" or "register" or "auto" or "inline") { _p++; continue; }
            if (w == "static") { sp.Static = true; _p++; continue; }
            if (w == "extern") { sp.Extern = true; _p++; continue; }
            if (w == "typedef") { sp.Typedef = true; _p++; continue; }
            if (w == "unsigned") { nUnsigned++; _p++; continue; }
            if (w == "signed") { nSigned++; _p++; continue; }
            if (w == "long") { nLong++; _p++; continue; }
            if (w == "short") { nShort++; _p++; continue; }
            if (baseWord != null || named != null) break;
            if (w is "void" or "char" or "int" or "float" or "double" or "_Bool" or "bool" or "boolean")
            {
                baseWord = w; _p++; continue;
            }
            switch (w)
            {
                case "byte": case "uint8_t": named = CType.UChar; _p++; continue;
                case "int8_t": named = CType.Char; _p++; continue;
                case "int16_t": named = CType.Short; _p++; continue;
                case "uint16_t": case "word": named = CType.UShort; _p++; continue;
                case "int32_t": named = CType.Int; _p++; continue;
                case "uint32_t": named = CType.UInt; _p++; continue;
                case "int64_t": named = CType.Long; _p++; continue;
                case "uint64_t": case "size_t": named = CType.ULong; _p++; continue;
                case "struct": _p++; named = ParseStruct(); continue;
                case "enum": _p++; named = ParseEnum(); continue;
                case "union": throw Err(L.T("union não é suportada neste interpretador (use struct)"));
                case "String": throw Err(L.T("o tipo String do Arduino não é suportado aqui; use vetores de char (ex.: char texto[20])"));
            }
            if (_ip.Classes.TryGetValue(w, out var cls) && _scope.Find(w) == null)
            {
                named = cls.Type; _p++; continue;
            }
            if (_scope.Find(w) is TypedefSym td) { named = td.T; _p++; continue; }
            break;
        }
        if (named != null)
        {
            if (nUnsigned > 0 || nLong > 0 || nShort > 0) throw Err(L.T("combinação de tipos inválida"), start);
            sp.T = named;
            return sp;
        }
        bool u = nUnsigned > 0;
        if (nUnsigned > 0 && nSigned > 0) throw Err(L.T("um tipo não pode ser signed e unsigned ao mesmo tempo"), start);
        switch (baseWord)
        {
            case "void": sp.T = CType.Void; break;
            case "char": sp.T = u ? CType.UChar : CType.Char; break;
            case "_Bool": case "bool": case "boolean": sp.T = CType.Bool; break;
            case "float": sp.T = CType.Float; break;
            case "double": sp.T = CType.Double; break;
            default:
                if (baseWord == null && nUnsigned == 0 && nSigned == 0 && nLong == 0 && nShort == 0)
                {
                    if (sp.Static || sp.Const || sp.Extern || sp.Typedef) { sp.T = CType.Int; break; }
                    throw Err(L.T("esperado um tipo (int, float, char...)"), start);
                }
                if (nShort > 0) sp.T = u ? CType.UShort : CType.Short;
                else if (nLong > 0) sp.T = u ? CType.ULong : CType.Long;
                else sp.T = u ? CType.UInt : CType.Int;
                break;
        }
        return sp;
    }

    CType ParseStruct()
    {
        string? tag = null;
        if (Peek().K == TokKind.Ident && !IsPunct("{")) tag = Next().Text;
        StructDef? def = null;
        if (tag != null) _tags.TryGetValue(tag, out def);
        if (IsPunct("{"))
        {
            _p++;
            if (def == null || def.Complete)
            {
                def = new StructDef { Name = tag ?? L.T("(anônima)") };
                if (tag != null) _tags[tag] = def;
            }
            while (!Accept("}"))
            {
                if (Peek().K == TokKind.EOF) throw Err(L.T("faltou fechar a struct com '}'"));
                var sp = ParseSpecs();
                do
                {
                    var d = ParseDeclarator(sp.T, false);
                    if (d.Params != null) throw Err(L.T("funções dentro de struct não são suportadas"));
                    if (d.Dims != null) throw Err(L.T("o tamanho de um vetor dentro de struct precisa ser constante"));
                    if (Accept(":")) throw Err(L.T("campos de bits (:) não são suportados"));
                    if (d.T.K == TK.Struct && !d.T.S!.Complete) throw Err(L.F("a struct {0} não pode conter a si mesma (use um ponteiro)", d.T.S.Name));
                    if (d.T.K == TK.Array && d.T.Len < 0) throw Err(L.T("vetor dentro de struct precisa de tamanho"));
                    if (def.ByName.ContainsKey(d.Name!)) throw Err(L.F("campo '{0}' repetido na struct", d.Name));
                    def.Add(d.Name!, d.T);
                } while (Accept(","));
                Expect(";", "depois do campo da struct");
            }
            def.Finish();
            TagAsType(tag, StructType(def));
            return StructType(def);
        }
        if (tag == null) throw Err(L.T("esperado o nome da struct ou '{'"));
        if (def == null)
        {
            def = new StructDef { Name = tag };
            _tags[tag] = def;
        }
        TagAsType(tag, StructType(def));
        return StructType(def);
    }

    /// <summary>C++ style: after "struct Point {...}" or "enum Cor {...}", "Point p;" also works.</summary>
    void TagAsType(string? tag, CType t)
    {
        if (tag == null || _scope.Find(tag) != null || _ip.Classes.ContainsKey(tag)) return;
        _scope.Syms[tag] = new TypedefSym { Name = tag, T = t, FromTag = true, Line = Peek().Line };
    }

    readonly Dictionary<StructDef, CType> _structTypes = new();
    CType StructType(StructDef d)
    {
        if (!_structTypes.TryGetValue(d, out var t)) _structTypes[d] = t = new CType(TK.Struct) { S = d };
        return t;
    }

    CType ParseEnum()
    {
        if (Peek().K == TokKind.Ident && !IsPunct("{")) TagAsType(Next().Text, CType.Int);
        if (Accept("{"))
        {
            long v = 0;
            while (!Accept("}"))
            {
                var nameTok = Peek();
                string name = ExpectIdent("o nome de um valor do enum");
                if (Accept("="))
                {
                    var e = ParseAssign();
                    v = ConstEval(e) ?? throw Err(L.T("o valor de um enum precisa ser constante"), nameTok);
                }
                Declare(new EnumSym { Name = name, Value = v, Line = nameTok.Line }, nameTok);
                v++;
                if (!Accept(",")) { Expect("}", "no fim do enum"); break; }
            }
        }
        return CType.Int;
    }

    sealed class Decl
    {
        public string? Name;
        public CType T = CType.Int;
        public List<VarSym>? Params;
        public bool OldStyle;
        public List<Expr?>? Dims;   // VLA dimensions (any non-constant)
        public CType? VlaBase;
        public Token NameTok = null!;
    }

    Decl ParseDeclarator(CType baseT, bool allowAbstract)
    {
        var d = new Decl { NameTok = Peek() };
        var t = baseT;
        while (true)
        {
            if (Accept("*")) { t = t.Pointer; while (Accept("const") || Accept("volatile")) { } continue; }
            if (IsPunct("&")) throw Err(L.T("referências (&) do C++ não são suportadas; use ponteiros (*)"));
            break;
        }
        if (IsPunct("(") && IsPunct("*", 1)) throw Err(L.T("ponteiros para função não são suportados neste interpretador"));
        if (Peek().K == TokKind.Ident && !Keywords.Contains(Peek().Text))
        {
            d.NameTok = Peek();
            d.Name = Next().Text;
        }
        else if (!allowAbstract) throw Err(L.F("esperado o nome da variável, mas encontrei {0}", Describe(Peek())));

        if (IsPunct("(") && d.Name != null && !(_ip.Classes.ContainsKey(baseT.N?.Name ?? "") && baseT.K == TK.Native && t == baseT))
        {
            _p++;
            d.Params = ParseParams(out d.OldStyle);
        }
        var dims = new List<Expr?>();
        while (Accept("["))
        {
            if (Accept("]")) { dims.Add(null); continue; }
            dims.Add(ParseExpr());
            Expect("]", "no tamanho do vetor");
        }
        if (dims.Count > 0)
        {
            var consts = dims.Select(e => e == null ? (long?)-1 : ConstEval(e)).ToList();
            if (consts.All(c => c != null))
            {
                for (int i = dims.Count - 1; i >= 0; i--)
                {
                    long n = consts[i]!.Value;
                    if (n == -1 && i > 0) throw Err(L.T("só a primeira dimensão de um vetor pode ficar sem tamanho"));
                    if (n < -1 || n == 0 && dims[i] != null) throw Err(L.T("o tamanho do vetor precisa ser maior que zero"));
                    t = CType.ArrayOf(t, n);
                }
            }
            else
            {
                if (dims.Any(e => e == null)) throw Err(L.T("vetor de tamanho variável precisa de todos os tamanhos"));
                if (dims.Count > 3) throw Err(L.T("vetores de tamanho variável com mais de 3 dimensões não são suportados"));
                d.Dims = dims;
                d.VlaBase = t;
                t = t.Pointer; // placeholder; real type built at runtime
            }
        }
        d.T = t;
        return d;
    }

    List<VarSym> ParseParams(out bool oldStyle)
    {
        var list = new List<VarSym>();
        oldStyle = false;
        if (Accept(")")) { oldStyle = true; return list; }
        if (Is("void") && IsPunct(")", 1)) { _p += 2; return list; }
        while (true)
        {
            if (IsPunct("...")) throw Err(L.T("funções com número variável de argumentos (...) não são suportadas"));
            if (!IsTypeStart()) throw Err(L.F("esperado o tipo do parâmetro, mas encontrei {0}", Describe(Peek())));
            var sp = ParseSpecs();
            var d = ParseDeclarator(sp.T, true);
            var t = d.T;
            if (d.Dims != null)
            {
                // int m[n][k] parameters: treat as pointer to the inner part when inner is constant
                if (d.Dims.Skip(1).Any(e => ConstEval(e!) == null))
                    throw Err(L.T("em parâmetros, só a primeira dimensão do vetor pode ser variável"));
                var inner = d.VlaBase!;
                for (int i = d.Dims.Count - 1; i >= 1; i--) inner = CType.ArrayOf(inner, ConstEval(d.Dims[i]!)!.Value);
                t = inner.Pointer;
            }
            else if (t.K == TK.Array) t = t.Elem!.Pointer;
            if (t.K == TK.Void) throw Err(L.T("um parâmetro não pode ser void"));
            list.Add(new VarSym { Name = d.Name ?? "", T = t, IsConst = sp.Const && t.IsScalar && !t.IsPtr, Line = d.NameTok.Line });
            if (Accept(")")) break;
            Expect(",", "entre os parâmetros");
        }
        return list;
    }

    void Declare(Sym s, Token at)
    {
        if (_scope.Syms.TryGetValue(s.Name, out var old))
        {
            if (old is FuncSym && s is FuncSym) { _scope.Syms[s.Name] = s; return; }
            if (old is TypedefSym { FromTag: true }) { _scope.Syms[s.Name] = s; return; }
            if (old is TypedefSym ot && s is TypedefSym nt && ReferenceEquals(ot.T, nt.T)) return;
            if (old is VarSym ov && ov.Global && s is VarSym) return; // extern + definition
            throw new CError(at.Line, L.F("'{0}' já foi declarado(a) antes (linha {1})", s.Name, old.Line), false);
        }
        _scope.Syms[s.Name] = s;
    }

    void ParseExternal()
    {
        var start = Peek();
        if (!IsTypeStart())
        {
            if (start.K == TokKind.Ident && IsPunct("(", 1))
            {
                // function without return type: treat as int (old C)
            }
            else throw Err(L.F("esperado uma declaração (variável ou função), mas encontrei {0}", Describe(start)));
        }
        var sp = IsTypeStart() ? ParseSpecs() : new Specs { T = CType.Int };
        if (Accept(";")) return;
        bool first = true;
        while (true)
        {
            var d = ParseDeclarator(sp.T, false);
            if (sp.Typedef)
            {
                if (d.Params != null) throw Err(L.T("typedef de função não é suportado"));
                Declare(new TypedefSym { Name = d.Name!, T = d.T, Line = d.NameTok.Line }, d.NameTok);
            }
            else if (d.Params != null)
            {
                var f = DeclareFunc(d, sp);
                if (first && IsPunct("{"))
                {
                    ParseFunctionBody(f, d);
                    return;
                }
            }
            else
            {
                if (d.Dims != null) throw Err(L.T("variáveis globais precisam de tamanho constante no vetor"), d.NameTok);
                DeclareVar(d, sp, true);
            }
            first = false;
            if (Accept(",")) continue;
            Expect(";", "depois da declaração");
            return;
        }
    }

    FuncDef DeclareFunc(Decl d, Specs sp)
    {
        if (d.T.K == TK.Array) throw Err(L.T("uma função não pode retornar um vetor"));
        if (_global.Syms.TryGetValue(d.Name!, out var old) && old is FuncSym fs)
        {
            if (fs.F.Body == null)
            {
                fs.F.Params = d.Params!;
                fs.F.OldStyle = d.OldStyle && fs.F.OldStyle;
                fs.F.Ret = d.T;
            }
            return fs.F;
        }
        if (old != null) throw Err(L.F("'{0}' já foi declarado como variável (linha {1})", d.Name, old.Line), d.NameTok);
        var f = new FuncDef { Name = d.Name!, Ret = d.T, Params = d.Params!, Line = d.NameTok.Line, OldStyle = d.OldStyle };
        _global.Syms[f.Name] = new FuncSym { Name = f.Name, F = f, Line = f.Line };
        _ip.Funcs[f.Name] = f;
        return f;
    }

    void ParseFunctionBody(FuncDef f, Decl d)
    {
        if (f.Body != null) throw Err(L.F("a função {0}() foi escrita duas vezes", f.Name), d.NameTok);
        f.Params = d.Params!;
        f.Ret = d.T;
        f.OldStyle = false;
        f.Line = d.NameTok.Line;
        _fn = f;
        f.FrameCells = 0;
        _scope = new Scope(_global);
        foreach (var p in f.Params)
        {
            if (p.Name.Length == 0) throw Err(L.F("parâmetro sem nome na função {0}()", f.Name), d.NameTok);
            p.Offset = f.FrameCells;
            f.FrameCells += p.T.Cells;
            Declare(p, d.NameTok);
        }
        var body = ParseBlock(newScope: false);
        f.Body = body;
        _scope = _global;
        _fn = null;
    }

    VarSym DeclareVar(Decl d, Specs sp, bool global)
    {
        var t = d.T;
        if (t.K == TK.Void) throw Err(L.T("uma variável não pode ser do tipo void"), d.NameTok);
        if (t.K == TK.Struct && !t.S!.Complete) throw Err(L.F("a struct {0} ainda não foi definida", t.S.Name), d.NameTok);
        Expr? init = null;
        Expr[]? ctor = null;
        if (Accept("="))
        {
            init = ParseInitializer();
            if (t.K == TK.Array && t.Len < 0)
            {
                long n = Initializer.CountFor(t.Elem!, init);
                t = CType.ArrayOf(t.Elem!, n);
            }
        }
        else if (t.K == TK.Native && Accept("("))
        {
            ctor = ParseArgs();
        }
        if (t.K == TK.Array && t.Len < 0) throw Err(L.F("o vetor '{0}' precisa de um tamanho ou de uma lista de valores", d.Name), d.NameTok);
        if (t.K == TK.Native && init != null) throw Err(L.F("objetos {0} não podem receber valor com =", t.N!.Name), d.NameTok);

        var v = new VarSym { Name = d.Name!, T = t, Line = d.NameTok.Line, IsConst = sp.Const && t.IsScalar && !t.IsPtr };
        if (global || sp.Static)
        {
            v.Global = true;
            v.Addr = _ip.AllocGlobal(t.Cells);
            if (v.IsConst && init != null && t.IsInteger && ConstEval(init) is long cv) v.ConstValue = cv;
            Declare(v, d.NameTok);
            if (init != null || ctor != null || t.K == TK.Native)
                _ip.GlobalInits.Add(new DeclStmt { V = v, Init = init, CtorArgs = ctor ?? Array.Empty<Expr>(), Line = d.NameTok.Line });
            return v;
        }
        if (d.Dims != null)
        {
            v.VlaDims = d.Dims;
            v.VlaBase = d.VlaBase;
            v.T = d.VlaBase!.Pointer;
            v.Offset = _fn!.FrameCells;
            _fn.FrameCells += 1 + d.Dims.Count;
        }
        else
        {
            v.Offset = _fn!.FrameCells;
            _fn.FrameCells += t.Cells;
            if (v.IsConst && init != null && t.IsInteger && ConstEval(init) is long cv) v.ConstValue = cv;
        }
        Declare(v, d.NameTok);
        _pendingDecls.Add(new DeclStmt { V = v, Init = init, CtorArgs = ctor, Line = d.NameTok.Line });
        return v;
    }

    readonly List<Stmt> _pendingDecls = new();

    Expr ParseInitializer()
    {
        if (!IsPunct("{")) return ParseAssign();
        var open = Next();
        var list = new InitList { Line = open.Line };
        while (!Accept("}"))
        {
            var item = new InitItem();
            if (IsPunct(".") && Peek(1).K == TokKind.Ident)
            {
                _p++;
                item.Field = Next().Text;
                Expect("=", "depois de .campo");
            }
            else if (IsPunct("["))
            {
                _p++;
                var ie = ParseExpr();
                long idx = ConstEval(ie) ?? throw Err(L.T("o índice do inicializador precisa ser constante"));
                item.Index = new ConstExpr { V = CVal.Int(idx), Line = ie.Line };
                Expect("]");
                Expect("=", "depois de [índice]");
            }
            item.Value = ParseInitializer();
            list.Items.Add(item);
            if (!Accept(",")) { Expect("}", "no fim da lista de valores"); break; }
        }
        return list;
    }

    // ---------------- statements ----------------
    Block ParseBlock(bool newScope = true)
    {
        var open = Peek();
        Expect("{");
        var saved = _scope;
        if (newScope) _scope = new Scope(_scope);
        _blockLevel++;
        var list = new List<Stmt>();
        while (!IsPunct("}"))
        {
            if (Peek().K == TokKind.EOF) throw new CError(open.Line, L.F("a chave '{{' aberta na linha {0} não foi fechada com '}}'", open.Line), false);
            ParseStatementInto(list);
        }
        _p++;
        _blockLevel--;
        _scope = saved;
        return new Block { S = list.ToArray(), Line = open.Line };
    }

    void ParseStatementInto(List<Stmt> list)
    {
        if (IsDeclarationStart())
        {
            ParseLocalDecl(list);
            return;
        }
        list.Add(ParseStatement());
    }

    bool IsDeclarationStart()
    {
        if (!IsTypeStart()) return false;
        // "Name(" where Name is a class but used as call? classes are always types here
        return true;
    }

    void ParseLocalDecl(List<Stmt> list)
    {
        var sp = ParseSpecs();
        if (Accept(";")) return;
        while (true)
        {
            var d = ParseDeclarator(sp.T, false);
            if (sp.Typedef)
            {
                if (d.Params != null || d.Dims != null) throw Err(L.T("typedef inválido"));
                Declare(new TypedefSym { Name = d.Name!, T = d.T, Line = d.NameTok.Line }, d.NameTok);
            }
            else if (d.Params != null)
            {
                DeclareFunc(d, sp); // local prototype
            }
            else
            {
                if (sp.Static && d.Dims != null) throw Err(L.T("variável static precisa de tamanho constante"), d.NameTok);
                _pendingDecls.Clear();
                DeclareVar(d, sp, false);
                list.AddRange(_pendingDecls);
                _pendingDecls.Clear();
            }
            if (Accept(",")) continue;
            Expect(";", "depois da declaração");
            return;
        }
    }

    Stmt ParseStatement()
    {
        System.Runtime.CompilerServices.RuntimeHelpers.EnsureSufficientExecutionStack();
        var t = Peek();
        int line = t.Line;
        if (t.K == TokKind.Punct)
        {
            if (t.Text == "{") return ParseBlock();
            if (t.Text == ";") { _p++; return new EmptyStmt { Line = line }; }
        }
        if (t.K == TokKind.Ident)
        {
            switch (t.Text)
            {
                case "if":
                    {
                        _p++;
                        Expect("(", "depois de if");
                        var c = ParseExpr();
                        Expect(")", "para fechar a condição do if");
                        var th = ParseSubStatement();
                        Stmt? el = null;
                        if (Accept("else")) el = ParseSubStatement();
                        return new IfStmt { C = c, Then = th, Else = el, Line = line };
                    }
                case "while":
                    {
                        _p++;
                        Expect("(", "depois de while");
                        var c = ParseExpr();
                        Expect(")", "para fechar a condição do while");
                        _loopDepth++;
                        var body = ParseSubStatement();
                        _loopDepth--;
                        return new WhileStmt { C = c, Body = body, Line = line };
                    }
                case "do":
                    {
                        _p++;
                        _loopDepth++;
                        var body = ParseSubStatement();
                        _loopDepth--;
                        if (!Accept("while")) throw Err(L.T("esperado 'while' depois do bloco do 'do'"));
                        Expect("(", "depois de while");
                        var c = ParseExpr();
                        Expect(")", "para fechar a condição do while");
                        Expect(";", "depois do do-while");
                        return new DoStmt { C = c, Body = body, Line = line };
                    }
                case "for":
                    {
                        _p++;
                        Expect("(", "depois de for");
                        var saved = _scope;
                        _scope = new Scope(_scope);
                        Stmt? init = null;
                        if (!Accept(";"))
                        {
                            if (IsDeclarationStart())
                            {
                                var l = new List<Stmt>();
                                ParseLocalDecl(l);
                                init = l.Count == 1 ? l[0] : new Block { S = l.ToArray(), Line = line };
                            }
                            else
                            {
                                init = new ExprStmt { E = ParseExpr(), Line = line };
                                Expect(";", "no for (depois da inicialização)");
                            }
                        }
                        Expr? c = null, nx = null;
                        if (!Accept(";")) { c = ParseExpr(); Expect(";", "no for (depois da condição)"); }
                        if (!IsPunct(")")) nx = ParseExpr();
                        Expect(")", "para fechar o for");
                        _loopDepth++;
                        var body = ParseSubStatement();
                        _loopDepth--;
                        _scope = saved;
                        return new ForStmt { Init = init, C = c, Next = nx, Body = body, Line = line };
                    }
                case "switch":
                    {
                        _p++;
                        Expect("(", "depois de switch");
                        var e = ParseExpr();
                        Expect(")", "para fechar o switch");
                        if (!IsPunct("{")) throw Err(L.T("o switch precisa de um bloco { case ...: }"));
                        _switchDepth++;
                        _blockLevelOfSwitch.Push(_blockLevel + 1);
                        var body = ParseBlock();
                        _blockLevelOfSwitch.Pop();
                        _switchDepth--;
                        return new SwitchStmt { E = e, Body = body, Line = line };
                    }
                case "case":
                case "default":
                    {
                        _p++;
                        if (_switchDepth == 0) throw Err(L.F("'{0}' fora de um switch", t.Text), t);
                        if (_blockLevelOfSwitch.Peek() != _blockLevel)
                            throw Err(L.F("'{0}' precisa ficar direto dentro do bloco do switch", t.Text), t);
                        Expr? v = null;
                        if (t.Text == "case")
                        {
                            v = ParseCond();
                            if (ConstEval(v) is long cv) v = new ConstExpr { V = CVal.LongV(cv), Line = line };
                        }
                        Expect(":", t.Text == "case" ? "depois do case" : "depois do default");
                        return new CaseLabel { Value = v, Line = line };
                    }
                case "break":
                    _p++;
                    if (_loopDepth == 0 && _switchDepth == 0) throw Err(L.T("'break' fora de um laço ou switch"), t);
                    Expect(";", "depois de break");
                    return new JumpStmt { Kind = Flow.Break, Line = line };
                case "continue":
                    _p++;
                    if (_loopDepth == 0) throw Err(L.T("'continue' fora de um laço"), t);
                    Expect(";", "depois de continue");
                    return new JumpStmt { Kind = Flow.Continue, Line = line };
                case "return":
                    {
                        _p++;
                        Expr? e = null;
                        if (!IsPunct(";")) e = ParseExpr();
                        if (_fn != null && _fn.Ret.K == TK.Void && e != null) throw Err(L.F("a função {0}() é void e não pode retornar um valor", _fn.Name), t);
                        Expect(";", "depois do return");
                        return new ReturnStmt { E = e, Line = line };
                    }
                case "goto":
                    throw Err(L.T("goto não é suportado (use laços e break)"));
                case "else":
                    throw Err(L.T("'else' sem um 'if' antes (verifique as chaves e o ';' antes do else)"));
            }
        }
        var ex = ParseExpr();
        Expect(";", null);
        return new ExprStmt { E = ex, Line = line };
    }

    Stmt ParseSubStatement()
    {
        if (IsDeclarationStart())
        {
            // a declaration as the body of if/for: give it its own scope
            var saved = _scope;
            _scope = new Scope(_scope);
            var l = new List<Stmt>();
            ParseLocalDecl(l);
            _scope = saved;
            return new Block { S = l.ToArray(), Line = l.Count > 0 ? l[0].Line : Peek().Line };
        }
        return ParseStatement();
    }

    // ---------------- expressions ----------------
    Expr ParseExpr()
    {
        var e = ParseAssign();
        while (IsPunct(","))
        {
            int line = Next().Line;
            e = new CommaExpr { A = e, B = ParseAssign(), Line = line };
        }
        return e;
    }

    static readonly Dictionary<string, BinKind> AssignOps = new()
    {
        ["="] = BinKind.None, ["+="] = BinKind.Add, ["-="] = BinKind.Sub, ["*="] = BinKind.Mul, ["/="] = BinKind.Div,
        ["%="] = BinKind.Mod, ["<<="] = BinKind.Shl, [">>="] = BinKind.Shr, ["&="] = BinKind.And, ["|="] = BinKind.Or, ["^="] = BinKind.Xor
    };

    Expr ParseAssign()
    {
        var l = ParseCond();
        var t = Peek();
        if (t.K == TokKind.Punct && AssignOps.TryGetValue(t.Text, out var op))
        {
            _p++;
            if (!l.IsLValue) throw Err(L.F("o lado esquerdo de '{0}' precisa ser uma variável", t.Text), t);
            if (l is IdentExpr id && id.S.IsConst) throw Err(L.F("'{0}' é constante (const) e não pode ser alterada", id.S.Name), t);
            var r = ParseAssign();
            return new AssignExpr { Op = op, L = l, R = r, Line = t.Line };
        }
        return l;
    }

    Expr ParseCond()
    {
        var c = ParseBinary(0);
        if (IsPunct("?"))
        {
            int line = Next().Line;
            var a = ParseExpr();
            Expect(":", "no operador ternário ? :");
            var b = ParseCond();
            return new CondExpr { C = c, A = a, B = b, Line = line };
        }
        return c;
    }

    static readonly string[][] Levels =
    {
        new[] { "||" }, new[] { "&&" }, new[] { "|" }, new[] { "^" }, new[] { "&" },
        new[] { "==", "!=" }, new[] { "<", "<=", ">", ">=" }, new[] { "<<", ">>" }, new[] { "+", "-" }, new[] { "*", "/", "%" }
    };

    static BinKind KindOf(string op) => op switch
    {
        "+" => BinKind.Add, "-" => BinKind.Sub, "*" => BinKind.Mul, "/" => BinKind.Div, "%" => BinKind.Mod,
        "<<" => BinKind.Shl, ">>" => BinKind.Shr, "&" => BinKind.And, "|" => BinKind.Or, "^" => BinKind.Xor,
        "==" => BinKind.Eq, "!=" => BinKind.Ne, "<" => BinKind.Lt, "<=" => BinKind.Le, ">" => BinKind.Gt, ">=" => BinKind.Ge,
        _ => BinKind.None
    };

    Expr ParseBinary(int level)
    {
        if (level >= Levels.Length) return ParseUnary();
        var a = ParseBinary(level + 1);
        while (true)
        {
            var t = Peek();
            if (t.K != TokKind.Punct || Array.IndexOf(Levels[level], t.Text) < 0) return a;
            _p++;
            var b = ParseBinary(level + 1);
            if (t.Text == "&&" || t.Text == "||") a = new LogicExpr { IsAnd = t.Text == "&&", A = a, B = b, Line = t.Line };
            else a = Fold(new BinExpr { Op = KindOf(t.Text), A = a, B = b, Line = t.Line });
        }
    }

    Expr Fold(BinExpr b)
    {
        if (b.A is ConstExpr x && b.B is ConstExpr y)
        {
            try { return new ConstExpr { V = _ip.BinOp(b.Op, x.V, y.V, b.Line), Line = b.Line }; }
            catch (CError) { }
        }
        return b;
    }

    Expr ParseUnary()
    {
        System.Runtime.CompilerServices.RuntimeHelpers.EnsureSufficientExecutionStack();
        var t = Peek();
        int line = t.Line;
        if (t.K == TokKind.Punct)
        {
            switch (t.Text)
            {
                case "++":
                case "--":
                    {
                        _p++;
                        var e = ParseUnary();
                        if (!e.IsLValue) throw Err(L.F("{0} precisa de uma variável", t.Text), t);
                        CheckNotConst(e, t);
                        return new IncDecExpr { E = e, Delta = t.Text == "++" ? 1 : -1, Prefix = true, Line = line };
                    }
                case "-":
                    {
                        _p++;
                        var e = ParseUnary();
                        if (e is ConstExpr c && c.V.T.IsArith)
                            return new ConstExpr { V = c.V.T.IsFloat ? new CVal { T = c.V.T, D = -c.V.D } : new CVal { T = c.V.T.Promoted, L = Conv.Norm(-c.V.L, c.V.T.Promoted) }, Line = line };
                        return new UnaryExpr { Op = UnKind.Neg, E = e, Line = line };
                    }
                case "+": _p++; return new UnaryExpr { Op = UnKind.Plus, E = ParseUnary(), Line = line };
                case "!": _p++; return new UnaryExpr { Op = UnKind.Not, E = ParseUnary(), Line = line };
                case "~": _p++; return new UnaryExpr { Op = UnKind.BitNot, E = ParseUnary(), Line = line };
                case "*": _p++; return new DerefExpr { E = ParseUnary(), Line = line };
                case "&":
                    {
                        _p++;
                        var e = ParseUnary();
                        if (!e.IsLValue) throw Err(L.T("o operador & precisa de uma variável (ex.: &x)"), t);
                        return new AddrOfExpr { E = e, Line = line };
                    }
                case "(":
                    if (IsTypeStart(1))
                    {
                        _p++;
                        var ty = ParseTypeName();
                        Expect(")", "para fechar a conversão de tipo");
                        if (IsPunct("{")) throw Err(L.T("literais compostos (tipo){...} não são suportados"));
                        var e = ParseUnary();
                        if (e is ConstExpr c && ty.IsArith && c.V.T.IsArith) return new ConstExpr { V = Conv.To(c.V, ty), Line = line };
                        return new CastExpr { T = ty, E = e, Line = line };
                    }
                    break;
            }
        }
        if (t.K == TokKind.Ident && t.Text == "sizeof")
        {
            _p++;
            if (IsPunct("(") && IsTypeStart(1))
            {
                _p++;
                var ty = ParseTypeName();
                Expect(")", "no sizeof");
                return new ConstExpr { V = CVal.Of(CType.ULong, ty.Bytes), Line = line };
            }
            var e = ParseUnary();
            if (e is IdentExpr id && id.S.VlaDims == null) return new ConstExpr { V = CVal.Of(CType.ULong, id.S.T.Bytes), Line = line };
            return new SizeofExpr { E = e, Line = line };
        }
        return ParsePostfix(ParsePrimary());
    }

    void CheckNotConst(Expr e, Token t)
    {
        if (e is IdentExpr id && id.S.IsConst) throw Err(L.F("'{0}' é constante (const) e não pode ser alterada", id.S.Name), t);
    }

    CType ParseTypeName()
    {
        var sp = ParseSpecs();
        var d = ParseDeclarator(sp.T, true);
        if (d.Name != null) throw Err(L.F("nome inesperado '{0}' dentro do tipo", d.Name));
        if (d.Dims != null) throw Err(L.T("tamanho de vetor precisa ser constante aqui"));
        return d.T;
    }

    Expr[] ParseArgs()
    {
        var args = new List<Expr>();
        if (Accept(")")) return args.ToArray();
        while (true)
        {
            args.Add(ParseAssign());
            if (Accept(")")) break;
            Expect(",", "entre os argumentos");
        }
        return args.ToArray();
    }

    Expr ParsePostfix(Expr e)
    {
        while (true)
        {
            var t = Peek();
            if (t.K != TokKind.Punct) return e;
            switch (t.Text)
            {
                case "[":
                    {
                        _p++;
                        var idx = ParseExpr();
                        Expect("]", "para fechar o índice");
                        e = new IndexExpr { Base = e, Index = idx, Line = t.Line };
                        break;
                    }
                case "(":
                    throw Err(L.T("só é possível chamar funções pelo nome"), t);
                case ".":
                case "->":
                    {
                        _p++;
                        string name = ExpectIdent(t.Text == "." ? "o nome do campo depois de ." : "o nome do campo depois de ->");
                        if (IsPunct("(") && t.Text == ".")
                        {
                            _p++;
                            var args = ParseArgs();
                            e = new MethodCallExpr { Obj = e, Name = name, Args = args, Line = t.Line };
                        }
                        else e = new MemberExpr { Base = e, Name = name, Arrow = t.Text == "->", Line = t.Line };
                        break;
                    }
                case "++":
                case "--":
                    _p++;
                    if (!e.IsLValue) throw Err(L.F("{0} precisa de uma variável", t.Text), t);
                    CheckNotConst(e, t);
                    e = new IncDecExpr { E = e, Delta = t.Text == "++" ? 1 : -1, Prefix = false, Line = t.Line };
                    break;
                default:
                    return e;
            }
        }
    }

    Expr ParsePrimary()
    {
        var t = Next();
        int line = t.Line;
        switch (t.K)
        {
            case TokKind.Int:
                {
                    CType ty;
                    ulong u = (ulong)t.I;
                    bool hexOrOct = t.Text.Length > 1 && t.Text[0] == '0';
                    if (t.LongSfx) ty = t.Unsigned ? CType.ULong : CType.Long;
                    else if (t.Unsigned) ty = u <= uint.MaxValue ? CType.UInt : CType.ULong;
                    else if (u <= int.MaxValue) ty = CType.Int;
                    else if (hexOrOct && u <= uint.MaxValue) ty = CType.UInt;
                    else ty = u <= long.MaxValue ? CType.Long : CType.ULong;
                    return new ConstExpr { V = CVal.Of(ty, t.I), Line = line };
                }
            case TokKind.Float:
                return new ConstExpr { V = t.FloatSfx ? new CVal { T = CType.Float, D = (float)t.F } : CVal.Dbl(t.F), Line = line };
            case TokKind.Char:
                return new ConstExpr { V = CVal.Of(CType.Char, t.I), Line = line };
            case TokKind.String:
                return new StrLit { A = _ip.InternString(t.Text), T = CType.ArrayOf(CType.Char, t.Text.Length + 1), Text = t.Text, Line = line };
            case TokKind.Punct:
                if (t.Text == "(")
                {
                    var e = ParseExpr();
                    Expect(")", "para fechar o parêntese");
                    return e;
                }
                throw new CError(line, L.F("expressão esperada, mas encontrei '{0}'", t.Text), false);
            case TokKind.EOF:
                throw new CError(line, L.T("o código terminou no meio de uma expressão (faltou algo no final?)"), false);
        }
        // identifier
        string name = t.Text;
        if (name == "true") return new ConstExpr { V = CVal.Of(CType.Bool, 1), Line = line };
        if (name == "false") return new ConstExpr { V = CVal.Of(CType.Bool, 0), Line = line };
        if (Keywords.Contains(name)) throw new CError(line, L.F("palavra reservada '{0}' em lugar inesperado", name), false);
        var sym = _scope.Find(name);
        if (IsPunct("("))
        {
            if (sym is VarSym) throw new CError(line, L.F("'{0}' é uma variável, não uma função", name), false);
            _p++;
            var args = ParseArgs();
            var call = new CallExpr { Name = name, Args = args, Line = line };
            _calls.Add(call);
            return call;
        }
        switch (sym)
        {
            case VarSym v:
                if (v.ConstValue is long cv && v.T.IsInteger && !IsMutationNext())
                    return new ConstExpr { V = CVal.Of(v.T, cv), Line = line };
                return new IdentExpr { S = v, Line = line };
            case EnumSym en:
                return new ConstExpr { V = CVal.Int(en.Value), Line = line };
            case TypedefSym:
                throw new CError(line, L.F("'{0}' é um tipo, não um valor", name), false);
            case FuncSym:
                throw new CError(line, L.F("ponteiros para função não são suportados (faltaram os parênteses para chamar {0}()?)", name), false);
        }
        if (_ip.Natives.ContainsKey(name))
            throw new CError(line, L.F("'{0}' é uma função: chame com parênteses, {1}(...)", name, name), false);
        var names = new List<string>();
        for (var s = _scope; s != null; s = (s == _global ? null : FindParent(s)))
            names.AddRange(s.Syms.Where(kv => kv.Value is VarSym).Select(kv => kv.Key));
        var sug = Suggest(name, names);
        string msg = sug != null ? L.F("'{0}' não foi declarado(a) — você quis dizer '{1}'?", name, sug)
                                 : L.F("'{0}' não foi declarado(a) (declare a variável antes de usar, ex.: int {0};)", name);
        throw new CError(line, msg, false);
    }

    static Scope? FindParent(Scope s) => s.Parent;

    bool IsMutationNext()
    {
        var t = Peek();
        if (t.K != TokKind.Punct) return false;
        if (AssignOps.ContainsKey(t.Text) || t.Text == "++" || t.Text == "--") return true;
        // &CONST: the previous token is '&' used as unary
        return false;
    }

    // ---------------- constant evaluation ----------------
    long? ConstEval(Expr e)
    {
        switch (e)
        {
            case ConstExpr c: return c.V.T.IsFloat ? null : c.V.L;
            case IdentExpr id: return id.S.ConstValue;
            case UnaryExpr u:
                {
                    var v = ConstEval(u.E);
                    if (v == null) return null;
                    return u.Op switch { UnKind.Neg => -v, UnKind.Plus => v, UnKind.Not => v == 0 ? 1 : 0, _ => ~v };
                }
            case BinExpr b:
                {
                    var x = ConstEval(b.A);
                    var y = ConstEval(b.B);
                    if (x == null || y == null) return null;
                    try { return _ip.BinOp(b.Op, CVal.LongV(x.Value), CVal.LongV(y.Value), b.Line).L; }
                    catch (CError) { return null; }
                }
            case CondExpr c:
                {
                    var x = ConstEval(c.C);
                    if (x == null) return null;
                    return x != 0 ? ConstEval(c.A) : ConstEval(c.B);
                }
            case CastExpr c:
                {
                    var v = ConstEval(c.E);
                    return v == null || !c.T.IsInteger ? null : Conv.Norm(v.Value, c.T);
                }
            case LogicExpr l:
                {
                    var x = ConstEval(l.A);
                    var y = ConstEval(l.B);
                    if (x == null || y == null) return null;
                    return l.IsAnd ? (x != 0 && y != 0 ? 1 : 0) : (x != 0 || y != 0 ? 1 : 0);
                }
        }
        return null;
    }
}
