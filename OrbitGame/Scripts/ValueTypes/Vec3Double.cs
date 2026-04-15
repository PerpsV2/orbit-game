using System;

namespace OrbitGame;

public readonly struct Vec3Double(double x, double y, double z) : IEquatable<Vec3Double>
{
    public static Vec3Double Zero = new(0, 0, 0);
    
    public double X { get; } = x;
    public double Y { get; } = y;
    public double Z { get; } = z;
    
    public static double Dot(Vec3Double left, Vec3Double right)
        => left.X * right.X + left.Y * right.Y + left.Z * right.Z;
    
    public static Vec3Double Cross(Vec3Double left, Vec3Double right)
        => new(
            left.Y * right.Z - left.Z * right.Y, 
            left.Z * right.X - left.X * right.Z, 
            left.X * right.Y - left.Y * right.X
        );
    
    public double MagnitudeSquared()
        => X * X + Y * Y + Z * Z;
    
    public double Magnitude()
        => Math.Sqrt(MagnitudeSquared());

    public static Vec3Double Direction(Vec3Double start, Vec3Double end)
    {
        Vec3Double difference = end - start;
        return difference.Normalize();
    }
    
    public Vec3Double Normalize()
        => new Vec3Double(X, Y, Z) / Magnitude();
    
    public static Vec3Double operator +(Vec3Double value) 
        => value;
    public static Vec3Double operator -(Vec3Double value) 
        => new(-value.X, -value.Y, -value.Z);
    public static Vec3Double operator +(Vec3Double left, Vec3Double right)
        => new(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
    public static Vec3Double operator -(Vec3Double left, Vec3Double right)
        => left + -right;
    public static Vec3Double operator *(Vec3Double vector, double scalar) 
        => new(vector.X * scalar, vector.Y * scalar, vector.Z * scalar);
    public static Vec3Double operator /(Vec3Double vector, double scalar)
        => new(vector.X / scalar, vector.Y / scalar, vector.Z / scalar);
    public static bool operator ==(Vec3Double left, Vec3Double right)
        => left.Equals(right);
    public static bool operator !=(Vec3Double left, Vec3Double right)
        => !left.Equals(right);
    
    public static explicit operator Vec2Double(Vec3Double value)
        => new (value.X, value.Y);
    
    public static explicit operator Vec2<SDecimal>(Vec3Double vec)
        => new(vec.X, vec.Y);
    
    public override string ToString()
        => "<" + X + ", " + Y + ", " + Z + ">";

    public string ToString(string? format, IFormatProvider? formatProvider = null) 
        => "<" + 
           X.ToString(format, formatProvider) + ", " + 
           Y.ToString(format, formatProvider) + ", " + 
           Z.ToString(format, formatProvider) + 
           ">";

    public bool Equals(Vec3Double other)
    {
        return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
    }

    public override bool Equals(object? obj)
    {
        return obj is Vec3Double other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }
}