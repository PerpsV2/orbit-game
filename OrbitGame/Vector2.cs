namespace OrbitGame;

public struct Vector2(ScientificDecimal x, ScientificDecimal y) : IEquatable<Vector2>
{
    public static Vector2 Zero => new(0, 0);
    public ScientificDecimal X { get; set; } = x;
    public ScientificDecimal Y { get; set; } = y;

    public static Vector2 operator -(Vector2 a) => new Vector2(-a.X, -a.Y);

    public static Vector2 operator +(Vector2 a, Vector2 b)
        => new(a.X + b.X, a.Y + b.Y);

    public static Vector2 operator -(Vector2 a, Vector2 b)
        => a + -b;
    
    public static Vector2 operator *(Vector2 a, ScientificDecimal b) 
        => new (a.X * b, a.Y * b);

    public static Vector2 operator /(Vector2 a, ScientificDecimal b)
    {
        if (b == 0) throw new DivideByZeroException();
        return new Vector2(a.X / b, a.Y / b);
    }
    
    // dot product
    public static Vector2 operator *(Vector2 a, Vector2 b)
        => new Vector2(a.X * b.X, a.Y * b.Y);

    public ScientificDecimal Magnitude()
        => ScientificDecimal.Sqrt(X * X + Y * Y);
    
    public static Vector2 DirectionVectorBetween(Vector2 start, Vector2 end)
    {
        Vector2 difference = end - start;
        return difference / difference.Magnitude();
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