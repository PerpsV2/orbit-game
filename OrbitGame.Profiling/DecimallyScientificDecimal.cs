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
public struct DecimallyScientificDecimal
{
    private decimal _mantissa;
    public decimal Mantissa
    {
        readonly get
        {
            return _mantissa;
        }
        private set
        {
            _mantissa = value;
        }
    }

    private int _exponent;
    public int Exponent
    {
        readonly get
        {
            return _exponent;
        }
        private set
        {
            _exponent = value;
        }
    }

    /// <summary>
    /// Create a DecimallyScientificDecimal using a mantissa and an exponent of ten.
    /// </summary>
    /// <param name="mantissa">Mantissa (does not need to be normalized)</param>
    /// <param name="exponent">Exponent of ten</param>
    public DecimallyScientificDecimal(decimal mantissa, int exponent)
    {
        Mantissa = mantissa;
        Exponent = exponent;
        Normalize();
    }

    /// <summary>
    /// Create a DecimallyScientificDecimal using an exponent of ten.
    /// </summary>
    /// <param name="exponent">Exponent of ten</param>
    public DecimallyScientificDecimal(int exponent)
        : this(1, exponent) {}

    public DecimallyScientificDecimal()
        : this(0, 0) {}

    /// <summary>
    /// Sets the largest non-zero digit of the mantissa to be in the ones place
    /// </summary>
    private DecimallyScientificDecimal Normalize()
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
    private DecimallyScientificDecimal IncreaseExponent(int exponent)
    {
        int exponentDifference = exponent - Exponent;
        if (exponentDifference < 0) throw new ArgumentOutOfRangeException();
        if (exponentDifference == 0) return this;
        Mantissa /= (Decimal)Math.Pow(10, exponentDifference);
        Exponent += exponentDifference;

        return this;
    }
    
    private static DecimallyScientificDecimal Add(DecimallyScientificDecimal left, DecimallyScientificDecimal right)
    {
        if (left.Exponent > right.Exponent)
            return new DecimallyScientificDecimal(right.IncreaseExponent(left.Exponent).Mantissa + left.Mantissa, left.Exponent);
        if (right.Exponent > left.Exponent)
            return new DecimallyScientificDecimal(left.IncreaseExponent(right.Exponent).Mantissa + right.Mantissa, right.Exponent);
        return new DecimallyScientificDecimal(left.Mantissa + right.Mantissa, left.Exponent);
    }

    private static DecimallyScientificDecimal Multiply(DecimallyScientificDecimal left, DecimallyScientificDecimal right)
    {
        return new DecimallyScientificDecimal(left.Mantissa * right.Mantissa, 
            left.Exponent + right.Exponent).Normalize();
    }

    private static DecimallyScientificDecimal Divide(DecimallyScientificDecimal dividend, DecimallyScientificDecimal divisor)
    {
        return new DecimallyScientificDecimal(dividend.Mantissa / divisor.Mantissa, 
            dividend.Exponent - divisor.Exponent).Normalize();
    }
    
    public static DecimallyScientificDecimal Square(DecimallyScientificDecimal value)
        => value * value;

    public static DecimallyScientificDecimal operator +(DecimallyScientificDecimal value) 
        => value;

    public static DecimallyScientificDecimal operator -(DecimallyScientificDecimal value)
    {
        return new(-value.Mantissa, value.Exponent);
    } 
    
    public static DecimallyScientificDecimal operator +(DecimallyScientificDecimal left, DecimallyScientificDecimal right) 
        => Add(left, right);
    public static DecimallyScientificDecimal operator -(DecimallyScientificDecimal left, DecimallyScientificDecimal right) 
        => Add(left, -right);
    public static DecimallyScientificDecimal operator*(DecimallyScientificDecimal left, DecimallyScientificDecimal right)
        => Multiply(left, right);
    public static DecimallyScientificDecimal operator/(DecimallyScientificDecimal dividend, DecimallyScientificDecimal divisor)
        => Divide(dividend, divisor);
}