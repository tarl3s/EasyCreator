namespace EasyCreator.Exercises;

/// <summary>Lessons py-13 / js-17 (stacks and queues; bracket matching, postfix notation): programs that need a stack, and tracing stack / queue operations and RPN.</summary>
[Exercises("py-13", "js-17")]
public sealed class P14Stacks : IExerciseGenerator
{
    public Exercise Make(Lesson lesson, Difficulty d, Random r)
    {
        bool js = Pg.Js(lesson);
        int lv = (int)d;
        return r.Next(3) != 0 ? MakeCode(js, lv, r) : MakeOut(js, lv, r);
    }

    // A random postfix expression with + - * over small numbers whose evaluation never fails.
    static string Rpn(Random r, int ops, out long value)
    {
        var st = new List<long>(); var toks = new List<string>(); int need = ops + 1, pushed = 0, done = 0;
        while (done < ops || pushed < need)
        {
            bool canOp = st.Count >= 2 && done < ops, canPush = pushed < need;
            if (canPush && (!canOp || r.Next(2) == 0)) { int v = Ex.Int(r, 1, 9); st.Add(v); toks.Add(v.ToString()); pushed++; }
            else { string op = Ex.Pick(r, "+", "-", "*"); long b = st[^1], a = st[^2]; if (op == "*" && (a == 0 || b == 0) && (a < 0 || b < 0)) op = "+"; st.RemoveRange(st.Count - 2, 2); st.Add(op == "+" ? a + b : op == "-" ? a - b : a * b); toks.Add(op); done++; }
        }
        value = st[0];
        return string.Join(" ", toks);
    }

