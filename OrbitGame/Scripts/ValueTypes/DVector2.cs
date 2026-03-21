using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace OrbitGame;

/// <summary>
/// 2-dimensional vector struct composed of ScientificDecimal components.
/// </summary>
/// <param name="x">X-component</param>
/// <param name="y">Y-component</param>
public readonly struct DVector2<T>(T x, T y) : IEquatable<DVector2<T>>, IFormattable 
    where T : IArbitraryPlaceDecimal<T>
{
    /// <summary>
    /// 2D null vector.
    /// </summary>
    public static DVector2<T> Zero = new(T.Zero, T.Zero);
    public T X { get; } = x;
    public T Y { get; } = y;
    
    /// <summary>
    /// Create a unit length SD_Vector2 from a principal angle.
    /// </summary>
    public static DVector2<T> FromPolar(double angle)
        => new(T.FromDouble(Math.Cos(angle)), T.FromDouble(Math.Sin(angle)));

    /// <summary>
    /// Create a SD_Vector2 from polar coordinates.
    /// </summary>
    public static DVector2<T> FromPolar(double angle, T magnitude)
        => new(magnitude * T.FromDouble(Math.Cos(angle)), magnitude * T.FromDouble(Math.Sin(angle)));
    
    public static T Dot(DVector2<T> left, DVector2<T> right)
        => left.X * right.X + left.Y * right.Y;

    /// <summary>
    /// Calculates the cross product of two SD_Vector2 assuming the Z value of each is zero.
    /// </summary>
    /// <returns>Cross product as a SD_Vector3 with only a Z-component</returns>
    public static DVector3<T> Cross(DVector2<T> left, DVector2<T> right)
        => new(T.Zero, T.Zero, left.X * right.Y - left.Y * right.X);
    
    /// <summary>
    /// Returns the Euclidean distance magnitude of the vector.
    /// </summary>
    public T Magnitude()
        => T.Sqrt(X * X + Y * Y);
    
    /// <summary>
    /// Returns the squared Euclidean distance magnitude of the vector.
    /// </summary>
    /// <remarks>Better performance than the base Magnitude method</remarks>
    public T MagnitudeSquared()
        => X * X + Y * Y;
    
    /// <summary>
    /// Returns the principal angle direction of the vector.
    /// </summary>
    public double Direction()
    {
        if (X == T.Zero && Y == T.Zero) throw new DivideByZeroException();
        if (X == T.Zero && Y > T.Zero) return Math.PI / 2;
        if (X == T.Zero && Y < T.Zero) return 3 * Math.PI / 2;
        
        double angle = T.Atan2(Y, X);
        return Utils.WrapAngle(angle);
    }

    /// <summary>
    /// Returns the principal angle direction between a start and end vector.
    /// </summary>
    public static double Direction(DVector2<T> start, DVector2<T> end)
    {
        DVector2<T> difference = end - start;
        return difference.Direction();
    }

    /// <summary>
    /// Sets the magnitude of the vector to one without changing direction.
    /// </summary>
    public DVector2<T> Normalize()
    {
        if (this == Zero)
            throw new ArithmeticException("Cannot normalize zero vector");
        return FromPolar(Direction());
    }

    public DVector2<TResult> Map<TResult>() where TResult : IArbitraryPlaceDecimal<TResult>, new()
        => new(X.Map<TResult>(), Y.Map<TResult>());
    
    /// <summary>
    /// Rotates a SD_Vector2 about the origin along the Z-axis by a given angle.
    /// </summary>
    public static DVector2<T> RotatePoint(DVector2<T> point, double angle)
        => new(
            point.X * T.FromDouble(Math.Cos(angle)) - point.Y * T.FromDouble(Math.Sin(angle)),
            point.X * T.FromDouble(Math.Sin(angle)) + point.Y * T.FromDouble(Math.Cos(angle))
        );
    
    #region Operators

    public static DVector2<T> operator +(DVector2<T> value) 
        => value;
    public static DVector2<T> operator -(DVector2<T> value) 
        => new(-value.X, -value.Y);
    public static DVector2<T> operator +(DVector2<T> a, DVector2<T> b)
        => new(a.X + b.X, a.Y + b.Y);
    public static DVector2<T> operator -(DVector2<T> a, DVector2<T> b)
        => a + -b;
    public static DVector2<T> operator *(DVector2<T> a, T b) 
        => new(a.X * b, a.Y * b);
    public static DVector2<T> operator /(DVector2<T> a, T b)
        => new(a.X / b, a.Y / b);
    public static bool operator ==(DVector2<T> left, DVector2<T> right)
        => left.Equals(right);
    public static bool operator !=(DVector2<T> left, DVector2<T> right)
        => !left.Equals(right);
    
    #endregion
    
    #region Casts
    public static implicit operator DVector3<T>(DVector2<T> value)
        => new (value.X, value.Y, T.Zero);
    
    public static explicit operator Vector2(DVector2<T> value)
        => new ((float)T.ToDouble(value.X), (float)T.ToDouble(value.Y));
    
    #endregion
    
    public override string ToString()
        => "<" + X + ", " + Y + ">";

    public string ToString(string? format, IFormatProvider? formatProvider) 
        => ToString();

    public bool Equals(DVector2<T> other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }
    
    public override bool Equals(object? obj)
    {
        return obj is DVector2<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}