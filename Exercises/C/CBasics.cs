using System.Text;

namespace EasyCreator.Exercises;

/// <summary>C lessons 1-3: printf, variables and types, input and arithmetic. Alternates between "write the program"
/// and "what does it print".</summary>
[Exercises("c-01", "c-02", "c-03")]
public sealed class CBasics : IExerciseGenerator
{
    const string Starter = "#include <stdio.h>\n\nint main(void) {\n    // escreva seu código aqui\n    return 0;\n}\n";

    public Exercise Make(Lesson lesson, Difficulty d, Random r) => lesson.Id switch
    {
        "c-01" => r.Next(2) == 0 ? Write01(d, r) : Predict01(d, r),
        "c-02" => r.Next(2) == 0 ? Write02(d, r) : Predict02(d, r),
        _ => r.Next(2) == 0 ? Write03(d, r) : Predict03(d, r),
    };

    // ---------- c-01 ----------
    static readonly string[] Texts = { "Bom dia, equipe!", "Hoje tem prova", "C e rapido", "Aprender e divertido", "Codigo limpo",
        "Ola, mundo!", "Meu primeiro programa", "Vamos jogar", "Fim de semana chegou", "Tudo certo por aqui", "Estudar compensa" };

    static Exercise Write01(Difficulty d, Random r)
    {
        var lines = new List<string>(); var calls = new List<string>();
        var pool = Texts.OrderBy(_ => r.Next()).ToList();
        for (int i = 0; i < 2; i++) { lines.Add(pool[i]); calls.Add($"printf(\"{PK.Esc(pool[i], true)}\\n\");"); }
        string decl = "";
        if (d >= Difficulty.Medium)
        {
            int pct = r.Next(5, 100);
            string label = PK.Pick(r, "Bateria", "Progresso", "Desconto", "Carga");
            lines.Add($"{label}: {pct}%"); calls.Add($"printf(\"{label}: {pct}%%\\n\");");
        }
        if (d >= Difficulty.Hard)
        {
            string w = PK.Pick(r, "oi", "vamos", "bora", "pronto");
            string dir = PK.Pick(r, "jogos", "fotos", "notas", "musica");
            lines.Add($"Ele disse \"{w}\""); calls.Add($"printf(\"Ele disse \\\"{w}\\\"\\n\");");
            lines.Add($"Pasta: C:\\{dir}"); calls.Add($"printf(\"Pasta: C:\\\\{dir}\\n\");");
        }
        int ga = 0, gb = 0;
        if (d == Difficulty.Genius)
        {
            ga = r.Next(12, 90); gb = r.Next(3, 12);
            decl = $"    int a = {ga};\n    int b = {gb};\n";
            lines.Add($"{ga} + {gb} = {ga + gb}"); calls.Add("printf(\"%d + %d = %d\\n\", a, b, a + b);");
            lines.Add($"{ga} * {gb} = {ga * gb} ({ga * gb % 100}%)"); calls.Add("printf(\"%d * %d = %d (%d%%)\\n\", a, b, a * b, (a * b) % 100);");
        }
        string expected = string.Join("\n", lines);
        string sol = "#include <stdio.h>\n\nint main(void) {\n" + decl + string.Join("\n", calls.Select(c => "    " + c)) + "\n    return 0;\n}\n";
        string starter = decl.Length == 0 ? Starter : "#include <stdio.h>\n\nint main(void) {\n" + decl + "    // escreva seu código aqui\n    return 0;\n}\n";
        string st = d == Difficulty.Genius
            ? L.F("Declare `int a = {0}` e `int b = {1}` e mostre as linhas abaixo exatamente como estão. Os números das duas últimas linhas devem ser **calculados** com `a` e `b` (a última mostra o resto da divisão do produto por 100 seguido de `%`).", ga, gb)
            : L.F("Escreva um programa em C que mostre exatamente as {0} linhas abaixo (ele não lê nada). Cuidado com as aspas, a barra invertida e o símbolo `%` dentro do `printf`.", lines.Count);
        return PK.Write(st, starter, sol, PK.Fixed(expected),
            L.T("Cada linha é um `printf` terminado em `\\n`. Para mostrar `%` escreva `%%`, para aspas `\\\"` e para a barra `\\\\`; os números calculados usam `%d`."));
    }

