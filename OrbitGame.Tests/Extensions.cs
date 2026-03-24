

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OrbitGame;
using Vector2 = Microsoft.Xna.Framework.Vector2;
// ReSharper disable once CheckNamespace

namespace Xunit;

public abstract partial class Assert
{
    public static readonly double Epsilon = 1e-10;

    public static void Equal<T>(T left, T right, T tolerance) where T : IArbitraryPlaceDecimal<T>
    {
        if (T.IsInfinity(left) && T.IsInfinity(right))
        {
            Equal(T.IsInfinity(left), T.IsInfinity(right));
            Equal(left.Positive, right.Positive);
        }
        else
        {
            True(T.Abs(left - right) < tolerance);
        }
    }
    
    public static void Equal<T>(DVector2<T> left, DVector2<T> right) where T : IArbitraryPlaceDecimal<T>
    {
        Equal(left.X, right.X, T.FromDouble(Epsilon));
        Equal(left.Y, right.Y, T.FromDouble(Epsilon));
    }
    
    public static void Equal<T>(DVector2<T> left, DVector2<T> right, double epsilon) where T : IArbitraryPlaceDecimal<T>
    {
        Equal(left.X, right.X, T.FromDouble(epsilon));
        Equal(left.Y, right.Y, T.FromDouble(epsilon));
    }

    public static void Equal(Vector2 left, Vector2 right)
    {
        Equal(left.X, right.X, Epsilon);
        Equal(left.Y, right.Y, Epsilon);
    }

    public static void Equal<T>(OrbitGame.DVector3<T> left, OrbitGame.DVector3<T> right) 
        where T : IArbitraryPlaceDecimal<T>
    {
        Equal(left.X, right.X, T.FromDouble(Epsilon));
        Equal(left.Y, right.Y, T.FromDouble(Epsilon));
        Equal(left.Z, right.Z, T.FromDouble(Epsilon));
    }

    public static void Equal<T>(Matrix3X3<T> left, Matrix3X3<T> right) 
        where T : IArbitraryPlaceDecimal<T>
    {
        for (int i = 0; i < 9; ++i)
            True(T.Abs(left.Data[i] - right.Data[i]) < T.FromDouble(Epsilon));
    }
    
    /// <summary>
    /// Assert equality of two collision manifolds
    /// </summary>
    public static void Equal<T>(HashSet<DVector2<T>> left, HashSet<DVector2<T>> right) 
        where T : IArbitraryPlaceDecimal<T>
    {
        Equal(left.Count, right.Count);
        for (int i = 0; i < left.Count; ++i)
            Equal(left.ElementAt(i), right.ElementAt(i));
    }
        
    public static void Equal(PhysicsCollision left, PhysicsCollision right)
    {
        Equal(left.Incident, right.Incident);
        Equal(left.Reference, right.Reference);
        Equal(left.CollisionManifold, right.CollisionManifold);
        Equal(left.PenetrationVector, right.PenetrationVector);
    }

    public static void Raises<T>(Action<Action<object?, T>> subscribeAction, Action testAction)
    {
        bool eventRaised = false;
        subscribeAction((_, _) => { eventRaised = true; });
        testAction();
        True(eventRaised);
    }
    
    public static void NotRaises<T>(Action<Action<object?, T>> subscribeAction, Action testAction)
    {
        bool eventRaised = false;
        subscribeAction((_, _) => { eventRaised = true; });
        testAction();
        False(eventRaised);
    }
}