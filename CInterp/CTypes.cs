using System.Globalization;

namespace EasyCreator.CInterp;

/// <summary>Type kinds, ordered so that Bool..Long are the integer kinds (rank = order).</summary>
public enum TK : byte { Void, Bool, Char, Short, Int, Long, Float, Double, Ptr, Array, Struct, Native }

/// <summary>A C type. Memory is a flat array of 64-bit cells: every scalar takes one cell, aggregates
/// take one cell per scalar inside them. <see cref="Bytes"/> reports the real C size for sizeof.</summary>
public sealed class CType
{
    public TK K;
    public bool Unsigned;
    public CType? Elem;          // Ptr / Array
    public long Len = -1;        // Array length; -1 = unknown
    public StructDef? S;         // Struct
    public NativeClass? N;       // Native (library object such as Servo)
    CType? _ptr;

    public CType(TK k, bool unsigned = false) { K = k; Unsigned = unsigned; }

    public static readonly CType Void = new(TK.Void), Bool = new(TK.Bool),
        Char = new(TK.Char), UChar = new(TK.Char, true),
        Short = new(TK.Short), UShort = new(TK.Short, true),
        Int = new(TK.Int), UInt = new(TK.Int, true),
        Long = new(TK.Long), ULong = new(TK.Long, true),
        Float = new(TK.Float), Double = new(TK.Double);

    public CType Pointer => _ptr ??= new CType(TK.Ptr) { Elem = this };
    public static CType ArrayOf(CType elem, long len) => new(TK.Array) { Elem = elem, Len = len };

    public bool IsInteger => K >= TK.Bool && K <= TK.Long;
    public bool IsFloat => K == TK.Float || K == TK.Double;
    public bool IsArith => K >= TK.Bool && K <= TK.Double;
    public bool IsPtr => K == TK.Ptr;
    public bool IsScalar => K >= TK.Bool && K <= TK.Ptr;
    public bool IsAggregate => K == TK.Array || K == TK.Struct;

    public long Cells => K switch
    {
        TK.Array => Math.Max(0, Len) * Elem!.Cells,
        TK.Struct => S!.Cells,
        _ => 1
    };

    public long Bytes => K switch
    {
        TK.Void or TK.Bool or TK.Char => 1,
        TK.Short => 2,
        TK.Int or TK.Float => 4,
        TK.Array => Math.Max(0, Len) * Elem!.Bytes,
        TK.Struct => S!.Bytes,
        _ => 8
    };

    public long Align => K switch
    {
        TK.Array => Elem!.Align,
        TK.Struct => S!.Align,
        _ => Math.Max(1, Bytes)
    };

    /// <summary>Integer promotion: bool/char/short become int.</summary>
    public CType Promoted => IsInteger && K < TK.Int ? Int : this;

    public override string ToString() => K switch
    {
        TK.Void => "void",
        TK.Bool => "bool",
        TK.Char => Unsigned ? "unsigned char" : "char",
        TK.Short => Unsigned ? "unsigned short" : "short",
        TK.Int => Unsigned ? "unsigned int" : "int",
        TK.Long => Unsigned ? "unsigned long" : "long",
        TK.Float => "float",
        TK.Double => "double",
        TK.Ptr => Elem + "*",
        TK.Array => Elem + "[" + (Len >= 0 ? Len.ToString(CultureInfo.InvariantCulture) : "") + "]",
        TK.Struct => "struct " + S!.Name,
        TK.Native => N!.Name,
        _ => "?"
    };

    /// <summary>Usual arithmetic conversions.</summary>
    public static CType Common(CType a, CType b)
    {
        if (a.K == TK.Double || b.K == TK.Double) return Double;
        if (a.K == TK.Float || b.K == TK.Float) return Float;
        a = a.Promoted; b = b.Promoted;
        if (a.K != b.K) return a.K > b.K ? a : b;
        return a.Unsigned ? a : b;
    }
}

public sealed class Field
{
    public string Name = "";
    public CType T = CType.Int;
    public long Offset;      // in cells
}

public sealed class StructDef
{
    public string Name = "";
    public readonly List<Field> Fields = new();
    public readonly Dictionary<string, Field> ByName = new();
    public long Cells, Bytes, Align = 1;
    public bool Complete;

    public void Add(string name, CType t)
    {
        var f = new Field { Name = name, T = t, Offset = Cells };
        Fields.Add(f);
        ByName[name] = f;
        Cells += t.Cells;
        long al = t.Align;
        Bytes = (Bytes + al - 1) / al * al + t.Bytes;
        Align = Math.Max(Align, al);
    }

    public void Finish()
    {
        Bytes = (Bytes + Align - 1) / Align * Align;
        if (Bytes == 0) Bytes = 1;
        Complete = true;
    }
}

/// <summary>A runtime value. Integers and pointers use <see cref="L"/>, floating types use <see cref="D"/>.
/// Struct values carry the address of the struct in <see cref="L"/>; native objects carry their handle.</summary>
public struct CVal
{
    public CType T;
    public long L;
    public double D;

    public static CVal Of(CType t, long v) => new() { T = t, L = v };
    public static CVal Int(long v) => new() { T = CType.Int, L = (int)v };
    public static CVal LongV(long v) => new() { T = CType.Long, L = v };
    public static CVal Dbl(double v) => new() { T = CType.Double, D = v };
    public static CVal BoolV(bool b) => new() { T = CType.Int, L = b ? 1 : 0 };
    public static readonly CVal VoidV = new() { T = CType.Void };

    public bool IsTrue => T.IsFloat ? D != 0 : L != 0;

    public double AsDouble => T.IsFloat ? D : (T.Unsigned && T.K == TK.Long ? (double)(ulong)L : L);

    public long AsLong => T.IsFloat ? Conv.D2L(D) : L;

    public override string ToString() => T.IsFloat ? D.ToString(CultureInfo.InvariantCulture) : L.ToString(CultureInfo.InvariantCulture);
}

public static class Conv
{
    public static long D2L(double d)
    {
        if (double.IsNaN(d)) return 0;
        if (d >= 9.2e18) return long.MaxValue;
        if (d <= -9.2e18) return long.MinValue;
        return (long)d;
    }

    /// <summary>Wraps an integer to the width and signedness of <paramref name="t"/>.</summary>
    public static long Norm(long v, CType t) => t.K switch
    {
        TK.Bool => v != 0 ? 1 : 0,
        TK.Char => t.Unsigned ? (byte)v : (sbyte)v,
        TK.Short => t.Unsigned ? (ushort)v : (short)v,
        TK.Int => t.Unsigned ? (uint)v : (int)v,
        _ => v
    };

    public static CVal To(CVal v, CType to)
    {
        if (ReferenceEquals(v.T, to)) return v;
        switch (to.K)
        {
            case TK.Float: return new CVal { T = to, D = (float)v.AsDouble };
            case TK.Double: return new CVal { T = to, D = v.AsDouble };
            case TK.Bool: return new CVal { T = to, L = v.IsTrue ? 1 : 0 };
            case TK.Char: case TK.Short: case TK.Int: case TK.Long:
                {
                    long l;
                    if (v.T.IsFloat)
                    {
                        double d = v.D;
                        l = to.Unsigned && d >= 9.2e18 ? (long)(ulong)Math.Min(d, 1.8e19) : D2L(d);
                    }
                    else l = v.L;
                    return new CVal { T = to, L = Norm(l, to) };
                }
            case TK.Ptr: return new CVal { T = to, L = v.T.IsFloat ? D2L(v.D) : v.L };
            case TK.Void: return new CVal { T = to };
            default: return v;
        }
    }
}
