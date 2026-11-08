namespace EasyCreator.Exercises;

/// <summary>js-14 (Map and Set): counting and comparing collections of words and numbers, and predicting insertion order, duplicates and key types.</summary>
[Exercises("js-14")]
public sealed class P18MapSet : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(lv, r) : MakeOut(lv, r);
    }

    static string Sent(Random r, int n, string[]? pool = null) => string.Join(" ", Enumerable.Range(0, n).Select(_ => Ex.Pick(r, pool ?? Pg.Words)));

    static Exercise MakeCode(int lv, Random r)
    {
        string[] pool = Pg.Words.OrderBy(_ => r.Next()).Take(6).ToArray();
        switch (lv)
        {
            case 0:
            {
                var tests = Pg.Tests(new[] { "a b a c b", "x", Sent(r, Ex.Int(r, 3, 10), pool), Sent(r, Ex.Int(r, 3, 10), pool), Sent(r, Ex.Int(r, 3, 10), pool) }, i => i.Trim().Split(' ').Distinct().Count().ToString());
                string sol = "const palavras = prompt().split(\" \");\nconst vistas = new Set(palavras);\nconsole.log(vistas.size);";
                return Ex.Code(Pg.Prompt(L.T("Leia uma linha de palavras separadas por espaço e mostre quantas palavras DIFERENTES ela tem. Use um `Set`."), tests[0]),
                    "const palavras = prompt().split(\" \");\n// escreva aqui", sol, tests, L.T("Um Set guarda cada valor uma única vez: ao criá-lo a partir da lista, os repetidos somem e `size` dá a quantidade de valores diferentes."));
            }
            case 1:
            {
                string Model(string s) { var g = s.Trim().Split(' ').GroupBy(w => w).OrderByDescending(x => x.Count()).ThenBy(x => x.Key, StringComparer.Ordinal).First(); return g.Key + " " + g.Count(); }
                var tests = Pg.Tests(new[] { "a b a c b a", "x", Sent(r, Ex.Int(r, 3, 10), pool), Sent(r, Ex.Int(r, 3, 10), pool), Sent(r, Ex.Int(r, 3, 10), pool) }, Model);
                string sol = "const cont = new Map();\nfor (const w of prompt().split(\" \")) {\n  cont.set(w, (cont.get(w) ?? 0) + 1);\n}\nlet melhor = null;\nfor (const [w, c] of cont) {\n  if (melhor === null || c > cont.get(melhor) || (c === cont.get(melhor) && w < melhor)) melhor = w;\n}\nconsole.log(melhor, cont.get(melhor));";
                return Ex.Code(Pg.Prompt(L.T("Leia uma linha de palavras separadas por espaço. Mostre a palavra que mais aparece e quantas vezes aparece, separados por um espaço. Em caso de empate, mostre a menor em ordem alfabética (comparação comum de strings). Use um `Map` para contar."), tests[0]),
                    "const palavras = prompt().split(\" \");\n// escreva aqui", sol, tests, L.T("O Map associa palavra → contagem: para cada palavra, soma 1 ao valor que já existe (ou a 0). Depois percorremos os pares guardando o melhor, e no empate vence a palavra alfabeticamente menor."));
            }
            case 2:
            {
                string Model(string inp)
                {
                    var ls = inp.TrimEnd('\n').Split('\n'); var a = new HashSet<string>(ls[0].Split(' ')); var b = new HashSet<string>(ls[1].Split(' '));
                    string J(IEnumerable<string> x) { var l = x.OrderBy(s => s, StringComparer.Ordinal).ToList(); return l.Count == 0 ? "-" : string.Join(" ", l); }
                    return $"Comum: {J(a.Intersect(b))}\nSó na primeira: {J(a.Except(b))}\nDistintas: {a.Union(b).Count()}";
                }
                var ins = new List<string> { "a b c\nb c d" };
                for (int i = 0; i < 4; i++) ins.Add(Sent(r, Ex.Int(r, 2, 6), pool) + "\n" + Sent(r, Ex.Int(r, 2, 6), pool));
                var tests = Pg.Tests(ins, Model);
                string sol = "const a = new Set(prompt().split(\" \"));\nconst b = new Set(prompt().split(\" \"));\nconst j = (s) => (s.size === 0 ? \"-\" : [...s].sort().join(\" \"));\nconsole.log(`Comum: ${j(new Set([...a].filter((x) => b.has(x))))}`);\nconsole.log(`Só na primeira: ${j(new Set([...a].filter((x) => !b.has(x))))}`);\nconsole.log(`Distintas: ${new Set([...a, ...b]).size}`);";
                return Ex.Code(Pg.Prompt(L.T("Leia duas linhas de palavras separadas por espaço. Mostre três linhas: `Comum: ` com as palavras que estão nas DUAS linhas, `Só na primeira: ` com as que estão só na primeira linha e `Distintas: ` com a quantidade de palavras diferentes no total. As listas saem em ordem alfabética e separadas por espaço; uma lista vazia aparece como `-`."), tests[0]),
                    "const a = new Set(prompt().split(\" \"));\nconst b = new Set(prompt().split(\" \"));\n// escreva aqui", sol, tests, L.T("Interseção: filtrar os elementos de a que b.has(). Diferença: os que b NÃO tem. União: espalhar os dois dentro de um novo Set e ler o size. `has` é rápido porque o Set não percorre a lista."));
            }
            default:
            {
                string Inp() { int n = Ex.Int(r, 4, 10); var v = Pg.Arr(r, n, 1, 14); return n + "\n" + string.Join(" ", v); }
                long Best(string inp) { var s = new HashSet<int>(inp.TrimEnd('\n').Split('\n')[1].Split(' ').Select(int.Parse)); int best = 0; foreach (var x in s) if (!s.Contains(x - 1)) { int len = 1; while (s.Contains(x + len)) len++; best = Math.Max(best, len); } return best; }
                var tests = Pg.Tests(new[] { "6\n100 4 200 1 3 2", Inp(), Inp(), Inp(), Inp() }, i => Best(i).ToString());
                string sol = "const n = Number(prompt());\nconst s = new Set(prompt().split(\" \").map(Number));\nlet melhor = 0;\nfor (const x of s) {\n  if (!s.has(x - 1)) {\n    let len = 1;\n    while (s.has(x + len)) len++;\n    melhor = Math.max(melhor, len);\n  }\n}\nconsole.log(melhor);";
                return Ex.Code(Pg.Prompt(L.T("Leia `n` e uma linha com `n` inteiros (podem se repetir). Mostre o tamanho da MAIOR sequência de números consecutivos que dá para formar com eles, em qualquer ordem (por exemplo, 100 4 200 1 3 2 tem 1, 2, 3, 4 → tamanho 4)."), tests[0]),
                    "const n = Number(prompt());\nconst v = prompt().split(\" \").map(Number);\n// escreva aqui", sol, tests, L.T("Truque: guarde tudo num Set. Só começamos a contar a partir de um número x cujo antecessor x − 1 NÃO está no Set (o começo de uma sequência); daí contamos x+1, x+2... enquanto existirem. Cada número é visitado poucas vezes: O(n), sem ordenar."));
            }
        }
    }

    static Exercise MakeOut(int lv, Random r)
    {
        if (lv == 0)
        {
            var vs = Pg.Arr(r, Ex.Int(r, 5, 8), 1, 6); int q = Ex.Int(r, 1, 8), rem = vs[0];
            var set = new List<int>(); foreach (var v in vs) if (!set.Contains(v)) set.Add(v);
            string code = $"const s = new Set([{string.Join(", ", vs)}]);\nconsole.log(s.size);\nconsole.log(s.has({q}));\ns.add({q});\ns.delete({rem});\nconsole.log([...s].join(\" \"));\nconsole.log(s.size);";
            var after = new List<int>(set); if (!after.Contains(q)) after.Add(q); after.Remove(rem);
            string outp = $"{set.Count}\n{(set.Contains(q) ? "true" : "false")}\n{string.Join(" ", after)}\n{after.Count}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("O Set ignora valores repetidos, mantém a ordem em que cada valor entrou pela PRIMEIRA vez, e add de um valor que já existe não muda nada. delete tira o valor onde ele estava; um valor adicionado depois fica no fim."));
        }
        if (lv <= 2)
        {
            var ws = Enumerable.Range(0, Ex.Int(r, 5, 8)).Select(_ => Ex.Pick(r, Pg.Words.OrderBy(__ => r.Next()).Take(3).ToArray())).ToArray();
            ws = ws.Select((w, i) => i % 2 == 0 ? Pg.Words[i % 3] : Pg.Words[(i + r.Next(3)) % 3]).ToArray();
            var m = new List<(string k, int c)>();
            foreach (var w in ws) { int ix = m.FindIndex(x => x.k == w); if (ix < 0) m.Add((w, 1)); else m[ix] = (w, m[ix].c + 1); }
            string drop = m[0].k;
            string code = $"const m = new Map();\nfor (const w of [{string.Join(", ", ws.Select(w => $"\"{w}\""))}]) {{\n  m.set(w, (m.get(w) ?? 0) + 1);\n}}\nconsole.log(m.size);\nconsole.log([...m.keys()].join(\" \"));\nconsole.log([...m.values()].join(\" \"));\nm.delete(\"{drop}\");\nconsole.log(m.size, m.get(\"{drop}\"));";
            string outp = $"{m.Count}\n{string.Join(" ", m.Select(x => x.k))}\n{string.Join(" ", m.Select(x => x.c))}\n{m.Count - 1} undefined";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("O Map também lembra a ordem de inserção das chaves: a ordem em que cada palavra apareceu pela primeira vez. Contar muda o valor mas não a posição da chave. Depois de delete, get devolve undefined."));
        }
        {
            int a = Ex.Int(r, 1, 9), b = Ex.Int(r, 10, 20);
            string code = $"const o = {{}};\no[{a}] = \"num\";\no[\"{a}\"] = \"txt\";\nconst m = new Map();\nm.set({a}, \"num\");\nm.set(\"{a}\", \"txt\");\nconst s = new Set([{a}, \"{a}\", {a}, {b}, {b}.0]);\nconsole.log(Object.keys(o).length, o[{a}]);\nconsole.log(m.size, m.get({a}));\nconsole.log(s.size);\nconsole.log(new Set([NaN, NaN, 0, -0]).size);\nconsole.log([...new Set(\"banana\")].join(\"\"));";
            string outp = $"1 txt\n2 num\n3\n2\nban";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("Num objeto comum as chaves viram TEXTO: o[1] e o[\"1\"] são a mesma chave (a segunda atribuição sobrescreve). No Map e no Set os tipos são respeitados: 1 e \"1\" são diferentes (e 20 e 20.0 são o mesmo número). Num Set, NaN conta como igual a NaN e 0 como igual a −0. Um Set de uma string guarda as letras sem repetir."));
        }
    }
}
