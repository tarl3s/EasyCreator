namespace EasyCreator.Exercises;

/// <summary>Lessons py-15 / js-12 (closures; decorators and memoization / scope and closures): function factories, counters, memoization, and tracing what a closure remembers.</summary>
[Exercises("py-15", "js-12")]
public sealed class P13Closures : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        bool js = Pg.Js(lesson);
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(js, lv, r) : MakeOut(js, lv, r);
    }

    record Fac(string Py, string Js, string Desc, string PyBody, string JsBody, Func<long, long, long> F);

    static readonly Fac[] Facs =
    {
        new("criar_multiplicador", "criarMultiplicador", "multiplica o seu argumento por `k`", "x * k", "x * k", (k, x) => k * x),
        new("criar_somador", "criarSomador", "soma `k` ao seu argumento", "x + k", "x + k", (k, x) => k + x),
        new("criar_potencia", "criarPotencia", "eleva o seu argumento ao expoente `k`", "x ** k", "x ** k", (k, x) => (long)Math.Pow(x, k)),
        new("criar_resto", "criarResto", "devolve o resto da divisão do seu argumento por `k`", "x % k", "x % k", (k, x) => x % k),
    };

    static Exercise MakeCode(bool js, int lv, Random r)
    {
        switch (lv)
        {
            case 0:
            {
                var f = Ex.Pick(r, Facs);
                var ins = Enumerable.Range(0, 5).Select(_ => $"{(f.Py == "criar_potencia" ? Ex.Int(r, 1, 4) : Ex.Int(r, 2, 9))}\n{(f.Py == "criar_potencia" ? Ex.Int(r, 1, 9) : Ex.Int(r, 1, 60))}");
                var tests = Pg.Tests(ins, i => { var p = i.TrimEnd('\n').Split('\n').Select(long.Parse).ToArray(); return f.F(p[0], p[1]).ToString(); });
                string name = js ? f.Js : f.Py, rd = Pg.Code(Pg.ReadInt(js, "k"), Pg.ReadInt(js, "x"));
                string sol = js ? Pg.Code($"function {name}(k) {{", $"  return (x) => {f.JsBody};", "}", rd, $"const f = {name}(k);", "console.log(f(x));")
                    : Pg.Code($"def {name}(k):", $"    def interna(x):", $"        return {f.PyBody}", "    return interna", rd, $"f = {name}(k)", "print(f(x))");
                return Ex.Code(Pg.Prompt(L.F("Escreva a função fábrica `{0}(k)`, que DEVOLVE uma função de um argumento `x`; a função devolvida {1}. Depois leia `k` e `x` (um por linha), crie a função com `{0}(k)` e mostre o resultado de aplicá-la a `x`.", name, L.T(f.Desc)), tests[0]),
                    rd + "\n" + Pg.Comment(js, L.T("escreva a fábrica de funções e use-a")), sol, tests,
                    L.T("A função de dentro \"lembra\" o k da fábrica mesmo depois de a fábrica ter terminado: isso é um closure. Cada chamada da fábrica cria uma função nova com o seu próprio k."));
            }
            case 1:
            {
                var ins = new List<string> { "5\n3\n4", "0\n1\n1" };
                for (int i = 0; i < 4; i++) ins.Add($"{Ex.Int(r, -5, 20)}\n{Ex.Int(r, 1, 7)}\n{Ex.Int(r, 1, 7)}");
                var tests = Pg.Tests(ins, i => { var p = i.TrimEnd('\n').Split('\n').Select(int.Parse).ToArray(); return string.Join("\n", Enumerable.Range(0, p[2]).Select(k => (p[0] + k * p[1]).ToString())); });
                string rd = Pg.Code(Pg.ReadInt(js, "inicio"), Pg.ReadInt(js, "passo"), Pg.ReadInt(js, "n"));
                string sol = js ? Pg.Code("function criarContador(inicio, passo) {", "  let atual = inicio - passo;", "  return () => {", "    atual += passo;", "    return atual;", "  };", "}", rd, "const c = criarContador(inicio, passo);", "for (let i = 0; i < n; i++) console.log(c());")
                    : Pg.Code("def criar_contador(inicio, passo):", "    atual = inicio - passo", "    def proximo():", "        nonlocal atual", "        atual += passo", "        return atual", "    return proximo", rd, "c = criar_contador(inicio, passo)", "for i in range(n):", "    print(c())");
                return Ex.Code(Pg.Prompt(L.T("Escreva a função `criar_contador(inicio, passo)` (em JavaScript, `criarContador`) que devolve uma função SEM argumentos; a primeira chamada dela devolve `inicio`, a segunda `inicio + passo`, a terceira `inicio + 2·passo`, e assim por diante (a função guarda o seu próprio estado). Leia `inicio`, `passo` e `n` (um por linha) e mostre os `n` primeiros valores devolvidos, um por linha."), tests[0]),
                    rd + "\n" + Pg.Comment(js, L.T("escreva a função e chame o contador n vezes")), sol, tests,
                    js ? L.T("A variável `atual` mora na fábrica e a função devolvida a altera a cada chamada; o estado sobrevive entre as chamadas.") : L.T("A variável `atual` mora na fábrica; para ALTERAR (e não só ler) uma variável de fora, a função de dentro precisa declarar `nonlocal atual`."));
            }
            case 2:
            {
                int a = Ex.Int(r, 1, 5), b = Ex.Int(r, 1, 9);
                string Model(string inp) { var ls = inp.TrimEnd('\n').Split('\n'); var seen = new HashSet<long>(); var o = new List<string>(); int c = 0; foreach (var s in ls.Skip(1)) { long x = long.Parse(s); if (seen.Add(x)) c++; o.Add((x * x * a + b).ToString()); } o.Add("Cálculos: " + c); return string.Join("\n", o); }
                var ins = new List<string> { "5\n3\n4\n3\n3\n4" };
                for (int i = 0; i < 4; i++) { int n = Ex.Int(r, 3, 8); ins.Add(n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => Ex.Int(r, 1, 5)))); }
                var tests = Pg.Tests(ins, Model);
                string rd = Pg.ReadInt(js, "n");
                string sol = js ? Pg.Code("let calculos = 0;", "function pesado(x) {", "  calculos++;", $"  return x * x * {a} + {b};", "}", "function memoriza(f) {", "  const guardado = new Map();", "  return (x) => {", "    if (!guardado.has(x)) guardado.set(x, f(x));", "    return guardado.get(x);", "  };", "}", "const rapido = memoriza(pesado);", rd, "for (let i = 0; i < n; i++) console.log(rapido(Number(prompt())));", "console.log(`Cálculos: ${calculos}`);")
                    : Pg.Code("calculos = [0]", "def pesado(x):", "    calculos[0] += 1", $"    return x * x * {a} + {b}", "def memoriza(f):", "    guardado = {}", "    def nova(x):", "        if x not in guardado:", "            guardado[x] = f(x)", "        return guardado[x]", "    return nova", "rapido = memoriza(pesado)", rd, "for i in range(n):", "    print(rapido(int(input())))", "print(\"Cálculos: \" + str(calculos[0]))");
                string given = js ? Pg.Code("let calculos = 0;", "function pesado(x) {", "  calculos++;", $"  return x * x * {a} + {b};", "}") : Pg.Code("calculos = [0]", "def pesado(x):", "    calculos[0] += 1", $"    return x * x * {a} + {b}");
                return Ex.Code(Pg.Prompt(L.T("O código inicial já tem a função `pesado(x)` (que conta quantas vezes foi realmente executada). Escreva `memoriza(f)`, que recebe uma função e devolve outra que lembra os resultados já calculados: para um `x` repetido, devolve o resultado guardado SEM chamar `f` de novo. Leia `n` e depois `n` inteiros; para cada um mostre `rapido(x)`, onde `rapido = memoriza(pesado)`, e no fim `Cálculos: <quantas vezes pesado rodou>`."), tests[0]),
                    given + "\n" + rd + "\n" + Pg.Comment(js, L.T("escreva memoriza e use-a para ler e mostrar os n valores")), sol, tests,
                    L.T("A função devolvida guarda um dicionário (ou Map) de resultados dentro do closure. Se o argumento já está lá, devolve o guardado; senão chama f, guarda e devolve. Assim o contador só sobe para argumentos novos."));
            }
            default:
            {
                string Model(string inp) { var ls = inp.TrimEnd('\n').Split('\n'); int n = int.Parse(ls[0]); long x = long.Parse(ls[n + 1]); var vals = new List<long> { x }; for (int i = 1; i <= n; i++) { var p = ls[i].Split(' '); long k = long.Parse(p[1]); x = p[0] == "+" ? x + k : p[0] == "-" ? x - k : x * k; vals.Add(x); } return string.Join(" ", vals) + "\nFinal: " + x; }
                var ins = new List<string> { "3\n+ 3\n* 2\n- 1\n5" };
                for (int i = 0; i < 4; i++) { int n = Ex.Int(r, 2, 5); ins.Add(n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => $"{Ex.Pick(r, "+", "-", "*")} {Ex.Int(r, 1, 6)}")) + "\n" + Ex.Int(r, -3, 9)); }
                var tests = Pg.Tests(ins, Model);
                string rd = Pg.ReadInt(js, "n");
                string sol = js ? Pg.Code("function criar(op, k) {", "  if (op === \"+\") return (x) => x + k;", "  if (op === \"-\") return (x) => x - k;", "  return (x) => x * k;", "}", rd, "const fs = [];", "for (let i = 0; i < n; i++) {", "  const [op, txt] = prompt().split(\" \");", "  fs.push(criar(op, Number(txt)));", "}", "let x = Number(prompt());", "const vals = [x];", "for (const f of fs) {", "  x = f(x);", "  vals.push(x);", "}", "console.log(vals.join(\" \"));", "console.log(`Final: ${x}`);")
                    : Pg.Code("def criar(op, k):", "    if op == \"+\":", "        return lambda x: x + k", "    if op == \"-\":", "        return lambda x: x - k", "    return lambda x: x * k", rd, "fs = []", "for i in range(n):", "    op, txt = input().split()", "    fs.append(criar(op, int(txt)))", "x = int(input())", "vals = [x]", "for f in fs:", "    x = f(x)", "    vals.append(x)", "print(\" \".join(str(v) for v in vals))", "print(\"Final: \" + str(x))");
                return Ex.Code(Pg.Prompt(L.T("Leia `n`, depois `n` operações, uma por linha, no formato `<sinal> <número>` com o sinal `+`, `-` ou `*`, e por fim um valor inicial `x`. Transforme cada operação numa FUNÇÃO (guardada numa lista) e aplique-as em ordem sobre `x`. Mostre numa linha o valor inicial e o valor depois de cada operação, separados por espaço, e na linha seguinte `Final: <valor final>`."), tests[0]),
                    rd + "\n" + Pg.Comment(js, L.T("escreva o programa: uma função por operação, guardadas numa lista")), sol, tests,
                    L.T("A fábrica `criar(op, k)` devolve uma função que lembra o seu próprio `op` e `k`. Criar as funções por uma fábrica (e não diretamente dentro do laço) evita o erro clássico de todas as funções acabarem enxergando o mesmo valor da última volta."));
            }
        }
    }

    static Exercise MakeOut(bool js, int lv, Random r)
    {
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        if (lv <= 1)
        {
            int s1 = Ex.Int(r, 0, 5), s2 = Ex.Int(r, 10, 20), st = Ex.Int(r, 1, 4);
            string code = js ? $"function criar(n) {{\n  return () => {{\n    n += {st};\n    return n;\n  }};\n}}\nconst a = criar({s1});\nconst b = criar({s2});\n" : $"def criar(n):\n    def prox():\n        nonlocal n\n        n += {st}\n        return n\n    return prox\na = criar({s1})\nb = criar({s2})\n";
            var seq = new List<string> { "a", "a", "b", "a", "b" }; long va = s1, vb = s2; var outs = new List<string>();
            foreach (var c in seq) { if (c == "a") { va += st; outs.Add(va.ToString()); } else { vb += st; outs.Add(vb.ToString()); } }
            code += string.Join("\n", seq.Select(c => pr($"{c}()")));
            string outp = string.Join("\n", outs);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("Cada chamada de `criar` cria um closure com o seu PRÓPRIO n: a e b guardam contagens independentes, e cada chamada soma o passo ao n do contador chamado."));
        }
        if (lv == 2)
        {
            int k = Ex.Int(r, 1, 9), x = Ex.Int(r, 1, 9), y = Ex.Int(r, 1, 9);
            string code = js ? $"function log(f) {{\n  return (x) => {{\n    console.log(\"chama\", x);\n    const r = f(x);\n    console.log(\"volta\", r);\n    return r;\n  }};\n}}\nconst g = log((x) => x + {k});\nconst h = log(g);\nconsole.log(g({x}));\nconsole.log(h({y}));"
                : $"def log(f):\n    def nova(x):\n        print(\"chama\", x)\n        r = f(x)\n        print(\"volta\", r)\n        return r\n    return nova\ng = log(lambda x: x + {k})\nh = log(g)\nprint(g({x}))\nprint(h({y}))";
            var outs = new List<string> { $"chama {x}", $"volta {x + k}", (x + k).ToString(), $"chama {y}", $"chama {y}", $"volta {y + k}", $"volta {y + k}", (y + k).ToString() };
            string outp = string.Join("\n", outs);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("`log(f)` devolve uma função que embrulha f: escreve \"chama\", chama f, escreve \"volta\" e devolve o resultado. h embrulha g, que já é embrulhada: as mensagens aparecem em camadas, de fora para dentro e de volta."));
        }
        {
            int a0 = Ex.Int(r, 1, 5), b0 = Ex.Int(r, 6, 9);
            string code = js ? $"function par() {{\n  let n = 0;\n  const inc = () => ++n;\n  const get = () => n;\n  return [inc, get];\n}}\nconst [i1, g1] = par();\nconst [i2, g2] = par();\n"
                : "def par():\n    n = 0\n    def inc():\n        nonlocal n\n        n += 1\n        return n\n    def get():\n        return n\n    return inc, get\ni1, g1 = par()\ni2, g2 = par()\n";
            code += string.Join("\n", Enumerable.Range(0, a0).Select(_ => pr("i1()").Replace("print(i1())", "i1()").Replace("console.log(i1());", "i1();"))) + "\n"
                + string.Join("\n", Enumerable.Range(0, 2).Select(_ => js ? "i2();" : "i2()")) + "\n" + pr("g1()") + "\n" + pr("g2()") + "\n" + pr("i1() + i2()");
            string outp = $"{a0}\n2\n{a0 + 1 + 3}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("Cada chamada de par() cria um novo n e duas funções que COMPARTILHAM esse n (inc muda, get lê). Os pares (i1, g1) e (i2, g2) têm contagens separadas. A última linha soma o resultado de duas chamadas: i1() e i2()."));
        }
    }
}
