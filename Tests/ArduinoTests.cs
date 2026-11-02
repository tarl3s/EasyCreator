using System.IO;
using System.Diagnostics;
using System.Text.Json;
using EasyCreator.Arduino;
using EasyCreator.CInterp;

namespace EasyCreator.Tests;

/// <summary>Headless tests for the C interpreter, the Arduino simulator and the robotics lessons.</summary>
public static class ArduinoTests
{
    sealed record CCase(string Name, string Code, string Stdin, string? Expected, string? ErrorContains = null);

    static int _fail, _pass;
    static TextWriter _log = TextWriter.Null;

    /// <summary>Runs every test; returns the number of failures. Writes one line per test to <paramref name="log"/>.</summary>
    public static int Run(TextWriter log)
    {
        _log = log;
        _fail = _pass = 0;
        var sw = Stopwatch.StartNew();
        try
        {
            CInterpreter.RunOnBigStack(() =>
            {
                RunCTests();
                RunHookTests();
                RunSimulatorTests();
                RunLessonTests();
                RunSessionTests();
                return 0;
            });
        }
        catch (Exception e)
        {
            Fail("test runner", "exceção inesperada: " + e);
        }
        log.WriteLine($"ArduinoTests: {_pass} passed, {_fail} failed ({sw.Elapsed.TotalSeconds:0.0} s)");
        return _fail;
    }

    static void Pass(string name) { _pass++; _log.WriteLine("PASS " + name); }
    static void Fail(string name, string why) { _fail++; _log.WriteLine("FAIL " + name + " — " + why); }

    static void Check(string name, bool ok, string why = "")
    {
        if (ok) Pass(name); else Fail(name, why);
    }

    static string Norm(string s) => s.Replace("\r\n", "\n").TrimEnd();

