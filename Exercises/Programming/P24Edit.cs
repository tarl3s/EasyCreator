namespace EasyCreator.Exercises;

/// <summary>py-20 (dynamic programming on strings): longest common subsequence, edit distance, palindromic completion; and filling the tables by hand.</summary>
[Exercises("py-20")]
public sealed class P24Edit : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(lv, r) : MakeOut(lv, r);
    }

    static int[,] Lcs(string a, string b) { var t = new int[a.Length + 1, b.Length + 1]; for (int i = 1; i <= a.Length; i++) for (int j = 1; j <= b.Length; j++) t[i, j] = a[i - 1] == b[j - 1] ? t[i - 1, j - 1] + 1 : Math.Max(t[i - 1, j], t[i, j - 1]); return t; }
    static int[,] Edit(string a, string b) { var t = new int[a.Length + 1, b.Length + 1]; for (int i = 0; i <= a.Length; i++) t[i, 0] = i; for (int j = 0; j <= b.Length; j++) t[0, j] = j; for (int i = 1; i <= a.Length; i++) for (int j = 1; j <= b.Length; j++) t[i, j] = Math.Min(Math.Min(t[i - 1, j] + 1, t[i, j - 1] + 1), t[i - 1, j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1)); return t; }
    static string Rev(string s) => new string(s.Reverse().ToArray());

    static string Word(Random r, int lo, int hi, string alphabet = "abcde") => new string(Enumerable.Range(0, Ex.Int(r, lo, hi)).Select(_ => alphabet[r.Next(alphabet.Length)]).ToArray());
    static string Pair(Random r, int lo = 2, int hi = 7) { var a = Word(r, lo, hi); string b = r.Next(2) == 0 ? Word(r, lo, hi) : Mutate(r, a); return a + "\n" + b; }
    static string Mutate(Random r, string a) { var l = a.ToList(); int k = Ex.Int(r, 1, 2); for (int i = 0; i < k; i++) { int p = r.Next(l.Count); switch (r.Next(3)) { case 0: l[p] = "abcde"[r.Next(5)]; break; case 1: l.RemoveAt(p); if (l.Count == 0) l.Add('a'); break; default: l.Insert(p, "abcde"[r.Next(5)]); break; } } return new string(l.ToArray()); }

    static Exercise MakeCode(int lv, Random r)
    {
        string rd = "a = input()\nb = input()";
        string starter = rd + "\n" + Pg.Comment(false, L.T("escreva a tabela de programação dinâmica"));
        switch (lv)
        {
            case 0:
            {
                var tests = Pg.Tests(new[] { "abcde\nace", Pair(r), Pair(r), Pair(r), Pair(r) }, i => { var p = i.TrimEnd('\n').Split('\n'); return Lcs(p[0], p[1])[p[0].Length, p[1].Length].ToString(); });
                string sol = rd + "\nt = [[0] * (len(b) + 1) for _ in range(len(a) + 1)]\nfor i in range(1, len(a) + 1):\n    for j in range(1, len(b) + 1):\n        if a[i - 1] == b[j - 1]:\n            t[i][j] = t[i - 1][j - 1] + 1\n        else:\n            t[i][j] = max(t[i - 1][j], t[i][j - 1])\nprint(t[len(a)][len(b)])";
                return Ex.Code(Pg.Prompt(L.T("Leia duas palavras (uma por linha, letras minúsculas). Mostre o tamanho da MAIOR SUBSEQUÊNCIA COMUM: a maior sequência de letras que aparece nas duas palavras, na mesma ordem, mas não necessariamente seguidas (em `abcde` e `ace` é `ace`, tamanho 3). Use uma tabela de programação dinâmica."), tests[0]), starter, sol, tests,
                    L.T("t[i][j] é a resposta para os i primeiros caracteres de a e os j primeiros de b. Se os últimos caracteres são iguais, estendem a melhor subsequência anterior (diagonal + 1); senão, descartamos o último de uma das palavras e ficamos com a melhor das duas opções."));
            }
            case 1:
            {
                var tests = Pg.Tests(new[] { "abc\nabd", Pair(r), Pair(r), Pair(r), Pair(r) }, i => { var p = i.TrimEnd('\n').Split('\n'); return (p[0].Length + p[1].Length - 2 * Lcs(p[0], p[1])[p[0].Length, p[1].Length]).ToString(); });
                string sol = rd + "\nt = [[0] * (len(b) + 1) for _ in range(len(a) + 1)]\nfor i in range(1, len(a) + 1):\n    for j in range(1, len(b) + 1):\n        if a[i - 1] == b[j - 1]:\n            t[i][j] = t[i - 1][j - 1] + 1\n        else:\n            t[i][j] = max(t[i - 1][j], t[i][j - 1])\nprint(len(a) + len(b) - 2 * t[len(a)][len(b)])";
                return Ex.Code(Pg.Prompt(L.T("Leia duas palavras (uma por linha). Queremos transformar a primeira na segunda usando SÓ duas operações: apagar um caractere ou inserir um caractere (sem trocar). Mostre o menor número de operações."), tests[0]), starter, sol, tests,
                    L.T("As letras que podem ficar são exatamente uma subsequência comum; todas as outras da primeira palavra são apagadas e todas as outras da segunda são inseridas. Quanto maior a subsequência comum, menos operações: tamanho(a) + tamanho(b) − 2·LCS."));
            }
            case 2:
            {
                var tests = Pg.Tests(new[] { "kitten\nsitting", "abc\nabc", Pair(r), Pair(r), Pair(r), Pair(r) }, i => { var p = i.TrimEnd('\n').Split('\n'); return Edit(p[0], p[1])[p[0].Length, p[1].Length].ToString(); });
                string sol = rd + "\nt = [[0] * (len(b) + 1) for _ in range(len(a) + 1)]\nfor i in range(len(a) + 1):\n    t[i][0] = i\nfor j in range(len(b) + 1):\n    t[0][j] = j\nfor i in range(1, len(a) + 1):\n    for j in range(1, len(b) + 1):\n        custo = 0 if a[i - 1] == b[j - 1] else 1\n        t[i][j] = min(t[i - 1][j] + 1, t[i][j - 1] + 1, t[i - 1][j - 1] + custo)\nprint(t[len(a)][len(b)])";
                return Ex.Code(Pg.Prompt(L.T("Leia duas palavras (uma por linha, letras minúsculas). Mostre a DISTÂNCIA DE EDIÇÃO: o menor número de operações para transformar a primeira na segunda, onde cada operação é inserir, apagar ou TROCAR um caractere (`kitten` → `sitting` é 3)."), tests[0]), starter, sol, tests,
                    L.T("Na célula (i, j) escolhemos a melhor entre: apagar (cima + 1), inserir (esquerda + 1) ou trocar/manter (diagonal + 0 se os caracteres são iguais, + 1 se são diferentes). A primeira linha e a primeira coluna valem o número de caracteres (transformar do/para o texto vazio)."));
            }
            default:
            {
                int Min(string s) { var t = Lcs(s, Rev(s)); return s.Length - t[s.Length, s.Length]; }
                var tests = Pg.Tests(new[] { "ab", "aba", Word(r, 3, 8), Word(r, 3, 8), Word(r, 4, 9), Word(r, 4, 9) }, i => Min(i.Trim()).ToString());
                string sol = "s = input()\nt = s[::-1]\nn = len(s)\ntab = [[0] * (n + 1) for _ in range(n + 1)]\nfor i in range(1, n + 1):\n    for j in range(1, n + 1):\n        if s[i - 1] == t[j - 1]:\n            tab[i][j] = tab[i - 1][j - 1] + 1\n        else:\n            tab[i][j] = max(tab[i - 1][j], tab[i][j - 1])\nprint(n - tab[n][n])";
                return Ex.Code(Pg.Prompt(L.T("Leia uma palavra (letras minúsculas). Mostre o MENOR número de letras que é preciso INSERIR (em qualquer posição) para ela virar um palíndromo (`ab` precisa de 1: `aba`; `aba` precisa de 0)."), tests[0]),
                    "s = input()\n" + Pg.Comment(false, L.T("escreva a tabela de programação dinâmica")), sol, tests,
                    L.T("Truque: a parte da palavra que já é \"simétrica\" é a maior subsequência palindrômica, e ela é a maior subsequência comum entre a palavra e ela mesma INVERTIDA. As letras fora dela precisam de um par inserido: resposta = tamanho − LCS(s, inversa de s)."));
            }
        }
    }

    static Exercise MakeOut(int lv, Random r)
    {
        string a = Word(r, 3, 5), b = r.Next(2) == 0 ? Word(r, 3, 5) : Mutate(r, a);
        if (lv == 0)
        {
            var t = Lcs(a, b); var last = Enumerable.Range(0, b.Length + 1).Select(j => t[a.Length, j]);
            string code = $"a = \"{a}\"\nb = \"{b}\"\nt = [[0] * (len(b) + 1) for _ in range(len(a) + 1)]\nfor i in range(1, len(a) + 1):\n    for j in range(1, len(b) + 1):\n        if a[i - 1] == b[j - 1]:\n            t[i][j] = t[i - 1][j - 1] + 1\n        else:\n            t[i][j] = max(t[i - 1][j], t[i][j - 1])\nprint(\" \".join(str(x) for x in t[-1]))\nprint(t[-1][-1])";
            string outp = $"{string.Join(" ", last)}\n{t[a.Length, b.Length]}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("A última linha da tabela mostra o tamanho da melhor subsequência comum entre a palavra a inteira (\"{0}\") e os j primeiros caracteres de b (\"{1}\"), para j = 0, 1, 2, ... O último valor é a resposta.", a, b));
        }
        if (lv == 1)
        {
            var t = Edit(a, b);
            string code = $"a = \"{a}\"\nb = \"{b}\"\nt = [[0] * (len(b) + 1) for _ in range(len(a) + 1)]\nfor i in range(len(a) + 1):\n    t[i][0] = i\nfor j in range(len(b) + 1):\n    t[0][j] = j\nfor i in range(1, len(a) + 1):\n    for j in range(1, len(b) + 1):\n        custo = 0 if a[i - 1] == b[j - 1] else 1\n        t[i][j] = min(t[i - 1][j] + 1, t[i][j - 1] + 1, t[i - 1][j - 1] + custo)\nprint(t[len(a)][len(b)])\nprint(\" \".join(str(x) for x in t[len(a)]))";
            string outp = $"{t[a.Length, b.Length]}\n{string.Join(" ", Enumerable.Range(0, b.Length + 1).Select(j => t[a.Length, j]))}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("Transformando \"{0}\" em \"{1}\": a distância é o último valor da última linha; a linha inteira dá a distância de \"{0}\" para cada começo de \"{1}\" (incluindo o vazio, que custa len(a)).", a, b));
        }
        if (lv == 2)
        {
            var t = Edit(a, b);
            var rows = Enumerable.Range(0, a.Length + 1).Select(i => string.Join(" ", Enumerable.Range(0, b.Length + 1).Select(j => t[i, j])));
            string code = $"a = \"{a}\"\nb = \"{b}\"\nt = [[0] * (len(b) + 1) for _ in range(len(a) + 1)]\nfor i in range(len(a) + 1):\n    t[i][0] = i\nfor j in range(len(b) + 1):\n    t[0][j] = j\nfor i in range(1, len(a) + 1):\n    for j in range(1, len(b) + 1):\n        custo = 0 if a[i - 1] == b[j - 1] else 1\n        t[i][j] = min(t[i - 1][j] + 1, t[i][j - 1] + 1, t[i - 1][j - 1] + custo)\nfor linha in t:\n    print(\" \".join(str(x) for x in linha))";
            string outp = string.Join("\n", rows);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("Preencha a tabela célula a célula. A primeira linha é 0 1 2 ... e a primeira coluna também; cada célula é o menor de: cima + 1, esquerda + 1 e diagonal + (0 se os caracteres são iguais, senão 1)."));
        }
        {
            string s = Word(r, 4, 6); var t = Lcs(s, Rev(s));
            string code = $"s = \"{s}\"\nt = s[::-1]\nn = len(s)\ntab = [[0] * (n + 1) for _ in range(n + 1)]\nfor i in range(1, n + 1):\n    for j in range(1, n + 1):\n        if s[i - 1] == t[j - 1]:\n            tab[i][j] = tab[i - 1][j - 1] + 1\n        else:\n            tab[i][j] = max(tab[i - 1][j], tab[i][j - 1])\nprint(t)\nprint(tab[n][n])\nprint(n - tab[n][n])";
            string outp = $"{Rev(s)}\n{t[s.Length, s.Length]}\n{s.Length - t[s.Length, s.Length]}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("t é a palavra \"{0}\" invertida. A tabela calcula a maior subsequência comum entre a palavra e a sua inversa ({1}); o que sobra ({2}) é quantas letras faltariam para formar um palíndromo.", s, t[s.Length, s.Length], s.Length - t[s.Length, s.Length]));
        }
    }
}
