using System.Text;

namespace EasyCreator.Exercises;

/// <summary>C# lessons 1-5: Console output, variables and types, input and arithmetic, if/else, loops.</summary>
[Exercises("cs-01", "cs-02", "cs-03", "cs-04", "cs-05")]
public sealed class CsBasics : IExerciseGenerator
{
    const string Starter = "// escreva seu código aqui\n";

    public Exercise Make(Lesson lesson, Difficulty d, Random r) => lesson.Id switch
    {
        "cs-01" => r.Next(2) == 0 ? Write01(d, r) : Predict01(d, r),
        "cs-02" => r.Next(2) == 0 ? Write02(d, r) : Predict02(d, r),
        "cs-03" => r.Next(2) == 0 ? Write03(d, r) : Predict03(d, r),
        "cs-04" => r.Next(2) == 0 ? Write04(d, r) : Predict04(d, r),
        _ => r.Next(2) == 0 ? Write05(d, r) : Predict05(d, r),
    };

    static readonly string[] Texts = { "Bom dia, equipe!", "Hoje tem prova", "C# é divertido", "Aprender compensa", "Código limpo", "Vamos jogar", "Fim de semana chegou", "Tudo certo por aqui", "Estudar todo dia", "Meu primeiro programa" };
    static readonly string[] Names = { "Ana", "Bruno", "Carla", "Davi", "Eva", "Fabio", "Gabi", "Hugo", "Iris", "Joao", "Lia", "Mauro" };

    // ---------- cs-01 ----------
    static Exercise Write01(Difficulty d, Random r)
    {
        var pool = Texts.OrderBy(_ => r.Next()).ToList(); var lines = new List<string>(); var code = new StringBuilder();
        string st;
        if (d == Difficulty.Easy)
        {
            for (int i = 0; i < 3; i++) { lines.Add(pool[i]); code.Append($"Console.WriteLine(\"{pool[i]}\");\n"); }
            st = L.T("Escreva um programa em C# que mostre exatamente as três linhas abaixo (ele não lê nada).");
        }
        else if (d == Difficulty.Medium)
        {
            string nome = PK.Pick(r, Names); int ano = r.Next(1990, 2015), n = r.Next(5, 30);
            lines.Add($"Olá, {nome}!"); lines.Add($"Você nasceu em {ano}."); lines.Add($"Daqui a {n} anos será {ano + n}.");
            code.Append($"string nome = \"{nome}\";\nint ano = {ano};\nConsole.WriteLine($\"Olá, {{nome}}!\");\nConsole.WriteLine($\"Você nasceu em {{ano}}.\");\nConsole.WriteLine($\"Daqui a {n} anos será {{ano + {n}}}.\");\n");
            st = L.F("Crie `string nome = \"{0}\"` e `int ano = {1}` e mostre as linhas abaixo usando **interpolação** (`$\"...\"`); o ano da última linha deve ser calculado (`ano + {2}`).", nome, ano, n);
        }
        else if (d == Difficulty.Hard)
        {
            string p = PK.Pick(r, "Caderno", "Mochila", "Caneta", "Lápis"); double preco = Math.Round(r.Next(300, 9000) / 100.0, 2); int q = r.Next(2, 9); string aspas = PK.Pick(r, "oi", "vamos", "pronto");
            lines.Add($"Produto: {p}"); lines.Add($"Preço: R$ {PK.F2(preco)}"); lines.Add($"Total ({q}x): R$ {PK.F2(preco * q)}"); lines.Add($"Ele disse \"{aspas}\" e saiu de C:\\temp");
            code.Append($"string produto = \"{p}\";\ndouble preco = {preco.ToString("0.00", PK.Inv)};\nint q = {q};\nConsole.WriteLine($\"Produto: {{produto}}\");\nConsole.WriteLine($\"Preço: R$ {{preco:F2}}\");\nConsole.WriteLine($\"Total ({{q}}x): R$ {{preco * q:F2}}\");\nConsole.WriteLine(\"Ele disse \\\"{aspas}\\\" e saiu de C:\\\\temp\");\n");
            st = L.F("Crie `string produto = \"{0}\"`, `double preco = {1}` e `int q = {2}` e mostre as linhas abaixo. Valores em reais usam 2 casas decimais (`:F2`); a última linha tem aspas e uma barra invertida, que precisam de escape.", p, Ex.N(preco), q);
        }
        else
        {
            int a = r.Next(3, 40), b = r.Next(2, 9); string x = PK.Pick(r, "alfa", "beta", "gama");
            lines.Add($"[{x,-6}]|{a,5}|{b,-4}|"); lines.Add($"{a} / {b} = {PK.F2((double)a / b)}"); lines.Add($"{{{a * b}}} {a:D4}");
            code.Append($"string x = \"{x}\";\nint a = {a};\nint b = {b};\nConsole.WriteLine($\"[{{x,-6}}]|{{a,5}}|{{b,-4}}|\");\nConsole.WriteLine($\"{{a}} / {{b}} = {{(double)a / b:F2}}\");\nConsole.WriteLine($\"{{{{{{a * b}}}}}} {{a:D4}}\");\n");
            st = L.F("Com `string x = \"{0}\"`, `int a = {1}` e `int b = {2}` mostre as linhas abaixo exatamente (cuidado com espaços: `{{x,-6}}` alinha à esquerda em 6 colunas, `{{a,5}}` à direita em 5; `{{{{` e `}}}}` mostram chaves; `:D4` completa com zeros).", x, a, b);
        }
        string pre = d == Difficulty.Genius ? $"string x = \"{lines[0].Substring(1, 5).Trim()}\";\nint a = 0;\n" : "";
        return PK.Write(st, Starter, code.ToString(), PK.Fixed(string.Join("\n", lines)),
            L.T("`Console.WriteLine` mostra o texto e muda de linha. Com `$\"...\"` o que está entre `{}` é calculado; `:F2` fixa 2 decimais, `,-6` alinha à esquerda em 6 colunas e `\\\"` mostra uma aspas."));
    }

