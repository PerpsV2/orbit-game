namespace OrbitGame.Tests;

public static class AssertExtensions
{
    private static readonly double Epsilon = 1e-10;
    
    public static void Vector2Equals(Vector2 v1, Vector2 v2)
    {
        Assert.True((double)(v1 - v2).Magnitude() < Epsilon);
    }

    public static void Vector3Equals(Vector3 v1, Vector3 v2)
    {
        Assert.True((double)(v1 - v2).Magnitude() < Epsilon);
    }

    public static void FuzzyEquals(double a, double b)
    {
        Assert.True(a - b < Epsilon);
    }
}