    // ===================================================================== C interpreter
    static readonly CCase[] CCases =
    {
        new("precedência aritmética", "int main(){ printf(\"%d %d %d %d\\n\", 2+3*4, (2+3)*4, 10-4-3, 2*3%4); return 0; }", "", "14 20 3 2"),
        new("precedência bits/shift", "int main(){ printf(\"%d %d %d\\n\", 1<<2+1, 5&3|8, 6^3); return 0; }", "", "8 9 5"),
        new("curto-circuito", "int main(){ int a=0; int b = (a!=0) && (10/a > 1); printf(\"%d %d\\n\", b, !a || a++); printf(\"%d\\n\", a); return 0; }", "", "0 1\n0"),
        new("ternário aninhado", "int main(){ int x=7; printf(\"%s\\n\", x>5 ? x>8 ? \"big\" : \"mid\" : \"small\"); return 0; }", "", "mid"),
        new("atribuição composta", "int main(){ int x=10; x+=5; x*=2; x-=3; x/=4; x%=4; x<<=3; x|=1; x^=2; x&=0xF; printf(\"%d\\n\", x); return 0; }", "", "3"),
        new("incremento pré/pós", "int main(){ int i=5; int a=i++; int b=++i; int c=i--; printf(\"%d %d %d %d\\n\", a, b, c, i); return 0; }", "", "5 7 7 6"),
        new("divisão inteira e casts", "int main(){ printf(\"%d %d %.2f %d %d\\n\", 7/2, -7/2, (double)7/2, (int)-2.7, -7%3); return 0; }", "", "3 -3 3.50 -2 -1"),
        new("aritmética com char", "int main(){ char c='a'; c+=2; printf(\"%c %d\\n\", c, c); return 0; }", "", "c 99"),
        new("unsigned dá a volta", "int main(){ unsigned int u=0; u--; unsigned char b=250; b+=10; printf(\"%u %d\\n\", u, b); return 0; }", "", "4294967295 4"),
        new("overflow de int", "int main(){ int m = 2147483647; m++; printf(\"%d\\n\", m); return 0; }", "", "-2147483648"),
        new("comparação unsigned", "int main(){ unsigned int u = 1; int n = -1; printf(\"%d %d\\n\", n < u, n < 1); return 0; }", "", "0 1"),
        new("recursão fatorial", "long fat(int n){ return n <= 1 ? 1 : n * fat(n-1); } int main(){ printf(\"%ld\\n\", fat(20)); return 0; }", "", "2432902008176640000"),
        new("recursão fibonacci", "int fib(int n){ if (n < 2) return n; return fib(n-1) + fib(n-2); } int main(){ printf(\"%d\\n\", fib(25)); return 0; }", "", "75025"),
        new("torre de hanoi", "int mov=0; void h(int n,int a,int b,int c){ if(n==0) return; h(n-1,a,c,b); mov++; h(n-1,c,b,a); } int main(){ h(10,1,2,3); printf(\"%d\\n\", mov); return 0; }", "", "1023"),
        new("vetores e sizeof", "int main(){ int v[] = {3,9,2,7}; int n = sizeof(v)/sizeof(v[0]); int m = v[0], s = 0; for (int i = 0; i < n; i++) { s += v[i]; if (v[i] > m) m = v[i]; } printf(\"%d %d %d\\n\", n, s, m); return 0; }", "", "4 21 9"),
        new("matriz 2D", "int main(){ int a[2][2]={{1,2},{3,4}}, b[2][2]={{5,6},{7,8}}, c[2][2]; for(int i=0;i<2;i++) for(int j=0;j<2;j++){ c[i][j]=0; for(int k=0;k<2;k++) c[i][j]+=a[i][k]*b[k][j]; } printf(\"%d %d %d %d\\n\", c[0][0], c[0][1], c[1][0], c[1][1]); return 0; }", "", "19 22 43 50"),
        new("matriz em função", "int soma(int m[][3], int linhas){ int s=0; for(int i=0;i<linhas;i++) for(int j=0;j<3;j++) s+=m[i][j]; return s; } int main(){ int m[2][3] = {1,2,3,4,5,6}; printf(\"%d %d\\n\", soma(m, 2), m[1][0]); return 0; }", "", "21 4"),
        new("strings da biblioteca", "#include <string.h>\nint main(){ char a[30] = \"ola\"; char b[] = \"mundo\"; strcat(a, \" \"); strcat(a, b); printf(\"%s %d %d %d %d\\n\", a, (int)strlen(a), strcmp(\"abc\",\"abd\") < 0, strcmp(b, \"mundo\"), strcmp(\"b\",\"a\") > 0); return 0; }", "", "ola mundo 9 1 0 1"),
        new("inverter string", "#include <string.h>\nvoid inv(char *s){ int i=0, j=strlen(s)-1; while(i<j){ char t=s[i]; s[i]=s[j]; s[j]=t; i++; j--; } } int main(){ char s[] = \"hello\"; inv(s); printf(\"%s %d\\n\", s, (int)sizeof(s)); return 0; }", "", "olleh 6"),
        new("strchr/strstr/strncpy", "#include <string.h>\nint main(){ char s[] = \"banana\"; char *p = strchr(s, 'n'); char *q = strstr(s, \"ana\"); char d[10]; strncpy(d, s, 3); d[3] = '\\0'; printf(\"%s %s %s %d\\n\", p, q, d, (int)(p - s)); return 0; }", "", "nana anana ban 2"),
        new("ponteiros e aritmética", "int main(){ int a[5]={10,20,30,40,50}; int *p=a; p+=2; printf(\"%d %d %d %d\\n\", *p, *(p+1), p[-1], (int)(p-a)); *p = 99; printf(\"%d\\n\", a[2]); return 0; }", "", "30 40 20 2\n99"),
        new("ponteiro para ponteiro", "int main(){ int x=5; int *p=&x; int **pp=&p; **pp=9; printf(\"%d %d\\n\", x, *p); return 0; }", "", "9 9"),
        new("troca com ponteiros", "void troca(int *a, int *b){ int t=*a; *a=*b; *b=t; } int main(){ int x=1, y=2; troca(&x, &y); printf(\"%d %d\\n\", x, y); return 0; }", "", "2 1"),
        new("percorrer string com ponteiro", "int main(){ const char *s = \"abcde\"; int n = 0; while (*s++) n++; printf(\"%d\\n\", n); return 0; }", "", "5"),
        new("structs", "#include <stdio.h>\nstruct Ponto { int x, y; };\nstruct Retangulo { struct Ponto canto; int largura, altura; char nome[10]; };\nvoid moveValor(struct Ponto p) { p.x = 100; }\nvoid movePtr(struct Ponto *p) { p->x += 1; p->y += 2; }\nstruct Ponto soma(struct Ponto a, struct Ponto b) { struct Ponto r; r.x = a.x + b.x; r.y = a.y + b.y; return r; }\nint main() {\n  struct Retangulo r = {{1, 2}, 30, 40, \"ret\"};\n  moveValor(r.canto);\n  movePtr(&r.canto);\n  struct Ponto s = soma(r.canto, r.canto);\n  printf(\"%d %d %d %s %d %d\\n\", r.canto.x, r.canto.y, r.largura * r.altura, r.nome, s.x, s.y);\n  return 0;\n}", "", "2 4 1200 ret 4 8"),
        new("lista ligada com malloc", "#include <stdlib.h>\ntypedef struct No { int valor; struct No *prox; } No;\nNo *insere(No *lista, int v) { No *n = (No*)malloc(sizeof(No)); n->valor = v; n->prox = lista; return n; }\nint main() {\n  No *lista = NULL;\n  for (int i = 1; i <= 5; i++) lista = insere(lista, i);\n  int soma = 0;\n  for (No *p = lista; p != NULL; p = p->prox) { printf(\"%d \", p->valor); soma += p->valor; }\n  printf(\"| %d\\n\", soma);\n  while (lista) { No *t = lista; lista = lista->prox; free(t); }\n  return 0;\n}", "", "5 4 3 2 1 | 15"),
        new("vetor de structs com typedef", "typedef struct { char nome[20]; double nota; } Aluno; int main(){ Aluno t[3] = {{\"Ana\", 9.5}, {\"Bia\", 7.0}, {\"Caio\", 8.25}}; double s = 0; Aluno *melhor = &t[0]; for (int i = 0; i < 3; i++) { s += t[i].nota; if (t[i].nota > melhor->nota) melhor = &t[i]; } printf(\"%.2f %s\\n\", s / 3, melhor->nome); return 0; }", "", "8.25 Ana"),
        new("inicializador designado", "struct P { int x, y, z; }; int main(){ struct P p = { .z = 3, .x = 1 }; int v[5] = { [3] = 7, 1 }; printf(\"%d %d %d %d %d %d\\n\", p.x, p.y, p.z, v[0], v[3], v[4]); return 0; }", "", "1 0 3 0 7 1"),
        new("malloc 2D", "#include <stdlib.h>\nint main(){ int n=3; int **m = malloc(n*sizeof(int*)); for(int i=0;i<n;i++){ m[i]=malloc(n*sizeof(int)); for(int j=0;j<n;j++) m[i][j]=i*n+j; } printf(\"%d %d\\n\", m[2][1], m[1][2]); for(int i=0;i<n;i++) free(m[i]); free(m); return 0; }", "", "7 5"),
        new("formatos do printf", "int main(){ printf(\"[%5d][%-5d][%05d][%+d][%x][%X][%o][%c][%s][%.3s][%8.3f][%-8.2f][%e][%g][%%][%ld][%u]\\n\", 42, 42, 42, 42, 255, 255, 8, 'z', \"hi\", \"abcdef\", 3.14159, 2.5, 1234.5, 0.5, 123456789L, 3000000000u); return 0; }", "",
            "[   42][42   ][00042][+42][ff][FF][10][z][hi][abc][   3.142][2.50    ][1.234500e+03][0.5][%][123456789][3000000000]"),
        new("printf %g e largura *", "int main(){ printf(\"%g %g %g %*d|%.1f\\n\", 100000.0, 1000000.0, 0.0001234, 4, 7, 0.05); return 0; }", "", "100000 1e+06 0.0001234    7|0.1"),
        new("scanf soma e média", "int main(){ int n; scanf(\"%d\", &n); int s = 0; for (int i = 0; i < n; i++) { int x; scanf(\"%d\", &x); s += x; } printf(\"soma=%d media=%.2f\\n\", s, (float)s / n); return 0; }", "4\n1 2 3 4\n", "soma=10 media=2.50"),
        new("scanf texto e número", "int main(){ char nome[20]; int idade; double altura; scanf(\"%s %d %lf\", nome, &idade, &altura); printf(\"%s tem %d anos e %.2f m\\n\", nome, idade, altura); return 0; }", "Ana 20 1.65", "Ana tem 20 anos e 1.65 m"),
        new("scanf até EOF", "int main(){ int x, s = 0, n = 0; while (scanf(\"%d\", &x) == 1) { s += x; n++; } printf(\"%d %d\\n\", n, s); return 0; }", "1 2 3\n4\n", "4 10"),
        new("scanf %c", "int main(){ char a, b; scanf(\"%c %c\", &a, &b); printf(\"%c%c\\n\", b, a); return 0; }", "x y", "yx"),
        new("getchar/putchar", "#include <ctype.h>\nint main(){ int c; while ((c = getchar()) != EOF) putchar(toupper(c)); return 0; }", "abc\n", "ABC"),
        new("fgets e contagem", "#include <string.h>\nint main(){ char linha[100]; fgets(linha, 100, stdin); int v = 0; for (int i = 0; linha[i]; i++) if (strchr(\"aeiou\", linha[i])) v++; printf(\"%d\\n\", v); return 0; }", "programacao em c\n", "6"),
        new("switch com fallthrough", "int main(){ for (int i = 0; i < 4; i++) { switch (i) { case 0: printf(\"zero \"); case 1: printf(\"um \"); break; case 2: printf(\"dois \"); break; default: printf(\"outro \"); } } printf(\"\\n\"); return 0; }", "", "zero um um dois outro"),
        new("laços, break e continue (primos)", "int main(){ for (int n = 2; n < 20; n++) { int primo = 1; for (int d = 2; d * d <= n; d++) { if (n % d == 0) { primo = 0; break; } } if (!primo) continue; printf(\"%d \", n); } printf(\"\\n\"); return 0; }", "", "2 3 5 7 11 13 17 19"),
        new("do-while", "int main(){ int n = 12345, s = 0; do { s += n % 10; n /= 10; } while (n > 0); printf(\"%d\\n\", s); return 0; }", "", "15"),
        new("for com vírgula", "int main(){ int i, j, c = 0; for (i = 0, j = 10; i < j; i++, j--) c++; printf(\"%d %d %d\\n\", c, i, j); return 0; }", "", "5 5 5"),
        new("macros #define", "#define N 5\n#define QUAD(x) ((x)*(x))\n#define MAIOR(a,b) ((a) > (b) ? (a) : (b))\n#ifdef N\n#define TEM 1\n#else\n#define TEM 0\n#endif\nint main(){ printf(\"%d %d %d %d\\n\", N, QUAD(N+1), MAIOR(3, 9), TEM); return 0; }", "", "5 36 9 1"),
        new("enum", "enum Cor { VERMELHO, VERDE = 5, AZUL }; int main(){ enum Cor c = AZUL; printf(\"%d %d %d\\n\", VERMELHO, VERDE, c); return 0; }", "", "0 5 6"),
        new("variável static", "int conta(){ static int n = 0; n++; return n; } int main(){ conta(); conta(); printf(\"%d\\n\", conta()); return 0; }", "", "3"),
        new("globais e sombreamento", "int x = 10; int vet[3]; int main(){ int x = 5; { int x = 1; printf(\"%d \", x); } printf(\"%d %d\\n\", x, vet[2]); return 0; }", "", "1 5 0"),
        new("matemática", "#include <math.h>\nint main(){ printf(\"%.3f %.1f %.0f %.2f %d %.2f\\n\", sqrt(2), pow(2,10), floor(-2.5), fabs(-3.25), abs(-7), fmod(7.5, 2)); return 0; }", "", "1.414 1024.0 -3 3.25 7 1.50"),
        new("operadores de bits", "int main(){ unsigned char b = 0xA5; printf(\"%d %d %d %d\\n\", b >> 4, b & 0x0F, (unsigned char)~b, 1 << 10); return 0; }", "", "10 5 90 1024"),
        new("float vs double", "int main(){ float f = 0.1f; double d = 0.1; printf(\"%d %d\\n\", f == 0.1, d == 0.1); return 0; }", "", "0 1"),
        new("vetor de tamanho variável", "int main(){ int n; scanf(\"%d\", &n); int a[n]; for (int i = 0; i < n; i++) a[i] = i * i; printf(\"%d %d\\n\", a[n-1], (int)(sizeof(a) / sizeof(int))); return 0; }", "5", "16 5"),
        new("sprintf e atoi", "#include <stdlib.h>\nint main(){ char buf[32]; sprintf(buf, \"%d-%s\", 42, \"ok\"); int v = atoi(\"123abc\"); printf(\"%s %d %.1f\\n\", buf, v + 1, atof(\"2.5\") * 2); return 0; }", "", "42-ok 124 5.0"),
        new("escapes e literais", "int main(){ printf(\"%d %d %d %d %d\\n\", '\\n', '\\0', '\\x41', 017, 0x1F); return 0; }", "", "10 0 65 15 31"),
        new("sizeof de tipos", "int main(){ printf(\"%d %d %d %d %d\\n\", (int)sizeof(char), (int)sizeof(int), (int)sizeof(double), (int)sizeof(int*), (int)sizeof(long)); return 0; }", "", "1 4 8 8 8"),
        new("unsigned long long", "int main(){ unsigned long long big = 1ULL << 63; printf(\"%llu\\n\", big); return 0; }", "", "9223372036854775808"),
        new("ordenar nomes", "#include <string.h>\nint main(){ char n[3][10] = {\"Caio\", \"Ana\", \"Bia\"}; char t[10]; for (int i = 0; i < 3; i++) for (int j = 0; j < 2 - i; j++) if (strcmp(n[j], n[j+1]) > 0) { strcpy(t, n[j]); strcpy(n[j], n[j+1]); strcpy(n[j+1], t); } printf(\"%s %s %s\\n\", n[0], n[1], n[2]); return 0; }", "", "Ana Bia Caio"),
        new("ponteiro percorrendo structs", "struct P { int v; }; int main(){ struct P a[4] = {{1},{2},{3},{4}}; int s = 0; for (struct P *p = a; p < a + 4; p++) s += p->v; printf(\"%d\\n\", s); return 0; }", "", "10"),
        new("protótipo antes do uso", "int quadrado(int x);\nint main(){ printf(\"%d\\n\", quadrado(9)); return 0; }\nint quadrado(int x){ return x * x; }", "", "81"),
        new("struct estilo C++ (sem 'struct')", "struct Ponto { int x; int y; }; Ponto origem() { Ponto p = {0, 0}; return p; } int main(){ Ponto p = origem(); p.y = 3; printf(\"%d %d\\n\", p.x, p.y); return 0; }", "", "0 3"),
        new("bool e true/false", "#include <stdbool.h>\nint main(){ bool b = 5; bool c = false; printf(\"%d %d %d\\n\", b, c, !c && b); return 0; }", "", "1 0 1"),
        new("puts", "int main(){ puts(\"linha 1\"); puts(\"linha 2\"); return 0; }", "", "linha 1\nlinha 2"),
        new("rand com semente fixa é determinístico", "#include <stdlib.h>\nint main(){ srand(7); int a = rand(); srand(7); int b = rand(); printf(\"%d %d\\n\", a == b, a >= 0 && a <= RAND_MAX); return 0; }", "", "1 1"),
        new("exit", "#include <stdlib.h>\nint main(){ printf(\"antes\\n\"); exit(0); printf(\"depois\\n\"); return 0; }", "", "antes"),
        new("main sem return", "int main(){ printf(\"oi\\n\"); }", "", "oi"),

        // ---- errors (friendly messages) ----
        new("erro: ponto e vírgula", "int main(){\n  int x = 5\n  printf(\"%d\", x);\n  return 0;\n}", "", null, "linha 2"),
        new("erro: ; mensagem amigável", "int main(){\n  int x = 5\n  return 0;\n}", "", null, "faltou ';'"),
        new("erro: variável não declarada", "int main(){ int valor = 1; printf(\"%d\", valr); return 0; }", "", null, "você quis dizer 'valor'"),
        new("erro: função não existe", "int main(){ printn(\"x\"); return 0; }", "", null, "printf"),
        new("erro: índice fora do vetor", "int main(){ int a[3]; for (int i = 0; i <= 3; i++) a[i] = i; return 0; }", "", null, "fora dos limites"),
        new("erro: divisão por zero", "int main(){ int z = 0; printf(\"%d\", 10 / z); return 0; }", "", null, "divisão por zero"),
        new("erro: ponteiro NULL", "int main(){ int *p = NULL; *p = 3; return 0; }", "", null, "NULL"),
        new("erro: scanf sem &", "int main(){ int x; scanf(\"%d\", x); return 0; }", "5", null, "&"),
        new("erro: recursão infinita", "int f(int n){ return f(n + 1) + 1; } int main(){ return f(0); }", "", null, "recursão"),
        new("erro: laço infinito é interrompido", "int main(){ int x = 0; while (1) { x++; } return 0; }", "", null, "laço infinito"),
        new("erro: chave não fechada", "int main(){\n  if (1) {\n    printf(\"x\");\n  return 0;\n}", "", null, "não foi fechada"),
        new("erro: const não pode mudar", "int main(){ const int LIMITE = 3; LIMITE = 4; return 0; }", "", null, "constante"),
        new("erro: free inválido", "#include <stdlib.h>\nint main(){ int x; free(&x); return 0; }", "", null, "malloc"),
        new("erro: printf com valores de menos", "int main(){ printf(\"%d %d\\n\", 1); return 0; }", "", null, "mais valores"),
        new("erro: string sem fechar", "int main(){ printf(\"oi); return 0; }", "", null, "não foi fechado"),
        new("erro: sem main", "int soma(int a, int b){ return a + b; }", "", null, "main"),
    };

