using System.Globalization;
using System.Text;

namespace EasyCreator.CInterp;

public enum TokKind { Ident, Int, Float, Char, String, Punct, EOF }

public sealed class Token
{
    public TokKind K;
    public string Text = "";
    public long I;
    public double F;
    public bool Unsigned, LongSfx, FloatSfx;
    public int Line;

    public Token At(int line)
    {
        var t = (Token)MemberwiseClone();
        t.Line = line;
        return t;
    }

    public override string ToString() => K switch
    {
        TokKind.EOF => L.T("fim do arquivo"),
        TokKind.String => "\"" + Text + "\"",
        TokKind.Char => "'" + Text + "'",
        _ => Text
    };
}

/// <summary>A compile or runtime error with a friendly Portuguese message.</summary>
public sealed class CError : Exception
{
    public int Line { get; }
    public bool Runtime { get; }
    public string Msg { get; }

    public CError(int line, string msg, bool runtime) : base(msg)
    {
        Line = line; Msg = msg; Runtime = runtime;
    }

    public string Friendly =>
        Line > 0
            ? L.F(Runtime ? "Erro de execução (linha {0}): {1}" : "Erro de compilação (linha {0}): {1}", Line, Msg)
            : L.F(Runtime ? "Erro de execução: {0}" : "Erro de compilação: {0}", Msg);

    public override string ToString() => Friendly;
}

public sealed class Macro
{
    public string Name = "";
    public List<string>? Params;      // null = object-like
    public List<Token> Body = new();
}

/// <summary>Preprocessor + tokenizer: strips comments, handles #define/#undef/#include/#if*, expands macros.</summary>
public static class CLexer
{
    static readonly string[] Puncts =
    {
        ">>=", "<<=", "...", "->", "++", "--", "<<", ">>", "<=", ">=", "==", "!=", "&&", "||",
        "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "::",
        "+", "-", "*", "/", "%", "=", "<", ">", "!", "~", "&", "|", "^", "?", ":", ";", ",", ".",
        "(", ")", "[", "]", "{", "}", "#"
    };

    static CError Err(int line, string msg) => new(line, msg, false);

    public static List<Token> Tokenize(string src, Dictionary<string, Macro> macros)
    {
        string text = StripComments(src.Replace("\r\n", "\n").Replace('\r', '\n'));
        var rawLines = text.Split('\n');
        var output = new List<Token>();
        // condition stack: (parentActive, active, taken)
        var cond = new Stack<(bool parent, bool active, bool taken)>();
        bool Active() => cond.Count == 0 || cond.Peek().active;

        for (int li = 0; li < rawLines.Length; li++)
        {
            int lineNo = li + 1;
            string line = rawLines[li];
            while (line.EndsWith("\\") && li + 1 < rawLines.Length)
            {
                line = line[..^1] + " " + rawLines[++li];
            }
            string trimmed = line.TrimStart();
            if (trimmed.StartsWith("#"))
            {
                Directive(trimmed[1..].TrimStart(), lineNo, macros, cond, Active());
                continue;
            }
            if (!Active()) continue;
            var lineToks = new List<Token>();
            LexLine(line, lineNo, lineToks);
            Expand(lineToks, output, new HashSet<string>(), macros, lineNo);
        }
        if (cond.Count > 0) throw Err(rawLines.Length, L.T("faltou um #endif"));

        // merge adjacent string literals
        var merged = new List<Token>(output.Count + 1);
        foreach (var t in output)
        {
            if (t.K == TokKind.String && merged.Count > 0 && merged[^1].K == TokKind.String)
            {
                var prev = merged[^1].At(merged[^1].Line);
                prev.Text += t.Text;
                merged[^1] = prev;
            }
            else merged.Add(t);
        }
        merged.Add(new Token { K = TokKind.EOF, Text = "", Line = rawLines.Length });
        return merged;
    }

