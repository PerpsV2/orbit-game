using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace OrbitGame;

/// <summary>
/// Represents a number in scientific notation with double precision and arbitrary place value.
/// </summary>
public struct SDecimal : IArbitraryPlaceDecimal<SDecimal>
{
    /// <summary>
    /// Number of default sig-figs when printing an SDecimal.
    /// </summary>
    private const int DefaultPrintPrecision = Options.ScientificPrintPrecision;
    /// <summary>
    /// Tolerance to use when comparing equality between two SDecimals.
    /// </summary>
    private const double ComparisonTolerance = Options.ScientificComparisonTolerance;

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
    
    public static int Radix { get; } = 10;
    
    public static SDecimal Zero { get; } = 0;
    public static SDecimal One { get; } = 1;
    public static SDecimal AdditiveIdentity { get; } = 0;
    public static SDecimal MultiplicativeIdentity { get; } = 1;
    public static SDecimal PositiveInfinity { get; } = new(true);
    public static SDecimal NegativeInfinity { get; } = new(false);
    /// <summary>
    /// Represents an SDecimal which is equivalent to double.Epsilon
    /// </summary>
    public static SDecimal DoubleEpsilon { get; } = new(double.Epsilon, 0);

    /// <summary>
    /// Create an SDecimal using a mantissa and a power of ten.
    /// </summary>
    /// <param name="mantissa">Mantissa (does not need to be normalized).</param>
    /// <param name="exponent">Power of ten.</param>
    public SDecimal(double mantissa, int exponent)
    {
        Mantissa = mantissa;
        Exponent = exponent;
        Normalize();
    }

    /// <summary>
    /// Create an SDecimal using a power of ten.
    /// </summary>
    /// <param name="exponent">Power of ten.</param>
    public SDecimal(int exponent)
        : this(1, exponent) {}

    /// <summary>
    /// Create an SDecimal with a value of zero.
    /// </summary>
    public SDecimal()
        : this(0, 0) {}

    /// <summary>
    /// Create an infinite SDecimal.
    /// </summary>
    /// <param name="positive">Sign of the infinite SDecimal</param>
    private SDecimal(bool positive)
        : this(positive ? 1 : -1, 0)
    {
        _infinite = true;
    }

    public static SDecimal FromDouble(double value, int exponent = 0)
    {
        if (double.IsPositiveInfinity(value)) return PositiveInfinity;
        if (double.IsNegativeInfinity(value)) return NegativeInfinity;
        if (double.IsNaN(value)) throw new ArithmeticException("Cannot convert NaN into an SDecimal");
        return new(value, exponent);
    }

    public static double ToDouble(SDecimal value)
    {
        if (IsPositiveInfinity(value)) return double.PositiveInfinity;
        if (IsNegativeInfinity(value)) return double.NegativeInfinity;
        if (Abs(value) < double.Epsilon) return 0;
        if (Abs(value) > double.MaxValue) throw new OverflowException("SDecimal is outside of the range of a double");
        return value.Mantissa * Math.Pow(10, value.Exponent);
    } 

    /// <summary>
    /// Sets the largest non-zero digit of the mantissa to be in the ones place.
    /// </summary>
    /// <returns>The normalized SDecimal.</returns>
    /// <exception cref="ArithmeticException">Attempted to normalize infinite SDecimal</exception>
    private SDecimal Normalize()
    {
        if (_infinite) throw new ArithmeticException("Cannot normalize infinite SDecimal");
        
        if (Mantissa == 0)
        {
            Exponent = 0;
            return this;
        }
        
        while (Math.Abs(Mantissa) >= 10)
        {
            Mantissa /= 10;
            Exponent++;
        }
        
        while (Math.Abs(Mantissa) < 1)
        {
            Mantissa *= 10;
            Exponent--;
        }

        return this;
    }

