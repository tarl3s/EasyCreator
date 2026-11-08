namespace EasyCreator.Exercises;

/// <summary>Lessons py-12 / js-11 (lambda, sorted, comprehensions / map, filter, reduce): transforming lists, ordering with a key, grouping, and the closure-in-a-loop trap.</summary>
[Exercises("py-12", "js-11")]
public sealed class P12Functional : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        bool js = Pg.Js(lesson);
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(js, lv, r) : MakeOut(js, lv, r);
    }

    record Tf(string Desc, string Py, string Js, Func<int[], IEnumerable<int>> F);

    static Tf[] Tfs(Random r)
    {
        int k = Ex.Int(r, 2, 9), t = Ex.Int(r, 5, 25);
        return new[]
        {
            new Tf(L.T("o quadrado de cada número"), "[x * x for x in v]", "v.map((x) => x * x)", a => a.Select(x => x * x)),
            new Tf(L.F("cada número multiplicado por {0}", k), $"[x * {k} for x in v]", $"v.map((x) => x * {k})", a => a.Select(x => x * k)),
            new Tf(L.T("somente os números pares"), "[x for x in v if x % 2 == 0]", "v.filter((x) => x % 2 === 0)", a => a.Where(x => x % 2 == 0)),
            new Tf(L.F("somente os números maiores que {0}", t), $"[x for x in v if x > {t}]", $"v.filter((x) => x > {t})", a => a.Where(x => x > t)),
            new Tf(L.T("somente os números ímpares, cada um somado a 1"), "[x + 1 for x in v if x % 2 == 1]", "v.filter((x) => x % 2 === 1).map((x) => x + 1)", a => a.Where(x => x % 2 == 1).Select(x => x + 1)),
        };
    }

    static List<string> NumLists(Random r, int lo, int hi, Func<int[], bool> ok)
    {
        var l = new List<string>();
        while (l.Count < 5) { var v = Pg.Arr(r, Ex.Int(r, 4, 9), lo, hi); if (ok(v)) l.Add(v.Length + "\n" + string.Join(" ", v)); }
        return l;
    }
    static int[] Parse(string i) => i.TrimEnd('\n').Split('\n')[1].Split(' ').Select(int.Parse).ToArray();

    static Exercise MakeCode(bool js, int lv, Random r)
    {
        string rd = Pg.Code(Pg.ReadInt(js, "n"), Pg.ReadInts(js, "v"));
        string starter = rd + "\n" + Pg.Comment(js, L.T("escreva o resto do programa aqui"));
        string join(string e) => js ? $"console.log({e}.join(\" \"));" : $"print(\" \".join(str(x) for x in {e}))";
        switch (lv)
        {
            case 0:
            {
                var tf = Ex.Pick(r, Tfs(r).Take(4).ToArray());
                var tests = Pg.Tests(NumLists(r, 1, 40, v => tf.F(v).Any()), i => string.Join(" ", tf.F(Parse(i))));
                return Ex.Code(Pg.Prompt(L.F("Leia `n` e uma linha com `n` inteiros. Mostre, numa linha separada por espaços, {0} (na ordem original).", tf.Desc) + " " + (js ? L.T("Use map / filter.") : L.T("Use uma compreensão de lista.")), tests[0]),
                    starter, rd + "\n" + join(js ? tf.Js : tf.Py), tests,
                    js ? L.T("map aplica uma função a cada elemento e devolve a nova lista; filter mantém só os elementos para os quais a função devolve verdadeiro.") : L.T("Uma compreensão [expressão for x in lista if condição] cria a nova lista: a expressão transforma cada x e o if (opcional) escolhe quais entram."));
            }
            case 1:
            {
                var tf = Ex.Pick(r, Tfs(r).Skip(4).Concat(Tfs(r).Skip(2).Take(2)).ToArray());
                var tests = Pg.Tests(NumLists(r, 1, 40, v => tf.F(v).Any()), i => { var o = tf.F(Parse(i)).ToArray(); return string.Join(" ", o) + "\nSoma: " + o.Sum() + "\nMaior: " + o.Max(); });
                string sol = rd + "\n" + (js ? $"const r = {tf.Js};\nconsole.log(r.join(\" \"));\nconsole.log(`Soma: ${{r.reduce((a, b) => a + b, 0)}}`);\nconsole.log(`Maior: ${{Math.max(...r)}}`);" : $"r = {tf.Py}\nprint(\" \".join(str(x) for x in r))\nprint(f\"Soma: {{sum(r)}}\")\nprint(f\"Maior: {{max(r)}}\")");
                return Ex.Code(Pg.Prompt(L.F("Leia `n` e uma linha com `n` inteiros. Considere a lista com {0}. Mostre três linhas: essa lista (separada por espaços), `Soma: <soma dela>` e `Maior: <maior valor dela>`.", tf.Desc) + " " + L.T("(Garantido: a lista não fica vazia.)"), tests[0]),
                    starter, sol, tests,
                    js ? L.T("Primeiro construímos a lista nova com filter/map; depois a soma sai de um reduce (acumulador começando em 0) e o maior de Math.max com a lista espalhada (...r).") : L.T("Primeiro construímos a lista nova com a compreensão; depois sum e max (funções prontas) dão a soma e o maior."));
            }
            case 2:
            {
                int mode = r.Next(3);
                var words = Enumerable.Range(0, 5).Select(_ => string.Join(" ", Enumerable.Range(0, Ex.Int(r, 4, 7)).Select(__ => Ex.Pick(r, Pg.Words)))).ToList();
                var nums = NumLists(r, 10, 99, v => true);
                if (mode == 2)
                {
                    var tests = Pg.Tests(nums, i => string.Join(" ", Parse(i).OrderBy(x => x % 10).ThenBy(x => x)));
                    string sol = rd + "\n" + (js ? "v.sort((a, b) => (a % 10) - (b % 10) || a - b);\nconsole.log(v.join(\" \"));" : "v.sort(key=lambda x: (x % 10, x))\nprint(\" \".join(str(x) for x in v))");
                    return Ex.Code(Pg.Prompt(L.T("Leia `n` e uma linha com `n` inteiros positivos. Mostre-os ordenados pelo ÚLTIMO DÍGITO (do menor para o maior); se dois terminam no mesmo dígito, o menor número vem primeiro."), tests[0]), starter, sol, tests,
                        L.T("A chave de ordenação é o par (último dígito, o próprio número): o primeiro item manda, e o segundo desempata."));
                }
                else
                {
                    bool byLen = mode == 0;
                    var tests = Pg.Tests(words, i => { var p = i.Trim().Split(' '); return string.Join(" ", byLen ? p.OrderBy(w => w.Length).ThenBy(w => w, StringComparer.Ordinal) : p.OrderBy(w => w[^1]).ThenBy(w => w, StringComparer.Ordinal)); });
                    string key = byLen ? (js ? "(a, b) => a.length - b.length || (a < b ? -1 : a > b ? 1 : 0)" : "lambda w: (len(w), w)") : (js ? "(a, b) => (a.charCodeAt(a.length - 1) - b.charCodeAt(b.length - 1)) || (a < b ? -1 : a > b ? 1 : 0)" : "lambda w: (w[-1], w)");
                    string sol = Pg.Code(Pg.ReadStr(js, "s"), js ? "const p = s.split(\" \");" : "p = s.split()", js ? $"p.sort({key});" : $"p.sort(key={key})", js ? "console.log(p.join(\" \"));" : "print(\" \".join(p))");
                    return Ex.Code(Pg.Prompt((byLen ? L.T("Leia uma frase (palavras separadas por um espaço, minúsculas) e mostre as palavras ordenadas pelo TAMANHO, da menor para a maior; palavras do mesmo tamanho ficam em ordem alfabética.") : L.T("Leia uma frase (palavras separadas por um espaço, minúsculas) e mostre as palavras ordenadas pela ÚLTIMA LETRA; palavras com a mesma última letra ficam em ordem alfabética.")), tests[0]),
                        Pg.Code(Pg.ReadStr(js, "s"), Pg.Comment(js, L.T("escreva o resto do programa aqui"))), sol, tests,
                        L.T("Ordenamos com uma função que diz como comparar. Duas regras em cascata: a principal (tamanho ou última letra) e, se empatar, a ordem alfabética."));
                }
            }
            default:
            {
                var tests = Pg.Tests(Enumerable.Range(0, 5).Select(_ => string.Join(" ", Enumerable.Range(0, Ex.Int(r, 5, 9)).Select(__ => Ex.Pick(r, Pg.Words)))), i =>
                {
                    var g = i.Trim().Split(' ').GroupBy(w => w[0]).OrderByDescending(x => x.Count()).ThenBy(x => x.Key);
                    return string.Join("\n", g.Select(x => $"{x.Key}: {x.Count()}"));
                });
                string sol = js ? Pg.Code(Pg.ReadStr(true, "s"), "const cont = {};", "for (const w of s.split(\" \")) {", "  cont[w[0]] = (cont[w[0]] ?? 0) + 1;", "}", "const ls = Object.keys(cont).sort((a, b) => cont[b] - cont[a] || (a < b ? -1 : 1));", "for (const l of ls) console.log(`${l}: ${cont[l]}`);")
                    : Pg.Code(Pg.ReadStr(false, "s"), "cont = {}", "for w in s.split():", "    cont[w[0]] = cont.get(w[0], 0) + 1", "for l in sorted(cont, key=lambda l: (-cont[l], l)):", "    print(l + \": \" + str(cont[l]))");
                return Ex.Code(Pg.Prompt(L.T("Leia uma frase (palavras separadas por um espaço, minúsculas). Agrupe as palavras pela PRIMEIRA LETRA e mostre uma linha `letra: quantidade` para cada letra, da letra com MAIS palavras para a com menos; em empate, em ordem alfabética das letras."), tests[0]),
                    Pg.Code(Pg.ReadStr(js, "s"), Pg.Comment(js, L.T("escreva o resto do programa aqui"))), sol, tests,
                    L.T("Contamos num dicionário letra → quantidade. A ordenação precisa de duas regras: quantidade DECRESCENTE (em Python, a chave usa −quantidade; em JavaScript, cont[b] − cont[a]) e, no empate, a letra em ordem crescente."));
            }
        }
    }

    static Exercise MakeOut(bool js, int lv, Random r)
    {
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        if (lv == 0)
        {
            var tf = Ex.Pick(r, Tfs(r)); var v = Pg.Arr(r, Ex.Int(r, 4, 6), 1, 40); for (int g = 0; g < 200 && !tf.F(v).Any(); g++) v = Pg.Arr(r, Ex.Int(r, 4, 6), 1, 40);
            string code = (js ? $"const v = [{string.Join(", ", v)}];\n" : $"v = [{string.Join(", ", v)}]\n") + pr(js ? tf.Js + ".join(\" \")" : $"\" \".join(str(x) for x in {tf.Py})");
            string outp = string.Join(" ", tf.F(v));
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("Aplique a regra a cada elemento da lista, na ordem. Se houver filtro, só os que passam entram na lista nova."));
        }
        if (lv == 1)
        {
            var ws = Enumerable.Range(0, 5).Select(_ => Ex.Pick(r, Pg.Words)).Distinct().ToArray();
            bool rev = r.Next(2) == 0;
            var o = ws.OrderBy(w => w.Length).ThenBy(w => w, StringComparer.Ordinal).ToList(); if (rev) o.Reverse();
            string lst = string.Join(", ", ws.Select(w => $"\"{w}\""));
            string key = js ? "[...p].sort((a, b) => a.length - b.length || (a < b ? -1 : 1))" : "sorted(p, key=lambda w: (len(w), w))";
            string code = (js ? $"const p = [{lst}];\n" : $"p = [{lst}]\n") + (rev ? (js ? $"console.log({key}.reverse().join(\" \"));" : $"print(\" \".join(reversed({key})))") : (js ? $"console.log({key}.join(\" \"));" : $"print(\" \".join({key}))"));
            code += "\n" + pr(js ? "p.join(\" \")" : "\" \".join(p)");
            string outp = string.Join(" ", o) + "\n" + string.Join(" ", ws);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("A ordenação por tamanho desempata alfabeticamente. Ela cria uma lista NOVA (sorted / cópia + sort): a lista original p continua na ordem em que foi escrita, e é isso que a última linha mostra."));
        }
        if (lv == 2)
        {
            var v = Pg.Arr(r, Ex.Int(r, 5, 7), -5, 15); int t = Ex.Int(r, 2, 8);
            var lines = new List<string> { js ? $"const v = [{string.Join(", ", v)}];" : $"v = [{string.Join(", ", v)}]" };
            var outs = new List<string>();
            lines.Add(pr(js ? $"v.filter((x) => x > {t}).length" : $"len([x for x in v if x > {t}])")); outs.Add(v.Count(x => x > t).ToString());
            lines.Add(pr(js ? "v.reduce((a, x) => a + x * x, 0)" : "sum(x * x for x in v)")); outs.Add(v.Sum(x => x * x).ToString());
            lines.Add(pr(js ? "v.every((x) => x > -6)" : "all(x > -6 for x in v)")); outs.Add(js ? "true" : "True");
            lines.Add(pr(js ? $"v.some((x) => x === {t})" : $"any(x == {t} for x in v)")); outs.Add(v.Contains(t) ? (js ? "true" : "True") : (js ? "false" : "False"));
            lines.Add(pr(js ? "v.map((x) => Math.abs(x)).reduce((a, b) => Math.max(a, b))" : "max(abs(x) for x in v)")); outs.Add(v.Max(Math.Abs).ToString());
            string outp = string.Join("\n", outs);
            return Ex.Output(PgOut.Prompt(outp), string.Join("\n", lines), outp, L.T("Resolva uma linha por vez: contar os que passam no filtro, somar os quadrados, e os testes \"todos\" (every / all) e \"algum\" (some / any), que dão verdadeiro ou falso."));
        }
        {
            int n = Ex.Int(r, 3, 5), b = Ex.Int(r, 2, 9);
            if (r.Next(2) == 0 || !js && false)
            {
                // Python: lambdas in a comprehension all share the final i; a default argument freezes it.
                if (js)
                {
                    bool useVar = r.Next(2) == 0;
                    string code = $"const fs = [];\nfor ({(useVar ? "var" : "let")} i = 0; i < {n}; i++) {{\n  fs.push((x) => x * i);\n}}\nconsole.log(fs.map((f) => f({b})).join(\" \"));";
                    string outp = string.Join(" ", Enumerable.Range(0, n).Select(i => (useVar ? n : i) * b));
                    return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("Com `var`, existe UMA só variável i para o laço inteiro: quando as funções são chamadas, o laço já terminou e i vale o valor final. Com `let`, cada volta tem a sua própria cópia de i, e cada função guarda a sua."));
                }
                else
                {
                    bool frozen = r.Next(2) == 0;
                    string code = $"fs = [lambda x{(frozen ? ", i=i" : "")}: x * i for i in range({n})]\nprint(\" \".join(str(f({b})) for f in fs))";
                    string outp = string.Join(" ", Enumerable.Range(0, n).Select(i => (frozen ? i : n - 1) * b));
                    return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("A lambda não copia o i: ela olha o valor de i NO MOMENTO em que é chamada, e nessa hora a compreensão já terminou (i vale o último valor). Com `i=i` como argumento padrão, o valor de cada volta é congelado na criação."));
                }
            }
            {
                var v = Pg.Arr(r, 5, 1, 9); int k = Ex.Int(r, 2, 4);
                string code = js ? $"const v = [{string.Join(", ", v)}];\nconst mult = (k) => (x) => x * k;\nconsole.log(v.map(mult({k})).join(\" \"));\nconsole.log(v.map(mult({k})).filter((x) => x > 10).length);"
                    : $"v = [{string.Join(", ", v)}]\nmult = lambda k: (lambda x: x * k)\nprint(\" \".join(str(mult({k})(x)) for x in v))\nprint(len([x for x in map(mult({k}), v) if x > 10]))";
                string outp = string.Join(" ", v.Select(x => x * k)) + "\n" + v.Count(x => x * k > 10);
                return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("mult({0}) é uma função nova que multiplica por {0}. Aplicada a cada elemento, dá a primeira linha; a segunda linha conta quantos resultados passam de 10.", k));
            }
        }
    }
}
