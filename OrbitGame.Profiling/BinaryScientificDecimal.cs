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
        while (_mantissa < 1L << (_precision - 1))
        {
            _mantissa <<= 1;
            _exponent -= 1;
        }

        while (_mantissa > 1L << _precision)
        {
            _mantissa >>= 1;
            _exponent += 1;
        }
    }

    private void IncreaseExponent(int goal)
    {
        if (_exponent == goal) return;
        if (_exponent > goal) throw new ArithmeticException();
        _mantissa >>= goal - _exponent;
        _exponent = goal;
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
        int resultExponent = resultPrecision - (left._precision + left._exponent) - (right._precision + right._exponent);
        return new(resultMantissa, resultExponent, resultPrecision);
        
        /* result precision is min of left and right precision
         * 1011*2^0 (4 digits) * 101*2^1 (3 digits)
         * 1011 * 1010
         *
         * Major factors
         * i from 0 to < minPrecision
         * i = 0: 1000 * 1000 11 (1011 >> 3) * (1010 >> 3) = 1000000 (1000)
         * i = 1: 1000 * 000  10 (1011 >> 3) * (1010 >> 2) = 000000  (000)
         *        000 * 1000  01 (1011 >> 2) * (1010 >> 3) = 000000  (000)
         * i = 2: 1000 * 10   11 (1011 >> 3) * (1010 >> 1) = 10000   (10)
         *        000 * 000   00 (1011 >> 2) * (1010 >> 2) = 00000   (00)
         *        10 * 1000   11 (1011 >> 1) * (1010 >> 3) = 10000   (10)
         * i = 3: 1000 * 0    10 = (0)
         *        000 * 10    01 = (0)
         *        10 * 000    10 = (0)
         *        1 * 1000    11 = (1)
         * Result 1101
         */
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