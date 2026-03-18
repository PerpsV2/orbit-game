using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace OrbitGame.Profiling;

public struct BigIntScientificDecimal : INumber<BigIntScientificDecimal>
{
    private BigInteger _mantissa;
    private BigInteger Mantissa
    {
        get
        {
            if (_infinite) throw new ArithmeticException("Infinite scientific decimal has no mantissa");
            return _mantissa;
        }
        set
        {
            if (_infinite) throw new ArithmeticException("Cannot set mantissa of an infinite scientific decimal");
            _mantissa = value;
        }
    }

    private int _exponent;
    private int Exponent
    {
        get
        {
            if (_infinite) throw new ArithmeticException("Infinite scientific decimal has no mantissa");
            return _exponent;
        }
        set
        {
            if (_infinite) throw new ArithmeticException("Cannot set mantissa of an infinite scientific decimal");
            _exponent = value;
        }
    }
    private readonly bool _infinite;
    private readonly bool Positive => BigInteger.IsPositive(_mantissa);
    private readonly bool Negative => BigInteger.IsNegative(_mantissa);

    public static int Radix { get; } = 10;

    public static BigIntScientificDecimal One { get; } = new(1, 0, false);
    public static BigIntScientificDecimal Zero { get; } = new(0, 0, false);
    public static BigIntScientificDecimal AdditiveIdentity { get; } = new(0, 0, false);
    public static BigIntScientificDecimal MultiplicativeIdentity { get; } = new(1, 0, false);
    private static int MinExponent { get; } = -100;

    private BigIntScientificDecimal(BigInteger mantissa, int exponent, bool infinite)
    {
        _infinite = infinite;
        if (infinite)
        {
            _mantissa = mantissa >= 0 ? 1 : -1;
            _exponent = 0;
            return;
        }
        Mantissa = mantissa;
        Exponent = exponent;
        Normalize();
    }

    public BigIntScientificDecimal(double mantissa, int exponent)
    {
        this = FromFloatingPoint(mantissa, exponent);
    }
    
    private static BigIntScientificDecimal FromFloatingPoint(float value, int exponent)
    {
        if (value == 0) return Zero;
        if (float.IsPositiveInfinity(value)) return new BigIntScientificDecimal(1, 0, true);
        if (float.IsNegativeInfinity(value)) return new BigIntScientificDecimal(-1, 0, true);
        if (float.IsNaN(value)) throw new ArithmeticException("Cannot convert NaN float to scientific decimal");
            
        while (Math.Abs(value) < 1e+7)
        {
            value *= 10;
            exponent--;
        }
        
        return new BigIntScientificDecimal((long)value, exponent, false);
    }
    
    private static BigIntScientificDecimal FromFloatingPoint(double value, int exponent)
    {
        if (value == 0) return Zero;
        if (double.IsPositiveInfinity(value)) return new BigIntScientificDecimal(1, 0, true);
        if (double.IsNegativeInfinity(value)) return new BigIntScientificDecimal(-1, 0, true);
        if (double.IsNaN(value)) throw new ArithmeticException("Cannot convert NaN double to scientific decimal");
        
        while (Math.Abs(value) < 1e+16)
        {
            value *= 10;
            exponent--;
        }

        return new BigIntScientificDecimal((long)value, exponent, false);
    }
    
    private void Normalize()
    {
        if (_infinite) throw new ArithmeticException("Cannot normalize infinite scientific decimal");
        
        if (Mantissa == 0)
        {
            Exponent = 0;
            return;
        }
        if (Exponent < MinExponent) IncreaseExponent(MinExponent);

        while (true) {
            BigInteger quotient = BigInteger.DivRem(Mantissa, 10, out BigInteger remainder);
            if (!remainder.IsZero) break;
            Mantissa = quotient;
            Exponent++;
        }
    }

