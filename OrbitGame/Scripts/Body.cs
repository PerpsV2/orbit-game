using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Delegate for calculating instant acceleration given other spatial info.
/// </summary>
public delegate Vec2Double CalculateAccelerationMethod();

/// <summary>
/// A KinematicObject with physics information.
/// </summary>
public abstract class Body : KinematicObject
{
    public SDecimal Mass;
    public Color Colour;
    public Body? Parent;
    public readonly PatchedConicPath OrbitPath;

    protected ObjectInfo ObjectInfo;
    
    public IMesh Mesh => ObjectInfo.Mesh;
    public CompactCollider Collider => ObjectInfo.Collider;
    public Material Material => ObjectInfo.Material;
    
    protected Body(
        string identifier, 
        SpatialInfo spatialInfo,
        ObjectInfo objectInfo,
        SDecimal mass,
        Color colour,
        Body? parent)
        : base(identifier, spatialInfo)
    {
        Mass = mass;
        Colour = colour;
        Parent = parent;
        ObjectInfo = objectInfo;
        Position = spatialInfo.Position + (parent?.Position ?? Vec2Double.Zero);
        Velocity = spatialInfo.Velocity + (parent?.Velocity ?? Vec2Double.Zero);
        OrbitPath = new PatchedConicPath(
            objectInfo.OrbitMesh ?? throw new NullReferenceException("Body was constructed without an OrbitMesh"),
            colour
        );
        OrbitGame.UpdateFrame += Body_UpdateFrame;
    }
    
    protected virtual void Body_UpdateFrame(object? e, EventArgs args)
    {
        Acceleration = Vec2Double.Zero;
        AngularAcceleration = 0;
    }
    
    /// <summary>
    /// Calculate the gravitational acceleration caused by the attraction of one other body.
    /// </summary>
    private Vec2Double CalculateGravitationalAcceleration(Body attractor)
    {
        Vec2Double direction = (attractor.Position - Position).Normalize();
        double magnitude = (double)(Constants.G * attractor.Mass / (Position - attractor.Position).MagnitudeSquared());
        return direction * magnitude;
        //Vec2<SDecimal> differenceVector = Position - attractor.Position;
        //return differenceVector * Constants.G * attractor.Mass / SDecimal.IntPow(differenceVector.Magnitude(), 3);
    }

    /// <summary>
    /// Calculate the net gravitational acceleration with all bodies in the scene.
    /// </summary>
    private Vec2Double CalculateNetGravitationalAcceleration(IEnumerable<Body> attractors)
    {
        Vec2Double result = Vec2Double.Zero;
        foreach (var attractor in attractors)
        {
            if (attractor == this) continue;
            result += CalculateGravitationalAcceleration(attractor);
        }

        return result;
    }
    
    /// <summary>
    /// Calculate the net acceleration from gravity and other sources.
    /// </summary>
    public virtual Vec2Double CalculateNetAcceleration()
    {
        return CalculateNetGravitationalAcceleration(OrbitGame.Hierarchy.GetObjectsOfType<Planet>());
    }
    
    /// <summary>
    /// Calculate the Keplerian orbit around a central force with the option to calculate certain orbital initials
    /// </summary>
    public virtual void GenerateOrbitPath(SDecimal time)
    {
        if (Parent == null) return;
        OrbitPath.Conics[0].Orbit = new KeplerOrbit(this, Parent, time);
    }

    public void UpdatePosition_Integrator(
        double timeStep, 
        NumericalIntegrator integrator, 
        CalculateAccelerationMethod calculateAcceleration,
        uint integrationIterationAmount = Options.IntegratorIterationAmount)
    {
        timeStep /= integrationIterationAmount;
        for (int i = 0; i < integrationIterationAmount; ++i)
        {
            switch (integrator)
            {
                case NumericalIntegrator.ExplicitEuler:
                    Acceleration = calculateAcceleration();
                    Velocity += Acceleration * timeStep;
                    Position += Velocity * timeStep;
                    AngularVelocity += AngularAcceleration * timeStep;
                    Angle += AngularVelocity * timeStep;
                    break;
                case NumericalIntegrator.ImplicitEuler:
                    Acceleration = calculateAcceleration();
                    Position += Velocity * timeStep;
                    Velocity += Acceleration * timeStep;
                    AngularVelocity += AngularAcceleration * timeStep;
                    Angle += AngularVelocity * timeStep;
                    break;
                case NumericalIntegrator.VelocityVerlet:
                    Vec2Double acceleration1 = calculateAcceleration();
                    Position += Velocity * timeStep + acceleration1 * 0.5f * timeStep * timeStep;
                    Vec2Double acceleration2 = calculateAcceleration();
                    Velocity += (acceleration1 + acceleration2) * 0.5f * timeStep;
                    AngularVelocity += AngularAcceleration * timeStep;
                    Angle += AngularVelocity * timeStep;
                    break;
                case NumericalIntegrator.RungeKutta4:
                    Vec2Double originalPosition = Position;
                    Vec2Double originalVelocity = Velocity;

                    Acceleration = calculateAcceleration();
                    Vec2Double originalAcceleration = Acceleration;
                    Vec2Double velocityK1 = originalAcceleration * timeStep;
                    Vec2Double positionK1 = Velocity * timeStep;

                    Position = originalPosition + positionK1 * 0.5f;
                    Velocity = originalVelocity + velocityK1 * 0.5f;

                    Acceleration = calculateAcceleration();
                    Vec2Double velocityK2 = Acceleration * timeStep;
                    Vec2Double positionK2 = Velocity * timeStep;

                    Position = originalPosition + positionK2 * 0.5f;
                    Velocity = originalVelocity + velocityK2 * 0.5f;

                    Acceleration = calculateAcceleration();
                    Vec2Double velocityK3 = Acceleration * timeStep;
                    Vec2Double positionK3 = Velocity * timeStep;

                    Position = originalPosition + positionK3;
                    Velocity = originalVelocity + velocityK3;

                    Acceleration = calculateAcceleration();
                    Vec2Double velocityK4 = Acceleration * timeStep;
                    Vec2Double positionK4 = Velocity * timeStep;

                    Velocity = originalVelocity +
                               (velocityK1 + velocityK2 * 2 + velocityK3 * 2 + velocityK4) * (1f / 6f);
                    Position = originalPosition +
                               (positionK1 + positionK2 * 2 + positionK3 * 2 + positionK4) * (1f / 6f);

                    AngularVelocity += AngularAcceleration * timeStep;
                    Angle += AngularVelocity * timeStep;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(integrator));
            }
        }
    }

    public void UpdatePosition_Kepler(SDecimal totalTime, SDecimal timeDiff)
    {
        if (OrbitPath.IsEmpty()) return;
        SpatialInfo newState = OrbitPath.GetSpatialInfoAtTime(totalTime);
        SpatialInfo.Position = newState.Position;
        SpatialInfo.Velocity = newState.Velocity;
        SpatialInfo.AngularVelocity += (double)(AngularAcceleration * timeDiff); 
        SpatialInfo.Angle += (double)(AngularVelocity * timeDiff);
    }

    public abstract class BodyTemplate : KinematicObjectTemplate;
}