    static string Balanced(string s, string kinds, out string verdict)
    {
        var st = new Stack<char>();
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            int o = kinds.IndexOf(c);
            if (o >= 0 && o % 2 == 0) st.Push(c);
            else if (o >= 0) { if (st.Count == 0 || kinds.IndexOf(st.Peek()) + 1 != o) { verdict = $"Erro na coluna {i + 1}"; return verdict; } st.Pop(); }
        }
        verdict = st.Count > 0 ? "Faltou fechar" : "OK";
        return verdict;
    }

    static string BrString(Random r, string kinds, int len)
    {
        var sb = new System.Text.StringBuilder(); var st = new Stack<char>();
        for (int i = 0; i < len; i++)
        {
            if (st.Count > 0 && r.Next(2) == 0) { sb.Append(kinds[kinds.IndexOf(st.Pop()) + 1]); }
            else { int k = r.Next(kinds.Length / 2) * 2; st.Push(kinds[k]); sb.Append(kinds[k]); }
            if (r.Next(4) == 0) sb.Append((char)('a' + r.Next(5)));
        }
        while (st.Count > 0) sb.Append(kinds[kinds.IndexOf(st.Pop()) + 1]);
        var res = sb.ToString();
        if (r.Next(3) == 0 && res.Length > 2) { int p = r.Next(res.Length); var arr = res.ToCharArray(); arr[p] = kinds[r.Next(kinds.Length)]; res = new string(arr); }
        return res;
    }

    static Exercise MakeCode(bool js, int lv, Random r)
    {
        switch (lv)
        {
            case 0:
            {
                var tests = Pg.Tests(new[] { "a", "casa", "banana", Ex.Pick(r, Pg.Words), Ex.Pick(r, Pg.Words) + Ex.Pick(r, Pg.Words) }, i => new string(i.Trim().Reverse().ToArray()));
                string sol = js ? Pg.Code(Pg.ReadStr(true, "s"), "const pilha = [];", "for (const c of s) pilha.push(c);", "let saida = \"\";", "while (pilha.length > 0) saida += pilha.pop();", "console.log(saida);")
                    : Pg.Code(Pg.ReadStr(false, "s"), "pilha = []", "for c in s:", "    pilha.append(c)", "saida = \"\"", "while len(pilha) > 0:", "    saida += pilha.pop()", "print(saida)");
                return Ex.Code(Pg.Prompt(L.T("Leia uma palavra e mostre-a ao contrário, usando uma PILHA: empilhe cada letra na ordem em que aparecem e depois desempilhe todas, formando a saída."), tests[0]),
                    Pg.Starter(js, Pg.ReadStr(js, "s")), sol, tests,
                    L.T("Uma pilha devolve primeiro o que entrou por último (LIFO). Empilhar as letras e desempilhar tudo inverte a ordem. append/push põe no topo; pop tira do topo."));
            }
            case 1:
            {
                string kinds = "()";
                var tests = Pg.Tests(new[] { "(a(b)c)", ")(", BrString(r, kinds, 3), BrString(r, kinds, 4), BrString(r, kinds, 5) }, i => { Balanced(i.Trim(), kinds, out var v); return v == "OK" ? "OK" : "ERRO"; });
                string sol = js ? Pg.Code(Pg.ReadStr(true, "s"), "let abertos = 0, ok = true;", "for (const c of s) {", "  if (c === \"(\") abertos++;", "  else if (c === \")\") {", "    if (abertos === 0) ok = false;", "    else abertos--;", "  }", "}", "console.log(ok && abertos === 0 ? \"OK\" : \"ERRO\");")
                    : Pg.Code(Pg.ReadStr(false, "s"), "abertos = 0", "ok = True", "for c in s:", "    if c == \"(\":", "        abertos += 1", "    elif c == \")\":", "        if abertos == 0:", "            ok = False", "        else:", "            abertos -= 1", "print(\"OK\" if ok and abertos == 0 else \"ERRO\")");
                return Ex.Code(Pg.Prompt(L.T("Leia uma linha de texto (pode ter letras). Mostre `OK` se os parênteses `(` e `)` estão balanceados — cada `)` fecha um `(` anterior e no fim nada fica aberto — ou `ERRO` se não."), tests[0]),
                    Pg.Starter(js, Pg.ReadStr(js, "s")), sol, tests,
                    L.T("Com um só tipo de parêntese a pilha vira um simples contador de abertos: `(` soma 1, `)` tira 1 (e se não há aberto, é erro). No fim, o contador precisa ser 0."));
            }
            case 2:
            {
                if (r.Next(2) == 0)
                {
                    string kinds = "()[]{}";
                    var tests = Pg.Tests(new[] { "([)]", "((", "{[()]}", BrString(r, kinds, 4), BrString(r, kinds, 5), BrString(r, kinds, 6) }, i => { Balanced(i.Trim(), kinds, out var v); return v; });
                    string sol = js ? Pg.Code(Pg.ReadStr(true, "s"), "const par = { \")\": \"(\", \"]\": \"[\", \"}\": \"{\" };", "const pilha = [];", "let res = \"\";", "for (let i = 0; i < s.length && res === \"\"; i++) {", "  const c = s[i];", "  if (\"([{\".includes(c)) pilha.push(c);", "  else if (c in par) {", "    if (pilha.length === 0 || pilha[pilha.length - 1] !== par[c]) res = `Erro na coluna ${i + 1}`;", "    else pilha.pop();", "  }", "}", "if (res === \"\") res = pilha.length > 0 ? \"Faltou fechar\" : \"OK\";", "console.log(res);")
                        : Pg.Code(Pg.ReadStr(false, "s"), "par = {\")\": \"(\", \"]\": \"[\", \"}\": \"{\"}", "pilha = []", "res = \"\"", "for i in range(len(s)):", "    c = s[i]", "    if c in \"([{\":", "        pilha.append(c)", "    elif c in par:", "        if len(pilha) == 0 or pilha[-1] != par[c]:", "            res = \"Erro na coluna \" + str(i + 1)", "            break", "        pilha.pop()", "if res == \"\":", "    res = \"Faltou fechar\" if len(pilha) > 0 else \"OK\"", "print(res)");
                    return Ex.Code(Pg.Prompt(L.T("Leia uma linha de texto. Os símbolos `()`, `[]` e `{}` precisam estar balanceados (os outros caracteres são ignorados). As colunas contam a partir de 1. Mostre `OK` se está tudo certo; `Erro na coluna <c>` no PRIMEIRO fechamento que não combina com o último aberto (ou que não tem nenhum aberto); ou `Faltou fechar` se a linha acabou com símbolos ainda abertos."), tests[0]),
                        Pg.Starter(js, Pg.ReadStr(js, "s")), sol, tests,
                        L.T("Cada abertura vai para a pilha. Cada fechamento precisa combinar com o TOPO da pilha: se combina, desempilha; se não (ou se a pilha está vazia), é o primeiro erro. Se a linha acaba e a pilha não está vazia, faltou fechar."));
                }
                else
                {
                    var ins = new List<string> { "3 4 +" };
                    var vals = new Dictionary<string, long>();
                    for (int i = 0; i < 5; i++) { var e = Rpn(r, Ex.Int(r, 1, 4), out var v); ins.Add(e); vals[e] = v; }
                    vals["3 4 +"] = 7;
                    var tests = Pg.Tests(ins, i => vals[i.Trim()].ToString());
                    string sol = js ? Pg.Code(Pg.ReadStr(true, "linha"), "const pilha = [];", "for (const t of linha.split(\" \")) {", "  if (t === \"+\" || t === \"-\" || t === \"*\") {", "    const b = pilha.pop();", "    const a = pilha.pop();", "    pilha.push(t === \"+\" ? a + b : t === \"-\" ? a - b : a * b);", "  } else {", "    pilha.push(Number(t));", "  }", "}", "console.log(pilha[0]);")
                        : Pg.Code(Pg.ReadStr(false, "linha"), "pilha = []", "for t in linha.split():", "    if t in (\"+\", \"-\", \"*\"):", "        b = pilha.pop()", "        a = pilha.pop()", "        pilha.append(a + b if t == \"+\" else a - b if t == \"-\" else a * b)", "    else:", "        pilha.append(int(t))", "print(pilha[0])");
                    return Ex.Code(Pg.Prompt(L.T("Notação polonesa reversa (RPN): os operadores vêm DEPOIS dos números. Por exemplo, `3 4 + 2 *` significa (3 + 4) × 2 = 14. Leia uma linha com uma expressão RPN (números inteiros de 1 dígito e os operadores `+`, `-`, `*`, separados por espaço) e mostre o valor dela."), tests[0]),
                        Pg.Starter(js, Pg.ReadStr(js, "linha")), sol, tests,
                        L.T("Lemos da esquerda para a direita: número → empilha; operador → tira os dois do topo (o primeiro tirado é o operando da DIREITA, b; o segundo é o da esquerda, a), calcula a op b e empilha o resultado. No fim, a pilha tem só o valor final."));
                }
            }
            default:
            {
                bool temps = r.Next(2) == 0;
                static string Inp(int[] v) => v.Length + "\n" + string.Join(" ", v);
                var ins = new List<string> { Inp(new[] { 2, 7, 4, 3, 5 }) };
                for (int i = 0; i < 4; i++) ins.Add(Inp(Pg.Arr(r, Ex.Int(r, 4, 9), 1, 20)));
                string Model(string inp) { var v = inp.TrimEnd('\n').Split('\n')[1].Split(' ').Select(int.Parse).ToArray(); var o = new List<int>(); for (int i = 0; i < v.Length; i++) { int res = temps ? 0 : -1; for (int j = i + 1; j < v.Length; j++) if (v[j] > v[i]) { res = temps ? j - i : v[j]; break; } o.Add(res); } return string.Join(" ", o); }
                var tests = Pg.Tests(ins, Model);
                string rd = Pg.Code(Pg.ReadInt(js, "n"), Pg.ReadInts(js, "v"));
                string res = temps ? (js ? "res[p] = i - p;" : "res[p] = i - p") : (js ? "res[p] = v[i];" : "res[p] = v[i]");
                string zero = temps ? "0" : "-1";
                string sol = js ? Pg.Code(rd, $"const res = new Array(n).fill({zero});", "const pilha = [];", "for (let i = 0; i < n; i++) {", "  while (pilha.length > 0 && v[pilha[pilha.length - 1]] < v[i]) {", "    const p = pilha.pop();", "    " + res, "  }", "  pilha.push(i);", "}", "console.log(res.join(\" \"));")
                    : Pg.Code(rd, $"res = [{zero}] * n", "pilha = []", "for i in range(n):", "    while len(pilha) > 0 and v[pilha[-1]] < v[i]:", "        p = pilha.pop()", "        " + res, "    pilha.append(i)", "print(\" \".join(str(x) for x in res))");
                return Ex.Code(Pg.Prompt((temps ? L.T("Leia `n` e uma linha com `n` temperaturas inteiras (uma por dia). Para cada dia, mostre quantos dias é preciso esperar até um dia MAIS QUENTE (estritamente maior); 0 se isso nunca acontece. Escreva os resultados numa linha, separados por espaço.")
                    : L.T("Leia `n` e uma linha com `n` inteiros. Para cada posição, mostre o PRIMEIRO valor maior (estritamente) que aparece à sua direita, ou -1 se não há nenhum. Escreva os resultados numa linha, separados por espaço.")), tests[0]),
                    rd + "\n" + Pg.Comment(js, L.T("escreva o resto do programa aqui")), sol, tests,
                    L.T("Truque: uma pilha de posições \"ainda sem resposta\". Ao chegar um valor maior que o do topo, ele é a resposta do topo (desempilhamos e anotamos); repetimos enquanto der. Depois empilhamos a posição atual. Cada posição entra e sai uma vez: O(n), em vez de comparar todos os pares."));
            }
        }
    }

    static Exercise MakeOut(bool js, int lv, Random r)
    {
        string pr(string e) => js ? $"console.log({e});" : $"print({e})";
        if (lv <= 2 && !(lv == 2 && r.Next(2) == 0))
        {
            bool queue = lv >= 1;
            var lines = new List<string> { js ? (queue ? "const f = [];" : "const s = [];") : (queue ? "f = []" : "s = []") };
            var model = new List<int>(); var outs = new List<string>(); int ops = Ex.Int(r, 6, 9);
            string nm = queue ? "f" : "s";
            for (int i = 0; i < ops; i++)
            {
                int pick = model.Count == 0 ? 0 : r.Next(5);
                if (pick <= 1) { int v = Ex.Int(r, 1, 9); lines.Add(js ? $"{nm}.push({v});" : $"{nm}.append({v})"); model.Add(v); }
                else if (pick == 2) { lines.Add(pr(queue ? (js ? $"{nm}.shift()" : $"{nm}.pop(0)") : $"{nm}.pop()")); outs.Add((queue ? model[0] : model[^1]).ToString()); model.RemoveAt(queue ? 0 : model.Count - 1); }
                else if (pick == 3) { lines.Add(pr(queue ? $"{nm}[0]" : (js ? $"{nm}[{nm}.length - 1]" : $"{nm}[-1]"))); outs.Add((queue ? model[0] : model[^1]).ToString()); }
                else { lines.Add(pr(js ? $"{nm}.length" : $"len({nm})")); outs.Add(model.Count.ToString()); }
            }
            if (outs.Count == 0) { lines.Add(pr(js ? $"{nm}.length" : $"len({nm})")); outs.Add(model.Count.ToString()); }
            string outp = string.Join("\n", outs);
            return Ex.Output(PgOut.Prompt(outp), string.Join("\n", lines), outp, queue ? L.T("Fila (FIFO): quem entra primeiro sai primeiro. push/append põe no FIM; shift/pop(0) tira e mostra o do INÍCIO; f[0] só espia o início.") : L.T("Pilha (LIFO): o último a entrar é o primeiro a sair. push/append põe no topo; pop tira e mostra o topo; s[-1] só espia o topo, sem tirar."));
        }
        {
            int ops = lv == 2 ? Ex.Int(r, 2, 3) : Ex.Int(r, 3, 4);
            string e = Rpn(r, ops, out long val);
            var st = new List<long>(); int maxs = 0;
            foreach (var t in e.Split(' ')) { if (t is "+" or "-" or "*") { long b = st[^1], a = st[^2]; st.RemoveRange(st.Count - 2, 2); st.Add(t == "+" ? a + b : t == "-" ? a - b : a * b); } else st.Add(long.Parse(t)); maxs = Math.Max(maxs, st.Count); }
            string code = js ? $"const pilha = [];\nlet maximo = 0;\nfor (const t of \"{e}\".split(\" \")) {{\n  if (t === \"+\" || t === \"-\" || t === \"*\") {{\n    const b = pilha.pop();\n    const a = pilha.pop();\n    pilha.push(t === \"+\" ? a + b : t === \"-\" ? a - b : a * b);\n  }} else {{\n    pilha.push(Number(t));\n  }}\n  maximo = Math.max(maximo, pilha.length);\n}}\nconsole.log(pilha[0], maximo);"
                : $"pilha = []\nmaximo = 0\nfor t in \"{e}\".split():\n    if t in (\"+\", \"-\", \"*\"):\n        b = pilha.pop()\n        a = pilha.pop()\n        pilha.append(a + b if t == \"+\" else a - b if t == \"-\" else a * b)\n    else:\n        pilha.append(int(t))\n    maximo = max(maximo, len(pilha))\nprint(pilha[0], maximo)";
            string outp = $"{val} {maxs}";
            return Ex.Output(PgOut.Prompt(outp), code, outp, L.F("A expressão RPN `{0}` vale {1}. A linha mostra o valor final e o MAIOR tamanho que a pilha chegou a ter ({2}): cada número empilha, cada operador troca dois itens por um.", e, val, maxs));
        }
    }
}
