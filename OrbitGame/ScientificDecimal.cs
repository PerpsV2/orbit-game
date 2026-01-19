using System.Globalization;
namespace OrbitGame;

/// <summary>
/// Number with decimal precision but arbitrary place value
/// </summary>
public struct ScientificDecimal : IComparable<ScientificDecimal>, IEquatable<ScientificDecimal>, IFormattable
{
    private const int PrintPrecision = Options.ScientificPrintPrecision;
    
    // TODO: Create better definitions for max and minvalue which don't break when doing comparisons
    public static readonly ScientificDecimal MaxValue = new(9.99M, int.MaxValue);
    public static readonly ScientificDecimal MinValue = new(-9.99M, int.MinValue);
    public decimal Mantissa { get; set; }
    public int Exponent { get; set; }
    public bool Positive => decimal.IsPositive(Mantissa);
    public bool Negative => decimal.IsNegative(Mantissa);

    public ScientificDecimal(decimal mantissa, int exponent)
    {
        Mantissa = mantissa;
        Exponent = exponent;
        Normalize();
    }

    public ScientificDecimal(int exponent)
        : this(1m, exponent) {}

    public ScientificDecimal()
        : this(0, 0) {}

    /// <summary>
    /// Sets the largest non-zero digit of the mantissa to be in the ones place
    /// </summary>
    private ScientificDecimal Normalize()
    {
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
        if (exponentDifference == 0) return Normalize();
        while (Exponent != exponent)
        {
            Mantissa /= 10;
            Exponent++;
        }
        return this;
    }
    
    #region Conversions

    // to scientific decimal
    public static implicit operator ScientificDecimal(int value) 
        => new(value, 0);

    public static implicit operator ScientificDecimal(double value)
        => new((decimal)value, 0);
    
    public static implicit operator ScientificDecimal(decimal value) 
        => new(value, 0);

    // from scientific decimal
    public static explicit operator double(ScientificDecimal value)
        => (double)value.Mantissa * Math.Pow(10, value.Exponent);
    
    public static explicit operator float(ScientificDecimal value)
        => Convert.ToSingle((double)value);
    
    public static explicit operator int (ScientificDecimal value)
        => (int)((double)value.Mantissa * Math.Pow(10, value.Exponent));
    
    public static explicit operator uint (ScientificDecimal value)
        => (uint)((double)value.Mantissa * Math.Pow(10, value.Exponent));
    
    #endregion
    
    #region Operators

    private static ScientificDecimal Add(ScientificDecimal left, ScientificDecimal right)
    {
        return (left.Exponent > right.Exponent ? 
            new ScientificDecimal(right.IncreaseExponent(left.Exponent).Mantissa + left.Mantissa, left.Exponent) :
            new ScientificDecimal(left.IncreaseExponent(right.Exponent).Mantissa + right.Mantissa, right.Exponent))
            .Normalize();
    }

    private static ScientificDecimal Multiply(ScientificDecimal left, ScientificDecimal right)
         => new ScientificDecimal(left.Mantissa * right.Mantissa, left.Exponent + right.Exponent).Normalize();

    private static ScientificDecimal Divide(ScientificDecimal dividend, ScientificDecimal divisor)
        => new ScientificDecimal(dividend.Mantissa / divisor.Mantissa, dividend.Exponent - divisor.Exponent).Normalize();

    public ScientificDecimal Square()
        => this * this;
    
    public ScientificDecimal Sqrt()
    {
        if (Mantissa < 0)
            throw new ArgumentOutOfRangeException();
        if (Exponent % 2 != 0) IncreaseExponent(Exponent + 1);
        return new ScientificDecimal(Utils.DecimalSqrt(Mantissa), Exponent / 2);
    }

    public ScientificDecimal Abs()
        => new (Math.Abs(Mantissa), Exponent);

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
        => this < min ? min : this > max ? max : this;
    
    public static ScientificDecimal operator +(ScientificDecimal value) 
        => value;
    public static ScientificDecimal operator -(ScientificDecimal value) 
        => new(-value.Mantissa, value.Exponent);
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
        => (right - left).Positive;
    public static bool operator >(ScientificDecimal left, ScientificDecimal right)
        => (left - right).Positive;
    public static bool operator <=(ScientificDecimal left, ScientificDecimal right)
        => left < right || left == right;
    public static bool operator >=(ScientificDecimal left, ScientificDecimal right)
        => left > right || left == right;
    
    #endregion
    
    public override string ToString()
    {
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