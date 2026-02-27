using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Delegate for calculating instant acceleration given other spatial info.
/// </summary>
public delegate SD_Vector2 CalculateAccelerationMethod();

/// <summary>
/// A KinematicObject with physics information.
/// </summary>
public abstract class Body : KinematicObject
{
    public ScientificDecimal Mass;
    public Color Colour;
    public Body? Parent;
    public readonly KeplerOrbitPath KeplerOrbitPath = new();

    private readonly ObjectInfo _objectInfo;
    
    public IMesh Mesh => _objectInfo.Mesh;
    public CompactCollider Collider => _objectInfo.Collider;
    public Material Material => _objectInfo.Material;

    private readonly IEnumerable<Body> _attractors = OrbitGame.Bodies;
    
    protected Body(
        string identifier, 
        SpatialInfo spatialInfo,
        ObjectInfo objectInfo,
        ScientificDecimal mass,
        Color colour,
        Body? parent)
        : base(identifier, spatialInfo)
    {
        Mass = mass;
        Colour = colour;
        Parent = parent;
        _objectInfo = objectInfo;
        Position = spatialInfo.Position + (parent?.Position ?? SD_Vector2.Zero);
        Velocity = spatialInfo.Velocity + (parent?.Velocity ?? SD_Vector2.Zero);
        Mesh.GenerateBuffers();
    }
    
    /// <summary>
    /// Calculate the gravitational acceleration caused by the attraction of one other body.
    /// </summary>
    private SD_Vector2 CalculateGravitationalAcceleration(Body attractor)
    {
        double angle = SD_Vector2.Direction(Position, attractor.Position);
        SD_Vector2 direction = SD_Vector2.FromPolar(angle);
        ScientificDecimal magnitude = Constants.G * attractor.Mass / (Position - attractor.Position).MagnitudeSquared();
        return direction * magnitude;
    }

    /// <summary>
    /// Calculate the net gravitational acceleration with all bodies in the scene.
    /// </summary>
    private SD_Vector2 CalculateNetGravitationalAcceleration(IEnumerable<Body> attractors)
    {
        SD_Vector2 result = SD_Vector2.Zero;
        return attractors
            .Where(x => x != this)
            .Aggregate(result, (sum, next) => 
                sum + CalculateGravitationalAcceleration(next));
    }
    
    /// <summary>
    /// Calculate the net acceleration from gravity and other sources.
    /// </summary>
    public virtual SD_Vector2 CalculateNetAcceleration()
    {
        return CalculateNetGravitationalAcceleration(_attractors);
    }
    
    /// <summary>
    /// Calculate the Keplerian orbit around a central force with the option to calculate certain orbital initials
    /// </summary>
    protected KeplerOrbit? CalculateKeplerianOrbit(Body? centralForce, ScientificDecimal? time)
    {
        if (centralForce == null) return null;
        
        SD_Vector2 relVelocity = Velocity - centralForce.Velocity;
        SD_Vector2 relPosition = Position - centralForce.Position;
        
        SD_Vector2 momentum = relVelocity * Mass;
        SD_Vector3 angularMomentum = SD_Vector2.Cross(relPosition, momentum);
        SD_Vector2 directionVector = relPosition.Normalize();
        ScientificDecimal forceStrength = Mass * centralForce.Mass * Constants.G;

        SD_Vector2 lrlVector = (SD_Vector2)SD_Vector3.Cross(momentum, angularMomentum) -
                            directionVector * Mass * forceStrength;
        if (lrlVector == SD_Vector2.Zero) return null;

        double periapsis = Utils.WrapAngle(lrlVector.Direction());
        ScientificDecimal eccentricity = lrlVector.Magnitude() / (Mass * forceStrength).Abs();
        ScientificDecimal semiLatusRectum = angularMomentum.Magnitude().Square() / Mass / forceStrength;

        if (semiLatusRectum == 0) return null;

        return new KeplerOrbit(this, centralForce, (double)eccentricity, periapsis, semiLatusRectum, time);
    }

    public void GenerateKeplerianOrbit(ScientificDecimal time)
        => KeplerOrbitPath.Orbit = CalculateKeplerianOrbit(Parent, time);

    public void UpdatePosition_Integrator(
        ScientificDecimal timeStep, 
        NumericalIntegrator integrator, 
        CalculateAccelerationMethod calculateAcceleration)
    {
        timeStep /= Options.IntegratorIterationAmount;
        for (int i = 0; i < Options.IntegratorIterationAmount; ++i)
        {
            switch (integrator)
            {
                case NumericalIntegrator.ExplicitEuler:
                    Acceleration = calculateAcceleration();
                    Velocity += Acceleration * timeStep;
                    Position += Velocity * timeStep;
                    Angle += AngularVelocity * (double)timeStep;
                    break;
                case NumericalIntegrator.ImplicitEuler:
                    Acceleration = calculateAcceleration();
                    Position += Velocity * timeStep;
                    Velocity += Acceleration * timeStep;
                    Angle += AngularVelocity * (double)timeStep;
                    break;
                case NumericalIntegrator.VelocityVerlet:
                    SD_Vector2 acceleration1 = calculateAcceleration();
                    Position += Velocity * timeStep + acceleration1 * 0.5f * timeStep * timeStep;
                    SD_Vector2 acceleration2 = calculateAcceleration();
                    Velocity += (acceleration1 + acceleration2) * 0.5f * timeStep;
                    Angle += AngularVelocity * (double)timeStep;
                    break;
                case NumericalIntegrator.RungeKutta4:
                    SD_Vector2 originalPosition = Position;
                    SD_Vector2 originalVelocity = Velocity;

                    Acceleration = calculateAcceleration();
                    SD_Vector2 originalAcceleration = Acceleration;
                    SD_Vector2 velocityK1 = originalAcceleration * timeStep;
                    SD_Vector2 positionK1 = Velocity * timeStep;

                    Position = originalPosition + positionK1 * 0.5f;
                    Velocity = originalVelocity + velocityK1 * 0.5f;

                    Acceleration = calculateAcceleration();
                    SD_Vector2 velocityK2 = Acceleration * timeStep;
                    SD_Vector2 positionK2 = Velocity * timeStep;

                    Position = originalPosition + positionK2 * 0.5f;
                    Velocity = originalVelocity + velocityK2 * 0.5f;

                    Acceleration = calculateAcceleration();
                    SD_Vector2 velocityK3 = Acceleration * timeStep;
                    SD_Vector2 positionK3 = Velocity * timeStep;

                    Position = originalPosition + positionK3;
                    Velocity = originalVelocity + velocityK3;

                    Acceleration = calculateAcceleration();
                    SD_Vector2 velocityK4 = Acceleration * timeStep;
                    SD_Vector2 positionK4 = Velocity * timeStep;

                    Velocity = originalVelocity +
                               (velocityK1 + velocityK2 * 2 + velocityK3 * 2 + velocityK4) * (1f / 6f);
                    Position = originalPosition +
                               (positionK1 + positionK2 * 2 + positionK3 * 2 + positionK4) * (1f / 6f);

                    Angle += AngularVelocity * (double)timeStep;
                    break;
            }
        }
    }

    public void UpdatePosition_Kepler(ScientificDecimal totalTime, ScientificDecimal timeDiff)
    {
        if (KeplerOrbitPath.Orbit == null) return;
        SpatialInfo newState = KeplerOrbitPath.Orbit.Value.GetStateAtTime(totalTime);
        newState.Velocity = (newState.Position - Position) / timeDiff;
        SpatialInfo.Position = newState.Position;
        SpatialInfo.Velocity = newState.Velocity;
    }
}