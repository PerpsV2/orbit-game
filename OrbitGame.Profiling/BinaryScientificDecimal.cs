using System.Globalization;

namespace OrbitGame.Profiling;

public class PrecisionException : Exception
{
    public PrecisionException() { }
    public PrecisionException(string message) : base(message) { }
}

/// <summary>
/// Number with decimal precision but arbitrary place value.
/// </summary>
public struct BinaryScientificDecimal
{
    private const int MaxPrecision = 62;

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

    public static BinaryScientificDecimal Zero { get; } = new BinaryScientificDecimal(0, 0, MaxPrecision);
    public static BinaryScientificDecimal One { get; } = new BinaryScientificDecimal(1, 0, MaxPrecision);
    public static BinaryScientificDecimal AdditiveIdentity { get; } = Zero;
    public static BinaryScientificDecimal MultiplicativeIdentity { get; } = One;
    public static BinaryScientificDecimal PositiveInfinity { get; } = new(true, true);
    public static BinaryScientificDecimal NegativeInfinity { get; } = new(true, false);
    
    public static int Radix { get; } = 2;
    

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
        if (divisor._mantissa == 0) return PositiveInfinity * (dividend.Positive ? new(1, 0) : new(-1, 0));
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
        throw new NotImplementedException();
    }

    private static BinaryScientificDecimal Square(BinaryScientificDecimal value)
        => value * value;

    private static BinaryScientificDecimal IntPow(BinaryScientificDecimal value, uint power)
    {
        BinaryScientificDecimal result = One;
        for (uint i = 0; i < power; ++i)
            result *= value;
        return result;
    }

    private static BinaryScientificDecimal Sqrt(BinaryScientificDecimal value)
    {
        throw new NotImplementedException();
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
    
    public static BinaryScientificDecimal MaxMagnitude(BinaryScientificDecimal x, BinaryScientificDecimal y)
    {
        throw new NotImplementedException();
    }

    public static BinaryScientificDecimal MaxMagnitudeNumber(BinaryScientificDecimal x, BinaryScientificDecimal y)
    {
        throw new NotImplementedException();
    }

    public static BinaryScientificDecimal MinMagnitude(BinaryScientificDecimal x, BinaryScientificDecimal y)
    {
        throw new NotImplementedException();
    }

    public static BinaryScientificDecimal MinMagnitudeNumber(BinaryScientificDecimal x, BinaryScientificDecimal y)
    {
        throw new NotImplementedException();
    }

    public bool Equals(BinaryScientificDecimal other)
    {
        throw new NotImplementedException();
    }

    public static BinaryScientificDecimal operator %(BinaryScientificDecimal left, BinaryScientificDecimal right)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        formatProvider ??= CultureInfo.InvariantCulture;
        
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

    
    public static bool operator ==(BinaryScientificDecimal left, BinaryScientificDecimal right)
    {
        throw new NotImplementedException();
    }

    public static bool operator !=(BinaryScientificDecimal left, BinaryScientificDecimal right)
    {
        throw new NotImplementedException();
    }

    public static bool operator >(BinaryScientificDecimal left, BinaryScientificDecimal right)
    {
        throw new NotImplementedException();
    }

    public static bool operator >=(BinaryScientificDecimal left, BinaryScientificDecimal right)
    {
        throw new NotImplementedException();
    }

    public static bool operator <(BinaryScientificDecimal left, BinaryScientificDecimal right)
    {
        throw new NotImplementedException();
    }

    public static bool operator <=(BinaryScientificDecimal left, BinaryScientificDecimal right)
    {
        throw new NotImplementedException();
    }
}