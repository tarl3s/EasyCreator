using System.Text;

namespace EasyCreator.Exercises;

/// <summary>C lessons 7-10: arrays and strings, structs, recursion, sorting and binary search.</summary>
[Exercises("c-07", "c-08", "c-09", "c-10")]
public sealed class CData : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r) => lesson.Id switch
    {
        "c-07" => r.Next(2) == 0 ? Write07(d, r) : Predict07(d, r),
        "c-08" => r.Next(2) == 0 ? Write08(d, r) : Predict08(d, r),
        "c-09" => r.Next(2) == 0 ? Write09(d, r) : Predict09(d, r),
        _ => r.Next(2) == 0 ? Write10(d, r) : Predict10(d, r),
    };

    static readonly string[] Words = { "banana", "radar", "computador", "janela", "arara", "sol", "estudar", "programa", "ovo", "laranja", "teclado", "mouse", "nuvem", "arquivo", "ana", "reviver", "pixel", "memoria", "vetor", "chave" };
    static readonly string[] Names = { "Ana", "Bruno", "Carla", "Davi", "Eva", "Fabio", "Gabi", "Hugo", "Iris", "Joao", "Lia", "Mauro" };

    static string Str(string body, string inc = "#include <string.h>\n") => PK.CMain(body, inc);

    // ---------- c-07 ----------
    static Exercise Write07(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Leia uma palavra (só letras minúsculas, sem espaços) e mostre `Tamanho: <letras>` e `Vogais: <quantidade de a, e, i, o, u>`.");
                sol = PK.CMain("    char p[101];\n    int i, v = 0;\n    scanf(\"%s\", p);\n    for (i = 0; p[i] != '\\0'; i++) {\n        if (p[i] == 'a' || p[i] == 'e' || p[i] == 'i' || p[i] == 'o' || p[i] == 'u') {\n            v++;\n        }\n    }\n    printf(\"Tamanho: %d\\n\", i);\n    printf(\"Vogais: %d\\n\", v);\n");
                gen = g => PK.Pick(g, Words) + "\n"; oracle = i => { var w = i.Trim(); return $"Tamanho: {w.Length}\nVogais: {w.Count(c => "aeiou".Contains(c))}"; };
                why = L.T("Uma string em C termina em `'\\0'`: percorra até achar esse caractere; o `i` final é o tamanho.");
                break;
            case Difficulty.Medium:
                st = L.T("Leia uma palavra e, depois, uma letra (na mesma linha, separadas por espaço). Mostre `Ocorrencias: <quantas vezes a letra aparece>`, `Primeira: <índice da primeira, ou -1>` e `Ultima: <índice da última, ou -1>` (índices começam em 0).");
                sol = PK.CMain("    char p[101];\n    char c;\n    int i, n = 0, primeira = -1, ultima = -1;\n    scanf(\"%s %c\", p, &c);\n    for (i = 0; p[i] != '\\0'; i++) {\n        if (p[i] == c) {\n            n++;\n            if (primeira < 0) {\n                primeira = i;\n            }\n            ultima = i;\n        }\n    }\n    printf(\"Ocorrencias: %d\\n\", n);\n    printf(\"Primeira: %d\\n\", primeira);\n    printf(\"Ultima: %d\\n\", ultima);\n");
                gen = g => { var w = PK.Pick(g, Words); char c = g.Next(4) == 0 ? (char)('a' + g.Next(26)) : w[g.Next(w.Length)]; return $"{w} {c}\n"; };
                oracle = i => { var p = i.Trim().Split(' '); int n = p[0].Count(c => c == p[1][0]); return $"Ocorrencias: {n}\nPrimeira: {p[0].IndexOf(p[1][0])}\nUltima: {p[0].LastIndexOf(p[1][0])}"; };
                why = L.T("Um único laço conta, guarda a primeira posição só uma vez (enquanto ainda é -1) e atualiza a última a cada acerto.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia `n` (1 a 20) e depois `n` inteiros. Mostre `Invertido: <os números de trás para frente, separados por espaço>`, `Maior: <valor> na posicao <índice da primeira ocorrência>` e `Crescente: sim` se estiverem em ordem estritamente crescente (senão `Crescente: nao`).");
                sol = PK.CMain("    int v[20];\n    int n, i, m = 0, cresc = 1;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &v[i]);\n    }\n    printf(\"Invertido:\");\n    for (i = n - 1; i >= 0; i--) {\n        printf(\" %d\", v[i]);\n    }\n    printf(\"\\n\");\n    for (i = 1; i < n; i++) {\n        if (v[i] > v[m]) {\n            m = i;\n        }\n        if (v[i] <= v[i - 1]) {\n            cresc = 0;\n        }\n    }\n    printf(\"Maior: %d na posicao %d\\n\", v[m], m);\n    printf(\"Crescente: %s\\n\", cresc ? \"sim\" : \"nao\");\n");
                gen = g => { int n = g.Next(1, 9); var v = g.Next(3) == 0 ? PK.Distinct(g, n, 1, 30).OrderBy(x => x).ToList() : PK.Rand(g, n, -5, 12); return $"{n}\n{PK.Join(v)}\n"; };
                oracle = i => { var v = PK.Ints(i).Skip(1).ToArray(); int m = 0; bool c = true; for (int j = 1; j < v.Length; j++) { if (v[j] > v[m]) m = j; if (v[j] <= v[j - 1]) c = false; } return $"Invertido: {PK.Join(v.Reverse())}\nMaior: {v[m]} na posicao {m}\nCrescente: {(c ? "sim" : "nao")}"; };
                why = L.T("Para inverter basta imprimir do último índice ao zero. O maior só troca quando o novo valor é estritamente maior, por isso fica a primeira ocorrência.");
                break;
            default:
                st = L.T("Leia um deslocamento `k` (1 a 25) e uma palavra de letras minúsculas (um por linha). Aplique a cifra de César: cada letra avança `k` posições no alfabeto, voltando ao `a` depois do `z`. Mostre `Cifrado: <palavra cifrada>` e `Voltas: <quantas letras passaram do z e voltaram ao começo>`.");
                sol = PK.CMain("    char p[101];\n    int k, i, v = 0, pos;\n    scanf(\"%d\", &k);\n    scanf(\"%s\", p);\n    for (i = 0; p[i] != '\\0'; i++) {\n        pos = p[i] - 'a' + k;\n        if (pos >= 26) {\n            v++;\n        }\n        p[i] = 'a' + pos % 26;\n    }\n    printf(\"Cifrado: %s\\n\", p);\n    printf(\"Voltas: %d\\n\", v);\n");
                gen = g => $"{g.Next(1, 26)}\n{PK.Pick(g, Words)}\n";
                oracle = i => { var t = i.Split('\n', StringSplitOptions.RemoveEmptyEntries); int k = int.Parse(t[0]); var sb = new StringBuilder(); int v = 0; foreach (char c in t[1]) { int pos = c - 'a' + k; if (pos >= 26) v++; sb.Append((char)('a' + pos % 26)); } return $"Cifrado: {sb}\nVoltas: {v}"; };
                why = L.T("A posição da letra é `p[i] - 'a'`. Somando `k` e tirando o resto por 26 o alfabeto \"dá a volta\"; passou de 25 significa que voltou.");
                break;
        }
        return PK.Write(st, PK.CStarter, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict07(Difficulty d, Random r)
    {
        string w = PK.Pick(r, Words); int k = r.Next(1, 4); var o = new List<string>(); string code; int n = r.Next(4, 7);
        switch (d)
        {
            case Difficulty.Easy:
                code = Str($"    char s[] = \"{w}\";\n    int i;\n    for (i = 0; s[i] != '\\0'; i++) {{\n        if (s[i] == '{w[0]}') {{\n            s[i] = s[i] - 32;\n        }}\n    }}\n    printf(\"%s %d\\n\", s, (int)strlen(s));\n");
                o.Add(w.Replace(w[0].ToString(), char.ToUpper(w[0]).ToString()) + " " + w.Length);
                break;
            case Difficulty.Medium:
            {
                var v = PK.Rand(r, n, 1, 9);
                code = PK.CMain($"    int v[{n}] = {{{PK.Join(v, ", ")}}};\n    int i, s = 0;\n    for (i = 0; i < {n}; i++) {{\n        if (i % 2 == 0) {{\n            s += v[i];\n        }} else {{\n            v[i] = v[i] * {k};\n        }}\n    }}\n    printf(\"%d\\n\", s);\n    for (i = 0; i < {n}; i++) {{\n        printf(\"%d \", v[i]);\n    }}\n    printf(\"\\n\");\n");
                int s = 0; for (int i = 0; i < n; i++) { if (i % 2 == 0) s += v[i]; else v[i] *= k; }
                o.Add(s.ToString()); o.Add(PK.Join(v) + " ");
                break;
            }
            case Difficulty.Hard:
            {
                var v = PK.Rand(r, n, 1, 20);
                code = PK.CMain($"    int v[{n}] = {{{PK.Join(v, ", ")}}};\n    int i = 0, j = {n - 1}, t;\n    while (i < j) {{\n        t = v[i];\n        v[i] = v[j];\n        v[j] = t;\n        i += 1;\n        j -= 1;\n    }}\n    for (i = 0; i < {n}; i++) {{\n        printf(\"%d\", v[i]);\n        if (i < {n - 1}) {{\n            printf(\"-\");\n        }}\n    }}\n    printf(\"\\n%d\\n\", v[0] + v[{n - 1}]);\n");
                var rv = Enumerable.Reverse(v).ToList(); o.Add(PK.Join(rv, "-")); o.Add((rv[0] + rv[n - 1]).ToString());
                break;
            }
            default:
            {
                string s = w.Length >= 6 ? w : w + "xyz";
                int cut = r.Next(2, 5);
                code = Str($"    char s[20] = \"{s}\";\n    char t[20];\n    int i, n = strlen(s);\n    for (i = 0; i < n; i++) {{\n        t[i] = s[n - 1 - i];\n    }}\n    t[n] = '\\0';\n    s[{cut}] = '\\0';\n    printf(\"%s|%s|%d\\n\", s, t, (int)strlen(s));\n    printf(\"%d\\n\", strcmp(s, \"{s[..cut]}\") == 0);\n");
                o.Add($"{s[..cut]}|{new string(s.Reverse().ToArray())}|{cut}"); o.Add("1");
                break;
            }
        }
        return PK.Predict(d <= Difficulty.Easy ? L.F("O texto usado no programa tem a palavra \"{0}\".", w) : L.F("O programa percorre um vetor ou texto de {0} posições.", n), code, string.Join("\n", o),
            L.T("Os índices de vetores e textos começam em 0. Um `char` é um número (maiúscula = minúscula − 32). Escrever `'\\0'` numa posição encurta a string ali. Acompanhe cada volta anotando o vetor."));
    }

    // ---------- c-08 ----------
    static Exercise Write08(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Use `typedef struct { int x; int y; } Ponto;`. Leia dois pontos (`x y` cada um, em linhas separadas) e mostre `Soma: (<x1+x2>, <y1+y2>)` e `Distancia ao quadrado: <(x2-x1)² + (y2-y1)²>`.");
                sol = PK.CMain("    Ponto a, b;\n    scanf(\"%d %d\", &a.x, &a.y);\n    scanf(\"%d %d\", &b.x, &b.y);\n    printf(\"Soma: (%d, %d)\\n\", a.x + b.x, a.y + b.y);\n    printf(\"Distancia ao quadrado: %d\\n\", (b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y));\n").Replace("int main", "typedef struct {\n    int x;\n    int y;\n} Ponto;\n\nint main");
                gen = g => $"{g.Next(-9, 10)} {g.Next(-9, 10)}\n{g.Next(-9, 10)} {g.Next(-9, 10)}\n";
                oracle = i => { var v = PK.Ints(i); return $"Soma: ({v[0] + v[2]}, {v[1] + v[3]})\nDistancia ao quadrado: {(v[2] - v[0]) * (v[2] - v[0]) + (v[3] - v[1]) * (v[3] - v[1])}"; };
                why = L.T("Os campos de uma struct se acessam com `.`; `scanf` precisa do endereço de cada campo (`&a.x`).");
                break;
            case Difficulty.Medium:
                st = L.T("Leia `n` (1 a 10) e, para cada produto, uma linha `nome preço quantidade` (preço e quantidade inteiros). Guarde-os em um vetor de structs e mostre uma linha `<nome>: <preço*quantidade>` por produto, depois `Total: <soma>` e `Maior: <nome do produto de maior valor; empate: o primeiro>`.");
                sol = "#include <stdio.h>\n\ntypedef struct {\n    char nome[30];\n    int preco;\n    int qtd;\n} Produto;\n\nint main(void) {\n    Produto p[10];\n    int n, i, total = 0, m = 0;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%s %d %d\", p[i].nome, &p[i].preco, &p[i].qtd);\n        printf(\"%s: %d\\n\", p[i].nome, p[i].preco * p[i].qtd);\n        total += p[i].preco * p[i].qtd;\n        if (p[i].preco * p[i].qtd > p[m].preco * p[m].qtd) {\n            m = i;\n        }\n    }\n    printf(\"Total: %d\\n\", total);\n    printf(\"Maior: %s\\n\", p[m].nome);\n    return 0;\n}\n";
                gen = g => { int n = g.Next(1, 7); var nm = Names.OrderBy(_ => g.Next()).Take(n).ToList(); return n + "\n" + string.Join("\n", nm.Select(x => $"{x} {g.Next(1, 40)} {g.Next(1, 9)}")) + "\n"; };
                oracle = i => { var t = i.Split(new[] { '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries); int n = int.Parse(t[0]); var sb = new List<string>(); int tot = 0, best = -1; string bn = ""; for (int j = 0; j < n; j++) { int v = int.Parse(t[2 + 3 * j]) * int.Parse(t[3 + 3 * j]); sb.Add($"{t[1 + 3 * j]}: {v}"); tot += v; if (v > best) { best = v; bn = t[1 + 3 * j]; } } sb.Add($"Total: {tot}"); sb.Add($"Maior: {bn}"); return string.Join("\n", sb); };
                why = L.T("Um vetor de structs é lido em laço (`p[i].campo`). Para o maior, guarde o índice e só troque quando o valor for estritamente maior.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia `n` (1 a 8) e `n` jogadores (`nome pontos`, um por linha). Mostre o ranking em ordem decrescente de pontos, uma linha `<posição>. <nome> - <pontos>`; quem empatar fica na ordem em que foi lido. Ordene você mesmo o vetor de structs (por exemplo, com insertion sort).");
                sol = "#include <stdio.h>\n\ntypedef struct {\n    char nome[30];\n    int pontos;\n} Jogador;\n\nint main(void) {\n    Jogador j[8];\n    Jogador chave;\n    int n, i, k;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%s %d\", j[i].nome, &j[i].pontos);\n    }\n    for (i = 1; i < n; i++) {\n        chave = j[i];\n        k = i - 1;\n        while (k >= 0 && j[k].pontos < chave.pontos) {\n            j[k + 1] = j[k];\n            k--;\n        }\n        j[k + 1] = chave;\n    }\n    for (i = 0; i < n; i++) {\n        printf(\"%d. %s - %d\\n\", i + 1, j[i].nome, j[i].pontos);\n    }\n    return 0;\n}\n";
                gen = g => { int n = g.Next(2, 8); var nm = Names.OrderBy(_ => g.Next()).Take(n).ToList(); return n + "\n" + string.Join("\n", nm.Select(x => $"{x} {g.Next(0, 12) * 10}")) + "\n"; };
                oracle = i => { var t = i.Split(new[] { '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries); int n = int.Parse(t[0]); var l = Enumerable.Range(0, n).Select(j => (name: t[1 + 2 * j], pts: int.Parse(t[2 + 2 * j]))).ToList(); var s = l.Select((x, idx) => (x, idx)).OrderByDescending(a => a.x.pts).ThenBy(a => a.idx).Select(a => a.x).ToList(); return string.Join("\n", s.Select((x, j) => $"{j + 1}. {x.name} - {x.pts}")); };
                why = L.T("Structs podem ser copiadas inteiras com `=`, o que facilita ordenar: a `chave` guarda o elemento e os maiores deslizam. Trocar só quando `pontos <` estritamente menor mantém a ordem dos empates.");
                break;
            default:
                st = L.T("Leia `n` (1 a 8) e `n` retângulos (`x y largura altura`, inteiros positivos; (x, y) é o canto de cima à esquerda). Mostre `Area total: <soma das áreas>`, `Maior: <número (começando em 1) do retângulo de maior área; empate: o primeiro>` e `Sobrepostos: <quantos pares (i, j) com i < j se sobrepõem com área maior que zero>`. Só encostar nas bordas não conta.");
                sol = "#include <stdio.h>\n\ntypedef struct {\n    int x;\n    int y;\n    int w;\n    int h;\n} Ret;\n\nint sobrepoe(Ret a, Ret b) {\n    return a.x < b.x + b.w && b.x < a.x + a.w && a.y < b.y + b.h && b.y < a.y + a.h;\n}\n\nint main(void) {\n    Ret r[8];\n    int n, i, j, total = 0, m = 0, pares = 0;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%d %d %d %d\", &r[i].x, &r[i].y, &r[i].w, &r[i].h);\n        total += r[i].w * r[i].h;\n        if (r[i].w * r[i].h > r[m].w * r[m].h) {\n            m = i;\n        }\n    }\n    for (i = 0; i < n; i++) {\n        for (j = i + 1; j < n; j++) {\n            if (sobrepoe(r[i], r[j])) {\n                pares++;\n            }\n        }\n    }\n    printf(\"Area total: %d\\n\", total);\n    printf(\"Maior: %d\\n\", m + 1);\n    printf(\"Sobrepostos: %d\\n\", pares);\n    return 0;\n}\n";
                gen = g => { int n = g.Next(2, 7); return n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => $"{g.Next(0, 10)} {g.Next(0, 10)} {g.Next(1, 6)} {g.Next(1, 6)}")) + "\n"; };
                oracle = i => { var v = PK.Ints(i); int n = v[0]; int tot = 0, m = 0, pairs = 0; int[] A(int k) => v.Skip(1 + 4 * k).Take(4).ToArray(); for (int a = 0; a < n; a++) { var q = A(a); tot += q[2] * q[3]; if (q[2] * q[3] > A(m)[2] * A(m)[3]) m = a; for (int b = a + 1; b < n; b++) { var s = A(b); if (q[0] < s[0] + s[2] && s[0] < q[0] + q[2] && q[1] < s[1] + s[3] && s[1] < q[1] + q[3]) pairs++; } } return $"Area total: {tot}\nMaior: {m + 1}\nSobrepostos: {pairs}"; };
                why = L.T("Dois retângulos se sobrepõem quando seus intervalos em x se cruzam E os intervalos em y também, com desigualdades estritas (encostar não conta). Compare todos os pares com dois laços.");
                break;
        }
        return PK.Write(st, PK.CStarter, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict08(Difficulty d, Random r)
    {
        int a = r.Next(1, 9), b = r.Next(1, 9), k = r.Next(2, 6); var o = new List<string>(); string code;
        string def = "typedef struct {\n    int x;\n    int y;\n} P;\n\n";
        switch (d)
        {
            case Difficulty.Easy:
                code = PK.CHead + def + $"int main(void) {{\n    P p = {{{a}, {b}}};\n    P q = p;\n    q.x = q.x + {k};\n    q.y = q.x * 2;\n    printf(\"%d %d\\n\", p.x, p.y);\n    printf(\"%d %d\\n\", q.x, q.y);\n    return 0;\n}}\n";
                o.Add($"{a} {b}"); o.Add($"{a + k} {(a + k) * 2}");
                break;
            case Difficulty.Medium:
                code = PK.CHead + def + $"void mover(P p, int d) {{\n    p.x = p.x + d;\n}}\n\nvoid empurrar(P *p, int d) {{\n    p->x = p->x + d;\n    p->y = p->y - d;\n}}\n\nint main(void) {{\n    P p = {{{a}, {b}}};\n    mover(p, {k});\n    printf(\"%d %d\\n\", p.x, p.y);\n    empurrar(&p, {k});\n    printf(\"%d %d\\n\", p.x, p.y);\n    return 0;\n}}\n";
                o.Add($"{a} {b}"); o.Add($"{a + k} {b - k}");
                break;
            case Difficulty.Hard:
            {
                var xs = PK.Rand(r, 3, 1, 9); var ys = PK.Rand(r, 3, 1, 9);
                code = PK.CHead + def + $"int main(void) {{\n    P v[3] = {{{{{xs[0]}, {ys[0]}}}, {{{xs[1]}, {ys[1]}}}, {{{xs[2]}, {ys[2]}}}}};\n    P *p = v;\n    int i, s = 0;\n    for (i = 0; i < 3; i++) {{\n        p->x = p->x + p->y;\n        s += p->x;\n        p++;\n    }}\n    printf(\"%d %d %d\\n\", v[0].x, v[1].x, v[2].x);\n    printf(\"%d\\n\", s);\n    return 0;\n}}\n";
                var nx = new int[3]; for (int i = 0; i < 3; i++) nx[i] = xs[i] + ys[i];
                o.Add($"{nx[0]} {nx[1]} {nx[2]}"); o.Add(nx.Sum().ToString());
                break;
            }
            default:
            {
                var xs = PK.Rand(r, 3, 1, 9);
                code = PK.CHead + "typedef struct {\n    int v;\n    int n;\n} C;\n\n" + $"void passo(C *c, int d) {{\n    c->v = c->v * d + c->n;\n    c->n++;\n}}\n\nC maior(C a, C b) {{\n    return a.v >= b.v ? a : b;\n}}\n\nint main(void) {{\n    C c[3] = {{{{{xs[0]}, 0}}, {{{xs[1]}, 1}}, {{{xs[2]}, 2}}}};\n    C m;\n    int i;\n    for (i = 0; i < 3; i++) {{\n        passo(&c[i], {k});\n    }}\n    m = c[0];\n    for (i = 1; i < 3; i++) {{\n        m = maior(m, c[i]);\n    }}\n    m.n = m.n * 10;\n    printf(\"%d %d %d\\n\", c[0].v, c[1].v, c[2].v);\n    printf(\"%d %d\\n\", m.v, m.n);\n    return 0;\n}}\n";
                var cv = new int[3]; var cn = new int[3]; for (int i = 0; i < 3; i++) { cv[i] = xs[i] * k + i; cn[i] = i + 1; }
                int mi = 0; for (int i = 1; i < 3; i++) if (cv[i] > cv[mi]) mi = i; // maior(m, c[i]) keeps the first on ties
                o.Add($"{cv[0]} {cv[1]} {cv[2]}"); o.Add($"{cv[mi]} {cn[mi] * 10}");
                break;
            }
        }
        return PK.Predict(L.F("O programa usa uma struct com dois números e começa com {0} e {1}.", a, b), code, string.Join("\n", o),
            L.T("Atribuir uma struct (`q = p`) copia todos os campos: mudar a cópia não muda o original. Passar a struct para uma função também copia; só um ponteiro (`->`) altera o original. `p++` num ponteiro para struct avança para o próximo elemento do vetor."));
    }

    // ---------- c-09 ----------
    static Exercise Write09(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Leia `n` (0 a 12). Escreva duas funções **recursivas**: `int soma(int n)` (1 + 2 + ... + n, com soma(0) = 0) e `long fatorial(int n)`. Mostre `Soma: <soma(n)>` e `Fatorial: <n!>`.");
                sol = "#include <stdio.h>\n\nint soma(int n) {\n    if (n == 0) {\n        return 0;\n    }\n    return n + soma(n - 1);\n}\n\nlong fatorial(int n) {\n    if (n <= 1) {\n        return 1;\n    }\n    return n * fatorial(n - 1);\n}\n\nint main(void) {\n    int n;\n    scanf(\"%d\", &n);\n    printf(\"Soma: %d\\n\", soma(n));\n    printf(\"Fatorial: %ld\\n\", fatorial(n));\n    return 0;\n}\n";
                gen = g => $"{g.Next(0, 13)}\n"; oracle = i => { int n = PK.Ints(i)[0]; long f = 1; for (int j = 2; j <= n; j++) f *= j; return $"Soma: {n * (n + 1) / 2}\nFatorial: {f}"; };
                why = L.T("Toda recursão tem um caso base (aqui n = 0 ou 1) e um passo que chama a função com um problema menor (`n - 1`).");
                break;
            case Difficulty.Medium:
                st = L.T("Leia `n` (0 a 999999) e `b` e `e` (base e expoente, `e` de 0 a 8, base de 1 a 9). Escreva **recursivamente** `int somaDigitos(int n)` e `long potencia(int b, int e)`. Mostre `Digitos: <soma dos dígitos de n>` e `Potencia: <b elevado a e>`. A potência deve usar a ideia `b^e = b * b^(e-1)`.");
                sol = "#include <stdio.h>\n\nint somaDigitos(int n) {\n    if (n == 0) {\n        return 0;\n    }\n    return n % 10 + somaDigitos(n / 10);\n}\n\nlong potencia(int b, int e) {\n    if (e == 0) {\n        return 1;\n    }\n    return b * potencia(b, e - 1);\n}\n\nint main(void) {\n    int n, b, e;\n    scanf(\"%d\", &n);\n    scanf(\"%d %d\", &b, &e);\n    printf(\"Digitos: %d\\n\", somaDigitos(n));\n    printf(\"Potencia: %ld\\n\", potencia(b, e));\n    return 0;\n}\n";
                gen = g => $"{g.Next(0, 1000000)}\n{g.Next(1, 10)} {g.Next(0, 9)}\n";
                oracle = i => { var v = PK.Ints(i); int n = v[0], s = 0; while (n > 0) { s += n % 10; n /= 10; } long p = 1; for (int j = 0; j < v[2]; j++) p *= v[1]; return $"Digitos: {s}\nPotencia: {p}"; };
                why = L.T("`n % 10` é o último dígito e `n / 10` é o resto do número, um problema menor. Para a potência, `b^0 = 1` é o caso base.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia `n` (1 a 4), o número de discos da Torre de Hanói, todos no pino A. Escreva uma função recursiva `hanoi(int n, char de, char para, char aux)` que mostra cada movimento no formato `Disco <d>: <de> -> <para>` (disco 1 é o menor) para levar todos ao pino C. Depois mostre `Movimentos: <total>`.");
                sol = "#include <stdio.h>\n\nint total = 0;\n\nvoid hanoi(int n, char de, char para, char aux) {\n    if (n == 0) {\n        return;\n    }\n    hanoi(n - 1, de, aux, para);\n    printf(\"Disco %d: %c -> %c\\n\", n, de, para);\n    total++;\n    hanoi(n - 1, aux, para, de);\n}\n\nint main(void) {\n    int n;\n    scanf(\"%d\", &n);\n    hanoi(n, 'A', 'C', 'B');\n    printf(\"Movimentos: %d\\n\", total);\n    return 0;\n}\n";
                gen = g => $"{g.Next(1, 5)}\n";
                oracle = i => { int n = PK.Ints(i)[0]; var l = new List<string>(); void H(int k, char a, char b, char c) { if (k == 0) return; H(k - 1, a, c, b); l.Add($"Disco {k}: {a} -> {b}"); H(k - 1, c, b, a); } H(n, 'A', 'C', 'B'); l.Add($"Movimentos: {l.Count}"); return string.Join("\n", l); };
                why = L.T("Para mover n discos de A para C: leve os n-1 de cima para B, mova o maior para C e traga os n-1 de B para C. Cada papel dos pinos (origem, destino, auxiliar) muda de uma chamada para outra.");
                break;
            default:
                st = L.T("Leia `n` (1 a 12), depois `n` inteiros positivos distintos e, por fim, um alvo `t`. Usando **recursão** (cada número entra ou não entra no subconjunto), mostre `Subconjuntos: <quantos subconjuntos têm soma exatamente t>` e `Menor: <menor tamanho de um subconjunto com soma t>` (ou `Menor: nenhum` se não existir).");
                sol = "#include <stdio.h>\n\nint v[12];\nint n;\nint contagem = 0;\nint menor = 100;\n\nvoid busca(int i, int soma, int tam, int alvo) {\n    if (soma == alvo) {\n        contagem++;\n        if (tam < menor) {\n            menor = tam;\n        }\n    }\n    if (i == n || soma >= alvo) {\n        return;\n    }\n    busca(i + 1, soma + v[i], tam + 1, alvo);\n    busca(i + 1, soma, tam, alvo);\n}\n\nint main(void) {\n    int i, t;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &v[i]);\n    }\n    scanf(\"%d\", &t);\n    busca(0, 0, 0, t);\n    printf(\"Subconjuntos: %d\\n\", contagem);\n    if (contagem == 0) {\n        printf(\"Menor: nenhum\\n\");\n    } else {\n        printf(\"Menor: %d\\n\", menor);\n    }\n    return 0;\n}\n";
                gen = g => { int n = g.Next(3, 10); var l = PK.Distinct(g, n, 1, 15); return $"{n}\n{PK.Join(l)}\n{g.Next(5, 30)}\n"; };
                oracle = i => { var v = PK.Ints(i); int n = v[0]; var a = v.Skip(1).Take(n).ToArray(); int t = v[n + 1]; int cnt = 0, mn = 100; for (int m = 1; m < (1 << n); m++) { int s = 0, sz = 0; for (int b = 0; b < n; b++) if ((m >> b & 1) == 1) { s += a[b]; sz++; } if (s == t) { cnt++; mn = Math.Min(mn, sz); } } return $"Subconjuntos: {cnt}\nMenor: {(cnt == 0 ? "nenhum" : mn.ToString())}"; };
                why = L.T("Em cada posição há duas escolhas: o número entra (soma e tamanho crescem) ou não entra. Como todos são positivos, dá para parar quando a soma passa do alvo. Cada chamada que chega na soma exata conta um subconjunto.");
                break;
        }
        return PK.Write(st, PK.CStarter, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict09(Difficulty d, Random r)
    {
        int n = r.Next(3, 7), k = r.Next(2, 5), a = r.Next(10, 99), b = r.Next(2, 9) * r.Next(1, 5); var o = new List<string>(); string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = PK.CHead + $"int f(int n) {{\n    if (n <= 1) {{\n        return 1;\n    }}\n    return n * f(n - 1);\n}}\n\nint main(void) {{\n    printf(\"%d\\n\", f({n}));\n    printf(\"%d\\n\", f({n - 2}) + 1);\n    return 0;\n}}\n";
                { int F(int x) => x <= 1 ? 1 : x * F(x - 1); o.Add(F(n).ToString()); o.Add((F(n - 2) + 1).ToString()); }
                break;
            case Difficulty.Medium:
                code = PK.CHead + $"void mostra(int n) {{\n    if (n == 0) {{\n        return;\n    }}\n    printf(\"%d \", n);\n    mostra(n - 1);\n    printf(\"%d \", n * {k});\n}}\n\nint main(void) {{\n    mostra({n});\n    printf(\"\\n\");\n    return 0;\n}}\n";
                { var sb = new StringBuilder(); void M(int x) { if (x == 0) return; sb.Append(x + " "); M(x - 1); sb.Append(x * k + " "); } M(n); o.Add(sb.ToString()); }
                break;
            case Difficulty.Hard:
            {
                code = PK.CHead + $"int passos = 0;\n\nint mdc(int a, int b) {{\n    passos++;\n    printf(\"%d %d\\n\", a, b);\n    if (b == 0) {{\n        return a;\n    }}\n    return mdc(b, a % b);\n}}\n\nint main(void) {{\n    int r = mdc({a}, {b});\n    printf(\"%d %d\\n\", r, passos);\n    return 0;\n}}\n";
                int x = a, y = b, p = 0; while (true) { p++; o.Add($"{x} {y}"); if (y == 0) break; (x, y) = (y, x % y); }
                o.Add($"{x} {p}");
                break;
            }
            default:
            {
                code = PK.CHead + $"int calls = 0;\n\nint f(int n) {{\n    calls++;\n    if (n < 2) {{\n        return n;\n    }}\n    if (n % 2 == 0) {{\n        return f(n - 1) + f(n - 2);\n    }}\n    return f(n - 1) * 2 - {k};\n}}\n\nint main(void) {{\n    printf(\"%d\\n\", f({n + 1}));\n    printf(\"%d\\n\", calls);\n    return 0;\n}}\n";
                int calls = 0; int F(int x) { calls++; if (x < 2) return x; if (x % 2 == 0) return F(x - 1) + F(x - 2); return F(x - 1) * 2 - k; }
                int res = F(n + 1); o.Add(res.ToString()); o.Add(calls.ToString());
                break;
            }
        }
        return PK.Predict(L.F("A função recursiva do programa é chamada com o valor {0} e o programa usa também o número {1}.", d == Difficulty.Hard ? a : d == Difficulty.Genius ? n + 1 : n, d == Difficulty.Hard ? b : k), code, string.Join("\n", o),
            L.T("Desça a pilha de chamadas até o caso base e depois volte combinando os resultados. O que vem antes da chamada recursiva acontece na ida (ordem de entrada); o que vem depois acontece na volta (ordem inversa)."));
    }

    // ---------- c-10 ----------
    static Exercise Write10(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        const string Sw = "";
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Leia `n` (1 a 15) e `n` inteiros. Ordene-os em ordem **crescente** com um algoritmo seu (bubble sort, por exemplo, sem usar bibliotecas) e mostre `Ordenado: <números separados por espaço>`, `Menor: <o menor>` e `Maior: <o maior>`.");
                sol = "#include <stdio.h>\n\nint main(void) {\n    int v[15];\n    int n, i, j, t;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &v[i]);\n    }\n    for (i = 0; i < n - 1; i++) {\n        for (j = 0; j < n - 1 - i; j++) {\n            if (v[j] > v[j + 1]) {\n                t = v[j];\n                v[j] = v[j + 1];\n                v[j + 1] = t;\n            }\n        }\n    }\n    printf(\"Ordenado:\");\n    for (i = 0; i < n; i++) {\n        printf(\" %d\", v[i]);\n    }\n    printf(\"\\n\");\n    printf(\"Menor: %d\\n\", v[0]);\n    printf(\"Maior: %d\\n\", v[n - 1]);\n    return 0;\n}\n";
                gen = g => { int n = g.Next(1, 10); return $"{n}\n{PK.Join(PK.Rand(g, n, -20, 40))}\n"; };
                oracle = i => { var v = PK.Ints(i).Skip(1).OrderBy(x => x).ToArray(); return $"Ordenado: {PK.Join(v)}\nMenor: {v[0]}\nMaior: {v[^1]}"; };
                why = L.T("No bubble sort, cada passada leva o maior valor restante para o fim; depois de ordenar, o menor está em `v[0]` e o maior em `v[n-1]`.");
                break;
            case Difficulty.Medium:
                st = L.T("Leia `n` (1 a 15) e `n` inteiros. Mostre `Decrescente: <números do maior para o menor>` e `Mediana: <o valor do meio do vetor ordenado; se n for par, a média dos dois do meio com 1 casa decimal>`.");
                sol = "#include <stdio.h>\n\nint main(void) {\n    int v[15];\n    int n, i, j, t;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &v[i]);\n    }\n    for (i = 0; i < n - 1; i++) {\n        for (j = 0; j < n - 1 - i; j++) {\n            if (v[j] < v[j + 1]) {\n                t = v[j];\n                v[j] = v[j + 1];\n                v[j + 1] = t;\n            }\n        }\n    }\n    printf(\"Decrescente:\");\n    for (i = 0; i < n; i++) {\n        printf(\" %d\", v[i]);\n    }\n    printf(\"\\n\");\n    if (n % 2 == 1) {\n        printf(\"Mediana: %d\\n\", v[n / 2]);\n    } else {\n        printf(\"Mediana: %.1f\\n\", (v[n / 2 - 1] + v[n / 2]) / 2.0);\n    }\n    return 0;\n}\n";
                gen = g => { int n = g.Next(1, 10); return $"{n}\n{PK.Join(PK.Rand(g, n, 0, 50))}\n"; };
                oracle = i => { var v = PK.Ints(i).Skip(1).OrderByDescending(x => x).ToArray(); int n = v.Length; string m = n % 2 == 1 ? v[n / 2].ToString() : ((v[n / 2 - 1] + v[n / 2]) / 2.0).ToString("F1", PK.Inv); return $"Decrescente: {PK.Join(v)}\nMediana: {m}"; };
                why = L.T("Para ordem decrescente basta inverter a comparação. A mediana é o elemento do meio (`v[n/2]`) ou, com n par, a média dos dois do meio.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia `n` (1 a 15) e `n` inteiros. Ordene com **bubble sort** contando as trocas (cada vez que dois vizinhos trocam de lugar) e as passadas completas que o algoritmo faz antes de parar (pare quando uma passada não trocar nada; a passada sem trocas também conta). Mostre `Trocas: <total>`, `Passadas: <total>` e `Ordenado: <números>`.");
                sol = "#include <stdio.h>\n\nint main(void) {\n    int v[15];\n    int n, i, j, t, trocou, trocas = 0, passadas = 0;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &v[i]);\n    }\n    do {\n        trocou = 0;\n        for (j = 0; j < n - 1; j++) {\n            if (v[j] > v[j + 1]) {\n                t = v[j];\n                v[j] = v[j + 1];\n                v[j + 1] = t;\n                trocas++;\n                trocou = 1;\n            }\n        }\n        passadas++;\n    } while (trocou);\n    printf(\"Trocas: %d\\n\", trocas);\n    printf(\"Passadas: %d\\n\", passadas);\n    printf(\"Ordenado:\");\n    for (i = 0; i < n; i++) {\n        printf(\" %d\", v[i]);\n    }\n    printf(\"\\n\");\n    return 0;\n}\n";
                gen = g => { int n = g.Next(1, 10); return $"{n}\n{PK.Join(PK.Rand(g, n, 1, 30))}\n"; };
                oracle = i => { var v = PK.Ints(i).Skip(1).ToArray(); int n = v.Length, tr = 0, pa = 0; bool sw; do { sw = false; for (int j = 0; j < n - 1; j++) if (v[j] > v[j + 1]) { (v[j], v[j + 1]) = (v[j + 1], v[j]); tr++; sw = true; } pa++; } while (sw); return $"Trocas: {tr}\nPassadas: {pa}\nOrdenado: {PK.Join(v)}"; };
                why = L.T("Cada troca de vizinhos desfaz exatamente uma inversão, então o número de trocas é o de pares fora de ordem. O laço só termina quando uma passada inteira não encontra nada para trocar.");
                break;
            default:
                st = L.T("Leia `n` (1 a 12), `n` inteiros **já em ordem crescente** (podem repetir), depois `q` (1 a 5) e `q` consultas. Para cada consulta `x`, use **busca binária** e mostre `<x>: primeiro indice <i>, ocorrencias <c>`, onde `i` é a posição da **primeira** ocorrência de `x` (começando em 0) e `c` quantas vezes ele aparece; se não existir, mostre `<x>: ausente`.");
                sol = "#include <stdio.h>\n\nint primeiro(int v[], int n, int x) {\n    int ini = 0, fim = n;\n    int meio;\n    while (ini < fim) {\n        meio = (ini + fim) / 2;\n        if (v[meio] < x) {\n            ini = meio + 1;\n        } else {\n            fim = meio;\n        }\n    }\n    return ini;\n}\n\nint main(void) {\n    int v[12];\n    int n, q, i, x, a, b;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &v[i]);\n    }\n    scanf(\"%d\", &q);\n    for (i = 0; i < q; i++) {\n        scanf(\"%d\", &x);\n        a = primeiro(v, n, x);\n        b = primeiro(v, n, x + 1);\n        if (a < n && v[a] == x) {\n            printf(\"%d: primeiro indice %d, ocorrencias %d\\n\", x, a, b - a);\n        } else {\n            printf(\"%d: ausente\\n\", x);\n        }\n    }\n    return 0;\n}\n";
                gen = g => { int n = g.Next(1, 11); var v = PK.Rand(g, n, 1, 8).OrderBy(x => x).ToList(); int q = g.Next(1, 6); return $"{n}\n{PK.Join(v)}\n{q}\n{PK.Join(PK.Rand(g, q, 0, 9), "\n")}\n"; };
                oracle = i => { var v = PK.Ints(i); int n = v[0]; var a = v.Skip(1).Take(n).ToArray(); int q = v[n + 1]; var l = new List<string>(); for (int j = 0; j < q; j++) { int x = v[n + 2 + j]; int f = Array.IndexOf(a, x); l.Add(f < 0 ? $"{x}: ausente" : $"{x}: primeiro indice {f}, ocorrencias {a.Count(y => y == x)}"); } return string.Join("\n", l); };
                why = L.T("A busca binária \"limite inferior\" acha a primeira posição com valor >= x: se `v[meio] < x` descarte a metade da esquerda, senão guarde o meio como candidato. O número de ocorrências é a diferença entre o limite de x e o de x+1.");
                break;
        }
        _ = Sw;
        return PK.Write(st, PK.CStarter, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict10(Difficulty d, Random r)
    {
        int n = r.Next(5, 8); var v = PK.Rand(r, n, 1, 20); var o = new List<string>(); string code;
        string Arr(int[] a) => PK.Join(a);
        var w = v.ToArray();
        string decl = $"    int v[{n}] = {{{PK.Join(v, ", ")}}};\n";
        switch (d)
        {
            case Difficulty.Easy:
                code = PK.CMain(decl + $"    int i, t;\n    for (i = 0; i < {n - 1}; i++) {{\n        if (v[i] > v[i + 1]) {{\n            t = v[i];\n            v[i] = v[i + 1];\n            v[i + 1] = t;\n        }}\n    }}\n    for (i = 0; i < {n}; i++) {{\n        printf(\"%d \", v[i]);\n    }}\n    printf(\"\\n\");\n");
                for (int i = 0; i < n - 1; i++) if (w[i] > w[i + 1]) (w[i], w[i + 1]) = (w[i + 1], w[i]);
                o.Add(Arr(w) + " ");
                break;
            case Difficulty.Medium:
            {
                int k = r.Next(2, 4);
                code = PK.CMain(decl + $"    int i, j, t, p;\n    for (p = 0; p < {k}; p++) {{\n        for (j = 0; j < {n - 1}; j++) {{\n            if (v[j] < v[j + 1]) {{\n                t = v[j];\n                v[j] = v[j + 1];\n                v[j + 1] = t;\n            }}\n        }}\n        for (i = 0; i < {n}; i++) {{\n            printf(\"%d \", v[i]);\n        }}\n        printf(\"\\n\");\n    }}\n");
                for (int p = 0; p < k; p++) { for (int j = 0; j < n - 1; j++) if (w[j] < w[j + 1]) (w[j], w[j + 1]) = (w[j + 1], w[j]); o.Add(Arr(w) + " "); }
                break;
            }
            case Difficulty.Hard:
            {
                var s = PK.Distinct(r, n + 2, 1, 40).OrderBy(x => x).ToArray(); int nn = s.Length;
                int x = r.Next(3) == 0 ? r.Next(1, 41) : s[r.Next(nn)];
                code = PK.CMain($"    int v[{nn}] = {{{PK.Join(s, ", ")}}};\n    int ini = 0, fim = {nn - 1}, meio, achou = -1;\n    while (ini <= fim) {{\n        meio = (ini + fim) / 2;\n        printf(\"%d \", v[meio]);\n        if (v[meio] == {x}) {{\n            achou = meio;\n            break;\n        }}\n        if (v[meio] < {x}) {{\n            ini = meio + 1;\n        }} else {{\n            fim = meio - 1;\n        }}\n    }}\n    printf(\"\\n%d\\n\", achou);\n");
                int a = 0, b = nn - 1, f = -1; var sb = new StringBuilder();
                while (a <= b) { int m = (a + b) / 2; sb.Append(s[m] + " "); if (s[m] == x) { f = m; break; } if (s[m] < x) a = m + 1; else b = m - 1; }
                o.Add(sb.ToString()); o.Add(f.ToString());
                return PK.Predict(L.F("O programa procura o número {0} num vetor ordenado.", x), code, string.Join("\n", o), L.T("A busca olha sempre o elemento do meio da faixa `[ini, fim]`, descarta a metade que não pode conter o valor e repete. O programa mostra cada elemento do meio olhado e, no fim, a posição achada ou -1."));
            }
            default:
            {
                code = PK.CMain(decl + $"    int i, j, chave, desl = 0;\n    for (i = 1; i < {n}; i++) {{\n        chave = v[i];\n        j = i - 1;\n        while (j >= 0 && v[j] > chave) {{\n            v[j + 1] = v[j];\n            j--;\n            desl++;\n        }}\n        v[j + 1] = chave;\n    }}\n    printf(\"%d\\n\", desl);\n    for (i = 0; i < {n}; i++) {{\n        printf(\"%d \", v[i]);\n    }}\n    printf(\"\\n%d\\n\", v[{n / 2}] + v[0] - v[{n - 1}]);\n");
                int desl = 0; for (int i = 1; i < n; i++) { int c = w[i], j = i - 1; while (j >= 0 && w[j] > c) { w[j + 1] = w[j]; j--; desl++; } w[j + 1] = c; }
                o.Add(desl.ToString()); o.Add(Arr(w) + " "); o.Add((w[n / 2] + w[0] - w[n - 1]).ToString());
                break;
            }
        }
        return PK.Predict(L.F("O vetor do programa começa com {0} números, o primeiro é {1}.", n, v[0]), code, string.Join("\n", o),
            L.T("Simule o algoritmo passo a passo anotando o vetor depois de cada passada. Uma passada de bubble sort compara vizinhos e leva o maior (ou menor) valor restante para a ponta; o insertion sort desliza os maiores para a direita até achar o lugar da chave."));
    }
}
