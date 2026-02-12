using System;
using System.Collections.Generic;

namespace OrbitGame;

/// <summary>
/// Represents a unique object with only spatial information.
/// Contains methods relating to different coordinate spaces.
/// </summary>
/// <param name="identifier">Unique ID for the KinematicObject</param>
/// <param name="spatialInfo">Position, Velocity, Acceleration, Angle, and AngularVelocity of KinematicObject</param>
public abstract class KinematicObject(
    string identifier,
    SpatialInfo spatialInfo)
{
    public readonly string Identifier = identifier;

    public SpatialInfo SpatialInfo = spatialInfo;
    
    // Access properties of SpatialInfo
    public SD_Vector2 Position
    {
        get => SpatialInfo.Position;
        set => SpatialInfo.Position = value;
    }
    public SD_Vector2 Velocity
    {
        get => SpatialInfo.Velocity;
        set => SpatialInfo.Velocity = value;
    }
    public SD_Vector2 Acceleration
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

    public SD_Vector2 ForwardVector => SD_Vector2.FromPolar(SpatialInfo.Angle);
    public SD_Vector2 RightVector => SD_Vector2.FromPolar(SpatialInfo.Angle - Math.PI / 2);

    #region Coordinate Transforms
    
    /// <summary>
    /// Convert a SD_Vector2 from object space to world space.
    /// </summary>
    public SD_Vector2 ObjectToWorldSpace(SD_Vector2 point)
    {
        return Matrix3X3.Translation(SpatialInfo.Position) * Matrix3X3.Rotation(SpatialInfo.Angle) * point;
    }

    /// <summary>
    /// Convert a SD_Vector2 from world space to object space.
    /// </summary>
    public SD_Vector2 WorldToObjectSpace(SD_Vector2 point)
    {
        return Matrix3X3.Rotation(-SpatialInfo.Angle) * Matrix3X3.Translation(-SpatialInfo.Position) * point;
    }

    /// <summary>
    /// Convert a SD_Vector2 from one object space to another.
    /// </summary>
    /// <param name="point">Point to convert</param>
    /// <param name="newOriginObject">Kinematic object space to convert into</param>
    public SD_Vector2 ObjectToObjectSpace(SD_Vector2 point, KinematicObject newOriginObject)
    {
        return newOriginObject.WorldToObjectSpace(ObjectToWorldSpace(point));
    }
    
    #endregion

    /// <summary>
    /// Update the position after resetting the world origin.
    /// </summary>
    /// <param name="origin"></param>
    public virtual void ResetOrigin(SD_Vector2 origin)
    {
        Position -= origin;
    }
    
    /// <summary>
    /// Factory class for KinematicObject.
    /// </summary>
    public abstract class KinematicObjectTemplate
    {
        private Dictionary<string, KinematicObject> Instances { get; } = new();

        /// <summary>
        /// Add an instance of a KinematicObject into the pool of objects
        /// </summary>
        protected void AddInstance(string identifier, KinematicObject instance)
            => Instances.Add(identifier, instance);

        /// <summary>
        /// Remove an instance from the pool of objects
        /// </summary>
        public bool Destroy(string identifier)
            => Instances.Remove(identifier);
    }
}