    /// <summary>
    /// Increase the exponent without modifying the actual value of the number
    /// </summary>
    /// <param name="exponent">Exponent to increase to.</param>
    /// <returns>The value with an increased exponent.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Attempted to increase exponent to a number less than the current exponent.
    /// </exception>
    private SDecimal IncreaseExponent(int exponent)
    {
        if (_infinite) throw new ArithmeticException("Cannot increase exponent of an infinite SDecimal");
        
        int exponentDifference = exponent - Exponent;
        if (exponentDifference < 0) throw new ArgumentOutOfRangeException();
        if (exponentDifference == 0) return this;
        Mantissa /= Math.Pow(10, exponentDifference);
        Exponent += exponentDifference;

        return this;
    }
    
    /// <summary>
    /// Add two numbers together.
    /// </summary>
    /// <param name="left">Left number.</param>
    /// <param name="right">Right number.</param>
    /// <returns>The sum of the left and right numbers.</returns>
    /// <exception cref="ArithmeticException">Attempted to add opposite signed infinite SDecimals.</exception>
    private static SDecimal Add(SDecimal left, SDecimal right)
    {
        if (left._infinite && right._infinite)
        {
            if (left.Positive == right.Positive) return left;
            throw new ArithmeticException("Cannot add opposite signed infinite SDecimals");
        }
        if (left._infinite) return left;
        if (right._infinite) return right;
        if (left.Exponent > right.Exponent)
            return new SDecimal(right.IncreaseExponent(left.Exponent).Mantissa + left.Mantissa, left.Exponent);
        if (right.Exponent > left.Exponent)
            return new SDecimal(left.IncreaseExponent(right.Exponent).Mantissa + right.Mantissa, right.Exponent);
        return new SDecimal(left.Mantissa + right.Mantissa, left.Exponent);
    }

    /// <summary>
    /// Multiply two numbers together.
    /// </summary>
    /// <param name="left">Left number.</param>
    /// <param name="right">Right number.</param>
    /// <returns>The product of the left and right numbers.</returns>
    private static SDecimal Multiply(SDecimal left, SDecimal right)
    {
        if (left == 0 || right == 0) return 0;
        if (left._infinite || right._infinite) 
            return new((left.Positive && right.Positive) || (left.Negative && right.Negative));
        return new SDecimal(left.Mantissa * right.Mantissa, 
            left.Exponent + right.Exponent).Normalize();
    }

    /// <summary>
    /// Divide an SDecimal by another SDecimal
    /// </summary>
    /// <param name="dividend">The dividend.</param>
    /// <param name="divisor">The divisor.</param>
    /// <returns>The dividend divided by the divisor</returns>
    /// <exception cref="ArithmeticException">
    /// Infinite SDecimal was divided by another SDecimal or zero was divided by zero
    /// </exception>
    private static SDecimal Divide(SDecimal dividend, SDecimal divisor)
    {
        if (divisor == 0 && dividend == 0) throw new ArithmeticException("Cannot divide zero by zero");
        if (divisor == 0) return PositiveInfinity * (dividend.Positive ? 1 : -1);
        if (dividend._infinite && divisor._infinite) 
            throw new ArithmeticException("Cannot divide an infinite SDecimal by another infinite SDecimal");
        if (dividend._infinite) return dividend * divisor;
        if (divisor._infinite) return 0;
        return new SDecimal(dividend.Mantissa / divisor.Mantissa, 
            dividend.Exponent - divisor.Exponent).Normalize();
    }

    /// <summary>
    /// Calculates the modulo between two values.
    /// </summary>
    /// <param name="value">Value to mod.</param>
    /// <param name="mod">Modulo amount.</param>
    /// <returns>Returns the remainder of the value divided by the mod.</returns>
    /// <exception cref="ArithmeticException">
    /// Attempted to modulate a value by zero or attempted to modulate an infinite SDecimal
    /// </exception>
    // TODO: Change modulo to calculate remainder instead of modulo as C# usually does.
    private static SDecimal Modulo(SDecimal value, SDecimal mod)
    {
        if (mod == 0) throw new ArithmeticException("Cannot modulate a value by zero");
        if (value._infinite || mod._infinite) throw new ArithmeticException("Cannot modulate an infinite SDecimal");
        return value - mod * Math.Floor((double)(value / mod));
    }
    
    public static SDecimal Square(SDecimal value)
        => value * value;

