namespace OrbitGame.Tests;

public static class AssertExtensions
{
    private static readonly double Vector2EqualsTolerance = 0.00001;
    
    public static void Vector2Equals(Vector2 v1, Vector2 v2)
    {
        Assert.True((double)(v1 - v2).Magnitude() < Vector2EqualsTolerance);
    }
    
    private static readonly double FuzzyEqualsTolerance = 0.00001;

    public static void FuzzyEquals(double a, double b)
    {
        Assert.True(a - b < FuzzyEqualsTolerance);
    }
}