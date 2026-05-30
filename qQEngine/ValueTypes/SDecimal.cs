using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace qQEngine;

public struct SDecimal : INumber<SDecimal>
{
    private const int DefaultPrintPrecision = 5;
    
    public static SDecimal Zero { get; } = new(0, 0);
    public static SDecimal One { get; } = new(1, 0);
    public static SDecimal AdditiveIdentity { get; } = new(0, 0);
    public static SDecimal MultiplicativeIdentity { get; } = new(1, 0);
    public static int Radix { get; } = 10;

    public static SDecimal PositiveInfinity { get; } = new(true);
    public static SDecimal NegativeInfinity { get; } = new(false);

    static SDecimal FloatEpsilon { get; } = new(float.Epsilon, 0);
    static SDecimal FloatMinValue { get; } = new(float.MinValue, 0);
    static SDecimal FloatMaxValue { get; } = new(float.MaxValue, 0);
    
    static SDecimal DoubleEpsilon { get; } = new(double.Epsilon, 0);
    static SDecimal DoubleMinValue { get; } = new(double.MinValue, 0);
    static SDecimal DoubleMaxValue { get; } = new(double.MaxValue, 0);
    
    static SDecimal Int32MinValue { get; } = new(int.MinValue, 0);
    static SDecimal Int32MaxValue { get; } = new(int.MaxValue, 0);

    static SDecimal UInt32MinValue { get; } = new(uint.MinValue, 0);
    static SDecimal UInt32MaxValue { get; } = new(uint.MaxValue, 0);

    static SDecimal Int64MinValue { get; } = new(long.MinValue, 0);
    static SDecimal Int64MaxValue { get; } = new(long.MaxValue, 0);

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

    /// <summary>
    /// Constructs a normalized scientific decimal from a mantissa and an exponent.
    /// </summary>
    /// <param name="mantissa">Mantissa of the scientific decimal.</param>
    /// <param name="exponent">Exponent of the scientific decimal.</param>
    public SDecimal(double mantissa, int exponent)
    {
        Mantissa = mantissa;
        Exponent = exponent;
        Normalize();
    }

    /// <summary>
    /// Constructs a scientific decimal from a power of ten.
    /// </summary>
    /// <param name="exponent">Exponent of the power of ten.</param>
    public SDecimal(int exponent)
        : this(1, exponent)
    {
    }

    /// <summary>
    /// Constructs a zero scientific decimal.
    /// </summary>
    public SDecimal()
        : this(0, 0)
    {
    }

    /// <summary>
    /// Constructs an infinite scientific decimal.
    /// </summary>
    /// <param name="positive">Sign of the infinite scientific decimal.</param>
    private SDecimal(bool positive)
        : this(positive ? 1 : -1, 0)
    {
        _infinite = true;
    }

    private SDecimal(SDecimal value)
    {
        if (IsInfinity(value)) this = IsPositive(value) ? PositiveInfinity : NegativeInfinity;
        else
        {
            Mantissa = value.Mantissa;
            Exponent = value.Exponent;
        }

        Normalize();
    }

    /// <summary>
    /// Normalizes this number by adjusting the exponent so that the mantissa is 1-digit long.
    /// When normalizing zero, the exponent is also set to zero.
    /// When normalizing infinities, the exponent is set to zero and the mantissa is set to either 1 or -1 depending on the sign.
    /// </summary>
    private void Normalize()
    {
        if (_infinite)
        {
            _mantissa = IsPositive(this) ? 1 : -1;
            _exponent = 0;
            return;
        }
        
        double absMantissa = Math.Abs(Mantissa);
        
        // value is already normalized
        if (absMantissa is >= 1 and < 10) return;
        
        if (absMantissa == 0)
        {
            Exponent = 0;
            return;
        }

        // if the absolute value of the mantissa is small enough,
        // use iterated multiplication/division instead of multiplying by a power
        if (absMantissa is >= 10 and < 1e+10)
        {
            while (Math.Abs(Mantissa) >= 10)
            {
                Mantissa /= 10;
                Exponent++;
            }

            return;
        }

        if (absMantissa is < 1 and >= 1e-10)
        {
            while (Math.Abs(Mantissa) < 1)
            {
                Mantissa *= 10;
                Exponent--;
            }

            return;
        }

        double exponentDiff = Math.Ceiling(Math.Log10(absMantissa));
        Mantissa *= Math.Pow(10, -exponentDiff);
        Exponent += (int)exponentDiff;
    }

