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
    public Vec2<SDecimal> Position
    {
        get => SpatialInfo.Position;
        set => SpatialInfo.Position = value;
    }
    public Vec2<SDecimal> Velocity
    {
        get => SpatialInfo.Velocity;
        set => SpatialInfo.Velocity = value;
    }
    public Vec2<SDecimal> Acceleration
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
    private readonly Action<string>? _destructor;
    
    protected KinematicObject(string identifier, SpatialInfo spatialInfo, Action<string>? destructor = null)
    {
        _destructor = destructor;
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

    /// <summary>
    /// Removes the object from its respective template instance.
    /// </summary>
    public void Destroy()
    {
        if (_destructor is null) return;
        _destructor.Invoke(Identifier);
    }
    
    /// <summary>
    /// Factory class for KinematicObject.
    /// </summary>
    public abstract class KinematicObjectTemplate
    {
        public static Dictionary<string, KinematicObject> AllInstances { get; } = new();
        
        protected Dictionary<string, KinematicObject> Instances { get; } = new();

        /// <summary>
        /// Add an instance of a KinematicObject into the pool of objects
        /// </summary>
        protected virtual void AddInstance(string identifier, KinematicObject instance)
        {
            if (!Instances.TryAdd(identifier, instance) || !AllInstances.TryAdd(identifier, instance))
                throw new ArgumentException($"KinematicObject with identifier '{identifier}' has already been added");
        }

        /// <summary>
        /// Remove an instance from the pool of objects
        /// </summary>
        public virtual void DestroyInstance(string identifier)
        {
            AllInstances.Remove(identifier);
            Instances.Remove(identifier);
        }

        public static void DestroyAll()
        {
            foreach (var instance in AllInstances)
                instance.Value.Destroy();
        }
    }
}