namespace EasyCreator.Exercises;

/// <summary>Lesson 7 (lists / arrays and strings): operations on a sentence's words, letters, anagrams, and tracing list/string operations and aliasing.</summary>
[Exercises("py-07", "js-07")]
public sealed class P07ListsStrings : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        bool js = Pg.Js(lesson);
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(js, lv, r) : MakeOut(js, lv, r);
    }

    record SOp(string Label, string Py, string Js, Func<string[], string, string> F);

    static readonly SOp[] SOps =
    {
        new("Palavras", "len(p)", "p.length", (p, s) => p.Length.ToString()),
        new("Primeira", "p[0]", "p[0]", (p, s) => p[0]),
        new("Última", "p[-1]", "p[p.length - 1]", (p, s) => p[^1]),
        new("Invertida", "\" \".join(p[::-1])", "[...p].reverse().join(\" \")", (p, s) => string.Join(" ", p.Reverse())),
        new("Mais longa", "max(p, key=len)", "p.reduce((m, w) => w.length > m.length ? w : m)", (p, s) => p.Aggregate((m, w) => w.Length > m.Length ? w : m)),
        new("Em ordem", "\" \".join(sorted(p))", "[...p].sort().join(\" \")", (p, s) => string.Join(" ", p.OrderBy(w => w, StringComparer.Ordinal))),
        new("Maiúsculas", "s.upper()", "s.toUpperCase()", (p, s) => s.ToUpperInvariant()),
        new("Letras", "len(s.replace(\" \", \"\"))", "s.split(\" \").join(\"\").length", (p, s) => s.Replace(" ", "").Length.ToString()),
    };

    static string Sentence(Random r, int n = -1)
    {
        n = n < 0 ? Ex.Int(r, 2, 6) : n;
        return string.Join(" ", Enumerable.Range(0, n).Select(_ => Ex.Pick(r, Pg.Words)));
    }

    static Exercise MakeCode(bool js, int lv, Random r)
    {
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        switch (lv)
        {
            case 0:
            case 1:
            {
                var chosen = SOps.OrderBy(_ => r.Next()).Take(lv == 0 ? 2 : 4).ToArray();
                var tests = Pg.Tests(new[] { "o rato roeu a roupa", "sol", Sentence(r), Sentence(r) }, i => { var s = i.Trim(); var p = s.Split(' '); return string.Join("\n", chosen.Select(o => $"{o.Label}: {o.F(p, s)}")); });
                var lines = new List<string> { Pg.ReadStr(js, "s"), js ? "const p = s.split(\" \");" : "p = s.split()" };
                lines.AddRange(chosen.Select(o => Pg.Pr(js, o.Label + ": {" + (js ? o.Js : o.Py) + "}")));
                return Ex.Code(Pg.Prompt(L.T("Leia uma frase (palavras separadas por UM espaço, só letras minúsculas sem acento) e mostre uma linha para cada item, nesta ordem:") + "\n"
                    + Pg.Plain(string.Join("\n", chosen.Select(o => o.Label + ": <valor>"))) + "\n"
                    + L.T("(Palavras: quantas; Primeira/Última: a primeira e a última palavra; Invertida: as palavras na ordem contrária; Mais longa: a primeira palavra de maior tamanho; Em ordem: as palavras em ordem alfabética; Maiúsculas: a frase toda em maiúsculas; Letras: quantas letras, sem contar os espaços.)"), tests[0]),
                    Pg.Code(Pg.ReadStr(js, "s"), js ? "const p = s.split(\" \");" : "p = s.split()", Pg.Comment(js, L.T("escreva os prints pedidos"))), Pg.Code(lines.ToArray()), tests,
                    L.T("Separamos a frase em uma lista de palavras com split e usamos o que a lista e o texto oferecem: tamanho, posições (0 é a primeira; a última fica em tamanho − 1), inverter, ordenar, maior por tamanho e maiúsculas."));
            }
            case 2:
            {
                if (r.Next(2) == 0)
                {
                    string Model(string s, char c) => $"Vezes: {s.Count(x => x == c)}\nPosição: {s.IndexOf(c)}";
                    var ins = new List<string> { "banana\na", "casa\nz" };
                    for (int i = 0; i < 3; i++) { var s = Sentence(r, 3); char c = r.Next(4) == 0 ? 'z' : s[r.Next(s.Length)]; ins.Add(s + "\n" + (c == ' ' ? 'a' : c)); }
                    var tests = Pg.Tests(ins, i => { var p = i.TrimEnd('\n').Split('\n'); return Model(p[0], p[1][0]); });
                    var sol = Pg.Code(Pg.ReadStr(js, "s"), Pg.ReadStr(js, "c"), Pg.Pr(js, "Vezes: {" + (js ? "s.split(c).length - 1" : "s.count(c)") + "}"), Pg.Pr(js, "Posição: {" + (js ? "s.indexOf(c)" : "s.find(c)") + "}"));
                    return Ex.Code(Pg.Prompt(L.T("Leia uma frase e, na linha seguinte, uma letra. Mostre quantas vezes a letra aparece na frase e a posição da primeira aparição (a primeira posição é 0; se a letra não aparece, a posição é -1)."), tests[0]),
                        Pg.Starter(js, Pg.ReadStr(js, "s"), Pg.ReadStr(js, "c")), sol, tests,
                        L.T("Python tem s.count(c) e s.find(c); JavaScript tem s.indexOf(c) e, para contar, dá para dividir a frase pela letra (split) e ver quantos pedaços saem: aparições = pedaços − 1."));
                }
                else
                {
                    string Model(string s) { var t = s.Replace(" ", "").ToLowerInvariant(); return t.SequenceEqual(t.Reverse()) ? "sim" : "não"; }
                    var pal = new[] { "Ame a ema", "Socorram me subi no onibus em Marrocos", "A base do teto desaba", "Luz azul", "o rato roeu" };
                    var ins = pal.Concat(new[] { Sentence(r, 2), Sentence(r, 3) }).ToList();
                    string w = Ex.Pick(r, Pg.Words); ins.Add(w + " a " + new string(w.Reverse().ToArray()));
                    var tests = Pg.Tests(ins, i => Model(i.Trim()));
                    var sol = js ? Pg.Code(Pg.ReadStr(true, "s"), "const t = s.split(\" \").join(\"\").toLowerCase();", "const inv = t.split(\"\").reverse().join(\"\");", "console.log(t === inv ? \"sim\" : \"não\");")
                        : Pg.Code(Pg.ReadStr(false, "s"), "t = s.replace(\" \", \"\").lower()", "print(\"sim\" if t == t[::-1] else \"não\")");
                    return Ex.Code(Pg.Prompt(L.T("Leia uma frase e mostre `sim` se ela é um palíndromo e `não` se não é. Ignore os espaços e a diferença entre maiúsculas e minúsculas (\"Ame a ema\" é palíndromo)."), tests[0]),
                        Pg.Starter(js, Pg.ReadStr(js, "s")), sol, tests,
                        L.T("Tiramos os espaços, passamos tudo para minúsculas e comparamos o texto com ele mesmo invertido."));
                }
            }
            default:
            {
                if (r.Next(2) == 0)
                {
                    string Model(string a, string b) => string.Concat(a.OrderBy(c => c)) == string.Concat(b.OrderBy(c => c)) ? "sim" : "não";
                    var ins = new List<string> { "amor\nroma", "gato\ngata" };
                    for (int i = 0; i < 3; i++) { string w = Ex.Pick(r, Pg.Words); var chars = w.OrderBy(_ => r.Next()).ToArray(); string o = r.Next(2) == 0 ? new string(chars) : Ex.Pick(r, Pg.Words); ins.Add(w + "\n" + o); }
                    var tests = Pg.Tests(ins, i => { var p = i.TrimEnd('\n').Split('\n'); return Model(p[0], p[1]); });
                    var sol = js ? Pg.Code(Pg.ReadStr(true, "a"), Pg.ReadStr(true, "b"), "const ord = (w) => w.split(\"\").sort().join(\"\");", "console.log(ord(a) === ord(b) ? \"sim\" : \"não\");")
                        : Pg.Code(Pg.ReadStr(false, "a"), Pg.ReadStr(false, "b"), "print(\"sim\" if sorted(a) == sorted(b) else \"não\")");
                    return Ex.Code(Pg.Prompt(L.T("Duas palavras são anagramas quando usam exatamente as mesmas letras, as mesmas vezes, em outra ordem (amor e roma). Leia duas palavras (uma por linha, minúsculas) e mostre `sim` se são anagramas ou `não` se não são."), tests[0]),
                        Pg.Starter(js, Pg.ReadStr(js, "a"), Pg.ReadStr(js, "b")), sol, tests,
                        L.T("O truque: se as duas palavras têm as mesmas letras, ao colocar as letras de cada uma em ordem alfabética elas ficam IGUAIS. Basta ordenar e comparar."));
                }
                else
                {
                    var tests = Pg.Tests(new[] { "ana maria silva", "sol", Sentence(r, 3), Sentence(r, 4), Sentence(r, 2) },
                        i => { var p = i.Trim().Split(' '); return $"Sigla: {string.Concat(p.Select(w => char.ToUpperInvariant(w[0])))}\nTítulo: {string.Join(" ", p.Select(w => char.ToUpperInvariant(w[0]) + w[1..]))}"; });
                    var sol = js ? Pg.Code(Pg.ReadStr(true, "s"), "const p = s.split(\" \");", "console.log(`Sigla: ${p.map((w) => w[0].toUpperCase()).join(\"\")}`);", "console.log(`Título: ${p.map((w) => w[0].toUpperCase() + w.slice(1)).join(\" \")}`);")
                        : Pg.Code(Pg.ReadStr(false, "s"), "p = s.split()", "print(\"Sigla: \" + \"\".join(w[0].upper() for w in p))", "print(\"Título: \" + \" \".join(w[0].upper() + w[1:] for w in p))");
                    return Ex.Code(Pg.Prompt(L.T("Leia uma frase (palavras separadas por um espaço, minúsculas) e mostre duas linhas: `Sigla: ` seguida das iniciais das palavras em maiúsculas, e `Título: ` com a frase onde cada palavra começa com letra maiúscula."), tests[0]),
                        Pg.Starter(js, Pg.ReadStr(js, "s")), sol, tests,
                        L.T("Percorremos a lista de palavras transformando cada uma: a inicial é w[0] em maiúscula; o resto da palavra é o corte w[1:] (Python) ou w.slice(1) (JavaScript). Depois juntamos com join."));
                }
            }
        }
    }

    static Exercise MakeOut(bool js, int lv, Random r)
    {
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        string jn(string l) => js ? $"{l}.join(\" \")" : $"\" \".join(str(x) for x in {l})";
        switch (lv)
        {
            case 0:
            {
                var l = Pg.Arr(r, 3, 1, 9).ToList(); int a = Ex.Int(r, 10, 20), b = Ex.Int(r, 1, 9);
                var code = new List<string> { js ? $"const l = [{string.Join(", ", l)}];" : $"l = [{string.Join(", ", l)}]", js ? $"l.push({a});" : $"l.append({a})" };
                var work = new List<int>(l) { a };
                var outs = new List<string> { work.Count.ToString() };
                code.Add(pr(js ? "l.length" : "len(l)"));
                code.Add(js ? "const ultimo = l.pop();" : "ultimo = l.pop()"); int last = work[^1]; work.RemoveAt(work.Count - 1);
                code.Add(pr("ultimo")); outs.Add(last.ToString());
                code.Add(js ? $"l.push({b});" : $"l.append({b})"); work.Add(b);
                code.Add(pr(jn("l"))); outs.Add(string.Join(" ", work));
                code.Add(pr(js ? "l[0] + l[l.length - 1]" : "l[0] + l[-1]")); outs.Add((work[0] + work[^1]).ToString());
                string outp = string.Join("\n", outs);
                return Ex.Output(PgOut.Prompt(outp), string.Join("\n", code), outp, L.T("append/push põe no fim da lista, pop tira (e devolve) o último. Acompanhe a lista depois de cada comando."));
            }
            case 1:
            {
                var l = Pg.Arr(r, 6, 1, 9); string w = Ex.Pick(r, Pg.Words) + Ex.Pick(r, Pg.Words);
                int i1 = Ex.Int(r, 0, 2), i2 = i1 + Ex.Int(r, 2, 3), k = Ex.Int(r, 2, 3);
                var code = new List<string> { js ? $"const l = [{string.Join(", ", l)}];" : $"l = [{string.Join(", ", l)}]", js ? $"const w = \"{w}\";" : $"w = \"{w}\"" };
                var outs = new List<string>();
                code.Add(pr(jn(js ? $"l.slice({i1}, {i2})" : $"l[{i1}:{i2}]"))); outs.Add(string.Join(" ", l.Skip(i1).Take(i2 - i1)));
                code.Add(pr(js ? $"w.slice(0, {k})" : $"w[:{k}]")); outs.Add(w[..k]);
                code.Add(pr(js ? $"w.slice(-{k})" : $"w[-{k}:]")); outs.Add(w[^k..]);
                code.Add(pr(jn(js ? $"l.slice({i1})" : $"l[{i1}:]"))); outs.Add(string.Join(" ", l.Skip(i1)));
                string outp = string.Join("\n", outs);
                return Ex.Output(PgOut.Prompt(outp), string.Join("\n", code), outp, L.T("O corte [a:b] / slice(a, b) pega da posição a até ANTES da b (a primeira posição é 0). Sem o fim, vai até o final; com número negativo, conta a partir do fim."));
            }
            case 2:
            {
                string w1 = Ex.Pick(r, Pg.Words), w2 = Ex.Pick(r, Pg.Words), w3 = Ex.Pick(r, Pg.Words);
                string s = $"{w1},{w2},{w3}"; char ch = w1[r.Next(w1.Length)];
                var code = new List<string> { js ? $"const s = \"{s}\";" : $"s = \"{s}\"" };
                var outs = new List<string>();
                code.Add(pr(js ? "s.split(\",\").length" : "len(s.split(\",\"))")); outs.Add("3");
                code.Add(pr(js ? "s.split(\",\").join(\" - \")" : "\" - \".join(s.split(\",\"))")); outs.Add($"{w1} - {w2} - {w3}");
                code.Add(pr(js ? $"s.indexOf(\"{ch}\")" : $"s.find(\"{ch}\")")); outs.Add(s.IndexOf(ch).ToString());
                code.Add(pr(js ? $"s.split(\"{ch}\").join(\"*\")" : $"s.replace(\"{ch}\", \"*\")")); outs.Add(s.Replace(ch.ToString(), "*"));
                string outp = string.Join("\n", outs);
                return Ex.Output(PgOut.Prompt(outp), string.Join("\n", code), outp, L.T("split(\",\") quebra o texto em pedaços nas vírgulas; join junta de novo com outro separador; find/indexOf devolve a posição da PRIMEIRA aparição; replace troca todas as aparições."));
            }
            default:
            {
                var a = Pg.Arr(r, 3, 1, 9).ToList(); int x = Ex.Int(r, 10, 20), y = Ex.Int(r, 21, 30), z = Ex.Int(r, 31, 40);
                var b = a; var c = new List<int>(a);
                var code = new List<string> { js ? $"const a = [{string.Join(", ", a)}];" : $"a = [{string.Join(", ", a)}]", js ? "const b = a;" : "b = a", js ? "const c = [...a];" : "c = a[:]" };
                code.Add(js ? $"b.push({x});" : $"b.append({x})"); b.Add(x);
                code.Add(js ? $"c.push({y});" : $"c.append({y})"); c.Add(y);
                code.Add(js ? $"a.push({z});" : $"a.append({z})"); a.Add(z);
                code.Add(pr(js ? "a.length + \" \" + b.length + \" \" + c.length" : "len(a), len(b), len(c)"));
                code.Add(pr(js ? "(a === b) + \" \" + (a === c)" : "a is b, a is c"));
                code.Add(pr(js ? "a.reduce((s, v) => s + v, 0) + \" \" + c.reduce((s, v) => s + v, 0)" : "sum(a), sum(c)"));
                string outp = $"{a.Count} {b.Count} {c.Count}\n{(js ? "true false" : "True False")}\n{a.Sum()} {c.Sum()}";
                return Ex.Output(PgOut.Prompt(outp), string.Join("\n", code), outp,
                    L.T("`b = a` NÃO copia a lista: a e b são dois nomes para a MESMA lista, então mexer em uma mexe na outra. `a[:]` (ou [...a]) faz uma cópia independente, que só muda quando mexemos em c."));
            }
        }
    }
}