    /// <summary>
    /// Increases the exponent of a scientific decimal while roughly preserving the value.
    /// </summary>
    /// <param name="exponent">The exponent to increase to.</param>
    /// <exception cref="ArgumentException">The exponent to increase to is less than the current exponent.</exception>
    private void IncreaseExponent(int exponent)
    {
        int exponentDiff = exponent - Exponent;
        switch (exponentDiff)
        {
            case 0:
                return;
            case < 0:
                throw new ArgumentException("Exponent argument must be greater than or equal to this number's exponent");
            // if the exponent difference is small enough, use iterated division instead of multiplying by a power of ten.
            case < 10:
            {
                while (Exponent != exponent)
                {
                    Exponent++;
                    Mantissa /= 10;
                }

                return;
            }
            default:
                Exponent = exponent;
                Mantissa /= Math.Pow(10, exponentDiff);
                break;
        }
    }

    /// <summary>
    /// Adds two scientific decimals.
    /// </summary>
    /// <param name="left">Left scientific decimal.</param>
    /// <param name="right">Right scientific decimal.</param>
    /// <returns>The sum of the two scientific decimals.</returns>
    /// <exception cref="ArithmeticException">Attempted to add opposite signed infinite scientific decimals.</exception>
    private static SDecimal Add(SDecimal left, SDecimal right)
    {
        if (left._infinite && right._infinite)
            return IsPositive(left) == IsPositive(right) ? 
                left : throw new ArithmeticException("Cannot add opposite signed infinite ScientificDecimals");
        if (left._infinite) return left;
        if (right._infinite) return right;
        if (left.Exponent > right.Exponent) right.IncreaseExponent(left.Exponent);
        if (right.Exponent > left.Exponent) left.IncreaseExponent(right.Exponent);
        return new(left.Mantissa + right.Mantissa, left.Exponent);
    }

    /// <summary>
    /// Multiplies two scientific decimals.
    /// </summary>
    /// <param name="left">Left scientific decimal.</param>
    /// <param name="right">Right scientific decimal.</param>
    /// <returns>The product of the two scientific decimals.</returns>
    private static SDecimal Multiply(SDecimal left, SDecimal right)
    {
        if (left == 0 || right == 0) return 0;
        if (left._infinite || right._infinite) 
            return new((IsPositive(left) && IsPositive(right)) || (IsNegative(left) && IsNegative(right)));
        return new(left.Mantissa * right.Mantissa, left.Exponent + right.Exponent);
    }

    /// <summary>
    /// Divides two scientific decimals.
    /// </summary>
    /// <param name="dividend">Value to divide.</param>
    /// <param name="divisor">Value to divide by.</param>
    /// <returns>The quotient of the dividend divided by the divisor.</returns>
    /// <exception cref="DivideByZeroException">Attempted to divide zero by zero.</exception>
    /// <exception cref="ArithmeticException">Attempted to divide an infinite scientific decimal by another infinite scientific decimal</exception>
    private static SDecimal Divide(SDecimal dividend, SDecimal divisor)
    {
        if (divisor == 0 && dividend == 0) throw new DivideByZeroException("Cannot divide zero by zero");
        if (divisor == 0) return PositiveInfinity * (IsPositive(dividend) ? 1 : -1);
        if (dividend._infinite && divisor._infinite) 
            throw new ArithmeticException("Cannot divide an infinite ScientificDecimal by another infinite ScientificDecimal");
        if (dividend._infinite) return dividend * divisor;
        if (divisor._infinite) return 0;
        return new(dividend.Mantissa / divisor.Mantissa, dividend.Exponent - divisor.Exponent);
    }

    /// <summary>
    /// Calculates the remainder between two values.
    /// </summary>
    /// <param name="dividend">Value to divide.</param>
    /// <param name="divisor">Value to divide by.</param>
    /// <returns>The remainder of the dividend divided by the divisor.</returns>
    /// <exception cref="DivideByZeroException">Attempted to calculate the remainder of a division by zero.</exception>
    /// <exception cref="ArithmeticException">Attempted to calculate the remainder of infinity</exception>
    private static SDecimal Remainder(SDecimal dividend, SDecimal divisor)
    {
        if (divisor == 0) throw new DivideByZeroException("Cannot calculate the remainder of a division by zero");
        if (IsInfinity(dividend)) throw new ArithmeticException("Cannot calculate the remainder of infinity");
        SDecimal quotient = dividend / divisor;
        if (IsPositive(quotient)) return dividend - divisor * Floor(quotient);
        return new(dividend - divisor * Ceiling(quotient));
    }