    static void Directive(string d, int line, Dictionary<string, Macro> macros,
        Stack<(bool parent, bool active, bool taken)> cond, bool active)
    {
        int i = 0;
        while (i < d.Length && char.IsLetter(d[i])) i++;
        string name = d[..i];
        string rest = d[i..].Trim();
        switch (name)
        {
            case "ifdef":
            case "ifndef":
                {
                    bool def = macros.ContainsKey(FirstIdent(rest));
                    bool v = name == "ifdef" ? def : !def;
                    cond.Push((active, active && v, v));
                    return;
                }
            case "if":
                {
                    bool v = active && EvalIf(rest, line, macros) != 0;
                    cond.Push((active, active && v, v));
                    return;
                }
            case "elif":
                {
                    if (cond.Count == 0) throw Err(line, L.T("#elif sem #if"));
                    var top = cond.Pop();
                    bool v = !top.taken && top.parent && EvalIf(rest, line, macros) != 0;
                    cond.Push((top.parent, top.parent && v, top.taken || v));
                    return;
                }
            case "else":
                {
                    if (cond.Count == 0) throw Err(line, L.T("#else sem #if"));
                    var top = cond.Pop();
                    cond.Push((top.parent, top.parent && !top.taken, true));
                    return;
                }
            case "endif":
                if (cond.Count == 0) throw Err(line, L.T("#endif sem #if"));
                cond.Pop();
                return;
        }
        if (!active) return;
        switch (name)
        {
            case "include":
            case "pragma":
            case "line":
            case "warning":
                return;
            case "error":
                throw Err(line, L.F("#error {0}", rest));
            case "undef":
                macros.Remove(FirstIdent(rest));
                return;
            case "define":
                {
                    int j = 0;
                    while (j < rest.Length && (char.IsLetterOrDigit(rest[j]) || rest[j] == '_')) j++;
                    string mname = rest[..j];
                    if (mname.Length == 0) throw Err(line, L.T("#define precisa de um nome"));
                    var m = new Macro { Name = mname };
                    string body;
                    if (j < rest.Length && rest[j] == '(')
                    {
                        int close = rest.IndexOf(')', j);
                        if (close < 0) throw Err(line, L.T("#define com parênteses não fechados"));
                        m.Params = rest[(j + 1)..close].Split(',').Select(s => s.Trim()).Where(s => s.Length > 0).ToList();
                        body = rest[(close + 1)..];
                    }
                    else body = rest[j..];
                    LexLine(body, line, m.Body);
                    macros[mname] = m;
                    return;
                }
            default:
                throw Err(line, L.F("diretiva desconhecida #{0}", name));
        }
    }

    static string FirstIdent(string s)
    {
        int j = 0;
        while (j < s.Length && (char.IsLetterOrDigit(s[j]) || s[j] == '_')) j++;
        return s[..j];
    }

    // ---- #if expression evaluation ----
    static long EvalIf(string expr, int line, Dictionary<string, Macro> macros)
    {
        var raw = new List<Token>();
        LexLine(expr, line, raw);
        var pre = new List<Token>();
        for (int i = 0; i < raw.Count; i++)
        {
            if (raw[i].K == TokKind.Ident && raw[i].Text == "defined")
            {
                string n;
                if (i + 1 < raw.Count && raw[i + 1].Text == "(") { n = i + 2 < raw.Count ? raw[i + 2].Text : ""; i += 3; }
                else { n = i + 1 < raw.Count ? raw[i + 1].Text : ""; i += 1; }
                pre.Add(new Token { K = TokKind.Int, I = macros.ContainsKey(n) ? 1 : 0, Text = "1", Line = line });
            }
            else pre.Add(raw[i]);
        }
        var toks = new List<Token>();
        Expand(pre, toks, new HashSet<string>(), macros, line);
        int p = 0;
        long v = IfOr(toks, ref p);
        return v;
    }

