namespace EasyCreator.Exercises;

/// <summary>py-17 (parsing and evaluating expressions): evaluators of growing power, and predicting how Python reads precedence, unary minus and floor division.</summary>
[Exercises("py-17")]
public sealed class P17Parser : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(lv, r) : MakeOut(lv, r);
    }

    // ---- expression trees with Python semantics ----
    abstract record Node;
    sealed record Lit(long V) : Node;
    sealed record Neg(Node X) : Node;
    sealed record Bin(string Op, Node L, Node R) : Node;

    static int Prec(string op) => op switch { "+" or "-" => 1, "*" or "//" or "%" => 2, _ => 4 };
    static int PrecOf(Node n) => n switch { Bin b => Prec(b.Op), Neg => 3, _ => 5 };

    static long FloorDiv(long a, long b) { long q = a / b; if ((a % b != 0) && ((a < 0) != (b < 0))) q--; return q; }
    static long Eval(Node n) => n switch
    {
        Lit l => l.V,
        Neg g => -Eval(g.X),
        Bin b => b.Op switch { "+" => Eval(b.L) + Eval(b.R), "-" => Eval(b.L) - Eval(b.R), "*" => Eval(b.L) * Eval(b.R), "//" => FloorDiv(Eval(b.L), Eval(b.R)), "%" => Eval(b.L) - Eval(b.R) * FloorDiv(Eval(b.L), Eval(b.R)), _ => (long)Math.Pow(Eval(b.L), Eval(b.R)) },
        _ => 0,
    };

    static string Render(Node n, bool spaced = true)
    {
        string sp = spaced ? " " : "";
        switch (n)
        {
            case Lit l: return l.V.ToString();
            case Neg g: return g.X is Lit ? "-" + Render(g.X, spaced) : "-(" + Render(g.X, spaced) + ")";
            default:
                var b = (Bin)n;
                string Side(Node c, bool right)
                {
                    string s = Render(c, spaced);
                    int pc = PrecOf(c), pp = Prec(b.Op);
                    bool leftAssoc = b.Op != "**";
                    // unary minus as the base of ** needs parentheses; a negative literal too
                    if (b.Op == "**" && !right && (c is Neg || (c is Lit lt && lt.V < 0))) return "(" + s + ")";
                    bool needs = pc < pp || (pc == pp && (leftAssoc ? right : !right));
                    return needs ? "(" + s + ")" : s;
                }
                return Side(b.L, false) + sp + b.Op + sp + Side(b.R, true);
        }
    }

    static Node Gen(Random r, int depth, string[] ops, bool neg, bool allowPow)
    {
        if (depth == 0) return new Lit(r.Next(1, 10));
        string op = ops[r.Next(ops.Length)];
        if (op == "**") return new Bin("**", new Lit(r.Next(2, 5)), new Lit(r.Next(0, 4)));
        Node l = Gen(r, depth - 1, ops, neg, allowPow), rr = op is "//" or "%" ? new Lit(r.Next(1, 10)) : Gen(r, r.Next(depth), ops, neg, allowPow);
        if (neg && r.Next(5) == 0) l = new Neg(l);
        return new Bin(op, l, rr);
    }

    static string[] Mixed(Random r, int n, string[] ops, int depth, bool neg = false)
    {
        var l = new List<string>();
        while (l.Count < n) { var t = Gen(r, depth, ops, neg, true); if (t is Lit) continue; if (Math.Abs(Eval(t)) > 1_000_000) continue; l.Add(Render(t)); }
        return l.ToArray();
    }

    // ---- tiny reference parser for the code exercises (tokens already separated by spaces) ----
    static long ParseExpr(string[] t, ref int i, bool prec)
    {
        long v = prec ? ParseTerm(t, ref i) : ParseAtom(t, ref i);
        while (i < t.Length && (t[i] == "+" || t[i] == "-")) { string op = t[i++]; long w = prec ? ParseTerm(t, ref i) : ParseAtom(t, ref i); v = op == "+" ? v + w : v - w; }
        return v;
    }
    static long ParseTerm(string[] t, ref int i)
    {
        long v = ParseAtom(t, ref i);
        while (i < t.Length && (t[i] == "*" || t[i] == "//")) { string op = t[i++]; long w = ParseAtom(t, ref i); v = op == "*" ? v * w : FloorDiv(v, w); }
        return v;
    }
    static long ParseAtom(string[] t, ref int i)
    {
        if (t[i] == "(") { i++; long v = ParseExpr(t, ref i, true); i++; return v; }
        return long.Parse(t[i++]);
    }
    static long Calc(string line, bool prec) { var t = line.Trim().Split(' '); int i = 0; return ParseExpr(t, ref i, prec); }

    static string Flat(Random r, int terms, string[] ops, bool parens)
    {
        var parts = new List<string>();
        for (int k = 0; k < terms; k++) { parts.Add(r.Next(1, 10).ToString()); if (k < terms - 1) parts.Add(ops[r.Next(ops.Length)]); }
        if (parens && terms >= 3)
        {
            int a = r.Next(0, terms - 1) * 2, b = a + 2 * r.Next(1, (terms - a / 2));
            if (b >= parts.Count) b = parts.Count - 1;
            parts[a] = "( " + parts[a]; parts[b] = parts[b] + " )";
        }
        return string.Join(" ", parts).Replace("( ", "( ").Replace("  ", " ");
    }

    static Exercise MakeCode(int lv, Random r)
    {
        string rd = "linha = input()";
        string starter = rd + "\n" + Pg.Comment(false, L.T("escreva o resto do programa aqui"));
        switch (lv)
        {
            case 0:
            {
                string[] ops = { "+", "-", "*", "//", "%" };
                string Item() { string op = Ex.Pick(r, ops); int a = Ex.Int(r, 1, 99), b = Ex.Int(r, 1, 12); return $"{a} {op} {b}"; }
                long Do(string l) { var p = l.Trim().Split(' '); long a = long.Parse(p[0]), b = long.Parse(p[2]); return p[1] switch { "+" => a + b, "-" => a - b, "*" => a * b, "//" => a / b, _ => a % b }; }
                var tests = Pg.Tests(new[] { "7 * 6", "20 // 3", Item(), Item(), Item() }, i => Do(i).ToString());
                string sol = "linha = input()\na, op, b = linha.split()\na = int(a)\nb = int(b)\nif op == \"+\":\n    print(a + b)\nelif op == \"-\":\n    print(a - b)\nelif op == \"*\":\n    print(a * b)\nelif op == \"//\":\n    print(a // b)\nelse:\n    print(a % b)";
                return Ex.Code(Pg.Prompt(L.T("Leia uma linha no formato `<a> <op> <b>` (separados por espaço), com `op` sendo `+`, `-`, `*`, `//` ou `%` (b nunca é 0) e mostre o resultado, SEM usar `eval`."), tests[0]), starter, sol, tests,
                    L.T("Dividimos a linha em três pedaços (split), convertemos os números e escolhemos a conta pelo texto do operador com if/elif. É o menor avaliador possível."));
            }
            case 1:
            {
                var tests = Pg.Tests(new[] { "10 - 3 + 4 - 1", "5", Flat(r, Ex.Int(r, 2, 6), new[] { "+", "-" }, false), Flat(r, Ex.Int(r, 2, 6), new[] { "+", "-" }, false), Flat(r, Ex.Int(r, 3, 7), new[] { "+", "-" }, false) }, i => Calc(i, false).ToString());
                string sol = "linha = input()\nt = linha.split()\ntotal = int(t[0])\ni = 1\nwhile i < len(t):\n    n = int(t[i + 1])\n    if t[i] == \"+\":\n        total += n\n    else:\n        total -= n\n    i += 2\nprint(total)";
                return Ex.Code(Pg.Prompt(L.T("Leia uma linha com uma expressão só de inteiros positivos, `+` e `-`, tudo separado por espaços (por exemplo `10 - 3 + 4`). Calcule da esquerda para a direita e mostre o resultado, sem usar `eval`."), tests[0]), starter, sol, tests,
                    L.T("Separamos em tokens; o primeiro é o valor inicial e depois vêm pares (operador, número). Um laço de 2 em 2 aplica cada par ao total."));
            }
            case 2:
            {
                var tests = Pg.Tests(new[] { "2 + 3 * 4", "8 // 3 * 3 - 1", Flat(r, Ex.Int(r, 3, 6), new[] { "+", "-", "*", "//" }, false), Flat(r, Ex.Int(r, 3, 6), new[] { "+", "-", "*", "//" }, false), Flat(r, Ex.Int(r, 3, 7), new[] { "+", "-", "*" }, false) }, i => Calc(i, true).ToString());
                string sol = "linha = input()\nt = linha.split()\ni = 0\ndef termo():\n    global i\n    v = int(t[i])\n    i += 1\n    while i < len(t) and t[i] in (\"*\", \"//\"):\n        op = t[i]\n        n = int(t[i + 1])\n        i += 2\n        v = v * n if op == \"*\" else v // n\n    return v\ntotal = termo()\nwhile i < len(t):\n    op = t[i]\n    i += 1\n    n = termo()\n    total = total + n if op == \"+\" else total - n\nprint(total)";
                return Ex.Code(Pg.Prompt(L.T("Leia uma linha com inteiros positivos e os operadores `+`, `-`, `*` e `//` (divisão inteira), separados por espaços, SEM parênteses. Respeite a precedência (`*` e `//` antes de `+` e `-`) e mostre o resultado, sem usar `eval`. Os números dividem sempre por valores positivos."), tests[0]), starter, sol, tests,
                    L.T("Uma função `termo` lê um número e já resolve todos os `*` e `//` que vierem logo depois; o laço principal soma ou subtrai os termos. Assim a multiplicação \"gruda\" nos vizinhos antes da soma."));
            }
            default:
            {
                string[] tl = { "( 2 + 3 ) * 4", "2 * ( 3 + ( 4 - 1 ) )", Flat(r, Ex.Int(r, 4, 7), new[] { "+", "-", "*" }, true), Flat(r, Ex.Int(r, 4, 7), new[] { "+", "-", "*" }, true), Flat(r, Ex.Int(r, 4, 7), new[] { "+", "-", "*" }, true) };
                var tests = Pg.Tests(tl, i => Calc(i, true).ToString());
                string sol = "linha = input()\nt = linha.split()\ni = 0\ndef expr():\n    global i\n    v = termo()\n    while i < len(t) and t[i] in (\"+\", \"-\"):\n        op = t[i]\n        i += 1\n        w = termo()\n        v = v + w if op == \"+\" else v - w\n    return v\ndef termo():\n    global i\n    v = atomo()\n    while i < len(t) and t[i] == \"*\":\n        i += 1\n        v = v * atomo()\n    return v\ndef atomo():\n    global i\n    if t[i] == \"(\":\n        i += 1\n        v = expr()\n        i += 1\n        return v\n    v = int(t[i])\n    i += 1\n    return v\nprint(expr())";
                return Ex.Code(Pg.Prompt(L.T("Leia uma linha com inteiros positivos, `+`, `-`, `*` e parênteses, TUDO separado por espaços (inclusive os parênteses: `( 2 + 3 ) * 4`). Mostre o resultado respeitando a precedência e os parênteses, sem usar `eval`."), tests[0]), starter, sol, tests,
                    L.T("Três funções que se chamam, uma por nível de precedência: `expr` (soma/subtração) usa `termo` (multiplicação), que usa `atomo` (número ou um parêntese, que reabre `expr`). A recursão faz os parênteses voltarem ao nível mais baixo."));
            }
        }
    }

    static Exercise MakeOut(int lv, Random r)
    {
        var ops = lv switch { 0 => new[] { "+", "-", "*" }, 1 => new[] { "+", "-", "*", "//", "%" }, _ => new[] { "+", "-", "*", "//", "%", "**" } };
        var exprs = Mixed(r, 3, ops, lv == 0 ? 2 : 3, neg: lv >= 2);
        var vals = exprs.Select(e => { return e; }).ToArray();
        // re-evaluate by regenerating trees: keep (text, value) together
        var rs = new List<(string, long)>();
        for (int guard = 0; rs.Count < 3 && guard < 500; guard++)
        {
            var t = Gen(r, lv == 0 ? 2 : 3, ops, lv >= 2, true); if (t is Lit) continue; long v = Eval(t); if (Math.Abs(v) > 1_000_000) continue; rs.Add((Render(t), v));
        }
        string code = string.Join("\n", rs.Select(x => $"print({x.Item1})"));
        string outp = string.Join("\n", rs.Select(x => x.Item2));
        return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("Ordem de leitura do Python: parênteses; `**` (da direita para a esquerda); menos unário; `*`, `//` e `%` (da esquerda para a direita); por último `+` e `-`. Lembre que `//` e `%` arredondam para BAIXO, mesmo com números negativos.") + "\n"
            + string.Join("\n", rs.Select(x => $"`{x.Item1}` → {x.Item2}")));
    }
}
