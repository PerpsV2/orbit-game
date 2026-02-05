using System;

namespace OrbitGame;

public struct SpatialInfo(SD_Vector2 position, SD_Vector2 velocity, SD_Vector2 acceleration, double angle, double angularVelocity)
{
    public SD_Vector2 Position { get; set; } = position;
    public SD_Vector2 Velocity { get; set; } = velocity;
    public SD_Vector2 Acceleration { get; set; } = acceleration;
    private double _angle = Utils.UnsignedMod(angle, Math.Tau);

    public double Angle
    {
        get => Utils.UnsignedMod(_angle, Math.Tau);
        set => _angle = value;
    }

    public double AngularVelocity { get; set; } = angularVelocity;
}