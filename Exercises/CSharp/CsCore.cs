using System.Text;

namespace EasyCreator.Exercises;

/// <summary>C# lessons 6-10: methods, arrays and strings, classes/lists/dictionaries, recursion, sorting and searching.</summary>
[Exercises("cs-06", "cs-07", "cs-08", "cs-09", "cs-10")]
public sealed class CsCore : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r) => lesson.Id switch
    {
        "cs-06" => r.Next(2) == 0 ? Write06(d, r) : Predict06(d, r),
        "cs-07" => r.Next(2) == 0 ? Write07(d, r) : Predict07(d, r),
        "cs-08" => r.Next(2) == 0 ? Write08(d, r) : Predict08(d, r),
        "cs-09" => r.Next(2) == 0 ? Write09(d, r) : Predict09(d, r),
        _ => r.Next(2) == 0 ? Write10(d, r) : Predict10(d, r),
    };

    static readonly string[] Words = { "banana", "radar", "computador", "janela", "arara", "sol", "estudar", "programa", "ovo", "laranja", "teclado", "mouse", "nuvem", "arquivo", "ana", "reviver", "pixel", "memoria", "vetor", "chave" };
    static readonly string[] Names = { "Ana", "Bruno", "Carla", "Davi", "Eva", "Fabio", "Gabi", "Hugo", "Iris", "Joao", "Lia", "Mauro" };
    const string S = "// escreva seu código aqui\n";
    static string NList(Random g, int max, int lo, int hi) { int n = g.Next(1, max + 1); return $"{n}\n{PK.Join(PK.Rand(g, n, lo, hi))}\n"; }
    const string ReadArr = "int n = int.Parse(Console.ReadLine());\nint[] v = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();\n";

    // ---------- cs-06 ----------
    static Exercise Write06(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Escreva o método `static int Mdc(int a, int b)` (Euclides) e use-o: leia `a` e `b` (um por linha, positivos) e mostre `MDC: <mdc>` e `MMC: <mmc>`.");
                sol = "int a = int.Parse(Console.ReadLine());\nint b = int.Parse(Console.ReadLine());\nConsole.WriteLine($\"MDC: {Mdc(a, b)}\");\nConsole.WriteLine($\"MMC: {a / Mdc(a, b) * b}\");\nstatic int Mdc(int a, int b)\n{\n    while (b != 0) (a, b) = (b, a % b);\n    return a;\n}\n";
                gen = g => $"{g.Next(1, 120)}\n{g.Next(1, 120)}\n";
                oracle = i => { var v = PK.Ints(i); int a = v[0], b = v[1]; while (b != 0) (a, b) = (b, a % b); return $"MDC: {a}\nMMC: {v[0] / a * v[1]}"; };
                why = L.T("Euclides: o MDC não muda ao trocar `(a, b)` por `(b, a % b)`. O MMC é `a / MDC * b`.");
                break;
            case Difficulty.Medium:
                st = L.T("Escreva `static bool EhPrimo(int n)` e leia `a` e `b` (um por linha). Mostre `Primos: <os primos de a até b separados por espaço>` (ou `Primos: nenhum`) e `Quantidade: <quantos>`. Se `a > b`, troque-os antes.");
                sol = "int a = int.Parse(Console.ReadLine());\nint b = int.Parse(Console.ReadLine());\nif (a > b) (a, b) = (b, a);\nvar p = new List<int>();\nfor (int i = a; i <= b; i++) if (EhPrimo(i)) p.Add(i);\nConsole.WriteLine(\"Primos: \" + (p.Count == 0 ? \"nenhum\" : string.Join(\" \", p)));\nConsole.WriteLine($\"Quantidade: {p.Count}\");\nstatic bool EhPrimo(int n)\n{\n    if (n < 2) return false;\n    for (int d = 2; d * d <= n; d++) if (n % d == 0) return false;\n    return true;\n}\n";
                gen = g => $"{g.Next(0, 60)}\n{g.Next(0, 60)}\n";
                oracle = i => { var v = PK.Ints(i); int a = Math.Min(v[0], v[1]), b = Math.Max(v[0], v[1]); var p = Enumerable.Range(a, b - a + 1).Where(n => n >= 2 && Enumerable.Range(2, Math.Max(0, (int)Math.Sqrt(n) - 1)).All(x => n % x != 0)).ToList(); return "Primos: " + (p.Count == 0 ? "nenhum" : PK.Join(p)) + $"\nQuantidade: {p.Count}"; };
                why = L.T("Um método pequeno (`EhPrimo`) deixa o laço principal limpo. Para ser primo, nenhum divisor de 2 até a raiz.");
                break;
            case Difficulty.Hard:
                st = L.T("Escreva `static (int menor, int maior, double media) Resumo(int[] v)`, que devolve uma tupla. Leia `n` e, na linha seguinte, `n` inteiros; mostre `Menor: <x>`, `Maior: <x>`, `Média: <média com 2 casas>` e `Amplitude: <maior - menor>`.");
                sol = ReadArr + "var (menor, maior, media) = Resumo(v);\nConsole.WriteLine($\"Menor: {menor}\");\nConsole.WriteLine($\"Maior: {maior}\");\nConsole.WriteLine($\"Média: {media:F2}\");\nConsole.WriteLine($\"Amplitude: {maior - menor}\");\nstatic (int menor, int maior, double media) Resumo(int[] v) => (v.Min(), v.Max(), v.Average());\n";
                gen = g => NList(g, 10, -20, 50);
                oracle = i => { var v = PK.Ints(i).Skip(1).ToArray(); return $"Menor: {v.Min()}\nMaior: {v.Max()}\nMédia: {PK.F2(v.Average())}\nAmplitude: {v.Max() - v.Min()}"; };
                why = L.T("Uma tupla devolve vários valores de uma vez; na chamada ela se desmonta com `var (a, b, c) = ...`.");
                break;
            default:
                st = L.T("Escreva `static long PotMod(long b, long e, long m)`, que calcula `b^e mod m` por **exponenciação rápida** (eleve ao quadrado e olhe o bit mais baixo de `e`), sem laço de `e` repetições. Leia `b`, `e` e `m` (um por linha; `e` até 1000000000) e mostre `Resultado: <b^e mod m>` e `Inverso: sim` se `PotMod(b, m - 2, m)` multiplicado por `b` dá 1 módulo `m` (para `m` primo é o inverso de Fermat), senão `Inverso: nao`.");
                sol = "long b = long.Parse(Console.ReadLine());\nlong e = long.Parse(Console.ReadLine());\nlong m = long.Parse(Console.ReadLine());\nConsole.WriteLine($\"Resultado: {PotMod(b, e, m)}\");\nConsole.WriteLine(\"Inverso: \" + (PotMod(b, m - 2, m) * (b % m) % m == 1 ? \"sim\" : \"nao\"));\nstatic long PotMod(long b, long e, long m)\n{\n    long r = 1;\n    b %= m;\n    while (e > 0)\n    {\n        if ((e & 1) == 1) r = r * b % m;\n        b = b * b % m;\n        e >>= 1;\n    }\n    return r;\n}\n";
                gen = g => $"{g.Next(2, 1000)}\n{g.Next(0, 1000000000)}\n{PK.Pick(g, 7, 11, 13, 97, 101, 1009, 10007, 100, 36)}\n";
                oracle = i =>
                {
                    var t = i.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(long.Parse).ToArray();
                    long P(long b, long e, long m) { long r = 1; b %= m; while (e > 0) { if ((e & 1) == 1) r = r * b % m; b = b * b % m; e >>= 1; } return r; }
                    return $"Resultado: {P(t[0], t[1], t[2])}\nInverso: {(P(t[0], t[2] - 2, t[2]) * (t[0] % t[2]) % t[2] == 1 ? "sim" : "nao")}";
                };
                why = L.T("Se o bit mais baixo de `e` é 1, multiplique o resultado pela base atual; depois eleve a base ao quadrado e desloque `e` um bit. Isso leva cerca de 30 passos em vez de um bilhão.");
                break;
        }
        return PK.Write(st, S, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict06(Difficulty d, Random r)
    {
        int a = r.Next(2, 9), b = r.Next(2, 9), k = r.Next(2, 5); var o = new List<string>(); string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = $"int x = {a};\nint y = Dobro(x);\nConsole.WriteLine($\"{{x}} {{y}}\");\nstatic int Dobro(int n)\n{{\n    n = n * 2;\n    return n;\n}}\n"; o.Add($"{a} {a * 2}"); break;
            case Difficulty.Medium:
                code = $"int x = {a}, y = {b};\nSoma(x, ref y);\nConsole.WriteLine($\"{{x}} {{y}}\");\nTroca(ref x, ref y);\nConsole.WriteLine($\"{{x}} {{y}}\");\nstatic void Soma(int x, ref int y)\n{{\n    x = x + {k};\n    y = y + x;\n}}\nstatic void Troca(ref int p, ref int q)\n{{\n    (p, q) = (q, p);\n}}\n";
                { int x = a, y = b; y += x + k; o.Add($"{x} {y}"); (x, y) = (y, x); o.Add($"{x} {y}"); }
                break;
            case Difficulty.Hard:
                code = $"int q;\nbool ok = Dividir({a * k + 1}, {k}, out q, out int resto);\nConsole.WriteLine($\"{{ok}} {{q}} {{resto}}\");\nConsole.WriteLine(Dividir(5, 0, out q, out resto));\nConsole.WriteLine($\"{{q}} {{resto}}\");\nstatic bool Dividir(int a, int b, out int q, out int r)\n{{\n    q = 0;\n    r = a;\n    if (b == 0) return false;\n    q = a / b;\n    r = a % b;\n    return true;\n}}\n";
                o.Add($"True {(a * k + 1) / k} {(a * k + 1) % k}"); o.Add("False"); o.Add("0 5");
                break;
            default:
                code = $"int f(int n) => n < 2 ? n : f(n - 1) + f(n - 2);\nint contador = 0;\nint Passo(int x)\n{{\n    contador++;\n    return x * {k};\n}}\nint s = 0;\nfor (int i = 0; i < {a}; i++) s += Passo(i) + f(i);\nConsole.WriteLine($\"{{s}} {{contador}}\");\nConsole.WriteLine(f({a + 3}));\n";
                { int F(int n) => n < 2 ? n : F(n - 1) + F(n - 2); int s = 0; for (int i = 0; i < a; i++) s += i * k + F(i); o.Add($"{s} {a}"); o.Add(F(a + 3).ToString()); }
                break;
        }
        return PK.Predict(L.F("O programa usa os valores {0}, {1} e {2}.", a, b, k), code, string.Join("\n", o),
            L.T("Parâmetros comuns são cópias: mudar dentro do método não muda quem chamou. `ref` e `out` passam a própria variável. Um método que usa variáveis de fora (função local) enxerga e altera essas variáveis."));
    }

    // ---------- cs-07 ----------
    static Exercise Write07(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Leia uma palavra (letras minúsculas) e mostre `Tamanho: <n>`, `Vogais: <quantas>`, `Invertida: <a palavra de trás para frente>` e `Palíndromo: sim` ou `Palíndromo: não`.");
                sol = "string p = Console.ReadLine();\nstring inv = new string(p.Reverse().ToArray());\nConsole.WriteLine($\"Tamanho: {p.Length}\");\nConsole.WriteLine($\"Vogais: {p.Count(c => \"aeiou\".Contains(c))}\");\nConsole.WriteLine($\"Invertida: {inv}\");\nConsole.WriteLine(\"Palíndromo: \" + (p == inv ? \"sim\" : \"não\"));\n";
                gen = g => PK.Pick(g, Words) + "\n";
                oracle = i => { var p = i.Trim(); var inv = new string(p.Reverse().ToArray()); return $"Tamanho: {p.Length}\nVogais: {p.Count(c => "aeiou".Contains(c))}\nInvertida: {inv}\nPalíndromo: {(p == inv ? "sim" : "não")}"; };
                why = L.T("Uma string se comporta como uma sequência de letras: `Length`, `Reverse`, `Count` e a comparação `==` resolvem tudo.");
                break;
            case Difficulty.Medium:
                st = L.T("Leia uma linha com palavras separadas por espaços (um ou mais). Mostre `Palavras: <quantas>`, `Maior: <a mais longa; empate: a primeira>` e `Invertida: <as palavras em ordem inversa separadas por um espaço>`. Se não houver palavras, mostre só `Palavras: 0`.");
                sol = "string[] p = (Console.ReadLine() ?? \"\").Split(' ', StringSplitOptions.RemoveEmptyEntries);\nConsole.WriteLine($\"Palavras: {p.Length}\");\nif (p.Length > 0)\n{\n    Console.WriteLine($\"Maior: {p.Aggregate((a, b) => b.Length > a.Length ? b : a)}\");\n    Array.Reverse(p);\n    Console.WriteLine($\"Invertida: {string.Join(\" \", p)}\");\n}\n";
                gen = g => string.Join(PK.Pick(g, " ", "  "), Enumerable.Range(0, g.Next(0, 6)).Select(_ => PK.Pick(g, Words))) + "\n";
                oracle = i => { var p = i.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries); if (p.Length == 0) return "Palavras: 0"; return $"Palavras: {p.Length}\nMaior: {p.Aggregate((a, b) => b.Length > a.Length ? b : a)}\nInvertida: {string.Join(" ", p.Reverse())}"; };
                why = L.T("`Split` com `RemoveEmptyEntries` ignora espaços repetidos. Para a maior palavra, só troque quando uma for estritamente mais longa.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia `n` e `n` inteiros (na linha seguinte). Mostre `Invertido: <os números de trás para frente>`, `Maior: <valor> na posição <índice da primeira ocorrência>` e `Crescente: sim` se estiverem em ordem estritamente crescente (senão `Crescente: não`).");
                sol = ReadArr + "int m = 0;\nbool cresc = true;\nfor (int i = 1; i < n; i++)\n{\n    if (v[i] > v[m]) m = i;\n    if (v[i] <= v[i - 1]) cresc = false;\n}\nConsole.WriteLine(\"Invertido: \" + string.Join(\" \", v.Reverse()));\nConsole.WriteLine($\"Maior: {v[m]} na posição {m}\");\nConsole.WriteLine(\"Crescente: \" + (cresc ? \"sim\" : \"não\"));\n";
                gen = g => { int n = g.Next(1, 9); var v = g.Next(3) == 0 ? PK.Distinct(g, n, 1, 30).OrderBy(x => x).ToList() : PK.Rand(g, n, -5, 12); return $"{n}\n{PK.Join(v)}\n"; };
                oracle = i => { var v = PK.Ints(i).Skip(1).ToArray(); int m = 0; bool c = true; for (int j = 1; j < v.Length; j++) { if (v[j] > v[m]) m = j; if (v[j] <= v[j - 1]) c = false; } return $"Invertido: {PK.Join(v.Reverse())}\nMaior: {v[m]} na posição {m}\nCrescente: {(c ? "sim" : "não")}"; };
                why = L.T("O maior só é trocado quando o novo valor é estritamente maior, então fica a primeira ocorrência. Crescente exige cada elemento maior que o anterior.");
                break;
            default:
                st = L.T("Leia duas palavras (uma por linha). Mostre `Anagramas: sim` se uma é um reordenamento das letras da outra (ignorando maiúsculas) ou `Anagramas: não`; depois leia mentalmente um deslocamento `k` igual ao tamanho da primeira palavra e mostre `Cifrada: <primeira palavra com a cifra de César de deslocamento k, só letras minúsculas>` (cada letra avança k posições com volta depois do z).");
                sol = "string a = Console.ReadLine().ToLower();\nstring b = Console.ReadLine().ToLower();\nbool ana = a.OrderBy(c => c).SequenceEqual(b.OrderBy(c => c));\nConsole.WriteLine(\"Anagramas: \" + (ana ? \"sim\" : \"não\"));\nint k = a.Length;\nvar sb = new StringBuilder();\nforeach (char c in a) sb.Append((char)('a' + (c - 'a' + k) % 26));\nConsole.WriteLine($\"Cifrada: {sb}\");\n";
                gen = g => { var w = PK.Pick(g, Words); string w2 = g.Next(2) == 0 ? new string(w.OrderBy(_ => g.Next()).ToArray()) : PK.Pick(g, Words); return $"{w}\n{w2}\n"; };
                oracle = i => { var t = i.Split('\n', StringSplitOptions.RemoveEmptyEntries); string a = t[0], b = t[1]; bool ana = a.OrderBy(c => c).SequenceEqual(b.OrderBy(c => c)); return "Anagramas: " + (ana ? "sim" : "não") + "\nCifrada: " + new string(a.Select(c => (char)('a' + (c - 'a' + a.Length) % 26)).ToArray()); };
                why = L.T("Dois textos são anagramas quando as letras ordenadas são iguais. Na cifra, a posição da letra é `c - 'a'`; some `k` e tire o resto por 26 para dar a volta no alfabeto.");
                break;
        }
        return PK.Write(st, S, "using System.Text;\n" + sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict07(Difficulty d, Random r)
    {
        string w = PK.Pick(r, Words), w2 = PK.Pick(r, Words); int n = r.Next(4, 7); var v = PK.Rand(r, n, 1, 20); var o = new List<string>(); string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = $"string s = \"{w}\";\nConsole.WriteLine(s.ToUpper());\nConsole.WriteLine(s[0] + \"-\" + s[^1]);\nConsole.WriteLine(s.Substring(1, 2));\nConsole.WriteLine(s.IndexOf('{w[^1]}'));\n";
                o.Add(w.ToUpper()); o.Add($"{w[0]}-{w[^1]}"); o.Add(w.Substring(1, 2)); o.Add(w.IndexOf(w[^1]).ToString()); break;
            case Difficulty.Medium:
                code = $"string[] p = \"{w} {w2}  {w}\".Split(' ', StringSplitOptions.RemoveEmptyEntries);\nConsole.WriteLine(p.Length);\nConsole.WriteLine(string.Join(\"+\", p));\nConsole.WriteLine(p[0] == p[2]);\nConsole.WriteLine(p[1].Length + p[2].Length);\n";
                o.Add("3"); o.Add($"{w}+{w2}+{w}"); o.Add("True"); o.Add((w2.Length + w.Length).ToString()); break;
            case Difficulty.Hard:
                code = $"int[] v = {{ {PK.Join(v, ", ")} }};\nArray.Reverse(v);\nint s = 0;\nfor (int i = 0; i < v.Length; i += 2) s += v[i];\nConsole.WriteLine(string.Join(\",\", v));\nConsole.WriteLine(s);\nArray.Sort(v);\nConsole.WriteLine(v[0] + \" \" + v[^1]);\n";
                { var a = Enumerable.Reverse(v).ToArray(); o.Add(string.Join(",", a)); o.Add(Enumerable.Range(0, a.Length).Where(i => i % 2 == 0).Sum(i => a[i]).ToString()); Array.Sort(a); o.Add($"{a[0]} {a[^1]}"); }
                break;
            default:
                code = $"string s = \"{w}{w2}\";\nvar cont = new int[26];\nforeach (char c in s) cont[c - 'a']++;\nint best = 0;\nfor (int i = 1; i < 26; i++) if (cont[i] > cont[best]) best = i;\nConsole.WriteLine($\"{{(char)('a' + best)}} {{cont[best]}}\");\nConsole.WriteLine(new string(s.Where((c, i) => i % 3 == 0).ToArray()));\nConsole.WriteLine(s.Replace(\"{w[0]}\", \"#\"));\n";
                { string s = w + w2; var cont = new int[26]; foreach (char c in s) cont[c - 'a']++; int best = 0; for (int i = 1; i < 26; i++) if (cont[i] > cont[best]) best = i; o.Add($"{(char)('a' + best)} {cont[best]}"); o.Add(new string(s.Where((c, i) => i % 3 == 0).ToArray())); o.Add(s.Replace(w[0].ToString(), "#")); }
                break;
        }
        return PK.Predict(L.F("O programa trabalha com o texto \"{0}\".", w), code, string.Join("\n", o),
            L.T("Índices começam em 0 e `[^1]` é o último. `Substring(início, tamanho)` recorta. `Split` com `RemoveEmptyEntries` ignora espaços repetidos. Acompanhe cada instrução com o valor atual da variável."));
    }

    // ---------- cs-08 ----------
    static Exercise Write08(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Crie uma classe `Produto` com `Nome`, `Preco` (int) e `Qtd` (int) e o método `Total()` (`Preco * Qtd`). Leia `n` e `n` linhas `nome preço quantidade`. Mostre `<nome>: <total>` por produto, `Total: <soma>` e `Maior: <nome do de maior total; empate: o primeiro>`.");
                sol = "int n = int.Parse(Console.ReadLine());\nvar lista = new List<Produto>();\nfor (int i = 0; i < n; i++)\n{\n    var p = Console.ReadLine().Split(' ');\n    lista.Add(new Produto { Nome = p[0], Preco = int.Parse(p[1]), Qtd = int.Parse(p[2]) });\n}\nforeach (var p in lista) Console.WriteLine($\"{p.Nome}: {p.Total()}\");\nConsole.WriteLine($\"Total: {lista.Sum(p => p.Total())}\");\nvar m = lista[0];\nforeach (var p in lista) if (p.Total() > m.Total()) m = p;\nConsole.WriteLine($\"Maior: {m.Nome}\");\nclass Produto\n{\n    public string Nome { get; set; }\n    public int Preco { get; set; }\n    public int Qtd { get; set; }\n    public int Total() => Preco * Qtd;\n}\n";
                gen = g => { int n = g.Next(1, 7); var nm = Names.OrderBy(_ => g.Next()).Take(n); return n + "\n" + string.Join("\n", nm.Select(x => $"{x} {g.Next(1, 40)} {g.Next(1, 9)}")) + "\n"; };
                oracle = i => { var t = i.Split(new[] { '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries); int n = int.Parse(t[0]); var o = new List<string>(); int tot = 0, best = -1; string bn = ""; for (int j = 0; j < n; j++) { int v = int.Parse(t[2 + 3 * j]) * int.Parse(t[3 + 3 * j]); o.Add($"{t[1 + 3 * j]}: {v}"); tot += v; if (v > best) { best = v; bn = t[1 + 3 * j]; } } o.Add($"Total: {tot}"); o.Add($"Maior: {bn}"); return string.Join("\n", o); };
                why = L.T("A classe agrupa os dados e o comportamento (`Total()`). Uma `List<Produto>` guarda os objetos; para o maior, troque só quando o total for estritamente maior.");
                break;
            case Difficulty.Medium:
                st = L.T("Leia `n` e depois `n` palavras (uma linha, separadas por espaço). Usando um `Dictionary<string, int>`, mostre uma linha `<palavra>: <vezes>` para cada palavra distinta **na ordem da primeira aparição** e `Mais comum: <palavra>` (empate: a que apareceu primeiro).");
                sol = "int n = int.Parse(Console.ReadLine());\nvar cont = new Dictionary<string, int>();\nforeach (var w in Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries))\n    cont[w] = cont.GetValueOrDefault(w) + 1;\nstring best = null;\nforeach (var kv in cont)\n{\n    Console.WriteLine($\"{kv.Key}: {kv.Value}\");\n    if (best == null || kv.Value > cont[best]) best = kv.Key;\n}\nConsole.WriteLine($\"Mais comum: {best}\");\n";
                gen = g => { int n = g.Next(1, 12); return n + "\n" + string.Join(" ", Enumerable.Range(0, n).Select(_ => PK.Pick(g, "sol", "mar", "casa", "livro", "pedra"))) + "\n"; };
                oracle = i => { var t = i.Split(new[] { '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToList(); var order = new List<string>(); var c = new Dictionary<string, int>(); foreach (var w in t) { if (!c.ContainsKey(w)) { c[w] = 0; order.Add(w); } c[w]++; } var best = order[0]; foreach (var w in order) if (c[w] > c[best]) best = w; return string.Join("\n", order.Select(w => $"{w}: {c[w]}")) + $"\nMais comum: {best}"; };
                why = L.T("O dicionário associa cada palavra a um contador. Ele enumera as chaves na ordem de inserção (se nada foi removido), o que dá a ordem da primeira aparição.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia `n` linhas `nome nota` (nota inteira); um aluno pode aparecer várias vezes. Com uma classe `Aluno` (nome, lista de notas, método `Media()`), mostre `<nome>: <média com 2 casas>` na ordem da primeira aparição e depois `Ranking: <nomes por média decrescente, separados por vírgula; empate: ordem da primeira aparição>`.");
                sol = "int n = int.Parse(Console.ReadLine());\nvar alunos = new List<Aluno>();\nfor (int i = 0; i < n; i++)\n{\n    var p = Console.ReadLine().Split(' ');\n    var a = alunos.FirstOrDefault(x => x.Nome == p[0]);\n    if (a == null) { a = new Aluno { Nome = p[0] }; alunos.Add(a); }\n    a.Notas.Add(int.Parse(p[1]));\n}\nforeach (var a in alunos) Console.WriteLine($\"{a.Nome}: {a.Media():F2}\");\nConsole.WriteLine(\"Ranking: \" + string.Join(\", \", alunos.OrderByDescending(a => a.Media()).Select(a => a.Nome)));\nclass Aluno\n{\n    public string Nome { get; set; }\n    public List<int> Notas { get; } = new();\n    public double Media() => Notas.Average();\n}\n";
                gen = g => { int n = g.Next(1, 9); var nm = Names.OrderBy(_ => g.Next()).Take(4).ToList(); return n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => $"{PK.Pick(g, nm.ToArray())} {g.Next(0, 11)}")) + "\n"; };
                oracle = i => { var l = i.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(x => x.Split(' ')).ToList(); var order = new List<string>(); var notas = new Dictionary<string, List<int>>(); foreach (var p in l) { if (!notas.ContainsKey(p[0])) { notas[p[0]] = new(); order.Add(p[0]); } notas[p[0]].Add(int.Parse(p[1])); } return string.Join("\n", order.Select(a => $"{a}: {PK.F2(notas[a].Average())}")) + "\nRanking: " + string.Join(", ", order.OrderByDescending(a => notas[a].Average())); };
                why = L.T("Procure o aluno pelo nome; se não existir, crie e adicione. `OrderByDescending` é estável: os empates ficam na ordem original.");
                break;
            default:
                st = L.T("Estoque de loja. Leia `n` comandos, um por linha: `add <item> <qtd>` soma ao estoque; `remove <item> <qtd>` tira (se faltar, mostre `Falta <item>: tem <x>, pediu <y>` e não altere); `consulta <item>` mostra `<item>: <qtd>` (0 se nunca apareceu). No fim mostre `Itens: <quantos têm estoque maior que zero>` e `Unidades: <soma>`. Use `Dictionary<string, int>`.");
                sol = "int n = int.Parse(Console.ReadLine());\nvar est = new Dictionary<string, int>();\nfor (int i = 0; i < n; i++)\n{\n    var p = Console.ReadLine().Split(' ');\n    int atual = est.GetValueOrDefault(p[1]);\n    if (p[0] == \"add\") est[p[1]] = atual + int.Parse(p[2]);\n    else if (p[0] == \"remove\")\n    {\n        int q = int.Parse(p[2]);\n        if (q > atual) Console.WriteLine($\"Falta {p[1]}: tem {atual}, pediu {q}\");\n        else est[p[1]] = atual - q;\n    }\n    else Console.WriteLine($\"{p[1]}: {atual}\");\n}\nConsole.WriteLine($\"Itens: {est.Count(kv => kv.Value > 0)}\");\nConsole.WriteLine($\"Unidades: {est.Values.Sum()}\");\n";
                gen = g => { int n = g.Next(2, 10); var items = new[] { "pao", "leite", "ovo" }; var sb = new StringBuilder(n + "\n"); for (int k = 0; k < n; k++) { int t = g.Next(4); string it = PK.Pick(g, items); sb.Append(t <= 1 ? $"add {it} {g.Next(1, 9)}\n" : t == 2 ? $"remove {it} {g.Next(1, 9)}\n" : $"consulta {it}\n"); } return sb.ToString(); };
                oracle = i => { var est = new Dictionary<string, int>(); var o = new List<string>(); foreach (var ln in i.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1)) { var p = ln.Split(' '); int at = est.GetValueOrDefault(p[1]); if (p[0] == "add") est[p[1]] = at + int.Parse(p[2]); else if (p[0] == "remove") { int q = int.Parse(p[2]); if (q > at) o.Add($"Falta {p[1]}: tem {at}, pediu {q}"); else est[p[1]] = at - q; } else o.Add($"{p[1]}: {at}"); } o.Add($"Itens: {est.Count(kv => kv.Value > 0)}"); o.Add($"Unidades: {est.Values.Sum()}"); return string.Join("\n", o); };
                why = L.T("`GetValueOrDefault` devolve 0 para um item que nunca apareceu. Confira o estoque antes de remover; só altere o dicionário quando o pedido cabe.");
                break;
        }
        return PK.Write(st, S, "using System.Text;\n" + sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict08(Difficulty d, Random r)
    {
        int a = r.Next(1, 9), b = r.Next(1, 9), k = r.Next(2, 5); string w = PK.Pick(r, Names); var o = new List<string>(); string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = $"var p = new Ponto {{ X = {a}, Y = {b} }};\nvar q = p;\nq.X += {k};\nConsole.WriteLine($\"{{p.X}} {{p.Y}}\");\nConsole.WriteLine(p == q);\nclass Ponto\n{{\n    public int X {{ get; set; }}\n    public int Y {{ get; set; }}\n}}\n";
                o.Add($"{a + k} {b}"); o.Add("True"); break;
            case Difficulty.Medium:
                code = $"var l = new List<int> {{ {a}, {b}, {a + b} }};\nl.Add({k});\nl.Insert(1, {k * 2});\nl.Remove({b});\nConsole.WriteLine(string.Join(\",\", l));\nConsole.WriteLine(l.Count + \" \" + l.Sum());\nvar d = new Dictionary<string, int> {{ [\"{w}\"] = {a} }};\nd[\"{w}\"] += {k};\nd[\"x\"] = {b};\nConsole.WriteLine($\"{{d[\"{w}\"]}} {{d.Count}} {{d.ContainsKey(\"y\")}}\");\n";
                { var l = new List<int> { a, b, a + b }; l.Add(k); l.Insert(1, k * 2); l.Remove(b); o.Add(string.Join(",", l)); o.Add($"{l.Count} {l.Sum()}"); o.Add($"{a + k} 2 False"); }
                break;
            case Difficulty.Hard:
                code = $"var c = new Conta(\"{w}\", {a * 10});\nc.Depositar({b});\nbool ok = c.Sacar({a * 10 + b + 1});\nConsole.WriteLine($\"{{ok}} {{c.Saldo}}\");\nok = c.Sacar({a});\nConsole.WriteLine($\"{{ok}} {{c.Saldo}} {{c}}\");\nclass Conta\n{{\n    public string Dono {{ get; }}\n    public int Saldo {{ get; private set; }}\n    public Conta(string dono, int saldo) {{ Dono = dono; Saldo = saldo; }}\n    public void Depositar(int v) => Saldo += v;\n    public bool Sacar(int v)\n    {{\n        if (v > Saldo) return false;\n        Saldo -= v;\n        return true;\n    }}\n    public override string ToString() => Dono + \"=\" + Saldo;\n}}\n";
                o.Add($"False {a * 10 + b}"); o.Add($"True {a * 10 + b - a} {w}={a * 10 + b - a}"); break;
            default:
                code = $"var fila = new Dictionary<string, List<int>>();\nvoid Add(string chave, int v)\n{{\n    if (!fila.ContainsKey(chave)) fila[chave] = new List<int>();\n    fila[chave].Add(v);\n}}\nfor (int i = 1; i <= {a + 4}; i++) Add(i % 3 == 0 ? \"A\" : i % 2 == 0 ? \"B\" : \"C\", i * {k});\nforeach (var kv in fila)\n    Console.WriteLine($\"{{kv.Key}}: {{string.Join(\" \", kv.Value)}} ({{kv.Value.Sum()}})\");\nConsole.WriteLine(fila.Values.Max(l => l.Count));\n";
                { var f = new Dictionary<string, List<int>>(); for (int i = 1; i <= a + 4; i++) { var key = i % 3 == 0 ? "A" : i % 2 == 0 ? "B" : "C"; if (!f.ContainsKey(key)) f[key] = new(); f[key].Add(i * k); } foreach (var kv in f) o.Add($"{kv.Key}: {string.Join(" ", kv.Value)} ({kv.Value.Sum()})"); o.Add(f.Values.Max(l => l.Count).ToString()); }
                break;
        }
        return PK.Predict(L.F("O programa usa o nome {0} e os valores {1} e {2}.", w, a, b), code, string.Join("\n", o),
            L.T("Classes são tipos de referência: `var q = p` aponta para o MESMO objeto, então mudar `q` muda `p`. Listas crescem com `Add`/`Insert`; dicionários guardam pares chave→valor e enumeram na ordem de inserção."));
    }

    // ---------- cs-09 ----------
    static Exercise Write09(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Leia `n` (0 a 12). Com métodos **recursivos**, mostre `Soma: <1+2+...+n>`, `Fatorial: <n!>` e `Fibonacci: <fib(n)>` (com fib(0) = 0 e fib(1) = 1).");
                sol = "int n = int.Parse(Console.ReadLine());\nConsole.WriteLine($\"Soma: {Soma(n)}\");\nConsole.WriteLine($\"Fatorial: {Fat(n)}\");\nConsole.WriteLine($\"Fibonacci: {Fib(n)}\");\nstatic int Soma(int n) => n == 0 ? 0 : n + Soma(n - 1);\nstatic long Fat(int n) => n <= 1 ? 1 : n * Fat(n - 1);\nstatic int Fib(int n) => n < 2 ? n : Fib(n - 1) + Fib(n - 2);\n";
                gen = g => $"{g.Next(0, 13)}\n";
                oracle = i => { int n = PK.Ints(i)[0]; long f = 1; for (int j = 2; j <= n; j++) f *= j; int a = 0, b = 1; for (int j = 0; j < n; j++) (a, b) = (b, a + b); return $"Soma: {n * (n + 1) / 2}\nFatorial: {f}\nFibonacci: {a}"; };
                why = L.T("Cada método tem um caso base (n = 0, 1) e um passo que chama a si mesmo com um problema menor.");
                break;
            case Difficulty.Medium:
                st = L.T("Leia `n` (0 a 999999) e `b` e `e` (base de 1 a 9 e expoente de 0 a 8, na linha seguinte). Com recursão, mostre `Dígitos: <soma dos dígitos de n>`, `Potência: <b elevado a e>` e `Binário: <n em binário>` (para 0, `0`).");
                sol = "int n = int.Parse(Console.ReadLine());\nvar t = Console.ReadLine().Split(' ');\nint b = int.Parse(t[0]), e = int.Parse(t[1]);\nConsole.WriteLine($\"Dígitos: {SomaDig(n)}\");\nConsole.WriteLine($\"Potência: {Pot(b, e)}\");\nConsole.WriteLine($\"Binário: {(n == 0 ? \"0\" : Bin(n))}\");\nstatic int SomaDig(int n) => n == 0 ? 0 : n % 10 + SomaDig(n / 10);\nstatic long Pot(int b, int e) => e == 0 ? 1 : b * Pot(b, e - 1);\nstatic string Bin(int n) => n == 0 ? \"\" : Bin(n / 2) + (n % 2);\n";
                gen = g => $"{g.Next(0, 1000000)}\n{g.Next(1, 10)} {g.Next(0, 9)}\n";
                oracle = i => { var v = PK.Ints(i); int n = v[0], s = 0, m = n; while (m > 0) { s += m % 10; m /= 10; } long p = 1; for (int j = 0; j < v[2]; j++) p *= v[1]; return $"Dígitos: {s}\nPotência: {p}\nBinário: {Convert.ToString(n, 2)}"; };
                why = L.T("Soma de dígitos: último dígito + soma do resto. Binário: o binário de `n/2` seguido do bit `n%2`.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia uma palavra de 1 a 4 letras **distintas**. Usando recursão, mostre todas as permutações das letras em ordem alfabética, uma por linha, e depois `Total: <quantidade>`.");
                sol = "string s = new string(Console.ReadLine().OrderBy(c => c).ToArray());\nint total = 0;\nGerar(\"\", s);\nConsole.WriteLine($\"Total: {total}\");\nvoid Gerar(string prefixo, string resto)\n{\n    if (resto.Length == 0) { Console.WriteLine(prefixo); total++; return; }\n    for (int i = 0; i < resto.Length; i++)\n        Gerar(prefixo + resto[i], resto.Remove(i, 1));\n}\n";
                gen = g => new string("abcdefghij".OrderBy(_ => g.Next()).Take(g.Next(1, 5)).ToArray()) + "\n";
                oracle = i => { string s = new string(i.Trim().OrderBy(c => c).ToArray()); var l = new List<string>(); void G(string p, string rest) { if (rest.Length == 0) { l.Add(p); return; } for (int k = 0; k < rest.Length; k++) G(p + rest[k], rest.Remove(k, 1)); } G("", s); l.Add($"Total: {l.Count}"); return string.Join("\n", l); };
                why = L.T("Escolha cada letra restante como próxima, e permute o que sobrou. Com as letras ordenadas e o laço em ordem, as permutações saem em ordem alfabética.");
                break;
            default:
                st = L.T("Leia `n` (1 a 12), depois `n` inteiros positivos distintos e um alvo `t`. Com recursão (cada número entra ou não no subconjunto), mostre `Subconjuntos: <quantos têm soma exatamente t>` e `Menor: <menor tamanho entre eles>` (ou `Menor: nenhum`).");
                sol = "int n = int.Parse(Console.ReadLine());\nint[] v = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();\nint t = int.Parse(Console.ReadLine());\nint cont = 0, menor = int.MaxValue;\nBusca(0, 0, 0);\nConsole.WriteLine($\"Subconjuntos: {cont}\");\nConsole.WriteLine(\"Menor: \" + (cont == 0 ? \"nenhum\" : menor.ToString()));\nvoid Busca(int i, int soma, int tam)\n{\n    if (soma == t) { cont++; menor = Math.Min(menor, tam); }\n    if (i == n || soma >= t) return;\n    Busca(i + 1, soma + v[i], tam + 1);\n    Busca(i + 1, soma, tam);\n}\n";
                gen = g => { int n = g.Next(3, 10); return $"{n}\n{PK.Join(PK.Distinct(g, n, 1, 15))}\n{g.Next(5, 30)}\n"; };
                oracle = i => { var v = PK.Ints(i); int n = v[0]; var a = v.Skip(1).Take(n).ToArray(); int t = v[n + 1]; int c = 0, mn = int.MaxValue; for (int m = 1; m < (1 << n); m++) { int s = 0, z = 0; for (int b = 0; b < n; b++) if ((m >> b & 1) == 1) { s += a[b]; z++; } if (s == t) { c++; mn = Math.Min(mn, z); } } return $"Subconjuntos: {c}\nMenor: {(c == 0 ? "nenhum" : mn.ToString())}"; };
                why = L.T("Em cada posição há duas escolhas: o número entra (soma e tamanho crescem) ou não. Como todos são positivos, dá para parar quando a soma passa do alvo.");
                break;
        }
        return PK.Write(st, S, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict09(Difficulty d, Random r)
    {
        int n = r.Next(3, 7), k = r.Next(2, 5), a = r.Next(10, 99), b = r.Next(2, 9) * r.Next(1, 5); var o = new List<string>(); string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = $"Console.WriteLine(F({n}));\nConsole.WriteLine(F({n - 2}) + 1);\nstatic int F(int n) => n <= 1 ? 1 : n * F(n - 1);\n";
                { int F(int x) => x <= 1 ? 1 : x * F(x - 1); o.Add(F(n).ToString()); o.Add((F(n - 2) + 1).ToString()); }
                break;
            case Difficulty.Medium:
                code = $"Mostra({n});\nConsole.WriteLine();\nstatic void Mostra(int n)\n{{\n    if (n == 0) return;\n    Console.Write(n + \" \");\n    Mostra(n - 1);\n    Console.Write(n * {k} + \" \");\n}}\n";
                { var sb = new StringBuilder(); void M(int x) { if (x == 0) return; sb.Append(x + " "); M(x - 1); sb.Append(x * k + " "); } M(n); o.Add(sb.ToString()); }
                break;
            case Difficulty.Hard:
                code = $"int passos = 0;\nint r = Mdc({a}, {b});\nConsole.WriteLine($\"{{r}} {{passos}}\");\nint Mdc(int a, int b)\n{{\n    passos++;\n    Console.WriteLine($\"{{a}} {{b}}\");\n    return b == 0 ? a : Mdc(b, a % b);\n}}\n";
                { int x = a, y = b, p = 0; while (true) { p++; o.Add($"{x} {y}"); if (y == 0) break; (x, y) = (y, x % y); } o.Add($"{x} {p}"); }
                break;
            default:
                code = $"int chamadas = 0;\nvar memo = new Dictionary<int, long>();\nlong F(int n)\n{{\n    chamadas++;\n    if (n < 2) return n;\n    if (memo.TryGetValue(n, out var v)) return v;\n    return memo[n] = F(n - 1) + F(n - 2);\n}}\nConsole.WriteLine(F({n + 8}));\nConsole.WriteLine(chamadas);\nConsole.WriteLine(F({n + 8}) + \" \" + chamadas);\n";
                { int calls = 0; var memo = new Dictionary<int, long>(); long F(int x) { calls++; if (x < 2) return x; if (memo.TryGetValue(x, out var v)) return v; return memo[x] = F(x - 1) + F(x - 2); } long r1 = F(n + 8); o.Add(r1.ToString()); o.Add(calls.ToString()); long r2 = F(n + 8); o.Add($"{r2} {calls}"); }
                break;
        }
        return PK.Predict(L.F("A recursão do programa começa com o valor {0} e usa também o número {1}.", d == Difficulty.Hard ? a : n, d == Difficulty.Hard ? b : k), code, string.Join("\n", o),
            L.T("Desça a pilha de chamadas até o caso base e depois volte combinando os resultados. O que vem antes da chamada recursiva acontece na ida; o que vem depois, na volta (ordem inversa). Memoização evita recalcular valores já conhecidos."));
    }

    // ---------- cs-10 ----------
    static Exercise Write10(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        const string rd = "int n = int.Parse(Console.ReadLine());\nint[] v = (Console.ReadLine() ?? \"\").Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray();\n";
        switch (d)
        {
            case Difficulty.Easy:
                st = L.T("Leia `n` e, na linha seguinte, `n` inteiros. Ordene em ordem crescente **com um algoritmo seu** (sem `Array.Sort` nem LINQ) e mostre `Ordenado: <números>`, `Menor: <x>` e `Maior: <x>`. Para `n = 0`, mostre só `(vazio)`.");
                sol = rd + "if (n == 0) { Console.WriteLine(\"(vazio)\"); return; }\nfor (int i = 1; i < n; i++)\n{\n    int chave = v[i], j = i - 1;\n    while (j >= 0 && v[j] > chave) { v[j + 1] = v[j]; j--; }\n    v[j + 1] = chave;\n}\nConsole.WriteLine(\"Ordenado: \" + string.Join(\" \", v));\nConsole.WriteLine($\"Menor: {v[0]}\");\nConsole.WriteLine($\"Maior: {v[n - 1]}\");\n";
                gen = g => { int n = g.Next(0, 10); return $"{n}\n{PK.Join(PK.Rand(g, n, -20, 40))}\n"; };
                oracle = i => { var v = PK.Ints(i).Skip(1).OrderBy(x => x).ToArray(); return v.Length == 0 ? "(vazio)" : $"Ordenado: {PK.Join(v)}\nMenor: {v[0]}\nMaior: {v[^1]}"; };
                why = L.T("No insertion sort, cada elemento (a chave) desliza para trás até achar seu lugar na parte já ordenada.");
                break;
            case Difficulty.Medium:
                st = L.T("Leia `n` inteiros **já em ordem crescente** (linha 2) e um alvo (linha 3), com busca binária. Mostre `Posição: <índice>` (ou `Posição: -1`) e `Passos: <quantas vezes o meio foi examinado>`. Use `ini`, `fim` e `meio = (ini + fim) / 2` sobre a faixa fechada `[0, n-1]`.");
                sol = rd + "int alvo = int.Parse(Console.ReadLine());\nint ini = 0, fim = n - 1, passos = 0, pos = -1;\nwhile (ini <= fim)\n{\n    int meio = (ini + fim) / 2;\n    passos++;\n    if (v[meio] == alvo) { pos = meio; break; }\n    if (v[meio] < alvo) ini = meio + 1; else fim = meio - 1;\n}\nConsole.WriteLine($\"Posição: {pos}\");\nConsole.WriteLine($\"Passos: {passos}\");\n";
                gen = g => { int n = g.Next(1, 14); var v = PK.Distinct(g, n, 1, 60).OrderBy(x => x).ToList(); return $"{n}\n{PK.Join(v)}\n{(g.Next(3) == 0 ? g.Next(0, 62) : v[g.Next(n)])}\n"; };
                oracle = i => { var t = PK.Ints(i); int n = t[0]; var v = t.Skip(1).Take(n).ToArray(); int alvo = t[n + 1]; int a = 0, b = n - 1, s = 0, pos = -1; while (a <= b) { int m = (a + b) / 2; s++; if (v[m] == alvo) { pos = m; break; } if (v[m] < alvo) a = m + 1; else b = m - 1; } return $"Posição: {pos}\nPassos: {s}"; };
                why = L.T("A cada passo a faixa cai pela metade, por isso são cerca de log₂ n passos. A contagem depende de onde o alvo está.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia `n` e `n` inteiros (a lista **pode ter repetidos**, já crescente), depois `q` e `q` consultas (uma por linha). Para cada consulta `x`, com busca binária, mostre `<x>: primeiro <i>, quantidade <c>` (i = índice da primeira ocorrência) ou `<x>: ausente`.");
                sol = rd + "int q = int.Parse(Console.ReadLine());\nfor (int k = 0; k < q; k++)\n{\n    int x = int.Parse(Console.ReadLine());\n    int a = Limite(x), b = Limite(x + 1);\n    Console.WriteLine(a < n && v[a] == x ? $\"{x}: primeiro {a}, quantidade {b - a}\" : $\"{x}: ausente\");\n}\nint Limite(int x)\n{\n    int ini = 0, fim = n;\n    while (ini < fim)\n    {\n        int meio = (ini + fim) / 2;\n        if (v[meio] < x) ini = meio + 1; else fim = meio;\n    }\n    return ini;\n}\n";
                gen = g => { int n = g.Next(1, 11); var v = PK.Rand(g, n, 1, 8).OrderBy(x => x).ToList(); int q = g.Next(1, 5); return $"{n}\n{PK.Join(v)}\n{q}\n{PK.Join(PK.Rand(g, q, 0, 9), "\n")}\n"; };
                oracle = i => { var t = PK.Ints(i); int n = t[0]; var a = t.Skip(1).Take(n).ToArray(); int q = t[n + 1]; var l = new List<string>(); for (int j = 0; j < q; j++) { int x = t[n + 2 + j]; int f = Array.IndexOf(a, x); l.Add(f < 0 ? $"{x}: ausente" : $"{x}: primeiro {f}, quantidade {a.Count(y => y == x)}"); } return string.Join("\n", l); };
                why = L.T("O \"limite inferior\" acha a primeira posição com valor >= x. A quantidade é o limite de x+1 menos o de x.");
                break;
            default:
                st = L.T("Leia `n` e `n` inteiros e um alvo `s`. Ordene uma cópia com insertion sort contando os **deslocamentos** (cada vez que um elemento é copiado uma casa para a direita). Mostre `Deslocamentos: <total>` (igual ao número de pares fora de ordem) e, usando dois ponteiros na lista ordenada, `Pares: <quantos pares de posições i < j somam s>`.");
                sol = rd + "int s = int.Parse(Console.ReadLine());\nint desl = 0;\nfor (int i = 1; i < n; i++)\n{\n    int chave = v[i], j = i - 1;\n    while (j >= 0 && v[j] > chave) { v[j + 1] = v[j]; j--; desl++; }\n    v[j + 1] = chave;\n}\nint a = 0, b = n - 1, pares = 0;\nwhile (a < b)\n{\n    int soma = v[a] + v[b];\n    if (soma < s) a++;\n    else if (soma > s) b--;\n    else\n    {\n        if (v[a] == v[b]) { int m = b - a + 1; pares += m * (m - 1) / 2; break; }\n        int ca = 1, cb = 1;\n        while (v[a + ca] == v[a]) ca++;\n        while (v[b - cb] == v[b]) cb++;\n        pares += ca * cb;\n        a += ca; b -= cb;\n    }\n}\nConsole.WriteLine($\"Deslocamentos: {desl}\");\nConsole.WriteLine($\"Pares: {pares}\");\n";
                gen = g => { int n = g.Next(1, 11); return $"{n}\n{PK.Join(PK.Rand(g, n, 1, 8))}\n{g.Next(2, 16)}\n"; };
                oracle = i => { var t = PK.Ints(i); int n = t[0]; var v = t.Skip(1).Take(n).ToArray(); int s = t[n + 1]; int inv = 0, p = 0; for (int a = 0; a < n; a++) for (int b = a + 1; b < n; b++) { if (v[a] > v[b]) inv++; if (v[a] + v[b] == s) p++; } return $"Deslocamentos: {inv}\nPares: {p}"; };
                why = L.T("Cada deslocamento desfaz exatamente uma inversão. Com a lista ordenada, dois ponteiros nas pontas andam conforme a soma seja pequena ou grande; valores repetidos exigem contar os blocos iguais.");
                break;
        }
        return PK.Write(st, S, sol, PK.Tests(r, 7, gen, oracle), why);
    }

    static Exercise Predict10(Difficulty d, Random r)
    {
        int n = r.Next(5, 8); var v = PK.Rand(r, n, 1, 20); var o = new List<string>(); string code; var w = v.ToArray();
        switch (d)
        {
            case Difficulty.Easy:
                code = $"int[] v = {{ {PK.Join(v, ", ")} }};\nfor (int i = 0; i < v.Length - 1; i++)\n    if (v[i] > v[i + 1]) (v[i], v[i + 1]) = (v[i + 1], v[i]);\nConsole.WriteLine(string.Join(\" \", v));\n";
                for (int i = 0; i < n - 1; i++) if (w[i] > w[i + 1]) (w[i], w[i + 1]) = (w[i + 1], w[i]); o.Add(PK.Join(w)); break;
            case Difficulty.Medium:
            {
                int k = r.Next(2, 4);
                code = $"int[] v = {{ {PK.Join(v, ", ")} }};\nfor (int p = 0; p < {k}; p++)\n{{\n    for (int j = 0; j < v.Length - 1; j++)\n        if (v[j] < v[j + 1]) (v[j], v[j + 1]) = (v[j + 1], v[j]);\n    Console.WriteLine(string.Join(\" \", v));\n}}\n";
                for (int p = 0; p < k; p++) { for (int j = 0; j < n - 1; j++) if (w[j] < w[j + 1]) (w[j], w[j + 1]) = (w[j + 1], w[j]); o.Add(PK.Join(w)); }
                break;
            }
            case Difficulty.Hard:
            {
                var s = PK.Distinct(r, n + 2, 1, 40).OrderBy(x => x).ToArray(); int nn = s.Length; int x = r.Next(3) == 0 ? r.Next(1, 41) : s[r.Next(nn)];
                code = $"int[] v = {{ {PK.Join(s, ", ")} }};\nint ini = 0, fim = v.Length - 1, achou = -1;\nwhile (ini <= fim)\n{{\n    int meio = (ini + fim) / 2;\n    Console.Write(v[meio] + \" \");\n    if (v[meio] == {x}) {{ achou = meio; break; }}\n    if (v[meio] < {x}) ini = meio + 1; else fim = meio - 1;\n}}\nConsole.WriteLine();\nConsole.WriteLine(achou);\n";
                int a = 0, b = nn - 1, f = -1; var sb = new StringBuilder(); while (a <= b) { int m = (a + b) / 2; sb.Append(s[m] + " "); if (s[m] == x) { f = m; break; } if (s[m] < x) a = m + 1; else b = m - 1; }
                o.Add(sb.ToString()); o.Add(f.ToString());
                return PK.Predict(L.F("O programa procura o número {0} num vetor ordenado.", x), code, string.Join("\n", o), L.T("A busca olha sempre o elemento do meio da faixa `[ini, fim]`, descarta a metade que não pode conter o valor e repete. O programa mostra cada meio olhado e, no fim, a posição achada ou -1."));
            }
            default:
            {
                code = $"int[] v = {{ {PK.Join(v, ", ")} }};\nint desl = 0;\nfor (int i = 1; i < v.Length; i++)\n{{\n    int chave = v[i], j = i - 1;\n    while (j >= 0 && v[j] > chave) {{ v[j + 1] = v[j]; j--; desl++; }}\n    v[j + 1] = chave;\n}}\nConsole.WriteLine(desl);\nConsole.WriteLine(string.Join(\" \", v));\nConsole.WriteLine(v[{n / 2}] + v[0] - v[^1]);\n";
                int desl = 0; for (int i = 1; i < n; i++) { int c = w[i], j = i - 1; while (j >= 0 && w[j] > c) { w[j + 1] = w[j]; j--; desl++; } w[j + 1] = c; }
                o.Add(desl.ToString()); o.Add(PK.Join(w)); o.Add((w[n / 2] + w[0] - w[^1]).ToString());
                break;
            }
        }
        return PK.Predict(L.F("O vetor do programa começa com {0} números, o primeiro é {1}.", n, v[0]), code, string.Join("\n", o),
            L.T("Simule o algoritmo passo a passo anotando o vetor depois de cada passada. Uma passada de bubble sort compara vizinhos e leva o maior (ou menor) valor restante para a ponta; o insertion sort desliza os maiores para a direita até achar o lugar da chave."));
    }
}
