using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace OrbitGame;

/// <summary>
/// Represents a 2D vector with arbitrary place decimals as arguments.
/// </summary>
/// <param name="x">X-component</param>
/// <param name="y">Y-component</param>
/// <typeparam name="T">Arbitrary place decimal type.</typeparam>
public readonly struct Vec2<T>(T x, T y) : IEquatable<Vec2<T>>, IFormattable 
    where T : IArbitraryPlaceDecimal<T>
{
    /// <summary>
    /// Value of the zero vector.
    /// </summary>
    public static Vec2<T> Zero = new(T.Zero, T.Zero);
    
    /// <summary>
    /// X-component.
    /// </summary>
    public T X { get; } = x;
    /// <summary>
    /// Y-component.
    /// </summary>
    public T Y { get; } = y;
    
    /// <summary>
    /// Creates a unit direction vector.
    /// </summary>
    /// <param name="angle">Angle of the vector.</param>
    /// <returns>A unit vector with a direction of the angle.</returns>
    public static Vec2<T> FromPolar(double angle)
        => new(T.FromDouble(Math.Cos(angle)), T.FromDouble(Math.Sin(angle)));
 
    /// <summary>
    /// Creates a vector from polar coordinates.
    /// </summary>
    /// <param name="angle">Direction of the vector.</param>
    /// <param name="magnitude">Magnitude of the vector.</param>
    /// <returns>A vector with the provided direction and magnitude.</returns>
    public static Vec2<T> FromPolar(double angle, T magnitude)
        => new(magnitude * T.FromDouble(Math.Cos(angle)), magnitude * T.FromDouble(Math.Sin(angle)));
    
    /// <summary>
    /// Calculates the dot product between two vectors.
    /// </summary>
    /// <param name="left">Left vector.</param>
    /// <param name="right">Right vector.</param>
    /// <returns>The dot product of the left and right vectors.</returns>
    public static T Dot(Vec2<T> left, Vec2<T> right)
        => left.X * right.X + left.Y * right.Y;
    
    /// <summary>
    /// Calculates the 3D cross product between two vectors.
    /// </summary>
    /// <param name="left">Left vector.</param>
    /// <param name="right">Right vector.</param>
    /// <returns>The cross product of the left and right vectors.</returns>
    public static Vec3<T> Cross(Vec2<T> left, Vec2<T> right)
        => new(T.Zero, T.Zero, left.X * right.Y - left.Y * right.X);
    
    /// <summary>
    /// Calculates the vector's magnitude squared.
    /// </summary>
    /// <returns>The magnitude squared of this vector.</returns>
    public T MagnitudeSquared()
        => T.Square(X) + T.Square(Y);
    
    /// <summary>
    /// Calculate the vector's magnitude.
    /// </summary>
    /// <returns>The magnitude of this vector</returns>
    public T Magnitude()
        => T.Sqrt(MagnitudeSquared());
    
    /// <summary>
    /// Calculates the direction of this vector.
    /// </summary>
    /// <returns>The direction of the vector relative to the positive x-axis in radians.</returns>
    /// <exception cref="DivideByZeroException">
    /// Attempted to calculate the direction of the zero vector.
    /// </exception>
    public double Direction()
    {
        if (X == T.Zero && Y == T.Zero) throw new DivideByZeroException();
        
        double angle = T.Atan2(Y, X);
        return Utils.WrapAngle(angle);
    }

    /// <summary>
    /// Calculates the angle between two vectors.
    /// </summary>
    /// <param name="start">Start vector.</param>
    /// <param name="end">End vector.</param>
    /// <returns>The direction of the difference vector between the start and the end.</returns>
    public static double Direction(Vec2<T> start, Vec2<T> end)
    {
        Vec2<T> difference = end - start;
        return difference.Direction();
    }
    
    /// <summary>
    /// Sets the magnitude of the vector to one.
    /// </summary>
    /// <returns>A vector with the same direction as this but with a magnitude of one.</returns>
    /// <exception cref="DivideByZeroException">
    /// Attempted to normalize the zero vector.
    /// </exception>
    public Vec2<T> Normalize()
    {
        if (this == Zero)
            throw new DivideByZeroException("Cannot normalize zero vector");
        return this / Magnitude();
    }

    /// <summary>
    /// Maps the type of this vector's components from one arbitrary place decimal to another.
    /// </summary>
    /// <typeparam name="TResult">Resultant type.</typeparam>
    /// <returns>The mapped vector with the same value as this.</returns>
    public Vec2<TResult> Map<TResult>() where TResult : IArbitraryPlaceDecimal<TResult>, new()
        => new(X.Map<TResult>(), Y.Map<TResult>());
    
    /// <summary>
    /// Rotates a vector around the origin.
    /// </summary>
    /// <param name="point">Vector to rotate.</param>
    /// <param name="angle">Angle to rotate by.</param>
    /// <returns>The vector rotated around the origin by the angle.</returns>
    public static Vec2<T> RotatePoint(Vec2<T> point, double angle)
        => new(
            point.X * T.FromDouble(Math.Cos(angle)) - point.Y * T.FromDouble(Math.Sin(angle)),
            point.X * T.FromDouble(Math.Sin(angle)) + point.Y * T.FromDouble(Math.Cos(angle))
        );

    public static Vec2<T> operator +(Vec2<T> value) 
        => value;
    public static Vec2<T> operator -(Vec2<T> value) 
        => new(-value.X, -value.Y);
    public static Vec2<T> operator +(Vec2<T> a, Vec2<T> b)
        => new(a.X + b.X, a.Y + b.Y);
    public static Vec2<T> operator -(Vec2<T> a, Vec2<T> b)
        => a + -b;
    public static Vec2<T> operator *(Vec2<T> a, T b) 
        => new(a.X * b, a.Y * b);
    public static Vec2<T> operator /(Vec2<T> a, T b)
        => new(a.X / b, a.Y / b);
    public static bool operator ==(Vec2<T> left, Vec2<T> right)
        => left.Equals(right);
    public static bool operator !=(Vec2<T> left, Vec2<T> right)
        => !left.Equals(right);
    
    public static implicit operator Vec3<T>(Vec2<T> value)
        => new (value.X, value.Y, T.Zero);
    
    public static explicit operator Vector2(Vec2<T> value)
        => new ((float)T.ConvertToDouble(value.X), (float)T.ConvertToDouble(value.Y));
    
    public static explicit operator Vec2Double(Vec2<T> value)
        => new (T.ConvertToDouble(value.X), T.ConvertToDouble(value.Y));
    
    public override string ToString()
        => "<" + X + ", " + Y + ">";

    public string ToString(string? format, IFormatProvider? formatProvider = null) 
        => "<" + X.ToString(format, formatProvider) + ", " + Y.ToString(format, formatProvider) + ">";

    public bool Equals(Vec2<T> other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }
    
    public override bool Equals(object? obj)
    {
        return obj is Vec2<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}