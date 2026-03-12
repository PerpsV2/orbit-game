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
public struct BinaryScientificDecimal : IFormattable
{
    private long _mantissa;
    private int _exponent;
    private int _precision;

    public BinaryScientificDecimal(long mantissa, int exponent, int precision)
    {
        _mantissa = mantissa;
        _exponent = exponent;
        _precision = precision;
        Normalize();
    }

    public static BinaryScientificDecimal FromDecimal(decimal mantissa, int exponent, int precision)
    {
        throw new NotImplementedException();
    }

    private void Normalize()
    {
        while (Math.Abs(_mantissa) < 1L << (_precision - 1))
        {
            _mantissa <<= 1;
            _exponent -= 1;
        }

        while (Math.Abs(_mantissa) > 1L << _precision)
        {
            _mantissa >>= 1;
            _exponent += 1;
        }
    }

    public void IncreaseExponent(int goal)
    {
        bool negative = _mantissa < 0;
        if (negative) _mantissa = -_mantissa;
        if (_exponent == goal) return;
        if (_exponent > goal) throw new ArithmeticException();
        _mantissa >>= goal - _exponent;
        _exponent = goal;
        if (negative) _mantissa = -_mantissa;
    }

    public static BinaryScientificDecimal Add(BinaryScientificDecimal left, BinaryScientificDecimal right)
    {
        if (left._exponent < right._exponent) left.IncreaseExponent(right._exponent);
        else if (right._exponent < left._exponent) right.IncreaseExponent(left._exponent);
        return new(left._mantissa + right._mantissa, 
            int.Max(left._exponent, right._exponent), 
            int.Min(left._precision, right._precision));
    }

    public static BinaryScientificDecimal Multiply(BinaryScientificDecimal left, BinaryScientificDecimal right)
    {
        int resultPrecision = int.Min(left._precision, right._precision);
        long resultMantissa = 0b0L;
        for (int i = 0; i < resultPrecision; ++i)
        {
            for (int j = 0; j <= i; ++j)
            {
                long leftDigitValue = (left._mantissa >> (left._precision - 1 - j)) & 1;
                long rightDigitValue = (right._mantissa >> (right._precision - 1 - (i - j))) & 1;
                if ((leftDigitValue & rightDigitValue) == 1) resultMantissa += 0b1L << (resultPrecision - i);
            }
        }
        int resultExponent = left._precision + left._exponent + right._precision + right._exponent - resultPrecision - 2;
        return new(resultMantissa, resultExponent, resultPrecision);
    }

    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        formatProvider ??= CultureInfo.InvariantCulture;
        
        if (string.IsNullOrEmpty(format))
            format = "G";
        
        switch (format.ToUpperInvariant())
        {
            case "B":
                return $"M:{_mantissa:B}, E:{_exponent}, P:{_precision}";
            case "G":
                return $"M:{_mantissa}, E:{_exponent}, P:{_precision}";
            case "N":
                return $"N:{_mantissa * Math.Pow(2, _exponent)}";
            default:
                throw new FormatException();
        }
    }
}