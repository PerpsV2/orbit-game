using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace OrbitGame;

/// <summary>
/// Represents a number in scientific notation which has both arbitrary precision and place value.
/// </summary>
public struct PDecimal : IArbitraryPlaceDecimal<PDecimal>
{
    /// <summary>
    /// Minimum exponent for a PDecimal to avoid infinitely precise decimals from occuring due to division.
    /// </summary>
    private const int MinExponent = -50;
    /// <summary>
    /// Number of extra decimals of precision produced by a division operation.
    /// </summary>
    private const int DivisionDecimals = 15;
    /// <summary>
    /// Number of extra decimals of precision produced by a square root operation.
    /// </summary>
    private const int SqrtDecimals = 20;
    
    private BigInteger _mantissa;
    public BigInteger Mantissa
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
    public int Exponent
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
    public readonly bool Positive => BigInteger.IsPositive(_mantissa);
    public readonly bool Negative => BigInteger.IsNegative(_mantissa);

    public static int Radix { get; } = 10;

    public static PDecimal Zero { get; } = new(0, 0, false);
    public static PDecimal One { get; } = new(1, 0, false);
    public static PDecimal AdditiveIdentity { get; } = new(0, 0, false);
    public static PDecimal MultiplicativeIdentity { get; } = new(1, 0, false);
    public static PDecimal PositiveInfinity { get; } = new(1, 0, true);
    public static PDecimal NegativeInfinity { get; } = new(-1, 0, true);

    /// <summary>
    /// Create a PDecimal with an integer mantissa, an exponent, and an infinite flag.
    /// </summary>
    /// <param name="mantissa">Integer mantissa.</param>
    /// <param name="exponent">Power of ten exponent.</param>
    /// <param name="infinite">Whether the number should be infinite or not.</param>
    private PDecimal(BigInteger mantissa, int exponent, bool infinite)
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

    /// <summary>
    /// Create a PDecimal with a double mantissa and a power of ten.
    /// </summary>
    /// <param name="mantissa">Mantissa (does not need to be normalized).</param>
    /// <param name="exponent">Power of ten.</param>
    public PDecimal(double mantissa, int exponent)
    {
        this = FromDouble(mantissa, exponent);
    }
    
    /// <summary>
    /// Create a PDecimal with a power of ten.
    /// </summary>
    /// <param name="exponent">Power of ten.</param>
    public PDecimal(int exponent)
    {
        _infinite = false;
        _mantissa = 1;
        _exponent = exponent;
        Normalize();
    }
    
    public static PDecimal FromDouble(double value, int exponent = 0)
    {
        if (value == 0) return Zero;
        if (double.IsPositiveInfinity(value)) return new PDecimal(1, 0, true);
        if (double.IsNegativeInfinity(value)) return new PDecimal(-1, 0, true);
        if (double.IsNaN(value)) throw new ArgumentException("Cannot convert NaN double to scientific decimal");
        
        while (Math.Abs(value) < 1e+16)
        {
            value *= 10;
            exponent--;
        }

        return new PDecimal((long)value, exponent, false);
    }

    public static double ToDouble(PDecimal value)
    {
        if (IsPositiveInfinity(value)) return double.PositiveInfinity;
        if (IsNegativeInfinity(value)) return double.NegativeInfinity;
        double result = (double)value.Mantissa;
        return result * Math.Pow(10, value.Exponent);
    }

    public static double ToDoubleSafe(PDecimal value)
    {
        return 0;
    }
    