    private void DecreaseExponent(int exponent)
    {
        if (_infinite) throw new ArithmeticException("Cannot decrease exponent of infinite scientific decimal");
        if (Mantissa == 0) return;
        
        if (exponent >= Exponent) return;
        Mantissa *= BigInteger.Pow(10, Exponent - exponent);
        Exponent = exponent;
    }

    private void IncreaseExponent(int exponent)
    {
        if (_infinite) throw new ArithmeticException("Cannot increase exponent of infinite scientific decimal");
        if (Mantissa == 0) return;
        
        if (exponent <= Exponent) return;
        Mantissa /= BigInteger.Pow(10, exponent - Exponent);
        Exponent = exponent;
    }
    
    private static BigIntScientificDecimal Negate(BigIntScientificDecimal value)
    {
        value.Mantissa *= -1;
        return value;
    }

    private static BigIntScientificDecimal Add(BigIntScientificDecimal left, BigIntScientificDecimal right)
    {
        if (left == 0) return right;
        if (right == 0) return left;
        if (left._infinite && right._infinite)
        {
            if (left.Positive == right.Positive) return left;
            throw new ArithmeticException("Cannot add opposite signed infinite scientific decimals");
        }
        if (left._infinite) return left;
        if (right._infinite) return right;
        
        if (left.Exponent > right.Exponent) left.DecreaseExponent(right.Exponent);
        if (right.Exponent > left.Exponent) right.DecreaseExponent(left.Exponent);
        return new(left.Mantissa + right.Mantissa, left.Exponent, false);
    }

    private static BigIntScientificDecimal Multiply(BigIntScientificDecimal left, BigIntScientificDecimal right)
    {
        if (left == 0 || right == 0) return 0;
        if (left._infinite || right._infinite)
        {
            bool infiniteSign = (left.Positive && right.Positive) || (left.Negative && right.Negative);
            return new(infiniteSign ? 1 : -1, 0, true);
        }
        return new(left.Mantissa * right.Mantissa, left.Exponent + right.Exponent, false);
    }
    
    private static BigIntScientificDecimal Divide(BigIntScientificDecimal dividend, BigIntScientificDecimal divisor)
    {
        if (divisor._mantissa == 0 && dividend._mantissa == 0) throw new ArithmeticException("Cannot divide zero by zero");
        if (divisor._mantissa == 0) return new(dividend.Positive ? 1 : -1, 0, true);
        if (dividend._mantissa == 0) return dividend;
        if (dividend._infinite && divisor._infinite) 
            throw new ArithmeticException("Cannot divide two infinite scientific decimals");
        if (dividend._infinite) return dividend * divisor;
        if (divisor._infinite) return 0;
        
        int precisionPlaces = (int)Math.Ceiling(BigInteger.Log10(BigInteger.Abs(dividend.Mantissa)));
        BigInteger resultMantissa = dividend._mantissa * BigInteger.Pow(10, precisionPlaces) / divisor._mantissa;
        int resultExponent = dividend._exponent - divisor._exponent - precisionPlaces;
        return new(resultMantissa, resultExponent, false);
    }

    private static BigIntScientificDecimal Modulo(BigIntScientificDecimal value, BigIntScientificDecimal mod)
    {
        if (mod == 0) throw new ArithmeticException("Cannot modulate a value by zero");
        if (value._infinite) throw new ArithmeticException("Cannot modulate an infinite scientific decimal");
        if (mod._infinite) return value;
        return value - mod * Floor(value / mod);
    }

    public static BigIntScientificDecimal Floor(BigIntScientificDecimal value)
    {
        if (value._infinite || IsInteger(value)) return value;
        if (value.Negative) return -Ceiling(-value);
        value.IncreaseExponent(0);
        return value;
    }

    public static BigIntScientificDecimal Ceiling(BigIntScientificDecimal value)
    {
        if (value._infinite || IsInteger(value)) return value;
        if (value.Negative) return -Floor(-value);
        return Floor(value) + 1;
    }

