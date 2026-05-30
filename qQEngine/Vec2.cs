namespace qQEngine;

/// <summary>
/// Represents a 2D vector with arbitrary place decimals as arguments.
/// </summary>
/// <param name="x">X-component</param>
/// <param name="y">Y-component</param>
public readonly struct Vec2(SDecimal x, SDecimal y) : IEquatable<Vec2>, IFormattable 
{
    /// <summary>
    /// Value of the zero vector.
    /// </summary>
    public static Vec2 Zero = new(SDecimal.Zero, SDecimal.Zero);
    
    /// <summary>
    /// X-component.
    /// </summary>
    public SDecimal X { get; } = x;
    /// <summary>
    /// Y-component.
    /// </summary>
    public SDecimal Y { get; } = y;
    
    /// <summary>
    /// Creates a unit direction vector.
    /// </summary>
    /// <param name="angle">Angle of the vector.</param>
    /// <returns>A unit vector with a direction of the angle.</returns>
    public static Vec2 FromPolar(double angle)
        => new(SDecimal.Cos(angle), SDecimal.Sin(angle));
 
    /// <summary>
    /// Creates a vector from polar coordinates.
    /// </summary>
    /// <param name="angle">Direction of the vector.</param>
    /// <param name="magnitude">Magnitude of the vector.</param>
    /// <returns>A vector with the provided direction and magnitude.</returns>
    public static Vec2 FromPolar(double angle, SDecimal magnitude)
        => new(magnitude * SDecimal.Cos(angle), magnitude * SDecimal.Sin(angle));
    
    /// <summary>
    /// Calculates the dot product between two vectors.
    /// </summary>
    /// <param name="left">Left vector.</param>
    /// <param name="right">Right vector.</param>
    /// <returns>The dot product of the left and right vectors.</returns>
    public static SDecimal Dot(Vec2 left, Vec2 right)
        => left.X * right.X + left.Y * right.Y;
    
    /// <summary>
    /// Calculates the 3D cross product between two vectors.
    /// </summary>
    /// <param name="left">Left vector.</param>
    /// <param name="right">Right vector.</param>
    /// <returns>The cross product of the left and right vectors.</returns>
    public static Vec3 Cross(Vec2 left, Vec2 right)
        => new(SDecimal.Zero, SDecimal.Zero, left.X * right.Y - left.Y * right.X);
    
    /// <summary>
    /// Calculates the vector's magnitude squared.
    /// </summary>
    /// <returns>The magnitude squared of this vector.</returns>
    public SDecimal MagnitudeSquared()
        => SDecimal.Square(X) + SDecimal.Square(Y);
    
    /// <summary>
    /// Calculate the vector's magnitude.
    /// </summary>
    /// <returns>The magnitude of this vector</returns>
    public SDecimal Magnitude()
        => SDecimal.Sqrt(MagnitudeSquared());
    
    /// <summary>
    /// Calculates the direction of this vector.
    /// </summary>
    /// <returns>The direction of the vector relative to the positive x-axis in radians.</returns>
    /// <exception cref="DivideByZeroException">
    /// Attempted to calculate the direction of the zero vector.
    /// </exception>
    public double Direction()
    {
        if (X == SDecimal.Zero && Y == SDecimal.Zero) throw new DivideByZeroException();
        
        double angle = SDecimal.Atan2(Y, X);
        return Utils.WrapAngle(angle);
    }

    /// <summary>
    /// Calculates the angle between two vectors.
    /// </summary>
    /// <param name="start">Start vector.</param>
    /// <param name="end">End vector.</param>
    /// <returns>The direction of the difference vector between the start and the end.</returns>
    public static double Direction(Vec2 start, Vec2 end)
    {
        Vec2 difference = end - start;
        return difference.Direction();
    }
    
    /// <summary>
    /// Sets the magnitude of the vector to one.
    /// </summary>
    /// <returns>A vector with the same direction as this but with a magnitude of one.</returns>
    /// <exception cref="DivideByZeroException">
    /// Attempted to normalize the zero vector.
    /// </exception>
    public Vec2 Normalize()
    {
        if (this == Zero)
            throw new DivideByZeroException("Cannot normalize zero vector");
        return this / Magnitude();
    }
    
    /// <summary>
    /// Rotates a vector around the origin.
    /// </summary>
    /// <param name="point">Vector to rotate.</param>
    /// <param name="angle">Angle to rotate by.</param>
    /// <returns>The vector rotated around the origin by the angle.</returns>
    public static Vec2 RotatePoint(Vec2 point, double angle)
        => new(
            point.X * SDecimal.Cos(angle) - point.Y * SDecimal.Sin(angle),
            point.X * SDecimal.Sin(angle) + point.Y * SDecimal.Cos(angle)
        );

    public static Vec2 operator +(Vec2 value) 
        => value;
    public static Vec2 operator -(Vec2 value) 
        => new(-value.X, -value.Y);
    public static Vec2 operator +(Vec2 a, Vec2 b)
        => new(a.X + b.X, a.Y + b.Y);
    public static Vec2 operator -(Vec2 a, Vec2 b)
        => a + -b;
    public static Vec2 operator *(Vec2 a, SDecimal b) 
        => new(a.X * b, a.Y * b);
    public static Vec2 operator /(Vec2 a, SDecimal b)
        => new(a.X / b, a.Y / b);
    public static bool operator ==(Vec2 left, Vec2 right)
        => left.Equals(right);
    public static bool operator !=(Vec2 left, Vec2 right)
        => !left.Equals(right);
    
    public static implicit operator Vec3(Vec2 value)
        => new (value.X, value.Y, SDecimal.Zero);
    
    public static explicit operator Vec2Double(Vec2 value)
        => new ((double)value.X, (double)value.Y);
    
    public override string ToString()
        => "<" + X + ", " + Y + ">";

    public string ToString(string? format, IFormatProvider? formatProvider = null) 
        => "<" + X.ToString(format, formatProvider) + ", " + Y.ToString(format, formatProvider) + ">";

    public bool Equals(Vec2 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }
    
    public override bool Equals(object? obj)
    {
        return obj is Vec2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}