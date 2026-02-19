// ReSharper disable once CheckNamespace
namespace Xunit;

public abstract partial class Assert
{
    public static readonly double Epsilon = 1e-10;

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
    
    public static void Equal(OrbitGame.ScientificDecimal left, OrbitGame.ScientificDecimal right, 
        OrbitGame.ScientificDecimal tolerance)
    {
        if (left.IsInfinite && right.IsInfinite)
        {
            Equal(left.IsInfinite, right.IsInfinite);
            Equal(left.Positive, right.Positive);
        }
        else
        {
            True(left - right < tolerance);
        }
    }
    
    public static void Equal(OrbitGame.SD_Vector2 left, OrbitGame.SD_Vector2 right)
    {
        Equal(left.X, right.X, Epsilon);
        Equal(left.Y, right.Y, Epsilon);
    }

    public static void Equal(OrbitGame.SD_Vector3 left, OrbitGame.SD_Vector3 right)
    {
        Equal(left.X, right.X, Epsilon);
        Equal(left.Y, right.Y, Epsilon);
        Equal(left.Z, right.Z, Epsilon);
    }

    public static void Equal(OrbitGame.Matrix3X3 left, OrbitGame.Matrix3X3 right)
    {
        for (int i = 0; i < 9; ++i)
            True(Math.Abs((double)(left.Data[i] - right.Data[i])) < Epsilon);
    }
    
    /// <summary>
    /// Assert equality of two collision manifolds
    /// </summary>
    public static void Equal(HashSet<OrbitGame.SD_Vector2> left, HashSet<OrbitGame.SD_Vector2> right)
    {
        Equal(left.Count, right.Count);
        for (int i = 0; i < left.Count; ++i)
        {
            Equal(left.ElementAt(i), right.ElementAt(i));
        }
    }
        
    public static void Equal(OrbitGame.PhysicsCollision left, OrbitGame.PhysicsCollision right)
    {
        Equal(left.Incident, right.Incident);
        Equal(left.Reference, right.Reference);
        Equal(left.CollisionManifold, right.CollisionManifold);
        Equal(left.PenetrationVector, right.PenetrationVector);
    }
}