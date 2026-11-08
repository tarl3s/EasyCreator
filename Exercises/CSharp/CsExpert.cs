using System.Text;

namespace EasyCreator.Exercises;

/// <summary>C# lessons 16-20: delegates and events, BFS, dynamic programming, iterators, Dijkstra.</summary>
[Exercises("cs-16", "cs-17", "cs-18", "cs-19", "cs-20")]
public sealed class CsExpert : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r) => lesson.Id switch
    {
        "cs-16" => r.Next(2) == 0 ? Write16(d, r) : Predict16(d, r),
        "cs-17" => r.Next(2) == 0 ? Write17(d, r) : Predict17(d, r),
        "cs-18" => r.Next(2) == 0 ? Write18(d, r) : Predict18(d, r),
        "cs-19" => r.Next(2) == 0 ? Write19(d, r) : Predict19(d, r),
        _ => r.Next(2) == 0 ? Write20(d, r) : Predict20(d, r),
    };

    const string S = "// escreva seu código aqui\n";
    static readonly string[] Cities = { "Rio", "Sao", "Recife", "Belem", "Natal", "Curitiba", "Salvador", "Manaus" };

    // ---------- cs-16 ----------
    static Exercise Write16(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        if (d <= Difficulty.Medium)
        {
            bool m = d == Difficulty.Medium;
            st = L.T("Leia um inteiro `x` e, na linha seguinte, uma lista de operações separadas por espaço, escolhidas entre `dobro` (x*2), `mais3` (x+3), `quadrado` (x*x)" + (m ? (", `menos1` (x-1) e `meio` (x/2 inteiro)") : "") + ". Guarde as operações num `Dictionary<string, Func<int, int>>` e aplique-as em ordem, mostrando `<operação>: <valor>` a cada passo e, no fim, `Final: <valor>`.");
            sol = "int x = int.Parse(Console.ReadLine());\nvar ops = new Dictionary<string, Func<int, int>>\n{\n    [\"dobro\"] = v => v * 2,\n    [\"mais3\"] = v => v + 3,\n    [\"quadrado\"] = v => v * v,\n" + (m ? "    [\"menos1\"] = v => v - 1,\n    [\"meio\"] = v => v / 2,\n" : "") + "};\nforeach (var nome in Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries))\n{\n    x = ops[nome](x);\n    Console.WriteLine($\"{nome}: {x}\");\n}\nConsole.WriteLine($\"Final: {x}\");\n";
            string[] names = m ? new[] { "dobro", "mais3", "quadrado", "menos1", "meio" } : new[] { "dobro", "mais3", "quadrado" };
            gen = g => $"{g.Next(1, 9)}\n{string.Join(" ", Enumerable.Range(0, g.Next(1, 6)).Select(_ => PK.Pick(g, names)))}\n";
            oracle = i => { var t = i.Split('\n', StringSplitOptions.RemoveEmptyEntries); int x = int.Parse(t[0]); var o = new List<string>(); foreach (var n in t[1].Split(' ')) { x = n switch { "dobro" => x * 2, "mais3" => x + 3, "quadrado" => x * x, "menos1" => x - 1, _ => x / 2 }; o.Add($"{n}: {x}"); } o.Add($"Final: {x}"); return string.Join("\n", o); };
            why = L.T("Uma lambda é uma função sem nome; guardada num dicionário, o nome do texto escolhe qual aplicar: `ops[nome](x)`.");
        }
        else
        {
            bool gn = d == Difficulty.Genius;
            st = L.T("Leia o limite de temperatura (inteiro) e, na linha seguinte, as leituras (inteiros separados por espaço). Complete a classe `Sensor` com o evento `Action<int, int> Alerta`, disparado por `Registrar(valor)` com o número da leitura (a partir de 1) e o valor quando o valor for **maior** que o limite. No `Main` assine **duas** lambdas, nesta ordem: uma mostra `Alerta: leitura <i> = <v>` e outra conta quantos alertas ocorreram. No fim mostre `Total de alertas: <n>`." + (gn ? " " + ("Depois que ocorrer o 2º alerta, cancele a assinatura da primeira lambda (`-=`): os alertas seguintes só são contados, não mostrados.") : ""));
            sol = "int limite = int.Parse(Console.ReadLine());\nvar leituras = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);\nvar s = new Sensor();\nint total = 0;\nAction<int, int> mostrar = (i, v) => Console.WriteLine($\"Alerta: leitura {i} = {v}\");\ns.Alerta += mostrar;\ns.Alerta += (i, v) =>\n{\n    total++;\n" + (gn ? "    if (total == 2) s.Alerta -= mostrar;\n" : "") + "};\nforeach (var v in leituras) s.Registrar(v, limite);\nConsole.WriteLine($\"Total de alertas: {total}\");\nclass Sensor\n{\n    public event Action<int, int> Alerta;\n    int n;\n    public void Registrar(int v, int limite)\n    {\n        n++;\n        if (v > limite) Alerta?.Invoke(n, v);\n    }\n}\n";
            gen = g => $"{g.Next(10, 40)}\n{PK.Join(PK.Rand(g, g.Next(1, 12), 0, 50))}\n";
            oracle = i => { var t = i.Split('\n', StringSplitOptions.RemoveEmptyEntries); int lim = int.Parse(t[0]); var v = t[1].Split(' ').Select(int.Parse).ToArray(); var o = new List<string>(); int total = 0; bool show = true; for (int k = 0; k < v.Length; k++) if (v[k] > lim) { if (show) o.Add($"Alerta: leitura {k + 1} = {v[k]}"); total++; if (gn && total == 2) show = false; } o.Add($"Total de alertas: {total}"); return string.Join("\n", o); };
            why = L.T("Um `event` guarda uma lista de funções assinadas com `+=`; `Invoke` chama todas, na ordem em que foram assinadas. `-=` cancela uma assinatura. `?.` evita erro se ninguém assinou.");
        }
        return PK.Write(st, S, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict16(Difficulty d, Random r)
    {
        int a = r.Next(2, 6), b = r.Next(2, 6); var o = new List<string>(); string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = $"Func<int, int> f = x => x * {a};\nFunc<int, int> g = x => x + {b};\nConsole.WriteLine(f(g(2)));\nConsole.WriteLine(g(f(2)));\nvar h = (int x) => f(x) - g(x);\nConsole.WriteLine(h({a + b}));\n";
                o.Add(((2 + b) * a).ToString()); o.Add((2 * a + b).ToString()); o.Add(((a + b) * a - (a + b + b)).ToString()); break;
            case Difficulty.Medium:
                code = $"Action<string> falar = s => Console.Write(s + \" \");\nAction muda = () => falar(\"A\");\nmuda += () => falar(\"B\");\nmuda += muda;\nmuda();\nConsole.WriteLine();\nint c = {a};\nFunc<int> inc = () => ++c;\nConsole.WriteLine(inc() + inc());\nConsole.WriteLine(c);\n";
                o.Add("A B A B "); o.Add((a + 1 + a + 2).ToString()); o.Add((a + 2).ToString()); break;
            case Difficulty.Hard:
                code = $"var acoes = new List<Action>();\nfor (int i = 0; i < {a}; i++)\n    acoes.Add(() => Console.Write(i + \" \"));\nforeach (var x in acoes) x();\nConsole.WriteLine();\nacoes.Clear();\nforeach (var j in new[] {{ {b}, {b + 1}, {b + 2} }})\n    acoes.Add(() => Console.Write(j * 2 + \" \"));\nforeach (var x in acoes) x();\nConsole.WriteLine();\n";
                { var sb = new StringBuilder(); for (int i = 0; i < a; i++) sb.Append(a + " "); o.Add(sb.ToString()); o.Add($"{b * 2} {(b + 1) * 2} {(b + 2) * 2} "); }
                break;
            default:
                code = $"var tela = new Tela();\nvar log = new List<string>();\ntela.Mudou += v => log.Add(\"A\" + v);\nint soma = 0;\ntela.Mudou += v => soma += v;\ntela.Mudou += v => {{ if (v > {a}) log.Add(\"grande\"); }};\nfor (int i = {a - 1}; i <= {a + 1}; i++) tela.Set(i);\ntela.Set({a + 1});\nConsole.WriteLine(string.Join(\",\", log));\nConsole.WriteLine(soma);\nclass Tela\n{{\n    int atual = -1;\n    public event Action<int> Mudou;\n    public void Set(int v)\n    {{\n        if (v == atual) return;\n        atual = v;\n        Mudou?.Invoke(v);\n    }}\n}}\n";
                { var log = new List<string>(); int soma = 0, at = -1; void Set(int v) { if (v == at) return; at = v; log.Add("A" + v); soma += v; if (v > a) log.Add("grande"); } for (int i = a - 1; i <= a + 1; i++) Set(i); Set(a + 1); o.Add(string.Join(",", log)); o.Add(soma.ToString()); }
                break;
        }
        return PK.Predict(L.F("O programa usa os números {0} e {1}.", a, b), code, string.Join("\n", o),
            L.T("Delegates multicast chamam todas as funções, na ordem de assinatura. Uma lambda captura a VARIÁVEL (não uma cópia): `i` do `for` é uma só, e vale o valor final quando as ações rodam; `j` do `foreach` é nova a cada volta."));
    }

    // ---------- cs-17 ----------
    static string Roads(Random g, int cities, int m, out List<(string, string)> edges)
    {
        var names = Cities.Take(cities).ToArray(); var set = new List<(string, string)>(); int tries = 0;
        while (set.Count < m && tries++ < 200) { var a = PK.Pick(g, names); var b = PK.Pick(g, names); if (a == b || set.Contains((a, b)) || set.Contains((b, a))) continue; set.Add((a, b)); }
        edges = set; return string.Join("\n", set.Select(e => $"{e.Item1} {e.Item2}"));
    }

    static Dictionary<string, List<string>> Adj(IEnumerable<(string, string)> e)
    {
        var d = new Dictionary<string, List<string>>(); foreach (var (a, b) in e) { if (!d.ContainsKey(a)) d[a] = new(); if (!d.ContainsKey(b)) d[b] = new(); d[a].Add(b); d[b].Add(a); }
        return d;
    }

    static Exercise Write17(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        string rd = "int m = int.Parse(Console.ReadLine());\nvar adj = new Dictionary<string, List<string>>();\nvoid Liga(string a, string b)\n{\n    if (!adj.ContainsKey(a)) adj[a] = new List<string>();\n    adj[a].Add(b);\n}\nfor (int i = 0; i < m; i++)\n{\n    var p = Console.ReadLine().Split(' ');\n    Liga(p[0], p[1]);\n    Liga(p[1], p[0]);\n}\nvar t = Console.ReadLine().Split(' ');\nstring origem = t[0], destino = t[1];\n";
        gen = g =>
        {
            int m = g.Next(1, 8); var body = Roads(g, g.Next(3, 7), m, out var e); var nodes = e.SelectMany(x => new[] { x.Item1, x.Item2 }).Distinct().ToList();
            return $"{e.Count}\n{body}\n{PK.Pick(g, nodes.ToArray())} {PK.Pick(g, nodes.ToArray())}\n";
        };
        (List<(string, string)> edges, string o, string dst) Parse(string i) { var l = i.Split('\n', StringSplitOptions.RemoveEmptyEntries); int m = int.Parse(l[0]); var e = l.Skip(1).Take(m).Select(x => x.Split(' ')).Select(x => (x[0], x[1])).ToList(); var t = l[m + 1].Split(' '); return (e, t[0], t[1]); }
        Dictionary<string, int> Bfs(Dictionary<string, List<string>> adj, string o) { var dist = new Dictionary<string, int> { [o] = 0 }; var q = new Queue<string>(); q.Enqueue(o); while (q.Count > 0) { var u = q.Dequeue(); foreach (var w in adj[u]) if (!dist.ContainsKey(w)) { dist[w] = dist[u] + 1; q.Enqueue(w); } } return dist; }
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Leia `M` estradas de mão dupla (`A B`) e depois `origem destino`. Monte a lista de adjacência na ordem lida e mostre `Vizinhos de <origem>: <vizinhos na ordem em que as estradas foram lidas, separados por espaço>` e `Grau: <quantidade de vizinhos>`.");
                sol = rd + "var viz = adj[origem];\nConsole.WriteLine($\"Vizinhos de {origem}: {string.Join(\" \", viz)}\");\nConsole.WriteLine($\"Grau: {viz.Count}\");\n";
                oracle = i => { var (e, o, _) = Parse(i); var a = Adj(e)[o]; return $"Vizinhos de {o}: {string.Join(" ", a)}\nGrau: {a.Count}"; };
                why = L.T("Cada estrada de mão dupla entra nas duas listas. A lista de um vértice guarda os vizinhos na ordem em que as estradas apareceram.");
                break;
            case Difficulty.Medium:
                st = L.T("Leia `M` estradas de mão dupla e `origem destino`. Faça uma **BFS** a partir da origem (vizinhos na ordem lida) e mostre `Distância: <menor número de estradas até o destino, ou -1 se for inalcançável>` e `Visitadas: <quantas cidades a BFS alcança, incluindo a origem>`.");
                sol = rd + "var dist = new Dictionary<string, int> { [origem] = 0 };\nvar fila = new Queue<string>();\nfila.Enqueue(origem);\nwhile (fila.Count > 0)\n{\n    var u = fila.Dequeue();\n    foreach (var w in adj[u])\n        if (!dist.ContainsKey(w)) { dist[w] = dist[u] + 1; fila.Enqueue(w); }\n}\nConsole.WriteLine($\"Distância: {(dist.TryGetValue(destino, out var x) ? x : -1)}\");\nConsole.WriteLine($\"Visitadas: {dist.Count}\");\n";
                oracle = i => { var (e, o, dst) = Parse(i); var dist = Bfs(Adj(e), o); return $"Distância: {(dist.TryGetValue(dst, out var x) ? x : -1)}\nVisitadas: {dist.Count}"; };
                why = L.T("A fila garante que as cidades a distância k sejam descobertas antes das de distância k+1; o dicionário de distâncias também serve de conjunto de visitadas.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia `M` estradas de mão dupla e `origem destino`. Com BFS (vizinhos na ordem lida, guardando o pai de cada cidade ao descobri-la), mostre `Caminho: <origem> -> ... -> <destino>` e `Distância: <estradas>`; se não houver caminho, mostre só `Sem caminho`.");
                sol = rd + "var pai = new Dictionary<string, string> { [origem] = null };\nvar fila = new Queue<string>();\nfila.Enqueue(origem);\nwhile (fila.Count > 0)\n{\n    var u = fila.Dequeue();\n    foreach (var w in adj[u])\n        if (!pai.ContainsKey(w)) { pai[w] = u; fila.Enqueue(w); }\n}\nif (!pai.ContainsKey(destino)) { Console.WriteLine(\"Sem caminho\"); return; }\nvar caminho = new List<string>();\nfor (var c = destino; c != null; c = pai[c]) caminho.Add(c);\ncaminho.Reverse();\nConsole.WriteLine(\"Caminho: \" + string.Join(\" -> \", caminho));\nConsole.WriteLine($\"Distância: {caminho.Count - 1}\");\n";
                oracle = i =>
                {
                    var (e, o, dst) = Parse(i); var adj = Adj(e); var pai = new Dictionary<string, string> { [o] = null }; var q = new Queue<string>(); q.Enqueue(o);
                    while (q.Count > 0) { var u = q.Dequeue(); foreach (var w in adj[u]) if (!pai.ContainsKey(w)) { pai[w] = u; q.Enqueue(w); } }
                    if (!pai.ContainsKey(dst)) return "Sem caminho"; var p = new List<string>(); for (var c = dst; c != null; c = pai[c]) p.Add(c); p.Reverse(); return "Caminho: " + string.Join(" -> ", p) + $"\nDistância: {p.Count - 1}";
                };
                why = L.T("Guardar o pai de cada cidade ao descobri-la permite reconstruir o caminho de trás para frente a partir do destino; depois inverte-se a lista.");
                break;
            default:
                st = L.T("Leia `M` estradas de mão dupla e uma última linha `origem destino` (ignore-a). Mostre `Componentes: <quantos grupos de cidades ligadas entre si>`, `Maior: <tamanho do maior grupo>` e `Pontes: <estradas cuja remoção aumentaria o número de componentes>`.");
                sol = rd + "var cidades = adj.Keys.ToList();\nint Componentes(string ignA, string ignB)\n{\n    var vis = new HashSet<string>();\n    int c = 0;\n    foreach (var s in cidades)\n    {\n        if (vis.Contains(s)) continue;\n        c++;\n        var fila = new Queue<string>();\n        fila.Enqueue(s);\n        vis.Add(s);\n        while (fila.Count > 0)\n        {\n            var u = fila.Dequeue();\n            foreach (var w in adj[u])\n            {\n                if ((u == ignA && w == ignB) || (u == ignB && w == ignA)) continue;\n                if (vis.Add(w)) fila.Enqueue(w);\n            }\n        }\n    }\n    return c;\n}\nvar visitado = new HashSet<string>();\nint maior = 0;\nforeach (var s in cidades)\n{\n    if (visitado.Contains(s)) continue;\n    int tam = 0;\n    var fila = new Queue<string>();\n    fila.Enqueue(s);\n    visitado.Add(s);\n    while (fila.Count > 0)\n    {\n        var u = fila.Dequeue();\n        tam++;\n        foreach (var w in adj[u]) if (visitado.Add(w)) fila.Enqueue(w);\n    }\n    maior = Math.Max(maior, tam);\n}\nint comp = Componentes(null, null);\nint pontes = 0;\nvar vistas = new HashSet<string>();\nforeach (var u in cidades)\n    foreach (var w in adj[u].Distinct())\n        if (string.CompareOrdinal(u, w) < 0 && Componentes(u, w) > comp) pontes++;\nConsole.WriteLine($\"Componentes: {comp}\");\nConsole.WriteLine($\"Maior: {maior}\");\nConsole.WriteLine($\"Pontes: {pontes}\");\n";
                oracle = i =>
                {
                    var (e, _, _) = Parse(i); var adj = Adj(e); var cid = adj.Keys.ToList();
                    int Comp(string a, string b, out int big) { var vis = new HashSet<string>(); int c = 0; big = 0; foreach (var s in cid) { if (vis.Contains(s)) continue; c++; int sz = 0; var q = new Queue<string>(); q.Enqueue(s); vis.Add(s); while (q.Count > 0) { var u = q.Dequeue(); sz++; foreach (var w in adj[u]) { if ((u == a && w == b) || (u == b && w == a)) continue; if (vis.Add(w)) q.Enqueue(w); } } big = Math.Max(big, sz); } return c; }
                    int comp = Comp(null, null, out int maior); int pontes = 0; foreach (var u in cid) foreach (var w in adj[u].Distinct()) if (string.CompareOrdinal(u, w) < 0 && Comp(u, w, out _) > comp) pontes++;
                    return $"Componentes: {comp}\nMaior: {maior}\nPontes: {pontes}";
                };
                why = L.T("Conte componentes com uma BFS por cidade ainda não visitada. Uma estrada é ponte se, ignorando-a, o número de componentes sobe: refaça a contagem sem essa estrada.");
                break;
        }
        return PK.Write(st, S, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict17(Difficulty d, Random r)
    {
        int n = r.Next(5, 8); var edges = new List<(int, int)>(); int tries = 0; while (edges.Count < n + 1 && tries++ < 100) { int a = r.Next(n), b = r.Next(n); if (a != b && !edges.Contains((a, b)) && !edges.Contains((b, a))) edges.Add((a, b)); }
        var adj = Enumerable.Range(0, n).Select(_ => new List<int>()).ToArray(); foreach (var (a, b) in edges) { adj[a].Add(b); adj[b].Add(a); }
        string init = string.Join("\n", edges.Select(e => $"g[{e.Item1}].Add({e.Item2});\ng[{e.Item2}].Add({e.Item1});"));
        string head = $"var g = new List<int>[{n}];\nfor (int i = 0; i < {n}; i++) g[i] = new List<int>();\n{init}\n";
        var o = new List<string>(); string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = head + $"Console.WriteLine(string.Join(\" \", g.Select(l => l.Count)));\nConsole.WriteLine(string.Join(\" \", g[0]));\n";
                o.Add(string.Join(" ", adj.Select(l => l.Count))); o.Add(string.Join(" ", adj[0])); break;
            case Difficulty.Medium:
            {
                code = head + $"var vis = new bool[{n}];\nvar fila = new Queue<int>();\nfila.Enqueue(0);\nvis[0] = true;\nwhile (fila.Count > 0)\n{{\n    int u = fila.Dequeue();\n    Console.Write(u + \" \");\n    foreach (int w in g[u])\n        if (!vis[w]) {{ vis[w] = true; fila.Enqueue(w); }}\n}}\nConsole.WriteLine();\n";
                var vis = new bool[n]; var q = new Queue<int>(); q.Enqueue(0); vis[0] = true; var sb = new StringBuilder(); while (q.Count > 0) { int u = q.Dequeue(); sb.Append(u + " "); foreach (var w in adj[u]) if (!vis[w]) { vis[w] = true; q.Enqueue(w); } } o.Add(sb.ToString()); break;
            }
            case Difficulty.Hard:
            {
                code = head + $"var dist = Enumerable.Repeat(-1, {n}).ToArray();\nvar fila = new Queue<int>();\nfila.Enqueue(0);\ndist[0] = 0;\nwhile (fila.Count > 0)\n{{\n    int u = fila.Dequeue();\n    foreach (int w in g[u].OrderByDescending(x => x))\n        if (dist[w] < 0) {{ dist[w] = dist[u] + 1; fila.Enqueue(w); }}\n}}\nConsole.WriteLine(string.Join(\" \", dist));\nConsole.WriteLine(dist.Max());\n";
                var dist = Enumerable.Repeat(-1, n).ToArray(); var q = new Queue<int>(); q.Enqueue(0); dist[0] = 0; while (q.Count > 0) { int u = q.Dequeue(); foreach (var w in adj[u].OrderByDescending(x => x)) if (dist[w] < 0) { dist[w] = dist[u] + 1; q.Enqueue(w); } }
                o.Add(string.Join(" ", dist)); o.Add(dist.Max().ToString()); break;
            }
            default:
            {
                code = head + $"var comp = new int[{n}];\nint c = 0;\nfor (int s = 0; s < {n}; s++)\n{{\n    if (comp[s] != 0) continue;\n    c++;\n    var fila = new Queue<int>();\n    fila.Enqueue(s);\n    comp[s] = c;\n    while (fila.Count > 0)\n    {{\n        int u = fila.Dequeue();\n        foreach (int w in g[u]) if (comp[w] == 0) {{ comp[w] = c; fila.Enqueue(w); }}\n    }}\n}}\nConsole.WriteLine(c);\nConsole.WriteLine(string.Join(\"\", comp));\n";
                var comp = new int[n]; int cc = 0; for (int s = 0; s < n; s++) { if (comp[s] != 0) continue; cc++; var q = new Queue<int>(); q.Enqueue(s); comp[s] = cc; while (q.Count > 0) { int u = q.Dequeue(); foreach (var w in adj[u]) if (comp[w] == 0) { comp[w] = cc; q.Enqueue(w); } } }
                o.Add(cc.ToString()); o.Add(string.Join("", comp)); break;
            }
        }
        return PK.Predict(L.F("O grafo do programa tem {0} vértices e as arestas {1}.", n, string.Join(" ", edges.Select(e => $"{e.Item1}-{e.Item2}"))), code, string.Join("\n", o),
            L.T("Desenhe o grafo (as arestas valem nos dois sentidos). A BFS tira o da frente da fila, olha os vizinhos na ordem da lista e coloca no fim os ainda não visitados; a distância de um novo vizinho é a do atual mais 1."));
    }

    // ---------- cs-18 ----------
    static Exercise Write18(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Uma escada tem `n` degraus (1 a 30). Em cada passo você sobe 1 ou 2 degraus. Com programação dinâmica (`dp[i] = dp[i-1] + dp[i-2]`), mostre `Formas: <de quantas maneiras dá para chegar ao degrau n>`. Para subir 1, 2 **ou 3** (mesma ideia), mostre também `Com 3: <maneiras>`.");
                sol = "int n = int.Parse(Console.ReadLine());\nvar a = new long[n + 3];\nvar b = new long[n + 3];\na[0] = 1; b[0] = 1;\nfor (int i = 1; i <= n; i++)\n{\n    a[i] = a[i - 1] + (i >= 2 ? a[i - 2] : 0);\n    b[i] = b[i - 1] + (i >= 2 ? b[i - 2] : 0) + (i >= 3 ? b[i - 3] : 0);\n}\nConsole.WriteLine($\"Formas: {a[n]}\");\nConsole.WriteLine($\"Com 3: {b[n]}\");\n";
                gen = g => $"{g.Next(1, 31)}\n";
                oracle = i => { int n = PK.Ints(i)[0]; var a = new long[n + 3]; var b = new long[n + 3]; a[0] = b[0] = 1; for (int k = 1; k <= n; k++) { a[k] = a[k - 1] + (k >= 2 ? a[k - 2] : 0); b[k] = b[k - 1] + (k >= 2 ? b[k - 2] : 0) + (k >= 3 ? b[k - 3] : 0); } return $"Formas: {a[n]}\nCom 3: {b[n]}"; };
                why = L.T("O número de formas de chegar ao degrau i é a soma das formas de chegar aos degraus de onde se pode saltar para ele. Guardar os valores evita recalcular.");
                break;
            case Difficulty.Medium:
                st = L.T("Leia as moedas (inteiros positivos distintos, separados por espaço) e o valor `V` (0 a 1000). Com programação dinâmica mostre `Mínimo: <menor quantidade de moedas que soma V>` (ou `Mínimo: impossível`) e `Formas: <de quantas maneiras diferentes, sem importar a ordem, dá para formar V>`.");
                sol = "int[] m = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();\nint V = int.Parse(Console.ReadLine());\nvar min = new int[V + 1];\nvar formas = new long[V + 1];\nArray.Fill(min, int.MaxValue);\nmin[0] = 0;\nformas[0] = 1;\nforeach (int c in m)\n    for (int v = c; v <= V; v++)\n    {\n        if (min[v - c] != int.MaxValue) min[v] = Math.Min(min[v], min[v - c] + 1);\n        formas[v] += formas[v - c];\n    }\nConsole.WriteLine(\"Mínimo: \" + (min[V] == int.MaxValue ? \"impossível\" : min[V].ToString()));\nConsole.WriteLine($\"Formas: {formas[V]}\");\n";
                gen = g => { var c = PK.Distinct(g, g.Next(1, 5), 1, 12); return $"{PK.Join(c)}\n{g.Next(0, 60)}\n"; };
                oracle = i => { var l = i.Split('\n', StringSplitOptions.RemoveEmptyEntries); var m = l[0].Split(' ').Select(int.Parse).ToArray(); int V = int.Parse(l[1]); var min = Enumerable.Repeat(int.MaxValue, V + 1).ToArray(); var f = new long[V + 1]; min[0] = 0; f[0] = 1; foreach (int c in m) for (int v = c; v <= V; v++) { if (min[v - c] != int.MaxValue) min[v] = Math.Min(min[v], min[v - c] + 1); f[v] += f[v - c]; } return "Mínimo: " + (min[V] == int.MaxValue ? "impossível" : min[V].ToString()) + $"\nFormas: {f[V]}"; };
                why = L.T("`min[v]` é o melhor para o valor v usando as moedas já vistas; `formas[v]` conta combinações. Percorrer as moedas no laço de fora conta cada combinação uma só vez (sem importar a ordem).");
                break;
            case Difficulty.Hard:
                st = L.T("Leia duas palavras (uma por linha). Com programação dinâmica calcule a **maior subsequência comum** (as letras aparecem nas duas, na mesma ordem, não necessariamente juntas). Mostre `Tamanho: <comprimento>` e `Subsequência: <uma subsequência comum de maior tamanho; ao reconstruir, em caso de empate suba na tabela (i-1) antes de ir para a esquerda (j-1)>`.");
                sol = "string a = Console.ReadLine(), b = Console.ReadLine();\nvar dp = new int[a.Length + 1, b.Length + 1];\nfor (int i = 1; i <= a.Length; i++)\n    for (int j = 1; j <= b.Length; j++)\n        dp[i, j] = a[i - 1] == b[j - 1] ? dp[i - 1, j - 1] + 1 : Math.Max(dp[i - 1, j], dp[i, j - 1]);\nvar sb = new System.Text.StringBuilder();\nint x = a.Length, y = b.Length;\nwhile (x > 0 && y > 0)\n{\n    if (a[x - 1] == b[y - 1]) { sb.Insert(0, a[x - 1]); x--; y--; }\n    else if (dp[x - 1, y] >= dp[x, y - 1]) x--;\n    else y--;\n}\nConsole.WriteLine($\"Tamanho: {dp[a.Length, b.Length]}\");\nConsole.WriteLine($\"Subsequência: {sb}\");\n";
                gen = g => { string[] w = { "abacate", "abacaxi", "banana", "bacalhau", "casa", "cadeira", "computador", "comparador", "programa", "grama" }; return $"{PK.Pick(g, w)}\n{PK.Pick(g, w)}\n"; };
                oracle = i =>
                {
                    var l = i.Split('\n', StringSplitOptions.RemoveEmptyEntries); string a = l[0], b = l[1]; var dp = new int[a.Length + 1, b.Length + 1];
                    for (int x = 1; x <= a.Length; x++) for (int y = 1; y <= b.Length; y++) dp[x, y] = a[x - 1] == b[y - 1] ? dp[x - 1, y - 1] + 1 : Math.Max(dp[x - 1, y], dp[x, y - 1]);
                    var sb = new StringBuilder(); int p = a.Length, q = b.Length; while (p > 0 && q > 0) { if (a[p - 1] == b[q - 1]) { sb.Insert(0, a[p - 1]); p--; q--; } else if (dp[p - 1, q] >= dp[p, q - 1]) p--; else q--; }
                    return $"Tamanho: {dp[a.Length, b.Length]}\nSubsequência: {sb}";
                };
                why = L.T("`dp[i,j]` é a maior subsequência comum dos prefixos de tamanhos i e j: se as letras coincidem, 1 + diagonal; senão, o melhor entre cima e esquerda. Para reconstruir, ande de volta pela tabela.");
                break;
            default:
                st = L.T("Problema da mochila. Leia `n` (1 a 8) itens, cada um numa linha `peso valor`, e a capacidade `C` (0 a 40). Cada item pode ser usado no máximo uma vez. Mostre `Valor máximo: <valor>` e `Itens: <quantos itens foram usados na solução que a reconstrução da tabela encontra, preferindo NÃO usar o último item em caso de empate>`.");
                sol = "int n = int.Parse(Console.ReadLine());\nvar p = new int[n];\nvar v = new int[n];\nfor (int i = 0; i < n; i++)\n{\n    var t = Console.ReadLine().Split(' ');\n    p[i] = int.Parse(t[0]);\n    v[i] = int.Parse(t[1]);\n}\nint C = int.Parse(Console.ReadLine());\nvar dp = new int[n + 1, C + 1];\nfor (int i = 1; i <= n; i++)\n    for (int c = 0; c <= C; c++)\n    {\n        dp[i, c] = dp[i - 1, c];\n        if (c >= p[i - 1]) dp[i, c] = Math.Max(dp[i, c], dp[i - 1, c - p[i - 1]] + v[i - 1]);\n    }\nint usados = 0, cap = C;\nfor (int i = n; i >= 1; i--)\n    if (dp[i, cap] != dp[i - 1, cap]) { usados++; cap -= p[i - 1]; }\nConsole.WriteLine($\"Valor máximo: {dp[n, C]}\");\nConsole.WriteLine($\"Itens: {usados}\");\n";
                gen = g => { int n = g.Next(1, 8); return n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => $"{g.Next(1, 15)} {g.Next(1, 30)}")) + "\n" + g.Next(0, 41) + "\n"; };
                oracle = i =>
                {
                    var t = PK.Ints(i); int n = t[0]; var p = new int[n]; var v = new int[n]; for (int k = 0; k < n; k++) { p[k] = t[1 + 2 * k]; v[k] = t[2 + 2 * k]; } int C = t[1 + 2 * n];
                    var dp = new int[n + 1, C + 1]; for (int k = 1; k <= n; k++) for (int c = 0; c <= C; c++) { dp[k, c] = dp[k - 1, c]; if (c >= p[k - 1]) dp[k, c] = Math.Max(dp[k, c], dp[k - 1, c - p[k - 1]] + v[k - 1]); }
                    int us = 0, cap = C; for (int k = n; k >= 1; k--) if (dp[k, cap] != dp[k - 1, cap]) { us++; cap -= p[k - 1]; }
                    return $"Valor máximo: {dp[n, C]}\nItens: {us}";
                };
                why = L.T("`dp[i,c]` é o melhor valor usando os i primeiros itens com capacidade c: ou ignora o item i, ou o usa (pagando o peso e ganhando o valor). Se `dp[i,c]` difere de `dp[i-1,c]`, o item i foi usado.");
                break;
        }
        return PK.Write(st, S, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict18(Difficulty d, Random r)
    {
        int n = r.Next(5, 9), a = r.Next(1, 4), b = r.Next(2, 5); var o = new List<string>(); string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = $"var dp = new long[{n + 1}];\ndp[0] = {a};\ndp[1] = {b};\nfor (int i = 2; i <= {n}; i++) dp[i] = dp[i - 1] + dp[i - 2];\nConsole.WriteLine(string.Join(\" \", dp));\n";
                { var dp = new long[n + 1]; dp[0] = a; dp[1] = b; for (int i = 2; i <= n; i++) dp[i] = dp[i - 1] + dp[i - 2]; o.Add(string.Join(" ", dp)); }
                break;
            case Difficulty.Medium:
                code = $"var dp = new int[{n * 2 + 1}];\nfor (int i = 1; i < dp.Length; i++)\n{{\n    dp[i] = dp[i - 1] + 1;\n    if (i >= {b}) dp[i] = Math.Min(dp[i], dp[i - {b}] + 1);\n}}\nConsole.WriteLine(string.Join(\" \", dp));\n";
                { var dp = new int[n * 2 + 1]; for (int i = 1; i < dp.Length; i++) { dp[i] = dp[i - 1] + 1; if (i >= b) dp[i] = Math.Min(dp[i], dp[i - b] + 1); } o.Add(string.Join(" ", dp)); }
                break;
            case Difficulty.Hard:
            {
                var w = new[] { "casa", "mesa", "pasta", "massa" }; string s = PK.Pick(r, w), t = PK.Pick(r, w);
                code = $"string a = \"{s}\", b = \"{t}\";\nvar dp = new int[a.Length + 1, b.Length + 1];\nfor (int i = 1; i <= a.Length; i++)\n    for (int j = 1; j <= b.Length; j++)\n        dp[i, j] = a[i - 1] == b[j - 1] ? dp[i - 1, j - 1] + 1 : Math.Max(dp[i - 1, j], dp[i, j - 1]);\nfor (int i = 0; i <= a.Length; i++)\n{{\n    var linha = new List<int>();\n    for (int j = 0; j <= b.Length; j++) linha.Add(dp[i, j]);\n    Console.WriteLine(string.Join(\"\", linha));\n}}\n";
                var dp = new int[s.Length + 1, t.Length + 1]; for (int i = 1; i <= s.Length; i++) for (int j = 1; j <= t.Length; j++) dp[i, j] = s[i - 1] == t[j - 1] ? dp[i - 1, j - 1] + 1 : Math.Max(dp[i - 1, j], dp[i, j - 1]);
                for (int i = 0; i <= s.Length; i++) o.Add(string.Join("", Enumerable.Range(0, t.Length + 1).Select(j => dp[i, j])));
                return PK.Predict(L.F("As palavras comparadas são \"{0}\" e \"{1}\".", s, t), code, string.Join("\n", o), L.T("Preencha a tabela linha por linha: letras iguais valem 1 + diagonal; senão, o maior entre o valor de cima e o da esquerda."));
            }
            default:
            {
                var moedas = PK.Distinct(r, 3, 1, 6).OrderBy(x => x).ToArray(); int V = r.Next(6, 12);
                code = $"int[] m = {{ {PK.Join(moedas, ", ")} }};\nint V = {V};\nvar formas = new int[V + 1];\nformas[0] = 1;\nforeach (int c in m)\n{{\n    for (int v = c; v <= V; v++) formas[v] += formas[v - c];\n    Console.WriteLine(string.Join(\" \", formas));\n}}\n";
                var f = new int[V + 1]; f[0] = 1; foreach (int c in moedas) { for (int v = c; v <= V; v++) f[v] += f[v - c]; o.Add(string.Join(" ", f)); }
                break;
            }
        }
        return PK.Predict(L.F("O programa usa os números {0}, {1} e {2}.", n, a, b), code, string.Join("\n", o),
            L.T("Acompanhe o vetor `dp` preenchendo posição por posição (ou moeda por moeda), sempre usando os valores já calculados nas posições anteriores."));
    }

    // ---------- cs-19 ----------
    static Exercise Write19(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Escreva um iterador `static IEnumerable<int> Multiplos(int k)` com `yield return` que gera infinitamente `k, 2k, 3k, ...`. Leia `k` e `n` (um por linha) e mostre `Primeiros <n>: <os n primeiros múltiplos separados por espaço>` e `Soma: <soma deles>`, usando `Take`.");
                sol = "int k = int.Parse(Console.ReadLine());\nint n = int.Parse(Console.ReadLine());\nvar l = Multiplos(k).Take(n).ToList();\nConsole.WriteLine($\"Primeiros {n}: {string.Join(\" \", l)}\");\nConsole.WriteLine($\"Soma: {l.Sum()}\");\nstatic IEnumerable<int> Multiplos(int k)\n{\n    for (int i = 1; ; i++) yield return i * k;\n}\n";
                gen = g => $"{g.Next(1, 20)}\n{g.Next(1, 10)}\n"; oracle = i => { var v = PK.Ints(i); var l = Enumerable.Range(1, v[1]).Select(x => x * v[0]).ToList(); return $"Primeiros {v[1]}: {PK.Join(l)}\nSoma: {l.Sum()}"; };
                why = L.T("O iterador entrega um valor por vez a cada `yield return` e continua de onde parou. Como é preguiçoso, pode ser infinito: `Take(n)` pede só os primeiros.");
                break;
            case Difficulty.Medium:
                st = L.T("Escreva `static IEnumerable<long> Fibonacci()` (0, 1, 1, 2, 3, ... infinito) com `yield`. Leia um limite `L` e mostre `Menores que <L>: <os termos menores que L separados por espaço>` (use `TakeWhile`), `Pares: <soma dos termos pares menores que L>` e `Índice do primeiro >= L: <posição (começando em 0) do primeiro termo maior ou igual a L>`.");
                sol = "long L = long.Parse(Console.ReadLine());\nvar menores = Fibonacci().TakeWhile(x => x < L).ToList();\nConsole.WriteLine($\"Menores que {L}: {string.Join(\" \", menores)}\");\nConsole.WriteLine($\"Pares: {menores.Where(x => x % 2 == 0).Sum()}\");\nConsole.WriteLine($\"Índice do primeiro >= {L}: {menores.Count}\");\nstatic IEnumerable<long> Fibonacci()\n{\n    long a = 0, b = 1;\n    while (true)\n    {\n        yield return a;\n        (a, b) = (b, a + b);\n    }\n}\n";
                gen = g => $"{g.Next(1, 2000)}\n";
                oracle = i => { long L = long.Parse(i.Trim()); var l = new List<long>(); long a = 0, b = 1; while (a < L) { l.Add(a); (a, b) = (b, a + b); } return $"Menores que {L}: {string.Join(" ", l)}\nPares: {l.Where(x => x % 2 == 0).Sum()}\nÍndice do primeiro >= {L}: {l.Count}"; };
                why = L.T("`TakeWhile` consome o iterador infinito só até a condição falhar. O índice do primeiro termo que não entrou é o tamanho da lista.");
                break;
            case Difficulty.Hard:
                st = L.T("Escreva `static IEnumerable<int> Primos()` (infinito, com `yield`). Leia `n` e `a` (um por linha). Mostre `O <n>º primo: <valor>`, `Primeiro primo maior que <a>: <valor>` e `Primos até <a>: <quantidade de primos menores ou iguais a a>`. Use LINQ sobre o iterador (`ElementAt`, `First`, `TakeWhile`).");
                sol = "int n = int.Parse(Console.ReadLine());\nint a = int.Parse(Console.ReadLine());\nConsole.WriteLine($\"O {n}º primo: {Primos().ElementAt(n - 1)}\");\nConsole.WriteLine($\"Primeiro primo maior que {a}: {Primos().First(p => p > a)}\");\nConsole.WriteLine($\"Primos até {a}: {Primos().TakeWhile(p => p <= a).Count()}\");\nstatic IEnumerable<int> Primos()\n{\n    for (int n = 2; ; n++)\n    {\n        bool primo = true;\n        for (int d = 2; d * d <= n; d++) if (n % d == 0) { primo = false; break; }\n        if (primo) yield return n;\n    }\n}\n";
                gen = g => $"{g.Next(1, 60)}\n{g.Next(1, 400)}\n";
                oracle = i => { var v = PK.Ints(i); int n = v[0], a = v[1]; var l = new List<int>(); for (int x = 2; l.Count < 200; x++) { bool p = true; for (int dd = 2; dd * dd <= x; dd++) if (x % dd == 0) { p = false; break; } if (p) l.Add(x); } return $"O {n}º primo: {l[n - 1]}\nPrimeiro primo maior que {a}: {l.First(p => p > a)}\nPrimos até {a}: {l.Count(p => p <= a)}"; };
                why = L.T("O iterador de primos nunca acaba, mas cada consulta LINQ puxa só o que precisa: `ElementAt` os primeiros n, `First` até achar, `TakeWhile` até passar do limite.");
                break;
            default:
                st = L.T("Escreva `static IEnumerable<long> Collatz(long n)` (de `n` até 1, inclusive) e `static IEnumerable<T> Pares<T>(IEnumerable<T> s)` que devolve os elementos nas posições 0, 2, 4... de qualquer sequência, preguiçosamente. Leia `n` (1 a 3000) e mostre `Tamanho: <quantos termos tem a sequência de Collatz>`, `Máximo: <maior termo>` e `Alternados: <termos nas posições pares (0, 2, 4...) separados por espaço>` (para n = 1 só o `1`).");
                sol = "long n = long.Parse(Console.ReadLine());\nConsole.WriteLine($\"Tamanho: {Collatz(n).Count()}\");\nConsole.WriteLine($\"Máximo: {Collatz(n).Max()}\");\nConsole.WriteLine($\"Alternados: {string.Join(\" \", Pares(Collatz(n)))}\");\nstatic IEnumerable<long> Collatz(long n)\n{\n    yield return n;\n    while (n != 1)\n    {\n        n = n % 2 == 0 ? n / 2 : 3 * n + 1;\n        yield return n;\n    }\n}\nstatic IEnumerable<T> Pares<T>(IEnumerable<T> s)\n{\n    int i = 0;\n    foreach (var x in s)\n    {\n        if (i % 2 == 0) yield return x;\n        i++;\n    }\n}\n";
                gen = g => $"{g.Next(1, 3001)}\n";
                oracle = i => { long n = long.Parse(i.Trim()); var l = new List<long> { n }; while (n != 1) { n = n % 2 == 0 ? n / 2 : 3 * n + 1; l.Add(n); } return $"Tamanho: {l.Count}\nMáximo: {l.Max()}\nAlternados: {string.Join(" ", l.Where((x, k) => k % 2 == 0))}"; };
                why = L.T("Os dois iteradores se encadeiam: `Pares` consome o que `Collatz` gera, um termo por vez, sem montar listas. O primeiro `yield return n` entrega o ponto de partida.");
                break;
        }
        return PK.Write(st, S, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict19(Difficulty d, Random r)
    {
        int a = r.Next(2, 6), b = r.Next(2, 5); var o = new List<string>(); string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = $"foreach (var x in Contar({a}))\n    Console.WriteLine(\"usou \" + x);\nstatic IEnumerable<int> Contar(int n)\n{{\n    for (int i = 1; i <= n; i++)\n    {{\n        Console.WriteLine(\"gerou \" + i);\n        yield return i * {b};\n    }}\n}}\n";
                for (int i = 1; i <= a; i++) { o.Add("gerou " + i); o.Add("usou " + i * b); } break;
            case Difficulty.Medium:
                code = $"var q = Contar({a + 2});\nConsole.WriteLine(\"criou\");\nvar primeiro = q.First();\nConsole.WriteLine(\"primeiro \" + primeiro);\nConsole.WriteLine(q.Take({b}).Sum());\nstatic IEnumerable<int> Contar(int n)\n{{\n    for (int i = 1; i <= n; i++)\n    {{\n        Console.Write(\"g\" + i + \" \");\n        yield return i;\n    }}\n}}\n";
                { o.Add("criou"); o.Add("g1 primeiro 1"); var sb = new StringBuilder(); int s = 0; for (int i = 1; i <= b; i++) { sb.Append("g" + i + " "); s += i; } o.Add(sb.ToString() + s); }
                break;
            case Difficulty.Hard:
                code = $"var q = Numeros().Where(x =>\n{{\n    Console.Write(\"f\" + x + \" \");\n    return x % 2 == 0;\n}}).Select(x =>\n{{\n    Console.Write(\"s\" + x + \" \");\n    return x * {b};\n}});\nforeach (var v in q.Take({a}))\n    Console.WriteLine(\"= \" + v);\nstatic IEnumerable<int> Numeros()\n{{\n    int i = 1;\n    while (true) yield return i++;\n}}\n";
                { int cnt = 0; var sb = new StringBuilder(); for (int x = 1; cnt < a; x++) { sb.Append("f" + x + " "); if (x % 2 == 0) { sb.Append("s" + x + " "); o.Add(sb.ToString() + "= " + x * b); sb.Clear(); cnt++; } } }
                break;
            default:
                code = $"int chamadas = 0;\nIEnumerable<int> Gera()\n{{\n    chamadas++;\n    for (int i = 0; i < {a}; i++) yield return i * {b};\n}}\nvar q = Gera();\nConsole.WriteLine(chamadas);\nConsole.WriteLine(q.Count() + \" \" + chamadas);\nConsole.WriteLine(q.Sum() + \" \" + chamadas);\nvar l = q.ToList();\nConsole.WriteLine(l.Count + \" \" + chamadas);\nforeach (var x in l.Concat(q).Skip({a - 1}).Take(3)) Console.Write(x + \" \");\nConsole.WriteLine(chamadas);\n";
                { var l = Enumerable.Range(0, a).Select(i => i * b).ToList(); o.Add("0"); o.Add($"{a} 1"); o.Add($"{l.Sum()} 2"); o.Add($"{a} 3"); o.Add(string.Join("", l.Concat(l).Skip(a - 1).Take(3).Select(x => x + " ")) + "4"); }
                break;
        }
        return PK.Predict(L.F("O iterador do programa usa os números {0} e {1}.", a, b), code, string.Join("\n", o),
            L.T("Um iterador com `yield` só roda quando alguém pede o próximo valor, e para depois de cada `yield return`. Cada consumo (`foreach`, `Count`, `Sum`...) executa o iterador DE NOVO desde o início; `ToList` guarda os valores."));
    }

    // ---------- cs-20 ----------
    static string WGraph(Random g, int v, int e)
    {
        var set = new HashSet<(int, int)>(); int tries = 0; while (set.Count < e && tries++ < 200) { int a = g.Next(v), b = g.Next(v); if (a != b) set.Add((a, b)); }
        return $"{v} {set.Count}\n" + string.Join("", set.Select(x => $"{x.Item1} {x.Item2} {g.Next(1, 10)}\n"));
    }

    static long[] Dij(int[] t, out long[] ways, out int[] pai)
    {
        int V = t[0], E = t[1]; var adj = Enumerable.Range(0, V).Select(_ => new List<(int, int)>()).ToArray();
        for (int k = 0; k < E; k++) adj[t[2 + 3 * k]].Add((t[3 + 3 * k], t[4 + 3 * k]));
        int s = t[2 + 3 * E]; var dist = Enumerable.Repeat(long.MaxValue, V).ToArray(); ways = new long[V]; pai = Enumerable.Repeat(-1, V).ToArray(); dist[s] = 0; ways[s] = 1;
        var pq = new PriorityQueue<int, long>(); pq.Enqueue(s, 0);
        while (pq.TryDequeue(out int u, out long du))
        {
            if (du > dist[u]) continue;
            foreach (var (w, c) in adj[u]) { if (du + c < dist[w]) { dist[w] = du + c; ways[w] = ways[u]; pai[w] = u; pq.Enqueue(w, dist[w]); } else if (du + c == dist[w]) ways[w] += ways[u]; }
        }
        return dist;
    }

    static Exercise Write20(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        string rd = "var p0 = Console.ReadLine().Split(' ');\nint V = int.Parse(p0[0]), E = int.Parse(p0[1]);\nvar adj = new List<(int v, int w)>[V];\nfor (int i = 0; i < V; i++) adj[i] = new List<(int v, int w)>();\nfor (int i = 0; i < E; i++)\n{\n    var p = Console.ReadLine().Split(' ');\n    adj[int.Parse(p[0])].Add((int.Parse(p[1]), int.Parse(p[2])));\n}\nint origem = int.Parse(Console.ReadLine());\nvar dist = new long[V];\nArray.Fill(dist, long.MaxValue);\nvar pai = new int[V];\nArray.Fill(pai, -1);\nvar ways = new long[V];\ndist[origem] = 0;\nways[origem] = 1;\nvar pq = new PriorityQueue<int, long>();\npq.Enqueue(origem, 0);\nwhile (pq.TryDequeue(out int u, out long du))\n{\n    if (du > dist[u]) continue;\n    foreach (var (v, w) in adj[u])\n    {\n        if (du + w < dist[v]) { dist[v] = du + w; ways[v] = ways[u]; pai[v] = u; pq.Enqueue(v, dist[v]); }\n        else if (du + w == dist[v]) ways[v] += ways[u];\n    }\n}\n";
        gen = g => { int V = g.Next(2, 7); return WGraph(g, V, g.Next(1, V * 2)) + g.Next(V) + "\n"; };
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Leia `V` e `E` (vértices de 0 a V-1) e `E` arestas **dirigidas** `u v peso` (pesos de 1 a 9), depois a `origem`. Com Dijkstra e `PriorityQueue`, mostre a menor distância da origem a cada vértice, uma linha `<v>: <distância>` (ou `<v>: inalcançável`).");
                sol = rd + "for (int i = 0; i < V; i++) Console.WriteLine($\"{i}: {(dist[i] == long.MaxValue ? \"inalcançável\" : dist[i].ToString())}\");\n";
                oracle = i => { var t = PK.Ints(i); var dist = Dij(t, out _, out _); return string.Join("\n", dist.Select((x, k) => $"{k}: {(x == long.MaxValue ? "inalcançável" : x.ToString())}")); };
                why = L.T("A fila de prioridade entrega sempre o vértice mais perto da origem ainda não fechado; ao relaxar uma aresta, só entra na fila se a distância melhorou. Entradas antigas na fila são ignoradas (`du > dist[u]`).");
                break;
            case Difficulty.Medium:
                st = L.T("Mesma entrada (grafo dirigido com pesos e origem). Mostre `Mais distante: <vértice> (<distância>)` entre os alcançáveis (empate: o menor número), `Alcançáveis: <quantos vértices, incluindo a origem>` e `Soma: <soma das distâncias dos alcançáveis>`.");
                sol = rd + "int melhor = origem;\nint alc = 0;\nlong soma = 0;\nfor (int i = 0; i < V; i++)\n{\n    if (dist[i] == long.MaxValue) continue;\n    alc++;\n    soma += dist[i];\n    if (dist[i] > dist[melhor]) melhor = i;\n}\nConsole.WriteLine($\"Mais distante: {melhor} ({dist[melhor]})\");\nConsole.WriteLine($\"Alcançáveis: {alc}\");\nConsole.WriteLine($\"Soma: {soma}\");\n";
                oracle = i => { var t = PK.Ints(i); var dist = Dij(t, out _, out _); int s = t[2 + 3 * t[1]]; int best = s; for (int k = 0; k < dist.Length; k++) if (dist[k] != long.MaxValue && dist[k] > dist[best]) best = k; var ok = dist.Where(x => x != long.MaxValue); return $"Mais distante: {best} ({dist[best]})\nAlcançáveis: {ok.Count()}\nSoma: {ok.Sum()}"; };
                why = L.T("Depois do Dijkstra, as distâncias finitas são os alcançáveis. Para o mais distante, só troque quando a distância for estritamente maior.");
                break;
            case Difficulty.Hard:
                st = L.T("Mesma entrada. Para cada vértice alcançável, em ordem de número, mostre `<v>: <distância> via <caminho da origem até v com ' -> '>`, guardando o vértice anterior (pai) quando a distância melhora. Para vértices inalcançáveis mostre `<v>: inalcançável`.");
                sol = rd + "for (int i = 0; i < V; i++)\n{\n    if (dist[i] == long.MaxValue) { Console.WriteLine($\"{i}: inalcançável\"); continue; }\n    var c = new List<int>();\n    for (int x = i; x != -1; x = pai[x]) c.Add(x);\n    c.Reverse();\n    Console.WriteLine($\"{i}: {dist[i]} via {string.Join(\" -> \", c)}\");\n}\n";
                oracle = i =>
                {
                    var t = PK.Ints(i); var dist = Dij(t, out _, out var pai); var o = new List<string>();
                    for (int k = 0; k < dist.Length; k++) { if (dist[k] == long.MaxValue) { o.Add($"{k}: inalcançável"); continue; } var c = new List<int>(); for (int x = k; x != -1; x = pai[x]) c.Add(x); c.Reverse(); o.Add($"{k}: {dist[k]} via {string.Join(" -> ", c)}"); }
                    return string.Join("\n", o);
                };
                why = L.T("O pai de um vértice é atualizado só quando uma distância estritamente menor é encontrada; seguindo os pais de trás para frente reconstrói-se o caminho.");
                break;
            default:
                st = L.T("Mesma entrada. Além da distância mínima, conte de **quantos caminhos diferentes** de custo mínimo a origem chega a cada vértice (a origem tem 1). Mostre `<v>: <distância> em <caminhos> caminho(s)` ou `<v>: inalcançável`, e por fim `Total de caminhos mínimos: <soma dos caminhos de todos os alcançáveis>`.");
                sol = rd + "long total = 0;\nfor (int i = 0; i < V; i++)\n{\n    if (dist[i] == long.MaxValue) { Console.WriteLine($\"{i}: inalcançável\"); continue; }\n    Console.WriteLine($\"{i}: {dist[i]} em {ways[i]} caminho(s)\");\n    total += ways[i];\n}\nConsole.WriteLine($\"Total de caminhos mínimos: {total}\");\n";
                oracle = i =>
                {
                    var t = PK.Ints(i); var dist = Dij(t, out var ways, out _); var o = new List<string>(); long tot = 0;
                    for (int k = 0; k < dist.Length; k++) { if (dist[k] == long.MaxValue) { o.Add($"{k}: inalcançável"); continue; } o.Add($"{k}: {dist[k]} em {ways[k]} caminho(s)"); tot += ways[k]; }
                    o.Add($"Total de caminhos mínimos: {tot}"); return string.Join("\n", o);
                };
                why = L.T("Quando uma nova distância é MENOR, os caminhos mínimos de `v` são os de `u`; quando é IGUAL, somam-se. Como os pesos são positivos, `u` já está com seus caminhos completos ao ser retirado da fila.");
                break;
        }
        return PK.Write(st, S, sol, PK.Tests(r, 7, gen, oracle), why);
    }

    static Exercise Predict20(Difficulty d, Random r)
    {
        var o = new List<string>(); string code; int n = r.Next(4, 7);
        switch (d)
        {
            case Difficulty.Easy:
            {
                var items = PK.Distinct(r, n, 1, 30); var names = Cities.Take(n).ToArray();
                code = $"var pq = new PriorityQueue<string, int>();\n" + string.Join("", names.Select((nm, i) => $"pq.Enqueue(\"{nm}\", {items[i]});\n")) + "while (pq.Count > 0) Console.Write(pq.Dequeue() + \" \");\nConsole.WriteLine();\n";
                o.Add(string.Join("", names.Select((nm, i) => (nm, items[i])).OrderBy(x => x.Item2).Select(x => x.nm + " "))); break;
            }
            case Difficulty.Medium:
            {
                var pr = PK.Rand(r, n, 1, 4);
                code = $"var pq = new PriorityQueue<int, int>();\nint[] prioridades = {{ {PK.Join(pr, ", ")} }};\nfor (int i = 0; i < prioridades.Length; i++) pq.Enqueue(i, prioridades[i]);\nvar saida = new List<int>();\nwhile (pq.TryDequeue(out int item, out int p))\n{{\n    saida.Add(item);\n    if (p == 1 && item < 3) pq.Enqueue(item + 10, 5);\n}}\nConsole.WriteLine(string.Join(\" \", saida));\n";
                // empate: ordem de saída de itens com a mesma prioridade não é garantida pelo .NET -> usar prioridades distintas
                var items = PK.Distinct(r, n, 1, 20); var pq = new PriorityQueue<int, int>(); for (int i = 0; i < n; i++) pq.Enqueue(i, items[i]);
                code = $"var pq = new PriorityQueue<int, int>();\nint[] prioridades = {{ {PK.Join(items, ", ")} }};\nfor (int i = 0; i < prioridades.Length; i++) pq.Enqueue(i, prioridades[i]);\nvar saida = new List<int>();\nwhile (pq.TryDequeue(out int item, out int p))\n{{\n    saida.Add(item);\n    if (item < 2 && p < 100) pq.Enqueue(item + 10, p + 7);\n}}\nConsole.WriteLine(string.Join(\" \", saida));\n";
                var q = new PriorityQueue<int, int>(); for (int i = 0; i < n; i++) q.Enqueue(i, items[i]); var sa = new List<int>(); var seen = new HashSet<int>(); while (q.TryDequeue(out int it, out int p)) { sa.Add(it); if (it < 2 && p < 100) q.Enqueue(it + 10, p + 7); }
                // distinct priorities cannot be guaranteed after p+7; guard
                var allPr = new List<int>(items); allPr.AddRange(items.Take(2).Select(x => x + 7)); allPr.AddRange(items.Take(2).Select(x => x + 14).Where(_ => false));
                if (allPr.Distinct().Count() != allPr.Count) return Predict20(d, r);
                o.Add(string.Join(" ", sa)); _ = seen;
                break;
            }
            case Difficulty.Hard:
            {
                int[][] e = { new[] { 0, 1, 4 }, new[] { 0, 2, 1 }, new[] { 2, 1, 2 }, new[] { 1, 3, 1 }, new[] { 2, 3, 5 } };
                int bump = r.Next(0, 4); e[3][2] += bump; e[1][2] += r.Next(0, 3);
                code = $"var adj = new List<(int v, int w)>[4];\nfor (int i = 0; i < 4; i++) adj[i] = new List<(int v, int w)>();\n" + string.Join("", e.Select(x => $"adj[{x[0]}].Add(({x[1]}, {x[2]}));\n")) + "var dist = new int[] { 0, int.MaxValue, int.MaxValue, int.MaxValue };\nvar pq = new PriorityQueue<int, int>();\npq.Enqueue(0, 0);\nwhile (pq.TryDequeue(out int u, out int du))\n{\n    if (du > dist[u]) continue;\n    Console.Write(u + \":\" + du + \" \");\n    foreach (var (v, w) in adj[u])\n        if (du + w < dist[v]) { dist[v] = du + w; pq.Enqueue(v, dist[v]); }\n}\nConsole.WriteLine();\nConsole.WriteLine(string.Join(\" \", dist));\n";
                var ad = Enumerable.Range(0, 4).Select(_ => new List<(int, int)>()).ToArray(); foreach (var x in e) ad[x[0]].Add((x[1], x[2]));
                var dist = new[] { 0, int.MaxValue, int.MaxValue, int.MaxValue }; var q = new PriorityQueue<int, int>(); q.Enqueue(0, 0); var sb = new StringBuilder(); var prs = new HashSet<int>();
                while (q.TryDequeue(out int u, out int du)) { if (du > dist[u]) continue; sb.Append($"{u}:{du} "); foreach (var (v, w) in ad[u]) if (du + w < dist[v]) { dist[v] = du + w; if (!prs.Add(dist[v] * 10 + v)) { } q.Enqueue(v, dist[v]); } }
                // equal-priority ties may order differently: avoid by checking all enqueued priorities are distinct
                var tryDist = new List<int>(); var q2 = new PriorityQueue<int, int>(); var d2 = new[] { 0, int.MaxValue, int.MaxValue, int.MaxValue }; q2.Enqueue(0, 0); tryDist.Add(0);
                while (q2.TryDequeue(out int u, out int du)) { if (du > d2[u]) continue; foreach (var (v, w) in ad[u]) if (du + w < d2[v]) { d2[v] = du + w; tryDist.Add(d2[v]); q2.Enqueue(v, d2[v]); } }
                if (tryDist.Distinct().Count() != tryDist.Count) return Predict20(d, r);
                o.Add(sb.ToString()); o.Add(string.Join(" ", dist)); break;
            }
            default:
            {
                var w = PK.Distinct(r, 5, 1, 9); // grid-like chain with shortcut
                code = $"int[] custo = {{ {PK.Join(w, ", ")} }};\nvar dist = new long[custo.Length];\nArray.Fill(dist, long.MaxValue);\ndist[0] = custo[0];\nvar pq = new PriorityQueue<int, long>();\npq.Enqueue(0, dist[0]);\nint pops = 0;\nwhile (pq.TryDequeue(out int i, out long d))\n{{\n    pops++;\n    if (d > dist[i]) continue;\n    foreach (int j in new[] {{ i + 1, i + 2 }})\n    {{\n        if (j >= custo.Length) continue;\n        if (d + custo[j] < dist[j]) {{ dist[j] = d + custo[j]; pq.Enqueue(j, dist[j]); }}\n    }}\n}}\nConsole.WriteLine(string.Join(\" \", dist));\nConsole.WriteLine(pops);\n";
                var ds = Enumerable.Repeat(long.MaxValue, 5).ToArray(); ds[0] = w[0]; var q = new PriorityQueue<int, long>(); q.Enqueue(0, ds[0]); int pops = 0; var prios = new List<long> { ds[0] };
                while (q.TryDequeue(out int i, out long dd)) { pops++; if (dd > ds[i]) continue; foreach (int j in new[] { i + 1, i + 2 }) { if (j >= 5) continue; if (dd + w[j] < ds[j]) { ds[j] = dd + w[j]; prios.Add(ds[j]); q.Enqueue(j, ds[j]); } } }
                if (prios.Distinct().Count() != prios.Count) return Predict20(d, r);
                o.Add(string.Join(" ", ds)); o.Add(pops.ToString()); break;
            }
        }
        return PK.Predict(L.F("O programa usa uma fila de prioridade com {0} elementos.", n), code, string.Join("\n", o),
            L.T("A `PriorityQueue` sempre entrega primeiro o elemento de MENOR prioridade. No Dijkstra, cada vértice entra com sua distância provisória; se ele sair da fila com uma distância maior que a registrada, é uma entrada velha e é ignorada."));
    }
}