    static void RunCTests()
    {
        foreach (var c in CCases)
        {
            var timeout = c.ErrorContains == "laço infinito" ? TimeSpan.FromSeconds(1) : TimeSpan.FromSeconds(10);
            RunResult r;
            try { r = CRunner.Run(c.Code, c.Stdin, timeout); }
            catch (Exception e) { Fail("C: " + c.Name, "o runner lançou exceção: " + e.Message); continue; }
            if (c.Expected != null)
            {
                bool ok = r.Ok && Norm(r.Stdout) == Norm(c.Expected);
                Check("C: " + c.Name, ok, "esperado [" + c.Expected + "] obtido [" + Norm(r.Stdout) + "] erro=" + r.Error);
            }
            else
            {
                bool ok = !r.Ok && r.Error.Contains(c.ErrorContains!, StringComparison.OrdinalIgnoreCase);
                Check("C: " + c.Name, ok, "esperado erro contendo '" + c.ErrorContains + "', obtido ok=" + r.Ok + " erro=" + r.Error);
            }
        }

        // the async runner (thread with big stack) and the ILanguageRunner contract
        var runner = new CRunner();
        var res = runner.RunAsync("int main(){ printf(\"%d\\n\", 6*7); return 0; }", "", TimeSpan.FromSeconds(5)).GetAwaiter().GetResult();
        Check("CRunner.RunAsync", res.Ok && Norm(res.Stdout) == "42" && runner.LanguageId == "c" && !runner.IsExternal, res.Error);

        // cancellation
        using var cts = new CancellationTokenSource(200);
        var rc = runner.RunAsync("int main(){ for(;;){} }", "", TimeSpan.FromSeconds(30), cts.Token).GetAwaiter().GetResult();
        Check("CRunner cancelamento", !rc.Ok && rc.Error.Contains("cancelada"), rc.Error);

        // deep but legal recursion works
        var deep = CRunner.Run("int s(int n){ return n == 0 ? 0 : 1 + s(n - 1); } int main(){ printf(\"%d\\n\", s(5000)); return 0; }", "", TimeSpan.FromSeconds(10));
        Check("C: recursão profunda legítima (5000)", deep.Ok && Norm(deep.Stdout) == "5000", deep.Error);
    }

