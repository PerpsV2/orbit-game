namespace qQEngine;

/// <summary>
/// Struct containing positional information about an object and some of its derivatives.
/// </summary>
public struct SpatialInfo(
    Vec2Double position, Vec2Double velocity, Vec2Double acceleration, 
    double angle, double angularVelocity, double angularAcceleration) 
    : IEquatable<SpatialInfo>
{
    public Vec2Double Position { get; set; } = position;
    public Vec2Double Velocity { get; set; } = velocity;
    public Vec2Double Acceleration { get; set; } = acceleration;
    
    private double _angle = Utils.WrapAngle(angle);
    public double Angle
    {
        get => Utils.WrapAngle(_angle);
        set => _angle = value;
    }
    public double AngularVelocity { get; set; } = angularVelocity;
    public double AngularAcceleration { get; set; } = angularAcceleration;

    public SpatialInfo(Vec2 position, Vec2 velocity, double angle = 0, double angularVelocity = 0)
        : this((Vec2Double)position, (Vec2Double)velocity, Vec2Double.Zero, angle, angularVelocity, 0) { }
    
    public SpatialInfo(Vec2 position, double angle)
        : this(position, Vec2.Zero, angle) { }
    
    public SpatialInfo(Vec2 position)
        : this((Vec2Double)position, Vec2Double.Zero, Vec2Double.Zero, 0, 0, 0) {}

    public static bool operator !=(SpatialInfo left, SpatialInfo right)
    {
        return !(left == right);
    }

    public static bool operator ==(SpatialInfo left, SpatialInfo right)
    {
        return left.Equals(right);
    }

    public override string ToString()
    {
        return $"{{Pos: {Position}, Vel: {Velocity}, Acc: {Acceleration}, " +
               $"Ang: {Angle}, AngVel: {AngularVelocity}, AngAcc: {AngularVelocity}}}";
    }

    public bool Equals(SpatialInfo other)
    {
        return Position.Equals(other.Position) && 
               Velocity.Equals(other.Velocity) && 
               Acceleration.Equals(other.Acceleration) && 
               Angle.Equals(other.Angle) && 
               AngularVelocity.Equals(other.AngularVelocity) &&
               AngularAcceleration.Equals(other.AngularAcceleration);
    }

    public override bool Equals(object? obj)
    {
        return obj is SpatialInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Position, Velocity, Acceleration, Angle, AngularVelocity, AngularAcceleration);
    }
}