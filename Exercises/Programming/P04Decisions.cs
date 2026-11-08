namespace EasyCreator.Exercises;

/// <summary>Lesson 4 (if / elif / else): thresholds, ladders, classification, boolean conditions, if vs elif.</summary>
[Exercises("py-04", "js-04")]
public sealed class P04Decisions : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        bool js = Pg.Js(lesson);
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(js, lv, r) : MakeOut(js, lv, r);
    }

    // ---------- code ----------
    static readonly string[][] LabelSets =
    {
        new[] { "Ótimo", "Bom", "Regular", "Ruim" }, new[] { "Calor", "Agradável", "Frio", "Gelado" }, new[] { "Gigante", "Grande", "Médio", "Pequeno" },
        new[] { "Veloz", "Normal", "Lento", "Parado" }, new[] { "Platina", "Ouro", "Prata", "Bronze" },
    };

    static string If(bool js, string cond, string body, string kw = "if")
    {
        if (js) return $"{(kw == "if" ? "if" : "else if")} ({cond}) {{\n  {body}\n}}";
        return $"{kw} {cond}:\n    {body}";
    }
    static string Else(bool js, string body) => js ? $"else {{\n  {body}\n}}" : $"else:\n    {body}";

    static Exercise MakeCode(bool js, int lv, Random r)
    {
        string pr(string s) => Pg.Pr(js, s);
        switch (lv)
        {
            case 0:
            {
                if (r.Next(2) == 0)
                {
                    int t = Ex.Int(r, 5, 60);
                    var set = LabelSets[r.Next(LabelSets.Length)];
                    string a = set[0], b = set[3];
                    var tests = Pg.Tests(new[] { "0", t.ToString(), (t - 1).ToString(), (t + Ex.Int(r, 1, 20)).ToString(), Ex.Int(r, 0, t - 2 > 0 ? t - 2 : 1).ToString() }, i => int.Parse(i.Trim()) >= t ? a : b);
                    var sol = Pg.Code(Pg.ReadInt(js, "x"), If(js, "x >= " + t, pr(a)), Else(js, pr(b)));
                    return Ex.Code(Pg.Prompt(L.F("Leia um inteiro `x`. Se `x` for maior ou igual a {0}, mostre `{1}`; caso contrário, mostre `{2}`.", t, a, b), tests[0]),
                        Pg.Starter(js, Pg.ReadInt(js, "x")), sol, tests,
                        L.T("Uma única decisão com dois caminhos: if (x ≥ limite) para o primeiro texto e else para todo o resto."));
                }
                else
                {
                    int k = Ex.Int(r, 2, 9);
                    string yes = Ex.Pick(r, "Múltiplo", "Divisível", "Sim"), no = Ex.Pick(r, "Não", "Resto", "Sobra");
                    var tests = Pg.Tests(new[] { "0", (k * 3).ToString(), (k * 3 + 1).ToString(), Ex.Int(r, 1, 99).ToString(), Ex.Int(r, 1, 99).ToString() }, i => int.Parse(i.Trim()) % k == 0 ? yes : no);
                    var sol = Pg.Code(Pg.ReadInt(js, "x"), If(js, $"x % {k} == 0", pr(yes)), Else(js, pr(no)));
                    return Ex.Code(Pg.Prompt(L.F("Leia um inteiro `x`. Se `x` for divisível por {0} (o resto da divisão é 0), mostre `{1}`; senão, mostre `{2}`.", k, yes, no), tests[0]),
                        Pg.Starter(js, Pg.ReadInt(js, "x")), sol, tests,
                        L.T("x é divisível por k quando x % k == 0. Note o == (comparar) em vez de = (guardar)."));
                }
            }
            case 1:
            {
                var set = LabelSets[r.Next(LabelSets.Length)];
                int t3 = Ex.Int(r, 10, 30), t2 = t3 + Ex.Int(r, 10, 25), t1 = t2 + Ex.Int(r, 10, 25);
                int max = Ex.Pick(r, 100, 120);
                string Model(int x) => x < 0 || x > max ? "Inválido" : x >= t1 ? set[0] : x >= t2 ? set[1] : x >= t3 ? set[2] : set[3];
                var tests = Pg.Tests(new[] { t1.ToString(), (t2 - 1).ToString(), "-3", (max + 1).ToString(), t3.ToString(), (t3 - 1).ToString(), Ex.Int(r, 0, max).ToString() }, i => Model(int.Parse(i.Trim())));
                var sol = Pg.Code(Pg.ReadInt(js, "x"), If(js, $"x < 0 {(js ? "||" : "or")} x > {max}", pr("Inválido")), If(js, "x >= " + t1, pr(set[0]), "elif"),
                    If(js, "x >= " + t2, pr(set[1]), "elif"), If(js, "x >= " + t3, pr(set[2]), "elif"), Else(js, pr(set[3])));
                return Ex.Code(Pg.Prompt(L.F("Leia um inteiro `x` e mostre uma única palavra: `Inválido` se `x` for menor que 0 ou maior que {0}; senão `{1}` se `x` ≥ {2}, `{3}` se `x` ≥ {4}, `{5}` se `x` ≥ {6} e `{7}` nos demais casos.", max, set[0], t1, set[1], t2, set[2], t3, set[3]), tests[0]),
                    Pg.Starter(js, Pg.ReadInt(js, "x")), sol, tests,
                    L.T("Primeiro eliminamos o caso inválido; depois uma escada elif, do limite mais alto para o mais baixo: o primeiro teste verdadeiro decide e os outros são ignorados."));
            }
            case 2:
            {
                int kind = r.Next(3);
                if (kind == 0)
                {
                    string Model(int a, int b, int c)
                    {
                        if (a >= b + c || b >= a + c || c >= a + b) return "Não é triângulo";
                        return a == b && b == c ? "Equilátero" : a == b || b == c || a == c ? "Isósceles" : "Escaleno";
                    }
                    var triples = new[] { (3, 4, 5), (5, 5, 5), (2, 2, 3), (1, 2, 3), (7, 3, 3), (6, 6, 12) }.OrderBy(_ => r.Next()).Take(3)
                        .Concat(Enumerable.Range(0, 3).Select(_ => (Ex.Int(r, 1, 12), Ex.Int(r, 1, 12), Ex.Int(r, 1, 12)))).ToArray();
                    var tests = Pg.Tests(triples.Select(t => $"{t.Item1}\n{t.Item2}\n{t.Item3}"), i => { var p = i.TrimEnd('\n').Split('\n').Select(int.Parse).ToArray(); return Model(p[0], p[1], p[2]); });
                    string or = js ? "||" : "or", and = js ? "&&" : "and";
                    var sol = Pg.Code(Pg.ReadInt(js, "a"), Pg.ReadInt(js, "b"), Pg.ReadInt(js, "c"),
                        If(js, $"a >= b + c {or} b >= a + c {or} c >= a + b", pr("Não é triângulo")), If(js, $"a == b {and} b == c", pr("Equilátero"), "elif"),
                        If(js, $"a == b {or} b == c {or} a == c", pr("Isósceles"), "elif"), Else(js, pr("Escaleno")));
                    return Ex.Code(Pg.Prompt(L.T("Leia três inteiros positivos (os lados de um triângulo, um por linha) e mostre uma linha: `Não é triângulo` se algum lado for maior ou igual à soma dos outros dois; senão `Equilátero` (3 lados iguais), `Isósceles` (exatamente 2 iguais) ou `Escaleno` (todos diferentes)."), tests[0]),
                        Pg.Starter(js, Pg.ReadInt(js, "a"), Pg.ReadInt(js, "b"), Pg.ReadInt(js, "c")), sol, tests,
                        L.T("Testamos a existência primeiro (cada lado precisa ser menor que a soma dos outros dois). Depois, na ordem: três iguais, algum par igual, senão todos diferentes — a ordem importa, pois equilátero também tem pares iguais."));
                }
                if (kind == 1)
                {
                    bool Leap(int y) => y % 400 == 0 || (y % 4 == 0 && y % 100 != 0);
                    var tests = Pg.Tests(new[] { "2000", "1900", "2024", "2023", Ex.Int(r, 1600, 2100).ToString(), Ex.Int(r, 1600, 2100).ToString() }, i => Leap(int.Parse(i.Trim())) ? "Bissexto" : "Comum");
                    string or = js ? "||" : "or", and = js ? "&&" : "and";
                    var sol = Pg.Code(Pg.ReadInt(js, "ano"), If(js, $"ano % 400 == 0 {or} (ano % 4 == 0 {and} ano % 100 != 0)", pr("Bissexto")), Else(js, pr("Comum")));
                    return Ex.Code(Pg.Prompt(L.T("Leia um ano e mostre `Bissexto` ou `Comum`. Um ano é bissexto se é divisível por 4 e não por 100, ou se é divisível por 400."), tests[0]),
                        Pg.Starter(js, Pg.ReadInt(js, "ano")), sol, tests,
                        L.T("A regra pede combinar condições: (divisível por 4 E NÃO por 100) OU divisível por 400. 1900 não é bissexto (100 corta), 2000 é (400 salva)."));
                }
                {
                    int a = Ex.Int(r, 2, 5), b = Ex.Int(r, 6, 9);
                    string la = Ex.Pick(r, "Zip", "Tic", "Bum"), lb = Ex.Pick(r, "Zap", "Tac", "Pow");
                    string Model(int n) => n % a == 0 && n % b == 0 ? la + lb : n % a == 0 ? la : n % b == 0 ? lb : n.ToString();
                    int lcm = a * b / Gcd(a, b);
                    var tests = Pg.Tests(new[] { (lcm * 2).ToString(), lcm.ToString(), a.ToString(), b.ToString(), (lcm + 1).ToString(), Ex.Int(r, 1, 200).ToString() }, i => Model(int.Parse(i.Trim())));
                    string and = js ? "&&" : "and";
                    var sol = Pg.Code(Pg.ReadInt(js, "n"), If(js, $"n % {a} == 0 {and} n % {b} == 0", pr(la + lb)), If(js, $"n % {a} == 0", pr(la), "elif"), If(js, $"n % {b} == 0", pr(lb), "elif"),
                        Else(js, js ? "console.log(n);" : "print(n)"));
                    return Ex.Code(Pg.Prompt(L.F("Leia um inteiro `n`. Mostre `{0}` se `n` for múltiplo de {1}, `{2}` se for múltiplo de {3} e `{4}` se for múltiplo dos dois; se não for múltiplo de nenhum, mostre o próprio `n`.", la, a, lb, b, la + lb), tests[0]),
                        Pg.Starter(js, Pg.ReadInt(js, "n")), sol, tests,
                        L.T("O caso mais específico (múltiplo dos dois) precisa vir PRIMEIRO; se viesse depois, o teste de um só múltiplo já o capturaria."));
                }
            }
            default:
            {
                int kind = r.Next(3);
                if (kind == 0)
                {
                    var tests = Pg.Tests(new[] { "4\n4\n4", "3\n8\n3", "5\n1\n9", "9\n9\n1", Pg.Sp(Pg.Arr(r, 3, 1, 50)).Replace(' ', '\n'), Pg.Sp(Pg.Arr(r, 3, 1, 50)).Replace(' ', '\n') },
                        i => { var p = i.TrimEnd('\n').Split('\n').Select(int.Parse).OrderBy(x => x).ToArray(); return p[1].ToString(); });
                    string[] rd = { Pg.ReadInt(js, "a"), Pg.ReadInt(js, "b"), Pg.ReadInt(js, "c") };
                    string sum = js ? "console.log(a + b + c - Math.max(a, b, c) - Math.min(a, b, c));" : "print(a + b + c - max(a, b, c) - min(a, b, c))";
                    return Ex.Code(Pg.Prompt(L.T("Leia três inteiros (um por linha) e mostre o do MEIO, isto é, o que ficaria na posição central se os três fossem colocados em ordem crescente (se houver repetidos, o valor repetido conta normalmente)."), tests[0]),
                        Pg.Starter(js, rd), Pg.Code(rd.Append(sum).ToArray()), tests,
                        L.T("Não é preciso ordenar: a soma dos três menos o maior e menos o menor sobra exatamente o valor do meio."));
                }
                if (kind == 1)
                {
                    string Model(int a, int b, int c, int d) { int lo = Math.Max(a, c), hi = Math.Min(b, d); return lo <= hi ? (hi - lo).ToString() : "Sem interseção"; }
                    var ins = new List<string> { "1\n5\n3\n9", "1\n2\n5\n6", "4\n8\n1\n4" };
                    for (int i = 0; i < 3; i++) { int a = Ex.Int(r, 0, 20), b = a + Ex.Int(r, 0, 15), c = Ex.Int(r, 0, 20), d2 = c + Ex.Int(r, 0, 15); ins.Add($"{a}\n{b}\n{c}\n{d2}"); }
                    var tests = Pg.Tests(ins, i => { var p = i.TrimEnd('\n').Split('\n').Select(int.Parse).ToArray(); return Model(p[0], p[1], p[2], p[3]); });
                    string[] rd = { Pg.ReadInt(js, "a"), Pg.ReadInt(js, "b"), Pg.ReadInt(js, "c"), Pg.ReadInt(js, "d") };
                    var sol = Pg.Code(rd.Concat(new[] { js ? "const ini = Math.max(a, c);" : "ini = max(a, c)", js ? "const fim = Math.min(b, d);" : "fim = min(b, d)",
                        If(js, "ini <= fim", js ? "console.log(fim - ini);" : "print(fim - ini)"), Else(js, pr("Sem interseção")) }).ToArray());
                    return Ex.Code(Pg.Prompt(L.T("Leia quatro inteiros `a`, `b`, `c`, `d` (um por linha) que formam dois intervalos `[a, b]` e `[c, d]`, com a ≤ b e c ≤ d. Mostre o comprimento da parte em comum dos intervalos (zero se só se tocam) ou `Sem interseção` se não têm ponto em comum."), tests[0]),
                        Pg.Starter(js, rd), sol, tests,
                        L.T("A parte comum começa no maior dos inícios e termina no menor dos fins. Se o começo passa do fim, não há interseção; senão o comprimento é fim − início."));
                }
                {
                    int[] dm = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
                    var ins = new List<string> { "28\n2", "31\n12", "15\n6", "30\n4" };
                    for (int i = 0; i < 3; i++) { int m = Ex.Int(r, 1, 12), day = Ex.Int(r, 1, dm[m - 1]); ins.Add($"{day}\n{m}"); }
                    var tests = Pg.Tests(ins, i => { var p = i.TrimEnd('\n').Split('\n').Select(int.Parse).ToArray(); int day = p[0], m = p[1];
                        if (day < dm[m - 1]) return $"{day + 1}/{m}"; return m == 12 ? "1/1" : $"1/{m + 1}"; });
                    string dlist = "[31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31]";
                    string lenExpr = js ? $"{dlist}[m - 1]" : $"{dlist}[m - 1]";
                    var sol = Pg.Code(Pg.ReadInt(js, "dia"), Pg.ReadInt(js, "m"), If(js, $"dia < {lenExpr}", pr("{dia + 1}/{m}")), If(js, "m == 12", pr("1/1"), "elif"), Else(js, pr("1/{m + 1}")));
                    return Ex.Code(Pg.Prompt(L.T("Leia o dia e o mês (um por linha) de uma data de um ano que NÃO é bissexto (fevereiro tem 28 dias) e mostre o dia seguinte no formato `dia/mês`."), tests[0]),
                        Pg.Starter(js, Pg.ReadInt(js, "dia"), Pg.ReadInt(js, "m")), sol, tests,
                        L.T("Uma lista com os tamanhos dos meses evita doze ifs: se o dia ainda é menor que o tamanho do mês, só soma 1; senão vira o dia 1 do mês seguinte, e dezembro volta para janeiro."));
                }
            }
        }
    }

    static int Gcd(int a, int b) => b == 0 ? a : Gcd(b, a % b);

    // ---------- output ----------
    abstract record B;
    sealed record Cmp(string V, string Op, int K) : B;
    sealed record Bo(bool And, B L, B R) : B;
    sealed record Nt(B X) : B;

    static bool Ev(B b, Dictionary<string, int> env) => b switch
    {
        Cmp c => c.Op switch { ">" => env[c.V] > c.K, "<" => env[c.V] < c.K, ">=" => env[c.V] >= c.K, "==" => env[c.V] == c.K, "!=" => env[c.V] != c.K, _ => env[c.V] <= c.K },
        Bo o => o.And ? Ev(o.L, env) && Ev(o.R, env) : Ev(o.L, env) || Ev(o.R, env),
        Nt n => !Ev(n.X, env),
        _ => false,
    };

    static string Rd(B b, bool js, bool top = true)
    {
        switch (b)
        {
            case Cmp c: return $"{c.V} {c.Op} {c.K}";
            case Nt n: return (js ? "!" : "not ") + "(" + Rd(n.X, js) + ")";
            default:
                var o = (Bo)b;
                string s = Rd(o.L, js, false) + (o.And ? (js ? " && " : " and ") : (js ? " || " : " or ")) + Rd(o.R, js, false);
                return top ? s : "(" + s + ")";
        }
    }

    static B RandCmp(Random r, string[] vars) => new Cmp(Ex.Pick(r, vars), Ex.Pick(r, ">", "<", ">=", "==", "!=", "<="), Ex.Int(r, 1, 9));

    static Exercise MakeOut(bool js, int lv, Random r)
    {
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        if (lv == 3) return IfVsElif(js, r);
        if (lv == 2)
        {
            var vars = new[] { "a", "b", "c" };
            var env = vars.ToDictionary(v => v, _ => Ex.Int(r, 1, 9));
            var lines = new List<string> { js ? $"const a = {env["a"]}, b = {env["b"]}, c = {env["c"]};" : $"a = {env["a"]}; b = {env["b"]}; c = {env["c"]}" };
            var outs = new List<string>(); var expl = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                B cond = r.Next(3) switch
                {
                    0 => new Bo(true, RandCmp(r, vars), RandCmp(r, vars)),
                    1 => new Bo(false, RandCmp(r, vars), RandCmp(r, vars)),
                    _ => new Bo(r.Next(2) == 0, new Nt(RandCmp(r, vars)), RandCmp(r, vars)),
                };
                string s = Rd(cond, js);
                bool v = Ev(cond, env);
                lines.Add(js ? $"if ({s}) {{ console.log(\"sim{i + 1}\"); }} else {{ console.log(\"nao{i + 1}\"); }}" : $"if {s}:\n    print(\"sim{i + 1}\")\nelse:\n    print(\"nao{i + 1}\")");
                outs.Add((v ? "sim" : "nao") + (i + 1));
                expl.Add($"`{s}` → {(v ? (js ? "true" : "True") : (js ? "false" : "False"))}");
            }
            string outp = string.Join("\n", outs);
            return Ex.Output(PgOut.Prompt(outp), string.Join("\n", lines), outp,
                L.T("Calcule cada condição trocando as variáveis pelos valores: E (and / &&) só é verdadeiro se os dois lados são; OU (or / ||) basta um; NÃO inverte.") + "\n" + string.Join("\n", expl)
                + "\n" + L.F("Valores: a = {0}, b = {1}, c = {2}.", env["a"], env["b"], env["c"]));
        }
        // lv 0/1: if / else or a ladder on x
        {
            int x = Ex.Int(r, 0, 30);
            int n = lv == 0 ? 1 : 3;
            var ts = Enumerable.Range(0, n).Select(_ => Ex.Int(r, 0, 30)).OrderByDescending(t => t).Distinct().ToList();
            string[] words = Pg.Words.OrderBy(_ => r.Next()).Take(ts.Count + 1).ToArray();
            var lines = new List<string> { js ? $"let x = {x};" : $"x = {x}" };
            string res = words[^1];
            bool found = false;
            for (int i = 0; i < ts.Count; i++)
            {
                lines.Add(If(js, "x > " + ts[i], pr($"\"{words[i]}\""), i == 0 ? "if" : "elif"));
                if (!found && x > ts[i]) { res = words[i]; found = true; }
            }
            lines.Add(Else(js, pr($"\"{words[^1]}\"")));
            bool bump = lv == 1 && r.Next(2) == 0;
            var outs = new List<string> { res };
            if (bump)
            {
                lines.Add(js ? "x = x + 10;" : "x = x + 10"); lines.Add(If(js, "x > " + ts[0], pr($"\"{words[0]}\"")));
                if (x + 10 > ts[0]) outs.Add(words[0]);
            }
            lines.Add(pr("\"fim\"")); outs.Add("fim");
            string outp = string.Join("\n", outs);
            return Ex.Output(PgOut.Prompt(outp), string.Join("\n", lines), outp,
                L.F("Com x = {0}, os testes vão de cima para baixo e só o primeiro verdadeiro executa (um else/elif não é lido depois disso). O print final fica fora do if e roda sempre.", x));
        }
    }

    // if and elif mixed: a separate `if` is evaluated even after an earlier chain matched.
    static Exercise IfVsElif(bool js, Random r)
    {
        int a = Ex.Int(r, 2, 4), b = Ex.Int(r, 3, 6), c = Ex.Int(r, 5, 12);
        var xs = Enumerable.Range(0, 4).Select(_ => Ex.Int(r, 1, 30)).ToArray();
        var outs = new List<string>();
        foreach (int n in xs)
        {
            if (n % a == 0) outs.Add($"{n} A"); else if (n % b == 0) outs.Add($"{n} B");
            if (n > c) outs.Add($"{n} grande");
        }
        if (outs.Count == 0) { xs[0] = c + 1; outs.Add($"{xs[0]} grande"); if (xs[0] % a == 0) outs.Insert(0, $"{xs[0]} A"); else if (xs[0] % b == 0) outs.Insert(0, $"{xs[0]} B"); }
        outs.Clear();
        foreach (int n in xs)
        {
            if (n % a == 0) outs.Add($"{n} A"); else if (n % b == 0) outs.Add($"{n} B");
            if (n > c) outs.Add($"{n} grande");
        }
        if (outs.Count == 0) outs.Add("nada");
        string list = string.Join(", ", xs);
        string code = js
            ? $"for (const n of [{list}]) {{\n  if (n % {a} == 0) {{\n    console.log(n, \"A\");\n  }} else if (n % {b} == 0) {{\n    console.log(n, \"B\");\n  }}\n  if (n > {c}) {{\n    console.log(n, \"grande\");\n  }}\n}}"
            : $"for n in [{list}]:\n    if n % {a} == 0:\n        print(n, \"A\")\n    elif n % {b} == 0:\n        print(n, \"B\")\n    if n > {c}:\n        print(n, \"grande\")";
        if (outs.Count == 1 && outs[0] == "nada") code += "\n" + (js ? "console.log(\"nada\");" : "print(\"nada\")");
        string outp = string.Join("\n", outs);
        return Ex.Output(PgOut.Prompt(outp), code, outp,
            L.F("Para cada n: a cadeia if/elif escolhe NO MÁXIMO um entre A (múltiplo de {0}) e B (múltiplo de {1}); o segundo `if` (n > {2}) é independente e também pode escrever, mesmo depois de A ou B.", a, b, c));
    }
}