    /// <summary>
    /// Calculates the modulo between two values.
    /// </summary>
    /// <param name="value">Value to find the mod of.</param>
    /// <param name="mod">Value to modulate by.</param>
    /// <returns>The value modulated by the mod.</returns>
    /// <exception cref="DivideByZeroException">Attempted to modulate by zero.</exception>
    /// <exception cref="ArithmeticException">Attempted to calculate the modulo of infinity</exception>
    public static SDecimal Mod(SDecimal value, SDecimal mod)
    {
        if (mod == 0) throw new DivideByZeroException("Cannot modulate by zero");
        if (IsInfinity(value)) throw new ArithmeticException("Cannot modulate infinity");
        return new(value - mod * Floor(value / mod));
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
    /// <param name="value">The base of the power.</param>
    /// <param name="amount">The exponent of the power.</param>
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
    /// <param name="value">Value to calculate the square root of.</param>
    /// <returns>The square root of the value.</returns>
    /// <exception cref="ArithmeticException">Attempted to take the square root of a negative number</exception>
    public static SDecimal Sqrt(SDecimal value)
    {
        if (IsNegative(value)) throw new ArithmeticException("Cannot take the square root of a negative SDecimal");
        if (value._infinite) return value;
        if (value.Exponent % 2 != 0) value.IncreaseExponent(value.Exponent + 1);
        return new SDecimal(Math.Sqrt(value.Mantissa), value.Exponent / 2);
    }

    /// <summary>
    /// Calculates the atan2 value of two scientific decimals.
    /// </summary>
    /// <param name="y">Y-value scientific decimal.</param>
    /// <param name="x">X-value scientific decimal.</param>
    /// <returns>The quadrant corrected tangent of the y-value divided by the x-value.</returns>
    /// <exception cref="DivideByZeroException">Attempted to divide zero by zero while calculating atan2.</exception>
    public static double Atan2(SDecimal y, SDecimal x)
    {
        double quotient = ConvertToDoubleSaturating(y / x);
        if (x > Zero) return Math.Atan(quotient);
        if (x < Zero && y >= Zero) return Math.Atan(quotient) + Math.PI;
        if (x < Zero && y < Zero) return Math.Atan(quotient) - Math.PI;
        if (x == Zero & y > Zero) return Math.PI / 2;
        if (x == Zero & y < Zero) return -Math.PI / 2;
        throw new DivideByZeroException("Cannot calculate atan2 of 0 / 0");
    }

    /// <summary>
    /// Calculates the cosine value of a scientific decimal.
    /// </summary>
    /// <param name="value">Value to calculate the cosine of.</param>
    /// <returns>The cosine of the value.</returns>
    public static double Cos(SDecimal value)
        => Math.Cos((double)(value % Math.Tau));

    /// <summary>
    /// Calculates the sine value of a scientific decimal.
    /// </summary>
    /// <param name="value">Value to calculate the sine of.</param>
    /// <returns>The sine of the value.</returns>
    public static double Sin(SDecimal value)
        => Math.Sin((double)(value % Math.Tau));

    /// <summary>
    /// Calculates the tangent value of a scientific decimal.
    /// </summary>
    /// <param name="value">Value to calculate the tangent of.</param>
    /// <returns>The tangent of the value.</returns>
    public static double Tan(SDecimal value)
        => Math.Tan((double)(value % Math.PI));

    public static SDecimal Abs(SDecimal value)
    {
        if (IsInfinity(value)) return PositiveInfinity;
        return new(Math.Abs(value.Mantissa), value.Exponent);
    }

    /// <summary>
    /// Computes the minimum value of one or more scientific decimals.
    /// </summary>
    /// <param name="value">First value.</param>
    /// <param name="values">Additional values.</param>
    /// <returns>The minimum of all the provided values.</returns>
    public static SDecimal Min(SDecimal value, params SDecimal[] values)
    {
        SDecimal result = value;
        foreach (var n in values)
            if (n < result)
                result = n;
        return result;
    }

    /// <summary>
    /// Computes the maximum value of one or more scientific decimals.
    /// </summary>
    /// <param name="value">First value.</param>
    /// <param name="values">Additional values.</param>
    /// <returns>The maximum of all the provided values.</returns>
    public static SDecimal Max(SDecimal value, params SDecimal[] values)
    {
        SDecimal result = value;
        foreach (var n in values)
            if (n > result)
                result = n;
        return result;
    }

    /// <summary>
    /// Rounds a scientific decimal to the nearest integer.
    /// </summary>
    /// <param name="value">Value to round.</param>
    /// <param name="mode">Behaviour of rounding midpoints.</param>
    /// <returns>The value rounded to the nearest integer.</returns>
    public static SDecimal Round(SDecimal value, MidpointRounding mode = MidpointRounding.ToEven)
    {
        if (IsInfinity(value) || value.Mantissa == 0) return value;
        if (value.Exponent < -1) return 0;
        if (value.Exponent == -1) return new(double.Round(value.Mantissa * 0.1, mode), 0);
        return new(double.Round(value.Mantissa, Math.Clamp(value.Exponent, 0, 15), mode), value.Exponent);
    }

    /// <summary>
    /// Rounds a scientific decimal down to an integer.
    /// </summary>
    /// <param name="value">Value to round.</param>
    /// <returns>The value rounded down to an integer.</returns>
    public static SDecimal Floor(SDecimal value)
    {
        if (IsInfinity(value) || value.Mantissa == 0) return value;
        SDecimal roundDiff = value - Round(value);
        if (roundDiff < Zero) return value - One - roundDiff;
        return value - roundDiff;
    }

    /// <summary>
    /// Rounds a scientific decimal up to an integer.
    /// </summary>
    /// <param name="value">Value to round.</param>
    /// <returns>The value rounded up to an integer.</returns>
    public static SDecimal Ceiling(SDecimal value)
    {
        if (IsInfinity(value) || value.Mantissa == 0) return value;
        SDecimal roundDiff = value - Round(value);
        if (roundDiff > Zero) return value + One - roundDiff;
        return value - roundDiff;
    }

    /// <summary>
    /// Truncates the decimal portion of a scientific decimal.
    /// </summary>
    /// <param name="value">Value to truncate.</param>
    /// <returns>The truncated integer value.</returns>
    public static SDecimal Truncate(SDecimal value)
    {
        if (IsPositive(value)) 
            return Floor(value);
        return Ceiling(value);
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
        if ((IsInfinity(min) && IsInfinity(max) && IsPositive(min) == IsPositive(max)) || max < min)
            throw new ArgumentOutOfRangeException(nameof(max), "SDecimal clamp maximum cannot be less than the minimum");
        return value < min ? min : value > max ? max : value;
    }

    public static SDecimal operator +(SDecimal value)
        => value;

    public static SDecimal operator -(SDecimal value)
    {
        if (IsPositiveInfinity(value)) return NegativeInfinity;
        if (IsNegativeInfinity(value)) return PositiveInfinity;
        return new(-value.Mantissa, value.Exponent);
    }

    public static SDecimal operator +(SDecimal left, SDecimal right)
        => Add(left, right);

    public static SDecimal operator -(SDecimal left, SDecimal right)
        => Add(left, -right);

    public static SDecimal operator ++(SDecimal value)
        => value + One;

    public static SDecimal operator --(SDecimal value)
        => value - One;

    public static SDecimal operator *(SDecimal left, SDecimal right)
        => Multiply(left, right);

    public static SDecimal operator /(SDecimal left, SDecimal right)
        => Divide(left, right);

    public static SDecimal operator %(SDecimal left, SDecimal right)
        => Remainder(left, right);

    public static bool operator ==(SDecimal left, SDecimal right)
        => left.Equals(right);

    public static bool operator !=(SDecimal left, SDecimal right)
        => !left.Equals(right);

    public static bool operator >(SDecimal left, SDecimal right)
    {
        if (IsInfinity(left)) return IsPositive(left);
        if (IsInfinity(right)) return IsNegative(right);
        SDecimal difference = left - right;
        return IsPositive(difference) && difference != Zero;
    }

    public static bool operator >=(SDecimal left, SDecimal right)
        => left > right || left == right;

    public static bool operator <(SDecimal left, SDecimal right)
    {
        if (IsInfinity(left)) return IsNegative(left);
        if (IsInfinity(right)) return IsPositive(right);
        SDecimal difference = right - left;
        return IsPositive(difference) && difference != Zero;
    }

    public static bool operator <=(SDecimal left, SDecimal right)
        => left < right || left == right;

    // to ScientificDecimal
    public static implicit operator SDecimal(int value) 
        => new(value, 0);

    public static implicit operator SDecimal(uint value)
        => new(value, 0);

    public static implicit operator SDecimal(long value)
        => new(value, 0);

    public static implicit operator SDecimal(double value)
        => FromDouble(value);
    
    public static implicit operator SDecimal(float value) 
        => FromDouble(value);

    // from ScientificDecimal
    public static explicit operator double(SDecimal value)
        => ConvertToDoubleChecked(value);
    
    public static explicit operator float(SDecimal value)
        => ConvertToFloatChecked(value);
    
    public static explicit operator int(SDecimal value)
        => ConvertToIntChecked(value);
    
    public static explicit operator uint(SDecimal value)
        => ConvertToUIntChecked(value);
    
    public static explicit operator long(SDecimal value)
        => ConvertToLongChecked(value);

    static bool INumberBase<SDecimal>.IsZero(SDecimal value)
        => !value._infinite && value.Mantissa == 0;
    
    public static bool IsPositive(SDecimal value)
        => value._mantissa >= 0;
    
    public static bool IsNegative(SDecimal value)
        => value._mantissa < 0;

    static bool INumberBase<SDecimal>.IsFinite(SDecimal value)
        => !value._infinite;
    
    static bool INumberBase<SDecimal>.IsRealNumber(SDecimal value)
        => !IsInfinity(value);

    static bool INumberBase<SDecimal>.IsImaginaryNumber(SDecimal value)
        => false;

    static bool INumberBase<SDecimal>.IsComplexNumber(SDecimal value)
        => false;
    
    public static bool IsInteger(SDecimal value)
        => !value._infinite && double.IsInteger(ConvertToDoubleSaturating(value));
    
    public static bool IsEvenInteger(SDecimal value)
        => !value._infinite && double.IsEvenInteger(ConvertToDoubleSaturating(value));
    
    public static bool IsOddInteger(SDecimal value)
        => !value._infinite && double.IsOddInteger(ConvertToDoubleSaturating(value));

    public static bool IsInfinity(SDecimal value)
        => value._infinite;
    
    public static bool IsPositiveInfinity(SDecimal value)
        => value._infinite && IsPositive(value);

    public static bool IsNegativeInfinity(SDecimal value)
        => value._infinite && IsNegative(value);

    static bool INumberBase<SDecimal>.IsNaN(SDecimal value)
        => false;

    static bool INumberBase<SDecimal>.IsCanonical(SDecimal value)
        => true;

    static bool INumberBase<SDecimal>.IsNormal(SDecimal value)
        => double.IsNormal(value.Mantissa);
    
    static bool INumberBase<SDecimal>.IsSubnormal(SDecimal value)
        => double.IsSubnormal(value.Mantissa);
    
    private static SDecimal FromDouble(double value)
    {
        if (double.IsPositiveInfinity(value)) return PositiveInfinity;
        if (double.IsNegativeInfinity(value)) return NegativeInfinity;
        if (double.IsNaN(value)) throw new ArgumentException("Cannot convert NaN into an SDecimal");
        return new(value, 0);
    }

    private static float ConvertToFloatSaturating(SDecimal value)
    {
        if (IsPositiveInfinity(value)) return float.PositiveInfinity;
        if (IsNegativeInfinity(value)) return float.NegativeInfinity;
        if (Abs(value) < FloatEpsilon) return 0;
        if (value > FloatMaxValue) return float.MaxValue;
        if (value < FloatMinValue) return float.MinValue;
        return (float)(value.Mantissa * Math.Pow(10, value.Exponent));
    }
    
    private static float ConvertToFloatChecked(SDecimal value)
    {
        if (IsPositiveInfinity(value)) return float.PositiveInfinity;
        if (IsNegativeInfinity(value)) return float.NegativeInfinity;
        if (value > FloatMaxValue) throw new ArgumentOutOfRangeException();
        if (value < FloatMinValue) throw new ArgumentOutOfRangeException();
        return (float)(value.Mantissa * Math.Pow(10, value.Exponent));
    }
    
    private static double ConvertToDoubleSaturating(SDecimal value)
    {
        if (IsPositiveInfinity(value)) return double.PositiveInfinity;
        if (IsNegativeInfinity(value)) return double.NegativeInfinity;
        if (Abs(value) < DoubleEpsilon) return 0;
        if (value > DoubleMaxValue) return double.MaxValue;
        if (value < DoubleMinValue) return double.MinValue;
        return value.Mantissa * Math.Pow(10, value.Exponent);
    }

    private static double ConvertToDoubleChecked(SDecimal value)
    {
        if (IsPositiveInfinity(value)) return double.PositiveInfinity;
        if (IsNegativeInfinity(value)) return double.NegativeInfinity;
        if (value > DoubleMaxValue) throw new ArgumentOutOfRangeException();
        if (value < DoubleMinValue) throw new ArgumentOutOfRangeException();
        return value.Mantissa * Math.Pow(10, value.Exponent);
    }

    private static int ConvertToIntSaturating(SDecimal value)
    {
        if (IsInfinity(value)) throw new ArgumentException("Cannot convert infinite scientific decimal to int");
        if (value > Int32MaxValue) return int.MaxValue;
        if (value < Int32MinValue) return int.MinValue;
        return (int)(value.Mantissa * Math.Pow(10, value.Exponent));
    }

    private static int ConvertToIntChecked(SDecimal value)
    {
        if (IsInfinity(value)) throw new ArgumentException("Cannot convert infinite scientific decimal to int");
        if (value > Int32MaxValue) throw new ArgumentOutOfRangeException();
        if (value < Int32MinValue) throw new ArgumentOutOfRangeException();
        return (int)(value.Mantissa * Math.Pow(10, value.Exponent));
    }
    
    private static uint ConvertToUIntSaturating(SDecimal value)
    {
        if (IsInfinity(value)) throw new ArgumentException("Cannot convert infinite scientific decimal to uint");
        if (value > UInt32MaxValue) return uint.MaxValue;
        if (value < UInt32MinValue) return uint.MinValue;
        return (uint)(value.Mantissa * Math.Pow(10, value.Exponent));
    }

    private static uint ConvertToUIntChecked(SDecimal value)
    {
        if (IsInfinity(value)) throw new ArgumentException("Cannot convert infinite scientific decimal to uint");
        if (value > UInt32MaxValue) throw new ArgumentOutOfRangeException();
        if (value < UInt32MinValue) throw new ArgumentOutOfRangeException();
        return (uint)(value.Mantissa * Math.Pow(10, value.Exponent));
    }

    private static long ConvertToLongSaturating(SDecimal value)
    {
        if (IsInfinity(value)) throw new ArgumentException("Cannot convert infinite scientific decimal to long");
        if (value > Int64MaxValue) return long.MaxValue;
        if (value < Int64MinValue) return long.MinValue;
        return (long)(value.Mantissa * Math.Pow(10, value.Exponent));
    }

    private static long ConvertToLongChecked(SDecimal value)
    {
        if (IsInfinity(value)) throw new ArgumentException("Cannot convert infinite scientific decimal to long");
        if (value > Int64MaxValue) throw new ArgumentOutOfRangeException();
        if (value < Int64MinValue) throw new ArgumentOutOfRangeException();
        return (long)(value.Mantissa * Math.Pow(10, value.Exponent));
    }
    
    static bool INumberBase<SDecimal>.TryConvertFromChecked<TOther>(TOther value, out SDecimal result)
    {
        throw new NotImplementedException();
    }
    
    static bool INumberBase<SDecimal>.TryConvertFromSaturating<TOther>(TOther value, out SDecimal result)
    {
        throw new NotImplementedException();
    }
    
    static bool INumberBase<SDecimal>.TryConvertFromTruncating<TOther>(TOther value, out SDecimal result)
    {
        throw new NotImplementedException();
    }
    
    static bool INumberBase<SDecimal>.TryConvertToChecked<TOther>(SDecimal value, [MaybeNullWhen(false)] out TOther result)
    {
        throw new NotImplementedException();
    }
    
    static bool INumberBase<SDecimal>.TryConvertToSaturating<TOther>(SDecimal value, [MaybeNullWhen(false)] out TOther result)
    {
        throw new NotImplementedException();
    }
    
    static bool INumberBase<SDecimal>.TryConvertToTruncating<TOther>(SDecimal value, [MaybeNullWhen(false)] out TOther result)
    {
        throw new NotImplementedException();
    }
    
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
        if (IsNegative(this)) mantissaString = mantissaString.Substring(1);
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
        if (IsNegative(this)) return "-" + result;
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
    
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
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
        if (IsInfinity(this) && IsInfinity(other))
            return IsPositive(this) && IsPositive(other);
        if (!IsInfinity(this) && !IsInfinity(other))
            return Mantissa.Equals(other.Mantissa) && 
                   Exponent.Equals(other.Exponent);
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Mantissa, Exponent);
    }
}