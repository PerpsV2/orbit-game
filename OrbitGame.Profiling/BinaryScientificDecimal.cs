using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace OrbitGame.Profiling;

public class PrecisionException : Exception
{
    public PrecisionException() { }
    public PrecisionException(string message) : base(message) { }
}

/// <summary>
/// Number with decimal precision but arbitrary place value.
/// </summary>
public struct BinaryScientificDecimal : INumber<BinaryScientificDecimal>
{
    private const int MaxPrecision = 62;
    private const int SqrtMaxIterations = 16;
    private static readonly BinaryScientificDecimal SqrtEpsilon = new(0b1L, -62);

    private long _mantissa;
    private int _exponent;
    private int _precision;
    private readonly bool _infinite;

    public long Mantissa
    {
        get => _mantissa;
        private set
        {
            if (_infinite) throw new ArithmeticException("Cannot set the mantissa of an infinite ScientificDecimal");
            _mantissa = value;
        }
    }
    public int Exponent
    {
        get => _exponent;
        private set
        {
            if (_infinite) throw new ArithmeticException("Cannot set the mantissa of an infinite ScientificDecimal");
            _exponent = value;
        }
    }
    public int Precision
    {
        get => _precision;
        private set
        {
            if (_infinite) throw new ArithmeticException("Cannot set the mantissa of an infinite ScientificDecimal");
            _precision = value;
        }
    }

    public static BinaryScientificDecimal Zero => new(0, 0);
    public static BinaryScientificDecimal One => new(1, 0);
    public static BinaryScientificDecimal AdditiveIdentity => Zero;
    public static BinaryScientificDecimal MultiplicativeIdentity => One;
    public static BinaryScientificDecimal PositiveInfinity => new(true, true);
    public static BinaryScientificDecimal NegativeInfinity => new(true, false);
    public static int Radix => 2;


    public BinaryScientificDecimal(long mantissa, int exponent, int precision = MaxPrecision)
    {
        Mantissa = mantissa;
        Exponent = exponent;
        Precision = precision;
        Normalize();
    }
    
    public BinaryScientificDecimal(decimal mantissa, int exponent)
        => this = FromDecimal(mantissa, exponent);

    public BinaryScientificDecimal(int exponent)
        => this = FromDecimal(1, exponent);

    private BinaryScientificDecimal(bool isInfinite, bool isPositive)
    {
        Mantissa = isPositive ? 1 : -1;
        _infinite = isInfinite;
    }
    
    public readonly bool Positive => long.IsPositive(_mantissa);
    public readonly bool Negative => long.IsNegative(_mantissa);

    private static BinaryScientificDecimal FromDecimal(decimal mantissa, int exponent)
    {
        mantissa *= (decimal)Math.Pow(10, exponent);
        int decimalPlaces = BitConverter.GetBytes(decimal.GetBits(mantissa)[3])[2];
        mantissa *= (decimal)Math.Pow(10, decimalPlaces);
        BinaryScientificDecimal result = new BinaryScientificDecimal((long)mantissa, 0);
        for (int i = 0; i < decimalPlaces; ++i)
            result /= new BinaryScientificDecimal(10, 0);
        return result;
    }

    private void Normalize()
    {
        while (Math.Abs(Mantissa) < 1L << (Precision - 1))
        {
            Mantissa <<= 1;
            Exponent -= 1;
        }

        while (Math.Abs(Mantissa) > 1L << Precision)
        {
            Mantissa >>= 1;
            Exponent += 1;
        }
    }

    private void IncreaseExponent(int goal)
    {
        bool negative = Mantissa < 0;
        if (negative) Mantissa = -Mantissa;
        if (Exponent == goal) return;
        if (Exponent > goal) throw new ArithmeticException();
        Mantissa >>= goal - Exponent;
        Precision -= goal - Exponent;
        Exponent = goal;
        if (negative) Mantissa = -Mantissa;
    }

