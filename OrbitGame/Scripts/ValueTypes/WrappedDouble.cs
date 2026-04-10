using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

namespace OrbitGame;

public class ExtendedDouble(double value) : IArbitraryPlaceDecimal<ExtendedDouble>
{
    public double Value { get; } = value;

    private bool Infinite => double.IsInfinity(Value);
    public bool Positive => double.IsPositive(Value);
    public bool Negative => double.IsNegative(Value);
    
    public static int Radix { get; } = 10;
    
    public static ExtendedDouble Zero { get; } = 0;
    public static ExtendedDouble One { get; } = 1;
    public static ExtendedDouble AdditiveIdentity { get; } = 0;
    public static ExtendedDouble MultiplicativeIdentity { get; } = 1;
    public static ExtendedDouble PositiveInfinity { get; } = double.PositiveInfinity;
    public static ExtendedDouble NegativeInfinity { get; } = double.NegativeInfinity;

    public ExtendedDouble() 
        : this(0) { }
    
    private ExtendedDouble(bool positive) 
        : this(positive ? double.PositiveInfinity : double.NegativeInfinity) { }

    public static ExtendedDouble FromDouble(double value, int exponent = 0)
        => new ExtendedDouble(value * Math.Pow(10, exponent));

    public static double ConvertToDouble(ExtendedDouble value)
        => value.Value;

    public static double ConvertToDoubleSaturating(ExtendedDouble value)
        => value.Value;
    
    public static ExtendedDouble Mod(ExtendedDouble value, ExtendedDouble mod)
    {
        if (mod == 0) throw new DivideByZeroException("Cannot modulate by zero");
        if (value.Infinite || mod.Infinite) throw new ArithmeticException("Cannot modulate infinity or by infinity");
        return value - mod * Math.Floor((double)(value / mod));
    }
    
    /// <summary>
    /// Calculates the square of a value.
    /// </summary>
    /// <param name="value">Value to square.</param>
    /// <returns>The value multiplied by itself.</returns>
    public static ExtendedDouble Square(ExtendedDouble value)
        => value * value;

