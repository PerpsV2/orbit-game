using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace qQEngine;

public struct ScientificDecimal : INumber<ScientificDecimal>
{
    /// <summary>
    /// Number of default sig-figs when printing an ScientificDecimals.
    /// </summary>
    private const int DefaultPrintPrecision = 5;
    /// <summary>
    /// Tolerance to use when comparing equality between two ScientificDecimals.
    /// </summary>
    private const double ComparisonTolerance = 0.00001;

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
    
    public static ScientificDecimal Zero { get; } = 0;
    public static ScientificDecimal One { get; } = 1;
    public static ScientificDecimal AdditiveIdentity { get; } = 0;
    public static ScientificDecimal MultiplicativeIdentity { get; } = 1;
    public static ScientificDecimal PositiveInfinity { get; } = new(true);
    public static ScientificDecimal NegativeInfinity { get; } = new(false);
    
    /// <summary>
    /// Represents an ScientificDecimal which is equivalent to double.Epsilon
    /// </summary>
    public static ScientificDecimal DoubleEpsilon { get; } = new(double.Epsilon, 0);
    /// <summary>
    /// Represents an ScientificDecimal which is equivalent to double.MaxValue
    /// </summary>
    public static ScientificDecimal DoubleMaxValue { get; } = new(double.MaxValue, 0);
    /// <summary>
    /// Represents an ScientificDecimal which is equivalent to double.MinValue
    /// </summary>
    public static ScientificDecimal DoubleMinValue { get; } = new(double.MinValue, 0);

    /// <summary>
    /// Create an ScientificDecimal using a mantissa and a power of ten.
    /// </summary>
    /// <param name="mantissa">Mantissa (does not need to be normalized).</param>
    /// <param name="exponent">Power of ten.</param>
    public ScientificDecimal(double mantissa, int exponent)
    {
        Mantissa = mantissa;
        Exponent = exponent;
        Normalize();
    }

    /// <summary>
    /// Create an ScientificDecimal using a power of ten.
    /// </summary>
    /// <param name="exponent">Power of ten.</param>
    public ScientificDecimal(int exponent)
        : this(1, exponent) {}

    /// <summary>
    /// Create an ScientificDecimal with a value of zero.
    /// </summary>
    public ScientificDecimal()
        : this(0, 0) {}

    /// <summary>
    /// Create an infinite ScientificDecimal.
    /// </summary>
    /// <param name="positive">Sign of the infinite ScientificDecimal</param>
    private ScientificDecimal(bool positive)
        : this(positive ? 1 : -1, 0)
    {
        _infinite = true;
    }

    public static ScientificDecimal FromDouble(double value, int exponent = 0)
    {
        if (double.IsPositiveInfinity(value)) return PositiveInfinity;
        if (double.IsNegativeInfinity(value)) return NegativeInfinity;
        if (double.IsNaN(value)) throw new ArgumentException("Cannot convert NaN into an ScientificDecimal");
        return new(value, exponent);
    }

    public static double ConvertToDouble(ScientificDecimal value)
    {
        if (IsPositiveInfinity(value)) return double.PositiveInfinity;
        if (IsNegativeInfinity(value)) return double.NegativeInfinity;
        //if (Abs(value) < DoubleEpsilon) return 0;
        //if (Abs(value) > DoubleMaxValue) throw new OverflowException("ScientificDecimal is outside of the range of a double");
        return value.Mantissa * Math.Pow(10, value.Exponent);
    }

    public static double ConvertToDoubleSaturating(ScientificDecimal value)
    {
        if (IsPositiveInfinity(value)) return double.PositiveInfinity;
        if (IsNegativeInfinity(value)) return double.NegativeInfinity;
        if (Abs(value) < DoubleEpsilon) return 0;
        if (value > DoubleMaxValue) return double.MaxValue;
        if (value < DoubleMinValue) return double.MinValue;
        return value.Mantissa * Math.Pow(10, value.Exponent);
    }

