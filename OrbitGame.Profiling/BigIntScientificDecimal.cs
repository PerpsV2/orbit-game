using System.Numerics;

namespace OrbitGame.Profiling;

public struct BigIntScientificDecimal : IEquatable<BigIntScientificDecimal>, IFormattable
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
            
        while (value < 1e+7)
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
        
        while (value < 1e+16)
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
            Mantissa = 0;
            Exponent = 0;
            return;
        }
        if (Exponent < MinExponent) IncreaseExponent(MinExponent);
        
        int trailingZeroCount = (int)BigInteger.TrailingZeroCount(Mantissa);
        Mantissa /= BigInteger.Pow(10, trailingZeroCount);
        Exponent += trailingZeroCount;
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

    private static readonly BigIntScientificDecimal InitialGuessConstant1 = new(282352, -5);
    private static readonly BigIntScientificDecimal InitialGuessConstant2 = new(188235, -5);
    private static readonly double IterationConstant1 = Math.Log10(17);
    private static readonly int DivisionDecimals = 10;
    
    private static BigIntScientificDecimal Divide(BigIntScientificDecimal dividend, BigIntScientificDecimal divisor)
    {
        if (divisor._mantissa == 0 && dividend._mantissa == 0) throw new ArithmeticException("Cannot divide zero by zero");
        if (divisor._mantissa == 0) return new(dividend.Positive ? 1 : -1, 0, true);
        if (dividend._mantissa == 0) return dividend;
        if (dividend._infinite && divisor._infinite) 
            throw new ArithmeticException("Cannot divide two infinite scientific decimals");
        if (dividend._infinite) return dividend * divisor;
        if (divisor._infinite) return 0;
        
        int shiftAmount = divisor.Exponent + (int)Math.Ceiling(BigInteger.Log10(BigInteger.Abs(divisor.Mantissa)));
        int precisionPlaces = dividend.Exponent + (int)Math.Ceiling(BigInteger.Log10(BigInteger.Abs(dividend.Mantissa)));
        dividend.Exponent -= shiftAmount;
        divisor.Exponent -= shiftAmount;
        BigIntScientificDecimal invDivisor = InitialGuessConstant1 - InitialGuessConstant2 * divisor;
        int iterations = (int)Math.Ceiling(Math.Log2((
            (precisionPlaces + DivisionDecimals) * Math.Log2(10) + 1) / IterationConstant1
            )) + 1;
        for (int i = 0; i < iterations; ++i)
        {
            invDivisor += invDivisor * (1 - divisor * invDivisor);
            invDivisor.IncreaseExponent(-DivisionDecimals - precisionPlaces - iterations);
        }

        return dividend * invDivisor;
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
        if (value._infinite) return value;
        if (IsNegative(value)) return -Ceiling(-value);
        value.IncreaseExponent(0);
        return value;
    }

    public static BigIntScientificDecimal Ceiling(BigIntScientificDecimal value)
    {
        if (value._infinite) return value;
        if (IsNegative(value)) return -Floor(-value);
        if (value.Exponent == 0) return value;
        return Floor(value) + 1;
    }

    private static BigIntScientificDecimal Round(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
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

    private static readonly BigIntScientificDecimal OneHalf = new(5, -1);
    private static readonly BigIntScientificDecimal SqrtEpsilon = new(1, -10);
    
    public static BigIntScientificDecimal Sqrt(BigIntScientificDecimal value)
    {
        if (value.Negative) throw new ArithmeticException("Cannot take the square root of a negative ScientificDecimal");
        if (value._infinite) return value;
        
        int precisionPlaces = (int)Math.Ceiling(BigInteger.Log10(value.Mantissa) + DivisionDecimals);
        BigIntScientificDecimal lastGuess;
        BigIntScientificDecimal bestGuess = OneHalf * value;
        do {
            lastGuess = bestGuess;
            bestGuess = OneHalf * (bestGuess + value / bestGuess);
            bestGuess.IncreaseExponent(-precisionPlaces - DivisionDecimals);
        }
        while  (Abs(bestGuess - lastGuess) < SqrtEpsilon);
        return bestGuess;
    }

    public static BigIntScientificDecimal MaxMagnitude(BigIntScientificDecimal x, BigIntScientificDecimal y)
    {
        throw new NotImplementedException();
    }

    public static BigIntScientificDecimal MaxMagnitudeNumber(BigIntScientificDecimal x, BigIntScientificDecimal y)
    {
        throw new NotImplementedException();
    }

    public static BigIntScientificDecimal MinMagnitude(BigIntScientificDecimal x, BigIntScientificDecimal y)
    {
        throw new NotImplementedException();
    }

    public static BigIntScientificDecimal MinMagnitudeNumber(BigIntScientificDecimal x, BigIntScientificDecimal y)
    {
        throw new NotImplementedException();
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
    {
        return Equals(left, right);
    }

    public static bool operator !=(BigIntScientificDecimal left, BigIntScientificDecimal right)
    {
        return !Equals(left, right);
    }

    public static bool operator >(BigIntScientificDecimal left, BigIntScientificDecimal right)
    {
        return (right - left).Positive;
    }

    public static bool operator <(BigIntScientificDecimal left, BigIntScientificDecimal right)
    {
        return (left - right).Positive;
    }

    public static bool operator >=(BigIntScientificDecimal left, BigIntScientificDecimal right)
    {
        return left > right || left == right;
    }

    public static bool operator <=(BigIntScientificDecimal left, BigIntScientificDecimal right)
    {
        return left < right || left == right;
    }
    
    public static implicit operator BigIntScientificDecimal(int value)
        => new(value, 0);
    
    public static implicit operator BigIntScientificDecimal(uint value)
        => new(value, 0);
    
    public static implicit operator BigIntScientificDecimal(long value)
        => new(value, 0);

    public static implicit operator BigIntScientificDecimal(float value)
        => FromFloatingPoint(value, 0);

    public static implicit operator BigIntScientificDecimal(double value)
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

    public int CompareTo(BigIntScientificDecimal other)
        => this < other ? -1 : this > other ? 1 : 0;
    
    public int CompareTo(object? obj)
    {
        if (obj is BigIntScientificDecimal other)
            return this < other ? -1 : this > other ? 1 : 0;
        return -1;
    }

    public bool Equals(BigIntScientificDecimal other)
    {
        return Mantissa == other.Mantissa &&
               Exponent == other.Exponent;
    }

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
                return ((double)this).ToString("N9");
            default:
                throw new FormatException();
        }
    }

    public static bool IsCanonical(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public static bool IsComplexNumber(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public static bool IsEvenInteger(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public static bool IsFinite(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public static bool IsImaginaryNumber(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public static bool IsInfinity(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public static bool IsInteger(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public static bool IsNaN(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public static bool IsNegative(BigIntScientificDecimal value)
        => value.Mantissa < 0;

    public static bool IsNegativeInfinity(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public static bool IsNormal(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public static bool IsOddInteger(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public static bool IsPositive(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public static bool IsPositiveInfinity(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public static bool IsRealNumber(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public static bool IsSubnormal(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public static bool IsZero(BigIntScientificDecimal value)
    {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        return obj is BigIntScientificDecimal other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_mantissa, _exponent, _infinite);
    }
}