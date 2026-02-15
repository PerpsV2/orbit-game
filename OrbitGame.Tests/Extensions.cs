// ReSharper disable once CheckNamespace
namespace Xunit;

public abstract partial class Assert
{
    private static readonly double Epsilon = 1e-8;
    
    public static void Equal(double a, double b)
    {
        True(Math.Abs(a - b) < Epsilon);
    }

    public static void Equal(OrbitGame.ScientificDecimal a, OrbitGame.ScientificDecimal b)
    {
        if (a.IsInfinite && b.IsInfinite)
        {
            Equal(a.IsInfinite, b.IsInfinite);
            Equal(a.Positive, b.Positive);
        }
        else
        {
            Equal(a.Mantissa, b.Mantissa);
            Equal(a.Exponent, b.Exponent);
        }
    }
    
    public static void Equal(OrbitGame.SD_Vector2 a, OrbitGame.SD_Vector2 b)
    {
        True((double)(a - b).Magnitude() < Epsilon);
    }

    public static void Equal(OrbitGame.SD_Vector3 a, OrbitGame.SD_Vector3 b)
    {
        True((double)(a - b).Magnitude() < Epsilon);
    }

    public static void Equal(OrbitGame.Matrix3X3 a, OrbitGame.Matrix3X3 b)
    {
        for (int i = 0; i < 9; ++i)
            Assert.True(Math.Abs((double)(a.Data[i] - b.Data[i])) < Epsilon);
    }
}