    /// <summary>
    /// Sets the largest non-zero digit of the mantissa to be in the ones place.
    /// </summary>
    /// <returns>The normalized ScientificDecimal.</returns>
    /// <exception cref="ArithmeticException">Attempted to normalize infinite ScientificDecimal</exception>
    private ScientificDecimal Normalize()
    {
        if (_infinite) throw new ArithmeticException("Cannot normalize infinite ScientificDecimal");
        
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
    private ScientificDecimal IncreaseExponent(int exponent)
    {
        if (_infinite) throw new ArithmeticException("Cannot increase exponent of an infinite ScientificDecimal");
        
        int exponentDifference = exponent - Exponent;
        if (exponentDifference < 0) throw new ArgumentOutOfRangeException();
        if (exponentDifference == 0) return this;

        while (Exponent != exponent)
        {
            Mantissa /= 10;
            if (Mantissa == 0)
            {
                Exponent = exponent;
                break;
            }
            Exponent++;
        }

        return this;
    }
    
    /// <summary>
    /// Add two numbers together.
    /// </summary>
    /// <param name="left">Left number.</param>
    /// <param name="right">Right number.</param>
    /// <returns>The sum of the left and right numbers.</returns>
    /// <exception cref="ArithmeticException">Attempted to add opposite signed infinite ScientificDecimals.</exception>
    private static ScientificDecimal Add(ScientificDecimal left, ScientificDecimal right)
    {
        if (left._infinite && right._infinite)
        {
            if (left.Positive == right.Positive) return left;
            throw new ArithmeticException("Cannot add opposite signed infinite ScientificDecimals");
        }
        if (left._infinite) return left;
        if (right._infinite) return right;
        if (left.Exponent > right.Exponent)
            return new ScientificDecimal(right.IncreaseExponent(left.Exponent).Mantissa + left.Mantissa, left.Exponent);
        if (right.Exponent > left.Exponent)
            return new ScientificDecimal(left.IncreaseExponent(right.Exponent).Mantissa + right.Mantissa, right.Exponent);
        return new ScientificDecimal(left.Mantissa + right.Mantissa, left.Exponent);
    }

    /// <summary>
    /// Multiply two numbers together.
    /// </summary>
    /// <param name="left">Left number.</param>
    /// <param name="right">Right number.</param>
    /// <returns>The product of the left and right numbers.</returns>
    private static ScientificDecimal Multiply(ScientificDecimal left, ScientificDecimal right)
    {
        if (left == 0 || right == 0) return 0;
        if (left._infinite || right._infinite) 
            return new((left.Positive && right.Positive) || (left.Negative && right.Negative));
        return new ScientificDecimal(left.Mantissa * right.Mantissa, 
            left.Exponent + right.Exponent).Normalize();
    }

    /// <summary>
    /// Divide an ScientificDecimal by another ScientificDecimal
    /// </summary>
    /// <param name="dividend">The dividend.</param>
    /// <param name="divisor">The divisor.</param>
    /// <returns>The dividend divided by the divisor</returns>
    /// <exception cref="DivideByZeroException">
    /// Attempted to divide zero by zero
    /// </exception>
    /// <exception cref="ArithmeticException">
    /// Infinite ScientificDecimal was divided by another ScientificDecimal
    /// </exception>
    private static ScientificDecimal Divide(ScientificDecimal dividend, ScientificDecimal divisor)
    {
        if (divisor == 0 && dividend == 0) throw new DivideByZeroException("Cannot divide zero by zero");
        if (divisor == 0) return PositiveInfinity * (dividend.Positive ? 1 : -1);
        if (dividend._infinite && divisor._infinite) 
            throw new ArithmeticException("Cannot divide an infinite ScientificDecimal by another infinite ScientificDecimal");
        if (dividend._infinite) return dividend * divisor;
        if (divisor._infinite) return 0;
        return new ScientificDecimal(dividend.Mantissa / divisor.Mantissa, 
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
    private static ScientificDecimal Remainder(ScientificDecimal dividend, ScientificDecimal divisor)
    {
        if (divisor == 0) throw new ArithmeticException("Cannot calculate the remainder of a division by zero");
        if (dividend._infinite || divisor._infinite) 
            throw new ArithmeticException("Cannot calculate the remainder of infinity");
        return dividend - divisor * Math.Truncate((double)(dividend / divisor));
    }
    
    public static ScientificDecimal Mod(ScientificDecimal value, ScientificDecimal mod)
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
    public static ScientificDecimal Square(ScientificDecimal value)
        => value * value;

    /// <summary>
    /// Calculates an integer power of a value.
    /// </summary>
    /// <param name="value">Base value.</param>
    /// <param name="amount">Exponent value.</param>
    /// <returns>The base raised to the exponent.</returns>
    public static ScientificDecimal IntPow(ScientificDecimal value, int amount)
    {
        ScientificDecimal result = One;
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
    public static ScientificDecimal Sqrt(ScientificDecimal value)
    {
        if (value.Negative) throw new ArithmeticException("Cannot take the square root of a negative ScientificDecimal");
        if (value._infinite) return value;
        if (value.Exponent % 2 != 0) value.IncreaseExponent(value.Exponent + 1);
        return new ScientificDecimal(Math.Sqrt(value.Mantissa), value.Exponent / 2);
    }
    
    public static double Atan2(ScientificDecimal y, ScientificDecimal x)
    {
        double quotient = ConvertToDouble(y / x);
        if (x > 0) return Math.Atan(quotient);
        if (x < 0 && y >= 0) return Math.Atan(quotient) + Math.PI;
        if (x < 0 && y < 0) return Math.Atan(quotient) - Math.PI;
        if (x == 0 & y > 0) return Math.PI / 2;
        if (x == 0 & y < 0) return -Math.PI / 2;
        throw new DivideByZeroException("Cannot calculate atan2 of 0 / 0");
    }

    public static double Cos(ScientificDecimal value)
        => Math.Cos((double)(value % Math.Tau));

    public static double Sin(ScientificDecimal value)
        => Math.Sin((double)(value % Math.Tau));

    public static double Tan(ScientificDecimal value)
        => Math.Tan((double)(value % Math.PI));

    public static ScientificDecimal Abs(ScientificDecimal value)
    {
        if (value._infinite) return new ScientificDecimal(true);
        return new (Math.Abs(value.Mantissa), value.Exponent);
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

    public static ScientificDecimal Round(ScientificDecimal value, MidpointRounding mode = MidpointRounding.ToEven)
    {
        if (value._infinite) throw new ArithmeticException("Cannot round infinite ScientificDecimal");
        if (value.Mantissa == 0) return value;
        if (value.Exponent < -1) return 0;
        if (value.Exponent == -1) return new(double.Round(value.Mantissa * 0.1, mode), 0);
        return new(double.Round(value.Mantissa, Math.Clamp(value.Exponent, 0, 15), mode), value.Exponent);
    }

    public static ScientificDecimal Floor(ScientificDecimal value)
    {
        if (value._infinite) throw new ArithmeticException("Cannot round infinite ScientificDecimal");
        if (value.Mantissa == 0) return value;
        ScientificDecimal roundDiff = value - Round(value);
        if (roundDiff < 0) return value - 1 - roundDiff;
        return value - roundDiff;
    }

    public static ScientificDecimal Ceiling(ScientificDecimal value)
    {
        if (value._infinite) throw new ArithmeticException("Cannot round infinite ScientificDecimal");
        if (value.Mantissa == 0) return value;
        ScientificDecimal roundDiff = value - Round(value);
        if (roundDiff > 0) return value + 1 - roundDiff;
        return value - roundDiff;
    }

    [Obsolete("MinMagnitude is obsolete, Use Min method instead.")]
    public static ScientificDecimal MinMagnitude(ScientificDecimal x, ScientificDecimal y)
        => Min(x, y);

    public static ScientificDecimal MinMagnitudeNumber(ScientificDecimal x, ScientificDecimal y)
        => IsInfinity(x) ? IsInfinity(y) ? throw new ArithmeticException() : y : IsInfinity(y) ? x : Min(x, y);
    
    [Obsolete("MaxMagnitude is obsolete. Use Max method instead.")]
    public static ScientificDecimal MaxMagnitude(ScientificDecimal x, ScientificDecimal y)
        => Max(x, y);

    public static ScientificDecimal MaxMagnitudeNumber(ScientificDecimal x, ScientificDecimal y)
        => IsInfinity(x) ? IsInfinity(y) ? throw new ArithmeticException() : y : IsInfinity(y) ? x : Max(x, y);

    public static ScientificDecimal Clamp(ScientificDecimal value, ScientificDecimal min, ScientificDecimal max)
    {
        if (max < min) throw new ArgumentException("ScientificDecimal clamp maximum cannot be less than the minimum");
        return value < min ? min : value > max ? max : value;
    }

    public static ScientificDecimal operator +(ScientificDecimal value) 
        => value;

    public static ScientificDecimal operator -(ScientificDecimal value)
    {
        if (value._infinite) return new(value.Negative);
        return new(-value.Mantissa, value.Exponent);
    } 
    public static ScientificDecimal operator +(ScientificDecimal left, ScientificDecimal right) 
        => Add(left, right);
    public static ScientificDecimal operator -(ScientificDecimal left, ScientificDecimal right) 
        => Add(left, -right);
    public static ScientificDecimal operator ++(ScientificDecimal value) 
        => Add(value, 1);
    public static ScientificDecimal operator --(ScientificDecimal value)
        => Add(value, -1);
    public static ScientificDecimal operator*(ScientificDecimal left, ScientificDecimal right)
        => Multiply(left, right);
    public static ScientificDecimal operator/(ScientificDecimal dividend, ScientificDecimal divisor)
        => Divide(dividend, divisor);
    public static ScientificDecimal operator %(ScientificDecimal value, ScientificDecimal mod)
        => Remainder(value, mod);
    public static bool operator ==(ScientificDecimal left, ScientificDecimal right)
        => left.Equals(right);
    public static bool operator !=(ScientificDecimal left, ScientificDecimal right) 
        => !left.Equals(right);
    public static bool operator <(ScientificDecimal left, ScientificDecimal right)
    {
        if (left._infinite) return left.Negative;
        if (right._infinite) return right.Positive;
        if (left.Positive != right.Positive) return right.Positive;
        if (left.Exponent != right.Exponent) return left.Positive ? left.Exponent < right.Exponent : left.Exponent > right.Exponent;
        return (right - left).Positive;
    }

    public static bool operator >(ScientificDecimal left, ScientificDecimal right)
    {
        if (left._infinite) return left.Positive;
        if (right._infinite) return right.Negative;
        if (left.Positive != right.Positive) return left.Positive;
        if (left.Exponent != right.Exponent) return left.Positive ? left.Exponent > right.Exponent : left.Exponent < right.Exponent;
        return (left - right).Positive;
    }

    public static bool operator <=(ScientificDecimal left, ScientificDecimal right)
        => left < right || left == right;
    public static bool operator >=(ScientificDecimal left, ScientificDecimal right)
        => left > right || left == right;

    // to ScientificDecimal
    public static implicit operator ScientificDecimal(int value) 
        => new(value, 0);

    public static implicit operator ScientificDecimal(double value)
        => FromDouble(value);
    
    public static implicit operator ScientificDecimal(float value) 
        => FromDouble(value);

    // from ScientificDecimal
    public static explicit operator double(ScientificDecimal value)
        => ConvertToDouble(value);
    
    public static explicit operator float(ScientificDecimal value)
        => Convert.ToSingle((double)value);
    
    public static explicit operator int(ScientificDecimal value)
        => (int)ConvertToDouble(value);
    
    public static explicit operator uint(ScientificDecimal value)
        => (uint)ConvertToDouble(value);
    
    public static explicit operator long (ScientificDecimal value)
        => (long)ConvertToDouble(value);
    
    static bool INumberBase<ScientificDecimal>.IsZero(ScientificDecimal value)
        => value is { _infinite: false, Mantissa: 0 };
    
    [Obsolete("IsPositive method is obsolete. Use Positive property instead.")]
    static bool INumberBase<ScientificDecimal>.IsPositive(ScientificDecimal value)
        => (value == 0) || value.Positive;
    
    [Obsolete("IsNegative method is obsolete. Use Negative property instead.")]
    static bool INumberBase<ScientificDecimal>.IsNegative(ScientificDecimal value)
        => value.Negative;

    static bool INumberBase<ScientificDecimal>.IsFinite(ScientificDecimal value)
        => !value._infinite;
    
    static bool INumberBase<ScientificDecimal>.IsRealNumber(ScientificDecimal value)
        => !value._infinite;

    static bool INumberBase<ScientificDecimal>.IsImaginaryNumber(ScientificDecimal value)
        => false;
    
    static bool INumberBase<ScientificDecimal>.IsComplexNumber(ScientificDecimal value)
        => false;
    
    public static bool IsInteger(ScientificDecimal value)
        => !value._infinite && double.IsInteger(ConvertToDoubleSaturating(value));

    public static bool IsEvenInteger(ScientificDecimal value)
        => !value._infinite && double.Abs(ConvertToDoubleSaturating(value) % 2) <= ComparisonTolerance;
    
    public static bool IsOddInteger(ScientificDecimal value)
        => !value._infinite && double.Abs(ConvertToDoubleSaturating(value) % 2 - 1) <= ComparisonTolerance;

    static bool INumberBase<ScientificDecimal>.IsNaN(ScientificDecimal value)
        => value._infinite || double.IsNaN(value.Mantissa);
    
    public static bool IsInfinity(ScientificDecimal value)
        => value._infinite;

    public static bool IsPositiveInfinity(ScientificDecimal value)
        => value is { Positive: true, _infinite: true };
    
    public static bool IsNegativeInfinity(ScientificDecimal value)
        => value is { Negative: true, _infinite: true };
    
    static bool INumberBase<ScientificDecimal>.IsCanonical(ScientificDecimal value)
        => value.Mantissa is >= 0 and < 10;
    
    static bool INumberBase<ScientificDecimal>.IsNormal(ScientificDecimal value)
        => double.IsNormal(value.Mantissa);

    static bool INumberBase<ScientificDecimal>.IsSubnormal(ScientificDecimal value)
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
    
    public static ScientificDecimal Parse(string s, IFormatProvider? provider = null)
    {
        throw new NotImplementedException();
    }
    
    public static ScientificDecimal Parse(string s, NumberStyles style, IFormatProvider? provider = null)
        => Parse(s, provider);

    public static ScientificDecimal Parse(ReadOnlySpan<char> s, IFormatProvider? provider = null)
        => Parse(s.ToString(), provider);
    
    public static ScientificDecimal Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider = null)
        => Parse(s.ToString(), provider);

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out ScientificDecimal result)
    {
        throw new NotImplementedException();
    }
    
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, 
        out ScientificDecimal result)
        => TryParse(s, provider, out result);

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ScientificDecimal result)
        => TryParse(s.ToString(), provider, out result);
    
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, 
        out ScientificDecimal result)
        => TryParse(s.ToString(), provider, out result);

