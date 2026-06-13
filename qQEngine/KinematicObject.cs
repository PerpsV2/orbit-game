namespace qQEngine;

/// <summary>
/// Represents a unique object with only spatial information.
/// Contains methods for conversions between world and object spaces.
/// </summary>
public abstract class KinematicObject(string identifier, SpatialInfo spatialInfo)
{
    public readonly string Identifier = identifier;

    public SpatialInfo SpatialInfo = spatialInfo;

    // Access properties of SpatialInfo
    public Vec2Double Position
    {
        get => SpatialInfo.Position;
        set => SpatialInfo.Position = value;
    }
    public Vec2Double Velocity
    {
        get => SpatialInfo.Velocity;
        set => SpatialInfo.Velocity = value;
    }
    public Vec2Double Acceleration
    {
        get => SpatialInfo.Acceleration;
        set => SpatialInfo.Acceleration = value;
    }
    public double Angle
    {
        get => SpatialInfo.Angle;
        set => SpatialInfo.Angle = value;
    }
    public double AngularVelocity
    {
        get => SpatialInfo.AngularVelocity;
        set => SpatialInfo.AngularVelocity = value;
    }

    public double AngularAcceleration
    {
        get => SpatialInfo.AngularAcceleration;
        set => SpatialInfo.AngularAcceleration = value;
    }

    public Vec2 ForwardVector => Vec2.FromPolar(SpatialInfo.Angle);
    public Vec2 RightVector => Vec2.FromPolar(SpatialInfo.Angle - Math.PI / 2);

    /// <summary>
    /// Convert a SD_Vector2 from object space to world space.
    /// </summary>
    public Vec2 ObjectToWorldSpace(Vec2 point)
    {
        return Matrix3X3.Translation(SpatialInfo.Position) * Matrix3X3.Rotation(SpatialInfo.Angle) * point;
    }

    /// <summary>
    /// Convert a SD_Vector2 from world space to object space.
    /// </summary>
    public Vec2 WorldToObjectSpace(Vec2 point)
    {
        return Matrix3X3.Rotation(-SpatialInfo.Angle) * Matrix3X3.Translation(-SpatialInfo.Position) * point;
    }

    /// <summary>
    /// Convert a SD_Vector2 from one object space to another.
    /// </summary>
    /// <param name="point">Point to convert</param>
    /// <param name="newOriginObject">Kinematic object space to convert into</param>
    public Vec2 ObjectToObjectSpace(Vec2 point, KinematicObject newOriginObject)
    {
        return newOriginObject.WorldToObjectSpace(ObjectToWorldSpace(point));
    }
}