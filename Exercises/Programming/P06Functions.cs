namespace EasyCreator.Exercises;

/// <summary>Lesson 6 (functions): writing small functions from a description, and tracing calls, return values, defaults and scope.</summary>
[Exercises("py-06", "js-06")]
public sealed class P06Functions : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        bool js = Pg.Js(lesson);
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(js, lv, r) : MakeOut(js, lv, r);
    }

    record Fn(string Py, string Js, string[] Ps, string Desc, string[] PyBody, string[] JsBody, Func<long[], long> F, Func<Random, long[]> Args, bool Bool = false);

    static long Gcd(long a, long b) => b == 0 ? a : Gcd(b, a % b);
    static bool Prime(long n) { if (n < 2) return false; for (long d = 2; d * d <= n; d++) if (n % d == 0) return false; return true; }

    static readonly Fn[] Simple =
    {
        new("dobro", "dobro", new[] { "n" }, "o dobro de `n`", new[] { "return n * 2" }, new[] { "return n * 2;" }, a => a[0] * 2, r => new long[] { Ex.Int(r, 0, 60) }),
        new("quadrado", "quadrado", new[] { "n" }, "o quadrado de `n`", new[] { "return n * n" }, new[] { "return n * n;" }, a => a[0] * a[0], r => new long[] { Ex.Int(r, 0, 40) }),
        new("eh_par", "ehPar", new[] { "n" }, "`True` (`true`) se `n` for par e `False` (`false`) se for ímpar", new[] { "return n % 2 == 0" }, new[] { "return n % 2 === 0;" }, a => a[0] % 2 == 0 ? 1 : 0, r => new long[] { Ex.Int(r, 0, 99) }, true),
        new("triplo_mais_um", "triploMaisUm", new[] { "n" }, "o triplo de `n` mais 1", new[] { "return 3 * n + 1" }, new[] { "return 3 * n + 1;" }, a => 3 * a[0] + 1, r => new long[] { Ex.Int(r, 0, 60) }),
    };

    static readonly Fn[] Two =
    {
        new("maior", "maior", new[] { "a", "b" }, "o maior entre `a` e `b`", new[] { "if a > b:", "    return a", "return b" }, new[] { "return a > b ? a : b;" }, a => Math.Max(a[0], a[1]), r => new long[] { Ex.Int(r, 0, 50), Ex.Int(r, 0, 50) }),
        new("area_retangulo", "areaRetangulo", new[] { "l", "h" }, "a área de um retângulo de lados `l` e `h`", new[] { "return l * h" }, new[] { "return l * h;" }, a => a[0] * a[1], r => new long[] { Ex.Int(r, 1, 30), Ex.Int(r, 1, 30) }),
        new("media_inteira", "mediaInteira", new[] { "a", "b" }, "a média de `a` e `b` com divisão inteira (sem casas decimais)", new[] { "return (a + b) // 2" }, new[] { "return Math.trunc((a + b) / 2);" }, a => (a[0] + a[1]) / 2, r => new long[] { Ex.Int(r, 0, 90), Ex.Int(r, 0, 90) }),
        new("diferenca_absoluta", "diferencaAbsoluta", new[] { "a", "b" }, "a diferença entre `a` e `b` sem sinal (sempre positiva ou zero)", new[] { "if a > b:", "    return a - b", "return b - a" }, new[] { "return a > b ? a - b : b - a;" }, a => Math.Abs(a[0] - a[1]), r => new long[] { Ex.Int(r, 0, 70), Ex.Int(r, 0, 70) }),
        new("limitar", "limitar", new[] { "x", "lo", "hi" }, "o valor `x` limitado ao intervalo de `lo` a `hi` (`lo` se `x` for menor que `lo`, `hi` se for maior que `hi`, senão o próprio `x`)", new[] { "if x < lo:", "    return lo", "if x > hi:", "    return hi", "return x" }, new[] { "if (x < lo) return lo;", "if (x > hi) return hi;", "return x;" },
            a => Math.Clamp(a[0], a[1], a[2]), r => { int lo = Ex.Int(r, 0, 20); return new long[] { Ex.Int(r, 0, 60), lo, lo + Ex.Int(r, 5, 25) }; }),
    };

    static readonly Fn[] Loops =
    {
        new("soma_ate", "somaAte", new[] { "n" }, "a soma 1 + 2 + ... + `n`", new[] { "total = 0", "for i in range(1, n + 1):", "    total += i", "return total" }, new[] { "let total = 0;", "for (let i = 1; i <= n; i++) total += i;", "return total;" }, a => a[0] * (a[0] + 1) / 2, r => new long[] { Ex.Int(r, 1, 100) }),
        new("fatorial", "fatorial", new[] { "n" }, "o fatorial de `n` (1 · 2 · ... · n, com 0! = 1)", new[] { "res = 1", "for i in range(2, n + 1):", "    res *= i", "return res" }, new[] { "let res = 1;", "for (let i = 2; i <= n; i++) res *= i;", "return res;" }, a => Enumerable.Range(1, (int)a[0]).Aggregate(1L, (x, y) => x * y), r => new long[] { Ex.Int(r, 0, 12) }),
        new("conta_divisores", "contaDivisores", new[] { "n" }, "a quantidade de divisores positivos de `n`", new[] { "c = 0", "for d in range(1, n + 1):", "    if n % d == 0:", "        c += 1", "return c" }, new[] { "let c = 0;", "for (let d = 1; d <= n; d++) {", "  if (n % d === 0) c++;", "}", "return c;" }, a => Enumerable.Range(1, (int)a[0]).Count(d => a[0] % d == 0), r => new long[] { Ex.Int(r, 1, 120) }),
        new("soma_digitos", "somaDigitos", new[] { "n" }, "a soma dos dígitos de `n`", new[] { "s = 0", "while n > 0:", "    s += n % 10", "    n = n // 10", "return s" }, new[] { "let s = 0;", "while (n > 0) {", "  s += n % 10;", "  n = Math.trunc(n / 10);", "}", "return s;" }, a => a[0].ToString().Sum(c => c - '0'), r => new long[] { Ex.Int(r, 0, 99999) }),
        new("eh_primo", "ehPrimo", new[] { "n" }, "`True` (`true`) se `n` for primo e `False` (`false`) caso contrário (1 e 0 não são primos)", new[] { "if n < 2:", "    return False", "d = 2", "while d * d <= n:", "    if n % d == 0:", "        return False", "    d += 1", "return True" },
            new[] { "if (n < 2) return false;", "for (let d = 2; d * d <= n; d++) {", "  if (n % d === 0) return false;", "}", "return true;" }, a => Prime(a[0]) ? 1 : 0, r => new long[] { Ex.Int(r, 0, 150) }, true),
        new("potencia", "potencia", new[] { "b", "e" }, "`b` elevado a `e` (e ≥ 0), usando um laço, sem o operador de potência", new[] { "res = 1", "for i in range(e):", "    res *= b", "return res" }, new[] { "let res = 1;", "for (let i = 0; i < e; i++) res *= b;", "return res;" }, a => (long)Math.Pow(a[0], a[1]), r => new long[] { Ex.Int(r, 0, 9), Ex.Int(r, 0, 6) }),
        new("mdc", "mdc", new[] { "a", "b" }, "o máximo divisor comum de `a` e `b` (use o algoritmo de Euclides: repita trocando `a, b` por `b, a % b` até `b` ser 0)", new[] { "while b != 0:", "    a, b = b, a % b", "return a" }, new[] { "while (b !== 0) {", "  const t = b;", "  b = a % b;", "  a = t;", "}", "return a;" }, a => Gcd(a[0], a[1]), r => new long[] { Ex.Int(r, 1, 120), Ex.Int(r, 1, 120) }),
    };

    static string Define(Fn f, bool js)
    {
        string ps = string.Join(", ", f.Ps);
        if (js) return $"function {f.Js}({ps}) {{\n" + string.Join("\n", f.JsBody.Select(l => "  " + l)) + "\n}";
        return $"def {f.Py}({ps}):\n" + string.Join("\n", f.PyBody.Select(l => "    " + l));
    }

    static Exercise FnExercise(Fn f, bool js, Random r, string hint, string[]? extraDefs = null)
    {
        string name = js ? f.Js : f.Py;
        var ins = Enumerable.Range(0, 5).Select(_ => string.Join("\n", f.Args(r))).ToList();
        string Model(string inp) { var a = inp.TrimEnd('\n').Split('\n').Select(long.Parse).ToArray(); var v = f.F(a); return f.Bool ? (v == 1 ? "sim" : "não") : v.ToString(); }
        var tests = Pg.Tests(ins, Model);
        string call = f.Bool ? (js ? $"console.log({name}({string.Join(", ", f.Ps)}) ? \"sim\" : \"não\");" : $"print(\"sim\" if {name}({string.Join(", ", f.Ps)}) else \"não\")") : (js ? $"console.log({name}({string.Join(", ", f.Ps)}));" : $"print({name}({string.Join(", ", f.Ps)}))");
        string main = string.Join("\n", f.Ps.Select(p => Pg.ReadInt(js, p))) + "\n" + call;
        string sol = string.Join("\n", (extraDefs ?? Array.Empty<string>()).Append(Define(f, js)).Append(main));
        string starter = string.Join("\n", f.Ps.Select(p => Pg.ReadInt(js, p))) + "\n" + Pg.Comment(js, L.T("escreva a função (ela deve vir antes de ser usada) e mostre o resultado da chamada"));
        string readTxt = f.Ps.Length == 1 ? L.T("Depois leia um inteiro") : L.F("Depois leia {0} inteiros (um por linha), nesta ordem: {1}", f.Ps.Length, string.Join(", ", f.Ps.Select(p => "`" + p + "`")));
        string head = L.F("Escreva a função `{0}({1})` que devolve {2}.", name, string.Join(", ", f.Ps), L.T(f.Desc)) + " " + readTxt + " "
            + (f.Bool ? L.T("e mostre `sim` se a função devolver verdadeiro ou `não` se devolver falso.") : L.T("e mostre o que a função devolve para esses valores."));
        return Ex.Code(Pg.Prompt(head + (hint.Length > 0 ? "\n\n" + hint : ""), tests[0]), starter, sol, tests,
            L.T("A função recebe os valores por parâmetros, faz a conta e DEVOLVE o resultado com return (não com print). O programa principal lê as entradas, chama a função e mostra o que ela devolveu."));
    }

    static Exercise MakeCode(bool js, int lv, Random r)
    {
        switch (lv)
        {
            case 0: return FnExercise(Ex.Pick(r, Simple), js, r, "");
            case 1: return FnExercise(Ex.Pick(r, Two), js, r, "");
            case 2: return FnExercise(Ex.Pick(r, Loops), js, r, "");
            default:
            {
                int kind = r.Next(3);
                if (kind == 0)
                {
                    var f = new Fn("mmc", "mmc", new[] { "a", "b" }, "o mínimo múltiplo comum de `a` e `b`", new[] { "return a // mdc(a, b) * b" }, new[] { "return Math.trunc(a / mdc(a, b)) * b;" }, a => a[0] / Gcd(a[0], a[1]) * a[1], rr => new long[] { Ex.Int(rr, 1, 60), Ex.Int(rr, 1, 60) });
                    var mdc = Loops.First(x => x.Py == "mdc");
                    return FnExercise(f, js, r, L.T("Primeiro escreva também a função `mdc(a, b)` (Euclides) e use-a dentro de `mmc`: mmc(a, b) = a ÷ mdc(a, b) × b."), new[] { Define(mdc, js) });
                }
                if (kind == 1)
                {
                    var f = new Fn("quantos_primos", "quantosPrimos", new[] { "a", "b" }, "a quantidade de números primos no intervalo de `a` até `b`, inclusive (a ≤ b)", new[] { "c = 0", "for n in range(a, b + 1):", "    if eh_primo(n):", "        c += 1", "return c" }, new[] { "let c = 0;", "for (let n = a; n <= b; n++) {", "  if (ehPrimo(n)) c++;", "}", "return c;" },
                        a => Enumerable.Range((int)a[0], (int)(a[1] - a[0] + 1)).Count(x => Prime(x)), rr => { int a = Ex.Int(rr, 0, 60); return new long[] { a, a + Ex.Int(rr, 0, 80) }; });
                    var pr = Loops.First(x => x.Py == "eh_primo");
                    return FnExercise(f, js, r, L.T("Escreva também a função `eh_primo(n)` / `ehPrimo(n)` e use-a dentro da nova função, em vez de repetir a verificação."), new[] { Define(pr, js) });
                }
                {
                    var f = new Fn("raiz_digital", "raizDigital", new[] { "n" }, "a raiz digital de `n`: some os dígitos; se o resultado ainda tiver mais de um dígito, some de novo, até sobrar um só dígito", new[] { "while n >= 10:", "    n = soma_digitos(n)", "return n" }, new[] { "while (n >= 10) {", "  n = somaDigitos(n);", "}", "return n;" },
                        a => { long n = a[0]; while (n >= 10) n = n.ToString().Sum(c => c - '0'); return n; }, rr => new long[] { Ex.Int(rr, 0, 999999) });
                    var sd = Loops.First(x => x.Py == "soma_digitos");
                    return FnExercise(f, js, r, L.T("Escreva também a função `soma_digitos(n)` / `somaDigitos(n)` (soma dos dígitos) e chame-a dentro da nova função. Dica: raiz digital de n também é 1 + (n − 1) % 9 para n > 0, mas aqui use a função."), new[] { Define(sd, js) });
                }
            }
        }
    }

    static Exercise MakeOut(bool js, int lv, Random r)
    {
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        string def(string name, string ps, params string[] body) => js ? $"function {name}({ps}) {{\n" + string.Join("\n", body.Select(l => "  " + l)) + "\n}" : $"def {name}({ps}):\n" + string.Join("\n", body.Select(l => "    " + l));
        switch (lv)
        {
            case 0:
            {
                int k = Ex.Int(r, 2, 5), c = Ex.Int(r, 1, 9), dd = Ex.Int(r, 1, 9), a = Ex.Int(r, 1, 9), b = Ex.Int(r, 1, 9), cc = Ex.Int(r, 1, 9);
                long f(long x) => x * k + c; long g(long x) => f(x) - dd;
                string code = def("f", "x", js ? $"return x * {k} + {c};" : $"return x * {k} + {c}") + "\n" + def("g", "x", js ? $"return f(x) - {dd};" : $"return f(x) - {dd}") + "\n"
                    + pr($"f({a})") + "\n" + pr($"g({b})") + "\n" + pr($"f(g({cc}))");
                string outp = string.Join("\n", f(a), g(b), f(g(cc)));
                return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("f(x) = x·{0} + {1} e g(x) = f(x) − {2}. Resolva de dentro para fora: f(g({3})) primeiro calcula g({3}) = {4} e depois f({4}) = {5}.", k, c, dd, cc, g(cc), f(g(cc))));
            }
            case 1:
            {
                int x = Ex.Int(r, 1, 9), y = Ex.Int(r, 1, 9); string w1 = Ex.Pick(r, "dentro", "aqui", "rodou"), w2 = Ex.Pick(r, "outra", "sem", "vazia");
                string code = def("f", "x", pr($"\"{w1}\", x"), "return x + 1") .Replace("print(\"" + w1 + "\", x)\n    return", "print(\"" + w1 + "\", x)\n    return") + "\n"
                    + def("g", "x", pr($"\"{w2}\", x")) + "\n" + pr($"f({x})") + "\n" + pr($"g({y})");
                string outp = $"{w1} {x}\n{x + 1}\n{w2} {y}\n{(js ? "undefined" : "None")}";
                return Ex.Output(PgOut.Prompt(outp), code, outp,
                    L.T("Chamar a função executa o print que há dentro dela ANTES de o resultado ser mostrado. Uma função sem return devolve None (Python) ou undefined (JavaScript), e isso também é escrito pelo print de fora."));
            }
            case 2:
            {
                int x0 = Ex.Int(r, 1, 9), loc = Ex.Int(r, 10, 20), d1 = Ex.Int(r, 2, 6), a = Ex.Int(r, 1, 9), b = Ex.Int(r, 1, 9);
                string code = (js ? $"let x = {x0};\n" : $"x = {x0}\n")
                    + def("f", "a, b = " + d1, js ? "let x = a * b;" : "x = a * b", "return x + a")
                    + "\n" + def("g", "", js ? $"let x = {loc};" : $"x = {loc}", "return x") + "\n"
                    + pr($"f({a})") + "\n" + pr($"f({a}, {b})") + "\n" + pr("g()") + "\n" + pr("x");
                string outp = string.Join("\n", a * d1 + a, a * b + a, loc, x0);
                return Ex.Output(PgOut.Prompt(outp), code, outp,
                    L.F("Sem o segundo argumento, b vale o padrão {0}. As variáveis `x` criadas dentro das funções são locais: a de fora continua valendo {1}.", d1, x0));
            }
            default:
            {
                var start = new List<int> { Ex.Int(r, 1, 5), Ex.Int(r, 1, 5), Ex.Int(r, 1, 5) };
                int v = Ex.Int(r, 6, 9);
                string ini = string.Join(", ", start);
                string append = js ? "lst.push(v);" : "lst.append(v)", rebind = js ? "lst = [...lst, v];" : "lst = lst + [v]";
                var lst = new List<int>(start); lst.Add(v); // after mutate
                int mutLen = lst.Count;
                string pop = js ? "lst.pop();" : "lst.pop()";
                string code = (js ? $"let nums = [{ini}];\n" : $"nums = [{ini}]\n")
                    + def("muda", "lst, v", append) + "\n" + def("troca", "lst, v", rebind, js ? "return lst.length;" : "return len(lst)") + "\n" + def("tira", "lst", pop) + "\n"
                    + $"muda(nums, {v})\n".Replace("\n", js ? ";\n" : "\n") + pr(js ? "nums.length" : "len(nums)") + "\n"
                    + pr($"troca(nums, {v})") + "\n" + pr(js ? "nums.length" : "len(nums)") + "\n" + $"tira(nums)".Replace("tira(nums)", js ? "tira(nums);" : "tira(nums)") + "\n" + pr(js ? "nums.join(\" \")" : "\" \".join(str(n) for n in nums)");
                lst.RemoveAt(lst.Count - 1);
                string outp = string.Join("\n", mutLen, mutLen + 1, mutLen, string.Join(" ", lst));
                return Ex.Output(PgOut.Prompt(outp), code, outp,
                    L.T("A lista é passada por referência: `muda` e `tira` alteram a lista de fora (append/push e pop). Já `troca` cria uma lista NOVA com + (ou spread) e só a variável local passa a apontar para ela: a lista de fora não muda, mas o len devolvido é o da nova."));
            }
        }
    }
}
