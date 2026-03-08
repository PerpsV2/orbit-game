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
public readonly struct SD_Vector2(ScientificDecimal x, ScientificDecimal y) 
    : IEquatable<SD_Vector2>, IFormattable
{
    /// <summary>
    /// 2D null vector.
    /// </summary>
    public static SD_Vector2 Zero = new(0, 0);
    public ScientificDecimal X { get; } = x;
    public ScientificDecimal Y { get; } = y;
    
    /// <summary>
    /// Create a unit length SD_Vector2 from a principal angle.
    /// </summary>
    public static SD_Vector2 FromPolar(double angle)
        => new(Math.Cos(angle), Math.Sin(angle));

    /// <summary>
    /// Create a SD_Vector2 from polar coordinates.
    /// </summary>
    public static SD_Vector2 FromPolar(double angle, ScientificDecimal magnitude)
        => new(magnitude * Math.Cos(angle), magnitude * Math.Sin(angle));
    
    public static ScientificDecimal Dot(SD_Vector2 left, SD_Vector2 right)
        => left.X * right.X + left.Y * right.Y;

    /// <summary>
    /// Calculates the cross product of two SD_Vector2 assuming the Z value of each is zero.
    /// </summary>
    /// <returns>Cross product as a SD_Vector3 with only a Z-component</returns>
    public static SD_Vector3 Cross(SD_Vector2 left, SD_Vector2 right)
        => new(0, 0, left.X * right.Y - left.Y * right.X);
    
    /// <summary>
    /// Returns the Euclidean distance magnitude of the vector.
    /// </summary>
    public ScientificDecimal Magnitude()
        => ScientificDecimal.Sqrt(X * X + Y * Y);
    
    /// <summary>
    /// Returns the squared Euclidean distance magnitude of the vector.
    /// </summary>
    /// <remarks>Better performance than the base Magnitude method</remarks>
    public ScientificDecimal MagnitudeSquared()
        => X * X + Y * Y;
    
    /// <summary>
    /// Returns the principal angle direction of the vector.
    /// </summary>
    public double Direction()
    {
        if (X == 0 && Y == 0) throw new DivideByZeroException();
        if (X == 0 && Y > 0) return Math.PI / 2;
        if (X == 0 && Y < 0) return 3 * Math.PI / 2;
        
        double angle = Math.Atan2((double)Y, (double)X);
        return Utils.WrapAngle(angle);
    }

    /// <summary>
    /// Returns the principal angle direction between a start and end vector.
    /// </summary>
    public static double Direction(SD_Vector2 start, SD_Vector2 end)
    {
        SD_Vector2 difference = end - start;
        return difference.Direction();
    }

    /// <summary>
    /// Sets the magnitude of the vector to one without changing direction.
    /// </summary>
    public SD_Vector2 Normalize()
    {
        if (this == Zero)
            throw new ArithmeticException("Cannot normalize zero vector");
        return FromPolar(Direction());
    }
    
    /// <summary>
    /// Rotates a SD_Vector2 about the origin along the Z-axis by a given angle.
    /// </summary>
    public static SD_Vector2 RotatePoint(SD_Vector2 point, double angle)
        => new(
            point.X * Math.Cos(angle) - point.Y * Math.Sin(angle),
            point.X * Math.Sin(angle) + point.Y * Math.Cos(angle)
        );
    
    #region Operators

    public static SD_Vector2 operator +(SD_Vector2 value) 
        => value;
    public static SD_Vector2 operator -(SD_Vector2 value) 
        => new(-value.X, -value.Y);
    public static SD_Vector2 operator +(SD_Vector2 a, SD_Vector2 b)
        => new(a.X + b.X, a.Y + b.Y);
    public static SD_Vector2 operator -(SD_Vector2 a, SD_Vector2 b)
        => a + -b;
    public static SD_Vector2 operator *(SD_Vector2 a, ScientificDecimal b) 
        => new(a.X * b, a.Y * b);
    public static SD_Vector2 operator /(SD_Vector2 a, ScientificDecimal b)
        => new(a.X / b, a.Y / b);
    public static bool operator ==(SD_Vector2 left, SD_Vector2 right)
        => left.Equals(right);
    public static bool operator !=(SD_Vector2 left, SD_Vector2 right)
        => !left.Equals(right);
    
    #endregion
    
    #region Casts
    public static implicit operator SD_Vector3(SD_Vector2 value)
        => new (value.X, value.Y, 0);
    
    public static explicit operator Vector2(SD_Vector2 value)
        => new Vector2((float)value.X, (float)value.Y);
    
    #endregion
    
    public override string ToString()
        => "<" + X + ", " + Y + ">";

    public string ToString(string? format, IFormatProvider? formatProvider) 
        => ToString();

    public bool Equals(SD_Vector2 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }
    
    public override bool Equals(object? obj)
    {
        return obj is SD_Vector2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}