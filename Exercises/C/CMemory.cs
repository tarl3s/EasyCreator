using System.Text;

namespace EasyCreator.Exercises;

/// <summary>C lessons 11-14: pointer arithmetic, matrices, text analysis, dynamic memory.</summary>
[Exercises("c-11", "c-12", "c-13", "c-14")]
public sealed class CMemory : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r) => lesson.Id switch
    {
        "c-11" => r.Next(2) == 0 ? Write11(d, r) : Predict11(d, r),
        "c-12" => r.Next(2) == 0 ? Write12(d, r) : Predict12(d, r),
        "c-13" => r.Next(2) == 0 ? Write13(d, r) : Predict13(d, r),
        _ => r.Next(2) == 0 ? Write14(d, r) : Predict14(d, r),
    };

    static string NList(Random g, int max, int lo, int hi) { int n = g.Next(1, max + 1); return $"{n}\n{PK.Join(PK.Rand(g, n, lo, hi))}\n"; }
    static readonly string[] Words = { "sol", "mar", "casa", "livro", "pedra", "nuvem", "teclado", "cafe", "janela", "ponte", "radio", "fogo" };
    const string Read = "    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", v + i);\n    }\n";

    // ---------- c-11 ----------
    static Exercise Write11(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        string intro = L.T("Leia `n` (1 a 20) e, na linha seguinte, `n` inteiros. Escreva as funções **usando apenas ponteiros** (sem `v[i]` dentro delas). Todas recebem o intervalo `[ini, fim)`: `ini` aponta para o primeiro elemento e `fim` para uma posição depois do último.");
        switch (d)
        {
            case Difficulty.Easy:
                st = intro + " " + L.T("Função `int soma(const int *ini, const int *fim)`. Mostre `Soma: <soma>` e `Media: <média com 2 casas>`.");
                sol = "#include <stdio.h>\n\nint soma(const int *ini, const int *fim) {\n    int s = 0;\n    while (ini < fim) {\n        s += *ini;\n        ini++;\n    }\n    return s;\n}\n\nint main(void) {\n    int v[20];\n    int n, i;\n" + Read + "    printf(\"Soma: %d\\n\", soma(v, v + n));\n    printf(\"Media: %.2f\\n\", (double)soma(v, v + n) / n);\n    return 0;\n}\n";
                gen = g => NList(g, 12, -9, 30); oracle = i => { var v = PK.Ints(i).Skip(1).ToArray(); return $"Soma: {v.Sum()}\nMedia: {PK.F2((double)v.Sum() / v.Length)}"; };
                why = L.T("Um ponteiro percorre o intervalo: `*ini` é o valor atual e `ini++` avança uma posição; pare quando `ini == fim`.");
                break;
            case Difficulty.Medium:
                st = intro + " " + L.T("Função `const int *menor(const int *ini, const int *fim)` que devolve um ponteiro para o menor elemento (empate: o primeiro). Mostre `Menor: <valor> na posicao <índice>`, onde o índice vem da **subtração de ponteiros**.");
                sol = "#include <stdio.h>\n\nconst int *menor(const int *ini, const int *fim) {\n    const int *m = ini;\n    const int *p;\n    for (p = ini + 1; p < fim; p++) {\n        if (*p < *m) {\n            m = p;\n        }\n    }\n    return m;\n}\n\nint main(void) {\n    int v[20];\n    int n, i;\n    const int *m;\n" + Read + "    m = menor(v, v + n);\n    printf(\"Menor: %d na posicao %d\\n\", *m, (int)(m - v));\n    return 0;\n}\n";
                gen = g => NList(g, 12, -9, 30); oracle = i => { var v = PK.Ints(i).Skip(1).ToArray(); int m = Array.IndexOf(v, v.Min()); return $"Menor: {v[m]} na posicao {m}"; };
                why = L.T("A função devolve o endereço do menor; `m - v` é a distância em elementos entre dois ponteiros do mesmo vetor, ou seja, o índice.");
                break;
            case Difficulty.Hard:
                st = intro + " " + L.T("Escreva `void rotacionar(int *ini, int *fim)`, que move cada elemento uma posição para a esquerda e coloca o primeiro no fim, e `int contar(const int *ini, const int *fim, int x)`. Depois de ler os números, leia `x` (na linha seguinte) e mostre `Rotacionado: <vetor>` e `Aparece: <quantas vezes x aparece>`.");
                sol = "#include <stdio.h>\n\nvoid rotacionar(int *ini, int *fim) {\n    int primeiro = *ini;\n    int *p;\n    for (p = ini; p + 1 < fim; p++) {\n        *p = *(p + 1);\n    }\n    *(fim - 1) = primeiro;\n}\n\nint contar(const int *ini, const int *fim, int x) {\n    int c = 0;\n    for (; ini < fim; ini++) {\n        if (*ini == x) {\n            c++;\n        }\n    }\n    return c;\n}\n\nint main(void) {\n    int v[20];\n    int n, i, x;\n" + Read + "    scanf(\"%d\", &x);\n    rotacionar(v, v + n);\n    printf(\"Rotacionado:\");\n    for (i = 0; i < n; i++) {\n        printf(\" %d\", *(v + i));\n    }\n    printf(\"\\n\");\n    printf(\"Aparece: %d\\n\", contar(v, v + n, x));\n    return 0;\n}\n";
                gen = g => { var l = NList(g, 10, 1, 6); return l + g.Next(1, 7) + "\n"; };
                oracle = i => { var t = PK.Ints(i); int n = t[0]; var v = t.Skip(1).Take(n).ToList(); int x = t[n + 1]; int f = v[0]; v.RemoveAt(0); v.Add(f); return $"Rotacionado: {PK.Join(v)}\nAparece: {v.Count(y => y == x)}"; };
                why = L.T("Rotacionar: guarde o primeiro, copie cada elemento do vizinho da direita (`*p = *(p + 1)`) e ponha o primeiro na última posição `*(fim - 1)`.");
                break;
            default:
                st = intro + " " + L.T("Desta vez os números vêm **em ordem crescente** (podem repetir). Escreva `int *unicos(int *ini, int *fim)`, que remove as repetições no próprio vetor e devolve o novo `fim`, e `int pares(const int *ini, const int *fim, int alvo)`, que conta os pares de **posições** `i < j` com soma `alvo` usando dois ponteiros (um em cada ponta). Depois de ler `n` números e o `alvo` (linha seguinte), mostre `Unicos: <vetor sem repetições>` e `Pares: <quantidade de pares de valores distintos com soma alvo, no vetor sem repetições>`.");
                sol = "#include <stdio.h>\n\nint *unicos(int *ini, int *fim) {\n    int *w = ini;\n    int *p;\n    if (ini == fim) {\n        return fim;\n    }\n    for (p = ini + 1; p < fim; p++) {\n        if (*p != *w) {\n            w++;\n            *w = *p;\n        }\n    }\n    return w + 1;\n}\n\nint pares(const int *ini, const int *fim, int alvo) {\n    const int *a = ini;\n    const int *b = fim - 1;\n    int c = 0;\n    while (a < b) {\n        if (*a + *b == alvo) {\n            c++;\n            a++;\n            b--;\n        } else if (*a + *b < alvo) {\n            a++;\n        } else {\n            b--;\n        }\n    }\n    return c;\n}\n\nint main(void) {\n    int v[20];\n    int n, i, alvo;\n    int *fim;\n" + Read + "    scanf(\"%d\", &alvo);\n    fim = unicos(v, v + n);\n    printf(\"Unicos:\");\n    for (i = 0; i < fim - v; i++) {\n        printf(\" %d\", v[i]);\n    }\n    printf(\"\\n\");\n    printf(\"Pares: %d\\n\", pares(v, fim, alvo));\n    return 0;\n}\n";
                gen = g => { int n = g.Next(1, 12); var v = PK.Rand(g, n, 1, 9).OrderBy(x => x).ToList(); return $"{n}\n{PK.Join(v)}\n{g.Next(3, 17)}\n"; };
                oracle = i => { var t = PK.Ints(i); int n = t[0]; var u = t.Skip(1).Take(n).Distinct().ToList(); int alvo = t[n + 1]; int c = 0; for (int a = 0; a < u.Count; a++) for (int b = a + 1; b < u.Count; b++) if (u[a] + u[b] == alvo) c++; return $"Unicos: {PK.Join(u)}\nPares: {c}"; };
                why = L.T("Com o vetor ordenado, um ponteiro `w` marca o fim da parte sem repetições e `p` lê. Para os pares, se a soma é pequena avance o ponteiro da esquerda, se é grande recue o da direita; cada acerto consome os dois.");
                break;
        }
        return PK.Write(st, PK.CStarter, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict11(Difficulty d, Random r)
    {
        int n = r.Next(5, 8); var v = PK.Rand(r, n, 1, 9); var o = new List<string>(); string code;
        string decl = $"    int v[{n}] = {{{PK.Join(v, ", ")}}};\n";
        int i1 = r.Next(1, n - 2), k = r.Next(2, 5);
        switch (d)
        {
            case Difficulty.Easy:
                code = PK.CMain(decl + $"    int *p = v;\n    p = p + {i1};\n    printf(\"%d %d\\n\", *p, *(p + 1));\n    *p = *p + {k};\n    printf(\"%d %d\\n\", v[{i1}], (int)(p - v));\n");
                o.Add($"{v[i1]} {v[i1 + 1]}"); o.Add($"{v[i1] + k} {i1}");
                break;
            case Difficulty.Medium:
                code = PK.CMain(decl + $"    int *p = v;\n    int *q = v + {n - 1};\n    int s = 0;\n    while (p <= q) {{\n        s += *p * *q;\n        p++;\n        q--;\n    }}\n    printf(\"%d\\n\", s);\n    printf(\"%d %d\\n\", *p, (int)(p - q));\n");
                { int p = 0, q = n - 1, s = 0; while (p <= q) { s += v[p] * v[q]; p++; q--; } o.Add(s.ToString()); o.Add($"{(p < n ? v[p].ToString() : "?")} {p - q}"); if (p >= n) return Predict11(d, r); }
                break;
            case Difficulty.Hard:
                code = PK.CMain(decl + $"    int *p = v;\n    int i, t = 0;\n    for (i = 0; i < {n - 1}; i++) {{\n        *p = *p + *(p + 1);\n        t += *p++;\n    }}\n    printf(\"%d %d\\n\", t, *p);\n    for (i = 0; i < {n}; i++) {{\n        printf(\"%d \", v[i]);\n    }}\n    printf(\"\\n\");\n");
                { var w = v.ToArray(); int t = 0; for (int i = 0; i < n - 1; i++) { w[i] = w[i] + w[i + 1]; t += w[i]; } o.Add($"{t} {w[n - 1]}"); o.Add(PK.Join(w) + " "); }
                break;
            default:
                code = PK.CMain(decl + $"    int *a = v;\n    int *b = v + {n - 1};\n    int t;\n    while (a < b) {{\n        t = *a;\n        *a = *b;\n        *b = t;\n        a += 2;\n        b -= 1;\n    }}\n    printf(\"%d %d\\n\", (int)(a - v), (int)(b - v));\n    for (t = 0; t < {n}; t++) {{\n        printf(\"%d \", *(v + t));\n    }}\n    printf(\"\\n\");\n");
                { var w = v.ToArray(); int a = 0, b = n - 1; while (a < b) { (w[a], w[b]) = (w[b], w[a]); a += 2; b -= 1; } o.Add($"{a} {b}"); o.Add(PK.Join(w) + " "); }
                break;
        }
        return PK.Predict(L.F("O vetor do programa tem {0} números e começa com {1}.", n, v[0]) + " " + L.F("Os valores são {0}.", PK.Join(v, ", ")), code, string.Join("\n", o),
            L.T("`p + k` aponta k elementos adiante (não k bytes). `*p++` lê o valor e depois avança o ponteiro. A diferença de dois ponteiros do mesmo vetor é o número de elementos entre eles."));
    }

    // ---------- c-12 ----------
    static string Matrix(Random g, int l, int c, int lo, int hi) => string.Join("\n", Enumerable.Range(0, l).Select(_ => PK.Join(PK.Rand(g, c, lo, hi))));
    const string ReadM = "    scanf(\"%d %d\", &l, &c);\n    for (i = 0; i < l; i++) {\n        for (j = 0; j < c; j++) {\n            scanf(\"%d\", &m[i][j]);\n        }\n    }\n";

    static Exercise Write12(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        string intro = L.T("Leia `l` e `c` (de 1 a 6) e depois a matriz `l x c` (`l` linhas de `c` inteiros).");
        string head = "#include <stdio.h>\n\nint main(void) {\n    int m[6][6];\n    int l, c, i, j;\n";
        Func<Random, int[][]> dummy = null!;
        switch (d)
        {
            case Difficulty.Easy:
                st = intro + " " + L.T("Mostre uma linha `Linha <i>: <soma da linha>` para cada linha (começando em 1) e, no fim, `Total: <soma de tudo>`.");
                sol = head + "    int s, total = 0;\n" + ReadM + "    for (i = 0; i < l; i++) {\n        s = 0;\n        for (j = 0; j < c; j++) {\n            s += m[i][j];\n        }\n        printf(\"Linha %d: %d\\n\", i + 1, s);\n        total += s;\n    }\n    printf(\"Total: %d\\n\", total);\n    return 0;\n}\n";
                gen = g => { int l = g.Next(1, 6), c = g.Next(1, 6); return $"{l} {c}\n{Matrix(g, l, c, -5, 20)}\n"; };
                oracle = i => { var v = PK.Ints(i); int l = v[0], c = v[1]; var sb = new List<string>(); int tot = 0; for (int a = 0; a < l; a++) { int s = v.Skip(2 + a * c).Take(c).Sum(); sb.Add($"Linha {a + 1}: {s}"); tot += s; } sb.Add($"Total: {tot}"); return string.Join("\n", sb); };
                why = L.T("Dois laços aninhados: o de fora escolhe a linha `i`, o de dentro percorre as colunas `j` acumulando. Zere a soma da linha antes do laço interno.");
                break;
            case Difficulty.Medium:
                st = intro + " " + L.T("Mostre a **transposta** (a matriz `c x l` em que a linha `j` é a coluna `j` da original): `c` linhas, cada uma com `l` números separados por espaço.");
                sol = head + ReadM + "    for (j = 0; j < c; j++) {\n        for (i = 0; i < l; i++) {\n            if (i > 0) {\n                printf(\" \");\n            }\n            printf(\"%d\", m[i][j]);\n        }\n        printf(\"\\n\");\n    }\n    return 0;\n}\n";
                gen = g => { int l = g.Next(1, 6), c = g.Next(1, 6); return $"{l} {c}\n{Matrix(g, l, c, -5, 20)}\n"; };
                oracle = i => { var v = PK.Ints(i); int l = v[0], c = v[1]; return string.Join("\n", Enumerable.Range(0, c).Select(j => PK.Join(Enumerable.Range(0, l).Select(a => v[2 + a * c + j])))); };
                why = L.T("A transposta troca os papéis dos índices: o laço de fora percorre as colunas `j` e o de dentro as linhas `i`, imprimindo `m[i][j]`.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia `n` (1 a 6) e uma matriz quadrada `n x n`. Mostre `Principal: <soma da diagonal principal>`, `Secundaria: <soma da diagonal secundária>` e `Simetrica: sim` se `m[i][j] == m[j][i]` para todos os pares (senão `Simetrica: nao`).");
                sol = "#include <stdio.h>\n\nint main(void) {\n    int m[6][6];\n    int n, i, j, p = 0, s = 0, sim = 1;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        for (j = 0; j < n; j++) {\n            scanf(\"%d\", &m[i][j]);\n        }\n    }\n    for (i = 0; i < n; i++) {\n        p += m[i][i];\n        s += m[i][n - 1 - i];\n        for (j = 0; j < n; j++) {\n            if (m[i][j] != m[j][i]) {\n                sim = 0;\n            }\n        }\n    }\n    printf(\"Principal: %d\\n\", p);\n    printf(\"Secundaria: %d\\n\", s);\n    printf(\"Simetrica: %s\\n\", sim ? \"sim\" : \"nao\");\n    return 0;\n}\n";
                gen = g =>
                {
                    int n = g.Next(1, 5); var m = new int[n, n]; bool sym = g.Next(2) == 0;
                    for (int a = 0; a < n; a++) for (int b = 0; b < n; b++) m[a, b] = g.Next(0, 6);
                    if (sym) for (int a = 0; a < n; a++) for (int b = 0; b < a; b++) m[a, b] = m[b, a];
                    return $"{n}\n" + string.Join("\n", Enumerable.Range(0, n).Select(a => PK.Join(Enumerable.Range(0, n).Select(b => m[a, b])))) + "\n";
                };
                oracle = i => { var v = PK.Ints(i); int n = v[0]; int M(int a, int b) => v[1 + a * n + b]; int p = 0, s = 0; bool sim = true; for (int a = 0; a < n; a++) { p += M(a, a); s += M(a, n - 1 - a); for (int b = 0; b < n; b++) if (M(a, b) != M(b, a)) sim = false; } return $"Principal: {p}\nSecundaria: {s}\nSimetrica: {(sim ? "sim" : "nao")}"; };
                why = L.T("Na diagonal principal a coluna é igual à linha (`m[i][i]`); na secundária é `n-1-i`. Uma matriz é simétrica quando nenhum par difere de seu espelho.");
                break;
            default:
                st = intro + " " + L.T("Mostre os elementos em **espiral** no sentido horário, a partir do canto superior esquerdo, todos em uma linha separados por espaço (primeiro a linha de cima, depois a coluna da direita, a linha de baixo de trás para frente e a coluna da esquerda de baixo para cima, e assim para dentro).");
                sol = head + "    int topo, base, esq, dir, primeiro = 1;\n" + ReadM + "    topo = 0;\n    base = l - 1;\n    esq = 0;\n    dir = c - 1;\n    while (topo <= base && esq <= dir) {\n        for (j = esq; j <= dir; j++) {\n            printf(primeiro ? \"%d\" : \" %d\", m[topo][j]);\n            primeiro = 0;\n        }\n        topo++;\n        for (i = topo; i <= base; i++) {\n            printf(\" %d\", m[i][dir]);\n        }\n        dir--;\n        if (topo <= base) {\n            for (j = dir; j >= esq; j--) {\n                printf(\" %d\", m[base][j]);\n            }\n            base--;\n        }\n        if (esq <= dir) {\n            for (i = base; i >= topo; i--) {\n                printf(\" %d\", m[i][esq]);\n            }\n            esq++;\n        }\n    }\n    printf(\"\\n\");\n    return 0;\n}\n";
                gen = g => { int l = g.Next(1, 6), c = g.Next(1, 6); return $"{l} {c}\n{Matrix(g, l, c, 0, 30)}\n"; };
                oracle = i =>
                {
                    var v = PK.Ints(i); int l = v[0], c = v[1]; int M(int a, int b) => v[2 + a * c + b]; var res = new List<int>();
                    int t = 0, bs = l - 1, e = 0, dr = c - 1;
                    while (t <= bs && e <= dr)
                    {
                        for (int j = e; j <= dr; j++) res.Add(M(t, j)); t++;
                        for (int a = t; a <= bs; a++) res.Add(M(a, dr)); dr--;
                        if (t <= bs) { for (int j = dr; j >= e; j--) res.Add(M(bs, j)); bs--; }
                        if (e <= dr) { for (int a = bs; a >= t; a--) res.Add(M(a, e)); e++; }
                    }
                    return PK.Join(res);
                };
                why = L.T("Quatro limites (topo, base, esquerda, direita) encolhem a cada volta. Depois de percorrer cada lado, aquele limite anda para dentro; os dois últimos lados precisam de um teste porque a matriz pode ter acabado.");
                break;
        }
        _ = dummy;
        return PK.Write(st, PK.CStarter, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict12(Difficulty d, Random r)
    {
        int n = r.Next(3, 5), k = r.Next(1, 4), a = r.Next(1, 4); var o = new List<string>(); string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = PK.CMain($"    int m[{n}][{n}];\n    int i, j;\n    for (i = 0; i < {n}; i++) {{\n        for (j = 0; j < {n}; j++) {{\n            m[i][j] = i * {k} + j;\n        }}\n    }}\n    for (i = 0; i < {n}; i++) {{\n        printf(\"%d %d\\n\", m[i][0], m[i][{n - 1}]);\n    }}\n");
                for (int i = 0; i < n; i++) o.Add($"{i * k} {i * k + n - 1}");
                break;
            case Difficulty.Medium:
                code = PK.CMain($"    int m[{n}][{n}];\n    int i, j, s = 0, t = 0;\n    for (i = 0; i < {n}; i++) {{\n        for (j = 0; j < {n}; j++) {{\n            m[i][j] = (i + {a}) * (j + 1);\n        }}\n    }}\n    for (i = 0; i < {n}; i++) {{\n        s += m[i][i];\n        t += m[i][{n - 1} - i];\n    }}\n    printf(\"%d %d\\n\", s, t);\n");
                { int s = 0, t = 0; for (int i = 0; i < n; i++) { s += (i + a) * (i + 1); t += (i + a) * (n - 1 - i + 1); } o.Add($"{s} {t}"); }
                break;
            case Difficulty.Hard:
                code = PK.CMain($"    int m[{n}][{n}];\n    int i, j, t;\n    for (i = 0; i < {n}; i++) {{\n        for (j = 0; j < {n}; j++) {{\n            m[i][j] = i + j * {k};\n        }}\n    }}\n    for (i = 0; i < {n}; i++) {{\n        for (j = i + 1; j < {n}; j++) {{\n            t = m[i][j];\n            m[i][j] = m[j][i];\n            m[j][i] = t;\n        }}\n    }}\n    for (i = 0; i < {n}; i++) {{\n        for (j = 0; j < {n}; j++) {{\n            printf(\"%d \", m[i][j]);\n        }}\n        printf(\"\\n\");\n    }}\n");
                { var m = new int[n, n]; for (int i = 0; i < n; i++) for (int j = 0; j < n; j++) m[i, j] = i + j * k; for (int i = 0; i < n; i++) for (int j = i + 1; j < n; j++) (m[i, j], m[j, i]) = (m[j, i], m[i, j]); for (int i = 0; i < n; i++) o.Add(string.Join("", Enumerable.Range(0, n).Select(j => m[i, j] + " "))); }
                break;
            default:
            {
                var A = new int[2, 2]; var B = new int[2, 2]; for (int i = 0; i < 2; i++) for (int j = 0; j < 2; j++) { A[i, j] = r.Next(0, 5); B[i, j] = r.Next(0, 5); }
                code = PK.CMain($"    int a[2][2] = {{{{{A[0, 0]}, {A[0, 1]}}}, {{{A[1, 0]}, {A[1, 1]}}}}};\n    int b[2][2] = {{{{{B[0, 0]}, {B[0, 1]}}}, {{{B[1, 0]}, {B[1, 1]}}}}};\n    int c[2][2];\n    int i, j, k;\n    for (i = 0; i < 2; i++) {{\n        for (j = 0; j < 2; j++) {{\n            c[i][j] = 0;\n            for (k = 0; k < 2; k++) {{\n                c[i][j] += a[i][k] * b[k][j];\n            }}\n        }}\n    }}\n    for (i = 0; i < 2; i++) {{\n        printf(\"%d %d\\n\", c[i][0], c[i][1]);\n    }}\n    printf(\"%d\\n\", c[0][0] + c[1][1]);\n");
                var C = new int[2, 2]; for (int i = 0; i < 2; i++) for (int j = 0; j < 2; j++) for (int q = 0; q < 2; q++) C[i, j] += A[i, q] * B[q, j];
                o.Add($"{C[0, 0]} {C[0, 1]}"); o.Add($"{C[1, 0]} {C[1, 1]}"); o.Add((C[0, 0] + C[1, 1]).ToString());
                break;
            }
        }
        return PK.Predict(d == Difficulty.Genius ? L.T("O programa multiplica duas matrizes 2x2 escritas no código.") : L.F("O programa usa uma matriz {0}x{0} e os números {1} e {2} na fórmula de preenchimento.", n, k, a), code, string.Join("\n", o),
            L.T("Calcule cada elemento com a fórmula dada, linha por linha. `m[i][j]` é a linha `i`, coluna `j`. Na multiplicação, `c[i][j]` é a soma de `a[i][k] * b[k][j]` sobre `k`."));
    }

    // ---------- c-13 ----------
    static string Line(Random g, int words, bool spaces)
    {
        var l = Enumerable.Range(0, words).Select(_ => PK.Pick(g, Words)).ToList();
        return string.Join(spaces ? PK.Pick(g, " ", "  ", " ") : " ", l);
    }

    static Exercise Write13(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        string intro = L.T("Leia **uma linha** com `fgets`.");
        string head = "#include <stdio.h>\n#include <ctype.h>\n\nint main(void) {\n    char linha[201];\n    int i;\n    if (fgets(linha, 201, stdin) == NULL) {\n        linha[0] = '\\0';\n    }\n";
        switch (d)
        {
            case Difficulty.Easy:
                st = intro + " " + L.T("A linha tem palavras de letras minúsculas separadas por um ou mais espaços. Mostre `Palavras: <quantas>` e `Letras: <quantas letras no total>`.");
                sol = head + "    int palavras = 0, letras = 0, dentro = 0;\n    for (i = 0; linha[i] != '\\0'; i++) {\n        if (isalpha(linha[i])) {\n            letras++;\n            if (!dentro) {\n                palavras++;\n            }\n            dentro = 1;\n        } else {\n            dentro = 0;\n        }\n    }\n    printf(\"Palavras: %d\\n\", palavras);\n    printf(\"Letras: %d\\n\", letras);\n    return 0;\n}\n";
                gen = g => Line(g, g.Next(1, 6), true) + "\n"; oracle = i => { var w = i.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries); return $"Palavras: {w.Length}\nLetras: {w.Sum(x => x.Length)}"; };
                why = L.T("Uma palavra começa quando uma letra aparece logo depois de um não-letra. Um sinalizador `dentro` diz se já estamos numa palavra; só conte na transição.");
                break;
            case Difficulty.Medium:
                st = intro + " " + L.T("A linha mistura palavras e números inteiros não negativos. Mostre `Numeros: <quantos números aparecem>` e `Soma: <soma deles>`. Dígitos seguidos formam um só número (`ab12cd345` tem os números 12 e 345).");
                sol = head + "    int numeros = 0, soma = 0, atual = 0, dentro = 0;\n    for (i = 0; ; i++) {\n        if (isdigit(linha[i])) {\n            atual = atual * 10 + (linha[i] - '0');\n            dentro = 1;\n        } else {\n            if (dentro) {\n                numeros++;\n                soma += atual;\n            }\n            atual = 0;\n            dentro = 0;\n        }\n        if (linha[i] == '\\0') {\n            break;\n        }\n    }\n    printf(\"Numeros: %d\\n\", numeros);\n    printf(\"Soma: %d\\n\", soma);\n    return 0;\n}\n";
                gen = g => { var sb = new StringBuilder(); int k = g.Next(2, 7); for (int q = 0; q < k; q++) sb.Append(g.Next(3) == 0 ? g.Next(1, 500).ToString() : PK.Pick(g, Words)).Append(g.Next(3) == 0 ? "" : " "); return sb.ToString().TrimEnd() + "\n"; };
                oracle = i => { int n = 0, s = 0, cur = 0; bool dentro = false; foreach (char c in i + "\0") { if (char.IsDigit(c)) { cur = cur * 10 + (c - '0'); dentro = true; } else { if (dentro) { n++; s += cur; } cur = 0; dentro = false; } } return $"Numeros: {n}\nSoma: {s}"; };
                why = L.T("Acumule os dígitos num número (`atual * 10 + dígito`). Quando o número termina (um não-dígito aparece depois de dígitos), some-o e conte. Não esqueça do número que termina junto com a linha.");
                break;
            case Difficulty.Hard:
                st = intro + " " + L.T("A linha tem palavras minúsculas separadas por espaços. Mostre a linha com a **primeira letra de cada palavra em maiúscula** (os espaços ficam como estão) e, na linha seguinte, `Maior palavra: <a palavra mais longa, em minúsculas; empate: a primeira>`.");
                sol = head + "    char melhor[201];\n    char atual[201];\n    int n = 0, nm = 0, ini = 1;\n    melhor[0] = '\\0';\n    for (i = 0; ; i++) {\n        if (isalpha(linha[i])) {\n            atual[n] = linha[i];\n            n++;\n            putchar(ini ? toupper(linha[i]) : linha[i]);\n            ini = 0;\n        } else {\n            if (n > nm) {\n                nm = n;\n                atual[n] = '\\0';\n                {\n                    int k;\n                    for (k = 0; k <= n; k++) {\n                        melhor[k] = atual[k];\n                    }\n                }\n            }\n            n = 0;\n            ini = 1;\n            if (linha[i] == '\\n' || linha[i] == '\\0') {\n                break;\n            }\n            putchar(linha[i]);\n        }\n    }\n    printf(\"\\nMaior palavra: %s\\n\", melhor);\n    return 0;\n}\n";
                gen = g => Line(g, g.Next(1, 6), true) + "\n";
                oracle = i => { var t = i.TrimEnd('\n'); var sb = new StringBuilder(); bool ini = true; foreach (char c in t) { if (char.IsLetter(c)) { sb.Append(ini ? char.ToUpper(c) : c); ini = false; } else { sb.Append(c); ini = true; } } var best = t.Split(' ', StringSplitOptions.RemoveEmptyEntries).Aggregate("", (a, b) => b.Length > a.Length ? b : a); return sb + "\nMaior palavra: " + best; };
                why = L.T("Marque o começo de cada palavra com um sinalizador. Guarde a palavra atual numa cópia e, quando ela terminar, compare o tamanho com o da melhor (só substitui se for estritamente maior).");
                break;
            default:
                st = intro + " " + L.T("A linha tem uma expressão com parênteses `()`, colchetes `[]` e chaves `{}` misturados com letras e espaços. Se todos estiverem balanceados (cada um fecha com o par certo, na ordem certa), mostre `balanceado`. Senão mostre `erro na coluna <c>` com a coluna (começando em 1) do primeiro problema: um fechamento que não combina com o último aberto (ou sem nenhum aberto), ou, se a linha acabar com algum aberto, a coluna do último que ficou aberto.");
                sol = head + "    char pilha[201];\n    int col[201];\n    int topo = 0;\n    char c, a;\n    for (i = 0; linha[i] != '\\0' && linha[i] != '\\n'; i++) {\n        c = linha[i];\n        if (c == '(' || c == '[' || c == '{') {\n            pilha[topo] = c;\n            col[topo] = i + 1;\n            topo++;\n        } else if (c == ')' || c == ']' || c == '}') {\n            if (topo == 0) {\n                printf(\"erro na coluna %d\\n\", i + 1);\n                return 0;\n            }\n            a = pilha[topo - 1];\n            if ((c == ')' && a != '(') || (c == ']' && a != '[') || (c == '}' && a != '{')) {\n                printf(\"erro na coluna %d\\n\", i + 1);\n                return 0;\n            }\n            topo--;\n        }\n    }\n    if (topo > 0) {\n        printf(\"erro na coluna %d\\n\", col[topo - 1]);\n    } else {\n        printf(\"balanceado\\n\");\n    }\n    return 0;\n}\n";
                gen = g =>
                {
                    if (g.Next(3) == 0) { string o = "([{"; var sb = new StringBuilder(); for (int q = 0; q < g.Next(1, 6); q++) sb.Append(PK.Pick(g, "(", "[", "{", ")", "]", "}", "a", "b")); return sb + "\n"; }
                    var st2 = new Stack<char>(); var res = new StringBuilder(); int len = g.Next(1, 5);
                    for (int q = 0; q < len; q++) { char open = PK.Pick(g, '(', '[', '{'); st2.Push(open); res.Append(open); res.Append(PK.Pick(g, "x", "")); if (g.Next(2) == 0 && st2.Count > 0) { var p = st2.Pop(); res.Append(p == '(' ? ')' : p == '[' ? ']' : '}'); } }
                    while (st2.Count > 0 && g.Next(6) != 0) { var p = st2.Pop(); res.Append(p == '(' ? ')' : p == '[' ? ']' : '}'); }
                    return res + "\n";
                };
                oracle = i =>
                {
                    var s = i.TrimEnd('\n'); var stk = new List<(char c, int col)>();
                    for (int q = 0; q < s.Length; q++)
                    {
                        char c = s[q];
                        if ("([{".Contains(c)) stk.Add((c, q + 1));
                        else if (")]}".Contains(c)) { if (stk.Count == 0) return $"erro na coluna {q + 1}"; var a = stk[^1].c; if ((c == ')' && a != '(') || (c == ']' && a != '[') || (c == '}' && a != '{')) return $"erro na coluna {q + 1}"; stk.RemoveAt(stk.Count - 1); }
                    }
                    return stk.Count > 0 ? $"erro na coluna {stk[^1].col}" : "balanceado";
                };
                why = L.T("Uma pilha guarda os símbolos abertos (e a coluna deles). Ao ver um fechamento, o topo da pilha precisa ser o par correspondente; se a pilha estiver vazia ou o par for outro, esse é o erro.");
                break;
        }
        return PK.Write(st, PK.CStarter, sol, PK.Tests(r, 7, gen, oracle), why);
    }

    static Exercise Predict13(Difficulty d, Random r)
    {
        var o = new List<string>(); string code;
        string w = PK.Pick(r, Words), w2 = PK.Pick(r, Words); int a = r.Next(1, 300), b = r.Next(1, 90);
        string text = $"{w}{a} {w2}{b}";
        switch (d)
        {
            case Difficulty.Easy:
                code = PK.CMain($"    char s[] = \"{text}\";\n    int i, letras = 0, digitos = 0;\n    for (i = 0; s[i] != '\\0'; i++) {{\n        if (isalpha(s[i])) {{\n            letras++;\n        }} else if (isdigit(s[i])) {{\n            digitos++;\n        }}\n    }}\n    printf(\"%d %d %d\\n\", letras, digitos, i);\n", "#include <ctype.h>\n");
                o.Add($"{text.Count(char.IsLetter)} {text.Count(char.IsDigit)} {text.Length}");
                break;
            case Difficulty.Medium:
                code = PK.CMain($"    char s[] = \"{text}\";\n    int i, atual = 0, soma = 0;\n    for (i = 0; ; i++) {{\n        if (isdigit(s[i])) {{\n            atual = atual * 10 + s[i] - '0';\n        }} else {{\n            soma += atual;\n            atual = 0;\n        }}\n        if (s[i] == '\\0') {{\n            break;\n        }}\n    }}\n    printf(\"%d\\n\", soma);\n", "#include <ctype.h>\n");
                o.Add((a + b).ToString());
                break;
            case Difficulty.Hard:
                code = PK.CMain($"    char s[] = \"{text}\";\n    int i;\n    for (i = 0; s[i] != '\\0'; i++) {{\n        if (islower(s[i])) {{\n            s[i] = toupper(s[i]);\n        }} else if (s[i] == ' ') {{\n            s[i] = '_';\n        }}\n    }}\n    printf(\"%s\\n\", s);\n    printf(\"%c%c\\n\", s[0], s[i - 1]);\n", "#include <ctype.h>\n");
                { var t = text.ToUpper().Replace(' ', '_'); o.Add(t); o.Add($"{t[0]}{t[^1]}"); }
                break;
            default:
            {
                string expr = $"{a} + {b}-{r.Next(1, 40)}";
                var parts = expr.Replace(" ", "").Split(new[] { '+', '-' }); int sign = 1, res = 0, ia = 0; int[] vals = parts.Select(int.Parse).ToArray(); var ops = expr.Where(c => c == '+' || c == '-').ToArray();
                res = vals[0]; for (int i = 0; i < ops.Length; i++) res += (ops[i] == '+' ? 1 : -1) * vals[i + 1]; _ = sign; _ = ia;
                code = PK.CMain($"    char s[] = \"{expr}\";\n    char *p = s;\n    int total = 0, sinal = 1, num;\n    while (*p != '\\0') {{\n        if (isdigit(*p)) {{\n            num = 0;\n            while (isdigit(*p)) {{\n                num = num * 10 + (*p - '0');\n                p++;\n            }}\n            total += sinal * num;\n        }} else {{\n            if (*p == '-') {{\n                sinal = -1;\n            }} else if (*p == '+') {{\n                sinal = 1;\n            }}\n            p++;\n        }}\n    }}\n    printf(\"%d %d\\n\", total, (int)(p - s));\n", "#include <ctype.h>\n");
                o.Add($"{res} {expr.Length}");
                break;
            }
        }
        return PK.Predict(L.F("O texto do programa contém os números {0} e {1}.", a, b), code, string.Join("\n", o),
            L.T("O programa percorre o texto caractere por caractere. `isalpha` e `isdigit` classificam cada um; um número é formado acumulando `atual * 10 + dígito`. Letras minúsculas viram maiúsculas com `toupper` e o espaço é trocado por `_`."));
    }

    // ---------- c-14 ----------
    static Exercise Write14(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Leia `n` (1 a 50) e, depois, `n` inteiros. Guarde-os num vetor alocado com `malloc(n * sizeof(int))` e mostre `Invertido: <números de trás para frente>`, `Soma: <soma>` e `Maior: <maior valor>`. Libere a memória com `free` no fim.");
                sol = "#include <stdio.h>\n#include <stdlib.h>\n\nint main(void) {\n    int n, i, soma = 0;\n    int *v;\n    scanf(\"%d\", &n);\n    v = malloc(n * sizeof(int));\n    if (v == NULL) {\n        return 1;\n    }\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &v[i]);\n        soma += v[i];\n    }\n    printf(\"Invertido:\");\n    for (i = n - 1; i >= 0; i--) {\n        printf(\" %d\", v[i]);\n    }\n    printf(\"\\n\");\n    printf(\"Soma: %d\\n\", soma);\n    {\n        int maior = v[0];\n        for (i = 1; i < n; i++) {\n            if (v[i] > maior) {\n                maior = v[i];\n            }\n        }\n        printf(\"Maior: %d\\n\", maior);\n    }\n    free(v);\n    return 0;\n}\n";
                gen = g => NList(g, 15, -20, 50); oracle = i => { var v = PK.Ints(i).Skip(1).ToArray(); return $"Invertido: {PK.Join(v.Reverse())}\nSoma: {v.Sum()}\nMaior: {v.Max()}"; };
                why = L.T("`malloc(n * sizeof(int))` reserva espaço para `n` inteiros; depois dele o ponteiro se usa como um vetor. Todo `malloc` termina com um `free`.");
                break;
            case Difficulty.Medium:
                st = L.T("Leia inteiros até aparecer `0` (ele não conta). Guarde-os num vetor dinâmico que começa com capacidade 1 e **dobra** (com `realloc`) quando enche. Mostre `Lidos: <quantidade>`, `Capacidade final: <capacidade>` e `Redimensionamentos: <quantas vezes o vetor cresceu>`.");
                sol = "#include <stdio.h>\n#include <stdlib.h>\n\nint main(void) {\n    int cap = 1, n = 0, x, cresceu = 0;\n    int *v = malloc(cap * sizeof(int));\n    while (scanf(\"%d\", &x) == 1 && x != 0) {\n        if (n == cap) {\n            cap = cap * 2;\n            v = realloc(v, cap * sizeof(int));\n            cresceu++;\n        }\n        v[n] = x;\n        n++;\n    }\n    printf(\"Lidos: %d\\n\", n);\n    printf(\"Capacidade final: %d\\n\", cap);\n    printf(\"Redimensionamentos: %d\\n\", cresceu);\n    free(v);\n    return 0;\n}\n";
                gen = g => { int n = g.Next(0, 18); return PK.Join(PK.Rand(g, n, 1, 90)) + (n > 0 ? " " : "") + "0\n"; };
                oracle = i => { var v = PK.Ints(i).TakeWhile(x => x != 0).ToArray(); int cap = 1, c = 0; for (int j = 0; j < v.Length; j++) if (j == cap) { cap *= 2; c++; } return $"Lidos: {v.Length}\nCapacidade final: {cap}\nRedimensionamentos: {c}"; };
                why = L.T("Antes de guardar, se `n == cap` o vetor está cheio: dobre `cap` e chame `realloc`. A capacidade vai 1, 2, 4, 8... então ela cresce a cada potência de 2.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia `l` e `c` (de 1 a 8) e depois `l` linhas de `c` inteiros. Guarde a matriz em **um único bloco** alocado com `malloc(l * c * sizeof(int))` (o elemento da linha `i`, coluna `j` fica em `m[i * c + j]`) e mostre `Colunas: <soma de cada coluna, separadas por espaço>` e `Maior linha: <número da linha de maior soma, começando em 1; empate: a primeira>`.");
                sol = "#include <stdio.h>\n#include <stdlib.h>\n\nint main(void) {\n    int l, c, i, j, melhor = 0, sm, sc;\n    int *m;\n    int best = 0;\n    scanf(\"%d %d\", &l, &c);\n    m = malloc(l * c * sizeof(int));\n    for (i = 0; i < l * c; i++) {\n        scanf(\"%d\", &m[i]);\n    }\n    printf(\"Colunas:\");\n    for (j = 0; j < c; j++) {\n        sc = 0;\n        for (i = 0; i < l; i++) {\n            sc += m[i * c + j];\n        }\n        printf(\" %d\", sc);\n    }\n    printf(\"\\n\");\n    for (i = 0; i < l; i++) {\n        sm = 0;\n        for (j = 0; j < c; j++) {\n            sm += m[i * c + j];\n        }\n        if (i == 0 || sm > best) {\n            best = sm;\n            melhor = i;\n        }\n    }\n    printf(\"Maior linha: %d\\n\", melhor + 1);\n    free(m);\n    return 0;\n}\n";
                gen = g => { int l = g.Next(1, 6), c = g.Next(1, 6); return $"{l} {c}\n{Matrix(g, l, c, 0, 20)}\n"; };
                oracle = i => { var v = PK.Ints(i); int l = v[0], c = v[1]; int M(int a, int b) => v[2 + a * c + b]; var cols = Enumerable.Range(0, c).Select(j => Enumerable.Range(0, l).Sum(a => M(a, j))); int bi = 0, bs = -1; for (int a = 0; a < l; a++) { int s = Enumerable.Range(0, c).Sum(j => M(a, j)); if (s > bs) { bs = s; bi = a; } } return $"Colunas: {PK.Join(cols)}\nMaior linha: {bi + 1}"; };
                why = L.T("Uma matriz num bloco só usa a conta `i * c + j` para achar o elemento. Para somar uma coluna, `i` varia e `j` fica fixo; numa linha, o contrário.");
                break;
            default:
                st = L.T("Leia `n` (1 a 8) e `n` palavras (letras minúsculas, até 20 letras). Copie cada uma para memória alocada com `malloc(strlen(palavra) + 1)` e `strcpy`, guardando os ponteiros num vetor `char *p[8]`. Ordene **os ponteiros** em ordem alfabética (`strcmp`; empate: ordem de leitura) e mostre uma linha `<palavra> (<tamanho>)` para cada, depois `Total de letras: <soma dos tamanhos>`. Libere tudo.");
                sol = "#include <stdio.h>\n#include <stdlib.h>\n#include <string.h>\n\nint main(void) {\n    char *p[8];\n    char *t;\n    char buf[21];\n    int n, i, j, total = 0;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%s\", buf);\n        p[i] = malloc(strlen(buf) + 1);\n        strcpy(p[i], buf);\n        total += strlen(buf);\n    }\n    for (i = 1; i < n; i++) {\n        t = p[i];\n        j = i - 1;\n        while (j >= 0 && strcmp(p[j], t) > 0) {\n            p[j + 1] = p[j];\n            j--;\n        }\n        p[j + 1] = t;\n    }\n    for (i = 0; i < n; i++) {\n        printf(\"%s (%d)\\n\", p[i], (int)strlen(p[i]));\n    }\n    printf(\"Total de letras: %d\\n\", total);\n    for (i = 0; i < n; i++) {\n        free(p[i]);\n    }\n    return 0;\n}\n";
                gen = g => { int n = g.Next(1, 8); var w = Enumerable.Range(0, n).Select(_ => PK.Pick(g, Words)).ToList(); return $"{n}\n{string.Join(" ", w)}\n"; };
                oracle = i => { var t = i.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries); var w = t.Skip(1).ToList(); var s = w.Select((x, k) => (x, k)).OrderBy(a => a.x, StringComparer.Ordinal).ThenBy(a => a.k).Select(a => a.x).ToList(); return string.Join("\n", s.Select(x => $"{x} ({x.Length})")) + $"\nTotal de letras: {w.Sum(x => x.Length)}"; };
                why = L.T("Cada palavra ganha seu próprio bloco (`strlen + 1` por causa do `'\\0'`). Ordenar o vetor de ponteiros só troca endereços, não copia texto. Cada `malloc` precisa do seu `free`.");
                break;
        }
        return PK.Write(st, PK.CStarter, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict14(Difficulty d, Random r)
    {
        int n = r.Next(3, 7), k = r.Next(2, 5); var o = new List<string>(); string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = PK.CMain($"    int *v = malloc({n} * sizeof(int));\n    int i, s = 0;\n    for (i = 0; i < {n}; i++) {{\n        v[i] = (i + 1) * {k};\n        s += v[i];\n    }}\n    printf(\"%d %d\\n\", v[{n - 1}], s);\n    free(v);\n", "#include <stdlib.h>\n");
                o.Add($"{n * k} {k * n * (n + 1) / 2}");
                break;
            case Difficulty.Medium:
            {
                int start = r.Next(1, 3);
                code = PK.CMain($"    int cap = {start}, n = 0, i;\n    int *v = malloc(cap * sizeof(int));\n    for (i = 0; i < {n + 4}; i++) {{\n        if (n == cap) {{\n            printf(\"%d->%d \", cap, cap * 2);\n            cap *= 2;\n            v = realloc(v, cap * sizeof(int));\n        }}\n        v[n++] = i * {k};\n    }}\n    printf(\"\\n%d %d\\n\", n, v[n - 1]);\n    free(v);\n", "#include <stdlib.h>\n");
                int cap = start, nn = 0; var sb = new StringBuilder(); for (int i = 0; i < n + 4; i++) { if (nn == cap) { sb.Append($"{cap}->{cap * 2} "); cap *= 2; } nn++; }
                o.Add(sb.ToString()); o.Add($"{nn} {(n + 3) * k}");
                break;
            }
            case Difficulty.Hard:
                code = PK.CMain($"    int *a = malloc({n} * sizeof(int));\n    int *b = a;\n    int i;\n    for (i = 0; i < {n}; i++) {{\n        a[i] = i * i;\n    }}\n    b[0] = {k * 10};\n    b += 2;\n    *b = *b + a[1];\n    printf(\"%d %d %d\\n\", a[0], a[2], b[-1]);\n    printf(\"%d\\n\", (int)(b - a));\n    free(a);\n", "#include <stdlib.h>\n");
                { var a = new int[n]; for (int i = 0; i < n; i++) a[i] = i * i; a[0] = k * 10; a[2] = a[2] + a[1]; o.Add($"{a[0]} {a[2]} {a[1]}"); o.Add("2"); }
                break;
            default:
            {
                int cols = r.Next(2, 4), rows = r.Next(2, 4);
                code = PK.CMain($"    int **m = malloc({rows} * sizeof(int *));\n    int i, j, s = 0;\n    for (i = 0; i < {rows}; i++) {{\n        m[i] = calloc({cols}, sizeof(int));\n        for (j = 0; j <= i && j < {cols}; j++) {{\n            m[i][j] = i + j + {k};\n        }}\n    }}\n    for (i = 0; i < {rows}; i++) {{\n        for (j = 0; j < {cols}; j++) {{\n            printf(\"%d \", m[i][j]);\n            s += m[i][j];\n        }}\n        printf(\"\\n\");\n        free(m[i]);\n    }}\n    printf(\"%d\\n\", s);\n    free(m);\n", "#include <stdlib.h>\n");
                int s = 0; for (int i = 0; i < rows; i++) { var sb = new StringBuilder(); for (int j = 0; j < cols; j++) { int val = j <= i ? i + j + k : 0; sb.Append(val + " "); s += val; } o.Add(sb.ToString()); }
                o.Add(s.ToString());
                break;
            }
        }
        return PK.Predict(L.F("O programa usa o valor {0} e o fator {1}.", n, k), code, string.Join("\n", o),
            L.T("`malloc` devolve um bloco que se usa como vetor (não vem zerado; `calloc` zera). Dobrar a capacidade com `realloc` mantém o conteúdo. Um ponteiro que avança (`b += 2`) continua apontando para dentro do mesmo bloco, então `b[-1]` é o elemento anterior."));
    }
}
