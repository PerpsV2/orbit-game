using System.Collections;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace OrbitGame.Profiling;

public struct BitScientificDecimal
{
    private string Mantissa { get; set; }
    private int Exponent { get; set; }
    private int PrecisionPlaceValue { get; set; }
    private int SigFigs { get; set; }

    public BitScientificDecimal(string mantissa, int exponent, int precisionPlaceValue)
    {
        SigFigs = mantissa.Length;
        Mantissa = mantissa;
        Exponent = exponent;
        PrecisionPlaceValue = precisionPlaceValue;
        Normalize();
    }

    public void Normalize()
    {
        Mantissa = Mantissa.TrimStart('0');
        SetExponent(-SigFigs);
    }

    public void SetExponent(int exponent)
    {
        if (Exponent == exponent) return;
        if (Exponent > exponent)
        {
            Mantissa += new string('0', Exponent - exponent);
            Exponent = exponent;
        }

        int subStringEnd = Mantissa.Length - 1 - (exponent - Exponent);
        if (subStringEnd < 0) Mantissa = "0";
        else Mantissa = Mantissa.Substring(0, subStringEnd);
        Exponent = exponent;
    }

    public static BitScientificDecimal Add(BitScientificDecimal left, BitScientificDecimal right)
    {
        if (left.Exponent > right.Exponent) right.SetExponent(left.Exponent);
        if (right.Exponent > left.Exponent) left.SetExponent(right.Exponent);
        right.Mantissa = right.Mantissa.PadLeft(left.Mantissa.Length, '0');
        left.Mantissa = left.Mantissa.PadLeft(right.Mantissa.Length, '0');
        int minPrecisionPlaceValue = int.Min(left.PrecisionPlaceValue, right.PrecisionPlaceValue);
        
        StringBuilder sumMantissaBuilder = new(left.Mantissa.Length, left.Mantissa.Length + 1);
        bool carry = false;
        for (int i = right.Mantissa.Length - 1; i >= 0; i--)
        {
            bool rightBit = right.Mantissa[i] == '1';
            bool leftBit = left.Mantissa[i] == '1';
            bool xor1 = rightBit ^ leftBit;
            sumMantissaBuilder.Insert(0, xor1 ^ carry ? '1' : '0');
            carry = (rightBit & leftBit) | (xor1 & carry);
        }

        sumMantissaBuilder.Insert(0, carry ? '1' : '0');
        return new(sumMantissaBuilder.ToString(), left.Exponent, minPrecisionPlaceValue);
    }
 
    public override string ToString()
    {
        return ToString("NB");
    }

    public string ToString(string format)
    {
        if (string.IsNullOrEmpty(format)) format = "NB";
        switch (format)
        {
            case "G":
                return Mantissa + "e" + Exponent.ToString("+0;-#") + " P:" + PrecisionPlaceValue;
            case "N":
                return long.Parse(Mantissa, NumberStyles.BinaryNumber) + "e2" + Exponent.ToString("+0;-#") + " P:" + PrecisionPlaceValue;
            case "NB":
                return BigInteger.Parse("0" + Mantissa, NumberStyles.BinaryNumber) + "e2" + Exponent.ToString("+0;-#") + " P:" + PrecisionPlaceValue;
            default:
                throw new FormatException();
        }
    }
}