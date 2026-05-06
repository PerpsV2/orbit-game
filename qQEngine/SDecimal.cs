using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace qQEngine;

public struct SDecimal : INumber<SDecimal>
{
    public static SDecimal Zero { get; } = new(0, 0);
    public static SDecimal One { get; } = new(1, 0);
    public static SDecimal AdditiveIdentity { get; } = new(0, 0);
    public static SDecimal MultiplicativeIdentity { get; } = new(1, 0);
    public static int Radix { get; } = 10;
    
    public double Mantissa { get; private set; }
    public int Exponent { get; private set; }
    
    public SDecimal(double mantissa, int exponent)
    {
        Mantissa = mantissa;
        Exponent = exponent;
        Normalize();
    }

    private void Normalize()
    {
        double absMantissa = Math.Abs(Mantissa);
        if (absMantissa is >= 1 and < 10) return;
        if (absMantissa == 0)
        {
            Exponent = 0;
            return;
        }

        if (absMantissa is >= 10 and < 100000)
        {
            while (Math.Abs(Mantissa) >= 10)
            {
                Mantissa /= 10;
                Exponent++;
            }
            return;
        }

        if (absMantissa is < 1 and >= 0.00001)
        {
            while (Math.Abs(Mantissa) < 1)
            {
                Mantissa *= 10;
                Exponent--;
            }
            return;
        }

        double exponentDiff = Math.Ceiling(Math.Log10(absMantissa));
        Mantissa *= Math.Pow(10, -exponentDiff);
        Exponent += (int)exponentDiff;
    }

    public static SDecimal operator +(SDecimal value)
        => value;

    public static SDecimal operator -(SDecimal value)
        => new(-value.Mantissa, value.Exponent);
    
    public static SDecimal operator +(SDecimal left, SDecimal right)
    {
        throw new NotImplementedException();
    }

    public static SDecimal operator -(SDecimal left, SDecimal right)
        => left + -right;

    public static SDecimal operator ++(SDecimal value)
        => value + One;

    public static SDecimal operator --(SDecimal value)
        => value - One;

    public static SDecimal operator *(SDecimal left, SDecimal right)
        => new(left.Mantissa * right.Mantissa, left.Exponent + right.Exponent);

    public static SDecimal operator /(SDecimal left, SDecimal right)
        => new(left.Mantissa / right.Mantissa, left.Exponent - right.Exponent);
    
    public static SDecimal operator %(SDecimal left, SDecimal right)
    {
        throw new NotImplementedException();
    }
    
    public static bool operator ==(SDecimal left, SDecimal right)
    {
        throw new NotImplementedException();
    }
    
    public static bool operator !=(SDecimal left, SDecimal right)
    {
        throw new NotImplementedException();
    }
    
    public static bool operator >(SDecimal left, SDecimal right)
    {
        throw new NotImplementedException();
    }
    
    public static bool operator >=(SDecimal left, SDecimal right)
    {
        throw new NotImplementedException();
    }
    
    public static bool operator <(SDecimal left, SDecimal right)
    {
        throw new NotImplementedException();
    }
    
    public static bool operator <=(SDecimal left, SDecimal right)
    {
        throw new NotImplementedException();
    }
    
    public static SDecimal Abs(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsCanonical(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsComplexNumber(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsEvenInteger(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsFinite(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsImaginaryNumber(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsInfinity(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsInteger(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsNaN(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsNegative(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsNegativeInfinity(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsNormal(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsOddInteger(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsPositive(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsPositiveInfinity(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsRealNumber(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsSubnormal(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static bool IsZero(SDecimal value)
    {
        throw new NotImplementedException();
    }
    public static SDecimal MaxMagnitude(SDecimal x, SDecimal y)
    {
        throw new NotImplementedException();
    }
    public static SDecimal MaxMagnitudeNumber(SDecimal x, SDecimal y)
    {
        throw new NotImplementedException();
    }
    public static SDecimal MinMagnitude(SDecimal x, SDecimal y)
    {
        throw new NotImplementedException();
    }
    public static SDecimal MinMagnitudeNumber(SDecimal x, SDecimal y)
    {
        throw new NotImplementedException();
    }
    public static bool TryConvertFromChecked<TOther>(TOther value, [MaybeNullWhen(false)] out SDecimal result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }
    public static bool TryConvertFromSaturating<TOther>(TOther value, [MaybeNullWhen(false)] out SDecimal result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }
    public static bool TryConvertFromTruncating<TOther>(TOther value, [MaybeNullWhen(false)] out SDecimal result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }
    public static bool TryConvertToChecked<TOther>(SDecimal value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }
    public static bool TryConvertToSaturating<TOther>(SDecimal value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }
    public static bool TryConvertToTruncating<TOther>(SDecimal value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out SDecimal result)
    {
        throw new NotImplementedException();
    }
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out SDecimal result)
    {
        throw new NotImplementedException();
    }
    public int CompareTo(object? obj)
    {
        throw new NotImplementedException();
    }
    public bool Equals(SDecimal? other)
    {
        throw new NotImplementedException();
    }
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        throw new NotImplementedException();
    }
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    public static SDecimal Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out SDecimal result)
    {
        throw new NotImplementedException();
    }
    public static SDecimal Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out SDecimal result)
    {
        throw new NotImplementedException();
    }
    public static SDecimal Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    public static SDecimal Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public int CompareTo(SDecimal other)
    {
        throw new NotImplementedException();
    }
    
    public override bool Equals(object? obj)
    {
        return obj is SDecimal other && Equals(other);
    }

    public bool Equals(SDecimal other)
    {
        throw new NotImplementedException();
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Mantissa, Exponent);
    }
}