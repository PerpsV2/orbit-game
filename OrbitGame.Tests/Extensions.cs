// ReSharper disable once CheckNamespace
namespace Xunit;

public abstract partial class Assert
{
    private static readonly int Precision = 5;

    public static void Equal(OrbitGame.ScientificDecimal left, OrbitGame.ScientificDecimal right)
    {
        if (left.IsInfinite && right.IsInfinite)
        {
            Equal(left.IsInfinite, right.IsInfinite);
            Equal(left.Positive, right.Positive);
        }
        else
        {
            Equal(left.Mantissa, right.Mantissa);
            Equal(left.Exponent, right.Exponent);
        }
    }
    
    public static void Equal(OrbitGame.SD_Vector2 left, OrbitGame.SD_Vector2 right)
    {
        Equal(0, (left - right).Magnitude());
    }

    public static void Equal(OrbitGame.SD_Vector3 left, OrbitGame.SD_Vector3 right)
    {
        Equal(0, (left - right).Magnitude());
    }

    public static void Equal(OrbitGame.Matrix3X3 left, OrbitGame.Matrix3X3 right)
    {
        for (int i = 0; i < 9; ++i)
            True(Math.Abs((double)(left.Data[i] - right.Data[i])) < Precision);
    }
}