    // ===================================================================== hooks
    static void RunHookTests()
    {
        var ip = new CInterpreter();
        ip.AddNative("dobro", (i, a) => CVal.Int(a[0].AsLong * 2));
        var cls = ip.AddClass("Contador", (i, a) => 1);
        long total = 0;
        cls.Methods["soma"] = (i, h, a) => { total += a[0].AsLong; return CVal.LongV(total); };
        ip.AddNativeObject("cont", cls);
        ip.DefineMacro("DEZ", "10");
        try
        {
            ip.Compile("Contador outro; int main(){ cont.soma(dobro(DEZ)); outro.soma(1); printf(\"%ld\\n\", cont.soma(0)); return 0; }");
            ip.RunMain();
            Check("hook: funções e classes nativas", Norm(ip.Out.ToString()) == "21", "saída " + ip.Out);
        }
        catch (CError e) { Fail("hook: funções e classes nativas", e.Friendly); }

        var ip2 = new CInterpreter();
        try
        {
            ip2.Compile("int main(){ Serial.begin(9600); return 0; }");
            Fail("hook: objeto desconhecido dá erro", "compilou sem Serial registrado");
        }
        catch (CError e) { Check("hook: objeto desconhecido dá erro", e.Msg.Contains("Serial"), e.Friendly); }
    }

