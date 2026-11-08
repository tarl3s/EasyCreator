namespace EasyCreator.Exercises;

/// <summary>Lesson 9 (recursion): writing small recursive functions, and tracing return values, print order and call counts.</summary>
[Exercises("py-09", "js-09")]
public sealed class P09Recursion : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        bool js = Pg.Js(lesson);
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(js, lv, r) : MakeOut(js, lv, r);
    }

    // A recursive function to write: parameters named "s" are text, the others integers.
    record RFn(string Name, string[] Ps, string Desc, string[] PyBody, string[] JsBody, Func<string[], string> F, Func<Random, string[]> Args);

    static long Fib(int n) => n < 2 ? n : Fib(n - 1) + Fib(n - 2);

    static readonly RFn[] Easy =
    {
        new("soma_ate", new[] { "n" }, "a soma 1 + 2 + ... + `n` (com `soma_ate(0) = 0`)", new[] { "if n == 0:", "    return 0", "return n + soma_ate(n - 1)" }, new[] { "if (n === 0) return 0;", "return n + somaAte(n - 1);" }, a => (long.Parse(a[0]) * (long.Parse(a[0]) + 1) / 2).ToString(), r => new[] { Ex.Int(r, 0, 60).ToString() }),
        new("potencia", new[] { "b", "e" }, "`b` elevado a `e` (e ≥ 0), sempre multiplicando `b` por `potencia(b, e - 1)`", new[] { "if e == 0:", "    return 1", "return b * potencia(b, e - 1)" }, new[] { "if (e === 0) return 1;", "return b * potencia(b, e - 1);" }, a => ((long)Math.Pow(long.Parse(a[0]), long.Parse(a[1]))).ToString(), r => new[] { Ex.Int(r, 0, 9).ToString(), Ex.Int(r, 0, 8).ToString() }),
        new("conta_digitos", new[] { "n" }, "a quantidade de dígitos de `n` (n ≥ 1; um número de 1 dígito devolve 1)", new[] { "if n < 10:", "    return 1", "return 1 + conta_digitos(n // 10)" }, new[] { "if (n < 10) return 1;", "return 1 + contaDigitos(Math.trunc(n / 10));" }, a => a[0].Length.ToString(), r => new[] { Ex.Int(r, 1, 99999999).ToString() }),
    };

    static readonly RFn[] Medium =
    {
        new("soma_digitos", new[] { "n" }, "a soma dos dígitos de `n` (n ≥ 0), sem laços: o último dígito é `n % 10` e o resto é `n` dividido por 10", new[] { "if n == 0:", "    return 0", "return n % 10 + soma_digitos(n // 10)" }, new[] { "if (n === 0) return 0;", "return (n % 10) + somaDigitos(Math.trunc(n / 10));" }, a => a[0].Sum(c => c - '0').ToString(), r => new[] { Ex.Int(r, 0, 99999999).ToString() }),
        new("fibonacci", new[] { "n" }, "o n-ésimo número de Fibonacci (fib(0) = 0, fib(1) = 1, fib(n) = fib(n−1) + fib(n−2))", new[] { "if n < 2:", "    return n", "return fibonacci(n - 1) + fibonacci(n - 2)" }, new[] { "if (n < 2) return n;", "return fibonacci(n - 1) + fibonacci(n - 2);" }, a => Fib(int.Parse(a[0])).ToString(), r => new[] { Ex.Int(r, 0, 20).ToString() }),
        new("inverter", new[] { "s" }, "o texto `s` invertido (use o primeiro caractere e a chamada recursiva com o resto do texto; o texto vazio devolve vazio)", new[] { "if s == \"\":", "    return \"\"", "return inverter(s[1:]) + s[0]" }, new[] { "if (s === \"\") return \"\";", "return inverter(s.slice(1)) + s[0];" }, a => new string(a[0].Reverse().ToArray()), r => new[] { Ex.Pick(r, Pg.Words) + Ex.Pick(r, Pg.Words) }),
    };

    static readonly RFn[] Hard =
    {
        new("mdc", new[] { "a", "b" }, "o máximo divisor comum de `a` e `b` (b pode ser 0): mdc(a, 0) = a e mdc(a, b) = mdc(b, a % b)", new[] { "if b == 0:", "    return a", "return mdc(b, a % b)" }, new[] { "if (b === 0) return a;", "return mdc(b, a % b);" }, a => Gcd(long.Parse(a[0]), long.Parse(a[1])).ToString(), r => new[] { Ex.Int(r, 1, 200).ToString(), Ex.Int(r, 0, 200).ToString() }),
        new("binario", new[] { "n" }, "a escrita de `n` em binário, como texto (binario(0) = \"0\"; para n ≥ 2 é binario(n ÷ 2) seguido do bit n % 2)", new[] { "if n < 2:", "    return str(n)", "return binario(n // 2) + str(n % 2)" }, new[] { "if (n < 2) return String(n);", "return binario(Math.trunc(n / 2)) + String(n % 2);" }, a => Convert.ToString(long.Parse(a[0]), 2), r => new[] { Ex.Int(r, 0, 5000).ToString() }),
        new("palindromo", new[] { "s" }, "`sim` se o texto `s` é palíndromo e `nao` se não é, comparando as pontas e chamando a função com o miolo (textos de 0 ou 1 letra são palíndromos)", new[] { "if len(s) < 2:", "    return \"sim\"", "if s[0] != s[-1]:", "    return \"nao\"", "return palindromo(s[1:-1])" }, new[] { "if (s.length < 2) return \"sim\";", "if (s[0] !== s[s.length - 1]) return \"nao\";", "return palindromo(s.slice(1, -1));" },
            a => a[0].SequenceEqual(a[0].Reverse()) ? "sim" : "nao", r => { var w = Ex.Pick(r, Pg.Words); return new[] { r.Next(2) == 0 ? w + new string(w.Reverse().ToArray()) : w + Ex.Pick(r, Pg.Words) }; }),
    };

    static long Gcd(long a, long b) => b == 0 ? a : Gcd(b, a % b);

    static string Fname(RFn f, bool js) => js ? string.Concat(f.Name.Split('_').Select((p, i) => i == 0 ? p : char.ToUpper(p[0]) + p[1..])) : f.Name;

    static string Body(RFn f, bool js)
    {
        string name = Fname(f, js), ps = string.Join(", ", f.Ps);
        string pyBody = string.Join("\n", f.PyBody.Select(l => "    " + l));
        string jsBody = string.Join("\n", f.JsBody.Select(l => "  " + l));
        return js ? $"function {name}({ps}) {{\n{jsBody}\n}}" : $"def {name}({ps}):\n{pyBody}";
    }

    static Exercise RFnExercise(RFn f, bool js, Random r, string extra = "")
    {
        string name = Fname(f, js);
        var tests = Pg.Tests(Enumerable.Range(0, 5).Select(_ => string.Join("\n", f.Args(r))), i => f.F(i.TrimEnd('\n').Split('\n')));
        string reads = string.Join("\n", f.Ps.Select(p => p == "s" ? Pg.ReadStr(js, "s") : Pg.ReadInt(js, p)));
        string call = js ? $"console.log({name}({string.Join(", ", f.Ps)}));" : $"print({name}({string.Join(", ", f.Ps)}))";
        string sol = Body(f, js) + "\n" + reads + "\n" + call;
        string starter = reads + "\n" + Pg.Comment(js, L.T("escreva a função recursiva e mostre o resultado da chamada"));
        string head = L.F("Escreva a função RECURSIVA `{0}({1})` (sem laços) que devolve {2}.", name, string.Join(", ", f.Ps), L.T(f.Desc)) + " "
            + (f.Ps.Length == 1 ? L.T("Depois leia o valor (uma linha) e mostre o que a função devolve.") : L.F("Depois leia os {0} valores (um por linha, na ordem dos parâmetros) e mostre o que a função devolve.", f.Ps.Length));
        return Ex.Code(Pg.Prompt(head + (extra.Length > 0 ? "\n\n" + extra : ""), tests[0]), starter, sol, tests,
            L.T("Toda função recursiva tem um CASO BASE (a resposta direta, sem chamar a si mesma) e um passo recursivo que chama a função com um problema menor. Aqui o caso base vem primeiro; o passo recursivo combina o valor atual com o resultado da chamada menor."));
    }

    static Exercise MakeCode(bool js, int lv, Random r)
    {
        switch (lv)
        {
            case 0: return RFnExercise(Ex.Pick(r, Easy), js, r);
            case 1: return RFnExercise(Ex.Pick(r, Medium), js, r);
            case 2: return RFnExercise(Ex.Pick(r, Hard), js, r);
            default:
            {
                if (r.Next(2) == 0)
                {
                    int Ways(int n) => n < 0 ? 0 : n == 0 ? 1 : Ways(n - 1) + Ways(n - 2) + Ways(n - 3);
                    var f = new RFn("formas", new[] { "n" }, "de quantas maneiras se sobe uma escada de `n` degraus dando passos de 1, 2 ou 3 degraus por vez (formas(0) = 1: há uma maneira de não subir; formas de um número negativo = 0)",
                        new[] { "if n < 0:", "    return 0", "if n == 0:", "    return 1", "return formas(n - 1) + formas(n - 2) + formas(n - 3)" }, new[] { "if (n < 0) return 0;", "if (n === 0) return 1;", "return formas(n - 1) + formas(n - 2) + formas(n - 3);" },
                        a => Ways(int.Parse(a[0])).ToString(), rr => new[] { Ex.Int(rr, 0, 16).ToString() });
                    return RFnExercise(f, js, r, L.T("Dica: o último passo foi de 1, 2 ou 3 degraus; some as maneiras de chegar em cada um desses pontos de partida."));
                }
                else
                {
                    long Subs(int[] v, int i, int t) => i == v.Length ? (t == 0 ? 1 : 0) : Subs(v, i + 1, t) + Subs(v, i + 1, t - v[i]);
                    var ins = new List<string> { "4\n1 2 3 4\n5", "3\n1 1 1\n2" };
                    for (int k = 0; k < 3; k++) { int n = Ex.Int(r, 3, 10); var v = Pg.Arr(r, n, 1, 9); ins.Add($"{n}\n{string.Join(" ", v)}\n{Ex.Int(r, 3, 20)}"); }
                    var tests = Pg.Tests(ins, i => { var p = i.TrimEnd('\n').Split('\n'); return Subs(p[1].Split(' ').Select(int.Parse).ToArray(), 0, int.Parse(p[2])).ToString(); });
                    string reads = Pg.Code(Pg.ReadInt(js, "n"), Pg.ReadInts(js, "v"), Pg.ReadInt(js, "alvo"));
                    string sol = js ? Pg.Code("function conta(v, i, t) {", "  if (i === v.length) return t === 0 ? 1 : 0;", "  return conta(v, i + 1, t) + conta(v, i + 1, t - v[i]);", "}", reads, "console.log(conta(v, 0, alvo));")
                        : Pg.Code("def conta(v, i, t):", "    if i == len(v):", "        return 1 if t == 0 else 0", "    return conta(v, i + 1, t) + conta(v, i + 1, t - v[i])", reads, "print(conta(v, 0, alvo))");
                    return Ex.Code(Pg.Prompt(L.T("Leia `n`, depois uma linha com `n` inteiros positivos e, na linha seguinte, um valor `alvo`. Mostre de quantas formas diferentes se pode escolher alguns desses números (escolhas por POSIÇÃO: dois números iguais em posições diferentes contam como escolhas diferentes) para que a soma seja exatamente `alvo`. Use recursão: para cada número, ou ele entra na soma ou não entra."), tests[0]),
                        Pg.Code(reads, Pg.Comment(js, L.T("escreva a função recursiva e mostre o resultado"))), sol, tests,
                        L.T("A função olha o número da posição i e se ramifica em duas chamadas: sem ele (o alvo continua o mesmo) e com ele (o alvo diminui do valor). O caso base é ter passado de todos os números: conta 1 se o alvo chegou a zero, senão 0."));
                }
            }
        }
    }

    // ---- tracing ----
    record Mystery(string[] Py, string[] Js, Func<long, long> F);

    static readonly Mystery[] Mysteries =
    {
        new(new[] { "if n == 0:", "    return 0", "return n + f(n - 1)" }, new[] { "if (n === 0) return 0;", "return n + f(n - 1);" }, n => n * (n + 1) / 2),
        new(new[] { "if n <= 1:", "    return 1", "return n * f(n - 1)" }, new[] { "if (n <= 1) return 1;", "return n * f(n - 1);" }, n => n <= 1 ? 1 : Enumerable.Range(1, (int)n).Aggregate(1L, (a, b) => a * b)),
        new(new[] { "if n == 0:", "    return 0", "return f(n - 1) + 2 * n - 1" }, new[] { "if (n === 0) return 0;", "return f(n - 1) + 2 * n - 1;" }, n => n * n),
        new(new[] { "if n < 10:", "    return n", "return f(n // 10) + n % 10" }, new[] { "if (n < 10) return n;", "return f(Math.trunc(n / 10)) + (n % 10);" }, n => n.ToString().Sum(c => c - '0')),
        new(new[] { "if n == 0:", "    return 0", "return f(n // 2) * 10 + n % 2" }, new[] { "if (n === 0) return 0;", "return f(Math.trunc(n / 2)) * 10 + (n % 2);" }, n => long.Parse(n == 0 ? "0" : Convert.ToString(n, 2))),
        new(new[] { "if n < 2:", "    return n", "return f(n - 1) + f(n - 2)" }, new[] { "if (n < 2) return n;", "return f(n - 1) + f(n - 2);" }, n => Fib((int)n)),
        new(new[] { "if n == 0:", "    return 1", "return 2 * f(n - 1) + 1" }, new[] { "if (n === 0) return 1;", "return 2 * f(n - 1) + 1;" }, n => (1L << (int)(n + 1)) - 1),
    };

    static string FDef(string[] body, bool js, string name = "f", string ps = "n") =>
        js ? $"function {name}({ps}) {{\n" + string.Join("\n", body.Select(l => "  " + l)) + "\n}" : $"def {name}({ps}):\n" + string.Join("\n", body.Select(l => "    " + l));

    static Exercise MakeOut(bool js, int lv, Random r)
    {
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        if (lv == 0 || lv == 1)
        {
            var pool = lv == 0 ? Mysteries.Take(3).ToArray() : Mysteries.Skip(3).Take(3).ToArray();
            var m = Ex.Pick(r, pool);
            var args = Enumerable.Range(0, 3).Select(_ => (long)(lv == 0 ? Ex.Int(r, 0, 9) : (m == Mysteries[5] ? Ex.Int(r, 0, 12) : Ex.Int(r, 5, 9999)))).ToArray();
            string code = FDef(js ? m.Js : m.Py, js) + "\n" + string.Join("\n", args.Select(a => pr($"f({a})")));
            string outp = string.Join("\n", args.Select(m.F));
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("Desça até o caso base e volte combinando os resultados. Por exemplo, para o primeiro valor escreva as chamadas empilhadas (f(n), f(n−1), ... até o caso base) e depois resolva de baixo para cima."));
        }
        if (lv == 2)
        {
            int n = Ex.Int(r, 2, 4), step = Ex.Pick(r, 1, 1, 2); if (step == 2) n = Ex.Int(r, 3, 6);
            int mode = r.Next(3); // 0 before, 1 after, 2 both
            var outs = new List<string>();
            void Rec(int k) { if (k <= 0) return; if (mode != 1) outs.Add($"vai {k}"); Rec(k - step); if (mode != 0) outs.Add($"volta {k}"); }
            Rec(n);
            var body = new List<string>(); var jsb = new List<string>();
            body.Add("if n <= 0:"); body.Add("    return"); jsb.Add("if (n <= 0) return;");
            if (mode != 1) { body.Add("print(\"vai\", n)"); jsb.Add("console.log(\"vai\", n);"); }
            body.Add($"f(n - {step})"); jsb.Add($"f(n - {step});");
            if (mode != 0) { body.Add("print(\"volta\", n)"); jsb.Add("console.log(\"volta\", n);"); }
            string code = FDef(js ? jsb.ToArray() : body.ToArray(), js) + "\n" + (js ? $"f({n});" : $"f({n})");
            string outp = string.Join("\n", outs);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("O que fica ANTES da chamada recursiva acontece na descida; o que fica DEPOIS só acontece na volta, em ordem inversa (a última chamada a descer é a primeira a voltar)."));
        }
        // Genius
        if (r.Next(2) == 0)
        {
            int n = Ex.Int(r, 4, 12); int calls = 0;
            long Fb(int k) { calls++; return k < 2 ? k : Fb(k - 1) + Fb(k - 2); }
            long v = Fb(n);
            string code = js ? $"let chamadas = 0;\nfunction fib(n) {{\n  chamadas++;\n  if (n < 2) return n;\n  return fib(n - 1) + fib(n - 2);\n}}\nconsole.log(fib({n}), chamadas);"
                : $"chamadas = [0]\ndef fib(n):\n    chamadas[0] += 1\n    if n < 2:\n        return n\n    return fib(n - 1) + fib(n - 2)\nprint(fib({n}), chamadas[0])";
            string outp = $"{v} {calls}";
            return Ex.Output(PgOut.Prompt(outp, L.T("O contador soma 1 cada vez que a função é chamada, inclusive nas chamadas que caem no caso base.")), code, outp,
                L.F("fib({0}) = {1}. Cada chamada com n ≥ 2 faz mais duas; no total são {2} chamadas (o número de chamadas c(n) cumpre c(n) = 1 + c(n−1) + c(n−2)).", n, v, calls));
        }
        else
        {
            int k = Ex.Int(r, 3, 12), q = Ex.Int(r, 3, 12);
            string t = js ? "true" : "True", f = js ? "false" : "False";
            string code = js ? $"function par(n) {{\n  return n === 0 ? true : impar(n - 1);\n}}\nfunction impar(n) {{\n  return n === 0 ? false : par(n - 1);\n}}\nconsole.log(par({k}));\nconsole.log(impar({k}));\nconsole.log(par({q}) === impar({q}));"
                : $"def par(n):\n    return True if n == 0 else impar(n - 1)\ndef impar(n):\n    return False if n == 0 else par(n - 1)\nprint(par({k}))\nprint(impar({k}))\nprint(par({q}) == impar({q}))";
            string outp = string.Join("\n", k % 2 == 0 ? t : f, k % 2 == 0 ? f : t, f);
            return Ex.Output(PgOut.Prompt(outp), code, outp,
                L.T("As duas funções se chamam alternadamente (recursão mútua), tirando 1 a cada vez: par(n) termina em par(0) = verdadeiro quando n é par e em impar(0) = falso quando n é ímpar. Por isso par(x) e impar(x) são sempre opostos, e a comparação dá falso."));
        }
    }
}