    private static BinaryScientificDecimal Add(BinaryScientificDecimal left, BinaryScientificDecimal right)
    {
        if (left._infinite && right._infinite)
        {
            if (left.Positive == right.Positive) return left;
            throw new ArithmeticException("Cannot add opposite signed scientific decimals");
        }
        if (left._infinite) return left;
        if (right._infinite) return right;
        
        if (left.Exponent < right.Exponent) left.IncreaseExponent(right.Exponent);
        else if (right.Exponent < left.Exponent) right.IncreaseExponent(left.Exponent);
        return new(left.Mantissa + right.Mantissa, 
            int.Max(left.Exponent, right.Exponent), 
            int.Min(left.Precision, right.Precision));
    }

    private static BinaryScientificDecimal Multiply(BinaryScientificDecimal left, BinaryScientificDecimal right)
    {
        if (left._mantissa == 0 || right._mantissa == 0) return Zero;
        if (left._infinite || right._infinite) return new(true, 
            (left.Positive && right.Positive) || (left.Negative && right.Negative));
        
        bool leftNegative = left.Mantissa < 0;
        bool rightNegative = right.Mantissa < 0;
        if (leftNegative) left.Mantissa *= -1;
        if (rightNegative) right.Mantissa *= -1;
        int resultPrecision = int.Min(left.Precision, right.Precision);
        left.Mantissa >>= left.Precision - resultPrecision / 2;
        right.Mantissa >>= right.Precision - resultPrecision / 2 - resultPrecision % 2;
        long resultMantissa = left.Mantissa * right.Mantissa;
        int resultExponent = left.Exponent + left.Precision + right.Exponent + right.Precision - resultPrecision;
        if (leftNegative ^ rightNegative) resultMantissa *= -1;
        return new BinaryScientificDecimal(resultMantissa, resultExponent, resultPrecision);
    }

    private static BinaryScientificDecimal Divide(BinaryScientificDecimal dividend, BinaryScientificDecimal divisor)
    {
        if (divisor._mantissa == 0 && dividend._mantissa == 0)
            throw new ArithmeticException("Cannot divide zero by zero");
        if (divisor._mantissa == 0) return dividend.Positive ? PositiveInfinity : NegativeInfinity;
        if (divisor._infinite && dividend._infinite)
            throw new ArithmeticException("Cannot divide an infinite value by an infinite value");
        if (dividend._infinite) return dividend;
        if (divisor._infinite) return Zero;
        
        bool leftNegative = dividend.Mantissa < 0;
        bool rightNegative = divisor.Mantissa < 0;
        if (leftNegative) dividend.Mantissa *= -1;
        if (rightNegative) divisor.Mantissa *= -1;
        int resultPrecision = int.Min(dividend.Precision, divisor.Precision);
        if (dividend.Exponent < divisor.Exponent) dividend.IncreaseExponent(divisor.Exponent);
        else if (divisor.Exponent < dividend.Exponent) divisor.IncreaseExponent(dividend.Exponent);
        long resultMantissa = 0b0L;
        divisor.Mantissa <<= dividend.Precision - divisor.Precision;
        for (int i = 0; i < resultPrecision; ++i)
        {
            if (dividend.Mantissa == 0) break;
            if (divisor.Mantissa <= dividend.Mantissa)
            {
                dividend.Mantissa -= divisor.Mantissa;
                resultMantissa += 0b1L << resultPrecision - i - 1;
            }
            divisor.Mantissa >>= 1;
        }
        int resultExponent = dividend.Precision + dividend.Exponent - divisor.Precision - divisor.Exponent - resultPrecision + 1;
        if (leftNegative ^ rightNegative) resultMantissa *= -1;
        return new(resultMantissa, resultExponent, resultPrecision);
    }

    private static BinaryScientificDecimal Modulo(BinaryScientificDecimal value, BinaryScientificDecimal mod)
    {
        if (mod._mantissa == 0) throw new ArithmeticException("Cannot modulate a value by zero");
        if (value._infinite || mod._infinite) 
            throw new ArithmeticException("Cannot modulate an infinite ScientificDecimal");
        return value - mod * Floor(value / mod);
    }

