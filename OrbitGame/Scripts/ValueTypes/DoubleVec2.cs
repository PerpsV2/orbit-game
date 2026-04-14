using System;

namespace OrbitGame;

public readonly struct DoubleVec2(double x, double y) : IEquatable<DoubleVec2>
{
    public static DoubleVec2 Zero = new(0, 0);
    
    public double X { get; } = x;

    public double Y { get; } = y;
    
    public static DoubleVec2 FromPolar(double angle)
        => new(Math.Cos(angle), Math.Sin(angle));
 
    public static DoubleVec2 FromPolar(double angle, double magnitude)
        => new(magnitude * Math.Cos(angle), magnitude * Math.Sin(angle));
    
    public static double Dot(DoubleVec2 left, DoubleVec2 right)
        => left.X * right.X + left.Y * right.Y;
    
    public static DoubleVec3 Cross(DoubleVec2 left, DoubleVec2 right)
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

    public static double Direction(DoubleVec2 start, DoubleVec2 end)
    {
        DoubleVec2 difference = end - start;
        return difference.Direction();
    }
    
    public DoubleVec2 Normalize()
    {
        if (this == Zero)
            throw new DivideByZeroException("Cannot normalize zero vector");
        return this / Magnitude();
    }
    
    public static DoubleVec2 RotatePoint(DoubleVec2 point, double angle)
        => new(
            point.X * Math.Cos(angle) - point.Y * Math.Sin(angle),
            point.X * Math.Sin(angle) + point.Y * Math.Cos(angle)
        );

    public static DoubleVec2 operator +(DoubleVec2 value) 
        => value;
    public static DoubleVec2 operator -(DoubleVec2 value) 
        => new(-value.X, -value.Y);
    public static DoubleVec2 operator +(DoubleVec2 a, DoubleVec2 b)
        => new(a.X + b.X, a.Y + b.Y);
    public static DoubleVec2 operator -(DoubleVec2 a, DoubleVec2 b)
        => a + -b;
    public static DoubleVec2 operator *(DoubleVec2 a, double b) 
        => new(a.X * b, a.Y * b);
    public static DoubleVec2 operator /(DoubleVec2 a, double b)
        => new(a.X / b, a.Y / b);
    public static Vec2<SDecimal> operator *(DoubleVec2 a, SDecimal b)
        => new(a.X * b, a.Y * b);
    public static bool operator ==(DoubleVec2 left, DoubleVec2 right)
        => left.Equals(right);
    public static bool operator !=(DoubleVec2 left, DoubleVec2 right)
        => !left.Equals(right);
    
    public static implicit operator DoubleVec3(DoubleVec2 vec)
        => new(vec.X, vec.Y, 0);
    
    public static implicit operator Vec2<SDecimal>(DoubleVec2 vec)
        => new(vec.X, vec.Y);
    
    public static implicit operator Vec3<SDecimal>(DoubleVec2 vec)
        => new(vec.X, vec.Y, 0);
    
    public override string ToString()
        => "<" + X + ", " + Y + ">";

    public string ToString(string? format, IFormatProvider? formatProvider = null) 
        => "<" + X.ToString(format, formatProvider) + ", " + Y.ToString(format, formatProvider) + ">";

    public bool Equals(DoubleVec2 other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y);
    }
    
    public override bool Equals(object? obj)
    {
        return obj is DoubleVec2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}