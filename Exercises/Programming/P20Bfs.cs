namespace EasyCreator.Exercises;

/// <summary>py-18 (graphs and breadth-first search): visit order, distances, connected components, grid mazes and multi-source search; and tracing a BFS by hand.</summary>
[Exercises("py-18")]
public sealed class P20Bfs : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(lv, r) : MakeOut(lv, r);
    }

    static (int n, List<(int, int)> edges, List<int>[] adj) RandGraph(Random r, int n, int m)
    {
        var set = new HashSet<(int, int)>(); var edges = new List<(int, int)>();
        for (int tries = 0; edges.Count < m && tries < 100; tries++) { int u = r.Next(1, n + 1), v = r.Next(1, n + 1); if (u == v) continue; var k = (Math.Min(u, v), Math.Max(u, v)); if (set.Add(k)) edges.Add((u, v)); }
        var adj = Enumerable.Range(0, n + 1).Select(_ => new List<int>()).ToArray();
        foreach (var (u, v) in edges) { adj[u].Add(v); adj[v].Add(u); }
        foreach (var l in adj) l.Sort();
        return (n, edges, adj);
    }
    static string GraphInput(int n, List<(int, int)> e) => $"{n} {e.Count}\n" + string.Concat(e.Select(x => $"{x.Item1} {x.Item2}\n"));
    static List<int> Order(List<int>[] adj, int s) { var seen = new HashSet<int> { s }; var q = new List<int> { s }; for (int i = 0; i < q.Count; i++) foreach (var w in adj[q[i]]) if (seen.Add(w)) q.Add(w); return q; }
    static int[] Dist(List<int>[] adj, int s) { var d = Enumerable.Repeat(-1, adj.Length).ToArray(); d[s] = 0; var q = new Queue<int>(); q.Enqueue(s); while (q.Count > 0) { int u = q.Dequeue(); foreach (var w in adj[u]) if (d[w] < 0) { d[w] = d[u] + 1; q.Enqueue(w); } } return d; }

    static (List<int>[] adj, int n) ParseGraph(string inp, out int s)
    {
        var ls = inp.TrimEnd('\n').Split('\n'); var h = ls[0].Split(' ').Select(int.Parse).ToArray(); int n = h[0], m = h[1];
        var adj = Enumerable.Range(0, n + 1).Select(_ => new List<int>()).ToArray();
        for (int i = 1; i <= m; i++) { var p = ls[i].Split(' ').Select(int.Parse).ToArray(); adj[p[0]].Add(p[1]); adj[p[1]].Add(p[0]); }
        foreach (var l in adj) l.Sort();
        s = ls.Length > m + 1 ? int.Parse(ls[m + 1].Split(' ')[0]) : 1;
        return (adj, n);
    }

    const string ReadG = "n, m = map(int, input().split())\nadj = [[] for _ in range(n + 1)]\nfor _ in range(m):\n    u, v = map(int, input().split())\n    adj[u].append(v)\n    adj[v].append(u)\nfor lista in adj:\n    lista.sort()";

    static string GInputs(Random r, bool withS, bool withT)
    {
        int n = Ex.Int(r, 4, 8), m = Ex.Int(r, n - 1, n + 2); var g = RandGraph(r, n, m);
        string s = GraphInput(n, g.edges);
        if (withS) s += Ex.Int(r, 1, n);
        if (withT) s += " " + Ex.Int(r, 1, n);
        return s;
    }

    static (string[] rows, int sr, int sc, int er, int ec) Grid(Random r, int h, int w)
    {
        var g = new char[h][]; for (int i = 0; i < h; i++) { g[i] = new char[w]; for (int j = 0; j < w; j++) g[i][j] = r.Next(100) < 25 ? '#' : '.'; }
        int sr = r.Next(h), sc = r.Next(w), er, ec; do { er = r.Next(h); ec = r.Next(w); } while (er == sr && ec == sc);
        g[sr][sc] = 'S'; g[er][ec] = 'E';
        return (g.Select(x => new string(x)).ToArray(), sr, sc, er, ec);
    }
    static int GridDist(string[] g, char from, char to, bool multi = false)
    {
        int h = g.Length, w = g[0].Length; var d = new int[h, w]; for (int i = 0; i < h; i++) for (int j = 0; j < w; j++) d[i, j] = -1;
        var q = new Queue<(int, int)>();
        for (int i = 0; i < h; i++) for (int j = 0; j < w; j++) if (g[i][j] == from) { d[i, j] = 0; q.Enqueue((i, j)); }
        while (q.Count > 0) { var (a, b) = q.Dequeue(); foreach (var (da, db) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) }) { int x = a + da, y = b + db; if (x < 0 || y < 0 || x >= h || y >= w || g[x][y] == '#' || d[x, y] >= 0) continue; d[x, y] = d[a, b] + 1; q.Enqueue((x, y)); } }
        for (int i = 0; i < h; i++) for (int j = 0; j < w; j++) if (g[i][j] == to) return d[i, j];
        return -1;
    }

    static Exercise MakeCode(int lv, Random r)
    {
        string starterG = ReadG;
        switch (lv)
        {
            case 0:
            {
                var tests = Pg.Tests(new[] { "5 4\n1 2\n1 3\n2 4\n3 5\n1", GInputs(r, true, false), GInputs(r, true, false), GInputs(r, true, false) }, i => { var (adj, n) = ParseGraph(i, out int s); return string.Join(" ", Order(adj, s)); });
                string sol = ReadG + "\ns = int(input())\nvisto = [False] * (n + 1)\nvisto[s] = True\nfila = [s]\ni = 0\nwhile i < len(fila):\n    u = fila[i]\n    i += 1\n    for w in adj[u]:\n        if not visto[w]:\n            visto[w] = True\n            fila.append(w)\nprint(\" \".join(str(x) for x in fila))";
                return Ex.Code(Pg.Prompt(L.T("Leia `N` e `M` (vértices de 1 a N e número de arestas), depois `M` linhas `u v` (cada uma uma aresta de ida e volta entre u e v) e, por fim, o vértice inicial `s`. Faça uma BUSCA EM LARGURA a partir de `s` visitando os vizinhos de cada vértice em ordem CRESCENTE, e mostre a ordem de visita numa linha, separada por espaço (só os vértices que o caminho alcança)."), tests[0]),
                    starterG + "\ns = int(input())\n" + Pg.Comment(false, L.T("escreva a busca em largura")), sol, tests,
                    L.T("A fila garante a ordem por camadas: primeiro o início, depois todos os vizinhos dele, depois os vizinhos dos vizinhos... Marcamos cada vértice como visto ao ENFILEIRAR, para ele entrar na fila uma só vez."));
            }
            case 1:
            {
                var tests = Pg.Tests(new[] { "4 3\n1 2\n2 3\n3 4\n1 4", GInputs(r, true, true), GInputs(r, true, true), GInputs(r, true, true), GInputs(r, true, true) }, i => { var (adj, n) = ParseGraph(i, out _); var last = i.TrimEnd('\n').Split('\n')[^1].Split(' ').Select(int.Parse).ToArray(); return Dist(adj, last[0])[last[1]].ToString(); });
                string sol = ReadG + "\ns, t = map(int, input().split())\ndist = [-1] * (n + 1)\ndist[s] = 0\nfila = [s]\ni = 0\nwhile i < len(fila):\n    u = fila[i]\n    i += 1\n    for w in adj[u]:\n        if dist[w] == -1:\n            dist[w] = dist[u] + 1\n            fila.append(w)\nprint(dist[t])";
                return Ex.Code(Pg.Prompt(L.T("Leia `N` e `M`, depois `M` arestas `u v` (de ida e volta) e, por fim, uma linha com dois vértices `s` e `t`. Mostre o MENOR número de arestas num caminho de `s` até `t`, ou `-1` se não há caminho."), tests[0]),
                    starterG + "\ns, t = map(int, input().split())\n" + Pg.Comment(false, L.T("escreva a busca em largura")), sol, tests,
                    L.T("Na busca em largura, a primeira vez que chegamos a um vértice é pelo caminho mais curto. Guardamos a distância: a de um vértice novo é a do vizinho de onde veio mais 1. Quem nunca foi alcançado fica com −1."));
            }
            case 2:
            {
                if (r.Next(2) == 0)
                {
                    string Gi() { int h = Ex.Int(r, 3, 6), w = Ex.Int(r, 3, 6); var g = Grid(r, h, w); return $"{h} {w}\n" + string.Join("\n", g.rows); }
                    var tests = Pg.Tests(new[] { "3 3\nS..\n.#.\n..E", Gi(), Gi(), Gi(), Gi() }, i => { var ls = i.TrimEnd('\n').Split('\n'); int d = GridDist(ls.Skip(1).ToArray(), 'S', 'E'); return d < 0 ? "Sem caminho" : $"Passos: {d}"; });
                    string sol = "h, w = map(int, input().split())\ng = [input() for _ in range(h)]\ndist = [[-1] * w for _ in range(h)]\nfila = []\nfor i in range(h):\n    for j in range(w):\n        if g[i][j] == \"S\":\n            dist[i][j] = 0\n            fila.append((i, j))\nk = 0\nres = -1\nwhile k < len(fila):\n    a, b = fila[k]\n    k += 1\n    if g[a][b] == \"E\":\n        res = dist[a][b]\n        break\n    for da, db in ((1, 0), (-1, 0), (0, 1), (0, -1)):\n        x, y = a + da, b + db\n        if 0 <= x < h and 0 <= y < w and g[x][y] != \"#\" and dist[x][y] == -1:\n            dist[x][y] = dist[a][b] + 1\n            fila.append((x, y))\nprint(\"Passos: \" + str(res) if res >= 0 else \"Sem caminho\")";
                    return Ex.Code(Pg.Prompt(L.T("Leia `H` e `W` (linhas e colunas) e depois `H` linhas com `W` caracteres: `.` livre, `#` parede, `S` partida e `E` chegada (um de cada). Andando um passo por vez para cima, baixo, esquerda ou direita (sem entrar em paredes), mostre `Passos: <menor número de passos de S até E>` ou `Sem caminho`."), tests[0]),
                        "h, w = map(int, input().split())\ng = [input() for _ in range(h)]\n" + Pg.Comment(false, L.T("escreva a busca em largura na grade")), sol, tests,
                        L.T("A grade é um grafo em que cada célula livre liga às quatro vizinhas. A busca em largura a partir de S descobre as células por ordem de distância; ao tirar o E da fila, a distância dele é a resposta. Se a fila esvaziar antes, não há caminho."));
                }
                else
                {
                    string Model(string inp) { var (adj, n) = ParseGraph(inp, out _); var seen = new bool[n + 1]; int comps = 0, best = 0; for (int s = 1; s <= n; s++) { if (seen[s]) continue; comps++; var o = Order(adj, s); foreach (var x in o) seen[x] = true; best = Math.Max(best, o.Count); } return $"Componentes: {comps}\nMaior: {best}"; }
                    var tests = Pg.Tests(new[] { "6 3\n1 2\n2 3\n5 6", GInputs(r, false, false), GInputs(r, false, false), GInputs(r, false, false) }, Model);
                    string sol = ReadG + "\nvisto = [False] * (n + 1)\ncomps = 0\nmaior = 0\nfor s in range(1, n + 1):\n    if visto[s]:\n        continue\n    comps += 1\n    visto[s] = True\n    fila = [s]\n    i = 0\n    while i < len(fila):\n        u = fila[i]\n        i += 1\n        for w in adj[u]:\n            if not visto[w]:\n                visto[w] = True\n                fila.append(w)\n    maior = max(maior, len(fila))\nprint(\"Componentes: \" + str(comps))\nprint(\"Maior: \" + str(maior))";
                    return Ex.Code(Pg.Prompt(L.T("Leia `N`, `M` e `M` arestas `u v` (de ida e volta). Um COMPONENTE é um grupo de vértices ligados entre si por caminhos. Mostre `Componentes: <quantos>` e `Maior: <tamanho do maior componente>` (vértices sem arestas contam como componentes de tamanho 1)."), tests[0]),
                        starterG + "\n" + Pg.Comment(false, L.T("escreva o resto do programa aqui")), sol, tests,
                        L.T("Percorremos os vértices de 1 a N; cada vértice ainda não visto inicia uma nova busca em largura que marca todo o seu componente. Contamos as buscas iniciadas e medimos o maior alcance."));
                }
            }
            default:
            {
                string Gi() { int h = Ex.Int(r, 3, 6), w = Ex.Int(r, 3, 6); var g = Grid(r, h, w); var rows = g.rows.Select(x => x.Replace('S', 'F')).ToArray(); int extra = Ex.Int(r, 0, 2); for (int k = 0; k < extra; k++) { int a = r.Next(h), b = r.Next(w); if (rows[a][b] == '.') rows[a] = rows[a].Remove(b, 1).Insert(b, "F"); } return $"{h} {w}\n" + string.Join("\n", rows); }
                var tests = Pg.Tests(new[] { "3 4\nF...\n.#..\n..#E", Gi(), Gi(), Gi(), Gi() }, i => { var ls = i.TrimEnd('\n').Split('\n'); int d = GridDist(ls.Skip(1).ToArray(), 'F', 'E'); return d < 0 ? "Seguro" : $"Chega em {d}"; });
                string sol = "h, w = map(int, input().split())\ng = [input() for _ in range(h)]\ndist = [[-1] * w for _ in range(h)]\nfila = []\nfor i in range(h):\n    for j in range(w):\n        if g[i][j] == \"F\":\n            dist[i][j] = 0\n            fila.append((i, j))\nk = 0\nwhile k < len(fila):\n    a, b = fila[k]\n    k += 1\n    for da, db in ((1, 0), (-1, 0), (0, 1), (0, -1)):\n        x, y = a + da, b + db\n        if 0 <= x < h and 0 <= y < w and g[x][y] != \"#\" and dist[x][y] == -1:\n            dist[x][y] = dist[a][b] + 1\n            fila.append((x, y))\nres = -1\nfor i in range(h):\n    for j in range(w):\n        if g[i][j] == \"E\":\n            res = dist[i][j]\nprint(\"Chega em \" + str(res) if res >= 0 else \"Seguro\")";
                return Ex.Code(Pg.Prompt(L.T("Um incêndio! Leia `H` e `W` e depois `H` linhas com `W` caracteres: `.` livre, `#` parede, `F` fogo (pode haver VÁRIOS focos) e `E` uma saída. O fogo se espalha uma célula por minuto para cima, baixo, esquerda e direita, sem atravessar paredes. Mostre `Chega em <minutos>` (o instante em que o fogo alcança `E`) ou `Seguro` se nunca alcança."), tests[0]),
                    "h, w = map(int, input().split())\ng = [input() for _ in range(h)]\n" + Pg.Comment(false, L.T("escreva a busca em largura na grade")), sol, tests,
                    L.T("Truque: em vez de uma busca por foco, colocamos TODOS os focos na fila no começo, com distância 0. A busca em largura única avança todos ao mesmo tempo, e a distância de cada célula é o tempo até o foco MAIS PRÓXIMO chegar."));
            }
        }
    }

    static Exercise MakeOut(int lv, Random r)
    {
        int n = Ex.Int(r, 5, 7); var g = RandGraph(r, n, Ex.Int(r, n - 1, n + 1));
        string adjCode = "adj = [[], " + string.Join(", ", Enumerable.Range(1, n).Select(i => "[" + string.Join(", ", g.adj[i]) + "]")) + "]";
        int s = Ex.Int(r, 1, n);
        if (lv == 0)
        {
            string code = adjCode + $"\nvisto = {{{s}}}\nfila = [{s}]\nordem = []\nwhile len(fila) > 0:\n    u = fila.pop(0)\n    ordem.append(u)\n    for w in adj[u]:\n        if w not in visto:\n            visto.add(w)\n            fila.append(w)\nprint(\" \".join(str(x) for x in ordem))";
            string outp = string.Join(" ", Order(g.adj, s));
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Comece em {0}. A cada passo, tire o primeiro da fila, registre-o e enfileire os vizinhos ainda não vistos, na ordem das listas. Vértices que o caminho não alcança não aparecem.", s));
        }
        if (lv == 1)
        {
            var d = Dist(g.adj, s); int t = Ex.Int(r, 1, n);
            string code = adjCode + $"\ndist = [-1] * {n + 1}\ndist[{s}] = 0\nfila = [{s}]\nwhile len(fila) > 0:\n    u = fila.pop(0)\n    for w in adj[u]:\n        if dist[w] == -1:\n            dist[w] = dist[u] + 1\n            fila.append(w)\nprint(dist[{t}])\nprint(max(dist))\nprint(dist.count(-1))";
            string outp = $"{d[t]}\n{d.Max()}\n{d.Count(x => x == -1)}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("A lista dist tem uma posição extra (índice 0, que nunca é visitado e fica em −1). Depois da busca a partir de {0}: dist = {1}. A segunda linha é o maior valor da lista e a terceira conta os −1 (inclui a posição 0).", s, "[" + string.Join(", ", d) + "]"));
        }
        {
            int h = Ex.Int(r, 3, 4), w = Ex.Int(r, 3, 5); var gr = Grid(r, h, w);
            int steps = GridDist(gr.rows, 'S', 'E');
            // count cells reachable from S and the largest queue length
            var rows = gr.rows; var dd = new int[h, w]; for (int i = 0; i < h; i++) for (int j = 0; j < w; j++) dd[i, j] = -1;
            var q = new List<(int, int)> { (gr.sr, gr.sc) }; dd[gr.sr, gr.sc] = 0; int reach = 0;
            for (int k = 0; k < q.Count; k++) { reach++; var (a, b) = q[k]; foreach (var (da, db) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) }) { int x = a + da, y = b + db; if (x < 0 || y < 0 || x >= h || y >= w || rows[x][y] == '#' || dd[x, y] >= 0) continue; dd[x, y] = dd[a, b] + 1; q.Add((x, y)); } }
            int far = 0; for (int i = 0; i < h; i++) for (int j = 0; j < w; j++) far = Math.Max(far, dd[i, j]);
            string code = $"g = [{string.Join(", ", rows.Select(x => "\"" + x + "\""))}]\nh = {h}\nw = {w}\ndist = [[-1] * w for _ in range(h)]\nfila = [({gr.sr}, {gr.sc})]\ndist[{gr.sr}][{gr.sc}] = 0\nk = 0\nwhile k < len(fila):\n    a, b = fila[k]\n    k += 1\n    for da, db in ((1, 0), (-1, 0), (0, 1), (0, -1)):\n        x, y = a + da, b + db\n        if 0 <= x < h and 0 <= y < w and g[x][y] != \"#\" and dist[x][y] == -1:\n            dist[x][y] = dist[a][b] + 1\n            fila.append((x, y))\nprint(len(fila))\nprint(max(max(l) for l in dist))\nprint(dist[{gr.er}][{gr.ec}])";
            string outp = $"{reach}\n{far}\n{steps}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Mapa:\n{0}\nA busca parte de S ({1}, {2}). `len(fila)` no fim é o número de células alcançadas (incluindo S); o maior valor de dist é a célula alcançada mais distante; e dist da chegada E vale {3} (−1 se não dá para chegar).", string.Join("\n", rows), gr.sr, gr.sc, steps));
        }
    }
}