    public static BigIntScientificDecimal Round(BigIntScientificDecimal value, MidpointRounding mode = MidpointRounding.ToEven)
    {
        if (value._infinite || IsInteger(value)) return value;
        BigIntScientificDecimal floor = Floor(value);
        BigIntScientificDecimal ceiling = Ceiling(value);
        BigIntScientificDecimal floorDist = Abs(floor - value);
        BigIntScientificDecimal ceilDist = Abs(ceiling - value);
        if (floorDist < ceilDist) return floor;
        if (ceilDist < floorDist) return ceiling;
        switch (mode)
        {
            case MidpointRounding.ToEven: return IsEvenInteger(floor) ? floor : ceiling;
            case MidpointRounding.AwayFromZero: return value.Positive ? ceiling : floor;
            case MidpointRounding.ToZero: return value.Positive ? floor : ceiling;
            case MidpointRounding.ToPositiveInfinity: return ceiling;
            case MidpointRounding.ToNegativeInfinity: return floor; 
            default: throw new ArgumentException("Invalid midpoint rounding mode");
        }
    }
    
    public static BigIntScientificDecimal Abs(BigIntScientificDecimal value)
        => new(BigInteger.Abs(value.Mantissa), value.Exponent, value._infinite);
    
    public static BigIntScientificDecimal Square(BigIntScientificDecimal value)
        => value * value;

    public static BigIntScientificDecimal IntPow(BigIntScientificDecimal value, uint amount)
    {
        BigIntScientificDecimal result = One;
        for (uint i = 0; i < amount; ++i)
            result *= value;
        return result;
    }

    private const int SqrtDecimals = 10;
    
    public static BigIntScientificDecimal Sqrt(BigIntScientificDecimal value)
    {
        if (value.Negative) throw new ArithmeticException("Cannot take the square root of a negative ScientificDecimal");
        if (value._infinite) return value;
        if (value._mantissa == 0) return Zero;
        int digits = (int)Math.Floor(BigInteger.Log10(value._mantissa));
        value.DecreaseExponent(value._exponent + digits - SqrtDecimals);
        if (value._exponent % 2 != 0) value.DecreaseExponent(value._exponent + (value._exponent < 0 ? -1 : 1));

        BigInteger lastGuess;
        BigInteger bestGuess = value._mantissa >> 1;
        do
        {
            lastGuess = bestGuess;
            bestGuess = (lastGuess + value._mantissa / lastGuess) >> 1;
        } while (BigInteger.Abs(bestGuess - lastGuess) > 1);

        return new BigIntScientificDecimal(bestGuess, value._exponent / 2, false);
    }
    
    public static BigIntScientificDecimal Min(BigIntScientificDecimal value, params BigIntScientificDecimal[] values)
    {
        BigIntScientificDecimal result = value;
        foreach (var n in values)
            if (n < result) result = n;
        return result;
    }
    
    public static BigIntScientificDecimal Max(BigIntScientificDecimal value, params BigIntScientificDecimal[] values)
    {
        BigIntScientificDecimal result = value;
        foreach (var n in values)
            if (n > result) result = n;
        return result;
    }

    [Obsolete("MinMagnitude is obsolete, Use Min method instead.")]
    public static BigIntScientificDecimal MinMagnitude(BigIntScientificDecimal x, BigIntScientificDecimal y)
        => Max(x, y);
    
    public static BigIntScientificDecimal MinMagnitudeNumber(BigIntScientificDecimal x, BigIntScientificDecimal y)
        => IsNaN(x) ? IsNaN(y) ? throw new ArithmeticException() : y : IsNaN(y) ? x : Min(x, y);

    public static BigIntScientificDecimal MaxMagnitude(BigIntScientificDecimal x, BigIntScientificDecimal y)
        => Max(x, y);
    
    public static BigIntScientificDecimal MaxMagnitudeNumber(BigIntScientificDecimal x, BigIntScientificDecimal y)
        => IsNaN(x) ? IsNaN(y) ? throw new ArithmeticException() : y : IsNaN(y) ? x : Max(x, y);
    