    static Exercise Predict01(Difficulty d, Random r)
    {
        var code = new StringBuilder("#include <stdio.h>\n\nint main(void) {\n"); var outp = new StringBuilder();
        int a = r.Next(2, 30), b = r.Next(2, 9); char ch = (char)('A' + r.Next(0, 20));
        int n = d switch { Difficulty.Easy => 3, Difficulty.Medium => 4, Difficulty.Hard => 5, _ => 6 };
        var opts = new List<(string src, string o)>
        {
            ($"printf(\"%d%%\\n\", {a * 2});", $"{a * 2}%\n"),
            ($"printf(\"[%c]\", '{ch}');", $"[{ch}]"), ($"printf(\"%d-%d\\n\", {a}, {b});", $"{a}-{b}\n"),
            ("printf(\"\\n\");", "\n"), ($"printf(\"%d %d\\n\", {a} + {b}, {a} * {b});", $"{a + b} {a * b}\n"),
            ($"printf(\"%c%c\\n\", '{ch}', '{ch}' + 1);", $"{ch}{(char)(ch + 1)}\n"), ("printf(\"%s!\\n\", \"fim\");", "fim!\n"),
            ($"printf(\"fim={a}\");", $"fim={a}"),
        };
        var picked = new List<(string, string)> { ("printf(\"Valor: \");", "Valor: "), ($"printf(\"%d\\n\", {a});", $"{a}\n") };
        foreach (var o in opts.OrderBy(_ => r.Next()).Take(n - 2)) picked.Add(o);
        foreach (var (s, o) in picked) { code.Append("    ").Append(s).Append('\n'); outp.Append(o); }
        code.Append("    return 0;\n}\n");
        return PK.Predict(L.F("Este programa mostra o número {0} em algum momento e termina com `return 0`.", a), code.ToString(), outp.ToString().TrimEnd('\n', ' '),
            L.T("O `printf` só muda de linha onde há `\\n`; `%d` e `%c` são trocados pelos valores e `%%` vira um único `%`. Junte tudo na ordem em que as chamadas acontecem."));
    }

