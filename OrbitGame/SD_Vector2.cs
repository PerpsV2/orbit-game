using System;
using System.Collections.Generic;
using System.Linq;

namespace OrbitGame;

public struct SD_Vector2(ScientificDecimal x, ScientificDecimal y) 
    : IEquatable<SD_Vector2>, IFormattable
{
    public static SD_Vector2 Zero => new(0, 0);
    public ScientificDecimal X { get; set; } = x;
    public ScientificDecimal Y { get; set; } = y;
    
    public static SD_Vector2 FromPolar(double angle)
        => new(Math.Cos(angle), Math.Sin(angle));

    public static SD_Vector2 FromPolar(double angle, ScientificDecimal magnitude)
        => new(magnitude * Math.Cos(angle), magnitude * Math.Sin(angle));

    #region Operators
    
    public static ScientificDecimal Dot(SD_Vector2 left, SD_Vector2 right)
        => left.X * right.X + left.Y * right.Y;

    /// <summary>
    /// returns the cross product of two 2D vectors assuming the Z value of each is zero
    /// </summary>
    public static SD_Vector3 Cross(SD_Vector2 left, SD_Vector2 right)
        => new(0, 0, left.X * right.Y - left.Y * right.X);
    
    public readonly ScientificDecimal Magnitude()
        => (X * X + Y * Y).Sqrt();

    public readonly SD_Vector2 Normalize()
    {
        ScientificDecimal magnitude = Magnitude();
        if (magnitude == 0) throw new ArithmeticException("Cannot normalize zero vector");
        return new(X / magnitude, Y / magnitude);
    }

    public static SD_Vector2 operator +(SD_Vector2 value) 
        => value;
    public static SD_Vector2 operator -(SD_Vector2 value) 
        => new(-value.X, -value.Y);
    public static SD_Vector2 operator +(SD_Vector2 a, SD_Vector2 b)
        => new(a.X + b.X, a.Y + b.Y);
    public static SD_Vector2 operator -(SD_Vector2 a, SD_Vector2 b)
        => a + -b;
    public static SD_Vector2 operator *(SD_Vector2 a, ScientificDecimal b) 
        => new(a.X * b, a.Y * b);
    public static SD_Vector2 operator /(SD_Vector2 a, ScientificDecimal b)
        => new(a.X / b, a.Y / b);
    public static bool operator ==(SD_Vector2 left, SD_Vector2 right)
        => left.Equals(right);
    public static bool operator !=(SD_Vector2 left, SD_Vector2 right)
        => !left.Equals(right);
    
    #endregion
    
    public static implicit operator SD_Vector3(SD_Vector2 value)
        => new (value.X, value.Y, 0);

    /// <summary>
    /// Decomposes the vertices of a convex polygon into triangles
    /// </summary>
    public static (SD_Vector2 a, SD_Vector2 b, SD_Vector2 c)[] TriangulateConvex(SD_Vector2[] points)
    {
        if (points.Length < 3) throw new ArgumentException("Convex shape must have at least 3 points.");
        var triangulation = new (SD_Vector2 a, SD_Vector2 b, SD_Vector2 c)[points.Length - 2];
        for (int i = 1; i < points.Length - 1; ++i)
            triangulation[i - 1] = (points[0], points[i], points[i + 1]);

        return triangulation;
    }
    
    /// <summary>
    /// Returns the center of mass from the vertices of a convex polygon
    /// </summary>
    public static SD_Vector2 CenterOfMassConvex(SD_Vector2[] points)
    {
        var triangles = TriangulateConvex(points);
        SD_Vector2 centerOfMass = Zero;
        foreach (var triangle in triangles)
        {
            SD_Vector2 centroid = (triangle.a + triangle.b + triangle.c) / 3;
            centerOfMass += centroid;
        }

        centerOfMass /= triangles.Length;

        return centerOfMass;
    }

    /// <summary>
    /// Re-centers a convex polygon at its center of mass
    /// </summary>
    public static SD_Vector2[] CenterConvex(SD_Vector2[] points)
        => points.Select(v => v - CenterOfMassConvex(points)).ToArray();

    /// <summary>
    /// Returns whether a set of three points is ordered clockwise or counter-clockwise
    /// </summary>
    public static RotationDirection TripletRotationDirection(SD_Vector2[] triplet)
    {
        if (triplet.Length != 3) throw new ArgumentException("Vector2 triplet must have exactly 3 values");
        double edgeSlope1 = (double)((triplet[1].Y - triplet[0].Y) * (triplet[2].X - triplet[0].X));
        double edgeSlope2 = (double)((triplet[2].Y - triplet[0].Y) * (triplet[1].X - triplet[0].X));
        return edgeSlope1 > edgeSlope2 ? RotationDirection.Clockwise :
            edgeSlope1 < edgeSlope2 ? RotationDirection.Counterclockwise : RotationDirection.None;
    }
    
    /// <summary>
    /// Returns the vertex order of the convex hull for the set of points given as a linked list
    /// </summary>
    public static LinkedList<int> GetConvexHullIndices(SD_Vector2[] points)
    {
        // get leftmost point to start
        int leftmostIndex = 0;
        for (int i = 0; i < points.Length; ++i)
            if (points[i].X < points[leftmostIndex].X) leftmostIndex = i;
        LinkedList<int> convexHull = new();
        convexHull.AddFirst(leftmostIndex);

        int currentIndex = leftmostIndex;
        do {
            int nextIndex = (currentIndex + 1) % points.Length;
            for (int i = 0; i < points.Length; ++i)
            {
                if (i == currentIndex || i == nextIndex) continue;
                if (TripletRotationDirection([points[currentIndex], points[i], points[nextIndex]]) ==
                    RotationDirection.Counterclockwise)
                    nextIndex = i;
            }
            convexHull.AddLast(nextIndex);
            currentIndex = nextIndex;
        } while (currentIndex != leftmostIndex);

        return convexHull;
    }
    
    public static SD_Vector2 DirectionVectorBetween(SD_Vector2 start, SD_Vector2 end)
    {
        SD_Vector2 difference = end - start;
        return difference / difference.Magnitude();
    }

    public double GetPrincipalAngle()
    {
        if (x == 0 && y == 0) throw new DivideByZeroException();
        if (x == 0 && y > 0) return Math.PI / 2;
        if (x == 0 && y < 0) return 3 * Math.PI / 2;
        
        double angle = Math.Atan2((double)Y, (double)X);
        return Utils.UnsignedMod(angle, Math.Tau);
    }

    public static double GetPrincipalAngle(SD_Vector2 start, SD_Vector2 end)
    {
        SD_Vector2 difference = end - start;
        return difference.GetPrincipalAngle();
    }

    public override string ToString()
        => "<" + X + ", " + Y + ">";

    public string ToString(string? format, IFormatProvider? formatProvider) 
        => ToString();

    public bool Equals(SD_Vector2 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    public override bool Equals(object? obj)
    {
        return obj is SD_Vector2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}