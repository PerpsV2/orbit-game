using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace OrbitGame.Profiling;

/// <summary>
/// Number with decimal precision but arbitrary place value.
/// </summary>
public struct ScientificDecimal : INumber<ScientificDecimal>
{
    private const int DefaultPrintPrecision = 5;
    private const double ComparisonTolerance = 0.000000001;

    public static ScientificDecimal Zero => 0;
    public static ScientificDecimal One => 1;
    public static ScientificDecimal AdditiveIdentity => 0;
    public static ScientificDecimal MultiplicativeIdentity => 1;
    public static int Radix => 10;

    /// <summary>
    /// Represents a number that approaches positive infinity.
    /// </summary>
    public static readonly ScientificDecimal PosInfinity = new(true, true);
    /// <summary>
    /// Represents a number that approaches negative infinity.
    /// </summary>
    public static readonly ScientificDecimal NegInfinity = new(true, false);
    
    private readonly bool _infinite = false;

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

    /// <summary>
    /// Create a ScientificDecimal using a mantissa and an exponent of ten.
    /// </summary>
    /// <param name="mantissa">Mantissa (does not need to be normalized)</param>
    /// <param name="exponent">Exponent of ten</param>
    public ScientificDecimal(double mantissa, int exponent)
    {
        Mantissa = mantissa;
        Exponent = exponent;
        Normalize();
    }

    /// <summary>
    /// Create a ScientificDecimal using an exponent of ten.
    /// </summary>
    /// <param name="exponent">Exponent of ten</param>
    public ScientificDecimal(int exponent)
        : this(1, exponent) {}

    public ScientificDecimal()
        : this(0, 0) {}

    private ScientificDecimal(bool infinite, bool positive)
        : this(positive ? 1 : -1, 0)
    {
        _infinite = infinite;
    }

    public readonly bool Positive => double.IsPositive(_mantissa);
    public readonly bool Negative => double.IsNegative(_mantissa);

    /// <summary>
    /// Sets the largest non-zero digit of the mantissa to be in the ones place
    /// </summary>
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
    private ScientificDecimal IncreaseExponent(int exponent)
    {
        int exponentDifference = exponent - Exponent;
        if (exponentDifference < 0) throw new ArgumentOutOfRangeException();
        if (exponentDifference == 0) return this;
        Mantissa /= Math.Pow(10, exponentDifference);
        Exponent += exponentDifference;

        return this;
    }
    
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

    private static ScientificDecimal Multiply(ScientificDecimal left, ScientificDecimal right)
    {
        if (left == 0 || right == 0) return 0;
        if (left._infinite || right._infinite) return new(true, 
            (left.Positive && right.Positive) || (left.Negative && right.Negative));
        return new ScientificDecimal(left.Mantissa * right.Mantissa, 
            left.Exponent + right.Exponent).Normalize();
    }

    private static ScientificDecimal Divide(ScientificDecimal dividend, ScientificDecimal divisor)
    {
        if (divisor == 0 && dividend == 0) throw new ArithmeticException("Cannot divide zero by zero");
        if (divisor == 0) return PosInfinity * (dividend.Positive ? 1 : -1);
        if (dividend._infinite && divisor._infinite) 
            throw new ArithmeticException("Cannot divide an infinite ScientificDecimal by another infinite ScientificDecimal");
        if (dividend._infinite) return dividend * divisor;
        if (divisor._infinite) return 0;
        return new ScientificDecimal(dividend.Mantissa / divisor.Mantissa, 
            dividend.Exponent - divisor.Exponent).Normalize();
    }

    private static ScientificDecimal Modulo(ScientificDecimal value, ScientificDecimal mod)
    {
        if (mod == 0) throw new ArithmeticException("Cannot modulate a value by zero");
        if (value._infinite || mod._infinite) throw new ArithmeticException("Cannot modulate an infinite ScientificDecimal");
        return value - mod * Math.Floor((double)(value / mod));
    }
    
    public static ScientificDecimal Square(ScientificDecimal value)
        => value * value;


    public static ScientificDecimal IntPow(ScientificDecimal value, uint amount)
    {
        ScientificDecimal result = 1;
        for (uint i = 0; i < amount; ++i)
            result *= value;

        return result;
    }

    public static ScientificDecimal Sqrt(ScientificDecimal value)
    {
        if (value.Negative) throw new ArithmeticException("Cannot take the square root of a negative ScientificDecimal");
        if (value._infinite) return value;
        if (value.Exponent % 2 != 0) value.IncreaseExponent(value.Exponent + 1);
        return new ScientificDecimal(Math.Sqrt(value.Mantissa), value.Exponent / 2);
    }

