namespace EasyCreator.Exercises;

/// <summary>js-19 (dynamic programming: the coin problem): tables for stairs, minimum coins, number of ways and the 0/1 knapsack; and filling a table by hand.</summary>
[Exercises("js-19")]
public sealed class P23Coins : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(lv, r) : MakeOut(lv, r);
    }

    static int[] MinCoins(int[] coins, int max) { var d = Enumerable.Repeat(-1, max + 1).ToArray(); d[0] = 0; for (int v = 1; v <= max; v++) foreach (var c in coins) if (c <= v && d[v - c] >= 0 && (d[v] < 0 || d[v - c] + 1 < d[v])) d[v] = d[v - c] + 1; return d; }
    static long[] Ways(int[] coins, int max) { var w = new long[max + 1]; w[0] = 1; foreach (var c in coins) for (int v = c; v <= max; v++) w[v] += w[v - c]; return w; }
    static int[] Coins(Random r) { var set = new List<int> { 1 }; var pool = new[] { 2, 3, 4, 5, 6, 7, 10 }; foreach (var c in pool.OrderBy(_ => r.Next()).Take(Ex.Int(r, 2, 3))) set.Add(c); if (r.Next(3) == 0) set.Remove(1); if (set.Count < 2) set.Add(5); return set.OrderBy(x => r.Next()).ToArray(); }
    static string CoinInput(Random r) { var c = Coins(r); int q = Ex.Int(r, 2, 4); return string.Join(" ", c) + "\n" + q + "\n" + string.Join("\n", Enumerable.Range(0, q).Select(_ => Ex.Int(r, 0, 30))); }
    static (int[] coins, int[] qs) ParseCoins(string i) { var ls = i.TrimEnd('\n').Split('\n'); return (ls[0].Split(' ').Select(int.Parse).ToArray(), ls.Skip(2).Select(int.Parse).ToArray()); }
    const string Rd = "const moedas = prompt().split(\" \").map(Number);\nconst q = Number(prompt());\nconst valores = [];\nfor (let i = 0; i < q; i++) valores.push(Number(prompt()));\nconst maximo = Math.max(...valores);";

    static Exercise MakeCode(int lv, Random r)
    {
        string starter = Rd + "\n// escreva o resto do programa aqui";
        switch (lv)
        {
            case 0:
            {
                int steps = r.Next(2) == 0 ? 2 : 3;
                long W(int n) { var t = new long[n + 1]; t[0] = 1; for (int i = 1; i <= n; i++) for (int s = 1; s <= steps; s++) if (i >= s) t[i] += t[i - s]; return t[n]; }
                var tests = Pg.Tests(new[] { "1", "10", Ex.Int(r, 2, 30).ToString(), Ex.Int(r, 2, 30).ToString(), Ex.Int(r, 2, 30).ToString() }, i => W(int.Parse(i.Trim())).ToString());
                string loop = steps == 2 ? "tab[i] = tab[i - 1] + (i >= 2 ? tab[i - 2] : 0);" : "tab[i] = tab[i - 1] + (i >= 2 ? tab[i - 2] : 0) + (i >= 3 ? tab[i - 3] : 0);";
                string sol = $"const n = Number(prompt());\nconst tab = new Array(n + 1).fill(0);\ntab[0] = 1;\nfor (let i = 1; i <= n; i++) {{\n  {loop}\n}}\nconsole.log(tab[n]);";
                return Ex.Code(Pg.Prompt(steps == 2 ? L.T("Uma escada tem `n` degraus e você sobe de 1 em 1 ou de 2 em 2 degraus. Leia `n` e mostre de quantas maneiras diferentes (a ordem dos passos conta) se chega ao topo. Use uma TABELA preenchida do degrau 1 até `n`, sem recursão.") : L.T("Uma escada tem `n` degraus e você sobe de 1 em 1, de 2 em 2 ou de 3 em 3 degraus. Leia `n` e mostre de quantas maneiras diferentes (a ordem dos passos conta) se chega ao topo. Use uma TABELA preenchida do degrau 1 até `n`, sem recursão."), tests[0]),
                    "const n = Number(prompt());\n// escreva a tabela e mostre tab[n]", sol, tests,
                    L.T("tab[i] é o número de maneiras de chegar ao degrau i. O último passo veio de um degrau abaixo, dois abaixo (ou três): somamos as maneiras de chegar a cada um. Começamos de tab[0] = 1 (uma maneira de não subir)."));
            }
            case 1:
            {
                var tests = Pg.Tests(new[] { "1 3 4\n3\n6\n0\n7", CoinInput(r), CoinInput(r), CoinInput(r), CoinInput(r) }, i => { var (c, qs) = ParseCoins(i); var d = MinCoins(c, qs.Max()); return string.Join("\n", qs.Select(v => d[v] < 0 ? $"{v}: impossível" : $"{v}: {d[v]}")); });
                string sol = Rd + "\nconst min = new Array(maximo + 1).fill(Infinity);\nmin[0] = 0;\nfor (let v = 1; v <= maximo; v++) {\n  for (const c of moedas) {\n    if (c <= v && min[v - c] + 1 < min[v]) min[v] = min[v - c] + 1;\n  }\n}\nfor (const v of valores) console.log(min[v] === Infinity ? `${v}: impossível` : `${v}: ${min[v]}`);";
                return Ex.Code(Pg.Prompt(L.T("Leia uma linha com os valores das moedas (inteiros positivos, diferentes, quantidade ilimitada de cada), depois `q` e `q` valores de troco (≥ 0), um por linha. Calcule UMA tabela de programação dinâmica até o maior valor pedido e, para cada troco `v`, mostre `v: <menor número de moedas>` ou `v: impossível`."), tests[0]), starter, sol, tests,
                    L.T("min[v] é o menor número de moedas que formam v. Para cada valor, tentamos cada moeda c: se v − c é possível, uma opção é min[v − c] + 1. Preenchemos a tabela do menor valor para o maior, uma só vez, e todos os pedidos saem dela."));
            }
            case 2:
            {
                var tests = Pg.Tests(new[] { "1 2 5\n3\n5\n0\n3", CoinInput(r), CoinInput(r), CoinInput(r), CoinInput(r) }, i => { var (c, qs) = ParseCoins(i); var w = Ways(c, qs.Max()); return string.Join("\n", qs.Select(v => $"{v}: {w[v]}")); });
                string sol = Rd + "\nconst formas = new Array(maximo + 1).fill(0);\nformas[0] = 1;\nfor (const c of moedas) {\n  for (let v = c; v <= maximo; v++) formas[v] += formas[v - c];\n}\nfor (const v of valores) console.log(`${v}: ${formas[v]}`);";
                return Ex.Code(Pg.Prompt(L.T("Leia uma linha com os valores das moedas (positivos, diferentes, quantidade ilimitada de cada), depois `q` e `q` valores (≥ 0), um por linha. Para cada valor `v`, mostre `v: <de quantas formas diferentes dá para formar v>`, onde formas com as mesmas moedas em outra ORDEM contam como uma só (para v = 0 há 1 forma: não usar moeda nenhuma)."), tests[0]), starter, sol, tests,
                    L.T("Truque para não contar a mesma combinação em ordens diferentes: o laço de FORA percorre as moedas e o de dentro os valores. Assim, ao considerar a moeda c, só somamos formas que a usam depois das moedas anteriores, numa ordem fixa."));
            }
            default:
            {
                int Knap(string inp) { var ls = inp.TrimEnd('\n').Split('\n'); var h = ls[0].Split(' ').Select(int.Parse).ToArray(); var best = new int[h[1] + 1]; foreach (var l in ls.Skip(1)) { var p = l.Split(' ').Select(int.Parse).ToArray(); for (int c = h[1]; c >= p[0]; c--) best[c] = Math.Max(best[c], best[c - p[0]] + p[1]); } return best[h[1]]; }
                string Gi() { int n = Ex.Int(r, 3, 6), cap = Ex.Int(r, 5, 15); return $"{n} {cap}\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => $"{Ex.Int(r, 1, 8)} {Ex.Int(r, 1, 20)}")); }
                var tests = Pg.Tests(new[] { "3 5\n2 3\n3 4\n4 5", Gi(), Gi(), Gi(), Gi() }, i => Knap(i).ToString());
                string sol = "const [n, cap] = prompt().split(\" \").map(Number);\nconst melhor = new Array(cap + 1).fill(0);\nfor (let i = 0; i < n; i++) {\n  const [peso, valor] = prompt().split(\" \").map(Number);\n  for (let c = cap; c >= peso; c--) {\n    melhor[c] = Math.max(melhor[c], melhor[c - peso] + valor);\n  }\n}\nconsole.log(melhor[cap]);";
                return Ex.Code(Pg.Prompt(L.T("Problema da mochila. Leia `n` e a capacidade `C` (na mesma linha) e depois `n` linhas `<peso> <valor>`. Cada item só pode ser levado UMA vez. Mostre o maior valor total que cabe numa mochila de capacidade `C`."), tests[0]),
                    "const [n, cap] = prompt().split(\" \").map(Number);\n// escreva o resto do programa aqui", sol, tests,
                    L.T("melhor[c] é o maior valor com capacidade c. Para cada item, atualizamos a tabela de trás para frente (c de cap até o peso): assim melhor[c − peso] ainda NÃO inclui o item atual e ele não é contado duas vezes. Se fôssemos da frente para trás, seria a mochila com itens ilimitados."));
            }
        }
    }

    static Exercise MakeOut(int lv, Random r)
    {
        var coins = Coins(r); int max = Ex.Int(r, 6, 10); string cs = string.Join(", ", coins);
        if (lv <= 1)
        {
            var d = MinCoins(coins, max);
            string code = $"const moedas = [{cs}];\nconst min = new Array({max + 1}).fill(Infinity);\nmin[0] = 0;\nfor (let v = 1; v <= {max}; v++) {{\n  for (const c of moedas) {{\n    if (c <= v && min[v - c] + 1 < min[v]) min[v] = min[v - c] + 1;\n  }}\n}}\nconsole.log(min.map((x) => (x === Infinity ? \"-\" : x)).join(\" \"));";
            string outp = string.Join(" ", d.Select(x => x < 0 ? "-" : x.ToString()));
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Moedas [{0}]. Preencha de 0 a {1}: min[0] = 0 e, para cada v, o menor de min[v − c] + 1 entre as moedas c que cabem. Um \"-\" marca valores impossíveis (Infinity).", cs, max));
        }
        if (lv == 2)
        {
            var w = Ways(coins, max);
            string code = $"const moedas = [{cs}];\nconst formas = new Array({max + 1}).fill(0);\nformas[0] = 1;\nfor (const c of moedas) {{\n  for (let v = c; v <= {max}; v++) formas[v] += formas[v - c];\n}}\nconsole.log(formas.join(\" \"));";
            string outp = string.Join(" ", w);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Moedas [{0}]. Processe uma moeda por vez, do valor da moeda até {1}: formas[v] ganha formas[v − c]. Depois da última moeda, formas[v] é o número de combinações que somam v.", cs, max));
        }
        {
            int n = Ex.Int(r, 3, 4), cap = Ex.Int(r, 5, 8); var items = Enumerable.Range(0, n).Select(_ => (p: Ex.Int(r, 1, 5), v: Ex.Int(r, 1, 9))).ToArray();
            var best = new int[cap + 1]; var rows = new List<string>();
            foreach (var (p, v) in items) { for (int c = cap; c >= p; c--) best[c] = Math.Max(best[c], best[c - p] + v); rows.Add(string.Join(" ", best)); }
            string code = $"const itens = [{string.Join(", ", items.Select(i => $"[{i.p}, {i.v}]"))}];\nconst melhor = new Array({cap + 1}).fill(0);\nfor (const [p, v] of itens) {{\n  for (let c = {cap}; c >= p; c--) {{\n    melhor[c] = Math.max(melhor[c], melhor[c - p] + v);\n  }}\n  console.log(melhor.join(\" \"));\n}}";
            string outp = string.Join("\n", rows);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("Uma linha por item: a tabela melhor[0..C] depois de considerar aquele item. Cada capacidade c vira o maior entre manter o valor antigo e usar o item (peso p, valor v) sobre melhor[c − p], que ainda é o valor ANTES do item atual (por isso o laço vai de trás para frente)."));
        }
    }
}
