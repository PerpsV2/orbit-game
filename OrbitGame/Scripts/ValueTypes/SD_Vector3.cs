using System;

namespace OrbitGame;

/// <summary>
/// 3-dimensional vector struct composed of ScientificDecimal components.
/// </summary>
/// <param name="x">X-component</param>
/// <param name="y">Y-component</param>
/// <param name="z">Z-component</param>
public readonly struct SD_Vector3(ScientificDecimal x, ScientificDecimal y, ScientificDecimal z) 
    : IEquatable<SD_Vector3>, IFormattable
{
    /// <summary>
    /// 3D null vector.
    /// </summary>
    public static SD_Vector3 Zero = new(0, 0, 0);
    
    public ScientificDecimal X { get; } = x;
    public ScientificDecimal Y { get; } = y;
    public ScientificDecimal Z { get; } = z;
    
    /// <summary>
    /// Calculates the dot product of two vectors.
    /// </summary>
    public static ScientificDecimal Dot(SD_Vector3 left, SD_Vector3 right)
        => left.X * right.X + left.Y * right.Y + left.Z * right.Z;

    /// <summary>
    /// Calculates the cross product of two vectors.
    /// </summary>
    public static SD_Vector3 Cross(SD_Vector3 left, SD_Vector3 right)
        => new(
            left.Y * right.Z - left.Z * right.Y, 
            left.Z * right.X - left.X * right.Z, 
            left.X * right.Y - left.Y * right.X
        );

    /// <summary>
    /// Calculates the Euclidean distance magnitude of the vector
    /// </summary>
    public ScientificDecimal Magnitude()
        => (X * X + Y * Y + Z * Z).Sqrt();
    
    /// <summary>
    /// Sets the magnitude of the vector to one without changing direction.
    /// </summary>
    public SD_Vector3 Normalize()
        => new SD_Vector3(X, Y, Z) / Magnitude();
    
    /// <summary>
    /// Returns the normalized direction vector between a start and end vector.
    /// </summary>
    public static SD_Vector3 DirectionVector(SD_Vector3 start, SD_Vector3 end)
    {
        SD_Vector3 difference = end - start;
        return difference / difference.Magnitude();
    }

    #region Operators
    
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
    
    #region Casts
    
    public static explicit operator SD_Vector2(SD_Vector3 value)
        => new (value.X, value.Y);
    
    #endregion
    
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