    /// <summary>
    /// Removes any trailing zeroes from the 
    /// </summary>
    /// <exception cref="ArithmeticException">Attempted to normalize infinite PDecimal</exception>
    private void Normalize()
    {
        if (_infinite) throw new ArithmeticException("Cannot normalize infinite PDecimal");
        
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
    
    /// <summary>
    /// Increase the exponent while truncating excess mantissa digits.
    /// </summary>
    /// <param name="exponent">Exponent to increase to.</param>
    /// <exception cref="ArithmeticException">
    /// Attempted to increase the exponent of an infinite scientific decimal.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Attempted to increase exponent to a number less than the current exponent.
    /// </exception>
    private void IncreaseExponent(int exponent)
    {
        if (_infinite) throw new ArithmeticException("Cannot increase exponent of an infinite PDecimal");
        if (Mantissa == 0) return;
        
        if (exponent <= Exponent) throw new ArgumentOutOfRangeException();
        Mantissa /= BigInteger.Pow(10, exponent - Exponent);
        Exponent = exponent;
    }

    /// <summary>
    /// Decrease the exponent while maintaining the value of the number.
    /// </summary>
    /// <param name="exponent">Exponent to decrease to.</param>
    /// <exception cref="ArithmeticException">
    /// Attempted to decrease the exponent of an infinite scientific decimal.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Attempted to decrease exponent to a number greater than the current exponent.
    /// </exception>
    private void DecreaseExponent(int exponent)
    {
        if (_infinite) throw new ArithmeticException("Cannot decrease exponent of infinite scientific decimal");
        if (Mantissa == 0) return;
        
        if (exponent >= Exponent) throw new ArgumentOutOfRangeException();
        Mantissa *= BigInteger.Pow(10, Exponent - exponent);
        Exponent = exponent;
    }
    
    /// <summary>
    /// Negate a number.
    /// </summary>
    /// <param name="value">Value to negate.</param>
    /// <returns>The negated value.</returns>
    private static PDecimal Negate(PDecimal value)
    {
        value._mantissa *= -1;
        return value;
    }

    /// <summary>
    /// Add two numbers together.
    /// </summary>
    /// <param name="left">Left number.</param>
    /// <param name="right">Right number.</param>
    /// <returns>The sum of the left and right numbers.</returns>
    /// <exception cref="ArithmeticException">Attempted to add opposite signed infinite PDecimals.</exception>
    private static PDecimal Add(PDecimal left, PDecimal right)
    {
        if (left._infinite && right._infinite)
        {
            if (left.Positive == right.Positive) return left;
            throw new ArithmeticException("Cannot add opposite signed infinite scientific decimals");
        }
        if (left._infinite) return left;
        if (right._infinite) return right;
        if (left == 0) return right;
        if (right == 0) return left;
        
        if (left.Exponent > right.Exponent) left.DecreaseExponent(right.Exponent);
        if (right.Exponent > left.Exponent) right.DecreaseExponent(left.Exponent);
        return new(left.Mantissa + right.Mantissa, left.Exponent, false);
    }

    /// <summary>
    /// Multiply two numbers together.
    /// </summary>
    /// <param name="left">Left number.</param>
    /// <param name="right">Right number.</param>
    /// <returns>The product of the left and right numbers.</returns>
    private static PDecimal Multiply(PDecimal left, PDecimal right)
    {
        if (left == 0 || right == 0) return 0;
        if (left._infinite || right._infinite)
        {
            bool infiniteSign = (left.Positive && right.Positive) || (left.Negative && right.Negative);
            return new(infiniteSign ? 1 : -1, 0, true);
        }

        return new(left.Mantissa * right.Mantissa, left.Exponent + right.Exponent, false);
    }

    /// <summary>
    /// Divide a PDecimal by another PDecimal.
    /// </summary>
    /// <param name="dividend">The dividend.</param>
    /// <param name="divisor">The divisor.</param>
    /// <param name="divisionDecimals">Number of extra decimal places to compute.</param>
    /// <returns>The dividend divided by the divisor</returns>
    /// <exception cref="ArithmeticException">
    /// Infinite SDecimal was divided by another SDecimal or zero was divided by zero
    /// </exception>
    private static PDecimal Divide(PDecimal dividend, PDecimal divisor, int divisionDecimals)
    {
        if (divisor._mantissa == 0 && dividend._mantissa == 0) 
            throw new ArithmeticException("Cannot divide zero by zero");
        if (divisor._mantissa == 0) return new(dividend.Positive ? 1 : -1, 0, true);
        if (dividend._mantissa == 0) return dividend;
        if (dividend._infinite && divisor._infinite) 
            throw new ArithmeticException("Cannot divide an infinite PDecimal by another infinite PDecimal");
        if (dividend._infinite) return dividend * divisor;
        if (divisor._infinite) return 0;
        
        int precisionPlaces = (int)Math.Floor(BigInteger.Log10(BigInteger.Abs(divisor.Mantissa))) + divisionDecimals;
        BigInteger resultMantissa = dividend._mantissa * BigInteger.Pow(10, precisionPlaces) / divisor._mantissa;
        int resultExponent = dividend._exponent - divisor._exponent - precisionPlaces;
        return new(resultMantissa, resultExponent, false);
    }

    /// <summary>
    /// Calculates the remainder between two values.
    /// </summary>
    /// <param name="dividend">Dividend.</param>
    /// <param name="divisor">Divisor.</param>
    /// <returns>Returns the remainder of the dividend divided by the divisor.</returns>
    /// <exception cref="ArithmeticException">
    /// Attempted to calculate the remainder of a division by zero or a division involving infinity
    /// </exception>
    /// // TODO: Change modulo to calculate remainder instead of modulo as C# usually does.
    private static PDecimal Remainder(PDecimal dividend, PDecimal divisor)
    {
        if (divisor == 0) throw new ArithmeticException("Cannot modulate a value by zero");
        if (dividend._infinite) throw new ArithmeticException("Cannot modulate an infinite PDecimal");
        if (divisor._infinite) throw new ArithmeticException("Cannot modulate by an infinite PDecimal");
        return dividend - divisor * Floor(Divide(dividend, divisor, dividend.Exponent));
    }
    
    public static PDecimal Square(PDecimal value)
        => value * value;

    public static PDecimal IntPow(PDecimal value, int amount)
    {
        PDecimal result = One;
        for (uint i = 0; i < amount; ++i)
            result *= value;
        return result;
    }
    
    public static PDecimal Sqrt(PDecimal value)
    {
        if (value.Negative) throw new ArithmeticException("Cannot take the square root of a negative ScientificDecimal");
        if (value._infinite) return value;
        if (value._mantissa == 0) return Zero;
        int digits = (int)Math.Floor(BigInteger.Log10(value._mantissa));
        value.DecreaseExponent(value._exponent - digits - SqrtDecimals);
        if (value._exponent % 2 != 0) value.DecreaseExponent(value._exponent + (value._exponent < 0 ? -1 : 1));

        BigInteger lastGuess;
        BigInteger bestGuess = value._mantissa >> 1;
        do
        {
            lastGuess = bestGuess;
            bestGuess = (lastGuess + value._mantissa / lastGuess) >> 1;
        } while (BigInteger.Abs(bestGuess - lastGuess) > 1);

        return new PDecimal(bestGuess, value._exponent / 2, false);
    }
    
    public static double Atan2(PDecimal y, PDecimal x)
    {
        return Math.Atan2((double)y, (double)x);
    }

    public static double Cos(PDecimal value)
    {
        throw new NotImplementedException();
    }

    public static double Sin(PDecimal value)
    {
        throw new NotImplementedException();
    }

    public static double Tan(PDecimal value)
    {
        throw new NotImplementedException();
    }
    
    public static PDecimal Abs(PDecimal value)
        => new(BigInteger.Abs(value._mantissa), value._exponent, value._infinite);

    public static PDecimal Min(PDecimal value, params PDecimal[] values)
    {
        PDecimal result = value;
        foreach (var n in values)
            if (n < result) result = n;
        return result;
    }
    
    public static PDecimal Max(PDecimal value, params PDecimal[] values)
    {
        PDecimal result = value;
        foreach (var n in values)
            if (n > result) result = n;
        return result;
    }
    
    public static PDecimal Round(PDecimal value, MidpointRounding mode = MidpointRounding.ToEven)
    {
        if (value._infinite || IsInteger(value)) return value;
        PDecimal floor = Floor(value);
        PDecimal ceiling = Ceiling(value);
        PDecimal floorDist = Abs(floor - value);
        PDecimal ceilDist = Abs(ceiling - value);
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

    public static PDecimal Floor(PDecimal value)
    {
        if (value._infinite || IsInteger(value)) return value;
        if (value.Negative) return -Ceiling(-value);
        value.IncreaseExponent(0);
        return value;
    }

    public static PDecimal Ceiling(PDecimal value)
    {
        if (value._infinite || IsInteger(value)) return value;
        if (value.Negative) return -Floor(-value);
        return Floor(value) + 1;
    }

    [Obsolete("MinMagnitude is obsolete, Use Min method instead.")]
    public static PDecimal MinMagnitude(PDecimal x, PDecimal y)
        => Max(x, y);
    
    public static PDecimal MinMagnitudeNumber(PDecimal x, PDecimal y)
        => IsNaN(x) ? IsNaN(y) ? throw new ArithmeticException() : y : IsNaN(y) ? x : Min(x, y);

    public static PDecimal MaxMagnitude(PDecimal x, PDecimal y)
        => Max(x, y);
    
    public static PDecimal MaxMagnitudeNumber(PDecimal x, PDecimal y)
        => IsNaN(x) ? IsNaN(y) ? throw new ArithmeticException() : y : IsNaN(y) ? x : Max(x, y);
    
    public static PDecimal Clamp(
        PDecimal value, 
        PDecimal min, 
        PDecimal max)
    {
        if (max < min) throw new ArithmeticException("ScientificDecimal clamp maximum cannot be less than the minimum");
        return value < min ? min : value > max ? max : value;
    }
    
    public TOther Map<TOther>() where TOther : new()
    {
        PDecimal value = this;
        TOther other = new TOther();
        if (other is SDecimal)
        {
            SDecimal sDecimal;
            if (IsPositiveInfinity(value)) sDecimal = SDecimal.PositiveInfinity;
            else if (IsNegativeInfinity(value)) sDecimal = SDecimal.NegativeInfinity;
            else
            {
                if (BigInteger.Abs(value.Mantissa) > long.MaxValue)
                    value.IncreaseExponent(value.Exponent +
                                           ((int)Math.Floor(BigInteger.Log10(BigInteger.Abs(value.Mantissa))) - 17)
                    );

                sDecimal = new SDecimal((long)value.Mantissa, value.Exponent);
            }
            if (sDecimal is TOther result) return result;
        }
        
        else if (other is PDecimal)
        {
            if (value is TOther result) return result;
        }
        
        throw new InvalidCastException();
    }
    
    public static PDecimal operator +(PDecimal value)
        => value;
    
    public static PDecimal operator -(PDecimal value)
        => Negate(value);
    
    public static PDecimal operator +(PDecimal left, PDecimal right)
        => Add(left, right);
    
    public static PDecimal operator -(PDecimal left, PDecimal right)
        => Add(left, -right);

    public static PDecimal operator ++(PDecimal value)
        => Add(value, One);
    
    public static PDecimal operator --(PDecimal value)
        => Add(value, -One);
    
    public static PDecimal operator *(PDecimal left, PDecimal right)
        => Multiply(left, right);
    
    public static PDecimal operator /(PDecimal left, PDecimal right)
        => Divide(left, right, DivisionDecimals);

    public static PDecimal operator %(PDecimal left, PDecimal right)
        => Remainder(left, right);

    public static bool operator ==(PDecimal left, PDecimal right)
        => Equals(left, right);

    public static bool operator !=(PDecimal left, PDecimal right)
        => !Equals(left, right);

    public static bool operator >(PDecimal left, PDecimal right)
    {
        if (left == right) return false;
        if (left._infinite) return left.Positive;
        if (right._infinite) return right.Negative;
        return (left - right).Positive;
    }

    public static bool operator <(PDecimal left, PDecimal right)
    {
        if (left == right) return false;
        if (left._infinite) return left.Negative;
        if (right._infinite) return right.Positive;
        return (right - left).Positive;
    }

    public static bool operator >=(PDecimal left, PDecimal right)
        => left > right || left == right;

    public static bool operator <=(PDecimal left, PDecimal right)
        => left < right || left == right;
    
    // to PDecimal
    public static implicit operator PDecimal(int value)
        => new(value, 0);
    
    public static implicit operator PDecimal(uint value)
        => new(value, 0);
    
    public static implicit operator PDecimal(long value)
        => new(value, 0);

    public static implicit operator PDecimal(float value)
        => FromDouble(value, 0);

    public static implicit operator PDecimal(double value)
        => FromDouble(value, 0);

    // from PDecimal
    public static explicit operator int(PDecimal value)
    {
        PDecimal result = value.Positive ? Floor(value) : Ceiling(value);
        result.DecreaseExponent(0);
        return (int)result.Mantissa;
    }
    
    public static explicit operator uint(PDecimal value)
    {
        PDecimal result = Floor(value);
        result.DecreaseExponent(0);
        return (uint)result.Mantissa;
    }
    
    public static explicit operator long(PDecimal value)
    {
        PDecimal result = Floor(value);
        result.DecreaseExponent(0);
        return (long)result.Mantissa;
    }

    public static explicit operator float(PDecimal value)
        => (float)ToDouble(value);
    
    public static explicit operator double(PDecimal value)
        => ToDouble(value);

    public static explicit operator SDecimal(PDecimal value)
        => value.Map<SDecimal>();
    
    public static bool IsZero(PDecimal value)
        => !value._infinite && value._mantissa == 0;
    
    [Obsolete("IsPositive method is obsolete. Use Positive property instead.")]
    public static bool IsPositive(PDecimal value)
        => value.Positive;
    
    [Obsolete("IsNegative method is obsolete. Use Negative property instead.")]
    public static bool IsNegative(PDecimal value)
        => value.Negative;
    
    public static bool IsFinite(PDecimal value)
        => !value._infinite;
    
    public static bool IsRealNumber(PDecimal value)
        => true;

    public static bool IsImaginaryNumber(PDecimal value)
        => false;
    
    public static bool IsComplexNumber(PDecimal value)
        => false;

    public static bool IsInteger(PDecimal value)
        => value is { _infinite: false, _exponent: >= 0 };
    
    public static bool IsEvenInteger(PDecimal value)
        => IsInteger(value) && value % 2 == Zero;
    
    public static bool IsOddInteger(PDecimal value)
        => IsInteger(value) && value % 2 == One;
    
    public static bool IsNaN(PDecimal value)
        => value._infinite;
    
    public static bool IsInfinity(PDecimal value)
        => value._infinite;
    
    public static bool IsPositiveInfinity(PDecimal value)
        => value is { Positive: true, _infinite: true };
    
    public static bool IsNegativeInfinity(PDecimal value)
        => value is { Negative: true, _infinite: true };
    
    public static bool IsCanonical(PDecimal value)
    {
        PDecimal normalized = value;
        normalized.Normalize();
        return value == normalized;
    }

    public static bool IsNormal(PDecimal value)
        => !value._infinite && value._mantissa != 0;

    public static bool IsSubnormal(PDecimal value)
        => false;

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
    
    public static PDecimal Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static PDecimal Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static PDecimal Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static PDecimal Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out PDecimal result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out PDecimal result)
    {
        throw new NotImplementedException();
    }
    
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, 
        out PDecimal result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, 
        out PDecimal result)
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromChecked<TOther>(TOther value, out PDecimal result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromSaturating<TOther>(TOther value, out PDecimal result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromTruncating<TOther>(TOther value, out PDecimal result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToChecked<TOther>(PDecimal value, [MaybeNullWhen(false)] out TOther result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToSaturating<TOther>(PDecimal value, [MaybeNullWhen(false)] out TOther result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToTruncating<TOther>(PDecimal value, [MaybeNullWhen(false)] out TOther result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }
    
    public int CompareTo(object? obj)
    {
        if (obj is PDecimal other)
            return this < other ? -1 : this > other ? 1 : 0;
        return -1;
    }
    
    public int CompareTo(PDecimal other)
        => this < other ? -1 : this > other ? 1 : 0;

    public override bool Equals(object? obj)
    {
        return obj is PDecimal other && Equals(other);
    }
    
    public bool Equals(PDecimal other)
    {
        return (_mantissa == other._mantissa &&
               _exponent == other._exponent) || 
               (_infinite && other._infinite &&
                Positive == other.Positive);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_mantissa, _exponent, _infinite);
    }
}