    static Exercise Predict01(Difficulty d, Random r)
    {
        int a = r.Next(2, 30), b = r.Next(2, 9); string w = PK.Pick(r, Names); var o = new StringBuilder(); var c = new StringBuilder();
        c.Append($"Console.Write(\"{w}\");\nConsole.Write(\" \");\nConsole.WriteLine({a});\n"); o.Append($"{w} {a}\n");
        if (d >= Difficulty.Medium) { c.Append($"Console.WriteLine({a} + {b});\nConsole.WriteLine(\"{a}\" + {b});\nConsole.WriteLine($\"{{{{{{{a} * {b}}}}}}}\");\n"); o.Append($"{a + b}\n{a}{b}\n{{{a * b}}}\n"); }
        if (d >= Difficulty.Hard) { c.Append($"Console.WriteLine(\"A\\tB\".Length);\nConsole.WriteLine(\"x\\\\y\" + 'z');\nConsole.WriteLine(\"{{\" + {a} + \"}}\");\n"); o.Append($"3\nx\\yz\n{{{a}}}\n"); }
        if (d == Difficulty.Genius) { c.Append($"Console.Write({a} + \"-\" + {b});\nConsole.Write('\\n');\nConsole.WriteLine('a' + 1);\nConsole.WriteLine((char)('a' + {b}));\n"); o.Append($"{a}-{b}\n98\n{(char)('a' + b)}\n"); }
        return PK.Predict(L.F("O programa usa os números {0} e {1} e o nome {2}.", a, b, w), c.ToString(), o.ToString().TrimEnd('\n'),
            L.T("`Write` não muda de linha; `WriteLine` muda. `número + texto` vira texto (concatenação), mas `número + número` soma. Um `char` somado a um `int` vira número; com `(char)` volta a ser letra."));
    }

    // ---------- cs-02 ----------
    static Exercise Write02(Difficulty d, Random r)
    {
        string nome = PK.Pick(r, Names); int idade = r.Next(12, 70); double altura = Math.Round(1.4 + r.Next(0, 60) / 100.0, 2); bool est = r.Next(2) == 0; int k = r.Next(3, 20); char ch = (char)('A' + r.Next(0, 24)); double preco = Math.Round(r.Next(100, 9000) / 100.0, 2);
        var lines = new List<string>(); var code = new StringBuilder();
        code.Append($"string nome = \"{nome}\";\nint idade = {idade};\ndouble altura = {altura.ToString("0.00", PK.Inv)};\nbool estudante = {(est ? "true" : "false")};\nchar letra = '{ch}';\n");
        var all = new List<(string line, string expr, string val)>
        {
            ("Nome", "nome", nome), ("Idade", "idade", idade.ToString()), ("Altura", "altura:F2", PK.F2(altura)), ("Estudante", "estudante", est ? "True" : "False"),
            ($"Daqui a {k} anos", $"idade + {k}", (idade + k).ToString()), ("Metade da idade", "idade / 2", (idade / 2).ToString()), ("Dobro da altura", "altura * 2:F2", PK.F2(altura * 2)),
            ("Tamanho do nome", "nome.Length", nome.Length.ToString()),
        };
        int cnt = d switch { Difficulty.Easy => 4, Difficulty.Medium => 5, _ => 6 };
        var chosen = all.Take(4).Concat(all.Skip(4).OrderBy(_ => r.Next()).Take(cnt - 4)).ToList();
        foreach (var (line, expr, val) in chosen) { lines.Add($"{line}: {val}"); code.Append($"Console.WriteLine($\"{line}: {{{expr}}}\");\n"); }
        if (d >= Difficulty.Hard) { lines.Add($"Próxima letra: {(char)(ch + 1)}"); code.Append("Console.WriteLine($\"Próxima letra: {(char)(letra + 1)}\");\n"); }
        if (d == Difficulty.Genius) { lines.Add($"Código: {(int)ch}"); lines.Add($"IMC com peso 70: {PK.F2(70 / (altura * altura))}"); code.Append("Console.WriteLine($\"Código: {(int)letra}\");\nConsole.WriteLine($\"IMC com peso 70: {70 / (altura * altura):F2}\");\n"); }
        string decl = $"string nome = \"{nome}\";\nint idade = {idade};\ndouble altura = {altura.ToString("0.00", PK.Inv)};\nbool estudante = {(est ? "true" : "false")};\nchar letra = '{ch}';\n";
        string st = L.F("Crie as variáveis `string nome = \"{0}\"`, `int idade = {1}`, `double altura = {2}`, `bool estudante = {3}` e `char letra = '{4}'` e mostre exatamente as linhas abaixo, **calculando** cada valor a partir das variáveis (o que é decimal usa 2 casas).", nome, idade, Ex.N(altura), est ? "true" : "false", ch);
        st += "\n\n" + L.T("Dicas: `idade / 2` é divisão inteira; `(char)(letra + 1)` dá a letra seguinte; `nome.Length` é o tamanho do texto; o IMC é `peso / (altura * altura)`.");
        return PK.Write(st, decl + "// mostre as linhas aqui\n", code.ToString(), PK.Fixed(string.Join("\n", lines)),
            L.T("Cada tipo guarda um tipo de valor: `string` texto, `int` inteiro, `double` real, `bool` verdadeiro/falso, `char` uma letra. Divisão entre `int` descarta a parte decimal; `bool` aparece como `True`/`False`."));
    }

