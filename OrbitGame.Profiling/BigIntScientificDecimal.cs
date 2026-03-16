using System.Numerics;

namespace OrbitGame.Profiling;

public struct BigIntScientificDecimal
{
    private BigInteger _mantissa;
    private int _exponent;

    public BigIntScientificDecimal(BigInteger mantissa, int exponent)
    {
        _mantissa = mantissa;
        _exponent = exponent;
        Normalize();
    }
    
    public void Normalize()
    {
        while (_mantissa % 10 == 0)
        {
            _mantissa /= 10;
            _exponent++;
        }
    }

    public void IncreaseExponent(int exponent)
    {
        if (exponent <= _exponent) return;
        while (exponent != _exponent)
        {
            _mantissa *= 10;
            _exponent++;
        }
    }

    public static BigIntScientificDecimal Negate(BigIntScientificDecimal value)
    {
        value._mantissa *= -1;
        return value;
    }

    public static BigIntScientificDecimal Add(BigIntScientificDecimal left, BigIntScientificDecimal right)
    {
        if (left._exponent < right._exponent) left.IncreaseExponent(right._exponent);
        if (right._exponent < left._exponent) right.IncreaseExponent(left._exponent);
        return new(left._mantissa + right._mantissa, left._exponent);
    }


    public static BigIntScientificDecimal operator +(BigIntScientificDecimal value)
        => value;
    
    public static BigIntScientificDecimal operator -(BigIntScientificDecimal value)
        => Negate(value);
    
    public static BigIntScientificDecimal operator +(BigIntScientificDecimal left, BigIntScientificDecimal right)
        => Add(left, right);
    
    public static BigIntScientificDecimal operator -(BigIntScientificDecimal left, BigIntScientificDecimal right)
        => Add(left, -right);
        
    public override string ToString()
        => ToString("G");

    public string ToString(string format)
    {
        if (string.IsNullOrEmpty(format)) format = "G";

        switch (format)
        {
            case "G":
                return _mantissa + "e" + _exponent.ToString("+0;-0");
            default:
                throw new FormatException();
        }
    }
}