namespace EasyCreator.Exercises;

/// <summary>py-16 (binary search trees): functions over a tree built from a list, and tracing traversals, heights and search paths.</summary>
[Exercises("py-16")]
public sealed class P16Bst : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(lv, r) : MakeOut(lv, r);
    }

    class N { public int V; public N? L, R; }
    static N? Ins(N? n, int v) { if (n == null) return new N { V = v }; if (v < n.V) n.L = Ins(n.L, v); else if (v > n.V) n.R = Ins(n.R, v); return n; }
    static N? Build(IEnumerable<int> vs) { N? root = null; foreach (var v in vs) root = Ins(root, v); return root; }
    static int Height(N? n) => n == null ? 0 : 1 + Math.Max(Height(n.L), Height(n.R));
    static int Leaves(N? n) => n == null ? 0 : n.L == null && n.R == null ? 1 : Leaves(n.L) + Leaves(n.R);
    static int Count(N? n) => n == null ? 0 : 1 + Count(n.L) + Count(n.R);
    static void InOrder(N? n, List<int> o) { if (n == null) return; InOrder(n.L, o); o.Add(n.V); InOrder(n.R, o); }
    static void Pre(N? n, List<int> o) { if (n == null) return; o.Add(n.V); Pre(n.L, o); Pre(n.R, o); }
    static void Post(N? n, List<int> o) { if (n == null) return; Post(n.L, o); Post(n.R, o); o.Add(n.V); }

    const string TreeCode = "class No:\n    def __init__(self, v):\n        self.v = v\n        self.esq = None\n        self.dir = None\ndef inserir(no, v):\n    if no is None:\n        return No(v)\n    if v < no.v:\n        no.esq = inserir(no.esq, v)\n    elif v > no.v:\n        no.dir = inserir(no.dir, v)\n    return no";
    const string ReadTree = "raiz = None\nfor x in map(int, input().split()):\n    raiz = inserir(raiz, x)";

    static string Vals(Random r, int n, int hi = 60) => string.Join(" ", Enumerable.Range(0, n).Select(_ => r.Next(1, hi + 1)));
    static int[] P(string line) => line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();

    static Exercise MakeCode(int lv, Random r)
    {
        string Starter(string fn, string call) => TreeCode + "\n" + ReadTree + "\n" + Pg.Comment(false, L.T("escreva a função recursiva abaixo")) + "\n" + fn + "\n" + call;
        switch (lv)
        {
            case 0:
            case 1:
            {
                var opts = lv == 0
                    ? new[] { ("altura", "a ALTURA da árvore (número de níveis; a árvore vazia tem altura 0 e uma árvore só com a raiz tem altura 1)", "    if no is None:\n        return 0\n    return 1 + max(altura(no.esq), altura(no.dir))", (Func<N?, int>)Height),
                              ("folhas", "a quantidade de FOLHAS (nós sem filhos)", "    if no is None:\n        return 0\n    if no.esq is None and no.dir is None:\n        return 1\n    return folhas(no.esq) + folhas(no.dir)", Leaves),
                              ("soma", "a SOMA de todos os valores da árvore", "    if no is None:\n        return 0\n    return no.v + soma(no.esq) + soma(no.dir)", n => { var o = new List<int>(); InOrder(n, o); return o.Sum(); }) }
                    : new[] { ("menor", "o MENOR valor da árvore (a árvore não é vazia; lembre que o menor fica sempre indo para a esquerda)", "    while no.esq is not None:\n        no = no.esq\n    return no.v", (Func<N?, int>)(n => { var o = new List<int>(); InOrder(n, o); return o.Min(); })),
                              ("maior", "o MAIOR valor da árvore (a árvore não é vazia)", "    while no.dir is not None:\n        no = no.dir\n    return no.v", n => { var o = new List<int>(); InOrder(n, o); return o.Max(); }),
                              ("quantos", "a quantidade de nós da árvore", "    if no is None:\n        return 0\n    return 1 + quantos(no.esq) + quantos(no.dir)", Count) };
                var (name, desc, body, f) = Ex.Pick(r, opts);
                var tests = Pg.Tests(Enumerable.Range(0, 5).Select(_ => Vals(r, Ex.Int(r, 1, 10))), i => f(Build(P(i.Trim()))).ToString());
                string fn = $"def {name}(no):\n{body}", call = $"print({name}(raiz))";
                return Ex.Code(Pg.Prompt(L.F("O código inicial já constrói uma árvore binária de busca a partir de uma linha de inteiros (valores repetidos são ignorados). Escreva a função `{0}(no)` que devolve {1}, e mostre o valor para a árvore lida.", name, L.T(desc)), tests[0]),
                    Starter($"def {name}(no):\n    pass", call), TreeCode + "\n" + ReadTree + "\n" + fn + "\n" + call, tests,
                    lv == 0 ? L.T("A recursão trata o nó vazio como caso base e combina as respostas das duas subárvores (esquerda e direita) com o nó atual.") : L.T("Numa árvore de busca, o menor valor está seguindo sempre o filho da esquerda, e o maior seguindo sempre o da direita. Contar nós é recursão sobre as duas subárvores."));
            }
            case 2:
            {
                if (r.Next(2) == 0)
                {
                    var ins = Enumerable.Range(0, 5).Select(_ => { var vals = Vals(r, Ex.Int(r, 4, 10)); var a = P(vals); int k = Ex.Int(r, 1, a.Distinct().Count()); return vals + "\n" + k; });
                    var tests = Pg.Tests(ins, i => { var ls = i.TrimEnd('\n').Split('\n'); var o = new List<int>(); InOrder(Build(P(ls[0])), o); return o[int.Parse(ls[1]) - 1].ToString(); });
                    string fn = "def em_ordem(no, saida):\n    if no is None:\n        return\n    em_ordem(no.esq, saida)\n    saida.append(no.v)\n    em_ordem(no.dir, saida)";
                    string main = "k = int(input())\nsaida = []\nem_ordem(raiz, saida)\nprint(saida[k - 1])";
                    return Ex.Code(Pg.Prompt(L.T("O código inicial lê uma linha de inteiros e monta uma árvore binária de busca (repetidos são ignorados). Leia depois um inteiro `k` e mostre o `k`-ésimo MENOR valor da árvore (k = 1 é o menor). Dica: a travessia em ordem (esquerda, nó, direita) visita os valores em ordem crescente."), tests[0]),
                        TreeCode + "\n" + ReadTree + "\nk = int(input())\n" + Pg.Comment(false, L.T("escreva o resto do programa aqui")), TreeCode + "\n" + ReadTree + "\n" + fn + "\n" + main, tests,
                        L.T("Numa árvore de busca, percorrer esquerda → nó → direita lista os valores em ordem crescente. Guardamos essa lista e o k-ésimo menor é a posição k − 1."));
                }
                else
                {
                    var ins = Enumerable.Range(0, 5).Select(_ => { var vals = Vals(r, Ex.Int(r, 4, 10)); var a = P(vals); int q = r.Next(4) == 0 ? Ex.Int(r, 1, 60) : a[r.Next(a.Length)]; return vals + "\n" + q; });
                    var tests = Pg.Tests(ins, i => { var ls = i.TrimEnd('\n').Split('\n'); int q = int.Parse(ls[1]); var n = Build(P(ls[0])); var path = new List<int>(); while (n != null) { path.Add(n.V); if (q == n.V) return "Caminho: " + string.Join(" ", path); n = q < n.V ? n.L : n.R; } return "Não encontrado"; });
                    string main = "q = int(input())\ncaminho = []\nno = raiz\nachou = False\nwhile no is not None:\n    caminho.append(no.v)\n    if q == no.v:\n        achou = True\n        break\n    no = no.esq if q < no.v else no.dir\nif achou:\n    print(\"Caminho: \" + \" \".join(str(x) for x in caminho))\nelse:\n    print(\"Não encontrado\")";
                    return Ex.Code(Pg.Prompt(L.T("O código inicial lê uma linha de inteiros e monta uma árvore binária de busca. Leia depois um valor `q` e procure-o a partir da raiz, mostrando os valores visitados: `Caminho: <valores separados por espaço>` (terminando em `q`) ou `Não encontrado` se `q` não está na árvore."), tests[0]),
                        TreeCode + "\n" + ReadTree + "\nq = int(input())\n" + Pg.Comment(false, L.T("escreva o resto do programa aqui")), TreeCode + "\n" + ReadTree + "\n" + main, tests,
                        L.T("A cada nó: se q é igual, achamos; se é menor, seguimos pela esquerda; se é maior, pela direita. Anotamos cada nó visitado. Se chegarmos a um lugar vazio, q não existe."));
                }
            }
            default:
            {
                int Lca(N? root, int a, int b) { var n = root; while (n != null) { if (a < n.V && b < n.V) n = n.L; else if (a > n.V && b > n.V) n = n.R; else return n.V; } return -1; }
                var ins = Enumerable.Range(0, 5).Select(_ => { var vals = Vals(r, Ex.Int(r, 5, 11)); var a = P(vals).Distinct().ToArray(); return vals + "\n" + a[r.Next(a.Length)] + " " + a[r.Next(a.Length)]; });
                var tests = Pg.Tests(ins, i => { var ls = i.TrimEnd('\n').Split('\n'); var ab = P(ls[1]); return Lca(Build(P(ls[0])), ab[0], ab[1]).ToString(); });
                string main = "a, b = map(int, input().split())\nno = raiz\nwhile True:\n    if a < no.v and b < no.v:\n        no = no.esq\n    elif a > no.v and b > no.v:\n        no = no.dir\n    else:\n        break\nprint(no.v)";
                return Ex.Code(Pg.Prompt(L.T("O código inicial lê uma linha de inteiros e monta uma árvore binária de busca. Leia depois dois valores `a` e `b` (os dois existem na árvore) e mostre o ANCESTRAL COMUM MAIS BAIXO deles: o nó mais profundo que tem `a` e `b` na sua subárvore (um valor conta como ancestral de si mesmo)."), tests[0]),
                    TreeCode + "\n" + ReadTree + "\na, b = map(int, input().split())\n" + Pg.Comment(false, L.T("escreva o resto do programa aqui")), TreeCode + "\n" + ReadTree + "\n" + main, tests,
                    L.T("Truque da árvore de busca: descendo da raiz, se a e b são AMBOS menores que o nó, o ancestral comum está à esquerda; se ambos são maiores, à direita. No primeiro nó em que eles se separam (ou um deles é o próprio nó), esse nó é o ancestral comum mais baixo."));
            }
        }
    }

    static Exercise MakeOut(int lv, Random r)
    {
        var vals = Enumerable.Range(0, Ex.Int(r, 5, 8)).Select(_ => r.Next(1, 61)).ToArray();
        var root = Build(vals); string lst = string.Join(", ", vals);
        string head = TreeCode + $"\nraiz = None\nfor x in [{lst}]:\n    raiz = inserir(raiz, x)\n";
        if (lv == 0)
        {
            int kind = r.Next(3); var o = new List<int>();
            string[] names = { "pre", "em", "pos" };
            if (kind == 0) Pre(root, o); else if (kind == 1) InOrder(root, o); else Post(root, o);
            string body = kind == 0 ? "    saida.append(no.v)\n    visita(no.esq)\n    visita(no.dir)" : kind == 1 ? "    visita(no.esq)\n    saida.append(no.v)\n    visita(no.dir)" : "    visita(no.esq)\n    visita(no.dir)\n    saida.append(no.v)";
            string code = head + "saida = []\ndef visita(no):\n    if no is None:\n        return\n" + body + "\nvisita(raiz)\nprint(\" \".join(str(x) for x in saida))";
            string outp = string.Join(" ", o);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Primeiro monte a árvore inserindo na ordem [{0}] (repetidos são ignorados; menor vai para a esquerda, maior para a direita). Depois siga a ordem da função: {1}.", lst,
                kind == 0 ? L.T("nó, esquerda, direita") : kind == 1 ? L.T("esquerda, nó, direita (valores em ordem crescente)") : L.T("esquerda, direita, nó")));
        }
        if (lv == 1)
        {
            string code = head + "def altura(no):\n    if no is None:\n        return 0\n    return 1 + max(altura(no.esq), altura(no.dir))\ndef folhas(no):\n    if no is None:\n        return 0\n    if no.esq is None and no.dir is None:\n        return 1\n    return folhas(no.esq) + folhas(no.dir)\nprint(raiz.v)\nprint(altura(raiz))\nprint(folhas(raiz))";
            string outp = $"{root!.V}\n{Height(root)}\n{Leaves(root)}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Insira na ordem [{0}]: o primeiro valor é a raiz. A altura é o número de níveis do caminho mais longo; as folhas são os nós sem filhos.", lst));
        }
        if (lv == 2)
        {
            int q = r.Next(3) == 0 ? r.Next(1, 61) : vals[r.Next(vals.Length)];
            var path = new List<int>(); var n = root; bool found = false;
            while (n != null) { path.Add(n.V); if (q == n.V) { found = true; break; } n = q < n.V ? n.L : n.R; }
            string code = head + $"q = {q}\nno = raiz\npassos = 0\nwhile no is not None and no.v != q:\n    passos += 1\n    no = no.esq if q < no.v else no.dir\nprint(passos)\nprint(\"achou\" if no is not None else \"nao\")";
            string outp = $"{(found ? path.Count - 1 : path.Count)}\n{(found ? "achou" : "nao")}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Procurando {0} a partir da raiz, o caminho é {1}. `passos` conta os nós visitados ANTES de chegar ao valor (ou de cair fora da árvore).", q, string.Join(" → ", path)));
        }
        {
            var o = new List<int>(); InOrder(root, o);
            var d = o.Distinct().ToArray();
            int k = Ex.Int(r, 1, 2);
            int left = Count(root!.L), right = Count(root.R);
            string code = head + "def contar(no):\n    if no is None:\n        return 0\n    return 1 + contar(no.esq) + contar(no.dir)\nprint(contar(raiz.esq), contar(raiz.dir))\nprint(contar(raiz) == len(set([" + lst + "])))\nsaida = []\ndef nivel(no, p):\n    if no is None:\n        return\n    if p == " + k + ":\n        saida.append(no.v)\n    nivel(no.esq, p + 1)\n    nivel(no.dir, p + 1)\nnivel(raiz, 0)\nprint(\" \".join(str(x) for x in saida))";
            var lvl = new List<int>(); void Nv(N? n, int p) { if (n == null) return; if (p == k) lvl.Add(n.V); Nv(n.L, p + 1); Nv(n.R, p + 1); }
            Nv(root, 0);
            string outp = $"{left} {right}\nTrue\n{string.Join(" ", lvl)}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("A raiz é {0}: sua subárvore esquerda tem {1} nós e a direita {2}. Como repetidos são ignorados, a árvore tem tantos nós quanto valores distintos (verdadeiro). A última linha lista, da esquerda para a direita, os nós do nível {3} (a raiz é o nível 0).", root.V, left, right, k));
        }
    }
}
