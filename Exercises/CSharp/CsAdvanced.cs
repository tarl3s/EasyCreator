using System.Text;

namespace EasyCreator.Exercises;

/// <summary>C# lessons 11-15: exceptions, interfaces and polymorphism, generics, LINQ, records.</summary>
[Exercises("cs-11", "cs-12", "cs-13", "cs-14", "cs-15")]
public sealed class CsAdvanced : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r) => lesson.Id switch
    {
        "cs-11" => r.Next(2) == 0 ? Write11(d, r) : Predict11(d, r),
        "cs-12" => r.Next(2) == 0 ? Write12(d, r) : Predict12(d, r),
        "cs-13" => r.Next(2) == 0 ? Write13(d, r) : Predict13(d, r),
        "cs-14" => r.Next(2) == 0 ? Write14(d, r) : Predict14(d, r),
        _ => r.Next(2) == 0 ? Write15(d, r) : Predict15(d, r),
    };

    const string S = "// escreva seu código aqui\n";
    static readonly string[] Names = { "Ana", "Bruno", "Carla", "Davi", "Eva", "Fabio", "Gabi", "Hugo", "Iris", "Joao", "Lia", "Mauro" };

    // ---------- cs-11 ----------
    static Exercise Write11(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        if (d <= Difficulty.Medium)
        {
            bool div = d == Difficulty.Medium;
            if (!div)
            {
                st = L.T("Leia `n` e depois `n` linhas de texto. Para cada linha tente converter em inteiro com `int.Parse` dentro de um `try`. Se der certo mostre `ok: <o dobro>`; se a linha não for um número, capture a `FormatException` e mostre `erro: '<texto>' não é número`. No fim mostre `Válidos: <quantos deram certo>`.");
                sol = "int n = int.Parse(Console.ReadLine());\nint ok = 0;\nfor (int i = 0; i < n; i++)\n{\n    string t = Console.ReadLine();\n    try\n    {\n        int v = int.Parse(t);\n        Console.WriteLine($\"ok: {v * 2}\");\n        ok++;\n    }\n    catch (FormatException)\n    {\n        Console.WriteLine($\"erro: '{t}' não é número\");\n    }\n}\nConsole.WriteLine($\"Válidos: {ok}\");\n";
                gen = g => { int n = g.Next(1, 7); return n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => g.Next(3) == 0 ? PK.Pick(g, "abc", "12x", "um", "3.5") : g.Next(-50, 100).ToString())) + "\n"; };
                oracle = i => { var l = i.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1); var o = new List<string>(); int ok = 0; foreach (var t in l) { if (int.TryParse(t, out var v)) { o.Add($"ok: {v * 2}"); ok++; } else o.Add($"erro: '{t}' não é número"); } o.Add($"Válidos: {ok}"); return string.Join("\n", o); };
                why = L.T("O código que pode falhar vai no `try`; o `catch (FormatException)` roda só quando a conversão falha, e o programa continua com a próxima linha.");
            }
            else
            {
                st = L.T("Leia `n` e depois `n` linhas `a b` (dois textos). Mostre `a / b = <quociente inteiro>`; se `b` for 0 mostre `erro: divisão por zero` (capture `DivideByZeroException`); se algum não for inteiro mostre `erro: entrada inválida` (`FormatException`). Mostre no fim `Erros: <quantos erros>`.");
                sol = "int n = int.Parse(Console.ReadLine());\nint erros = 0;\nfor (int i = 0; i < n; i++)\n{\n    var p = Console.ReadLine().Split(' ');\n    try\n    {\n        int a = int.Parse(p[0]), b = int.Parse(p[1]);\n        Console.WriteLine($\"{a} / {b} = {a / b}\");\n    }\n    catch (DivideByZeroException)\n    {\n        Console.WriteLine(\"erro: divisão por zero\");\n        erros++;\n    }\n    catch (FormatException)\n    {\n        Console.WriteLine(\"erro: entrada inválida\");\n        erros++;\n    }\n}\nConsole.WriteLine($\"Erros: {erros}\");\n";
                gen = g => { int n = g.Next(1, 7); return n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => g.Next(5) == 0 ? "7 x" : $"{g.Next(-30, 60)} {g.Next(0, 6)}")) + "\n"; };
                oracle = i => { var l = i.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1); var o = new List<string>(); int e = 0; foreach (var ln in l) { var p = ln.Split(' '); if (!int.TryParse(p[0], out var a) || !int.TryParse(p[1], out var b)) { o.Add("erro: entrada inválida"); e++; } else if (b == 0) { o.Add("erro: divisão por zero"); e++; } else o.Add($"{a} / {b} = {a / b}"); } o.Add($"Erros: {e}"); return string.Join("\n", o); };
                why = L.T("Vários `catch` tratam erros diferentes. Cada exceção cai no primeiro `catch` compatível.");
            }
        }
        else
        {
            bool gn = d == Difficulty.Genius;
            st = L.T("Uma conta começa com saldo 0. Leia `n` comandos: `depositar <v>` soma; `sacar <v>` tira. Crie a exceção `SaldoInsuficienteException : Exception`, lançada por `Sacar` quando `v` é maior que o saldo (mensagem `saldo insuficiente: faltam <x>`). Valores não positivos lançam `ArgumentException` com a mensagem `valor inválido`. Capture ambas no `Main`, mostrando `erro: <mensagem>`; sem erro mostre `saldo: <saldo>`." + (gn ? " " + ("Use também um `finally` que conta as operações tentadas e mostre `Operações: <total>` no fim.") : ""));
            sol = "int n = int.Parse(Console.ReadLine());\nvar c = new Conta();\nint ops = 0;\nfor (int i = 0; i < n; i++)\n{\n    var p = Console.ReadLine().Split(' ');\n    try\n    {\n        int v = int.Parse(p[1]);\n        if (p[0] == \"depositar\") c.Depositar(v); else c.Sacar(v);\n        Console.WriteLine($\"saldo: {c.Saldo}\");\n    }\n    catch (SaldoInsuficienteException e) { Console.WriteLine($\"erro: {e.Message}\"); }\n    catch (ArgumentException e) { Console.WriteLine($\"erro: {e.Message}\"); }\n    finally { ops++; }\n}\n" + (gn ? "Console.WriteLine($\"Operações: {ops}\");\n" : "") + "class SaldoInsuficienteException : Exception\n{\n    public SaldoInsuficienteException(string m) : base(m) { }\n}\nclass Conta\n{\n    public int Saldo { get; private set; }\n    public void Depositar(int v)\n    {\n        if (v <= 0) throw new ArgumentException(\"valor inválido\");\n        Saldo += v;\n    }\n    public void Sacar(int v)\n    {\n        if (v <= 0) throw new ArgumentException(\"valor inválido\");\n        if (v > Saldo) throw new SaldoInsuficienteException($\"saldo insuficiente: faltam {v - Saldo}\");\n        Saldo -= v;\n    }\n}\n";
            gen = g => { int n = g.Next(2, 8); return n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => (g.Next(2) == 0 ? "depositar " : "sacar ") + g.Next(-2, 40))) + "\n"; };
            oracle = i =>
            {
                int saldo = 0, ops = 0; var o = new List<string>();
                foreach (var ln in i.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1))
                {
                    var p = ln.Split(' '); int v = int.Parse(p[1]); ops++;
                    if (v <= 0) o.Add("erro: valor inválido");
                    else if (p[0] == "depositar") { saldo += v; o.Add($"saldo: {saldo}"); }
                    else if (v > saldo) o.Add($"erro: saldo insuficiente: faltam {v - saldo}");
                    else { saldo -= v; o.Add($"saldo: {saldo}"); }
                }
                if (gn) o.Add($"Operações: {ops}");
                return string.Join("\n", o);
            };
            why = L.T("Uma exceção própria herda de `Exception`. `throw` interrompe o método e sobe até o `catch` certo; o `finally` roda sempre, com ou sem erro.");
        }
        return PK.Write(st, S, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict11(Difficulty d, Random r)
    {
        int a = r.Next(2, 9), b = r.Next(0, 3); var o = new List<string>(); string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = $"try\n{{\n    Console.WriteLine(\"A\");\n    int a0 = {a}, b0 = {b};\n    int x = a0 / b0;\n    Console.WriteLine(\"B\" + x);\n}}\ncatch (DivideByZeroException)\n{{\n    Console.WriteLine(\"C\");\n}}\nfinally\n{{\n    Console.WriteLine(\"D\");\n}}\nConsole.WriteLine(\"E\");\n";
                o.Add("A"); if (b == 0) o.Add("C"); else o.Add("B" + a / b); o.Add("D"); o.Add("E"); break;
            case Difficulty.Medium:
                code = $"for (int i = 0; i < 3; i++)\n{{\n    try\n    {{\n        if (i == {b}) throw new Exception(\"x\" + i);\n        Console.Write(\"ok\" + i + \" \");\n    }}\n    catch (Exception e)\n    {{\n        Console.Write(e.Message + \" \");\n        continue;\n    }}\n    finally\n    {{\n        Console.Write(\"f \");\n    }}\n    Console.Write(\"fim \");\n}}\nConsole.WriteLine();\n";
                { var sb = new StringBuilder(); for (int i = 0; i < 3; i++) { if (i == b) { sb.Append("x" + i + " f "); continue; } sb.Append("ok" + i + " f fim "); } o.Add(sb.ToString()); }
                break;
            case Difficulty.Hard:
                code = $"Console.WriteLine(F({a}));\nConsole.WriteLine(F({b}));\nstatic string F(int n)\n{{\n    try\n    {{\n        if (n < 2) throw new ArgumentException(\"pequeno\");\n        return \"grande\";\n    }}\n    catch (ArgumentException e)\n    {{\n        return e.Message;\n    }}\n    finally\n    {{\n        Console.WriteLine(\"f\" + n);\n    }}\n}}\n";
                o.Add("f" + a); o.Add(a < 2 ? "pequeno" : "grande"); o.Add("f" + b); o.Add(b < 2 ? "pequeno" : "grande"); break;
            default:
                code = $"try\n{{\n    Nivel1({a});\n}}\ncatch (InvalidOperationException e)\n{{\n    Console.WriteLine(\"capturou \" + e.Message + \" / \" + e.InnerException?.Message);\n}}\nstatic void Nivel1(int n)\n{{\n    try\n    {{\n        Nivel2(n);\n    }}\n    catch (ArgumentOutOfRangeException e)\n    {{\n        Console.WriteLine(\"nivel1 \" + e.ParamName);\n        throw new InvalidOperationException(\"falhou\", e);\n    }}\n    finally\n    {{\n        Console.WriteLine(\"limpa\");\n    }}\n}}\nstatic void Nivel2(int n)\n{{\n    if (n % 2 == 0) throw new ArgumentOutOfRangeException(\"n\");\n    Console.WriteLine(\"ok \" + n);\n}}\n";
                if (a % 2 == 0) { o.Add("nivel1 n"); o.Add("limpa"); o.Add("capturou falhou / Specified argument was out of the range of valid values. (Parameter 'n')"); } else { o.Add("ok " + a); o.Add("limpa"); }
                break;
        }
        return PK.Predict(L.F("O programa usa o número {0}.", d == Difficulty.Medium || d == Difficulty.Hard ? a * 10 + b : a), code, string.Join("\n", o),
            L.T("Acompanhe a ordem: o `try` roda até a exceção, depois o `catch` compatível, e o `finally` roda SEMPRE (mesmo com `return` ou `continue`). Uma exceção que ninguém captura sobe para quem chamou."));
    }

    // ---------- cs-12 ----------
    static Exercise Write12(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        string shapes = "interface IForma\n{\n    string Nome { get; }\n    double Area();\n    double Perimetro();\n}\nclass Retangulo : IForma\n{\n    double l, a;\n    public Retangulo(double l, double a) { this.l = l; this.a = a; }\n    public string Nome => \"Retângulo\";\n    public double Area() => l * a;\n    public double Perimetro() => 2 * (l + a);\n}\nclass Circulo : IForma\n{\n    double r;\n    public Circulo(double r) { this.r = r; }\n    public string Nome => \"Círculo\";\n    public double Area() => Math.PI * r * r;\n    public double Perimetro() => 2 * Math.PI * r;\n}\nclass Triangulo : IForma\n{\n    double a, b, c;\n    public Triangulo(double a, double b, double c) { this.a = a; this.b = b; this.c = c; }\n    public string Nome => \"Triângulo\";\n    public double Area() { double p = (a + b + c) / 2; return Math.Sqrt(p * (p - a) * (p - b) * (p - c)); }\n    public double Perimetro() => a + b + c;\n}\n";
        bool hard = d >= Difficulty.Hard;
        string fmt = hard ? "retangulo <largura> <altura>`, `circulo <raio>` ou `triangulo <a> <b> <c>" : "retangulo <largura> <altura>` ou `circulo <raio>";
        st = L.F("Implemente a interface `IForma` (propriedade `Nome`, métodos `Area()` e `Perimetro()`) nas classes `Retangulo`, `Circulo`" + (hard ? " e `Triangulo` (Herão)" : "") + ". Leia `n` e `n` linhas: `{0}` (inteiros). Para cada forma mostre `<Nome>: área <área com 2 casas>, perímetro <perímetro com 2 casas>`.", fmt);
        if (d == Difficulty.Genius) st += " " + L.T("Depois mostre `Maior área: <Nome>` (empate: a primeira) e `Soma das áreas: <total com 2 casas>`; guarde tudo numa `List<IForma>`.");
        sol = "int n = int.Parse(Console.ReadLine());\nvar lista = new List<IForma>();\nfor (int i = 0; i < n; i++)\n{\n    var p = Console.ReadLine().Split(' ');\n    var v = p.Skip(1).Select(double.Parse).ToArray();\n    IForma f = p[0] switch\n    {\n        \"retangulo\" => new Retangulo(v[0], v[1]),\n        \"circulo\" => new Circulo(v[0]),\n        _ => new Triangulo(v[0], v[1], v[2])\n    };\n    lista.Add(f);\n    Console.WriteLine($\"{f.Nome}: área {f.Area():F2}, perímetro {f.Perimetro():F2}\");\n}\n" + (d == Difficulty.Genius ? "var maior = lista[0];\nforeach (var f in lista) if (f.Area() > maior.Area()) maior = f;\nConsole.WriteLine($\"Maior área: {maior.Nome}\");\nConsole.WriteLine($\"Soma das áreas: {lista.Sum(f => f.Area()):F2}\");\n" : "") + shapes;
        gen = g =>
        {
            int n = g.Next(1, 6); var sb = new StringBuilder(n + "\n");
            for (int k = 0; k < n; k++)
            {
                int t = g.Next(hard ? 3 : 2);
                if (t == 0) sb.Append($"retangulo {g.Next(1, 12)} {g.Next(1, 12)}\n"); else if (t == 1) sb.Append($"circulo {g.Next(1, 10)}\n");
                else sb.Append(PK.Pick(g, "triangulo 3 4 5\n", "triangulo 5 5 6\n", "triangulo 6 8 10\n", "triangulo 7 7 7\n"));
            }
            return sb.ToString();
        };
        oracle = i =>
        {
            var o = new List<string>(); var areas = new List<(string, double)>();
            foreach (var ln in i.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1))
            {
                var p = ln.Split(' '); var v = p.Skip(1).Select(x => double.Parse(x, PK.Inv)).ToArray(); string nome; double a, per;
                if (p[0] == "retangulo") { nome = "Retângulo"; a = v[0] * v[1]; per = 2 * (v[0] + v[1]); }
                else if (p[0] == "circulo") { nome = "Círculo"; a = Math.PI * v[0] * v[0]; per = 2 * Math.PI * v[0]; }
                else { nome = "Triângulo"; double s = (v[0] + v[1] + v[2]) / 2; a = Math.Sqrt(s * (s - v[0]) * (s - v[1]) * (s - v[2])); per = v[0] + v[1] + v[2]; }
                o.Add($"{nome}: área {PK.F2(a)}, perímetro {PK.F2(per)}"); areas.Add((nome, a));
            }
            if (d == Difficulty.Genius) { var m = areas[0]; foreach (var x in areas) if (x.Item2 > m.Item2) m = x; o.Add($"Maior área: {m.Item1}"); o.Add($"Soma das áreas: {PK.F2(areas.Sum(x => x.Item2))}"); }
            return string.Join("\n", o);
        };
        why = L.T("Cada classe cumpre o contrato da interface do seu jeito. Na lista de `IForma`, a chamada `f.Area()` usa o código da classe real do objeto (polimorfismo), sem `if` por tipo.");
        return PK.Write(st, S, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict12(Difficulty d, Random r)
    {
        int a = r.Next(2, 9), b = r.Next(2, 9); var o = new List<string>(); string code;
        string animals = "class Animal\n{\n    public virtual string Som() => \"...\";\n    public string Fala() => \"Som: \" + Som();\n}\nclass Cao : Animal\n{\n    public override string Som() => \"au\";\n}\nclass Gato : Animal\n{\n    public override string Som() => \"miau\";\n}\nclass Mudo : Animal { }\n";
        switch (d)
        {
            case Difficulty.Easy:
                code = $"Animal[] zoo = {{ new Cao(), new Gato(), new Mudo(), new Cao() }};\nfor (int i = 0; i < {a % 4 + 1}; i++) Console.WriteLine(zoo[i].Fala());\n" + animals;
                { string[] s = { "au", "miau", "...", "au" }; for (int i = 0; i < a % 4 + 1; i++) o.Add("Som: " + s[i]); }
                break;
            case Difficulty.Medium:
                code = $"IPonto p = new Dobro({a});\nConsole.WriteLine(p.Valor());\nConsole.WriteLine(((Dobro)p).Extra({b}));\nIPonto q = new Soma({a}, {b});\nConsole.WriteLine(q.Valor() + p.Valor());\nConsole.WriteLine(q is Dobro);\ninterface IPonto {{ int Valor(); }}\nclass Dobro : IPonto\n{{\n    int x;\n    public Dobro(int x) {{ this.x = x; }}\n    public int Valor() => x * 2;\n    public int Extra(int k) => Valor() + k;\n}}\nclass Soma : IPonto\n{{\n    int a, b;\n    public Soma(int a, int b) {{ this.a = a; this.b = b; }}\n    public int Valor() => a + b;\n}}\n";
                o.Add((a * 2).ToString()); o.Add((a * 2 + b).ToString()); o.Add((a + b + a * 2).ToString()); o.Add("False"); break;
            case Difficulty.Hard:
                code = $"Base x = new Filha();\nConsole.WriteLine(x.A() + x.B());\nConsole.WriteLine(((Base)new Filha()).C({a}));\nConsole.WriteLine(new Filha().C({b}));\nclass Base\n{{\n    public virtual string A() => \"a\";\n    public string B() => \"b\" + A();\n    public virtual int C(int n) => n;\n}}\nclass Filha : Base\n{{\n    public override string A() => \"A\";\n    public new string B() => \"B\";\n    public override int C(int n) => base.C(n) * 10 + 1;\n}}\n";
                o.Add("AbA"); o.Add((a * 10 + 1).ToString()); o.Add((b * 10 + 1).ToString()); break;
            default:
                code = $"var itens = new List<Item> {{ new Barato({a}), new Caro({b}), new Barato({a + b}) }};\nint total = 0;\nforeach (var i in itens)\n{{\n    total += i.Preco();\n    Console.WriteLine(i);\n}}\nConsole.WriteLine(total + \" \" + itens.Max(i => i.Preco()));\nabstract class Item\n{{\n    protected int baseP;\n    protected Item(int p) {{ baseP = p; }}\n    public abstract int Preco();\n    public override string ToString() => GetType().Name + \":\" + Preco();\n}}\nclass Barato : Item\n{{\n    public Barato(int p) : base(p) {{ }}\n    public override int Preco() => baseP;\n}}\nclass Caro : Item\n{{\n    public Caro(int p) : base(p) {{ }}\n    public override int Preco() => baseP * 3 + 1;\n}}\n";
                o.Add($"Barato:{a}"); o.Add($"Caro:{b * 3 + 1}"); o.Add($"Barato:{a + b}"); o.Add($"{a + b * 3 + 1 + a + b} {Math.Max(Math.Max(a, b * 3 + 1), a + b)}"); break;
        }
        return PK.Predict(L.F("As classes do programa recebem os números {0} e {1}.", a, b), code, string.Join("\n", o),
            L.T("Com `virtual`/`override` (ou interface), o método executado é o da classe REAL do objeto, não o do tipo da variável. Um método sem `virtual` (ou escondido com `new`) usa o tipo da variável. `ToString` pode ser sobrescrito."));
    }

    // ---------- cs-13 ----------
    static Exercise Write13(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        if (d <= Difficulty.Medium)
        {
            st = L.T("Escreva o método genérico `static T Maior<T>(List<T> l) where T : IComparable<T>` (primeira ocorrência do maior em caso de empate). Leia duas linhas: a primeira com inteiros e a segunda com palavras (minúsculas), separados por espaço, sempre com pelo menos um item. Mostre `Maior número: <x>`, `Maior palavra: <x>` (ordem alfabética, `string.CompareOrdinal`)" + (d == Difficulty.Medium ? (" e `Menor palavra: <x>`") : "") + ".");
            sol = "var nums = Console.ReadLine().Split(' ').Select(int.Parse).ToList();\nvar pals = Console.ReadLine().Split(' ').ToList();\nConsole.WriteLine($\"Maior número: {Maior(nums)}\");\nConsole.WriteLine($\"Maior palavra: {Maior(pals)}\");\n" + (d == Difficulty.Medium ? "Console.WriteLine($\"Menor palavra: {Menor(pals)}\");\n" : "") + "static T Maior<T>(List<T> l) where T : IComparable<T>\n{\n    T m = l[0];\n    foreach (var x in l) if (x.CompareTo(m) > 0) m = x;\n    return m;\n}\nstatic T Menor<T>(List<T> l) where T : IComparable<T>\n{\n    T m = l[0];\n    foreach (var x in l) if (x.CompareTo(m) < 0) m = x;\n    return m;\n}\n";
            string[] W = { "sol", "mar", "casa", "livro", "pedra", "nuvem", "teclado", "cafe" };
            gen = g => PK.Join(PK.Rand(g, g.Next(1, 8), -30, 99)) + "\n" + string.Join(" ", Enumerable.Range(0, g.Next(1, 6)).Select(_ => PK.Pick(g, W))) + "\n";
            oracle = i => { var t = i.Split('\n', StringSplitOptions.RemoveEmptyEntries); var n = t[0].Split(' ').Select(int.Parse); var w = t[1].Split(' '); string res = $"Maior número: {n.Max()}\nMaior palavra: {w.Max(StringComparer.Ordinal)}"; return d == Difficulty.Medium ? res + $"\nMenor palavra: {w.Min(StringComparer.Ordinal)}" : res; };
            why = L.T("`where T : IComparable<T>` garante que `CompareTo` existe para o tipo. Um único método serve para `int`, `string` e qualquer tipo comparável.");
        }
        else
        {
            bool gn = d == Difficulty.Genius;
            st = L.T("Escreva a classe genérica `Pilha<T>` (métodos `Push`, `Pop` e propriedade `Count`; `Pop` numa pilha vazia lança `InvalidOperationException`). Leia `n` comandos de uma pilha de inteiros: `push <v>`, `pop` (mostra `saiu <v>` ou `erro: pilha vazia`) e `tamanho` (mostra `tamanho <x>`)." + (gn ? " " + ("No fim, esvazie a pilha com um método genérico `Esvaziar<T>(Pilha<T> p)` que devolve a `List<T>` na ordem de saída e mostre `Restou: <valores separados por espaço>` (ou `Restou: nada`).") : ""));
            sol = "int n = int.Parse(Console.ReadLine());\nvar p = new Pilha<int>();\nfor (int i = 0; i < n; i++)\n{\n    var c = Console.ReadLine().Split(' ');\n    if (c[0] == \"push\") p.Push(int.Parse(c[1]));\n    else if (c[0] == \"pop\")\n    {\n        try { Console.WriteLine($\"saiu {p.Pop()}\"); }\n        catch (InvalidOperationException) { Console.WriteLine(\"erro: pilha vazia\"); }\n    }\n    else Console.WriteLine($\"tamanho {p.Count}\");\n}\n" + (gn ? "var resto = Esvaziar(p);\nConsole.WriteLine(\"Restou: \" + (resto.Count == 0 ? \"nada\" : string.Join(\" \", resto)));\nstatic List<T> Esvaziar<T>(Pilha<T> p)\n{\n    var l = new List<T>();\n    while (p.Count > 0) l.Add(p.Pop());\n    return l;\n}\n" : "") + "class Pilha<T>\n{\n    private readonly List<T> itens = new();\n    public int Count => itens.Count;\n    public void Push(T x) => itens.Add(x);\n    public T Pop()\n    {\n        if (itens.Count == 0) throw new InvalidOperationException(\"vazia\");\n        T x = itens[^1];\n        itens.RemoveAt(itens.Count - 1);\n        return x;\n    }\n}\n";
            gen = g => { int n = g.Next(2, 10); return n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => { int t = g.Next(5); return t <= 1 ? $"push {g.Next(1, 50)}" : t <= 3 ? "pop" : "tamanho"; })) + "\n"; };
            oracle = i => { var s = new Stack<int>(); var o = new List<string>(); foreach (var ln in i.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1)) { var c = ln.Split(' '); if (c[0] == "push") s.Push(int.Parse(c[1])); else if (c[0] == "pop") o.Add(s.Count == 0 ? "erro: pilha vazia" : $"saiu {s.Pop()}"); else o.Add($"tamanho {s.Count}"); } if (gn) o.Add("Restou: " + (s.Count == 0 ? "nada" : string.Join(" ", s))); return string.Join("\n", o); };
            why = L.T("`Pilha<T>` guarda qualquer tipo, escolhido ao criar (`Pilha<int>`). O mesmo código funciona para todos, com verificação de tipo em tempo de compilação.");
        }
        return PK.Write(st, S, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict13(Difficulty d, Random r)
    {
        int a = r.Next(2, 9), b = r.Next(10, 30); string w = PK.Pick(r, Names), w2 = PK.Pick(r, Names); var o = new List<string>(); string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = $"Console.WriteLine(Dobro({a}));\nConsole.WriteLine(Dobro(\"{w}\"));\nConsole.WriteLine(Dobro({a}.5));\nstatic T Dobro<T>(T x) => x switch\n{{\n    int i => (T)(object)(i * 2),\n    string s => (T)(object)(s + s),\n    double dd => (T)(object)(dd * 2),\n    _ => x\n}};\n";
                o.Add((a * 2).ToString()); o.Add(w + w); o.Add((a + 0.5) * 2 + ""); break;
            case Difficulty.Medium:
                code = $"var c = new Caixa<int>({a});\nvar s = new Caixa<string>(\"{w}\");\nConsole.WriteLine(c.Conteudo + 1);\nConsole.WriteLine(s.Conteudo + 1);\nConsole.WriteLine(c);\nConsole.WriteLine(s);\nConsole.WriteLine(c.Equals(new Caixa<int>({a})));\nclass Caixa<T>\n{{\n    public T Conteudo {{ get; }}\n    public Caixa(T c) {{ Conteudo = c; }}\n    public override string ToString() => \"[\" + Conteudo + \"]\";\n    public override bool Equals(object o) => o is Caixa<T> x && Equals(x.Conteudo, Conteudo);\n    public override int GetHashCode() => Conteudo.GetHashCode();\n}}\n";
                o.Add((a + 1).ToString()); o.Add(w + "1"); o.Add($"[{a}]"); o.Add($"[{w}]"); o.Add("True"); break;
            case Difficulty.Hard:
                code = $"var p = new Par<string, int>(\"{w}\", {a});\nvar q = p.Trocar();\nConsole.WriteLine(p + \" \" + q);\nvar l = new List<Par<string, int>> {{ p, new Par<string, int>(\"{w2}\", {b}), new Par<string, int>(\"x\", {a + 1}) }};\nConsole.WriteLine(l.Max(x => x.B) + \" \" + l.Count);\nclass Par<A, B2>\n{{\n    public A Primeiro {{ get; }}\n    public B2 B {{ get; }}\n    public Par(A a, B2 b) {{ Primeiro = a; B = b; }}\n    public Par<B2, A> Trocar() => new Par<B2, A>(B, Primeiro);\n    public override string ToString() => \"<\" + Primeiro + \",\" + B + \">\";\n}}\n";
                o.Add($"<{w},{a}> <{a},{w}>"); o.Add($"{Math.Max(Math.Max(a, b), a + 1)} 3"); break;
            default:
                code = $"var f = Aplicar<int, string>(x => new string('*', x), {a % 5 + 1});\nvar g = Aplicar<string, int>(s => s.Length, \"{w}{w2}\");\nConsole.WriteLine(f + \" \" + g);\nConsole.WriteLine(Soma(new[] {{ {a}, {b}, {a} }}) + \" \" + Soma(new[] {{ \"{w}\", \"{w2}\" }}));\nstatic R Aplicar<T, R>(Func<T, R> fn, T v) => fn(v);\nstatic T Soma<T>(T[] xs) where T : notnull\n{{\n    dynamic s = xs[0];\n    for (int i = 1; i < xs.Length; i++) s = s + (dynamic)xs[i];\n    return s;\n}}\n";
                o.Add(new string('*', a % 5 + 1) + " " + (w + w2).Length); o.Add($"{a + b + a} {w}{w2}"); break;
        }
        return PK.Predict(L.F("O programa usa os valores {0}, {1} e o nome {2}.", a, b, w), code, string.Join("\n", o),
            L.T("O tipo genérico (`T`) é trocado pelo tipo usado ao criar ou chamar. `Caixa<int>` e `Caixa<string>` são tipos diferentes. Com `string`, `+` concatena; com `int`, soma."));
    }

    // ---------- cs-14 ----------
    static Exercise Write14(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        if (d <= Difficulty.Medium)
        {
            st = L.T("Leia `n` e na linha seguinte `n` inteiros. Usando **LINQ**, mostre `Pares ao quadrado: <os pares elevados ao quadrado, na ordem original>` (ou `nenhum`), `Distintos: <valores distintos em ordem crescente>`" + (d == Difficulty.Medium ? (", `Soma dos maiores que a média: <soma dos valores estritamente maiores que a média>` e `Top 3: <os três maiores valores distintos, do maior para o menor>`") : "") + ".");
            sol = "int n = int.Parse(Console.ReadLine());\nvar v = Console.ReadLine().Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();\nvar pares = v.Where(x => x % 2 == 0).Select(x => x * x).ToList();\nConsole.WriteLine(\"Pares ao quadrado: \" + (pares.Count == 0 ? \"nenhum\" : string.Join(\" \", pares)));\nConsole.WriteLine(\"Distintos: \" + string.Join(\" \", v.Distinct().OrderBy(x => x)));\n" + (d == Difficulty.Medium ? "double media = v.Average();\nConsole.WriteLine($\"Soma dos maiores que a média: {v.Where(x => x > media).Sum()}\");\nConsole.WriteLine(\"Top 3: \" + string.Join(\" \", v.Distinct().OrderByDescending(x => x).Take(3)));\n" : "");
            gen = g => { int n = g.Next(1, 10); return $"{n}\n{PK.Join(PK.Rand(g, n, 1, 20))}\n"; };
            oracle = i => { var v = PK.Ints(i).Skip(1).ToList(); var pares = v.Where(x => x % 2 == 0).Select(x => x * x).ToList(); var o = "Pares ao quadrado: " + (pares.Count == 0 ? "nenhum" : PK.Join(pares)) + "\nDistintos: " + PK.Join(v.Distinct().OrderBy(x => x)); if (d == Difficulty.Medium) { double m = v.Average(); o += $"\nSoma dos maiores que a média: {v.Where(x => x > m).Sum()}\nTop 3: {PK.Join(v.Distinct().OrderByDescending(x => x).Take(3))}"; } return o; };
            why = L.T("`Where` filtra, `Select` transforma, `Distinct` remove repetidos, `OrderBy` ordena e `Take` pega os primeiros. As consultas se encadeiam.");
        }
        else
        {
            bool gn = d == Difficulty.Genius;
            st = L.T("Leia `n` e `n` vendas `produto categoria quantidade preço` (quantidade e preço inteiros). Valor da venda = quantidade × preço. Com **LINQ** mostre, para cada categoria, em ordem **decrescente de total** (empate: ordem alfabética da categoria), uma linha `<categoria>: <total> em <vendas> venda(s)`" + (gn ? (", depois `Melhor produto: <produto com maior total somando todas as vendas; empate: ordem alfabética>` e `Ticket médio: <total geral / nº de vendas, 2 casas>`") : "") + ".");
            sol = "int n = int.Parse(Console.ReadLine());\nvar v = new List<(string prod, string cat, int q, int p)>();\nfor (int i = 0; i < n; i++)\n{\n    var t = Console.ReadLine().Split(' ');\n    v.Add((t[0], t[1], int.Parse(t[2]), int.Parse(t[3])));\n}\nforeach (var g in v.GroupBy(x => x.cat).OrderByDescending(g => g.Sum(x => x.q * x.p)).ThenBy(g => g.Key, StringComparer.Ordinal))\n    Console.WriteLine($\"{g.Key}: {g.Sum(x => x.q * x.p)} em {g.Count()} venda(s)\");\n" + (gn ? "var melhor = v.GroupBy(x => x.prod).OrderByDescending(g => g.Sum(x => x.q * x.p)).ThenBy(g => g.Key, StringComparer.Ordinal).First();\nConsole.WriteLine($\"Melhor produto: {melhor.Key}\");\nConsole.WriteLine($\"Ticket médio: {v.Sum(x => x.q * x.p) / (double)n:F2}\");\n" : "");
            gen = g => { int n = g.Next(1, 8); return n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => $"{PK.Pick(g, "pao", "leite", "cafe", "suco", "queijo")} {PK.Pick(g, "padaria", "bebida", "frios")} {g.Next(1, 6)} {g.Next(1, 30)}")) + "\n"; };
            oracle = i =>
            {
                var v = i.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(l => l.Split(' ')).Select(t => (prod: t[0], cat: t[1], q: int.Parse(t[2]), p: int.Parse(t[3]))).ToList();
                var o = v.GroupBy(x => x.cat).OrderByDescending(g => g.Sum(x => x.q * x.p)).ThenBy(g => g.Key, StringComparer.Ordinal).Select(g => $"{g.Key}: {g.Sum(x => x.q * x.p)} em {g.Count()} venda(s)").ToList();
                if (gn) { o.Add("Melhor produto: " + v.GroupBy(x => x.prod).OrderByDescending(g => g.Sum(x => x.q * x.p)).ThenBy(g => g.Key, StringComparer.Ordinal).First().Key); o.Add("Ticket médio: " + PK.F2(v.Sum(x => x.q * x.p) / (double)v.Count)); }
                return string.Join("\n", o);
            };
            why = L.T("`GroupBy` agrupa as vendas por categoria; dentro de cada grupo `Sum` e `Count` dão os totais. `OrderByDescending(...).ThenBy(...)` desempata por ordem alfabética.");
        }
        return PK.Write(st, S, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict14(Difficulty d, Random r)
    {
        var v = PK.Rand(r, 7, 1, 20); int k = r.Next(3, 10); var o = new List<string>(); string code; string arr = $"var v = new[] {{ {PK.Join(v, ", ")} }};\n";
        switch (d)
        {
            case Difficulty.Easy:
                code = arr + $"Console.WriteLine(v.Where(x => x > {k}).Count());\nConsole.WriteLine(v.Select(x => x * 2).Sum());\nConsole.WriteLine(v.Max() - v.Min());\nConsole.WriteLine(v.Any(x => x == {k}));\n";
                o.Add(v.Count(x => x > k).ToString()); o.Add((v.Sum() * 2).ToString()); o.Add((v.Max() - v.Min()).ToString()); o.Add(v.Contains(k) ? "True" : "False"); break;
            case Difficulty.Medium:
                code = arr + $"Console.WriteLine(string.Join(\",\", v.Where(x => x % 2 == 1).OrderByDescending(x => x)));\nConsole.WriteLine(string.Join(\",\", v.Skip(2).Take(3)));\nConsole.WriteLine(v.First(x => x > {k}));\nConsole.WriteLine(v.Distinct().Count());\n";
                o.Add(string.Join(",", v.Where(x => x % 2 == 1).OrderByDescending(x => x))); o.Add(string.Join(",", v.Skip(2).Take(3))); { var f = v.FirstOrDefault(x => x > k); if (f == 0) return Predict14(d, r); o.Add(f.ToString()); } o.Add(v.Distinct().Count().ToString()); break;
            case Difficulty.Hard:
                code = arr + $"var grupos = v.GroupBy(x => x % {k % 4 + 2}).OrderBy(g => g.Key);\nforeach (var g in grupos)\n    Console.WriteLine($\"{{g.Key}}: {{string.Join(\" \", g)}} = {{g.Sum()}}\");\nConsole.WriteLine(v.Zip(v.Skip(1), (a, b) => b - a).Count(x => x > 0));\n";
                { int m = k % 4 + 2; foreach (var g in v.GroupBy(x => x % m).OrderBy(g => g.Key)) o.Add($"{g.Key}: {string.Join(" ", g)} = {g.Sum()}"); o.Add(v.Zip(v.Skip(1), (a, b) => b - a).Count(x => x > 0).ToString()); }
                break;
            default:
                code = arr + $"var q = v.Where(x =>\n{{\n    Console.Write(\"w\" + x + \" \");\n    return x > {k};\n}});\nConsole.WriteLine(\"antes\");\nConsole.WriteLine(q.First());\nConsole.WriteLine(q.Count());\nvar lista = q.ToList();\nConsole.WriteLine(lista.Count);\n";
                { var sb = new StringBuilder(); o.Add("antes"); int first = -1; var lines = new List<string>(); string cur = ""; foreach (var x in v) { cur += "w" + x + " "; if (x > k) { first = x; break; } } if (first < 0) return Predict14(d, r); o.Add(cur + first); cur = ""; int cnt = 0; foreach (var x in v) { cur += "w" + x + " "; if (x > k) cnt++; } o.Add(cur + cnt); cur = ""; foreach (var x in v) cur += "w" + x + " "; o.Add(cur + cnt); _ = sb; _ = lines; }
                break;
        }
        return PK.Predict(L.F("A lista usada pelo programa é {0}.", PK.Join(v, ", ")), code, string.Join("\n", o),
            L.T("Cada método LINQ devolve uma nova sequência, na ordem do encadeamento. `Where` com efeitos colaterais mostra a avaliação **preguiçosa**: a consulta só roda quando é consumida (`First`, `Count`, `ToList`), e roda de novo a cada consumo."));
    }

    // ---------- cs-15 ----------
    static Exercise Write15(Difficulty d, Random r)
    {
        string st, sol, why; Func<Random, string> gen; Func<string, string> oracle;
        if (d <= Difficulty.Medium)
        {
            st = L.T("Crie o record `record Aluno(string Nome, int Nota);`. Leia `n` linhas `nome nota`. Mostre `Melhor: <o record do aluno de maior nota; empate: o primeiro>` (o `ToString` automático do record) e, na linha seguinte, `Bônus: <o mesmo aluno com 1 ponto a mais, criado com 'with', limitado a 10>`." + (d == Difficulty.Medium ? " " + ("Depois mostre `Iguais: <quantos pares de alunos (i < j) são iguais em valor, isto é, mesmo nome e mesma nota>`.") : ""));
            sol = "int n = int.Parse(Console.ReadLine());\nvar l = new List<Aluno>();\nfor (int i = 0; i < n; i++)\n{\n    var p = Console.ReadLine().Split(' ');\n    l.Add(new Aluno(p[0], int.Parse(p[1])));\n}\nvar m = l[0];\nforeach (var a in l) if (a.Nota > m.Nota) m = a;\nConsole.WriteLine($\"Melhor: {m}\");\nConsole.WriteLine($\"Bônus: {m with { Nota = Math.Min(10, m.Nota + 1) }}\");\n" + (d == Difficulty.Medium ? "int iguais = 0;\nfor (int i = 0; i < n; i++)\n    for (int j = i + 1; j < n; j++)\n        if (l[i] == l[j]) iguais++;\nConsole.WriteLine($\"Iguais: {iguais}\");\n" : "") + "record Aluno(string Nome, int Nota);\n";
            gen = g => { int n = g.Next(1, 7); return n + "\n" + string.Join("\n", Enumerable.Range(0, n).Select(_ => $"{PK.Pick(g, "Ana", "Bia", "Caio")} {g.Next(5, 11)}")) + "\n"; };
            oracle = i =>
            {
                var l = i.Split('\n', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(x => x.Split(' ')).Select(t => (t[0], int.Parse(t[1]))).ToList(); var m = l[0]; foreach (var a in l) if (a.Item2 > m.Item2) m = a;
                string o = $"Melhor: Aluno {{ Nome = {m.Item1}, Nota = {m.Item2} }}\nBônus: Aluno {{ Nome = {m.Item1}, Nota = {Math.Min(10, m.Item2 + 1)} }}";
                if (d == Difficulty.Medium) { int c = 0; for (int a = 0; a < l.Count; a++) for (int b = a + 1; b < l.Count; b++) if (l[a] == l[b]) c++; o += $"\nIguais: {c}"; }
                return o;
            };
            why = L.T("Um `record` gera `ToString` (`Aluno { Nome = Ana, Nota = 9 }`) e igualdade por valor: dois records com os mesmos campos são `==`. `with` cria uma cópia alterando só o que você indica.");
        }
        else
        {
            bool gn = d == Difficulty.Genius;
            st = L.T("Um robô começa em (0, 0) e lê **uma linha** de comandos, cada um uma letra: `N` (y+1), `S` (y−1), `L` (x+1) e `O` (x−1); a linha pode estar vazia. Use `record Ponto(int X, int Y)` com um método `Mover(char)` que devolve o novo ponto (com `with`) e guarde os pontos visitados num `HashSet<Ponto>` (começando pelo ponto inicial). Mostre `Final: <ponto final>` (o `ToString` do record), `Distintos: <pontos diferentes visitados>` e `Manhattan: <|x| + |y| do final>`." + (gn ? " " + ("Mostre também `Passou duas vezes: <quantos passos terminaram num ponto que já tinha sido visitado>`.") : ""));
            sol = "string cmd = Console.ReadLine() ?? \"\";\nvar p = new Ponto(0, 0);\nvar vis = new HashSet<Ponto> { p };\nint repetidos = 0;\nforeach (char c in cmd)\n{\n    p = p.Mover(c);\n    if (!vis.Add(p)) repetidos++;\n}\nConsole.WriteLine($\"Final: {p}\");\nConsole.WriteLine($\"Distintos: {vis.Count}\");\nConsole.WriteLine($\"Manhattan: {Math.Abs(p.X) + Math.Abs(p.Y)}\");\n" + (gn ? "Console.WriteLine($\"Passou duas vezes: {repetidos}\");\n" : "") + "record Ponto(int X, int Y)\n{\n    public Ponto Mover(char c) => c switch\n    {\n        'N' => this with { Y = Y + 1 },\n        'S' => this with { Y = Y - 1 },\n        'L' => this with { X = X + 1 },\n        'O' => this with { X = X - 1 },\n        _ => this\n    };\n}\n";
            gen = g => string.Concat(Enumerable.Range(0, g.Next(0, 14)).Select(_ => "NSLO"[g.Next(4)])) + "\n";
            oracle = i =>
            {
                var cmd = i.TrimEnd('\n'); int x = 0, y = 0, rep = 0; var vis = new HashSet<(int, int)> { (0, 0) };
                foreach (char c in cmd) { if (c == 'N') y++; else if (c == 'S') y--; else if (c == 'L') x++; else x--; if (!vis.Add((x, y))) rep++; }
                var o = $"Final: Ponto {{ X = {x}, Y = {y} }}\nDistintos: {vis.Count}\nManhattan: {Math.Abs(x) + Math.Abs(y)}"; if (gn) o += $"\nPassou duas vezes: {rep}"; return o;
            };
            why = L.T("Como `Ponto` é um record, dois pontos com os mesmos X e Y são iguais e têm o mesmo hash: o `HashSet` conta posições distintas sem código extra. `Add` devolve falso se já existia.");
        }
        return PK.Write(st, S, sol, PK.Tests(r, 6, gen, oracle), why);
    }

    static Exercise Predict15(Difficulty d, Random r)
    {
        int a = r.Next(1, 9), b = r.Next(1, 9), k = r.Next(2, 6); string w = PK.Pick(r, Names); var o = new List<string>(); string code;
        switch (d)
        {
            case Difficulty.Easy:
                code = $"var p = new P({a}, {b});\nvar q = p with {{ Y = {k} }};\nConsole.WriteLine(p);\nConsole.WriteLine(q);\nConsole.WriteLine(p == q);\nConsole.WriteLine(p == new P({a}, {b}));\nrecord P(int X, int Y);\n";
                o.Add($"P {{ X = {a}, Y = {b} }}"); o.Add($"P {{ X = {a}, Y = {k} }}"); o.Add(b == k ? "True" : "False"); o.Add("True"); break;
            case Difficulty.Medium:
                code = $"var a = new Pessoa(\"{w}\", {a});\nvar b = a with {{ Idade = {a} + 1 }};\nvar c = new Pessoa(\"{w}\", {a + 1});\nConsole.WriteLine(b == c);\nConsole.WriteLine(ReferenceEquals(b, c));\nConsole.WriteLine(a.Equals(b));\nvar (n, i) = b;\nConsole.WriteLine(n + i);\nrecord Pessoa(string Nome, int Idade);\n";
                o.Add("True"); o.Add("False"); o.Add("False"); o.Add($"{w}{a + 1}"); break;
            case Difficulty.Hard:
                code = $"var s = new HashSet<P> {{ new P({a}, {b}), new P({b}, {a}), new P({a}, {b}) }};\nConsole.WriteLine(s.Count);\nvar d = new Dictionary<P, string> {{ [new P({a}, {b})] = \"{w}\" }};\nConsole.WriteLine(d.ContainsKey(new P({a}, {b})));\nConsole.WriteLine(d[new P({a}, {b})]);\nvar t = new T2(new List<int> {{ {a} }});\nvar u = t with {{ }};\nu.L.Add({b});\nConsole.WriteLine(t.L.Count);\nrecord P(int X, int Y);\nrecord T2(List<int> L);\n";
                o.Add((a == b ? 1 : 2).ToString()); o.Add("True"); o.Add(w); o.Add("2"); break;
            default:
                code = $"var l = new List<P> {{ new P({a}, {b}), new P({b}, {a}), new P({a}, {k}) }};\nConsole.WriteLine(l.Distinct().Count());\nConsole.WriteLine(l.IndexOf(new P({b}, {a})));\nConsole.WriteLine(string.Join(\";\", l.OrderBy(p => p).Select(p => p.X + \",\" + p.Y)));\nvar m = l[0] with {{ X = l[1].X }};\nConsole.WriteLine(m == l[1] ? \"igual\" : m.ToString());\nrecord P(int X, int Y) : IComparable<P>\n{{\n    public int CompareTo(P o) => X != o.X ? X.CompareTo(o.X) : Y.CompareTo(o.Y);\n}}\n";
                { var ps = new List<(int, int)> { (a, b), (b, a), (a, k) }; o.Add(ps.Distinct().Count().ToString()); o.Add(ps.IndexOf((b, a)).ToString()); o.Add(string.Join(";", ps.OrderBy(p => p.Item1).ThenBy(p => p.Item2).Select(p => p.Item1 + "," + p.Item2))); var mm = (b, ps[0].Item2); o.Add(mm == ps[1] ? "igual" : $"P {{ X = {mm.Item1}, Y = {mm.Item2} }}"); }
                break;
        }
        return PK.Predict(L.F("Os valores usados nos records são {0}, {1} e {2}.", a, b, k), code, string.Join("\n", o),
            L.T("Records comparam por VALOR (`==` e `Equals`), mas `ReferenceEquals` compara objetos. `with` cria uma cópia alterada. `ToString` mostra `Tipo { Campo = valor }`. Campos que são listas são copiados por referência (cópia rasa)."));
    }
}
