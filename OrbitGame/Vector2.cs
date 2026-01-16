namespace OrbitGame;

public struct Vector2(ScientificDecimal x, ScientificDecimal y) 
    : IEquatable<Vector2>, IFormattable
{
    public static Vector2 Zero => new(0, 0);
    public ScientificDecimal X { get; set; } = x;
    public ScientificDecimal Y { get; set; } = y;
    
    public static Vector2 FromPolar(double angle)
        => new(Math.Cos(angle), Math.Sin(angle));

    public static Vector2 FromPolar(double angle, ScientificDecimal magnitude)
        => new(magnitude * Math.Cos(angle), magnitude * Math.Sin(angle));

    #region Operators
    
    public static ScientificDecimal Dot(Vector2 left, Vector2 right)
        => left.X * right.X + left.Y * right.Y;

    // returns the cross product of two 2D vectors assuming the Z value of each is zero
    public static Vector3 Cross(Vector2 left, Vector2 right)
        => new(0, 0, left.X * right.Y - left.Y * right.X);
    
    public ScientificDecimal Magnitude()
        => (X * X + Y * Y).Sqrt();

    public Vector2 Normalize()
        => this /= Magnitude();
    
    public static Vector2 operator +(Vector2 value) 
        => value;
    public static Vector2 operator -(Vector2 value) 
        => new(-value.X, -value.Y);
    public static Vector2 operator +(Vector2 a, Vector2 b)
        => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b)
        => a + -b;
    public static Vector2 operator *(Vector2 a, ScientificDecimal b) 
        => new(a.X * b, a.Y * b);
    public static Vector2 operator /(Vector2 a, ScientificDecimal b)
        => new(a.X / b, a.Y / b);
    public static bool operator ==(Vector2 left, Vector2 right)
        => left.Equals(right);
    public static bool operator !=(Vector2 left, Vector2 right)
        => !left.Equals(right);
    
    #endregion
    
    public static Vector2 DirectionVectorBetween(Vector2 start, Vector2 end)
    {
        Vector2 difference = end - start;
        return difference / difference.Magnitude();
    }

    // returns the inertia tensor for a convex shape
    public static (Vector2 a, Vector2 b, Vector2 c)[] TriangulateConvex(Vector2 origin, Vector2[] points)
    {
        
    }

    public static RotationDirection TripletRotationDirection(Vector2[] triplet)
    {
        ScientificDecimal edgeSlope1 = (triplet[1].Y - triplet[0].Y) * (triplet[2].X - triplet[0].X);
        ScientificDecimal edgeSlope2 = (triplet[2].Y - triplet[0].Y) * (triplet[1].X - triplet[0].X);
        return edgeSlope1 > edgeSlope2 ? RotationDirection.Clockwise :
            edgeSlope1 < edgeSlope2 ? RotationDirection.Counterclockwise : RotationDirection.None;
    }

    // returns the vertex order of the convex hull for the set of points given as a linked list
    public static LinkedList<int> GetConvexHullIndices(Vector2[] points)
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

    public double GetPrincipalAngle()
    {
        if (x == 0 && y == 0) throw new DivideByZeroException();
        if (x == 0 && y > 0) return Math.PI / 2;
        if (x == 0 && y < 0) return 3 * Math.PI / 2;
        
        double angle = Math.Atan((double)(Y / X));
        if (X < 0 && Y > 0) return Math.PI + angle;
        if (X < 0 && Y < 0) return Math.PI + angle;
        if (X > 0 && Y < 0) return Math.Tau + angle;
        return angle;
    }

    public static double GetPrincipalAngle(Vector2 start, Vector2 end)
    {
        Vector2 difference = end - start;
        return difference.GetPrincipalAngle();
    }

    public override string ToString()
        => "<" + X + ", " + Y + ">";

    public string ToString(string? format, IFormatProvider? formatProvider) 
        => ToString();

    public bool Equals(Vector2 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}