    /// <summary>
    /// Calculates an integer power of a value.
    /// </summary>
    /// <param name="value">Base value.</param>
    /// <param name="amount">Exponent value.</param>
    /// <returns>The base raised to the exponent.</returns>
    public static ExtendedDouble IntPow(ExtendedDouble value, int amount)
    {
        ExtendedDouble result = One;
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
    public static ExtendedDouble Sqrt(ExtendedDouble value)
    {
        if (value.Negative) throw new ArithmeticException("Cannot take the square root of a negative SDecimal");
        return Math.Sqrt(value.Value);
    }

    public static double Atan2(ExtendedDouble y, ExtendedDouble x)
        => Math.Atan2(y.Value, x.Value);

    public static double Cos(ExtendedDouble value)
        => Math.Cos(value.Value);

    public static double Sin(ExtendedDouble value)
        => Math.Sin(value.Value);

    public static double Tan(ExtendedDouble value)
        => Math.Tan(value.Value);

    public static ExtendedDouble Abs(ExtendedDouble value)
        => Math.Abs(value.Value);

    public static ExtendedDouble Min(ExtendedDouble value, params ExtendedDouble[] values)
    {
        ExtendedDouble result = value;
        foreach (var n in values)
            if (n < result) result = n;
        return result;
    }
    
    public static ExtendedDouble Max(ExtendedDouble value, params ExtendedDouble[] values)
    {
        ExtendedDouble result = value;
        foreach (var n in values)
            if (n > result) result = n;
        return result;
    }

    public static ExtendedDouble Round(ExtendedDouble value, MidpointRounding mode = MidpointRounding.ToEven)
        => Math.Round(value.Value, mode);

    public static ExtendedDouble Floor(ExtendedDouble value)
        => Math.Floor(value.Value);

    public static ExtendedDouble Ceiling(ExtendedDouble value)
        => Math.Ceiling(value.Value);

    [Obsolete("MinMagnitude is obsolete, Use Min method instead.")]
    public static ExtendedDouble MinMagnitude(ExtendedDouble x, ExtendedDouble y)
        => Min(x, y);

    public static ExtendedDouble MinMagnitudeNumber(ExtendedDouble x, ExtendedDouble y)
        => IsInfinity(x) ? IsInfinity(y) ? throw new ArithmeticException() : y : IsInfinity(y) ? x : Min(x, y);
    
    [Obsolete("MaxMagnitude is obsolete. Use Max method instead.")]
    public static ExtendedDouble MaxMagnitude(ExtendedDouble x, ExtendedDouble y)
        => Max(x, y);

    public static ExtendedDouble MaxMagnitudeNumber(ExtendedDouble x, ExtendedDouble y)
        => IsInfinity(x) ? IsInfinity(y) ? throw new ArithmeticException() : y : IsInfinity(y) ? x : Max(x, y);

    public static ExtendedDouble Clamp(ExtendedDouble value, ExtendedDouble min, ExtendedDouble max)
    {
        if (max < min) throw new ArgumentException("ScientificDecimal clamp maximum cannot be less than the minimum");
        return value < min ? min : value > max ? max : value;
    }
    
    public TOther Map<TOther>() where TOther : new()
    {
        ExtendedDouble value = this;
        TOther other = new TOther();
        if (other is ExtendedDouble)
        {
            if (value is TOther result) return result;
        }
        
        else if (other is PDecimal)
        {
            PDecimal pDecimal;
            if (IsPositiveInfinity(value)) pDecimal = PDecimal.PositiveInfinity;
            else if (IsNegativeInfinity(value)) pDecimal = PDecimal.NegativeInfinity;
            else pDecimal = new PDecimal(value.Value, 0);
            if (pDecimal is TOther result) return result;
        }

        else if (other is SDecimal)
        {
            SDecimal sDecimal;
            if (IsPositiveInfinity(value)) sDecimal = SDecimal.PositiveInfinity;
            else if (IsNegativeInfinity(value)) sDecimal = SDecimal.NegativeInfinity;
            else sDecimal = new SDecimal(value.Value, 0);
            if (sDecimal is TOther result) return result;
        }
        
        throw new InvalidCastException();
    }

    public static ExtendedDouble operator +(ExtendedDouble value) 
        => value;
    public static ExtendedDouble operator -(ExtendedDouble value)
        => new(-value.Value);
    public static ExtendedDouble operator +(ExtendedDouble left, ExtendedDouble right) 
        => new(left.Value + right.Value);
    public static ExtendedDouble operator -(ExtendedDouble left, ExtendedDouble right) 
        => new(left.Value - right.Value);
    public static ExtendedDouble operator ++(ExtendedDouble value) 
        => new(value.Value + 1);
    public static ExtendedDouble operator --(ExtendedDouble value)
        => new(value.Value - 1);
    public static ExtendedDouble operator *(ExtendedDouble left, ExtendedDouble right)
        => new(left.Value * right.Value);
    public static ExtendedDouble operator /(ExtendedDouble dividend, ExtendedDouble divisor)
        => new(dividend.Value / divisor.Value);
    public static ExtendedDouble operator %(ExtendedDouble value, ExtendedDouble mod)
        => new(value.Value % mod.Value);
    public static bool operator ==(ExtendedDouble? left, ExtendedDouble? right)
        => left is not null && left.Equals(right);
    public static bool operator !=(ExtendedDouble? left, ExtendedDouble? right) 
        => left is not null && !left.Equals(right);
    public static bool operator <(ExtendedDouble left, ExtendedDouble right)
        => left.Value < right.Value;
    public static bool operator >(ExtendedDouble left, ExtendedDouble right)
        => left.Value > right.Value;
    public static bool operator <=(ExtendedDouble left, ExtendedDouble right)
        => left.Value <= right.Value;
    public static bool operator >=(ExtendedDouble left, ExtendedDouble right)
        => left.Value >= right.Value;

    // to ExtendedDouble
    public static implicit operator ExtendedDouble(int value) 
        => new(value);
    public static implicit operator ExtendedDouble(double value)
        => new(value);
    public static implicit operator ExtendedDouble(float value)
        => new(value);

    // from ExtendedDouble
    public static explicit operator double(ExtendedDouble value)
        => value.Value;
    public static explicit operator float(ExtendedDouble value)
        => Convert.ToSingle(value.Value);
    public static explicit operator int(ExtendedDouble value)
        => (int)value.Value;
    public static explicit operator uint(ExtendedDouble value)
        => (uint)value.Value;
    public static explicit operator long (ExtendedDouble value)
        => (long)value.Value;

    public static explicit operator SDecimal(ExtendedDouble value)
        => value.Map<SDecimal>();

    static bool INumberBase<ExtendedDouble>.IsZero(ExtendedDouble value)
        => value.Value == 0;
    
    [Obsolete("IsPositive method is obsolete. Use Positive property instead.")]
    static bool INumberBase<ExtendedDouble>.IsPositive(ExtendedDouble value)
        => value.Positive;
    
    [Obsolete("IsNegative method is obsolete. Use Negative property instead.")]
    static bool INumberBase<ExtendedDouble>.IsNegative(ExtendedDouble value)
        => value.Negative;

    static bool INumberBase<ExtendedDouble>.IsFinite(ExtendedDouble value)
        => !double.IsInfinity(value.Value);
    
    static bool INumberBase<ExtendedDouble>.IsRealNumber(ExtendedDouble value)
        => double.IsRealNumber(value.Value);

    static bool INumberBase<ExtendedDouble>.IsImaginaryNumber(ExtendedDouble value)
        => false;
    
    static bool INumberBase<ExtendedDouble>.IsComplexNumber(ExtendedDouble value)
        => false;
    
    public static bool IsInteger(ExtendedDouble value)
        => double.IsInteger(value.Value);

    public static bool IsEvenInteger(ExtendedDouble value)
        => double.IsEvenInteger(value.Value);
    
    public static bool IsOddInteger(ExtendedDouble value)
        => double.IsOddInteger(value.Value);

   public static bool IsNaN(ExtendedDouble value)
        => double.IsNaN(value.Value);
    
    public static bool IsInfinity(ExtendedDouble value)
        => double.IsInfinity(value.Value);

    public static bool IsPositiveInfinity(ExtendedDouble value)
        => double.IsPositiveInfinity(value.Value);

    public static bool IsNegativeInfinity(ExtendedDouble value)
        => double.IsNegativeInfinity(value.Value);
    
    static bool INumberBase<ExtendedDouble>.IsCanonical(ExtendedDouble value)
        => true;
    
    static bool INumberBase<ExtendedDouble>.IsNormal(ExtendedDouble value)
        => double.IsNormal(value.Value);

    static bool INumberBase<ExtendedDouble>.IsSubnormal(ExtendedDouble value)
        => double.IsSubnormal(value.Value);
    
    public override string ToString()
        => ToString("G");

    public string ToString(string? format)
        => ToString(format, CultureInfo.InvariantCulture);

    public string ToString(string? format, IFormatProvider? formatProvider)
        => Value.ToString(format, formatProvider);
    
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static ExtendedDouble Parse(string s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static ExtendedDouble Parse(string s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static ExtendedDouble Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }
    
    public static ExtendedDouble Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out ExtendedDouble result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, out ExtendedDouble result)
    {
        throw new NotImplementedException();
    }
    
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, 
        out ExtendedDouble result)
    {
        throw new NotImplementedException();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, 
        out ExtendedDouble result)
    {
        throw new NotImplementedException();
    }

