using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Delegate for calculating instant acceleration given other spatial info.
/// </summary>
public delegate DVector2<SDecimal> CalculateAccelerationMethod();

/// <summary>
/// A KinematicObject with physics information.
/// </summary>
public abstract class Body : KinematicObject
{
    public SDecimal Mass;
    public Color Colour;
    public Body? Parent;
    public readonly KeplerOrbitPath KeplerOrbitPath = new();

    private readonly ObjectInfo _objectInfo;
    
    public IMesh Mesh => _objectInfo.Mesh;
    public CompactCollider Collider => _objectInfo.Collider;
    public Material Material => _objectInfo.Material;
    
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
        _objectInfo = objectInfo;
        Position = spatialInfo.Position + (parent?.Position ?? DVector2<SDecimal>.Zero);
        Velocity = spatialInfo.Velocity + (parent?.Velocity ?? DVector2<SDecimal>.Zero);
        OrbitGame.UpdateFrame += Body_UpdateFrame;
    }
    
    protected virtual void Body_UpdateFrame(object? e, EventArgs args)
    {
        Acceleration = DVector2<SDecimal>.Zero;
        AngularAcceleration = 0;
    }
    
    /// <summary>
    /// Calculate the gravitational acceleration caused by the attraction of one other body.
    /// </summary>
    private DVector2<SDecimal> CalculateGravitationalAcceleration(Body attractor)
    {
        double angle = DVector2<SDecimal>.Direction(Position, attractor.Position);
        DVector2<SDecimal> direction = DVector2<SDecimal>.FromPolar(angle);
        SDecimal magnitude = Constants.G * attractor.Mass / (Position - attractor.Position).MagnitudeSquared();
        return direction * magnitude;
    }

    /// <summary>
    /// Calculate the net gravitational acceleration with all bodies in the scene.
    /// </summary>
    private DVector2<SDecimal> CalculateNetGravitationalAcceleration(IEnumerable<Body> attractors)
    {
        DVector2<SDecimal> result = DVector2<SDecimal>.Zero;
        return attractors
            .Where(x => x != this)
            .Aggregate(result, (sum, next) => 
                sum + CalculateGravitationalAcceleration(next));
    }
    
    /// <summary>
    /// Calculate the net acceleration from gravity and other sources.
    /// </summary>
    public virtual DVector2<SDecimal> CalculateNetAcceleration()
    {
        return CalculateNetGravitationalAcceleration(KinematicObjectTemplate.AllInstances.Values.OfType<Body>());
    }
    
    /// <summary>
    /// Calculate the Keplerian orbit around a central force with the option to calculate certain orbital initials
    /// </summary>
    protected KeplerOrbit? CalculateKeplerianOrbit(Body? centralForce, SDecimal time)
    {
        if (centralForce == null) return null;
        return new KeplerOrbit(this, centralForce, time);
    }

    public void GenerateKeplerianOrbit(SDecimal time)
        => KeplerOrbitPath.Orbit = CalculateKeplerianOrbit(Parent, time);

    public void UpdatePosition_Integrator(
        SDecimal timeStep, 
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
                    AngularVelocity += AngularAcceleration * (double)timeStep;
                    Angle += AngularVelocity * (double)timeStep;
                    break;
                case NumericalIntegrator.ImplicitEuler:
                    Acceleration = calculateAcceleration();
                    Position += Velocity * timeStep;
                    Velocity += Acceleration * timeStep;
                    AngularVelocity += AngularAcceleration * (double)timeStep;
                    Angle += AngularVelocity * (double)timeStep;
                    break;
                case NumericalIntegrator.VelocityVerlet:
                    DVector2<SDecimal> acceleration1 = calculateAcceleration();
                    Position += Velocity * timeStep + acceleration1 * 0.5f * timeStep * timeStep;
                    DVector2<SDecimal> acceleration2 = calculateAcceleration();
                    Velocity += (acceleration1 + acceleration2) * 0.5f * timeStep;
                    AngularVelocity += AngularAcceleration * (double)timeStep;
                    Angle += AngularVelocity * (double)timeStep;
                    break;
                case NumericalIntegrator.RungeKutta4:
                    DVector2<SDecimal> originalPosition = Position;
                    DVector2<SDecimal> originalVelocity = Velocity;

                    Acceleration = calculateAcceleration();
                    DVector2<SDecimal> originalAcceleration = Acceleration;
                    DVector2<SDecimal> velocityK1 = originalAcceleration * timeStep;
                    DVector2<SDecimal> positionK1 = Velocity * timeStep;

                    Position = originalPosition + positionK1 * 0.5f;
                    Velocity = originalVelocity + velocityK1 * 0.5f;

                    Acceleration = calculateAcceleration();
                    DVector2<SDecimal> velocityK2 = Acceleration * timeStep;
                    DVector2<SDecimal> positionK2 = Velocity * timeStep;

                    Position = originalPosition + positionK2 * 0.5f;
                    Velocity = originalVelocity + velocityK2 * 0.5f;

                    Acceleration = calculateAcceleration();
                    DVector2<SDecimal> velocityK3 = Acceleration * timeStep;
                    DVector2<SDecimal> positionK3 = Velocity * timeStep;

                    Position = originalPosition + positionK3;
                    Velocity = originalVelocity + velocityK3;

                    Acceleration = calculateAcceleration();
                    DVector2<SDecimal> velocityK4 = Acceleration * timeStep;
                    DVector2<SDecimal> positionK4 = Velocity * timeStep;

                    Velocity = originalVelocity +
                               (velocityK1 + velocityK2 * 2 + velocityK3 * 2 + velocityK4) * (1f / 6f);
                    Position = originalPosition +
                               (positionK1 + positionK2 * 2 + positionK3 * 2 + positionK4) * (1f / 6f);

                    AngularVelocity += AngularAcceleration * (double)timeStep;
                    Angle += AngularVelocity * (double)timeStep;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(integrator));
            }
        }
    }

    public void UpdatePosition_Kepler(SDecimal totalTime, SDecimal timeDiff)
    {
        if (KeplerOrbitPath.Orbit == null) return;
        SpatialInfo newState = KeplerOrbitPath.Orbit.Value.GetStateAtTime(totalTime);
        SpatialInfo.Position = newState.Position;
        SpatialInfo.Velocity = newState.Velocity;
        SpatialInfo.AngularVelocity += (double)(AngularAcceleration * timeDiff); 
        SpatialInfo.Angle += (double)(AngularVelocity * timeDiff);
    }
}