    public static ScientificDecimal Abs(ScientificDecimal value)
    {
        if (value._infinite) return new ScientificDecimal(true, true);
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

    public static ScientificDecimal Clamp(ScientificDecimal value, ScientificDecimal min, ScientificDecimal max)
    {
        if (max < min) throw new ArithmeticException("ScientificDecimal clamp maximum cannot be less than the minimum");
        return value < min ? min : value > max ? max : value;
    }

    /// <summary>
    /// Rounds to the nearest integer value.
    /// </summary>
    public ScientificDecimal Round()
    {
        if (_infinite) throw new ArithmeticException("Cannot round infinite ScientificDecimal");
        if (Mantissa == 0) return this;
        if (Exponent < -1) return 0;
        if (Exponent == -1) return new(double.Round(Mantissa * 0.1), 0);
        return new(double.Round(Mantissa, Exponent), Exponent);
    }
    
    public ScientificDecimal Floor()
    {
        if (_infinite) throw new ArithmeticException("Cannot round infinite ScientificDecimal");
        if (Mantissa == 0) return this;
        if (Exponent < -1) return 0;
        if (Exponent == -1) return new(double.Round(Mantissa * 0.1), 0);
        return new(double.Round(Mantissa, Exponent), Exponent);
    }
    
    #region Operators

    public static ScientificDecimal operator +(ScientificDecimal value) 
        => value;

    public static ScientificDecimal operator -(ScientificDecimal value)
    {
        if (value._infinite) return new(true, value.Negative);
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
        => Modulo(value, mod);
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
    
    #endregion
    
    #region Casts

    // to scientific decimal
    public static implicit operator ScientificDecimal(int value) 
        => new(value, 0);

    public static implicit operator ScientificDecimal(double value)
        => new(value, 0);
    
    public static implicit operator ScientificDecimal(float value) 
        => new(value, 0);

    // from scientific decimal
    public static explicit operator double(ScientificDecimal value)
        => value.Mantissa * Math.Pow(10, value.Exponent);
    
    public static explicit operator float(ScientificDecimal value)
        => Convert.ToSingle((double)value);
    
    public static explicit operator int (ScientificDecimal value)
        => (int)(value.Mantissa * Math.Pow(10, value.Exponent));
    
    public static explicit operator uint (ScientificDecimal value)
        => (uint)(value.Mantissa * Math.Pow(10, value.Exponent));
    
    #endregion
    
    public static bool IsZero(ScientificDecimal value)
        => value is { _infinite: false, Mantissa: 0 };
    
    [Obsolete("IsPositive method is obsolete. Use Positive property instead.")]
    public static bool IsPositive(ScientificDecimal value)
        => IsZero(value) || value.Positive;
    
    [Obsolete("IsNegative method is obsolete. Use Negative property instead.")]
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
        => !value._infinite && double.IsInteger((double)value);

    public static bool IsOddInteger(ScientificDecimal value)
        => !value._infinite && double.Abs((double)value % 2 - 1) <= ComparisonTolerance;

    public static bool IsEvenInteger(ScientificDecimal value)
        => !value._infinite && double.Abs((double)value % 2) <= ComparisonTolerance;

    public static bool IsNaN(ScientificDecimal value)
        => IsInfinity(value) || double.IsNaN(value.Mantissa);
    
    public static bool IsInfinity(ScientificDecimal value)
        => value._infinite;

    public static bool IsNegativeInfinity(ScientificDecimal value)
        => value.Negative && IsInfinity(value);

    public static bool IsPositiveInfinity(ScientificDecimal value)
        => value.Positive && IsInfinity(value);
    
    public static bool IsCanonical(ScientificDecimal value)
        => value.Mantissa is >= 0 and < 10;
    
    public static bool IsNormal(ScientificDecimal value)
        => double.IsNormal(value.Mantissa);

    public static bool IsSubnormal(ScientificDecimal value)
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
    
    public static ScientificDecimal Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static ScientificDecimal Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static ScientificDecimal Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static ScientificDecimal Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out ScientificDecimal result)
    {
        throw new NotImplementedException();
    }
    
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, 
        out ScientificDecimal result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ScientificDecimal result)
    {
        throw new NotImplementedException();
    }
    
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, 
        out ScientificDecimal result)
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromChecked<TOther>(TOther value, out ScientificDecimal result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromSaturating<TOther>(TOther value, out ScientificDecimal result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertFromTruncating<TOther>(TOther value, out ScientificDecimal result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToChecked<TOther>(ScientificDecimal value, [MaybeNullWhen(false)] out TOther result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToSaturating<TOther>(ScientificDecimal value, [MaybeNullWhen(false)] out TOther result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
    }

    public static bool TryConvertToTruncating<TOther>(ScientificDecimal value, [MaybeNullWhen(false)] out TOther result) 
        where TOther : INumberBase<TOther>
    {
        throw new NotImplementedException();
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
        return HashCode.Combine(Mantissa, Exponent);
    }
}