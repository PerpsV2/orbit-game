using System;

namespace OrbitGame;

public struct SD_Vector3(ScientificDecimal x, ScientificDecimal y, ScientificDecimal z) 
    : IEquatable<SD_Vector3>, IFormattable
{
    public static SD_Vector3 Zero => new(0, 0, 0);
    public ScientificDecimal X { get; set; } = x;
    public ScientificDecimal Y { get; set; } = y;
    public ScientificDecimal Z { get; set; } = z;

    #region Operators

    public static ScientificDecimal Dot(SD_Vector3 left, SD_Vector3 right)
        => left.X * right.X + left.Y * right.Y + left.Z * right.Z;

    public static SD_Vector3 Cross(SD_Vector3 left, SD_Vector3 right)
        => new(
            left.Y * right.Z - left.Z * right.Y, 
            left.Z * right.X - left.X * right.Z, 
            left.X * right.Y - left.Y * right.X
            );

    public readonly ScientificDecimal Magnitude()
        => (X * X + Y * Y + Z * Z).Sqrt();

    public readonly SD_Vector3 Normalize()
        => new SD_Vector3(X, Y, Z) / Magnitude();
    
    public static SD_Vector3 operator +(SD_Vector3 value) 
        => value;
    public static SD_Vector3 operator -(SD_Vector3 value) 
        => new(-value.X, -value.Y, -value.Z);
    public static SD_Vector3 operator +(SD_Vector3 left, SD_Vector3 right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    public static SD_Vector3 operator -(SD_Vector3 left, SD_Vector3 right)
        => left + -right;
    public static SD_Vector3 operator *(SD_Vector3 vector, ScientificDecimal scalar) 
        => new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    public static SD_Vector3 operator /(SD_Vector3 vector, ScientificDecimal scalar)
        => new(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
    public static bool operator ==(SD_Vector3 left, SD_Vector3 right)
        => left.Equals(right);
    public static bool operator !=(SD_Vector3 left, SD_Vector3 right)
        => !left.Equals(right);
    
    #endregion
    
    public static explicit operator SD_Vector2(SD_Vector3 value)
        => new (value.X, value.Y);
    
    public static SD_Vector3 DirectionVectorBetween(SD_Vector3 start, SD_Vector3 end)
    {
        SD_Vector3 difference = end - start;
        return difference / difference.Magnitude();
    }
    
    public override string ToString()
        => "<" + X + ", " + Y + ", " + Z + ">";

    public string ToString(string? format, IFormatProvider? formatProvider) 
        => ToString();

    public bool Equals(SD_Vector3 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    }

    public override bool Equals(object? obj)
    {
        return obj is SD_Vector2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }
}