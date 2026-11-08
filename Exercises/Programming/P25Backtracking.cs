namespace EasyCreator.Exercises;

/// <summary>js-20 (backtracking): generating sequences, permutations and subsets by trying and undoing choices, N queens, grid paths; and tracing the order of a backtracking search.</summary>
[Exercises("js-20")]
public sealed class P25Backtracking : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(lv, r) : MakeOut(lv, r);
    }

    static void Bin(int n, string cur, bool noPair, List<string> o, string a, string b) { if (cur.Length == n) { o.Add(cur); return; } Bin(n, cur + a, noPair, o, a, b); if (!(noPair && cur.EndsWith(b))) Bin(n, cur + b, noPair, o, a, b); }
    static readonly (string, string)[] Syms = { ("0", "1"), ("a", "b"), ("N", "S"), ("x", "y"), (".", "#"), ("-", "+"), ("A", "B"), ("o", "x") };
    static void Perm(string s, string cur, bool[] used, List<string> o) { if (cur.Length == s.Length) { o.Add(cur); return; } for (int i = 0; i < s.Length; i++) if (!used[i]) { used[i] = true; Perm(s, cur + s[i], used, o); used[i] = false; } }
    static int Queens(int n, out string first, out long calls)
    {
        int count = 0; string f = ""; long c = 0; var cols = new int[n];
        bool Ok(int row, int col) { for (int i = 0; i < row; i++) if (cols[i] == col || Math.Abs(cols[i] - col) == row - i) return false; return true; }
        void Go(int row) { c++; if (row == n) { count++; if (f == "") f = string.Join(" ", cols.Select(x => x + 1)); return; } for (int col = 0; col < n; col++) if (Ok(row, col)) { cols[row] = col; Go(row + 1); } }
        Go(0); first = f; calls = c; return count;
    }

    static Exercise MakeCode(int lv, Random r)
    {
        switch (lv)
        {
            case 0:
            case 1:
            {
                bool noPair = lv == 1;
                var (sa, sb) = Ex.Pick(r, Syms);
                var tests = Pg.Tests(new[] { "1", "3", "2", Ex.Int(r, 1, 5).ToString(), Ex.Int(r, 2, 5).ToString() }, i => { var o = new List<string>(); Bin(int.Parse(i.Trim()), "", noPair, o, sa, sb); return o.Count + "\n" + string.Join("\n", o); });
                string guard = noPair ? $"\n  if (atual === \"\" || atual[atual.length - 1] !== \"{sb}\") gerar(atual + \"{sb}\");" : $"\n  gerar(atual + \"{sb}\");";
                string sol = $"const n = Number(prompt());\nconst saida = [];\nfunction gerar(atual) {{\n  if (atual.length === n) {{\n    saida.push(atual);\n    return;\n  }}\n  gerar(atual + \"{sa}\");{guard}\n}}\ngerar(\"\");\nconsole.log(saida.length);\nfor (const s of saida) console.log(s);";
                return Ex.Code(Pg.Prompt(noPair ? L.F("Leia `n` (1 ≤ n ≤ 5). Gere, por BACKTRACKING (recursão), todas as sequências de `n` símbolos `{0}`/`{1}` em que NÃO há dois `{1}` seguidos. Mostre primeiro a quantidade e depois as sequências, uma por linha, na ordem em que o backtracking as encontra (tentando sempre o `{0}` antes do `{1}`).", sa, sb)
                        : L.F("Leia `n` (1 ≤ n ≤ 5). Gere, por BACKTRACKING (recursão), todas as sequências de `n` símbolos `{0}`/`{1}`. Mostre primeiro a quantidade e depois as sequências, uma por linha, na ordem em que o backtracking as encontra (tentando sempre o `{0}` antes do `{1}`).", sa, sb), tests[0]),
                    "const n = Number(prompt());\n// escreva a função recursiva", sol, tests,
                    L.T("A função recebe a sequência montada até agora. Se já tem n dígitos, é uma solução. Senão, tenta acrescentar 0 e, se for permitido, acrescenta 1: cada tentativa é uma chamada recursiva, e ao voltar a sequência anterior continua intacta (o texto é recriado a cada chamada)."));
            }
            case 2:
            {
                string W() => new string("abcdefghijk".OrderBy(_ => r.Next()).Take(Ex.Int(r, 2, 4)).OrderBy(x => x).ToArray());
                var tests = Pg.Tests(new[] { "abc", "a", W(), W(), W() }, i => { var o = new List<string>(); var s = i.Trim(); Perm(s, "", new bool[s.Length], o); return o.Count + "\n" + string.Join("\n", o); });
                string sol = "const s = prompt();\nconst usado = new Array(s.length).fill(false);\nconst saida = [];\nfunction gerar(atual) {\n  if (atual.length === s.length) {\n    saida.push(atual);\n    return;\n  }\n  for (let i = 0; i < s.length; i++) {\n    if (usado[i]) continue;\n    usado[i] = true;\n    gerar(atual + s[i]);\n    usado[i] = false;\n  }\n}\ngerar(\"\");\nconsole.log(saida.length);\nfor (const p of saida) console.log(p);";
                return Ex.Code(Pg.Prompt(L.T("Leia uma palavra de 1 a 4 letras DIFERENTES, já em ordem alfabética. Gere, por backtracking, todas as PERMUTAÇÕES dessas letras (todas as ordens possíveis). Mostre a quantidade e depois as permutações, uma por linha, em ordem alfabética (escolhendo sempre a menor letra disponível primeiro)."), tests[0]),
                    "const s = prompt();\n// escreva a função recursiva", sol, tests,
                    L.T("Em cada posição tentamos todas as letras ainda não usadas: marcamos a letra como usada, chamamos a recursão para a posição seguinte e DESFAZEMOS a marca ao voltar (é o \"back\" de backtracking). Como as letras são tentadas em ordem, a saída já sai em ordem alfabética."));
            }
            default:
            {
                if (r.Next(4) == 0)
                {
                    var tests = Pg.Tests(new[] { "4", "1", "3", Ex.Int(r, 1, 7).ToString(), Ex.Int(r, 4, 7).ToString() }, i => { int k = Queens(int.Parse(i.Trim()), out var f, out _); return $"Soluções: {k}\n" + (k == 0 ? "Nenhuma solução" : "Colunas: " + f); });
                    string sol = "const n = Number(prompt());\nconst cols = new Array(n).fill(0);\nlet total = 0;\nlet primeira = null;\nfunction ok(linha, col) {\n  for (let i = 0; i < linha; i++) {\n    if (cols[i] === col || Math.abs(cols[i] - col) === linha - i) return false;\n  }\n  return true;\n}\nfunction colocar(linha) {\n  if (linha === n) {\n    total++;\n    if (primeira === null) primeira = cols.map((c) => c + 1).join(\" \");\n    return;\n  }\n  for (let col = 0; col < n; col++) {\n    if (ok(linha, col)) {\n      cols[linha] = col;\n      colocar(linha + 1);\n    }\n  }\n}\ncolocar(0);\nconsole.log(`Soluções: ${total}`);\nconsole.log(total === 0 ? \"Nenhuma solução\" : `Colunas: ${primeira}`);";
                    return Ex.Code(Pg.Prompt(L.T("Leia `n` (1 ≤ n ≤ 7). Coloque `n` rainhas num tabuleiro `n × n` sem que duas se ataquem (mesma coluna ou mesma diagonal; uma por linha). Mostre `Soluções: <quantas existem>` e, na linha seguinte, `Colunas: <coluna de cada linha, de 1 a n>` da PRIMEIRA solução encontrada (linha por linha, colunas tentadas da esquerda para a direita), ou `Nenhuma solução`."), tests[0]),
                        "const n = Number(prompt());\n// escreva o backtracking", sol, tests,
                        L.T("Colocamos uma rainha por linha. Para cada coluna testamos se a posição é segura em relação às rainhas já colocadas (mesma coluna ou |diferença de colunas| = diferença de linhas). Se for, seguimos para a próxima linha; ao voltar da recursão, tentamos a próxima coluna. Chegar à linha n é uma solução."));
                }
                else
                {
                    int Paths(int h, int w, HashSet<(int, int)> bad) { int Go(int i, int j) => i >= h || j >= w || bad.Contains((i, j)) ? 0 : (i == h - 1 && j == w - 1 ? 1 : Go(i + 1, j) + Go(i, j + 1)); return Go(0, 0); }
                    string Gi() { int h = Ex.Int(r, 2, 5), w = Ex.Int(r, 2, 5); var rows = Enumerable.Range(0, h).Select(i => new string(Enumerable.Range(0, w).Select(j => (i + j > 0 && !(i == h - 1 && j == w - 1) && r.Next(100) < 25) ? '#' : '.').ToArray())).ToArray(); return $"{h} {w}\n" + string.Join("\n", rows); }
                    var tests = Pg.Tests(new[] { "3 3\n...\n.#.\n...", Gi(), Gi(), Gi(), Gi() }, i => { var ls = i.TrimEnd('\n').Split('\n'); var hw = ls[0].Split(' ').Select(int.Parse).ToArray(); var bad = new HashSet<(int, int)>(); for (int a = 0; a < hw[0]; a++) for (int b = 0; b < hw[1]; b++) if (ls[a + 1][b] == '#') bad.Add((a, b)); return Paths(hw[0], hw[1], bad).ToString(); });
                    string sol = "const [h, w] = prompt().split(\" \").map(Number);\nconst g = [];\nfor (let i = 0; i < h; i++) g.push(prompt());\nfunction caminhos(i, j) {\n  if (i >= h || j >= w || g[i][j] === \"#\") return 0;\n  if (i === h - 1 && j === w - 1) return 1;\n  return caminhos(i + 1, j) + caminhos(i, j + 1);\n}\nconsole.log(caminhos(0, 0));";
                    return Ex.Code(Pg.Prompt(L.T("Leia `H` e `W` e depois `H` linhas com `W` caracteres: `.` livre e `#` obstáculo (o canto superior esquerdo e o inferior direito são sempre livres). Partindo do canto superior esquerdo, andando só para a DIREITA ou para BAIXO, mostre de quantos caminhos diferentes se chega ao canto inferior direito sem pisar em obstáculos."), tests[0]),
                        "const [h, w] = prompt().split(\" \").map(Number);\nconst g = [];\nfor (let i = 0; i < h; i++) g.push(prompt());\n// escreva a função recursiva", sol, tests,
                        L.T("Em cada célula há duas escolhas (direita ou baixo). Sair da grade ou pisar num obstáculo vale 0 caminhos; chegar ao destino vale 1; no resto, somamos os caminhos das duas escolhas. Para grades pequenas a recursão direta basta."));
                }
            }
        }
    }

    static Exercise MakeOut(int lv, Random r)
    {
        if (lv == 0)
        {
            int n = Ex.Int(r, 2, 3); var items = Pg.Words.OrderBy(_ => r.Next()).Take(n).ToArray(); var o = new List<string>();
            void Go(int i, string cur) { if (i == n) { o.Add(cur.Length == 0 ? "-" : cur.TrimStart(' ')); return; } Go(i + 1, cur); Go(i + 1, cur + " " + items[i]); }
            Go(0, "");
            string code = $"const itens = [{string.Join(", ", items.Select(x => $"\"{x}\""))}];\nfunction sub(i, atual) {{\n  if (i === itens.length) {{\n    console.log(atual === \"\" ? \"-\" : atual.trim());\n    return;\n  }}\n  sub(i + 1, atual);\n  sub(i + 1, atual + \" \" + itens[i]);\n}}\nsub(0, \"\");";
            string outp = string.Join("\n", o);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("Para cada item há duas ramificações: NÃO levar (chamada com `atual` igual) e levar (acrescenta o item). A primeira ramificação é explorada inteira antes da segunda, então o conjunto vazio sai primeiro e o último a sair é o que leva todos os itens."));
        }
        if (lv == 1 || lv == 3)
        {
            int n = Ex.Int(r, 3, 4), target = Ex.Int(r, 4, 9); var v = Pg.Arr(r, n, 1, 6); var o = new List<string>(); long calls = 0;
            void Go(int i, int sum, string path) { calls++; if (sum == target) { o.Add(path.Trim()); return; } if (i == n || sum > target) return; Go(i + 1, sum + v[i], path + " " + v[i]); Go(i + 1, sum, path); }
            Go(0, 0, "");
            string code = $"const v = [{string.Join(", ", v)}];\nconst alvo = {target};\nlet chamadas = 0;\nfunction busca(i, soma, caminho) {{\n  chamadas++;\n  if (soma === alvo) {{\n    console.log(caminho.trim());\n    return;\n  }}\n  if (i === v.length || soma > alvo) return;\n  busca(i + 1, soma + v[i], caminho + \" \" + v[i]);\n  busca(i + 1, soma, caminho);\n}}\nbusca(0, 0, \"\");\nconsole.log(chamadas);";
            o.Add(calls.ToString());
            string outp = string.Join("\n", o);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Cada chamada tenta usar v[i] primeiro e depois pular. Soluções aparecem quando a soma chega a {0}. O corte `soma > alvo` abandona caminhos sem esperança (poda). A última linha conta TODAS as chamadas, inclusive as que terminaram em corte ou sem solução.", target));
        }
        {
            int n = lv == 2 ? Ex.Int(r, 4, 5) : Ex.Int(r, 5, 6);
            int k = Queens(n, out var first, out var calls);
            string code = $"const n = {n};\nconst cols = new Array(n).fill(0);\nlet total = 0, chamadas = 0;\nfunction ok(linha, col) {{\n  for (let i = 0; i < linha; i++) {{\n    if (cols[i] === col || Math.abs(cols[i] - col) === linha - i) return false;\n  }}\n  return true;\n}}\nfunction colocar(linha) {{\n  chamadas++;\n  if (linha === n) {{\n    total++;\n    return;\n  }}\n  for (let col = 0; col < n; col++) {{\n    if (ok(linha, col)) {{\n      cols[linha] = col;\n      colocar(linha + 1);\n    }}\n  }}\n}}\ncolocar(0);\nconsole.log(total, chamadas);";
            string outp = $"{k} {calls}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Para n = {0} existem {1} soluções. `chamadas` conta cada vez que `colocar` é chamada, isto é, cada posição parcial válida explorada ({2} no total, incluindo a chamada inicial e as que chegam à linha n).", n, k, calls));
        }
    }
}