    static bool INumberBase<ScientificDecimal>.TryConvertFromChecked<TOther>(TOther value, out ScientificDecimal result)
    {
        result = new();
        return false;
    }

    static bool INumberBase<ScientificDecimal>.TryConvertFromSaturating<TOther>(TOther value, out ScientificDecimal result) 
    {
        result = new();
        return false;
    }

    static bool INumberBase<ScientificDecimal>.TryConvertFromTruncating<TOther>(TOther value, out ScientificDecimal result)
    {
        result = new();
        return false;
    }

    static bool INumberBase<ScientificDecimal>.TryConvertToChecked<TOther>(ScientificDecimal value, [MaybeNullWhen(false)] out TOther result)
    {
        result = default;
        return false;
    }

    static bool INumberBase<ScientificDecimal>.TryConvertToSaturating<TOther>(ScientificDecimal value, [MaybeNullWhen(false)] out TOther result) 
    {
        result = default;
        return false;
    }

    static bool INumberBase<ScientificDecimal>.TryConvertToTruncating<TOther>(ScientificDecimal value, [MaybeNullWhen(false)] out TOther result) 
    {
        result = default;
        return false;
    }
    
    public int CompareTo(object? obj)
    {
        if (obj is ScientificDecimal other)
            return this < other ? -1 : this > other ? 1 : 0;
        return -1;
    }
    public int CompareTo(ScientificDecimal other)
        => this < other ? -1 : this > other ? 1 : 0;
    
    public override bool Equals(object? obj)
    {
        return obj is ScientificDecimal other && Equals(other);
    }

    public bool Equals(ScientificDecimal other)
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