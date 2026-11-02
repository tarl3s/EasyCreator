using System.Globalization;
using System.Text;

namespace EasyCreator.CInterp;

/// <summary>The C standard library subset: stdio, string, stdlib, math, ctype.</summary>
public static class CStdLib
{
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    static void Need(CInterpreter ip, CVal[] a, int n, string fn)
    {
        if (a.Length < n) throw ip.RtErr(L.F("{0}() precisa de {1} argumento(s), mas recebeu {2}", fn, n, a.Length));
    }

    static long Ptr(CInterpreter ip, CVal v, string fn, int argNo)
    {
        if (!v.T.IsPtr) throw ip.RtErr(L.F("{0}(): o argumento {1} precisa ser um ponteiro/texto, mas é {2}", fn, argNo, v.T));
        return v.L;
    }

    static string Str(CInterpreter ip, CVal v, string fn, int argNo) => ip.ReadCString(Ptr(ip, v, fn, argNo));

    public static void Register(CInterpreter ip)
    {
        // ---- predefined macros ----
        ip.DefineMacro("NULL", "((void*)0)");
        ip.DefineMacro("EOF", "(-1)");
        ip.DefineMacro("RAND_MAX", "32767");
        ip.DefineMacro("INT_MAX", "2147483647");
        ip.DefineMacro("INT_MIN", "(-2147483647-1)");
        ip.DefineMacro("UINT_MAX", "4294967295U");
        ip.DefineMacro("LONG_MAX", "9223372036854775807L");
        ip.DefineMacro("LONG_MIN", "(-9223372036854775807L-1)");
        ip.DefineMacro("CHAR_MAX", "127");
        ip.DefineMacro("CHAR_MIN", "(-128)");
        ip.DefineMacro("SHRT_MAX", "32767");
        ip.DefineMacro("M_PI", "3.14159265358979323846");
        ip.DefineMacro("M_E", "2.7182818284590452354");
        ip.DefineMacro("EXIT_SUCCESS", "0");
        ip.DefineMacro("EXIT_FAILURE", "1");
        ip.DefineMacro("stdin", "((void*)0)");
        ip.DefineMacro("stdout", "((void*)1)");
        ip.DefineMacro("stderr", "((void*)2)");
        ip.DefineMacro("TRUE", "1");
        ip.DefineMacro("FALSE", "0");

        // ---- stdio ----
        ip.AddNative("printf", (ip, a) =>
        {
            Need(ip, a, 1, "printf");
            var s = Format(ip, Str(ip, a[0], "printf", 1), a, 1, "printf");
            ip.Print(s);
            return CVal.Int(s.Length);
        });
        ip.AddNative("fprintf", (ip, a) =>
        {
            Need(ip, a, 2, "fprintf");
            var s = Format(ip, Str(ip, a[1], "fprintf", 2), a, 2, "fprintf");
            ip.Print(s);
            return CVal.Int(s.Length);
        });
        ip.AddNative("sprintf", (ip, a) =>
        {
            Need(ip, a, 2, "sprintf");
            var s = Format(ip, Str(ip, a[1], "sprintf", 2), a, 2, "sprintf");
            ip.WriteCString(Ptr(ip, a[0], "sprintf", 1), s);
            return CVal.Int(s.Length);
        });
        ip.AddNative("snprintf", (ip, a) =>
        {
            Need(ip, a, 3, "snprintf");
            var s = Format(ip, Str(ip, a[2], "snprintf", 3), a, 3, "snprintf");
            long n = a[1].AsLong;
            if (n > 0) ip.WriteCString(Ptr(ip, a[0], "snprintf", 1), s.Length >= n ? s[..(int)(n - 1)] : s);
            return CVal.Int(s.Length);
        });
        ip.AddNative("puts", (ip, a) => { Need(ip, a, 1, "puts"); ip.Print(Str(ip, a[0], "puts", 1) + "\n"); return CVal.Int(1); });
        ip.AddNative("fputs", (ip, a) => { Need(ip, a, 1, "fputs"); ip.Print(Str(ip, a[0], "fputs", 1)); return CVal.Int(1); });
        ip.AddNative("putchar", (ip, a) => { Need(ip, a, 1, "putchar"); ip.Print(((char)(byte)a[0].AsLong).ToString()); return CVal.Int((byte)a[0].AsLong); });
        ip.AddNative("putc", (ip, a) => { Need(ip, a, 1, "putc"); ip.Print(((char)(byte)a[0].AsLong).ToString()); return CVal.Int((byte)a[0].AsLong); });
        ip.AddNative("fflush", (ip, a) => CVal.Int(0));
        ip.AddNative("getchar", (ip, a) => CVal.Int(ip.InPos < ip.In.Length ? (byte)ip.In[ip.InPos++] : -1));
        ip.AddNative("scanf", (ip, a) =>
        {
            Need(ip, a, 1, "scanf");
            var r = new InputReader(ip.In, ip.InPos);
            int n = Scan(ip, Str(ip, a[0], "scanf", 1), a, 1, r, "scanf");
            ip.InPos = r.Pos;
            return CVal.Int(n);
        });
        ip.AddNative("sscanf", (ip, a) =>
        {
            Need(ip, a, 2, "sscanf");
            var r = new InputReader(Str(ip, a[0], "sscanf", 1), 0);
            return CVal.Int(Scan(ip, Str(ip, a[1], "sscanf", 2), a, 2, r, "sscanf"));
        });
        ip.AddNative("fgets", (ip, a) =>
        {
            Need(ip, a, 2, "fgets");
            long buf = Ptr(ip, a[0], "fgets", 1);
            long n = a[1].AsLong;
            if (ip.InPos >= ip.In.Length || n <= 1) return CVal.Of(CType.Char.Pointer, 0);
            var sb = new StringBuilder();
            while (ip.InPos < ip.In.Length && sb.Length < n - 1)
            {
                char c = ip.In[ip.InPos++];
                sb.Append(c);
                if (c == '\n') break;
            }
            ip.WriteCString(buf, sb.ToString());
            return CVal.Of(CType.Char.Pointer, buf);
        });
        ip.AddNative("gets", (ip, a) =>
        {
            Need(ip, a, 1, "gets");
            long buf = Ptr(ip, a[0], "gets", 1);
            if (ip.InPos >= ip.In.Length) return CVal.Of(CType.Char.Pointer, 0);
            var sb = new StringBuilder();
            while (ip.InPos < ip.In.Length)
            {
                char c = ip.In[ip.InPos++];
                if (c == '\n') break;
                sb.Append(c);
            }
            ip.WriteCString(buf, sb.ToString());
            return CVal.Of(CType.Char.Pointer, buf);
        });

        // ---- string.h ----
        ip.AddNative("strlen", (ip, a) => { Need(ip, a, 1, "strlen"); return CVal.Of(CType.ULong, Str(ip, a[0], "strlen", 1).Length); });
        ip.AddNative("strcpy", (ip, a) =>
        {
            Need(ip, a, 2, "strcpy");
            var s = Str(ip, a[1], "strcpy", 2);
            ip.WriteCString(Ptr(ip, a[0], "strcpy", 1), s);
            return a[0];
        });
        ip.AddNative("strncpy", (ip, a) =>
        {
            Need(ip, a, 3, "strncpy");
            var s = Str(ip, a[1], "strncpy", 2);
            long d = Ptr(ip, a[0], "strncpy", 1), n = a[2].AsLong;
            for (long i = 0; i < n; i++) ip.Write(d + i, i < s.Length ? (sbyte)(byte)s[(int)i] : 0);
            return a[0];
        });
        ip.AddNative("strcat", (ip, a) =>
        {
            Need(ip, a, 2, "strcat");
            long d = Ptr(ip, a[0], "strcat", 1);
            var s1 = ip.ReadCString(d);
            ip.WriteCString(d + s1.Length, Str(ip, a[1], "strcat", 2));
            return a[0];
        });
        ip.AddNative("strncat", (ip, a) =>
        {
            Need(ip, a, 3, "strncat");
            long d = Ptr(ip, a[0], "strncat", 1);
            var s1 = ip.ReadCString(d);
            var s2 = Str(ip, a[1], "strncat", 2);
            long n = a[2].AsLong;
            ip.WriteCString(d + s1.Length, s2.Length > n ? s2[..(int)n] : s2);
            return a[0];
        });
        ip.AddNative("strcmp", (ip, a) => { Need(ip, a, 2, "strcmp"); return CVal.Int(Cmp(Str(ip, a[0], "strcmp", 1), Str(ip, a[1], "strcmp", 2), int.MaxValue)); });
        ip.AddNative("strncmp", (ip, a) => { Need(ip, a, 3, "strncmp"); return CVal.Int(Cmp(Str(ip, a[0], "strncmp", 1), Str(ip, a[1], "strncmp", 2), (int)Math.Min(int.MaxValue, a[2].AsLong))); });
        ip.AddNative("strchr", (ip, a) =>
        {
            Need(ip, a, 2, "strchr");
            long p = Ptr(ip, a[0], "strchr", 1);
            var s = ip.ReadCString(p);
            char c = (char)(byte)a[1].AsLong;
            int i = c == 0 ? s.Length : s.IndexOf(c);
            return CVal.Of(CType.Char.Pointer, i < 0 ? 0 : p + i);
        });
        ip.AddNative("strrchr", (ip, a) =>
        {
            Need(ip, a, 2, "strrchr");
            long p = Ptr(ip, a[0], "strrchr", 1);
            var s = ip.ReadCString(p);
            char c = (char)(byte)a[1].AsLong;
            int i = c == 0 ? s.Length : s.LastIndexOf(c);
            return CVal.Of(CType.Char.Pointer, i < 0 ? 0 : p + i);
        });
        ip.AddNative("strstr", (ip, a) =>
        {
            Need(ip, a, 2, "strstr");
            long p = Ptr(ip, a[0], "strstr", 1);
            int i = ip.ReadCString(p).IndexOf(Str(ip, a[1], "strstr", 2), StringComparison.Ordinal);
            return CVal.Of(CType.Char.Pointer, i < 0 ? 0 : p + i);
        });
        ip.AddNative("memset", (ip, a) =>
        {
            Need(ip, a, 3, "memset");
            long p = Ptr(ip, a[0], "memset", 1);
            long cells = CellsFor(a[0].T, a[2].AsLong);
            ip.CheckRange(p, cells);
            var et = a[0].T.Elem!;
            long v = a[1].AsLong & 0xFF;
            long fill = et.K == TK.Char ? (sbyte)(byte)v : (v == 0 ? 0 : (et.IsFloat ? 0 : RepeatByte(v, et)));
            for (long i = 0; i < cells; i++) ip.Mem[p + i] = fill;
            return a[0];
        });
        NativeFn memcpy = (ip, a) =>
        {
            Need(ip, a, 3, "memcpy");
            long d = Ptr(ip, a[0], "memcpy", 1), s = Ptr(ip, a[1], "memcpy", 2);
            long cells = CellsFor(a[0].T.Elem!.K == TK.Void ? a[1].T : a[0].T, a[2].AsLong);
            ip.CheckRange(d, cells);
            ip.CheckRange(s, cells);
            Array.Copy(ip.Mem, s, ip.Mem, d, cells);
            return a[0];
        };
        ip.AddNative("memcpy", memcpy);
        ip.AddNative("memmove", memcpy);

        // ---- stdlib ----
        ip.AddNative("malloc", (ip, a) => { Need(ip, a, 1, "malloc"); return CVal.Of(CType.Void.Pointer, ip.Malloc(a[0].AsLong)); });
        ip.AddNative("calloc", (ip, a) => { Need(ip, a, 2, "calloc"); return CVal.Of(CType.Void.Pointer, ip.Malloc(a[0].AsLong * a[1].AsLong)); });
        ip.AddNative("realloc", (ip, a) =>
        {
            Need(ip, a, 2, "realloc");
            long old = a[0].L;
            long n = ip.Malloc(a[1].AsLong);
            if (old != 0 && n != 0)
            {
                long sz = ip.BlockSize(old);
                if (sz < 0) throw ip.RtErr(L.T("realloc() recebeu um ponteiro que não veio de malloc"));
                Array.Copy(ip.Mem, old, ip.Mem, n, Math.Min(sz, Math.Max(1, a[1].AsLong)));
                ip.Free(old);
            }
            return CVal.Of(CType.Void.Pointer, n);
        });
        ip.AddNative("free", (ip, a) => { Need(ip, a, 1, "free"); ip.Free(a[0].L); return CVal.VoidV; });
        ip.AddNative("exit", (ip, a) => throw new CExit(a.Length > 0 ? (int)a[0].AsLong : 0));
        ip.AddNative("abort", (ip, a) => throw ip.RtErr(L.T("o programa chamou abort()")));
        ip.AddNative("atoi", (ip, a) => { Need(ip, a, 1, "atoi"); return CVal.Int(ParseLeadingInt(Str(ip, a[0], "atoi", 1))); });
        ip.AddNative("atol", (ip, a) => { Need(ip, a, 1, "atol"); return CVal.LongV(ParseLeadingInt(Str(ip, a[0], "atol", 1))); });
        ip.AddNative("atof", (ip, a) =>
        {
            Need(ip, a, 1, "atof");
            var r = new InputReader(Str(ip, a[0], "atof", 1), 0);
            return CVal.Dbl(r.ReadDouble(int.MaxValue) ?? 0);
        });
        ip.AddNative("rand", (ip, a) => CVal.Int(ip.NextRand()));
        ip.AddNative("srand", (ip, a) => { Need(ip, a, 1, "srand"); ip.RandState = (uint)a[0].AsLong; return CVal.VoidV; });
        ip.AddNative("time", (ip, a) =>
        {
            long t = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (a.Length > 0 && a[0].T.IsPtr && a[0].L != 0) ip.Write(a[0].L, t);
            return CVal.LongV(t);
        });
        ip.AddNative("clock", (ip, a) => CVal.LongV(ip.Steps / 100));
        ip.DefineMacro("CLOCKS_PER_SEC", "1000000");
        ip.AddNative("abs", (ip, a) => { Need(ip, a, 1, "abs"); return a[0].T.IsFloat ? new CVal { T = a[0].T, D = Math.Abs(a[0].D) } : new CVal { T = a[0].T.Promoted, L = Math.Abs(a[0].L) }; });
        ip.AddNative("labs", (ip, a) => { Need(ip, a, 1, "labs"); return CVal.LongV(Math.Abs(a[0].AsLong)); });
        ip.AddNative("assert", (ip, a) =>
        {
            Need(ip, a, 1, "assert");
            if (!a[0].IsTrue) throw ip.RtErr(L.T("assert falhou: a condição testada é falsa"));
            return CVal.VoidV;
        });

        // ---- math ----
        Math1(ip, "sqrt", x => x < 0 ? double.NaN : Math.Sqrt(x));
        Math1(ip, "fabs", Math.Abs);
        Math1(ip, "sin", Math.Sin);
        Math1(ip, "cos", Math.Cos);
        Math1(ip, "tan", Math.Tan);
        Math1(ip, "asin", Math.Asin);
        Math1(ip, "acos", Math.Acos);
        Math1(ip, "atan", Math.Atan);
        Math1(ip, "exp", Math.Exp);
        Math1(ip, "log", Math.Log);
        Math1(ip, "log10", Math.Log10);
        Math1(ip, "log2", Math.Log2);
        Math1(ip, "floor", Math.Floor);
        Math1(ip, "ceil", Math.Ceiling);
        Math1(ip, "round", x => Math.Round(x, MidpointRounding.AwayFromZero));
        Math1(ip, "trunc", Math.Truncate);
        Math1(ip, "cbrt", Math.Cbrt);
        Math2(ip, "pow", Math.Pow);
        Math2(ip, "atan2", Math.Atan2);
        Math2(ip, "fmod", (x, y) => x % y);
        Math2(ip, "fmin", Math.Min);
        Math2(ip, "fmax", Math.Max);
        Math2(ip, "hypot", (x, y) => Math.Sqrt(x * x + y * y));

        // ---- ctype ----
        Ctype(ip, "isdigit", c => c >= '0' && c <= '9');
        Ctype(ip, "isalpha", c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'));
        Ctype(ip, "isalnum", c => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'));
        Ctype(ip, "isspace", c => c == ' ' || (c >= 9 && c <= 13));
        Ctype(ip, "isupper", c => c >= 'A' && c <= 'Z');
        Ctype(ip, "islower", c => c >= 'a' && c <= 'z');
        Ctype(ip, "ispunct", c => c > 32 && c < 127 && !char.IsLetterOrDigit((char)c));
        Ctype(ip, "isxdigit", c => Uri.IsHexDigit((char)c));
        ip.AddNative("toupper", (ip, a) => { Need(ip, a, 1, "toupper"); long c = a[0].AsLong; return CVal.Int(c >= 'a' && c <= 'z' ? c - 32 : c); });
        ip.AddNative("tolower", (ip, a) => { Need(ip, a, 1, "tolower"); long c = a[0].AsLong; return CVal.Int(c >= 'A' && c <= 'Z' ? c + 32 : c); });
    }

    static long RepeatByte(long b, CType t)
    {
        long v = 0;
        for (int i = 0; i < t.Bytes && i < 8; i++) v = (v << 8) | b;
        return Conv.Norm(v, t);
    }

    static long CellsFor(CType ptr, long bytes)
    {
        var e = ptr.Elem;
        long eb = e == null || e.K == TK.Void ? 1 : Math.Max(1, e.Bytes);
        long ec = e == null || e.K == TK.Void ? 1 : Math.Max(1, e.Cells);
        return (bytes + eb - 1) / eb * ec;
    }

    static int Cmp(string a, string b, int n)
    {
        for (int i = 0; i < n; i++)
        {
            int ca = i < a.Length ? (byte)a[i] : 0, cb = i < b.Length ? (byte)b[i] : 0;
            if (ca != cb) return ca < cb ? -1 : 1;
            if (ca == 0) return 0;
        }
        return 0;
    }

    static long ParseLeadingInt(string s)
    {
        var r = new InputReader(s, 0);
        return r.ReadInt(10, int.MaxValue, false) ?? 0;
    }

    static void Math1(CInterpreter ip, string name, Func<double, double> f) =>
        ip.AddNative(name, (ip, a) => { Need(ip, a, 1, name); return CVal.Dbl(f(a[0].AsDouble)); });

    static void Math2(CInterpreter ip, string name, Func<double, double, double> f) =>
        ip.AddNative(name, (ip, a) => { Need(ip, a, 2, name); return CVal.Dbl(f(a[0].AsDouble, a[1].AsDouble)); });

    static void Ctype(CInterpreter ip, string name, Func<long, bool> f) =>
        ip.AddNative(name, (ip, a) => { Need(ip, a, 1, name); return CVal.Int(f((byte)a[0].AsLong) ? 1 : 0); });

    // ======================= printf =======================
    public static string Format(CInterpreter ip, string fmt, CVal[] args, int argi, string fn)
    {
        var sb = new StringBuilder();
        for (int i = 0; i < fmt.Length; i++)
        {
            char c = fmt[i];
            if (c != '%') { sb.Append(c); continue; }
            int start = i;
            i++;
            if (i >= fmt.Length) { sb.Append('%'); break; }
            if (fmt[i] == '%') { sb.Append('%'); continue; }
            bool left = false, plus = false, space = false, zero = false, alt = false;
            for (; i < fmt.Length; i++)
            {
                char f = fmt[i];
                if (f == '-') left = true;
                else if (f == '+') plus = true;
                else if (f == ' ') space = true;
                else if (f == '0') zero = true;
                else if (f == '#') alt = true;
                else break;
            }
            int width = -1, prec = -1;
            if (i < fmt.Length && fmt[i] == '*') { width = (int)NextArg(ip, args, ref argi, fn, fmt).AsLong; if (width < 0) { left = true; width = -width; } i++; }
            else { int w = 0; bool any = false; while (i < fmt.Length && char.IsDigit(fmt[i])) { w = Math.Min(10000, w * 10 + (fmt[i] - '0')); i++; any = true; } if (any) width = w; }
            if (i < fmt.Length && fmt[i] == '.')
            {
                i++;
                if (i < fmt.Length && fmt[i] == '*') { prec = (int)NextArg(ip, args, ref argi, fn, fmt).AsLong; i++; }
                else { int pr = 0; while (i < fmt.Length && char.IsDigit(fmt[i])) { pr = Math.Min(10000, pr * 10 + (fmt[i] - '0')); i++; } prec = pr; }
            }
            string len = "";
            while (i < fmt.Length && "hlLzjtq".IndexOf(fmt[i]) >= 0) { len += fmt[i]; i++; }
            if (i >= fmt.Length) { sb.Append(fmt[start..]); break; }
            char conv = fmt[i];
            string body;
            string sign = "";
            bool numeric = true;
            switch (conv)
            {
                case 'd':
                case 'i':
                    {
                        var v = NextArg(ip, args, ref argi, fn, fmt);
                        NeedInteger(ip, v, fn, len, conv);
                        long x = v.AsLong;
                        if (len == "h") x = (short)x;
                        else if (len == "hh") x = (sbyte)x;
                        else if (len == "") x = (int)x;
                        if (x < 0) { sign = "-"; body = x == long.MinValue ? "9223372036854775808" : (-x).ToString(Inv); }
                        else { body = x.ToString(Inv); sign = plus ? "+" : space ? " " : ""; }
                        if (prec >= 0) { body = body.PadLeft(prec, '0'); if (prec == 0 && x == 0) body = ""; zero = false; }
                        break;
                    }
                case 'u':
                case 'x':
                case 'X':
                case 'o':
                    {
                        var v = NextArg(ip, args, ref argi, fn, fmt);
                        NeedInteger(ip, v, fn, len, conv);
                        ulong x = (ulong)v.AsLong;
                        if (len == "h") x = (ushort)x;
                        else if (len == "hh") x = (byte)x;
                        else if (len == "") x = (uint)x;
                        body = conv switch
                        {
                            'u' => x.ToString(Inv),
                            'x' => x.ToString("x", Inv),
                            'X' => x.ToString("X", Inv),
                            _ => Convert.ToString((long)x, 8)
                        };
                        if (alt && x != 0) sign = conv == 'x' ? "0x" : conv == 'X' ? "0X" : conv == 'o' ? "0" : "";
                        if (prec >= 0) { body = body.PadLeft(prec, '0'); zero = false; }
                        break;
                    }
                case 'f':
                case 'F':
                case 'e':
                case 'E':
                case 'g':
                case 'G':
                    {
                        var v = NextArg(ip, args, ref argi, fn, fmt);
                        if (!v.T.IsFloat) throw ip.RtErr(L.F("{0}: %{1} espera um número com casas decimais (double), mas recebeu {2}. Use %d para inteiros ou converta com (double).", fn, len + conv, v.T));
                        double d = v.AsDouble;
                        if (prec < 0) prec = 6;
                        bool neg = d < 0 || (d == 0 && double.IsNegative(d));
                        double ad = Math.Abs(d);
                        if (double.IsNaN(d)) { body = char.IsUpper(conv) ? "NAN" : "nan"; neg = false; zero = false; }
                        else if (double.IsInfinity(d)) { body = char.IsUpper(conv) ? "INF" : "inf"; zero = false; }
                        else body = FormatFloat(ad, conv, prec, alt);
                        sign = neg ? "-" : plus ? "+" : space ? " " : "";
                        break;
                    }
                case 'c':
                    {
                        var v = NextArg(ip, args, ref argi, fn, fmt);
                        NeedInteger(ip, v, fn, len, conv);
                        body = ((char)(byte)v.AsLong).ToString();
                        numeric = false;
                        break;
                    }
                case 's':
                    {
                        var v = NextArg(ip, args, ref argi, fn, fmt);
                        if (!v.T.IsPtr) throw ip.RtErr(L.F("{0}: %s precisa de um texto (char*), mas recebeu {1}", fn, v.T));
                        body = v.L == 0 ? "(null)" : ip.ReadCString(v.L, prec >= 0 ? prec : 1 << 20);
                        if (prec >= 0 && body.Length > prec) body = body[..prec];
                        numeric = false;
                        break;
                    }
                case 'p':
                    {
                        var v = NextArg(ip, args, ref argi, fn, fmt);
                        body = v.L == 0 ? "(nil)" : "0x" + (v.L * 4).ToString("x", Inv);
                        numeric = false;
                        break;
                    }
                case 'n':
                    {
                        var v = NextArg(ip, args, ref argi, fn, fmt);
                        if (v.T.IsPtr) ip.Store(v.L, v.T.Elem!, CVal.Int(sb.Length));
                        continue;
                    }
                default:
                    throw ip.RtErr(L.F("{0}: formato desconhecido '%{1}'", fn, conv));
            }
            int total = sign.Length + body.Length;
            if (width > total)
            {
                if (left) sb.Append(sign).Append(body).Append(' ', width - total);
                else if (zero && numeric) sb.Append(sign).Append('0', width - total).Append(body);
                else sb.Append(' ', width - total).Append(sign).Append(body);
            }
            else sb.Append(sign).Append(body);
        }
        return sb.ToString();
    }

    static CVal NextArg(CInterpreter ip, CVal[] args, ref int argi, string fn, string fmt)
    {
        if (argi >= args.Length)
            throw ip.RtErr(L.F("{0}: o formato \"{1}\" pede mais valores do que foram passados", fn, Escape(fmt)));
        var v = args[argi++];
        if (v.T.K == TK.Struct || v.T.K == TK.Void) throw ip.RtErr(L.F("{0}: não é possível imprimir um valor do tipo {1}", fn, v.T));
        return v;
    }

    /// <summary>Integer conversions (%d %u %x %o %c) with a floating argument print garbage in real C (the bits are read
    /// from the wrong place); the lessons warn about it, so the interpreter stops with an explanation instead.</summary>
    static void NeedInteger(CInterpreter ip, CVal v, string fn, string len, char conv)
    {
        if (v.T.IsFloat)
            throw ip.RtErr(L.F("{0}: %{1} espera um número inteiro, mas recebeu {2}. Use %f para números com casas decimais ou converta com (int).", fn, len + conv, v.T));
    }

    static string Escape(string s) => s.Replace("\n", "\\n").Replace("\t", "\\t");

    public static string FormatFloat(double ad, char conv, int prec, bool alt)
    {
        switch (char.ToLowerInvariant(conv))
        {
            case 'f':
                {
                    var s = ad.ToString("F" + Math.Min(prec, 300), Inv);
                    if (alt && prec == 0) s += ".";
                    return s;
                }
            case 'e':
                {
                    var s = ExpFormat(ad, prec);
                    if (alt && prec == 0) s = s.Insert(1, ".");
                    return char.IsUpper(conv) ? s.ToUpperInvariant() : s;
                }
            default:
                {
                    int p = prec == 0 ? 1 : prec;
                    string r;
                    if (ad == 0) r = p > 1 ? "0." + new string('0', p - 1) : "0";
                    else
                    {
                        // the exponent X is the one %e would show with P-1 digits (after rounding, as C requires)
                        string es = ExpFormat(ad, p - 1);
                        int x = int.Parse(es[(es.IndexOf('e') + 1)..], Inv);
                        r = x < p && x >= -4 ? ad.ToString("F" + (p - 1 - x), Inv) : es;
                    }
                    int ePos = r.IndexOf('e');
                    string mant = ePos >= 0 ? r[..ePos] : r, exp = ePos >= 0 ? r[ePos..] : "";
                    if (!alt) { if (mant.Contains('.')) mant = mant.TrimEnd('0').TrimEnd('.'); }
                    else if (!mant.Contains('.')) mant += ".";
                    r = mant + exp;
                    return char.IsUpper(conv) ? r.ToUpperInvariant() : r;
                }
        }
    }

    /// <summary>%e body "d.ddde+XX". The standard "e" format rounds the exact binary value (ties to even) like printf;
    /// custom formats such as "0.00e+00" round a 15-digit approximation instead (2.675 gave 2.68e+00; C prints 2.67e+00).</summary>
    static string ExpFormat(double ad, int prec)
    {
        var s = ad.ToString("e" + Math.Min(prec, 300).ToString(Inv), Inv);   // d.ddde+XXX
        int e = s.IndexOf('e');
        string digits = s[(e + 2)..].TrimStart('0').PadLeft(2, '0');
        return s[..(e + 2)] + digits;
    }

    // ======================= scanf =======================
    public sealed class InputReader
    {
        readonly string _s;
        public int Pos;
        public InputReader(string s, int pos) { _s = s; Pos = pos; }
        public bool AtEnd => Pos >= _s.Length;
        public int Peek() => Pos < _s.Length ? _s[Pos] : -1;
        public void SkipWs() { while (Pos < _s.Length && char.IsWhiteSpace(_s[Pos])) Pos++; }

        public long? ReadInt(int radix, int width, bool unsignedOk)
        {
            SkipWs();
            int st = Pos, end = Math.Min(_s.Length, width == int.MaxValue ? _s.Length : Pos + width);
            int p = Pos;
            bool neg = false;
            if (p < end && (_s[p] == '-' || _s[p] == '+')) { neg = _s[p] == '-'; p++; }
            if (radix == 0)
            {
                if (p + 1 < end && _s[p] == '0' && (_s[p + 1] == 'x' || _s[p + 1] == 'X')) { radix = 16; p += 2; }
                else if (p < end && _s[p] == '0') radix = 8;
                else radix = 10;
            }
            else if (radix == 16 && p + 1 < end && _s[p] == '0' && (_s[p + 1] == 'x' || _s[p + 1] == 'X')) p += 2;
            int ds = p;
            ulong v = 0;
            while (p < end)
            {
                int d = Digit(_s[p]);
                if (d < 0 || d >= radix) break;
                v = unchecked(v * (ulong)radix + (ulong)d);
                p++;
            }
            if (p == ds) { Pos = st; return null; }
            Pos = p;
            long r = unchecked((long)v);
            return neg ? unchecked(-r) : r;
        }

        static int Digit(char c) => c >= '0' && c <= '9' ? c - '0' : c >= 'a' && c <= 'z' ? c - 'a' + 10 : c >= 'A' && c <= 'Z' ? c - 'A' + 10 : -1;

        public double? ReadDouble(int width)
        {
            SkipWs();
            int st = Pos, end = Math.Min(_s.Length, width == int.MaxValue ? _s.Length : Pos + width);
            int p = Pos;
            if (p < end && (_s[p] == '-' || _s[p] == '+')) p++;
            int ds = p;
            while (p < end && char.IsDigit(_s[p])) p++;
            if (p < end && _s[p] == '.') { p++; while (p < end && char.IsDigit(_s[p])) p++; }
            if (p == ds || (p == ds + 1 && _s[ds] == '.')) { Pos = st; return null; }
            if (p < end && (_s[p] == 'e' || _s[p] == 'E'))
            {
                int q = p + 1;
                if (q < end && (_s[q] == '-' || _s[q] == '+')) q++;
                if (q < end && char.IsDigit(_s[q])) { while (q < end && char.IsDigit(_s[q])) q++; p = q; }
            }
            Pos = p;
            return double.TryParse(_s[st..p], NumberStyles.Float, Inv, out var d) ? d : 0;
        }

        public string ReadWord(int width)
        {
            SkipWs();
            int st = Pos;
            while (Pos < _s.Length && !char.IsWhiteSpace(_s[Pos]) && Pos - st < width) Pos++;
            return _s[st..Pos];
        }

        public char Next() => _s[Pos++];
    }

    public static int Scan(CInterpreter ip, string fmt, CVal[] args, int argi, InputReader r, string fn)
    {
        int count = 0;
        bool anyInput = false;
        for (int i = 0; i < fmt.Length; i++)
        {
            char c = fmt[i];
            if (char.IsWhiteSpace(c)) { r.SkipWs(); continue; }
            if (c != '%')
            {
                if (r.Peek() == c) { r.Pos++; continue; }
                return count;
            }
            i++;
            if (i >= fmt.Length) break;
            if (fmt[i] == '%') { r.SkipWs(); if (r.Peek() == '%') { r.Pos++; continue; } return count; }
            bool suppress = false;
            if (fmt[i] == '*') { suppress = true; i++; }
            int width = 0;
            while (i < fmt.Length && char.IsDigit(fmt[i])) { width = width * 10 + (fmt[i] - '0'); i++; }
            if (width == 0) width = int.MaxValue;
            string len = "";
            while (i < fmt.Length && "hlLzjq".IndexOf(fmt[i]) >= 0) { len += fmt[i]; i++; }
            if (i >= fmt.Length) break;
            char conv = fmt[i];
            if (conv != 'c' && conv != '[' && conv != 'n') r.SkipWs();
            CVal target = default;
            if (!suppress && conv != 'n')
            {
                if (argi >= args.Length) throw ip.RtErr(L.F("{0}: o formato \"{1}\" pede mais variáveis do que foram passadas", fn, Escape(fmt)));
                target = args[argi++];
                if (!target.T.IsPtr || target.L == 0)
                    throw ip.RtErr(L.F("{0}: o argumento {1} precisa ser um endereço — você esqueceu o & antes da variável? (ex.: scanf(\"%d\", &x))", fn, argi));
                CheckScanTarget(ip, fn, len, conv, target.T.Elem!);
            }
            if (r.AtEnd && conv != 'n') return count == 0 && !anyInput ? -1 : count;
            switch (conv)
            {
                case 'd':
                case 'i':
                case 'u':
                case 'x':
                case 'X':
                case 'o':
                    {
                        int radix = conv switch { 'd' or 'u' => 10, 'i' => 0, 'o' => 8, _ => 16 };
                        var v = r.ReadInt(radix, width, conv != 'd');
                        if (v == null) return count;
                        anyInput = true;
                        if (!suppress) { StoreScanned(ip, target, CVal.LongV(v.Value), fn); count++; }
                        break;
                    }
                case 'f':
                case 'e':
                case 'g':
                case 'E':
                case 'G':
                case 'a':
                    {
                        var v = r.ReadDouble(width);
                        if (v == null) return count;
                        anyInput = true;
                        if (!suppress) { StoreScanned(ip, target, CVal.Dbl(v.Value), fn); count++; }
                        break;
                    }
                case 'c':
                    {
                        int n = width == int.MaxValue ? 1 : width;
                        if (r.AtEnd) return count == 0 ? -1 : count;
                        anyInput = true;
                        for (int k = 0; k < n && !r.AtEnd; k++)
                        {
                            char ch = r.Next();
                            if (!suppress) ip.Write(target.L + k, (sbyte)(byte)ch);
                        }
                        if (!suppress) count++;
                        break;
                    }
                case 's':
                    {
                        var w = r.ReadWord(width);
                        if (w.Length == 0) return count;
                        anyInput = true;
                        if (!suppress) { ip.WriteCString(target.L, w); count++; }
                        break;
                    }
                case '[':
                    {
                        int close = fmt.IndexOf(']', i + 2);
                        if (close < 0) throw ip.RtErr(L.F("{0}: %[ sem ]", fn));
                        string set = fmt[(i + 1)..close];
                        i = close;
                        bool negate = set.StartsWith("^");
                        if (negate) set = set[1..];
                        var sb = new StringBuilder();
                        while (!r.AtEnd && sb.Length < width)
                        {
                            char ch = (char)r.Peek();
                            bool inSet = InSet(set, ch);
                            if (inSet == negate) break;
                            sb.Append(r.Next());
                        }
                        if (sb.Length == 0) return count;
                        anyInput = true;
                        if (!suppress) { ip.WriteCString(target.L, sb.ToString()); count++; }
                        break;
                    }
                case 'n':
                    if (!suppress && argi < args.Length) { var t = args[argi++]; if (t.T.IsPtr) StoreScanned(ip, t, CVal.LongV(r.Pos), fn); }
                    break;
                default:
                    throw ip.RtErr(L.F("{0}: formato desconhecido '%{1}'", fn, conv));
            }
        }
        return count;
    }

    /// <summary>A scanf conversion that doesn't match the variable writes the wrong number of bytes in real C (the
    /// value comes out as garbage, or a neighbour is overwritten). The lessons teach the matching pairs
    /// (%d int, %ld long, %f float, %lf double, %c char, %s char[]), so a mismatch stops the program with the fix.</summary>
    static void CheckScanTarget(CInterpreter ip, string fn, string len, char conv, CType et)
    {
        if (et.K == TK.Array) et = et.Elem!;
        string spec = "%" + len + conv;
        switch (conv)
        {
            case 'd': case 'i': case 'u': case 'x': case 'X': case 'o':
                {
                    if (!et.IsArith || et.K == TK.Bool) return;
                    var want = len switch { "" => TK.Int, "h" => TK.Short, "hh" => TK.Char, _ => TK.Long };
                    if (et.IsFloat)
                        throw ip.RtErr(L.F("{0}: {1} lê um número inteiro, mas a variável é {2}. Para {2} use {3}.", fn, spec, et, ScanSpecFor(et, conv)));
                    if (et.K != want)
                        throw ip.RtErr(L.F("{0}: {1} não combina com uma variável {2}. Para {2} use {3}.", fn, spec, et, ScanSpecFor(et, conv)));
                    return;
                }
            case 'f': case 'e': case 'g': case 'E': case 'G': case 'a':
                {
                    if (!et.IsArith || et.K == TK.Bool) return;
                    if (et.IsInteger)
                        throw ip.RtErr(L.F("{0}: {1} lê um número com casas decimais, mas a variável é {2}. Para {2} use {3}.", fn, spec, et, ScanSpecFor(et, 'd')));
                    bool wantDouble = len is "l" or "L";
                    if (et.K == TK.Double && !wantDouble)
                        throw ip.RtErr(L.F("{0}: {1} lê um float, mas a variável é double. Para double use %l{2} (no printf, %f serve para os dois).", fn, spec, conv));
                    if (et.K == TK.Float && wantDouble)
                        throw ip.RtErr(L.F("{0}: {1} lê um double, mas a variável é float. Para float use %{2}.", fn, spec, conv));
                    return;
                }
            case 'c':
            case 's':
            case '[':
                if (et.K != TK.Char)
                    throw ip.RtErr(conv == 'c'
                        ? L.F("{0}: %c lê um caractere e precisa de uma variável char, mas a variável é {1}.", fn, et)
                        : L.F("{0}: {1} lê um texto e precisa de um vetor de char (ex.: char nome[50];), mas recebeu {2}.", fn, "%" + conv, et));
                return;
        }
    }

    static string ScanSpecFor(CType t, char conv) => t.K switch
    {
        TK.Double => "%lf",
        TK.Float => "%f",
        TK.Char => "%c",
        TK.Short => "%h" + conv,
        TK.Long => "%l" + conv,
        _ => "%" + conv
    };

    static bool InSet(string set, char c)
    {
        for (int k = 0; k < set.Length; k++)
        {
            if (k + 2 < set.Length && set[k + 1] == '-') { if (c >= set[k] && c <= set[k + 2]) return true; k += 2; continue; }
            if (set[k] == c) return true;
        }
        return false;
    }

    static void StoreScanned(CInterpreter ip, CVal ptr, CVal v, string fn)
    {
        var et = ptr.T.Elem!;
        if (!et.IsArith) throw ip.RtErr(L.F("{0}: não é possível ler um número para uma variável do tipo {1}", fn, et));
        ip.Store(ptr.L, et, v);
    }
}
