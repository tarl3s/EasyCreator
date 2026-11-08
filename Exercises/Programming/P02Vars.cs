using System.Globalization;

namespace EasyCreator.Exercises;

/// <summary>Lesson 2 (variables and types): reading typed values, text operations, predicting reassignments and types.</summary>
[Exercises("py-02", "js-02")]
public sealed class P02Vars : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        bool js = Pg.Js(lesson);
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(js, lv, r) : MakeOut(js, lv, r);
    }

    record WordOp(string Label, string Py, string Js, Func<string, string> F);

    static Exercise MakeCode(bool js, int lv, Random r)
    {
        switch (lv)
        {
            case 0:
            {
                int k = Ex.Int(r, 3, 25);
                string label = Ex.Pick(r, "Nome", "Aluno", "Pessoa");
                var tests = Pg.Tests(new[] { "Ana\n15", "Bruno\n40", Ex.Pick(r, Pg.Names) + "\n" + Ex.Int(r, 1, 90), Ex.Pick(r, Pg.Names) + "\n" + Ex.Int(r, 1, 90) },
                    i => { var p = i.TrimEnd('\n').Split('\n'); return $"{label}: {p[0]}\nDaqui a {k} anos: {int.Parse(p[1]) + k}"; });
                var sol = Pg.Code(Pg.ReadStr(js, "nome"), Pg.ReadInt(js, "idade"), Pg.Pr(js, label + ": {nome}"), Pg.Pr(js, $"Daqui a {k} anos: " + "{idade + " + k + "}"));
                return Ex.Code(Pg.Prompt(L.T("Leia um nome (texto) e uma idade (inteiro), cada um em uma linha. Mostre duas linhas:") + "\n" + Pg.Plain($"{label}: <nome>\nDaqui a {k} anos: <idade + {k}>"), tests[0]),
                    Pg.Starter(js, Pg.ReadStr(js, "nome"), Pg.ReadInt(js, "idade")), sol, tests,
                    L.T("A idade precisa virar número (int / Number) para podermos somar; o nome continua texto. Depois é só montar as duas linhas com as variáveis."));
            }
            case 1:
            {
                var prices = new[] { "12.50", "3.90", "7.25", "19.99", "2.40", "45.00", "8.75" };
                var ins = new[] { "12.50\n3", "2.40\n5" }.Concat(Enumerable.Range(0, 3).Select(_ => Ex.Pick(r, prices) + "\n" + Ex.Int(r, 1, 9))).ToArray();
                string Model(string i)
                {
                    var p = i.TrimEnd('\n').Split('\n');
                    decimal total = decimal.Parse(p[0], CultureInfo.InvariantCulture) * int.Parse(p[1]);
                    decimal pay = Math.Ceiling(total / 50m) * 50m;
                    return $"Total: R$ {total.ToString("0.00", CultureInfo.InvariantCulture)}\nTroco para R$ {pay.ToString("0", CultureInfo.InvariantCulture)}: R$ {(pay - total).ToString("0.00", CultureInfo.InvariantCulture)}";
                }
                var tests = Pg.Tests(ins, Model);
                string rf = js ? "const preco = Number(prompt());" : "preco = float(input())";
                var sol = js
                    ? Pg.Code(rf, Pg.ReadInt(true, "qtd"), "const total = preco * qtd;", "const pago = Math.ceil(total / 50) * 50;",
                        "console.log(`Total: R$ ${total.toFixed(2)}`);", "console.log(`Troco para R$ ${pago}: R$ ${(pago - total).toFixed(2)}`);")
                    : Pg.Code(rf, Pg.ReadInt(false, "qtd"), "total = preco * qtd", "pago = -(-total // 50) * 50", "print(f\"Total: R$ {total:.2f}\")", "print(f\"Troco para R$ {int(pago)}: R$ {pago - total:.2f}\")");
                return Ex.Code(Pg.Prompt(L.T("Leia o preço de um produto (número decimal) e a quantidade comprada (inteiro). Mostre o total com 2 casas decimais. Depois suponha que a pessoa paga com o menor múltiplo de 50 reais que cobre o total e mostre o troco, também com 2 casas:")
                    + "\n" + Pg.Plain("Total: R$ <total>\nTroco para R$ <valor pago>: R$ <troco>"), tests[0]),
                    Pg.Starter(js, rf, Pg.ReadInt(js, "qtd")), sol, tests,
                    L.T("total = preço × quantidade. Para achar o menor múltiplo de 50 que cobre o total arredondamos total/50 para cima e multiplicamos por 50. O troco é pago − total, mostrado com 2 casas (:.2f em Python, toFixed(2) em JavaScript)."));
            }
            case 2:
            {
                var pool = new[]
                {
                    new WordOp("Tamanho", "len(p)", "p.length", w => w.Length.ToString()),
                    new WordOp("Maiúsculas", "p.upper()", "p.toUpperCase()", w => w.ToUpperInvariant()),
                    new WordOp("Minúsculas", "p.lower()", "p.toLowerCase()", w => w.ToLowerInvariant()),
                    new WordOp("Primeira", "p[0]", "p[0]", w => w[0].ToString()),
                    new WordOp("Última", "p[-1]", "p[p.length - 1]", w => w[^1].ToString()),
                };
                int times = Ex.Int(r, 2, 4);
                var chosen = pool.OrderBy(_ => r.Next()).Take(3).Append(new WordOp($"Repetida {times}x", $"p * {times}", $"p.repeat({times})", w => string.Concat(Enumerable.Repeat(w, times)))).ToArray();
                string Rand(string w) { string x; do x = string.Concat(w.Select(c => r.Next(2) == 0 ? char.ToUpperInvariant(c) : c)); while (x.Contains("NaN")); return x; }
                var tests = Pg.Tests(new[] { "x", "Banana", "TIGRE", Rand(Ex.Pick(r, Pg.Words)), Rand(Ex.Pick(r, Pg.Words)) },
                    i => string.Join("\n", chosen.Select(o => $"{o.Label}: {o.F(i.Trim())}")));
                var sol = Pg.Code(new[] { Pg.ReadStr(js, "p") }.Concat(chosen.Select(o => Pg.Pr(js, o.Label + ": {" + (js ? o.Js : o.Py) + "}"))).ToArray());
                return Ex.Code(Pg.Prompt(L.T("Leia uma palavra (uma linha) e mostre uma linha para cada item, nesta ordem:") + "\n" + Pg.Plain(string.Join("\n", chosen.Select(o => o.Label + ": <valor>")))
                    + "\n" + L.T("(Primeira e Última são a primeira e a última letra.)"), tests[0]),
                    Pg.Starter(js, Pg.ReadStr(js, "p")), sol, tests,
                    L.T("Cada item usa um recurso do texto: tamanho, maiúsculas/minúsculas, posição (0 é a primeira letra; a última fica no tamanho − 1) e repetição."));
            }
            default:
            {
                var tests = Pg.Tests(new[] { "12\n5", "7\n300", Ex.Int(r, 10, 99) + "\n" + Ex.Int(r, 10, 999), Ex.Int(r, 1, 9) + "\n" + Ex.Int(r, 1, 9), Ex.Int(r, 100, 999) + "\n" + Ex.Int(r, 100, 999) },
                    i => { var p = i.TrimEnd('\n').Split('\n'); return $"Colados: {p[0]}{p[1]}\nSomados: {int.Parse(p[0]) + int.Parse(p[1])}\nDígitos: {p[0].Length + p[1].Length}"; });
                var sol = js
                    ? Pg.Code("const a = prompt();", "const b = prompt();", "const colados = a + b;", "console.log(`Colados: ${colados}`);", "console.log(`Somados: ${Number(a) + Number(b)}`);", "console.log(`Dígitos: ${colados.length}`);")
                    : Pg.Code("a = input()", "b = input()", "colados = a + b", "print(f\"Colados: {colados}\")", "print(f\"Somados: {int(a) + int(b)}\")", "print(f\"Dígitos: {len(colados)}\")");
                return Ex.Code(Pg.Prompt(L.T("Leia dois números inteiros positivos, um por linha. Mostre três linhas: os dois números COLADOS um depois do outro (como texto, 12 e 5 viram 125), a SOMA deles e a quantidade de dígitos do número colado."), tests[0]),
                    Pg.Starter(js, js ? "const a = prompt();" : "a = input()", js ? "const b = prompt();" : "b = input()"), sol, tests,
                    L.T("O que se lê é texto: com + os textos se colam (\"12\" + \"5\" = \"125\") e o tamanho do texto colado é a quantidade de dígitos. Para somar é preciso converter cada um para número antes."));
            }
        }
    }

    static Exercise MakeOut(bool js, int lv, Random r)
    {
        if (lv == 1 || (lv == 2 && r.Next(2) == 0)) return Types(js, r, lv);
        if (lv == 3) return Quirks(js, r);
        return Reassign(js, r, lv);
    }

    // a and b change through a few statements; print both.
    static Exercise Reassign(bool js, Random r, int lv)
    {
        long a = Ex.Int(r, 1, 9), b = Ex.Int(r, 1, 9);
        var code = new List<string> { js ? $"let a = {a};" : $"a = {a}", js ? $"let b = {b};" : $"b = {b}" };
        int steps = lv == 0 ? 2 : 4;
        for (int i = 0; i < steps; i++)
        {
            int k = Ex.Int(r, 2, 5);
            switch (r.Next(6))
            {
                case 0: code.Add(js ? "a = a + b;" : "a = a + b"); a += b; break;
                case 1: code.Add(js ? "b = a - b;" : "b = a - b"); b = a - b; break;
                case 2: code.Add(js ? $"a = a * {k};" : $"a = a * {k}"); a *= k; break;
                case 3: code.Add(js ? "b += a;" : "b += a"); b += a; break;
                case 4: code.Add(js ? $"a = b + {k};" : $"a = b + {k}"); a = b + k; break;
                default: code.Add(js ? "b = b * b;" : "b = b * b"); b *= b; break;
            }
        }
        code.Add(js ? "console.log(a, b);" : "print(a, b)");
        string outp = $"{a} {b}";
        return Ex.Output(PgOut.Prompt(outp), string.Join("\n", code), outp,
            L.T("Execute linha por linha, anotando os valores de a e b depois de cada atribuição. Cada atribuição usa os valores que as variáveis têm NAQUELE momento.") + "\n"
            + string.Join("\n", Trace(code.Take(code.Count - 1).ToList(), r)));
    }

    // Re-run the assignment lines to show a trace of (a, b); lines are simple enough to parse here.
    static List<string> Trace(List<string> code, Random _)
    {
        long a = 0, b = 0;
        var res = new List<string>();
        foreach (var raw in code)
        {
            var s = raw.TrimEnd(';').Replace("let ", "");
            if (s.StartsWith("a = a + b")) a += b;
            else if (s.StartsWith("b = a - b")) b = a - b;
            else if (s.StartsWith("a = a * ")) a *= long.Parse(s[8..]);
            else if (s.StartsWith("b += a")) b += a;
            else if (s.StartsWith("a = b + ")) a = b + long.Parse(s[8..]);
            else if (s.StartsWith("b = b * b")) b *= b;
            else if (s.StartsWith("a = ")) a = long.Parse(s[4..]);
            else if (s.StartsWith("b = ")) b = long.Parse(s[4..]);
            res.Add($"`{raw}` → a = {a}, b = {b}");
        }
        return res;
    }

    record TypeRow(string Py, string Js, string PyType, string JsType);

    static readonly TypeRow[] TypeRows =
    {
        new("7", "7", "int", "number"), new("2.5", "2.5", "float", "number"), new("\"7\"", "\"7\"", "str", "string"),
        new("True", "true", "bool", "boolean"), new("7 / 2", "7 / 2", "float", "number"), new("7 // 2", "Math.trunc(7 / 2)", "int", "number"),
        new("\"a\" * 3", "\"a\".repeat(3)", "str", "string"), new("len(\"abc\")", "\"abc\".length", "int", "number"),
        new("3 > 2", "3 > 2", "bool", "boolean"), new("str(7)", "String(7)", "str", "string"), new("int(\"7\")", "Number(\"7\")", "int", "number"),
        new("1 + 2.0", "1 + 2.0", "float", "number"), new("\"ab\" + \"cd\"", "\"ab\" + \"cd\"", "str", "string"), new("10 % 3", "10 % 3", "int", "number"),
    };

    static Exercise Types(bool js, Random r, int lv)
    {
        var rows = TypeRows.OrderBy(_ => r.Next()).Take(lv == 1 ? 3 : 5).ToArray();
        var code = rows.Select(x => js ? $"console.log(typeof ({x.Js}));" : $"print(type({x.Py}).__name__)").ToArray();
        string outp = string.Join("\n", rows.Select(x => js ? x.JsType : x.PyType));
        return Ex.Output(PgOut.Prompt(outp, L.T("Cada linha mostra o TIPO do valor (o nome que a linguagem dá a ele).")), string.Join("\n", code), outp,
            L.T("O tipo é decidido pelo resultado: / sempre dá número decimal em Python; texto entre aspas é texto mesmo que pareça número; comparações dão verdadeiro/falso.") + "\n"
            + string.Join("\n", rows.Select(x => $"`{(js ? x.Js : x.Py)}` → {(js ? x.JsType : x.PyType)}")));
    }

    record Quirk(string Py, string Js, string PyOut, string JsOut);

    static Exercise Quirks(bool js, Random r)
    {
        var all = new[]
        {
            new Quirk("10 / 5", "10 / 5", "2.0", "2"), new Quirk("\"7\" * 3", "\"7\" * 3", "777", "21"), new Quirk("True + True", "true + true", "2", "2"),
            new Quirk("5 == \"5\"", "5 == \"5\"", "False", "true"), new Quirk("2 ** 10", "2 ** 10", "1024", "1024"), new Quirk("10 % 3", "10 % 3", "1", "1"),
            new Quirk("\"ab\" == \"ab\"", "\"ab\" == \"ab\"", "True", "true"), new Quirk("1 + 1.5", "1 + 1.5", "2.5", "2.5"), new Quirk("\"3\" + \"4\"", "\"3\" + \"4\"", "34", "34"),
            new Quirk("7 // 2", "Math.trunc(7 / 2)", "3", "3"), new Quirk("8 / 2 == 4", "8 / 2 == 4", "True", "true"), new Quirk("\"5\" * 2 == 55", "\"5\" * 2 == 55", "False", "false"),
        };
        var rows = all.OrderBy(_ => r.Next()).Take(4).ToArray();
        string outp = string.Join("\n", rows.Select(x => js ? x.JsOut : x.PyOut));
        var code = rows.Select(x => js ? $"console.log({x.Js});" : $"print({x.Py})");
        return Ex.Output(PgOut.Prompt(outp, L.T("Atenção: cada linguagem tem as suas manias com tipos. Escreva exatamente o que ELA imprime (verdadeiro/falso aparecem do jeito dela).")),
            string.Join("\n", code), outp,
            L.T("Detalhes que mudam o resultado: em Python / dá sempre decimal (10 / 5 é 2.0), texto * número repete o texto e os booleanos se escrevem True/False; em JavaScript * converte o texto em número, == compara convertendo os tipos e os booleanos são true/false.") + "\n"
            + string.Join("\n", rows.Select(x => $"`{(js ? x.Js : x.Py)}` → {(js ? x.JsOut : x.PyOut)}")));
    }
}
