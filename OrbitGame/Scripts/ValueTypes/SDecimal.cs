using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
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
        if (double.IsNaN(value)) throw new ArgumentException("Cannot convert NaN into an SDecimal");
        return new(value, exponent);
    }

    public static double ConvertToDouble(SDecimal value)
    {
        if (IsPositiveInfinity(value)) return double.PositiveInfinity;
        if (IsNegativeInfinity(value)) return double.NegativeInfinity;
        if (Abs(value) < double.Epsilon) return 0;
        if (Abs(value) > double.MaxValue) throw new OverflowException("SDecimal is outside of the range of a double");
        return value.Mantissa * Math.Pow(10, value.Exponent);
    }

    public static double ConvertToDoubleSaturating(SDecimal value)
    {
        if (IsPositiveInfinity(value)) return double.PositiveInfinity;
        if (IsNegativeInfinity(value)) return double.NegativeInfinity;
        if (Abs(value) < double.Epsilon) return 0;
        if (value > double.MaxValue) return double.MaxValue;
        if (value < double.MinValue) return double.MinValue;
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
    /// <exception cref="DivideByZeroException">
    /// Attempted to divide zero by zero
    /// </exception>
    /// <exception cref="ArithmeticException">
    /// Infinite SDecimal was divided by another SDecimal
    /// </exception>
    private static SDecimal Divide(SDecimal dividend, SDecimal divisor)
    {
        if (divisor == 0 && dividend == 0) throw new DivideByZeroException("Cannot divide zero by zero");
        if (divisor == 0) return PositiveInfinity * (dividend.Positive ? 1 : -1);
        if (dividend._infinite && divisor._infinite) 
            throw new ArithmeticException("Cannot divide an infinite SDecimal by another infinite SDecimal");
        if (dividend._infinite) return dividend * divisor;
        if (divisor._infinite) return 0;
        return new SDecimal(dividend.Mantissa / divisor.Mantissa, 
            dividend.Exponent - divisor.Exponent).Normalize();
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
        return new SDecimal(Utils.DecimalSqrt(value.Mantissa), value.Exponent / 2);
    }
    
    public static double Atan2(SDecimal y, SDecimal x)
    {
        double quotient = ConvertToDoubleSaturating(y / x);
        if (x > 0) return Math.Atan(quotient);
        if (x < 0 && y >= 0) return Math.Atan(quotient) + Math.PI;
        if (x < 0 && y < 0) return Math.Atan(quotient) - Math.PI;
        if (x == 0 & y > 0) return Math.PI / 2;
        if (x == 0 & y < 0) return -Math.PI / 2;
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
        if (roundDiff < 0) return value - 1 - roundDiff;
        return value - roundDiff;
    }

    public static SDecimal Ceiling(SDecimal value)
    {
        if (value._infinite) throw new ArithmeticException("Cannot round infinite SDecimal");
        if (value.Mantissa == 0) return value;
        SDecimal roundDiff = value - Round(value);
        if (roundDiff > 0) return value + 1 - roundDiff;
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
        => Remainder(value, mod);
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
        => FromDouble(value);
    
    public static implicit operator SDecimal(float value) 
        => FromDouble(value);

    // from SDecimal
    public static explicit operator double(SDecimal value)
        => ConvertToDouble(value);
    
    public static explicit operator float(SDecimal value)
        => Convert.ToSingle((double)value);
    
    public static explicit operator int(SDecimal value)
        => (int)ConvertToDouble(value);
    
    public static explicit operator uint(SDecimal value)
        => (uint)ConvertToDouble(value);
    
    public static explicit operator long (SDecimal value)
        => (long)ConvertToDouble(value);

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
    
    static bool INumberBase<SDecimal>.IsRealNumber(SDecimal value)
        => !value._infinite;

    static bool INumberBase<SDecimal>.IsImaginaryNumber(SDecimal value)
        => false;
    
    static bool INumberBase<SDecimal>.IsComplexNumber(SDecimal value)
        => false;
    
    public static bool IsInteger(SDecimal value)
        => !value._infinite && double.IsInteger(ConvertToDoubleSaturating(value));

    public static bool IsEvenInteger(SDecimal value)
        => !value._infinite && double.Abs(ConvertToDoubleSaturating(value) % 2) <= ComparisonTolerance;
    
    public static bool IsOddInteger(SDecimal value)
        => !value._infinite && double.Abs(ConvertToDoubleSaturating(value) % 2 - 1) <= ComparisonTolerance;

    static bool INumberBase<SDecimal>.IsNaN(SDecimal value)
        => value._infinite || double.IsNaN(value.Mantissa);
    
    public static bool IsInfinity(SDecimal value)
        => value._infinite;

    public static bool IsPositiveInfinity(SDecimal value)
        => value is { Positive: true, _infinite: true };
    
    public static bool IsNegativeInfinity(SDecimal value)
        => value is { Negative: true, _infinite: true };
    
    static bool INumberBase<SDecimal>.IsCanonical(SDecimal value)
        => value.Mantissa is >= 0 and < 10;
    
    static bool INumberBase<SDecimal>.IsNormal(SDecimal value)
        => double.IsNormal(value.Mantissa);

    static bool INumberBase<SDecimal>.IsSubnormal(SDecimal value)
        => double.IsSubnormal(value.Mantissa);
    
    private string ToStringGeneral(string format)
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

    private string ToStringNumber(string format)
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
        // strip the negative sign and re-add at the end
        if (Negative) mantissaString = mantissaString.Substring(1);
        int mantissaDecimalIndex = mantissaString.IndexOf('.');
        if (mantissaDecimalIndex < 0)
        {
            mantissaString += '.';
            mantissaDecimalIndex = mantissaString.IndexOf('.');
        }
        int resultDecimalIndex = mantissaDecimalIndex + Exponent;
        string result = mantissaString.Substring(0, mantissaDecimalIndex) + 
                        mantissaString.Substring(mantissaDecimalIndex + 1);
        string resultDecimalInsert = ".";
        
        if (resultDecimalIndex <= 0)
        {
            result = result.PadLeft(result.Length + Math.Abs(resultDecimalIndex), '0');
            resultDecimalIndex = 0;
            resultDecimalInsert = "0.";
        }
        
        if (resultDecimalIndex >= result.Length)
        {
            result = result.PadRight(resultDecimalIndex, '0');
            resultDecimalInsert = "";
        }
        
        result = result.Insert(resultDecimalIndex, resultDecimalInsert);
        if (Negative) return "-" + result;
        return result;
    }

    private string ToStringGeneral()
        => ToStringGeneral("G" + DefaultPrintPrecision);
    
    private string ToStringNumber()
        => ToStringNumber("N" + DefaultPrintPrecision);
    
    public override string ToString()
        => ToStringGeneral();

    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        if (string.IsNullOrEmpty(format))
            format = "G";

        switch (format.ToUpperInvariant())
        {
            case "G": return ToStringGeneral(); // general format
            case var f when new Regex(@"G[1-9]\d*").IsMatch(f): 
                return ToStringGeneral(f); // custom precision format
            case "N": return ToStringNumber(); // standard decimal format
            case var f when new Regex(@"N[1-9]\d*").IsMatch(f): 
                return ToStringNumber(f); // standard decimal format with precision
            default: throw new FormatException($"The format '{format}' is not supported.");
        }
    }
    
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, 
        IFormatProvider? provider = null)
    {
        throw new NotImplementedException();
    }
    
    public static SDecimal Parse(string s, IFormatProvider? provider = null)
    {
        throw new NotImplementedException();
    }
    
    public static SDecimal Parse(string s, NumberStyles style, IFormatProvider? provider = null)
        => Parse(s, provider);

    public static SDecimal Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
        => Parse(s.ToString(), provider);
    
    public static SDecimal Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider = null)
        => Parse(s.ToString(), provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out SDecimal result)
    {
        throw new NotImplementedException();
    }
    
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, 
        out SDecimal result)
        => TryParse(s, provider, out result);

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out SDecimal result)
        => TryParse(s.ToString(), provider, out result);
    
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, 
        out SDecimal result)
        => TryParse(s.ToString(), provider, out result);

    static bool INumberBase<SDecimal>.TryConvertFromChecked<TOther>(TOther value, out SDecimal result)
    {
        result = new();
        return false;
    }

    static bool INumberBase<SDecimal>.TryConvertFromSaturating<TOther>(TOther value, out SDecimal result) 
    {
        result = new();
        return false;
    }

    static bool INumberBase<SDecimal>.TryConvertFromTruncating<TOther>(TOther value, out SDecimal result)
    {
        result = new();
        return false;
    }

    static bool INumberBase<SDecimal>.TryConvertToChecked<TOther>(SDecimal value, [MaybeNullWhen(false)] out TOther result)
    {
        result = default;
        return false;
    }

    static bool INumberBase<SDecimal>.TryConvertToSaturating<TOther>(SDecimal value, [MaybeNullWhen(false)] out TOther result) 
    {
        result = default;
        return false;
    }

    static bool INumberBase<SDecimal>.TryConvertToTruncating<TOther>(SDecimal value, [MaybeNullWhen(false)] out TOther result) 
    {
        result = default;
        return false;
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
        return HashCode.Combine(Mantissa, Exponent, _infinite);
    }
}