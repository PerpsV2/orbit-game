using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace OrbitGame;

/// <summary>
/// Number with decimal precision but arbitrary place value.
/// </summary>
public struct ScientificDecimal : INumber<ScientificDecimal>
{
    private const int MaxPrecision = 62;
    private const int SqrtMaxIterations = 16;
    private static readonly ScientificDecimal SqrtEpsilon = new(0b1L, -30, MaxPrecision);

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

    public static ScientificDecimal Zero { get; } = new(0, 0, MaxPrecision);
    public static ScientificDecimal One { get; } = new(1, 0, MaxPrecision);
    public static ScientificDecimal AdditiveIdentity => Zero;
    public static ScientificDecimal MultiplicativeIdentity => One;
    public static ScientificDecimal PosInfinity => new(true, true);
    public static ScientificDecimal NegInfinity => new(true, false);
    public static int Radix => 2;


    public ScientificDecimal(long mantissa, int exponent, int precision)
    {
        Mantissa = mantissa;
        Exponent = exponent;
        Precision = precision;
        Normalize();
    }

    /*public ScientificDecimal(decimal mantissa, int exponent)
    {
        bool negative = mantissa < 0;
        if (negative) mantissa *= -1;
        try
        {
            this = FromDecimal(mantissa * (decimal)Math.Pow(10, exponent));
        }
        catch (OverflowException)
        {
            this = FromDecimal(mantissa) * FromPowerOfTen(exponent);
        }
        if (negative) _mantissa *= -1;
    }*/

    public ScientificDecimal(double mantissa, int exponent)
    {
        bool negative = mantissa < 0;
        if (negative) mantissa *= -1;
        try
        {
            if (exponent < -10) throw new OverflowException();
            this = FromFloatingPoint(mantissa * Math.Pow(10, exponent));
        }
        catch (OverflowException)
        {
            this = FromFloatingPoint(mantissa) * FromPowerOfTen(exponent);
        }
        if (negative) _mantissa *= -1;
    }

    public ScientificDecimal(int exponent)
    {
        try
        {
            if (exponent < -10) throw new OverflowException();
            this = FromFloatingPoint(Math.Pow(10, exponent));
        }
        catch (OverflowException)
        {
            this = FromPowerOfTen(exponent);
        }
    }

    private ScientificDecimal(bool isInfinite, bool isPositive)
    {
        Mantissa = isPositive ? 1 : -1;
        _infinite = isInfinite;
    }
    
    public readonly bool Positive => long.IsPositive(_mantissa);
    public readonly bool Negative => long.IsNegative(_mantissa);
    
    private static ScientificDecimal FromDecimal(decimal mantissa, int precision = MaxPrecision)
    {
        bool negative = mantissa < 0;
        if (negative) mantissa *= -1;
        long integralComponent = (long)decimal.Truncate(mantissa);
        string binMantissaString = integralComponent.ToString("B");
        int integralDigits = binMantissaString.Length;
        decimal fractionalComponent = mantissa - integralComponent;
        int fractionalDigits = precision - integralDigits;
        for (int i = 0; i < fractionalDigits; ++i)
        {
            fractionalComponent *= 2;
            if (fractionalComponent > 1)
            {
                fractionalComponent--;
                binMantissaString += '1';
            }
            else binMantissaString += '0';
        }

        ScientificDecimal result = new ScientificDecimal(long.Parse(binMantissaString, NumberStyles.BinaryNumber), 
            integralDigits - precision, precision);
        return negative ? -result : result;
    }

    private static ScientificDecimal FromFloatingPoint(double mantissa, int precision = MaxPrecision)
    {
        bool negative = mantissa < 0;
        if (negative) mantissa *= -1;
        long integralComponent = (long)double.Truncate(mantissa);
        string binMantissaString = integralComponent.ToString("B");
        int integralDigits = binMantissaString.Length;
        double fractionalComponent = mantissa - integralComponent;
        int fractionalDigits = precision - integralDigits;
        for (int i = 0; i < fractionalDigits; ++i)
        {
            fractionalComponent *= 2;
            if (fractionalComponent > 1)
            {
                fractionalComponent--;
                binMantissaString += '1';
            }
            else binMantissaString += '0';
        }

        ScientificDecimal result = new ScientificDecimal(long.Parse(binMantissaString, NumberStyles.BinaryNumber), 
            integralDigits - precision, precision);
        return negative ? -result : result;
    }

    private static ScientificDecimal FromPowerOfTen(int exponent, int precision = MaxPrecision)
    {
        double log = exponent * Math.Log2(10);
        int logIntegral = (int)Math.Floor(log);
        double logFractional = log - logIntegral;
        long binMantissa = (long)Math.Floor(Math.Pow(2, logFractional + MaxPrecision));
        return new ScientificDecimal(binMantissa, logIntegral - precision, precision);
    }

