using System;

namespace OrbitGame;

public struct SpatialInfo(SD_Vector2 position, SD_Vector2 velocity, SD_Vector2 acceleration, double angle, double angularVelocity) 
    : IEquatable<SpatialInfo>
{
    public SD_Vector2 Position { get; set; } = position;
    public SD_Vector2 Velocity { get; set; } = velocity;
    public SD_Vector2 Acceleration { get; set; } = acceleration;
    private double _angle = Utils.WrapAngle(angle);

    public double Angle
    {
        get => Utils.WrapAngle(_angle);
        set => _angle = value;
    }

    public double AngularVelocity { get; set; } = angularVelocity;

    public SpatialInfo(SD_Vector2 position, SD_Vector2 velocity, double angle = 0, double angularVelocity = 0)
        : this(position, velocity, SD_Vector2.Zero, angle, angularVelocity) { }
    
    public SpatialInfo(SD_Vector2 position, double angle)
        : this(position, SD_Vector2.Zero, angle) { }
    
    public SpatialInfo(SD_Vector2 position)
        : this(position, SD_Vector2.Zero, SD_Vector2.Zero, 0, 0) {}

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
        return $"{{Pos: {Position}, Vel: {Velocity}, Acc: {Acceleration}, Ang: {Angle}, AngVel: {AngularVelocity}}}";
    }

    public bool Equals(SpatialInfo other)
    {
        return _angle.Equals(other._angle) && 
               Position.Equals(other.Position) && 
               Velocity.Equals(other.Velocity) && 
               Acceleration.Equals(other.Acceleration) && 
               AngularVelocity.Equals(other.AngularVelocity);
    }

    public override bool Equals(object? obj)
    {
        return obj is SpatialInfo other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_angle, Position, Velocity, Acceleration, AngularVelocity);
    }
}