namespace EasyCreator.Exercises;

/// <summary>js-18 (graphs: dependencies and topological sorting): in-degrees, available items, Kahn's algorithm, cycles and levels; and tracing Kahn's algorithm by hand.</summary>
[Exercises("js-18")]
public sealed class P21Topo : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(lv, r) : MakeOut(lv, r);
    }

    // names + edges "A B" (A is a prerequisite of B), no repeated pairs; optionally with a cycle.
    static (string[] names, List<(string a, string b)> edges) Make(Random r, int n, bool cycle)
    {
        var names = Pg.Words.OrderBy(_ => r.Next()).Take(n).ToArray();
        var order = names.OrderBy(_ => r.Next()).ToArray();
        var edges = new List<(string, string)>();
        for (int i = 0; i < n; i++) for (int j = i + 1; j < n; j++) if (r.Next(100) < 35) edges.Add((order[i], order[j]));
        if (edges.Count == 0) edges.Add((order[0], order[1]));
        if (cycle) { var (a, b) = edges[r.Next(edges.Count)]; if (!edges.Contains((b, a))) edges.Add((b, a)); }
        return (names, edges.OrderBy(_ => r.Next()).ToList());
    }
    static string Inp((string[] names, List<(string a, string b)> edges) g) => string.Join(" ", g.names) + "\n" + g.edges.Count + "\n" + string.Join("\n", g.edges.Select(e => e.a + " " + e.b));
    static (string[] names, List<(string, string)> edges) Parse(string i)
    {
        var ls = i.TrimEnd('\n').Split('\n'); int m = int.Parse(ls[1]);
        return (ls[0].Split(' '), ls.Skip(2).Take(m).Select(l => { var p = l.Split(' '); return (p[0], p[1]); }).ToList());
    }
    static Dictionary<string, int> Indeg((string[] names, List<(string, string)> edges) g) { var d = g.names.ToDictionary(x => x, _ => 0); foreach (var (_, b) in g.edges) d[b]++; return d; }
    static (List<string> order, List<List<string>> levels) Kahn((string[] names, List<(string, string)> edges) g)
    {
        var deg = Indeg(g); var order = new List<string>(); var levels = new List<List<string>>();
        var done = new HashSet<string>();
        while (true)
        {
            var avail = g.names.Where(x => !done.Contains(x) && deg[x] == 0).OrderBy(x => x, StringComparer.Ordinal).ToList();
            if (avail.Count == 0) break;
            levels.Add(avail);
            foreach (var x in avail) { done.Add(x); foreach (var (a, b) in g.edges) if (a == x) deg[b]--; }
        }
        // Kahn picking the alphabetically smallest available each time
        var deg2 = Indeg(g); var used = new HashSet<string>();
        while (true)
        {
            var c = g.names.Where(x => !used.Contains(x) && deg2[x] == 0).OrderBy(x => x, StringComparer.Ordinal).FirstOrDefault();
            if (c == null) break;
            used.Add(c); order.Add(c); foreach (var (a, b) in g.edges) if (a == c) deg2[b]--;
        }
        return (order, levels);
    }

    static Exercise MakeCode(int lv, Random r)
    {
        string rd = "const nomes = prompt().split(\" \");\nconst m = Number(prompt());\nconst arestas = [];\nfor (let i = 0; i < m; i++) arestas.push(prompt().split(\" \"));";
        string starter = rd + "\n// escreva o resto do programa aqui";
        string Gi(bool cyc = false) => Inp(Make(r, Ex.Int(r, 4, 7), cyc));
        string deg = "const grau = {};\nfor (const n of nomes) grau[n] = 0;\nfor (const [a, b] of arestas) grau[b]++;";
        switch (lv)
        {
            case 0:
            {
                var tests = Pg.Tests(new[] { "b a c\n2\na b\na c", Gi(), Gi(), Gi() }, i => { var g = Parse(i); var d = Indeg(g); return string.Join("\n", g.names.OrderBy(x => x, StringComparer.Ordinal).Select(x => $"{x}: {d[x]}")); });
                string sol = rd + "\n" + deg + "\nfor (const n of [...nomes].sort()) console.log(`${n}: ${grau[n]}`);";
                return Ex.Code(Pg.Prompt(L.T("Leia uma linha com nomes de disciplinas separados por espaço, depois um inteiro `m` e `m` linhas `<A> <B>`, cada uma dizendo que `A` é pré-requisito de `B` (não há linhas repetidas). Para cada disciplina, em ordem alfabética, mostre uma linha `<nome>: <quantos pré-requisitos diretos ela tem>`."), tests[0]), starter, sol, tests,
                    L.T("O número de pré-requisitos diretos é o GRAU DE ENTRADA do vértice: começamos todos com 0 e, para cada aresta A → B, somamos 1 ao grau de B."));
            }
            case 1:
            {
                var tests = Pg.Tests(new[] { "b a c\n2\na b\na c", Gi(), Gi(), Gi() }, i => { var g = Parse(i); var d = Indeg(g); return string.Join(" ", g.names.Where(x => d[x] == 0).OrderBy(x => x, StringComparer.Ordinal)); });
                string sol = rd + "\n" + deg + "\nconsole.log(nomes.filter((n) => grau[n] === 0).sort().join(\" \"));";
                return Ex.Code(Pg.Prompt(L.T("Leia uma linha de disciplinas, `m` e `m` linhas `<A> <B>` (A é pré-requisito de B, sem linhas repetidas). Mostre, numa linha separada por espaço e em ordem alfabética, as disciplinas que podem ser cursadas AGORA, isto é, as que não têm pré-requisito nenhum."), tests[0]), starter, sol, tests,
                    L.T("As disciplinas disponíveis são as de grau de entrada 0. Calculamos os graus e filtramos; o sort() comum já ordena os nomes como texto."));
            }
            case 2:
            {
                var tests = Pg.Tests(new[] { "b a c\n2\na b\na c", Gi(true), Gi(), Gi(), Gi(), Gi(true) }, i => { var g = Parse(i); var (o, _) = Kahn(g); return o.Count == g.names.Length ? string.Join(" ", o) : "Ciclo"; });
                string sol = rd + "\n" + deg + "\nconst ordem = [];\nconst feitas = new Set();\nwhile (true) {\n  const disp = nomes.filter((n) => !feitas.has(n) && grau[n] === 0).sort();\n  if (disp.length === 0) break;\n  const c = disp[0];\n  feitas.add(c);\n  ordem.push(c);\n  for (const [a, b] of arestas) if (a === c) grau[b]--;\n}\nconsole.log(ordem.length === nomes.length ? ordem.join(\" \") : \"Ciclo\");";
                return Ex.Code(Pg.Prompt(L.T("Leia uma linha de disciplinas, `m` e `m` linhas `<A> <B>` (A é pré-requisito de B). Escolha uma ordem para cursar TODAS: a cada passo, entre as que estão disponíveis (todos os pré-requisitos já cursados), cursar a menor em ordem alfabética (comparação comum de strings). Mostre a ordem numa linha separada por espaço, ou `Ciclo` se for impossível (há pré-requisitos que dependem uns dos outros)."), tests[0]), starter, sol, tests,
                    L.T("É o algoritmo de Kahn: repetimos \"pegar a menor disciplina de grau 0, cursá-la e diminuir 1 no grau de quem depende dela\". Se em algum momento não há nenhuma de grau 0 mas ainda restam disciplinas, elas formam um ciclo."));
            }
            default:
            {
                var tests = Pg.Tests(new[] { "b a c d\n3\na b\na c\nc d", Gi(true), Gi(), Gi(), Gi(), Gi() }, i => { var g = Parse(i); var (o, lv2) = Kahn(g); return o.Count == g.names.Length ? $"Semestres: {lv2.Count}" : "Ciclo"; });
                string sol = rd + "\n" + deg + "\nlet semestres = 0, cursadas = 0;\nlet atuais = nomes.filter((n) => grau[n] === 0);\nwhile (atuais.length > 0) {\n  semestres++;\n  cursadas += atuais.length;\n  const prox = [];\n  for (const c of atuais) {\n    for (const [a, b] of arestas) {\n      if (a === c && --grau[b] === 0) prox.push(b);\n    }\n  }\n  atuais = prox;\n}\nconsole.log(cursadas === nomes.length ? `Semestres: ${semestres}` : \"Ciclo\");";
                return Ex.Code(Pg.Prompt(L.T("Leia uma linha de disciplinas, `m` e `m` linhas `<A> <B>` (A é pré-requisito de B). Em cada semestre dá para cursar TODAS as disciplinas disponíveis ao mesmo tempo (as que já tiveram os pré-requisitos cursados em semestres ANTERIORES). Mostre `Semestres: <menor número de semestres para cursar tudo>` ou `Ciclo` se for impossível."), tests[0]), starter, sol, tests,
                    L.T("Truque: Kahn por CAMADAS. Cada camada é o conjunto de disciplinas de grau 0 naquele momento; cursá-las todas libera a próxima camada (as que chegam a grau 0). O número de camadas é o número mínimo de semestres. Se no fim sobrou disciplina, havia ciclo."));
            }
        }
    }

    static Exercise MakeOut(int lv, Random r)
    {
        bool cyc = lv == 3 && r.Next(2) == 0;
        var g = Make(r, Ex.Int(r, 4, 6), cyc);
        string names = string.Join(", ", g.names.Select(x => $"\"{x}\"")), edges = string.Join(", ", g.edges.Select(e => $"[\"{e.a}\", \"{e.b}\"]"));
        string head = $"const nomes = [{names}];\nconst arestas = [{edges}];\nconst grau = {{}};\nfor (const n of nomes) grau[n] = 0;\nfor (const [a, b] of arestas) grau[b]++;\n";
        var (order, levels) = Kahn(g);
        if (lv == 0)
        {
            var deg = Indeg(g); string q = Ex.Pick(r, g.names);
            string code = head + $"console.log(grau[\"{q}\"]);\nconsole.log(nomes.filter((n) => grau[n] === 0).length);\nconsole.log(Math.max(...Object.values(grau)));";
            string outp = $"{deg[q]}\n{deg.Values.Count(x => x == 0)}\n{deg.Values.Max()}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("Cada aresta [A, B] soma 1 ao grau de B. As linhas mostram: o grau da disciplina pedida, quantas têm grau 0 e o maior grau."));
        }
        if (lv == 1)
        {
            string code = head + "const ordem = [];\nconst feitas = new Set();\nwhile (true) {\n  const disp = nomes.filter((n) => !feitas.has(n) && grau[n] === 0).sort();\n  if (disp.length === 0) break;\n  const c = disp[0];\n  feitas.add(c);\n  ordem.push(c);\n  for (const [a, b] of arestas) if (a === c) grau[b]--;\n}\nconsole.log(ordem.join(\" \"));";
            string outp = string.Join(" ", order);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("A cada volta, entre os de grau 0 ainda não feitos, escolha o MENOR em ordem alfabética, escreva-o e diminua 1 no grau de quem dependia dele. Repita até não haver nenhum disponível."));
        }
        {
            string code = head + "let atuais = nomes.filter((n) => grau[n] === 0);\nlet semestres = 0, total = 0;\nwhile (atuais.length > 0) {\n  semestres++;\n  total += atuais.length;\n  const prox = [];\n  for (const c of atuais) {\n    for (const [a, b] of arestas) {\n      if (a === c && --grau[b] === 0) prox.push(b);\n    }\n  }\n  atuais = prox;\n}\nconsole.log(semestres, total);\nconsole.log(total === nomes.length ? \"completo\" : \"sobrou\");";
            string outp = $"{levels.Count} {levels.Sum(l => l.Count)}\n{(levels.Sum(l => l.Count) == g.names.Length ? "completo" : "sobrou")}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("Cada volta do laço é uma camada: todas as disciplinas de grau 0 naquele momento. `total` soma quantas foram cursadas; se ficou menor que o número de disciplinas, as que sobraram dependem umas das outras (ciclo)."));
        }
    }
}