    private void Normalize()
    {
        if (Mantissa == 0)
        {
            Exponent = int.MinValue;
            return;
        }
        
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
        Mantissa >>= int.Min(goal - Exponent, 63);
        Precision -= goal - Exponent;
        Exponent = goal;
        if (negative) Mantissa = -Mantissa;
    }

    private static ScientificDecimal Negate(ScientificDecimal value)
    {
        if (value._infinite) return new(true, value.Negative);
        return new(-value._mantissa, value._exponent, value._precision);
    }

    private static ScientificDecimal Add(ScientificDecimal left, ScientificDecimal right)
    {
        if (left._mantissa == 0) return right;
        if (right._mantissa == 0) return left;
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
            int.Max(left.Precision, right.Precision));
    }

    private static ScientificDecimal Multiply(ScientificDecimal left, ScientificDecimal right)
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
        return new ScientificDecimal(resultMantissa, resultExponent, resultPrecision);
    }

    private static ScientificDecimal Divide(ScientificDecimal dividend, ScientificDecimal divisor)
    {
        if (divisor._mantissa == 0 && dividend._mantissa == 0)
            throw new ArithmeticException("Cannot divide zero by zero");
        if (divisor._mantissa == 0) return dividend.Positive ? PosInfinity : NegInfinity;
        if (divisor._infinite && dividend._infinite)
            throw new ArithmeticException("Cannot divide an infinite value by an infinite value");
        if (dividend._infinite) return divisor.Positive == dividend.Positive ? PosInfinity : NegInfinity;
        if (divisor._infinite) return Zero;
        
        bool leftNegative = dividend.Mantissa < 0;
        bool rightNegative = divisor.Mantissa < 0;
        if (leftNegative) dividend.Mantissa *= -1;
        if (rightNegative) divisor.Mantissa *= -1;
        int resultPrecision = int.Min(dividend.Precision, divisor.Precision);
        if (dividend.Exponent < divisor.Exponent) dividend.IncreaseExponent(divisor.Exponent);
        else if (divisor.Exponent < dividend.Exponent) divisor.IncreaseExponent(dividend.Exponent);
        long resultMantissa = 0b0L;
        int precisionDifference = dividend.Precision - divisor.Precision;
        if (precisionDifference > 0) divisor.Mantissa <<= precisionDifference;
        else divisor.Mantissa >>= -precisionDifference;
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

    private static ScientificDecimal Modulo(ScientificDecimal value, ScientificDecimal mod)
    {
        if (mod._mantissa == 0) throw new ArithmeticException("Cannot modulate a value by zero");
        if (value._infinite || mod._infinite) 
            throw new ArithmeticException("Cannot modulate an infinite ScientificDecimal");
        return value - mod * Floor(value / mod);
    }

    public static ScientificDecimal Square(ScientificDecimal value)
        => value * value;

    public static ScientificDecimal IntPow(ScientificDecimal value, uint power)
    {
        ScientificDecimal result = One;
        for (uint i = 0; i < power; ++i)
            result *= value;
        return result;
    }

    public static ScientificDecimal Sqrt(ScientificDecimal value)
    {
        if (value.Negative) throw new ArithmeticException("Cannot take the square root of a negative number");
        if (value._infinite) return value;
        if (value._mantissa == 0) return Zero;
        ScientificDecimal bestGuess = value;
        ScientificDecimal nextGuess = bestGuess;
        int iterations = 0;
        do
        {
            bestGuess = nextGuess;
            nextGuess = bestGuess + value / bestGuess;
            nextGuess._exponent--;
            iterations++;
        } while (Abs(bestGuess * bestGuess - value) > SqrtEpsilon && iterations < SqrtMaxIterations);

        return bestGuess;
    }

    public static ScientificDecimal Abs(ScientificDecimal value)
    {
        if (value._infinite) return new(true, true);
        return value.Negative ? -value : value;
    }
    
    public static ScientificDecimal Min(ScientificDecimal value, params ScientificDecimal[] values)
    {
        ScientificDecimal result = value;
        foreach (var n in values)
            if (n < result) result = n;
        return result;
    }
    
    public static ScientificDecimal Max(ScientificDecimal value, params ScientificDecimal[] values)
    {
        ScientificDecimal result = value;
        foreach (var n in values)
            if (n > result) result = n;
        return result;
    }
    
    [Obsolete("MinMagnitude is obsolete, Use Min method instead.")]
    public static ScientificDecimal MinMagnitude(ScientificDecimal x, ScientificDecimal y)
        => Min(x, y);

    public static ScientificDecimal MinMagnitudeNumber(ScientificDecimal x, ScientificDecimal y)
        => IsNaN(x) ? IsNaN(y) ? throw new ArithmeticException() : y : IsNaN(y) ? x : Min(x, y);

    [Obsolete("MaxMagnitude is obsolete. Use Max method instead.")]
    public static ScientificDecimal MaxMagnitude(ScientificDecimal x, ScientificDecimal y)
        => Max(x, y);

    public static ScientificDecimal MaxMagnitudeNumber(ScientificDecimal x, ScientificDecimal y)
        => IsNaN(x) ? IsNaN(y) ? throw new ArithmeticException() : y : IsNaN(y) ? x : Max(x, y);

    public static ScientificDecimal Clamp(
        ScientificDecimal value, 
        ScientificDecimal min, 
        ScientificDecimal max)
    {
        if (max < min) throw new ArithmeticException("Clamp maximum cannot be less than the minimum");
        return value < min ? min : value > max ? max : value;
    }

    public static ScientificDecimal Round(ScientificDecimal value)
    {
        bool negative = value._mantissa < 0;
        if (negative) value._mantissa *= -1;
        if (IsInteger(value)) return value;
        return negative ? ((value._mantissa >> -value._exponent - 1) & 0b1L) == 0 ? -Floor(value) : -Ceil(value) :
            ((value._mantissa >> -value._exponent - 1) & 0b1L) == 1 ? Ceil(value) : Floor(value);
    }

    public static ScientificDecimal Floor(ScientificDecimal value)
    {
        bool negative = value._mantissa < 0;
        if (negative) value._mantissa *= -1;
        if (IsInteger(value)) return value;
        value._mantissa >>= -value._exponent;
        value._mantissa <<= -value._exponent;
        if (negative)
        {
            value += One;
            value._mantissa *= -1;
        }
        return value;
    }
    
    public static ScientificDecimal FloorTowardsZero(ScientificDecimal value)
    {
        bool negative = value._mantissa < 0;
        if (negative) value._mantissa *= -1;
        if (IsInteger(value)) return value;
        value._mantissa >>= -value._exponent;
        value._mantissa <<= -value._exponent;
        if (negative) value._mantissa *= -1;
        return value;
    }

    public static ScientificDecimal Ceil(ScientificDecimal value)
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
        return value + One;
    }

    public static ScientificDecimal operator +(ScientificDecimal value)
        => value;
    public static ScientificDecimal operator -(ScientificDecimal value)
        => Negate(value);
    public static ScientificDecimal operator +(ScientificDecimal left, ScientificDecimal right)
        => Add(left, right);
    public static ScientificDecimal operator -(ScientificDecimal left, ScientificDecimal right)
        => Add(left, -right);
    public static ScientificDecimal operator ++(ScientificDecimal value)
        => Add(value, One);
    public static ScientificDecimal operator --(ScientificDecimal value)
        => Add(value, -One);
    public static ScientificDecimal operator *(ScientificDecimal left, ScientificDecimal right)
        => Multiply(left, right);
    public static ScientificDecimal operator /(ScientificDecimal left, ScientificDecimal right)
        => Divide(left, right);
    public static ScientificDecimal operator %(ScientificDecimal left, ScientificDecimal right)
        => Modulo(left, right);
    
    public static bool operator ==(ScientificDecimal left, ScientificDecimal right)
        => left.Equals(right);
    public static bool operator !=(ScientificDecimal left, ScientificDecimal right) 
        => !left.Equals(right);
    public static bool operator <(ScientificDecimal left, ScientificDecimal right)
    {
        if (left == right) return false;
        if (left._infinite) return left.Negative;
        if (right._infinite) return right.Positive;
        return (right - left).Positive;
    }
    public static bool operator >(ScientificDecimal left, ScientificDecimal right)
    {
        if (left == right) return false;
        if (left._infinite) return left.Positive;
        if (right._infinite) return right.Negative;
        return (left - right).Positive;
    }
    public static bool operator <=(ScientificDecimal left, ScientificDecimal right)
        => left < right || left == right;
    public static bool operator >=(ScientificDecimal left, ScientificDecimal right)
        => left > right || left == right;

    public static implicit operator ScientificDecimal(int value)
        => new(value, 0, MaxPrecision);
    public static implicit operator ScientificDecimal(uint value)
        => new(value, 0, MaxPrecision);
    public static implicit operator ScientificDecimal(long value)
        => new(value, 0, MaxPrecision);
    public static implicit operator ScientificDecimal(ulong value)
        => new((long)value, 0, MaxPrecision);
    public static implicit operator ScientificDecimal(decimal value)
        => FromDecimal(value);
    public static implicit operator ScientificDecimal(float value)
        => FromFloatingPoint(value);
    public static implicit operator ScientificDecimal(double value)
        => FromFloatingPoint(value);

    public static explicit operator int(ScientificDecimal value)
    {
        if (value._exponent < 0) return (int)(FloorTowardsZero(value)._mantissa >> -value._exponent);
        return (int)(FloorTowardsZero(value)._mantissa << value._exponent);
    }
    public static explicit operator long(ScientificDecimal value)
    {
        if (value._exponent < 0) return FloorTowardsZero(value)._mantissa >> -value._exponent;
        return FloorTowardsZero(value)._mantissa << value._exponent;
    }
    public static explicit operator float(ScientificDecimal value)
        => (float)(value._mantissa * Math.Pow(2, value._exponent));
    public static explicit operator double(ScientificDecimal value)
        => value._mantissa * Math.Pow(2, value._exponent);
    public override string ToString()
    {
        return ToString("N", CultureInfo.CurrentCulture);
    }
    
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
                return $"{Mantissa * Math.Pow(2, Exponent)}";
            default:
                throw new FormatException();
        }
    }

    public int CompareTo(object? obj)
    {
        if (obj is ScientificDecimal scientificDecimal)
            return CompareTo(scientificDecimal);
        return -1;
    }
    
    public int CompareTo(ScientificDecimal other)
        => this < other ? -1 : this > other ? 1 : 0;
    
    public bool Equals(ScientificDecimal other)
    {
        if (_infinite && other._infinite) return Positive == other.Positive;
        if (_infinite || other._infinite) return false;
        return _mantissa == other._mantissa &&
               _exponent == other._exponent &&
               _precision == other._precision;
    }
    
    public override bool Equals(object? obj)
    {
        return obj is ScientificDecimal other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_mantissa, _exponent, _precision, _infinite);
    }
    
    public static ScientificDecimal Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out ScientificDecimal result)
    {
        throw new NotImplementedException();
    }

    public static ScientificDecimal Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ScientificDecimal result)
    {
        throw new NotImplementedException();
    }

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static ScientificDecimal Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static ScientificDecimal Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, out ScientificDecimal result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, out ScientificDecimal result)
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromChecked<TOther>(TOther value, out ScientificDecimal result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromSaturating<TOther>(TOther value, out ScientificDecimal result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromTruncating<TOther>(TOther value, out ScientificDecimal result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToChecked<TOther>(ScientificDecimal value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToSaturating<TOther>(ScientificDecimal value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToTruncating<TOther>(ScientificDecimal value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool IsZero(ScientificDecimal value)
        => value is { _infinite: false, Mantissa: 0 };
    
    public static bool IsPositive(ScientificDecimal value)
        => IsZero(value) || value.Positive;
    
    public static bool IsNegative(ScientificDecimal value)
        => value.Negative;
    
    public static bool IsFinite(ScientificDecimal value)
        => !value._infinite;
    
    public static bool IsRealNumber(ScientificDecimal value)
        => true;

    public static bool IsImaginaryNumber(ScientificDecimal value)
        => false;
    
    public static bool IsComplexNumber(ScientificDecimal value)
        => false;

    public static bool IsInteger(ScientificDecimal value)
    {
        if (IsInfinity(value)) return false;
        if (value._mantissa < 0) value._mantissa *= -1;
        if (value._exponent >= 0) return true;
        for (int i = 0; i < -value._exponent; ++i)
        {
            if ((value._mantissa & 1) == 1) return false;
            value._mantissa >>= 1;
        }
        return true;
    }

    public static bool IsOddInteger(ScientificDecimal value)
    {
        if (!IsInteger(value)) return false;
        value._mantissa >>= -value._exponent;
        if ((value._mantissa & 1) == 1) return true;
        return false;
    }

    public static bool IsEvenInteger(ScientificDecimal value)
    {
        if (!IsInteger(value)) return false;
        value._mantissa >>= -value._exponent;
        if ((value._mantissa & 1) == 0) return true;
        return false;
    }

    public static bool IsNaN(ScientificDecimal value)
        => IsInfinity(value);
    
    public static bool IsInfinity(ScientificDecimal value)
        => value._infinite;

    public static bool IsNegativeInfinity(ScientificDecimal value)
        => value.Negative && IsInfinity(value);

    public static bool IsPositiveInfinity(ScientificDecimal value)
        => value.Positive && IsInfinity(value);

    public static bool IsCanonical(ScientificDecimal value)
        => throw new NotImplementedException();
    
    public static bool IsNormal(ScientificDecimal value)
        => throw new NotImplementedException();

    public static bool IsSubnormal(ScientificDecimal value)
        => throw new NotImplementedException();
}