    static long IfPrimary(List<Token> t, ref int p)
    {
        if (p >= t.Count) return 0;
        var k = t[p++];
        if (k.Text == "(") { long v = IfOr(t, ref p); if (p < t.Count && t[p].Text == ")") p++; return v; }
        if (k.Text == "!") return IfPrimary(t, ref p) == 0 ? 1 : 0;
        if (k.Text == "-") return -IfPrimary(t, ref p);
        if (k.K == TokKind.Int || k.K == TokKind.Char) return k.I;
        return 0; // unknown identifiers are 0
    }

    static long IfBin(List<Token> t, ref int p, int level)
    {
        string[][] ops =
        {
            new[] { "||" }, new[] { "&&" }, new[] { "==", "!=" }, new[] { "<", ">", "<=", ">=" },
            new[] { "+", "-" }, new[] { "*", "/", "%" }
        };
        if (level >= ops.Length) return IfPrimary(t, ref p);
        long a = IfBin(t, ref p, level + 1);
        while (p < t.Count && t[p].K == TokKind.Punct && ops[level].Contains(t[p].Text))
        {
            string op = t[p++].Text;
            long b = IfBin(t, ref p, level + 1);
            a = op switch
            {
                "||" => (a != 0 || b != 0) ? 1 : 0,
                "&&" => (a != 0 && b != 0) ? 1 : 0,
                "==" => a == b ? 1 : 0,
                "!=" => a != b ? 1 : 0,
                "<" => a < b ? 1 : 0,
                ">" => a > b ? 1 : 0,
                "<=" => a <= b ? 1 : 0,
                ">=" => a >= b ? 1 : 0,
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" => b == 0 ? 0 : a / b,
                _ => b == 0 ? 0 : a % b
            };
        }
        return a;
    }

    static long IfOr(List<Token> t, ref int p) => IfBin(t, ref p, 0);

    // ---- macro expansion ----
    static void Expand(List<Token> src, List<Token> dst, HashSet<string> active, Dictionary<string, Macro> macros, int line)
    {
        for (int i = 0; i < src.Count; i++)
        {
            var t = src[i];
            if (t.K != TokKind.Ident || !macros.TryGetValue(t.Text, out var m) || active.Contains(t.Text))
            {
                dst.Add(t);
                continue;
            }
            if (m.Params == null)
            {
                active.Add(m.Name);
                Expand(m.Body.Select(b => b.At(t.Line)).ToList(), dst, active, macros, line);
                active.Remove(m.Name);
                continue;
            }
            if (i + 1 >= src.Count || src[i + 1].Text != "(")
            {
                dst.Add(t);
                continue;
            }
            // collect arguments
            var args = new List<List<Token>>();
            var cur = new List<Token>();
            int depth = 0, j = i + 2;
            for (; j < src.Count; j++)
            {
                var a = src[j];
                if (a.K == TokKind.Punct && (a.Text == "(" || a.Text == "[")) depth++;
                if (a.K == TokKind.Punct && (a.Text == ")" || a.Text == "]"))
                {
                    if (depth == 0 && a.Text == ")") break;
                    depth--;
                }
                if (depth == 0 && a.K == TokKind.Punct && a.Text == ",") { args.Add(cur); cur = new List<Token>(); continue; }
                cur.Add(a);
            }
            if (j >= src.Count) throw Err(t.Line, L.F("chamada da macro {0} sem ')' na mesma linha", m.Name));
            if (cur.Count > 0 || args.Count > 0) args.Add(cur);
            if (args.Count != m.Params.Count && !(m.Params.Count == 0 && args.Count == 0))
                throw Err(t.Line, L.F("a macro {0} espera {1} argumento(s)", m.Name, m.Params.Count));
            var expandedArgs = args.Select(a => { var o = new List<Token>(); Expand(a, o, active, macros, line); return o; }).ToList();
            var body = new List<Token>();
            foreach (var b in m.Body)
            {
                int pi = b.K == TokKind.Ident ? m.Params.IndexOf(b.Text) : -1;
                if (pi >= 0) body.AddRange(expandedArgs[pi].Select(x => x.At(t.Line)));
                else body.Add(b.At(t.Line));
            }
            active.Add(m.Name);
            Expand(body, dst, active, macros, line);
            active.Remove(m.Name);
            i = j;
        }
    }

