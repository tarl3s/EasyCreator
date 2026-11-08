using System.Text;

namespace EasyCreator.Exercises;

/// <summary>C lessons 15-20: linked lists, stacks, bit manipulation, merge sort, hash tables, BFS.</summary>
[Exercises("c-15", "c-16", "c-17", "c-18", "c-19", "c-20")]
public sealed class CStructures : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r) => lesson.Id switch
    {
        "c-15" => r.Next(2) == 0 ? Write15(d, r) : Predict15(d, r),
        "c-16" => r.Next(2) == 0 ? Write16(d, r) : Predict16(d, r),
        "c-17" => r.Next(2) == 0 ? Write17(d, r) : Predict17(d, r),
        "c-18" => r.Next(2) == 0 ? Write18(d, r) : Predict18(d, r),
        "c-19" => r.Next(2) == 0 ? Write19(d, r) : Predict19(d, r),
        _ => r.Next(2) == 0 ? Write20(d, r) : Predict20(d, r),
    };

    static readonly string[] Words = { "sol", "mar", "casa", "livro", "pedra", "nuvem", "teclado", "cafe", "janela", "ponte", "radio", "fogo" };
    static string NList(Random g, int max, int lo, int hi) { int n = g.Next(1, max + 1); return $"{n}\n{PK.Join(PK.Rand(g, n, lo, hi))}\n"; }
    const string Node = "#include <stdio.h>\n#include <stdlib.h>\n\ntypedef struct No {\n    int valor;\n    struct No *prox;\n} No;\n\n";
    const string Show = "void mostrar(No *c) {\n    printf(\"Lista:\");\n    if (c == NULL) {\n        printf(\" vazia\");\n    }\n    for (; c != NULL; c = c->prox) {\n        printf(\" %d\", c->valor);\n    }\n    printf(\"\\n\");\n}\n\n";
    const string Free = "    while (cab != NULL) {\n        No *p = cab->prox;\n        free(cab);\n        cab = p;\n    }\n";

    // ---------- c-15 ----------
    static Exercise Write15(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        string def = L.T("Use uma lista ligada com `typedef struct No { int valor; struct No *prox; } No;` (um nó por número, alocado com `malloc`, tudo liberado com `free` no fim).");
        switch (d)
        {
            case Difficulty.Easy:
                st = def + " " + L.T("Leia `n` e `n` inteiros e insira cada um **no início** da lista. Mostre `Lista: <valores do início ao fim>` e `Tamanho: <n>`. (Como cada um entra na frente, a lista sai na ordem inversa da leitura.)");
                sol = Node + Show + "int main(void) {\n    No *cab = NULL;\n    No *novo;\n    int n, i, x, tam = 0;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &x);\n        novo = malloc(sizeof(No));\n        novo->valor = x;\n        novo->prox = cab;\n        cab = novo;\n        tam++;\n    }\n    mostrar(cab);\n    printf(\"Tamanho: %d\\n\", tam);\n" + Free + "    return 0;\n}\n";
                gen = g => NList(g, 10, 1, 60); oracle = i => { var v = PK.Ints(i).Skip(1).Reverse().ToArray(); return $"Lista: {PK.Join(v)}\nTamanho: {v.Length}"; };
                why = L.T("Inserir no início: o novo nó aponta para a antiga cabeça (`novo->prox = cab`) e vira a cabeça (`cab = novo`).");
                break;
            case Difficulty.Medium:
                st = def + " " + L.T("Leia `n` e `n` inteiros e insira cada um **no fim** da lista (mantendo a ordem de leitura). Mostre `Lista: <valores>`, `Soma: <soma>` e `Maior: <maior valor>` percorrendo a lista.");
                sol = Node + Show + "int main(void) {\n    No *cab = NULL;\n    No *novo;\n    No *p;\n    int n, i, x, soma = 0, maior = 0;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &x);\n        novo = malloc(sizeof(No));\n        novo->valor = x;\n        novo->prox = NULL;\n        if (cab == NULL) {\n            cab = novo;\n        } else {\n            p = cab;\n            while (p->prox != NULL) {\n                p = p->prox;\n            }\n            p->prox = novo;\n        }\n    }\n    mostrar(cab);\n    for (p = cab; p != NULL; p = p->prox) {\n        soma += p->valor;\n        if (p == cab || p->valor > maior) {\n            maior = p->valor;\n        }\n    }\n    printf(\"Soma: %d\\n\", soma);\n    printf(\"Maior: %d\\n\", maior);\n" + Free + "    return 0;\n}\n";
                gen = g => NList(g, 10, -10, 60); oracle = i => { var v = PK.Ints(i).Skip(1).ToArray(); return $"Lista: {PK.Join(v)}\nSoma: {v.Sum()}\nMaior: {v.Max()}"; };
                why = L.T("Para inserir no fim é preciso andar até o último nó (`p->prox == NULL`). Se a lista está vazia, o novo nó é a cabeça.");
                break;
            case Difficulty.Hard:
                st = def + " " + L.T("Leia `n` e depois `n` comandos: `I <v>` insere `v` no início; `R <v>` remove a **primeira** ocorrência de `v` (se não existir, mostre `Nao encontrado: <v>`); `P` mostra `Lista: <valores>` (ou `Lista: vazia`). No fim mostre `Tamanho: <nós que sobraram>`.");
                sol = Node + Show + "int main(void) {\n    No *cab = NULL;\n    No *novo;\n    No *p;\n    No *ant;\n    int n, i, v, tam = 0;\n    char op;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\" %c\", &op);\n        if (op == 'I') {\n            scanf(\"%d\", &v);\n            novo = malloc(sizeof(No));\n            novo->valor = v;\n            novo->prox = cab;\n            cab = novo;\n            tam++;\n        } else if (op == 'R') {\n            scanf(\"%d\", &v);\n            ant = NULL;\n            p = cab;\n            while (p != NULL && p->valor != v) {\n                ant = p;\n                p = p->prox;\n            }\n            if (p == NULL) {\n                printf(\"Nao encontrado: %d\\n\", v);\n            } else {\n                if (ant == NULL) {\n                    cab = p->prox;\n                } else {\n                    ant->prox = p->prox;\n                }\n                free(p);\n                tam--;\n            }\n        } else {\n            mostrar(cab);\n        }\n    }\n    printf(\"Tamanho: %d\\n\", tam);\n" + Free + "    return 0;\n}\n";
                gen = g =>
                {
                    int n = g.Next(2, 10); var sb = new StringBuilder(n + "\n");
                    for (int k = 0; k < n; k++) { int t = g.Next(5); sb.Append(t <= 1 ? $"I {g.Next(1, 6)}\n" : t == 2 ? "P\n" : $"R {g.Next(1, 6)}\n"); }
                    return sb.ToString();
                };
                oracle = i =>
                {
                    var lines = i.Split('\n', StringSplitOptions.RemoveEmptyEntries); var lst = new List<int>(); var o = new List<string>();
                    foreach (var ln in lines.Skip(1)) { var t = ln.Split(' '); if (t[0] == "I") lst.Insert(0, int.Parse(t[1])); else if (t[0] == "R") { int v = int.Parse(t[1]); int ix = lst.IndexOf(v); if (ix < 0) o.Add($"Nao encontrado: {v}"); else lst.RemoveAt(ix); } else o.Add(lst.Count == 0 ? "Lista: vazia" : "Lista: " + PK.Join(lst)); }
                    o.Add($"Tamanho: {lst.Count}"); return string.Join("\n", o);
                };
                why = L.T("Para remover é preciso lembrar do nó anterior (`ant`): ele passa a apontar para o seguinte do removido. Se o removido é a cabeça, a cabeça anda para o próximo.");
                break;
            default:
                st = def + " " + L.T("Leia `n` e `n` inteiros e monte a lista **na ordem de leitura**. Mostre `Original: <valores>`. Depois **inverta a lista mudando os ponteiros** `prox` (sem criar nós novos) e mostre `Invertida: <valores>`. Por fim mostre `Meio: <valor do nó do meio>` da lista invertida (com `n` par, o segundo dos dois do meio), achado com dois ponteiros: um anda 1 nó por vez e o outro 2.");
                sol = Node + "void mostrar(const char *t, No *c) {\n    printf(\"%s:\", t);\n    for (; c != NULL; c = c->prox) {\n        printf(\" %d\", c->valor);\n    }\n    printf(\"\\n\");\n}\n\nNo *inverter(No *c) {\n    No *ant = NULL;\n    No *prox;\n    while (c != NULL) {\n        prox = c->prox;\n        c->prox = ant;\n        ant = c;\n        c = prox;\n    }\n    return ant;\n}\n\nint main(void) {\n    No *cab = NULL;\n    No *fim = NULL;\n    No *novo;\n    No *lento;\n    No *rapido;\n    int n, i, x;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &x);\n        novo = malloc(sizeof(No));\n        novo->valor = x;\n        novo->prox = NULL;\n        if (cab == NULL) {\n            cab = novo;\n        } else {\n            fim->prox = novo;\n        }\n        fim = novo;\n    }\n    mostrar(\"Original\", cab);\n    cab = inverter(cab);\n    mostrar(\"Invertida\", cab);\n    lento = cab;\n    rapido = cab;\n    while (rapido != NULL && rapido->prox != NULL) {\n        lento = lento->prox;\n        rapido = rapido->prox->prox;\n    }\n    printf(\"Meio: %d\\n\", lento->valor);\n" + Free + "    return 0;\n}\n";
                gen = g => NList(g, 11, 1, 90);
                oracle = i => { var v = PK.Ints(i).Skip(1).ToArray(); var inv = v.Reverse().ToArray(); return $"Original: {PK.Join(v)}\nInvertida: {PK.Join(inv)}\nMeio: {inv[inv.Length / 2]}"; };
                why = L.T("Inverter: percorra guardando o próximo antes de mexer; faça `c->prox = ant` e avance `ant` e `c`. Meio: quando o rápido (2 nós por passo) chega ao fim, o lento (1 por passo) está na metade. Guardar o `fim` evita percorrer a lista a cada inserção.");
                break;
        }
        return PK.Write(st, PK.CStarter, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict15(Difficulty d, Random r)
    {
        var v = PK.Rand(r, 4, 1, 9); int k = r.Next(2, 6); var o = new List<string>(); string code;
        string nd = "typedef struct No {\n    int valor;\n    struct No *prox;\n} No;\n\n";
        switch (d)
        {
            case Difficulty.Easy:
                code = PK.CHead + nd + $"int main(void) {{\n    No a = {{{v[0]}, NULL}};\n    No b = {{{v[1]}, NULL}};\n    No c = {{{v[2]}, NULL}};\n    No *p;\n    int s = 0;\n    a.prox = &b;\n    b.prox = &c;\n    for (p = &a; p != NULL; p = p->prox) {{\n        s += p->valor;\n    }}\n    printf(\"%d %d\\n\", s, a.prox->prox->valor);\n    return 0;\n}}\n";
                o.Add($"{v[0] + v[1] + v[2]} {v[2]}");
                break;
            case Difficulty.Medium:
                code = PK.CHead + "#include <stdlib.h>\n\n" + nd + $"int main(void) {{\n    No *cab = NULL;\n    No *n;\n    int i;\n    for (i = 1; i <= {v.Count}; i++) {{\n        n = malloc(sizeof(No));\n        n->valor = i * {k};\n        n->prox = cab;\n        cab = n;\n    }}\n    for (n = cab; n != NULL; n = n->prox) {{\n        printf(\"%d \", n->valor);\n    }}\n    printf(\"\\n%d\\n\", cab->valor - cab->prox->valor);\n    return 0;\n}}\n";
                o.Add(PK.Join(Enumerable.Range(1, v.Count).Reverse().Select(i => i * k)) + " "); o.Add(k.ToString());
                break;
            case Difficulty.Hard:
                code = PK.CHead + nd + $"int main(void) {{\n    No n[4] = {{{{{v[0]}, NULL}}, {{{v[1]}, NULL}}, {{{v[2]}, NULL}}, {{{v[3]}, NULL}}}};\n    No *p;\n    No *ant = NULL;\n    int i;\n    for (i = 0; i < 3; i++) {{\n        n[i].prox = &n[i + 1];\n    }}\n    n[0].prox = &n[2];\n    for (p = &n[0]; p != NULL; p = p->prox) {{\n        printf(\"%d \", p->valor);\n        ant = p;\n    }}\n    printf(\"\\n%d\\n\", ant->valor * {k});\n    return 0;\n}}\n";
                o.Add($"{v[0]} {v[2]} {v[3]} "); o.Add((v[3] * k).ToString());
                break;
            default:
                code = PK.CHead + nd + $"int main(void) {{\n    No n[4] = {{{{{v[0]}, NULL}}, {{{v[1]}, NULL}}, {{{v[2]}, NULL}}, {{{v[3]}, NULL}}}};\n    No *cab = &n[0];\n    No *ant = NULL;\n    No *prox;\n    No *p;\n    int i;\n    for (i = 0; i < 3; i++) {{\n        n[i].prox = &n[i + 1];\n    }}\n    while (cab != NULL) {{\n        prox = cab->prox;\n        cab->prox = ant;\n        ant = cab;\n        cab = prox;\n    }}\n    for (p = ant; p != NULL; p = p->prox) {{\n        printf(\"%d \", p->valor);\n    }}\n    printf(\"\\n%d\\n\", n[0].prox == NULL);\n    return 0;\n}}\n";
                o.Add($"{v[3]} {v[2]} {v[1]} {v[0]} "); o.Add("1");
                break;
        }
        return PK.Predict(L.F("Os valores guardados nos nós são {0}.", PK.Join(v, ", ")) + " " + L.F("O fator usado é {0}.", k), code, string.Join("\n", o),
            L.T("Desenhe os nós como caixas com setas: `prox` é a seta para o próximo nó e `NULL` é o fim. Siga as setas a partir da cabeça. Inserir no início faz a ordem de impressão ficar invertida; mudar um `prox` pode pular nós."));
    }

    // ---------- c-16 ----------
    static Exercise Write16(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        string stack = "int pilha[100];\nint topo = 0;\n\nvoid empilhar(int x) {\n    pilha[topo] = x;\n    topo++;\n}\n\nint desempilhar(void) {\n    topo--;\n    return pilha[topo];\n}\n\n";
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Implemente uma pilha de inteiros com um vetor e um índice `topo`. Leia `n` e `n` comandos: `P <v>` empilha `v`; `D` desempilha e mostra `Saiu: <valor>` (ou `Pilha vazia` se não houver nada); no fim mostre `Restam: <quantidade>`.");
                sol = "#include <stdio.h>\n\n" + stack + "int main(void) {\n    int n, i, v;\n    char op;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\" %c\", &op);\n        if (op == 'P') {\n            scanf(\"%d\", &v);\n            empilhar(v);\n        } else if (topo == 0) {\n            printf(\"Pilha vazia\\n\");\n        } else {\n            printf(\"Saiu: %d\\n\", desempilhar());\n        }\n    }\n    printf(\"Restam: %d\\n\", topo);\n    return 0;\n}\n";
                gen = g => { int n = g.Next(2, 10); var sb = new StringBuilder(n + "\n"); for (int k = 0; k < n; k++) sb.Append(g.Next(5) < 3 ? $"P {g.Next(1, 50)}\n" : "D\n"); return sb.ToString(); };
                oracle = i => { var lines = i.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1); var s = new Stack<int>(); var o = new List<string>(); foreach (var ln in lines) { if (ln[0] == 'P') s.Push(int.Parse(ln[2..])); else o.Add(s.Count == 0 ? "Pilha vazia" : $"Saiu: {s.Pop()}"); } o.Add($"Restam: {s.Count}"); return string.Join("\n", o); };
                why = L.T("A pilha é LIFO (o último a entrar é o primeiro a sair): `topo` marca a próxima posição livre; empilhar grava e sobe, desempilhar desce e lê. Teste `topo == 0` antes de desempilhar.");
                break;
            case Difficulty.Medium:
                st = L.T("Leia uma expressão **pós-fixa** de inteiros não negativos com os operadores `+`, `-` e `*` separados por espaços (a expressão é sempre válida). Avalie com uma pilha e mostre `Resultado: <valor>`. Em `a b -`, o resultado é `a - b`.");
                sol = "#include <stdio.h>\n#include <stdlib.h>\n#include <ctype.h>\n\n" + stack + "int main(void) {\n    char tok[32];\n    int a, b;\n    while (scanf(\"%s\", tok) == 1) {\n        if (isdigit(tok[0])) {\n            empilhar(atoi(tok));\n        } else {\n            b = desempilhar();\n            a = desempilhar();\n            if (tok[0] == '+') {\n                empilhar(a + b);\n            } else if (tok[0] == '-') {\n                empilhar(a - b);\n            } else {\n                empilhar(a * b);\n            }\n        }\n    }\n    printf(\"Resultado: %d\\n\", pilha[0]);\n    return 0;\n}\n";
                gen = g => PostFix(g, g.Next(2, 6)) + "\n";
                oracle = i => "Resultado: " + EvalPost(i.Trim());
                why = L.T("Número: empilha. Operador: desempilha dois (o primeiro a sair é o operando da direita `b`), calcula `a op b` e empilha o resultado. No fim, a pilha guarda só o resultado.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia uma expressão pós-fixa (números não negativos e `+ - * /`, separados por espaços) e avalie com uma pilha. Mostre `Resultado: <valor>` (divisão inteira) ou, no lugar, **só** uma destas mensagens ao primeiro erro: `Erro: faltam operandos` (operador com menos de dois valores), `Erro: divisao por zero` ou `Erro: sobraram <k> valores` (quando terminar com mais de um valor na pilha).");
                sol = "#include <stdio.h>\n#include <stdlib.h>\n#include <ctype.h>\n\n" + stack + "int main(void) {\n    char tok[32];\n    int a, b;\n    while (scanf(\"%s\", tok) == 1) {\n        if (isdigit(tok[0])) {\n            empilhar(atoi(tok));\n        } else {\n            if (topo < 2) {\n                printf(\"Erro: faltam operandos\\n\");\n                return 0;\n            }\n            b = desempilhar();\n            a = desempilhar();\n            if (tok[0] == '+') {\n                empilhar(a + b);\n            } else if (tok[0] == '-') {\n                empilhar(a - b);\n            } else if (tok[0] == '*') {\n                empilhar(a * b);\n            } else {\n                if (b == 0) {\n                    printf(\"Erro: divisao por zero\\n\");\n                    return 0;\n                }\n                empilhar(a / b);\n            }\n        }\n    }\n    if (topo == 1) {\n        printf(\"Resultado: %d\\n\", pilha[0]);\n    } else {\n        printf(\"Erro: sobraram %d valores\\n\", topo);\n    }\n    return 0;\n}\n";
                gen = g =>
                {
                    int t = g.Next(6);
                    string e = PostFix(g, g.Next(2, 5), true);
                    if (t == 0) e += " " + g.Next(1, 9);
                    if (t == 1) e += " +";
                    if (t == 2) e = g.Next(1, 9) + " " + g.Next(0, 2) + " /";
                    if (t == 3) e = "+ " + e;
                    return e + "\n";
                };
                oracle = i =>
                {
                    var s = new Stack<int>();
                    foreach (var tk in i.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (char.IsDigit(tk[0])) s.Push(int.Parse(tk));
                        else
                        {
                            if (s.Count < 2) return "Erro: faltam operandos";
                            int b = s.Pop(), a = s.Pop();
                            if (tk == "+") s.Push(a + b); else if (tk == "-") s.Push(a - b); else if (tk == "*") s.Push(a * b);
                            else { if (b == 0) return "Erro: divisao por zero"; s.Push(a / b); }
                        }
                    }
                    return s.Count == 1 ? $"Resultado: {s.Peek()}" : $"Erro: sobraram {s.Count} valores";
                };
                why = L.T("Antes de cada operador confira se há dois valores. A divisão por zero também é checada antes de calcular. No fim, uma expressão correta deixa exatamente um valor na pilha.");
                break;
            default:
                st = L.T("Leia `n` (1 a 15) e `n` inteiros. Para cada posição, mostre o **próximo elemento maior** à sua direita (o primeiro valor estritamente maior que vem depois dele) ou `-1` se não houver. Mostre uma linha `Proximos: <valores separados por espaço>`. Use uma pilha de índices para resolver em uma única passada.");
                sol = "#include <stdio.h>\n\nint main(void) {\n    int v[15];\n    int res[15];\n    int pilha[15];\n    int topo = 0;\n    int n, i;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &v[i]);\n        res[i] = -1;\n    }\n    for (i = 0; i < n; i++) {\n        while (topo > 0 && v[pilha[topo - 1]] < v[i]) {\n            topo--;\n            res[pilha[topo]] = v[i];\n        }\n        pilha[topo] = i;\n        topo++;\n    }\n    printf(\"Proximos:\");\n    for (i = 0; i < n; i++) {\n        printf(\" %d\", res[i]);\n    }\n    printf(\"\\n\");\n    return 0;\n}\n";
                gen = g => NList(g, 12, 1, 30);
                oracle = i => { var v = PK.Ints(i).Skip(1).ToArray(); var res = v.Select((x, k) => { for (int j = k + 1; j < v.Length; j++) if (v[j] > x) return v[j]; return -1; }); return "Proximos: " + PK.Join(res); };
                why = L.T("A pilha guarda os índices que ainda esperam um maior. Quando chega um valor maior que o do topo, ele é a resposta de quem está no topo: resolva e desempilhe até o topo não ser menor. Cada índice entra e sai uma vez, então a passada é única.");
                break;
        }
        return PK.Write(st, PK.CStarter, sol, PK.Tests(r, 7, gen, oracle), why);
    }

    static string PostFix(Random g, int terms, bool allowDiv = false)
    {
        // builds a valid postfix expression with `terms` numbers
        var toks = new List<string>(); int depth = 0, nums = 0; var ops = allowDiv ? new[] { "+", "-", "*", "/" } : new[] { "+", "-", "*" };
        while (nums < terms || depth > 1)
        {
            if (nums < terms && (depth < 2 || g.Next(2) == 0)) { toks.Add(g.Next(1, 10).ToString()); nums++; depth++; }
            else { toks.Add(PK.Pick(g, ops)); depth--; }
        }
        return string.Join(" ", toks);
    }

    static string EvalPost(string e)
    {
        var s = new Stack<long>();
        foreach (var tk in e.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (char.IsDigit(tk[0])) { s.Push(long.Parse(tk)); continue; }
            long b = s.Pop(), a = s.Pop();
            s.Push(tk == "+" ? a + b : tk == "-" ? a - b : tk == "*" ? a * b : a / b);
        }
        return s.Peek().ToString();
    }

    static Exercise Predict16(Difficulty d, Random r)
    {
        var o = new List<string>(); string code;
        string stack = "int pilha[20];\nint topo = 0;\n\nvoid push(int x) {\n    pilha[topo++] = x;\n}\n\nint pop(void) {\n    return pilha[--topo];\n}\n\n";
        int a = r.Next(1, 9), b = r.Next(1, 9), c = r.Next(1, 9), k = r.Next(2, 5);
        switch (d)
        {
            case Difficulty.Easy:
                code = PK.CHead + stack + $"int main(void) {{\n    push({a});\n    push({b});\n    push({c});\n    printf(\"%d \", pop());\n    push({k});\n    printf(\"%d \", pop());\n    printf(\"%d \", pop());\n    printf(\"%d\\n\", topo);\n    return 0;\n}}\n";
                o.Add($"{c} {k} {b} {1}");
                break;
            case Difficulty.Medium:
                code = PK.CHead + stack + $"int main(void) {{\n    int x, y;\n    push({a});\n    push({b});\n    y = pop();\n    x = pop();\n    push(x * y);\n    push({c});\n    y = pop();\n    x = pop();\n    push(x - y);\n    printf(\"%d %d\\n\", pop(), topo);\n    return 0;\n}}\n";
                o.Add($"{a * b - c} 0");
                break;
            case Difficulty.Hard:
            {
                var v = PK.Rand(r, 6, 1, 9);
                code = PK.CHead + stack + $"int main(void) {{\n    int v[6] = {{{PK.Join(v, ", ")}}};\n    int i, soma = 0;\n    for (i = 0; i < 6; i++) {{\n        while (topo > 0 && pilha[topo - 1] < v[i]) {{\n            soma += pop();\n        }}\n        push(v[i]);\n    }}\n    printf(\"%d %d\\n\", soma, topo);\n    return 0;\n}}\n";
                var st = new Stack<int>(); int s = 0; foreach (var x in v) { while (st.Count > 0 && st.Peek() < x) s += st.Pop(); st.Push(x); }
                o.Add($"{s} {st.Count}");
                break;
            }
            default:
            {
                string e = PostFix(r, 4);
                code = PK.CHead + stack + $"int main(void) {{\n    char *e = \"{e}\";\n    int i, a, b, maior = 0;\n    for (i = 0; e[i] != '\\0'; i++) {{\n        if (e[i] >= '0' && e[i] <= '9') {{\n            push(e[i] - '0');\n        }} else if (e[i] != ' ') {{\n            b = pop();\n            a = pop();\n            push(e[i] == '+' ? a + b : e[i] == '-' ? a - b : a * b);\n        }}\n        if (topo > maior) {{\n            maior = topo;\n        }}\n    }}\n    printf(\"%d %d\\n\", pop(), maior);\n    return 0;\n}}\n";
                int maior = 0, depth = 0; foreach (var tk in e.Split(' ')) { if (char.IsDigit(tk[0])) depth++; else depth--; maior = Math.Max(maior, depth); }
                o.Add($"{EvalPost(e)} {maior}");
                break;
            }
        }
        return PK.Predict(L.F("A pilha começa vazia e o programa usa os valores {0}, {1} e {2}.", a, b, c), code, string.Join("\n", o),
            L.T("Simule a pilha desenhando-a de baixo para cima. `pop` remove e devolve o topo; `push` coloca no topo. Para um operador, o primeiro `pop` dá o operando da direita."));
    }

    // ---------- c-17 ----------
    static string Bin(uint v, int w) => Convert.ToString(v, 2).PadLeft(w, '0');

    static Exercise Write17(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Leia um inteiro sem sinal `n` (`%u`). Usando só operadores de bits, mostre `Dobro: <n*2>` (deslocamento), `Metade: <n/2>`, `Quadruplo: <n*4>` e `Ultimo bit: <0 ou 1>` (o bit menos significativo).");
                sol = "#include <stdio.h>\n\nint main(void) {\n    unsigned int n;\n    scanf(\"%u\", &n);\n    printf(\"Dobro: %u\\n\", n << 1);\n    printf(\"Metade: %u\\n\", n >> 1);\n    printf(\"Quadruplo: %u\\n\", n << 2);\n    printf(\"Ultimo bit: %u\\n\", n & 1u);\n    return 0;\n}\n";
                gen = g => g.Next(0, 5000) + "\n"; oracle = i => { uint n = uint.Parse(i.Trim()); return $"Dobro: {n << 1}\nMetade: {n >> 1}\nQuadruplo: {n << 2}\nUltimo bit: {n & 1}"; };
                why = L.T("`n << k` multiplica por 2^k e `n >> k` divide por 2^k (inteiro). `n & 1` isola o último bit: 1 para ímpar, 0 para par.");
                break;
            case Difficulty.Medium:
                st = L.T("Leia `n` (sem sinal) e `k` (0 a 15). Mostre `Bit <k>: <0 ou 1>` (o valor do bit `k` de `n`), `Ligado: <n com o bit k ligado>`, `Desligado: <n com o bit k desligado>` e `Invertido: <n com o bit k invertido>`.");
                sol = "#include <stdio.h>\n\nint main(void) {\n    unsigned int n, mascara;\n    int k;\n    scanf(\"%u %d\", &n, &k);\n    mascara = 1u << k;\n    printf(\"Bit %d: %u\\n\", k, (n >> k) & 1u);\n    printf(\"Ligado: %u\\n\", n | mascara);\n    printf(\"Desligado: %u\\n\", n & ~mascara);\n    printf(\"Invertido: %u\\n\", n ^ mascara);\n    return 0;\n}\n";
                gen = g => $"{g.Next(0, 70000)} {g.Next(0, 16)}\n"; oracle = i => { var t = i.Split(' '); uint n = uint.Parse(t[0]); int k = int.Parse(t[1]); uint m = 1u << k; return $"Bit {k}: {(n >> k) & 1}\nLigado: {n | m}\nDesligado: {n & ~m}\nInvertido: {n ^ m}"; };
                why = L.T("Crie uma máscara com só o bit `k` ligado (`1u << k`). `|` liga, `& ~máscara` desliga, `^` inverte e `(n >> k) & 1` lê o bit.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia `n` (0 a 255). Mostre `Binario: <n em 8 bits>`, `Invertido: <os 8 bits de n lidos de trás para frente, em binário de 8 bits>` e `Valor: <o número decimal correspondente a esses bits invertidos>`.");
                sol = "#include <stdio.h>\n\nint main(void) {\n    unsigned int n, inv = 0;\n    int i;\n    scanf(\"%u\", &n);\n    printf(\"Binario: \");\n    for (i = 7; i >= 0; i--) {\n        printf(\"%u\", (n >> i) & 1u);\n    }\n    printf(\"\\n\");\n    for (i = 0; i < 8; i++) {\n        inv = (inv << 1) | ((n >> i) & 1u);\n    }\n    printf(\"Invertido: \");\n    for (i = 7; i >= 0; i--) {\n        printf(\"%u\", (inv >> i) & 1u);\n    }\n    printf(\"\\n\");\n    printf(\"Valor: %u\\n\", inv);\n    return 0;\n}\n";
                gen = g => g.Next(0, 256) + "\n";
                oracle = i => { uint n = uint.Parse(i.Trim()); uint inv = 0; for (int k = 0; k < 8; k++) inv = (inv << 1) | ((n >> k) & 1); return $"Binario: {Bin(n, 8)}\nInvertido: {Bin(inv, 8)}\nValor: {inv}"; };
                why = L.T("Para inverter os bits: leia o bit `i` de `n` (do menos para o mais significativo) e empurre-o para a direita de `inv` com `(inv << 1) | bit`.");
                break;
            default:
                st = L.T("Leia `n` (1 a 9, ímpar) e depois `n` inteiros sem sinal. Todos aparecem **duas vezes**, exceto um que aparece uma vez só. Mostre `Unico: <o valor que aparece uma vez>` (sem ordenar nem contar: use XOR) e, na linha seguinte, `Distancia: <quantos bits diferem entre o primeiro e o último número da lista>` (a distância de Hamming: o número de bits 1 de `a ^ b`).");
                sol = "#include <stdio.h>\n\nint main(void) {\n    unsigned int v[9];\n    unsigned int x = 0, d;\n    int n, i, bits = 0;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%u\", &v[i]);\n        x ^= v[i];\n    }\n    printf(\"Unico: %u\\n\", x);\n    d = v[0] ^ v[n - 1];\n    while (d != 0) {\n        d = d & (d - 1);\n        bits++;\n    }\n    printf(\"Distancia: %d\\n\", bits);\n    return 0;\n}\n";
                gen = g => { int pairs = g.Next(0, 4); var vals = PK.Distinct(g, pairs + 1, 1, 1000); var list = new List<int>(); list.Add(vals[0]); foreach (var p in vals.Skip(1)) { list.Add(p); list.Add(p); } list = list.OrderBy(_ => g.Next()).ToList(); return $"{list.Count}\n{PK.Join(list)}\n"; };
                oracle = i => { var t = PK.Ints(i); int n = t[0]; var v = t.Skip(1).Take(n).ToArray(); int x = 0; foreach (var y in v) x ^= y; return $"Unico: {x}\nDistancia: {System.Numerics.BitOperations.PopCount((uint)(v[0] ^ v[n - 1]))}"; };
                why = L.T("`a ^ a = 0` e `a ^ 0 = a`: ao fazer XOR de todos, os pares se anulam e sobra o número único. Para contar bits 1, `d & (d - 1)` apaga o bit 1 mais baixo a cada volta.");
                break;
        }
        return PK.Write(st, PK.CStarter, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict17(Difficulty d, Random r)
    {
        uint a = (uint)r.Next(10, 250), b = (uint)r.Next(10, 250); int k = r.Next(1, 5), j = r.Next(0, 6); var o = new List<string>(); string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = PK.CMain($"    unsigned int a = {a};\n    unsigned int b = {b};\n    printf(\"%u %u %u\\n\", a & b, a | b, a ^ b);\n    printf(\"%u %u\\n\", a << {k}, a >> {k});\n");
                o.Add($"{a & b} {a | b} {a ^ b}"); o.Add($"{a << k} {a >> k}");
                break;
            case Difficulty.Medium:
                code = PK.CMain($"    unsigned int a = {a};\n    unsigned int m = 1u << {j};\n    printf(\"%u\\n\", m);\n    printf(\"%u %u %u\\n\", a | m, a & ~m, a ^ m);\n    printf(\"%u\\n\", (a >> {j}) & 1u);\n");
                { uint m = 1u << j; o.Add(m.ToString()); o.Add($"{a | m} {a & ~m} {a ^ m}"); o.Add(((a >> j) & 1).ToString()); }
                break;
            case Difficulty.Hard:
                code = PK.CMain($"    unsigned int n = {a};\n    int bits = 0;\n    while (n != 0) {{\n        n = n & (n - 1);\n        bits++;\n    }}\n    printf(\"%d\\n\", bits);\n    n = {b};\n    printf(\"%u\\n\", n & (~n + 1u));\n    printf(\"%d\\n\", (n & (n - 1)) == 0);\n");
                { o.Add(System.Numerics.BitOperations.PopCount(a).ToString()); o.Add((b & (~b + 1)).ToString()); o.Add(((b & (b - 1)) == 0 ? 1 : 0).ToString()); }
                break;
            default:
                code = PK.CMain($"    unsigned int x = {a};\n    unsigned int y = {b};\n    x = x ^ y;\n    y = x ^ y;\n    x = x ^ y;\n    printf(\"%u %u\\n\", x, y);\n    printf(\"%u\\n\", (x & 15u) << 4 | (y >> {k}));\n    printf(\"%u\\n\", ~x & 255u);\n");
                { uint x = b, y = a; o.Add($"{x} {y}"); o.Add((((x & 15u) << 4) | (y >> k)).ToString()); o.Add((~x & 255u).ToString()); }
                break;
        }
        return PK.Predict(L.F("O programa trabalha com os números {0} e {1}.", a, b), code, string.Join("\n", o),
            L.T("Escreva os números em binário para ver os bits. `&` mantém só os bits 1 em ambos, `|` os 1 em qualquer um, `^` os diferentes; `<< k` multiplica por 2^k e `>> k` divide. XOR três vezes troca dois valores sem variável extra."));
    }

    // ---------- c-18 ----------
    static Exercise Write18(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        string ms = "long mergeSort(int v[], int tmp[], int ini, int fim, long *comp) {\n    int meio, i, j, k;\n    long inv = 0;\n    if (fim - ini < 2) {\n        return 0;\n    }\n    meio = (ini + fim) / 2;\n    inv += mergeSort(v, tmp, ini, meio, comp);\n    inv += mergeSort(v, tmp, meio, fim, comp);\n    i = ini;\n    j = meio;\n    k = ini;\n    while (i < meio && j < fim) {\n        (*comp)++;\n        if (v[i] <= v[j]) {\n            tmp[k++] = v[i++];\n        } else {\n            tmp[k++] = v[j++];\n            inv += meio - i;\n        }\n    }\n    while (i < meio) {\n        tmp[k++] = v[i++];\n    }\n    while (j < fim) {\n        tmp[k++] = v[j++];\n    }\n    for (k = ini; k < fim; k++) {\n        v[k] = tmp[k];\n    }\n    return inv;\n}\n\n";
        string rd = "    int v[20];\n    int tmp[20];\n    int n, i;\n    long comp = 0, inv;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &v[i]);\n    }\n    inv = mergeSort(v, tmp, 0, n, &comp);\n";
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Leia `n1` e depois `n1` inteiros **em ordem crescente**; depois `n2` e `n2` inteiros também em ordem crescente (`n1` e `n2` entre 1 e 8). Faça a **intercalação** (merge) das duas listas, sem ordenar tudo de novo, e mostre `Intercalado: <os n1+n2 números em ordem crescente>`. Em empate, o elemento da primeira lista vem antes.");
                sol = "#include <stdio.h>\n\nint main(void) {\n    int a[8];\n    int b[8];\n    int n1, n2, i, j, k;\n    scanf(\"%d\", &n1);\n    for (i = 0; i < n1; i++) {\n        scanf(\"%d\", &a[i]);\n    }\n    scanf(\"%d\", &n2);\n    for (j = 0; j < n2; j++) {\n        scanf(\"%d\", &b[j]);\n    }\n    i = 0;\n    j = 0;\n    k = 0;\n    printf(\"Intercalado:\");\n    while (i < n1 || j < n2) {\n        if (j >= n2 || (i < n1 && a[i] <= b[j])) {\n            printf(\" %d\", a[i]);\n            i++;\n        } else {\n            printf(\" %d\", b[j]);\n            j++;\n        }\n        k++;\n    }\n    printf(\"\\n\");\n    return 0;\n}\n";
                gen = g => { int n1 = g.Next(1, 8), n2 = g.Next(1, 8); return $"{n1}\n{PK.Join(PK.Rand(g, n1, 1, 30).OrderBy(x => x))}\n{n2}\n{PK.Join(PK.Rand(g, n2, 1, 30).OrderBy(x => x))}\n"; };
                oracle = i => { var t = PK.Ints(i); int n1 = t[0]; var a = t.Skip(1).Take(n1); int n2 = t[n1 + 1]; var b = t.Skip(n1 + 2).Take(n2); return "Intercalado: " + PK.Join(a.Concat(b).OrderBy(x => x)); };
                why = L.T("Dois índices, um em cada lista: a cada passo copie o menor dos dois elementos atuais e avance só aquele índice. Quando uma lista acaba, copie o resto da outra.");
                break;
            case Difficulty.Medium:
                st = L.T("Leia `n` (1 a 16) e `n` inteiros. Ordene com **merge sort** (dividir ao meio, ordenar cada metade e intercalar) e mostre `Ordenado: <números>` e `Comparacoes: <quantas comparações entre elementos foram feitas nas intercalações>`. Conte uma comparação (`v[i] <= v[j]`) cada vez que as duas metades ainda têm elementos; a divisão do meio é `(ini + fim) / 2` com a faixa `[ini, fim)`.");
                sol = "#include <stdio.h>\n\n" + ms + "int main(void) {\n" + rd + "    printf(\"Ordenado:\");\n    for (i = 0; i < n; i++) {\n        printf(\" %d\", v[i]);\n    }\n    printf(\"\\n\");\n    printf(\"Comparacoes: %ld\\n\", comp);\n    return 0;\n}\n";
                gen = g => NList(g, 14, 1, 40);
                oracle = i => { var v = PK.Ints(i).Skip(1).ToArray(); long comp = 0; MergeCount(v, new int[v.Length], 0, v.Length, ref comp); return $"Ordenado: {PK.Join(v)}\nComparacoes: {comp}"; };
                why = L.T("Divida a faixa `[ini, fim)` ao meio, ordene cada metade recursivamente e intercale com um vetor auxiliar. Cada passo do laço de intercalação que compara `v[i]` com `v[j]` é uma comparação.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia `n` (1 a 20) e `n` inteiros. Mostre `Inversoes: <quantos pares (i, j) com i < j têm v[i] > v[j]>` (valores iguais não contam) e `Ordenado: <números em ordem crescente>`. Calcule as inversões **durante o merge sort**: quando um elemento da metade direita passa na frente de elementos da esquerda, ele \"pula\" todos os que ainda restam na esquerda.");
                sol = "#include <stdio.h>\n\n" + ms + "int main(void) {\n" + rd + "    printf(\"Inversoes: %ld\\n\", inv);\n    printf(\"Ordenado:\");\n    for (i = 0; i < n; i++) {\n        printf(\" %d\", v[i]);\n    }\n    printf(\"\\n\");\n    return 0;\n}\n";
                gen = g => NList(g, 18, 1, 25);
                oracle = i => { var v = PK.Ints(i).Skip(1).ToArray(); long inv = 0; for (int a = 0; a < v.Length; a++) for (int b = a + 1; b < v.Length; b++) if (v[a] > v[b]) inv++; return $"Inversoes: {inv}\nOrdenado: {PK.Join(v.OrderBy(x => x))}"; };
                why = L.T("Na intercalação, quando `v[j]` (direita) é menor que `v[i]` (esquerda), ele é menor que todos os elementos de `v[i]` até o fim da metade esquerda: some `meio - i` inversões de uma vez.");
                break;
            default:
                st = L.T("Leia `n` (1 a 15) e `n` inteiros positivos. Conte os **pares importantes**: pares de posições `i < j` com `v[i] > 2 * v[j]`. Mostre `Pares: <quantidade>`. Com um laço duplo funciona; tente fazer em `O(n log n)`, contando durante o merge sort ANTES de intercalar cada par de metades (as duas metades já estão ordenadas).");
                sol = "#include <stdio.h>\n\nlong contar(int v[], int tmp[], int ini, int fim) {\n    int meio, i, j, k;\n    long total = 0;\n    if (fim - ini < 2) {\n        return 0;\n    }\n    meio = (ini + fim) / 2;\n    total += contar(v, tmp, ini, meio);\n    total += contar(v, tmp, meio, fim);\n    j = meio;\n    for (i = ini; i < meio; i++) {\n        while (j < fim && v[i] > 2L * v[j]) {\n            j++;\n        }\n        total += j - meio;\n    }\n    i = ini;\n    j = meio;\n    k = ini;\n    while (i < meio && j < fim) {\n        if (v[i] <= v[j]) {\n            tmp[k++] = v[i++];\n        } else {\n            tmp[k++] = v[j++];\n        }\n    }\n    while (i < meio) {\n        tmp[k++] = v[i++];\n    }\n    while (j < fim) {\n        tmp[k++] = v[j++];\n    }\n    for (k = ini; k < fim; k++) {\n        v[k] = tmp[k];\n    }\n    return total;\n}\n\nint main(void) {\n    int v[15];\n    int tmp[15];\n    int n, i;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &v[i]);\n    }\n    printf(\"Pares: %ld\\n\", contar(v, tmp, 0, n));\n    return 0;\n}\n";
                gen = g => NList(g, 14, 1, 30);
                oracle = i => { var v = PK.Ints(i).Skip(1).ToArray(); int c = 0; for (int a = 0; a < v.Length; a++) for (int b = a + 1; b < v.Length; b++) if (v[a] > 2 * v[b]) c++; return $"Pares: {c}"; };
                why = L.T("Com as duas metades já ordenadas, o ponteiro `j` só anda para a frente quando `i` cresce: para cada `i` da esquerda, os elementos da direita com `2*v[j] < v[i]` formam um prefixo da metade direita. Depois disso, intercale normalmente.");
                break;
        }
        return PK.Write(st, PK.CStarter, sol, PK.Tests(r, 7, gen, oracle), why);
    }

    static long MergeCount(int[] v, int[] tmp, int ini, int fim, ref long comp)
    {
        if (fim - ini < 2) return 0;
        int meio = (ini + fim) / 2; long inv = MergeCount(v, tmp, ini, meio, ref comp) + MergeCount(v, tmp, meio, fim, ref comp);
        int i = ini, j = meio, k = ini;
        while (i < meio && j < fim) { comp++; if (v[i] <= v[j]) tmp[k++] = v[i++]; else { tmp[k++] = v[j++]; inv += meio - i; } }
        while (i < meio) tmp[k++] = v[i++]; while (j < fim) tmp[k++] = v[j++];
        for (k = ini; k < fim; k++) v[k] = tmp[k];
        return inv;
    }

    static Exercise Predict18(Difficulty d, Random r)
    {
        var o = new List<string>(); string code;
        int n = r.Next(4, 8);
        var a = PK.Rand(r, n, 1, 20).OrderBy(x => x).ToArray(); var b = PK.Rand(r, n - 1, 1, 20).OrderBy(x => x).ToArray();
        string mergeFn = "int mescla(int a[], int na, int b[], int nb, int out[]) {\n    int i = 0, j = 0, k = 0;\n    while (i < na && j < nb) {\n        if (a[i] <= b[j]) {\n            out[k++] = a[i++];\n        } else {\n            out[k++] = b[j++];\n        }\n    }\n    while (i < na) {\n        out[k++] = a[i++];\n    }\n    while (j < nb) {\n        out[k++] = b[j++];\n    }\n    return k;\n}\n\n";
        switch (d)
        {
            case Difficulty.Easy:
            {
                int p = r.Next(1, 4);
                code = PK.CHead + mergeFn + $"int main(void) {{\n    int a[{n}] = {{{PK.Join(a, ", ")}}};\n    int b[{n - 1}] = {{{PK.Join(b, ", ")}}};\n    int c[{2 * n}];\n    int k = mescla(a, {n}, b, {n - 1}, c);\n    int i;\n    for (i = 0; i < k; i += {p}) {{\n        printf(\"%d \", c[i]);\n    }}\n    printf(\"\\n%d\\n\", k);\n    return 0;\n}}\n";
                var c = a.Concat(b).OrderBy(x => x).ToArray(); var sb = new StringBuilder(); for (int i = 0; i < c.Length; i += p) sb.Append(c[i] + " "); o.Add(sb.ToString()); o.Add(c.Length.ToString());
                break;
            }
            case Difficulty.Medium:
            {
                var v = PK.Rand(r, 4, 1, 9);
                code = PK.CHead + "void mostra(int v[], int ini, int fim) {\n    int i;\n    for (i = ini; i < fim; i++) {\n        printf(\"%d \", v[i]);\n    }\n    printf(\"\\n\");\n}\n\n" + "void ordena(int v[], int tmp[], int ini, int fim) {\n    int meio, i, j, k;\n    if (fim - ini < 2) {\n        return;\n    }\n    meio = (ini + fim) / 2;\n    ordena(v, tmp, ini, meio);\n    ordena(v, tmp, meio, fim);\n    i = ini;\n    j = meio;\n    k = ini;\n    while (i < meio && j < fim) {\n        tmp[k++] = v[i] <= v[j] ? v[i++] : v[j++];\n    }\n    while (i < meio) {\n        tmp[k++] = v[i++];\n    }\n    while (j < fim) {\n        tmp[k++] = v[j++];\n    }\n    for (k = ini; k < fim; k++) {\n        v[k] = tmp[k];\n    }\n    mostra(v, ini, fim);\n}\n\n" + $"int main(void) {{\n    int v[4] = {{{PK.Join(v, ", ")}}};\n    int tmp[4];\n    ordena(v, tmp, 0, 4);\n    return 0;\n}}\n";
                var w = v.ToArray(); void Sort(int ini, int fim) { if (fim - ini < 2) return; int meio = (ini + fim) / 2; Sort(ini, meio); Sort(meio, fim); Array.Sort(w, ini, fim - ini); o.Add(string.Join("", w.Skip(ini).Take(fim - ini).Select(x => x + " "))); }
                Sort(0, 4);
                break;
            }
            case Difficulty.Hard:
            {
                var v = PK.Rand(r, 5, 1, 9);
                code = PK.CHead + "long cont = 0;\n\nvoid ordena(int v[], int tmp[], int ini, int fim) {\n    int meio, i, j, k;\n    if (fim - ini < 2) {\n        return;\n    }\n    meio = (ini + fim) / 2;\n    ordena(v, tmp, ini, meio);\n    ordena(v, tmp, meio, fim);\n    i = ini;\n    j = meio;\n    k = ini;\n    while (i < meio && j < fim) {\n        if (v[i] <= v[j]) {\n            tmp[k++] = v[i++];\n        } else {\n            cont += meio - i;\n            tmp[k++] = v[j++];\n        }\n    }\n    while (i < meio) {\n        tmp[k++] = v[i++];\n    }\n    while (j < fim) {\n        tmp[k++] = v[j++];\n    }\n    for (k = ini; k < fim; k++) {\n        v[k] = tmp[k];\n    }\n}\n\n" + $"int main(void) {{\n    int v[5] = {{{PK.Join(v, ", ")}}};\n    int tmp[5];\n    int i;\n    ordena(v, tmp, 0, 5);\n    printf(\"%ld\\n\", cont);\n    for (i = 0; i < 5; i++) {{\n        printf(\"%d \", v[i]);\n    }}\n    printf(\"\\n\");\n    return 0;\n}}\n";
                int inv = 0; for (int i = 0; i < 5; i++) for (int j = i + 1; j < 5; j++) if (v[i] > v[j]) inv++;
                o.Add(inv.ToString()); o.Add(string.Join("", v.OrderBy(x => x).Select(x => x + " ")));
                break;
            }
            default:
            {
                var v = PK.Rand(r, 6, 1, 9);
                code = PK.CHead + "int meio(int ini, int fim) {\n    return (ini + fim) / 2;\n}\n\nint prof(int ini, int fim) {\n    if (fim - ini < 2) {\n        return 1;\n    }\n    int m = meio(ini, fim);\n    int a = prof(ini, m);\n    int b = prof(m, fim);\n    return 1 + (a > b ? a : b);\n}\n\nint chamadas = 0;\n\nvoid divide(int ini, int fim) {\n    chamadas++;\n    if (fim - ini < 2) {\n        return;\n    }\n    divide(ini, meio(ini, fim));\n    divide(meio(ini, fim), fim);\n}\n\n" + $"int main(void) {{\n    divide(0, {v[0] + 4});\n    printf(\"%d %d\\n\", chamadas, prof(0, {v[0] + 4}));\n    return 0;\n}}\n";
                int calls = 0; int Prof(int ini, int fim) { if (fim - ini < 2) return 1; int m = (ini + fim) / 2; return 1 + Math.Max(Prof(ini, m), Prof(m, fim)); } void Div(int ini, int fim) { calls++; if (fim - ini < 2) return; Div(ini, (ini + fim) / 2); Div((ini + fim) / 2, fim); }
                Div(0, v[0] + 4); o.Add($"{calls} {Prof(0, v[0] + 4)}");
                break;
            }
        }
        return PK.Predict(d == Difficulty.Genius ? L.T("O programa só divide faixas ao meio (sem ordenar nada).") : L.T("O programa faz parte do merge sort."), code, string.Join("\n", o),
            L.T("Acompanhe as divisões ao meio (`(ini + fim) / 2`) e as intercalações de baixo para cima: cada intercalação junta duas metades já ordenadas, sempre pegando o menor dos dois primeiros elementos. Em empate vale o da esquerda."));
    }

    // ---------- c-19 ----------
    static Exercise Write19(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Leia `n` (1 a 6), `m` (2 a 20, o tamanho da tabela) e `n` palavras de letras minúsculas. Para cada palavra calcule o hash `h = (h * 31 + letra) % m` começando em `h = 0` (use o código ASCII da letra) e mostre `<palavra>: <h>`. Isso diz em que balde cada palavra cairia.");
                sol = "#include <stdio.h>\n\nint main(void) {\n    int n, m, i, j;\n    char p[31];\n    long h;\n    scanf(\"%d %d\", &n, &m);\n    for (i = 0; i < n; i++) {\n        scanf(\"%s\", p);\n        h = 0;\n        for (j = 0; p[j] != '\\0'; j++) {\n            h = (h * 31 + p[j]) % m;\n        }\n        printf(\"%s: %ld\\n\", p, h);\n    }\n    return 0;\n}\n";
                gen = g => { int n = g.Next(1, 6); return $"{n} {g.Next(2, 20)}\n{string.Join(" ", Enumerable.Range(0, n).Select(_ => PK.Pick(g, Words)))}\n"; };
                oracle = i => { var t = i.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries); int m = int.Parse(t[1]); return string.Join("\n", t.Skip(2).Select(w => { long h = 0; foreach (char c in w) h = (h * 31 + c) % m; return $"{w}: {h}"; })); };
                why = L.T("Cada letra entra no hash por `h * 31 + letra`; o `% m` a cada passo mantém o valor pequeno e o resultado final é o índice do balde (de 0 a m-1).");
                break;
            case Difficulty.Medium:
                st = L.T("Leia `m` (5 a 11, o tamanho da tabela), `n` (1 a `m`) e `n` inteiros positivos distintos. Insira cada um numa tabela de tamanho `m` com **endereçamento aberto e sondagem linear**: posição inicial `chave % m`; se estiver ocupada, tente a seguinte (voltando ao 0 depois da última). Mostre a tabela inteira, uma linha `<posição>: <chave ou ->` para cada posição de 0 a m-1.");
                sol = "#include <stdio.h>\n\nint main(void) {\n    int tab[11];\n    int m, n, i, x, p;\n    scanf(\"%d %d\", &m, &n);\n    for (i = 0; i < m; i++) {\n        tab[i] = 0;\n    }\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &x);\n        p = x % m;\n        while (tab[p] != 0) {\n            p = (p + 1) % m;\n        }\n        tab[p] = x;\n    }\n    for (i = 0; i < m; i++) {\n        if (tab[i] == 0) {\n            printf(\"%d: -\\n\", i);\n        } else {\n            printf(\"%d: %d\\n\", i, tab[i]);\n        }\n    }\n    return 0;\n}\n";
                gen = g => { int m = g.Next(5, 12); int n = g.Next(1, m + 1); return $"{m} {n}\n{PK.Join(PK.Distinct(g, n, 1, 99))}\n"; };
                oracle = i => { var t = PK.Ints(i); int m = t[0], n = t[1]; var tab = new int[m]; foreach (var x in t.Skip(2).Take(n)) { int p = x % m; while (tab[p] != 0) p = (p + 1) % m; tab[p] = x; } return string.Join("\n", Enumerable.Range(0, m).Select(k => $"{k}: {(tab[k] == 0 ? "-" : tab[k].ToString())}")); };
                why = L.T("Numa colisão, a sondagem linear procura a próxima posição livre andando de 1 em 1 (com volta ao início). Como `n <= m`, sempre há lugar. Usamos 0 como \"vazio\" porque as chaves são positivas.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia `m` (3 a 8, o número de baldes) e `n` (1 a 12) e depois `n` inteiros positivos. Guarde cada chave no balde `chave % m` (encadeamento: cada balde é uma lista; novas chaves entram no **fim** do balde). Mostre uma linha `Balde <i>: <chaves do balde na ordem de chegada, ou ->` para cada balde de 0 a m-1 e `Colisoes: <quantas chaves caíram num balde que já tinha alguma chave>`.");
                sol = "#include <stdio.h>\n\nint main(void) {\n    int bal[8][12];\n    int qtd[8];\n    int m, n, i, j, x, b, col = 0;\n    scanf(\"%d %d\", &m, &n);\n    for (i = 0; i < m; i++) {\n        qtd[i] = 0;\n    }\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &x);\n        b = x % m;\n        if (qtd[b] > 0) {\n            col++;\n        }\n        bal[b][qtd[b]] = x;\n        qtd[b]++;\n    }\n    for (i = 0; i < m; i++) {\n        printf(\"Balde %d:\", i);\n        if (qtd[i] == 0) {\n            printf(\" -\");\n        }\n        for (j = 0; j < qtd[i]; j++) {\n            printf(\" %d\", bal[i][j]);\n        }\n        printf(\"\\n\");\n    }\n    printf(\"Colisoes: %d\\n\", col);\n    return 0;\n}\n";
                gen = g => { int m = g.Next(3, 9), n = g.Next(1, 13); return $"{m} {n}\n{PK.Join(PK.Rand(g, n, 1, 60))}\n"; };
                oracle = i => { var t = PK.Ints(i); int m = t[0], n = t[1]; var b = Enumerable.Range(0, m).Select(_ => new List<int>()).ToArray(); int col = 0; foreach (var x in t.Skip(2).Take(n)) { if (b[x % m].Count > 0) col++; b[x % m].Add(x); } return string.Join("\n", b.Select((l, k) => $"Balde {k}: {(l.Count == 0 ? "-" : PK.Join(l))}")) + $"\nColisoes: {col}"; };
                why = L.T("A função de hash `chave % m` escolhe o balde; chaves diferentes que caem no mesmo balde formam uma lista. Uma colisão acontece sempre que o balde escolhido já não está vazio.");
                break;
            default:
                st = L.T("Leia `n` (1 a 15) e `n` palavras. Use `m = 7` baldes com hash `soma dos códigos ASCII das letras % 7`. Mostre, **na ordem em que cada palavra apareceu pela primeira vez**, uma linha `<palavra>: <contagem> (balde <b>)`, depois `Mais frequente: <palavra> (<contagem>)` (empate: a que apareceu primeiro) e `Balde mais cheio: <b> com <k> palavras distintas` (empate: o menor número de balde).");
                sol = "#include <stdio.h>\n#include <string.h>\n\nint main(void) {\n    char pal[15][31];\n    int cont[15];\n    int balde[15];\n    int dist = 0, n, i, j, k, s, achou, melhor = 0, bm = 0;\n    int porBalde[7];\n    char w[31];\n    for (i = 0; i < 7; i++) {\n        porBalde[i] = 0;\n    }\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%s\", w);\n        achou = -1;\n        for (j = 0; j < dist; j++) {\n            if (strcmp(pal[j], w) == 0) {\n                achou = j;\n            }\n        }\n        if (achou >= 0) {\n            cont[achou]++;\n        } else {\n            s = 0;\n            for (k = 0; w[k] != '\\0'; k++) {\n                s += w[k];\n            }\n            strcpy(pal[dist], w);\n            cont[dist] = 1;\n            balde[dist] = s % 7;\n            porBalde[s % 7]++;\n            dist++;\n        }\n    }\n    for (i = 0; i < dist; i++) {\n        printf(\"%s: %d (balde %d)\\n\", pal[i], cont[i], balde[i]);\n        if (cont[i] > cont[melhor]) {\n            melhor = i;\n        }\n    }\n    for (i = 1; i < 7; i++) {\n        if (porBalde[i] > porBalde[bm]) {\n            bm = i;\n        }\n    }\n    printf(\"Mais frequente: %s (%d)\\n\", pal[melhor], cont[melhor]);\n    printf(\"Balde mais cheio: %d com %d palavras distintas\\n\", bm, porBalde[bm]);\n    return 0;\n}\n";
                gen = g => { int n = g.Next(1, 14); return $"{n}\n{string.Join(" ", Enumerable.Range(0, n).Select(_ => PK.Pick(g, Words)))}\n"; };
                oracle = i =>
                {
                    var t = i.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList(); var order = new List<string>(); var cnt = new Dictionary<string, int>();
                    foreach (var w in t) { if (!cnt.ContainsKey(w)) { cnt[w] = 0; order.Add(w); } cnt[w]++; }
                    int B(string w) => w.Sum(c => (int)c) % 7; var per = new int[7]; foreach (var w in order) per[B(w)]++;
                    var best = order[0]; foreach (var w in order) if (cnt[w] > cnt[best]) best = w; int bm = 0; for (int q = 1; q < 7; q++) if (per[q] > per[bm]) bm = q;
                    return string.Join("\n", order.Select(w => $"{w}: {cnt[w]} (balde {B(w)})")) + $"\nMais frequente: {best} ({cnt[best]})\nBalde mais cheio: {bm} com {per[bm]} palavras distintas";
                };
                why = L.T("O hash (soma dos códigos % 7) só decide o balde; para contar, procure a palavra entre as já vistas. Guarde a ordem de chegada para imprimir e compare com `>` estrito para manter o primeiro nos empates.");
                break;
        }
        return PK.Write(st, PK.CStarter, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict19(Difficulty d, Random r)
    {
        var o = new List<string>(); string code; int m = r.Next(5, 9);
        switch (d)
        {
            case Difficulty.Easy:
            {
                string w = PK.Pick(r, Words);
                code = PK.CMain($"    char *p = \"{w}\";\n    int i;\n    long h = 0;\n    for (i = 0; p[i] != '\\0'; i++) {{\n        h = (h * 31 + p[i]) % {m};\n        printf(\"%ld \", h);\n    }}\n    printf(\"\\n\");\n");
                long h = 0; var sb = new StringBuilder(); foreach (char c in w) { h = (h * 31 + c) % m; sb.Append(h + " "); } o.Add(sb.ToString());
                break;
            }
            case Difficulty.Medium:
            {
                var keys = PK.Distinct(r, 5, 1, 40);
                code = PK.CMain($"    int chaves[5] = {{{PK.Join(keys, ", ")}}};\n    int tab[{m}] = {{0}};\n    int i, p;\n    for (i = 0; i < 5; i++) {{\n        p = chaves[i] % {m};\n        while (tab[p] != 0) {{\n            p = (p + 1) % {m};\n        }}\n        tab[p] = chaves[i];\n    }}\n    for (i = 0; i < {m}; i++) {{\n        printf(\"%d \", tab[i]);\n    }}\n    printf(\"\\n\");\n");
                var tab = new int[m]; foreach (var x in keys) { int p = x % m; while (tab[p] != 0) p = (p + 1) % m; tab[p] = x; } o.Add(string.Join("", tab.Select(x => x + " ")));
                break;
            }
            case Difficulty.Hard:
            {
                var keys = PK.Rand(r, 7, 1, 50);
                code = PK.CMain($"    int chaves[7] = {{{PK.Join(keys, ", ")}}};\n    int qtd[{m}] = {{0}};\n    int i, maior = 0, col = 0;\n    for (i = 0; i < 7; i++) {{\n        if (qtd[chaves[i] % {m}] > 0) {{\n            col++;\n        }}\n        qtd[chaves[i] % {m}]++;\n    }}\n    for (i = 1; i < {m}; i++) {{\n        if (qtd[i] > qtd[maior]) {{\n            maior = i;\n        }}\n    }}\n    printf(\"%d %d %d\\n\", col, maior, qtd[maior]);\n");
                var q = new int[m]; int col = 0; foreach (var x in keys) { if (q[x % m] > 0) col++; q[x % m]++; } int mx = 0; for (int i = 1; i < m; i++) if (q[i] > q[mx]) mx = i; o.Add($"{col} {mx} {q[mx]}");
                break;
            }
            default:
            {
                var keys = PK.Distinct(r, 6, 1, 60);
                code = PK.CMain($"    int chaves[6] = {{{PK.Join(keys, ", ")}}};\n    int tab[{m + 3}] = {{0}};\n    int i, p, passos = 0, pior = 0, s;\n    for (i = 0; i < 6; i++) {{\n        p = chaves[i] % {m + 3};\n        s = 1;\n        while (tab[p] != 0) {{\n            p = (p + 1) % {m + 3};\n            s++;\n        }}\n        tab[p] = chaves[i];\n        passos += s;\n        if (s > pior) {{\n            pior = s;\n        }}\n    }}\n    printf(\"%d %d\\n\", passos, pior);\n    for (i = 0; i < {m + 3}; i++) {{\n        printf(\"%d\", tab[i] > 0);\n    }}\n    printf(\"\\n\");\n");
                int M = m + 3; var tab = new int[M]; int passos = 0, pior = 0; foreach (var x in keys) { int p = x % M, s = 1; while (tab[p] != 0) { p = (p + 1) % M; s++; } tab[p] = x; passos += s; pior = Math.Max(pior, s); }
                o.Add($"{passos} {pior}"); o.Add(string.Join("", tab.Select(x => x > 0 ? "1" : "0")));
                break;
            }
        }
        return PK.Predict(L.F("A tabela do programa tem tamanho {0}.", d == Difficulty.Genius ? m + 3 : m), code, string.Join("\n", o),
            L.T("Calcule o balde de cada chave com `chave % tamanho`. Se a posição já tem alguém, a sondagem linear tenta a seguinte (com volta ao início). Acompanhe a tabela depois de cada inserção."));
    }

    // ---------- c-20 ----------
    static string Graph(Random g, int n, int m)
    {
        var edges = new HashSet<(int, int)>(); int tries = 0;
        while (edges.Count < m && tries++ < 200) { int a = g.Next(n), b = g.Next(n); if (a == b) continue; edges.Add((Math.Min(a, b), Math.Max(a, b))); }
        return $"{n} {edges.Count}\n" + string.Join("\n", edges.Select(e => $"{e.Item1} {e.Item2}")) + (edges.Count > 0 ? "\n" : "");
    }

    static List<int>[] Adj(int[] t)
    {
        int n = t[0], m = t[1]; var adj = Enumerable.Range(0, n).Select(_ => new List<int>()).ToArray();
        for (int i = 0; i < m; i++) { int a = t[2 + 2 * i], b = t[3 + 2 * i]; adj[a].Add(b); adj[b].Add(a); }
        foreach (var l in adj) l.Sort(); return adj;
    }

    static Exercise Write20(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        string rd = "    int adj[8][8];\n    int n, m, i, j, a, b;\n    scanf(\"%d %d\", &n, &m);\n    for (i = 0; i < n; i++) {\n        for (j = 0; j < n; j++) {\n            adj[i][j] = 0;\n        }\n    }\n    for (i = 0; i < m; i++) {\n        scanf(\"%d %d\", &a, &b);\n        adj[a][b] = 1;\n        adj[b][a] = 1;\n    }\n";
        string graphIn = L.T("Leia `n` (2 a 8, vértices numerados de 0 a n-1) e `m` (arestas) e depois `m` linhas `a b`, cada uma uma aresta **sem direção** entre `a` e `b`.");
        switch (d)
        {
            case Difficulty.Easy:
                st = graphIn + " " + L.T("Guarde numa matriz de adjacência e mostre uma linha `Grau <v>: <quantas arestas tocam v>` para cada vértice e `Maior grau: <vértice de maior grau; empate: o menor número>`.");
                sol = "#include <stdio.h>\n\nint main(void) {\n" + rd + "    {\n        int g, melhor = 0, gm = -1;\n        for (i = 0; i < n; i++) {\n            g = 0;\n            for (j = 0; j < n; j++) {\n                g += adj[i][j];\n            }\n            printf(\"Grau %d: %d\\n\", i, g);\n            if (g > gm) {\n                gm = g;\n                melhor = i;\n            }\n        }\n        printf(\"Maior grau: %d\\n\", melhor);\n    }\n    return 0;\n}\n";
                gen = g => { int n = g.Next(2, 9); return Graph(g, n, g.Next(0, n + 3)); };
                oracle = i => { var t = PK.Ints(i); var adj = Adj(t); int best = 0; for (int k = 1; k < t[0]; k++) if (adj[k].Count > adj[best].Count) best = k; return string.Join("\n", adj.Select((l, k) => $"Grau {k}: {l.Count}")) + $"\nMaior grau: {best}"; };
                why = L.T("Na matriz de adjacência, `adj[a][b] = 1` marca a aresta nos dois sentidos. O grau de um vértice é a soma da sua linha.");
                break;
            case Difficulty.Medium:
                st = graphIn + " " + L.T("Faça uma **busca em largura** (BFS) a partir do vértice 0, visitando os vizinhos em ordem crescente de número, e mostre `Ordem: <vértices na ordem em que foram visitados>`. Só os vértices alcançáveis aparecem.");
                sol = "#include <stdio.h>\n\nint main(void) {\n" + rd + "    {\n        int fila[8];\n        int vis[8];\n        int ini = 0, fim = 0, v;\n        for (i = 0; i < n; i++) {\n            vis[i] = 0;\n        }\n        fila[fim++] = 0;\n        vis[0] = 1;\n        printf(\"Ordem:\");\n        while (ini < fim) {\n            v = fila[ini++];\n            printf(\" %d\", v);\n            for (j = 0; j < n; j++) {\n                if (adj[v][j] && !vis[j]) {\n                    vis[j] = 1;\n                    fila[fim++] = j;\n                }\n            }\n        }\n        printf(\"\\n\");\n    }\n    return 0;\n}\n";
                gen = g => { int n = g.Next(2, 9); return Graph(g, n, g.Next(1, n + 2)); };
                oracle = i => { var t = PK.Ints(i); var adj = Adj(t); var vis = new bool[t[0]]; var q = new Queue<int>(); var o = new List<int>(); q.Enqueue(0); vis[0] = true; while (q.Count > 0) { int v = q.Dequeue(); o.Add(v); foreach (var w in adj[v]) if (!vis[w]) { vis[w] = true; q.Enqueue(w); } } return "Ordem: " + PK.Join(o); };
                why = L.T("A fila (FIFO) garante que todos os vértices a uma distância k sejam visitados antes dos de distância k+1. Marque como visitado ao colocar na fila, para não entrar duas vezes.");
                break;
            case Difficulty.Hard:
                st = graphIn + " " + L.T("Leia também, na linha seguinte, o vértice de origem `s`. Com BFS, mostre uma linha `<v>: <menor número de arestas de s até v, ou -1 se não for alcançável>` para cada vértice de 0 a n-1 e `Alcancaveis: <quantos vértices (incluindo s) são alcançáveis>`.");
                sol = "#include <stdio.h>\n\nint main(void) {\n" + rd + "    {\n        int fila[8];\n        int dist[8];\n        int ini = 0, fim = 0, v, s, alc = 0;\n        scanf(\"%d\", &s);\n        for (i = 0; i < n; i++) {\n            dist[i] = -1;\n        }\n        dist[s] = 0;\n        fila[fim++] = s;\n        while (ini < fim) {\n            v = fila[ini++];\n            alc++;\n            for (j = 0; j < n; j++) {\n                if (adj[v][j] && dist[j] < 0) {\n                    dist[j] = dist[v] + 1;\n                    fila[fim++] = j;\n                }\n            }\n        }\n        for (i = 0; i < n; i++) {\n            printf(\"%d: %d\\n\", i, dist[i]);\n        }\n        printf(\"Alcancaveis: %d\\n\", alc);\n    }\n    return 0;\n}\n";
                gen = g => { int n = g.Next(2, 9); return Graph(g, n, g.Next(0, n + 2)) + g.Next(n) + "\n"; };
                oracle = i => { var t = PK.Ints(i); int n = t[0], m = t[1]; var adj = Adj(t); int s = t[^1]; var dist = Enumerable.Repeat(-1, n).ToArray(); var q = new Queue<int>(); dist[s] = 0; q.Enqueue(s); int alc = 0; while (q.Count > 0) { int v = q.Dequeue(); alc++; foreach (var w in adj[v]) if (dist[w] < 0) { dist[w] = dist[v] + 1; q.Enqueue(w); } } _ = m; return string.Join("\n", dist.Select((x, k) => $"{k}: {x}")) + $"\nAlcancaveis: {alc}"; };
                why = L.T("A distância de um vizinho novo é a do vértice atual mais 1. Quem ainda está com -1 nunca foi alcançado pela fila.");
                break;
            default:
                st = L.T("Leia `l` e `c` (de 1 a 6) e depois `l` linhas de `c` caracteres: `.` livre, `#` parede, `S` origem e `E` destino. Andando para cima, baixo, esquerda e direita, mostre `Passos: <menor número de passos de S até E>` e `Visitadas: <quantas casas (incluindo S) a BFS consegue alcançar partindo de S, mesmo depois de achar E>`. Se E não for alcançável, mostre `Passos: impossivel`.");
                sol = "#include <stdio.h>\n\nint main(void) {\n    char g[6][8];\n    int dist[6][6];\n    int fl[36];\n    int fc[36];\n    int l, c, i, j, d, ini = 0, fim = 0, sl = 0, sc = 0, el = 0, ec = 0, vis = 0, nl, nc;\n    int dl[4] = {-1, 1, 0, 0};\n    int dc[4] = {0, 0, -1, 1};\n    scanf(\"%d %d\", &l, &c);\n    for (i = 0; i < l; i++) {\n        scanf(\"%s\", g[i]);\n        for (j = 0; j < c; j++) {\n            dist[i][j] = -1;\n            if (g[i][j] == 'S') {\n                sl = i;\n                sc = j;\n            }\n            if (g[i][j] == 'E') {\n                el = i;\n                ec = j;\n            }\n        }\n    }\n    dist[sl][sc] = 0;\n    fl[fim] = sl;\n    fc[fim] = sc;\n    fim++;\n    while (ini < fim) {\n        i = fl[ini];\n        j = fc[ini];\n        ini++;\n        vis++;\n        for (d = 0; d < 4; d++) {\n            nl = i + dl[d];\n            nc = j + dc[d];\n            if (nl < 0 || nl >= l || nc < 0 || nc >= c || g[nl][nc] == '#' || dist[nl][nc] >= 0) {\n                continue;\n            }\n            dist[nl][nc] = dist[i][j] + 1;\n            fl[fim] = nl;\n            fc[fim] = nc;\n            fim++;\n        }\n    }\n    if (dist[el][ec] < 0) {\n        printf(\"Passos: impossivel\\n\");\n    } else {\n        printf(\"Passos: %d\\n\", dist[el][ec]);\n    }\n    printf(\"Visitadas: %d\\n\", vis);\n    return 0;\n}\n";
                gen = g =>
                {
                    int l = g.Next(2, 6), c = g.Next(2, 6); var grid = new char[l, c];
                    for (int a = 0; a < l; a++) for (int b = 0; b < c; b++) grid[a, b] = g.Next(4) == 0 ? '#' : '.';
                    var cells = Enumerable.Range(0, l * c).OrderBy(_ => g.Next()).Take(2).ToArray(); grid[cells[0] / c, cells[0] % c] = 'S'; grid[cells[1] / c, cells[1] % c] = 'E';
                    return $"{l} {c}\n" + string.Join("\n", Enumerable.Range(0, l).Select(a => new string(Enumerable.Range(0, c).Select(b => grid[a, b]).ToArray()))) + "\n";
                };
                oracle = i =>
                {
                    var t = i.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries); int l = int.Parse(t[0]), c = int.Parse(t[1]); var rows = t.Skip(2).ToArray();
                    var dist = new int[l, c]; for (int a = 0; a < l; a++) for (int b = 0; b < c; b++) dist[a, b] = -1;
                    (int, int) s = (0, 0), e = (0, 0); for (int a = 0; a < l; a++) for (int b = 0; b < c; b++) { if (rows[a][b] == 'S') s = (a, b); if (rows[a][b] == 'E') e = (a, b); }
                    var q = new Queue<(int, int)>(); dist[s.Item1, s.Item2] = 0; q.Enqueue(s); int vis = 0;
                    while (q.Count > 0) { var (a, b) = q.Dequeue(); vis++; foreach (var (da, db) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) }) { int x = a + da, y = b + db; if (x < 0 || y < 0 || x >= l || y >= c || rows[x][y] == '#' || dist[x, y] >= 0) continue; dist[x, y] = dist[a, b] + 1; q.Enqueue((x, y)); } }
                    return (dist[e.Item1, e.Item2] < 0 ? "Passos: impossivel" : $"Passos: {dist[e.Item1, e.Item2]}") + $"\nVisitadas: {vis}";
                };
                why = L.T("Uma grade é um grafo em que cada casa livre liga às quatro vizinhas. A BFS guarda a distância em cada casa; o que sai da fila é contado como visitado. Fila com coordenadas em dois vetores (linha e coluna).");
                break;
        }
        return PK.Write(st, PK.CStarter, sol, PK.Tests(r, 7, gen, oracle), why);
    }

    static Exercise Predict20(Difficulty d, Random r)
    {
        var o = new List<string>(); string code; int n = r.Next(4, 7);
        string g0 = Graph(r, n, n + r.Next(0, 3)); var t = PK.Ints(g0); var adj = Adj(t); int m = t[1];
        var edges = Enumerable.Range(0, m).Select(i => (t[2 + 2 * i], t[3 + 2 * i])).ToList();
        string edgeInit = $"    int arestas[{Math.Max(m, 1)}][2] = {{{string.Join(", ", edges.Select(e => $"{{{e.Item1}, {e.Item2}}}"))}}};\n";
        string build = $"    int adj[{n}][{n}] = {{{{0}}}};\n    int i, j, v;\n" + edgeInit + $"    for (i = 0; i < {m}; i++) {{\n        adj[arestas[i][0]][arestas[i][1]] = 1;\n        adj[arestas[i][1]][arestas[i][0]] = 1;\n    }}\n";
        switch (d)
        {
            case Difficulty.Easy:
                code = PK.CMain(build + $"    for (v = 0; v < {n}; v++) {{\n        int g = 0;\n        for (j = 0; j < {n}; j++) {{\n            g += adj[v][j];\n        }}\n        printf(\"%d \", g);\n    }}\n    printf(\"\\n\");\n");
                o.Add(string.Join("", adj.Select(l => l.Count + " ")));
                break;
            case Difficulty.Medium:
            {
                code = PK.CMain(build + $"    int fila[{n}];\n    int vis[{n}] = {{0}};\n    int ini = 0, fim = 0;\n    fila[fim++] = 0;\n    vis[0] = 1;\n    while (ini < fim) {{\n        v = fila[ini++];\n        printf(\"%d \", v);\n        for (j = 0; j < {n}; j++) {{\n            if (adj[v][j] && !vis[j]) {{\n                vis[j] = 1;\n                fila[fim++] = j;\n            }}\n        }}\n    }}\n    printf(\"\\n\");\n");
                var vis = new bool[n]; var q = new Queue<int>(); var ord = new List<int>(); q.Enqueue(0); vis[0] = true; while (q.Count > 0) { int v = q.Dequeue(); ord.Add(v); foreach (var w in adj[v]) if (!vis[w]) { vis[w] = true; q.Enqueue(w); } }
                o.Add(string.Join("", ord.Select(x => x + " ")));
                break;
            }
            case Difficulty.Hard:
            {
                code = PK.CMain(build + $"    int fila[{n}];\n    int dist[{n}];\n    int ini = 0, fim = 0;\n    for (i = 0; i < {n}; i++) {{\n        dist[i] = -1;\n    }}\n    fila[fim++] = 0;\n    dist[0] = 0;\n    while (ini < fim) {{\n        v = fila[ini++];\n        for (j = {n} - 1; j >= 0; j--) {{\n            if (adj[v][j] && dist[j] < 0) {{\n                dist[j] = dist[v] + 1;\n                fila[fim++] = j;\n            }}\n        }}\n    }}\n    for (i = 0; i < {n}; i++) {{\n        printf(\"%d \", dist[i]);\n    }}\n    printf(\"\\n\");\n    for (i = 0; i < fim; i++) {{\n        printf(\"%d \", fila[i]);\n    }}\n    printf(\"\\n\");\n");
                var dist = Enumerable.Repeat(-1, n).ToArray(); var q = new List<int> { 0 }; dist[0] = 0; int ptr = 0;
                while (ptr < q.Count) { int v = q[ptr++]; for (int j = n - 1; j >= 0; j--) if (adj[v].Contains(j) && dist[j] < 0) { dist[j] = dist[v] + 1; q.Add(j); } }
                o.Add(string.Join("", dist.Select(x => x + " "))); o.Add(string.Join("", q.Select(x => x + " ")));
                break;
            }
            default:
            {
                code = PK.CMain(build + $"    int fila[{n}];\n    int dist[{n}];\n    int ini = 0, fim = 0, maior = 0, soma = 0, comp = 0;\n    for (i = 0; i < {n}; i++) {{\n        dist[i] = -1;\n    }}\n    for (i = 0; i < {n}; i++) {{\n        if (dist[i] >= 0) {{\n            continue;\n        }}\n        comp++;\n        ini = 0;\n        fim = 0;\n        fila[fim++] = i;\n        dist[i] = 0;\n        while (ini < fim) {{\n            v = fila[ini++];\n            for (j = 0; j < {n}; j++) {{\n                if (adj[v][j] && dist[j] < 0) {{\n                    dist[j] = dist[v] + 1;\n                    fila[fim++] = j;\n                    soma += dist[j];\n                    if (dist[j] > maior) {{\n                        maior = dist[j];\n                    }}\n                }}\n            }}\n        }}\n    }}\n    printf(\"%d %d %d\\n\", comp, maior, soma);\n");
                var dist = Enumerable.Repeat(-1, n).ToArray(); int comp = 0, maior = 0, soma = 0;
                for (int i = 0; i < n; i++) { if (dist[i] >= 0) continue; comp++; var q = new Queue<int>(); q.Enqueue(i); dist[i] = 0; while (q.Count > 0) { int v = q.Dequeue(); foreach (var w in adj[v]) if (dist[w] < 0) { dist[w] = dist[v] + 1; q.Enqueue(w); soma += dist[w]; maior = Math.Max(maior, dist[w]); } } }
                o.Add($"{comp} {maior} {soma}");
                break;
            }
        }
        return PK.Predict(L.F("O grafo do programa tem {0} vértices e as arestas {1}.", n, string.Join(" ", edges.Select(e => $"{e.Item1}-{e.Item2}"))), code, string.Join("\n", o),
            L.T("Desenhe o grafo com as arestas (valem nos dois sentidos). A BFS usa uma fila: tira o da frente, olha os vizinhos na ordem do laço e coloca no fim os ainda não visitados. A distância de um vizinho novo é a do atual mais 1."));
    }
}
