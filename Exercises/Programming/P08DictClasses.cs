namespace EasyCreator.Exercises;

/// <summary>Lesson 8 (dictionaries / objects and classes): counting with a map, classes with methods, an inventory and a bank account driven by commands; tracing maps and objects.</summary>
[Exercises("py-08", "js-08")]
public sealed class P08DictClasses : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        bool js = Pg.Js(lesson);
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(js, lv, r) : MakeOut(js, lv, r);
    }

    record Meth(string Name, string Label, string Desc, string PyExpr, string JsExpr, Func<long[], long> F);
    record Cls(string Name, string[] Fields, Meth[] Methods);

    static readonly Cls[] Classes =
    {
        new("Retangulo", new[] { "largura", "altura" }, new[] { new Meth("area", "Área", "a área (largura × altura)", "self.largura * self.altura", "this.largura * this.altura", a => a[0] * a[1]), new Meth("perimetro", "Perímetro", "o perímetro (2 × (largura + altura))", "2 * (self.largura + self.altura)", "2 * (this.largura + this.altura)", a => 2 * (a[0] + a[1])) }),
        new("Caixa", new[] { "comprimento", "largura", "altura" }, new[] { new Meth("volume", "Volume", "o volume (comprimento × largura × altura)", "self.comprimento * self.largura * self.altura", "this.comprimento * this.largura * this.altura", a => a[0] * a[1] * a[2]), new Meth("superficie", "Superfície", "a área da superfície (2 × (c·l + c·a + l·a))", "2 * (self.comprimento * self.largura + self.comprimento * self.altura + self.largura * self.altura)", "2 * (this.comprimento * this.largura + this.comprimento * this.altura + this.largura * this.altura)", a => 2 * (a[0] * a[1] + a[0] * a[2] + a[1] * a[2])) }),
        new("Produto", new[] { "preco", "quantidade" }, new[] { new Meth("total", "Total", "o total (preço × quantidade)", "self.preco * self.quantidade", "this.preco * this.quantidade", a => a[0] * a[1]), new Meth("com_desconto", "Com desconto", "o total menos 10% dele, com divisão inteira (total − total ÷ 10)", "self.total() - self.total() // 10", "this.total() - Math.trunc(this.total() / 10)", a => a[0] * a[1] - a[0] * a[1] / 10) }),
        new("Aluno", new[] { "nota1", "nota2", "nota3" }, new[] { new Meth("soma", "Soma", "a soma das três notas", "self.nota1 + self.nota2 + self.nota3", "this.nota1 + this.nota2 + this.nota3", a => a[0] + a[1] + a[2]), new Meth("media", "Média", "a média inteira (soma ÷ 3, divisão inteira)", "self.soma() // 3", "Math.trunc(this.soma() / 3)", a => (a[0] + a[1] + a[2]) / 3) }),
    };

    static string ClassDef(Cls c, bool js)
    {
        string ps = string.Join(", ", c.Fields);
        if (js)
            return $"class {c.Name} {{\n  constructor({ps}) {{\n" + string.Join("\n", c.Fields.Select(f => $"    this.{f} = {f};")) + "\n  }\n"
                + string.Join("\n", c.Methods.Select(m => $"  {m.Name}() {{\n    return {m.JsExpr};\n  }}")) + "\n}";
        return $"class {c.Name}:\n    def __init__(self, {ps}):\n" + string.Join("\n", c.Fields.Select(f => $"        self.{f} = {f}")) + "\n"
            + string.Join("\n", c.Methods.Select(m => $"    def {m.Name}(self):\n        return {m.PyExpr}"));
    }

    static Exercise MakeCode(bool js, int lv, Random r)
    {
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        switch (lv)
        {
            case 0:
            {
                bool distinct = r.Next(2) == 0;
                string[] pool = Pg.Words.OrderBy(_ => r.Next()).Take(4).ToArray();
                var ins = new List<string> { "5\nsol lua sol mar sol\nsol", "3\na b c\nz" };
                for (int i = 0; i < 3; i++) { int n = Ex.Int(r, 2, 8); var ws = Enumerable.Range(0, n).Select(_ => Ex.Pick(r, pool)).ToArray(); ins.Add($"{n}\n{string.Join(" ", ws)}\n{Ex.Pick(r, pool.Append("zzz").ToArray())}"); }
                var tests = Pg.Tests(ins, i => { var p = i.TrimEnd('\n').Split('\n'); var ws = p[1].Split(' '); return distinct ? $"Distintas: {ws.Distinct().Count()}\nVezes: {ws.Count(w => w == p[2])}" : $"Vezes: {ws.Count(w => w == p[2])}"; });
                var lines = new List<string> { Pg.ReadInt(js, "n"), js ? "const ws = prompt().split(\" \");" : "ws = input().split()", Pg.ReadStr(js, "q"),
                    js ? "const cont = {};" : "cont = {}", js ? "for (const w of ws) {" : "for w in ws:", js ? "  cont[w] = (cont[w] ?? 0) + 1;" : "    cont[w] = cont.get(w, 0) + 1" };
                if (js) lines.Add("}");
                if (distinct) lines.Add(Pg.Pr(js, "Distintas: {" + (js ? "Object.keys(cont).length" : "len(cont)") + "}"));
                lines.Add(Pg.Pr(js, "Vezes: {" + (js ? "cont[q] ?? 0" : "cont.get(q, 0)") + "}"));
                return Ex.Code(Pg.Prompt((distinct ? L.T("Leia um inteiro `n`, depois uma linha com `n` palavras separadas por espaço e, na linha seguinte, uma palavra de consulta. Conte as palavras usando um dicionário (palavra → quantidade) e mostre quantas palavras DISTINTAS há e quantas vezes a palavra de consulta aparece (0 se não aparece):") : L.T("Leia um inteiro `n`, depois uma linha com `n` palavras separadas por espaço e, na linha seguinte, uma palavra de consulta. Conte as palavras usando um dicionário (palavra → quantidade) e mostre quantas vezes a palavra de consulta aparece (0 se não aparece):")) + "\n"
                    + Pg.Plain((distinct ? "Distintas: <quantidade>\n" : "") + "Vezes: <vezes>"), tests[0]),
                    Pg.Code(Pg.ReadInt(js, "n"), js ? "const ws = prompt().split(\" \");" : "ws = input().split()", Pg.ReadStr(js, "q"), Pg.Comment(js, L.T("conte as palavras num dicionário"))), Pg.Code(lines.ToArray()), tests,
                    L.T("O dicionário guarda palavra → quantidade. Para cada palavra, somamos 1 ao valor que já existe (ou a 0, se for a primeira vez). A consulta usa o mesmo \"valor ou 0\"."));
            }
            case 1:
            {
                var c = Ex.Pick(r, Classes);
                var methods = c.Methods;
                var ins = Enumerable.Range(0, 5).Select(_ => string.Join("\n", c.Fields.Select(__ => Ex.Int(r, 1, 20)))).ToList();
                var tests = Pg.Tests(ins, i => { var a = i.TrimEnd('\n').Split('\n').Select(long.Parse).ToArray(); return string.Join("\n", methods.Select(m => $"{m.Label}: {m.F(a)}")); });
                var lines = new List<string> { ClassDef(c, js) };
                lines.AddRange(c.Fields.Select(f => Pg.ReadInt(js, f)));
                lines.Add(js ? $"const obj = new {c.Name}({string.Join(", ", c.Fields)});" : $"obj = {c.Name}({string.Join(", ", c.Fields)})");
                lines.AddRange(methods.Select(m => Pg.Pr(js, m.Label + ": {obj." + m.Name + "()}")));
                string desc = string.Join(" ", methods.Select(m => "`" + m.Name + "()` " + L.F("devolve {0};", L.T(m.Desc))));
                return Ex.Code(Pg.Prompt(L.F("Crie a classe `{0}` com um construtor que recebe {1} e guarda cada valor num atributo, e dois métodos sem parâmetros: {2} Depois leia os valores (inteiros, um por linha, na mesma ordem), crie um objeto e mostre uma linha para cada método:", c.Name, string.Join(", ", c.Fields.Select(f => "`" + f + "`")), desc)
                    + "\n" + Pg.Plain(string.Join("\n", methods.Select(m => m.Label + ": <valor>"))), tests[0]),
                    Pg.Code(c.Fields.Select(f => Pg.ReadInt(js, f)).Append(Pg.Comment(js, L.T("crie a classe antes de usá-la e mostre os resultados"))).ToArray()), Pg.Code(lines.ToArray()), tests,
                    L.T("O construtor guarda cada valor num atributo do objeto (self em Python, this em JavaScript). Cada método usa os atributos para calcular e devolve o resultado; o programa cria o objeto e chama os métodos."));
            }
            case 2:
            {
                string[] items = Pg.Words.OrderBy(_ => r.Next()).Take(3).ToArray();
                string Model(string inp)
                {
                    var ls = inp.TrimEnd('\n').Split('\n'); var d = new SortedDictionary<string, int>(StringComparer.Ordinal); var o = new List<string>();
                    foreach (var l in ls.Skip(1))
                    {
                        var p = l.Split(' '); int q = int.Parse(p[2]); d.TryGetValue(p[1], out int cur);
                        if (p[0] == "entrada") d[p[1]] = cur + q; else if (cur >= q) d[p[1]] = cur - q; else o.Add("Falta " + p[1]);
                    }
                    var left = d.Where(kv => kv.Value > 0).Select(kv => $"{kv.Key}: {kv.Value}").ToList();
                    o.AddRange(left.Count == 0 ? new List<string> { "Vazio" } : left);
                    return string.Join("\n", o);
                }
                var ins = new List<string>();
                ins.Add($"3\nentrada {items[0]} 5\nsaida {items[0]} 7\nsaida {items[1]} 1");
                ins.Add($"2\nentrada {items[1]} 3\nsaida {items[1]} 3");
                for (int i = 0; i < 3; i++)
                {
                    int n = Ex.Int(r, 3, 8);
                    ins.Add(n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => $"{Ex.Pick(r, "entrada", "entrada", "saida")} {Ex.Pick(r, items)} {Ex.Int(r, 1, 9)}")));
                }
                var tests = Pg.Tests(ins, Model);
                var sol = js
                    ? Pg.Code(Pg.ReadInt(true, "n"), "const est = {};", "for (let i = 0; i < n; i++) {", "  const [op, nome, q] = prompt().split(\" \");", "  const qtd = Number(q);", "  const atual = est[nome] ?? 0;", "  if (op === \"entrada\") {", "    est[nome] = atual + qtd;", "  } else if (atual >= qtd) {", "    est[nome] = atual - qtd;", "  } else {", "    console.log(`Falta ${nome}`);", "  }", "}",
                        "const nomes = Object.keys(est).filter((k) => est[k] > 0).sort();", "if (nomes.length === 0) console.log(\"Vazio\");", "for (const k of nomes) console.log(`${k}: ${est[k]}`);")
                    : Pg.Code(Pg.ReadInt(false, "n"), "est = {}", "for i in range(n):", "    op, nome, q = input().split()", "    q = int(q)", "    atual = est.get(nome, 0)", "    if op == \"entrada\":", "        est[nome] = atual + q", "    elif atual >= q:", "        est[nome] = atual - q", "    else:", "        print(\"Falta \" + nome)",
                        "nomes = sorted(k for k in est if est[k] > 0)", "if len(nomes) == 0:", "    print(\"Vazio\")", "for k in nomes:", "    print(k + \": \" + str(est[k]))");
                return Ex.Code(Pg.Prompt(L.T("Controle de estoque. Leia `n` e depois `n` comandos, um por linha, no formato `<operação> <item> <quantidade>`. `entrada` soma a quantidade ao item; `saida` tira, mas só se houver o suficiente — senão mostre `Falta <item>` na hora e não mude nada. No fim mostre, em ordem alfabética, uma linha `item: quantidade` para cada item com quantidade maior que zero (ou `Vazio` se não sobrou nenhum)."), tests[0]),
                    Pg.Code(Pg.ReadInt(js, "n"), Pg.Comment(js, L.T("leia os n comandos e guarde as quantidades num dicionário"))), sol, tests,
                    L.T("O dicionário item → quantidade guarda o estoque. Em cada comando: entrada soma; saída confere se a quantidade atual (ou 0) basta antes de subtrair. No fim ordenamos os nomes e mostramos só quem tem estoque."));
            }
            default:
            {
                string Model(string inp)
                {
                    var ls = inp.TrimEnd('\n').Split('\n'); int lim = int.Parse(ls[0]); int saldo = 0, ops = 0; var o = new List<string>();
                    foreach (var l in ls.Skip(2))
                    {
                        var p = l.Split(' '); int v = int.Parse(p[1]);
                        if (p[0] == "d") { saldo += v; ops++; }
                        else if (saldo + lim >= v) { saldo -= v; ops++; }
                        else o.Add("Negado " + v);
                    }
                    o.Add("Saldo: " + saldo); o.Add("Operações: " + ops);
                    return string.Join("\n", o);
                }
                var ins = new List<string> { "50\n3\nd 30\ns 100\ns 10", "0\n2\nd 5\ns 6" };
                for (int i = 0; i < 3; i++)
                {
                    int n = Ex.Int(r, 3, 8);
                    ins.Add(Ex.Pick(r, 0, 50, 100, 200) + "\n" + n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => $"{Ex.Pick(r, "d", "s", "s")} {Ex.Int(r, 10, 150)}")));
                }
                var tests = Pg.Tests(ins, Model);
                var sol = js
                    ? Pg.Code("class Conta {", "  constructor(limite) {", "    this.saldo = 0;", "    this.limite = limite;", "    this.ops = 0;", "  }", "  depositar(v) {", "    this.saldo += v;", "    this.ops++;", "  }", "  sacar(v) {", "    if (this.saldo + this.limite < v) return false;", "    this.saldo -= v;", "    this.ops++;", "    return true;", "  }", "}",
                        Pg.ReadInt(true, "limite"), Pg.ReadInt(true, "n"), "const c = new Conta(limite);", "for (let i = 0; i < n; i++) {", "  const [op, txt] = prompt().split(\" \");", "  const v = Number(txt);", "  if (op === \"d\") {", "    c.depositar(v);", "  } else if (!c.sacar(v)) {", "    console.log(`Negado ${v}`);", "  }", "}", "console.log(`Saldo: ${c.saldo}`);", "console.log(`Operações: ${c.ops}`);")
                    : Pg.Code("class Conta:", "    def __init__(self, limite):", "        self.saldo = 0", "        self.limite = limite", "        self.ops = 0", "    def depositar(self, v):", "        self.saldo += v", "        self.ops += 1", "    def sacar(self, v):", "        if self.saldo + self.limite < v:", "            return False", "        self.saldo -= v", "        self.ops += 1", "        return True",
                        Pg.ReadInt(false, "limite"), Pg.ReadInt(false, "n"), "c = Conta(limite)", "for i in range(n):", "    op, txt = input().split()", "    v = int(txt)", "    if op == \"d\":", "        c.depositar(v)", "    elif not c.sacar(v):", "        print(\"Negado \" + str(v))", "print(\"Saldo: \" + str(c.saldo))", "print(\"Operações: \" + str(c.ops))");
                return Ex.Code(Pg.Prompt(L.T("Conta com cheque especial. Crie a classe `Conta` com o limite do cheque especial no construtor; o saldo começa em 0. `depositar(v)` soma `v`. `sacar(v)` só tira o dinheiro se `saldo + limite` for pelo menos `v` (o saldo pode ficar negativo, até −limite) e devolve verdadeiro; senão não muda nada e devolve falso. Leia o limite, depois `n` e `n` comandos (`d <valor>` para depositar, `s <valor>` para sacar). Para cada saque negado mostre `Negado <valor>`. No fim mostre `Saldo: <saldo>` e `Operações: <quantas operações deram certo>`."), tests[0]),
                        Pg.Code(Pg.ReadInt(js, "limite"), Pg.ReadInt(js, "n"), Pg.Comment(js, L.T("escreva a classe e leia os comandos"))), sol, tests,
                        L.T("O estado (saldo, limite, contador de operações) mora dentro do objeto. O método sacar decide sozinho se pode e AVISA com o valor devolvido; quem chama usa esse retorno para mostrar \"Negado\". O limite entra na regra saldo + limite ≥ valor."));
            }
        }
    }

    static Exercise MakeOut(bool js, int lv, Random r)
    {
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        switch (lv)
        {
            case 0:
            {
                string[] pool = Pg.Words.OrderBy(_ => r.Next()).Take(3).ToArray();
                var ws = Enumerable.Range(0, Ex.Int(r, 5, 8)).Select(_ => Ex.Pick(r, pool)).ToArray();
                var cnt = ws.GroupBy(w => w).ToDictionary(g => g.Key, g => g.Count());
                string q = ws[0];
                string list = string.Join(", ", ws.Select(w => $"\"{w}\""));
                var code = new List<string> { js ? "const d = {};" : "d = {}", js ? $"for (const w of [{list}]) {{\n  d[w] = (d[w] ?? 0) + 1;\n}}" : $"for w in [{list}]:\n    d[w] = d.get(w, 0) + 1" };
                code.Add(pr(js ? "Object.keys(d).length" : "len(d)")); code.Add(pr($"d[\"{q}\"]"));
                code.Add(pr(js ? "Object.keys(d).sort().join(\" \")" : "\" \".join(sorted(d))"));
                string outp = $"{cnt.Count}\n{cnt[q]}\n{string.Join(" ", cnt.Keys.OrderBy(k => k, StringComparer.Ordinal))}";
                return Ex.Output(PgOut.Prompt(outp), string.Join("\n", code), outp, L.T("O dicionário conta quantas vezes cada palavra aparece: as chaves são as palavras DIFERENTES. len dá o número de chaves; sorted/sort as coloca em ordem alfabética."));
            }
            case 1:
            case 2:
            {
                int n0 = Ex.Int(r, 1, 9), k1 = Ex.Int(r, 1, 5), k2 = Ex.Int(r, 2, 6), n1 = Ex.Int(r, 1, 9);
                bool chain = lv == 2;
                string def = js
                    ? $"class Cont {{\n  constructor(n) {{\n    this.n = n;\n  }}\n  soma(k) {{\n    this.n += k;\n    return {(chain ? "this" : "this.n")};\n  }}\n  dobro() {{\n    return this.n * 2;\n  }}\n}}"
                    : $"class Cont:\n    def __init__(self, n):\n        self.n = n\n    def soma(self, k):\n        self.n += k\n        return {(chain ? "self" : "self.n")}\n    def dobro(self):\n        return self.n * 2";
                var code = new List<string> { def, js ? $"const a = new Cont({n0});" : $"a = Cont({n0})", js ? $"const b = new Cont({n1});" : $"b = Cont({n1})" };
                var outs = new List<string>();
                long a = n0, b = n1;
                if (!chain)
                {
                    a += k1; code.Add(pr($"a.soma({k1})")); outs.Add(a.ToString());
                    b += k2; code.Add(pr($"b.soma({k2})")); outs.Add(b.ToString());
                    code.Add(pr("a.dobro()")); outs.Add((a * 2).ToString());
                    code.Add(pr("a.n + b.n")); outs.Add((a + b).ToString());
                }
                else
                {
                    a += k1; a += k2; code.Add(pr($"a.soma({k1}).soma({k2}).n")); outs.Add(a.ToString());
                    b += k1; code.Add($"b.soma({k1})" + (js ? ";" : "")); code.Add(pr("b.dobro()")); outs.Add((b * 2).ToString());
                    code.Add(js ? "const c = a;" : "c = a"); code.Add($"c.soma({k2})" + (js ? ";" : "")); a += k2;
                    code.Add(pr("a.n")); outs.Add(a.ToString());
                    code.Add(pr("b.n")); outs.Add(b.ToString());
                }
                string outp = string.Join("\n", outs);
                return Ex.Output(PgOut.Prompt(outp), string.Join("\n", code), outp,
                    chain ? L.T("Cada objeto tem o seu próprio n. Como soma devolve o próprio objeto (self / this), as chamadas podem ser encadeadas: a.soma(x).soma(y) altera `a` duas vezes. `c = a` não cria outro objeto: c e a são o mesmo.")
                          : L.T("Cada objeto tem o seu próprio n: mexer em a não muda b. O método soma altera o atributo e devolve o novo valor."));
            }
            default:
            {
                var keys = new[] { "a", "b", "c" }.OrderBy(_ => r.Next()).ToArray();
                var d = new Dictionary<string, int> { [keys[0]] = Ex.Int(r, 1, 9), [keys[1]] = Ex.Int(r, 1, 9) };
                string miss = keys[2]; int extra = Ex.Int(r, 10, 20), v0 = d[keys[0]];
                var code = new List<string> { js ? $"const d = {{ {keys[0]}: {d[keys[0]]}, {keys[1]}: {d[keys[1]]} }};" : $"d = {{\"{keys[0]}\": {d[keys[0]]}, \"{keys[1]}\": {d[keys[1]]}}}" };
                var outs = new List<string>();
                code.Add(pr(js ? $"d[\"{miss}\"] ?? 0" : $"d.get(\"{miss}\", 0)")); outs.Add("0");
                code.Add(pr(js ? $"\"{miss}\" in d" : $"\"{miss}\" in d")); outs.Add(js ? "false" : "False");
                code.Add(pr(js ? $"\"{keys[0]}\" in d" : $"\"{keys[0]}\" in d")); outs.Add(js ? "true" : "True");
                code.Add(js ? "const e = d;" : "e = d");
                code.Add(js ? $"e[\"{miss}\"] = {extra};" : $"e[\"{miss}\"] = {extra}");
                code.Add(js ? $"e[\"{keys[0]}\"] += 1;" : $"e[\"{keys[0]}\"] += 1");
                code.Add(pr(js ? "Object.keys(d).length" : "len(d)")); outs.Add("3");
                code.Add(pr($"d[\"{keys[0]}\"] + d[\"{miss}\"]")); outs.Add((v0 + 1 + extra).ToString());
                code.Add(pr(js ? $"d[\"{miss}\"] ?? 0" : $"d.get(\"{miss}\", 0)")); outs.Add(extra.ToString());
                string outp = string.Join("\n", outs);
                return Ex.Output(PgOut.Prompt(outp), string.Join("\n", code), outp,
                    L.T("get com padrão (ou ?? 0) evita erro quando a chave não existe; `in` só olha se a chave existe. `e = d` não copia o dicionário: e e d são o MESMO dicionário, então as mudanças feitas por `e` aparecem em `d`."));
            }
        }
    }
}
