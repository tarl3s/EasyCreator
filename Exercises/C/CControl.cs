using System.Text;

namespace EasyCreator.Exercises;

/// <summary>C lessons 4-6: decisions with if/else, loops, functions and pointers.</summary>
[Exercises("c-04", "c-05", "c-06")]
public sealed class CControl : IExerciseGenerator
{
    const string Head = "#include <stdio.h>\n\n";
    const string Starter = "#include <stdio.h>\n\nint main(void) {\n    // escreva seu código aqui\n    return 0;\n}\n";

    public Exercise Make(Lesson lesson, Difficulty d, Random r) => lesson.Id switch
    {
        "c-04" => r.Next(2) == 0 ? Write04(d, r) : Predict04(d, r),
        "c-05" => r.Next(2) == 0 ? Write05(d, r) : Predict05(d, r),
        _ => r.Next(2) == 0 ? Write06(d, r) : Predict06(d, r),
    };

    static string Main(string body) => Head + "int main(void) {\n" + body + "    return 0;\n}\n";

    // ---------- c-04 ----------
    static Exercise Write04(Difficulty d, Random r)
    {
        string st, sol; Func<Random, string> gen; Func<string, string> oracle; string why;
        switch (d)
        {
            case Difficulty.Easy when r.Next(2) == 0:
            {
                int k = r.Next(3, 10);
                st = L.F("Leia um inteiro `n`. Mostre `<n> e multiplo de {0}` se ele for múltiplo de {0}, senão `<n> nao e multiplo de {0}`.", k);
                sol = Main($"    int n;\n    scanf(\"%d\", &n);\n    if (n % {k} == 0) {{\n        printf(\"%d e multiplo de {k}\\n\", n);\n    }} else {{\n        printf(\"%d nao e multiplo de {k}\\n\", n);\n    }}\n");
                gen = g => $"{g.Next(1, 200)}\n"; oracle = i => { int n = PK.Ints(i)[0]; return n % k == 0 ? $"{n} e multiplo de {k}" : $"{n} nao e multiplo de {k}"; };
                why = L.T("Um número é múltiplo de k quando o resto da divisão por k é zero: `n % k == 0`.");
                break;
            }
            case Difficulty.Easy:
                st = L.T("Leia um inteiro. Mostre `positivo` se for maior que zero, `negativo` se for menor que zero e `zero` se for zero.");
                sol = Main("    int n;\n    scanf(\"%d\", &n);\n    if (n > 0) {\n        printf(\"positivo\\n\");\n    } else if (n < 0) {\n        printf(\"negativo\\n\");\n    } else {\n        printf(\"zero\\n\");\n    }\n");
                gen = g => $"{g.Next(-9, 10)}\n"; oracle = i => { int n = PK.Ints(i)[0]; return n > 0 ? "positivo" : n < 0 ? "negativo" : "zero"; };
                why = L.T("Três casos exclusivos: uma cadeia `if / else if / else` escolhe só um.");
                break;
            case Difficulty.Medium:
                st = L.T("Leia os três lados de um triângulo (inteiros, um por linha). Se algum lado for maior ou igual à soma dos outros dois, mostre `nao forma triangulo`. Senão mostre `equilatero` (3 lados iguais), `isosceles` (exatamente 2 iguais) ou `escaleno` (todos diferentes).");
                sol = Main("    int a, b, c;\n    scanf(\"%d\", &a);\n    scanf(\"%d\", &b);\n    scanf(\"%d\", &c);\n    if (a >= b + c || b >= a + c || c >= a + b) {\n        printf(\"nao forma triangulo\\n\");\n    } else if (a == b && b == c) {\n        printf(\"equilatero\\n\");\n    } else if (a == b || b == c || a == c) {\n        printf(\"isosceles\\n\");\n    } else {\n        printf(\"escaleno\\n\");\n    }\n");
                gen = g => { int a = g.Next(1, 8); int b = g.Next(1, 8); int c = g.Next(1, 8); return g.Next(4) == 0 ? $"{a}\n{a}\n{a}\n" : $"{a}\n{b}\n{c}\n"; };
                oracle = i => { var v = PK.Ints(i); int a = v[0], b = v[1], c = v[2]; return a >= b + c || b >= a + c || c >= a + b ? "nao forma triangulo" : a == b && b == c ? "equilatero" : a == b || b == c || a == c ? "isosceles" : "escaleno"; };
                why = L.T("Primeiro a existência do triângulo (cada lado menor que a soma dos outros); depois conte quantos lados são iguais, do caso mais restrito para o mais geral.");
                break;
            case Difficulty.Hard:
            {
                int t1 = r.Next(2, 6) * 50, t2 = t1 + r.Next(2, 6) * 50, p1 = r.Next(1, 4) * 5, p2 = p1 + r.Next(1, 4) * 5;
                st = L.F("Leia o valor de uma compra (inteiro). Compras abaixo de {0} não têm desconto; de {0} até {1} (inclusive) têm {2}% de desconto; acima de {1} têm {3}%. Mostre `Desconto: <p>%` e, na linha seguinte, `Total: <valor final com 2 casas decimais>`.", t1, t2, p1, p2);
                sol = Main($"    int v, p;\n    scanf(\"%d\", &v);\n    if (v < {t1}) {{\n        p = 0;\n    }} else if (v <= {t2}) {{\n        p = {p1};\n    }} else {{\n        p = {p2};\n    }}\n    printf(\"Desconto: %d%%\\n\", p);\n    printf(\"Total: %.2f\\n\", v * (100 - p) / 100.0);\n");
                gen = g => $"{g.Next(10, t2 + 150)}\n";
                oracle = i => { int v = PK.Ints(i)[0]; int p = v < t1 ? 0 : v <= t2 ? p1 : p2; return $"Desconto: {p}%\nTotal: {PK.F2(v * (100 - p) / 100.0)}"; };
                why = L.T("Teste as faixas em ordem crescente; `v * (100 - p) / 100.0` calcula o preço final sem perder os centavos (o `100.0` torna a divisão real).");
                break;
            }
            default:
                st = L.T("Leia dia, mês e ano (três inteiros, um por linha). Mostre `valida` se a data existe no calendário (fevereiro tem 29 dias em ano bissexto) ou `invalida` caso contrário. Anos bissextos: divisíveis por 4 e não por 100, ou por 400.");
                sol = Main("    int dia, mes, ano, max;\n    scanf(\"%d\", &dia);\n    scanf(\"%d\", &mes);\n    scanf(\"%d\", &ano);\n    if (mes < 1 || mes > 12 || dia < 1) {\n        printf(\"invalida\\n\");\n        return 0;\n    }\n    if (mes == 2) {\n        max = ((ano % 4 == 0 && ano % 100 != 0) || ano % 400 == 0) ? 29 : 28;\n    } else if (mes == 4 || mes == 6 || mes == 9 || mes == 11) {\n        max = 30;\n    } else {\n        max = 31;\n    }\n    printf(\"%s\\n\", dia <= max ? \"valida\" : \"invalida\");\n");
                gen = g => $"{g.Next(1, 33)}\n{g.Next(1, 14)}\n{PK.Pick(g, 1900, 2000, 2023, 2024, 2100, 2400)}\n";
                oracle = i => { var v = PK.Ints(i); int dia = v[0], mes = v[1], ano = v[2]; if (mes < 1 || mes > 12 || dia < 1) return "invalida"; int max = mes == 2 ? ((ano % 4 == 0 && ano % 100 != 0) || ano % 400 == 0 ? 29 : 28) : mes is 4 or 6 or 9 or 11 ? 30 : 31; return dia <= max ? "valida" : "invalida"; };
                why = L.T("Descubra o último dia do mês (28/29/30/31 conforme o mês e o ano bissexto) e compare. Cuidado com mês fora de 1..12 antes de usar a tabela.");
                break;
        }
        return PK.Write(st, Starter, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict04(Difficulty d, Random r)
    {
        int x = r.Next(1, 40), y = r.Next(1, 40), t1 = r.Next(5, 15), t2 = t1 + r.Next(5, 15);
        var c = new StringBuilder($"int x = {x};\n    int y = {y};\n"); var o = new List<string>();
        string Chain(string var, int v)
        {
            c.Append($"    if ({var} < {t1}) {{\n        printf(\"baixo\\n\");\n    }} else if ({var} < {t2}) {{\n        printf(\"medio\\n\");\n    }} else {{\n        printf(\"alto\\n\");\n    }}\n");
            return v < t1 ? "baixo" : v < t2 ? "medio" : "alto";
        }
        o.Add(Chain("x", x));
        if (d >= Difficulty.Medium)
        {
            c.Append("    if (x > y) {\n        printf(\"x\\n\");\n    }\n    if (x % 2 == 0) {\n        printf(\"par\\n\");\n    } else {\n        printf(\"impar\\n\");\n    }\n");
            if (x > y) o.Add("x"); o.Add(x % 2 == 0 ? "par" : "impar");
        }
        if (d >= Difficulty.Hard)
        {
            c.Append($"    if (x > {t1} && y > {t1} || x + y == {t2}) {{\n        printf(\"A\\n\");\n    }} else if (!(x < y)) {{\n        printf(\"B\\n\");\n    }} else {{\n        printf(\"C\\n\");\n    }}\n");
            o.Add(x > t1 && y > t1 || x + y == t2 ? "A" : !(x < y) ? "B" : "C");
        }
        if (d == Difficulty.Genius)
        {
            c.Append($"    int n = 0;\n    if (x > {t1} || ++n > 0) {{\n        n = n + 10;\n    }}\n    if (y < {t1} && ++n > 0) {{\n        n = n + 100;\n    }}\n    if (x == y) printf(\"=\");\n    else printf(\"!\");\n    printf(\"%d\\n\", n);\n");
            int n = 0; if (x > t1 || ++n > 0) n += 10; if (y < t1 && ++n > 0) n += 100; o.Add((x == y ? "=" : "!") + n);
        }
        return PK.Predict(L.F("O programa começa com x = {0} e y = {1}.", x, y), Main("    " + c), string.Join("\n", o),
            L.T("Siga cada `if` com os valores atuais; numa cadeia `else if` só o primeiro teste verdadeiro executa. Em `||` e `&&` o segundo lado só é calculado se o primeiro não decidir o resultado, então um `++n` ali pode nem acontecer."));
    }

    // ---------- c-05 ----------
    static Exercise Write05(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy when r.Next(2) == 0:
            {
                int k = r.Next(2, 8);
                st = L.F("Leia um inteiro `n` e mostre, em uma linha e separados por espaço, os múltiplos de {0} de {0} até `n` (inclusive). Se não houver nenhum (n menor que {0}), mostre `nenhum`.", k);
                sol = Main($"    int n, i, primeiro = 1;\n    scanf(\"%d\", &n);\n    for (i = {k}; i <= n; i += {k}) {{\n        if (!primeiro) {{\n            printf(\" \");\n        }}\n        printf(\"%d\", i);\n        primeiro = 0;\n    }}\n    if (primeiro) {{\n        printf(\"nenhum\");\n    }}\n    printf(\"\\n\");\n");
                gen = g => $"{g.Next(1, 45)}\n"; oracle = i => { int n = PK.Ints(i)[0]; var l = new List<int>(); for (int j = k; j <= n; j += k) l.Add(j); return l.Count == 0 ? "nenhum" : PK.Join(l); };
                why = L.T("O `for` começa no primeiro múltiplo e anda de k em k. Um sinalizador evita o espaço extra antes do primeiro número.");
                break;
            }
            case Difficulty.Easy:
                st = L.T("Leia um inteiro `n` (n >= 1) e mostre `Soma: <1+2+...+n>` e `Quadrados: <soma de 1²+2²+...+n²>`.");
                sol = Main("    int n, i;\n    long s = 0, q = 0;\n    scanf(\"%d\", &n);\n    for (i = 1; i <= n; i++) {\n        s += i;\n        q += (long)i * i;\n    }\n    printf(\"Soma: %ld\\n\", s);\n    printf(\"Quadrados: %ld\\n\", q);\n");
                gen = g => $"{g.Next(1, 60)}\n"; oracle = i => { int n = PK.Ints(i)[0]; long s = 0, q = 0; for (int j = 1; j <= n; j++) { s += j; q += j * j; } return $"Soma: {s}\nQuadrados: {q}"; };
                why = L.T("Um acumulador começa em zero e recebe cada termo dentro do laço.");
                break;
            case Difficulty.Medium:
                st = L.T("Leia `n` e depois `n` inteiros (um por linha). Mostre `Positivos: <quantos são maiores que zero>`, `Soma dos pares: <soma dos valores pares>` e `Maior sequencia crescente: <tamanho da maior sequência de elementos consecutivos estritamente crescentes>`.");
                sol = Main("    int n, i, x, ant = 0, pos = 0, soma = 0, seq = 0, melhor = 0;\n    scanf(\"%d\", &n);\n    for (i = 0; i < n; i++) {\n        scanf(\"%d\", &x);\n        if (x > 0) {\n            pos++;\n        }\n        if (x % 2 == 0) {\n            soma += x;\n        }\n        if (i > 0 && x > ant) {\n            seq++;\n        } else {\n            seq = 1;\n        }\n        if (seq > melhor) {\n            melhor = seq;\n        }\n        ant = x;\n    }\n    printf(\"Positivos: %d\\n\", pos);\n    printf(\"Soma dos pares: %d\\n\", soma);\n    printf(\"Maior sequencia crescente: %d\\n\", melhor);\n");
                gen = g => { int n = g.Next(1, 9); return n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => g.Next(-6, 15))) + "\n"; };
                oracle = i => { var v = PK.Ints(i); int n = v[0]; int pos = 0, soma = 0, seq = 0, best = 0, ant = 0; for (int j = 0; j < n; j++) { int x = v[j + 1]; if (x > 0) pos++; if (x % 2 == 0) soma += x; seq = j > 0 && x > ant ? seq + 1 : 1; best = Math.Max(best, seq); ant = x; } return $"Positivos: {pos}\nSoma dos pares: {soma}\nMaior sequencia crescente: {best}"; };
                why = L.T("Três contadores no mesmo laço. A sequência cresce enquanto `x > ant` e recomeça em 1 quando quebra; guarde a maior.");
                break;
            case Difficulty.Hard:
                st = L.T("Leia um inteiro `n` (n >= 0). Mostre `Digitos: <quantidade de dígitos>`, `Soma: <soma dos dígitos>` e `Invertido: <n escrito de trás para frente, sem zeros à esquerda>` (para n = 0, mostre 1 dígito, soma 0 e invertido 0). Use `while` com `/` e `%`.");
                sol = Main("    int n, soma = 0, dig = 0, inv = 0;\n    scanf(\"%d\", &n);\n    if (n == 0) {\n        dig = 1;\n    }\n    while (n > 0) {\n        soma += n % 10;\n        inv = inv * 10 + n % 10;\n        n /= 10;\n        dig++;\n    }\n    printf(\"Digitos: %d\\n\", dig);\n    printf(\"Soma: %d\\n\", soma);\n    printf(\"Invertido: %d\\n\", inv);\n");
                gen = g => $"{PK.Pick(g, 0, g.Next(1, 10), g.Next(10, 1000), g.Next(1000, 100000), g.Next(1, 10) * 1000, g.Next(10000, 900000) / 10 * 10)}\n";
                oracle = i => { int n = PK.Ints(i)[0]; int soma = 0, dig = n == 0 ? 1 : 0, inv = 0; int m = n; while (m > 0) { soma += m % 10; inv = inv * 10 + m % 10; m /= 10; dig++; } return $"Digitos: {dig}\nSoma: {soma}\nInvertido: {inv}"; };
                why = L.T("A cada volta `n % 10` é o último dígito e `n /= 10` o descarta. O invertido é montado multiplicando por 10 e somando o dígito. O caso n = 0 nunca entra no laço.");
                break;
            default:
                st = L.T("Leia um inteiro `n` (1 <= n <= 5000). Siga a sequência de Collatz: se o número for par, divida por 2; se for ímpar, multiplique por 3 e some 1; pare ao chegar em 1. Mostre `Passos: <quantas operações>` e `Maximo: <maior valor atingido, incluindo n>`.");
                sol = Main("    long n;\n    long maximo;\n    int passos = 0;\n    scanf(\"%ld\", &n);\n    maximo = n;\n    while (n != 1) {\n        if (n % 2 == 0) {\n            n = n / 2;\n        } else {\n            n = 3 * n + 1;\n        }\n        passos++;\n        if (n > maximo) {\n            maximo = n;\n        }\n    }\n    printf(\"Passos: %d\\n\", passos);\n    printf(\"Maximo: %ld\\n\", maximo);\n");
                gen = g => $"{g.Next(1, 5001)}\n";
                oracle = i => { long n = PK.Ints(i)[0], max = n; int p = 0; while (n != 1) { n = n % 2 == 0 ? n / 2 : 3 * n + 1; p++; max = Math.Max(max, n); } return $"Passos: {p}\nMaximo: {max}"; };
                why = L.T("Um `while (n != 1)` aplica a regra e conta os passos; o máximo é atualizado a cada novo valor. Use `long`: os valores podem passar de 32 mil.");
                break;
        }
        return PK.Write(st, Starter, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict05(Difficulty d, Random r)
    {
        var c = new StringBuilder(); var o = new List<string>();
        int a = r.Next(2, 5), n = r.Next(5, 12);
        switch (d)
        {
            case Difficulty.Easy:
            {
                int s0 = r.Next(1, 6), st = r.Next(2, 5);
                c.Append($"    int i;\n    for (i = {s0}; i <= {n + s0}; i += {st}) {{\n        printf(\"%d \", i);\n    }}\n    printf(\"\\n%d\\n\", i);\n");
                var l = new List<int>(); int i2; for (i2 = s0; i2 <= n + s0; i2 += st) l.Add(i2);
                o.Add(PK.Join(l) + " "); o.Add(i2.ToString());
                break;
            }
            case Difficulty.Medium:
            {
                c.Append($"    int i, s = 0, k = {a};\n    for (i = 1; i <= {n}; i++) {{\n        if (i % k == 0) {{\n            continue;\n        }}\n        s += i;\n    }}\n    printf(\"%d\\n\", s);\n    while (k < {n * 3}) {{\n        k *= 2;\n    }}\n    printf(\"%d\\n\", k);\n");
                int s = 0, k = a; for (int i = 1; i <= n; i++) { if (i % k == 0) continue; s += i; }
                o.Add(s.ToString()); while (k < n * 3) k *= 2; o.Add(k.ToString());
                break;
            }
            case Difficulty.Hard:
            {
                int m = r.Next(3, 6);
                c.Append($"    int i, j;\n    for (i = 1; i <= {m}; i++) {{\n        for (j = 1; j <= i; j++) {{\n            if (j == {a}) {{\n                break;\n            }}\n            printf(\"%d\", i * j);\n            printf(\"%s\", j < i ? \",\" : \"\");\n        }}\n        printf(\"\\n\");\n    }}\n");
                for (int i = 1; i <= m; i++) { var sb = new StringBuilder(); for (int j = 1; j <= i; j++) { if (j == a) break; sb.Append(i * j); sb.Append(j < i ? "," : ""); } o.Add(sb.ToString()); }
                break;
            }
            default:
            {
                int lim = r.Next(20, 60);
                c.Append($"    int i = 1, total = 0, cont = 0;\n    while (1) {{\n        i = i * {a} + 1;\n        if (i % 2 == 0) {{\n            continue;\n        }}\n        total += i % 10;\n        cont++;\n        if (total > {lim} || cont == 6) {{\n            break;\n        }}\n    }}\n    printf(\"%d %d %d\\n\", i, total, cont);\n");
                int i2 = 1, tot = 0, cont2 = 0;
                while (true) { i2 = i2 * a + 1; if (i2 % 2 == 0) continue; tot += i2 % 10; cont2++; if (tot > lim || cont2 == 6) break; }
                o.Add($"{i2} {tot} {cont2}");
                break;
            }
        }
        return PK.Predict(L.F("O programa usa o número {0} como valor-chave.", a * 100 + n),
            Main(c.ToString()), string.Join("\n", o),
            L.T("Execute o laço volta a volta anotando as variáveis. `continue` pula para a próxima volta, `break` sai do laço mais interno. Depois do `for`, a variável do contador guarda o primeiro valor que falhou no teste."));
    }

    // ---------- c-06 ----------
    static Exercise Write06(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        switch (d)
        {
            case Difficulty.Easy:
            {
                st = L.T("Escreva a função `int potencia(int base, int exp)` (laço, sem `pow`) que devolve base elevada a exp. No `main`, leia `base` e `exp` (um por linha) e mostre `<base>^<exp> = <resultado>`. Os testes usam resultados pequenos (cabem em `int`).");
                sol = Head + "int potencia(int base, int exp) {\n    int r = 1, i;\n    for (i = 0; i < exp; i++) {\n        r *= base;\n    }\n    return r;\n}\n\nint main(void) {\n    int b, e;\n    scanf(\"%d\", &b);\n    scanf(\"%d\", &e);\n    printf(\"%d^%d = %d\\n\", b, e, potencia(b, e));\n    return 0;\n}\n";
                gen = g => $"{g.Next(2, 10)}\n{g.Next(0, 6)}\n"; oracle = i => { var v = PK.Ints(i); int p = 1; for (int j = 0; j < v[1]; j++) p *= v[0]; return $"{v[0]}^{v[1]} = {p}"; };
                why = L.T("A função começa com 1 e multiplica pela base `exp` vezes; expoente 0 devolve 1 sem entrar no laço.");
                break;
            }
            case Difficulty.Medium:
                st = L.T("Escreva `int mdc(int a, int b)` (algoritmo de Euclides: enquanto `b != 0`, troque `(a, b)` por `(b, a % b)`) e `int mmc(int a, int b)`. No `main`, leia `a` e `b` (um por linha, positivos) e mostre `MDC: <mdc>` e `MMC: <mmc>`.");
                sol = Head + "int mdc(int a, int b) {\n    int t;\n    while (b != 0) {\n        t = a % b;\n        a = b;\n        b = t;\n    }\n    return a;\n}\n\nint mmc(int a, int b) {\n    return a / mdc(a, b) * b;\n}\n\nint main(void) {\n    int a, b;\n    scanf(\"%d\", &a);\n    scanf(\"%d\", &b);\n    printf(\"MDC: %d\\n\", mdc(a, b));\n    printf(\"MMC: %d\\n\", mmc(a, b));\n    return 0;\n}\n";
                gen = g => $"{g.Next(1, 120)}\n{g.Next(1, 120)}\n";
                oracle = i => { var v = PK.Ints(i); int a = v[0], b = v[1]; while (b != 0) (a, b) = (b, a % b); return $"MDC: {a}\nMMC: {v[0] / a * v[1]}"; };
                why = L.T("Euclides: o MDC não muda ao trocar `(a, b)` por `(b, a % b)`. O MMC é `a * b / MDC`; dividir antes de multiplicar evita estouro.");
                break;
            case Difficulty.Hard:
                st = L.T("Escreva `void dividir(int a, int b, int *q, int *r)`, que grava em `*q` o quociente e em `*r` o resto de `a / b`, e `int ehPrimo(int n)`. No `main`, leia `a` e `b` (`b` nunca é zero) e mostre `<a> = <b> * <q> + <r>` e, na linha seguinte, `Quociente primo: sim` ou `Quociente primo: nao`.");
                sol = Head + "int ehPrimo(int n) {\n    int d;\n    if (n < 2) {\n        return 0;\n    }\n    for (d = 2; d * d <= n; d++) {\n        if (n % d == 0) {\n            return 0;\n        }\n    }\n    return 1;\n}\n\nvoid dividir(int a, int b, int *q, int *r) {\n    *q = a / b;\n    *r = a % b;\n}\n\nint main(void) {\n    int a, b, q, r;\n    scanf(\"%d\", &a);\n    scanf(\"%d\", &b);\n    dividir(a, b, &q, &r);\n    printf(\"%d = %d * %d + %d\\n\", a, b, q, r);\n    printf(\"Quociente primo: %s\\n\", ehPrimo(q) ? \"sim\" : \"nao\");\n    return 0;\n}\n";
                gen = g => $"{g.Next(1, 400)}\n{g.Next(1, 20)}\n";
                oracle = i => { var v = PK.Ints(i); int q = v[0] / v[1], rr = v[0] % v[1]; bool p = q >= 2; for (int x = 2; x * x <= q; x++) if (q % x == 0) p = false; return $"{v[0]} = {v[1]} * {q} + {rr}\nQuociente primo: {(p ? "sim" : "nao")}"; };
                why = L.T("Para devolver dois valores, a função recebe os endereços (`&q`, `&r`) e grava neles com `*q = ...`. Primo: nenhum divisor de 2 até a raiz.");
                break;
            default:
                st = L.T("Escreva `void ordenar3(int *a, int *b, int *c)`, que deixa os três valores em ordem crescente (`*a <= *b <= *c`) trocando-os de lugar, e `int contar(int n, int d)`, que conta quantas vezes o dígito `d` aparece em `n` (n >= 0; para n = 0 e d = 0 conta 1). No `main`, leia `x`, `y`, `z` e `d` (um por linha) e mostre `Ordem: <x> <y> <z>` (já ordenados) e `Digito <d>: <total de aparições nos três números>`.");
                sol = Head + "void trocar(int *a, int *b) {\n    int t = *a;\n    *a = *b;\n    *b = t;\n}\n\nvoid ordenar3(int *a, int *b, int *c) {\n    if (*a > *b) {\n        trocar(a, b);\n    }\n    if (*b > *c) {\n        trocar(b, c);\n    }\n    if (*a > *b) {\n        trocar(a, b);\n    }\n}\n\nint contar(int n, int d) {\n    int c = 0;\n    if (n == 0 && d == 0) {\n        return 1;\n    }\n    while (n > 0) {\n        if (n % 10 == d) {\n            c++;\n        }\n        n /= 10;\n    }\n    return c;\n}\n\nint main(void) {\n    int x, y, z, d;\n    scanf(\"%d\", &x);\n    scanf(\"%d\", &y);\n    scanf(\"%d\", &z);\n    scanf(\"%d\", &d);\n    ordenar3(&x, &y, &z);\n    printf(\"Ordem: %d %d %d\\n\", x, y, z);\n    printf(\"Digito %d: %d\\n\", d, contar(x, d) + contar(y, d) + contar(z, d));\n    return 0;\n}\n";
                gen = g => $"{g.Next(0, 3000)}\n{g.Next(0, 3000)}\n{g.Next(0, 3000)}\n{g.Next(0, 10)}\n";
                oracle = i => { var v = PK.Ints(i); var s = v.Take(3).OrderBy(x => x).ToArray(); int d2 = v[3]; int Cnt(int n) { if (n == 0) return d2 == 0 ? 1 : 0; int c = 0; while (n > 0) { if (n % 10 == d2) c++; n /= 10; } return c; } return $"Ordem: {s[0]} {s[1]} {s[2]}\nDigito {d2}: {s.Sum(Cnt)}"; };
                why = L.T("Com três valores, três comparações-trocas (a,b), (b,c), (a,b) bastam. Como a função recebe ponteiros, `trocar(a, b)` reaproveita os mesmos ponteiros sem `&`.");
                break;
        }
        string starter = Head + "// escreva as funções aqui\n\nint main(void) {\n    // leia os dados e use as funções\n    return 0;\n}\n";
        return PK.Write(st, starter, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict06(Difficulty d, Random r)
    {
        int a = r.Next(2, 9), b = r.Next(2, 9), k = r.Next(2, 5);
        var o = new List<string>();
        string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = Head + $"int dobro(int x) {{\n    x = x * 2;\n    return x;\n}}\n\nint main(void) {{\n    int a = {a};\n    int b = dobro(a);\n    printf(\"%d %d\\n\", a, b);\n    return 0;\n}}\n";
                o.Add($"{a} {a * 2}");
                break;
            case Difficulty.Medium:
                code = Head + $"void soma(int x, int *y) {{\n    x = x + {k};\n    *y = *y + x;\n}}\n\nint main(void) {{\n    int a = {a};\n    int b = {b};\n    soma(a, &b);\n    printf(\"%d %d\\n\", a, b);\n    soma(b, &a);\n    printf(\"%d %d\\n\", a, b);\n    return 0;\n}}\n";
                { int A = a, B = b; B = B + (A + k); o.Add($"{A} {B}"); A = A + (B + k); o.Add($"{A} {B}"); }
                break;
            case Difficulty.Hard:
                code = Head + $"int f(int n) {{\n    if (n % 2 == 0) {{\n        return n / 2;\n    }}\n    return {k} * n + 1;\n}}\n\nvoid troca(int *p, int *q) {{\n    int t = *p;\n    *p = *q;\n    *q = t;\n}}\n\nint main(void) {{\n    int a = {a};\n    int b = {b};\n    int i;\n    for (i = 0; i < 3; i++) {{\n        a = f(a);\n        troca(&a, &b);\n        printf(\"%d %d\\n\", a, b);\n    }}\n    return 0;\n}}\n";
                { int A = a, B = b; for (int i = 0; i < 3; i++) { A = A % 2 == 0 ? A / 2 : k * A + 1; (A, B) = (B, A); o.Add($"{A} {B}"); } }
                break;
            default:
                code = Head + $"int g = {a};\n\nint h(int x) {{\n    g = g + x;\n    return g * 2;\n}}\n\nvoid m(int *p) {{\n    *p = h(*p) - 1;\n}}\n\nint main(void) {{\n    int v = {b};\n    int w;\n    m(&v);\n    w = h(v) + h(1);\n    printf(\"%d %d %d\\n\", v, w, g);\n    return 0;\n}}\n";
                { int G = a; int H(int x) { G += x; return G * 2; } int v = b; v = H(v) - 1; int h1 = H(v); int h2 = H(1); o.Add($"{v} {h1 + h2} {G}"); }
                break;
        }
        return PK.Predict(L.F("As variáveis começam com os valores {0} e {1}.", a, b), code, string.Join("\n", o),
            L.T("Em C os argumentos são copiados: mudar `x` dentro da função não muda quem chamou. Só quem recebe um endereço (`int *p`) consegue alterar a variável original com `*p = ...`. Uma variável global é compartilhada por todas as funções."));
    }
}