    static Exercise Predict02(Difficulty d, Random r)
    {
        int a = r.Next(7, 60), b = r.Next(2, 8); double x = r.Next(10, 99) / 10.0; var c = new StringBuilder(); var o = new List<string>();
        c.Append($"int a = {a};\nint b = {b};\nConsole.WriteLine(a / b);\nConsole.WriteLine(a % b);\nConsole.WriteLine((double)a / b > a / b);\n"); o.Add((a / b).ToString()); o.Add((a % b).ToString()); o.Add(((double)a / b > a / b) ? "True" : "False");
        c.Append($"double x = {x.ToString("0.0", PK.Inv)};\nConsole.WriteLine((int)x);\n"); o.Add(((int)x).ToString());
        if (d >= Difficulty.Medium) { c.Append("double e = a / b;\nConsole.WriteLine(e);\nConsole.WriteLine(a / (double)b == e);\n"); o.Add(((double)(a / b)).ToString(PK.Inv)); o.Add((a / (double)b == (double)(a / b)) ? "True" : "False"); }
        if (d >= Difficulty.Hard) { c.Append("string s = \"v\" + a + b;\nConsole.WriteLine(s);\nConsole.WriteLine(1 + 2 + \"x\" + 1 + 2);\nvar t = a > b ? \"maior\" : \"menor\";\nConsole.WriteLine(t);\n"); o.Add($"v{a}{b}"); o.Add("3x12"); o.Add(a > b ? "maior" : "menor"); }
        if (d == Difficulty.Genius) { c.Append("long big = int.MaxValue;\nbig = big + a;\nint wrap = unchecked((int)big);\nConsole.WriteLine(big);\nConsole.WriteLine(wrap < 0);\nbyte by = (byte)(a * 10);\nConsole.WriteLine(by);\n"); long big = (long)int.MaxValue + a; o.Add(big.ToString()); o.Add(unchecked((int)big) < 0 ? "True" : "False"); o.Add(unchecked((byte)(a * 10)).ToString()); }
        return PK.Predict(L.F("O programa começa com a = {0} e b = {1}.", a, b), c.ToString(), string.Join("\n", o),
            L.T("`int / int` é divisão inteira e `%` o resto. `(double)a / b` converte antes de dividir; `(int)x` corta os decimais. Guardar uma divisão inteira num `double` não recupera as casas. `número + texto` vira texto. Um `byte` guarda de 0 a 255 e dá a volta."));
    }

