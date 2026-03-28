using System;

namespace OrbitGame;

/// <summary>
/// 3-dimensional vector struct composed of ScientificDecimal components.
/// </summary>
/// <param name="x">X-component</param>
/// <param name="y">Y-component</param>
/// <param name="z">Z-component</param>
public readonly struct Vec3<T>(T x, T y, T z) 
    : IEquatable<Vec3<T>>, IFormattable 
    where T : IArbitraryPlaceDecimal<T>
{
    /// <summary>
    /// 3D null vector.
    /// </summary>
    public static Vec3<T> Zero = new(T.Zero, T.Zero, T.Zero);
    
    public T X { get; } = x;
    public T Y { get; } = y;
    public T Z { get; } = z;
    
    /// <summary>
    /// Calculates the dot product of two vectors.
    /// </summary>
    public static T Dot(Vec3<T> left, Vec3<T> right)
        => left.X * right.X + left.Y * right.Y + left.Z * right.Z;

    /// <summary>
    /// Calculates the cross product of two vectors.
    /// </summary>
    public static Vec3<T> Cross(Vec3<T> left, Vec3<T> right)
        => new(
            left.Y * right.Z - left.Z * right.Y, 
            left.Z * right.X - left.X * right.Z, 
            left.X * right.Y - left.Y * right.X
        );

    /// <summary>
    /// Calculates the Euclidean distance magnitude of the vector
    /// </summary>
    public T Magnitude()
        => T.Sqrt(X * X + Y * Y + Z * Z);
    
    /// <summary>
    /// Sets the magnitude of the vector to one without changing direction.
    /// </summary>
    public Vec3<T> Normalize()
        => new Vec3<T>(X, Y, Z) / Magnitude();
    
    /// <summary>
    /// Returns the normalized direction vector between a start and end vector.
    /// </summary>
    public static Vec3<T> DirectionVector(Vec3<T> start, Vec3<T> end)
    {
        Vec3<T> difference = end - start;
        return difference / difference.Magnitude();
    }
    
    public Vec3<TResult> Map<TResult>() where TResult : IArbitraryPlaceDecimal<TResult>, new()
        => new(X.Map<TResult>(), Y.Map<TResult>(), Z.Map<TResult>());

    #region Operators
    
    public static Vec3<T> operator +(Vec3<T> value) 
        => value;
    public static Vec3<T> operator -(Vec3<T> value) 
        => new(-value.X, -value.Y, -value.Z);
    public static Vec3<T> operator +(Vec3<T> left, Vec3<T> right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    public static Vec3<T> operator -(Vec3<T> left, Vec3<T> right)
        => left + -right;
    public static Vec3<T> operator *(Vec3<T> vector, T scalar) 
        => new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    public static Vec3<T> operator /(Vec3<T> vector, T scalar)
        => new(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
    public static bool operator ==(Vec3<T> left, Vec3<T> right)
        => left.Equals(right);
    public static bool operator !=(Vec3<T> left, Vec3<T> right)
        => !left.Equals(right);
    
    #endregion
    
    #region Casts
    
    public static explicit operator Vec2<T>(Vec3<T> value)
        => new (value.X, value.Y);
    
    #endregion
    
    public override string ToString()
        => "<" + X + ", " + Y + ", " + Z + ">";

    public string ToString(string? format, IFormatProvider? formatProvider) 
        => ToString();

    public bool Equals(Vec3<T> other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vec2<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }
}