    // ===================================================================== simulator basics
    static List<CheckOutcome> Grade(List<ComponentSpec> comps, List<SimCheck> checks, string sketch) =>
        new ArduinoSimulator().Grade(comps, checks, sketch);

    static ComponentSpec Comp(string id, string type, params (string k, int v)[] pins) =>
        new() { Id = id, Type = type, Pins = pins.ToDictionary(p => p.k, p => p.v) };

    static SimCheck Chk(string kind, string target, string value, int atMs, params string[] stim) =>
        new() { Kind = kind, Target = target, Value = value, AtMs = atMs, Stimulus = stim.ToList() };

    static void RunSimulatorTests()
    {
        var led = new List<ComponentSpec> { Comp("led1", "led", ("pin", 13)) };

        // missing pinMode: warning + LED stays off
        {
            var b = new ArduinoBoard(led);
            var ip = new CInterpreter();
            b.Attach(ip);
            ip.Compile("void setup(){ digitalWrite(13, HIGH); } void loop(){ delay(100); }");
            b.AfterAdvance = () => { if (b.TimeUs > 300_000) throw new CHalt(); };
            try { ip.Call("setup"); while (true) ip.Call("loop"); } catch (CHalt) { }
            var s = b.GetComponents()[0];
            Check("sim: digitalWrite sem pinMode avisa", !s.On && b.Warnings.Any(w => w.Contains("pinMode")), string.Join(";", b.Warnings));
        }

        // virtual time: delay(1000) does not sleep in real time
        {
            var sw = Stopwatch.StartNew();
            var r = Grade(led, new List<SimCheck> { Chk("pin", "13", "HIGH", 59_500) },
                "void setup(){ pinMode(13, OUTPUT); } void loop(){ digitalWrite(13, HIGH); delay(60000); }");
            Check("sim: tempo virtual (60 s simulados rápido)", r[0].Passed && sw.Elapsed.TotalSeconds < 10, r[0].Detail);
        }

        // busy-wait with millis() advances time too
        {
            var r = Grade(led, new List<SimCheck> { Chk("pin", "13", "HIGH", 1500), Chk("pin", "13", "LOW", 500) },
                "unsigned long t0; void setup(){ pinMode(13, OUTPUT); t0 = millis(); } void loop(){ if (millis() - t0 > 1000) digitalWrite(13, HIGH); }");
            Check("sim: espera ocupada com millis()", r.All(x => x.Passed), string.Join(" / ", r.Select(x => x.Detail)));
        }

        // infinite loop in setup still ends (virtual time keeps going)
        {
            var r = Grade(led, new List<SimCheck> { Chk("pin", "13", "HIGH", 300) },
                "void setup(){ pinMode(13, OUTPUT); digitalWrite(13, HIGH); while (1) { } } void loop(){ }");
            Check("sim: while(1) no setup não trava a correção", r[0].Passed, r[0].Detail);
        }

        // compile error → every check fails with the message
        {
            var r = Grade(led, new List<SimCheck> { Chk("pin", "13", "HIGH", 300), Chk("pin", "13", "LOW", 600) },
                "void setup(){ pinMode(13, OUTPUT) }\nvoid loop(){}");
            Check("sim: erro de compilação vira mensagem", r.All(x => !x.Passed && x.Detail.Contains("';'")), r[0].Detail);
        }

        // missing loop()
        {
            var r = Grade(led, new List<SimCheck> { Chk("pin", "13", "HIGH", 300) }, "void setup(){ }");
            Check("sim: falta loop()", !r[0].Passed && r[0].Detail.Contains("loop"), r[0].Detail);
        }

        // runtime error in loop
        {
            var r = Grade(led, new List<SimCheck> { Chk("pin", "13", "HIGH", 300) },
                "int v[3]; int i = 0; void setup(){ } void loop(){ v[i] = 1; i++; delay(10); }");
            Check("sim: erro de execução vira mensagem", !r[0].Passed && r[0].Detail.Contains("fora dos limites"), r[0].Detail);
        }

        // Serial without begin
        {
            var r = Grade(led, new List<SimCheck> { Chk("serial", "", "oi", 300) },
                "void setup(){ Serial.println(\"oi\"); } void loop(){ }");
            Check("sim: Serial sem begin não mostra nada", !r[0].Passed && r[0].Detail.Contains("Serial.begin"), r[0].Detail);
        }

        // Serial formatting
        {
            var r = Grade(led, new List<SimCheck> { Chk("serial", "", "x=7 f=3.14 h=FF b=101 c=A", 300) },
                "void setup(){ Serial.begin(9600); Serial.print(\"x=\"); Serial.print(7); Serial.print(\" f=\"); Serial.print(3.14159); Serial.print(\" h=\"); Serial.print(255, HEX); Serial.print(\" b=\"); Serial.print(5, BIN); Serial.print(\" c=\"); Serial.println('A'); } void loop(){ }");
            Check("sim: formatos do Serial.print", r[0].Passed, r[0].Detail);
        }

        // RGB LED colour names and PWM pins
        {
            var comps = new List<ComponentSpec> { Comp("rgb", "rgbled", ("r", 9), ("g", 10), ("b", 11)) };
            var r = Grade(comps, new List<SimCheck> { Chk("component", "rgb", "yellow", 100), Chk("component", "rgb.g", "100", 100) },
                "void setup(){ analogWrite(9, 255); analogWrite(10, 100); analogWrite(11, 0); } void loop(){ }");
            Check("sim: LED RGB por nome de cor", r.All(x => x.Passed), string.Join(" / ", r.Select(x => x.Detail)));
        }

        // analogWrite on a non-PWM pin warns
        {
            var b = new ArduinoBoard(led);
            var ip = new CInterpreter();
            b.Attach(ip);
            ip.Compile("void setup(){ analogWrite(13, 100); } void loop(){ }");
            ip.Call("setup");
            Check("sim: analogWrite em pino sem PWM avisa", b.Warnings.Any(w => w.Contains("PWM")) && b.EffOut(13) == 0, string.Join(";", b.Warnings));
        }

        // comparison syntax
        Check("sim: Match faixas e operadores",
            ArduinoSimulator.Match(">=10", "12", 12, 0, false, out _) && ArduinoSimulator.Match("80..120", "100", 100, 0, false, out _)
            && !ArduinoSimulator.Match("<5", "7", 7, 0, false, out _) && ArduinoSimulator.Match("HIGH", "HIGH", 255, 0, false, out _)
            && ArduinoSimulator.Match("off", "off", 0, 0, false, out _) && ArduinoSimulator.Match("!=0", "3", 3, 0, false, out _));
    }

