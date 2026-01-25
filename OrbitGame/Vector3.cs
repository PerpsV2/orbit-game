namespace OrbitGame;

public struct Vector3(ScientificDecimal x, ScientificDecimal y, ScientificDecimal z) 
    : IEquatable<Vector3>, IFormattable
{
    public static Vector3 Zero => new(0, 0, 0);
    public ScientificDecimal X { get; set; } = x;
    public ScientificDecimal Y { get; set; } = y;
    public ScientificDecimal Z { get; set; } = z;

    #region Operators

    public static ScientificDecimal Dot(Vector3 left, Vector3 right)
        => left.X * right.X + left.Y * right.Y + left.Z * right.Z;

    public static Vector3 Cross(Vector3 left, Vector3 right)
        => new(
            left.Y * right.Z - left.Z * right.Y, 
            left.Z * right.X - left.X * right.Z, 
            left.X * right.Y - left.Y * right.X
            );

    public ScientificDecimal Magnitude()
        => (X * X + Y * Y + Z * Z).Sqrt();

    public Vector3 Normalize()
        => this /= Magnitude();
    
    public static Vector3 operator +(Vector3 value) 
        => value;
    public static Vector3 operator -(Vector3 value) 
        => new(-value.X, -value.Y, -value.Z);
    public static Vector3 operator +(Vector3 left, Vector3 right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    public static Vector3 operator -(Vector3 left, Vector3 right)
        => left + -right;
    public static Vector3 operator *(Vector3 vector, ScientificDecimal scalar) 
        => new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    public static Vector3 operator /(Vector3 vector, ScientificDecimal scalar)
        => new(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
    public static bool operator ==(Vector3 left, Vector3 right)
        => left.Equals(right);
    public static bool operator !=(Vector3 left, Vector3 right)
        => !left.Equals(right);
    
    #endregion
    
    public static explicit operator Vector2(Vector3 value)
        => new (value.X, value.Y);
    
    public static Vector3 DirectionVectorBetween(Vector3 start, Vector3 end)
    {
        Vector3 difference = end - start;
        return difference / difference.Magnitude();
    }
    
    public override string ToString()
        => "<" + X + ", " + Y + ", " + Z + ">";

    public string ToString(string? format, IFormatProvider? formatProvider) 
        => ToString();

    public bool Equals(Vector3 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vector2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }
}