    public static BinaryScientificDecimal Square(BinaryScientificDecimal value)
        => value * value;

    public static BinaryScientificDecimal IntPow(BinaryScientificDecimal value, uint power)
    {
        BinaryScientificDecimal result = One;
        for (uint i = 0; i < power; ++i)
            result *= value;
        return result;
    }

    public static BinaryScientificDecimal Sqrt(BinaryScientificDecimal value)
    {
        if (value.Negative) throw new ArithmeticException("Cannot take the square root of a negative number");
        BinaryScientificDecimal bestGuess = value;
        BinaryScientificDecimal nextGuess = bestGuess;
        int iterations = 0;
        do
        {
            bestGuess = nextGuess;
            nextGuess = bestGuess + value / bestGuess;
            nextGuess._exponent--;
            iterations++;
            Console.WriteLine(iterations);
        } while (Abs(nextGuess - bestGuess) > SqrtEpsilon && iterations < SqrtMaxIterations);

        return bestGuess;
    }

    public static BinaryScientificDecimal Abs(BinaryScientificDecimal value)
    {
        if (value._infinite) return new(true, true);
        return new(long.Abs(value._mantissa), value._exponent, value._precision);
    }
    
    public static BinaryScientificDecimal Min(BinaryScientificDecimal value, params BinaryScientificDecimal[] values)
    {
        BinaryScientificDecimal result = value;
        foreach (var n in values)
            if (n < result) result = n;
        return result;
    }
    
    public static BinaryScientificDecimal Max(BinaryScientificDecimal value, params BinaryScientificDecimal[] values)
    {
        BinaryScientificDecimal result = value;
        foreach (var n in values)
            if (n > result) result = n;
        return result;
    }
    
    [Obsolete("MinMagnitude is obsolete, Use Min method instead.")]
    public static BinaryScientificDecimal MinMagnitude(BinaryScientificDecimal x, BinaryScientificDecimal y)
        => Min(x, y);

    public static BinaryScientificDecimal MinMagnitudeNumber(BinaryScientificDecimal x, BinaryScientificDecimal y)
        => IsNaN(x) ? IsNaN(y) ? throw new ArithmeticException() : y : IsNaN(y) ? x : Min(x, y);

    [Obsolete("MaxMagnitude is obsolete. Use Max method instead.")]
    public static BinaryScientificDecimal MaxMagnitude(BinaryScientificDecimal x, BinaryScientificDecimal y)
        => Max(x, y);

    public static BinaryScientificDecimal MaxMagnitudeNumber(BinaryScientificDecimal x, BinaryScientificDecimal y)
        => IsNaN(x) ? IsNaN(y) ? throw new ArithmeticException() : y : IsNaN(y) ? x : Max(x, y);

    public static BinaryScientificDecimal Clamp(
        BinaryScientificDecimal value, 
        BinaryScientificDecimal min, 
        BinaryScientificDecimal max)
    {
        if (max < min) throw new ArithmeticException("Clamp maximum cannot be less than the minimum");
        return value < min ? min : value > max ? max : value;
    }

    public static BinaryScientificDecimal Round(BinaryScientificDecimal value)
    {
        bool negative = value._mantissa < 0;
        if (negative) value._mantissa *= -1;
        if (IsInteger(value)) return value;
        return negative ? ((value._mantissa >> -value._exponent - 1) & 0b1L) == 0 ? -Floor(value) : -Ceil(value) :
            ((value._mantissa >> -value._exponent - 1) & 0b1L) == 1 ? Ceil(value) : Floor(value);
    }

    public static BinaryScientificDecimal Floor(BinaryScientificDecimal value)
    {
        bool negative = value._mantissa < 0;
        if (negative) value._mantissa *= -1;
        if (IsInteger(value)) return value;
        value._mantissa >>= -value._exponent;
        value._mantissa <<= -value._exponent;
        if (negative)
        {
            value += new BinaryScientificDecimal(1, 0);
            value._mantissa *= -1;
        }
        return value;
    }

