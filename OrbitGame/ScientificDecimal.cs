using System;
using System.Globalization;
namespace OrbitGame;

/// <summary>
/// Number with decimal precision but arbitrary place value.
/// </summary>
public struct ScientificDecimal : IComparable<ScientificDecimal>, IEquatable<ScientificDecimal>, IFormattable
{
    private const int PrintPrecision = Options.ScientificPrintPrecision;
    
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
        get
        {
            if (_infinite) throw new ArithmeticException("Infinite ScientificDecimal has no mantissa");
            return _mantissa;
        }
        set
        {
            if (_infinite) throw new ArithmeticException("Cannot set mantissa of infinite ScientificDecimal");
            _mantissa = value;
        }
    }

    private int _exponent;
    public int Exponent
    {
        get
        {
            if (_infinite) throw new ArithmeticException("Infinite ScientificDecimal has no exponent");
            return _exponent;
        }
        set
        {
            if (_infinite) throw new ArithmeticException("Cannot set exponent of infinite ScientificDecimal");
            _exponent = value;
        }
    }
    
    public bool Positive => double.IsPositive(_mantissa);
    public bool Negative => double.IsNegative(_mantissa);
    public bool IsInfinite => _infinite;
    

    public ScientificDecimal(double mantissa, int exponent)
    {
        Mantissa = mantissa;
        Exponent = exponent;
        Normalize();
    }

    public ScientificDecimal(int exponent)
        : this(1, exponent) {}

    public ScientificDecimal()
        : this(0, 0) {}

    private ScientificDecimal(bool infinite, bool positive)
        : this(positive ? 1 : -1, 0)
    {
        _infinite = infinite;
    }

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
    
    #region Conversions

    // to scientific decimal
    public static implicit operator ScientificDecimal(int value) 
        => new(value, 0);

    public static implicit operator ScientificDecimal(double value)
        => new(value, 0);
    
    public static implicit operator ScientificDecimal(decimal value) 
        => new((double)value, 0);

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
    
    #region Operators

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

    public ScientificDecimal Square()
        => this * this;
    
    public ScientificDecimal Sqrt()
    {
        if (Negative) throw new ArithmeticException("Cannot take the square root of a negative ScientificDecimal");
        if (_infinite) return this;
        if (Exponent % 2 != 0) IncreaseExponent(Exponent + 1);
        return new ScientificDecimal(Utils.DecimalSqrt(Mantissa), Exponent / 2);
    }

    public ScientificDecimal Abs()
    {
        if (_infinite) return new ScientificDecimal(true, true);
        return new (Math.Abs(Mantissa), Exponent);
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

    public ScientificDecimal Clamp(ScientificDecimal min, ScientificDecimal max)
    {
        if (max < min) throw new ArithmeticException("ScientificDecimal clamp maximum cannot be less than the minimum");
        return this < min ? min : this > max ? max : this;
    }

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
    
    public override string ToString()
    {
        if (_infinite) return (Positive ? "" : "-") + "Infinity";
        string mantissaString = Mantissa.ToString(CultureInfo.InvariantCulture);
        mantissaString = (Positive ? "" : "-") + mantissaString.Substring(Positive ? 0 : 1, 
            Math.Min(PrintPrecision + 1, mantissaString.Length - (Positive ? 0 : 1)));
        return mantissaString + "e" + Exponent.ToString("+0;-#");
    }

    public string ToString(string? format, IFormatProvider? formatProvider) => ToString();

    public int CompareTo(ScientificDecimal other)
        => this < other ? -1 : this > other ? 1 : 0;
    
    public int CompareTo(object? obj)
    {
        if (obj is not ScientificDecimal @decimal) 
            throw new ArgumentException($"Object must be of type {nameof(ScientificDecimal)}");
        return CompareTo(@decimal);
    }

    public bool Equals(ScientificDecimal other)
    {
        if (_infinite && other._infinite) return Positive == other.Positive;
        if (_infinite || other._infinite) return false;
        return Mantissa == other.Mantissa && Exponent == other.Exponent;
    }

    public override bool Equals(object? obj)
    {
        return obj is ScientificDecimal other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Mantissa, Exponent);
    }
}