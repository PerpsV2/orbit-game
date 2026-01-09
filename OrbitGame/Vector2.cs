namespace OrbitGame;

public struct Vector2 : IEquatable<Vector2>
{
    public Vector2(ScientificDecimal x, ScientificDecimal y)
    {
        X = x;
        Y = y;
    }

    public static Vector2 FromPolar(double angle, ScientificDecimal magnitude)
        => new(magnitude * Math.Cos(angle), magnitude * Math.Sin(angle));

    public static Vector2 Zero => new(0, 0);
    public ScientificDecimal X { get; set; }
    public ScientificDecimal Y { get; set; }

    public static Vector2 operator -(Vector2 a) => new Vector2(-a.X, -a.Y);

    public static Vector2 operator +(Vector2 a, Vector2 b)
        => new(a.X + b.X, a.Y + b.Y);

    public static Vector2 operator -(Vector2 a, Vector2 b)
        => a + -b;
    
    public static Vector2 operator *(Vector2 a, ScientificDecimal b) 
        => new(a.X * b, a.Y * b);

    public static Vector2 operator /(Vector2 a, ScientificDecimal b)
    {
        if (b == 0) throw new DivideByZeroException();
        return new Vector2(a.X / b, a.Y / b);
    }
    
    // dot product
    public static ScientificDecimal operator *(Vector2 a, Vector2 b)
        => a.X * b.X + a.Y * b.Y;

    public static Vector2 ApplyRotation(Vector2 vector, double angle)
        => new(vector.X * Math.Cos(angle) - vector.Y * Math.Sin(angle),
            vector.X * Math.Sin(angle) + vector.Y * Math.Cos(angle));

    public ScientificDecimal Magnitude()
        => ScientificDecimal.Sqrt(X * X + Y * Y);

    public Vector2 Normalize() =>
        this /= Magnitude();
    
    public static Vector2 DirectionVectorBetween(Vector2 start, Vector2 end)
    {
        Vector2 difference = end - start;
        return difference / difference.Magnitude();
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

    public double PrincipalAngle()
    {
        double angle = Math.Atan((double)(Y / X));
        if (X < 0 && Y > 0) return Math.PI + angle;
        if (X < 0 && Y < 0) return Math.PI + angle;
        if (X > 0 && Y < 0) return Math.Tau + angle;
        return angle;
    }

    public static double AngleTo(Vector2 start, Vector2 end)
    {
        Vector2 difference = end - start;
        return difference.PrincipalAngle();
    }

    public static Vector2 DirectionVector(double angle) =>
        new(Math.Cos(angle), Math.Sin(angle));

    public override string ToString()
        => "<" + X + ", " + Y + ">";

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