    public static BinaryScientificDecimal Ceil(BinaryScientificDecimal value)
    {
        bool negative = value._mantissa < 0;
        if (negative) value._mantissa *= -1;
        if (IsInteger(value)) return value;
        value = Floor(value);
        if (negative)
        {
            value._mantissa *= -1;
            return value;
        }
        return value + new BinaryScientificDecimal(1, 0);
    }

    public static BinaryScientificDecimal operator +(BinaryScientificDecimal value)
        => value;
    public static BinaryScientificDecimal operator -(BinaryScientificDecimal value)
        => new(-value.Mantissa, value.Exponent, value.Precision);
    public static BinaryScientificDecimal operator +(BinaryScientificDecimal left, BinaryScientificDecimal right)
        => Add(left, right);
    public static BinaryScientificDecimal operator -(BinaryScientificDecimal left, BinaryScientificDecimal right)
        => Add(left, -right);
    public static BinaryScientificDecimal operator ++(BinaryScientificDecimal value)
        => Add(value, One);
    public static BinaryScientificDecimal operator --(BinaryScientificDecimal value)
        => Add(value, -One);
    public static BinaryScientificDecimal operator *(BinaryScientificDecimal left, BinaryScientificDecimal right)
        => Multiply(left, right);
    public static BinaryScientificDecimal operator /(BinaryScientificDecimal left, BinaryScientificDecimal right)
        => Divide(left, right);
    public static BinaryScientificDecimal operator %(BinaryScientificDecimal left, BinaryScientificDecimal right)
        => Modulo(left, right);
    
    public static bool operator ==(BinaryScientificDecimal left, BinaryScientificDecimal right)
        => left.Equals(right);
    public static bool operator !=(BinaryScientificDecimal left, BinaryScientificDecimal right) 
        => !left.Equals(right);
    public static bool operator <(BinaryScientificDecimal left, BinaryScientificDecimal right)
    {
        if (left == right) return false;
        if (left._infinite) return left.Negative;
        if (right._infinite) return right.Positive;
        return (right - left).Positive;
    }
    public static bool operator >(BinaryScientificDecimal left, BinaryScientificDecimal right)
    {
        if (left == right) return false;
        if (left._infinite) return left.Positive;
        if (right._infinite) return right.Negative;
        return (left - right).Positive;
    }
    public static bool operator <=(BinaryScientificDecimal left, BinaryScientificDecimal right)
        => left < right || left == right;
    public static bool operator >=(BinaryScientificDecimal left, BinaryScientificDecimal right)
        => left > right || left == right;

    public static implicit operator BinaryScientificDecimal(int value)
        => new(value, 0);
    public static implicit operator BinaryScientificDecimal(uint value)
        => new(value, 0);
    public static implicit operator BinaryScientificDecimal(long value)
        => new(value, 0);
    public static implicit operator BinaryScientificDecimal(ulong value)
        => new(value, 0);
    public static explicit operator BinaryScientificDecimal(decimal value)
        => FromDecimal(value, 0);
    public static explicit operator BinaryScientificDecimal(float value)
        => FromDecimal((decimal)value, 0);
    public static explicit operator BinaryScientificDecimal(double value)
        => FromDecimal((decimal)value, 0);

    public static explicit operator int(BinaryScientificDecimal value)
        => (int)(Floor(value)._mantissa << value._exponent);
    public static explicit operator long(BinaryScientificDecimal value)
        => Floor(value)._mantissa << value._exponent;
    public static explicit operator float(BinaryScientificDecimal value)
        => (float)(value._mantissa * Math.Pow(2, value._exponent));
    public static explicit operator double(BinaryScientificDecimal value)
        => value._mantissa * Math.Pow(2, value._exponent);
    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    public int CompareTo(object? obj)
    {
        if (obj is BinaryScientificDecimal scientificDecimal)
            return CompareTo(scientificDecimal);
        return -1;
    }
    
