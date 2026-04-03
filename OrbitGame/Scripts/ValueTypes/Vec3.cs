using System;

namespace OrbitGame;

/// <summary>
/// Represents a 3D vector with arbitrary place decimals as arguments.
/// </summary>
/// <param name="x">X-component</param>
/// <param name="y">Y-component</param>
/// <param name="z">Z-component</param>
/// <typeparam name="T">Arbitrary place decimal type.</typeparam>
public readonly struct Vec3<T>(T x, T y, T z) 
    : IEquatable<Vec3<T>>, IFormattable 
    where T : IArbitraryPlaceDecimal<T>
{
    /// <summary>
    /// Value of the zero vector.
    /// </summary>
    public static Vec3<T> Zero = new(T.Zero, T.Zero, T.Zero);
    
    /// <summary>
    /// X-component.
    /// </summary>
    public T X { get; } = x;
    /// <summary>
    /// Y-component.
    /// </summary>
    public T Y { get; } = y;
    /// <summary>
    /// Z-component
    /// </summary>
    public T Z { get; } = z;
    
    /// <summary>
    /// Calculates the dot product between two vectors.
    /// </summary>
    /// <param name="left">Left vector.</param>
    /// <param name="right">Right vector.</param>
    /// <returns>The dot product of the left and right vectors.</returns>
    public static T Dot(Vec3<T> left, Vec3<T> right)
        => left.X * right.X + left.Y * right.Y + left.Z * right.Z;
    
    /// <summary>
    /// Calculates the 3D cross product between two vectors.
    /// </summary>
    /// <param name="left">Left vector.</param>
    /// <param name="right">Right vector.</param>
    /// <returns>The cross product of the left and right vectors.</returns>
    public static Vec3<T> Cross(Vec3<T> left, Vec3<T> right)
        => new(
            left.Y * right.Z - left.Z * right.Y, 
            left.Z * right.X - left.X * right.Z, 
            left.X * right.Y - left.Y * right.X
        );
    
    /// <summary>
    /// Calculates the vector's magnitude squared.
    /// </summary>
    /// <returns>The magnitude squared of this vector.</returns>
    public T MagnitudeSquared()
        => T.Square(X) + T.Square(Y) + T.Square(Z);
    
    /// <summary>
    /// Calculate the vector's magnitude.
    /// </summary>
    /// <returns>The magnitude of this vector</returns>
    public T Magnitude()
        => T.Sqrt(MagnitudeSquared());
    
    /// <summary>
    /// Calculates the direction vector between two vectors.
    /// </summary>
    /// <param name="start">Start vector.</param>
    /// <param name="end">End vector.</param>
    /// <returns>The direction vector from the start vector to the end vector.</returns>
    public static Vec3<T> Direction(Vec3<T> start, Vec3<T> end)
    {
        Vec3<T> difference = end - start;
        return difference.Normalize();
    }
    
    /// <summary>
    /// Sets the magnitude of the vector to one.
    /// </summary>
    /// <returns>A vector with the same direction as this but with a magnitude of one.</returns>
    /// <exception cref="DivideByZeroException">
    /// Attempted to normalize the zero vector.
    /// </exception>
    public Vec3<T> Normalize()
        => new Vec3<T>(X, Y, Z) / Magnitude();
    
    /// <summary>
    /// Maps the type of this vector's components from one arbitrary place decimal to another.
    /// </summary>
    /// <typeparam name="TResult">Resultant type.</typeparam>
    /// <returns>The mapped vector with the same value as this.</returns>
    public Vec3<TResult> Map<TResult>() where TResult : IArbitraryPlaceDecimal<TResult>, new()
        => new(X.Map<TResult>(), Y.Map<TResult>(), Z.Map<TResult>());
    
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
    
    public static explicit operator Vec2<T>(Vec3<T> value)
        => new (value.X, value.Y);
    
    public override string ToString()
        => "<" + X + ", " + Y + ", " + Z + ">";

    public string ToString(string? format, IFormatProvider? formatProvider = null) 
        => "<" + 
           X.ToString(format, formatProvider) + ", " + 
           Y.ToString(format, formatProvider) + ", " + 
           Z.ToString(format, formatProvider) + 
           ">";

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