    public static BigIntScientificDecimal Clamp(
        BigIntScientificDecimal value, 
        BigIntScientificDecimal min, 
        BigIntScientificDecimal max)
    {
        if (max < min) throw new ArithmeticException("ScientificDecimal clamp maximum cannot be less than the minimum");
        return value < min ? min : value > max ? max : value;
    }
    
    public static BigIntScientificDecimal operator +(BigIntScientificDecimal value)
        => value;
    
    public static BigIntScientificDecimal operator -(BigIntScientificDecimal value)
        => Negate(value);
    
    public static BigIntScientificDecimal operator +(BigIntScientificDecimal left, BigIntScientificDecimal right)
        => Add(left, right);
    
    public static BigIntScientificDecimal operator -(BigIntScientificDecimal left, BigIntScientificDecimal right)
        => Add(left, -right);

    public static BigIntScientificDecimal operator ++(BigIntScientificDecimal value)
        => Add(value, One);
    
    public static BigIntScientificDecimal operator --(BigIntScientificDecimal value)
        => Add(value, -One);
    
    public static BigIntScientificDecimal operator *(BigIntScientificDecimal left, BigIntScientificDecimal right)
        => Multiply(left, right);
    
    public static BigIntScientificDecimal operator /(BigIntScientificDecimal left, BigIntScientificDecimal right)
        => Divide(left, right);

    public static BigIntScientificDecimal operator %(BigIntScientificDecimal left, BigIntScientificDecimal right)
        => Modulo(left, right);

    public static bool operator ==(BigIntScientificDecimal left, BigIntScientificDecimal right)
        => Equals(left, right);

    public static bool operator !=(BigIntScientificDecimal left, BigIntScientificDecimal right)
        => !Equals(left, right);

    public static bool operator >(BigIntScientificDecimal left, BigIntScientificDecimal right)
    {
        if (left == right) return false;
        if (left._infinite) return left.Positive;
        if (right._infinite) return right.Negative;
        return (left - right).Positive;
    }

    public static bool operator <(BigIntScientificDecimal left, BigIntScientificDecimal right)
    {
        if (left == right) return false;
        if (left._infinite) return left.Negative;
        if (right._infinite) return right.Positive;
        return (right - left).Positive;
    }

    public static bool operator >=(BigIntScientificDecimal left, BigIntScientificDecimal right)
        => left > right || left == right;

    public static bool operator <=(BigIntScientificDecimal left, BigIntScientificDecimal right)
        => left < right || left == right;
    
    public static implicit operator BigIntScientificDecimal(int value)
        => new(value, 0);
    
    public static implicit operator BigIntScientificDecimal(uint value)
        => new(value, 0);
    
    public static implicit operator BigIntScientificDecimal(long value)
        => new(value, 0);

    public static explicit operator BigIntScientificDecimal(float value)
        => FromFloatingPoint(value, 0);

    public static explicit operator BigIntScientificDecimal(double value)
        => FromFloatingPoint(value, 0);

    public static explicit operator int(BigIntScientificDecimal value)
    {
        BigIntScientificDecimal result = Floor(value);
        result.DecreaseExponent(0);
        return (int)result.Mantissa;
    }
    
    public static explicit operator uint(BigIntScientificDecimal value)
    {
        BigIntScientificDecimal result = Floor(value);
        result.DecreaseExponent(0);
        return (uint)result.Mantissa;
    }
    
    public static explicit operator long(BigIntScientificDecimal value)
    {
        BigIntScientificDecimal result = Floor(value);
        result.DecreaseExponent(0);
        return (long)result.Mantissa;
    }
    
    public static explicit operator float(BigIntScientificDecimal value)
    {
        double result = (double)value.Mantissa;
        return (float)(result * Math.Pow(10, value.Exponent));
    }
    
    public static explicit operator double(BigIntScientificDecimal value)
    {
        double result = (double)value.Mantissa;
        return result * Math.Pow(10, value.Exponent);
    }