    // ---------- c-02 ----------
    static Exercise Write02(Difficulty d, Random r)
    {
        int a = r.Next(10, 99), b = r.Next(2, 9); double price = Math.Round(r.Next(5, 200) + r.Next(0, 4) * 0.25, 2);
        char letra = (char)('A' + r.Next(0, 24));
        var all = new List<(string label, string fmt, string expr, string value)>
        {
            ("Soma", "%d", "a + b", (a + b).ToString()), ("Diferenca", "%d", "a - b", (a - b).ToString()),
            ("Produto", "%d", "a * b", (a * b).ToString()), ("Divisao inteira", "%d", "a / b", (a / b).ToString()),
            ("Resto", "%d", "a % b", (a % b).ToString()), ("Divisao real", "%.2f", "1.0 * a / b", PK.F2(1.0 * a / b)),
            ("Dobro do preco", "%.2f", "preco * 2", PK.F2(price * 2)),
        };
        int k = d switch { Difficulty.Easy => 3, Difficulty.Medium => 4, _ => 5 };
        var chosen = all.OrderBy(_ => r.Next()).Take(k).ToList();
        var lines = new List<string> { $"a = {a}, b = {b}" };
        var calls = new List<string> { "printf(\"a = %d, b = %d\\n\", a, b);" };
        foreach (var c in chosen) { lines.Add($"{c.label}: {c.value}"); calls.Add($"printf(\"{c.label}: {c.fmt}\\n\", {c.expr});"); }
        if (d >= Difficulty.Hard)
        {
            lines.Add($"Letra: {letra}"); calls.Add("printf(\"Letra: %c\\n\", letra);");
            lines.Add($"Proxima: {(char)(letra + 1)}"); calls.Add("printf(\"Proxima: %c\\n\", letra + 1);");
        }
        if (d == Difficulty.Genius)
        {
            lines.Add($"Codigo: {(int)letra}"); calls.Add("printf(\"Codigo: %d\\n\", letra);");
            lines.Add($"Minuscula: {char.ToLower(letra)}"); calls.Add("printf(\"Minuscula: %c\\n\", letra + 32);");
        }
        string decl = $"    int a = {a};\n    int b = {b};\n    double preco = {price.ToString("0.00", PK.Inv)};\n    char letra = '{letra}';\n";
        string head = "#include <stdio.h>\n\nint main(void) {\n" + decl;
        var op = new Dictionary<string, string> { ["Soma"] = "a + b", ["Diferenca"] = "a - b", ["Produto"] = "a * b", ["Divisao inteira"] = "a / b (inteira)", ["Resto"] = "a % b", ["Divisao real"] = "a / b com casas decimais", ["Dobro do preco"] = "preco * 2" };
        string st = L.F("Crie `int a = {0}`, `int b = {1}`, `double preco = {2}` e `char letra = '{3}'` e mostre exatamente as linhas abaixo, **calculando** cada valor a partir das variáveis (sem escrever o resultado à mão). Reais usam 2 casas decimais.", a, b, Ex.N(price), letra);
        st += "\n\n" + L.T("As contas pedidas, na ordem:") + " " + string.Join("; ", chosen.Select(c => $"{c.label} = {op[c.label]}"));
        if (d >= Difficulty.Hard) st += "; " + L.T("depois a letra e a letra seguinte") + (d == Difficulty.Genius ? "; " + L.T("depois o código numérico da letra e a letra minúscula (sem `tolower`)") : "");
        st += ".";
        string sol = head + string.Join("\n", calls.Select(c => "    " + c)) + "\n    return 0;\n}\n";
        string starter = head + "    // mostre as linhas aqui\n    return 0;\n}\n";
        return PK.Write(st, starter, sol, PK.Fixed(string.Join("\n", lines)),
            L.T("`int / int` descarta a parte decimal; para a divisão real force um operando a `double` (`1.0 * a / b`). Um `char` é um número: `letra + 1` é a próxima letra, `letra + 32` passa para minúscula."));
    }

