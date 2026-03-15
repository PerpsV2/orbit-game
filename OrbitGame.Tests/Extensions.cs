

using System;
using System.Collections.Generic;
using System.Linq;
using OrbitGame;
// ReSharper disable once CheckNamespace
using Microsoft.Xna.Framework;

namespace Xunit;

public abstract partial class Assert
{
    public static readonly double Epsilon = 1e-2;

    public static void Equal(OrbitGame.ScientificDecimal left, OrbitGame.ScientificDecimal right)
    {
        Equal(left, right, Epsilon);
        //Equal(left.ToString(), right.ToString());
        /*if (ScientificDecimal.IsInfinity(left) && ScientificDecimal.IsInfinity(right))
        {
            Equal(ScientificDecimal.IsInfinity(left), ScientificDecimal.IsInfinity(right));
            Equal(left.Positive, right.Positive);
        }
        else
        {
            Equal(left.Exponent, right.Exponent);
            Equal(left.Mantissa, right.Mantissa);
        }*/
    }
    
    public static void Equal(OrbitGame.ScientificDecimal left, OrbitGame.ScientificDecimal right, 
        OrbitGame.ScientificDecimal tolerance)
    {
        if (ScientificDecimal.IsInfinity(left) && ScientificDecimal.IsInfinity(right))
        {
            Equal(ScientificDecimal.IsInfinity(left), ScientificDecimal.IsInfinity(right));
            Equal(left.Positive, right.Positive);
        }
        else
        {
            if (ScientificDecimal.Abs(left - right) < tolerance * Epsilon)
            {
                Equal(left.ToString(), left.ToString());
                Equal(right.ToString(), right.ToString());
            }
            else Equal(left.ToString(), right.ToString());
        }
    }
    
    public static void Equal(OrbitGame.SD_Vector2 left, OrbitGame.SD_Vector2 right)
    {
        Equal(left.X, right.X, Epsilon);
        Equal(left.Y, right.Y, Epsilon);
    }

    public static void Equal(Vector2 left, Vector2 right)
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