using System;
using System.Collections.Generic;

namespace OrbitGame;

/// <summary>
/// Represents a unique object with only spatial information.
/// Contains methods for conversions between world and object spaces.
/// </summary>
public abstract class KinematicObject
{
    public readonly string Identifier;

    public SpatialInfo SpatialInfo;

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

    public Vec2<SDecimal> ForwardVector => Vec2<SDecimal>.FromPolar(SpatialInfo.Angle);
    public Vec2<SDecimal> RightVector => Vec2<SDecimal>.FromPolar(SpatialInfo.Angle - Math.PI / 2);
    
    protected KinematicObject(string identifier, SpatialInfo spatialInfo)
    {
        Identifier = identifier;
        SpatialInfo = spatialInfo;
        OriginBody.OnResetOrigin += KinematicObject_OnResetOrigin;
    }
    
    /// <summary>
    /// Update the position after resetting the world origin.
    /// </summary>
    private void KinematicObject_OnResetOrigin(object? obj, OriginBodyEventArgs e)
    {
        Position += e.PositionOffset;
    }
    
    /// <summary>
    /// Convert a SD_Vector2 from object space to world space.
    /// </summary>
    public Vec2<SDecimal> ObjectToWorldSpace(Vec2<SDecimal> point)
    {
        return Matrix3X3<SDecimal>.Translation(SpatialInfo.Position) * Matrix3X3<SDecimal>.Rotation(SpatialInfo.Angle) * point;
    }

    /// <summary>
    /// Convert a SD_Vector2 from world space to object space.
    /// </summary>
    public Vec2<SDecimal> WorldToObjectSpace(Vec2<SDecimal> point)
    {
        return Matrix3X3<SDecimal>.Rotation(-SpatialInfo.Angle) * Matrix3X3<SDecimal>.Translation(-SpatialInfo.Position) * point;
    }

    /// <summary>
    /// Convert a SD_Vector2 from one object space to another.
    /// </summary>
    /// <param name="point">Point to convert</param>
    /// <param name="newOriginObject">Kinematic object space to convert into</param>
    public Vec2<SDecimal> ObjectToObjectSpace(Vec2<SDecimal> point, KinematicObject newOriginObject)
    {
        return newOriginObject.WorldToObjectSpace(ObjectToWorldSpace(point));
    }

    public void Destroy()
    {
        return;
    }

    /// <summary>
    /// Factory class for KinematicObject.
    /// </summary>
    public abstract class KinematicObjectTemplate;
}