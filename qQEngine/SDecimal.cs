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

    public static SDecimal PositiveInfinity { get; } = new();
    public static SDecimal NegativeInfinity { get; } = new();
    
    public static SDecimal DoubleEpsilon { get; } = new(double.Epsilon, 0);
    public static SDecimal DoubleMaxValue { get; } = new(double.MaxValue, 0);
    public static SDecimal DoubleMinValue { get; } = new(double.MinValue, 0);
    
    private double _mantissa;
    public double Mantissa
    {
        readonly get
        {
            if (_infinite) throw new ArithmeticException("Infinite ScientificDecimal has no mantissa");
            return _mantissa;
        }
        private set
        {
            if (_infinite) throw new ArithmeticException("Cannot set mantissa of infinite ScientificDecimal");
            _mantissa = value;
        }
    }

    private int _exponent;
    public int Exponent
    {
        readonly get
        {
            if (_infinite) throw new ArithmeticException("Infinite ScientificDecimal has no exponent");
            return _exponent;
        }
        private set
        {
            if (_infinite) throw new ArithmeticException("Cannot set exponent of infinite ScientificDecimal");
            _exponent = value;
        }
    }
    
    private readonly bool _infinite = false;
    
    public readonly bool Positive => double.IsPositive(_mantissa);
    public readonly bool Negative => double.IsNegative(_mantissa);
    
    public SDecimal(double mantissa, int exponent)
    {
        Mantissa = mantissa;
        Exponent = exponent;
        Normalize();
    }
    
    public SDecimal(int exponent)
        : this(1, exponent) {}
    
    public SDecimal()
        : this(0, 0) {}

    private SDecimal(bool positive)
        : this(positive ? 1 : -1, 0)
    {
        _infinite = true;
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

        if (absMantissa is >= 10 and < 1e+10)
        {
            while (Math.Abs(Mantissa) >= 10)
            {
                Mantissa /= 10;
                Exponent++;
            }
            return;
        }

        if (absMantissa is < 1 and >= 1e-10)
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

    private void IncreaseExponent(int exponent)
    {
        int exponentDiff = exponent - Exponent;
        switch (exponentDiff)
        {
            case 0:
                return;
            case < 0:
                throw new ArgumentException("Exponent argument must be greater than or equal to this number's exponent");
            case < 10:
            {
                while (Exponent != exponent)
                {
                    Exponent++;
                    Mantissa /= 10;
                }
                
                return;
            }
            default:
                Exponent = exponent;
                Mantissa /= Math.Pow(10, exponentDiff);
                break;
        }
    }
    
    /// <summary>
    /// Calculates the remainder between two values.
    /// </summary>
    /// <param name="dividend">Value to divide.</param>
    /// <param name="divisor">Divisor.</param>
    /// <returns>The remainder of the dividend divided by the divisor.</returns>
    /// <exception cref="ArithmeticException">
    /// Attempted to calculate the remainder of a division by zero or attempted to calculate the remainder of infinity
    /// </exception>
    private static SDecimal Remainder(SDecimal dividend, SDecimal divisor)
    {
        if (divisor == 0) throw new ArithmeticException("Cannot calculate the remainder of a division by zero");
        if (dividend._infinite || divisor._infinite) 
            throw new ArithmeticException("Cannot calculate the remainder of infinity");
        return dividend - divisor * Math.Truncate((double)(dividend / divisor));
    }
    
    public static SDecimal Mod(SDecimal value, SDecimal mod)
    {
        if (mod == 0) throw new ArithmeticException("Cannot modulate by zero");
        if (value._infinite || mod._infinite) throw new ArithmeticException("Cannot modulate infinity or by infinity");
        return value - mod * Math.Floor((double)(value / mod));
    }
    
    /// <summary>
    /// Calculates the square of a value.
    /// </summary>
    /// <param name="value">Value to square.</param>
    /// <returns>The value multiplied by itself.</returns>
    public static SDecimal Square(SDecimal value)
        => value * value;

    /// <summary>
    /// Calculates an integer power of a value.
    /// </summary>
    /// <param name="value">Base value.</param>
    /// <param name="amount">Exponent value.</param>
    /// <returns>The base raised to the exponent.</returns>
    public static SDecimal IntPow(SDecimal value, int amount)
    {
        SDecimal result = One;
        if (amount > 0)
            for (int i = 0; i < amount; ++i)
                result *= value;
        if (amount < 0)
            for (int i = 0; i < -amount; ++i)
                result /= value;
        return result;
    }

    /// <summary>
    /// Calculates the square root of a value.
    /// </summary>
    /// <param name="value">Value to square root.</param>
    /// <returns>The square root of the value.</returns>
    /// <exception cref="ArithmeticException">Attempted to take the square root of a negative number</exception>
    public static SDecimal Sqrt(SDecimal value)
    {
        if (value.Negative) throw new ArithmeticException("Cannot take the square root of a negative SDecimal");
        if (value._infinite) return value;
        if (value.Exponent % 2 != 0) value.IncreaseExponent(value.Exponent + 1);
        return new SDecimal(Math.Sqrt(value.Mantissa), value.Exponent / 2);
    }
    
    public static double Atan2(SDecimal y, SDecimal x)
    {
        double quotient = ConvertToDoubleSaturating(y / x);
        if (x > Zero) return Math.Atan(quotient);
        if (x < Zero && y >= Zero) return Math.Atan(quotient) + Math.PI;
        if (x < Zero && y < Zero) return Math.Atan(quotient) - Math.PI;
        if (x == Zero & y > Zero) return Math.PI / 2;
        if (x == Zero & y < Zero) return -Math.PI / 2;
        throw new DivideByZeroException("Cannot calculate atan2 of 0 / 0");
    }

    public static double Cos(SDecimal value)
        => Math.Cos((double)(value % Math.Tau));

    public static double Sin(SDecimal value)
        => Math.Sin((double)(value % Math.Tau));

    public static double Tan(SDecimal value)
        => Math.Tan((double)(value % Math.PI));

    public static SDecimal Abs(SDecimal value)
    {
        if (value._infinite) return new SDecimal(true);
        return new (Math.Abs(value.Mantissa), value.Exponent);
    }

    public static SDecimal Min(SDecimal value, params SDecimal[] values)
    {
        SDecimal result = value;
        foreach (var n in values)
            if (n < result) result = n;
        return result;
    }
    
    public static SDecimal Max(SDecimal value, params SDecimal[] values)
    {
        SDecimal result = value;
        foreach (var n in values)
            if (n > result) result = n;
        return result;
    }

    public static SDecimal Round(SDecimal value, MidpointRounding mode = MidpointRounding.ToEven)
    {
        if (value._infinite) throw new ArithmeticException("Cannot round infinite SDecimal");
        if (value.Mantissa == 0) return value;
        if (value.Exponent < -1) return 0;
        if (value.Exponent == -1) return new(double.Round(value.Mantissa * 0.1, mode), 0);
        return new(double.Round(value.Mantissa, Math.Clamp(value.Exponent, 0, 15), mode), value.Exponent);
    }

    public static SDecimal Floor(SDecimal value)
    {
        if (value._infinite) throw new ArithmeticException("Cannot round infinite SDecimal");
        if (value.Mantissa == 0) return value;
        SDecimal roundDiff = value - Round(value);
        if (roundDiff < Zero) return value - One - roundDiff;
        return value - roundDiff;
    }

    public static SDecimal Ceiling(SDecimal value)
    {
        if (value._infinite) throw new ArithmeticException("Cannot round infinite SDecimal");
        if (value.Mantissa == 0) return value;
        SDecimal roundDiff = value - Round(value);
        if (roundDiff > Zero) return value + One - roundDiff;
        return value - roundDiff;
    }

    [Obsolete("MinMagnitude is obsolete, Use Min method instead.")]
    public static SDecimal MinMagnitude(SDecimal x, SDecimal y)
        => Min(x, y);

    public static SDecimal MinMagnitudeNumber(SDecimal x, SDecimal y)
        => IsInfinity(x) ? IsInfinity(y) ? throw new ArithmeticException() : y : IsInfinity(y) ? x : Min(x, y);
    
    [Obsolete("MaxMagnitude is obsolete. Use Max method instead.")]
    public static SDecimal MaxMagnitude(SDecimal x, SDecimal y)
        => Max(x, y);

    public static SDecimal MaxMagnitudeNumber(SDecimal x, SDecimal y)
        => IsInfinity(x) ? IsInfinity(y) ? throw new ArithmeticException() : y : IsInfinity(y) ? x : Max(x, y);

    public static SDecimal Clamp(SDecimal value, SDecimal min, SDecimal max)
    {
        if (max < min) throw new ArgumentException("SDecimal clamp maximum cannot be less than the minimum");
        return value < min ? min : value > max ? max : value;
    }

    public static SDecimal operator +(SDecimal value)
        => value;

    public static SDecimal operator -(SDecimal value)
        => new(-value.Mantissa, value.Exponent);
    
    public static SDecimal operator +(SDecimal left, SDecimal right)
    {
        if (left.Exponent > right.Exponent) left.IncreaseExponent(right.Exponent);
        if (right.Exponent > left.Exponent) right.IncreaseExponent(left.Exponent);
        return new(left.Mantissa + right.Mantissa, left.Exponent);
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
    
    // to ScientificDecimal
    public static implicit operator SDecimal(int value) 
        => new(value, 0);

    public static implicit operator SDecimal(double value)
        => FromDouble(value);
    
    public static implicit operator SDecimal(float value) 
        => FromDouble(value);

    // from ScientificDecimal
    public static explicit operator double(SDecimal value)
        => ConvertToDoubleSaturating(value);
    
    public static explicit operator float(SDecimal value)
        => Convert.ToSingle((double)value);
    
    public static explicit operator int(SDecimal value)
        => (int)ConvertToDoubleSaturating(value);
    
    public static explicit operator uint(SDecimal value)
        => (uint)ConvertToDoubleSaturating(value);
    
    public static explicit operator long (SDecimal value)
        => (long)ConvertToDoubleSaturating(value);
    
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
    
    private static SDecimal FromDouble(double value)
    {
        if (double.IsPositiveInfinity(value)) return PositiveInfinity;
        if (double.IsNegativeInfinity(value)) return NegativeInfinity;
        if (double.IsNaN(value)) throw new ArgumentException("Cannot convert NaN into an SDecimal");
        return new(value, 0);
    }
    
    private static double ConvertToDoubleSaturating(SDecimal value)
    {
        if (IsPositiveInfinity(value)) return double.PositiveInfinity;
        if (IsNegativeInfinity(value)) return double.NegativeInfinity;
        if (Abs(value) < DoubleEpsilon) return 0;
        if (value > DoubleMaxValue) return double.MaxValue;
        if (value < DoubleMinValue) return double.MinValue;
        return value.Mantissa * Math.Pow(10, value.Exponent);
    }

    private static double ConvertToDoubleUnchecked(SDecimal value)
    {
        if (IsPositiveInfinity(value)) return double.PositiveInfinity;
        if (IsNegativeInfinity(value)) return double.NegativeInfinity;
        return value.Mantissa * Math.Pow(10, value.Exponent);
    }
    
    public static bool TryConvertFromChecked<TOther>(TOther value, out SDecimal result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }
    public static bool TryConvertFromSaturating<TOther>(TOther value, out SDecimal result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }
    public static bool TryConvertFromTruncating<TOther>(TOther value, out SDecimal result) where TOther : INumberBase<TOther>
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
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out SDecimal result)
    {
        throw new NotImplementedException();
    }
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out SDecimal result)
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
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out SDecimal result)
    {
        throw new NotImplementedException();
    }
    public static SDecimal Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out SDecimal result)
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