    // ---- comments ----
    static string StripComments(string s)
    {
        var sb = new StringBuilder(s.Length);
        int line = 1;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '\n') line++;
            if (c == '"' || c == '\'')
            {
                char q = c;
                sb.Append(c);
                i++;
                while (i < s.Length && s[i] != q && s[i] != '\n')
                {
                    if (s[i] == '\\' && i + 1 < s.Length && s[i + 1] != '\n') { sb.Append(s[i]); i++; }
                    sb.Append(s[i]);
                    i++;
                }
                if (i < s.Length) { sb.Append(s[i]); if (s[i] == '\n') line++; }
                continue;
            }
            if (c == '/' && i + 1 < s.Length && s[i + 1] == '/')
            {
                while (i < s.Length && s[i] != '\n') i++;
                if (i < s.Length) { sb.Append('\n'); line++; }
                continue;
            }
            if (c == '/' && i + 1 < s.Length && s[i + 1] == '*')
            {
                int start = line;
                i += 2;
                sb.Append(' ');
                while (i < s.Length && !(s[i] == '*' && i + 1 < s.Length && s[i + 1] == '/'))
                {
                    if (s[i] == '\n') { sb.Append('\n'); line++; }
                    i++;
                }
                if (i >= s.Length) throw Err(start, L.T("comentário /* não foi fechado com */"));
                i++;
                continue;
            }
            sb.Append(c);
        }
        return sb.ToString();
    }

    // ---- tokens of one line ----
    public static void LexLine(string s, int line, List<Token> outList)
    {
        int i = 0;
        while (i < s.Length)
        {
            char c = s[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }
            if (char.IsLetter(c) || c == '_')
            {
                int st = i;
                while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_')) i++;
                outList.Add(new Token { K = TokKind.Ident, Text = s[st..i], Line = line });
                continue;
            }
            if (char.IsDigit(c) || (c == '.' && i + 1 < s.Length && char.IsDigit(s[i + 1])))
            {
                outList.Add(LexNumber(s, ref i, line));
                continue;
            }
            if (c == '\'')
            {
                i++;
                var sb = new StringBuilder();
                while (i < s.Length && s[i] != '\'') sb.Append(ReadChar(s, ref i, line));
                if (i >= s.Length) throw Err(line, L.T("caractere entre aspas simples não foi fechado (falta ')"));
                i++;
                if (sb.Length == 0) throw Err(line, L.T("caractere vazio ''"));
                outList.Add(new Token { K = TokKind.Char, Text = sb.ToString(), I = (sbyte)(byte)sb[0], Line = line });
                continue;
            }
            if (c == '"')
            {
                i++;
                var sb = new StringBuilder();
                while (i < s.Length && s[i] != '"') sb.Append(ReadChar(s, ref i, line));
                if (i >= s.Length) throw Err(line, L.T("texto entre aspas não foi fechado (falta \")"));
                i++;
                outList.Add(new Token { K = TokKind.String, Text = sb.ToString(), Line = line });
                continue;
            }
            string? p = null;
            foreach (var cand in Puncts)
                if (string.CompareOrdinal(s, i, cand, 0, cand.Length) == 0) { p = cand; break; }
            if (p == null)
            {
                string hint = c is '“' or '”' or '‘' or '’' ? L.T(" (use aspas retas \" ou ')") : "";
                throw Err(line, L.F("caractere inválido '{0}'{1}", c, hint));
            }
            outList.Add(new Token { K = TokKind.Punct, Text = p, Line = line });
            i += p.Length;
        }
    }

    static char ReadChar(string s, ref int i, int line)
    {
        char c = s[i++];
        if (c != '\\') return c;
        if (i >= s.Length) return '\\';
        char e = s[i++];
        switch (e)
        {
            case 'n': return '\n';
            case 't': return '\t';
            case 'r': return '\r';
            case '0' when i >= s.Length || !char.IsDigit(s[i]): return '\0';
            case 'a': return '\a';
            case 'b': return '\b';
            case 'f': return '\f';
            case 'v': return '\v';
            case '\\': return '\\';
            case '\'': return '\'';
            case '"': return '"';
            case '?': return '?';
            case 'x':
                {
                    int v = 0, n = 0;
                    while (i < s.Length && Uri.IsHexDigit(s[i]) && n < 2) { v = v * 16 + Convert.ToInt32(s[i].ToString(), 16); i++; n++; }
                    return (char)v;
                }
            default:
                if (e >= '0' && e <= '7')
                {
                    int v = e - '0', n = 1;
                    while (i < s.Length && s[i] >= '0' && s[i] <= '7' && n < 3) { v = v * 8 + (s[i] - '0'); i++; n++; }
                    return (char)(v & 0xFF);
                }
                return e;
        }
    }

    static Token LexNumber(string s, ref int i, int line)
    {
        int st = i;
        var t = new Token { Line = line };
        bool isFloat = false;
        if (s[i] == '0' && i + 1 < s.Length && (s[i + 1] == 'x' || s[i + 1] == 'X'))
        {
            i += 2;
            int ds = i;
            while (i < s.Length && Uri.IsHexDigit(s[i])) i++;
            if (ds == i) throw Err(line, L.T("número hexadecimal inválido"));
            t.I = (long)ulong.Parse(s[ds..i], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }
        else if (s[i] == '0' && i + 1 < s.Length && (s[i + 1] == 'b' || s[i + 1] == 'B'))
        {
            i += 2;
            int ds = i;
            while (i < s.Length && (s[i] == '0' || s[i] == '1')) i++;
            if (ds == i) throw Err(line, L.T("número binário inválido"));
            t.I = Convert.ToInt64(s[ds..i], 2);
        }
        else
        {
            while (i < s.Length && char.IsDigit(s[i])) i++;
            if (i < s.Length && s[i] == '.') { isFloat = true; i++; while (i < s.Length && char.IsDigit(s[i])) i++; }
            if (i < s.Length && (s[i] == 'e' || s[i] == 'E'))
            {
                int save = i;
                i++;
                if (i < s.Length && (s[i] == '+' || s[i] == '-')) i++;
                if (i < s.Length && char.IsDigit(s[i])) { isFloat = true; while (i < s.Length && char.IsDigit(s[i])) i++; }
                else i = save;
            }
            string num = s[st..i];
            if (isFloat)
            {
                t.F = double.Parse(num, NumberStyles.Float, CultureInfo.InvariantCulture);
            }
            else if (num.Length > 1 && num[0] == '0')
            {
                foreach (char ch in num) if (ch > '7') throw Err(line, L.F("número octal inválido '{0}' (números começando com 0 são octais)", num));
                t.I = Convert.ToInt64(num, 8);
            }
            else
            {
                if (!ulong.TryParse(num, NumberStyles.None, CultureInfo.InvariantCulture, out var u))
                    throw Err(line, L.F("número grande demais: {0}", num));
                t.I = (long)u;
            }
        }
        while (i < s.Length && "uUlLfF".IndexOf(s[i]) >= 0)
        {
            char c = char.ToLowerInvariant(s[i]);
            if (c == 'u') t.Unsigned = true;
            else if (c == 'l') t.LongSfx = true;
            else if (c == 'f')
            {
                if (!isFloat) break;
                t.FloatSfx = true;
            }
            i++;
        }
        if (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_'))
            throw Err(line, L.F("número mal escrito: '{0}'", s[st..(i + 1)]));
        t.K = isFloat ? TokKind.Float : TokKind.Int;
        t.Text = s[st..i];
        return t;
    }
}
