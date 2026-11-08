using System.Globalization;

namespace EasyCreator.Exercises;

/// <summary>Lesson 5 (for / while): sequences, statistics over n numbers, digit loops, and tracing loops with break / continue.</summary>
[Exercises("py-05", "js-05")]
public sealed class P05Loops : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        bool js = Pg.Js(lesson);
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(js, lv, r) : MakeOut(js, lv, r);
    }

    record Stat(string Label, string Py, string Js, Func<int[], string> F, string PyInit, string JsInit, string PyBody, string JsBody);

    static readonly Stat[] Stats =
    {
        new("Soma", "{soma}", "{soma}", v => v.Sum().ToString(), "soma = 0", "let soma = 0;", "soma += x", "soma += x;"),
        new("Maior", "{maior}", "{maior}", v => v.Max().ToString(), "maior = -1000000", "let maior = -1000000;", "if x > maior:\n        maior = x", "if (x > maior) maior = x;"),
        new("Menor", "{menor}", "{menor}", v => v.Min().ToString(), "menor = 1000000", "let menor = 1000000;", "if x < menor:\n        menor = x", "if (x < menor) menor = x;"),
        new("Pares", "{pares}", "{pares}", v => v.Count(x => x % 2 == 0).ToString(), "pares = 0", "let pares = 0;", "if x % 2 == 0:\n        pares += 1", "if (x % 2 === 0) pares++;"),
        new("Positivos", "{pos}", "{pos}", v => v.Count(x => x > 0).ToString(), "pos = 0", "let pos = 0;", "if x > 0:\n        pos += 1", "if (x > 0) pos++;"),
        new("Média", "{tot / n:.2f}", "{(tot / n).toFixed(2)}", v => Math.Round((decimal)v.Sum() / v.Length, 2, MidpointRounding.AwayFromZero).ToString("0.00", CultureInfo.InvariantCulture), "tot = 0", "let tot = 0;", "tot += x", "tot += x;"),
    };

    static Exercise MakeCode(bool js, int lv, Random r)
    {
        switch (lv)
        {
            case 0:
            {
                int kind = r.Next(4);
                if (kind == 0)
                {
                    var tests = Pg.Tests(new[] { "5", "1", Ex.Int(r, 2, 12).ToString(), Ex.Int(r, 2, 12).ToString() }, i => string.Join("\n", Enumerable.Range(1, int.Parse(i.Trim())).Select(x => x * x)));
                    var sol = js ? Pg.Code(Pg.ReadInt(true, "n"), "for (let i = 1; i <= n; i++) console.log(i * i);") : Pg.Code(Pg.ReadInt(false, "n"), "for i in range(1, n + 1):", "    print(i * i)");
                    return Ex.Code(Pg.Prompt(L.T("Leia um inteiro `n` (n ≥ 1) e mostre os quadrados dos números de 1 até `n`, um por linha (1, 4, 9, ...)."), tests[0]), Pg.Starter(js, Pg.ReadInt(js, "n")), sol, tests,
                        L.T("Um laço com i de 1 até n (inclusive!) e print de i * i a cada volta. Em Python range(1, n + 1) para antes do último; em JavaScript use i <= n."));
                }
                if (kind == 1)
                {
                    int k = Ex.Int(r, 2, 9);
                    var tests = Pg.Tests(new[] { (k * 4).ToString(), (k * 4 + k - 1).ToString(), Ex.Int(r, k, 60).ToString(), Ex.Int(r, k, 60).ToString() }, i => string.Join("\n", Enumerable.Range(1, int.Parse(i.Trim()) / k).Select(x => x * k)));
                    var sol = js ? Pg.Code(Pg.ReadInt(true, "n"), $"for (let i = {k}; i <= n; i += {k}) console.log(i);") : Pg.Code(Pg.ReadInt(false, "n"), $"for i in range({k}, n + 1, {k}):", "    print(i)");
                    return Ex.Code(Pg.Prompt(L.F("Leia um inteiro `n` (n ≥ {0}) e mostre, um por linha, os múltiplos de {0} que são menores ou iguais a `n`, em ordem crescente.", k), tests[0]), Pg.Starter(js, Pg.ReadInt(js, "n")), sol, tests,
                        L.F("O laço começa em {0} e anda de {0} em {0} (passo) enquanto não passar de n.", k));
                }
                if (kind == 2)
                {
                    var tests = Pg.Tests(new[] { "5", "1", Ex.Int(r, 2, 15).ToString(), Ex.Int(r, 2, 15).ToString() }, i => string.Join("\n", Enumerable.Range(0, int.Parse(i.Trim())).Select(x => (int.Parse(i.Trim()) - x).ToString()).Append("Fogo!")));
                    var sol = js ? Pg.Code(Pg.ReadInt(true, "n"), "for (let i = n; i >= 1; i--) console.log(i);", "console.log(\"Fogo!\");") : Pg.Code(Pg.ReadInt(false, "n"), "for i in range(n, 0, -1):", "    print(i)", "print(\"Fogo!\")");
                    return Ex.Code(Pg.Prompt(L.T("Leia um inteiro `n` (n ≥ 1) e faça uma contagem regressiva: mostre `n`, `n-1`, ..., 1, um por linha, e por fim a palavra `Fogo!`."), tests[0]), Pg.Starter(js, Pg.ReadInt(js, "n")), sol, tests,
                        L.T("O laço desce de n até 1 (passo −1) e o `Fogo!` fica DEPOIS do laço, fora dele, para sair uma vez só."));
                }
                {
                    var tests = Pg.Tests(new[] { "7", "12", Ex.Int(r, 2, 15).ToString(), Ex.Int(r, 2, 15).ToString() }, i => string.Join("\n", Enumerable.Range(1, 10).Select(x => $"{i.Trim()} x {x} = {int.Parse(i.Trim()) * x}")));
                    var sol = js ? Pg.Code(Pg.ReadInt(true, "k"), "for (let i = 1; i <= 10; i++) console.log(`${k} x ${i} = ${k * i}`);") : Pg.Code(Pg.ReadInt(false, "k"), "for i in range(1, 11):", "    print(f\"{k} x {i} = {k * i}\")");
                    return Ex.Code(Pg.Prompt(L.T("Leia um inteiro `k` e mostre a tabuada dele, de 1 a 10, no formato `k x i = resultado`, uma linha para cada `i`."), tests[0]), Pg.Starter(js, Pg.ReadInt(js, "k")), sol, tests,
                        L.T("Um laço com i de 1 a 10 e um print que monta a frase com k, i e o produto k * i."));
                }
            }
            case 1:
            {
                var chosen = Stats.OrderBy(_ => r.Next()).Take(3).ToArray();
                var ins = new List<string> { "4\n3\n8\n-2\n5", "1\n7" };
                for (int i = 0; i < 3; i++) { int n = Ex.Int(r, 2, 7); ins.Add(n + "\n" + string.Join("\n", Pg.Arr(r, n, -20, 50))); }
                var tests = Pg.Tests(ins, inp => { var p = inp.TrimEnd('\n').Split('\n').Select(int.Parse).ToArray(); var v = p.Skip(1).ToArray(); return string.Join("\n", chosen.Select(s => $"{s.Label}: {s.F(v)}")); });
                var lines = new List<string> { Pg.ReadInt(js, "n") };
                lines.AddRange(chosen.Select(s => js ? s.JsInit : s.PyInit));
                lines.Add(js ? "for (let i = 0; i < n; i++) {" : "for i in range(n):");
                lines.Add(js ? "  const x = Number(prompt());" : "    x = int(input())");
                foreach (var s in chosen)
                    lines.Add(js ? "  " + s.JsBody : "    " + s.PyBody);
                if (js) lines.Add("}");
                foreach (var s in chosen) lines.Add(Pg.Pr(js, s.Label + ": " + (js ? s.Js : s.Py)));
                var starter = Pg.Code(Pg.ReadInt(js, "n"), Pg.Comment(js, L.T("leia os n números dentro de um laço e calcule o que se pede")));
                return Ex.Code(Pg.Prompt(L.T("Leia um inteiro `n` (n ≥ 1) e depois `n` inteiros, um por linha. Mostre uma linha para cada item, nesta ordem:") + "\n" + Pg.Plain(string.Join("\n", chosen.Select(s => s.Label + ": <valor>")))
                    + "\n" + L.T("(Pares: quantos números são pares; Positivos: quantos são maiores que zero; Média com 2 casas decimais.)"), tests[0]),
                    starter, Pg.Code(lines.ToArray()), tests,
                    L.T("Antes do laço criamos os acumuladores (soma começa em 0, maior bem pequeno, menor bem grande, contadores em 0). Dentro do laço lemos cada x e atualizamos todos; só depois do laço mostramos os resultados."));
            }
            case 2:
            {
                int kind = r.Next(3);
                if (kind == 0)
                {
                    var tests = Pg.Tests(new[] { "12345", "7", "100", Ex.Int(r, 10, 99999).ToString(), Ex.Int(r, 10, 99999).ToString() },
                        i => { var s = i.Trim(); return $"Dígitos: {s.Length}\nSoma: {s.Sum(c => c - '0')}"; });
                    var sol = js ? Pg.Code(Pg.ReadInt(true, "n"), "let dig = 0, soma = 0;", "while (n > 0) {", "  soma += n % 10;", "  n = Math.trunc(n / 10);", "  dig++;", "}", "console.log(`Dígitos: ${dig}`);", "console.log(`Soma: ${soma}`);")
                        : Pg.Code(Pg.ReadInt(false, "n"), "dig = 0", "soma = 0", "while n > 0:", "    soma += n % 10", "    n = n // 10", "    dig += 1", "print(f\"Dígitos: {dig}\")", "print(f\"Soma: {soma}\")");
                    return Ex.Code(Pg.Prompt(L.T("Leia um inteiro `n` (n ≥ 1) e mostre quantos dígitos ele tem e a soma deles, SEM transformar em texto (use um laço while com divisões)."), tests[0]),
                        Pg.Starter(js, Pg.ReadInt(js, "n")), sol, tests,
                        L.T("Enquanto n > 0: n % 10 é o último dígito (soma nele) e n ÷ 10 descarta esse dígito. Contamos as voltas para saber quantos dígitos há."));
                }
                if (kind == 1)
                {
                    string Model(int n) { var ds = Enumerable.Range(1, n).Where(x => n % x == 0).ToList(); return string.Join(" ", ds) + "\n" + $"Total: {ds.Count}"; }
                    var tests = Pg.Tests(new[] { "12", "1", "13", Ex.Int(r, 2, 120).ToString(), Ex.Int(r, 2, 120).ToString() }, i => Model(int.Parse(i.Trim())));
                    var sol = js ? Pg.Code(Pg.ReadInt(true, "n"), "const ds = [];", "for (let d = 1; d <= n; d++) {", "  if (n % d === 0) ds.push(d);", "}", "console.log(ds.join(\" \"));", "console.log(`Total: ${ds.length}`);")
                        : Pg.Code(Pg.ReadInt(false, "n"), "ds = []", "for d in range(1, n + 1):", "    if n % d == 0:", "        ds.append(d)", "print(\" \".join(str(d) for d in ds))", "print(f\"Total: {len(ds)}\")");
                    return Ex.Code(Pg.Prompt(L.T("Leia um inteiro `n` (n ≥ 1). Mostre numa linha todos os divisores de `n` em ordem crescente, separados por espaço, e na linha seguinte `Total: <quantidade>`."), tests[0]),
                        Pg.Starter(js, Pg.ReadInt(js, "n")), sol, tests,
                        L.T("Testamos cada candidato d de 1 até n: se n % d == 0, d é divisor. Guardamos os divisores numa lista para mostrá-los juntos e contá-los."));
                }
                {
                    string Model(int n) { int steps = 0, mx = n; long v = n; while (v != 1) { v = v % 2 == 0 ? v / 2 : 3 * v + 1; steps++; mx = (int)Math.Max(mx, v); } return $"Passos: {steps}\nMaior valor: {mx}"; }
                    var tests = Pg.Tests(new[] { "6", "1", "27", Ex.Int(r, 2, 60).ToString(), Ex.Int(r, 2, 60).ToString() }, i => Model(int.Parse(i.Trim())));
                    var sol = js ? Pg.Code(Pg.ReadInt(true, "n"), "let passos = 0, maior = n;", "while (n !== 1) {", "  n = n % 2 === 0 ? n / 2 : 3 * n + 1;", "  passos++;", "  if (n > maior) maior = n;", "}", "console.log(`Passos: ${passos}`);", "console.log(`Maior valor: ${maior}`);")
                        : Pg.Code(Pg.ReadInt(false, "n"), "passos = 0", "maior = n", "while n != 1:", "    if n % 2 == 0:", "        n = n // 2", "    else:", "        n = 3 * n + 1", "    passos += 1", "    if n > maior:", "        maior = n", "print(f\"Passos: {passos}\")", "print(f\"Maior valor: {maior}\")");
                    return Ex.Code(Pg.Prompt(L.T("Leia um inteiro `n` (n ≥ 1). Repita até chegar em 1: se o número é par, divida por 2; se é ímpar, troque por 3·n + 1. Mostre quantos passos foram dados e o maior valor que apareceu (contando o valor inicial)."), tests[0]),
                        Pg.Starter(js, Pg.ReadInt(js, "n")), sol, tests,
                        L.T("É um laço while, pois não sabemos quantas voltas serão. A cada volta aplicamos a regra, contamos o passo e guardamos o maior valor visto."));
                }
            }
            default:
            {
                int kind = r.Next(3);
                if (kind == 0)
                {
                    string Model(int lim) { int k = 0, s = 0; while (s < lim) { k++; s += k; } return $"{k} {s}"; }
                    var tests = Pg.Tests(new[] { "1000", "10", "1", Ex.Int(r, 2, 500).ToString(), Ex.Int(r, 100, 5000).ToString() }, i => Model(int.Parse(i.Trim())));
                    var sol = js ? Pg.Code(Pg.ReadInt(true, "lim"), "let k = 0, soma = 0;", "while (soma < lim) {", "  k++;", "  soma += k;", "}", "console.log(k, soma);")
                        : Pg.Code(Pg.ReadInt(false, "lim"), "k = 0", "soma = 0", "while soma < lim:", "    k += 1", "    soma += k", "print(k, soma)");
                    return Ex.Code(Pg.Prompt(L.T("Leia um limite `L` (L ≥ 1). Some 1 + 2 + 3 + ... até a soma ser maior ou igual a `L`. Mostre numa linha o último número somado e a soma obtida, separados por espaço."), tests[0]),
                        Pg.Starter(js, Pg.ReadInt(js, "lim")), sol, tests,
                        L.T("Não sabemos quantas parcelas serão precisas: o while repete \"enquanto a soma ainda é menor que L\", somando o próximo inteiro a cada volta."));
                }
                if (kind == 1)
                {
                    string Model(int n) { int steps = 0; while (n >= 10) { int p = 1; while (n > 0) { p *= n % 10; n /= 10; } n = p; steps++; } return $"{steps} {n}"; }
                    var tests = Pg.Tests(new[] { "39", "7", "999", "2777", Ex.Int(r, 10, 9999).ToString(), Ex.Int(r, 10, 9999).ToString() }, i => Model(int.Parse(i.Trim())));
                    var sol = js ? Pg.Code(Pg.ReadInt(true, "n"), "let passos = 0;", "while (n >= 10) {", "  let p = 1;", "  while (n > 0) {", "    p *= n % 10;", "    n = Math.trunc(n / 10);", "  }", "  n = p;", "  passos++;", "}", "console.log(passos, n);")
                        : Pg.Code(Pg.ReadInt(false, "n"), "passos = 0", "while n >= 10:", "    p = 1", "    while n > 0:", "        p *= n % 10", "        n = n // 10", "    n = p", "    passos += 1", "print(passos, n)");
                    return Ex.Code(Pg.Prompt(L.T("Leia um inteiro `n` (n ≥ 1). Enquanto `n` tiver mais de um dígito, troque-o pelo PRODUTO dos seus dígitos (39 → 3·9 = 27 → 2·7 = 14 → 1·4 = 4). Mostre quantas trocas foram feitas e o dígito final, separados por espaço."), tests[0]),
                        Pg.Starter(js, Pg.ReadInt(js, "n")), sol, tests,
                        L.T("São dois laços: o de fora repete enquanto n ≥ 10; o de dentro percorre os dígitos de n (n % 10 e n ÷ 10) multiplicando-os. Depois n passa a valer esse produto."));
                }
                {
                    string Model(int n) { int c = 0; for (int a = 2; a <= n; a++) { int s = 1; for (int d = 2; d * d <= a; d++) if (a % d == 0) { s += d; if (d != a / d) s += a / d; } if (s == a) c++; } return c.ToString(); }
                    var tests = Pg.Tests(new[] { "30", "5", "500", Ex.Int(r, 6, 600).ToString(), Ex.Int(r, 6, 600).ToString() }, i => Model(int.Parse(i.Trim())));
                    var sol = js ? Pg.Code(Pg.ReadInt(true, "n"), "let c = 0;", "for (let a = 2; a <= n; a++) {", "  let s = 0;", "  for (let d = 1; d < a; d++) {", "    if (a % d === 0) s += d;", "  }", "  if (s === a) c++;", "}", "console.log(c);")
                        : Pg.Code(Pg.ReadInt(false, "n"), "c = 0", "for a in range(2, n + 1):", "    s = 0", "    for d in range(1, a):", "        if a % d == 0:", "            s += d", "    if s == a:", "        c += 1", "print(c)");
                    return Ex.Code(Pg.Prompt(L.T("Um número é perfeito quando é igual à soma dos seus divisores menores que ele (6 = 1 + 2 + 3). Leia `n` (n ≥ 2) e mostre quantos números perfeitos existem de 2 até `n`."), tests[0]),
                        Pg.Starter(js, Pg.ReadInt(js, "n")), sol, tests,
                        L.T("Laço duplo: o de fora percorre os candidatos a; o de dentro soma os divisores d < a. Se a soma é igual a a, contamos um perfeito."));
                }
            }
        }
    }

    static Exercise MakeOut(bool js, int lv, Random r)
    {
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        switch (lv)
        {
            case 0:
            {
                int a = Ex.Int(r, 1, 4), b = a + Ex.Int(r, 3, 6), s = Ex.Pick(r, 1, 1, 2);
                var lines = new List<string>(); var outs = new List<string>();
                if (r.Next(2) == 0)
                {
                    int tot = 0; for (int i = a; i < b; i += s) tot += i;
                    lines.Add(js ? "let total = 0;" : "total = 0");
                    lines.Add(js ? $"for (let i = {a}; i < {b}; i += {s}) {{\n  total += i;\n}}" : $"for i in range({a}, {b}, {s}):\n    total += i");
                    lines.Add(pr("total")); outs.Add(tot.ToString());
                }
                else
                {
                    lines.Add(js ? $"for (let i = {a}; i < {b}; i += {s}) {{\n  console.log(i * i);\n}}" : $"for i in range({a}, {b}, {s}):\n    print(i * i)");
                    for (int i = a; i < b; i += s) outs.Add((i * i).ToString());
                }
                string outp = string.Join("\n", outs);
                return Ex.Output(PgOut.Prompt(outp), string.Join("\n", lines), outp,
                    L.F("O laço começa em {0}, anda de {1} em {1} e para ANTES de chegar em {2} (o limite não entra). Acompanhe i a cada volta.", a, s, b));
            }
            case 1:
            {
                int n = Ex.Int(r, 30, 200), c = 0, steps = Ex.Int(r, 2, 3);
                var vals = new List<int>(); int v = n;
                while (v > 1) { vals.Add(v); v /= steps; c++; }
                string code = js ? $"let n = {n};\nlet c = 0;\nwhile (n > 1) {{\n  n = Math.trunc(n / {steps});\n  c++;\n}}\nconsole.log(c, n);" : $"n = {n}\nc = 0\nwhile n > 1:\n    n = n // {steps}\n    c += 1\nprint(c, n)";
                string outp = $"{c} {v}";
                return Ex.Output(PgOut.Prompt(outp), code, outp,
                    L.F("Divide-se n por {0} (divisão inteira) até n deixar de ser maior que 1: n vale {1}. Foram {2} voltas e n terminou em {3}.", steps, string.Join(" → ", vals.Append(v)), c, v));
            }
            case 2:
            {
                int rows = Ex.Int(r, 3, 5); string ch = Ex.Pick(r, "*", "#", "o");
                bool numbers = r.Next(2) == 0;
                var outs = Enumerable.Range(1, rows).Select(i => numbers ? string.Concat(Enumerable.Range(1, i)) : new string(ch[0], i)).ToList();
                string code = numbers
                    ? (js ? $"for (let i = 1; i <= {rows}; i++) {{\n  let linha = \"\";\n  for (let j = 1; j <= i; j++) {{\n    linha += j;\n  }}\n  console.log(linha);\n}}"
                          : $"for i in range(1, {rows + 1}):\n    linha = \"\"\n    for j in range(1, i + 1):\n        linha += str(j)\n    print(linha)")
                    : (js ? $"for (let i = 1; i <= {rows}; i++) {{\n  console.log(\"{ch}\".repeat(i));\n}}" : $"for i in range(1, {rows + 1}):\n    print(\"{ch}\" * i)");
                string outp = string.Join("\n", outs);
                return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("Na volta i do laço de fora a linha tem i itens (um repetido i vezes, ou os números de 1 até i colados). Escreva uma linha por volta."));
            }
            default:
            {
                int lim = Ex.Int(r, 20, 60), a = Ex.Int(r, 2, 4), cap = Ex.Int(r, 8, 25);
                int total = 0, cnt = 0; var outs = new List<string>();
                for (int i = 1; i <= 40; i++)
                {
                    if (i % a == 0) continue;
                    total += i; cnt++;
                    if (total > lim) { outs.Add($"{i} {total}"); break; }
                    if (cnt == cap) break;
                }
                outs.Add($"{cnt} {total}");
                string code = js
                    ? $"let total = 0, cnt = 0;\nfor (let i = 1; i <= 40; i++) {{\n  if (i % {a} === 0) continue;\n  total += i;\n  cnt++;\n  if (total > {lim}) {{\n    console.log(i, total);\n    break;\n  }}\n  if (cnt === {cap}) break;\n}}\nconsole.log(cnt, total);"
                    : $"total = 0\ncnt = 0\nfor i in range(1, 41):\n    if i % {a} == 0:\n        continue\n    total += i\n    cnt += 1\n    if total > {lim}:\n        print(i, total)\n        break\n    if cnt == {cap}:\n        break\nprint(cnt, total)";
                string outp = string.Join("\n", outs);
                return Ex.Output(PgOut.Prompt(outp), code, outp,
                    L.F("`continue` pula os múltiplos de {0} sem somá-los; `break` encerra o laço de uma vez. Some os não-múltiplos de {0} (1, 2, ...) acompanhando total e cnt até passar de {1} ou chegar a {2} termos. O print final roda sempre depois do laço.", a, lim, cap));
            }
        }
    }
}
