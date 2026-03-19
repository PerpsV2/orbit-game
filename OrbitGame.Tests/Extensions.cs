

using System;
using System.Collections.Generic;
using System.Linq;
using OrbitGame;
// ReSharper disable once CheckNamespace
using Microsoft.Xna.Framework;

namespace Xunit;

public abstract partial class Assert
{
    public static readonly double Epsilon = 1e-10;

    public static void Equal(OrbitGame.PDecimal left, OrbitGame.PDecimal right)
    {
        Equal(left, right, Epsilon);
    }
    
    public static void Equal(OrbitGame.PDecimal left, OrbitGame.PDecimal right, 
        OrbitGame.PDecimal tolerance)
    {
        if (PDecimal.IsInfinity(left) && PDecimal.IsInfinity(right))
        {
            Equal(PDecimal.IsInfinity(left), PDecimal.IsInfinity(right));
            Equal(left.Positive, right.Positive);
        }
        else
        {
            True(PDecimal.Abs(left - right) < tolerance);
        }
    }
    
    public static void Equal(OrbitGame.DVector2<> left, OrbitGame.DVector2<> right)
    {
        Equal(left.X, right.X, Epsilon);
        Equal(left.Y, right.Y, Epsilon);
    }

    public static void Equal(Vector2 left, Vector2 right)
    {
        Equal(left.X, right.X, Epsilon);
        Equal(left.Y, right.Y, Epsilon);
    }

    public static void Equal(OrbitGame.DVector3<> left, OrbitGame.DVector3<> right)
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
    public static void Equal(HashSet<OrbitGame.DVector2<>> left, HashSet<OrbitGame.DVector2<>> right)
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