    public static SDecimal IntPow(SDecimal value, int amount)
    {
        SDecimal result = 1;
        for (uint i = 0; i < amount; ++i)
            result *= value;

        return result;
    }

    public static SDecimal Sqrt(SDecimal value)
    {
        if (value.Negative) throw new ArithmeticException("Cannot take the square root of a negative ScientificDecimal");
        if (value._infinite) return value;
        if (value.Exponent % 2 != 0) value.IncreaseExponent(value.Exponent + 1);
        return new SDecimal(Utils.DecimalSqrt(value.Mantissa), value.Exponent / 2);
    }

    public static double Atan2(SDecimal y, SDecimal x)
    {
        return Math.Atan2((double)y, (double)x);
    }

    public static double Cos(SDecimal value)
    {
        throw new NotImplementedException();
    }

    public static double Sin(SDecimal value)
    {
        throw new NotImplementedException();
    }

    public static double Tan(SDecimal value)
    {
        throw new NotImplementedException();
    }

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
        if (value._infinite) throw new ArithmeticException("Cannot round infinite ScientificDecimal");
        if (value.Mantissa == 0) return value;
        if (value.Exponent < -1) return 0;
        if (value.Exponent == -1) return new(double.Round(value.Mantissa * 0.1), 0);
        return new(double.Round(value.Mantissa, Math.Clamp(value.Exponent, 0, 15)), value.Exponent);
    }

    public static SDecimal Floor(SDecimal value)
    {
        if (value._infinite) throw new ArithmeticException("Cannot round infinite ScientificDecimal");
        if (value.Mantissa == 0) return value;
        SDecimal roundDiff = value - Round(value);
        if (roundDiff < 0) return value - 1 - roundDiff;
        return value - roundDiff;
    }

    public static SDecimal Ceiling(SDecimal value)
    {
        throw new NotImplementedException();
    }

    [Obsolete("MinMagnitude is obsolete, Use Min method instead.")]
    public static SDecimal MinMagnitude(SDecimal x, SDecimal y)
        => Min(x, y);

    public static SDecimal MinMagnitudeNumber(SDecimal x, SDecimal y)
        => IsNaN(x) ? IsNaN(y) ? throw new ArithmeticException() : y : IsNaN(y) ? x : Min(x, y);
    
    [Obsolete("MaxMagnitude is obsolete. Use Max method instead.")]
    public static SDecimal MaxMagnitude(SDecimal x, SDecimal y)
        => Max(x, y);

    public static SDecimal MaxMagnitudeNumber(SDecimal x, SDecimal y)
        => IsNaN(x) ? IsNaN(y) ? throw new ArithmeticException() : y : IsNaN(y) ? x : Max(x, y);

    public static SDecimal Clamp(SDecimal value, SDecimal min, SDecimal max)
    {
        if (max < min) throw new ArgumentException("ScientificDecimal clamp maximum cannot be less than the minimum");
        return value < min ? min : value > max ? max : value;
    }
    
    public TOther Map<TOther>() where TOther : new()
    {
        SDecimal value = this;
        TOther other = new TOther();
        if (other is PDecimal)
        {
            PDecimal pDecimal;
            if (IsPositiveInfinity(value)) pDecimal = PDecimal.PositiveInfinity;
            else if (IsNegativeInfinity(value)) pDecimal = PDecimal.NegativeInfinity;
            else pDecimal = new PDecimal(value.Mantissa, value.Exponent);
            if (pDecimal is TOther result) return result;
        }
        
        else if (other is SDecimal)
        {
            if (value is TOther result) return result;
        }
        
        throw new InvalidCastException();
    }

    public static SDecimal operator +(SDecimal value) 
        => value;

    public static SDecimal operator -(SDecimal value)
    {
        if (value._infinite) return new(value.Negative);
        return new(-value.Mantissa, value.Exponent);
    } 
    public static SDecimal operator +(SDecimal left, SDecimal right) 
        => Add(left, right);
    public static SDecimal operator -(SDecimal left, SDecimal right) 
        => Add(left, -right);
    public static SDecimal operator ++(SDecimal value) 
        => Add(value, 1);
    public static SDecimal operator --(SDecimal value)
        => Add(value, -1);
    public static SDecimal operator*(SDecimal left, SDecimal right)
        => Multiply(left, right);
    public static SDecimal operator/(SDecimal dividend, SDecimal divisor)
        => Divide(dividend, divisor);
    public static SDecimal operator %(SDecimal value, SDecimal mod)
        => Modulo(value, mod);
    public static bool operator ==(SDecimal left, SDecimal right)
        => left.Equals(right);
    public static bool operator !=(SDecimal left, SDecimal right) 
        => !left.Equals(right);
    public static bool operator <(SDecimal left, SDecimal right)
    {
        if (left == right) return false;
        if (left._infinite) return left.Negative;
        if (right._infinite) return right.Positive;
        return (right - left).Positive;
    }

    public static bool operator >(SDecimal left, SDecimal right)
    {
        if (left == right) return false;
        if (left._infinite) return left.Positive;
        if (right._infinite) return right.Negative;
        return (left - right).Positive;
    }

    public static bool operator <=(SDecimal left, SDecimal right)
        => left < right || left == right;
    public static bool operator >=(SDecimal left, SDecimal right)
        => left > right || left == right;

    // to SDecimal
    public static implicit operator SDecimal(int value) 
        => new(value, 0);

    public static implicit operator SDecimal(double value)
        => new(value, 0);
    
    public static implicit operator SDecimal(float value) 
        => new(value, 0);

    // from SDecimal
    public static explicit operator double(SDecimal value)
        => ToDouble(value);
    
    public static explicit operator float(SDecimal value)
        => Convert.ToSingle((double)value);
    
    public static explicit operator int(SDecimal value)
        => (int)(value.Mantissa * Math.Pow(10, value.Exponent));
    
    public static explicit operator uint(SDecimal value)
        => (uint)(value.Mantissa * Math.Pow(10, value.Exponent));
    
    public static explicit operator long (SDecimal value)
        => (long)(value.Mantissa * Math.Pow(10, value.Exponent));

    public static explicit operator PDecimal(SDecimal value)
        => value.Map<PDecimal>();
    
    static bool INumberBase<SDecimal>.IsZero(SDecimal value)
        => value is { _infinite: false, Mantissa: 0 };
    
    [Obsolete("IsPositive method is obsolete. Use Positive property instead.")]
    static bool INumberBase<SDecimal>.IsPositive(SDecimal value)
        => (value == 0) || value.Positive;
    
    [Obsolete("IsNegative method is obsolete. Use Negative property instead.")]
    static bool INumberBase<SDecimal>.IsNegative(SDecimal value)
        => value.Negative;

    static bool INumberBase<SDecimal>.IsFinite(SDecimal value)
        => !value._infinite;
    
    public static bool IsRealNumber(SDecimal value)
        => true;

    public static bool IsImaginaryNumber(SDecimal value)
        => false;
    
    public static bool IsComplexNumber(SDecimal value)
        => false;
    
    public static bool IsInteger(SDecimal value)
        => !value._infinite && double.IsInteger((double)value);

    public static bool IsEvenInteger(SDecimal value)
        => !value._infinite && double.Abs((double)value % 2) <= ComparisonTolerance;
    
    public static bool IsOddInteger(SDecimal value)
        => !value._infinite && double.Abs((double)value % 2 - 1) <= ComparisonTolerance;

    public static bool IsNaN(SDecimal value)
        => IsInfinity(value) || double.IsNaN(value.Mantissa);
    
    public static bool IsInfinity(SDecimal value)
        => value._infinite;

    public static bool IsPositiveInfinity(SDecimal value)
        => value.Positive && IsInfinity(value);
    
    public static bool IsNegativeInfinity(SDecimal value)
        => value.Negative && IsInfinity(value);
    
    public static bool IsCanonical(SDecimal value)
        => value.Mantissa is >= 0 and < 10;
    
    public static bool IsNormal(SDecimal value)
        => double.IsNormal(value.Mantissa);

    public static bool IsSubnormal(SDecimal value)
        => double.IsSubnormal(value.Mantissa);
    
    private string ToStringPrecision(string format)
    {
        if (IsPositiveInfinity(this)) return "PositiveInfinity";
        if (IsNegativeInfinity(this)) return "NegativeInfinity";
        
        int sigFigs;
        try
        {
            sigFigs = int.Parse(format.Substring(1));
        }
        catch (FormatException)
        {
            sigFigs = DefaultPrintPrecision;
        }

        string mantissaString = Mantissa.ToString("F" + (sigFigs - 1));
        return mantissaString + "e" + Exponent.ToString("+0;-#");
    }

    private string ToStringStandardDecimal()
        => ToStringStandardDecimal("S" + DefaultPrintPrecision);

    private string ToStringStandardDecimal(string format)
    {
        if (IsPositiveInfinity(this)) return "PositiveInfinity";
        if (IsNegativeInfinity(this)) return "NegativeInfinity";
        
        int sigFigs;
        try
        {
            sigFigs = int.Parse(format.Substring(1));
        }
        catch (FormatException)
        {
            sigFigs = DefaultPrintPrecision;
        }
        
        string mantissaString = Mantissa.ToString("N" + (sigFigs - 1));
        if (Negative) mantissaString = mantissaString.Substring(1);
        int mantissaDecimalIndex = mantissaString.IndexOf('.');
        int resultDecimalIndex = mantissaDecimalIndex + Exponent;
        string result = mantissaString.Substring(0, mantissaDecimalIndex) + 
                        mantissaString.Substring(mantissaDecimalIndex + 1);
        string resultDecimalInsert = ".";
        
        if (resultDecimalIndex < 0)
        {
            result = result.PadLeft(result.Length + Math.Abs(resultDecimalIndex), '0');
            resultDecimalIndex = 0;
            resultDecimalInsert = "0.";
        }
        
        if (resultDecimalIndex > result.Length)
        {
            result = result.PadRight(resultDecimalIndex, '0');
            resultDecimalInsert = "";
        }
        
        result = result.Insert(resultDecimalIndex, resultDecimalInsert);
        if (Negative) return "-" + result;
        return result;
    }

    private string ToStringGeneral()
        => ToStringPrecision("P" + DefaultPrintPrecision);
    
    public override string ToString()
        => ToStringGeneral();

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format))
            format = "G";

        switch (format.ToUpperInvariant())
        {
            case "G": return ToStringGeneral(); // general format
            case "S": return ToStringStandardDecimal(); // standard decimal format
            case var f when new Regex(@"S\d*").IsMatch(f): 
                return ToStringStandardDecimal(f); // standard decimal format with precision
            case var f when new Regex(@"S\d+").IsMatch(f): 
                return ToStringPrecision(f); // custom precision format
            default: throw new FormatException($"The format '{format}' is not supported.");
        }
    }
    
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static SDecimal Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static SDecimal Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static SDecimal Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static SDecimal Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out SDecimal result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out SDecimal result)
    {
        throw new NotImplementedException();
    }
    
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, 
        out SDecimal result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, 
        out SDecimal result)
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromChecked<TOther>(TOther value, out SDecimal result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromSaturating<TOther>(TOther value, out SDecimal result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromTruncating<TOther>(TOther value, out SDecimal result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToChecked<TOther>(SDecimal value, [MaybeNullWhen(false)] out TOther result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToSaturating<TOther>(SDecimal value, [MaybeNullWhen(false)] out TOther result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToTruncating<TOther>(SDecimal value, [MaybeNullWhen(false)] out TOther result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }
    
    public int CompareTo(object? obj)
    {
        if (obj is SDecimal other)
            return this < other ? -1 : this > other ? 1 : 0;
        return -1;
    }
    public int CompareTo(SDecimal other)
        => this < other ? -1 : this > other ? 1 : 0;
    
    public override bool Equals(object? obj)
    {
        return obj is SDecimal other && Equals(other);
    }

    public bool Equals(SDecimal other)
    {
        if (_infinite && other._infinite) return Positive == other.Positive;
        if (_infinite || other._infinite) return false;
        return Math.Abs(Mantissa - other.Mantissa) < ComparisonTolerance && Exponent == other.Exponent;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Mantissa, Exponent);
    }
}