    // ---------- cs-03 ----------
    static Exercise Write03(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
            {
                int kind = r.Next(3);
                if (kind == 0)
                {
                    st = L.T("Leia a base e a altura de um retângulo (inteiros, uma por linha) e mostre `Área: <b*h>` e `Perímetro: <2*(b+h)>`.");
                    sol = "int b = int.Parse(Console.ReadLine());\nint h = int.Parse(Console.ReadLine());\nConsole.WriteLine($\"Área: {b * h}\");\nConsole.WriteLine($\"Perímetro: {2 * (b + h)}\");\n";
                    gen = g => $"{g.Next(1, 40)}\n{g.Next(1, 40)}\n"; oracle = i => { var v = PK.Ints(i); return $"Área: {v[0] * v[1]}\nPerímetro: {2 * (v[0] + v[1])}"; };
                }
                else if (kind == 1)
                {
                    st = L.T("Leia dois inteiros `a` e `b` (um por linha, `b` nunca é zero) e mostre `Quociente: <a/b inteiro>`, `Resto: <a%b>` e `Real: <a/b com 3 casas decimais>`.");
                    sol = "int a = int.Parse(Console.ReadLine());\nint b = int.Parse(Console.ReadLine());\nConsole.WriteLine($\"Quociente: {a / b}\");\nConsole.WriteLine($\"Resto: {a % b}\");\nConsole.WriteLine($\"Real: {(double)a / b:F3}\");\n";
                    gen = g => $"{g.Next(1, 200)}\n{g.Next(1, 15)}\n"; oracle = i => { var v = PK.Ints(i); return $"Quociente: {v[0] / v[1]}\nResto: {v[0] % v[1]}\nReal: {((double)v[0] / v[1]).ToString("F3", PK.Inv)}"; };
                }
                else
                {
                    st = L.T("Leia o preço de um produto e a quantidade (inteiros, um por linha). Mostre `Total: <preço*quantidade>` e `Com 10% de desconto: <total com 2 casas decimais>`.");
                    sol = "int p = int.Parse(Console.ReadLine());\nint q = int.Parse(Console.ReadLine());\nConsole.WriteLine($\"Total: {p * q}\");\nConsole.WriteLine($\"Com 10% de desconto: {p * q * 0.9:F2}\");\n";
                    gen = g => $"{g.Next(1, 100)}\n{g.Next(1, 20)}\n"; oracle = i => { var v = PK.Ints(i); return $"Total: {v[0] * v[1]}\nCom 10% de desconto: {PK.F2(v[0] * v[1] * 0.9)}"; };
                }
                why = L.T("`int.Parse(Console.ReadLine())` lê uma linha e converte em inteiro. Divisão de inteiros é inteira; para casas decimais converta um operando com `(double)`.");
                break;
            }
            case Difficulty.Medium:
                if (r.Next(2) == 0)
                {
                    st = L.T("Leia uma duração em segundos (um inteiro) e mostre-a como `<h>h <m>min <s>s`; por exemplo `3725` vira `1h 2min 5s`.");
                    sol = "int t = int.Parse(Console.ReadLine());\nConsole.WriteLine($\"{t / 3600}h {t % 3600 / 60}min {t % 60}s\");\n";
                    gen = g => $"{g.Next(0, 90000)}\n"; oracle = i => { int t = PK.Ints(i)[0]; return $"{t / 3600}h {t % 3600 / 60}min {t % 60}s"; };
                    why = L.T("Horas = t / 3600; do que sobra (`t % 3600`) saem os minutos (`/ 60`) e os segundos (`% 60`).");
                }
                else
                {
                    st = L.T("Leia três notas decimais (com ponto, uma por linha) e mostre `Média: <média com 2 casas>` e `Maior: <maior nota com 1 casa>`.");
                    sol = "double a = double.Parse(Console.ReadLine());\ndouble b = double.Parse(Console.ReadLine());\ndouble c = double.Parse(Console.ReadLine());\nConsole.WriteLine($\"Média: {(a + b + c) / 3:F2}\");\nConsole.WriteLine($\"Maior: {Math.Max(a, Math.Max(b, c)):F1}\");\n";
                    gen = g => $"{g.Next(0, 101) / 10.0:0.0}\n{g.Next(0, 101) / 10.0:0.0}\n{g.Next(0, 101) / 10.0:0.0}\n".Replace(',', '.');
                    oracle = i => { var v = i.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(x => double.Parse(x, PK.Inv)).ToArray(); return $"Média: {PK.F2(v.Sum() / 3)}\nMaior: {v.Max().ToString("F1", PK.Inv)}"; };
                    why = L.T("`double.Parse` lê números decimais com ponto. `Math.Max` devolve o maior de dois valores; para três, aninhe as chamadas.");
                }
                break;
            case Difficulty.Hard:
                st = L.T("Leia um número de 3 dígitos (de 100 a 999) e mostre `Centena: <c>`, `Dezena: <d>`, `Unidade: <u>`, `Soma: <soma dos dígitos>` e `Invertido: <o número escrito de trás para frente>`, usando só `/` e `%`.");
                sol = "int n = int.Parse(Console.ReadLine());\nint c = n / 100, d = n / 10 % 10, u = n % 10;\nConsole.WriteLine($\"Centena: {c}\");\nConsole.WriteLine($\"Dezena: {d}\");\nConsole.WriteLine($\"Unidade: {u}\");\nConsole.WriteLine($\"Soma: {c + d + u}\");\nConsole.WriteLine($\"Invertido: {u * 100 + d * 10 + c}\");\n";
                gen = g => $"{g.Next(100, 1000)}\n"; oracle = i => { int n = PK.Ints(i)[0]; int c = n / 100, dd = n / 10 % 10, u = n % 10; return $"Centena: {c}\nDezena: {dd}\nUnidade: {u}\nSoma: {c + dd + u}\nInvertido: {u * 100 + dd * 10 + c}"; };
                why = L.T("`n / 100` é a centena, `n / 10 % 10` a dezena e `n % 10` a unidade. O invertido remonta os dígitos com os pesos trocados.");
                break;
            default:
                st = L.T("Leia a hora e o minuto de partida e a duração de uma viagem em minutos (três inteiros, um por linha). Mostre o horário de chegada como `HH:MM` (dois dígitos cada) e, na linha seguinte, `+<dias>d` com quantos dias a mais se passaram (0 se for no mesmo dia).");
                sol = "int h = int.Parse(Console.ReadLine());\nint m = int.Parse(Console.ReadLine());\nint dur = int.Parse(Console.ReadLine());\nint total = h * 60 + m + dur;\nConsole.WriteLine($\"{total / 60 % 24:D2}:{total % 60:D2}\");\nConsole.WriteLine($\"+{total / 1440}d\");\n";
                gen = g => $"{g.Next(0, 24)}\n{g.Next(0, 60)}\n{g.Next(1, 4000)}\n"; oracle = i => { var v = PK.Ints(i); int t = v[0] * 60 + v[1] + v[2]; return $"{t / 60 % 24:00}:{t % 60:00}\n+{t / 1440}d"; };
                why = L.T("Converta tudo em minutos desde a meia-noite. As horas na tela são `total / 60 % 24`, os minutos `total % 60` e os dias a mais `total / 1440`. `:D2` completa com zero à esquerda.");
                break;
        }
        return PK.Write(st, "// leia as entradas e mostre os resultados\n", sol, PK.Tests(r, 5, gen, oracle), why);
    }

    static Exercise Predict03(Difficulty d, Random r)
    {
        int a = r.Next(10, 99), b = r.Next(2, 9), c = r.Next(2, 6); var code = new StringBuilder($"int a = {a};\nint b = {b};\nint c = {c};\n"); var o = new List<string>();
        code.Append("Console.WriteLine(a + b * c);\nConsole.WriteLine((a + b) * c);\nConsole.WriteLine($\"{a / b / c} {a % b % c}\");\n"); o.Add((a + b * c).ToString()); o.Add(((a + b) * c).ToString()); o.Add($"{a / b / c} {a % b % c}");
        if (d >= Difficulty.Medium) { code.Append("Console.WriteLine(-a / b);\nConsole.WriteLine(-a % b);\n"); o.Add((-a / b).ToString()); o.Add((-a % b).ToString()); }
        if (d >= Difficulty.Hard) { code.Append("Console.WriteLine((a + b) / c);\nConsole.WriteLine((a + b) / (double)c);\nConsole.WriteLine(Math.Round((a + b) / (double)c, 1));\n"); o.Add(((a + b) / c).ToString()); o.Add(((a + b) / (double)c).ToString(PK.Inv)); o.Add(Math.Round((a + b) / (double)c, 1).ToString(PK.Inv)); }
        if (d == Difficulty.Genius) { code.Append("a = a - a % 10;\nb = b * 2 - c;\na += b;\nb *= a;\nConsole.WriteLine($\"{a} {b}\");\nConsole.WriteLine(a++ + ++b);\nConsole.WriteLine($\"{a} {b}\");\n"); int a2 = a - a % 10, b2 = b * 2 - c; a2 += b2; b2 *= a2; o.Add($"{a2} {b2}"); o.Add((a2 + b2 + 1).ToString()); o.Add($"{a2 + 1} {b2 + 1}"); }
        return PK.Predict(L.F("Os valores iniciais são a = {0}, b = {1} e c = {2}.", a, b, c), code.ToString(), string.Join("\n", o),
            L.T("`*`, `/` e `%` vêm antes de `+` e `-`; os parênteses mandam primeiro. Divisão e resto de negativos truncam em direção a zero. `a++` usa o valor e depois soma; `++b` soma e depois usa."));
    }

    // ---------- cs-04 ----------
    static Exercise Write04(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                {
                    int k = r.Next(3, 10);
                    st = L.F("Leia um inteiro `n`. Mostre `<n> é múltiplo de {0}` se for múltiplo de {0}, senão `<n> não é múltiplo de {0}`.", k);
                    sol = $"int n = int.Parse(Console.ReadLine());\nif (n % {k} == 0)\n    Console.WriteLine($\"{{n}} é múltiplo de {k}\");\nelse\n    Console.WriteLine($\"{{n}} não é múltiplo de {k}\");\n";
                    gen = g => $"{g.Next(1, 200)}\n"; oracle = i => { int n = PK.Ints(i)[0]; return n % k == 0 ? $"{n} é múltiplo de {k}" : $"{n} não é múltiplo de {k}"; };
                    why = L.T("Um número é múltiplo de k quando o resto da divisão por k é zero: `n % k == 0`.");
                }
                else
                {
                    st = L.T("Leia um inteiro. Mostre `positivo` se for maior que zero, `negativo` se menor e `zero` se for zero.");
                    sol = "int n = int.Parse(Console.ReadLine());\nif (n > 0) Console.WriteLine(\"positivo\");\nelse if (n < 0) Console.WriteLine(\"negativo\");\nelse Console.WriteLine(\"zero\");\n";
                    gen = g => $"{g.Next(-9, 10)}\n"; oracle = i => { int n = PK.Ints(i)[0]; return n > 0 ? "positivo" : n < 0 ? "negativo" : "zero"; };
                    why = L.T("Três casos exclusivos: uma cadeia `if / else if / else` escolhe só um.");
                }
                break;
            case Difficulty.Medium:
                st = L.T("Leia os três lados de um triângulo (inteiros, um por linha). Se algum lado for maior ou igual à soma dos outros dois, mostre `não forma triângulo`. Senão mostre `equilátero` (3 iguais), `isósceles` (exatamente 2 iguais) ou `escaleno` (todos diferentes).");
                sol = "int a = int.Parse(Console.ReadLine());\nint b = int.Parse(Console.ReadLine());\nint c = int.Parse(Console.ReadLine());\nif (a >= b + c || b >= a + c || c >= a + b)\n    Console.WriteLine(\"não forma triângulo\");\nelse if (a == b && b == c)\n    Console.WriteLine(\"equilátero\");\nelse if (a == b || b == c || a == c)\n    Console.WriteLine(\"isósceles\");\nelse\n    Console.WriteLine(\"escaleno\");\n";
                gen = g => { int a = g.Next(1, 8), b = g.Next(1, 8), c = g.Next(1, 8); return g.Next(4) == 0 ? $"{a}\n{a}\n{a}\n" : $"{a}\n{b}\n{c}\n"; };
                oracle = i => { var v = PK.Ints(i); int a = v[0], b = v[1], c = v[2]; return a >= b + c || b >= a + c || c >= a + b ? "não forma triângulo" : a == b && b == c ? "equilátero" : a == b || b == c || a == c ? "isósceles" : "escaleno"; };
                why = L.T("Primeiro a existência do triângulo (cada lado menor que a soma dos outros); depois conte quantos lados são iguais, do caso mais restrito ao mais geral.");
                break;
            case Difficulty.Hard:
            {
                int t1 = r.Next(2, 6) * 50, t2 = t1 + r.Next(2, 6) * 50, p1 = r.Next(1, 4) * 5, p2 = p1 + r.Next(1, 4) * 5;
                st = L.F("Leia o valor de uma compra (inteiro). Abaixo de {0} não há desconto; de {0} até {1} (inclusive) o desconto é {2}%; acima de {1} é {3}%. Mostre `Desconto: <p>%` e, na linha seguinte, `Total: <valor final com 2 casas decimais>`.", t1, t2, p1, p2);
                sol = $"int v = int.Parse(Console.ReadLine());\nint p;\nif (v < {t1}) p = 0;\nelse if (v <= {t2}) p = {p1};\nelse p = {p2};\nConsole.WriteLine($\"Desconto: {{p}}%\");\nConsole.WriteLine($\"Total: {{v * (100 - p) / 100.0:F2}}\");\n";
                gen = g => $"{g.Next(10, t2 + 150)}\n"; oracle = i => { int v = PK.Ints(i)[0]; int p = v < t1 ? 0 : v <= t2 ? p1 : p2; return $"Desconto: {p}%\nTotal: {PK.F2(v * (100 - p) / 100.0)}"; };
                why = L.T("Teste as faixas em ordem crescente. `v * (100 - p) / 100.0` calcula o preço final sem perder centavos (o `100.0` torna a divisão real).");
                break;
            }
            default:
                st = L.T("Leia dia, mês e ano (três inteiros, um por linha). Mostre `válida` se a data existe (fevereiro tem 29 dias em ano bissexto: divisível por 4 e não por 100, ou por 400) ou `inválida` caso contrário. Use `switch` ou `if`.");
                sol = "int dia = int.Parse(Console.ReadLine());\nint mes = int.Parse(Console.ReadLine());\nint ano = int.Parse(Console.ReadLine());\nif (mes < 1 || mes > 12 || dia < 1)\n{\n    Console.WriteLine(\"inválida\");\n    return;\n}\nbool bissexto = (ano % 4 == 0 && ano % 100 != 0) || ano % 400 == 0;\nint max = mes switch\n{\n    2 => bissexto ? 29 : 28,\n    4 or 6 or 9 or 11 => 30,\n    _ => 31\n};\nConsole.WriteLine(dia <= max ? \"válida\" : \"inválida\");\n";
                gen = g => $"{g.Next(1, 33)}\n{g.Next(1, 14)}\n{PK.Pick(g, 1900, 2000, 2023, 2024, 2100, 2400)}\n";
                oracle = i => { var v = PK.Ints(i); int dia = v[0], mes = v[1], ano = v[2]; if (mes < 1 || mes > 12 || dia < 1) return "inválida"; int max = mes == 2 ? ((ano % 4 == 0 && ano % 100 != 0) || ano % 400 == 0 ? 29 : 28) : mes is 4 or 6 or 9 or 11 ? 30 : 31; return dia <= max ? "válida" : "inválida"; };
                why = L.T("Descubra o último dia do mês (28/29/30/31 conforme o mês e o ano bissexto) e compare. Confira o mês fora de 1..12 antes de usar a tabela.");
                break;
        }
        return PK.Write(st, "// leia as entradas e decida\n", sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict04(Difficulty d, Random r)
    {
        int x = r.Next(1, 40), y = r.Next(1, 40), t1 = r.Next(5, 15), t2 = t1 + r.Next(5, 15);
        var c = new StringBuilder($"int x = {x};\nint y = {y};\n"); var o = new List<string>();
        c.Append($"if (x < {t1}) Console.WriteLine(\"baixo\");\nelse if (x < {t2}) Console.WriteLine(\"medio\");\nelse Console.WriteLine(\"alto\");\n"); o.Add(x < t1 ? "baixo" : x < t2 ? "medio" : "alto");
        if (d >= Difficulty.Medium) { c.Append("if (x > y) Console.WriteLine(\"x\");\nConsole.WriteLine(x % 2 == 0 ? \"par\" : \"impar\");\n"); if (x > y) o.Add("x"); o.Add(x % 2 == 0 ? "par" : "impar"); }
        if (d >= Difficulty.Hard) { c.Append($"if (x > {t1} && y > {t1} || x + y == {t2}) Console.WriteLine(\"A\");\nelse if (!(x < y)) Console.WriteLine(\"B\");\nelse Console.WriteLine(\"C\");\n"); o.Add(x > t1 && y > t1 || x + y == t2 ? "A" : !(x < y) ? "B" : "C"); }
        if (d == Difficulty.Genius)
        {
            c.Append($"int n = 0;\nif (x > {t1} || ++n > 0) n += 10;\nif (y < {t1} && ++n > 0) n += 100;\nstring s = x switch\n{{\n    < {t1} => \"a\",\n    < {t2} => \"b\",\n    _ => \"c\"\n}};\nConsole.WriteLine(s + n);\n");
            int n = 0; if (x > t1 || ++n > 0) n += 10; if (y < t1 && ++n > 0) n += 100; o.Add((x < t1 ? "a" : x < t2 ? "b" : "c") + n);
        }
        return PK.Predict(L.F("O programa começa com x = {0} e y = {1}.", x, y), c.ToString(), string.Join("\n", o),
            L.T("Siga cada `if` com os valores atuais; numa cadeia `else if` só o primeiro teste verdadeiro executa. Em `||` e `&&` o segundo lado só é calculado se o primeiro não decidir o resultado, então um `++n` ali pode nem acontecer."));
    }

    // ---------- cs-05 ----------
    static Exercise Write05(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
                if (r.Next(2) == 0)
                {
                    int k = r.Next(2, 8);
                    st = L.F("Leia um inteiro `n` e mostre, em uma linha separados por espaço, os múltiplos de {0} de {0} até `n` (inclusive). Se não houver nenhum, mostre `nenhum`.", k);
                    sol = $"int n = int.Parse(Console.ReadLine());\nvar lista = new List<int>();\nfor (int i = {k}; i <= n; i += {k})\n    lista.Add(i);\nConsole.WriteLine(lista.Count == 0 ? \"nenhum\" : string.Join(\" \", lista));\n";
                    gen = g => $"{g.Next(1, 45)}\n"; oracle = i => { int n = PK.Ints(i)[0]; var l = new List<int>(); for (int j = k; j <= n; j += k) l.Add(j); return l.Count == 0 ? "nenhum" : PK.Join(l); };
                    why = L.T("O `for` começa no primeiro múltiplo e anda de k em k; `string.Join` junta a lista com espaços.");
                }
                else
                {
                    st = L.T("Leia `n` (n >= 1) e mostre `Soma: <1+2+...+n>` e `Quadrados: <soma de 1²+2²+...+n²>`.");
                    sol = "int n = int.Parse(Console.ReadLine());\nlong s = 0, q = 0;\nfor (int i = 1; i <= n; i++)\n{\n    s += i;\n    q += (long)i * i;\n}\nConsole.WriteLine($\"Soma: {s}\");\nConsole.WriteLine($\"Quadrados: {q}\");\n";
                    gen = g => $"{g.Next(1, 60)}\n"; oracle = i => { int n = PK.Ints(i)[0]; long s = 0, q = 0; for (int j = 1; j <= n; j++) { s += j; q += j * j; } return $"Soma: {s}\nQuadrados: {q}"; };
                    why = L.T("Um acumulador começa em zero e recebe cada termo dentro do laço.");
                }
                break;
            case Difficulty.Medium:
                st = L.T("Leia `n` e depois `n` inteiros (um por linha). Mostre `Positivos: <quantos são maiores que zero>`, `Soma dos pares: <soma dos valores pares>` e `Maior sequência crescente: <tamanho da maior sequência de elementos consecutivos estritamente crescentes>`. Se `n` for 0, mostre só `Lista vazia`.");
                sol = "int n = int.Parse(Console.ReadLine());\nif (n == 0)\n{\n    Console.WriteLine(\"Lista vazia\");\n    return;\n}\nint pos = 0, soma = 0, seq = 0, melhor = 0, ant = 0;\nfor (int i = 0; i < n; i++)\n{\n    int x = int.Parse(Console.ReadLine());\n    if (x > 0) pos++;\n    if (x % 2 == 0) soma += x;\n    seq = (i > 0 && x > ant) ? seq + 1 : 1;\n    if (seq > melhor) melhor = seq;\n    ant = x;\n}\nConsole.WriteLine($\"Positivos: {pos}\");\nConsole.WriteLine($\"Soma dos pares: {soma}\");\nConsole.WriteLine($\"Maior sequência crescente: {melhor}\");\n";
                gen = g => { int n = g.Next(0, 9); return n + "\n" + string.Join("", Enumerable.Range(0, n).Select(_ => g.Next(-6, 15) + "\n")); };
                oracle = i => { var v = PK.Ints(i); int n = v[0]; if (n == 0) return "Lista vazia"; int pos = 0, soma = 0, seq = 0, best = 0, ant = 0; for (int j = 0; j < n; j++) { int x = v[j + 1]; if (x > 0) pos++; if (x % 2 == 0) soma += x; seq = j > 0 && x > ant ? seq + 1 : 1; best = Math.Max(best, seq); ant = x; } return $"Positivos: {pos}\nSoma dos pares: {soma}\nMaior sequência crescente: {best}"; };
                why = L.T("Três contadores no mesmo laço. A sequência cresce enquanto `x > ant` e recomeça em 1 quando quebra; guarde a maior.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia um inteiro `n` (n >= 0). Mostre `Dígitos: <quantidade de dígitos>`, `Soma: <soma dos dígitos>` e `Invertido: <n escrito de trás para frente, sem zeros à esquerda>` (para n = 0: 1 dígito, soma 0 e invertido 0). Use `while` com `/` e `%`.");
                sol = "int n = int.Parse(Console.ReadLine());\nint dig = n == 0 ? 1 : 0, soma = 0, inv = 0;\nwhile (n > 0)\n{\n    soma += n % 10;\n    inv = inv * 10 + n % 10;\n    n /= 10;\n    dig++;\n}\nConsole.WriteLine($\"Dígitos: {dig}\");\nConsole.WriteLine($\"Soma: {soma}\");\nConsole.WriteLine($\"Invertido: {inv}\");\n";
                gen = g => $"{PK.Pick(g, 0, g.Next(1, 10), g.Next(10, 1000), g.Next(1000, 100000), g.Next(1, 10) * 1000, g.Next(10000, 900000) / 10 * 10)}\n";
                oracle = i => { int n = PK.Ints(i)[0]; int soma = 0, dig = n == 0 ? 1 : 0, inv = 0, m = n; while (m > 0) { soma += m % 10; inv = inv * 10 + m % 10; m /= 10; dig++; } return $"Dígitos: {dig}\nSoma: {soma}\nInvertido: {inv}"; };
                why = L.T("A cada volta `n % 10` é o último dígito e `n /= 10` o descarta. O invertido é montado multiplicando por 10 e somando o dígito. O caso n = 0 nunca entra no laço.");
                break;
            default:
                st = L.T("Leia um inteiro `n` (1 <= n <= 5000). Siga a sequência de Collatz: se o número for par, divida por 2; se for ímpar, multiplique por 3 e some 1; pare ao chegar em 1. Mostre `Passos: <quantas operações>` e `Máximo: <maior valor atingido, incluindo n>`.");
                sol = "long n = long.Parse(Console.ReadLine());\nlong max = n;\nint passos = 0;\nwhile (n != 1)\n{\n    n = n % 2 == 0 ? n / 2 : 3 * n + 1;\n    passos++;\n    if (n > max) max = n;\n}\nConsole.WriteLine($\"Passos: {passos}\");\nConsole.WriteLine($\"Máximo: {max}\");\n";
                gen = g => $"{g.Next(1, 5001)}\n";
                oracle = i => { long n = PK.Ints(i)[0], max = n; int p = 0; while (n != 1) { n = n % 2 == 0 ? n / 2 : 3 * n + 1; p++; max = Math.Max(max, n); } return $"Passos: {p}\nMáximo: {max}"; };
                why = L.T("Um `while (n != 1)` aplica a regra e conta os passos; o máximo é atualizado a cada novo valor. Use `long`: os valores podem passar de 32 mil.");
                break;
        }
        return PK.Write(st, "// leia a entrada e use um laço\n", sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict05(Difficulty d, Random r)
    {
        var c = new StringBuilder(); var o = new List<string>(); int a = r.Next(2, 5), n = r.Next(5, 12);
        switch (d)
        {
            case Difficulty.Easy:
            {
                int s0 = r.Next(1, 6), st = r.Next(2, 5);
                c.Append($"int i;\nfor (i = {s0}; i <= {n + s0}; i += {st})\n    Console.Write(i + \" \");\nConsole.WriteLine();\nConsole.WriteLine(i);\n");
                var l = new List<int>(); int i2; for (i2 = s0; i2 <= n + s0; i2 += st) l.Add(i2); o.Add(PK.Join(l) + " "); o.Add(i2.ToString());
                break;
            }
            case Difficulty.Medium:
            {
                c.Append($"int s = 0, k = {a};\nfor (int i = 1; i <= {n}; i++)\n{{\n    if (i % k == 0) continue;\n    s += i;\n}}\nConsole.WriteLine(s);\nwhile (k < {n * 3}) k *= 2;\nConsole.WriteLine(k);\n");
                int s = 0, k = a; for (int i = 1; i <= n; i++) { if (i % k == 0) continue; s += i; } o.Add(s.ToString()); while (k < n * 3) k *= 2; o.Add(k.ToString());
                break;
            }
            case Difficulty.Hard:
            {
                int m = r.Next(3, 6);
                c.Append($"for (int i = 1; i <= {m}; i++)\n{{\n    var partes = new List<int>();\n    for (int j = 1; j <= i; j++)\n    {{\n        if (j == {a}) break;\n        partes.Add(i * j);\n    }}\n    Console.WriteLine(string.Join(\",\", partes));\n}}\n");
                for (int i = 1; i <= m; i++) { var p = new List<int>(); for (int j = 1; j <= i; j++) { if (j == a) break; p.Add(i * j); } o.Add(string.Join(",", p)); }
                break;
            }
            default:
            {
                int lim = r.Next(20, 60);
                c.Append($"int i = 1, total = 0, cont = 0;\nwhile (true)\n{{\n    i = i * {a} + 1;\n    if (i % 2 == 0) continue;\n    total += i % 10;\n    cont++;\n    if (total > {lim} || cont == 6) break;\n}}\nConsole.WriteLine($\"{{i}} {{total}} {{cont}}\");\n");
                int i2 = 1, tot = 0, cont2 = 0; while (true) { i2 = i2 * a + 1; if (i2 % 2 == 0) continue; tot += i2 % 10; cont2++; if (tot > lim || cont2 == 6) break; }
                o.Add($"{i2} {tot} {cont2}");
                break;
            }
        }
        return PK.Predict(L.F("O programa usa o número {0} como valor-chave.", a * 100 + n), c.ToString(), string.Join("\n", o),
            L.T("Execute o laço volta a volta anotando as variáveis. `continue` pula para a próxima volta e `break` sai do laço mais interno."));
    }
}