    // ===================================================================== lessons
    public static string? FindContent(string relative)
    {
        var dirs = new List<string> { AppContext.BaseDirectory, Directory.GetCurrentDirectory() };
        foreach (var start in dirs)
        {
            for (var d = new DirectoryInfo(start); d != null; d = d.Parent)
            {
                var p = Path.Combine(d.FullName, relative);
                if (File.Exists(p)) return p;
            }
        }
        return null;
    }

    static void RunLessonTests()
    {
        var path = FindContent(Path.Combine("Content", "robotics", "arduino.json"));
        if (path == null) { Fail("lições: arquivo", "Content/robotics/arduino.json não encontrado"); return; }
        Track? track;
        try { track = JsonSerializer.Deserialize<Track>(File.ReadAllText(path)); }
        catch (Exception e) { Fail("lições: JSON", e.Message); return; }
        if (track == null) { Fail("lições: JSON", "vazio"); return; }
        Check("lições: trilha robotics/c", track.Area == "robotics" && track.Language == "c" && track.Id.Length > 0 && track.Lessons.Count >= 10,
            $"area={track.Area} language={track.Language} lessons={track.Lessons.Count}");
        var ids = new HashSet<string>();
        int prevLevel = 0;
        var sim = new ArduinoSimulator();
        foreach (var l in track.Lessons)
        {
            string n = "lição " + l.Id;
            bool fields = l.Title.Length > 0 && l.Theory.Length > 200 && l.Task.Length > 0 && l.Starter.Length > 0 && l.Solution.Length > 0
                          && l.Hints.Count >= 2 && l.Hints.Count <= 3 && l.Components.Count > 0 && l.Checks.Count > 0 && ids.Add(l.Id)
                          && l.Level >= 1 && l.Level <= 5 && l.Level >= prevLevel;
            Check(n + ": campos completos", fields, "título/teoria/tarefa/dicas/componentes/verificações/nível");
            prevLevel = l.Level;
            var sol = sim.GradeAsync(l, l.Solution).GetAwaiter().GetResult();
            var bad = sol.Where(o => !o.Passed).ToList();
            Check(n + ": a solução passa em tudo", bad.Count == 0, string.Join(" | ", bad.Select(b => b.Description + ": " + b.Detail)));
            var st = sim.GradeAsync(l, l.Starter).GetAwaiter().GetResult();
            Check(n + ": o código inicial falha em algo", st.Any(o => !o.Passed), "o código inicial passou em todas as verificações");
        }
    }

