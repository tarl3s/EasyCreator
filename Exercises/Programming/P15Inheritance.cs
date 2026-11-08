namespace EasyCreator.Exercises;

/// <summary>Lessons py-14 / js-15 (inheritance and polymorphism): class hierarchies that read commands, and tracing overridden methods, super() chains and constructor order.</summary>
[Exercises("py-14", "js-15")]
public sealed class P15Inheritance : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        bool js = Pg.Js(lesson);
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(js, lv, r) : MakeOut(js, lv, r);
    }

    static readonly (string Kind, string Cls, string Sound)[] Animals =
    {
        ("cachorro", "Cachorro", "au au"), ("gato", "Gato", "miau"), ("vaca", "Vaca", "muu"), ("pato", "Pato", "quack"), ("galo", "Galo", "cocorico"),
    };

    static string Cls(bool js, string name, string? parent, string[] fields, string[] pyMethods, string[] jsMethods, string[]? ctor = null, string[]? jsCtor = null)
    {
        if (js)
        {
            var sb = new System.Text.StringBuilder($"class {name}{(parent != null ? " extends " + parent : "")} {{\n");
            if (jsCtor != null) sb.Append("  constructor(" + string.Join(", ", fields) + ") {\n" + string.Join("\n", jsCtor.Select(l => "    " + l)) + "\n  }\n");
            sb.Append(string.Join("\n", jsMethods)).Append("\n}");
            return sb.ToString();
        }
        var p = new System.Text.StringBuilder($"class {name}{(parent != null ? "(" + parent + ")" : "")}:\n");
        if (ctor != null) p.Append("    def __init__(self" + string.Concat(fields.Select(f => ", " + f)) + "):\n" + string.Join("\n", ctor.Select(l => "        " + l)) + "\n");
        p.Append(string.Join("\n", pyMethods));
        return p.ToString();
    }

    static Exercise MakeCode(bool js, int lv, Random r)
    {
        switch (lv)
        {
            case 0:
            {
                var pick = Animals.OrderBy(_ => r.Next()).Take(3).ToArray();
                string Item() { var a = Ex.Pick(r, pick); return a.Kind + " " + Ex.Pick(r, Pg.Names); }
                var tests = Pg.Tests(new[] { $"{pick[0].Kind} Rex", Item(), Item(), Item() }, i => { var p = i.Trim().Split(' '); var a = pick.First(x => x.Kind == p[0]); return $"{p[1]} diz {a.Sound}"; });
                var defs = new List<string>();
                defs.Add(Cls(js, "Animal", null, new[] { "nome" }, new[] { "    def som(self):", "        return \"...\"", "    def apresenta(self):", "        return self.nome + \" diz \" + self.som()" }.Select(l => l).ToArray(),
                    new[] { "  som() {", "    return \"...\";", "  }", "  apresenta() {", "    return this.nome + \" diz \" + this.som();", "  }" }, new[] { "self.nome = nome" }, new[] { "this.nome = nome;" }));
                foreach (var a in pick) defs.Add(Cls(js, a.Cls, "Animal", Array.Empty<string>(), new[] { "    def som(self):", $"        return \"{a.Sound}\"" }, new[] { "  som() {", $"    return \"{a.Sound}\";", "  }" }));
                string map = js ? "const tipos = { " + string.Join(", ", pick.Select(a => $"{a.Kind}: {a.Cls}")) + " };" : "tipos = {" + string.Join(", ", pick.Select(a => $"\"{a.Kind}\": {a.Cls}")) + "}";
                string main = js ? Pg.Code(map, "const [tipo, nome] = prompt().split(\" \");", "console.log(new tipos[tipo](nome).apresenta());") : Pg.Code(map, "tipo, nome = input().split()", "print(tipos[tipo](nome).apresenta())");
                string sol = string.Join("\n", defs) + "\n" + main;
                string classes = string.Join(", ", pick.Select(a => $"`{a.Cls}` (`{a.Kind}`, \"{a.Sound}\")"));
                return Ex.Code(Pg.Prompt(L.F("Crie a classe `Animal` com o nome no construtor e dois métodos: `som()`, que devolve `...`, e `apresenta()`, que devolve `<nome> diz <som()>`. Depois crie as classes filhas {0}, cada uma SOBRESCREVENDO só `som()` com o seu som. Leia uma linha `<tipo> <nome>` (tipo em minúsculas, como entre parênteses), crie o animal certo e mostre o que `apresenta()` devolve.", classes), tests[0]),
                    Pg.Comment(js, L.T("escreva as classes e leia a linha")), sol, tests,
                    L.T("`apresenta()` é escrito UMA vez, na classe mãe, e chama `som()`. Como cada filha sobrescreve `som()`, o mesmo código mostra textos diferentes conforme o tipo do objeto: isso é polimorfismo. Um dicionário/objeto liga o texto do tipo à classe."));
            }
            case 1:
            case 2:
            {
                bool emp = lv == 2;
                if (!emp)
                {
                    string Model(string l) { var p = l.Trim().Split(' '); int a = int.Parse(p[1]), b = p.Length > 2 ? int.Parse(p[2]) : a; string nome = p[0] == "quadrado" ? "Quadrado" : "Retangulo"; return $"{nome}: área {a * b}, perímetro {2 * (a + b)}"; }
                    string Item() => r.Next(2) == 0 ? $"retangulo {Ex.Int(r, 1, 12)} {Ex.Int(r, 1, 12)}" : $"quadrado {Ex.Int(r, 1, 12)}";
                    var tests = Pg.Tests(new[] { "retangulo 3 4", "quadrado 5", Item(), Item(), Item() }, Model);
                    string defs = js ? Pg.Code("class Retangulo {", "  constructor(l, a) {", "    this.l = l;", "    this.a = a;", "  }", "  area() {", "    return this.l * this.a;", "  }", "  perimetro() {", "    return 2 * (this.l + this.a);", "  }", "  descricao() {", "    return `${this.constructor.name}: área ${this.area()}, perímetro ${this.perimetro()}`;", "  }", "}", "class Quadrado extends Retangulo {", "  constructor(lado) {", "    super(lado, lado);", "  }", "}")
                        : Pg.Code("class Retangulo:", "    def __init__(self, l, a):", "        self.l = l", "        self.a = a", "    def area(self):", "        return self.l * self.a", "    def perimetro(self):", "        return 2 * (self.l + self.a)", "    def descricao(self):", "        return type(self).__name__ + \": área \" + str(self.area()) + \", perímetro \" + str(self.perimetro())", "class Quadrado(Retangulo):", "    def __init__(self, lado):", "        super().__init__(lado, lado)");
                    string main = js ? Pg.Code(Pg.ReadStr(true, "linha"), "const p = linha.split(\" \");", "const obj = p[0] === \"quadrado\" ? new Quadrado(Number(p[1])) : new Retangulo(Number(p[1]), Number(p[2]));", "console.log(obj.descricao());")
                        : Pg.Code(Pg.ReadStr(false, "linha"), "p = linha.split()", "obj = Quadrado(int(p[1])) if p[0] == \"quadrado\" else Retangulo(int(p[1]), int(p[2]))", "print(obj.descricao())");
                    return Ex.Code(Pg.Prompt(L.T("Crie a classe `Retangulo(l, a)` com os métodos `area()`, `perimetro()` e `descricao()`, que devolve `<NomeDaClasse>: área <área>, perímetro <perímetro>` (o nome da classe do próprio objeto). Crie `Quadrado(lado)` como FILHA de `Retangulo`, reaproveitando tudo (só o construtor muda). Leia uma linha: `retangulo <l> <a>` ou `quadrado <lado>`, crie o objeto e mostre a descrição."), tests[0]),
                        Pg.Comment(js, L.T("escreva as classes e leia a linha")), defs + "\n" + main, tests,
                        L.T("O quadrado é um retângulo de lados iguais: o construtor da filha chama o da mãe com (lado, lado) usando super, e herda area, perimetro e descricao sem repetir código. O nome impresso vem da classe real do objeto."));
                }
                else
                {
                    int bonus = Ex.Pick(r, 300, 500, 800), mult = Ex.Pick(r, 2, 3);
                    long Sal(string t, int b) => t == "funcionario" ? b : t == "gerente" ? b + bonus : (b + bonus) * mult;
                    string Item() => $"{Ex.Pick(r, "funcionario", "gerente", "diretor")} {Ex.Pick(r, Pg.Names)} {Ex.Int(r, 10, 40) * 100}";
                    string Model(string inp) { var ls = inp.TrimEnd('\n').Split('\n').Skip(1).Select(l => l.Split(' ')).ToList(); long tot = 0; var o = new List<string>(); foreach (var p in ls) { long s = Sal(p[0], int.Parse(p[2])); tot += s; o.Add($"{p[1]} ({p[0]}): R$ {s}"); } o.Add("Total: R$ " + tot); return string.Join("\n", o); }
                    var ins = new List<string> { "3\nfuncionario Ana 2000\ngerente Bia 3000\ndiretor Caio 5000" };
                    for (int i = 0; i < 4; i++) { int n = Ex.Int(r, 2, 5); ins.Add(n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => Item()))); }
                    var tests = Pg.Tests(ins, Model);
                    string defs = js ? Pg.Code("class Funcionario {", "  constructor(nome, base) {", "    this.nome = nome;", "    this.base = base;", "  }", "  salario() {", "    return this.base;", "  }", "}", "class Gerente extends Funcionario {", "  salario() {", $"    return super.salario() + {bonus};", "  }", "}", "class Diretor extends Gerente {", "  salario() {", $"    return super.salario() * {mult};", "  }", "}")
                        : Pg.Code("class Funcionario:", "    def __init__(self, nome, base):", "        self.nome = nome", "        self.base = base", "    def salario(self):", "        return self.base", "class Gerente(Funcionario):", "    def salario(self):", $"        return super().salario() + {bonus}", "class Diretor(Gerente):", "    def salario(self):", $"        return super().salario() * {mult}");
                    string main = js ? Pg.Code(Pg.ReadInt(true, "n"), "const tipos = { funcionario: Funcionario, gerente: Gerente, diretor: Diretor };", "let total = 0;", "for (let i = 0; i < n; i++) {", "  const [tipo, nome, base] = prompt().split(\" \");", "  const f = new tipos[tipo](nome, Number(base));", "  total += f.salario();", "  console.log(`${nome} (${tipo}): R$ ${f.salario()}`);", "}", "console.log(`Total: R$ ${total}`);")
                        : Pg.Code(Pg.ReadInt(false, "n"), "tipos = {\"funcionario\": Funcionario, \"gerente\": Gerente, \"diretor\": Diretor}", "total = 0", "for i in range(n):", "    tipo, nome, base = input().split()", "    f = tipos[tipo](nome, int(base))", "    total += f.salario()", "    print(nome + \" (\" + tipo + \"): R$ \" + str(f.salario()))", "print(\"Total: R$ \" + str(total))");
                    return Ex.Code(Pg.Prompt(L.F("Folha de pagamento. `Funcionario(nome, base)` tem `salario()` que devolve o salário-base. `Gerente` é filha de `Funcionario`: o salário é o da mãe MAIS {0}. `Diretor` é filha de `Gerente`: o salário é o do gerente multiplicado por {1}. Leia `n` e depois `n` linhas `<tipo> <nome> <base>` (tipo: `funcionario`, `gerente` ou `diretor`). Para cada uma mostre `<nome> (<tipo>): R$ <salário>` e, no fim, `Total: R$ <soma dos salários>`. Use `super` para reaproveitar o cálculo da classe mãe.", bonus, mult), tests[0]),
                        Pg.Code(Pg.ReadInt(js, "n"), Pg.Comment(js, L.T("escreva as classes e leia as linhas"))), defs + "\n" + main, tests,
                        L.F("Cada filha chama o cálculo da mãe com super e acrescenta a sua parte: gerente = base + {0}; diretor = (base + {0}) × {1}, porque o super do diretor já devolve o salário do gerente.", bonus, mult));
                }
            }
            default:
            {
                var shapes = new[] { ("quadrado", 1), ("retangulo", 2), ("triangulo", 2) };
                long Area(string t, int a, int b) => t == "quadrado" ? a * a : t == "retangulo" ? a * b : a * b / 2;
                string Item() { var t = Ex.Pick(r, "quadrado", "retangulo", "triangulo"); return t == "quadrado" ? $"{t} {Ex.Int(r, 1, 9)}" : $"{t} {Ex.Int(r, 1, 9)} {Ex.Int(r, 1, 9)}"; }
                string Model(string inp)
                {
                    var ls = inp.TrimEnd('\n').Split('\n').Skip(1).Select((l, idx) => { var p = l.Split(' '); int a = int.Parse(p[1]), b = p.Length > 2 ? int.Parse(p[2]) : a; return (idx, t: p[0], area: Area(p[0], a, b)); }).ToList();
                    var ord = ls.OrderByDescending(x => x.area).ThenBy(x => x.idx);
                    return string.Join("\n", ord.Select(x => $"{x.t} {x.area}")) + "\nTotal: " + ls.Sum(x => x.area);
                }
                var ins = new List<string> { "3\nquadrado 3\nretangulo 2 5\ntriangulo 4 5" };
                for (int i = 0; i < 4; i++) { int n = Ex.Int(r, 2, 6); ins.Add(n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => Item()))); }
                var tests = Pg.Tests(ins, Model);
                string defs = js ? Pg.Code("class Forma {", "  constructor(nome) {", "    this.nome = nome;", "  }", "  area() {", "    return 0;", "  }", "}", "class Quadrado extends Forma {", "  constructor(l) {", "    super(\"quadrado\");", "    this.l = l;", "  }", "  area() {", "    return this.l * this.l;", "  }", "}", "class Retangulo extends Forma {", "  constructor(l, a) {", "    super(\"retangulo\");", "    this.l = l;", "    this.a = a;", "  }", "  area() {", "    return this.l * this.a;", "  }", "}", "class Triangulo extends Forma {", "  constructor(b, h) {", "    super(\"triangulo\");", "    this.b = b;", "    this.h = h;", "  }", "  area() {", "    return Math.trunc((this.b * this.h) / 2);", "  }", "}")
                    : Pg.Code("class Forma:", "    def __init__(self, nome):", "        self.nome = nome", "    def area(self):", "        return 0", "class Quadrado(Forma):", "    def __init__(self, l):", "        super().__init__(\"quadrado\")", "        self.l = l", "    def area(self):", "        return self.l * self.l", "class Retangulo(Forma):", "    def __init__(self, l, a):", "        super().__init__(\"retangulo\")", "        self.l = l", "        self.a = a", "    def area(self):", "        return self.l * self.a", "class Triangulo(Forma):", "    def __init__(self, b, h):", "        super().__init__(\"triangulo\")", "        self.b = b", "        self.h = h", "    def area(self):", "        return self.b * self.h // 2");
                string main = js ? Pg.Code(Pg.ReadInt(true, "n"), "const lista = [];", "for (let i = 0; i < n; i++) {", "  const p = prompt().split(\" \");", "  const v = p.slice(1).map(Number);", "  if (p[0] === \"quadrado\") lista.push(new Quadrado(v[0]));", "  else if (p[0] === \"retangulo\") lista.push(new Retangulo(v[0], v[1]));", "  else lista.push(new Triangulo(v[0], v[1]));", "}", "const ord = lista.map((f, i) => [f, i]).sort((x, y) => y[0].area() - x[0].area() || x[1] - y[1]);", "for (const [f] of ord) console.log(`${f.nome} ${f.area()}`);", "console.log(`Total: ${lista.reduce((s, f) => s + f.area(), 0)}`);")
                    : Pg.Code(Pg.ReadInt(false, "n"), "lista = []", "for i in range(n):", "    p = input().split()", "    v = [int(x) for x in p[1:]]", "    if p[0] == \"quadrado\":", "        lista.append(Quadrado(v[0]))", "    elif p[0] == \"retangulo\":", "        lista.append(Retangulo(v[0], v[1]))", "    else:", "        lista.append(Triangulo(v[0], v[1]))", "for f in sorted(lista, key=lambda f: -f.area()):", "    print(f.nome + \" \" + str(f.area()))", "print(\"Total: \" + str(sum(f.area() for f in lista)))");
                return Ex.Code(Pg.Prompt(L.T("Crie a classe `Forma(nome)` com `area()` e as filhas `Quadrado(l)`, `Retangulo(l, a)` e `Triangulo(b, h)` (área = b·h ÷ 2, divisão inteira), cada uma passando o seu nome (`quadrado`, `retangulo`, `triangulo`) ao construtor da mãe. Leia `n` e `n` linhas (`quadrado <l>`, `retangulo <l> <a>` ou `triangulo <b> <h>`). Mostre as formas da MAIOR para a MENOR área, uma por linha como `<nome> <área>` (em caso de empate, na ordem em que foram lidas), e por fim `Total: <soma das áreas>`."), tests[0]),
                    Pg.Code(Pg.ReadInt(js, "n"), Pg.Comment(js, L.T("escreva as classes e leia as formas"))), defs + "\n" + main, tests,
                    L.T("O laço trata todas as formas igualmente: só chama `area()`, e cada objeto responde do seu jeito (polimorfismo). A ordenação e a soma usam o resultado desse método, sem precisar saber o tipo de cada forma. A ordenação é estável: empates mantêm a ordem de leitura."));
            }
        }
    }

    static Exercise MakeOut(bool js, int lv, Random r)
    {
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        if (lv == 0)
        {
            var an = Animals.OrderBy(_ => r.Next()).Take(3).ToArray(); var names = Pg.Names.OrderBy(_ => r.Next()).Take(3).ToArray();
            string defs = Cls(js, "Animal", null, new[] { "nome" }, new[] { "    def som(self):", "        return \"...\"", "    def apresenta(self):", "        return self.nome + \" diz \" + self.som()" }, new[] { "  som() {", "    return \"...\";", "  }", "  apresenta() {", "    return this.nome + \" diz \" + this.som();", "  }" }, new[] { "self.nome = nome" }, new[] { "this.nome = nome;" });
            bool plain = r.Next(2) == 0; // one child does not override
            var outs = new List<string>(); var objs = new List<string>(); var cls = new List<string>();
            for (int i = 0; i < 3; i++)
            {
                bool over = !(plain && i == 1);
                cls.Add(Cls(js, an[i].Cls, "Animal", Array.Empty<string>(), over ? new[] { "    def som(self):", $"        return \"{an[i].Sound}\"" } : new[] { "    pass" }, over ? new[] { "  som() {", $"    return \"{an[i].Sound}\";", "  }" } : new[] { "" }));
                objs.Add(js ? $"new {an[i].Cls}(\"{names[i]}\")" : $"{an[i].Cls}(\"{names[i]}\")");
                outs.Add($"{names[i]} diz {(over ? an[i].Sound : "...")}");
            }
            string code = defs + "\n" + string.Join("\n", cls) + "\n" + (js ? $"for (const a of [{string.Join(", ", objs)}]) {{\n  console.log(a.apresenta());\n}}" : $"for a in [{string.Join(", ", objs)}]:\n    print(a.apresenta())");
            string outp = string.Join("\n", outs);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("`apresenta()` está só na mãe e chama `som()` do OBJETO real. Quem sobrescreve `som()` usa o seu som; quem não sobrescreve herda o `...` da mãe."));
        }
        if (lv == 1)
        {
            string[] parts = Pg.Words.OrderBy(_ => r.Next()).Take(3).ToArray();
            bool[] sup = { true, r.Next(2) == 0 }; bool[] pre = { r.Next(2) == 0, r.Next(2) == 0 };
            string[] pyc = new string[3], jsc = new string[3];
            string[] names = { "A", "B", "C" }; var res = new string[3]; res[0] = parts[0];
            var sbp = new List<string> { "class A:\n    def f(self):\n        return \"" + parts[0] + "\"" };
            var sbj = new List<string> { "class A {\n  f() {\n    return \"" + parts[0] + "\";\n  }\n}" };
            for (int i = 1; i < 3; i++)
            {
                string p = parts[i];
                if (sup[i - 1]) { res[i] = pre[i - 1] ? p + res[i - 1] : res[i - 1] + p; sbp.Add($"class {names[i]}({names[i - 1]}):\n    def f(self):\n        return " + (pre[i - 1] ? $"\"{p}\" + super().f()" : $"super().f() + \"{p}\"")); sbj.Add($"class {names[i]} extends {names[i - 1]} {{\n  f() {{\n    return " + (pre[i - 1] ? $"\"{p}\" + super.f();" : $"super.f() + \"{p}\";") + "\n  }\n}"); }
                else { res[i] = p; sbp.Add($"class {names[i]}({names[i - 1]}):\n    def f(self):\n        return \"{p}\""); sbj.Add($"class {names[i]} extends {names[i - 1]} {{\n  f() {{\n    return \"{p}\";\n  }}\n}}"); }
            }
            string code = string.Join("\n", js ? sbj : sbp) + "\n" + string.Join("\n", names.Select(n => pr(js ? $"new {n}().f()" : $"{n}().f()")));
            string outp = string.Join("\n", res);
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("Quando a filha usa `super().f()` (ou `super.f()`), o resultado da mãe entra na conta, antes ou depois do texto da filha conforme a ordem escrita; se a filha só devolve o seu texto, ela SUBSTITUI o da mãe, e as classes mais abaixo herdam esse novo comportamento."));
        }
        if (lv == 2)
        {
            string[] w = Pg.Words.OrderBy(_ => r.Next()).Take(3).ToArray();
            bool beforeB = r.Next(2) == 0, beforeC = r.Next(2) == 0;
            var outs = new List<string>(); outs.Add($"inicio {w[0]}"); // A
            // B builds on A, C on B: constructor prints either before or after calling super.
            var seqB = beforeB ? new[] { $"fim {w[1]}", null! } : new[] { null!, $"fim {w[1]}" };
            var o = new List<string>();
            // creating C(): C ctor -> (print before?) super -> B ctor -> (print before?) super -> A ctor
            if (beforeC) o.Add($"C {w[2]}"); if (beforeB) o.Add($"B {w[1]}"); o.Add($"A {w[0]}"); if (!beforeB) o.Add($"B {w[1]}"); if (!beforeC) o.Add($"C {w[2]}");
            string c1 = js ? $"class A {{\n  constructor() {{\n    console.log(\"A {w[0]}\");\n  }}\n}}\nclass B extends A {{\n  constructor() {{\n{(beforeB ? $"    console.log(\"B {w[1]}\");\n" : "")}    super();\n{(!beforeB ? $"    console.log(\"B {w[1]}\");\n" : "")}  }}\n}}\nclass C extends B {{\n  constructor() {{\n{(beforeC ? $"    console.log(\"C {w[2]}\");\n" : "")}    super();\n{(!beforeC ? $"    console.log(\"C {w[2]}\");\n" : "")}  }}\n}}\nnew C();\nconsole.log(\"pronto\");"
                : $"class A:\n    def __init__(self):\n        print(\"A {w[0]}\")\nclass B(A):\n    def __init__(self):\n{(beforeB ? $"        print(\"B {w[1]}\")\n" : "")}        super().__init__()\n{(!beforeB ? $"        print(\"B {w[1]}\")\n" : "")}class C(B):\n    def __init__(self):\n{(beforeC ? $"        print(\"C {w[2]}\")\n" : "")}        super().__init__()\n{(!beforeC ? $"        print(\"C {w[2]}\")\n" : "")}C()\nprint(\"pronto\")";
            o.Add("pronto");
            string outp = string.Join("\n", o);
            return Ex.Output(PgOut.Prompt(outp), c1, outp, L.T("Criar um C() executa o construtor de C, que chama o de B, que chama o de A. O que cada construtor escreve ANTES de chamar super acontece na descida (C, depois B); o que escreve DEPOIS acontece na volta (A termina primeiro, depois B, depois C)."));
        }
        {
            int xa = Ex.Int(r, 1, 5), xc = Ex.Int(r, 6, 9), nine = Ex.Int(r, 10, 20), plus = Ex.Int(r, 2, 6);
            string code = js ? $"class A {{\n  constructor() {{\n    this.x = {xa};\n  }}\n  dobro() {{\n    return this.x * 2;\n  }}\n}}\nclass B extends A {{}}\nclass C extends A {{\n  constructor() {{\n    super();\n    this.x = {xc};\n  }}\n}}\nconst b = new B();\nconsole.log(b.x, new C().x, new C().dobro());\nb.x = {nine};\nconsole.log(b.x, new B().x, b.dobro());\nb.x += {plus};\nconsole.log(b.dobro());"
                : $"class A:\n    def __init__(self):\n        self.x = {xa}\n    def dobro(self):\n        return self.x * 2\nclass B(A):\n    pass\nclass C(A):\n    def __init__(self):\n        super().__init__()\n        self.x = {xc}\nb = B()\nprint(b.x, C().x, C().dobro())\nb.x = {nine}\nprint(b.x, B().x, b.dobro())\nb.x += {plus}\nprint(b.dobro())";
            string outp = $"{xa} {xc} {xc * 2}\n{nine} {xa} {nine * 2}\n{(nine + plus) * 2}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.T("B não tem construtor próprio: usa o da mãe (x começa igual ao de A). C chama o da mãe e DEPOIS troca x. Cada objeto tem o seu próprio x: mudar `b.x` não afeta os outros B criados depois, e `dobro()` herdado sempre usa o x do próprio objeto."));
        }
    }
}
