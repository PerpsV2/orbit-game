namespace OrbitGame.Tests;

public static class AssertExtensions
{
    private static readonly double Epsilon = 1e-8;

    public delegate void AssertEqual<in T>(T a, T b);

    private static void Equal<T>(IList<T> a, IList<T> b, AssertEqual<T> assert)
    {
        Assert.Equal(a.Count, b.Count);
        for (int i = 0; i < a.Count; i++)
            assert(a.ElementAt(i), b.ElementAt(i));
    }
    
    public static void Equal(double a, double b)
    {
        Assert.True(Math.Abs(a - b) < Epsilon);
    }

    public static void Equal(ScientificDecimal a, ScientificDecimal b)
    {
        Assert.True((a - b).Abs() < Epsilon);
    }
    
    public static void Equal(Vector2 a, Vector2 b)
    {
        Assert.True((double)(a - b).Magnitude() < Epsilon);
    }

    public static void Equal(Vector3 a, Vector3 b)
    {
        Assert.True((double)(a - b).Magnitude() < Epsilon);
    }

    public static void Equal(Matrix3X3 a, Matrix3X3 b)
    {
        for (int i = 0; i < 9; ++i)
            Assert.True(Math.Abs((double)(a.Data[i] - b.Data[i])) < Epsilon);
    }

    public static void Equal(PhysicsCollision a, PhysicsCollision b)
    {
        Assert.Equal(a.Reference, b.Reference);
        Assert.Equal(a.Incident, b.Incident);
        Equal(a.CollisionManifold, b.CollisionManifold, Equal);
        Equal(a.PenetrationVector, b.PenetrationVector);
    }
}