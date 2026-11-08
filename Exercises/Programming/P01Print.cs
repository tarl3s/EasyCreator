namespace EasyCreator.Exercises;

/// <summary>Lesson 1 (print / console.log): greeting programs, frames, and predicting what a few prints show.</summary>
[Exercises("py-01", "js-01")]
public sealed class P01Print : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        bool js = Pg.Js(lesson);
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(js, lv, r) : MakeOut(js, lv, r);
    }

    static Exercise MakeCode(bool js, int lv, Random r)
    {
        string Q(string s) => "\"" + s + "\"";
        string[] names = Pg.Names;
        switch (lv)
        {
            case 0:
            {
                string g = Ex.Pick(r, "Olá", "Oi", "Salve", "Bem-vindo"), p = Ex.Pick(r, "!", ".");
                string s = Ex.Pick(r, "Vamos programar.", "Prazer em conhecer.", "Hoje o dia é de código.", "Tudo pronto para começar.");
                var tests = Pg.Tests(new[] { Ex.Pick(r, names), "Zeca", Ex.Pick(r, names) + " " + Ex.Pick(r, names) }, i => $"{g}, {i.Trim()}{p}\n{s}");
                var sol = Pg.Code(Pg.ReadStr(js, "nome"), Pg.Pr(js, g + ", {nome}" + p), Pg.Pr(js, s));
                return Ex.Code(Pg.Prompt(L.T("Leia o nome de uma pessoa (uma linha) e mostre exatamente duas linhas:") + "\n" + Pg.Plain($"{g}, <nome>{p}\n{s}"), tests[0]),
                    Pg.Starter(js, Pg.ReadStr(js, "nome")), sol, tests,
                    L.T("Guardamos a linha lida numa variável e usamos a variável dentro do texto do print (f-string em Python, crase com ${} em JavaScript). A segunda linha é um texto fixo."));
            }
            case 1:
            {
                var lines = new[] { "{nome} mora em {cidade}.", "Cidade: {cidade}", "Pessoa: {nome}", "{nome} ({cidade})", "{cidade} é a casa de {nome}." };
                var pick = lines.OrderBy(_ => r.Next()).Take(2).ToArray();
                string[] cities = { "Recife", "Curitiba", "Belem", "Natal", "Salvador", "Manaus", "Santos" };
                string Fmt(string t, string n, string c) => t.Replace("{nome}", n).Replace("{cidade}", c);
                var ins = new[] { "Ana\nRecife", "Bruno\nSao Paulo", Ex.Pick(r, names) + "\n" + Ex.Pick(r, cities), Ex.Pick(r, names) + "\n" + Ex.Pick(r, cities) };
                var tests = Pg.Tests(ins, i => { var p = i.TrimEnd('\n').Split('\n'); return string.Join("\n", pick.Select(t => Fmt(t, p[0], p[1]))); });
                var sol = Pg.Code(Pg.ReadStr(js, "nome"), Pg.ReadStr(js, "cidade"), Pg.Pr(js, pick[0]), Pg.Pr(js, pick[1]));
                return Ex.Code(Pg.Prompt(L.T("Leia duas linhas: o nome de uma pessoa e o nome de uma cidade. Mostre exatamente duas linhas:") + "\n"
                    + Pg.Plain(string.Join("\n", pick.Select(t => t.Replace("{nome}", "<nome>").Replace("{cidade}", "<cidade>")))), tests[0]),
                    Pg.Starter(js, Pg.ReadStr(js, "nome"), Pg.ReadStr(js, "cidade")), sol, tests,
                    L.T("Cada entrada vai para a sua variável (a primeira linha lida é o nome, a segunda a cidade) e cada print monta o texto pedido com as duas variáveis."));
            }
            case 2:
            {
                string c = Ex.Pick(r, "*", "#", "=", "+", "-");
                var tests = Pg.Tests(new[] { "Ana", "Sol", Ex.Pick(r, Pg.Words), Ex.Pick(r, Pg.Words) + Ex.Pick(r, Pg.Words) },
                    i => { var w = i.Trim(); return new string(c[0], w.Length + 4) + $"\n{c} {w} {c}\n" + new string(c[0], w.Length + 4); });
                string lp = js ? "console.log(linha);" : "print(linha)";
                var sol = Pg.Code(Pg.ReadStr(js, "w"), js ? $"const linha = {Pg.Repeat(js, Q(c), "(w.length + 4)")};" : $"linha = {Pg.Repeat(js, Q(c), "(len(w) + 4)")}",
                    lp, Pg.Pr(js, c + " {w} " + c), lp);
                return Ex.Code(Pg.Prompt(L.F("Leia uma palavra e mostre-a numa moldura de `{0}`: a linha de cima e a de baixo têm o tamanho da palavra mais 4 caracteres `{0}`, e a do meio é `{0} palavra {0}`.", c), tests[0]),
                    Pg.Starter(js, Pg.ReadStr(js, "w")), sol, tests,
                    L.T("Se a palavra tem n letras, a linha do meio tem n + 4 caracteres (dois símbolos e dois espaços). A moldura é o símbolo repetido n + 4 vezes, guardada numa variável para ser usada em cima e embaixo."));
            }
            default:
            {
                bool diamond = r.Next(2) == 0;
                string Model(string w)
                {
                    var lines = new List<string>();
                    for (int i = 1; i <= w.Length; i++) lines.Add(w[..i]);
                    if (diamond) for (int i = w.Length - 1; i >= 1; i--) lines.Add(w[..i]);
                    return string.Join("\n", lines);
                }
                var tests = Pg.Tests(new[] { "x", "casa", "ab", Ex.Pick(r, Pg.Words), Ex.Pick(r, Pg.Words) }, i => Model(i.Trim()));
                string up = js ? "for (let i = 1; i <= w.length; i++) console.log(w.slice(0, i));" : "for i in range(1, len(w) + 1):\n    print(w[:i])";
                string down = js ? "for (let i = w.length - 1; i >= 1; i--) console.log(w.slice(0, i));" : "for i in range(len(w) - 1, 0, -1):\n    print(w[:i])";
                var sol = Pg.Code(Pg.ReadStr(js, "w"), up) + (diamond ? "\n" + down : "");
                return Ex.Code(Pg.Prompt(L.T("Leia uma palavra e mostre, uma por linha, os seus começos cada vez maiores: a primeira linha tem só a 1ª letra, a segunda as 2 primeiras, e assim até a palavra inteira.")
                    + (diamond ? " " + L.T("Depois volte diminuindo: mostre de novo os começos de tamanho (n−1) até 1, formando um losango.") : ""), tests[0]),
                    Pg.Starter(js, Pg.ReadStr(js, "w")), sol, tests,
                    L.T("O truque é cortar a palavra: w[:i] (Python) ou w.slice(0, i) (JavaScript) devolve as i primeiras letras. Um laço faz i crescer de 1 até o tamanho da palavra; para o losango, outro laço volta de n−1 até 1."));
            }
        }
    }

    static Exercise MakeOut(bool js, int lv, Random r)
    {
        string a = Ex.Pick(r, Pg.Words), b = Ex.Pick(r, Pg.Words);
        while (b == a) b = Ex.Pick(r, Pg.Words);
        int n = Ex.Int(r, 2, 4);
        string Q(string s) => "\"" + s + "\"";
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        // (code, output) of form k, using the variables a and b.
        (string c, string o) Form(int k) => k switch
        {
            0 => (pr("a"), a),
            1 => (pr("a, b"), a + " " + b),
            2 => (pr("a + b"), a + b),
            3 => (pr(Pg.Repeat(js, "b", n.ToString())), string.Concat(Enumerable.Repeat(b, n))),
            4 => (pr(Q("-") + (js ? ".repeat(" + n + ")" : " * " + n) + " + " + Q(">")), new string('-', n) + ">"),
            5 => (pr("a + " + Q(" ") + " + b"), a + " " + b),
            6 => (pr(Q("1\\n2")), "1\n2"),
            7 => (pr(js ? "a[0] + b[b.length - 1]" : "a[0] + b[-1]"), a[0].ToString() + b[^1]),
            _ => (pr(js ? "a.toUpperCase() + b" : "a.upper() + b"), a.ToUpperInvariant() + b),
        };
        int[] pool = lv switch { 0 => new[] { 0, 1, 2 }, 1 => new[] { 0, 1, 2, 3, 5 }, 2 => new[] { 1, 2, 3, 4, 5, 6 }, _ => new[] { 3, 4, 6, 7, 8, 5 } };
        int count = lv switch { 0 => 3, 1 => 4, 2 => 4, _ => 5 };
        var chosen = pool.OrderBy(_ => r.Next()).Take(count).ToArray();
        var progLines = new List<string> { js ? $"const a = {Q(a)};" : $"a = {Q(a)}", js ? $"const b = {Q(b)};" : $"b = {Q(b)}" };
        var outs = new List<string>();
        foreach (var k in chosen) { var (c, o) = Form(k); progLines.Add(c); outs.Add(o); }
        string explain = string.Join("\n", chosen.Select((k, i) => $"{i + 1}. `{Form(k).c}` → " + Form(k).o.Replace("\n", " ⏎ ")));
        return Ex.Output(PgOut.Prompt(string.Join("\n", outs)),
            string.Join("\n", progLines), string.Join("\n", outs),
            L.T("Cada print escreve uma linha. Com vírgula os valores saem separados por um espaço; com + os textos são colados sem espaço; * (ou repeat) repete o texto; \\n quebra a linha.") + "\n" + explain);
    }
}
