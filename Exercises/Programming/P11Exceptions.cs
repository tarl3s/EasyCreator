namespace EasyCreator.Exercises;

/// <summary>Lessons py-11 / js-13 (exceptions: try / except, try / catch, throw): validating input, safe operations, and tracing try / catch / finally flows.</summary>
[Exercises("py-11", "js-13")]
public sealed class P11Exceptions : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        bool js = Pg.Js(lesson);
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(js, lv, r) : MakeOut(js, lv, r);
    }

    static readonly string[] Junk = { "abc", "7x", "ola", "1.5x", "xyz", "12a", "tres" };

    static Exercise MakeCode(bool js, int lv, Random r)
    {
        switch (lv)
        {
            case 0:
            {
                int k = Ex.Int(r, 2, 9);
                string Item() => r.Next(3) == 0 ? Ex.Pick(r, Junk) : (r.Next(4) == 0 ? "-" : "") + Ex.Int(r, 1, 99);
                var tests = Pg.Tests(new[] { "21", "abc", "-4", Item(), Item(), Item() }, i => int.TryParse(i.Trim(), out int x) ? $"Dobro vezes {k}: {x * 2 * k}" : "Inválido");
                string sol = js ? Pg.Code(Pg.ReadStr(true, "s"), "try {", "  const x = JSON.parse(s);", $"  console.log(`Dobro vezes {k}: ${{x * 2 * {k}}}`);", "} catch (e) {", "  console.log(\"Inválido\");", "}")
                    : Pg.Code(Pg.ReadStr(false, "s"), "try:", "    x = int(s)", $"    print(f\"Dobro vezes {k}: {{x * 2 * {k}}}\")", "except ValueError:", "    print(\"Inválido\")");
                return Ex.Code(Pg.Prompt(L.F("Leia uma linha. Se ela for um número inteiro `x`, mostre `Dobro vezes {0}: <x · 2 · {0}>`; se NÃO for (por exemplo `abc`), o programa não pode quebrar: mostre `Inválido`.", k) + " " + (js ? L.T("Dica: JSON.parse(texto) lança um erro quando o texto não é um valor válido.") : L.T("Dica: int(texto) lança ValueError quando o texto não é um inteiro.")), tests[0]),
                    Pg.Code(Pg.ReadStr(js, "s"), Pg.Comment(js, L.T("converta o texto dentro de um bloco que captura o erro"))), sol, tests,
                    L.T("O código que pode falhar fica dentro de try; se der erro, a execução pula para o except/catch, que mostra a mensagem em vez de deixar o programa terminar com erro."));
            }
            case 1:
            {
                var ins = new List<string> { "10\n0", "7\n2" };
                for (int i = 0; i < 4; i++) ins.Add($"{Ex.Int(r, 0, 90)}\n{Ex.Pick(r, 0, 0, 3, 4, 5, 7)}");
                var tests = Pg.Tests(ins, i => { var p = i.TrimEnd('\n').Split('\n').Select(int.Parse).ToArray(); return p[1] == 0 ? "Erro: divisão por zero" : $"Resultado: {p[0] / p[1]}"; });
                string rd = Pg.Code(Pg.ReadInt(js, "a"), Pg.ReadInt(js, "b"));
                string sol = js ? Pg.Code("function dividir(a, b) {", "  if (b === 0) throw new Error(\"divisão por zero\");", "  return Math.trunc(a / b);", "}", rd, "try {", "  console.log(`Resultado: ${dividir(a, b)}`);", "} catch (e) {", "  console.log(`Erro: ${e.message}`);", "}")
                    : Pg.Code(rd, "try:", "    print(f\"Resultado: {a // b}\")", "except ZeroDivisionError:", "    print(\"Erro: divisão por zero\")");
                return Ex.Code(Pg.Prompt(L.T("Leia dois inteiros `a` e `b` (um por linha, ambos ≥ 0). Mostre `Resultado: <a ÷ b, divisão inteira>`; se `b` for 0, mostre `Erro: divisão por zero`.") + " "
                    + (js ? L.T("Em JavaScript dividir por zero não dá erro sozinho: escreva uma função `dividir(a, b)` que LANCE um erro com `throw new Error(\"divisão por zero\")` e capture-o com try/catch.") : L.T("Capture o erro ZeroDivisionError com try/except.")), tests[0]),
                    rd + "\n" + Pg.Comment(js, L.T("escreva o resto do programa aqui")), sol, tests,
                    js ? L.T("A função verifica a condição e usa throw para sinalizar o problema; quem chama usa try/catch e mostra e.message.") : L.T("Python lança ZeroDivisionError sozinho ao dividir por zero; o except captura só esse tipo de erro e mostra a mensagem combinada."));
            }
            case 2:
            {
                int lim = Ex.Int(r, 50, 500);
                string Model(string s) { if (!(s.TrimStart('-').Length > 0 && s.TrimStart('-').All(char.IsDigit))) return $"{s}: não é número"; int v = int.Parse(s); return v < 0 ? $"{s}: negativo" : v > lim ? $"{s}: grande demais" : $"{s}: ok {v}"; }
                string Item() => Ex.Pick(r, "abc", "7x", Ex.Int(r, 1, 99).ToString(), (-Ex.Int(r, 1, 50)).ToString(), (lim + Ex.Int(r, 1, 50)).ToString(), lim.ToString());
                var ins = new List<string> { string.Join("\n", new[] { "5", "12", "-5", "abc", lim.ToString(), (lim + 1).ToString() }) };
                for (int i = 0; i < 4; i++) { int n = Ex.Int(r, 3, 6); ins.Add(n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => Item()))); }
                var tests = Pg.Tests(ins, i => string.Join("\n", i.TrimEnd('\n').Split('\n').Skip(1).Select(Model)));
                string sol = js ? Pg.Code("function checa(s) {", "  if (!/^-?\\d+$/.test(s)) throw new Error(\"não é número\");", "  const v = Number(s);", "  if (v < 0) throw new Error(\"negativo\");", $"  if (v > {lim}) throw new Error(\"grande demais\");", "  return v;", "}", Pg.ReadInt(true, "n"), "for (let i = 0; i < n; i++) {", "  const s = prompt();", "  try {", "    console.log(`${s}: ok ${checa(s)}`);", "  } catch (e) {", "    console.log(`${s}: ${e.message}`);", "  }", "}")
                    : Pg.Code("def checa(s):", "    if not s.lstrip(\"-\").isdigit():", "        raise ValueError(\"não é número\")", "    v = int(s)", "    if v < 0:", "        raise ValueError(\"negativo\")", $"    if v > {lim}:", "        raise ValueError(\"grande demais\")", "    return v", Pg.ReadInt(false, "n"), "for i in range(n):", "    s = input()", "    try:", "        print(s + \": ok \" + str(checa(s)))", "    except ValueError as e:", "        print(s + \": \" + str(e))");
                return Ex.Code(Pg.Prompt(L.F("Leia `n` e depois `n` linhas, cada uma um valor. Escreva uma função `checa` que LANÇA um erro com a mensagem certa, verificando nesta ordem: o texto não é um inteiro (`não é número`); o número é negativo (`negativo`); o número é maior que {0} (`grande demais`). Para cada linha mostre `<linha>: ok <valor>` se passou ou `<linha>: <mensagem do erro>` se falhou. Um erro numa linha não pode impedir as seguintes.", lim), tests[0]),
                    Pg.Code(Pg.ReadInt(js, "n"), Pg.Comment(js, L.T("escreva a função e o laço que lê as linhas"))), sol, tests,
                    L.T("A função só valida e lança; quem decide o que fazer com o erro é o laço, que envolve cada chamada em try/catch. Assim uma linha ruim mostra a mensagem e o laço continua para a próxima."));
            }
            default:
            {
                string Model(string inp) { int sum = 0, errs = 0; foreach (var s in inp.TrimEnd('\n').Split('\n')) { if (s == "fim") break; if (int.TryParse(s, out int v)) sum += v; else errs++; } return $"Soma: {sum}\nErros: {errs}"; }
                string Item() => r.Next(3) == 0 ? Ex.Pick(r, Junk) : Ex.Int(r, -20, 60).ToString();
                var ins = new List<string> { "5\n3\nabc\n4\nfim\n100" };
                for (int i = 0; i < 4; i++) { int n = Ex.Int(r, 2, 7); ins.Add(string.Join("\n", Enumerable.Range(0, n).Select(_ => Item())) + "\nfim\n" + Ex.Int(r, 1, 50)); }
                var tests = Pg.Tests(ins, Model);
                string sol = js ? Pg.Code("let soma = 0, erros = 0;", "while (true) {", "  const s = prompt();", "  if (s === \"fim\" || s === null) break;", "  if (/^-?\\d+$/.test(s)) soma += Number(s);", "  else erros++;", "}", "console.log(`Soma: ${soma}`);", "console.log(`Erros: ${erros}`);")
                    : Pg.Code("soma = 0", "erros = 0", "while True:", "    s = input()", "    if s == \"fim\":", "        break", "    try:", "        soma += int(s)", "    except ValueError:", "        erros += 1", "print(f\"Soma: {soma}\")", "print(f\"Erros: {erros}\")");
                return Ex.Code(Pg.Prompt(L.T("Leia linhas até aparecer a linha `fim` (as linhas depois dela não devem ser lidas). Cada linha que for um inteiro entra numa soma; as outras (texto qualquer) são ERROS que não podem derrubar o programa nem parar a leitura. Mostre `Soma: <soma>` e `Erros: <quantas linhas não eram números>`."), tests[0]),
                    Pg.Comment(js, L.T("escreva o programa: ele não sabe quantas linhas virão")), sol, tests,
                    L.T("Não sabemos quantas linhas vêm, então o laço é infinito (while True) e só termina com break quando lê `fim`. A conversão fica dentro de try/except: se falhar, contamos um erro e o laço segue."));
            }
        }
    }

    static Exercise MakeOut(bool js, int lv, Random r)
    {
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        string ind(string[] ls, int n) => string.Join("\n", ls.Select(l => new string(' ', n) + l));
        if (lv <= 1)
        {
            int lim = Ex.Int(r, 1, 5); var xs = Enumerable.Range(0, 3).Select(_ => Ex.Int(r, -3, 8)).ToArray();
            string w = Ex.Pick(r, "ok", "bom", "valeu"), e = Ex.Pick(r, "erro", "falhou", "ruim");
            string code = js ? $"function f(x) {{\n  try {{\n    if (x < {lim}) throw new Error(\"pequeno\");\n    console.log(\"{w}\");\n    return x * 2;\n  }} catch (err) {{\n    console.log(\"{e}\", err.message);\n    return -1;\n  }}\n}}\n" + string.Join("\n", xs.Select(x => $"console.log(f({x}));"))
                : $"def f(x):\n    try:\n        if x < {lim}:\n            raise ValueError(\"pequeno\")\n        print(\"{w}\")\n        return x * 2\n    except ValueError as err:\n        print(\"{e}\", err)\n        return -1\n" + string.Join("\n", xs.Select(x => $"print(f({x}))"));
            var outs = new List<string>();
            foreach (var x in xs) { if (x < lim) { outs.Add($"{e} pequeno"); outs.Add("-1"); } else { outs.Add(w); outs.Add((x * 2).ToString()); } }
            string outp = string.Join("\n", outs);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Quando x < {0}, o raise/throw interrompe o try e a execução salta para o except/catch (o print \"{1}\" e o return do try NÃO acontecem). Quando não há erro, o except é ignorado.", lim, w));
        }
        if (lv == 2)
        {
            int lim = Ex.Int(r, 1, 4); var xs = Enumerable.Range(0, 3).Select(_ => Ex.Int(r, -2, 6)).ToArray();
            string code = js ? $"function f(x) {{\n  try {{\n    if (x < {lim}) throw new Error(\"neg\");\n    console.log(\"ok\");\n    return x * 2;\n  }} catch (e) {{\n    console.log(\"erro\");\n    return -1;\n  }} finally {{\n    console.log(\"fim\");\n  }}\n}}\n" + string.Join("\n", xs.Select(x => $"console.log(f({x}));"))
                : $"def f(x):\n    try:\n        if x < {lim}:\n            raise ValueError(\"neg\")\n        print(\"ok\")\n        return x * 2\n    except ValueError:\n        print(\"erro\")\n        return -1\n    finally:\n        print(\"fim\")\n" + string.Join("\n", xs.Select(x => $"print(f({x}))"));
            var outs = new List<string>();
            foreach (var x in xs) { if (x < lim) outs.Add("erro"); else outs.Add("ok"); outs.Add("fim"); outs.Add(x < lim ? "-1" : (x * 2).ToString()); }
            string outp = string.Join("\n", outs);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("O bloco finally roda SEMPRE, tanto com erro quanto sem, e antes de a função entregar o valor do return. Por isso \"fim\" aparece antes do número que o print de fora escreve."));
        }
        {
            int m = Ex.Pick(r, 3, 4); var ks = Enumerable.Range(1, 5).ToArray();
            string code = js ? $"function interna(k) {{\n  if (k % {m} === 0) throw new Error(\"A\");\n  if (k % {m} === 1) throw new Error(\"B\");\n  return k;\n}}\nfunction meio(k) {{\n  try {{\n    return interna(k);\n  }} catch (e) {{\n    if (e.message !== \"A\") throw e;\n    console.log(\"trata A\");\n    return 0;\n  }}\n}}\nfor (const k of [1, 2, 3, 4, 5]) {{\n  try {{\n    console.log(meio(k));\n  }} catch (e) {{\n    console.log(\"pegou\", e.message);\n  }}\n}}"
                : $"def interna(k):\n    if k % {m} == 0:\n        raise ValueError(\"A\")\n    if k % {m} == 1:\n        raise ValueError(\"B\")\n    return k\ndef meio(k):\n    try:\n        return interna(k)\n    except ValueError as e:\n        if str(e) != \"A\":\n            raise\n        print(\"trata A\")\n        return 0\nfor k in [1, 2, 3, 4, 5]:\n    try:\n        print(meio(k))\n    except ValueError as e:\n        print(\"pegou\", e)";
            var outs = new List<string>();
            foreach (var k in ks) { if (k % m == 0) { outs.Add("trata A"); outs.Add("0"); } else if (k % m == 1) outs.Add("pegou B"); else outs.Add(k.ToString()); }
            string outp = string.Join("\n", outs);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Para k % {0} == 0 a função do meio trata o erro A (escreve e devolve 0). Para k % {0} == 1 o erro B é relançado (raise / throw), sobe para o laço e é capturado lá; o print da função do meio não acontece. Nos demais casos devolve k.", m));
        }
    }
}