    static bool INumberBase<ExtendedDouble>.TryConvertFromChecked<TOther>(TOther value, out ExtendedDouble result)
    {
        throw new NotImplementedException();
    }

    static bool INumberBase<ExtendedDouble>.TryConvertFromSaturating<TOther>(TOther value, out ExtendedDouble result) 
    {
        throw new NotImplementedException();
    }

    static bool INumberBase<ExtendedDouble>.TryConvertFromTruncating<TOther>(TOther value, out ExtendedDouble result) 
    {
        throw new NotImplementedException();
    }

    static bool INumberBase<ExtendedDouble>.TryConvertToChecked<TOther>(ExtendedDouble value, [MaybeNullWhen(false)] out TOther result) 
    {
        throw new NotImplementedException();
    }

    static bool INumberBase<ExtendedDouble>.TryConvertToSaturating<TOther>(ExtendedDouble value, [MaybeNullWhen(false)] out TOther result) 
    {
        throw new NotImplementedException();
    }

    static bool INumberBase<ExtendedDouble>.TryConvertToTruncating<TOther>(ExtendedDouble value, [MaybeNullWhen(false)] out TOther result) 
    {
        throw new NotImplementedException();
    }
    
    public int CompareTo(object? obj)
    {
        if (obj is ExtendedDouble other)
            return this < other ? -1 : this > other ? 1 : 0;
        return -1;
    }
    public int CompareTo(ExtendedDouble? other)
        => other != null && this < other ? -1 : other != null && this > other ? 1 : 0;
    
    public override bool Equals(object? obj)
    {
        return obj is ExtendedDouble other && Equals(other);
    }

    public bool Equals(ExtendedDouble? other)
        => other != null && Value.Equals(other.Value);

    public override int GetHashCode()
        => Value.GetHashCode();
}