namespace EasyCreator.Exercises;

/// <summary>js-16 (iterators and generators): writing finite and infinite generators, and predicting the interleaving of a generator's side effects with its consumer.</summary>
[Exercises("js-16")]
public sealed class P19Generators : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(lv, r) : MakeOut(lv, r);
    }

    static Exercise MakeCode(int lv, Random r)
    {
        switch (lv)
        {
            case 0:
            {
                int mode = r.Next(3);
                string[] descs = { L.T("os números de 1 até `n`"), L.T("os quadrados dos números de 1 até `n`"), L.T("a contagem regressiva de `n` até 1") };
                string[] bodies = { "for (let i = 1; i <= n; i++) yield i;", "for (let i = 1; i <= n; i++) yield i * i;", "for (let i = n; i >= 1; i--) yield i;" };
                var tests = Pg.Tests(new[] { "1", "5", Ex.Int(r, 2, 12).ToString(), Ex.Int(r, 2, 12).ToString() }, i => { int n = int.Parse(i.Trim()); var l = Enumerable.Range(1, n); return string.Join(" ", mode == 0 ? l : mode == 1 ? l.Select(x => x * x) : l.Reverse()); });
                string sol = $"function* sequencia(n) {{\n  {bodies[mode]}\n}}\nconst n = Number(prompt());\nconsole.log([...sequencia(n)].join(\" \"));";
                return Ex.Code(Pg.Prompt(L.F("Escreva o GERADOR `sequencia(n)` (uma `function*` que usa `yield`) que produz {0}, nessa ordem. Depois leia `n` (n ≥ 1) e mostre, numa linha separada por espaços, todos os valores produzidos por `sequencia(n)`.", descs[mode]), tests[0]),
                    "const n = Number(prompt());\n// escreva o gerador e mostre os valores", sol, tests,
                    L.T("Um gerador é uma função que pode PAUSAR em cada `yield` entregando um valor. Espalhar o gerador com [...gen] consome todos os valores até ele terminar."));
            }
            case 1:
            {
                int mode = r.Next(3);
                string[] descs = { L.T("os números pares de 0 até `limite`, inclusive"), L.T("os `limite` primeiros números da sequência de Fibonacci (0, 1, 1, 2, 3, 5, ...)"), L.T("os números de 1 até `limite` que NÃO são múltiplos de 3") };
                string[] bodies = { "for (let i = 0; i <= limite; i += 2) yield i;", "let a = 0, b = 1;\n  for (let i = 0; i < limite; i++) {\n    yield a;\n    [a, b] = [b, a + b];\n  }", "for (let i = 1; i <= limite; i++) {\n    if (i % 3 !== 0) yield i;\n  }" };
                IEnumerable<long> Seq(int lim) { if (mode == 0) { for (int i = 0; i <= lim; i += 2) yield return i; } else if (mode == 1) { long a = 0, b = 1; for (int i = 0; i < lim; i++) { yield return a; (a, b) = (b, a + b); } } else { for (int i = 1; i <= lim; i++) if (i % 3 != 0) yield return i; } }
                var tests = Pg.Tests(new[] { "1", "10", Ex.Int(r, 2, 25).ToString(), Ex.Int(r, 2, 25).ToString(), Ex.Int(r, 2, 25).ToString() }, i => string.Join(" ", Seq(int.Parse(i.Trim()))));
                string sol = $"function* valores(limite) {{\n  {bodies[mode]}\n}}\nconst limite = Number(prompt());\nconst lista = [];\nfor (const v of valores(limite)) lista.push(v);\nconsole.log(lista.join(\" \"));";
                return Ex.Code(Pg.Prompt(L.F("Escreva o gerador `valores(limite)` que produz {0}. Leia `limite` (≥ 1) e mostre os valores produzidos numa linha, separados por espaço, percorrendo o gerador com `for...of`.", descs[mode]), tests[0]),
                    "const limite = Number(prompt());\n// escreva o gerador e percorra-o com for...of", sol, tests,
                    L.T("O gerador mantém suas variáveis (como a e b do Fibonacci) entre um `yield` e o próximo. O `for...of` pede um valor de cada vez até o gerador acabar."));
            }
            case 2:
            {
                int mode = r.Next(3);
                string[] descs = { L.T("os múltiplos de `k`: k, 2k, 3k, ..."), L.T("as potências de `k`: 1, k, k², k³, ..."), L.T("os números triangulares 1, 3, 6, 10, 15, ... (somas 1, 1+2, 1+2+3, ...) — aqui `k` não é usado, mas é lido mesmo assim") };
                string[] bodies = { "let x = k;\n  while (true) {\n    yield x;\n    x += k;\n  }", "let x = 1;\n  while (true) {\n    yield x;\n    x *= k;\n  }", "let x = 0, i = 1;\n  while (true) {\n    x += i;\n    i++;\n    yield x;\n  }" };
                long Nth(int k, int idx) => mode == 0 ? (long)k * (idx + 1) : mode == 1 ? (long)Math.Pow(k, idx) : (long)(idx + 1) * (idx + 2) / 2;
                var tests = Pg.Tests(new[] { "3\n4", "2\n1", $"{Ex.Int(r, 2, 6)}\n{Ex.Int(r, 2, 9)}", $"{Ex.Int(r, 2, 6)}\n{Ex.Int(r, 2, 9)}", $"{Ex.Int(r, 2, 6)}\n{Ex.Int(r, 2, 9)}" }, i => { var p = i.TrimEnd('\n').Split('\n').Select(int.Parse).ToArray(); return string.Join(" ", Enumerable.Range(0, p[1]).Select(x => Nth(p[0], x))); });
                string sol = $"function* infinito(k) {{\n  {bodies[mode]}\n}}\nfunction* pegar(gen, n) {{\n  if (n <= 0) return;\n  let c = 0;\n  for (const v of gen) {{\n    yield v;\n    c++;\n    if (c === n) return;\n  }}\n}}\nconst k = Number(prompt());\nconst n = Number(prompt());\nconsole.log([...pegar(infinito(k), n)].join(\" \"));";
                return Ex.Code(Pg.Prompt(L.F("Escreva dois geradores: `infinito(k)`, que produz SEM FIM {0}; e `pegar(gen, n)`, que produz só os `n` primeiros valores de qualquer gerador `gen` e então termina. Leia `k` e `n` (um por linha) e mostre os `n` primeiros valores de `infinito(k)`, separados por espaço.", descs[mode]), tests[0]),
                    "const k = Number(prompt());\nconst n = Number(prompt());\n// escreva os dois geradores", sol, tests,
                    L.T("O gerador infinito não gasta memória nem trava porque só calcula o próximo valor quando alguém pede. `pegar` é quem decide parar: conta os valores e dá `return` ao chegar em n, e o laço infinito nunca é consumido além disso."));
            }
            default:
            {
                string Model(string inp) { var ls = inp.TrimEnd('\n').Split('\n'); var a = ls[0].Split(' ').Select(int.Parse).ToArray(); var b = ls[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray(); var o = new List<int>(); int i = 0; while (i < a.Length || i < b.Length) { if (i < a.Length) o.Add(a[i]); if (i < b.Length) o.Add(b[i]); i++; } return string.Join(" ", o); }
                string Pair() => string.Join(" ", Pg.Arr(r, Ex.Int(r, 1, 6), 1, 9)) + "\n" + string.Join(" ", Pg.Arr(r, Ex.Int(r, 1, 6), 1, 9));
                var tests = Pg.Tests(new[] { "1 2 3\n10 20", Pair(), Pair(), Pair() }, Model);
                string sol = "function* intercalar(a, b) {\n  const ia = a[Symbol.iterator]();\n  const ib = b[Symbol.iterator]();\n  while (true) {\n    const x = ia.next();\n    const y = ib.next();\n    if (x.done && y.done) return;\n    if (!x.done) yield x.value;\n    if (!y.done) yield y.value;\n  }\n}\nconst a = prompt().split(\" \").map(Number);\nconst b = prompt().split(\" \").map(Number);\nconsole.log([...intercalar(a, b)].join(\" \"));";
                return Ex.Code(Pg.Prompt(L.T("Leia duas linhas, cada uma com inteiros separados por espaço. Escreva o gerador `intercalar(a, b)`, que produz um elemento de `a`, um de `b`, outro de `a`, outro de `b`, e assim por diante; quando uma lista acaba, produz o que sobrar da outra. Mostre os valores produzidos numa linha separada por espaço."), tests[0]),
                    "const a = prompt().split(\" \").map(Number);\nconst b = prompt().split(\" \").map(Number);\n// escreva o gerador", sol, tests,
                    L.T("Pegamos um iterador de cada lista (`[Symbol.iterator]()`) e pedimos `next()` aos dois em turnos. Cada resultado tem `done` (acabou?) e `value`. Quando ambos acabaram, o gerador termina; enquanto só um acabou, o outro continua sozinho."));
            }
        }
    }

    static Exercise MakeOut(int lv, Random r)
    {
        int m = Ex.Int(r, 2, 3);
        string[] labels = Pg.Words.OrderBy(_ => r.Next()).Take(m + 1).ToArray();
        var vals = Enumerable.Range(0, m).Select(_ => Ex.Int(r, 1, 9)).ToArray();
        string gen = "function* g() {\n" + string.Concat(Enumerable.Range(0, m).Select(i => $"  console.log(\"{labels[i]}\");\n  yield {vals[i]};\n")) + $"  console.log(\"{labels[m]}\");\n}}\n";
        var o = new List<string>();
        if (lv <= 1)
        {
            int calls = lv == 0 ? m : m + 1;
            var code = gen + "const it = g();\nconsole.log(\"início\");\n";
            o.Add("início");
            int pos = 0;
            for (int c = 0; c < calls; c++)
            {
                bool lastDone = pos >= m;
                if (lv == 1 && c == calls - 1) { code += "console.log(it.next().done);\n"; if (pos < m) { o.Add(labels[pos]); o.Add("false"); pos++; } else { o.Add(labels[m]); o.Add("true"); pos = m + 1; } }
                else if (pos < m) { code += "console.log(it.next().value);\n"; o.Add(labels[pos]); o.Add(vals[pos].ToString()); pos++; }
                else { code += "console.log(it.next().value);\n"; if (pos == m) { o.Add(labels[m]); pos++; } o.Add("undefined"); }
            }
            string outp = string.Join("\n", o);
            return Ex.Output(PgOut.Prompt(outp), code.TrimEnd('\n'), outp, L.T("Criar o gerador (g()) não executa nada. Cada next() roda o corpo até o PRÓXIMO yield, e só então o valor é entregue ao console.log de fora. Depois do último yield, o next() seguinte executa o resto do corpo e devolve done: true e value: undefined."));
        }
        {
            int stop = vals[r.Next(m)]; bool useBreak = true;
            var code = gen + $"for (const v of g()) {{\n  console.log(\"usa\", v);\n  if (v === {stop}) break;\n}}\nconsole.log(\"fim\");";
            bool stopped = false;
            for (int i = 0; i < m && !stopped; i++) { o.Add(labels[i]); o.Add($"usa {vals[i]}"); if (vals[i] == stop) stopped = true; }
            if (!stopped) o.Add(labels[m]);
            o.Add("fim");
            _ = useBreak;
            string outp = string.Join("\n", o);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("O for...of e o gerador trabalham em revezamento: o gerador roda até o yield, o laço usa o valor, e só então o gerador continua. Ao dar break em v = {0}, o gerador é abandonado e o código depois do último yield (o último log) NUNCA roda.", stop));
        }
    }
}
