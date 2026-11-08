namespace EasyCreator.Exercises;

/// <summary>py-19 (weighted shortest paths, Dijkstra): distances in a weighted road map, the farthest city, a free road; and tracing Dijkstra by hand.</summary>
[Exercises("py-19")]
public sealed class P22Dijkstra : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(lv, r) : MakeOut(lv, r);
    }

    const int Inf = 1_000_000_000;

    static (int n, List<(int u, int v, int w)> e) Rand(Random r, int n, int m, int maxW = 9)
    {
        var e = new List<(int, int, int)>();
        for (int i = 2; i <= n; i++) e.Add((r.Next(1, i), i, r.Next(1, maxW + 1))); // spanning tree keeps most of it connected
        if (r.Next(5) == 0) e.RemoveAt(r.Next(e.Count));
        while (e.Count < m) { int u = r.Next(1, n + 1), v = r.Next(1, n + 1); if (u != v) e.Add((u, v, r.Next(1, maxW + 1))); }
        return (n, e);
    }
    static string Inp((int n, List<(int u, int v, int w)> e) g, string tail = "") => $"{g.n} {g.e.Count}\n" + string.Concat(g.e.Select(x => $"{x.u} {x.v} {x.w}\n")) + tail;
    static ((int n, List<(int u, int v, int w)> e) g, int[] tail) Parse(string i)
    {
        var ls = i.TrimEnd('\n').Split('\n'); var h = ls[0].Split(' ').Select(int.Parse).ToArray();
        var e = ls.Skip(1).Take(h[1]).Select(l => { var p = l.Split(' ').Select(int.Parse).ToArray(); return (p[0], p[1], p[2]); }).ToList();
        return ((h[0], e), ls.Length > h[1] + 1 ? ls[h[1] + 1].Split(' ').Select(int.Parse).ToArray() : Array.Empty<int>());
    }
    static int[] Dij((int n, List<(int u, int v, int w)> e) g, int s, List<int>? order = null, int[]? prev = null)
    {
        var dist = Enumerable.Repeat(Inf, g.n + 1).ToArray(); var done = new bool[g.n + 1]; dist[s] = 0;
        for (int it = 0; it < g.n; it++)
        {
            int u = -1; for (int v = 1; v <= g.n; v++) if (!done[v] && dist[v] < Inf && (u == -1 || dist[v] < dist[u])) u = v;
            if (u == -1) break; done[u] = true; order?.Add(u);
            foreach (var (a, b, w) in g.e) { if (a == u && dist[u] + w < dist[b]) { dist[b] = dist[u] + w; if (prev != null) prev[b] = u; } if (b == u && dist[u] + w < dist[a]) { dist[a] = dist[u] + w; if (prev != null) prev[a] = u; } }
        }
        return dist;
    }

    const string ReadG = "n, m = map(int, input().split())\nadj = [[] for _ in range(n + 1)]\nfor _ in range(m):\n    u, v, w = map(int, input().split())\n    adj[u].append((v, w))\n    adj[v].append((u, w))";
    const string DijPy = "INF = 10 ** 9\ndist = [INF] * (n + 1)\ndist[s] = 0\nfeito = [False] * (n + 1)\nfor _ in range(n):\n    u = -1\n    for v in range(1, n + 1):\n        if not feito[v] and dist[v] < INF and (u == -1 or dist[v] < dist[u]):\n            u = v\n    if u == -1:\n        break\n    feito[u] = True\n    for w, c in adj[u]:\n        if dist[u] + c < dist[w]:\n            dist[w] = dist[u] + c";

    static Exercise MakeCode(int lv, Random r)
    {
        string Gi(string tail) { int n = Ex.Int(r, 4, 7); var g = Rand(r, n, Ex.Int(r, n - 1, n + 3)); return Inp(g, tail); }
        switch (lv)
        {
            case 0:
            case 1:
            {
                bool one = lv == 0;
                string Tail() => one ? $"1 {Ex.Int(r, 2, 4)}" : "1";
                var ins = new[] { "5 3\n1 2 4\n2 3 1\n1 3 7\n" + (one ? "1 3" : "1"), Gi(Tail()), Gi(Tail()), Gi(Tail()), Gi(Tail()) };
                var tests = Pg.Tests(ins, i => { var (g, t) = Parse(i); var d = Dij(g, t[0]); if (one) return d[t[1]] >= Inf ? "Inalcançável" : d[t[1]].ToString(); return string.Join("\n", Enumerable.Range(1, g.n).Select(v => $"{v}: {(d[v] >= Inf ? "-" : d[v].ToString())}")); });
                string rdS = one ? "s, t = map(int, input().split())" : "s = int(input())";
                string tail = one ? "print(\"Inalcançável\" if dist[t] >= INF else dist[t])" : "for v in range(1, n + 1):\n    print(str(v) + \": \" + (\"-\" if dist[v] >= INF else str(dist[v])))";
                string sol = ReadG + "\n" + rdS + "\n" + DijPy + "\n" + tail;
                return Ex.Code(Pg.Prompt(L.F("Leia `N` e `M` (cidades de 1 a N e número de estradas), depois `M` linhas `u v w` (uma estrada de mão dupla de comprimento `w ≥ 1` entre `u` e `v`; pode haver estradas repetidas) e, por fim, {0}. Use o algoritmo de DIJKSTRA e mostre {1}.",
                        one ? L.T("uma linha com a cidade de origem `s` e a de destino `t`") : L.T("a cidade de origem `s`"),
                        one ? L.T("o menor comprimento total de um caminho de `s` até `t`, ou `Inalcançável` se não há caminho") : L.T("uma linha `v: distância` para cada cidade `v` de 1 a N (com `-` no lugar da distância se ela é inalcançável)")), tests[0]),
                    ReadG + "\n" + rdS + "\n" + Pg.Comment(false, L.T("escreva o algoritmo de Dijkstra")), sol, tests,
                    L.T("Dijkstra guarda a melhor distância conhecida de cada cidade. Repete: pega a cidade ainda não fechada com a menor distância, fecha-a (agora a distância dela é definitiva) e tenta melhorar as distâncias dos vizinhos passando por ela."));
            }
            case 2:
            {
                var ins = new[] { Gi(""), Gi(""), Gi(""), Gi("") };
                var tests = Pg.Tests(ins.Select(x => x + "1"), i => { var (g, _) = Parse(i); var d = Dij(g, 1); int best = 1; for (int v = 1; v <= g.n; v++) if (d[v] < Inf && d[v] > d[best]) best = v; return $"Mais distante: {best} ({d[best]})\nSoma: {Enumerable.Range(1, g.n).Where(v => d[v] < Inf).Sum(v => d[v])}"; });
                string sol = ReadG + "\ns = 1\n" + DijPy + "\nbest = 1\ntot = 0\nfor v in range(1, n + 1):\n    if dist[v] < INF:\n        tot += dist[v]\n        if dist[v] > dist[best]:\n            best = v\nprint(\"Mais distante: \" + str(best) + \" (\" + str(dist[best]) + \")\")\nprint(\"Soma: \" + str(tot))";
                return Ex.Code(Pg.Prompt(L.T("Leia `N`, `M`, `M` estradas `u v w` (mão dupla) e, na última linha, um número qualquer (a origem é SEMPRE a cidade 1). Com Dijkstra a partir da cidade 1, mostre `Mais distante: <cidade> (<distância>)` — a cidade alcançável mais distante, e em empate a de MENOR número — e `Soma: <soma das distâncias de todas as cidades alcançáveis>` (a cidade 1 conta 0)."), tests[0]),
                    ReadG + "\ns = int(input())\n" + Pg.Comment(false, L.T("escreva o resto do programa aqui")), sol, tests,
                    L.T("Depois de calcular as distâncias, percorremos as cidades de 1 a N guardando a maior distância finita; usar `>` (e não `>=`) mantém a de menor número em caso de empate. A soma ignora as cidades que não foram alcançadas."));
            }
            default:
            {
                int Free(string inp)
                {
                    var (g, _) = Parse(inp); int n = g.n; var d = new int[n + 1, 2]; for (int i = 0; i <= n; i++) { d[i, 0] = Inf; d[i, 1] = Inf; }
                    d[1, 0] = 0; var done = new bool[n + 1, 2];
                    while (true)
                    {
                        int bu = -1, bk = -1, bd = Inf; for (int v = 1; v <= n; v++) for (int k = 0; k < 2; k++) if (!done[v, k] && d[v, k] < bd) { bd = d[v, k]; bu = v; bk = k; }
                        if (bu == -1) break; done[bu, bk] = true;
                        foreach (var (a, b, w) in g.e) { foreach (var (x, y) in new[] { (a, b), (b, a) }) { if (x != bu) continue; if (d[bu, bk] + w < d[y, bk]) d[y, bk] = d[bu, bk] + w; if (bk == 0 && d[bu, 0] < d[y, 1]) d[y, 1] = d[bu, 0]; } }
                    }
                    return Math.Min(d[n, 0], d[n, 1]);
                }
                var tests = Pg.Tests(new[] { Gi(""), Gi(""), Gi(""), "4 3\n1 2 5\n2 3 5\n3 4 5\n", Gi("") }, i => Free(i) >= Inf ? "Inalcançável" : Free(i).ToString());
                string sol = ReadG + "\nINF = 10 ** 9\ndist = [[INF, INF] for _ in range(n + 1)]\ndist[1][0] = 0\nfeito = [[False, False] for _ in range(n + 1)]\nwhile True:\n    bu, bk, bd = -1, -1, INF\n    for v in range(1, n + 1):\n        for k in range(2):\n            if not feito[v][k] and dist[v][k] < bd:\n                bd, bu, bk = dist[v][k], v, k\n    if bu == -1:\n        break\n    feito[bu][bk] = True\n    for y, c in adj[bu]:\n        if dist[bu][bk] + c < dist[y][bk]:\n            dist[y][bk] = dist[bu][bk] + c\n        if bk == 0 and dist[bu][0] < dist[y][1]:\n            dist[y][1] = dist[bu][0]\nres = min(dist[n])\nprint(\"Inalcançável\" if res >= INF else res)";
                return Ex.Code(Pg.Prompt(L.T("Leia `N`, `M` e `M` estradas `u v w` (mão dupla). Você viaja da cidade 1 até a cidade N e tem um VALE-PEDÁGIO: pode passar de graça (custo 0) por NO MÁXIMO UMA estrada da viagem. Mostre o menor custo total possível, ou `Inalcançável` se N não pode ser alcançada."), tests[0]),
                    ReadG + "\n" + Pg.Comment(false, L.T("escreva o resto do programa aqui")), sol, tests,
                    L.T("Truque: cada cidade passa a ter DOIS estados — (cidade, ainda tenho o vale) e (cidade, já usei o vale). Dijkstra roda sobre esses estados: andar normalmente mantém o estado e soma w; usar o vale passa do estado 0 para o 1 sem somar nada. A resposta é o menor entre os dois estados de N."));
            }
        }
    }

    static Exercise MakeOut(int lv, Random r)
    {
        int n = Ex.Int(r, 4, 5); var g = Rand(r, n, Ex.Int(r, n, n + 2), 6);
        string edges = string.Join(", ", g.e.Select(e => $"({e.u}, {e.v}, {e.w})"));
        int s = Ex.Int(r, 1, n);
        string code = $"n = {n}\nestradas = [{edges}]\nINF = 99\ndist = [INF] * (n + 1)\ndist[{s}] = 0\nfeito = [False] * (n + 1)\nordem = []\nfor _ in range(n):\n    u = -1\n    for v in range(1, n + 1):\n        if not feito[v] and dist[v] < INF and (u == -1 or dist[v] < dist[u]):\n            u = v\n    if u == -1:\n        break\n    feito[u] = True\n    ordem.append(u)\n    for a, b, c in estradas:\n        if a == u and dist[u] + c < dist[b]:\n            dist[b] = dist[u] + c\n        if b == u and dist[u] + c < dist[a]:\n            dist[a] = dist[u] + c\n";
        var order = new List<int>(); var d = Dij(g, s, order);
        string Shown(int v) => d[v] >= Inf ? "99" : d[v].ToString();
        if (lv == 0)
        {
            code += "print(\" \".join(str(x) for x in dist[1:]))";
            string outp = string.Join(" ", Enumerable.Range(1, n).Select(Shown));
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Dijkstra a partir de {0}. O valor 99 (INF) marca as cidades que nenhuma estrada alcança. Fechando as cidades na ordem {1}, as distâncias finais são as da resposta (cidade 1 primeiro).", s, string.Join(", ", order)));
        }
        if (lv == 1)
        {
            code += "print(\" \".join(str(x) for x in ordem))\nprint(len(ordem))";
            string outp = $"{string.Join(" ", order)}\n{order.Count}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("A ordem é a em que as cidades são FECHADAS: sempre a de menor distância provisória que ainda está aberta (em empate, a de menor número). Cidades inalcançáveis nunca são fechadas."));
        }
        {
            int t = Ex.Int(r, 1, n);
            code += $"print(dist[{t}])\nprint(sum(x for x in dist[1:] if x < INF))\nprint(max(x for x in dist[1:] if x < INF))";
            var fin = Enumerable.Range(1, n).Where(v => d[v] < Inf).Select(v => d[v]).ToArray();
            string outp = $"{Shown(t)}\n{fin.Sum()}\n{fin.Max()}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Distâncias finais a partir de {0}: [{1}] (99 = inalcançável). Pedem a distância até {2}, a soma das finitas e a maior finita.", s, string.Join(", ", Enumerable.Range(1, n).Select(Shown)), t));
        }
    }
}
