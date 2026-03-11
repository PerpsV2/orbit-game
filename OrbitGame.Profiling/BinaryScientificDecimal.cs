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
        mantissa *= (decimal)Math.Pow(10, exponent);
        long binMantissa = 0b0L;
        for (int i = 0; i < precision; ++i)
        {
            int closestPowerOfTwo = (int)Math.Floor(Math.Log10((double)mantissa) * Math.Log2(10));
            binMantissa |= 1L << closestPowerOfTwo;
            mantissa -= closestPowerOfTwo;
        }
        
        return new BinaryScientificDecimal(binMantissa, exponent, precision);
    }

    private void Normalize()
    {
        while (_mantissa < 1L << (_precision - 1))
            _mantissa <<= 1;

        while (_mantissa > 1L << _precision)
            _mantissa >>= 1;
    }

    public override string ToString()
    {
        return ToString("G", CultureInfo.CurrentCulture);
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format))
            format = "G";
        
        switch (format?.ToUpperInvariant())
        {
            case "B":
                return $"M:{_mantissa:B}, E:{_exponent}, P:{_precision}";
            case "G":
                return $"M:{_mantissa}, E:{_exponent}, P:{_precision}";
            default:
                throw new FormatException();
        }
    }
}