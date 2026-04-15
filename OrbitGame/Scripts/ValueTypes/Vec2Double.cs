using System;

namespace OrbitGame;

public readonly struct Vec2Double(double x, double y) : IEquatable<Vec2Double>
{
    public static Vec2Double Zero = new(0, 0);
    
    public double X { get; } = x;

    public double Y { get; } = y;
    
    public static Vec2Double FromPolar(double angle)
        => new(Math.Cos(angle), Math.Sin(angle));
 
    public static Vec2Double FromPolar(double angle, double magnitude)
        => new(magnitude * Math.Cos(angle), magnitude * Math.Sin(angle));
    
    public static double Dot(Vec2Double left, Vec2Double right)
        => left.X * right.X + left.Y * right.Y;
    
    public static Vec3Double Cross(Vec2Double left, Vec2Double right)
        => new(0, 0, left.X * right.Y - left.Y * right.X);
    
    public double MagnitudeSquared()
        => X * X + Y * Y;
    
    public double Magnitude()
        => Math.Sqrt(MagnitudeSquared());
    
    public double Direction()
    {
        if (X == 0 && Y == 0) throw new DivideByZeroException();
        
        double angle = Math.Atan2(Y, X);
        return Utils.WrapAngle(angle);
    }

    public static double Direction(Vec2Double start, Vec2Double end)
    {
        Vec2Double difference = end - start;
        return difference.Direction();
    }
    
    public Vec2Double Normalize()
    {
        if (this == Zero)
            throw new DivideByZeroException("Cannot normalize zero vector");
        return this / Magnitude();
    }
    
    public static Vec2Double RotatePoint(Vec2Double point, double angle)
        => new(
            point.X * Math.Cos(angle) - point.Y * Math.Sin(angle),
            point.X * Math.Sin(angle) + point.Y * Math.Cos(angle)
        );

    public static Vec2Double operator +(Vec2Double value) 
        => value;
    public static Vec2Double operator -(Vec2Double value) 
        => new(-value.X, -value.Y);
    public static Vec2Double operator +(Vec2Double a, Vec2Double b)
        => new(a.X + b.X, a.Y + b.Y);
    public static Vec2Double operator -(Vec2Double a, Vec2Double b)
        => a + -b;
    public static Vec2Double operator *(Vec2Double a, double b) 
        => new(a.X * b, a.Y * b);
    public static Vec2Double operator /(Vec2Double a, double b)
        => new(a.X / b, a.Y / b);
    public static Vec2<SDecimal> operator *(Vec2Double a, SDecimal b)
        => new(a.X * b, a.Y * b);
    public static bool operator ==(Vec2Double left, Vec2Double right)
        => left.Equals(right);
    public static bool operator !=(Vec2Double left, Vec2Double right)
        => !left.Equals(right);
    
    public static implicit operator Vec3Double(Vec2Double vec)
        => new(vec.X, vec.Y, 0);
    
    public static implicit operator Vec2<SDecimal>(Vec2Double vec)
        => new(vec.X, vec.Y);
    
    public static implicit operator Vec3<SDecimal>(Vec2Double vec)
        => new(vec.X, vec.Y, 0);
    
    public override string ToString()
        => "<" + X + ", " + Y + ">";

    public string ToString(string? format, IFormatProvider? formatProvider = null) 
        => "<" + X.ToString(format, formatProvider) + ", " + Y.ToString(format, formatProvider) + ">";

    public bool Equals(Vec2Double other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }
    
    public override bool Equals(object? obj)
    {
        return obj is Vec2Double other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}