    public static bool IsZero(BigIntScientificDecimal value)
        => !value._infinite && value._mantissa == 0;
    
    [Obsolete("IsPositive method is obsolete. Use Positive property instead.")]
    public static bool IsPositive(BigIntScientificDecimal value)
        => value.Positive;
    
    [Obsolete("IsNegative method is obsolete. Use Negative property instead.")]
    public static bool IsNegative(BigIntScientificDecimal value)
        => value.Negative;

    public static bool IsInteger(BigIntScientificDecimal value)
        => value is { _infinite: false, _exponent: >= 0 };
    
    public static bool IsEvenInteger(BigIntScientificDecimal value)
        => IsInteger(value) && value % 2 == Zero;
    
    public static bool IsOddInteger(BigIntScientificDecimal value)
        => IsInteger(value) && value % 2 == One;
    
    public static bool IsRealNumber(BigIntScientificDecimal value)
        => true;

    public static bool IsImaginaryNumber(BigIntScientificDecimal value)
        => false;
    
    public static bool IsComplexNumber(BigIntScientificDecimal value)
        => false;
    
    public static bool IsCanonical(BigIntScientificDecimal value)
    {
        BigIntScientificDecimal normalized = value;
        normalized.Normalize();
        return value == normalized;
    }

    public static bool IsNormal(BigIntScientificDecimal value)
        => !value._infinite && value._mantissa != 0;

    public static bool IsSubnormal(BigIntScientificDecimal value)
        => false;
    
    public static bool IsFinite(BigIntScientificDecimal value)
        => !value._infinite;
    
    public static bool IsInfinity(BigIntScientificDecimal value)
        => value._infinite;
    
    public static bool IsPositiveInfinity(BigIntScientificDecimal value)
        => value is { Positive: true, _infinite: true };
    
    public static bool IsNegativeInfinity(BigIntScientificDecimal value)
        => value is { Negative: true, _infinite: true };
    
    public static bool IsNaN(BigIntScientificDecimal value)
        => value._infinite;

    public override string ToString()
        => ToString("G");
    
    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        if (string.IsNullOrEmpty(format)) format = "G";

        switch (format)
        {
            case "G":
                return Mantissa + "e" + Exponent.ToString("+0;-0");
            case "N":
                if (_infinite && Positive) return "Positive Infinity";
                if (_infinite && Negative) return "Negative Infinity";
                return ((double)this).ToString("N10");
            default:
                throw new FormatException();
        }
    }
    
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static BigIntScientificDecimal Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static BigIntScientificDecimal Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static BigIntScientificDecimal Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static BigIntScientificDecimal Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out BigIntScientificDecimal result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out BigIntScientificDecimal result)
    {
        throw new NotImplementedException();
    }
    
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, 
        out BigIntScientificDecimal result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, 
        out BigIntScientificDecimal result)
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromChecked<TOther>(TOther value, out BigIntScientificDecimal result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromSaturating<TOther>(TOther value, out BigIntScientificDecimal result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromTruncating<TOther>(TOther value, out BigIntScientificDecimal result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToChecked<TOther>(BigIntScientificDecimal value, [MaybeNullWhen(false)] out TOther result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToSaturating<TOther>(BigIntScientificDecimal value, [MaybeNullWhen(false)] out TOther result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToTruncating<TOther>(BigIntScientificDecimal value, [MaybeNullWhen(false)] out TOther result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }
    
    public int CompareTo(object? obj)
    {
        if (obj is BigIntScientificDecimal other)
            return this < other ? -1 : this > other ? 1 : 0;
        return -1;
    }
    
    public int CompareTo(BigIntScientificDecimal other)
        => this < other ? -1 : this > other ? 1 : 0;

    public override bool Equals(object? obj)
    {
        return obj is BigIntScientificDecimal other && Equals(other);
    }
    
    public bool Equals(BigIntScientificDecimal other)
    {
        return Mantissa == other.Mantissa &&
               Exponent == other.Exponent;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_mantissa, _exponent, _infinite);
    }
}