    // ===================================================================== live session
    static void RunSessionTests()
    {
        var comps = new List<ComponentSpec> { Comp("led1", "led", ("pin", 13)), Comp("btn1", "button", ("pin", 2)) };
        using (var s = new ArduinoSession())
        {
            bool ok = s.Start("void setup(){ pinMode(13, OUTPUT); pinMode(2, INPUT_PULLUP); Serial.begin(9600); Serial.println(\"pronto\"); }\n" +
                              "void loop(){ digitalWrite(13, digitalRead(2) == LOW ? HIGH : LOW); delay(5); }", comps);
            Check("sessão: Start", ok && s.IsRunning, s.Error ?? "");
            s.SpeedFactor = 4;
            for (int i = 0; i < 10; i++) s.Tick(16);
            bool off = s.GetComponent("led1")?.On == false;
            s.PressButton("btn1", true);
            for (int i = 0; i < 5; i++) s.Tick(16);
            bool on = s.GetComponent("led1")?.On == true;
            Check("sessão: botão ao vivo acende o LED", off && on && s.TimeMs > 900 && s.SerialText.Contains("pronto"),
                $"off={off} on={on} t={s.TimeMs} serial={s.SerialText}");
            Check("sessão: pinos expostos", s.GetPins().Length == 20 && s.GetPins()[13].Value == 255);
            s.Stop();
            Check("sessão: Stop", !s.IsRunning);
        }
        using (var s = new ArduinoSession())
        {
            bool ok = s.Start("void setup(){ int x = 1 }", comps);
            Check("sessão: erro de compilação", !ok && s.Error != null && s.Error.Contains("';'"), s.Error ?? "");
        }
        using (var s = new ArduinoSession())
        {
            s.Start("int v[2]; int i; void setup(){ } void loop(){ v[i++] = 1; delay(100); }", comps);
            for (int i = 0; i < 30 && s.IsRunning; i++) s.Tick(50);
            Check("sessão: erro de execução", !s.IsRunning && s.Error != null && s.Error.Contains("fora dos limites"), s.Error ?? "(sem erro)");
        }
        // robot arena exposed
        var robot = new List<ComponentSpec> { new() { Id = "r", Type = "robot", Color = "obstacles", Pins = new() { ["in1"] = 8, ["in2"] = 7, ["ena"] = 5, ["in3"] = 4, ["in4"] = 2, ["enb"] = 6, ["trig"] = 10, ["echo"] = 11, ["lineL"] = 14, ["lineR"] = 15 } } };
        using (var s = new ArduinoSession())
        {
            s.Start("void setup(){ pinMode(8,OUTPUT); pinMode(7,OUTPUT); pinMode(4,OUTPUT); pinMode(2,OUTPUT); digitalWrite(8,HIGH); digitalWrite(4,HIGH); analogWrite(5,200); analogWrite(6,200); } void loop(){ }", robot);
            for (int i = 0; i < 30; i++) s.Tick(33);
            var r = s.GetRobot();
            Check("sessão: robô anda e arena exposta", r != null && r.X > s.Arena!.Start.X + 5 && s.Arena.Obstacles.Count > 0, r == null ? "sem robô" : $"x={r.X}");
        }
    }
}
