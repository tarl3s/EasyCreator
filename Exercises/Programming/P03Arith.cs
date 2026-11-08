using System.Globalization;

namespace EasyCreator.Exercises;

/// <summary>Lesson 3 (input and arithmetic): + - * // %, unit conversions, digit tricks, precedence and negative-number quirks.</summary>
[Exercises("py-03", "js-03")]
public sealed class P03Arith : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        bool js = Pg.Js(lesson);
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(js, lv, r) : (lv == 3 ? Negative(js, r) : Precedence(js, lv, r));
    }

    record Op(string Label, string Py, string Js, Func<int, int, int> F);

    static readonly Op[] Ops =
    {
        new("Soma", "a + b", "a + b", (a, b) => a + b), new("Diferença", "a - b", "a - b", (a, b) => a - b), new("Produto", "a * b", "a * b", (a, b) => a * b),
        new("Divisão inteira", "a // b", "Math.trunc(a / b)", (a, b) => a / b), new("Resto", "a % b", "a % b", (a, b) => a % b),
    };

    record Unit(string Small, string Big, int K, string One);
    static readonly Unit[] Units =
    {
        new("minutos", "horas", 60, "hora"), new("dias", "semanas", 7, "semana"), new("centavos", "reais", 100, "real"), new("ovos", "dúzias", 12, "dúzia"),
        new("segundos", "minutos", 60, "minuto"), new("meses", "anos", 12, "ano"), new("gramas", "quilos", 1000, "quilo"),
    };

    static Exercise MakeCode(bool js, int lv, Random r)
    {
        switch (lv)
        {
            case 0:
            {
                var chosen = Ops.OrderBy(_ => r.Next()).Take(3).ToArray();
                var tests = Pg.Tests(new[] { "17\n5", "9\n2", Ex.Int(r, 20, 90) + "\n" + Ex.Int(r, 2, 9), Ex.Int(r, 20, 90) + "\n" + Ex.Int(r, 2, 9), "7\n7" },
                    i => { var p = i.TrimEnd('\n').Split('\n'); int a = int.Parse(p[0]), b = int.Parse(p[1]); return string.Join("\n", chosen.Select(o => $"{o.Label}: {o.F(a, b)}")); });
                var sol = Pg.Code(new[] { Pg.ReadInt(js, "a"), Pg.ReadInt(js, "b") }.Concat(chosen.Select(o => Pg.Pr(js, o.Label + ": {" + (js ? o.Js : o.Py) + "}"))).ToArray());
                return Ex.Code(Pg.Prompt(L.T("Leia dois inteiros positivos `a` e `b` (um por linha, `b` nunca é zero) e mostre uma linha para cada item, nesta ordem:") + "\n"
                    + Pg.Plain(string.Join("\n", chosen.Select(o => o.Label + ": <valor>"))) + "\n" + L.T("(Divisão inteira é o quociente sem a parte decimal; Resto é o que sobra da divisão.)"), tests[0]),
                    Pg.Starter(js, Pg.ReadInt(js, "a"), Pg.ReadInt(js, "b")), sol, tests,
                    L.T("Lemos os dois números já convertidos para inteiro e fazemos cada conta. A divisão inteira usa // (Python) ou Math.trunc(a / b) (JavaScript); o resto usa %."));
            }
            case 1:
            {
                var u = Ex.Pick(r, Units);
                var tests = Pg.Tests(new[] { (u.K * 2 + 3).ToString(), (u.K - 1).ToString(), Ex.Int(r, u.K, u.K * 20).ToString(), (u.K * 5).ToString(), Ex.Int(r, 0, u.K * 9).ToString() },
                    i => { int v = int.Parse(i.Trim()); return $"{Pg.Cap(u.Big)}: {v / u.K}\nSobram: {v % u.K} {u.Small}"; });
                var sol = Pg.Code(Pg.ReadInt(js, "total"), Pg.Pr(js, Pg.Cap(u.Big) + ": {" + Pg.IDiv(js, "total", u.K.ToString()) + "}"), Pg.Pr(js, $"Sobram: {{total % {u.K}}} {u.Small}"));
                return Ex.Code(Pg.Prompt(L.F("Leia uma quantidade inteira de {0} e converta para {1}: mostre quantas {1} inteiras cabem e quantos {0} sobram. Como 1 {2} tem {3} {0}:", L.T(u.Small), L.T(u.Big), L.T(u.One), u.K)
                    + "\n" + Pg.Plain($"{Pg.Cap(u.Big)}: <inteiras>\nSobram: <resto> {u.Small}"), tests[0]),
                    Pg.Starter(js, Pg.ReadInt(js, "total")), sol, tests,
                    L.T("A divisão inteira conta quantos grupos completos existem; o resto (%) é o que sobrou fora dos grupos."));
            }
            case 2:
            {
                if (r.Next(2) == 0)
                {
                    var tests = Pg.Tests(new[] { "3600", "3725", "59", "86399", Ex.Int(r, 100, 9000).ToString(), Ex.Int(r, 1, 80000).ToString() },
                        i => { int s = int.Parse(i.Trim()); return $"{s / 3600}h {s % 3600 / 60}min {s % 60}s"; });
                    var sol = Pg.Code(Pg.ReadInt(js, "s"), Pg.Pr(js, "{" + Pg.IDiv(js, "s", "3600") + "}h {" + Pg.IDiv(js, "s % 3600", "60") + "}min {s % 60}s"));
                    return Ex.Code(Pg.Prompt(L.T("Leia um tempo em segundos (inteiro) e mostre-o no formato `Hh Mmin Ss` (horas, minutos e segundos que sobram, sem zeros à esquerda)."), tests[0]),
                        Pg.Starter(js, Pg.ReadInt(js, "s")), sol, tests,
                        L.T("Horas = s ÷ 3600 (divisão inteira). Do que sobra (s % 3600), os minutos são ÷ 60 e os segundos são o resto de s por 60."));
                }
                else
                {
                    var tests = Pg.Tests(new[] { "10\n7\n4", "1\n2\n2", Pg.Sp(Pg.Arr(r, 3, 0, 30)).Replace(' ', '\n'), Pg.Sp(Pg.Arr(r, 3, 0, 30)).Replace(' ', '\n') },
                        i => { var p = i.TrimEnd('\n').Split('\n').Select(int.Parse).ToArray(); int s = p.Sum();
                            decimal avg = Math.Round((decimal)s / 3, 2, MidpointRounding.AwayFromZero);
                            return $"Soma: {s}\nMédia: {avg.ToString("0.00", CultureInfo.InvariantCulture)}\nAmplitude: {p.Max() - p.Min()}"; });
                    string[] rd = { Pg.ReadInt(js, "a"), Pg.ReadInt(js, "b"), Pg.ReadInt(js, "c") };
                    var sol = js
                        ? Pg.Code(rd.Concat(new[] { "console.log(`Soma: ${a + b + c}`);", "console.log(`Média: ${((a + b + c) / 3).toFixed(2)}`);", "console.log(`Amplitude: ${Math.max(a, b, c) - Math.min(a, b, c)}`);" }).ToArray())
                        : Pg.Code(rd.Concat(new[] { "print(f\"Soma: {a + b + c}\")", "print(f\"Média: {(a + b + c) / 3:.2f}\")", "print(f\"Amplitude: {max(a, b, c) - min(a, b, c)}\")" }).ToArray());
                    return Ex.Code(Pg.Prompt(L.T("Leia três inteiros (um por linha) e mostre a soma, a média com 2 casas decimais e a amplitude (maior menos menor):") + "\n"
                        + Pg.Plain("Soma: <soma>\nMédia: <média>\nAmplitude: <maior - menor>"), tests[0]),
                        Pg.Starter(js, rd), sol, tests,
                        L.T("A média é a soma dividida por 3 (divisão real, mostrada com 2 casas). A amplitude usa o maior e o menor valor: max − min."));
                }
            }
            default:
            {
                var tests = Pg.Tests(new[] { "100", "123", "400", "907", Ex.Int(r, 100, 999).ToString(), Ex.Int(r, 100, 999).ToString() },
                    i => { var s = i.Trim(); return int.Parse(new string(s.Reverse().ToArray())).ToString(); });
                string expr = js ? "(n % 10) * 100 + (Math.trunc(n / 10) % 10) * 10 + Math.trunc(n / 100)" : "(n % 10) * 100 + (n // 10 % 10) * 10 + n // 100";
                var sol = Pg.Code(Pg.ReadInt(js, "n"), js ? $"console.log({expr});" : $"print({expr})");
                return Ex.Code(Pg.Prompt(L.T("Leia um número inteiro de 3 dígitos e mostre o número escrito ao contrário, como número (123 vira 321; 400 vira 4, porque os zeros à esquerda somem). Use só contas, sem transformar em texto."), tests[0]),
                    Pg.Starter(js, Pg.ReadInt(js, "n")), sol, tests,
                    L.T("Os dígitos saem com divisão e resto: n % 10 é a unidade, (n ÷ 10) % 10 é a dezena e n ÷ 100 é a centena. O invertido é unidade·100 + dezena·10 + centena."));
            }
        }
    }

    // ---- predicting expressions ----
    abstract record Node;
    sealed record Var(string Name) : Node;
    sealed record Bin(char Op, Node L, Node R) : Node;

    static int Eval(Node n, Dictionary<string, int> env) => n switch
    {
        Var v => env[v.Name],
        Bin b => b.Op switch
        {
            '+' => Eval(b.L, env) + Eval(b.R, env), '-' => Eval(b.L, env) - Eval(b.R, env), '*' => Eval(b.L, env) * Eval(b.R, env),
            '/' => Eval(b.L, env) / Eval(b.R, env), _ => Eval(b.L, env) % Eval(b.R, env),
        },
        _ => 0,
    };

    static int Prec(Node n) => n is Bin b ? (b.Op is '+' or '-' ? 1 : 2) : 3;

    static string Render(Node n, bool js)
    {
        if (n is Var v) return v.Name;
        var b = (Bin)n;
        if (b.Op == (char)47 && js) { string W(Node c) { var s = Render(c, js); return c is Bin cb && cb.Op != (char)47 ? "(" + s + ")" : s; } return $"Math.trunc({W(b.L)} / {W(b.R)})"; }
        string Side(Node c, bool right) { var s = Render(c, js); int pc = c is Bin cb && cb.Op == '/' && js ? 3 : Prec(c); int pp = Prec(b); return pc < pp || (right && pc == pp) ? "(" + s + ")" : s; }
        string op = b.Op == '/' ? "//" : b.Op.ToString();
        return $"{Side(b.L, false)} {op} {Side(b.R, true)}";
    }

    static Node Gen(Random r, string[] vars, int depth, bool divs)
    {
        if (depth == 0) return new Var(Ex.Pick(r, vars));
        char op = divs ? Ex.Pick(r, '+', '-', '*', '/', '%') : Ex.Pick(r, '+', '-', '*');
        Node l = Gen(r, vars, depth - 1, divs), rr = op is '/' or '%' ? new Var(Ex.Pick(r, vars)) : Gen(r, vars, r.Next(depth), divs);
        return new Bin(op, l, rr);
    }

    static Exercise Precedence(bool js, int lv, Random r)
    {
        var vars = new[] { "a", "b", "c" };
        var env = new Dictionary<string, int> { ["a"] = Ex.Int(r, 2, 12), ["b"] = Ex.Int(r, 2, 12), ["c"] = Ex.Int(r, 2, 12) };
        var lines = new List<string> { js ? $"const a = {env["a"]}, b = {env["b"]}, c = {env["c"]};" : $"a = {env["a"]}; b = {env["b"]}; c = {env["c"]}" };
        var outs = new List<string>(); var expl = new List<string>();
        int count = lv == 0 ? 3 : 4;
        for (int tries = 0; outs.Count < count && tries < 400; tries++)
        {
            var n = Gen(r, vars, lv == 0 ? 2 : 3, lv >= 1);
            if (n is Var) continue;
            // the right side of / and % must not be zero and the left side must be >= 0 (floor and trunc agree then)
            bool nz = false; D(n, env, ref nz); if (nz || !Safe(n, env)) continue;
            string s = Render(n, js);
            lines.Add(js ? $"console.log({s});" : $"print({s})");
            outs.Add(Eval(n, env).ToString());
            expl.Add($"`{s}` → {outs[^1]}");
        }
        if (outs.Count == 0) { outs.Add("0"); lines.Add(js ? "console.log(0);" : "print(0)"); expl.Add("0"); }
        string joined = string.Join("\n", outs);
        return Ex.Output(PgOut.Prompt(joined), string.Join("\n", lines), joined,
            L.T("Primeiro vêm os parênteses, depois *, // (ou Math.trunc) e %, e só no fim + e −; operadores do mesmo nível vão da esquerda para a direita.") + "\n"
            + string.Join("\n", expl) + "\n" + L.F("Valores: a = {0}, b = {1}, c = {2}.", env["a"], env["b"], env["c"]));
    }

    // JavaScript prints -0 for things like (-3) * 0; flag those so the expected output is never "-0".
    static double D(Node n, Dictionary<string, int> env, ref bool nz)
    {
        if (n is Var v) return env[v.Name];
        var b = (Bin)n; double l = D(b.L, env, ref nz), rr = D(b.R, env, ref nz);
        double res = b.Op switch { (char)43 => l + rr, (char)45 => l - rr, (char)42 => l * rr, (char)47 => rr == 0 ? 0 : Math.Truncate(l / rr), _ => rr == 0 ? 0 : l % rr };
        if (res == 0 && double.IsNegative(res)) nz = true;
        return res;
    }

    static bool Safe(Node n, Dictionary<string, int> env)
    {
        if (n is not Bin b) return true;
        if (!Safe(b.L, env) || !Safe(b.R, env)) return false;
        if (b.Op is '/' or '%') return Eval(b.R, env) > 0 && Eval(b.L, env) >= 0;
        return true;
    }

    static Exercise Negative(bool js, Random r)
    {
        int a, b; do { a = Ex.Int(r, 3, 20); b = Ex.Int(r, 2, 6); } while (a <= b || a % b == 0);
        var rows = new List<(string py, string jsCode, int pyV, int jsV)>
        {
            ($"-{a} // {b}", $"Math.trunc(-{a} / {b})", Pg.FloorDiv(-a, b), -(a / b)),
            ($"-{a} % {b}", $"-{a} % {b}", Pg.FloorMod(-a, b), -(a % b)),
            ($"{a} % -{b}", $"{a} % -{b}", Pg.FloorMod(a, -b), a % b),
            ($"{a} // -{b}", $"Math.trunc({a} / -{b})", Pg.FloorDiv(a, -b), -(a / b)),
            ($"(-{a}) * (-{b}) - {b}", $"(-{a}) * (-{b}) - {b}", a * b - b, a * b - b),
            ($"2 ** 3 ** 2", "2 ** 3 ** 2", 512, 512),
        };
        var pick = rows.OrderBy(_ => r.Next()).Take(4).ToArray();
        string outp = string.Join("\n", pick.Select(x => js ? x.jsV : x.pyV));
        return Ex.Output(PgOut.Prompt(outp, L.T("Cuidado com os números negativos: Python e JavaScript não arredondam a divisão do mesmo jeito.")),
            string.Join("\n", pick.Select(x => js ? $"console.log({x.jsCode});" : $"print({x.py})")), outp,
            L.T("Em Python, // arredonda para BAIXO (−7 // 2 = −4) e o resto % tem o sinal do divisor. Em JavaScript, Math.trunc corta a parte decimal (−7 / 2 → −3) e o % tem o sinal do número da esquerda. A potência ** associa da direita para a esquerda: 2 ** 3 ** 2 = 2 ** 9.") + "\n"
            + string.Join("\n", pick.Select(x => $"`{(js ? x.jsCode : x.py)}` → {(js ? x.jsV : x.pyV)}")));
    }
}
