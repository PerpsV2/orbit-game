namespace qQEngine;

/// <summary>
/// Represents a 3D vector with arbitrary place decimals as arguments.
/// </summary>
/// <param name="x">X-component</param>
/// <param name="y">Y-component</param>
/// <param name="z">Z-component</param>
public readonly struct Vec3(SDecimal x, SDecimal y, SDecimal z) : IEquatable<Vec3>, IFormattable
{
    /// <summary>
    /// Value of the zero vector.
    /// </summary>
    public static Vec3 Zero = new(SDecimal.Zero, SDecimal.Zero, SDecimal.Zero);
    
    /// <summary>
    /// X-component.
    /// </summary>
    public SDecimal X { get; } = x;
    /// <summary>
    /// Y-component.
    /// </summary>
    public SDecimal Y { get; } = y;
    /// <summary>
    /// Z-component
    /// </summary>
    public SDecimal Z { get; } = z;
    
    /// <summary>
    /// Calculates the dot product between two vectors.
    /// </summary>
    /// <param name="left">Left vector.</param>
    /// <param name="right">Right vector.</param>
    /// <returns>The dot product of the left and right vectors.</returns>
    public static SDecimal Dot(Vec3 left, Vec3 right)
        => left.X * right.X + left.Y * right.Y + left.Z * right.Z;
    
    /// <summary>
    /// Calculates the 3D cross product between two vectors.
    /// </summary>
    /// <param name="left">Left vector.</param>
    /// <param name="right">Right vector.</param>
    /// <returns>The cross product of the left and right vectors.</returns>
    public static Vec3 Cross(Vec3 left, Vec3 right)
        => new(
            left.Y * right.Z - left.Z * right.Y, 
            left.Z * right.X - left.X * right.Z, 
            left.X * right.Y - left.Y * right.X
        );
    
    /// <summary>
    /// Calculates the vector's magnitude squared.
    /// </summary>
    /// <returns>The magnitude squared of this vector.</returns>
    public SDecimal MagnitudeSquared()
        => SDecimal.Square(X) + SDecimal.Square(Y) + SDecimal.Square(Z);
    
    /// <summary>
    /// Calculate the vector's magnitude.
    /// </summary>
    /// <returns>The magnitude of this vector</returns>
    public SDecimal Magnitude()
        => SDecimal.Sqrt(MagnitudeSquared());
    
    /// <summary>
    /// Calculates the direction vector between two vectors.
    /// </summary>
    /// <param name="start">Start vector.</param>
    /// <param name="end">End vector.</param>
    /// <returns>The direction vector from the start vector to the end vector.</returns>
    public static Vec3 Direction(Vec3 start, Vec3 end)
    {
        Vec3 difference = end - start;
        return difference.Normalize();
    }
    
    /// <summary>
    /// Sets the magnitude of the vector to one.
    /// </summary>
    /// <returns>A vector with the same direction as this but with a magnitude of one.</returns>
    /// <exception cref="DivideByZeroException">
    /// Attempted to normalize the zero vector.
    /// </exception>
    public Vec3 Normalize()
        => new Vec3(X, Y, Z) / Magnitude();
    
    public static Vec3 operator +(Vec3 value) 
        => value;
    public static Vec3 operator -(Vec3 value) 
        => new(-value.X, -value.Y, -value.Z);
    public static Vec3 operator +(Vec3 left, Vec3 right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    public static Vec3 operator -(Vec3 left, Vec3 right)
        => left + -right;
    public static Vec3 operator *(Vec3 vector, SDecimal scalar) 
        => new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    public static Vec3 operator /(Vec3 vector, SDecimal scalar)
        => new(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
    public static bool operator ==(Vec3 left, Vec3 right)
        => left.Equals(right);
    public static bool operator !=(Vec3 left, Vec3 right)
        => !left.Equals(right);
    
    public static explicit operator Vec2(Vec3 value)
        => new (value.X, value.Y);
    
    public override string ToString()
        => "<" + X + ", " + Y + ", " + Z + ">";

    public string ToString(string? format, IFormatProvider? formatProvider = null) 
        => "<" + 
           X.ToString(format, formatProvider) + ", " + 
           Y.ToString(format, formatProvider) + ", " + 
           Z.ToString(format, formatProvider) + 
           ">";

    public bool Equals(Vec3 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vec2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }
}