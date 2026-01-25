namespace OrbitGame.Tests;

public static class AssertExtensions
{
    private static readonly double Epsilon = 1e-10;
    
    public static void FuzzyEquals(double a, double b)
    {
        Assert.True(a - b < Epsilon);
    }
    
    public static void Vector2Equals(Vector2 v1, Vector2 v2)
    {
        Assert.True((double)(v1 - v2).Magnitude() < Epsilon);
    }

    public static void Vector3Equals(Vector3 v1, Vector3 v2)
    {
        Assert.True((double)(v1 - v2).Magnitude() < Epsilon);
    }

    public static void Matrix3X3Equals(Matrix3X3 m1, Matrix3X3 m2)
    {
        for (int i = 0; i < 9; ++i)
            Assert.True((double)(m1.Data[i] - m2.Data[i]) < Epsilon);
    }
}