    public int CompareTo(BinaryScientificDecimal other)
        => this < other ? -1 : this > other ? 1 : 0;

    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        if (string.IsNullOrEmpty(format))
            format = "G";
        
        switch (format.ToUpperInvariant())
        {
            case "B":
                return $"M:{Mantissa:B}, E:{Exponent}, P:{Precision}";
            case "G":
                return $"M:{Mantissa}, E:{Exponent}, P:{Precision}";
            case "N":
                return $"N:{Mantissa * Math.Pow(2, Exponent)}";
            default:
                throw new FormatException();
        }
    }
    
    public bool Equals(BinaryScientificDecimal other)
    {
        if (_infinite && other._infinite) return Positive == other.Positive;
        if (_infinite || other._infinite) return false;
        return _mantissa == other._mantissa &&
               _exponent == other._exponent &&
               _precision == other._precision;
    }
    
    public override bool Equals(object? obj)
    {
        return obj is BinaryScientificDecimal other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_mantissa, _exponent, _precision, _infinite);
    }
    
    public static BinaryScientificDecimal Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out BinaryScientificDecimal result)
    {
        throw new NotImplementedException();
    }

    public static BinaryScientificDecimal Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out BinaryScientificDecimal result)
    {
        throw new NotImplementedException();
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static BinaryScientificDecimal Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static BinaryScientificDecimal Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out BinaryScientificDecimal result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out BinaryScientificDecimal result)
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromChecked<TOther>(TOther value, out BinaryScientificDecimal result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromSaturating<TOther>(TOther value, out BinaryScientificDecimal result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromTruncating<TOther>(TOther value, out BinaryScientificDecimal result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToChecked<TOther>(BinaryScientificDecimal value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToSaturating<TOther>(BinaryScientificDecimal value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToTruncating<TOther>(BinaryScientificDecimal value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool IsZero(BinaryScientificDecimal value)
        => value is { _infinite: false, Mantissa: 0 };
    
    public static bool IsPositive(BinaryScientificDecimal value)
        => IsZero(value) || value.Positive;
    
    public static bool IsNegative(BinaryScientificDecimal value)
        => value.Negative;
    
    public static bool IsFinite(BinaryScientificDecimal value)
        => !value._infinite;
    
    public static bool IsRealNumber(BinaryScientificDecimal value)
        => true;

    public static bool IsImaginaryNumber(BinaryScientificDecimal value)
        => false;
    
    public static bool IsComplexNumber(BinaryScientificDecimal value)
        => false;

    public static bool IsInteger(BinaryScientificDecimal value)
    {
        if (value._mantissa < 0) value._mantissa *= -1;
        if (value._exponent >= 0) return true;
        for (int i = 0; i < -value._exponent; ++i)
        {
            if ((value._mantissa & 1) == 1) return false;
            value._mantissa >>= 1;
        }
        return true;
    }

    public static bool IsOddInteger(BinaryScientificDecimal value)
    {
        if (!IsInteger(value)) return false;
        value._mantissa >>= -value._exponent - 1;
        if ((value._mantissa & 1) == 1) return true;
        return false;
    }

    public static bool IsEvenInteger(BinaryScientificDecimal value)
    {
        if (!IsInteger(value)) return false;
        value._mantissa >>= -value._exponent - 1;
        if ((value._mantissa & 1) == 0) return true;
        return false;
    }

    public static bool IsNaN(BinaryScientificDecimal value)
        => IsInfinity(value);
    
    public static bool IsInfinity(BinaryScientificDecimal value)
        => value._infinite;

    public static bool IsNegativeInfinity(BinaryScientificDecimal value)
        => value.Negative && IsInfinity(value);

    public static bool IsPositiveInfinity(BinaryScientificDecimal value)
        => value.Positive && IsInfinity(value);

    public static bool IsCanonical(BinaryScientificDecimal value)
        => throw new NotImplementedException();
    
    public static bool IsNormal(BinaryScientificDecimal value)
        => throw new NotImplementedException();

    public static bool IsSubnormal(BinaryScientificDecimal value)
        => throw new NotImplementedException();
}