    static Exercise Predict02(Difficulty d, Random r)
    {
        int a = r.Next(7, 60), b = r.Next(2, 8); double x = r.Next(10, 99) / 10.0 + 0.05 * r.Next(0, 2); char ch = (char)('a' + r.Next(0, 20));
        var code = new StringBuilder("#include <stdio.h>\n\nint main(void) {\n");
        var o = new List<string>();
        code.Append($"    int a = {a};\n    int b = {b};\n");
        code.Append("    int q = a / b;\n    int m = a % b;\n    printf(\"%d %d\\n\", q, m);\n"); o.Add($"{a / b} {a % b}");
        code.Append($"    double x = {x.ToString("0.0#", PK.Inv)};\n    int t = (int)x;\n    printf(\"%d\\n\", t);\n"); o.Add(((int)x).ToString());
        if (d >= Difficulty.Medium) { code.Append("    double e = a / b;\n    printf(\"%.1f\\n\", e);\n"); o.Add(((double)(a / b)).ToString("F1", PK.Inv)); }
        int q = a / b, m = a % b;
        if (d >= Difficulty.Hard)
        {
            code.Append($"    char c = '{ch}';\n    c = c + b;\n    printf(\"%c %d\\n\", c, c);\n"); o.Add($"{(char)(ch + b)} {(int)(ch + b)}");
            code.Append("    a = a + 1;\n    b += a;\n    printf(\"%d %d\\n\", a, b);\n"); o.Add($"{a + 1} {b + a + 1}");
        }
        if (d == Difficulty.Genius)
        {
            code.Append("    int z = a * b - q / 2 + m;\n    double w = z / 4;\n    printf(\"%d %.2f\\n\", z, w + x);\n");
            int aa = a + 1, bb = b + aa, z = aa * bb - q / 2 + m;
            o.Add($"{z} {PK.F2(z / 4 + x)}");
        }
        code.Append("    return 0;\n}\n");
        return PK.Predict(L.F("O programa usa `a = {0}` e `b = {1}`.", a, b), code.ToString(), string.Join("\n", o),
            L.T("Divisão entre inteiros é inteira (descarta a parte decimal) e `%` é o resto. Converter `double` para `int` corta os decimais. Guardar uma divisão inteira num `double` não recupera as casas perdidas. Um `char` guarda o código da letra, então somar move para outra letra."));
    }

    // ---------- c-03 ----------
    static Exercise Write03(Difficulty d, Random r)
    {
        switch (d)
        {
            case Difficulty.Easy:
            {
                int kind = r.Next(3);
                string st, body; Func<string, string> oracle; Func<Random, string> gen;
                if (kind == 0)
                {
                    st = L.T("Leia a base e a altura de um retângulo (inteiros, uma por linha) e mostre a área e o perímetro, no formato `Area: <valor>` e `Perimetro: <valor>`.");
                    body = "    int b, h;\n    scanf(\"%d\", &b);\n    scanf(\"%d\", &h);\n    printf(\"Area: %d\\n\", b * h);\n    printf(\"Perimetro: %d\\n\", 2 * (b + h));\n";
                    gen = g => $"{g.Next(1, 40)}\n{g.Next(1, 40)}\n"; oracle = i => { var v = PK.Ints(i); return $"Area: {v[0] * v[1]}\nPerimetro: {2 * (v[0] + v[1])}"; };
                }
                else if (kind == 1)
                {
                    st = L.T("Leia o preço de um produto e a quantidade comprada (inteiros, um por linha) e mostre `Total: <preco*quantidade>` e `Com desconto: <total menos 10%, com 2 casas decimais>`.");
                    body = "    int p, q;\n    scanf(\"%d\", &p);\n    scanf(\"%d\", &q);\n    printf(\"Total: %d\\n\", p * q);\n    printf(\"Com desconto: %.2f\\n\", p * q * 0.9);\n";
                    gen = g => $"{g.Next(1, 100)}\n{g.Next(1, 20)}\n"; oracle = i => { var v = PK.Ints(i); return $"Total: {v[0] * v[1]}\nCom desconto: {PK.F2(v[0] * v[1] * 0.9)}"; };
                }
                else
                {
                    st = L.T("Leia dois inteiros `a` e `b` (um por linha, `b` nunca é zero) e mostre `Quociente: <a/b inteiro>`, `Resto: <a%b>` e `Real: <a/b com 3 casas decimais>`.");
                    body = "    int a, b;\n    scanf(\"%d\", &a);\n    scanf(\"%d\", &b);\n    printf(\"Quociente: %d\\n\", a / b);\n    printf(\"Resto: %d\\n\", a % b);\n    printf(\"Real: %.3f\\n\", (double)a / b);\n";
                    gen = g => $"{g.Next(1, 200)}\n{g.Next(1, 15)}\n"; oracle = i => { var v = PK.Ints(i); return $"Quociente: {v[0] / v[1]}\nResto: {v[0] % v[1]}\nReal: {((double)v[0] / v[1]).ToString("F3", PK.Inv)}"; };
                }
                return PK.Write(st, Starter, "#include <stdio.h>\n\nint main(void) {\n" + body + "    return 0;\n}\n", PK.Tests(r, 4, gen, oracle),
                    L.T("Leia com `scanf(\"%d\", &variavel)`, calcule com os operadores e mostre com `printf`. Divisão de inteiros é inteira; para casas decimais use `(double)` ou `1.0 *`."));
            }
            case Difficulty.Medium:
            {
                if (r.Next(2) == 0)
                    return PK.Write(L.T("Leia uma duração em segundos (um inteiro) e mostre-a como `<h>h <m>min <s>s`, por exemplo `3725` vira `1h 2min 5s`."),
                        Starter, "#include <stdio.h>\n\nint main(void) {\n    int t;\n    scanf(\"%d\", &t);\n    printf(\"%dh %dmin %ds\\n\", t / 3600, t % 3600 / 60, t % 60);\n    return 0;\n}\n",
                        PK.Tests(r, 5, g => $"{g.Next(0, 90000)}\n", i => { int t = PK.Ints(i)[0]; return $"{t / 3600}h {t % 3600 / 60}min {t % 60}s"; }),
                        L.T("Horas = t / 3600 (inteiro). O que sobra é t % 3600, e dele saem os minutos (/ 60) e os segundos (% 60)."));
                return PK.Write(L.T("Leia três notas inteiras (uma por linha) e mostre `Media: <média com 2 casas>` e `Soma: <soma>`, e na linha seguinte `Falta: <quanto falta para a soma chegar a 21>` ou `Falta: 0` se já chegou (use só aritmética, sem `if`)."),
                    Starter, "#include <stdio.h>\n\nint main(void) {\n    int a, b, c, s;\n    scanf(\"%d\", &a);\n    scanf(\"%d\", &b);\n    scanf(\"%d\", &c);\n    s = a + b + c;\n    printf(\"Media: %.2f\\n\", s / 3.0);\n    printf(\"Soma: %d\\n\", s);\n    printf(\"Falta: %d\\n\", (21 - s) * (s < 21));\n    return 0;\n}\n",
                    PK.Tests(r, 5, g => $"{g.Next(0, 11)}\n{g.Next(0, 11)}\n{g.Next(0, 11)}\n", i => { var v = PK.Ints(i); int s = v.Sum(); return $"Media: {PK.F2(s / 3.0)}\nSoma: {s}\nFalta: {Math.Max(0, 21 - s)}"; }),
                    L.T("Uma comparação como `s < 21` vale 1 (verdade) ou 0 (falso) em C; multiplicar por ela zera a falta quando a soma já chegou."));
            }
            case Difficulty.Hard:
            {
                if (r.Next(2) == 0)
                    return PK.Write(L.T("Leia um número de 3 dígitos (de 100 a 999) e mostre `Centena: <c>`, `Dezena: <d>`, `Unidade: <u>`, `Soma: <soma dos dígitos>` e `Invertido: <o número escrito de trás para frente>` (use só `/` e `%`)."),
                        Starter, "#include <stdio.h>\n\nint main(void) {\n    int n, c, d, u;\n    scanf(\"%d\", &n);\n    c = n / 100;\n    d = n / 10 % 10;\n    u = n % 10;\n    printf(\"Centena: %d\\n\", c);\n    printf(\"Dezena: %d\\n\", d);\n    printf(\"Unidade: %d\\n\", u);\n    printf(\"Soma: %d\\n\", c + d + u);\n    printf(\"Invertido: %d\\n\", u * 100 + d * 10 + c);\n    return 0;\n}\n",
                        PK.Tests(r, 5, g => $"{g.Next(100, 1000)}\n", i => { int n = PK.Ints(i)[0]; int c = n / 100, dd = n / 10 % 10, u = n % 10; return $"Centena: {c}\nDezena: {dd}\nUnidade: {u}\nSoma: {c + dd + u}\nInvertido: {u * 100 + dd * 10 + c}"; }),
                        L.T("`n / 100` é a centena, `n / 10 % 10` a dezena e `n % 10` a unidade. O invertido remonta os dígitos com os pesos trocados."));
                return PK.Write(L.T("Leia o valor de uma compra em reais (inteiro) e mostre quantas notas de 100, 50, 20, 10, 5 e 2 e quantas moedas de 1 real são necessárias, usando o menor número de notas (comece pelas maiores). Uma linha por valor: `100: <qtd>`, `50: <qtd>`, ..., `1: <qtd>`."),
                    Starter, "#include <stdio.h>\n\nint main(void) {\n    int v, i;\n    int notas[7] = {100, 50, 20, 10, 5, 2, 1};\n    scanf(\"%d\", &v);\n    for (i = 0; i < 7; i++) {\n        printf(\"%d: %d\\n\", notas[i], v / notas[i]);\n        v = v % notas[i];\n    }\n    return 0;\n}\n",
                    PK.Tests(r, 5, g => $"{g.Next(1, 2000)}\n", i => { int v = PK.Ints(i)[0]; var sb = new List<string>(); foreach (var n in new[] { 100, 50, 20, 10, 5, 2, 1 }) { sb.Add($"{n}: {v / n}"); v %= n; } return string.Join("\n", sb); }),
                    L.T("Para cada nota, `v / nota` diz quantas cabem e `v % nota` é o que sobra para as notas menores."));
            }
            default:
                return PK.Write(L.T("Leia as horas e os minutos de um horário e a duração de uma viagem em minutos (três inteiros, um por linha). Mostre o horário de chegada como `HH:MM` com dois dígitos cada e, na linha seguinte, `+<dias>d` com quantos dias a mais se passaram (0 se chegou no mesmo dia). Dica: `%02d`."),
                    Starter, "#include <stdio.h>\n\nint main(void) {\n    int h, m, dur, total;\n    scanf(\"%d\", &h);\n    scanf(\"%d\", &m);\n    scanf(\"%d\", &dur);\n    total = h * 60 + m + dur;\n    printf(\"%02d:%02d\\n\", total / 60 % 24, total % 60);\n    printf(\"+%dd\\n\", total / 1440);\n    return 0;\n}\n",
                    PK.Tests(r, 5, g => $"{g.Next(0, 24)}\n{g.Next(0, 60)}\n{g.Next(1, 4000)}\n", i => { var v = PK.Ints(i); int t = v[0] * 60 + v[1] + v[2]; return $"{t / 60 % 24:00}:{t % 60:00}\n+{t / 1440}d"; }),
                    L.T("Converta tudo para minutos desde a meia-noite. As horas na tela são `total / 60 % 24`, os minutos `total % 60` e os dias a mais `total / 1440`. `%02d` completa com zero à esquerda."));
        }
    }

    static Exercise Predict03(Difficulty d, Random r)
    {
        int a = r.Next(10, 99), b = r.Next(2, 9), c = r.Next(2, 6);
        var code = new StringBuilder($"#include <stdio.h>\n\nint main(void) {{\n    int a = {a};\n    int b = {b};\n    int c = {c};\n");
        var o = new List<string>();
        code.Append("    printf(\"%d\\n\", a + b * c);\n"); o.Add((a + b * c).ToString());
        code.Append("    printf(\"%d\\n\", (a + b) * c);\n"); o.Add(((a + b) * c).ToString());
        code.Append("    printf(\"%d %d\\n\", a / b / c, a % b % c);\n"); o.Add($"{a / b / c} {a % b % c}");
        if (d >= Difficulty.Medium) { code.Append("    printf(\"%d\\n\", -a / b);\n    printf(\"%d\\n\", -a % b);\n"); o.Add((-a / b).ToString()); o.Add((-a % b).ToString()); }
        if (d >= Difficulty.Hard)
        {
            code.Append("    double r1 = (a + b) / c;\n    double r2 = (a + b) / (double)c;\n    printf(\"%.2f %.2f\\n\", r1, r2);\n");
            o.Add($"{PK.F2((a + b) / c)} {PK.F2((a + b) / (double)c)}");
        }
        if (d == Difficulty.Genius)
        {
            code.Append("    a = a - a % 10;\n    b = b * 2 - c;\n    printf(\"%d %d\\n\", a, b);\n    printf(\"%d\\n\", a / 10 + b % 3 * c);\n");
            int a2 = a - a % 10, b2 = b * 2 - c; o.Add($"{a2} {b2}"); o.Add((a2 / 10 + b2 % 3 * c).ToString());
        }
        code.Append("    return 0;\n}\n");
        return PK.Predict(L.F("Os valores iniciais são a = {0}, b = {1} e c = {2}.", a, b, c), code.ToString(), string.Join("\n", o),
            L.T("`*`, `/` e `%` vêm antes de `+` e `-`, e os parênteses mandam primeiro. Em C a divisão e o resto de números negativos truncam em direção a zero (o resto tem o sinal do dividendo). `(a + b) / c` com inteiros já é uma divisão inteira, mesmo guardada num `double` depois."));
    }
}
