namespace EasyCreator.Exercises;

/// <summary>Lesson 10 (sorting, binary search, complexity): ordering and searching lists, counting inversions, tracing a bubble-sort pass and a binary search.</summary>
[Exercises("py-10", "js-10")]
public sealed class P10Sorting : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        bool js = Pg.Js(lesson);
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(js, lv, r) : MakeOut(js, lv, r);
    }

    static string Inp(int[] v) => v.Length + "\n" + string.Join(" ", v);
    static int[] Parse(string i) => i.TrimEnd('\n').Split('\n')[1].Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();
    static List<string> Lists(Random r, int lo, int hi, bool sorted = false, int min = 3, int max = 9)
    {
        var l = new List<string>();
        for (int i = 0; i < 5; i++) { var v = Pg.Arr(r, Ex.Int(r, min, max), lo, hi); if (sorted) Array.Sort(v); l.Add(Inp(v)); }
        return l;
    }

    static Exercise MakeCode(bool js, int lv, Random r)
    {
        string rd = Pg.Code(Pg.ReadInt(js, "n"), Pg.ReadInts(js, "v"));
        string starter = rd + "\n" + Pg.Comment(js, L.T("escreva o resto do programa aqui"));
        string asc = js ? "v.sort((a, b) => a - b);" : "v.sort()";
        string join = js ? "v.join(\" \")" : "\" \".join(str(x) for x in v)";
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        switch (lv)
        {
            case 0:
            {
                int mode = r.Next(3), k = Ex.Int(r, 2, 3);
                var tests = Pg.Tests(Lists(r, -20, 60, false, 4, 9), i => { var v = Parse(i).OrderBy(x => x).ToArray(); return mode == 0 ? string.Join(" ", v) : mode == 1 ? string.Join(" ", v.Reverse()) : string.Join(" ", v.Take(k)); });
                string body = mode switch
                {
                    0 => asc + "\n" + pr(join),
                    1 => asc + "\n" + (js ? "v.reverse();" : "v.reverse()") + "\n" + pr(join),
                    _ => asc + "\n" + pr(js ? $"v.slice(0, {k}).join(\" \")" : $"\" \".join(str(x) for x in v[:{k}])"),
                };
                string what = mode == 0 ? L.T("os números em ordem crescente") : mode == 1 ? L.T("os números em ordem decrescente") : L.F("os {0} menores números, em ordem crescente", k);
                return Ex.Code(Pg.Prompt(L.F("Leia `n` e, na linha seguinte, `n` inteiros separados por espaço. Mostre, numa linha separada por espaços, {0}.", what), tests[0]), starter, rd + "\n" + body, tests,
                    L.T("Ordenamos a lista com a ferramenta pronta da linguagem (em JavaScript é preciso passar a comparação (a, b) => a - b, senão ordena como texto) e mostramos o que foi pedido."));
            }
            case 1:
            {
                if (r.Next(2) == 0)
                {
                    var ins = new List<string>();
                    for (int i = 0; i < 5; i++) { var v = Pg.Arr(r, Ex.Int(r, 3, 9), 1, 40).Distinct().OrderBy(x => x).ToArray(); int q = r.Next(3) == 0 ? Ex.Int(r, 1, 40) : v[r.Next(v.Length)]; ins.Add(Inp(v) + "\n" + q); }
                    var tests = Pg.Tests(ins, i => { var p = i.TrimEnd('\n').Split('\n'); var v = Parse(i); return Array.IndexOf(v, int.Parse(p[2])).ToString(); });
                    string rd2 = rd + "\n" + Pg.ReadInt(js, "q");
                    string sol = js ? rd2 + "\nlet lo = 0, hi = n - 1, res = -1;\nwhile (lo <= hi) {\n  const mid = Math.trunc((lo + hi) / 2);\n  if (v[mid] === q) {\n    res = mid;\n    break;\n  } else if (v[mid] < q) {\n    lo = mid + 1;\n  } else {\n    hi = mid - 1;\n  }\n}\nconsole.log(res);"
                        : rd2 + "\nlo, hi, res = 0, n - 1, -1\nwhile lo <= hi:\n    mid = (lo + hi) // 2\n    if v[mid] == q:\n        res = mid\n        break\n    elif v[mid] < q:\n        lo = mid + 1\n    else:\n        hi = mid - 1\nprint(res)";
                    return Ex.Code(Pg.Prompt(L.T("Leia `n`, uma linha com `n` inteiros DISTINTOS já em ordem crescente e, depois, um valor `q`. Com BUSCA BINÁRIA (olhar o meio e descartar metade a cada passo), mostre a posição de `q` na lista (a primeira posição é 0) ou -1 se ele não está."), tests[0]),
                        rd2 + "\n" + Pg.Comment(js, L.T("escreva a busca binária")), sol, tests,
                        L.T("Mantemos o intervalo [lo, hi] onde q pode estar. Olhamos o meio: se é q, achamos; se é menor que q, descartamos a metade da esquerda; se é maior, a da direita. Como o intervalo cai pela metade a cada passo, são ~log₂ n passos."));
                }
                else
                {
                    var tests = Pg.Tests(Lists(r, 1, 30, false, 4, 10), i => string.Join(" ", Parse(i).Distinct().OrderBy(x => x)));
                    string sol = rd + "\n" + (js ? "const u = [...new Set(v)].sort((a, b) => a - b);\nconsole.log(u.join(\" \"));" : "print(\" \".join(str(x) for x in sorted(set(v))))");
                    return Ex.Code(Pg.Prompt(L.T("Leia `n` e uma linha com `n` inteiros (podem se repetir). Mostre, em ordem crescente e sem repetições, os valores que aparecem, separados por espaço."), tests[0]), starter, sol, tests,
                        L.T("Um conjunto (set) descarta os repetidos; depois é só ordenar e mostrar."));
                }
            }
            case 2:
            {
                if (r.Next(2) == 0)
                {
                    var tests = Pg.Tests(Lists(r, -15, 15, false, 4, 9), i => string.Join(" ", Parse(i).OrderBy(x => Math.Abs(x)).ThenBy(x => x)));
                    string sol = rd + "\n" + (js ? "v.sort((a, b) => Math.abs(a) - Math.abs(b) || a - b);\nconsole.log(v.join(\" \"));" : "v.sort(key=lambda x: (abs(x), x))\nprint(\" \".join(str(x) for x in v))");
                    return Ex.Code(Pg.Prompt(L.T("Leia `n` e uma linha com `n` inteiros. Mostre-os ordenados pelo VALOR ABSOLUTO (sem o sinal), do menor para o maior; se dois têm o mesmo valor absoluto, o menor (com sinal) vem primeiro."), tests[0]), starter, sol, tests,
                        L.T("A ordenação usa uma chave: primeiro o valor absoluto e, em caso de empate, o próprio número. Em Python a chave é uma tupla (abs(x), x); em JavaScript a comparação devolve a diferença dos absolutos, e se for 0 usa a diferença dos números."));
                }
                else
                {
                    var ins = new List<string>();
                    for (int i = 0; i < 5; i++) { var v = Pg.Arr(r, Ex.Int(r, 5, 12), 1, 6); Array.Sort(v); ins.Add(Inp(v) + "\n" + Ex.Int(r, 1, 7)); }
                    var tests = Pg.Tests(ins, i => { var p = i.TrimEnd('\n').Split('\n'); return Parse(i).Count(x => x == int.Parse(p[2])).ToString(); });
                    string sol = rd + "\n" + Pg.ReadInt(js, "q") + "\n" + (js ? "console.log(v.filter((x) => x === q).length);" : "print(v.count(q))");
                    return Ex.Code(Pg.Prompt(L.T("Leia `n`, uma linha com `n` inteiros em ordem crescente (com repetições) e um valor `q`. Mostre quantas vezes `q` aparece na lista."), tests[0]),
                        rd + "\n" + Pg.ReadInt(js, "q") + "\n" + Pg.Comment(js, L.T("escreva o resto do programa aqui")), sol, tests,
                        L.T("Para listas pequenas basta contar. Numa lista ordenada muito grande daria para usar duas buscas binárias (primeira e última posição de q) e subtrair, em O(log n)."));
                }
            }
            default:
            {
                int kind = r.Next(3);
                if (kind == 0)
                {
                    var tests = Pg.Tests(Lists(r, 1, 30, false, 3, 9), i => { var v = Parse(i); int c = 0; for (int a = 0; a < v.Length; a++) for (int b = a + 1; b < v.Length; b++) if (v[a] > v[b]) c++; return c.ToString(); });
                    string sol = rd + "\n" + (js ? "let c = 0;\nfor (let i = 0; i < n; i++) {\n  for (let j = i + 1; j < n; j++) {\n    if (v[i] > v[j]) c++;\n  }\n}\nconsole.log(c);" : "c = 0\nfor i in range(n):\n    for j in range(i + 1, n):\n        if v[i] > v[j]:\n            c += 1\nprint(c)");
                    return Ex.Code(Pg.Prompt(L.T("Uma INVERSÃO é um par de posições i < j com `v[i] > v[j]` (um número maior antes de um menor). Leia `n` e uma linha com `n` inteiros e mostre quantas inversões a lista tem. (Uma lista ordenada tem 0.)"), tests[0]), starter, sol, tests,
                        L.T("Comparamos cada posição i com todas as posições j depois dela e contamos os pares fora de ordem. Dois laços aninhados: cerca de n²/2 comparações."));
                }
                if (kind == 1)
                {
                    var ins = new List<string>();
                    for (int i = 0; i < 6; i++) { var v = Pg.Arr(r, Ex.Int(r, 4, 10), 1, 30); ins.Add(Inp(v) + "\n" + Ex.Int(r, 5, 55)); }
                    var tests = Pg.Tests(ins, i => { var v = Parse(i); int s = int.Parse(i.TrimEnd('\n').Split('\n')[2]); for (int a = 0; a < v.Length; a++) for (int b = a + 1; b < v.Length; b++) if (v[a] + v[b] == s) return "sim"; return "não"; });
                    string sol = rd + "\n" + Pg.ReadInt(js, "s") + "\n" + (js ? "v.sort((a, b) => a - b);\nlet i = 0, j = n - 1, ok = false;\nwhile (i < j) {\n  const t = v[i] + v[j];\n  if (t === s) {\n    ok = true;\n    break;\n  }\n  if (t < s) i++;\n  else j--;\n}\nconsole.log(ok ? \"sim\" : \"não\");"
                        : "v.sort()\ni, j, ok = 0, n - 1, False\nwhile i < j:\n    t = v[i] + v[j]\n    if t == s:\n        ok = True\n        break\n    if t < s:\n        i += 1\n    else:\n        j -= 1\nprint(\"sim\" if ok else \"não\")");
                    return Ex.Code(Pg.Prompt(L.T("Leia `n`, uma linha com `n` inteiros e um valor `s`. Mostre `sim` se existem DUAS posições diferentes cujos valores somam exatamente `s`, ou `não` se não existem."), tests[0]),
                        rd + "\n" + Pg.ReadInt(js, "s") + "\n" + Pg.Comment(js, L.T("escreva o resto do programa aqui")), sol, tests,
                        L.T("Truque: ordenando a lista, usamos dois ponteiros, um em cada ponta. Se a soma é menor que s, só avançar o da esquerda pode aumentá-la; se é maior, recuamos o da direita. Cada passo descarta um elemento: O(n) depois da ordenação."));
                }
                {
                    var tests = Pg.Tests(Lists(r, 1, 90, false, 3, 9), i => { var v = Parse(i).OrderBy(x => x).ToArray(); return Enumerable.Range(1, v.Length - 1).Min(k => v[k] - v[k - 1]).ToString(); });
                    string sol = rd + "\n" + (js ? "v.sort((a, b) => a - b);\nlet m = Infinity;\nfor (let i = 1; i < n; i++) m = Math.min(m, v[i] - v[i - 1]);\nconsole.log(m);" : "v.sort()\nm = min(v[i] - v[i - 1] for i in range(1, n))\nprint(m)");
                    return Ex.Code(Pg.Prompt(L.T("Leia `n` (n ≥ 2) e uma linha com `n` inteiros. Mostre a MENOR diferença (sem sinal) entre quaisquer dois números da lista, em posições diferentes."), tests[0]), starter, sol, tests,
                        L.T("Em vez de testar todos os pares, ordenamos: a menor diferença sempre acontece entre vizinhos da lista ordenada. Basta olhar as n − 1 diferenças consecutivas."));
                }
            }
        }
    }

    static Exercise MakeOut(bool js, int lv, Random r)
    {
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        var v = Pg.Arr(r, Ex.Int(r, 5, 7), 1, 9).ToList();
        string lst = string.Join(", ", v);
        string jn(string l) => js ? $"{l}.join(\" \")" : $"\" \".join(str(x) for x in {l})";
        if (lv <= 1)
        {
            int passes = lv == 0 ? 1 : 2;
            var w = new List<int>(v); var outs = new List<string>();
            for (int p = 0; p < passes; p++) { for (int i = 0; i + 1 < w.Count; i++) if (w[i] > w[i + 1]) (w[i], w[i + 1]) = (w[i + 1], w[i]); outs.Add(string.Join(" ", w)); }
            string code = js ? $"const v = [{lst}];\nfor (let p = 0; p < {passes}; p++) {{\n  for (let i = 0; i + 1 < v.length; i++) {{\n    if (v[i] > v[i + 1]) {{\n      const t = v[i];\n      v[i] = v[i + 1];\n      v[i + 1] = t;\n    }}\n  }}\n  console.log(v.join(\" \"));\n}}"
                : $"v = [{lst}]\nfor p in range({passes}):\n    for i in range(len(v) - 1):\n        if v[i] > v[i + 1]:\n            v[i], v[i + 1] = v[i + 1], v[i]\n    print(\" \".join(str(x) for x in v))";
            string outp = string.Join("\n", outs);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("Em cada passada do bubble sort comparamos vizinhos da esquerda para a direita e trocamos os que estão fora de ordem; o maior valor \"flutua\" até o final. A linha mostrada é a lista depois de cada passada."));
        }
        if (lv == 2)
        {
            var s = v.Distinct().OrderBy(x => x).ToList(); int target = r.Next(3) == 0 ? Ex.Int(r, 1, 9) : s[r.Next(s.Count)];
            int lo = 0, hi = s.Count - 1, steps = 0, res = -1;
            while (lo <= hi) { int mid = (lo + hi) / 2; steps++; if (s[mid] == target) { res = mid; break; } if (s[mid] < target) lo = mid + 1; else hi = mid - 1; }
            string sl = string.Join(", ", s);
            string code = js ? $"const v = [{sl}];\nlet lo = 0, hi = v.length - 1, passos = 0, res = -1;\nwhile (lo <= hi) {{\n  const mid = Math.trunc((lo + hi) / 2);\n  passos++;\n  if (v[mid] === {target}) {{\n    res = mid;\n    break;\n  }}\n  if (v[mid] < {target}) lo = mid + 1;\n  else hi = mid - 1;\n}}\nconsole.log(res, passos);"
                : $"v = [{sl}]\nlo, hi, passos, res = 0, len(v) - 1, 0, -1\nwhile lo <= hi:\n    mid = (lo + hi) // 2\n    passos += 1\n    if v[mid] == {target}:\n        res = mid\n        break\n    if v[mid] < {target}:\n        lo = mid + 1\n    else:\n        hi = mid - 1\nprint(res, passos)";
            string outp = $"{res} {steps}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Procurando {0} na lista ordenada [{1}]: a cada passo olha-se o meio do intervalo. Foram {2} passos e a posição achada é {3} (-1 = não achou).", target, sl, steps, res));
        }
        {
            var w = new List<int>(v); int swaps = 0, cmps = 0;
            for (int i = 0; i < w.Count - 1; i++) { int m = i; for (int j = i + 1; j < w.Count; j++) { cmps++; if (w[j] < w[m]) m = j; } if (m != i) { (w[i], w[m]) = (w[m], w[i]); swaps++; } }
            string code = js ? $"const v = [{lst}];\nlet trocas = 0, comparacoes = 0;\nfor (let i = 0; i < v.length - 1; i++) {{\n  let m = i;\n  for (let j = i + 1; j < v.length; j++) {{\n    comparacoes++;\n    if (v[j] < v[m]) m = j;\n  }}\n  if (m !== i) {{\n    const t = v[i];\n    v[i] = v[m];\n    v[m] = t;\n    trocas++;\n  }}\n}}\nconsole.log(trocas, comparacoes);\nconsole.log(v.join(\" \"));"
                : $"v = [{lst}]\ntrocas = 0\ncomparacoes = 0\nfor i in range(len(v) - 1):\n    m = i\n    for j in range(i + 1, len(v)):\n        comparacoes += 1\n        if v[j] < v[m]:\n            m = j\n    if m != i:\n        v[i], v[m] = v[m], v[i]\n        trocas += 1\nprint(trocas, comparacoes)\nprint(\" \".join(str(x) for x in v))";
            string outp = $"{swaps} {cmps}\n{string.Join(" ", w)}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Na ordenação por seleção, a cada posição i procura-se o menor do resto e troca-se (só se ele não já está lá). As comparações são sempre n(n−1)/2 = {0}, qualquer que seja a lista; as trocas dependem da ordem inicial.", cmps));
        }
    }
}
